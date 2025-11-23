using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Pet_Shop_Project.Views
{
    public partial class SignUp : Page
    {
        public SignUp()
        {
            InitializeComponent();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            PerformSignUp();
        }

        private void ConfirmPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Cho phép nhấn Enter để đăng ký
            if (e.Key == Key.Enter)
            {
                PerformSignUp();
            }
        }

        private void PerformSignUp()
        {
            // Ẩn thông báo lỗi cũ
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;

            // Lấy dữ liệu từ form
            string fullName = FullNameTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validation
            if (string.IsNullOrEmpty(fullName))
            {
                ShowError("Please enter your full name");
                FullNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Please enter username");
                UsernameTextBox.Focus();
                return;
            }

            if (username.Length < 4)
            {
                ShowError("Username must be at least 4 characters");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter email");
                EmailTextBox.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                EmailTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(phone))
            {
                ShowError("Please enter phone number");
                PhoneTextBox.Focus();
                return;
            }

            if (!IsValidPhone(phone))
            {
                ShowError("Please enter a valid phone number");
                PhoneTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter password");
                PasswordBox.Focus();
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                PasswordBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Please confirm your password");
                ConfirmPasswordBox.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                ConfirmPasswordBox.Focus();
                return;
            }

            if (AgreeTermsCheckBox.IsChecked != true)
            {
                ShowError("You must agree to the Terms and Conditions");
                return;
            }

            // TODO: Thực hiện logic đăng ký
            bool registrationSuccess = RegisterUser(fullName, username, email, phone, password);

            if (registrationSuccess)
            {
                MessageBox.Show("Registration successful! Please sign in.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Chuyển về trang đăng nhập
                NavigateToSignIn();
            }
            else
            {
                ShowError("Registration failed. Username or email may already exist.");
            }
        }

        private bool RegisterUser(string fullName, string username, string email, string phone, string password)
        {
            // TODO: Implement registration logic
            // - Kiểm tra username/email đã tồn tại chưa
            // - Hash password
            // - Lưu vào database
            // Tạm thời return true để test
            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Chấp nhận số điện thoại 10-11 số, có thể có dấu cách, dấu gạch ngang
            string cleanPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");
            return Regex.IsMatch(cleanPhone, @"^\d{10,11}$");
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSignIn();
        }

        private void NavigateToSignIn()
        {
            // TODO: Navigate to SignIn page
            NavigationService?.Navigate(new SignIn());
        }

        private void Terms_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Hiển thị Terms and Conditions
            MessageBox.Show("Terms and Conditions content will be displayed here.",
                "Terms and Conditions",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}