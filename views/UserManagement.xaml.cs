using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pet_Shop_Project.Views
{
    public partial class UserManagement : Page
    {
        private UserService userService;
        private List<User> allUsers;
        private List<User> filteredUsers;

        public UserManagement()
        {
            InitializeComponent();
            userService = new UserService();
            LoadUsers();
        }

        // Load tất cả users từ database
        private void LoadUsers()
        {
            try
            {
                allUsers = userService.GetAllUsers();
                filteredUsers = new List<User>(allUsers);
                DisplayUsers(filteredUsers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách người dùng: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Hiển thị danh sách users lên UI
        private void DisplayUsers(List<User> users)
        {
            if (users == null || users.Count == 0)
            {
                UserDataGrid.ItemsSource = null;
                EmptyStatePanel.Visibility = Visibility.Visible;
                return;
            }

            EmptyStatePanel.Visibility = Visibility.Collapsed;

            // Chuyển đổi User thành UserDisplayModel để binding
            var displayUsers = users.Select(u => new UserDisplayModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                RoleDisplay = GetRoleDisplay(u.Role),
                RoleColor = GetRoleColor(u.Role)
            }).ToList();

            UserDataGrid.ItemsSource = displayUsers;
        }

        // Chuyển đổi Role gốc (DB) sang hiển thị (UI)
        private string GetRoleDisplay(string role)
        {
            switch (role?.ToLower())
            {
                case "admin": return "Quản trị viên";
                case "customer": return "Khách hàng";
                default: return "Khách hàng";
            }
        }

        // 💡 HÀM MỚI: Chuyển đổi từ Role hiển thị (UI) sang tên Role lưu trong DB
        private string GetRoleInternalName(string roleDisplay)
        {
            switch (roleDisplay)
            {
                case "Quản trị viên": return "Admin";
                case "Khách hàng": return "User"; // Hoặc "User" nếu bạn lưu là "User"
                default: return null; // Dành cho "Tất cả vai trò" hoặc không xác định
            }
        }

        // Màu sắc cho Role Badge
        private string GetRoleColor(string role)
        {
            switch (role?.ToLower())
            {
                case "admin": return "#DC3545";    // Đỏ
                case "user": return "#FFAD57"; 
                default: return "#FFCC96";
            }
        }

        // ===== NÚT THÊM USER =====
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var addDialog = new AddEditUserDialog();
            addDialog.Owner = Window.GetWindow(this);

            if (addDialog.ShowDialog() == true)
            {
                LoadUsers(); // Reload danh sách sau khi thêm
            }
        }

        

        // ===== NÚT CHỈNH SỬA USER =====
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = (sender as Button)?.Tag.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                User user = userService.GetUserById(userId);

                if (user != null)
                {
                    // Giả định AddEditUserDialog tồn tại
                    var editDialog = new AddEditUserDialog(user);
                    editDialog.Owner = Window.GetWindow(this);

                    if (editDialog.ShowDialog() == true)
                    {
                        LoadUsers(); // Reload sau khi edit
                    }
                }
            }
        }
        
       
        // ===== NÚT XÓA USER =====
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = (sender as Button)?.Tag.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                User user = userService.GetUserById(userId);

                if (user != null)
                {
                    // Giả định DeleteUserDialog tồn tại
                    var confirmDialog = new DeleteUserDialog(user);
                    confirmDialog.Owner = Window.GetWindow(this);

                    if (confirmDialog.ShowDialog() == true && confirmDialog.DeleteSuccess)
                    {
                        LoadUsers(); // Reload sau khi xóa thành công
                    }
                }
            }
        }

        // ===== TÌM KIẾM & LỌC =====
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void RoleFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Áp dụng cả Search và Filter
        private void ApplyFilters()
        {
            if (allUsers == null) return;

            string searchText = SearchTextBox?.Text?.ToLower() ?? "";
            string selectedRoleDisplay = (RoleFilterComboBox?.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Chuyển đổi tên hiển thị vai trò sang tên vai trò DB
            string selectedRoleInternal = GetRoleInternalName(selectedRoleDisplay);

            filteredUsers = allUsers.Where(u =>
            {
                // Filter theo search text
                bool matchSearch = string.IsNullOrEmpty(searchText) ||
                    u.FullName?.ToLower().Contains(searchText) == true ||
                    u.Email?.ToLower().Contains(searchText) == true ||
                    u.Phone?.Contains(searchText) == true;

                // Filter theo role: Chỉ lọc nếu không phải "Tất cả vai trò" (selectedRoleInternal != null)
                bool matchRole = selectedRoleInternal == null ||
                    u.Role?.Equals(selectedRoleInternal, StringComparison.OrdinalIgnoreCase) == true;

                return matchSearch && matchRole;
            }).ToList();

            DisplayUsers(filteredUsers);
        }

        // ===== HOVER EFFECTS =====
        private void UserItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(248, 249, 250));
            }
        }

        private void UserItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = System.Windows.Media.Brushes.White;
            }
        }
    }
        
    // Model để hiển thị trên UI
    public class UserDisplayModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } // Giữ lại Role DB để dễ thao tác
        public string RoleDisplay { get; set; } // Role hiển thị trên UI
        public string RoleColor { get; set; } // Màu sắc cho Role Badge
    }
}