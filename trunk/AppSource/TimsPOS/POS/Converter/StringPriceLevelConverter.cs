using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
namespace CPC.Converter
{
    public class StringPriceLevelConverter: IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            short priceLevelId = 0;
            try
            {
                if (value != null)
                {
                    string content = value.ToString();
                    priceLevelId = CPC.Helper.Common.PriceSchemas.SingleOrDefault(x=>x.Text.Equals(value)).Value;
                }
            }
            catch
            {
                priceLevelId = 0;
            }
            return priceLevelId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();

        }

        #endregion
    }
}
