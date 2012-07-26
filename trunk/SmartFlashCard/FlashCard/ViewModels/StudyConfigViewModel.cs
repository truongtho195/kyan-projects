using System.Collections.Generic;
using System.Linq;
using FlashCard.Views;
using System.Waf.Applications;
using System.Collections.ObjectModel;
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

        #region Variables
        public delegate void handlerControl(string message);
        public event handlerControl ButtonClickHandler;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

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

        #region" SelectedCard"
        private CardModel _selectedCard;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CardModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    RaisePropertyChanged(() => SelectedCard);
                    if (SelectedCard != null && SelectedCard.Card.Lessons.Count>0)
                    {
                        SelectedCard.CheckedAll = true;
                        SelectedCard.LessonCollection = new ObservableCollection<LessonModel>(SelectedCard.Card.Lessons.Select(x=>new LessonModel(x)));
                        SetCheckValueForCollection();
                        RaisePropertyChanged(() => TotalLesson);
                    }
                }
            }
        } 
        #endregion

        #region" CheckedAll"
        private bool? _checkedAll;
        /// <summary>
        /// Gets or sets the CheckedAll.
        /// </summary>
        public bool? CheckedAll
        {
            get { return _checkedAll; }
            set
            {
                if (_checkedAll != value)
                {
                    _checkedAll = value;
                    RaisePropertyChanged(() => CheckedAll);
                    SetCheckValueForCollection();
                }
            }
        }
        #endregion


        #region" TotalLesson"
        /// <summary>
        /// Gets the TotalLesson.
        /// </summary>
        public int TotalLesson
        {
            get
            {
                if (SelectedCard != null && SelectedCard.LessonCollection != null)
                    return SelectedCard.LessonCollection.Count(x=>x.IsChecked);
                return 0;
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
            if (SelectedCard == null)
                return false;
            if (!SelectedSetupModel.Errors.Any() && SelectedCard.LessonCollection!=null && SelectedCard.LessonCollection.Any(x => x.IsChecked))
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
                //foreach (var item in SelectedCard.LessonCollection.Where(x => x.IsChecked))
                //{
                //    //Check condition if user set Lesson user Know => remove this item lesson
                //    if (lesson != null && lesson.Count() > 0)
                //    {
                //        lst.AddRange(lesson);
                //    }
                //}
                lst.AddRange(SelectedCard.LessonCollection.Where(x => x.IsChecked));
                if (lst != null && lst.Count > 0)
                {
                    LessonCollection = new List<LessonModel>(lst);
                }

                //Handle Setup
                App.SetupModel = SelectedSetupModel;

                SetupRepository setupRepository = new SetupRepository();
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                App.SetupModel.ToEntity();
                if (App.SetupModel.IsNew)
                {
                    setupRepository.Add<Setup>(App.SetupModel.Setup);
                    setupRepository.Commit();
                }
                else if (App.SetupModel.IsDirty)
                {
                    setupRepository.Update<Setup>(App.SetupModel.Setup);
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

        #region CheckedCommand

        /// <summary>
        /// Gets the Checked Command.
        /// <summary>
        private ICommand _checkedCommand;
        public ICommand CheckedCommand
        {
            get
            {
                if (_checkedCommand == null)
                    _checkedCommand = new RelayCommand(this.OnCheckedExecute, this.OnCheckedCanExecute);
                return _checkedCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Checked command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCheckedCanExecute(object param)
        {
            if ("Parent".Equals(param))
            {
                return SelectedCard != null && SelectedCard.LessonCollection != null && SelectedCard.LessonCollection.Count > 0;
            }
            return true;
        }

        /// <summary>
        /// Method to invoke when the Checked command is executed.
        /// </summary>
        private void OnCheckedExecute(object param)
        {
            if ("Parent".Equals(param))
            {
                SetCheckValueForCollection();
            }
            else
            {
                SetCheckValueForParent();
                RaisePropertyChanged(() => TotalLesson);
            }
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



        private void SetCheckValueForCollection()
        {
            foreach (var item in SelectedCard.LessonCollection)
            {
                item.IsChecked = SelectedCard.CheckedAll.HasValue ? SelectedCard.CheckedAll.Value : false;
            }
                
        }

        private void SetCheckValueForParent()
        {
            if (SelectedCard == null || (SelectedCard != null && SelectedCard.LessonCollection == null))
            {
                SelectedCard.CheckedAll = false;
                return;
            }

            if(SelectedCard.LessonCollection.Count==0)
            {
                SelectedCard.CheckedAll = false;
                return;
            }
            bool? status=null;
            for (int i = 0; i < SelectedCard.LessonCollection.Count; i++)
            {
                var lessonModel = SelectedCard.LessonCollection[i];
                bool? current = lessonModel.IsChecked;
                if (i == 0)
                {
                    status = current;
                }
                else if (status != current)
                {
                    status = null;
                    break;
                }
            }
            SelectedCard.CheckedAll = status;
        }


        #endregion


    }
}
