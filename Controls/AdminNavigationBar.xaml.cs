using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pet_Shop_Project.Services;
using Pet_Shop_Project.Views;

namespace Pet_Shop_Project.Controls
{
    public partial class AdminNavigationBar : UserControl
    {
        public AdminNavigationBar()
        {
            InitializeComponent();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
           Services.AdminNavigationService.Instance.NavigateToInventory();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
           Services.AdminNavigationService.Instance.NavigateToOrder();
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            Services.AdminNavigationService.Instance.NavigateToReview();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Đăng xuất thành công!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                 
                Login loginWindow = new Login();
                loginWindow.Show();

                Window.GetWindow(this)?.Close();
            }
        }
    }
}