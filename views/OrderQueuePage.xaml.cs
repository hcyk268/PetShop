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



    public partial class OrderQueuePage : Page, INotifyPropertyChanged
    {
        private OrderService _orderService = new OrderService();

        private ObservableCollection<Order> _allOrders;

        private bool _active = false;

        private string _userid;


        // Cache cac trang con de chuyen tab nhanh, tranh tao lai nhieu lan
        private OQPPendingApproval _pendingPage;
        private OQPShipping _shippingPage;
        private OQPSuccess _successPage;
        private OQPCanceled _canceledPage;


        // Luu loai trang hien tai de giu dung trang thai khi refresh
        private string _currentPageType = "Pending";

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


            MainScreenOQP.Navigated += MainScreenOQP_Navigated;


            Loaded += OrderQueuePage_Loaded;
        }


        // Khi frame dieu huong, cap nhat trang thai nut theo trang hien tai
        private void MainScreenOQP_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

            UpdateButtonStates();
        }


        // Doi mau va opacity cho cac nut theo loai trang dang hien thi
        private void UpdateButtonStates()
        {
            if (MainScreenOQP.Content == null) return;

            setForeColorDefault();
            setOpacityButton();

            var currentPage = MainScreenOQP.Content;

            if (currentPage is OQPPendingApproval)
            {
                odppendingbutton.Foreground = clickedtext;
                _currentPageType = "Pending";
            }
            else if (currentPage is OQPShipping)
            {
                odpshippingbutton.Foreground = clickedtext;
                _currentPageType = "Shipping";
            }
            else if (currentPage is OQPSuccess)
            {
                odpsuccessbutton.Foreground = clickedtext;
                _currentPageType = "Success";
            }
            else if (currentPage is OQPCanceled)
            {
                odpcanceledbutton.Foreground = clickedtext;
                _currentPageType = "Canceled";
            }
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
            odppendingbutton.Opacity = odpshippingbutton.Opacity = odpsuccessbutton.Opacity = odpcanceledbutton.Opacity = 1.0;
        }

        private async void OrderQueuePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OrderQueuePage_Loaded;
            await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            try
            {

                // Tai danh sach don va khoi tao cac trang con
                AllOrders = await _orderService.GetOrdersByUser(_userid);


                await Dispatcher.InvokeAsync(() =>
                {
                    _pendingPage = new OQPPendingApproval(AllOrders);
                    _shippingPage = new OQPShipping(AllOrders);
                    _successPage = new OQPSuccess(AllOrders, _userid);
                    _canceledPage = new OQPCanceled(AllOrders);


                    MainScreenOQP.Navigate(_pendingPage);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i don hàng: {ex.Message}",
                    "L?i",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                loadingIndicatorOQP.Visibility = Visibility.Collapsed;
                MainScreenOQP.Visibility = Visibility.Visible;
            }
        }


        public async Task RefreshOrdersAsync()
        {
            try
            {
                MainScreenOQP.Visibility = Visibility.Collapsed;
                loadingIndicatorOQP.Visibility = Visibility.Visible;


                // Refresh du lieu va dung lai cache trang
                AllOrders = await _orderService.GetOrdersByUser(_userid);


                await Dispatcher.InvokeAsync(() =>
                {
                    _pendingPage = new OQPPendingApproval(AllOrders);
                    _shippingPage = new OQPShipping(AllOrders);
                    _successPage = new OQPSuccess(AllOrders, _userid);
                    _canceledPage = new OQPCanceled(AllOrders);


                    switch (_currentPageType)
                    {
                        case "Pending":
                            MainScreenOQP.Navigate(_pendingPage);
                            break;
                        case "Shipping":
                            MainScreenOQP.Navigate(_shippingPage);
                            break;
                        case "Success":
                            MainScreenOQP.Navigate(_successPage);
                            break;
                        case "Canceled":
                            MainScreenOQP.Navigate(_canceledPage);
                            break;
                    }
                });
            }
            finally
            {
                loadingIndicatorOQP.Visibility = Visibility.Collapsed;
                MainScreenOQP.Visibility = Visibility.Visible;
            }
        }

        private async void odppendingbutton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageType == "Pending") return;

            setForeColorDefault();
            setOpacityButton();
            odppendingbutton.Foreground = clickedtext;


            await Dispatcher.InvokeAsync(() =>
            {
                if (_pendingPage == null)
                    _pendingPage = new OQPPendingApproval(AllOrders);

                MainScreenOQP.Navigate(_pendingPage);
            });
        }

        private async void odpshippingbutton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageType == "Shipping") return;

            setForeColorDefault();
            setOpacityButton();
            odpshippingbutton.Foreground = clickedtext;

            await Dispatcher.InvokeAsync(() =>
            {
                if (_shippingPage == null)
                    _shippingPage = new OQPShipping(AllOrders);

                MainScreenOQP.Navigate(_shippingPage);
            });
        }

        private async void odpsuccessbutton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageType == "Success") return;

            setForeColorDefault();
            setOpacityButton();
            odpsuccessbutton.Foreground = clickedtext;

            await Dispatcher.InvokeAsync(() =>
            {
                if (_successPage == null)
                    _successPage = new OQPSuccess(AllOrders, _userid);

                MainScreenOQP.Navigate(_successPage);
            });
        }

        private async void odpcanceledbutton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageType == "Canceled") return;

            setForeColorDefault();
            setOpacityButton();
            odpcanceledbutton.Foreground = clickedtext;

            await Dispatcher.InvokeAsync(() =>
            {
                if (_canceledPage == null)
                    _canceledPage = new OQPCanceled(AllOrders);

                MainScreenOQP.Navigate(_canceledPage);
            });
        }
    }
}
