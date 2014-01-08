using System.Linq;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows;
using CPC.POS.View;
using CPC.Control;
using System;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using CPC.POS.Database;
using CPC.Helper;

namespace CPC.POS.ViewModel
{

    /// <summary>
    /// Verify code reward Card
    /// </summary>
    class VerifyRedeemRewardViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand RedeemCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        public enum ReeedemRewardType
        {
            Apply = 1,
            Redeemded = 2,//redeem later
            Cancel = 3
        }
        public enum ValidateType
        {
            None = 0,
            Success = 1,
            Fail = 2,
            NotAny = 3,
            Existed = 4
        }



        public ConfirmMemberRedeemRewardView ConfirmMemberRedeemRewardView { get; set; }
        #endregion

        #region Constructors
        public VerifyRedeemRewardViewModel(base_SaleOrderModel saleOrderModel)
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
            SaleOrderModel = saleOrderModel;

            //Load Member ship if null
            //For Form SaleOrder is Existed & Membership is just created;
            short memebershipActivedStatus = (short)MemberShipStatus.Actived;
            if (saleOrderModel.GuestModel.MembershipValidated == null)
            {
                base_MemberShip membership = saleOrderModel.GuestModel.base_Guest.base_MemberShip.FirstOrDefault(x => x.Status.Equals(memebershipActivedStatus));
                if (membership != null)
                    saleOrderModel.GuestModel.MembershipValidated = new base_MemberShipModel(membership);
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

        #region ValidReward
        private int _validReward;
        /// <summary>
        /// Gets or sets the ValidMember.
        /// None = 0;
        /// Success=1
        /// Fail = 2
        /// </summary>
        public int ValidReward
        {
            get { return _validReward; }
            set
            {
                if (_validReward != value)
                {
                    _validReward = value;
                    OnPropertyChanged(() => ValidReward);
                }
            }
        }
        #endregion

        #region IDCardNumber
        private string _idCardNumber;
        /// <summary>
        /// Gets or sets the IDCardNumber.
        /// </summary>
        public string IDCardNumber
        {
            get { return _idCardNumber; }
            set
            {
                if (_idCardNumber != value)
                {
                    _idCardNumber = value;
                    OnPropertyChanged(() => IDCardNumber);
                }
            }
        }
        #endregion

        #region Message
        private string _message = string.Empty;
        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(() => Message);
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

            if (SaleOrderModel == null)
                return false;
            return
                 SaleOrderModel.GuestModel.MembershipValidated != null
                 && ValidReward.Is(ValidateType.Success)
                && SaleOrderModel.GuestModel.GuestRewardCollection != null;
                
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute()
        {
            ViewActionType = ReeedemRewardType.Apply;
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
            //return false;
            return true;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnRedeemCommandExecute()
        {
            ViewActionType = ReeedemRewardType.Redeemded;
            SaleOrderModel.GuestModel.GuestRewardCollection.Clear();
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
            SaleOrderModel.GuestModel.GuestRewardCollection.Clear();
            //if (SaleOrderModel.GuestModel.GuestRewardCollection.Any(x => x.IsChecked))
            //{
            //    foreach (base_GuestRewardModel guestRewardUpdated in SaleOrderModel.GuestModel.GuestRewardCollection.Where(x => x.IsChecked))
            //        guestRewardUpdated.IsChecked = false;
            //}
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #region BarcodeInput
        /// <summary>
        /// Gets the BarcodeInput Command.
        /// <para>With Retun(Enter) key press</para> 
        /// <summary>

        public RelayCommand<object> BarcodeInputCommand { get; private set; }



        /// <summary>
        /// Method to check whether the BarcodeInput command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnBarcodeInputCommandCanExecute(object param)
        {
            return !string.IsNullOrWhiteSpace(IDCardNumber);
        }


        /// <summary>
        /// Method to invoke when the BarcodeInput command is executed.
        /// </summary>
        private void OnBarcodeInputCommandExecute(object param)
        {
            if (param != null)
            {
                if (param.ToString().IndexOf("****") >= 0 && ValidReward.Is(ValidateType.Success))
                {
                    if (OnApplyCommandCanExecute())
                    {
                        OnApplyCommandExecute();
                    }
                }
                else
                {
                    VerifyRewardCode(param.ToString());
                }
            }
        }


        #endregion

        #region ViewLoadedCommand
        /// <summary>
        /// Gets the ViewLoaded Command.
        /// <summary>

        public RelayCommand<object> ViewLoadedCommand { get; private set; }



        /// <summary>
        /// Method to check whether the ViewLoaded command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnViewLoadedCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the ViewLoaded command is executed.
        /// </summary>
        private void OnViewLoadedCommandExecute(object param)
        {
            GetView();
            ConfirmMemberRedeemRewardView.txtMemberBarcode.Focus();
        }
        #endregion

        #region BarcodeChangedCommand
        /// <summary>
        /// Gets the BarcodeChanged Command.
        /// <summary>

        public RelayCommand<object> BarcodeChangedCommand { get; private set; }


        /// <summary>
        /// Method to check whether the BarcodeChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnBarcodeChangedCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the BarcodeChanged command is executed.
        /// </summary>
        private void OnBarcodeChangedCommandExecute(object param)
        {
            if (IDCardNumber.Length == 1 && ValidReward != (int)ValidateType.None)
            {
                ValidReward = (int)ValidateType.None;
                this.Message = string.Empty;
                ConfirmMemberRedeemRewardView.imgValid.Fill = SetImageSource();
            }
            else if (ValidReward == (int)ValidateType.Success && IDCardNumber.IndexOf("****") < 0)
            {
                ValidReward = (int)ValidateType.None;
                ConfirmMemberRedeemRewardView.imgValid.Fill = SetImageSource();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(IDCardNumber)
                    && IDCardNumber.IndexOf("****") < 0
                    && IDCardNumber.Length == 13
                    && OnBarcodeInputCommandCanExecute(null))
                {
                    VerifyRewardCode(IDCardNumber);
                }
            }


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
            //With Retun(Enter) key press
            BarcodeInputCommand = new RelayCommand<object>(OnBarcodeInputCommandExecute, OnBarcodeInputCommandCanExecute);
            ViewLoadedCommand = new RelayCommand<object>(OnViewLoadedCommandExecute, OnViewLoadedCommandCanExecute);
            BarcodeChangedCommand = new RelayCommand<object>(OnBarcodeChangedCommandExecute, OnBarcodeChangedCommandCanExecute);
        }

        /// <summary>
        /// Verify MemeberShip
        /// </summary>
        /// <param name="barcode"></param>
        private void VerifyRewardCode(string barcode)
        {
            GetView();

            ConfirmMemberRedeemRewardView.imgValid.Fill = null;
            if (barcode != null && !string.IsNullOrWhiteSpace(barcode) && SaleOrderModel.GuestModel.MembershipValidated != null)
            {
                SaleOrderModel.GuestModel.GuestRewardCollection.Clear();
                string msg = string.Empty;
                base_GuestReward guestReward = null;
                
                if (CheckReward(barcode, out msg, out guestReward))
                {
                    Message = string.Empty;
                    ValidReward = (int)ValidateType.Success;
                    ConfirmMemberRedeemRewardView.txtMemberBarcode.Text = "*************";
                    ConfirmMemberRedeemRewardView.txtMemberBarcode.SelectAll();
                    ConfirmMemberRedeemRewardView.btnApply.IsDefault = true;

                    SaleOrderModel.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestReward));
                }
                else
                {
                    FailValidation(msg);
                }
            }

            ConfirmMemberRedeemRewardView.imgValid.Fill = SetImageSource();
        }

