using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Toolkit.Base;
using System.Windows.Input;
using CPC.Toolkit.Command;
using System.ComponentModel;
using CPC.POS.Database;
using System.Windows;
using CPC.POS.Repository;
using CPC.Helper;
using CPC.POS.Model;
using System.Linq.Expressions;
using System.Globalization;

namespace CPC.POS.ViewModel
{
    class POReturnSearchViewModel : ViewModelBase
    {
        #region Enum

        private enum CompareType
        {
            GreaterThan = 0,
            LessThan = 1,
            Equal = 2
        }

        #endregion

        #region Fields

        private readonly string _purchaseOrderColumnSort = "It.Id";

        /// <summary>
        /// Gets data on a separate thread.
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        /// <summary>
        /// PurchaseOrder predicate.
        /// </summary>
        Expression<Func<base_PurchaseOrder, bool>> _predicate = PredicateBuilder.True<base_PurchaseOrder>();

        #endregion

        #region Constructors

        public POReturnSearchViewModel()
        {
            Initialize();

            _ownerViewModel = App.Current.MainWindow.DataContext;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunWorkerCompleted);
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
                    OnKeywordChanged();
                }
            }
        }

        #endregion

        #region Barcode

        private string _barcode;
        /// <summary>
        /// Gets or sets Barcode.
        /// </summary>
        public string Barcode
        {
            get
            {
                return _barcode;
            }
            set
            {
                if (_barcode != value)
                {
                    _barcode = value;
                    OnPropertyChanged(() => Barcode);
                }
            }
        }

        #endregion

        #region PurchaseOrderNo

        private string _purchaseOrderNo;
        /// <summary>
        /// Gets or sets PurchaseOrderNo.
        /// </summary>
        public string PurchaseOrderNo
        {
            get
            {
                return _purchaseOrderNo;
            }
            set
            {
                if (_purchaseOrderNo != value)
                {
                    _purchaseOrderNo = value;
                    OnPropertyChanged(() => PurchaseOrderNo);
                }
            }
        }

        #endregion

        #region PurchasedDateFrom

        private DateTime? _purchasedDateFrom;
        /// <summary>
        /// Gets or sets PurchasedDateFrom.
        /// </summary>
        public DateTime? PurchasedDateFrom
        {
            get
            {
                return _purchasedDateFrom;
            }
            set
            {
                if (_purchasedDateFrom != value)
                {
                    _purchasedDateFrom = value;
                    OnPropertyChanged(() => PurchasedDateFrom);
                }
            }
        }

        #endregion

        #region PurchasedDateTo

        private DateTime? _purchasedDateTo;
        /// <summary>
        /// Gets or sets PurchasedDateTo.
        /// </summary>
        public DateTime? PurchasedDateTo
        {
            get
            {
                return _purchasedDateTo;
            }
            set
            {
                if (_purchasedDateTo != value)
                {
                    _purchasedDateTo = value;
                    OnPropertyChanged(() => PurchasedDateTo);
                }
            }
        }

        #endregion

        #region VendorName

        private string _vendorName;
        /// <summary>
        /// Gets or sets VendorName.
        /// </summary>
        public string VendorName
        {
            get
            {
                return _vendorName;
            }
            set
            {
                if (_vendorName != value)
                {
                    _vendorName = value;
                    OnPropertyChanged(() => VendorName);
                }
            }
        }

        #endregion

        #region TotalCompareType

        private int _totalCompareType;
        /// <summary>
        /// Gets or sets TotalCompareType.
        /// </summary>
        public int TotalCompareType
        {
            get
            {
                return _totalCompareType;
            }
            set
            {
                if (_totalCompareType != value)
                {
                    _totalCompareType = value;
                    OnPropertyChanged(() => TotalCompareType);
                }
            }
        }

        #endregion

        #region Total

        private decimal _total;
        /// <summary>
        /// Gets or sets Total.
        /// </summary>
        public decimal Total
        {
            get
            {
                return _total;
            }
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

        #region HasUsedAdvanceSearch

        private bool _hasUsedAdvanceSearch;
        public bool HasUsedAdvanceSearch
        {
            get
            {
                return _hasUsedAdvanceSearch;
            }
            set
            {
                if (_hasUsedAdvanceSearch != value)
                {
                    _hasUsedAdvanceSearch = value;
                    OnPropertyChanged(() => HasUsedAdvanceSearch);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

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

        #region OpenViewCommand

        private ICommand _openViewCommand;
        /// <summary>
        /// When 'New PO', 'Go To PO List' button clicked, command will executes.
        /// </summary>
        public ICommand OpenViewCommand
        {
            get
            {
                if (_openViewCommand == null)
                {
                    _openViewCommand = new RelayCommand<string>(OpenViewExecute, CanOpenViewExecute);
                }
                return _openViewCommand;
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

        #region SearchPOExecute

        /// <summary>
        /// Search PurchaseOrder when 'enter' key pressed.
        /// </summary>
        private void SearchPOExecute()
        {
            Load();
        }

        #endregion

        #region GetPurchaseOrdersExecute

        /// <summary>
        /// Gets range of purchase orders.
        /// </summary>
        private void GetPurchaseOrdersExecute()
        {
            LoadNext();
        }

        #endregion

        #region OpenViewExecute

        private void OpenViewExecute(string parameter)
        {
            OpenView(parameter);
        }

        #endregion

        #region CanOpenViewExecute

        private bool CanOpenViewExecute(string parameter)
        {
            if (parameter == "Selected")
            {
                return _selectedPurchaseOrder != null;
            }

            return true;
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Close popup.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnKeywordChanged

        /// <summary>
        /// Occurs when Keyword property changed.
        /// </summary>
        private void OnKeywordChanged()
        {
            HasUsedAdvanceSearch = false;
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
            GetVendors();
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Load

        /// <summary>
        /// Search PurchaseOrder.
        /// </summary>
        private void Load()
        {
            _backgroundWorker.RunWorkerAsync("Load");
        }

        #endregion

        #region LoadNext

        private void LoadNext()
        {
            if (!IsBusy)
            {
                _backgroundWorker.RunWorkerAsync("LoadNext");
            }
        }

        #endregion

        #region OpenView

        /// <summary>
        /// Open view.
        /// </summary>
        private void OpenView(string parameter)
        {
            if (parameter == "PO")
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("PurchaseOrder", new base_PurchaseOrderModel());
                Close(false);
            }
            else if (parameter == "POList")
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("PurchaseOrderList");
                Close(false);
            }
            else
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("PurchaseOrder", _selectedPurchaseOrder);
                Close(true);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Close popup.
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
                    base_ProductRepository productRepository = new base_ProductRepository();
                    base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                    if (argument != "LoadNext")
                    {
                        _predicate = PredicateBuilder.True<base_PurchaseOrder>();

                        // No used advance search.
                        if (!_hasUsedAdvanceSearch)
                        {
                            #region Basic search

                            Expression<Func<base_PurchaseOrder, bool>> predicateChild = PredicateBuilder.True<base_PurchaseOrder>();

                            if (!string.IsNullOrWhiteSpace(_keyword))
                            {
                                predicateChild = PredicateBuilder.False<base_PurchaseOrder>();

                                // PurchaseOrderNo.
                                predicateChild = predicateChild.Or(x => x.PurchaseOrderNo.ToLower().Contains(_keyword.ToLower()));

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
                                if (decimal.TryParse(_keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalKey))
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

                            _predicate = _predicate.And(x => !x.IsPurge && !x.IsLocked && x.QtyReceived > 0).And(predicateChild);

                            #endregion
                        }
                        else
                        {
                            #region Advance search

                            _predicate = _predicate.And(x => !x.IsPurge && !x.IsLocked && x.QtyReceived > 0);

                            // POCard.
                            if (!string.IsNullOrWhiteSpace(_barcode))
                            {
                                _predicate = _predicate.And(x => x.POCard != null && x.POCard.ToLower().Contains(_barcode.ToLower()));
                            }

                            // PurchaseOrderNo.
                            if (!string.IsNullOrWhiteSpace(_purchaseOrderNo))
                            {
                                _predicate = _predicate.And(x => x.PurchaseOrderNo != null && x.PurchaseOrderNo.ToLower().Contains(_purchaseOrderNo.ToLower()));
                            }

                            // PurchasedDate.
                            if (_purchasedDateFrom.HasValue && _purchasedDateTo.HasValue)
                            {
                                DateTime from = _purchasedDateFrom.Value.Date;
                                DateTime to = new DateTime(_purchasedDateTo.Value.Year, _purchasedDateTo.Value.Month, _purchasedDateTo.Value.Day, 23, 59, 59);
                                _predicate = _predicate.And(x => x.PurchasedDate >= from && x.PurchasedDate <= to);
                            }
                            else if (_purchasedDateFrom.HasValue)
                            {
                                DateTime from = _purchasedDateFrom.Value.Date;
                                _predicate = _predicate.And(x => x.PurchasedDate >= from);
                            }
                            else if (_purchasedDateTo.HasValue)
                            {
                                DateTime to = new DateTime(_purchasedDateTo.Value.Year, _purchasedDateTo.Value.Month, _purchasedDateTo.Value.Day, 23, 59, 59);
                                _predicate = _predicate.And(x => x.PurchasedDate <= to);
                            }

                            // Vendor company name.
                            if (!string.IsNullOrWhiteSpace(_vendorName))
                            {
                                IEnumerable<string> vendorIDList = _vendorCollection.Where(x =>
                                          x.Mark == MarkType.Vendor.ToDescription() &&
                                          x.Company != null &&
                                          x.Company.ToLower().Contains(_vendorName.ToLower())).Select(x => x.Resource.ToString());
                                _predicate = _predicate.And(x => vendorIDList.Contains(x.VendorResource));
                            }

                            // Total.
                            if (Total != 0)
                            {
                                if (_totalCompareType == (int)CompareType.GreaterThan)
                                {
                                    _predicate = _predicate.And(x => x.Total > _total);
                                }
                                else if (_totalCompareType == (int)CompareType.LessThan)
                                {
                                    _predicate = _predicate.And(x => x.Total < _total);
                                }
                                else if (_totalCompareType == (int)CompareType.Equal)
                                {
                                    _predicate = _predicate.And(x => x.Total == _total);
                                }
                            }

                            #endregion
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

                purchaseOrderModel.HasWantReturn = true;
                purchaseOrderModel.IsNew = false;
                purchaseOrderModel.IsDirty = false;
                _purchaseOrderCollection.Add(purchaseOrderModel);
            }
        }

        #endregion

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
