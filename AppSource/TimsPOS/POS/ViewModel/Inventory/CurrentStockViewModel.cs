using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class CurrentStockViewModel : ViewModelBase
    {
        #region Define

        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        public RelayCommand<object> SearchCommand { get; private set; }

        #endregion

        #region Constructors

        public CurrentStockViewModel()
        {
            InitialCommand();
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

        #region ProductCollection
        /// <summary>
        /// Gets or sets the ProductCollection.
        /// </summary>
        private ObservableCollection<base_ProductModel> _productCollection = new ObservableCollection<base_ProductModel>();
        public ObservableCollection<base_ProductModel> ProductCollection
        {
            get
            {
                return _productCollection;
            }
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

        #region TotalProducts
        private int _totalProducts;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalProducts
        {

            get
            {
                return _totalProducts;
            }
            set
            {
                if (_totalProducts != value)
                {
                    _totalProducts = value;
                    OnPropertyChanged(() => TotalProducts);
                }
            }
        }
        #endregion

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
                        this.OnSearchCommandExecute(FilterText);
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

        #region CurrentPageIndex
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get
            {
                return _currentPageIndex;
            }
            set
            {
                _currentPageIndex = value;
                OnPropertyChanged(() => CurrentPageIndex);
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
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        private void OnSearchCommandExecute(object param)
        {
            try
            {
                // TODO: Handle command logic here
                this.SearchAlert = string.Empty;
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && this.SearchOption == 0)//Search All
                {
                    Expression<Func<base_Product, bool>> predicate = this.CreateSearchPredicate(Keyword);
                    this.LoadProduct(predicate, false, 0);
                }
                else if (param != null)
                {
                    this.Keyword = param.ToString();
                    if (this.SearchOption == 0)
                    {
                        //Thong bao Can co dk
                        this.SearchAlert = "Search Option is required";
                    }
                    else
                    {
                        Expression<Func<base_Product, bool>> predicate = this.CreateSearchPredicate(Keyword);
                        this.LoadProduct(predicate, false, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region LoadDataByStepCommand

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
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadProduct(predicate, false, this.CurrentPageIndex);
        }

        #endregion

        #endregion

        #region Private Methods

        #region InitialCommand

        private void InitialCommand()
        {
            // Route the commands
            this.SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
        }

        #endregion

        #region SearchProduct

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (this.SearchOption.Has(SearchOptions.Code))
                {
                    predicate = predicate.And(x => x.Code.ToLower().Contains(keyword.ToLower()));
                }
                else if (this.SearchOption.Has(SearchOptions.ItemName))
                {
                    predicate = predicate.And(x => x.ProductName.ToLower().Contains(keyword.ToLower()));
                }
                else if (this.SearchOption.Has(SearchOptions.PartNumber))
                {
                    predicate = predicate.And(x => x.PartNumber.ToLower().Contains(keyword.ToLower()));
                }
                else if (this.SearchOption.Has(SearchOptions.Barcode))
                {
                    predicate = predicate.And(x => x.Barcode.ToLower().Contains(keyword.ToLower()));
                }
            }
            return predicate;
        }

        #endregion

        #region LoadProduct

        private void LoadProduct(Expression<Func<base_Product, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                this.ProductCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                base.IsBusy = true;
                //if(refreshData)
                //To count all User in Data base show on grid
                this.TotalProducts = _productRepository.GetIQueryable().Count();
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * base.NumberOfDisplayItems;
                IList<base_Product> transferStocks = _productRepository.GetRange(indexItem, base.NumberOfDisplayItems, "It.Id", predicate);
                foreach (var item in transferStocks)
                    bgWorker.ReportProgress(0, item);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                //To add item of TransferStock.
                base_ProductModel model = new base_ProductModel(e.UserState as base_Product);
                model.ProductStoreCollection = new ObservableCollection<base_ProductStoreModel>();
                foreach (var item in model.base_Product.base_ProductStore.OrderBy(x => x.StoreCode))
                    model.ProductStoreCollection.Add(new base_ProductStoreModel(item));
                this.ProductCollection.Add(model);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                base.IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        #endregion

        #endregion

        #region Public Methods

        #region OnViewChangingCommandCanExecute

        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return true;
        }

        #endregion

        #region LoadData

        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            //Get Store
            this.StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
            this.ProductCollection.Clear();
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
            if (!string.IsNullOrWhiteSpace(Keyword) && SearchOption > 0)
            {
                predicate = this.CreateSearchPredicate(Keyword);
            }
            this.LoadProduct(predicate, true);
        }

        #endregion

        #endregion
    }
}
