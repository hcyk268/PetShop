using Pet_Shop_Project.Controls;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Pet_Shop_Project.Views
{
    public partial class HomePage : Page
    {
        private ProductService productService;
        private ReviewService reviewService;
        private List<Product> allProducts;
        private string currentCategory = "Tất cả";

        public HomePage()
        {
            InitializeComponent();
            productService = new ProductService();
            reviewService = new ReviewService();
            Loaded += HomePage_Loaded;

            // Đăng ký event cho SearchBox
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllProductsAsync();
        }

        // Load tất cả sản phẩm - dùng khi reset hoặc khởi động
        public async Task LoadAllProductsAsync()
        {
            try
            {
                allProducts = await productService.GetAllProductsAsync();
                currentCategory = "Tất cả";
                DisplayProducts(allProducts);

                SearchBox.Text = string.Empty;

                FilterComboBox.SelectedIndex = 0;

                ResetCategoryButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải sản phẩm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hiển thị danh sách sản phẩm lên UI
        private void DisplayProducts(List<Product> products)
        {
            ProductsPanel.Children.Clear();

            if (products == null || products.Count == 0)
            {
                TextBlock noProducts = new TextBlock
                {
                    Text = "Không tìm thấy sản phẩm nào",
                    FontSize = 18,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(20)
                };
                ProductsPanel.Children.Add(noProducts);
                return;
            }

            foreach (var product in products)
            {
                try
                {
                    var productCard = new ProductCard(product);
                    ProductsPanel.Children.Add(productCard);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi tạo ProductCard: {ex.Message}");
                }
            }
        }

        // Click button Thức ăn
        private async void FoodButton_Click(object sender, RoutedEventArgs e)
        {
            await FilterByCategoryAsync("Thức ăn");
            SetActiveButton(FoodButton);
        }

        // Click button Đồ chơi
        private async void ToyButton_Click(object sender, RoutedEventArgs e)
        {
            await FilterByCategoryAsync("Đồ chơi");
            SetActiveButton(ToyButton);
        }

        // Click button Dụng cụ
        private async void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            await FilterByCategoryAsync("Dụng cụ");
            SetActiveButton(ToolButton);
        }

        // Click button Thiết bị
        private async void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            await FilterByCategoryAsync("Thiết bị");
            SetActiveButton(DeviceButton);
        }

        // Set button active (hiện shadow bằng cách thay đổi Effect)
        private void SetActiveButton(Button activeButton)
        {
            // Reset tất cả buttons
            ResetCategoryButtons();

            // Set shadow cho button được chọn
            activeButton.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                ShadowDepth = 0,
                BlurRadius = 15,
                Opacity = 0.3,
                Color = System.Windows.Media.Colors.Black
            };
        }

        // Reset tất cả category buttons về trạng thái bình thường
        private void ResetCategoryButtons()
        {
            FoodButton.Effect = null;
            ToyButton.Effect = null;
            ToolButton.Effect = null;
            DeviceButton.Effect = null;
        }

        // Lọc sản phẩm theo category
        private async Task FilterByCategoryAsync(string category)
        {
            try
            {
                currentCategory = category;

                // Lấy sản phẩm từ database theo category
                var filteredProducts = await productService.GetProductsByCategoryAsync(category);

                // Cập nhật allProducts để filter và search hoạt động đúng
                allProducts = filteredProducts;

                // Clear search box khi chuyển category
                SearchBox.Text = string.Empty;

                // Apply filter hiện tại
                await ApplyCurrentFilterAsync(filteredProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lọc sản phẩm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý thay đổi filter ComboBox
        private async void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allProducts == null || allProducts.Count == 0) return;

            await ApplyCurrentFilterAsync(allProducts);
        }

        // Filter sorting
        private async Task ApplyCurrentFilterAsync(List<Product> products)
        {
            if (FilterComboBox.SelectedItem == null || products == null)
            {
                DisplayProducts(products);
                return;
            }

            var selectedFilter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            List<Product> sortedProducts = new List<Product>(products);

            switch (selectedFilter)
            {
                case "Giá thấp đến cao":
                    sortedProducts = products.OrderBy(p => p.FinalPrice).ToList();
                    break;

                case "Giá cao đến thấp":
                    sortedProducts = products.OrderByDescending(p => p.FinalPrice).ToList();
                    break;

                case "Đánh giá cao nhất":
                    // Sắp xếp theo rating từ database
                    sortedProducts = await SortByRatingAsync(products);
                    break;

                default: // Tất cả
                    sortedProducts = products;
                    break;
            }

            DisplayProducts(sortedProducts);
        }

        // Sắp xếp sản phẩm theo rating từ database
        private async Task<List<Product>> SortByRatingAsync(List<Product> products)
        {
            try
            {
                // Tạo dictionary để lưu rating của từng sản phẩm
                var productRatings = new Dictionary<string, double>();

                foreach (var product in products)
                {
                    double avgRating = await reviewService.GetAverageRatingAsync(product.ProductId);
                    productRatings[product.ProductId] = avgRating;
                }

                // Sắp xếp theo rating giảm dần
                return products.OrderByDescending(p => productRatings.ContainsKey(p.ProductId)
                    ? productRatings[p.ProductId]
                    : 0).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi sắp xếp theo rating: {ex.Message}");
                return products;
            }
        }

        // Xử lý tìm kiếm
        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Nếu xóa hết text, hiển thị lại products theo category hiện tại
                if (currentCategory == "Tất cả")
                {
                    allProducts = await productService.GetAllProductsAsync();
                }
                else
                {
                    allProducts = await productService.GetProductsByCategoryAsync(currentCategory);
                }
                await ApplyCurrentFilterAsync(allProducts);
                return;
            }

            // Tìm kiếm trong danh sách hiện tại (đã lọc theo category)
            var searchResults = allProducts.Where(p =>
                p.Name.ToLower().Contains(searchText.ToLower())).ToList();

            await ApplyCurrentFilterAsync(searchResults);
        }
    }
}