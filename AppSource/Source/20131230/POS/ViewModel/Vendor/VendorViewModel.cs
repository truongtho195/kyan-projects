using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CPC.DragDrop;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;
using CPCToolkitExtLibraries;

namespace CPC.POS.ViewModel
{
    class VendorViewModel : ViewModelBase, IDragSource, IDropTarget
    {
        #region Defines

        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_GuestAddressRepository _addressRepository = new base_GuestAddressRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_GuestAdditionalRepository _additionalRepository = new base_GuestAdditionalRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_UOMRepository _uomRepository = new base_UOMRepository();
        private base_PurchaseOrderRepository _purchaseOrderRepository = new base_PurchaseOrderRepository();
        private base_VendorProductRepository _vendorProductRepository = new base_VendorProductRepository();
        private base_GuestRewardRepository _guestRewardRepository = new base_GuestRewardRepository();
        private base_GuestGroupRepository _guestGroupRepository = new base_GuestGroupRepository();
        private base_CustomFieldRepository _customFieldRepository = new base_CustomFieldRepository();

        private string _vendorMark = MarkType.Vendor.ToDescription();

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        #endregion

        #region Properties

        #region Search

        private bool isSearchMode = false;
        /// <summary>
        /// Search Mode: 
        /// true open the Search grid.
        /// false close the search grid and open data entry.
        /// </summary>
        public bool IsSearchMode
        {
            get { return isSearchMode; }
            set
            {
                if (value != isSearchMode)
                {
                    isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }


        #region Keyword
        private string _keyword;
        /// <summary>
        /// Gets or sets the Keyword.
        /// </summary>
        public string Keyword
        {
            get { return _keyword; }
            set
            {
                if (_keyword != value)
                {
                    _keyword = value;
                    ResetTimer();
                    OnPropertyChanged(() => Keyword);
                }
            }
        }
        #endregion


        private ObservableCollection<string> _columnCollection;
        /// <summary>
        /// Gets or sets the ColumnCollection.
        /// </summary>
        public ObservableCollection<string> ColumnCollection
        {
            get { return _columnCollection; }
            set
            {
                if (_columnCollection != value)
                {
                    _columnCollection = value;
                    OnPropertyChanged(() => ColumnCollection);
                }
            }
        }

        #endregion

        private ObservableCollection<base_GuestModel> _vendorCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the VendorCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorCollection
        {
            get { return _vendorCollection; }
            set
            {
                if (_vendorCollection != value)
                {
                    _vendorCollection = value;
                    OnPropertyChanged(() => VendorCollection);
                }
            }
        }

        private base_GuestModel _selectedVendor;
        /// <summary>
        /// Gets or sets the SelectedVendor.
        /// </summary>
        public base_GuestModel SelectedVendor
        {
            get { return _selectedVendor; }
            set
            {
                if (_selectedVendor != value)
                {
                    _selectedVendor = value;
                    OnPropertyChanged(() => SelectedVendor);
                }
            }
        }

        #region AddressTypeCollection

        private AddressTypeCollection _addressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection AddressTypeCollection
        {
            get { return _addressTypeCollection; }
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

        #region Parameter

        private Common _parameter;
        /// <summary>
        /// Gets or sets the Parameter.
        /// </summary>
        public Common Parameter
        {
            get
            {
                return _parameter;
            }
            set
            {
                _parameter = value;
                OnPropertyChanged(() => Parameter);
            }
        }


        #endregion

        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public List<base_DepartmentModel> CategoryList { get; set; }

        /// <summary>
        /// Gets or sets the UOMList
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> UOMList { get; set; }

        private base_PurchaseOrderModel _totalPurchaseOrder;
        /// <summary>
        /// Gets or sets the TotalPurchaseOrder.
        /// </summary>
        public base_PurchaseOrderModel TotalPurchaseOrder
        {
            get { return _totalPurchaseOrder; }
            set
            {
                if (_totalPurchaseOrder != value)
                {
                    _totalPurchaseOrder = value;
                    OnPropertyChanged(() => TotalPurchaseOrder);
                }
            }
        }

        /// <summary>
        /// Gets or sets the GuestGroupCollection.
        /// </summary>
        public ObservableCollection<base_GuestGroupModel> GuestGroupCollection { get; set; }

        private int _selectedTabIndex;
        /// <summary>
        /// Gets or sets the SelectedTabIndex.
        /// </summary>
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged(() => SelectedTabIndex);
                    if (SelectedVendor != null)
                        OnSelectedTabIndexChanged();
                }
            }
        }

        private bool _focusDefault;
        /// <summary>
        /// Gets or sets the FocusDefault.
        /// </summary>
        public bool FocusDefault
        {
            get { return _focusDefault; }
            set
            {
                if (_focusDefault != value)
                {
                    _focusDefault = value;
                    OnPropertyChanged(() => FocusDefault);
                }
            }
        }

        /// <summary>
        /// Gets the IsManualGenerate.
        /// </summary>
        public bool IsManualGenerate
        {
            get
            {
                if (Define.CONFIGURATION == null)
                    return false;
                return Define.CONFIGURATION.IsManualGenerate;
            }
        }

        private ObservableCollection<base_CustomFieldModel> _customFieldCollection;
        /// <summary>
        /// Gets or sets the CustomFieldCollection.
        /// </summary>
        public ObservableCollection<base_CustomFieldModel> CustomFieldCollection
        {
            get { return _customFieldCollection; }
            set
            {
                if (_customFieldCollection != value)
                {
                    _customFieldCollection = value;
                    OnPropertyChanged(() => CustomFieldCollection);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public VendorViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            StickyManagementViewModel = new PopupStickyViewModel();

            LoadStaticData();

            InitialCommand();

            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
        }

        public VendorViewModel(bool isList, object param = null)
            : this()
        {
            ChangeSearchMode(isList, param);
        }

        #endregion

        #region Command Methods

        #region SearchCommand

        /// <summary>
        /// Gets the SearchCommand command.
        /// </summary>
        public ICommand SearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                //_keyword = param.ToString();

                if (_waitingTimer != null)
                    _waitingTimer.Stop();

                // Load data by predicate
                LoadDataByPredicate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region PopupAdvanceSearchCommand

        /// <summary>
        /// Gets the PopupAdvanceSearchCommand command.
        /// </summary>
        public ICommand PopupAdvanceSearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupAdvanceSearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupAdvanceSearchCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupAdvanceSearchCommand command is executed.
        /// </summary>
        private void OnPopupAdvanceSearchCommandExecute(object param)
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();

            PopupVendorAdvanceSearchViewModel viewModel = new PopupVendorAdvanceSearchViewModel();
            bool? msgResult = _dialogService.ShowDialog<PopupVendorAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue && msgResult.Value)
            {
                // Load data by search predicate
                LoadDataByPredicate(viewModel.AdvanceSearchPredicate, false, 0);
            }
        }

        #endregion

        #region NewCommand

        /// <summary>
        /// Gets the NewCommand command.
        /// </summary>
        public ICommand NewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return UserPermissions.AllowAddVendor;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (IsSearchMode)
            {
                IsSearchMode = false;
                NewVendor();
            }
            else if (ShowNotification(null))
                NewVendor();
        }

        #endregion

        #region EditCommand

        /// <summary>
        /// Gets the EditCommand command.
        /// </summary>
        public ICommand EditCommand { get; private set; }

