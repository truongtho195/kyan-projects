using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class CostAdjustmentHistoryViewModel : ViewModelBase
    {
        #region Defines

        private base_CostAdjustmentRepository _costAdjustmentRepository = new base_CostAdjustmentRepository();
        private base_QuantityAdjustmentRepository _quantityAdjustmentRepository = new base_QuantityAdjustmentRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        #endregion

        #region Properties

        #region Search
        #region Keyword
        private string _keyword;
        /// <summary>
        /// Gets or sets the Keyword.
        /// </summary>
        public string Keyword
        {
            get { return _keyword; }
            set
            {
                if (_keyword != value)
                {
                    _keyword = value;
                    ResetTimer();
                    OnPropertyChanged(() => Keyword);
                }
            }
        }
        #endregion

        #endregion

        private ObservableCollection<base_CostAdjustmentModel> _costAdjustmentCollection = new ObservableCollection<base_CostAdjustmentModel>();
        /// <summary>
        /// Gets or sets the CostAdjustmentCollection.
        /// </summary>
        public ObservableCollection<base_CostAdjustmentModel> CostAdjustmentCollection
        {
            get { return _costAdjustmentCollection; }
            set
            {
                if (_costAdjustmentCollection != value)
                {
                    _costAdjustmentCollection = value;
                    OnPropertyChanged(() => CostAdjustmentCollection);
                }
            }
        }

        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public List<base_DepartmentModel> CategoryList { get; private set; }

        /// <summary>
        /// Gets or sets the BrandList
        /// </summary>
        public List<base_DepartmentModel> BrandList { get; set; }

        /// <summary>
        /// Gets or sets the VendorList
        /// </summary>
        public List<base_GuestModel> VendorList { get; private set; }

        /// <summary>
        /// Gets or sets the StoreList
        /// </summary>
        public List<base_StoreModel> StoreList { get; private set; }

        private int _totalCostAdjustment;
        /// <summary>
        /// Gets or sets the TotalCostAdjustment.
        /// </summary>
        public int TotalCostAdjustment
        {
            get { return _totalCostAdjustment; }
            set
            {
                if (_totalCostAdjustment != value)
                {
                    _totalCostAdjustment = value;
                    OnPropertyChanged(() => TotalCostAdjustment);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CostAdjustmentHistoryViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            LoadStaticData();
            InitialCommand();

            //Initial Auto Complete Search
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
        }

        #endregion

        #region Command Methods

        #region SearchCommand

        /// <summary>
        /// Gets the SearchCommand command.
        /// </summary>
        public ICommand SearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {

                if (_waitingTimer != null)
                    _waitingTimer.Stop();

                //Load data by predicate
                LoadDataByPredicate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region LoadStepCommand

        /// <summary>
        /// Gets the LoadStepCommand command.
        /// </summary>
        public ICommand LoadStepCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadStepCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStepCommand command is executed.
        /// </summary>
        private void OnLoadStepCommandExecute()
        {
            // Load data by predicate
            LoadDataByPredicate(false, CostAdjustmentCollection.Count);
        }

        #endregion

        #region RestoreCommand

        /// <summary>
        /// Gets the RestoreCommand command.
        /// </summary>
        public ICommand RestoreCommand { get; private set; }

        /// <summary>
        /// Method to check whether the RestoreCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRestoreCommandCanExecute(object param)
        {
            if (param == null)
                return false;

            // Get cost adjustment model
            base_CostAdjustmentModel costAdjustmentModel = param as base_CostAdjustmentModel;

            return costAdjustmentModel.IsReversed.HasValue && !costAdjustmentModel.IsReversed.Value;
        }

        /// <summary>
        /// Method to invoke when the RestoreCommand command is executed.
        /// </summary>
        private void OnRestoreCommandExecute(object param)
        {
            try
            {
                // Get cost adjustment model
                base_CostAdjustmentModel costAdjustmentModel = param as base_CostAdjustmentModel;

                // Update status
                costAdjustmentModel.IsReversed = true;
                costAdjustmentModel.Reason = (short)AdjustmentReason.Reverse;
                costAdjustmentModel.Status = (short)AdjustmentStatus.Reversing;

                // Map data from model to entity
                costAdjustmentModel.ToEntity();

                // Save adjustment
                SaveAdjustment(costAdjustmentModel);

                // Restore average unit cost
                if (costAdjustmentModel.AdjustmentOldCost.HasValue)
                    costAdjustmentModel.ProductModel.AverageUnitCost = costAdjustmentModel.AdjustmentOldCost.Value;

                // Calculator margin, markup and price
                costAdjustmentModel.ProductModel.UpdateMarginMarkupAndPrice();

                // Get product store by define store code
                base_ProductStore productStore = costAdjustmentModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));

                if (productStore != null)
                {
                    foreach (base_ProductUOM productUOM in productStore.base_ProductUOM)
                    {
                        if (productUOM.BaseUnitNumber != 0)
                        {
                            // Update average cost for other UOM
                            productUOM.AverageCost = costAdjustmentModel.ProductModel.AverageUnitCost * productUOM.BaseUnitNumber;
                        }
                    }
                }

                // Map data from model to entity
                costAdjustmentModel.ProductModel.ToEntity();

                // Accept changes
                _costAdjustmentRepository.Commit();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region PopupAdvanceSearchCommand

        /// <summary>
        /// Gets the PopupAdvanceSearchCommand command.
        /// </summary>
        public ICommand PopupAdvanceSearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupAdvanceSearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupAdvanceSearchCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupAdvanceSearchCommand command is executed.
        /// </summary>
        private void OnPopupAdvanceSearchCommandExecute(object param)
        {
            PopupAdjustmentAdvanceSearchViewModel viewModel = new PopupAdjustmentAdvanceSearchViewModel();
            bool? msgResult = _dialogService.ShowDialog<PopupAdjustmentAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue)
            {
                if (msgResult.Value)
                {
                    // Load data by search predicate
                    LoadDataByPredicate(viewModel.CostAdjustmentPredicate, false, 0);
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            try
            {
                // Get all department to load category and brand list
                IEnumerable<base_DepartmentModel> departments = _departmentRepository.
                    GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                    Select(x => new base_DepartmentModel(x));

                // Load category list
                CategoryList = new List<base_DepartmentModel>(departments.Where(x => x.LevelId == 1));

                // Load brand list
                BrandList = new List<base_DepartmentModel>(departments.Where(x => x.LevelId == 2));

                // Load vendor list
                string vendorType = MarkType.Vendor.ToDescription();
                VendorList = new List<base_GuestModel>(_guestRepository.
                    GetAll(x => x.Mark.Equals(vendorType) && x.IsActived && !x.IsPurged).Select(x => new base_GuestModel(x)));

                // Load store list
                StoreList = new List<base_StoreModel>(_storeRepository.GetAll().OrderBy(x => x.Id).Select(x => new base_StoreModel(x)));
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            LoadStepCommand = new RelayCommand(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            RestoreCommand = new RelayCommand<object>(OnRestoreCommandExecute, OnRestoreCommandCanExecute);
            PopupAdvanceSearchCommand = new RelayCommand<object>(OnPopupAdvanceSearchCommandExecute, OnPopupAdvanceSearchCommandCanExecute);
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_CostAdjustment, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_CostAdjustment, bool>> predicate = PredicateBuilder.True<base_CostAdjustment>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<base_CostAdjustment>();

                // Parse keyword to DateTime
                DateTime dateTimeKeyword = DateTimeExt.Now;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeKeyword))
                {
                    int year = dateTimeKeyword.Year;
                    int month = dateTimeKeyword.Month;
                    int day = dateTimeKeyword.Day;

                    // Get all adjustments that LoggedTime contain keyword
                    predicate = predicate.Or(x => x.LoggedTime.Year.Equals(year) &&
                        x.LoggedTime.Month.Equals(month) && x.LoggedTime.Day.Equals(day));
                }

                // Get all stores contain keyword
                IEnumerable<base_StoreModel> stores = StoreList.Where(x => x.Name.ToLower().Contains(keyword));
                IList<int> storeIDList = new List<int>();
                foreach (base_StoreModel store in stores)
                {
                    int storeIndex = StoreList.IndexOf(store);
                    if (!storeIDList.Any(x => x.Equals(storeIndex)))
                        storeIDList.Add(storeIndex);
                }

                // Get all adjustments that StoreCode contain keyword
                if (storeIDList.Count() > 0)
                    predicate = predicate.Or(x => storeIDList.Contains(x.StoreCode.Value));

                // Get all reason types contain keyword
                IEnumerable<ComboItem> reasonItems = Common.AdjustmentReason.Where(x => x.Text.ToLower().Contains(keyword));
                IEnumerable<short> reasonItemIDList = reasonItems.Select(x => x.Value);

                // Get all adjustments that Reason contain keyword
                if (reasonItemIDList.Count() > 0)
                    predicate = predicate.Or(x => reasonItemIDList.Contains(x.Reason));

                // Get all statuses contain keyword
                IEnumerable<ComboItem> statusItems = Common.AdjustmentStatus.Where(x => x.Text.ToLower().Contains(keyword));
                IEnumerable<short> statusItemIDList = statusItems.Select(x => x.Value);

                // Get all adjustments that Status contain keyword
                if (statusItemIDList.Count() > 0)
                    predicate = predicate.Or(x => statusItemIDList.Contains(x.Status));

                // Get all adjustments that Code contain keyword
                predicate = predicate.Or(x => x.base_Product.Code.ToLower().Contains(keyword));

                // Get all adjustments that ProductName contain keyword
                predicate = predicate.Or(x => x.base_Product.ProductName.ToLower().Contains(keyword));

                // Get all adjustments that Attribute contain keyword
                predicate = predicate.Or(x => x.base_Product.Attribute.ToLower().Contains(keyword));

                // Get all adjustments that Size contain keyword
                predicate = predicate.Or(x => x.base_Product.Size.ToLower().Contains(keyword));

                // Parse keyword to Decimal
                decimal decimalKeyword = 0;
                if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalKeyword) && decimalKeyword != 0)
                {
                    // Get all adjustments that OldCost contain keyword
                    predicate = predicate.Or(x => x.OldCost.Equals(decimalKeyword));

                    // Get all adjustments that NewCost contain keyword
                    predicate = predicate.Or(x => x.NewCost.Equals(decimalKeyword));

                    // Get all adjustments that CostDifference contain keyword
                    predicate = predicate.Or(x => x.CostDifference.Equals(decimalKeyword));
                }
            }

            // Default condition
            predicate = predicate.And(x => x.base_Product.IsPurge == false);

            return predicate;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_CostAdjustment, bool>> predicate = CreateSearchPredicate(_keyword);

            // Load data by predicate
            LoadDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(Expression<Func<base_CostAdjustment, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                CostAdjustmentCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                try
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    if (refreshData)
                    {
                        // Refresh data
                        //_costAdjustmentRepository.Refresh();
                    }

                    // Get total cost adjustment with condition in predicate
                    TotalCostAdjustment = _costAdjustmentRepository.GetIQueryable(predicate).Count();

                    // Get data with range
                    IList<base_CostAdjustment> costAdjustments = _costAdjustmentRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.LoggedTime, predicate);
                    foreach (base_CostAdjustment costAdjustment in costAdjustments)
                    {
                        bgWorker.ReportProgress(0, costAdjustment);
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_CostAdjustmentModel costAdjustmentModel = new base_CostAdjustmentModel((base_CostAdjustment)e.UserState);
                LoadRelationData(costAdjustmentModel);
                CostAdjustmentCollection.Add(costAdjustmentModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Turn off BusyIndicator
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationData(base_CostAdjustmentModel costAdjustmentModel)
        {
            // Get product model
            costAdjustmentModel.ProductModel = new base_ProductModel(costAdjustmentModel.base_CostAdjustment.base_Product);

            // Get store name
            if (costAdjustmentModel.StoreCode.HasValue)
                costAdjustmentModel.StoreName = StoreList.ElementAt(costAdjustmentModel.StoreCode.Value).Name;
        }

        /// <summary>
        /// Save adjustment when restore cost
        /// </summary>
        /// <param name="costAdjustmentItem"></param>
        private void SaveAdjustment(base_CostAdjustmentModel costAdjustmentItem)
        {
            try
            {
                // Get logged time
                DateTime loggedTime = DateTimeExt.Now;

                // Get product store model
                base_ProductStore productStore = costAdjustmentItem.base_CostAdjustment.base_Product.base_ProductStore.
                    SingleOrDefault(x => x.StoreCode.Equals(costAdjustmentItem.StoreCode));

                // Get new and old quantity
                decimal newQuantity = productStore.QuantityOnHand;
                decimal oldQuantity = newQuantity;

                // Get new and old cost
                decimal newCost = costAdjustmentItem.OldCost / newQuantity;
                decimal oldCost = costAdjustmentItem.ProductModel.AverageUnitCost;

                // Save quantity adjustment
                // Create new quantity adjustment
                base_QuantityAdjustment quantityAdjustment = new base_QuantityAdjustment();
                quantityAdjustment.ProductId = costAdjustmentItem.ProductModel.Id;
                quantityAdjustment.ProductResource = costAdjustmentItem.ProductModel.Resource.ToString();
                quantityAdjustment.NewQty = newQuantity;
                quantityAdjustment.OldQty = oldQuantity;
                quantityAdjustment.AdjustmentQtyDiff = newQuantity - oldQuantity;
                quantityAdjustment.CostDifference = newCost * quantityAdjustment.AdjustmentQtyDiff;
                quantityAdjustment.LoggedTime = loggedTime;
                quantityAdjustment.Reason = (short)AdjustmentReason.Reverse;
                quantityAdjustment.Status = (short)AdjustmentStatus.Reversed;
                quantityAdjustment.UserCreated = Define.USER.LoginName;
                quantityAdjustment.IsReversed = true;
                quantityAdjustment.StoreCode = costAdjustmentItem.StoreCode;

                // Add new quantity adjustment item to database
                _quantityAdjustmentRepository.Add(quantityAdjustment);

                // Save cost adjustment
                // Create new cost adjustment
                base_CostAdjustmentModel costAdjustmentModel = new base_CostAdjustmentModel();
                costAdjustmentModel.ProductId = costAdjustmentItem.ProductModel.Id;
                costAdjustmentModel.ProductResource = costAdjustmentItem.ProductModel.Resource.ToString();
                costAdjustmentModel.AdjustmentNewCost = newCost;
                costAdjustmentModel.AdjustmentOldCost = oldCost;
                costAdjustmentModel.AdjustCostDifference = newCost - oldCost;
                costAdjustmentModel.NewCost = newCost * newQuantity;
                costAdjustmentModel.OldCost = oldCost * newQuantity;
                costAdjustmentModel.CostDifference = costAdjustmentModel.NewCost - costAdjustmentModel.OldCost;
                costAdjustmentModel.LoggedTime = loggedTime;
                costAdjustmentModel.Reason = (short)AdjustmentReason.Reverse;
                costAdjustmentModel.Status = (short)AdjustmentStatus.Reversed;
                costAdjustmentModel.UserCreated = Define.USER.LoginName;
                costAdjustmentModel.IsReversed = true;
                costAdjustmentModel.StoreCode = costAdjustmentItem.StoreCode;
                costAdjustmentModel.StoreName = costAdjustmentItem.StoreName;
                costAdjustmentModel.ProductModel = costAdjustmentItem.ProductModel;

                // Add new cost adjustment to collection
                CostAdjustmentCollection.Add(costAdjustmentModel);

                // Update total cost adjustment
                TotalCostAdjustment++;

                // Map data from model to entity
                costAdjustmentModel.ToEntity();

                // Add new cost adjustment item to database
                _costAdjustmentRepository.Add(costAdjustmentModel.base_CostAdjustment);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #region Auto Searching
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
            if (Define.CONFIGURATION.IsAutoSearch && this._waitingTimer != null)
            {
                this._waitingTimer.Stop();
                this._waitingTimer.Start();
                _timerCounter = 0;
            }
        }
        #endregion

        #endregion

        #region Override Methods

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            // Load data by predicate
            LoadDataByPredicate();
        }

        #endregion
    }
}