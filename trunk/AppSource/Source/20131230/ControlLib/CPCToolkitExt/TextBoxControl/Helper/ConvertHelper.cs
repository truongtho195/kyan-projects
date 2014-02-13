using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;

namespace CPCToolkitExt.TextBoxControl.Helper
{
   public class ConvertHelper
    {
       public static System.Windows.Media.ImageBrush CreateBrushFromBitmap(System.Drawing.Bitmap bitmap)
       {
           BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
               bitmap.GetHbitmap(),
               IntPtr.Zero,
               Int32Rect.Empty,
               BitmapSizeOptions.FromEmptyOptions());

           return new System.Windows.Media.ImageBrush(bitmapSource);
       }
    }
}
