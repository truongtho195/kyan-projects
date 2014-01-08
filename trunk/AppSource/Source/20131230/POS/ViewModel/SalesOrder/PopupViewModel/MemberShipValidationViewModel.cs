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
using CPC.POS.Database;
using System.Collections.Generic;

namespace CPC.POS.ViewModel
{

    /// <summary>
    /// Verify membership code
    /// </summary>
    class MemberShipValidationViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand RedeemCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_MemberShipRepository _memberShipRepository= new base_MemberShipRepository();

        public enum ReeedemRewardType
        {
            Apply = 1,
            Redeemded = 2,
            Cancel = 3
        }
        public enum ValidateType
        {
            None = 0,
            Success = 1,
            Fail = 2
        }

        public MemberShipValidationView MemberShipValidationView { get; set; }
        #endregion

        #region Constructors
        public MemberShipValidationViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        //public MemberShipValidationViewModel(base_SaleOrderModel saleOrderModel)
        //    : this()
        //{
        //    SaleOrderModel = saleOrderModel;
        //}
        #endregion

        #region Properties

        #region SaleOrderModel

        public ReeedemRewardType ViewActionType { get; set; }

        #region ValidMember
        private int _validMember;
        /// <summary>
        /// Gets or sets the ValidMember.
        /// None = 0;
        /// Success=1
        /// Fail = 2
        /// </summary>
        public int ValidMember
        {
            get { return _validMember; }
            set
            {
                if (_validMember != value)
                {
                    _validMember = value;
                    OnPropertyChanged(() => ValidMember);
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

        #region CustomerModel
        private base_GuestModel _customerModel;
        /// <summary>
        /// Gets or sets the Customer.
        /// </summary>
        public base_GuestModel CustomerModel
        {
            get { return _customerModel; }
            set
            {
                if (_customerModel != value)
                {
                    _customerModel = value;
                    OnPropertyChanged(() => CustomerModel);
                }
            }
        }
        #endregion

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

            return ValidMember == (int)ValidateType.Success
                && CustomerModel != null;
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

        #region BarcodeInput
        /// <summary>
        /// Gets the BarcodeInput Command.
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
                VerifyMemberShip(param.ToString());
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
            this.MemberShipValidationView.txtMemberBarcode.Focus();
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
            if (IDCardNumber.Length == 1 && ValidMember != (int)ValidateType.None)
            {
                ValidMember = (int)ValidateType.None;
                this.MemberShipValidationView.imgValid.Fill = SetImageSource();
            }
            else if (ValidMember == (int)ValidateType.Success && IDCardNumber.IndexOf("****") < 0)
            {
                ValidMember = (int)ValidateType.None;
                this.MemberShipValidationView.imgValid.Fill = SetImageSource();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(IDCardNumber)
                    && IDCardNumber.IndexOf("****") < 0
                    && IDCardNumber.Length == 13
                    && OnBarcodeInputCommandCanExecute(null))
                {
                    VerifyMemberShip(IDCardNumber);
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
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            BarcodeInputCommand = new RelayCommand<object>(OnBarcodeInputCommandExecute, OnBarcodeInputCommandCanExecute);
            ViewLoadedCommand = new RelayCommand<object>(OnViewLoadedCommandExecute, OnViewLoadedCommandCanExecute);
            BarcodeChangedCommand = new RelayCommand<object>(OnBarcodeChangedCommandExecute, OnBarcodeChangedCommandCanExecute);
        }

        /// <summary>
        /// Verify MemeberShip
        /// </summary>
        /// <param name="barcode"></param>
        private void VerifyMemberShip(string barcode)
        {
            GetView();

            this.MemberShipValidationView.imgValid.Fill = null;
            if (ValidMember == (int)ValidateType.Success && OnApplyCommandCanExecute())
            {
                OnApplyCommandExecute();
            }
            else if (barcode != null && !string.IsNullOrWhiteSpace(barcode))
            {

                short memebershipActivedStatus = (short)MemberShipStatus.Actived;
                IEnumerable<base_MemberShip> memberShipValidateds = _memberShipRepository.GetAll(x => x.Status.Equals(memebershipActivedStatus) && x.IdCard != string.Empty && x.IdCard.Equals(barcode));
                if (memberShipValidateds.Any())
                {
                    ValidMember = (int)ValidateType.Success;

                    this.MemberShipValidationView.txtMemberBarcode.Text = "*************";
                    this.MemberShipValidationView.txtMemberBarcode.SelectAll();
                    base_MemberShip membership = memberShipValidateds.FirstOrDefault();
                    CustomerModel = new base_GuestModel(membership.base_Guest);
                }
                else
                {
                    ValidMember = (int)ValidateType.Fail;
                    CustomerModel = null;
                    this.MemberShipValidationView.txtMemberBarcode.Text = string.Empty;
                    this.MemberShipValidationView.txtMemberBarcode.Focus();
                }
            }
            else
            {
                ValidMember = (int)ValidateType.Fail;
                CustomerModel = null;
                this.MemberShipValidationView.txtMemberBarcode.Text = string.Empty;
                this.MemberShipValidationView.txtMemberBarcode.Focus();
            }

            this.MemberShipValidationView.imgValid.Fill = SetImageSource();
        }

        private void GetView()
        {
            PopupContainer popupContainer = FindOwnerWindow(_ownerViewModel) as PopupContainer;
            if (popupContainer != null && this.MemberShipValidationView == null)
                this.MemberShipValidationView = popupContainer.grdContent.Children[0] as MemberShipValidationView;
        }

        private DrawingBrush SetImageSource()
        {
            ValidateType validMember = (ValidateType)Enum.Parse(typeof(ValidateType), ValidMember.ToString());
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


        #endregion

    }
}
