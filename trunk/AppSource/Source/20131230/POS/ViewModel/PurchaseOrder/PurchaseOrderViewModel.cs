using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using BarcodeLib;
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
    public partial class PurchaseOrderViewModel : ViewModelBase
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
        /// Holds old selected PurchaseOrder.
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

        /// <summary>
        /// Holds vendor identity.
        /// </summary>
        private Guid _vendorResource = Guid.Empty;

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

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
                if (_productCollectionOutSide == null)
                {
                    _oldPurchaseOrder = param as base_PurchaseOrderModel;
                    if (_oldPurchaseOrder == null)
                    {
                        _vendorResource = (Guid)param;
                    }
                }

                IsSearchMode = false;
            }

            _ownerViewModel = App.Current.MainWindow.DataContext;

            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunWorkerCompleted);


            //Initial Auto Complete Search
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 1);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
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
                    ResetTimer();
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

        #region ProductBarcode

        private string _productBarcode;
        /// <summary>
        /// Gets or sets ProductBarcode.
        /// </summary>
        public string ProductBarcode
        {
            get
            {
                return _productBarcode;
            }
            set
            {
                if (_productBarcode != value)
                {
                    _productBarcode = value;
                    OnPropertyChanged(() => ProductBarcode);
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

        #region FocusDefault

        private bool _focusDefault;
        /// <summary>
        /// Gets or sets FocusDefault.
        /// </summary>
        public bool FocusDefault
        {
            get
            {
                return _focusDefault;
            }
            set
            {
                if (_focusDefault != value)
                {
                    _focusDefault = value;
                    OnPropertyChanged(() => FocusDefault);
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

        #region LockAndUnLockCommand

        private ICommand _lockAndUnLockCommand;
        /// <summary>
        /// When 'Lock' button clicked, LockAndUnLockCommand will executes. 
        /// </summary>
        public ICommand LockAndUnLockCommand
        {
            get
            {
                if (_lockAndUnLockCommand == null)
                {
                    _lockAndUnLockCommand = new RelayCommand(LockAndUnLockExecute, CanLockAndUnLockExecute);
                }
                return _lockAndUnLockCommand;
            }
        }

        #endregion

        #region OpenSelectTrackingNumberViewCommand

        private ICommand _openSelectTrackingNumberViewCommand;
        /// <summary>
        /// When 'Serial Tracking Detail' MenuItem clicked, OpenSelectTrackingNumberViewCommand will executes. 
        /// </summary>
        public ICommand OpenSelectTrackingNumberViewCommand
        {
            get
            {
                if (_openSelectTrackingNumberViewCommand == null)
                {
                    _openSelectTrackingNumberViewCommand = new RelayCommand(OpenSelectTrackingNumberViewExecute, CanOpenSelectTrackingNumberViewExecute);
                }
                return _openSelectTrackingNumberViewCommand;
            }
        }

        #endregion

        #region TotalRefundChangedCommand

        private ICommand _totalRefundChangedCommand;
        /// <summary>
        /// When TotalRefund property of return detail changed, TotalRefundChangedCommand will executes.
        /// </summary>
        public ICommand TotalRefundChangedCommand
        {
            get
            {
                if (_totalRefundChangedCommand == null)
                {
                    _totalRefundChangedCommand = new RelayCommand(TotalRefundChangedExecute);
                }
                return _totalRefundChangedCommand;
            }
        }

        #endregion

        #region PrintCommand

        private ICommand _printCommand;
        /// <summary>
        /// When 'Print' button clicked, NewCommand will executes. 
        /// </summary>
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand<string>(PrintExecute, CanPrintExecute);
                }
                return _printCommand;
            }
        }

        #endregion

        #region OpenSearchVendorCommand

        private ICommand _openSearchVendorCommand;
        public ICommand OpenSearchVendorCommand
        {
            get
            {
                if (_openSearchVendorCommand == null)
                {
                    _openSearchVendorCommand = new RelayCommand(OpenSearchVendorExecute, CanOpenSearchVendorExecute);
                }
                return _openSearchVendorCommand;
            }
        }

        #endregion

        #region ShowPurchaseOrderPaymentViewCommand

        private ICommand _showPurchaseOrderPaymentViewCommand;
        /// <summary>
        /// Show payment view.
        /// </summary>
        public ICommand ShowPurchaseOrderPaymentViewCommand
        {
            get
            {
                if (_showPurchaseOrderPaymentViewCommand == null)
                {
                    _showPurchaseOrderPaymentViewCommand = new RelayCommand(ShowPurchaseOrderPaymentViewExecute, CanShowPurchaseOrderPaymentViewExecute);
                }
                return _showPurchaseOrderPaymentViewCommand;
            }
        }

        #endregion

        #region GetProductCommand

        private ICommand _getProductCommand;
        /// <summary>
        /// Gets product.
        /// </summary>
        public ICommand GetProductCommand
        {
            get
            {
                if (_getProductCommand == null)
                {
                    _getProductCommand = new RelayCommand(GetProductExecute);
                }
                return _getProductCommand;
            }
        }

        #endregion

        #region ShowPaymentHistoryDetailCommand

        private ICommand _showPaymentHistoryDetailCommand;
        /// <summary>
        /// Show detail of payment history.
        /// </summary>
        public ICommand ShowPaymentHistoryDetailCommand
        {
            get
            {
                if (_showPaymentHistoryDetailCommand == null)
                {
                    _showPaymentHistoryDetailCommand = new RelayCommand<base_ResourcePaymentModel>(ShowPaymentHistoryDetailExecute);
                }
                return _showPaymentHistoryDetailCommand;
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
            if (_selectedPurchaseOrder == null || _selectedPurchaseOrder.IsFullWorkflow || _selectedPurchaseOrder.IsLocked)
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
            if (UserPermissions.AllowDeleteProductPurchaseOrder)
            {
                DeletePurchaseOrderDetail();
            }
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

            return UserPermissions.AllowDeleteProductPurchaseOrder;
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
            if (_selectedPurchaseOrderDetail == null)
            {
                return;
            }

            bool mustCancelEdit = false;

            if (_selectedPurchaseOrderDetail.HasError)
            {
                mustCancelEdit = true;
            }

            if (!mustCancelEdit && _selectedPurchaseOrder.PurchaseOrderReceiveCollection != null && Define.CONFIGURATION.IsAllowRGO != true)
            {
                decimal sumReceivedQty = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                    x.PODResource == _selectedPurchaseOrderDetail.Resource.ToString()).Sum(x => x.RecQty);
                decimal orderQty = _selectedPurchaseOrderDetail.Quantity;
                if (orderQty < sumReceivedQty)
                {
                    mustCancelEdit = true;
                }
            }

            if (mustCancelEdit)
            {
                (_selectedPurchaseOrderDetail as IEditableObject).CancelEdit();
                (_selectedPurchaseOrderDetail as IEditableObject).BeginEdit();
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text1, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            else if (_selectedPurchaseOrderDetail.Quantity != _selectedPurchaseOrderDetail.BackupQuantity)
            {
                _selectedPurchaseOrderDetail.BackupQuantity = _selectedPurchaseOrderDetail.Quantity;
                AddSerials(_selectedPurchaseOrderDetail, true);
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

            return UserPermissions.AllowAddPurchaseOrder;
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
                _selectedPurchaseOrder.IsLocked ||
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
                _selectedPurchaseOrder.IsLocked ||
                (_selectedPurchaseOrder.Status != (short)PurchaseStatus.Open && _selectedPurchaseOrder.HasReceivedItem))
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
                (dataGrid.SelectedItems.Cast<base_PurchaseOrderModel>()).Any(x => x.Status != (short)PurchaseStatus.Open && x.HasReceivedItem))
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

            return UserPermissions.AllowAddPurchaseOrder;
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
                _selectedPurchaseOrder.IsLocked ||
                !_selectedPurchaseOrder.CanPurchase ||
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
                //_selectedPurchaseOrder.IsFullWorkflow ||
                _selectedPurchaseOrder.IsLocked ||
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
                _selectedPurchaseOrder.IsLocked ||
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
            if (_selectedPurchaseOrderDetail == null || _selectedPurchaseOrderDetail.IsFullReceived)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region LockAndUnLockExecute

        /// <summary>
        /// Lock or UnLock PurchaseOrder.
        /// </summary>
        private void LockAndUnLockExecute()
        {
            LockAndUnLock();
        }

        #endregion

        #region CanLockAndUnLockExecute

        /// <summary>
        /// Determine whether can call LockAndUnLockExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanLockAndUnLockExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.HasError ||
                _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.HasError) ||
                _selectedPurchaseOrder.ResourceReturnDetailCollection.Any(x => x.HasError))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region OpenSelectTrackingNumberViewExecute

        /// <summary>
        /// Open SelectTrackingNumberView.
        /// </summary>
        private void OpenSelectTrackingNumberViewExecute()
        {
            OpenSelectTrackingNumberView(_selectedPurchaseOrderDetail, false, !_selectedPurchaseOrderDetail.IsFullReceived);
        }

        #endregion

        #region CanOpenSelectTrackingNumberViewExecute

        /// <summary>
        /// Determine whether can call OpenSelectTrackingNumberViewExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanOpenSelectTrackingNumberViewExecute()
        {
            if (_selectedPurchaseOrderDetail == null || !_selectedPurchaseOrderDetail.IsSerialTracking)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region TotalRefundChangedExecute

        /// <summary>
        /// Calls when TotalRefund property of return detail changed.
        /// </summary>
        private void TotalRefundChangedExecute()
        {
            decimal totalRefund = GetTotalRefundOfResourceReturn();
            if (_selectedPurchaseOrder.ResourceReturn.TotalRefund < totalRefund)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text28, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region PrintExecute

        private void PrintExecute(string obj)
        {
            // Show Purchase Order report
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            rpt.ShowReport("rptPurchaseOrder", "", obj, null, SelectedPurchaseOrder);

        }

        #endregion

        #region CanPrintExecute

        /// <summary>
        /// Determine whether can call PrintExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanPrintExecute(string obj)
        {
            if (string.IsNullOrEmpty(obj) ||
                _selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.IsNew ||
                _selectedPurchaseOrder.IsDirty ||
                _selectedPurchaseOrder.PurchaseOrderDetailCollection.IsDirty ||
                _selectedPurchaseOrder.PurchaseOrderReceiveCollection.IsDirty ||
                _selectedPurchaseOrder.ResourceReturnDetailCollection.IsDirty ||
                _selectedPurchaseOrder.Status < (short)PurchaseStatus.FullyReceived)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region OpenSearchVendorExecute

        /// <summary>
        /// Open search vendor view.
        /// </summary>
        private void OpenSearchVendorExecute()
        {
            OpenSearchVendor();
        }

        #endregion

        #region CanOpenSearchVendorExecute

        /// <summary>
        /// Determine whether can call OpenSearchVendorExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanOpenSearchVendorExecute()
        {
            if (_selectedPurchaseOrder == null || _vendorCollection == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ShowPurchaseOrderPaymentViewExecute

        /// <summary>
        /// Show payment history.
        /// </summary>
        private void ShowPurchaseOrderPaymentViewExecute()
        {
            ShowPurchaseOrderPaymentView();
        }

        #endregion

        #region CanShowPurchaseOrderPaymentViewExecute

        /// <summary>
        /// Determine whether can call ShowPurchaseOrderPaymentViewExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanShowPurchaseOrderPaymentViewExecute()
        {
            if (_selectedPurchaseOrder == null ||
                _selectedPurchaseOrder.IsNew ||
                (_selectedPurchaseOrder.PurchaseOrderDetailCollection != null && !_selectedPurchaseOrder.PurchaseOrderDetailCollection.Any()) ||
                (_selectedPurchaseOrder.Total > 0 && _selectedPurchaseOrder.Paid > 0 && _selectedPurchaseOrder.Balance == 0))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ShowPaymentHistoryDetailExecute

        /// <summary>
        /// Show detail of payment history.
        /// </summary>
        private void ShowPaymentHistoryDetailExecute(base_ResourcePaymentModel resourcePaymentModel)
        {
            ShowPaymentHistoryDetail(resourcePaymentModel);
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
                    base_GuestGroupRepository guestGroupRepository = new base_GuestGroupRepository();
                    VendorCollection = new CollectionBase<base_GuestModel>();

                    string vendorType = MarkType.Vendor.ToDescription();
                    IList<base_Guest> guests = guestRepository.GetAll(x => x.IsActived && x.Mark == vendorType);

                    base_GuestModel vendor;
                    base_GuestGroup guestGroup;
                    Guid guestId;
                    foreach (base_Guest guest in guests)
                    {
                        // Gets vendor.
                        guestRepository.Refresh(guest);
                        vendor = new base_GuestModel(guest);

                        // Gets group name.
                        if (!string.IsNullOrWhiteSpace(vendor.GroupResource))
                        {
                            guestId = new Guid(vendor.GroupResource);
                            guestGroup = guestGroupRepository.Get(x => x.Resource == guestId);
                            if (guestGroup != null)
                            {
                                vendor.GroupName = guestGroup.Name;
                            }
                        }

                        // Gets address.
                        vendor.AddressCollection = new ObservableCollection<base_GuestAddressModel>(
                            vendor.base_Guest.base_GuestAddress.Select(x => new base_GuestAddressModel(x)));
                        vendor.AddressModel = vendor.AddressCollection.SingleOrDefault(x => x.IsDefault);

                        // Add item in collection.
                        vendor.IsDirty = false;
                        _vendorCollection.Add(vendor);
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
                _selectedPurchaseOrder.VendorResource = newVendor.Resource.ToString();
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                            Expression<Func<base_PurchaseOrder, bool>> predicateChild = PredicateBuilder.True<base_PurchaseOrder>();
                            if (!string.IsNullOrWhiteSpace(_keyword))
                            {
                                predicateChild = PredicateBuilder.False<base_PurchaseOrder>();

                                // PurchaseOrderNo.
                                predicateChild = predicateChild.Or(x => x.PurchaseOrderNo.ToLower().Contains(_keyword.ToLower()));

                                // POCard.
                                predicateChild = predicateChild.Or(x => x.POCard != null && x.POCard.ToLower().Contains(_keyword.ToLower()));

                                // Company name based on VendorResource.
                                IEnumerable<string> vendorIDList = _vendorCollection.Where(x =>
                                        x.Mark == MarkType.Vendor.ToDescription() &&
                                        x.Company != null &&
                                        x.Company.ToLower().Contains(_keyword.ToLower())).Select(x => x.Resource.ToString());
                                predicateChild = predicateChild.Or(x => vendorIDList.Contains(x.VendorResource));

                                // Status.
                                IEnumerable<short> statusIDList = Common.PurchaseStatus.Where(x =>
                                        x.Text.ToLower().Contains(_keyword.ToLower())).Select(x => x.Value);
                                predicateChild = predicateChild.Or(x => statusIDList.Contains(x.Status));

                                decimal decimalKey;
                                if (decimal.TryParse(_keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalKey) && decimalKey != 0)
                                {
                                    // QtyOrdered.
                                    predicateChild = predicateChild.Or(x => x.QtyOrdered == decimalKey);

                                    // QtyDue.
                                    predicateChild = predicateChild.Or(x => x.QtyDue == decimalKey);

                                    // QtyReceived.
                                    predicateChild = predicateChild.Or(x => x.QtyReceived == decimalKey);

                                    // Total.
                                    predicateChild = predicateChild.Or(x => x.Total == decimalKey);

                                    // Paid.
                                    predicateChild = predicateChild.Or(x => x.Paid == decimalKey);

                                    // Balance.
                                    predicateChild = predicateChild.Or(x => x.Balance == decimalKey);

                                    // UnFilled.
                                    predicateChild = predicateChild.Or(x => x.UnFilled == decimalKey);
                                }

                                DateTime dateTimeKey;
                                if (DateTime.TryParseExact(_keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeKey))
                                {
                                    int year = dateTimeKey.Year;
                                    int month = dateTimeKey.Month;
                                    int day = dateTimeKey.Day;

                                    // PurchasedDate.
                                    predicateChild = predicateChild.Or(x =>
                                        x.PurchasedDate.Year.Equals(year) &&
                                        x.PurchasedDate.Month.Equals(month) &&
                                        x.PurchasedDate.Day.Equals(day));

                                    // PaymentDueDate.
                                    predicateChild = predicateChild.Or(x =>
                                        x.PaymentDueDate.HasValue &&
                                        x.PaymentDueDate.Value.Year.Equals(year) &&
                                        x.PaymentDueDate.Value.Month.Equals(month) &&
                                        x.PaymentDueDate.Value.Day.Equals(day));

                                    // ShipDate.
                                    predicateChild = predicateChild.Or(x =>
                                        x.ShipDate.HasValue &&
                                        x.ShipDate.Value.Year.Equals(year) &&
                                        x.ShipDate.Value.Month.Equals(month) &&
                                        x.ShipDate.Value.Day.Equals(day));
                                }
                            }

                            _predicate = _predicate.And(x => !x.IsPurge && !x.IsLocked).And(predicateChild);
                        }

                        // Initialize PurchaseOrderCollection.
                        PurchaseOrderCollection = new CollectionBase<base_PurchaseOrderModel>();
                        PurchaseOrderTotal = purchaseOrderRepository.GetIQueryable(_predicate).Count();
                    }

                    IList<base_PurchaseOrder> purchaseOrders = purchaseOrderRepository.GetRangeDescending(_purchaseOrderCollection.Count, NumberOfDisplayItems, x => x.Id, _predicate);
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                purchaseOrderModel.StatusItem = Common.PurchaseStatus.FirstOrDefault(x => x.Value == purchaseOrderModel.Status);
                purchaseOrderModel.ResourceReturn = new base_ResourceReturnModel();
                purchaseOrderModel.PurchaseOrderDetailCollection = new CollectionBase<base_PurchaseOrderDetailModel>();
                purchaseOrderModel.PurchaseOrderReceiveCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
                purchaseOrderModel.PaymentCollection = new CollectionBase<base_ResourcePaymentModel>();
                purchaseOrderModel.ResourceReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
                if (_vendorCollection != null)
                {
                    // Gets selected vendor.
                    base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Resource.ToString() == purchaseOrderModel.VendorResource);
                    if (vendor != null)
                    {
                        // Gets VendorName..
                        purchaseOrderModel.VendorName = vendor.Company;
                    }
                }
                if (_storeCollection != null)
                {
                    // Gets selected store.
                    base_StoreModel store = _storeCollection.ElementAt(purchaseOrderModel.StoreCode);
                    if (store != null)
                    {
                        purchaseOrderModel.StoreName = store.Name;
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
                FocusDefaultElement();
            }
        }

        #endregion

        #region CreatePurchaseOrder

        /// <summary>
        /// Create new purchase order.
        /// </summary>
        private void CreatePurchaseOrder()
        {
            Barcode barcodeObject = new Barcode()
            {
                IncludeLabel = true
            };
            base_StoreModel store = _storeCollection.ElementAt(Define.StoreCode);
            base_PurchaseOrderModel purchaseOrder = new base_PurchaseOrderModel();
            purchaseOrder.Status = (short)PurchaseStatus.Open;
            purchaseOrder.PurchaseOrderNo = DateTime.Now.ToString(Define.PurchaseOrderNoFormat);
            barcodeObject.Encode(TYPE.UPCA, purchaseOrder.PurchaseOrderNo, 200, 70);
            purchaseOrder.POCard = barcodeObject.RawData;
            purchaseOrder.POCardImg = barcodeObject.Encoded_Image_Bytes;
            purchaseOrder.PurchasedDate = DateTime.Now.Date;
            purchaseOrder.ShipDate = DateTime.Now.Date;
            purchaseOrder.PaymentDueDate = purchaseOrder.ShipDate;
            purchaseOrder.StoreCode = Define.StoreCode;
            purchaseOrder.ShipAddress = store != null ? string.Format("{0}. {1}", store.City, store.Street) : null;
            purchaseOrder.Shift = Define.ShiftCode;
            purchaseOrder.Resource = Guid.NewGuid();
            if (_vendorResource != Guid.Empty)
            {
                purchaseOrder.VendorResource = _vendorResource.ToString();
            }
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
            purchaseOrder.PurchaseOrderDetailCollection = new CollectionBase<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderDetailReceiveCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderDetailReturnCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderReceiveCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
            purchaseOrder.PurchaseOrderReceiveCollection.CollectionChanged += PurchaseOrderReceiveCollectionChanged;
            purchaseOrder.PaymentCollection = new CollectionBase<base_ResourcePaymentModel>();
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
            OnPropertyChanged(() => AllowPurchaseReceive);
            OnPropertyChanged(() => AllowPurchaseOrderReturn);
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
                base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Resource.ToString() == _selectedPurchaseOrder.VendorResource);
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

            RefreshProduct(_selectedProduct);

            MessageBoxResult result = MessageBoxResult.Yes;
            if (_selectedProduct.IsUnOrderAble)
            {
                result = Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.Text2, _selectedProduct.ProductName), Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }

            if (result == MessageBoxResult.No)
            {
                SelectedProduct = null;
                return;
            }

            base_UOMRepository UOMRepository = new base_UOMRepository();
            base_ProductUOMModel productUOM;
            base_PurchaseOrderDetailModel purchaseOrderDetail = new base_PurchaseOrderDetailModel();
            purchaseOrderDetail.PurchaseOrderId = _selectedPurchaseOrder.Id;
            purchaseOrderDetail.PurchaseOrder = _selectedPurchaseOrder;
            purchaseOrderDetail.ProductResource = _selectedProduct.Resource.ToString();
            purchaseOrderDetail.ItemCode = _selectedProduct.Code;
            purchaseOrderDetail.ItemName = _selectedProduct.ProductName;
            purchaseOrderDetail.ItemAtribute = _selectedProduct.Attribute;
            purchaseOrderDetail.ItemSize = _selectedProduct.Size;
            purchaseOrderDetail.ItemDescription = _selectedProduct.Description;
            purchaseOrderDetail.IsSerialTracking = _selectedProduct.IsSerialTracking;

            base_PurchaseOrderDetailModel purchaseOrderDetailContainProductBefore = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                x.ProductResource == purchaseOrderDetail.ProductResource);
            if (purchaseOrderDetailContainProductBefore != null)
            {
                purchaseOrderDetail.OnHandQtyOnBaseUnit = purchaseOrderDetailContainProductBefore.OnHandQtyOnBaseUnit;
                purchaseOrderDetail.OnHandQtyOnBaseUnitTemp = purchaseOrderDetailContainProductBefore.OnHandQtyOnBaseUnitTemp;
                purchaseOrderDetail.UOMCollection = purchaseOrderDetailContainProductBefore.UOMCollection;
            }
            else
            {
                purchaseOrderDetail.OnHandQtyOnBaseUnit = GetOnHandQty(_selectedPurchaseOrder.StoreCode, _selectedProduct);
                purchaseOrderDetail.OnHandQtyOnBaseUnitTemp = purchaseOrderDetail.OnHandQtyOnBaseUnit;
                purchaseOrderDetail.UOMCollection = GetUOMCollection(_selectedProduct.base_Product);
            }
            if (purchaseOrderDetail.UOMCollection == null)
            {
                purchaseOrderDetail.UOMCollection = new CollectionBase<base_ProductUOMModel>();
            }

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
                purchaseOrderDetail.BaseUOM = productUOM.Name;
                purchaseOrderDetail.OnHandQty = Math.Round((decimal)purchaseOrderDetail.OnHandQtyOnBaseUnit / productUOM.BaseUnitNumber, 2);
                purchaseOrderDetail.Price = productUOM.RegularPrice;
            }
            else
            {
                purchaseOrderDetail.Price = 0;
            }

            purchaseOrderDetail.Quantity = _selectedProduct.DefaultQuantity;
            purchaseOrderDetail.BackupQuantity = 1;
            purchaseOrderDetail.DueQty = 1;
            purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * purchaseOrderDetail.Price;
            purchaseOrderDetail.Resource = Guid.NewGuid();

            purchaseOrderDetail.PropertyChanged += PurchaseOrderDetailPropertyChanged;
            purchaseOrderDetail.IsNew = true;
            _selectedPurchaseOrder.PurchaseOrderDetailCollection.Add(purchaseOrderDetail);
            _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Add(purchaseOrderDetail);

            AddSerials(purchaseOrderDetail, true);

            CalculateSubTotalForPurchaseOrder();

            // Calculate order quantity of purchase order.
            CalculateOrderQtyOfPurchaseOrder();

            SelectedProduct = null;

            // Determine status.
            if (_selectedPurchaseOrder.Status == (short)PurchaseStatus.FullyReceived)
            {
                _selectedPurchaseOrder.Status = (short)PurchaseStatus.InProgress;
            }
        }

        /// <summary>
        /// Add a purchase order detail.
        /// </summary>
        /// <param name="product">Product to add on purchase order detail.</param>
        private base_PurchaseOrderDetailModel AddPurchaseOrderDetail(base_ProductModel product, bool hasPurchaseManyItem)
        {
            if (product == null)
            {
                return null;
            }

            RefreshProduct(product);

            MessageBoxResult result = MessageBoxResult.Yes;
            if (product.IsUnOrderAble)
            {
                result = Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.Text2, product.ProductName), Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }

            if (result == MessageBoxResult.No)
            {
                return null;
            }

            base_UOMRepository UOMRepository = new base_UOMRepository();
            base_ProductUOMModel productUOM;
            base_PurchaseOrderDetailModel purchaseOrderDetail = new base_PurchaseOrderDetailModel();
            purchaseOrderDetail.PurchaseOrderId = _selectedPurchaseOrder.Id;
            purchaseOrderDetail.PurchaseOrder = _selectedPurchaseOrder;
            purchaseOrderDetail.ProductResource = product.Resource.ToString();
            purchaseOrderDetail.ItemCode = product.Code;
            purchaseOrderDetail.ItemName = product.ProductName;
            purchaseOrderDetail.ItemAtribute = product.Attribute;
            purchaseOrderDetail.ItemSize = product.Size;
            purchaseOrderDetail.ItemDescription = product.Description;
            purchaseOrderDetail.IsSerialTracking = product.IsSerialTracking;

            base_PurchaseOrderDetailModel purchaseOrderDetailContainProductBefore = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                x.ProductResource == purchaseOrderDetail.ProductResource);
            if (purchaseOrderDetailContainProductBefore != null)
            {
                purchaseOrderDetail.OnHandQtyOnBaseUnit = purchaseOrderDetailContainProductBefore.OnHandQtyOnBaseUnit;
                purchaseOrderDetail.OnHandQtyOnBaseUnitTemp = purchaseOrderDetailContainProductBefore.OnHandQtyOnBaseUnitTemp;
                purchaseOrderDetail.UOMCollection = purchaseOrderDetailContainProductBefore.UOMCollection;
            }
            else
            {
                purchaseOrderDetail.OnHandQtyOnBaseUnit = GetOnHandQty(_selectedPurchaseOrder.StoreCode, product);
                purchaseOrderDetail.OnHandQtyOnBaseUnitTemp = purchaseOrderDetail.OnHandQtyOnBaseUnit;
                purchaseOrderDetail.UOMCollection = GetUOMCollection(product.base_Product);
            }
            if (purchaseOrderDetail.UOMCollection == null)
            {
                purchaseOrderDetail.UOMCollection = new CollectionBase<base_ProductUOMModel>();
            }

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
                purchaseOrderDetail.BaseUOM = productUOM.Name;
                purchaseOrderDetail.OnHandQty = Math.Round((decimal)purchaseOrderDetail.OnHandQtyOnBaseUnit / productUOM.BaseUnitNumber, 2);
                purchaseOrderDetail.Price = productUOM.RegularPrice;
            }
            else
            {
                purchaseOrderDetail.Price = 0;
            }


            purchaseOrderDetail.Quantity = product.DefaultQuantity;
            purchaseOrderDetail.BackupQuantity = product.ReOrderQuatity;
            purchaseOrderDetail.DueQty = product.ReOrderQuatity;
            purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * purchaseOrderDetail.Price;
            purchaseOrderDetail.Resource = Guid.NewGuid();

            purchaseOrderDetail.PropertyChanged += PurchaseOrderDetailPropertyChanged;
            purchaseOrderDetail.IsNew = true;
            _selectedPurchaseOrder.PurchaseOrderDetailCollection.Add(purchaseOrderDetail);
            _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Add(purchaseOrderDetail);

            if (hasPurchaseManyItem)
            {
                AddSerials(purchaseOrderDetail, true);
            }

            CalculateSubTotalForPurchaseOrder();

            // Calculate order quantity of purchase order.
            CalculateOrderQtyOfPurchaseOrder();

            return purchaseOrderDetail;
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
                            //ProductId = product.Id,
                            UOMId = UOM.Id,
                            Name = UOM.Name,
                            BaseUnitNumber = 1,
                            RegularPrice = product.OrderCost,
                            IsNew = false,
                            IsDirty = false
                        });
                    }

                    // Get product store by store code
                    base_ProductStore productStore = product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));

                    if (productStore != null)
                    {
                        // Gets the remaining units.
                        foreach (base_ProductUOM item in productStore.base_ProductUOM)
                        {
                            UOMCollection.Add(new base_ProductUOMModel(item)
                            {
                                Name = item.base_UOM.Name,
                                RegularPrice = product.OrderCost * item.BaseUnitNumber,
                                IsDirty = false
                            });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
        private decimal GetOnHandQty(int index, base_ProductModel product)
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

        #region OpenSelectTrackingNumberView

        /// <summary>
        /// Open SelectTrackingNumberView.
        /// </summary>
        /// <param name="purchaseOrderDetail">PurchaseOrderDetail that contains serial list to edit.</param>
        /// <param name="isShowQuantity">True will show quantity TextBox.</param>
        /// <param name="canEdit">Determine whether can edit in this View.</param>
        private void OpenSelectTrackingNumberView(base_PurchaseOrderDetailModel purchaseOrderDetail, bool isShowQuantity, bool canEdit)
        {
            SelectTrackingNumberViewModel selectTrackingNumberViewModel = new SelectTrackingNumberViewModel(purchaseOrderDetail, isShowQuantity, canEdit);
            bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, selectTrackingNumberViewModel, "Tracking Serial Number");
            if (result == true && canEdit)
            {
                purchaseOrderDetail = selectTrackingNumberViewModel.PurchaseOrderDetailModel;
            }
        }

        #endregion

        #region OpenMultiTrackingNumberView

        /// <summary>
        /// Open MultiTrackingNumberView.
        /// </summary>
        /// <param name="purchaseOrderDetailList">PurchaseOrderDetail list to add serial.</param>
        private void OpenMultiTrackingNumberView(IEnumerable<base_PurchaseOrderDetailModel> purchaseOrderDetailList)
        {
            MultiTrackingNumberViewModel multiTrackingNumberViewModel = new MultiTrackingNumberViewModel(purchaseOrderDetailList);
            _dialogService.ShowDialog<MultiTrackingNumberView>(_ownerViewModel, multiTrackingNumberViewModel, "Multi Tracking Serial");
        }

        #endregion

        #region AddSerials

        /// <summary>
        ///  Add serials.
        /// </summary>
        /// <param name="purchaseOrderDetail">PurchaseOrderDetail that contains serial list to edit.</param>
        /// <param name="isShowQuantity">True will show quantity TextBox.</param>
        /// <param name="canEdit">Determine whether can edit in this View.</param>
        private void AddSerials(base_PurchaseOrderDetailModel purchaseOrderDetail, bool isShowQuantity, bool canEdit = true)
        {
            if (purchaseOrderDetail.IsSerialTracking && purchaseOrderDetail.Quantity > 0)
            {
                OpenSelectTrackingNumberView(purchaseOrderDetail, isShowQuantity, canEdit);
            }
        }

        /// <summary>
        /// Add serials.
        /// </summary>
        /// <param name="purchaseOrderDetailList">PurchaseOrderDetail list to add serial.</param>
        private void AddSerials(IEnumerable<base_PurchaseOrderDetailModel> purchaseOrderDetailList)
        {
            OpenMultiTrackingNumberView(purchaseOrderDetailList);
        }

        #endregion

        #region DeletePurchaseOrderDetail

        /// <summary>
        /// Delete purchase detail item.
        /// </summary>
        private void DeletePurchaseOrderDetail()
        {
            // Check payment.
            if (_selectedPurchaseOrder.HasPayment)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Purchase order has paid. You can not delete this item.", Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check item received.
            if (_selectedPurchaseOrderDetail.HasReceivedItem)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text3, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Holds PurchaseOrderDetail's resource.
                    string id = _selectedPurchaseOrderDetail.Resource.ToString();
                    base_PurchaseOrderDetailModel purchaseOrderDetailDelete = _selectedPurchaseOrderDetail;

                    _selectedPurchaseOrder.PurchaseOrderDetailCollection.Remove(purchaseOrderDetailDelete);
                    _selectedPurchaseOrder.PurchaseOrderDetailReceiveCollection.Remove(purchaseOrderDetailDelete);

                    CalculateSubTotalForPurchaseOrder();

                    // Calculate order quantity of purchase order.
                    CalculateOrderQtyOfPurchaseOrder();

                    // Determine status.
                    if (_selectedPurchaseOrder.Status < (short)PurchaseStatus.PaidInFull)
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
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text5, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    else if (purchaseOrderReceiveView.CurrentAddItem != null)
                    {
                        isContainsErrorItem = object.ReferenceEquals(purchaseOrderReceiveView.CurrentAddItem, selectedPurchaseOrderReceive);
                    }
                    else
                    {
                        isContainsErrorItem = true;
                    }
                }
            }

            if (purchaseOrderReceiveError == null || isContainsErrorItem)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text6, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel(false, false, false, false, false, false);
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, "Search Product");
            if (dialogResult == true)
            {
                List<base_PurchaseOrderDetailModel> serialPurchaseOrderDetails = new List<base_PurchaseOrderDetailModel>();
                base_PurchaseOrderDetailModel purchaseOrderDetailModel;
                foreach (base_ProductModel product in productSearchViewModel.SelectedProducts)
                {
                    purchaseOrderDetailModel = AddPurchaseOrderDetail(product, false);
                    if (purchaseOrderDetailModel != null && purchaseOrderDetailModel.IsSerialTracking)
                    {
                        serialPurchaseOrderDetails.Add(purchaseOrderDetailModel);
                    }
                }

                if (serialPurchaseOrderDetails.Any())
                {
                    AddSerials(serialPurchaseOrderDetails);
                }
            }
        }

        #endregion

        #region SearchPO

        /// <summary>
        /// Search PurchaseOrder.
        /// </summary>
        private void SearchPO()
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();

            _hasUsedAdvanceSearch = false;
            _backgroundWorker.RunWorkerAsync("Load");
        }

        #endregion

        #region SearchPOAdvance

        /// <summary>
        /// Search PurchaseOrder with advance options.
        /// </summary>
        private void SearchPOAdvance()
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();

            POAdvanceSearchViewModel POAdvanceSearchViewModel = new POAdvanceSearchViewModel(_vendorCollection);
            bool? dialogResult = _dialogService.ShowDialog<POAdvanceSearchView>(_ownerViewModel, POAdvanceSearchViewModel, "Advance Search");
            if (dialogResult == true)
            {
                // Reset search.
                Keyword = null;

                _hasUsedAdvanceSearch = true;
                _predicate = POAdvanceSearchViewModel.Predicate.And(x => !x.IsLocked);
                _backgroundWorker.RunWorkerAsync("Load");
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save purchase order.
        /// </summary>
        private bool Save()
        {
            bool isSuccess = false;

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
                base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Resource.ToString() == _selectedPurchaseOrder.VendorResource);
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
                    if (_selectedPurchaseOrder.Status < (short)PurchaseStatus.InProgress &&
                        _selectedPurchaseOrder.PurchaseOrderDetailCollection.Any())
                    {
                        // Determine status.
                        _selectedPurchaseOrder.Status = (short)PurchaseStatus.InProgress;
                    }
                    _selectedPurchaseOrder.DateCreated = now;
                    _selectedPurchaseOrder.ToEntity();
                    purchaseOrderRepository.Add(_selectedPurchaseOrder.base_PurchaseOrder);
                    purchaseOrderRepository.Commit();
                    _selectedPurchaseOrder.Id = _selectedPurchaseOrder.base_PurchaseOrder.Id;
                    _selectedPurchaseOrder.IsNew = false;
                    _selectedPurchaseOrder.IsDirty = false;

                    // Insert PurchaseOrderDetail.
                    base_ProductUOMModel newProductUOM;
                    foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
                    {
                        // Update QuantityOnOrder.
                        newProductUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.UOMId);
                        if (newProductUOM != null)
                        {
                            decimal increaseQtyQuantityOnOrder = (item.Quantity - item.ReceivedQty) * newProductUOM.BaseUnitNumber;
                            if (increaseQtyQuantityOnOrder != 0)
                            {
                                productRepository.UpdateQuantityOnOrder(item.ProductResource, _selectedPurchaseOrder.StoreCode, increaseQtyQuantityOnOrder);
                            }
                        }

                        // Insert item.
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
                    decimal totalRefund = GetTotalRefundOfResourceReturn();
                    if (_selectedPurchaseOrder.ResourceReturn.TotalRefund < totalRefund)
                    {
                        if (Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text28, Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            return isSuccess;
                        }
                    }

                    // Update PurchaseOrder.
                    // Hold old store.
                    int oldStore = _selectedPurchaseOrder.base_PurchaseOrder.StoreCode;
                    if (_selectedPurchaseOrder.Status < (short)PurchaseStatus.InProgress &&
                        _selectedPurchaseOrder.PurchaseOrderDetailCollection.Any())
                    {
                        // Determine status.
                        _selectedPurchaseOrder.Status = (short)PurchaseStatus.InProgress;
                    }
                    _selectedPurchaseOrder.IsReturned = _selectedPurchaseOrder.ResourceReturnDetailCollection.Any(x => x.IsReturned);
                    _selectedPurchaseOrder.DateUpdate = now;
                    _selectedPurchaseOrder.ToEntity();
                    purchaseOrderRepository.Commit();

                    string purchaseOrderID = _selectedPurchaseOrder.Resource.ToString();

                    // Insert new items on PurchaseOrderDetail. 
                    ObservableCollection<base_PurchaseOrderDetailModel> newItems = _selectedPurchaseOrder.PurchaseOrderDetailCollection.NewItems;
                    if (newItems.Count > 0)
                    {
                        base_ProductUOMModel newProductUOM;
                        foreach (base_PurchaseOrderDetailModel item in newItems)
                        {
                            // Update QuantityOnOrder.
                            newProductUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.UOMId);
                            if (newProductUOM != null)
                            {
                                decimal increaseQtyQuantityOnOrder = (item.Quantity - item.ReceivedQty) * newProductUOM.BaseUnitNumber;
                                if (increaseQtyQuantityOnOrder != 0)
                                {
                                    productRepository.UpdateQuantityOnOrder(item.ProductResource, _selectedPurchaseOrder.StoreCode, increaseQtyQuantityOnOrder);
                                }
                            }

                            // Insert item.
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
                        base_ProductUOMModel newProductUOM;
                        base_ProductUOMModel oldProductUOM;
                        foreach (base_PurchaseOrderDetailModel item in dirtyItems)
                        {
                            oldProductUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.base_PurchaseOrderDetail.UOMId);
                            newProductUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.UOMId);

                            // Change store.
                            if (_selectedPurchaseOrder.StoreCode != oldStore)
                            {
                                // Subtract QuantityOnOrder on old store.
                                if (oldProductUOM != null)
                                {
                                    decimal increaseQtyQuantityOnOrder = -(item.base_PurchaseOrderDetail.Quantity * oldProductUOM.BaseUnitNumber);
                                    if (increaseQtyQuantityOnOrder != 0)
                                    {
                                        productRepository.UpdateQuantityOnOrder(item.ProductResource, oldStore, increaseQtyQuantityOnOrder);
                                    }
                                }

                                // Increase QuantityOnOrder on new store.
                                if (newProductUOM != null)
                                {
                                    decimal increaseQtyQuantityOnOrder = (item.Quantity - item.ReceivedQty) * newProductUOM.BaseUnitNumber;
                                    if (increaseQtyQuantityOnOrder != 0)
                                    {
                                        productRepository.UpdateQuantityOnOrder(item.ProductResource, _selectedPurchaseOrder.StoreCode, increaseQtyQuantityOnOrder);
                                    }
                                }
                            }
                            else
                            {
                                // Update QuantityOnOrder.
                                if (newProductUOM != null && oldProductUOM != null)
                                {
                                    decimal increaseQtyQuantityOnOrder = (item.Quantity * newProductUOM.BaseUnitNumber) -
                                        (item.base_PurchaseOrderDetail.Quantity * oldProductUOM.BaseUnitNumber) -
                                        (item.ReceivedQty * newProductUOM.BaseUnitNumber - item.base_PurchaseOrderDetail.ReceivedQty * oldProductUOM.BaseUnitNumber);
                                    if (increaseQtyQuantityOnOrder != 0)
                                    {
                                        productRepository.UpdateQuantityOnOrder(item.ProductResource, _selectedPurchaseOrder.StoreCode, increaseQtyQuantityOnOrder);
                                    }
                                }
                            }

                            // Update item.
                            item.ToEntity();
                            purchaseOrderDetailRepository.Commit();
                            item.IsDirty = false;
                        }
                    }

                    // Insert new items on PurchaseOrderReceive. 
                    ObservableCollection<base_PurchaseOrderReceiveModel> newPORS = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.NewItems;
                    if (newPORS.Count > 0)
                    {
                        // Gets new received items group by ProductResource.
                        var query = newPORS.Where(x => x.IsReceived).GroupBy(x => x.ProductResource).ToList();

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

                        if (query.Any())
                        {
                            List<base_PurchaseOrderReceiveModel> group;
                            decimal maxPrice = 0;
                            decimal sumPriceReceivedQty;
                            decimal totalReceivedQty;
                            base_ProductUOMModel productUOMBase;
                            CollectionBase<base_ProductUOMModel> UOMCollection;
                            foreach (var item in query)
                            {
                                // Gets items in group.
                                group = item.ToList();
                                // Gets max price.
                                maxPrice = Math.Round(group.Max(x => x.PurchaseOrderDetail.Price / x.PurchaseOrderDetail.UOMCollection.FirstOrDefault(y => y.UOMId == x.PurchaseOrderDetail.UOMId).BaseUnitNumber), 2);
                                sumPriceReceivedQty = 0;
                                totalReceivedQty = 0;
                                foreach (base_PurchaseOrderReceiveModel receivedItem in group)
                                {
                                    productUOMBase = receivedItem.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == receivedItem.PurchaseOrderDetail.UOMId);
                                    // (receivedItem.PurchaseOrderDetail.Price / productUOMBase.BaseUnitNumber) * (receivedItem.RecQty * productUOMBase.BaseUnitNumber)
                                    // Shortcut: (receivedItem.PurchaseOrderDetail.Price * receivedItem.RecQty)
                                    sumPriceReceivedQty += (receivedItem.PurchaseOrderDetail.Price * receivedItem.RecQty);
                                    totalReceivedQty += (receivedItem.RecQty * productUOMBase.BaseUnitNumber);
                                }

                                productRepository.UpdateCost(item.Key, _selectedPurchaseOrder.StoreCode, maxPrice, sumPriceReceivedQty, totalReceivedQty);
                                productRepository.UpdateProductStore(item.Key, _selectedPurchaseOrder.StoreCode, totalReceivedQty, sumPriceReceivedQty, 0, 0);

                                // Update UOM collection.
                                UOMCollection = group.First().PurchaseOrderDetail.UOMCollection;
                                foreach (base_ProductUOMModel productUOM in UOMCollection)
                                {
                                    productUOM.RegularPrice = maxPrice * productUOM.BaseUnitNumber;
                                }
                            }
                            productRepository.Commit();
                        }
                    }

                    // Update dirty items on PurchaseOrderReceive. 
                    ObservableCollection<base_PurchaseOrderReceiveModel> dirtyPORS = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.DirtyItems;
                    if (dirtyPORS.Count > 0)
                    {
                        // Gets new received items group by ProductResource.
                        var query = dirtyPORS.Where(x => x.IsReceived && !x.base_PurchaseOrderReceive.IsReceived).GroupBy(x => x.ProductResource).ToList();

                        foreach (base_PurchaseOrderReceiveModel item in dirtyPORS)
                        {
                            item.ToEntity();
                            purchaseOrderReceiveRepository.Commit();
                            item.IsDirty = false;
                        }

                        if (query.Any())
                        {
                            List<base_PurchaseOrderReceiveModel> group;
                            decimal maxPrice = 0;
                            decimal sumPriceReceivedQty;
                            decimal totalReceivedQty;
                            base_ProductUOMModel productUOMBase;
                            CollectionBase<base_ProductUOMModel> UOMCollection;
                            foreach (var item in query)
                            {
                                // Gets items in group.
                                group = item.ToList();
                                // Gets max price.
                                maxPrice = Math.Round(group.Max(x => x.PurchaseOrderDetail.Price / x.PurchaseOrderDetail.UOMCollection.FirstOrDefault(y => y.UOMId == x.PurchaseOrderDetail.UOMId).BaseUnitNumber), 2);
                                sumPriceReceivedQty = 0;
                                totalReceivedQty = 0;
                                foreach (base_PurchaseOrderReceiveModel receivedItem in group)
                                {
                                    productUOMBase = receivedItem.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == receivedItem.PurchaseOrderDetail.UOMId);
                                    // (receivedItem.PurchaseOrderDetail.Price / productUOMBase.BaseUnitNumber) * (receivedItem.RecQty * productUOMBase.BaseUnitNumber)
                                    // Shortcut: (receivedItem.PurchaseOrderDetail.Price * receivedItem.RecQty)
                                    sumPriceReceivedQty += (receivedItem.PurchaseOrderDetail.Price * receivedItem.RecQty);
                                    totalReceivedQty += (receivedItem.RecQty * productUOMBase.BaseUnitNumber);
                                }

                                productRepository.UpdateCost(item.Key, _selectedPurchaseOrder.StoreCode, maxPrice, sumPriceReceivedQty, totalReceivedQty);
                                productRepository.UpdateProductStore(item.Key, _selectedPurchaseOrder.StoreCode, totalReceivedQty, sumPriceReceivedQty, 0, 0);

                                // Update UOM collection.
                                UOMCollection = group.First().PurchaseOrderDetail.UOMCollection;
                                foreach (base_ProductUOMModel productUOM in UOMCollection)
                                {
                                    productUOM.RegularPrice = maxPrice * productUOM.BaseUnitNumber;
                                }
                            }
                            productRepository.Commit();
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
                        base_ProductUOMModel oldProductUOM;
                        foreach (base_PurchaseOrderDetailModel item in deletedItems)
                        {
                            // Update QuantityOnOrder on old store.
                            oldProductUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.base_PurchaseOrderDetail.UOMId);
                            if (oldProductUOM != null)
                            {
                                decimal increaseQtyQuantityOnOrder = -(item.base_PurchaseOrderDetail.Quantity * oldProductUOM.BaseUnitNumber);
                                if (increaseQtyQuantityOnOrder != 0)
                                {
                                    productRepository.UpdateQuantityOnOrder(item.ProductResource, oldStore, increaseQtyQuantityOnOrder);
                                }
                            }

                            // Delete item.
                            purchaseOrderDetailRepository.Delete(item.base_PurchaseOrderDetail);
                            purchaseOrderDetailRepository.Commit();
                        }

                        deletedItems.Clear();
                    }

                    // Insert new ResourcePayment.
                    ObservableCollection<base_ResourcePaymentModel> newResourcePaymentList = _selectedPurchaseOrder.PaymentCollection.NewItems;
                    if (newResourcePaymentList.Any())
                    {
                        foreach (base_ResourcePaymentModel resourcePayment in newResourcePaymentList)
                        {
                            resourcePayment.ToEntity();
                            resourcePaymentRepository.Add(resourcePayment.base_ResourcePayment);
                            resourcePaymentRepository.Commit();
                            resourcePayment.Id = resourcePayment.base_ResourcePayment.Id;
                            // Insert new ResourcePaymentDetail.
                            if (resourcePayment.PaymentDetailCollection != null && resourcePayment.PaymentDetailCollection.Any())
                            {
                                foreach (base_ResourcePaymentDetailModel resourcePaymentDetail in resourcePayment.PaymentDetailCollection)
                                {
                                    resourcePaymentDetail.ResourcePaymentId = resourcePayment.Id;
                                    resourcePaymentDetail.ToEntity();
                                    resourcePaymentDetailRepository.Add(resourcePaymentDetail.base_ResourcePaymentDetail);
                                    resourcePaymentDetailRepository.Commit();
                                    resourcePaymentDetail.Id = resourcePaymentDetail.base_ResourcePaymentDetail.Id;
                                    resourcePaymentDetail.EndUpdate();
                                }
                            }
                            resourcePayment.EndUpdate();
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
                        // Gets new returned items group by ProductResource.
                        var query = newRRDS.Where(x => x.IsReturned).GroupBy(x => x.ProductResource).ToList();

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

                        if (query.Any())
                        {
                            List<base_ResourceReturnDetailModel> group;
                            decimal sumPriceReturnedQty;
                            decimal totalReturnedQty;
                            base_ProductUOMModel productUOMBase;
                            foreach (var item in query)
                            {
                                // Gets items in group.
                                group = item.ToList();
                                sumPriceReturnedQty = 0;
                                totalReturnedQty = 0;
                                foreach (base_ResourceReturnDetailModel returnedItem in group)
                                {
                                    productUOMBase = returnedItem.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == returnedItem.PurchaseOrderDetail.UOMId);
                                    // (returnedItem.PurchaseOrderDetail.Price / productUOMBase.BaseUnitNumber) * (returnedItem.ReturnQty * productUOMBase.BaseUnitNumber)
                                    // Shortcut: (returnedItem.PurchaseOrderDetail.Price * returnedItem.ReturnQty)
                                    sumPriceReturnedQty += (returnedItem.PurchaseOrderDetail.Price * returnedItem.ReturnQty);
                                    totalReturnedQty += (returnedItem.ReturnQty * productUOMBase.BaseUnitNumber);
                                }

                                productRepository.UpdateProductStore(item.Key, _selectedPurchaseOrder.StoreCode, 0, 0, totalReturnedQty, sumPriceReturnedQty);
                            }
                            productRepository.Commit();
                        }
                    }

                    // Update dirty items on ResourceReturnDetail. 
                    ObservableCollection<base_ResourceReturnDetailModel> dirtyRRDS = _selectedPurchaseOrder.ResourceReturnDetailCollection.DirtyItems;
                    if (dirtyRRDS.Count > 0)
                    {
                        // Gets new returned items group by ProductResource.
                        var query = dirtyRRDS.Where(x => x.IsReturned && !x.base_ResourceReturnDetail.IsReturned).GroupBy(x => x.ProductResource).ToList();

                        foreach (base_ResourceReturnDetailModel item in dirtyRRDS)
                        {
                            item.ToEntity();
                            resourceReturnDetailRepository.Commit();
                            item.IsDirty = false;
                        }

                        if (query.Any())
                        {
                            List<base_ResourceReturnDetailModel> group;
                            decimal sumPriceReturnedQty;
                            decimal totalReturnedQty;
                            base_ProductUOMModel productUOMBase;
                            foreach (var item in query)
                            {
                                // Gets items in group.
                                group = item.ToList();
                                sumPriceReturnedQty = 0;
                                totalReturnedQty = 0;
                                foreach (base_ResourceReturnDetailModel returnedItem in group)
                                {
                                    productUOMBase = returnedItem.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == returnedItem.PurchaseOrderDetail.UOMId);
                                    // (returnedItem.PurchaseOrderDetail.Price / productUOMBase.BaseUnitNumber) * (returnedItem.ReturnQty * productUOMBase.BaseUnitNumber)
                                    // Shortcut: (returnedItem.PurchaseOrderDetail.Price * returnedItem.ReturnQty)
                                    sumPriceReturnedQty += (returnedItem.PurchaseOrderDetail.Price * returnedItem.ReturnQty);
                                    totalReturnedQty += (returnedItem.ReturnQty * productUOMBase.BaseUnitNumber);
                                }

                                productRepository.UpdateProductStore(item.Key, _selectedPurchaseOrder.StoreCode, 0, 0, totalReturnedQty, sumPriceReturnedQty);
                            }
                            productRepository.Commit();
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
                    decimal currentOnHandQty = 0;
                    IEnumerable<base_PurchaseOrderDetailModel> productList = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Distinct(new PurchaseOrderDetailComparer());
                    foreach (base_PurchaseOrderDetailModel item in productList)
                    {
                        // Always quantity on base unit.
                        decimal increaseQty = item.OnHandQtyOnBaseUnit - item.OnHandQtyOnBaseUnitTemp;
                        if (increaseQty == 0)
                        {
                            continue;
                        }

                        if (increaseQty > 0)
                        {
                            currentOnHandQty = productRepository.UpdateOnHandQuantity(item.ProductResource, _selectedPurchaseOrder.StoreCode, increaseQty, false);
                        }
                        else
                        {
                            currentOnHandQty = productRepository.UpdateOnHandQuantity(item.ProductResource, _selectedPurchaseOrder.StoreCode, Math.Abs(increaseQty), true);
                        }

                        productRepository.Commit();

                        foreach (base_PurchaseOrderDetailModel itemFriend in _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                            x.ProductResource == item.ProductResource))
                        {
                            itemFriend.OnHandQtyOnBaseUnit = currentOnHandQty;
                            itemFriend.OnHandQtyOnBaseUnitTemp = itemFriend.OnHandQtyOnBaseUnit;
                            itemFriend.IsDirty = false;
                        }
                    }
                }

                UnitOfWork.CommitTransaction();

                _selectedPurchaseOrder.IsDirty = false;

                if (_selectedPurchaseOrder.IsFullWorkflow)
                {
                    IsSearchMode = true;
                }

                isSuccess = true;
            }
            catch (Exception exception)
            {
                UnitOfWork.RollbackTransaction();
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            finally
            {
                _selectedPurchaseOrder.RaiseCanPurchasePropertyChanged();
            }

            return isSuccess;
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
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text7, Language.Warning, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        Save();
                        isUnactive = true;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        // Not Save.
                        RestorePurchaseOrder();
                        isUnactive = true;
                    }
                    else
                    {
                        isUnactive = false;
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
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text7, Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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

        #region Lock

        /// <summary>
        /// Lock PurchaseOrder.
        /// </summary>
        private void Lock()
        {
            bool isSuccess = Save();
            if (isSuccess)
            {
                _purchaseOrderCollection.Remove(_selectedPurchaseOrder);
                IsSearchMode = true;
            }
        }

        #endregion

        #region UnLock

        /// <summary>
        /// Unlock PurchaseOrder.
        /// </summary>
        private void UnLock()
        {
            try
            {
                base_PurchaseOrderRepository purchaseOrderRepository = new base_PurchaseOrderRepository();
                // Update PurchaseOrder.
                _selectedPurchaseOrder.DateUpdate = DateTime.Now;
                _selectedPurchaseOrder.ToEntity();
                purchaseOrderRepository.Commit();
                _selectedPurchaseOrder.IsDirty = false;
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
        /// Delete PurchaseOrder.
        /// </summary>
        private void Delete()
        {
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                _selectedPurchaseOrder.VendorResource = selectedItem.VendorResource;
                _selectedPurchaseOrder.StoreCode = selectedItem.StoreCode;

                // Close search component.
                IsSearchMode = false;

                // Gets products order in root PurchaseOrder.
                IList<base_PurchaseOrderDetail> purchaseOrderDetails = purchaseOrderDetailRepository.GetAll(x => x.PurchaseOrderId == selectedItem.Id);
                base_ProductModel productModel;
                base_Product product;
                Guid podProductId;
                foreach (base_PurchaseOrderDetail purchaseOrderDetail in purchaseOrderDetails)
                {
                    // Gets and refresh product of item.
                    podProductId = new Guid(purchaseOrderDetail.ProductResource);
                    product = productRepository.Get(x => x.Resource == podProductId);
                    if (product != null)
                    {
                        productRepository.Refresh(product);
                        productModel = new base_ProductModel(product);

                        // Add new PurchaseOrderDetail.
                        AddPurchaseOrderDetail(productModel, false);
                    }
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                        _selectedPurchaseOrder.PurchaseOrderDetailCollection.Clear();
                        _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Clear();
                        _selectedPurchaseOrder.ResourceReturnDetailCollection.Clear();
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
            FocusDefaultElement();

            OnPropertyChanged(() => AllowPurchaseOrderReturn);
            OnPropertyChanged(() => AllowPurchaseReceive);
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
                    base_ProductModel productModel = null;
                    base_Product product;
                    base_ProductUOMModel productUOM;
                    Guid podProductId;
                    foreach (base_PurchaseOrderDetail item in purchaseOrderDetails)
                    {
                        purchaseOrderDetailRepository.Refresh(item);
                        purchaseOrderDetailModel = new base_PurchaseOrderDetailModel(item);

                        // Gets and refresh product of item.
                        podProductId = new Guid(item.ProductResource);
                        product = productRepository.Get(x => x.Resource == podProductId);
                        if (product != null)
                        {
                            productRepository.Refresh(product);
                            productModel = new base_ProductModel(product);
                        }

                        // Gets UOMCollection.
                        purchaseOrderDetailModelBefore = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                            x.ProductResource == item.ProductResource);
                        if (purchaseOrderDetailModelBefore != null && purchaseOrderDetailModelBefore.UOMCollection != null)
                        {
                            purchaseOrderDetailModel.UOMCollection = purchaseOrderDetailModelBefore.UOMCollection;
                        }
                        else
                        {
                            if (productModel != null)
                            {
                                purchaseOrderDetailModel.UOMCollection = GetUOMCollection(productModel.base_Product);
                            }
                        }
                        if (purchaseOrderDetailModel.UOMCollection == null)
                        {
                            purchaseOrderDetailModel.UOMCollection = new CollectionBase<base_ProductUOMModel>();
                        }

                        if (productModel != null)
                        {
                            purchaseOrderDetailModel.ItemDescription = productModel.Description;
                            purchaseOrderDetailModel.IsSerialTracking = productModel.IsSerialTracking;
                        }
                        productUOM = purchaseOrderDetailModel.UOMCollection.FirstOrDefault(x => x.UOMId == purchaseOrderDetailModel.UOMId);
                        if (productUOM != null)
                        {
                            purchaseOrderDetailModel.UnitName = productUOM.Name;
                        }
                        purchaseOrderDetailModel.OnHandQtyOnBaseUnit = GetOnHandQty(_selectedPurchaseOrder.StoreCode, productModel);
                        purchaseOrderDetailModel.OnHandQtyOnBaseUnitTemp = purchaseOrderDetailModel.OnHandQtyOnBaseUnit;
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
                        purchaseOrderReceiveModel.ItemDescription = purchaseOrderDetailModel.ItemDescription;
                        purchaseOrderReceiveModel.Amount = purchaseOrderReceiveModel.RecQty * (purchaseOrderReceiveModel.Price - purchaseOrderReceiveModel.Discount);
                        purchaseOrderReceiveModel.PODResource = purchaseOrderDetailModel.Resource.ToString();
                        purchaseOrderReceiveModel.PropertyChanged += PurchaseOrderReceivePropertyChanged;
                        purchaseOrderReceiveModel.IsDirty = false;
                        _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Add(purchaseOrderReceiveModel);
                    }
                    _selectedPurchaseOrder.PurchaseOrderReceiveCollection.CollectionChanged += PurchaseOrderReceiveCollectionChanged;

                    // Gets PaymentCollection of PurchaseOrder.
                    _selectedPurchaseOrder.PaymentCollection = new CollectionBase<base_ResourcePaymentModel>(resourcePaymentRepository.GetAll(x =>
                        x.DocumentResource == purchaseOrderID).Select(x => new base_ResourcePaymentModel(x)));

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
                        resourceReturnDetailModel.ItemDescription = purchaseOrderDetailModel.ItemDescription;
                        resourceReturnDetailModel.IsPurchaseOrderUsed = true;
                        resourceReturnDetailModel.PropertyChanged += ResourceReturnDetailPropertyChanged;
                        resourceReturnDetailModel.IsDirty = false;
                        _selectedPurchaseOrder.ResourceReturnDetailCollection.Add(resourceReturnDetailModel);
                    }
                    _selectedPurchaseOrder.ResourceReturnDetailCollection.CollectionChanged += ResourceReturnDetailCollectionChanged;

                    // Calculate total receive of purchase order.
                    CalculateTotalReceiveOfPurchaseOrder();

                    _selectedPurchaseOrder.RaiseCanChangeStorePropertyChanged();
                    _selectedPurchaseOrder.RaiseCanPurchasePropertyChanged();

                    _selectedPurchaseOrder.IsDirty = false;
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            finally
            {
                // Select default TabItem.
                _currentTabItem = (int)TabItems.Order;
                OnPropertyChanged(() => CurrentTabItem);
            }
        }

        #endregion

        #region CalculateSubTotalForPurchaseOrder

        /// <summary>
        /// Calculate SubTotal for PurchaseOrder.
        /// </summary>
        private void CalculateSubTotalForPurchaseOrder()
        {
            decimal subTotal = 0;
            foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                if (item.Amount.HasValue)
                {
                    subTotal += item.Amount.Value;
                }
            }
            _selectedPurchaseOrder.SubTotal = subTotal;
        }


        #endregion

        #region CalculateTotalForPurchaseOrder

        /// <summary>
        /// Calculate Total for PurchaseOrder.
        /// </summary>
        private void CalculateTotalForPurchaseOrder()
        {
            decimal subTotal = _selectedPurchaseOrder.SubTotal;
            decimal discountAmount = _selectedPurchaseOrder.DiscountAmount;
            decimal freight = _selectedPurchaseOrder.Freight;
            decimal taxAmount = _selectedPurchaseOrder.TaxAmount;
            _selectedPurchaseOrder.Total = subTotal - discountAmount + freight + taxAmount;
        }

        #endregion

        #region ReceiveAll

        /// <summary>
        /// Received all items.
        /// </summary>
        private void ReceiveAll()
        {
            bool isCalculate = false;
            decimal sumReceivedQty = 0;
            decimal additionReceivedQty = 0;
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
                    purchaseOrderReceive.ItemDescription = purchaseOrderDetail.ItemDescription;
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

            decimal sumReceivedQty = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                x.PODResource == purchaseOrderReceive.PODResource).Sum(x => x.RecQty);

            // Additon received quantity.
            decimal additionReceivedQty = purchaseOrderReceive.PurchaseOrderDetail.Quantity - sumReceivedQty;
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

            // Gets received items.
            IEnumerable<base_PurchaseOrderReceiveModel> receivedList = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                x.PODResource == id && x.IsReceived);

            base_ProductUOMModel productUOM;
            decimal sumReceivedQty = 0;
            decimal sumReceivedQtyOnBaseUnit = 0;
            foreach (base_PurchaseOrderReceiveModel item in receivedList)
            {
                if (item.PurchaseOrderDetail != null && item.PurchaseOrderDetail.UOMCollection != null)
                {
                    productUOM = item.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == item.PurchaseOrderDetail.UOMId);
                    if (productUOM != null)
                    {
                        sumReceivedQtyOnBaseUnit += productUOM.BaseUnitNumber * item.RecQty;
                        sumReceivedQty += item.RecQty;
                    }
                }
            }

            base_PurchaseOrderDetailModel purchaseOrderDetail = _selectedPurchaseOrder.PurchaseOrderDetailCollection.FirstOrDefault(x =>
                x.Resource.ToString() == id);
            if (purchaseOrderDetail != null && purchaseOrderDetail.UOMCollection != null)
            {
                productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == purchaseOrderDetail.UOMId);
                if (productUOM != null)
                {
                    decimal increase = sumReceivedQtyOnBaseUnit - (productUOM.BaseUnitNumber * purchaseOrderDetail.ReceivedQty);

                    // Update On-hand quantity in stock.
                    purchaseOrderDetail.OnHandQtyOnBaseUnit += increase;

                    IEnumerable<base_PurchaseOrderDetailModel> productList = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                        x.ProductResource == purchaseOrderDetail.ProductResource);
                    foreach (base_PurchaseOrderDetailModel itemFriend in productList)
                    {
                        itemFriend.OnHandQtyOnBaseUnit = purchaseOrderDetail.OnHandQtyOnBaseUnit;
                    }
                }
                purchaseOrderDetail.ReceivedQty = sumReceivedQty;
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text8, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
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
            decimal sum = 0;
            base_ProductUOMModel productUOM;
            foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                if (item.UOMCollection != null)
                {
                    productUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.UOMId);
                    if (productUOM != null)
                    {
                        sum += productUOM.BaseUnitNumber * item.Quantity;
                    }
                }
            }
            _selectedPurchaseOrder.QtyOrdered = (int)sum;

            CalculateDueQtyOfPurchaseOrder();
        }

        #endregion

        #region CalculateReceivedQtyOfPurchaseOrder

        /// <summary>
        /// Calculate received quantity of purchase order.
        /// </summary>
        private void CalculateReceivedQtyOfPurchaseOrder()
        {
            decimal sum = 0;
            base_ProductUOMModel productUOM;
            foreach (base_PurchaseOrderDetailModel item in _selectedPurchaseOrder.PurchaseOrderDetailCollection)
            {
                if (item.UOMCollection != null)
                {
                    productUOM = item.UOMCollection.FirstOrDefault(x => x.UOMId == item.UOMId);
                    if (productUOM != null)
                    {
                        sum += productUOM.BaseUnitNumber * item.ReceivedQty;
                    }
                }
            }
            _selectedPurchaseOrder.QtyReceived = (int)sum;

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
            decimal sum = 0;
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

            purchaseOrderDetail.RaiseCanChangeQuantityChanged();
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
                //!_selectedPurchaseOrder.PurchaseOrderDetailCollection.Any(x => !x.IsFullReceived) &&
                _selectedPurchaseOrder.Total > 0 &&
                _selectedPurchaseOrder.Paid > 0 &&
                _selectedPurchaseOrder.Balance <= 0)
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
            decimal sumReturnedQty = 0;
            decimal additionReturnQty = 0;
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
                    resourceReturnDetail.ItemDescription = purchaseOrderDetail.ItemDescription;
                    resourceReturnDetail.UnitName = purchaseOrderDetail.UnitName;
                    resourceReturnDetail.Price = purchaseOrderDetail.Price;
                    resourceReturnDetail.Discount = purchaseOrderDetail.Discount;
                    resourceReturnDetail.ReturnQty = additionReturnQty;
                    resourceReturnDetail.Amount = resourceReturnDetail.ReturnQty * (resourceReturnDetail.Price - resourceReturnDetail.Discount);
                    resourceReturnDetail.PurchaseOrderDetail = purchaseOrderDetail;
                    if (resourceReturnDetail.PurchaseOrderDetail != null && resourceReturnDetail.PurchaseOrderDetail.UOMCollection != null)
                    {
                        base_ProductUOMModel productUOM = resourceReturnDetail.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == resourceReturnDetail.PurchaseOrderDetail.UOMId);
                        if (productUOM != null)
                        {
                            resourceReturnDetail.ReturnQtyUOM = resourceReturnDetail.ReturnQty * productUOM.BaseUnitNumber;
                        }
                    }
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

            decimal sumReturnedQty = _selectedPurchaseOrder.ResourceReturnDetailCollection.Where(x =>
                x.OrderDetailResource == resourceReturnDetail.OrderDetailResource).Sum(x => x.ReturnQty);

            // Additon returned quantity.
            decimal additionReturnedQty = resourceReturnDetail.PurchaseOrderDetail.ReceivedQty - sumReturnedQty;
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
            decimal returnQtyTotal = 0;
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
                bool hasPurchaseManyItem = _productCollectionOutSide.Any(x => x.IsSerialTracking && x.ReOrderQuatity > 1);
                foreach (base_ProductModel product in _productCollectionOutSide)
                {
                    AddPurchaseOrderDetail(product, hasPurchaseManyItem);
                }

                if (!hasPurchaseManyItem)
                {
                    IEnumerable<base_PurchaseOrderDetailModel> serialPurchaseOrderDetails = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                        x.IsSerialTracking);
                    if (serialPurchaseOrderDetails.Any())
                    {
                        AddSerials(serialPurchaseOrderDetails);
                    }
                }
            }
        }

        #endregion

        #region SelectDefaultPurchaseOrder

        /// <summary>
        /// Select default PurchaseOrder.
        /// </summary>
        /// <param name="purchaseOrder">PurchaseOrder to select.</param>
        private void SelectDefaultPurchaseOrder(base_PurchaseOrderModel purchaseOrder)
        {
            if (purchaseOrder != null)
            {
                SelectedPurchaseOrder = _purchaseOrderCollection.FirstOrDefault(x => x.Id == purchaseOrder.Id);
                if (_selectedPurchaseOrder == null)
                {
                    if (purchaseOrder.IsNew)
                    {
                        CreatePurchaseOrder();
                    }
                    else
                    {
                        _purchaseOrderCollection.Add(purchaseOrder);
                        SelectedPurchaseOrder = purchaseOrder;
                        GetMoreInformation();
                    }
                }
                else
                {
                    _selectedPurchaseOrder.HasWantReturn = purchaseOrder.HasWantReturn;
                    GetMoreInformation();
                }

                if (_selectedPurchaseOrder.HasWantReturn)
                {
                    _currentTabItem = 3;
                    OnPropertyChanged(() => CurrentTabItem);
                    OnPropertyChanged(() => AllowPurchaseOrderReturn);
                    OnPropertyChanged(() => AllowPurchaseReceive);
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
            if (resourceReturnDetail.PurchaseOrderDetail != null && resourceReturnDetail.PurchaseOrderDetail.UOMCollection != null)
            {
                base_ProductUOMModel productUOM = resourceReturnDetail.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x =>
                    x.UOMId == resourceReturnDetail.PurchaseOrderDetail.UOMId);
                if (productUOM == null)
                {
                    return;
                }

                if (isIncrease)
                {
                    resourceReturnDetail.PurchaseOrderDetail.OnHandQtyOnBaseUnit += resourceReturnDetail.ReturnQty * productUOM.BaseUnitNumber;
                }
                else
                {
                    resourceReturnDetail.PurchaseOrderDetail.OnHandQtyOnBaseUnit -= resourceReturnDetail.ReturnQty * productUOM.BaseUnitNumber;
                }

                IEnumerable<base_PurchaseOrderDetailModel> productList = _selectedPurchaseOrder.PurchaseOrderDetailCollection.Where(x =>
                    x.ProductResource == resourceReturnDetail.PurchaseOrderDetail.ProductResource);
                foreach (base_PurchaseOrderDetailModel itemFriend in productList)
                {
                    itemFriend.OnHandQtyOnBaseUnit = resourceReturnDetail.PurchaseOrderDetail.OnHandQtyOnBaseUnit;
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

        #region CalculateTotalRefundForResourceReturn

        /// <summary>
        /// Calculate TotalRefund in return TabItem.
        /// </summary>
        private void CalculateTotalRefundForResourceReturn()
        {
            decimal totalRefund = 0;
            foreach (base_ResourceReturnDetailModel item in _selectedPurchaseOrder.ResourceReturnDetailCollection)
            {
                if (item.IsReturned)
                {
                    totalRefund += item.Amount;
                }
            }
            _selectedPurchaseOrder.ResourceReturn.TotalRefund = totalRefund;
        }

        /// <summary>
        /// Calculate TotalRefund in return TabItem.
        /// </summary>
        private decimal GetTotalRefundOfResourceReturn()
        {
            decimal totalRefund = 0;
            foreach (base_ResourceReturnDetailModel item in _selectedPurchaseOrder.ResourceReturnDetailCollection)
            {
                if (item.IsReturned)
                {
                    totalRefund += item.Amount;
                }
            }
            return totalRefund;
        }

        #endregion

        #region EditProduct

        /// <summary>
        /// Edit product.
        /// </summary>
        private void EditProduct()
        {
            //base_ProductModel product = _productCollection.FirstOrDefault(x => x.Resource.ToString() == _selectedPurchaseOrderDetail.ProductResource);

            base_ProductModel productModel = new base_ProductModel();
            productModel.Resource = new Guid(_selectedPurchaseOrderDetail.ProductResource);
            productModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>(_selectedPurchaseOrderDetail.UOMCollection);
            productModel.BaseUOMId = _selectedPurchaseOrderDetail.UOMId;
            productModel.RegularPrice = _selectedPurchaseOrderDetail.Price;
            productModel.CurrentPrice = _selectedPurchaseOrderDetail.Price;
            productModel.OnHandStore = _selectedPurchaseOrderDetail.Quantity;
            productModel.ProductName = _selectedPurchaseOrderDetail.ItemName;
            productModel.Attribute = _selectedPurchaseOrderDetail.ItemAtribute;
            productModel.Size = _selectedPurchaseOrderDetail.ItemSize;
            productModel.Description = _selectedPurchaseOrderDetail.ItemDescription;

            PopupEditProductViewModel viewModel = new PopupEditProductViewModel(productModel, true, !_selectedPurchaseOrderDetail.HasReceivedItem);
            bool? result = _dialogService.ShowDialog<PopupEditProductView>(_ownerViewModel, viewModel, "Edit product");
            if (result.HasValue && result.Value)
            {
                _selectedPurchaseOrderDetail.UOMId = viewModel.SelectedProductUOM.UOMId;
                _selectedPurchaseOrderDetail.Price = productModel.CurrentPrice;
                _selectedPurchaseOrderDetail.Quantity = (int)productModel.OnHandStore;
                _selectedPurchaseOrderDetail.ItemName = productModel.ProductName;
                _selectedPurchaseOrderDetail.ItemAtribute = productModel.Attribute;
                _selectedPurchaseOrderDetail.ItemSize = productModel.Size;
                _selectedPurchaseOrderDetail.ItemDescription = productModel.Description;

                IEnumerable<base_PurchaseOrderReceiveModel> receiveFriend = _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Where(x => x.PODResource == _selectedPurchaseOrderDetail.Resource.ToString());
                foreach (var item in receiveFriend)
                {
                    item.ItemName = _selectedPurchaseOrderDetail.ItemName;
                    item.ItemAtribute = _selectedPurchaseOrderDetail.ItemAtribute;
                    item.ItemSize = _selectedPurchaseOrderDetail.ItemSize;
                    item.ItemDescription = _selectedPurchaseOrderDetail.ItemDescription;
                }

                IEnumerable<base_ResourceReturnDetailModel> returnFriends = _selectedPurchaseOrder.ResourceReturnDetailCollection.Where(x => x.OrderDetailResource == _selectedPurchaseOrderDetail.Resource.ToString());
                foreach (var item in returnFriends)
                {
                    item.ItemName = _selectedPurchaseOrderDetail.ItemName;
                    item.ItemAtribute = _selectedPurchaseOrderDetail.ItemAtribute;
                    item.ItemSize = _selectedPurchaseOrderDetail.ItemSize;
                    item.ItemDescription = _selectedPurchaseOrderDetail.ItemDescription;
                }
            }
        }

        #endregion

        #region LockAndUnLock

        /// <summary>
        /// Lock or UnLock PurchaseOrder.
        /// </summary>
        private void LockAndUnLock()
        {
            CommitOrCancelChange();
            _selectedPurchaseOrder.IsLocked = !_selectedPurchaseOrder.IsLocked;
            if (_selectedPurchaseOrder.IsLocked)
            {
                Lock();
            }
            else
            {
                UnLock();
            }
        }

        #endregion

        #region RefreshProduct

        /// <summary>
        /// Refresh product.
        /// </summary>
        /// <param name="product"></param>
        private void RefreshProduct(base_ProductModel product)
        {
            try
            {
                // Refresh product.
                base_ProductRepository productRepository = new Repository.base_ProductRepository();
                productRepository.Refresh(product.base_Product);
                product.OnHandStore1 = product.base_Product.OnHandStore1;
                product.OnHandStore2 = product.base_Product.OnHandStore2;
                product.OnHandStore3 = product.base_Product.OnHandStore3;
                product.OnHandStore4 = product.base_Product.OnHandStore4;
                product.OnHandStore5 = product.base_Product.OnHandStore5;
                product.OnHandStore6 = product.base_Product.OnHandStore6;
                product.OnHandStore7 = product.base_Product.OnHandStore7;
                product.OnHandStore8 = product.base_Product.OnHandStore8;
                product.OnHandStore9 = product.base_Product.OnHandStore9;
                product.OnHandStore10 = product.base_Product.OnHandStore10;
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region FocusDefaultElement

        private void FocusDefaultElement()
        {
            FocusDefault = false;
            FocusDefault = true;
        }

        #endregion

        #region OpenSearchVendor

        /// <summary>
        /// Open search vendor view.
        /// </summary>
        private void OpenSearchVendor()
        {
            VendorSearchViewModel vendorSearchViewModel = new VendorSearchViewModel(_vendorCollection);
            bool? result = _dialogService.ShowDialog<VendorSearchView>(_ownerViewModel, vendorSearchViewModel, "Search Vendor");
            if (result == true)
            {
                _selectedPurchaseOrder.VendorResource = vendorSearchViewModel.SelectedVendor.Resource.ToString();
            }
        }

        #endregion

        #region ShowPurchaseOrderPaymentView

        /// <summary>
        /// Show PurchaseOrderPaymentView.
        /// </summary>
        private void ShowPurchaseOrderPaymentView()
        {
            string purchaseOrderId = _selectedPurchaseOrder.Resource.ToString();
            decimal lastPayment = 0;
            decimal balance = _selectedPurchaseOrder.Balance;
            base_ResourcePaymentModel resourcePayment = _selectedPurchaseOrder.PaymentCollection.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            if (resourcePayment != null)
            {
                lastPayment = resourcePayment.TotalPaid;
                balance = resourcePayment.Balance;
            }
            PurchaseOrderPaymentViewModel purchaseOrderPaymentViewModel = new PurchaseOrderPaymentViewModel(_selectedPurchaseOrder, balance, lastPayment);
            bool? result = _dialogService.ShowDialog<PurchaseOrderPaymentView>(_ownerViewModel, purchaseOrderPaymentViewModel, "Payment");
            if (result == true)
            {
                _selectedPurchaseOrder.PaymentCollection.Add(purchaseOrderPaymentViewModel.PaymentModel);
                _selectedPurchaseOrder.Paid = _selectedPurchaseOrder.PaymentCollection.Sum(x => x.TotalPaid);
                _selectedPurchaseOrder.Balance = _selectedPurchaseOrder.Total - _selectedPurchaseOrder.Paid;
            }
        }

        #endregion

        #region GetProductExecute

        /// <summary>
        /// Gets product.
        /// </summary>
        /// <param name="barcode">Product's barcode.</param>
        private void GetProductExecute()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_productBarcode))
                {
                    base_ProductRepository productRepository = new Repository.base_ProductRepository();
                    base_Product product = productRepository.Get(x => x.Barcode == _productBarcode);
                    if (product != null)
                    {
                        productRepository.Refresh(product);
                        SelectedProduct = new base_ProductModel(product);
                        ProductBarcode = null;
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

        #region ShowPaymentHistoryDetail

        /// <summary>
        /// Show detail of payment history.
        /// </summary>
        private void ShowPaymentHistoryDetail(base_ResourcePaymentModel resourcePaymentModel)
        {
            try
            {
                if (resourcePaymentModel == null)
                {
                    return;
                }

                if (resourcePaymentModel.PaymentDetailCollection == null)
                {
                    base_ResourcePaymentDetailRepository resourcePaymentDetailRepository = new base_ResourcePaymentDetailRepository();
                    resourcePaymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>(resourcePaymentDetailRepository.GetAll(x =>
                        x.ResourcePaymentId == resourcePaymentModel.Id).Select(x => new base_ResourcePaymentDetailModel(x)));
                }

                POSOPaymentHistoryDetailViewModel viewModel = new POSOPaymentHistoryDetailViewModel(resourcePaymentModel);
                bool? dialogResult = _dialogService.ShowDialog<POSOPaymentHistoryDetailView>(_ownerViewModel, viewModel, Language.GetMsg("Title_PaymentHistoryDetail"));
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
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
                if (_productCollectionOutSide == null)
                {
                    _oldPurchaseOrder = param as base_PurchaseOrderModel;
                    if (_oldPurchaseOrder == null)
                    {
                        _vendorResource = (Guid)param;
                    }
                    else
                    {
                        _vendorResource = Guid.Empty;
                        SelectDefaultPurchaseOrder(_oldPurchaseOrder);
                    }
                }
                else
                {
                    // Forces null to create new purchase order with input product collection when WorkerRunWorkerCompleted.
                    _oldPurchaseOrder = null;
                    _vendorResource = Guid.Empty;
                }

                IsSearchMode = false;
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
                            purchaseOrder.StoreName = store.Name;
                        }
                        else
                        {
                            purchaseOrder.ShipAddress = null;
                            purchaseOrder.StoreName = null;
                        }

                        // Update on-hand quantity in stock.
                        base_ProductRepository productRepository = new Repository.base_ProductRepository();
                        base_ProductModel productModel = null;
                        base_Product product;
                        Guid podProductId;
                        foreach (base_PurchaseOrderDetailModel item in purchaseOrder.PurchaseOrderDetailCollection)
                        {
                            podProductId = new Guid(item.ProductResource);
                            product = productRepository.Get(x => x.Resource == podProductId);
                            if (product != null)
                            {
                                productModel = new base_ProductModel(product);
                                item.OnHandQtyOnBaseUnit = GetOnHandQty(purchaseOrder.StoreCode, productModel);
                                item.OnHandQtyOnBaseUnitTemp = item.OnHandQtyOnBaseUnit;
                                // Force calculate QuantityOnOrder of ProductStore.
                                item.IsDirty = true;
                            }
                        }
                    }

                    break;

                case "VendorResource":

                    if (_vendorCollection != null)
                    {
                        // Gets selected vendor.
                        base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Resource.ToString() == purchaseOrder.VendorResource);
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
                        // Determine status.
                        if (purchaseOrder.Status < (short)PurchaseStatus.PaidInFull)
                        {
                            purchaseOrder.Status = (short)PurchaseStatus.PaidInFull;
                        }
                    }

                    break;

                case "CanReceive":
                    OnPropertyChanged(() => AllowPurchaseReceive);
                    OnPropertyChanged(() => AllowPurchaseOrderReturn);
                    break;

                case "SubTotal":
                case "DiscountAmount":
                case "Freight":
                case "TaxAmount":

                    CalculateTotalForPurchaseOrder();

                    break;

                case "Balance":

                    if (_selectedPurchaseOrder.Balance <= 0)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CheckFullWorkflow();
                        }));
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
                            purchaseOrderDetail.BaseUOM = productUOM.Name;
                            purchaseOrderDetail.OnHandQty = Math.Round((decimal)purchaseOrderDetail.OnHandQtyOnBaseUnit / productUOM.BaseUnitNumber, 2);
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

                    // Calculate order quantity of purchase order.
                    CalculateOrderQtyOfPurchaseOrder();

                    break;

                case "Discount":

                    purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * (purchaseOrderDetail.Price - purchaseOrderDetail.Discount);

                    // Update Discount in PurchaseOrderReceive.
                    IEnumerable<base_PurchaseOrderReceiveModel> receiveDiscountItems = purchaseOrderDetail.PurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                        x.PODResource == purchaseOrderDetail.Resource.ToString());
                    foreach (var item in receiveDiscountItems)
                    {
                        item.Discount = purchaseOrderDetail.Discount;
                    }

                    break;

                case "Price":

                    purchaseOrderDetail.Amount = purchaseOrderDetail.Quantity * (purchaseOrderDetail.Price - purchaseOrderDetail.Discount);

                    // Update Price in PurchaseOrderReceive.
                    IEnumerable<base_PurchaseOrderReceiveModel> receivePriceItems = purchaseOrderDetail.PurchaseOrder.PurchaseOrderReceiveCollection.Where(x =>
                        x.PODResource == purchaseOrderDetail.Resource.ToString());
                    foreach (var item in receivePriceItems)
                    {
                        item.Price = purchaseOrderDetail.Price;
                    }

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

                    CalculateSubTotalForPurchaseOrder();

                    break;

                case "IsFullReceived":

                    // Determine status.
                    if (purchaseOrderDetail.PurchaseOrder.Status < (short)PurchaseStatus.PaidInFull)
                    {
                        if (purchaseOrderDetail.PurchaseOrder.PurchaseOrderDetailCollection.Count > 0 &&
                            !purchaseOrderDetail.PurchaseOrder.PurchaseOrderDetailCollection.Any(x => !x.IsFullReceived))
                        {
                            purchaseOrderDetail.PurchaseOrder.Status = (short)PurchaseStatus.FullyReceived;
                        }
                    }

                    CheckFullWorkflow();

                    break;

                case "OnHandQtyOnBaseUnit":

                    if (purchaseOrderDetail.UOMCollection != null)
                    {
                        base_ProductUOMModel productUOM = purchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == purchaseOrderDetail.UOMId);
                        if (productUOM != null)
                        {
                            purchaseOrderDetail.OnHandQty = Math.Round((decimal)purchaseOrderDetail.OnHandQtyOnBaseUnit / productUOM.BaseUnitNumber, 2);
                        }
                    }

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
                        purchaseOrderReceive.ItemDescription = purchaseOrderReceive.PurchaseOrderDetail.ItemDescription;
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
                            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text9, Language.POS, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    purchaseOrderReceive.IsReceived = false;
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                            else
                            {
                                // Determine status.
                                if (purchaseOrderReceive.PurchaseOrder.Status < (short)PurchaseStatus.InProgress)
                                {
                                    purchaseOrderReceive.PurchaseOrder.Status = (short)PurchaseStatus.InProgress;
                                }

                                //AddRemovePayment(purchaseOrderReceive);
                                CalculateSumReceivedQty(purchaseOrderReceive.PODResource);
                                _selectedPurchaseOrder.RaiseCanChangeStorePropertyChanged();
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                purchaseOrderReceive.IsReceived = false;
                                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text10, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                    else
                    {
                        //AddRemovePayment(purchaseOrderReceive);
                        CalculateSumReceivedQty(purchaseOrderReceive.PODResource);
                        _selectedPurchaseOrder.RaiseCanChangeStorePropertyChanged();
                    }

                    // Not allow Change UOM, Price, Discount.
                    if (purchaseOrderReceive.PurchaseOrderDetail != null)
                    {
                        purchaseOrderReceive.PurchaseOrderDetail.RaiseHasReceivedItemChanged();
                        purchaseOrderReceive.PurchaseOrderDetail.RaiseCanChangeUOMChanged();
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
                        resourceReturnDetail.ItemDescription = resourceReturnDetail.PurchaseOrderDetail.ItemDescription;
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
                        resourceReturnDetail.UnitName = null;
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
                    if (resourceReturnDetail.PurchaseOrderDetail != null && resourceReturnDetail.PurchaseOrderDetail.UOMCollection != null)
                    {
                        base_ProductUOMModel productUOM = resourceReturnDetail.PurchaseOrderDetail.UOMCollection.FirstOrDefault(x => x.UOMId == resourceReturnDetail.PurchaseOrderDetail.UOMId);
                        if (productUOM != null)
                        {
                            resourceReturnDetail.ReturnQtyUOM = resourceReturnDetail.ReturnQty * productUOM.BaseUnitNumber;
                        }
                    }

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
                            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text11, Language.POS, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    resourceReturnDetail.IsReturned = false;
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                resourceReturnDetail.IsReturned = false;
                                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text12, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }

                        CalculateOnHandQtyWhenReturn(resourceReturnDetail);
                        CalculateTotalRefundForResourceReturn();
                    }
                    else
                    {
                        CalculateOnHandQtyWhenReturn(resourceReturnDetail, true);
                        CalculateTotalRefundForResourceReturn();
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
                case "ReturnFee":
                case "TotalRefund":

                    _selectedPurchaseOrder.IsDirty = true;
                    _selectedPurchaseOrder.ResourceReturn.Balance = (_selectedPurchaseOrder.ResourceReturn.SubTotal - _selectedPurchaseOrder.ResourceReturn.TotalRefund) +
                        _selectedPurchaseOrder.ResourceReturn.ReturnFee;

                    break;
            }
        }

        #endregion

        #region Auto Searching
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
                SearchPO();
                _waitingTimer.Stop();
            }
        }

        /// <summary>
        /// Reset timer for Auto complete search
        /// </summary>
        protected virtual void ResetTimer()
        {
            if (Define.CONFIGURATION.IsAutoSearch && this._waitingTimer != null)
            {
                this._waitingTimer.Stop();
                this._waitingTimer.Start();
                _timerCounter = 0;
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
                    SelectDefaultPurchaseOrder(_oldPurchaseOrder);
                }
                else
                {
                    CreatePurchaseOrder();
                    if (_vendorResource != Guid.Empty)
                    {
                        _selectedPurchaseOrder.IsDirty = true;
                        _vendorResource = Guid.Empty;
                    }
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

    public partial class PurchaseOrderViewModel : ViewModelBase, IDropTarget
    {
        #region Properties

        /// <summary>
        /// Gets the AllowPurchaseReceive.
        /// </summary>
        public bool AllowPurchaseReceive
        {
            get
            {
                if (SelectedPurchaseOrder == null)
                    return UserPermissions.AllowPurchaseReceive;
                return UserPermissions.AllowPurchaseReceive && SelectedPurchaseOrder.CanReceive;
            }
        }

        /// <summary>
        /// Gets the AllowPurchaseOrderReturn.
        /// </summary>
        public bool AllowPurchaseOrderReturn
        {
            get
            {
                if (SelectedPurchaseOrder == null)
                    return UserPermissions.AllowPurchaseOrderReturn;
                return UserPermissions.AllowPurchaseOrderReturn && SelectedPurchaseOrder.CanReceive;
            }
        }

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if (dropInfo.Data is Guid)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Purchase Order", dropInfo.Data);
            }
            else if (dropInfo.Data is Guid)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Purchase Order", dropInfo.Data);
            }
        }

        #endregion
    }
}