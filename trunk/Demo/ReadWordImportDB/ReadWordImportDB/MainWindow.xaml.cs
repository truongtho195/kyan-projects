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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReadWordImportDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReadTextFromPDF();
        }
        private PDFLibNet.PDFWrapper _pdfDoc;

        private void ReadMSWord()
        {
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = @"E:\3000.doc";
            object readOnly = true;
            Microsoft.Office.Interop.Word.Document docs = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);
            string totaltext = "";
            for (int i = 0; i < docs.Paragraphs.Count; i++)
            {
                totaltext += " \r\n " + docs.Paragraphs[i + 1].Range.Text.ToString();
            }
            Console.WriteLine(totaltext);
            docs.Close();
            word.Quit();
        }

        private void ReadTextFromPDF()
        {
            try
            {
                _pdfDoc = new PDFLibNet.PDFWrapper();

                _pdfDoc.UseMuPDF = true;
                _pdfDoc.LoadPDF("E://oxford3000.pdf");
                int TotalPage = _pdfDoc.Pages.Count();
                var result = _pdfDoc.PrintToFile("E://exportFromPDF.doc", 1, TotalPage);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
