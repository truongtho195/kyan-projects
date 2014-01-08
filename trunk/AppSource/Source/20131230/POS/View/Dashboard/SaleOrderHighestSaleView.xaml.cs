using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.ViewModel;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for SaleOrderHighestSaleView.xaml
    /// </summary>
    public partial class SaleOrderHighestSaleView : UserControl
    {
        #region Contructors

        public SaleOrderHighestSaleView()
        {
            InitializeComponent();
            this.DataContext = new SaleOrderHighestSaleViewModel(gridView, null);
        }

        public SaleOrderHighestSaleView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new SaleOrderHighestSaleViewModel(gridView, configuration);
        }

        #endregion
    }
}
