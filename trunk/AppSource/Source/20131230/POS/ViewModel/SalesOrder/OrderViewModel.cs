using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;
using CPC.POS.View;
using System.Collections.ObjectModel;
using CPC.Helper;
using System.Windows;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Data.Objects;
using System.Windows.Threading;

namespace CPC.POS.ViewModel
{
    public class OrderViewModel : ViewModelBase
    {
        #region Define
        //Respository
        protected base_GuestRepository _guestRepository = new base_GuestRepository();
        protected base_ProductRepository _productRepository = new base_ProductRepository();
        protected base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        protected base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        protected base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        protected base_SaleTaxLocationRepository _saleTaxRepository = new base_SaleTaxLocationRepository();
        protected base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        protected base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        protected base_CardManagementRepository _cardManagementRepository = new base_CardManagementRepository();
        protected base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();
        protected base_GuestRewardSaleOrderRepository _guestRewardSaleOrderRepository = new base_GuestRewardSaleOrderRepository();

        protected string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        protected string EMPLOYEE_MARK = MarkType.Employee.ToDescription();

        protected bool _viewExisted = false;
        protected bool _customerSetRelation = true;

        protected bool _requireProductCard = false;

        /// <summary>
        /// Number item add new to collection
        /// <para>Using For load step item</para>
        /// </summary>
        protected int _numberNewItem = 0;

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        /// <summary>
        /// Predicate for condition searching
        /// </summary>
        protected Expression<Func<base_SaleOrder, bool>> _predicate;
        #endregion

        #region Constructors
        public OrderViewModel()
        {
            InitialCommand();
            LoadStaticData();
            _ownerViewModel = App.Current.MainWindow.DataContext;

            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = TimeSpan.FromSeconds(1);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }

            POSConfig = Define.CONFIGURATION;
        }
        #endregion

        #region Properties
        //Form Property
        #region POSConfig
        private base_ConfigurationModel _posConfig;
        /// <summary>
        /// Gets or sets the POSConfi.
        /// </summary>
        public base_ConfigurationModel POSConfig
        {
            get { return _posConfig; }
            set
            {
                if (_posConfig != value)
                {
                    _posConfig = value;
                    OnPropertyChanged(() => POSConfig);
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

        public MarkType MarkType { get; set; }

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

        /// <summary>
        /// Deactive Customer
        /// </summary>
        public List<base_GuestModel> CustomerList { get; set; }
        #endregion

        #region SelectedCustomer

        protected base_GuestModel _selectedCustomer;
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
                    SelectedCustomerChanging(value);
                    _selectedCustomer = value;
                    OnPropertyChanged(() => SelectedCustomer);
                    SelectedCustomerChanged();
                }
            }
        }



        #endregion

        //Product
        #region ProductCollection
        //private ObservableCollection<base_ProductModel> _productCollection;
        ///// <summary>
        ///// Gets or sets the ProductCollection.
        ///// </summary>
        //public ObservableCollection<base_ProductModel> ProductCollection
        //{
        //    get { return _productCollection; }
        //    set
        //    {
        //        if (_productCollection != value)
        //        {
        //            _productCollection = value;
        //            OnPropertyChanged(() => ProductCollection);
        //        }
        //    }
        //}
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

        #region BarcodeProduct
        private string _barcodeProduct;
        /// <summary>
        /// Gets or sets the BarcodeProduct.
        /// </summary>
        public string BarcodeProduct
        {
            get { return _barcodeProduct; }
            set
            {
                if (_barcodeProduct != value)
                {
                    _barcodeProduct = value;
                    OnPropertyChanged(() => BarcodeProduct);
                }
            }
        }
        #endregion

        //Employee
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

        //Other Property
        #region SaleTaxCollection
        public List<base_SaleTaxLocationModel> SaleTaxLocationCollection
        {
            get;
            set;
        }
        #endregion

