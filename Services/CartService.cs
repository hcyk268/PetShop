using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Pet_Shop_Project.Models;

namespace Pet_Shop_Project.Models
{
    public static class CartService
    {
        public static ObservableCollection<CartItem> CartItems { get; } = new ObservableCollection<CartItem>();

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        public static void AddToCart(Product product, int quantity = 1, string variant = null)
        {
            // Kiểm tra sản phẩm đã có trong giỏ chưa (cùng ProductId và Variant)
            var existingItem = CartItems.FirstOrDefault(i =>
                i.ProductId == product.ProductId &&
                i.Variant == variant);

            if (existingItem != null)
            {
                // Tăng số lượng nếu đã có
                existingItem.Quantity += quantity;
            }
            else
            {
                // Thêm mới nếu chưa có
                var newItem = new CartItem
                {
                    CartItemId = Guid.NewGuid().ToString(),
                    ProductId = product.ProductId,
                    Product = product, // 👈 Liên kết với Product
                    Quantity = quantity,
                    Variant = variant,
                    IsSelected = false
                };
                CartItems.Add(newItem);
            }
        }

        /// <summary>
        /// Thêm CartItem trực tiếp (dùng khi đã có CartItem)
        /// </summary>
        public static void AddToCart(CartItem item)
        {
            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var existingItem = CartItems.FirstOrDefault(i =>
                i.ProductId == item.ProductId &&
                i.Variant == item.Variant);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                CartItems.Add(item);
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        public static void RemoveFromCart(CartItem item)
        {
            CartItems.Remove(item);
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public static void ClearCart()
        {
            CartItems.Clear();
        }

        /// <summary>
        /// Tính tổng giá trị giỏ hàng (chỉ các item được chọn)
        /// </summary>
        public static decimal GetTotalAmount()
        {
            return CartItems.Where(i => i.IsSelected).Sum(i => i.SubTotal);
        }

        /// <summary>
        /// Lấy danh sách các item đã được chọn
        /// </summary>
        public static List<CartItem> GetSelectedItems()
        {
            return CartItems.Where(i => i.IsSelected).ToList();
        }
    }
}