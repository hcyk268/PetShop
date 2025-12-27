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

        // ========== CONTACT INFORMATION EDIT FUNCTIONS ==========
        private void EditContact_Click(object sender, RoutedEventArgs e)
        {
            EmailText.Visibility = Visibility.Collapsed;
            EmailEdit.Visibility = Visibility.Visible;
            EmailEdit.Text = EmailText.Text == "Chưa cập nhật" ? "" : EmailText.Text;

            PhoneText.Visibility = Visibility.Collapsed;
            PhoneEdit.Visibility = Visibility.Visible;
            PhoneEdit.Text = PhoneText.Text == "Chưa cập nhật" ? "" : PhoneText.Text;

            AddressText.Visibility = Visibility.Collapsed;
            AddressEdit.Visibility = Visibility.Visible;
            AddressEdit.Text = AddressText.Text == "Chưa cập nhật" ? "" : AddressText.Text;

            EditContactButton.Visibility = Visibility.Collapsed;
            SaveContactButton.Visibility = Visibility.Visible;
            CancelContactButton.Visibility = Visibility.Visible;
        }

        private void SaveContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = EmailEdit.Text.Trim();
                string phone = PhoneEdit.Text.Trim();
                string address = AddressEdit.Text.Trim();

                // ✅ VALIDATE EMAIL
                if (!string.IsNullOrEmpty(email))
                {
                    var emailValidation = ValidateEmail(email);
                    if (!emailValidation.IsValid)
                    {
                        MessageBox.Show(emailValidation.ErrorMessage, "Lỗi Email",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        EmailEdit.Focus();
                        return;
                    }
                }

                // ✅ VALIDATE PHONE
                if (!string.IsNullOrEmpty(phone))
                {
                    var phoneValidation = ValidatePhone(phone);
                    if (!phoneValidation.IsValid)
                    {
                        MessageBox.Show(phoneValidation.ErrorMessage, "Lỗi Số điện thoại",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        PhoneEdit.Focus();
                        return;
                    }
                }

                // ✅ VALIDATE ADDRESS
                if (!string.IsNullOrEmpty(address))
                {
                    var addressValidation = ValidateAddress(address);
                    if (!addressValidation.IsValid)
                    {
                        MessageBox.Show(addressValidation.ErrorMessage, "Lỗi Địa chỉ",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        AddressEdit.Focus();
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
            EmailText.Visibility = Visibility.Visible;
            EmailEdit.Visibility = Visibility.Collapsed;

            PhoneText.Visibility = Visibility.Visible;
            PhoneEdit.Visibility = Visibility.Collapsed;

            AddressText.Visibility = Visibility.Visible;
            AddressEdit.Visibility = Visibility.Collapsed;

            EditContactButton.Visibility = Visibility.Visible;
            SaveContactButton.Visibility = Visibility.Collapsed;
            CancelContactButton.Visibility = Visibility.Collapsed;
        }

        // ========== ACCOUNT INFORMATION EDIT FUNCTIONS ==========
        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            FullNameText.Visibility = Visibility.Collapsed;
            FullNameEdit.Visibility = Visibility.Visible;
            FullNameEdit.Text = FullNameText.Text == "Chưa cập nhật" ? "" : FullNameText.Text;

            EditAccountButton.Visibility = Visibility.Collapsed;
            SaveAccountButton.Visibility = Visibility.Visible;
            CancelAccountButton.Visibility = Visibility.Visible;
        }

        private void SaveAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = FullNameEdit.Text.Trim();

                var nameValidation = ValidateFullName(fullName);
                if (!nameValidation.IsValid)
                {
                    MessageBox.Show(nameValidation.ErrorMessage, "Lỗi Họ tên",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    FullNameEdit.Focus();
                    return;
                }

                currentUser.FullName = fullName;
                bool success = userService.UpdateUser(currentUser);

                if (success)
                {
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
            FullNameText.Visibility = Visibility.Visible;
            FullNameEdit.Visibility = Visibility.Collapsed;

            EditAccountButton.Visibility = Visibility.Visible;
            SaveAccountButton.Visibility = Visibility.Collapsed;
            CancelAccountButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Validate Email với các quy tắc đầy đủ
        /// </summary>
        private ValidationResult ValidateEmail(string email)
        {
            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ValidationResult(false, "Email không được để trống!");
            }

            // Regex đầy đủ cho email
            string emailPattern = @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z]{2,})+$";

            if (!Regex.IsMatch(email, emailPattern))
            {
                return new ValidationResult(false, "Email không hợp lệ! Vui lòng nhập đúng định dạng email.");
            }

            // Kiểm tra các trường hợp đặc biệt
            if (email.Contains("..") || email.StartsWith(".") || email.EndsWith("."))
            {
                return new ValidationResult(false, "Email không được có dấu chấm liên tiếp hoặc ở đầu/cuối!");
            }

            return new ValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Validate số điện thoại Việt Nam
        /// </summary>
        private ValidationResult ValidatePhone(string phone)
        {
            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(phone))
            {
                return new ValidationResult(false, "Số điện thoại không được để trống!");
            }

            // Loại bỏ khoảng trắng
            phone = phone.Replace(" ", "").Replace("-", "");

            // Kiểm tra chỉ chứa số
            if (!Regex.IsMatch(phone, @"^\d+$"))
            {
                return new ValidationResult(false, "Số điện thoại chỉ được chứa số!");
            }

            // Kiểm tra độ dài
            if (phone.Length != 10)
            {
                return new ValidationResult(false, "Số điện thoại phải có đúng 10 chữ số!");
            }

            return new ValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Validate địa chỉ
        /// </summary>
        private ValidationResult ValidateAddress(string address)
        {
            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(address))
            {
                return new ValidationResult(false, "Địa chỉ không được để trống!");
            }

            // Kiểm tra ký tự đặc biệt nguy hiểm
            string dangerousChars = @"[<>""'%;()&+]";
            if (Regex.IsMatch(address, dangerousChars))
            {
                return new ValidationResult(false, "Địa chỉ chứa ký tự không hợp lệ!");
            }

            return new ValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Validate họ tên
        /// </summary>
        private ValidationResult ValidateFullName(string fullName)
        {
            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return new ValidationResult(false, "Họ tên không được để trống!");
            }

            // Kiểm tra độ dài
            if (fullName.Length < 2)
            {
                return new ValidationResult(false, "Họ tên quá ngắn! Vui lòng nhập ít nhất 2 ký tự.");
            }

            if (fullName.Length > 50)
            {
                return new ValidationResult(false, "Họ tên quá dài! Vui lòng nhập tối đa 50 ký tự.");
            }

            // Kiểm tra chỉ chứa chữ cái, khoảng trắng và dấu tiếng Việt
            string namePattern = @"^[a-zA-ZÀ-ỹ\s]+$";
            if (!Regex.IsMatch(fullName, namePattern))
            {
                return new ValidationResult(false, "Họ tên chỉ được chứa chữ cái và khoảng trắng!");
            }

            // Kiểm tra khoảng trắng liên tiếp
            if (Regex.IsMatch(fullName, @"\s{2,}"))
            {
                return new ValidationResult(false, "Họ tên không được chứa nhiều khoảng trắng liên tiếp!");
            }

            return new ValidationResult(true, string.Empty);
        }

        private bool IsNumeric(string text)
        {
            return Regex.IsMatch(text, @"^\d+$");
        }

        private void PhoneEdit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            e.Handled = !IsNumeric(e.Text);
        }

        // Hàm khác
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

    // Kết quả validate
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}