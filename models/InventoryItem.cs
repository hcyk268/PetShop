using System;
using System.ComponentModel;

namespace Pet_Shop_Project.Models
{
    public class InventoryItem : INotifyPropertyChanged
    {
        private string _inventoryId;
        private string _productId;
        private string _productCode;
        private string _productName;
        private string _category;
        private decimal _sellingPrice;
        private decimal _costPrice;
        private int _stockQuantity;
        private int _orderedQuantity;
        private int _minStockLevel;
        private int _maxStockLevel;
        private string _serialIMEI;
        private DateTime _lastUpdated;
        private Product _product;

        public string InventoryId
        {
            get => _inventoryId;
            set
            {
                _inventoryId = value;
                OnPropertyChanged(nameof(InventoryId));
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

        public string ProductCode
        {
            get => _productCode;
            set
            {
                _productCode = value;
                OnPropertyChanged(nameof(ProductCode));
            }
        }

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged(nameof(ProductName));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public decimal SellingPrice
        {
            get => _sellingPrice;
            set
            {
                _sellingPrice = value;
                OnPropertyChanged(nameof(SellingPrice));
            }
        }

        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                _costPrice = value;
                OnPropertyChanged(nameof(CostPrice));
            }
        }

        public int StockQuantity
        {
            get => _stockQuantity;
            set
            {
                _stockQuantity = value;
                OnPropertyChanged(nameof(StockQuantity));
                OnPropertyChanged(nameof(StockStatus));
                OnPropertyChanged(nameof(IsNegative));
            }
        }

        public int OrderedQuantity
        {
            get => _orderedQuantity;
            set
            {
                _orderedQuantity = value;
                OnPropertyChanged(nameof(OrderedQuantity));
            }
        }

        public int MinStockLevel
        {
            get => _minStockLevel;
            set
            {
                _minStockLevel = value;
                OnPropertyChanged(nameof(MinStockLevel));
                OnPropertyChanged(nameof(StockStatus));
            }
        }

        public int MaxStockLevel
        {
            get => _maxStockLevel;
            set
            {
                _maxStockLevel = value;
                OnPropertyChanged(nameof(MaxStockLevel));
                OnPropertyChanged(nameof(StockStatus));
            }
        }

        public string SerialIMEI
        {
            get => _serialIMEI;
            set
            {
                _serialIMEI = value;
                OnPropertyChanged(nameof(SerialIMEI));
            }
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                OnPropertyChanged(nameof(LastUpdated));
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

        // Computed properties
        public string StockStatus
        {
            get
            {
                if (StockQuantity == 0) return "Hết hàng";
                if (StockQuantity < MinStockLevel) return "Dưới định mức";
                if (StockQuantity > MaxStockLevel) return "Vượt định mức";
                return "Còn hàng";
            }
        }

        public bool IsNegative => StockQuantity < MinStockLevel;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}