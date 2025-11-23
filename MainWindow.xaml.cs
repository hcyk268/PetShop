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

namespace Pet_Shop_Project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate đến trang đầu tiên
            MainFrame.Navigate(new SignIn());
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame.Navigate(new HomePage());
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame.Navigate(new ProductsPage());
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame.Navigate(new OrdersPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SignIn());
        }
    }
}