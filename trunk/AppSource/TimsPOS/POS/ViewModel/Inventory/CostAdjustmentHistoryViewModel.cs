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
    class CostAdjustmentHistoryViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_CostAdjustmentRepository _costAdjustmentRepository = new base_CostAdjustmentRepository();
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

                //// Initial predicate
                //Expression<Func<base_CostAdjustment, bool>> predicate = PredicateBuilder.True<base_CostAdjustment>();

                //// Default condition
                //string keyword = "mini";
                //predicate = predicate.And(x => x.base_Product.IsPurge == false);
                //predicate = predicate.And(x => x.base_Product.Description.ToLower().Contains(keyword) ||
                //    x.base_Product.StyleModel.ToLower().Contains(keyword));
                //predicate = predicate.And(x => x.base_Product.PartNumber.ToLower().Contains(keyword));
                //predicate = predicate.And(x => x.base_Product.Code.ToLower().Contains(keyword));
                //var ling = _costAdjustmentRepository.GetAll(predicate);

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
            // Get cost adjustment model
            base_CostAdjustmentModel costAdjustmentModel = param as base_CostAdjustmentModel;

            // Update status
            costAdjustmentModel.IsReversed = true;
            costAdjustmentModel.Status = "Reversed";

            // Map data from model to entity
            costAdjustmentModel.ToEntity();

            // Accept changes
            _costAdjustmentRepository.Commit();
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
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            return true;
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateProductSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false);

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.Code))
                {
                    predicate = predicate.And(x => x.Code.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.ItemName))
                {
                    predicate = predicate.And(x => x.ProductName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.PartNumber))
                {
                    predicate = predicate.And(x => x.PartNumber.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Description))
                {
                    // Initial sub predicate
                    //Expression<Func<base_CostAdjustment, bool>> predicateDescription = PredicateBuilder.False<base_CostAdjustment>();
                    //predicateDescription = predicateDescription.Or(x => x.base_Product.Description.Contains(keyword.ToLower()));
                    //predicateDescription = predicateDescription.Or(x => x.base_Product.StyleModel.Contains(keyword.ToLower()));
                    //predicate = predicate.And(predicateDescription);

                    predicate = predicate.And(x => x.Description.ToLower().Contains(keyword.ToLower()) ||
                        x.StyleModel.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Vendor))
                {
                    // Get all vendors that contain keyword
                    IEnumerable<base_GuestModel> vendors = VendorList.Where(x => x.Company.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<long> vendorIDList = vendors.Select(x => x.Id);

                    // Get all product that contain in category list
                    if (vendorIDList.Count() > 0)
                        predicate = predicate.And(x => vendorIDList.Contains(x.VendorId));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
                if (SearchOption.Has(SearchOptions.Barcode))
                {
                    predicate = predicate.And(x => x.Barcode.ToLower().Contains(keyword.ToLower()));
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
                    if (categoryIDList.Count() > 0)
                        predicate = predicate.And(x => categoryIDList.Contains(x.ProductCategoryId) ||
                            (x.ProductBrandId.HasValue && brandIDList.Contains(x.ProductBrandId.Value)));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
            }

            return predicate;
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
                    List<long> vendorIDList = vendors.Select(x => x.Id).ToList();

                    // Get all product that contain in category list
                    if (vendorIDList.Count() > 0)
                        predicate = predicate.And(x => vendorIDList.Contains(x.base_Product.VendorId));
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
                    if (categoryIDList.Count() > 0)
                        predicate = predicate.And(x => categoryIDList.Contains(x.base_Product.ProductCategoryId) ||
                            (x.base_Product.ProductBrandId.HasValue && brandIDList.Contains(x.base_Product.ProductBrandId.Value)));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
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
            Expression<Func<base_CostAdjustment, bool>> predicate = CreateSearchPredicate(Keyword);
            //Expression<Func<base_Product, bool>> predicateProduct = CreateProductSearchPredicate(Keyword);

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
                    //TotalCostAdjustment = _costAdjustmentRepository.GetIQueryable(predicate).Count();

                    // Get data with range
                    //IList<base_CostAdjustment> costAdjustments = _costAdjustmentRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);

                    lock (UnitOfWork.Locker)
                    {
                        IList<base_CostAdjustment> costAdjustments = _costAdjustmentRepository.GetAll(predicate);

                        foreach (base_CostAdjustment costAdjustment in costAdjustments)
                        {
                            bgWorker.ReportProgress(0, costAdjustment);
                        }
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
