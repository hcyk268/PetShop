using LiveCharts;
using LiveCharts.Wpf;
using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pet_Shop_Project.Views
{
    public partial class DashBoard : Page
    {
        private string currentUserId;
        private UserService userService;
        private User currentUser;
        public DashBoard() : this("Admin")
        {
        }

        

        // Constructor chính - nhận userId từ trang Login
        public DashBoard(string userId)
        {
            InitializeComponent();

            currentUserId = userId;
            userService = new UserService();

            // Load thông tin user từ database
            LoadUserInfo();

            // Khởi tạo ViewModel với tên user
            if (currentUser != null)
            {
                DataContext = new DashboardViewModel(currentUser.FullName ?? "Admin");
            }
            else
            {
                DataContext = new DashboardViewModel("Admin");
            }
        }
        private void LoadUserInfo()
        {
            try
            {
                currentUser = userService.GetUserById(currentUserId);

                if (currentUser == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin tài khoản!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
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

        // Properties cho biểu đồ
        public SeriesCollection WeeklyRevenueSeries { get; set; }
        public List<string> WeekLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public double YAxisStep { get; set; }

        private DashboardService dashboardService;

        public DashboardViewModel(string name)
        {
            UserName = name;
            UserGreeting = $"Chào mừng trở lại, {name}!";
            dashboardService = new DashboardService();

            LoadDashboardData();
            LoadWeeklyChart();
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

        private void LoadWeeklyChart()
        {
            // Lấy dữ liệu từ database
            var weeklyData = dashboardService.GetWeeklyRevenue();

            // Sắp xếp theo thứ tự ngày
            var sortedData = weeklyData.OrderBy(x => x.Key).ToList();

            // Chuẩn bị labels (T2, T3, T4...)
            WeekLabels = sortedData.Select(x => x.Key).ToList();

            // Chuẩn bị values (doanh thu từng ngày)
            var values = sortedData.Select(x => (double)x.Value).ToList();

            // Tính step cho trục Y (để dễ đọc)
            double maxValue = values.Count > 0 ? values.Max() : 0;
            YAxisStep = maxValue > 0 ? Math.Ceiling(maxValue / 5 / 100000) * 100000 : 100000;

            // Tạo series cho biểu đồ với màu sắc rõ ràng
            WeeklyRevenueSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "💰 Doanh thu",
                    Values = new ChartValues<double>(values),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 12,
                    Fill = new SolidColorBrush(Color.FromArgb(80, 255, 140, 0)),  // Cam trong suốt
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 140, 0)),     // Cam đậm
                    StrokeThickness = 4,
                    LineSmoothness = 0.3,  // Đường cong mượt hơn
                    DataLabels = true,     // Hiển thị số trên mỗi điểm
                    LabelPoint = point => (point.Y / 1000).ToString("N0") + "k"  // Format: 100k, 200k
                }
            };

            // Formatter cho trục Y (hiển thị số tiền)
            YFormatter = value => {
                if (value >= 1000000)
                    return (value / 1000000).ToString("N1") + "M";  // 1.5M, 2.0M
                else if (value >= 1000)
                    return (value / 1000).ToString("N0") + "k";     // 100k, 500k
                else
                    return value.ToString("N0");
            };
        }
    }
