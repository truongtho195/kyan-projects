using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.ViewModel;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for CustomerHighestSaleView.xaml
    /// </summary>
    public partial class CustomerHighestSaleView : UserControl
    {
        #region Contructors

        public CustomerHighestSaleView()
        {
            InitializeComponent();
            this.DataContext = new CustomerHighestSaleViewModel(gridView, null);
        }

        public CustomerHighestSaleView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new CustomerHighestSaleViewModel(gridView, configuration);
        }

        #endregion
    }
}