        /// <summary>
        /// Method to check whether the EditCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the EditCommand command is executed.
        /// </summary>
        private void OnEditCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            // Edit selected item
            OnDoubleClickViewCommandExecute(dataGridControl.SelectedItem);
        }

        #endregion

        #region SaveCommand

        /// <summary>
        /// Gets the SaveCommand command.
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return IsValid && IsEdit();
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SaveVendor(SelectedVendor);
        }

        #endregion

        #region DeleteCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (SelectedVendor == null)
                return false;
            return !IsEdit() && !SelectedVendor.IsNew;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this vendor?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                try
                {
                    if (SelectedVendor.IsNew)
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();

                        SelectedVendor = null;
                        IsSearchMode = true;
                    }
                    else if (IsValid)
                    {
                        List<ItemModel> ItemModel = new List<ItemModel>();
                        string resource = SelectedVendor.Resource.Value.ToString();
                        if (!_purchaseOrderRepository.GetAll().Select(x => x.VendorResource).Contains(resource))
                        {
                            // Remove all popup sticky
                            StickyManagementViewModel.DeleteAllResourceNote();

                            SelectedVendor.IsPurged = true;
                            SelectedVendor.ToEntity();
                            _guestRepository.Commit();
                            SelectedVendor.EndUpdate();
                            VendorCollection.Remove(SelectedVendor);
                            IsSearchMode = true;
                        }
                        else
                        {
                            ItemModel.Add(new ItemModel { Id = SelectedVendor.Id, Text = SelectedVendor.GuestNo, Resource = SelectedVendor.Resource.ToString() });
                            _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "PurchaseOrder"), "Problem Detection");
                        }
                    }
                    else
                        return;
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region DeletesCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeletesCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeletesCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            DataGridControl dataGridControl = param as DataGridControl;

            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this vendor?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                try
                {
                    bool flag = false;
                    List<ItemModel> ItemModel = new List<ItemModel>();
                    for (int i = 0; i < (dataGridControl.SelectedItems as ObservableCollection<object>).Count; i++)
                    {
                        base_GuestModel model = (dataGridControl.SelectedItems as ObservableCollection<object>)[i] as base_GuestModel;
                        string resource = model.Resource.Value.ToString();
                        if (!_purchaseOrderRepository.GetAll().Select(x => x.VendorResource).Contains(resource))
                        {
                            model.IsPurged = true;
                            model.ToEntity();
                            _guestRepository.Commit();
                            model.EndUpdate();
                            VendorCollection.Remove(model);

                            // Remove all popup sticky
                            StickyManagementViewModel.DeleteAllResourceNote(model.ResourceNoteCollection);

                            i--;
                        }
                        else
                        {
                            ItemModel.Add(new ItemModel { Id = model.Id, Text = model.GuestNo, Resource = resource });
                            flag = true;
                        }
                    }
                    if (flag)
                        _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "PurchaseOrder"), "Problem Detection");
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region DuplicateCommand

        /// <summary>
        /// Gets the DuplicateCommand command.
        /// </summary>
        public ICommand DuplicateCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DuplicateCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDuplicateCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1 && UserPermissions.AllowAddVendor;
        }

        /// <summary>
        /// Method to invoke when the DuplicateCommand command is executed.
        /// </summary>
        private void OnDuplicateCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;
            base_GuestModel selectedItem = dataGridControl.SelectedItem as base_GuestModel;
            IsSearchMode = false;

            // Create new vendor model
            SelectedVendor = new base_GuestModel { Mark = selectedItem.Mark };

            // Duplicate vendor
            SelectedVendor.IsActived = true;
            SelectedVendor.FirstName = selectedItem.FirstName;
            SelectedVendor.MiddleName = selectedItem.MiddleName;
            SelectedVendor.LastName = selectedItem.LastName;
            SelectedVendor.Company = string.Format("{0} (Copy)", selectedItem.Company);
            SelectedVendor.GroupResource = selectedItem.GroupResource;
            SelectedVendor.GroupName = selectedItem.GroupName;
            SelectedVendor.DateCreated = DateTimeExt.Now;
            SelectedVendor.GuestNo = DateTimeExt.Now.ToString(Define.GuestNoFormat);
            SelectedVendor.Resource = Guid.NewGuid();
            SelectedVendor.UserCreated = Define.USER.LoginName;
            SelectedVendor.Shift = Define.ShiftCode;

            // Initial guest address collection
            SelectedVendor.AddressCollection = new ObservableCollection<base_GuestAddressModel>();

            // Initial address control collection
            SelectedVendor.AddressControlCollection = new AddressControlCollection();

            foreach (AddressControlModel addressControlItem in selectedItem.AddressControlCollection)
            {
                // Create new guest address model
                base_GuestAddressModel guestAddressModel = new base_GuestAddressModel();

                // Duplicate guest address model
                guestAddressModel.ToModel(addressControlItem);

                // Get default guest address model
                if (guestAddressModel.IsDefault)
                    SelectedVendor.AddressModel = guestAddressModel;

                // Add new guest address model to collection
                SelectedVendor.AddressCollection.Add(guestAddressModel);

                // Create new address control model
                AddressControlModel addressControlModel = guestAddressModel.ToAddressControlModel();

                // Add new address control model to collection
                SelectedVendor.AddressControlCollection.Add(addressControlModel);

                // Turn off IsDirty & IsNew
                guestAddressModel.EndUpdate();
                addressControlModel.IsDirty = false;
            }

            // Duplicate additional model
            SelectedVendor.AdditionalModel = new base_GuestAdditionalModel();
            SelectedVendor.PaymentTermDescription = selectedItem.PaymentTermDescription;
            SelectedVendor.TermDiscount = selectedItem.TermDiscount;
            SelectedVendor.TermNetDue = selectedItem.TermNetDue;
            SelectedVendor.TermPaidWithinDay = selectedItem.TermPaidWithinDay;
            SelectedVendor.AdditionalModel.FedTaxId = selectedItem.AdditionalModel.FedTaxId;

            SelectedVendor.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            SelectedVendor.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            StickyManagementViewModel.SetParentResource(SelectedVendor.Resource.ToString(), SelectedVendor.ResourceNoteCollection);

            // Turn off IsDirty
            SelectedVendor.IsDirty = false;
            SelectedVendor.AdditionalModel.IsDirty = false;
        }

        #endregion

        #region PopupMergeItemCommand

        /// <summary>
        /// Gets the PopupMergeItemCommand command.
        /// </summary>
        public ICommand PopupMergeItemCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupMergeItemCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupMergeItemCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1 && VendorCollection.Count > 1;
        }

        /// <summary>
        /// Method to invoke when the PopupMergeItemCommand command is executed.
        /// </summary>
        private void OnPopupMergeItemCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            // Get vendor model
            base_GuestModel vendorModel = dataGridControl.SelectedItem as base_GuestModel;

            PopupMergeVendorViewModel viewModel = new PopupMergeVendorViewModel(vendorModel, VendorCollection);
            bool? result = _dialogService.ShowDialog<PopupMergeVendorView>(_ownerViewModel, viewModel, "Merge Vendor");
            if (result.HasValue && result.Value)
            {
                try
                {
                    // Get source vendor model
                    base_GuestModel sourceVendorModel = VendorCollection.SingleOrDefault(x => x.Id.Equals(viewModel.SourceVendor.Id));

                    // Get target vendor model
                    base_GuestModel targetVendorModel = VendorCollection.SingleOrDefault(x => x.Id.Equals(viewModel.TargetVendor.Id));

                    string sourceVendorResource = sourceVendorModel.Resource.ToString();
                    string targetVendorResource = targetVendorModel.Resource.ToString();

                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote(sourceVendorModel.ResourceNoteCollection);

                    // Get all guest reward that contain vendor id
                    IList<base_GuestReward> guestRewards = _guestRewardRepository.GetAll(x => x.GuestId.Equals(sourceVendorModel.Id));
                    foreach (base_GuestReward guestReward in guestRewards)
                    {
                        // Update vendor id in GuestReward
                        guestReward.GuestId = targetVendorModel.Id;
                    }

                    // Get all product that contain vendor id
                    IList<base_Product> products = _productRepository.GetAll(x => x.VendorId.Equals(sourceVendorModel.Id));
                    foreach (base_Product product in products)
                    {
                        // Update vendor id in Product
                        product.VendorId = targetVendorModel.Id;

                        // Get vendor product
                        base_VendorProduct vendorProduct = product.base_VendorProduct.SingleOrDefault(x => x.VendorId.Equals(targetVendorModel.Id));

                        if (vendorProduct != null)
                        {
                            // Delete vendor product from database
                            _vendorProductRepository.Delete(vendorProduct);
                        }
                    }

                    // Get all purchase order that contain vendor id
                    IList<base_PurchaseOrder> purchaseOrders = _purchaseOrderRepository.GetAll(x => x.VendorResource.Equals(sourceVendorResource));
                    foreach (base_PurchaseOrder purchaseOrder in purchaseOrders)
                    {
                        // Update vendor code and vendor resource in PurchaseOrder
                        purchaseOrder.VendorCode = targetVendorModel.GuestNo;
                        purchaseOrder.VendorResource = targetVendorResource;
                    }

                    // Get all vendor product that contain vendor id
                    //IList<base_VendorProduct> vendorProducts = _vendorProductRepository.GetAll(x => x.VendorId.Equals(sourceVendorModel.Id));
                    if (targetVendorModel.base_Guest.base_VendorProduct.Count == 0)
                    {
                        foreach (base_VendorProduct vendorProduct in sourceVendorModel.base_Guest.base_VendorProduct.ToList())
                        {
                            if (vendorProduct.base_Product.VendorId.Equals(targetVendorModel.Id))
                            {
                                // Delete vendor product from database
                                _vendorProductRepository.Delete(vendorProduct);
                            }
                            else
                            {
                                // Update vendor id in VendorProduct
                                vendorProduct.VendorId = targetVendorModel.Id;
                            }
                        }
                    }

                    // Remove source vendor from database
                    _guestRepository.Delete(sourceVendorModel.base_Guest);

                    // Remove source vendor from collection
                    if (sourceVendorModel != null)
                        VendorCollection.Remove(sourceVendorModel);

                    // Accept changes
                    _guestRepository.Commit();
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region PurchaseOrderCommand

        /// <summary>
        /// Gets the PurchaseOrderCommand command.
        /// </summary>
        public ICommand PurchaseOrderCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PurchaseOrderCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPurchaseOrderCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the PurchaseOrderCommand command is executed.
        /// </summary>
        private void OnPurchaseOrderCommandExecute(object param)
        {
            // Convert param to vendor model
            base_GuestModel vendorModel = param as base_GuestModel;

            // Open purchase order detail
            (_ownerViewModel as MainViewModel).OpenViewExecute("Purchase Order", vendorModel.Resource.Value);
        }

        #endregion

        #region DoubleClickViewCommand

        /// <summary>
        /// Gets the DoubleClickViewCommand command.
        /// </summary>
        public ICommand DoubleClickViewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DoubleClick command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DoubleClick command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            if (param != null && IsSearchMode)
            {
                // Update selected vendor
                SelectedVendor = param as base_GuestModel;

                // Load Additional
                LoadAdditionalModel(SelectedVendor);

                // Set parent resource
                StickyManagementViewModel.SetParentResource(SelectedVendor.Resource.ToString(), SelectedVendor.ResourceNoteCollection);

                // Show detail form
                IsSearchMode = false;
            }
            else if (IsSearchMode)
            {
                // Show detail form
                IsSearchMode = false;
            }
            else if (ShowNotification(null))
            {
                // Show list form
                IsSearchMode = true;
            }
        }

        #endregion

        #region PopupAddTermCommand

        /// <summary>
        /// Gets the PopupAddTermCommand command.
        /// </summary>
        public ICommand PopupAddTermCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupAddTermCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupAddTermCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupAddTermCommand command is executed.
        /// </summary>
        private void OnPopupAddTermCommandExecute()
        {
            short dueDays = SelectedVendor.TermNetDue;
            decimal discount = SelectedVendor.TermDiscount;
            short discountDays = SelectedVendor.TermPaidWithinDay;
            PaymentTermViewModel paymentTermViewModel = new PaymentTermViewModel(SelectedVendor.IsCOD, dueDays, discount, discountDays);
            bool? dialogResult = _dialogService.ShowDialog<PaymentTermView>(_ownerViewModel, paymentTermViewModel, "Add Term");
            if (dialogResult == true)
            {
                SelectedVendor.TermNetDue = paymentTermViewModel.DueDays;
                SelectedVendor.TermDiscount = paymentTermViewModel.Discount;
                SelectedVendor.TermPaidWithinDay = paymentTermViewModel.DiscountDays;
                SelectedVendor.PaymentTermDescription = paymentTermViewModel.Description;
                SelectedVendor.IsCOD = paymentTermViewModel.IsCOD;
            }
        }

        #endregion

        #region PopupGuestGroupCommand

        /// <summary>
        /// Gets the PopupGuestGroupCommand command.
        /// </summary>
        public ICommand PopupGuestGroupCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupGuestGroupCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupGuestGroupCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupGuestGroupCommand command is executed.
        /// </summary>
        private void OnPopupGuestGroupCommandExecute()
        {
            PopupAddNewGroupViewModel viewModel = new PopupAddNewGroupViewModel(_vendorMark);
            bool? result = _dialogService.ShowDialog<PopupAddNewGroupView>(_ownerViewModel, viewModel, "Add new group");
            if (result.HasValue && result.Value)
            {
                // Add new guest group to collection
                GuestGroupCollection.Add(viewModel.SelectedGuestGroup);

                SelectedVendor.GroupResource = viewModel.SelectedGuestGroup.Resource.ToString();
            }
        }

        #endregion

        #region InsertDateStampCommand

        /// <summary>
        /// Gets the InsertDateStampCommand command.
        /// </summary>
        public ICommand InsertDateStampCommand { get; private set; }

        /// <summary>
        /// Method to check whether the InsertDateStampCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnInsertDateStampCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the InsertDateStampCommand command is executed.
        /// </summary>
        private void OnInsertDateStampCommandExecute(object param)
        {
            CPCToolkitExt.TextBoxControl.TextBox remarkTextBox = param as CPCToolkitExt.TextBoxControl.TextBox;
            SetValueControlHelper.InsertTimeStamp(remarkTextBox);
        }

        #endregion

        #region Print Command

        /// <summary>
        /// Gets the Print command.
        /// </summary>
        public ICommand PrintCommand { get; private set; }

        /// <summary>
        /// Check can print report
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool CanPrintReportExecute()
        {
            return (SelectedVendor != null && !SelectedVendor.IsNew && !SelectedVendor.IsDirty);
        }

        /// <summary>
        /// Method to invoke when the PrintCommand is executed.
        /// </summary>
        private void PrintReportExecute()
        {
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            string param = "'" + SelectedVendor.Resource.ToString() + "'";
            rpt.ShowReport("rptVendorProfile", param);
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            try
            {
                // Created by Thaipn
                Parameter = new Common();

                // Get address type collection
                // Created by Thaipn
                AddressTypeCollection = new AddressTypeCollection();
                AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = "Home" });
                AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = "Business" });
                AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });
                AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });

                // Load category list
                if (CategoryList == null)
                {
                    CategoryList = new List<base_DepartmentModel>(_departmentRepository.
                            GetAll(x => x.IsActived == true && x.LevelId == 1).
                            Select(x => new base_DepartmentModel(x)));
                }

                // Load UOM list
                if (UOMList == null)
                {
                    UOMList = new ObservableCollection<CheckBoxItemModel>(_uomRepository.GetIQueryable(x => x.IsActived).
                            OrderBy(x => x.Name).Select(x => new CheckBoxItemModel { Value = x.Id, Text = x.Name }));
                }

                // Load guest group collection
                if (GuestGroupCollection == null)
                {
                    GuestGroupCollection = new ObservableCollection<base_GuestGroupModel>(_guestGroupRepository.
                        GetAll(x => x.Mark.Equals(_vendorMark)).
                        Select(x => new base_GuestGroupModel(x) { GuestGroupResource = x.Resource.ToString() }));
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Initial commands
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            PopupAdvanceSearchCommand = new RelayCommand<object>(OnPopupAdvanceSearchCommandExecute, OnPopupAdvanceSearchCommandCanExecute);
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            PopupAddTermCommand = new RelayCommand(OnPopupAddTermCommandExecute, OnPopupAddTermCommandCanExecute);
            DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
            PopupMergeItemCommand = new RelayCommand<object>(OnPopupMergeItemCommandExecute, OnPopupMergeItemCommandCanExecute);
            PurchaseOrderCommand = new RelayCommand<object>(OnPurchaseOrderCommandExecute, OnPurchaseOrderCommandCanExecute);
            PopupGuestGroupCommand = new RelayCommand(OnPopupGuestGroupCommandExecute, OnPopupGuestGroupCommandCanExecute);
            DuplicateCommand = new RelayCommand<object>(OnDuplicateCommandExecute, OnDuplicateCommandCanExecute);
            InsertDateStampCommand = new RelayCommand<object>(OnInsertDateStampCommandExecute, OnInsertDateStampCommandCanExecute);
            PrintCommand = new RelayCommand(PrintReportExecute, CanPrintReportExecute);
        }

        /// <summary>
        /// Check has edit on form
        /// </summary>
        /// <returns></returns>
        private bool IsEdit()
        {
            if (SelectedVendor == null)
                return false;

            //Repaired by Thaipn.
            return (SelectedVendor.IsDirty || SelectedVendor.AdditionalModel.IsDirty ||
                (SelectedVendor.AddressControlCollection != null && SelectedVendor.AddressControlCollection.IsEditingData) ||
                (SelectedVendor.PhotoCollection != null && SelectedVendor.PhotoCollection.IsDirty));
        }

        /// <summary>
        /// Notify when exit or change form
        /// </summary>
        /// <returns>True is continue action</returns>
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;

            // Check data is edited
            if (IsEdit())
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (msgResult.Is(MessageBoxResult.Cancel))
                {
                    return false;
                }
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        result = SaveVendor(SelectedVendor);
                    }
                    else
                    {
                        result = false;
                    }

                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
                else
                {
                    if (SelectedVendor.IsNew)
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();

                        SelectedVendor = null;
                        if (isClosing.HasValue && !isClosing.Value)
                        {
                            IsSearchMode = true;
                        }
                    }
                    else
                    {
                        // Rollback vendor
                        SelectedVendor.AddressCollection = null;
                        SelectedVendor.PhotoCollection = null;
                        SelectedVendor.AdditionalModel = null;
                        SelectedVendor.ProductCollection = null;
                        SelectedVendor.PurchaseOrderCollection = null;
                        SelectedVendor.ToModelAndRaise();
                        SelectedVendor.EndUpdate();
                        LoadRelationVendorData(SelectedVendor);

                        // Close all popup sticky
                        StickyManagementViewModel.CloseAllPopupSticky();
                    }
                }
            }
            else
            {
                if (SelectedVendor != null && SelectedVendor.IsNew)
                {
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();
                }
                else
                {
                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
            }

            // Clear selected item
            if (result && isClosing == null && SelectedVendor != null)
                SelectedVendor = null;

            return result;
        }

        /// <summary>
        /// Create new a VendorModel and some default value
        /// </summary>
        private void NewVendor()
        {
            SelectedVendor = new base_GuestModel { Mark = MarkType.Vendor.ToDescription() };
            SelectedVendor.IsActived = true;
            SelectedVendor.DateCreated = DateTimeExt.Now;
            SelectedVendor.GuestNo = DateTimeExt.Now.ToString(Define.GuestNoFormat);
            SelectedVendor.PositionId = 0;
            SelectedVendor.Resource = Guid.NewGuid();
            SelectedVendor.UserCreated = Define.USER.LoginName;
            SelectedVendor.Shift = Define.ShiftCode;

            // Update StatusItem
            SelectedVendor.StatusItem = Common.StatusBasic.SingleOrDefault(x => (x.Value == 1).Equals(SelectedVendor.IsActived));

            // Initial address control collection
            // Created by Thaipn.
            SelectedVendor.AddressControlCollection = new AddressControlCollection();
            SelectedVendor.AddressControlCollection.Add(new AddressControlModel
            {
                IsDefault = true,
                IsNew = true
            });

            SelectedVendor.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            SelectedVendor.AdditionalModel = new base_GuestAdditionalModel();
            SelectedVendor.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            StickyManagementViewModel.SetParentResource(SelectedVendor.Resource.ToString(), SelectedVendor.ResourceNoteCollection);

            // Turn off IsDirty
            SelectedVendor.IsDirty = false;

            FocusDefault = false;
            FocusDefault = true;
        }

        /// <summary>
        /// Function save Vendor
        /// </summary>
        /// <param name="param"></param>
        private bool SaveVendor(base_GuestModel vendorModel)
        {
            // Check duplicate vendor
            if (CheckDuplicateVendor(vendorModel))
                return false;
            //To save picture.
            if (this.SelectedVendor.PhotoCollection != null && this.SelectedVendor.PhotoCollection.Count > 0)
            {
                this.SelectedVendor.PhotoCollection.FirstOrDefault().IsNew = false;
                this.SelectedVendor.PhotoCollection.FirstOrDefault().IsDirty = false;
                this.SelectedVendor.Picture = this.SelectedVendor.PhotoCollection.FirstOrDefault().ImageBinary;
            }
            else
                this.SelectedVendor.Picture = null;
            if (this.SelectedVendor.PhotoCollection.DeletedItems != null &&
                 this.SelectedVendor.PhotoCollection.DeletedItems.Count > 0)
                this.SelectedVendor.PhotoCollection.DeletedItems.Clear();

            // Get group name for vendor
            if (vendorModel.base_Guest.GroupResource != vendorModel.GroupResource)
            {
                base_GuestGroupModel guestGroupItem = GuestGroupCollection.FirstOrDefault(x => x.Resource.ToString().Equals(vendorModel.GroupResource));
                if (guestGroupItem != null)
                    vendorModel.GroupName = guestGroupItem.Name;
            }

            // Vendor is create new
            if (vendorModel.IsNew)
            {
                // Insert a new vendor
                SaveNew(vendorModel);
            }
            else // Vendor is edited
            {
                // Update vendor
                SaveUpdate(vendorModel);
            }

            // Update StatusItem
            vendorModel.StatusItem = Common.StatusBasic.SingleOrDefault(x => (x.Value == 1).Equals(vendorModel.IsActived));

            // Turn off IsDirty & IsNew
            vendorModel.EndUpdate();
            vendorModel.AdditionalModel.EndUpdate();

            return true;
        }

        /// <summary>
        /// Save when create new Vendor
        /// </summary>
        private void SaveNew(base_GuestModel vendorModel)
        {
            try
            {
                // Map data from model to entity
                vendorModel.ToEntity();

                // Insert address
                // Created by Thaipn
                foreach (AddressControlModel addressControlModel in vendorModel.AddressControlCollection)
                {
                    base_GuestAddressModel addressModel = new base_GuestAddressModel();
                    addressModel.DateCreated = DateTimeExt.Now;
                    addressModel.UserCreated = Define.USER.LoginName;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);
                    addressModel.GuestResource = vendorModel.Resource.ToString();

                    // Update default address
                    if (addressModel.IsDefault)
                        vendorModel.AddressModel = addressModel;

                    // Map data from model to entity
                    addressModel.ToEntity();
                    vendorModel.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);

                    // Turn off IsDirty & IsNew
                    addressModel.EndUpdate();

                    addressControlModel.IsNew = false;
                    addressControlModel.IsDirty = false;
                }

                //// Save image
                //if (vendorModel.PhotoCollection != null && vendorModel.PhotoCollection.Count > 0)
                //{
                //    foreach (base_ResourcePhotoModel photoModel in vendorModel.PhotoCollection.Where(x => x.IsNew))
                //    {
                //        //photoModel.LargePhotoFilename = new System.IO.FileInfo(photoModel.ImagePath).Name;
                //        photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;

                //        // Update resource photo
                //        photoModel.Resource = vendorModel.Resource.ToString();

                //        // Map data from model to entity
                //        photoModel.ToEntity();
                //        _photoRepository.Add(photoModel.base_ResourcePhoto);

                //        // Copy image from client to server
                //        SaveImage(photoModel);

                //        // Turn off IsDirty & IsNew
                //        photoModel.EndUpdate();
                //    }
                //}

                // Update default photo if it is deleted
                if (vendorModel.PhotoCollection.Any())
                    vendorModel.PhotoDefault = vendorModel.PhotoCollection.FirstOrDefault();
                else
                    vendorModel.PhotoDefault = new base_ResourcePhotoModel();

                vendorModel.AdditionalModel.ToEntity();
                vendorModel.base_Guest.base_GuestAdditional.Add(vendorModel.AdditionalModel.base_GuestAdditional);

                _guestRepository.Add(vendorModel.base_Guest);
                _guestRepository.Commit();

                // Update ID from entity to model
                vendorModel.Id = vendorModel.base_Guest.Id;
                vendorModel.AdditionalModel.GuestId = vendorModel.base_Guest.Id;
                vendorModel.AdditionalModel.Id = vendorModel.AdditionalModel.base_GuestAdditional.Id;
                foreach (base_ResourcePhotoModel photoModel in vendorModel.PhotoCollection)
                {
                    photoModel.Id = photoModel.base_ResourcePhoto.Id;

                    // Turn off IsDirty & IsNew
                    photoModel.EndUpdate();
                }

                // Push new vendor to collection
                VendorCollection.Insert(0, vendorModel);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Update a vendor
        /// </summary>
        private void SaveUpdate(base_GuestModel vendorModel)
        {
            try
            {
                vendorModel.DateUpdated = DateTimeExt.Now;
                if (Define.USER != null)
                    vendorModel.UserUpdated = Define.USER.LoginName;

                // Map data from model to entity
                vendorModel.ToEntity();

                #region Save address

                // Insert or update address
                // Created by Thaipn
                foreach (AddressControlModel addressControlModel in vendorModel.AddressControlCollection.Where(x => x.IsDirty))
                {
                    base_GuestAddressModel addressModel = new base_GuestAddressModel();

                    // Insert new address
                    if (addressControlModel.IsNew)
                    {
                        addressModel.DateCreated = DateTimeExt.Now;
                        addressModel.UserCreated = Define.USER.LoginName;
                        // Map date from AddressControlModel to AddressModel
                        addressModel.ToModel(addressControlModel);
                        addressModel.GuestResource = vendorModel.Resource.ToString();

                        // Map data from model to entity
                        addressModel.ToEntity();
                        vendorModel.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                    }
                    // Update address
                    else
                    {
                        base_GuestAddress address = vendorModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId == addressControlModel.AddressTypeID);
                        addressModel = new base_GuestAddressModel(address);

                        addressModel.DateUpdated = DateTimeExt.Now;
                        addressModel.UserUpdated = Define.USER.LoginName;
                        // Map date from AddressControlModel to AddressModel
                        addressModel.ToModel(addressControlModel);
                        addressModel.ToEntity();
                    }

                    // Update default address
                    if (addressModel.IsDefault)
                        vendorModel.AddressModel = addressModel;

                    // Turn off IsDirty & IsNew
                    addressModel.EndUpdate();

                    addressControlModel.IsNew = false;
                    addressControlModel.IsDirty = false;
                }

                #endregion

                #region Save photo

                //// Remove photo were deleted
                //if (vendorModel.PhotoCollection != null &&
                //    vendorModel.PhotoCollection.DeletedItems != null && vendorModel.PhotoCollection.DeletedItems.Count > 0)
                //{
                //    foreach (base_ResourcePhotoModel photoModel in vendorModel.PhotoCollection.DeletedItems)
                //    {
                //        //System.IO.FileInfo fileInfo = new System.IO.FileInfo(photoModel.ImagePath);
                //        //fileInfo.MoveTo(photoModel.ImagePath + "temp");
                //        //System.IO.FileInfo fileInfoTemp = new System.IO.FileInfo(photoModel.ImagePath + "temp");
                //        //fileInfoTemp.Delete();

                //        _photoRepository.Delete(photoModel.base_ResourcePhoto);
                //    }
                //    vendorModel.PhotoCollection.DeletedItems.Clear();
                //}
                //// Update photo
                //if (vendorModel.PhotoCollection != null && vendorModel.PhotoCollection.Count > 0)
                //{
                //    foreach (base_ResourcePhotoModel photoModel in vendorModel.PhotoCollection.Where(x => x.IsDirty))
                //    {
                //        //photoModel.LargePhotoFilename = new System.IO.FileInfo(photoModel.ImagePath).Name;
                //        photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;

                //        // Update resource photo
                //        if (string.IsNullOrWhiteSpace(photoModel.Resource))
                //            photoModel.Resource = vendorModel.Resource.ToString();

                //        // Map data from model to entity
                //        photoModel.ToEntity();

                //        if (photoModel.IsNew)
                //            _photoRepository.Add(photoModel.base_ResourcePhoto);

                //        // Copy image from client to server
                //        SaveImage(photoModel);

                //        // Turn off IsDirty & IsNew
                //        photoModel.EndUpdate();
                //    }
                //}

                //// Update default photo if it is deleted
                //if (vendorModel.PhotoCollection.Any())
                //    vendorModel.PhotoDefault = vendorModel.PhotoCollection.FirstOrDefault();
                //else
                //    vendorModel.PhotoDefault = new base_ResourcePhotoModel();

                #endregion

                vendorModel.AdditionalModel.GuestId = vendorModel.Id;

                // Map data from model to entity
                vendorModel.AdditionalModel.ToEntity();

                if (vendorModel.base_Guest.base_GuestAdditional.Count == 0)
                    vendorModel.base_Guest.base_GuestAdditional.Add(vendorModel.AdditionalModel.base_GuestAdditional);

                _guestRepository.Commit();

                if (vendorModel.AdditionalModel.IsNew)
                    vendorModel.AdditionalModel.Id = vendorModel.AdditionalModel.base_GuestAdditional.Id;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<base_Guest>();

                if (ColumnCollection.Count > 0)
                {
                    if (ColumnCollection.Contains(SearchOptions.GuestNo.ToString()))
                    {
                        // Get all vendors that GuestNo contain keyword
                        predicate = predicate.Or(x => x.GuestNo.Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Status.ToString()))
                    {
                        // Get all statuses contain keyword
                        IEnumerable<ComboItem> statusItems = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(keyword));
                        IEnumerable<bool> statusItemIDList = statusItems.Select(x => x.Value == 1);

                        // Get all vendors that Status contain keyword
                        if (statusItemIDList.Count() > 0)
                            predicate = predicate.Or(x => statusItemIDList.Contains(x.IsActived));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Company.ToString()))
                    {
                        // Get all vendors that Company contain keyword
                        predicate = predicate.Or(x => x.Company.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.FirstName.ToString()))
                    {
                        // Get all vendors that FirstName contain keyword
                        predicate = predicate.Or(x => x.FirstName.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.LastName.ToString()))
                    {
                        // Get all vendors that LastName contain keyword
                        predicate = predicate.Or(x => x.LastName.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Group.ToString()))
                    {
                        // Get all groups contain keyword
                        IEnumerable<base_GuestGroupModel> guestGroups = GuestGroupCollection.Where(x => x.Name.ToLower().Contains(keyword));
                        IEnumerable<string> guestGroupResourceList = guestGroups.Select(x => x.Resource.ToString());

                        // Get all vendors that Group contain keyword
                        if (guestGroupResourceList.Count() > 0)
                            predicate = predicate.Or(x => guestGroupResourceList.Contains(x.GroupResource));
                    }
                    if (ColumnCollection.Contains(SearchOptions.City.ToString()))
                    {
                        // Get all vendors that City contain keyword
                        predicate = predicate.Or(x => x.base_GuestAddress.Any(y => y.IsDefault && y.City.ToLower().Contains(keyword)));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Country.ToString()))
                    {
                        // Get all countries that contain keyword
                        IEnumerable<ComboItem> countryItems = Common.Countries.Where(x => x.Text.ToLower().Contains(keyword));
                        IEnumerable<int> countryItemIDList = countryItems.Select(x => (int)x.Value);

                        // Get all vendors that Country contain keyword
                        if (countryItemIDList.Count() > 0)
                            predicate = predicate.Or(x => x.base_GuestAddress.Any(y => y.IsDefault && countryItemIDList.Contains(y.CountryId)));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Phone.ToString()))
                    {
                        // Get all vendors that Phone contain keyword
                        predicate = predicate.Or(x => x.Phone1.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.CellPhone.ToString()))
                    {
                        // Get all vendors that CellPhone contain keyword
                        predicate = predicate.Or(x => x.CellPhone.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Email.ToString()))
                    {
                        // Get all vendors that Email contain keyword
                        predicate = predicate.Or(x => x.Email.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Website.ToString()))
                    {
                        // Get all vendors that Website contain keyword
                        predicate = predicate.Or(x => x.Website.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom1.ToString()))
                    {
                        // Get all vendors that Custom1 contain keyword
                        predicate = predicate.Or(x => x.Custom1.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom2.ToString()))
                    {
                        // Get all vendors that Custom2 contain keyword
                        predicate = predicate.Or(x => x.Custom2.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom3.ToString()))
                    {
                        // Get all vendors that Custom3 contain keyword
                        predicate = predicate.Or(x => x.Custom3.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom4.ToString()))
                    {
                        // Get all vendors that Custom4 contain keyword
                        predicate = predicate.Or(x => x.Custom4.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom5.ToString()))
                    {
                        // Get all vendors that Custom5 contain keyword
                        predicate = predicate.Or(x => x.Custom5.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom6.ToString()))
                    {
                        // Get all vendors that Custom6 contain keyword
                        predicate = predicate.Or(x => x.Custom6.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom7.ToString()))
                    {
                        // Get all vendors that Custom7 contain keyword
                        predicate = predicate.Or(x => x.Custom7.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Custom8.ToString()))
                    {
                        // Get all vendors that Custom8 contain keyword
                        predicate = predicate.Or(x => x.Custom8.ToLower().Contains(keyword));
                    }
                }
            }

            // Default condition
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_vendorMark));

            return predicate;
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Guest, bool>> predicate = CreateSearchPredicate(_keyword);

            // Load data by predicate
            LoadDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="predicate">Condition for load data</param>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(Expression<Func<base_Guest, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                VendorCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                try
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    if (refreshData)
                    {
                        //_guestRepository.Refresh();
                        //_addressRepository.Refresh();
                        //_photoRepository.Refresh();
                        //_additionalRepository.Refresh();
                    }

                    // Get data with range
                    IList<base_Guest> vendors = _guestRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.DateCreated, predicate);
                    foreach (base_Guest vendor in vendors)
                    {
                        bgWorker.ReportProgress(0, vendor);
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create vendor model
                base_GuestModel vendorModel = new base_GuestModel((base_Guest)e.UserState);

                // Load relation data
                LoadRelationVendorData(vendorModel);

                // Add to collection
                VendorCollection.Add(vendorModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for vendor
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadRelationVendorData(base_GuestModel vendorModel)
        {
            // Update StatusItem
            vendorModel.StatusItem = Common.StatusBasic.SingleOrDefault(x => (x.Value == 1).Equals(vendorModel.IsActived));

            // Get group name for vendor
            if (string.IsNullOrWhiteSpace(vendorModel.GroupName))
            {
                base_GuestGroupModel guestGroupItem = GuestGroupCollection.FirstOrDefault(x => x.Resource.ToString().Equals(vendorModel.GroupResource));
                if (guestGroupItem != null)
                    vendorModel.GroupName = guestGroupItem.Name;
            }

            // Load Address
            LoadAddressCollection(vendorModel);

            // Load Photo
            LoadPhotoCollection(vendorModel);

            // Load Additional
            LoadAdditionalModel(vendorModel);

            // Load resource note collection
            LoadResourceNoteCollection(vendorModel);
        }

        /// <summary>
        /// Load address collection
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadAddressCollection(base_GuestModel vendorModel)
        {
            if (vendorModel.AddressCollection == null)
            {
                vendorModel.AddressCollection = new ObservableCollection<base_GuestAddressModel>(
                    vendorModel.base_Guest.base_GuestAddress.Select(x => new base_GuestAddressModel(x)));
                vendorModel.AddressModel = vendorModel.AddressCollection.SingleOrDefault(x => x.IsDefault);

                // Update CountryItem
                vendorModel.CountryItem = Common.Countries.SingleOrDefault(x => x.Value.Equals((short)vendorModel.AddressModel.CountryId));

                vendorModel.AddressControlCollection = new AddressControlCollection();
                foreach (base_GuestAddressModel addressModel in vendorModel.AddressCollection)
                {
                    AddressControlModel addressControlModel = addressModel.ToAddressControlModel();
                    addressControlModel.IsDirty = false;
                    addressControlModel.IsChangeData = false;
                    vendorModel.AddressControlCollection.Add(addressControlModel);
                }
            }
        }

        /// <summary>
        /// Load photo collection
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadPhotoCollection(base_GuestModel vendorModel)
        {
            if (vendorModel.PhotoCollection == null)
            {
                vendorModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
                if (vendorModel.Picture != null && vendorModel.Picture.Length > 0)
                {
                    base_ResourcePhotoModel ResourcePhotoModel = new base_ResourcePhotoModel();
                    ResourcePhotoModel.ImageBinary = vendorModel.Picture;
                    ResourcePhotoModel.IsDirty = false;
                    ResourcePhotoModel.IsNew = false;
                    vendorModel.PhotoCollection.Add(ResourcePhotoModel);

                    // Set default photo
                    if (vendorModel.PhotoCollection.Any())
                        vendorModel.PhotoDefault = vendorModel.PhotoCollection.FirstOrDefault();
                    else
                        vendorModel.PhotoDefault = new base_ResourcePhotoModel();
                }
            }
        }

        /// <summary>
        /// Load additional model
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadAdditionalModel(base_GuestModel vendorModel)
        {
            if (vendorModel.AdditionalModel == null)
            {
                if (vendorModel.base_Guest.base_GuestAdditional.Count > 0)
                {
                    vendorModel.AdditionalModel = new base_GuestAdditionalModel(
                        vendorModel.base_Guest.base_GuestAdditional.FirstOrDefault());
                    vendorModel.AdditionalModel.EndUpdate();
                }
                else
                    vendorModel.AdditionalModel = new base_GuestAdditionalModel();
            }
        }

        /// <summary>
        /// Load purchase order collection
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadPurchaseOrderCollection(base_GuestModel vendorModel)
        {
            try
            {
                if (vendorModel.PurchaseOrderCollection == null)
                {
                    // Get vendor resource
                    string vendorResource = vendorModel.Resource.ToString();

                    // Initial purchase order collection
                    vendorModel.PurchaseOrderCollection = new ObservableCollection<base_PurchaseOrderModel>(_purchaseOrderRepository.
                        GetAll(x => x.VendorResource.Equals(vendorResource)).Select(x => new base_PurchaseOrderModel(x)
                        {
                            // Update status item
                            StatusItem = Common.PurchaseStatus.FirstOrDefault(y => y.Value.Equals(x.Status))
                        }));

                    TotalPurchaseOrder = new base_PurchaseOrderModel
                    {
                        Total = vendorModel.PurchaseOrderCollection.Sum(x => x.Total),
                        Paid = vendorModel.PurchaseOrderCollection.Sum(x => x.Paid),
                        Balance = vendorModel.PurchaseOrderCollection.Sum(x => x.Balance)
                    };
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load product vendor collection
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadProductVendorCollection(base_GuestModel vendorModel)
        {
            try
            {
                if (vendorModel.ProductCollection == null)
                {
                    // Initial purchase order collection
                    vendorModel.ProductCollection = new ObservableCollection<base_ProductModel>();

                    // Get all vendor product of selected vendor
                    IEnumerable<base_VendorProduct> vendorProducts = _vendorProductRepository.GetIEnumerable(x => x.VendorId.Equals(vendorModel.Id));
                    IEnumerable<long> productIDList = vendorProducts.Select(x => x.ProductId);

                    // Get all product of selected vendor
                    IList<base_Product> products = _productRepository.GetAll(x => x.IsPurge == false && (x.VendorId.Equals(vendorModel.Id) || productIDList.Count(y => y.Equals(x.Id)) > 0));

                    foreach (base_Product product in products)
                    {
                        base_ProductModel productModel = new base_ProductModel(product);
                        LoadRelationProductData(productModel);
                        vendorModel.ProductCollection.Add(productModel);

                        // Turn off IsDirty & IsNew
                        productModel.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load relation data for product
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationProductData(base_ProductModel productModel)
        {
            try
            {
                // Update ItemType
                productModel.ItemTypeItem = Common.ItemTypes.SingleOrDefault(x => x.Value.Equals(productModel.ItemTypeId));

                // Load Photo
                if (productModel.PhotoCollection == null)
                {
                    string resource = productModel.Resource.ToString();
                    productModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(
                        _photoRepository.GetAll(x => x.Resource.Equals(resource)).
                        Select(x => new base_ResourcePhotoModel(x)
                        {
                            ImagePath = Path.Combine(IMG_PRODUCT_DIRECTORY, productModel.Code, x.LargePhotoFilename),
                            IsDirty = false
                        }));

                    // Set default photo
                    if (productModel.PhotoCollection.Any())
                        productModel.PhotoDefault = productModel.PhotoCollection.FirstOrDefault();
                    else
                        productModel.PhotoDefault = new base_ResourcePhotoModel();
                }

                // Get category name for product
                if (string.IsNullOrWhiteSpace(productModel.CategoryName))
                {
                    base_DepartmentModel categoryItem = CategoryList.FirstOrDefault(x => x.Id.Equals(productModel.ProductCategoryId));
                    if (categoryItem != null)
                        productModel.CategoryName = categoryItem.Name;
                }

                // Get uom name for product
                if (string.IsNullOrWhiteSpace(productModel.UOMName))
                {
                    CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productModel.BaseUOMId));
                    if (uomItem != null)
                        productModel.UOMName = uomItem.Text;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load Custom Field from db
        /// </summary>
        /// <param name="customerModel"></param>
        private void LoadCustomFieldCollection()
        {
            try
            {
                IEnumerable<base_CustomField> customerFields = _customFieldRepository.GetAll().Where(x => x.Mark.Equals(_vendorMark));
                _customFieldRepository.Refresh(customerFields);
                if (customerFields != null && customerFields.Any())
                {
                    CustomFieldCollection = new ObservableCollection<base_CustomFieldModel>(customerFields.OrderBy(x => x.Id).Select(x => new base_CustomFieldModel(x)));
                }
                else
                {
                    CustomFieldCollection = new ObservableCollection<base_CustomFieldModel>();
                    for (int i = 1; i < 9; i++)
                    {
                        base_CustomFieldModel customFieldModel = new base_CustomFieldModel();
                        customFieldModel.Mark = _vendorMark;
                        customFieldModel.FieldName = "Custom " + i;
                        customFieldModel.IsShow = true;
                        customFieldModel.Label = customFieldModel.FieldName;
                        customFieldModel.ToEntity();
                        //insert to db
                        _customFieldRepository.Add(customFieldModel.base_CustomField);
                        customFieldModel.EndUpdate();
                        //Add To Collection
                        CustomFieldCollection.Add(customFieldModel);
                    }
                    _customFieldRepository.Commit();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Check Customer is duplicate
        /// <para>Value Compare : Email,Phone1</para>
        /// </summary>
        /// 
        /// <param name="vendorModel"></param>
        /// 
        /// <returns></returns>
        private bool CheckDuplicateVendor(base_GuestModel vendorModel)
        {
            bool result = false;
            try
            {
                IQueryable<base_Guest> query = _guestRepository.
                        GetIQueryable(x => !x.GuestNo.Equals(vendorModel.GuestNo) && x.Mark.Equals(_vendorMark) && !x.IsPurged &&
                            (x.Phone1.Equals(vendorModel.Phone1) ||
                            x.Email.ToLower().Equals(vendorModel.Email.ToLower()) ||
                            x.AccountNumber.ToLower().Equals(vendorModel.AccountNumber.ToLower())));
                if (query.Count() > 0)
                {
                    result = true;
                    MessageBoxResult resultMsg = Xceed.Wpf.Toolkit.MessageBox.Show("This vendor has existed. Please recheck Account Number, Email or Phone. Do you want to view profiles?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (MessageBoxResult.Yes.Is(resultMsg))
                    {
                        base_GuestModel guestModel = new base_GuestModel(query.FirstOrDefault());
                        if (guestModel.base_Guest.base_GuestProfile.Count > 0)
                            guestModel.PersonalInfoModel = new base_GuestProfileModel(guestModel.base_Guest.base_GuestProfile.FirstOrDefault());
                        else
                            guestModel.PersonalInfoModel = new base_GuestProfileModel();
                        ViewProfileViewModel viewProfileViewModel = new ViewProfileViewModel();
                        viewProfileViewModel.GuestModel = guestModel;
                        _dialogService.ShowDialog<ViewProfile>(_ownerViewModel, viewProfileViewModel, "View Profile");
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
            return result;
        }

        private void OnSelectedTabIndexChanged()
        {
            switch (SelectedTabIndex)
            {
                case 1: // Additional Tab
                    break;
                case 2: // Product Vendor Tab
                    // Load product by selected vendor id
                    LoadProductVendorCollection(SelectedVendor);
                    break;
                case 3: // Purchase Order History Tab
                    // Load purchase order collection
                    LoadPurchaseOrderCollection(SelectedVendor);
                    break;
            }
        }

        /// <summary>
        /// Event Tick for search ching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void _waitingTimer_Tick(object sender, EventArgs e)
        {
            _timerCounter++;
            if (_timerCounter == Define.DelaySearching)
            {
                OnSearchCommandExecute(null);
                _waitingTimer.Stop();
            }
        }

        /// <summary>
        /// Reset timer for Auto complete search
        /// </summary>
        protected virtual void ResetTimer()
        {
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                this._waitingTimer.Stop();
                this._waitingTimer.Start();
                _timerCounter = 0;
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Load data when open form
        /// </summary>
        public override void LoadData()
        {
            if (SelectedVendor != null && !SelectedVendor.IsNew)
            {
                lock (UnitOfWork.Locker)
                {
                    // Refresh static data
                    CategoryList = null;
                    UOMList = null;
                    GuestGroupCollection = null;

                    // Load static data
                    LoadStaticData();
                }

                SelectedVendor.AddressCollection = null;
                SelectedVendor.PhotoCollection = null;
                SelectedVendor.AdditionalModel = null;
                SelectedVendor.ProductCollection = null;
                SelectedVendor.PurchaseOrderCollection = null;
                SelectedVendor.ToModelAndRaise();
                SelectedVendor.EndUpdate();
                LoadRelationVendorData(SelectedVendor);

                // Load data at selected tab
                OnSelectedTabIndexChanged();

                // Reload IsManualGenerate value from configuration
                OnPropertyChanged(() => IsManualGenerate);
            }

            // Load custom field collection
            LoadCustomFieldCollection();

            // Load data by predicate
            LoadDataByPredicate(true);
        }

        /// <summary>
        /// Switch to search mode
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ShowNotification(null))
                {
                    if (!isList)
                    {
                        NewVendor();
                        IsSearchMode = false;
                    }
                    else
                        IsSearchMode = true;
                }
            }
            else
            {
                try
                {
                    Guid vendorGuid = new Guid();
                    if (Guid.TryParse(param.ToString(), out vendorGuid))
                    {
                        // Get vendor from product collection if product view is opened
                        base_GuestModel vendorModel = VendorCollection.SingleOrDefault(x => x.Resource.Equals(vendorGuid));

                        if (vendorModel == null)
                        {
                            // Get product from database if product is not loaded
                            vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Resource.HasValue && x.Resource.Value.Equals(vendorGuid)));
                        }

                        if (vendorModel != null)
                        {
                            // Display detail grid
                            IsSearchMode = true;

                            // Load relation collection
                            OnDoubleClickViewCommandExecute(vendorModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        /// <summary>
        /// Show notification when exit or change form
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ShowNotification(isClosing);
        }

        #endregion

        #region Save Image

        /// <summary>
        /// Get product image folder
        /// </summary>
        private string IMG_PRODUCT_DIRECTORY = Path.Combine(Define.CONFIGURATION.DefautlImagePath, "Product");

        /// <summary>
        /// Get vendor image folder
        /// </summary>
        private string IMG_VENDOR_DIRECTORY = Path.Combine(Define.CONFIGURATION.DefautlImagePath, "Vendor");

        /// <summary>
        /// Copy image to server folder
        /// </summary>
        /// <param name="model"></param>
        private void SaveImage(base_ResourcePhotoModel model)
        {
            try
            {
                // Server image path
                string imgGuestDirectory = Path.Combine(IMG_VENDOR_DIRECTORY, SelectedVendor.GuestNo);

                // Create folder image on server if is not exist
                if (!Directory.Exists(imgGuestDirectory))
                    Directory.CreateDirectory(imgGuestDirectory);

                // Check client image to copy to server
                FileInfo clientFileInfo = new FileInfo(model.ImagePath);
                if (clientFileInfo.Exists)
                {
                    // Get file name image
                    string serverFileName = Path.Combine(imgGuestDirectory, model.LargePhotoFilename);
                    FileInfo serverFileInfo = new FileInfo(serverFileName);
                    if (!serverFileInfo.Exists)
                        clientFileInfo.CopyTo(serverFileName, true);
                    model.ImagePath = serverFileName;
                }
                else
                    model.ImagePath = string.Empty;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region Note Module

        /// <summary>
        /// Initial resource note repository
        /// </summary>
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();

        /// <summary>
        /// Get or sets the StickyManagementViewModel
        /// </summary>
        public PopupStickyViewModel StickyManagementViewModel { get; set; }

        /// <summary>
        /// Load resource note collection
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadResourceNoteCollection(base_GuestModel guestModel)
        {
            // Load resource note collection
            if (guestModel.ResourceNoteCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        #endregion

        #region IDragSource Members

        public void StartDrag(DragInfo dragInfo)
        {
            IEnumerable<base_GuestModel> sourceItems = dragInfo.SourceItems.Cast<base_GuestModel>();

            if (sourceItems.Count() == 1)
            {
                dragInfo.Data = sourceItems.SingleOrDefault().Resource.Value;

                dragInfo.Effects = (dragInfo.Data != null) ? DragDropEffects.Move : DragDropEffects.None;
            }
        }

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(DropInfo dropInfo)
        {

        }

        #endregion
    }
}