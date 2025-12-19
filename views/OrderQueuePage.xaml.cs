using Pet_Shop_Project.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Pet_Shop_Project.Services;
using Pet_Shop_Project.Controls;
using NavService = Pet_Shop_Project.Services.NavigationService;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for OrderQueuePage.xaml
    /// </summary>
    public partial class OrderQueuePage : Page, INotifyPropertyChanged
    {
        private OrderService _orderService = new OrderService();

        private ObservableCollection<Order> _allOrders;

        private bool _active = false;

        private string _userid;

        SolidColorBrush defaulttext = (SolidColorBrush)(new BrushConverter().ConvertFrom("#222"));
        SolidColorBrush clickedtext = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFAD57"));
        

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavService.Instance.GoBack();
        }

        public OrderQueuePage(string userid)
        {
            InitializeComponent();

            _userid = userid;

            AllOrders = new ObservableCollection<Order>();
            MainScreenOQP.Visibility = Visibility.Collapsed;
            loadingIndicatorOQP.Visibility = Visibility.Visible;

            setForeColorDefault();
            setOpacityButton();
            odppendingbutton.Foreground = clickedtext;

            Loaded += OrderQueuePage_Loaded;
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

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged(nameof(Active));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void setForeColorDefault()
        {
            odppendingbutton.Foreground = odpshippingbutton.Foreground = odpsuccessbutton.Foreground = odpcanceledbutton.Foreground = defaulttext;
        }

        protected void setOpacityButton()
        {
            odppendingbutton.Opacity = odpshippingbutton.Opacity = odpsuccessbutton.Opacity = odpcanceledbutton.Opacity = 0.5;
        }

        private async void OrderQueuePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OrderQueuePage_Loaded;
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                AllOrders = await _orderService.GetOrdersByUser(_userid);
                MainScreenOQP.Navigate(new OQPPendingApproval(AllOrders));
            }
            finally
            {
                loadingIndicatorOQP.Visibility = Visibility.Collapsed;
                MainScreenOQP.Visibility = Visibility.Visible;
            }
        }

        private void odppendingbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odppendingbutton.Foreground = clickedtext;

            MainScreenOQP.Navigate(new OQPPendingApproval(AllOrders));
        }

        private void odpshippingbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odpshippingbutton.Foreground = clickedtext;

            MainScreenOQP.Navigate(new OQPShipping(AllOrders));
        }

        private void odpsuccessbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odpsuccessbutton.Foreground = clickedtext;

            MainScreenOQP.Navigate(new OQPSuccess(AllOrders, _userid));
        }

        private void odpcanceledbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odpcanceledbutton.Foreground = clickedtext;

            MainScreenOQP.Navigate(new OQPCanceled(AllOrders));
        }
    }
}