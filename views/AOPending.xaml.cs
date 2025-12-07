using Pet_Shop_Project.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for AOPending.xaml
    /// </summary>
    public partial class AOPending : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderPendings;
        private ObservableCollection<Order> _allOrders;
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        public AOPending(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderPendings = new ObservableCollection<Order>();
            SubscribeOrders();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => { SubscribeOrders(); FilterOrders(); };
            DataContext = this;
        }

        protected void FilterOrders()
        {
            OrderPendings.Clear();
            foreach (var order in _allOrders)
                if (order.ApprovalStatus == "Waiting")
                    OrderPendings.Add(order);

            OnPropertyChanged(nameof(TotalOrderPending));
        }

        public int TotalOrderPending => _orderPendings.Count;
        public ObservableCollection<Order> OrderPendings
        {
            get => _orderPendings;
            set
            {
                _orderPendings = value;
                OnPropertyChanged(nameof(OrderPendings));
            }
        }

        private void ViewOrderDetail_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var order = btn?.Tag as Order;
            if (order == null) return;

            int countproduct = 0;
            var detailBuilder = new StringBuilder();
            detailBuilder.AppendLine("🛍️ Sản phẩm trong hóa đơn:");
            foreach (var deta in order.Details)
            {
                countproduct += (int)deta.Quantity;
                detailBuilder.AppendLine($"  • {deta.Product?.Name}");
            }

            MessageBox.Show(
                $"📦 Mã đơn hàng: {order.OrderId}\n"
                + $"👤 Mã khách hàng: {order.UserId}\n"
                + $"📅 Ngày đặt hàng: {order.OrderDate:dd/MM/yyyy HH:mm}\n"
                + $"💰 Tổng tiền đơn hàng: {order.TotalAmount} VND \n"
                + $"🏠 Địa chỉ giao hàng: {order.Address}\n"
                + $"📝 Ghi chú thêm: {order.Note}\n"
                + $"🔢 Tổng số sản phẩm: {countproduct}\n\n"
                + detailBuilder.ToString(),
                "Thông tin hóa đơn 🧾"
            );
        }

        private async void ApproveOrder_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var order = btn?.Tag as Order;
            await UpdateOrder(order, "Approved", "Xác nhận đơn hàng", "Đơn hàng đã được duyệt và chuyển đi giao hàng", "Shipped");
        }

        private async void RejectOrder_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var order = btn?.Tag as Order;
            await UpdateOrder(order, "Rejected", "Hủy bỏ đơn hàng", "Đơn hàng đã được hủy bỏ", "Pending");
        }

        private async Task UpdateOrder(Order order, string newStatus, string confirmMessage, string successMessage, string shipStatus)
        {
            if (order == null) return;

            var confirm = MessageBox.Show(confirmMessage, "Xac nhan", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionDB))
                {
                    await conn.OpenAsync();
                    string query = @"
                        UPDATE ORDERS 
                        SET ApprovalStatus = @Status,
                            ShippingStatus = @ShipStatus
                        WHERE OrderId = @OrderId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", newStatus);
                        cmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                        cmd.Parameters.AddWithValue("@ShipStatus", shipStatus);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                order.ApprovalStatus = newStatus;
                order.ShippingStatus = shipStatus;
                FilterOrders();

                MessageBox.Show(successMessage, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Cập nhật không thành công: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubscribeOrders()
        {
            foreach (var o in _allOrders)
                o.PropertyChanged -= Order_PropertyChanged;
            foreach (var o in _allOrders)
                o.PropertyChanged += Order_PropertyChanged;
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
