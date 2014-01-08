using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using CPC.Helper;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class VoidLayawayViewModel : ViewModelBase
    {
        #region Define
        public base_SaleOrderModel SaleOrderModel { get; set; }
        #endregion

        #region Constructors
        public VoidLayawayViewModel(base_SaleOrderModel layawayModel, base_LayawayManagerModel layawayManagerModel)
        {
            InitialCommand();
            SaleOrderModel = layawayModel;
            this.LayawayNo = layawayModel.SONumber;
            this.CustomerName = layawayModel.GuestModel.LegalName;
            this.TotalPaid = (layawayModel.Deposit ?? 0) + layawayModel.Paid;
            this.LayawayTotalFee = layawayModel.LTotalFee;
            int purchaseDay = DateTime.Today.Subtract(layawayModel.OrderDate.Value.Date).Days;

            //!layawayManagerModel.IsNoCancelFee:alway calculate Cancel fee
            //Order cancel out of CancelWithinDay
            if (!layawayManagerModel.IsNoCancelFee || (layawayManagerModel.IsNoCancelFee && purchaseDay > Convert.ToInt16(layawayManagerModel.CancelWithinDay)))
            {
                if (layawayManagerModel.CancellationUnit.Is(UnitType.Money))
                {
                    CalculationFee = layawayManagerModel.CancellationFee;
                }
                else if (layawayManagerModel.CancellationUnit.Is(UnitType.Percent))
                {
                    CalculationFee = (layawayModel.Total * layawayManagerModel.CancellationFee) / 100;
                }
            }
            else
            {
                CalculationFee = 0;
            }

            CalcRefundAmount();

        }
        #endregion

        #region Properties


        #region LayawayNo
        private string _layawayNo;
        /// <summary>
        /// Gets or sets the LayawayNo.
        /// </summary>
        public string LayawayNo
        {
            get { return _layawayNo; }
            set
            {
                if (_layawayNo != value)
                {
                    _layawayNo = value;
                    OnPropertyChanged(() => LayawayNo);
                }
            }
        }
        #endregion

        #region CustomerName
        private string _customerName;
        /// <summary>
        /// Gets or sets the CustomerName.
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            set
            {
                if (_customerName != value)
                {
                    _customerName = value;
                    OnPropertyChanged(() => CustomerName);
                }
            }
        }
        #endregion

        #region TotalPaid
        private decimal _totalPaid;
        /// <summary>
        /// Gets or sets the TotalPaid.
        /// </summary>
        public decimal TotalPaid
        {
            get { return _totalPaid; }
            set
            {
                if (_totalPaid != value)
                {
                    _totalPaid = value;
                    OnPropertyChanged(() => TotalPaid);
                }
            }
        }
        #endregion

        #region CalculationFee
        private decimal _calculationFee;
        /// <summary>
        /// Gets or sets the CalculationFee.
        /// </summary>
        public decimal CalculationFee
        {
            get { return _calculationFee; }
            set
            {
                if (_calculationFee != value)
                {
                    _calculationFee = value;
                    OnPropertyChanged(() => CalculationFee);
                }
            }
        }
        #endregion

        #region RefundAmount
        private decimal _refundAmount;
        /// <summary>
        /// Gets or sets the RefundAmount.
        /// </summary>
        public decimal RefundAmount
        {
            get { return _refundAmount; }
            set
            {
                if (_refundAmount != value)
                {
                    _refundAmount = value;
                    OnPropertyChanged(() => RefundAmount);
                }
            }
        }
        #endregion


        #region LayawayTotalFee
        private decimal _laywayTotalFee;
        /// <summary>
        /// Gets or sets the LayawayTotalFee.
        /// </summary>
        public decimal LayawayTotalFee
        {
            get { return _laywayTotalFee; }
            set
            {
                if (_laywayTotalFee != value)
                {
                    _laywayTotalFee = value;
                    OnPropertyChanged(() => LayawayTotalFee);
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
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
         
            MessageBoxResult result = MessageBoxResult.Yes;

            PaymentModel.TotalPaid = RefundAmount * -1;
            if (RefundAmount < 0)//need to payment more
            {
                //Payment With Cash
                base_ResourcePaymentDetailModel paymentDetailModel = new base_ResourcePaymentDetailModel();
                paymentDetailModel.PaymentType = "P";
                paymentDetailModel.PaymentMethodId = (short)PaymentMethod.Cash;
                paymentDetailModel.PaymentMethod = Common.PaymentMethods.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals((short)PaymentMethod.Cash)).Text;
                paymentDetailModel.CardType = 0;
                paymentDetailModel.Paid = PaymentModel.TotalPaid;
                paymentDetailModel.Change = 0;
                paymentDetailModel.Tip = 0;
                PaymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>();
                
                PaymentModel.PaymentDetailCollection.Add(paymentDetailModel);
                string paidString = string.Format(Define.ConverterCulture, Define.CurrencyFormat, (RefundAmount * -1));
                result = Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("You need to pay {0}",paidString)  , Language.GetMsg("SO_Title_VoidBill"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

            }
            else if (RefundAmount > 0)
            {
                string paidString = string.Format(Define.ConverterCulture, Define.CurrencyFormat, RefundAmount);
                result = Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("Are you sure to refunded with {0}", paidString), Language.GetMsg("SO_Title_VoidBill"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            }

            if (result.Equals(MessageBoxResult.Yes))
            {
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

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private void CalcRefundAmount()
        {
            this.RefundAmount = this.TotalPaid - CalculationFee - LayawayTotalFee;
        }
        #endregion

        #region Public Methods
        #endregion
    }


}
