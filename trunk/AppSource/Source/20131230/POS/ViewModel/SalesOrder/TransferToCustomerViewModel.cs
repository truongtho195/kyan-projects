using System;
using System.Linq;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class TransferToCustomerViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public TransferToCustomerViewModel(base_CardManagementModel cardManagementModel)
        {
            SelectedCardManagement = cardManagementModel;
            Initialize();
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

        #region CustomerList

        private CollectionBase<base_GuestModel> _customerList;
        /// <summary>
        /// Gets or sets customer list.
        /// </summary>
        public CollectionBase<base_GuestModel> CustomerList
        {
            get
            {
                return _customerList;
            }
            set
            {
                if (_customerList != value)
                {
                    _customerList = value;
                    OnPropertyChanged(() => CustomerList);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// Save.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(SaveExecute, CanSaveExecute);
                }
                return _saveCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// Cancel.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SaveExecute

        /// <summary>
        /// Save.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        private bool CanSaveExecute()
        {
            return _selectedCardManagement.IsDirty;
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            try
            {
                base_GuestRepository guestRepository = new base_GuestRepository();
                string customerMark = MarkType.Customer.ToDescription();
                CustomerList = new CollectionBase<base_GuestModel>(guestRepository.GetAll(x =>
                    !x.IsPurged && x.IsActived && x.Mark == customerMark).Select(x => new base_GuestModel(x, false)));
                CustomerList.Insert(0, new base_GuestModel());
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save.
        /// </summary>
        private void Save()
        {
            try
            {
                base_CardManagementRepository cardManagementRepository = new base_CardManagementRepository();
                _selectedCardManagement.ToEntity();
                cardManagementRepository.Commit();
                _selectedCardManagement.CustomerGifted = _customerList.FirstOrDefault(x => x.Resource == new Guid(_selectedCardManagement.GuestGiftedResource)).LegalName;
                _selectedCardManagement.IsDirty = false;
                Close(true);
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            if (_selectedCardManagement.IsDirty)
            {
                _selectedCardManagement.GuestGiftedResource = _selectedCardManagement.base_CardManagement.GuestGiftedResource;
                _selectedCardManagement.IsDirty = false;
            }
            Close(false);
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