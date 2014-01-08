using System;
using System.Collections.Generic;
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

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for ScheduleManagement.xaml
    /// </summary>
    public partial class TemplateCategoryView
    {
        #region Contructors

        public TemplateCategoryView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region TextBoxIsVisibleChanged

        private void TextBoxIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(true))
            {
                TextBox textBox = sender as TextBox;
                textBox.SelectAll();
                textBox.Focus();
            }
        }

        #endregion

        #region TextBoxLoaded

        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.IsVisible)
            {
                textBox.SelectAll();
                textBox.Focus();
            }
        }

        #endregion

        #region TreeviewItemMouseRightButtonDown

        private void TreeviewItemMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            item.IsSelected = true;
            e.Handled = true;

            //if (object.ReferenceEquals(item.Header, (e.OriginalSource as FrameworkElement).DataContext))
            //{
            //    item.IsSelected = true;
            //    e.Handled = true;
            //}
        }

        #endregion
    }
}