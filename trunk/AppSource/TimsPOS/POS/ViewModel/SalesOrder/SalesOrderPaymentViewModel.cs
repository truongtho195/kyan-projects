using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.View;

namespace CPC.POS.ViewModel
{
    class SalesOrderPaymenViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand AcceptedPaymentCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        //Respository
        private base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();

        public bool SavePayment { get; set; }

        private bool _isInitialData = false;
        private decimal _remainTotal = 0;
        #endregion

        #region Constructors
        public SalesOrderPaymenViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }
        public SalesOrderPaymenViewModel(base_SaleOrderModel saleOrderModel, decimal balance, decimal totalDeposit = 0, decimal lastPayment = 0, bool isPaidFull = false)
            : this()
        {
            IsPaidFull = isPaidFull;
            TotalDiscount = saleOrderModel.DiscountAmount;
            TotalDeposit = totalDeposit;
            LastPayment = lastPayment;
            Refunded = saleOrderModel.ReturnModel.TotalRefund;
            this.IsQuotation = false;
            InitialPaymentMethod(MarkType.SaleOrder.ToDescription(), saleOrderModel.Resource.ToString(), saleOrderModel.SONumber, saleOrderModel.Total, balance, saleOrderModel.RewardValueApply);
        }


        public SalesOrderPaymenViewModel(base_SaleOrderModel saleOrderModel, decimal balance,decimal depositTaken)
            : this()
        {
            IsQuotation = true;
            DepositTaken = depositTaken;
            Total = saleOrderModel.Total;
            InitialPaymentMethod(MarkType.SaleOrder.ToDescription(), saleOrderModel.Resource.ToString(), saleOrderModel.SONumber, saleOrderModel.Total, balance, 0);
        }
        #endregion

        #region Properties

        #region TotalDiscount
        private decimal _totalDiscount;
        /// <summary>
        /// Gets or sets the TotalDiscount.
        /// </summary>
        public decimal TotalDiscount
        {
            get { return _totalDiscount; }
            set
            {
                if (_totalDiscount != value)
                {
                    _totalDiscount = value;
                    OnPropertyChanged(() => TotalDiscount);
                }
            }
        }
        #endregion

        #region TotalDeposit
        private decimal _totalDeposit;
        /// <summary>
        /// Gets or sets the TotalDeposit.
        /// </summary>
        public decimal TotalDeposit
        {
            get { return _totalDeposit; }
            set
            {
                if (_totalDeposit != value)
                {
                    _totalDeposit = value;
                    OnPropertyChanged(() => TotalDeposit);
                }
            }
        }
        #endregion

        #region LastPayment
        private decimal _lastPayment;
        /// <summary>
        /// Gets or sets the LastPayment.
        /// </summary>
        public decimal LastPayment
        {
            get { return _lastPayment; }
            set
            {
                if (_lastPayment != value)
                {
                    _lastPayment = value;
                    OnPropertyChanged(() => LastPayment);
                }
            }
        }
        #endregion

        #region DepositTaken
        private decimal _depositTaken;
        /// <summary>
        /// Gets or sets the DepositTaken.
        /// </summary>
        public decimal DepositTaken
        {
            get { return _depositTaken; }
            set
            {
                if (_depositTaken != value)
                {
                    _depositTaken = value;
                    OnPropertyChanged(() => DepositTaken);
                }
            }
        }
        #endregion

        #region Total
        private decimal _total;
        /// <summary>
        /// Gets or sets the Total.
        /// </summary>
        public decimal  Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(() => Total);
                }
            }
        }
        #endregion

        #region PaymentModel
        private base_ResourcePaymentModel _paymentModel;
        /// <summary>
        /// Gets or sets the PaymentModel.
        /// </summary>
        public base_ResourcePaymentModel PaymentModel
        {
            get { return _paymentModel; }
            set
            {
                if (_paymentModel != value)
                {
                    _paymentModel = value;
                    OnPropertyChanged(() => PaymentModel);
                }
            }
        }
        #endregion

        #region SelectedPaymentDetail
        private base_ResourcePaymentDetailModel _selectedPaymentDetail;
        /// <summary>
        /// Gets or sets the SelectedPaymentMethod.
        /// </summary>
        public base_ResourcePaymentDetailModel SelectedPaymentDetail
        {
            get { return _selectedPaymentDetail; }
            set
            {
                if (_selectedPaymentDetail != value)
                {
                    _selectedPaymentDetail = value;
                    OnPropertyChanged(() => SelectedPaymentDetail);
                }
            }
        }
        #endregion

        #region IsDeposit
        private bool _isQuotation;
        /// <summary>
        /// Gets or sets the IsDeposit.
        /// </summary>
        public bool IsQuotation
        {
            get { return _isQuotation; }
            set
            {
                if (_isQuotation != value)
                {
                    _isQuotation = value;
                    OnPropertyChanged(() => IsQuotation);
                }
            }
        }
        #endregion

        #region Refunded
        private decimal _refunded;
        /// <summary>
        /// Gets or sets the Refunded.
        /// </summary>
        public decimal Refunded
        {
            get { return _refunded; }
            set
            {
                if (_refunded != value)
                {
                    _refunded = value;
                    OnPropertyChanged(() => Refunded);
                }
            }
        }
        #endregion


        public bool IsPaidFull { get; set; }

        #endregion

        #region Commands Methods

        #region AcceptedPaymentCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAcceptedPaymentCommandCanExecute()
        {
            if (PaymentModel == null)
                return false;
            if (IsQuotation)
            {
                return PaymentModel.IsDirty && PaymentModel.TotalAmount > 0 && PaymentModel.TotalPaid > 0 && PaymentModel.TotalPaid <= PaymentModel.TotalAmount;
            }
            else
            {
                return PaymentModel.IsDirty;
            }

            //return PaymentModel.IsDirty
            //    && (!IsQuotation && PaymentModel.TotalAmount > 0 && PaymentModel.TotalPaid > 0 && ((IsPaidFull && PaymentModel.Balance == 0) || !IsPaidFull))
            //       || (IsQuotation && PaymentModel.TotalPaid > 0);
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnAcceptedPaymentCommandExecute()
        {
            List<base_ResourcePaymentDetailModel> paymentMethodsList = new List<base_ResourcePaymentDetailModel>();

            //AddPayment Methods
            paymentMethodsList.AddRange(PaymentModel.PaymentDetailCollection.Where(x => x.IsDirty && x.Paid > 0));

            //Add CardCollection
            foreach (base_ResourcePaymentDetailModel paymentDetailModel in PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId.Equals(4) || x.PaymentMethodId.Equals(64)).ToList())
                paymentMethodsList.AddRange(paymentDetailModel.PaymentCardCollection.Where(x => x.IsDirty && x.Paid > 0));

            //Create list payment need to inser to db
            PaymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>(paymentMethodsList);
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

        #region OpenPaymentCardView
        /// <summary>
        /// Gets the OpenPaymentCardView Command.
        /// <summary>

        public RelayCommand<object> OpenPaymentCardViewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OpenPaymentCardView command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenPaymentCardViewCommandCanExecute(object param)
        {
            if (SelectedPaymentDetail == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the OpenPaymentCardView command is executed.
        /// </summary>
        private void OnOpenPaymentCardViewCommandExecute(object param)
        {
            if (SelectedPaymentDetail.PaymentMethodId == 4 || SelectedPaymentDetail.PaymentMethodId == 64)
            {
                string title = SelectedPaymentDetail.PaymentMethodId == 4 ? "Credit Card" : "Gift Card";

                PaymentCardViewModel paymentCardViewModel = new PaymentCardViewModel(SelectedPaymentDetail, PaymentModel.Balance);

                bool? result = _dialogService.ShowDialog<PaymentCardView>(_ownerViewModel, paymentCardViewModel, title);
                if (result == true)
                {
                    SelectedPaymentDetail.Paid = paymentCardViewModel.PaymentMethodModel.Paid;
                    SelectedPaymentDetail.Reference = paymentCardViewModel.PaymentMethodModel.Reference;
                    SelectedPaymentDetail.PaymentCardCollection.Clear();
                    foreach (var item in paymentCardViewModel.PaymentMethodModel.PaymentCardCollection)
                    {
                        SelectedPaymentDetail.PaymentCardCollection.Add(item);
                    }
                }
            }
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
            if (PaymentModel != null && PaymentModel.PaymentDetailCollection!=null)
                _remainTotal = PaymentModel.TotalAmount - PaymentModel.PaymentDetailCollection.Where(x => x != SelectedPaymentDetail).Sum(x => x.Paid);

            if (SelectedPaymentDetail != null && SelectedPaymentDetail.Paid==0 && !SelectedPaymentDetail.IsCard)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SelectedPaymentDetail.Paid = _remainTotal > 0 ? _remainTotal : 0;
                }), System.Windows.Threading.DispatcherPriority.DataBind);
            }
        } 
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            AcceptedPaymentCommand = new RelayCommand(OnAcceptedPaymentCommandExecute, OnAcceptedPaymentCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            OpenPaymentCardViewCommand = new RelayCommand<object>(OnOpenPaymentCardViewCommandExecute, OnOpenPaymentCardViewCommandCanExecute);
            FillMoneyCommand = new RelayCommand<object>(OnFillMoneyCommandExecute, OnFillMoneyCommandCanExecute);
        }

        private void InitialPaymentMethod(string remark, string docResource, string docNumber, decimal totalAmount, decimal balance, decimal rewardValue)
        {
            _isInitialData = true;
            if (IsQuotation)//Using For Quotation
            {
                base_ResourcePayment payment = _paymentRepository.Get(x => x.DocumentNo.Equals(docNumber) && x.DocumentResource.Equals(docResource));
                if (payment != null)
                    PaymentModel = new base_ResourcePaymentModel(payment);
                else
                {
                    PaymentModel = new base_ResourcePaymentModel()
                    {
                        DocumentResource = docResource,
                        DocumentNo = docNumber,
                        IsDeposit = true,
                        DateCreated = DateTime.Now,
                        UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                        Resource = Guid.NewGuid(),
                        IsDirty = false

                    };
                }
            }
            else //Using For Payment
            {
                PaymentModel = new base_ResourcePaymentModel()
                {
                    DocumentResource = docResource,
                    DocumentNo = docNumber,
                    IsDeposit = false,
                    DateCreated = DateTime.Now,
                    UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                    Resource = Guid.NewGuid(),
                    IsDirty = false

                };
            }
            PaymentModel.LastRewardAmount = rewardValue;
            PaymentModel.Mark = remark;
            PaymentModel.TotalAmount = balance;
            PaymentModel.CalcBalance(IsQuotation);
            PaymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>();

            if (Define.CONFIGURATION.AcceptedPaymentMethod.HasValue)
            {
                foreach (ComboItem paymentMethod in Common.PaymentMethods.Where(x => !x.Islocked))
                {
                    //Check payment methods accepted
                    if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has((int)paymentMethod.Value))
                    {
                        base_ResourcePaymentDetailModel paymentDetailModel = new base_ResourcePaymentDetailModel();

                        paymentDetailModel.PaymentType = "P";

                        paymentDetailModel.PaymentMethodId = paymentMethod.Value;
                        paymentDetailModel.ResourcePaymentId = PaymentModel.Id;
                        paymentDetailModel.PaymentMethod = paymentMethod.Text;
                        paymentDetailModel.CardType = 0;
                        paymentDetailModel.Tip = 0;

                        paymentDetailModel.GiftCardNo = string.Empty;
                        paymentDetailModel.IsCreditCard = paymentMethod.Value == 4 ? true : false;//Show coloumn Tip when creditcard
                        paymentDetailModel.IsPaymentCardMethod = (paymentMethod.Value == 4 || paymentMethod.Value == 64) ? true : false;
                        paymentDetailModel.IsCard = false;
                        paymentDetailModel.IsDirty = false;
                        paymentDetailModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(paymentDetailModel_PropertyChanged);
                        paymentDetailModel.PaymentCardCollection = new CollectionBase<base_ResourcePaymentDetailModel>();
                        //Add Card with Credit & GiftCard
                        //Check CreditCard
                        if (paymentMethod.Value == 4)
                        {
                            paymentDetailModel.EnableRow = false;
                            foreach (ComboItem paymentCard in Common.PaymentCardTypes)
                            {
                                //Check CreditCard is accepted in config
                                if (Define.CONFIGURATION.AcceptedCardType.Value.Has((int)paymentCard.Value))
                                {
                                    base_ResourcePaymentDetailModel paymentCardModel = new base_ResourcePaymentDetailModel();
                                    paymentCardModel.PaymentType = "P";
                                    paymentCardModel.ResourcePaymentId = PaymentModel.Id;
                                    paymentCardModel.PaymentMethodId = paymentMethod.Value;
                                    paymentCardModel.PaymentMethod = paymentMethod.Text;
                                    paymentCardModel.IsCard = true;
                                    paymentCardModel.CardType = paymentCard.Value;
                                    paymentCardModel.CardName = paymentCard.Text;
                                    paymentCardModel.Tip = 0;
                                    paymentCardModel.GiftCardNo = string.Empty;
                                    paymentCardModel.IsDirty = false;
                                    paymentCardModel.IsNew = false;
                                    paymentCardModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(paymentCardModel_PropertyChanged);
                                    paymentDetailModel.PaymentCardCollection.Add(paymentCardModel);
                                }
                            }
                        }
                        else if (paymentMethod.Value == 64)//Giftcard
                        {
                            paymentDetailModel.EnableRow = false;
                            foreach (ComboItem paymentCard in Common.GiftCardTypes)
                            {
                                //Check CreditCard is accepted in config
                                if (Define.CONFIGURATION.AcceptedGiftCardMethod.Has((int)paymentCard.Value))
                                {
                                    base_ResourcePaymentDetailModel paymentCardModel = new base_ResourcePaymentDetailModel();
                                    paymentCardModel.PaymentType = "P";
                                    paymentCardModel.ResourcePaymentId = PaymentModel.Id;
                                    paymentCardModel.PaymentMethodId = paymentMethod.Value;
                                    paymentCardModel.PaymentMethod = paymentMethod.Text;
                                    paymentCardModel.CardType = paymentCard.Value;
                                    paymentCardModel.CardName = paymentCard.Text;
                                    paymentCardModel.IsCard = true;
                                    paymentCardModel.Tip = 0;
                                    paymentCardModel.GiftCardNo = string.Empty;
                                    paymentCardModel.IsDirty = false;
                                    paymentCardModel.IsNew = false;
                                    paymentCardModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(paymentCardModel_PropertyChanged);
                                    paymentDetailModel.PaymentCardCollection.Add(paymentCardModel);
                                }
                            }
                        }
                        //Add Payment Method is accepted in config
                        PaymentModel.PaymentDetailCollection.Add(paymentDetailModel);
                    }
                }
                if (balance < 0)
                {
                    PaymentModel.CalcChange();
                    PaymentModel.CalcBalance();
                }
            }
            _isInitialData = false;
        }

        void paymentDetailModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isInitialData)
                return;
            switch (e.PropertyName)
            {
                case "Paid":
                    PaymentModel.TotalPaid = PaymentModel.PaymentDetailCollection.Sum(x => x.Paid);
                    PaymentModel.CalcChange();
                    PaymentModel.CalcBalance(IsQuotation);
                    break;
            }
        }

        void paymentCardModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Paid":
                    //SelectedPaymentDetail.Paid = SelectedPaymentDetail.PaymentCardCollection.Sum(x => x.Paid);
                    break;
            }
        }

        #endregion

    }
}
