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
using POSReport;

namespace CPC.POSReport.View
{
	/// <summary>
    /// Interaction logic for ReportPermissionView.xaml
	/// </summary>
    public partial class ReportPermissionView
	{
        public ReportPermissionView()
		{
			this.InitializeComponent();
		}

        public void ShowDialog(ReportPermissionView view, string reportName)
        {
            this.DataContext = new CPC.POSReport.ViewModel.ReportPermissionViewModel(view, reportName);
            this.ShowInTaskbar = false;
            this.Owner = App.Current.MainWindow;
            this.ShowDialog();
        }

        private void BrdTopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
	}
}