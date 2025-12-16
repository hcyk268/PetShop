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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Pet_Shop_Project.Services;
using System.Collections.ObjectModel;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for AdminProductEditDialog.xaml
    /// </summary>
    public partial class AdminProductEditDialog : Page
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        private Product _originalProduct;
        private readonly UploadImageService _uploadImageService = new UploadImageService();
        private bool _isEditMode;
        public Product Product { get; private set; }

        public AdminProductEditDialog(Product product)
        {
            InitializeComponent();
            _originalProduct = product;
            _isEditMode = product != null;
            Loaded += async (_, __) => { await LoadCategoriesAsync(); };
            
            if (_isEditMode)
            {
                DialogTitle.Text = "Chỉnh sửa sản phẩm";
                LoadProductData();
            }
            else
            {
                DialogTitle.Text = "Thêm sản phẩm mới";
            }
        }

        private void LoadProductData()
        {
            if (_originalProduct == null) return;
            TxtProductName.Text = _originalProduct.Name;
            TxtDescription.Text = _originalProduct.Description;
            TxtUnitPrice.Text = _originalProduct.UnitPrice.ToString();
            TxtDiscount.Text = _originalProduct.Discount.ToString();
            TxtUnitInStock.Text = _originalProduct.UnitInStock.ToString();
            TxtPicture.Text = _originalProduct.Picture.ToString();
           
            // Set category
            foreach (System.Windows.Controls.ComboBoxItem item in CmbCategory.Items)
            {
                if (item.Content.ToString() == _originalProduct.Category)
                {
                    CmbCategory.SelectedItem = item;
                    break;
                }
            }

            // Load image preview
            if (!string.IsNullOrEmpty(_originalProduct.Picture))
            {
                LoadImagePreview(_originalProduct.Picture);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var items = new ObservableCollection<string>();
            const string sql = "SELECT DISTINCT Category FROM PRODUCTS WHERE Category IS NOT NULL AND Category <> '' ORDER BY Category";
            using (var conn = new SqlConnection(_conn))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(reader["Category"].ToString());
                    }
                }
            }
            CmbCategory.ItemsSource = items;
            if (_originalProduct != null && !string.IsNullOrEmpty(_originalProduct.Category))
                CmbCategory.SelectedItem = _originalProduct.Category;
        }

        private async void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Chọn hình ảnh sản phẩm"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFilePath = openFileDialog.FileName;
                var (secureUrl, publicId) = await _uploadImageService.UploadAsync(selectedFilePath, "products");
                TxtPicture.Text = secureUrl;
                LoadImagePreview(secureUrl);
            }
        }

        private void LoadImagePreview(string imagePath)
        {
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ImgPreview.Source = bitmap;
                    ImagePreviewBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    // Try as URL
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.EndInit();

                    ImgPreview.Source = bitmap;
                    ImagePreviewBorder.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                ImagePreviewBorder.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;
            
            try
            {
                var selectedCategory = CmbCategory.SelectedItem as string ?? CmbCategory.Text;
                if (_isEditMode)
                {
                   // Update existing product
                    Product = new Product
                    {
                        Name = TxtProductName.Text.Trim(),
                        Description = TxtDescription.Text.Trim(),
                        UnitPrice = decimal.Parse(TxtUnitPrice.Text),
                        Discount = double.Parse(TxtDiscount.Text),
                        UnitInStock = int.Parse(TxtUnitInStock.Text),
                        Picture = TxtPicture.Text.Trim(),
                        Category = selectedCategory
                    };
                    if (await UpdateProductAsync(Product))
                    {
                        CloseDialog(true);
                    }
                }
                else
                {
                    // Create new product
                    Product = new Product
                    {
                        Name = TxtProductName.Text.Trim(),
                        Description = TxtDescription.Text.Trim(),
                        UnitPrice = decimal.Parse(TxtUnitPrice.Text),
                        Discount = double.Parse(TxtDiscount.Text),
                        UnitInStock = int.Parse(TxtUnitInStock.Text),
                        Picture = TxtPicture.Text.Trim(),
                        Category = selectedCategory
                    };

                    if (await AddProductAsync(Product))
                    {
                        CloseDialog(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> AddProductAsync(Product product)
        {
            const string sql = @"INSERT INTO PRODUCTS (Name, Description, UnitPrice, UnitInStock, Discount, Picture, Category)
                                 OUTPUT INSERTED.ProductId
                                 VALUES (@Name, @Description, @UnitPrice, @UnitInStock, @Discount, @Picture, @Category)";

            using (var conn = new SqlConnection(_conn))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", product.Name);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@UnitInStock", product.UnitInStock);
                    cmd.Parameters.AddWithValue("@Discount", product.Discount);
                    cmd.Parameters.AddWithValue("@Picture", product.Picture ?? "");
                    cmd.Parameters.AddWithValue("@Category", product.Category ?? "");

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {                
                    product.ProductId = result.ToString();
                    MessageBox.Show("Thêm sản phẩm thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                    }
                }
            }
            return false;
        }

        private async Task<bool> UpdateProductAsync(Product product)
        {
            const string sql = @"UPDATE PRODUCTS
                                SET Name = @Name,
                                    Description = @Description,
                                    UnitPrice = @UnitPrice,
                                    UnitInStock = @UnitInStock,
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
                        cmd.Parameters.AddWithValue("@ProductId", _originalProduct.ProductId);
                        cmd.Parameters.AddWithValue("@Name", product.Name);
                        cmd.Parameters.AddWithValue("@Description", product.Description);
                        cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                        cmd.Parameters.AddWithValue("@UnitInStock", product.UnitInStock);
                        cmd.Parameters.AddWithValue("@Discount", product.Discount);
                        cmd.Parameters.AddWithValue("@Picture", product.Picture ?? "");
                        cmd.Parameters.AddWithValue("@Category", product.Category ?? "");

                        int result = await cmd.ExecuteNonQueryAsync();
                        if (result > 0)
                        {
                            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật sản phẩm: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtProductName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtProductName.Focus();
                return false;
            }

            if (CmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbCategory.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtDiscount.Text) ||
              !double.TryParse(TxtDiscount.Text, out double discount) || discount < 0 || discount > 1)
            {
                MessageBox.Show("Giảm giá phải trong khoảng 0-1", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDiscount.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtUnitPrice.Text) ||
              !decimal.TryParse(TxtUnitPrice.Text, out decimal price) || price < 0)
            {
                 MessageBox.Show("Giá bán không hợp lệ", "Thông báo",
                 MessageBoxButton.OK, MessageBoxImage.Warning);
                 TxtUnitPrice.Focus();
                 return false;
            }

            if (string.IsNullOrWhiteSpace(TxtUnitInStock.Text) ||
             !int.TryParse(TxtUnitInStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Số lượng tồn kho không hợp lệ", "Thông báo",
                  MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUnitInStock.Focus();
                return false;
            }
            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(false);
        }

        //public event EventHandler<Product> Saved;
        //public event EventHandler Canceled;
        private void CloseDialog(bool success)
        {
            var host = Window.GetWindow(this);
            if (host != null)
            {
                host.DialogResult = success;
                host.Close();
            }
        }
    }
}