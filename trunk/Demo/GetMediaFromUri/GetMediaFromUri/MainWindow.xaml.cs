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
using System.Net;
using System.IO;

namespace GetMediaFromUri
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnGetMedia.Click += new RoutedEventHandler(btnGetMedia_Click);
        }

        private void btnGetMedia_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                MediaPlayer media = new MediaPlayer();
                string keyword = "Hello world";
                string strUrl = string.Format("{0}{1}&tl=en", "http://translate.google.com/translate_tts?q=", keyword);

                var ur = new Uri(strUrl, UriKind.RelativeOrAbsolute);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ur);
                WebResponse response = request.GetResponse();
                Stream strm = response.GetResponseStream();

                if (strm.CanRead)
                {
                    SaveStreamToFile(strm, string.Format("E:\\{0}.mp3",keyword));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void SaveStreamToFile(Stream stream, string filename)
        {
            using (Stream destination = File.Create(filename))
                Write(stream, destination);
        }

        //Typically I implement this Write method as a Stream extension method. 
        //The framework handles buffering.

        public void Write(Stream from, Stream to)
        {
            for (int a = from.ReadByte(); a != -1; a = from.ReadByte())
                to.WriteByte((byte)a);
        }
    }
}
