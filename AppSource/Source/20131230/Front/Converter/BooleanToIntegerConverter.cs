using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CPC.POS;

namespace CPC.Converter
{
    class BooleanToIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return (int)StatusBasic.Deactive;//False
            return (bool)value ? (int)StatusBasic.Active : (int)StatusBasic.Deactive;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return int.Parse(value.ToString()).Equals((int)StatusBasic.Active);
        }
    }
}
