using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Models
{
    public class Shipment : INotifyPropertyChanged
    {
        private int _shipmentId;
        private int _orderId;
        private DateTime _shipDate;
        private string _status; // Shipping / Delivered

        public int ShipmentId
        {
            get => _shipmentId;
            set
            {
                _shipmentId = value;
                OnPropertyChanged(nameof(ShipmentId));
            }
        }

        public int OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged(nameof(OrderId));
            }
        }

        public DateTime ShipDate
        {
            get => _shipDate;
            set
            {
                _shipDate = value;
                OnPropertyChanged(nameof(ShipDate));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
