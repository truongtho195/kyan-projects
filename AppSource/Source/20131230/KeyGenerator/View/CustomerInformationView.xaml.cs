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
using KeyGenerator.Model;
using CPC.Toolkit.Base;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using CPC.Toolkit.Command;
using KeyGenerator.ViewModel;

namespace KeyGenerator
{
    /// <summary>
    /// Interaction logic for CustomerInformationView.xaml
    /// </summary>
    public partial class CustomerInformationView
    {
        #region Construtor
        public CustomerInformationView()
        {
            this.InitializeComponent();

            BrdTopBar.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(BrdTopBar_PreviewMouseLeftButtonDown);

            btnMinimize.Click += new RoutedEventHandler(btnMinimize_Click);
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            this.DataContext = new CustomerInformationViewModel();
        }

        
        #endregion


        #region Methods
        /// <summary>
        /// Set maximized for form with Current Screen
        /// </summary>
        private void MaximinzedScreen()
        {
            this.MaxWidth = SystemParameters.WorkArea.Width+2;
            this.MaxHeight = SystemParameters.WorkArea.Height+2;
            this.WindowState = WindowState.Maximized;
        }
        #endregion


        #region Events

        #region UI Event
        void BrdTopBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == System.Windows.WindowState.Maximized)
                    this.WindowState = System.Windows.WindowState.Normal;
                else
                    MaximinzedScreen();
            }
            else
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

        #endregion


    }
}