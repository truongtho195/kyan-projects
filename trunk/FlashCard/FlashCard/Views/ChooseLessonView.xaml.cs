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
using System.Waf.Applications;
using FlashCard.ViewModels;

namespace FlashCard.Views
{
    /// <summary>
    /// Interaction logic for ChooseLessonView.xaml
    /// </summary>
    public partial class ChooseLessonView :  Window,IView
    {
        public ChooseLessonView()
        {
            InitializeComponent();
            viewModel = new Lazy<ChooseLessonViewModel>(() => ViewHelper.GetViewModel<ChooseLessonViewModel>(this));
            var a = new ChooseLessonViewModel(this).View;
            InitialEvent();
        }
        #region Variables
        private readonly Lazy<ChooseLessonViewModel> viewModel;
        #endregion

        #region Events
        private void bdHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState.Equals(WindowState.Maximized))
                    this.WindowState = WindowState.Normal;
                else
                    MaximinzedScreen();
            }
            this.DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

       

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState.Equals(WindowState.Maximized))
                this.WindowState = WindowState.Normal;
            else
                MaximinzedScreen();

        }
        #endregion

        #region Methods

        /// <summary>
        /// Set maximized for form with Current Screen
        /// </summary>
        private void MaximinzedScreen()
        {
            this.MaxWidth = SystemParameters.WorkArea.Width + 5;
            this.MaxHeight = SystemParameters.WorkArea.Height + 5;
            this.WindowState = WindowState.Maximized;
        }
        private void InitialEvent()
        {
            this.bdHeader.MouseLeftButtonDown += new MouseButtonEventHandler(bdHeader_MouseLeftButtonDown);
            this.btnMinimize.Click += new RoutedEventHandler(btnMinimize_Click);
            this.btnMaximize.Click += new RoutedEventHandler(btnMaximize_Click);
            //this.btnExit.Click : call from command
            //this.btnExit.Click += new RoutedEventHandler(btnExit_Click);
        }
        #endregion

    }
}
