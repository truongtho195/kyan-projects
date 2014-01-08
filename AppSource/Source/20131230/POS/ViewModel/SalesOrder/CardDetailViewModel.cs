using System;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.Helper;
using CPC.POS.Repository;
using CPC.POS.Database;

namespace CPC.POS.ViewModel
{
    public class CardDetailViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public CardDetailViewModel(base_CardManagementModel cardManagementModel)
        {
            SelectedCardManagement = cardManagementModel;
            InitData();
        }

        #endregion

        #region Properties

        #region SelectedCardManagement

        /// <summary>
        /// Gets or sets SelectedCardManagement.
        /// </summary>
        private base_CardManagementModel _selectedCardManagement;
        /// <summary>
        /// Gets or sets SelectedCardManagement.
        /// </summary>
        public base_CardManagementModel SelectedCardManagement
        {
            get
            {
                return _selectedCardManagement;
            }
            set
            {
                if (_selectedCardManagement != value)
                {
                    _selectedCardManagement = value;
                    OnPropertyChanged(() => SelectedCardManagement);
                }
            }
        }

        #endregion

        #region SaleOrders

        private CollectionBase<base_SaleOrderModel> _saleOrders;
        public CollectionBase<base_SaleOrderModel> SaleOrders
        {
            get
            {
                return _saleOrders;
            }
            set
            {
                if (_saleOrders != value)
                {
                    _saleOrders = value;
                    OnPropertyChanged(() => SaleOrders);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region OkCommand

        private ICommand _OKCommand;
        public ICommand OkCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new RelayCommand(OkExecute);
                }
                return _OKCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OkExecute

        private void OkExecute()
        {
            Close(true);
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #endregion

        #region Private Methods

        #region InitData

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void InitData()
        {
            try
            {
                base_SaleOrderDetailRepository saleOrderDetailRepository = new base_SaleOrderDetailRepository();
                base_SaleOrderRepository saleOrderRepository = new base_SaleOrderRepository();
                base_ResourcePaymentDetailRepository resourcePaymentDetailRepository = new base_ResourcePaymentDetailRepository();

                SaleOrders = new CollectionBase<base_SaleOrderModel>(saleOrderDetailRepository.GetAll(x =>
                    x.SerialTracking == _selectedCardManagement.CardNumber).Select(x => new base_SaleOrderModel(x.base_SaleOrder)));

                foreach (base_SaleOrderModel item in _saleOrders)
                {
                    base_ResourcePaymentDetail paymentDetail = resourcePaymentDetailRepository.Get(x =>
                        x.base_ResourcePayment != null &&
                        x.base_ResourcePayment.DocumentNo == item.SONumber &&
                        x.PaymentMethodId == _selectedCardManagement.CardTypeId &&
                        x.Reference == _selectedCardManagement.CardNumber);
                    if (paymentDetail != null)
                    {
                        item.AmountTendered = paymentDetail.Paid;
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Close

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Close(bool result)
        {
            FindOwnerWindow(this).DialogResult = result;
        }

        #endregion

        #endregion

        #region WriteLog

        private void WriteLog(Exception exception)
        {
            _log4net.Error(string.Format("Message: {0}. Source: {1}.", exception.Message, exception.Source));
            if (exception.InnerException != null)
            {
                _log4net.Error(exception.InnerException.ToString());
            }
        }

        #endregion
    }
}
