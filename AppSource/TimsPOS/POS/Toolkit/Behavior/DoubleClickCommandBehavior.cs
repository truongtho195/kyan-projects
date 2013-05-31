using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CPC.Toolkit.Behavior
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



        public static bool GetCanDoubleClickOnChild(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanDoubleClickOnChildProperty);
        }

        public static void SetCanDoubleClickOnChild(DependencyObject obj, bool value)
        {
            obj.SetValue(CanDoubleClickOnChildProperty, value);
        }

        // Using a DependencyProperty as the backing store for Test.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanDoubleClickOnChildProperty =
            DependencyProperty.RegisterAttached("CanDoubleClickOnChild", typeof(bool), typeof(DoubleClickCommandBehavior), new UIPropertyMetadata(null));




        //// Using a DependencyProperty as the backing store for CanDoubleOnChild.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty CanDoubleOnChildProperty =
        //    DependencyProperty.Register("CanDoubleOnChild", typeof(bool), typeof(DoubleClickCommandBehavior), new PropertyMetadata(null));

        //public static bool GetCanDoubleOnChild(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(CanDoubleOnChildProperty);
        //}

        //public static void SetCanDoubleOnChild(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(CanDoubleOnChildProperty, value);
        //}



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
                    var dataGridDetailsPresenter = FindDataGridDetailsPresenter(mouseClickObj);
                    
                    // Block command execute when click plus button or DataGridRowDetail 
                    //&& (dataGridDetailsPresenter == null)
                    bool canDoubleClickOnChild = GetCanDoubleClickOnChild(depObj);
                    if (!canDoubleClickOnChild && dataGridDetailsPresenter != null)
                        return;

                    //if (!(parentOfMouseClickObj is ToggleButton))
                    if (parentOfMouseClickObj is DataGridCellsPanel || parentOfMouseClickObj is Border || parentOfMouseClickObj is TextBlock || parentOfMouseClickObj is ContentPresenter)
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
                //throw;
            }
        }

        /// <summary>
        /// Get ContainerView from children DependencyObject
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        private static DataGridDetailsPresenter FindDataGridDetailsPresenter(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            // Check if this is the end of the tree       
            if (parent == null)
                return null;

            DataGridDetailsPresenter parentWindow = parent as DataGridDetailsPresenter;
            if (parentWindow != null)
                return parentWindow;
            else
                // Use recursion until it reaches a Window           
                return FindDataGridDetailsPresenter(parent);
        }

        #endregion
    }
}
