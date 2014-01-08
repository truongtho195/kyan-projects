using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Globalization;
using System.Threading;

namespace CPC.POS.ViewModel
{
    class CountSheetViewModel : ViewModelBase
    {
        #region Define
        //To define command to use it in class.
        public RelayCommand<object> NewCommand { get; private set; }
        public RelayCommand<object> EditCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand RestoreCommand { get; private set; }
        public RelayCommand<object> FilterProductCommand { get; private set; }
        //To define repository to use it in class.
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_CountStockRepository _countStockRepository = new base_CountStockRepository();
        private base_CountStockDetailRepository _countStockDetailRepository = new base_CountStockDetailRepository();
        private base_PricingManagerRepository _pricingManagerRepository = new base_PricingManagerRepository();
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_PricingChangeRepository _pricingChangeRepository = new base_PricingChangeRepository();
        private base_GuestRepository _vendorRepository = new base_GuestRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_ProductStoreRepository _productStoreRepository = new base_ProductStoreRepository();
        //To get type of affectpricing.
        private string _typeAffectPricing = string.Empty;
        //To define SelectedCountStockClone to use it in class.
        private base_CountStockModel _selectedCountStockClone { get; set; }

        private bool _requireWaitingLoadStore = false;
        #endregion

        #region Constructors

        public CountSheetViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            this.InitialCommand();
            this.InitialData();
        }

