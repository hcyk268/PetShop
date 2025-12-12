using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Pet_Shop_Project.Models;

namespace Pet_Shop_Project.Services
{
    public static class InventoryService
    {
        private static readonly string _conn = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        // Lấy tất cả hàng hóa trong kho
        public static async Task<List<InventoryItem>> GetAllInventoryAsync()
        {
            var items = new List<InventoryItem>();
            const string sql = @"SELECT ProductId, Name, Description, UnitPrice,
                                        UnitInStock, Discount, Picture, Category
                                FROM PRODUCTS
                                ORDER BY Name";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        var product = new Product
                        {
                            ProductId   = reader["ProductId"].ToString(),
                            Name        = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            UnitPrice   = (decimal)reader["UnitPrice"],
                            UnitInStock = (int)reader["UnitInStock"],
                            Discount    = (double)reader["Discount"],
                            Picture     = reader["Picture"].ToString(),
                            Category    = reader["Category"].ToString()
                        };

                        var item = new InventoryItem
                        {
                            
                            InventoryId     = product.ProductId,
                            ProductId       = product.ProductId,
                            ProductCode     = product.ProductId,
                            ProductName     = product.Name,
                            Category        = product.Category,
                            SellingPrice    = product.UnitPrice,
                            CostPrice       = product.UnitPrice * (decimal)(1 - product.Discount),
                            StockQuantity   = product.UnitInStock,   // tồn kho lấy từ UnitInStock
                            OrderedQuantity = 0,                     // không có cột, gán 0
                            MinStockLevel   = 0,                     // không có cột, gán 0
                            MaxStockLevel   = product.UnitInStock,   // gán tạm
                            LastUpdated     = DateTime.Now,
                            Product         = product
                        };

                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tải danh sách kho hàng: " + ex.Message);
            }

            return items;
        }

        // Thêm sản phẩm vào kho
        public static async Task<bool> AddInventoryItemAsync(InventoryItem item)
        {
            const string sql = @"UPDATE PRODUCTS
                                SET UnitInStock = @StockQuantity
                                WHERE ProductId = @ProductId";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@StockQuantity", item.StockQuantity);
                        return await cmd.ExecuteNonQueryAsync() > 0;                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm sản phẩm vào kho: " + ex.Message);
            }
        }

        // Cập nhật thông tin kho
        public static async Task<bool> UpdateInventoryItemAsync(InventoryItem item)
        {
            const string sql = @"UPDATE PRODUCTS
                                 SET UnitInStock = @StockQuantity
                                 WHERE ProductId = @ProductId";
                

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@StockQuantity", item.StockQuantity);
                        return await cmd.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật thông tin kho: " + ex.Message);
            }
        }

        // Xóa sản phẩm khỏi kho
        public static async Task<bool> DeleteInventoryItemAsync(string productId)
        {
            const string sql = @"
            BEGIN TRAN
                DELETE FROM REVIEWS      WHERE ProductId = @ProductId;
                DELETE FROM ORDER_DETAILS WHERE ProductId = @ProductId;
                DELETE FROM CART_ITEMS WHERE ProductId = @ProductId;
                DELETE FROM PRODUCTS   WHERE ProductId = @ProductId;
            COMMIT TRAN";
            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        return await cmd.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa sản phẩm khỏi kho: " + ex.Message);
            }
        }


        // Tìm kiếm theo mã hoặc tên
        public static async Task<List<InventoryItem>> SearchInventoryAsync(string searchTerm)
        {
            var items = new List<InventoryItem>();
            const string sql = @"SELECT ProductId, Name, Description, UnitPrice,
                                UnitInStock, Discount, Picture, Category
                                FROM PRODUCTS
                                WHERE ProductId LIKE @Search OR Name LIKE @Search
                                ORDER BY Name";

            try
            {
                using (var conn = new SqlConnection(_conn))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var product = new Product
                                {
                                    ProductId   = reader["ProductId"].ToString(),
                                    Name        = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice   = (decimal)reader["UnitPrice"],
                                    UnitInStock = (int)reader["UnitInStock"],
                                    Discount    = (double)reader["Discount"],
                                    Picture     = reader["Picture"].ToString(),
                                    Category    = reader["Category"].ToString()
                                };

                                var item = new InventoryItem
                                {
                                    // Không còn InventoryId riêng, dùng ProductId làm khóa tạm
                                    InventoryId     = product.ProductId,
                                    ProductId       = product.ProductId,
                                    ProductCode     = product.ProductId,
                                    ProductName     = product.Name,
                                    Category        = product.Category,
                                    SellingPrice    = product.UnitPrice,
                                    CostPrice       = product.UnitPrice * (decimal)(1 - product.Discount),
                                    StockQuantity   = product.UnitInStock,   // tồn kho lấy từ UnitInStock
                                    MaxStockLevel   = product.UnitInStock,   // gán tạm
                                    LastUpdated     = DateTime.Now,
                                    Product         = product
                                };

                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm: " + ex.Message);
            }

            return items;
        }
    }
}