using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PricingCustomViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();

        private bool _isCheckingAll;
        private bool _isRightDataLoaded;
        private List<object> _resultFilterList;
        private ICollectionView _leftCollectionView;
        /// <summary>
        /// Gets ProductID.
        /// </summary>
        public List<ComboItem> ProductIDList;

        /// <summary>
        /// Gets or sets the DepartmentList
        /// </summary>
        public List<ComboItem> CategoryList { get; set; }
        #endregion

        #region Properties

        private ObservableCollection<base_ProductModel> _leftProductCollection = new ObservableCollection<base_ProductModel>();
        /// <summary>
        /// Gets or sets the LeftProductCollection.
        /// </summary>
        public ObservableCollection<base_ProductModel> LeftProductCollection
        {
            get { return _leftProductCollection; }
            set
            {
                if (_leftProductCollection != value)
                {
                    _leftProductCollection = value;
                    OnPropertyChanged(() => LeftProductCollection);
                }
            }
        }

        private ObservableCollection<base_ProductModel> _rightProductCollection = new ObservableCollection<base_ProductModel>();
        /// <summary>
        /// Gets or sets the RightProductCollection.
        /// </summary>
        public ObservableCollection<base_ProductModel> RightProductCollection
        {
            get { return _rightProductCollection; }
            set
            {
                if (_rightProductCollection != value)
                {
                    _rightProductCollection = value;
                    OnPropertyChanged(() => RightProductCollection);
                }
            }
        }

        private object _resultFilterCollection;
        /// <summary>
        /// Gets or sets the ResultFilterCollection.
        /// </summary>
        public object ResultFilterCollection
        {
            get { return _resultFilterCollection; }
            set
            {
                if (_resultFilterCollection != value)
                {
                    _resultFilterCollection = value;
                    OnPropertyChanged(() => ResultFilterCollection);

                    if (ResultFilterCollection != null)
                        _resultFilterList = ResultFilterCollection as List<object>;
                    else
                        _resultFilterList = null;

                    OnRightItemChecked();
                }
            }
        }

        private int _totalProducts;
        /// <summary>
        /// Gets or sets the TotalProducts.
        /// </summary>
        public int TotalProducts
        {
            get { return _totalProducts; }
            set
            {
                if (_totalProducts != value)
                {
                    _totalProducts = value;
                    OnPropertyChanged(() => TotalProducts);
                }
            }
        }

        private bool? _isCheckedAllLeft = false;
        /// <summary>
        /// Gets or sets the IsCheckedAllLeft.
        /// </summary>
        public bool? IsCheckedAllLeft
        {
            get { return _isCheckedAllLeft; }
            set
            {
                if (_isCheckedAllLeft != value)
                {
                    _isCheckedAllLeft = value;
                    OnPropertyChanged(() => IsCheckedAllLeft);
                    if (IsCheckedAllLeft.HasValue)
                    {
                        _isCheckingAll = true;
                        foreach (base_ProductModel productModel in LeftProductCollection)
                            productModel.IsChecked = IsCheckedAllLeft.Value;
                        _isCheckingAll = false;
                    }
                }
            }
        }

        private bool? _isCheckedAllRight = false;
        /// <summary>
        /// Gets or sets the IsCheckedAllRight.
        /// </summary>
        public bool? IsCheckedAllRight
        {
            get { return _isCheckedAllRight; }
            set
            {
                if (_isCheckedAllRight != value)
                {
                    _isCheckedAllRight = value;
                    OnPropertyChanged(() => IsCheckedAllRight);
                    if (IsCheckedAllRight.HasValue)
                    {
                        _isCheckingAll = true;
                        if (_resultFilterList != null)
                            foreach (base_ProductModel productModel in _resultFilterList.Cast<base_ProductModel>())
                                productModel.IsChecked = IsCheckedAllRight.Value;
                        else
                            foreach (base_ProductModel productModel in RightProductCollection)
                                productModel.IsChecked = IsCheckedAllRight.Value;
                        _isCheckingAll = false;
                    }
                }
            }
        }

        public int CurrentPageIndexLeft { get; set; }

        #region Search And Filter Left

        private int _searchOptionLeft;
        /// <summary>
        /// Gets or sets the SearchOptionLeft.
        /// </summary>
        public int SearchOptionLeft
        {
            get { return _searchOptionLeft; }
            set
            {
                if (_searchOptionLeft != value)
                {
                    _searchOptionLeft = value;
                    OnPropertyChanged(() => SearchOptionLeft);
                    if (!string.IsNullOrWhiteSpace(FilterTextLeft))
                        OnLeftSearchCommandExecute(FilterTextLeft);
                }
            }
        }

        private string _filterTextLeft;
        /// <summary>
        /// Gets or sets the FilterTextLeft.
        /// <para>Keyword user input but not press enter</para>
        /// <remarks>Binding in textbox keyword</remarks>
        /// </summary>
        public string FilterTextLeft
        {
            get { return _filterTextLeft; }
            set
            {
                if (_filterTextLeft != value)
                {
                    _filterTextLeft = value;
                    OnPropertyChanged(() => FilterTextLeft);
                }
            }
        }

        public string KeywordLeft { get; set; }

        private string _searchAlertLeft;
        /// <summary>
        /// Gets or sets the SearchAlertLeft.
        /// </summary>
        public string SearchAlertLeft
        {
            get { return _searchAlertLeft; }
            set
            {
                if (_searchAlertLeft != value)
                {
                    _searchAlertLeft = value;
                    OnPropertyChanged(() => SearchAlertLeft);
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PricingCustomViewModel()
        {
            InitialCommand();

        }
        public PricingCustomViewModel(List<ComboItem> categoryList)
        {
            this.InitialCommand();
            this.CategoryList = categoryList;
            // Load data by predicate
            this.LoadLeftDataByPredicate(true);

        }
        /// <summary>
        /// Constructor with load data
        /// </summary>
        /// <param name="categoryList">Category list</param>
        /// <param name="promotionAffectList">Promotion affect list</param>
        #endregion

        #region Command Methods

        #region LeftSearchCommand

        /// <summary>
        /// Gets the LeftSearchCommand command.
        /// </summary>
        public ICommand LeftSearchCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the LeftSearchCommand command is executed.
        /// </summary>
        private void OnLeftSearchCommandExecute(object param)
        {
            try
            {
                SearchAlertLeft = string.Empty;
                // Search All
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOptionLeft == 0)
                {
                    // Load data by predicate
                    LoadLeftDataByPredicate(false);
                }
                else if (param != null)
                {
                    KeywordLeft = param.ToString();
                    if (SearchOptionLeft == 0)
                    {
                        // Alert: Search option is required
                        SearchAlertLeft = "Search Option is required";
                    }
                    else
                    {
                        // Load data by predicate
                        LoadLeftDataByPredicate(false);
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

        #region LoadLeftStepCommand

        /// <summary>
        /// Gets the LoadLeftStepCommand command.
        /// </summary>
        public ICommand LoadLeftStepCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the LoadLeftStepCommand command is executed.
        /// </summary>
        private void OnLoadLeftStepCommandExecute()
        {
            // Load data by predicate
            LoadLeftDataByPredicate(false, LeftProductCollection.Count);
        }

        #endregion

        #region MoveCommand

        /// <summary>
        /// Gets the MoveCommand command.
        /// </summary>
        public ICommand MoveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the MoveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMoveCommandCanExecute()
        {
            return LeftProductCollection.Count(x => x.IsChecked) > 0;
        }

        /// <summary>
        /// Method to invoke when the MoveCommand command is executed.
        /// </summary>
        private void OnMoveCommandExecute()
        {
            // Update total products
            TotalProducts -= LeftProductCollection.Count(x => x.IsChecked);

            // Move item from left to right collection
            foreach (base_ProductModel productModel in LeftProductCollection.Where(x => x.IsChecked).ToList())
            {
                productModel.PropertyChanged -= LeftItemChecked;
                productModel.PropertyChanged += RightItemChecked;
                productModel.IsChecked = false;
                RightProductCollection.Add(productModel);
                if (_resultFilterList != null)
                    _resultFilterList.Add(productModel);
                LeftProductCollection.Remove(productModel);
            }
            // Load step item for left collection
            if (LeftProductCollection.Count < NumberOfDisplayItems)
                this.OnLoadLeftStepCommandExecute();

            // Raise IsCheckedAll
            OnLeftItemChecked();
            OnRightItemChecked();
        }

        #endregion

        #region BackCommand

        /// <summary>
        /// Gets the BackCommand command.
        /// </summary>
        public ICommand BackCommand { get; private set; }

        /// <summary>
        /// Method to check whether the BackCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnBackCommandCanExecute()
        {
            bool result = true;
            if (_resultFilterList != null)
                result = _resultFilterList.Cast<ModelBase>().Count(x => x.IsChecked) > 0;
            else
                result = RightProductCollection.Count(x => x.IsChecked) > 0;
            return result;
        }

        /// <summary>
        /// Method to invoke when the BackCommand command is executed.
        /// </summary>
        private void OnBackCommandExecute()
        {
            long maxID = RightProductCollection.Max(x => x.Id);
            if (LeftProductCollection.Count > 0)
                maxID = LeftProductCollection.Max(x => x.Id);

            // Move item from right to left collection
            if (_resultFilterList != null)
            {
                // Update total products
                TotalProducts += _resultFilterList.Count;

                foreach (base_ProductModel productModel in _resultFilterList.Cast<base_ProductModel>())
                {
                    if (productModel.Id < maxID)
                    {
                        productModel.PropertyChanged -= RightItemChecked;
                        productModel.PropertyChanged += LeftItemChecked;
                        productModel.IsChecked = false;
                        LeftProductCollection.Add(productModel);
                    }
                    RightProductCollection.Remove(productModel);
                }

                // Clear result filter list
                _resultFilterList.Clear();
            }
            else
            {
                // Update total products
                TotalProducts += RightProductCollection.Count(x => x.IsChecked);
                foreach (base_ProductModel productModel in RightProductCollection.Where(x => x.IsChecked).ToList())
                {
                    if (productModel.Id < maxID)
                    {

                        productModel.PropertyChanged -= RightItemChecked;
                        productModel.PropertyChanged += LeftItemChecked;
                        productModel.IsChecked = false;
                        LeftProductCollection.Add(productModel);
                    }
                    RightProductCollection.Remove(productModel);
                }
            }
            // Raise IsCheckedAll
            OnLeftItemChecked();
            OnRightItemChecked();
        }

        #endregion

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return RightProductCollection.Count() > 0;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            ///To get all of productID.
            this.GetProductID();
            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        #region InitialCommand
        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            LeftSearchCommand = new RelayCommand<object>(OnLeftSearchCommandExecute);
            LoadLeftStepCommand = new RelayCommand(OnLoadLeftStepCommandExecute);
            MoveCommand = new RelayCommand(OnMoveCommandExecute, OnMoveCommandCanExecute);
            BackCommand = new RelayCommand(OnBackCommandExecute, OnBackCommandCanExecute);

            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion

        #region Left Methods

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateLeftSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Get all product has not deleted
            predicate = predicate.And(x => x.IsPurge == false);

            // Get all product that contain in category list
            if (this.CategoryList != null && this.CategoryList.Count() > 0)
            {
                IEnumerable<int> categoryList = CategoryList.Select(x => x.IntValue);
                predicate = predicate.And(x => categoryList.Contains(x.ProductCategoryId));
            }
            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOptionLeft > 0)
            {
                if (SearchOptionLeft.Has(SearchOptions.Code))
                {
                    predicate = predicate.And(x => x.Code.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOptionLeft.Has(SearchOptions.ItemName))
                {
                    predicate = predicate.And(x => x.ProductName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOptionLeft.Has(SearchOptions.Category))
                {
                    // Get all categories that contain keyword
                    IEnumerable<ComboItem> categories = CategoryList.Where(x => x.Text.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<int> categoryIDList = categories.Select(x => x.IntValue);

                    // Get all product that category contain in category list
                    if (categoryIDList.Count() > 0)
                        predicate = predicate.And(x => categoryIDList.Contains(x.ProductCategoryId));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
            }
            // Get all productID that contain in promotion affect list
            IEnumerable<long> productIDList = this.RightProductCollection.Select(x => x.Id);

            // Get all product that NOT contain in promotion affect list
            if (productIDList.Count() > 0)
                predicate = predicate.And(x => !productIDList.Contains(x.Id));

            return predicate;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadLeftDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create predicate
            Expression<Func<base_Product, bool>> predicate = CreateLeftSearchPredicate(KeywordLeft);

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                LeftProductCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                try
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    // Get total product with condition in predicate
                    this.TotalProducts = _productRepository.GetIQueryable(predicate).Count();

                    // Get data with range
                    IList<base_Product> products = _productRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                    foreach (base_Product product in products)
                    {
                        bgWorker.ReportProgress(0, product);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_ProductModel productModel = new base_ProductModel((base_Product)e.UserState);
                LoadLeftRelationData(productModel);
                LeftProductCollection.Add(productModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Load right data
                LoadRightData();

                // Resort left product collection
                _leftCollectionView.Refresh();

                // Turn off BusyIndicator
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadLeftRelationData(base_ProductModel productModel)
        {
            if (string.IsNullOrWhiteSpace(productModel.CategoryName))
            {
                ComboItem category = CategoryList.FirstOrDefault(x => x.IntValue.Equals(productModel.ProductCategoryId));
                if (category != null)
                    productModel.CategoryName = category.Text;
                else
                    productModel.CategoryName = string.Empty;
            }

            productModel.PropertyChanged += new PropertyChangedEventHandler(LeftItemChecked);
        }

        /// <summary>
        /// Turn on IsCheckedAll
        /// </summary>
        private void OnLeftItemChecked()
        {
            if (!_isCheckingAll)
            {
                int numOfItemChecked = LeftProductCollection.Count(x => x.IsChecked);
                if (numOfItemChecked == 0)
                    _isCheckedAllLeft = false;
                else if (numOfItemChecked == LeftProductCollection.Count)
                    _isCheckedAllLeft = true;
                else
                    _isCheckedAllLeft = null;
                OnPropertyChanged(() => IsCheckedAllLeft);
            }
        }

        #endregion

        #region Right Methods

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateRightSearchPredicate()
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.False<base_Product>();
            // Get all product has not deleted
            predicate = predicate.And(x => x.IsPurge == false);
            // If promotion afffect list have not item, set fake condition for predicate
            predicate = predicate.Or(x => x.Id < 0);
            return predicate;
        }

        /// <summary>
        /// Load data from database with predicate
        /// </summary>
        /// <param name="predicate">Expression</param>
        /// <param name="isRefreshData">Refresh data. Default is false</param>
        private void LoadRightDataByPredicate(Expression<Func<base_Product, bool>> predicate, bool isRefreshData = false)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            // Process content background worker
            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                {
                    IsBusy = true;
                }

                if (isRefreshData)
                {
                    _productRepository.Refresh();
                }

                // Get data from database with range
                IQueryable<base_Product> products = _productRepository.GetIQueryable(predicate);

                foreach (base_Product product in products)
                {
                    bgWorker.ReportProgress(0, product);
                }
            };

            // Process when background worker call ReportProgress method
            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_ProductModel productModel = new base_ProductModel((base_Product)e.UserState);
                LoadRightRelationData(productModel);
                RightProductCollection.Add(productModel);
            };

            // Process when background worker completed
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
        /// <param name="productModel">ProductModel</param>
        private void LoadRightRelationData(base_ProductModel productModel)
        {
        }

        /// <summary>
        /// Load right data
        /// </summary>
        private void LoadRightData()
        {
            // Load right data only first time
            if (!_isRightDataLoaded)
            {
                _isRightDataLoaded = true;

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = CreateRightSearchPredicate();

                // Load data by predicate
                LoadRightDataByPredicate(predicate);

                _leftCollectionView = CollectionViewSource.GetDefaultView(LeftProductCollection);
                if (_leftCollectionView.SortDescriptions.Count == 0)
                    _leftCollectionView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            }
        }

        /// <summary>
        /// Turn on IsCheckedAll
        /// </summary>
        private void OnRightItemChecked()
        {
            if (!_isCheckingAll)
            {
                int numOfItemChecked = 0;
                if (_resultFilterList != null)
                {
                    numOfItemChecked = _resultFilterList.Cast<ModelBase>().Count(x => x.IsChecked);
                    if (numOfItemChecked == 0)
                        _isCheckedAllRight = false;
                    else if (numOfItemChecked == _resultFilterList.Count)
                        _isCheckedAllRight = true;
                    else
                        _isCheckedAllRight = null;
                }
                else
                {
                    numOfItemChecked = RightProductCollection.Count(x => x.IsChecked);
                    if (numOfItemChecked == 0)
                        _isCheckedAllRight = false;
                    else if (numOfItemChecked == RightProductCollection.Count)
                        _isCheckedAllRight = true;
                    else
                        _isCheckedAllRight = null;
                }
                OnPropertyChanged(() => IsCheckedAllRight);
            }
        }

        #endregion

        #region GetProductID
        /// <summary>
        /// Get all productID.
        /// </summary>
        private void GetProductID()
        {
            this.ProductIDList = new List<ComboItem>();
            foreach (base_ProductModel productModel in this.RightProductCollection)
                this.ProductIDList.Add(new ComboItem { LongValue = productModel.Id });
        }
        #endregion

        #endregion

        #region Override Methods

        /// <summary>
        /// Process when left item checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftItemChecked(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    OnLeftItemChecked();
                    break;
            }
        }

        /// <summary>
        /// Process when right item checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightItemChecked(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    OnRightItemChecked();
                    break;
            }
        }

        #endregion
    }
}