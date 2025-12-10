using Pet_Shop_Project.Views;
using System.Windows;
using Pet_Shop_Project.Services;

namespace Pet_Shop_Project
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            Services.AdminNavigationService.Instance.Initialize(AdminScreen);
            Services.AdminNavigationService.Instance.NavigateToReview();
        }
    }
}