using Pet_Shop_Project.Services;
using Pet_Shop_Project.Views;
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

using NavService = Pet_Shop_Project.Services.NavigationService; // Bị trùng tên với File NavigationService
namespace Pet_Shop_Project.Controls
{
    public partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Tìm MainWindow
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // Kiểm tra nếu đang ở HomePage
                if (mainWindow.MainScreen.Content is HomePage homePage)
                {
                    // Reset HomePage - load lại tất cả sản phẩm
                    homePage.LoadAllProducts();
                }
                else
                {
                    // Navigate đến HomePage mới
                    NavService.Instance.NavigateToHome();
                }
            }
        }
    }
}