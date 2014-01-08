using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace CPC.Toolkit.Layout
{
    public static class Screenshot
    {
        /// <summary>
        /// Gets a JPG "screenshot" of the current UIElement
        /// </summary>
        /// <param name="source">UIElement to screenshot</param>
        /// <param name="scale">Scale to render the screenshot</param>
        /// <param name="quality">JPG Quality</param>
        /// <returns>Byte array of JPG data</returns>
        public static byte[] GetJpgImage(FrameworkElement source, double scale, int quality)
        {
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();

            // save current canvas transform
            Transform transform = source.LayoutTransform;

            // get size of control
            System.Windows.Size sizeOfControl = new System.Windows.Size(source.ActualWidth, source.ActualHeight);

            // measure and arrange the control
            source.Measure(sizeOfControl);

            // arrange the surface
            source.Arrange(new Rect(sizeOfControl));

            // craete and render surface and push bitmap to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((Int32)sizeOfControl.Width, (Int32)sizeOfControl.Height, 96d, 96d, PixelFormats.Pbgra32);

            // now render surface to bitmap
            renderBitmap.Render(source);

            // encode png data
            // puch rendered bitmap into it
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            Byte[] _imageArray = null;
            try
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    pngEncoder.Save(outputStream);
                    _imageArray = outputStream.ToArray();
                }

            }
            catch (Exception)
            {
                throw;
            }
            return _imageArray;

        }

        public static BitmapSource MakeGrayscale(System.Drawing.Bitmap original)
        {
            //make an empty bitmap the same size as original
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(original.Width, original.Height);

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    //get the pixel from the original image
                    System.Drawing.Color originalColor = original.GetPixel(i, j);

                    //create the grayscale version of the pixel
                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
                        + (originalColor.B * .11));

                    //create the color object
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(grayScale, grayScale, grayScale);

                    //set the new image's pixel to the grayscale version
                    newBitmap.SetPixel(i, j, newColor);
                }
            }

            BitmapSource bmpSource = ToBitmapSource(newBitmap);
            newBitmap.Dispose();
            newBitmap = null;

            return bmpSource;
        }

        /// <summary>    
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.   
        /// </summary>     
        /// <remarks>
        /// Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.    
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>   
        private static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;
            var hBitmap = source.GetHbitmap();
            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (System.ComponentModel.Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            } return bitSrc;
        }

        /// <summary> 
        /// FxCop requires all Marshalled functions to be in a class called NativeMethods. 
        /// </summary> 
        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
