using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pet_Shop_Project.Models
{
    public class Review : INotifyPropertyChanged
    {
        private string _reviewid;
        private string _productid;
        private string _userid;
        private int _rating;
        private string _comment;
        private DateTime _reviewdate;

        public string ReviewId 
        { 
            get => _reviewid;
            set
            {
                _reviewid = value;
                OnPropertyChanged(nameof(ReviewId));
            }

        }
        public string ProductId 
        { 
            get => _productid;
            set
            { 
                _productid = value;
                OnPropertyChanged(nameof(ProductId));
            } 
        }
        public string UserId
        { 
            get => _userid;
            set
            {
                _userid = value;
                OnPropertyChanged(nameof(UserId));
            } 
        }
        public int Rating 
        { 
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged(nameof(Rating));
            }
        }
        public string Comment 
        { 
            get => _comment; 
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }
        public DateTime ReviewDate 
        { 
            get => _reviewdate;
            set
            {
                _reviewdate = value;
                OnPropertyChanged(nameof(ReviewDate));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
