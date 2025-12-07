using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using MaterialDesignThemes.Wpf;

using NavService = Pet_Shop_Project.Services.NavigationService;

namespace Pet_Shop_Project.Views
{
    public partial class ProductDetailPage : Page
    {
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        private Product _product;
        private int _quantity = 1;
        private ReviewService _reviewService;

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

        public ProductDetailPage()
        {
            InitializeComponent();
            _reviewService = new ReviewService();
        }

        public ProductDetailPage(Product product) : this()
        {
            _product = product;
            LoadProductDetails();
            LoadReviews();
        }

        private void LoadProductDetails()
        {
            if (_product == null) return;

            ProductName.Text = _product.Name;
            ProductDescription.Text = _product.Description;

            if (!string.IsNullOrEmpty(_product.Picture))
            {
                try
                {
                    ProductImage.Source = new BitmapImage(new Uri(_product.Picture, UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Không load được ảnh: {ex.Message}");
                }
            }

            // Xử lý discount
            bool hasDiscount = _product.Discount > 0;

            if (hasDiscount)
            {
                // 1. Hiển thị discount badge
                DiscountBadge.Visibility = Visibility.Visible;
                DiscountText.Text = $"-{(_product.Discount * 100):0}%";

                // 2. Giá có gradient (FFC476 → FFA2A2)
                ProductPrice.Foreground = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(0xFF, 0xC4, 0x76), 0),
                        new GradientStop(Color.FromRgb(0xFF, 0xA2, 0xA2), 1)
                    }
                };

                // 3. Hiển thị giá sau giảm
                ProductPrice.Text = $"{_product.FinalPrice:N0}đ";

                // 4. Hiển thị giá gốc gạch ngang
                OriginalPrice.Text = $"{_product.UnitPrice:N0}đ";
                OriginalPrice.Visibility = Visibility.Visible;
            }
            else
            {
                // Không có discount: giá bình thường
                DiscountBadge.Visibility = Visibility.Collapsed;
                ProductPrice.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
                ProductPrice.Text = $"{_product.UnitPrice:N0}đ";
                OriginalPrice.Visibility = Visibility.Collapsed;
            }

            StockText.Text = _product.UnitInStock.ToString();

            // Load rating thực tế
            LoadRating();

            QuantityText.Text = _quantity.ToString();
        }

