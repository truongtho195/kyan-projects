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
    class QuantityAdjustmentHistoryViewModel : ViewModelBase
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
        private DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        private int _timerCounter = 0;
        #endregion

        #region Properties

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

        private ObservableCollection<base_QuantityAdjustmentModel> _quantityAdjustmentCollection = new ObservableCollection<base_QuantityAdjustmentModel>();
        /// <summary>
        /// Gets or sets the QuantityAdjustmentCollection.
        /// </summary>
        public ObservableCollection<base_QuantityAdjustmentModel> QuantityAdjustmentCollection
        {
            get { return _quantityAdjustmentCollection; }
            set
            {
                if (_quantityAdjustmentCollection != value)
                {
                    _quantityAdjustmentCollection = value;
                    OnPropertyChanged(() => QuantityAdjustmentCollection);
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

        private int _totalQuantityAdjustment;
        /// <summary>
        /// Gets or sets the TotalQuantityAdjustment.
        /// </summary>
        public int TotalQuantityAdjustment
        {
            get { return _totalQuantityAdjustment; }
            set
            {
                if (_totalQuantityAdjustment != value)
                {
                    _totalQuantityAdjustment = value;
                    OnPropertyChanged(() => TotalQuantityAdjustment);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public QuantityAdjustmentHistoryViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            LoadStaticData();
            InitialCommand();

            //Initial Auto Complete Search
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 1);
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

                // Load data by predicate
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
            if (!IsBusy)
                LoadDataByPredicate(false, QuantityAdjustmentCollection.Count);
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

            // Get quantity adjustment model
            base_QuantityAdjustmentModel quantityAdjustmentModel = param as base_QuantityAdjustmentModel;

            return quantityAdjustmentModel.IsReversed.HasValue && !quantityAdjustmentModel.IsReversed.Value;
        }

        /// <summary>
        /// Method to invoke when the RestoreCommand command is executed.
        /// </summary>
        private void OnRestoreCommandExecute(object param)
        {
            try
            {
                // Get quantity adjustment model
                base_QuantityAdjustmentModel quantityAdjustmentModel = param as base_QuantityAdjustmentModel;

                // Update status
                quantityAdjustmentModel.IsReversed = true;
                quantityAdjustmentModel.Reason = (short)AdjustmentReason.Reverse;
                quantityAdjustmentModel.Status = (short)AdjustmentStatus.Reversing;

                // Map data from model to entity
                quantityAdjustmentModel.ToEntity();

                // Save adjustment
                SaveAdjustment(quantityAdjustmentModel);

                // Restore quantity for product
                quantityAdjustmentModel.ProductModel.SetOnHandToStore(quantityAdjustmentModel.OldQty, quantityAdjustmentModel.StoreCode.Value);

                // Update total quantity in product
                quantityAdjustmentModel.ProductModel.QuantityOnHand -= quantityAdjustmentModel.AdjustmentQtyDiff;

                // Map data from model to entity
                quantityAdjustmentModel.ProductModel.ToEntity();

                // Accept changes
                _quantityAdjustmentRepository.Commit();
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
            if (_waitingTimer != null)
                _waitingTimer.Stop();

            PopupAdjustmentAdvanceSearchViewModel viewModel = new PopupAdjustmentAdvanceSearchViewModel();
            viewModel.IsQuantityAdjustment = true;
            bool? msgResult = _dialogService.ShowDialog<PopupAdjustmentAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue)
            {
                if (msgResult.Value)
                {
                    // Load data by search predicate
                    LoadDataByPredicate(viewModel.QuantityAdjustmentPredicate, false, 0);
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
        private Expression<Func<base_QuantityAdjustment, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_QuantityAdjustment, bool>> predicate = PredicateBuilder.True<base_QuantityAdjustment>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<base_QuantityAdjustment>();

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
                    // Get all adjustments that OldQty contain keyword
                    predicate = predicate.Or(x => x.OldQty.Equals(decimalKeyword));

                    // Get all adjustments that NewQty contain keyword
                    predicate = predicate.Or(x => x.NewQty.Equals(decimalKeyword));

                    // Get all adjustments that AdjustmentQtyDiff contain keyword
                    predicate = predicate.Or(x => x.AdjustmentQtyDiff.Equals(decimalKeyword));
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
            Expression<Func<base_QuantityAdjustment, bool>> predicate = CreateSearchPredicate(_keyword);

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
        private void LoadDataByPredicate(Expression<Func<base_QuantityAdjustment, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                QuantityAdjustmentCollection.Clear();

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

                    // Get total quantity adjustment with condition in predicate
                    TotalQuantityAdjustment = _quantityAdjustmentRepository.GetIQueryable(predicate).Count();

                    // Get data with range
                    IList<base_QuantityAdjustment> quantityAdjustments = _quantityAdjustmentRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.LoggedTime, predicate);
                    foreach (base_QuantityAdjustment quantityAdjustment in quantityAdjustments)
                    {
                        bgWorker.ReportProgress(0, quantityAdjustment);
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "POS", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_QuantityAdjustmentModel quantityAdjustmentModel = new base_QuantityAdjustmentModel((base_QuantityAdjustment)e.UserState);
                LoadRelationData(quantityAdjustmentModel);
                QuantityAdjustmentCollection.Add(quantityAdjustmentModel);
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
        private void LoadRelationData(base_QuantityAdjustmentModel quantityAdjustmentModel)
        {
            // Get product model
            quantityAdjustmentModel.ProductModel = new base_ProductModel(quantityAdjustmentModel.base_QuantityAdjustment.base_Product);

            // Get store name
            if (quantityAdjustmentModel.StoreCode.HasValue)
                quantityAdjustmentModel.StoreName = StoreList.ElementAt(quantityAdjustmentModel.StoreCode.Value).Name;
        }

        /// <summary>
        /// Save adjustment when restore quantity
        /// </summary>
        /// <param name="quantityAdjustmentItem"></param>
        private void SaveAdjustment(base_QuantityAdjustmentModel quantityAdjustmentItem)
        {
            try
            {
                // Get logged time
                DateTime loggedTime = DateTimeExt.Now;

                // Get product store model
                base_ProductStore productStore = quantityAdjustmentItem.base_QuantityAdjustment.base_Product.base_ProductStore.
                    SingleOrDefault(x => x.StoreCode.Equals(quantityAdjustmentItem.StoreCode));

                // Get new and old quantity
                decimal newQuantity = quantityAdjustmentItem.OldQty;
                decimal oldQuantity = productStore.QuantityOnHand;

                // Get new and old cost
                decimal newCost = quantityAdjustmentItem.ProductModel.AverageUnitCost;
                decimal oldCost = newCost;

                // Save quantity adjustment
                // Create new quantity adjustment
                base_QuantityAdjustmentModel quantityAdjustmentModel = new base_QuantityAdjustmentModel();
                quantityAdjustmentModel.ProductId = quantityAdjustmentItem.ProductModel.Id;
                quantityAdjustmentModel.ProductResource = quantityAdjustmentItem.ProductModel.Resource.ToString();
                quantityAdjustmentModel.NewQty = newQuantity;
                quantityAdjustmentModel.OldQty = oldQuantity;
                quantityAdjustmentModel.AdjustmentQtyDiff = newQuantity - oldQuantity;
                quantityAdjustmentModel.CostDifference = newCost * quantityAdjustmentModel.AdjustmentQtyDiff;
                quantityAdjustmentModel.LoggedTime = loggedTime;
                quantityAdjustmentModel.Reason = (short)AdjustmentReason.Reverse;
                quantityAdjustmentModel.Status = (short)AdjustmentStatus.Reversed;
                quantityAdjustmentModel.UserCreated = Define.USER.LoginName;
                quantityAdjustmentModel.IsReversed = true;
                quantityAdjustmentModel.StoreCode = quantityAdjustmentItem.StoreCode;
                quantityAdjustmentModel.StoreName = quantityAdjustmentItem.StoreName;
                quantityAdjustmentModel.ProductModel = quantityAdjustmentItem.ProductModel;

                // Add new quantity adjustment item to database
                _quantityAdjustmentRepository.Add(quantityAdjustmentModel.base_QuantityAdjustment);

                // Save cost adjustment
                if (newQuantity > 0)
                {
                    // Create new cost adjustment
                    base_CostAdjustment costAdjustment = new base_CostAdjustment();
                    costAdjustment.ProductId = quantityAdjustmentItem.ProductModel.Id;
                    costAdjustment.ProductResource = quantityAdjustmentItem.ProductModel.Resource.ToString();
                    costAdjustment.AdjustmentNewCost = newCost;
                    costAdjustment.AdjustmentOldCost = oldCost;
                    costAdjustment.AdjustCostDifference = newCost - oldCost;
                    costAdjustment.NewCost = newCost * newQuantity;
                    costAdjustment.OldCost = oldCost * newQuantity;
                    costAdjustment.CostDifference = costAdjustment.NewCost - costAdjustment.OldCost;
                    costAdjustment.LoggedTime = loggedTime;
                    costAdjustment.Reason = (short)AdjustmentReason.Reverse;
                    costAdjustment.Status = (short)AdjustmentStatus.Reversed;
                    costAdjustment.UserCreated = Define.USER.LoginName;
                    costAdjustment.IsReversed = true;
                    costAdjustment.StoreCode = quantityAdjustmentItem.StoreCode;

                    // Add new cost adjustment item to database
                    _costAdjustmentRepository.Add(costAdjustment);
                }

                // Map data from model to entity
                quantityAdjustmentModel.ToEntity();

                // Add new quantity adjustment to collection
                QuantityAdjustmentCollection.Add(quantityAdjustmentModel);

                // Update total quantity adjustment
                TotalQuantityAdjustment++;

                // Update quantity for product store
                productStore.QuantityOnHand = newQuantity;

                // Update quantity on hand for other UOM
                foreach (base_ProductUOM productUOM in productStore.base_ProductUOM)
                {
                    if (productUOM.BaseUnitNumber != 0)
                        productUOM.QuantityOnHand = Math.Round((decimal)newQuantity / productUOM.BaseUnitNumber, 2);
                }
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