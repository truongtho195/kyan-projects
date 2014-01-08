using System;
using System.Linq;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Database;
using CPC.POS.Repository;

namespace CPC.POS.ViewModel
{
    /// <summary>
    /// Payment with credit card
    /// </summary>
    class PaymentCardViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        private decimal _balance = 0;
        private decimal _remainTotal = 0;
        private base_CardManagementRepository _cardManagementRepository = new base_CardManagementRepository();
        #endregion

        #region Constructors
        public PaymentCardViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        public PaymentCardViewModel(base_ResourcePaymentDetailModel paymentMethod, base_ResourcePaymentModel paymentModel)
            : this()
        {
            PaymentModel = paymentModel;
            PaymentMethodModel = paymentMethod;
            SelectedPaymentModel = PaymentMethodModel.Clone();
            SelectedPaymentModel.PaymentCardCollection.Clear();
            IsAllowTip = PaymentMethodModel.PaymentMethodId.Equals((short)PaymentMethod.CreditCard) && (Define.CONFIGURATION.IsAllowCollectTipCreditCard.HasValue ? Define.CONFIGURATION.IsAllowCollectTipCreditCard.Value : false);
            _balance = paymentModel.Balance;
            foreach (base_ResourcePaymentDetailModel paymentCardModel in PaymentMethodModel.PaymentCardCollection)
            {
                base_ResourcePaymentDetailModel paymentClone = paymentCardModel.Clone();
                paymentClone.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PaymentDetailModel_PropertyChanged);
                SelectedPaymentModel.PaymentCardCollection.Add(paymentClone);
            }
        }


        #endregion

        #region Properties

        public base_ResourcePaymentModel PaymentModel { get; set; }

        #region PaymentMethodModel
        private base_ResourcePaymentDetailModel _selectedPaymentMethodModel;
        /// <summary>
        /// Gets or sets the PaymentMethodModel.
        /// <para>Property for binding to view</para>
        /// </summary>
        public base_ResourcePaymentDetailModel SelectedPaymentModel
        {
            get { return _selectedPaymentMethodModel; }
            set
            {
                if (_selectedPaymentMethodModel != value)
                {
                    _selectedPaymentMethodModel = value;
                    OnPropertyChanged(() => SelectedPaymentModel);
                }
            }
        }
        #endregion

        #region PaymentMethodModel
        private base_ResourcePaymentDetailModel _paymentMethodModel;
        /// <summary>
        /// Gets or sets the PaymentMethodModel.
        /// </summary>
        public base_ResourcePaymentDetailModel PaymentMethodModel
        {
            get { return _paymentMethodModel; }
            set
            {
                if (_paymentMethodModel != value)
                {
                    _paymentMethodModel = value;
                    OnPropertyChanged(() => PaymentMethodModel);
                }
            }
        }
        #endregion

        #region SelectedCard
        private base_ResourcePaymentDetailModel _selectedCard;
        /// <summary>
        /// Gets or sets the SelectedCard.
        /// </summary>
        public base_ResourcePaymentDetailModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    OnPropertyChanged(() => SelectedCard);
                }
            }
        }
        #endregion

        #region IsAllowTip
        private bool _isAllowTip = false;
        /// <summary>
        /// Gets or sets the IsAllowTip.
        /// </summary>
        public bool IsAllowTip
        {
            get { return _isAllowTip; }
            set
            {
                if (_isAllowTip != value)
                {
                    _isAllowTip = value;
                    OnPropertyChanged(() => IsAllowTip);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            if (SelectedPaymentModel == null)
                return false;
            return SelectedPaymentModel.PaymentCardCollection != null
                && !SelectedPaymentModel.PaymentCardCollection.Any(x => x.IsError)
                && SelectedPaymentModel.PaymentCardCollection.Any(x => x.Paid > 0);
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            SelectedPaymentModel.Paid = SelectedPaymentModel.PaymentCardCollection.Sum(x => x.Paid);
            SelectedPaymentModel.Reference = string.Join(", ", SelectedPaymentModel.PaymentCardCollection.Where(x => x.Paid > 0).Select(x => x.CardName));
            PaymentMethodModel = SelectedPaymentModel;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region Cancel Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #region FillMoneyCommand
        /// <summary>
        /// Gets the FillMoney Command.
        /// <summary>

        public RelayCommand<object> FillMoneyCommand { get; private set; }



        /// <summary>
        /// Method to check whether the FillMoney command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnFillMoneyCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the FillMoney command is executed.
        /// </summary>
        private void OnFillMoneyCommandExecute(object param)
        {
            if (SelectedPaymentModel != null && SelectedPaymentModel.PaymentCardCollection != null)
                _remainTotal = _balance - SelectedPaymentModel.PaymentCardCollection.Where(x => x != SelectedPaymentModel).Sum(x => x.Paid);

            if (SelectedCard != null && SelectedCard.Paid == 0)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 2;
                    SelectedCard.Paid = _remainTotal > 0 ? Math.Round(_remainTotal, decimalPlace) : 0;
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        #endregion


        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            FillMoneyCommand = new RelayCommand<object>(OnFillMoneyCommandExecute, OnFillMoneyCommandCanExecute);

        }

        /// <summary>
        /// Check total paid is not greater than Total Amount(remaining Total)
        /// </summary>
        /// <param name="paymentDetailModel"></param>
        private bool ValidatePaid(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            decimal paidWithCard = SelectedPaymentModel.PaymentCardCollection.Sum(x => x.Paid);
            //Total Paid without current payment
            decimal totalPaid = PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId != SelectedPaymentModel.PaymentMethodId).Sum(x => x.Paid);
            if (paidWithCard + totalPaid > PaymentModel.TotalAmount)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Calculate Tip Amount When Paid Changed
        /// </summary>
        /// <param name="paymentDetailModel"></param>
        private void CalcTipAmount(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            if (IsAllowTip)
                paymentDetailModel.Tip = (paymentDetailModel.Paid * Define.CONFIGURATION.TipPercent) / 100;
            else
                paymentDetailModel.Tip = 0;
        }

        private void ValidateCouponCard(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            int cardTypeId = Convert.ToInt32(paymentDetailModel.PaymentMethodId);
            if (!string.IsNullOrWhiteSpace(paymentDetailModel.Reference))
            {
                IQueryable<base_CardManagement> query = _cardManagementRepository.GetIQueryable(x => x.CardTypeId.Equals(cardTypeId) && !x.IsPurged && x.CardNumber.Equals(paymentDetailModel.Reference));
                if (query.Any())
                {
                    paymentDetailModel.CouponCardModel = new base_CardManagementModel(query.FirstOrDefault());
                    paymentDetailModel.IsCardValid = true;
                }
                else
                {
                    paymentDetailModel.IsCardValid = false;
                }
            }
            else
            {
                paymentDetailModel.IsCardValid = true;
                paymentDetailModel.CouponCardModel = null;
            }

        }

        private bool CheckRemainCard(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            if (paymentDetailModel.CouponCardModel != null && paymentDetailModel.CouponCardModel.RemainingAmount >= paymentDetailModel.Paid)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region PropertyChanged
        private void PaymentDetailModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base_ResourcePaymentDetailModel paymentDetailModel = sender as base_ResourcePaymentDetailModel;
            switch (e.PropertyName)
            {
                case "Paid":
                    paymentDetailModel.IsValid = ValidatePaid(paymentDetailModel);
                    //Caclculate Tip
                    CalcTipAmount(paymentDetailModel);
                    break;
                case "GiftCardNo":

                    break;


            }
        }


        #endregion
    }
}