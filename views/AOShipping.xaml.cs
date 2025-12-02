using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
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

        private void removeorderbtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            Order order = btn.Tag as Order;

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
                    conn.Open();

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

                        cmd.ExecuteNonQuery();
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
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Hủy đơn hàng thất bại: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }
    }
}
