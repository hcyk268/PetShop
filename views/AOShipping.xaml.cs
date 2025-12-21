using Pet_Shop_Project.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for AOShipping.xaml
    /// </summary>
    public partial class AOShipping : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderShipping;
        private ObservableCollection<Order> _allOrders;
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        public AOShipping(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderShipping = new ObservableCollection<Order>();
            SubscribeOrders();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => { SubscribeOrders(); FilterOrders(); };
            DataContext = this;
        }
        protected void FilterOrders()
        {
            OrderShipping.Clear();
            foreach (var order in _allOrders)
                if (order.ShippingStatus == "Shipped")
                    OrderShipping.Add(order);

            OnPropertyChanged(nameof(TotalOrderShipping));
        }
        public int TotalOrderShipping => _orderShipping.Count;

        public ObservableCollection<Order> OrderShipping
        {
            get => _orderShipping;
            set
            {
                _orderShipping = value;
                OnPropertyChanged(nameof(OrderShipping));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void removeorderbtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var order = btn?.Tag as Order;
            if (order == null) return;

            var dialog = MessageBox.Show(
                "Có chắc chắn hủy đơn hàng này?",
                "Xác Nhận Hủy",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (dialog != MessageBoxResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(_connectionDB))
                {
                    await conn.OpenAsync();

                    const string query = @"
                        UPDATE ORDERS
                        SET ApprovalStatus = @ApprovalStatus,
                            PaymentStatus = @PaymentStatus,
                            ShippingStatus = @ShippingStatus
                        WHERE OrderId = @OrderId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ApprovalStatus", "Rejected");
                        cmd.Parameters.AddWithValue("@PaymentStatus", "Pending");
                        cmd.Parameters.AddWithValue("@ShippingStatus", "Pending");
                        cmd.Parameters.AddWithValue("@OrderId", order.OrderId);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                order.ApprovalStatus = "Rejected";
                order.PaymentStatus = "Pending";
                order.ShippingStatus = "Pending";
                FilterOrders();

                MessageBox.Show(
                    "Đã hủy đơn hàng thành công.",
                    "Thành Công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"Hủy đơn hàng thất bại: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

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
    }
}
