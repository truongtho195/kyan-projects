using System;
using System.Collections.Generic;
using System.Linq;
using System.Waf.Applications;
using FlashCard.DataAccess;
using FlashCard.Model;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MVVMHelper.Commands;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Media.Animation;
using System.Windows;
using FlashCard.Views;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;
using FlashCard.Helper;
using System.Speech.Synthesis;
using System.Threading;
using System.ComponentModel;
using log4net;

namespace FlashCard.ViewModels
{
    public partial class MainViewModel : ViewModel<MainWindow>
    {
        #region Constructors
        public MainViewModel(MainWindow view)
            : base(view)
        {
            Initialize();
        }

        public void ExcuteMainForm()
        {
            if (App.SetupModel.IsEnableSlideShow)
            {
                InitialTimer();
                _timerPopup.Start();
            }
            else
            {
                SelectedLesson = LessonCollection.First();
                SelectedLesson.IsBackSide = false;
                ShowPopupForm();
            }
        }

        #endregion

        #region Variables
        public DispatcherTimer _timerPopup;
        DispatcherTimer _timerViewFullScreen;
        DispatcherTimer _waitForClose;
        FancyBalloon _balloon;
        Stopwatch _swCountTimerTick = new Stopwatch();
        LearnView _learnView;
        private int _currentItemIndex = 0;
        public int TimerCount { get; set; }
        public bool IsMouseEnter { get; set; }
        private MediaPlayer _listenWord;

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// When user Click to Hidden Button in popup, Mouse leave Executed. So TimerPopup call with wrong way 
        /// Need to set true if User Click to Hidden for poupup & set false when popup show again
        /// </summary>
        #endregion

        #region Properties

        #region Views
        private string _titles;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Titles
        {
            get
            {
                return _titles;
            }
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

        #region "  SelectedLesson"
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private LessonModel _selectedLesson;
        public LessonModel SelectedLesson
        {
            get { return _selectedLesson; }
            set
            {
                if (_selectedLesson != value)
                {
                    _selectedLesson = value;
                    RaisePropertyChanged(() => SelectedLesson);
                }
            }
        }
        #endregion

        #region "  SetupModel"
        //private SetupModel _setupModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public SetupModel SetupModel
        {
            get { return App.SetupModel; }
            //get { return _setupModel; }
            //set
            //{
            //    if (_setupModel != value)
            //    {
            //        _setupModel = value;
            //        RaisePropertyChanged(() => SetupModel);
            //    }
            //}
        }
        #endregion

        #region "  LessonCollection"
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private ObservableCollection<LessonModel> _lessonCollection;
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

        #region "  IsOtherFormShow"
        /// <summary>
        /// Set value for scenario : When OnPlayPauseExecute call this form, create timer 
        /// </summary>
        public bool IsOtherFormShow { get; set; }
        #endregion

        #region "  IsPopupStarted"
        private bool _isPopupStarted;
        /// <summary>
        /// This property Flag for PlayPause Popup
        /// </summary>
        public bool IsPopupStarted
        {
            get { return _isPopupStarted; }
            set
            {
                if (_isPopupStarted != value)
                {
                    _isPopupStarted = value;
                    if (_isPopupStarted)
                    {
                        IsCurrentStarted = true;
                        IsFullScreenStarted = false;
                    }

                    RaisePropertyChanged(() => IsPopupStarted);
                }
            }
        }
        #endregion

        #region"  IsFullScreenStarted"

        private bool _isFullSreenStarted;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsFullScreenStarted
        {
            get { return _isFullSreenStarted; }
            set
            {
                if (_isFullSreenStarted != value)
                {
                    _isFullSreenStarted = value;
                    if (_isFullSreenStarted)
                        IsPopupStarted = false;
                    RaisePropertyChanged(() => IsFullScreenStarted);
                }
            }
        }

        #endregion

        #region "  IconStatus"
        private string _iconStatus = @"/Icons/credit_card.ico";
        /// <summary>
        /// Gets or sets the IconStatus.
        /// </summary>
        public string IconStatus
        {
            get
            {
                if (IsCurrentStarted)
                {
                    _iconStatus = "/Icons/card_green.ico";
                }
                else
                {
                    _iconStatus = "/Icons/card_red.ico";
                }
                return _iconStatus;
            }
            //set
            //{
            //    if (_iconStatus != value)
            //    {
            //        _iconStatus = value;
            //        RaisePropertyChanged(() => IconStatus);
            //    }
            //}
        }
        #endregion

        #region "  IsCurrentStarted"
        private bool _isCurrentStarted;
        /// <summary>
        /// Property for set icon Ballon in tasbar
        /// </summary>
        public bool IsCurrentStarted
        {
            get
            {

                return _isCurrentStarted;
            }
            set
            {
                if (_isCurrentStarted != value)
                {
                    _isCurrentStarted = value;
                    RaisePropertyChanged(() => IsCurrentStarted);
                    RaisePropertyChanged(() => IconStatus);
                }
            }
        }
        #endregion
        #endregion

        #region Commands
        #region "  Change Side Command"
        /// <summary>
        /// SaveCommand
        /// <summary>
        private ICommand _changeSideCommand;
        public ICommand ChangeSideCommand
        {
            get
            {
                if (_changeSideCommand == null)
                {
                    _changeSideCommand = new RelayCommand(this.ChangeSideExecute, this.CanChangeSideExecute);
                }
                return _changeSideCommand;
            }
        }

        private bool CanChangeSideExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return true;
        }

