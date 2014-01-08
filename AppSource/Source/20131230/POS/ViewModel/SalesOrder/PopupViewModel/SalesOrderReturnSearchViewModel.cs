using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.Windows;
using System.ComponentModel;
using System.Linq.Expressions;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.Helper;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CPC.POS.ViewModel
{
    public class SalesOrderReturnSearchViewModel : ViewModelBase
    {
        #region Define
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_SaleTaxLocationRepository _saleTaxRepository = new base_SaleTaxLocationRepository();

        private string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        #endregion

        #region Constructors
        public SalesOrderReturnSearchViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            InitialCommand();

            LoadCustomer();

            LoadStores();

            LoadSaleTax();
        }


        #endregion

        #region Properties
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

        #region SaleTaxCollection
        public List<base_SaleTaxLocationModel> SaleTaxLocationCollection
        {
            get;
            set;
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
        //Search

        #region IsAdvanced
        private bool _isPreviousAdvanced;
        private bool _isAdvanced;
        /// <summary>
        /// Gets or sets the IsAdvanced.
        /// </summary>
        public bool IsAdvanced
        {
            get { return _isAdvanced; }
            set
            {
                if (_isAdvanced != value)
                {
                    _isAdvanced = value;
                    OnPropertyChanged(() => IsAdvanced);

                    //Clear Keyword 
                    if (IsAdvanced)
                    {
                        _keyword = string.Empty;
                        OnPropertyChanged(() => Keyword);
                    }
                }
            }
        }


        #endregion

        #region Keyword
        private string _keyword;
        /// <summary>
        /// Gets or sets the Keywork.
        /// </summary>
        public string Keyword
        {
            get { return _keyword; }
            set
            {
                if (_keyword != value)
                {
                    _keyword = value;
                    OnPropertyChanged(() => Keyword);

                    if (!string.IsNullOrWhiteSpace(Keyword))
                    {
                        //Close Advance Search when Keywork Changed
                        IsAdvanced = false;
                        ResetAdvanvedSearch();
                    }
                }
            }
        }

        //this text useful store condition for loadstep if user change Keyword but not click on button search
        //=> Load Step need load base on old condition
        public string SearchText { get; set; }
        #endregion

        #region Barcode
        //Store Value For LoadStep if user not click on search button
        private string _previousBarcode;
        private string _barcode;
        /// <summary>
        /// Gets or sets Barcode.
        /// <para>For Binding</para>
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

        #region DocumentNo
        //Store Value For LoadStep if user not click on search button
        private string _documentNoPrevious;
        private string _documentNo;
        /// <summary>
        /// Gets or sets the DocumentNo.
        /// <para>For Binding</para>
        /// </summary>
        public string DocumentNo
        {
            get { return _documentNo; }
            set
            {
                if (_documentNo != value)
                {
                    _documentNo = value;
                    OnPropertyChanged(() => DocumentNo);
                }
            }
        }
        #endregion

        #region OrderFrom
        //Store Value For LoadStep if user not click on search button
        private DateTime? _orderFromPrevious;
        private DateTime? _orderFrom;
        /// <summary>
        /// Gets or sets the OrderFrom.
        /// <para>For Binding</para>
        /// </summary>
        public DateTime? OrderFrom
        {
            get { return _orderFrom; }
            set
            {
                if (_orderFrom != value)
                {
                    _orderFrom = value;
                    OnPropertyChanged(() => OrderFrom);
                }
            }
        }
        #endregion

        #region OrderTo
        //Store Value For LoadStep if user not click on search button
        private DateTime? _orderToPrevious;
        private DateTime? _orderTo;
        /// <summary>
        /// Gets or sets the OrderTo.
        /// <para>For Binding</para>
        /// </summary>
        public DateTime? OrderTo
        {
            get { return _orderTo; }
            set
            {
                if (_orderTo != value)
                {
                    _orderTo = value;
                    OnPropertyChanged(() => OrderTo);
                }
            }
        }
        #endregion

        #region CustomerName
        //Store Value For LoadStep if user not click on search button
        private string _customerNamePrevious;
        private string _customerName;
        /// <summary>
        /// Gets or sets the CustomerName.
        /// <para>For Binding</para>
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            set
            {
                if (_customerName != value)
                {
                    _customerName = value;
                    OnPropertyChanged(() => CustomerName);
                }
            }
        }
        #endregion

        #region TotalType
        //Store Value For LoadStep if user not click on search button
        private string _totalCompareTypePrevious;
        private string _totalCompareType = ">";
        /// <summary>
        /// Gets or sets the TotalType.
        /// <para>For Binding</para>
        /// </summary>
        public string TotalCompareType
        {
            get { return _totalCompareType; }
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
        //Store Value For LoadStep if user not click on search button
        private decimal? _totalPrevious;
        private decimal? _total;
        /// <summary>
        /// Gets or sets the Total.
        /// <para>For Binding</para>
        /// </summary>
        public decimal? Total
        {
            get { return _total; }
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

        #endregion

        #region Commands Methods

        #region NewSaleOrderCommand
        /// <summary>
        /// Gets the NewSaleOrder Command.
        /// <summary>

        public RelayCommand<object> NewSaleOrderCommand { get; private set; }



        /// <summary>
        /// Method to check whether the NewSaleOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewSaleOrderCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the NewSaleOrder command is executed.
        /// </summary>
        private void OnNewSaleOrderCommandExecute(object param)
        {
            ComboItem cmbValue = new ComboItem();
            cmbValue.Text = "SaleOrderReturn.New";
            cmbValue.Detail = 0;
            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", cmbValue);
            CancelSaleOrderRetrunView();
        }
        #endregion

        #region GoToSaleOrderList
        /// <summary>
        /// Gets the GotoSaleOrderList Command.
        /// <summary>

        public RelayCommand<object> GotoSaleOrderListCommand { get; private set; }



        /// <summary>
        /// Method to check whether the GotoSaleOrderList command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnGotoSaleOrderListCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the GotoSaleOrderList command is executed.
        /// </summary>
        private void OnGotoSaleOrderListCommandExecute(object param)
        {
            ComboItem cmbValue = new ComboItem();
            cmbValue.Text = "SaleOrderReturn.SaleOrderList";
            cmbValue.Detail = 0;
            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", cmbValue);
            CancelSaleOrderRetrunView();
        }
        #endregion

        #region SelectedSaleOrderCommand
        /// <summary>
        /// Gets the SelectedSaleOrder Command.
        /// <summary>

        public RelayCommand<object> SelectedSaleOrderCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SelectedSaleOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSelectedSaleOrderCommandCanExecute(object param)
        {
            return SelectedSaleOrder != null;
        }


        /// <summary>
        /// Method to invoke when the SelectedSaleOrder command is executed.
        /// </summary>
        private void OnSelectedSaleOrderCommandExecute(object param)
        {
            ComboItem cmbValue = new ComboItem();
            cmbValue.Text = "SaleOrderReturn.SelectedItem";
            cmbValue.Detail = SelectedSaleOrder.Id;
            cmbValue.IsChecked = (_ownerViewModel as MainViewModel).IsActiveView("SalesOrder");
            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", cmbValue);
            CancelSaleOrderRetrunView();
        }
        #endregion

        #region CancelCommand
        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            CancelSaleOrderRetrunView();
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
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Search command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            //Set Value to variable search
            SearchText = Keyword;
            _isPreviousAdvanced = IsAdvanced;
            _previousBarcode = Barcode;
            _documentNoPrevious = DocumentNo;
            _orderFromPrevious = OrderFrom;
            _orderToPrevious = OrderTo;
            _customerNamePrevious = CustomerName;
            _totalPrevious = Total;
            _totalCompareTypePrevious = TotalCompareType;
            Search();
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
            Expression<Func<base_SaleOrder, bool>> predicate;

            if (_isPreviousAdvanced)
                predicate = CreateAdvancedSearchCondition();
            else
                predicate = CreateSimpleSearchCondition(SearchText);

            LoadDataByPredicate(predicate, false, SaleOrderCollection.Count);
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            NewSaleOrderCommand = new RelayCommand<object>(OnNewSaleOrderCommandExecute, OnNewSaleOrderCommandCanExecute);
            GotoSaleOrderListCommand = new RelayCommand<object>(OnGotoSaleOrderListCommandExecute, OnGotoSaleOrderListCommandCanExecute);
            SelectedSaleOrderCommand = new RelayCommand<object>(OnSelectedSaleOrderCommandExecute, OnSelectedSaleOrderCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
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
                return;
            }

            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                SaleOrderCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;
                Expression<Func<base_SaleOrder, bool>> predicateAll = PredicateBuilder.True<base_SaleOrder>();
                //Show item is created by SaleOrder or isConverted from Quote,Layaway,WorkOrder
                predicateAll = predicateAll.And(x => !x.IsPurge && !x.IsLocked && x.IsConverted && x.base_SaleOrderShip.Any(y => y.IsShipped)).And(predicate);

                //Count all SaleOrder in Data base show on grid
                lock (UnitOfWork.Locker)
                {
                    TotalSaleOrder = _saleOrderRepository.GetIQueryable(predicateAll).Count();

                    //Get data with range
                    IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetRange<DateTime>(currentIndex, NumberOfDisplayItems, x => x.OrderDate.Value, predicateAll);

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
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_SaleOrder, bool>> CreateSimpleSearchCondition(string keyword)
        {
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.False<base_SaleOrder>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
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

                if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalValue))
                {
                    //Total 
                    predicate = predicate.Or(x => x.Total == decimalValue);

                    //Deposit 
                    predicate = predicate.Or(x => x.Deposit == decimalValue);

                    //Balance 
                    predicate = predicate.Or(x => x.Balance.Equals(decimalValue));
                }

                //Price Level
                IEnumerable<short> priceSchemaList = Common.PriceSchemas.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => Convert.ToInt16(x.Value));
                predicate = predicate.Or(x => priceSchemaList.Contains(x.PriceSchemaId));

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

                //Tax Code & Tax Code Excemption
                predicate = predicate.Or(x => x.TaxCode.ToLower().Contains(keyword.ToLower()));

                //Tax Location
                IEnumerable<int> taxLocationList = SaleTaxLocationCollection.Where(x => x.ParentId == 0 && x.Name.ToLower().Contains(keyword.ToLower())).Select(x => x.Id);
                predicate = predicate.Or(x => taxLocationList.Contains(x.TaxLocation));

                //Tax Excemption
                if (Language.GetMsg("SO_TextBlock_TaxExemptionDisplay").ToLower().Contains(keyword.ToLower()))
                {
                    predicate = predicate.Or(x => x.IsTaxExemption);
                }

                //Tax Code Excemption
                predicate = predicate.Or(x => x.TaxExemption.ToLower().Contains(keyword.ToLower()));
            }
            else
            {
                predicate = PredicateBuilder.True<base_SaleOrder>();
            }
            return predicate;
        }

        /// <summary>
        /// Execute Search
        /// </summary>
        private void Search()
        {
            Expression<Func<base_SaleOrder, bool>> predicate;
            if (_isPreviousAdvanced)
                predicate = CreateAdvancedSearchCondition();
            else
                predicate = CreateSimpleSearchCondition(SearchText);

            LoadDataByPredicate(predicate, false, 0);
        }

        /// <summary>
        /// Create Conditionsearch with advanced
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_SaleOrder, bool>> CreateAdvancedSearchCondition()
        {
            Expression<Func<base_SaleOrder, bool>> advancedPredicate = PredicateBuilder.True<base_SaleOrder>();

            base_ProductRepository productRepository = new base_ProductRepository();
            base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

            //Search SO Card by SaleOrderBarcode
            if (!string.IsNullOrWhiteSpace(_previousBarcode))
                advancedPredicate = advancedPredicate.And(x => x.SOCard.ToLower().Contains(_previousBarcode.ToLower()));

            //Search SO Number by DocumentNo
            if (!string.IsNullOrWhiteSpace(_documentNoPrevious))
                advancedPredicate = advancedPredicate.And(x => x.SONumber.ToLower().Contains(DocumentNo.ToLower()));

            //Search by OrderFrom of OrderDate
            if (_orderFromPrevious.HasValue)
            {
                DateTime from = _orderFromPrevious.Value.Date;
                advancedPredicate = advancedPredicate.And(x => from <= x.OrderDate);
            }

            //Search OrderDate by OrderFrom
            if (_orderToPrevious.HasValue)
            {
                DateTime to = _orderToPrevious.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                advancedPredicate = advancedPredicate.And(x => to >= x.OrderDate);
            }
            //Search Customer by CustomerName
            if (!string.IsNullOrWhiteSpace(_customerNamePrevious))
            {
                var customerList = CustomerCollection.Where(y => y.LastName.ToLower().Contains(_customerNamePrevious.ToLower()) || y.FirstName.ToLower().Contains(_customerNamePrevious.ToLower())).Select(x => x.Resource.ToString());
                advancedPredicate = advancedPredicate.And(x => customerList.Contains(x.CustomerResource));
            }

            //Search Total By Total & CompareType
            if (this._totalPrevious.HasValue)
            {
                switch (this._totalCompareTypePrevious)
                {
                    case ">":
                        advancedPredicate = advancedPredicate.And(x => x.Total > this._totalPrevious);
                        break;
                    case "<":
                        advancedPredicate = advancedPredicate.And(x => x.Total < this._totalPrevious);
                        break;
                    case "=":
                        advancedPredicate = advancedPredicate.And(x => x.Total == this._totalPrevious);
                        break;
                }
            }


            return advancedPredicate;
        }

        /// <summary>
        /// Load All Customer From DB
        /// </summary>
        private void LoadCustomer()
        {
            IList<base_Guest> customerList = _guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && !x.IsPurged);

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
        /// Cancel ReturnSearch view
        /// </summary>
        private void CancelSaleOrderRetrunView()
        {
            Window window = this.FindOwnerWindow(this);
            window.DialogResult = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                //Set SaleOrderStatus
                saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                //Set Price Schema
                saleOrderModel.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.PriceSchemaId));


                Guid customerGuid = Guid.NewGuid();
                if (Guid.TryParse(saleOrderModel.CustomerResource, out customerGuid))
                {
                    base_GuestModel customerModel = CustomerCollection.SingleOrDefault(x => x.Resource == customerGuid);
                    if (customerModel != null)
                    {
                        saleOrderModel.GuestModel = customerModel;
                    }
                }

                GetSaleTax(saleOrderModel);

                saleOrderModel.IsDirty = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load Tax for SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void GetSaleTax(base_SaleOrderModel saleOrderModel)
        {
            //Get Tax Location
            base_SaleTaxLocationModel saleTaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == saleOrderModel.TaxLocation);
            if (saleTaxLocationModel != null)
            {
                saleOrderModel.TaxLocationModel = saleTaxLocationModel;
            }
            //Get Tax Code
            base_SaleTaxLocationModel taxCodeModel = SaleTaxLocationCollection.SingleOrDefault(x => x.ParentId == saleOrderModel.TaxLocationModel.Id && x.TaxCode.Equals(saleOrderModel.TaxCode));
            if (taxCodeModel != null)
            {
                saleOrderModel.TaxLocationModel.TaxCodeModel = taxCodeModel;
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
        /// Load SaleTaxCollection
        /// </summary>
        protected void LoadSaleTax()
        {
            IList<base_SaleTaxLocation> saleTaxList = _saleTaxRepository.GetAll();
            if (SaleTaxLocationCollection == null)
                SaleTaxLocationCollection = new List<base_SaleTaxLocationModel>(saleTaxList.Select(x => new base_SaleTaxLocationModel(x)));
        }

        /// <summary>
        /// Reset Advance search codition
        /// </summary>
        private void ResetAdvanvedSearch()
        {
            Barcode = string.Empty;
            DocumentNo = null;
            OrderFrom = null;
            OrderTo = null;
            CustomerName = string.Empty;
            Total = null;
            TotalCompareType = ">";
        }
        #endregion

        #region Public Methods
        #endregion
    }
}
