using System;
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
using System.Windows.Shapes;
using POSReport;

namespace CPC.POSReport.View
{
    /// <summary>
    /// Interaction logic for CCToView.xaml
    /// </summary>
    public partial class CCToView : Window
    {
        public CCToView()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Owner = App.Current.MainWindow;
        }

        private void CCToForm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
