using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FlashCard.Database;
using FlashCard.Helper;
using FlashCard.Views;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using MVVMHelper.Commands;
using FlashCard.Database.Repository;

namespace FlashCard.ViewModels
{
    public partial class MainViewModel : ViewModel<MainWindow>, IDisposable
    {
        #region Constructors
        public MainViewModel(MainWindow view)
            : base(view)
        {
            Initialize();

        }
        #endregion

        #region Variables
        private DispatcherTimer _timerPopup;
        private DispatcherTimer _timerViewFullScreen;
        private DispatcherTimer _waitForClose;

        private Stopwatch _swCountTimerTick = new Stopwatch();
        private LearnView _learnView;
        private int _currentItemIndex = 0;
        private MediaPlayer _listenWord = new MediaPlayer();
        private SoundPlayer _soundForShow = new SoundPlayer(FlashCard.Properties.Resources.Notification);
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Variable set user is current what side, avoid trigger & storyboard not sync together
        /// </summary>
        public bool IsCurrentBackSide { get; set; }

        public bool IsOtherEventControlRaise { get; set; }
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


        private StudyType _currentStudy;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public StudyType CurrentStudy
        {
            get { return _currentStudy; }
            set
            {
                if (_currentStudy != value)
                {
                    _currentStudy = value;
                    RaisePropertyChanged(() => CurrentStudy);
                }
            }
        }


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

        #region"  SelectedBackSide"
        private BackSide _selectedBackSide;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public BackSide SelectedBackSide
        {
            get { return _selectedBackSide; }
            set
            {
                if (_selectedBackSide != value)
                {
                    _selectedBackSide = value;
                    RaisePropertyChanged(() => SelectedBackSide);
                }
            }
        }
        #endregion

        #region "  SetupModel"
        //private SetupModel _setupModel;
        /// <summary>
        /// Gets the property value.
        /// </summary>

