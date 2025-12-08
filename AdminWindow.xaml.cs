using Pet_Shop_Project.Views;
using System.Windows;
using Pet_Shop_Project.Services;

namespace Pet_Shop_Project
{
    public partial class AdminWindow : Window
    {
        public string userid { get; set; }

        public AdminWindow(string userId)
        {
            InitializeComponent();
            this.userid = userId;

            // Initialize navigation service cho Admin
            Services.NavigationService.Instance.Initialize(AdminScreen);
            Services.NavigationService.Instance.setUserId(userid);

            // Navigate đến trang admin dashboard mặc định
            // Bạn có thể tạo AdminDashboard page hoặc dùng AccountPage
            AdminScreen.Navigate(new AccountPage(userId));
        }
    }
}