using Pet_Shop_Project.Models;
using System;
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



    public partial class OQPPendingApproval : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderPendings;
        private ObservableCollection<Order> _allOrders;
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        public OQPPendingApproval(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderPendings = new ObservableCollection<Order>();
            AttachExistingOrders();
            FilterOrders();
            _allOrders.CollectionChanged += AllOrders_CollectionChanged;
            DataContext = this;
        }
        public ObservableCollection<Order> OrderPendings 
        {
            get => _orderPendings;
            set
            {
                _orderPendings = value;
                OnPropertyChanged(nameof(OrderPendings));
            }
        }

        // Loc danh sach don dang cho duyet
        protected void FilterOrders()
        {
            OrderPendings.Clear();
            foreach (var order in _allOrders)
                if (order.ApprovalStatus == "Waiting")
                    OrderPendings.Add(order);
            
            OnPropertyChanged(nameof(TotalOrderPending));
        }

        public int TotalOrderPending => _orderPendings.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

        // Xac nhan truoc khi chuyen don sang trang thai huy
        private async void buttondeleteorder_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(
                "Bạn có thực sự muốn xóa bỏ không?",
                "Vui Lòng Xác Nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (messageBoxResult == MessageBoxResult.No) return;
            var btn = sender as Button;
            string ordid = btn?.Tag?.ToString();
            var ord = _allOrders.FirstOrDefault(x => x.OrderId == ordid);
            if (ord != null)
            {
                bool check = true;

                using (SqlConnection conn = new SqlConnection(_connectionDB))
                {
                    try
                    {
                        await conn.OpenAsync();

                        string query = @"
                            UPDATE ORDERS
                            SET ApprovalStatus = 'Rejected'
                            WHERE OrderId = @OrderId
                        ";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@OrderId", ordid);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        check = false;
                        Console.WriteLine("Delete error: " + ex.Message);
                    }
                }

                if (check)
                {
                    ord.ApprovalStatus = "Rejected";
                    FilterOrders();
                }
                else
                {
                    MessageBox.Show("Xóa Không Thành Công:((");
                }
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

        private void AttachExistingOrders()
        {
            foreach (var order in _allOrders)
                AttachOrder(order);
        }

        // Theo doi thay doi danh sach de gan/bo event va loc lai
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
