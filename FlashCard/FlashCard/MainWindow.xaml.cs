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
    public partial class MainWindow : IView
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

    }
}
