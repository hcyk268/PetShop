using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace Pet_Shop_Project.Views
{
    public partial class DeleteUserDialog : Window
    {
        private UserService userService;
        public User UserToDelete { get; private set; }
        public bool DeleteSuccess { get; private set; } = false;

        public DeleteUserDialog(User user)
        {
            InitializeComponent();
            userService = new UserService();
            UserToDelete = user;
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            if (UserToDelete != null)
            {
                UserNameRun.Text = UserToDelete.FullName;
                UserEmailRun.Text = UserToDelete.Email;
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

        // Nút Xóa - Thực hiện xóa trong database
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Gọi service để xóa user
                bool success = userService.DeleteUser(UserToDelete.UserId);

                if (success)
                {
                    DeleteSuccess = true;
                    // 💡 Đã loại bỏ MessageBox. Thay vào đó, đóng dialog ngay.
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Giữ lại MessageBox nếu xóa thất bại để thông báo lỗi rõ ràng
                    MessageBox.Show("Xóa người dùng thất bại! Có thể do lỗi kết nối hoặc ràng buộc dữ liệu.",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Nút Hủy
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}