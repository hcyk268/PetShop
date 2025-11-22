using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Pet_Shop_Project.Services
{
    public class ConvertLanguage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Không Xác Định";

            string v = value.ToString().ToLower();

            //Pending / Paid
            //Pending / Shipped / Delivered
            //Waiting / Approved / Rejected

            if (v == "pending" || v == "waiting")
                return "Đang Chờ";
            else if (v == "paid")
                return "Đã Thanh Toán";
            else if (v == "shipped")
                return "Đang Giao";
            else if (v == "delivered")
                return "Đã Giao";
            else if (v == "approved")
                return "Đã Duyệt";
            else if (v == "rejected")
                return "Bị Từ Chối";
            return "Không Xác Định";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
