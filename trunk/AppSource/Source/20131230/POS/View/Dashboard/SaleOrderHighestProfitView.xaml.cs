using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.ViewModel;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for SaleOrderHighestProfitView.xaml
    /// </summary>
    public partial class SaleOrderHighestProfitView : UserControl
    {
        #region Contructors

        public SaleOrderHighestProfitView()
        {
            InitializeComponent();
            this.DataContext = new SaleOrderHighestProfitViewModel(gridView, null);
        }

        public SaleOrderHighestProfitView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new SaleOrderHighestProfitViewModel(gridView, configuration);
        }

        #endregion
    }
}
