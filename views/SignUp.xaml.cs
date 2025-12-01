using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System.Windows.Media;

namespace Pet_Shop_Project.Views
{
    public partial class SignUp : Page
    {
        private UserService userService;

        public SignUp()
        {
            InitializeComponent();
            userService = new UserService();
        }

        // Xử lý đăng ký
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSignUp();
        }

        // Cho phép Enter để submit
        private void ConfirmPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSignUp();
            }
        }

        // Thực hiện đăng ký
        private void PerformSignUp()
        {
            // Lấy dữ liệu từ form
            string fullName = FullNameTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            bool termsAccepted = TermsCheckBox.IsChecked == true;

            // Validate tất cả các trường
            if (!ValidateInput(fullName, username, email, phone, password, confirmPassword, termsAccepted))
            {
                return;
            }

            try
            {
                // Kiểm tra username đã tồn tại chưa
                if (userService.IsUsernameExists(username))
                {
                    ShowError("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                    UsernameTextBox.Focus();
                    return;
                }

                // Kiểm tra email đã tồn tại chưa
                if (userService.IsEmailExists(email))
                {
                    ShowError("Email đã được đăng ký. Vui lòng sử dụng email khác.");
                    EmailTextBox.Focus();
                    return;
                }

                // Tạo đối tượng User mới
                User newUser = new User
                {
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    Role = "User", // Mặc định là Customer

                    CreatedDate = DateTime.Now
                };

                // Gọi service để đăng ký (insert vào database với username)
                bool success = userService.RegisterUser(newUser, username, password);

                if (success)
                {
                    // Đăng ký thành công
                    ShowSuccess("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.");

                    // Đợi 2 giây rồi chuyển sang trang đăng nhập
                    System.Threading.Tasks.Task.Delay(2000).ContinueWith(t =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NavigateToSignIn();
                        });
                    });
                }
                else
                {
                    ShowError("Đăng ký thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi đăng ký: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SignUp error: {ex.Message}");
            }
        }

        // Validate tất cả input
        private bool ValidateInput(string fullName, string username, string email,
                                   string phone, string password, string confirmPassword,
                                   bool termsAccepted)
        {
            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError("Vui lòng nhập họ và tên");
                FullNameTextBox.Focus();
                return false;
            }

            if (fullName.Length < 2)
            {
                ShowError("Họ và tên phải có ít nhất 2 ký tự");
                FullNameTextBox.Focus();
                return false;
            }

            // Kiểm tra username
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Vui lòng nhập tên đăng nhập");
                UsernameTextBox.Focus();
                return false;
            }

            if (username.Length < 4)
            {
                ShowError("Tên đăng nhập phải có ít nhất 4 ký tự");
                UsernameTextBox.Focus();
                return false;
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                ShowError("Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới");
                UsernameTextBox.Focus();
                return false;
            }

            // Kiểm tra email
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Vui lòng nhập email");
                EmailTextBox.Focus();
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Email không hợp lệ");
                EmailTextBox.Focus();
                return false;
            }

            // Kiểm tra số điện thoại
            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowError("Vui lòng nhập số điện thoại");
                PhoneTextBox.Focus();
                return false;
            }

            if (!IsValidPhone(phone))
            {
                ShowError("Số điện thoại không hợp lệ (10-11 số)");
                PhoneTextBox.Focus();
                return false;
            }

            // Kiểm tra mật khẩu
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                PasswordBox.Focus();
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("Mật khẩu phải có ít nhất 6 ký tự");
                PasswordBox.Focus();
                return false;
            }

            // Kiểm tra xác nhận mật khẩu
            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Vui lòng xác nhận mật khẩu");
                ConfirmPasswordBox.Focus();
                return false;
            }

            if (password != confirmPassword)
            {
                ShowError("Mật khẩu xác nhận không khớp");
                ConfirmPasswordBox.Focus();
                return false;
            }

            // Kiểm tra điều khoản
            if (!termsAccepted)
            {
                ShowError("Bạn phải đồng ý với điều khoản dịch vụ");
                return false;
            }

            return true;
        }

        // Validate email
        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        // Validate phone
        private bool IsValidPhone(string phone)
        {
            // Chỉ chấp nhận số, 10-11 ký tự
            return Regex.IsMatch(phone, @"^[0-9]{10,11}$");
        }

        // Hiển thị thông báo lỗi
        private void ShowError(string message)
        {
            MessageText.Text = message;
            MessageBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // #FFEBEE
            MessageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 83, 80)); // #EF5350
            MessageText.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40)); // #C62828
            MessageBorder.Visibility = Visibility.Visible;
        }

        // Hiển thị thông báo thành công
        private void ShowSuccess(string message)
        {
            MessageText.Text = message;
            MessageBorder.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // #E8F5E9
            MessageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4CAF50
            MessageText.Foreground = new SolidColorBrush(Color.FromRgb(27, 94, 32)); // #1B5E20
            MessageBorder.Visibility = Visibility.Visible;
        }

        // Chuyển sang trang đăng nhập
        private void SignInLink_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSignIn();
        }

        private void NavigateToSignIn()
        {
            try
            {
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new SignIn());
                }
                else
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.MainScreen?.Navigate(new SignIn());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở trang đăng nhập: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}