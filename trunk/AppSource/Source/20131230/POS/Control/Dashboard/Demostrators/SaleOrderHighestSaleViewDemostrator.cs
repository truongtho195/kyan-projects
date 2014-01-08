using CPC.POS.Interfaces;
using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.View;

namespace CPC.Control
{
    class SaleOrderHighestSaleViewDemostrator : IDemonstrator
    {
        #region IDemonstrator Members

        public string Name
        {
            get
            {
                return "SaleOrderHighestSaleView";
            }
        }

        public string Title
        {
            get
            {
                return "Sale Order Graph";
            }
        }

        public string Description
        {
            get
            {
                return "Sale Order Highest Sale";
            }
        }

        public UserControl Create(XElement configuration = null)
        {
            return new SaleOrderHighestSaleView(configuration);
        }

        #endregion
    }
}
