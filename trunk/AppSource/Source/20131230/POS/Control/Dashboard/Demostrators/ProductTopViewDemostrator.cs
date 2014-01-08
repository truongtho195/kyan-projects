using CPC.POS.Interfaces;
using CPC.POS.View;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CPC.Control
{
    class ProductTopViewDemostrator : IDemonstrator
    {
        #region IDemonstrator Members

        public string Name
        {
            get
            {
                return "ProductTopView";
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
                return "Show Top Products";
            }
        }

        public UserControl Create(XElement configuration = null)
        {
            return new ProductTopView(configuration);
        }

        #endregion
    }
}
