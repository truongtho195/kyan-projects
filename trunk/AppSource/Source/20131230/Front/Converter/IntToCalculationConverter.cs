using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using CPC.POS.Model;
using System.Collections.Generic;
using CPC.POS;
namespace CPC.Converter
{
    public class IntToCalculationConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string content = string.Empty;
            try
            {
                if (value != null)
                {
                    int id = Int16.Parse(value.ToString());
                    if (parameter.Equals("0"))
                        content = CPC.Helper.Common.AdjustmentTypes.SingleOrDefault(x => x.Value == id).Text;
                    else if (parameter.Equals("1"))
                        content = CPC.Helper.Common.BasePriceTypes.SingleOrDefault(x => x.Value == id).Text;
                    else
                    {
                        if (id == 0)
                            content = "%";
                        else
                            content = Define.CurrencySymbol;
                    }
                }
            }
            catch
            {
                content = string.Empty;
            }
            return content;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();

        }

        #endregion
    }
}
