using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Pet_Shop_Project.Views
{
    public partial class UserDetailDialog : Window
    {
        private UserService userService;
        public User CurrentUser { get; private set; }
        public bool ShouldEdit { get; private set; } = false;

        public UserDetailDialog(string userId)
        {
            InitializeComponent();
            userService = new UserService();
            LoadUserFromDatabase(userId);
        }

        // Constructor nhận User object trực tiếp (nếu đã có sẵn)
        public UserDetailDialog(User user) : this()
        {
            CurrentUser = user;
            LoadUserData();
        }

        // Constructor mặc định
        private UserDetailDialog()
        {
            InitializeComponent();
            userService = new UserService();
        }

        // Load user từ database theo UserId
        private void LoadUserFromDatabase(string userId)
        {
            try
            {
                CurrentUser = userService.GetUserById(userId);

                if (CurrentUser != null)
                {
                    LoadUserData();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin người dùng!",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadUserData()
        {
            if (CurrentUser != null)
            {
                // Avatar - lấy chữ cái đầu của tên
                string firstLetter = string.IsNullOrEmpty(CurrentUser.FullName)
                    ? "U"
                    : CurrentUser.FullName.Trim()[0].ToString().ToUpper();
                AvatarTextBlock.Text = firstLetter;

                // Basic Info
                FullNameTextBlock.Text = CurrentUser.FullName ?? "Chưa có tên";
                UserIdTextBlock.Text = CurrentUser.UserId ?? "N/A";
                EmailTextBlock.Text = CurrentUser.Email ?? "Chưa có email";
                PhoneTextBlock.Text = string.IsNullOrWhiteSpace(CurrentUser.Phone)
                    ? "Chưa có số điện thoại"
                    : CurrentUser.Phone;
                AddressTextBlock.Text = string.IsNullOrWhiteSpace(CurrentUser.Address)
                    ? "Chưa có địa chỉ"
                    : CurrentUser.Address;

                // Role Badge
                SetRoleBadge(CurrentUser.Role);
            }
        }

        private void SetRoleBadge(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                role = "User";
            }

            RoleTextBlock.Text = role;

            // Đổi màu badge theo role
            switch (role.ToLower())
            {
                case "admin":
                    RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")); // Đỏ
                    break;
                
                case "user":
                default:
                    RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); // Xanh lá
                    break;
            }
        }

        // Click vào backdrop để đóng dialog
        private void Backdrop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                DialogResult = false;
                Close();
            }
        }

        // Nút Chỉnh sửa - đóng dialog này và báo cho parent mở dialog Edit
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldEdit = true;
            DialogResult = true;
            Close();
        }

        // Nút Đóng
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}