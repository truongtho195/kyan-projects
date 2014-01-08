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
    /// Interaction logic for _1.xaml
    /// </summary>
    public partial class PermissionView : Window
    {
        public PermissionView()
        {
            InitializeComponent();
        }

        public void ShowDialog(PermissionView permissionView)
        {
            this.DataContext = new ViewModel.PermissionViewModel(permissionView);
            this.ShowInTaskbar = false;
            this.Owner = App.Current.MainWindow;
            this.ShowDialog();
        }

        private void BrdTopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                this.DragMove();
            }
        }
    }
}
