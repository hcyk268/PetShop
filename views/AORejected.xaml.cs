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
    /// Interaction logic for AORejected.xaml
    /// </summary>
    public partial class AORejected : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderRejected;
        private ObservableCollection<Order> _allOrders;
        public AORejected(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderRejected = new ObservableCollection<Order>();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
            DataContext = this;
        }
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
    }
}
