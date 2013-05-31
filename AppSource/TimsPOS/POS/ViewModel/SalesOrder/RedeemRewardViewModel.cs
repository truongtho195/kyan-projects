using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.Database;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
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
            foreach (base_GuestRewardModel item in saleOrderModel.GuestModel.GuestRewardCollection)
            {
                
                base_GuestRewardModel guestReward = new base_GuestRewardModel(item.base_GuestReward);

                if (guestReward.base_GuestReward.base_RewardManager != null)
                {
                    //Get Expire Date
                    int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == guestReward.base_GuestReward.base_RewardManager.RewardExpiration).Detail);
                    DateTime expireDate = saleOrderModel.GuestModel.GuestRewardCollection.FirstOrDefault().EearnedDate.Value.AddDays(expireDay);

                    if (guestReward.base_GuestReward.base_RewardManager.RewardAmtType.Equals((int)RewardAmtType.Money))
                    {
                        guestReward.RewardName = string.Format("$ {0}; expires {1}", guestReward.base_GuestReward.base_RewardManager.RewardAmount, expireDate.Date.ToString(Define.DateFormat));


                        if (guestReward.base_GuestReward.base_RewardManager.RewardAmount > saleOrderModel.Total)
                        {
                            decimal requiredAmount = guestReward.base_GuestReward.base_RewardManager.RewardAmount - saleOrderModel.Total;
                            guestReward.TotalAfterReward = 0;
                            guestReward.RequireReward = string.Format("* Requires ${0} in addditional purchases to apply now", requiredAmount);
                            guestReward.IsAccepted = false;
                        }
                        else
                        {
                            guestReward.TotalAfterReward = saleOrderModel.Total - guestReward.base_GuestReward.base_RewardManager.RewardAmount;
                            guestReward.IsAccepted = true;
                        }

                    }
                    else
                    {

                        guestReward.RewardName = string.Format("{0}% ; expires {1}", guestReward.base_GuestReward.base_RewardManager.RewardAmount, expireDate.Date.ToString(Define.DateFormat));
                        decimal subTotal = 0;
                        if (Define.CONFIGURATION.IsRewardOnTax)
                            subTotal = saleOrderModel.SubTotal - saleOrderModel.DiscountAmount + saleOrderModel.TaxAmount;
                        else
                            subTotal = saleOrderModel.SubTotal - saleOrderModel.DiscountAmount;

                        guestReward.TotalAfterReward = saleOrderModel.Total - (subTotal * guestReward.base_GuestReward.base_RewardManager.RewardAmount / 100);
                    }
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
            return SelectedReward.TotalAfterReward > 0 && GuestRewardCollection != null && GuestRewardCollection.Any(x=>x.IsChecked);
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
