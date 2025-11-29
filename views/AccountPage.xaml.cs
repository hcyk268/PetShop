using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pet_Shop_Project.Views
{
    public partial class AccountPage : Window
    {
        // THAY THẾ CHUỖI KẾT NỐI VÀO ĐÂY SAU KHI RESTORE DB
        private string connectionString = @"Data Source=DESKTOP-MEEB046;Initial Catalog=PETSHOP;Integrated Security=True";
        private int userId;

        // Hàm tạo mặc định: Cần thiết để WPF khởi động (StartupUri)
        public AccountPage()
        {
            InitializeComponent();
            this.userId = -1; // ID không hợp lệ cho trường hợp khởi động mặc định
        }

        // Hàm tạo chính: Dùng khi người dùng đăng nhập thành công
        public AccountPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            // Tải dữ liệu chỉ khi có userId hợp lệ
            if (this.userId > 0)
            {
                LoadUserData();
                LoadUserStats();
                LoadUserPreferences();
            }
        }

        #region Vùng Load Dữ Liệu

        private void LoadUserData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Đã thêm cột NgaySinh vào truy vấn
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

                                // Phân quyền
                                string role = reader["PhanQuyen"]?.ToString();
                                if (role == "NhanVien" || role == "Admin")
                                {
                                    txtRole.Text = "👨‍💼 Nhân Viên";
                                    borderRole.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
                                }
                                else
                                {
                                    txtRole.Text = "👤 Khách Hàng";
                                    borderRole.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                                }

                                // Ngày tạo (Năm tham gia)
                                if (reader["NgayTao"] != DBNull.Value)
                                {
                                    DateTime ngayTao = Convert.ToDateTime(reader["NgayTao"]);
                                    txtCreatedDate.Text = ngayTao.ToString("dd/MM/yyyy");
                                    txtMemberSince.Text = ngayTao.Year.ToString();
                                }
                                else
                                {
                                    txtCreatedDate.Text = "N/A";
                                    txtMemberSince.Text = "N/A";
                                }

                                // Lần đăng nhập cuối
                                if (reader["LanDangNhapCuoi"] != DBNull.Value)
                                {
                                    DateTime lastLogin = Convert.ToDateTime(reader["LanDangNhapCuoi"]);
                                    txtLastLogin.Text = lastLogin.ToString("dd/MM/yyyy HH:mm");
                                }
                                else
                                {
                                    txtLastLogin.Text = "Chưa có dữ liệu";
                                }

                                // Load avatar
                                LoadImage(reader["Avatar"]?.ToString(), AvatarImage);
                            }
                            else
                            {
                                ShowErrorMessage("Không tìm thấy thông tin người dùng!");
                                // Có thể giữ cửa sổ mở với dữ liệu "Chưa cập nhật"
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }

        private void LoadUserStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //đếm số đơn hàng
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
            // Logic giữ nguyên, đảm bảo các CheckBox được tải từ DB
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
                // Giữ mặc định nếu load lỗi
            }
        }

        private void LoadImage(string imagePath, ImageBrush imageBrush)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imagePath, UriKind.Absolute);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    imageBrush.ImageSource = image;
                }
                catch { }
            }
            else if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imagePath, UriKind.Relative);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    imageBrush.ImageSource = image;
                }
                catch { }
            }
        }

        #endregion

        #region Vùng Thao Tác Cơ Sở Dữ Liệu 

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
                ShowErrorMessage($"Lỗi khi cập nhật tùy chọn: {ex.Message}");
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
                ShowErrorMessage($"Không thể cập nhật database: {ex.Message}");
            }
        }

        #endregion

        #region Vùng Xử Lý Sự Kiện

        // Thêm nút Change Avatar trong XAML để sử dụng hàm này
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

                    UpdateImagePath("Avatar", destPath);
                    LoadImage(destPath, AvatarImage);
                    ShowSuccessMessage("Cập nhật ảnh đại diện thành công!");
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Lỗi khi cập nhật ảnh: {ex.Message}");
                }
            }
        }

        private void btnChangeBackground_Click(object sender, RoutedEventArgs e)
        {
            // Logic giữ nguyên, cho phép thay đổi hình nền
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh nền",
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Backgrounds", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(openFileDialog.FileName, destPath, true);

                    // Cập nhật database nếu có cột Background
                    // UpdateImagePath("Background", destPath);

                    LoadImage(destPath, BackgroundImage);
                    ShowSuccessMessage("Cập nhật hình nền thành công!");
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Lỗi khi cập nhật hình nền: {ex.Message}");
                }
            }
        }

        // Preferences Checkboxes
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

        // Action Buttons
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở form chỉnh sửa thông tin cá nhân (Address, Phone, Email, Birth Date).",
                            "Chỉnh Sửa", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUserData();
            LoadUserStats();
            LoadUserPreferences();
            ShowSuccessMessage("Đã làm mới dữ liệu!");
        }

        // Đổi tên từ btnChangePassword_Click thành btnPurchaseHistory_Click
        // Và tạo hàm mới cho việc đổi mật khẩu.
        private void btnPurchaseHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở trang lịch sử mua hàng với danh sách đơn hàng, trạng thái và chi tiết.",
                           "Lịch Sử Mua Hàng", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?",
                                         "Xác nhận Đăng Xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ShowSuccessMessage("Đăng xuất thành công! Quay về màn hình đăng nhập.");
                // Mở lại cửa sổ Login
                Window1 loginWindow = new Window1();
                loginWindow.Show();

                // Đóng cửa sổ hiện tại
                Window current = Window.GetWindow(this);
                current?.Close();
            }
        }

        #endregion

        #region Helpers

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        private void btn_PurHistory_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
    
}