using System;
using System.Linq;
using System.Windows.Data;
using CPC.POS;

namespace CPC.Converter
{
    public class PermissionConverter : IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Define.USER_AUTHORIZATION == null || Define.USER_AUTHORIZATION.Count == 0 || Define.ADMIN_ACCOUNT.Equals(Define.USER.LoginName))
                return true;
            string code = parameter.ToString();
            return (parameter != null && Define.USER_AUTHORIZATION.Select(x => x.Code).Contains(code));
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}