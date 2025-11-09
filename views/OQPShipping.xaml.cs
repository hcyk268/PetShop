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
    /// Interaction logic for OQPShipping.xaml
    /// </summary>
    public partial class OQPShipping : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _ordersShippings;
        private ObservableCollection<Order> _allOrders;
        public OQPShipping(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderShippings = new ObservableCollection<Order>();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
            DataContext = this;
        }

        protected void FilterOrders()
        {
            OrderShippings.Clear();
            foreach (var order in _allOrders)
                if (order.ShippingStatus == "Shipped")
                    OrderShippings.Add(order);
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

    }
}
