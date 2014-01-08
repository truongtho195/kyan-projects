using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace CPC.POSReport.Function
{
    public class RadioButtonConverter : IValueConverter
    {
        //List->Form (radiobutton)
        public object Convert(object value, Type targetType, object paramater, CultureInfo cluture)
        {
            try
            {
                if (!bool.Parse(value.ToString()))
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object paramater, CultureInfo cluture)
        {
            if (bool.Parse(value.ToString()))
                return false;
            return DependencyProperty.UnsetValue;
        }
    }
}
