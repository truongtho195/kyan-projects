using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;
using CPCToolkitExtLibraries;
using CPC.POS.Report.CrystalReport;
using SAPBusinessObjects.WPF.Viewer;
using System.Data;

namespace CPC.POS.ViewModel
{
    public class SalesOrderViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }


        //Respository
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        private base_SaleTaxLocationRepository _saleTaxRepository = new base_SaleTaxLocationRepository();
        private base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_GuestRewardRepository _guestRewardRepository = new base_GuestRewardRepository();
        private base_SaleOrderShipRepository _saleOrderShipRepository = new base_SaleOrderShipRepository();
        private base_SaleOrderShipDetailRepository _saleOrderShipDetailRepository = new base_SaleOrderShipDetailRepository();
        private base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();
        private base_ResourceReturnRepository _resourceReturnRepository = new base_ResourceReturnRepository();
        private base_ResourceReturnDetailRepository _resourceReturnDetailRepository = new base_ResourceReturnDetailRepository();
        private base_ProductGroupRepository _productGroupRepository = new base_ProductGroupRepository();

        private base_ProductStoreRepository _productStoreRespository = new base_ProductStoreRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();
        private string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        private string EMPLOYEE_MARK = MarkType.Employee.ToDescription();

        private bool _flagCustomerSetRelate = true;
        private bool _viewExisted = false;

        private List<base_PromotionModel> _promotionList;

        private enum SaleOrderTab
        {
            Order = 0,
            Ship = 1,
            Payment = 2,
            Return = 3
        }

        #endregion

        #region Constructors

        public SalesOrderViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            //Get value from config
            IsIncludeReturnFee = Define.CONFIGURATION.IsIncludeReturnFee;
            InitialCommand();

            // Get permission
            GetPermission();
        }

        public SalesOrderViewModel(bool isList, object param)
            : this()
        {
            LoadStaticData();
            LoadDynamicData();
            ChangeSearchMode(isList, param);
        }

        /// <summary>
        /// Constructor for quotation
        /// </summary>
        /// <param name="isList"></param>
        /// <param name="quotation"></param>
        public SalesOrderViewModel(bool isList)
            : this()
        {
            LoadStaticData();
            LoadDynamicData();
            ChangeSearchMode(isList, null);
        }

        #endregion

        #region Properties

        #region IsForceFocused
        private bool _isForceFocused;
        /// <summary>
        /// Gets or sets the IsForceFocus.
        /// </summary>
        public bool IsForceFocused
        {
            get { return _isForceFocused; }
            set
            {
                if (_isForceFocused != value)
                {
                    _isForceFocused = value;
                    OnPropertyChanged(() => IsForceFocused);
                }
            }
        }
        #endregion

        #region BreakAllChange
        private bool _breakAllChange = false;
        /// <summary>
        /// Gets or sets the BreakAllChange.
        /// </summary>
        public bool BreakAllChange
        {
            get { return _breakAllChange; }
            set
            {
                if (_breakAllChange != value)
                {
                    _breakAllChange = value;
                    _breakSODetailChange = value;
                }
            }
        }
        #endregion

        #region BreakSODetailChange
        private bool _breakSODetailChange = false;
        /// <summary>
        /// Gets or sets the BreakSODetailChange.
        /// </summary>
        public bool BreakSODetailChange
        {
            get { return _breakSODetailChange; }
            set
            {
                if (_breakSODetailChange != value)
                {
                    _breakSODetailChange = value;
                }
            }
        }
        #endregion

        #region IsIncludeReturnFee
        private bool _isIncludeReturnFee;
        /// <summary>
        /// Gets or sets the IsIncludeReturnFee.
        /// </summary>
        public bool IsIncludeReturnFee
        {
            get { return _isIncludeReturnFee; }
            set
            {
                if (_isIncludeReturnFee != value)
                {
                    _isIncludeReturnFee = value;
                    OnPropertyChanged(() => IsIncludeReturnFee);
                }
            }
        }
        #endregion

        #region IsSearchMode
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
        #endregion

        #region IsDirty
        /// <summary>
        /// Gets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                return SelectedSaleOrder.IsDirty
                    || (SelectedSaleOrder.SaleOrderDetailCollection != null
                            && (SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsDirty)
                            || SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Any()))
                    || (SelectedSaleOrder.SaleOrderShipCollection != null
                            && (SelectedSaleOrder.SaleOrderShipCollection.Any(x => x.IsDirty)
                            || SelectedSaleOrder.SaleOrderShipCollection.DeletedItems.Any()))
                    || (SelectedSaleOrder.ReturnModel != null && (SelectedSaleOrder.ReturnModel.IsDirty || SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.IsDirty)))
                    || (SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any(x => x.IsDirty))
                    || (SelectedSaleOrder.BillAddressModel != null && SelectedSaleOrder.BillAddressModel.IsDirty)
                    || (SelectedSaleOrder.ShipAddressModel != null && SelectedSaleOrder.ShipAddressModel.IsDirty);
            }

        }
        #endregion

        #region IsShipValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsShipValid
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                if (SelectedSaleOrder.SaleOrderShipCollection == null || (SelectedSaleOrder.SaleOrderShipCollection != null && !SelectedSaleOrder.SaleOrderShipCollection.Any()))
                    return true;

                return (SelectedSaleOrder.SaleOrderShipCollection != null && !SelectedSaleOrder.SaleOrderShipCollection.Any(x => x.IsError));
            }

        }
        #endregion

        #region IsOrderValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsOrderValid
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                if (SelectedSaleOrder.SaleOrderDetailCollection == null || (SelectedSaleOrder.SaleOrderDetailCollection != null && !SelectedSaleOrder.SaleOrderDetailCollection.Any()))
                    return true;

                return (SelectedSaleOrder.SaleOrderDetailCollection != null && !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsError))
                    && !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);
            }

        }
        #endregion

        #region IsReturnValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsReturnValid
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                return (SelectedSaleOrder.ReturnModel != null && !SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.HasError));
            }

        }
        #endregion

        #region IsValidTab
        private bool _isValidTab;
        /// <summary>
        /// Gets or sets the IsValidTab.
        /// </summary>
        public bool IsValidTab
        {
            get { return _isValidTab; }
            set
            {
                if (_isValidTab != value)
                {
                    _isValidTab = value;
                    OnPropertyChanged(() => IsValidTab);
                }
            }
        }
        #endregion

        #region BillAddressTypeCollection
        private AddressTypeCollection _billAddressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection BillAddressTypeCollection
        {
            get { return _billAddressTypeCollection; }
            set
            {
                if (_billAddressTypeCollection != value)
                {
                    _billAddressTypeCollection = value;
                    OnPropertyChanged(() => BillAddressTypeCollection);
                }
            }
        }
        #endregion

        #region ShipAddressTypeCollection
        private AddressTypeCollection _shipAddressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection ShipAddressTypeCollection
        {
            get { return _shipAddressTypeCollection; }
            set
            {
                if (_shipAddressTypeCollection != value)
                {
                    _shipAddressTypeCollection = value;
                    OnPropertyChanged(() => ShipAddressTypeCollection);
                }
            }
        }
        #endregion

        #region EmployeeCollection
        private ObservableCollection<base_GuestModel> _employeeCollection;
        /// <summary>
        /// Gets or sets the EmployeeCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> EmployeeCollection
        {
            get { return _employeeCollection; }
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

        #region StoreCollection
        private ObservableCollection<base_Store> _storeCollection;
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public ObservableCollection<base_Store> StoreCollection
        {
            get { return _storeCollection; }
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

        public List<base_SaleTaxLocationModel> SaleTaxLocationCollection { get; set; }

        #region SearchOption
        private int _searchOption;
        /// <summary>
        /// Gets or sets the SearchOption.
        /// </summary>
        public int SearchOption
        {
            get { return _searchOption; }
            set
            {
                if (_searchOption != value)
                {
                    _searchOption = value;
                    OnPropertyChanged(() => SearchOption);
                    if (!string.IsNullOrWhiteSpace(FilterText))
                        OnSearchCommandExecute(FilterText);
                }
            }
        }
        #endregion

        #region FilterText & Keyword
        private string _filterText;
        /// <summary>
        /// Gets or sets the FilterText.
        /// <para>Keyword user input but not press enter</para>
        /// <remarks>Binding in textbox keyword</remarks>
        /// </summary>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(() => FilterText);
                }
            }
        }

        public string Keyword { get; set; }
        #endregion

        #region SearchAlert
        private string _searchAlert;
        /// <summary>
        /// Gets or sets the SearchAlert.
        /// </summary>
        public string SearchAlert
        {
            get { return _searchAlert; }
            set
            {
                if (_searchAlert != value)
                {
                    _searchAlert = value;
                    OnPropertyChanged(() => SearchAlert);
                }
            }
        }
        #endregion

        //Customer
        #region CustomerFieldCollection
        ///// <summary>
        ///// Gets or sets the CustomerFieldCollection for Autocomplete Control
        ///// </summary>
        public DataSearchCollection CustomerFieldCollection { get; set; }
        #endregion

        #region CustomerCollection
        private CollectionBase<base_GuestModel> _customerCollection;
        /// <summary>
        /// Gets or sets the CustomerCollection.
        /// </summary>
        public CollectionBase<base_GuestModel> CustomerCollection
        {
            get { return _customerCollection; }
            set
            {
                if (_customerCollection != value)
                {
                    _customerCollection = value;
                    OnPropertyChanged(() => CustomerCollection);
                }
            }
        }
        #endregion

        #region SelectedCustomer

        private base_GuestModel _selectedCustomer;
        /// <summary>
        /// Gets or sets the SelectedCustomer.
        /// </summary>
        public base_GuestModel SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged(() => SelectedCustomer);
                    if (SelectedCustomer != null)
                        SelectedCustomerChanged(_flagCustomerSetRelate);
                }
            }
        }

        #endregion

        //Sale Order
        #region SelectedSaleOrder

        private base_SaleOrderModel _selectedSaleOrder;
        /// <summary>
        /// Gets or sets the SelectedSaleOrder.
        /// </summary>
        public base_SaleOrderModel SelectedSaleOrder
        {
            get { return _selectedSaleOrder; }
            set
            {
                if (_selectedSaleOrder != value)
                {
                    _selectedSaleOrder = value;
                    OnPropertyChanged(() => SelectedSaleOrder);
                    if (SelectedSaleOrder != null)
                    {
                        SelectedSaleOrder.PropertyChanged -= new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
                        SelectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
                    }
                }
            }
        }

        #endregion

        #region SaleOrderCollection
        private CollectionBase<base_SaleOrderModel> _saleOrderCollection = new CollectionBase<base_SaleOrderModel>();
        /// <summary>
        /// Gets or sets the SaleOrderCollection.
        /// </summary>
        public CollectionBase<base_SaleOrderModel> SaleOrderCollection
        {
            get { return _saleOrderCollection; }
            set
            {
                if (_saleOrderCollection != value)
                {
                    _saleOrderCollection = value;
                    OnPropertyChanged(() => SaleOrderCollection);
                }
            }
        }
        #endregion

        #region TotalSaleOrder
        private int _totalSaleOrder;
        /// <summary>
        /// Gets or sets the TotalSaleOrder.
        /// </summary>
        public int TotalSaleOrder
        {
            get { return _totalSaleOrder; }
            set
            {
                if (_totalSaleOrder != value)
                {
                    _totalSaleOrder = value;
                    OnPropertyChanged(() => TotalSaleOrder);
                }
            }
        }
        #endregion

        #region SelectedSaleOrderDetail
        private base_SaleOrderDetailModel _selectedSaleOrderDetail;
        /// <summary>
        /// Gets or sets the SelectedSaleOrderDetail.
        /// </summary>
        public base_SaleOrderDetailModel SelectedSaleOrderDetail
        {
            get { return _selectedSaleOrderDetail; }
            set
            {
                if (_selectedSaleOrderDetail != value)
                {
                    _selectedSaleOrderDetail = value;
                    OnPropertyChanged(() => SelectedSaleOrderDetail);
                }
            }
        }

        #endregion

        #region SelectedSaleOrderShip
        private base_SaleOrderShipModel _selectedSaleOrderShip;
        /// <summary>
        /// Gets or sets the SelectedSaleOrderShip.
        /// </summary>
        public base_SaleOrderShipModel SelectedSaleOrderShip
        {
            get { return _selectedSaleOrderShip; }
            set
            {
                if (_selectedSaleOrderShip != value)
                {
                    _selectedSaleOrderShip = value;
                    OnPropertyChanged(() => SelectedSaleOrderShip);
                    if (SelectedSaleOrderShip != null)
                    {
                        SelectedSaleOrderShip.PropertyChanged -= new PropertyChangedEventHandler(SelectedSaleOrderShip_PropertyChanged);
                        SelectedSaleOrderShip.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrderShip_PropertyChanged);
                    }
                }
            }
        }


        #endregion

        #region SaleOrderId
        private long _saleOrderId = 0;
        /// <summary>
        /// Gets or sets the QuotationId.
        /// </summary>
        public long SaleOrderId
        {
            get { return _saleOrderId; }
            set
            {
                if (_saleOrderId != value)
                {
                    _saleOrderId = value;
                    OnPropertyChanged(() => SaleOrderId);
                }
            }
        }
        /// <summary>
        /// Flag using for call from another from & set what tab user want
        /// </summary>
        private SaleOrderTab SaleOrderSelectedTab { get; set; }
        #endregion

        //Products
        #region ProductFieldCollection
        private DataSearchCollection _productFieldCollection;
        /// <summary>
        /// Gets or sets the ProductFieldCollection.
        /// </summary>
        public DataSearchCollection ProductFieldCollection
        {
            get { return _productFieldCollection; }
            set
            {
                if (_productFieldCollection != value)
                {
                    _productFieldCollection = value;
                    OnPropertyChanged(() => ProductFieldCollection);
                }
            }
        }
        #endregion

        #region ProductCollection
        private ObservableCollection<base_ProductModel> _productCollection;
        /// <summary>
        /// Gets or sets the ProductCollection.
        /// </summary>
        public ObservableCollection<base_ProductModel> ProductCollection
        {
            get { return _productCollection; }
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

        #region SelectedProduct
        private base_ProductModel _selectedProduct;
        /// <summary>
        /// Gets or sets the SelectedProduct.
        /// </summary>
        public base_ProductModel SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(() => SelectedProduct);
                    SelectedProductChanged();
                }
            }
        }

        #endregion

        #region SelectedTabIndex
        private int _previousTabIndex;
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
                    _previousTabIndex = _selectedTabIndex;
                    _selectedTabIndex = value;
                    TabChanged(value);
                    OnPropertyChanged(() => SelectedTabIndex);
                }
            }
        }
        #endregion

        #region IsAllowChangeOrder
        private bool _isAllowChangeOrder = true;
        /// <summary>
        /// Gets or sets the IsAllowChangeOrder.
        /// Allow change order when order not ship full Or Config allow change
        /// </summary>
        public bool IsAllowChangeOrder
        {
            get { return _isAllowChangeOrder; }
            set
            {
                if (_isAllowChangeOrder != value)
                {
                    _isAllowChangeOrder = value;
                    OnPropertyChanged(() => IsAllowChangeOrder);
                }
            }
        }
        #endregion

        //Return
        #region SaleOrderShipDetailFieldCollection
        private DataSearchCollection _saleOrderShipDetailFieldCollection;
        /// <summary>
        /// Gets or sets the SaleOrderShipDetailFieldCollection.
        /// </summary>
        public DataSearchCollection SaleOrderShipDetailFieldCollection
        {
            get { return _saleOrderShipDetailFieldCollection; }
            set
            {
                if (_saleOrderShipDetailFieldCollection != value)
                {
                    _saleOrderShipDetailFieldCollection = value;
                    OnPropertyChanged(() => SaleOrderShipDetailFieldCollection);
                }
            }
        }
        #endregion

        #region SelectedReturnDetail
        private object _selectedReturnDetail;
        /// <summary>
        /// Gets or sets the SelectedReturnDetail.
        /// </summary>
        public object SelectedReturnDetail
        {
            get { return _selectedReturnDetail; }
            set
            {
                if (_selectedReturnDetail != value)
                {
                    _selectedReturnDetail = value;
                    OnPropertyChanged(() => SelectedReturnDetail);
                    SelectedReturnDetailChanged();
                }
            }
        }


        #endregion

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (ChangeViewExecute(null))
            {
                CreateNewSaleOrder();
                IsSearchMode = false;
                IsForceFocused = true;
            }
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return IsDirty && IsValid && IsShipValid & IsOrderValid & IsReturnValid;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SaveSalesOrder();
        }

        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew && (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Open) || SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote));
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            if (SelectedSaleOrder != null)
            {
                //"Do you want to delete this item?"
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.Yes))
                {
                    SelectedSaleOrder.IsPurge = true;
                    SaveSalesOrder();
                    this.SaleOrderCollection.Remove(SelectedSaleOrder);
                    TotalSaleOrder -= 1;
                    _selectedSaleOrder = null;
                    IsSearchMode = true;
                }
            }
        }
        #endregion

        #region PrintCommand
        /// <summary>
        /// Gets the Print Command.
        /// <summary>

        public RelayCommand<object> PrintCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Print command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPrintCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew && SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any(x => !x.IsNew && x.IsDeposit.HasValue && !x.IsDeposit.Value);
        }


        /// <summary>
        /// Method to invoke when the Print command is executed.
        /// </summary>
        private void OnPrintCommandExecute(object param)
        {
            // Open  Report window   
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            string saleOrderResource = string.Empty;
            if (SelectedSaleOrder.Resource != null)
            {
                saleOrderResource = "'" + SelectedSaleOrder.Resource.ToString() + "'";
            }
            rpt.DataContext = new ReportViewModel("rptSODetails", saleOrderResource);
            rpt.ShowDialog();
        }
        #endregion

        #region SearchCommand
        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        private void OnSearchCommandExecute(object param)
        {
            SearchAlert = string.Empty;
            if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)//Search All
            {
                Expression<Func<base_SaleOrder, bool>> predicate = CreatePredicateWithConditionSearch(Keyword);
                LoadDataByPredicate(predicate, false, 0);

            }
            else if (param != null)
            {
                Keyword = param.ToString();
                if (SearchOption == 0)
                {
                    //Thong bao Can co dk
                    SearchAlert = "Search Option is required";
                }
                else
                {
                    Expression<Func<base_SaleOrder, bool>> predicate = CreatePredicateWithConditionSearch(Keyword);

                    LoadDataByPredicate(predicate, false, 0);
                }
            }
        }

        #endregion

        #region DoubleClickCommand

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

                SelectedSaleOrder = param as base_SaleOrderModel;

                SetSaleOrderRelation(SelectedSaleOrder);
                SelectedSaleOrder.RaiseTotalPaid();
                _selectedTabIndex = (int)SaleOrderTab.Order;
                OnPropertyChanged(() => SelectedTabIndex);
                //Set for selectedCustomer
                _flagCustomerSetRelate = false;
                SelectedCustomer = SelectedSaleOrder.GuestModel;

                SetAllowChangeOrder(SelectedSaleOrder);
                SelectedSaleOrder.IsDirty = false;
                _flagCustomerSetRelate = true;


                OnPropertyChanged(() => AllowSOShipping);
                OnPropertyChanged(() => AllowSOReturn);

                IsSearchMode = false;


            }
            else if (!IsSearchMode)//Change from Edit form to Search Gird check view has dirty
            {
                if (this.ChangeViewExecute(null))
                    this.IsSearchMode = true;
            }
            else
                this.IsSearchMode = !this.IsSearchMode;//Change View To
        }


        #endregion

        #region AddressPopupCommand
        public RelayCommand<object> AddressPopupCommand { get; private set; }

        /// <summary>
        /// Method to check whether the AddressPopup command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddressPopupCommandCanExecute(object param)
        {
            if (param == null) return false;
            if (SelectedCustomer == null) return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddressPopup command is executed.
        /// </summary>
        private void OnAddressPopupCommandExecute(object param)
        {
            base_GuestAddressModel addressModel = param as base_GuestAddressModel;
            PopupAddressViewModel addressViewModel = new PopupAddressViewModel(SelectedSaleOrder.GuestModel, addressModel);

            string strTitle = addressModel.AddressTypeId.Is(AddressType.Billing) ? "Bill Address" : "Ship Address";
            //addressViewModel.AddressModel = addressModel;
            bool? result = _dialogService.ShowDialog<PopupAddressView>(_ownerViewModel, addressViewModel, strTitle);
            if (result == true)
            {
                if (addressViewModel.AddressModel.AddressTypeId == (int)AddressType.Billing)
                    SelectedSaleOrder.BillAddress = addressViewModel.AddressModel.Text;
                if (addressViewModel.AddressModel.AddressTypeId == (int)AddressType.Shipping)
                    SelectedSaleOrder.ShipAddress = addressViewModel.AddressModel.Text;
            }
        }
        #endregion

        #region DeleteSaleOrderDetailCommand
        /// <summary>
        /// Gets the DeleteSaleOrderDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteSaleOrderDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderDetailCommandCanExecute(object param)
        {
            if (param == null) return false;
            return (param as base_SaleOrderDetailModel).PickQty == 0 && IsAllowChangeOrder && AllowDeleteProduct;
        }

        /// <summary>
        /// Method to invoke when the DeleteSaleOrderDetail command is executed.
        /// </summary>
        private void OnDeleteSaleOrderDetailCommandExecute(object param)
        {
            base_SaleOrderDetailModel saleOrderDetailModel = param as base_SaleOrderDetailModel;
            if (saleOrderDetailModel.PickQty == 0 && IsAllowChangeOrder)
            {
                DeleteItemSaleOrderDetail(saleOrderDetailModel);
            }
        }

        /// <summary>
        /// Gets the DeleteSaleOrderDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderDetailWithKeyCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteSaleOrderDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderDetailWithKeyCommandCanExecute(object param)
        {
            if (param == null) return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteSaleOrderDetail command is executed.
        /// </summary>
        private void OnDeleteSaleOrderDetailWithKeyCommandExecute(object param)
        {
            if (AllowDeleteProduct)
            {
                base_SaleOrderDetailModel saleOrderDetailModel = param as base_SaleOrderDetailModel;
                if (saleOrderDetailModel.PickQty > 0 || !IsAllowChangeOrder)
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ItemPicked"), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    DeleteItemSaleOrderDetail(saleOrderDetailModel);
            }
        }

        /// <summary>
        /// Method confirm & delete saleorder detail
        /// <para>Using for Delete by key & Menucontext</para>
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void DeleteItemSaleOrderDetail(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            //msg: Do you want to delete? 
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo);
            if (result.Is(MessageBoxResult.Yes))
            {
                //Item is product group => remove child item
                if (saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                {
                    foreach (base_SaleOrderDetailModel soDetailInGroup in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.Resource.ToString())).ToList())
                    {
                        SelectedSaleOrder.SaleOrderDetailCollection.Remove(soDetailInGroup);
                    }

                }//Get ProductInGroup if current item deleted is only 1, remove parent item
                else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))
                {
                    int numTheSameItem = SelectedSaleOrder.SaleOrderDetailCollection.Count(x => x.ParentResource.Equals(saleOrderDetailModel.ParentResource) && !x.Resource.Equals(saleOrderDetailModel.Resource));
                    if (numTheSameItem == 0)
                    {
                        base_SaleOrderDetailModel parentDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetailModel.ParentResource));
                        if (parentDetailModel != null)
                        {
                            SelectedSaleOrder.SaleOrderDetailCollection.Remove(parentDetailModel);
                        }
                    }
                }

                SelectedSaleOrder.SaleOrderDetailCollection.Remove(saleOrderDetailModel);
                //Not calculate discount on product group or product in group
                if (string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)
                    && !saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                {
                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    BreakSODetailChange = false;
                }
                SelectedSaleOrder.CalcSubTotal();
                SelectedSaleOrder.CalcBalance();
                if (SelectedSaleOrder.SaleOrderDetailCollection != null)
                    SelectedSaleOrder.IsHiddenErrorColumn = !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);
            }
        }
        #endregion

        #region AddNewCustomerCommand
        /// <summary>
        /// Gets the AddNewCustomer Command.
        /// <summary>

        public RelayCommand<object> AddNewCustomerCommand { get; private set; }

        /// <summary>
        /// Method to check whether the AddNewCustomer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddNewCustomerCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddNewCustomer command is executed.
        /// </summary>
        private void OnAddNewCustomerCommandExecute(object param)
        {
            PopupGuestViewModel popupGuestViewModel = new PopupGuestViewModel(MarkType.Customer, AddressType.Billing, (short)CustomerTypes.Individual);
            bool? result = _dialogService.ShowDialog<PopupGuestView>(_ownerViewModel, popupGuestViewModel, "Add Customer");
            if (result == true && popupGuestViewModel.NewItem != null)
            {
                CustomerCollection.Add(popupGuestViewModel.NewItem);
                SelectedCustomer = popupGuestViewModel.NewItem;
            }
        }
        #endregion

        #region QuantityChanged Command
        /// <summary>
        /// Gets the QtyChanged Command.
        /// <summary>

        public RelayCommand<object> QtyChangedCommand { get; private set; }

        /// <summary>
        /// Method to check whether the QtyChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnQtyChangedCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the QtyChanged command is executed.
        /// </summary>
        private void OnQtyChangedCommandExecute(object param)
        {
            if (param != null && SelectedSaleOrderDetail != null && !decimal.Parse(param.ToString()).Equals(SelectedSaleOrderDetail.Quantity))
            {
                SelectedSaleOrderDetail.Quantity = decimal.Parse(param.ToString());

                if (SelectedSaleOrderDetail.ProductModel != null && SelectedSaleOrderDetail.ProductModel.IsSerialTracking)
                    if (!SelectedSaleOrderDetail.IsError && SelectedSaleOrderDetail.Quantity > 0)
                        OpenTrackingSerialNumber(SelectedSaleOrderDetail, true);
                    else
                        SelectedSaleOrderDetail.SerialTracking = string.Empty;
                SelectedSaleOrder.CalcSubTotal();
            }
        }
        #endregion

        #region ManualChangePrice
        /// <summary>
        /// Gets the ManualChangePrice Command.
        /// <summary>

        public RelayCommand<object> ManualChangePriceCommand { get; private set; }


        /// <summary>
        /// Method to check whether the ManualChangePrice command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnManualChangePriceCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the ManualChangePrice command is executed.
        /// </summary>
        private void OnManualChangePriceCommandExecute(object param)
        {
            if (SelectedSaleOrderDetail != null && param != null && !Convert.ToDecimal(param).Equals(SelectedSaleOrderDetail.SalePrice))
            {
                SelectedSaleOrderDetail.IsManual = true;
                SelectedSaleOrderDetail.PromotionId = 0;
                SelectedSaleOrderDetail.SalePrice = Convert.ToDecimal(param);

                BreakSODetailChange = true;
                _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, SelectedSaleOrderDetail);
                BreakSODetailChange = false;

            }
        }
        #endregion

        #region AddTermCommand
        /// <summary>
        /// Gets the AddTerm Command.
        /// <summary>
        public RelayCommand AddTermCommand { get; private set; }


        /// <summary>
        /// Method to check whether the AddTerm command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddTermCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddTerm command is executed.
        /// </summary>
        private void OnAddTermCommandExecute()
        {
            short dueDays = SelectedSaleOrder.TermNetDue;
            decimal discount = SelectedSaleOrder.TermDiscountPercent;
            short discountDays = SelectedSaleOrder.TermPaidWithinDay;
            PaymentTermViewModel paymentTermViewModel = new PaymentTermViewModel(dueDays, discount, discountDays);
            bool? dialogResult = _dialogService.ShowDialog<PaymentTermView>(_ownerViewModel, paymentTermViewModel, "Add Term");
            if (dialogResult == true)
            {
                SelectedSaleOrder.TermNetDue = paymentTermViewModel.DueDays;
                SelectedSaleOrder.TermDiscountPercent = paymentTermViewModel.Discount;
                SelectedSaleOrder.TermPaidWithinDay = paymentTermViewModel.DiscountDays;
                SelectedSaleOrder.PaymentTermDescription = paymentTermViewModel.Description;
            }
        }
        #endregion

        #region LoadDatByStepCommand

        public RelayCommand<object> LoadStepCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadStep command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStep command is executed.
        /// </summary>
        private void OnLoadStepCommandExecute(object param)
        {
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = CreatePredicateWithConditionSearch(Keyword);
            LoadDataByPredicate(predicate, false, SaleOrderCollection.Count);
        }
        #endregion

        #region SearchProductAdvance
        /// <summary>
        /// Gets the SearchProductAdvance Command.
        /// <summary>

        public RelayCommand<object> SearchProductAdvanceCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SearchProductAdvance command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchProductAdvanceCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SearchProductAdvance command is executed.
        /// </summary>
        private void OnSearchProductAdvanceCommandExecute(object param)
        {
            SearchProductAdvance();
        }
        #endregion

        #region PickPackCommand
        /// <summary>
        /// Gets the PickPack Command.
        /// <summary>
        public RelayCommand<object> PickPackCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PickPack command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPickPackCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;

            return !SelectedSaleOrder.IsLocked && SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.DueQty > 0);
        }


        /// <summary>
        /// Method to invoke when the PickPack command is executed.
        /// </summary>
        private void OnPickPackCommandExecute(object param)
        {
            if (SelectedSaleOrder.Mark.Equals(MarkType.Layaway.ToDescription()) && SelectedSaleOrder.Balance > 0)
            {
                //msg:"You should paid in full"
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_NotifyShouldPaidInFull"), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            }

            PickPackViewModel pickPackViewModel = new PickPackViewModel(SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.DueQty > 0));
            bool? dialogResult = _dialogService.ShowDialog<PickPackView>(_ownerViewModel, pickPackViewModel, "Pick Pack");
            if (dialogResult == true)
            {
                if (pickPackViewModel.SaleOrderShipModel != null)
                {
                    pickPackViewModel.SaleOrderShipModel.SaleOrderId = SelectedSaleOrder.Id;
                    pickPackViewModel.SaleOrderShipModel.SaleOrderResource = SelectedSaleOrder.Resource.ToString();
                    //Check item is Existed in collection
                    var saleOrderShipModel = SelectedSaleOrder.SaleOrderShipCollection.SingleOrDefault(x => x.Resource == pickPackViewModel.SaleOrderShipModel.Resource);
                    if (saleOrderShipModel != null)
                    {
                        if (pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection != null && !pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection.Any())
                            saleOrderShipModel = pickPackViewModel.SaleOrderShipModel;
                        else
                            SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                    }
                    else
                        SelectedSaleOrder.SaleOrderShipCollection.Add(pickPackViewModel.SaleOrderShipModel);

                    //Update SaleOrderDetail
                    foreach (var item in pickPackViewModel.SaleOrderDetailList)
                    {
                        base_SaleOrderDetailModel saleOrderDetailUpdate = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == item.Resource);
                        saleOrderDetailUpdate.PickQty = item.PickQty;
                    }
                }

                SelectedSaleOrder.RaiseTotalPackedBox();
                SetShippingStatus(SelectedSaleOrder);
            }
        }
        #endregion

        #region DeleteSaleOrderShipCommand
        /// <summary>
        /// Gets the DeleteSaleOrderShip Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderShipCommand { get; private set; }


        /// <summary>
        /// Method to check whether the DeleteSaleOrderShip command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderShipCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return !(param as base_SaleOrderShipModel).IsShipped;
        }


        /// <summary>
        /// Method to invoke when the DeleteSaleOrderShip command is executed.
        /// </summary>
        private void OnDeleteSaleOrderShipCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;

            DeleteItemSaleOrderShip(saleOrderShipModel);
        }

        /// <summary>
        /// Gets the DeleteSaleOrderShip Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderShipWithKeyCommand { get; private set; }


        /// <summary>
        /// Method to check whether the DeleteSaleOrderShip command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderShipWithKeyCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteSaleOrderShip command is executed.
        /// </summary>
        private void OnDeleteSaleOrderShipWithKeyCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            if (saleOrderShipModel.IsShipped == true)
            {
                //This item is picked, can't delete
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ItemPicked"), Language.GetMsg("DeleteCaption"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                DeleteItemSaleOrderShip(saleOrderShipModel);
            }
        }

        /// <summary>
        ///confirm Delete item saleordership
        /// </summary>
        /// <param name="saleOrderShipModel"></param>
        private void DeleteItemSaleOrderShip(base_SaleOrderShipModel saleOrderShipModel)
        {
            //msg: NotifyDeleteItem
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result.Is(MessageBoxResult.Yes))
            {
                if (saleOrderShipModel.SaleOrderShipDetailCollection != null)
                {
                    //UpdatePickQty
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        Guid saleOrderDetailResource = Guid.Parse(saleOrderShipDetailModel.SaleOrderDetailResource);
                        var saleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == saleOrderDetailResource);
                        saleOrderDetailModel.PickQty -= saleOrderShipDetailModel.PackedQty;
                    }
                    SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                    SelectedSaleOrder.RaiseTotalPackedBox();
                }
            }
        }
        #endregion

        #region EditSaleOrderShipCommand
        /// <summary>
        /// Gets the EditSaleOrderShip Command.
        /// <summary>

        public RelayCommand<object> EditSaleOrderShipCommand { get; private set; }

        /// <summary>
        /// Method to check whether the EditSaleOrderShip command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditSaleOrderShipCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param as base_SaleOrderShipModel).IsShipped == false;
        }


        /// <summary>
        /// Method to invoke when the EditSaleOrderShip command is executed.
        /// </summary>
        private void OnEditSaleOrderShipCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            //Get list SaleOrderDetailResource
            var listSaleOrder = saleOrderShipModel.SaleOrderShipDetailCollection.Select(x => x.SaleOrderDetailResource);
            //Get SaleOrderDetail with list Sale OrderResource
            CollectionBase<base_SaleOrderDetailModel> listSaleOrderDetail = new CollectionBase<base_SaleOrderDetailModel>();
            //SelectedSaleOrder.SaleOrderDetailCollection.Where(x => listSaleOrder.Contains(x.Resource.ToString())).ToList();
            foreach (string resource in listSaleOrder)
            {
                base_SaleOrderDetailModel saleOrderDetailPacked = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(resource));
                if (saleOrderDetailPacked != null)
                {
                    //Check Has Parent => Add item parent 
                    if (!string.IsNullOrWhiteSpace(saleOrderDetailPacked.ParentResource))
                    {
                        base_SaleOrderDetailModel saleOrdeDetailParent = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetailPacked.ParentResource));
                        if (saleOrdeDetailParent != null && listSaleOrderDetail.Any(x => x.Resource.Equals(saleOrdeDetailParent.Resource)))
                            listSaleOrderDetail.Add(saleOrdeDetailParent);
                    }

                    //Add Children
                    listSaleOrderDetail.Add(saleOrderDetailPacked);
                }
            }


            PickPackViewModel pickPackViewModel = new PickPackViewModel(listSaleOrderDetail, saleOrderShipModel);
            bool? dialogResult = _dialogService.ShowDialog<PickPackView>(_ownerViewModel, pickPackViewModel, "Edit Pick Pack");
            if (dialogResult == true)
            {
                if (pickPackViewModel.SaleOrderShipModel != null)
                {
                    if (pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection != null && !pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection.Any())
                    {
                        SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                    }
                    else
                    {
                        //Update SaleOrderShip
                        saleOrderShipModel = pickPackViewModel.SaleOrderShipModel;
                    }
                }
                //Update SaleOrderDetail
                foreach (var item in pickPackViewModel.SaleOrderDetailList)
                {
                    base_SaleOrderDetailModel saleOrderDetailUpdate = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == item.Resource);
                    saleOrderDetailUpdate.PickQty = item.PickQty;
                }
                SelectedSaleOrder.RaiseTotalPackedBox();
            }

        }
        #endregion

        #region ViewPnPDetailCommand
        /// <summary>
        /// Gets the ViewPnPDetail Command.
        /// <summary>

        public RelayCommand<object> ViewPnPDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ViewPnPDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnViewPnPDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the ViewPnPDetail command is executed.
        /// </summary>
        private void OnViewPnPDetailCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            //Get list SaleOrderDetailResource
            var listSaleOrder = saleOrderShipModel.SaleOrderShipDetailCollection.Select(x => x.SaleOrderDetailResource);
            //Get SaleOrderDetail with list Sale OrderResource
            var listSaleOrderDetail = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => listSaleOrder.Contains(x.Resource.ToString())).ToList();

            PickPackViewModel pickPackViewModel = new PickPackViewModel(listSaleOrderDetail, saleOrderShipModel, true);
            bool? dialogResult = _dialogService.ShowDialog<PickPackView>(_ownerViewModel, pickPackViewModel, "View Pick Pack");
        }
        #endregion

        #region Shipped Command
        /// <summary>
        /// Gets the Shipped Command.
        /// <summary>

        public RelayCommand<object> ShippedCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Shipped command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShippedCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the Shipped command is executed.
        /// </summary>
        private void OnShippedCommandExecute(object param)
        {
            ShippedProcess(param);
        }


        #endregion

        #region PaymentCommand
        /// <summary>
        /// Gets the Payment Command.
        /// <summary>

        public RelayCommand<object> PaymentCommand { get; private set; }


        /// Method to check whether the Payment command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPaymentCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return IsOrderValid
                && !SelectedSaleOrder.IsNew
                && !SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull)
                && SelectedSaleOrder.SubTotal > 0;
        }

        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary> 
        private void OnPaymentCommandExecute(object param)
        {
            SaleOrderPayment();
            SelectedSaleOrder.RaiseTotalPaid();
            SelectedSaleOrder.PaymentProcess = SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any();
            ShowShipTab(SelectedSaleOrder);
        }
        #endregion

        #region EditProductCommand

        /// <summary>
        /// Gets the EditProduct Command.
        /// <summary>
        public RelayCommand<object> EditProductCommand { get; private set; }

        /// <summary>
        /// Method to check whether the EditProduct command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditProductCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return IsAllowChangeOrder;
        }

        /// <summary>
        /// Method to invoke when the EditProduct command is executed.
        /// </summary>
        private void OnEditProductCommandExecute(object param)
        {
            base_SaleOrderDetailModel saleOrderDetailModel = param as base_SaleOrderDetailModel;

            if (!saleOrderDetailModel.ProductModel.IsCoupon)
            {
                base_ProductModel productModel = new base_ProductModel();

                productModel.Resource = saleOrderDetailModel.ProductModel.Resource;
                productModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>(saleOrderDetailModel.ProductUOMCollection);
                productModel.BaseUOMId = saleOrderDetailModel.UOMId.Value;
                productModel.CurrentPrice = saleOrderDetailModel.SalePrice;
                productModel.RegularPrice = saleOrderDetailModel.RegularPrice;
                productModel.OnHandStore = saleOrderDetailModel.Quantity;
                productModel.ProductName = saleOrderDetailModel.ProductModel.ProductName;
                productModel.Attribute = saleOrderDetailModel.ProductModel.Attribute;
                productModel.Size = saleOrderDetailModel.ProductModel.Size;
                productModel.Description = saleOrderDetailModel.ProductModel.Description;
                productModel.IsOpenItem = saleOrderDetailModel.ProductModel.IsOpenItem;
                productModel.ItemTypeId = saleOrderDetailModel.ProductModel.ItemTypeId;

                //Edit Promotion when item is not product in group or product group
                bool isEditPromotion = true;
                if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))
                    isEditPromotion = false;
                else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))
                    isEditPromotion = false;

                PopupEditProductViewModel viewModel = new PopupEditProductViewModel(productModel, (PriceTypes)SelectedSaleOrder.PriceSchemaId, !saleOrderDetailModel.IsReadOnlyUOM, saleOrderDetailModel.PromotionId.Value, isEditPromotion);

                bool? result = _dialogService.ShowDialog<PopupEditProductView>(_ownerViewModel, viewModel, "Edit product");
                if (result.HasValue && result.Value)
                {
                    BreakSODetailChange = true;
                    //Set regular property
                    saleOrderDetailModel.UOMId = viewModel.SelectedProductUOM.UOMId;
                    SetPriceUOM(saleOrderDetailModel);
                    saleOrderDetailModel.UnitName = saleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(saleOrderDetailModel.UOMId)).Name;
                    saleOrderDetailModel.ItemName = productModel.ProductName;
                    saleOrderDetailModel.ItemAtribute = productModel.Attribute;
                    saleOrderDetailModel.ItemSize = productModel.Size;
                    saleOrderDetailModel.ProductModel.Description = productModel.Description;
                    saleOrderDetailModel.Quantity = productModel.OnHandStore;
                    saleOrderDetailModel.IsManual = viewModel.IsDiscountManual;



                    //Open Popup serial tracking 
                    if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.IsSerialTracking)
                        if (!saleOrderDetailModel.IsError && saleOrderDetailModel.Quantity > 0)
                            OpenTrackingSerialNumber(saleOrderDetailModel, true);
                        else
                            saleOrderDetailModel.SerialTracking = string.Empty;

                    if (isEditPromotion)
                    {
                        //Apply manual discount
                        if (saleOrderDetailModel.IsManual)
                        {
                            saleOrderDetailModel.SalePrice = productModel.CurrentPrice;
                            saleOrderDetailModel.RegularPrice = productModel.RegularPrice;
                            saleOrderDetailModel.SalePriceChanged();
                            CalculateMultiNPriceTax();
                            _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                            saleOrderDetailModel.PromotionId = 0;

                            BreakSODetailChange = true;
                            _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                            BreakSODetailChange = false;
                        }
                        else
                        {
                            base_PromotionModel promotionModel = new base_PromotionModel(_promotionRepository.Get(x => x.Id.Equals(viewModel.SelectedPromotion.Id)));
                            saleOrderDetailModel.PromotionId = promotionModel.Id;

                            BreakSODetailChange = true;
                            _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                            BreakSODetailChange = false;
                        }

                    }
                    else
                    {
                        _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                    }
                    CalculateMultiNPriceTax();
                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                    SelectedSaleOrder.CalcSubTotal();
                    BreakSODetailChange = false;
                }
            }
            else //EditCoupon
            {
                OpenCouponView(saleOrderDetailModel);
            }
        }

        #endregion

        #region LockOrderCommand
        /// <summary>
        /// Gets the LockOrder Command.
        /// <summary>

        public RelayCommand<object> LockOrderCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LockOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLockOrderCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return IsValid && IsShipValid & IsOrderValid & IsReturnValid;
        }


        /// <summary>
        /// Method to invoke when the LockOrder command is executed.
        /// </summary>
        private void OnLockOrderCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to Lock this item?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.No))
                return;

            SelectedSaleOrder.IsLocked = true;
            SetAllowChangeOrder(SelectedSaleOrder);
            SaveSalesOrder();
            SaleOrderCollection.Remove(SelectedSaleOrder);
            TotalSaleOrder -= 1;
            IsSearchMode = true;
        }
        #endregion

        //Main Grid
        #region DeleteItemsCommand
        /// <summary>
        /// Gets the DeleteItems Command.
        /// <summary>

        public RelayCommand<object> DeleteItemsCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteItems command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteItemsCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count > 0 && !datagrid.SelectedItems.Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Open));
        }


        /// <summary>
        /// Method to invoke when the DeleteItems command is executed.
        /// </summary>
        private void OnDeleteItemsCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            object collectionDelete = datagrid.SelectedItems.Cast<object>();
            DeleteItemsSaleOrder(collectionDelete);
        }



        /// <summary>
        /// Gets the DeleteItems Command.
        /// <summary>

        public RelayCommand<object> DeleteItemsWithKeyCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteItems command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteItemsWithKeyCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteItems command is executed.
        /// </summary>
        private void OnDeleteItemsWithKeyCommandExecute(object param)
        {
            if (param == null || (param as ObservableCollection<object>).Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Open)))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("delete these items is open", "POS", MessageBoxButton.OK);
                return;
            }

            DeleteItemsSaleOrder(param);
        }

        /// <summary>
        /// Confirm & delete saleorders
        /// </summary>
        /// <param name="param"></param>
        private void DeleteItemsSaleOrder(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete these items?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.No))
                return;

            foreach (base_SaleOrderModel saleOrderModel in (param as ObservableCollection<object>).Cast<base_SaleOrderModel>().ToList())
            {
                saleOrderModel.IsPurge = true;
                TotalSaleOrder -= 1;
                saleOrderModel.ToEntity();
                _saleOrderRepository.Commit();
                SaleOrderCollection.Remove(saleOrderModel);
            }

            if (SelectedSaleOrder != null)
                _selectedSaleOrder = null;
        }
        #endregion

        #region DuplicateItemCommand
        /// <summary>
        /// Gets the DuplicateItem Command.
        /// <summary>

        public RelayCommand<object> DuplicateItemCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DuplicateItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDuplicateItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count == 1;
        }


        /// <summary>
        /// Method to invoke when the DuplicateItem command is executed.
        /// </summary>
        private void OnDuplicateItemCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            base_SaleOrderModel saleOrderSource = datagrid.SelectedItem as base_SaleOrderModel;
            CreateNewSaleOrder();
            SelectedSaleOrder.CopyFrom(saleOrderSource);
            SelectedSaleOrder.CalcBalance();
            SetSaleOrderToModel(SelectedSaleOrder);
            //Check not set to collection
            if (saleOrderSource.SaleOrderDetailCollection == null && saleOrderSource.base_SaleOrder.base_SaleOrderDetail.Any())
                SetSaleOrderRelation(saleOrderSource);

            if (saleOrderSource.SaleOrderDetailCollection != null)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderSource.SaleOrderDetailCollection.Where(x => string.IsNullOrWhiteSpace(x.ParentResource)))
                {
                    string parentResource = CloneSaleOrderDetailModel(saleOrderDetailModel);
                    //Get Child item follow Resource parent
                    var childInGroup = saleOrderSource.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.Resource.ToString()));
                    if (childInGroup.Any())//Is a group 
                    {
                        foreach (base_SaleOrderDetailModel saleOrderDetaiInGrouplModel in childInGroup)
                        {
                            CloneSaleOrderDetailModel(saleOrderDetaiInGrouplModel, parentResource);
                        }
                    }
                }
            }

            SaveSalesOrder();

            _selectedTabIndex = (int)SaleOrderTab.Order;
            OnPropertyChanged(() => SelectedTabIndex);
            _selectedCustomer = null;
            //Set for selectedCustomer
            _flagCustomerSetRelate = false;
            SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));

            SetAllowChangeOrder(SelectedSaleOrder);
            SelectedSaleOrder.IsDirty = false;
            _flagCustomerSetRelate = true;

            IsSearchMode = false;
        }

        /// <summary>
        /// Clone & AddNew SaleOrderDetail
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        /// <returns></returns>
        private string CloneSaleOrderDetailModel(base_SaleOrderDetailModel saleOrderDetailModel, string parentResource = "")
        {
            base_SaleOrderDetailModel newSaleOrderDetailModel = new base_SaleOrderDetailModel();
            newSaleOrderDetailModel.Resource = Guid.NewGuid();
            newSaleOrderDetailModel.ParentResource = parentResource;
            newSaleOrderDetailModel.CopyFrom(saleOrderDetailModel);
            newSaleOrderDetailModel.CalcDueQty();

            //Set Item type Sale Order to know item is group/child or none
            if (newSaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                newSaleOrderDetailModel.ItemType = 1;
            else if (!string.IsNullOrWhiteSpace(newSaleOrderDetailModel.ParentResource))//Child item of group
                newSaleOrderDetailModel.ItemType = 2;
            else
                newSaleOrderDetailModel.ItemType = 0;

            SelectedSaleOrder.SaleOrderDetailCollection.Add(newSaleOrderDetailModel);
            _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, newSaleOrderDetailModel);
            return newSaleOrderDetailModel.Resource.ToString();
        }
        #endregion

        #region EditItemCommand
        /// <summary>
        /// Gets the EditItem Command.
        /// <summary>

        public RelayCommand<object> EditItemCommand { get; private set; }


        /// <summary>
        /// Method to check whether the EditItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the EditItem command is executed.
        /// </summary>
        private void OnEditItemCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            OnDoubleClickViewCommandExecute(datagrid.SelectedItem);
        }
        #endregion

        #region SaleOrderAdvanceSearchCommand
        /// <summary>
        /// Gets the SaleOrderAdvanceSearch Command.
        /// <summary>

        public RelayCommand<object> SaleOrderAdvanceSearchCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SaleOrderAdvanceSearch command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaleOrderAdvanceSearchCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SaleOrderAdvanceSearch command is executed.
        /// </summary>
        private void OnSaleOrderAdvanceSearchCommandExecute(object param)
        {
            OpenSOAdvanceSearch();
        }


        #endregion

        //Return
        #region ReturnAllCommand
        /// <summary>
        /// Gets the ReturnAll Command.
        /// <summary>

        public RelayCommand<object> ReturnAllCommand { get; private set; }

        ///Inital ReturnAll Command
        ///Need move to Constructor


        /// <summary>
        /// Method to check whether the ReturnAll command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnReturnAllCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null || SelectedSaleOrder.ReturnModel == null || SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.HasError))
                return false;
            return SelectedSaleOrder.SaleOrderShipCollection != null && SelectedSaleOrder.SaleOrderShipCollection.Any(x => x.IsShipped);
        }


        /// <summary>
        /// Method to invoke when the ReturnAll command is executed.
        /// </summary>
        private void OnReturnAllCommandExecute(object param)
        {
            ReturnAll();
        }


        #endregion

        #region DeleteReturnDetailCommand
        /// <summary>
        /// Gets the DeleteReturnDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteReturnDetailCommand { get; private set; }

        ///Inital DeleteReturnDetail Command
        ///Need move to Constructor


        /// <summary>
        /// Method to check whether the DeleteReturnDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteReturnDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param is base_ResourceReturnDetailModel) && !(param as base_ResourceReturnDetailModel).IsReturned;
        }


        /// <summary>
        /// Method to invoke when the DeleteReturnDetail command is executed.
        /// </summary>
        private void OnDeleteReturnDetailCommandExecute(object param)
        {
            base_ResourceReturnDetailModel returnDetailModel = param as base_ResourceReturnDetailModel;
            if (SelectedReturnDetail == null || returnDetailModel.IsTemporary)
                return;
            DeleteItemSaleOrderReturnDetail(returnDetailModel);
        }

        /// <summary>
        /// Gets the DeleteReturnDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteReturnDetailWithKeyCommand { get; private set; }

        ///Inital DeleteReturnDetail Command
        ///Need move to Constructor


        /// <summary>
        /// Method to check whether the DeleteReturnDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteReturnDetailWithKeyCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteReturnDetail command is executed.
        /// </summary>
        private void OnDeleteReturnDetailWithKeyCommandExecute(object param)
        {
            base_ResourceReturnDetailModel returnDetailModel = SelectedReturnDetail as base_ResourceReturnDetailModel;
            if (returnDetailModel == null || returnDetailModel.IsTemporary)
                return;

            if (returnDetailModel.IsReturned)
            {
                //Msg: Item has been returned, can not delete this item."
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_NotifyItemReturned"), Language.GetMsg("InformationCaption"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DeleteItemSaleOrderReturnDetail(returnDetailModel);
        }

        /// <summary>
        /// Delete SaleOrderReturn Detail
        /// </summary>
        /// <param name="returnDetailModel"></param>
        private void DeleteItemSaleOrderReturnDetail(base_ResourceReturnDetailModel returnDetailModel)
        {
            // Try to find ResourceReturnDetail error.
            base_ResourceReturnDetailModel resourceReturnDetailError = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.FirstOrDefault(x => x.HasError);
            bool isContainsErrorItem = false;
            if (resourceReturnDetailError != null)
            {
                ListCollectionView resourceReturnDetailView = CollectionViewSource.GetDefaultView(SelectedSaleOrder.ReturnModel.ReturnDetailCollection) as ListCollectionView;
                if (resourceReturnDetailView != null)
                {
                    if (resourceReturnDetailView.CurrentEditItem != null)
                        isContainsErrorItem = object.ReferenceEquals(resourceReturnDetailView.CurrentEditItem, returnDetailModel);
                    else if (resourceReturnDetailView.CurrentAddItem != null)
                        isContainsErrorItem = object.ReferenceEquals(resourceReturnDetailView.CurrentAddItem, returnDetailModel);
                    else
                        isContainsErrorItem = true;
                }
            }

            if (resourceReturnDetailError == null || isContainsErrorItem)
            {
                //msg:Do you want to delete item(s) ?
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Remove(returnDetailModel);
                    CalculateReturnSubtotal(SelectedSaleOrder);
                }
            }
        }
        #endregion

        #region SaleOrderRefunded Command
        /// <summary>
        /// Gets the SaleOrderRefunded Command.
        /// <summary>

        public RelayCommand<object> SaleOrderRefundedCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SaleOrderRefunded command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaleOrderRefundedCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Sum(x => x.TotalPaid - x.Change) > 0/*Has paid*/
                && SelectedSaleOrder.ReturnModel != null
                && SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned);//Has Return
        }


        /// <summary>
        /// Method to invoke when the SaleOrderRefunded command is executed.
        /// </summary>
        private void OnSaleOrderRefundedCommandExecute(object param)
        {
            RefundViewModel viewModel = new RefundViewModel(SelectedSaleOrder);
            bool? dialogResult = _dialogService.ShowDialog<RefundView>(_ownerViewModel, viewModel, "Sale Order Refund");
            if (dialogResult == true)
            {
                base_ResourcePaymentModel refundPaymentModel = viewModel.PaymentModel;
                if (Define.CONFIGURATION.DefaultCashiedUserName.HasValue && Define.CONFIGURATION.DefaultCashiedUserName.Value)
                    refundPaymentModel.Cashier = Define.USER.LoginName;
                refundPaymentModel.Shift = Define.ShiftCode;
                SelectedSaleOrder.PaymentCollection.Add(refundPaymentModel);
                SelectedSaleOrder.Paid = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value).Sum(x => x.TotalPaid);
                SelectedSaleOrder.ReturnModel.TotalRefund = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid < 0).Sum(x => x.TotalPaid);
            }
        }
        #endregion

        #region SerialTrackingCommand
        /// <summary>
        /// Gets the SerialTrackingDetail Command.
        /// <summary>

        public RelayCommand<object> SerialTrackingDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SerialTrackingDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSerialTrackingDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the SerialTrackingDetail command is executed.
        /// </summary>
        private void OnSerialTrackingDetailCommandExecute(object param)
        {
            base_SaleOrderDetailModel saleOrderDetailModel = param as base_SaleOrderDetailModel;
            OpenTrackingSerialNumber(saleOrderDetailModel, false, IsAllowChangeOrder);
        }
        #endregion

        #endregion "\Commands Methods"

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        private void InitialCommand()
        {
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            PrintCommand = new RelayCommand<object>(OnPrintCommandExecute, OnPrintCommandCanExecute);
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            AddressPopupCommand = new RelayCommand<object>(OnAddressPopupCommandExecute, OnAddressPopupCommandCanExecute);
            DeleteSaleOrderDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
            DeleteSaleOrderDetailWithKeyCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailWithKeyCommandExecute, OnDeleteSaleOrderDetailWithKeyCommandCanExecute);
            AddNewCustomerCommand = new RelayCommand<object>(OnAddNewCustomerCommandExecute, OnAddNewCustomerCommandCanExecute);
            QtyChangedCommand = new RelayCommand<object>(OnQtyChangedCommandExecute, OnQtyChangedCommandCanExecute);
            ManualChangePriceCommand = new RelayCommand<object>(OnManualChangePriceCommandExecute, OnManualChangePriceCommandCanExecute);
            LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            AddTermCommand = new RelayCommand(OnAddTermCommandExecute, OnAddTermCommandCanExecute);
            SearchProductAdvanceCommand = new RelayCommand<object>(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
            PickPackCommand = new RelayCommand<object>(OnPickPackCommandExecute, OnPickPackCommandCanExecute);
            DeleteSaleOrderShipCommand = new RelayCommand<object>(OnDeleteSaleOrderShipCommandExecute, OnDeleteSaleOrderShipCommandCanExecute);
            DeleteSaleOrderShipWithKeyCommand = new RelayCommand<object>(OnDeleteSaleOrderShipWithKeyCommandExecute, OnDeleteSaleOrderShipWithKeyCommandCanExecute);
            EditSaleOrderShipCommand = new RelayCommand<object>(OnEditSaleOrderShipCommandExecute, OnEditSaleOrderShipCommandCanExecute);
            ViewPnPDetailCommand = new RelayCommand<object>(OnViewPnPDetailCommandExecute, OnViewPnPDetailCommandCanExecute);
            ShippedCommand = new RelayCommand<object>(OnShippedCommandExecute, OnShippedCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);

            EditProductCommand = new RelayCommand<object>(OnEditProductCommandExecute, OnEditProductCommandCanExecute);
            LockOrderCommand = new RelayCommand<object>(OnLockOrderCommandExecute, OnLockOrderCommandCanExecute);
            //Using for Main Datagrid
            DeleteItemsCommand = new RelayCommand<object>(OnDeleteItemsCommandExecute, OnDeleteItemsCommandCanExecute);
            DeleteItemsWithKeyCommand = new RelayCommand<object>(OnDeleteItemsWithKeyCommandExecute, OnDeleteItemsWithKeyCommandCanExecute);
            DuplicateItemCommand = new RelayCommand<object>(OnDuplicateItemCommandExecute, OnDuplicateItemCommandCanExecute);
            EditItemCommand = new RelayCommand<object>(OnEditItemCommandExecute, OnEditItemCommandCanExecute);
            SaleOrderAdvanceSearchCommand = new RelayCommand<object>(OnSaleOrderAdvanceSearchCommandExecute, OnSaleOrderAdvanceSearchCommandCanExecute);

            //Return
            ReturnAllCommand = new RelayCommand<object>(OnReturnAllCommandExecute, OnReturnAllCommandCanExecute);
            DeleteReturnDetailCommand = new RelayCommand<object>(OnDeleteReturnDetailCommandExecute, OnDeleteReturnDetailCommandCanExecute);
            DeleteReturnDetailWithKeyCommand = new RelayCommand<object>(OnDeleteReturnDetailWithKeyCommandExecute, OnDeleteReturnDetailWithKeyCommandCanExecute);
            SaleOrderRefundedCommand = new RelayCommand<object>(OnSaleOrderRefundedCommandExecute, OnSaleOrderRefundedCommandCanExecute);

            //Quotation
            SerialTrackingDetailCommand = new RelayCommand<object>(OnSerialTrackingDetailCommandExecute, OnSerialTrackingDetailCommandCanExecute);
        }

        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                //Some data has changed. Do you want to save?
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M106"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {

                        result = SaveSalesOrder();
                    }
                    else //Has Error
                        result = false;
                }
                else
                {
                    if (SelectedSaleOrder.IsNew)
                    {
                        _selectedSaleOrder = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else //Old Item Rollback data
                    {
                        SelectedSaleOrder.ToModelAndRaise();
                        SetSaleOrderToModel(SelectedSaleOrder);
                        SetSaleOrderRelation(SelectedSaleOrder, true);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadStaticData()
        {
            this.BillAddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            BillAddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });

            ShipAddressTypeCollection = new AddressTypeCollection();
            ShipAddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });

            //Create Collection for filter customer with autocomplete
            CustomerFieldCollection = new DataSearchCollection();
            //CustomerFieldCollection.Add(new DataSearchModel { ID = 1, Level = 0, DisplayName = "All" });
            CustomerFieldCollection.Add(new DataSearchModel { ID = 1, Level = 0, DisplayName = "Customer Number", KeyName = "GuestNo" });
            CustomerFieldCollection.Add(new DataSearchModel { ID = 2, Level = 0, DisplayName = "LegalName", KeyName = "LegalName" });

            //Create collection for search products
            ProductFieldCollection = new DataSearchCollection();
            ProductFieldCollection.Add(new DataSearchModel { ID = 1, Level = 0, DisplayName = "Code", KeyName = "Code" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 2, Level = 0, DisplayName = "Barcode", KeyName = "Barcode" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 3, Level = 0, DisplayName = "Product Name", KeyName = "ProductName" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 4, Level = 0, DisplayName = "Attribute", KeyName = "Attribute" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 6, Level = 0, DisplayName = "Size", KeyName = "Size" });

            SaleOrderShipDetailFieldCollection = new DataSearchCollection
            {
                new DataSearchModel { ID = 1, Level = 0, DisplayName = "Code", KeyName = "ItemCode" },
                new DataSearchModel { ID = 2, Level = 0, DisplayName = "Product Name", KeyName = "ItemName" },
                new DataSearchModel { ID = 3, Level = 0, DisplayName = "Attribute", KeyName = "ItemAtribute" },
                new DataSearchModel { ID = 4, Level = 0, DisplayName = "Size", KeyName = "ItemSize" },
            };

        }

        /// <summary>
        /// Load relate data with form from database
        /// </summary>
        private void LoadDynamicData()
        {
            _promotionList = _promotionRepository.GetAll().OrderByDescending(x => x.DateUpdated).Select(x => new base_PromotionModel(x)
            {
                PromotionScheduleModel = new base_PromotionScheduleModel(x.base_PromotionSchedule.FirstOrDefault())
                {
                    ExpirationNoEndDate = !x.base_PromotionSchedule.FirstOrDefault().StartDate.HasValue
                }
            }).ToList();

            //Data using for binding

            //Load Customer
            LoadCustomer();

            //Get Employee
            LoadEmployee();

            //Get Store
            LoadStores();

            //Load AllProduct
            LoadProducts();

            //Load All Sale Tax
            LoadSaleTax();
        }

        /// <summary>
        /// Load Product From Database
        /// </summary>
        private void LoadProducts()
        {
            IList<base_Product> products;

            if (Define.StoreCode == 0)
                products = _productRepository.GetAll(x => !x.IsPurge.Value);
            else
                products = _productRepository.GetAll(x => !x.IsPurge.Value && x.base_ProductStore.Any(y => y.StoreCode.Equals(Define.StoreCode)));

            if (ProductCollection == null)
                ProductCollection = new ObservableCollection<base_ProductModel>(products.Select(x => new base_ProductModel(x)).OrderBy(x => x.Id));
            else
            {
                foreach (base_Product product in products.OrderBy(x => x.Id))
                {
                    if (ProductCollection.Any(x => x.Resource.Equals(product.Resource)))
                    {
                        base_ProductModel productModel = ProductCollection.SingleOrDefault(x => x.Resource.Equals(product.Resource));
                        productModel.UpdateModel(product, true);
                    }
                    else
                    {
                        ProductCollection.Add(new base_ProductModel(product));
                    }
                }

                //Remove Item From Local collection if in db collection is not existed
                IList<Guid> itemReomoveList = ProductCollection.Where(x => !x.IsCoupon).Select(x => x.Resource).Except(products.Select(x => x.Resource)).ToList();
                if (itemReomoveList != null)
                {
                    foreach (Guid resource in itemReomoveList)
                    {
                        base_ProductModel itemRemoved = ProductCollection.SingleOrDefault(x => x.Resource.Equals(resource));
                        ProductCollection.Remove(itemRemoved);
                    }
                }
            }

            if (!ProductCollection.Any(x => x.IsCoupon))
            {
                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(64))
                {
                    //Get PaymentCard with config
                    foreach (ComboItem paymentCard in Common.GiftCardTypes)
                    {
                        if (Define.CONFIGURATION.AcceptedGiftCardMethod.Has((int)paymentCard.Value))
                        {
                            base_ProductModel couponItem = new base_ProductModel();
                            couponItem.ProductName = paymentCard.Text;
                            string strGuidPatern = "{0}{0}{0}{0}{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                            couponItem.Code = string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", Convert.ToInt32(paymentCard.ObjValue));
                            string guidID = string.Format(strGuidPatern, Convert.ToInt32(paymentCard.ObjValue));
                            couponItem.Resource = Guid.Parse(guidID);
                            couponItem.IsOpenItem = true;
                            couponItem.IsCoupon = true;
                            ProductCollection.Add(couponItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load SaleTaxCollection
        /// </summary>
        private void LoadSaleTax()
        {
            IList<base_SaleTaxLocation> saleTaxList = _saleTaxRepository.GetAll();
            if (SaleTaxLocationCollection == null)
                SaleTaxLocationCollection = new List<base_SaleTaxLocationModel>(saleTaxList.Select(x => new base_SaleTaxLocationModel(x)));
            else
            {
                foreach (base_SaleTaxLocation saleTax in saleTaxList)
                {
                    if (SaleTaxLocationCollection.Any(x => x.Id.Equals(saleTax.Id)))
                    {
                        base_SaleTaxLocationModel saleTaxModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id.Equals(saleTax.Id));
                        saleTaxModel.UpdateModel(saleTax);
                        saleTaxModel.EndUpdate();
                    }
                    else
                    {
                        SaleTaxLocationCollection.Add(new base_SaleTaxLocationModel(saleTax));
                    }
                }

                //Remove Item From Local collection if in db collection is not existed
                IList<int> itemReomoveList = SaleTaxLocationCollection.Select(x => x.Id).Except(saleTaxList.Select(x => x.Id)).ToList();
                if (itemReomoveList != null)
                {
                    SaleTaxLocationCollection.RemoveAll(x => itemReomoveList.Contains(x.Id));
                }
            }
        }

        /// <summary>
        /// Load Store from db
        /// </summary>
        private void LoadStores()
        {
            IList<base_Store> stores = _storeRepository.GetAll();
            if (StoreCollection == null)
                StoreCollection = new ObservableCollection<base_Store>(stores.OrderBy(x => x.Id));
        }

        /// <summary>
        /// Load All Employee From Db 
        /// </summary>
        private void LoadEmployee()
        {
            IList<base_Guest> employeeList = _guestRepository.GetAll(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived);

            if (EmployeeCollection == null)
                EmployeeCollection = new CollectionBase<base_GuestModel>(employeeList.OrderBy(x => x.Id).Select(x => new base_GuestModel(x)));
            else
            {
                foreach (base_Guest employee in employeeList)
                {
                    if (EmployeeCollection.Any(x => x.Resource.Equals(employee.Resource)))
                    {
                        base_GuestModel employeeModel = EmployeeCollection.SingleOrDefault(x => x.Resource.Equals(employee.Resource));
                        employeeModel.UpdateModel(employee);
                        employeeModel.EndUpdate();
                    }
                    else
                    {
                        EmployeeCollection.Add(new base_GuestModel(employee));
                    }
                }

                //Remove Item From Local collection if in db collection is not existed
                IList<Guid?> itemReomoveList = EmployeeCollection.Where(x => x.Id != 0).Select(x => x.Resource).Except(employeeList.Select(x => x.Resource)).ToList();
                if (itemReomoveList != null)
                {
                    foreach (Guid resource in itemReomoveList)
                    {
                        base_GuestModel itemRemoved = EmployeeCollection.SingleOrDefault(x => x.Resource.Equals(resource));
                        EmployeeCollection.Remove(itemRemoved);
                    }
                }
            }

            if (!EmployeeCollection.Any(x => x.Id == 0))
                EmployeeCollection.Insert(0, new base_GuestModel() { Id = 0 });
        }

        /// <summary>
        /// Load All Customer From DB
        /// </summary>
        private void LoadCustomer()
        {
            IList<base_Guest> customerList = _guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && !x.IsPurged && x.IsActived);

            if (CustomerCollection == null)
                CustomerCollection = new CollectionBase<base_GuestModel>(customerList.OrderBy(x => x.Id).Select(x => new base_GuestModel(x)));
            else
            {
                foreach (base_Guest customer in customerList)
                {
                    //Check Item is existed,update model for item
                    if (CustomerCollection.Any(x => x.Resource.Equals(customer.Resource)))
                    {
                        base_GuestModel customerModel = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(customer.Resource));
                        customerModel.UpdateModel(customer);
                        customerModel.EndUpdate();
                    }
                    else //Add new item
                    {
                        CustomerCollection.Add(new base_GuestModel(customer));
                    }
                }
                //Remove Item From Local collection if in db collection is not existed
                IList<Guid?> itemReomoveList = CustomerCollection.Select(x => x.Resource).Except(customerList.Select(x => x.Resource)).ToList();
                if (itemReomoveList != null)
                {
                    foreach (Guid resource in itemReomoveList)
                    {
                        base_GuestModel itemRemoved = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(resource));
                        CustomerCollection.Remove(itemRemoved);
                    }
                }
            }
        }

        private base_RewardManager GetReward(DateTime orderDate)
        {
            var reward = _rewardManagerRepository.Get(x =>
                                        x.IsActived
                                        && ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                        || !x.IsTrackingPeriod)));
            return reward;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            BreakAllChange = true;

            //Set SaleOrderStatus
            saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
            //Set Price Schema
            saleOrderModel.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.PriceSchemaId));

            //Get CustomerModel & relation with customer
            if (CustomerCollection.Any(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)))
            {
                saleOrderModel.GuestModel = CustomerCollection.Where(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)).FirstOrDefault();

                _saleOrderRepository.SetGuestAdditionalModel(saleOrderModel);

                DateTime orderDate = saleOrderModel.OrderDate.Value.Date;
                //Ingore saleOrderModel.SubTotal >= x.PurchaseThreshold to show require reward
                var reward = GetReward(orderDate);
                saleOrderModel.IsApplyReward = saleOrderModel.GuestModel.IsRewardMember;
                saleOrderModel.IsApplyReward &= reward != null ? true : false;

                if (saleOrderModel.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                    saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

            }

            //Get SaleTax
            GetSaleTax(saleOrderModel);

            //Using for Calc Tax for Shipping if Tax is Price
            if (saleOrderModel.TaxLocationModel != null
                && saleOrderModel.TaxLocationModel.TaxCodeModel != null
                && saleOrderModel.TaxLocationModel.IsShipingTaxable)
            {
                saleOrderModel.ShipTaxAmount = _saleOrderRepository.CalcShipTaxAmount(saleOrderModel);
                saleOrderModel.ProductTaxAmount = saleOrderModel.TaxAmount - saleOrderModel.ShipTaxAmount;
            }
            else
                saleOrderModel.ProductTaxAmount = saleOrderModel.TaxAmount;

            //Check Deposit is accepted?
            saleOrderModel.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit

            //Set Address
            _saleOrderRepository.SetBillShipAddress(saleOrderModel.GuestModel, saleOrderModel);
            saleOrderModel.RaiseAnyShipped();
            saleOrderModel.SetFullPayment();
            BreakAllChange = false;
            saleOrderModel.IsDirty = false;
        }

        /// <summary>
        /// Load SetSaleOrderDetail, SetForSaleOrderShip, SetForShippedCollection,SetToSaleOrderReturn,LoadPaymentCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            SetSaleOrderDetail(saleOrderModel, isForce);

            SetCustomerRewardCollection(saleOrderModel, isForce);

            SetForSaleOrderShip(saleOrderModel, isForce);

            //Get SaleOrderShipDetail for return
            SetForShippedCollection(saleOrderModel, isForce);

            SetToSaleOrderReturn(saleOrderModel, isForce);

            LoadPaymentCollection(saleOrderModel);

            saleOrderModel.RaiseAnyShipped();
        }

        /// <summary>
        /// Load Sale Order Detail Collection with SaleOrderDetailCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce">Can set SaleOrderDetailCollection when difference null</param>
        private void SetSaleOrderDetail(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Load sale order detail
            if (isForce || saleOrderModel.SaleOrderDetailCollection == null || !saleOrderModel.SaleOrderDetailCollection.Any())
            {
                saleOrderModel.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
                saleOrderModel.SaleOrderDetailCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(SaleOrderDetailCollection_CollectionChanged);
                _saleOrderDetailRepository.Refresh(saleOrderModel.base_SaleOrder.base_SaleOrderDetail);
                foreach (base_SaleOrderDetail saleOrderDetail in saleOrderModel.base_SaleOrder.base_SaleOrderDetail.OrderBy(x => x.Id))
                {
                    base_SaleOrderDetailModel saleOrderDetailModel = new base_SaleOrderDetailModel(saleOrderDetail);
                    saleOrderDetailModel.Qty = saleOrderDetailModel.Quantity;
                    saleOrderDetailModel.ProductModel = ProductCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetail.ProductResource));

                    if (saleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        saleOrderDetailModel.IsQuantityAccepted = true;
                        saleOrderDetailModel.IsReadOnlyUOM = true;
                    }
                    else
                    {
                        //Get VendorName
                        base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == saleOrderDetailModel.ProductModel.VendorId));
                        if (vendorModel != null)
                            saleOrderDetailModel.ProductModel.VendorName = vendorModel.LegalName;

                        base_ProductGroup productGroupItem = null;
                        if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))
                        {
                            productGroupItem = saleOrderDetailModel.ProductModel.base_Product.base_ProductGroup.SingleOrDefault(x => x.ProductParentId.Equals(saleOrderDetailModel.ProductParentId));
                            saleOrderDetailModel.ProductGroupItem = productGroupItem;
                        }

                        saleOrderDetailModel.UOMId = -1;//Set UOM -1 because UOMCollection is Empty => UOMId not raise change after UOMCollection created
                        _saleOrderRepository.GetProductUOMforSaleOrderDetail(saleOrderDetailModel, false);
                        saleOrderDetailModel.UOMId = saleOrderDetail.UOMId;
                        base_ProductUOMModel unitItem = saleOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == saleOrderDetailModel.UOMId);

                        saleOrderDetailModel.UnitName = unitItem != null ? unitItem.Name : string.Empty;
                    }

                    saleOrderDetailModel.SalePriceChange = saleOrderDetailModel.SalePrice;
                    // -0.01 because 0.65 round to 0.6, 0.66 round to 0.7, 0.64 round to 0.6.
                    saleOrderDetailModel.UnitDiscount = Math.Round(Math.Round(saleOrderDetailModel.RegularPrice * saleOrderDetailModel.DiscountPercent / 100, 2) - 0.01M, MidpointRounding.AwayFromZero);
                    saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.SalePrice;
                    saleOrderDetailModel.TotalDiscount = Math.Round(Math.Round(saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity, 2), MidpointRounding.AwayFromZero);

                    //Set Item type Sale Order to know item is group/child or none
                    if (saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                        saleOrderDetailModel.ItemType = 1;
                    else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))//Child item of group
                        saleOrderDetailModel.ItemType = 2;
                    else
                        saleOrderDetailModel.ItemType = 0;

                    //Check RowDetail Visibility
                    _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    saleOrderDetailModel.IsQuantityAccepted = true;
                    saleOrderDetailModel.IsDirty = false;
                    saleOrderModel.SaleOrderDetailCollection.Add(saleOrderDetailModel);

                    _saleOrderRepository.CalcOnHandStore(saleOrderModel, saleOrderDetailModel);
                }
                if (saleOrderModel.SaleOrderDetailCollection != null)
                    saleOrderModel.IsHiddenErrorColumn = !saleOrderModel.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);

                ShowShipTab(saleOrderModel);
            }
        }

        /// <summary>
        /// Load Sale Order Shippeds Collection with SaleOrderShipDetailCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce">Can set SaleOrderShipDetailCollection when difference null</param>
        private void SetForShippedCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            if ((saleOrderModel.SaleOrderShipDetailCollection == null && saleOrderModel.SaleOrderShipCollection != null) || isForce)
            {
                saleOrderModel.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
                foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection.Where(x => x.IsShipped == true))
                {
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        saleOrderModel.SaleOrderShipDetailCollection.Add(saleOrderShipDetailModel);
                    }
                }

                saleOrderModel.SaleOrderShippedCollection = new CollectionBase<base_SaleOrderDetailModel>();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    //Item is shipped
                    if (saleOrderModel.SaleOrderShipDetailCollection.Any(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())))
                    {
                        //Item is shipped => lock Uom this item
                        if (!saleOrderDetailModel.IsReadOnlyUOM)
                            saleOrderDetailModel.IsReadOnlyUOM = true;

                        base_SaleOrderDetailModel saleOrderShipModel = saleOrderDetailModel.Clone();
                        saleOrderShipModel.IsNew = false;
                        saleOrderShipModel.PickQty = saleOrderModel.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty);
                        saleOrderShipModel.SubTotal = saleOrderShipModel.PickQty * saleOrderShipModel.SalePrice;
                        saleOrderModel.SaleOrderShippedCollection.Add(saleOrderShipModel);
                    }
                }
            }
        }

        /// <summary>
        /// Load Sale Order Return
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetToSaleOrderReturn(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Get Return Resource
            string saleOrderResource = saleOrderModel.Resource.ToString();
            base_ResourceReturn resourceReturn = _resourceReturnRepository.Get(x => x.DocumentResource.Equals(saleOrderResource));

            if (resourceReturn != null)
                saleOrderModel.ReturnModel = new base_ResourceReturnModel(resourceReturn);
            else
            {
                saleOrderModel.ReturnModel = new base_ResourceReturnModel();
                saleOrderModel.ReturnModel.DocumentNo = saleOrderModel.SONumber;
                saleOrderModel.ReturnModel.TotalAmount = saleOrderModel.Total;
                saleOrderModel.ReturnModel.DocumentResource = saleOrderModel.Resource.ToString();
                saleOrderModel.ReturnModel.Resource = Guid.NewGuid();
                saleOrderModel.ReturnModel.TotalRefund = 0;
                saleOrderModel.ReturnModel.Mark = "SO";
                saleOrderModel.ReturnModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                saleOrderModel.ReturnModel.DateCreated = DateTime.Today;
                saleOrderModel.ReturnModel.IsDirty = false;
            }
            if (isForce || saleOrderModel.ReturnModel.ReturnDetailCollection == null || !saleOrderModel.ReturnModel.ReturnDetailCollection.Any())
            {
                saleOrderModel.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
                saleOrderModel.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;
                foreach (base_ResourceReturnDetail resourceReturnDetail in saleOrderModel.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail)
                {
                    base_ResourceReturnDetailModel returnDetailModel = new base_ResourceReturnDetailModel(resourceReturnDetail);
                    returnDetailModel.SaleOrderModel = saleOrderModel;
                    returnDetailModel.SaleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource));
                    returnDetailModel.UnitName = returnDetailModel.SaleOrderDetailModel.UnitName;
                    CalcReturnDetailSubTotal(saleOrderModel, returnDetailModel);
                    saleOrderModel.ReturnModel.ReturnDetailCollection.Add(returnDetailModel);
                    returnDetailModel.IsDirty = false;
                    returnDetailModel.IsTemporary = false;
                }
            }
            saleOrderModel.ReturnModel.PropertyChanged += new PropertyChangedEventHandler(ReturnModel_PropertyChanged);
        }

        /// <summary>
        /// Load Sale Order Ship Collection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetForSaleOrderShip(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Collection Sale Order Ship
            if (isForce || saleOrderModel.SaleOrderShipCollection == null || !saleOrderModel.SaleOrderShipCollection.Any())
            {
                saleOrderModel.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();

                foreach (base_SaleOrderShip saleOrderShip in saleOrderModel.base_SaleOrder.base_SaleOrderShip)
                {
                    base_SaleOrderShipModel saleOrderShipModel = new base_SaleOrderShipModel(saleOrderShip);
                    saleOrderShipModel.IsChecked = saleOrderShipModel.IsShipped;
                    saleOrderShipModel.IsDirty = false;
                    //SaleOrderShipDetail
                    saleOrderShipModel.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
                    foreach (base_SaleOrderShipDetail saleOrderShipDetail in saleOrderShip.base_SaleOrderShipDetail)
                    {
                        base_SaleOrderShipDetailModel saleOrderShipDetailModel = new base_SaleOrderShipDetailModel(saleOrderShipDetail);
                        saleOrderShipDetailModel.SaleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetail.SaleOrderDetailResource));
                        saleOrderShipDetailModel.IsDirty = false;
                        saleOrderShipModel.SaleOrderShipDetailCollection.Add(saleOrderShipDetailModel);
                    }
                    saleOrderModel.SaleOrderShipCollection.Add(saleOrderShipModel);
                }

                saleOrderModel.PaymentProcess = saleOrderModel.SaleOrderShipCollection.Any();
            }
        }

        /// <summary>
        /// Set CustomerRewardCollection for RewardMember
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetCustomerRewardCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Get GuestReward collection
            if (isForce || saleOrderModel.GuestModel.GuestRewardCollection == null || !saleOrderModel.GuestModel.GuestRewardCollection.Any())
            {
                saleOrderModel.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    foreach (base_GuestReward guestReward in saleOrderModel.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(saleOrderModel.GuestModel.Id) && !x.IsApply && x.ActivedDate.Value <= DateTime.Today && (!x.ExpireDate.HasValue || x.ExpireDate.HasValue && DateTime.Today <= x.ExpireDate.Value)))
                    {
                        saleOrderModel.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestReward));
                    }
                }
            }
        }

        /// <summary>
        /// Load payment collection 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void LoadPaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            // Get document resource
            string docResource = saleOrderModel.Resource.ToString();

            // Get all payment by document resource
            IEnumerable<base_ResourcePayment> payments = _paymentRepository.GetAll(x => x.DocumentResource.Equals(docResource));

            // Load payment collection
            saleOrderModel.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>(payments.Select(x => new base_ResourcePaymentModel(x)));

            // Check show PaymentTab
            saleOrderModel.PaymentProcess = saleOrderModel.PaymentCollection.Any();
        }

        /// <summary>
        /// Load Tax for SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void GetSaleTax(base_SaleOrderModel saleOrderModel)
        {
            saleOrderModel.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == saleOrderModel.TaxLocation);
            //Get Tax Code
            saleOrderModel.TaxLocationModel.TaxCodeModel = SaleTaxLocationCollection.SingleOrDefault(x => x.ParentId == saleOrderModel.TaxLocationModel.Id && x.TaxCode.Equals(saleOrderModel.TaxCode));
            if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(saleOrderModel.TaxLocationModel.TaxCodeModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex"></param>
        private void LoadDataByPredicate(Expression<Func<base_SaleOrder, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy)//Break multi call to server
            {
                Console.WriteLine("IsBusy");
                return;
            }

            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                SaleOrderCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;
                predicate = predicate.And(x => !x.IsPurge && !x.IsLocked);

                short orderStatus = (short)SaleOrderStatus.Quote;
                short layawayStatus = (short)SaleOrderStatus.Layaway;
                //Show item is created by SaleOrder or isConverted from Quote,Layaway,WorkOrder
                predicate = predicate.And(x => x.OrderStatus != orderStatus && x.OrderStatus != layawayStatus && x.IsConverted);

                //Cout all SaleOrder in Data base show on grid
                lock (UnitOfWork.Locker)
                {
                    TotalSaleOrder = _saleOrderRepository.GetIQueryable(predicate).Count();

                    //Get data with range
                    IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetRange<DateTime>(currentIndex, NumberOfDisplayItems, x => x.OrderDate.Value, predicate);

                    if (refreshData)
                        _saleOrderRepository.Refresh(saleOrders);

                    foreach (base_SaleOrder saleOrder in saleOrders)
                    {
                        bgWorker.ReportProgress(0, saleOrder);
                    }
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_SaleOrderModel saleOrderModel = new base_SaleOrderModel((base_SaleOrder)e.UserState);
                SetSaleOrderToModel(saleOrderModel);
                SaleOrderCollection.Add(saleOrderModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (SaleOrderId > 0)
                {
                    SetSelectedSaleOrderFromAnother();
                }
                else
                {
                    if (_viewExisted && !IsSearchMode && SelectedSaleOrder != null && SaleOrderCollection.Any() && !SelectedSaleOrder.IsNew) //Item is selected
                    {
                        SetSelectedSaleOrderFromDbOrCollection();
                    }
                }

                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_SaleOrder, bool>> CreatePredicateWithConditionSearch(string keyword)
        {
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.SoNum))
                {
                    predicate = predicate.And(x => x.SONumber.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Customer))
                {
                    var customerList = CustomerCollection.Where(y => y.LastName.ToLower().Contains(keyword.ToLower()) || y.FirstName.ToLower().Contains(keyword.ToLower())).Select(x => x.Resource.ToString());
                    predicate = predicate.And(x => customerList.Contains(x.CustomerResource));
                }
            }
            return predicate;
        }

        /// <summary>
        /// 
        /// </summary>
        private base_SaleOrderModel CreateNewSaleOrder()
        {
            _selectedSaleOrder = new base_SaleOrderModel();
            _selectedSaleOrder.Shift = Define.ShiftCode;
            _selectedSaleOrder.IsTaxExemption = false;
            _selectedSaleOrder.IsConverted = true;
            _selectedSaleOrder.IsLocked = false;
            _selectedSaleOrder.SONumber = DateTime.Now.ToString(Define.GuestNoFormat);
            _saleOrderRepository.SOCardGenerate(_selectedSaleOrder, _selectedSaleOrder.SONumber);
            _selectedSaleOrder.DateCreated = DateTime.Now;
            _selectedSaleOrder.BookingChanel = Convert.ToInt16(Common.BookingChannel.First().ObjValue);
            _selectedSaleOrder.StoreCode = Define.StoreCode;//Default StoreCode
            _selectedSaleOrder.OrderDate = DateTime.Now;
            _selectedSaleOrder.RequestShipDate = DateTime.Now;
            _selectedSaleOrder.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            _selectedSaleOrder.TaxPercent = 0;
            _selectedSaleOrder.TaxAmount = 0;
            _selectedSaleOrder.Deposit = 0;
            _selectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
            _selectedSaleOrder.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(_selectedSaleOrder.OrderStatus));
            _selectedSaleOrder.Mark = MarkType.SaleOrder.ToDescription();
            _selectedSaleOrder.TermNetDue = 0;
            _selectedSaleOrder.TermDiscountPercent = 0;
            _selectedSaleOrder.TermPaidWithinDay = 0;
            _selectedSaleOrder.PaymentTermDescription = string.Empty;
            //Set Price Schema
            _selectedSaleOrder.PriceSchemaId = 1;
            _selectedSaleOrder.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(_selectedSaleOrder.PriceSchemaId));

            _selectedSaleOrder.TaxExemption = string.Empty;
            _selectedSaleOrder.SaleRep = EmployeeCollection.FirstOrDefault().GuestNo;
            _selectedSaleOrder.Resource = Guid.NewGuid();
            _selectedSaleOrder.WeightUnit = Common.ShipUnits.First().Value;
            _selectedSaleOrder.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit
            _selectedSaleOrder.WeightUnit = Define.CONFIGURATION.DefaultShipUnit.HasValue ? Define.CONFIGURATION.DefaultShipUnit.Value : Convert.ToInt16(Common.ShipUnits.First().ObjValue);
            _selectedSaleOrder.IsHiddenErrorColumn = true;

            _selectedSaleOrder.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
            _selectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
            //Get TaxLocation
            _selectedSaleOrder.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == _selectedSaleOrder.TaxLocation);

            //Create a sale order detail collection
            _selectedSaleOrder.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
            _selectedSaleOrder.SaleOrderDetailCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(SaleOrderDetailCollection_CollectionChanged);

            //create a sale order Ship Collection
            _selectedSaleOrder.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();
            _selectedSaleOrder.SaleOrderShippedCollection = new CollectionBase<base_SaleOrderDetailModel>();

            // Create new payment collection
            _selectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

            //ReturnModel & ReturnDetailCollection
            _selectedSaleOrder.ReturnModel = new base_ResourceReturnModel();
            _selectedSaleOrder.ReturnModel.DocumentNo = SelectedSaleOrder.SONumber;
            _selectedSaleOrder.ReturnModel.DocumentResource = SelectedSaleOrder.Resource.ToString();
            _selectedSaleOrder.ReturnModel.TotalAmount = SelectedSaleOrder.Total;
            _selectedSaleOrder.ReturnModel.Resource = Guid.NewGuid();
            _selectedSaleOrder.ReturnModel.TotalRefund = 0;
            _selectedSaleOrder.ReturnModel.TotalAmount = 0;
            _selectedSaleOrder.ReturnModel.Mark = "SO";
            _selectedSaleOrder.ReturnModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            _selectedSaleOrder.ReturnModel.DateCreated = DateTime.Today;
            _selectedSaleOrder.ReturnModel.IsDirty = false;
            _selectedSaleOrder.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
            _selectedSaleOrder.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;
            _selectedSaleOrder.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
            //Additional
            _selectedSaleOrder.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing, IsDirty = false };
            _selectedSaleOrder.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping, IsDirty = false };
            SelectedCustomer = null;
            //Set to fist tab & skip TabChanged Methods in SelectedTabIndex property
            _selectedTabIndex = 0;
            OnPropertyChanged(() => SelectedTabIndex);
            SetAllowChangeOrder(_selectedSaleOrder);
            _selectedSaleOrder.IsDirty = false;
            _selectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
            _selectedSaleOrder.ReturnModel.PropertyChanged += new PropertyChangedEventHandler(ReturnModel_PropertyChanged);
            OnPropertyChanged(() => SelectedSaleOrder);
            OnPropertyChanged(() => AllowSOShipping);
            OnPropertyChanged(() => AllowSOReturn);
            return _selectedSaleOrder;
        }

        /// <summary>
        /// add new sale Order Detail & Get UOM
        /// </summary>
        /// <returns></returns>
        private base_SaleOrderDetailModel AddNewSaleOrderDetail(base_ProductModel productModel, string ParentResource = "", base_ProductModel productParentItem = null)
        {
            base_SaleOrderDetailModel salesOrderDetailModel = new base_SaleOrderDetailModel();
            salesOrderDetailModel.Resource = Guid.NewGuid();
            base_ProductGroup productGroupItem = null;
            //Get Product GroupItem
            if (productParentItem != null)
            {
                productGroupItem = productModel.base_Product.base_ProductGroup.SingleOrDefault(x => x.ProductParentId.Equals(productParentItem.Id));
                salesOrderDetailModel.ProductGroupItem = productGroupItem;
                salesOrderDetailModel.ProductParentId = productParentItem.Id;
            }

            salesOrderDetailModel.Quantity = productGroupItem == null ? 1 : productGroupItem.Quantity;//Set Quantity Default of ProductInGroup
            salesOrderDetailModel.PromotionId = 0;
            salesOrderDetailModel.SerialTracking = string.Empty;
            salesOrderDetailModel.TaxCode = productModel.TaxCode;
            salesOrderDetailModel.ItemCode = productModel.Code;
            salesOrderDetailModel.ItemName = productModel.ProductName;
            salesOrderDetailModel.ProductResource = productModel.Resource.ToString();
            salesOrderDetailModel.OnHandQty = _saleOrderRepository.GetUpdateProduct(SelectedSaleOrder.StoreCode, productModel);
            salesOrderDetailModel.ItemAtribute = productModel.Attribute;
            salesOrderDetailModel.ItemSize = productModel.Size;
            salesOrderDetailModel.ProductModel = productModel;
            salesOrderDetailModel.ParentResource = ParentResource;

            //Set Item type Sale Order to know item is group/child or none
            if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                salesOrderDetailModel.ItemType = 1;
            else if (!string.IsNullOrWhiteSpace(salesOrderDetailModel.ParentResource))//Child item of group
                salesOrderDetailModel.ItemType = 2;
            else
                salesOrderDetailModel.ItemType = 0;

            salesOrderDetailModel.CalcSubTotal();
            SelectedSaleOrder.SaleOrderDetailCollection.Add(salesOrderDetailModel);
            return salesOrderDetailModel;
        }

        /// <summary>
        /// Create new SaleOrderDetail from the same SaleOrderDetail
        /// </summary>
        /// <param name="saleOrderdetail"></param>
        /// <returns></returns>
        private base_SaleOrderDetailModel NewSaleOrderDetail(base_SaleOrderDetailModel saleOrderdetail)
        {
            base_SaleOrderDetailModel newSaleOrderDetail = new base_SaleOrderDetailModel();
            newSaleOrderDetail.Resource = Guid.NewGuid();
            newSaleOrderDetail.ProductModel = saleOrderdetail.ProductModel;
            newSaleOrderDetail.ProductResource = saleOrderdetail.ProductResource;
            newSaleOrderDetail.ItemSize = saleOrderdetail.ItemSize;
            newSaleOrderDetail.ItemAtribute = saleOrderdetail.ItemAtribute;
            newSaleOrderDetail.ItemCode = saleOrderdetail.ItemCode;
            newSaleOrderDetail.ItemName = saleOrderdetail.ItemName;
            newSaleOrderDetail.SerialTracking = saleOrderdetail.SerialTracking;
            //SEt UOM
            newSaleOrderDetail.UOMId = saleOrderdetail.UOMId;
            newSaleOrderDetail.UnitName = saleOrderdetail.UnitName;
            newSaleOrderDetail.UOM = saleOrderdetail.UOM;
            newSaleOrderDetail.PromotionId = saleOrderdetail.PromotionId;
            newSaleOrderDetail.RegularPrice = saleOrderdetail.RegularPrice;
            newSaleOrderDetail.OnHandQty = saleOrderdetail.OnHandQty;
            newSaleOrderDetail.SalePrice = saleOrderdetail.SalePrice;
            newSaleOrderDetail.ProductUOMCollection = saleOrderdetail.ProductUOMCollection;
            newSaleOrderDetail.Quantity = 1;
            newSaleOrderDetail.CalcSubTotal();
            return newSaleOrderDetail;
        }

        /// <summary>
        /// Search product with advance options..
        /// </summary>
        private void SearchProductAdvance()
        {
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel();
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, "Search Product");
            if (dialogResult == true)
            {
                CreateSaleOrderDetailWithProducts(productSearchViewModel.SelectedProducts);
            }
        }

        /// <summary>
        /// Get SelectedSaleOrder From collection when Convert from quotation
        /// </summary>
        private void SetSelectedSaleOrderFromAnother()
        {
            if (SaleOrderId > 0)
            {
                SetSelectedSaleOrderFromDbOrCollection();


                //Calc Onhand
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    if (!saleOrderDetailModel.IsQuantityAccepted)
                    {
                        saleOrderDetailModel.IsQuantityAccepted = true;
                        _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                    }
                }

                //Set for selectedCustomer
                _flagCustomerSetRelate = false;
                SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));
                SetAllowChangeOrder(SelectedSaleOrder);
                ShowShipTab(SelectedSaleOrder);
                SelectedSaleOrder.IsDirty = false;
                _flagCustomerSetRelate = true;

                //Changed tab
                _selectedTabIndex = (int)SaleOrderSelectedTab;

                OnPropertyChanged(() => SelectedTabIndex);
                _saleOrderId = 0;
                IsSearchMode = false;
                IsForceFocused = true;
            }
        }

        /// <summary>
        /// Load Selected SaleOrder when item is selected with get from db or collection
        /// </summary>
        private void SetSelectedSaleOrderFromDbOrCollection()
        {
            if (SaleOrderCollection.Any(x => x.Id.Equals(SaleOrderId)))
            {
                SelectedSaleOrder = SaleOrderCollection.SingleOrDefault(x => x.Id.Equals(SaleOrderId));
            }
            else
            {
                //If Current SaleOrder loading not yet
                base_SaleOrder saleOrder = _saleOrderRepository.Get(x => x.Id.Equals(SaleOrderId));
                if (saleOrder != null)
                {
                    SelectedSaleOrder = new base_SaleOrderModel(saleOrder);
                    SetSaleOrderToModel(SelectedSaleOrder);
                }
            }
            if (SelectedSaleOrder != null)
                SetSaleOrderRelation(SelectedSaleOrder, true);
        }

        /// <summary>
        /// ShowShip Tab
        /// </summary>
        private void SetShippingStatus(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.OrderStatus < (short)SaleOrderStatus.Shipping && saleOrderModel.SaleOrderShipCollection != null && saleOrderModel.SaleOrderShipCollection.Any())
                saleOrderModel.OrderStatus = (short)SaleOrderStatus.Shipping;
        }

        /// <summary>
        /// Set for SaleOrderStatus when order is Ship full
        /// </summary>
        private void SetShipStatus()
        {
            bool ShipAll = true;
            foreach (var item in SelectedSaleOrder.SaleOrderDetailCollection)
            {
                decimal shipTotal = SelectedSaleOrder.SaleOrderShipCollection.Where(x => x.IsShipped == true).Sum(x => x.SaleOrderShipDetailCollection.Where(y => y.SaleOrderDetailResource == item.Resource.ToString() && y.ProductResource == item.ProductResource).Sum(z => z.PackedQty));
                ShipAll &= item.Qty > 0 && shipTotal == item.Qty;
            }

            if (!SelectedSaleOrder.Mark.Equals(MarkType.SaleOrder.ToDescription()))//Set Close for layaway
            {
                if (ShipAll)
                {
                    SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Close;
                }
            }
            else
            {
                if (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull))//Not change status when PaidInFull
                    return;

                if (ShipAll)
                    SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.FullyShipped;
                else if (SelectedSaleOrder.SaleOrderShipCollection.Any())
                    SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Shipping;
            }




        }

        /// <summary>
        /// Check to Show ship tab when has saleorder detail
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void ShowShipTab(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel != null)
            {
                if (saleOrderModel.Mark.Equals(MarkType.Layaway.ToDescription()))
                    saleOrderModel.ShipProcess = saleOrderModel.Balance == 0;
                else
                    saleOrderModel.ShipProcess = (saleOrderModel.SaleOrderDetailCollection != null ? saleOrderModel.SaleOrderDetailCollection.Any() : false) && !saleOrderModel.IsNew;

                OnPropertyChanged(() => AllowSOShipping);
                OnPropertyChanged(() => AllowSOReturn);
            }

        }

        /// <summary>
        /// set user change order follow config & order status
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetAllowChangeOrder(base_SaleOrderModel saleOrderModel)
        {
            if (BreakAllChange)
                return;
            if (saleOrderModel.IsLocked)
                this.IsAllowChangeOrder = false;
            else if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Quote))
                this.IsAllowChangeOrder = true;
            else if (saleOrderModel.PaymentCollection != null && saleOrderModel.PaymentCollection.Any(x => !x.IsDeposit.Value))/*has paid*/
                this.IsAllowChangeOrder = false;
            else if (saleOrderModel.OrderStatus < (short)SaleOrderStatus.FullyShipped)//Open or Shipping
                this.IsAllowChangeOrder = true;
            else if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull))
                this.IsAllowChangeOrder = false;
            else
                this.IsAllowChangeOrder = saleOrderModel.OrderStatus == (short)SaleOrderStatus.FullyShipped && Define.CONFIGURATION.IsAllowChangeOrder.Value;

        }

        /// <summary>
        /// Set Price to SaleOrderDetail with PriceLevelId(PriceSchemaId)
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void SetPriceUOM(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            if (saleOrderDetailModel.ProductUOMCollection != null)
            {
                base_ProductUOMModel productUOM = saleOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == saleOrderDetailModel.UOMId);
                base_ProductStore productStore = saleOrderDetailModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(SelectedSaleOrder.StoreCode));

                if (productStore != null)
                {
                    base_ProductUOM unitStore = productStore.base_ProductUOM.SingleOrDefault(x => x.UOMId.Equals(saleOrderDetailModel.UOMId));

                    //Update Quantity with base unit for ProductStore
                    if (unitStore == null)
                    {
                        if (saleOrderDetailModel.UOMId.Equals(saleOrderDetailModel.ProductModel.BaseUOMId))
                            saleOrderDetailModel.OnHandQty = productStore.QuantityOnHand;//Get Quantity Onhand from baseUnit
                        else
                            saleOrderDetailModel.OnHandQty = Convert.ToDecimal(productStore.QuantityOnHand) / Convert.ToDecimal(productUOM.BaseUnitNumber);
                    }
                    else
                    {
                        saleOrderDetailModel.OnHandQty = unitStore.QuantityOnHand;
                    }

                }
                else
                    saleOrderDetailModel.OnHandQty = 0;

                if (productUOM != null)
                {
                    saleOrderDetailModel.UnitName = productUOM.Name;
                    saleOrderDetailModel.UOM = productUOM.Name;

                    if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.RegularPrice))
                    {
                        //set Price with Price Level
                        if (productUOM.RegularPrice > 0)
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.RegularPrice;
                            saleOrderDetailModel.SalePrice = productUOM.RegularPrice;
                        }
                        else
                        {
                            //Get Price with UserUpdate when regularPrice =0 (UpdateProductPrice)
                            saleOrderDetailModel.SalePrice = saleOrderDetailModel.ProductModel.RegularPrice;
                        }
                    }
                    else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.SalePrice))
                    {
                        saleOrderDetailModel.RegularPrice = productUOM.Price1;
                        saleOrderDetailModel.SalePrice = productUOM.Price1;
                    }
                    else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.WholesalePrice))
                    {
                        saleOrderDetailModel.RegularPrice = productUOM.Price2;
                        saleOrderDetailModel.SalePrice = productUOM.Price2;
                    }
                    else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.Employee))
                    {
                        saleOrderDetailModel.RegularPrice = productUOM.Price3;
                        saleOrderDetailModel.SalePrice = productUOM.Price3;
                    }
                    else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.CustomPrice))
                    {
                        saleOrderDetailModel.RegularPrice = productUOM.Price4;
                        saleOrderDetailModel.SalePrice = productUOM.Price4;
                    }
                }
            }
        }

       

        #region SelectedItem
        /// <summary>
        /// Selected Customer of autocomplete changed
        /// <param name="setRelation">set value using for Change customer</param>
        /// </summary>
        private void SelectedCustomerChanged(bool setRelation = true)
        {
            //Don't set SaleOrder relation
            if (!setRelation)
                return;

            SelectedSaleOrder.CustomerResource = SelectedCustomer.Resource.ToString();
            SelectedSaleOrder.GuestModel = CustomerCollection.Where(x => x.Resource.Equals(SelectedCustomer.Resource)).FirstOrDefault();

            //Get Reward & set PurchaseThreshold if Customer any reward
            var reward = GetReward(SelectedSaleOrder.OrderDate.Value.Date);

            SelectedSaleOrder.IsApplyReward = SelectedSaleOrder.GuestModel.IsRewardMember;
            SelectedSaleOrder.IsApplyReward &= reward != null ? true : false;
            //isReward Member
            if (SelectedSaleOrder.GuestModel.IsRewardMember)
            {
                //Get GuestReward collection
                SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                foreach (base_GuestReward guestReward in SelectedSaleOrder.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(SelectedSaleOrder.GuestModel.Id) && !x.IsApply && x.ActivedDate.Value <= DateTime.Today && (!x.ExpireDate.HasValue || (x.ExpireDate.HasValue && DateTime.Today <= x.ExpireDate.Value.Date))))
                {
                    SelectedSaleOrder.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestReward));
                }

                if (SelectedSaleOrder.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                    SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;
            }
            else
            {
                if (reward != null && reward.IsPromptEnroll && !SelectedSaleOrder.GuestModel.IsRewardMember)
                {
                    //"This customer is not currently a member of reward program. Do you want to enroll this one?"
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_EnrollRewardMember"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        SelectedSaleOrder.GuestModel.IsRewardMember = true;
                        SelectedCustomer.IsRewardMember = true;
                        SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                        if (SelectedSaleOrder.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                            SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;
                    }
                }
            }

            SelectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
            SelectedSaleOrder.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);

            //PaymentTerm
            SelectedSaleOrder.TermDiscountPercent = SelectedCustomer.TermDiscount;
            SelectedSaleOrder.TermNetDue = SelectedCustomer.TermNetDue;
            SelectedSaleOrder.TermPaidWithinDay = SelectedCustomer.TermPaidWithinDay;
            SelectedSaleOrder.PaymentTermDescription = SelectedCustomer.PaymentTermDescription;
            if (SelectedCustomer.SaleRepId.HasValue)
            {
                base_GuestModel sale = EmployeeCollection.SingleOrDefault(x => x.Id == SelectedCustomer.SaleRepId);
                if (sale != null)
                    SelectedSaleOrder.SaleRep = sale.LegalName;
            }

            _saleOrderRepository.SetBillShipAddress(SelectedCustomer, SelectedSaleOrder);

            //CheckSaleTaxLocation
            if (SelectedCustomer.base_Guest.base_GuestAdditional.Any())
            {
                //Check Customer Additional choose TaxLocation in TaxInformation & TaxLocation difference with Default TaxLocation in Configuration
                base_GuestAdditional guestAdditional = SelectedCustomer.base_Guest.base_GuestAdditional.FirstOrDefault();
                if (guestAdditional.IsTaxExemption)
                {
                    SelectedSaleOrder.IsTaxExemption = guestAdditional.IsTaxExemption;
                    SelectedSaleOrder.TaxExemption = guestAdditional.TaxExemptionNo;
                    SelectedSaleOrder.TaxAmount = 0;
                    SelectedSaleOrder.TaxPercent = 0;
                    SelectedSaleOrder.TaxLocation = 0;
                    SelectedSaleOrder.TaxCode = string.Empty;

                }
                else if (guestAdditional.SaleTaxLocation > 0
                   && guestAdditional.SaleTaxLocation != Define.CONFIGURATION.DefaultSaleTaxLocation)//Diffrence with DefaultTaxLocation
                {
                    SelectedSaleOrder.IsTaxExemption = false;
                    SelectedSaleOrder.TaxExemption = string.Empty;
                    base_SaleTaxLocation saleTaxLocation = _saleTaxRepository.Get(x => x.Id == guestAdditional.SaleTaxLocation && x.ParentId == 0);
                    // "Do you want to apply {0} tax"
                    string msg = string.Format(Language.GetMsg("SO_Message_ApplyTax"), saleTaxLocation.Name);
                    MessageBoxResult resultMsg = Xceed.Wpf.Toolkit.MessageBox.Show(msg, Language.GetMsg("POSCaption"), MessageBoxButton.YesNo);
                    if (resultMsg.Equals(MessageBoxResult.Yes))
                    {
                        //Get TaxRate From tableCustomer
                        SelectedSaleOrder.TaxLocation = Convert.ToInt32(guestAdditional.SaleTaxLocation);
                        SelectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                    }
                    else
                    {
                        //Get TaxRate from default & calculate with requirement
                        SelectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                    }
                }
                else
                {
                    SelectedSaleOrder.IsTaxExemption = false;
                    SelectedSaleOrder.TaxExemption = string.Empty;
                }
            }
            if (SelectedSaleOrder.TaxLocation > 0)
            {
                GetSaleTax(SelectedSaleOrder);
            }

            //set Customer option in additional of markdownprice Level
            _saleOrderRepository.SetGuestAdditionalModel(SelectedSaleOrder);

            if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.MarkdownPriceLevel))
                SelectedSaleOrder.PriceSchemaId = SelectedSaleOrder.GuestModel.AdditionalModel.PriceSchemeId.Value;

            //Calculate Tax when user change Customer after create order detail
            if (SelectedSaleOrder.SaleOrderDetailCollection.Any())
            {
                CalculateAllTax(SelectedSaleOrder);

                //Confirm if existed customer option discount  PriceLevelType.MarkdownPriceLevel 
                CalcDiscountAllItemWithCustomerAdditional();
            }
        }

        /// <summary>
        /// Selected Product changed
        /// </summary>
        private void SelectedProductChanged()
        {
            if (SelectedProduct != null)
            {
                try
                {
                    SaleProductHandle(SelectedProduct);
                }
                catch
                {
                    _selectedProduct = null;
                }
            }
            _selectedProduct = null;
        }

        /// <summary>
        /// Selected Return Detail Changed
        /// when item is selected,is check collection reference  exited with item choice (compare saleOrderResource)?
        /// unless get item from DeletedItems(used for store item) add to collection shipped(collection autocompelete choice item)
        /// </summary>
        private void SelectedReturnDetailChanged()
        {
            base_ResourceReturnDetailModel selectedReturnDetail = SelectedReturnDetail as base_ResourceReturnDetailModel;
            if (selectedReturnDetail == null || selectedReturnDetail.SaleOrderDetailModel == null)
                return;

            CheckReturned(selectedReturnDetail);
        }


        #endregion

        //CRUD region
        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void InsertSaleOrder()
        {
            if (SelectedSaleOrder.IsNew)
            {
                SelectedSaleOrder.Shift = Define.ShiftCode;
                UpdateCustomerAddress(SelectedSaleOrder.BillAddressModel);
                SelectedSaleOrder.BillAddressId = SelectedSaleOrder.BillAddressModel.Id;
                UpdateCustomerAddress(SelectedSaleOrder.ShipAddressModel);
                SelectedSaleOrder.ShipAddressId = SelectedSaleOrder.ShipAddressModel.Id;
                //Sale Order Detail Model
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderDetailModel.Quantity);

                    saleOrderDetailModel.ToEntity();
                    SelectedSaleOrder.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                }
                _productRepository.Commit();

                SavePaymentCollection(SelectedSaleOrder);

                SelectedSaleOrder.ToEntity();
                _saleOrderRepository.Add(SelectedSaleOrder.base_SaleOrder);

                _saleOrderRepository.Commit();
                SelectedSaleOrder.EndUpdate();
                //Set ID
                SelectedSaleOrder.ToModel();
                SelectedSaleOrder.EndUpdate();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    saleOrderDetailModel.ToModel();
                    saleOrderDetailModel.EndUpdate();
                }

                if (SelectedSaleOrder.PaymentCollection != null)
                {
                    foreach (base_ResourcePaymentModel paymentModel in SelectedSaleOrder.PaymentCollection.Where(x => x.IsNew))
                    {
                        paymentModel.ToModel();
                        //Update or Add New PaymentDetail
                        if (paymentModel.PaymentDetailCollection != null)
                        {
                            foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentModel.PaymentDetailCollection.Where(x => x.IsNew))
                            {
                                paymentDetailModel.ToModel();
                                paymentDetailModel.EndUpdate();
                            }
                        }
                        paymentModel.EndUpdate();
                    }
                }
                SaleOrderCollection.Insert(0, SelectedSaleOrder);
                TotalSaleOrder++;
                ShowShipTab(SelectedSaleOrder);
            }
        }

        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void UpdateSaleOrder()
        {
            SelectedSaleOrder.Shift = Define.ShiftCode;
            //Insert or update address for customer
            UpdateCustomerAddress(SelectedSaleOrder.BillAddressModel);
            UpdateCustomerAddress(SelectedSaleOrder.ShipAddressModel);
            //set dateUpdate
            SelectedSaleOrder.DateUpdated = DateTime.Now;
            #region SaleOrderDetail
            //Delete SaleOrderDetail
            if (SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Any())
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems)
                {
                    //Get quantity from entity to substract store(avoid quantity in model is changed)
                    _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                    _saleOrderDetailRepository.Delete(saleOrderDetailModel.base_SaleOrderDetail);
                }
                _saleOrderDetailRepository.Commit();
                SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Clear();
            }

            if (SelectedSaleOrder.IsPurge)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                }
            }
            else
            {
                //Sale Order Detail Model
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.IsDirty))
                {
                    //Need to check difference store code (user change to another store)
                    if (SelectedSaleOrder.StoreCode.Equals(SelectedSaleOrder.base_SaleOrder.StoreCode))
                    {
                        if (saleOrderDetailModel.Quantity != saleOrderDetailModel.base_SaleOrderDetail.Quantity || saleOrderDetailModel.UOMId != saleOrderDetailModel.base_SaleOrderDetail.UOMId) //addition quantity
                        {
                            _saleOrderRepository.UpdateCustomerQuantityChanged(saleOrderDetailModel, SelectedSaleOrder.StoreCode);
                        }
                    }
                    else
                    {
                        //Subtract quantity from "old store"(user change to another store)
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                        //Add quantity to new store
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderDetailModel.Quantity);
                    }

                    saleOrderDetailModel.ToEntity();
                    if (saleOrderDetailModel.IsNew)
                        SelectedSaleOrder.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                    saleOrderDetailModel.EndUpdate();
                }
            }
            _productRepository.Commit();
            #endregion

            #region SaleOrderShip
            if (SelectedSaleOrder.SaleOrderShipCollection.DeletedItems.Any())
            {
                //Delete Sale Order Ship Model
                foreach (base_SaleOrderShipModel saleOrderShipModel in SelectedSaleOrder.SaleOrderShipCollection.DeletedItems)
                    _saleOrderShipRepository.Delete(saleOrderShipModel.base_SaleOrderShip);
                _saleOrderShipRepository.Commit();
                SelectedSaleOrder.SaleOrderShipCollection.DeletedItems.Clear();
            }

            //Sale Order Ship Model
            foreach (base_SaleOrderShipModel saleOrderShipModel in SelectedSaleOrder.SaleOrderShipCollection.Where(x => x.IsDirty || x.IsNew))
            {
                saleOrderShipModel.IsShipped = saleOrderShipModel.IsChecked;

                // Delete SaleOrderShipDetail
                if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Any())
                {
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModelDel in saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems)
                        _saleOrderShipDetailRepository.Delete(saleOrderShipDetailModelDel.base_SaleOrderShipDetail);
                    _saleOrderShipDetailRepository.Commit();
                    saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Clear();
                }

                //Update SaleOrderShipDetail & Upd
                if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.Any())
                {
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        if (!saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                        {
                            //Package is shipped & is a new shipped
                            if (saleOrderShipModel.IsShipped && !saleOrderShipModel.base_SaleOrderShip.IsShipped)
                            {
                                //Descrease OnHand product Store which product in SaleOrderShipDetail
                                //Descrease store with Product On Hand in group with parent product
                                //Cause : Item Product Group not stockable 
                                if (saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                                {
                                    foreach (base_ProductGroup productGroup in saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1)
                                    {
                                        //Get Product From ProductCollection (child product)
                                        base_ProductModel productInGroupModel = this.ProductCollection.SingleOrDefault(x => x.Id.Equals(productGroup.base_Product.Id));
                                        //Get Unit Of Product

                                        base_ProductUOM productGroupUOM = _saleOrderRepository.GetProductUomOfProductInGroup(SelectedSaleOrder.StoreCode, productGroup);
                                        if (productGroupUOM != null)
                                        {
                                            decimal baseUnitNumber = productGroupUOM.BaseUnitNumber;
                                            //productGroup.Quantity : quantity default of group
                                            decimal packQty = productGroup.Quantity * saleOrderShipDetailModel.PackedQty;
                                            _productRepository.UpdateOnHandQuantity(productInGroupModel.Resource.ToString(), SelectedSaleOrder.StoreCode, packQty, true, baseUnitNumber);
                                        }
                                    }
                                }
                                else
                                {
                                    decimal baseUnitNumber = saleOrderShipDetailModel.SaleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(saleOrderShipDetailModel.SaleOrderDetailModel.UOMId)).BaseUnitNumber;
                                    _productRepository.UpdateOnHandQuantity(saleOrderShipDetailModel.ProductResource, SelectedSaleOrder.StoreCode, saleOrderShipDetailModel.PackedQty, true, baseUnitNumber);
                                }

                                //Descrease Quantity OnCustomer which product in SaleOrderDetail
                                _saleOrderRepository.UpdateCustomerQuantity(saleOrderShipDetailModel.SaleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderShipDetailModel.PackedQty, false);
                            }
                        }

                        saleOrderShipDetailModel.ToEntity();
                        if (saleOrderShipDetailModel.IsNew)
                            saleOrderShipModel.base_SaleOrderShip.base_SaleOrderShipDetail.Add(saleOrderShipDetailModel.base_SaleOrderShipDetail);
                    }

                    //Calulate Profit For product Package is shipped & is a new shipped
                    if (!saleOrderShipModel.base_SaleOrderShip.IsShipped && saleOrderShipModel.IsShipped)
                    {
                        /// Calulate Profit For product
                        var gShip = saleOrderShipModel.SaleOrderShipDetailCollection.GroupBy(x => x.ProductResource);
                        foreach (var item in gShip)//Foreach collection group with Product
                        {
                            if (item.Any(x => x.SaleOrderDetailModel.ProductModel.IsCoupon))//Not Calculate OnHand with Coupon
                                continue;
                            decimal totalQuantityBaseUom = 0;

                            if (item.Any(x => x.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group)))
                            {
                                foreach (var saleOrderShipDetail in item)
                                {
                                    //Get Product In Group
                                    foreach (base_ProductGroup productGroup in saleOrderShipDetail.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1)
                                    {
                                        base_ProductUOM productGroupUOM = _saleOrderRepository.GetProductUomOfProductInGroup(SelectedSaleOrder.StoreCode, productGroup);
                                        decimal quantityBaseUnit = productGroupUOM.BaseUnitNumber;
                                        totalQuantityBaseUom += quantityBaseUnit * saleOrderShipDetail.PackedQty;
                                        decimal total = 0;
                                        //Get SaleOrderDetail to know is item change price ?
                                        base_SaleOrderDetailModel saleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetail.SaleOrderDetailResource));
                                        if (saleOrderDetailModel != null)
                                        {
                                            int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
                                            decimal totalPriceProductGroup = saleOrderShipDetail.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1.Sum(x => x.Amount);
                                            decimal unitPrice = productGroup.RegularPrice + (productGroup.RegularPrice * (saleOrderDetailModel.SalePrice - totalPriceProductGroup) / totalPriceProductGroup);
                                            total = Math.Round(productGroup.Quantity * unitPrice, 2);
                                            _productRepository.UpdateProductStore(productGroup.ProductResource, SelectedSaleOrder.StoreCode, totalQuantityBaseUom, total, 0, 0, true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var saleOrderShipDetail in item)
                                {
                                    decimal quantityBaseUnit = saleOrderShipDetail.SaleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(saleOrderShipDetail.SaleOrderDetailModel.UOMId)).BaseUnitNumber;
                                    totalQuantityBaseUom += quantityBaseUnit * saleOrderShipDetail.PackedQty;
                                }
                                _productRepository.UpdateProductStore(item.Key, SelectedSaleOrder.StoreCode, totalQuantityBaseUom, item.Sum(x => x.SaleOrderDetailModel.SalePrice * x.PackedQty), 0, 0, true);
                            }
                        }
                    }

                }
                //Map value Of Model To Entity
                saleOrderShipModel.ToEntity();
                if (saleOrderShipModel.IsNew)
                    SelectedSaleOrder.base_SaleOrder.base_SaleOrderShip.Add(saleOrderShipModel.base_SaleOrderShip);

            }
            #endregion

            #region SaleOrderReturn
            if (SelectedSaleOrder.ReturnModel != null)
            {
                bool calcGuestReward = false;
                if (SelectedSaleOrder.ReturnModel.SubTotal != SelectedSaleOrder.ReturnModel.base_ResourceReturn.SubTotal
                    && SelectedSaleOrder.Paid + SelectedSaleOrder.Deposit.Value >= SelectedSaleOrder.RewardAmount
                    && SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned))
                {
                    calcGuestReward = true;
                    //Value subtract
                    decimal valueSubtract = SelectedSaleOrder.ReturnModel.SubTotal - SelectedSaleOrder.ReturnModel.base_ResourceReturn.SubTotal;
                    SelectedSaleOrder.GuestModel.PurchaseDuringTrackingPeriod -= valueSubtract;
                }

                SelectedSaleOrder.ReturnModel.ToEntity();
                //Update Refund for SaleOrder
                SelectedSaleOrder.RefundedAmount = SelectedSaleOrder.ReturnModel.TotalRefund;

                if (SelectedSaleOrder.ReturnModel.IsNew && SelectedSaleOrder.ReturnModel.ReturnDetailCollection.DeletedItems.Any())
                {
                    foreach (base_ResourceReturnDetailModel returnDetailModel in SelectedSaleOrder.ReturnModel.ReturnDetailCollection.DeletedItems.Where(x => !x.IsTemporary))
                        _resourceReturnDetailRepository.Delete(returnDetailModel.base_ResourceReturnDetail);
                }
                SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Clear();

                foreach (base_ResourceReturnDetailModel returnDetailModel in SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsDirty))
                {
                    if (!returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        decimal totalQuantityBaseUom = 0;
                        if (!returnDetailModel.base_ResourceReturnDetail.IsReturned && returnDetailModel.IsReturned)//New Item Return
                        {
                            decimal quantityBaseUnit = returnDetailModel.SaleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(returnDetailModel.SaleOrderDetailModel.UOMId)).BaseUnitNumber;

                            totalQuantityBaseUom = quantityBaseUnit * returnDetailModel.ReturnQty;
                            //Update Product Profit
                            _productRepository.UpdateProductStore(returnDetailModel.ProductResource, SelectedSaleOrder.StoreCode, 0, 0, totalQuantityBaseUom, returnDetailModel.Price * returnDetailModel.ReturnQty, true);

                            //Increase Store for return product
                            _productRepository.UpdateOnHandQuantity(returnDetailModel.ProductResource, SelectedSaleOrder.StoreCode, totalQuantityBaseUom);
                        }
                    }

                    returnDetailModel.ToEntity();
                    if (returnDetailModel.IsNew)
                        SelectedSaleOrder.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail.Add(returnDetailModel.base_ResourceReturnDetail);
                }

                //Handle Return Reward For reward Member
                if (SelectedSaleOrder.GuestModel.IsRewardMember)
                {
                    var reward = GetReward(SelectedSaleOrder.OrderDate.Value.Date);

                    if (reward != null && calcGuestReward && SelectedSaleOrder.Balance == 0)
                    {
                        int totalOfReward = Convert.ToInt32(Math.Truncate(SelectedSaleOrder.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold));
                        short rewardAvaliable = (short)GuestRewardStatus.Available;
                        short rewardPending = (short)GuestRewardStatus.Pending;

                        SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold - ((SelectedSaleOrder.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold) % 1) * reward.PurchaseThreshold;

                        IEnumerable<base_GuestReward> guestReward = SelectedSaleOrder.GuestModel.base_Guest.base_GuestReward.Where(x => (x.Status == rewardAvaliable || x.Status == rewardPending) && x.Reason != "Manual");
                        if (guestReward.Any() && totalOfReward > 0 && totalOfReward < guestReward.Count())
                        {
                            int rewardRemoved = guestReward.Count() - totalOfReward;
                            for (int i = rewardRemoved; i > 0; i--)
                                _guestRewardRepository.Delete(guestReward.ElementAt(i - 1));

                            _guestRewardRepository.Commit();
                        }

                        //Get GuestReward collection
                        SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                        foreach (base_GuestReward guestRewardItem in SelectedSaleOrder.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(SelectedSaleOrder.GuestModel.Id) && !x.IsApply && x.ActivedDate.Value <= DateTime.Today && (!x.ExpireDate.HasValue || (x.ExpireDate.HasValue && DateTime.Today <= x.ExpireDate.Value.Date))))
                        {
                            SelectedSaleOrder.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestRewardItem));
                        }
                    }
                }

                if (SelectedSaleOrder.ReturnModel.IsNew)
                    _resourceReturnRepository.Add(SelectedSaleOrder.ReturnModel.base_ResourceReturn);
                _resourceReturnRepository.Commit();


                //Calculate Commission when return product
                if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned))
                    CalcCommissionForReturn(SelectedSaleOrder);
                calcGuestReward = false;
                //Update ID
                SelectedSaleOrder.ReturnModel.Id = SelectedSaleOrder.ReturnModel.base_ResourceReturn.Id;
                SelectedSaleOrder.ReturnModel.EndUpdate();

                foreach (base_ResourceReturnDetailModel returnDetailModel in SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsDirty))
                {
                    returnDetailModel.Id = returnDetailModel.base_ResourceReturnDetail.Id;
                    returnDetailModel.ResourceReturnId = returnDetailModel.base_ResourceReturnDetail.ResourceReturnId;
                    returnDetailModel.EndUpdate();
                }
            }
            #endregion

            #region Payment
            SavePaymentCollection(SelectedSaleOrder);
            #endregion

            #region Commission
            if (SelectedSaleOrder.CommissionCollection != null && SelectedSaleOrder.CommissionCollection.Any())
            {
                foreach (base_SaleCommissionModel saleCommissionModel in SelectedSaleOrder.CommissionCollection)
                {
                    saleCommissionModel.ToEntity();
                    if (saleCommissionModel.IsNew)
                        _saleCommissionRepository.Add(saleCommissionModel.base_SaleCommission);
                }
                _saleCommissionRepository.Commit();
                SelectedSaleOrder.CommissionCollection.Clear();
            }
            #endregion

            SelectedSaleOrder.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedSaleOrder.ToEntity();
            _saleOrderRepository.Commit();
            _productRepository.Commit();

            //Set ID
            #region Update Id & Set End Update
            SelectedSaleOrder.ToModel();
            SelectedSaleOrder.EndUpdate();
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
            {
                saleOrderDetailModel.ToModel();
                saleOrderDetailModel.EndUpdate();
            }

            foreach (base_SaleOrderShipModel saleOrderShipModel in SelectedSaleOrder.SaleOrderShipCollection)
            {
                saleOrderShipModel.ToModel();
                foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                {
                    saleOrderShipModel.ToModel();
                    saleOrderShipModel.EndUpdate();
                }
                saleOrderShipModel.EndUpdate();
            }

            //Update ID For Payment
            if (SelectedSaleOrder.PaymentCollection != null)
            {
                foreach (base_ResourcePaymentModel paymentModel in SelectedSaleOrder.PaymentCollection.Where(x => x.IsNew))
                {
                    paymentModel.ToModel();
                    //Update or Add New PaymentDetail
                    if (paymentModel.PaymentDetailCollection != null)
                    {
                        foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentModel.PaymentDetailCollection.Where(x => x.IsNew))
                        {
                            paymentDetailModel.ToModel();
                            paymentDetailModel.EndUpdate();
                        }
                    }
                    paymentModel.EndUpdate();
                }
            }
            #endregion

        }

        /// <summary>
        /// Save Sale Order
        /// </summary>
        /// <returns></returns>
        private bool SaveSalesOrder()
        {
            bool result = false;
            try
            {
                UnitOfWork.BeginTransaction();
                if (SelectedSaleOrder.IsNew)
                    InsertSaleOrder();
                else
                    UpdateSaleOrder();

                UpdateCustomer(SelectedSaleOrder);
                UnitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                result = false;
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        /// <summary>
        /// Insert /Update new Bill or Ship Address for customer 
        /// </summary>
        /// <param name="customerAddressModel"></param>
        private void UpdateCustomerAddress(base_GuestAddressModel customerAddressModel)
        {
            if (customerAddressModel.IsDirty)
            {
                customerAddressModel.GuestId = SelectedCustomer.Id;
                customerAddressModel.DateCreated = DateTime.Now;
                customerAddressModel.ToEntity();
                if (customerAddressModel.IsNew)
                    _guestAddressRepository.Add(customerAddressModel.base_GuestAddress);
                _guestAddressRepository.Commit();
                customerAddressModel.Id = customerAddressModel.base_GuestAddress.Id;
                customerAddressModel.EndUpdate();
            }
        }

        /// <summary>
        /// calculate commission for employee
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SaveSaleCommission(base_SaleOrderModel saleOrderModel, decimal total)
        {
            ComboItem item = Common.BookingChannel.SingleOrDefault(x => x.Value == SelectedSaleOrder.BookingChanel);
            if (item.Flag)//True : this booking channel dont use commission
                return;

            if (saleOrderModel.CommissionCollection == null)
                saleOrderModel.CommissionCollection = new CollectionBase<base_SaleCommissionModel>();
            Guid customerGuid = Guid.Parse(saleOrderModel.CustomerResource);
            //Get Customer with CustomerResource
            base_GuestModel customerModel = CustomerCollection.Where(x => x.Resource == customerGuid).SingleOrDefault();
            if (customerModel != null && customerModel.SaleRepId.HasValue)
            {
                base_GuestModel employeeModel = EmployeeCollection.Where(x => x.Id == customerModel.SaleRepId).SingleOrDefault();
                //base_SaleCommission saleCommission = _saleCommissionRepository.Get(x => x.GuestResource == employeeModel.ResourceString && x.SOResource.Equals(saleOrderModel.ResourceString));

                base_SaleCommissionModel newSaleCommission = new base_SaleCommissionModel();
                newSaleCommission.ComissionPercent = employeeModel.CommissionPercent;
                newSaleCommission.GuestResource = employeeModel.Resource.ToString();
                newSaleCommission.Remark = MarkType.SaleOrder.ToDescription();
                newSaleCommission.Sign = "+";
                newSaleCommission.SODate = saleOrderModel.OrderDate;
                newSaleCommission.SONumber = saleOrderModel.SONumber;
                newSaleCommission.SOResource = saleOrderModel.Resource.ToString();
                newSaleCommission.SOTotal = total;
                newSaleCommission.CommissionAmount = total * newSaleCommission.ComissionPercent / 100;
                saleOrderModel.CommissionCollection.Add(newSaleCommission);
            }
        }

        /// <summary>
        /// Calculate Commission for refunded 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalcCommissionForReturn(base_SaleOrderModel saleOrderModel)
        {
            ComboItem item = Common.BookingChannel.SingleOrDefault(x => x.Value == SelectedSaleOrder.BookingChanel);
            if (item.Flag)//True : this booking channel dont use commission
                return;

            if (saleOrderModel.CommissionCollection == null)
                saleOrderModel.CommissionCollection = new CollectionBase<base_SaleCommissionModel>();

            Guid customerGuid = Guid.Parse(saleOrderModel.CustomerResource);
            //Get Customer with CustomerResource
            base_GuestModel customerModel = CustomerCollection.Where(x => x.Resource == customerGuid).SingleOrDefault();
            if (customerModel != null && customerModel.SaleRepId.HasValue)
            {
                base_GuestModel employeeModel = EmployeeCollection.Where(x => x.Id == customerModel.SaleRepId).SingleOrDefault();
                string remarkReturn = MarkType.SaleOrderReturn.ToDescription();
                base_SaleCommission saleCommission = _saleCommissionRepository.Get(x => x.Sign.Equals("-") && x.Remark.Equals(remarkReturn) && x.GuestResource == employeeModel.ResourceString && x.SOResource.Equals(saleOrderModel.ResourceString));
                if (saleCommission == null)
                {
                    base_SaleCommissionModel newSaleCommission = new base_SaleCommissionModel();
                    newSaleCommission.ComissionPercent = employeeModel.CommissionPercent;
                    newSaleCommission.GuestResource = employeeModel.Resource.ToString();
                    newSaleCommission.Remark = MarkType.SaleOrderReturn.ToDescription();
                    newSaleCommission.Sign = "-";
                    newSaleCommission.SODate = saleOrderModel.OrderDate;
                    newSaleCommission.SONumber = saleOrderModel.SONumber;
                    newSaleCommission.SOResource = saleOrderModel.Resource.ToString();
                    newSaleCommission.SOTotal = saleOrderModel.Paid + saleOrderModel.Deposit;
                    newSaleCommission.CommissionAmount = saleOrderModel.ReturnModel.TotalRefund * newSaleCommission.ComissionPercent / 100;
                    saleOrderModel.CommissionCollection.Add(newSaleCommission);
                }
                else
                {
                    base_SaleCommissionModel UpdateSaleCommission = new base_SaleCommissionModel(saleCommission);
                    UpdateSaleCommission.SOTotal = saleOrderModel.Paid + saleOrderModel.Deposit;
                    UpdateSaleCommission.CommissionAmount = saleOrderModel.ReturnModel.TotalRefund * UpdateSaleCommission.ComissionPercent / 100;
                    if (saleOrderModel.CommissionCollection.Any(x => x.Sign.Equals("-")))
                    {
                        base_SaleCommissionModel updateCommisionModel = saleOrderModel.CommissionCollection.SingleOrDefault(x => x.Sign.Equals("-"));
                        updateCommisionModel = UpdateSaleCommission;
                    }
                    else
                    {
                        saleOrderModel.CommissionCollection.Add(UpdateSaleCommission);
                    }
                }
            }
        }

        /// <summary>
        /// Delete Sale Commision of SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void DeleteSaleCommission(base_SaleOrderModel saleOrderModel)
        {
            base_SaleCommission saleCommission = _saleCommissionRepository.GetAll().ToList().SingleOrDefault(x => x.SOResource.Equals(saleOrderModel.Resource.ToString()));
            if (saleCommission != null)
            {
                _saleCommissionRepository.Delete(saleCommission);
                _saleCommissionRepository.Commit();
            }
        }

        /// <summary>
        /// Update Customer when PaymentTerm changed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void UpdateCustomer(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.GuestModel.IsDirty)
            {
                //Update Term
                saleOrderModel.GuestModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                saleOrderModel.GuestModel.PaymentTermDescription = saleOrderModel.PaymentTermDescription;
                saleOrderModel.GuestModel.TermDiscount = saleOrderModel.TermDiscountPercent;
                saleOrderModel.GuestModel.TermNetDue = saleOrderModel.TermNetDue;
                saleOrderModel.GuestModel.TermPaidWithinDay = saleOrderModel.TermPaidWithinDay;

                //Update Customer Reward 
                saleOrderModel.GuestModel.ToEntity();
            }

            //Onlyt reward Member
            if (saleOrderModel.GuestModel.IsRewardMember)
            {
                if (saleOrderModel.GuestModel.GuestRewardCollection != null)
                {
                    //Update Guest Reward
                    if (saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                    {
                        //Insert or update item > today(not show in collection GuestReward)
                        foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems)
                        {
                            guestRewardModel.ToEntity();
                            if (guestRewardModel.IsNew)
                                saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                        }
                        saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Clear();
                    }

                    if (saleOrderModel.GuestModel.GuestRewardCollection.Any(x => x.IsNew))
                    {
                        foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection)
                        {
                            guestRewardModel.ToEntity();
                            if (guestRewardModel.IsNew)
                                saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                        }
                    }
                }
            }

            _guestRepository.Commit();
            //Set Id For Reward
            if (saleOrderModel.GuestModel.GuestRewardCollection != null)
            {
                if (saleOrderModel.GuestModel.GuestRewardCollection.Any(x => x.IsNew))
                {
                    foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.Where(x => x.IsNew))
                    {
                        guestRewardModel.Id = guestRewardModel.base_GuestReward.Id;
                        guestRewardModel.EndUpdate();
                    }
                }

            }
            saleOrderModel.GuestModel.EndUpdate();
        }

        /// <summary>
        /// Execute payment
        /// </summary>
        private void SaleOrderPayment()
        {
            if (SelectedSaleOrder.IsNew)
                return;

            bool? resultReward;
            bool isPayFull = false;
            bool isRewardOnDiscount = Define.CONFIGURATION.IsRewardOnDiscount.HasValue ? Define.CONFIGURATION.IsRewardOnDiscount.Value : false;
            bool isApplyRewardDiscount = isRewardOnDiscount || (!isRewardOnDiscount && SelectedSaleOrder.DiscountPercent == 0);
            //Show Reward Form
            //Need check has any Guest Reward
            //Show Reward only SaleOrder Payment
            #region Check & Apply Reward
            if (isApplyRewardDiscount &&
                   SelectedSaleOrder.GuestModel.IsRewardMember
                   && SelectedSaleOrder.GuestModel.GuestRewardCollection != null && SelectedSaleOrder.GuestModel.GuestRewardCollection.Any()
                   && SelectedSaleOrder.PaymentCollection != null
                   && !SelectedSaleOrder.PaymentCollection.Any(x => !x.IsDeposit.Value) /* This order is paid with multi pay*/
                   )
            {
                //Confirm User want to Payment Full
                //msg: You have some rewards, you need to pay fully and use these rewards. Do you?
                MessageBoxResult confirmPayFull = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_NotifyPayfullUseReward"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (confirmPayFull.Equals(MessageBoxResult.Yes))//User Payment full
                {
                    isPayFull = true;
                    int ViewActionType;
                    if (Define.CONFIGURATION.IsRequirePromotionCode.HasValue && Define.CONFIGURATION.IsRequirePromotionCode.Value)//Open Enter Barcode 
                    {

                        ConfirmMemberRedeemRewardViewModel confirmMemberRedeemRewardViewModel = new ConfirmMemberRedeemRewardViewModel(SelectedSaleOrder);
                        resultReward = _dialogService.ShowDialog<ConfirmMemberRedeemRewardView>(_ownerViewModel, confirmMemberRedeemRewardViewModel, Language.GetMsg("SO_Message_RedeemReward"));
                        ViewActionType = (int)confirmMemberRedeemRewardViewModel.ViewActionType;
                    }
                    else
                    {
                        RedeemRewardViewModel redeemRewardViewModel = new RedeemRewardViewModel(SelectedSaleOrder);
                        resultReward = _dialogService.ShowDialog<RedeemRewardView>(_ownerViewModel, redeemRewardViewModel, Language.GetMsg("SO_Message_RedeemReward"));
                        ViewActionType = (int)redeemRewardViewModel.ViewActionType;
                    }

                    if (resultReward == true)
                    {
                        if (ViewActionType == (int)ConfirmMemberRedeemRewardViewModel.ReeedemRewardType.Redeemded)
                            SelectedSaleOrder.RewardValueApply = 0;
                        else
                            SelectedSaleOrder.IsRedeeem = true;//Customer used reward
                    }
                }
                else
                {
                    isPayFull = false;
                    resultReward = true;
                }
            }
            else
                resultReward = true;
            #endregion

            if (resultReward == true)
            {
                SelectedSaleOrder.RewardValueApply = 0;
                //Calc Subtotal user apply reward
                if (SelectedSaleOrder.GuestModel.GuestRewardCollection != null && SelectedSaleOrder.GuestModel.GuestRewardCollection.Any(x => x.IsChecked))
                {
                    base_GuestRewardModel guestReward = SelectedSaleOrder.GuestModel.GuestRewardCollection.Single(x => x.IsChecked);
                    if (guestReward != null)
                    {
                        //Update Subtoal After apply reward
                        if (guestReward.base_GuestReward.base_RewardManager.RewardAmtType.Is(RewardType.Pecent))
                        {
                            decimal subTotal = 0;
                            if (Define.CONFIGURATION.IsRewardOnTax)//Check reward include tax ?
                                subTotal = SelectedSaleOrder.SubTotal - SelectedSaleOrder.DiscountAmount + SelectedSaleOrder.TaxAmount;
                            else
                                subTotal = SelectedSaleOrder.SubTotal - SelectedSaleOrder.DiscountAmount;

                            SelectedSaleOrder.RewardValueApply = Math.Round(Math.Round(subTotal * guestReward.base_GuestReward.base_RewardManager.RewardAmount / 100) - 0.01M, MidpointRounding.AwayFromZero);
                        }
                        else
                            SelectedSaleOrder.RewardValueApply = guestReward.base_GuestReward.base_RewardManager.RewardAmount;
                    }
                }
                //Update total have to paid
                if (SelectedSaleOrder.base_SaleOrder.RewardAmount == 0)
                    SelectedSaleOrder.RewardAmount = Math.Round(Math.Round(SelectedSaleOrder.Total - SelectedSaleOrder.RewardValueApply) - 0.01M, MidpointRounding.AwayFromZero);
                //SelectedSaleOrder.Total - SelectedSaleOrder.RewardValueApply;

                //Return Product Proccess
                //Subtract total of refunded in Return process
                decimal refunded = SelectedSaleOrder.ReturnModel != null ? SelectedSaleOrder.ReturnModel.TotalRefund : 0;

                //Handle subtract money when has some product is return
                decimal returnValue = 0;
                if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any())
                    returnValue = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsReturned).Sum(x => x.Amount) + CalculateReturnTax(SelectedSaleOrder.ReturnModel, SelectedSaleOrder);

                //End Return Product Proccess
                decimal paidValue = SelectedSaleOrder.PaymentCollection.Sum(x => x.TotalPaid - x.Change);
                decimal balance = SelectedSaleOrder.RewardAmount - returnValue - paidValue - (SelectedSaleOrder.Deposit.HasValue ? SelectedSaleOrder.Deposit.Value : 0);

                decimal totalDeposit = 0;
                decimal lastPayment = 0;
                if (SelectedSaleOrder.PaymentCollection != null)
                {
                    totalDeposit = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
                    base_ResourcePaymentModel lastPaymentModel = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid > 0).OrderBy(x => x.DateCreated).LastOrDefault();
                    if (lastPaymentModel != null)
                        lastPayment = lastPaymentModel.TotalPaid;
                }

                //Show Payment
                SalesOrderPaymenViewModel paymentViewModel = new SalesOrderPaymenViewModel(SelectedSaleOrder, balance, totalDeposit, lastPayment, isPayFull);
                bool? dialogResult = _dialogService.ShowDialog<SalesOrderPaymentView>(_ownerViewModel, paymentViewModel, "Payment");
                if (dialogResult == true)
                {
                    //Calc Reward , redeem & update subtotal
                    CalcRedeemReward(SelectedSaleOrder);

                    if (Define.CONFIGURATION.DefaultCashiedUserName.HasValue && Define.CONFIGURATION.DefaultCashiedUserName.Value)
                        paymentViewModel.PaymentModel.Cashier = Define.USER.LoginName;
                    // Add new payment to collection
                    SelectedSaleOrder.PaymentCollection.Add(paymentViewModel.PaymentModel);

                    SelectedSaleOrder.Paid = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value).Sum(x => x.TotalPaid - x.Change);
                    SelectedSaleOrder.CalcBalance();

                    //paymentViewModel.PaymentModel.TotalPaid - (SelectedSaleOrder.Deposit.HasValue ? SelectedSaleOrder.Deposit.Value : 0);
                    //Set Status
                    if (SelectedSaleOrder.Paid + SelectedSaleOrder.Deposit.Value >= SelectedSaleOrder.RewardAmount - returnValue)
                    {
                        if (SelectedSaleOrder.GuestModel.IsRewardMember)//Only for Reward Member
                        {
                            //Check IsCalRewardAfterRedeem Config
                            SelectedSaleOrder.GuestModel.PurchaseDuringTrackingPeriod += SelectedSaleOrder.RewardAmount;

                            bool isRewardApplied = SelectedSaleOrder.IsRedeeem;
                            if (Define.CONFIGURATION.IsCalRewardAfterRedeem //Calc reward anywat
                                || (!Define.CONFIGURATION.IsCalRewardAfterRedeem && !isRewardApplied))//calc new reward when so not apply redeem
                            {
                                CreateNewReward(SelectedSaleOrder);
                            }

                        }

                        if (SelectedSaleOrder.Mark.Equals(MarkType.SaleOrder.ToDescription()))
                        {
                            SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.PaidInFull;
                        }
                        else
                        {
                            SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Close;//Set status to close when SO convert from Layaway/workorder/Quote
                        }

                        //Calculate & create commission for Employee
                        SaveSaleCommission(SelectedSaleOrder, SelectedSaleOrder.PaymentCollection.Sum(x => x.TotalPaid - x.Change));

                        SaveSalesOrder();

                        //Not change to search when layaway cause after paid user may be execute shipping process
                        if (!SelectedSaleOrder.Mark.Equals(MarkType.Layaway.ToDescription()))
                            this.IsSearchMode = true;
                    }
                }
                else
                {
                    if (SelectedSaleOrder.GuestModel != null //Need for Quotation
                        && SelectedSaleOrder.GuestModel.GuestRewardCollection != null)
                    {
                        //Reset check
                        IEnumerable<base_GuestRewardModel> guestRewards = SelectedSaleOrder.GuestModel.GuestRewardCollection.Where(x => x.IsChecked);
                        foreach (base_GuestRewardModel guestReward in guestRewards)
                            guestReward.IsChecked = false;
                        SelectedSaleOrder.IsRedeeem = false;
                    }
                    SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total;
                }
                // Reset reward apply after use
                SelectedSaleOrder.RewardValueApply = 0;
            }

            SetAllowChangeOrder(SelectedSaleOrder);
        }

        /// <summary>
        /// Save payment collection, payment detail and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SavePaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.PaymentCollection != null)
            {
                foreach (base_ResourcePaymentModel paymentItem in saleOrderModel.PaymentCollection.Where(x => x.IsDirty))
                {
                    // Map data from model to entity
                    paymentItem.ToEntity();

                    if (paymentItem.PaymentDetailCollection != null)
                    {
                        foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentItem.PaymentDetailCollection.Where(x => x.IsDirty))
                        {
                            // Map data from model to entity
                            paymentDetailModel.ToEntity();

                            // Add new payment detail to database
                            if (paymentDetailModel.Id == 0)
                                paymentItem.base_ResourcePayment.base_ResourcePaymentDetail.Add(paymentDetailModel.base_ResourcePaymentDetail);
                        }
                    }

                    if (paymentItem.IsNew)
                        _paymentRepository.Add(paymentItem.base_ResourcePayment);
                    _paymentRepository.Commit();
                }
            }
        }

        /// <summary>
        /// Calculate Reward apply for guest
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="saleOrderModel"></param>
        private void CalcRedeemReward(base_SaleOrderModel saleOrderModel)
        {
            if (!saleOrderModel.GuestModel.IsRewardMember)
                return;
            //Calc Subtotal user apply reward
            if (saleOrderModel.GuestModel.GuestRewardCollection.Any(x => x.IsChecked))
            {
                base_GuestRewardModel guestReward = saleOrderModel.GuestModel.GuestRewardCollection.Single(x => x.IsChecked);
                //Update RewardAmount After apply reward
                if (guestReward.base_GuestReward.base_RewardManager.RewardAmtType.Is(RewardType.Pecent))
                    guestReward.RewardValue = saleOrderModel.SubTotal * guestReward.base_GuestReward.base_RewardManager.RewardAmount / 100;
                else
                    guestReward.RewardValue = guestReward.base_GuestReward.base_RewardManager.RewardAmount;

                //Set Total Reward Redeemed
                saleOrderModel.GuestModel.TotalRewardRedeemed += guestReward.RewardValue;

                //Update Reward Redeemed to Reward manager
                guestReward.base_GuestReward.base_RewardManager.TotalRewardRedeemed += guestReward.RewardValue;
                //UpdateDate Guest Reward
                guestReward.Amount = saleOrderModel.SubTotal;
                guestReward.IsApply = true;
                guestReward.AppliedDate = DateTime.Today;
                guestReward.Status = (short)GuestRewardStatus.Redeemed;
                saleOrderModel.GuestModel.GuestRewardCollection.Remove(guestReward);
            }
        }

        /// <summary>
        /// Create New Reward for customer
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CreateNewReward(base_SaleOrderModel saleOrderModel)
        {
            if (!saleOrderModel.GuestModel.IsRewardMember)
                return;
            //saleOrderModel.SubTotal >= x.PurchaseThreshold
            var reward = GetReward(saleOrderModel.OrderDate.Value.Date);

            if (reward != null)
            {
                int totalOfReward = 0;
                string msgExpireDate = string.Empty;
                //Check if not set Purchase Threshold
                if (saleOrderModel.GuestModel.RequirePurchaseNextReward == 0)
                    saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

                decimal totalOfPurchase = saleOrderModel.RewardAmount + (reward.PurchaseThreshold - saleOrderModel.GuestModel.RequirePurchaseNextReward);
                if (totalOfPurchase > reward.PurchaseThreshold)
                {
                    totalOfReward = Convert.ToInt32(Math.Truncate(totalOfPurchase / reward.PurchaseThreshold));

                    for (int i = 0; i < totalOfReward; i++)
                    {
                        base_GuestRewardModel guestRewardModel = new base_GuestRewardModel();
                        guestRewardModel.EarnedDate = DateTime.Today;
                        guestRewardModel.IsApply = false;
                        guestRewardModel.RewardId = reward.Id;
                        guestRewardModel.GuestId = saleOrderModel.GuestModel.Id;
                        guestRewardModel.SaleOrderNo = string.Empty;
                        guestRewardModel.SaleOrderResource = string.Empty;
                        guestRewardModel.Amount = 0;
                        guestRewardModel.RewardValue = 0;
                        guestRewardModel.Remark = string.Empty;

                        //Set Block reward redeemption for ??? days after earned
                        if (reward.IsBlockRedemption && reward.RedemptionAfterDays > 0)
                        {
                            guestRewardModel.Status = (short)GuestRewardStatus.Pending;
                            guestRewardModel.ActivedDate = guestRewardModel.EarnedDate.Value.AddDays(reward.RedemptionAfterDays);
                        }
                        else
                        {
                            guestRewardModel.Status = (int)GuestRewardStatus.Available;
                            guestRewardModel.ActivedDate = guestRewardModel.EarnedDate.Value;
                        }
                        //Set Expired Date For Reward
                        if (reward.RewardExpiration != 0)//RewardExpiration =0 (Never Expired)
                        {
                            int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == reward.RewardExpiration).Detail);
                            guestRewardModel.ExpireDate = guestRewardModel.ActivedDate.Value.AddDays(expireDay);
                        }
                        else
                        {
                            guestRewardModel.ExpireDate = null;
                        }

                        msgExpireDate = guestRewardModel.ExpireDate.HasValue ? guestRewardModel.ExpireDate.Value.ToString(Define.DateFormat) : Language.GetMsg("SO_Message_RewardNeverExpired");

                        if (guestRewardModel.ActivedDate.Value <= DateTime.Today)
                            saleOrderModel.GuestModel.GuestRewardCollection.Add(guestRewardModel);
                        else
                            saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Add(guestRewardModel);
                    }
                }

                //Calculate Require Purchase Next Reward
                //A is PurchaseDuringTrackingPeriod
                //P is PurchaseThreshold
                //R is RequirePurchaseNextReward
                //R = P - (A/P % 2 * P)

                saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold - ((saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold) % 1) * reward.PurchaseThreshold;

                //Notify to Cashier about reward customer earned ? 
                if (reward.IsInformCashier)
                {
                    if (totalOfReward > 0)
                    {
                        string rewardProgram = string.Empty;
                        if (reward.RewardAmtType.Equals(RewardType.Money))
                            rewardProgram = string.Format(Language.GetMsg("SO_Message_RewardAmount"), string.Format(Define.ConverterCulture, Define.CurrencyFormat, reward.RewardAmount));
                        else
                            rewardProgram = string.Format(Language.GetMsg("SO_Message_RewardAmount") + " %", reward.RewardAmount);

                        //Msg : You are received : {0} reward(s) {1}  \nExpire Date : {2}

                        Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.GetMsg("SO_Message_ReceivedReward").ToString().Replace("\\n", "\n"), totalOfReward, rewardProgram, msgExpireDate), Language.GetMsg("POSCaption"), MessageBoxButton.OK);
                    }
                }
            }
        }

        /// <summary>
        /// Create sale order detail with multi Product
        /// </summary>
        /// <param name="productCollection"></param>
        private void CreateSaleOrderDetailWithProducts(IEnumerable<base_ProductModel> productCollection)
        {
            foreach (base_ProductModel productModel in productCollection)
            {
                SaleProductHandle(productModel, false);
            }
            //Open MultiSerialTracking with item has serial tracking
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                IEnumerable<base_SaleOrderDetailModel> saleDetailSerialCollection = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel.IsSerialTracking && string.IsNullOrWhiteSpace(x.SerialTracking));
                if (saleDetailSerialCollection != null && saleDetailSerialCollection.Any())
                {
                    //Show Popup Update Serial Tracking for which item has quantity =1
                    IEnumerable<base_SaleOrderDetailModel> saleOrderCollectionWithOneItem = saleDetailSerialCollection.Where(x => x.Quantity == 1);
                    if (saleOrderCollectionWithOneItem.Any())
                    {
                        MultiTrackingNumberViewModel multiTrackingNumber = new MultiTrackingNumberViewModel(saleOrderCollectionWithOneItem);
                        bool? dialogResult = _dialogService.ShowDialog<MultiTrackingNumberView>(_ownerViewModel, multiTrackingNumber, "Multi Tracking Serial");
                    }

                    //Show popup update serial tracking for which item has quantity >1
                    IEnumerable<base_SaleOrderDetailModel> saleOrderCollectionWithMultiItem = saleDetailSerialCollection.Where(x => x.Quantity > 1);
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderCollectionWithMultiItem)
                    {
                        OpenTrackingSerialNumber(saleOrderDetailModel, true, true);
                    }

                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Insert & calculate with single product to sale order detail
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="showSerialPopup"></param>
        private void SaleProductHandle(base_ProductModel productModel, bool showSerialPopup = true)
        {
            if (!productModel.IsCoupon)
            {
                //Get VendorName
                base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == productModel.VendorId));
                if (vendorModel != null)
                    productModel.VendorName = vendorModel.LegalName;
            }

            //Product Group Process
            if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))//Process for ProductGroup
            {
                //Product Group :Create new SaleOrderDetail , ProductUOM & add to collection 
                this.BreakSODetailChange = true;
                string parentResource = SaleOrderDetailProcess(productModel, showSerialPopup);
                this.BreakSODetailChange = false;
                //ProductGroup Child
                foreach (base_ProductGroup productGroup in productModel.base_Product.base_ProductGroup1)
                {
                    //Get Product From ProductCollection
                    base_ProductModel productInGroupModel = this.ProductCollection.SingleOrDefault(x => x.Id.Equals(productGroup.base_Product.Id));
                    //Create new SaleOrderDetail , ProductUOM & add to collection
                    this.BreakSODetailChange = true;
                    SaleOrderDetailProcess(productInGroupModel, showSerialPopup, parentResource, productModel);
                    this.BreakSODetailChange = false;
                }
            }
            else
            {
                //Create new SaleOrderDetail , ProductUOM & add to collection
                this.BreakSODetailChange = true;
                SaleOrderDetailProcess(productModel, showSerialPopup);
                this.BreakSODetailChange = false;
            }
        }

        /// <summary>
        /// Handle single new sale Order detail
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="showSerialPopup"></param>
        /// <param name="productSerialParent"></param>
        private string SaleOrderDetailProcess(base_ProductModel productModel, bool showSerialPopup, string ParentResource = "", base_ProductModel productParentModel = null)
        {

            base_SaleOrderDetailModel salesOrderDetailModel = AddNewSaleOrderDetail(productModel, ParentResource, productParentModel);

            if (salesOrderDetailModel.ProductModel.IsCoupon)
            {
                salesOrderDetailModel.IsQuantityAccepted = true;
                salesOrderDetailModel.IsReadOnlyUOM = true;
                OpenCouponView(salesOrderDetailModel);
            }
            else
            {
                //Get Product UOMCollection
                _saleOrderRepository.GetProductUOMforSaleOrderDetail(salesOrderDetailModel);

                SetPriceUOM(salesOrderDetailModel);

                //Update price when regular price =0

                if (UpdateProductPrice(salesOrderDetailModel.ProductModel))
                {
                    salesOrderDetailModel.RegularPrice = salesOrderDetailModel.ProductModel.RegularPrice;
                    salesOrderDetailModel.SalePrice = salesOrderDetailModel.ProductModel.RegularPrice;
                }

                //SetUnit Price for productInGroup
                if (!string.IsNullOrWhiteSpace(ParentResource))
                {
                    int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
                    decimal totalPriceProductGroup = productParentModel.base_Product.base_ProductGroup1.Sum(x => x.Amount);
                    base_ProductGroup productGroup = productParentModel.base_Product.base_ProductGroup1.SingleOrDefault(x => x.ProductId.Equals(productModel.Id));
                    base_SaleOrderDetailModel salesOrderDetailParentModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(ParentResource));
                    if (productGroup != null && salesOrderDetailParentModel != null)
                    {
                        decimal unitPrice = productModel.RegularPrice + (productModel.RegularPrice * (salesOrderDetailParentModel.SalePrice - totalPriceProductGroup) / totalPriceProductGroup);
                        salesOrderDetailModel.SalePrice = unitPrice;
                    }
                }

                //Calculate Discount for product
                CalculateDiscount(salesOrderDetailModel);

                //Check Show Detail
                _saleOrderRepository.CheckToShowDatagridRowDetail(salesOrderDetailModel);

                salesOrderDetailModel.CalcDueQty();
                salesOrderDetailModel.CalUnfill();

                //Check on hand quatity
                _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, salesOrderDetailModel);

                if (productModel.IsSerialTracking && showSerialPopup)
                    OpenTrackingSerialNumber(salesOrderDetailModel, true);
            }

            CalculateAllTax(SelectedSaleOrder);

            salesOrderDetailModel.CalcSubTotal();
            salesOrderDetailModel.CalcDueQty();
            salesOrderDetailModel.CalUnfill();
            SelectedSaleOrder.CalcSubTotal();
            SelectedSaleOrder.CalcBalance();
            //set show ship tab when has product in detail
            ShowShipTab(SelectedSaleOrder);
            //Set ship tab status if config set IsAllowChange order when Ship Fully
            SetShipStatus();
            return salesOrderDetailModel.Resource.ToString();
        }

        /// <summary>
        /// Shipped Proccess 
        /// User click to shipped
        /// </summary>
        /// <param name="param"></param>
        private void ShippedProcess(object param)
        {
            //msg : "Do you want to ship?"
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ConfirmShipItem"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo);
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            if (result.Is(MessageBoxResult.Yes))
            {

                saleOrderShipModel.IsShipped = saleOrderShipModel.IsChecked;

                SelectedSaleOrder.ShippedBox = Convert.ToInt16(SelectedSaleOrder.SaleOrderShipCollection.Count(x => x.IsShipped));

                SelectedSaleOrder.RaiseAnyShipped();

                SetShipStatus();

                if (SelectedSaleOrder.PaymentCollection == null)
                    SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                //Set Referrence value Refund fee from config
                if (Define.CONFIGURATION.IsIncludeReturnFee || (SelectedSaleOrder.ReturnModel.ReturnFeePercent == 0 && SelectedSaleOrder.ReturnModel.ReturnFee == 0))
                    SelectedSaleOrder.ReturnModel.ReturnFeePercent = Define.CONFIGURATION.ReturnFeePercent;

                foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                {
                    saleOrderShipDetailModel.SaleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                    base_SaleOrderShipDetailModel saleOrderShipClone = saleOrderShipDetailModel.Clone();
                    saleOrderShipClone.SaleOrderDetailModel = saleOrderShipDetailModel.SaleOrderDetailModel;

                    SelectedSaleOrder.SaleOrderShipDetailCollection.Add(saleOrderShipClone);


                    //Set for return Collection
                    //Existed item SaleOrderShippedDetail in Shipped Collection
                    if (SelectedSaleOrder.SaleOrderShippedCollection.Any(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource))
                        || SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Any(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource)))
                    {
                        base_SaleOrderDetailModel saleOrderDetailModel = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                        if (saleOrderDetailModel != null)
                        {
                            saleOrderDetailModel.PickQty = SelectedSaleOrder.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty);
                            saleOrderDetailModel.SubTotal = saleOrderDetailModel.PickQty * saleOrderDetailModel.SalePrice;
                        }
                        else
                        {
                            base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                            if (saleOrderShippedRemoved != null)
                            {
                                SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                                SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
                            }
                        }
                    }
                    else
                    {
                        base_SaleOrderDetailModel saleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource)).Clone();
                        saleOrderDetailModel.PickQty = saleOrderShipDetailModel.PackedQty;
                        saleOrderDetailModel.SubTotal = saleOrderDetailModel.PickQty * saleOrderDetailModel.SalePrice;
                        SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderDetailModel);
                    }

                    //lock quantity Combobox when item is shipped
                    Guid saleOrderShipDetailResource = Guid.Parse(saleOrderShipDetailModel.SaleOrderDetailResource);
                    base_SaleOrderDetailModel lockUOM = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.Equals(saleOrderShipDetailResource));
                    if (lockUOM != null && !lockUOM.IsReadOnlyUOM)
                    {
                        lockUOM.IsReadOnlyUOM = true;
                    }

                }
                _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                SelectedSaleOrder.SetFullPayment();
                //Save SaleOrder After Shipped
                UpdateSaleOrder();
                _productRepository.Commit();
            }
            else
            {

                saleOrderShipModel.IsChecked = false;
                saleOrderShipModel.IsShipped = false;
            }
        }

        //Calculation region
        #region "Calculate Tax"

        /// <summary>
        /// Calculate Price Dependent Tax
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="subTotal"></param>
        /// <param name="taxPercent"></param>
        /// <param name="taxAmount"></param>
        private decimal CalcPriceDependentTax(base_SaleOrderModel saleOrderModel)
        {
            decimal taxAmount = 0;
            if (saleOrderModel.IsTaxExemption == true)
            {
                taxAmount = 0;
            }
            else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
            {
                base_SaleTaxLocationOptionModel saleTaxLocationOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    if (!saleOrderDetailModel.ProductModel.IsCoupon)
                        taxAmount += _saleOrderRepository.CalcPriceDependentItem(saleOrderDetailModel.SubTotal, saleOrderDetailModel.SalePrice, saleTaxLocationOptionModel);
                }
            }
            else
            {
                taxAmount = 0;
            }
            return taxAmount;
        }

        /// <summary>
        /// Calculate Tax for each other saleOrderDetail with itemprice (regular price)
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private decimal CalcMultiTaxForProduct(base_SaleOrderModel saleOrderModel)
        {
            decimal taxAmount = 0;
            if (saleOrderModel.IsTaxExemption == true)
            {
                taxAmount = 0;
            }
            else
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    if (!saleOrderDetailModel.ProductModel.IsCoupon)//18/06/2013: not calculate tax for coupon
                        taxAmount += _saleOrderRepository.CalcMultiTaxForItem(saleOrderModel.TaxLocationModel.SaleTaxLocationOptionCollection, saleOrderDetailModel.SubTotal, saleOrderDetailModel.SalePrice);
                }
            }
            return taxAmount;
        }

        /// <summary>
        /// Calculate multi, price dependent tax when sale price changed
        /// </summary>
        private void CalculateMultiNPriceTax()
        {
            if (SelectedSaleOrder.TaxLocationModel.TaxCodeModel != null)
            {
                if (Convert.ToInt32(SelectedSaleOrder.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Single))
                    return;

                if (Convert.ToInt32(SelectedSaleOrder.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Multi))
                {
                    SelectedSaleOrder.ProductTaxAmount = CalcMultiTaxForProduct(SelectedSaleOrder);
                }
                else if (Convert.ToInt32(SelectedSaleOrder.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Price))
                {
                    SelectedSaleOrder.ProductTaxAmount = CalcPriceDependentTax(SelectedSaleOrder);
                }

                SelectedSaleOrder.TaxAmount = SelectedSaleOrder.ProductTaxAmount + SelectedSaleOrder.ShipTaxAmount;
                SelectedSaleOrder.TaxPercent = 0;
            }
        }

        /// <summary>
        /// Apply Tax
        /// </summary>
        private void CalculateAllTax(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
            {
                if (saleOrderModel.SaleOrderDetailCollection.Count(x => x.ProductModel.IsCoupon) == saleOrderModel.SaleOrderDetailCollection.Count())
                {
                    saleOrderModel.ProductTaxAmount = 0;
                    saleOrderModel.TaxPercent = 0;
                }
                else
                {
                    if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Multi))
                    {
                        saleOrderModel.TaxPercent = 0;
                        saleOrderModel.ProductTaxAmount = CalcMultiTaxForProduct(saleOrderModel);
                    }
                    else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Price))
                    {
                        saleOrderModel.TaxPercent = 0;
                        saleOrderModel.ProductTaxAmount = CalcPriceDependentTax(saleOrderModel);
                        saleOrderModel.ShipTaxAmount = _saleOrderRepository.CalcShipTaxAmount(saleOrderModel);
                    }
                    else
                    {
                        decimal taxAmount = 0;
                        decimal taxPercent = 0;
                        _saleOrderRepository.CalcSingleTax(saleOrderModel, saleOrderModel.SubTotal, out taxPercent, out taxAmount);
                        saleOrderModel.ProductTaxAmount = taxAmount;
                        saleOrderModel.TaxPercent = taxPercent;
                    }
                }

            }
        }

        /// <summary>
        /// Apply Tax
        /// </summary>
        private decimal CalculateReturnTax(base_ResourceReturnModel returnModel, base_SaleOrderModel saleOrderModel)
        {
            decimal result = 0;
            if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
            {
                if (saleOrderModel.IsTaxExemption)
                {
                    result = 0;
                }
                else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Multi))
                {
                    saleOrderModel.TaxPercent = 0;

                    foreach (base_ResourceReturnDetailModel returnDetailModel in returnModel.ReturnDetailCollection.Where(x => x.IsReturned))
                    {
                        if (!returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)//18/06/2013: not calculate tax for coupon
                            result += _saleOrderRepository.CalcMultiTaxForItem(saleOrderModel.TaxLocationModel.SaleTaxLocationOptionCollection, returnDetailModel.Amount, returnDetailModel.SaleOrderDetailModel.SalePrice);
                    }
                }
                else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Price))
                {
                    saleOrderModel.TaxPercent = 0;
                    base_SaleTaxLocationOptionModel saleTaxLocationOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                    foreach (base_ResourceReturnDetailModel returnDetailModel in returnModel.ReturnDetailCollection.Where(x => x.IsReturned))
                    {
                        if (!returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                            result += _saleOrderRepository.CalcPriceDependentItem(returnDetailModel.Amount, returnDetailModel.SaleOrderDetailModel.SalePrice, saleTaxLocationOptionModel);
                    }
                }
                else
                {


                    base_SaleTaxLocationOptionModel taxOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                    if (taxOptionModel != null)
                    {
                        foreach (base_ResourceReturnDetailModel returnDetailModel in returnModel.ReturnDetailCollection.Where(x => x.IsReturned))
                        {
                            result += returnDetailModel.Amount * taxOptionModel.TaxRate / 100;
                        }
                    }
                }

            }
            return result;
        }
        #endregion

        #region Calculate Discounts
        /// <summary>
        /// Discount with Customer option in Additional. & set price if customer has additional MarkdownPriceLevel
        /// <para>when PriceSchemeId = PriceLevelType.FixedDiscountOnAllItems</para>
        /// <para>using for user change customer</para>
        /// </summary>
        private void CalcDiscountAllItemWithCustomerAdditional()
        {
            if (SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any())
            {
                if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.FixedDiscountOnAllItems))
                {
                    //"Do you want to apply customer discount?"
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ApplyCustomerDiscount"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo);
                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                        {
                            CalcDiscountWithAdditional(saleOrderDetailModel);
                        }
                    }
                }
                else if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.MarkdownPriceLevel))
                {
                    PriceSchemaChanged();
                }
                else //Calculate Promotion of POS if existed
                {
                    CalcDiscountAllItemWithPOSProgram();
                }
            }
        }

        /// <summary>
        /// Calculate All item with pos program discount
        /// </summary>
        private void CalcDiscountAllItemWithPOSProgram()
        {
            base_PromotionModel promotionModel;
            if (_promotionList.Any(x => !x.PromotionScheduleModel.ExpirationNoEndDate))//Has StartDate && EndDate
            {
                promotionModel = _promotionList.OrderByDescending(x => x.PromotionScheduleModel.StartDate).ThenBy(x => x.PromotionScheduleModel.EndDate).Where(x =>
                                                           !x.PromotionScheduleModel.ExpirationNoEndDate
                                                        && x.PromotionScheduleModel.StartDate <= SelectedSaleOrder.OrderDate
                                                        && SelectedSaleOrder.OrderDate <= x.PromotionScheduleModel.EndDate).FirstOrDefault();
                if (promotionModel == null)
                    promotionModel = _promotionList.FirstOrDefault();
            }
            else
                promotionModel = _promotionList.FirstOrDefault();

            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
            {
                if (IsAcceptedPromotion(promotionModel, saleOrderDetailModel))
                {
                    saleOrderDetailModel.PromotionId = promotionModel.Id;
                    saleOrderDetailModel.IsManual = false;

                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    BreakSODetailChange = false;
                }
                else
                {
                    _saleOrderRepository.ResetProductDiscount(SelectedSaleOrder, saleOrderDetailModel);

                    //Not any discount with user choice product
                    saleOrderDetailModel.PromotionId = 0;
                }
            }
        }

        /// <summary>
        /// Calculate discount with GuestAddtion or program
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        private void CalculateDiscount(base_SaleOrderDetailModel salesOrderDetailModel)
        {
            if (SelectedSaleOrder.GuestModel != null &&
                SelectedSaleOrder.GuestModel.AdditionalModel != null
                && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((int)PriceLevelType.FixedDiscountOnAllItems))
                CalcDiscountWithAdditional(salesOrderDetailModel);
            else
                CalcProductDiscountWithProgram(salesOrderDetailModel);
        }

        /// <summary>
        /// Calculate discount with Customer additional 
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CalcDiscountWithAdditional(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            if ((saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group) || !string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)) && saleOrderDetailModel.RegularPrice < saleOrderDetailModel.SalePrice)
            {
                saleOrderDetailModel.PromotionId = 0;
                return;
            }
            saleOrderDetailModel.IsManual = true;
            saleOrderDetailModel.PromotionId = 0;
            if (Convert.ToInt32(SelectedSaleOrder.GuestModel.AdditionalModel.Unit).Equals(0))//$
            {
                if (saleOrderDetailModel.RegularPrice > SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value)
                {
                    saleOrderDetailModel.UnitDiscount = SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value;
                    //So tien dc giam trên 1 đợn vi
                    saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.RegularPrice - saleOrderDetailModel.UnitDiscount;
                    saleOrderDetailModel.SalePrice = saleOrderDetailModel.DiscountAmount;
                    saleOrderDetailModel.DiscountPercent = Math.Round((saleOrderDetailModel.RegularPrice - saleOrderDetailModel.SalePrice) / saleOrderDetailModel.RegularPrice * 100, 2);
                    //Tổng số tiền dc giảm trên tổng số sản phẩm
                    saleOrderDetailModel.TotalDiscount = saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity;
                    _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                }
                else
                {
                    _saleOrderRepository.ResetProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                }
            }
            else //Discount with percent
            {
                //so tien giảm trên 1 đơn vi
                saleOrderDetailModel.UnitDiscount = (saleOrderDetailModel.RegularPrice * SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value / 100);
                saleOrderDetailModel.DiscountPercent = SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value;
                //So tien dc giam trên 1 đợn vi
                saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.RegularPrice - saleOrderDetailModel.UnitDiscount;
                saleOrderDetailModel.SalePrice = saleOrderDetailModel.DiscountAmount;
                //Tổng số tiền dc giảm trên tổng số sản phẩm
                saleOrderDetailModel.TotalDiscount = saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity;
                _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
            }
        }

        /// <summary>
        /// Calculate discount for product
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CalcProductDiscountWithProgram(base_SaleOrderDetailModel saleOrderDetailModel, bool resetDiscPercent = false)
        {
            //reset Discount percent Or calculate new Price when change UOM
            if (resetDiscPercent)
            {
                saleOrderDetailModel.DiscountPercent = 0;
                saleOrderDetailModel.PromotionId = 0;
                saleOrderDetailModel.UnitDiscount = Math.Round(saleOrderDetailModel.RegularPrice * saleOrderDetailModel.DiscountPercent, 2);
                saleOrderDetailModel.SalePrice = saleOrderDetailModel.RegularPrice;
                saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.SalePrice;
            }
            saleOrderDetailModel.TotalDiscount = Math.Round(saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity, 0);

            base_SaleOrderModel saleOrderModel = SelectedSaleOrder;
            var promotionList = _promotionRepository.GetAll(x => x.Status == (int)StatusBasic.Active).OrderByDescending(x => x.DateUpdated).Select(x => new base_PromotionModel(x)
            {
                PromotionScheduleModel = new base_PromotionScheduleModel(x.base_PromotionSchedule.FirstOrDefault())
                {
                    ExpirationNoEndDate = !x.base_PromotionSchedule.FirstOrDefault().StartDate.HasValue
                }
            });

            base_PromotionModel promotionModel;
            if (promotionList.Any(x => !x.PromotionScheduleModel.ExpirationNoEndDate))//Has StartDate && EndDate
            {
                promotionModel = promotionList.OrderByDescending(x => x.PromotionScheduleModel.StartDate).ThenBy(x => x.PromotionScheduleModel.EndDate).Where(x =>
                                                           !x.PromotionScheduleModel.ExpirationNoEndDate
                                                        && x.PromotionScheduleModel.StartDate <= saleOrderModel.OrderDate
                                                        && saleOrderModel.OrderDate <= x.PromotionScheduleModel.EndDate).FirstOrDefault();
                if (promotionModel == null)
                    promotionModel = promotionList.FirstOrDefault();
            }
            else
                promotionModel = promotionList.FirstOrDefault();

            if (IsAcceptedPromotion(promotionModel, saleOrderDetailModel))
            {
                saleOrderDetailModel.PromotionId = promotionModel.Id;

                BreakSODetailChange = true;
                _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                BreakSODetailChange = false;
            }
            else
                saleOrderDetailModel.PromotionId = 0;

        }

        #endregion

        /// <summary>
        /// Calculate Remain Return Quantity
        /// </summary>
        /// <param name="returnDetailModel"></param>
        private void CalculateRemainReturnQty(base_ResourceReturnDetailModel returnDetailModel, bool IsCalcAll = false)
        {
            decimal TotalItemReturn = 0;

            if (IsCalcAll)
                TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
            else
                TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.Resource.Equals(returnDetailModel.Resource) && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
            decimal remainQuantity = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource)).Sum(x => Convert.ToDecimal(x.PickQty)) - TotalItemReturn;
            returnDetailModel.ReturnQty = remainQuantity;
        }

        /// <summary>
        /// Calculate Subtotal of Return
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalculateReturnSubtotal(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.ReturnModel != null && saleOrderModel.ReturnModel.ReturnDetailCollection.Any())
            {
                //saleOrderModel.ReturnModel.SubTotal = saleOrderModel.ReturnModel.ReturnDetailCollection.Sum(x => x.Amount);
                decimal subtotal = saleOrderModel.ReturnModel.ReturnDetailCollection.Sum(x => x.Amount + x.VAT - x.RewardRedeem - ((x.Amount * saleOrderModel.DiscountPercent) / 100));
                int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
                saleOrderModel.ReturnModel.SubTotal = Math.Round(Math.Round(subtotal, decimalPlace) - 0.01M, MidpointRounding.AwayFromZero);
            }
            else
                saleOrderModel.ReturnModel.SubTotal = 0;
        }

        /// <summary>
        /// Return Detail Subtotal = Amount + VAT - rewardReedem - Discount(Order)
        /// </summary>
        /// <param name="returnDetailModel"></param>
        private void CalcReturnDetailSubTotal(base_SaleOrderModel saleOrderModel, base_ResourceReturnDetailModel returnDetailModel)
        {
            decimal subtotal = returnDetailModel.Amount + returnDetailModel.VAT - returnDetailModel.RewardRedeem - ((returnDetailModel.Amount * saleOrderModel.DiscountPercent) / 100);
            int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
            returnDetailModel.SubTotalDetail = Math.Round(Math.Round(subtotal, decimalPlace) - 0.01M, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calc Return Reward for item returned
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="returnDetailModel"></param>
        private void CalcReturnDetailRewardRedeem(base_SaleOrderModel saleOrderModel, base_ResourceReturnDetailModel returnDetailModel)
        {
            if (saleOrderModel.Total != saleOrderModel.RewardAmount && saleOrderModel.RewardAmount > 0)//Has Apply Reward
            {
                decimal rewardApply = saleOrderModel.Total - saleOrderModel.RewardAmount;
                //Calculate reward redeem with amount include tax
                decimal rewardRedeem = Math.Round(Math.Round(((returnDetailModel.Amount + returnDetailModel.VAT) * rewardApply) / saleOrderModel.Total, Define.CONFIGURATION.DecimalPlaces.Value) - 0.01M, MidpointRounding.AwayFromZero);
                returnDetailModel.RewardRedeem = rewardRedeem;
            }
        }

        //Update value
        /// <summary>
        /// Accepted Promotion
        /// </summary>
        /// <param name="promotionModel"></param>
        /// <param name="saleOrderDetailModel"></param>
        /// <returns></returns>
        private bool IsAcceptedPromotion(base_PromotionModel promotionModel, base_SaleOrderDetailModel saleOrderDetailModel)
        {
            bool result = true;
            if (promotionModel == null)
                return false;

            if (promotionModel.AffectDiscount == 0)//All Item
            {
                result = true;
            }
            else if (promotionModel.AffectDiscount == 1)//All Item Category
            {
                result = saleOrderDetailModel.ProductModel.ProductCategoryId == promotionModel.CategoryId;
            }
            else if (promotionModel.AffectDiscount == 2)//All item Vendor
            {
                result = saleOrderDetailModel.ProductModel.VendorId == promotionModel.VendorId;
            }
            else if (promotionModel.AffectDiscount == 3)//Custom 
            {
                result = promotionModel.base_Promotion.base_PromotionAffect.Any(x => x.Id == saleOrderDetailModel.ProductModel.Id);
            }
            return result && Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).In(promotionModel.PriceSchemaRange.Value) && !saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group) && string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        private void TabChanged(int saleTab)
        {
            //if (!IsDirty)
            //    return;
            bool allowChangeTab = true;
            switch (saleTab)
            {
                case (int)SaleOrderTab.Order:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                        allowChangeTab = false;
                    break;
                case (int)SaleOrderTab.Ship:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                    {
                        allowChangeTab = false;
                    }
                    else
                        if (SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsDirty) && _previousTabIndex.Is(SaleOrderTab.Order))//Change from SaleOrderTab
                        {
                            if (IsValid & IsOrderValid)
                                SaveSalesOrder();
                        }
                    break;
                case (int)SaleOrderTab.Payment:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                    {
                        allowChangeTab = false;
                    }
                    break;
                case (int)SaleOrderTab.Return:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                    {
                        allowChangeTab = false;
                    }
                    break;
            }

            if (!allowChangeTab)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //msg: notify fix error
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M107"), Language.GetMsg("POSCaption"), MessageBoxButton.OK);
                    _selectedTabIndex = _previousTabIndex;
                    OnPropertyChanged(() => SelectedTabIndex);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }


        }

        /// <summary>
        /// Return All 
        /// Set all item is shipped to return collection
        /// If exited item in return collection and item is not set returned, it will be added quantity. Otherwise create new item to return collection 
        /// </summary>
        private void ReturnAll()
        {
            if (SelectedSaleOrder.SaleOrderShipDetailCollection != null)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    if (SelectedSaleOrder.SaleOrderShipDetailCollection.Any(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())))
                    {
                        base_ResourceReturnDetailModel returnDetailModel = new base_ResourceReturnDetailModel();

                        returnDetailModel.SaleOrderDetailModel = saleOrderDetailModel;
                        returnDetailModel.OrderDetailResource = saleOrderDetailModel.Resource.ToString();
                        returnDetailModel.SaleOrderModel = SelectedSaleOrder;
                        returnDetailModel.IsParent = (returnDetailModel.SaleOrderDetailModel.ProductModel != null && returnDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group));
                        CalculateRemainReturnQty(returnDetailModel, true);
                        if (returnDetailModel.ReturnQty > 0)
                        {
                            returnDetailModel.ProductResource = saleOrderDetailModel.ProductResource;
                            returnDetailModel.ItemCode = saleOrderDetailModel.ItemCode;
                            returnDetailModel.ItemName = saleOrderDetailModel.ItemName;
                            returnDetailModel.ItemAtribute = saleOrderDetailModel.ItemAtribute;
                            returnDetailModel.ItemSize = saleOrderDetailModel.ItemSize;
                            returnDetailModel.UnitName = saleOrderDetailModel.UnitName;
                            returnDetailModel.Price = saleOrderDetailModel.SalePrice;
                            returnDetailModel.Amount = returnDetailModel.Price * returnDetailModel.ReturnQty;
                            CalcReturnQtyBaseUnit(returnDetailModel, returnDetailModel.SaleOrderDetailModel);
                            returnDetailModel.IsTemporary = false;
                            //Existed item not return & the same of SaleOrderDetailResource=>update Return Qty
                            if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsReturned && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Any())
                            {
                                base_ResourceReturnDetailModel returnDetailModelUpdate = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.SingleOrDefault(x => !x.IsReturned && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource));
                                returnDetailModelUpdate.ReturnQty += returnDetailModel.ReturnQty;
                            }
                            else
                            {
                                SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Add(returnDetailModel);
                                returnDetailModel.IsTemporary = false;
                            }

                            returnDetailModel.VAT = _saleOrderRepository.CalculateReturnDetailTax(returnDetailModel, SelectedSaleOrder);
                            CalcReturnDetailRewardRedeem(SelectedSaleOrder, returnDetailModel);
                            CalcReturnDetailSubTotal(SelectedSaleOrder, returnDetailModel);
                        }
                    }
                }
                CalculateReturnSubtotal(SelectedSaleOrder);
            }
        }

        /// <summary>
        /// Check item is return all. 
        /// if item is return all, remove collection shipped to not show in autocomplete choice Product
        /// </summary>
        private void CheckReturned()
        {
            if (SelectedSaleOrder == null)
                return;
            var allReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.SaleOrderDetailModel != null);


            foreach (var item in allReturn)
            {
                decimal totalReturn = allReturn.Where(x => x.OrderDetailResource.Equals(item.OrderDetailResource)).Sum(x => x.ReturnQty);
                decimal totalShipped = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(item.OrderDetailResource)).Sum(x => x.PickQty);
                totalShipped += SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Where(x => x.Resource.ToString().Equals(item.OrderDetailResource)).Sum(x => x.PickQty);
                if (totalShipped <= totalReturn)
                {
                    base_SaleOrderDetailModel saleOrderShippedModel = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(item.OrderDetailResource));
                    if (saleOrderShippedModel != null)
                        SelectedSaleOrder.SaleOrderShippedCollection.Remove(saleOrderShippedModel);
                }
                else
                {
                    base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(item.OrderDetailResource));
                    if (saleOrderShippedRemoved != null)
                    {
                        //add To CollectionShipped
                        SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                        //Remove In Collection DeletedItems
                        SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
                    }
                }
            }
        }

        /// <summary>
        /// Check Item Is Return All
        /// </summary>
        /// <param name="selectedReturnDetail"></param>
        private void CheckReturned(base_ResourceReturnDetailModel selectedReturnDetail)
        {
            if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.OrderDetailResource.Equals(selectedReturnDetail.OrderDetailResource)))
            {
                base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(selectedReturnDetail.OrderDetailResource));
                if (saleOrderShippedRemoved != null)
                {
                    SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                    SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
                }
                //Remove Item Returned All
                //Get Item Diffrent with Current Item Selected
                var saleOrderShipped = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => !x.Resource.ToString().Equals(selectedReturnDetail.OrderDetailResource));
                foreach (base_SaleOrderDetailModel saleOrderShippedModel in saleOrderShipped.ToList())
                {
                    decimal totalReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.SaleOrderDetailModel != null && x.SaleOrderDetailModel.Resource.Equals(saleOrderShippedModel.Resource)).Sum(x => x.ReturnQty);
                    decimal totalShipped = saleOrderShippedModel.PickQty;
                    totalShipped += SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Where(x => x.Resource.Equals(saleOrderShippedModel.Resource)).Sum(x => x.PickQty);
                    if (totalShipped <= totalReturn)
                    {
                        SelectedSaleOrder.SaleOrderShippedCollection.Remove(saleOrderShippedModel);
                    }
                }

            }
        }

        /// <summary>
        /// Store Changed
        /// </summary>
        private void StoreChanged()
        {
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in this.SelectedSaleOrder.SaleOrderDetailCollection)
            {
                SetPriceUOM(saleOrderDetailModel);

                CalculateDiscount(saleOrderDetailModel);

                _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);

                saleOrderDetailModel.CalcSubTotal();

                saleOrderDetailModel.CalcDueQty();

                saleOrderDetailModel.CalUnfill();
            }
            SelectedSaleOrder.CalcSubTotal();
        }

        /// <summary>
        /// Update price when PriceSchema Changed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void PriceSchemaChanged()
        {
            if (SelectedSaleOrder.SaleOrderDetailCollection != null)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    SetPriceUOM(saleOrderDetailModel);

                    saleOrderDetailModel.CalcSubTotal();

                    saleOrderDetailModel.CalcDueQty();

                    saleOrderDetailModel.CalUnfill();

                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    BreakSODetailChange = false;

                    _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                }
                SelectedSaleOrder.CalcSubTotal();
                SelectedSaleOrder.CalcDiscountAmount();
                //Need Calculate Total after Subtotal & Discount Percent changed
                SelectedSaleOrder.CalcTotal();
                SelectedSaleOrder.CalcBalance();
            }
        }

        /// <summary>
        /// Update Pick quatity for parent when Child of Product Group Changed qty of pick pack
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void UpdatePickQtyForParent(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))//ChildOf ProductGroup
            {
                //Get Parent Item for update
                base_SaleOrderDetailModel parentSaleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetailModel.ParentResource));
                var childGroupList = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.ParentResource));
                decimal totalQty = childGroupList.Sum(x => x.Quantity);
                decimal totalOfPick = childGroupList.Sum(x => x.PickQty);
                decimal parentPickQty = totalQty == 0 ? 0 : totalOfPick * parentSaleOrderDetailModel.Quantity / totalQty;
                parentSaleOrderDetailModel.PickQty = Math.Round(parentPickQty, 2);
            }
        }

        //Handle from another form
        /// <summary>
        /// Open form tracking serial number
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="isShowQty"></param>
        private void OpenTrackingSerialNumber(base_SaleOrderDetailModel salesOrderDetailModel, bool isShowQty = false, bool isEditing = true)
        {
            if (!salesOrderDetailModel.ProductModel.IsSerialTracking)
                return;
            //Show Tracking Serial
            SelectTrackingNumberViewModel trackingNumberViewModel = new SelectTrackingNumberViewModel(salesOrderDetailModel, isShowQty, isEditing);
            bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, trackingNumberViewModel, "Tracking Serial Number");

            if (result == true)
            {
                if (isEditing)
                {
                    salesOrderDetailModel = trackingNumberViewModel.SaleOrderDetailModel;

                    CalculateDiscount(salesOrderDetailModel);
                }
            }

        }

        /// <summary>
        /// Update price when product price is 0
        /// </summary>
        /// <param name="productModel"></param>
        private base_SaleOrderDetailModel UpdateProductPrice(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            if (saleOrderDetailModel.ProductModel.RegularPrice == 0)
            {
                UpdateTransactionViewModel updateTransactionViewModel = new UpdateTransactionViewModel(saleOrderDetailModel.ProductModel);
                bool? result = _dialogService.ShowDialog<UpdateTransactionView>(_ownerViewModel, updateTransactionViewModel, "Update Product Price");
                if (result == true)
                {
                    saleOrderDetailModel.ProductModel = updateTransactionViewModel.ProductModel;
                    if (!updateTransactionViewModel.IsUpdateProductPrice)//If user dont update to db. Set new price to saleprice & regular price still 0
                    {
                        saleOrderDetailModel.RegularPrice = updateTransactionViewModel.NewPrice;
                        saleOrderDetailModel.ProductModel.RegularPrice = updateTransactionViewModel.NewPrice;
                        saleOrderDetailModel.SalePrice = updateTransactionViewModel.NewPrice;
                    }
                    else
                    {
                        base_ProductModel productUpdate = ProductCollection.SingleOrDefault(x => x.Resource.Equals(updateTransactionViewModel.ProductModel.Resource));
                        if (productUpdate != null)
                        {
                            productUpdate.RegularPrice = updateTransactionViewModel.ProductModel.RegularPrice;
                        }
                        //Update BaseUnit & UnitCollection
                        _saleOrderRepository.GetProductUOMforSaleOrderDetail(saleOrderDetailModel);
                    }
                    return saleOrderDetailModel;
                }
            }
            return saleOrderDetailModel;
        }

        private bool UpdateProductPrice(base_ProductModel productModel)
        {
            bool resultValue = false;
            if (productModel.RegularPrice <= 0)
            {
                UpdateTransactionViewModel updateTransactionViewModel = new UpdateTransactionViewModel(productModel);
                bool? result = _dialogService.ShowDialog<UpdateTransactionView>(_ownerViewModel, updateTransactionViewModel, "Update Product Price");
                if (result == true)
                {
                    productModel.RegularPrice = updateTransactionViewModel.ProductModel.RegularPrice;
                   
                    //Update ProductColletion
                    if (updateTransactionViewModel.IsUpdateProductPrice)
                    {
                        base_ProductModel productUpdate = ProductCollection.SingleOrDefault(x => x.Resource.Equals(updateTransactionViewModel.ProductModel.Resource));
                        if (productUpdate != null)
                            productUpdate.RegularPrice = updateTransactionViewModel.ProductModel.RegularPrice;
                    }
                    resultValue = true;
                }
            }
            return resultValue;
        }

        /// <summary>
        /// Open coupon view to update Amount & Coupon Code
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void OpenCouponView(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            CouponViewModel couponViewModel = new CouponViewModel();
            couponViewModel.SaleOrderDetailModel = saleOrderDetailModel;
            bool? result = _dialogService.ShowDialog<CouponView>(_ownerViewModel, couponViewModel, "Coupon");
            _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
        }

        /// <summary>
        /// Open Sale Order or Quotation Advance Search
        /// </summary>
        private void OpenSOAdvanceSearch()
        {
            POSOAdvanceSearchViewModel viewModel = new POSOAdvanceSearchViewModel(false);
            bool? dialogResult = _dialogService.ShowDialog<POSOAdvanceSearchView>(_ownerViewModel, viewModel, "Sale Order Advance Search");

            if (dialogResult == true)
            {
                Expression<Func<base_SaleOrder, bool>> predicate = viewModel.SOPredicate;
                LoadDataByPredicate(predicate, false, 0);
            }
        }

        #endregion

        #region Propertychanged
        private void SelectedSaleOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_SaleOrderModel saleOrderModel = sender as base_SaleOrderModel;
            switch (e.PropertyName)
            {
                case "SubTotal":
                    CalculateAllTax(saleOrderModel);
                    saleOrderModel.CalcDiscountAmount();
                    break;
                case "Total":
                    saleOrderModel.CalcBalance();
                    break;
                case "RewardAmount":
                case "Deposit":
                    saleOrderModel.CalcBalance();
                    break;
                case "Paid":
                    saleOrderModel.CalcBalance();
                    break;
                case "Shipping":
                    saleOrderModel.ShipTaxAmount = _saleOrderRepository.CalcShipTaxAmount(saleOrderModel);
                    saleOrderModel.CalcTotal();
                    break;
                case "ProductTaxAmount":
                case "ShipTaxAmount":
                    if (saleOrderModel.TaxLocationModel.TaxCodeModel.IsTaxAfterDiscount)
                        saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                    else
                        saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;

                    break;
                case "TaxAmount":
                    saleOrderModel.CalcTotal();
                    break;
                case "DiscountAmount":
                    saleOrderModel.CalcDiscountPercent();
                    saleOrderModel.SkipDisc = false;
                    if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                    {
                        if (saleOrderModel.TaxLocationModel.TaxCodeModel.IsTaxAfterDiscount)
                            saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                        else
                            saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;
                    }

                    saleOrderModel.CalcTotal();
                    break;
                case "DiscountPercent":
                    saleOrderModel.CalcDiscountAmount();
                    saleOrderModel.SkipDisc = false;
                    break;
                case "PriceSchemaId"://Update Price When Price Schema Changed
                    PriceSchemaChanged();

                    saleOrderModel.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.PriceSchemaId));
                    break;
                case "OrderStatus":
                    SetAllowChangeOrder(saleOrderModel);
                    SelectedSaleOrder.SetFullPayment();

                    //Set Text Status
                    saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                    break;
                case "StoreCode":
                    StoreChanged();
                    break;
                case "TotalPaid":
                    saleOrderModel.ReturnModel.CalcBalance(saleOrderModel.TotalPaid);
                    break;


            }
        }

        private void SaleOrderDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BreakSODetailChange)
                return;
            base_SaleOrderDetailModel saleOrderDetailModel = sender as base_SaleOrderDetailModel;
            switch (e.PropertyName)
            {
                case "SalePrice":
                    saleOrderDetailModel.SalePriceChanged();
                    saleOrderDetailModel.CalcSubTotal();
                    CalculateMultiNPriceTax();
                    _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    break;
                case "Quantity":
                    //Update child quantity when parent change (apply only for Product Group)

                    if (saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                    {
                        var childInGroup = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.Resource.ToString()));
                        if (childInGroup.Any())//Is a group 
                        {
                            foreach (base_SaleOrderDetailModel saleOrderDetaiInGrouplModel in childInGroup)
                            {
                                saleOrderDetaiInGrouplModel.Quantity = saleOrderDetaiInGrouplModel.ProductGroupItem.Quantity * saleOrderDetailModel.Quantity;
                                //Update Parent Pick Qty
                                UpdatePickQtyForParent(saleOrderDetaiInGrouplModel);
                                if (saleOrderDetaiInGrouplModel.ProductModel.IsSerialTracking)
                                    OpenTrackingSerialNumber(saleOrderDetaiInGrouplModel, true, true);
                            }
                        }
                    }
                    else//Child of Product Group Change Quanity
                        UpdatePickQtyForParent(saleOrderDetailModel);
                    saleOrderDetailModel.CalcDueQty();
                    saleOrderDetailModel.CalcSubTotal();
                    if (!saleOrderDetailModel.ProductModel.IsSerialTracking)
                    {
                        BreakSODetailChange = true;
                        _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                        BreakSODetailChange = false;
                    }

                    CalculateMultiNPriceTax();
                    SelectedSaleOrder.CalcSubTotal();
                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                    SetShipStatus();
                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    break;
                case "DueQty":
                    saleOrderDetailModel.CalUnfill();
                    break;
                case "PickQty":
                    //Calc PickQty for parent if pickqty change is a child of ProductGroup
                    UpdatePickQtyForParent(saleOrderDetailModel);

                    saleOrderDetailModel.CalcDueQty();
                    break;
                case "UOMId":
                    SetPriceUOM(saleOrderDetailModel);

                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    BreakSODetailChange = false;

                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);

                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    break;
                case "SubTotal":
                    SelectedSaleOrder.CalcSubTotal();
                    break;
                case "IsQuantityAccepted":
                    if (SelectedSaleOrder.SaleOrderDetailCollection != null)
                        SelectedSaleOrder.IsHiddenErrorColumn = !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);
                    break;
            }
        }

        private void SaleOrderDetailCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base_SaleOrderDetailModel saleOrderDetailModel;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    saleOrderDetailModel = item as base_SaleOrderDetailModel;
                    saleOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    saleOrderDetailModel = item as base_SaleOrderDetailModel;
                    saleOrderDetailModel.PropertyChanged -= new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                }
            }
        }

        private void SelectedSaleOrderShip_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BoxNo":
                    SelectedSaleOrder.RaiseTotalShipBox();
                    break;
                case "Weight":
                    SelectedSaleOrder.RaiseTotalWeight();
                    break;
            }

        }

        private void ResourceReturnDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourceReturnDetailModel returnDetailModel = sender as base_ResourceReturnDetailModel;
            switch (e.PropertyName)
            {
                case "SaleOrderDetailModel":
                    if (returnDetailModel.SaleOrderDetailModel != null)
                    {
                        if (string.IsNullOrWhiteSpace(returnDetailModel.OrderDetailResource) || !returnDetailModel.SaleOrderDetailModel.ProductResource.Equals(returnDetailModel.ProductResource))
                        {
                            returnDetailModel.OrderDetailResource = returnDetailModel.SaleOrderDetailModel.Resource.ToString();
                            returnDetailModel.SaleOrderModel = SelectedSaleOrder;
                            returnDetailModel.ProductResource = returnDetailModel.SaleOrderDetailModel.ProductResource;
                            returnDetailModel.ItemCode = returnDetailModel.SaleOrderDetailModel.ItemCode;
                            returnDetailModel.ItemName = returnDetailModel.SaleOrderDetailModel.ItemName;
                            returnDetailModel.ItemAtribute = returnDetailModel.SaleOrderDetailModel.ItemAtribute;
                            returnDetailModel.ItemSize = returnDetailModel.SaleOrderDetailModel.ItemSize;
                            returnDetailModel.UnitName = returnDetailModel.SaleOrderDetailModel.UnitName;
                            returnDetailModel.Price = returnDetailModel.SaleOrderDetailModel.SalePrice;
                            //Product is Parent of goup not change quantity when return
                            returnDetailModel.IsParent = (returnDetailModel.SaleOrderDetailModel.ProductModel != null && returnDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group));
                            CalculateRemainReturnQty(returnDetailModel);
                        }
                    }
                    else
                    {
                        returnDetailModel.OrderDetailResource = null;
                        returnDetailModel.ProductResource = null;
                        returnDetailModel.ItemCode = null;
                        returnDetailModel.ItemName = null;
                        returnDetailModel.ItemAtribute = null;
                        returnDetailModel.ItemSize = null;
                        returnDetailModel.Price = 0;
                        returnDetailModel.ReturnQty = 0;
                    }
                    break;
                case "Price":
                    returnDetailModel.Amount = returnDetailModel.Price * returnDetailModel.ReturnQty;
                    break;
                case "ReturnQty":
                    //resourceReturnDetailModel.SaleOrderDetailModel.ProductModel.
                    returnDetailModel.Amount = returnDetailModel.Price * returnDetailModel.ReturnQty;
                    base_SaleOrderDetailModel saleOrderDetail = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource));
                    if (saleOrderDetail != null && SelectedSaleOrder != null)
                    {
                        decimal TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
                        var remainQuantity = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.PickQty) - TotalItemReturn;
                        saleOrderDetail.QtyAfterRerturn = remainQuantity;

                        CalcReturnQtyBaseUnit(returnDetailModel, saleOrderDetail);
                        CheckReturned(returnDetailModel);
                    }
                    break;

                case "Amount":
                    returnDetailModel.VAT = _saleOrderRepository.CalculateReturnDetailTax(returnDetailModel, SelectedSaleOrder);
                    CalcReturnDetailRewardRedeem(SelectedSaleOrder, returnDetailModel);
                    CalcReturnDetailSubTotal(SelectedSaleOrder, returnDetailModel);
                    CalculateReturnSubtotal(SelectedSaleOrder);
                    break;

                case "IsReturned":
                    if (returnDetailModel.IsReturned)
                    {
                        if (!returnDetailModel.HasError)
                        {
                            //"Are you sure you return this item ?"
                            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M110"), Language.GetMsg("POSCaption"), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    returnDetailModel.IsReturned = false;
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                returnDetailModel.IsReturned = false;

                                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M111"), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                        SelectedSaleOrder.ReturnModel.RaiseRefundAccepted();
                    }
                    break;

            }
        }

        private void CalcReturnQtyBaseUnit(base_ResourceReturnDetailModel resourceReturnDetailModel, base_SaleOrderDetailModel saleOrderDetail)
        {
            //Get BaseUnit & convert value to Qty to BaseUnit for ReturnQtyUOM

            base_ProductUOMModel productUomModel = null;
            if (saleOrderDetail.ProductUOMCollection != null)
                productUomModel = saleOrderDetail.ProductUOMCollection.Single(x => !saleOrderDetail.ProductModel.IsCoupon && x.UOMId.Equals(saleOrderDetail.UOMId));
            if (productUomModel != null)
            {
                decimal quantityBaseUnit = productUomModel.BaseUnitNumber * resourceReturnDetailModel.ReturnQty;
                //Update To ReturnQtyUOM
                resourceReturnDetailModel.ReturnQtyUOM = quantityBaseUnit;
            }
            else
                resourceReturnDetailModel.ReturnQtyUOM = resourceReturnDetailModel.ReturnQty;
        }

        private void ReturnModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourceReturnModel returnModel = sender as base_ResourceReturnModel;
            switch (e.PropertyName)
            {
                case "TotalRefund":
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "SubTotal":
                    returnModel.CalcReturnFee();
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "ReturnFee":
                    //returnModel.SetRefundedFeePercent();
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "ReturnFeePercent":
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
            }
        }

        private void ReturnDetailCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base_ResourceReturnDetailModel resourceReturnDetail;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.ReturnedDate = DateTime.Now;
                    resourceReturnDetail.IsTemporary = true;
                    resourceReturnDetail.IsDirty = false;
                    resourceReturnDetail.PropertyChanged += ResourceReturnDetailModel_PropertyChanged;
                }
                CheckReturned();
                SelectedSaleOrder.ReturnModel.RaiseRefundAccepted();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.PropertyChanged -= ResourceReturnDetailModel_PropertyChanged;
                }
                //CheckReturned();
            }
        }

        #endregion

        #region Override Methods

        public override void LoadData()
        {
            //Flag When Existed view Call LoadDynamicData Data
            if (_viewExisted)
                LoadDynamicData();

            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            LoadDataByPredicate(predicate);
            _viewExisted = true;


        }

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ChangeViewExecute(isClosing);
        }

        /// <summary>
        /// Change view from Ribbon
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ChangeViewExecute(null))
                {
                    if (!isList)
                    {
                        CreateNewSaleOrder();
                        IsSearchMode = false;
                        IsForceFocused = true;
                    }
                    else
                        IsSearchMode = true;
                }
            }
            else
            {
                if (param is ComboItem)
                {
                    SaleOrderSelectedTab = SaleOrderTab.Order;
                    ComboItem cmbValue = param as ComboItem;
                    if (cmbValue.Text.Equals("Quotation") || cmbValue.Text.Equals(MarkType.WorkOrder.ToDescription()))
                    {
                        IsSearchMode = false;
                        SaleOrderId = Convert.ToInt32(cmbValue.Detail);
                        _selectedSaleOrder = null;
                    }
                    else if (cmbValue.Text.Equals("UnLock"))
                    {
                        IsSearchMode = true;
                        SaleOrderId = Convert.ToInt32(cmbValue.Detail);
                        _selectedSaleOrder = null;
                        OnPropertyChanged(() => SelectedSaleOrder);
                        IsSearchMode = false;
                    }
                    else if (cmbValue.Text.Equals("Customer"))//Create SaleOrder With Customer
                    {
                        CreateNewSaleOrder();
                        decimal customerId = Convert.ToInt64(cmbValue.Detail);
                        SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Id.Equals(customerId));
                        this.IsSearchMode = false;
                    }
                    else if (cmbValue.Text.Equals("SaleOrderReturn.New"))
                    {
                        CreateNewSaleOrder();
                        this.IsSearchMode = false;
                        SaleOrderSelectedTab = SaleOrderTab.Return;
                    }
                    else if (cmbValue.Text.Equals("SaleOrderReturn.SaleOrderList"))
                    {
                        this.IsSearchMode = true;
                    }
                    else if (cmbValue.Text.Equals("SaleOrderReturn.SelectedItem"))
                    {
                        SaleOrderId = Convert.ToInt32(cmbValue.Detail);
                        _selectedSaleOrder = null;
                        this.IsSearchMode = false;
                        SaleOrderSelectedTab = SaleOrderTab.Return;
                    }
                    //when View Active =>  LoadData methods will be loaded again
                    bool saleOrderActived = (_ownerViewModel as MainViewModel).IsActiveView("SalesOrder");
                    if (saleOrderActived)
                    {
                        LoadData();
                    }

                }
                else //Create saleOrder with ProductCollection
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CreateNewSaleOrder();
                        IEnumerable<base_ProductModel> productCollection = param as IEnumerable<base_ProductModel>;
                        CreateSaleOrderDetailWithProducts(productCollection);
                        this.IsSearchMode = false;
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        #endregion

        #region Permission

        #region Properties

        private bool _allowSOShipping = true;
        /// <summary>
        /// Gets or sets the AllowSOShipping.
        /// </summary>
        public bool AllowSOShipping
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return _allowSOShipping;
                return _allowSOShipping && SelectedSaleOrder.ShipProcess;
            }
            set
            {
                if (_allowSOShipping != value)
                {
                    _allowSOShipping = value;
                    OnPropertyChanged(() => AllowSOShipping);
                }
            }
        }

        private bool _allowSOReturn = true;
        /// <summary>
        /// Gets or sets the AllowSOReturn.
        /// </summary>
        public bool AllowSOReturn
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return _allowSOReturn;
                return _allowSOReturn && SelectedSaleOrder.ShipProcess;
            }
            set
            {
                if (_allowSOReturn != value)
                {
                    _allowSOReturn = value;
                    OnPropertyChanged(() => AllowSOReturn);
                }
            }
        }

        private bool _allowAddCustomer = true;
        /// <summary>
        /// Gets or sets the AllowAddCustomer.
        /// </summary>
        public bool AllowAddCustomer
        {
            get { return _allowAddCustomer; }
            set
            {
                if (_allowAddCustomer != value)
                {
                    _allowAddCustomer = value;
                    OnPropertyChanged(() => AllowAddCustomer);
                }
            }
        }

        private bool _allowDeleteProduct = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProduct.
        /// </summary>
        public bool AllowDeleteProduct
        {
            get { return _allowDeleteProduct; }
            set
            {
                if (_allowDeleteProduct != value)
                {
                    _allowDeleteProduct = value;
                    OnPropertyChanged(() => AllowDeleteProduct);
                }
            }
        }

        #endregion

        /// <summary>
        /// Get permissions
        /// </summary>
        public override void GetPermission()
        {
            if (!IsAdminPermission && !IsFullPermission)
            {
                // Get all user rights
                IEnumerable<string> userRightCodes = Define.USER_AUTHORIZATION.Select(x => x.Code);

                // Get sale order shipping permission
                AllowSOShipping = userRightCodes.Contains("SO100-04-11");

                // Get sale order return permission
                AllowSOReturn = userRightCodes.Contains("SO100-04-05");

                // Get add/copy customer permission
                AllowAddCustomer = userRightCodes.Contains("SO100-01-01");

                // Get delete product in sale order permission
                AllowDeleteProduct = userRightCodes.Contains("SO100-04-13");
            }
        }

        #endregion
    }
}