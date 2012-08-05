using System;
using System.Windows;
using System.Waf.Applications;
using FlashCard.ViewModels;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private readonly Lazy<MainViewModel> viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new Lazy<MainViewModel>(() => ViewHelper.GetViewModel<MainViewModel>(this));
            var a = new MainViewModel(this).View;
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }


        #region Properties
        private MainViewModel ViewModel { get { return viewModel.Value; } }
        #endregion

        #region Methods
        public void CloseForm()
        {
            this.Close();
        }
        #endregion

    }
}
