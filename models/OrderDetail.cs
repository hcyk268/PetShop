using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Models
{
    public class OrderDetail : INotifyPropertyChanged
    {
        private string _orderDetailId;
        private string _orderId;
        private string _productId;
        private int _quantity;
        private Product _product;

        public string OrderDetailId
        {
            get => _orderDetailId;
            set
            {
                _orderDetailId = value;
                OnPropertyChanged(nameof(OrderDetailId));
            }
        }

        public string OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged(nameof(OrderId));
            }
        }

        public string ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(ProductId));
                OnPropertyChanged(nameof(SubTotal)); // SubTotal phụ thuộc Product
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(SubTotal)); // SubTotal phụ thuộc Quantity
            }
        }

        

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public decimal SubTotal => Quantity * Product.UnitPrice * (decimal)(1 - Product.Discount);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
