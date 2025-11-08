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
    /// Interaction logic for OQPCanceled.xaml
    /// </summary>
    public partial class OQPCanceled : Page, INotifyPropertyChanged
    {
        public OQPCanceled()
        {
            InitializeComponent();
            LoadCanceledFromDB();
        }

        public ObservableCollection<Order> OrderCanceled { set; get; }

        private int _ToTalCanceled;

        public int ToTalCanceled
        {
            get => _ToTalCanceled;
            set
            {
                _ToTalCanceled = value;
                OnPropertyChanged(nameof(ToTalCanceled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

        protected void LoadCanceledFromDB()
        {

        }
    }
}
