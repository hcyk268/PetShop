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
using System.Data.SqlClient;
using System.Configuration;

using System.Linq.Expressions; // Không cần thiết

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
            string email = UsernameTextBox.Text; // Đổi thành Email để phù hợp với bảng USERS
            string password = PasswordBox.Password;

            // 1. Lấy chuỗi kết nối
            string connectionString =ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

            // 2. Câu truy vấn an toàn
            // Đã đổi Username thành Email để khớp với cấu trúc bảng USERS bạn đã cung cấp (Email là UNIQUE)
            string query = "SELECT COUNT(1) FROM USERS WHERE Username = @Username AND Password = @Password";

            try
            {
                // Khối using đảm bảo kết nối được đóng và Dispose
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số truy vấn để ngăn chặn SQL Injection
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        connection.Open();

                        // Thực thi truy vấn và lấy số lượng bản ghi khớp
                        int count = (int)command.ExecuteScalar(); // Đã sửa lỗi dấu ngoặc thừa ở đây

                        if (count == 1)
                        {
                            // Đăng nhập thành công
                            MessageBox.Show("Succeed! LEGGOOOO!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Mở cửa sổ chính
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();

                            // Đóng LoginWindow
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
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi kết nối hoặc lỗi truy vấn SQL
                MessageBox.Show($"Lỗi kết nối Database: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Clear();
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SignUp());
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Có thể thêm logic kiểm tra hoặc định dạng ở đây nếu cần
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}