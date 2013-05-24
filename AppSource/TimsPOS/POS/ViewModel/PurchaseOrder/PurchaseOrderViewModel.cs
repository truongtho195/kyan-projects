using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;
using CPCToolkitExt.DataGridControl;

namespace CPC.POS.ViewModel
{
    public class PurchaseOrderViewModel : ViewModelBase
    {
        #region Enums

        private enum TabItems
        {
            Order = 0,
            Receive = 1,
            Payment = 2,
            Return = 3
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets data on a separate thread.
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Column on purchase order table used for sort.
        /// </summary>
        private readonly string _purchaseOrderColumnSort = "It.Id";

        /// <summary>
        /// Holds old slected PurchaseOrder.
        /// </summary>
        private base_PurchaseOrderModel _oldPurchaseOrder;

        /// <summary>
        /// PurchaseOrder predicate.
        /// </summary>
        Expression<Func<base_PurchaseOrder, bool>> _predicate = PredicateBuilder.True<base_PurchaseOrder>();

        /// <summary>
        /// Determine whether used PO advance search.
        /// </summary>
        private bool _hasUsedAdvanceSearch = false;

        /// <summary>
        /// Holds index of old current TabItem.
        /// </summary>
        private int _oldCurrentTabItem;

        /// <summary>
        /// Contains product collection used for auto add.
        /// </summary>
        private IEnumerable<base_ProductModel> _productCollectionOutSide;

        #endregion

        #region Contructors

        public PurchaseOrderViewModel(bool isSearchMode, object param)
        {
            if (param == null)
            {
                IsSearchMode = isSearchMode;
            }
            else
            {
                _productCollectionOutSide = param as IEnumerable<base_ProductModel>;

                IsSearchMode = false;
            }

            _ownerViewModel = App.Current.MainWindow.DataContext;

            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunWorkerCompleted);
        }

        #endregion

        #region Properties

        #region IsSearchMode

        private bool _isSearchMode;
        /// <summary>
        /// Gets a value indicates whether search component is open.
        /// </summary>
        public bool IsSearchMode
        {
            get
            {
                return _isSearchMode;
            }
            private set
            {
                if (_isSearchMode != value)
                {
                    _isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }

        #endregion

        #region HasSearchPurchaseOrderNo

        private bool _hasSearchPurchaseOrderNo;
        /// <summary>
        /// Gets or sets HasSearchPurchaseOrderNo.
        /// </summary>
        public bool HasSearchPurchaseOrderNo
        {
            get
            {
                return _hasSearchPurchaseOrderNo;
            }
            set
            {
                if (_hasSearchPurchaseOrderNo != value)
                {
                    _hasSearchPurchaseOrderNo = value;
                    OnPropertyChanged(() => HasSearchPurchaseOrderNo);
                    OnHasSearchPurchaseOrderNoChanged();
                }
            }
        }

        #endregion

        #region HasSearchVendor

        private bool _hasSearchVendor;
        /// <summary>
        /// Gets or sets HasSearchVendor.
        /// </summary>
        public bool HasSearchVendor
        {
            get
            {
                return _hasSearchVendor;
            }
            set
            {
                if (_hasSearchVendor != value)
                {
                    _hasSearchVendor = value;
                    OnPropertyChanged(() => HasSearchVendor);
                    OnHasSearchVendorChanged();
                }
            }
        }

        #endregion

        #region Keyword

        private string _keyword;
        /// <summary>
        /// Gets or sets Keyword used for search PurchaseOrder.
        /// </summary>
        public string Keyword
        {
            get
            {
                return _keyword;
            }
            set
            {
                if (_keyword != value)
                {
                    _keyword = value;
                    OnPropertyChanged(() => Keyword);
                }
            }
        }

        #endregion

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

        #region StoreCollection

        private CollectionBase<base_StoreModel> _storeCollection;
        /// <summary>
        /// Gets or sets StoreCollection.
        /// </summary>
        public CollectionBase<base_StoreModel> StoreCollection
        {
            get
            {
                return _storeCollection;
            }
            set
            {
                if (_storeCollection != value)
                {
                    _storeCollection = value;
                    OnPropertyChanged(() => StoreCollection);
                }
            }
        }

        #endregion

        #region PurchaseOrderCollection

        private CollectionBase<base_PurchaseOrderModel> _purchaseOrderCollection;
        /// <summary>
        /// Gets PurchaseOrderCollection.
        /// </summary>
        public CollectionBase<base_PurchaseOrderModel> PurchaseOrderCollection
        {
            get
            {
                return _purchaseOrderCollection;
            }
            private set
            {
                if (_purchaseOrderCollection != value)
                {
                    _purchaseOrderCollection = value;
                    OnPropertyChanged(() => PurchaseOrderCollection);
                }
            }
        }

        #endregion

        #region PurchaseOrderTotal

        private int _purchaseOrderTotal;
        /// <summary>
        /// Gets PurchaseOrderTotal.
        /// </summary>
        public int PurchaseOrderTotal
        {
            get
            {
                return _purchaseOrderTotal;
            }
            private set
            {
                if (_purchaseOrderTotal != value)
                {
                    _purchaseOrderTotal = value;
                    OnPropertyChanged(() => PurchaseOrderTotal);
                }
            }
        }

        #endregion

        #region SelectedPurchaseOrder

        private base_PurchaseOrderModel _selectedPurchaseOrder;
        /// <summary>
        /// Gets or sets SelectedPurchaseOrder.
        /// </summary>
        public base_PurchaseOrderModel SelectedPurchaseOrder
        {
            get
            {
                return _selectedPurchaseOrder;
            }
            set
            {
                if (_selectedPurchaseOrder != value)
                {
                    _selectedPurchaseOrder = value;
                    OnPropertyChanged(() => SelectedPurchaseOrder);
                }
            }
        }

        #endregion

        #region ProductCollection

        private CollectionBase<base_ProductModel> _productCollection;
        /// <summary>
        /// Gets or sets ProductCollection.
        /// </summary>
        public CollectionBase<base_ProductModel> ProductCollection
        {
            get
            {
                return _productCollection;
            }
            set
            {
                if (_productCollection != value)
                {
                    _productCollection = value;
                    OnPropertyChanged(() => ProductCollection);
                }
            }
        }

        #endregion

        #region ProductFieldCollection

        private DataSearchCollection _productFieldCollection;
        /// <summary>
        /// Gets ProductFieldCollection.
        /// </summary>
        public DataSearchCollection ProductFieldCollection
        {
            get
            {
                return _productFieldCollection;
            }
            private set
            {
                if (_productFieldCollection != value)
                {
                    _productFieldCollection = value;
                    OnPropertyChanged(() => ProductFieldCollection);
                }
            }
        }

        #endregion

        #region PurchaseOrderDetailFieldCollection

        private DataSearchCollection _purchaseOrderDetailFieldCollection;
        /// <summary>
        /// Gets PurchaseOrderDetailFieldCollection.
        /// </summary>
        public DataSearchCollection PurchaseOrderDetailFieldCollection
        {
            get
            {
                return _purchaseOrderDetailFieldCollection;
            }
            private set
            {
                if (_purchaseOrderDetailFieldCollection != value)
                {
                    _purchaseOrderDetailFieldCollection = value;
                    OnPropertyChanged(() => PurchaseOrderDetailFieldCollection);
                }
            }
        }

        #endregion

        #region SelectedProduct

        private base_ProductModel _selectedProduct;
        /// <summary>
        /// Gets or sets SelectedProduct.
        /// </summary>
        public base_ProductModel SelectedProduct
        {
            get
            {
                return _selectedProduct;
            }
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(() => SelectedProduct);
                    OnSelectedProductChanged();
                }
            }
        }

        #endregion

        #region SelectedPurchaseOrderDetail

        private base_PurchaseOrderDetailModel _selectedPurchaseOrderDetail;
        /// <summary>
        /// Gets or sets SelectedPurchaseOrderDetail.
        /// </summary>
        public base_PurchaseOrderDetailModel SelectedPurchaseOrderDetail
        {
            get
            {
                return _selectedPurchaseOrderDetail;
            }
            set
            {
                if (_selectedPurchaseOrderDetail != value)
                {
                    _selectedPurchaseOrderDetail = value;
                    OnPropertyChanged(() => SelectedPurchaseOrderDetail);
                }
            }
        }

        #endregion

        #region SelectedPurchaseOrderReceive

        private object _selectedPurchaseOrderReceive;
        /// <summary>
        /// Gets or sets SelectedPurchaseOrderReceive.
        /// </summary>
        public object SelectedPurchaseOrderReceive
        {
            get
            {
                return _selectedPurchaseOrderReceive;
            }
            set
            {
                if (_selectedPurchaseOrderReceive != value)
                {
                    _selectedPurchaseOrderReceive = value;
                    OnPropertyChanged(() => SelectedPurchaseOrderReceive);
                }
            }
        }

        #endregion

        #region SelectedResourceReturnDetail

        private object _selectedResourceReturnDetail;
        /// <summary>
        /// Gets or sets SelectedResourceReturnDetail.
        /// </summary>
        public object SelectedResourceReturnDetail
        {
            get
            {
                return _selectedResourceReturnDetail;
            }
            set
            {
                if (_selectedResourceReturnDetail != value)
                {
                    _selectedResourceReturnDetail = value;
                    OnPropertyChanged(() => SelectedResourceReturnDetail);
                    OnSelectedResourceReturnDetailChanged();
                }
            }
        }

        #endregion

        #region CurrentTabItem

        private int _currentTabItem;
        /// <summary>
        /// Gets or sets CurrentTabItem.
        /// </summary>
        public int CurrentTabItem
        {
            get
            {
                return _currentTabItem;
            }
            set
            {
                if (_currentTabItem != value)
                {
                    _oldCurrentTabItem = _currentTabItem;
                    _currentTabItem = value;
                    OnPropertyChanged(() => CurrentTabItem);
                    OnCurrentTabItemChanged();
                }
            }
        }

        #endregion

        #region PaymentMethodCollection

        private ObservableCollection<ComboItem> _paymentMethodCollection;
        /// <summary>
        /// Gets or sets PaymentMethodCollection.
        /// </summary>
        public ObservableCollection<ComboItem> PaymentMethodCollection
        {
            get
            {
                return _paymentMethodCollection;
            }
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

        #endregion

        #region Command Properties

        #region OpenSearchComponentCommand

        private ICommand _openSearchComponentCommand;
        /// <summary>
        /// When 'Search' Button clicked, OpenSearchComponentCommand will executes.
        /// </summary>
        public ICommand OpenSearchComponentCommand
        {
            get
            {
                if (_openSearchComponentCommand == null)
                {
                    _openSearchComponentCommand = new RelayCommand(OpenSearchComponentExecute);
                }
                return _openSearchComponentCommand;
            }
        }

        #endregion

        #region CloseSearchComponentCommand

        private ICommand _closeSearchComponentCommand;
        /// <summary>
        /// When double clicked on DataGridRow in DataGrid, CloseSearchComponentCommand will executes.
        /// </summary>
        public ICommand CloseSearchComponentCommand
        {
            get
            {
                if (_closeSearchComponentCommand == null)
                {
                    _closeSearchComponentCommand = new RelayCommand(CloseSearchComponentExecute, CanCloseSearchComponentExecute);
                }
                return _closeSearchComponentCommand;
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
                    _addVendorCommand = new RelayCommand(AddVendorExecute, CanAddVendorExecute);
                }
                return _addVendorCommand;
            }
        }

        #endregion

        #region GetPurchaseOrdersCommand

        private ICommand _getPurchaseOrdersCommand;
        /// <summary>
        /// When DataGrid scroll, command will executes.
        /// </summary>
        public ICommand GetPurchaseOrdersCommand
        {
            get
            {
                if (_getPurchaseOrdersCommand == null)
                {
                    _getPurchaseOrdersCommand = new RelayCommand(GetPurchaseOrdersExecute);
                }
                return _getPurchaseOrdersCommand;
            }
        }

        #endregion

        #region AddTermCommand

        private ICommand _addTermCommand;
        /// <summary>
        /// When 'Add Term' Button clicked, AddTermCommand will executes.
        /// </summary>
        public ICommand AddTermCommand
        {
            get
            {
                if (_addTermCommand == null)
                {
                    _addTermCommand = new RelayCommand(AddTermExecute, CanAddTermExecute);
                }
                return _addTermCommand;
            }
        }

        #endregion

        #region DeletePurchaseOrderDetailCommand

        private ICommand _deletePurchaseOrderDetailCommand;
        /// <summary>
        /// When 'Del' key pressed while DataGrid focused. DeletePurchaseOrderDetailCommand will executes.
        /// </summary>
        public ICommand DeletePurchaseOrderDetailCommand
        {
            get
            {
                if (_deletePurchaseOrderDetailCommand == null)
                {
                    _deletePurchaseOrderDetailCommand = new RelayCommand(DeletePurchaseOrderDetailExecute, CanDeletePurchaseOrderDetailExecute);
                }
                return _deletePurchaseOrderDetailCommand;
            }
        }

        #endregion

        #region DeletePurchaseOrderReceiveCommand

        private ICommand _deletePurchaseOrderReceiveCommand;
        /// <summary>
        /// When 'Del' key pressed while DataGrid focused. DeletePurchaseOrderReceiveCommand will executes.
        /// </summary>
        public ICommand DeletePurchaseOrderReceiveCommand
        {
            get
            {
                if (_deletePurchaseOrderReceiveCommand == null)
                {
                    _deletePurchaseOrderReceiveCommand = new RelayCommand(DeletePurchaseOrderReceiveExecute, CanDeletePurchaseOrderReceiveExecute);
                }
                return _deletePurchaseOrderReceiveCommand;
            }
        }

        #endregion

        #region DeleteResourceReturnDetailCommand

        private ICommand _deleteResourceReturnDetailCommand;
        /// <summary>
        /// When 'Del' key pressed while DataGrid focused. DeleteResourceReturnDetailCommand will executes.
        /// </summary>
        public ICommand DeleteResourceReturnDetailCommand
        {
            get
            {
                if (_deleteResourceReturnDetailCommand == null)
                {
                    _deleteResourceReturnDetailCommand = new RelayCommand(DeleteResourceReturnDetailExecute, CanDeleteResourceReturnDetailExecute);
                }
                return _deleteResourceReturnDetailCommand;
            }
        }

        #endregion

        #region QuantityChangedCommand

        private ICommand _quantityChangedCommand;
        /// <summary>
        /// When Quantity property of purchase order detail changed, QuantityChangedCommand will executes.
        /// </summary>
        public ICommand QuantityChangedCommand
        {
            get
            {
                if (_quantityChangedCommand == null)
                {
                    _quantityChangedCommand = new RelayCommand(QuantityChangedExecute);
                }
                return _quantityChangedCommand;
            }
        }

        #endregion

        #region NewCommand

        private ICommand _newCommand;
        /// <summary>
        /// When 'New' button clicked, NewCommand will executes. 
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

        #region EditCommand

