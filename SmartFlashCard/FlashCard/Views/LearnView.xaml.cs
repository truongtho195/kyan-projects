﻿using System.Windows;
using System.Windows.Input;

namespace FlashCard.Views
{
    /// <summary>
    /// Interaction logic for LearnView.xaml
    /// </summary>
    public partial class LearnView : Window
    {
        public LearnView()
        {
            InitializeComponent();
            InitialEvent();
            //Double current Fonsize
            this.tbLessonName.FontSize *= 1.5;
        }

        #region Events
        private void bdHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // ExitApplication(e);
        }

        private static void ExitApplication(RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to exit ? ", "Question.", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
                Application.Current.Shutdown();
            else e.Handled = true;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState.Equals(WindowState.Maximized))
                this.WindowState = WindowState.Normal;

        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                //   AltDown = true;
            }
            else if (e.SystemKey == Key.LeftAlt && e.SystemKey == Key.F4)
            {
                ExitApplication(e);
            }
        }


        #endregion


        #region Methods
        private void InitialEvent()
        {
            this.bdHeader.MouseLeftButtonDown += new MouseButtonEventHandler(bdHeader_MouseLeftButtonDown);
            this.btnMinimize.Click += new RoutedEventHandler(btnMinimize_Click);
            this.btnMaximize.Click += new RoutedEventHandler(btnMaximize_Click);
            // this.btnExit.Click += new RoutedEventHandler(btnExit_Click);
            this.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);
        }
        #endregion
    }
}