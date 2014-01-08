using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    public class POPaymentHistoryViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public POPaymentHistoryViewModel(string purchaseOrderId)
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            GetPaymentHistory(purchaseOrderId);
        }

        #endregion

        #region Properties

        #region ResourcePaymentDetails

        private CollectionBase<base_ResourcePaymentDetailModel> _resourcePaymentDetails;
        /// <summary>
        /// Gets ResourcePaymentDetails.
        /// </summary>
        public CollectionBase<base_ResourcePaymentDetailModel> ResourcePaymentDetails
        {
            get
            {
                return _resourcePaymentDetails;
            }
            private set
            {
                if (_resourcePaymentDetails != value)
                {
                    _resourcePaymentDetails = value;
                    OnPropertyChanged(() => ResourcePaymentDetails);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region OKCommand

        private ICommand _OKCommand;
        public ICommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new RelayCommand(OKExecute);
                }
                return _OKCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OKExecute

        private void OKExecute()
        {
            Close(true);
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #endregion

        #region Private Methods

        #region GetPaymentHistory

        /// <summary>
        /// Get payment history.
        /// </summary>
        private void GetPaymentHistory(string purchaseOrderId)
        {
            try
            {
                base_ResourcePaymentDetailRepository resourcePaymentDetailRepository = new base_ResourcePaymentDetailRepository();
                ResourcePaymentDetails = new CollectionBase<base_ResourcePaymentDetailModel>(resourcePaymentDetailRepository.GetAll(x =>
                    x.base_ResourcePayment.DocumentResource == purchaseOrderId & x.Paid > 0).Select(x => new base_ResourcePaymentDetailModel(x, false)));
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
