using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Pet_Shop_Project.Services
{
    public class UserService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        public UserService()
        {
        }

        // ✅ METHOD XÁC THỰC - So sánh username/password với database
        public User AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query lấy thông tin user theo username
                    string query = @"SELECT UserId, Username, Password, FullName, Email, Phone, Address, Role
                            FROM Users 
                            WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 1. Lấy mật khẩu gốc (clear-text) từ database
                                string storedPassword = reader["Password"].ToString();

                                // ❌ BỎ QUA HASHING và so sánh trực tiếp
                                if (password == storedPassword)
                                {
                                    // Đăng nhập thành công - Tạo User object
                                    User user = new User
                                    {
                                        UserId = reader["UserId"].ToString(),
                                        FullName = reader["FullName"].ToString(),
                                        Email = reader["Email"]?.ToString(),
                                        Phone = reader["Phone"]?.ToString(),
                                        Address = reader["Address"]?.ToString(),
                                        Role = reader["Role"].ToString()
                                    };

                                    return user;
                                }
                            }
                        }
                    }
                }

                // ❌ Sai username hoặc password
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
                throw new Exception($"Lỗi xác thực: {ex.Message}");
            }
        }
        // Lấy tất cả users (cho Admin quản lý)
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT UserId, Username, FullName, Email, Phone, Address, Role
                                    FROM Users 
                                    ORDER BY UserId DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = reader["UserId"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"]?.ToString(),
                                    Phone = reader["Phone"]?.ToString(),
                                    Address = reader["Address"]?.ToString(),
                                    Role = reader["Role"].ToString()
                                });
                            }
                        }
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllUsers error: {ex.Message}");
                throw;
            }
        }

        // Xóa user
        public bool DeleteUser(string userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM USERS WHERE UserId = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteUser error: {ex.Message}");
                throw new Exception($"Không thể xóa người dùng: {ex.Message}");
            }
        }
        // Hash password bằng SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Lấy thông tin user theo UserId
        public User GetUserById(string userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT UserId, FullName, Email, Phone, Address, Role, CreatedDate
                                    FROM Users 
                                    WHERE UserId = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = reader["UserId"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"]?.ToString(),
                                    Phone = reader["Phone"]?.ToString(),
                                    Address = reader["Address"]?.ToString(),
                                    Role = reader["Role"].ToString(),
                                    CreatedDate = (DateTime)reader["CreatedDate"],
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserById error: {ex.Message}");
                throw;
            }
        }

        // Kiểm tra username đã tồn tại chưa
        public bool IsUsernameExists(string username)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsUsernameExists error: {ex.Message}");
                return false;
            }
        }

        // Kiểm tra email đã tồn tại chưa
        public bool IsEmailExists(string email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsEmailExists error: {ex.Message}");
                return false;
            }
        }

        // Đăng ký user mới
        public bool RegisterUser(User user, string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO Users 
                                    (Username, FullName, Email, Phone, Address, Password, Role, CreatedDate) 
                                    VALUES 
                                    (@Username, @FullName, @Email, @Phone, @Address, @Password, @Role, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                        cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");
                        cmd.Parameters.AddWithValue("@Address", user.Address ?? "");
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Role", user.Role ?? "User");
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RegisterUser error: {ex.Message}");
                throw new Exception($"Không thể đăng ký tài khoản: {ex.Message}");
            }
        }
        // Thêm method này vào cuối class UserService (trước dấu đóng ngoặc })

        // Đổi mật khẩu
        public bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra mật khẩu cũ có đúng không
                    string checkQuery = @"SELECT Password FROM Users WHERE UserId = @UserId";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);

                        object result = checkCmd.ExecuteScalar();

                        if (result == null)
                        {
                            return false; // User không tồn tại
                        }

                        string storedPassword = result.ToString();

                        // So sánh mật khẩu cũ
                        if (oldPassword != storedPassword)
                        {
                            return false; // Mật khẩu cũ không đúng
                        }
                    }

                    // Cập nhật mật khẩu mới
                    string updateQuery = @"UPDATE Users 
                                  SET Password = @NewPassword 
                                  WHERE UserId = @UserId";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@UserId", userId);
                        updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePassword error: {ex.Message}");
                throw new Exception($"LÃƒÆ’Ã‚Â¡Ãƒâ€šÃ‚Â»ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Âi khi ÃƒÆ’Ã¢â‚¬Å¾ÃƒÂ¢Ã¢â€šÂ¬Ã‹Å“ÃƒÆ’Ã‚Â¡Ãƒâ€šÃ‚Â»ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¢i mÃƒÆ’Ã‚Â¡Ãƒâ€šÃ‚ÂºÃƒâ€šÃ‚Â­t khÃƒÆ’Ã‚Â¡Ãƒâ€šÃ‚ÂºÃƒâ€šÃ‚Â©u: {ex.Message}");
            }
        }
        // Cập nhật thông tin user
        public bool UpdateUser(User user)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"UPDATE Users 
                                    SET FullName = @FullName,
                                        Email = @Email,
                                        Phone = @Phone,
                                        Address = @Address
                                    WHERE UserId = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", user.UserId);
                        cmd.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                        cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                        cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");
                        cmd.Parameters.AddWithValue("@Address", user.Address ?? "");
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUser error: {ex.Message}");
                return false;
            }
        }

        public async Task<ObservableCollection<User>> GetAllUsersAsync()
        {
            ObservableCollection<User> allUsers = new ObservableCollection<User>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    var query = " SELECT * FROM USERS";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                User user = new User
                                {
                                    UserId = reader["UserId"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Username = reader["Username"].ToString(),
                                    CreatedDate = (DateTime)reader["CreatedDate"]
                                };
                                allUsers.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }

            return allUsers;
        }

        public async Task<ObservableCollection<User>> GetAllCustomer()
        {
            ObservableCollection<User> allCustomers = new ObservableCollection<User>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    var query = " SELECT * FROM USERS WHERE Role = 'User'";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                User user = new User
                                {
                                    UserId = reader["UserId"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Username = reader["Username"].ToString(),
                                    CreatedDate = (DateTime)reader["CreatedDate"]
                                };
                                allCustomers.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }

            return allCustomers;
        }

        public async Task<string> AddVirtualUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tên không được để trống", nameof(name));
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string query = @"INSERT INTO Users
                                (Username, FullName, Email, Password, Role, CreatedDate)
                                OUTPUT INSERTED.UserId
                                VALUES
                                (@Username, @FullName, @Email, @Password, @Role, @CreatedDate)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", "Virtual_" + DateTime.Now.ToString("ddMMyyHHmmss"));
                    cmd.Parameters.AddWithValue("@FullName", name.Trim());
                    cmd.Parameters.AddWithValue("@Email", "Virtual_" + DateTime.Now.ToString("ddMMyyHHmmss"));
                    cmd.Parameters.AddWithValue("@Password", "Virtual_" + DateTime.Now.ToString("ddMMyyHHmmss"));
                    cmd.Parameters.AddWithValue("@Role", "User");
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        throw new Exception("Không lấy được UserId sau khi insert");
                    }
                    return result.ToString();
                }
            }
        }
    }

}
