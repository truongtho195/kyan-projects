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
using FlashCard.Helper;

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
        /// Set From another collection after handled will out to collection for study
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
                    _selectedSetupModel.ToModel();
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
                    if (SelectedCard != null && SelectedCard.Card.Lessons.Count > 0)
                    {
                        SelectedCard.CheckedAll = true;
                        SelectedCard.LessonCollection = new ObservableCollection<LessonModel>(SelectedCard.Card.Lessons.Select(x => new LessonModel(x)));
                        SetCheckValueForCollection(SelectedCard.CheckedAll.Value);
                        RaisePropertyChanged(() => TotalLesson);
                    }
                }
            }
        }
        #endregion

        #region" SelectedStudy"
        private StudyModel _selectedStudy;
        /// <summary>
        /// Gets or sets the SelectedStudy.
        /// </summary>
        public StudyModel SelectedStudy
        {
            get { return _selectedStudy; }
            set
            {
                if (_selectedStudy != value)
                {
                    _selectedStudy = value;
                    RaisePropertyChanged(() => SelectedStudy);
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
                    return SelectedCard.LessonCollection.Count(x => x.IsChecked);
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
            if (!SelectedSetupModel.Errors.Any() && SelectedCard.LessonCollection != null && SelectedCard.LessonCollection.Any(x => x.IsChecked))
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
                //Save setup
                App.SetupModel = SelectedSetupModel;
                SetupRepository setupRepository = new SetupRepository();
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

                // Get  & Set Lesson for study
                List<LessonModel> lst = new List<LessonModel>();
                lst.AddRange(SelectedCard.LessonCollection.Where(x => x.IsChecked));
                if (lst != null && lst.Count > 0)
                {
                    //Shuffle
                    LessonCollection = new List<LessonModel>(lst);
                    if (App.SetupModel.Setup.IsShuffle == true)
                    {
                        var lessonShuffle = ShuffleList.Randomize<LessonModel>(LessonCollection.ToList());
                        LessonCollection = new List<LessonModel>(lessonShuffle);
                    }

                    var LimitCardNum = LessonCollection.Count;
                    if (App.SetupModel.Setup.IsLimitCard == true)
                    {
                        if (App.SetupModel.Setup.LimitCardNum < LimitCardNum)
                            LimitCardNum = App.SetupModel.Setup.LimitCardNum.Value;
                        LessonCollection = new List<LessonModel>(LessonCollection.GetRange(0,LimitCardNum));
                    }
                }


                //Store last study 
                if (!SelectedCard.IsFile)
                {
                    // For Study 
                    // Set Last Lesson user Study
                    StudyRepository studyRespository = new StudyRepository();

                    //set IsLast
                    var studyAll = studyRespository.GetAll<Study>();
                    Study study;
                    if (studyAll.Count == 0)
                    {
                        study = new Study();
                        study.StudyID = AutoGeneration.NewSeqGuid();
                        study.LastStudyDate = DateTime.Today;
                        studyRespository.Add<Study>(study);
                        studyRespository.Commit();
                    }
                    else
                    {
                        study = studyAll.FirstOrDefault();
                    }

                    //reset data IsLastStudy == true => set all to false
                    foreach (var item in study.StudyDetails.Where(x => x.IsLastStudy == true))
                    {
                        item.IsLastStudy = false;
                        studyRespository.Update(item);
                    }
                    studyRespository.Commit();

                    //Store & set isLastStudy
                    foreach (var item in LessonCollection)
                    {
                        var studyDetail = study.StudyDetails.Where(x => x.LessonID.Equals(item.LessonID)).SingleOrDefault();
                        if (studyDetail != null)
                        {
                            studyDetail.IsLastStudy = true;
                        }
                        else
                        {
                            StudyDetail detail = new StudyDetail();
                            detail.StudyDetailID = AutoGeneration.NewSeqGuid();
                            detail.LessonID = item.LessonID;
                            detail.IsLastStudy = true;
                            detail.StudyID = study.StudyID;
                            study.StudyDetails.Add(detail);
                        }
                    }
                    studyRespository.Commit();
                }
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

        #region"  CheckedCommand"

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
            CheckedForList(param);
        }


        #endregion

        #region GetLastLesson

        #region GetLastLessonCommand
        private ICommand _getLastLessonCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand GetLastLessonCommand
        {
            get
            {
                if (_getLastLessonCommand == null)
                {
                    _getLastLessonCommand = new RelayCommand(this.GetLastLessonExecute, this.CanGetLastLessonExecute);
                }
                return _getLastLessonCommand;
            }
        }

        private bool CanGetLastLessonExecute(object param)
        {
            if (SelectedStudy.Study.StudyDetails.Count == 0)
                return false;
            return SelectedStudy.Study.StudyDetails.Where(x => x.IsLastStudy == true).Count() > 0;
        }

        private void GetLastLessonExecute(object param)
        {
            var lesson = SelectedStudy.Study.StudyDetails.Where(x => x.IsLastStudy == true).Select(x => x.Lesson).Distinct();

            var card = lesson.FirstOrDefault().Card;
            var selectCard = CardCollection.SingleOrDefault(x => x.Card.CardID.Equals(card.CardID));

            SelectedCard = selectCard;
            SelectedCard.CheckedAll = false;
            CheckedForList("Parent");

            foreach (var item in lesson)
            {
                var lessonChecked = SelectedCard.LessonCollection.Where(x => x.LessonID.Equals(item.LessonID)).SingleOrDefault();
                lessonChecked.IsChecked = true;
                CheckedForList("Child");
            }
        }
        #endregion

        #endregion
        #endregion

        #region Methods
        private void InitialData()
        {
            log.Info("|| {*} === InitialData ===");
            try
            {
                SelectedSetupModel = App.SetupModel;
                SelectedStudy = App.StudyModel;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }


        }

        private void SetCheckValueForCollection(bool isChecked)
        {
            foreach (var item in SelectedCard.LessonCollection)
            {
                item.IsChecked = isChecked;
            }

        }

        /// <summary>
        ///  Set checked from parent or child 
        /// </summary>
        /// <param name="param">Parent : set from parent to child</param>
        public void CheckedForList(object param)
        {
            if ("Parent".Equals(param))
            {
                SetCheckValueForCollection(SelectedCard.CheckedAll.Value);
            }
            else
            {
                SetCheckValueForParent();
                RaisePropertyChanged(() => TotalLesson);
            }
        }

        private void SetCheckValueForParent()
        {
            if (SelectedCard == null || (SelectedCard != null && SelectedCard.LessonCollection == null))
            {
                SelectedCard.CheckedAll = false;
                return;
            }

            if (SelectedCard.LessonCollection.Count == 0)
            {
                SelectedCard.CheckedAll = false;
                return;
            }
            bool? status = null;
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
