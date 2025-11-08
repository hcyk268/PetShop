using Pet_Shop_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for OrderQueuePage.xaml
    /// </summary>
    public partial class OrderQueuePage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _allOrders;

        private string _connectionDB = "Data Source=HAI\\SQLEXPRESS;Initial Catalog=PETSHOP;Integrated Security=True;";
        public OrderQueuePage()
        {
            InitializeComponent();
            GetDataFromDB();
        }

        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set
            {
                _allOrders = value;
                OnPropertyChanged(nameof(AllOrders));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void GetDataFromDB()
        {
            using (SqlConnection conn = new SqlConnection(_connectionDB))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Connected Successfully");

                    string query = @"
                        SELECT *
                        FROM ORDERS o
                        INNER JOIN ORDER_DETAILS od ON o.OrderId = od.OrderId
                        INNER JOIN PRODUCTS p ON p.ProductId = od.ProductId
                    "; //Chua Them Du Lieu Cho Dung Voi UserId => Them WHERE o.UserId = ... => Truyen constructor
                    
                    SqlCommand cmd = new SqlCommand(query, conn);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["OrderId"]} - {reader["OrderDate"]} - {reader["TotalAmount"]} " +
                            $"- {reader["ApprovalStatus"]} - {reader["PaymentStatus"]} - {reader["ShippingStatus"]} " +
                            $"- {reader["Address"]} - {reader["Quantity"]} - {reader["Name"]} - {reader["Picture"]}");

                        //var order = new Order(
                        //    OderId = 
                        //);
                    }
                    reader.Close();
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("Connected UnSuccessfully");
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

    }

}
