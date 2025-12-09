using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pet_Shop_Project.Views
{
    public partial class ChangePasswordDialog : Window
    {
        private string userId;
        private UserService userService;

        public ChangePasswordDialog(string userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.userService = new UserService();
            SaveButton.IsEnabled = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ClearErrors();

            // Show password strength if new password has content
            if (!string.IsNullOrEmpty(NewPasswordBox.Password))
            {
                CheckPasswordStrength(NewPasswordBox.Password);
            }
            else
            {
                PasswordStrengthPanel.Visibility = Visibility.Collapsed;
            }

            ValidateForm();
        }

        private void CheckPasswordStrength(string password)
        {
            PasswordStrengthPanel.Visibility = Visibility.Visible;
            int strength = 0;

            if (password.Length >= 6) strength += 25;
            if (password.Length >= 8) strength += 20;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]")) strength += 15;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]")) strength += 15;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]")) strength += 15;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[^a-zA-Z0-9]")) strength += 10;

            PasswordStrengthBar.Value = strength;

            if (strength < 40)
            {
                PasswordStrengthText.Text = "Yếu";
                PasswordStrengthBar.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }
            else if (strength < 70)
            {
                PasswordStrengthText.Text = "Trung bình";
                PasswordStrengthBar.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            }
            else
            {
                PasswordStrengthText.Text = "Mạnh";
                PasswordStrengthBar.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
        }

        private void ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(OldPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(NewPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                isValid = false;
            }
            else if (NewPasswordBox.Password.Length < 6)
            {
                ShowError(NewPasswordError, "Mật khẩu phải có ít nhất 6 ký tự");
                isValid = false;
            }
            else if (NewPasswordBox.Password == OldPasswordBox.Password)
            {
                ShowError(NewPasswordError, "Mật khẩu mới phải khác mật khẩu cũ");
                isValid = false;
            }
            else if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowError(ConfirmPasswordError, "Mật khẩu xác nhận không khớp");
                isValid = false;
            }

            SaveButton.IsEnabled = isValid;
        }

        private void ShowError(TextBlock errorBlock, string message)
        {
            errorBlock.Text = message;
            errorBlock.Visibility = Visibility.Visible;
        }

        private void ClearErrors()
        {
            OldPasswordError.Visibility = Visibility.Collapsed;
            NewPasswordError.Visibility = Visibility.Collapsed;
            ConfirmPasswordError.Visibility = Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Đang xử lý...";

                string oldPassword = OldPasswordBox.Password;
                string newPassword = NewPasswordBox.Password;

                bool success = userService.ChangePassword(userId, oldPassword, newPassword);

                if (success)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError(OldPasswordError, "Mật khẩu hiện tại không đúng");
                    SaveButton.IsEnabled = true;
                    SaveButton.Content = "Lưu thay đổi";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Lưu thay đổi";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}