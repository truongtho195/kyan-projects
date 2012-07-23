using System;
using System.Collections.Generic;
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
using System.Linq;
using FlashCard.ViewModels;
using FlashCard.Database;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for LessonManage.xaml
    /// </summary>
    public partial class LessonManageView : Window, IView
    {
        //private readonly HashSet<ValidationError> errors = new HashSet<ValidationError>();
        public LessonManageView()
        {
            this.InitializeComponent();
            viewModel = new Lazy<LessonViewModel>(() => ViewHelper.GetViewModel<LessonViewModel>(this));
            var a = new LessonViewModel(this).View;
            InitialEvent();
        }
        public LessonManageView(List<LessonModel> lessonCollection)
        {
            this.InitializeComponent();
            viewModel = new Lazy<LessonViewModel>(() => ViewHelper.GetViewModel<LessonViewModel>(this));
            var a = new LessonViewModel(this, lessonCollection).View;
            InitialEvent();
        }

        //public LessonManageView(bool isFromPopup)
        //{
        //    this.InitializeComponent();
        //    viewModel = new Lazy<LessonViewModel>(() => ViewHelper.GetViewModel<LessonViewModel>(this));
        //    var a = new LessonViewModel(this, isFromPopup).View;
        //    InitialEvent();
        //}

        private void ErrorChangedHandler(object sender, ValidationErrorEventArgs e)
        {
            //if (e.Action == ValidationErrorEventAction.Added)
            //{
            //    errors.Add(e.Error);
            //}
            //else
            //{
            //    errors.Remove(e.Error);
            //}
           // ViewModel.IsValid = !errors.Any();
        }

        #region Variables
        private readonly Lazy<LessonViewModel> viewModel;
        #endregion

        #region Properties
        private LessonViewModel ViewModel { get { return viewModel.Value; } }
        
        #endregion

        #region Events
        private void bdHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState.Equals(WindowState.Maximized))
                {
                    this.WindowState = WindowState.Normal;
                    rtShdB.Visibility = Visibility.Visible;
                    rtShdBR.Visibility = Visibility.Visible;
                    rtShdR.Visibility = Visibility.Visible;
                }
                else
                {
                    rtShdB.Visibility = Visibility.Collapsed;
                    rtShdBR.Visibility = Visibility.Collapsed;
                    rtShdR.Visibility = Visibility.Collapsed;
                    MaximinzedScreen();
                }
            }
            this.DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication(e);
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState.Equals(WindowState.Maximized))
            {
                this.WindowState = WindowState.Normal;
                rtShdB.Visibility = Visibility.Visible;
                rtShdBR.Visibility = Visibility.Visible;
                rtShdR.Visibility = Visibility.Visible;
            }
            else
            {
                rtShdB.Visibility =  Visibility.Collapsed;
                rtShdBR.Visibility = Visibility.Collapsed;
                rtShdR.Visibility =  Visibility.Collapsed;
                MaximinzedScreen();
            }

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
        /// <summary>
        /// Set maximized for form with Current Screen
        /// </summary>
        private void MaximinzedScreen()
        {
            this.MaxWidth = SystemParameters.WorkArea.Width;
            this.MaxHeight = SystemParameters.WorkArea.Height;
            this.WindowState = WindowState.Maximized;
        }

        private void ExitApplication(RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(this, "Do you want to exit ? ", "Question.", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
                Application.Current.Shutdown();
            else e.Handled = true;
        }

        private void InitialEvent()
        {
            this.bdHeader.MouseLeftButtonDown += new MouseButtonEventHandler(bdHeader_MouseLeftButtonDown);
            this.btnMinimize.Click += new RoutedEventHandler(btnMinimize_Click);
            this.btnMaximize.Click += new RoutedEventHandler(btnMaximize_Click);
            this.btnExit.Click += new RoutedEventHandler(btnExit_Click);
            this.btnClose.Click += new RoutedEventHandler(btnExit_Click);
            this.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);
        }

        
        #endregion
    }
}