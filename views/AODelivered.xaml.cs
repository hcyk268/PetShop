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
    /// <summary>
    /// Interaction logic for AOShipped.xaml
    /// </summary>
    public partial class AODelivered : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderDelivered;
        private ObservableCollection<Order> _allOrders;
        public AODelivered(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderDelivered = new ObservableCollection<Order>();
            SubscribeOrders();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => { SubscribeOrders(); FilterOrders(); };
            DataContext = this;
        }

        protected void FilterOrders()
        {
            OrderDelivered.Clear();
            foreach (var order in _allOrders)
                if (order.ShippingStatus == "Delivered")
                    OrderDelivered.Add(order);

            OnPropertyChanged(nameof(TotalOrderDelivered));
        }

        public int TotalOrderDelivered => _orderDelivered.Count;
        public ObservableCollection<Order> OrderDelivered
        {
            get => _orderDelivered;
            set
            {
                _orderDelivered = value;
                OnPropertyChanged(nameof(OrderDelivered));
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
