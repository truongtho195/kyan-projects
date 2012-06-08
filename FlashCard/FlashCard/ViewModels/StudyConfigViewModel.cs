﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlashCard.Views;
using System.Waf.Applications;
using FlashCard.Model;
using System.Collections.ObjectModel;
using FlashCard.DataAccess;
using System.Windows.Input;
using MVVMHelper.Commands;

namespace FlashCard.ViewModels
{
    public class StudyConfigViewModel : ViewModel<StudyConfigView>
    {
        #region Constructors
        public StudyConfigViewModel(StudyConfigView view)
            : base(view)
        {
            InitialData();
        }
        #endregion

        #region Properties
        #region "  CategoryCollection"
        private ObservableCollection<CategoryModel> _categoryCollection;
        /// <summary>
        /// Gets or sets the CategoryCollection.
        /// </summary>
        public ObservableCollection<CategoryModel> CategoryCollection
        {
            get { return _categoryCollection; }
            set
            {
                if (_categoryCollection != value)
                {
                    _categoryCollection = value;
                    RaisePropertyChanged(() => CategoryCollection);
                }
            }
        }
        #endregion

        #region "  LessonCollection"
        private ObservableCollection<LessonModel> _lessonCollection;
        /// <summary>
        /// Gets or sets the LessonCollection.
        /// </summary>
        public ObservableCollection<LessonModel> LessonCollection
        {
            get { return _lessonCollection; }
            set
            {
                if (_lessonCollection != value)
                {
                    _lessonCollection = value;
                    RaisePropertyChanged(() => LessonCollection);
                }
            }
        }
        #endregion


        #region "  SetupModel"
        private SetupModel _setupModel;
        /// <summary>
        /// Gets or sets the SetupModel.
        /// <para>Property For communicate another class</para>
        /// </summary>
        public SetupModel SetupModel
        {
            get { return _setupModel; }
            set
            {
                if (_setupModel != value)
                {
                    _setupModel = value;
                    SelectedSetupModel = new SetupModel();
                    SelectedSetupModel = _setupModel;
                    RaisePropertyChanged(() => SetupModel);
                }
            }
        }
        #endregion

        #region "  SelectedSetupModel"
        private SetupModel _selectedSetupModel;
        /// <summary>
        /// Gets or sets the SelectedSetupModel.
        /// <para>Property For Binding in view</para>
        /// </summary>
        public SetupModel SelectedSetupModel
        {
            get { return _selectedSetupModel; }
            set
            {
                if (_selectedSetupModel != value)
                {
                    _selectedSetupModel = value;
                    RaisePropertyChanged(() => SelectedSetupModel);
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        #region "  OK Command"
        /// <summary>
        /// Gets the OK Command.
        /// <summary>
        private ICommand _okCommand;
        public ICommand OKCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new RelayCommand(this.OnOKExecute, this.OnOKCanExecute);
                return _okCommand;
            }
        }

        /// <summary>
        /// Method to check whether the OK command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOKCanExecute(object param)
        {
            if (!SelectedSetupModel.Errors.Any() && CategoryCollection.Any(x=>x.IsChecked))
                return true;
            return false;
        }

        /// <summary>
        /// Method to invoke when the OK command is executed.
        /// </summary>
        private void OnOKExecute(object param)
        {
            List<LessonModel> lst = new List<LessonModel>();
            LessonDataAccess lessonDA = new LessonDataAccess();
            foreach (var item in CategoryCollection.Where(x => x.IsChecked))
            {
                var lesson = lessonDA.GetAllWithRelation().Where(x => x.CategoryModel.CategoryID == item.CategoryID);
                //Check condition if user set Lesson user Know => remove this item lesson
                if (lesson != null && lesson.Count()>0)
                {
                    lst.AddRange(lesson);
                }
            }
            if (lst != null && lst.Count>0)
            {
                LessonCollection = new ObservableCollection<LessonModel>(lst);
            }

            //Handle Setup
            SetupModel = SelectedSetupModel;
            this.ViewCore.DialogResult = true;
            this.ViewCore.Close();
            
        }
        #endregion

        #region "  CancelCommand"

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>
        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(this.OnCancelExecute, this.OnCancelCanExecute);
                return _cancelCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelExecute(object param)
        {
            ViewCore.DialogResult = false;
            ViewCore.Close();
        }
        #endregion
        #endregion

        

        #region Methods
        private void InitialData()
        {
            CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
            CategoryCollection = new ObservableCollection<CategoryModel>(categoryDataAccess.GetAll());
        }
        #endregion


    }
}
