using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using CPC.Toolkit.Command;
using System.Collections.ObjectModel;
using CPC.Helper;
using System.Windows;

namespace CPC.POS.ViewModel
{
    public class LayawayPaymentCardViewModel : ViewModelBase
    {
        #region Define



        #endregion

        #region Constructors
        public LayawayPaymentCardViewModel()
        {

            InitialCommand();
        }
        public LayawayPaymentCardViewModel(decimal balance, base_ResourcePaymentDetailModel paymentDetailMethod, base_ResourcePaymentModel paymentModel)
            : this()
        {
            Balance = balance;
            InitialData(paymentDetailMethod);
            PaymentDetailMethod = paymentDetailMethod.Clone();
            PaymentModel = paymentModel;
            IsAllowTip = PaymentDetailMethod.PaymentMethodId.Equals((short)PaymentMethod.CreditCard) && (Define.CONFIGURATION.IsAllowCollectTipCreditCard.HasValue ? Define.CONFIGURATION.IsAllowCollectTipCreditCard.Value : false);
            MappingPaidToPaymentCard(PaymentModel.PaymentDetailCollection, PaymentCardCollection);
        }
        #endregion

        #region Properties
        #region PaymentCardCollection
        private ObservableCollection<base_ResourcePaymentDetailModel> _paymentCardCollection = new ObservableCollection<base_ResourcePaymentDetailModel>();
        /// <summary>
        /// Gets or sets the PaymentMethodCollection.
        /// </summary>
        public ObservableCollection<base_ResourcePaymentDetailModel> PaymentCardCollection
        {
            get { return _paymentCardCollection; }
            set
            {
                if (_paymentCardCollection != value)
                {
                    _paymentCardCollection = value;
                    OnPropertyChanged(() => PaymentCardCollection);
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


        #region PaymentDetailMethod
        private base_ResourcePaymentDetailModel _paymentDetailMethod;
        /// <summary>
        /// Gets or sets the PaymentMethod.
        /// </summary>
        public base_ResourcePaymentDetailModel PaymentDetailMethod
        {
            get { return _paymentDetailMethod; }
            set
            {
                if (_paymentDetailMethod != value)
                {
                    _paymentDetailMethod = value;
                    OnPropertyChanged(() => PaymentDetailMethod);
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


        #region SelectedPaymentDetail
        private base_ResourcePaymentDetailModel _selectedPaymentDetail;
        /// <summary>
        /// Gets or sets the PaymentDetailMethod.
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


        public decimal Balance { get; set; }
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
            if (PaymentDetailMethod == null)
                return false;
            return PaymentCardCollection != null
                && !PaymentCardCollection.Any(x => x.IsError)
                && PaymentCardCollection.Any(x => x.Paid > 0);
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            //Map value from Collection in datagrid to saleORder collection 
            MappingPaidToPaymentCard(PaymentCardCollection, PaymentModel.PaymentDetailCollection, true);

            //Remove Item Paid equal 0
            IEnumerable<base_ResourcePaymentDetailModel> paymentZero = PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId.Equals(PaymentDetailMethod.PaymentMethodId) && x.Paid == 0).ToList();
            foreach (base_ResourcePaymentDetailModel paymentCardDetail in paymentZero)
            {
                PaymentModel.PaymentDetailCollection.Remove(paymentCardDetail);
            }

            //Sum paid in this transaction
            PaymentDetailMethod.Paid = PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId.Equals(PaymentDetailMethod.PaymentMethodId)).Sum(x => x.Paid + x.Tip);
            PaymentDetailMethod.Reference = string.Join(", ", PaymentModel.PaymentDetailCollection.Where(x => x.PaymentMethodId.Equals(PaymentDetailMethod.PaymentMethodId) && x.Paid > 0).Select(x => x.CardName));
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
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
            try
            {
                decimal _remainTotal = 0;
                _remainTotal = Balance - PaymentModel.PaymentDetailCollection.Where(x => !x.PaymentMethodId.Equals(PaymentDetailMethod.PaymentMethodId)).Sum(x => x.Paid) - PaymentCardCollection.Sum(x => x.Paid);

                if (SelectedPaymentDetail != null && SelectedPaymentDetail.Paid == 0)
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 2;
                        SelectedPaymentDetail.Paid = _remainTotal > 0 ? Math.Round(_remainTotal, decimalPlace) : 0;

                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            FillMoneyCommand = new RelayCommand<object>(OnFillMoneyCommandExecute, OnFillMoneyCommandCanExecute);
        }

        private void InitialData(base_ResourcePaymentDetailModel paymentMethodModel)
        {
            if (Define.CONFIGURATION.AcceptedCardType.HasValue)
            {
                int paymentCardTypeAccepted = Define.CONFIGURATION.AcceptedCardType ?? 0;
                IEnumerable<ComboItem> paymentCards = Common.PaymentCardTypes.Where(x => !x.Islocked && paymentCardTypeAccepted.Has(Convert.ToInt32(x.ObjValue)));

                foreach (ComboItem paymentCard in paymentCards)
                {
                    base_ResourcePaymentDetailModel paymentDetailModel = new base_ResourcePaymentDetailModel();

                    paymentDetailModel.PaymentType = "P";
                    //paymentDetailModel.ResourcePaymentId = PaymentModel.Id;
                    paymentDetailModel.PaymentMethodId = paymentMethodModel.PaymentMethodId;
                    paymentDetailModel.PaymentMethod = paymentCard.Text;
                    paymentDetailModel.CardType = paymentCard.Value;
                    paymentDetailModel.CardName = paymentCard.Text;
                    paymentDetailModel.Tip = 0;
                    paymentDetailModel.GiftCardNo = string.Empty;
                    paymentDetailModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PaymentDetailModel_PropertyChanged);
                    paymentDetailModel.IsDirty = false;
                    paymentDetailModel.IsNew = false;
                    PaymentCardCollection.Add(paymentDetailModel);
                }
            }
        }


        /// <summary>
        /// Mapping Collection
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="addItem"></param>
        private void MappingPaidToPaymentCard(ObservableCollection<base_ResourcePaymentDetailModel> from, ObservableCollection<base_ResourcePaymentDetailModel> to, bool addItem = false)
        {
            if (from != null && from.Any())
            {
                foreach (base_ResourcePaymentDetailModel paymentDetailModel in from.Where(x => x.PaymentMethodId.Equals(PaymentDetailMethod.PaymentMethodId) && x.CardType != 0))//Get All Payment Method
                {
                    //Update Paid To Show in datagrid Payment method
                    base_ResourcePaymentDetailModel paymentDetail = to.SingleOrDefault(x => x.PaymentMethodId.Equals(paymentDetailModel.PaymentMethodId) && x.CardType.Equals(paymentDetailModel.CardType));
                    if (paymentDetail != null)
                    {
                        paymentDetail.Paid = paymentDetailModel.Paid;
                        paymentDetail.Reference = paymentDetailModel.Reference;
                    }
                    else
                    {
                        if (addItem && paymentDetailModel.Paid > 0)
                        {
                            to.Add(paymentDetailModel);
                        }
                    }
                }
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
        #endregion

        #region Public Methods
        #endregion

        #region PropertyChanged
        private void PaymentDetailModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base_ResourcePaymentDetailModel paymentDetailModel = sender as base_ResourcePaymentDetailModel;
            switch (e.PropertyName)
            {
                case "Paid":
                    CalcTipAmount(paymentDetailModel);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }


}
