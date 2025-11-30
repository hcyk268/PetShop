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
using System.Configuration;
using System.Data.SqlClient;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for Cart.xaml
    /// </summary>
    public partial class Cart : Page
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        private readonly string _userId;
        private ObservableCollection<CartItem> cartItems;
        private bool isAllSelected = false;
        private User currentUser; // Thông tin người dùng hiện tại
        public Cart(string userId)
        {
            InitializeComponent();
            _userId = userId ?? throw new ArgumentNullException(nameof(userId));
            Loaded += Cart_Loaded; // async void event, OK
        }

        // Nếu vẫn cần constructor rỗng (designer), truyền user giả:
        public Cart() : this("USR001") { }

        #region Load Data
        // Load thông tin người dùng hiện tại
        private void Cart_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentUser();   // sync
            LoadCartFromDb();    // dùng bản sync bạn đã viết
        }

        private void LoadCurrentUser()
        {
            const string sql = @"SELECT UserId, FullName, Phone, Address, Email, Role
                                FROM USERS WHERE UserId = @UserId";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentUser = new User
                                {
                                    UserId = reader["UserId"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Role = reader["Role"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin người dùng: " + ex.Message);
            }
        }
        /*
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
        } */

        // Load dữ liệu vào giỏ hàng
        private void LoadCartFromDb()
        {
            try
            {
                CartService.ClearCart();

                const string sql = @"
                    SELECT ci.CartItemId, ci.ProductId, ci.Quantity,
                        p.ProductId AS PId, p.Name, p.Description, p.UnitPrice, p.UnitInStock,
                        p.Discount, p.Picture
                    FROM CART_ITEMS ci
                    JOIN CART c     ON c.CartId = ci.CartId   -- dùng bảng có UserId
                    JOIN PRODUCTS p ON p.ProductId = ci.ProductId
                    WHERE c.UserId = @UserId";                

                using (var conn = new SqlConnection(_conn))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            
                            while (reader.Read())
                            {
                                
                                var product = new Product
                                {
                                    ProductId = reader["PId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                    UnitInStock = (int)reader["UnitInStock"],
                                    Discount = (double)reader["Discount"],
                                    Picture = reader["Picture"].ToString()
                                };
                                var item = new CartItem
                                {
                                    CartItemId = reader["CartItemId"].ToString(),
                                    ProductId = reader["ProductId"].ToString(),
                                    Quantity = (int)reader["Quantity"],
                                    Product = product,
                                    IsSelected = false
                                };
                                CartService.AddToCart(item);
                                item.PropertyChanged += CartItem_PropertyChanged;
                            }
                            
                        }
                    }
                }

                cartItems = CartService.CartItems;
                CartItemsControl.ItemsSource = cartItems;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải giỏ hàng: " + ex.Message);
            }
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
