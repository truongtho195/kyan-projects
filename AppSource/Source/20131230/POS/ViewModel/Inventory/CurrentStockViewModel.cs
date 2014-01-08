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
using System.Diagnostics;
using Xceed.Wpf.Toolkit;

namespace CPC.POS.ViewModel
{
    class CurrentStockViewModel : ViewModelBase
    {
        #region Define
        //To create new base_StoreRepository to use it in class.
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        //To create new base_ProductRepository to use it in class.
        private base_ProductRepository _productRepository = new base_ProductRepository();
        //To create new base_DepartmentRepository to use it in class.
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        //To create new base_GuestRepository to use it in class.
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        //To create new base_UOMRepository to use it in class.
        private base_UOMRepository _uomRepository = new base_UOMRepository();
        //To create SearchCommand to use it in class.
        public RelayCommand<object> SearchCommand { get; private set; }
        //To create VendorType to use it in class.
        protected string VendorType = MarkType.Vendor.ToDescription();
        //To create ResourceClone to use it in class.
        private Guid _resourceClone = Guid.Empty;
        #endregion

        #region Constructors

        public CurrentStockViewModel()
        {
            base._ownerViewModel = App.Current.MainWindow.DataContext;
            this.InitialCommand();
            // Load static data
            this.LoadStaticData();
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

        #region Search
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
                    OnPropertyChanged(() => Keyword);

                }
            }
        }
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
                if (this._currentPageIndex != value)
                {
                    _currentPageIndex = value;
                    OnPropertyChanged(() => CurrentPageIndex);
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

        #region SelectedProduct
        /// <summary>
        /// Gets or sets the SelectedProduct.
        /// </summary>
        private base_ProductModel _selectedProduct = new base_ProductModel();
        public base_ProductModel SelectedProduct
        {
            get
            {
                return _selectedProduct;
            }
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(() => SelectedProduct);
                    if (this.SelectedProduct != null)
                        this._resourceClone = this.SelectedProduct.Resource;
                }
            }
        }
        #endregion

        #region ColumnCollection
        /// <summary>
        /// Gets or sets the ColumnCollection.
        /// </summary>
        public ObservableCollection<string> _columnCollection;
        public ObservableCollection<string> ColumnCollection
        {
            get
            {
                return _columnCollection;
            }
            set
            {
                if (_columnCollection != value)
                {
                    _columnCollection = value;
                    OnPropertyChanged(() => ColumnCollection);
                }
            }

        }
        #endregion

        #region CategoryCollection
        private ObservableCollection<base_DepartmentModel> _categoryCollection;
        /// <summary>
        /// Gets or sets the CategoryCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> CategoryCollection
        {
            get { return _categoryCollection; }
            set
            {
                if (_categoryCollection != value)
                {
                    _categoryCollection = value;
                    OnPropertyChanged(() => CategoryCollection);
                }
            }
        }
        #endregion

        #region VendorCollection
        private ObservableCollection<base_GuestModel> _vendorCollection;
        /// <summary>
        /// Gets or sets the VendorCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorCollection
        {
            get { return _vendorCollection; }
            set
            {
                if (_vendorCollection != value)
                {
                    _vendorCollection = value;
                    OnPropertyChanged(() => VendorCollection);
                }
            }
        }
        #endregion

        #region UOMList
        private ObservableCollection<CheckBoxItemModel> _uomList;
        /// <summary>
        /// Gets or sets the UOMList.
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> UOMList
        {
            get { return _uomList; }
            set
            {
                if (_uomList != value)
                {
                    _uomList = value;
                    OnPropertyChanged(() => UOMList);
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
                Expression<Func<base_Product, bool>> predicate = this.CreateSearchPredicate(this.Keyword);
                this.LoadProduct(predicate, false, 0);
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
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
            predicate = predicate.And(x => x.IsPurge == false);
            if (!string.IsNullOrWhiteSpace(this.Keyword))//Load Step Current With Search Current with Search
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadProduct(predicate, false, this.CurrentPageIndex);
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
            return param != null ? true : false;
        }

        /// <summary>
        /// Method to invoke when the DoubleClick command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            if (param != null && param is base_ProductModel)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Product", (param as base_ProductModel).Resource);
            }
        }
        #endregion

        #endregion

        #region Private Methods

        #region InitialCommand
        //To register command.
        private void InitialCommand()
        {
            // Route the commands
            this.SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnDoubleClickViewCommandCanExecute);
        }
        #endregion

        #region Search condition
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
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                predicate = PredicateBuilder.False<base_Product>();
                keyword = keyword.ToLower();
                if (ColumnCollection.Contains(SearchOptions.Code.ToString()))
                {
                    // Get all products that Code contain keyword
                    predicate = predicate.Or(x => x.Code.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.Category.ToString()))
                {
                    // Get all categories contain keyword
                    IEnumerable<base_DepartmentModel> categories = CategoryCollection.Where(x => x.Name.ToLower().Contains(keyword));
                    IEnumerable<int> categoryIDList = categories.Select(x => x.Id);

                    // Get all products that Category contain keyword
                    if (categoryIDList.Count() > 0)
                        predicate = predicate.Or(x => categoryIDList.Contains(x.ProductCategoryId));
                }
                if (ColumnCollection.Contains(SearchOptions.Vendor.ToString()))
                {
                    // Get all vendors contain keyword
                    IEnumerable<base_GuestModel> vendors = VendorCollection.Where(x => x.Company.ToLower().Contains(keyword));
                    IEnumerable<long> vendorIDList = vendors.Select(x => x.Id);

                    // Get all products that Vendor contain keyword
                    if (vendorIDList.Count() > 0)
                        predicate = predicate.Or(x => vendorIDList.Contains(x.VendorId));
                }
                if (ColumnCollection.Contains(SearchOptions.ItemName.ToString()))
                {
                    // Get all products that ProductName contain keyword
                    predicate = predicate.Or(x => x.ProductName.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.Description.ToString()))
                {
                    // Get all products that Description contain keyword
                    predicate = predicate.Or(x => x.Description.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.Attribute.ToString()))
                {
                    // Get all products that Attribute contain keyword
                    predicate = predicate.Or(x => x.Attribute.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.Size.ToString()))
                {
                    // Get all products that Size contain keyword
                    predicate = predicate.Or(x => x.Size.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.Barcode.ToString()))
                {
                    // Get all products that Barcode contain keyword
                    predicate = predicate.Or(x => x.Barcode.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.PartNumber.ToString()))
                {
                    predicate = predicate.Or(x => x.PartNumber.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.Barcode.ToString()))
                {
                    // Get all products that Barcode contain keyword
                    predicate = predicate.Or(x => x.Barcode.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.ALU.ToString()))
                {
                    // Get all products that ALU contain keyword
                    predicate = predicate.Or(x => x.ALU.ToLower().Contains(keyword));
                }
                if (ColumnCollection.Contains(SearchOptions.UOM.ToString()))
                {
                    // Get all unit of measures contain keyword
                    IEnumerable<CheckBoxItemModel> uomItems = UOMList.Where(x => x.Text.ToLower().Contains(keyword));
                    IEnumerable<int> uomIDList = uomItems.Select(x => x.Value);

                    // Get all products that UOM contain keyword
                    predicate = predicate.Or(x => uomIDList.Contains(x.BaseUOMId));
                }
            }
            predicate = predicate.And(x => x.IsPurge == false);
            return predicate;
        }

        #endregion

        #region LoadProduct
        /// <summary>
        /// To load product.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex"></param>
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
                this.TotalProducts = _productRepository.GetIQueryable(predicate).Count();
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * NumberOfDisplayItems;
                IList<base_Product> transferStocks = _productRepository.GetRange(indexItem, NumberOfDisplayItems, "It.Id", predicate);
                foreach (var item in transferStocks)
                    bgWorker.ReportProgress(0, item);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                //To add item of list.
                base_ProductModel model = new base_ProductModel(e.UserState as base_Product);
                base_Guest vendor = _guestRepository.Get(x => x.Mark.Equals(VendorType) && x.IsActived && !x.IsPurged && x.Id.Equals(model.VendorId));//.SingleOrDefault();
                if (vendor != null)
                    model.VendorName = vendor.Company;
                var category = this._departmentRepository.Get(x => x.LevelId == 1 && x.Id == model.ProductCategoryId);//.SingleOrDefault(x => x.Id == model.ProductCategoryId);
                if (category != null)
                    model.CategoryName = category.Name;
                var uom = this._uomRepository.Get(x => x.IsActived && x.Id == model.BaseUOMId);//.SingleOrDefault(x => x.Id == model.BaseUOMId);
                if (uom != null)
                    model.UOMName = uom.Name;
                model.ProductStoreCollection = new CollectionBase<base_ProductStoreModel>();
                foreach (var item in model.base_Product.base_ProductStore.OrderBy(x => x.StoreCode))
                    model.ProductStoreCollection.Add(new base_ProductStoreModel(item));
                this.ProductCollection.Add(model);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                base.IsBusy = false;
                if (this._resourceClone != Guid.Empty)
                    this.SelectedProduct = this.ProductCollection.SingleOrDefault(x => x.Resource == this._resourceClone);
            };
            bgWorker.RunWorkerAsync();
        }

        #endregion

        #region LoadStaticData
        private void LoadStaticData()
        {
            try
            {
                IEnumerable<base_DepartmentModel> departments = this._departmentRepository.
                   GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                   OrderBy(x => x.Name).Select(x => new base_DepartmentModel(x, false));
                CategoryCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 1));

                string vendorType = MarkType.Vendor.ToDescription();
                VendorCollection = new ObservableCollection<base_GuestModel>(this._guestRepository.
                    GetAll(x => x.Mark.Equals(vendorType) && x.IsActived && !x.IsPurged).
                    OrderBy(x => x.Company).
                    Select(x => new base_GuestModel(x, false)));
                UOMList = new ObservableCollection<CheckBoxItemModel>(this._uomRepository.GetIQueryable(x => x.IsActived).
                            OrderBy(x => x.Name).Select(x => new CheckBoxItemModel { Value = x.Id, Text = x.Name }));
                UOMList.Insert(0, new CheckBoxItemModel { Value = 0, Text = string.Empty });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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
            if (isClosing)
                this._resourceClone = Guid.Empty;
            return true;
        }

        #endregion

        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            try
            {
                //Get Store
                this.StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
                this.ProductCollection.Clear();
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
                predicate = predicate.And(x => x.IsPurge == false);
                if (!string.IsNullOrWhiteSpace(this.Keyword))
                    predicate = this.CreateSearchPredicate(this.Keyword);
                this.LoadProduct(predicate, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
        #endregion

        #endregion
    }
}