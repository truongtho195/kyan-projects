using System;
using Microsoft.Windows.Controls.Ribbon;
using System.Windows.Input;

namespace CPC.POS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            HiddenTabs();
            this.KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
        }

        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if(e.Key==Key.)
            //{
            //}
        }
        
        private void HiddenTabs()
        {
            // Inventory
            //(this.ribbon.Items[0] as RibbonTab).Visibility = System.Windows.Visibility.Collapsed;
            //(this.ribbon.Items[1] as RibbonTab).Visibility = System.Windows.Visibility.Collapsed;

            // Label Status
            this.tbStatus.Text = String.Empty;
        }
    }
}
