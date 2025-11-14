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
using System.Windows.Shapes;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class Window1: Window // Đã đổi tên lớp từ Window1 thành LoginWindow
    {
        public Window1()
        {
            InitializeComponent();
        }

        // Thêm phương thức xử lý sự kiện Click cho nút ĐĂNG NHẬP
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin từ các trường nhập liệu (đã đặt x:Name="txtUsername" và x:Name="txtPassword" trong XAML)
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // TODO: Thay thế bằng logic kiểm tra đăng nhập thực tế của bạn
            if (username == "admin" && password == "123")
            {
                // Đăng nhập thành công
                MessageBox.Show("Succeed! LEGGOOOO!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ví dụ: Mở cửa sổ chính và đóng cửa sổ đăng nhập
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                // Đăng nhập thất bại
                MessageBox.Show("Oops! Username and Password doesn't match!", "Notification", MessageBoxButton.OK, MessageBoxImage.Error);

                // Xóa mật khẩu để người dùng nhập lại
                txtPassword.Clear();
            }
        }
    }
}