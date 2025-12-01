using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;

using Pet_Shop_Project.Models;

using System.Security.Cryptography;


namespace Pet_Shop_Project.Services
{
    public class UserService
    {
        private readonly string connectionString;

        public UserService()
        {
            // ✅ Thay đổi connection string theo database của bạn
            connectionString = @"Server=DESKTOP-MEEB046;Database=PETSHOP;Integrated Security=True;";
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

                    string query = @"SELECT UserId, FullName, Email, Phone, Address, Role
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
                                    Role = reader["Role"].ToString()
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
                                    (Username, FullName, Email, Phone, Password, Role) 
                                    VALUES 
                                    (@Username, @FullName, @Email, @Phone, @Password, @Role)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                        cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");
                        cmd.Parameters.AddWithValue("@Password", HashPassword(password));
                        cmd.Parameters.AddWithValue("@Role", user.Role ?? "Customer");

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
    }
}