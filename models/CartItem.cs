using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Models
{

    public class CartItem : INotifyPropertyChanged
    {
        private string _cartItemId;
        private string _productId;
        private int _quantity;

        public string CartItemId
        {
            get => _cartItemId;
            set
            {
                _cartItemId = value;
                OnPropertyChanged(nameof(CartItemId));
            }
        }

        public string ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(ProductId));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
