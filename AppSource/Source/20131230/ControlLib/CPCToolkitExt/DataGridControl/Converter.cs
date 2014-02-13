using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
namespace CPCToolkitExt.DataGridControl
{
    /// <summary>
    ///     Converts DataGridHeadersVisibility to Visibility based on the given parameter.
    /// </summary> 
    public class DataGridHeaderVisibilityToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Convert DataGridHeadersVisibility to Visibility
        /// </summary>
        /// <param name="value">DataGridHeadersVisibility</param>
        /// <param name="targetType">Visibility</param>
        /// <param name="parameter">DataGridHeadersVisibility that represents the minimum DataGridHeadersVisibility that is needed for a Visibility of Visible</param>
        /// <param name="culture">null</param>
        /// <returns>Visible or Collapsed based on the value & converter mode</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var visible = false;

                if (value is DataGridHeadersVisibility && parameter is DataGridHeadersVisibility)
                {
                    var valueAsDataGridHeadersVisibility = (DataGridHeadersVisibility)value;
                    var parameterAsDataGridHeadersVisibility = (DataGridHeadersVisibility)parameter;

                    switch (valueAsDataGridHeadersVisibility)
                    {
                        case DataGridHeadersVisibility.All:
                            visible = true;
                            break;
                        case DataGridHeadersVisibility.Column:
                            visible = parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.Column ||
                                        parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.None;
                            break;
                        case DataGridHeadersVisibility.Row:
                            visible = parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.Row ||
                                        parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.None;
                            break;
                    }
                }

                if (targetType == typeof(Visibility))
                {
                    return visible ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }

        }

        /// <summary>
        ///     Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ValidationToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class SelectedItemConverter : IMultiValueConverter
    {

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0].Equals(values[1]))
                return true;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class SelectedWithNewItemConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DataGridControl)
            {
                DataGridControl dataGrid = values[0] as DataGridControl;
                if (dataGrid.Items != null && !dataGrid.Items.Cast<object>().Last().ToString().Equals("{NewItemPlaceholder}"))
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NewItemConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1].ToString().Equals("{NewItemPlaceholder}"))
                return true;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DictionarytoBoolConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null) return false;
            else if (values[0] is Dictionary<object, object>)
                if ((values[0] as Dictionary<object, object>).Where(x => x.Key == values[1]).Count() > 0)
                    return true;
            return false;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    public class EnumToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is bool && bool.Parse(value.ToString()))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
