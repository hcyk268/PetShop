using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pet_Shop_Project.Views
{
    public partial class ChangeAddressDialog : Window
    {
        private string userId;
        private UserService userService;
        public string NewAddress { get; private set; }

        public ChangeAddressDialog(string userId, string currentAddress)
        {
            InitializeComponent();
            this.userId = userId;
            this.userService = new UserService();

            // Set current address
            AddressTextBox.Text = currentAddress ?? "";
            SaveButton.IsEnabled = false;
        }

        private void AddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddressError.Visibility = Visibility.Collapsed;
            ValidateForm();
        }

        private void ValidateForm()
        {
            string address = AddressTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(address))
            {
                SaveButton.IsEnabled = false;
            }
            else if (address.Length < 10)
            {
                ShowError("Địa chỉ quá ngắn, vui lòng nhập đầy đủ");
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            AddressError.Text = message;
            AddressError.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Đang lưu...";

                string newAddress = AddressTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(newAddress))
                {
                    ShowError("Vui lòng nhập địa chỉ");
                    SaveButton.IsEnabled = true;
                    SaveButton.Content = "Lưu thay đổi";
                    return;
                }

                if (newAddress.Length < 10)
                {
                    ShowError("Địa chỉ quá ngắn, vui lòng nhập đầy đủ");
                    SaveButton.IsEnabled = true;
                    SaveButton.Content = "Lưu thay đổi";
                    return;
                }

                // Get current user and update address
                User user = userService.GetUserById(userId);
                if (user != null)
                {
                    user.Address = newAddress;
                    bool success = userService.UpdateUser(user);

                    if (success)
                    {
                        NewAddress = newAddress;
                        MessageBox.Show("Cập nhật địa chỉ thành công!",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        ShowError("Không thể cập nhật địa chỉ");
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