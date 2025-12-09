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
        public AccountPage(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            userService = new UserService();
            //load thông tin user từ database
            LoadUserInfo(); //viết hàm bổ sung
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
                    RoleText.Text = GetRoleDisplayName(currentUser.Role); //viết hàm bổ sung
                    JoinDateText.Text = DateTime.Now.ToString("dd/MM/yyyy");
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
        //chuyển role,
        private string GetRoleDisplayName(string role)
        {
            switch (role?.ToLower())
            {
                case "Admin":
                    return "Nhân viên";
                case "User":
                    return "Khách hàng";
                default:
                    return "Khách hàng";
            }
        }
        private void ChangeAvatar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Tạo hộp thoại chọn file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Ảnh (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            openFileDialog.Title = "Chọn ảnh làm avatar";
            if (openFileDialog.ShowDialog() == true)
            {
                // Lấy đường dẫn ảnh
                string selectedImagePath = openFileDialog.FileName;
                // Hiện ảnh lên UI
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedImagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                AvatarBrush.ImageSource = bitmap;
            }
        }
        // Sửa số điện thoại
        private void EditPhone_Click(object sender, RoutedEventArgs e)
        {
            ChangePhoneDialog dialog = new ChangePhoneDialog(currentUserId, PhoneText.Text);
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Cập nhật hiển thị số điện thoại mới
                PhoneText.Text = dialog.NewPhone;
            }
        }
        private void EditAddress_Click(object sender, RoutedEventArgs e)
        {
            ChangeAddressDialog dialog = new ChangeAddressDialog(currentUserId, AddressText.Text);
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Cập nhật hiển thị địa chỉ mới
                AddressText.Text = dialog.NewAddress;
            }
        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Mở dialog đổi mật khẩu
            ChangePasswordDialog dialog = new ChangePasswordDialog(currentUserId);
            bool? result = dialog.ShowDialog();

            // Nếu đổi mật khẩu thành công
            if (result == true)
            {
                // Password đã được đổi thành công  
                // Có thể refresh lại thông tin nếu cần
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

                Window1 loginWindow = new Window1();
                loginWindow.Show();

                Window.GetWindow(this)?.Close();
            }
        }
    }
}