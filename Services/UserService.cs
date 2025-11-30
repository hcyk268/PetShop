using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Pet_Shop_Project.Services
{
    public class UserService
    {
        private readonly string connectionString;

        public UserService()
        {
            // Thay đổi connection string theo database của bạn
            connectionString = @"Server=YOUR_SERVER;Database=PetShopDB;Integrated Security=True;";

            // Hoặc dùng SQL Server Authentication:
            // connectionString = @"Server=YOUR_SERVER;Database=PetShopDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;";
        }

        // Xác thực người dùng - trả về User nếu đúng, null nếu sai
        public User AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT UserId, Username, PasswordHash, FullName, Email, 
                                           Phone, Address, Role, CreatedDate, IsActive
                                    FROM Users 
                                    WHERE Username = @Username AND IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PasswordHash"].ToString();
                                if (HashPassword(password) == storedHash) // So sánh trực tiếp
                                {
                                    // Đăng nhập thành công - tạo đối tượng User
                                    User user = new User
                                    {
                                        UserId = Convert.ToString(reader["UserId"]),
                                        Username = reader["Username"].ToString(),
                                        FullName = reader["FullName"].ToString(),
                                        Email = reader["Email"]?.ToString(),
                                        Phone = reader["Phone"]?.ToString(),
                                        Address = reader["Address"]?.ToString(),
                                        Role = reader["Role"].ToString(),
                                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                                        AvatarPath = reader["AvatarPath"]?.ToString()
                                    };

                                    return user;
                                }
                            }
                        }
                    }
                }

                // Sai username hoặc password
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
                throw new Exception($"Lỗi xác thực: {ex.Message}");
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

                    string query = @"SELECT UserId, Username, FullName, Email, Phone, 
                                           Address, Role, CreatedDate, IsActive
                                    FROM Users 
                                    WHERE UserId = @UserId AND IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = Convert.ToString(reader["UserId"]),
                                    Username = reader["Username"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"]?.ToString(),
                                    Phone = reader["Phone"]?.ToString(),
                                    Address = reader["Address"]?.ToString(),
                                    Role = reader["Role"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
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

        // Đăng ký user mới (INSERT vào database)
        public bool RegisterUser(User user, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO Users 
                                    (Username, Password, FullName, Email, Phone, 
                                     Role, CreatedDate, IsActive)
                                    VALUES 
                                    (@Username, @Password, @FullName, @Email, @Phone, 
                                     @Role, @CreatedDate, @IsActive)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@Password", HashPassword(password)); // Hash password
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                        cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");
                        cmd.Parameters.AddWithValue("@Role", user.Role);
                        cmd.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);
                        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);

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

        // Hash password - SHA256 (tạm thời, nên dùng BCrypt trong thực tế)
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

        // Verify password với BCrypt (nếu dùng BCrypt.Net-Next package)
        /*
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        */

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

        // Đổi mật khẩu
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra mật khẩu cũ
                    string checkQuery = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        string storedHash = checkCmd.ExecuteScalar()?.ToString();

                        if (storedHash == null || HashPassword(oldPassword) != storedHash)
                        {
                            return false; // Mật khẩu cũ không đúng
                        }
                    }

                    // Cập nhật mật khẩu mới
                    string updateQuery = "UPDATE Users SET PasswordHash = @NewHash WHERE UserId = @UserId";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@UserId", userId);
                        updateCmd.Parameters.AddWithValue("@NewHash", HashPassword(newPassword));

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePassword error: {ex.Message}");
                return false;
            }
        }
    }
}