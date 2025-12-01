using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Pet_Shop_Project.Models;


namespace Pet_Shop_Project.Services
{
    public class ProductService
    {
        private string connectionString;

        public ProductService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        }

        // Lấy tất cả sản phẩm
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ProductId, Name, Description, UnitPrice, UnitInStock, Discount, Picture, Category " +
                                 "FROM dbo.PRODUCTS WHERE UnitInStock > 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product
                                {
                                    ProductId = reader["ProductId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                    UnitInStock = Convert.ToInt32(reader["UnitInStock"]),
                                    Discount = reader["Discount"] != DBNull.Value ? Convert.ToDouble(reader["Discount"]) : 0,
                                    Picture = reader["Picture"].ToString(),
                                    Category = reader["Category"].ToString()
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi kết nối database: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return products;
        }

        // Lọc sản phẩm theo category
        public List<Product> GetProductsByCategory(string category)
        {
            List<Product> products = new List<Product>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ProductId, Name, Description, UnitPrice, UnitInStock, Discount, Picture, Category " +
                                 "FROM dbo.PRODUCTS " +
                                 "WHERE Category = @category AND UnitInStock > 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@category", category);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product
                                {
                                    ProductId = reader["ProductId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                    UnitInStock = Convert.ToInt32(reader["UnitInStock"]),
                                    Discount = reader["Discount"] != DBNull.Value ? Convert.ToDouble(reader["Discount"]) : 0,
                                    Picture = reader["Picture"].ToString(),
                                    Category = reader["Category"].ToString()
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi lọc theo danh mục: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return products;
        }

        // Tìm kiếm sản phẩm theo tên
        public List<Product> SearchProducts(string keyword)
        {
            List<Product> products = new List<Product>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ProductId, Name, Description, UnitPrice, UnitInStock, Discount, Picture, Category " +
                                 "FROM dbo.PRODUCTS " +
                                 "WHERE Name LIKE @keyword AND UnitInStock > 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product
                                {
                                    ProductId = reader["ProductId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                    UnitInStock = Convert.ToInt32(reader["UnitInStock"]),
                                    Discount = reader["Discount"] != DBNull.Value ? Convert.ToDouble(reader["Discount"]) : 0,
                                    Picture = reader["Picture"].ToString(),
                                    Category = reader["Category"].ToString()
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return products;
        }
    }
}