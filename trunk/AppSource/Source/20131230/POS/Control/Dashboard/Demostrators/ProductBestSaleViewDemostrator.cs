using CPC.POS.Interfaces;
using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.View;

namespace CPC.Control
{
    class ProductBestSaleViewDemostrator : IDemonstrator
    {
        #region IDemonstrator Members

        public string Name
        {
            get
            {
                return "ProductBestSaleView";
            }
        }

        public string Title
        {
            get
            {
                return "Products Graph";
            }
        }

        public string Description
        {
            get
            {
                return "Products Best Sale";
            }
        }

        public UserControl Create(XElement configuration = null)
        {
            return new ProductBestSaleView(configuration);
        }

        #endregion
    }
}
