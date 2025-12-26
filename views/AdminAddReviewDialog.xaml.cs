using MaterialDesignThemes.Wpf;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Pet_Shop_Project.Views
{
        public partial class AdminAddReviewDialog : Window
    {
        private readonly ReviewService _reviewService = new ReviewService();
        private readonly UserService _userService = new UserService();
        // Mac dinh chon 5 sao
        private int _selectedRating = 5;
        private Button[] _starButtons;

        public AdminAddReviewDialog()
        {
            InitializeComponent();
            Loaded += Dialog_Loaded;
        }

        private async void Dialog_Loaded(object sender, RoutedEventArgs e)
        {

            InitializeStars();

            try
            {
                var products = await _reviewService.GetAllProductsForSelectionAsync();
                ProductComboBox.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải sản phẩm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Khoi tao cac nut sao va gan su kien tuong tac
        private void InitializeStars()
        {
            _starButtons = new Button[5];

            for (int i = 0; i < 5; i++)
            {
                int starIndex = i + 1;

                var button = new Button
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(6),
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
                    Kind = PackIconKind.Star,
                    Width = 28,
                    Height = 28,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xD9, 0x78)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                button.Content = icon;
                button.Click += StarButton_Click;
                button.MouseEnter += StarButton_MouseEnter;
                button.MouseLeave += StarButton_MouseLeave;

                StarStackPanel.Children.Add(button);
                _starButtons[i] = button;
            }


            UpdateStarDisplay(_selectedRating);
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

        // Cap nhat hien thi sao theo rating hien tai/dang hover
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

        // An/hien placeholder theo noi dung comment
        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            CommentPlaceholder.Visibility = string.IsNullOrEmpty(CommentTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Kiem tra du lieu va luu danh gia
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập tên người đánh giá!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung đánh giá!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (_selectedRating == 0)
            {
                MessageBox.Show("Vui lòng chọn số sao đánh giá!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = ProductComboBox.SelectedItem as Product;
            string displayName = UserNameTextBox.Text.Trim();
            string userId;


            // Tranh submit nhieu lan
            SaveButton.IsEnabled = false;

            try
            {

                userId = await _userService.AddVirtualUser(displayName);


                bool success = await _reviewService.AddReviewAsync(
                    product.ProductId,
                    userId,
                    _selectedRating,
                    CommentTextBox.Text.Trim()
                );

                if (success)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi thêm đánh giá!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    SaveButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SaveButton.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
