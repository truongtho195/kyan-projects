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
    /// Interaction logic for ChangGroup.xaml
    /// </summary>
    public partial class ChangeGroupView : Window
    {
        public ChangeGroupView()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Owner = App.Current.MainWindow;
        }

        private void BrdTopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
