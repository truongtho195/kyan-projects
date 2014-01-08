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
using System.Windows.Shapes;
using POSReport;

namespace CPC.POSReport.View
{
	/// <summary>
	/// Interaction logic for test.xaml
	/// </summary>
	public partial class CustomerPaymentOptional : Window
	{
        public CustomerPaymentOptional()
		{
			this.InitializeComponent();
            // Do not show form in taskbar
            this.ShowInTaskbar = false;
            // Set Owner of view 
            this.Owner = App.Current.MainWindow;
		}

        private void TestWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}