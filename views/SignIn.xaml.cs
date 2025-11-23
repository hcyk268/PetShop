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
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Page
    {
        public SignIn()
        {
            InitializeComponent();
        }

        // Phương thức xử lý sự kiện Click cho nút ĐĂNG NHẬP
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin từ các trường nhập liệu
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // TODO: Thay thế bằng logic kiểm tra đăng nhập thực tế của bạn
            if (username == "admin" && password == "123")
            {
                // Đăng nhập thành công
                MessageBox.Show("Succeed! LEGGOOOO!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                // Mở cửa sổ chính và đóng cửa sổ đăng nhập
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Đóng LoginWindow (lấy Window chứa Page này)
                Window.GetWindow(this)?.Close();
            }
            else
            {
                // Đăng nhập thất bại
                MessageBox.Show("Oops! Username and Password doesn't match!", "Notification", MessageBoxButton.OK, MessageBoxImage.Error);

                // Xóa mật khẩu để người dùng nhập lại
                PasswordBox.Clear();
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SignUp());
        }
    }
}