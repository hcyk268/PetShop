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
    /// Interaction logic for OQPCanceled.xaml
    /// </summary>
    public partial class OQPCanceled : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderCanceled;
        private ObservableCollection<Order> _allOrders;
        public OQPCanceled(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderCanceled = new ObservableCollection<Order>();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
            DataContext = this;
        }

        public ObservableCollection<Order> OrderCanceled
        {
            get => _orderCanceled;
            set
            {
                _orderCanceled = value;
                OnPropertyChanged(nameof(OrderCanceled));
            }
        }

        protected void FilterOrders()
        {
            OrderCanceled.Clear();
            foreach (var order in _allOrders)
                if (order.ApprovalStatus == "Rejected")
                    OrderCanceled.Add(order);

            OnPropertyChanged(nameof(TotalOrderCancel));
        }

        public int TotalOrderCancel => _orderCanceled.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

        private void ImageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;

            border.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopLeft
            };
        }
    }
}
