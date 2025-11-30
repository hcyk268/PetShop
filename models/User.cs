using System;

namespace Pet_Shop_Project.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; } // "Customer", "Admin", "Staff"
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public string PasswordHash { get; set; }  // Để lưu mật khẩu đã hash
        public string AvatarPath { get; set; }    // Đường dẫn ảnh avatar

        // Constructor
        public User()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
            Role = "Customer";
        }

        // Override ToString để debug dễ hơn
        public override string ToString()
        {
            return $"User: {Username} - {FullName} ({Role})";
        }
    }
}