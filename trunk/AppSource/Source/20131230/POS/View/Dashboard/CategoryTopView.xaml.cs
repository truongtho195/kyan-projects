using System.Windows.Controls;
using System.Xml.Linq;
using CPC.POS.ViewModel;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for CategoryTopView.xaml
    /// </summary>
    public partial class CategoryTopView : UserControl
    {
        #region Contructors

        public CategoryTopView()
        {
            InitializeComponent();
            this.DataContext = new CategoryTopViewModel(gridView, null);
        }

        public CategoryTopView(XElement configuration)
        {
            InitializeComponent();
            this.DataContext = new CategoryTopViewModel(gridView, configuration);
        }

        #endregion
    }
}
