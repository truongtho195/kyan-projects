using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
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

        #endregion

        #region Properties

        #region Search And Filter

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
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                SearchAlert = string.Empty;

                // Search All
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)
                {
                    // Load data by predicate
                    LoadDataByPredicate(false);
                }
                else if (param != null)
                {
                    Keyword = param.ToString();
                    if (SearchOption == 0)
                    {
                        // Alert: Search option is required
                        SearchAlert = "Search Option is required";
                    }
                    else
                    {
                        // Load data by predicate
                        LoadDataByPredicate(false);
                    }
                }
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
            bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PopupAdjustmentAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue)
            {
                if (msgResult.Value)
                {
                    if (param != null)
                        Keyword = param.ToString();

                    // Create basic predicate combine with advance predicate
                    Expression<Func<base_QuantityAdjustment, bool>> predicate = CreateSearchPredicate(Keyword);

                    // Set advance search condition
                    predicate = predicate.And(viewModel.QuantityAdjustmentPredicate);

                    // Load data by search predicate
                    LoadDataByPredicate(predicate, false, 0);
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

            // Default condition
            predicate = predicate.And(x => x.base_Product.IsPurge == false);

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.Code))
                {
                    predicate = predicate.And(x => x.base_Product.Code.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.ItemName))
                {
                    predicate = predicate.And(x => x.base_Product.ProductName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.PartNumber))
                {
                    predicate = predicate.And(x => x.base_Product.PartNumber.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Description))
                {
                    predicate = predicate.And(x => x.base_Product.Description.ToLower().Contains(keyword.ToLower()) ||
                        x.base_Product.StyleModel.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Vendor))
                {
                    // Get all vendors that contain keyword
                    IEnumerable<base_GuestModel> vendors = VendorList.Where(x => x.Company.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<long> vendorIDList = vendors.Select(x => x.Id);

                    // Get all product that contain in category list
                    if (vendorIDList.Count() > 0)
                        predicate = predicate.And(x => vendorIDList.Count(y => y.Equals(x.base_Product.VendorId)) > 0);
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
                if (SearchOption.Has(SearchOptions.Barcode))
                {
                    predicate = predicate.And(x => x.base_Product.Barcode.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Category))
                {
                    // Get all categories that contain keyword
                    IEnumerable<base_DepartmentModel> categories = CategoryList.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<int> categoryIDList = categories.Select(x => x.Id);

                    // Get all brands that contain keyword
                    IEnumerable<base_DepartmentModel> brands = BrandList.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<int> brandIDList = brands.Select(x => x.Id);

                    // Get all product that contain in category or brand list
                    if (categoryIDList.Count() > 0 || brandIDList.Count() > 0)
                        predicate = predicate.And(x => categoryIDList.Count(y => y.Equals(x.base_Product.ProductCategoryId)) > 0 ||
                            (x.base_Product.ProductBrandId.HasValue && brandIDList.Count(y => y.Equals(x.base_Product.ProductBrandId.Value)) > 0));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
            }

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
            Expression<Func<base_QuantityAdjustment, bool>> predicate = CreateSearchPredicate(Keyword);

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
                    IList<base_QuantityAdjustment> quantityAdjustments = _quantityAdjustmentRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                    foreach (base_QuantityAdjustment quantityAdjustment in quantityAdjustments)
                    {
                        bgWorker.ReportProgress(0, quantityAdjustment);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                    Console.WriteLine(ex.ToString());
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
            // Get logged time
            DateTime loggedTime = DateTimeExt.Now;

            // Get product store model
            base_ProductStore productStore = quantityAdjustmentItem.base_QuantityAdjustment.base_Product.base_ProductStore.
                SingleOrDefault(x => x.StoreCode.Equals(quantityAdjustmentItem.StoreCode));

            // Get new and old quantity
            int newQuantity = quantityAdjustmentItem.OldQty;
            int oldQuantity = productStore.QuantityOnHand;

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
