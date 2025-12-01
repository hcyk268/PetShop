using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for AdminOrder.xaml
    /// </summary>
    public partial class AdminOrder : Page, INotifyPropertyChanged
    {
        private OrderService _orderService = new OrderService();

        private ObservableCollection<Order> _allOrders;

        private string _userid;

        SolidColorBrush defaulttext = (SolidColorBrush)(new BrushConverter().ConvertFrom("#222"));
        SolidColorBrush clickedtext = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF6B6B"));
        public AdminOrder()
        {
            InitializeComponent();

            _allOrders = _orderService.GetOrdersByUser();

            setForeColorDefault();
            pendingbtn.Foreground = clickedtext;
        }

        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set
            {
                _allOrders = value;
                OnPropertyChanged(nameof(AllOrders));
            }
        }
        protected void setForeColorDefault()
        {
            pendingbtn.Foreground = shippedbtn.Foreground 
                = shippingbtn.Foreground = rejectedbtn.Foreground = defaulttext;
        }
        private void pendingbtn_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            pendingbtn.Foreground = clickedtext;
        }

        private void shippingbtn_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            shippingbtn.Foreground = clickedtext;
        }

        private void shippedbtn_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            shippedbtn.Foreground = clickedtext;
        }

        private void rejectedbtn_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            rejectedbtn.Foreground = clickedtext;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
