using System.Collections.Generic;
using System.Linq;
using FlashCard.Views;
using System.Waf.Applications;
using FlashCard.Models;
using System.Collections.ObjectModel;
using FlashCard.DataAccess;
using System.Windows.Input;
using MVVMHelper.Commands;
using System;
using log4net;
using FlashCard.Database;
using FlashCard.Database.Repository;

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
        public delegate void handlerControl(string message);
        public event handlerControl ButtonClickHandler;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        #region Titles
        private string _titles = string.Empty;
        /// <summary>
        /// Gets or sets the Titles.
        /// </summary>
        public string Titles
        {
            get { return _titles; }
            set
            {
                if (_titles != value)
                {
                    _titles = value;
                    RaisePropertyChanged(() => Titles);
                }
            }
        }
        #endregion


        #region "  CategoryCollection"
        private List<CardModel> _cardCollection;
        /// <summary>
        /// Gets or sets the CategoryCollection.
        /// </summary>
        public List<CardModel> CardCollection
        {
            get { return _cardCollection; }
            set
            {
                if (_cardCollection != value)
                {
                    _cardCollection = value;
                    RaisePropertyChanged(() => CardCollection);
                }
            }
        }
        #endregion

        #region "  LessonCollection"
        private List<LessonModel> _lessonCollection;
        /// <summary>
        /// Gets or sets the LessonCollection.
        /// </summary>
        public List<LessonModel> LessonCollection
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

                    if (_setupModel != null)
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
            if (!SelectedSetupModel.Errors.Any() && CardCollection.Any(x => x.Checked))
                return true;
            return false;
        }

        /// <summary>
        /// Method to invoke when the OK command is executed.
        /// </summary>
        private void OnOKExecute(object param)
        {
            log.Info("|| {*} === OK Command Executed ===");
            try
            {
                List<LessonModel> lst = new List<LessonModel>();
                foreach (var item in CardCollection.Where(x => x.Checked))
                {
                    //!!!! Not sure LessonCollection.Where(x => x.CategoryID == item.CategoryID);
                    var lesson = LessonCollection.Where(x => x.Lesson.CardID == item.Card.CardID);
                    //Check condition if user set Lesson user Know => remove this item lesson
                    if (lesson != null && lesson.Count() > 0)
                    {
                        lst.AddRange(lesson);
                    }
                }
                if (lst != null && lst.Count > 0)
                {
                    LessonCollection = new List<LessonModel>(lst);
                }

                //Handle Setup
                App.SetupModel = SelectedSetupModel;

                SetupRepository setupRepository = new SetupRepository();
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                if (App.SetupModel.IsNew)
                    setupRepository.Add(App.SetupModel.Setup);
                else if (App.SetupModel.IsDirty)
                {
                    setupRepository.Update(App.SetupModel.Setup);
                    setupRepository.Commit();
                   
                }
                App.SetupModel.EndUpdate();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            ButtonClickHandler.Invoke("OkExecute");

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
            log.Info("|| {*} === Cancel Command Executed ===");
            ButtonClickHandler.Invoke("CancelExecute");
        }
        #endregion
        #endregion

        #region Methods
        private void InitialData()
        {
            log.Info("|| {*} === InitialData ===");
            try
            {
                SelectedSetupModel = App.SetupModel;
                //CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                //var cate = categoryDataAccess.GetAllWithRelation().Where(x => x.LessonNum > 0);
                //CategoryCollection = new ObservableCollection<CategoryModel>(cate);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }


        }
        #endregion


    }
}
