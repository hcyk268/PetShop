using Pet_Shop_Project.Services;
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

namespace Pet_Shop_Project.Views
{
    public partial class DashBoard : Page
    {
        public DashBoard() : this("Admin")
        {
        }

        public DashBoard(string userName)
        {
            InitializeComponent();

            DataContext = new DashboardViewModel(userName);
        }
    }

    public class DashboardViewModel
    {
        public string UserGreeting { get; set; }
        public string UserName { get; set; }

        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public string TotalRevenue { get; set; }

        public List<string> BestSellers { get; set; }
        public List<string> WorstSellers { get; set; }

        private DashboardService dashboardService;

        public DashboardViewModel(string name)
        {
            UserName = name;
            UserGreeting = $"Chào mừng trở lại, {name}!";

            dashboardService = new DashboardService();

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            TotalUsers = dashboardService.GetTotalUsers();
            TotalProducts = dashboardService.GetTotalProducts();
            TotalOrders = dashboardService.GetTotalOrders();
            TotalRevenue = dashboardService.GetTotalRevenue().ToString("N0") + " VND";

            BestSellers = dashboardService.GetBestSellers();
            WorstSellers = dashboardService.GetWorstSellers();
        }
    }
}

