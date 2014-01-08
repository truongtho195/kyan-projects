using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using System.Collections.ObjectModel;
using CPC.Helper;
using System.Windows;
using CPC.POS.View;

namespace CPC.POS.ViewModel
{
    class LayawayPaymentViewModel : ViewModelBase
    {
        #region Define
        private decimal _balance = 0;
        private decimal _depositMin = 0;
        #endregion

        #region Constructors
        public LayawayPaymentViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();

            InitialData();
        }

        public LayawayPaymentViewModel(base_SaleOrderModel saleOrderModel, base_LayawayManagerModel layawayManagerModel)
            : this()
        {
            this.SaleOrderModel = saleOrderModel;
            //
            saleOrderModel.RewardAmount = saleOrderModel.Total;
            _balance = saleOrderModel.RewardAmount - (saleOrderModel.Deposit.Value + saleOrderModel.Paid);
            decimal depositTaken = saleOrderModel.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
           
            //Total Paid
            Paid = (saleOrderModel.Deposit ?? 0) + saleOrderModel.Paid;

            //Total Amount of Layaway
            TotalAmount = saleOrderModel.RewardAmount;

            //OutStanding (balance)
            OutStandingAmount = _balance;

            //Set Layaway Manager Setup
            StartDate = layawayManagerModel.StartDate;
            ExpireDate = layawayManagerModel.EndDate;

            //Calculate Deposit Amount(Require Desposit for Layaway)

            //calculate deposit from percent
            decimal depositAmountByPercent = (saleOrderModel.Total * layawayManagerModel.DepositPercent) / 100;

            if (saleOrderModel.Total < layawayManagerModel.DepositAmount)
            {
                _depositMin = depositAmountByPercent - saleOrderModel.Deposit ?? 0;
            }
            else
            {
                //Get what amount more than;
                _depositMin = (depositAmountByPercent > layawayManagerModel.DepositAmount ? depositAmountByPercent : layawayManagerModel.DepositAmount) - saleOrderModel.Deposit ?? 0;
            }
            ComboItem item=PaymentMethodCollection.FirstOrDefault();
            if (item != null)
                PaymentMethod = Convert.ToInt16(item.ObjValue);


            // Show Desposit
            DepositAmount = this.SaleOrderModel.Deposit??0;

        }
        #endregion

        #region Properties

        #region DepositAmount
        private decimal _depositAmount;
        /// <summary>
        /// Gets or sets the DepositAmount.
        /// </summary>
        public decimal DepositAmount
        {
            get { return _depositAmount; }
            set
            {
                if (_depositAmount != value)
                {
                    _depositAmount = value;
                    OnPropertyChanged(() => DepositAmount);
                    //CalulateOutStanding();
                    
                }
            }
        }
        #endregion

        #region AdvancePayment
        private decimal _advancePayment;
        /// <summary>
        /// Gets or sets the AdvancePayment.
        /// </summary>
        public decimal AdvancePayment
        {
            get { return _advancePayment; }
            set
            {
                if (_advancePayment != value)
                {
                    _advancePayment = value;
                    OnPropertyChanged(() => AdvancePayment);

                    //CalulateOutStanding();
                }
            }
        }

       

      
        #endregion

