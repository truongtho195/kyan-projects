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

namespace RegexDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            regCheck();
        }

        private void regCheck()
        {
            //air (n) /eə/ không khí, bầu không khí, không gian
            //go bad bẩn thỉu, thối, hỏng
            //"act (n) (v)  hành động, hành vi, cử chỉ, đối xử"
            //beside prep. /bi'said/ bên cạnh, so với
            string strInput = "beside prep. /bi'said/ bên cạnh, so với";
            string LessonName = string.Empty;
            string Descrtiption = string.Empty;
            string BackSide = string.Empty;
            var indexFirst = strInput.IndexOf('(');
            var indexLast = strInput.LastIndexOf('/');
            if (indexFirst > -1)
            {
                LessonName = strInput.Substring(0, indexFirst);
                if (indexLast > -1)
                {

                }
                else if (strInput.LastIndexOf(')') > -1) //act (...) decre
                {
                    indexLast = strInput.LastIndexOf(')');
                }
                else
                {
                    indexLast = -1;
                }

                if (indexLast != -1)
                {
                    Descrtiption = strInput.Substring(indexFirst, indexLast - indexFirst + 1);
                    BackSide = strInput.Substring(indexLast + 1, strInput.Length - indexLast - 1);
                }
            }
            else
            { 
                // Unformat
            }

        }
    }
}
