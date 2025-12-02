using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Pet_Shop_Project.Models;
using System.Threading;      // ✅ Để dùng User class



namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Page
    {
        private UserService userService;
        public SignIn()
        {
            InitializeComponent();
            userService = new UserService();
        }

        //xử lý nút đăng nhập
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin(); //tí nữa viết hàm đăng nhập - so sánh
        }


        //cho phép enter để đăng nhập
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

        //thực hiện đăng nhập
        private async void PerformLogin()
        {
            //dữ liệu từ form
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
                    MessageBox.Show($"Đăng nhập thành công!\nXin chào {user.FullName}", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                    var currWindow = Window.GetWindow(this);
                    MainWindow mainWindow = new MainWindow(user.UserId);
                    
                    mainWindow.Show();
                    
                    await Task.Delay(150);
                    
                    currWindow.Close();
                }
                else
                {
                    //sai thông tin đăng nhập
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

        //nút quên mật khẩu
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
            // Nếu có thông báo lỗi đang hiện, ẩn đi khi người dùng bắt đầu gõ lại
            if (ErrorMessage.Visibility == Visibility.Visible)
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
            }
        }



    }
}