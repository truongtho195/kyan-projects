using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PurchaseOrderPaymentViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand AcceptedPaymentCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        //Respository
        private base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();

        public bool SavePayment { get; set; }

        private bool _isInitialData = false;
        private decimal _remainTotal = 0;

        private decimal _balance = 0;
        public base_PurchaseOrderModel PurchaseOrderModel
        {
            get;
            set;
        }

        #endregion

        #region Constructors
        public PurchaseOrderPaymentViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }
        public PurchaseOrderPaymentViewModel(base_PurchaseOrderModel purchaseOrderModel, decimal balance, decimal lastPayment = 0)
            : this()
        {
            PurchaseOrderModel = purchaseOrderModel;
            LastPayment = lastPayment;
            int decimailPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
            _balance = Math.Round(balance, decimailPlace);
            InitialPaymentMethod(MarkType.PurchaseOrder.ToDescription(), purchaseOrderModel.Resource.ToString(), purchaseOrderModel.PurchaseOrderNo, purchaseOrderModel.Total, _balance);
        }

        #endregion

        #region Properties

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

        #region Total
        private decimal _total;
        /// <summary>
        /// Gets or sets the Total.
        /// </summary>
        public decimal Total
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

            return PaymentModel.IsDirty && ((PaymentModel.TotalAmount <= 0 && _balance <= PurchaseOrderModel.Paid) ||
                       (PaymentModel.TotalAmount > 0 && PaymentModel.TotalPaid > 0));
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
            foreach (base_ResourcePaymentDetailModel paymentDetailModel in PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId.Equals((short)PaymentMethod.CreditCard) || x.PaymentMethodId.Equals((short)PaymentMethod.GiftCard)).ToList())
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
        /// <para>Open Popup Payment by card </para>
        /// </summary>
        private void OnOpenPaymentCardViewCommandExecute(object param)
        {
            if (SelectedPaymentDetail.PaymentMethodId == (short)PaymentMethod.CreditCard || SelectedPaymentDetail.IsCard)
            {
                if (SelectedPaymentDetail.PaymentMethodId == (short)PaymentMethod.CreditCard)
                {
                    PaymentCardViewModel paymentCardViewModel = new PaymentCardViewModel(SelectedPaymentDetail, PaymentModel);
                    bool? result = _dialogService.ShowDialog<PaymentCardView>(_ownerViewModel, paymentCardViewModel, "Credit Card");
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
                else
                {
                    string title = SelectedPaymentDetail.PaymentMethodId == (short)PaymentMethod.GiftCard ? "Gift Card" : "Gift Certification";

                    CardPaymentViewModel cardPaymentViewModel = new CardPaymentViewModel(SelectedPaymentDetail, PaymentModel);
                    bool? result = _dialogService.ShowDialog<CardPaymentView>(_ownerViewModel, cardPaymentViewModel, title);
                    if (result == true)
                    {
                        SelectedPaymentDetail.CouponCardModel = cardPaymentViewModel.PaymentMethodModel.CouponCardModel;
                        SelectedPaymentDetail.Paid = cardPaymentViewModel.PaymentMethodModel.Paid;
                        SelectedPaymentDetail.Reference = cardPaymentViewModel.PaymentMethodModel.Reference;
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
            if (PaymentModel != null && PaymentModel.PaymentDetailCollection != null)
                _remainTotal = PaymentModel.TotalAmount - PaymentModel.PaymentDetailCollection.Where(x => x != SelectedPaymentDetail).Sum(x => x.Paid);
            if (SelectedPaymentDetail != null && SelectedPaymentDetail.Paid == 0 && !SelectedPaymentDetail.IsCard)
            {

                int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 2;
                SelectedPaymentDetail.Paid = _remainTotal > 0 ? Math.Round(_remainTotal, decimalPlace) : 0;
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

        private void InitialPaymentMethod(string remark, string docResource, string docNumber, decimal totalAmount, decimal balance)
        {
            _isInitialData = true;

            PaymentModel = new base_ResourcePaymentModel()
            {
                DocumentResource = docResource,
                DocumentNo = docNumber,
                DateCreated = DateTime.Now,
                UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                Resource = Guid.NewGuid(),
                IsDirty = false

            };

            PaymentModel.Mark = remark;
            PaymentModel.TotalAmount = balance;
            PaymentModel.CalcBalance();
            PaymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>();

            if (Define.CONFIGURATION.AcceptedPaymentMethod.HasValue)
            {
                foreach (ComboItem paymentMethod in Common.PaymentMethods.Where(x => !x.Islocked))
                {
                    if (paymentMethod.Value.Is(PaymentMethod.GiftCard))
                    {
                        continue;
                    }

                    //Check payment methods accepted
                    if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has((int)paymentMethod.Value))
                    {
                        base_ResourcePaymentDetailModel paymentDetailModel = new base_ResourcePaymentDetailModel();
                        paymentDetailModel.PaymentType = "P";
                        paymentDetailModel.PaymentMethodId = paymentMethod.Value;
                        paymentDetailModel.ResourcePaymentId = PaymentModel.Id;
                        paymentDetailModel.PaymentMethod = paymentMethod.Text;
                        //[Check again]
                        paymentDetailModel.IsCard = false;

                        paymentDetailModel.Tip = 0;
                        paymentDetailModel.GiftCardNo = string.Empty;
                        //[Check again]
                        paymentDetailModel.IsCreditCard = paymentMethod.Value == (short)PaymentMethod.CreditCard ? true : false;//Show coloumn Tip when creditcard
                        //[Check again]
                        paymentDetailModel.IsPaymentCardMethod = (paymentMethod.Value == (short)PaymentMethod.CreditCard || paymentMethod.Value == (short)PaymentMethod.GiftCard || paymentMethod.Value == (short)PaymentMethod.GiftCertificate) ? true : false;
                        paymentDetailModel.IsDirty = false;
                        paymentDetailModel.IsNew = false;
                        paymentDetailModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(paymentDetailModel_PropertyChanged);
                        paymentDetailModel.PaymentCardCollection = new CollectionBase<base_ResourcePaymentDetailModel>();
                        //Add Card with Credit & GiftCard
                        //Check CreditCard
                        if (paymentMethod.Value == (short)PaymentMethod.CreditCard)
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
                                    paymentCardModel.PaymentMethod = paymentCard.Text;//paymentMethod.Text;
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
                        else if (paymentMethod.Value == (short)PaymentMethod.GiftCard || paymentMethod.Value == (short)PaymentMethod.GiftCertificate)//Giftcard
                        {
                            paymentDetailModel.EnableRow = false;
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

        /// <summary>
        /// Check total paid is not greater than Total Amount(remaining Total)
        /// </summary>
        /// <param name="paymentDetailModel"></param>
        private void ValidatePaid(base_ResourcePaymentDetailModel paymentDetailModel)
        {
            if (PaymentModel.TotalPaid > PaymentModel.TotalAmount)
                paymentDetailModel.IsValid = false;
            else
                paymentDetailModel.IsValid = true;
        }
        #endregion

        #region PropertyChanged
        void paymentDetailModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isInitialData)
                return;
            base_ResourcePaymentDetailModel paymentDetailModel = sender as base_ResourcePaymentDetailModel;
            switch (e.PropertyName)
            {
                case "Paid":
                    PaymentModel.TotalPaid = PaymentModel.PaymentDetailCollection.Sum(x => x.Paid);
                    PaymentModel.CalcChange();
                    PaymentModel.CalcBalance();
                    ValidatePaid(paymentDetailModel);
                    PaymentModel.IsHiddenErrorColumn = !PaymentModel.PaymentDetailCollection.Any(x => !x.IsValid);
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