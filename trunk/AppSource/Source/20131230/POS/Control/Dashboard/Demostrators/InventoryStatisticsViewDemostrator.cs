using CPC.POS.Interfaces;
using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.View;

namespace CPC.Control
{
    class InventoryStatisticsViewDemostrator : IDemonstrator
    {
        #region IDemonstrator Members

        public string Name
        {
            get
            {
                return "InventoryStatisticsView";
            }
        }

        public string Title
        {
            get
            {
                return "Inventory Graph";
            }
        }

        public string Description
        {
            get
            {
                return "Inventory Statistics";
            }
        }

        public UserControl Create(XElement configuration = null)
        {
            return new InventoryStatisticsView(configuration);
        }

        #endregion
    }
}
