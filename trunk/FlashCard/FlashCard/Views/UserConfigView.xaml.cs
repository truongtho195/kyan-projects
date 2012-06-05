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
    /// Interaction logic for UserConfigView.xaml
    /// </summary>
    public partial class UserConfigView : Window, IView
    {
        public UserConfigView()
        {
            InitializeComponent();
            viewModel = new Lazy<UserConfigViewModel>(() => ViewHelper.GetViewModel<UserConfigViewModel>(this));
        }
        #region Properties
        private readonly Lazy<UserConfigViewModel> viewModel;
        #endregion
    }
}
