using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace CPC.POSReport.Function.Converter
{
    public class TrueFalseConverter : IValueConverter
    {
        // ByteArray To ImageSource
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return !(bool)value;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return !(bool)value;
            }
            return false;
        }
    }
}
