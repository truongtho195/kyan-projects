using System;
using System.Linq;
using System.Windows.Data;
using CPC.Helper;

namespace CPC.Converter
{
    class ComboxItemToTextConverter : IValueConverter
    {
        /// <summary>
        /// Get Text value from xml Common
        /// <para>Support</para>
        /// </summary>
        /// <param name="value">Id of item in collection common xml</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">which collection want to convert</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = string.Empty;
            if (parameter != null && value != null)
            {
                switch (parameter.ToString())
                {
                    case "CustomerType":
                        result = Common.CustomerTypes.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "JobTitle":
                        var itemJobTitle = Common.JobTitles.SingleOrDefault(x => x.Value == short.Parse(value.ToString()));
                        if (itemJobTitle != null)
                            result = itemJobTitle.Text;
                        break;
                    case "Country":
                        result = Common.Countries.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "States":
                        result = Common.States.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "Status":
                        int valueCompare = bool.Parse(value.ToString()) ? 1 : 2;
                        result = Common.StatusBasic.Single(x => x.Value == valueCompare).Text;
                        break;
                    case "PaymentCardType":
                        result = Common.PaymentCardTypes.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "Gender":
                        result = Common.Gender.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "PromotionTypes":
                        result = Common.PromotionTypeAll.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "StatusBasic":
                        result = Common.StatusBasicAll.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "ItemTypes":
                        result = Common.ItemTypes.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "StatusSO":
                        result = Common.StatusSalesOrders.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "PriceSchema":
                        result = Common.PriceSchemas.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "TransferStockStatus":
                        result = Common.TransferStockStatus.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "PurchaseStatus":
                        result = Common.PurchaseStatus.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "CountStockStatus":
                        result = Common.CountStockStatus.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "GuestRewardStatus":
                        result = Common.GuestRewardStatus.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "AdjustmentReason":
                        result = Common.AdjustmentReason.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    case "AdjustmentStatus":
                        result = Common.AdjustmentStatus.Single(x => x.Value == short.Parse(value.ToString())).Text;
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
