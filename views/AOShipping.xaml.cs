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
    /// Interaction logic for AOShipping.xaml
    /// </summary>
    public partial class AOShipping : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderShipping;
        private ObservableCollection<Order> _allOrders;
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
                _allOrders = value;
                OnPropertyChanged(nameof(OrderShipping));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
