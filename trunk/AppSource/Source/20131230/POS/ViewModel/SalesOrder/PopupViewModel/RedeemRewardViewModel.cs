using System.Linq;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    /// <summary>
    /// List Choice reward apply to sale order
    /// <remarks>If using reward sumable is not usefull</remarks>
    /// </summary>
    class RedeemRewardViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand RedeemCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();

        public enum ReeedemRewardType
        {
            Apply = 1,
            Redeemded = 2,
            Cancel = 3
        }
        #endregion

        #region Constructors
        public RedeemRewardViewModel(base_SaleOrderModel saleOrderModel)
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
            SaleOrderModel = saleOrderModel;

            decimal totalOfDiscount = saleOrderModel.SaleOrderDetailCollection.Sum(x => x.TotalDiscount);
            foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.Where(x => x.ActivedDate.Value.Date <= DateTime.Today.Date && (!x.ExpireDate.HasValue || (x.ExpireDate.HasValue && saleOrderModel.OrderDate.Value.Date <= x.ExpireDate.Value))))
            {
                //Clone Item
                base_GuestRewardModel guestReward;
                if (guestRewardModel.IsTemporary)
                    guestReward = guestRewardModel.Clone();//hold property of temporary item
                else
                    guestReward = new base_GuestRewardModel(guestRewardModel.base_GuestReward);

                if (guestReward != null)
                {
                    decimal subTotal = 0;
                    if (Define.CONFIGURATION.IsRewardOnTax)
                        subTotal = saleOrderModel.SubTotal - saleOrderModel.DiscountAmount + saleOrderModel.TaxAmount+saleOrderModel.Shipping;
                    else
                        subTotal = saleOrderModel.SubTotal - saleOrderModel.DiscountAmount + saleOrderModel.Shipping;

                    //if (guestReward.RewardSetupUnit.Equals((short)RewardType.Money))
                    //{
                    //    string expiredDate =guestReward.ExpireDate.HasValue? guestReward.ExpireDate.Value.Date.ToString(Define.DateFormat):"never";
                    //    guestReward.RewardName = string.Format("{0} off; expires {1}", string.Format(Define.ConverterCulture, Define.CurrencyFormat, guestReward.base_GuestReward.base_RewardManager.RewardAmount), expiredDate);

                    //    if (guestReward.RewardValueEarned > saleOrderModel.Total)
                    //    {
                    //        decimal requiredAmount = guestReward.RewardValueEarned - subTotal;
                    //        guestReward.TotalAfterReward = 0;
                    //        guestReward.RequireReward = string.Format("* Requires {0} in addditional purchases to apply now", string.Format(Define.ConverterCulture, Define.CurrencyFormat, requiredAmount));
                    //        guestReward.IsAccepted = false;
                    //    }
                    //    else
                    //    {
                    //        guestReward.TotalAfterReward = subTotal - guestReward.RewardValueEarned;
                    //        guestReward.IsAccepted = true;
                    //    }
                    //    //Format to Currency, not binding "TotalAfterReward" to view. Binding by TotalRewardDisplay because some case show string empty
                    //    guestReward.TotalRewardDisplay = guestReward.TotalAfterReward > 0 ? string.Format(Define.ConverterCulture, Define.CurrencyFormat, guestReward.TotalAfterReward) : string.Empty;

                    //}
                    //else
                    //{
                    //    string msgExpiredDate = guestReward.base_GuestReward.ExpireDate.HasValue ? guestReward.base_GuestReward.ExpireDate.Value.ToString(Define.DateFormat) : Language.GetMsg("SO_Message_RewardNeverExpired");
                    //    guestReward.RewardName = string.Format("{0}% off; expires {1}", string.Format(Define.ConverterCulture, "{0:N0}", guestReward.RewardValueEarned), msgExpiredDate);
                    //    guestReward.TotalAfterReward = saleOrderModel.Total - (subTotal * guestReward.RewardValueEarned / 100);

                    //}
                    ////Check Reward less than discount

                    //if (guestReward.TotalAfterReward < totalOfDiscount && !Define.CONFIGURATION.IsRewardLessThanDiscount)
                    //{
                    //    guestReward.IsAccepted = false;
                    //    guestReward.TotalRewardDisplay = string.Empty;
                    //    guestReward.RequireReward = "* Greater discount already applied. Remove other discount to apply this reward now.";
                    //}
                    //else
                    //{
                    //    guestReward.IsAccepted = true;
                    //    guestReward.TotalRewardDisplay = string.Format(Define.ConverterCulture, Define.CurrencyFormat, guestReward.TotalAfterReward);
                    //}
                }

                GuestRewardCollection.Add(guestReward);

            }
        }
        #endregion

        #region Properties


        #region SaleOrderModel

        private base_SaleOrderModel _saleOrderModel;
        /// <summary>
        /// Gets or sets the SaleOrderModel

        /// </summary>
        public base_SaleOrderModel SaleOrderModel
        {
            get { return _saleOrderModel; }
            set
            {
                if (_saleOrderModel != value)
                {
                    _saleOrderModel = value;
                    OnPropertyChanged(() => SaleOrderModel);
                }
            }
        }
        #endregion

        #region GuestRewardCollection
        private CollectionBase<base_GuestRewardModel> _guestRewardCollection = new CollectionBase<base_GuestRewardModel>();
        /// <summary>
        /// Gets or sets the GuestRewardCollection.
        /// </summary>
        public CollectionBase<base_GuestRewardModel> GuestRewardCollection
        {
            get { return _guestRewardCollection; }
            set
            {
                if (_guestRewardCollection != value)
                {
                    _guestRewardCollection = value;
                    OnPropertyChanged(() => GuestRewardCollection);
                }
            }
        }
        #endregion

        public ReeedemRewardType ViewActionType { get; set; }


        #region SelectedReward
        private base_GuestRewardModel _selectedReward;
        /// <summary>
        /// Gets or sets the SelectedReward.
        /// </summary>
        public base_GuestRewardModel SelectedReward
        {
            get { return _selectedReward; }
            set
            {
                if (_selectedReward != value)
                {
                    _selectedReward = value;
                    OnPropertyChanged(() => SelectedReward);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods

        #region ApplyCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnApplyCommandCanExecute()
        {
            if (SelectedReward == null)
                return false;
            return SelectedReward.TotalAfterReward > 0 && GuestRewardCollection != null && GuestRewardCollection.Any(x => x.IsChecked);
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute()
        {
            ViewActionType = ReeedemRewardType.Apply;
            foreach (base_GuestRewardModel guestRewardModel in GuestRewardCollection.Where(x => x.IsChecked))
            {
                base_GuestRewardModel guestRewardUpdated = SaleOrderModel.GuestModel.GuestRewardCollection.Single(x => x.Id == guestRewardModel.Id);
                if (guestRewardUpdated != null)
                    guestRewardUpdated.IsChecked = guestRewardModel.IsChecked;
            }
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region Redeem Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRedeemCommandCanExecute()
        {
            if (GuestRewardCollection != null && GuestRewardCollection.Any(x => !x.IsAccepted && x.base_GuestReward.base_RewardManager != null && x.base_GuestReward.base_RewardManager.RewardAmtType.Equals((int)RewardType.Pecent)))
                return false;
            return true;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnRedeemCommandExecute()
        {
            ViewActionType = ReeedemRewardType.Redeemded;
            foreach (base_GuestRewardModel guestRewardUpdated in SaleOrderModel.GuestModel.GuestRewardCollection.Where(x => x.IsChecked))
                guestRewardUpdated.IsChecked = false;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;

        }
        #endregion

        #region CancelCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            ViewActionType = ReeedemRewardType.Cancel;
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            ApplyCommand = new RelayCommand(OnApplyCommandExecute, OnApplyCommandCanExecute);
            RedeemCommand = new RelayCommand(OnRedeemCommandExecute, OnRedeemCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion
    }
}