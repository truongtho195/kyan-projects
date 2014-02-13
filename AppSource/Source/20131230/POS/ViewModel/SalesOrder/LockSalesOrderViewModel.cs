using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Threading;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class LockSalesOrderViewModel : ViewModelBase
    {
        #region Define

        public RelayCommand<object> SearchCommand { get; private set; }

        //Respository
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_ResourceReturnRepository _resourceReturnRepository = new base_ResourceReturnRepository();
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();
        private base_SaleTaxLocationRepository _saleTaxRepository = new base_SaleTaxLocationRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();

        private string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        private string EMPLOYEE_MARK = MarkType.Employee.ToDescription();



        /// <summary>
        /// Using for viewQuotation
        /// </summary>

        private enum SaleOrderTab
        {
            Order = 0,
            Ship = 1,
            Payment = 2,
            Return = 3
        }

        private bool _initialData = false;

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        #endregion

        #region Constructors

        public LockSalesOrderViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            //Get value from config
            IsIncludeReturnFee = Define.CONFIGURATION.IsIncludeReturnFee;

            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }

            InitialCommand();
            LoadStaticData();

        }

        public LockSalesOrderViewModel(bool isList, object param)
            : this()
        {
            ChangeSearchMode(isList, param);
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
                    ContainerTitle = IsSearchMode ? Language.GetMsg("SO_Title_SaleOrderLockedList") : Language.GetMsg("SO_Title_SaleOrderLocked");
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
                return SelectedSaleOrder.IsDirty;
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


        public List<base_SaleTaxLocationModel> SaleTaxLocationCollection { get; set; }


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
                    ResetTimer();
                    OnPropertyChanged(() => FilterText);
                }
            }
        }

        //FilterText will be stored to use for Load Step(load previous Fitler text that after user change)
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


        #endregion

        #region Commands Methods

        #region SearchCommand
        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        public bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        public void OnSearchCommandExecute(object param)
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();
            //Filter Text = null => Search all
            Keyword = FilterText;
            Expression<Func<base_SaleOrder, bool>> predicate = CreateSimpleSearchPredicate(Keyword);
            LoadDataByPredicate(predicate, false, 0);

        }

        #endregion

        #region UnLockSaleOrderCommand
        /// <summary>
        /// Gets the UnLockSaleOrder Command.
        /// <summary>

        public RelayCommand<object> UnLockSaleOrderCommand { get; private set; }


        /// <summary>
        /// Method to check whether the UnLockSaleOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnUnLockSaleOrderCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the UnLockSaleOrder command is executed.
        /// </summary>
        private void OnUnLockSaleOrderCommandExecute(object param)
        {

            UnLockSaleOrderProccess();
        }


        #endregion

        #region UnLockSaleOrderWithMenuCommand
        /// <summary>
        /// Gets the UnLockSaleOrderWithMenu Command.
        /// <summary>

        public RelayCommand<object> UnLockSaleOrderWithMenuCommand { get; private set; }



        /// <summary>
        /// Method to check whether the UnLockSaleOrderWithMenu command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnUnLockSaleOrderWithMenuCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param as ObservableCollection<object>).Count == 1;
        }


        /// <summary>
        /// Method to invoke when the UnLockSaleOrderWithMenu command is executed.
        /// </summary>
        private void OnUnLockSaleOrderWithMenuCommandExecute(object param)
        {

            base_SaleOrderModel saleOrderModel = (param as ObservableCollection<object>).First() as base_SaleOrderModel;
            _selectedSaleOrder = saleOrderModel;
            UnLockSaleOrderProccess();

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

            if (!string.IsNullOrWhiteSpace(Keyword))//Load Step Current With Search Current with Search
                predicate = CreateSimpleSearchPredicate(Keyword);
            LoadDataByPredicate(predicate, false, SaleOrderCollection.Count);
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
            if (IsSearchMode)
            {
                _selectedSaleOrder = null;
                SelectedSaleOrder = param as base_SaleOrderModel;

                SetSaleOrderRelation(SelectedSaleOrder);

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

        #endregion "Commands Methods"

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            UnLockSaleOrderCommand = new RelayCommand<object>(OnUnLockSaleOrderCommandExecute, OnUnLockSaleOrderCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);

            UnLockSaleOrderWithMenuCommand = new RelayCommand<object>(OnUnLockSaleOrderWithMenuCommandExecute, OnUnLockSaleOrderWithMenuCommandCanExecute);
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
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    result = true;
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
                    }
                }
            }

            return result;
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
                predicate = predicate.And(x => !x.IsPurge && x.IsLocked && x.IsConverted);
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
        /// Create predicate Simple Search Condition
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_SaleOrder, bool>> CreateSimpleSearchPredicate(string keyword)
        {
            //Default Condition is Search All
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                //Set Condition
                predicate = PredicateBuilder.False<base_SaleOrder>();

                //SO Number
                predicate = predicate.Or(x => x.SONumber.ToLower().Contains(keyword.ToLower()));

                //Status
                IEnumerable<short> statusList = Common.StatusSalesOrders.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                predicate = predicate.Or(x => statusList.Contains(x.OrderStatus));

                //Search Date 
                DateTime orderDate;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out orderDate))
                {
                    int orderYear = orderDate.Year;
                    int orderMonth = orderDate.Month;
                    int orderDay = orderDate.Day;
                    //Order Date
                    predicate = predicate.Or(x => x.OrderDate.HasValue && x.OrderDate.Value.Year == orderYear && x.OrderDate.Value.Month == orderMonth && x.OrderDate.Value.Day == orderDay);
                }

                //Search Customer Name
                var customerList = CustomerCollection.Where(y => y.LastName.ToLower().Contains(keyword.ToLower()) || y.FirstName.ToLower().Contains(keyword.ToLower())).Select(x => x.Resource.ToString());
                predicate = predicate.Or(x => customerList.Contains(x.CustomerResource));

                //Search deciaml
                decimal decimalValue = 0;

                if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalValue) && decimalValue != 0)
                {
                    //Total 
                    predicate = predicate.Or(x => x.Total == decimalValue);
                }

                ///
                ///Search Store 
                ///
                IEnumerable<base_Store> storeList = StoreCollection.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                //Collection Store index , cause StoreCode sale order storage by index
                IList<int> storeIndexList = new List<int>();
                foreach (base_Store item in storeList)
                {
                    int storeIndex = StoreCollection.IndexOf(item);
                    if (!storeIndexList.Any(x => x.Equals(storeIndex)))
                        storeIndexList.Add(storeIndex);
                }
                predicate = predicate.Or(x => storeIndexList.Contains(x.StoreCode));
            }
            return predicate;
        }

        /// <summary>
        /// Execuate Unlock
        /// </summary>
        private void UnLockSaleOrderProccess()
        {
            try
            {
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_UnLockOrder"), Language.POS, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    UnLockSaleOrder();
                    ComboItem cmbValue = new ComboItem();
                    cmbValue.Text = "UnLock";
                    cmbValue.Detail = SelectedSaleOrder.Id;
                    (_ownerViewModel as MainViewModel).OpenViewExecute("Sales Order", cmbValue);
                    IsSearchMode = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void UnLockSaleOrder()
        {

            SelectedSaleOrder.IsLocked = false;
            //set dateUpdate
            SelectedSaleOrder.DateUpdated = DateTime.Now;

            SelectedSaleOrder.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedSaleOrder.ToEntity();
            _saleOrderRepository.Commit();
            SelectedSaleOrder.EndUpdate();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                //_initialData = true;
                //BreakAllChange = true;

                //Set SaleOrderStatus
                saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                //Set Price Schema
                saleOrderModel.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.PriceSchemaId));

                //Get CustomerModel & relation with customer
                if (CustomerCollection.Any(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)))
                {
                    saleOrderModel.GuestModel = CustomerCollection.Where(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)).FirstOrDefault();

                    _saleOrderRepository.SetGuestAdditionalModel(saleOrderModel);

                    //Get Reward & set PurchaseThreshold if Customer any reward
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
                    saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
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
                //_initialData = false;
                //BreakAllChange = false;
                saleOrderModel.IsDirty = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load SetSaleOrderDetail, SetForSaleOrderShip, SetForShippedCollection,SetToSaleOrderReturn,LoadPaymentCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
            {
                SetSaleOrderDetail(saleOrderModel, isForce);

                SetForSaleOrderShip(saleOrderModel, isForce);

                //Get SaleOrderShipDetail for return
                SetForShippedCollection(saleOrderModel, isForce);

                SetToSaleOrderReturn(saleOrderModel, isForce);

                LoadPaymentCollection(saleOrderModel);

                saleOrderModel.RaiseAnyShipped();
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
        private void SetSaleOrderDetail(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
            {
                //Load sale order detail
                if (isForce || saleOrderModel.SaleOrderDetailCollection == null || !saleOrderModel.SaleOrderDetailCollection.Any())
                {
                    saleOrderModel.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
                    _saleOrderDetailRepository.Refresh(saleOrderModel.base_SaleOrder.base_SaleOrderDetail);
                    foreach (base_SaleOrderDetail saleOrderDetail in saleOrderModel.base_SaleOrder.base_SaleOrderDetail.OrderBy(x => x.Id))
                    {
                        base_SaleOrderDetailModel saleOrderDetailModel = new base_SaleOrderDetailModel(saleOrderDetail);
                        saleOrderDetailModel.Qty = saleOrderDetailModel.Quantity;
                        //Get Product
                        base_Product product = _productRepository.GetProductByResource(saleOrderDetail.ProductResource);

                        if (product != null)
                        {
                            saleOrderDetailModel.ProductModel = new base_ProductModel(product);
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
                    }
                    if (saleOrderModel.SaleOrderDetailCollection != null)
                        saleOrderModel.IsHiddenErrorColumn = !saleOrderModel.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);

                    ShowShipTab(saleOrderModel);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Load Sale Order Shippeds Collection with SaleOrderShipDetailCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce">Can set SaleOrderShipDetailCollection when difference null</param>
        private void SetForShippedCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Load Sale Order Return
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetToSaleOrderReturn(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
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
                    foreach (base_ResourceReturnDetail resourceReturnDetail in saleOrderModel.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail)
                    {
                        base_ResourceReturnDetailModel returnDetailModel = new base_ResourceReturnDetailModel(resourceReturnDetail);
                        returnDetailModel.SaleOrderModel = saleOrderModel;
                        returnDetailModel.SaleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource));
                        returnDetailModel.UnitName = returnDetailModel.SaleOrderDetailModel.UnitName;
                        //CalcReturnDetailSubTotal(saleOrderModel, returnDetailModel);
                        saleOrderModel.ReturnModel.ReturnDetailCollection.Add(returnDetailModel);
                        returnDetailModel.IsDirty = false;
                        returnDetailModel.IsTemporary = false;
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
        /// Load Sale Order Ship Collection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetForSaleOrderShip(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        private void GetSaleTax(base_SaleOrderModel saleOrderModel)
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

        private void LoadStaticData()
        {
            if (CustomerCollection != null)
                CustomerCollection.Clear();

            CustomerCollection = new CollectionBase<base_GuestModel>(_guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && !x.IsPurged).Select(x => new base_GuestModel(x)).OrderBy(x => x.Id));

            //Get Store
            if (StoreCollection != null)
                StoreCollection.Clear();
            StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));

            //Load All Sale Tax
            if (SaleTaxLocationCollection != null)
                SaleTaxLocationCollection.Clear();
            SaleTaxLocationCollection = new List<base_SaleTaxLocationModel>(_saleTaxRepository.GetAll().Select(x => new base_SaleTaxLocationModel(x)));

            ////Load AllProduct
            //if (ProductCollection != null)
            //    ProductCollection.Clear();
            //ProductCollection = new ObservableCollection<base_ProductModel>(_productRepository.GetAll(x => !x.IsPurge.Value).Select(x => new base_ProductModel(x)));
        }

        /// <summary>
        /// Calc Ship Tax with PriceDepent
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private decimal CalcShipTaxAmount(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.TaxLocationModel.TaxCodeModel != null
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
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
            try
            {
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
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return taxAmountResult;
        }

        //Update value
        /// <summary>
        /// Get UOM Collection For sale order detail
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="SetPrice"> True : Set price after set Product Unit</param>
        private void GetProductUOMforSaleOrderDetail(base_SaleOrderDetailModel salesOrderDetailModel, bool SetPrice = true)
        {
            try
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
                        //ProductId = salesOrderDetailModel.ProductModel.Id,
                        UOMId = UOM.Id,
                        Name = UOM.Name,
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

                base_ProductStore productStore = salesOrderDetailModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));
                if (productStore != null)
                {
                    // Gets the remaining units.
                    foreach (base_ProductUOM item in productStore.base_ProductUOM)
                    {
                        salesOrderDetailModel.ProductUOMCollection.Add(new base_ProductUOMModel(item)
                        {
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
                            salesOrderDetailModel.SalePrice = productUOM.RegularPrice;
                        }
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
            saleOrderDetailModel.IsVisibleRowDetail |= saleOrderDetailModel.RegularPrice > saleOrderDetailModel.SalePrice && saleOrderDetailModel.DiscountPercent > 0;
        }

        /// <summary>
        /// Load payment collection and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void LoadPaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                // Initial payment product enumerable

                // Get document resource
                string docResource = saleOrderModel.Resource.ToString();

                // Get all payment by document resource
                IEnumerable<base_ResourcePayment> payments = _paymentRepository.GetAll(x => x.DocumentResource.Equals(docResource));

                // Load payment collection
                saleOrderModel.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>(payments.Select(x => new base_ResourcePaymentModel(x)));

                // Check show PaymentTab
                saleOrderModel.PaymentProcess = saleOrderModel.PaymentCollection.Any();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
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

        /// <summary>
        /// Set Address From customer
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetBillShipAddress(base_GuestModel guestModel, base_SaleOrderModel saleOrderModel)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check to Show ship tab when has saleorder detail
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void ShowShipTab(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel != null)
                saleOrderModel.ShipProcess = (saleOrderModel.SaleOrderDetailCollection != null ? saleOrderModel.SaleOrderDetailCollection.Any() : false) && !saleOrderModel.IsNew;
        }

        private base_RewardManager GetReward(DateTime orderDate)
        {
            try
            {
                short status = (short)StatusBasic.Active;
                var reward = _rewardManagerRepository.Get(x => x.Status.Equals(status) &&
                                             ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                            || !x.IsTrackingPeriod)));
                return reward;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }
        #endregion

        #region Event
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

        #region Propertychanged



        #endregion

        #region Override Methods

        public override void LoadData()
        {
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load with Search Condition
                predicate = CreateSimpleSearchPredicate(FilterText); // CreatePredicateWithConditionSearch(Keyword);

            LoadDataByPredicate(predicate);
        }

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            if (IsBusy)
                return false;
            return ChangeViewExecute(isClosing);
        }

        /// <summary>
        /// Change view from Ribbon
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (SelectedSaleOrder == null)
            {
                IsSearchMode = true;
            }
        }

        public override void ChangeLanguage()
        {
            base.ChangeLanguage();
            //Change Title
            ContainerTitle = IsSearchMode ? Language.GetMsg("SO_Title_SaleOrderLockedList") : Language.GetMsg("SO_Title_SaleOrderLocked");
        }

        #endregion
    }
}