        private ICommand _editCommand;
        /// <summary>
        /// When 'Edit' MenuItem clicked, EditCommand will executes. 
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand<DataGridControl>(EditExecute, CanEditExecute);
                }
                return _editCommand;
            }
        }

        #endregion

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' button clicked, SaveCommand will executes. 
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
        /// When 'Delete' button clicked, DeleteCommand will executes. 
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

        #region DeletesCommand

        private ICommand _deletesCommand;
        /// <summary>
        /// When 'Delete' MenuItem clicked, DeletesCommand will executes. 
        /// </summary>
        public ICommand DeletesCommand
        {
            get
            {
                if (_deletesCommand == null)
                {
                    _deletesCommand = new RelayCommand<DataGridControl>(DeletesExecute, CanDeletesExecute);
                }
                return _deletesCommand;
            }
        }

        #endregion

        #region DuplicateCommand

        private ICommand _duplicateCommand;
        /// <summary>
        /// When 'Duplicate' MenuItem clicked, DuplicateCommand will executes. 
        /// </summary>
        public ICommand DuplicateCommand
        {
            get
            {
                if (_duplicateCommand == null)
                {
                    _duplicateCommand = new RelayCommand<DataGridControl>(DuplicateExecute, CanDuplicateExecute);
                }
                return _duplicateCommand;
            }
        }

        #endregion

        #region SearchProductAdvanceCommand

        private ICommand _searchProductAdvanceCommand;
        /// <summary>
        /// When 'Advance Search' button clicked, SearchProductAdvanceCommand will executes.
        /// </summary>
        public ICommand SearchProductAdvanceCommand
        {
            get
            {
                if (_searchProductAdvanceCommand == null)
                {
                    _searchProductAdvanceCommand = new RelayCommand(SearchProductAdvanceExecute, CanSearchProductAdvanceExecute);
                }
                return _searchProductAdvanceCommand;
            }
        }

        #endregion

        #region SearchPOCommand

        private ICommand _searchPOCommand;
        /// <summary>
        /// When 'Enter' key pressed while 'Search' TextBox focused, SearchPOCommand will executes.
        /// </summary>
        public ICommand SearchPOCommand
        {
            get
            {
                if (_searchPOCommand == null)
                {
                    _searchPOCommand = new RelayCommand(SearchPOExecute);
                }
                return _searchPOCommand;
            }
        }

        #endregion

        #region SearchPOAdvanceCommand

        private ICommand _searchPOAdvanceCommand;
        /// <summary>
        /// When 'Advance Search' button clicked, SearchPOAdvanceCommand will executes.
        /// </summary>
        public ICommand SearchPOAdvanceCommand
        {
            get
            {
                if (_searchPOAdvanceCommand == null)
                {
                    _searchPOAdvanceCommand = new RelayCommand(SearchPOAdvanceExecute);
                }
                return _searchPOAdvanceCommand;
            }
        }

        #endregion

        #region ShowReceiveItemCommand

        private ICommand _showReceiveItemCommand;
        /// <summary>
        /// When 'Receive' button clicked, ShowReceiveItemCommand will executes. 
        /// </summary>
        public ICommand ShowReceiveItemCommand
        {
            get
            {
                if (_showReceiveItemCommand == null)
                {
                    _showReceiveItemCommand = new RelayCommand(ShowReceiveItemExecute, CanShowReceiveItemExecute);
                }
                return _showReceiveItemCommand;
            }
        }

        #endregion

        #region ReceiveAllCommand

        private ICommand _receiveAllCommand;
        /// <summary>
        /// When 'Receive All' button clicked, ReceiveAllCommand will executes. 
        /// </summary>
        public ICommand ReceiveAllCommand
        {
            get
            {
                if (_receiveAllCommand == null)
                {
                    _receiveAllCommand = new RelayCommand(ReceiveAllExecute, CanReceiveAllExecute);
                }
                return _receiveAllCommand;
            }
        }

        #endregion

        #region ReturnAllCommand

        private ICommand _returnAllCommand;
        /// <summary>
        /// When 'Return All' button clicked, ReturnAllCommand will executes. 
        /// </summary>
        public ICommand ReturnAllCommand
        {
            get
            {
                if (_returnAllCommand == null)
                {
                    _returnAllCommand = new RelayCommand(ReturnAllExecute, CanReturnAllExecute);
                }
                return _returnAllCommand;
            }
        }

        #endregion

        #region EditProductCommand

        private ICommand _editProductCommand;
        /// <summary>
        /// When 'Edit' MenuItem clicked, EditProductCommand will executes. 
        /// </summary>
        public ICommand EditProductCommand
        {
            get
            {
                if (_editProductCommand == null)
                {
                    _editProductCommand = new RelayCommand(EditProductExecute, CanEditProductExecute);
                }
                return _editProductCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OpenSearchComponentExecute

        /// <summary>
        /// Open search component.
        /// </summary>
        private void OpenSearchComponentExecute()
        {
            OpenSearchComponent();
        }

        #endregion

        #region CloseSearchComponentExecute

        /// <summary>
        /// Close search component.
        /// </summary>
        private void CloseSearchComponentExecute()
        {
            CloseSearchComponent();
        }

        #endregion

        #region CanCloseSearchComponentExecute

        /// <summary>
        /// Determine whether can call CloseSearchComponentExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanCloseSearchComponentExecute()
        {
            return true;
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

        #region CanAddVendorExecute

        /// <summary>
        /// Determine whether can call AddVendorExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanAddVendorExecute()
        {
            if (_vendorCollection == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region GetPurchaseOrdersExecute

        /// <summary>
        /// Gets range of purchase orders.
        /// </summary>
        private void GetPurchaseOrdersExecute()
        {
            if (!IsBusy)
            {
                _backgroundWorker.RunWorkerAsync("LoadNext");
            }
        }

        #endregion

        #region AddTermExecute

        /// <summary>
        /// Add a term.
        /// </summary>
        private void AddTermExecute()
        {
            AddTerm();
        }

        #endregion

        #region CanAddTermExecute

        /// <summary>
        /// Determine whether can call AddTermExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanAddTermExecute()
        {
            if (_selectedPurchaseOrder == null || _selectedPurchaseOrder.IsFullWorkflow)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeletePurchaseOrderDetailExecute

        /// <summary>
        /// Delete purchase detail item.
        /// </summary>
        private void DeletePurchaseOrderDetailExecute()
        {
            DeletePurchaseOrderDetail();
        }

        #endregion

        #region CanDeletePurchaseOrderDetailExecute

        /// <summary>
        /// Determine whether can call DeletePurchaseOrderDetailExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanDeletePurchaseOrderDetailExecute()
        {
            if (_selectedPurchaseOrderDetail == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeletePurchaseOrderReceiveExecute

        /// <summary>
        /// Delete purchase receive item.
        /// </summary>
        private void DeletePurchaseOrderReceiveExecute()
        {
            DeletePurchaseOrderReceive();
        }

        #endregion

        #region CanDeletePurchaseOrderReceiveExecute

        /// <summary>
        /// Determine whether can call DeletePurchaseOrderReceiveExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanDeletePurchaseOrderReceiveExecute()
        {
            if (_selectedPurchaseOrderReceive == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeleteResourceReturnDetailExecute

        /// <summary>
        /// Delete resoure return item.
        /// </summary>
        private void DeleteResourceReturnDetailExecute()
        {
            DeleteResourceReturnDetail();
        }

        #endregion

        #region CanDeleteResourceReturnDetailExecute

        /// <summary>
        /// Determine whether can call DeleteResourceReturnDetailExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanDeleteResourceReturnDetailExecute()
        {
            if (_selectedResourceReturnDetail == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region QuantityChangedExecute

        /// <summary>
        /// Calls when Quantity property of purchase order detail changed.
        /// </summary>
        private void QuantityChangedExecute()
        {
            if (_selectedPurchaseOrderDetail.Quantity != _selectedPurchaseOrderDetail.BackupQuantity)
            {
                _selectedPurchaseOrderDetail.BackupQuantity = _selectedPurchaseOrderDetail.Quantity;
                AddSerials(_selectedPurchaseOrderDetail, false);
            }
        }

        #endregion

        #region NewExecute

        /// <summary>
        /// Create new purchase order.
        /// </summary>
        private void NewExecute()
        {
            CreateNewPurchaseOrder();
        }

        #endregion

        #region CanNewExecute

        /// <summary>
        /// Determine whether can call NewExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanNewExecute()
        {
            if (_selectedPurchaseOrder != null && _selectedPurchaseOrder.IsNew)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region EditExecute

        /// <summary>
        /// Edit current selected purchase order.
        /// </summary>
        private void EditExecute(DataGridControl dataGrid)
        {
            CloseSearchComponent();
        }

        #endregion

        #region CanEditExecute

        /// <summary>
        /// Determine whether can call EditExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanEditExecute(DataGridControl dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItems.Count != 1)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region SaveExecute

        /// <summary>
        /// Save purchase order.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Determine whether can call SaveExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanSaveExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.HasError ||
                _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.HasError) ||
                _selectedPurchaseOrder.ResourceReturnDetailCollection.Any(x => x.HasError) || (
                !_selectedPurchaseOrder.IsDirty &&
                !_selectedPurchaseOrder.PurchaseOrderDetailCollection.IsDirty &&
                !_selectedPurchaseOrder.PurchaseOrderReceiveCollection.IsDirty &&
                !_selectedPurchaseOrder.ResourceReturnDetailCollection.IsDirty))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeleteExecute

        /// <summary>
        /// Delete PurchaseOrder.
        /// </summary>
        private void DeleteExecute()
        {
            Delete();
        }

        #endregion

        #region CanDeleteExecute

        /// <summary>
        /// Determine whether can call DeleteExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanDeleteExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.IsNew ||
                _selectedPurchaseOrder.Status != (short)PurchaseStatus.Open)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeletesExecute

        /// <summary>
        /// Delete multi PurchaseOrder.
        /// </summary>
        private void DeletesExecute(DataGridControl dataGrid)
        {
            Deletes(dataGrid);
        }

        #endregion

        #region CanDeletesExecute

        /// <summary>
        /// Determine whether can call DeletesExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanDeletesExecute(DataGridControl dataGrid)
        {
            if (dataGrid == null ||
                dataGrid.SelectedItems.Count <= 0 ||
                (dataGrid.SelectedItems.Cast<base_PurchaseOrderModel>()).Any(x => x.Status != (short)PurchaseStatus.Open))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DuplicateExecute

        /// <summary>
        /// Duplicate a PurchaseOrder.
        /// </summary>
        private void DuplicateExecute(DataGridControl dataGrid)
        {
            Duplicate(dataGrid);
        }

        #endregion

        #region CanDuplicateExecute

        /// <summary>
        /// Determine whether can call DuplicateExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanDuplicateExecute(DataGridControl dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItems.Count != 1)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region SearchProductAdvanceExecute

        /// <summary>
        /// Search product with advance options..
        /// </summary>
        private void SearchProductAdvanceExecute()
        {
            SearchProductAdvance();
        }

        #endregion

        #region CanSearchProductAdvanceExecute

        /// <summary>
        /// Determine whether can call SearchProductAdvanceExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanSearchProductAdvanceExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.IsFullWorkflow ||
                _selectedPurchaseOrder.PurchaseOrderDetailCollection == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region SearchPOExecute

        /// <summary>
        /// Search PurchaseOrder when 'enter' key pressed.
        /// </summary>
        private void SearchPOExecute()
        {
            SearchPO();
        }

        #endregion

        #region SearchPOAdvanceExecute

        /// <summary>
        /// Search PurchaseOrder with advance options.
        /// </summary>
        private void SearchPOAdvanceExecute()
        {
            SearchPOAdvance();
        }

        #endregion

        #region ShowReceiveItemExecute

        /// <summary>
        /// Show 'Receive' TabItem.
        /// </summary>
        private void ShowReceiveItemExecute()
        {
            ShowReceiveItem();
        }

        #endregion

        #region CanShowReceiveItemExecute

        /// <summary>
        /// Determine whether can call ShowReceiveItemExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanShowReceiveItemExecute()
        {
            if (_selectedPurchaseOrder == null || _selectedPurchaseOrder.IsNew)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ReceiveAllExecute

        /// <summary>
        /// Received all items.
        /// </summary>
        private void ReceiveAllExecute()
        {
            ReceiveAll();
        }

        #endregion

        #region CanReceiveAllExecute

        /// <summary>
        /// Determine whether can call ReceiveAllExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanReceiveAllExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.IsFullWorkflow ||
                _selectedPurchaseOrder.PurchaseOrderDetailCollection == null ||
                _selectedPurchaseOrder.PurchaseOrderReceiveCollection == null ||
                _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.HasError))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ReturnAllExecute

        /// <summary>
        /// Returned all items.
        /// </summary>
        private void ReturnAllExecute()
        {
            ReturnAll();
        }

        #endregion

        #region CanReturnAllExecute

        /// <summary>
        /// Determine whether can call ReturnAllExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanReturnAllExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.PurchaseOrderDetailCollection == null ||
                _selectedPurchaseOrder.ResourceReturnDetailCollection == null ||
                _selectedPurchaseOrder.ResourceReturnDetailCollection.Any(x => x.HasError))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region EditProductExecute

        /// <summary>
        /// Edit product.
        /// </summary>
        private void EditProductExecute()
        {
            EditProduct();
        }

        #endregion

        #region CanEditProductExecute

        /// <summary>
        /// Determine whether can call EditProductExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanEditProductExecute()
        {
            if (_selectedPurchaseOrderDetail == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnCurrentTabItemChanged

        /// <summary>
        /// Occurs when CurrentTabItem property changed.
        /// </summary>
        private void OnCurrentTabItemChanged()
        {
            ChangeTabItem();
        }

        #endregion

        #region OnSelectedProductChanged

        /// <summary>
        /// Occurs when SelectedProduct property changed.
        /// </summary>
        private void OnSelectedProductChanged()
        {
            AddPurchaseOrderDetail();
        }

        #endregion

        #region OnSelectedResourceReturnDetailChanged

        /// <summary>
        /// Occurs when SelectedResourceReturnDetail property changed.
        /// </summary>
        private void OnSelectedResourceReturnDetailChanged()
        {
            base_ResourceReturnDetailModel selectedResourceReturnDetail = _selectedResourceReturnDetail as base_ResourceReturnDetailModel;
            if (selectedResourceReturnDetail == null)
            {
                return;
            }

            CheckReturned();

            if (selectedResourceReturnDetail.PurchaseOrderDetail != null &&
                !_selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Contains(selectedResourceReturnDetail.PurchaseOrderDetail))
            {
                _selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Add(selectedResourceReturnDetail.PurchaseOrderDetail);
            }
        }

        #endregion

        #region OnHasSearchPurchaseOrderNoChanged

        /// <summary>
        /// Occurs when HasSearchPurchaseOrderNo property changed.
        /// </summary>
        private void OnHasSearchPurchaseOrderNoChanged()
        {
            // Search again.
            SearchPO();
        }

        #endregion

        #region OnHasSearchVendorChanged

        /// <summary>
        /// Occurs when HasSearchVendor property changed.
        /// </summary>
        private void OnHasSearchVendorChanged()
        {
            // Search again.
            SearchPO();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            // Avoids to change value of selected PurchaseOrder when loading data.
            if (_selectedPurchaseOrder != null)
            {
                SelectedPurchaseOrder = null;
            }

            InitPaymentMethodCollection();
            GetVendors();
            GetStores();
            GetProducts();
            CreateProductFields();
            CreatePurchaseOrderDetailFields();

            // Gets all purchase orders.
            _backgroundWorker.RunWorkerAsync("Load");
        }

        #endregion

        #region GetVendors

        /// <summary>
        /// Gets all vendors.
        /// </summary>
        private void GetVendors()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_GuestRepository guestRepository = new base_GuestRepository();
                    string vendorType = MarkType.Vendor.ToDescription();
                    VendorCollection = new CollectionBase<base_GuestModel>(guestRepository.GetAll(x =>
                        x.IsActived && x.Mark == vendorType).Select(x => new base_GuestModel(guestRepository.Refresh(x))));
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                _selectedPurchaseOrder.VendorId = newVendor.Id;
            }
        }

        #endregion

        #region GetStores

        /// <summary>
        /// Gets all stores.
        /// </summary>
        private void GetStores()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_StoreRepository storeRepository = new base_StoreRepository();
                    StoreCollection = new CollectionBase<base_StoreModel>(storeRepository.GetAll().Select(x =>
                        new base_StoreModel(storeRepository.Refresh(x))).OrderBy(x => x.Id));
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region InitPaymentMethodCollection

        /// <summary>
        /// Initialize PaymentMethodCollection.
        /// </summary>
        private void InitPaymentMethodCollection()
        {
            PaymentMethodCollection = new ObservableCollection<ComboItem>(Common.PaymentMethods.Where(x =>
                (Define.CONFIGURATION.AcceptedPaymentMethod & x.Value) == x.Value).OrderBy(x => x.Text));
        }

        #endregion

        #region GetPurchaseOrders

        /// <summary>
        /// Gets range of purchase orders.
        /// </summary>
        private void GetPurchaseOrders(string argument)
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_PurchaseOrderRepository purchaseOrderRepository = new base_PurchaseOrderRepository();

                    if (argument != "LoadNext")
                    {
                        if (!_hasUsedAdvanceSearch)
                        {
                            _predicate = PredicateBuilder.True<base_PurchaseOrder>();
                            _predicate = _predicate.And(x => !x.IsPurge);

                            if (_hasSearchPurchaseOrderNo && !string.IsNullOrWhiteSpace(_keyword))
                            {
                                _predicate = _predicate.And(x => x.PurchaseOrderNo.ToLower().Contains(_keyword.ToLower()));
                            }

                            if (_hasSearchVendor && !string.IsNullOrWhiteSpace(_keyword))
                            {
                                IEnumerable<long> vendorIDList = _vendorCollection.Where(x =>
                                    x.Mark == MarkType.Vendor.ToDescription() &&
                                    x.Company != null &&
                                    x.Company.ToLower().Contains(_keyword.ToLower())).Select(x => x.Id);
                                _predicate = _predicate.And(x => vendorIDList.Contains(x.VendorId));
                            }
                        }

                        // Initialize PurchaseOrderCollection.
                        PurchaseOrderCollection = new CollectionBase<base_PurchaseOrderModel>();
                        PurchaseOrderTotal = purchaseOrderRepository.GetIQueryable(_predicate).Count();
                    }

                    IList<base_PurchaseOrder> purchaseOrders = purchaseOrderRepository.GetRange(_purchaseOrderCollection.Count, NumberOfDisplayItems, _purchaseOrderColumnSort, _predicate);
                    foreach (base_PurchaseOrder purchaseOrder in purchaseOrders)
                    {
                        if (purchaseOrderRepository.Refresh(purchaseOrder) != null)
                        {
                            _backgroundWorker.ReportProgress(0, purchaseOrder);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetPurchaseOrder

        /// <summary>
        /// Get a purchase order.
        /// </summary>
        private void GetPurchaseOrder(base_PurchaseOrder purchaseOrder)
        {
            if (_purchaseOrderCollection.FirstOrDefault(x => x.Id == purchaseOrder.Id) == null)
            {
                base_PurchaseOrderModel purchaseOrderModel = new base_PurchaseOrderModel(purchaseOrder);
                purchaseOrderModel.ResourceReturn = new base_ResourceReturnModel();
                purchaseOrderModel.ResourcePayment = new base_ResourcePaymentModel();
                purchaseOrderModel.ResourcePaymentDetail = new base_ResourcePaymentDetailModel();
                purchaseOrderModel.PurchaseOrderDetailCollection = new CollectionBase<base_PurchaseOrderDetailModel>();
                purchaseOrderModel.PurchaseOrderReceiveCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
                purchaseOrderModel.PurchaseOrderPaymentCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
                purchaseOrderModel.ResourceReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
                if (_vendorCollection != null)
                {
                    // Gets selected vendor.
                    base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Id == purchaseOrderModel.VendorId);
                    if (vendor != null)
                    {
                        // Gets VendorName..
                        purchaseOrderModel.VendorName = vendor.Company;
                    }
                }
                purchaseOrderModel.PropertyChanged += PurchaseOrderPropertyChanged;
                purchaseOrderModel.IsNew = false;
                purchaseOrderModel.IsDirty = false;
                _purchaseOrderCollection.Add(purchaseOrderModel);
            }
        }

        #endregion

        #region CreateNewPurchaseOrder

        /// <summary>
        /// Create new purchase order.
        /// </summary>
        private void CreateNewPurchaseOrder()
        {
            if (SaveNotify())
            {
                CommitOrCancelChange();
                IsSearchMode = false;
                CreatePurchaseOrder();
            }
        }

        #endregion

        #region CreatePurchaseOrder

        /// <summary>
        /// Create new purchase order.
        /// </summary>
        private void CreatePurchaseOrder()
        {
            base_StoreModel store = _storeCollection.ElementAt(Define.StoreCode);
            base_PurchaseOrderModel purchaseOrder = new base_PurchaseOrderModel();
            purchaseOrder.PurchaseOrderNo = DateTime.Now.ToString(Define.PurchaseOrderNoFormat);
            purchaseOrder.PurchasedDate = DateTime.Now.Date;
            purchaseOrder.ShipDate = DateTime.Now.Date;
            purchaseOrder.PaymentDueDate = purchaseOrder.ShipDate;
            purchaseOrder.StoreCode = Define.StoreCode;
            purchaseOrder.ShipAddress = store != null ? string.Format("{0}. {1}", store.City, store.Street) : null;
            purchaseOrder.Resource = Guid.NewGuid();
            purchaseOrder.ResourceReturn = new base_ResourceReturnModel
            {
                DocumentResource = purchaseOrder.Resource.ToString(),
                DocumentNo = purchaseOrder.PurchaseOrderNo,
                TotalAmount = purchaseOrder.Total,
                Resource = Guid.NewGuid(),
                Mark = OrderMarkType.PurchaseOrder.ToDescription(),
                IsDirty = false
            };
            purchaseOrder.ResourceReturn.PropertyChanged += ResourceReturnPropertyChanged;
            purchaseOrder.ResourcePayment = new base_ResourcePaymentModel
            {
                DocumentResource = purchaseOrder.Resource.ToString(),
                DocumentNo = purchaseOrder.PurchaseOrderNo,
                DateCreated = DateTime.Now.Date,
                Resource = Guid.NewGuid(),
                Mark = OrderMarkType.PurchaseOrder.ToDescription()
            };
            purchaseOrder.ResourcePayment.PropertyChanged += ResourcePaymentPropertyChanged;
            purchaseOrder.ResourcePaymentDetail = new base_ResourcePaymentDetailModel();
            purchaseOrder.ResourcePaymentDetail.PaymentType = "P";
            purchaseOrder.ResourcePaymentDetail.ResourcePaymentId = purchaseOrder.ResourcePayment.Id;
            purchaseOrder.ResourcePaymentDetail.PaymentMethodId = Define.CONFIGURATION.DefaultPaymentMethod.HasValue ? Define.CONFIGURATION.DefaultPaymentMethod.Value : _paymentMethodCollection.First().Value;
            purchaseOrder.ResourcePaymentDetail.PaymentMethod = _paymentMethodCollection.FirstOrDefault(x => x.Value == purchaseOrder.ResourcePaymentDetail.PaymentMethodId).Text;
            purchaseOrder.ResourcePaymentDetail.Paid = purchaseOrder.ResourcePayment.TotalPaid;
            purchaseOrder.ResourcePaymentDetail.PropertyChanged += ResourcePaymentDetailPropertyChanged;
            purchaseOrder.PurchaseOrderDetailCollection = new CollectionBase<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderDetailReceiveCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderDetailReturnCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderReceiveCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
            purchaseOrder.PurchaseOrderReceiveCollection.CollectionChanged += PurchaseOrderReceiveCollectionChanged;
            purchaseOrder.PurchaseOrderPaymentCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
            purchaseOrder.ResourceReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
            purchaseOrder.ResourceReturnDetailCollection.CollectionChanged += ResourceReturnDetailCollectionChanged;
            purchaseOrder.PropertyChanged += PurchaseOrderPropertyChanged;
            purchaseOrder.IsDirty = false;
            purchaseOrder.IsNew = true;
            _purchaseOrderCollection.Add(purchaseOrder);
            SelectedPurchaseOrder = purchaseOrder;

            // Select default TabItem.
            _currentTabItem = (int)TabItems.Order;
            OnPropertyChanged(() => CurrentTabItem);
        }

        #endregion

        #region GetProducts

        /// <summary>
        /// Gets all products.
        /// </summary>
        private void GetProducts()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_ProductRepository productRepository = new Repository.base_ProductRepository();
                    ProductCollection = new CollectionBase<base_ProductModel>(productRepository.GetAll().Select(x =>
                        new base_ProductModel(x)));
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region CreateProductFields

        /// <summary>
        /// Create product's fields used for search product.
        /// </summary>
        private void CreateProductFields()
        {
            ProductFieldCollection = new DataSearchCollection
            {
                new DataSearchModel { ID = 1, Level = 0, DisplayName = "Code", KeyName = "Code" },
                new DataSearchModel { ID = 2, Level = 0, DisplayName = "Product Name", KeyName = "ProductName" },
                new DataSearchModel { ID = 3, Level = 0, DisplayName = "Attribute", KeyName = "Attribute" },
                new DataSearchModel { ID = 4, Level = 0, DisplayName = "Size", KeyName = "Size" },
                new DataSearchModel { ID = 5, Level = 0, DisplayName = "Barcode", KeyName = "Barcode" }
            };
        }

        #endregion

        #region CreatePurchaseOrderDetailFields

        /// <summary>
        /// Create PurchaseOrderDetail's fields used for search PurchaseOrderDetail.
        /// </summary>
        private void CreatePurchaseOrderDetailFields()
        {
            PurchaseOrderDetailFieldCollection = new DataSearchCollection
            {
                new DataSearchModel { ID = 1, Level = 0, DisplayName = "Code", KeyName = "ItemCode" },
                new DataSearchModel { ID = 2, Level = 0, DisplayName = "Product Name", KeyName = "ItemName" },
                new DataSearchModel { ID = 3, Level = 0, DisplayName = "Attribute", KeyName = "ItemAtribute" },
                new DataSearchModel { ID = 4, Level = 0, DisplayName = "Size", KeyName = "ItemSize" },
            };
        }

        #endregion

        #region AddTerm

        /// <summary>
        /// Add a term.
        /// </summary>
        private void AddTerm()
        {
            short dueDays = _selectedPurchaseOrder.TermNetDue;
            decimal discount = _selectedPurchaseOrder.TermDiscountPercent;
            short discountDays = _selectedPurchaseOrder.TermPaidWithinDay;
            PaymentTermViewModel paymentTermViewModel = new PaymentTermViewModel(dueDays, discount, discountDays);
            bool? dialogResult = _dialogService.ShowDialog<PaymentTermView>(_ownerViewModel, paymentTermViewModel, "Add Term");
            if (dialogResult == true)
            {
                _selectedPurchaseOrder.TermNetDue = paymentTermViewModel.DueDays;
                _selectedPurchaseOrder.TermDiscountPercent = paymentTermViewModel.Discount;
                _selectedPurchaseOrder.TermPaidWithinDay = paymentTermViewModel.DiscountDays;
                _selectedPurchaseOrder.PaymentTermDescription = paymentTermViewModel.Description;

                // Gets selected vendor.
                base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Id == _selectedPurchaseOrder.VendorId);
                if (vendor != null)
                {
                    // Update TermNetDue, TermDiscountPercent, TermPaidWithinDay, PaymentTermDescription.
                    vendor.TermNetDue = _selectedPurchaseOrder.TermNetDue;
                    vendor.TermDiscount = _selectedPurchaseOrder.TermDiscountPercent;
                    vendor.TermPaidWithinDay = _selectedPurchaseOrder.TermPaidWithinDay;
                    vendor.PaymentTermDescription = _selectedPurchaseOrder.PaymentTermDescription;
                }
            }
        }

        #endregion

        #region AddPurchaseOrderDetail

        /// <summary>
        /// Add a purchase order detail.
        /// </summary>
        private void AddPurchaseOrderDetail()
        {
            if (_selectedProduct == null)
            {
                return;
            }

            MessageBoxResult result = MessageBoxResult.Yes;
            if (_selectedProduct.IsUnOrderAble)
            {
                result = MessageBox.Show(string.Format("Product name: {0} is marked  as 'Unorderable' in inventory. Are you sure you want to add this item ?", _selectedProduct.ProductName), "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }

            if (result == MessageBoxResult.No)
            {
                SelectedProduct = null;
                return;
            }

            base_UOMRepository UOMRepository = new base_UOMRepository();
            base_ProductUOMModel productUOM;
            base_PurchaseOrderDetailModel purchaseOrderDetail = new base_PurchaseOrderDetailModel();
            purchaseOrderDetail.UOMCollection = GetUOMCollection(_selectedProduct.base_Product);
            if (purchaseOrderDetail.UOMCollection == null)
            {
                purchaseOrderDetail.UOMCollection = new CollectionBase<base_ProductUOMModel>();
            }
            purchaseOrderDetail.PurchaseOrderId = _selectedPurchaseOrder.Id;
            purchaseOrderDetail.PurchaseOrder = _selectedPurchaseOrder;
            purchaseOrderDetail.ProductResource = _selectedProduct.Resource.ToString();
            purchaseOrderDetail.ItemCode = _selectedProduct.Code;
            purchaseOrderDetail.ItemName = _selectedProduct.ProductName;
            purchaseOrderDetail.ItemAtribute = _selectedProduct.Attribute;
            purchaseOrderDetail.ItemSize = _selectedProduct.Size;
            purchaseOrderDetail.IsSerialTracking = _selectedProduct.IsSerialTracking;
            if (_selectedProduct.OrderUOMId.HasValue)
            {
                productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == _selectedProduct.OrderUOMId);
            }
            else
            {
                productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == _selectedProduct.BaseUOMId);
            }
            if (productUOM != null)
            {
                purchaseOrderDetail.UOMId = productUOM.UOMId;
                purchaseOrderDetail.UnitName = productUOM.Name;
                purchaseOrderDetail.BaseUOM = productUOM.Code;
                purchaseOrderDetail.Price = productUOM.RegularPrice;
            }
            else
            {
                purchaseOrderDetail.Price = 0;
            }

            base_PurchaseOrderDetailModel purchaseOrderDetailContainProductBefore = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                x.ProductResource == purchaseOrderDetail.ProductResource);
            if (purchaseOrderDetailContainProductBefore != null)
            {
                purchaseOrderDetail.OnHandQty = purchaseOrderDetailContainProductBefore.OnHandQty;
                purchaseOrderDetail.OnHandQtyTemp = purchaseOrderDetailContainProductBefore.OnHandQtyTemp;
            }
            else
            {
                purchaseOrderDetail.OnHandQty = GetOnHandQty(_selectedPurchaseOrder.StoreCode, _selectedProduct);
                purchaseOrderDetail.OnHandQtyTemp = purchaseOrderDetail.OnHandQty;
            }
            purchaseOrderDetail.Quantity = 1;
            purchaseOrderDetail.BackupQuantity = 1;
            purchaseOrderDetail.DueQty = 1;
            purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * purchaseOrderDetail.Price;
            purchaseOrderDetail.Resource = Guid.NewGuid();

            purchaseOrderDetail.PropertyChanged += PurchaseOrderDetailPropertyChanged;
            purchaseOrderDetail.IsNew = true;
            _selectedPurchaseOrder.PurchaseOrderDetailCollection.Add(purchaseOrderDetail);
            _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Add(purchaseOrderDetail);

            AddSerials(purchaseOrderDetail, true);

            CalculateTotalForPurchaseOrder();

            // Calculate order quantity of purchase order.
            CalculateOrderQtyOfPurchaseOrder();

            SelectedProduct = null;

            // Determine status.
            if (_selectedPurchaseOrder.Status == (short)PurchaseStatus.FullyReceived)
            {
                _selectedPurchaseOrder.Status = (short)PurchaseStatus.Receiving;
            }
        }

        /// <summary>
        /// Add a purchase order detail.
        /// </summary>
        /// <param name="product">Product to add on purchase order detail.</param>
        private void AddPurchaseOrderDetail(base_ProductModel product)
        {
            if (product == null)
            {
                return;
            }

            MessageBoxResult result = MessageBoxResult.Yes;
            if (product.IsUnOrderAble)
            {
                result = MessageBox.Show(string.Format("Product name: {0} is marked  as 'Unorderable' in inventory. Are you sure you want to add this item ?", product.ProductName), "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }

            if (result == MessageBoxResult.No)
            {
                return;
            }

            base_UOMRepository UOMRepository = new base_UOMRepository();
            base_ProductUOMModel productUOM;
            base_PurchaseOrderDetailModel purchaseOrderDetail = new base_PurchaseOrderDetailModel();
            purchaseOrderDetail.UOMCollection = GetUOMCollection(product.base_Product);
            if (purchaseOrderDetail.UOMCollection == null)
            {
                purchaseOrderDetail.UOMCollection = new CollectionBase<base_ProductUOMModel>();
            }
            purchaseOrderDetail.PurchaseOrderId = _selectedPurchaseOrder.Id;
            purchaseOrderDetail.PurchaseOrder = _selectedPurchaseOrder;
            purchaseOrderDetail.ProductResource = product.Resource.ToString();
            purchaseOrderDetail.ItemCode = product.Code;
            purchaseOrderDetail.ItemName = product.ProductName;
            purchaseOrderDetail.ItemAtribute = product.Attribute;
            purchaseOrderDetail.ItemSize = product.Size;
            purchaseOrderDetail.IsSerialTracking = product.IsSerialTracking;
            if (product.OrderUOMId.HasValue)
            {
                productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == product.OrderUOMId);
            }
            else
            {
                productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == product.BaseUOMId);
            }
            if (productUOM != null)
            {
                purchaseOrderDetail.UOMId = productUOM.UOMId;
                purchaseOrderDetail.UnitName = productUOM.Name;
                purchaseOrderDetail.BaseUOM = productUOM.Code;
                purchaseOrderDetail.Price = productUOM.RegularPrice;
            }
            else
            {
                purchaseOrderDetail.Price = 0;
            }

            base_PurchaseOrderDetailModel purchaseOrderDetailContainProductBefore = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                x.ProductResource == purchaseOrderDetail.ProductResource);
            if (purchaseOrderDetailContainProductBefore != null)
            {
                purchaseOrderDetail.OnHandQty = purchaseOrderDetailContainProductBefore.OnHandQty;
                purchaseOrderDetail.OnHandQtyTemp = purchaseOrderDetailContainProductBefore.OnHandQtyTemp;
            }
            else
            {
                purchaseOrderDetail.OnHandQty = GetOnHandQty(_selectedPurchaseOrder.StoreCode, product);
                purchaseOrderDetail.OnHandQtyTemp = purchaseOrderDetail.OnHandQty;
            }
            purchaseOrderDetail.Quantity = 1;
            purchaseOrderDetail.BackupQuantity = 1;
            purchaseOrderDetail.DueQty = 1;
            purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * purchaseOrderDetail.Price;
            purchaseOrderDetail.Resource = Guid.NewGuid();

            purchaseOrderDetail.PropertyChanged += PurchaseOrderDetailPropertyChanged;
            purchaseOrderDetail.IsNew = true;
            _selectedPurchaseOrder.PurchaseOrderDetailCollection.Add(purchaseOrderDetail);
            _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Add(purchaseOrderDetail);

            CalculateTotalForPurchaseOrder();

            // Calculate order quantity of purchase order.
            CalculateOrderQtyOfPurchaseOrder();
        }

        #endregion

        #region GetUOMCollection

        /// <summary>
        /// Gets unit collection of product.
        /// </summary>
        /// <param name="product">Product to gets unit collection.</param>
        /// <returns>Unit collection.</returns>
        private CollectionBase<base_ProductUOMModel> GetUOMCollection(base_Product product)
        {
            base_UOMRepository UOMRepository = new base_UOMRepository();
            CollectionBase<base_ProductUOMModel> UOMCollection = new CollectionBase<base_ProductUOMModel>();

            try
            {
                lock (UnitOfWork.Locker)
                {
                    // Add base unit in UOMCollection.
                    base_UOM UOM = UOMRepository.Get(x => x.Id == product.BaseUOMId);
                    if (UOM != null)
                    {
                        UOMCollection.Add(new base_ProductUOMModel
                        {
                            ProductId = product.Id,
                            UOMId = UOM.Id,
                            Code = UOM.Code,
                            Name = UOM.Name,
                            RegularPrice = product.RegularPrice,
                            IsNew = false,
                            IsDirty = false
                        });
                    }

                    // Gets the remaining units.
                    foreach (base_ProductUOM item in product.base_ProductUOM)
                    {
                        UOMCollection.Add(new base_ProductUOMModel(item)
                        {
                            Code = item.base_UOM.Code,
                            Name = item.base_UOM.Name,
                            IsDirty = false
                        });
                    }


                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return UOMCollection;
        }

        #endregion

        #region GetOnHandQty

        /// <summary>
        /// Gets on-hand quantity in stock.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="product">Product has inventory information.</param>
        /// <returns>On-hand quantity.</returns>
        private int GetOnHandQty(int index, base_ProductModel product)
        {
            if (product == null)
            {
                return -1;
            }

            switch (index)
            {
                case 0:
                    return product.OnHandStore1;

                case 1:
                    return product.OnHandStore2;

                case 2:
                    return product.OnHandStore3;

                case 3:
                    return product.OnHandStore4;

                case 4:
                    return product.OnHandStore5;

                case 5:
                    return product.OnHandStore6;

                case 6:
                    return product.OnHandStore7;

                case 7:
                    return product.OnHandStore8;

                case 8:
                    return product.OnHandStore9;

                case 9:
                    return product.OnHandStore10;

                default:
                    return -3;
            }
        }

        #endregion

        #region AddSerials

        /// <summary>
        ///  Add serials.
        /// </summary>
        /// <param name="isShowQuantity">True will show quantity TextBox.</param>
        private void AddSerials(base_PurchaseOrderDetailModel purchaseOrderDetail, bool isShowQuantity)
        {
            if (purchaseOrderDetail.IsSerialTracking && purchaseOrderDetail.Quantity > 0)
            {
                //Show Tracking Serial
                SelectTrackingNumberViewModel selectTrackingNumberViewModel = new SelectTrackingNumberViewModel(purchaseOrderDetail, isShowQuantity);
                bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, selectTrackingNumberViewModel, "Tracking Serial Number");
                if (result == true)
                {
                    purchaseOrderDetail = selectTrackingNumberViewModel.PurchaseOrderDetailModel;
                }
            }
        }

        /// <summary>
        /// Add serials.
        /// </summary>
        /// <param name="purchaseOrderDetailList">PurchaseOrderDetail list to add serial.</param>
        private void AddSerials(IEnumerable<base_PurchaseOrderDetailModel> purchaseOrderDetailList)
        {
            MultiTrackingNumberViewModel multiTrackingNumberViewModel = new MultiTrackingNumberViewModel(purchaseOrderDetailList);
            bool? result = _dialogService.ShowDialog<MultiTrackingNumberView>(_ownerViewModel, multiTrackingNumberViewModel, "Multi Tracking Serial");
        }

        #endregion

        #region DeletePurchaseOrderDetail

        /// <summary>
        /// Delete purchase detail item.
        /// </summary>
        private void DeletePurchaseOrderDetail()
        {
            // Check item received.
            if (_selectedPurchaseOrderDetail.HasReceivedItem)
            {
                MessageBox.Show("Exists an item has been received in this purchase order, can not delete purchase order.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Try to find PurchaseOrderDetail error.
            base_PurchaseOrderDetailModel purchaseOrderDetailError = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x => x.HasError);
            bool isContainsErrorItem = false;
            if (purchaseOrderDetailError != null)
            {
                isContainsErrorItem = object.ReferenceEquals(_selectedPurchaseOrderDetail, purchaseOrderDetailError);
            }

            if (purchaseOrderDetailError == null || isContainsErrorItem)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item(s)?", "Delete item(s)", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Holds PurchaseOrderDetail's resource.
                    string id = _selectedPurchaseOrderDetail.Resource.ToString();
                    base_PurchaseOrderDetailModel purchaseOrderDetailDelete = _selectedPurchaseOrderDetail;

                    _selectedPurchaseOrder.PurchaseOrderDetailCollection.Remove(purchaseOrderDetailDelete);
                    _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Remove(purchaseOrderDetailDelete);

                    CalculateTotalForPurchaseOrder();

                    // Calculate order quantity of purchase order.
                    CalculateOrderQtyOfPurchaseOrder();

                    // Determine status.
                    if (_selectedPurchaseOrder.Status < (short)PurchaseStatus.FullyReceived)
                    {
                        if (_selectedPurchaseOrder.PurchaseOrderDetailCollection.Count > 0 &&
                            !_selectedPurchaseOrder.PurchaseOrderDetailCollection.Any(x => !x.IsFullReceived))
                        {
                            _selectedPurchaseOrder.Status = (short)PurchaseStatus.FullyReceived;
                        }
                    }

                    // Delete all PurchaseOrderReceives of PurchaseOrderDetail.
                    List<base_PurchaseOrderReceiveModel> purchaseOrderReceivelist = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                        x.PODResource == id).ToList();
                    foreach (base_PurchaseOrderReceiveModel item in purchaseOrderReceivelist)
                    {
                        _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Remove(item);
                    }

                    CheckFullWorkflow();
                }
            }
        }

        #endregion

        #region DeletePurchaseOrderReceive

        /// <summary>
        /// Delete purchase receive item.
        /// </summary>
        private void DeletePurchaseOrderReceive()
        {
            base_PurchaseOrderReceiveModel selectedPurchaseOrderReceive = _selectedPurchaseOrderReceive as base_PurchaseOrderReceiveModel;
            if (selectedPurchaseOrderReceive == null || selectedPurchaseOrderReceive.IsTemporary)
            {
                return;
            }

            if (selectedPurchaseOrderReceive.IsReceived)
            {
                MessageBox.Show("Item has been received in this purchase order, can not delete this item.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Try to find PurchaseOrderReceive error.
            base_PurchaseOrderReceiveModel purchaseOrderReceiveError = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.FirstOrDefault(x => x.HasError);
            bool isContainsErrorItem = false;
            if (purchaseOrderReceiveError != null)
            {
                ListCollectionView purchaseOrderReceiveView = CollectionViewSource.GetDefaultView(_selectedPurchaseOrder.PurchaseOrderReceiveCollection) as ListCollectionView;
                if (purchaseOrderReceiveView != null)
                {
                    if (purchaseOrderReceiveView.CurrentEditItem != null)
                    {
                        isContainsErrorItem = object.ReferenceEquals(purchaseOrderReceiveView.CurrentEditItem, selectedPurchaseOrderReceive);
                    }
                }
            }

            if (purchaseOrderReceiveError == null || isContainsErrorItem)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item(s)?", "Delete item(s)", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Remove(selectedPurchaseOrderReceive);

                    // Check full received.
                    CheckFullReceived(selectedPurchaseOrderReceive.PurchaseOrderDetail);

                    // Calculate total receive of purchase order.
                    CalculateTotalReceiveOfPurchaseOrder();
                }
            }
        }

        #endregion

        #region DeleteResourceReturnDetail

        /// <summary>
        /// Delete resoure return item.
        /// </summary>
        private void DeleteResourceReturnDetail()
        {
            base_ResourceReturnDetailModel selectedResourceReturnDetail = _selectedResourceReturnDetail as base_ResourceReturnDetailModel;
            if (selectedResourceReturnDetail == null || selectedResourceReturnDetail.IsTemporary)
            {
                return;
            }

            if (selectedResourceReturnDetail.IsReturned)
            {
                MessageBox.Show("Item has been returned in this purchase order, can not delete this item.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Try to find ResourceReturnDetail error.
            base_ResourceReturnDetailModel resourceReturnDetailError = _selectedPurchaseOrder.ResourceReturnDetailCollection.FirstOrDefault(x => x.HasError);
            bool isContainsErrorItem = false;
            if (resourceReturnDetailError != null)
            {
                ListCollectionView resourceReturnDetailView = CollectionViewSource.GetDefaultView(_selectedPurchaseOrder.ResourceReturnDetailCollection) as ListCollectionView;
                if (resourceReturnDetailView != null)
                {
                    if (resourceReturnDetailView.CurrentEditItem != null)
                    {
                        isContainsErrorItem = object.ReferenceEquals(resourceReturnDetailView.CurrentEditItem, selectedResourceReturnDetail);
                    }
                }
            }

            if (resourceReturnDetailError == null || isContainsErrorItem)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item(s)?", "Delete item(s)", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _selectedPurchaseOrder.ResourceReturnDetailCollection.Remove(selectedResourceReturnDetail);
                    CalculateSubTotalForResourceReturn();
                }
            }
        }

        #endregion

        #region SearchProductAdvance

        /// <summary>
        /// Search product with advance options..
        /// </summary>
        private void SearchProductAdvance()
        {
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel();
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, "Search Product");
            if (dialogResult == true)
            {
                SelectedProduct = productSearchViewModel.SelectedProduct;
            }
        }

        #endregion

        #region SearchPO

        /// <summary>
        /// Search PurchaseOrder.
        /// </summary>
        private void SearchPO()
        {
            if (_hasUsedAdvanceSearch)
            {
                _hasUsedAdvanceSearch = false;
            }
            _backgroundWorker.RunWorkerAsync("Load");
        }

        #endregion

        #region SearchPOAdvance

        /// <summary>
        /// Search PurchaseOrder with advance options.
        /// </summary>
        private void SearchPOAdvance()
        {
            POAdvanceSearchViewModel POAdvanceSearchViewModel = new POAdvanceSearchViewModel();
            bool? dialogResult = _dialogService.ShowDialog<POAdvanceSearchView>(_ownerViewModel, POAdvanceSearchViewModel, "Search Purchase Orders");
            if (dialogResult == true)
            {
                // Reset search.
                Keyword = null;
                _hasSearchPurchaseOrderNo = false;
                _hasSearchVendor = false;
                OnPropertyChanged(() => HasSearchPurchaseOrderNo);
                OnPropertyChanged(() => HasSearchVendor);

                _hasUsedAdvanceSearch = true;
                _predicate = POAdvanceSearchViewModel.Predicate;
                _backgroundWorker.RunWorkerAsync("Load");
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save purchase order.
        /// </summary>
        private void Save()
        {
            try
            {
                UnitOfWork.BeginTransaction();

                base_PurchaseOrderRepository purchaseOrderRepository = new base_PurchaseOrderRepository();
                base_PurchaseOrderDetailRepository purchaseOrderDetailRepository = new base_PurchaseOrderDetailRepository();
                base_PurchaseOrderReceiveRepository purchaseOrderReceiveRepository = new base_PurchaseOrderReceiveRepository();
                base_GuestRepository guestRepository = new base_GuestRepository();
                base_ProductRepository productRepository = new base_ProductRepository();
                base_ResourceReturnRepository resourceReturnRepository = new base_ResourceReturnRepository();
                base_ResourceReturnDetailRepository resourceReturnDetailRepository = new base_ResourceReturnDetailRepository();
                base_ResourcePaymentRepository resourcePaymentRepository = new base_ResourcePaymentRepository();
                base_ResourcePaymentDetailRepository resourcePaymentDetailRepository = new base_ResourcePaymentDetailRepository();
                DateTime now = DateTime.Now;

                // Update vendor.
                // Gets selected vendor.
                base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Id == _selectedPurchaseOrder.VendorId);
                if (vendor != null && vendor.IsDirty)
                {
                    vendor.DateUpdated = now;
                    vendor.ToEntity();
                    guestRepository.Commit();
                    vendor.IsDirty = false;
                }

                if (_selectedPurchaseOrder.IsNew)
                {
                    // Insert PurchaseOrder.
                    _selectedPurchaseOrder.DateCreated = now;
                    _selectedPurchaseOrder.ToEntity();
                    purchaseOrderRepository.Add(_selectedPurchaseOrder.base_PurchaseOrder);
                    purchaseOrderRepository.Commit();
                    _selectedPurchaseOrder.Id = _selectedPurchaseOrder.base_PurchaseOrder.Id;
                    _selectedPurchaseOrder.IsNew = false;
                    _selectedPurchaseOrder.IsDirty = false;

                    // Insert PurchaseOrderDetail.
                    foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
                    {
                        item.PurchaseOrderId = _selectedPurchaseOrder.Id;
                        item.ToEntity();
                        purchaseOrderDetailRepository.Add(item.base_PurchaseOrderDetail);
                        purchaseOrderDetailRepository.Commit();
                        item.Id = item.base_PurchaseOrderDetail.Id;
                        item.IsNew = false;
                        item.IsDirty = false;
                    }
                }
                else
                {
                    // Update PurchaseOrder.
                    _selectedPurchaseOrder.DateUpdate = now;
                    _selectedPurchaseOrder.ToEntity();
                    purchaseOrderRepository.Commit();
                    _selectedPurchaseOrder.IsDirty = false;

                    // Insert new items on PurchaseOrderDetail. 
                    ObservableCollection<base_PurchaseOrderDetailModel> newItems = _selectedPurchaseOrder.PurchaseOrderDetailCollection.NewItems;
                    if (newItems.Count > 0)
                    {
                        foreach (base_PurchaseOrderDetailModel item in newItems)
                        {
                            item.PurchaseOrderId = _selectedPurchaseOrder.Id;
                            item.ToEntity();
                            purchaseOrderDetailRepository.Add(item.base_PurchaseOrderDetail);
                            purchaseOrderDetailRepository.Commit();
                            item.Id = item.base_PurchaseOrderDetail.Id;
                            item.IsNew = false;
                            item.IsDirty = false;
                        }
                    }

                    // Update dirty items on PurchaseOrderDetail. 
                    ObservableCollection<base_PurchaseOrderDetailModel> dirtyItems = _selectedPurchaseOrder.PurchaseOrderDetailCollection.DirtyItems;
                    if (dirtyItems.Count > 0)
                    {
                        foreach (base_PurchaseOrderDetailModel item in dirtyItems)
                        {
                            item.ToEntity();
                            purchaseOrderDetailRepository.Commit();
                            item.IsDirty = false;
                        }
                    }

                    // Insert new items on PurchaseOrderReceive. 
                    ObservableCollection<base_PurchaseOrderReceiveModel> newPORS = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.NewItems;
                    if (newPORS.Count > 0)
                    {
                        foreach (base_PurchaseOrderReceiveModel item in newPORS)
                        {
                            if (item.PurchaseOrderDetailId <= 0)
                            {
                                item.PurchaseOrderDetailId = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x => x.Resource.ToString() == item.PODResource).Id;
                            }
                            item.ToEntity();
                            purchaseOrderReceiveRepository.Add(item.base_PurchaseOrderReceive);
                            purchaseOrderReceiveRepository.Commit();
                            item.Id = item.base_PurchaseOrderReceive.Id;
                            item.IsNew = false;
                            item.IsDirty = false;
                        }
                    }

                    // Update dirty items on PurchaseOrderReceive. 
                    ObservableCollection<base_PurchaseOrderReceiveModel> dirtyPORS = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.DirtyItems;
                    if (dirtyPORS.Count > 0)
                    {
                        foreach (base_PurchaseOrderReceiveModel item in dirtyPORS)
                        {
                            item.ToEntity();
                            purchaseOrderReceiveRepository.Commit();
                            item.IsDirty = false;
                        }
                    }

                    // Delete deleted items on PurchaseOrderReceive.
                    ObservableCollection<base_PurchaseOrderReceiveModel> deletedPORS = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.DeletedItems;
                    if (deletedPORS.Count > 0)
                    {
                        foreach (base_PurchaseOrderReceiveModel item in deletedPORS)
                        {
                            purchaseOrderReceiveRepository.Delete(item.base_PurchaseOrderReceive);
                            purchaseOrderReceiveRepository.Commit();
                        }

                        deletedPORS.Clear();
                    }

                    // Delete deleted items on PurchaseOrderDetail.
                    ObservableCollection<base_PurchaseOrderDetailModel> deletedItems = _selectedPurchaseOrder.PurchaseOrderDetailCollection.DeletedItems;
                    if (deletedItems.Count > 0)
                    {
                        foreach (base_PurchaseOrderDetailModel item in deletedItems)
                        {
                            purchaseOrderDetailRepository.Delete(item.base_PurchaseOrderDetail);
                            purchaseOrderDetailRepository.Commit();
                        }

                        deletedItems.Clear();
                    }

                    // Insert new ResourcePayment.
                    if (_selectedPurchaseOrder.ResourcePayment.IsNew ||
                        _selectedPurchaseOrder.ResourcePayment.TotalPaid > _selectedPurchaseOrder.ResourcePayment.base_ResourcePayment.TotalPaid)
                    {
                        // Insert new ResourcePayment.
                        if (_selectedPurchaseOrder.ResourcePayment.Id > 0)
                        {
                            _selectedPurchaseOrder.ResourcePayment.Id = 0;
                            _selectedPurchaseOrder.ResourcePayment.NewResourcePaymentEntity();
                        }
                        _selectedPurchaseOrder.ResourcePayment.UserCreated = Define.USER.LoginName;
                        _selectedPurchaseOrder.ResourcePayment.ToEntity();
                        resourcePaymentRepository.Add(_selectedPurchaseOrder.ResourcePayment.base_ResourcePayment);
                        resourcePaymentRepository.Commit();
                        _selectedPurchaseOrder.ResourcePayment.Id = _selectedPurchaseOrder.ResourcePayment.base_ResourcePayment.Id;
                        _selectedPurchaseOrder.ResourcePayment.IsNew = false;
                        _selectedPurchaseOrder.ResourcePayment.IsDirty = false;
                    }
                    else
                    {
                        // Update new ResourcePayment.
                        if (_selectedPurchaseOrder.ResourcePayment.IsDirty)
                        {
                            _selectedPurchaseOrder.ResourcePayment.ToEntity();
                            resourcePaymentRepository.Commit();
                            _selectedPurchaseOrder.ResourcePayment.IsDirty = false;
                        }
                    }

                    // Insert new ResourcePaymentDetail.
                    if (_selectedPurchaseOrder.ResourcePaymentDetail.IsNew)
                    {
                        // Insert new ResourcePaymentDetail.
                        _selectedPurchaseOrder.ResourcePaymentDetail.ResourcePaymentId = _selectedPurchaseOrder.ResourcePayment.Id;
                        _selectedPurchaseOrder.ResourcePaymentDetail.ToEntity();
                        resourcePaymentDetailRepository.Add(_selectedPurchaseOrder.ResourcePaymentDetail.base_ResourcePaymentDetail);
                        resourcePaymentRepository.Commit();
                        _selectedPurchaseOrder.ResourcePaymentDetail.Id = _selectedPurchaseOrder.ResourcePaymentDetail.base_ResourcePaymentDetail.Id;
                        _selectedPurchaseOrder.ResourcePaymentDetail.IsNew = false;
                        _selectedPurchaseOrder.ResourcePaymentDetail.IsDirty = false;
                    }
                    else
                    {
                        // Update new ResourcePaymentDetail.
                        if (_selectedPurchaseOrder.ResourcePaymentDetail.IsDirty)
                        {
                            _selectedPurchaseOrder.ResourcePaymentDetail.ResourcePaymentId = _selectedPurchaseOrder.ResourcePayment.Id;
                            _selectedPurchaseOrder.ResourcePaymentDetail.ToEntity();
                            resourcePaymentDetailRepository.Commit();
                            _selectedPurchaseOrder.ResourcePaymentDetail.IsDirty = false;
                        }
                    }

                    // Insert new ResourceReturn.
                    if (_selectedPurchaseOrder.ResourceReturn.IsNew)
                    {
                        // Insert new ResourceReturn.
                        if (_selectedPurchaseOrder.ResourceReturnDetailCollection.Any())
                        {
                            _selectedPurchaseOrder.ResourceReturn.DateCreated = now;
                            _selectedPurchaseOrder.ResourceReturn.UserCreated = Define.USER.LoginName;
                            _selectedPurchaseOrder.ResourceReturn.ToEntity();
                            resourceReturnRepository.Add(_selectedPurchaseOrder.ResourceReturn.base_ResourceReturn);
                            resourceReturnRepository.Commit();
                            _selectedPurchaseOrder.ResourceReturn.Id = _selectedPurchaseOrder.ResourceReturn.base_ResourceReturn.Id;
                            _selectedPurchaseOrder.ResourceReturn.IsNew = false;
                            _selectedPurchaseOrder.ResourceReturn.IsDirty = false;
                        }
                    }
                    else
                    {
                        // Update new ResourceReturn.
                        if (_selectedPurchaseOrder.ResourceReturn.IsDirty)
                        {
                            _selectedPurchaseOrder.ResourceReturn.ToEntity();
                            resourceReturnRepository.Commit();
                            _selectedPurchaseOrder.ResourceReturn.IsDirty = false;
                        }
                    }

                    // Insert new items on ResourceReturnDetail. 
                    ObservableCollection<base_ResourceReturnDetailModel> newRRDS = _selectedPurchaseOrder.ResourceReturnDetailCollection.NewItems;
                    if (newRRDS.Count > 0)
                    {
                        foreach (base_ResourceReturnDetailModel item in newRRDS)
                        {
                            if (item.ResourceReturnId <= 0)
                            {
                                item.ResourceReturnId = _selectedPurchaseOrder.ResourceReturn.Id;
                            }
                            item.ToEntity();
                            resourceReturnDetailRepository.Add(item.base_ResourceReturnDetail);
                            resourceReturnDetailRepository.Commit();
                            item.Id = item.base_ResourceReturnDetail.Id;
                            item.IsNew = false;
                            item.IsDirty = false;
                        }
                    }

                    // Update dirty items on ResourceReturnDetail. 
                    ObservableCollection<base_ResourceReturnDetailModel> dirtyRRDS = _selectedPurchaseOrder.ResourceReturnDetailCollection.DirtyItems;
                    if (dirtyRRDS.Count > 0)
                    {
                        foreach (base_ResourceReturnDetailModel item in dirtyRRDS)
                        {
                            item.ToEntity();
                            resourceReturnDetailRepository.Commit();
                            item.IsDirty = false;
                        }
                    }

                    // Delete deleted items on ResourceReturnDetail.
                    ObservableCollection<base_ResourceReturnDetailModel> deletedRRDS = _selectedPurchaseOrder.ResourceReturnDetailCollection.DeletedItems;
                    if (deletedRRDS.Count > 0)
                    {
                        foreach (base_ResourceReturnDetailModel item in deletedRRDS)
                        {
                            resourceReturnDetailRepository.Delete(item.base_ResourceReturnDetail);
                            resourceReturnDetailRepository.Commit();
                        }

                        deletedRRDS.Clear();
                    }

                    // Update on-hand quantity in Stock.
                    int currentOnHandQty = 0;
                    IEnumerable<base_PurchaseOrderDetailModel> productList = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Distinct(new PurchaseOrderDetailComparer());
                    foreach (base_PurchaseOrderDetailModel item in productList)
                    {
                        int increaseQty = item.OnHandQty - item.OnHandQtyTemp;
                        if (increaseQty == 0)
                        {
                            continue;
                        }
                        if (increaseQty > 0)
                        {
                            currentOnHandQty = productRepository.UpdateOnHandQuantity(item.ProductResource, _selectedPurchaseOrder.StoreCode, increaseQty);
                        }
                        else
                        {
                            currentOnHandQty = productRepository.UpdateOnHandQuantity(item.ProductResource, _selectedPurchaseOrder.StoreCode, Math.Abs(increaseQty), true);
                        }
                        productRepository.Commit();
                        foreach (base_PurchaseOrderDetailModel itemFriend in _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                            x.ProductResource == item.ProductResource))
                        {
                            itemFriend.OnHandQty = currentOnHandQty;
                            itemFriend.OnHandQtyTemp = itemFriend.OnHandQty;
                            itemFriend.IsDirty = false;
                        }
                    }
                }

                UnitOfWork.CommitTransaction();

                if (_selectedPurchaseOrder.IsFullWorkflow)
                {
                    IsSearchMode = true;
                }
            }
            catch (Exception exception)
            {
                UnitOfWork.RollbackTransaction();
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region SaveNotify

        /// <summary>
        /// Save purchase order with notification.
        /// </summary>
        /// <returns>True will unactvie.</returns>
        private bool SaveNotify()
        {
            bool isUnactive = true;

            if (_selectedPurchaseOrder == null)
            {
                return isUnactive;
            }

            // No errors.
            if (!_selectedPurchaseOrder.HasError &&
                !_selectedPurchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.HasError) &&
                !_selectedPurchaseOrder.ResourceReturnDetailCollection.Any(x => x.HasError))
            {
                if (_selectedPurchaseOrder.IsDirty ||
                    _selectedPurchaseOrder.PurchaseOrderDetailCollection.IsDirty ||
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection.IsDirty ||
                    _selectedPurchaseOrder.ResourceReturnDetailCollection.IsDirty)
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        Save();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        RestorePurchaseOrder();
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                if (!_selectedPurchaseOrder.IsNew ||
                    _selectedPurchaseOrder.IsDirty ||
                    _selectedPurchaseOrder.PurchaseOrderDetailCollection.IsDirty ||
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection.IsDirty ||
                    _selectedPurchaseOrder.ResourceReturnDetailCollection.IsDirty)
                {
                    // Quention continue.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Continue work.
                        isUnactive = false;
                    }
                    else
                    {
                        // Not continue work.
                        RestorePurchaseOrder();
                        isUnactive = true;
                    }
                }
                else
                {
                    // Not continue work.
                    RestorePurchaseOrder();
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete PurchaseOrder.
        /// </summary>
        private void Delete()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item?", "Delete item", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DeletePurchaseOrder(_selectedPurchaseOrder);
                IsSearchMode = true;
            }
        }

        #endregion

        #region Deletes

        /// <summary>
        /// Delete multi PurchaseOrder.
        /// </summary>
        private void Deletes(DataGridControl dataGrid)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item(s)?", "Delete item(s)", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                List<base_PurchaseOrderModel> selectedItems = dataGrid.SelectedItems.Cast<base_PurchaseOrderModel>().ToList();
                foreach (base_PurchaseOrderModel item in selectedItems)
                {
                    DeletePurchaseOrder(item);
                }
            }
        }

        #endregion

        #region DeletePurchaseOrder

        /// <summary>
        /// Delete a PurchaseOrder.
        /// </summary>
        private void DeletePurchaseOrder(base_PurchaseOrderModel purchaseOrder)
        {
            try
            {
                base_PurchaseOrderRepository purchaseOrderRepository = new base_PurchaseOrderRepository();
                purchaseOrder.base_PurchaseOrder.IsPurge = true;
                purchaseOrderRepository.Commit();
                purchaseOrder.IsPurge = true;
                _purchaseOrderCollection.Remove(purchaseOrder);
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Duplicate

        /// <summary>
        /// Duplicate a PurchaseOrder.
        /// </summary>
        private void Duplicate(DataGridControl dataGrid)
        {
            try
            {
                // Init Repository.
                base_PurchaseOrderDetailRepository purchaseOrderDetailRepository = new base_PurchaseOrderDetailRepository();
                base_ProductRepository productRepository = new Repository.base_ProductRepository();

                // Gets selected item.
                base_PurchaseOrderModel selectedItem = dataGrid.SelectedItem as base_PurchaseOrderModel;
                if (selectedItem == null)
                {
                    return;
                }

                // Create new PurchaseOrder.
                CreatePurchaseOrder();

                // Copy some information for new PurchaseOrder.
                _selectedPurchaseOrder.VendorId = selectedItem.VendorId;
                _selectedPurchaseOrder.StoreCode = selectedItem.StoreCode;

                // Close search component.
                IsSearchMode = false;

                // Gets products order in root PurchaseOrder.
                IList<base_PurchaseOrderDetail> purchaseOrderDetails = purchaseOrderDetailRepository.GetAll(x => x.PurchaseOrderId == selectedItem.Id);
                base_ProductModel product;
                foreach (base_PurchaseOrderDetail purchaseOrderDetail in purchaseOrderDetails)
                {
                    // Gets and refresh product of item.
                    product = _productCollection.FirstOrDefault(x => x.Resource.ToString() == purchaseOrderDetail.ProductResource);
                    productRepository.Refresh(product.base_Product);
                    product.ToModel();

                    // Add new PurchaseOrderDetail.
                    AddPurchaseOrderDetail(product);
                }

                IEnumerable<base_PurchaseOrderDetailModel> serialPurchaseOrderDetails = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                    x.IsSerialTracking);
                if (serialPurchaseOrderDetails.Any())
                {
                    AddSerials(serialPurchaseOrderDetails);
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region RestorePurchaseOrder

        /// <summary>
        /// Restore PurchaseOrder.
        /// </summary>
        private void RestorePurchaseOrder()
        {
            if (_selectedPurchaseOrder != null)
            {
                if (!_selectedPurchaseOrder.IsNew)
                {
                    if (_selectedPurchaseOrder.IsDirty)
                    {
                        _selectedPurchaseOrder.ToModelAndRaise();
                        _selectedPurchaseOrder.IsDirty = false;
                    }
                }
                else
                {
                    _purchaseOrderCollection.Remove(_selectedPurchaseOrder);
                    _selectedPurchaseOrder = null;
                }
            }
        }

        #endregion

        #region OpenSearchComponent

        /// <summary>
        /// Open search component.
        /// </summary>
        private void OpenSearchComponent()
        {
            if (SaveNotify())
            {
                IsSearchMode = true;
                CommitOrCancelChange();
            }
        }

        #endregion

        #region CloseSearchComponent

        /// <summary>
        /// Close search component.
        /// </summary>
        private void CloseSearchComponent()
        {
            IsSearchMode = false;
            GetMoreInformation();
        }

        #endregion

        #region GetMoreInformation

        /// <summary>
        /// Gets more information of purchase order.
        /// </summary>
        private void GetMoreInformation()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_ProductRepository productRepository = new Repository.base_ProductRepository();
                    base_PurchaseOrderDetailRepository purchaseOrderDetailRepository = new base_PurchaseOrderDetailRepository();
                    base_PurchaseOrderReceiveRepository purchaseOrderReceiveRepository = new base_PurchaseOrderReceiveRepository();
                    base_ResourceReturnRepository resourceReturnRepository = new base_ResourceReturnRepository();
                    base_ResourceReturnDetailRepository resourceReturnDetailRepository = new base_ResourceReturnDetailRepository();
                    base_ResourcePaymentRepository resourcePaymentRepository = new base_ResourcePaymentRepository();
                    base_ResourcePaymentDetailRepository resourcePaymentDetailRepository = new base_ResourcePaymentDetailRepository();
                    _selectedPurchaseOrder.PurchaseOrderDetailCollection = new CollectionBase<base_PurchaseOrderDetailModel>();
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
                    _selectedPurchaseOrder.ResourceReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();

                    // Gets PurchaseOrderDetails of PurchaseOrder.
                    IList<base_PurchaseOrderDetail> purchaseOrderDetails = purchaseOrderDetailRepository.GetAll(x => x.PurchaseOrderId == _selectedPurchaseOrder.Id);
                    base_PurchaseOrderDetailModel purchaseOrderDetailModel;
                    base_PurchaseOrderDetailModel purchaseOrderDetailModelBefore;
                    base_ProductModel product;
                    base_ProductUOMModel productUOM;
                    foreach (base_PurchaseOrderDetail item in purchaseOrderDetails)
                    {
                        purchaseOrderDetailRepository.Refresh(item);
                        purchaseOrderDetailModel = new base_PurchaseOrderDetailModel(item);

                        // Gets and refresh product of item.
                        product = _productCollection.FirstOrDefault(x => x.Resource.ToString() == item.ProductResource);
                        productRepository.Refresh(product.base_Product);
                        product.ToModel();

                        // Gets UOMCollection.
                        purchaseOrderDetailModelBefore = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                            x.ProductResource == item.ProductResource);
                        if (purchaseOrderDetailModelBefore != null && purchaseOrderDetailModelBefore.UOMCollection != null)
                        {
                            purchaseOrderDetailModel.UOMCollection = purchaseOrderDetailModelBefore.UOMCollection;
                        }
                        else
                        {
                            if (product != null)
                            {
                                purchaseOrderDetailModel.UOMCollection = GetUOMCollection(product.base_Product);
                            }
                        }
                        if (purchaseOrderDetailModel.UOMCollection == null)
                        {
                            purchaseOrderDetailModel.UOMCollection = new CollectionBase<base_ProductUOMModel>();
                        }

                        if (product != null)
                        {
                            purchaseOrderDetailModel.IsSerialTracking = product.IsSerialTracking;
                        }
                        productUOM = purchaseOrderDetailModel.UOMCollection.FirstOrDefault(x => x.UOMId == purchaseOrderDetailModel.UOMId);
                        if (productUOM != null)
                        {
                            purchaseOrderDetailModel.UnitName = productUOM.Name;
                        }
                        purchaseOrderDetailModel.OnHandQty = GetOnHandQty(_selectedPurchaseOrder.StoreCode, product);
                        purchaseOrderDetailModel.OnHandQtyTemp = purchaseOrderDetailModel.OnHandQty;
                        purchaseOrderDetailModel.BackupQuantity = item.Quantity;
                        purchaseOrderDetailModel.PurchaseOrder = _selectedPurchaseOrder;
                        purchaseOrderDetailModel.PropertyChanged += PurchaseOrderDetailPropertyChanged;
                        purchaseOrderDetailModel.IsDirty = false;
                        _selectedPurchaseOrder.PurchaseOrderDetailCollection.Add(purchaseOrderDetailModel);
                    }

                    // Gets PurchaseOrderDetailReceiveCollection of PurchaseOrder.
                    _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection = new ObservableCollection<base_PurchaseOrderDetailModel>(_selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x => !x.IsFullReceived));

                    // Gets PurchaseOrderReceives of PurchaseOrder.
                    string purchaseOrderID = _selectedPurchaseOrder.Resource.ToString();
                    IList<base_PurchaseOrderReceive> purchaseOrderReceives = purchaseOrderReceiveRepository.GetAll(x => x.POResource == purchaseOrderID);
                    base_PurchaseOrderReceiveModel purchaseOrderReceiveModel;
                    foreach (base_PurchaseOrderReceive item in purchaseOrderReceives)
                    {
                        purchaseOrderReceiveRepository.Refresh(item);
                        purchaseOrderReceiveModel = new base_PurchaseOrderReceiveModel(item);
                        purchaseOrderDetailModel = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x => x.Id == item.PurchaseOrderDetailId);
                        purchaseOrderReceiveModel.PurchaseOrderDetail = purchaseOrderDetailModel;
                        purchaseOrderReceiveModel.PurchaseOrder = _selectedPurchaseOrder;
                        purchaseOrderReceiveModel.UnitName = purchaseOrderDetailModel.UnitName;
                        purchaseOrderReceiveModel.Discount = purchaseOrderDetailModel.Discount;
                        purchaseOrderReceiveModel.Amount = purchaseOrderReceiveModel.RecQty * (purchaseOrderReceiveModel.Price - purchaseOrderReceiveModel.Discount);
                        purchaseOrderReceiveModel.PODResource = purchaseOrderDetailModel.Resource.ToString();
                        purchaseOrderReceiveModel.PropertyChanged += PurchaseOrderReceivePropertyChanged;
                        purchaseOrderReceiveModel.IsDirty = false;
                        _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Add(purchaseOrderReceiveModel);
                    }
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection.CollectionChanged += PurchaseOrderReceiveCollectionChanged;

                    // Gets PurchaseOrderPaymentCollection of PurchaseOrder.
                    _selectedPurchaseOrder.PurchaseOrderPaymentCollection = new CollectionBase<base_PurchaseOrderReceiveModel>(_selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x => x.IsReceived));

                    // Gets PurchaseOrderDetailReturnCollection of PurchaseOrder.
                    _selectedPurchaseOrder.PurchaseOrderDetailReturnCollection = new ObservableCollection<base_PurchaseOrderDetailModel>(_selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x => x.HasReceivedItem));

                    // Gets ResourceReturn.
                    base_ResourceReturn resourceReturn = resourceReturnRepository.Get(x => x.DocumentResource == purchaseOrderID);
                    if (resourceReturn != null)
                    {
                        _selectedPurchaseOrder.ResourceReturn = new base_ResourceReturnModel(resourceReturn);
                    }
                    else
                    {
                        _selectedPurchaseOrder.ResourceReturn = new base_ResourceReturnModel
                        {
                            DocumentResource = _selectedPurchaseOrder.Resource.ToString(),
                            DocumentNo = _selectedPurchaseOrder.PurchaseOrderNo,
                            TotalAmount = _selectedPurchaseOrder.Total,
                            Resource = Guid.NewGuid(),
                            Mark = OrderMarkType.PurchaseOrder.ToDescription(),
                            IsDirty = false
                        };
                    }
                    _selectedPurchaseOrder.ResourceReturn.PropertyChanged += ResourceReturnPropertyChanged;

                    // Gets ResourcePayment.
                    base_ResourcePayment resourcePayment = null;
                    if (resourcePaymentRepository.GetIQueryable(x => x.DocumentResource == purchaseOrderID).Count() > 0)
                    {
                        long newResourcePaymentId = resourcePaymentRepository.GetIQueryable(x => x.DocumentResource == purchaseOrderID).Max(x => x.Id);
                        resourcePayment = resourcePaymentRepository.Get(x => x.DocumentResource == purchaseOrderID && x.Id == newResourcePaymentId);
                    }

                    if (resourcePayment != null)
                    {
                        _selectedPurchaseOrder.ResourcePayment = new base_ResourcePaymentModel(resourcePayment);
                    }
                    else
                    {
                        _selectedPurchaseOrder.ResourcePayment = new base_ResourcePaymentModel
                        {
                            DocumentResource = _selectedPurchaseOrder.Resource.ToString(),
                            DocumentNo = _selectedPurchaseOrder.PurchaseOrderNo,
                            DateCreated = DateTime.Now.Date,
                            Mark = OrderMarkType.PurchaseOrder.ToDescription(),
                            Resource = Guid.NewGuid(),
                            IsDirty = false
                        };
                    }
                    _selectedPurchaseOrder.ResourcePayment.PropertyChanged += ResourcePaymentPropertyChanged;

                    // Gets ResourcePaymentDetail.
                    base_ResourcePaymentDetail resourcePaymentDetail = resourcePaymentDetailRepository.Get(x => x.ResourcePaymentId == _selectedPurchaseOrder.ResourcePayment.Id);
                    if (resourcePaymentDetail != null)
                    {
                        _selectedPurchaseOrder.ResourcePaymentDetail = new base_ResourcePaymentDetailModel(resourcePaymentDetail);
                    }
                    else
                    {
                        _selectedPurchaseOrder.ResourcePaymentDetail = new base_ResourcePaymentDetailModel();
                        _selectedPurchaseOrder.ResourcePaymentDetail.PaymentType = "P";
                        _selectedPurchaseOrder.ResourcePaymentDetail.ResourcePaymentId = _selectedPurchaseOrder.ResourcePayment.Id;
                        _selectedPurchaseOrder.ResourcePaymentDetail.PaymentMethodId = Define.CONFIGURATION.DefaultPaymentMethod.HasValue ? Define.CONFIGURATION.DefaultPaymentMethod.Value : _paymentMethodCollection.First().Value;
                        _selectedPurchaseOrder.ResourcePaymentDetail.PaymentMethod = _paymentMethodCollection.FirstOrDefault(x => x.Value == _selectedPurchaseOrder.ResourcePaymentDetail.PaymentMethodId).Text;
                        _selectedPurchaseOrder.ResourcePaymentDetail.Paid = _selectedPurchaseOrder.ResourcePayment.TotalPaid;
                        _selectedPurchaseOrder.ResourcePaymentDetail.IsDirty = false;
                    }
                    _selectedPurchaseOrder.ResourcePaymentDetail.PropertyChanged += ResourcePaymentDetailPropertyChanged;

                    // Gets ResourceReturnDetails of PurchaseOrder.
                    IList<base_ResourceReturnDetail> resourceReturnDetails = resourceReturnDetailRepository.GetAll(x => x.ResourceReturnId == _selectedPurchaseOrder.ResourceReturn.Id);
                    base_ResourceReturnDetailModel resourceReturnDetailModel;
                    foreach (base_ResourceReturnDetail item in resourceReturnDetails)
                    {
                        resourceReturnDetailRepository.Refresh(item);
                        purchaseOrderDetailModel = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x => x.Resource.ToString() == item.OrderDetailResource);
                        resourceReturnDetailModel = new base_ResourceReturnDetailModel(item);
                        resourceReturnDetailModel.PurchaseOrder = _selectedPurchaseOrder;
                        resourceReturnDetailModel.PurchaseOrderDetail = purchaseOrderDetailModel;
                        resourceReturnDetailModel.UnitName = purchaseOrderDetailModel.UnitName;
                        resourceReturnDetailModel.IsPurchaseOrderUsed = true;
                        resourceReturnDetailModel.PropertyChanged += ResourceReturnDetailPropertyChanged;
                        resourceReturnDetailModel.IsDirty = false;
                        _selectedPurchaseOrder.ResourceReturnDetailCollection.Add(resourceReturnDetailModel);
                    }
                    _selectedPurchaseOrder.ResourceReturnDetailCollection.CollectionChanged += ResourceReturnDetailCollectionChanged;

                    // Calculate total receive of purchase order.
                    CalculateTotalReceiveOfPurchaseOrder();

                    _selectedPurchaseOrder.RaiseCanChangeStorePropertyChanged();
                    _selectedPurchaseOrder.IsDirty = false;
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                // Select default TabItem.
                _currentTabItem = (int)TabItems.Order;
                OnPropertyChanged(() => CurrentTabItem);
            }
        }

        #endregion

        #region CalculateTotalForPurchaseOrder

        /// <summary>
        /// Calculate Total for PurchaseOrder.
        /// </summary>
        private void CalculateTotalForPurchaseOrder()
        {
            decimal total = 0;
            foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                if (item.Amount.HasValue)
                {
                    total += item.Amount.Value;
                }
            }
            _selectedPurchaseOrder.Total = total;
        }

        #endregion

        #region ShowReceiveItem

        /// <summary>
        /// Show 'Receive' TabItem.
        /// </summary>
        private void ShowReceiveItem()
        {
            if (_selectedPurchaseOrder.Status < (short)PurchaseStatus.Receiving)
            {
                _selectedPurchaseOrder.Status = (short)PurchaseStatus.Receiving;
            }
            CurrentTabItem = (int)TabItems.Receive;
        }

        #endregion

        #region ReceiveAll

        /// <summary>
        /// Received all items.
        /// </summary>
        private void ReceiveAll()
        {
            bool isCalculate = false;
            int sumReceivedQty = 0;
            int additionReceivedQty = 0;
            base_PurchaseOrderReceiveModel purchaseOrderReceive = null;

            foreach (var purchaseOrderDetail in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                // Calculate sum of received quantity of PurchaseOrderDetail.
                sumReceivedQty = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                x.PODResource == purchaseOrderDetail.Resource.ToString()).Sum(x => x.RecQty);

                // Addition received quantity.
                additionReceivedQty = purchaseOrderDetail.Quantity - sumReceivedQty;

                if (additionReceivedQty > 0)
                {
                    if (!isCalculate)
                    {
                        isCalculate = true;
                    }

                    purchaseOrderReceive = new base_PurchaseOrderReceiveModel();
                    purchaseOrderReceive.PurchaseOrderDetailId = purchaseOrderDetail.Id;
                    purchaseOrderReceive.POResource = _selectedPurchaseOrder.Resource.ToString();
                    purchaseOrderReceive.PODResource = purchaseOrderDetail.Resource.ToString();
                    purchaseOrderReceive.ProductResource = purchaseOrderDetail.ProductResource;
                    purchaseOrderReceive.ItemCode = purchaseOrderDetail.ItemCode;
                    purchaseOrderReceive.ItemName = purchaseOrderDetail.ItemName;
                    purchaseOrderReceive.ItemAtribute = purchaseOrderDetail.ItemAtribute;
                    purchaseOrderReceive.ItemSize = purchaseOrderDetail.ItemSize;
                    purchaseOrderReceive.UnitName = purchaseOrderDetail.UnitName;
                    purchaseOrderReceive.Price = purchaseOrderDetail.Price;
                    purchaseOrderReceive.RecQty = additionReceivedQty;
                    purchaseOrderReceive.Discount = purchaseOrderDetail.Discount;
                    purchaseOrderReceive.Amount = purchaseOrderReceive.RecQty * (purchaseOrderReceive.Price - purchaseOrderReceive.Discount);
                    purchaseOrderReceive.PurchaseOrderDetail = purchaseOrderDetail;
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Add(purchaseOrderReceive);
                    purchaseOrderReceive.IsTemporary = false;
                }
            }

            if (isCalculate)
            {
                // Calculate total receive of purchase order.
                CalculateTotalReceiveOfPurchaseOrder();
            }
        }

        #endregion

        #region CalculateAdditionReceivedQty

        /// <summary>
        /// Calculate addition received quantity of PurchaseOrderReceive.
        /// </summary>
        /// <param name="purchaseOrderReceive">PurchaseOrderReceive to calculate.</param>
        private void CalculateAdditionReceivedQty(base_PurchaseOrderReceiveModel purchaseOrderReceive)
        {
            // Set RecQty = 0 to ignore calculate this item.
            purchaseOrderReceive.RecQty = 0;

            int sumReceivedQty = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                x.PODResource == purchaseOrderReceive.PODResource).Sum(x => x.RecQty);

            // Additon received quantity.
            int additionReceivedQty = purchaseOrderReceive.PurchaseOrderDetail.Quantity - sumReceivedQty;
            if (additionReceivedQty < 0)
            {
                additionReceivedQty = 0;
            }
            purchaseOrderReceive.RecQty = additionReceivedQty;
        }

        #endregion

        #region CalculateSumReceivedQty

        /// <summary>
        /// Calculate sum of received quantity of PurchaseOrderDetail.
        /// </summary>
        /// <param name="id">PurchaseOrderDetail's identity.</param>
        private void CalculateSumReceivedQty(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            int sumReceivedQty = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                x.PODResource == id && x.IsReceived).Sum(x => x.RecQty);

            base_PurchaseOrderDetailModel purchaseOrderDetail = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                x.Resource.ToString() == id);
            if (purchaseOrderDetail != null)
            {
                int increase = sumReceivedQty - purchaseOrderDetail.ReceivedQty;
                purchaseOrderDetail.ReceivedQty = sumReceivedQty;

                // Update On-hand quantity in stock.
                purchaseOrderDetail.OnHandQty += increase;

                IEnumerable<base_PurchaseOrderDetailModel> productList = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                    x.ProductResource == purchaseOrderDetail.ProductResource);
                foreach (base_PurchaseOrderDetailModel itemFriend in productList)
                {
                    itemFriend.OnHandQty = purchaseOrderDetail.OnHandQty;
                }
            }
        }

        #endregion

        #region ChangeTabItem

        /// <summary>
        /// Executes change TabItem.
        /// </summary>
        /// <returns>True allow change TabItem. Otherwise, False.</returns>
        private bool ChangeTabItem()
        {
            bool isAllowChangeTabItem = true;

            TabItems oldTabItem = (TabItems)_oldCurrentTabItem;
            if (oldTabItem == TabItems.Order)
            {
                if (_selectedPurchaseOrder.PurchaseOrderDetailCollection.Any(x => x.HasError))
                {
                    isAllowChangeTabItem = false;
                }
            }
            else if (oldTabItem == TabItems.Receive)
            {
                if (_selectedPurchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.HasError))
                {
                    isAllowChangeTabItem = false;
                }
            }
            else if (oldTabItem == TabItems.Return)
            {
                if (_selectedPurchaseOrder.ResourceReturnDetailCollection.Any(x => x.HasError))
                {
                    isAllowChangeTabItem = false;
                }
            }

            if (!isAllowChangeTabItem)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _currentTabItem = _oldCurrentTabItem;
                    OnPropertyChanged(() => CurrentTabItem);
                    MessageBox.Show("Fix error in current TabItem before change TabItem.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }

            return isAllowChangeTabItem;
        }

        #endregion

        #region CommitOrCancelChange

        /// <summary>
        /// Commit or cancel change in PurchaseOrderDetailCollection and PurchaseOrderReceiveCollection.
        /// Fix error: 'DeferRefresh' is not allowed during an AddNew or EditItem transaction.
        /// </summary>
        private void CommitOrCancelChange(bool isOnPurchaseOrderDetailCollectionView = true, bool isOnPurchaseOrderReceiveCollectionView = true, bool isOnResourceReturnDetailCollectionView = true)
        {
            if (_selectedPurchaseOrder == null)
            {
                return;
            }

            if (isOnPurchaseOrderDetailCollectionView)
            {
                ListCollectionView purchaseOrderDetailCollectionView = CollectionViewSource.GetDefaultView(_selectedPurchaseOrder.PurchaseOrderDetailCollection) as ListCollectionView;
                if (purchaseOrderDetailCollectionView != null)
                {
                    if (purchaseOrderDetailCollectionView.IsEditingItem)
                    {
                        if ((purchaseOrderDetailCollectionView.CurrentEditItem as base_PurchaseOrderDetailModel).HasError)
                        {
                            purchaseOrderDetailCollectionView.CancelEdit();
                        }
                        else
                        {
                            purchaseOrderDetailCollectionView.CommitEdit();
                        }
                    }
                    if (purchaseOrderDetailCollectionView.IsAddingNew)
                    {
                        if ((purchaseOrderDetailCollectionView.CurrentAddItem as base_PurchaseOrderDetailModel).HasError)
                        {
                            purchaseOrderDetailCollectionView.CancelNew();
                        }
                        else
                        {
                            purchaseOrderDetailCollectionView.CommitNew();
                        }

                    }
                }
            }

            if (isOnPurchaseOrderReceiveCollectionView)
            {
                ListCollectionView purchaseOrderReceiveCollectionView = CollectionViewSource.GetDefaultView(_selectedPurchaseOrder.PurchaseOrderReceiveCollection) as ListCollectionView;
                if (purchaseOrderReceiveCollectionView != null)
                {
                    if (purchaseOrderReceiveCollectionView.IsEditingItem)
                    {
                        if ((purchaseOrderReceiveCollectionView.CurrentEditItem as base_PurchaseOrderReceiveModel).HasError)
                        {
                            purchaseOrderReceiveCollectionView.CancelEdit();
                        }
                        else
                        {
                            purchaseOrderReceiveCollectionView.CommitEdit();
                        }
                    }
                    if (purchaseOrderReceiveCollectionView.IsAddingNew)
                    {
                        if ((purchaseOrderReceiveCollectionView.CurrentAddItem as base_PurchaseOrderReceiveModel).HasError)
                        {
                            purchaseOrderReceiveCollectionView.CancelNew();
                        }
                        else
                        {
                            purchaseOrderReceiveCollectionView.CommitNew();
                        }
                    }
                }
            }

            if (isOnResourceReturnDetailCollectionView)
            {
                ListCollectionView resourceReturnDetailCollectionView = CollectionViewSource.GetDefaultView(_selectedPurchaseOrder.ResourceReturnDetailCollection) as ListCollectionView;
                if (resourceReturnDetailCollectionView != null)
                {
                    if (resourceReturnDetailCollectionView.IsEditingItem)
                    {
                        if ((resourceReturnDetailCollectionView.CurrentEditItem as base_ResourceReturnDetailModel).HasError)
                        {
                            resourceReturnDetailCollectionView.CancelEdit();
                        }
                        else
                        {
                            resourceReturnDetailCollectionView.CommitEdit();
                        }
                    }
                    if (resourceReturnDetailCollectionView.IsAddingNew)
                    {
                        if ((resourceReturnDetailCollectionView.CurrentAddItem as base_ResourceReturnDetailModel).HasError)
                        {
                            resourceReturnDetailCollectionView.CancelNew();
                        }
                        else
                        {
                            resourceReturnDetailCollectionView.CommitNew();
                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateOrderQtyOfPurchaseOrder

        /// <summary>
        /// Calculate order quantity of purchase order.
        /// </summary>
        private void CalculateOrderQtyOfPurchaseOrder()
        {
            int sum = 0;
            foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                sum += item.Quantity;
            }
            _selectedPurchaseOrder.QtyOrdered = sum;

            CalculateDueQtyOfPurchaseOrder();
        }

        #endregion

        #region CalculateReceivedQtyOfPurchaseOrder

        /// <summary>
        /// Calculate received quantity of purchase order.
        /// </summary>
        private void CalculateReceivedQtyOfPurchaseOrder()
        {
            int sum = 0;
            foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                sum += item.ReceivedQty;
            }
            _selectedPurchaseOrder.QtyReceived = sum;

            CalculateDueQtyOfPurchaseOrder();
        }

        #endregion

        #region CalculateDueQtyOfPurchaseOrder

        /// <summary>
        /// Calculate due quantity and unfilled of purchase order.
        /// </summary>
        private void CalculateDueQtyOfPurchaseOrder()
        {
            // Calculate UnFilledQty.
            if (_selectedPurchaseOrder.QtyOrdered != 0)
            {
                _selectedPurchaseOrder.UnFilled = Math.Round(Math.Round((decimal)((_selectedPurchaseOrder.QtyOrdered - _selectedPurchaseOrder.QtyReceived) * 100) / _selectedPurchaseOrder.QtyOrdered, 2) - 0.01M, 1, MidpointRounding.AwayFromZero);
            }
            else
            {
                _selectedPurchaseOrder.UnFilled = 0;
            }
            if (_selectedPurchaseOrder.UnFilled < 0)
            {
                _selectedPurchaseOrder.UnFilled = 0;
            }

            // Calculate DueQty.
            _selectedPurchaseOrder.QtyDue = _selectedPurchaseOrder.QtyOrdered - _selectedPurchaseOrder.QtyReceived;
            if (_selectedPurchaseOrder.QtyDue < 0)
            {
                _selectedPurchaseOrder.QtyDue = 0;
            }
        }

        #endregion

        #region CalculateTotalReceiveOfPurchaseOrder

        /// <summary>
        /// Calculate total receive of purchase order.
        /// </summary>
        private void CalculateTotalReceiveOfPurchaseOrder()
        {
            int sum = 0;
            foreach (base_PurchaseOrderReceiveModel item in _selectedPurchaseOrder.PurchaseOrderReceiveCollection)
            {
                sum += item.RecQty;
            }
            _selectedPurchaseOrder.TotalReceive = sum;
        }

        #endregion

        #region CheckFullReceived

        /// <summary>
        /// Check full received for PurchaseOrderDetail.
        /// </summary>
        /// <param name="purchaseOrderDetail">PurchaseOrderDetail to check.</param>
        private void CheckFullReceived(base_PurchaseOrderDetailModel purchaseOrderDetail)
        {
            if (_selectedPurchaseOrder.IsNew)
            {
                return;
            }

            if (purchaseOrderDetail.ReceivedQty > 0 &&
                purchaseOrderDetail.ReceivedQty >= purchaseOrderDetail.Quantity &&
                _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Count(x => x.PODResource == purchaseOrderDetail.Resource.ToString()) > 0 &&
                !_selectedPurchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.PODResource == purchaseOrderDetail.Resource.ToString() && !x.IsReceived))
            {
                purchaseOrderDetail.IsFullReceived = true;
                if (_selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Contains(purchaseOrderDetail))
                {
                    _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Remove(purchaseOrderDetail);
                }
            }
            else
            {
                purchaseOrderDetail.IsFullReceived = false;
                if (!_selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Contains(purchaseOrderDetail))
                {
                    _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Add(purchaseOrderDetail);
                }
            }
        }

        #endregion

        #region CheckFullWorkflow

        /// <summary>
        /// Check IsFullWorkflow of PurchaseOrder.
        /// </summary>
        private void CheckFullWorkflow()
        {
            if (_selectedPurchaseOrder.IsNew)
            {
                return;
            }

            if (_selectedPurchaseOrder.PurchaseOrderDetailCollection.Any() &&
                !_selectedPurchaseOrder.PurchaseOrderDetailCollection.Any(x => !x.IsFullReceived) &&
                _selectedPurchaseOrder.ResourcePayment.TotalAmount > 0 &&
                _selectedPurchaseOrder.ResourcePayment.TotalPaid > 0 &&
                _selectedPurchaseOrder.ResourcePayment.Balance == 0)
            {
                CommitOrCancelChange();
                _selectedPurchaseOrder.IsFullWorkflow = true;
            }
            else
            {
                _selectedPurchaseOrder.IsFullWorkflow = false;
            }
        }

        #endregion

        #region ReturnAll

        /// <summary>
        /// Returned all items.
        /// </summary>
        private void ReturnAll()
        {
            int sumReturnedQty = 0;
            int additionReturnQty = 0;
            base_ResourceReturnDetailModel resourceReturnDetail = null;

            foreach (var purchaseOrderDetail in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                // Calculate sum of returned quantity of ResourceReturnDetail.
                sumReturnedQty = _selectedPurchaseOrder.ResourceReturnDetailCollection.Where(x =>
                x.OrderDetailResource == purchaseOrderDetail.Resource.ToString()).Sum(x => x.ReturnQty);

                // Addition received quantity.
                additionReturnQty = purchaseOrderDetail.ReceivedQty - sumReturnedQty;

                if (additionReturnQty > 0)
                {
                    resourceReturnDetail = new base_ResourceReturnDetailModel();
                    resourceReturnDetail.PurchaseOrder = _selectedPurchaseOrder;
                    resourceReturnDetail.OrderDetailResource = purchaseOrderDetail.Resource.ToString();
                    resourceReturnDetail.ProductResource = purchaseOrderDetail.ProductResource;
                    resourceReturnDetail.ItemCode = purchaseOrderDetail.ItemCode;
                    resourceReturnDetail.ItemName = purchaseOrderDetail.ItemName;
                    resourceReturnDetail.ItemAtribute = purchaseOrderDetail.ItemAtribute;
                    resourceReturnDetail.ItemSize = purchaseOrderDetail.ItemSize;
                    resourceReturnDetail.UnitName = purchaseOrderDetail.UnitName;
                    resourceReturnDetail.Price = purchaseOrderDetail.Price;
                    resourceReturnDetail.Discount = purchaseOrderDetail.Discount;
                    resourceReturnDetail.ReturnQty = additionReturnQty;
                    resourceReturnDetail.Amount = resourceReturnDetail.ReturnQty * (resourceReturnDetail.Price - resourceReturnDetail.Discount);
                    resourceReturnDetail.PurchaseOrderDetail = purchaseOrderDetail;
                    _selectedPurchaseOrder.ResourceReturnDetailCollection.Add(resourceReturnDetail);
                    resourceReturnDetail.IsTemporary = false;
                }
            }

            CalculateSubTotalForResourceReturn();
        }

        #endregion

        #region CalculateAdditionReturnedQty

        /// <summary>
        /// Calculate addition returned quantity of ResourceReturnDetail.
        /// </summary>
        /// <param name="resourceReturnDetail">ResourceReturnDetail to calculate.</param>
        private void CalculateAdditionReturnedQty(base_ResourceReturnDetailModel resourceReturnDetail)
        {
            // Set ReturnQty = 0 to ignore calculate this item.
            resourceReturnDetail.ReturnQty = 0;

            int sumReturnedQty = _selectedPurchaseOrder.ResourceReturnDetailCollection.Where(x =>
                x.OrderDetailResource == resourceReturnDetail.OrderDetailResource).Sum(x => x.ReturnQty);

            // Additon returned quantity.
            int additionReturnedQty = resourceReturnDetail.PurchaseOrderDetail.ReceivedQty - sumReturnedQty;
            if (additionReturnedQty < 0)
            {
                additionReturnedQty = 0;
            }
            resourceReturnDetail.ReturnQty = additionReturnedQty;
        }

        #endregion

        #region CheckReturned

        /// <summary>
        /// Check whether can return product in PurchaseOrderDetail.
        /// </summary>
        /// <param name="purchaseOrderDetail">PurchaseOrderDetail to check.</param>
        private void CheckReturned(base_PurchaseOrderDetailModel purchaseOrderDetail)
        {
            if (purchaseOrderDetail.HasReceivedItem)
            {
                if (!_selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Contains(purchaseOrderDetail))
                {
                    _selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Add(purchaseOrderDetail);
                }
            }
            else
            {
                if (_selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Contains(purchaseOrderDetail))
                {
                    _selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Remove(purchaseOrderDetail);
                }
            }
        }

        /// <summary>
        /// Check whether can return product in PurchaseOrderDetail.
        /// </summary>
        private void CheckReturned()
        {
            int returnQtyTotal = 0;
            IEnumerable<base_ResourceReturnDetailModel> oldResourceReturnDetails = _selectedPurchaseOrder.ResourceReturnDetailCollection.Where(x => x.PurchaseOrderDetail != null);
            foreach (base_ResourceReturnDetailModel item in oldResourceReturnDetails)
            {
                returnQtyTotal = _selectedPurchaseOrder.ResourceReturnDetailCollection.Where(x => x.OrderDetailResource == item.OrderDetailResource).Sum(x => x.ReturnQty);
                if (item.PurchaseOrderDetail.ReceivedQty <= returnQtyTotal)
                {
                    if (_selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Contains(item.PurchaseOrderDetail))
                    {
                        _selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Remove(item.PurchaseOrderDetail);
                    }
                }
                else
                {
                    if (!_selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Contains(item.PurchaseOrderDetail))
                    {
                        _selectedPurchaseOrder.PurchaseOrderDetailReturnCollection.Add(item.PurchaseOrderDetail);
                    }
                }
            }
        }

        #endregion

        #region AddProductsOutSide

        /// <summary>
        /// Add product list in purchase order.
        /// </summary>
        private void AddProductsOutSide()
        {
            if (_productCollectionOutSide != null && _productCollectionOutSide.Count() > 0)
            {
                foreach (base_ProductModel product in _productCollectionOutSide)
                {
                    AddPurchaseOrderDetail(product);
                }

                IEnumerable<base_PurchaseOrderDetailModel> serialPurchaseOrderDetails = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                    x.IsSerialTracking);
                if (serialPurchaseOrderDetails.Any())
                {
                    AddSerials(serialPurchaseOrderDetails);
                }
            }
        }

        #endregion

        #region CalculateOnHandQtyWhenReturn

        /// <summary>
        /// Calculate on-hand quantity when return item.
        /// </summary>
        private void CalculateOnHandQtyWhenReturn(base_ResourceReturnDetailModel resourceReturnDetail, bool isIncrease = false)
        {
            if (resourceReturnDetail.PurchaseOrderDetail != null)
            {
                if (isIncrease)
                {
                    resourceReturnDetail.PurchaseOrderDetail.OnHandQty += resourceReturnDetail.ReturnQty;
                }
                else
                {
                    resourceReturnDetail.PurchaseOrderDetail.OnHandQty -= resourceReturnDetail.ReturnQty;
                }

                IEnumerable<base_PurchaseOrderDetailModel> productList = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                    x.ProductResource == resourceReturnDetail.PurchaseOrderDetail.ProductResource);
                foreach (base_PurchaseOrderDetailModel itemFriend in productList)
                {
                    itemFriend.OnHandQty = resourceReturnDetail.PurchaseOrderDetail.OnHandQty;
                }
            }
        }

        #endregion

        #region CalculateSubTotalForResourceReturn

        /// <summary>
        /// Calculate Subtotal in return TabItem.
        /// </summary>
        private void CalculateSubTotalForResourceReturn()
        {
            decimal subTotal = 0;
            foreach (base_ResourceReturnDetailModel item in _selectedPurchaseOrder.ResourceReturnDetailCollection)
            {
                subTotal += item.Amount;
            }
            _selectedPurchaseOrder.ResourceReturn.SubTotal = subTotal;
        }

        #endregion

        #region AddRemovePayment

        /// <summary>
        /// Add or remove payment.
        /// </summary>
        private void AddRemovePayment(base_PurchaseOrderReceiveModel purchaseOrderReceive)
        {
            if (purchaseOrderReceive.IsReceived)
            {
                if (!_selectedPurchaseOrder.PurchaseOrderPaymentCollection.Contains(purchaseOrderReceive))
                {
                    _selectedPurchaseOrder.PurchaseOrderPaymentCollection.Add(purchaseOrderReceive);
                    CalculateTotalAmountForResourcePayment();
                }
            }
            else
            {
                if (_selectedPurchaseOrder.PurchaseOrderPaymentCollection.Contains(purchaseOrderReceive))
                {
                    _selectedPurchaseOrder.PurchaseOrderPaymentCollection.Remove(purchaseOrderReceive);
                    CalculateTotalAmountForResourcePayment();
                }
            }
        }

        #endregion

        #region CalculateTotalAmountForResourcePayment

        /// <summary>
        /// Calculate TotalAmount in payment TabItem.
        /// </summary>
        private void CalculateTotalAmountForResourcePayment()
        {
            decimal total = 0;
            foreach (base_PurchaseOrderReceiveModel item in _selectedPurchaseOrder.PurchaseOrderPaymentCollection)
            {
                total += item.Amount;
            }
            _selectedPurchaseOrder.ResourcePayment.TotalAmount = total;
        }

        #endregion

        #region EditProduct

        /// <summary>
        /// Edit product.
        /// </summary>
        private void EditProduct()
        {
            MessageBox.Show("Edit Product.");
        }

        #endregion

        #endregion

        #region Override Methods

        #region LoadData

        public override void LoadData()
        {
            Initialize();
        }

        #endregion

        #region OnViewChangingCommandCanExecute

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            if (_selectedPurchaseOrder != null)
            {
                // Holds old selected item.
                _oldPurchaseOrder = _selectedPurchaseOrder;
                bool isUnactive = SaveNotify();
                if (isUnactive)
                {
                    CommitOrCancelChange();
                }
                return isUnactive;
            }

            return true;
        }

        #endregion

        #region ChangeSearchMode

        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (_isSearchMode)
                {
                    if (!isList)
                    {
                        IsSearchMode = false;
                        CreatePurchaseOrder();
                    }
                }
                else
                {
                    if (isList)
                    {
                        OpenSearchComponent();
                    }
                }
            }
            else
            {
                _productCollectionOutSide = param as IEnumerable<base_ProductModel>;
                IsSearchMode = false;
                // Forces null to create new purchase order with input product collection when WorkerRunWorkerCompleted.
                _oldPurchaseOrder = null;
            }
        }

        #endregion

        #endregion

        #region Events

        #region PurchaseOrderPropertyChanged

        private void PurchaseOrderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_PurchaseOrderModel purchaseOrder = sender as base_PurchaseOrderModel;
            switch (e.PropertyName)
            {
                case "StoreCode":

                    if (_storeCollection != null)
                    {
                        // Gets selected store.
                        base_StoreModel store = _storeCollection.ElementAt(purchaseOrder.StoreCode);
                        if (store != null)
                        {
                            // Gets store's address.
                            purchaseOrder.ShipAddress = string.Format("{0}. {1}", store.City, store.Street);
                        }
                        else
                        {
                            purchaseOrder.ShipAddress = null;
                        }

                        // Update on-hand quantity in stock.
                        foreach (base_PurchaseOrderDetailModel item in purchaseOrder.PurchaseOrderDetailCollection)
                        {
                            item.OnHandQty = GetOnHandQty(purchaseOrder.StoreCode, _productCollection.FirstOrDefault(x =>
                                x.Resource.ToString() == item.ProductResource));
                            item.OnHandQtyTemp = item.OnHandQty;
                        }
                    }

                    break;

                case "VendorId":

                    if (_vendorCollection != null)
                    {
                        // Gets selected vendor.
                        base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Id == purchaseOrder.VendorId);
                        if (vendor != null)
                        {
                            // Update VendorCode, VendorName.
                            purchaseOrder.VendorCode = vendor.GuestNo;
                            purchaseOrder.VendorName = vendor.Company;

                            // Update TermNetDue, TermDiscountPercent, TermPaidWithinDay, PaymentTermDescription.
                            purchaseOrder.TermNetDue = vendor.TermNetDue;
                            purchaseOrder.TermDiscountPercent = vendor.TermDiscount;
                            purchaseOrder.TermPaidWithinDay = vendor.TermPaidWithinDay;
                            purchaseOrder.PaymentTermDescription = vendor.PaymentTermDescription;
                        }
                        else
                        {
                            purchaseOrder.VendorCode = null;
                            purchaseOrder.VendorName = null;
                        }
                    }

                    break;

                case "ShipDate":

                    if (purchaseOrder.ShipDate.HasValue)
                    {
                        purchaseOrder.PaymentDueDate = purchaseOrder.ShipDate.Value.AddDays(purchaseOrder.TermNetDue);
                    }
                    else
                    {
                        purchaseOrder.PaymentDueDate = null;
                    }

                    break;

                case "TermNetDue":

                    if (purchaseOrder.ShipDate.HasValue)
                    {
                        purchaseOrder.PaymentDueDate = purchaseOrder.ShipDate.Value.AddDays(purchaseOrder.TermNetDue);
                    }
                    else
                    {
                        purchaseOrder.PaymentDueDate = null;
                    }

                    break;

                case "IsFullWorkflow":

                    if (purchaseOrder.IsFullWorkflow)
                    {
                        if (purchaseOrder.Status < (short)PurchaseStatus.PaidInFull)
                        {
                            purchaseOrder.Status = (short)PurchaseStatus.PaidInFull;
                        }
                    }

                    break;
            }
        }

        #endregion

        #region PurchaseOrderDetailPropertyChanged

        private void PurchaseOrderDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_PurchaseOrderDetailModel purchaseOrderDetail = sender as base_PurchaseOrderDetailModel;
            switch (e.PropertyName)
            {
                case "UOMId":

                    if (purchaseOrderDetail.UOMCollection != null)
                    {
                        base_ProductUOMModel productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == purchaseOrderDetail.UOMId);
                        if (productUOM != null)
                        {
                            purchaseOrderDetail.UnitName = productUOM.Name;
                            purchaseOrderDetail.BaseUOM = productUOM.Code;
                            purchaseOrderDetail.Price = productUOM.RegularPrice;

                            // Update Price in PurchaseOrderReceive.
                            IEnumerable<base_PurchaseOrderReceiveModel> receiveItems = purchaseOrderDetail.PurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                                x.PODResource == purchaseOrderDetail.Resource.ToString());
                            foreach (var item in receiveItems)
                            {
                                item.Price = productUOM.RegularPrice;
                                item.UnitName = productUOM.Name;
                            }
                        }
                    }

                    break;

                case "Discount":

                    purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * (purchaseOrderDetail.Price - purchaseOrderDetail.Discount);

                    break;

                case "Price":

                    purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * (purchaseOrderDetail.Price - purchaseOrderDetail.Discount);

                    break;

                case "Quantity":

                    purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * (purchaseOrderDetail.Price - purchaseOrderDetail.Discount);

                    // Calculate UnFilledQty.
                    if (purchaseOrderDetail.Quantity != 0)
                    {
                        purchaseOrderDetail.UnFilledQty = Math.Round(Math.Round((decimal)((purchaseOrderDetail.Quantity - purchaseOrderDetail.ReceivedQty) * 100) / purchaseOrderDetail.Quantity, 2) - 0.01M, 1, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        purchaseOrderDetail.UnFilledQty = 0;
                    }
                    if (purchaseOrderDetail.UnFilledQty < 0)
                    {
                        purchaseOrderDetail.UnFilledQty = 0;
                    }

                    // Calculate DueQty.
                    purchaseOrderDetail.DueQty = purchaseOrderDetail.Quantity - purchaseOrderDetail.ReceivedQty;
                    if (purchaseOrderDetail.DueQty < 0)
                    {
                        purchaseOrderDetail.DueQty = 0;
                    }

                    // Determine full received.
                    CheckFullReceived(purchaseOrderDetail);

                    // Calculate order quantity of purchase order.
                    CalculateOrderQtyOfPurchaseOrder();

                    break;

                case "ReceivedQty":

                    // Calculate UnFilledQty.
                    if (purchaseOrderDetail.Quantity != 0)
                    {
                        purchaseOrderDetail.UnFilledQty = Math.Round(Math.Round((decimal)((purchaseOrderDetail.Quantity - purchaseOrderDetail.ReceivedQty) * 100) / purchaseOrderDetail.Quantity, 2) - 0.01M, 1, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        purchaseOrderDetail.UnFilledQty = 0;
                    }
                    if (purchaseOrderDetail.UnFilledQty < 0)
                    {
                        purchaseOrderDetail.UnFilledQty = 0;
                    }

                    // Calculate DueQty.
                    purchaseOrderDetail.DueQty = purchaseOrderDetail.Quantity - purchaseOrderDetail.ReceivedQty;
                    if (purchaseOrderDetail.DueQty < 0)
                    {
                        purchaseOrderDetail.DueQty = 0;
                    }

                    // Determine full received.
                    CheckFullReceived(purchaseOrderDetail);

                    // Determine whether can return product in PurchaseOrderDetail.
                    CheckReturned(purchaseOrderDetail);

                    // Calculate received quantity of purchase order.
                    CalculateReceivedQtyOfPurchaseOrder();

                    break;

                case "Amount":

                    CalculateTotalForPurchaseOrder();

                    break;

                case "IsFullReceived":

                    if (purchaseOrderDetail.PurchaseOrder.Status < (short)PurchaseStatus.FullyReceived)
                    {
                        if (purchaseOrderDetail.PurchaseOrder.PurchaseOrderDetailCollection.Count > 0 &&
                            !purchaseOrderDetail.PurchaseOrder.PurchaseOrderDetailCollection.Any(x => !x.IsFullReceived))
                        {
                            purchaseOrderDetail.PurchaseOrder.Status = (short)PurchaseStatus.FullyReceived;
                        }
                    }

                    CheckFullWorkflow();

                    break;
            }
        }

        #endregion

        #region PurchaseOrderReceiveCollectionChanged

        private void PurchaseOrderReceiveCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base_PurchaseOrderReceiveModel purchaseOrderReceive;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    purchaseOrderReceive = item as base_PurchaseOrderReceiveModel;
                    purchaseOrderReceive.PurchaseOrder = _selectedPurchaseOrder;
                    purchaseOrderReceive.POResource = _selectedPurchaseOrder.Resource.ToString();
                    purchaseOrderReceive.ReceiveDate = DateTime.Now;
                    purchaseOrderReceive.Resource = Guid.NewGuid();
                    purchaseOrderReceive.IsTemporary = true;
                    purchaseOrderReceive.PropertyChanged += PurchaseOrderReceivePropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    purchaseOrderReceive = item as base_PurchaseOrderReceiveModel;
                    purchaseOrderReceive.PropertyChanged -= PurchaseOrderReceivePropertyChanged;
                }
            }
        }

        #endregion

        #region PurchaseOrderReceivePropertyChanged

        private void PurchaseOrderReceivePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_PurchaseOrderReceiveModel purchaseOrderReceive = sender as base_PurchaseOrderReceiveModel;

            switch (e.PropertyName)
            {
                case "PurchaseOrderDetail":

                    if (purchaseOrderReceive.PurchaseOrderDetail != null)
                    {
                        purchaseOrderReceive.PurchaseOrderDetailId = purchaseOrderReceive.PurchaseOrderDetail.Id;
                        purchaseOrderReceive.PODResource = purchaseOrderReceive.PurchaseOrderDetail.Resource.ToString();
                        purchaseOrderReceive.ProductResource = purchaseOrderReceive.PurchaseOrderDetail.ProductResource;
                        purchaseOrderReceive.ItemCode = purchaseOrderReceive.PurchaseOrderDetail.ItemCode;
                        purchaseOrderReceive.ItemName = purchaseOrderReceive.PurchaseOrderDetail.ItemName;
                        purchaseOrderReceive.ItemAtribute = purchaseOrderReceive.PurchaseOrderDetail.ItemAtribute;
                        purchaseOrderReceive.ItemSize = purchaseOrderReceive.PurchaseOrderDetail.ItemSize;
                        purchaseOrderReceive.UnitName = purchaseOrderReceive.PurchaseOrderDetail.UnitName;
                        purchaseOrderReceive.Price = purchaseOrderReceive.PurchaseOrderDetail.Price;
                        CalculateAdditionReceivedQty(purchaseOrderReceive);
                        purchaseOrderReceive.Discount = purchaseOrderReceive.PurchaseOrderDetail.Discount;
                        purchaseOrderReceive.Amount = purchaseOrderReceive.RecQty * (purchaseOrderReceive.Price - purchaseOrderReceive.Discount);
                    }
                    else
                    {
                        purchaseOrderReceive.PurchaseOrderDetailId = 0;
                        purchaseOrderReceive.ProductResource = null;
                        purchaseOrderReceive.PODResource = null;
                        purchaseOrderReceive.ItemCode = null;
                        purchaseOrderReceive.ItemName = null;
                        purchaseOrderReceive.ItemAtribute = null;
                        purchaseOrderReceive.ItemSize = null;
                        purchaseOrderReceive.RecQty = 0;
                        purchaseOrderReceive.Price = 0;
                        purchaseOrderReceive.Discount = 0;
                    }

                    break;

                case "IsReceived":

                    // Calculate RecQty.
                    if (purchaseOrderReceive.IsReceived)
                    {
                        if (!purchaseOrderReceive.HasError)
                        {
                            MessageBoxResult result = MessageBox.Show("Are you sure you received this item ?", "POS", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    purchaseOrderReceive.IsReceived = false;
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                            else
                            {
                                AddRemovePayment(purchaseOrderReceive);
                                CalculateSumReceivedQty(purchaseOrderReceive.PODResource);
                                _selectedPurchaseOrder.RaiseCanChangeStorePropertyChanged();
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                purchaseOrderReceive.IsReceived = false;
                                MessageBox.Show("Fix error(s) before receive this item.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                    else
                    {
                        AddRemovePayment(purchaseOrderReceive);
                        CalculateSumReceivedQty(purchaseOrderReceive.PODResource);
                        _selectedPurchaseOrder.RaiseCanChangeStorePropertyChanged();
                    }

                    break;

                case "RecQty":

                    purchaseOrderReceive.Amount = purchaseOrderReceive.RecQty * (purchaseOrderReceive.Price - purchaseOrderReceive.Discount);

                    // Calculate total receive of purchase order.
                    CalculateTotalReceiveOfPurchaseOrder();

                    break;

                case "Price":

                    purchaseOrderReceive.Amount = purchaseOrderReceive.RecQty * (purchaseOrderReceive.Price - purchaseOrderReceive.Discount);

                    break;
            }
        }

        #endregion

        #region ResourceReturnDetailCollectionChanged

        private void ResourceReturnDetailCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base_ResourceReturnDetailModel resourceReturnDetail;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.PurchaseOrder = _selectedPurchaseOrder;
                    resourceReturnDetail.ResourceReturnId = _selectedPurchaseOrder.ResourceReturn.Id;
                    resourceReturnDetail.ReturnedDate = DateTime.Now;
                    resourceReturnDetail.IsPurchaseOrderUsed = true;
                    resourceReturnDetail.IsTemporary = true;
                    resourceReturnDetail.PropertyChanged += ResourceReturnDetailPropertyChanged;
                }

                CheckReturned();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.PropertyChanged -= ResourceReturnDetailPropertyChanged;
                }
            }
        }

        #endregion

        #region ResourceReturnDetailPropertyChanged

        private void ResourceReturnDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourceReturnDetailModel resourceReturnDetail = sender as base_ResourceReturnDetailModel;

            switch (e.PropertyName)
            {
                case "PurchaseOrderDetail":

                    if (resourceReturnDetail.PurchaseOrderDetail != null)
                    {
                        resourceReturnDetail.OrderDetailResource = resourceReturnDetail.PurchaseOrderDetail.Resource.ToString();
                        resourceReturnDetail.ProductResource = resourceReturnDetail.PurchaseOrderDetail.ProductResource;
                        resourceReturnDetail.ItemCode = resourceReturnDetail.PurchaseOrderDetail.ItemCode;
                        resourceReturnDetail.ItemName = resourceReturnDetail.PurchaseOrderDetail.ItemName;
                        resourceReturnDetail.ItemAtribute = resourceReturnDetail.PurchaseOrderDetail.ItemAtribute;
                        resourceReturnDetail.ItemSize = resourceReturnDetail.PurchaseOrderDetail.ItemSize;
                        resourceReturnDetail.UnitName = resourceReturnDetail.PurchaseOrderDetail.UnitName;
                        resourceReturnDetail.Price = resourceReturnDetail.PurchaseOrderDetail.Price;
                        resourceReturnDetail.Discount = resourceReturnDetail.PurchaseOrderDetail.Discount;
                        CalculateAdditionReturnedQty(resourceReturnDetail);
                    }
                    else
                    {
                        resourceReturnDetail.OrderDetailResource = null;
                        resourceReturnDetail.ProductResource = null;
                        resourceReturnDetail.ItemCode = null;
                        resourceReturnDetail.ItemName = null;
                        resourceReturnDetail.ItemAtribute = null;
                        resourceReturnDetail.ItemSize = null;
                        resourceReturnDetail.Price = 0;
                        resourceReturnDetail.Discount = 0;
                        resourceReturnDetail.ReturnQty = 0;
                    }

                    break;

                case "Price":

                    resourceReturnDetail.Amount = resourceReturnDetail.ReturnQty * (resourceReturnDetail.Price - resourceReturnDetail.Discount);

                    break;

                case "ReturnQty":

                    resourceReturnDetail.Amount = resourceReturnDetail.ReturnQty * (resourceReturnDetail.Price - resourceReturnDetail.Discount);

                    break;

                case "Amount":

                    CalculateSubTotalForResourceReturn();

                    break;

                case "IsReturned":

                    // Calculate RecQty.
                    if (resourceReturnDetail.IsReturned)
                    {
                        if (!resourceReturnDetail.HasError)
                        {
                            MessageBoxResult result = MessageBox.Show("Are you sure you return this item ?", "POS", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    resourceReturnDetail.IsReturned = false;
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                            else
                            {
                                if (resourceReturnDetail.PurchaseOrder.Status < (short)PurchaseStatus.InProgress)
                                {
                                    if (resourceReturnDetail.PurchaseOrder.ResourceReturnDetailCollection.Any(x => x.IsReturned))
                                    {
                                        resourceReturnDetail.PurchaseOrder.Status = (short)PurchaseStatus.InProgress;
                                    }
                                }
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                resourceReturnDetail.IsReturned = false;
                                MessageBox.Show("Fix error(s) before return this item.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }

                        CalculateOnHandQtyWhenReturn(resourceReturnDetail);
                    }
                    else
                    {
                        CalculateOnHandQtyWhenReturn(resourceReturnDetail, true);
                    }

                    break;
            }
        }

        #endregion

        #region ResourceReturnPropertyChanged

        private void ResourceReturnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SubTotal":
                case "DiscountPercent":
                case "DiscountAmount":
                case "Freight":
                case "TotalRefund":
                case "Balance":

                    _selectedPurchaseOrder.IsDirty = true;

                    break;
            }
        }

        #endregion

        #region ResourcePaymentPropertyChanged

        private void ResourcePaymentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourcePaymentModel resourcePayment = sender as base_ResourcePaymentModel;
            switch (e.PropertyName)
            {
                case "TotalPaid":

                    _selectedPurchaseOrder.Paid = resourcePayment.TotalPaid;
                    _selectedPurchaseOrder.ResourcePaymentDetail.Paid = resourcePayment.TotalPaid;
                    _selectedPurchaseOrder.IsDirty = true;

                    break;

                case "Balance":

                    _selectedPurchaseOrder.Balance = resourcePayment.Balance;
                    _selectedPurchaseOrder.IsDirty = true;
                    if (resourcePayment.Balance == 0)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CheckFullWorkflow();
                        }));
                    }

                    break;

                case "TotalAmount":
                case "DateCreated":
                case "Remark":

                    _selectedPurchaseOrder.IsDirty = true;

                    break;
            }
        }

        #endregion

        #region ResourcePaymentDetailPropertyChanged

        private void ResourcePaymentDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourcePaymentDetailModel resourcePaymentDetail = sender as base_ResourcePaymentDetailModel;
            switch (e.PropertyName)
            {
                case "PaymentMethodId":

                    resourcePaymentDetail.PaymentMethod = _paymentMethodCollection.FirstOrDefault(x => x.Value == resourcePaymentDetail.PaymentMethodId).Text;
                    _selectedPurchaseOrder.IsDirty = true;

                    break;
            }
        }

        #endregion

        #endregion

        #region BackgroundWorker Events

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            GetPurchaseOrders(e.Argument.ToString());
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GetPurchaseOrder(e.UserState as base_PurchaseOrder);
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            if (!_isSearchMode)
            {
                if (_oldPurchaseOrder != null)
                {
                    SelectedPurchaseOrder = _oldPurchaseOrder;
                }
                else
                {
                    CreatePurchaseOrder();

                    AddProductsOutSide();
                }
            }
        }

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

        #region PurchaseOrderDetailComparer

        private class PurchaseOrderDetailComparer : IEqualityComparer<base_PurchaseOrderDetailModel>
        {
            #region IEqualityComparer<base_PurchaseOrderDetailModel> Members

            public bool Equals(base_PurchaseOrderDetailModel x, base_PurchaseOrderDetailModel y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.ProductResource == y.ProductResource;
            }

            public int GetHashCode(base_PurchaseOrderDetailModel obj)
            {
                return obj.ProductResource.GetHashCode();
            }

            #endregion
        }

        #endregion
    }
}
