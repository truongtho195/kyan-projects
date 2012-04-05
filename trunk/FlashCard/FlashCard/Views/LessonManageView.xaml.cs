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
using FlashCard.ViewModels;

namespace FlashCard
{
	/// <summary>
	/// Interaction logic for LessonManage.xaml
	/// </summary>
	public partial class LessonManageView : Window,IView
	{
        public LessonManageView()
		{
			this.InitializeComponent();
            viewModel = new Lazy<LessonViewModel>(() => ViewHelper.GetViewModel<LessonViewModel>(this));
            var a = new LessonViewModel(this).View;
        }

        #region Variables
        private readonly Lazy<LessonViewModel> viewModel;
        #endregion

        #region Properties

        #region Properties
        private LessonViewModel ViewModel { get { return viewModel.Value; } }
        #endregion
        #endregion
    }
}