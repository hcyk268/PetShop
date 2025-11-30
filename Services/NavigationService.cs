using System;
using System.Windows.Controls;
using Pet_Shop_Project.Views;
using Pet_Shop_Project.Models;

namespace Pet_Shop_Project.Services
{
    public class NavigationService
    {
        private static NavigationService instance;
        private Frame mainFrame;

        private NavigationService() { }

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

        // Navigate về HomePage MỚI - Reset tất cả
        public void NavigateToHome()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new HomePage());
            }
        }

        // Navigate đến ProductDetail
        public void NavigateToProductDetail(Product product)
        {
            if (mainFrame != null)
            {
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
            mainFrame.Navigate(new OrderQueuePage());
        }

        public void NavigateToCart()
        {
            mainFrame.Navigate(new Cart());
        }
    }
}