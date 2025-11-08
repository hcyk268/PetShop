using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Models
{
    public class Order : INotifyPropertyChanged
    {
        private string _orderId;
        private string _userId;
        private DateTime _orderDate;
        private decimal _totalAmount;
        private string _approvalStatus; // Waiting / Approved / Rejected
        private string _paymentStatus;  // Pending / Paid
        private string _shippingStatus; // Pending / Shipped / Delivered
        private string _address;
        private string _note;
        private ObservableCollection<OrderDetail> _details = new ObservableCollection<OrderDetail>();

        public string OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged(nameof(OrderId));
            }
        }

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        public DateTime OrderDate
        {
            get => _orderDate;
            set
            {
                _orderDate = value;
                OnPropertyChanged(nameof(OrderDate));
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public string ApprovalStatus
        {
            get => _approvalStatus;
            set
            {
                _approvalStatus = value;
                OnPropertyChanged(nameof(ApprovalStatus));
            }
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set
            {
                _paymentStatus = value;
                OnPropertyChanged(nameof(PaymentStatus));
            }
        }

        public string ShippingStatus
        {
            get => _shippingStatus;
            set
            {
                _shippingStatus = value;
                OnPropertyChanged(nameof(ShippingStatus));
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged(nameof(Note));
            }
        }

        public ObservableCollection<OrderDetail> Details
        {
            get => _details;
            set
            {
                _details = value;
                OnPropertyChanged(nameof(Details));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
