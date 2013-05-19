using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CPC.Converter
{
    class DiscountRateToIntegerConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return "$";
            if (value.ToString().Equals("0"))
                return "$";
            else
                return "%";
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return 0;
            if ("$".Equals(value.ToString()))
                return 0;
            else
                return 1;
        }
    }
}
