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
    /// Interaction logic for AOPending.xaml
    /// </summary>
    public partial class AOPending : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderPendings;
        private ObservableCollection<Order> _allOrders;
        public AOPending(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderPendings = new ObservableCollection<Order>();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
            DataContext = this;
        }

        protected void FilterOrders()
        {
            OrderPendings.Clear();
            foreach (var order in _allOrders)
                if (order.ApprovalStatus == "Waiting")
                    OrderPendings.Add(order);

            OnPropertyChanged(nameof(TotalOrderPending));
        }

        public int TotalOrderPending => _orderPendings.Count;
        public ObservableCollection<Order> OrderPendings
        {
            get => _orderPendings;
            set
            {
                _orderPendings = value;
                OnPropertyChanged(nameof(OrderPendings));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
