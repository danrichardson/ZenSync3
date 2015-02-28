using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ZenSync
{
    class PasswordToStarConverter: IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // Do the conversion from bool to visibility
            var result = String.Empty;
            var length = value.ToString().Length;
            if (length == 0)
                return result;
            result = value.ToString()[0].ToString();
            for (int i = 1; i < value.ToString().Length; i++)
            {
                result += "*";
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            return value;
        }
    }
}
