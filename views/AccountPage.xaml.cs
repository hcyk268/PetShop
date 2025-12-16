using Microsoft.Win32;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Pet_Shop_Project.Views
{
    public partial class AccountPage : Page
    {
        private string currentUserId;
        private UserService userService;
        private User currentUser;
        private bool isEditMode = false;

        // Lưu giá trị ban đầu để có thể cancel
        private string originalEmail;
        private string originalAddress;
        private string originalPhone;
        private string originalFullName;

        public AccountPage(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            userService = new UserService();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            try
            {
                currentUser = userService.GetUserById(currentUserId);
                if (currentUser != null)
                {
                    UserNameText.Text = currentUser.FullName ?? "Người dùng";
                    EmailText.Text = currentUser.Email ?? "Đang cập nhật...";
                    PhoneText.Text = currentUser.Phone ?? "Đang cập nhật...";
                    AddressText.Text = currentUser.Address ?? "Đang cập nhật...";
                    FullNameText.Text = currentUser.FullName ?? "Đang cập nhật...";
                    RoleText.Text = GetRoleDisplayName(currentUser.Role);
                    JoinDateText.Text = currentUser.CreatedDate.ToString("dd/MM/yyyy");

                    // Lưu giá trị ban đầu
                    originalEmail = EmailText.Text;
                    originalAddress = AddressText.Text;
                    originalPhone = PhoneText.Text;
                    originalFullName = FullNameText.Text;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin tài khoản!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role?.ToLower())
            {
                case "admin":
                    return "Nhân viên";
                case "user":
                    return "Khách hàng";
                default:
                    return "Khách hàng";
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            isEditMode = true;
            SetEditMode(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    MessageBox.Show("Email không được để trống!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
                {
                    MessageBox.Show("Họ tên không được để trống!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cập nhật thông tin user
                currentUser.Email = EmailTextBox.Text.Trim();
                currentUser.Address = AddressTextBox.Text.Trim();
                currentUser.Phone = PhoneTextBox.Text.Trim();
                currentUser.FullName = FullNameTextBox.Text.Trim();

                // Gọi service để cập nhật vào database
                bool updateSuccess = userService.UpdateUser(currentUser);

                if (updateSuccess)
                {
                    // Cập nhật hiển thị
                    EmailText.Text = currentUser.Email;
                    AddressText.Text = currentUser.Address;
                    PhoneText.Text = currentUser.Phone;
                    FullNameText.Text = currentUser.FullName;
                    UserNameText.Text = currentUser.FullName;

                    // Cập nhật giá trị ban đầu
                    originalEmail = EmailText.Text;
                    originalAddress = AddressText.Text;
                    originalPhone = PhoneText.Text;
                    originalFullName = FullNameText.Text;

                    MessageBox.Show("Cập nhật thông tin thành công!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    isEditMode = false;
                    SetEditMode(false);
                }
                else
                {
                    MessageBox.Show("Cập nhật thông tin thất bại!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Khôi phục giá trị ban đầu
            EmailTextBox.Text = originalEmail;
            AddressTextBox.Text = originalAddress;
            PhoneTextBox.Text = originalPhone;
            FullNameTextBox.Text = originalFullName;

            isEditMode = false;
            SetEditMode(false);
        }

        private void SetEditMode(bool isEdit)
        {
            if (isEdit)
            {
                // Hiện nút Save và Cancel, ẩn nút Edit
                EditButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;

                // Hiện TextBox, ẩn TextBlock
                EmailDisplayBorder.Visibility = Visibility.Collapsed;
                EmailEditBorder.Visibility = Visibility.Visible;
                EmailTextBox.Text = EmailText.Text;

                AddressDisplayBorder.Visibility = Visibility.Collapsed;
                AddressEditBorder.Visibility = Visibility.Visible;
                AddressTextBox.Text = AddressText.Text;

                PhoneDisplayBorder.Visibility = Visibility.Collapsed;
                PhoneEditBorder.Visibility = Visibility.Visible;
                PhoneTextBox.Text = PhoneText.Text;

                FullNameDisplayBorder.Visibility = Visibility.Collapsed;
                FullNameEditBorder.Visibility = Visibility.Visible;
                FullNameTextBox.Text = FullNameText.Text;
            }
            else
            {
                // Hiện nút Edit, ẩn nút Save và Cancel
                EditButton.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;

                // Hiện TextBlock, ẩn TextBox
                EmailDisplayBorder.Visibility = Visibility.Visible;
                EmailEditBorder.Visibility = Visibility.Collapsed;

                AddressDisplayBorder.Visibility = Visibility.Visible;
                AddressEditBorder.Visibility = Visibility.Collapsed;

                PhoneDisplayBorder.Visibility = Visibility.Visible;
                PhoneEditBorder.Visibility = Visibility.Collapsed;

                FullNameDisplayBorder.Visibility = Visibility.Visible;
                FullNameEditBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeAvatar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Ảnh (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            openFileDialog.Title = "Chọn ảnh làm avatar";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedImagePath = openFileDialog.FileName;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedImagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                AvatarBrush.ImageSource = bitmap;
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordDialog dialog = new ChangePasswordDialog(currentUserId);
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Password đã được đổi thành công
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentUser = null;

                MessageBox.Show("Đăng xuất thành công!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Login loginWindow = new Login();
                loginWindow.Show();

                Window.GetWindow(this)?.Close();
            }
        }
    }
}