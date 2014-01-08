using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;
using CPC.POS.Database;
namespace CPC.Converter
{
    public class IntToProductStoreConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string content = string.Empty;
            if (values != null && values.Count() > 0)
            {
                if (parameter != null && parameter.Equals("CountStock"))
                    content = (values[1] as ObservableCollection<base_Store>).ElementAt(int.Parse(values[0].ToString())).Name;
                else
                    content = String.Format("{0} :", (values[1] as ObservableCollection<base_Store>).ElementAt(int.Parse(values[0].ToString())).Name);
            }
            return content;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
