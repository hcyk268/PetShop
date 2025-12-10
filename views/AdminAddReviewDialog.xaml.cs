using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;

namespace Pet_Shop_Project.Views
{
    public partial class AdminAddReviewDialog : Window
    {
        private readonly ReviewService _reviewService = new ReviewService();
        private readonly UserService _userService = new UserService();

        public AdminAddReviewDialog()
        {
            InitializeComponent();
            Loaded += Dialog_Loaded;
        }

        private async void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var products = await _reviewService.GetAllProductsForSelectionAsync();
                CmbProduct.ItemsSource = products;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private int GetRating()
        {
            if (Rb1Star.IsChecked == true) return 1;
            if (Rb2Star.IsChecked == true) return 2;
            if (Rb3Star.IsChecked == true) return 3;
            if (Rb4Star.IsChecked == true) return 4;
            return 5;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Chọn sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtUserName.Text))
            {
                MessageBox.Show("Nhập tên người đánh giá!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtComment.Text))
            {
                MessageBox.Show("Nhập nội dung!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = CmbProduct.SelectedItem as Product;
            string displayName = TxtUserName.Text.Trim();
            string userId;

            try
            {
                userId = await _userService.AddVirtualUser(displayName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không tạo được user ảo: {ex.Message}");
                return;
            }

            bool success = await _reviewService.AddReviewAsync(
                product.ProductId,
                userId,
                GetRating(),
                TxtComment.Text.Trim()
            );

            if (success)
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
