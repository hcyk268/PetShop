using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NavService = Pet_Shop_Project.Services.NavigationService;

namespace Pet_Shop_Project.Views
{
    public partial class CartPage : Page
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        private readonly string _userId;
        private ObservableCollection<CartItemViewModel> cartItems;
        private bool isAllSelected = false;
        private User currentUser;

        public CartPage(string userid)
        {
            InitializeComponent();
            _userId = userid;
            Loaded += Cart_LoadedAsync;
        }

        #region Load Data
        async void Cart_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await LoadCurrentUserAsync();
            await LoadCartFromDbAsync();
        }

        private async Task LoadCurrentUserAsync()
        {
            const string sql = @"SELECT UserId, FullName, Phone, Address, Email, Role
                                FROM USERS WHERE UserId = @UserId";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _userId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
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

        private async Task LoadCartFromDbAsync()
        {
            try
            {
                CartService.ClearCart();

                const string sql = @"
                    SELECT ci.CartItemId, ci.ProductId, ci.Quantity,
                        p.ProductId AS PId, p.Name, p.Description, p.UnitPrice, p.UnitInStock,
                        p.Discount, p.Picture
                    FROM CART_ITEMS ci
                    JOIN CART c ON c.CartId = ci.CartId
                    JOIN PRODUCTS p ON p.ProductId = ci.ProductId
                    WHERE c.UserId = @UserId";

                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _userId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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

                // Convert to ViewModel
                cartItems = new ObservableCollection<CartItemViewModel>(
                    CartService.CartItems.Select(ci => new CartItemViewModel(ci))
                );

                foreach (var vm in cartItems)
                {
                    vm.PropertyChanged += CartItem_PropertyChanged;
                }

                CartItemsControl.ItemsSource = cartItems;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải giỏ hàng: " + ex.Message);
            }
        }
        #endregion

        #region Database Operations
        // Cập nhật số lượng trong database
        private async Task UpdateQuantityInDatabaseAsync(string cartItemId, int newQuantity)
        {
            try
            {
                const string sql = @"UPDATE CART_ITEMS 
                                    SET Quantity = @Quantity 
                                    WHERE CartItemId = @CartItemId";

                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                        cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật số lượng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xóa sản phẩm khỏi database
        private async Task DeleteCartItemFromDatabaseAsync(string cartItemId)
        {
            try
            {
                const string sql = @"DELETE FROM CART_ITEMS WHERE CartItemId = @CartItemId";

                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa sản phẩm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xóa nhiều sản phẩm cùng lúc
        private async Task DeleteMultipleCartItemsFromDatabaseAsync(IEnumerable<string> cartItemIds)
        {
            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();

                    foreach (var cartItemId in cartItemIds)
                    {
                        const string sql = @"DELETE FROM CART_ITEMS WHERE CartItemId = @CartItemId";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa nhiều sản phẩm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavService.Instance.GoBack();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            isAllSelected = !isAllSelected;
            foreach (var item in cartItems)
            {
                item.IsSelected = isAllSelected;
            }

            // Update button text
            if (SelectAllButton != null)
            {
                var template = SelectAllButton.Template;
                var textBlock = template?.FindName("selectText", SelectAllButton) as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = isAllSelected ? "Bỏ chọn" : "Chọn tất cả";
                }
            }
        }

        private async void DeleteSelected_Click(object sender, RoutedEventArgs e)
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
                // Lấy danh sách CartItemId để xóa từ database
                var cartItemIds = selectedItems.Select(vm => vm.CartItemId).ToList();

                // Xóa từ database
                await DeleteMultipleCartItemsFromDatabaseAsync(cartItemIds);

                // Xóa từ UI và CartService
                foreach (var vm in selectedItems)
                {
                    vm.PropertyChanged -= CartItem_PropertyChanged;
                    CartService.RemoveFromCart(vm.OriginalCartItem);
                    cartItems.Remove(vm);
                }

                isAllSelected = false;
                UpdateSummary();

                MessageBox.Show("Đã xóa sản phẩm thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemViewModel;

            if (item != null && item.Quantity > 1)
            {
                int oldQuantity = item.Quantity;
                item.Quantity--;

                // Cập nhật database
                await UpdateQuantityInDatabaseAsync(item.CartItemId, item.Quantity);
            }
        }

        private async void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemViewModel;

            if (item != null)
            {
                if (item.Product != null && item.Quantity >= item.Product.UnitInStock)
                {
                    MessageBox.Show($"Số lượng tối đa: {item.Product.UnitInStock}",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int oldQuantity = item.Quantity;
                item.Quantity++;

                // Cập nhật database
                await UpdateQuantityInDatabaseAsync(item.CartItemId, item.Quantity);
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

            var orderDetails = ConvertToOrderDetails(selectedItems);

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

        private void CartFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cartItems == null || CartItemsControl == null) return;

            var selectedFilter = (CartFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            switch (selectedFilter)
            {
                case "Đã chọn":
                    CartItemsControl.ItemsSource = new ObservableCollection<CartItemViewModel>(
                        cartItems.Where(i => i.IsSelected));
                    break;

                case "Chưa chọn":
                    CartItemsControl.ItemsSource = new ObservableCollection<CartItemViewModel>(
                        cartItems.Where(i => !i.IsSelected));
                    break;

                default: // "Tất cả"
                    CartItemsControl.ItemsSource = cartItems;
                    break;
            }
        }

        private void ImageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                    RadiusX = border.CornerRadius.TopLeft,
                    RadiusY = border.CornerRadius.TopLeft
                };
            }
        }

        // Click vào background của cart item để toggle selection
        private void CartItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var item = border?.Tag as CartItemViewModel;
            if (item != null)
            {
                item.IsSelected = !item.IsSelected;
            }
        }

        // Prevent event bubbling for checkbox
        private void CheckBox_PreventBubble(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        // Prevent event bubbling for buttons
        private void Button_PreventBubble(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Helper Methods
        private ObservableCollection<OrderDetail> ConvertToOrderDetails(List<CartItem> cartItems)
        {
            var orderDetails = new ObservableCollection<OrderDetail>();

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

    // ViewModel để hiển thị CartItem với các properties bổ sung cho UI
    public class CartItemViewModel : INotifyPropertyChanged
    {
        private CartItem _originalCartItem;

        public CartItemViewModel(CartItem cartItem)
        {
            _originalCartItem = cartItem;
        }

        public CartItem OriginalCartItem => _originalCartItem;

        public string CartItemId => _originalCartItem.CartItemId;
        public string ProductId => _originalCartItem.ProductId;
        public Product Product => _originalCartItem.Product;

        public bool IsSelected
        {
            get => _originalCartItem.IsSelected;
            set
            {
                if (_originalCartItem.IsSelected != value)
                {
                    _originalCartItem.IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public int Quantity
        {
            get => _originalCartItem.Quantity;
            set
            {
                if (_originalCartItem.Quantity != value)
                {
                    _originalCartItem.Quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(SubTotal));
                    OnPropertyChanged(nameof(DisplayPrice));
                }
            }
        }

        public string Name => _originalCartItem.Name;
        public string Image => _originalCartItem.Image;
        public string Variant => _originalCartItem.Variant;
        public decimal SubTotal => _originalCartItem.SubTotal;

        // Giá giảm
        public bool HasDiscount => Product != null && Product.Discount > 0;

        public Visibility DiscountVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;

        public string DiscountText => HasDiscount ? $"-{(Product.Discount * 100):0}%" : "";

        public Visibility VariantVisibility => string.IsNullOrEmpty(Variant) ? Visibility.Collapsed : Visibility.Visible;

        // Giá hiển thị
        public string DisplayPrice
        {
            get
            {
                if (Product == null) return "0đ";
                decimal pricePerUnit = HasDiscount ? Product.FinalPrice : Product.UnitPrice;
                return $"{(pricePerUnit * Quantity):N0}đ";
            }
        }

        public string OriginalPrice
        {
            get
            {
                if (!HasDiscount || Product == null) return "";
                return $"{(Product.UnitPrice * Quantity):N0}đ";
            }
        }

        public Visibility OriginalPriceVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;

        public Brush PriceForeground
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}