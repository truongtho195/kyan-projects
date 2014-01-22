using System;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.Helper;
using CPC.POS.Model;

namespace CPC.POS.ViewModel
{
    class PopupWorkPermissionAdvanceSearchViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the AdvanceSearchPredicate
        /// </summary>
        public Expression<Func<base_Guest, bool>> AdvanceSearchPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

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

        private int _permissionType;
        /// <summary>
        /// Gets or sets the PermissionType.
        /// </summary>
        public int PermissionType
        {
            get { return _permissionType; }
            set
            {
                if (_permissionType != value)
                {
                    this.IsDirty = true;
                    _permissionType = value;
                    OnPropertyChanged(() => PermissionType);
                }
            }
        }

        private int _overtimeOptions;
        /// <summary>
        /// Gets or sets the OvertimeOptions.
        /// </summary>
        public int OvertimeOptions
        {
            get { return _overtimeOptions; }
            set
            {
                if (_overtimeOptions != value)
                {
                    this.IsDirty = true;
                    _overtimeOptions = value;
                    OnPropertyChanged(() => OvertimeOptions);
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

        private int _totalWorkPermission;
        /// <summary>
        /// Gets or sets the TotalWorkPermission.
        /// </summary>
        public int TotalWorkPermission
        {
            get { return _totalWorkPermission; }
            set
            {
                if (_totalWorkPermission != value)
                {
                    this.IsDirty = true;
                    _totalWorkPermission = value;
                    OnPropertyChanged(() => TotalWorkPermission);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupWorkPermissionAdvanceSearchViewModel()
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

            // Search by FromDate and ToDate
            if (FromDate.HasValue && ToDate.HasValue)
            {
                // Get work permissions between FromDate and ToDate
                predicate = predicate.And(x => x.tims_WorkPermission.
                    Any(y => (y.FromDate <= FromDate && FromDate < y.ToDate) || (y.FromDate <= ToDate && ToDate < y.ToDate)));
            }
            else if (FromDate.HasValue)
            {
                // Get work permissions greater than FromDate
                predicate = predicate.And(x => x.tims_WorkPermission.Any(y => y.FromDate <= FromDate && FromDate <= y.ToDate));
            }
            else if (ToDate.HasValue)
            {
                // Get work permissions less than ToDate
                predicate = predicate.And(x => x.tims_WorkPermission.Any(y => y.FromDate <= ToDate && ToDate <= y.ToDate));
            }

            // Search by PermissionType
            if (PermissionType > 0)
            {
                foreach (ComboItem comboItem in Common.WorkPermissionType)
                {
                    int valueCheck = comboItem.Value;
                    if ((PermissionType & valueCheck) == valueCheck)
                    {
                        predicate = predicate.And(x => x.tims_WorkPermission.
                                Any(y => (y.PermissionType & valueCheck) == valueCheck));
                    }
                }

                //// Search by PermissionType
                //predicate = predicate.And(x => x.tims_WorkPermission.Any(y => y.PermissionType.Equals(PermissionType)));
            }

            if (OvertimeOptions > 0)
            {
                // Search by OvertimeOptions
                predicate = predicate.And(x => x.tims_WorkPermission.Any(y => y.OvertimeOptions.Equals(OvertimeOptions)));
            }

            // Get work permissions that TotalWorkPermission contain keyword
            if (!string.IsNullOrWhiteSpace(CompareType))
            {
                switch (CompareType)
                {
                    case ">":
                        // Get work permissions that TotalWorkPermission greater than keyword
                        predicate = predicate.And(x => x.tims_WorkPermission.Count > TotalWorkPermission);
                        break;
                    case "<":
                        // Get work permissions that TotalWorkPermission less than keyword
                        predicate = predicate.And(x => x.tims_WorkPermission.Count < TotalWorkPermission);
                        break;
                    case "=":
                        // Get work permissions that TotalWorkPermission equal keyword
                        predicate = predicate.And(x => x.tims_WorkPermission.Count == TotalWorkPermission);
                        break;
                }
            }

            // Get employee mark
            string employeeMark = MarkType.Employee.ToDescription();

            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(employeeMark) && x.IsActived);

            return predicate;
        }

        #endregion
    }
}