using System;
using System.Windows.Data;
using System.Windows.Controls;

namespace FlashCard.Converters
{
    public class ListViewLineNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            ListBoxItem item = value as ListBoxItem;

            ListBox view = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;

            int index = view.ItemContainerGenerator.IndexFromContainer(item) + 1;
            return index.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
