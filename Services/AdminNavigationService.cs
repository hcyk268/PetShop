using Pet_Shop_Project.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Pet_Shop_Project.Services
{
    public class AdminNavigationService
    {
        private static AdminNavigationService instance;
        private Frame mainFrame;
        public static AdminNavigationService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AdminNavigationService();
                }
                return instance;
            }
        }
        public void Initialize(Frame frame)
        {
            mainFrame = frame;
        }

        public void NavigateToDashBoard()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new DashBoard());
            }
        }

        public void NavigateToInventory()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new AdminInventory());
            }
        }

        public void NavigateToUser()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new UserManagement());
            }
        }

        public void NavigateToOrder()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new AdminOrder());
            }
        }
        public void NavigateToReview()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new AdminReviewPage());
            }
        }

        public void NavigateToCreateOrder()
        {
            if (mainFrame != null)
            {
                mainFrame.Navigate(new AdminCreateOrder());
            }
        }
    }
}
