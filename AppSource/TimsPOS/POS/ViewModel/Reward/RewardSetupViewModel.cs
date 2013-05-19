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

        //Repository
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
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
            this.Delete();
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
            this.Delete();
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
                //To update data into base_RewardManager table.
                this.RewardManagerModel.ToEntity();
                this._rewardManagerRepository.Commit();
                this.RewardManagerModel.EndUpdate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update" + ex.ToString());
            }
        }
        /// <summary>
        /// To update IsLock on base_ResoureceAccount table.
        /// </summary>
        private void Delete()
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to lock this account?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Delete" + ex.ToString());
            }
        }

        private bool IsEditData()
        {
            bool isUnactive = true;
            if (this.RewardManagerModel != null)
            {
                if (this.RewardManagerModel.IsDirty)
                {
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
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