        public CountSheetViewModel(bool isList, object param = null)
            : this()
        {
            this.ChangeSearchMode(isList, param);
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

        #region SelectedCountStock
        /// <summary>
        /// Gets or sets the SelectedCountStock.
        /// </summary>
        private base_CountStockModel _selectedCountStock;
        public base_CountStockModel SelectedCountStock
        {
            get
            {
                return _selectedCountStock;
            }
            set
            {
                if (_selectedCountStock != value)
                {
                    _selectedCountStock = value;
                    this.OnPropertyChanged(() => SelectedCountStock);
                }
            }
        }

        private void SelectedCountStock_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
        #endregion

        #region CountStockCollection
        /// <summary>
        /// Gets or sets the CountStockCollection.
        /// </summary>
        private ObservableCollection<base_CountStockModel> _countStockCollection = new ObservableCollection<base_CountStockModel>();
        public ObservableCollection<base_CountStockModel> CountStockCollection
        {
            get
            {
                return _countStockCollection;
            }
            set
            {
                if (_countStockCollection != value)
                {
                    _countStockCollection = value;
                    OnPropertyChanged(() => CountStockCollection);
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

        #region TotalCountSheet
        private int _totalCountSheet;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalCountSheet
        {

            get
            {
                return _totalCountSheet;
            }
            set
            {
                if (_totalCountSheet != value)
                {
                    _totalCountSheet = value;
                    OnPropertyChanged(() => TotalCountSheet);
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

        #region IsSetDefault
        protected bool _isSetDefault;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsSetDefault</para>
        /// </summary>
        public bool IsSetDefault
        {
            get { return this._isSetDefault; }
            set
            {
                if (this._isSetDefault != value)
                {
                    this._isSetDefault = value;
                    OnPropertyChanged(() => IsSetDefault);

                }
            }
        }
        #endregion

        #region IsIncludeAccountLocked
        protected bool _isIncludeAccountLocked;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsIncludeAccountLocked</para>
        /// </summary>
        public bool IsIncludeAccountLocked
        {
            get { return this._isIncludeAccountLocked; }
            set
            {
                if (this._isIncludeAccountLocked != value)
                {
                    this._isIncludeAccountLocked = value;
                    OnPropertyChanged(() => IsIncludeAccountLocked);
                    this.SetVisibilityData(value);
                }
            }
        }
        #endregion

        #region EnableFilteringData
        /// <summary>
        /// To get , set value when enable colunm.
        /// </summary>
        private bool _enableFilteringData = true;
        public bool EnableFilteringData
        {
            get { return _enableFilteringData; }
            set
            {
                if (_enableFilteringData != value)
                {
                    _enableFilteringData = value;
                    OnPropertyChanged(() => EnableFilteringData);
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
                    this.Keyword = this.FilterText;
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

        #region NotePopupCollection
        /// <summary>
        /// Gets or sets the NotePopupCollection.
        /// </summary>
        public ObservableCollection<PopupContainer> NotePopupCollection { get; set; }
        #endregion

        #region ShowOrHiddenNote
        /// <summary>
        /// Gets the ShowOrHiddenNote
        /// </summary>
        public string ShowOrHiddenNote
        {
            get
            {
                //if (this.NotePopupCollection.Count == 0)
                //    return "Show Stickies";
                //else if (this.NotePopupCollection.Count == this.SelectedCountStock.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                //    return "Hide Stickies";
                //else
                return "Show Stickies";
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

        #region SelectedStore
        /// <summary>
        /// Gets or sets the SelectedStore.
        /// </summary>
        private base_Store _selectedStore;
        public base_Store SelectedStore
        {
            get
            {
                return _selectedStore;
            }
            set
            {
                if (_selectedStore != value)
                {
                    _selectedStore = value;
                    this.OnPropertyChanged(() => SelectedStore);
                    this.IsSetAllQuantity = false;
                    if (this.SelectedStore != null)
                    {
                        Expression<Func<base_ProductStore, bool>> predicate = PredicateBuilder.True<base_ProductStore>();
                        predicate = this.CreateProductPredicate(this.StoreCollection.IndexOf(this.SelectedStore));
                        this.LoadProductWithStore(predicate, true);
                        this.SelectedCountStock.IsDirty = true;
                        this.SelectedCountStock.IsChangeProductCollection = true;
                    }
                }
            }
        }
        #endregion

        #region IsSetAllQuantity
        private bool isSetAllQuantity = false;
        /// <summary>
        /// Search Mode: 
        /// true open the Search grid.
        /// false close the search grid and open data entry.
        /// </summary>
        public bool IsSetAllQuantity
        {
            get { return isSetAllQuantity; }
            set
            {
                if (value != isSetAllQuantity)
                {
                    isSetAllQuantity = value;
                    OnPropertyChanged(() => IsSetAllQuantity);
                    if (this.SelectedCountStock != null && this.SelectedCountStock.CountStockDetailCollection != null)
                        if (this.IsSetAllQuantity && this.SelectedCountStock.Status <= 2)
                        {
                            foreach (var item in this.SelectedCountStock.CountStockDetailCollection.Where(x => !x.Difference.HasValue))
                            {
                                item.CountedQty = 0;
                                item.IsSetCounted = true;
                            }
                        }
                        else if (this.SelectedCountStock.Status <= 2 && this.SelectedCountStock.IsDirty)
                        {
                            foreach (var item in this.SelectedCountStock.CountStockDetailCollection.Where(x => x.IsSetCounted))
                            {
                                item.IsSetCounted = false;
                                item.Difference = null;
                                item.CountedQty = null;
                            }
                        }
                }
            }
        }
        #endregion

        #region SelectedIndex
        /// <summary>
        /// Gets or sets the SelectedIndex.
        /// </summary>
        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(() => SelectedIndex);
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

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute(object param)
        {
            if (this.ChangeViewExecute(null))
            {
                // TODO: Handle command logic here
                this.SelectedStore = null;
                this.TotalProducts = this.ProductCollection.Count;
                this.SelectedCountStock = new base_CountStockModel();
                this.SelectedCountStock.Resource = Guid.NewGuid();
                this.SelectedCountStock.DocumentNo = DateTime.Now.ToString(Define.GuestNoFormat);
                this.SelectedCountStock.UserCreated = Define.USER.LoginName;
                this.SelectedCountStock.IsLoad = true;
                this.SelectedCountStock.Status = 1;
                this.SelectedCountStock.DateCreated = DateTimeExt.Now;
                this.SelectedCountStock.CountStockDetailCollection = new ObservableCollection<base_CountStockDetailModel>();
                this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count;
                _requireWaitingLoadStore = true;
                this.SelectedStore = this.StoreCollection.ElementAt(Define.StoreCode);
                _requireWaitingLoadStore = false;
                this.SelectedCountStock.IsDirty = false;
                this.SelectedCountStock.IsLoad = false;
                this.SelectedCountStock.IsChangeProductCollection = false;
                //To set enable of detail grid.
                this.IsSearchMode = false;
            }
        }
        #endregion

        #region EditCommand
        /// <summary>
        /// Method to check whether the EditCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditCommandCanExecute(object param)
        {
            return this.IsValid && (this.SelectedCountStock != null && !this.SelectedCountStock.IsDirty && this.SelectedCountStock.Status == 2);
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return this.IsValid && (this.SelectedCountStock != null && this.SelectedCountStock.IsDirty && this.SelectedCountStock.CountStockDetailCollection != null && this.SelectedCountStock.CountStockDetailCollection.Count > 0);
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            try
            {
                this.SelectedCountStock.Shift = Define.ShiftCode;
                // TODO: Handle command logic here
                if (this.SelectedCountStock.IsNew)
                    this.Insert();
                else
                    this.Update();
                this.SelectedCountStock.ToModelAndRaise();
                this.SelectedCountStock.EndUpdate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (this.SelectedCountStock != null)
                return true;
            return false;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            // TODO: Handle command logic here
            this.Delete();
        }
        #endregion

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
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())))//Search All
                {
                    Expression<Func<base_CountStock, bool>> predicate = PredicateBuilder.True<base_CountStock>();
                    this.LoadCountSheet(predicate, false, 0);
                }
                else if (param != null)
                {
                    this.Keyword = param.ToString();
                    Expression<Func<base_CountStock, bool>> predicate = this.CreateSearchPredicate(this.Keyword);
                    this.LoadCountSheet(predicate, false, 0);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        private bool IsDateTime()
        {
            try
            {
                DateTime dateTime = DateTime.Parse(this.Keyword);
                return (!dateTime.ToString().Equals("1/1/0001") && this.Keyword.Contains("/") && this.Keyword.Length >= 8);
            }
            catch
            {
                return false;
            }

        }
        #endregion

        #region DoubleClickCommand

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
            this.IsSetAllQuantity = false;
            this.SelectedStore = null;
            if (param != null && this.IsSearchMode)
            {
                this.IsSearchMode = false;
                this.SetCountSheetDetail();
            }
            else if (this.IsSearchMode)
                //To show detail form.
                this.IsSearchMode = false;
            else if (this.ShowNotification(null))
            {
                // Show list form
                this.IsSearchMode = true;
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
            Expression<Func<base_CountStock, bool>> predicate = PredicateBuilder.True<base_CountStock>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadCountSheet(predicate, false, this.CurrentPageIndex);
        }
        #endregion

        #region FilterProductCommand
        /// <summary>
        /// Method use to filter product.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnFilterProductCommandCanExecute(object param)
        {
            return true;
        }
        /// <summary>
        /// Method to invoke when the LoadStep command is executed.
        /// </summary>
        private void OnFilterProductCommandExecute(object param)
        {
            bool isLoad = false;
        }
        #endregion

        #region ApplyCommand
        /// <summary>
        /// Method to check whether the ApplyCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnApplyCommandCanExecute()
        {
            return (this.SelectedCountStock != null && !this.SelectedCountStock.IsDirty && !this.SelectedCountStock.IsNew && this.SelectedCountStock.Status == 2 && this.SelectedCountStock.HasValue);
        }
        /// <summary>
        /// Method to invoke when the ApplyCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute()
        {
            //To close product view
            if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text21, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string content = string.Format(Language.Text22);
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(content, Language.Information, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msgResult.Is(MessageBoxResult.Yes))
                this.Apply();
        }
        #endregion

        #region DeleteCountStockDetailCommand
        /// <summary>
        /// Gets the DeleteSaleOrderDetail Command.
        /// <summary>
        public RelayCommand<object> DeleteCountStockDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteSaleOrderDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderDetailCommandCanExecute(object param)
        {
            if (param == null) return false;
            if (this.SelectedCountStock != null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteSaleOrderDetail command is executed.
        /// </summary>
        private void OnDeleteSaleOrderDetailCommandExecute(object param)
        {
            if (param != null)
            {
                base_CountStockDetailModel countStockDetailModel = param as base_CountStockDetailModel;
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Information, MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.Yes))
                {
                    this.SelectedCountStock.IsDirty = true;
                    this.SelectedCountStock.IsChangeProductCollection = true;
                    this.SelectedCountStock.CountStockDetailCollection.Remove(countStockDetailModel);
                    this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count();
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods

        #region InitialData
        //To get data when user opens this view.
        private void InitialData()
        {
            this.LoadStore();
            this.NotePopupCollection = new ObservableCollection<PopupContainer>();
            this.NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };
        }
        #endregion

        #region SearchPricing
        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_CountStock, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_CountStock, bool>> predicate = PredicateBuilder.True<base_CountStock>();
            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Set conditions for predicate
                predicate = PredicateBuilder.False<base_CountStock>();
                //To search with DocumentNo.
                if (this.ColumnCollection.Contains(SearchOptions.AccountNum.ToString()))
                    predicate = predicate.Or(x => x.DocumentNo.ToLower().Contains(keyword.ToLower()));
                //To search with Status.
                if (this.ColumnCollection.Contains(SearchOptions.Status.ToString()))
                {
                    IEnumerable<int> query = Common.CountStockStatus.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => Int32.Parse((x.Value).ToString()));
                    predicate = predicate.Or(x => query.Contains(x.Status));
                }
                //To search with Date .
                DateTime date;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    int year = date.Year;
                    int month = date.Month;
                    int day = date.Day;
                    //To search with Date Created.
                    if (this.ColumnCollection.Contains(SearchOptions.StartDate.ToString()))
                        predicate = predicate.Or(x => x.DateCreated.Year == year && x.DateCreated.Month == month && x.DateCreated.Day == day);
                    //To search with Date Completed.
                    if (this.ColumnCollection.Contains(SearchOptions.CompleteDate.ToString()))
                        predicate = predicate.Or(x => x.CompletedDate.HasValue && x.CompletedDate.Value.Year == year && x.CompletedDate.Value.Month == month && x.CompletedDate.Value.Day == day);
                }
                //To search with Counted by.
                if (this.ColumnCollection.Contains(SearchOptions.Counted.ToString()))
                    predicate = predicate.Or(x => x.UserCounted.ToLower().Contains(keyword.ToLower()));
            }
            return predicate;
        }

        private Expression<Func<base_ProductStore, bool>> CreateProductPredicate(int storeId)
        {
            // Initial predicate
            Expression<Func<base_ProductStore, bool>> predicate = PredicateBuilder.True<base_ProductStore>();
            // Set conditions for predicate
            predicate = predicate.And(x => x.StoreCode == storeId);
            return predicate;
        }
        #endregion

        #region InitialCommand
        /// <summary>
        /// To initialize commands. 
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            this.NewCommand = new RelayCommand<object>(this.OnNewCommandExecute, this.OnNewCommandCanExecute);
            this.EditCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnEditCommandCanExecute);
            this.SaveCommand = new RelayCommand(this.OnSaveCommandExecute, this.OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(this.OnDeleteCommandExecute, this.OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(this.OnSearchCommandExecute, this.OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnDoubleClickViewCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
            this.FilterProductCommand = new RelayCommand<object>(this.OnFilterProductCommandExecute, this.OnFilterProductCommandCanExecute);
            this.ApplyCommand = new RelayCommand(this.OnApplyCommandExecute, this.OnApplyCommandCanExecute);
            this.DeleteCountStockDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
        }
        #endregion

        #region LoadProduct
        //To load product.
        private void LoadProduct()
        {
            try
            {
                this.ProductCollection.Clear();
                IEnumerable<base_Product> _products = _productRepository.GetIEnumerable();
                foreach (var item in _products)
                {
                    base_ProductModel _productModel = new base_ProductModel(item);
                    this.ProductCollection.Add(_productModel);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadProduct" + ex.ToString());
            }
        }
        #endregion

        #region LoadCountSheet
        //To load count sheet form DB.
        private void LoadCountSheet(Expression<Func<base_CountStock, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                this.CountStockCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                base.IsBusy = true;
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * NumberOfDisplayItems;
                try
                {
                    IList<base_CountStock> countStocks = _countStockRepository.GetRangeDescending(indexItem, NumberOfDisplayItems, x => x.DateCreated, predicate);
                    foreach (var item in countStocks)
                        bgWorker.ReportProgress(0, item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw;
                }
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                //To add item.
                base_CountStockModel model = new base_CountStockModel(e.UserState as base_CountStock);
                if (model.base_CountStock.base_CountStockDetail != null
                    && model.base_CountStock.base_CountStockDetail.Count(x => x.Difference == null || x.CountedQuantity == null) > 0)
                    model.HasValue = false;
                if (model.Status == 3)
                    model.IsEnable = false;
                this.CountStockCollection.Add(model);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (this._selectedCountStockClone != null && !this.IsSearchMode)
                {
                    this.SelectedCountStock = this.CountStockCollection.SingleOrDefault(x => x.Id == this._selectedCountStockClone.Id);
                    this.SetCountSheetDetail();
                    this._selectedCountStockClone = null;
                }
                //To count all User in Data base show on grid
                this.TotalCountSheet = _countStockRepository.GetIQueryable().Count();
                base.IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }
        #endregion

        #region LoadProductWithStore
        //To load product with store from DB.
        private void LoadProductWithStore(Expression<Func<base_ProductStore, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            AutoResetEvent doneEvent = null;
            if (_requireWaitingLoadStore)
                doneEvent = new AutoResetEvent(false);
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                this.SelectedCountStock.CountStockDetailCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                
                //if(refreshData)
                //To add item.
                short itemTypeGroup = (short)ItemTypes.Group;
                short itemTypeServices = (short)ItemTypes.Services;
                short itemInsurance = (short)ItemTypes.Insurance;
                IEnumerable<base_ProductStore> productStores = _productStoreRepository.GetAll(predicate).Where(x => x.base_Product.ItemTypeId != itemTypeGroup && x.base_Product.ItemTypeId != itemTypeServices && x.base_Product.ItemTypeId != itemInsurance).OrderBy(x => x.StoreCode);
                foreach (var productStore in productStores)
                    bgWorker.ReportProgress(0, productStore);
                if (_requireWaitingLoadStore)
                    doneEvent.Set();
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_ProductStore productStore = e.UserState as base_ProductStore;
                base_CountStockDetailModel model = new base_CountStockDetailModel();
                model.ProductId = productStore.base_Product.Id;
                model.ProductCode = productStore.base_Product.Code;
                model.ProductResource = productStore.base_Product.Resource.ToString();
                model.StoreId = Int16.Parse(productStore.StoreCode.ToString());
                model.Quantity = productStore.QuantityOnHand;
                model.Attribute = productStore.base_Product.Attribute;
                model.Size = productStore.base_Product.Size;
                model.Description = productStore.base_Product.Description;
                model.ProductName = productStore.base_Product.ProductName;
                model.Difference = null;
                model.IsCounted = false;
                model.EndUpdate();
                model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                this.SelectedCountStock.CountStockDetailCollection.Add(model);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                
                //To count all User in Data base show on grid
                this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count;
            };
            bgWorker.RunWorkerAsync();

            if (_requireWaitingLoadStore && doneEvent!=null)
                doneEvent.WaitOne();
        }
        #endregion

        #region Model_PropertyChanged
        //Event PropertyChanged of CountStockModel
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!this.SelectedCountStock.IsLoad)
                switch (e.PropertyName)
                {
                    case "CountedQty":
                        base_CountStockDetailModel model = sender as base_CountStockDetailModel;
                        if (model.CountedQty.HasValue)
                        {
                            model.Difference = model.CountedQty - model.Quantity;
                            model.IsCounted = true;
                        }
                        model.CountedQuantity = model.CountedQty;
                        this.SelectedCountStock.IsDirty = true;
                        this.SelectedCountStock.IsChangeProductCollection = true;
                        break;
                }
        }
        #endregion

        #region LoadStore
        //To get data from DB.
        private void LoadStore()
        {
            //To get data from store table.
            this.StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
        }
        #endregion

        #region ChangeViewExecute
        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.SelectedCountStock != null && this.SelectedCountStock.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Information, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Cancel))
                    return false;
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (this.OnSaveCommandCanExecute())
                    {
                        this.OnSaveCommandExecute();
                        result = true;
                    }
                    else //Has Error
                        result = false;

                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (this.SelectedCountStock.IsNew)
                    {
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                        this.SelectedCountStock = null;
                        this._selectedCountStockClone = this.SelectedCountStock;
                    }
                    else //Old Item Rollback data
                    {
                        this.RollBackCountSheet();
                        this._selectedCountStockClone = this.SelectedCountStock;
                    }
                }
            }
            else
            {
                if (this.SelectedCountStock != null && this.SelectedCountStock.IsNew)
                    this.IsSearchMode = true;
                else
                {
                    if (this.SelectedCountStock != null && !this.SelectedCountStock.IsNew && !this.IsSearchMode)
                    {
                        this._selectedCountStockClone = null;
                        this._selectedCountStockClone = this.SelectedCountStock;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Insert
        /// <summary>
        /// To insert data into base_PricingManager and base_PricingChange table.
        /// </summary>
        private void Insert()
        {
            try
            {
                //MessageBoxResult msgResult = MessageBoxResult.None;
                //msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text23, Language.Warning, MessageBoxButton.YesNo);
                //if (msgResult.Is(MessageBoxResult.Yes))
                //{
                this.SelectedCountStock.UserCreated = Define.USER.LoginName;
                this.SelectedCountStock.DateCreated = DateTime.Now;
                this.SelectedCountStock.Status = 2;
                this.SelectedCountStock.ToEntity();
                foreach (var item in this.SelectedCountStock.CountStockDetailCollection)
                {
                    item.CountedQuantity = item.CountedQty;
                    item.ToEntity();
                    this.SelectedCountStock.base_CountStock.base_CountStockDetail.Add(item.base_CountStockDetail);
                    item.IsCounted = true;
                    item.EndUpdate();
                }
                if (this.SelectedCountStock.CountStockDetailCollection.Count(x => !x.Difference.HasValue) > 0)
                    this.SelectedCountStock.HasValue = false;
                else
                    this.SelectedCountStock.HasValue = true;
                this._countStockRepository.Add(this.SelectedCountStock.base_CountStock);
                this._countStockRepository.Commit();
                this.SelectedCountStock.Id = this.SelectedCountStock.base_CountStock.Id;
                this.CountStockCollection.Insert(0, this.SelectedCountStock);
                this.SelectedCountStock.EndUpdate();
                this.SelectedCountStock.IsChangeProductCollection = false;
                App.WriteUserLog("CountStock", "User counted a stock." + this.SelectedCountStock.Id);
                this.TotalCountSheet = this.CountStockCollection.Count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Insert" + ex.ToString());
            }
        }
        #endregion

        #region Apply
        /// <summary>
        /// //To update price on Product table.
        /// </summary>
        private void Apply()
        {
            try
            {
                this.SelectedCountStock.Shift = Define.ShiftCode;
                this.SelectedCountStock.UserCounted = Define.USER.LoginName;
                this.SelectedCountStock.CompletedDate = DateTimeExt.Now;
                this.SelectedCountStock.Status = 3;
                this.SelectedCountStock.ToEntity();
                foreach (var item in this.SelectedCountStock.base_CountStock.base_CountStockDetail)
                {
                    this._productRepository.UpdateOnHandQuantity(item.ProductResource, item.StoreId, item.Quantity, true);
                    this._productRepository.UpdateOnHandQuantity(item.ProductResource, item.StoreId, item.CountedQuantity.Value);
                }
                this._pricingManagerRepository.Commit();
                this.SelectedCountStock.EndUpdate();
                this.SelectedCountStock.IsEnable = false;
                this.SelectedCountStock.IsLoad = false;
                this.SelectedCountStock.IsChangeProductCollection = false;
                App.WriteUserLog("CountStock", "User applied stock count." + this.SelectedCountStock.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update" + ex.ToString());
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// To update IsLock on base_ResoureceAccount table.
        /// </summary>
        private void Delete()
        {
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to lock this account?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Delete" + ex.ToString());
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// To update data when user change value of control on view.
        /// </summary>
        private void Update()
        {
            try
            {
                //MessageBoxResult msgResult = MessageBoxResult.None;
                //msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text23, Language.Warning, MessageBoxButton.YesNo);
                //if (msgResult.Is(MessageBoxResult.Yes))
                //{
                this.SelectedCountStock.Shift = Define.ShiftCode;
                this.SelectedCountStock.Status = 2;
                this.SelectedCountStock.ToEntity();
                if (this.SelectedCountStock.IsChangeProductCollection)
                {
                    _countStockDetailRepository.Delete(this.SelectedCountStock.base_CountStock.base_CountStockDetail);
                    this.SelectedCountStock.base_CountStock.base_CountStockDetail.Clear();
                    _pricingChangeRepository.Commit();
                    foreach (var item in this.SelectedCountStock.CountStockDetailCollection)
                    {
                        item.CountedQuantity = item.CountedQty;
                        item.ToEntity();
                        this.SelectedCountStock.base_CountStock.base_CountStockDetail.Add(item.base_CountStockDetail);
                        item.IsCounted = true;
                        item.EndUpdate();
                        App.WriteUserLog("CountStock", "User update stock count." + this.SelectedCountStock.Id);
                    }
                }
                if (this.SelectedCountStock.CountStockDetailCollection.Count(x => !x.Difference.HasValue) > 0)
                    this.SelectedCountStock.HasValue = false;
                else
                    this.SelectedCountStock.HasValue = true;
                this._pricingManagerRepository.Commit();
                this.SelectedCountStock.EndUpdate();
                this.SelectedCountStock.IsChangeProductCollection = false;
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update" + ex.ToString());
            }
        }
        #endregion

        #region IsEditData
        private bool IsEditData()
        {
            if (this.SelectedCountStock == null)
                return false;

            return (this.SelectedCountStock.IsDirty);
        }
        #endregion

        #region SetIsCheckedUserRight
        /// <summary>
        /// To set IsChecked on UserRight DataGrid.
        /// </summary>
        private void SetIsCheckedUserRight()
        {

        }
        #endregion

        #region SetDefaultValue
        /// <summary>
        /// To set default value for fields.
        /// </summary>
        private void SetDefaultValue()
        {

        }
        #endregion

        #region RollBackData
        /// <summary>
        /// To rollback data when user click Cancel.
        /// </summary>
        private void RollBackData(bool isChangeItem)
        {

        }
        #endregion

        #region VisibilityData
        /// <summary>
        ///To show item when user check into Check Box.
        /// </summary>
        private void SetVisibilityData(bool value)
        {

        }
        #endregion

        #region ChangeCurrentPrice
        //To changecurrent price when user change list product.
        private void ChangeCurrentPrice()
        {

        }
        #endregion

        #region ChangeDataOfProduct
        /// <summary>
        /// To change price of product.
        /// </summary>
        /// <param name="basePriceID"></param>
        private void ChangeDataOfProduct(int basePriceID)
        {

        }
        #endregion

        #region CalculationData
        /// <summary>
        /// To calculate price of product.
        /// </summary>
        /// <param name="calculationType"></param>
        /// <param name="number1"></param>
        /// <param name="amountValue"></param>
        /// <param name="amountUnit"></param>
        /// <returns></returns>
        private decimal CalculationData(short calculationType, decimal number1, decimal amountValue, short amountUnit)
        {
            decimal number2 = 0;
            //To get Type of Amount ( % or $ ) 
            if (amountUnit == 2)
                number2 = amountValue;
            else
                number2 = number1 * (amountValue / 100);
            if (number2 == 0) return 0;
            //To get calulation.
            if (calculationType == 1) // Plus
                return number1 + number2;
            else if (calculationType == 2)
                return number1 - number2; //Subtract
            else if (calculationType == 3)
                return number1 * number2; //Multiple 
            else
                return number1 / number2;//Divide
        }
        #endregion

        #region ReturnPrice
        /// <summary>
        /// To return type of price 
        /// </summary>
        /// <param name="priceLevel">it is string of priceLevel.</param>
        /// <param name="product">It is a item of product.</param>
        /// <returns></returns>
        private decimal ReturnPrice(string priceLevel, base_ProductModel product)
        {

            return 0;
        }
        #endregion

        #region SetProductPrice
        /// <summary>
        /// To set type of price 
        /// </summary>
        /// <param name="priceLevel">it is string of priceLevel.</param>
        /// <param name="product">It is a item of product.</param>
        /// <returns></returns>
        private void SetProductPrice(base_Product product, base_ProductModel productModel)
        {

        }
        #endregion

        #region RestoreProductPrice
        /// <summary>
        /// To restore type of price 
        /// </summary>
        /// <param name="priceLevel">it is string of priceLevel.</param>
        /// <param name="product">It is a item of product.</param>
        /// <returns></returns>
        private void RestoreProductPrice(base_Product product, base_PricingChange pricingChange)
        {

        }
        #endregion

        #region IsActiveChangData
        /// <summary>
        /// To check condition to execute method..
        /// </summary>
        /// <returns></returns>
        private bool IsActiveChangData()
        {
            return true;
        }
        #endregion

        #region ShowNotification
        //To show notification when user close or change  this view.
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;
            // Check data is edited
            if (this.IsEditData())
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Cancel))
                    return false;
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (this.OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        this.OnSaveCommandExecute();
                        result = true;
                    }
                    else
                        result = false;
                }
                else
                {
                    if (this.SelectedCountStock.IsNew)
                    {
                        //SelectedProduct = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                        this.SelectedCountStock = null;
                        this._selectedCountStockClone = this.SelectedCountStock;
                    }
                    else
                    {
                        //Remove popup note
                        this.RollBackCountSheet();
                    }
                    this.SelectedStore = null;
                }
            }
            return result;
        }
        #endregion

        #region RollBackCountSheet
        //To return data when users dont save them.
        private void RollBackCountSheet()
        {
            this.SelectedCountStock.ToModelAndRaise();
            this.SelectedCountStock.IsChangeProductCollection = false;
            this.SelectedCountStock.EndUpdate();
        }
        #endregion

        #region SetCountSheetDetail
        /// <summary>
        /// To set data for CountSheetDetail.
        /// </summary>
        private void SetCountSheetDetail()
        {
            if (this.SelectedCountStock != null)
            {
                this.SelectedCountStock.IsLoad = true;
                //To load CountSheetDetail.
                this.SelectedCountStock.CountStockDetailCollection = new ObservableCollection<base_CountStockDetailModel>();
                foreach (var productStore in this.SelectedCountStock.base_CountStock.base_CountStockDetail)
                {
                    base_Product product = _productRepository.GetIQueryable(x => x.Id == productStore.ProductId).SingleOrDefault();
                    if (product != null)
                    {
                        base_CountStockDetailModel model = new base_CountStockDetailModel(productStore);
                        model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                        model.Attribute = product.Attribute;
                        model.Size = product.Size;
                        model.CountedQty = model.CountedQuantity;
                        model.Description = product.Description;
                        model.ProductName = product.ProductName;
                        model.ProductCode = product.Code;
                        model.IsCounted = true;
                        model.EndUpdate();
                        this.SelectedCountStock.CountStockDetailCollection.Add(model);
                    }
                }
                if (this.SelectedCountStock.CountStockDetailCollection.Count(x => !x.Difference.HasValue) > 0)
                    this.SelectedCountStock.HasValue = false;
                this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count;
                this.SelectedCountStock.IsChangeProductCollection = false;
                this.SelectedCountStock.IsLoad = false;
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
            return ChangeViewExecute(isClosing);
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            this.CountStockCollection.Clear();
            Expression<Func<base_CountStock, bool>> predicate = PredicateBuilder.True<base_CountStock>();
            if (!string.IsNullOrWhiteSpace(Keyword))
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadCountSheet(predicate, true);
        }
        #endregion

        #region ChangeSearchMode
        /// <summary>
        /// ChangeSearchMode
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (this.ChangeViewExecute(null))
                {
                    if (!isList)
                    {
                        this.SelectedIndex = -1;
                        this.OnNewCommandExecute(null);
                        this.IsSearchMode = false;
                    }
                    else
                        this.IsSearchMode = true;
                }
            }
        }
        #endregion

        #endregion
    }
}