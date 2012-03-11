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
using FlashCard.DataAccess;
using System.Waf.Applications;
using FlashCard.ViewModels;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow :IView
    {
        //FancyBalloon balloon;

        //DispatcherTimer timer;
        private readonly Lazy<MainViewModel> viewModel;

//        List<string> tipList = new List<string>();
  //      Random random = new Random();
       
        public MainWindow()
        {
            InitializeComponent();

            viewModel = new Lazy<MainViewModel>(() => ViewHelper.GetViewModel<MainViewModel>(this));
            var a= new MainViewModel(this).View;
        }


        #region Properties
        private MainViewModel ViewModel { get { return viewModel.Value; } }
        #endregion

     

        void balloon_MouseLeave(object sender, MouseEventArgs e)
        {
            //timer.Start();
            if (MyNotifyIcon.IsLoaded)
            {
                //Thread.Sleep(4000);
                MyNotifyIcon.CloseBalloon();
            }
        }

        void balloon_MouseEnter(object sender, MouseEventArgs e)
        {
         //   timer.Stop();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < 5; i++)
            {

                Thread.Sleep(1000);
                //balloon.inform = "Custom Balloon";
                //MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, null);
                Thread.Sleep(6000);
                MyNotifyIcon.HideBalloonTip();

            }
        }


        private void MainViewModel()
        {
            CategoryDataAccess cate = new CategoryDataAccess();
            cate.GetAllWithRelation();
        }


    }
}
