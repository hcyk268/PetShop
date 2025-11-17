using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
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
    /// Interaction logic for OQPPendingApproval.xaml
    /// </summary>
    public partial class OQPPendingApproval : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orderPendings;
        private ObservableCollection<Order> _allOrders;
        private readonly string _connectionDB = ConfigurationManager.ConnectionStrings["PetShopDB"].ConnectionString;
        public OQPPendingApproval(ObservableCollection<Order> allOrders)
        {
            InitializeComponent();
            _allOrders = allOrders;
            OrderPendings = new ObservableCollection<Order>();
            FilterOrders();
            _allOrders.CollectionChanged += (s, e) => FilterOrders();
            DataContext = this;
        }
        public ObservableCollection<Order> OrderPendings 
        {
            get => _orderPendings;
            set
            {
                _orderPendings = value;
                OnPropertyChanged(nameof(OrderPendings));
            }
        }

        protected void FilterOrders()
        {
            OrderPendings.Clear();
            foreach (var order in _allOrders)
                if (order.ApprovalStatus == "Waiting")
                    OrderPendings.Add(order);
            
            OnPropertyChanged(nameof(TotalOrderPending));
        }

        public int TotalOrderPending => _orderPendings.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nameProperty)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProperty));

        private void buttondeleteorder_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(
                "Bạn có thực sự muốn xóa bỏ không?",
                "Vui Lòng Xác Nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (messageBoxResult == MessageBoxResult.No) return;
            var btn = sender as Button;
            string ordid = btn.Tag.ToString();
            var ord = _allOrders.FirstOrDefault(x => x.OrderId == ordid);
            if (ord != null)
            {
                bool check = true;

                using (SqlConnection conn = new SqlConnection(_connectionDB))
                {
                    try
                    {
                        conn.Open();
                        Console.WriteLine("Connected Successfully");

                        string query = @"
                            DELETE FROM SHIPMENTS
                            WHERE OrderId = @OrderId;

                            DELETE FROM ORDER_DETAILS
                            WHERE OrderId = @OrderId;

                            DELETE FROM ORDERS
                            WHERE OrderId = @OrderId;
                        ";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@OrderId", ordid);

                        cmd.ExecuteNonQuery();

                        Console.WriteLine("Delete success");
                    }
                    catch (Exception ex)
                    {
                        check = false;
                        Console.WriteLine("Connected UnSuccessfully Or Delete Error");
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }

                if (check)
                {
                    _allOrders.Remove(ord);
                }
                else
                {
                    MessageBox.Show("Xóa Không Thành Công:((");
                }
            }
        }
    }
}
