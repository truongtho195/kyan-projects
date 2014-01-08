using System;
using System.Windows.Data;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace CPC.Converter
{
    class ByteArrayToBitmapImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null || targetType != typeof(ImageSource))
                {
                    return null;
                }

                byte[] bytes = value as byte[];
                if (bytes != null && bytes.Length > 0)
                {
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        return bitmapImage;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || targetType != typeof(byte[]))
            {
                return null;
            }

            BitmapImage bitmapImage = value as BitmapImage;
            if (bitmapImage != null)
            {
                return File.ReadAllBytes(bitmapImage.UriSource.LocalPath);
            }

            return null;
        }

        #endregion
    }
}
