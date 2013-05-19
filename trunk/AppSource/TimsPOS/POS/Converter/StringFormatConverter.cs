using System;
using System.Diagnostics;
using System.Windows.Data;

namespace CPC.Converter
{
    public class StringFormatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null) parameter = String.Empty;
            if (value == null) return string.Empty;

            switch (parameter.ToString())
            {
                case "Phone":
                    return this.PhoneStringFormat(value.ToString());
                case "FaxNumber":
                    return this.FaxStringFormat(value.ToString());
                case "Zip":
                    return this.ZipStringFormat(value.ToString());
                case "CardNumber":
                    return CardNumberFormat(value.ToString());
                default:
                    return string.Empty;
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string PhoneStringFormat(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content)) return string.Empty;
                if (content.Length == 10)
                    return string.Format("({0}) {1}-{2}", content.Substring(0, 3), content.Substring(3, 3), content.Substring(6, 4));
                else if (content.Length > 10)
                    return string.Format("({0}) {1}-{2} - {3}", content.Substring(0, 3), content.Substring(3, 3), content.Substring(6, 4), content.Substring(10, content.Length - 10));
            }
            catch (Exception ex)
            {
                Debug.Write("FormatData" + ex.ToString());
            }
            return string.Empty;
        }

        private string FaxStringFormat(string content)
        {
            try
            {
                if (!String.IsNullOrEmpty(content))
                    return content.Substring(0, 3) + "-" + content.Substring(3, 3) + "-" + content.Substring(6, 4);

            }
            catch (Exception ex)
            {
                Debug.Write("FaxStringFormat" + ex.ToString());
            }
            return string.Empty;
        }

        private string ZipStringFormat(string content)
        {
            try
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if (content.Length == 9)
                        return content.Substring(0, 5) + "-" + content.Substring(5, 4);
                    else
                        return content;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("ZipStringFormat" + ex.ToString());
            }
            return string.Empty;
        }

        private string CardNumberFormat(string content)
        {
            bool isSecure = true;
            try
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if (content.Length == 16)
                        return string.Format("{0}-{1}-{2}-{3}", content.Substring(0, 4), isSecure?"****":content.Substring(5, 4),isSecure?"****": content.Substring(9, 4), content.Substring(12, 4));
                    else
                        return content;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("CardNumberFormat" + ex.ToString());
            }
            return string.Empty;
        }
        #endregion
    }
}
