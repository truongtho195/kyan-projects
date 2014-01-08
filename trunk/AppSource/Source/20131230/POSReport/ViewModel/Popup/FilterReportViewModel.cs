using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CPC.POSReport.Model;
using Toolkit.Base;
using CPC.POSReport.Repository;
using System.Windows.Input;
using Toolkit.Command;
using CPC.POSReport.View;
using CPC.POSReport.Function;
using Xceed.Wpf.Toolkit;

namespace CPC.POSReport.ViewModel
{
    class FilterReportViewModel : ModelBase
    {
        #region -Properties-

        #region -Category Id-
        /// <summary>
        /// Get or set Category Id
        /// </summary>
        private int _categoryId;
        public int CategoryId
        {
            get { return _categoryId; }
            set { _categoryId = value;}
        }
        #endregion

        #region -Product Resource-
        /// <summary>
        /// Get or set Category Id
        /// </summary>
        private string _productResource;
        public string ProductResource
        {
            get { return _productResource; }
            set { _productResource = value; }
        }
        #endregion        

        #region -Store Model -
        /// <summary>
        /// Get or set Store model 
        /// </summary>
        private base_StoreModel _storeModel;
        public base_StoreModel StoreModel
        {
            get { return _storeModel; }
            set
            { _storeModel = value; }
        }