        #region PromotionList
        public List<base_PromotionModel> _promotionList
        {
            get;
            set;
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

        #region RewardManagerCollection
        public List<base_RewardManager> RewardManagerCollection { get; set; }
        #endregion

        #region ExtensionProducts
        private List<base_ProductModel> _extensionProducts = new List<base_ProductModel>();
        /// <summary>
        /// Gets or sets the ExtensionProducts.
        /// </summary>
        public List<base_ProductModel> ExtensionProducts
        {
            get { return _extensionProducts; }
            set
            {
                if (_extensionProducts != value)
                {
                    _extensionProducts = value;
                }
            }
        }
        #endregion

        //Sale Order
        #region SelectedSaleOrder

        protected base_SaleOrderModel _selectedSaleOrder;
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
                    SelectedSaleOrderChanging(value);
                    _selectedSaleOrder = value;
                    OnPropertyChanged(() => SelectedSaleOrder);
                    SelectedSaleOrderChanged();
                }
            }
        }


        #endregion

        #region SelectedSaleOrderDetail
        protected base_SaleOrderDetailModel _selectedSaleOrderDetail;
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
        /// Gets the New Command.
        /// <summary>

        public RelayCommand<object> NewCommand { get; private set; }


        /// <summary>
        /// Method to check whether the New command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected virtual bool OnNewCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the New command is executed.
        /// </summary>
        protected virtual void OnNewCommandExecute(object param)
        {
            RemoveCustomerDeactived();
        }


        #endregion

        #region SaveCommand
        /// <summary>
        /// Gets the Save Command.
        /// <summary>

        public RelayCommand<object> SaveCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Save command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected virtual bool OnSaveCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Save command is executed.
        /// </summary>
        protected virtual void OnSaveCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Gets the Delete Command.
        /// <summary>

        public RelayCommand<object> DeleteCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Delete command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected virtual bool OnDeleteCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Delete command is executed.
        /// </summary>
        protected virtual void OnDeleteCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region SearchCommand
        /// <summary>
        /// Gets the Search Command.
        /// <summary>

        public RelayCommand<object> SearchCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Search command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected virtual bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Search command is executed.
        /// </summary>
        protected virtual void OnSearchCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        //SaleOrderDetail
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
            return (param as base_SaleOrderDetailModel).PickQty == 0 && IsAllowChangeOrder;
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ItemPicked"), Language.POS, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
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
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Is(MessageBoxResult.Yes))
            {
                //Item is product group => remove child item
                if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
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
                    && saleOrderDetailModel.ProductModel != null
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

                _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
            }
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
        protected virtual bool OnSearchProductAdvanceCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SearchProductAdvance command is executed.
        /// </summary>
        protected virtual void OnSearchProductAdvanceCommandExecute(object param)
        {
            SearchProductAdvance();
        }
        #endregion

        #region SearchProductCommand
        /// <summary>
        /// Gets the SearchProduct Command.
        /// <summary>

        public RelayCommand<object> SearchProductCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SearchProduct command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected virtual bool OnSearchProductCommandCanExecute(object param)
        {
            return !string.IsNullOrWhiteSpace(BarcodeProduct);
        }


        /// <summary>
        /// Method to invoke when the SearchProduct command is executed.
        /// </summary>
        protected virtual void OnSearchProductCommandExecute(object param)
        {

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
            return SelectedSaleOrderDetail != null && SelectedSaleOrderDetail.ProductModel != null;
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
            try
            {
                if (SelectedSaleOrderDetail != null && param != null && !Convert.ToDecimal(param).Equals(SelectedSaleOrderDetail.SalePrice))
                {
                    SelectedSaleOrderDetail.IsManual = true;
                    SelectedSaleOrderDetail.PromotionId = 0;
                    SelectedSaleOrderDetail.PromotionName = string.Empty;
                    SelectedSaleOrderDetail.SalePrice = Convert.ToDecimal(param);
                    SelectedSaleOrderDetail.SalePriceChanged(true);

                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, SelectedSaleOrderDetail);
                    BreakSODetailChange = false;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            return IsAllowChangeOrder && (param as base_SaleOrderDetailModel).ProductModel != null;
        }

        /// <summary>
        /// Method to invoke when the EditProduct command is executed.
        /// </summary>
        private void OnEditProductCommandExecute(object param)
        {
            try
            {
                base_SaleOrderDetailModel saleOrderDetailModel = param as base_SaleOrderDetailModel;

                if (saleOrderDetailModel.ProductModel.IsCoupon)
                {
                    //EditCoupon
                    OpenCouponView(saleOrderDetailModel);
                }
                else
                {
                    base_ProductModel productModel = new base_ProductModel();

                    productModel.Resource = saleOrderDetailModel.ProductModel.Resource;
                    productModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>(saleOrderDetailModel.ProductUOMCollection);
                    productModel.BaseUOMId = saleOrderDetailModel.UOMId.Value;
                    productModel.CurrentPrice = saleOrderDetailModel.SalePrice;
                    productModel.RegularPrice = saleOrderDetailModel.RegularPrice;
                    productModel.OnHandStore = saleOrderDetailModel.Quantity;
                    productModel.ProductName = saleOrderDetailModel.ProductModel.ProductName;
                    productModel.Attribute = saleOrderDetailModel.ItemAtribute;
                    productModel.Size = saleOrderDetailModel.ItemSize;
                    productModel.Description = saleOrderDetailModel.ProductModel.Description;
                    productModel.IsOpenItem = saleOrderDetailModel.ProductModel.IsOpenItem;
                    productModel.ProductCategoryId = saleOrderDetailModel.ProductModel.ProductCategoryId;
                    productModel.ItemTypeId = saleOrderDetailModel.ProductModel.ItemTypeId;

                    //Edit Promotion when item is not product in group or product group
                    bool isEditPromotion = true;
                    if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))
                        isEditPromotion = false;
                    else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))
                        isEditPromotion = false;

                    PopupEditProductViewModel viewModel = new PopupEditProductViewModel(productModel, (PriceTypes)SelectedSaleOrder.PriceSchemaId, !saleOrderDetailModel.IsReadOnlyUOM, saleOrderDetailModel.PromotionId.Value, isEditPromotion);

                    bool? result = _dialogService.ShowDialog<PopupEditProductView>(_ownerViewModel, viewModel, Language.GetMsg("SO_Title_EditProduct"));
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
                            if (!saleOrderDetailModel.IsError && saleOrderDetailModel.Quantity > 0 && productModel.OnHandStore != saleOrderDetailModel.Quantity)
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
                                saleOrderDetailModel.DiscountPercent = viewModel.DiscountPercent;
                                saleOrderDetailModel.CalcDicountByPercent();
                                //_saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                                saleOrderDetailModel.PromotionId = 0;
                                saleOrderDetailModel.PromotionName = string.Empty;

                                //CalculateDiscount(saleOrderDetailModel);
                                BreakSODetailChange = true;
                                _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                                BreakSODetailChange = false;
                            }
                            else
                            {
                                saleOrderDetailModel.PromotionId = viewModel.SelectedPromotion.Id;
                                saleOrderDetailModel.PromotionName = viewModel.SelectedPromotion.Name;
                                //CalculateDiscount(saleOrderDetailModel);

                                BreakSODetailChange = true;
                                _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                                BreakSODetailChange = false;
                            }
                        }
                        else
                        {
                            //_saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                        }
                        _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                        CalculateMultiNPriceTax();

                        _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                        _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                        SelectedSaleOrder.CalcSubTotal();
                        BreakSODetailChange = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            return (param as base_SaleOrderDetailModel).ProductModel != null;
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
            OpenNewPopupForm();
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

            string strTitle = addressModel.AddressTypeId.Is(AddressType.Billing) ? Language.GetMsg("SO_Title_BillAddress") : Language.GetMsg("SO_Title_ShippingAddress");
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
            PaymentTermViewModel paymentTermViewModel = new PaymentTermViewModel(SelectedSaleOrder.IsCOD, dueDays, discount, discountDays);
            bool? dialogResult = _dialogService.ShowDialog<PaymentTermView>(_ownerViewModel, paymentTermViewModel, Language.GetMsg("SO_Title_AddTerm"));
            if (dialogResult == true)
            {
                SelectedSaleOrder.TermNetDue = paymentTermViewModel.DueDays;
                SelectedSaleOrder.TermDiscountPercent = paymentTermViewModel.Discount;
                SelectedSaleOrder.TermPaidWithinDay = paymentTermViewModel.DiscountDays;
                SelectedSaleOrder.PaymentTermDescription = paymentTermViewModel.Description;
                SelectedSaleOrder.IsCOD = paymentTermViewModel.IsCOD;
            }
        }
        #endregion

        #region CustomerSearch
        /// <summary>
        /// Gets the CustomerSearch Command.
        /// <summary>

        public RelayCommand<object> CustomerSearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CustomerSearch command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCustomerSearchCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CustomerSearch command is executed.
        /// </summary>
        private void OnCustomerSearchCommandExecute(object param)
        {
            ////--------------------Confirm Reward Membership-------------------
            //MemberShipValidationViewModel memberShipValidationViewModel = new MemberShipValidationViewModel();
            //var memberShipValidated = _dialogService.ShowDialog<MemberShipValidationView>(_ownerViewModel, memberShipValidationViewModel, "Membership validation");
            //if (memberShipValidated == true && memberShipValidationViewModel.CustomerModel != null)
            //{
            //    SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(memberShipValidationViewModel.CustomerModel.Resource));
            //}
            CustomerSearchViewModel viewModel = new CustomerSearchViewModel(CustomerCollection, this);
            var result = _dialogService.ShowDialog<CustomerSearchView>(_ownerViewModel, viewModel, Language.GetMsg("SO_Title_CustomerSearch"));
            if (result == true)
            {
                switch (viewModel.CurrentViewAction)
                {
                    case CustomerSearchViewModel.ActionView.Cancel:
                        break;
                    case CustomerSearchViewModel.ActionView.SelectedItem:
                        SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(viewModel.SelectedCustomer.Resource));
                        break;
                    case CustomerSearchViewModel.ActionView.NewCustomer:
                        OpenNewPopupForm();
                        break;
                    case CustomerSearchViewModel.ActionView.GoToList:
                        (_ownerViewModel as MainViewModel).OpenViewExecute("CustomerList");
                        break;
                    default:
                        break;
                }
            }


        }
        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Initial Command 
        ///<para>NewCommand</para>
        /// <para>SaveCommand</para>
        /// <para>DeleteCommand</para>
        /// <para>SearchCommand</para>
        /// <para>QtyChangedCommand</para>
        /// <para>SearchProductAdvanceCommand</para>
        /// <para>AddressPopupCommand</para>
        /// <para>ManualChangePriceCommand</para>
        /// <para>EditProductCommand</para>
        /// <para>AddNewCustomerCommand</para>
        /// <para>SerialTrackingDetailCommand</para>
        /// <para>AddTermCommand</para>
        /// <para>DeleteSaleOrderDetailCommand</para>
        /// <para>DeleteSaleOrderDetailWithKeyCommand</para>
        /// </summary>
        protected virtual void InitialCommand()
        {
            NewCommand = new RelayCommand<object>(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);

            QtyChangedCommand = new RelayCommand<object>(OnQtyChangedCommandExecute, OnQtyChangedCommandCanExecute);
            SearchProductAdvanceCommand = new RelayCommand<object>(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
            SearchProductCommand = new RelayCommand<object>(OnSearchProductCommandExecute, OnSearchProductCommandCanExecute);
            AddressPopupCommand = new RelayCommand<object>(OnAddressPopupCommandExecute, OnAddressPopupCommandCanExecute);
            ManualChangePriceCommand = new RelayCommand<object>(OnManualChangePriceCommandExecute, OnManualChangePriceCommandCanExecute);
            EditProductCommand = new RelayCommand<object>(OnEditProductCommandExecute, OnEditProductCommandCanExecute);
            SerialTrackingDetailCommand = new RelayCommand<object>(OnSerialTrackingDetailCommandExecute, OnSerialTrackingDetailCommandCanExecute);
            AddNewCustomerCommand = new RelayCommand<object>(OnAddNewCustomerCommandExecute, OnAddNewCustomerCommandCanExecute);
            AddTermCommand = new RelayCommand(OnAddTermCommandExecute, OnAddTermCommandCanExecute);

            DeleteSaleOrderDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
            DeleteSaleOrderDetailWithKeyCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailWithKeyCommandExecute, OnDeleteSaleOrderDetailWithKeyCommandCanExecute);

            CustomerSearchCommand = new RelayCommand<object>(OnCustomerSearchCommandExecute, OnCustomerSearchCommandCanExecute);
        }

        //LoadData 
        /// <summary>
        /// Data can change
        /// Load Dynamic data when initial form or change view from other
        /// <para>LoadDiscountProgram</para>
        /// <para>LoadProducts</para>
        /// <para>LoadCustomer</para>
        /// <para>LoadEmployee</para>
        /// <para>LoadSaleTax</para>
        /// </summary>
        protected virtual void LoadDynamicData()
        {
            //Load DiscountCollection with program
            LoadDiscountProgram();

            //Load Customer
            LoadCustomer();

            //Get Employee
            LoadEmployee();

            //Load All Sale Tax
            LoadSaleTax();

            //Load All Reward Manager
            LoadRewardManagers();

        }

        /// <summary>
        /// Load Static Data
        /// </summary>
        protected virtual void LoadStaticData()
        {
            this.BillAddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            BillAddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = Language.GetMsg("SO_TextBlock_Billing") });

            ShipAddressTypeCollection = new AddressTypeCollection();
            ShipAddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = Language.GetMsg("SO_TextBlock_Shipping") });

            //Create Collection for filter customer with autocomplete
            CustomerFieldCollection = new DataSearchCollection() { 
                new DataSearchModel { ID = 1, Level = 0, DisplayName = Language.GetMsg("CUS_Text_CustomerNumber"), KeyName = "GuestNo" },
                new DataSearchModel { ID = 2, Level = 0, DisplayName = Language.GetMsg("CUS_Text_Name"), KeyName = "LegalName" }
            };

            //Create collection for search products
            ProductFieldCollection = new DataSearchCollection() { 
                new DataSearchModel { ID = 1, Level = 0, DisplayName = Language.GetMsg("C174"), KeyName = "Code" },
                new DataSearchModel { ID = 2, Level = 0, DisplayName = Language.GetMsg("C176"), KeyName = "Barcode" },
                new DataSearchModel { ID = 3, Level = 0, DisplayName = Language.GetMsg("C175"), KeyName = "ProductName" },
                new DataSearchModel { ID = 4, Level = 0, DisplayName = Language.GetMsg("C116"), KeyName = "Attribute" },
                new DataSearchModel { ID = 6, Level = 0, DisplayName = Language.GetMsg("C117"), KeyName = "Size" }
              };


        }

        /// <summary>
        /// Load All Customer From DB
        /// </summary>
        protected virtual void LoadCustomer()
        {
            lock (UnitOfWork.Locker)
            {
                IList<base_Guest> customerList = _guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && !x.IsPurged);//&& x.IsActived

                if (CustomerCollection == null)
                    CustomerCollection = new CollectionBase<base_GuestModel>();//customerList.OrderBy(x => x.Id).Select(x => new base_GuestModel(x))

                CustomerCollection.DeletedItems.Clear();
                //else
                //{
                foreach (base_Guest customer in customerList)
                {
                    if (customer.IsActived)
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
                    else
                    {
                        //Remove item has in customer collection is deActived
                        if (CustomerCollection.Any(x => x.Resource.Equals(customer.Resource)))
                        {
                            base_GuestModel itemRemoved = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(customer.Resource));
                            CustomerCollection.Remove(itemRemoved);
                        }
                        else
                        {
                            CustomerCollection.DeletedItems.Add(new base_GuestModel(customer));
                        }
                    }
                }
                //Remove Item From Local collection if in db collection is not existed
                IList<Guid?> itemReomoveList = CustomerCollection.Select(x => x.Resource).Except(customerList.Where(x => x.IsActived).Select(x => x.Resource)).ToList();
                if (itemReomoveList != null)
                {
                    foreach (Guid resource in itemReomoveList)
                    {
                        base_GuestModel itemRemoved = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(resource));
                        CustomerCollection.Remove(itemRemoved);
                        CustomerCollection.DeletedItems.Remove(itemRemoved);
                    }
                }
                //}
            }
        }

        /// <summary>
        /// Load Reward Management
        /// </summary>
        protected virtual void LoadRewardManagers()
        {
            if (RewardManagerCollection == null || RewardManagerCollection.Any())
            {
                IList<base_RewardManager> rewards = _rewardManagerRepository.GetAll();
                if (RewardManagerCollection == null)
                {
                    RewardManagerCollection = new List<base_RewardManager>(rewards);
                }
                else
                {
                    foreach (base_RewardManager reward in rewards)
                    {
                        _rewardManagerRepository.Refresh(reward);
                        if (RewardManagerCollection.Any(x => x.Id.Equals(reward.Id)))
                        {
                            base_RewardManager rewardUpdated = RewardManagerCollection.SingleOrDefault(x => x.Id.Equals(reward.Id));
                            rewardUpdated = reward;
                        }
                        else
                        {
                            RewardManagerCollection.Add(reward);
                        }

                    }
                }

            }
        }

        /// <summary>
        /// Load All Employee From Db 
        /// </summary>
        protected void LoadEmployee()
        {
            IList<base_Guest> employeeList = _guestRepository.GetAll(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived);

            if (EmployeeCollection == null)
                EmployeeCollection = new CollectionBase<base_GuestModel>(employeeList.OrderBy(x => x.Id).Select(x => new base_GuestModel(x)));
            else
            {
                foreach (base_Guest employee in employeeList)
                {
                    _guestRepository.Refresh(employee);

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
        /// Load SaleTaxCollection
        /// </summary>
        protected void LoadSaleTax()
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
        /// 
        /// </summary>
        protected void LoadDiscountProgram()
        {
            short promotionStatus = (short)StatusBasic.Active;
            IEnumerable<base_Promotion> promotionList = _promotionRepository.GetAll(x => x.Status.Equals(promotionStatus));
            if (_promotionList == null || !_promotionList.Any())
            {
                _promotionList = new List<base_PromotionModel>(promotionList.OrderByDescending(x => x.DateUpdated).Select(x => new base_PromotionModel(x)));
            }
            else
            {
                foreach (base_Promotion promotion in promotionList)
                {
                    if (_promotionList.Any(x => x.Id.Equals(promotion.Id)))
                    {
                        base_PromotionModel promotionModel = _promotionList.SingleOrDefault(x => x.Id.Equals(promotion.Id));
                        _promotionRepository.Refresh(promotionModel.base_Promotion);
                        promotionModel.ToModel();
                        promotionModel.EndUpdate();
                    }
                    else
                    {
                        _promotionRepository.Refresh(promotion);
                        _promotionList.Add(new base_PromotionModel(promotion));
                    }
                }
            }
        }

        /// <summary>
        /// Load Extension product for search
        /// <para>Gift Card</para>
        /// </summary>
        protected void LoadProductExtension()
        {
            if (!ExtensionProducts.Any(x => x.IsCoupon))
            {
                string strGuidPatern = "{0}{0}{0}{0}{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                string strCodePatern = "{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                string strBarCodePatern = "{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";

                Guid giftCardGuid = Guid.Parse(string.Format(strGuidPatern, 1));
                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(PaymentMethod.GiftCard) && !ExtensionProducts.Any(x => x.Resource.Equals(giftCardGuid)))
                {
                    base_ProductModel couponItem = new base_ProductModel();
                    ComboItem card = Common.PaymentMethods.FirstOrDefault(x => x.Value.Equals((short)PaymentMethod.GiftCard));
                    couponItem.ProductName = card.Text;
                    //Using for CardTypeId
                    couponItem.ProductCategoryId = card.Value;
                    couponItem.Code = string.Format(strCodePatern, 1);//64 : PaymentMethod.GiftCard
                    couponItem.Barcode = string.Format(strBarCodePatern, 1);
                    couponItem.Resource = giftCardGuid;
                    couponItem.IsOpenItem = true;
                    couponItem.IsCoupon = true;
                    ExtensionProducts.Add(couponItem);
                }

                Guid giftCertificateGuid = Guid.Parse(string.Format(strGuidPatern, 2));
                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(PaymentMethod.GiftCertificate) && !ExtensionProducts.Any(x => x.Resource.Equals(giftCertificateGuid)))
                {
                    base_ProductModel couponItem = new base_ProductModel();
                    ComboItem card = Common.PaymentMethods.FirstOrDefault(x => x.Value.Equals((short)PaymentMethod.GiftCertificate));
                    //Using for CardTypeId
                    couponItem.ProductCategoryId = card.Value;
                    couponItem.ProductName = card.Text;
                    couponItem.Code = string.Format(strCodePatern, 2);//128 : PaymentMethod.GiftCertificate
                    couponItem.Barcode = string.Format(strBarCodePatern, 2);
                    couponItem.Resource = giftCertificateGuid;
                    couponItem.IsOpenItem = true;
                    couponItem.IsCoupon = true;
                    ExtensionProducts.Add(couponItem);
                }

            }

        }

        //Set Value
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected virtual void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                BreakAllChange = true;

                //Set SaleOrderStatus
                saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                //Set Price Schema
                saleOrderModel.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.PriceSchemaId));

                //Get CustomerModel & relation with customer

                base_GuestModel customerModel = CustomerCollection.FirstOrDefault(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource));
                if (customerModel == null)//this customer could is deactived
                    customerModel = CustomerCollection.DeletedItems.FirstOrDefault(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource));

                saleOrderModel.GuestModel = customerModel;

                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    DateTime orderDate = saleOrderModel.OrderDate.Value.Date;

                    var reward = GetReward(orderDate);
                    saleOrderModel.IsApplyReward = saleOrderModel.GuestModel.IsRewardMember;
                    saleOrderModel.IsApplyReward &= reward != null ? true : false;

                    if (saleOrderModel.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                        saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

                    saleOrderModel.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                }


                //Get SaleTax
                GetSaleTax(saleOrderModel);


                //Check Deposit is accepted?
                saleOrderModel.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit

                saleOrderModel.RaiseAnyShipped();
                saleOrderModel.SetFullPayment();

                BreakAllChange = false;
                saleOrderModel.IsDirty = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load Sale Order Detail Collection with SaleOrderDetailCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce">Can set SaleOrderDetailCollection when difference null</param>
        protected virtual void SetSaleOrderDetail(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
            {
                //Load sale order detail
                if (isForce || saleOrderModel.SaleOrderDetailCollection == null || !saleOrderModel.SaleOrderDetailCollection.Any())
                {
                    saleOrderModel.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
                    saleOrderModel.SaleOrderDetailCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(SaleOrderDetailCollection_CollectionChanged);

                    foreach (base_SaleOrderDetail saleOrderDetail in saleOrderModel.base_SaleOrder.base_SaleOrderDetail.OrderBy(x => x.Id))
                    {
                        _saleOrderDetailRepository.Refresh(saleOrderDetail);
                        base_SaleOrderDetailModel saleOrderDetailModel = new base_SaleOrderDetailModel(saleOrderDetail);
                        saleOrderDetailModel.Qty = saleOrderDetailModel.Quantity;
                        //Get Product
                        base_Product product = _productRepository.GetProductByResource(saleOrderDetail.ProductResource);

                        if (product != null)
                        {
                            saleOrderDetailModel.ProductModel = new base_ProductModel(product);
                        }
                        else
                        {
                            base_ProductModel productModel = ExtensionProducts.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetail.ProductResource));
                            if (productModel != null)
                                saleOrderDetailModel.ProductModel = productModel;
                            else
                                saleOrderDetailModel.ProductModel = null;
                        }

                        if (saleOrderDetailModel.ProductModel != null)
                        {
                            if (saleOrderDetailModel.ProductModel.IsCoupon)
                            {
                                saleOrderDetailModel.IsQuantityAccepted = true;
                                saleOrderDetailModel.IsReadOnlyUOM = true;
                                //Get Coupon From CardManagement
                                base_CardManagement cardManager = _cardManagementRepository.Get(x => !x.IsPurged && x.CardNumber.Equals(saleOrderDetailModel.SerialTracking));
                                if (cardManager != null)
                                    saleOrderDetailModel.CouponCardModel = new base_CardManagementModel(cardManager);
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
                        }
                        else
                        {
                            //Lock row => product is not figure out
                            saleOrderDetailModel.IsAllowChange = false;
                        }
                        saleOrderModel.SaleOrderDetailCollection.Add(saleOrderDetailModel);

                        _saleOrderRepository.CalcOnHandStore(saleOrderModel, saleOrderDetailModel);

                    }
                    if (saleOrderModel.SaleOrderDetailCollection != null)
                        saleOrderModel.IsHiddenErrorColumn = !saleOrderModel.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);


                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Set whatever relate with saleOrder
        /// <para>
        /// Set SetGuestAdditionalModel,SetBillShipAddress,SetMemberShipValidated,SetSaleOrderDetail
        /// </para>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        protected virtual void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
            {
                _saleOrderRepository.SetGuestAdditionalModel(saleOrderModel);

                //Set Address
                _saleOrderRepository.SetBillShipAddress(saleOrderModel.GuestModel, saleOrderModel);

                SetMemberShipValidated(saleOrderModel);

                //Using for Calc Tax for Shipping if Tax is Price
                CalcProductNShipTaxAmount(saleOrderModel);

                //Set SaleOrderDetail To SaleOrder
                SetSaleOrderDetail(saleOrderModel, isForce);

                //load payment & payment detail
                LoadPaymentCollection(saleOrderModel, isForce);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Set MemberShip For SaleOrder Guest
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected virtual void SetMemberShipValidated(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.GuestModel != null && saleOrderModel.GuestModel.IsRewardMember)
            {
                //Get MemberShip
                short memebershipActivedStatus = (short)MemberShipStatus.Actived;
                base_MemberShip membership = saleOrderModel.GuestModel.base_Guest.base_MemberShip.FirstOrDefault(x => x.Status.Equals(memebershipActivedStatus));
                if (membership != null)
                    saleOrderModel.GuestModel.MembershipValidated = new base_MemberShipModel(membership);
                else
                    saleOrderModel.GuestModel.MembershipValidated = NewMemberShipModel(saleOrderModel.GuestModel);
            }
        }

        /// <summary>
        /// Load payment collection 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected virtual void LoadPaymentCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            // Get document resource
            string docResource = saleOrderModel.Resource.ToString();
            if (saleOrderModel.PaymentCollection == null || !saleOrderModel.PaymentCollection.Any() || isForce)
            {
                // Get all payment by document resource
                IEnumerable<base_ResourcePayment> payments = _paymentRepository.GetAll(x => x.DocumentResource.Equals(docResource));

                saleOrderModel.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();
                base_ResourcePaymentModel paymentModel;
                // Load payment collection
                foreach (base_ResourcePayment resourcePayment in payments)
                {
                    paymentModel = new base_ResourcePaymentModel(resourcePayment);
                    //load Resource Payment Detail
                    paymentModel.PaymentDetailCollection = new CollectionBase<base_ResourcePaymentDetailModel>(resourcePayment.base_ResourcePaymentDetail.Select(x => new base_ResourcePaymentDetailModel(x)));
                    saleOrderModel.PaymentCollection.Add(paymentModel);
                }

            }
            // Check show PaymentTab
            saleOrderModel.PaymentProcess = saleOrderModel.PaymentCollection == null ? false : saleOrderModel.PaymentCollection.Any();
        }

        //Create New
        /// <summary>
        /// 
        /// </summary>
        protected virtual base_SaleOrderModel CreateNewSaleOrder()
        {
            try
            {
                _selectedSaleOrder = new base_SaleOrderModel();
                _selectedSaleOrder.Shift = Define.ShiftCode;
                _selectedSaleOrder.IsTaxExemption = false;
                _selectedSaleOrder.IsConverted = false;
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

                _selectedSaleOrder.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing, IsDirty = false };
                _selectedSaleOrder.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping, IsDirty = false };

                //Get TaxLocation
                _selectedSaleOrder.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == _selectedSaleOrder.TaxLocation);

                //Create a sale order detail collection
                _selectedSaleOrder.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();


                // Create new payment collection
                _selectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                //Call from 
                CreateExtentSaleOrder();

                _selectedSaleOrder.IsDirty = false;
                OnPropertyChanged(() => SelectedSaleOrder);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return _selectedSaleOrder;
        }

        #region SelectedItem Changed

        /// <summary>
        /// Selected SaleOrder Changing
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SelectedSaleOrderChanging(base_SaleOrderModel value) { }

        /// <summary>
        /// Selected dSaleOrder Changed
        /// </summary>
        protected virtual void SelectedSaleOrderChanged() { }

        /// <summary>
        /// Selected Customer Changing
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SelectedCustomerChanging(base_GuestModel value)
        {

        }

        /// <summary>
        /// Selected Customer of autocomplete changed
        /// <param name="setRelation">set value using for Change customer</param>
        /// </summary>
        protected virtual void SelectedCustomerChanged()
        {
            //Don't set SaleOrder relation
            if (!_customerSetRelation || SelectedCustomer == null)
                return;

            SelectedSaleOrder.CustomerResource = SelectedCustomer.Resource.ToString();
            SelectedSaleOrder.GuestModel = CustomerCollection.Where(x => x.Resource.Equals(SelectedCustomer.Resource)).FirstOrDefault();

            //Get Reward & set PurchaseThreshold if Customer any reward
            var reward = GetReward(SelectedSaleOrder.OrderDate.Value.Date);

            SelectedSaleOrder.IsApplyReward = true;
            SelectedSaleOrder.IsApplyReward &= reward != null ? true : false;
            //isReward Member
            if (SelectedSaleOrder.GuestModel.IsRewardMember)
            {
                //Get GuestReward collection
                SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();

                if (SelectedSaleOrder.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                    SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

                //Get member ship
                SetMemberShipValidated(SelectedSaleOrder);
            }
            else
            {
                if (reward != null && reward.IsPromptEnroll && !SelectedSaleOrder.GuestModel.IsRewardMember)
                {
                    //"This customer is not currently a member of reward program. Do you want to enroll this one?"
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_EnrollRewardMember"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        SelectedSaleOrder.GuestModel.IsRewardMember = true;
                        SelectedCustomer.IsRewardMember = true;
                        SelectedSaleOrder.GuestModel.MembershipValidated = NewMemberShipModel(SelectedSaleOrder.GuestModel);
                        SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                        if (SelectedSaleOrder.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                            SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;
                    }
                }
            }
            //Set To Show Reward Icon
            SelectedSaleOrder.IsApplyReward &= SelectedSaleOrder.GuestModel.IsRewardMember;


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

            //SetTaxLocation

            SetSaleTaxLocationForSaleOrder(SelectedSaleOrder);

            //set Customer option in additional of markdownprice Level
            _saleOrderRepository.SetGuestAdditionalModel(SelectedSaleOrder);

            if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.MarkdownPriceLevel))
                SelectedSaleOrder.PriceSchemaId = SelectedSaleOrder.GuestModel.AdditionalModel.PriceSchemeId.Value;


            if (SelectedSaleOrder.SaleOrderDetailCollection.Any())
            {
                //Confirm if existed customer option discount  PriceLevelType.MarkdownPriceLevel 
                CalcDiscountAllItemWithCustomerAdditional();
            }
        }

        /// <summary>
        /// Selected Product changed
        /// </summary>
        protected virtual void SelectedProductChanged()
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
        #endregion

        #region Get Data

        /// <summary>
        /// Get Reward From Reward Collection
        /// </summary>
        /// <param name="orderDate">
        /// order Date !=null : which reward is Active & OrderDate>= Start date & OrderDate <= EndDate
        /// OrderDate =null : Get reward without conditional
        /// </param>
        /// <returns></returns>
        protected base_RewardManager GetReward(DateTime? orderDate = null)
        {
            base_RewardManager rewardMangers;
            try
            {
                if (orderDate.HasValue)
                {
                    rewardMangers = RewardManagerCollection.SingleOrDefault(x => x.Status.Is(StatusBasic.Active) &&
                                                   ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                                  || !x.IsTrackingPeriod)));

                }
                else
                {
                    rewardMangers = RewardManagerCollection.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }

            return rewardMangers;
        }
        /// <summary>
        /// Load Tax for SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void GetSaleTax(base_SaleOrderModel saleOrderModel)
        {
            saleOrderModel.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == saleOrderModel.TaxLocation);
            if (saleOrderModel.TaxLocationModel != null)
            {
                //Get Tax Code
                saleOrderModel.TaxLocationModel.TaxCodeModel = SaleTaxLocationCollection.SingleOrDefault(x => x.ParentId == saleOrderModel.TaxLocationModel.Id && x.TaxCode.Equals(saleOrderModel.TaxCode));
                if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                    saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(saleOrderModel.TaxLocationModel.TaxCodeModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)));
            }
        }
        #endregion

        #region Calculate Tax

        /// <summary>
        /// Calculate Price Dependent Tax
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="subTotal"></param>
        /// <param name="taxPercent"></param>
        /// <param name="taxAmount"></param>
        protected decimal CalcPriceDependentTax(base_SaleOrderModel saleOrderModel)
        {
            decimal taxAmount = 0;
            if (saleOrderModel.IsTaxExemption == true)
            {
                taxAmount = 0;
            }
            else if (saleOrderModel.TaxLocationModel != null && Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
            {
                base_SaleTaxLocationOptionModel saleTaxLocationOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    //if (!saleOrderDetailModel.ProductModel.IsCoupon)//8/11/2013 Calculate tax for Coupon
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
        protected decimal CalcMultiTaxForProduct(base_SaleOrderModel saleOrderModel)
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
                    //if (!saleOrderDetailModel.ProductModel.IsCoupon)//18/06/2013: not calculate tax for coupon
                    //8/11/2013 Calculate tax for Coupon
                    taxAmount += _saleOrderRepository.CalcMultiTaxForItem(saleOrderModel.TaxLocationModel.SaleTaxLocationOptionCollection, saleOrderDetailModel.SubTotal, saleOrderDetailModel.SalePrice);
                }
            }
            return taxAmount;
        }

        /// <summary>
        /// Calculate multi, price dependent tax when sale price changed
        /// </summary>
        protected void CalculateMultiNPriceTax()
        {
            if (SelectedSaleOrder.TaxLocationModel != null && SelectedSaleOrder.TaxLocationModel.TaxCodeModel != null)
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
        protected void CalculateAllTax(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null)
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
                        saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
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
        /// Calc Ship Tax with PriceDepent
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected decimal CalcShipTaxAmount(CPC.POS.Model.base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null
                      && Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price)
                       && saleOrderModel.TaxLocationModel.IsShipingTaxable)
                {
                    CPC.POS.Model.base_SaleTaxLocationModel shippingTaxCode = SaleTaxLocationCollection.SingleOrDefault(x => x.Id.Equals(saleOrderModel.TaxLocationModel.ShippingTaxCodeId));
                    if (shippingTaxCode != null
                        && Convert.ToInt32(shippingTaxCode.TaxOption).Is(SalesTaxOption.Price)
                        && shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault() != null
                        && shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault().IsApplyAmountOver)
                    {
                        CPC.POS.Model.base_SaleTaxLocationOptionModel taxOptionModel = new CPC.POS.Model.base_SaleTaxLocationOptionModel(shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault());
                        return _saleOrderRepository.CalcPriceDependentItem(saleOrderModel.Shipping, saleOrderModel.Shipping, taxOptionModel);
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
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
            try
            {
                //Not calculate discount for layaway
                if (!SelectedSaleOrder.Mark.Equals(CPC.POS.MarkType.Layaway) && SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any())
                {
                    if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.FixedDiscountOnAllItems))
                    {
                        //"Do you want to apply customer discount?"
                        MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ApplyCustomerDiscount"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Calculate All item with pos program discount
        /// </summary>
        private void CalcDiscountAllItemWithPOSProgram()
        {
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
            {
                CalcProductDiscountWithProgram(SelectedSaleOrder, saleOrderDetailModel);
            }
        }

        /// <summary>
        /// Calculate discount with GuestAddtion or program
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        protected void CalculateDiscount(base_SaleOrderDetailModel salesOrderDetailModel)
        {
            try
            {
                //Not calculate Discount for layaway
                if (SelectedSaleOrder.Mark.Equals(CPC.POS.MarkType.Layaway.ToDescription()))
                    return;
                if (salesOrderDetailModel.IsManual)
                    salesOrderDetailModel.CalcDicountByPercent();
                else if (SelectedSaleOrder.GuestModel != null &&
                   SelectedSaleOrder.GuestModel.AdditionalModel != null
                   && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((int)PriceLevelType.FixedDiscountOnAllItems))
                    CalcDiscountWithAdditional(salesOrderDetailModel);
                else
                    CalcProductDiscountWithProgram(SelectedSaleOrder, salesOrderDetailModel);

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Calculate discount with Customer additional 
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        protected void CalcDiscountWithAdditional(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            try
            {
                if ((saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group) || !string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)) && saleOrderDetailModel.RegularPrice < saleOrderDetailModel.SalePrice)
                {
                    saleOrderDetailModel.PromotionId = 0;
                    saleOrderDetailModel.PromotionName = string.Empty;
                    return;
                }
                saleOrderDetailModel.IsManual = true;
                saleOrderDetailModel.PromotionId = 0;
                if (Convert.ToInt32(SelectedSaleOrder.GuestModel.AdditionalModel.Unit).Equals(1))//$
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Calculate discount for product
        /// auto choice promotion program for current SaleOrderDetailItem
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CalcProductDiscountWithProgram(base_SaleOrderModel saleOrderModel, base_SaleOrderDetailModel saleOrderDetailModel, bool resetDiscPercent = false)
        {
            try
            {
                //reset Discount percent Or calculate new Price when change UOM
                if (resetDiscPercent)
                {
                    _saleOrderRepository.ResetProductDiscount(saleOrderModel, saleOrderDetailModel);
                }

                base_PromotionModel promotionModel = GetPromotionForSaleOrderDetail(saleOrderModel, saleOrderDetailModel);

                if (promotionModel != null && !saleOrderDetailModel.IsManual)
                {
                    saleOrderDetailModel.PromotionId = promotionModel.Id;
                    saleOrderDetailModel.PromotionName = promotionModel.Name;

                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(saleOrderModel, saleOrderDetailModel);
                    BreakSODetailChange = false;
                }
                else
                {
                    if (saleOrderDetailModel.IsManual)
                    {
                        BreakSODetailChange = true;
                        _saleOrderRepository.CalcProductDiscount(saleOrderModel, saleOrderDetailModel);
                        BreakSODetailChange = false;
                    }
                    else
                    {
                        saleOrderDetailModel.PromotionId = 0;
                        saleOrderDetailModel.PromotionName = string.Empty;

                        _saleOrderRepository.ResetProductDiscount(saleOrderModel, saleOrderDetailModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }

        }

        /// <summary>
        /// Get Promotion for sale order detail
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        /// <returns></returns>
        protected base_PromotionModel GetPromotionForSaleOrderDetail(base_SaleOrderModel saleOrderModel, base_SaleOrderDetailModel saleOrderDetailModel)
        {
            base_PromotionModel promotionModel;
            try
            {
                //Not calculate for Parent & child in product group or manual
                if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)
                    || saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group)
                    || saleOrderDetailModel.IsManual)
                {
                    return null;
                }

                int productCategoryId = saleOrderDetailModel.ProductModel.ProductCategoryId;
                //Check condition : product in saleOrder has in Promotion Category 
                //                  OrderDate in Start & Endate if existed 
                promotionModel = _promotionList.FirstOrDefault(x => x.CategoryId.HasValue && x.CategoryId.Value.Equals(productCategoryId)
                                    && Convert.ToInt32(saleOrderModel.PriceSchemaId).In(x.PriceSchemaRange.Value)
                                    && (!x.StartDate.HasValue || (x.StartDate.HasValue && x.StartDate.Value <= saleOrderModel.OrderDate))
                                    && (!x.EndDate.HasValue || (x.EndDate.HasValue && x.EndDate.Value >= saleOrderModel.OrderDate)));

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return promotionModel;
        }
        #endregion

        #region Commission
        /// <summary>
        /// calculate commission for employee
        /// <para>Need Payment Full</para>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void SaveSaleCommission(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.CommissionCollection == null)
                    saleOrderModel.CommissionCollection = new CollectionBase<base_SaleCommissionModel>();
                if (!Define.CONFIGURATION.IsAllwayCommision)
                {
                    ComboItem item = Common.BookingChannel.SingleOrDefault(x => x.Value == SelectedSaleOrder.BookingChanel);
                    if (item.Flag)//True : this booking channel dont use commission
                        return;
                }

                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection.Where(x => x.ProductModel != null && x.ProductModel.IsAllowCommission))
                {
                    decimal qtyReturn = 0;
                    if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.IsAllowCommission && saleOrderDetailModel.ProductModel.ComissionPercent > 0)
                    {
                        //Get Quantity Item is Return
                        if (saleOrderModel.ReturnModel != null && saleOrderModel.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned))
                            qtyReturn = saleOrderModel.ReturnModel.ReturnDetailCollection.Where(x => x.IsReturned && x.OrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.ReturnQty);

                        //Calculate Commision for Employee
                        Guid customerGuid = Guid.Parse(saleOrderModel.CustomerResource);
                        //Get Customer with CustomerResourceC
                        base_GuestModel customerModel = CustomerCollection.SingleOrDefault(x => x.Resource == customerGuid);
                        if (customerModel != null && customerModel.SaleRepId.HasValue)
                        {
                            base_GuestModel employeeModel = EmployeeCollection.SingleOrDefault(x => x.Id == customerModel.SaleRepId);
                            if (employeeModel != null)
                            {
                                base_SaleCommissionModel employeeCommission = new base_SaleCommissionModel();
                                employeeCommission.Remark = MarkType.SaleOrder.ToDescription();
                                employeeCommission.GuestResource = employeeModel.Resource.ToString();
                                employeeCommission.Sign = "+";
                                employeeCommission.Mark = "E";
                                employeeCommission.SOResource = saleOrderModel.Resource.ToString();
                                employeeCommission.SONumber = saleOrderModel.SONumber;
                                employeeCommission.SOTotal = saleOrderModel.RewardAmount;
                                employeeCommission.SODate = saleOrderModel.OrderDate;
                                employeeCommission.SaleOrderDetailResource = saleOrderDetailModel.Resource.ToString();
                                employeeCommission.ProductResource = saleOrderDetailModel.ProductModel.Resource.ToString();
                                employeeCommission.Attribute = saleOrderDetailModel.ProductModel.Attribute;
                                employeeCommission.RegularPrice = saleOrderDetailModel.RegularPrice;
                                employeeCommission.Price = saleOrderDetailModel.SalePrice;
                                employeeCommission.Size = saleOrderDetailModel.ProductModel.Size;
                                employeeCommission.Quanity = saleOrderDetailModel.Quantity - qtyReturn;
                                employeeCommission.Amount = employeeCommission.Price * employeeCommission.Quanity; //saleOrderDetailModel.SubTotal;
                                employeeCommission.ComissionPercent = employeeModel.CommissionPercent;


                                if (saleOrderDetailModel.ProductModel.CommissionUnit == 1) //$
                                {
                                    employeeCommission.CommissionAmount = saleOrderDetailModel.ProductModel.ComissionPercent * employeeCommission.Quanity;
                                }
                                else
                                {
                                    decimal comissionOfProduct = (saleOrderDetailModel.ProductModel.ComissionPercent * employeeCommission.Amount.Value) / 100;
                                    employeeCommission.CommissionAmount = (comissionOfProduct * employeeCommission.ComissionPercent) / 100;
                                }

                                saleOrderModel.CommissionCollection.Add(employeeCommission);

                                //Has Manager
                                if (!string.IsNullOrWhiteSpace(employeeModel.ManagerResource))
                                {
                                    //Calculate Commission for Manager
                                    base_GuestModel managerModel = EmployeeCollection.SingleOrDefault(x => x.Resource.ToString().Equals(employeeModel.ManagerResource));
                                    if (managerModel != null)
                                    {
                                        base_SaleCommissionModel managerCommission = new base_SaleCommissionModel();
                                        managerCommission.Remark = MarkType.SaleOrder.ToDescription();
                                        managerCommission.GuestResource = managerModel.Resource.ToString();
                                        managerCommission.SOResource = saleOrderModel.Resource.ToString();
                                        managerCommission.SONumber = saleOrderModel.SONumber;
                                        managerCommission.SOTotal = saleOrderModel.RewardAmount;
                                        managerCommission.SODate = saleOrderModel.OrderDate;
                                        managerCommission.SaleOrderDetailResource = saleOrderDetailModel.Resource.ToString();
                                        managerCommission.ProductResource = saleOrderDetailModel.ProductModel.Resource.ToString();
                                        managerCommission.Attribute = saleOrderDetailModel.ProductModel.Attribute;
                                        managerCommission.Size = saleOrderDetailModel.ProductModel.Size;
                                        managerCommission.Attribute = saleOrderDetailModel.ProductModel.Attribute;
                                        managerCommission.RegularPrice = saleOrderDetailModel.RegularPrice;
                                        managerCommission.Price = saleOrderDetailModel.SalePrice;
                                        managerCommission.Quanity = saleOrderDetailModel.Quantity - qtyReturn;//subtract item is return in saleOrderDetail
                                        managerCommission.Amount = managerCommission.Quanity * managerCommission.Price; //saleOrderDetailModel.SubTotal;
                                        managerCommission.ComissionPercent = managerModel.CommissionPercent;
                                        managerCommission.Sign = "+";
                                        managerCommission.Mark = "M";

                                        //calculate commission manager from percent of commission employee
                                        managerCommission.CommissionAmount = (employeeCommission.CommissionAmount * managerCommission.ComissionPercent) / 100;

                                        saleOrderModel.CommissionCollection.Add(managerCommission);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #region Update Value
        /// <summary>
        /// Update price when PriceSchema Changed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void PriceSchemaChanged()
        {
            try
            {
                if (SelectedSaleOrder.SaleOrderDetailCollection != null)
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                    {
                        SetPriceUOM(saleOrderDetailModel);

                        saleOrderDetailModel.CalcSubTotal();

                        saleOrderDetailModel.CalcDueQty();

                        saleOrderDetailModel.CalUnfill();

                        CalculateDiscount(saleOrderDetailModel);

                        _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    }
                    SelectedSaleOrder.CalcSubTotal();
                    SelectedSaleOrder.CalcDiscountAmount();
                    //Need Calculate Total after Subtotal & Discount Percent changed
                    SelectedSaleOrder.CalcTotal();
                    SelectedSaleOrder.CalcBalance();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Set Price to SaleOrderDetail with PriceLevelId(PriceSchemaId)
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        protected void SetPriceUOM(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            try
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
                                if (!saleOrderDetailModel.IsManual)
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
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price1;
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.WholesalePrice))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price2;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price2;
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.Employee))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price3;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price3;
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.CustomPrice))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price4;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price4;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        #endregion

        //Product Process
        /// <summary>
        /// Insert & calculate with single product to sale order detail
        /// <para>showSerialPopup : false => Multi Product Input</para>
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="showSerialPopup"></param>
        protected void SaleProductHandle(base_ProductModel productModel, bool showSerialPopup = true)
        {
            try
            {
                if (!productModel.IsCoupon)
                {
                    //Get VendorName
                    base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == productModel.VendorId));
                    if (vendorModel != null)
                        productModel.VendorName = vendorModel.LegalName;
                }

                string lastSaleOrderDetailResource = string.Empty;
                //Product Group Process
                if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))//Process for ProductGroup
                {
                    //Check Has Children in product Group
                    if (productModel.base_Product.base_ProductGroup1.Any())
                    {
                        //Product Group :Create new SaleOrderDetail , ProductUOM & add to collection 
                        this.BreakSODetailChange = true;
                        string parentResource = SaleOrderDetailProcess(productModel, showSerialPopup, string.Empty);
                        lastSaleOrderDetailResource = parentResource;
                        this.BreakSODetailChange = false;
                        //ProductGroup Child
                        foreach (base_ProductGroup productGroup in productModel.base_Product.base_ProductGroup1)
                        {
                            long productId = productGroup.base_Product.Id;
                            base_Product product = _productRepository.Get(x => x.Id.Equals(productId));
                            if (product != null)
                            {
                                //Get Product From ProductCollection
                                base_ProductModel productInGroupModel = new base_ProductModel(product);
                                //Create new SaleOrderDetail , ProductUOM & add to collection
                                this.BreakSODetailChange = true;
                                SaleOrderDetailProcess(productInGroupModel, showSerialPopup, parentResource, productModel);
                                this.BreakSODetailChange = false;
                            }
                            else
                            {
                                _log4net.Info("Product Id =" + productId + " is not exited");
                            }
                        }
                    }
                    else
                    {
                        _log4net.Info("Product Group but not child");
                    }
                }
                else
                {
                    //Create new SaleOrderDetail , ProductUOM & add to collection
                    this.BreakSODetailChange = true;
                    lastSaleOrderDetailResource = SaleOrderDetailProcess(productModel, showSerialPopup);
                    this.BreakSODetailChange = false;
                }

                if (!string.IsNullOrWhiteSpace(lastSaleOrderDetailResource))
                {
                    SelectedSaleOrderDetail = SelectedSaleOrder.SaleOrderDetailCollection.FirstOrDefault(x => x.Resource.ToString().Equals(lastSaleOrderDetailResource));
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handle single new sale Order detail
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="showSerialPopup"></param>
        /// <param name="productSerialParent"></param>
        protected string SaleOrderDetailProcess(base_ProductModel productModel, bool showSerialPopup, string ParentResource = "", base_ProductModel productParentModel = null)
        {
            try
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

                        //Update Product Unit Collection (useful for setPriceUom when change again PriceSchema)
                        base_ProductUOMModel baseUnitModel = salesOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId.Equals(salesOrderDetailModel.ProductModel.BaseUOMId));
                        if (baseUnitModel != null)
                        {
                            baseUnitModel.RegularPrice = salesOrderDetailModel.ProductModel.RegularPrice;
                        }
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

                if (salesOrderDetailModel != null)
                {

                    CalculateAllTax(SelectedSaleOrder);

                    salesOrderDetailModel.CalcSubTotal();
                    salesOrderDetailModel.CalcDueQty();
                    salesOrderDetailModel.CalUnfill();
                    SelectedSaleOrder.CalcSubTotal();
                    SelectedSaleOrder.CalcBalance();
                    ////set show ship tab when has product in detail
                    //ShowShipTab(SelectedSaleOrder);
                    ////Set ship tab status if config set IsAllowChange order when Ship Fully
                    //SetShipStatus();
                    return salesOrderDetailModel.Resource.ToString();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return string.Empty;
        }

        /// <summary>
        /// add new sale Order Detail & Get UOM
        /// </summary>
        /// <returns></returns>
        protected base_SaleOrderDetailModel AddNewSaleOrderDetail(base_ProductModel productModel, string ParentResource = "", base_ProductModel productParentItem = null)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        //OpenForm
        /// <summary>
        /// Open coupon view to update Amount & Coupon Code
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        protected void OpenCouponView(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            try
            {
                string titleView = saleOrderDetailModel.ProductModel.ProductCategoryId.Equals((int)PaymentMethod.GiftCard) ? Language.GetMsg("SO_Title_GiftCard") : Language.GetMsg("SO_Title_GiftCertification");
                CouponViewModel couponViewModel = new CouponViewModel();
                couponViewModel.SaleOrderDetailModel = saleOrderDetailModel;
                couponViewModel.SaleOrderModel = SelectedSaleOrder;
                bool? result = _dialogService.ShowDialog<CouponView>(_ownerViewModel, couponViewModel, titleView);
                if (result == true)
                {
                    _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                }
                else
                {
                    if (saleOrderDetailModel.CouponCardModel == null)
                        SelectedSaleOrder.SaleOrderDetailCollection.Remove(saleOrderDetailModel);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Update price when product price is 0
        /// </summary>
        /// <param name="productModel"></param>
        protected bool UpdateProductPrice(base_ProductModel productModel)
        {
            bool resultValue = false;
            try
            {
                if (productModel.RegularPrice <= 0)
                {
                    UpdateTransactionViewModel updateTransactionViewModel = new UpdateTransactionViewModel(productModel);
                    bool? result = _dialogService.ShowDialog<UpdateTransactionView>(_ownerViewModel, updateTransactionViewModel, Language.GetMsg("SO_Title_UpdateProductPrice"));
                    if (result == true)
                    {
                        productModel.RegularPrice = updateTransactionViewModel.NewPrice;


                        //Update ProductColletion
                        if (updateTransactionViewModel.IsUpdateProductPrice)
                        {
                            base_Product product = _productRepository.GetProductByResource(updateTransactionViewModel.ProductModel.Resource.ToString());
                            if (product != null)
                            {
                                base_ProductModel productUpdate = new base_ProductModel(product);
                                productUpdate.RegularPrice = updateTransactionViewModel.ProductModel.RegularPrice;
                            }
                        }
                        resultValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return resultValue;
        }

        /// <summary>
        /// Open form tracking serial number
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="isShowQty"></param>
        protected void OpenTrackingSerialNumber(base_SaleOrderDetailModel salesOrderDetailModel, bool isShowQty = false, bool isEditing = true)
        {
            try
            {
                if (!salesOrderDetailModel.ProductModel.IsSerialTracking)
                    return;
                //Show Tracking Serial
                SelectTrackingNumberViewModel trackingNumberViewModel = new SelectTrackingNumberViewModel(salesOrderDetailModel, isShowQty, isEditing);
                bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, trackingNumberViewModel, Language.GetMsg("SO_Title_TrackingSerialNumber"));

                if (result == true)
                {
                    if (isEditing)
                    {
                        salesOrderDetailModel = trackingNumberViewModel.SaleOrderDetailModel;

                        CalculateDiscount(salesOrderDetailModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Search product with advance options..
        /// </summary>
        protected virtual void SearchProductAdvance()
        {
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel();
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, Language.GetMsg("SO_Title_SearchProduct"));
            if (dialogResult == true)
            {
                CreateSaleOrderDetailWithProducts(productSearchViewModel.SelectedProducts);
            }
        }

        /// <summary>
        /// Open Popup New Customer
        /// </summary>
        protected void OpenNewPopupForm()
        {
            PopupGuestViewModel popupGuestViewModel = new PopupGuestViewModel(MarkType.Customer, AddressType.Billing, (short)CustomerTypes.Individual);
            bool? result = _dialogService.ShowDialog<PopupGuestView>(_ownerViewModel, popupGuestViewModel, Language.GetMsg("SO_Title_AddCustomer"));
            if (result == true && popupGuestViewModel.NewItem != null)
            {
                CustomerCollection.Add(popupGuestViewModel.NewItem);
                SelectedCustomer = popupGuestViewModel.NewItem;
            }
        }

        /// <summary>
        /// Create sale order detail with multi Product
        /// </summary>
        /// <param name="productCollection"></param>
        protected void CreateSaleOrderDetailWithProducts(List<base_ProductModel> productCollection)
        {
            try
            {
                foreach (base_ProductModel productModel in productCollection)
                {
                    SaleProductHandle(productModel, false);
                }
                //Open MultiSerialTracking with item has serial tracking
                //App.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{

                IEnumerable<base_SaleOrderDetailModel> saleDetailSerialCollection = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel.IsSerialTracking && string.IsNullOrWhiteSpace(x.SerialTracking));
                if (saleDetailSerialCollection != null && saleDetailSerialCollection.Any())
                {
                    //Show Popup Update Serial Tracking for which item has quantity =1
                    IEnumerable<base_SaleOrderDetailModel> saleOrderCollectionWithOneItem = saleDetailSerialCollection.Where(x =>x.IsNew && x.Quantity == 1);
                    if (saleOrderCollectionWithOneItem.Any())
                    {
                        MultiTrackingNumberViewModel multiTrackingNumber = new MultiTrackingNumberViewModel(saleOrderCollectionWithOneItem);
                        bool? dialogResult = _dialogService.ShowDialog<MultiTrackingNumberView>(_ownerViewModel, multiTrackingNumber, Language.GetMsg("SO_Title_MultiTrackingSerial"));
                    }

                    //Show popup update serial tracking for which item has quantity >1
                    IEnumerable<base_SaleOrderDetailModel> saleOrderCollectionWithMultiItem = saleDetailSerialCollection.Where(x => x.IsNew && x.Quantity > 1);
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderCollectionWithMultiItem)
                    {
                        OpenTrackingSerialNumber(saleOrderDetailModel, true, true);
                    }

                }
                productCollection.Clear();
                //}), System.Windows.Threading.DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method using for set in create SaleOrder Process
        /// </summary>
        protected virtual void CreateExtentSaleOrder() { }

        /// <summary>
        /// Check when change enter so num from user
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <returns></returns>
        protected virtual bool CheckDuplicateSoNum(base_SaleOrderModel saleOrderModel)
        {
            bool result = false;
            try
            {
                if (saleOrderModel != null)
                {
                    IQueryable<base_SaleOrder> query = _saleOrderRepository.GetIQueryable(x => x.Resource != saleOrderModel.Resource && x.SONumber.Equals(saleOrderModel.SONumber));
                    if (query.Any())
                        result = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            saleOrderModel.IsDuplicateSoNum = result;
            return result;
        }

        /// <summary>
        /// Set SaleTax with Booking Channel & Customer additional
        /// require when customer change & Booking Channel Change
        /// </summary>
        protected void SetSaleTaxLocationForSaleOrder(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                //V3: Not follow with booking channel Booking Channel
                if (Define.CONFIGURATION.IsAllowAntiExemptionTax)//Using Default Tax although these customer has set Tax
                {
                    saleOrderModel.IsTaxExemption = false;
                    saleOrderModel.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                    saleOrderModel.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                }
                else
                {
                    if (SelectedCustomer != null && SelectedCustomer.base_Guest.base_GuestAdditional.Any())
                    {
                        //Check Customer Additionnal has set SaleTax=> true : Using that SaleTax ,otherwise TaxExemption
                        base_GuestAdditional guestAdditional = SelectedCustomer.base_Guest.base_GuestAdditional.FirstOrDefault();
                        //This customer is tax excemption
                        if (guestAdditional.IsTaxExemption)
                        {
                            saleOrderModel.IsTaxExemption = true;
                            //Tax exemption code is a ResellerTaxId
                            saleOrderModel.TaxExemption = guestAdditional.ResellerTaxId;
                            saleOrderModel.TaxAmount = 0;
                            saleOrderModel.TaxPercent = 0;
                            saleOrderModel.TaxLocation = 0;
                            saleOrderModel.TaxCode = string.Empty;
                        }
                        else
                        {
                            saleOrderModel.IsTaxExemption = false;
                            saleOrderModel.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                            saleOrderModel.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                        }
                    }
                    else
                    {
                        saleOrderModel.IsTaxExemption = false;
                        saleOrderModel.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                        saleOrderModel.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                    }

                }

                if (saleOrderModel.TaxLocation > 0)
                {
                    GetSaleTax(saleOrderModel);
                }

                //Calculate SaleOrderDetail Tax when Tax Changed
                if (saleOrderModel.SaleOrderDetailCollection.Any())
                {
                    CalculateAllTax(SelectedSaleOrder);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method calc Ship Tax & Product Tax Of this SaleOrder
        /// <para>SaleOrderTax = Ship Tax(Price Dependency) + Product Tax</para>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void CalcProductNShipTaxAmount(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.TaxLocationModel != null
                        && saleOrderModel.TaxLocationModel.TaxCodeModel != null
                        && saleOrderModel.TaxLocationModel.IsShipingTaxable)
                {
                    saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
                    saleOrderModel.ProductTaxAmount = saleOrderModel.TaxAmount - saleOrderModel.ShipTaxAmount;
                }
                else
                    saleOrderModel.ProductTaxAmount = saleOrderModel.TaxAmount;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //Customer
        /// <summary>
        /// Remove all Customer is Deactived in customer collection in combobox choice or list search customer
        /// </summary>
        protected void RemoveCustomerDeactived()
        {
            //Remove which items is deactived
            if (CustomerCollection.Any(x => !x.IsActived))
            {
                IEnumerable<base_GuestModel> customerDeactiveds = CustomerCollection.Where(x => !x.IsActived);
                foreach (base_GuestModel deactivedItem in customerDeactiveds.ToList())
                {
                    CustomerCollection.Remove(deactivedItem);
                }
            }
        }

        /// <summary>
        /// Create New Member Ship Model & Gen Barcode
        /// </summary>
        /// <returns></returns>
        protected base_MemberShipModel NewMemberShipModel(base_GuestModel customerModel)
        {
            try
            {
                base_MemberShipModel memberShipModel = new base_MemberShipModel();
                memberShipModel.Status = (short)MemberShipStatus.Actived;
                memberShipModel.GuestResource = customerModel.Resource.ToString();
                memberShipModel.DateCreated = DateTime.Today;
                memberShipModel.DateUpdated = DateTime.Now;
                memberShipModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                memberShipModel.IsPurged = false;
                memberShipModel.Status = Convert.ToInt16(MemberShipStatus.Actived);
                memberShipModel.MemberType = MemberShipType.Normal.ToDescription();

                IdCardBarcodeGen(memberShipModel);
                return memberShipModel;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;

            }
        }

        /// <summary>
        /// Generate Barcode with EAN13 Format (13 digit numberic)
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        protected void IdCardBarcodeGen(base_MemberShipModel memberShipModel)
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                //GenMemberShipCard
                if (string.IsNullOrWhiteSpace(memberShipModel.IdCard))
                {
                    using (BarcodeLib.Barcode barCode = new BarcodeLib.Barcode())
                    {
                        barCode.IncludeLabel = true;
                        string idCard = currentDate.ToString("yyMMddHHmmss");
                        barCode.Encode(BarcodeLib.TYPE.EAN13, idCard, 200, 70);
                        memberShipModel.IdCardImg = barCode.Encoded_Image_Bytes;
                        memberShipModel.IdCard = barCode.RawData;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Insert /Update new Bill or Ship Address for customer 
        /// </summary>
        /// <param name="customerAddressModel"></param>
        protected void UpdateCustomerAddress(base_GuestAddressModel customerAddressModel)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Update Customer when PaymentTerm changed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void UpdateCustomer(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.GuestModel.IsDirty)
                {
                    //Update Term
                    saleOrderModel.GuestModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                    saleOrderModel.GuestModel.PaymentTermDescription = saleOrderModel.PaymentTermDescription;
                    saleOrderModel.GuestModel.TermDiscount = saleOrderModel.TermDiscountPercent;
                    saleOrderModel.GuestModel.TermNetDue = saleOrderModel.TermNetDue;
                    saleOrderModel.GuestModel.TermPaidWithinDay = saleOrderModel.TermPaidWithinDay;
                    saleOrderModel.GuestModel.IsCOD = saleOrderModel.IsCOD;

                    //Update Customer Reward 
                    saleOrderModel.GuestModel.ToEntity();
                }

                //Onlyt reward Member
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    //Update Membership
                    if (saleOrderModel.GuestModel.MembershipValidated != null && saleOrderModel.GuestModel.MembershipValidated.IsDirty)
                    {
                        saleOrderModel.GuestModel.MembershipValidated.GuestId = saleOrderModel.GuestModel.Id;
                        saleOrderModel.GuestModel.MembershipValidated.ToEntity();
                        if (saleOrderModel.GuestModel.MembershipValidated.IsNew)
                            saleOrderModel.GuestModel.base_Guest.base_MemberShip.Add(saleOrderModel.GuestModel.MembershipValidated.base_MemberShip);
                    }

                    if (saleOrderModel.GuestModel.GuestRewardCollection != null)
                    {
                        //Update Guest Reward Create New
                        if (saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                        {
                            //Add New Guest Reward Created
                            foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems)
                            {
                                guestRewardModel.ToEntity();
                                if (guestRewardModel.IsNew)
                                    saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                            }

                        }

                        //Update Reward Redeem
                        foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection)
                        {
                            //Skip item is a temporary (reward is a sum of cash rewards)
                            if (guestRewardModel.GuestRewardDetailCollection != null)
                            {
                                foreach (base_GuestRewardDetailModel guestRewardDetailModel in guestRewardModel.GuestRewardDetailCollection)
                                {
                                    guestRewardDetailModel.DateApplied = DateTime.Now;
                                    guestRewardDetailModel.UserApplied = Define.USER != null ? Define.USER.LoginName : string.Empty;
                                    guestRewardDetailModel.ToEntity();
                                    if (guestRewardDetailModel.IsNew)
                                        guestRewardModel.base_GuestReward.base_GuestRewardDetail.Add(guestRewardDetailModel.base_GuestRewardDetail);

                                }
                            }

                            guestRewardModel.ToEntity();

                            if (guestRewardModel.IsNew)
                                saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);

                        }

                    }
                }

                _guestRepository.Commit();

                if (saleOrderModel.GuestRewardSaleOrderModel != null && !string.IsNullOrWhiteSpace(saleOrderModel.GuestRewardSaleOrderModel.SaleOrderResource))
                {
                    //Release reward in store
                    if ((!string.IsNullOrWhiteSpace(saleOrderModel.GuestRewardSaleOrderModel.RewardRef) || saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId > 0)
                        && saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any() && saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                    {
                        base_GuestRewardModel guestRewardReleased = saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.SingleOrDefault(x => x.Id.Equals(saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId) || x.GetHashCode().ToString().Equals(saleOrderModel.GuestRewardSaleOrderModel.RewardRef));
                        saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId = guestRewardReleased.base_GuestReward.Id;
                    }

                    saleOrderModel.GuestRewardSaleOrderModel.ToEntity();
                    //Add GuestRewardSaleOrder
                    if (saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                    {
                        _guestRewardSaleOrderRepository.Add(saleOrderModel.GuestRewardSaleOrderModel.base_GuestRewardSaleOrder);
                    }

                    //Commit GuestRewardSaleOrderModel
                    _guestRewardSaleOrderRepository.Commit();
                }



                //Set Id For Reward]
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    if (saleOrderModel.GuestModel.GuestRewardCollection != null)
                        saleOrderModel.GuestModel.GuestRewardCollection.Clear();

                    if (saleOrderModel.GuestModel.GuestRewardCollection != null && saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                        saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Clear();
                    saleOrderModel.GuestModel.EndUpdate();

                    if (saleOrderModel.GuestModel.MembershipValidated != null && saleOrderModel.GuestModel.MembershipValidated.IsDirty)
                    {
                        saleOrderModel.GuestModel.MembershipValidated.ToModel();
                        saleOrderModel.GuestModel.MembershipValidated.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void SaleOrderDetailCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void SaleOrderDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {


        }

        /// <summary>
        /// Save payment collection, payment detail and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void SavePaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            try
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
                                if (paymentDetailModel.CouponCardModel != null && paymentDetailModel.CouponCardModel.IsDirty)
                                {
                                    paymentDetailModel.CouponCardModel.RemainingAmount -= paymentDetailModel.Paid;
                                    paymentDetailModel.CouponCardModel.IsUsed = true;
                                    paymentDetailModel.CouponCardModel.LastUsed = DateTime.Now;
                                    paymentDetailModel.CouponCardModel.ToEntity();
                                    paymentDetailModel.CouponCardModel.EndUpdate();
                                }

                                // Add new payment detail to database
                                if (paymentDetailModel.Id == 0)
                                    paymentItem.base_ResourcePayment.base_ResourcePaymentDetail.Add(paymentDetailModel.base_ResourcePaymentDetail);
                            }
                        }

                        if (paymentItem.IsNew)
                        {
                            paymentItem.Shift = Define.ShiftCode;
                            _paymentRepository.Add(paymentItem.base_ResourcePayment);
                        }

                        _paymentRepository.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Save Order Ship & Relation to DB
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void SaveSaleOrderShipCollection(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.SaleOrderShipCollection == null)
                    return;

                base_SaleOrderShipRepository saleOrderShipRepository = new base_SaleOrderShipRepository();
                base_SaleOrderShipDetailRepository saleOrderShipDetailRepository = new base_SaleOrderShipDetailRepository();

                if (saleOrderModel.SaleOrderShipCollection.DeletedItems.Any())
                {
                    //Delete Sale Order Ship Model
                    foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection.DeletedItems)
                        saleOrderShipRepository.Delete(saleOrderShipModel.base_SaleOrderShip);
                    saleOrderShipRepository.Commit();
                    saleOrderModel.SaleOrderShipCollection.DeletedItems.Clear();
                }

                //Sale Order Ship Model
                foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection.Where(x => x.IsDirty || x.IsNew))
                {
                    //saleOrderShipModel.IsShipped = saleOrderShipModel.IsChecked;

                    // Delete SaleOrderShipDetail
                    if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Any())
                    {
                        foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModelDel in saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems)
                            saleOrderShipDetailRepository.Delete(saleOrderShipDetailModelDel.base_SaleOrderShipDetail);
                        saleOrderShipDetailRepository.Commit();
                        saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Clear();
                    }

                    //Update SaleOrderShipDetail & Upd
                    if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.Any())
                    {
                        foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                        {
                            //Package is shipped & is a new shipped =>update Onhand,CustomerQty
                            if (saleOrderShipModel.IsShipped && !saleOrderShipModel.base_SaleOrderShip.IsShipped && !saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                            {
                                //Descrease OnHand product Store which product in SaleOrderShipDetail
                                //Descrease store with Product On Hand in group with parent product
                                //Cause : Item Product Group not stockable 
                                if (saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                                {
                                    foreach (base_ProductGroup productGroup in saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1)
                                    {
                                        long productId = productGroup.base_Product.Id;
                                        base_Product product = _productRepository.Get(x => x.Id.Equals(productId));
                                        if (product != null)
                                        {
                                            //Get Product From ProductCollection (child product)
                                            base_ProductModel productInGroupModel = new base_ProductModel(product);

                                            //Get Unit Of Product
                                            base_ProductUOM productGroupUOM = _saleOrderRepository.GetProductUomOfProductInGroup(saleOrderModel.StoreCode, productGroup);
                                            if (productGroupUOM != null)
                                            {
                                                decimal baseUnitNumber = productGroupUOM.BaseUnitNumber;

                                                //productGroup.Quantity : quantity default of group
                                                decimal packQty = productGroup.Quantity * saleOrderShipDetailModel.PackedQty;
                                                _productRepository.UpdateOnHandQuantity(productInGroupModel.Resource.ToString(), saleOrderModel.StoreCode, packQty, true, baseUnitNumber);
                                            }
                                        }
                                        else
                                        {

                                            _log4net.Info("ProductId :'" + productId + "' is not existed");
                                        }
                                    }
                                }
                                else
                                {
                                    decimal baseUnitNumber = saleOrderShipDetailModel.SaleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(saleOrderShipDetailModel.SaleOrderDetailModel.UOMId)).BaseUnitNumber;
                                    _productRepository.UpdateOnHandQuantity(saleOrderShipDetailModel.ProductResource, saleOrderModel.StoreCode, saleOrderShipDetailModel.PackedQty, true, baseUnitNumber);
                                }

                                //Descrease Quantity OnCustomer which product in SaleOrderDetail
                                _saleOrderRepository.UpdateCustomerQuantity(saleOrderShipDetailModel.SaleOrderDetailModel, saleOrderModel.StoreCode, saleOrderShipDetailModel.PackedQty, false);
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
                                            base_ProductUOM productGroupUOM = _saleOrderRepository.GetProductUomOfProductInGroup(saleOrderModel.StoreCode, productGroup);
                                            decimal quantityBaseUnit = productGroupUOM.BaseUnitNumber;
                                            totalQuantityBaseUom += quantityBaseUnit * saleOrderShipDetail.PackedQty;
                                            decimal total = 0;
                                            //Get SaleOrderDetail to know is item change price ?
                                            base_SaleOrderDetailModel saleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetail.SaleOrderDetailResource));
                                            if (saleOrderDetailModel != null)
                                            {
                                                int decimalPlace = Define.CONFIGURATION.DecimalPlaces ?? 0;
                                                decimal totalPriceProductGroup = saleOrderShipDetail.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1.Sum(x => x.Amount);
                                                decimal unitPrice = productGroup.RegularPrice + (productGroup.RegularPrice * (saleOrderDetailModel.SalePrice - totalPriceProductGroup) / totalPriceProductGroup);
                                                total = Math.Round(productGroup.Quantity * unitPrice, 2);
                                                _productRepository.UpdateProductStore(productGroup.ProductResource, saleOrderModel.StoreCode, totalQuantityBaseUom, total, 0, 0, true);
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
                                    _productRepository.UpdateProductStore(item.Key, saleOrderModel.StoreCode, totalQuantityBaseUom, item.Sum(x => x.SaleOrderDetailModel.SalePrice * x.PackedQty), 0, 0, true);
                                }
                            }
                        }

                    }
                    //Map value Of Model To Entity
                    saleOrderShipModel.ToEntity();
                    if (saleOrderShipModel.IsNew)
                        saleOrderModel.base_SaleOrder.base_SaleOrderShip.Add(saleOrderShipModel.base_SaleOrderShip);

                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Change language for inheritance class
        /// </summary>
        protected virtual void ChangLanguageExtension()
        {
            //Update BillAddressTypeCollection
            foreach (AddressTypeModel item in BillAddressTypeCollection)
            {
                if (item.ID.Equals(2))
                {
                    item.Name = Language.GetMsg("SO_TextBlock_Billing");
                }
            }

            //Update ShipAddressTypeCollection
            foreach (AddressTypeModel item in ShipAddressTypeCollection)
            {
                if (item.ID.Equals(3))
                {
                    item.Name = Language.GetMsg("SO_TextBlock_Shipping");
                }
            }

            //Update CustomerFieldCollection
            foreach (DataSearchModel item in CustomerFieldCollection)
            {
                switch (item.ID)
                {
                    case 1:
                        item.DisplayName = Language.GetMsg("CUS_Text_CustomerNumber");
                        break;
                    case 2:
                        item.DisplayName = Language.GetMsg("CUS_Text_Name");
                        break;
                }
            }

            //Update for ProductFieldCollection
            foreach (DataSearchModel item in ProductFieldCollection)
            {
                switch (item.ID)
                {
                    case 1:
                        item.DisplayName = Language.GetMsg("C174");
                        break;
                    case 2:
                        item.DisplayName = Language.GetMsg("C176");
                        break;
                    case 3:
                        item.DisplayName = Language.GetMsg("C175");
                        break;
                    case 4:
                        item.DisplayName = Language.GetMsg("C116");
                        break;
                    case 6:
                        item.DisplayName = Language.GetMsg("C117");
                        break;
                }
            }
        }

        #endregion

        #region Permissions
        #region Properties

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

        #endregion
        #endregion
    }
}