using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Pet_Shop_Project.Models;

namespace Pet_Shop_Project.Services
{
    public class ReviewService
    {
        private string connectionString;

        public ReviewService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        }

        // Lấy tất cả reviews của một sản phẩm
        public async Task<List<Review>> GetReviewsByProductIdAsync(string productId)
        {
            List<Review> reviews = new List<Review>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT ReviewId, ProductId, UserId, Rating, Comment, ReviewDate
                                   FROM dbo.REVIEWS
                                   WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Review review = new Review
                                {
                                    ReviewId = reader["ReviewId"].ToString(),
                                    ProductId = reader["ProductId"].ToString(),
                                    UserId = reader["UserId"].ToString(),
                                    Rating = Convert.ToInt32(reader["Rating"]),
                                    Comment = reader["Comment"].ToString(),
                                    ReviewDate = Convert.ToDateTime(reader["ReviewDate"])
                                };
                                reviews.Add(review);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lấy reviews: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return reviews;
        }

        // Tính rating trung bình của một sản phẩm
        public async Task<double> GetAverageRatingAsync(string productId)
        {
            double averageRating = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT AVG(CAST(Rating AS FLOAT)) FROM dbo.REVIEWS WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        var result = await cmd.ExecuteScalarAsync();

                        if (result != DBNull.Value && result != null)
                        {
                            averageRating = Convert.ToDouble(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tính rating: {ex.Message}");
            }

            return averageRating;
        }

        // Đếm số lượng reviews của một sản phẩm
        public async Task<int> GetReviewCountAsync(string productId)
        {
            int count = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT COUNT(*) FROM dbo.REVIEWS WHERE ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đếm reviews: {ex.Message}");
            }

            return count;
        }

        // Lấy thông tin user từ review
        public async Task<string> GetUserFullNameAsync(string userId)
        {
            string fullName = "Người dùng";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT FullName FROM dbo.USERS WHERE UserId = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        var result = await cmd.ExecuteScalarAsync();

                        if (result != null)
                        {
                            fullName = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy tên user: {ex.Message}");
            }

            return fullName;
        }

        // Thêm review mới
        public async Task<bool> AddReviewAsync(string productId, string userId, int rating, string comment)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO dbo.REVIEWS (ProductId, UserId, Rating, Comment, ReviewDate)
                                   VALUES (@ProductId, @UserId, @Rating, @Comment, @ReviewDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Rating", rating);
                        cmd.Parameters.AddWithValue("@Comment", comment);
                        cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi thêm review: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // Lấy tất cả reviews
        public async Task<List<Review>> GetAllReviewsAsync()
        {
            List<Review> reviews = new List<Review>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT ReviewId, ProductId, UserId, Rating, Comment, ReviewDate
                                    FROM dbo.REVIEWS
                                    ORDER BY ReviewDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Review review = new Review
                                {
                                    ReviewId = reader["ReviewId"].ToString(),
                                    ProductId = reader["ProductId"].ToString(),
                                    UserId = reader["UserId"].ToString(),
                                    Rating = Convert.ToInt32(reader["Rating"]),
                                    Comment = reader["Comment"].ToString(),
                                    ReviewDate = Convert.ToDateTime(reader["ReviewDate"])
                                };
                                reviews.Add(review);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lấy danh sách reviews: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return reviews;
        }

        // Xóa review theo ReviewId
        public async Task<bool> DeleteReviewAsync(string reviewId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM dbo.REVIEWS WHERE ReviewId = @ReviewId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReviewId", reviewId);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi xóa review: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // Lấy danh sách sản phẩm
        public async Task<List<Product>> GetAllProductsForSelectionAsync()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT ProductId, Name, Picture FROM dbo.PRODUCTS ORDER BY Name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Product product = new Product
                                {
                                    ProductId = reader["ProductId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Picture = reader["Picture"]?.ToString() ?? ""
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy danh sách sản phẩm: {ex.Message}");
            }

            return products;
        }
    }
}

