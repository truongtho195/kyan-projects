using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace CPC.Toolkit.Behavior
{
    /// <summary>
    /// Behavior find row is can focusable & focus to control in cell Index
    /// <remarks>
    /// Using condidional to set focused (IsFocused) for control, set UseCondition = true
    /// </remarks>
    /// </summary>
    public class AutoFocusDatagridCellBehavior : Behavior<DataGrid>
    {
        public string ControlName { get; set; }
        public int CellIndex { get; set; }
        public bool UseCondition { get; set; }

        private static string _controlName;
        private static int _cellIndex;
        #region Initial
        protected override void OnAttached()
        {
            ////Set Default Value
            //SetIsFocused(AssociatedObject, true);
            base.OnAttached();
            AssociatedObject.Loaded += new System.Windows.RoutedEventHandler(AssociatedObject_Loaded);

            if (string.IsNullOrWhiteSpace(ControlName))
                throw new ArgumentException("Control Name is required!");
            _controlName = ControlName;
            _cellIndex = CellIndex;

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= new System.Windows.RoutedEventHandler(AssociatedObject_Loaded);
        }
        #endregion

        #region PropertyDepedency
        #region IsFocused

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
            "IsFocused", typeof(bool), typeof(AutoFocusDatagridCellBehavior), new FrameworkPropertyMetadata());

        public static bool GetIsFocused(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(IsFocusedProperty, value);
        }

        #endregion
        #endregion

        #region Event
        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null)
                return;

            bool focusable = UseCondition ? GetIsFocused(fe) : true;

            if (AssociatedObject.Items.Count > 0 && focusable)
            {
                for (int i = 0; i < AssociatedObject.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        if (!row.IsSelected)
                            row.IsSelected = true;

                        DataGridCell cell = GetCell(AssociatedObject, row, CellIndex);

                        if (EnterEdit(cell, AssociatedObject))//Focus success
                        {
                            AssociatedObject.ScrollIntoView(row, AssociatedObject.Columns[CellIndex]);
                            return;
                        }
                    }
                }
            }
        }


        #endregion

        #region Methods
        private static bool EnterEdit(DataGridCell gridCell, DataGrid grid, bool bFocus = true)
        {
            if (gridCell != null && !gridCell.IsEditing)
            {
                // enables editing on single click  
                if (!gridCell.IsFocused)
                    gridCell.Focus();
                if (!gridCell.IsSelected && grid.SelectionUnit == DataGridSelectionUnit.Cell)
                    gridCell.IsSelected = true;
                grid.BeginEdit();
                if (bFocus)
                {
                    DataTemplate editingTemplate = (gridCell.Content as ContentPresenter).ContentTemplate;
                    if (editingTemplate != null)
                    {
                        var control = editingTemplate.FindName(_controlName, (gridCell.Content as ContentPresenter)) as UIElement;
                        if (control != null && control.Focusable)
                        {
                            control.Focus();
                            return true;
                        }
                    }

                    //var control = FindChildElement<UIElement>(gridCell, _controlName);
                    //if (control != null && control.Focusable)
                    //{
                    //    control.Focus();
                    //    return true;
                    //}
                }
            }
            return false;
        }

        #endregion

        #region Util

        /// <summary>
        /// Gets the specified cell of the DataGrid
        /// </summary>
        /// <param name="grid">The DataGrid instance</param>
        /// <param name="row">The row of the cell</param>
        /// <param name="column">The column index of the cell</param>
        /// <returns>A cell of the DataGrid</returns>
        private static DataGridCell GetCell(DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);

                return cell;
            }
            return null;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        private static T FindChildElement<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChildElement<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Gets the visual child of an element
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="parent">The parent of the expected element</param>
        /// <returns>A visual child</returns>
        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        #endregion
    }
}