        private void ChangeSideExecute(object param)
        {
            log.Info("||{*} === Change Side Command Executed === ");
            SelectedLesson.IsBackSide = !SelectedLesson.IsBackSide;
            if ("Popup".Equals(param.ToString()))
            {
                Storyboard sb;
                if (SelectedLesson.IsBackSide)
                    sb = (Storyboard)_balloon.FindResource("sbChangeToBack");
                else
                    sb = (Storyboard)_balloon.FindResource("sbChangeToFront");
                _balloon.BeginStoryboard(sb);
            }
            else
            {
                //sbBackSide
                //Storyboard sb;
                //if (SelectedLesson.IsBackSide)
                //    sb = (Storyboard)_learnView.FindResource("sbBackSide");
                //else
                //    sb = (Storyboard)_learnView.FindResource("sbFrontSide");
                //_learnView.BeginStoryboard(sb);

                if (App.SetupModel.IsEnableSlideShow)
                {
                    DispatcherTimer stopChangeLesson = new DispatcherTimer();
                    stopChangeLesson.Interval = new TimeSpan(0, 0, 0, 3);
                    stopChangeLesson.Tick += new EventHandler(waitUserClick_Tick);
                    _timerViewFullScreen.Stop();
                    stopChangeLesson.Start();
                }
            }

        }


        #endregion

        #region "  Fancy Ballon Mouse Enter Command"
        private ICommand _fancyBallonMouseEnterCommand;
        public ICommand FancyBallonMouseEnterCommand
        {
            get
            {
                if (_fancyBallonMouseEnterCommand == null)
                {
                    _fancyBallonMouseEnterCommand = new RelayCommand(this.FancyBallonMouseEnterExecute, this.CanFancyBallonMouseEnterExecute);
                }
                return _fancyBallonMouseEnterCommand;
            }
        }

        private bool CanFancyBallonMouseEnterExecute(object param)
        {
            if (!App.SetupModel.IsEnableSlideShow)
                return false;
            return true;
        }

