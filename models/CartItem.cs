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
        private bool isSelected; // Hien them
        private string _imageUrl; // Hien them
        private string _cartItemId;
        private string _productId;
        private int _quantity;
        private Product _product;
        private string _variant;

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

        public bool IsSelected
        {
            get => isSelected;
            set 
            { 
                isSelected = value; OnPropertyChanged(nameof(IsSelected)); 
            }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                _imageUrl = value;
                OnPropertyChanged(nameof(ImageUrl));
            }
        }

        // 👇 Tham chiếu đến Product
        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(SubTotal));
            }
        }

        public string Variant
        {
            get => _variant;
            set
            {
                _variant = value;
                OnPropertyChanged(nameof(Variant));
            }
        }
        public string Name => Product?.Name ?? "N/A";
        public decimal Price => Product?.FinalPrice ?? 0;
        public decimal SubTotal => Quantity * Price;

        // Event và method từ file gốc (GIỮ NGUYÊN)
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
