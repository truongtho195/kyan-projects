using System.Windows.Controls;
using CPC.POS.ViewModel;
using System.Xml.Linq;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for InventoryStatisticsView.xaml
    /// </summary>
    public partial class InventoryStatisticsView : UserControl
    {
        #region Contructors

        public InventoryStatisticsView()
        {
            InitializeComponent();
            this.DataContext = new InventoryStatisticsViewModel(null);
        }

        public InventoryStatisticsView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new InventoryStatisticsViewModel(configuration);
        }

        #endregion
    }
}
