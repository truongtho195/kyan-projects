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
            _timer.Interval = _timerOut;
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
            ViewCore.Hide();
        }
        private void _timer_Tick(object sender, EventArgs e)
        {
            _balloon = new FancyBalloon();
            if (_count < LessonCollection.Count - 1)
                _count++;
            else
                _count = 0;
            SelectedLesson = LessonCollection[_count];
            SelectedLesson.IsBackSide = false;
            if (!_balloon.IsLoaded)
            {
                ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade,(int) _timerOut.TotalMilliseconds);
            }
            //CloseBalloon();
        }

        #endregion

        #region Variables
        LessonDataAccess _lessonDA;
        DispatcherTimer _timer;
        FancyBalloon _balloon;
        TimeSpan _timerOut = new TimeSpan(0, 0, 10);
        int _count = 0;
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
            SelectedLesson.IsBackSide = !SelectedLesson.IsBackSide;
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
            CloseBalloon();
            _timer.Start();
        }

        private void CloseBalloon()
        {
            if (_balloon.IsLoaded)
            {
                var timeSleep = _timerOut.TotalMilliseconds / 2;
                Thread.Sleep((int)timeSleep);
                ViewCore.MyNotifyIcon.CloseBalloon();
            }

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
            _timer.Stop();
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            _lessonDA = new LessonDataAccess();
            LessonCollection = new List<LessonModel>(_lessonDA.GetAllWithRelation());
        }
        #endregion




    }
}
