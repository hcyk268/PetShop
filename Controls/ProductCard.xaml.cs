using MaterialDesignThemes.Wpf;
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


namespace Pet_Shop_Project.Controls
{
    public partial class ProductCard : UserControl
    {
        private Product _product;

        public ProductCard()
        {
            InitializeComponent();
        }

        public ProductCard(Product product) : this()
        {
            _product = product;
            LoadProductData();
        }

        private void LoadProductData()
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

            // Set price (format with VND)
            ProductPrice.Text = $"{_product.FinalPrice:N0} đ";

            // Generate rating stars (assuming 5-star rating system)
            GenerateStars(5); // You can modify this to use actual rating from database
        }

        private void GenerateStars(int rating)
        {
            StarPanel.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                var starIcon = new PackIcon
                {
                    Kind = i < rating ? PackIconKind.Star : PackIconKind.StarOutline,
                    Width = 12,
                    Height = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)),
                    Margin = new Thickness(0, 0, 1, 0)
                };
                StarPanel.Children.Add(starIcon);
            }

            RatingText.Text = $"{rating}/5";
        }

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                LoadProductData();
            }
        }
    }
}