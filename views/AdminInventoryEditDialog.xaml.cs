using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for AdminInventoryEditDialog.xaml
    /// </summary>
    public partial class AdminInventoryEditDialog : Window
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        private InventoryItem _originalItem;
        private bool _isEditMode;
        private Product _loadedProduct;

        public InventoryItem InventoryItem { get; private set; }

        public AdminInventoryEditDialog()
        {
            InitializeComponent();
            Loaded += AdminInventoryEditDialog_Loaded;
        }
        public AdminInventoryEditDialog(InventoryItem item)
        {
            InitializeComponent();
            _originalItem = item;
            _isEditMode = item != null;

            Loaded += AdminInventoryEditDialog_Loaded;
        }

        private async void AdminInventoryEditDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AdminInventoryEditDialog_Loaded;

            if (_isEditMode)
            {
                DialogTitle.Text = "Chỉnh sửa thông tin kho";
                ProductSelectionPanel.Visibility = Visibility.Collapsed;
                await LoadItemDataAsync();
            }
            else
            {
                DialogTitle.Text = "Thêm sản phẩm vào kho";
                ProductSelectionPanel.Visibility = Visibility.Visible;
                await LoadProductsAsync();
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = new List<Product>();
                const string sql = "SELECT ProductId, Name FROM PRODUCTS ORDER BY Name";

                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader["ProductId"].ToString(),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }

                ProductComboBox.ItemsSource = products;
                if (products.Count > 0)
                {
                    ProductComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách sản phẩm: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCategoriesAsycn(string selectedCategory = null)
        {
            var items = new List<string>();
            const string sql = "SELECT DISTINCT Category FROM PRODUCTS WHERE Category IS NOT NULL AND Category <> '' ORDER BY Category";
            using (var conn = new SqlConnection(_conn))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        items.Add(reader["Category"].ToString());
                    }
                }
            }
            CmbCategory.ItemsSource = items;
            if (!string.IsNullOrWhiteSpace(selectedCategory))
            {
                CmbCategory.SelectedItem = items
                    .FirstOrDefault(c => string.Equals(c, selectedCategory, StringComparison.OrdinalIgnoreCase));
            }
        }


        private async Task LoadItemDataAsync()
        {
            if (_originalItem == null) return;

            TxtProductCode.Text = _originalItem.ProductCode;
            TxtProductName.Text = _originalItem.ProductName;
            TxtStockQuantity.Text = _originalItem.StockQuantity.ToString();

            var product = _originalItem.Product ?? await LoadProductDetailsAsync(_originalItem.ProductId);
            if (product != null)
            {
                _loadedProduct = product;
                TxtDescription.Text = product.Description;
                TxtUnitPrice.Text = product.UnitPrice.ToString();
                TxtDiscount.Text = product.Discount.ToString();
                TxtPicture.Text = product.Picture;
                LoadImagePreview(product.Picture);
                await LoadCategoriesAsycn(product.Category);
            }
        }

        private void SetCategorySelection(string category)
        {
            if (CmbCategory.ItemsSource is IEnumerable<string> cats)
            {
                CmbCategory.SelectedItem = cats.FirstOrDefault(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
            }
        }


        private async void ProductComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is Product product)
            {
                TxtProductCode.Text = product.ProductId;
                TxtProductName.Text = product.Name;
                var fullProduct = await LoadProductDetailsAsync(product.ProductId);
                if (fullProduct != null)
                {
                    _loadedProduct = fullProduct;
                    TxtDescription.Text = fullProduct.Description;
                    TxtUnitPrice.Text = fullProduct.UnitPrice.ToString();
                    TxtStockQuantity.Text = fullProduct.UnitInStock.ToString();
                    TxtDiscount.Text = fullProduct.Discount.ToString();
                    TxtPicture.Text = fullProduct.Picture;
                    await LoadCategoriesAsycn(fullProduct.Category);
                    LoadImagePreview(fullProduct.Picture);
                }
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Chọn hình ảnh"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TxtPicture.Text = openFileDialog.FileName;
                LoadImagePreview(openFileDialog.FileName);
            }
        }

        private void LoadImagePreview(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    ImagePreviewBorder.Visibility = Visibility.Collapsed;
                    return;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ImgPreview.Source = bitmap;
                ImagePreviewBorder.Visibility = Visibility.Visible;
            }
            catch
            {
                ImagePreviewBorder.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnCreateNewProduct_Click(object sender, RoutedEventArgs e)
        {
            var page = new AdminProductEditDialog(null);
            var win = new Window
            {
                Owner = this,
                Content = page,
                Width = 500,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Title = "Tạo sản phẩm mới"
            };

            if (win.ShowDialog() == true)
            {
                MessageBox.Show("Sản phẩm mới đã được tạo thành công!\nVui lòng chọn sản phẩm từ danh sách để thêm vào kho.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadProductsAsync();

                if (page.Product != null)
                {
                    var newProduct = ProductComboBox.Items.Cast<Product>()
                        .FirstOrDefault(p => p.ProductId == page.Product.ProductId);
                    if (newProduct != null)
                    {
                        ProductComboBox.SelectedItem = newProduct;
                    }
                }
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var category = CmbCategory.SelectedItem as string ?? CmbCategory.Text;
                double.TryParse(TxtDiscount.Text, out var discount);
                var product = new Product
                {
                    ProductId = TxtProductCode.Text.Trim(),
                    Name = TxtProductName.Text.Trim(),
                    Description = TxtDescription.Text.Trim(),
                    UnitPrice = decimal.Parse(TxtUnitPrice.Text),
                    UnitInStock = int.Parse(TxtStockQuantity.Text),
                    Picture = TxtPicture.Text.Trim(),
                    Category = CmbCategory.SelectedItem as string ?? CmbCategory.Text,
                    Discount = discount
                };

                InventoryItem = new InventoryItem
                {
                    InventoryId = _isEditMode ? _originalItem.InventoryId : product.ProductId,
                    ProductId = product.ProductId,
                    ProductCode = product.ProductId,
                    ProductName = product.Name,
                    Category = category,
                    SellingPrice = product.UnitPrice,
                    CostPrice = product.UnitPrice * (decimal)(1 - product.Discount),
                    StockQuantity = product.UnitInStock,
                    LastUpdated = DateTime.Now,
                    Product = product
                };

                bool saved = _isEditMode
                    ? await UpdateInventoryItemAsync(InventoryItem)
                    : await AddInventoryItemAsync(InventoryItem);

                if (saved)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Product> LoadProductDetailsAsync(string productId)
        {
            const string sql = @"
                SELECT ProductId, Name, Description, UnitPrice, UnitInStock, 
                       Discount, Picture, Category
                FROM PRODUCTS 
                WHERE ProductId = @ProductId";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Product
                                {
                                    ProductId = reader["ProductId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                    UnitInStock = (int)reader["UnitInStock"],
                                    Discount = (double)reader["Discount"],
                                    Picture = reader["Picture"].ToString(),
                                    Category = reader["Category"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin sản phẩm: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        
        private async Task<bool> AddInventoryItemAsync(InventoryItem item)
        {
            const string sql = @"UPDATE PRODUCTS 
                                SET Name = @Name,
                                    Description = @Description,
                                    Discount = @Discount,
                                    UnitPrice = @UnitPrice,
                                    UnitInStock = @StockQuantity,
                                    Picture = @Picture,
                                    Category = @Category
                                WHERE ProductId = @ProductId";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();

                    using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM PRODUCTS WHERE ProductId = @ProductId", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            MessageBox.Show("Sản phẩm không tồn tại trong danh sách!", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Name", item.ProductName);
                        cmd.Parameters.AddWithValue("@Description", item.Product?.Description ?? "");
                        cmd.Parameters.AddWithValue("@UnitPrice", item.SellingPrice);
                        cmd.Parameters.AddWithValue("@StockQuantity", item.StockQuantity);
                        cmd.Parameters.AddWithValue("@Discount", item.Product?.Discount ?? 0d);
                        cmd.Parameters.AddWithValue("@Picture", item.Product?.Picture ?? "");
                        cmd.Parameters.AddWithValue("@Category", item.Category ?? "");

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Thêm sản phẩm vào kho thành công!", "Thành công",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm vào kho: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private async Task<bool> UpdateInventoryItemAsync(InventoryItem item)
        {
            const string sql = @"UPDATE PRODUCTS 
                                SET Description = @Description,
                                    UnitPrice = @UnitPrice,
                                    UnitInStock = @StockQuantity,
                                    Discount = @Discount,
                                    Picture = @Picture,
                                    Category = @Category
                                WHERE ProductId = @ProductId";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Description", item.Product?.Description ?? "");
                        cmd.Parameters.AddWithValue("@Discount", item.Product?.Discount ?? 0d);
                        cmd.Parameters.AddWithValue("@UnitPrice", item.SellingPrice);
                        cmd.Parameters.AddWithValue("@StockQuantity", item.StockQuantity);
                        cmd.Parameters.AddWithValue("@Picture", item.Product?.Picture ?? "");
                        cmd.Parameters.AddWithValue("@Category", item.Category ?? "");

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Cập nhật thông tin kho thành công!", "Thành công",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật kho: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtStockQuantity.Text) ||
                !int.TryParse(TxtStockQuantity.Text, out int stockQty) || stockQty < 0)
            {
                MessageBox.Show("Số lượng tồn kho không hợp lệ", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtStockQuantity.Focus();
                return false;
            }
            return true;
        }



        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
