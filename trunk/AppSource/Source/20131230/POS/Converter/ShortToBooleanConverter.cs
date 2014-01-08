using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CPC.POS;

namespace CPC.Converter
{
    public class ShortToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            if (int.Parse(value.ToString()) == ((int)CutOffScheduleType.Weekly) && parameter.Equals(CutOffScheduleType.Weekly.ToString()))
                return true;
            else if (int.Parse(value.ToString()) == ((int)CutOffScheduleType.Monthly) && parameter.Equals(CutOffScheduleType.Monthly.ToString()))
                return true;
            else if ((int.Parse(value.ToString()) == ((int)CutOffScheduleType.Yearly) && parameter.Equals(CutOffScheduleType.Yearly.ToString())))
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0;
            if (bool.Parse(value.ToString()))
            {
                if (parameter.Equals(CutOffScheduleType.Weekly.ToString()))
                    return (int)CutOffScheduleType.Weekly;
                else if (parameter.Equals(CutOffScheduleType.Monthly.ToString()))
                    return (int)CutOffScheduleType.Monthly;
                else if (parameter.Equals(CutOffScheduleType.Yearly.ToString()))
                    return (int)CutOffScheduleType.Yearly;
            }
            return 0;
        }
    }
}
