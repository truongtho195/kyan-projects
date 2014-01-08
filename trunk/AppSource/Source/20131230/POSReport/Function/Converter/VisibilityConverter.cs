using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace CPC.POSReport.Function.Converter
{
    public class VisibilityConverter : IValueConverter
    {
        // ByteArray To ImageSource
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            if (value != null)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
