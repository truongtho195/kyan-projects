using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;
using CPC.POS.Database;
using CPC.POS;
namespace CPC.Converter
{
    public class DecimalFormatConverter : IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (Decimal.Parse(value.ToString()) > 0)
                    return String.Format(Define.NumericFormat, Define.CurrencyFormat, Decimal.Parse(value.ToString()));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
