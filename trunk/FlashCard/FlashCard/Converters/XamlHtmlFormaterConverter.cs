using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
//using HTMLConverter;

namespace FlashCard.Converters
{
    public class XamlHtmlFormaterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && string.IsNullOrWhiteSpace(value.ToString()))
            {
                return HtmlToXamlConverter.ConvertHtmlToXaml(value.ToString(), true);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && string.IsNullOrWhiteSpace(value.ToString()))
            {
                return HtmlFromXamlConverter.ConvertXamlToHtml(value.ToString());
            }
            return null;
        }
    }
}
