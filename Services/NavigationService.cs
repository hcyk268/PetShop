using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Pet_Shop_Project.Views;

namespace Pet_Shop_Project.Services
{
    public class NavigationService
    {
        private Frame mainFrame;
        private static NavigationService instance;

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

        public void NavigateToHome()
        {
            mainFrame.Navigate(new HomePage());
        }

        public void NavigateToOrder()
        {
            mainFrame.Navigate(new OrderQueuePage());
        }
    }
}
