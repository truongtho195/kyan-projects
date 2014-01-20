using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CPC.Control.Converters
{
    public class PathToBitmapImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            // Value contains the full path to the image or document. 
            string filePath = value.ToString();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            try
            {
                // Load the image, specify CacheOption so the file is not locked. 
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.UriSource = new Uri(filePath);
                bitmapImage.EndInit();
                // If EndInit succesful then this is image.
                return bitmapImage;
            }
            catch (NotSupportedException)
            {
                // Got a NotSupportedException, this is not image.
                return filePath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
