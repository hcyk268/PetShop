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
        private UserService userService;
        private User currentUser;

        public AccountPage(string userId)
        {
            InitializeComponent();
            userService = new UserService();

            // 1. Dùng ID để query database (cần sửa UserService để chấp nhận string)
            this.currentUser = userService.GetUserById(userId);

            if (currentUser != null)
            {
                // 2. Load thông tin lên UI
                LoadUserInfo();
            }
            else
            {
                // Xử lý lỗi nếu không tìm thấy User (ID không hợp lệ)
                MessageBox.Show("Không tìm thấy thông tin tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.Navigate(new SignIn());
            }
        }
        public AccountPage()
        {
            InitializeComponent();
            userService = new UserService();
            Loaded += AccountPage_Loaded;
        }

        private void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem có user đang đăng nhập không
            if (!SessionManager.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem thông tin tài khoản!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Chuyển về trang đăng nhập
                NavigationService?.Navigate(new SignIn());
                return;
            }

            // Lấy thông tin user từ session
            currentUser = SessionManager.CurrentUser;

            // Load thông tin user lên UI
            LoadUserInfo();
        }

        /// <summary>
        /// Load thông tin user lên giao diện
        /// </summary>
        private void LoadUserInfo()
        {
            if (currentUser == null) return;

            try
            {
                // Hiển thị thông tin cơ bản
                UserNameText.Text = currentUser.FullName ?? "Người dùng";
                EmailText.Text = currentUser.Email ?? "Chưa cập nhật";

                // Hiển thị role
                RoleText.Text = GetRoleDisplayName(currentUser.Role);

                // Hiển thị số điện thoại
                PhoneText.Text = string.IsNullOrWhiteSpace(currentUser.Phone)
                    ? "Chưa cập nhật"
                    : currentUser.Phone;

                // Hiển thị địa chỉ
                AddressText.Text = string.IsNullOrWhiteSpace(currentUser.Address)
                    ? "Chưa cập nhật"
                    : currentUser.Address;

                // Hiển thị ngày tham gia
                JoinDateText.Text = currentUser.CreatedDate.ToString("dd/MM/yyyy");

                // Load avatar nếu có
                LoadAvatar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị thông tin: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Chuyển đổi role thành tên hiển thị
        /// </summary>
        private string GetRoleDisplayName(string role)
        {
            switch (role?.ToLower())
            {
                case "admin":
                    return "Quản trị viên";
                case "customer":
                    return "Khách hàng";
                case "staff":
                    return "Nhân viên";
                default:
                    return "Khách hàng";
            }
        }

        /// <summary>
        /// Load avatar từ database hoặc hiển thị avatar mặc định
        /// </summary>
        private void LoadAvatar()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(currentUser.AvatarPath) &&
                    File.Exists(currentUser.AvatarPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(currentUser.AvatarPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    AvatarBrush.ImageSource = bitmap;
                }
                else
                {
                    // Sử dụng avatar mặc định
                    AvatarBrush.ImageSource = new BitmapImage(
                        new Uri("pack://application:,,,/Images/default-avatar.png"));
                }
            }
            catch (Exception)
            {
                // Nếu có lỗi, dùng avatar mặc định
                AvatarBrush.ImageSource = new BitmapImage(
                    new Uri("pack://application:,,,/Images/default-avatar.png"));
            }
        }

        /// <summary>
        /// Thay đổi avatar
        /// </summary>
        private void ChangeAvatar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Chọn ảnh đại diện"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFile = openFileDialog.FileName;

                    // Tạo thư mục lưu avatar nếu chưa có
                    string avatarFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars");
                    if (!Directory.Exists(avatarFolder))
                    {
                        Directory.CreateDirectory(avatarFolder);
                    }

                    // Tạo tên file mới
                    string fileName = $"avatar_{currentUser.UserId}_{DateTime.Now:yyyyMMddHHmmss}" +
                        Path.GetExtension(selectedFile);
                    string newAvatarPath = Path.Combine(avatarFolder, fileName);

                    // Copy file vào thư mục Avatars
                    File.Copy(selectedFile, newAvatarPath, true);

                    // Cập nhật vào database
                    currentUser.AvatarPath = newAvatarPath;
                    bool success = userService.UpdateUser(currentUser);

                    if (success)
                    {
                        // Cập nhật session
                        SessionManager.UpdateUserInfo(currentUser);

                        // Hiển thị avatar mới
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(newAvatarPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        AvatarBrush.ImageSource = bitmap;

                        MessageBox.Show("Cập nhật ảnh đại diện thành công!",
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật ảnh đại diện!",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thay đổi avatar: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sửa số điện thoại
        /// </summary>
        private void EditPhone_Click(object sender, RoutedEventArgs e)
        {
            string newPhone = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập số điện thoại mới:",
                "Cập nhật số điện thoại",
                currentUser.Phone ?? "",
                -1, -1);

            if (!string.IsNullOrWhiteSpace(newPhone))
            {
                UpdateUserField("Phone", newPhone);
            }
        }

        /// <summary>
        /// Sửa địa chỉ
        /// </summary>
        private void EditAddress_Click(object sender, RoutedEventArgs e)
        {
            string newAddress = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập địa chỉ mới:",
                "Cập nhật địa chỉ",
                currentUser.Address ?? "",
                -1, -1);

            if (!string.IsNullOrWhiteSpace(newAddress))
            {
                UpdateUserField("Address", newAddress);
            }
        }

        /// <summary>
        /// Cập nhật thông tin user
        /// </summary>
        private void UpdateUserField(string fieldName, string newValue)
        {
            try
            {
                switch (fieldName)
                {
                    case "Phone":
                        currentUser.Phone = newValue;
                        break;
                    case "Address":
                        currentUser.Address = newValue;
                        break;
                }

                bool success = userService.UpdateUser(currentUser);

                if (success)
                {
                    // Cập nhật session
                    SessionManager.UpdateUserInfo(currentUser);

                    // Reload UI
                    LoadUserInfo();

                    MessageBox.Show("Cập nhật thành công!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật thông tin!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Có thể tạo một dialog riêng để đổi mật khẩu
            MessageBox.Show("Tính năng đổi mật khẩu đang được phát triển!",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO: Implement change password dialog
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc muốn đăng xuất?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Xóa session
                SessionManager.Logout();

                MessageBox.Show("Đã đăng xuất thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Chuyển về trang đăng nhập
                NavigationService?.Navigate(new SignIn());
            }
        }
    }
}