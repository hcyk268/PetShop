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

        // Cache các page để tránh tạo mới liên tục
        private OQPPendingApproval _pendingPage;
        private OQPShipping _shippingPage;
        private OQPSuccess _successPage;
        private OQPCanceled _canceledPage;

        // Track trang hiện tại
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
            odppendingbutton.Opacity = 1.0;

            // Subscribe to Navigated event
            MainScreenOQP.Navigated += MainScreenOQP_Navigated;

            // Load data asynchronously
            Loaded += OrderQueuePage_Loaded;
        }

        // Event handler khi MainScreenOQP navigate
        private void MainScreenOQP_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Cập nhật UI button dựa trên page hiện tại
            UpdateButtonStates();
        }

        // Method để cập nhật trạng thái button dựa trên page hiện tại
        private void UpdateButtonStates()
        {
            if (MainScreenOQP.Content == null) return;

            setForeColorDefault();
            setOpacityButton();

            var currentPage = MainScreenOQP.Content;

            if (currentPage is OQPPendingApproval)
            {
                odppendingbutton.Foreground = clickedtext;
                odppendingbutton.Opacity = 1.0;
                _currentPageType = "Pending";
            }
            else if (currentPage is OQPShipping)
            {
                odpshippingbutton.Foreground = clickedtext;
                odpshippingbutton.Opacity = 1.0;
                _currentPageType = "Shipping";
            }
            else if (currentPage is OQPSuccess)
            {
                odpsuccessbutton.Foreground = clickedtext;
                odpsuccessbutton.Opacity = 1.0;
                _currentPageType = "Success";
            }
            else if (currentPage is OQPCanceled)
            {
                odpcanceledbutton.Foreground = clickedtext;
                odpcanceledbutton.Opacity = 1.0;
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
            odppendingbutton.Opacity = odpshippingbutton.Opacity = odpsuccessbutton.Opacity = odpcanceledbutton.Opacity = 0.5;
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
                // Load orders asynchronously
                AllOrders = await _orderService.GetOrdersByUser(_userid);

                // Initialize cached pages with loaded data
                await Dispatcher.InvokeAsync(() =>
                {
                    _pendingPage = new OQPPendingApproval(AllOrders);
                    _shippingPage = new OQPShipping(AllOrders);
                    _successPage = new OQPSuccess(AllOrders, _userid);
                    _canceledPage = new OQPCanceled(AllOrders);

                    // Navigate to default page (Pending)
                    MainScreenOQP.Navigate(_pendingPage);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải đơn hàng: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                loadingIndicatorOQP.Visibility = Visibility.Collapsed;
                MainScreenOQP.Visibility = Visibility.Visible;
            }
        }

        // Method để refresh data khi cần
        public async Task RefreshOrdersAsync()
        {
            try
            {
                MainScreenOQP.Visibility = Visibility.Collapsed;
                loadingIndicatorOQP.Visibility = Visibility.Visible;

                // Reload orders
                AllOrders = await _orderService.GetOrdersByUser(_userid);

                // Recreate cached pages with new data
                await Dispatcher.InvokeAsync(() =>
                {
                    _pendingPage = new OQPPendingApproval(AllOrders);
                    _shippingPage = new OQPShipping(AllOrders);
                    _successPage = new OQPSuccess(AllOrders, _userid);
                    _canceledPage = new OQPCanceled(AllOrders);

                    // Navigate to current page type
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
            if (_currentPageType == "Pending") return; // Already on this page

            setForeColorDefault();
            setOpacityButton();
            odppendingbutton.Foreground = clickedtext;
            odppendingbutton.Opacity = 1.0;

            // Use cached page
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
            odpshippingbutton.Opacity = 1.0;

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
            odpsuccessbutton.Opacity = 1.0;

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
            odpcanceledbutton.Opacity = 1.0;

            await Dispatcher.InvokeAsync(() =>
            {
                if (_canceledPage == null)
                    _canceledPage = new OQPCanceled(AllOrders);

                MainScreenOQP.Navigate(_canceledPage);
            });
        }
    }
}