using System.Windows.Controls;
using CPC.POS.ViewModel;
using System.Xml.Linq;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for ProductTopView.xaml
    /// </summary>
    public partial class ProductTopView : UserControl
    {
        #region Contructors

        public ProductTopView()
        {
            InitializeComponent();
            this.DataContext = new ProductTopViewModel(gridView, null);
        }

        public ProductTopView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new ProductTopViewModel(gridView, configuration);
        }

        #endregion
    }
}
