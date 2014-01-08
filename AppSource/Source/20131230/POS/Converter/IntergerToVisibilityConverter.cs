using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace CPC.Converter
{
    class IntergerToVisibilityConverter:IValueConverter
    {

        private Visibility _falseToVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility when false.
        /// </summary>
        public Visibility FalseToVisibility
        {
            get
            {
                return _falseToVisibility;
            }
            set
            {
                _falseToVisibility = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                parameter = -1;
            if (value == null)
                return Visibility.Collapsed;

            Visibility visibility = Visibility.Visible;
            if (int.Parse(value.ToString()).Equals(int.Parse(parameter.ToString())))
            {
                visibility = ReverseVisibility(FalseToVisibility);
            }
            else
            {
                visibility = FalseToVisibility;
            }

            return visibility;
            //if ( int.Parse(value.ToString()).Equals(int.Parse(parameter.ToString())))
            //    return Visibility.Visible;
            //return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reverse visibility
        /// </summary>
        /// <param name="visibility"></param>
        /// <returns></returns>
        private Visibility ReverseVisibility(Visibility visibility)
        {
            Visibility revertVisibility = Visibility.Visible;
            switch (visibility)
            {
                case Visibility.Collapsed:
                    revertVisibility = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    revertVisibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
            return revertVisibility;
        }
    }
}
