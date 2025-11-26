using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YourNamespace
{
    public partial class AccountPage : Window
    {
        private string connectionString = @"Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Integrated Security=True";
        private int userId;

        public AccountPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData();
            LoadUserStats();
            LoadUserPreferences();
        }

        private void LoadUserData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT HoTen, Email, SoDienThoai, DiaChi, PhanQuyen, 
                                           Avatar, NgaySinh, NgayTao, LanDangNhapCuoi 
                                   FROM NguoiDung 
                                   WHERE ID = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Thông tin cơ bản
                                txtFullName.Text = reader["HoTen"]?.ToString() ?? "Chưa cập nhật";
                                txtEmail.Text = reader["Email"]?.ToString() ?? "Chưa cập nhật";
                                txtPhone.Text = reader["SoDienThoai"]?.ToString() ?? "Chưa cập nhật";
                                txtAddress.Text = reader["DiaChi"]?.ToString() ?? "Chưa cập nhật";

                                // Ngày sinh
                                if (reader["NgaySinh"] != DBNull.Value)
                                {
                                    DateTime ngaySinh = Convert.ToDateTime(reader["NgaySinh"]);
                                    txtBirthDate.Text = ngaySinh.ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    txtBirthDate.Text = "Chưa cập nhật";
                                }

                                // Phân quyền với màu sắc
                                string role = reader["PhanQuyen"]?.ToString();
                                if (role == "NhanVien")
                                {
                                    txtRole.Text = "👨‍💼 Nhân Viên";
                                    borderRole.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5252"));
                                }
                                else
                                {
                                    txtRole.Text = "👤 Khách Hàng";
                                    borderRole.SetResourceReference(Border.BackgroundProperty, "AccentColor");
                                }

                                // Năm tham gia
                                if (reader["NgayTao"] != DBNull.Value)
                                {
                                    DateTime ngayTao = Convert.ToDateTime(reader["NgayTao"]);
                                    txtMemberSince.Text = ngayTao.Year.ToString();
                                }
                                else
                                {
                                    txtMemberSince.Text = "N/A";
                                }

                                // Load avatar
                                LoadImage(reader["Avatar"]?.ToString(), AvatarImage);
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy thông tin người dùng!",
                                              "Lỗi",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void LoadUserStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Đếm số đơn hàng
                    string query = @"SELECT COUNT(*) FROM DonHang WHERE UserID = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        int totalOrders = Convert.ToInt32(cmd.ExecuteScalar());
                        txtTotalOrders.Text = totalOrders.ToString();
                    }
                }
            }
            catch
            {
                txtTotalOrders.Text = "0";
            }
        }

        private void LoadUserPreferences()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT NhanEmailThongBao, NhanSMSThongBao, DangKyKhuyenMai 
                                   FROM NguoiDung WHERE ID = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                chkEmailNotifications.IsChecked = reader["NhanEmailThongBao"] != DBNull.Value
                                    && Convert.ToBoolean(reader["NhanEmailThongBao"]);
                                chkSMSNotifications.IsChecked = reader["NhanSMSThongBao"] != DBNull.Value
                                    && Convert.ToBoolean(reader["NhanSMSThongBao"]);
                                chkNewsletters.IsChecked = reader["DangKyKhuyenMai"] != DBNull.Value
                                    && Convert.ToBoolean(reader["DangKyKhuyenMai"]);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Nếu không load được preferences, giữ mặc định
            }
        }

        private void LoadImage(string imagePath, ImageBrush imageBrush)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    imageBrush.ImageSource = image;
                }
                catch
                {
                    // Giữ hình mặc định nếu load lỗi
                }
            }
        }

        // Navigation Buttons
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            // Điều hướng về trang chủ
            // MainWindow mainWindow = new MainWindow();
            // mainWindow.Show();
            // this.Close();

            MessageBox.Show("Quay về trang chủ", "Điều hướng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnAccount_Click(object sender, RoutedEventArgs e)
        {
            // Đã ở trang account rồi
            MessageBox.Show("Bạn đang ở trang tài khoản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Avatar & Image Functions
        private void btnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Avatars", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(openFileDialog.FileName, destPath, true);

                    // Update database
                    UpdateImagePath("Avatar", destPath);

                    // Reload image
                    LoadImage(destPath, AvatarImage);

                    MessageBox.Show("Cập nhật ảnh đại diện thành công!",
                                  "Thành công",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật ảnh: {ex.Message}",
                                  "Lỗi",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void UpdateImagePath(string columnName, string imagePath)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = $"UPDATE NguoiDung SET {columnName} = @ImagePath WHERE ID = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể cập nhật database: {ex.Message}");
            }
        }

        // Preferences
        private void chkEmailNotifications_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePreference("NhanEmailThongBao", chkEmailNotifications.IsChecked == true);
        }

        private void chkSMSNotifications_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePreference("NhanSMSThongBao", chkSMSNotifications.IsChecked == true);
        }

        private void chkNewsletters_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePreference("DangKyKhuyenMai", chkNewsletters.IsChecked == true);
        }

        private void UpdatePreference(string columnName, bool value)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = $"UPDATE NguoiDung SET {columnName} = @Value WHERE ID = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Value", value);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật tùy chọn: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        // Action Buttons
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Mở dialog chỉnh sửa thông tin
            // EditProfileDialog editDialog = new EditProfileDialog(userId);
            // if (editDialog.ShowDialog() == true)
            // {
            //     LoadUserData();
            // }

            MessageBox.Show("Mở form chỉnh sửa thông tin cá nhân (Address, Phone, Email, Birth Date)",
                          "Chỉnh Sửa",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void btnPurchaseHistory_Click(object sender, RoutedEventArgs e)
        {
            // Mở trang lịch sử mua hàng
            // PurchaseHistoryWindow historyWindow = new PurchaseHistoryWindow(userId);
            // historyWindow.ShowDialog();

            MessageBox.Show("Mở trang lịch sử mua hàng với danh sách đơn hàng, trạng thái và chi tiết",
                          "Lịch Sử",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Mở dialog đổi mật khẩu
            // ChangePasswordDialog passwordDialog = new ChangePasswordDialog(userId);
            // passwordDialog.ShowDialog();

            MessageBox.Show("Mở form đổi mật khẩu với xác thực mật khẩu cũ",
                          "Đổi Mật Khẩu",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?",
                                        "Xác nhận Đăng Xuất",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear session và quay về trang đăng nhập
                // LoginWindow loginWindow = new LoginWindow();
                // loginWindow.Show();
                // Application.Current.MainWindow.Close();

                MessageBox.Show("Đăng xuất thành công! Quay về màn hình đăng nhập.",
                              "Đăng Xuất",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}