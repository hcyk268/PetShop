using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for OQPSuccess.xaml
    /// </summary>
    public partial class OQPSuccess : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderSuccesses;
        private ObservableCollection<Order> _allOrders;
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        private string _userId;

        public OQPSuccess(ObservableCollection<Order> allOrders, string userId)
        {
            InitializeComponent();
            _allOrders = allOrders;
            _userId = userId;
            OrderSuccesses = new ObservableCollection<Order>();
            AttachExistingOrders();
            FilterOrders();
            _allOrders.CollectionChanged += AllOrders_CollectionChanged;
            DataContext = this;
        }

        public ObservableCollection<Order> OrderSuccesses
        {
            get => _orderSuccesses;
            set
            {
                _orderSuccesses = value;
                OnPropertyChanged(nameof(OrderSuccesses));
            }
        }

        protected void FilterOrders()
        {
            OrderSuccesses.Clear();
            foreach (var order in _allOrders)
                if (order.ShippingStatus == "Delivered")
                    OrderSuccesses.Add(order);

            OnPropertyChanged(nameof(TotalOrderSuccess));
        }

        public int TotalOrderSuccess => _orderSuccesses.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

        private void ImageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;

            border.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopLeft
            };
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var orderDetail = button?.Tag as OrderDetail;

                if (orderDetail == null)
                {
                    MessageBox.Show("Không thể lấy thông tin sản phẩm!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(_userId))
                {
                    MessageBox.Show("Không xác định được người dùng!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mở ReviewWindow
                var reviewWindow = new ReviewWindow(orderDetail, _userId)
                {
                    Owner = Window.GetWindow(this),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                bool? result = reviewWindow.ShowDialog();

                if (result == true)
                {
                    // Đánh giá thành công
                    // Có thể refresh UI hoặc cập nhật trạng thái nếu cần
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void reorderbtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var oldOrder = btn?.Tag as Order;

            if (oldOrder?.Details == null || oldOrder.Details.Count == 0)
            {
                MessageBox.Show("Đơn hàng không có sẵn để đặt.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Bạn muốn đặt lại đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var newOrder = BuildReorder(oldOrder);
            if (!await CheckStock(newOrder.Details)) return;

            if (await SaveReorderToDatabase(newOrder))
            {
                _allOrders.Add(newOrder);
                MessageBox.Show("Đặt lại đơn hàng thành công!", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private Order BuildReorder(Order oldOrder)
        {
            var details = new ObservableCollection<OrderDetail>();

            foreach (var detail in oldOrder.Details ?? Enumerable.Empty<OrderDetail>())
            {
                if (detail == null) continue;

                details.Add(new OrderDetail
                {
                    ProductId = string.IsNullOrWhiteSpace(detail.ProductId) ? detail.Product?.ProductId : detail.ProductId,
                    Product = detail.Product,
                    Quantity = detail.Quantity
                });
            }

            var order = new Order
            {
                UserId = oldOrder.UserId,
                OrderDate = DateTime.Now,
                ApprovalStatus = "Waiting",
                PaymentStatus = "Pending",
                ShippingStatus = "Pending",
                Address = oldOrder.Address,
                Note = oldOrder.Note,
                Details = details
            };

            order.TotalAmount = details.Sum(d => d?.Product != null ? d.SubTotal : 0m);

            return order;
        }

        private async Task<bool> CheckStock(IEnumerable<OrderDetail> details)
        {
            const string sql = "SELECT UnitInStock FROM PRODUCTS WHERE ProductId=@ProductId";

            using (var conn = new SqlConnection(_connectionDB))
            {
                await conn.OpenAsync();
                foreach (var detail in details)
                {
                    if (detail == null || string.IsNullOrWhiteSpace(detail.ProductId)) continue;

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", detail.ProductId);
                        var stockObj = await cmd.ExecuteScalarAsync();
                        var stock = (stockObj as int?) ?? (stockObj != null ? Convert.ToInt32(stockObj) : 0);
                        if (detail.Quantity > stock)
                        {
                            var productName = detail.Product?.Name ?? detail.ProductId;
                            MessageBox.Show(
                                $"Sản phẩm '{productName}' chỉ còn {stock} trong kho.",
                                "Không Đủ Hàng",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private async Task<bool> SaveReorderToDatabase(Order order)
        {
            const string insertOrder = @"INSERT INTO ORDERS (UserId, OrderDate, TotalAmount, ApprovalStatus,
                                        PaymentStatus, ShippingStatus, Address, Note)
                                        OUTPUT INSERTED.OrderId
                                        VALUES (@UserId,@OrderDate,@TotalAmount,@ApprovalStatus,
                                        @PaymentStatus,@ShippingStatus,@Address,@Note)";

            const string insertDetail = @"INSERT INTO ORDER_DETAILS (OrderId, ProductId, Quantity)
                                        VALUES (@OrderId,@ProductId,@Quantity)";

            const string updateStock = @"UPDATE PRODUCTS SET UnitInStock = UnitInStock - @Quantity WHERE ProductId=@ProductId";

            using (var conn = new SqlConnection(_connectionDB))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(insertOrder, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", order.UserId);
                            cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                            cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                            cmd.Parameters.AddWithValue("@ApprovalStatus", order.ApprovalStatus);
                            cmd.Parameters.AddWithValue("@PaymentStatus", order.PaymentStatus);
                            cmd.Parameters.AddWithValue("@ShippingStatus", order.ShippingStatus);
                            cmd.Parameters.AddWithValue("@Address", (object)order.Address ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Note", (object)order.Note ?? DBNull.Value);

                            var insertedId = await cmd.ExecuteScalarAsync();
                            order.OrderId = insertedId?.ToString();
                        }

                        foreach (var detail in order.Details)
                        {
                            detail.OrderId = order.OrderId;

                            using (var cmd = new SqlCommand(insertDetail, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                                cmd.Parameters.AddWithValue("@ProductId", detail.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (var cmd = new SqlCommand(updateStock, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", detail.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            if (detail.Product != null)
                            {
                                detail.Product.UnitInStock -= detail.Quantity;
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        MessageBox.Show($"Không thể đặt lại đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
        }

        private void AttachExistingOrders()
        {
            foreach (var order in _allOrders)
                AttachOrder(order);
        }

        private void AllOrders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Order ord in e.OldItems)
                    DetachOrder(ord);

            if (e.NewItems != null)
                foreach (Order ord in e.NewItems)
                    AttachOrder(ord);

            FilterOrders();
        }

        private void AttachOrder(Order order)
        {
            if (order == null) return;
            order.PropertyChanged -= Order_PropertyChanged;
            order.PropertyChanged += Order_PropertyChanged;
        }

        private void DetachOrder(Order order)
        {
            if (order == null) return;
            order.PropertyChanged -= Order_PropertyChanged;
        }

        private void Order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Order.ApprovalStatus) ||
                e.PropertyName == nameof(Order.ShippingStatus) ||
                e.PropertyName == nameof(Order.PaymentStatus))
            {
                FilterOrders();
            }
        }
    }
}