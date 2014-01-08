using System;
using System.Collections.Generic;
using System.ComponentModel;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Repository;
using CPC.POS.Database;
using System.Linq;
using System.Windows.Media;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class CouponViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        private base_CardManagementRepository _cardManagementRepository = new base_CardManagementRepository();

        public enum ValidateType
        {
            None = 0,
            Success = 1,
            Fail = 2
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CouponViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        #endregion

        #region Properties

        #region IsDirty
        private bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }
        #endregion

        #region Amount
        private decimal _amount;
        /// <summary>
        /// Gets or sets the Amount.
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Amount);
                }
            }
        }
        #endregion

        #region CouponCode
        private string _couponCode;
        /// <summary>
        /// Gets or sets the CouponCode.
        /// </summary>
        public string CouponCode
        {
            get { return _couponCode; }
            set
            {
                if (_couponCode != value)
                {
                    _couponCode = value;
                    IsDirty = true;
                    IsCardValid = false;
                    SetImageSource(ValidateType.None);
                    OnPropertyChanged(() => CouponCode);
                    OnPropertyChanged(() => Error);
                }
            }
        }
        #endregion

        #region SaleOrderDetailModel
        private base_SaleOrderDetailModel _saleOrderDetailModel;
        /// <summary>
        /// Gets or sets the SaleOrderDetailModel.
        /// </summary>
        public base_SaleOrderDetailModel SaleOrderDetailModel
        {
            get { return _saleOrderDetailModel; }
            set
            {
                if (_saleOrderDetailModel != value)
                {
                    _saleOrderDetailModel = value;
                    OnPropertyChanged(() => SaleOrderDetailModel);
                    SaleOrderDetailModelChanged();
                }
            }
        }

        private void SaleOrderDetailModelChanged()
        {
            if (SaleOrderDetailModel != null)
            {
                this.Amount = SaleOrderDetailModel.SalePrice > 0 ? SaleOrderDetailModel.SalePrice : 0;
                this.CouponCode = !string.IsNullOrWhiteSpace(SaleOrderDetailModel.SerialTracking) ? SaleOrderDetailModel.SerialTracking : string.Empty;
                this.CouponCardModel = SaleOrderDetailModel.CouponCardModel;
                if (SaleOrderDetailModel.CouponCardModel != null)
                {
                    IsCardValid = true;
                    SetImageSource(ValidateType.Success);
                }
                else
                {
                    SetImageSource(ValidateType.None);
                }
                this.IsDirty = false;
            }
        }
        #endregion

        #region CouponCardModel
        private base_CardManagementModel _couponCardModel;
        /// <summary>
        /// Gets or sets the CouponCardModel.
        /// </summary>
        public base_CardManagementModel CouponCardModel
        {
            get { return _couponCardModel; }
            set
            {
                if (_couponCardModel != value)
                {
                    _couponCardModel = value;
                    OnPropertyChanged(() => CouponCardModel);
                }
            }
        }
        #endregion

        #region IsCardValid
        private bool _isCardValid = false;
        /// <summary>
        /// Gets or sets the IsCardValid.
        /// </summary>
        public bool IsCardValid
        {
            get { return _isCardValid; }
            set
            {
                if (_isCardValid != value)
                {
                    _isCardValid = value;
                    OnPropertyChanged(() => IsCardValid);
                }
            }
        }
        #endregion

        #region IconInfo
        private DrawingBrush _iconInfo;
        /// <summary>
        /// Gets or sets the IconInfo.
        /// </summary>
        public DrawingBrush IconInfo
        {
            get { return _iconInfo; }
            set
            {
                if (_iconInfo != value)
                {
                    _iconInfo = value;
                    OnPropertyChanged(() => IconInfo);
                }
            }
        }
        #endregion

        #region SaleOrderModel
        private base_SaleOrderModel _saleOrderModel;
        /// <summary>
        /// Gets or sets the SaleOrderModel.
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

        #endregion

        #region Commands Methods

        #region OkCommand

        /// <summary>
        /// Gets the Ok Command.
        /// <summary>
        public RelayCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute(object param)
        {
            return this.IsDirty && IsValid && IsCardValid;
        }

        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            if (IsDirty)
            {
                SaleOrderDetailModel.SalePrice = this.Amount;
                SaleOrderDetailModel.SerialTracking = this.CouponCode;
                SaleOrderDetailModel.CouponCardModel = CouponCardModel;
                SaleOrderDetailModel.CouponCardModel.InitialAmount = this.Amount;
            }
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }

        #endregion

        #region Cancel Command

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>
        public RelayCommand CancelCommand { get; private set; }




        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #region ValidateCardCommand

        /// <summary>
        /// Gets the ValidateCard Command.
        /// <summary>

        public RelayCommand<object> ValidateCardCommand { get; private set; }



        /// <summary>
        /// Method to check whether the ValidateCard command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnValidateCardCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the ValidateCard command is executed.
        /// </summary>
        private void OnValidateCardCommandExecute(object param)
        {
            base_CardManagementModel cardManagementModel;
            
            if (CheckCardToSale(out cardManagementModel))
            {
                IsCardValid = true;
                CouponCardModel = cardManagementModel;
                //Set Amount For Card
                Amount = CouponCardModel.InitialAmount;
                SetImageSource(ValidateType.Success);
            }
            else
            {
                Amount = 0;
                IsCardValid = false;
                SetImageSource(ValidateType.Fail);
            }
            OnPropertyChanged(() => Error);
        }
        #endregion



        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            ValidateCardCommand = new RelayCommand<object>(OnValidateCardCommandExecute, OnValidateCardCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private bool CheckCardToSale(out base_CardManagementModel cardManagementModel)
        {
            if (!string.IsNullOrWhiteSpace(CouponCode))
            {
                //Check Code is Existed in SaleOrder
                if (SaleOrderModel.SaleOrderDetailCollection.Any(x => !x.Resource.Equals(SaleOrderDetailModel.Resource) && x.SerialTracking.Trim().Equals(CouponCode.Trim())))
                {
                    cardManagementModel = null;
                    return false;
                }
                else
                {
                    short cardTypeId = Convert.ToInt16(SaleOrderDetailModel.ProductModel.ProductCategoryId);
                    IQueryable<base_CardManagement> query = _cardManagementRepository.GetIQueryable(x => x.CardTypeId.Equals(cardTypeId) && !x.IsPurged && !x.IsSold && (x.GuestResourcePurchased == null || x.GuestResourcePurchased == string.Empty) && x.CardNumber.Equals(CouponCode));
                    _cardManagementRepository.Refresh(query);
                    if (query.Any())
                    {
                        cardManagementModel = new base_CardManagementModel(query.FirstOrDefault());
                        cardManagementModel.IsNew = true;
                        return true;
                    }
                    else if (SaleOrderDetailModel.CouponCardModel != null && SaleOrderDetailModel.CouponCardModel.CardNumber.Equals(CouponCode))
                    {
                        cardManagementModel = SaleOrderDetailModel.CouponCardModel;
                        return true;
                    }
                }
            }
            cardManagementModel = null;
            return false;
        }

        private DrawingBrush SetImageSource(ValidateType validType)
        {
            FrameworkElement fwElement = new FrameworkElement();
            DrawingBrush img = null;
            switch (validType)
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
            IconInfo = img;

            return img;
        }
        #endregion

        #region Public Methods

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets or sets the IsError.
        /// </summary>
        public bool IsError
        {
            get { return !string.IsNullOrWhiteSpace(Error); }

        }

        public string Error
        {
            get
            {
                List<string> errors = new List<string>();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string msg = this[prop.Name];
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Add(msg);
                    }
                }
                return string.Join(Environment.NewLine, errors);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                switch (columnName)
                {
                    case "Amount":
                        //if (Amount == 0)
                        //    message = "Amount is required";
                        break;
                    case "CouponCode":
                        if (string.IsNullOrWhiteSpace(CouponCode))
                            message = "Coupon Code is required";
                        break;
                    case "IsCardValid":
                        if (!string.IsNullOrWhiteSpace(CouponCode) &&!IsCardValid)
                            message = "Card is not valid";
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