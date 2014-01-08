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
using POSApplicationServer.ViewModel;

namespace POSLicense
{
	/// <summary>
	/// Interaction logic for ApplicationServer.xaml
	/// </summary>
	public partial class ApplicationServer : Window
	{
		public ApplicationServer()
		{
			this.InitializeComponent();
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            btnMinimize.Click += new RoutedEventHandler(btnMinimize_Click);
            brdTopBar.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(brdTopBar_PreviewMouseLeftButtonDown);
            this.DataContext = new POSApplicationSeverViewModel();
            
		}

        void brdTopBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

       
	}
}