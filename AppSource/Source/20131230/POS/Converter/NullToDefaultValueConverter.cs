using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CPC.Converter
{
    public class NullToDefaultValueConverter : IValueConverter
    {
        public bool RequiredGreaterOrEqualCurrentDate
        {
            get;
            set;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(DateTime))
            {
                if (value == null || (RequiredGreaterOrEqualCurrentDate && System.Convert.ToDateTime(value).Date < DateTime.Now.Date))
                {
                    return DateTime.Now;
                }
            }

            return value;
        }

        #endregion
    }
}
