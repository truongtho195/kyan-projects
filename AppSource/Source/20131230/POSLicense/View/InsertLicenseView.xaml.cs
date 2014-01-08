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
using POSLicense.ViewModel;

namespace POSLicense
{
	/// <summary>
	/// Interaction logic for InsertLicense.xaml
	/// </summary>
	public partial class InsertLicenseView : Window
	{
        #region Ctor
        public InsertLicenseView()
        {
            this.InitializeComponent();
            BrdTopBar.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(BrdTopBar_PreviewMouseLeftButtonDown);
            btnMinimize.Click += new RoutedEventHandler(btnMinimize_Click);
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            this.DataContext = new InsertLicenseViewModel();
        }
        
        #endregion

        #region Event
        void BrdTopBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        #endregion
    }
}