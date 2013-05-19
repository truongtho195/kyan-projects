using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using log4net.Util;
using System.Collections.Generic;

namespace CPC.Converter
{
    public class UniversalValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null;

            // obtain the conveter for the target type
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            try
            {
                // determine if the supplied value is of a suitable type
                if (converter.CanConvertFrom(value.GetType()))
                {
                    // return the converted value
                    return converter.ConvertFrom(value);
                }
                else
                {
                    // try to convert from the string representation
                    return converter.ConvertFrom(value.ToString());
                }
            }
            catch (Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Enables specifying a Windows Special Folder in the Log4Net section of App.config.
    /// </summary>
    /// <remarks>
    /// Source: http://logging.apache.org/log4net/release/release-notes.html; see section 
    /// titled "PatternString for pattern based configuration". Note that the converter is 
    /// expected to be deprecated in Log4Net 1.2.11, which is supposed to contain a new macro, 
    /// %envFolderPath{}, to perform this task. 
    /// </remarks>
    public class SpecialFolderPatternConverter : PatternConverter
    {
        /* Source: http://logging.apache.org/log4net/release/release-notes.html */

        /* See 'Remarks' in XML comments regarding deprecation of this converter. */

        protected override void Convert(TextWriter writer, object state)
        {
            var specialFolder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), base.Option, true);

            writer.Write(Environment.GetFolderPath(specialFolder));
        }
    }

    /// <summary>
    /// This converter is used to show DateTime in short date format
    /// </summary>
    public class DateFormattingConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return ((DateTime)value).ToShortDateString();

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Ignore empty strings. this will cause the binding to bypass validation.
            if (string.IsNullOrEmpty((string)value))
                return Binding.DoNothing;

            string dateString = (string)value;

            // Append first month and day if just the year was entered
            if (dateString.Length == 4)
                dateString = "1/1/" + dateString;

            DateTime date;
            DateTime.TryParse(dateString, out date);
            return date;
        }

        #endregion
    }

    /// <summary>
    /// This converter is used to change the bool to the visibility.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /* Source: https://skydrive.live.com/?cid=0c0b4f9d80b744cd&id=C0B4F9D80B744CD%21793 */

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

        /// <summary>
        /// convert value from (Visibility) to bool
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter != null)
                {
                    var DefaultValue = bool.Parse(parameter.ToString());
                    FalseToVisibility = !DefaultValue ? Visibility.Visible : Visibility.Collapsed;
                }
                bool boolean = default(bool);

                if ((Visibility)value == FalseToVisibility)
                {
                    boolean = false;
                }
                else
                {
                    boolean = true;
                }

                return boolean;
            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// convert value from bool to (Visibility) 
        /// </summary>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter != null)
                {
                    var DefaultValue = bool.Parse(parameter.ToString());
                    FalseToVisibility = !DefaultValue ? Visibility.Visible : Visibility.Collapsed;
                }

                Visibility visibility = Visibility.Visible;
                if ((bool)value)
                {
                    visibility = ReverseVisibility(FalseToVisibility);
                }
                else
                {
                    visibility = FalseToVisibility;
                }

                return visibility;
            }
            catch
            {
                throw;
            }

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

    /// <summary>
    /// This converter is used to change the visibility to the bool.
    /// </summary>
    public class VisibilityToBoolConverter : IValueConverter
    {
        /* Source: https://skydrive.live.com/?cid=0c0b4f9d80b744cd&id=C0B4F9D80B744CD%21793 */

        private Visibility _falseToVisibility = Visibility.Collapsed;
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

        /// <summary>
        /// convert value from (Visibility) to bool
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool boolean = default(bool);
                if ((Visibility)value == FalseToVisibility)
                {
                    boolean = false;
                }
                else
                {
                    boolean = true;
                }

                return boolean;
            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// convert value from bool to (Visibility) 
        /// </summary>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility visibility = Visibility.Visible;
                if ((bool)value)
                {
                    visibility = ReverseVisibility(FalseToVisibility);
                }
                else
                {
                    visibility = FalseToVisibility;
                }

                return visibility;
            }
            catch
            {
                throw;
            }

        }
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

    /// <summary>
    /// This converter is used to convert enum to visibility.
    /// Return Collapsed when value and parameter are equal.
    /// Return Visible when value and parameter are not equal.
    /// </summary>
    public class EnumToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (string.Compare(value.ToString(), parameter as string, false) == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// This converter is used to convert enum to visibility.
    /// Return Visible when value and parameter are equal.
    /// Return Collapsed when value and parameter are not equal.
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            string comparativeValue = String.Empty;
            if (parameter is string)
            {
                comparativeValue = parameter as string;
            }
            else if (parameter is Enum)
            {
                comparativeValue = parameter.ToString();
            }

            if (string.Compare(value.ToString(), comparativeValue, false) == 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Reserse the boolean
    /// </summary>
    public class ReserseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            if (value is bool)
                result = !((bool)value);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            if (value is bool)
                result = !((bool)value);
            return result;
        }
    }

    public class EnumToBoolConverter : IValueConverter
    {
        private int targetValue;

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
            int mask = (int)parameter;
            this.targetValue = (int)value;
            return IsReversed ? !((mask & this.targetValue) == mask) : ((mask & this.targetValue) == mask);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            this.targetValue ^= (int)parameter;
            if (targetType.BaseType.Equals(typeof(Enum)))
                return Enum.Parse(targetType, this.targetValue.ToString());
            return targetValue;
        }
    }

    /// <summary>
    /// Convert a file path to image source.
    /// </summary>
    public class FileToImageSourceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageSource source = null;
            try
            {
                if (value != null && !string.IsNullOrEmpty(value as string))
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(value.ToString());

                    if (fileInfo.Exists)
                        source = (ImageSource)(new ImageSourceConverter()).ConvertFromString(value.ToString());
                }
            }
            catch
            {
                source = null;
            }
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();

        }

        #endregion
    }

    public class RadioButtonToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object DefaultValue
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                if (targetType == typeof(bool))
                    value = false;
                if (targetType == typeof(int) || targetType == typeof(short))
                    value = 0;
                value = 0;
            }
            return string.Compare(value.ToString(), parameter.ToString(), true) == 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (value.Equals(false))
            //{
            //    if (DefaultValue != null)
            //        return DefaultValue;

            //    if (targetType == typeof(bool))
            //        value = false;
            //    if (targetType == typeof(int) || targetType == typeof(short))
            //        value = 0;

            //    return value;
            //}
            //else
            //{
            if (targetType == typeof(int) || targetType == typeof(short))
                return int.Parse(parameter.ToString());
            if (targetType == typeof(bool))
                return bool.Parse(parameter.ToString());
            return parameter;
            // }
        }
        #endregion
    }

    public class IntegerFormatConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int result;
            if (value == null)
                return null;
            if (value != null && string.IsNullOrWhiteSpace(value.ToString()))
                return null;
            int.TryParse(value.ToString(), out result);
            return result;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int result;
            int.TryParse(value.ToString(), out result);
            if (result == 0) return null;
            return result;
        }
    }

    /// <summary>
    /// Convert bool type to Visibility type.
    /// </summary>
    public class VisibilityRowConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return bool.Parse(value.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Convert Enum type to Boolean type(only using in FringerPrint).
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object DefaultValue
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                if (targetType == typeof(Boolean)) value = false;
                if (targetType == typeof(Int32)) value = 0;
                value = 0;
            }
            return String.Compare(value.ToString(), parameter.ToString(), true) == 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Equals(false))
            {
                if (DefaultValue != null) return DefaultValue;

                if (targetType == typeof(Boolean)) value = false;
                if (targetType == typeof(Int32)) value = 0;

                return value;
            }
            else
            {
                if (targetType == typeof(Int32)) return Int32.Parse(parameter.ToString());
                if (targetType == typeof(Boolean)) return bool.Parse(parameter.ToString());
                return parameter;
            }
        }
        #endregion
    }

    /// <summary>
    /// Convert int  to text.
    /// </summary>
    public class IntToTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            try
            {
                switch (int.Parse(value.ToString()))
                {
                    case 1:
                        return "Review";
                    case 2:
                        return "Promotion";
                    case 3:
                        return "Termination";
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();

        }

        #endregion
    }

    /// <summary>
    /// ReadOnly RowDetailTemplate when Datagrid has error
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public class RowEditingEnableConverter : IMultiValueConverter
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

        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return IsReversed ? true : false;

            System.Windows.Controls.DataGridRow row = value[0] as System.Windows.Controls.DataGridRow;
            var dataGrid = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
            if (dataGrid != null)
            {
                System.Reflection.PropertyInfo inf = dataGrid.GetType().BaseType.GetProperty("HasRowValidationError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (inf != null)
                {
                    var hasError = (bool)inf.GetValue(dataGrid, null);
                    if (hasError && !row.IsEditing)
                        return IsReversed ? false : true;
                }
            }
            return IsReversed ? true : false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


  

    /// <summary>
    /// Convert HeadersVisibility type to Visibility type
    /// </summary>
    public class DataGridVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class RowToIndexConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var dataGridRow = values.LastOrDefault() as DataGridRow;
            int rowNumber = dataGridRow.GetIndex() + 1;
            return rowNumber.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// This converter is used to convert list to visibility.
    /// Return Collapsed when value contains parameter.
    /// Return Visible when value does not contains parameter.
    /// </summary>
    public class ListToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            IList<int> list = value as IList<int>;
            if (list.Contains((int)parameter))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    

    
}
