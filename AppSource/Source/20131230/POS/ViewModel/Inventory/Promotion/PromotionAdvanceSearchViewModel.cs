using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PromotionAdvanceSearchViewModel : ViewModelBase
    {
        #region Defines

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the PromotionTypeID
        /// </summary>
        public short PromotionTypeID { get; set; }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Gets or sets the StartDate
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the EndDate
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the AdvanceSearchPredicate
        /// </summary>
        public Expression<Func<base_Promotion, bool>> AdvanceSearchPredicate { get; set; }

        public ObservableCollection<CheckBoxItemModel> PriceSchemaCollection { get; set; }

        private ObservableCollection<ComboItem> _promotionTypes;
        /// <summary>
        /// Gets or sets the PromotionTypes.
        /// </summary>
        public ObservableCollection<ComboItem> PromotionTypes
        {
            get { return _promotionTypes; }
            set
            {
                if (_promotionTypes != value)
                {
                    _promotionTypes = value;
                    OnPropertyChanged(() => PromotionTypes);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PromotionAdvanceSearchViewModel(ObservableCollection<ComboItem> promotionTypes)
        {
            // Set default value
            PromotionTypeID = 0;
            Status = 0;
            PriceSchemaCollection = new ObservableCollection<CheckBoxItemModel>(Common.PriceSchemas.Select(x => new CheckBoxItemModel(x)));
            PromotionTypes = promotionTypes;
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
            AdvanceSearchPredicate = CreatePredicateWithConditionSearch();
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
        private Expression<Func<base_Promotion, bool>> CreatePredicateWithConditionSearch()
        {
            Expression<Func<base_Promotion, bool>> predicate = PredicateBuilder.True<base_Promotion>();

            // Search by PromotionTypeID
            if (PromotionTypeID > 0)
            {
                predicate = predicate.And(x => x.PromotionTypeId == PromotionTypeID);
            }

            // Search by StartDate and EndDate
            if (StartDate.HasValue && EndDate.HasValue)
            {
                // Get promotions that Date in between StartDate and EndDate
                predicate = predicate.And(x => x.base_PromotionSchedule.
                Count(y => (y.StartDate.HasValue && y.StartDate.Value >= StartDate.Value) &&
                    (y.EndDate.HasValue && y.EndDate.Value <= EndDate.Value)) > 0);
            }
            else if (StartDate.HasValue)
            {
                // Get promotions that Schedule's StartDate greater than StartDate
                predicate = predicate.And(x => x.base_PromotionSchedule.
                Count(y => y.StartDate.HasValue && y.StartDate.Value >= StartDate.Value) > 0);
            }
            else if (EndDate.HasValue)
            {
                // Get promotions that Schedule's EndDate less than EndDate
                predicate = predicate.And(x => x.base_PromotionSchedule.
                Count(y => y.EndDate.HasValue && y.EndDate.Value <= EndDate.Value) > 0);
            }

            // Search by Status
            if (Status > 0)
                predicate = predicate.And(x => x.Status == Status);

            // Search by PriceSchemaRange
            foreach (CheckBoxItemModel checkBoxItemModel in PriceSchemaCollection.Where(x => x.IsChecked))
            {
                int valueCheck = checkBoxItemModel.Value;
                predicate = predicate.And(x => (x.PriceSchemaRange.Value & valueCheck) == valueCheck);
            }

            return predicate;
        }

        #endregion
    }
}