        /// <summary>
        /// Get or set Store model collection
        /// </summary>
        private ObservableCollection<base_StoreModel> _storeModelCollection;
        public ObservableCollection<base_StoreModel> StoreModelCollection
        {
            get { return _storeModelCollection; }
            set
            {
                if (_storeModelCollection != value)
                {
                    _storeModelCollection = value;
                    OnPropertyChanged(() => StoreModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set To Store model 
        /// </summary>
        private base_StoreModel _toStoreModel;
        public base_StoreModel ToStoreModel
        {
            get { return _toStoreModel; }
            set
            {
                if (_toStoreModel != value)
                {
                    _toStoreModel = value;
                    OnPropertyChanged(() => ToStoreModel);
                }
            }
        }

        /// <summary>
        /// Get or set To Store model collection
        /// </summary>
        private ObservableCollection<base_StoreModel> _toStoreModelCollection;
        public ObservableCollection<base_StoreModel> ToStoreModelCollection
        {
            get { return _toStoreModelCollection; }
            set
            {
                if (_toStoreModelCollection != value)
                {
                    _toStoreModelCollection = value;
                    OnPropertyChanged(() => ToStoreModelCollection);
                }
            }
        }
        #endregion
              
        #region -Depart model-
        /// <summary>
        /// Get or set Depart model
        /// </summary>
        private base_DepartmentModel _departModel;
        public base_DepartmentModel DepartModel
        {
            get { return _departModel; }
            set { _departModel = value; }
        }

        #region -Depart model collection-
        /// <summary>
        /// Get or set Depart model collection
        /// </summary>
        private ObservableCollection<base_DepartmentModel> _departModelCollection;
        public ObservableCollection<base_DepartmentModel> DepartModelCollection
        {
            get { return _departModelCollection; }
            set
            {
                if (_departModelCollection != value)
                {
                    _departModelCollection = value;
                    OnPropertyChanged(() => DepartModelCollection);
                }
            }
        }
        #endregion
        #endregion        

        #region -Category model-
        /// <summary>
        /// Get or set Category model
        /// </summary>
        private base_DepartmentModel _categoryModel;
        public base_DepartmentModel CategoryModel
        {
            get { return _categoryModel; }
            set
            {
                if (_categoryModel != value)
                {
                    _categoryModel = value;
                    OnPropertyChanged(() => CategoryModel);
                }
            }
        }

        #region -Category model collection-
        /// <summary>
        /// Get or set category model collection
        /// </summary>
        private ObservableCollection<base_DepartmentModel> _categoryModelCollection;
        public ObservableCollection<base_DepartmentModel> CategoryModelCollection
        {
            get { return _categoryModelCollection; }
            set
            {
                if (_categoryModelCollection != value)
                {
                    _categoryModelCollection = value;
                    OnPropertyChanged(() => CategoryModelCollection);
                }
            }
        }
        #endregion

        #endregion
       
        #region -Product model-
        /// <summary>
        /// Get or set Product model collection
        /// </summary>
        private base_ProductModel _productModel;
        public base_ProductModel ProductModel
        {
            get { return _productModel; }
            set
            {
                if (_productModel != value)
                {
                    _productModel = value;
                    OnPropertyChanged(() => ProductModel);
                }
            }
        }

        #region -Product model collection-
        /// <summary>
        /// Get or set Product model collection
        /// </summary>
        private ObservableCollection<base_ProductModel> _productModelCollection;
        public ObservableCollection<base_ProductModel> ProductModelCollection
        {
            get { return _productModelCollection; }
            set
            {
                if (_productModelCollection != value)
                {
                    _productModelCollection = value;
                    OnPropertyChanged(() => ProductModelCollection);
                }
            }
        }
        #endregion
        #endregion

        #region -Customer Model -
        /// <summary>
        /// Get or set Customer model 
        /// </summary>
        private base_GuestModel _customerModel;
        public base_GuestModel CustomerModel
        {
            get { return _customerModel; }
            set { _customerModel = value; }
        }

        /// <summary>
        /// Get or set Customer Model Collection
        /// </summary>
        private ObservableCollection<base_GuestModel> _customerModelCollection;
        public ObservableCollection<base_GuestModel> CustomerCollection
        {
            get { return _customerModelCollection; }
            set
            {
                if (_customerModelCollection != value)
                {
                    _customerModelCollection = value;
                    OnPropertyChanged(() => CustomerCollection);
                }
            }
        }
        #endregion

        #region -Is Show Control- 

        #region -Is show PO Status-
        /// <summary>
        /// Get or set Is show PO Status
        /// </summary>
        private string _isShowPOStatus = "Collapsed";
        public string IsShowPOStatus
        {
            get { return _isShowPOStatus; }
            set
            {
                if (_isShowPOStatus != value)
                {
                    _isShowPOStatus = value;
                    OnPropertyChanged(() => IsShowPOStatus);
                }
            }
        }
        #endregion

        #region -Is show country-
        // <summary>
        /// Get or set Is show Store
        /// </summary>
        private string _isShowCountry = "Collapsed";
        public string IsShowCountry
        {
            get { return _isShowCountry; }
            set
            {
                if (_isShowCountry != value)
                {
                    _isShowCountry = value;
                    OnPropertyChanged(() => IsShowCountry);
                }
            }
        }
        #endregion

        #region -Is show Store-
        /// <summary>
        /// Get or set Is show Store
        /// </summary>
        private string _isShowStore = "Visible";
        public string IsShowStore
        {
            get { return _isShowStore; }
            set
            {
                if (_isShowStore != value)
                {
                    _isShowStore = value;
                    OnPropertyChanged(() => IsShowStore);
                }
            }
        }

        /// <summary>
        /// Get or set Is show Depart
        /// </summary>
        private string _isShowToStore = "Collapsed";
        public string IsShowToStore
        {
            get { return _isShowToStore; }
            set
            {
                if (_isShowToStore != value)
                {
                    _isShowToStore = value;
                    OnPropertyChanged(() => IsShowToStore);
                }
            }
        }
        #endregion

        #region -Is show Depart-
        /// <summary>
        /// Get or set Is show Depart 
        /// </summary>
        private string _isShowDepart00 = "collapsed";
        public string IsShowDepart00
        {
            get { return _isShowDepart00; }
            set
            {
                if (_isShowDepart00 != value)
                {
                    _isShowDepart00 = value;
                    OnPropertyChanged(() => IsShowDepart00);
                }
            }
        }
        #endregion

        #region -Is show category-
        /// <summary>
        /// Get or set Is show category
        /// </summary>
        private string _isShowCategory10 = "collapsed";
        public string IsShowCategory10
        {
            get { return _isShowCategory10; }
            set
            {
                if (_isShowCategory10 != value)
                {
                    _isShowCategory10 = value;
                    OnPropertyChanged(() => IsShowCategory10);
                }
            }
        }

        /// <summary>
        /// Get or set Is show category
        /// </summary>
        private string _isShowCategory01 = "collapsed";
        public string IsShowCategory01
        {
            get { return _isShowCategory01; }
            set
            {
                if (_isShowCategory01 != value)
                {
                    _isShowCategory01 = value;
                    OnPropertyChanged(()=>IsShowCategory01);
                }
            }
        }
        #endregion

        #region -Is show product-
        /// <summary>
        /// Get or set Is show product
        /// </summary>
        private string _isShowProduct11 = "collapsed";
        public string IsShowProduct11
        {
            get { return _isShowProduct11; }
            set
            {
                if (_isShowProduct11 != value)
                {
                    _isShowProduct11 = value;
                    OnPropertyChanged(()=>IsShowProduct11);
                }
            }
        }

        /// <summary>
        /// Get or set Is show product
        /// </summary>
        private string _isShowProduct01 = "collapsed";
        public string IsShowProduct01
        {
            get { return _isShowProduct01; }
            set
            {
                if (_isShowProduct01 != value)
                {
                    _isShowProduct01 = value;
                    OnPropertyChanged(()=>IsShowProduct01);
                }
            }
        }
        #endregion

        #region -Is show Order date-
        /// <summary>
        /// Get or set Is show product
        /// </summary>
        private string _isShowOrderDate = "collapsed"; 
        public string IsShowOrderDate
        {
            get { return _isShowOrderDate; }
            set
            {
                if (_isShowOrderDate != value)
                {
                    _isShowOrderDate = value;
                    OnPropertyChanged(()=>IsShowOrderDate);
                }
            }
        }
        #endregion

        #region -Is show Ship date-
        /// <summary>
        /// Get or set Is show product
        /// </summary>
        private string _isShowShipDate = "collapsed";
        public string IsShowShipDate
        {
            get { return _isShowShipDate; }
            set
            {
                if (_isShowShipDate != value)
                {
                    _isShowShipDate = value;
                    OnPropertyChanged(()=>IsShowShipDate);
                }
            }
        }
        #endregion

        #region -Is show adjustment-
        /// <summary>
        /// Get or set Is show adjustment
        /// </summary>
        private string _isShowAdjustment = "collapsed";
        public string IsShowAdjustment
        {
            get { return _isShowAdjustment; }
            set
            {
                if (_isShowAdjustment != value)
                {
                    _isShowAdjustment = value;
                    OnPropertyChanged(()=>IsShowAdjustment);
                }
            }
        }
        #endregion

        #region -Is show SOStatus-
        /// <summary>
        /// Get or set Is show SOStatus33
        /// </summary>
        private string _isShowSOStatus33 = "collapsed";
        public string IsShowSOStatus33
        {
            get { return _isShowSOStatus33; }
            set
            {
                if (_isShowSOStatus33 != value)
                {
                    _isShowSOStatus33 = value;
                    OnPropertyChanged(()=>IsShowSOStatus33);
                }
            }
        }
        /// <summary>
        /// Get or set Is show SOStatus 11
        /// </summary>
        private string _isShowSOStatus11 = "collapsed";
        public string IsShowSOStatus11
        {
            get { return _isShowSOStatus11; }
            set
            {
                if (_isShowSOStatus11 != value)
                {
                    _isShowSOStatus11 = value;
                    OnPropertyChanged(()=>IsShowSOStatus11);
                }
            }
        }
        #endregion

        #region -Is show Customer-
        /// <summary>
        /// Get or set Is show Customer
        /// </summary>
        private string _sShowCustomer33 = "collapsed";
        public string IsShowCustomer33
        {
            get { return _sShowCustomer33; }
            set
            {
                if (_sShowCustomer33 != value)
                {
                    _sShowCustomer33 = value;
                    OnPropertyChanged(()=>IsShowCustomer33);
                }
            }
        }
        
        /// <summary>
        /// Get or set Is show Customer
        /// </summary>
        private string _sShowCustomer11 = "collapsed";
        public string IsShowCustomer11
        {
            get { return _sShowCustomer11; }
            set
            {
                if (_sShowCustomer11 != value)
                {
                    _sShowCustomer11 = value;
                    OnPropertyChanged(()=>IsShowCustomer11);
                }
            }
        }
        #endregion

        #region -Is show Transfer Stock Status-
        /// <summary>
        /// Get or set Is show Transfer Stock Status
        /// </summary>
        private string _isShowTransferStockStatus = "collapsed";
        public string IsShowTransferStockStatus
        {
            get { return _isShowTransferStockStatus; }
            set
            {
                if (_isShowTransferStockStatus != value)
                {
                    _isShowTransferStockStatus = value;
                    OnPropertyChanged(()=>IsShowTransferStockStatus);
                }
            }
        }
        #endregion

        #endregion 

        #region -Label From & To Date-
        /// <summary>
        /// Get or set Label FromDate
        /// </summary>
        private string _lblFromDate  = "Order From";
        public string LblFromDate
        {
            get { return _lblFromDate; }
            set
            {
                if (_lblFromDate != value)
                {
                    _lblFromDate = value;
                    OnPropertyChanged(()=>LblFromDate);
                }
            }
        }

        /// <summary>
        /// Get or set Label ToDate
        /// </summary>
        private string _lblToDate = "Order To";
        public string LblToDate
        {
            get { return _lblToDate; }
            set
            {
                if (_lblToDate != value)
                {
                    _lblToDate = value;
                    OnPropertyChanged(()=>LblToDate);
                }
            }
        }
        #endregion

        #region -Label Ship From Date-
        /// <summary>
        /// Get or set Label Ship FromDate
        /// </summary>
        private string _lblShipFromDate = "Ship From";
        public string LblShipFromDate
        {
            get { return _lblShipFromDate; }
            set
            {
                if (_lblShipFromDate != value)
                {
                    _lblShipFromDate = value;
                    OnPropertyChanged(()=>LblShipFromDate);
                }
            }
        }

        /// <summary>
        /// Get or set Label Ship ToDate
        /// </summary>
        private string _lblShipToDate = "Ship To";
        public string LblShipToDate
        {
            get { return _lblShipToDate; }
            set
            {
                if (_lblShipToDate != value)
                {
                    _lblShipToDate = value;
                    OnPropertyChanged(()=>LblShipToDate);
                }
            }
        }
        #endregion

        #region -Adjustment Reason-

        private ComboItem _adjustmentReason;
        /// <summary>
        /// Gets or sets the AdjustmentReason
        /// </summary>
        public ComboItem AdjustmentReason
        {
            get { return _adjustmentReason; }
            set { _adjustmentReason = value; }
        }

        private IList<ComboItem> _lstAdjustmentReason;
        /// <summary>
        /// Gets or sets the AdjustmentReason
        /// </summary>
        public IList<ComboItem> LstAdjustmentReason
        {
            set { _lstAdjustmentReason = value; }
            get
            {
                if (null == _lstAdjustmentReason)
                {
                    _lstAdjustmentReason = XMLHelper.GetElements("AdjustmentReason");
                    ComboItem item = new ComboItem(-1, "-All Reason-");                                      
                    _lstAdjustmentReason.Insert(0, item);
                    AdjustmentReason = _lstAdjustmentReason[0];
                }
                return _lstAdjustmentReason;
            }            
        }
        #endregion

        #region -Adjustment Status-

        private ComboItem _adjustmentStatus;
        /// <summary>
        /// Gets or sets the AdjustmentStatus
        /// </summary>
        public ComboItem AdjustmentStatus
        {
            get { return _adjustmentStatus; }
            set { _adjustmentStatus = value; }
        }

        private IList<ComboItem> _lstAdjustmentStatus;
        /// <summary>
        /// Gets or sets the AdjustmentStatus
        /// </summary>
        public IList<ComboItem> LstAdjustmentStatus
        {
            set { _lstAdjustmentStatus = value; }
            get
            {
                if (null == _lstAdjustmentStatus)
                {
                    _lstAdjustmentStatus = XMLHelper.GetElements("AdjustmentStatus");
                    ComboItem item = new ComboItem(-1, "-All Status-");                             
                    _lstAdjustmentStatus.Insert(0, item);   
                    AdjustmentStatus = _lstAdjustmentStatus[0];
                }
                return _lstAdjustmentStatus;
            }            
        }
        #endregion

        #region -Transfer Stock Status-

        private ComboItem _transferStockStatus;
        /// <summary>
        /// Gets or sets the TransferStockStatus
        /// </summary>
        public ComboItem TransferStockStatus
        {
            get { return _transferStockStatus; }
            set { _transferStockStatus = value; }
        }

        private IList<ComboItem> _lstTransferStockStatus;
        /// <summary>
        /// Gets or sets the AdjustmentStatus
        /// </summary>
        public IList<ComboItem> LstTransferStockStatus
        {
            set { _lstTransferStockStatus = value; }
            get
            {
                if (null == _lstTransferStockStatus)
                {
                    _lstTransferStockStatus = XMLHelper.GetElements("TransferStockStatus");
                    ComboItem item = new ComboItem(-1,"-All Status-");
                    _lstTransferStockStatus.Insert(0, item);
                    TransferStockStatus = _lstTransferStockStatus[0];
                }
                return _lstTransferStockStatus;
            }            
        }
        #endregion

        #region -Country-

        private ComboItem _countryModel;
        /// <summary>
        /// Gets or sets the Country
        /// </summary>
        public ComboItem CountryModel
        {
            get { return _countryModel; }
            set { _countryModel = value; }
        }

        private IList<ComboItem> _lstCountry;
        /// <summary>
        /// Gets or sets the Country Collection
        /// </summary>
        public IList<ComboItem> LstCountry
        {
            set { _lstCountry = value; }
            get
            {
                if (null == _lstCountry)
                {
                    _lstCountry = XMLHelper.GetElements("country");
                    ComboItem item = new ComboItem(-1, "-All Country-");
                    _lstCountry.Insert(0, item);
                    CountryModel = _lstCountry[0];
                }
                return _lstCountry;
            }
        }
        #endregion 

        #region -Purchase Order Status-

        private ComboItem _pOStatus;
        /// <summary>
        /// Gets or sets the POStatus
        /// </summary>
        public ComboItem POStatus
        {
            get { return _pOStatus; }
            set { _pOStatus = value;}
        }

        private IList<ComboItem> _lstPOStatus;
        /// <summary>
        /// Gets or sets LstPOStatus
        /// </summary>
        public IList<ComboItem> LstPOStatus
        {
            set { _lstPOStatus = value; }
            get
            {
                if (null == _lstPOStatus)
                {
                    _lstPOStatus = XMLHelper.GetElements("PurchaseStatus");
                    ComboItem item = new ComboItem(-1,"-All Status-");   
                    _lstPOStatus.Insert(0, item);
                    POStatus = _lstPOStatus[0];
                }
                return _lstPOStatus;
            }            
        }
        #endregion

        #region -Sale Order Status-

        private ComboItem _sOStatus;
        /// <summary>
        /// Gets or sets the AdjustmentStatus
        /// </summary>
        public ComboItem SOStatus
        {
            get { return _sOStatus; }
            set { _sOStatus = value; }
        }

        private IList<ComboItem> _lstSOStatus;
        /// <summary>
        /// Gets or sets the AdjustmentStatus
        /// </summary>
        public IList<ComboItem> LstSOStatus
        {
            set { _lstSOStatus = value; }
            get
            {
                if (null == _lstSOStatus)
                {
                    _lstSOStatus = XMLHelper.GetElements("SaleStatus");
                    ComboItem item = new ComboItem(-1, "-All Status-");
                    _lstSOStatus.Insert(0, item);
                    SOStatus = _lstSOStatus[0];
                }
                return _lstSOStatus;
            }            
        }
        #endregion
        
        #region -Order Date-
        /// <summary>
        /// Get or set FromDate
        /// </summary>
        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged(()=>FromDate);
                }
            }
        }

        /// <summary>
        /// Get or set ToDate
        /// </summary>
        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;
                    OnPropertyChanged(()=>ToDate);
                }
            }
        }
        #endregion

        #region -Ship date-
        /// <summary>
        /// Get or set ShipFrom
        /// </summary>
        private DateTime? _shipFrom;
        public DateTime? ShipFrom
        {
            get { return _shipFrom; }
            set
            {
                if (_shipFrom != value)
                {
                    _shipFrom = value;
                    OnPropertyChanged(()=>ShipFrom);
                }
            }
        }
        
        /// <summary>
        /// Get or set ShipTo
        /// </summary>
        private DateTime? _shipTo;
        public DateTime? ShipTo
        {
            get { return _shipTo; }
            set
            {
                if (_shipTo != value)
                {
                    _shipTo = value;
                    OnPropertyChanged(()=>ShipTo);
                }
            }
        }
        #endregion

        #endregion -Property- 

        #region -Defines-
        private ObservableCollection<base_ProductModel> productCollection;
        private ObservableCollection<base_DepartmentModel> categoryCollection;
        base_DepartmentRepository departRepo = new base_DepartmentRepository();
        base_ProductRepository productRepo = new base_ProductRepository();
        base_GuestRepository guestRepo = new base_GuestRepository();
        base_StoreRepository storeRepo = new base_StoreRepository();

        public MainViewModel MainViewModel { get; set; }
        public Optional OptionalWindow { get; set; }
        public ReportOptional reportOptionalWindow { get; set; }
        public PurchaseOptional purchaseOptionalWindow { get; set; }
        public CustomerPaymentOptional customerPaymentOptionalWindow { get; set; } 
        short window = -1;
        #endregion -Defines- 

        #region -Contructor-
        public FilterReportViewModel()
        { }

        public FilterReportViewModel(object reportPopupWindow, MainViewModel mainViewModel, string reportName, short currentWindow)
        {
            InitCommand();
            switch (currentWindow)
            {
                case (int)Common.FilterWindow.CustomerPaymentOptional:
                    customerPaymentOptionalWindow = (CustomerPaymentOptional)reportPopupWindow;
                    break;
                case (int)Common.FilterWindow.PurchaseOPtional:
                    purchaseOptionalWindow = (PurchaseOptional)reportPopupWindow;
                    break;
                case (int)Common.FilterWindow.ReportOptional:
                    reportOptionalWindow = (ReportOptional)reportPopupWindow;
                    break;
                case (int)Common.FilterWindow.Optional:
                    OptionalWindow = (Optional)reportPopupWindow;
                    break;
            }
            MainViewModel = mainViewModel;
            LoadData(reportName);
            window = currentWindow;
        }
        #endregion -Contructor- 

        #region -Command-

        #region -init command- 
        /// <summary>
        /// Init all command 
        /// </summary>
        private void InitCommand()
        {
            OKCommand = new RelayCommand(OKExecute);
            CancelCommand = new RelayCommand(CancelExecute);
            DepartSelectedItemChangedCommand = new RelayCommand(DepartSelectedItemChangedExecute, CanDepartSelectedItemChangedExecute);
            CategorySelectedItemChangedCommand = new RelayCommand(CategorySelectedItemChangedExecute, CanCategorySelectedItemChangedExecute);
            StoreSelectionChangedCommand = new RelayCommand(StoreSelectionChangedExecute, CanStoreSelectionChangedExecute);
        }
        #endregion

        #region -Ok Command-
        /// <summary>
        /// Set or get ok command
        /// </summary>
        public ICommand OKCommand { get; private set; }

        private void OKExecute()
        {
            try
            {
                // Get store code 
                if (StoreModel != null)
                {
                    int totalStore = StoreModelCollection.Count;
                    for (short i = 0; i < totalStore; i++)
                    {
                        if (StoreModelCollection[i].Id == StoreModel.Id)
                        {
                            MainViewModel.currentStoreCode = --i;
                            break;
                        }
                    }
                }
                // Get department      
                MainViewModel.departId = (DepartModel != null) ? DepartModel.Id : -1;
                // Get category          
                MainViewModel.categoryId = (CategoryModel != null) ? CategoryModel.Id : -1;
                // Get Product resource
                MainViewModel.productResource = (ProductModel != null && ProductModel.Id != -1) ? "'" + ProductModel.Resource.ToString() + "'" : "''";
                // Adjustment Status (Quantity Adjustment & Cost Adustment) 
                MainViewModel.adjustmentStatus = (AdjustmentStatus != null) ? AdjustmentStatus.Value : -1;
                // Adjustment Reason (Quantity Adjustment & Cost Adustment) 
                MainViewModel.adjustmentReason = (AdjustmentReason != null) ? AdjustmentReason.Value : -1;
                // POStatus 
                MainViewModel.purchaseOrderStatus = (POStatus != null) ? POStatus.Value : -1;
                // CountryModel 
                MainViewModel.countryValue = (CountryModel != null) ? CountryModel.Value : -1;
                // Set from date
                MainViewModel.fromDate = (FromDate != null) ? "'" + Common.ToShortDateString(FromDate.Value) + "'": "''";
                // Set to date
                MainViewModel.toDate = (ToDate != null) ? "'" + Common.ToShortDateString(ToDate.Value) + "'" : "''";
                // Set ship from date
                MainViewModel.shipFrom = (ShipFrom != null) ? "'" + Common.ToShortDateString(ShipFrom.Value) + "'" : "''";
                // Set ship to date
                MainViewModel.shipTo = (ShipTo != null) ? "'" + Common.ToShortDateString(ShipTo.Value) + "'" : "''";
                // SO status
                MainViewModel.saleOrderStatus = (SOStatus != null) ? SOStatus.Value : -1;
                // Get full name
                MainViewModel.customerResource = (CustomerModel != null && CustomerModel.Id != -1) ? "'" + CustomerModel.Resource.ToString() + "'": "''";
                // Get TransferStock Status
                MainViewModel.transferStockStatus = (TransferStockStatus != null) ? TransferStockStatus.Value : -1;
                // Get To Store (TranferStock History report)
                if (ToStoreModel != null)
                {
                    int totalStore = StoreModelCollection.Count;
                    for (short i = 0; i < totalStore; i++)
                    {
                        if (StoreModelCollection[i].Id == ToStoreModel.Id)
                        {
                            MainViewModel.transferTo = --i;
                            break;
                        }
                    }
                }
                // Close Popup window
                switch (window)
                {
                    case (short)Common.FilterWindow.Optional:
                        OptionalWindow.Close();
                        break;
                    case (short)Common.FilterWindow.ReportOptional:
                        reportOptionalWindow.Close();
                        break;
                    case (short)Common.FilterWindow.PurchaseOPtional:
                        purchaseOptionalWindow.Close();
                        break;
                    case (short)Common.FilterWindow.CustomerPaymentOptional:
                        customerPaymentOptionalWindow.Close();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        #region -Cancel Command-
        public ICommand CancelCommand { get; private set; }

        private void CancelExecute()
        {
            MainViewModel.currentStoreCode = -2;
            // Close Popup window
            switch (window)
            {
                case (short)Common.FilterWindow.Optional:
                    OptionalWindow.Close();
                    break;
                case (short)Common.FilterWindow.ReportOptional:
                    reportOptionalWindow.Close();
                    break;
                case (short)Common.FilterWindow.PurchaseOPtional:
                    purchaseOptionalWindow.Close();
                    break;
                case (short)Common.FilterWindow.CustomerPaymentOptional:
                    customerPaymentOptionalWindow.Close();
                    break;
            }
        }
        #endregion 

        #region -Department SelectedItem Changed Command-
        /// <summary>
        /// Set or get Department Selected item changed command
        /// </summary>
        public ICommand DepartSelectedItemChangedCommand { get; private set; }

        private void DepartSelectedItemChangedExecute()
        {
            try
            {
                base_DepartmentModel category = CreateDepart(true);
                // Filter by category
                if (DepartModel.Id != -1)
                {
                    CategoryModelCollection = new ObservableCollection<base_DepartmentModel>(
                                                    categoryCollection.Where(w => w.ParentId == DepartModel.Id));
                    CategoryModelCollection.Insert(0, category);
                }
                else
                {
                    CategoryModelCollection = new ObservableCollection<base_DepartmentModel>(categoryCollection);
                }
                CategoryModel = category;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanDepartSelectedItemChangedExecute()
        {
            return (CategoryModel != null && CategoryModelCollection != null && CategoryModelCollection.Count > 0);
        }
        #endregion 

        #region -Category SelectedItem Changed Command-
        /// <summary>
        /// Set or get category Selected item changed command
        /// </summary>
        public ICommand CategorySelectedItemChangedCommand { get; private set; }

        private void CategorySelectedItemChangedExecute()
        {
            try
            {
                // Filter by category
                ProductModelCollection = (CategoryModel.Id != -1) ? 
                                            new ObservableCollection<base_ProductModel>(productCollection.Where(w => w.ProductCategoryId == CategoryModel.Id)):
                                            new ObservableCollection<base_ProductModel>(productCollection.OrderBy(o => o.ProductName));
                base_ProductModel product = CreateProduct();
                ProductModelCollection.Insert(0, product);
                ProductModel = product;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanCategorySelectedItemChangedExecute()
        {
            return (CategoryModel != null && ProductModelCollection != null && ProductModelCollection.Count > 0);
        }
        #endregion 

        #region -Store Selection Changed Command-
        /// <summary>
        /// Set or get Store SelectionChanged Command
        /// </summary>
        public ICommand StoreSelectionChangedCommand { get; private set; }

        private void StoreSelectionChangedExecute()
        {
            try
            {
                ToStoreModelCollection = new ObservableCollection<base_StoreModel>(
                        StoreModelCollection.Where(w => w.Id != StoreModel.Id)
                    );
                if (ToStoreModelCollection.Count > 0)
                {
                    ToStoreModel = ToStoreModelCollection[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanStoreSelectionChangedExecute()
        {
            return (ToStoreModelCollection != null && StoreModel != null && StoreModel.Id != -1);
        }
        #endregion 

        #endregion -Command- 

        #region -Private method-

        #region -Get all Store model-
        /// <summary>
        /// Get all store model
        /// </summary>
        private void GetAllStore()
        {
            StoreModelCollection = new ObservableCollection<base_StoreModel>(
                    storeRepo.GetAll()
                    .Select(s => new base_StoreModel(s))
                    .OrderBy(o => o.Id)
                );
            base_StoreModel store = new base_StoreModel();
            store.Id = -1;
            store.Code = "-1";
            store.Name = "-All Store-";
            StoreModelCollection.Insert(0, store);
            StoreModel = store; 
        }
        
        #endregion

        #region -Get Department and Category collection-
        /// <summary>
        /// Get all Department
        /// </summary>
        private void GetAllDepartment()
        {
            // Get all Depart model
            DepartModelCollection = new ObservableCollection<base_DepartmentModel>(
                    departRepo.GetAll().Select(d => new base_DepartmentModel(d))
                    .Where(d => d.LevelId == 0)
                    .OrderBy(o => o.Name)
                );
            base_DepartmentModel depart = CreateDepart(false);
            DepartModelCollection.Insert(0,depart);
            DepartModel = depart;         
        }

        /// <summary>
        /// Get all Category
        /// </summary>
        private void GetAllCategory()
        {
            // Get all category model            
            categoryCollection = new ObservableCollection<base_DepartmentModel>(
                    departRepo.GetAll().Select(c => new base_DepartmentModel(c))
                    .Where(w => w.LevelId == 1)
                    .OrderBy(o => o.Name)
                );
            CategoryModelCollection = categoryCollection;
            base_DepartmentModel category = CreateDepart(true);
            CategoryModelCollection.Insert(0, category);
            CategoryModel = category;
        }
        /// <summary>
        /// Create base Department or Category
        /// if true then Create Category
        /// else Create Department
        /// </summary>
        /// <param name="isCategory"></param>
        /// <returns></returns>
        private base_DepartmentModel CreateDepart(bool isCategory)
        {
            base_DepartmentModel depart = new base_DepartmentModel();
            depart.Id = -1;
            depart.Name = (isCategory == true) ? "-All Category-" : "-All Department-";
            return depart;
        }
        #endregion

        #region -Get Product collection-
        /// <summary>
        /// Get all Product
        /// </summary>
        private void GetAllProduct()
        {
            ProductModelCollection = new ObservableCollection<base_ProductModel>();
            // Get all Product model
            DBHelper dbHelper = new DBHelper();
            System.Data.DataTable da = dbHelper.ExecuteQuery("v_rpt_get_all_product");
            int count = da.Rows.Count;
            if (count > 0)
            {                
                int i = 0;
                for (i = 0; i < count; i++)
                {
                    base_ProductModel product = new base_ProductModel();
                    product.Id = long.Parse(da.Rows[i][0].ToString());
                    product.ProductName = da.Rows[i][1].ToString();
                    Guid guid = new Guid(da.Rows[i][2].ToString());
                    product.Resource = guid;
                    if (da.Rows[i][3] != DBNull.Value)
                    {
                        product.ProductCategoryId = int.Parse(da.Rows[i][3].ToString());
                    }
                    ProductModelCollection.Add(product);
                }
            }
            productCollection = ProductModelCollection;
            base_ProductModel base_Product = CreateProduct();
            ProductModelCollection.Insert(0, base_Product);
            ProductModel = base_Product;
        }
        /// <summary>
        /// Create base Product
        /// </summary>
        /// <returns></returns>
        private base_ProductModel CreateProduct()
        {
            base_ProductModel product = new base_ProductModel();
            product.Id = -1;
            product.ProductName = "-All Product-";
            return product;
        }
        #endregion

        #region -Get Customer vs Vendor collection-
        /// <summary>
        /// Get all Customer
        /// </summary>
        private void GetAllCustomer()
        {
            // Get all Customer model
            CustomerCollection = new ObservableCollection<base_GuestModel>(
                    guestRepo.GetAll().Select(c => new base_GuestModel(c))
                    .Where(w => !string.IsNullOrEmpty(w.Mark) && w.Mark.ToLower() == "c")
                    .OrderBy(o => o.LastName)
                );
            base_GuestModel customer = CreateGuest();
            CustomerCollection.Insert(0, customer);
            CustomerModel = customer;
        }

        /// <summary>
        /// Get all Vendor
        /// </summary>
        private void GetAllVendor()
        {
            // Get all Customer model
            CustomerCollection = new ObservableCollection<base_GuestModel>(
                    guestRepo.GetAll().Select(c => new base_GuestModel(c))
                    .Where(w => w.Mark.ToLower() == "v")
                    .OrderBy(o => o.Company)
                );
            base_GuestModel customer = CreateGuest();
            CustomerCollection.Insert(0, customer);
            CustomerModel = customer;
        }
        
        /// <summary>
        /// Create base Guest
        /// </summary>
        /// <returns></returns>
        private base_GuestModel CreateGuest()
        {
            base_GuestModel customer = new base_GuestModel();
            customer.Id = -1;
            customer.FirstName = "-All Customer-";
            customer.Company = "-All Vendor-";
            return customer;
        }
        #endregion

        #endregion

        #region -Load Data-
        /// <summary>
        /// Show control by report name
        /// </summary>
        private void LoadData(string reportname)
        {
            try
            {
                GetAllStore();
                switch (reportname)
                {
                    #region -Inventory-
                    case Common.RPT_PRODUCT_LIST:
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        break;
                    case Common.RPT_COST_ADJUSTMENT:
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        IsShowAdjustment = "Visible";
                        IsShowOrderDate = "Visible";
                        LblFromDate = "Change from";
                        LblToDate = "To";
                        GetAllCategory();
                        GetAllProduct();
                        break;
                    case Common.RPT_QTY_ADJUSTMENT:
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        IsShowAdjustment = "Visible";
                        IsShowOrderDate = "Visible";
                        LblFromDate = "Change from";
                        LblToDate = "To";
                        GetAllCategory();
                        GetAllProduct();
                        break;
                    case Common.RPT_PRODUCT_SUMMARY_ACTIVITY:
                        IsShowCategory01 = "Visible";
                        GetAllDepartment();
                        GetAllCategory();
                        break;
                    case Common.RPT_CATEGORY_LIST:
                        IsShowStore = "collapsed";
                        IsShowCategory01 = "Visible";
                        IsShowDepart00 = "Visible";
                        GetAllDepartment();
                        GetAllCategory();
                        break;
                    case Common.RPT_REORDER_STOCK:
                        IsShowProduct01 = "Visible";
                        GetAllProduct();                        
                        break;
                    case Common.RPT_TRANSFER_HISTORY:
                        IsShowToStore = "Visible";
                        IsShowOrderDate = "Visible";
                        IsShowTransferStockStatus = "Visible";
                        LblFromDate = "Create from";
                        LblToDate = "To";
                        ToStoreModelCollection = new ObservableCollection<base_StoreModel>(StoreModelCollection);
                        ToStoreModel = StoreModel;
                        break;
                    case Common.RPT_TRANSER_DETAILS:
                        IsShowProduct11 = "Visible";
                        IsShowCategory10 = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        break;
                    #endregion

                    #region -Purchasing-
                    case Common.RPT_PO_SUMMARY:
                        IsShowCustomer11 = "Visible";
                        IsShowPOStatus = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllVendor();
                        break;
                    case Common.RPT_PO_DETAILS:
                        IsShowProduct01 = "Visible";
                        IsShowPOStatus = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllProduct();
                        break;
                    case Common.RPT_PRODUCT_COST:
                        IsShowCustomer11 = "Visible";                        
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        GetAllVendor();                        
                        break;
                    case Common.RPT_VENDOR_PRODUCT_LIST:
                        IsShowCustomer11 = "Visible";                        
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible"; 
                        GetAllCategory();
                        GetAllProduct();
                        GetAllVendor();
                        break;
                    case Common.RPT_VENDOR_LIST:
                        IsShowCountry = "Visible";
                        IsShowStore = "collapsed";
                        IsShowCustomer11 = "Visible";
                        GetAllVendor();
                        break;
                    #endregion

                    #region -Sales-
                    case Common.RPT_SALE_BY_PRODUCT_SUMMARY:
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        break;
                    case Common.RPT_SALE_BY_PRODUCT_DETAILS:
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        break;
                    case Common.RPT_SALE_ORDER_SUMMARY:
                        IsShowCustomer11 = "Visible";
                        IsShowSOStatus11 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCustomer();
                        break;
                    case Common.RPT_SALE_PROFIT_SUMMARY:
                        IsShowCustomer11 = "Visible";
                        IsShowSOStatus11 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCustomer();
                        break;
                    case Common.RPT_SALE_ORDER_OPERATION:
                        IsShowCustomer11 = "Visible";
                        IsShowSOStatus11 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCustomer();
                        break;
                    case Common.RPT_CUSTOMER_PAYMENT_SUMMARY:
                        IsShowCustomer11 = "Visible";
                        IsShowOrderDate = "Visible";
                        IsShowShipDate = "Visible";
                        LblFromDate = "Order from";
                        LblToDate = "Order to";
                        LblShipFromDate = "Payment from";
                        LblShipToDate = "To";
                        GetAllCustomer();
                        break;
                    case Common.RPT_CUSTOMER_PAYMENT_DETAILS:
                        IsShowCustomer11 = "Visible";
                        IsShowOrderDate = "Visible";
                        IsShowShipDate = "Visible";
                        LblFromDate = "Invoice from";
                        LblToDate = "To";
                        LblShipFromDate = "Paid from";
                        LblShipToDate = "To";
                        IsShowSOStatus11 = "Visible";
                        GetAllCustomer();
                        break;
                    case Common.RPT_PRODUCT_CUSTOMER:
                        IsShowCustomer33 = "Visible";
                        IsShowProduct11 = "Visible";
                        IsShowSOStatus33 = "Visible";
                        IsShowOrderDate = "Visible";
                        IsShowCategory10 = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        GetAllCustomer();
                        break;
                    case Common.RPT_CUSTOMER_ORDER_HISTORY:
                        IsShowCustomer33 = "Visible";
                        IsShowCategory10 = "Visible";
                        IsShowProduct11 = "Visible";
                        IsShowSOStatus33 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCategory();
                        GetAllProduct();
                        GetAllCustomer();
                        break;
                    case Common.RPT_SALE_REPRESENTATIVE:
                        IsShowCustomer11 = "Visible";
                        IsShowSOStatus11 = "Visible";
                        IsShowOrderDate = "Visible";
                        GetAllCustomer();
                        break;
                    case Common.RPT_VOIDED_INVOICE:
                        IsShowOrderDate = "Visible";
                        LblFromDate = "From";
                        LblToDate = "to";
                        break;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
        }

        #endregion        
    }
}
