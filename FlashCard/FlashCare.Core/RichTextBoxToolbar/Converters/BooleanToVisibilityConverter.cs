using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace RichTextBoxControl.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool DefaultValue = true;
            if (value == null)
                return Visibility.Collapsed;

            if (parameter != null)
                DefaultValue = bool.Parse(parameter.ToString());

            if (bool.Parse(value.ToString()).Equals(DefaultValue))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null) parameter = String.Empty;
            if (!parameter.Equals("false"))
                return value.Equals(Visibility.Visible) ? false : true;
            else
                return value.Equals(Visibility.Collapsed) ? false : true;
        }
    }
}
