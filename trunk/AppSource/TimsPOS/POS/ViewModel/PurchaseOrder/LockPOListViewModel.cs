﻿using System;
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
    public class LockPOListViewModel : ViewModelBase
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

        #endregion

        #region Contructors

        public LockPOListViewModel(bool isSearchMode, object param)
        {
            IsSearchMode = isSearchMode;
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
                    _currentTabItem = value;
                    OnPropertyChanged(() => CurrentTabItem);
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
                    _closeSearchComponentCommand = new RelayCommand(CloseSearchComponentExecute);
                }
                return _closeSearchComponentCommand;
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

        #region UnLockCommand

        private ICommand _unLockCommand;
        /// <summary>
        /// When 'Lock' button clicked, UnLockCommand will executes. 
        /// </summary>
        public ICommand UnLockCommand
        {
            get
            {
                if (_unLockCommand == null)
                {
                    _unLockCommand = new RelayCommand(UnLockExecute, CanUnLockExecute);
                }
                return _unLockCommand;
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

        #region UnLockExecute

        /// <summary>
        /// UnLock PurchaseOrder.
        /// </summary>
        private void UnLockExecute()
        {
            UnLock();
        }

        #endregion

        #region CanUnLockExecute

        /// <summary>
        /// Determine whether can call UnLockExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanUnLockExecute()
        {
            if (_selectedPurchaseOrder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region Property Changed Methods

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
                            _predicate = _predicate.And(x => !x.IsPurge && x.IsLocked);

                            if (_hasSearchPurchaseOrderNo && !string.IsNullOrWhiteSpace(_keyword))
                            {
                                _predicate = _predicate.And(x => x.PurchaseOrderNo.ToLower().Contains(_keyword.ToLower()));
                            }

                            if (_hasSearchVendor && !string.IsNullOrWhiteSpace(_keyword))
                            {
                                IEnumerable<string> vendorIDList = _vendorCollection.Where(x =>
                                    x.Mark == MarkType.Vendor.ToDescription() &&
                                    x.Company != null &&
                                    x.Company.ToLower().Contains(_keyword.ToLower())).Select(x => x.Resource.ToString());
                                _predicate = _predicate.And(x => vendorIDList.Contains(x.VendorResource));
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
                    base_GuestModel vendor = _vendorCollection.FirstOrDefault(x => x.Resource.ToString() == purchaseOrderModel.VendorResource);
                    if (vendor != null)
                    {
                        // Gets VendorName..
                        purchaseOrderModel.VendorName = vendor.Company;
                    }
                }
                purchaseOrderModel.IsNew = false;
                purchaseOrderModel.IsDirty = false;
                _purchaseOrderCollection.Add(purchaseOrderModel);
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
            purchaseOrder.ResourcePayment = new base_ResourcePaymentModel
            {
                DocumentResource = purchaseOrder.Resource.ToString(),
                DocumentNo = purchaseOrder.PurchaseOrderNo,
                DateCreated = DateTime.Now.Date,
                Resource = Guid.NewGuid(),
                Mark = OrderMarkType.PurchaseOrder.ToDescription()
            };
            purchaseOrder.ResourcePaymentDetail = new base_ResourcePaymentDetailModel();
            purchaseOrder.ResourcePaymentDetail.PaymentType = "P";
            purchaseOrder.ResourcePaymentDetail.ResourcePaymentId = purchaseOrder.ResourcePayment.Id;
            purchaseOrder.ResourcePaymentDetail.PaymentMethodId = Define.CONFIGURATION.DefaultPaymentMethod.HasValue ? Define.CONFIGURATION.DefaultPaymentMethod.Value : _paymentMethodCollection.First().Value;
            purchaseOrder.ResourcePaymentDetail.PaymentMethod = _paymentMethodCollection.FirstOrDefault(x => x.Value == purchaseOrder.ResourcePaymentDetail.PaymentMethodId).Text;
            purchaseOrder.ResourcePaymentDetail.Paid = purchaseOrder.ResourcePayment.TotalPaid;
            purchaseOrder.PurchaseOrderDetailCollection = new CollectionBase<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderDetailReceiveCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderDetailReturnCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            purchaseOrder.PurchaseOrderReceiveCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
            purchaseOrder.PurchaseOrderPaymentCollection = new CollectionBase<base_PurchaseOrderReceiveModel>();
            purchaseOrder.ResourceReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
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
                            Code = UOM.Code,
                            Name = UOM.Name,
                            RegularPrice = product.RegularPrice,
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
                                Code = item.base_UOM.Code,
                                Name = item.base_UOM.Name,
                                IsDirty = false
                            });
                        }
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
                _predicate = POAdvanceSearchViewModel.Predicate.And(x => x.IsLocked);
                _backgroundWorker.RunWorkerAsync("Load");
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
                // Unlock PurchaseOrder.
                _selectedPurchaseOrder.IsLocked = false;
                _selectedPurchaseOrder.DateUpdate = DateTime.Now;
                _selectedPurchaseOrder.ToEntity();
                purchaseOrderRepository.Commit();
                _selectedPurchaseOrder.IsDirty = false;

                IsSearchMode = true;
                (_ownerViewModel as MainViewModel).OpenViewExecute("PurchaseOrder", _selectedPurchaseOrder);
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region OpenSearchComponent

        /// <summary>
        /// Open search component.
        /// </summary>
        private void OpenSearchComponent()
        {
            IsSearchMode = true;
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
                        purchaseOrderReceiveModel.IsDirty = false;
                        _selectedPurchaseOrder.PurchaseOrderReceiveCollection.Add(purchaseOrderReceiveModel);
                    }

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

                    // Gets ResourcePayment.
                    base_ResourcePayment resourcePayment = null;
                    if (resourcePaymentRepository.GetIQueryable(x => x.DocumentResource == purchaseOrderID).Count() > 0)
                    {
                        _selectedPurchaseOrder.HasPayment = resourcePaymentRepository.GetIQueryable(x =>
                            x.DocumentResource == purchaseOrderID && x.TotalPaid > 0).Count() > 0;
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
                        resourceReturnDetailModel.IsDirty = false;
                        _selectedPurchaseOrder.ResourceReturnDetailCollection.Add(resourceReturnDetailModel);
                    }

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
            }

            return true;
        }

        #endregion

        #region ChangeSearchMode

        public override void ChangeSearchMode(bool isList, object param = null)
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
    }
}