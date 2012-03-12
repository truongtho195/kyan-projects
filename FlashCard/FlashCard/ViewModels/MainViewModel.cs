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
            _timer.Interval = new TimeSpan(0, 0, 20);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
            ViewCore.Hide();
        }
        private void _timer_Tick(object sender, EventArgs e)
        {
            _balloon = new FancyBalloon();
            _timer.Stop();
            if (_count < LessonCollection.Count - 1)
                _count++;
            else
                _count = 0;
            SelectedLesson = LessonCollection[_count];
            SelectedLesson.IsBackSide = false;
            ViewCore.MyNotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Slide, null);
            _timer.Start();
            _balloon.MouseEnter += new System.Windows.Input.MouseEventHandler(_balloon_MouseEnter);
            _balloon.MouseLeave += new System.Windows.Input.MouseEventHandler(_balloon_MouseLeave);
        }

        void _balloon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_balloon.IsLoaded)
            {
                Thread.Sleep(4000);
                ViewCore.MyNotifyIcon.CloseBalloon();
                _timer.Start();
            }
        }

        void _balloon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _timer.Stop();
        }
        #endregion

        #region Variables
        LessonDataAccess _lessonDA;
        DispatcherTimer _timer;
        FancyBalloon _balloon;
        int _count = 0;
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
