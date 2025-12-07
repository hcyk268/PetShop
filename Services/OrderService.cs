using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace Pet_Shop_Project.Services
{
    public class OrderService
    {
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;

        public async Task<ObservableCollection<Order>> GetOrdersByUser(string userId = null)
        {
            var orders = new ObservableCollection<Order>();
            var orderLookup = new Dictionary<string, Order>();

            try
            {
                using (var conn = new SqlConnection(_connectionDB))
                {
                    await conn.OpenAsync();

                    var sql = @"
                            SELECT *
                            FROM ORDERS o
                            JOIN ORDER_DETAILS od ON o.OrderId = od.OrderId
                            JOIN PRODUCTS p ON p.ProductId = od.ProductId
                           " + (userId != null ? "WHERE o.UserId = @userId" : "");

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        if (userId != null) cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var orderId = reader["OrderId"].ToString();

                                if (!orderLookup.TryGetValue(orderId, out var order))
                                {
                                    order = new Order
                                    {
                                        OrderId = orderId,
                                        UserId = reader["UserId"].ToString(),
                                        OrderDate = (DateTime)reader["OrderDate"],
                                        TotalAmount = (decimal)reader["TotalAmount"],
                                        ApprovalStatus = reader["ApprovalStatus"].ToString(),
                                        PaymentStatus = reader["PaymentStatus"].ToString(),
                                        ShippingStatus = reader["ShippingStatus"].ToString(),
                                        Address = reader["Address"].ToString(),
                                        Note = reader["Note"].ToString(),
                                        Details = new ObservableCollection<OrderDetail>()
                                    };
                                    orders.Add(order);
                                    orderLookup[orderId] = order;
                                }

                                order.Details.Add(
                                    new OrderDetail
                                    {
                                        OrderDetailId = reader["OrderDetailId"].ToString(),
                                        OrderId = orderId,
                                        ProductId = reader["ProductId"].ToString(),
                                        Quantity = (int)reader["Quantity"],
                                        Product = new Product
                                        {
                                            ProductId = reader["ProductId"].ToString(),
                                            Name = reader["Name"].ToString(),
                                            Description = reader["Description"].ToString(),
                                            UnitPrice = (decimal)reader["UnitPrice"],
                                            UnitInStock = (int)reader["UnitInStock"],
                                            Discount = reader["Discount"] != DBNull.Value ? Convert.ToDouble(reader["Discount"]) : 0,
                                            Picture = reader["Picture"].ToString()
                                        }
                                    }
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Khong the tai danh sach don hang: {ex.Message}", "Loi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return orders;
        }
    }
}
