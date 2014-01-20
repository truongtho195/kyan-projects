using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;
using CPC.POS.Database;
namespace CPC.Converter
{
    public class BoolToStringConverter : IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string content = string.Empty;
            if (value != null)
            {   
                //Reward Tracking is ON
                return bool.Parse(value.ToString()) ? "Reward Tracking is ON" : "Reward Tracking is OFF";
            }
            return content;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
