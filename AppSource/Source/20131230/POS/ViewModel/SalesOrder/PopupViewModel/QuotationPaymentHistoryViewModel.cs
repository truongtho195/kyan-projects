using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class QuotationPaymentHistoryViewModel : ViewModelBase
    {
        #region Define
        public enum PopupType
        {
            Deposit = 1,
            Refund = 2,
            Cancel = 3
        }
        #endregion

        #region Constructors
        public QuotationPaymentHistoryViewModel(base_SaleOrderModel saleOrderModel, decimal balance, decimal depositTaken)
        {
            _ownerViewModel = this;
            InitialCommand();
            this.SaleOrderModel = saleOrderModel;
            this.RemainTotal = balance;
            this.DepositTaken = depositTaken;
            this.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>(saleOrderModel.PaymentCollection.Where(x => x.IsDeposit.Value));
            //any payment
            if (saleOrderModel.PaymentCollection.Any(x => !x.IsDeposit.Value))
            {
                DepositUsed = depositTaken * -1;
            }
            DepositBalance = DepositTaken + DepositUsed;

        }
        #endregion

        #region Properties
        /// <summary>
        /// Notify ActionView 
        /// </summary>
        public PopupType ViewActionType { get; set; }

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

        #region DepositUsed
        private decimal _depositeUsed = 0;
        /// <summary>
        /// Gets or sets the DepositUsed.
        /// </summary>
        public decimal DepositUsed
        {
            get { return _depositeUsed; }
            set
            {
                if (_depositeUsed != value)
                {
                    _depositeUsed = value;
                    OnPropertyChanged(() => DepositUsed);
                }
            }
        }
        #endregion

        #region DepositBalance
        private decimal _depositBalance;
        /// <summary>
        /// Gets or sets the DepositBalance.
        /// </summary>
        public decimal DepositBalance
        {
            get { return _depositBalance; }
            set
            {
                if (_depositBalance != value)
                {
                    _depositBalance = value;
                    OnPropertyChanged(() => DepositBalance);
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

        #endregion

        #region Commands Methods

        #region Deposit Command
        /// <summary>
        /// Gets the Deposit Command.
        /// <summary>

        public RelayCommand<object> DepositCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Deposit command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDepositCommandCanExecute(object param)
        {
            if (SaleOrderModel == null)
                return false;
            return !SaleOrderModel.IsConverted && SaleOrderModel.Paid == 0;
        }


        /// <summary>
        /// Method to invoke when the Deposit command is executed.
        /// </summary>
        private void OnDepositCommandExecute(object param)
        {
            ViewActionType = PopupType.Deposit;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

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
            return SaleOrderModel.Deposit > 0 && !SaleOrderModel.IsConverted;
        }


        /// <summary>
        /// Method to invoke when the Refund command is executed.
        /// </summary>
        private void OnRefundCommandExecute(object param)
        {
            ViewActionType = PopupType.Refund;

            string msg = string.Format("Customer is desposit {0} \nDo you want to refund all?", string.Format(Define.ConverterCulture, Define.CurrencyFormat, this.SaleOrderModel.Deposit));
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(msg, "Refund", MessageBoxButton.YesNo, MessageBoxImage.Information,MessageBoxResult.Yes);
            if (result.Equals(MessageBoxResult.Yes))
            {
                if (this.SaleOrderModel.PaymentCollection == null)
                    this.SaleOrderModel.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();
                base_ResourcePaymentModel refundPaymentModel = new base_ResourcePaymentModel()
                {
                    IsDeposit = true,
                    DocumentResource = this.SaleOrderModel.Resource.ToString(),
                    DocumentNo = this.SaleOrderModel.SONumber,
                    DateCreated = DateTime.Now,
                    UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                    Resource = Guid.NewGuid(),
                    TotalAmount = this.SaleOrderModel.SubTotal,
                    TotalPaid = -this.SaleOrderModel.Deposit.Value,
                    Shift = Define.ShiftCode

                };
                if (Define.CONFIGURATION.DefaultCashiedUserName.HasValue && Define.CONFIGURATION.DefaultCashiedUserName.Value)
                    refundPaymentModel.Cashier = Define.USER.LoginName;
                this.SaleOrderModel.PaymentCollection.Add(refundPaymentModel);
                //Collection in QuotationPaymentHistory
                PaymentCollection.Add(refundPaymentModel);
                this.SaleOrderModel.Deposit = this.SaleOrderModel.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
                //Update Value in Popup
                RemainTotal = this.SaleOrderModel.RewardAmount - this.SaleOrderModel.Deposit.Value;
                DepositTaken = this.SaleOrderModel.Deposit.Value;
            }

            //FindOwnerWindow(_ownerViewModel).DialogResult = true;
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
            ViewActionType = PopupType.Cancel;
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            DepositCommand = new RelayCommand<object>(OnDepositCommandExecute, OnDepositCommandCanExecute);
            RefundCommand = new RelayCommand<object>(OnRefundCommandExecute, OnRefundCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion
    }
}