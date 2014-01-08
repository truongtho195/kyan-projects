using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.POS.View;
using System.Windows.Data;
using CPC.Helper;
using System.Collections.ObjectModel;
using CPC.POS.Repository;

namespace CPC.POS.ViewModel
{
    class VendorSearchViewModel : ViewModelBase
    {
        #region Fields

        CollectionBase<base_GuestModel> _vendorCollectionRoot;

        private base_GuestGroupRepository _guestGroupRepository = new base_GuestGroupRepository();
        #endregion

        #region Constructors

        public VendorSearchViewModel(CollectionBase<base_GuestModel> vendorCollection)
        {
            LoadDynamicData();
            _ownerViewModel = App.Current.MainWindow.DataContext;
            _vendorCollectionRoot = vendorCollection;
            VendorCollection = new CollectionBase<base_GuestModel>(vendorCollection);
            Total = VendorCollection.Count();
        }

        #endregion

        #region Properties

        #region VendorCollection

        private CollectionBase<base_GuestModel> _vendorCollection;
        /// <summary>
        /// Gets or sets VendorCollection.
        /// </summary>
        public CollectionBase<base_GuestModel> VendorCollection
        {
            get
            {
                return _vendorCollection;
            }
            set
            {
                if (_vendorCollection != value)
                {
                    _vendorCollection = value;
                    OnPropertyChanged(() => VendorCollection);
                }
            }
        }

        #endregion

        #region SelectedVendor

        private base_GuestModel _selectedVendor;
        public base_GuestModel SelectedVendor
        {
            get
            {
                return _selectedVendor;
            }
            set
            {
                if (_selectedVendor != value)
                {
                    _selectedVendor = value;
                    OnPropertyChanged(() => SelectedVendor);
                }
            }
        }

        #endregion

        #region GuestGroupCollection
        private ObservableCollection<base_GuestGroupModel> _guestGroupCollection;
        /// <summary>
        /// Gets or sets the GuestGroupCollection.
        /// </summary>
        public ObservableCollection<base_GuestGroupModel> GuestGroupCollection
        {
            get { return _guestGroupCollection; }
            set
            {
                if (_guestGroupCollection != value)
                {
                    _guestGroupCollection = value;
                    OnPropertyChanged(() => GuestGroupCollection);
                }
            }
        }
        #endregion

        #region Key

