using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using log4net.Util;

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
}