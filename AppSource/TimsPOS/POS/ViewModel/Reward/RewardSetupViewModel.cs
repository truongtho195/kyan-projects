using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.ComponentModel;
using System.Collections.ObjectModel;
using CPC.POS.Database;
using CPC.POS.Repository;
using System.Diagnostics;
using SecurityLib;
using System.Windows;
using System.Linq.Expressions;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    class RewardSetupViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand MemberListCommand { get; private set; }
        public RelayCommand RedemptionHistoryCommand { get; private set; }
        public RelayCommand TurnTrackingCommand { get; private set; }
        protected bool IsChangePurCharseThreshold = false;
        protected decimal NumbersOfRewardRelation = 0;
        //Repository
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_GuestRewardRepository _guestRewardRepository = new base_GuestRewardRepository();
        #endregion

        #region Constructors
        public RewardSetupViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            this.InitialCommand();
        }
        #endregion

        #region Properties

        #region RewardManagerModel
        /// <summary>
        /// Gets or sets the RewardManagerModel.
        /// </summary>
        private base_RewardManagerModel _rewardManagerModel;
        public base_RewardManagerModel RewardManagerModel
        {
            get
            {
                return _rewardManagerModel;
            }
            set
            {
                if (_rewardManagerModel != value)
                {
                    this._rewardManagerModel = value;
                    this.OnPropertyChanged(() => RewardManagerModel);
                }
            }
        }
        #endregion

        #region EnableFilteringData
        /// <summary>
        /// To get , set value when enable colunm.
        /// </summary>
        private bool _enableFilteringData = true;
        public bool EnableFilteringData
        {
            get { return _enableFilteringData; }
            set
            {
                if (_enableFilteringData != value)
                {
                    _enableFilteringData = value;
                    OnPropertyChanged(() => EnableFilteringData);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return this.IsValid && (this.RewardManagerModel != null && this.RewardManagerModel.IsDirty);
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            // TODO: Handle command logic here
            if (this.RewardManagerModel.IsNew)
                this.Insert();
            else
                this.Update();
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            return false;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            // TODO: Handle command logic here

        }
        #endregion

        #region MemberListCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMemberListCommandCanExecute()
        {
            return false;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnMemberListCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region RedemptionHistoryCommand
        /// <summary>
        /// Method to check whether the RedemptionHistoryCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRedemptionHistoryCommandCanExecute()
        {
            return false;
        }

        /// <summary>
        /// Method to invoke when the RedemptionHistoryCommand command is executed.
        /// </summary>
        private void OnRedemptionHistoryCommandExecute()
        {
            // TODO: Handle command logic here

        }
        #endregion

        #region TurnTrackingCommand
        /// <summary>
        /// Method to check whether the TurnTrackingCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnTurnTrackingCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the TurnTrackingCommand command is executed.
        /// </summary>
        private void OnTurnTrackingCommandExecute()
        {
            // TODO: Handle command logic here
            if (this.RewardManagerModel != null)
                this.RewardManagerModel.IsActived = this.RewardManagerModel.IsActived ? false : true;
        }
        #endregion

        #region PurchaseThresholdChanged Command
        /// <summary>
        /// Gets the QtyChanged Command.
        /// <summary>

        public RelayCommand<object> PurchaseThresholdChanged { get; private set; }

        /// <summary>
        /// Method to check whether the QtyChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPurchaseThresholdChangedCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the QtyChanged command is executed.
        /// </summary>
        private void OnPurchaseThresholdChangedExecute(object param)
        {
            if (param != null && this.RewardManagerModel != null)
            {

            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void InitialData()
        {
            //To load data from base_RewardManager.
        }

        #region InitialCommand
        /// <summary>
        /// To initialize commands. 
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            this.NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            this.SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            this.MemberListCommand = new RelayCommand(this.OnMemberListCommandExecute, this.OnMemberListCommandCanExecute);
            this.RedemptionHistoryCommand = new RelayCommand(this.OnRedemptionHistoryCommandExecute, this.OnRedemptionHistoryCommandCanExecute);
            this.TurnTrackingCommand = new RelayCommand(this.OnTurnTrackingCommandExecute, this.OnTurnTrackingCommandCanExecute);
            this.PurchaseThresholdChanged = new RelayCommand<object>(OnPurchaseThresholdChangedExecute, OnPurchaseThresholdChangedCanExecute);
        }
        #endregion

        #region LoadRewardManager
        /// <summary>
        /// To load data of base_RewardManager table.
        /// </summary>
        private void LoadRewardManager()
        {
            this.RewardManagerModel = null;
            base_RewardManager rewardManager = _rewardManagerRepository.GetIEnumerable().SingleOrDefault();
            if (rewardManager != null)
                this.RewardManagerModel = new base_RewardManagerModel(rewardManager);
            else
            {
                this.RewardManagerModel = new base_RewardManagerModel();
                this.RewardManagerModel.TotalRewardRedeemed = 10;
                this.RewardManagerModel.RewardAmtType = 1;
                this.RewardManagerModel.IsDirty = false;
            }
            this.RewardManagerModel.PropertyChanged += new PropertyChangedEventHandler(RewardManagerModel_PropertyChanged);
        }

        private void RewardManagerModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //To set value when IsTrackingPeriod is true.
                case "IsTrackingPeriod":
                    if (!this.RewardManagerModel.IsTrackingPeriod)
                    {
                        this.RewardManagerModel.StartDate = null;
                        this.RewardManagerModel.EndDate = null;
                        this.RewardManagerModel.IsNoEndDay = false;
                    }
                    break;

                //To set value when IsNoEndDay is true.
                case "IsNoEndDay":
                    if (this.RewardManagerModel.IsNoEndDay && this.RewardManagerModel.IsTrackingPeriod)
                        this.RewardManagerModel.EndDate = null;
                    break;

                //To set value when IsRedemptionLimit is true.
                case "IsRedemptionLimit":
                    if (!this.RewardManagerModel.IsRedemptionLimit)
                        this.RewardManagerModel.RedemptionLimitAmount = 0;
                    break;

                //To set value when IsBlockRedemption is true.
                case "IsBlockRedemption":
                    if (!this.RewardManagerModel.IsBlockRedemption)
                        this.RewardManagerModel.RedemptionAfterDays = 0;
                    break;
            }
        }
        #endregion

        #region ChangeViewExecute
        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            return true;
        }
        #endregion

        #region Insert,Update,Delete
        private void Insert()
        {
            try
            {
                //To insert data into base_RewardManager table.
                this.RewardManagerModel.ToEntity();
                this._rewardManagerRepository.Add(this.RewardManagerModel.base_RewardManager);
                this._rewardManagerRepository.Commit();
                this.RewardManagerModel.EndUpdate();
                App.WriteLUserLog("Reward", "User inserted a new reward.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Insert" + ex.ToString());
            }
        }
        private void Update()
        {
            try
            {
                this.NumbersOfRewardRelation = 0;
                short Availablestatus = (short)GuestRewardStatus.Available;
                //To check that this reward used.
                var checkReward = this._guestRewardRepository.GetAll().Where(x => (x.Status == Availablestatus));
                //To update data into base_GuestReward table.
                if ((this.RewardManagerModel.PurchaseThreshold != this.RewardManagerModel.base_RewardManager.PurchaseThreshold
                    || this.RewardManagerModel.RewardExpiration != this.RewardManagerModel.base_RewardManager.RewardExpiration
                || this.RewardManagerModel.RedemptionAfterDays != this.RewardManagerModel.base_RewardManager.RedemptionAfterDays)
                && (checkReward != null && checkReward.Count() > 0))
                {
                    string message = String.Format("{0}\n {1}\n {2}\n {3}"
                        , Application.Current.FindResource("RW_Message_Keepexisting") as string
                        , Application.Current.FindResource("RW_Message_Yes") as string
                        , Application.Current.FindResource("RW_Message_No") as string
                        , Application.Current.FindResource("RW_Message_Cancel") as string);
                    MessageBoxResult result = MessageBox.Show(message, "Notification", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel)
                        return;
                    else if (result == MessageBoxResult.No)
                    {
                        this.UpdatePurchaseThreshold();
                        if (!this.IsChangePurCharseThreshold)
                        {
                            int date = int.Parse(Common.RewardExpirationTypes.SingleOrDefault(x => x.ObjValue.Equals(this.RewardManagerModel.RewardExpiration.ToString())).Detail.ToString());
                            short RedeemedStatus = (short)GuestRewardStatus.Redeemed;
                            //To group guest on sale order.
                            var queryGuest = this._guestRewardRepository.GetAll().Where(x => (x.Status == Availablestatus || x.Status == RedeemedStatus) && (x.Reason != "Manual" || x.SaleOrderNo.Length > 0)).GroupBy(x => x.GuestId);
                            if (queryGuest != null)
                            {
                                foreach (var itemRewardGroup in queryGuest)
                                    if (itemRewardGroup.Count(x => x.Status == Availablestatus) > 0)
                                    {
                                        //To update RewardExpiration of item on base_GuestReward table.
                                        var RewardAvailable = itemRewardGroup.Where(x => x != null && x.Status == Availablestatus);
                                        foreach (var item in RewardAvailable)
                                            this.UpdateRewardExpiration(item, date);
                                        this._guestRewardRepository.Commit();
                                        this.NumbersOfRewardRelation = this.NumbersOfRewardRelation + RewardAvailable.Count();
                                    }
                            }
                        }
                        this.IsChangePurCharseThreshold = false;
                    }
                }
                //To update data into base_RewardManager table.
                this.RewardManagerModel.ToEntity();
                this._rewardManagerRepository.Commit();
                this.RewardManagerModel.EndUpdate();
                App.WriteLUserLog("Reward", "User updated a reward.");
                //Notification numbers of reward changed.
                if (this.NumbersOfRewardRelation > 0)
                    MessageBox.Show(String.Format(Application.Current.FindResource("RW_Message_Issued") as string, this.NumbersOfRewardRelation), Language.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update" + ex.ToString());
            }
        }

        private void UpdatePurchaseThreshold()
        {
            //To get all of saleOrder on base_guestReward table.
            if (this.RewardManagerModel.PurchaseThreshold != this.RewardManagerModel.base_RewardManager.PurchaseThreshold)
            {
                short Availablestatus = (short)GuestRewardStatus.Available;
                short RedeemedStatus = (short)GuestRewardStatus.Redeemed;
                //To group guest on sale order.
                var queryGuest = this._guestRewardRepository.GetAll().Where(x => (x.Status == Availablestatus || x.Status == RedeemedStatus) && (x.Reason != "Manual" || x.SaleOrderNo.Length > 0)).GroupBy(x => x.GuestId);
                if (queryGuest != null)
                {
                    foreach (var itemGuest in queryGuest)
                    {
                        //To check status of saleOrder on base_GuestReward table.
                        if (itemGuest.Count(x => x.Status == Availablestatus) > 0)
                        {
                            //To get PurchaseDuringTrackingPeriod from base_Guest table.
                            decimal PurchaseDuringTrackingPeriod = itemGuest.First().base_Guest.PurchaseDuringTrackingPeriod;
                            //To group reward on sale order.
                            var queryReward = itemGuest.GroupBy(x => x.SaleOrderNo);
                            //To sum amount of sale order.
                            var RedeemedReward = queryReward.Select(x => x.Where(y => y.Status == RedeemedStatus)).Where(x => x.Count() > 0);
                            //To sum AmountRedeemed from base_GuestReward table if status of item is 2.
                            decimal AmountRedeemedSum = 0;
                            if (RedeemedReward != null && RedeemedReward.Count() > 0)
                                AmountRedeemedSum = RedeemedReward.Sum(x => x.Select(y => y.Amount).First());
                            //decimal AmountRedeemedSum = queryReward.Sum(x => x.Where(y => y.Status == RedeemedStatus).Select(y => y.Amount).First());
                            decimal TotalAmount = PurchaseDuringTrackingPeriod - AmountRedeemedSum;
                            //To count NumbersofReward to insert them into base_GuestReward table.
                            decimal NumbersofReward = Decimal.Truncate(TotalAmount / this.RewardManagerModel.PurchaseThreshold);
                            if (NumbersofReward > 0)
                            {
                                //To insert item into base_GuestReward table.
                                this.InsertGuestReward(NumbersofReward, itemGuest.First());
                                //To update status of item on base_GuestReward table.
                                this.UpdateGuestReward(itemGuest);
                            }
                            else
                                //To update status of item on base_GuestReward table.
                                this.UpdateGuestReward(itemGuest);
                            this.NumbersOfRewardRelation = this.NumbersOfRewardRelation + NumbersofReward;
                        }
                    }
                }
            }
        }

        //To insert item into base_GuestReward table.
        private void InsertGuestReward(decimal number, base_GuestReward reward)
        {
            bool flagChange = (this.RewardManagerModel.RewardExpiration != this.RewardManagerModel.base_RewardManager.RewardExpiration
                || (this.RewardManagerModel.IsBlockRedemption && this.RewardManagerModel.RedemptionAfterDays != this.RewardManagerModel.base_RewardManager.RedemptionAfterDays));
            for (int i = 0; i < number; i++)
            {
                //To update RewardExpiration,TotalRewardRedeemed if user changes them.
                if (flagChange)
                {
                    int date = int.Parse(Common.RewardExpirationTypes.SingleOrDefault(x => x.ObjValue.Equals(this.RewardManagerModel.RewardExpiration.ToString())).Detail.ToString());
                    reward.ActivedDate = DateTimeExt.Today;
                    reward.EarnedDate = DateTimeExt.Today;
                    if (this.RewardManagerModel.IsBlockRedemption && this.RewardManagerModel.RedemptionAfterDays > 0)
                        reward.ActivedDate = DateTimeExt.Today.AddDays(this.RewardManagerModel.RedemptionAfterDays);
                    this.UpdateRewardExpiration(reward, date);
                }
                reward.AppliedDate = DateTimeExt.Today;
                //To insert item into base_GuestReward table.
                this._guestRewardRepository.Add(this.ToEntity(reward));
            }
            this.IsChangePurCharseThreshold = true;
            //To Commit that user insert item.
            this._guestRewardRepository.Commit();
        }
        //To update status of item on base_GuestReward table.
        private void UpdateGuestReward(IGrouping<long, base_GuestReward> Items)
        {
            foreach (var guestReward in Items)
                if (guestReward.Status == (short)GuestRewardStatus.Available)
                    guestReward.Status = (short)GuestRewardStatus.Removed;
            this._guestRewardRepository.Commit();
        }
        //To update RewardExpiration of item on base_GuestReward table.
        private void UpdateRewardExpiration(base_GuestReward reward, int date)
        {
            if (this.RewardManagerModel.RewardExpiration == 0)
                reward.ExpireDate = null;
            else
                reward.ExpireDate = DateTimeExt.Today.AddDays(date + this.RewardManagerModel.RedemptionAfterDays);
        }
        public base_GuestReward ToEntity(base_GuestReward reward)
        {
            base_GuestReward base_GuestReward = new base_GuestReward();
            base_GuestReward.GuestId = reward.GuestId;
            base_GuestReward.RewardId = reward.RewardId;
            base_GuestReward.Amount = 0;
            base_GuestReward.IsApply = false;
            base_GuestReward.EarnedDate = reward.EarnedDate;
            base_GuestReward.AppliedDate = null;
            base_GuestReward.RewardValue = 0;
            base_GuestReward.SaleOrderResource = reward.SaleOrderResource;
            base_GuestReward.SaleOrderNo = reward.SaleOrderNo;
            base_GuestReward.Remark = reward.Remark;
            base_GuestReward.ActivedDate = reward.ActivedDate;
            base_GuestReward.ExpireDate = reward.ExpireDate;
            base_GuestReward.Reason = reward.Reason;
            base_GuestReward.Status = (short)GuestRewardStatus.Available;
            return base_GuestReward;
        }
        /// <summary>
        /// To update IsLock on base_ResoureceAccount table.
        /// </summary>
        private bool IsEditData()
        {
            bool isUnactive = true;
            if (this.RewardManagerModel != null)
            {
                if (this.RewardManagerModel.IsDirty)
                {
                    MessageBoxResult msgResult = MessageBox.Show(Language.Text13, Language.Information, MessageBoxButton.YesNo);
                    if (msgResult == MessageBoxResult.Yes)
                        //To don't do anything if user click Cancel.
                        isUnactive = false;
                    else
                        //To close view or to change item .
                        isUnactive = true;
                }
            }
            return isUnactive;
        }

        #endregion

        #region SetDefaultValue
        /// <summary>
        /// To set default value for fields.
        /// </summary>
        private void SetDefaultValue()
        {

        }
        #endregion

        #region RollBackData
        /// <summary>
        /// To rollback data when user click Cancel.
        /// </summary>
        private void RollBackData(bool isChangeItem)
        {

        }
        #endregion

        #region OnSelectChanging
        private void OnSelectChanging()
        {

        }
        #endregion

        #region VisibilityData
        /// <summary>
        ///To show item when user check into Check Box.
        /// </summary>
        private void SetVisibilityData(bool value)
        {

        }
        #endregion

        #endregion

        #region Public Methods

        #region OnViewChangingCommandCanExecute
        /// <summary>
        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return this.IsEditData();
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            this.InitialData();
            this.LoadRewardManager();
        }
        #endregion

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                switch (columnName)
                {
                    case "EnableFilteringData":
                        break;
                }
                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }

        #endregion
    }
}
