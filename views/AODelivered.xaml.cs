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
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
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
    }
}
