using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CPC.Converter
{
    public class IntegerToBooleanConverter : IValueConverter
    {
        private bool _isReversed = false;
        /// <summary>
        /// Gets or sets the IsReversed.
        /// <para>Reverse return value if set true
        /// Default false</para>
        /// </summary>
        public bool IsReversed
        {
            get { return _isReversed; }
            set
            {
                if (_isReversed != value)
                {
                    _isReversed = value;
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return IsReversed ? !(value.ToString().Equals(parameter.ToString())) : (value.ToString().Equals(parameter.ToString()));

            //if (value.ToString().Equals(parameter.ToString()))
            //    return true;
            //else
            //    return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (IsReversed)
                return (bool)value ? null : parameter;
            else
                return (bool)value ? parameter : null;

            //if ((bool)value)
            //    return parameter;
            //else
            //    return null;
        }
    }
}
