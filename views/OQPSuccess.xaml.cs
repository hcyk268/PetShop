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
    /// Interaction logic for OQPSuccess.xaml
    /// </summary>
    public partial class OQPSuccess : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderSuccesses;
        private ObservableCollection<Order> _allOrders;
        public OQPSuccess(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderSuccesses = new ObservableCollection<Order>();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
            DataContext = this;
        }

        public ObservableCollection<Order> OrderSuccesses
        {
            get => _orderSuccesses;
            set
            {
                _orderSuccesses = value;
                OnPropertyChanged(nameof(OrderSuccesses));
            }
        }

        private int _ToTalSuccess;

        public int ToTalSuccess
        {
            get => _ToTalSuccess;
            set
            {
                _ToTalSuccess = value;
                OnPropertyChanged(nameof(ToTalSuccess));
            }
        }

        protected void FilterOrders()
        {
            OrderSuccesses.Clear();
            foreach (var order in _allOrders)
                if (order.ShippingStatus == "Delivered")
                    OrderSuccesses.Add(order);
        }

        public int TotalSuccess => _orderSuccesses.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

    }
}
