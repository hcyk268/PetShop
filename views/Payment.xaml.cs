using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for Payment.xaml
    /// </summary>
    public partial class Payment : Page
    {
        // Biến lưu trữ dữ liệu
        private ObservableCollection<OrderDetail> orderDetails;
        private User currentUser;
        private decimal subtotal = 0;
        private decimal shippingFee = 0;
        private string selectedShippingMethod = "Viettel Post";
        private readonly string _conn = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        public Payment()
        {
            InitializeComponent();
            Loaded += Payment_Loaded;
        }

        // Constructor nhận dữ liệu từ Cart
        public Payment(ObservableCollection<OrderDetail> cartItems, User user)
        {
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            orderDetails = cartItems ?? throw new ArgumentNullException(nameof(cartItems));
            if (!orderDetails.Any())
                throw new InvalidOperationException("Giỏ hàng trống");
            InitializeComponent();
            Loaded += Payment_Loaded;
        }

        private async Task<bool> HasSufficientStockAsync()
        {
            const string sql = "SELECT UnitInStock FROM PRODUCTS WHERE ProductId=@ProductId";
            using (var conn = new SqlConnection(_conn))
            {
                await conn.OpenAsync();
                foreach (var od in orderDetails)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", od.ProductId);
                        var stock = (int?)await cmd.ExecuteScalarAsync() ?? 0;
                        if (od.Quantity > stock)
                        {
                            MessageBox.Show($"Sản phẩm '{od.Product?.Name}' chỉ còn {stock} trong kho.",
                                "Không đủ hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private async Task<bool> SaveOrderToDatabaseAsync(Order order)
        {
            const string insertOrder = @"INSERT INTO ORDERS (UserId, OrderDate, TotalAmount, ApprovalStatus,
                                        PaymentStatus, ShippingStatus, Address, Note)
                                        OUTPUT INSERTED.OrderId
                                        VALUES (@UserId,@OrderDate,@TotalAmount,@ApprovalStatus,
                                        @PaymentStatus,@ShippingStatus,@Address,@Note)";

            const string insertDetail = @"INSERT INTO ORDER_DETAILS (OrderId, ProductId, Quantity)
                                        VALUES (@OrderId,@ProductId,@Quantity)";
            const string updateStock = @"UPDATE PRODUCTS SET UnitInStock = UnitInStock - @Quantity WHERE ProductId=@ProductId";
            const string deleteCart = @"DELETE ci FROM CART_ITEMS ci
                                        JOIN CART c ON c.CartId = ci.CartId
                                        WHERE c.UserId=@UserId AND ci.ProductId=@ProductId";

            using (var conn = new SqlConnection(_conn))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Order
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

                        // Details + stock + cart cleanup
                        foreach (var d in order.Details)
                        {
                            using (var cmd = new SqlCommand(insertDetail, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                                cmd.Parameters.AddWithValue("@ProductId", d.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
                                cmd.Parameters.AddWithValue("@UnitPrice", d.Product?.UnitPrice ?? 0);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (var cmd = new SqlCommand(updateStock, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", d.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (var cmd = new SqlCommand(deleteCart, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@UserId", order.UserId);
                                cmd.Parameters.AddWithValue("@ProductId", d.ProductId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task<bool> ClearPurchasedItemsFromDbAsync(IEnumerable<OrderDetail> details)
        {
            const string sql = @"DELETE ci FROM CART_ITEMS ci
                                JOIN CART c ON c.CartId = ci.CartId
                                WHERE c.UserId = @UserId AND ci.ProductId = @ProductId";

            using (var conn = new SqlConnection(_conn))
            {
                await conn.OpenAsync();
                foreach (var d in details)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", currentUser.UserId);
                        cmd.Parameters.AddWithValue("@ProductId", d.ProductId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            return true;
        }

        #region Load Data

        // Load dữ liệu thực từ Cart
        private void Payment_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        #endregion

        #region Update UI

        private void UpdateUI()
        {
            if (!IsLoaded) return;
            if (orderDetails == null || currentUser == null) return;

            UpdateAddress();
            UpdateProducts();
            UpdateSummary();
        }

        // Cập nhật địa chỉ - populate các TextBox
        private void UpdateAddress()
        {
            if (currentUser != null)
            {
                if (CustomerNameTextBox == null || CustomerPhoneTextBox == null ||
                    ProvinceComboBox == null || DetailedAddressTextBox == null)
                    return;

                // Điền thông tin hiện có từ currentUser
                CustomerNameTextBox.Text = currentUser.FullName;
                CustomerPhoneTextBox.Text = currentUser.Phone;

                // Parse địa chỉ để tách tỉnh/thành phố
                // Format: "Số nhà, đường, phường, quận, Tỉnh/Thành phố"
                if (!string.IsNullOrEmpty(currentUser.Address))
                {
                    var addressParts = currentUser.Address.Split(',');
                    if (addressParts.Length > 0)
                    {
                        // Lấy phần cuối cùng là tỉnh/thành phố
                        string province = addressParts[addressParts.Length - 1].Trim();

                        // Tìm và chọn tỉnh/thành phố trong ComboBox
                        foreach (ComboBoxItem item in ProvinceComboBox.Items)
                        {
                            if (item.Content.ToString().Contains(province) ||
                                province.Contains(item.Content.ToString()))
                            {
                                ProvinceComboBox.SelectedItem = item;
                                break;
                            }
                        }

                        // Phần còn lại là địa chỉ chi tiết
                        if (addressParts.Length > 1)
                        {
                            var detailedAddress = string.Join(", ",
                                addressParts.Take(addressParts.Length - 1));
                            DetailedAddressTextBox.Text = detailedAddress.Trim();
                        }
                    }
                }
            }
        }

        private void UpdateProducts()
        {
            // Tạo danh sách ProductDisplayItem để hiển thị
            var displayItems = orderDetails.Select(od => new ProductDisplayItem
            {
                Name = od.Product.Name,
                Variant = od.Product.Description,
                Price = od.SubTotal, // Đã tính discount
                Quantity = od.Quantity,
                ImageUrl = od.Product.Picture
            }).ToList();

            ProductItemsControl.ItemsSource = displayItems;

            // Tính tổng tiền
            subtotal = orderDetails.Sum(od => od.SubTotal);

            // Cập nhật số lượng
            int totalItems = orderDetails.Sum(od => od.Quantity);
            TotalItemsText.Text = totalItems == 1 ? "1 sản phẩm" : $"{totalItems} sản phẩm";
            SubtotalText.Text = $"{subtotal:N0}đ";
        }

        // Cập nhật tổng kết thanh toán
        private void UpdateSummary()
        {
            // Tính phí vận chuyển
            if (ViettelPostOption.IsChecked == true)
            {
                shippingFee = 0; // Miễn phí
                selectedShippingMethod = "Viettel Post";
                if (ShippingFeeText != null)
                {
                    ShippingFeeText.Text = "0đ";
                    ShippingFeeText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4CAF50
                }
            }
            else if (FastShippingOption.IsChecked == true)
            {
                shippingFee = 20000;
                selectedShippingMethod = "Giao hàng nhanh";
                if (ShippingFeeText != null)
                {
                    ShippingFeeText.Text = $"{shippingFee:N0}đ";
                    ShippingFeeText.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // #333333
                }
            }

            // Cập nhật subtotal
            if (SummarySubtotalText != null)
            {
                SummarySubtotalText.Text = $"{subtotal:N0}đ";
            }

            // Tổng cộng
            decimal total = subtotal + shippingFee;
            if (TotalAmountText != null)
            {
                TotalAmountText.Text = $"{total:N0}đ";
            }
        }

        #endregion

        #region Event Handlers

        // Nút quay lại
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        // Thay đổi phương thức vận chuyển
        private void ShippingOptionChanged(object sender, RoutedEventArgs e)
        {
            UpdateSummary();
        }

        // Đặt hàng
        private async void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            // Validate thông tin địa chỉ
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerPhoneTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerPhoneTextBox.Focus();
                return;
            }

            if (ProvinceComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Tỉnh/Thành phố", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ProvinceComboBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(DetailedAddressTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ cụ thể", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DetailedAddressTextBox.Focus();
                return;
            }

            if (currentUser == null || orderDetails == null || !orderDetails.Any())
            {
                MessageBox.Show("Thiếu thông tin đơn hàng hoặc người dùng.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!await HasSufficientStockAsync()) return;

            // Tạo địa chỉ đầy đủ từ các ô nhập
            string fullName = CustomerNameTextBox.Text.Trim();
            string phone = CustomerPhoneTextBox.Text.Trim();
            string province = (ProvinceComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string detailedAddress = DetailedAddressTextBox.Text.Trim();
            string fullAddress = $"{detailedAddress}, {province}";

            const string paymentMethod = "COD";
            const string paymentMethodDisplay = "Thanh toán khi nhận hàng (COD)";

            string note = SellerNoteTextBox.Text.Trim();
            decimal total = subtotal + shippingFee;

            string orderInfo = $"Xác nhận đặt hàng?\n\n" +
                              $"Người nhận: {fullName}\n" +
                              $"SĐT: {phone}\n" +
                              $"Địa chỉ: {fullAddress}\n\n" +
                              $"Số sản phẩm: {orderDetails.Sum(od => od.Quantity)}\n" +
                              $"Tạm tính: {subtotal:N0}đ\n" +
                              $"Phí ship: {shippingFee:N0}đ\n" +
                              $"Tổng cộng: {total:N0}đ\n\n" +
                              $"Phương thức vận chuyển: {selectedShippingMethod}\n" +
                              $"Phương thức thanh toán: {paymentMethodDisplay}\n";

            if (!string.IsNullOrEmpty(note))
            {
                orderInfo += $"Ghi chú: {note}";
            }

            var result = MessageBox.Show(orderInfo, "Xác nhận đặt hàng",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Tạo đơn hàng mới với địa chỉ đã cập nhật
                var newOrder = CreateOrder(paymentMethod, note, total, fullAddress);

                var success = true;
                try
                {
                    success = await SaveOrderToDatabaseAsync(newOrder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lưu đơn hàng thất bại: " + ex.Message,
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (success)
                {
                    MessageBox.Show(
                        $"Đặt hàng thành công!\n\n" +
                        $"Mã đơn hàng: {newOrder.OrderId}\n" +
                        $"Tổng tiền: {total:N0}đ\n\n" +
                        $"Cảm ơn bạn đã mua hàng. Chúng tôi sẽ liên hệ với bạn sớm nhất.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    await ClearPurchasedItemsFromDbAsync(orderDetails);
                    CartService.RemoveByProductIds(orderDetails.Select(d => d.ProductId));
                }
                else
                {
                    MessageBox.Show("Đặt hàng thất bại. Vui lòng thử lại sau.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Quay về trang chính hoặc trang đơn hàng
                NavigationService?.GoBack();
            }
        }

        #endregion

        #region Helper Methods

        // Tạo đối tượng Order với địa chỉ tùy chỉnh
        private Order CreateOrder(string paymentMethod, string note, decimal total, string address)
        {
            var order = new Order
            {
                OrderId = GenerateOrderId(),
                UserId = currentUser.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                ApprovalStatus = "Waiting",
                PaymentStatus = "Pending",
                ShippingStatus = "Pending",
                Address = address, // Sử dụng địa chỉ từ form
                Note = note,
                Details = new System.Collections.ObjectModel.ObservableCollection<OrderDetail>(orderDetails)
            };

            // Cập nhật OrderId cho các OrderDetail
            foreach (var detail in order.Details)
            {
                detail.OrderId = order.OrderId;
            }

            return order;
        }

        // Tạo mã đơn hàng
        private string GenerateOrderId()
        {
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}";
        }

        #endregion
    }

    #region Helper Classes

    // Class để hiển thị sản phẩm trong UI
    public class ProductDisplayItem
    {
        public string Name { get; set; }
        public string Variant { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }

    #endregion
}