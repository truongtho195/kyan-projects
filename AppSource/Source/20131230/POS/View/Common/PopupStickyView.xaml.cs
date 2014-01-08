
namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for PopupStickyView.xaml
    /// </summary>
    public partial class PopupStickyView
    {
        public PopupStickyView()
        {
            this.InitializeComponent();
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };
        }
    }
}