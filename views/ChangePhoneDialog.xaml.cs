using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pet_Shop_Project.Views
{
    public partial class ChangePhoneDialog : Window
    {
        private string userId;
        private UserService userService;
        public string NewPhone { get; private set; }

        public ChangePhoneDialog(string userId, string currentPhone)
        {
            InitializeComponent();
            this.userId = userId;
            this.userService = new UserService();

            // Set current phone
            PhoneTextBox.Text = currentPhone ?? "";
            SaveButton.IsEnabled = false;
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numbers
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhoneError.Visibility = Visibility.Collapsed;
            ValidateForm();
        }

        private void ValidateForm()
        {
            string phone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(phone))
            {
                SaveButton.IsEnabled = false;
            }
            else if (!phone.StartsWith("0"))
            {
                ShowError("Số điện thoại phải bắt đầu bằng số 0");
                SaveButton.IsEnabled = false;
            }
            else if (phone.Length != 10)
            {
                ShowError("Số điện thoại phải có đúng 10 số");
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            PhoneError.Text = message;
            PhoneError.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Đang lưu...";

                string newPhone = PhoneTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(newPhone))
                {
                    ShowError("Vui lòng nhập số điện thoại");
                    SaveButton.IsEnabled = true;
                    SaveButton.Content = "Lưu thay đổi";
                    return;
                }

                if (!newPhone.StartsWith("0") || newPhone.Length != 10)
                {
                    ShowError("Số điện thoại không hợp lệ");
                    SaveButton.IsEnabled = true;
                    SaveButton.Content = "Lưu thay đổi";
                    return;
                }

                // Get current user and update phone
                User user = userService.GetUserById(userId);
                if (user != null)
                {
                    user.Phone = newPhone;
                    bool success = userService.UpdateUser(user);

                    if (success)
                    {
                        NewPhone = newPhone;
                        MessageBox.Show("Cập nhật số điện thoại thành công!",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        ShowError("Không thể cập nhật số điện thoại");
                        SaveButton.IsEnabled = true;
                        SaveButton.Content = "Lưu thay đổi";
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin người dùng!",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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