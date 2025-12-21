using Microsoft.Win32;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Pet_Shop_Project.Views
{
    public partial class AccountPage : Page
    {
        private string currentUserId;
        private UserService userService;
        private User currentUser;
        private readonly UploadImageService uploadImageService;
        private const string DefaultAvatarPath = "pack://application:,,,/Images/avt.jpg";

        public AccountPage(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            userService = new UserService();
            uploadImageService = new UploadImageService();
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
                    EmailText.Text = currentUser.Email ?? "Chưa cập nhật";
                    PhoneText.Text = currentUser.Phone ?? "Chưa cập nhật";
                    AddressText.Text = currentUser.Address ?? "Chưa cập nhật";
                    FullNameText.Text = currentUser.FullName ?? "Chưa cập nhật";
                    RoleText.Text = GetRoleDisplayName(currentUser.Role);
                    JoinDateText.Text = currentUser.CreatedDate.ToString("dd/MM/yyyy");
                    SetAvatarImage(currentUser.Avatar);
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
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void SetAvatarImage(string avatarPath)
        {
            var path = string.IsNullOrWhiteSpace(avatarPath) ? DefaultAvatarPath : avatarPath;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                AvatarBrush.ImageSource = bitmap;
            }
            catch
            {
                AvatarBrush.ImageSource = new BitmapImage(new Uri(DefaultAvatarPath, UriKind.Absolute));
            }
        }

        private async void ChangeAvatar_Click(object sender, MouseButtonEventArgs e)
        {
            if (currentUser == null)
            {
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Ảnh (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            openFileDialog.Title = "Chọn ảnh làm avatar";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var (secureUrl, _) = await uploadImageService.UploadAsync(openFileDialog.FileName, "avatars");

                    if (string.IsNullOrWhiteSpace(secureUrl))
                    {
                        MessageBox.Show("Tải avatar thất bại", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    currentUser.Avatar = secureUrl;
                    bool isSaved = userService.UpdateUserAvatar(currentUser.UserId, secureUrl);

                    if (isSaved)
                    {
                        SetAvatarImage(secureUrl);
                        MessageBox.Show("Cập nhật avatar thành công", "Thành Công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể lưu avatar.", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải avatar: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        // Contact Information Edit Functions
        private void EditContact_Click(object sender, RoutedEventArgs e)
        {
            // Hide text displays, show text boxes
            EmailText.Visibility = Visibility.Collapsed;
            EmailEdit.Visibility = Visibility.Visible;
            EmailEdit.Text = EmailText.Text == "Chưa cập nhật" ? "" : EmailText.Text;

            PhoneText.Visibility = Visibility.Collapsed;
            PhoneEdit.Visibility = Visibility.Visible;
            PhoneEdit.Text = PhoneText.Text == "Chưa cập nhật" ? "" : PhoneText.Text;

            AddressText.Visibility = Visibility.Collapsed;
            AddressEdit.Visibility = Visibility.Visible;
            AddressEdit.Text = AddressText.Text == "Chưa cập nhật" ? "" : AddressText.Text;

            // Toggle buttons
            EditContactButton.Visibility = Visibility.Collapsed;
            SaveContactButton.Visibility = Visibility.Visible;
            CancelContactButton.Visibility = Visibility.Visible;
        }

        private void SaveContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                string email = EmailEdit.Text.Trim();
                string phone = PhoneEdit.Text.Trim();
                string address = AddressEdit.Text.Trim();

                // Validate email
                if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                {
                    MessageBox.Show("Email không hợp lệ!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate phone
                if (!string.IsNullOrEmpty(phone))
                {
                    if (!phone.StartsWith("0") || phone.Length != 10 || !IsNumeric(phone))
                    {
                        MessageBox.Show("Số điện thoại phải có 10 số và bắt đầu bằng 0!",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Update user object
                currentUser.Email = string.IsNullOrEmpty(email) ? null : email;
                currentUser.Phone = string.IsNullOrEmpty(phone) ? null : phone;
                currentUser.Address = string.IsNullOrEmpty(address) ? null : address;

                // Save to database
                bool success = userService.UpdateUser(currentUser);

                if (success)
                {
                    // Update display
                    EmailText.Text = currentUser.Email ?? "Chưa cập nhật";
                    PhoneText.Text = currentUser.Phone ?? "Chưa cập nhật";
                    AddressText.Text = currentUser.Address ?? "Chưa cập nhật";

                    ExitContactEditMode();

                    MessageBox.Show("Cập nhật thông tin thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật thông tin!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelContact_Click(object sender, RoutedEventArgs e)
        {
            ExitContactEditMode();
        }

        private void ExitContactEditMode()
        {
            // Show text displays, hide text boxes
            EmailText.Visibility = Visibility.Visible;
            EmailEdit.Visibility = Visibility.Collapsed;

            PhoneText.Visibility = Visibility.Visible;
            PhoneEdit.Visibility = Visibility.Collapsed;

            AddressText.Visibility = Visibility.Visible;
            AddressEdit.Visibility = Visibility.Collapsed;

            // Toggle buttons
            EditContactButton.Visibility = Visibility.Visible;
            SaveContactButton.Visibility = Visibility.Collapsed;
            CancelContactButton.Visibility = Visibility.Collapsed;
        }

        // Account Information Edit Functions
        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            // Hide text display, show text box
            FullNameText.Visibility = Visibility.Collapsed;
            FullNameEdit.Visibility = Visibility.Visible;
            FullNameEdit.Text = FullNameText.Text == "Chưa cập nhật" ? "" : FullNameText.Text;

            // Toggle buttons
            EditAccountButton.Visibility = Visibility.Collapsed;
            SaveAccountButton.Visibility = Visibility.Visible;
            CancelAccountButton.Visibility = Visibility.Visible;
        }

        private void SaveAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = FullNameEdit.Text.Trim();

                if (string.IsNullOrEmpty(fullName))
                {
                    MessageBox.Show("Họ tên không được để trống!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update user object
                currentUser.FullName = fullName;

                // Save to database
                bool success = userService.UpdateUser(currentUser);

                if (success)
                {
                    // Update displays
                    FullNameText.Text = currentUser.FullName;
                    UserNameText.Text = currentUser.FullName;

                    ExitAccountEditMode();

                    MessageBox.Show("Cập nhật thông tin thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật thông tin!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAccount_Click(object sender, RoutedEventArgs e)
        {
            ExitAccountEditMode();
        }

        private void ExitAccountEditMode()
        {
            // Show text display, hide text box
            FullNameText.Visibility = Visibility.Visible;
            FullNameEdit.Visibility = Visibility.Collapsed;

            // Toggle buttons
            EditAccountButton.Visibility = Visibility.Visible;
            SaveAccountButton.Visibility = Visibility.Collapsed;
            CancelAccountButton.Visibility = Visibility.Collapsed;
        }

        // Validation Functions
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

        private bool IsNumeric(string text)
        {
            Regex regex = new Regex("^[0-9]+$");
            return regex.IsMatch(text);
        }

        private void PhoneEdit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        // Other Functions
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordDialog dialog = new ChangePasswordDialog(currentUserId);
            dialog.ShowDialog();
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
