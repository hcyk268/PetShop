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
    /// Interaction logic for Cart.xaml
    /// </summary>
    public partial class Cart : Page
    {

        private ObservableCollection<CartItem> cartItems;
        private bool isAllSelected = false;
        public Cart()
        {
            InitializeComponent();
        }

        private void InitializeCart()
        {
            // Sử dụng CartService để quản lý giỏ hàng chung
            cartItems = CartService.CartItems;

            // Subscribe to property changes
            foreach (var item in cartItems)
            {
                item.PropertyChanged += CartItem_PropertyChanged;
            }
            CartItemsControl.ItemsSource = cartItems;
            UpdateSummary();
        }

        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected" || e.PropertyName == "Quantity")
            {
                UpdateSummary();
            }
        }

        private void UpdateSummary()
        {
            var selectedItems = cartItems.Where(i => i.IsSelected).ToList();
            int totalItems = selectedItems.Sum(i => i.Quantity);
            decimal totalPrice = selectedItems.Sum(i => i.SubTotal);

            if (TotalItemsText != null)
                TotalItemsText.Text = $"{totalItems} sản phẩm";

            if (TotalPriceText != null)
                TotalPriceText.Text = $"{totalPrice:N0}đ";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back or close window
            if(NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            isAllSelected = !isAllSelected;
            foreach (var item in cartItems)
            {
                item.IsSelected = isAllSelected;
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = cartItems.Where(i => i.IsSelected).ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để xóa", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa {selectedItems.Count} sản phẩm đã chọn?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in selectedItems)
                {
                    cartItems.Remove(item);
                }
                isAllSelected = false;
                UpdateSummary();
            }
        }

        private void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var item = button?.Tag as CartItem;
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
            }
        }

        private void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var item = button?.Tag as CartItem;
            if (item != null)
            {
                item.Quantity++;
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var item = button?.Tag as CartItem;

            if (item != null)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc muốn xóa sản phẩm?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    cartItems.Remove(item);
                    UpdateSummary();
                }
            }
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = cartItems.Where(i => i.IsSelected).ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một sản phẩm để đặt mua",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Tạo Order từ selectedItems
            // Có thể navigate đến trang checkout hoặc xử lý đặt hàng

            MessageBox.Show($"Đặt hàng thành công {selectedItems.Count} sản phẩm!",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
