using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows;
using CPC.POS.Repository;
using CPC.POS.Model;
using CPC.POS.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System;
using System.Linq;
using CPCToolkitExtLibraries;
using System.Collections.ObjectModel;
using CPC.POS.View;
using CPC.Helper;
using System.Windows.Data;
using System.Data.Objects.SqlClient;
using System.Collections.Specialized;
using CPCToolkitExt.DataGridControl;

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
        private base_ResourcePaymentProductRepository _paymentProductRepository = new base_ResourcePaymentProductRepository();
        private string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        private string EMPLOYEE_MARK = MarkType.Employee.ToDescription();

        private bool _flagCustomerSetRelate = true;
        private bool _initialData = false;

        // Declare payment product enumerable
        private IEnumerable<base_ResourcePaymentProductModel> _paymentProducts = null;

        /// <summary>
        /// Using for viewQuotation
        /// </summary>
        public bool IsQuotation { get; set; }

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

            InitialCommand();
            LoadStaticData();
        }

        public SalesOrderViewModel(bool isList, object param)
            : this()
        {
            ChangeSearchMode(isList, param);
        }

        /// <summary>
        /// Constructor for quotation
        /// </summary>
        /// <param name="isList"></param>
        /// <param name="quotation"></param>
        public SalesOrderViewModel(bool isList, bool quotation = false)
            : this()
        {
            IsQuotation = quotation;
            ChangeSearchMode(isList, null);
        }

        #endregion

        #region Properties

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

                return (SelectedSaleOrder.SaleOrderDetailCollection != null && !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsError));
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
                    //if (SelectedSaleOrderDetail != null)
                    //{
                    //    SelectedSaleOrderDetail.PropertyChanged -= new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                    //    SelectedSaleOrderDetail.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                    //}
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

                ComboItem item = Common.BookingChannel.SingleOrDefault(x => x.Value == SelectedSaleOrder.BookingChanel);

                if (!item.Flag)//False : has commission
                    SaveSaleCommission(SelectedSaleOrder);
                else
                    DeleteSaleCommission(SelectedSaleOrder);

                if (IsQuotation)//Save Payment when customer Deposit
                {
                    if (SelectedSaleOrder.PaymentModel != null && SelectedSaleOrder.PaymentModel.IsDirty)
                    {
                        SelectedSaleOrder.PaymentModel.DocumentNo = SelectedSaleOrder.SONumber;
                        SelectedSaleOrder.PaymentModel.DocumentResource = SelectedSaleOrder.Resource.ToString();
                        SelectedSaleOrder.PaymentModel.Remark = "SaleOrder";
                        _paymentRepository.SavePayment(SelectedSaleOrder.PaymentModel);
                        SelectedSaleOrder.PaymentModel = null;
                    }
                }
                UpdateCustomer(SelectedSaleOrder);
                UnitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                result = false;
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
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
            return SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Open) || SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote);
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            if (SelectedSaleOrder != null)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to delete this item?", "POS", MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.Yes))
                {
                    SelectedSaleOrder.IsPurge = true;
                    SaveSalesOrder();
                    this.SaleOrderCollection.Remove(SelectedSaleOrder);
                    IsSearchMode = true;
                }

            }
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

        #region PrintCommand
        /// <summary>
        /// Gets the Print Command.
        /// <summary>
        public RelayCommand PrintCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Print command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPrintCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Print command is executed.
        /// </summary>
        private void OnPrintCommandExecute()
        {
            SaleOrderPayment();
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

                SelectedTabIndex = (int)SaleOrderTab.Order;
                //Set for selectedCustomer
                _flagCustomerSetRelate = false;
                SelectedCustomer = SelectedSaleOrder.GuestModel;

                SetAllowChangeOrder(SelectedSaleOrder);
                SelectedSaleOrder.IsDirty = false;
                _flagCustomerSetRelate = true;

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
            PopupAddressViewModel addressViewModel = new PopupAddressViewModel();

            string strTitle = addressModel.AddressTypeId.Is(AddressType.Billing) ? "Bill Address" : "Ship Address";
            addressViewModel.AddressModel = addressModel;
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
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteSaleOrderDetail command is executed.
        /// </summary>
        private void OnDeleteSaleOrderDetailCommandExecute(object param)
        {
            base_SaleOrderDetailModel saleOrderDetailModel = param as base_SaleOrderDetailModel;
            if (saleOrderDetailModel.PickQty == 0 && IsAllowChangeOrder)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.Yes))
                {
                    SelectedSaleOrder.SaleOrderDetailCollection.Remove(saleOrderDetailModel);
                    CalcProductDiscount(saleOrderDetailModel);
                    SelectedSaleOrder.CalcSubTotal();
                }
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
            if (param != null && Convert.ToInt32(param) != SelectedSaleOrderDetail.Quantity)
            {
                SelectedSaleOrderDetail.Quantity = Convert.ToInt32(param);
                if (SelectedSaleOrderDetail.ProductModel != null && SelectedSaleOrderDetail.ProductModel.IsSerialTracking)
                    if (!SelectedSaleOrderDetail.IsError && SelectedSaleOrderDetail.Quantity > 0)
                        OpenTrackingSerialNumber(SelectedSaleOrderDetail);
                    else
                        SelectedSaleOrderDetail.SerialTracking = string.Empty;
                SelectedSaleOrder.CalcSubTotal();
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
            short quoteStatus = (short)SaleOrderStatus.Quote;
            if (IsQuotation)
                predicate = predicate.And(x => x.OrderStatus == quoteStatus);
            else
                predicate = predicate.And(x => x.OrderStatus != quoteStatus);

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
            return SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.DueQty > 0);
        }


        /// <summary>
        /// Method to invoke when the PickPack command is executed.
        /// </summary>
        private void OnPickPackCommandExecute(object param)
        {
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteSaleOrderShip command is executed.
        /// </summary>
        private void OnDeleteSaleOrderShipCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            if (saleOrderShipModel.IsShipped == true)
            {
                MessageBox.Show("This item is shipped, can't delete ?", "POS", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result.Is(MessageBoxResult.Yes))
                {
                    if (saleOrderShipModel.SaleOrderShipDetailCollection != null)
                    {
                        //UpdatePickQty
                        foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                        {
                            Guid saleOrderDetailResource = Guid.Parse(saleOrderShipDetailModel.SaleOrderDetailResource);
                            var saleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == saleOrderDetailResource);
                            saleOrderDetailModel.PickQty -= saleOrderShipDetailModel.PackedQty.Value;
                        }
                        SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                        SelectedSaleOrder.RaiseTotalPackedBox();
                    }
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
            var listSaleOrderDetail = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => listSaleOrder.Contains(x.Resource.ToString())).ToList();

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
            MessageBoxResult result = MessageBox.Show("Do you want to ship?", "POS", MessageBoxButton.YesNo);
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            if (result.Is(MessageBoxResult.Yes))
            {
                saleOrderShipModel.IsShipped = saleOrderShipModel.IsChecked;
                SelectedSaleOrder.ShippedBox = Convert.ToInt16(SelectedSaleOrder.SaleOrderShipCollection.Count(x => x.IsShipped.HasValue && x.IsShipped.Value));
                SelectedSaleOrder.RaiseAnyShipped();
                SetShipStatus();
                UpdateSaleOrder();
                if (SelectedSaleOrder.PaymentCollection == null)
                    SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                {
                    base_SaleOrderShipDetailModel saleOrderShipClone = saleOrderShipDetailModel.Clone();
                    saleOrderShipClone.SaleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                    SelectedSaleOrder.SaleOrderShipDetailCollection.Add(saleOrderShipClone);
                    _productRepository.UpdateOnHandQuantity(saleOrderShipDetailModel.ProductResource, SelectedSaleOrder.StoreCode, saleOrderShipDetailModel.PackedQty.Value, true);
                    //Set for return Collection
                    if (SelectedSaleOrder.SaleOrderShippedCollection.Any(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource))
                        || SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Any(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource)))
                    {
                        base_SaleOrderDetailModel saleOrderDetailModel = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                        if (saleOrderDetailModel != null)
                        {
                            saleOrderDetailModel.PickQty = SelectedSaleOrder.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty.Value);
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
                        saleOrderDetailModel.PickQty = saleOrderShipDetailModel.PackedQty.Value;
                        saleOrderDetailModel.SubTotal = saleOrderDetailModel.PickQty * saleOrderDetailModel.SalePrice;
                        SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderDetailModel);
                    }
                }

                // Display payment tab when one product is shipped
                if (!SelectedSaleOrder.PaymentProcess)
                    SelectedSaleOrder.PaymentProcess = true;
                LoadPaymentProductCollection(SelectedSaleOrder);

                // Update payment subtotal
                SelectedSaleOrder.PaymentSubTotal = SelectedSaleOrder.PaymentProductCollection.Sum(x => x.Amount);

                // Update payment total
                SelectedSaleOrder.PaymentTotal = SelectedSaleOrder.PaymentSubTotal - SelectedSaleOrder.PaymentDiscount + SelectedSaleOrder.PaymentTax + SelectedSaleOrder.PaymentShipping;

                _productRepository.Commit();
            }
            else
            {
                saleOrderShipModel.IsChecked = false;
                saleOrderShipModel.IsShipped = false;
            }
        }
        #endregion

        #region ShipProcessCommand
        /// <summary>
        /// Gets the ShipProcess Command.
        /// <summary>

        public RelayCommand<object> ShipProcessCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ShipProcess command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShipProcessCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;

            return (!SelectedSaleOrder.IsNew && SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any());
        }


        /// <summary>
        /// Method to invoke when the ShipProcess command is executed.
        /// </summary>
        private void OnShipProcessCommandExecute(object param)
        {
            ShowShipTab();
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary>
        private void OnPaymentCommandExecute(object param)
        {
            if (IsQuotation)
                DepositProcess();
            else
                SaleOrderPayment();
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
            if (IsQuotation)
                return datagrid.SelectedItems.Count > 0;
            else
                return !datagrid.SelectedItems.Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Open));
        }


        /// <summary>
        /// Method to invoke when the DeleteItems command is executed.
        /// </summary>
        private void OnDeleteItemsCommandExecute(object param)
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete these items?", "POS", MessageBoxButton.YesNo);

            if (msgResult.Is(MessageBoxResult.No))
                return;
            DataGridControl datagrid = param as DataGridControl;

            foreach (base_SaleOrderModel saleOrderModel in datagrid.SelectedItems.Cast<base_SaleOrderModel>().ToList())
            {
                saleOrderModel.IsPurge = true;
                saleOrderModel.ToEntity();
                _saleOrderRepository.Commit();
                SaleOrderCollection.Remove(saleOrderModel);
            }
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

            foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderSource.SaleOrderDetailCollection)
            {
                base_SaleOrderDetailModel newSaleOrderDetailModel = new base_SaleOrderDetailModel();
                newSaleOrderDetailModel.Resource = Guid.NewGuid();
                newSaleOrderDetailModel.CopyFrom(saleOrderDetailModel);
                newSaleOrderDetailModel.CalcDueQty();
                newSaleOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                SelectedSaleOrder.SaleOrderDetailCollection.Add(newSaleOrderDetailModel);
            }

            SaveSalesOrder();

            SelectedTabIndex = (int)SaleOrderTab.Order;
            _selectedCustomer = null;
            //Set for selectedCustomer
            _flagCustomerSetRelate = false;
            SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));

            SetAllowChangeOrder(SelectedSaleOrder);
            SelectedSaleOrder.IsDirty = false;
            _flagCustomerSetRelate = true;

            IsSearchMode = false;

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
            return true;
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteReturnDetail command is executed.
        /// </summary>
        private void OnDeleteReturnDetailCommandExecute(object param)
        {
            base_ResourceReturnDetailModel returnDetailModel = SelectedReturnDetail as base_ResourceReturnDetailModel;
            if (SelectedReturnDetail == null || returnDetailModel.IsTemporary)
                return;

            if (returnDetailModel.IsReturned)
            {
                MessageBox.Show("Item has been returned in this purchase order, can not delete this item.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Try to find ResourceReturnDetail error.
            base_ResourceReturnDetailModel resourceReturnDetailError = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.FirstOrDefault(x => x.HasError);
            bool isContainsErrorItem = false;
            if (resourceReturnDetailError != null)
            {
                ListCollectionView resourceReturnDetailView = CollectionViewSource.GetDefaultView(SelectedSaleOrder.ReturnModel.ReturnDetailCollection) as ListCollectionView;
                if (resourceReturnDetailView != null)
                {
                    if (resourceReturnDetailView.CurrentEditItem != null)
                    {
                        isContainsErrorItem = object.ReferenceEquals(resourceReturnDetailView.CurrentEditItem, returnDetailModel);
                    }
                }
            }

            if (resourceReturnDetailError == null || isContainsErrorItem)
            {

                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item(s)?", "Delete item(s)", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Remove(returnDetailModel);
                    CalculateReturnSubtotal(SelectedSaleOrder);
                }
            }



        }
        #endregion

        ///Quotation Region
        #region ConvertToSaleOrderCommand

        /// <summary>
        /// Gets the ConvertToSaleOrder Command.
        /// <summary>
        public RelayCommand<object> ConvertToSaleOrderCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ConvertToSaleOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnConvertToSaleOrderCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew && IsValid && IsOrderValid;
        }

        /// <summary>
        /// Method to invoke when the ConvertToSaleOrder command is executed.
        /// </summary>
        private void OnConvertToSaleOrderCommandExecute(object param)
        {
            SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
            SaveSalesOrder();
            IsSearchMode = true;
            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", SelectedSaleOrder);
        }

        #endregion

        #region ConvertItemToSaleOrderCommand
        /// <summary>
        /// Gets the ConvertItemToSaleOrder Command.
        /// Using on selected in datagrid
        /// <summary>

        public RelayCommand<object> ConvertItemToSaleOrderCommand { get; private set; }


        /// <summary>
        /// Method to check whether the ConvertItemToSaleOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnConvertItemToSaleOrderCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count == 1;
        }


        /// <summary>
        /// Method to invoke when the ConvertItemToSaleOrder command is executed.
        /// </summary>
        private void OnConvertItemToSaleOrderCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            _selectedSaleOrder = datagrid.SelectedItem as base_SaleOrderModel;
            SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
            SaveSalesOrder();
            IsSearchMode = true;
            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", SelectedSaleOrder);
        }
        #endregion

        #endregion "Commands Methods"

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        private void InitialCommand()
        {
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            PrintCommand = new RelayCommand(OnPrintCommandExecute, OnPrintCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            AddressPopupCommand = new RelayCommand<object>(OnAddressPopupCommandExecute, OnAddressPopupCommandCanExecute);
            DeleteSaleOrderDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
            AddNewCustomerCommand = new RelayCommand<object>(OnAddNewCustomerCommandExecute, OnAddNewCustomerCommandCanExecute);
            QtyChangedCommand = new RelayCommand<object>(OnQtyChangedCommandExecute, OnQtyChangedCommandCanExecute);
            LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            AddTermCommand = new RelayCommand(OnAddTermCommandExecute, OnAddTermCommandCanExecute);
            SearchProductAdvanceCommand = new RelayCommand<object>(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
            PickPackCommand = new RelayCommand<object>(OnPickPackCommandExecute, OnPickPackCommandCanExecute);
            DeleteSaleOrderShipCommand = new RelayCommand<object>(OnDeleteSaleOrderShipCommandExecute, OnDeleteSaleOrderShipCommandCanExecute);
            EditSaleOrderShipCommand = new RelayCommand<object>(OnEditSaleOrderShipCommandExecute, OnEditSaleOrderShipCommandCanExecute);
            ViewPnPDetailCommand = new RelayCommand<object>(OnViewPnPDetailCommandExecute, OnViewPnPDetailCommandCanExecute);
            ShippedCommand = new RelayCommand<object>(OnShippedCommandExecute, OnShippedCommandCanExecute);
            ShipProcessCommand = new RelayCommand<object>(OnShipProcessCommandExecute, OnShipProcessCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);

            //Using for Main Datagrid
            DeleteItemsCommand = new RelayCommand<object>(OnDeleteItemsCommandExecute, OnDeleteItemsCommandCanExecute);
            DuplicateItemCommand = new RelayCommand<object>(OnDuplicateItemCommandExecute, OnDuplicateItemCommandCanExecute);
            EditItemCommand = new RelayCommand<object>(OnEditItemCommandExecute, OnEditItemCommandCanExecute);

            //Return
            ReturnAllCommand = new RelayCommand<object>(OnReturnAllCommandExecute, OnReturnAllCommandCanExecute);
            DeleteReturnDetailCommand = new RelayCommand<object>(OnDeleteReturnDetailCommandExecute, OnDeleteReturnDetailCommandCanExecute);

            //Quotation
            ConvertToSaleOrderCommand = new RelayCommand<object>(OnConvertToSaleOrderCommandExecute, OnConvertToSaleOrderCommandCanExecute);
            ConvertItemToSaleOrderCommand = new RelayCommand<object>(OnConvertItemToSaleOrderCommandExecute, OnConvertItemToSaleOrderCommandCanExecute);

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
                msgResult = MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        //if (SaveCustomer())
                        result = SaveSalesOrder();
                    }
                    else //Has Error
                        result = false;
                }
                else
                {
                    if (SelectedSaleOrder.IsNew)
                    {
                        SelectedSaleOrder = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else //Old Item Rollback data
                    {
                        SelectedSaleOrder.ToModelAndRaise();
                        SetSaleOrderToModel(SelectedSaleOrder);
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

            CustomerCollection = new CollectionBase<base_GuestModel>(_guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && !x.IsPurged && x.IsActived).Select(x => new base_GuestModel(x)
            {

            }
                ).OrderBy(x => x.Id));
            //Get Employee
            EmployeeCollection = new ObservableCollection<base_GuestModel>(_guestRepository.GetAll(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived).Select(x => new base_GuestModel(x)));
            EmployeeCollection.Insert(0, new base_GuestModel() { Id = 0 });

            //Get Store
            StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));

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

            //Load AllProduct
            ProductCollection = new ObservableCollection<base_ProductModel>(_productRepository.GetAll().Select(x => new base_ProductModel(x)));

            //Load All Sale Tax
            SaleTaxLocationCollection = new List<base_SaleTaxLocationModel>(_saleTaxRepository.GetAll().Select(x => new base_SaleTaxLocationModel(x)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex"></param>
        private void LoadDataByPredicate(Expression<Func<base_SaleOrder, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                SaleOrderCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                IsBusy = true;
                //Cout all SaleOrder in Data base show on grid
                TotalSaleOrder = _saleOrderRepository.GetIQueryable(predicate).Count();
                //Get data with range
                IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                if (refreshData)
                    _saleOrderRepository.Refresh(saleOrders);
                foreach (base_SaleOrder saleOrder in saleOrders)
                {
                    bgWorker.ReportProgress(0, saleOrder);
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
            predicate = predicate.And(x => !x.IsPurge);
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
        /// <param name="saleOrderModel"></param>
        private void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            _initialData = true;

            //Get CustomerModel
            saleOrderModel.GuestModel = CustomerCollection.Where(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)).FirstOrDefault();

            //Get GuestReward collection
            saleOrderModel.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
            foreach (base_GuestReward guestReward in saleOrderModel.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(saleOrderModel.GuestModel.Id) && !x.IsApply))
            {
                int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == guestReward.base_RewardManager.RewardExpiration).Detail);
                DateTime expireDate = guestReward.EearnedDate.Value.AddDays(expireDay);
                if (expireDate.Date >= DateTime.Today)
                {
                    saleOrderModel.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestReward));
                }
            }

            //Get Reward & set PurchaseThreshold if Customer any reward
            DateTime orderDate = saleOrderModel.OrderDate.Value.Date;
            var reward = _rewardManagerRepository.Get(x =>
                                         x.IsActived && saleOrderModel.SubTotal >= x.PurchaseThreshold
                                         && ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                         || !x.IsTrackingPeriod)));
            if (!saleOrderModel.GuestModel.GuestRewardCollection.Any() && reward != null)
                saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

            //Get TaxCode
            saleOrderModel.TaxCodeModel = GetTaxCode(saleOrderModel.TaxLocation, saleOrderModel.TaxCode);


            //Get TaxLocation
            saleOrderModel.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == saleOrderModel.TaxLocation);

            //Using for Calc Tax for Shipping if Tax is Price
            if (saleOrderModel.TaxLocationModel != null
                && saleOrderModel.TaxCodeModel != null
                && saleOrderModel.TaxLocationModel.IsShipingTaxable)
            {
                saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
                saleOrderModel.ProductTaxAmount = saleOrderModel.TaxAmount - saleOrderModel.ShipTaxAmount;
            }

            //Check Deposit is accepted?
            saleOrderModel.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit

            #region Get & Set SaleOrderDetail
            //Load sale order detail
            saleOrderModel.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
            foreach (base_SaleOrderDetail saleOrderDetail in saleOrderModel.base_SaleOrder.base_SaleOrderDetail)
            {
                base_SaleOrderDetailModel saleOrderDetailModel = new base_SaleOrderDetailModel(saleOrderDetail);
                saleOrderDetailModel.Qty = saleOrderDetailModel.Quantity;
                saleOrderDetailModel.ProductModel = ProductCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetail.ProductResource));
                //Get VendorName
                base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == saleOrderDetailModel.ProductModel.VendorId));
                if (vendorModel != null)
                    saleOrderDetailModel.ProductModel.VendorName = vendorModel.LegalName;
                //Check RowDetail Visibility
                CheckToShowDatagridRowDetail(saleOrderDetailModel);
                saleOrderDetailModel.UOMId = -1;//Set UOM -1 because UOMCollection is Empty => UOMId not raise change after UOMCollection created
                GetProductUOMforSaleOrderDetail(saleOrderDetailModel, false);
                saleOrderDetailModel.UOMId = saleOrderDetail.UOMId;
                saleOrderDetailModel.UnitName = saleOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == saleOrderDetailModel.UOMId).Name;
                saleOrderDetailModel.UnitDiscount = Math.Round(saleOrderDetailModel.RegularPrice * saleOrderDetailModel.DiscountPercent / 100, 2);
                saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.SalePrice;
                saleOrderDetailModel.TotalDiscount = Math.Round(saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity, 2);
                saleOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                saleOrderDetailModel.IsDirty = false;
                saleOrderModel.SaleOrderDetailCollection.Add(saleOrderDetailModel);
            }
            #endregion

            #region Get & Set data for Ship
            //Collection Sale Order Ship
            saleOrderModel.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();

            foreach (base_SaleOrderShip saleOrderShip in saleOrderModel.base_SaleOrder.base_SaleOrderShip)
            {
                base_SaleOrderShipModel saleOrderShipModel = new base_SaleOrderShipModel(saleOrderShip);
                saleOrderShipModel.IsChecked = saleOrderShipModel.IsShipped.HasValue ? saleOrderShipModel.IsShipped.Value : false;
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
            //Check Show ShipTab
            saleOrderModel.ShipProcess = saleOrderModel.OrderStatus > 1;//saleOrderModel.SaleOrderShipCollection.Any();
            #endregion

            #region Get & Set Return
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

            saleOrderModel.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
            saleOrderModel.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;

            foreach (base_ResourceReturnDetail resourceReturnDetail in saleOrderModel.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail)
            {
                base_ResourceReturnDetailModel returnDetailModel = new base_ResourceReturnDetailModel(resourceReturnDetail);
                returnDetailModel.SaleOrderModel = saleOrderModel;
                returnDetailModel.SaleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource));
                saleOrderModel.ReturnModel.ReturnDetailCollection.Add(returnDetailModel);
                returnDetailModel.IsDirty = false;
                returnDetailModel.IsTemporary = false;
            }

            //Get SaleOrderShipDetail for return
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
                if (saleOrderModel.SaleOrderShipDetailCollection.Any(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())))
                {
                    base_SaleOrderDetailModel saleOrderShipModel = saleOrderDetailModel.Clone();
                    saleOrderShipModel.IsNew = false;
                    saleOrderShipModel.PickQty = saleOrderModel.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty.Value);
                    saleOrderShipModel.SubTotal = saleOrderShipModel.PickQty * saleOrderShipModel.SalePrice;
                    saleOrderModel.SaleOrderShippedCollection.Add(saleOrderShipModel);
                }
            }

            // Update payment subtotal
            saleOrderModel.PaymentSubTotal = saleOrderModel.SaleOrderShippedCollection.Sum(x => x.SubTotal);

            // Update payment total
            saleOrderModel.PaymentTotal = saleOrderModel.PaymentSubTotal - saleOrderModel.PaymentDiscount + saleOrderModel.PaymentTax + saleOrderModel.PaymentShipping;
            #endregion

            #region Load data for payment

            // Check show PaymentTab
            saleOrderModel.PaymentProcess = saleOrderModel.SaleOrderShipCollection.Any(x => x.IsShipped == true);

            // Load payment collection
            if (saleOrderModel.PaymentProcess)
            {
                LoadPaymentCollection(saleOrderModel);
            }

            #endregion

            //Set Address
            SetBillShipAddress(saleOrderModel.GuestModel, saleOrderModel);
            saleOrderModel.RaiseAnyShipped();
            _initialData = false;
            saleOrderModel.IsDirty = false;
        }

        /// <summary>
        /// 
        /// </summary>
        private base_SaleOrderModel CreateNewSaleOrder()
        {
            SelectedSaleOrder = new base_SaleOrderModel();
            SelectedSaleOrder.IsTaxExemption = false;
            SelectedSaleOrder.SONumber = DateTime.Now.ToString(Define.GuestNoFormat);
            SelectedSaleOrder.DateCreated = DateTime.Now;
            SelectedSaleOrder.BookingChanel = Convert.ToInt16(Common.BookingChannel.First().ObjValue);
            SelectedSaleOrder.StoreCode = Define.StoreCode;//Default StoreCode
            SelectedSaleOrder.OrderDate = DateTime.Now;
            SelectedSaleOrder.RequestShipDate = DateTime.Now;
            SelectedSaleOrder.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedSaleOrder.TaxPercent = 0;
            SelectedSaleOrder.TaxAmount = 0;
            SelectedSaleOrder.Deposit = 0;
            SelectedSaleOrder.OrderStatus = IsQuotation ? (short)SaleOrderStatus.Quote : (short)SaleOrderStatus.Open;
            SelectedSaleOrder.TermNetDue = 0;
            SelectedSaleOrder.TermDiscountPercent = 0;
            SelectedSaleOrder.TermPaidWithinDay = 0;
            SelectedSaleOrder.PaymentTermDescription = string.Empty;
            SelectedSaleOrder.PriceSchemaId = 1;
            SelectedSaleOrder.SaleRep = EmployeeCollection.FirstOrDefault().GuestNo;
            SelectedSaleOrder.Resource = Guid.NewGuid();
            SelectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
            SelectedSaleOrder.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
            SelectedSaleOrder.WeightUnit = Common.ShipUnits.First().Value;
            SelectedSaleOrder.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit

            //Get TaxLocation
            SelectedSaleOrder.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == SelectedSaleOrder.TaxLocation);

            //Create a sale order detail collection
            SelectedSaleOrder.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();

            //create a sale order Ship Collection
            SelectedSaleOrder.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();
            SelectedSaleOrder.SaleOrderShippedCollection = new CollectionBase<base_SaleOrderDetailModel>();

            // Create new payment collection
            SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

            //ReturnModel & ReturnDetailCollection
            SelectedSaleOrder.ReturnModel = new base_ResourceReturnModel();
            SelectedSaleOrder.ReturnModel.DocumentNo = SelectedSaleOrder.SONumber;
            SelectedSaleOrder.ReturnModel.DocumentResource = SelectedSaleOrder.Resource.ToString();
            SelectedSaleOrder.ReturnModel.TotalAmount = SelectedSaleOrder.Total;
            SelectedSaleOrder.ReturnModel.Resource = Guid.NewGuid();
            SelectedSaleOrder.ReturnModel.TotalRefund = 0;
            SelectedSaleOrder.ReturnModel.TotalAmount = 0;
            SelectedSaleOrder.ReturnModel.Mark = "SO";
            SelectedSaleOrder.ReturnModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedSaleOrder.ReturnModel.DateCreated = DateTime.Today;
            SelectedSaleOrder.ReturnModel.IsDirty = false;
            SelectedSaleOrder.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
            SelectedSaleOrder.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;
            SelectedSaleOrder.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
            //Additional
            SelectedSaleOrder.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing, IsDirty = false };
            SelectedSaleOrder.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping, IsDirty = false };
            SelectedCustomer = null;
            //Set to fist tab & skip TabChanged Methods in SelectedTabIndex property
            _selectedTabIndex = 0;
            OnPropertyChanged(() => SelectedTabIndex);

            SelectedSaleOrder.IsDirty = false;
            return SelectedSaleOrder;
        }

        /// <summary>
        /// Set Address From customer
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetBillShipAddress(base_GuestModel guestModel, base_SaleOrderModel saleOrderModel)
        {
            //Set Address To Bill or Ship
            if (guestModel.base_Guest.base_GuestAddress.Any(x => x.AddressTypeId == (int)AddressType.Billing))
            {
                base_GuestAddress billAdress = guestModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId == (int)AddressType.Billing);
                saleOrderModel.BillAddressModel = new base_GuestAddressModel(billAdress);
                saleOrderModel.BillAddress = saleOrderModel.BillAddressModel.Text;
            }
            else
            {
                saleOrderModel.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing };
                saleOrderModel.BillAddress = string.Empty;
            }
            if (guestModel.base_Guest.base_GuestAddress.Any(x => x.AddressTypeId == (int)AddressType.Shipping))
            {
                base_GuestAddress shippAdress = guestModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId == (int)AddressType.Shipping);
                saleOrderModel.ShipAddressModel = new base_GuestAddressModel(shippAdress);
                saleOrderModel.ShipAddress = saleOrderModel.ShipAddressModel.Text;
            }
            else
            {
                saleOrderModel.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping };
                saleOrderModel.ShipAddress = string.Empty;
            }
        }

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
            //isReward Member
            if (SelectedSaleOrder.GuestModel.IsRewardMember)
            {
                //Get GuestReward collection
                SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                foreach (base_GuestReward guestReward in SelectedSaleOrder.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(SelectedSaleOrder.GuestModel.Id) && !x.IsApply))
                {
                    int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == guestReward.base_RewardManager.RewardExpiration).Detail);
                    DateTime expireDate = guestReward.EearnedDate.Value.AddDays(expireDay);
                    if (expireDate.Date >= DateTime.Today)
                    {
                        SelectedSaleOrder.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestReward));
                    }
                }

                //Get Reward & set PurchaseThreshold if Customer any reward
                DateTime orderDate = SelectedSaleOrder.OrderDate.Value.Date;
                var reward = _rewardManagerRepository.Get(x =>
                                             x.IsActived
                                             && ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                             || !x.IsTrackingPeriod)));
                if (!SelectedSaleOrder.GuestModel.GuestRewardCollection.Any() && reward != null)
                    SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;
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

            SetBillShipAddress(SelectedCustomer, SelectedSaleOrder);

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
                   && guestAdditional.SaleTaxLocation != Define.CONFIGURATION.DefaultSaleTaxLocation//Diffrence with DefaultTaxLocation
                   )
                {
                    SelectedSaleOrder.IsTaxExemption = false;
                    SelectedSaleOrder.TaxExemption = string.Empty;
                    base_SaleTaxLocation saleTaxLocation = _saleTaxRepository.Get(x => x.Id == guestAdditional.SaleTaxLocation && x.ParentId == 0);
                    string msg = string.Format("Do you want to apply {0} tax", saleTaxLocation.Name);
                    MessageBoxResult resultMsg = MessageBox.Show(msg, "POS", MessageBoxButton.YesNo);
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
            }
            if (SelectedSaleOrder.TaxLocation > 0)
            {
                //get taxCode
                SelectedSaleOrder.TaxCodeModel = GetTaxCode(SelectedSaleOrder.TaxLocation, SelectedSaleOrder.TaxCode);

                //Get TaxLocation
                SelectedSaleOrder.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == SelectedSaleOrder.TaxLocation);
            }
            //Calculate Tax when user change Customer after create order detail
            if (SelectedSaleOrder.SaleOrderDetailCollection.Any())
            {
                decimal taxPercent;
                decimal taxAmount;
                if (SelectedSaleOrder.TaxCodeModel != null)
                {

                    if (Convert.ToInt32(SelectedSaleOrder.TaxCodeModel.TaxOption).Is(SalesTaxOption.Multi))
                    {
                        SelectedSaleOrder.ProductTaxAmount = CalcMultiTaxForProduct(SelectedSaleOrder);
                    }
                    else if (Convert.ToInt32(SelectedSaleOrder.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
                    {
                        SelectedSaleOrder.ProductTaxAmount = CalcPriceDependentTax(SelectedSaleOrder);
                        SelectedSaleOrder.ShipTaxAmount = CalcShipTaxAmount(SelectedSaleOrder);
                    }
                    else
                    {
                        CalcSingleTax(SelectedSaleOrder, SelectedSaleOrder.SubTotal, out taxPercent, out taxAmount);
                        SelectedSaleOrder.ProductTaxAmount = taxAmount;
                        SelectedSaleOrder.TaxPercent = 0;
                    }
                }
            }

        }

        /// <summary>
        /// Selected Product changed
        /// </summary>
        private void SelectedProductChanged()
        {
            if (SelectedProduct != null)
            {
                //Get VendorName
                base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == SelectedProduct.VendorId));
                if (vendorModel != null)
                    SelectedProduct.VendorName = vendorModel.LegalName;


                //Create new SaleOrderDetail , ProductUOM & add to collection
                base_SaleOrderDetailModel salesOrderDetailModel = AddNewSaleOrderDetail(SelectedProduct);

                //Get Product UOMCollection
                GetProductUOMforSaleOrderDetail(salesOrderDetailModel);

                //Update price when regular price =0
                salesOrderDetailModel = UpdateProductPrice(salesOrderDetailModel);

                SetPriceUOM(salesOrderDetailModel);

                //Calculate Discount for product
                CalcProductDiscount(salesOrderDetailModel);

                //Check Show Detail
                CheckToShowDatagridRowDetail(salesOrderDetailModel);

                if (SelectedProduct.IsSerialTracking)
                    OpenTrackingSerialNumber(salesOrderDetailModel, true);

                SelectedSaleOrder.CalcSubTotal();
                SetShipStatus();
            }
            SelectedProduct = null;
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

            if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.OrderDetailResource.Equals(selectedReturnDetail.OrderDetailResource)))
            {
                base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(selectedReturnDetail.OrderDetailResource));
                if (saleOrderShippedRemoved != null)
                {
                    SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                    SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
                }
            }

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
                SelectedProduct = productSearchViewModel.SelectedProduct;
            }
        }

        /// <summary>
        /// add new sale Order Detail & Get UOM
        /// </summary>
        /// <returns></returns>
        private base_SaleOrderDetailModel AddNewSaleOrderDetail(base_ProductModel productModel)
        {
            base_SaleOrderDetailModel salesOrderDetailModel = new base_SaleOrderDetailModel();
            salesOrderDetailModel.Resource = Guid.NewGuid();
            salesOrderDetailModel.Quantity = 1;
            salesOrderDetailModel.SerialTracking = string.Empty;
            salesOrderDetailModel.TaxCode = productModel.TaxCode;
            salesOrderDetailModel.ItemCode = productModel.Code;
            salesOrderDetailModel.ItemName = productModel.ProductName;
            salesOrderDetailModel.ProductResource = productModel.Resource.ToString();
            salesOrderDetailModel.OnHandQty = productModel.QuantityOnHand;
            salesOrderDetailModel.ItemAtribute = productModel.Attribute;
            salesOrderDetailModel.ItemSize = productModel.Size;
            salesOrderDetailModel.ProductModel = productModel;

            salesOrderDetailModel.CalcSubTotal();
            salesOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
            SelectedSaleOrder.SaleOrderDetailCollection.Add(salesOrderDetailModel);
            if (SelectedSaleOrder.TaxCodeModel != null)
            {
                if (Convert.ToInt32(SelectedSaleOrder.TaxCodeModel.TaxOption).Is(SalesTaxOption.Multi))
                {
                    SelectedSaleOrder.TaxPercent = 0;
                    SelectedSaleOrder.TaxAmount = CalcMultiTaxForProduct(SelectedSaleOrder);
                }
                else if (!Convert.ToInt32(SelectedSaleOrder.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
                {
                    SelectedSaleOrder.TaxPercent = 0;
                    SelectedSaleOrder.ProductTaxAmount = CalcPriceDependentTax(SelectedSaleOrder);
                    SelectedSaleOrder.ShipTaxAmount = CalcShipTaxAmount(SelectedSaleOrder);
                }
                else
                {
                    decimal taxAmount = 0;
                    decimal taxPercent = 0;
                    CalcSingleTax(SelectedSaleOrder, SelectedSaleOrder.SubTotal, out taxPercent, out taxAmount);
                    SelectedSaleOrder.ProductTaxAmount = taxAmount;
                    SelectedSaleOrder.TaxPercent = taxPercent;
                }

            }

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
            newSaleOrderDetail.BaseUOM = saleOrderdetail.BaseUOM;
            newSaleOrderDetail.RegularPrice = saleOrderdetail.RegularPrice;
            newSaleOrderDetail.SalePrice = saleOrderdetail.SalePrice;
            newSaleOrderDetail.ProductUOMCollection = saleOrderdetail.ProductUOMCollection;
            newSaleOrderDetail.Quantity = 1;
            newSaleOrderDetail.CalcSubTotal();
            return newSaleOrderDetail;
        }

        //CRUD region
        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void InsertSaleOrder()
        {
            if (SelectedSaleOrder.IsNew)
            {
                UpdateCustomerAddress(SelectedSaleOrder.BillAddressModel);
                SelectedSaleOrder.BillAddressId = SelectedSaleOrder.BillAddressModel.Id;
                UpdateCustomerAddress(SelectedSaleOrder.ShipAddressModel);
                SelectedSaleOrder.ShipAddressId = SelectedSaleOrder.BillAddressModel.Id;
                //Sale Order Detail Model
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    saleOrderDetailModel.ToEntity();
                    SelectedSaleOrder.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                    saleOrderDetailModel.EndUpdate();
                }
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
                SaleOrderCollection.Add(SelectedSaleOrder);
            }
        }

        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void UpdateSaleOrder()
        {
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
                    _saleOrderDetailRepository.Delete(saleOrderDetailModel.base_SaleOrderDetail);
                _saleOrderDetailRepository.Commit();
                SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Clear();
            }

            //Sale Order Detail Model
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.IsDirty))
            {
                saleOrderDetailModel.ToEntity();
                if (saleOrderDetailModel.IsNew)
                    SelectedSaleOrder.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                saleOrderDetailModel.EndUpdate();
            }
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
                saleOrderShipModel.ToEntity();
                //Delete SaleOrderShipDetail
                if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Any())
                {
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModelDel in saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems)
                        _saleOrderShipDetailRepository.Delete(saleOrderShipDetailModelDel.base_SaleOrderShipDetail);
                    _saleOrderShipDetailRepository.Commit();
                    saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Clear();
                }

                if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.Any())
                {
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection.Where(x => x.IsDirty || x.IsNew))
                    {
                        saleOrderShipDetailModel.ToEntity();
                        if (saleOrderShipDetailModel.IsNew)
                            saleOrderShipModel.base_SaleOrderShip.base_SaleOrderShipDetail.Add(saleOrderShipDetailModel.base_SaleOrderShipDetail);
                        saleOrderShipDetailModel.EndUpdate();
                    }
                }

                if (saleOrderShipModel.IsNew)
                    SelectedSaleOrder.base_SaleOrder.base_SaleOrderShip.Add(saleOrderShipModel.base_SaleOrderShip);
                saleOrderShipModel.EndUpdate();

            }
            #endregion

            #region SaleOrderReturn
            if (SelectedSaleOrder.ReturnModel != null && !IsQuotation)
            {
                SelectedSaleOrder.ReturnModel.ToEntity();
                if (SelectedSaleOrder.ReturnModel.IsNew && SelectedSaleOrder.ReturnModel.ReturnDetailCollection.DeletedItems.Any())
                {
                    foreach (base_ResourceReturnDetailModel returnDetailModel in SelectedSaleOrder.ReturnModel.ReturnDetailCollection.DeletedItems.Where(x => !x.IsTemporary))
                        _resourceReturnDetailRepository.Delete(returnDetailModel.base_ResourceReturnDetail);
                }
                SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Clear();

                foreach (base_ResourceReturnDetailModel returnDetailModel in SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary))
                {
                    if (returnDetailModel.IsDirty)
                    {
                        returnDetailModel.ToEntity();
                        if (returnDetailModel.IsNew)
                            SelectedSaleOrder.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail.Add(returnDetailModel.base_ResourceReturnDetail);
                    }
                }

                if (SelectedSaleOrder.ReturnModel.IsNew)
                    _resourceReturnRepository.Add(SelectedSaleOrder.ReturnModel.base_ResourceReturn);
                _resourceReturnRepository.Commit();

                //Update ID
                SelectedSaleOrder.ReturnModel.Id = SelectedSaleOrder.ReturnModel.base_ResourceReturn.Id;
                SelectedSaleOrder.ReturnModel.EndUpdate();

                foreach (base_ResourceReturnDetailModel returnDetailModel in SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsDirty && !x.IsTemporary))
                {
                    returnDetailModel.Id = returnDetailModel.base_ResourceReturnDetail.Id;
                    returnDetailModel.ResourceReturnId = returnDetailModel.base_ResourceReturnDetail.ResourceReturnId;
                    if (returnDetailModel.IsReturned)
                    {
                        //Increase Store
                        _productRepository.UpdateOnHandQuantity(returnDetailModel.ProductResource, SelectedSaleOrder.StoreCode, returnDetailModel.ReturnQty);
                    }
                    returnDetailModel.EndUpdate();
                }

            }
            #endregion

            #region Payment
            SavePaymentCollection(SelectedSaleOrder);
            #endregion

            SelectedSaleOrder.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedSaleOrder.ToEntity();
            _saleOrderRepository.Commit();

            //Set ID
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
        private void SaveSaleCommission(base_SaleOrderModel saleOrderModel)
        {
            Guid customerGuid = Guid.Parse(saleOrderModel.CustomerResource);
            //Get Customer with CustomerResource
            base_GuestModel customerModel = CustomerCollection.Where(x => x.Resource == customerGuid).SingleOrDefault();
            if (customerModel != null && customerModel.SaleRepId.HasValue)
            {
                base_GuestModel employeeModel = EmployeeCollection.Where(x => x.Id == customerModel.SaleRepId).SingleOrDefault();
                base_SaleCommission saleCommission = _saleCommissionRepository.Get(x => x.GuestResource == employeeModel.ResourceString && x.SOResource.Equals(saleOrderModel.ResourceString));
                if (saleCommission != null)
                    _saleCommissionRepository.Delete(saleCommission);

                base_SaleCommission newSaleCommission = new base_SaleCommission();
                newSaleCommission.ComissionPercent = employeeModel.CommissionPercent;
                newSaleCommission.GuestResource = employeeModel.Resource.ToString();
                newSaleCommission.Remark = "SaleOrder";
                newSaleCommission.Sign = "+";
                newSaleCommission.SODate = saleOrderModel.OrderDate;
                newSaleCommission.SONumber = saleOrderModel.SONumber;
                newSaleCommission.SOResource = saleOrderModel.Resource.ToString();
                newSaleCommission.SOTotal = saleOrderModel.SubTotal;
                newSaleCommission.CommissionAmount = newSaleCommission.SOTotal * newSaleCommission.ComissionPercent / 100;
                _saleCommissionRepository.Add(newSaleCommission);
                _saleCommissionRepository.Commit();
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
            //Update Term
            saleOrderModel.GuestModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            saleOrderModel.GuestModel.PaymentTermDescription = saleOrderModel.PaymentTermDescription;
            saleOrderModel.GuestModel.TermDiscount = saleOrderModel.TermDiscountPercent;
            saleOrderModel.GuestModel.TermNetDue = saleOrderModel.TermNetDue;
            saleOrderModel.GuestModel.TermPaidWithinDay = saleOrderModel.TermPaidWithinDay;

            //Update Customer Reward 
            saleOrderModel.GuestModel.ToEntity();

            //Onlyt reward Member
            if (saleOrderModel.GuestModel.IsRewardMember)
            {
                //Update Guest Reward
                if (saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                {
                    foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems)
                        guestRewardModel.ToEntity();
                    saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Clear();
                }
                if (saleOrderModel.GuestModel.GuestRewardCollection != null)
                {
                    if (saleOrderModel.GuestModel.GuestRewardCollection.Any(x => x.IsNew))
                    {
                        foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection)
                        {
                            guestRewardModel.ToEntity();
                            if (guestRewardModel.IsNew)
                                saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                        }
                    }
                    _guestRepository.Commit();

                    //Set Id For Reward
                    if (saleOrderModel.GuestModel.GuestRewardCollection.Any(x => x.IsNew))
                    {
                        foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.Where(x => x.IsNew))
                        {
                            guestRewardModel.Id = guestRewardModel.base_GuestReward.Id;
                            guestRewardModel.EndUpdate();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Execute payment
        /// </summary>
        private void SaleOrderPayment()
        {
            if (SelectedSaleOrder.IsNew && !IsQuotation)
                return;

            bool? resultReward;
            bool isPayFull = false;
            //Show Reward Form
            //Need check has any Guest Reward
            //Show Reward only SaleOrder Payment
            if (!IsQuotation && SelectedSaleOrder.GuestModel.IsRewardMember
                && SelectedSaleOrder.GuestModel.GuestRewardCollection != null && SelectedSaleOrder.GuestModel.GuestRewardCollection.Any()
                && SelectedSaleOrder.PaymentCollection != null
                && !SelectedSaleOrder.PaymentCollection.Any(x => !x.IsDeposit.Value) /* This order is paid with multi pay*/
                )
            {
                //Confirm User want to Payment Full
                MessageBoxResult confirmPayFull = MessageBox.Show("You have some rewards, you need to pay fully and use these rewards. Do you?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (confirmPayFull.Equals(MessageBoxResult.Yes))//User Payment full
                {
                    isPayFull = true;

                    RedeemRewardViewModel redeemRewardViewModel = new RedeemRewardViewModel(SelectedSaleOrder);
                    resultReward = _dialogService.ShowDialog<RedeemRewardView>(_ownerViewModel, redeemRewardViewModel, "Redeem Reward");
                    if (resultReward == true)
                    {
                        if (redeemRewardViewModel.ViewActionType == RedeemRewardViewModel.ReeedemRewardType.Redeemded)
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

            if (resultReward == true)
            {
                SelectedSaleOrder.RewardValueApply = 0;
                //Calc Subtotal user apply reward
                if (!IsQuotation && SelectedSaleOrder.GuestModel.GuestRewardCollection != null && SelectedSaleOrder.GuestModel.GuestRewardCollection.Any(x => x.IsChecked))
                {
                    base_GuestRewardModel guestReward = SelectedSaleOrder.GuestModel.GuestRewardCollection.Single(x => x.IsChecked);
                    if (guestReward != null)
                    {
                        //Update Subtoal After apply reward
                        if (guestReward.base_GuestReward.base_RewardManager.RewardAmtType.Is(RewardAmtType.Pecent))
                        {
                            decimal subTotal = 0;
                            if (Define.CONFIGURATION.IsRewardOnTax)//Check reward include tax ?
                                subTotal = SelectedSaleOrder.SubTotal - SelectedSaleOrder.DiscountAmount + SelectedSaleOrder.TaxAmount;
                            else
                                subTotal = SelectedSaleOrder.SubTotal - SelectedSaleOrder.DiscountAmount;

                            SelectedSaleOrder.RewardValueApply = subTotal * guestReward.base_GuestReward.base_RewardManager.RewardAmount / 100;
                        }
                        else
                            SelectedSaleOrder.RewardValueApply = guestReward.base_GuestReward.base_RewardManager.RewardAmount;
                    }
                }

                decimal balance = SelectedSaleOrder.Total - SelectedSaleOrder.RewardValueApply - SelectedSaleOrder.Paid - (SelectedSaleOrder.Deposit.HasValue ? SelectedSaleOrder.Deposit.Value : 0);

                decimal totalDeposit = 0;
                decimal lastPayment = 0;
                if (SelectedSaleOrder.PaymentCollection != null)
                {
                    totalDeposit = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
                    base_ResourcePaymentModel paymentModel = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value).OrderBy(x => x.DateCreated).LastOrDefault();
                    if (paymentModel != null)
                        lastPayment = paymentModel.TotalPaid;
                }
                //Show Payment
                SalesOrderPaymenViewModel paymentViewModel = new SalesOrderPaymenViewModel(SelectedSaleOrder, balance, totalDeposit, lastPayment, isPayFull);
                bool? dialogResult = _dialogService.ShowDialog<SalesOrderPaymentView>(_ownerViewModel, paymentViewModel, "Payment");
                if (dialogResult == true)
                {
                    //Calc Reaward , redeem & update subtotal
                    CalcRedeemReward(SelectedSaleOrder);

                    // Update payment product collection
                    UpdatePaymentCollection(SelectedSaleOrder, paymentViewModel);

                    SelectedSaleOrder.Paid = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value).Sum(x => x.TotalPaid);
                    //paymentViewModel.PaymentModel.TotalPaid - (SelectedSaleOrder.Deposit.HasValue ? SelectedSaleOrder.Deposit.Value : 0);
                    //Set Status

                    if (SelectedSaleOrder.Paid + SelectedSaleOrder.Deposit.Value >= SelectedSaleOrder.Total)
                    {
                        CreateNewReward(SelectedSaleOrder);
                        SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.PaidInFull;
                        SaveSalesOrder();
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
                    }
                }
                // Reset reward apply after use
                SelectedSaleOrder.RewardValueApply = 0;
            }

            SetAllowChangeOrder(SelectedSaleOrder);
        }

        /// <summary>
        /// Deposite for Quotation
        /// </summary>
        private void DepositProcess()
        {
            decimal balance = SelectedSaleOrder.Total - SelectedSaleOrder.Deposit.Value;
            //Show Payment
            SalesOrderPaymenViewModel paymentViewModel = new SalesOrderPaymenViewModel(SelectedSaleOrder, balance);
            bool? dialogResult = _dialogService.ShowDialog<DepositPaymentView>(_ownerViewModel, paymentViewModel, "Deposit");
            if (dialogResult == true)
            {
                if (IsQuotation)
                {
                    if (SelectedSaleOrder.PaymentModel == null)
                        SelectedSaleOrder.PaymentModel = paymentViewModel.PaymentModel;
                    else
                    {
                        if (SelectedSaleOrder.PaymentCollection != null && !SelectedSaleOrder.PaymentCollection.Any(x => x.IsDeposit.Value))
                        {
                            SelectedSaleOrder.PaymentCollection.Add(paymentViewModel.PaymentModel);
                        }
                        else
                        {
                            //Update PaymentCard
                            SelectedSaleOrder.PaymentModel = SelectedSaleOrder.PaymentCollection.SingleOrDefault(x => x.IsDeposit.Value);
                            if (SelectedSaleOrder.PaymentModel != null)
                            {
                                foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentViewModel.PaymentModel.PaymentDetailCollection)
                                    SelectedSaleOrder.PaymentModel.PaymentDetailCollection.Add(paymentDetailModel);
                            }
                        }
                    }
                    //Sum paid of payament method & Total paid in db
                    SelectedSaleOrder.PaymentModel.TotalPaid = SelectedSaleOrder.PaymentModel.base_ResourcePayment.TotalPaid + SelectedSaleOrder.PaymentModel.PaymentDetailCollection.Where(x => !x.IsCard).Sum(x => x.Paid);
                    SelectedSaleOrder.Deposit = SelectedSaleOrder.PaymentModel.TotalPaid;
                }
            }
        }

        /// <summary>
        /// Calculate & insert Reward for guest
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
                //Update Subtoal After apply reward
                if (guestReward.base_GuestReward.base_RewardManager.RewardAmtType.Is(RewardAmtType.Pecent))
                    saleOrderModel.SubTotal = saleOrderModel.SubTotal - saleOrderModel.SubTotal * guestReward.base_GuestReward.base_RewardManager.RewardAmount / 100;
                else
                    saleOrderModel.SubTotal = saleOrderModel.SubTotal - guestReward.base_GuestReward.base_RewardManager.RewardAmount;

                //UpdateDate Guest Reward
                guestReward.IsApply = true;
                guestReward.RedeemedDate = DateTime.Today;
                saleOrderModel.GuestModel.GuestRewardCollection.Remove(guestReward);
            }
            saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod += saleOrderModel.SubTotal;
        }

        /// <summary>
        /// Create New Reward for customer
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CreateNewReward(base_SaleOrderModel saleOrderModel)
        {
            if (!saleOrderModel.GuestModel.IsRewardMember)
                return;
            //Create new reward for Customer
            DateTime orderDate = saleOrderModel.OrderDate.Value.Date;
            //saleOrderModel.SubTotal >= x.PurchaseThreshold
            var reward = _rewardManagerRepository.Get(x =>
                                         x.IsActived
                                         && ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                         || !x.IsTrackingPeriod)));

            int totalOfReward = 0;
            if (reward != null)
            {
                decimal totalOfPurchase = (saleOrderModel.SubTotal - saleOrderModel.RewardValueApply) + (reward.PurchaseThreshold - saleOrderModel.GuestModel.RequirePurchaseNextReward);
                if (totalOfPurchase > reward.PurchaseThreshold)
                {
                    totalOfReward = Convert.ToInt32(Math.Truncate(totalOfPurchase / reward.PurchaseThreshold));

                    //Calculate Total Reward Redeemed = Subtotal * Reward Amount
                    if (reward.RewardAmtType.Is(RewardAmtType.Pecent))
                        saleOrderModel.GuestModel.TotalRewardRedeemed = saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod * reward.RewardAmount / 100;
                    else
                        saleOrderModel.GuestModel.TotalRewardRedeemed = reward.RewardAmount;

                    for (int i = 0; i < totalOfReward; i++)
                    {
                        base_GuestRewardModel guestRewardModel = new base_GuestRewardModel();
                        guestRewardModel.EearnedDate = DateTime.Today;
                        guestRewardModel.IsApply = false;
                        guestRewardModel.RewardId = reward.Id;
                        guestRewardModel.GuestId = saleOrderModel.GuestModel.Id;
                        guestRewardModel.SaleOrderNo = saleOrderModel.SONumber;
                        guestRewardModel.SaleOrderResource = saleOrderModel.Resource.ToString();
                        guestRewardModel.Amount = (saleOrderModel.SubTotal - saleOrderModel.RewardValueApply);
                        guestRewardModel.Remark = string.Empty;

                        if (reward.RewardAmtType.Is(RewardAmtType.Pecent))
                            guestRewardModel.RewardValue = (saleOrderModel.SubTotal - saleOrderModel.RewardValueApply) * reward.RewardAmount / 100;
                        else
                            guestRewardModel.RewardValue = reward.RewardAmount;
                        saleOrderModel.GuestModel.GuestRewardCollection.Add(guestRewardModel);
                    }
                }
            }
            //Calculate Require Purchase Next Reward
            //A is PurchaseDuringTrackingPeriod
            //P is PurchaseThreshold
            //R is RequirePurchaseNextReward
            //R = P - (A/P % 2 * P)

            saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold - ((saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold) % 1) * reward.PurchaseThreshold;
            if (totalOfReward > 0)
            {
                int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == reward.RewardExpiration).Detail);
                DateTime expireDate = saleOrderModel.GuestModel.GuestRewardCollection.FirstOrDefault().EearnedDate.Value.AddDays(expireDay);
                string rewardProgram = string.Empty;
                if (reward.RewardAmtType.Equals(RewardAmtType.Money))
                    rewardProgram = string.Format("Reward $ {0}", reward.RewardAmount);
                else
                    rewardProgram = string.Format("Reward {0}%", reward.RewardAmount);
                MessageBox.Show(string.Format("You are received : {0} reward(s) {1}  \nExpire Date : {2} \nRequire Purchase Next Reward :{3}", totalOfReward, rewardProgram, expireDate.Date.ToString(Define.DateFormat), saleOrderModel.GuestModel.RequirePurchaseNextReward.ToString(Define.CurrencyFormat)), "POS", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show(string.Format("Require Purchase Next Reward {0}", saleOrderModel.GuestModel.RequirePurchaseNextReward.ToString(Define.CurrencyFormat)), "POS", MessageBoxButton.OK);
            }

        }

        //Calculation region
        /// <summary>
        /// GetTaxCode & TaxCodeOptionCollection
        /// </summary>
        /// <param name="taxLocation"></param>
        /// <param name="taxCode"></param>
        /// <returns></returns>
        private base_SaleTaxLocationModel GetTaxCode(int taxLocation, string taxCode)
        {
            if (taxLocation > 0)
            {
                base_SaleTaxLocationModel TaxCodeModel = SaleTaxLocationCollection.SingleOrDefault(x => x.ParentId == taxLocation && x.TaxCode.Equals(taxCode));
                if (TaxCodeModel != null)
                    TaxCodeModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(TaxCodeModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)));
                return TaxCodeModel;
            }
            return null;
        }

        /// <summary>
        /// Calculator Tax Amount & Percent with Subtotal
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalcSingleTax(base_SaleOrderModel saleOrderModel, decimal subTotal, out decimal taxPercent, out decimal taxAmount)
        {
            if (saleOrderModel.IsTaxExemption == true)
            {
                taxAmount = 0;
                taxPercent = 0;
            }
            else
            {
                if (saleOrderModel.TaxCodeModel == null)
                    saleOrderModel.TaxCodeModel = GetTaxCode(saleOrderModel.TaxLocation, saleOrderModel.TaxCode);

                if (Convert.ToInt32(saleOrderModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Single))
                {
                    base_SaleTaxLocationOptionModel taxOptionModel = saleOrderModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                    if (taxOptionModel != null)
                        taxPercent = taxOptionModel.TaxRate;
                    else
                        taxPercent = 0;

                    if (saleOrderModel.TaxCodeModel.IsTaxAfterDiscount)
                        taxAmount = (subTotal - saleOrderModel.DiscountAmount) * taxPercent / 100;
                    else
                        taxAmount = subTotal * taxPercent / 100;
                }
                else
                {
                    taxAmount = 0;
                    taxPercent = 0;
                }
            }
        }

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
            else if (Convert.ToInt32(saleOrderModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
            {
                if (saleOrderModel.TaxCodeModel == null)
                    saleOrderModel.TaxCodeModel = GetTaxCode(saleOrderModel.TaxLocation, saleOrderModel.TaxCode);

                base_SaleTaxLocationOptionModel saleTaxLocationOptionModel = saleOrderModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                    taxAmount += CalcPriceDependentItem(saleOrderDetailModel.SubTotal, saleOrderDetailModel.SalePrice, saleTaxLocationOptionModel);
            }
            else
            {
                taxAmount = 0;
            }
            return taxAmount;
        }

        /// <summary>
        /// Calc Ship Tax with PriceDepent
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private decimal CalcShipTaxAmount(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.TaxCodeModel != null
                && saleOrderModel.TaxLocationModel.IsShipingTaxable)
            {
                base_SaleTaxLocationModel shippingTaxCode = SaleTaxLocationCollection.SingleOrDefault(x => x.Id.Equals(saleOrderModel.TaxLocationModel.ShippingTaxCodeId));
                if (shippingTaxCode != null
                    && Convert.ToInt32(shippingTaxCode.TaxOption).Is(SalesTaxOption.Price)
                    && shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault() != null
                    && shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault().IsApplyAmountOver)
                {
                    base_SaleTaxLocationOptionModel taxOptionModel = new base_SaleTaxLocationOptionModel(shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault());
                    return CalcPriceDependentItem(saleOrderModel.Shipping, saleOrderModel.Shipping, taxOptionModel);
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        /// <summary>
        /// Calculate TaxPreice dependent for item. 
        /// Using for Shipping Tax & SaleOrderTax
        /// </summary>
        /// <param name="subTotal"></param>
        /// <param name="compareValue"></param>
        /// <param name="saleTaxLocationOptionModel"></param>
        /// <returns></returns>
        private decimal CalcPriceDependentItem(decimal subTotal, decimal compareValue, base_SaleTaxLocationOptionModel saleTaxLocationOptionModel)
        {
            decimal taxAmountResult = 0;
            if (saleTaxLocationOptionModel.IsApplyAmountOver)//Apply Sale Tax Only the amount over the unit price
            {
                if (compareValue > saleTaxLocationOptionModel.TaxCondition) //Subtotal over TaxCondition
                {
                    taxAmountResult = (subTotal - saleTaxLocationOptionModel.TaxCondition) * saleTaxLocationOptionModel.TaxRate / 100;
                    //saleOrderModel.TaxAmount = (saleOrderModel.SubTotal - saleTaxLocationOptionModel.TaxCondition) * saleTaxLocationOptionModel.TaxRate / 100;
                }
            }
            else
            {
                taxAmountResult = subTotal * saleTaxLocationOptionModel.TaxRate / 100;
                //saleOrderModel.TaxAmount = saleOrderModel.SubTotal * saleTaxLocationOptionModel.TaxRate / 100;
            }
            return taxAmountResult;
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
                if (saleOrderModel.TaxCodeModel == null)
                    saleOrderModel.TaxCodeModel = GetTaxCode(saleOrderModel.TaxLocation, saleOrderModel.TaxCode);

                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    //Calculate Tax for saleOrderDetail
                    foreach (var saleTaxLocationOptionModel in saleOrderModel.TaxCodeModel.SaleTaxLocationOptionCollection)
                    {
                        if (!saleTaxLocationOptionModel.IsAllowAmountItemPriceRange)//Not check Allow ItemPrice Range=false
                        {
                            //taxAmount += subTotal * TaxRate
                            taxAmount += saleOrderDetailModel.SubTotal * saleTaxLocationOptionModel.TaxRate / 100;
                        }
                        else //Check Allow ItemPriceRange = true
                        {
                            if (saleTaxLocationOptionModel.PriceFrom > 0 && saleTaxLocationOptionModel.PriceTo > 0) //has Range
                            {
                                if (saleTaxLocationOptionModel.IsAllowSpecificItemPriceRange)
                                {
                                    //SalePrice In Range (Ex: [10,12] subtotal =11) => (Subtotal - Min)*Taxrate
                                    if (saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceFrom && saleOrderDetailModel.SalePrice <= saleTaxLocationOptionModel.PriceTo)
                                    {
                                        //taxAmount += (Subtotal - PriceFrom)* TaxRate
                                        taxAmount += (saleOrderDetailModel.SubTotal - saleTaxLocationOptionModel.PriceFrom) * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    //Out Of Range grather max value (Ex: [10,12] subtotal =14) => (Max-min) * TaxRate
                                    else if (saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceTo)
                                    {
                                        // taxAmount += = ( PriceTo -PriceFrom)* TaxRate
                                        taxAmount += (saleTaxLocationOptionModel.PriceTo - saleTaxLocationOptionModel.PriceFrom) * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    //else Out Of Range(smaller than min value)(Ex: [10,12] subtotal =9) => taxAmount =0 :  +=0

                                }
                                else
                                {
                                    //SalePrice In Range (Ex: [10,12] subtotal =11)=> Subtotal * TaxRate
                                    if (saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceFrom && saleOrderDetailModel.SalePrice <= saleTaxLocationOptionModel.PriceTo)
                                    {
                                        //taxAmount +=  subTotal* TaxRate

                                        taxAmount += saleOrderDetailModel.SubTotal * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    //Out of PriceFrom & Price To (Ex: [10,12] subtotal =14 or 9)=> TaxAmount =0
                                    //else if (saleOrderDetailModel.SalePrice <= saleTaxLocationOptionModel.PriceFrom || saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceTo)
                                    //{
                                    //    taxAmount += 0;
                                    //}
                                }
                            }
                            else if (saleTaxLocationOptionModel.PriceFrom > 0 && saleTaxLocationOptionModel.PriceTo == 0)//Above
                            {
                                if (saleTaxLocationOptionModel.IsAllowSpecificItemPriceRange)
                                {
                                    //SalePrice greather than PriceFrom (Ex: [10,~] subtotal =11)=>(Subtotal - Min)* TaxRate
                                    if (saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceFrom)
                                    {
                                        //taxAmount += (SubTotal - FriceFrom) * TaxRate
                                        taxAmount += (saleOrderDetailModel.SubTotal - saleTaxLocationOptionModel.PriceFrom) * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    //SalePrice smaller than PriceFrom (Ex: [10,~] subtotal =9)+0
                                }
                                else
                                {
                                    //SalePrice greather than PriceFrom (Ex: [10,~] subtotal =11)=>Subtotal* TaxRate
                                    if (saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceFrom)
                                    {
                                        //taxAmount += saleOrderDetailModel.SubTotal * TaxRate
                                        taxAmount += saleOrderDetailModel.SubTotal * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    // else Subtotal Smaller than PriceFrom (Ex: [10,~] subtotal = 9) => TaxAmout +=0

                                }
                            }
                            else if (saleTaxLocationOptionModel.PriceFrom == 0 && saleTaxLocationOptionModel.PriceTo > 0) //Below
                            {
                                if (saleTaxLocationOptionModel.IsAllowSpecificItemPriceRange)
                                {
                                    //Subtotal greather than PriceTo (Ex: [~,10] subtotal =11)=>Max * TaxRate
                                    if (saleOrderDetailModel.SalePrice >= saleTaxLocationOptionModel.PriceTo)
                                    {

                                        taxAmount += saleTaxLocationOptionModel.PriceTo * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    else//Subtotal smaller than PriceTo (Ex: [~,10] subtotal =9)=>Subtotal * TaxRate
                                    {
                                        taxAmount += saleOrderDetailModel.SubTotal * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                }
                                else
                                {

                                    if (saleOrderDetailModel.SalePrice < saleTaxLocationOptionModel.PriceTo)
                                    //Subtotal smaller than PriceTo (Ex: [~,10] subtotal =9)=>Subtotal * TaxRate
                                    {
                                        taxAmount += saleOrderDetailModel.SubTotal * saleTaxLocationOptionModel.TaxRate / 100;
                                    }
                                    //Subtotal greather than PriceTo (Ex: [~,10] subtotal =11)=>TaxAmout +=0
                                }
                            }

                        }
                    }
                }

                //End foreach
            }
            return taxAmount;
        }

        /// <summary>
        /// Calculate discount for product
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CalcProductDiscount(base_SaleOrderDetailModel saleOrderDetailModel, bool resetDiscPercent = false)
        {
            //reset Discount percent Or calculate new Price when change UOM
            if (resetDiscPercent)
            {
                saleOrderDetailModel.DiscountPercent = 0;
                saleOrderDetailModel.UnitDiscount = Math.Round(saleOrderDetailModel.RegularPrice * saleOrderDetailModel.DiscountPercent, 0);
                saleOrderDetailModel.SalePrice = saleOrderDetailModel.RegularPrice;
                saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.SalePrice;
            }
            saleOrderDetailModel.TotalDiscount = Math.Round(saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity, 0);

            if (saleOrderDetailModel.ProductModel.BaseUOMId != saleOrderDetailModel.UOMId)//Apply discount only Base Unit
                return;


            base_SaleOrderModel saleOrderModel = SelectedSaleOrder;
            var promotionList = _promotionRepository.GetAll(x => x.Status == (int)StatusBasic.Active).Select(x => new base_PromotionModel(x)
            {
                PromotionScheduleModel = new base_PromotionScheduleModel(x.base_PromotionSchedule.FirstOrDefault())
                {
                    ExpirationNoEndDate = !x.base_PromotionSchedule.FirstOrDefault().StartDate.HasValue
                }
            });

            base_PromotionModel promotionModel;
            if (promotionList.Any(x => !x.PromotionScheduleModel.ExpirationNoEndDate))//Has StartDate && EndDate
                promotionModel = promotionList.Where(x => !x.PromotionScheduleModel.ExpirationNoEndDate
                                                         && x.PromotionScheduleModel.StartDate <= saleOrderModel.OrderDate
                                                         && saleOrderModel.OrderDate <= x.PromotionScheduleModel.EndDate).FirstOrDefault();
            else
                promotionModel = promotionList.FirstOrDefault();

            if (IsAcceptedPromotion(promotionModel, saleOrderDetailModel))
            {
                //Sum the same of Product & all of them more than quantity of Promotion
                int sumOfItem = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductResource == saleOrderDetailModel.ProductResource && x.UOMId == saleOrderDetailModel.ProductModel.BaseUOMId).Sum(x => x.Quantity);
                switch (promotionModel.PromotionTypeId)
                {
                    case 1: //% off
                        //so tien giảm trên 1 đơn vi
                        saleOrderDetailModel.UnitDiscount = (saleOrderDetailModel.RegularPrice * promotionModel.TakeOff / 100);
                        saleOrderDetailModel.DiscountPercent = promotionModel.TakeOff;
                        //So tien dc giam trên 1 đợn vi
                        saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.RegularPrice - saleOrderDetailModel.UnitDiscount;
                        saleOrderDetailModel.SalePrice = saleOrderDetailModel.DiscountAmount;
                        //Tổng số tiền dc giảm trên tổng số sản phẩm
                        saleOrderDetailModel.TotalDiscount = saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity;
                        break;
                    case 2://$ off
                        //so tien giảm trên 1 đơn vi
                        saleOrderDetailModel.UnitDiscount = promotionModel.TakeOff;
                        saleOrderDetailModel.DiscountPercent = saleOrderDetailModel.UnitDiscount / saleOrderDetailModel.RegularPrice;
                        //So tien dc giam trên 1 đợn vi
                        saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.RegularPrice - saleOrderDetailModel.UnitDiscount;
                        saleOrderDetailModel.SalePrice = saleOrderDetailModel.DiscountAmount;
                        //Tổng số tiền dc giảm trên tổng số sản phẩm
                        saleOrderDetailModel.TotalDiscount = saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity;
                        break;
                    case 3://Buy X for $Y
                        #region Buy x for $Y
                        decimal sumOfMoney = saleOrderDetailModel.RegularPrice * sumOfItem;
                        if (sumOfItem >= promotionModel.BuyingQty)
                        {
                            if (sumOfMoney > promotionModel.GetingValue)
                            {
                                if (promotionModel.IsApplyToAboveQuantities)
                                {
                                    //Get the same of product & only base Unit
                                    foreach (var itemSaleOrderDetail in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductResource == saleOrderDetailModel.ProductResource && x.UOMId == saleOrderDetailModel.ProductModel.BaseUOMId))
                                    {
                                        //////Calculate Discount
                                        //so tien giảm trên 1 đơn vi
                                        itemSaleOrderDetail.UnitDiscount = Math.Round(itemSaleOrderDetail.RegularPrice - promotionModel.TakeOff, 2);
                                        itemSaleOrderDetail.DiscountPercent = itemSaleOrderDetail.UnitDiscount / itemSaleOrderDetail.RegularPrice;

                                        //Tổng số tiền dc giảm trên tổng số sản phẩm
                                        itemSaleOrderDetail.TotalDiscount = Math.Round(itemSaleOrderDetail.UnitDiscount * itemSaleOrderDetail.Quantity, 2);

                                        //So tien dc giam trên 1 đợn vi
                                        itemSaleOrderDetail.DiscountAmount = itemSaleOrderDetail.RegularPrice - itemSaleOrderDetail.UnitDiscount;
                                        itemSaleOrderDetail.SalePrice = itemSaleOrderDetail.DiscountAmount;
                                    }
                                }
                                else// not IsApplyToAboveQuantities
                                {
                                    int numberOfItemDiscount = sumOfItem - (sumOfItem % promotionModel.BuyingQty);
                                    //get the same of product & only base unit
                                    foreach (var itemSaleOrderDetail in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductResource == saleOrderDetailModel.ProductResource && x.UOMId == saleOrderDetailModel.ProductModel.BaseUOMId).ToList())
                                    {
                                        int quantityRemain = itemSaleOrderDetail.Quantity;//back up first quantity when item remove will be lost
                                        if (numberOfItemDiscount > 0)
                                        {
                                            if (itemSaleOrderDetail.Quantity <= numberOfItemDiscount)
                                            {
                                                //////Calculate Discount
                                                //so tien giảm trên 1 đơn vi
                                                itemSaleOrderDetail.UnitDiscount = Math.Round(itemSaleOrderDetail.RegularPrice - promotionModel.TakeOff, 2);
                                                itemSaleOrderDetail.DiscountPercent = Math.Round(itemSaleOrderDetail.UnitDiscount * 100 / itemSaleOrderDetail.RegularPrice, 2);

                                                //Tổng số tiền dc giảm trên tổng số sản phẩm
                                                itemSaleOrderDetail.TotalDiscount = Math.Round(itemSaleOrderDetail.UnitDiscount * itemSaleOrderDetail.Quantity, 2);

                                                //So tien dc giam trên 1 đợn vi
                                                itemSaleOrderDetail.DiscountAmount = itemSaleOrderDetail.RegularPrice - itemSaleOrderDetail.UnitDiscount;
                                                itemSaleOrderDetail.SalePrice = itemSaleOrderDetail.DiscountAmount;
                                                CheckToShowDatagridRowDetail(itemSaleOrderDetail);
                                            }
                                            else
                                            {

                                                base_SaleOrderDetailModel replaceItem = NewSaleOrderDetail(itemSaleOrderDetail);
                                                replaceItem.Quantity = numberOfItemDiscount;
                                                //////Calculate Discount
                                                //so tien giảm trên 1 đơn vi
                                                replaceItem.UnitDiscount = Math.Round(replaceItem.RegularPrice - promotionModel.TakeOff, 2);
                                                replaceItem.DiscountPercent = Math.Round(replaceItem.UnitDiscount * 100 / replaceItem.RegularPrice, 2);
                                                //Math.Round(replaceItem.UnitDiscount / replaceItem.RegularPrice); ;

                                                //Tổng số tiền dc giảm trên tổng số sản phẩm
                                                replaceItem.TotalDiscount = Math.Round(replaceItem.UnitDiscount * replaceItem.Quantity, 2);

                                                //So tien dc giam trên 1 đợn vi
                                                replaceItem.DiscountAmount = replaceItem.RegularPrice - replaceItem.UnitDiscount;
                                                replaceItem.SalePrice = replaceItem.DiscountAmount;

                                                //Set Serial
                                                if (!string.IsNullOrWhiteSpace(replaceItem.SerialTracking) && replaceItem.ProductModel.IsSerialTracking)
                                                {
                                                    IEnumerable<string> remainSerial = replaceItem.SerialTracking.Split(',').Take(replaceItem.Quantity);
                                                    replaceItem.SerialTracking = string.Join(", ", remainSerial);
                                                }
                                                replaceItem.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                                                CheckToShowDatagridRowDetail(replaceItem);

                                                var deleteItem = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == itemSaleOrderDetail.Resource);
                                                if (deleteItem != null)
                                                {
                                                    deleteItem.PropertyChanged -= new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                                                    SelectedSaleOrder.SaleOrderDetailCollection.Remove(deleteItem);
                                                }
                                                SelectedSaleOrder.SaleOrderDetailCollection.Add(replaceItem);
                                                int remainItem = quantityRemain - numberOfItemDiscount;
                                                if (remainItem > 0)
                                                {
                                                    base_SaleOrderDetailModel remainSaleOrderDetail = NewSaleOrderDetail(saleOrderDetailModel);
                                                    remainSaleOrderDetail.Quantity = remainItem;
                                                    //Reset Discount
                                                    remainSaleOrderDetail.DiscountPercent = 0;
                                                    remainSaleOrderDetail.UnitDiscount = Math.Round(remainSaleOrderDetail.RegularPrice * remainSaleOrderDetail.DiscountPercent, 0);
                                                    remainSaleOrderDetail.SalePrice = remainSaleOrderDetail.RegularPrice;
                                                    remainSaleOrderDetail.DiscountAmount = remainSaleOrderDetail.SalePrice;
                                                    remainSaleOrderDetail.TotalDiscount = Math.Round(remainSaleOrderDetail.UnitDiscount * remainSaleOrderDetail.Quantity, 0);
                                                    if (!string.IsNullOrWhiteSpace(remainSaleOrderDetail.SerialTracking) && saleOrderDetailModel.ProductModel.IsSerialTracking)
                                                    {
                                                        int skipItem = replaceItem.Quantity;
                                                        IEnumerable<string> remainSerial = remainSaleOrderDetail.SerialTracking.Split(',').Skip(skipItem).Take(remainSaleOrderDetail.Quantity);
                                                        remainSaleOrderDetail.SerialTracking = string.Join(", ", remainSerial);
                                                    }
                                                    CheckToShowDatagridRowDetail(remainSaleOrderDetail);

                                                    remainSaleOrderDetail.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                                                    SelectedSaleOrder.SaleOrderDetailCollection.Add(remainSaleOrderDetail);
                                                }
                                            }
                                            numberOfItemDiscount -= quantityRemain;
                                        }
                                        else
                                        {
                                            itemSaleOrderDetail.DiscountPercent = 0;
                                            itemSaleOrderDetail.UnitDiscount = Math.Round(itemSaleOrderDetail.RegularPrice * itemSaleOrderDetail.DiscountPercent, 0);
                                            itemSaleOrderDetail.SalePrice = itemSaleOrderDetail.RegularPrice;
                                            itemSaleOrderDetail.DiscountAmount = itemSaleOrderDetail.SalePrice;
                                            itemSaleOrderDetail.TotalDiscount = Math.Round(itemSaleOrderDetail.UnitDiscount * itemSaleOrderDetail.Quantity, 0);
                                        }
                                        itemSaleOrderDetail.CalcSubTotal();
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    case 4://Buy X and get Y % off
                        #region Buy X And Get Y % Off
                        if (sumOfItem >= promotionModel.BuyingQty)
                        {
                            if (promotionModel.IsApplyToAboveQuantities)
                            {
                                foreach (var itemSaleOrderDetail in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductResource == saleOrderDetailModel.ProductResource && x.UOMId == saleOrderDetailModel.ProductModel.BaseUOMId))
                                {
                                    //////Calculate Discount
                                    //so tien giảm trên 1 đơn vi
                                    itemSaleOrderDetail.UnitDiscount = itemSaleOrderDetail.RegularPrice * promotionModel.TakeOff / 100;
                                    itemSaleOrderDetail.DiscountPercent = promotionModel.TakeOff;

                                    //Tổng số tiền dc giảm trên tổng số sản phẩm
                                    itemSaleOrderDetail.TotalDiscount = itemSaleOrderDetail.UnitDiscount * itemSaleOrderDetail.Quantity;

                                    //So tien dc giam trên 1 đợn vi
                                    itemSaleOrderDetail.DiscountAmount = itemSaleOrderDetail.RegularPrice - itemSaleOrderDetail.UnitDiscount;
                                    itemSaleOrderDetail.SalePrice = itemSaleOrderDetail.DiscountAmount;
                                }
                            }
                            else
                            {
                                int numberOfItemDiscount = sumOfItem - (sumOfItem % promotionModel.BuyingQty);
                                foreach (var itemSaleOrderDetail in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductResource == saleOrderDetailModel.ProductResource && x.UOMId == saleOrderDetailModel.ProductModel.BaseUOMId).ToList())
                                {
                                    int quantityRemain = itemSaleOrderDetail.Quantity;
                                    if (numberOfItemDiscount > 0)
                                    {
                                        if (itemSaleOrderDetail.Quantity <= numberOfItemDiscount)
                                        {
                                            //////Calculate Discount
                                            //so tien giảm trên 1 đơn vi
                                            itemSaleOrderDetail.UnitDiscount = itemSaleOrderDetail.RegularPrice * promotionModel.TakeOff / 100;
                                            itemSaleOrderDetail.DiscountPercent = promotionModel.TakeOff;

                                            //Tổng số tiền dc giảm trên tổng số sản phẩm
                                            itemSaleOrderDetail.TotalDiscount = Math.Round(itemSaleOrderDetail.UnitDiscount * itemSaleOrderDetail.Quantity, 2);

                                            //So tien dc giam trên 1 đợn vi
                                            itemSaleOrderDetail.DiscountAmount = itemSaleOrderDetail.RegularPrice - itemSaleOrderDetail.UnitDiscount;
                                            itemSaleOrderDetail.SalePrice = itemSaleOrderDetail.DiscountAmount;

                                            CheckToShowDatagridRowDetail(itemSaleOrderDetail);
                                        }
                                        else
                                        {

                                            base_SaleOrderDetailModel replaceItem = NewSaleOrderDetail(itemSaleOrderDetail).Clone();
                                            replaceItem.Quantity = numberOfItemDiscount;
                                            //////Calculate Discount
                                            //so tien giảm trên 1 đơn vi
                                            replaceItem.UnitDiscount = Math.Round(replaceItem.RegularPrice * promotionModel.TakeOff / 100, 1);
                                            replaceItem.DiscountPercent = promotionModel.TakeOff;

                                            //Tổng số tiền dc giảm trên tổng số sản phẩm
                                            replaceItem.TotalDiscount = Math.Round(replaceItem.UnitDiscount * replaceItem.Quantity, 1);

                                            //So tien dc giam trên 1 đợn vi
                                            replaceItem.DiscountAmount = replaceItem.RegularPrice - replaceItem.UnitDiscount;
                                            replaceItem.SalePrice = replaceItem.DiscountAmount;


                                            //Set Serial
                                            if (!string.IsNullOrWhiteSpace(replaceItem.SerialTracking) && replaceItem.ProductModel.IsSerialTracking)
                                            {
                                                IEnumerable<string> remainSerial = replaceItem.SerialTracking.Split(',').Take(replaceItem.Quantity);
                                                replaceItem.SerialTracking = string.Join(", ", remainSerial);
                                            }
                                            replaceItem.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                                            CheckToShowDatagridRowDetail(replaceItem);

                                            var deleteItem = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == itemSaleOrderDetail.Resource);
                                            if (deleteItem != null)
                                            {
                                                deleteItem.PropertyChanged -= new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                                                SelectedSaleOrder.SaleOrderDetailCollection.Remove(deleteItem);
                                            }
                                            SelectedSaleOrder.SaleOrderDetailCollection.Add(replaceItem);
                                            int remainItem = quantityRemain - numberOfItemDiscount;
                                            if (remainItem > 0)
                                            {
                                                base_SaleOrderDetailModel remainSaleOrderDetail = NewSaleOrderDetail(saleOrderDetailModel);
                                                remainSaleOrderDetail.Quantity = remainItem;
                                                //Reset Discount
                                                remainSaleOrderDetail.DiscountPercent = 0;
                                                remainSaleOrderDetail.UnitDiscount = Math.Round(remainSaleOrderDetail.RegularPrice * remainSaleOrderDetail.DiscountPercent, 0);
                                                remainSaleOrderDetail.SalePrice = remainSaleOrderDetail.RegularPrice;
                                                remainSaleOrderDetail.DiscountAmount = remainSaleOrderDetail.SalePrice;
                                                remainSaleOrderDetail.TotalDiscount = Math.Round(remainSaleOrderDetail.UnitDiscount * remainSaleOrderDetail.Quantity, 0);

                                                if (!string.IsNullOrWhiteSpace(remainSaleOrderDetail.SerialTracking) && saleOrderDetailModel.ProductModel.IsSerialTracking)
                                                {
                                                    int skipItem = replaceItem.Quantity;
                                                    IEnumerable<string> remainSerial = remainSaleOrderDetail.SerialTracking.Split(',').Skip(skipItem).Take(remainSaleOrderDetail.Quantity);
                                                    remainSaleOrderDetail.SerialTracking = string.Join(", ", remainSerial);
                                                }
                                                CheckToShowDatagridRowDetail(remainSaleOrderDetail);

                                                remainSaleOrderDetail.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                                                SelectedSaleOrder.SaleOrderDetailCollection.Add(remainSaleOrderDetail);
                                            }
                                        }
                                        numberOfItemDiscount -= quantityRemain;
                                    }
                                    else
                                    {
                                        itemSaleOrderDetail.DiscountPercent = 0;
                                        itemSaleOrderDetail.UnitDiscount = Math.Round(itemSaleOrderDetail.RegularPrice * itemSaleOrderDetail.DiscountPercent, 0);
                                        itemSaleOrderDetail.SalePrice = itemSaleOrderDetail.RegularPrice;
                                        itemSaleOrderDetail.DiscountAmount = itemSaleOrderDetail.SalePrice;
                                        itemSaleOrderDetail.TotalDiscount = Math.Round(itemSaleOrderDetail.UnitDiscount * itemSaleOrderDetail.Quantity, 0);
                                    }
                                    itemSaleOrderDetail.CalcSubTotal();
                                }
                            }
                        }
                        #endregion
                        break;
                    case 5://Coupon
                        break;
                }
            }
        }

        /// <summary>
        /// Calculate Remain Return Quantity
        /// </summary>
        /// <param name="returnDetailModel"></param>
        private void CalculateRemainReturnQty(base_ResourceReturnDetailModel returnDetailModel)
        {
            int TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
            var remainQuantity = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.PickQty) - TotalItemReturn;
            returnDetailModel.ReturnQty = remainQuantity;
        }

        /// <summary>
        /// Calculate Subtotal of Return
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalculateReturnSubtotal(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.ReturnModel != null && saleOrderModel.ReturnModel.ReturnDetailCollection.Any())
                saleOrderModel.ReturnModel.SubTotal = saleOrderModel.ReturnModel.ReturnDetailCollection.Sum(x => x.Amount);
            else
                saleOrderModel.ReturnModel.SubTotal = 0;
        }

        //Update value
        /// <summary>
        /// Get UOM Collection For sale order detail
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="SetPrice"> True : Set price after set Product Unit</param>
        private void GetProductUOMforSaleOrderDetail(base_SaleOrderDetailModel salesOrderDetailModel, bool SetPrice = true)
        {
            salesOrderDetailModel.ProductUOMCollection = new ObservableCollection<base_ProductUOMModel>();
            base_UOMRepository UOMRepository = new base_UOMRepository();
            base_ProductUOMModel productUOM;

            // Add base unit in UOMCollection.
            base_UOM UOM = UOMRepository.Get(x => x.Id == salesOrderDetailModel.ProductModel.BaseUOMId);
            if (UOM != null)
            {
                salesOrderDetailModel.ProductUOMCollection.Add(new base_ProductUOMModel
                {
                    ProductId = salesOrderDetailModel.ProductModel.Id,
                    UOMId = UOM.Id,
                    Code = UOM.Code,
                    Name = UOM.Name,
                    RegularPrice = salesOrderDetailModel.ProductModel.RegularPrice,
                    Price1 = salesOrderDetailModel.ProductModel.Price1,
                    Price2 = salesOrderDetailModel.ProductModel.Price2,
                    Price3 = salesOrderDetailModel.ProductModel.Price3,
                    Price4 = salesOrderDetailModel.ProductModel.Price4,
                    IsNew = false,
                    IsDirty = false
                });
            }

            // Gets the remaining units.
            foreach (base_ProductUOM item in salesOrderDetailModel.ProductModel.base_Product.base_ProductUOM)
            {
                salesOrderDetailModel.ProductUOMCollection.Add(new base_ProductUOMModel(item)
                {
                    Code = item.base_UOM.Code,
                    Name = item.base_UOM.Name,
                    IsDirty = false
                });
            }
            if (SetPrice)
            {
                if (salesOrderDetailModel.ProductModel.SellUOMId.HasValue)
                    productUOM = salesOrderDetailModel.ProductUOMCollection.FirstOrDefault(x => x.UOMId == salesOrderDetailModel.ProductModel.SellUOMId);
                else
                    productUOM = salesOrderDetailModel.ProductUOMCollection.FirstOrDefault(x => x.UOMId == salesOrderDetailModel.ProductModel.BaseUOMId);

                if (productUOM != null)
                {
                    salesOrderDetailModel.UOMId = productUOM.UOMId;
                    salesOrderDetailModel.UnitName = productUOM.Name;
                    salesOrderDetailModel.BaseUOM = productUOM.Code;
                    salesOrderDetailModel.RegularPrice = productUOM.RegularPrice;
                    salesOrderDetailModel.SalePrice = productUOM.RegularPrice;
                }
            }
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
                if (productUOM != null)
                {
                    saleOrderDetailModel.UnitName = productUOM.Name;
                    saleOrderDetailModel.BaseUOM = productUOM.Code;

                    if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.RegularPrice))
                    {
                        //set Price with Price Level
                        saleOrderDetailModel.RegularPrice = productUOM.RegularPrice;
                        saleOrderDetailModel.SalePrice = productUOM.RegularPrice;
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

        /// <summary>
        /// show datagridrow when 
        /// <para>1/. This Product has Serial tracking (IsSerialTracking=true)</para>
        /// <para>2/. SaleOrder Detail has changed price</para>
        /// <para>3/. SaleOrder Detail has apply discount</para>
        /// <para>4/. SaleOrder Detail has apply Reward</para>
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CheckToShowDatagridRowDetail(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            saleOrderDetailModel.IsVisibleRowDetail = false;
            //HasSerial
            saleOrderDetailModel.IsVisibleRowDetail |= saleOrderDetailModel.ProductModel.IsSerialTracking;
            //Sale p
            saleOrderDetailModel.IsVisibleRowDetail |= saleOrderDetailModel.RegularPrice > saleOrderDetailModel.SalePrice;
        }

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
            return result && Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).In(promotionModel.PriceSchemaRange.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        private void TabChanged(int saleTab)
        {
            if (!IsDirty)
                return;
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
                    MessageBox.Show("Please fix error!", "POS", MessageBoxButton.OK);
                    _selectedTabIndex = _previousTabIndex;
                    OnPropertyChanged(() => SelectedTabIndex);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }


        }

        /// <summary>
        /// ShowShip Tab
        /// </summary>
        private void ShowShipTab()
        {
            SelectedSaleOrder.ShipProcess = true;
            SelectedTabIndex = (int)SaleOrderTab.Ship;
            if (SelectedSaleOrder.OrderStatus < (short)SaleOrderStatus.Shipping)
                SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Shipping;
        }

        /// <summary>
        /// Set for SaleOrderStatus when order is Ship full
        /// </summary>
        private void SetShipStatus()
        {
            if (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull))//Not change status when PaidInFull
                return;

            bool ShipAll = true;
            foreach (var item in SelectedSaleOrder.SaleOrderDetailCollection)
            {
                int shipTotal = SelectedSaleOrder.SaleOrderShipCollection.Where(x => x.IsShipped == true).Sum(x => x.SaleOrderShipDetailCollection.Where(y => y.SaleOrderDetailResource == item.Resource.ToString() && y.ProductResource == item.ProductResource).Sum(z => z.PackedQty.Value));
                ShipAll &= shipTotal == item.Qty;
            }
            if (ShipAll)
                SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.FullyShipped;
            else if (SelectedSaleOrder.SaleOrderShipCollection.Any())
                SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Shipping;
        }

        /// <summary>
        /// set user change order follow config & order status
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetAllowChangeOrder(base_SaleOrderModel saleOrderModel)
        {
            if (_initialData)
                return;

            if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Quote))
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
                        CalculateRemainReturnQty(returnDetailModel);
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
                int totalReturn = allReturn.Sum(x => x.ReturnQty);
                int totalShipped = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(item.OrderDetailResource)).Sum(x => x.PickQty);
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

        //Handle from another form
        /// <summary>
        /// Open form tracking serial number
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="isShowQty"></param>
        private void OpenTrackingSerialNumber(base_SaleOrderDetailModel salesOrderDetailModel, bool isShowQty = false)
        {
            //Show Tracking Serial
            SelectTrackingNumberViewModel trackingNumberViewModel = new SelectTrackingNumberViewModel(salesOrderDetailModel, isShowQty);
            bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, trackingNumberViewModel, "Tracking Serial Number");

            if (result == true)
                salesOrderDetailModel = trackingNumberViewModel.SaleOrderDetailModel;
            CalcProductDiscount(salesOrderDetailModel);
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
                        saleOrderDetailModel.SalePrice = updateTransactionViewModel.NewPrice;
                    else
                    {
                        //Update BaseUnit & UnitCollection
                        GetProductUOMforSaleOrderDetail(saleOrderDetailModel);
                    }
                    return saleOrderDetailModel;
                }
            }
            return saleOrderDetailModel;
        }

        #endregion

        #region Propertychanged
        private void SaleOrderDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_initialData)
                return;
            base_SaleOrderDetailModel saleOrderDetailModel = sender as base_SaleOrderDetailModel;
            switch (e.PropertyName)
            {
                case "SalePrice":
                    saleOrderDetailModel.CalcSubTotal();

                    if (SelectedSaleOrder.TaxCodeModel != null)
                    {
                        if (Convert.ToInt32(SelectedSaleOrder.TaxCodeModel.TaxOption).Is(SalesTaxOption.Multi))
                        {
                            SelectedSaleOrder.ProductTaxAmount = CalcMultiTaxForProduct(SelectedSaleOrder);
                        }
                        else if (!Convert.ToInt32(SelectedSaleOrder.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
                        {
                            SelectedSaleOrder.ProductTaxAmount = CalcPriceDependentTax(SelectedSaleOrder);
                        }

                        SelectedSaleOrder.TaxAmount = SelectedSaleOrder.ProductTaxAmount + SelectedSaleOrder.ShipTaxAmount;
                        SelectedSaleOrder.TaxPercent = 0;
                    }

                    CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    break;
                case "Quantity":
                    saleOrderDetailModel.CalcSubTotal();
                    saleOrderDetailModel.CalUnfill();
                    if (!saleOrderDetailModel.ProductModel.IsSerialTracking)
                        CalcProductDiscount(saleOrderDetailModel);
                    SelectedSaleOrder.CalcSubTotal();
                    SetShipStatus();
                    break;
                case "DueQty":
                    saleOrderDetailModel.CalUnfill();
                    break;
                case "UOMId":
                    SetPriceUOM(saleOrderDetailModel);
                    CalcProductDiscount(saleOrderDetailModel, true);
                    break;
                case "SubTotal":
                    SelectedSaleOrder.CalcSubTotal();
                    break;
            }
        }

        private void SelectedSaleOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_SaleOrderModel saleOrderModel = sender as base_SaleOrderModel;
            switch (e.PropertyName)
            {
                case "SubTotal":
                    if (saleOrderModel.TaxCodeModel != null && Convert.ToInt32(saleOrderModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Single))
                    {
                        decimal taxPercent;
                        decimal taxAmount;
                        CalcSingleTax(saleOrderModel, saleOrderModel.SubTotal, out taxPercent, out taxAmount);
                        saleOrderModel.ProductTaxAmount = taxAmount;
                        saleOrderModel.TaxPercent = taxPercent;
                    }
                    saleOrderModel.CalcDiscountPercent();
                    break;
                case "Total":
                case "Deposit":
                    saleOrderModel.CalcBalance();
                    break;
                case "Paid":
                    saleOrderModel.CalcBalance();
                    break;
                case "Shipping":
                    saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
                    saleOrderModel.CalcTotal();
                    break;
                case "ProductTaxAmount":
                case "ShipTaxAmount":
                    if (saleOrderModel.TaxCodeModel.IsTaxAfterDiscount)
                        saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                    else
                        saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;

                    break;
                case "TaxAmount":
                    saleOrderModel.CalcTotal();
                    break;
                case "DiscountAmount":
                    if (saleOrderModel.TaxCodeModel != null)
                    {
                        if (saleOrderModel.TaxCodeModel.IsTaxAfterDiscount)
                            saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                        else
                            saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;
                    }
                    saleOrderModel.CalcTotal();

                    break;
                case "PriceSchemaId"://Update Price When Price Schema Changed
                    {
                        if (saleOrderModel.SaleOrderDetailCollection != null)
                        {
                            foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                            {
                                SetPriceUOM(saleOrderDetailModel);
                                saleOrderDetailModel.CalcSubTotal();
                                CalcProductDiscount(saleOrderDetailModel);
                                //saleOrderDetailModel.CalcSubTotal();
                            }
                            saleOrderModel.CalcSubTotal();
                            saleOrderModel.CalcDiscountPercent();
                            //Need Calculate Total after Subtotal & Discount Percent changed
                            saleOrderModel.CalcTotal();
                        }
                    }
                    break;
                case "OrderStatus":
                    SetAllowChangeOrder(saleOrderModel);
                    break;
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
            base_ResourceReturnDetailModel resourceReturnDetailModel = sender as base_ResourceReturnDetailModel;
            switch (e.PropertyName)
            {
                case "SaleOrderDetailModel":
                    if (resourceReturnDetailModel.SaleOrderDetailModel != null)
                    {
                        resourceReturnDetailModel.OrderDetailResource = resourceReturnDetailModel.SaleOrderDetailModel.Resource.ToString();
                        resourceReturnDetailModel.SaleOrderModel = SelectedSaleOrder;
                        resourceReturnDetailModel.ProductResource = resourceReturnDetailModel.SaleOrderDetailModel.ProductResource;
                        resourceReturnDetailModel.ItemCode = resourceReturnDetailModel.SaleOrderDetailModel.ItemCode;
                        resourceReturnDetailModel.ItemName = resourceReturnDetailModel.SaleOrderDetailModel.ItemName;
                        resourceReturnDetailModel.ItemAtribute = resourceReturnDetailModel.SaleOrderDetailModel.ItemAtribute;
                        resourceReturnDetailModel.ItemSize = resourceReturnDetailModel.SaleOrderDetailModel.ItemSize;
                        resourceReturnDetailModel.UnitName = resourceReturnDetailModel.SaleOrderDetailModel.UnitName;
                        resourceReturnDetailModel.Price = resourceReturnDetailModel.SaleOrderDetailModel.SalePrice;
                        CalculateRemainReturnQty(resourceReturnDetailModel);

                    }
                    else
                    {
                        resourceReturnDetailModel.OrderDetailResource = null;
                        resourceReturnDetailModel.ProductResource = null;
                        resourceReturnDetailModel.ItemCode = null;
                        resourceReturnDetailModel.ItemName = null;
                        resourceReturnDetailModel.ItemAtribute = null;
                        resourceReturnDetailModel.ItemSize = null;
                        resourceReturnDetailModel.Price = 0;
                        resourceReturnDetailModel.ReturnQty = 0;
                    }
                    break;
                case "Price":
                    resourceReturnDetailModel.Amount = resourceReturnDetailModel.Price * resourceReturnDetailModel.ReturnQty;
                    break;
                case "ReturnQty":
                    resourceReturnDetailModel.Amount = resourceReturnDetailModel.Price * resourceReturnDetailModel.ReturnQty;
                    base_SaleOrderDetailModel saleOrderDetail = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(resourceReturnDetailModel.OrderDetailResource));
                    if (saleOrderDetail != null && SelectedSaleOrder != null)
                    {
                        int TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.OrderDetailResource.Equals(resourceReturnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
                        var remainQuantity = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(resourceReturnDetailModel.OrderDetailResource)).Sum(x => x.PickQty) - TotalItemReturn;
                        saleOrderDetail.QtyAfterRerturn = remainQuantity;
                    }
                    break;
                case "Amount":
                    CalculateReturnSubtotal(SelectedSaleOrder);
                    break;
                case "IsReturned":
                    if (resourceReturnDetailModel.IsReturned)
                    {
                        if (!resourceReturnDetailModel.HasError)
                        {
                            MessageBoxResult result = MessageBox.Show("Are you sure you return this item ?", "POS", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    resourceReturnDetailModel.IsReturned = false;
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                resourceReturnDetailModel.IsReturned = false;
                                MessageBox.Show("Fix error(s) before return this item.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
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
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.PropertyChanged -= ResourceReturnDetailModel_PropertyChanged;
                }
            }
        }
        #endregion

        #region Override Methods

        public override void LoadData()
        {
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            short orderStatus = (short)SaleOrderStatus.Quote;
            if (IsQuotation)
                predicate = predicate.And(x => x.OrderStatus == orderStatus && !x.IsPurge);
            else
                predicate = predicate.And(x => x.OrderStatus != orderStatus && !x.IsPurge);

            LoadDataByPredicate(predicate);
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
                    }
                    else
                        IsSearchMode = true;
                }
            }
            else
            {
                if (param is base_SaleOrderModel)
                {
                    IsSearchMode = true;
                    SelectedTabIndex = (int)SaleOrderTab.Order;
                    SelectedSaleOrder = param as base_SaleOrderModel;
                    //Set for selectedCustomer
                    _flagCustomerSetRelate = false;
                    SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));
                    SetAllowChangeOrder(SelectedSaleOrder);
                    SelectedSaleOrder.IsDirty = false;
                    _flagCustomerSetRelate = true;
                    IsSearchMode = false;
                    //Set dont show Ship tab.
                    SelectedSaleOrder.ShipProcess = false;
                    //Chang to Order tab if current on another tab
                    _selectedTabIndex = (short)SaleOrderTab.Order;
                    OnPropertyChanged(() => SelectedTabIndex);
                }
                else
                {
                    ChangeSearchMode(false);
                    IEnumerable<base_ProductModel> productCollection = param as IEnumerable<base_ProductModel>;
                    foreach (base_ProductModel productModel in productCollection)
                    {
                        base_SaleOrderDetailModel salesOrderDetailModel = AddNewSaleOrderDetail(productModel);
                        //Get Product UOMCollection
                        GetProductUOMforSaleOrderDetail(salesOrderDetailModel);
                        //Set Price follow PriceLevel
                        SetPriceUOM(salesOrderDetailModel);
                        //Calculate Discount for product
                        CalcProductDiscount(salesOrderDetailModel);
                        //Check Show gridDetail Discount or Serial
                        CheckToShowDatagridRowDetail(salesOrderDetailModel);
                    }
                    //Open MultiSerialTracking with item has serial tracking
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        IEnumerable<base_SaleOrderDetailModel> saleDetailSerialCollection = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel.IsSerialTracking);
                        if (saleDetailSerialCollection != null && saleDetailSerialCollection.Any())
                        {
                            MultiTrackingNumberViewModel multiTrackingNumber = new MultiTrackingNumberViewModel(saleDetailSerialCollection);
                            bool? dialogResult = _dialogService.ShowDialog<MultiTrackingNumberView>(_ownerViewModel, multiTrackingNumber, "Multi Tracking Serial");
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);


                }
            }
        }

        #endregion

        #region PaymentTab

        /// <summary>
        /// Load payment collection and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void LoadPaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            // Initial payment product enumerable
            _paymentProducts = new List<base_ResourcePaymentProductModel>();

            if (saleOrderModel.PaymentCollection == null)
            {
                // Get payment tax from sale order tax
                saleOrderModel.PaymentTax = saleOrderModel.TaxAmount;
                saleOrderModel.PaymentTaxPercent = saleOrderModel.TaxPercent;

                // Initial payment product collection
                saleOrderModel.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                // Get document resource
                string docResource = saleOrderModel.Resource.ToString();

                // Get all payment by document resource
                IEnumerable<base_ResourcePayment> payments = _paymentRepository.GetAll(x => x.DocumentResource.Equals(docResource));

                foreach (base_ResourcePayment payment in payments)
                {
                    // Create new payment model
                    base_ResourcePaymentModel paymentModel = new base_ResourcePaymentModel(payment);

                    // Load payment product collection
                    paymentModel.PaymentProductCollection = new ObservableCollection<base_ResourcePaymentProductModel>(
                        paymentModel.base_ResourcePayment.base_ResourcePaymentProduct.Select(x => new base_ResourcePaymentProductModel(x)));

                    // Load all payment product from database
                    _paymentProducts = _paymentProducts.Union(paymentModel.PaymentProductCollection);

                    // Load payment collection
                    saleOrderModel.PaymentCollection.Add(paymentModel);
                }

                // Load payment product from shipped product
                LoadPaymentProductCollection(saleOrderModel);
            }
        }

        private void LoadPaymentProductCollection(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.PaymentProductCollection == null && _paymentProducts != null)//Kyan Edited  _paymentProducts!=null:if Sale order is not, it will occur problem
            {
                // Load payment product from payment
                saleOrderModel.PaymentProductCollection = new ObservableCollection<base_ResourcePaymentProductModel>(_paymentProducts);
            }
            else
                saleOrderModel.PaymentProductCollection = new ObservableCollection<base_ResourcePaymentProductModel>();

            // Load payment product from shipped product
            foreach (base_SaleOrderShipDetailModel saleOrderShipDetailItem in saleOrderModel.SaleOrderShipDetailCollection.Where(x => !x.IsPaid && !x.IsChecked))
            {
                saleOrderShipDetailItem.IsChecked = true;

                // Create new payment product model
                base_ResourcePaymentProductModel paymentProductModel = new base_ResourcePaymentProductModel();

                // Copy value to payment product model
                paymentProductModel.CopyFromAndRaise(saleOrderShipDetailItem);

                // Add new payment product to collection
                saleOrderModel.PaymentProductCollection.Add(paymentProductModel);
            }
        }

        /// <summary>
        /// Update payment collection and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="paymentViewModel"></param>
        private void UpdatePaymentCollection(base_SaleOrderModel saleOrderModel, SalesOrderPaymenViewModel paymentViewModel)
        {
            // Initital payment product collection
            paymentViewModel.PaymentModel.PaymentProductCollection = new ObservableCollection<base_ResourcePaymentProductModel>(saleOrderModel.PaymentProductCollection);

            // Add new payment to collection
            saleOrderModel.PaymentCollection.Add(paymentViewModel.PaymentModel);

            // Update ship detail model
            foreach (base_SaleOrderShipDetailModel saleOrderShipDetailItem in saleOrderModel.SaleOrderShipDetailCollection.Where(x => !x.IsPaid))
                saleOrderShipDetailItem.IsPaid = true;
        }

        /// <summary>
        /// Save payment collection, payment detail and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SavePaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.SaleOrderShipDetailCollection != null)
            {
                // Update ship detail model
                foreach (base_SaleOrderShipDetailModel saleOrderShipDetailItem in saleOrderModel.SaleOrderShipDetailCollection.Where(x => x.IsPaid))
                {
                    base_SaleOrderShipDetail _saleOrderShipDetail = _saleOrderShipDetailRepository.Get(x => x.Id.Equals(saleOrderShipDetailItem.base_SaleOrderShipDetail.Id));
                    if (_saleOrderShipDetail != null)
                        _saleOrderShipDetail.IsPaid = true;
                }
            }

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
                            if (paymentDetailModel.IsNew)
                                paymentItem.base_ResourcePayment.base_ResourcePaymentDetail.Add(paymentDetailModel.base_ResourcePaymentDetail);
                        }
                    }

                    if (paymentItem.PaymentProductCollection != null)
                    {
                        foreach (base_ResourcePaymentProductModel paymentProductItem in paymentItem.PaymentProductCollection.Where(x => x.IsDirty))
                        {
                            // Map data from model to entity
                            paymentProductItem.ToEntity();

                            // Add new payment detail to database
                            if (paymentProductItem.IsNew)
                                paymentItem.base_ResourcePayment.base_ResourcePaymentProduct.Add(paymentProductItem.base_ResourcePaymentProduct);

                            // Turn off IsDirty & IsNew
                            paymentProductItem.EndUpdate();
                        }
                    }

                    if (saleOrderModel.PaymentProductCollection != null)
                    {
                        foreach (base_ResourcePaymentProductModel paymentProductItem in saleOrderModel.PaymentProductCollection.Where(x => x.IsDirty))
                        {
                            // Turn off IsDirty & IsNew
                            paymentProductItem.EndUpdate();
                        }
                    }

                    if (paymentItem.IsNew)
                        _paymentRepository.Add(paymentItem.base_ResourcePayment);
                    _paymentRepository.Commit();
                }
            }
        }

        #endregion
    }
}
