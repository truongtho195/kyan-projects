using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace CPC.POSReport.Function
{
    public class ImageConverter : IValueConverter
    {
        // ByteArray To ImageSource
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] imageByte = value as byte[];
            if (imageByte != null && imageByte.Length > 0)
            {
                MemoryStream memoryStream = new MemoryStream(imageByte);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            else
            {
                return @"/Image/NoImage.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //return null;
        }
    }
}
