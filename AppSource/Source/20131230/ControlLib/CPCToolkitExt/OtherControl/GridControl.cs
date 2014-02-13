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

namespace POS.UserControls
{
    public class GridControl : Grid
    {
        public GridControl()
        {
        }
        public bool? IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool?), typeof(GridControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsReadOnly)));

        protected static void ChangeIsReadOnly(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as GridControl).ChangeStyle(bool.Parse(e.NewValue.ToString()));
        }

        public void ChangeStyle(bool value)
        {
            this.Dispatcher.BeginInvoke(
                                     DispatcherPriority.Input,
                                     (ThreadStart)delegate
                                        {
                                            if (!this.IsLoaded) return;
                                            FrameworkElement query = this.Children.Cast<FrameworkElement>().SingleOrDefault(x => x.Name == "RCT_FOCUS");
                                            if (query != null)
                                            {
                                                this.Children.Remove(query);
                                            }
                                            if (!value)
                                            {
                                                this.RecurseTree(this, false);
                                                Rectangle rectangle = new Rectangle();
                                                rectangle.Name = "RCT_FOCUS";
                                                rectangle.Opacity = .3;
                                                rectangle.Fill = Brushes.BlueViolet;
                                                rectangle.Focusable = false;
                                                if (this.ColumnDefinitions != null && this.ColumnDefinitions.Count > 0)
                                                    Grid.SetColumnSpan(rectangle, this.ColumnDefinitions.Count());
                                                if (this.RowDefinitions != null && this.RowDefinitions.Count > 0)
                                                    Grid.SetRowSpan(rectangle, this.RowDefinitions.Count());
                                                this.Children.Add(rectangle);
                                            }
                                            else
                                            {
                                                this.RecurseTree(this, true);
                                            }
                                        });
        }

        void GridControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Escape)
            {
                if (this.IsReadOnly.HasValue
                    && this.IsReadOnly.Value)
                    e.Handled = true;
            }
        }

        private void RecurseTree(FrameworkElement control, bool isReadOnly)
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
                        (frameworkElement as DataGrid).PreviewKeyDown -= new KeyEventHandler(GridControl_PreviewKeyDown);
                    else
                        (frameworkElement as DataGrid).PreviewKeyDown += new KeyEventHandler(GridControl_PreviewKeyDown);
                    (frameworkElement as DataGrid).SelectedIndex = -1;
                }
                else
                {
                    frameworkElement.Focusable = isReadOnly;
                    if (isReadOnly)
                        frameworkElement.PreviewKeyDown -= new KeyEventHandler(GridControl_PreviewKeyDown);
                    else
                        frameworkElement.PreviewKeyDown += new KeyEventHandler(GridControl_PreviewKeyDown);
                }
            }
        }

        private void GridControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.IsReadOnly.HasValue)
                e.Handled = !this.IsReadOnly.Value;
        }
    }
}
