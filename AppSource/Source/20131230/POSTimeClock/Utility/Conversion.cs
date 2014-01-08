using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CPC.Utility
{
    public class Conversion
    {
        /// <summary>
        /// Convert IEnumerable to DataTable 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static DataTable EnumerableToTable<T>(IEnumerable<T> collection) where T : class
        {
            DataTable table = new DataTable();
            foreach (T obj in collection)
            {
                PropertyInfo[] propertyInfos = typeof(T).GetProperties();
                if (table.Columns.Count == 0)
                {
                    // This is the first row. create the columns for table (if these are unavailable).
                    foreach (PropertyInfo pi in propertyInfos)
                    {
                        Type pt = pi.PropertyType;

                        // Skip the nullable type
                        if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                            pt = Nullable.GetUnderlyingType(pt);
                        table.Columns.Add(pi.Name, pt);
                    }
                }

                // create data row
                DataRow row = table.NewRow();
                foreach (PropertyInfo pi in propertyInfos)
                {
                    object value = pi.GetValue(obj, null);
                    row[pi.Name] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }
            return table;
        }

        public static BitmapSource CreateBitmapSourceFromBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            if (Application.Current.Dispatcher == null)
                return null; // Is it possible?

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream. 
                    // I'm assuming it is PNG
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread
                    if (InvokeRequired)
                        return (BitmapSource)Application.Current.Dispatcher.Invoke(
                            new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                            DispatcherPriority.Normal,
                            memoryStream);

                    return CreateBitmapSourceFromBitmap(memoryStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool InvokeRequired
        {
            get { return Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }
    }
}