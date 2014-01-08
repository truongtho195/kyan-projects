using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;
using CPCToolkitExtLibraries;
using System.Collections.Specialized;

namespace CPC.POS.ViewModel
{
    /// <summary>
    /// Using for Layaway & Quotation
    /// </summary>
    public class LayawayViewModel : ViewModelBase
    {
        #region Define

        public RelayCommand NewCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }

        //Respository
        private base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_SaleTaxLocationRepository _saleTaxRepository = new base_SaleTaxLocationRepository();
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        private base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_ProductStoreRepository _productStoreRespository = new base_ProductStoreRepository();

        private string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        private string EMPLOYEE_MARK = MarkType.Employee.ToDescription();
        private List<base_ProductModel> _productCoupon;
        private bool _viewExisted = false;

        #endregion

        #region Constructors

        public LayawayViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            InitialCommand();
        }

        public LayawayViewModel(bool isList, bool isLayaway = true)
            : this()
        {
            IsLayaway = isLayaway;
            LoadStaticData();
            LoadDynamicData();
            ChangeSearchMode(isList, null);

            // Get permission
            GetPermission();
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

        #region IsLayaway
        private bool _isLayaway;
        /// <summary>
        /// Gets or sets the IsLayaway.
        /// </summary>
        public bool IsLayaway
        {
            get { return _isLayaway; }
            set
            {
                if (_isLayaway != value)
                {
                    _isLayaway = value;
                    OnPropertyChanged(() => IsLayaway);
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
                    || (SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any(x => x.IsDirty))
                    || (SelectedSaleOrder.BillAddressModel != null && SelectedSaleOrder.BillAddressModel.IsDirty)
                    || (SelectedSaleOrder.ShipAddressModel != null && SelectedSaleOrder.ShipAddressModel.IsDirty);
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

        //Static Property
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

        #region CustomerFieldCollection
        ///// <summary>
        ///// Gets or sets the CustomerFieldCollection for Autocomplete Control
        ///// </summary>
        public DataSearchCollection CustomerFieldCollection { get; set; }
        #endregion

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

        //Dynamic Data
        private List<base_PromotionModel> _promotionList { get; set; }

        public List<base_SaleTaxLocationModel> SaleTaxLocationCollection { get; set; }

        #region CustomerCollection
        private ObservableCollection<base_GuestModel> _customerCollection;
        /// <summary>
        /// Gets or sets the CustomerCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> CustomerCollection
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
                        SelectedCustomerChanged();
                }
            }
        }

        #endregion

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
            return IsDirty && IsValid && IsOrderValid;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            bool updateQtyCustomer = IsLayaway ? true : false;
            SaveLayaway(updateQtyCustomer);
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
            return !SelectedSaleOrder.IsNew;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            if (SelectedSaleOrder != null)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this item?", "POS", MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.Yes))
                {
                    SelectedSaleOrder.IsPurge = true;
                    bool updateQtyCustomer = IsLayaway ? true : false;
                    SaveLayaway(updateQtyCustomer);
                    this.SaleOrderCollection.Remove(SelectedSaleOrder);
                    TotalSaleOrder -= 1;
                    _selectedSaleOrder = null;
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

        #region DoubleClickCommand

        public RelayCommand<object> DoubleClickViewCommand { get; private set; }

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

                //Set for selectedCustomer
                _selectedCustomer = SelectedSaleOrder.GuestModel;
                OnPropertyChanged(() => SelectedCustomer);

                SetAllowChangeOrder(SelectedSaleOrder);
                SelectedSaleOrder.IsDirty = false;

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
            PopupAddressViewModel addressViewModel = new PopupAddressViewModel(SelectedSaleOrder.GuestModel,addressModel);

            string strTitle = addressModel.AddressTypeId.Is(AddressType.Billing) ? "Bill Address" : "Ship Address";
            
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
                if (saleOrderDetailModel.PickQty > 0)//|| !IsAllowChangeOrder
                    Xceed.Wpf.Toolkit.MessageBox.Show("This item is picked, can't delete ?", "POS", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (param != null && Convert.ToDecimal(param) != SelectedSaleOrderDetail.Quantity)
            {
                SelectedSaleOrderDetail.Quantity = Convert.ToDecimal(param);

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

            return !string.IsNullOrWhiteSpace(SelectedSaleOrder.CustomerResource) && SelectedSaleOrder.Paid == 0 && SelectedSaleOrder.SubTotal > SelectedSaleOrder.Deposit && (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote) || SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Layaway));

        }

        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary>
        private void OnPaymentCommandExecute(object param)
        {
            DepositProcess();

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
                            HandleOnSaleOrderDetailModel(saleOrderDetailModel);
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
                        HandleOnSaleOrderDetailModel(saleOrderDetailModel);
                        
                    }
                    CalculateMultiNPriceTax();
                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                    SelectedSaleOrder.CalcSubTotal();
                    BreakSODetailChange = false;
                }
            }

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
            object collectionDelete = datagrid.SelectedItems.Cast<object>();
            if (IsLayaway)
                return datagrid.SelectedItems.Count > 0
                    && !(collectionDelete as ObservableCollection<object>).Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Layaway));
            else
                return datagrid.SelectedItems.Count > 0
                    && !(collectionDelete as ObservableCollection<object>).Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Quote));

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
            if (param == null || (param as ObservableCollection<object>).Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Quote) || !x.OrderStatus.Equals((short)SaleOrderStatus.Layaway)))
            {
                if (IsLayaway)
                    Xceed.Wpf.Toolkit.MessageBox.Show("delete these items is Layaway", "POS", MessageBoxButton.OK);
                else
                    Xceed.Wpf.Toolkit.MessageBox.Show("delete these items is Quoation", "POS", MessageBoxButton.OK);
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



            bool updateQtyCustomer = IsLayaway ? true : false;
            SaveLayaway(updateQtyCustomer);

            _selectedCustomer = null;
            //Set for selectedCustomer
            _selectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));
            OnPropertyChanged(() => SelectedCustomer);
            SetAllowChangeOrder(SelectedSaleOrder);
            SelectedSaleOrder.IsDirty = false;
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

            return !SelectedSaleOrder.IsNew && IsValid && IsOrderValid && AllowAddSaleOrder &&
                (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Layaway) ||
                SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote));
        }

        /// <summary>
        /// Method to invoke when the ConvertToSaleOrder command is executed.
        /// </summary>
        private void OnConvertToSaleOrderCommandExecute(object param)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
                SelectedSaleOrder.IsConverted = true;

                bool updateOnCustomer = IsLayaway ? false : true;
                SaveLayaway(updateOnCustomer/*Update Product Quantity*/);
                IsSearchMode = true;
                ComboItem cmbValue = new ComboItem();
                cmbValue.Text = "Quotation";
                cmbValue.Detail = SelectedSaleOrder.Id;
                (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", cmbValue);
            }), System.Windows.Threading.DispatcherPriority.Background);

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
            return datagrid.SelectedItems.Count == 1 && AllowAddSaleOrder &&
                ((datagrid.SelectedItem as base_SaleOrderModel).OrderStatus == (short)SaleOrderStatus.Quote ||
                (datagrid.SelectedItem as base_SaleOrderModel).OrderStatus == (short)SaleOrderStatus.Layaway);
        }

        /// <summary>
        /// Method to invoke when the ConvertItemToSaleOrder command is executed.
        /// </summary>
        private void OnConvertItemToSaleOrderCommandExecute(object param)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                DataGridControl datagrid = param as DataGridControl;
                _selectedSaleOrder = datagrid.SelectedItem as base_SaleOrderModel;
                _selectedSaleOrder.IsConverted = true;
                _selectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
                bool updateOnCustomer = IsLayaway ? false : true;
                SetSaleOrderRelation(SelectedSaleOrder);
                SaveLayaway(updateOnCustomer);
                IsSearchMode = true;
                ComboItem cmbValue = new ComboItem();
                cmbValue.Text = "Quotation";
                cmbValue.Detail = SelectedSaleOrder.Id;
                (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", cmbValue);
            }), System.Windows.Threading.DispatcherPriority.Background);

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

        #region DepositHistory Command
        /// <summary>
        /// Gets the DepositHistory Command.
        /// <summary>

        public RelayCommand<object> DepositHistoryCommand { get; private set; }



        /// <summary>
        /// Method to check whether the DepositHistory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDepositHistoryCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DepositHistory command is executed.
        /// </summary>
        private void OnDepositHistoryCommandExecute(object param)
        {
            DepositHistoryProcess();
        }

        #endregion

        #region Refund Command
        /// <summary>
        /// Gets the Refund Command.
        /// <summary>

        public RelayCommand<object> RefundCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Refund command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRefundCommandCanExecute(object param)
        {
            return SelectedSaleOrder != null && SelectedSaleOrder.Deposit > 0 && (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Layaway) || SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote));
        }


        /// <summary>
        /// Method to invoke when the Refund command is executed.
        /// </summary>
        private void OnRefundCommandExecute(object param)
        {
            string msg = string.Format("Customer is desposit {0} \nDo you want to refund all?", string.Format(Define.ConverterCulture, Define.CurrencyFormat, SelectedSaleOrder.Deposit));
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(msg, "Refund", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result.Equals(MessageBoxResult.Yes))
            {
                if (SelectedSaleOrder.PaymentCollection == null)
                    SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();
                base_ResourcePaymentModel refundPaymentModel = new base_ResourcePaymentModel()
                {
                    IsDeposit = true,
                    DocumentResource = SelectedSaleOrder.Resource.ToString(),
                    DocumentNo = SelectedSaleOrder.SONumber,
                    DateCreated = DateTime.Now,
                    UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty,
                    Resource = Guid.NewGuid(),
                    TotalAmount = SelectedSaleOrder.SubTotal,
                    TotalPaid = -SelectedSaleOrder.Deposit.Value

                };
                if (Define.CONFIGURATION.DefaultCashiedUserName.HasValue && Define.CONFIGURATION.DefaultCashiedUserName.Value)
                    refundPaymentModel.Cashier = Define.USER.LoginName;
                SelectedSaleOrder.PaymentCollection.Add(refundPaymentModel);
                SelectedSaleOrder.Deposit = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            // Route the commands
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
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
            EditProductCommand = new RelayCommand<object>(OnEditProductCommandExecute, OnEditProductCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);
            //Using for Main Datagrid
            DeleteItemsCommand = new RelayCommand<object>(OnDeleteItemsCommandExecute, OnDeleteItemsCommandCanExecute);
            DeleteItemsWithKeyCommand = new RelayCommand<object>(OnDeleteItemsWithKeyCommandExecute, OnDeleteItemsWithKeyCommandCanExecute);
            DuplicateItemCommand = new RelayCommand<object>(OnDuplicateItemCommandExecute, OnDuplicateItemCommandCanExecute);
            EditItemCommand = new RelayCommand<object>(OnEditItemCommandExecute, OnEditItemCommandCanExecute);
            SaleOrderAdvanceSearchCommand = new RelayCommand<object>(OnSaleOrderAdvanceSearchCommandExecute, OnSaleOrderAdvanceSearchCommandCanExecute);

            //Quotation
            ConvertToSaleOrderCommand = new RelayCommand<object>(OnConvertToSaleOrderCommandExecute, OnConvertToSaleOrderCommandCanExecute);
            ConvertItemToSaleOrderCommand = new RelayCommand<object>(OnConvertItemToSaleOrderCommandExecute, OnConvertItemToSaleOrderCommandCanExecute);
            SerialTrackingDetailCommand = new RelayCommand<object>(OnSerialTrackingDetailCommandExecute, OnSerialTrackingDetailCommandCanExecute);
            DepositHistoryCommand = new RelayCommand<object>(OnDepositHistoryCommandExecute, OnDepositHistoryCommandCanExecute);
            RefundCommand = new RelayCommand<object>(OnRefundCommandExecute, OnRefundCommandCanExecute);
        }

        /// <summary>
        /// Load Static Data
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

            //Load Customer
            LoadCustomer();

            //Load Employee for sale rep
            LoadEmployee();

            //Load SaleTax
            LoadSaleTax();

            //load Products
            LoadProducts();
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
        /// Load Product From Database
        /// </summary>
        private void LoadProducts()
        {
            IList<base_Product> products = _productRepository.GetAll(x => !x.IsPurge.Value);

            if (ProductCollection == null)
                ProductCollection = new ObservableCollection<base_ProductModel>(products.Select(x => new base_ProductModel(x)));
            else
            {
                foreach (base_Product product in products)
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
            //Layaway Not Use Coupon

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
        /// LoadDataByPredicate
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
            Console.WriteLine("Searching");
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                SaleOrderCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                IsBusy = true;
                predicate = predicate.And(x => !x.IsPurge && !x.IsLocked);


                if (IsLayaway)
                {
                    string layawayMark = MarkType.Layaway.ToDescription();
                    predicate = predicate.And(x => x.Mark == layawayMark);
                }
                else
                {
                    string quotationMark = MarkType.Quotation.ToDescription();
                    predicate = predicate.And(x => x.Mark == quotationMark);
                }

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
                if (_viewExisted && !IsSearchMode && SelectedSaleOrder != null && SaleOrderCollection.Any() && !SelectedSaleOrder.IsNew) //Item is selected
                {
                    SelectedSaleOrder = SaleOrderCollection.SingleOrDefault(x => x.Id.Equals(SelectedSaleOrder.Id));
                    SetSaleOrderRelation(SelectedSaleOrder, true);
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
        /// <param name="saleOrderModel"></param>
        private void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            //_initialData = true;
            BreakAllChange = true;

            //Get CustomerModel
            saleOrderModel.GuestModel = CustomerCollection.Where(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)).FirstOrDefault();

            _saleOrderRepository.SetGuestAdditionalModel(saleOrderModel);

            //Get Reward & set PurchaseThreshold if Customer any reward
            DateTime orderDate = saleOrderModel.OrderDate.Value.Date;
            //Ingore saleOrderModel.SubTotal >= x.PurchaseThreshold to show require reward
            var reward = GetReward(orderDate);
            saleOrderModel.IsApplyReward = saleOrderModel.GuestModel.IsRewardMember;
            saleOrderModel.IsApplyReward &= reward != null ? true : false;

            //if (!saleOrderModel.GuestModel.GuestRewardCollection.Any() && reward != null)
            if (saleOrderModel.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;


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

            //Check Deposit is accepted?
            saleOrderModel.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit

            //Set Address
            _saleOrderRepository.SetBillShipAddress(saleOrderModel.GuestModel, saleOrderModel);
            saleOrderModel.RaiseAnyShipped();
            //_initialData = false;
            BreakAllChange = false;
            saleOrderModel.IsDirty = false;
        }

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

        private void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            SetSaleOrderDetail(saleOrderModel, isForce);
            SetCustomerRewardCollection(saleOrderModel, isForce);
            LoadPaymentCollection(saleOrderModel);
            saleOrderModel.RaiseAnyShipped();
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
        //Create
        /// <summary>
        /// 
        /// </summary>
        private base_SaleOrderModel CreateNewSaleOrder()
        {
            _selectedSaleOrder = new base_SaleOrderModel();
            _selectedSaleOrder.Shift = Define.ShiftCode;
            _selectedSaleOrder.IsTaxExemption = false;
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
            _selectedSaleOrder.Mark = IsLayaway ? MarkType.Layaway.ToDescription() : MarkType.Quotation.ToDescription();
            _selectedSaleOrder.OrderStatus = IsLayaway ? (short)SaleOrderStatus.Layaway : (short)SaleOrderStatus.Quote;
            _selectedSaleOrder.TermNetDue = 0;
            _selectedSaleOrder.TermDiscountPercent = 0;
            _selectedSaleOrder.TermPaidWithinDay = 0;
            _selectedSaleOrder.PaymentTermDescription = string.Empty;
            _selectedSaleOrder.PriceSchemaId = 1;
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

            //Additional
            _selectedSaleOrder.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing, IsDirty = false };
            _selectedSaleOrder.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping, IsDirty = false };
            SelectedCustomer = null;

            SetAllowChangeOrder(_selectedSaleOrder);
            _selectedSaleOrder.IsDirty = false;
            _selectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
            OnPropertyChanged(() => SelectedSaleOrder);
            return _selectedSaleOrder;
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
            salesOrderDetailModel.PromotionId = 0;
            salesOrderDetailModel.SerialTracking = string.Empty;
            salesOrderDetailModel.TaxCode = productModel.TaxCode;
            salesOrderDetailModel.ItemCode = productModel.Code;
            salesOrderDetailModel.ItemName = productModel.ProductName;
            salesOrderDetailModel.ProductResource = productModel.Resource.ToString();
            //salesOrderDetailModel.OnHandQty = GetUpdateProduct(productModel);
            salesOrderDetailModel.ItemAtribute = productModel.Attribute;
            salesOrderDetailModel.ItemSize = productModel.Size;
            salesOrderDetailModel.ProductModel = productModel;

            salesOrderDetailModel.CalcSubTotal();
            salesOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
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
        /// Insert New sale order
        /// </summary>
        private void InsertLayaway()
        {
            if (SelectedSaleOrder.IsNew)
            {
                UpdateCustomerAddress(SelectedSaleOrder.BillAddressModel);
                SelectedSaleOrder.BillAddressId = SelectedSaleOrder.BillAddressModel.Id;
                UpdateCustomerAddress(SelectedSaleOrder.ShipAddressModel);
                SelectedSaleOrder.ShipAddressId = SelectedSaleOrder.ShipAddressModel.Id;
                //Sale Order Detail Model
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    if (IsLayaway)
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
                SaleOrderCollection.Add(SelectedSaleOrder);
            }
        }

        /// <summary>
        /// UpdateQuote
        /// </summary>
        /// <param name="UpdateQtyCustomer"></param>
        private void UpdateLayaway(bool UpdateQtyCustomer = false)
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
                {
                    if (IsLayaway)
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderDetailModel.Quantity, false/*=Descrease*/);
                    _saleOrderDetailRepository.Delete(saleOrderDetailModel.base_SaleOrderDetail);
                }
                _saleOrderDetailRepository.Commit();
                SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Clear();
            }
            if (SelectedSaleOrder.IsPurge && IsLayaway)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                }
            }
            else
            {
                //Sale Order Detail Model
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    if (IsLayaway)
                    {
                        if (saleOrderDetailModel.Quantity != saleOrderDetailModel.base_SaleOrderDetail.Quantity || saleOrderDetailModel.UOMId != saleOrderDetailModel.base_SaleOrderDetail.UOMId && UpdateQtyCustomer)
                        {
                           _saleOrderRepository.UpdateCustomerQuantityChanged(saleOrderDetailModel, SelectedSaleOrder.StoreCode);
                        }
                    }
                    else if (UpdateQtyCustomer)// Convert To SO => Update Qty Onhand on Customer
                    {
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderDetailModel.Quantity);
                    }



                    ////Need to check difference store code (user change to another store)
                    //if (SelectedSaleOrder.StoreCode.Equals(SelectedSaleOrder.base_SaleOrder.StoreCode))
                    //{
                    //    int quantity = saleOrderDetailModel.Quantity - saleOrderDetailModel.base_SaleOrderDetail.Quantity;
                    //    UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, quantity);
                    //}
                    //else
                    //{
                    //    //Subtract quantity from "old store"(user change to another store)
                    //    UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                    //    //Add quantity to new store
                    //    UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderDetailModel.Quantity);
                    //}


                    saleOrderDetailModel.ToEntity();
                    if (saleOrderDetailModel.IsNew)
                        SelectedSaleOrder.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                }

            }

            _productRepository.Commit();
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
        /// SalveQuote
        /// </summary>
        /// <returns></returns>
        private bool SaveLayaway(bool UpdateQtyCustomer = false)
        {
            bool result = false;
            try
            {
                UnitOfWork.BeginTransaction();
                if (SelectedSaleOrder.IsNew)
                    InsertLayaway();
                else
                    UpdateLayaway(UpdateQtyCustomer);

                UpdateCustomer(SelectedSaleOrder);
                UnitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                result = false;
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            if (paymentDetailModel.IsNew)
                                paymentItem.base_ResourcePayment.base_ResourcePaymentDetail.Add(paymentDetailModel.base_ResourcePaymentDetail);
                        }
                    }

                    if (paymentItem.IsNew)
                        _paymentRepository.Add(paymentItem.base_ResourcePayment);
                    _paymentRepository.Commit();
                }
            }
        }

     
        //View Util
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
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        bool updateQtyCustomer = IsLayaway ? false : true;
                        result = SaveLayaway(updateQtyCustomer);

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
                        SetSaleOrderRelation(SelectedSaleOrder, true);
                    }
                }
            }

            return result;
        }

        //Selected Item
        /// <summary>
        /// Selected Customer of autocomplete changed
        /// <param name="setRelation">set value using for Change customer</param>
        /// </summary>
        private void SelectedCustomerChanged()
        {
            SelectedSaleOrder.CustomerResource = SelectedCustomer.Resource.ToString();
            SelectedSaleOrder.GuestModel = CustomerCollection.Where(x => x.Resource.Equals(SelectedCustomer.Resource)).FirstOrDefault();

            //Get Reward & set PurchaseThreshold if Customer any reward
            DateTime orderDate = SelectedSaleOrder.OrderDate.Value.Date;
            var reward = GetReward(orderDate);
            SelectedSaleOrder.IsApplyReward = SelectedSaleOrder.GuestModel.IsRewardMember;
            SelectedSaleOrder.IsApplyReward &= reward != null ? true : false;
            //isReward Member
            if (SelectedSaleOrder.GuestModel.IsRewardMember)
            {
                //Get GuestReward collection
                SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();

                SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                foreach (base_GuestReward guestReward in SelectedSaleOrder.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(SelectedSaleOrder.GuestModel.Id) && !x.IsApply && x.ActivedDate.Value <= DateTime.Today && (!x.ExpireDate.HasValue || x.ExpireDate.HasValue && DateTime.Today <= x.ExpireDate.Value)))
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
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("This customer is not currently a member of reward program. Do you want to enroll this one?", "POS - Reward Program", MessageBoxButton.YesNo, MessageBoxImage.Information);
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
                    string msg = string.Format("Do you want to apply {0} tax", saleTaxLocation.Name);
                    MessageBoxResult resultMsg = Xceed.Wpf.Toolkit.MessageBox.Show(msg, "POS", MessageBoxButton.YesNo);
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

        private base_RewardManager GetReward(DateTime orderDate)
        {
            var reward = _rewardManagerRepository.Get(x =>
                                        x.IsActived
                                        && ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                        || !x.IsTrackingPeriod)));
            return reward;
        }

        /// <summary>
        /// Selected Product changed
        /// </summary>
        private void SelectedProductChanged()
        {
            if (SelectedProduct != null)
            {
                SaleProductHandle(SelectedProduct);
            }
            _selectedProduct = null;
        }

        //Get Set Value
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
            else if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Quote) || saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Layaway))
                this.IsAllowChangeOrder = true;
            else if (saleOrderModel.Paid > 0)/*has paid*/
                this.IsAllowChangeOrder = false;
            else if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull)
                || saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Close)
                || saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Open)
                || saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.FullyShipped))
                this.IsAllowChangeOrder = false;
            else
                this.IsAllowChangeOrder = saleOrderModel.OrderStatus == (short)SaleOrderStatus.FullyShipped && Define.CONFIGURATION.IsAllowChangeOrder.Value;

        }

        private void GetSaleTax(base_SaleOrderModel saleOrderModel)
        {
            saleOrderModel.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == saleOrderModel.TaxLocation);
            //Get Tax Code
            saleOrderModel.TaxLocationModel.TaxCodeModel = SaleTaxLocationCollection.SingleOrDefault(x => x.ParentId == saleOrderModel.TaxLocationModel.Id && x.TaxCode.Equals(saleOrderModel.TaxCode));
            if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(saleOrderModel.TaxLocationModel.TaxCodeModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)));
        }

        /// <summary>
        /// Handle what relation with saleoder detail
        /// <para>Check Need to show row detail</para>
        /// <para>Calculate Subtotal</para>
        /// <para>Calculate Onhand store</para>
        /// </summary>
        /// <param name="replaceItem"></param>
        private void HandleOnSaleOrderDetailModel(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
            saleOrderDetailModel.CalcSubTotal();
            CalcOnHandStore(saleOrderDetailModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CalcOnHandStore(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            saleOrderDetailModel.IsQuantityAccepted = true;

            saleOrderDetailModel.RaiseIsQuantityAccepted();
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
        /// Get UOM Collection For sale order detail
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="SetPrice"> True : Set price after set Product Unit</param>
        public void GetProductUOMforSaleOrderDetail(CPC.POS.Model.base_SaleOrderDetailModel salesOrderDetailModel, bool SetPrice = true)
        {
            salesOrderDetailModel.ProductUOMCollection = new System.Collections.ObjectModel.ObservableCollection<CPC.POS.Model.base_ProductUOMModel>();

            CPC.POS.Model.base_ProductUOMModel productUOM;

            _productStoreRespository.Refresh(salesOrderDetailModel.ProductModel.base_Product.base_ProductStore);
            base_ProductStore productStore = salesOrderDetailModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));

            // Add base unit in UOMCollection.
            base_UOM UOM = UnitOfWork.Get<base_UOM>(x => x.Id == salesOrderDetailModel.ProductModel.BaseUOMId);

            if (UOM != null)
            {
                salesOrderDetailModel.ProductUOMCollection.Add(new CPC.POS.Model.base_ProductUOMModel
                {
                    //ProductId = salesOrderDetailModel.ProductModel.Id,
                    UOMId = UOM.Id,
                    Code = UOM.Code,
                    Name = UOM.Name,
                    QuantityOnHand = productStore != null ? productStore.QuantityOnHand : 0,
                    RegularPrice = salesOrderDetailModel.ProductModel.RegularPrice,
                    Price1 = salesOrderDetailModel.ProductModel.Price1,
                    Price2 = salesOrderDetailModel.ProductModel.Price2,
                    Price3 = salesOrderDetailModel.ProductModel.Price3,
                    Price4 = salesOrderDetailModel.ProductModel.Price4,
                    BaseUnitNumber = 1,
                    IsNew = false,
                    IsDirty = false
                });
            }

            if (productStore != null)
            {
                UnitOfWork.Refresh<base_ProductUOM>(productStore.base_ProductUOM);
                // Gets the remaining units.
                foreach (base_ProductUOM item in productStore.base_ProductUOM)
                {
                    salesOrderDetailModel.ProductUOMCollection.Add(new CPC.POS.Model.base_ProductUOMModel(item)
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
                        salesOrderDetailModel.UOM = productUOM.Name;
                        salesOrderDetailModel.RegularPrice = productUOM.RegularPrice;
                        if (salesOrderDetailModel.RegularPrice > 0)
                            salesOrderDetailModel.SalePrice = productUOM.RegularPrice;
                    }
                }
            }
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

        //Calculation
        #region Calculate Tax
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
        /// Calculate multi, price dependent tax when sale price changed
        /// </summary>
        private void CalculateMultiNPriceTax()
        {
            if (SelectedSaleOrder.TaxLocationModel.TaxCodeModel != null)
            {
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

                //End foreach
            }
            return taxAmount;
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
                    HandleOnSaleOrderDetailModel(saleOrderDetailModel);
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
                HandleOnSaleOrderDetailModel(saleOrderDetailModel);
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
        }

        #endregion

        //Handle From Onther Form
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
                            productUpdate.RegularPrice = updateTransactionViewModel.ProductModel.RegularPrice;
                        ////Update BaseUnit & UnitCollection
                        _saleOrderRepository.GetProductUOMforSaleOrderDetail(saleOrderDetailModel);
                    }
                    return saleOrderDetailModel;
                }
            }
            return saleOrderDetailModel;
        }

        /// <summary>
        /// Open form tracking serial number
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="isShowQty"></param>
        private void OpenTrackingSerialNumber(base_SaleOrderDetailModel salesOrderDetailModel, bool isShowQty = false, bool isEditing = true)
        {
            //Show Tracking Serial
            SelectTrackingNumberViewModel trackingNumberViewModel = new SelectTrackingNumberViewModel(salesOrderDetailModel, isShowQty, isEditing);
            bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, trackingNumberViewModel, "Tracking Serial Number");

            if (result == true)
            {
                if (isEditing)
                {
                    salesOrderDetailModel = trackingNumberViewModel.SaleOrderDetailModel;
                    if (SelectedSaleOrder.GuestModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((int)PriceLevelType.FixedDiscountOnAllItems))
                        CalcDiscountWithAdditional(salesOrderDetailModel);
                    else
                        CalcProductDiscountWithProgram(salesOrderDetailModel);
                }
            }

        }

        /// <summary>
        /// Deposite for Quotation
        /// </summary>
        private void DepositProcess()
        {
            if (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote) || SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Layaway))
                SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total;

            decimal balance = SelectedSaleOrder.RewardAmount - SelectedSaleOrder.Deposit.Value;
            decimal depositTaken = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);

            //Show Payment
            SalesOrderPaymenViewModel paymentViewModel = new SalesOrderPaymenViewModel(SelectedSaleOrder, balance, depositTaken);
            bool? dialogResult = _dialogService.ShowDialog<DepositPaymentView>(_ownerViewModel, paymentViewModel, "Deposit");
            if (dialogResult == true)
            {
                if (Define.CONFIGURATION.DefaultCashiedUserName.HasValue && Define.CONFIGURATION.DefaultCashiedUserName.Value)
                    paymentViewModel.PaymentModel.Cashier = Define.USER.LoginName;

                if (SelectedSaleOrder.PaymentCollection == null)
                    SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                SelectedSaleOrder.PaymentCollection.Add(paymentViewModel.PaymentModel);
                SelectedSaleOrder.Deposit = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
                SelectedSaleOrder.CalcSubTotal();
            }

            SelectedSaleOrder.PaymentProcess = SelectedSaleOrder.PaymentCollection.Any();
        }

        /// <summary>
        /// Search product with advance options..
        /// </summary>
        private void SearchProductAdvance()
        {
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel(false);
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, "Search Product");
            if (dialogResult == true)
            {
                CreateSaleOrderDetailWithProducts(productSearchViewModel.SelectedProducts);
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

                IEnumerable<base_SaleOrderDetailModel> saleDetailSerialCollection = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel.IsSerialTracking);
                if (saleDetailSerialCollection != null && saleDetailSerialCollection.Any())
                {
                    MultiTrackingNumberViewModel multiTrackingNumber = new MultiTrackingNumberViewModel(saleDetailSerialCollection);
                    bool? dialogResult = _dialogService.ShowDialog<MultiTrackingNumberView>(_ownerViewModel, multiTrackingNumber, "Multi Tracking Serial");

                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Open Sale Order or Quotation Advance Search
        /// </summary>
        private void OpenSOAdvanceSearch()
        {
            POSOAdvanceSearchViewModel viewModel = new POSOAdvanceSearchViewModel(false);
            bool? dialogResult = _dialogService.ShowDialog<POSOAdvanceSearchView>(_ownerViewModel, viewModel, "Layaway Advance Search");

            if (dialogResult == true)
            {
                Expression<Func<base_SaleOrder, bool>> predicate = viewModel.SOPredicate;
                LoadDataByPredicate(predicate, false, 0);
            }
        }

        /// <summary>
        /// {Quotation View}
        /// Show Popup Quoation Payment History
        /// </summary>
        private void DepositHistoryProcess()
        {
            SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total;
            decimal balance = SelectedSaleOrder.RewardAmount - (SelectedSaleOrder.Deposit.Value + SelectedSaleOrder.Paid);
            decimal depositTaken = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);

            QuotationPaymentHistoryViewModel viewModel = new QuotationPaymentHistoryViewModel(SelectedSaleOrder, balance, depositTaken);

            string title = Language.GetMsg("SO_Message_QuotePaymentHistory");
            if (IsLayaway)
                title = Language.GetMsg("SO_Message_LayawayPaymentHistory");

            bool? dialogResult = _dialogService.ShowDialog<QuotationPaymentHistoryView>(_ownerViewModel, viewModel, title);

            if (dialogResult == true)
            {
                switch (viewModel.ViewActionType)
                {
                    case QuotationPaymentHistoryViewModel.PopupType.Deposit:
                        DepositProcess();
                        break;
                    case QuotationPaymentHistoryViewModel.PopupType.Refund:
                        break;
                }
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
                decimal parentPickQty = totalOfPick * parentSaleOrderDetailModel.Quantity / totalQty;
                parentSaleOrderDetailModel.PickQty = Math.Round(parentPickQty, 2);
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
            }
            else
            {

                //Get Product UOMCollection
                _saleOrderRepository.GetProductUOMforSaleOrderDetail(salesOrderDetailModel);

                SetPriceUOM(salesOrderDetailModel);

                //Update price when regular price =0
                salesOrderDetailModel = UpdateProductPrice(salesOrderDetailModel);

                //SetUnit Price for productInGroup
                if (!string.IsNullOrWhiteSpace(ParentResource))
                {
                    int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
                    decimal totalPriceProductGroup = productParentModel.base_Product.base_ProductGroup1.Sum(x => x.Amount);
                    base_ProductGroup productGroup = productParentModel.base_Product.base_ProductGroup1.SingleOrDefault(x => x.ProductId.Equals(productModel.Id));
                    base_SaleOrderDetailModel salesOrderDetailParentModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(ParentResource));
                    if (productGroup != null && salesOrderDetailParentModel != null)
                    {
                        decimal unitPrice = productGroup.RegularPrice + (productGroup.RegularPrice * (salesOrderDetailParentModel.SalePrice - totalPriceProductGroup) / totalPriceProductGroup);
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
            return salesOrderDetailModel.Resource.ToString();
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
        #endregion

        #region PropertyChanged

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
                    saleOrderModel.RewardAmount = saleOrderModel.Total;
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
                    break;
                case "OrderStatus":
                    SetAllowChangeOrder(saleOrderModel);
                    break;
                case "StoreCode":
                    StoreChanged();
                    break;
                case "TotalPaid":

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
        #endregion

        #region Override Methods

        public override void LoadData()
        {
            //IsBusy = false;
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Flag When Existed view Call LoadStatic Data
                if (_viewExisted)
                    LoadDynamicData();

                Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
                LoadDataByPredicate(predicate);
                _viewExisted = true;

            }), System.Windows.Threading.DispatcherPriority.Background);
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
        }

        #endregion

        #region Permission

        #region Properties

        private bool _allowConvertToSO = true;
        /// <summary>
        /// Gets or sets the AllowConvertToSO.
        /// </summary>
        public bool AllowConvertToSO
        {
            get { return _allowConvertToSO; }
            set
            {
                if (_allowConvertToSO != value)
                {
                    _allowConvertToSO = value;
                    OnPropertyChanged(() => AllowConvertToSO);
                }
            }
        }

        private bool _allowAddSaleOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddSaleOrder.
        /// </summary>
        public bool AllowAddSaleOrder
        {
            get { return _allowAddSaleOrder; }
            set
            {
                if (_allowAddSaleOrder != value)
                {
                    _allowAddSaleOrder = value;
                    OnPropertyChanged(() => AllowAddSaleOrder);
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

                if (IsLayaway)
                {
                    // Get sale layaway permission
                    AllowConvertToSO = userRightCodes.Contains("SO100-05-04");
                }
                else
                {
                    // Get sale quotation permission
                    AllowConvertToSO = userRightCodes.Contains("SO100-03-08");
                }

                // Union add/copy sale order and allow convert to sale order permission
                AllowAddSaleOrder = userRightCodes.Contains("SO100-04-02") && AllowConvertToSO;

                // Get add/copy customer permission
                AllowAddCustomer = userRightCodes.Contains("SO100-01-01");

                if (IsLayaway)
                {
                    // Get delete product in layaway permission
                    AllowDeleteProduct = userRightCodes.Contains("SO100-05-08");
                }
                else
                {
                    // Get delete product in quotation permission
                    AllowDeleteProduct = userRightCodes.Contains("SO100-03-07");
                }
            }
        }

        #endregion
    }
}