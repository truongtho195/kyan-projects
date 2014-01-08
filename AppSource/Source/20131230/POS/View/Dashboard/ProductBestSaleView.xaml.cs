using System.Windows.Controls;
using CPC.POS.ViewModel;
using System.Xml.Linq;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for ProductBestSaleView.xaml
    /// </summary>
    public partial class ProductBestSaleView : UserControl
    {
        #region Contructors

        public ProductBestSaleView()
        {
            InitializeComponent();
            this.DataContext = new ProductBestSaleViewModel(gridView, null);
        }

        public ProductBestSaleView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new ProductBestSaleViewModel(gridView, configuration);
        }

        #endregion
    }
}
