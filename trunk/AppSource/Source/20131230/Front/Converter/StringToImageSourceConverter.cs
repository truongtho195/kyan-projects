using System;
using System.Windows.Data;
using System.Windows.Media;

namespace CPC.Converter
{
    public class StringToImageSourceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageSource source = null;
            try
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
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