        /// <summary>
        /// Set Form Valid Fail
        /// </summary>
        /// <param name="msg"></param>
        private void FailValidation(string msg)
        {
            ValidReward = (int)ValidateType.Fail;
            Message = msg;
            if (SaleOrderModel.GuestModel.MembershipValidated != null)
                SaleOrderModel.GuestModel.MembershipValidated.BarcodeValidation = (int)base_MemberShipModel.BarcodeValidates.None;
            ConfirmMemberRedeemRewardView.txtMemberBarcode.Text = string.Empty;
            ConfirmMemberRedeemRewardView.txtMemberBarcode.Focus();
        }

        /// <summary>
        /// Check Reward & Show Notify
        /// </summary>
        /// <param name="bardCode"></param>
        /// <param name="message"></param>
        /// <param name="reward"></param>
        /// <returns></returns>
        private bool CheckReward(string bardCode, out string message, out base_GuestReward reward)
        {
            bool result = false;
            message = string.Empty;
            reward = null;

            base_GuestReward guesReward = SaleOrderModel.GuestModel.base_Guest.base_GuestReward.SingleOrDefault(x => x.ScanCode.Equals(bardCode));
            if (guesReward != null)
            {
                reward = guesReward;
                if (guesReward.ActivedDate > DateTime.Today)
                {
                    message = Language.GetMsg("SO_Message_RewardNotReadyToUse");
                    result = false;
                }else if (guesReward.ExpireDate.HasValue && guesReward.ExpireDate <= DateTime.Today)
                {
                    message = Language.GetMsg("SO_Message_RewardExpired");
                    result = false;
                }
                else
                {
                    if (guesReward.base_GuestRewardDetail.Sum(x => x.RewardRedeemed) == guesReward.RewardValueEarned)
                    {
                        message = Language.GetMsg("SO_Message_RewardEmpty");
                        result = false;
                    }
                    else
                    {
                        message = Language.GetMsg("SO_Message_RewardCorrect");
                        result = true;
                    }
                }
            }
            else
            {
                message = Language.GetMsg("SO_Message_RewardNotCorrect");
            }
            return result;
        }

        private void GetView()
        {
            PopupContainer popupContainer = FindOwnerWindow(_ownerViewModel) as PopupContainer;
            if (popupContainer != null && ConfirmMemberRedeemRewardView == null)
                ConfirmMemberRedeemRewardView = popupContainer.grdContent.Children[0] as ConfirmMemberRedeemRewardView;
        }

        private DrawingBrush SetImageSource()
        {
            ValidateType validMember = (ValidateType)Enum.Parse(typeof(ValidateType), ValidReward.ToString());
            FrameworkElement fwElement = new FrameworkElement();
            DrawingBrush img = null;
            switch (validMember)
            {
                case ValidateType.None:
                    img = null;
                    break;
                case ValidateType.Success:
                    img = (fwElement.TryFindResource("OK") as DrawingBrush);
                    break;
                case ValidateType.Fail:
                    img = (fwElement.TryFindResource("Error") as DrawingBrush);
                    break;
            }
            return img;
        }

        private void Reset()
        {
            Message = string.Empty;
            ValidReward = (int)ValidateType.None;
            ConfirmMemberRedeemRewardView.imgValid.Fill = SetImageSource();
            ConfirmMemberRedeemRewardView.txtMemberBarcode.Text = string.Empty;
        }
        #endregion

        #region Events


        #endregion
    }
}
