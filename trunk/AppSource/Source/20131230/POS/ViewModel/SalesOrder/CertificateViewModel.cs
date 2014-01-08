using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using CPC.Helper;
using CPC.POS.Repository;
using System.ComponentModel;
using CPC.POS.Database;
using System.Windows.Input;
using CPC.Toolkit.Command;
using BarcodeLib;
using CPC.POS.View;

namespace CPC.POS.ViewModel
{
    public class CertificateViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Ignore SelectedCardManagement property change.
        /// </summary>
        private bool _ignoreChanging = false;

        private Barcode _barcodeObject;

        private CertificateCardTypeId _cardTypeId;

        #endregion

        #region Constructors

        public CertificateViewModel(CertificateCardTypeId cardTypeId)
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            _cardTypeId = cardTypeId;
            InitData(cardTypeId);
        }

        #endregion

        #region Properties

        #region IsCheckedAll

        /// <summary>
        /// Gets or sets IsCheckedAll.
        /// </summary>
        private bool? _isCheckedAll = false;
        /// <summary>
        /// Gets or sets IsCheckedAll.
        /// </summary>
        public bool? IsCheckedAll
        {
            get
            {
                return _isCheckedAll;
            }
            set
            {
                if (_isCheckedAll != value)
                {
                    _isCheckedAll = value;
                    OnPropertyChanged(() => IsCheckedAll);
                    OnIsCheckedAllChanged();
                }
            }
        }

        #endregion

        #region PaymentMethods

        /// <summary>
        /// Gets or sets PaymentMethods.
        /// </summary>
        private List<ComboItem> _paymentMethods;
        /// <summary>
        /// Gets or sets PaymentMethods.
        /// </summary>
        public List<ComboItem> PaymentMethods
        {
            get
            {
                return _paymentMethods;
            }
            set
            {
                if (_paymentMethods != value)
                {
                    _paymentMethods = value;
                    OnPropertyChanged(() => PaymentMethods);
                }
            }
        }

        #endregion

        #region CardManagements

        /// <summary>
        /// Gets or sets CardManagements.
        /// </summary>
        private CollectionBase<base_CardManagementModel> _cardManagements;
        /// <summary>
        /// Gets or sets CardManagements.
        /// </summary>
        public CollectionBase<base_CardManagementModel> CardManagements
        {
            get
            {
                return _cardManagements;
            }
            set
            {
                if (_cardManagements != value)
                {
                    _cardManagements = value;
                    OnPropertyChanged(() => CardManagements);
                }
            }
        }

        #endregion

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
                    if (OnSelectedCardManagementChanging())
                    {
                        _selectedCardManagement = value;
                        OnPropertyChanged(() => SelectedCardManagement);
                        OnSelectedCardManagementChanged();
                    }
                    else
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (_selectedCardManagement != null)
                            {
                                _selectedCardManagement.IsSelected = true;
                            }
                        }));
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region NewCommand

        private ICommand _newCommand;
        /// <summary>
        /// New card Command.
        /// </summary>
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(NewExecute, CanNewExecute);
                }
                return _newCommand;
            }
        }

        #endregion

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// Save card Command.
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

        #region DeleteCommand

        private ICommand _deleteCommand;
        /// <summary>
        /// Delete card Command.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(DeleteExecute, CanDeleteExecute);
                }
                return _deleteCommand;
            }
        }

        #endregion

        #region PrintCommand

        private ICommand _printCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand(PrintExecute, CanPrintExecute);
                }
                return _printCommand;
            }
        }

        #endregion

        #region TransferCommand

        private ICommand _transferCommand;
        public ICommand TransferCommand
        {
            get
            {
                if (_transferCommand == null)
                {
                    _transferCommand = new RelayCommand(TransferExecute, CanTransferExecute);
                }
                return _transferCommand;
            }
        }

        #endregion

        #region OpenCardDetailViewCommand

        private ICommand _openCardDetailViewCommand;
        /// <summary>
        /// Open CardDetailView
        /// </summary>
        public ICommand OpenCardDetailViewCommand
        {
            get
            {
                if (_openCardDetailViewCommand == null)
                {
                    _openCardDetailViewCommand = new RelayCommand(OpenCardDetailViewExecute, CanOpenCardDetailViewExecute);
                }
                return _openCardDetailViewCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region NewExecute

        /// <summary>
        /// New card.
        /// </summary>
        private void NewExecute()
        {
            New(_cardTypeId);
        }

        #endregion

        #region CanNewExecute

        /// <summary>
        /// Check whether NewExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanNewExecute()
        {
            if (_selectedCardManagement != null && _selectedCardManagement.IsNew)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region SaveExecute

        /// <summary>
        /// Save card.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Check whether SaveExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanSaveExecute()
        {
            if (_selectedCardManagement == null || !_selectedCardManagement.IsDirty || _selectedCardManagement.HasError)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeleteExecute

        /// <summary>
        /// Delete card.
        /// </summary>
        private void DeleteExecute()
        {
            Delete();
        }

        #endregion

        #region CanDeleteExecute

        /// <summary>
        /// Check whether DeleteExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanDeleteExecute()
        {
            if (_cardManagements == null)
            {
                return false;
            }

            return _cardManagements.Any(x => x.IsChecked);
        }

        #endregion

        #region PrintExecute

        private void PrintExecute()
        {
            Print();
        }

        #endregion

        #region CanPrintExecute

        /// <summary>
        /// Check whether PrintExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanPrintExecute()
        {
            return true;
        }

        #endregion

        #region TransferExecute

        private void TransferExecute()
        {
            Transfer();
        }

        #endregion

        #region CanTransferExecute

        /// <summary>
        /// Check whether TransferExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanTransferExecute()
        {
            return _selectedCardManagement != null && !_selectedCardManagement.IsDirty && !_selectedCardManagement.HasError;
        }

        #endregion

        #region OpenCardDetailViewExecute

        /// <summary>
        /// Open CardDetailView
        /// </summary>
        private void OpenCardDetailViewExecute()
        {
            OpenCardDetailView();
        }

        #endregion

        #region CanOpenCardDetailViewExecute

        private bool CanOpenCardDetailViewExecute()
        {
            return _selectedCardManagement != null;
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnSelectedCardManagementChanging

        /// <summary>
        /// Occurs before SelectedCardManagement property change.
        /// </summary>
        private bool OnSelectedCardManagementChanging()
        {
            if (_selectedCardManagement == null || _ignoreChanging)
            {
                return true;
            }

            if (_selectedCardManagement.IsDirty)
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text7, Language.Warning, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                {
                    if (_selectedCardManagement.HasError)
                    {
                        return false;
                    }
                    else
                    {
                        Save();
                        return true;
                    }
                }
                else
                {
                    Restore();
                    return true;
                }
            }

            return true;
        }

        #endregion

        #region OnSelectedCardManagementChanged

        /// <summary>
        /// Occurs after SelectedCardManagement property change.
        /// </summary>
        private void OnSelectedCardManagementChanged()
        {

        }

        #endregion

        #region OnIsCheckedAllChanged

        /// <summary>
        /// Occurs when IsCheckedAll property changed.
        /// </summary>
        private void OnIsCheckedAllChanged()
        {
            if (_cardManagements.Any() && _isCheckedAll.HasValue)
            {
                foreach (base_CardManagementModel item in _cardManagements)
                {
                    if (item.CanEdit)
                    {
                        item.PropertyChanged -= CardManagementPropertyChanged;
                        item.IsChecked = _isCheckedAll.Value;
                        item.PropertyChanged += CardManagementPropertyChanged;
                    }
                }
            }
        }

        #endregion

        #region CardManagementPropertyChanged

        private void CardManagementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                VerifyingIsCheckedAll();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region InitData

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void InitData(CertificateCardTypeId cardTypeId)
        {
            try
            {
                _barcodeObject = new Barcode()
                {
                    IncludeLabel = true
                };

                base_CardManagementRepository cardManagementRepository = new base_CardManagementRepository();
                base_GuestRepository guestRepository = new base_GuestRepository();
                PaymentMethods = new List<ComboItem>(Common.PaymentMethods.Where(x => x.Value == 64 || x.Value == 128));
                IList<base_CardManagement> cardManagementList = cardManagementRepository.GetAll(x => !x.IsPurged);
                CardManagements = new CollectionBase<base_CardManagementModel>();
                base_CardManagementModel cardManagement;
                base_Guest guest;
                Guid itemGuid;
                foreach (base_CardManagement item in cardManagementList)
                {
                    cardManagement = new base_CardManagementModel(item);
                    if (!string.IsNullOrWhiteSpace(item.GuestResourcePurchased))
                    {
                        itemGuid = new Guid(item.GuestResourcePurchased);
                        guest = guestRepository.Get(y => y.Resource.HasValue && y.Resource == itemGuid);
                        if (guest != null)
                        {
                            cardManagement.CustomerPurchased = GetLegalName(guest);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.GuestGiftedResource))
                    {
                        itemGuid = new Guid(item.GuestGiftedResource);
                        guest = guestRepository.Get(y => y.Resource.HasValue && y.Resource == itemGuid);
                        if (guest != null)
                        {
                            cardManagement.CustomerGifted = GetLegalName(guest);
                        }
                    }

                    cardManagement.IsNew = false;
                    cardManagement.IsDirty = false;
                    cardManagement.IsChecked = false;
                    cardManagement.PropertyChanged += CardManagementPropertyChanged;
                    CardManagements.Add(cardManagement);
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            finally
            {
                if (_cardManagements.Any())
                {
                    SelectedCardManagement = _cardManagements.First();
                }
            }
        }

        #endregion

        #region GetLegalName

        /// <summary>
        /// Gets LegalName.
        /// </summary>
        private string GetLegalName(base_Guest guest)
        {
            string legalName = string.Empty;
            if (string.IsNullOrWhiteSpace(guest.FirstName))
                legalName = guest.LastName;
            else if (string.IsNullOrWhiteSpace(guest.LastName))
                legalName = guest.FirstName;
            else
                legalName = string.Format("{0}, {1}", guest.LastName, guest.FirstName);
            return legalName;
        }

        #endregion

        #region VerifyingIsCheckedAll

        /// <summary>
        /// Verifying IsCheckedAll property's value.
        /// </summary>
        private void VerifyingIsCheckedAll()
        {
            int totalChecked = _cardManagements.Count(x => x.IsChecked && x.CanEdit);
            if (totalChecked == 0)
            {
                _isCheckedAll = false;
                OnPropertyChanged(() => IsCheckedAll);
            }
            else if (totalChecked < _cardManagements.Count(x => x.CanEdit))
            {
                _isCheckedAll = null;
                OnPropertyChanged(() => IsCheckedAll);
            }
            else
            {
                _isCheckedAll = true;
                OnPropertyChanged(() => IsCheckedAll);
            }
        }

        #endregion

        #region IncludePropetyChanged

        /// <summary>
        /// Include PropetyChanged event of item in alarm list.  
        /// </summary>
        private void IncludePropetyChanged()
        {
            if (_cardManagements.Any())
            {
                foreach (base_CardManagementModel item in _cardManagements)
                {
                    item.IsChecked = false;
                    if (item.CanEdit)
                    {
                        item.PropertyChanged += CardManagementPropertyChanged;
                    }
                }
            }
        }

        #endregion

        #region ExcludePropetyChanged

        /// <summary>
        /// Exclude PropetyChanged event of item in alarm list.  
        /// </summary>
        private void ExcludePropetyChanged()
        {
            if (_cardManagements.Any())
            {
                foreach (base_CardManagementModel item in _cardManagements)
                {
                    item.PropertyChanged -= CardManagementPropertyChanged;
                }
            }
        }

        #endregion

        #region New

        /// <summary>
        /// Create new card.
        /// </summary>
        private void New(CertificateCardTypeId cardTypeId)
        {
            SelectedCardManagement = null;

            base_CardManagementModel cardManagement = new base_CardManagementModel();
            cardManagement.CardNumber = DateTime.Now.ToString(Define.PurchaseOrderNoFormat);
            _barcodeObject.Encode(TYPE.UPCA, cardManagement.CardNumber, 200, 70);
            cardManagement.ScanCode = _barcodeObject.RawData;
            cardManagement.ScanImg = _barcodeObject.Encoded_Image_Bytes;
            // Backup CardNumber Information.
            cardManagement.base_CardManagement.CardNumber = cardManagement.CardNumber;
            cardManagement.base_CardManagement.ScanCode = cardManagement.ScanCode;
            cardManagement.base_CardManagement.ScanImg = cardManagement.ScanImg;
            cardManagement.CardTypeId = (short)cardTypeId;
            cardManagement.CustomerGifted = "System";
            cardManagement.CustomerPurchased = "System";
            cardManagement.Status = Common.StatusBasic.Any() ? Common.StatusBasic.First().Value : (short)0;

            cardManagement.IsNew = true;
            cardManagement.IsDirty = false;
            SelectedCardManagement = cardManagement;
        }

        #endregion

        #region Save

        /// <summary>
        /// Save card.
        /// </summary>
        private void Save()
        {
            try
            {
                base_CardManagementRepository cardManagementRepository = new base_CardManagementRepository();
                //Check CardNumber.
                if (string.IsNullOrWhiteSpace(_selectedCardManagement.CardNumber))
                {
                    _selectedCardManagement.CardNumber = _selectedCardManagement.base_CardManagement.CardNumber;
                    _selectedCardManagement.ScanCode = _selectedCardManagement.base_CardManagement.ScanCode;
                    _selectedCardManagement.ScanImg = _selectedCardManagement.base_CardManagement.ScanImg;
                }
                else if (string.Compare(_selectedCardManagement.CardNumber, _selectedCardManagement.base_CardManagement.CardNumber, false) != 0)
                {
                    if (_selectedCardManagement.CardNumber.Length < 11 || _selectedCardManagement.CardNumber.Length > 12)
                    {
                        throw new Exception("Card Number length invalid. Length must be 11 or 12");
                    }
                    else
                    {
                        // Generate code again.
                        _barcodeObject.Encode(TYPE.UPCA, _selectedCardManagement.CardNumber, 200, 70);
                        _selectedCardManagement.ScanCode = _barcodeObject.RawData;
                        _selectedCardManagement.ScanImg = _barcodeObject.Encoded_Image_Bytes;
                    }
                }

                if (_selectedCardManagement.IsNew)
                {
                    _selectedCardManagement.UserCreated = Define.USER.LoginName;
                    _selectedCardManagement.DateCreated = DateTime.Now;
                    _selectedCardManagement.ToEntity();
                    cardManagementRepository.Add(_selectedCardManagement.base_CardManagement);
                    cardManagementRepository.Commit();
                    _selectedCardManagement.IsDirty = false;
                    _selectedCardManagement.IsNew = false;

                    _selectedCardManagement.PropertyChanged += CardManagementPropertyChanged;
                    CardManagements.Add(_selectedCardManagement);
                }
                else
                {
                    _selectedCardManagement.ToEntity();
                    cardManagementRepository.Commit();
                    _selectedCardManagement.IsDirty = false;
                }

            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete card.
        /// </summary>
        private void Delete()
        {
            try
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.Warning, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                {
                    _ignoreChanging = true;
                    base_CardManagementRepository cardManagementRepository = new base_CardManagementRepository();
                    List<base_CardManagementModel> deleteList = _cardManagements.Where(x => x.IsChecked && x.CanEdit).ToList();
                    foreach (base_CardManagementModel item in deleteList)
                    {
                        item.IsPurged = true;
                        item.ToEntity();
                        _cardManagements.Remove(item);
                    }
                    cardManagementRepository.Commit();
                    _ignoreChanging = false;
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Print

        /// <summary>
        /// Print.
        /// </summary>
        private void Print()
        {
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            rpt.ShowReport("rptGiftCertificate", "");
        }

        #endregion

        #region Transfer

        /// <summary>
        /// Transfer.
        /// </summary>
        private void Transfer()
        {
            _dialogService.ShowDialog<TransferToCustomerView>(_ownerViewModel, new TransferToCustomerViewModel(_selectedCardManagement), "Transfer To Customer");
        }

        #endregion

        #region Restore

        /// <summary>
        /// Restore
        /// </summary>
        private void Restore()
        {
            if (!_selectedCardManagement.IsNew)
            {
                _selectedCardManagement.Restore();
                _selectedCardManagement.IsDirty = false;
            }
        }

        #endregion

        #region OpenCardDetailView

        /// <summary>
        /// Open CardDetailView
        /// </summary>
        private void OpenCardDetailView()
        {
            _dialogService.ShowDialog<CardDetailView>(_ownerViewModel, new CardDetailViewModel(_selectedCardManagement), "Card Detail");
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
