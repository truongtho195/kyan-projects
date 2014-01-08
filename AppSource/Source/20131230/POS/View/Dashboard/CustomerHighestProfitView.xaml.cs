using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.ViewModel;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for CustomerHighestProfitView.xaml
    /// </summary>
    public partial class CustomerHighestProfitView : UserControl
    {
        #region Contructors

        public CustomerHighestProfitView()
        {
            InitializeComponent();
            this.DataContext = new CustomerHighestProfitViewModel(gridView, null);
        }

        public CustomerHighestProfitView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new CustomerHighestProfitViewModel(gridView, configuration);
        }

        #endregion
    }
}
