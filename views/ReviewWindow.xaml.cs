using MaterialDesignThemes.Wpf;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pet_Shop_Project.Views
{
    public partial class ReviewWindow : Window
    {
        private OrderDetail _orderDetail;
        private string _userId;
        private ReviewService _reviewService;
        private int _selectedRating = 0;
        private Button[] _starButtons;

        public ReviewWindow(OrderDetail orderDetail, string userId)
        {
            InitializeComponent();
            _orderDetail = orderDetail;
            _userId = userId;
            _reviewService = new ReviewService();

            InitializeStars();
            LoadProductInfo();
        }

        private void InitializeStars()
        {
            _starButtons = new Button[5];

            for (int i = 0; i < 5; i++)
            {
                int starIndex = i + 1; 

                var button = new Button
                {
                    Width = 30,
                    Height = 39,
                    Margin = new Thickness(4),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = starIndex,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(0)
                };

                var icon = new PackIcon
                {
                    Kind = PackIconKind.StarOutline,
                    Width = 20,
                    Height = 20,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xD9, 0x78)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                button.Content = icon;
                button.Click += StarButton_Click;
                button.MouseEnter += StarButton_MouseEnter;
                button.MouseLeave += StarButton_MouseLeave;

                StarPanel.Children.Add(button);
                _starButtons[i] = button;
            }
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _selectedRating = (int)button.Tag;
            UpdateStarDisplay(_selectedRating);
        }

        private void StarButton_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            int hoverRating = (int)button.Tag;
            UpdateStarDisplay(hoverRating, true);
        }

        private void StarButton_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateStarDisplay(_selectedRating);
        }

        private void UpdateStarDisplay(int rating, bool isHover = false)
        {
            for (int i = 0; i < 5; i++)
            {
                var icon = _starButtons[i].Content as PackIcon;
                if (i < rating)
                {
                    icon.Kind = PackIconKind.Star;
                    icon.Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xD9, 0x78));
                }
                else
                {
                    icon.Kind = PackIconKind.StarOutline;
                    icon.Foreground = isHover
                        ? new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0))
                        : new SolidColorBrush(Color.FromRgb(0xF9, 0xD9, 0x78));
                }
            }
        }

        private void LoadProductInfo()
        {
            if (_orderDetail?.Product == null) return;

            var product = _orderDetail.Product;

            // Load product image
            if (!string.IsNullOrEmpty(product.Picture))
            {
                try
                {
                    ProductImage.Source = new BitmapImage(new Uri(product.Picture, UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
                }
            }

            ProductName.Text = product.Name;

            ProductQuantity.Text = _orderDetail.Quantity.ToString();

            bool hasDiscount = product.Discount > 0;

            if (hasDiscount)
            {
                // Show discounted price with gradient
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
                ProductPrice.Text = $"{product.FinalPrice:N0}đ";

                // Show discount badge
                DiscountBadge.Visibility = Visibility.Visible;
                DiscountText.Text = $"-{(product.Discount * 100):0}%";
            }
            else
            {
                // Regular price
                ProductPrice.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
                ProductPrice.Text = $"{product.UnitPrice:N0}đ";
                DiscountBadge.Visibility = Visibility.Collapsed;
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

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Show/hide placeholder based on text content
            PlaceholderText.Visibility = string.IsNullOrEmpty(CommentTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate rating
            if (_selectedRating == 0)
            {
                MessageBox.Show("Vui lòng chọn số sao đánh giá!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get comment
            string comment = CommentTextBox.Text;
            if (string.IsNullOrWhiteSpace(comment))
            {
                comment = ""; // Empty comment is acceptable
            }

            // Disable button to prevent double submission
            SubmitButton.IsEnabled = false;

            try
            {
                // Submit review to database
                bool success = await _reviewService.AddReviewAsync(
                    _orderDetail.ProductId,
                    _userId,
                    _selectedRating,
                    comment
                );

                if (success)
                {
                    MessageBox.Show("Đánh giá của bạn đã được gửi thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi gửi đánh giá!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    SubmitButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SubmitButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}