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
    public class LayawayPaymentMethodViewModel : ViewModelBase
    {
        #region Define

        #endregion

        #region Constructors
        public LayawayPaymentMethodViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
            InitialData();
        }

        public LayawayPaymentMethodViewModel(decimal balance, base_ResourcePaymentModel paymentMethodModel)
            : this()
        {
            Balance = balance;
            MinimumDeposit = 0;
            PaymentMethodModel = paymentMethodModel.Clone();
            MappingPaidToPaymentMethod(paymentMethodModel.PaymentDetailCollection, PaymentMethodCollection);
        }

        public LayawayPaymentMethodViewModel(decimal balance, base_ResourcePaymentModel paymentMethodModel, decimal depositMin)
            : this()
        {
            Balance = balance;
            PaymentMethodModel = paymentMethodModel.Clone();
            MinimumDeposit = depositMin;
            MappingPaidToPaymentMethod(paymentMethodModel.PaymentDetailCollection, PaymentMethodCollection);
        }



        #endregion

        #region Properties

        #region DepositMinVisibility
        private Visibility _depositMinVisibility = Visibility.Collapsed;
        /// <summary>
        /// Gets or sets the DepositMinVisibility.
        /// </summary>
        public Visibility DepositMinVisibility
        {
            get { return _depositMinVisibility; }
            set
            {
                if (_depositMinVisibility != value)
                {
                    _depositMinVisibility = value;
                    OnPropertyChanged(() => DepositMinVisibility);
                }
            }
        }
        #endregion

        #region MinimumDeposit



        private decimal _minimumDeposit;
        /// <summary>
        /// Gets or sets the MinimumDeposit.
        /// </summary>
        public decimal MinimumDeposit
        {
            get { return _minimumDeposit; }
            set
            {
                if (_minimumDeposit != value)
                {
                    _minimumDeposit = value;
                    OnPropertyChanged(() => MinimumDeposit);

                    if (MinimumDeposit == 0)
                        DepositMinVisibility = Visibility.Collapsed;
                    else
                        DepositMinVisibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region PaymentMethodModel
        private base_ResourcePaymentModel _paymentMethodModel;
        /// <summary>
        /// Gets or sets the PaymentMethodModel.
        /// </summary>
        public base_ResourcePaymentModel PaymentMethodModel
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

        #region PaymentMethodCollection
        private ObservableCollection<base_ResourcePaymentDetailModel> _paymentMethodCollection = new ObservableCollection<base_ResourcePaymentDetailModel>();
        /// <summary>
        /// Gets or sets the PaymentMethodCollection.
        /// </summary>
        public ObservableCollection<base_ResourcePaymentDetailModel> PaymentMethodCollection
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

        #region SelectedPaymentDetail
        private base_ResourcePaymentDetailModel _selectedPaymentDetail;
        /// <summary>
        /// Gets or sets the SelectedPaymentDetail.
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
            if (PaymentMethodCollection == null)
                return false;
            return PaymentMethodCollection.Any(x => x.Paid > 0) && PaymentMethodCollection.Sum(x => x.Paid) >= MinimumDeposit;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            //MapValue
            MappingPaidToPaymentMethod(PaymentMethodCollection, PaymentMethodModel.PaymentDetailCollection, true);

            IEnumerable<base_ResourcePaymentDetailModel> paymentZero = PaymentMethodModel.PaymentDetailCollection.Where(x => x.Paid == 0).ToList();
            foreach (base_ResourcePaymentDetailModel paymentCardDetail in paymentZero)
            {
                PaymentMethodModel.PaymentDetailCollection.Remove(paymentCardDetail);
            }
            //Sum of Payment method
            PaymentMethodModel.TotalPaid = PaymentMethodModel.PaymentDetailCollection.Where(x => x.CardType == 0).Sum(x => x.Paid);
            PaymentMethodModel.CalcChange();
            PaymentMethodModel.CalcBalance();
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

        #region OpenPaymentCardViewCommand
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the OpenPaymentCardView command is executed.
        /// </summary>
        private void OnOpenPaymentCardViewCommandExecute(object param)
        {
            if (SelectedPaymentDetail != null && !SelectedPaymentDetail.EnableRow)//is Card
            {
                if (SelectedPaymentDetail.PaymentMethodId == (short)PaymentMethod.CreditCard)
                {
                    decimal balance = Balance - PaymentMethodCollection.Where(x => x.CardType == 0).Sum(x => x.Paid);
                    LayawayPaymentCardViewModel paymentCardViewModel = new LayawayPaymentCardViewModel(balance, SelectedPaymentDetail, PaymentMethodModel);
                    bool? result = _dialogService.ShowDialog<LayawayPaymentCardView>(_ownerViewModel, paymentCardViewModel, "Credit Card");
                    if (result == true)
                    {
                        SelectedPaymentDetail.Paid = paymentCardViewModel.PaymentDetailMethod.Paid;
                        SelectedPaymentDetail.Reference = paymentCardViewModel.PaymentDetailMethod.Reference;
                    }
                }
                else
                {
                    string title = SelectedPaymentDetail.PaymentMethodId == (short)PaymentMethod.GiftCard ? "Gift Card" : "Gift Certification";

                    CardPaymentViewModel cardPaymentViewModel = new CardPaymentViewModel(SelectedPaymentDetail, PaymentMethodModel);
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
            decimal _remainTotal = 0;

            if (SelectedPaymentDetail != null && SelectedPaymentDetail.Paid == 0)
            {
                if (_minimumDeposit > 0)
                    _remainTotal = _minimumDeposit - PaymentMethodCollection.Where(x => x != SelectedPaymentDetail).Sum(x => x.Paid);
                else
                    _remainTotal = Balance - PaymentMethodCollection.Where(x => x != SelectedPaymentDetail).Sum(x => x.Paid);

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 2;
                    SelectedPaymentDetail.Paid = _remainTotal > 0 ? Math.Round(_remainTotal, decimalPlace) : 0;

                }), System.Windows.Threading.DispatcherPriority.Normal);
            }
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            OpenPaymentCardViewCommand = new RelayCommand<object>(OnOpenPaymentCardViewCommandExecute, OnOpenPaymentCardViewCommandCanExecute);
            FillMoneyCommand = new RelayCommand<object>(OnFillMoneyCommandExecute, OnFillMoneyCommandCanExecute);
        }




        private void MappingPaidToPaymentMethod(ObservableCollection<base_ResourcePaymentDetailModel> from, ObservableCollection<base_ResourcePaymentDetailModel> to, bool addItem = false)
        {
            if (from != null && from.Any())
            {
                foreach (base_ResourcePaymentDetailModel paymentDetailModel in from)//Get All Payment Method
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
                        if (addItem)
                        {
                            to.Add(paymentDetailModel);
                        }
                    }
                }
            }
        }
        private void InitialData()
        {
            if (Define.CONFIGURATION.AcceptedPaymentMethod.HasValue)
            {
                int paymentAccepted = Define.CONFIGURATION.AcceptedPaymentMethod ?? 0;
                IEnumerable<ComboItem> paymentMethods = Common.PaymentMethods.Where(x => !x.Islocked && paymentAccepted.Has(Convert.ToInt32(x.ObjValue)));
                foreach (ComboItem paymentMethod in paymentMethods)
                {
                    base_ResourcePaymentDetailModel paymentDetailModel = new base_ResourcePaymentDetailModel();

                    if (paymentMethod.Value == (short)PaymentMethod.CreditCard
                        || paymentMethod.Value == (short)PaymentMethod.GiftCard
                        || paymentMethod.Value == (short)PaymentMethod.GiftCertificate)
                    {
                        paymentDetailModel.EnableRow = false;
                    }
                    paymentDetailModel.PaymentType = "P";
                    paymentDetailModel.PaymentMethodId = paymentMethod.Value;
                    //paymentDetailModel.ResourcePaymentId = PaymentModel.Id;
                    paymentDetailModel.PaymentMethod = paymentMethod.Text;

                    paymentDetailModel.IsDirty = false;
                    paymentDetailModel.IsNew = false;
                    PaymentMethodCollection.Add(paymentDetailModel);
                }
            }
        }
        #endregion

        #region Public Methods


        #endregion
    }


}
