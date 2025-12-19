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

        public SignIn()
        {
            InitializeComponent();
            userService = new UserService();
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
                PasswordBox.Focus();
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
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Vui lòng nhập tên đăng nhập");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                return;
            }

            try
            {
                User user = userService.AuthenticateUser(username, password);

                if (user != null)
                {
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
                    ShowError("Tên đăng nhập hoặc mật khẩu không đúng");
                    PasswordBox.Clear();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi đăng nhập: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            var textBlock = ErrorMessage.Child as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = message;
            }
            ErrorMessage.Visibility = Visibility.Visible;
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

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
            }
        }
    }
}