using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Models
{
    public class Product : INotifyPropertyChanged
    {
        private int _productId;
        private string _name;
        private string _description;
        private decimal _unitPrice;
        private int _unitInStock;
        private double _discount;
        private string _picture;

        public int ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(ProductId));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        public int UnitInStock
        {
            get => _unitInStock;
            set
            {
                _unitInStock = value;
                OnPropertyChanged(nameof(UnitInStock));
            }
        }

        public double Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }

        public string Picture
        {
            get => _picture;
            set
            {
                _picture = value;
                OnPropertyChanged(nameof(Picture));
            }
        }

        public decimal FinalPrice => UnitPrice * (decimal)(1 - Discount);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
