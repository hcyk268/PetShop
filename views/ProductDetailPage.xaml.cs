using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using MaterialDesignThemes.Wpf;

using NavService = Pet_Shop_Project.Services.NavigationService; // Tránh bị trùng

namespace Pet_Shop_Project.Views
{
    public partial class ProductDetailPage : Page
    {
        private Product _product;
        private int _quantity = 1;
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
        }

        public ProductDetailPage(Product product) : this()
        {
            _product = product;
            LoadProductDetails();
        }

        private void LoadProductDetails()
        {
            if (_product == null) return;

            // Set product name
            ProductName.Text = _product.Name;

            // Set product description
            ProductDescription.Text = _product.Description;

            // Set product image
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

            // Set price with gradient (already in XAML)
            ProductPrice.Text = $"{_product.FinalPrice:N0}đ";

            // Set stock
            StockText.Text = _product.UnitInStock.ToString();

            // Generate rating stars (placeholder - 5 stars)
            GenerateStars(5);

            // Set initial quantity
            QuantityText.Text = _quantity.ToString();
        }

        private void GenerateStars(int rating)
        {
            StarPanel.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                var starIcon = new PackIcon
                {
                    Kind = i < rating ? PackIconKind.Star : PackIconKind.StarOutline,
                    Width = 16,
                    Height = 16,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)),
                    Margin = new Thickness(0, 0, 2, 0)
                };
                StarPanel.Children.Add(starIcon);
            }

            // Update rating text
            RatingText.Text = $"{rating}/5 *";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Quay lại page trước đó (HomePage) với trạng thái GIỮ NGUYÊN
            NavService.Instance.GoBack();
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder - sẽ implement sau
            MessageBox.Show($"Thêm {_quantity} sản phẩm vào giỏ hàng",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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