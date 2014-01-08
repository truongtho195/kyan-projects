using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CPC.POSReport.View.PopupForm.Report.ADOPDF
{
    public partial class pdfViewer : UserControl
    {
        public pdfViewer()
        {
            InitializeComponent();
        }
        public pdfViewer(string fileName)
        {
            InitializeComponent();
            try
            {
                pdfReportFile.LoadFile(fileName);
                //pdfReportFile.src = fileName;
                //pdfReportFile.Show();
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
