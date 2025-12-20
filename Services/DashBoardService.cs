using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
namespace Pet_Shop_Project.Services
{
    public class DashboardService
    {
        private readonly string connectionString;

        public DashboardService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        }

        public int GetTotalUsers()
        {
            string query = "SELECT COUNT(*) FROM USERS WHERE Role = 'User'";
            return ExecuteScalarInt(query);
        }

        public int GetTotalProducts()
        {
            string query = "SELECT COUNT(*) FROM PRODUCTS";
            return ExecuteScalarInt(query);
        }

        public int GetTotalOrders()
        {
            string query = "SELECT COUNT(*) FROM ORDERS WHERE ShippingStatus = 'Delivered'";
            return ExecuteScalarInt(query);
        }

        public decimal GetTotalRevenue()
        {
            string query = "SELECT SUM(TotalAmount) FROM ORDERS WHERE PaymentStatus = 'Paid'";
            return ExecuteScalarDecimal(query);
        }

        public List<string> GetBestSellers()
        {
            string query = @"
        SELECT 
            TOP 3 p.Name   
        FROM 
            ORDER_DETAILS od        
        JOIN 
            PRODUCTS p ON od.ProductID = p.ProductID  
        GROUP BY 
            p.Name
        ORDER BY 
            SUM(od.Quantity) DESC  ";
            return ExecuteList(query);
        }

        public List<string> GetWorstSellers()
        {
            string query = @"
        SELECT 
            TOP 3 p.Name   
        FROM 
            ORDER_DETAILS od        
        JOIN 
            PRODUCTS p ON od.ProductID = p.ProductID  
        GROUP BY 
            p.Name
        ORDER BY 
            SUM(od.Quantity) ASC  ";
            return ExecuteList(query);
        }

        // ===== THÊM MỚI: Lấy doanh thu 7 ngày gần nhất =====
        public Dictionary<string, decimal> GetWeeklyRevenue()
        {
            var result = new Dictionary<string, decimal>();

            string query = @"
                SELECT 
                    CONVERT(DATE, OrderDate) as OrderDay,
                    SUM(TotalAmount) as DailyRevenue
                FROM ORDERS
                WHERE PaymentStatus = 'Paid' 
                    AND OrderDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                GROUP BY CONVERT(DATE, OrderDate)
                ORDER BY OrderDay";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    DateTime day = rd.GetDateTime(0);
                    decimal revenue = rd.GetDecimal(1);

                    // Format: "T2\n01/12"
                    string dayLabel = GetDayLabel(day);
                    result[dayLabel] = revenue;
                }
            }

            // Đảm bảo có đủ 7 ngày (nếu ngày nào không có đơn thì revenue = 0)
            EnsureSevenDays(result);

            return result;
        }

        private string GetDayLabel(DateTime date)
        {
            // Trả về label theo định dạng Việt Nam: T2, T3, T4...
            var dayNames = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday, "T2" },
                { DayOfWeek.Tuesday, "T3" },
                { DayOfWeek.Wednesday, "T4" },
                { DayOfWeek.Thursday, "T5" },
                { DayOfWeek.Friday, "T6" },
                { DayOfWeek.Saturday, "T7" },
                { DayOfWeek.Sunday, "CN" }
            };

            return $"{dayNames[date.DayOfWeek]}\n{date:dd/MM}";
        }

        private void EnsureSevenDays(Dictionary<string, decimal> data)
        {
            // Nếu thiếu ngày nào thì thêm vào với revenue = 0
            for (int i = 6; i >= 0; i--)
            {
                DateTime day = DateTime.Today.AddDays(-i);
                string label = GetDayLabel(day);

                if (!data.ContainsKey(label))
                {
                    data[label] = 0;
                }
            }
        }

        // ===================== HÀM DÙNG CHUNG ======================
        private int ExecuteScalarInt(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private decimal ExecuteScalarDecimal(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        private List<string> ExecuteList(string query)
        {
            List<string> list = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    list.Add(rd[0].ToString());
                }
            }
            return list;
        }
    }
}