using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pet_Shop_Project.Models;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Views
{
    public partial class SignIn : Page
    {
        private UserService userService;
        private bool isPasswordVisible = false;

        public SignIn()
        {
            InitializeComponent();
            userService = new UserService();
        }

        // Toggle Password Visibility
        private void TogglePassword_Click(object sender, MouseButtonEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Show password as text
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
                PasswordTextBox.Focus();
                PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;
            }
            else
            {
                // Hide password
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
                PasswordBox.Focus();
            }
        }

        // Sync PasswordBox with TextBox - Hide error when typing
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HideError();
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideError();
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideError();
        }

        // Helper method to hide error
        private void HideError()
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
            }
        }

        // Xử lý nút đăng nhập
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        // Cho phép enter để đăng nhập
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (isPasswordVisible)
                {
                    PasswordTextBox.Focus();
                }
                else
                {
                    PasswordBox.Focus();
                }
            }
        }

        // Nút quay về SignUp
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }

        // Nút đóng cửa sổ
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        // Thực hiện đăng nhập
        private async void PerformLogin()
        {
            // Ẩn error message cũ trước
            ErrorMessage.Visibility = Visibility.Collapsed;

            string username = UsernameTextBox.Text.Trim();
            string password = isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Vui lòng nhập tên đăng nhập");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                if (isPasswordVisible)
                    PasswordTextBox.Focus();
                else
                    PasswordBox.Focus();
                return;
            }

            try
            {
                User user = userService.AuthenticateUser(username, password);

                if (user != null)
                {
                    // Ẩn error message nếu đăng nhập thành công
                    ErrorMessage.Visibility = Visibility.Collapsed;

                    MessageBox.Show($"Đăng nhập thành công!\nXin chào {user.FullName}",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    var currWindow = Window.GetWindow(this);

                    if (user.Role.Equals("Admin", StringComparison.Ordinal))
                    {
                        AdminWindow adminWindow = new AdminWindow();
                        adminWindow.Show();
                    }
                    else if (user.Role.Equals("User", StringComparison.Ordinal))
                    {
                        MainWindow mainWindow = new MainWindow(user.UserId);
                        mainWindow.Show();
                    }

                    await Task.Delay(150);
                    currWindow.Close();
                }
                else
                {
                    // Hiển thị error message - PASSWORD VẪN ĐƯỢC GIỮ NGUYÊN
                    ShowError("Tên đăng nhập hoặc mật khẩu không đúng");

                    // Focus vào username để người dùng kiểm tra lại
                    UsernameTextBox.Focus();
                    UsernameTextBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi đăng nhập: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            try
            {
                ErrorMessageText.Text = message;
                ErrorMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowError: {ex.Message}");
                MessageBox.Show($"Lỗi hiển thị thông báo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nút quên mật khẩu
        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ quản trị viên để lấy lại mật khẩu",
                "Quên mật khẩu",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Chuyển sang trang đăng ký
        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }
    }
}