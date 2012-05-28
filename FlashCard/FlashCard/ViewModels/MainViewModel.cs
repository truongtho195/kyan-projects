using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
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


namespace FlashCard.ViewModels
{
    public partial class MainViewModel : ViewModel<MainWindow>
    {
        #region Constructors
        public MainViewModel(MainWindow view)
            : base(view)
        {
            Initialize();
            InitialTimer();
            ViewCore.Hide();
        }
        #endregion

        #region Variables
        DispatcherTimer _timer;
        DispatcherTimer _timerViewFullScreen;
        DispatcherTimer _waitForClose;
        FancyBalloon _balloon;
        int _count = 0;
        public int TimerCount { get; set; }
        public bool IsMouseEnter { get; set; }
        #endregion

        #region Properties
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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private List<LessonModel> _lessonCollection;
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


        public bool IsLOtherFormShow { get; set; }

        public bool IsStarted { get; set; }

        #endregion

        #region Commands
        #region "Change Side Command"
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
            Storyboard sb;
            if (SelectedLesson.IsBackSide)
                sb = (Storyboard)_balloon.FindResource("sbChangeToBack");
            else
                sb = (Storyboard)_balloon.FindResource("sbChangeToFront");

            _balloon.BeginStoryboard(sb);
        }
        #endregion

        #region "Fancy Ballon Mouse Leave Command"
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
            return true;
        }

        private void FancyBallonMouseLeaveExecute(object param)
        {
            if (!IsLOtherFormShow && this.IsStarted == true)
            {
                var timerSpan = new TimeSpan(0, 0, 0, 0, ((int)SetupModel.ViewTime.TotalMilliseconds / 2));
                InitialWaitForClose(timerSpan);
            }
        }

        #endregion

        #region "Fancy Ballon Mouse Enter Command"
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
            return true;
        }

        private void FancyBallonMouseEnterExecute(object param)
        {
            var action = new Action(() =>
            {
                _waitForClose.Stop();
                _timer.Stop();
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }
        #endregion

        #region "Exit Command"
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

        #region "Lesson Manager Command"
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
            IsLOtherFormShow = true;
            LessonManageView lessonManager = new LessonManageView(true);
            StopPopupNotify();
            lessonManager.Show();
        }

        #endregion

        #region "Play Pause Command"
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


            if (this.IsStarted == null || this.IsStarted == true)
            {
                var action = new Action(() =>
                {
                    ViewCore.MyNotifyIcon.CloseBalloon();
                    if (_waitForClose != null)
                        _waitForClose.Stop();
                    _timer.Stop();
                });
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
                this.IsStarted = false;
            }
            else
            {
                if (!IsLOtherFormShow)
                {
                    var timerSpan = new TimeSpan(0, 0, 0, 0, ((int)SetupModel.ViewTime.TotalMilliseconds));
                    InitialWaitForClose(timerSpan);
                }
            }
        }
        #endregion

        #region "Full Screen Command"
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

        LearnView _learnView = new LearnView();
        /// <summary>
        /// Method to invoke when the FullScreen command is executed.
        /// </summary>
        private void OnFullScreenExecute(object param)
        {
            IsLOtherFormShow = true;
            _learnView = new LearnView();
            _learnView.DataContext = this;
            _timerViewFullScreen = new DispatcherTimer();
            _timerViewFullScreen.Interval = this.SetupModel.ViewTime;
            _timerViewFullScreen.Tick += new EventHandler(_timerViewFullScreen_Tick);
            _timerViewFullScreen.Start();
            Storyboard sb = (Storyboard)_learnView.FindResource("sbLoadForm");
            _learnView.BeginStoryboard(sb);
            _learnView.Show();
            OnPlayPauseExecute(null);

        }
        #endregion

        #region "ClosingFormCommand"

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
                IsLOtherFormShow = false;
                _timerViewFullScreen.Stop();
                OnPlayPauseExecute(null);
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            _learnView.Close();
        }


        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// Initial data
        /// </summary>
        private void Initialize()
        {
            List<UserModel> UserLessonCollection = new List<UserModel>();
            LessonDataAccess lessonDA = new LessonDataAccess();
            LessonCollection = new List<LessonModel>(lessonDA.GetAllWithRelation());
            SetupModel = new SetupModel();
            SetupModel.DistanceTime = new TimeSpan(0, 0, 3);
            SetupModel.ViewTime = new TimeSpan(0, 0, 7);
        }

        /// <summary>
        /// Start Timer & notify popup
        /// </summary>
        private void InitialTimer()
        {
            _timer = new DispatcherTimer();
            if (ViewCore.MyNotifyIcon == null || ViewCore.MyNotifyIcon.IsDisposed)
                ViewCore.MyNotifyIcon = new TaskbarIcon();
            _timer.Interval = SetupModel.TimeOut;
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        /// <summary>
        /// Startup timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!ViewCore.MyNotifyIcon.IsPopupOpen)
            {
                SetLesson();

                _balloon = new FancyBalloon();
                ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
                this.IsStarted = true;
                RaisePropertyChanged(() => SelectedLesson);

                var timerSpan = new TimeSpan(0, 0, 0, 0, ((int)SetupModel.ViewTime.TotalMilliseconds));
                InitialWaitForClose(timerSpan);
                Console.WriteLine(".....Showing .....");
            }
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
        /// Timer for show lesson of FullScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerViewFullScreen_Tick(object sender, EventArgs e)
        {
            SetLesson();
        }

        /// <summary>
        /// Method for close ballon
        /// </summary>
        private void WaitBalloon()
        {
            var action = new Action(() =>
            {
                ViewCore.MyNotifyIcon.CloseBalloon();
                Console.WriteLine("Closed.......");
                _timer.Start();
                _waitForClose.Stop();

            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Method to set lesson to show in popup or fullscreen
        /// </summary>
        private void SetLesson()
        {
            if (_count < LessonCollection.Count - 1)
                _count++;
            else
                _count = 0;

            SelectedLesson = LessonCollection[_count];
            SelectedLesson.IsBackSide = false;
        }


        /// <summary>
        /// Initial For Wait to close
        /// </summary>
        /// <param name="timeSpan"></param>
        private void InitialWaitForClose(TimeSpan timeSpan)
        {
            _waitForClose = new DispatcherTimer();
            _waitForClose.Interval = timeSpan;
            _waitForClose.Tick += new EventHandler(_waitForClose_Tick);
            _waitForClose.Start();
        }

        /// <summary>
        /// Method For Stop Popup Notify for another to show
        /// </summary>
        private void StopPopupNotify()
        {

            if (_waitForClose != null)
                _waitForClose.Stop();
            ViewCore.MyNotifyIcon.CloseBalloon();
            ViewCore.MyNotifyIcon.Dispose();
            _timer.Stop();
        }
        #endregion






    }
}
