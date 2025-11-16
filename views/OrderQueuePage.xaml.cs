using Pet_Shop_Project.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Interaction logic for OrderQueuePage.xaml
    /// </summary>
    public partial class OrderQueuePage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Order> _allOrders;

        private bool _active = false;

        private string _connectionDB = "Data Source=HAI\\SQLEXPRESS;Initial Catalog=PETSHOP;Integrated Security=True;";

        
        public OrderQueuePage()
        {
            InitializeComponent();
            AllOrders = new ObservableCollection<Order>();
            GetDataFromDB();
            setForeColorDefault();
            odppendingbutton.Foreground = clickedtext;
            MainScreenOQP.Navigate(new OQPPendingApproval(AllOrders));
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

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged(nameof(Active));
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

                        string orderid = reader["OrderId"].ToString();
                        Order order = null;
                        foreach (var check in AllOrders)
                        {
                            if (check.OrderId == orderid)
                            {
                                order = check; 
                                break;
                            }
                        }    

                        if (order == null)
                        {
                            order = new Order
                            {
                                OrderId = orderid,
                                UserId = reader["UserId"].ToString(),
                                OrderDate = (DateTime)reader["OrderDate"],
                                TotalAmount = (decimal)reader["TotalAmount"],
                                ApprovalStatus = reader["ApprovalStatus"].ToString(),
                                PaymentStatus = reader["PaymentStatus"].ToString(),
                                ShippingStatus = reader["ShippingStatus"].ToString(),
                                Address = reader["Address"].ToString(),
                                Note = reader["Note"].ToString(),
                                Details = new ObservableCollection<OrderDetail>()
                            };
                            AllOrders.Add(order);
                        }


                        order.Details.Add(
                            new OrderDetail
                            {
                                OrderDetailId = reader["OrderDetailId"].ToString(),
                                OrderId = orderid,
                                ProductId = reader["ProductId"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                Product = new Product
                                {
                                    ProductId = reader["ProductId"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                    UnitInStock = (int)reader["UnitInStock"],
                                    Discount = (double)reader["Discount"],
                                    Picture = reader["Picture"].ToString()
                                }
                            }
                        );
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

        SolidColorBrush defaulttext = (SolidColorBrush)(new BrushConverter().ConvertFrom("#222")); 
        SolidColorBrush clickedtext = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF6B6B"));
        protected void setForeColorDefault()
        {
            odppendingbutton.Foreground = odpshippingbutton.Foreground = odpsuccessbutton.Foreground = odpcanceledbutton.Foreground = defaulttext;
        }
        private void odppendingbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odppendingbutton.Foreground = clickedtext;
            MainScreenOQP.Navigate(new OQPPendingApproval(AllOrders));
        }

        private void odpshippingbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odpshippingbutton.Foreground = clickedtext;
            MainScreenOQP.Navigate(new OQPShipping(AllOrders));
        }

        private void odpsuccessbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odpsuccessbutton.Foreground = clickedtext;
            MainScreenOQP.Navigate(new OQPSuccess(AllOrders));
        }

        private void odpcanceledbutton_Click(object sender, RoutedEventArgs e)
        {
            setForeColorDefault();
            odpcanceledbutton.Foreground = clickedtext;
            MainScreenOQP.Navigate(new OQPCanceled(AllOrders));
        }
    }
}
