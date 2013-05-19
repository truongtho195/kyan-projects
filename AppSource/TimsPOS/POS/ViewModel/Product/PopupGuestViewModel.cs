using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;
using System.Linq;
using CPC.POS.Database;
using CPC.POS.View;

namespace CPC.POS.ViewModel
{
    public class PopupGuestViewModel : ViewModelBase
    {
        #region Fields

        private MarkType _markType = MarkType.Vendor;
        private AddressType _addressType = AddressType.Home;
        private short? _guestTypeId = null;

        #endregion

        #region Contructors

        /// <summary>
        /// Initialize PopupGuestViewModel with MarkType is Vendor and AddressType is Home.
        /// </summary>
        public PopupGuestViewModel()
        {
            _ownerViewModel = this;
            InitializeData();
        }

        /// <summary>
        /// Initialize PopupGuestViewModel with specify MarkType and AddressType.
        /// </summary>
        public PopupGuestViewModel(MarkType markType, AddressType addressType, short? guestTypeId = null)
        {
            _ownerViewModel = this;
            _markType = markType;
            _addressType = addressType;
            _guestTypeId = guestTypeId;
            InitializeData();
        }

        #endregion

        #region Properties

        #region NewItem

        private base_GuestModel _newItem;
        /// <summary>
        /// Gets a new item if exists.
        /// </summary>
        public base_GuestModel NewItem
        {
            get
            {
                return _newItem;
            }
            private set
            {
                if (_newItem != value)
                {
                    _newItem = value;
                    OnPropertyChanged(() => NewItem);
                }
            }
        }

        #endregion

        #region AddressTypeCollection

        private AddressTypeCollection _addressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection AddressTypeCollection
        {
            get
            {
                return _addressTypeCollection;
            }
            set
            {
                if (_addressTypeCollection != value)
                {
                    _addressTypeCollection = value;
                    OnPropertyChanged(() => AddressTypeCollection);
                }
            }
        }

        #endregion

        #region EmployeeCollection

        private CollectionBase<base_GuestModel> _employeeCollection;
        /// <summary>
        /// Gets or sets the EmployeeCollection.
        /// </summary>
        public CollectionBase<base_GuestModel> EmployeeCollection
        {
            get
            {
                return _employeeCollection;
            }
            set
            {
                if (_employeeCollection != value)
                {
                    _employeeCollection = value;
                    OnPropertyChanged(() => EmployeeCollection);
                }
            }
        }

        #endregion

        #region CommissionVisibility

        private Visibility _commissionVisibility = Visibility.Collapsed;
        /// <summary>
        /// Gets or sets CommissionVisibility.
        /// </summary>
        public Visibility CommissionVisibility
        {
            get
            {
                return _commissionVisibility;
            }
            set
            {
                if (_commissionVisibility != value)
                {
                    _commissionVisibility = value;
                    OnPropertyChanged(() => CommissionVisibility);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' button clicked, command will executes.
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
        /// When 'Cancel' button clicked, command will executes.
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

        /// <summary>
        /// Determine SaveExecute method can execute or not.
        /// </summary>
        /// <returns>True is execute.</returns>
        private bool CanSaveExecute()
        {
            if (_newItem == null || _newItem.HasError || _newItem.AddressControlCollection.IsErrorData)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CancelExecute

        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Save

        /// <summary>
        /// Save department, category, brand.
        /// </summary>
        private void Save()
        {
            try
            {
                base_GuestRepository guestRepository = new base_GuestRepository();

                // Try get guest.
                base_Guest guest = guestRepository.Get(x => !x.IsPurged && x.Mark == _newItem.Mark &&
                    (x.Phone1 == _newItem.Phone1 || (x.Email != null && _newItem.Email != null && x.Email.ToLower() == _newItem.Email.ToLower())));

                if (guest != null)
                {
                    MessageBoxResult resultMsg = MessageBox.Show("This customer has existed. Please recheck Email or Phone. Do you want to view profiles?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (resultMsg == MessageBoxResult.Yes)
                    {
                        base_GuestModel guestModel = new base_GuestModel(guest);
                        if (guest.base_GuestProfile.Count > 0)
                        {
                            guestModel.PersonalInfoModel = new base_GuestProfileModel(guest.base_GuestProfile.FirstOrDefault());
                        }
                        else
                        {
                            guestModel.PersonalInfoModel = new base_GuestProfileModel();
                        }
                        ViewProfileViewModel viewProfileViewModel = new ViewProfileViewModel();
                        viewProfileViewModel.GuestModel = guestModel;
                        _dialogService.ShowDialog<ViewProfile>(_ownerViewModel, viewProfileViewModel, "View Profile");
                    }
                }
                else
                {
                    // Save.
                    DateTime now = DateTime.Now;
                    _newItem.DateCreated = now;
                    if (Define.USER != null)
                    {
                        _newItem.UserCreated = Define.USER.LoginName;
                    }
                    _newItem.AddressCollection.Clear();
                    _newItem.base_Guest.base_GuestAddress.Clear();
                    base_GuestAddressModel guestAddress;
                    foreach (AddressControlModel item in _newItem.AddressControlCollection)
                    {
                        guestAddress = new base_GuestAddressModel();
                        guestAddress.ToModel(item);
                        guestAddress.ToEntity();
                        _newItem.AddressCollection.Add(guestAddress);
                        _newItem.base_Guest.base_GuestAddress.Add(guestAddress.base_GuestAddress);
                    }
                    _newItem.ToEntity();
                    guestRepository.Add(_newItem.base_Guest);
                    guestRepository.Commit();
                    _newItem.Id = _newItem.base_Guest.Id;

                    foreach (base_GuestAddressModel item in _newItem.AddressCollection)
                    {
                        item.Id = item.base_GuestAddress.Id;
                        item.IsNew = false;
                        item.IsDirty = false;
                    }
                    _newItem.IsNew = false;
                    _newItem.IsDirty = false;

                    FindOwnerWindow(this).DialogResult = true;
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            NewItem = null;
            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

        #region InitializeData

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void InitializeData()
        {
            try
            {
                AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection
                {
                    new AddressTypeModel { ID = 0, Name = "Home" },
                    new AddressTypeModel { ID = 1, Name = "Business" },
                    new AddressTypeModel { ID = 2, Name = "Billing" },
                    new AddressTypeModel { ID = 3, Name = "Shipping" }
                };


                NewItem = new base_GuestModel();
                _newItem.AddressCollection = new ObservableCollection<base_GuestAddressModel>();
                _newItem.AddressControlCollection = new AddressControlCollection
                {
                    new AddressControlModel{ AddressTypeID = (int)_addressType, IsDefault = true, IsNew = true, IsDirty = false}
                };

                _newItem.Mark = _markType.ToDescription();
                _newItem.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
                _newItem.Resource = Guid.NewGuid();
                _newItem.PositionId = 0;
                _newItem.GuestTypeId = _guestTypeId;
                _newItem.IsPrimary = false;
                _newItem.IsActived = true;
                _newItem.IsNew = true;
                _newItem.IsDirty = false;

                if (_markType == MarkType.Customer)
                {
                    CommissionVisibility = Visibility.Visible;
                    string employeeType = MarkType.Employee.ToDescription();
                    base_GuestRepository guestRepository = new base_GuestRepository();
                    EmployeeCollection = new CollectionBase<base_GuestModel>(guestRepository.GetAll(x =>
                        x.IsActived && !x.IsPurged && x.Mark == employeeType).Select(x => new base_GuestModel(x)));
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
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
