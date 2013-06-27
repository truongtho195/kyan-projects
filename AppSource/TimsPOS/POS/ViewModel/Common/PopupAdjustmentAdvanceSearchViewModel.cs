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
        /// Gets or sets the StartDate
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Gets or sets the EndDate
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Gets or sets the CostAdjustmentPredicate
        /// </summary>
        public Expression<Func<base_CostAdjustment, bool>> CostAdjustmentPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the QuantityAdjustmentPredicate
        /// </summary>
        public Expression<Func<base_QuantityAdjustment, bool>> QuantityAdjustmentPredicate { get; private set; }

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
            return true;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            // Create cost adjustment predicate
            CostAdjustmentPredicate = CreateCostAdjustmentSearchPredicate();

            // Create quantity adjustment predicate
            QuantityAdjustmentPredicate = CreateQuantityAdjustmentSearchPredicate();

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

            // Search by FromDate and ToDate
            if (FromDate.HasValue && ToDate.HasValue)
            {
                // Get cost adjustment that LoggedTime in between FromDate and ToDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime && x.LoggedTime <= ToDate);
            }
            else if (FromDate.HasValue)
            {
                // Get cost adjustment that LoggedTime greater than StartDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime);
            }
            else if (ToDate.HasValue)
            {
                // Get cost adjustment that LoggedTime less than EndDate
                predicate = predicate.And(x => x.LoggedTime <= ToDate);
            }

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

            // Search by FromDate and ToDate
            if (FromDate.HasValue && ToDate.HasValue)
            {
                // Get cost adjustment that LoggedTime in between FromDate and ToDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime && x.LoggedTime <= ToDate);
            }
            else if (FromDate.HasValue)
            {
                // Get cost adjustment that LoggedTime greater than StartDate
                predicate = predicate.And(x => FromDate <= x.LoggedTime);
            }
            else if (ToDate.HasValue)
            {
                // Get cost adjustment that LoggedTime less than EndDate
                predicate = predicate.And(x => x.LoggedTime <= ToDate);
            }

            return predicate;
        }

        #endregion
    }
}