        private void FancyBallonMouseEnterExecute(object param)
        {
            var action = new Action(() =>
            {
                log.Info("||{*} === Fancy Ballon Mouse Enter Command Executed === ");
                if (ViewCore.MyNotifyIcon.IsPopupOpen)
                {
                    _waitForClose.Stop();
                    _timerPopup.Stop();
                }
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }
        #endregion

        #region "  Fancy Ballon Mouse Leave Command"
        /// <summary>
        /// SaveCommand
        /// <summary>
        private ICommand _fancyBallonMouseLeaveCommand;
        public ICommand FancyBallonMouseLeaveCommand
        {
            get
            {
                if (_fancyBallonMouseLeaveCommand == null)
                {
                    _fancyBallonMouseLeaveCommand = new RelayCommand(this.FancyBallonMouseLeaveExecute, this.CanFancyBallonMouseLeaveExecute);
                }
                return _fancyBallonMouseLeaveCommand;
            }
        }

        private bool CanFancyBallonMouseLeaveExecute(object param)
        {
            if (!App.SetupModel.IsEnableSlideShow)
                return false;
            return true;
        }

        private void FancyBallonMouseLeaveExecute(object param)
        {
            log.Info("||{*} === Fancy Ballon Mouse Leave  Command Executed === ");
            if (!IsOtherFormShow && this.IsPopupStarted == true)
            {
                _swCountTimerTick.Stop();
                int time = 0;
                if (_swCountTimerTick.Elapsed.Seconds < App.SetupModel.ViewTimeSecond)
                    time = App.SetupModel.ViewTimeSecond - _swCountTimerTick.Elapsed.Seconds + 1;
                else
                    time = 2;
                var timerSpan = new TimeSpan(0, 0, 0, time);
                TimerForClosePopup(timerSpan);
                _swCountTimerTick.Reset();
                //Create timer popup cause When Mouse Enter _timerPopup is Stoped
                if (_timerPopup != null)
                    _timerPopup.Start();
            }
        }
        #endregion

        #region "  ExitCommand"
        /// <summary>
        /// Gets the Exit Command.
        /// <summary>
        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand(this.OnExitExecute, this.OnExitCanExecute);
                return _exitCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Exit command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnExitCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Exit command is executed.
        /// </summary>
        private void OnExitExecute(object param)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region "  Play Pause Command"
        /// <summary>
        /// Gets the PlayPause Command.
        /// <summary>
        private ICommand _playPauseCommand;
        public ICommand PlayPauseCommand
        {
            get
            {
                if (_playPauseCommand == null)
                    _playPauseCommand = new RelayCommand(this.OnPlayPauseExecute, this.OnPlayPauseCanExecute);
                return _playPauseCommand;
            }
        }

        /// <summary>
        /// Method to check whether the PlayPause command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPlayPauseCanExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the PlayPause command is executed.
        /// </summary>
        private void OnPlayPauseExecute(object param)
        {
            log.Info("||{*} === Play Pause Command Executed === ");
            if (App.SetupModel.IsEnableSlideShow)
            {
                if ("FullScreen".Equals(param.ToString()))
                {
                    if (_timerViewFullScreen.IsEnabled)
                    {
                        IsFullScreenStarted = false;
                        _timerViewFullScreen.Stop();
                    }
                    else
                    {
                        IsFullScreenStarted = true;
                        _timerViewFullScreen.Start();
                    }
                }
                else
                    PlayPauseBallonPopup(false);
            }
        }

        #endregion

        #region "{R}  ChooseLessonCommand"

        //private void UserConfigStudies()
        //{

        //    if (IsPopupStarted)
        //        PlayPauseBallonPopup(true);
        //    else
        //    {
        //        if (_timerViewFullScreen != null && _timerViewFullScreen.IsEnabled)
        //        {
        //            _timerViewFullScreen.Stop();
        //        }
        //    }
        //    StudyConfigView lessionView = new StudyConfigView();
        //    lessionView.GetViewModel<StudyConfigViewModel>().App.SetupModel = App.SetupModel;
        //    var cate = from x in LessonCollection
        //               group x by x.CategoryID into c
        //               select new CategoryModel
        //               {

        //                   CategoryID = c.Select(k => k.CategoryModel.CategoryID).First(),
        //                   CategoryName = c.Select(f => f.CategoryModel.CategoryName).First()

        //               };


        //    lessionView.GetViewModel<StudyConfigViewModel>().CategoryList = cate.ToList();
        //    //if (lessionView.ShowDialog() == true)
        //    //{
        //    var viewModel = lessionView.GetViewModel<StudyConfigViewModel>();

        //    App.SetupModel = viewModel.App.SetupModel;
        //    if (this.App.SetupModel.IsShuffle)
        //    {
        //        var lessonShuffle = ShuffleList.Randomize<LessonModel>(viewModel.LessonCollection);
        //        LessonCollection = new ObservableCollection<LessonModel>(lessonShuffle);
        //    }
        //    else
        //        LessonCollection = viewModel.LessonCollection;


        //    //set time for ballon popup
        //    if (_timerPopup != null)
        //        _timerPopup.Interval = App.SetupModel.TimeOut;
        //    if (_timerViewFullScreen != null)
        //        _timerViewFullScreen.Interval = new TimeSpan(0, 0, this.App.SetupModel.ViewTimeSecond);


        //    //}

        //    if (App.SetupModel.IsEnableSlideShow)
        //    {
        //        if (IsPopupStarted)
        //        {
        //            PlayPauseBallonPopup(false);
        //        }
        //        else
        //        {
        //            if (_timerViewFullScreen != null && !_timerViewFullScreen.IsEnabled)
        //            {
        //                _timerViewFullScreen.Start();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (IsPopupStarted)
        //        {
        //            SelectedLesson = LessonCollection.First();
        //            SelectedLesson.IsBackSide = false;
        //            _balloon = new FancyBalloon();
        //            _timerPopup.Stop();
        //            _waitForClose.Stop();
        //            ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
        //        }
        //    }
        //}

        #endregion

        #region "  ShowPopupCommand"
        private ICommand _showPopupCommand;
        //Gets or sets the property value
        public ICommand ShowPopupCommand
        {
            get
            {
                if (_showPopupCommand == null)
                {
                    _showPopupCommand = new RelayCommand(this.ShowPopupExecute, this.CanShowPopupExecute);
                }
                return _showPopupCommand;
            }
        }

        private bool CanShowPopupExecute(object param)
        {
            if (!App.SetupModel.IsEnableSlideShow)
                return true;
            return false;
        }

        private void ShowPopupExecute(object param)
        {
            log.Info("||{*} === Show Popup Command Executed === ");
            ShowPopupForm();
        }
        #endregion

        #region "  ListenCommand"
        private ICommand _listenCommand;
        //Gets or sets the property value
        public ICommand ListenCommand
        {
            get
            {
                if (_listenCommand == null)
                {
                    _listenCommand = new RelayCommand(this.ListenExecute, this.CanListenExecute);
                }
                return _listenCommand;
            }
        }

        private bool CanListenExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return true;
        }
        private void ListenExecute(object param)
        {
            try
            {
                log.Info("||{*} === Listen Command Executed === ");
                DispatcherTimer stopForListen = new DispatcherTimer();
                stopForListen = new DispatcherTimer();
                stopForListen.Interval = new TimeSpan(0, 0, 0, 5);
                stopForListen.Tick += new EventHandler(waitUserClick_Tick);

                if (CheckConnectionInternet.IsConnectedToInternet())
                {
                    log.DebugFormat("|| == Listen with google translate : {0}", SelectedLesson.LessonName);
                    _listenWord = new MediaPlayer();
                    string keyword = string.Format("{0}{1}&tl=en", "http://translate.google.com/translate_tts?q=", SelectedLesson.LessonName);
                    var ur = new Uri(keyword, UriKind.RelativeOrAbsolute);
                    _listenWord.Open(ur);
                    _listenWord.Play();
                }
                else
                {
                    log.DebugFormat("|| == Listen with Microsoft Speed : {0}", SelectedLesson.LessonName);
                    SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                    synthesizer.SpeakAsync(SelectedLesson.LessonName);
                }

                if ("FullScreen".Equals(param.ToString()) && App.SetupModel.IsEnableSlideShow)
                {
                    _timerViewFullScreen.Stop();
                    stopForListen.Start();
                }
            }
            catch (Exception ex)
            {
                if(log.IsDebugEnabled)
                    MessageBox.Show(ex.ToString());
            }


        }
        #endregion

        #region"  HiddenPopupCommand"
        private ICommand _hiddenPopupCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand HiddenPopupCommand
        {
            get
            {
                if (_hiddenPopupCommand == null)
                {
                    _hiddenPopupCommand = new RelayCommand(this.HiddenPopupExecute, this.CanHiddenPopupExecute);
                }
                return _hiddenPopupCommand;
            }
        }

        private bool CanHiddenPopupExecute(object param)
        {
            return true;
        }

        private void HiddenPopupExecute(object param)
        {
            log.Info("||{*} === Hidden Popup Command Executed === ");
            ViewCore.MyNotifyIcon.CloseBalloon();
            log.DebugFormat("|| == Popup Icon Status Is Close : {0}", ViewCore.MyNotifyIcon.IsClosed);
        }
        #endregion

        #region "  NextBackLessonCommand"

        /// <summary>
        /// Gets the NextBackLesson Command.
        /// <summary>
        private ICommand _nextBackLessonCommand;
        public ICommand NextBackLessonCommand
        {
            get
            {
                if (_nextBackLessonCommand == null)
                    _nextBackLessonCommand = new RelayCommand(this.OnNextBackLessonExecute, this.OnNextBackLessonCanExecute);
                return _nextBackLessonCommand;
            }
        }

        /// <summary>
        /// Method to check whether the NextBackLesson command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNextBackLessonCanExecute(object param)
        {

            if ("Next".Equals(param.ToString()))
            {
                return _currentItemIndex < LessonCollection.Count() - 1;
            }
            else
            {
                return _currentItemIndex != 0;
            }
        }

        /// <summary>
        /// Method to invoke when the NextBackLesson command is executed.
        /// </summary>
        /// 
        private void OnNextBackLessonExecute(object param)
        {
            log.Info("||{*} === Next Back Command Executed === ");
            if (App.SetupModel.IsEnableSlideShow)
                return;
            if ("Back".Equals(param.ToString()))
            {
                if (_currentItemIndex != 0)
                    _currentItemIndex--;
            }
            else
            {
                if (_currentItemIndex < LessonCollection.Count() - 1)
                    _currentItemIndex++;

            }
            SetLesson(false);

        }
        #endregion

        #region"  CancelCommand"
        private ICommand _cancelCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(this.CancelExecute, this.CanCancelExecute);
                }
                return _cancelCommand;
            }
        }

        private bool CanCancelExecute(object param)
        {
            return true;
        }

        private void CancelExecute(object param)
        {
            log.Info("||{*} === Cancel Command Executed === ");
            var result = MessageBox.Show("Do you want to exit study !", "Question !", MessageBoxButton.YesNo);
            if (result.Equals(MessageBoxResult.Yes))
            {
                IsPopupStarted = false;


                if ("FullScreen".Equals(param.ToString()))
                {
                    _timerViewFullScreen = null;
                    _learnView.Close();
                }
                else
                {
                    CloseTimerPopup();
                    _timerPopup = null;
                    _waitForClose = null;
                }
                ViewCore.MyNotifyIcon.Dispose();
                ViewCore.MyNotifyIcon = null;
                if (App.LessonMangeView != null)
                    App.LessonMangeView.Show();
            }
        }
        #endregion

        //Full Screen Region
        #region "  ClosingFormCommand"

        /// <summary>
        /// Gets the ClosingForm Command.
        /// <summary>
        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new DelegateCommand(this.OnCloseExecute, this.OnCloseCanExecute);
                return _closeCommand;
            }
        }
        /// <summary>
        /// Method to check whether the ClosingForm command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCloseCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ClosingForm command is executed.
        /// </summary>
        private void OnCloseExecute()
        {
            log.Info("||{*} === Close Command Executed === ");
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to exit fullscreen ? ", "Question.", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Storyboard sb = (Storyboard)_learnView.FindResource("sbUnLoadForm");
                sb.Completed += new EventHandler(sb_Completed);
                _learnView.BeginStoryboard(sb);
                if (_timerViewFullScreen != null && _timerViewFullScreen.IsEnabled)
                {
                    _timerViewFullScreen.Stop();
                    IsFullScreenStarted = false;
                }
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            _learnView.Close();
            _learnView = null;

            if (App.SetupModel.IsEnableSlideShow)
            {
                PlayPauseBallonPopup(false);
            }
            else
            {
                ShowPopupForm();
            }
        }
        #endregion

        #region "  Full Screen Command"
        /// <summary>
        /// Gets the FullScreen Command.
        /// <summary>
        private ICommand _fullScreenCommand;
        public ICommand FullScreenCommand
        {
            get
            {
                if (_fullScreenCommand == null)
                    _fullScreenCommand = new RelayCommand(this.OnFullScreenExecute, this.OnFullScreenCanExecute);
                return _fullScreenCommand;
            }
        }

        /// <summary>
        /// Method to check whether the FullScreen command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnFullScreenCanExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the FullScreen command is executed.
        /// </summary>
        private void OnFullScreenExecute(object param)
        {
            log.Info("|| {*} === Full Screen Call ===");
            //CloseTimerPopup();
            HiddenPopupExecute(null);
            CloseTimerPopup();
            //PlayPauseBallonPopup(true);
            if (_learnView == null)
                _learnView = new LearnView();
            _learnView.DataContext = this;
            if (App.SetupModel.IsEnableSlideShow)
                StartLessonFullScreen();
            else
                SetLesson();

            //Storyboard sb = (Storyboard)_learnView.FindResource("sbLoadForm");
            //_learnView.BeginStoryboard(sb);
            _learnView.Show();
        }
        #endregion

        #region "  MiniFullScreenCommand"

        /// <summary>
        /// Gets the MiniFullScreen Command.
        /// <summary>
        private ICommand _miniFullScreenCommand;
        public ICommand MiniFullScreenCommand
        {
            get
            {
                if (_miniFullScreenCommand == null)
                    _miniFullScreenCommand = new RelayCommand(this.OnMiniFullScreenExecute, this.OnMiniFullScreenCanExecute);
                return _miniFullScreenCommand;
            }
        }

        /// <summary>
        /// Method to check whether the MiniFullScreen command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMiniFullScreenCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the MiniFullScreen command is executed.
        /// </summary>
        bool StatusAfterHidden = false;
        private void OnMiniFullScreenExecute(object param)
        {
            StatusAfterHidden = _timerViewFullScreen != null? this._timerViewFullScreen.IsEnabled:false;
            log.Info("|| {*} === Mini Full Screen Execute Call ===");
            log.Debug("|| == Is Full Screen Started : "+ IsFullScreenStarted);
            if ("Minimized".Equals(param.ToString()) && _timerViewFullScreen != null && this._timerViewFullScreen.IsEnabled)
            {
                this._timerViewFullScreen.Stop();
                IsCurrentStarted = false;
                log.Debug("|| ==> FullScreen is Stoped!");
            }
            else if (this._timerViewFullScreen != null && !this._timerViewFullScreen.IsEnabled)
            {
                if (StatusAfterHidden)
                    this._timerViewFullScreen.Start();
                IsCurrentStarted = true;
                log.Debug("|| ==> FullScreen is Started!");
            }
            log.Debug("|| == Is Full Screen Started : " + IsFullScreenStarted);

        }
        #endregion

        #region"  KeyboardCommand"

        /// <summary>
        /// Gets the Keyboard Command.
        /// <summary>
        private ICommand _keyboardCommand;
        public ICommand KeyboardCommand
        {
            get
            {
                if (_keyboardCommand == null)
                    _keyboardCommand = new RelayCommand(this.OnKeyboardExecute, this.OnKeyboardCanExecute);
                return _keyboardCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Keyboard command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnKeyboardCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Keyboard command is executed.
        /// </summary>
        private void OnKeyboardExecute(object param)
        {

            log.Info("||{*} === Key Board Executed === ");
            switch (param.ToString())
            {
                case "L":
                    ListenExecute("FullScreen");
                    break;
                case "Left":
                    OnNextBackLessonExecute("Back");
                    break;
                case "Right":
                    OnNextBackLessonExecute("Next");
                    break;
                case "C":
                    ChangeSideExecute("FullScreen");
                    break;
            }
        }
        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// Initial data
        /// </summary>
        private void Initialize()
        {
            log.Info("|| {*} === Initialize MainViewModel ===");
            _listenWord = new MediaPlayer();

            test.Interval = new TimeSpan(0, 0, 1);
            test.Tick += new EventHandler(test_Tick);
        }

        public void GetLesson(List<LessonModel> listLesson)
        {
            //LessonDataAccess lessonDA = new LessonDataAccess();
            //var lesson = lessonDA.GetAllWithRelation();
            if (App.SetupModel.IsShuffle)
            {
                var lessonShuffle = ShuffleList.Randomize<LessonModel>(listLesson);
                LessonCollection = new ObservableCollection<LessonModel>(lessonShuffle);
            }
            else
            {
                LessonCollection = new ObservableCollection<LessonModel>(listLesson);
            }
        }

        /// <summary>
        /// Method to set lesson to show in popup or fullscreen
        /// </summary>
        private void SetLesson(bool isLoop = true)
        {
            var LimitCardNum = LessonCollection.Count;
            if (App.SetupModel.IsLimitCard)
            {
                if (App.SetupModel.LimitCardNum < LimitCardNum)
                    LimitCardNum = App.SetupModel.LimitCardNum;
            }

            if (isLoop)
            {
                if (_currentItemIndex < LimitCardNum - 1)
                    _currentItemIndex++;
                else
                    _currentItemIndex = 0;
            }

            SelectedLesson = LessonCollection[_currentItemIndex];
            SelectedLesson.IsBackSide = false;
            log.DebugFormat("|| == Current Item : {0}/{1}", _currentItemIndex, LimitCardNum);
        }

        //Timer Region

        ///<summary>
        ///Timer Tick
        ///
        ///  |-------------------Popup timer tick(_timerPopup)--------------------|
        ///  |------Show & Close Timer(_waitForClose)------|
        ///  
        ///  {__________________View time__________________}{_______Distance______}       
        ///  
        ///</summary>

        /// <summary>
        /// Start Timer & notify popup
        /// </summary>
        private void InitialTimer()
        {
            if (ViewCore.MyNotifyIcon == null || ViewCore.MyNotifyIcon.IsDisposed)
                ViewCore.MyNotifyIcon = new TaskbarIcon();

            if (_waitForClose == null)
                _waitForClose = new DispatcherTimer();
            _waitForClose.Interval = new TimeSpan(0, 0, 0, App.SetupModel.ViewTimeSecond);
            _waitForClose.Tick += new EventHandler(_waitForClose_Tick);

            if (_timerViewFullScreen == null)
                _timerViewFullScreen = new DispatcherTimer();
            _timerViewFullScreen.Interval = new TimeSpan(0, 0, App.SetupModel.ViewTimeSecond);
            _timerViewFullScreen.Tick += new EventHandler(_timerViewFullScreen_Tick);

            if (_timerPopup == null)
                _timerPopup = new DispatcherTimer();
            _timerPopup.Interval = App.SetupModel.TimeOut;
            _timerPopup.Tick += new EventHandler(_timer_Tick);

            //test.Start();
        }

        /// <summary>
        /// Startup timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Stopwatch testTimerPopup = new Stopwatch();
        private void _timer_Tick(object sender, EventArgs e)
        {
            log.DebugFormat("|| =============Summary=============");
            log.DebugFormat("|| == Timer tick : {0}", _timerPopup.IsEnabled);
            testTimerPopup.Start();
            _swCountTimerTick.Start();
            log.DebugFormat("|| == TimerPopup.Interval :{0}", _timerPopup.Interval);
            log.DebugFormat("|| == TimeOut :{0}", App.SetupModel.TimeOut.Seconds);
            if (!ViewCore.MyNotifyIcon.IsPopupOpen)
            {
                SetLesson();
                _balloon = new FancyBalloon();
                ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
                this.IsPopupStarted = true;
                //RaisePropertyChanged(() => SelectedLesson);
                var timerSpan = new TimeSpan(0, 0, 0, App.SetupModel.ViewTimeSecond);
                TimerForClosePopup(timerSpan);
                log.DebugFormat("|| =================================\n");
                log.DebugFormat("|| ==> Is Popup Showing");
            }
        }

        /// <summary>
        /// Initial For Wait to close
        /// </summary>
        /// <param name="timeSpan"></param>
        Stopwatch testTimeView = new Stopwatch();
        private void TimerForClosePopup(TimeSpan timeSpan)
        {
            testTimerPopup.Stop();
            testTimeView.Start();
            testTimerPopup.Reset();
            _waitForClose.Interval = timeSpan;
            _waitForClose.Start();
        }

        /// <summary>
        /// wait time for close popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _waitForClose_Tick(object sender, EventArgs e)
        {
            log.Info("|| {*} === Initial Timer For Close Popup === ");
            log.Info("|| ==> WaitBalloon() methods started ");
            countSecond = 0;
            WaitBalloon();
        }

        /// <summary>
        /// Method for close ballon
        /// </summary>
        private void WaitBalloon()
        {
            var action = new Action(() =>
            {
                ViewCore.MyNotifyIcon.CloseBalloon();
                testTimeView.Stop();
                log.DebugFormat("|| == View Timer :{0}", testTimeView.Elapsed.Seconds);
                testTimeView.Reset();
                log.DebugFormat("|| ==> Popup Closed.......");
                log.DebugFormat("|| ================================= \n");
                _waitForClose.Stop();
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Show lesson Full Screen
        /// </summary>
        private void StartLessonFullScreen()
        {
            _timerViewFullScreen.Interval = new TimeSpan(0, 0, App.SetupModel.ViewTimeSecond);
            IsFullScreenStarted = true;
            _timerViewFullScreen.Start();
        }

        /// <summary>
        /// Timer for show lesson of FullScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerViewFullScreen_Tick(object sender, EventArgs e)
        {
            SetLesson();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waitUserClick_Tick(object sender, EventArgs e)
        {
            //Not Uses IsFullScreenStop
            DispatcherTimer t = sender as DispatcherTimer;
            if (_timerViewFullScreen != null)
                _timerViewFullScreen.Start();
            t.Stop();
        }

        /// <summary>
        /// Play or Pause BallonPopup
        /// Set True if call to another form handle & return MainViewModel
        /// </summary>
        /// <param name="isOtherFormShow"></param>
        private void PlayPauseBallonPopup(bool isOtherFormShow)
        {
            if (_timerPopup != null)
                log.DebugFormat("|| Before Call PausePlay Of Lesson Management : {0}", _timerPopup.IsEnabled);

            //Timer (popup flashcard ) is Started=> stop it
            if (_timerPopup != null && _timerPopup.IsEnabled)
            {
                log.DebugFormat("|| === Timer is current of Popup \"Start\" ");
                CloseTimerPopup();
                countSecond = 0;
            }
            else
            {
                log.DebugFormat("|| === Timer is current of Popup \"Stop\" ");
                if (_timerPopup == null)
                {
                    log.DebugFormat("|| ===> Timer will be call \"InitialTimer \" methods ");
                    this.InitialTimer();
                }

                if (!isOtherFormShow)
                {
                    var timerSpan = new TimeSpan(0, 0, 0, App.SetupModel.ViewTimeSecond);
                    CloseTimerPopup();
                }
                log.DebugFormat("|| ==> Timer will be \"Started \" ");
                _timerPopup.Start();
            }
        }

        /// <summary>
        /// Method For Stop Popup Notify for another to show
        /// </summary>
        private void CloseTimerPopup()
        {
            var action = new Action(() =>
            {
                if (_waitForClose != null && _waitForClose.IsEnabled)
                    _waitForClose.Stop();
                if (_timerPopup != null && _timerPopup.IsEnabled)
                    _timerPopup.Stop();

                IsCurrentStarted = false;
                if (ViewCore.MyNotifyIcon.CustomBalloon != null)
                {
                    ViewCore.MyNotifyIcon.CloseBalloon();
                    IsPopupStarted = false;
                    log.DebugFormat("|| == Popup is current Stoped");
                    log.DebugFormat("|| ==> CloseTimerPopup");
                }
                if (_timerPopup != null)
                    log.DebugFormat("|| == Timer tick (_timerPopup) Status : {0}", _timerPopup.IsEnabled);
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Show popup without slide show
        /// </summary>
        private void ShowPopupForm()
        {
            _balloon = new FancyBalloon();
            IsCurrentStarted = true;
            ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
        }
        #endregion

        #region All For Test
        //Variable
        DispatcherTimer test = new DispatcherTimer();
        int countSecond = 0;

        //Event
        void test_Tick(object sender, EventArgs e)
        {
            countSecond++;
            if (_timerPopup != null)
            {
                Console.WriteLine("\n=====>>>>>>>[TEST TIMER] : {0} || Current second : {1} ", _timerPopup.IsEnabled, countSecond);
            }

        }

        #endregion
    }
}
