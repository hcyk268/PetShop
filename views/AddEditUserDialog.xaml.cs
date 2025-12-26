using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Pet_Shop_Project.Views
{
    public partial class AddEditUserDialog : Window
    {
        private UserService userService;
        private User currentUser;
        private bool isEditMode;

        // Constructor cho Add User (không có tham số)
        public AddEditUserDialog()
        {
            InitializeComponent();
            userService = new UserService();
            isEditMode = false;
            DialogTitle.Text = "Thêm người dùng mới";
            PasswordPanel.Visibility = Visibility.Visible;
        }

        // Constructor cho Edit User (có tham số User)
        public AddEditUserDialog(User user) : this()
        {
            currentUser = user;
            isEditMode = true;
            DialogTitle.Text = "Chỉnh sửa người dùng";
            PasswordPanel.Visibility = Visibility.Collapsed;

            // Load thông tin user lên form
            LoadUserData();
        }

        // Load thông tin user lên form (cho Edit mode)
        private void LoadUserData()
        {
            if (currentUser != null)
            {
                FullNameTextBox.Text = currentUser.FullName;
                UsernameTextBox.Text = currentUser.UserId; // Username = UserId
                UsernameTextBox.IsEnabled = false; // Không cho sửa username
                EmailTextBox.Text = currentUser.Email;
                PhoneTextBox.Text = currentUser.Phone;
                AddressTextBox.Text = currentUser.Address;

                // Set role
                foreach (var item in RoleComboBox.Items)
                {
                    var comboItem = item as System.Windows.Controls.ComboBoxItem;
                    if (comboItem?.Content.ToString() == currentUser.Role)
                    {
                        RoleComboBox.SelectedItem = comboItem;
                        break;
                    }
                }
            }
        }

        // Nút Lưu
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                UpdateUser();
            }
            else
            {
                AddUser();
            }
        }

        // Thêm user mới
        private void AddUser()
        {
            // Validate
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                string username = UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Kiểm tra username đã tồn tại
                if (userService.IsUsernameExists(username))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Tạo user mới
                User newUser = new User
                {
                    FullName = FullNameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    Role = (RoleComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString()
                };

                // Gọi service để thêm user
                bool success = userService.RegisterUser(newUser, username, password);

                if (success)
                {
                    MessageBox.Show("Thêm người dùng thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Thêm người dùng thất bại!",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Cập nhật user
        private void UpdateUser()
        {
            // Validate
            if (!ValidateInput(isUpdate: true))
            {
                return;
            }

            try
            {
                // Cập nhật thông tin
                currentUser.FullName = FullNameTextBox.Text.Trim();
                currentUser.Email = EmailTextBox.Text.Trim();
                currentUser.Phone = PhoneTextBox.Text.Trim();
                currentUser.Address = AddressTextBox.Text.Trim();
                currentUser.Role = (RoleComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

                // Gọi service để update
                bool success = userService.UpdateUser(currentUser);

                if (success)
                {
                    MessageBox.Show("Cập nhật người dùng thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Cập nhật người dùng thất bại!",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Validate input
        private bool ValidateInput(bool isUpdate = false)
        {
            // Họ tên
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return false;
            }

            // Username (chỉ khi Add)
            if (!isUpdate && string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập email!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Email không hợp lệ!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Password (chỉ khi Add)
            if (!isUpdate && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (!isUpdate && PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                PasswordBox.Focus();
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

        // Nút Hủy
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}