using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
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
                    else if(parameter.Equals("1"))
                        content = CPC.Helper.Common.BasePriceTypes.SingleOrDefault(x => x.Value == id).Text;
                    else
                        content = CPC.Helper.Common.ProductCommissionTypes.SingleOrDefault(x => x.Value == id).Text;
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
