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

        #endregion

        #region Constructors

        public LockSalesOrderViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            //Get value from config
            IsIncludeReturnFee = Define.CONFIGURATION.IsIncludeReturnFee;
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
        #endregion

        #region Commands Methods

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
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UnLockSaleOrder();
                (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", SelectedSaleOrder.Id);
                IsSearchMode = true;
            }), System.Windows.Threading.DispatcherPriority.Background);
            
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
                predicate = predicate.And(x => !x.IsPurge && x.IsLocked);
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
            _initialData = true;

            //Get CustomerModel
            saleOrderModel.GuestModel = CustomerCollection.Where(x => x.Resource.ToString().Equals(saleOrderModel.CustomerResource)).FirstOrDefault();

            //Get GuestReward collection
            saleOrderModel.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
            foreach (base_GuestReward guestReward in saleOrderModel.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(saleOrderModel.GuestModel.Id) && !x.IsApply))
            {
                int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == guestReward.base_RewardManager.RewardExpiration).Detail);
                DateTime expireDate = guestReward.EarnedDate.Value.AddDays(expireDay);
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

            //Set Address
            SetBillShipAddress(saleOrderModel.GuestModel, saleOrderModel);
            saleOrderModel.RaiseAnyShipped();
            _initialData = false;
            saleOrderModel.IsDirty = false;
        }

        private void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                SetSaleOrderDetail(saleOrderModel);

                SetForSaleOrderShip(saleOrderModel);

                SetToSaleOrderReturn(saleOrderModel);

                //Get SaleOrderShipDetail for return
                SetForShippedCollection(saleOrderModel);

                LoadPaymentCollection(saleOrderModel);

                saleOrderModel.IsDirty = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }

           
        }

        private void SetForShippedCollection(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.SaleOrderShipDetailCollection == null && saleOrderModel.SaleOrderShipCollection != null)
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
                    if (saleOrderModel.SaleOrderShipDetailCollection.Any(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())))
                    {
                        base_SaleOrderDetailModel saleOrderShipModel = saleOrderDetailModel.Clone();
                        saleOrderShipModel.IsNew = false;
                        saleOrderShipModel.PickQty = saleOrderModel.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty.Value);
                        saleOrderShipModel.SubTotal = saleOrderShipModel.PickQty * saleOrderShipModel.SalePrice;
                        saleOrderModel.SaleOrderShippedCollection.Add(saleOrderShipModel);
                    }
                }
            }
        }

        private void SetToSaleOrderReturn(base_SaleOrderModel saleOrderModel)
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
            if (saleOrderModel.ReturnModel.ReturnDetailCollection == null)
            {
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
            }
        }

        private void SetForSaleOrderShip(base_SaleOrderModel saleOrderModel)
        {
            //Collection Sale Order Ship
            if (saleOrderModel.SaleOrderShipCollection == null)
            {
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

                saleOrderModel.PaymentProcess = saleOrderModel.SaleOrderShipCollection.Any();
            }
        }

        private void SetSaleOrderDetail(base_SaleOrderModel saleOrderModel)
        {
            //Load sale order detail
            if (saleOrderModel.SaleOrderDetailCollection == null)
            {
                base_UOMRepository UOMRepository = new base_UOMRepository();
                saleOrderModel.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
                _saleOrderDetailRepository.Refresh(saleOrderModel.base_SaleOrder.base_SaleOrderDetail);
                foreach (base_SaleOrderDetail saleOrderDetail in saleOrderModel.base_SaleOrder.base_SaleOrderDetail)
                {
                    base_SaleOrderDetailModel saleOrderDetailModel = new base_SaleOrderDetailModel(saleOrderDetail);
                    saleOrderDetailModel.Qty = saleOrderDetailModel.Quantity;
                    saleOrderDetailModel.ProductModel = ProductCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetail.ProductResource));
                    //Get VendorName
                    base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == saleOrderDetailModel.ProductModel.VendorId));
                    if (vendorModel != null)
                        saleOrderDetailModel.ProductModel.VendorName = vendorModel.LegalName;

                    saleOrderDetailModel.UOMId = saleOrderDetail.UOMId;
                    saleOrderDetailModel.UnitName = UOMRepository.Get(x => x.Id.Equals(saleOrderDetailModel.UOMId.Value)).Name;
                    saleOrderDetailModel.UnitDiscount = Math.Round(Math.Round(saleOrderDetailModel.RegularPrice * saleOrderDetailModel.DiscountPercent / 100, 2) - 0.01M);
                    saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.SalePrice;
                    saleOrderDetailModel.TotalDiscount = Math.Round(saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity, 2);
                    //Check RowDetail Visibility
                    CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    //saleOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                    saleOrderDetailModel.IsDirty = false;
                    saleOrderModel.SaleOrderDetailCollection.Add(saleOrderDetailModel);

                }

                ShowShipTab(saleOrderModel);
            }
        }

        private void LoadStaticData()
        {
            if(CustomerCollection!=null)
                CustomerCollection.Clear();
            
            CustomerCollection = new CollectionBase<base_GuestModel>(_guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && !x.IsPurged && x.IsActived).Select(x => new base_GuestModel(x)).OrderBy(x => x.Id));

            //Get Store
            if (StoreCollection != null)
                StoreCollection.Clear();
            StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));

            //Load All Sale Tax
            if (SaleTaxLocationCollection!=null)
                SaleTaxLocationCollection.Clear();
            SaleTaxLocationCollection = new List<base_SaleTaxLocationModel>(_saleTaxRepository.GetAll().Select(x => new base_SaleTaxLocationModel(x)));
            
            //Load AllProduct
            if (ProductCollection != null)
                ProductCollection.Clear();
            ProductCollection = new ObservableCollection<base_ProductModel>(_productRepository.GetAll(x => !x.IsPurge.Value).Select(x => new base_ProductModel(x)));
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
                    //ProductId = salesOrderDetailModel.ProductModel.Id,
                    UOMId = UOM.Id,
                    Code = UOM.Code,
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
                        salesOrderDetailModel.SalePrice = productUOM.RegularPrice;
                    }
                }
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
        /// Check to Show ship tab when has saleorder detail
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void ShowShipTab(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel != null)
                saleOrderModel.ShipProcess = (saleOrderModel.SaleOrderDetailCollection != null ? saleOrderModel.SaleOrderDetailCollection.Any() : false) && !saleOrderModel.IsNew;
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

                    //CalculateMultiNPriceTax();

                    CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    break;
                case "Quantity":
                    saleOrderDetailModel.CalcSubTotal();
                    saleOrderDetailModel.CalUnfill();
                    //if (!saleOrderDetailModel.ProductModel.IsSerialTracking)
                    //    CalcProductDiscount(saleOrderDetailModel);
                    //SelectedSaleOrder.CalcSubTotal();
                    //SetShipStatus();
                    break;
                case "DueQty":
                    saleOrderDetailModel.CalUnfill();
                    break;
                case "UOMId":
                    //SetPriceUOM(saleOrderDetailModel);
                    //CalcProductDiscount(saleOrderDetailModel, true);
                    break;
                case "SubTotal":
                    SelectedSaleOrder.CalcSubTotal();
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
                case "Price":
                    resourceReturnDetailModel.Amount = resourceReturnDetailModel.Price * resourceReturnDetailModel.ReturnQty;
                    break;
                case "ReturnQty":
                    resourceReturnDetailModel.Amount = resourceReturnDetailModel.Price * resourceReturnDetailModel.ReturnQty;
                    base_SaleOrderDetailModel saleOrderDetail = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(resourceReturnDetailModel.OrderDetailResource));
                    if (saleOrderDetail != null && SelectedSaleOrder != null)
                    {
                        decimal TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.OrderDetailResource.Equals(resourceReturnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
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
                        SelectedSaleOrder.ReturnModel.RaiseRefundAccepted();
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
                //CheckReturned();
                SelectedSaleOrder.ReturnModel.RaiseRefundAccepted();
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
            predicate = predicate.And(x => x.OrderStatus != orderStatus);

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

            if (SelectedSaleOrder == null)
            {
                IsSearchMode = true;
            }
            //if (param == null)
            //{
            //    if (ChangeViewExecute(null))
            //    {
            //        if (!isList)
            //        {
            //            IsSearchMode = false;
            //        }
            //        else
            //            IsSearchMode = true;
            //    }
            //}
            //else
            //{

            //}
        }

        #endregion

    }
}
