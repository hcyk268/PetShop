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
        private int _orderDetailId;
        private int _productId;
        private int _quantity;
        private decimal _unitPrice;
        private double _discount;

        public int OrderDetailId
        {
            get => _orderDetailId;
            set
            {
                _orderDetailId = value;
                OnPropertyChanged(nameof(OrderDetailId));
            }
        }

        public int Product
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(Product));
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

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
                OnPropertyChanged(nameof(SubTotal)); // SubTotal phụ thuộc UnitPrice
            }
        }

        public double Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
                OnPropertyChanged(nameof(SubTotal)); // SubTotal phụ thuộc Discount
            }
        }

        public decimal SubTotal => Quantity * UnitPrice * (decimal)(1 - Discount);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
