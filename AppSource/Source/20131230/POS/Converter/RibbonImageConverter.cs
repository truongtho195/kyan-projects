using System;
using System.Windows.Data;

namespace CPC.Converter
{
    /// <summary>
    /// Change icon on ribbon when style changed
    /// </summary>
    class RibbonImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I added this because I kept getting DependecyProperty.UnsetValue 
            // Passed in as the program initializes
            if (value as string != null)
            {
                string ribbonImageFolder = value as string;
                string ribbonImageName = parameter as string;

                return string.Format("{0}{1}", ribbonImageFolder, ribbonImageName);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}