        private void LoadRating()
        {
            try
            {
                double avgRating = _reviewService.GetAverageRating(_product.ProductId);
                int reviewCount = _reviewService.GetReviewCount(_product.ProductId);

                GenerateStars(avgRating);

                if (reviewCount > 0)
                {
                    RatingText.Text = $"{avgRating:F1}";
                }
                else
                {
                    RatingText.Text = "0.0";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load rating: {ex.Message}");
                GenerateStars(0);
                RatingText.Text = "0.0";
            }
        }

        private void GenerateStars(double rating)
        {
            StarPanel.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                PackIconKind iconKind;

                if (rating >= i + 1)
                {
                    iconKind = PackIconKind.Star;
                }
                else if (rating >= i + 0.5)
                {
                    iconKind = PackIconKind.StarHalfFull;
                }
                else
                {
                    iconKind = PackIconKind.StarOutline;
                }

                var starIcon = new PackIcon
                {
                    Kind = iconKind,
                    Width = 12,
                    Height = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)),
                    Margin = new Thickness(0, 0, 1, 0)
                };
                StarPanel.Children.Add(starIcon);
            }
        }

        private void LoadReviews()
        {
            try
            {
                var reviews = _reviewService.GetReviewsByProductId(_product.ProductId);

                // Xóa các review placeholder
                ReviewsPanel.Children.Clear();

                if (reviews.Count == 0)
                {
                    // Hiển thị thông báo không có review
                    TextBlock noReviewText = new TextBlock
                    {
                        Text = "Chưa có đánh giá nào cho sản phẩm này",
                        FontSize = 15,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 20)
                    };
                    ReviewsPanel.Children.Add(noReviewText);
                    return;
                }

                // Tạo review items từ database
                foreach (var review in reviews)
                {
                    string userFullName = _reviewService.GetUserFullName(review.UserId);
                    var reviewItem = CreateReviewItem(userFullName, review.Rating, review.Comment, review.ReviewDate);
                    ReviewsPanel.Children.Add(reviewItem);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load reviews: {ex.Message}");
            }
        }

        private Border CreateReviewItem(string userName, int rating, string comment, DateTime reviewDate)
        {
            Border reviewBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF1, 0xE2)),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 15)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Avatar
            Border avatarBorder = new Border
            {
                Width = 60,
                Height = 60,
                CornerRadius = new CornerRadius(30),
                Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                Margin = new Thickness(0, 0, 20, 0)
            };

            TextBlock avatarText = new TextBlock
            {
                Text = "👤",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            avatarBorder.Child = avatarText;
            Grid.SetColumn(avatarBorder, 0);

            // Review Content
            StackPanel contentPanel = new StackPanel();

            // Header: Tên, Rating và Date trên cùng một hàng
            StackPanel headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            // User name
            TextBlock nameText = new TextBlock
            {
                Text = userName,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Rating
            TextBlock ratingText = new TextBlock
            {
                Text = $"{rating} ★",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };

            // Date với icon đồng hồ
            StackPanel datePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            PackIcon clockIcon = new PackIcon
            {
                Kind = PackIconKind.ClockOutline,
                Width = 14,
                Height = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            TextBlock dateText = new TextBlock
            {
                Text = reviewDate.ToString("dd/MM/yyyy"),
                FontSize = 16,
                FontWeight = FontWeights.Light,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                VerticalAlignment = VerticalAlignment.Center
            };

            datePanel.Children.Add(clockIcon);
            datePanel.Children.Add(dateText);

            headerPanel.Children.Add(nameText);
            headerPanel.Children.Add(ratingText);
            headerPanel.Children.Add(datePanel);

            // Comment
            TextBlock commentText = new TextBlock
            {
                Text = comment,
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                TextWrapping = TextWrapping.Wrap
            };

            contentPanel.Children.Add(headerPanel);
            contentPanel.Children.Add(commentText);

            Grid.SetColumn(contentPanel, 1);

            grid.Children.Add(avatarBorder);
            grid.Children.Add(contentPanel);

            reviewBorder.Child = grid;

            return reviewBorder;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavService.Instance.GoBack();
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy userId từ NavigationService
                string userId = NavService.Instance.userid;

                if (string.IsNullOrEmpty(userId))
                {
                    MessageBox.Show("Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra số lượng
                if (_quantity > _product.UnitInStock)
                {
                    MessageBox.Show($"Chỉ còn {_product.UnitInStock} sản phẩm trong kho!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Thêm vào database
                AddToCartInDatabase(userId, _product.ProductId, _quantity);

                // Thêm vào CartService (in-memory)
                CartService.AddToCart(_product, _quantity);

                MessageBox.Show($"Đã thêm {_quantity} sản phẩm '{_product.Name}' vào giỏ hàng!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reset quantity về 1
                _quantity = 1;
                QuantityText.Text = _quantity.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// Thêm sản phẩm vào giỏ hàng trong database
        private void AddToCartInDatabase(string userId, string productId, int quantity)
        {
            using (var conn = new SqlConnection(_connectionDB))
            {
                conn.Open();

                // Bước 1: Lấy hoặc tạo CartId cho user
                string cartId = GetOrCreateCart(conn, userId);

                // Bước 2: Kiểm tra sản phẩm đã có trong giỏ chưa
                string checkSql = @"
                    SELECT CartItemId, Quantity 
                    FROM CART_ITEMS 
                    WHERE CartId = @CartId AND ProductId = @ProductId";

                using (var checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@CartId", cartId);
                    checkCmd.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Sản phẩm đã tồn tại -> Cập nhật số lượng
                            string cartItemId = reader["CartItemId"].ToString();
                            int currentQty = (int)reader["Quantity"];
                            reader.Close();

                            UpdateCartItemQuantity(conn, cartItemId, currentQty + quantity);
                        }
                        else
                        {
                            // Sản phẩm chưa có -> Thêm mới
                            reader.Close();
                            InsertNewCartItem(conn, cartId, productId, quantity);
                        }
                    }
                }
            }
        }

        /// Lấy CartId của user, nếu chưa có thì tạo mới
        private string GetOrCreateCart(SqlConnection conn, string userId)
        {
            // Kiểm tra cart đã tồn tại chưa
            string checkCartSql = "SELECT CartId FROM CART WHERE UserId = @UserId";

            using (var cmd = new SqlCommand(checkCartSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return result.ToString();
                }
            }

            // Nếu chưa có, tạo cart mới
            string newCartId = Guid.NewGuid().ToString();
            string insertCartSql = "INSERT INTO CART (CartId, UserId) VALUES (@CartId, @UserId)";

            using (var cmd = new SqlCommand(insertCartSql, conn))
            {
                cmd.Parameters.AddWithValue("@CartId", newCartId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }

            return newCartId;
        }

        /// Cập nhật số lượng của CartItem
        private void UpdateCartItemQuantity(SqlConnection conn, string cartItemId, int newQuantity)
        {
            string updateSql = "UPDATE CART_ITEMS SET Quantity = @Quantity WHERE CartItemId = @CartItemId";

            using (var cmd = new SqlCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                cmd.ExecuteNonQuery();
            }
        }

        /// Thêm CartItem mới vào database
        private void InsertNewCartItem(SqlConnection conn, string cartId, string productId, int quantity)
        {
            string insertSql = @"
                INSERT INTO CART_ITEMS (CartId, ProductId, Quantity) 
                VALUES (@CartId, @ProductId, @Quantity)";

            using (var cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@CartId", cartId);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.ExecuteNonQuery();
            }
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_quantity > 1)
            {
                _quantity--;
                QuantityText.Text = _quantity.ToString();
            }
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_quantity < _product.UnitInStock)
            {
                _quantity++;
                QuantityText.Text = _quantity.ToString();
            }
            else
            {
                MessageBox.Show($"Chỉ còn {_product.UnitInStock} sản phẩm trong kho",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}