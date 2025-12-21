using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Pet_Shop_Project.Views;
using Pet_Shop_Project.Models;

namespace Pet_Shop_Project.Services
{
    public class NavigationService
    {
        private static NavigationService instance;
        private Frame mainFrame;

        // Cache các page để tránh tạo mới liên tục
        private Dictionary<string, Page> pageCache = new Dictionary<string, Page>();

        private NavigationService() { }

        public string userid { get; private set; }

        public void setUserId(string userid)
        {
            this.userid = userid;

            // Clear cache khi user thay đổi
            ClearCache();
        }

        public static NavigationService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NavigationService();
                }
                return instance;
            }
        }

        public void Initialize(Frame frame)
        {
            mainFrame = frame;
        }

        // Clear cache để tạo page mới
        public void ClearCache()
        {
            pageCache.Clear();
        }

        // Navigate về HomePage MỚI - Reset tất cả
        public void NavigateToHome()
        {
            if (mainFrame != null)
            {
                // Luôn tạo HomePage mới để refresh data
                var homePage = new HomePage();
                pageCache["Home"] = homePage;
                mainFrame.Navigate(homePage);
            }
        }

        // Navigate đến ProductDetail
        public void NavigateToProductDetail(Product product)
        {
            if (mainFrame != null)
            {
                // ProductDetail luôn tạo mới vì mỗi product khác nhau
                mainFrame.Navigate(new ProductDetailPage(product));
            }
        }

        // Quay lại page trước đó - GIỮ NGUYÊN trạng thái
        public void GoBack()
        {
            if (mainFrame != null && mainFrame.CanGoBack)
            {
                mainFrame.GoBack();
            }
        }

        public void NavigateToOrder()
        {
            if (mainFrame != null)
            {
                string cacheKey = $"Order_{userid}";

                // Kiểm tra cache
                if (!pageCache.ContainsKey(cacheKey))
                {
                    pageCache[cacheKey] = new OrderQueuePage(userid);
                }

                mainFrame.Navigate(pageCache[cacheKey]);
            }
        }

        public void NavigateToCart()
        {
            if (mainFrame != null)
            {
                // Cart nên được refresh mỗi lần navigate để có data mới nhất
                var cartPage = new CartPage(userid);
                pageCache[$"Cart_{userid}"] = cartPage;
                mainFrame.Navigate(cartPage);
            }
        }

        public void NavigateToAccount()
        {
            if (mainFrame != null)
            {
                string cacheKey = $"Account_{userid}";

                // Account có thể cache
                if (!pageCache.ContainsKey(cacheKey))
                {
                    pageCache[cacheKey] = new AccountPage(userid);
                }

                mainFrame.Navigate(pageCache[cacheKey]);
            }
        }

        // Method để force refresh một page cụ thể
        public void RefreshPage(string pageType)
        {
            string cacheKey = $"{pageType}_{userid}";

            if (pageCache.ContainsKey(cacheKey))
            {
                pageCache.Remove(cacheKey);
            }
        }

        // Method để refresh OrderQueuePage data
        public async void RefreshOrderPage()
        {
            string cacheKey = $"Order_{userid}";

            if (pageCache.ContainsKey(cacheKey) && pageCache[cacheKey] is OrderQueuePage orderPage)
            {
                await orderPage.RefreshOrdersAsync();
            }
        }
    }
}