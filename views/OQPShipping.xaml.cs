using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Interaction logic for OQPShipping.xaml
    /// </summary>
    public partial class OQPShipping : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _ordersShippings;
        private ObservableCollection<Order> _allOrders;
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        public OQPShipping(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderShippings = new ObservableCollection<Order>();
            AttachExistingOrders();
            FilterOrders();
            _allOrders.CollectionChanged += AllOrders_CollectionChanged;
            DataContext = this;
        }

        protected void FilterOrders()
        {
            OrderShippings.Clear();
            foreach (var order in _allOrders)
                if (order.ShippingStatus == "Shipped")
                    OrderShippings.Add(order);

            OnPropertyChanged(nameof(TotalOrderShipping));
        }

        public ObservableCollection<Order> OrderShippings
        {
            get => _ordersShippings;
            set
            {
                _ordersShippings = value;
                OnPropertyChanged(nameof(OrderShippings));
            }
        }

        public int TotalOrderShipping => _ordersShippings.Count;

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

        private void receivedbtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            Order order = btn.Tag as Order;

            var confirm = MessageBox.Show(
                "Bạn chắc chắn đã nhận được đơn hàng?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(_connectionDB))
                {
                    conn.Open();

                    const string query = @"
                        UPDATE ORDERS
                        SET ShippingStatus = @ShippingStatus,
                            PaymentStatus = @PaymentStatus
                        WHERE OrderId = @OrderId;

                        UPDATE SHIPMENTS
                        SET Status = @ShipmentStatus
                        WHERE OrderId = @OrderId;";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ShippingStatus", "Delivered");
                        cmd.Parameters.AddWithValue("@PaymentStatus", "Paid");
                        cmd.Parameters.AddWithValue("@ShipmentStatus", "Delivered");
                        cmd.Parameters.AddWithValue("@OrderId", order.OrderId);

                        cmd.ExecuteNonQuery();
                    }
                }

                order.ShippingStatus = "Delivered";
                order.PaymentStatus = "Paid";
                FilterOrders();

                MessageBox.Show(
                    "Cảm ơn quý khách đã xác nhận, quý khách có gì không hài lòng vui lòng feedback cho shop để shop cải thiện hơn.",
                    "Thành Công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Cập nhật thất bại: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
