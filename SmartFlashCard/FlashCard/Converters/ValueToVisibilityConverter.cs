using System;
using System.Windows.Data;
using System.Windows;

namespace FlashCard.Converters
{
    public class ValueToVisibilityConverter:IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                value = 0;
            }
            return (String.Compare(value.ToString(), parameter.ToString(), true) == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
