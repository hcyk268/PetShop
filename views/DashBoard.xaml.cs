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

        // Constructor không tham số - sử dụng mặc định
        public DashBoard()
        {
            InitializeComponent();

            currentUserId = "Admin";
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

        // Constructor có tham số - nhận userId từ trang Login
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải thông tin: {ex.Message}");
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
        // Lấy dữ liệu 30 ngày
        var monthlyData = dashboardService.GetMonthlyRevenue();

        // Sắp xếp theo thứ tự ngày
        var sortedData = monthlyData.OrderBy(x => {
            var parts = x.Key.Split('/');
            return new DateTime(DateTime.Now.Year, int.Parse(parts[1]), int.Parse(parts[0]));
        }).ToList();

        // Labels: 01/12, 02/12, 03/12...
        WeekLabels = sortedData.Select(x => x.Key).ToList();

        // Values
        var values = sortedData.Select(x => (double)x.Value).ToList();

        // Tính step cho trục Y
        double maxValue = values.Count > 0 ? values.Max() : 0;
        YAxisStep = maxValue > 0 ? Math.Ceiling(maxValue / 5 / 100000) * 100000 : 100000;

        // Tạo series với DataLabels hiển thị giá trị
        WeeklyRevenueSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "💰 Doanh thu",
                Values = new ChartValues<double>(values),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 8,
                Fill = new SolidColorBrush(Color.FromArgb(80, 255, 140, 0)),
                Stroke = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                StrokeThickness = 3,
                LineSmoothness = 0.3,
                DataLabels = true,  // BẬT hiển thị label
                FontSize = 9,       // Font size nhỏ để gọn
                LabelPoint = point => {
                    // Chỉ hiển thị giá trị (ngày đã có ở trục X rồi)
                    if (point.Y >= 1000000)
                        return $"{(point.Y / 1000000):N1}M";
                    else if (point.Y >= 1000)
                        return $"{(point.Y / 1000):N0}k";
                    else if (point.Y > 0)
                        return $"{point.Y:N0}";
                    else
                        return ""; // Không hiện gì nếu = 0
                }
            }
        };

        // Formatter cho trục Y
        YFormatter = value => {
            if (value >= 1000000)
                return (value / 1000000).ToString("N1") + "M";
            else if (value >= 1000)
                return (value / 1000).ToString("N0") + "k";
            else
                return value.ToString("N0");
        };
    }
}