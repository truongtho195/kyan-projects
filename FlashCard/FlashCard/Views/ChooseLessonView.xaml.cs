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
        }
        #region Variables
        private readonly Lazy<ChooseLessonViewModel> viewModel;
        #endregion
    }
}