        #region PaymentMethod
        private short _paymentMethod;
        /// <summary>
        /// Gets or sets the PaymentMethod.
        /// </summary>
        public short PaymentMethod
        {
            get { return _paymentMethod; }
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    OnPropertyChanged(() => PaymentMethod);
                }
            }
        }
        #endregion

        #region StartDate
        private DateTime _startDate;
        /// <summary>
        /// Gets or sets the StartDate.
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(() => StartDate);
                }
            }
        }
        #endregion

        #region ExpireDate
        private DateTime _expireDate;
        /// <summary>
        /// Gets or sets the ExpireDate.
        /// </summary>
        public DateTime ExpireDate
        {
            get { return _expireDate; }
            set
            {
                if (_expireDate != value)
                {
                    _expireDate = value;
                    OnPropertyChanged(() => ExpireDate);
                }
            }
        }
        #endregion

        #region TotalAmount
        private decimal _totalAmount;
        /// <summary>
        /// Gets or sets the TotalAmount.
        /// </summary>
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged(() => TotalAmount);
                }
            }
        }
        #endregion

        #region OutStandingAmount
        private decimal _outStandingAmount;
        /// <summary>
        /// Gets or sets the OutStandingAmount.
        /// </summary>
        public decimal OutStandingAmount
        {
            get { return _outStandingAmount; }
            set
            {
                if (_outStandingAmount != value)
                {
                    _outStandingAmount = value;
                    OnPropertyChanged(() => OutStandingAmount);
                }
            }
        }
        #endregion

        #region Paid
        private decimal _paid;
        /// <summary>
        /// Gets or sets the Paid.
        /// </summary>
        public decimal Paid
        {
            get { return _paid; }
            set
            {
                if (_paid != value)
                {
                    _paid = value;
                    OnPropertyChanged(() => Paid);
                }
            }
        }
        #endregion

        #region PaymentMethodCollection
        private ObservableCollection<ComboItem> _paymentMethodCollection = new ObservableCollection<ComboItem>();
        /// <summary>
        /// Gets or sets the PaymentMethodCollection.
        /// </summary>
        public ObservableCollection<ComboItem> PaymentMethodCollection
        {
            get { return _paymentMethodCollection; }
            set
            {
                if (_paymentMethodCollection != value)
                {
                    _paymentMethodCollection = value;
                    OnPropertyChanged(() => PaymentMethodCollection);
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

        #region DepositModel
        private base_ResourcePaymentModel _depositModel;
        /// <summary>
        /// Gets or sets the DepositModel.
        /// </summary>
        public base_ResourcePaymentModel DepositModel
        {
            get { return _depositModel; }
            set
            {
                if (_depositModel != value)
                {
                    _depositModel = value;
                    OnPropertyChanged(() => DepositModel);
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
            return AdvancePayment>0 || DepositAmount >0;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            MessageBoxResult result = MessageBoxResult.Yes;

            if (CalulateOutStanding() < 0)
                result = Xceed.Wpf.Toolkit.MessageBox.Show("total paid is more than total order, do you want to execute processing", Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            else
                result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to execute this transaction", Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

            if (result.Equals(MessageBoxResult.Yes))
            {
                //Deposit Payment
                if (_depositMin > 0 && DepositModel != null)
                {
                    decimal balance = SaleOrderModel.RewardAmount - (SaleOrderModel.Deposit.Value + SaleOrderModel.Paid);
                    DepositModel.Balance = balance - AdvancePayment;
                    DepositModel.DateCreated = DateTime.Now;
                    DepositModel.UserCreated = Define.USER.LoginName;
                    DepositModel.Change = DepositAmount > balance ? DepositAmount - balance : 0;
                    DepositModel.Cashier = (Define.CONFIGURATION.DefaultCashiedUserName ?? false) ? Define.USER.LoginName : string.Empty;
                    DepositModel.Shift = Define.ShiftCode;
                    //Sum paid for saleOrder
                    SaleOrderModel.Deposit += DepositModel.TotalPaid;
                    //Add to collection
                    SaleOrderModel.PaymentCollection.Add(DepositModel);
                }

                if (AdvancePayment > 0 && PaymentModel!=null)
                {
                    decimal balance = SaleOrderModel.RewardAmount - (SaleOrderModel.Deposit.Value + SaleOrderModel.Paid);

                    PaymentModel.Balance = balance - AdvancePayment;
                    PaymentModel.DateCreated = DateTime.Now;
                    PaymentModel.UserCreated = Define.USER.LoginName;
                    PaymentModel.Change = AdvancePayment > balance ? AdvancePayment - balance : 0;
                    PaymentModel.Cashier = (Define.CONFIGURATION.DefaultCashiedUserName ?? false) ? Define.USER.LoginName : string.Empty;
                    PaymentModel.Shift = Define.ShiftCode;
                   
                    //Sum paid for saleOrder
                    SaleOrderModel.Paid += PaymentModel.TotalPaid;
                    //Add to collection
                    SaleOrderModel.PaymentCollection.Add(PaymentModel);
                }

                FindOwnerWindow(_ownerViewModel).DialogResult = true;
            }
        }
        #endregion

        #region CancelCommand
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

        #region DepositCommand
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
            return _depositMin>0;
        }


        /// <summary>
        /// Method to invoke when the Deposit command is executed.
        /// </summary>
        private void OnDepositCommandExecute(object param)
        {
            if(DepositModel==null)
            {
                DepositModel = new base_ResourcePaymentModel()
                {
                    DocumentResource = SaleOrderModel.Resource.ToString(),
                    DocumentNo = SaleOrderModel.SONumber,
                    IsDeposit = true,
                    DateCreated = DateTime.Now,
                    UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                    Resource = Guid.NewGuid(),
                    Mark = MarkType.SaleOrder.ToDescription(),
                    TotalAmount = _balance,
                    PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>(),
                    IsDirty = false
                };
            }

            LayawayPaymentMethodViewModel viewModel = new LayawayPaymentMethodViewModel(OutStandingAmount,DepositModel, _depositMin);
            bool? dialogResult = _dialogService.ShowDialog<LayawayPaymentMethodView>(_ownerViewModel, viewModel, "Layaway Payment Method");
            if (dialogResult ?? false)
            {
                DepositModel = viewModel.PaymentMethodModel;
                DepositAmount = DepositModel.TotalPaid;
            }
        } 
        #endregion

        #region PaymentCommand
        /// <summary>
        /// Gets the Payment Command.
        /// <summary>

        public RelayCommand<object> PaymentCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Payment command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPaymentCommandCanExecute(object param)
        {
            return DepositAmount != 0;
        }


        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary>
        private void OnPaymentCommandExecute(object param)
        {
            if (PaymentModel == null)
            {
                PaymentModel = new base_ResourcePaymentModel()
                {
                    DocumentResource = SaleOrderModel.Resource.ToString(),
                    DocumentNo = SaleOrderModel.SONumber,
                    IsDeposit = false,
                    DateCreated = DateTime.Now,
                    UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                    Resource = Guid.NewGuid(),
                    Mark = MarkType.SaleOrder.ToDescription(),
                    TotalAmount = _balance,
                    PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>(),
                    IsDirty = false
                };
            }
            decimal balance = TotalAmount - DepositAmount - SaleOrderModel.PaymentCollection.Where(x=>!x.IsDeposit.Value).Sum(x=>x.TotalPaid);
            LayawayPaymentMethodViewModel viewModel = new LayawayPaymentMethodViewModel(balance, PaymentModel);
            bool? dialogResult = _dialogService.ShowDialog<LayawayPaymentMethodView>(_ownerViewModel, viewModel, "Layaway Payment Method");
            if (dialogResult ?? false)
            {
                PaymentModel = viewModel.PaymentMethodModel;
                AdvancePayment = PaymentModel.TotalPaid;
            }
        } 
        #endregion
        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            DepositCommand = new RelayCommand<object>(OnDepositCommandExecute, OnDepositCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);
        }

        private void InitialData()
        {
            PaymentMethodCollection.Clear();
            if (Define.CONFIGURATION.AcceptedPaymentMethod.HasValue)
            {
              int payemntAccepted = Define.CONFIGURATION.AcceptedPaymentMethod??0;
              IEnumerable<ComboItem> paymentMethods = Common.PaymentMethods.Where(x => !x.Islocked && payemntAccepted.Has(Convert.ToInt32(x.ObjValue)));
              if (paymentMethods != null)
              {
                  PaymentMethodCollection = new ObservableCollection<ComboItem>(paymentMethods);
              }
            }
        }


        private decimal CalulateOutStanding()
        {
            decimal paid = ((this.SaleOrderModel.Deposit ?? 0) + this.SaleOrderModel.Paid) + this.AdvancePayment;
            return this.TotalAmount - paid;
        }
        #endregion

        #region Public Methods

        #endregion
    }


}
