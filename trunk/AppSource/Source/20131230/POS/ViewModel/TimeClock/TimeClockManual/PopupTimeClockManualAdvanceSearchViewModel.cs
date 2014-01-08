using System;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupTimeClockManualAdvanceSearchViewModel : ViewModelBase
    {
        #region Defines

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the AdvanceSearchPredicate
        /// </summary>
        public Expression<Func<base_Guest, bool>> AdvanceSearchPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

        private string _firstName;
        /// <summary>
        /// Gets or sets the FirstName.
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    this.IsDirty = true;
                    _firstName = value;
                    OnPropertyChanged(() => FirstName);
                }
            }
        }

        private string _lastName;
        /// <summary>
        /// Gets or sets the LastName.
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    this.IsDirty = true;
                    _lastName = value;
                    OnPropertyChanged(() => LastName);
                }
            }
        }

        private DateTime? _fromDate;
        /// <summary>
        /// Gets or sets the FromDate.
        /// </summary>
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_fromDate != value)
                {
                    this.IsDirty = true;
                    _fromDate = value;
                    OnPropertyChanged(() => FromDate);
                }
            }
        }

        private DateTime? _toDate;
        /// <summary>
        /// Gets or sets the ToDate.
        /// </summary>
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                if (_toDate != value)
                {
                    this.IsDirty = true;
                    _toDate = value;
                    OnPropertyChanged(() => ToDate);
                }
            }
        }

        private string _compareType;
        /// <summary>
        /// Gets or sets the CompareType.
        /// </summary>
        public string CompareType
        {
            get { return _compareType; }
            set
            {
                if (_compareType != value)
                {
                    this.IsDirty = true;
                    _compareType = value;
                    OnPropertyChanged(() => CompareType);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupTimeClockManualAdvanceSearchViewModel()
        {
            // Initial commands
            InitialCommand();
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return IsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            // Create advance search predicate
            AdvanceSearchPredicate = CreateAdvanceSearchPredicate();

            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Guest, bool>> CreateAdvanceSearchPredicate()
        {
            // Initial predicate
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                // Get all emlpoyees that FirstName contain keyword
                predicate = predicate.And(x => x.FirstName.ToLower().Contains(FirstName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                // Get all emlpoyees that LastName contain keyword
                predicate = predicate.And(x => x.LastName.ToLower().Contains(LastName.ToLower()));
            }

            // Search by FromDate and ToDate
            if (FromDate.HasValue && ToDate.HasValue)
            {
                ToDate = ToDate.Value.AddDays(1);

                // Get adjustment that ClockIn in between FromDate and ToDate
                predicate = predicate.And(x => x.tims_TimeLog.Any(y => FromDate <= y.ClockIn && y.ClockIn < ToDate));
            }
            else if (FromDate.HasValue)
            {
                // Get adjustment that ClockIn greater than StartDate
                predicate = predicate.And(x => x.tims_TimeLog.Any(y => FromDate <= y.ClockIn));
            }
            else if (ToDate.HasValue)
            {
                ToDate = ToDate.Value.AddDays(1);

                // Get adjustment that ClockIn less than EndDate
                predicate = predicate.And(x => x.tims_TimeLog.Any(y => y.ClockIn < ToDate));
            }

            // Get all emlpoyees that ActualHours contain keyword
            if (!string.IsNullOrWhiteSpace(CompareType))
            {
                switch (CompareType)
                {
                    case ">":
                        // Get all emlpoyees that ActualHours greater than keyword

                        break;
                    case "<":
                        // Get all emlpoyees that ActualHours less than keyword

                        break;
                    case "=":
                        // Get all emlpoyees that ActualHours equal keyword

                        break;
                }
            }

            string employeeMark = MarkType.Employee.ToDescription();

            // Default condition
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(employeeMark) && x.IsTrackingHour);

            return predicate;
        }

        #endregion
    }
}