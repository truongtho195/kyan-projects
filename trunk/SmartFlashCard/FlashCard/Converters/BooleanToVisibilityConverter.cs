using System;
using System.Windows.Data;
using System.Windows;

namespace FlashCard.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converter boolean type to Visble or Collapse a object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">
        /// Default is true to visible 
        /// or using user value input
        /// </param>
        /// <param name="culture"></param>
        /// <returns>Visibility</returns>
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
            throw new NotImplementedException();
        }
    }
}
