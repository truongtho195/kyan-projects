﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Waf.Applications;
using FlashCard.DataAccess;
using FlashCard.Model;
using System.Windows.Threading;
using System.Threading;
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
using MVVMHelper.Common;

namespace FlashCard.ViewModels
{
    public partial class MainViewModel : ViewModel<MainWindow>
    {
        #region Constructors
        public MainViewModel(MainWindow view)
            : base(view)
        {
            Initialize();

            StudyConfigView lessionView = new StudyConfigView();
            lessionView.GetViewModel<StudyConfigViewModel>().SetupModel = SetupModel;

            ViewCore.Hide();
            if (lessionView.ShowDialog() == true)
            {
                var viewModel = lessionView.GetViewModel<StudyConfigViewModel>();
                LessonCollection = viewModel.LessonCollection;
                SetupModel = viewModel.SetupModel;
            }

            if (SetupModel.IsEnableSlideShow)
                InitialTimer();
            else
            {
                SelectedLesson = LessonCollection.First();
                SelectedLesson.IsBackSide = false;
                ShowPopupForm();
            }
            
            
        }

        #endregion

        #region Variables
        DispatcherTimer _timerPopup;
        DispatcherTimer _timerViewFullScreen;
        DispatcherTimer _waitForClose;
        FancyBalloon _balloon;
        Stopwatch _swCountTimerTick = new Stopwatch();
        LearnView _learnView;
        int _currentItemIndex = 0;
        public int TimerCount { get; set; }
        public bool IsMouseEnter { get; set; }


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
        private SetupModel _setupModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public SetupModel SetupModel
        {
            get { return _setupModel; }
            set
            {
                if (_setupModel != value)
                {
                    _setupModel = value;
                    RaisePropertyChanged(() => SetupModel);
                }
            }
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
                    if (value)
                        IsCurrentStarted = true;

                    RaisePropertyChanged(() => IsPopupStarted);
                }
            }
        }
        #endregion

        #region "  IconStatus"
        private string _iconStatus = @"/Icons/Circle_Green.ico";
        /// <summary>
        /// Gets or sets the IconStatus.
        /// </summary>
        public string IconStatus
        {
            get
            {
                if (IsCurrentStarted)
                {
                    _iconStatus = "/Icons/Circle_Green.ico";
                }
                else
                {
                    _iconStatus = "/Icons/Circle_Red.ico";
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
            return true;
        }

        private void ChangeSideExecute(object param)
        {
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

                //BackSideDetail = CloneObject.Clone<FlowDocument>(SelectedLesson.BackSideModel.BackSideDetail);
                if (SetupModel.IsEnableSlideShow)
                {
                    DispatcherTimer stopChangeLesson = new DispatcherTimer();
                    stopChangeLesson.Interval = new TimeSpan(0, 0, 0, 10);
                    stopChangeLesson.Tick += new EventHandler(stopChangeLesson_Tick);
                    _timerViewFullScreen.Stop();
                    stopChangeLesson.Start();
                }
            }

        }

        void stopChangeLesson_Tick(object sender, EventArgs e)
        {
            DispatcherTimer t = sender as DispatcherTimer;
            _timerViewFullScreen.Start();
            t.Stop();
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
            if (!SetupModel.IsEnableSlideShow)
                return false;
            return true;
        }

        private void FancyBallonMouseLeaveExecute(object param)
        {
            if (!IsOtherFormShow && this.IsPopupStarted == true)
            {
                _swCountTimerTick.Stop();
                int time = 0;
                if (_swCountTimerTick.Elapsed.Seconds < SetupModel.ViewTimeSecond)
                    time = SetupModel.ViewTimeSecond - _swCountTimerTick.Elapsed.Seconds;
                else
                    time = 1;
                var timerSpan = new TimeSpan(0, 0, 0, time);
                TimerForClosePopup(timerSpan);
                _swCountTimerTick.Reset();
                //Create timer popup cause When Mouse Enter _timerPopup is Stoped
                if (_timerPopup != null)
                    _timerPopup.Start();
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
            if (!SetupModel.IsEnableSlideShow)
                return false;
            return true;
        }

        private void FancyBallonMouseEnterExecute(object param)
        {
            var action = new Action(() =>
            {
                _waitForClose.Stop();
                _timerPopup.Stop();
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);

        }
        #endregion

        #region "  Exit Command"
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

        #region "  Lesson Manager Command"
        /// <summary>
        /// Gets the LessonManager Command.
        /// <summary>
        private ICommand _lessonManagerCommand;
        public ICommand LessonManagerCommand
        {
            get
            {
                if (_lessonManagerCommand == null)
                    _lessonManagerCommand = new RelayCommand(this.OnLessonManagerExecute, this.OnLessonManagerCanExecute);
                return _lessonManagerCommand;
            }
        }

        /// <summary>
        /// Method to check whether the LessonManager command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLessonManagerCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LessonManager command is executed.
        /// </summary>
        private void OnLessonManagerExecute(object param)
        {
            LessonManageView lessonManager = new LessonManageView(true);
            PlayPauseBallonPopup(true);
            //CloseTimerPopup();
            if (lessonManager.ShowDialog() == true)
            {
                PlayPauseBallonPopup(false);
            }
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
            return true;
        }

        /// <summary>
        /// Method to invoke when the PlayPause command is executed.
        /// </summary>
        private void OnPlayPauseExecute(object param)
        {
            if (IsPopupStarted)
            {
                PlayPauseBallonPopup(false);
            }
            else
            {
                if (_timerViewFullScreen.IsEnabled)
                    _timerViewFullScreen.Stop();
                else
                    _timerViewFullScreen.Start();
            }
        }

        #endregion

        #region "  ChooseLessonCommand"
        /// <summary>
        /// Gets the ChooseLesson Command.
        /// <summary>
        private ICommand _ChooseLessonCommand;
        public ICommand ChooseLessonCommand
        {
            get
            {
                if (_ChooseLessonCommand == null)
                    _ChooseLessonCommand = new RelayCommand(this.OnChooseLessonExecute, this.OnChooseLessonCanExecute);
                return _ChooseLessonCommand;
            }
        }

        /// <summary>
        /// Method to check whether the ChooseLesson command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChooseLessonCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ChooseLesson command is executed.
        /// </summary>
        private void OnChooseLessonExecute(object param)
        {
            UserConfigStudies();
        }

        private void UserConfigStudies()
        {

            if (IsPopupStarted)
                PlayPauseBallonPopup(true);
            else
            {
                if (_timerViewFullScreen != null && _timerViewFullScreen.IsEnabled)
                {
                    _timerViewFullScreen.Stop();
                }
            }
            StudyConfigView lessionView = new StudyConfigView();
            lessionView.GetViewModel<StudyConfigViewModel>().SetupModel = SetupModel;

            if (lessionView.ShowDialog() == true)
            {
                var viewModel = lessionView.GetViewModel<StudyConfigViewModel>();
                LessonCollection = viewModel.LessonCollection;

                SetupModel = viewModel.SetupModel;
                //set time for ballon popup
                if (_timerPopup != null)
                    _timerPopup.Interval = SetupModel.TimeOut;
                if(_timerViewFullScreen!=null)
                    _timerViewFullScreen.Interval = new TimeSpan(0, 0, this.SetupModel.ViewTimeSecond);


            }
            //else
            //{
            //    PlayPauseBallonPopup(false);
            //}

            if (SetupModel.IsEnableSlideShow)
            {
                if (IsPopupStarted)
                {
                    PlayPauseBallonPopup(false);
                }
                else
                {
                    if (_timerViewFullScreen != null && !_timerViewFullScreen.IsEnabled)
                    {
                        _timerViewFullScreen.Start();
                    }
                }
            }
            else
            {
                if (IsPopupStarted)
                {
                    SelectedLesson = LessonCollection.First();
                    SelectedLesson.IsBackSide = false;
                    _balloon = new FancyBalloon();
                    _timerPopup.Stop();
                    _waitForClose.Stop();
                    ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
                }
            }
        }




       
        #endregion

        #region "  ShowPopupCommand"
        private ICommand _showPopupCommand;
        //Relay Command In viewModel
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
            if (!SetupModel.IsEnableSlideShow)
                return true;
            return false;
        }

        private void ShowPopupExecute(object param)
        {
            ShowPopupForm();
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
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to exit ? ", "Question.", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Storyboard sb = (Storyboard)_learnView.FindResource("sbUnLoadForm");
                sb.Completed += new EventHandler(sb_Completed);
                _learnView.BeginStoryboard(sb);
                if (SetupModel.IsEnableSlideShow)
                    _timerViewFullScreen.Stop();
                PlayPauseBallonPopup(false);
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            _learnView.Close();
            _learnView = null;
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
            return true;
        }

        /// <summary>
        /// Method to invoke when the FullScreen command is executed.
        /// </summary>
        private void OnFullScreenExecute(object param)
        {
            CloseTimerPopup();

            if (_learnView == null)
                _learnView = new LearnView();
            _learnView.DataContext = this;
            if (SetupModel.IsEnableSlideShow)
                StartLessonFullScreen();
            else
                SetLesson();
            //Storyboard sb = (Storyboard)_learnView.FindResource("sbLoadForm");
            //_learnView.BeginStoryboard(sb);
            _learnView.Show();
            PlayPauseBallonPopup(true);

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
            //Debug.WriteLine("this._timerViewFullScreen IsEnabled : {0} | param : {1}", this._timerViewFullScreen.IsEnabled,param.ToString());
            if ("Minimized".Equals(param.ToString()) && this._timerViewFullScreen.IsEnabled)
            {
                this._timerViewFullScreen.Stop();
                IsCurrentStarted = false;
                Debug.WriteLine("|| FullScreen is Stoped!");
            }
            else if (!this._timerViewFullScreen.IsEnabled)
            {

                this._timerViewFullScreen.Start();
                IsCurrentStarted = true;
                Debug.WriteLine("|| FullScreen is Started!");
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
            //Set For View

            List<UserModel> UserLessonCollection = new List<UserModel>();
            LessonDataAccess lessonDA = new LessonDataAccess();
            LessonCollection = new ObservableCollection<LessonModel>(lessonDA.GetAllWithRelation());
            SetupModel = new SetupModel();
        }

        /// <summary>
        /// Method to set lesson to show in popup or fullscreen
        /// </summary>
        private void SetLesson(bool isLoop = true)
        {
            if (isLoop)
            {
                if (_currentItemIndex < LessonCollection.Count - 1)
                    _currentItemIndex++;
                else
                    _currentItemIndex = 0;
            }

            SelectedLesson = LessonCollection[_currentItemIndex];
            SelectedLesson.IsBackSide = false;
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
            if (_timerPopup == null)
                _timerPopup = new DispatcherTimer();
            if (ViewCore.MyNotifyIcon == null || ViewCore.MyNotifyIcon.IsDisposed)
                ViewCore.MyNotifyIcon = new TaskbarIcon();
            _timerPopup.Interval = SetupModel.TimeOut;
            _timerPopup.Tick += new EventHandler(_timer_Tick);
            _timerPopup.Start();
        }

        /// <summary>
        /// Startup timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Stopwatch testTimerPopup = new Stopwatch();
        private void _timer_Tick(object sender, EventArgs e)
        {
            testTimerPopup.Start();
            _swCountTimerTick.Start();
            Debug.WriteLine(" timerPopup.Interval :{0}", _timerPopup.Interval);
            Debug.WriteLine(" TimeOut :{0}", SetupModel.TimeOut.Seconds);
            if (!ViewCore.MyNotifyIcon.IsPopupOpen)
            {
                SetLesson();
                _balloon = new FancyBalloon();
                ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
                this.IsPopupStarted = true;
                RaisePropertyChanged(() => SelectedLesson);
                var timerSpan = new TimeSpan(0, 0, 0, SetupModel.ViewTimeSecond);
                TimerForClosePopup(timerSpan);
                Console.WriteLine(".....Showing .....");
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
            Debug.WriteLine("|[Test] testTimerPopup :{0}", testTimerPopup.Elapsed.Seconds);
            testTimerPopup.Reset();
            _waitForClose = new DispatcherTimer();
            _waitForClose.Interval = timeSpan;
            _waitForClose.Tick += new EventHandler(_waitForClose_Tick);
            _waitForClose.Start();
        }

        /// <summary>
        /// wait time for close popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _waitForClose_Tick(object sender, EventArgs e)
        {
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
                this.IsPopupStarted = false;
                testTimeView.Stop();
                Console.WriteLine("|[Test] View Timer :{0}", testTimeView.Elapsed.Seconds);
                testTimeView.Reset();
                Console.WriteLine("Closed.......");
                _waitForClose.Stop();
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Show lesson Full Screen
        /// </summary>
        private void StartLessonFullScreen()
        {
            _timerViewFullScreen = new DispatcherTimer();
            _timerViewFullScreen.Interval = new TimeSpan(0, 0, this.SetupModel.ViewTimeSecond);
            _timerViewFullScreen.Tick += new EventHandler(_timerViewFullScreen_Tick);
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
        /// Play or Pause BallonPopup
        /// Set True if call to another form handle & return MainViewModel
        /// </summary>
        /// <param name="isOtherFormShow"></param>
        private void PlayPauseBallonPopup(bool isOtherFormShow)
        {
            //If app is started => stop ballon popup 
            if (this.IsPopupStarted)
            {
                CloseTimerPopup();
                this.IsPopupStarted = false;
            }
            else
            {
                //Popup is "Not started"
                //Method call from this viewmodel => create timer for Ballon Popup
                if (!isOtherFormShow)
                {
                    var timerSpan = new TimeSpan(0, 0, 0, SetupModel.ViewTimeSecond);
                    CloseTimerPopup();
                    _timerPopup.Start();
                    //TimerForClosePopup(timerSpan);
                }
                else //this case can Ballon popup is starting in proccess, => need to break timer call popup show to call another process
                {
                    if (_timerPopup.IsEnabled)
                        _timerPopup.Stop();
                    if (_waitForClose != null)
                        _waitForClose.Stop();
                    IsCurrentStarted = false;
                }
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
                    ViewCore.MyNotifyIcon.CloseBalloon();
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
    }
}
