using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;

namespace FlashCard.Converters
{
    public class DatagridItemToLineNumberConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            DataGridRow item = value as DataGridRow;
            var altIndex = item.GetIndex() ;

            return (altIndex += 1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,CultureInfo culture)
        {
            return null;
        }

    }
}
