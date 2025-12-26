using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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



    public partial class AORejected : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderRejected;
        private ObservableCollection<Order> _allOrders;
        public AORejected(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderRejected = new ObservableCollection<Order>();
            SubscribeOrders();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => { SubscribeOrders(); FilterOrders(); };
            DataContext = this;
        }
        // Loc danh sach don da bi huy/tu choi
        protected void FilterOrders()
        {
            OrderRejected.Clear();
            foreach (var order in _allOrders)
                if (order.ApprovalStatus == "Rejected")
                    OrderRejected.Add(order);

            OnPropertyChanged(nameof(TotalOrderRejected));
        }
        public int TotalOrderRejected => _orderRejected.Count;

        public ObservableCollection<Order> OrderRejected
        {
            get => _orderRejected;
            set
            {
                _orderRejected = value;
                OnPropertyChanged(nameof(OrderRejected));
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
