using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for OQPShipping.xaml
    /// </summary>
    public partial class OQPShipping : Page, INotifyPropertyChanged
    {
        public OQPShipping()
        {
            InitializeComponent();
            LoadShippingFromDB();
        }

        public ObservableCollection<Order> OrderShippings { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

        protected void LoadShippingFromDB()
        {

        }

    }
}
