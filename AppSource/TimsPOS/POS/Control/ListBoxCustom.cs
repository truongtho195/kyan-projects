using System.Windows.Controls;

namespace CPC.Control
{
    public class ListBoxCustom : ListBox
    {
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (this.SelectedItem == null && e.RemovedItems.Count > 0)
                SelectedItem = e.RemovedItems[0];

            base.OnSelectionChanged(e);
        }
    }
}
