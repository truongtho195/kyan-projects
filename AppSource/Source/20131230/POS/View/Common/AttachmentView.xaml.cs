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
using System.Windows.Threading;
using System.Threading;
using Xceed.Wpf.Toolkit;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for TimeClockCorrection.xaml
    /// </summary>
    public partial class AttachmentView
    {
        #region Contructor

        public AttachmentView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Events

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
            if (textBox.Visibility == Visibility.Visible)
            {
                textBox.SelectAll();
                textBox.Focus();
            }
        }

        #endregion

        #region WatermarkTextBoxKeyDown

        private void WatermarkTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtSearch.GetBindingExpression(WatermarkTextBox.TextProperty).UpdateSource();
            }
        }

        #endregion

        #endregion
    }
}