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
        private User currentUser; // Thông tin người dùng hiện tại
        public Cart()
        {
            InitializeComponent();
            LoadCurrentUser();
            InitializeCart();
        }

        #region Load Data

        // Load thông tin người dùng hiện tại
        private void LoadCurrentUser()
        {
            // TODO: Lấy từ Session hoặc Database
            currentUser = new User
            {
                UserId = "U001",
                FullName = "Nguyễn Văn A",
                Phone = "(+84) 0123 456 789",
                Address = "123 Đường Nguyễn Văn Cừ, Phường 4, Quận 5, Thành phố Hồ Chí Minh, Việt Nam",
                Email = "nguyenvana@example.com",
                Role = "Customer"
            };
        }

        private void InitializeCart()
        {
            // Kiểm tra xem CartService có dữ liệu chưa
            if (CartService.CartItems == null || CartService.CartItems.Count == 0)
            {
                LoadSampleData();
            }

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

        // Load dữ liệu mẫu vào giỏ hàng
        private void LoadSampleData()
        {
            // Tạo các sản phẩm mẫu
            var product1 = new Product
            {
                ProductId = "P001",
                Name = "Thức ăn cho chó Royal Canin Medium Adult",
                Description = "5kg - Vị gà",
                UnitPrice = 500000,
                UnitInStock = 50,
                Picture = "https://www.petmart.vn/wp-content/uploads/2021/06/thuc-an-cho-cho-truong-thanh-royal-canin-medium-adult1-768x768.jpg"
            };

            var product2 = new Product
            {
                ProductId = "P002",
                Name = "Đồ chơi cho mèo - Tháp bóng",
                Description = "Size M",
                UnitPrice = 150000,
                UnitInStock = 100,
                Picture = "https://dathangsi.vn/upload/products/2023/07/0459-do-choi-cho-meo.jpg"
            };

            var product3 = new Product
            {
                ProductId = "P003",
                Name = "Vòng cổ cho chó SmartCollar",
                Description = "Size L - Màu đỏ",
                UnitPrice = 250000,
                UnitInStock = 30,
                Picture = "https://sanytuong.vn/wp-content/uploads/2022/08/Petpuls-Smart-Collar.png"
            };

            var product4 = new Product
            {
                ProductId = "P004",
                Name = "Lồng hamster cao cấp",
                Description = "40x30x25cm - Màu hồng",
                UnitPrice = 400000,
                UnitInStock = 20,
                Picture = "https://cocapet.net/wp-content/uploads/2022/11/12.-Meo-3-tang-47-x-30-x-60-cm-750.jpg"
            };

            // Thêm vào giỏ hàng qua CartService
            CartService.AddToCart(product1, 2, "5kg - Vị gà");
            CartService.AddToCart(product2, 3, "Size M");
            CartService.AddToCart(product3, 1, "Size L - Màu đỏ");
            CartService.AddToCart(product4, 1, "40x30x25cm - Màu hồng");
        }

        #endregion

        #region Event Handlers

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

        /*
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back or close window
            if(NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
        */

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
                    item.PropertyChanged -= CartItem_PropertyChanged;
                    CartService.RemoveFromCart(item);
                }
                isAllSelected = false;
                UpdateSummary();
            }
        }

        private void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItem;
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
            }
        }

        private void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItem;
            if (item != null)
            {
                // Kiểm tra tồn kho
                if (item.Product != null && item.Quantity >= item.Product.UnitInStock)
                {
                    MessageBox.Show($"Số lượng tối đa: {item.Product.UnitInStock}",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                item.Quantity++;
            }
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CartService.GetSelectedItems();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một sản phẩm để đặt hàng",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Kiểm tra tồn kho
            foreach (var item in selectedItems)
            {
                if (item.Product != null && item.Quantity > item.Product.UnitInStock)
                {
                    MessageBox.Show(
                        $"Sản phẩm '{item.Name}' chỉ còn {item.Product.UnitInStock} trong kho!",
                        "Không đủ hàng",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            // Chuyển đổi CartItem sang OrderDetail
            var orderDetails = ConvertToOrderDetails(selectedItems);

            // Navigate sang trang Payment
            try
            {
                var paymentPage = new Payment(orderDetails, currentUser);
                NavigationService?.Navigate(paymentPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Helper Methods

        // Chuyển đổi CartItem sang OrderDetail
        private List<OrderDetail> ConvertToOrderDetails(List<CartItem> cartItems)
        {
            var orderDetails = new List<OrderDetail>();

            foreach (var cartItem in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid().ToString(),
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Product = cartItem.Product
                };

                orderDetails.Add(orderDetail);
            }

            return orderDetails;
        }

        #endregion
    }
}
