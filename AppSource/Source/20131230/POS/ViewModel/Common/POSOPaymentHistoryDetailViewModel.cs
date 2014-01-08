using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using System.Collections.ObjectModel;
using CPC.POS.Model;
using System.ComponentModel;
using System.Windows.Data;

namespace CPC.POS.ViewModel
{
    /// <summary>
    /// Display payment history detail for PO & SO
    /// </summary>
    public class POSOPaymentHistoryDetailViewModel : ViewModelBase
    {
        #region Define
        private ICollectionView _paymentDetailHistory;
        #endregion

        #region Constructors
        public POSOPaymentHistoryDetailViewModel()
        {
            InitialCommand();
        }
        public POSOPaymentHistoryDetailViewModel(base_ResourcePaymentModel paymentModel):this()
        {
            if (paymentModel.PaymentDetailCollection != null)
            {
                PaymentDetailCollection = paymentModel.PaymentDetailCollection;
            }
            FilterCollection();
        }
        #endregion

        #region Properties

        #region PaymentDetailCollection
        private ObservableCollection<base_ResourcePaymentDetailModel> _paymentDetailCollection = new ObservableCollection<base_ResourcePaymentDetailModel>();
        /// <summary>
        /// Gets or sets the PaymentDetailCollection.
        /// </summary>
        public ObservableCollection<base_ResourcePaymentDetailModel> PaymentDetailCollection
        {
            get { return _paymentDetailCollection; }
            set
            {
                if (_paymentDetailCollection != value)
                {
                    _paymentDetailCollection = value;
                    OnPropertyChanged(() => PaymentDetailCollection);
                }
            }
        }
        #endregion


        #region Totals
        private int _totals;
        /// <summary>
        /// Gets or sets the Totals.
        /// </summary>
        public int  Totals
        {
            get { return _totals; }
            set
            {
                if (_totals != value)
                {
                    _totals = value;
                    OnPropertyChanged(() => Totals);
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
        public RelayCommand OkCommand { get; private set; }




        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
        }

        private void FilterCollection()
        {
            if (_paymentDetailHistory == null)
                _paymentDetailHistory = CollectionViewSource.GetDefaultView(PaymentDetailCollection);

            _paymentDetailHistory.Filter = obj =>
               {
                   bool result = true;
                   base_ResourcePaymentDetailModel paymentDetailModel = obj as base_ResourcePaymentDetailModel;
                   //Get Without Creditcard parent
                   if (paymentDetailModel.PaymentMethodId.Is(PaymentMethod.CreditCard) && paymentDetailModel.CardType.Equals(0))
                   {
                       result = false;
                   }
                   return result;
               };

            Totals = _paymentDetailHistory.OfType<object>().Count();
        }
        #endregion

        #region Public Methods
        #endregion
    }


}
