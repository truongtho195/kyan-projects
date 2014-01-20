using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CPC.POS;

namespace CPC.Converter
{
    class DiscountRateToIntegerConverter:IValueConverter
    {
        /// <summary>
        /// 0: %
        /// 1: $
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return Define.CONFIGURATION.CurrencySymbol;
            
            if (value.ToString().Equals("1"))
                return Define.CONFIGURATION.CurrencySymbol;
            else
                return "%";
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return 0;
            if (Define.CONFIGURATION.CurrencySymbol.Equals(value.ToString()))
                return 1;
            else
                return 0;
        }
    }
}
