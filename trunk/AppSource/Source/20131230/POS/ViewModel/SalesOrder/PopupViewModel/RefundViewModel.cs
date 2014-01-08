using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    class RefundViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        public bool IsDirty { get; set; }
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        #endregion

        #region Constructors
        public RefundViewModel(base_SaleOrderModel saleOrderModel)
        {
            _ownerViewModel = this;
            IsDirty = false;
            InitialCommand();
            this.SaleOrderModel = saleOrderModel;
            //Get Payment Collection
            this.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>(saleOrderModel.PaymentCollection);
            //Get amount Paid
            this.RemainTotal = this.PaymentCollection.Sum(x => x.TotalPaid - x.Change);

            //Get amount of returned
            int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
            decimal returnValue = SaleOrderModel.ReturnModel.ReturnDetailCollection.Where(x => string.IsNullOrWhiteSpace(x.StoreCardNo) && x.IsReturned).Sum(x => x.Amount + x.VAT - x.RewardRedeem - ((x.Amount * SaleOrderModel.DiscountPercent) / 100)) - SaleOrderModel.ReturnModel.Redeemed;
            TotalReturned = Math.Round(Math.Round(returnValue, decimalPlace) - 0.01M, MidpointRounding.AwayFromZero);

            //Get Recommend Amount refund
            decimal recommendRefunded = RemainTotal - (SaleOrderModel.RewardAmount - TotalReturned);

            //Set money To textbox
            if (recommendRefunded > RemainTotal)
                Refunded = RemainTotal;
            else
                Refunded = recommendRefunded;

            PaymentModel = new base_ResourcePaymentModel()
            {
                IsDeposit = false,
                DocumentResource = SaleOrderModel.Resource.ToString(),
                DocumentNo = SaleOrderModel.SONumber,
                DateCreated = DateTime.Now,
                UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                Resource = Guid.NewGuid(),
                Mark = MarkType.SaleOrder.ToDescription(),
                TotalAmount = SaleOrderModel.SubTotal,
                Shift = Define.ShiftCode
            };
            PaymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>();
        }
        #endregion

        #region Properties

        #region IsLockRefunded
        private bool _isLockRefunded = false;
        /// <summary>
        /// Gets or sets the IsLockRefunded.
        /// </summary>
        public bool IsLockRefunded
        {
            get { return _isLockRefunded; }
            set
            {
                if (_isLockRefunded != value)
                {
                    _isLockRefunded = value;
                    OnPropertyChanged(() => IsLockRefunded);
                }
            }
        }
        #endregion

        #region RemainTotal
        private decimal _remainTotal;
        /// <summary>
        /// Gets or sets the RemainTotal.
        /// </summary>
        public decimal RemainTotal
        {
            get { return _remainTotal; }
            set
            {
                if (_remainTotal != value)
                {
                    _remainTotal = value;
                    OnPropertyChanged(() => RemainTotal);
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
                    IsDirty = true;
                    OnPropertyChanged(() => Refunded);
                }
            }
        }
        #endregion

        public base_SaleOrderModel SaleOrderModel { get; set; }

        #region PaymentCollection
        private ObservableCollection<base_ResourcePaymentModel> _paymentCollection = new ObservableCollection<base_ResourcePaymentModel>();
        /// <summary>
        /// Gets or sets the PaymentCollection.
        /// </summary>
        public ObservableCollection<base_ResourcePaymentModel> PaymentCollection
        {
            get { return _paymentCollection; }
            set
            {
                if (_paymentCollection != value)
                {
                    _paymentCollection = value;
                    OnPropertyChanged(() => PaymentCollection);
                }
            }
        }
        #endregion

        #region TotalReturned
        private decimal _totalReturned;
        /// <summary>
        /// Gets or sets the TotalReturned.
        /// </summary>
        public decimal TotalReturned
        {
            get { return _totalReturned; }
            set
            {
                if (_totalReturned != value)
                {
                    _totalReturned = value;
                    OnPropertyChanged(() => TotalReturned);
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

        #region IsForceFocused

        private bool _isForceFocused = false;
        /// <summary>
        /// Gets or sets the IsForceFocused.
        /// <para>useful when popup call multiple</para>
        /// </summary>
        public bool IsForceFocused
        {
            get { return _isForceFocused; }
            set
            {
                if (_isForceFocused != value)
                {
                    _isForceFocused = value;
                    OnPropertyChanged(() => IsForceFocused);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods

        #region Refund Command
        /// <summary>
        /// Gets the Refund Command.
        /// <summary>

        public RelayCommand<object> RefundCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Refund command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRefundCommandCanExecute(object param)
        {
            if (SaleOrderModel == null)
                return false;
            return IsDirty && string.IsNullOrWhiteSpace(Error) && Refunded > 0;
        }


        /// <summary>
        /// Method to invoke when the Refund command is executed.
        /// </summary>
        private void OnRefundCommandExecute(object param)
        {
            decimal refunded = Refunded;
            //Change to nagative
            if (Refunded > 0)
                refunded = Refunded * -1;
            PaymentModel.TotalPaid = refunded;
            
            var paymentMethod = Common.PaymentMethods.SingleOrDefault(x => x.Value.Equals((short)PaymentMethod.Cash));
            //Default Refund by cash
            base_ResourcePaymentDetailModel paymentDetailModel = new base_ResourcePaymentDetailModel();
            paymentDetailModel.PaymentType = "P";
            paymentDetailModel.PaymentMethodId = paymentMethod.Value;
            paymentDetailModel.ResourcePaymentId = PaymentModel.Id;
            paymentDetailModel.PaymentMethod = paymentMethod.Text;
            paymentDetailModel.Tip = 0;
            paymentDetailModel.GiftCardNo = string.Empty;
            paymentDetailModel.Paid = PaymentModel.TotalPaid;
            paymentDetailModel.Change = 0;
            PaymentModel.PaymentDetailCollection.Add(paymentDetailModel);

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


        #region RefundReasonCommand
        /// <summary>
        /// Gets the RefundReason Command.
        /// <summary>

        public RelayCommand<object> RefundReasonCommand { get; private set; }



        /// <summary>
        /// Method to check whether the RefundReason command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRefundReasonCommandCanExecute(object param)
        {
            return Refunded > 0;
        }


        /// <summary>
        /// Method to invoke when the RefundReason command is executed.
        /// </summary>
        private void OnRefundReasonCommandExecute(object param)
        {
            IsForceFocused = false;
            RefundReasonViewModel viewModel = new RefundReasonViewModel();
            //Set Reason if existed
            if (!string.IsNullOrWhiteSpace(PaymentModel.Remark))
                viewModel.Reason = PaymentModel.Remark;

            bool? dialogResult = _dialogService.ShowDialog<RefundReasonView>(_ownerViewModel, viewModel, "Refund Reason");
            if (dialogResult == true)
            {
                PaymentModel.Remark = viewModel.Reason;
                //Unlock for Cashier enter amount refund
                IsLockRefunded = true;
                IsForceFocused = true;
            }
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            RefundCommand = new RelayCommand<object>(OnRefundCommandExecute, OnRefundCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            RefundReasonCommand = new RelayCommand<object>(OnRefundReasonCommandExecute, OnRefundReasonCommandCanExecute);
        }
        #endregion

        #region IDataErrorInfo Members
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
                    case "Refunded":

                        if (SaleOrderModel.ReturnModel.ReturnDetailCollection != null && SaleOrderModel.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned))
                        {
                            if (Refunded > RemainTotal)
                            {
                                message = "Refuned is not greater than " + RemainTotal;
                            }
                            else if ((RemainTotal - Refunded) < (SaleOrderModel.RewardAmount - TotalReturned)) //Total Refund < TotalPaid > amount return 
                            {
                                message = "Refuned is not greater than " + TotalReturned;
                            }
                        }

                        break;
                }
                return message;
            }
        }
        #endregion
    }
}