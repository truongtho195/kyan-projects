using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;

namespace FlashCard.Converters
{
    public class ListBoxItemToLineNumberConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            ListBoxItem item = value as ListBoxItem;

            ListBox view = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;

            int index = view.ItemContainerGenerator.IndexFromContainer(item) + 1;
            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,CultureInfo culture)
        {
            return null;
        }

    }
}
