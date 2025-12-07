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

namespace Pet_Shop_Project.Controls
{
    /// <summary>
    /// Interaction logic for NavigationBar.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            Services.NavigationService.Instance.NavigateToHome();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            Services.NavigationService.Instance.NavigateToOrder();
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            Services.NavigationService.Instance.NavigateToCart();
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            Services.NavigationService.Instance.NavigateToAccount();
        }
    }
}