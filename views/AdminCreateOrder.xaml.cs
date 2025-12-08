using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
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
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;

namespace Pet_Shop_Project.Views
{
    public class ItemInOrder : INotifyPropertyChanged
    {
        private Product _product;
        private int _quantity;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice => Product != null ? Quantity * Product.UnitPrice * (decimal)(1 - Product.Discount) : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class AdminCreateOrder : Page, INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private readonly ProductService _productService;
        private readonly string _connectionDB;
        private ObservableCollection<User> _allUsers;
        private ObservableCollection<User> _filteredUsers;
        private ObservableCollection<Product> _allProducts;
        private ObservableCollection<Product> _filteredProducts;
        private ObservableCollection<ItemInOrder> _itemsInOrder;
        private decimal _totalAmount;
        private User _selectedUser;

        public AdminCreateOrder()
        {
            InitializeComponent();

            _userService = new UserService();
            _productService = new ProductService();
            _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
            _filteredUsers = new ObservableCollection<User>();
            _filteredProducts = new ObservableCollection<Product>();
            _itemsInOrder = new ObservableCollection<ItemInOrder>();
            _totalAmount = 0;

            CustomerListControl.ItemsSource = _filteredUsers;
            ProductListControl.ItemsSource = _filteredProducts;
            OrderItemsControl.ItemsSource = _itemsInOrder;

            _ = LoadDataFromDB();
        }

        private async Task LoadDataFromDB()
        {
            AllUsers = await _userService.GetAllUsers() ?? new ObservableCollection<User>();
            FilteredUsers = new ObservableCollection<User>(AllUsers);
            CustomerListControl.ItemsSource = FilteredUsers;

            var products = _productService.GetAllProducts();
            AllProducts = products != null
                ? new ObservableCollection<Product>(products)
                : new ObservableCollection<Product>();
            ApplyProductFiltersAndSort();
        }

        public ObservableCollection<User> AllUsers
        {
            get => _allUsers;
            set
            {
                _allUsers = value;
                OnPropertyChanged(nameof(AllUsers));
            }
        }

