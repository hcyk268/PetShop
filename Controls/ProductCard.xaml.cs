using MaterialDesignThemes.Wpf;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pet_Shop_Project.Controls
{
    public partial class ProductCard : UserControl
    {
        private Product _product;
        private ReviewService _reviewService;

        public ProductCard()
        {
            InitializeComponent();
            _reviewService = new ReviewService();
        }

        public ProductCard(Product product) : this()
        {
            _product = product;
            _ = LoadProductDataAsync();
        }

        private async Task LoadProductDataAsync()
        {
            if (_product == null) return;

            // Set product name
            ProductName.Text = _product.Name;

            // Set product image
            if (!string.IsNullOrEmpty(_product.Picture))
            {
                try
                {
                    ProductImage.Source = new BitmapImage(new Uri(_product.Picture, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    // Use placeholder if image fails to load
                    ProductImage.Source = null;
                }
            }

            // Xử lý discount
            bool hasDiscount = _product.Discount > 0;

            if (hasDiscount)
            {
                // 1. Hiển thị discount badge
                DiscountBadge.Visibility = Visibility.Visible;
                DiscountText.Text = $"-{(_product.Discount * 100):0}%";

                // 2. Giá có gradient
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
                ProductPrice.Text = $"{_product.FinalPrice:N0} đ";

                // 4. Hiển thị giá gốc gạch ngang
                OriginalPrice.Text = $"{_product.UnitPrice:N0} đ";
                OriginalPrice.Visibility = Visibility.Visible;
            }
            else
            {
                // Không có discount: giá bình thường
                DiscountBadge.Visibility = Visibility.Collapsed;
                ProductPrice.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
                ProductPrice.Text = $"{_product.UnitPrice:N0} đ";
                OriginalPrice.Visibility = Visibility.Collapsed;
            }

            // Lấy rating thực tế từ database
            await LoadRatingAsync();
        }

        private async Task LoadRatingAsync()
        {
            try
            {
                double avgRating = await _reviewService.GetAverageRatingAsync(_product.ProductId);
                int reviewCount = await _reviewService.GetReviewCountAsync(_product.ProductId);

                // Làm tròn rating đến 0.5
                int displayRating = (int)Math.Round(avgRating * 2) / 2;
                if (displayRating > 5) displayRating = 5;
                if (displayRating < 0) displayRating = 0;

                GenerateStars(avgRating);

                // Hiển thị rating trung bình với số lượng reviews
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
                // Fallback to default
                GenerateStars(0);
                RatingText.Text = "(0/5)";
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
                    iconKind = PackIconKind.Star; // Full star
                }
                else if (rating >= i + 0.5)
                {
                    iconKind = PackIconKind.StarHalfFull; // Half star
                }
                else
                {
                    iconKind = PackIconKind.StarOutline; // Empty star
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

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                _ = LoadProductDataAsync();
            }
        }

        private void ProductCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_product != null)
            {
                // Navigate to ProductDetailPage using NavigationService
                Services.NavigationService.Instance.NavigateToProductDetail(_product);
            }
        }
    }
}