        private string _key;
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged(() => Key);
                }
            }
        }

        #endregion


        #region Total
        private int _total;
        /// <summary>
        /// Gets or sets the Total.
        /// </summary>
        public int Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(() => Total);
                }
            }
        }
        #endregion


        #endregion

        #region Command Properties

        #region SearchCommand

        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand(SearchExecute);
                }
                return _searchCommand;
            }
        }

        #endregion

        #region AddVendorCommand

        private ICommand _addVendorCommand;
        /// <summary>
        /// When 'Add New' Button in ComboBox clicked, AddVendorCommand will executes.
        /// </summary>
        public ICommand AddVendorCommand
        {
            get
            {
                if (_addVendorCommand == null)
                {
                    _addVendorCommand = new RelayCommand(AddVendorExecute);
                }
                return _addVendorCommand;
            }
        }

        #endregion

        #region OpenVendorViewCommand

        private ICommand _openVendorViewCommand;
        public ICommand OpenVendorViewCommand
        {
            get
            {
                if (_openVendorViewCommand == null)
                {
                    _openVendorViewCommand = new RelayCommand(OpenVendorViewExecute);
                }
                return _openVendorViewCommand;
            }
        }

        #endregion

        #region SelectCommand

        private ICommand _selectCommand;
        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(SelectExecute, CanSelectExecute);
                }
                return _selectCommand;
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

        #region SearchExecute

        private void SearchExecute()
        {
            Search();
        }

        #endregion

        #region AddVendorExecute

        /// <summary>
        /// Add new vendor.
        /// </summary>
        private void AddVendorExecute()
        {
            AddVendor();
        }

        #endregion

        #region OpenVendorViewExecute

        private void OpenVendorViewExecute()
        {
            OpenVendorView();
        }

        #endregion

        #region SelectExecute

        private void SelectExecute()
        {
            Select();
        }

        #endregion

        #region CanSelectExecute

        private bool CanSelectExecute()
        {
            return _selectedVendor != null;
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

        #region Search

        /// <summary>
        /// Search.
        /// </summary>
        private void Search()
        {
            ListCollectionView vendorCollectionView = CollectionViewSource.GetDefaultView(VendorCollection) as ListCollectionView;
            if (vendorCollectionView != null)
            {
                vendorCollectionView.Filter = (item) =>
                {
                    bool result = false;
                    base_GuestModel vendor = item as base_GuestModel;
                    if (vendor == null)
                    {
                        result = false;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(_key))
                        {
                            result = true;
                        }
                        else
                        {
                            //Search Customer By Guest No
                            result |= vendor.GuestNo.Contains(_key.ToLower());

                            //Search Customer By status
                            IEnumerable<bool> statusList = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(_key.ToLower())).Select(x => Convert.ToInt32(x.ObjValue).Is(StatusBasic.Active));
                            if (statusList.Any())
                                result |= statusList.Contains(vendor.IsActived);

                            //Search Customer By First Name
                            result |= !string.IsNullOrWhiteSpace(vendor.FirstName) && vendor.FirstName.ToLower().Contains(_key.ToLower());

                            //Search Customer By Last Name
                            result |= !string.IsNullOrWhiteSpace(vendor.LastName) && vendor.LastName.ToLower().Contains(_key.ToLower());

                            //Serach Customer By Company
                            result |= !string.IsNullOrWhiteSpace(vendor.Company) && vendor.Company.ToLower().Contains(_key.ToLower());

                            //Search Customer by Phone
                            result |= (!string.IsNullOrWhiteSpace(vendor.Phone1) && vendor.Phone1.Contains(_key.ToLower()))
                                     || (!string.IsNullOrWhiteSpace(vendor.Phone2) && vendor.Phone2.Contains(_key.ToLower()));

                            //Search Customer by CellPhone
                            if (!string.IsNullOrWhiteSpace(vendor.CellPhone))
                                result |= vendor.CellPhone.ToLower().Contains(_key.ToLower());

                            //Search Customer by Email
                            if (!string.IsNullOrWhiteSpace(vendor.Email))
                                result |= vendor.Email.ToLower().Contains(_key.ToLower());

                            //Search Customer by Website
                            if (!string.IsNullOrWhiteSpace(vendor.Website))
                                result |= vendor.Website.ToLower().Contains(_key.ToLower());

                            //Search Customer by City
                            result |= vendor.base_Guest.base_GuestAddress.Any(x => x.IsDefault && x.City.ToLower().Contains(_key.ToLower()));

                            // Search Customer by  Country
                            IEnumerable<int> countryList = Common.Countries.Where(x => x.Text.ToLower().Contains(_key.ToLower())).Select(x => Convert.ToInt32(x.ObjValue));
                            if (countryList.Any())
                                result |= vendor.base_Guest.base_GuestAddress.Any(x => x.IsDefault && countryList.Contains(x.CountryId));

                            // Get all guest group that contain keyword
                            IEnumerable<base_GuestGroupModel> guestGroups = GuestGroupCollection.Where(x => x.Name != string.Empty && x.Name.ToLower().Contains(_key.ToLower()));
                            IEnumerable<string> guestGroupResourceList = guestGroups.Select(x => x.Resource.ToString());

                            // Get all product that contain in guest group resource list
                            if (guestGroupResourceList.Any())
                            {
                                result |= guestGroupResourceList.Contains(vendor.GroupResource);
                            }
                        }
                    }
                    return result;
                };
                Total = vendorCollectionView.OfType<object>().Count();
            }
        }


        #endregion

        #region AddVendor

        /// <summary>
        /// Add new vendor.
        /// </summary>
        private void AddVendor()
        {
            PopupGuestViewModel popupGuestViewModel = new PopupGuestViewModel();
            _dialogService.ShowDialog<PopupGuestView>(_ownerViewModel, popupGuestViewModel, "Add Vendor");
            base_GuestModel newVendor = popupGuestViewModel.NewItem;
            // Add new vendor to VendorCollection.
            if (newVendor != null)
            {
                _vendorCollection.Add(newVendor);
                _vendorCollectionRoot.Add(newVendor);
            }
        }

        #endregion

        #region OpenVendorView

        /// <summary>
        /// Open vendor view.
        /// </summary>
        private void OpenVendorView()
        {
            Close(false);
            (_ownerViewModel as MainViewModel).OpenViewExecute("Vendor");
        }

        #endregion

        #region Select

        /// <summary>
        /// Select.
        /// </summary>
        private void Select()
        {
            Close(true);
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
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

        #region ClearFilter

        private void ClearFilter()
        {
            ListCollectionView vendorCollectionView = CollectionViewSource.GetDefaultView(VendorCollection) as ListCollectionView;
            if (vendorCollectionView != null)
            {
                vendorCollectionView.Filter = null;
            }
        }

        #endregion

        private void LoadDynamicData()
        {
            // Load guest group collection
            GuestGroupCollection = new ObservableCollection<base_GuestGroupModel>(_guestGroupRepository.GetAll().
                Select(x => new base_GuestGroupModel(x) { GuestGroupResource = x.Resource.ToString() }));
        }

        #endregion

        #region Override Methods

        #region LoadData

        public override void LoadData()
        {

        }

        #endregion

        #region OnViewChangingCommandCanExecute

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return true;
        }

        #endregion

        #endregion
    }
}
