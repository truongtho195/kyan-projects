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
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Data.SQLite;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FancyBalloon balloon;
        DispatcherTimer timer;

        List<string> tipList = new List<string>();
        Random random = new Random();
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel();
            tipList.Add("An gi hom nay ?");
            tipList.Add("Bạn đang suy nghĩ gì?");
            tipList.Add("Cố lên bạn nhé!");
            tipList.Add("Chúc bạn luôn may mắn nhé");

            //balloon.BalloonText = "Custom Balloon";
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 20);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            this.Hide();
            //MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 4000);
        }


        void timer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("Show now");
            balloon = new FancyBalloon();
            balloon.MouseEnter += new MouseEventHandler(balloon_MouseEnter);
            balloon.MouseLeave += new MouseEventHandler(balloon_MouseLeave);
            int ram = random.Next(0, tipList.Count());
            Debug.WriteLine(ram);
            balloon.BalloonText = tipList[ram];
            timer.Stop();
            MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
            timer.Start();
            //Thread.Sleep(4000);
            // MyNotifyIcon.CloseBalloon();
            Debug.WriteLine("Show end");
        }

        void balloon_MouseLeave(object sender, MouseEventArgs e)
        {
            timer.Start();
            if (MyNotifyIcon.IsLoaded)
            {
                Thread.Sleep(4000);
                MyNotifyIcon.CloseBalloon();
            }
        }

        void balloon_MouseEnter(object sender, MouseEventArgs e)
        {
            timer.Stop();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < 5; i++)
            {

                Thread.Sleep(1000);
                balloon.BalloonText = "Custom Balloon";
                MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, null);
                Thread.Sleep(6000);
                MyNotifyIcon.HideBalloonTip();

            }
        }


        string sqlConnection;
        private void MainViewModel()
        {
            sqlConnection = "Data Source=SmartFlashCardDB.s3db";
            DataTable dt = new DataTable();

            try
            {

                SQLiteConnection cnn = new SQLiteConnection(sqlConnection);
                cnn.Open();

                SQLiteCommand mycommand = new SQLiteCommand(cnn);

                mycommand.CommandText = "select * from Users";

                SQLiteDataReader reader = mycommand.ExecuteReader();

                dt.Load(reader);

                reader.Close();

                cnn.Close();

            }

            catch (Exception e)
            {
                CatchException(e);
            }





        }

        private void CatchException(Exception ex)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append("\n\n||========Flash Card Error==========");
            builder.Append("\n||Source : ");
            builder.Append(ex.Source);
            builder.Append("\n||");
            builder.Append("\n||Message :");
            builder.Append(ex.Message);
            builder.Append("\n||");
            builder.Append("\n||All :");
            builder.Append(ex.Message);
            builder.Append("\n||");
            Debug.WriteLine(builder.ToString());
        }


    }
}
