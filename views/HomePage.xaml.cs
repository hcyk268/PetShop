using Pet_Shop_Project.Controls;
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
    public partial class HomePage : Page
    {
        private List<Product> allProducts;
        private string currentCategory = "All";

        public HomePage()
        {
            InitializeComponent();
            LoadProducts();
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private void LoadProducts()
        {
            // TODO: Replace this with actual database call
            // For now, using sample data
            allProducts = GetSampleProducts();
            DisplayProducts(allProducts);
        }

        private List<Product> GetSampleProducts()
        {
            // Sample products - replace with actual database query
            return new List<Product>
            {
                new Product
                {
                    ProductId = "P001",
                    Name = "Sản phẩm 1",
                    Description = "Mô tả sản phẩm 1",
                    UnitPrice = 75000,
                    Discount = 0,
                    UnitInStock = 10,
                    Picture = "/Resources/product1.jpg" // Update with actual image path
                },
                new Product
                {
                    ProductId = "P002",
                    Name = "Sản phẩm 2",
                    Description = "Mô tả sản phẩm 2",
                    UnitPrice = 85000,
                    Discount = 0.1,
                    UnitInStock = 15,
                    Picture = "/Resources/product2.jpg"
                },
                new Product
                {
                    ProductId = "P003",
                    Name = "Sản phẩm 3",
                    Description = "Mô tả sản phẩm 3",
                    UnitPrice = 95000,
                    Discount = 0,
                    UnitInStock = 20,
                    Picture = "/Resources/product3.jpg"
                },
                new Product
                {
                    ProductId = "P004",
                    Name = "Sản phẩm 4",
                    Description = "Mô tả sản phẩm 4",
                    UnitPrice = 120000,
                    Discount = 0.15,
                    UnitInStock = 8,
                    Picture = "/Resources/product4.jpg"
                }
            };
        }

        private void DisplayProducts(List<Product> products)
        {
            ProductsPanel.Children.Clear();

            foreach (var product in products)
            {
                var productCard = new ProductCard(product);
                ProductsPanel.Children.Add(productCard);
            }
        }

        private void FoodButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategory = "Food";
            FilterProductsByCategory();
        }

        private void ToyButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategory = "Toy";
            FilterProductsByCategory();
        }

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategory = "Tool";
            FilterProductsByCategory();
        }

        private void FilterProductsByCategory()
        {
            // TODO: Implement category filtering based on your database structure
            // For now, showing all products
            var filteredProducts = allProducts; // Replace with actual category filter
            ApplyCurrentFilter(filteredProducts);
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedItem == null) return;

            var selectedFilter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var productsToFilter = GetCurrentProducts();

            ApplyCurrentFilter(productsToFilter);
        }

        private List<Product> GetCurrentProducts()
        {
            // Get products based on current category and search
            var products = allProducts;

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                products = products.Where(p =>
                    p.Name.ToLower().Contains(SearchBox.Text.ToLower())).ToList();
            }

            return products;
        }

        private void ApplyCurrentFilter(List<Product> products)
        {
            if (FilterComboBox.SelectedItem == null)
            {
                DisplayProducts(products);
                return;
            }

            var selectedFilter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            List<Product> sortedProducts = products;

            switch (selectedFilter)
            {
                case "Giá thấp đến cao":
                    sortedProducts = products.OrderBy(p => p.FinalPrice).ToList();
                    break;
                case "Giá cao đến thấp":
                    sortedProducts = products.OrderByDescending(p => p.FinalPrice).ToList();
                    break;
                case "Đánh giá cao nhất":
                    // TODO: Implement rating sort when rating field is added to Product model
                    sortedProducts = products;
                    break;
                default:
                    sortedProducts = products;
                    break;
            }

            DisplayProducts(sortedProducts);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                ApplyCurrentFilter(allProducts);
                return;
            }

            var filteredProducts = allProducts
                .Where(p => p.Name.ToLower().Contains(searchText))
                .ToList();

            ApplyCurrentFilter(filteredProducts);
        }
    }
}