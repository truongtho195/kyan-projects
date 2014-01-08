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
using System.Windows.Forms;

namespace CPC.POSReport.View.PopupForm.Report.ADOPDF
{
    /// <summary>
    /// Interaction logic for PDFView.xaml
    /// </summary>
    public partial class PDFView : Window
    {
        public PDFView()
        {            
            InitializeComponent();
        }
        public PDFView(string fileName)
        {
            InitializeComponent();
            this.MinHeight = 600;
            this.MinWidth = 800;
            this.WindowState = System.Windows.WindowState.Maximized;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            pdfViewer pdf = new pdfViewer(fileName);
            windowsFromsHost.Child = pdf;
            this.Show();
        }
    }
}
