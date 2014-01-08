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
using CPC.POSReport.ViewModel;
using Microsoft.Windows.Controls.Ribbon;
using POSReport;
using System.ComponentModel;

namespace CPC.POSReport.View
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : RibbonWindow
    {
        public Main()
        {            
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(Main_Closing);
            // crystalReport.ViewerCore.EnableDrillDown = false;       
            // reportImage.Cursor = CPC.POSReport.Function.CursorsHelper.CreateCursor(cursor, 5, 5);   

            //  DispatcherTimer setup
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            this.MinHeight = 700;
            this.MinWidth = 850;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            string hour, minute, second, day;
            day = string.Format("{0:MM/dd/yyyy}", DateTime.Now);
            hour = (DateTime.Now.Hour > 9) ? DateTime.Now.Hour.ToString() : "0" + DateTime.Now.Hour.ToString();
            minute = (DateTime.Now.Minute > 9) ? DateTime.Now.Minute.ToString() : "0" + DateTime.Now.Minute.ToString();
            second = (DateTime.Now.Second > 9) ? DateTime.Now.Second.ToString() : "0" + DateTime.Now.Second.ToString();
            txtblOClock.Text = day + " " + hour + ":" + minute + ":" + second;

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resuilt = Xceed.Wpf.Toolkit.MessageBox.Show("Do you realy want to exit?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (MessageBoxResult.Yes.Equals(resuilt))
            {
                App.Current.Shutdown();
            }
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Function.Common.IS_LOG_OUT)
            {
                MessageBoxResult resuilt = Xceed.Wpf.Toolkit.MessageBox.Show("Do you realy want to exit?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (MessageBoxResult.No.Equals(resuilt))
                {
                    e.Cancel = true;
                }
                else
                {
                    App.Current.Shutdown();
                }
            }
        }        
    }
}