        public SetupModel SetupModel
        {
            get { return CacheObject.Get<SetupModel>("SetupModel"); }
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
                        IsFullScreenStarted = false;
                        IsCurrentStarted = true;
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
                    {
                        IsPopupStarted = false;
                        IsCurrentStarted = true;
                    }
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
        /// Flag for pause play & set Icon ballon in tasbar
        /// </summary>
        public bool IsCurrentStarted
        {
            get
            {
                //return (_timerPopup != null && _timerPopup.IsEnabled || _timerViewFullScreen != null && _timerViewFullScreen.IsEnabled);
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

        #region"  CanListen"
        private bool _canListen = true;
        /// <summary>
        /// Gets or sets the CanListen.
        /// </summary>
        public bool CanListen
        {
            get { return _canListen; }
            set
            {
                if (_canListen != value)
                {
                    _canListen = value;
                    RaisePropertyChanged(() => CanListen);
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
            try
            {
                log.Info("||{*} === Change Side Command Executed === ");
                SelectedLesson.IsBackSide = !SelectedLesson.IsBackSide;
                Storyboard sbChangeSide;
                if ("Popup".Equals(param.ToString()))
                {
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        var control = ViewCore.MyNotifyIcon.CustomBalloon.Child as FancyBalloon;

                        if (SelectedLesson.IsBackSide)
                            sbChangeSide = (Storyboard)control.FindResource("sbChangeToBack");
                        else
                            sbChangeSide = (Storyboard)control.FindResource("sbChangeToFront");

                        sbChangeSide.Begin();
                    }));

                }
                else
                {
                    //if (SelectedLesson.IsBackSide)
                    //{
                    //    sbChangeSide = (Storyboard)_learnView.FindResource("sbToBackSide");
                    //}
                    //else
                    //    sbChangeSide = (Storyboard)_learnView.FindResource("sbToFrontSide");
                    //_learnView.BeginStoryboard(sbChangeSide);

                    if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true && IsCurrentStarted)
                    {
                        DispatcherTimer stopChangeLesson = new DispatcherTimer();
                        stopChangeLesson.Interval = new TimeSpan(0, 0, 0, 2);
                        stopChangeLesson.Tick += new EventHandler(waitUserClick_Tick);
                        _timerViewFullScreen.Stop();
                        stopChangeLesson.Start();
                    }
                }
                IsCurrentBackSide = SelectedLesson.IsBackSide;
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == false)
                return false;
            return ViewCore.MyNotifyIcon.CustomBalloon != null;
        }

        private void FancyBallonMouseEnterExecute(object param)
        {
            try
            {
                var action = new Action(() =>
                {
                    log.Info("||{*} === Fancy Ballon Mouse Enter Command Executed === ");
                    //if (!(ViewCore.MyNotifyIcon.CustomBalloon.Child as FancyBalloon).isClosing)
                    if (!IsOtherEventControlRaise)
                    {
                        var storyBoard = (ViewCore.MyNotifyIcon.CustomBalloon.Child as FancyBalloon).FindResource("FadeBack") as Storyboard;
                        storyBoard.Begin();
                        _waitForClose.Stop();
                        _timerPopup.Stop();
                    }
                });
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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
            if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == false)
                return false;
            return ViewCore.MyNotifyIcon.CustomBalloon != null;
        }

        private void FancyBallonMouseLeaveExecute(object param)
        {
            try
            {
                log.Info("||{*} === Fancy Ballon Mouse Leave  Command Executed === ");
                //IsOtherEventControlRaise== true; Set for sensario if use MouseEnter to Click FullScreen Button => MouseLeave will execute so timer start
                // IsOtherEventControlRaise know user not click on button fullScreen
                if (!IsOtherEventControlRaise)
                {
                    var storyBoard = (ViewCore.MyNotifyIcon.CustomBalloon.Child as FancyBalloon).FindResource("FadeLeave") as Storyboard;
                    storyBoard.Begin();
                }
                _swCountTimerTick.Stop();
                int time = 0;
                if (_swCountTimerTick.Elapsed.Seconds < CacheObject.Get<SetupModel>("SetupModel").Setup.ViewTimeSecond)
                    time = CacheObject.Get<SetupModel>("SetupModel").Setup.ViewTimeSecond - _swCountTimerTick.Elapsed.Seconds + 1;
                else
                    time = 2;
                var timerSpan = new TimeSpan(0, 0, 0, time);
                TimerForClosePopup(timerSpan);
                _swCountTimerTick.Reset();
                //Create timer popup cause When Mouse Enter _timerPopup is Stoped
                if (_timerPopup != null && !_timerPopup.IsEnabled)
                    _timerPopup.Start();

                //IsOtherEventControlRaise = false;
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            var msg = MessageBox.Show(ViewCore as Window, "Do you want to exit !", "Exit Flash Card", MessageBoxButton.YesNo);
            if (msg.Equals(MessageBoxResult.Yes))
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
            try
            {
                log.Info("||{*} === Play Pause Command Executed === ");
                if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true)
                {
                    if ("FullScreen".Equals(param.ToString()))
                    {
                        if (_timerViewFullScreen.IsEnabled)
                        {
                            //IsFullScreenStarted = false;
                            IsCurrentStarted = false;
                            this.CurrentStudy = StudyType.None;
                            _timerViewFullScreen.Stop();

                        }
                        else
                        {
                            //IsFullScreenStarted = true;
                            this.CurrentStudy = StudyType.FullScreen;
                            _timerViewFullScreen.Start();

                        }
                    }
                    else
                        PlayPauseBallonPopup(false);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

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
            if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == false)
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
        private RelayCommand _listenCommand;
        //Gets or sets the property value
        public RelayCommand ListenCommand
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
            return CanListen;
        }

        private void ListenExecute(object param)
        {
            try
            {
                DispatcherTimer waitForListener = new DispatcherTimer();
                waitForListener = new DispatcherTimer();
                waitForListener.Interval = new TimeSpan(0, 0, 0, 1);
                waitForListener.Tick += new EventHandler(waitForListener_Tick);
                log.Info("||{*} === Listen Command Executed === ");
                CanListen = false;
                TextToSpeechPlayer(SelectedLesson.LessonName.Trim());
                waitForListener.Start();
                if ("FullScreen".Equals(param.ToString()) && CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true && IsCurrentStarted)
                {
                    DispatcherTimer stopForListen = new DispatcherTimer();
                    stopForListen = new DispatcherTimer();
                    stopForListen.Interval = new TimeSpan(0, 0, 0, 2);
                    stopForListen.Tick += new EventHandler(waitUserClick_Tick);
                    _timerViewFullScreen.Stop();
                    stopForListen.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void waitForListener_Tick(object sender, EventArgs e)
        {
            var dispartcherTimer = sender as DispatcherTimer;
            dispartcherTimer.Stop();
            CanListen = true;
            CommandManager.InvalidateRequerySuggested();

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
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    log.Info("||{*} === Hidden Popup Command Executed === ");
                    IsOtherEventControlRaise = true;
                    ViewCore.MyNotifyIcon.CloseBalloon();
                    if (_timerPopup != null && !_timerPopup.IsEnabled)
                        _timerPopup.Start();
                    log.DebugFormat("|| == Popup Icon Status Is Close : {0}", ViewCore.MyNotifyIcon.IsClosed);
                }));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }


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
            if (LessonCollection == null || SelectedLesson == null)
                return false;

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
            try
            {
                log.Info("||{*} === Next Back Command Executed === ");
                if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true)
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
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

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
            try
            {
                log.Info("||{*} === Cancel Command Executed === ");
                //Close & waiting for answer
                if (this.CurrentStudy == StudyType.Popup)
                {
                    CloseTimerPopup();
                }
                else if(this.CurrentStudy == StudyType.FullScreen)
                {
                    _timerViewFullScreen.Stop();
                }
                var result = MessageBox.Show(ViewCore as Window, "Do you want to exit study !", "Question !", MessageBoxButton.YesNo);

                if (result.Equals(MessageBoxResult.Yes))
                {
                    //For Fullscreen
                    if (_timerViewFullScreen != null)
                        _timerViewFullScreen.Stop();
                    if (_learnView != null)
                    {
                        _learnView.Close();
                    }

                    //IsPopupStarted = false;
                    this.CurrentStudy = StudyType.None;
                    ViewCore.MyNotifyIcon.Dispose();

                    //Update SetupModel IsOpenLastStudy
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(
                        delegate
                        {
                            if (CacheObject.Get<SetupModel>("SetupModel").IsOpenLastStudy == true)
                            {
                                var setupModel = CacheObject.Get<SetupModel>("SetupModel");
                                setupModel.IsOpenLastStudy = false;
                                setupModel.ToEntity();
                                SetupRepository setupRepository = new SetupRepository();
                                setupRepository.Update<Setup>(setupModel.Setup);
                                setupRepository.Commit();
                                CacheObject.Add<SetupModel>("SetupModel", setupModel);
                            }
                        }));
                    //End Update SetupModel IsOpenLastStudy

                    GC.SuppressFinalize(this);
                    if (App.LessonMangeView != null)
                    {
                        App.LessonMangeView.Show();
                        App.LessonMangeView.Activate();
                    }
                }
                else
                {
                    if ("FullScreen".Equals(param.ToString()))
                        _timerViewFullScreen.Start();
                    else
                        _timerPopup.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
        /// <para>
        /// Close full screen and run popup
        /// </para>
        /// </summary>
        private void OnCloseExecute()
        {
            try
            {
                log.Info("||{*} === Close Command Executed === ");
                MessageBoxResult messageBoxResult = MessageBox.Show(ViewCore as Window, "Do you want to exit fullscreen ? ", "Question.", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult.Equals(MessageBoxResult.Yes))
                {
                    //Storyboard sb = (Storyboard)_learnView.FindResource("sbUnLoadForm");
                    //sb.Completed += new EventHandler(sb_Completed);
                    //_learnView.BeginStoryboard(sb);
                    if (_timerViewFullScreen != null)
                    {
                        _timerViewFullScreen.Stop();
                        this.CurrentStudy = StudyType.None;
                        _learnView.Close();
                        _learnView = null;

                        if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true)
                        {
                            //PlayPauseBallonPopup(true);
                            //Run popup
                            this.CurrentStudy = StudyType.Popup;
                            _timerPopup.Start();
                        }
                        else
                        {

                            ShowPopupForm();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            return !(this.CurrentStudy == StudyType.FullScreen);
        }

        /// <summary>
        /// Method to invoke when the FullScreen command is executed.
        /// </summary>
        private void OnFullScreenExecute(object param)
        {
            try
            {
                log.Info("|| {*} === Full Screen Call ===");
                IsOtherEventControlRaise = true;
                CloseTimerPopup();
                //PlayPauseBallonPopup(true);
                if (_learnView == null)
                    _learnView = new LearnView();
                _learnView.DataContext = this;
                if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true)
                    StartLessonFullScreen();
                else
                    SetLesson();

                //Storyboard sb = (Storyboard)_learnView.FindResource("sbLoadForm");
                //_learnView.BeginStoryboard(sb);
                _learnView.Show();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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
        private void OnMiniFullScreenExecute(object param)
        {
            try
            {
                //StatusAfterHidden = _timerViewFullScreen != null ? this._timerViewFullScreen.IsEnabled : false;
                //log.Info("|| {*} === Mini Full Screen Execute Call ===");
                //log.Debug("|| == Is Full Screen Started : " + IsFullScreenStarted);
                ////Minimized
                //if ("Maximized".Equals(param.ToString()) && _timerViewFullScreen != null && this._timerViewFullScreen.IsEnabled)
                //{
                //    this._timerViewFullScreen.Stop();
                //    IsCurrentStarted = false;
                //    log.Debug("|| ==> FullScreen is Stoped!");
                //}
                //else if (this._timerViewFullScreen != null && !this._timerViewFullScreen.IsEnabled)
                //{
                //    if (StatusAfterHidden)
                //        this._timerViewFullScreen.Start();
                //    IsCurrentStarted = true;
                //    log.Debug("|| ==> FullScreen is Started!");
                //}
                log.Debug("|| == Is Full Screen Started : " + IsFullScreenStarted);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

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
        }

        /// <summary>
        /// Public Method Call from another form
        /// </summary>
        public void ExcuteMainForm()
        {
            try
            {
                TextToSpeechPlayer("Welcome to Flash Card");
                this.CurrentStudy = StudyType.Popup;
                //IsPopupStarted = true;
                IsCurrentStarted = true;
                if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSlideShow == true)
                {
                    InitialTimer();
                    _timerPopup.Start();
                }
                else
                {
                    SetLesson();
                    ShowPopupForm();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Method to set lesson to show in popup or fullscreen
        /// </summary>
        private void SetLesson(bool isLoop = true)
        {
            try
            {


                log.DebugFormat("|| == Current Item : {0}/{1}", _currentItemIndex + 1, LessonCollection.Count);
                SelectedLesson = LessonCollection[_currentItemIndex];
                SelectedLesson.IsBackSide = false;  //    IsCurrentBackSide;
                RaisePropertyChanged(() => SelectedLesson);
                SelectedBackSide = SelectedLesson.Lesson.BackSides.Where(x => x.IsMain == 1).SingleOrDefault();
                if (isLoop)
                {
                    //LimitCardNum - 1
                    if (_currentItemIndex < LessonCollection.Count - 1)
                        _currentItemIndex++;
                    else
                        _currentItemIndex = 0;
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        /// <summary>
        /// Public method call from another form
        /// </summary>
        /// <param name="listLesson"></param>

        /// <summary>
        /// Method for Speech text
        /// </summary>
        /// <param name="TextForSpeech"></param>
        private void TextToSpeechPlayer(string TextForSpeech)
        {
            try
            {
                string textFile = TextForSpeech;
                if (textFile.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1)
                    textFile = CleanFileName(TextForSpeech);
                var currentFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                string strFullPath = string.Format(@"{0}\FlashCardSound\{1}.mp3", currentFolder, textFile);
                if (System.IO.File.Exists(strFullPath))
                {
                    log.Info("|| Listen with file ");
                    _listenWord.Close();
                    var ur = new Uri(strFullPath, UriKind.RelativeOrAbsolute);
                    _listenWord.Open(ur);
                    _listenWord.Play();
                }
                else if (CheckConnectionInternet.IsConnectedToInternet())
                {
                    log.Info("|| Listen with google translate : " + TextForSpeech);
                    _listenWord.Close();
                    string keyword = string.Format("{0}{1}&tl=en", "http://translate.google.com/translate_tts?q=", TextForSpeech);
                    var ur = new Uri(keyword, UriKind.RelativeOrAbsolute);
                    _listenWord.Open(ur);
                    _listenWord.Play();
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                        delegate
                        {
                            GetSoundFromGoogleTranslate.GetSoundGoogle(textFile, currentFolder + "\\FlashCardSound");
                        }));
                }
                else
                {
                    log.InfoFormat("|| Listen with Microsoft text Speech : {0}", TextForSpeech);
                    SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                    synthesizer.Speak(TextForSpeech);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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

            if (_timerPopup == null)
                _timerPopup = new DispatcherTimer();
            _timerPopup.Interval = CacheObject.Get<SetupModel>("SetupModel").TimeOut;
            _timerPopup.Tick += new EventHandler(_timer_Tick);

            if (_waitForClose == null)
                _waitForClose = new DispatcherTimer();
            _waitForClose.Interval = new TimeSpan(0, 0, (int)CacheObject.Get<SetupModel>("SetupModel").Setup.ViewTimeSecond);
            _waitForClose.Tick += new EventHandler(_waitForClose_Tick);

            if (_timerViewFullScreen == null)
                _timerViewFullScreen = new DispatcherTimer();
            _timerViewFullScreen.Interval = new TimeSpan(0, 0, (int)CacheObject.Get<SetupModel>("SetupModel").Setup.DistanceTimeSecond);
            _timerViewFullScreen.Tick += new EventHandler(_timerViewFullScreen_Tick);
        }

        /// <summary>
        /// Startup timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Stopwatch testTimerPopup = new Stopwatch();
        private void _timer_Tick(object sender, EventArgs e)
        {
            try
            {
                log.DebugFormat("|| =============Summary=============");
                log.DebugFormat("|| == Timer tick : {0}", _timerPopup.IsEnabled);
                testTimerPopup.Start();
                _swCountTimerTick.Start();
                log.DebugFormat("|| == TimerPopup.Interval :{0}", _timerPopup.Interval);
                log.DebugFormat("|| == TimeOut :{0}", CacheObject.Get<SetupModel>("SetupModel").TimeOut.Seconds);
                if (!ViewCore.MyNotifyIcon.IsPopupOpen)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        SetLesson();
                        if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSoundForShow == true)
                            _soundForShow.PlaySync();
                        FancyBalloon balloon = new FancyBalloon();
                        IsOtherEventControlRaise = false;
                        ViewCore.MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Fade, null);
                        //this.IsPopupStarted = true;
                        this.CurrentStudy = StudyType.Popup;
                        var timerSpan = new TimeSpan(0, 0, 0, CacheObject.Get<SetupModel>("SetupModel").Setup.ViewTimeSecond);
                        TimerForClosePopup(timerSpan);
                        if (CacheObject.Get<SetupModel>("SetupModel").SpeechWhenShow == true)
                            TextToSpeechPlayer(SelectedLesson.LessonName);
                        log.DebugFormat("|| =================================\n");
                        log.DebugFormat("|| ==> Is Popup Showing");
                    }));
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            try
            {
                log.Info("|| {*} === Initial Timer For Close Popup === ");
                log.Info("|| ==> WaitBalloon() methods started ");
                var action = new Action(() =>
                {
                    countSecond = 0;
                    IsCurrentBackSide = false;
                    _waitForClose.Stop();
                    ViewCore.MyNotifyIcon.CloseBalloon();
                    testTimeView.Stop();
                    log.DebugFormat("|| == View Timer :{0}", testTimeView.Elapsed.Seconds);
                    testTimeView.Reset();
                    log.DebugFormat("|| ==> Popup Closed.......");
                    log.DebugFormat("|| ================================= \n");
                });
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Show lesson Full Screen
        /// </summary>
        private void StartLessonFullScreen()
        {
            _timerViewFullScreen.Interval = new TimeSpan(0, 0, CacheObject.Get<SetupModel>("SetupModel").Setup.DistanceTimeSecond);
            //IsFullScreenStarted = true;
            this.CurrentStudy = StudyType.FullScreen;
            _timerViewFullScreen.Start();
        }

        /// <summary>
        /// Timer for show lesson of FullScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerViewFullScreen_Tick(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        SetLesson();
                        if (CacheObject.Get<SetupModel>("SetupModel").Setup.IsEnableSoundForShow == true)
                            _soundForShow.PlaySync();

                        if (CacheObject.Get<SetupModel>("SetupModel").SpeechWhenShow == true)
                            TextToSpeechPlayer(SelectedLesson.LessonName);
                    }));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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

            if (IsCurrentStarted)
            {
                log.DebugFormat("|| === Timer is current of Popup \"Start\" ");
                CloseTimerPopup();
                countSecond = 0;
                IsCurrentStarted = false;
            }
            else
            {
                if (_timerPopup == null)
                {
                    log.DebugFormat("|| ===> Timer will be call \"InitialTimer \" methods ");
                    this.InitialTimer();
                }
                log.DebugFormat("|| ==> Timer will be \"Started \" ");
                this.CurrentStudy = StudyType.Popup;
                IsCurrentStarted = true;
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
                if (_timerPopup != null)
                    _timerPopup.Stop();
                if (_waitForClose != null)
                    _waitForClose.Stop();
                if (ViewCore.MyNotifyIcon.CustomBalloon != null)
                {
                    ViewCore.MyNotifyIcon.CloseBalloon();
                    //IsPopupStarted = false;
                    this.CurrentStudy = StudyType.None;
                    log.DebugFormat("|| == Popup is current Stoped");
                    log.DebugFormat("|| ==> CloseTimerPopup");
                }
                if (_timerPopup != null)
                    log.DebugFormat("|| == Timer tick (_timerPopup) Status : {0}", _timerPopup.IsEnabled);
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, action);
        }

        /// <summary>
        /// Show popup without slide show
        /// </summary>
        private void ShowPopupForm()
        {
            try
            {
                FancyBalloon balloon = new FancyBalloon();
                this.CurrentStudy = StudyType.Popup;
                IsCurrentStarted = true;
                ViewCore.MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Fade, null);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Clean Invalid Char in file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string CleanFileName(string fileName)
        {
            return System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty)).Trim();
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

        public void Dispose()
        {
            _soundForShow.Dispose();
            _timerPopup = null;
            _timerViewFullScreen = null;

        }
    }
}
