using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Repository;
using CPC.POS.Model;
using CPC.POS.Database;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace CPC.POS.ViewModel
{
    /// <summary>
    /// Using Gift card or certification to payment
    /// </summary>
    class CardPaymentViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        private decimal _balance = 0;
        private base_CardManagementRepository _cardManagementRepository = new base_CardManagementRepository();
        #endregion

        #region Constructors
        public CardPaymentViewModel(base_ResourcePaymentDetailModel paymentMethod, base_ResourcePaymentModel paymentModel)
        {
            PaymentModel = paymentModel;
            PaymentMethodModel = paymentMethod.Clone();
            _balance = paymentModel.Balance;
            if (PaymentMethodModel.CouponCardModel != null)
            {
               _amount = PaymentMethodModel.Paid;
               OnPropertyChanged(() => Amount);
                _cardNumber = PaymentMethodModel.CouponCardModel.CardNumber;
                OnPropertyChanged(() => CardNumber);
                IsCardValid = true;
            }
            else
            {
                IsCardValid = null;
            }
            InitialCommand();
        }
        #endregion

        #region Properties
        public base_ResourcePaymentModel PaymentModel { get; private set; }

        public base_ResourcePaymentDetailModel PaymentMethodModel { get; private set; }

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
                    OnPropertyChanged(() => Amount);
                    AmountChanged();

                }
            }
        }


        #endregion

        #region CardNumber
        private string _cardNumber;
        /// <summary>
        /// Gets or sets the CardNumber.
        /// </summary>
        public string CardNumber
        {
            get { return _cardNumber; }
            set
            {
                if (_cardNumber != value)
                {
                    _cardNumber = value;
                    OnPropertyChanged(() => CardNumber);
                    IsCardValid = null;
                    PaymentMethodModel.CouponCardModel = null;
                    SetImageSource(IsCardValid);
                }
            }
        }
        #endregion

        #region IsCardValid
        private bool? _isCardValid;
        /// <summary>
        /// Gets or sets the IsCardValid.
        /// </summary>
        public bool? IsCardValid
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

        #region AmountError
        private string _amountError;
        /// <summary>
        /// Gets or sets the AmountError.
        /// </summary>
        public string AmountError
        {
            get { return _amountError; }
            set
            {
                if (_amountError != value)
                {
                    _amountError = value;
                    OnPropertyChanged(() => AmountError);
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


        #region ErrorInfo
        private string _errorInfo;
        /// <summary>
        /// Gets or sets the ErrorInfo.
        /// </summary>
        public string ErrorInfo
        {
            get { return _errorInfo; }
            set
            {
                if (_errorInfo != value)
                {
                    _errorInfo = value;
                    OnPropertyChanged(() => ErrorInfo);
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
            return IsCardValid==true && string.IsNullOrWhiteSpace(ErrorInfo) && PaymentMethodModel.CouponCardModel != null;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            PaymentMethodModel.CouponCardModel.IsDirty = true;
            PaymentMethodModel.Reference = CardNumber;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region Cancel Command
        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #region ValidateCardNumberCommand
        /// <summary>
        /// Gets the ValidateCardNumber Command.
        /// <summary>

        public RelayCommand<object> ValidateCardNumberCommand { get; private set; }



        /// <summary>
        /// Method to check whether the ValidateCardNumber command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnValidateCardNumberCommandCanExecute(object param)
        {
            if (string.IsNullOrWhiteSpace(CardNumber))
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the ValidateCardNumber command is executed.
        /// </summary>
        private void OnValidateCardNumberCommandExecute(object param)
        {
            if (PaymentMethodModel.CouponCardModel != null)
                return;

            short cardTypeId = Convert.ToInt16(this.PaymentMethodModel.PaymentMethodId);
            //256 : StoreCard
            IQueryable<base_CardManagement> query = _cardManagementRepository.GetIQueryable(x => !x.IsPurged && x.IsSold && (x.CardTypeId.Equals(cardTypeId) || x.CardTypeId.Equals(256)) && x.CardNumber.Equals(CardNumber));
            _cardManagementRepository.Refresh(query);
            if (query.Any())
            {
                base_CardManagementModel couponCardModel = new base_CardManagementModel(query.FirstOrDefault());
                PaymentMethodModel.CouponCardModel = couponCardModel;
                IsCardValid = true;
            }
            else
            {
                PaymentMethodModel.CouponCardModel = null;
                IsCardValid = false;
            }

            if (Amount > 0)
                CheckRemainCardAmount();
            SetImageSource(IsCardValid);
            OnPropertyChanged(() => Error);
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            ValidateCardNumberCommand = new RelayCommand<object>(OnValidateCardNumberCommandExecute, OnValidateCardNumberCommandCanExecute);
        }

        /// <summary>
        /// Amount Changed
        /// </summary>
        private void AmountChanged()
        {

            this.PaymentMethodModel.Paid = Amount;

            CheckRemainCardAmount();

            OnPropertyChanged(() => Error);
            //if (!ValidatePaid(this.PaymentMethodModel))
            //{
            //    AmountError = "Paid is not greater than remaining total";
            //}
        }


        private void CheckRemainCardAmount()
        {
            AmountError = string.Empty;
            if (this.PaymentMethodModel.CouponCardModel == null)
            {
                this.IsCardValid = false;
                return;
            }
            if (!CheckRemainCard(this.PaymentMethodModel))
            {
                AmountError = "Remain in card is not enough";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentDetailModel"></param>
        /// <returns></returns>
        private bool CheckRemainCard(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            if (paymentDetailModel.CouponCardModel != null && paymentDetailModel.CouponCardModel.RemainingAmount >= Amount)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Check total paid is not greater than Total Amount(remaining Total)
        /// </summary>
        /// <param name="paymentDetailModel"></param>
        private bool ValidatePaid(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            //Total Paid without current payment
            decimal totalPaid = PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId != PaymentMethodModel.PaymentMethodId).Sum(x => x.Paid);
            if (PaymentMethodModel.Paid + totalPaid > PaymentModel.TotalAmount)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //Set Image for textbox 
        private DrawingBrush SetImageSource(bool? validType)
        {
            FrameworkElement fwElement = new FrameworkElement();
            DrawingBrush img = null;
            if (validType == true)
            {
                img = (fwElement.TryFindResource("OK") as DrawingBrush);
            }
            else if (validType == false)
            {
                img = (fwElement.TryFindResource("Error") as DrawingBrush);
            }

            IconInfo = img;

            return img;
        }


        #endregion

        #region NotifyDataError
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
                        break;
                    case "IsCardValid":
                        if (IsCardValid == false)
                            message = "Card is not valid";
                        break;
                    case "AmountError":
                        if (!string.IsNullOrWhiteSpace(AmountError))
                            message = AmountError;
                        break;


                }
                ErrorInfo = message;
                if (!string.IsNullOrWhiteSpace(message))
                    return message;


                return null;
            }
        }
        #endregion
    }


}
