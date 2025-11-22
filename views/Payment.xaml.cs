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

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for Payment.xaml
    /// </summary>
    public partial class Payment : Page
    {
        // Biến lưu trữ dữ liệu
        private List<OrderDetail> orderDetails;
        private User currentUser;
        private decimal subtotal = 0;
        private decimal shippingFee = 0;
        private string selectedShippingMethod = "Viettel Post";

        public Payment()
        {
            InitializeComponent();
            LoadSampleData();
            UpdateUI();
        }

        // Constructor nhận dữ liệu từ Cart
        public Payment(List<OrderDetail> cartItems, User user) : this()
        {
            orderDetails = cartItems;
            currentUser = user;
            LoadData();
        }

        #region Load Data

        // Load dữ liệu mẫu
        private void LoadSampleData()
        {
            // Dữ liệu địa chỉ mẫu
            currentUser = new User
            {
                UserId = "U001",
                FullName = "Nguyễn Văn A",
                Phone = "(+84) 0123 456 789",
                Address = "123 Đường Nguyễn Văn Cừ, Phường 4, Quận 5, Thành phố Hồ Chí Minh, Việt Nam",
                Email = "nguyenvana@example.com"
            };

            // Dữ liệu sản phẩm mẫu
            orderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    OrderDetailId = "OD001",
                    ProductId = "P001",
                    Quantity = 1,
                    Product = new Product
                    {
                        ProductId = "P001",
                        Name = "Thức ăn cho chó Royal Canin",
                        Description = "Size 5kg",
                        UnitPrice = 500000,
                        Discount = 0,
                        Picture = "/Assets/sample-product.jpg",
                        UnitInStock = 50
                    }
                },
                new OrderDetail
                {
                    OrderDetailId = "OD002",
                    ProductId = "P002",
                    Quantity = 2,
                    Product = new Product
                    {
                        ProductId = "P002",
                        Name = "Đồ chơi cho mèo",
                        Description = "Chuột nhồi bông",
                        UnitPrice = 150000,
                        Discount = 0.1, // Giảm 10%
                        Picture = "/Assets/sample-product2.jpg",
                        UnitInStock = 100
                    }
                }
            };
        }

        // Load dữ liệu thực từ Cart
        private void LoadData()
        {
            if (orderDetails == null || orderDetails.Count == 0)
            {
                LoadSampleData();
            }
            UpdateUI();
        }

        #endregion

        #region Update UI

        private void UpdateUI()
        {
            UpdateAddress();
            UpdateProducts();
            UpdateSummary();
        }

        // Cập nhật địa chỉ
        private void UpdateAddress()
        {
            if (currentUser != null)
            {
                CustomerNameText.Text = currentUser.FullName;
                CustomerPhoneText.Text = currentUser.Phone;
                CustomerAddressText.Text = currentUser.Address;
                DefaultAddressBadge.Visibility = Visibility.Visible;
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
            SubtotalText.Text = $"₫ {subtotal:N0}";
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
                    ShippingFeeText.Text = "₫ 0";
                    ShippingFeeText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4CAF50
                }
            }
            else if (FastShippingOption.IsChecked == true)
            {
                shippingFee = 20000;
                selectedShippingMethod = "Giao hàng nhanh";
                if (ShippingFeeText != null)
                {
                    ShippingFeeText.Text = $"₫ {shippingFee:N0}";
                    ShippingFeeText.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // #333333
                }
            }

            // Cập nhật subtotal
            if (SummarySubtotalText != null)
            {
                SummarySubtotalText.Text = $"₫ {subtotal:N0}";
            }

            // Tổng cộng
            decimal total = subtotal + shippingFee;
            if (SummarySubtotalText != null)
            {
                TotalAmountText.Text = $"₫ {total:N0}";
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

        // Thay đổi địa chỉ
        private void ChangeAddress_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Mở dialog chọn địa chỉ khác hoặc chỉnh sửa địa chỉ
            MessageBox.Show("Chức năng thay đổi địa chỉ đang được phát triển!",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Thay đổi phương thức vận chuyển
        private void ShippingOptionChanged(object sender, RoutedEventArgs e)
        {
            UpdateSummary();
        }

        // Đặt hàng
        // Đặt hàng
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra phương thức thanh toán
            string paymentMethod = CODPaymentOption.IsChecked == true
                ? "COD"
                : "Card";

            if (CardPaymentOption.IsChecked == true)
            {
                MessageBox.Show("Chức năng thanh toán bằng thẻ đang được phát triển!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lấy ghi chú
            string note = SellerNoteTextBox.Text.Trim();

            // Tính tổng tiền
            decimal total = subtotal + shippingFee;

            // Tạo thông tin đơn hàng để hiển thị
            string paymentMethodDisplay = paymentMethod == "COD"
                ? "Thanh toán khi nhận hàng (COD)"
                : "Thẻ Tín dụng/Ghi nợ";

            string orderInfo = $"Xác nhận đặt hàng?\n\n" +
                              $"Người nhận: {currentUser.FullName}\n" +
                              $"SĐT: {currentUser.Phone}\n" +
                              $"Địa chỉ: {currentUser.Address}\n\n" +
                              $"Số sản phẩm: {orderDetails.Sum(od => od.Quantity)}\n" +
                              $"Tạm tính: {subtotal:N0}đ\n" +
                              $"Phí ship: {shippingFee:N0}đ\n";

            orderInfo += $"Tổng cộng: {total:N0}đ\n\n" +
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
                // Tạo đơn hàng mới
                var newOrder = CreateOrder(paymentMethod, note, total);

                // TODO: Lưu đơn hàng vào database
                // bool success = SaveOrderToDatabase(newOrder);
                bool success = true; // Giả lập thành công

                if (success)
                {
                    MessageBox.Show(
                        $"Đặt hàng thành công!\n\n" +
                        $"Mã đơn hàng: {newOrder.OrderId}\n" +
                        $"Tổng tiền: {total:N0}đ\n\n" +
                        $"Cảm ơn bạn đã mua hàng. Chúng tôi sẽ liên hệ với bạn sớm nhất.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // TODO: Xóa giỏ hàng và chuyển đến trang đơn hàng hoặc trang chủ
                    // ClearCart();
                    // NavigationService?.Navigate(new OrderHistory());
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Helper Methods

        // Tạo đối tượng Order
        private Order CreateOrder(string paymentMethod, string note, decimal total)
        {
            var order = new Order
            {
                OrderId = GenerateOrderId(),
                UserId = currentUser.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                ApprovalStatus = "Waiting",
                PaymentStatus = paymentMethod == "COD" ? "Pending" : "Paid",
                ShippingStatus = "Pending",
                Address = currentUser.Address,
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