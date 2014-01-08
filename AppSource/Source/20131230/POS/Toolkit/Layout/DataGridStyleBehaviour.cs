using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CPC.Toolkit.Layout
{
    public static class DataGridStyleBehaviour
    {
        #region attached property

        public static ControlTemplate GetSelectAllButtonTemplate(DataGrid obj)
        {
            return (ControlTemplate)obj.GetValue(SelectAllButtonTemplateProperty);
        }

        public static void SetSelectAllButtonTemplate(DataGrid obj, ControlTemplate value)
        {
            obj.SetValue(SelectAllButtonTemplateProperty, value);
        }

        public static readonly DependencyProperty SelectAllButtonTemplateProperty =
            DependencyProperty.RegisterAttached("SelectAllButtonTemplate",
            typeof(ControlTemplate), typeof(DataGridStyleBehaviour),
            new UIPropertyMetadata(null, OnSelectAllButtonTemplateChanged));

        #endregion

        #region property behaviour

        // property change event handler for SelectAllButtonTemplate
        private static void OnSelectAllButtonTemplateChanged(
            DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            DataGrid grid = depObj as DataGrid;
            if (grid == null)
                return;

            // handle the grid's Loaded event
            grid.Loaded += new RoutedEventHandler(Grid_Loaded);
        }

        // Handles the DataGrid's Loaded event
        private static void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            DependencyObject dep = grid;

            // Navigate down the visual tree to the button
            while (!(dep is Button))
            {
                if (VisualTreeHelper.GetChildrenCount(dep) > 0)
                {
                    dep = VisualTreeHelper.GetChild(dep, 0);
                }
                else
                {
                    break;
                }
            }
            Button button = dep as Button;

            // apply our new template
            ControlTemplate template = GetSelectAllButtonTemplate(grid);
            if (button != null)
            {
                button.Template = template;
            }
        }

        #endregion
    }
}
