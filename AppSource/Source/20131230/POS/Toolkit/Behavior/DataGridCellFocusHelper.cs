using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace CPC.Toolkit.Behavior
{
    public class DataGridCellFocusHelper
    {
        #region CellFocus
        public static DependencyProperty CellFocusProperty =
            DependencyProperty.RegisterAttached("CellFocus",
            typeof(bool),
            typeof(DataGridCellFocusHelper),
            new UIPropertyMetadata(false, OnCellFocusChanged));

        public static bool GetCellFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(CellFocusProperty);
        }

        public static void SetCellFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(CellFocusProperty, value);
        }
        public static void OnCellFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
        #endregion

        #region ItemSource
        public static DependencyProperty ItemSourceProperty =
            DependencyProperty.RegisterAttached("ItemSource",
            typeof(object),
            typeof(DataGridCellFocusHelper),
            new UIPropertyMetadata(null, OnItemSourceChanged));

        public static object GetItemSource(DependencyObject obj)
        {
            return (object)obj.GetValue(ItemSourceProperty);
        }

        public static void SetItemSource(DependencyObject obj, bool value)
        {
            obj.SetValue(ItemSourceProperty, value);
        }
        public static void OnItemSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (sender is DataGridRow)
                {
                    DataGridRow row = sender as DataGridRow;
                    row.PreviewKeyDown += (content, evt) =>
                    {
                        if (!DataGridCellFocusHelper.GetCellFocus(row))
                        {
                            if (evt.Key == Key.F2 && row.Item != null && !row.Item.ToString().Equals("{NewItemPlaceholder}"))
                                evt.Handled = true;
                        }
                    };

                    row.AddHandler(CommandManager.PreviewExecutedEvent,
                      (ExecutedRoutedEventHandler)((rowSender, args) =>
                      {
                          if (!DataGridCellFocusHelper.GetCellFocus(row))
                          {
                              if (args.Command == DataGrid.BeginEditCommand && row.Item != null && !row.Item.ToString().Equals("{NewItemPlaceholder}"))
                                  args.Handled = true;
                          }
                      }));
                }
                if (sender is DataGridCell)
                {
                    DataGridCell cell = sender as DataGridCell;
                    cell.PreviewKeyDown += (content, evt) =>
                    {
                        if (!DataGridCellFocusHelper.GetCellFocus(cell))
                        {
                            if (evt.Key == Key.F2)
                                evt.Handled = true;
                        }
                    };

                    //Cells are readonly.
                    cell.AddHandler(CommandManager.PreviewExecutedEvent,
                     (ExecutedRoutedEventHandler)((rowSender, args) =>
                     {
                         if (!DataGridCellFocusHelper.GetCellFocus(cell))
                         {
                             if (args.Command == DataGrid.BeginEditCommand)
                                 args.Handled = true;
                         }
                     }));
                    IsFocusComboBox = false;
                    //Cells are focused.
                    if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
                        cell.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(cell_PreviewMouseLeftButtonDown);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnItemSourceChanged" + ex.ToString());
            }
        }
        static bool IsFocusComboBox = false;
        static void cell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGridCell cell = sender as DataGridCell;
                if (DataGridCellFocusHelper.GetCellFocus(cell) && !IsFocusComboBox)
                {
                    if (!cell.IsFocused)
                        cell.Focus();
                    DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                    if (dataGrid != null)
                    {
                        if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                        {
                            if (!cell.IsSelected)
                                cell.IsSelected = true;
                        }
                        else
                        {
                            DataGridRow row = FindVisualParent<DataGridRow>(cell);
                            if (row != null && !row.IsSelected)
                            {
                                row.IsSelected = true;
                            }
                        }
                    }
                    //Set selected for ComboBox.
                    cell.Dispatcher.BeginInvoke(
                                DispatcherPriority.Input,
                                (ThreadStart)delegate
                                {
                                    if (Keyboard.FocusedElement is ComboBox)
                                    {
                                        ComboBox cm = Keyboard.FocusedElement as ComboBox;
                                        cm.Focus();
                                        cm.DropDownOpened += delegate
                                        {
                                            IsFocusComboBox = true;
                                            cm.Focus();
                                        };
                                        cm.DropDownClosed += delegate
                                        {
                                            IsFocusComboBox = false;
                                        };
                                    }
                                });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cell_PreviewMouseLeftButtonDown" + ex.ToString());
            }

        }
        #endregion

        #region Focus Command

        #region CommandParameter
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static DependencyProperty CommandParameterProperty =
             DependencyProperty.RegisterAttached("CommandParameter",
             typeof(object),
             typeof(DataGridCellFocusHelper),
             new UIPropertyMetadata(null));

        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }
        #endregion

        #region Command
        //
        // Summary:
        //     Gets or sets the command to invoke when users click to control on DataGridCell. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when users click to control on DataGridCell. The default value is null.
        public static DependencyProperty CommandProperty =
             DependencyProperty.RegisterAttached("Command",
             typeof(ICommand),
             typeof(DataGridCellFocusHelper),
             new UIPropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }
        public static void OnCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement)
                {
                    FrameworkElement control = sender as FrameworkElement;
                    control.Loaded += new RoutedEventHandler(Control_Loaded);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnCommandChanged DataGridCellFocusHelper" + ex.ToString());
            }
        }
        protected static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement control = sender as FrameworkElement;
                //To execute command when it isn't null and it was focused.
                if (control.IsFocused && DataGridCellFocusHelper.GetCommand(control) != null)
                    DataGridCellFocusHelper.GetCommand(control).Execute(DataGridCellFocusHelper.GetCommandParameter(control));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Control_Loaded DataGridCellFocusHelper" + ex.ToString());
            }
        }
        #endregion

        #endregion

        #region FindVisualParent
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        #endregion
    }
}
