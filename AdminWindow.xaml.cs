using Pet_Shop_Project.Views;
using System.Windows;
using Pet_Shop_Project.Services;

namespace Pet_Shop_Project
{
    public partial class AdminWindow : Window
    {
        public string userid { get; set; }

        public AdminWindow()
        {
            InitializeComponent();
        }
    }
}