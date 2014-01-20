using System;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using CPC.POS.Model;
using CPC.POS;

namespace CPC.Converter
{
    public class BinaryTotalToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int binaryTotal = System.Convert.ToInt32(value);
                List<int> numberCheckedList = new List<int>();

                IList<ComboItem> items = parameter as IList<ComboItem>;
                foreach (var item in items)
                {
                    if ((binaryTotal & item.Value) == item.Value)
                    {
                        numberCheckedList.Add(item.Value);
                    }
                }

                return string.Join(",", numberCheckedList);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int binaryTotal = 0;
                string[] numberCheckedList = value.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var numberChecked in numberCheckedList)
                {
                    binaryTotal += System.Convert.ToInt32(numberChecked);
                }

                IList<ComboItem> items = parameter as IList<ComboItem>;
                // Try find always payment method.
                ComboItem alwaysPaymentMethodItem = items.FirstOrDefault(x => x.IntValue == Define.DefaultCashPayment);
                if (alwaysPaymentMethodItem != null)
                {
                    short valueAlwaysPaymentMethod = alwaysPaymentMethodItem.Value;
                    if ((binaryTotal & valueAlwaysPaymentMethod) != valueAlwaysPaymentMethod)
                    {
                        binaryTotal += valueAlwaysPaymentMethod;
                    }
                }

                return binaryTotal;
            }
            catch
            {
                if (targetType.IsValueType)
                {
                    return Activator.CreateInstance(targetType);
                }

                return null;
            }
        }
    }
}
