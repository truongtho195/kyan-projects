using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;

namespace CPC.POS.ViewModel
{
    class PaymentCardViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        #endregion

        #region Constructors
        public PaymentCardViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        public PaymentCardViewModel(base_ResourcePaymentDetailModel paymentMethod):this()
        {
            PaymentMethodModel = paymentMethod;
            SelectedPaymentModel = PaymentMethodModel.Clone();
            SelectedPaymentModel.PaymentCardCollection.Clear();
           

            foreach (base_ResourcePaymentDetailModel paymentCardModel in PaymentMethodModel.PaymentCardCollection)
            {
                base_ResourcePaymentDetailModel paymentClone = paymentCardModel.Clone();
                paymentClone.Tip = PaymentMethodModel.PaymentMethodId == 4 ? Define.CONFIGURATION.TipPercent : 0;
                SelectedPaymentModel.PaymentCardCollection.Add(paymentClone);
            }
            
            
        }
        #endregion

        #region Properties

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

        /// <summary>
        /// Return value if true
        /// </summary>

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
            return !SelectedPaymentModel.PaymentCardCollection.Any(x=>!string.IsNullOrWhiteSpace(x.Error)) && SelectedPaymentModel.PaymentCardCollection.Any(x=>x.Paid>0);
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

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion

        #region Public Methods
        #endregion
    }


}
