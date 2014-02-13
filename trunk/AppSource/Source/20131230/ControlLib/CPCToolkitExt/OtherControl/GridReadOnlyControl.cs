using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System;

namespace CPCToolkitExt
{
    public class GridReadOnlyControl : Grid
    {
        #region Constructors
        public GridReadOnlyControl()
        {
        }
        #endregion

        #region Events

        private void GridReadOnlyControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Escape)
            {
                if (this.IsReadOnly.HasValue
                    && this.IsReadOnly.Value)
                    e.Handled = true;
            }
        }

        private void GridReadOnlyControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.IsReadOnly.HasValue)
                e.Handled = !this.IsReadOnly.Value;
        }
        #endregion

        #region DependencyProperties

        #region BackgroundWhenReadOnly
        public Brush BackgroundWhenReadOnly
        {
            get { return (Brush)GetValue(BackgroundWhenReadOnlyProperty); }
            set { SetValue(BackgroundWhenReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundWhenReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundWhenReadOnlyProperty =
            DependencyProperty.Register("BackgroundWhenReadOnly", typeof(Brush), typeof(GridReadOnlyControl), new UIPropertyMetadata(Brushes.Transparent));
        
        #endregion

        #region OpacityWhenReadOnly
        public double OpacityWhenReadOnly
        {
            get { return (double)GetValue(OpacityWhenReadOnlyProperty); }
            set { SetValue(OpacityWhenReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpacityWhenReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityWhenReadOnlyProperty =
            DependencyProperty.Register("OpacityWhenReadOnly", typeof(double), typeof(GridReadOnlyControl), new UIPropertyMetadata(1.0));
        
        #endregion
     
        #region IsReadOnly
        //
        // Summary:
        //     Gets or sets a value that indicates whether the text editing control is read-only
        //     to a user interacting with the control.
        //
        // Returns:
        //     true if the contents of the text editing control are read-only to a user;
        //     otherwise, the contents of the text editing control can be modified by the
        //     user. The default value is false.
        public bool? IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool?), typeof(GridReadOnlyControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsReadOnly)));

        protected static void ChangeIsReadOnly(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as GridReadOnlyControl).ChangeStyle(bool.Parse(e.NewValue.ToString()));
        }
        #endregion

        #endregion

        #region Methods
        protected void ChangeStyle(bool value)
        {
            if (!this.IsLoaded) return;
            this.Dispatcher.BeginInvoke(
                                     DispatcherPriority.Input,
                                     (ThreadStart)delegate
                                        {
                                            try
                                            {
                                                FrameworkElement query = this.Children.Cast<FrameworkElement>().SingleOrDefault(x => x.Name == "RCT_FOCUSGRIDCONTROL");
                                                //Remove Rectange
                                                if (query != null)
                                                    this.Children.Remove(query);
                                                if (!value)
                                                {
                                                    this.RecurseTree(this, false);
                                                    Rectangle rectangle = new Rectangle();
                                                    rectangle.Name = "RCT_FOCUSGRIDCONTROL";
                                                    rectangle.Opacity = this.OpacityWhenReadOnly;
                                                    rectangle.Fill = this.BackgroundWhenReadOnly;
                                                    rectangle.Focusable = false;
                                                    if (this.ColumnDefinitions != null && this.ColumnDefinitions.Count > 0)
                                                        Grid.SetColumnSpan(rectangle, this.ColumnDefinitions.Count());
                                                    if (this.RowDefinitions != null && this.RowDefinitions.Count > 0)
                                                        Grid.SetRowSpan(rectangle, this.RowDefinitions.Count());
                                                    //Add Rectange
                                                    this.Children.Add(rectangle);
                                                }
                                                else
                                                {
                                                    this.RecurseTree(this, true);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.Write("<<<<<<<<<<<<<<<<<<ChangeStyle of GridControl>>>>>>>>>>>>>>>>>>" + ex.ToString());
                                            }
                                        });
        }

        private void RecurseTree(FrameworkElement control, bool isReadOnly)
        {
            try
            {
                foreach (var childrenControl in LogicalTreeHelper.GetChildren(control))
                {
                    var frameworkElement = childrenControl as FrameworkElement;
                    if (frameworkElement == null) continue;
                    if (frameworkElement is GroupBox
                        || frameworkElement is DockPanel
                        || frameworkElement is Grid
                        || frameworkElement is StackPanel
                        || frameworkElement is TabControl)
                    {
                        RecurseTree(frameworkElement, isReadOnly);
                    }
                    else if (frameworkElement is DataGrid)
                    {
                        (frameworkElement as DataGrid).IsReadOnly = isReadOnly;
                        (frameworkElement as DataGrid).Focusable = false;
                        if (isReadOnly)
                            (frameworkElement as DataGrid).PreviewKeyDown -= new KeyEventHandler(GridReadOnlyControl_PreviewKeyDown);
                        else
                            (frameworkElement as DataGrid).PreviewKeyDown += new KeyEventHandler(GridReadOnlyControl_PreviewKeyDown);
                        (frameworkElement as DataGrid).SelectedIndex = -1;
                        foreach (var item in (frameworkElement as DataGrid).ItemsSource)
                        {
                            var row = (frameworkElement as DataGrid).ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                            if (null != row)
                            {
                                row.Focusable = isReadOnly;
                            }
                        }
                    }
                    else
                    {
                        frameworkElement.Focusable = isReadOnly;
                        if (isReadOnly)
                            frameworkElement.PreviewKeyDown -= new KeyEventHandler(GridReadOnlyControl_PreviewKeyDown);
                        else
                            frameworkElement.PreviewKeyDown += new KeyEventHandler(GridReadOnlyControl_PreviewKeyDown);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<RecurseTree of GridControl>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion
    }
}
