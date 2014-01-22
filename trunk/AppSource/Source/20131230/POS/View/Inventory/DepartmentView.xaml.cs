using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace CPC.POS.View
{
	/// <summary>
    /// Interaction logic for DepartmentView.xaml
	/// </summary>
	public partial class DepartmentView
	{
        public DepartmentView()
		{
			this.InitializeComponent();
		}

        private void WatermarkTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtSearch.GetBindingExpression(WatermarkTextBox.TextProperty).UpdateSource();
            }
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }
	}
}