        public ObservableCollection<User> FilteredUsers
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                OnPropertyChanged(nameof(FilteredUsers));
            }
        }

        public ObservableCollection<Product> AllProducts
        {
            get => _allProducts;
            set
            {
                _allProducts = value;
                OnPropertyChanged(nameof(AllProducts));
            }
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged(nameof(FilteredProducts));
            }
        }

        public ObservableCollection<ItemInOrder> ItemsInOrder
        {
            get => _itemsInOrder;
            set
            {
                _itemsInOrder = value;
                OnPropertyChanged(nameof(ItemsInOrder));
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged(nameof(TotalAmount));
                }
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCustomerSearchResults();
        }

        private void SelectCustomer_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is User user)
            {
                SelectedUser = user;
                SelectedCustomerPanel.Visibility = Visibility.Visible;
                CustomerSearchResults.Visibility = Visibility.Collapsed;
                UpdateCustomerUI(user);
                UseDefaultAddress.IsChecked = true;
                UpdateShippingAddress();
            }
        }

        private void ClearSelectedCustomer_Click(object sender, RoutedEventArgs e)
        {
            SelectedUser = null;
            SelectedCustomerPanel.Visibility = Visibility.Collapsed;
            CustomerSearchBox.Text = string.Empty;
            CustomerSearchResults.Visibility = Visibility.Collapsed;
            UseDefaultAddress.IsChecked = false;
            ShippingAddressBox.Text = string.Empty;
            ShippingAddressBox.IsEnabled = true;
        }

        private void UseDefaultAddress_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateShippingAddress();
        }

        private void ProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyProductFiltersAndSort();
        }

        private void PriceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyProductFiltersAndSort();
            }
        }

        private void PriceSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyProductFiltersAndSort();
            }
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            var product = btn.Tag as Product;

            var existing = ItemsInOrder.FirstOrDefault(i => i.Product?.ProductId == product.ProductId);
            var newQuantity = (existing?.Quantity ?? 0) + 1;

            if (newQuantity > product.UnitInStock)
            {
                MessageBox.Show("Không đủ hàng trong kho", "Không Đủ Hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (existing != null)
            {
                existing.Quantity = newQuantity;
            }
            else
            {
                ItemsInOrder.Add(new ItemInOrder
                {
                    Product = product,
                    Quantity = 1
                });
            }

            RefreshOrderSummary();
        }

        private void RemoveFromOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemInOrder item)
            {
                ItemsInOrder.Remove(item);
                RefreshOrderSummary();
            }
        }

        private async void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Chưa chọn người mua", "Thiếu Thông Tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ItemsInOrder.Any())
            {
                MessageBox.Show("Không có sản phẩm nào để lên đơn", "Thiếu Thông Tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var address = UseDefaultAddress.IsChecked == true
                ? SelectedUser?.Address
                : ShippingAddressBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("Không có địa chỉ giao hàng", "Thiếu Thông Tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!await HasSufficientStock()) return;

            var approvalStatus = (ApprovalStatusCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Waiting";
            var paymentStatus = (PaymentStatusCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Pending";
            var shippingStatus = (ShippingStatusCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Pending";

            var order = BuildOrder(address, approvalStatus, paymentStatus, shippingStatus);

            if (await SaveOrderToDatabase(order))
            {
                MessageBox.Show($"Đặt hàng thành công.\nMã đơn hàng: {order.OrderId}", "Thành Công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
            }
        }

        private void UpdateCustomerSearchResults()
        {
            if (AllUsers == null) return;
            var keyword = CustomerSearchBox.Text?.Trim().ToLower();
            FilteredUsers.Clear();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                CustomerSearchResults.Visibility = Visibility.Collapsed;
                return;
            }

            foreach (var user in AllUsers.Where(u =>
                (!string.IsNullOrWhiteSpace(u.FullName) && u.FullName.ToLower().Contains(keyword)) ||
                (!string.IsNullOrWhiteSpace(u.Phone) && u.Phone.ToLower().Contains(keyword)) ||
                (!string.IsNullOrWhiteSpace(u.Email) && u.Email.ToLower().Contains(keyword))))
            {
                FilteredUsers.Add(user);
            }

            CustomerSearchResults.Visibility = FilteredUsers.Any()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdateCustomerUI(User user)
        {
            SelectedCustomerName.Text = user.FullName;
            SelectedCustomerId.Text = $"ID: {user.UserId}";
            SelectedCustomerPhone.Text = user.Phone;
            SelectedCustomerEmail.Text = user.Email;
        }

        private void UpdateShippingAddress()
        {
            if (ShippingAddressBox == null) return;

            if (SelectedUser == null)
            {
                ShippingAddressBox.IsEnabled = true;
                return;
            }

            if (UseDefaultAddress.IsChecked == true)
            {
                ShippingAddressBox.Text = SelectedUser.Address;
                ShippingAddressBox.IsEnabled = false;
            }
            else
            {
                ShippingAddressBox.IsEnabled = true;
            }
        }

        private void ApplyProductFiltersAndSort()
        {
            if (AllProducts == null) return;

            IEnumerable<Product> query = AllProducts;
            var keyword = ProductSearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => (!string.IsNullOrWhiteSpace(p.Name) && p.Name.ToLower().Contains(keyword)));
            }

            switch (PriceFilterCombo.SelectedIndex)
            {
                case 1:
                    query = query.Where(p => p.UnitPrice < 100000);
                    break;
                case 2:
                    query = query.Where(p => p.UnitPrice >= 100000 && p.UnitPrice <= 500000);
                    break;
                case 3:
                    query = query.Where(p => p.UnitPrice > 500000 && p.UnitPrice <= 1000000);
                    break;
                case 4:
                    query = query.Where(p => p.UnitPrice > 1000000);
                    break;
            }

            switch (PriceSortCombo.SelectedIndex)
            {
                case 1:
                    query = query.OrderBy(p => p.UnitPrice);
                    break;
                case 2:
                    query = query.OrderByDescending(p => p.UnitPrice);
                    break;
            }

            FilteredProducts = new ObservableCollection<Product>(query);
            ProductListControl.ItemsSource = FilteredProducts;
        }

        private void RefreshOrderSummary()
        {
            TotalAmount = ItemsInOrder.Sum(i => i.TotalPrice);
            TotalAmountText.Text = $"{TotalAmount:N0} VND";
        }

        private async Task<bool> HasSufficientStock()
        {
            const string sql = "SELECT UnitInStock FROM PRODUCTS WHERE ProductId=@ProductId";

            using (var conn = new SqlConnection(_connectionDB))
            {
                await conn.OpenAsync();
                foreach (var item in ItemsInOrder)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                        var stockObj = await cmd.ExecuteScalarAsync();
                        var stock = stockObj != null && stockObj != DBNull.Value ? Convert.ToInt32(stockObj) : 0;
                        if (item.Quantity > stock)
                        {
                            MessageBox.Show($"Sản phẩm {item.Product.Name} chỉ còn {stock} trong kho.",
                                "Không Đủ Hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private async Task<bool> SaveOrderToDatabase(Order order)
        {
            const string insertOrder = @"INSERT INTO ORDERS (UserId, OrderDate, TotalAmount, ApprovalStatus,
                                        PaymentStatus, ShippingStatus, Address, Note)
                                        OUTPUT INSERTED.OrderId
                                        VALUES (@UserId,@OrderDate,@TotalAmount,@ApprovalStatus,
                                        @PaymentStatus,@ShippingStatus,@Address,@Note)";

            const string insertDetail = @"INSERT INTO ORDER_DETAILS (OrderId, ProductId, Quantity)
                                        VALUES (@OrderId,@ProductId,@Quantity)";
            const string updateStock = @"UPDATE PRODUCTS SET UnitInStock = UnitInStock - @Quantity WHERE ProductId=@ProductId";

            using (var conn = new SqlConnection(_connectionDB))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(insertOrder, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", order.UserId);
                            cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                            cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                            cmd.Parameters.AddWithValue("@ApprovalStatus", order.ApprovalStatus);
                            cmd.Parameters.AddWithValue("@PaymentStatus", order.PaymentStatus);
                            cmd.Parameters.AddWithValue("@ShippingStatus", order.ShippingStatus);
                            cmd.Parameters.AddWithValue("@Address", order.Address);
                            cmd.Parameters.AddWithValue("@Note", (object)order.Note ?? DBNull.Value);

                            var generatedId = await cmd.ExecuteScalarAsync();
                            order.OrderId = generatedId?.ToString();
                        }

                        foreach (var d in order.Details)
                        {
                            using (var cmd = new SqlCommand(insertDetail, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                                cmd.Parameters.AddWithValue("@ProductId", d.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (var cmd = new SqlCommand(updateStock, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", d.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        private Order BuildOrder(string address, string approvalStatus, string paymentStatus, string shippingStatus)
        {
            var details = new ObservableCollection<OrderDetail>(ItemsInOrder.Select(i => new OrderDetail
            {
                ProductId = i.Product.ProductId,
                Product = i.Product,
                Quantity = i.Quantity
            }));

            return new Order
            {
                UserId = SelectedUser.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = TotalAmount,
                ApprovalStatus = approvalStatus,
                PaymentStatus = paymentStatus,
                ShippingStatus = shippingStatus,
                Address = address,
                Note = OrderNoteBox.Text?.Trim(),
                Details = details
            };
        }

        private void ResetForm()
        {
            if (ShippingAddressBox == null) return;

            ItemsInOrder.Clear();
            RefreshOrderSummary();
            OrderNoteBox.Text = string.Empty;
            ShippingAddressBox.Text = string.Empty;
            UseDefaultAddress.IsChecked = false;
            SelectedUser = null;
            SelectedCustomerPanel.Visibility = Visibility.Collapsed;
            CustomerSearchResults.Visibility = Visibility.Collapsed;
            CustomerSearchBox.Text = string.Empty;
            ProductSearchBox.Text = string.Empty;
            PriceFilterCombo.SelectedIndex = 0;
            PriceSortCombo.SelectedIndex = 0;
            ApplyProductFiltersAndSort();
        }
    }

}
