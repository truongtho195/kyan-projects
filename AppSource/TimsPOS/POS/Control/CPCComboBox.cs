using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CPC.Control
{
    public class CPCComboBox : ComboBox
    {

        public CPCComboBox()
        {
           
        }
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            var binding = GetBindingExpression(SelectedValueProperty);
            if (binding != null)
            {
                binding.UpdateTarget();
            }
        }
    }
}
