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


namespace FlashCard.ViewModels
{
    public partial class MainViewModel : ViewModel<MainWindow>
    {
        #region Constructors

        public MainViewModel(MainWindow view)
            : base(view)
        {
            Initialize();
            _timer = new DispatcherTimer();

            _timer.Interval = SetupModel.TimeOut;
            _timer.Tick += new EventHandler(_timer_Tick);
            _stopWatch.Start();
            _timer.Start();
            ViewCore.Hide();
        }

        Stopwatch _stopWatch = new Stopwatch();
        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!ViewCore.MyNotifyIcon.IsPopupOpen)
            {
                if (_count < LessonCollection.Count - 1)
                    _count++;
                else
                    _count = 0;

                SelectedLesson = LessonCollection[_count];
                SelectedLesson.IsBackSide = false;

                _balloon = new FancyBalloon();
                ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, null);
                RaisePropertyChanged(() => SelectedLesson);
                _waitForClose = new Timer(WaitBalloon);
                _waitForClose.Change((int)SetupModel.ViewTime.TotalMilliseconds, Timeout.Infinite);
                Console.WriteLine(".....Showing .....");
            }
        }

        #endregion

        #region Variables
        DispatcherTimer _timer;
        Timer _waitForClose;
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


        #endregion

        #region Commands
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
            //sbChangeToFront

            SelectedLesson.IsBackSide = !SelectedLesson.IsBackSide;
            Storyboard sb;
            if (SelectedLesson.IsBackSide)
                sb = (Storyboard)_balloon.FindResource("sbChangeToBack");
            else
                sb = (Storyboard)_balloon.FindResource("sbChangeToFront");

            var action = new Action(() =>
            {
                _balloon.BeginStoryboard(sb);
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
            
        }

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
            _waitForClose = new Timer(WaitBalloon);
            _waitForClose.Change((int)SetupModel.ViewTime.TotalMilliseconds, Timeout.Infinite);
        }

        private void WaitBalloon(object state)
        {
            var action = new Action(() =>
            {
                ViewCore.MyNotifyIcon.CloseBalloon();
                Console.WriteLine("Closed.......");
                _timer.Start();
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }


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
                _waitForClose.Dispose();
                _timer.Stop();
            });
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, action);
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            LessonDataAccess lessonDA = new LessonDataAccess();
            LessonCollection = new List<LessonModel>(lessonDA.GetAllWithRelation());
            SetupModel = new SetupModel();
            SetupModel.DistanceTime = new TimeSpan(0, 0, 3);
            SetupModel.ViewTime = new TimeSpan(0, 0, 7);

        }
        #endregion




    }
}
