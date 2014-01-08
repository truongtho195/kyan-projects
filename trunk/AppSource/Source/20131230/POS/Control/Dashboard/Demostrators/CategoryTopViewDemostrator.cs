using CPC.POS.Interfaces;
using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.View;

namespace CPC.Control
{
    class CategoryTopViewDemostrator : IDemonstrator
    {
        #region IDemonstrator Members

        public string Name
        {
            get
            {
                return "CategoryTopView";
            }
        }

        public string Title
        {
            get
            {
                return "Categories Sales Graph";
            }
        }

        public string Description
        {
            get
            {
                return "Show Top Categories";
            }
        }

        public UserControl Create(XElement configuration = null)
        {
            return new CategoryTopView(configuration);
        }

        #endregion
    }
}
