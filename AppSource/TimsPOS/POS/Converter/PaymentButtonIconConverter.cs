using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using CPC.POS.Model;
using CPC.Helper;

namespace CPC.Converter
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class PaymentButtonIconConverter : IValueConverter
    {
        public FrameworkElement FrameElem = new FrameworkElement();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush br = new SolidColorBrush(Colors.Transparent);
            base_ResourcePaymentDetailModel paymentDetailModel = value as base_ResourcePaymentDetailModel;

            try
            {
                if ("PaymentMethod".Equals(parameter.ToString()))
                {
                    br = PaymentMethods(br, paymentDetailModel.PaymentMethodId);
                }
                else
                {
                    if (paymentDetailModel.PaymentMethodId == 4)
                    {
                        br = PaymentCreditCard(br, paymentDetailModel.CardType);
                    }
                    else
                    {
                        br = PaymentGiftCard(br, paymentDetailModel.CardType);
                    }
                }
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                return null;
            }
            return br;
        }

       

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #region Methods
        private Brush PaymentMethods(Brush br, short id)
        {
            ComboItem item = Common.PaymentMethods.Single(x => System.Convert.ToInt16(x.ObjValue).Equals(id));
            br = (FrameElem.TryFindResource(item.Symbol) as Brush);
            return br;
        }

        private Brush PaymentCreditCard(Brush br, short id)
        {
            ComboItem item = Common.PaymentCardTypes.Single(x => System.Convert.ToInt16(x.ObjValue).Equals(id));
            br = (FrameElem.TryFindResource(item.Symbol) as Brush);
            return br;
        }

        private Brush PaymentGiftCard(Brush br, short id)
        {
            ComboItem item = Common.GiftCardTypes.Single(x => System.Convert.ToInt16(x.ObjValue).Equals(id));
            br = (FrameElem.TryFindResource(item.Symbol) as Brush);
            return br;
        }
        #endregion
    }
}

