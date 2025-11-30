using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;


namespace Pet_Shop_Project.Views
{
    public partial class SignIn : Page
    {
        private UserService userService;
        private MainWindow mainWindow;

        public SignIn()
        {
            InitializeComponent();
            userService = new UserService();

            // Lấy reference đến MainWindow
            mainWindow = Application.Current.MainWindow as MainWindow;

            // Load trạng thái "Ghi nhớ tôi" nếu có
            LoadRememberedUser();
        }

        // Load thông tin user đã lưu (nếu có)
        private void LoadRememberedUser()
        {
            try
            {
                string savedUsername = Properties.Settings.Default.SavedUsername;
                bool rememberMe = Properties.Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername))
                {
                    UsernameTextBox.Text = savedUsername;
                    RememberMeCheckBox.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load remembered user: {ex.Message}");
            }
        }

        // Xử lý đăng nhập
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        // Cho phép Enter để đăng nhập
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

        // Thực hiện đăng nhập
        private void PerformLogin()
        {
            // Validate input
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Xóa thông báo lỗi cũ
            ErrorMessage.Visibility = Visibility.Collapsed;
            var errorTextBlock = ErrorMessage.Child as TextBlock;
            if (errorTextBlock != null)
            {
                errorTextBlock.Text = string.Empty;
            }
            ErrorMessage.Visibility = Visibility.Collapsed;

            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Vui lòng nhập tên đăng nhập");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                PasswordBox.Focus();
                return;
            }

            try
            {
                // Gọi service để xác thực
                User authenticatedUser = userService.AuthenticateUser(username, password);

                if (authenticatedUser != null)
                {
                    // Đăng nhập thành công
                    HandleSuccessfulLogin(authenticatedUser);
                }
                else
                {
                    // Sai thông tin đăng nhập
                    ShowError("Tên đăng nhập hoặc mật khẩu không đúng");
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi đăng nhập: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
        }

        // Xử lý sau khi đăng nhập thành công
        // File: SignIn.xaml.cs (phương thức HandleSuccessfulLogin)

        // ... (các phần trước)

        // Xử lý sau khi đăng nhập thành công
        private void HandleSuccessfulLogin(User user)
        {
            try
            {
                // Lưu thông tin user vào session
                SessionManager.CurrentUser = user;

                // ... (Xử lý Ghi nhớ tôi)
                // ... (Hiển thị thông báo)

                // Chuyển đến trang Account
                if (mainWindow != null)
                {
                    // 💡 SỬA: Dùng constructor mới AccountPage(string userId)
                    // và truyền UserId thực (giả sử UserId là string)
                    mainWindow.MainScreen.Navigate(new AccountPage(user.UserId.ToString()));
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi xử lý đăng nhập: {ex.Message}");
            }
        }
        // ...

        // Hiển thị thông báo lỗi
        private void ShowError(string message)
        {
            // Cast Border.Child thành TextBlock
            if (ErrorMessage.Child is TextBlock textBlock)
            {
                textBlock.Text = message;
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        // Chuyển sang trang đăng ký (nếu có)
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigate(new SignUp());
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.MainScreen.Navigate(new SignUp());
            }
        }

        // Quên mật khẩu
        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ quản trị viên để lấy lại mật khẩu",
                "Quên mật khẩu",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Quay lại trang chủ
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow?.MainScreen.Navigate(new HomePage());
        }
    }
}