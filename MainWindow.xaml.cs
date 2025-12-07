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
using Pet_Shop_Project.Services;

namespace Pet_Shop_Project
{
    public partial class MainWindow : Window
    {
        public string userid { get; set; }
        public MainWindow(string userId) //cần truyền userId vào để truy vấn chính xác
        {
            InitializeComponent();
            this.userid = userId;
            Services.NavigationService.Instance.Initialize(MainScreen);
            Services.NavigationService.Instance.setUserId(userid);
            MainScreen.Navigate(new AccountPage(userId));
        }
    }
}