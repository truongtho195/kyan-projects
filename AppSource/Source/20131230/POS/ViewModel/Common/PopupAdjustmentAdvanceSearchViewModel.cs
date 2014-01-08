using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupAdjustmentAdvanceSearchViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the CostAdjustmentPredicate
        /// </summary>
        public Expression<Func<base_CostAdjustment, bool>> CostAdjustmentPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the QuantityAdjustmentPredicate
        /// </summary>
        public Expression<Func<base_QuantityAdjustment, bool>> QuantityAdjustmentPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

        private string _code;
        /// <summary>
        /// Gets or sets the Code.
        /// </summary>
        public string Code
        {
            get { return _code; }
            set
            {
                if (_code != value)
                {
                    this.IsDirty = true;
                    _code = value;
                    OnPropertyChanged(() => Code);
                }
            }
        }

        private string _name;
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    this.IsDirty = true;
                    _name = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        private string _attribute;
        /// <summary>
        /// Gets or sets the Attribute.
        /// </summary>
        public string Attribute
        {
            get { return _attribute; }
            set
            {
                if (_attribute != value)
                {
                    this.IsDirty = true;
                    _attribute = value;
                    OnPropertyChanged(() => Attribute);
                }
            }
        }

        private string _size;
        /// <summary>
        /// Gets or sets the Size.
        /// </summary>
        public string Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    this.IsDirty = true;
                    _size = value;
                    OnPropertyChanged(() => Size);
                }
            }
        }

        private short _status;
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public short Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    this.IsDirty = true;
                    _status = value;
                    OnPropertyChanged(() => Status);
                }
            }
        }

        private short _reason;
        /// <summary>
        /// Gets or sets the Reason.
        /// </summary>
        public short Reason
        {
            get { return _reason; }
            set
            {
                if (_reason != value)
                {
                    this.IsDirty = true;
                    _reason = value;
                    OnPropertyChanged(() => Reason);
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

        /// <summary>
        /// Gets or sets the IsQuantityAdjustment.
        /// </summary>
        public bool IsQuantityAdjustment { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupAdjustmentAdvanceSearchViewModel()
        {
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
            if (!IsQuantityAdjustment)
            {
                // Create cost adjustment predicate
                CostAdjustmentPredicate = CreateCostAdjustmentSearchPredicate();
            }
            else
            {
                // Create quantity adjustment predicate
                QuantityAdjustmentPredicate = CreateQuantityAdjustmentSearchPredicate();
            }

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
        private Expression<Func<base_CostAdjustment, bool>> CreateCostAdjustmentSearchPredicate()
        {
            // Create predicate
            Expression<Func<base_CostAdjustment, bool>> predicate = PredicateBuilder.True<base_CostAdjustment>();

            if (!string.IsNullOrWhiteSpace(Code))
            {
                // Get all adjustments that Code contain keyword
                predicate = predicate.And(x => x.base_Product.Code.ToLower().Contains(Code.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                // Get all adjustments that Name contain keyword
                predicate = predicate.And(x => x.base_Product.ProductName.ToLower().Contains(Name.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Attribute))
            {
                // Get all adjustments that Attribute contain keyword
                predicate = predicate.And(x => x.base_Product.Attribute.ToLower().Contains(Attribute.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Size))
            {
                // Get all adjustments that Size contain keyword
                predicate = predicate.And(x => x.base_Product.Size.ToLower().Contains(Size.ToLower()));
            }
            if (Status > 0)
            {
                // Get all adjustments that Status equal keyword
                predicate = predicate.And(x => x.Status.Equals(Status));
            }
            if (Reason > 0)
            {
                // Get all adjustments that Reason equal keyword
                predicate = predicate.And(x => x.Reason.Equals(Reason));
            }

            // Search by FromDate and ToDate
            if (FromDate.HasValue && ToDate.HasValue)
            {
                ToDate = ToDate.Value.AddDays(1);

                // Get adjustment that LoggedTime in between FromDate and ToDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime && x.LoggedTime < ToDate);
            }
            else if (FromDate.HasValue)
            {
                // Get adjustment that LoggedTime greater than StartDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime);
            }
            else if (ToDate.HasValue)
            {
                ToDate = ToDate.Value.AddDays(1);

                // Get adjustment that LoggedTime less than EndDate
                predicate = predicate.And(x => x.LoggedTime < ToDate);
            }

            // Default condition
            predicate = predicate.And(x => x.base_Product.IsPurge == false);

            return predicate;
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_QuantityAdjustment, bool>> CreateQuantityAdjustmentSearchPredicate()
        {
            // Create predicate
            Expression<Func<base_QuantityAdjustment, bool>> predicate = PredicateBuilder.True<base_QuantityAdjustment>();

            if (!string.IsNullOrWhiteSpace(Code))
            {
                // Get all adjustments that Code contain keyword
                predicate = predicate.And(x => x.base_Product.Code.ToLower().Contains(Code.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                // Get all adjustments that Name contain keyword
                predicate = predicate.And(x => x.base_Product.ProductName.ToLower().Contains(Name.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Attribute))
            {
                // Get all adjustments that Attribute contain keyword
                predicate = predicate.And(x => x.base_Product.Attribute.ToLower().Contains(Attribute.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Size))
            {
                // Get all adjustments that Size contain keyword
                predicate = predicate.And(x => x.base_Product.Size.ToLower().Contains(Size.ToLower()));
            }
            if (Status > 0)
            {
                // Get all adjustments that Status equal keyword
                predicate = predicate.And(x => x.Status.Equals(Status));
            }
            if (Reason > 0)
            {
                // Get all adjustments that Reason equal keyword
                predicate = predicate.And(x => x.Reason.Equals(Reason));
            }

            // Search by FromDate and ToDate
            if (FromDate.HasValue && ToDate.HasValue)
            {
                ToDate = ToDate.Value.AddDays(1);

                // Get adjustment that LoggedTime in between FromDate and ToDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime && x.LoggedTime < ToDate);
            }
            else if (FromDate.HasValue)
            {
                // Get adjustment that LoggedTime greater than StartDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime);
            }
            else if (ToDate.HasValue)
            {
                ToDate = ToDate.Value.AddDays(1);

                // Get adjustment that LoggedTime less than EndDate
                predicate = predicate.And(x => x.LoggedTime < ToDate);
            }

            // Default condition
            predicate = predicate.And(x => x.base_Product.IsPurge == false);

            return predicate;
        }

        #endregion
    }
}