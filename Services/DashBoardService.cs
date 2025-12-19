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
                    SUM(od.Quantity) DESC";
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
                    SUM(od.Quantity) ASC";
            return ExecuteList(query);
        }

        // ===== Lấy doanh thu 30 ngày gần nhất =====
        public Dictionary<string, decimal> GetMonthlyRevenue()
        {
            var result = new Dictionary<string, decimal>();

            string query = @"
                SELECT 
                    CONVERT(DATE, OrderDate) as OrderDay,
                    SUM(TotalAmount) as DailyRevenue
                FROM ORDERS
                WHERE PaymentStatus = 'Paid' 
                    AND OrderDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
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

                    // Format: "01/12"
                    string dayLabel = day.ToString("dd/MM");
                    result[dayLabel] = revenue;
                }
            }

            // Đảm bảo có đủ 30 ngày
            EnsureThirtyDays(result);

            return result;
        }

        private void EnsureThirtyDays(Dictionary<string, decimal> data)
        {
            // Thêm 30 ngày gần nhất, nếu ngày nào không có dữ liệu thì để revenue = 0
            for (int i = 29; i >= 0; i--)
            {
                DateTime day = DateTime.Today.AddDays(-i);
                string label = day.ToString("dd/MM");

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