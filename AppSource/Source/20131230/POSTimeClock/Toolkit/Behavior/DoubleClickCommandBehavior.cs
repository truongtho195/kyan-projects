using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Tims.Toolkit.Behavior
{
    /// <summary>
    /// Double click DatagridRow raise commnad
    /// </summary>
    public static class DoubleClickCommandBehavior
    {
        #region Double Click Command Property

        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommand",
                                                typeof(ICommand), typeof(DoubleClickCommandBehavior),
                                              new PropertyMetadata(null, DoubleClickCommandChanged));

        public static ICommand GetDoubleClickCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DoubleClickCommandProperty);
        }

        public static void SetDoubleClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DoubleClickCommandProperty, value);
        }

        #endregion

        #region Double Click Command Parameter Property

        public static readonly DependencyProperty DoubleClickCommandParameterProperty =
           DependencyProperty.RegisterAttached("DoubleClickCommandParameter",
                                              typeof(object), typeof(DoubleClickCommandBehavior),
                                               new PropertyMetadata(null));

        public static object GetDoubleClickCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(DoubleClickCommandParameterProperty);
        }

        public static void SetDoubleClickCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(DoubleClickCommandParameterProperty, value);
        }

        #endregion

        #region Double Click Command Changed

        private static void DoubleClickCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = obj as Selector;

            if (selector != null)
                selector.PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
        }

        private static void HandlePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseEventArgs)
        {
            try
            {
                if (mouseEventArgs.ClickCount == 2)
                {
                    DependencyObject depObj = sender as DependencyObject;
                    DependencyObject mouseClickObj = mouseEventArgs.OriginalSource as DependencyObject;
                    DataGridRow dataGridRow = ItemsControl.ContainerFromElement(depObj as ItemsControl, mouseClickObj) as DataGridRow;
                    var parentOfMouseClickObj = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(mouseClickObj));
                    if (!(parentOfMouseClickObj is ToggleButton))
                        if (dataGridRow != null && dataGridRow.IsSelected)
                        {
                            Selector selector = depObj as Selector;
                            if (selector != null)
                            {
                                if (selector.SelectedItem != null && Keyboard.Modifiers != ModifierKeys.Control)
                                {
                                    ICommand command = GetDoubleClickCommand(depObj);
                                    object commandParameter = GetDoubleClickCommandParameter(depObj);
                                    command.Execute(commandParameter);
                                }
                            }
                        }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                throw;
            }
        }

        #endregion
    }
}
