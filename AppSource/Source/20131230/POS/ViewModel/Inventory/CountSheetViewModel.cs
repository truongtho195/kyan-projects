using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Threading;
using CPC.DragDrop;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class CountSheetViewModel : ViewModelBase, IDropTarget
    {
        #region Define
        //To define command to use it in class.
        public RelayCommand<object> NewCommand { get; private set; }
        public RelayCommand<object> EditCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand<object> ApplyCommand { get; private set; }
        public RelayCommand RestoreCommand { get; private set; }
        //To define repository to use it in class.
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_CountStockRepository _countStockRepository = new base_CountStockRepository();
        private base_CountStockDetailRepository _countStockDetailRepository = new base_CountStockDetailRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_GuestRepository _vendorRepository = new base_GuestRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_ProductStoreRepository _productStoreRepository = new base_ProductStoreRepository();
        //To get type of affectpricing.
        private string _typeAffectPricing = string.Empty;

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        private bool _isTransferFromProduct = false;
        private object _productCloneCollection = null;
        #endregion

        #region Constructors

        public CountSheetViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            this.InitialCommand();
            this.InitialData();

            //Initial Auto Complete Search
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
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
                    ResetTimer();
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
                    if (this.SelectedStore != null && this.SelectedCountStock.CountStockDetailCollection != null
                       && (this.SelectedCountStock.CountStockDetailCollection.Count == 0 || (this.SelectedCountStock.CountStockDetailCollection.Count > 0 && this.SelectedCountStock.CountStockDetailCollection.First().StoreId != this.StoreCollection.IndexOf(this.SelectedStore))))
                        this.IsChangeStore = true;
                    else
                        this.IsChangeStore = false;
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

        #region IsChangeStore
        protected bool _isChangeStore;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsChangeStore</para>
        /// </summary>
        public bool IsChangeStore
        {
            get { return this._isChangeStore; }
            set
            {
                if (this._isChangeStore != value)
                {
                    this._isChangeStore = value;
                    OnPropertyChanged(() => IsChangeStore);

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
                if (this.StoreCollection != null)
                    this.SelectedStore = this.StoreCollection.ElementAt(Define.StoreCode);
                this.SelectedCountStock.IsDirty = false;
                this.SelectedCountStock.IsLoad = false;
                this.SelectedCountStock.IsChangeProductCollection = false;
                this.SelectedCountStock.CountStockDetailCollection = new CollectionBase<base_CountStockDetailModel>();
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
            return (param == null || (param is ObservableCollection<object> && ((param as ObservableCollection<object>).Count == 0 || (param as ObservableCollection<object>).Count > 1))) ? false : true;
        }

        private void OnEditCommandExecute(object param)
        {
            try
            {
                this.IsSetAllQuantity = false;
                this.SelectedStore = null;
                this.SelectedCountStock = (param as ObservableCollection<object>)[0] as base_CountStockModel;
                //this.SetCountSheetDetail();
                this.SelectedCountStock.CountStockDetailCollection = new CollectionBase<base_CountStockDetailModel>();
                this.SelectedCountStock.IsChangeProductCollection = false;
                this.LoadSteepCountDetail();
                this.SelectedCountStock.EndUpdate();
                //To set enable of detail grid.
                this.IsSearchMode = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("OnDuplicateCommandExecute" + ex.ToString());
            }

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
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                bgWorker.DoWork += (sender, e) =>
                {
                    if (Define.DisplayLoading)
                        IsBusy = true;
                    this.SelectedCountStock.Shift = Define.ShiftCode;
                    // TODO: Handle command logic here
                    if (this.SelectedCountStock.IsNew)
                        this.Insert();
                    else
                        this.Update();
                    bgWorker.ReportProgress(0, "save");
                };
                bgWorker.ProgressChanged += (sender, e) =>
                {
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    base.IsBusy = false;
                    if (this.SelectedCountStock.IsNew)
                        this.CountStockCollection.Insert(0, this.SelectedCountStock);
                    this.SelectedCountStock.ToModelAndRaise();
                    this.SelectedCountStock.EndUpdate();
                    this.SelectedCountStock.IsChangeProductCollection = false;
                    this.TotalCountSheet = this.CountStockCollection.Count;
                    this.IsChangeStore = false;
                };
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine(ex);
            }
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
                if (_waitingTimer != null)
                    _waitingTimer.Stop();
                // TODO: Handle command logic here
                this.SearchAlert = string.Empty;
                if (string.IsNullOrWhiteSpace(Keyword))//Search All
                {
                    Expression<Func<base_CountStock, bool>> predicate = PredicateBuilder.True<base_CountStock>();
                    this.LoadCountSheet(predicate, false, 0);
                }
                else
                {
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
                this.SelectedCountStock = param as base_CountStockModel;
                //this.SetCountSheetDetail();
                this.SelectedCountStock.CountStockDetailCollection = new CollectionBase<base_CountStockDetailModel>();
                this.SelectedCountStock.IsChangeProductCollection = false;
                this.LoadSteepCountDetail();
                this.SelectedCountStock.EndUpdate();
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

        #region LoadProductByStepCommand

        public RelayCommand<object> LoadProductByStepCommand { get; private set; }
        /// <summary>
        /// Method to check whether the LoadStep command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadProductStepCommandCanExecute(object param)
        {
            return false;
        }
        /// <summary>
        /// Method to invoke when the LoadStep command is executed.
        /// </summary>
        private void OnLoadProductStepCommandExecute(object param)
        {
            this.LoadSteepCountDetail();
        }
        #endregion

        #region ApplyCommand
        /// <summary>
        /// Method to check whether the ApplyCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnApplyCommandCanExecute(object param)
        {
            if (this.IsValid && param != null
               && param is ObservableCollection<object>
               && (param as ObservableCollection<object>).Count == 1)
            {
                base_CountStockModel model = (param as ObservableCollection<object>)[0] as base_CountStockModel;
                if (model != null && !model.IsDirty && !model.IsNew && model.Status == 2 && model.HasValue)
                    return true;
            }
            return (this.SelectedCountStock != null && !this.SelectedCountStock.IsDirty && !this.SelectedCountStock.IsNew && this.SelectedCountStock.Status == 2 && this.SelectedCountStock.HasValue);
        }
        /// <summary>
        /// Method to invoke when the ApplyCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute(object param)
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
            {
                //To apply that change product.
                if (param != null && param is ObservableCollection<object>)
                    this.SelectedCountStock = (param as ObservableCollection<object>)[0] as base_CountStockModel;
                this.Apply();
            }
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

        #region SearchProductAdvanceCommand
        /// <summary>
        /// Gets the SearchProductAdvance Command.
        /// <summary>
        public RelayCommand<object> SearchProductAdvanceCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the SearchProductAdvance command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchProductAdvanceCommandCanExecute(object param)
        {
            return this.SelectedStore != null && this.SelectedCountStock != null && this.SelectedCountStock.CountStockDetailCollection != null && this.SelectedCountStock.Status < 3;
        }
        /// <summary>
        /// Method to invoke when the SearchProductAdvance command is executed.
        /// </summary>
        private void OnSearchProductAdvanceCommandExecute(object param)
        {
            try
            {
                if (this.IsChangeStore)
                {
                    this.IsChangeStore = false;
                    this.SelectedCountStock.CountStockDetailCollection.Clear();
                    this.TotalProducts = 0;
                    this.SelectedCountStock.IsChangeProductCollection = true;
                }
                int storeIndex = this.StoreCollection.IndexOf(this.SelectedStore);
                ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel(false, false, false, false, false, false, storeIndex);
                bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, "Count Sheet");
                string temp = string.Empty;
                if (dialogResult == true)
                {
                    foreach (var item in productSearchViewModel.SelectedProducts)
                    {
                        string productResource = item.Resource.ToString();
                        if (this.SelectedCountStock.CountStockDetailCollection.Count == 0
                            || this.SelectedCountStock.CountStockDetailCollection.Count(x => x.ProductResource == productResource) == 0)
                        {
                            base_CountStockDetailModel model = new base_CountStockDetailModel();
                            model.ProductId = item.Id;
                            model.ProductCode = item.Code;
                            model.ProductResource = item.Resource.ToString();
                            model.StoreId = Int16.Parse(storeIndex.ToString());
                            model.Quantity = item.QuantityOnHand;
                            model.Attribute = item.Attribute;
                            model.Size = item.Size;
                            model.Description = item.Description;
                            model.ProductName = item.ProductName;
                            if (this.IsSetAllQuantity)
                            {
                                model.CountedQty = 0;
                                model.IsSetCounted = true;
                            }
                            else
                            {
                                model.Difference = null;
                                model.CountedQty = null;
                            }
                            model.IsCounted = false;
                            model.EndUpdate();
                            model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                            //To add item.
                            this.SelectedCountStock.IsDirty = true;
                            this.SelectedCountStock.CountStockDetailCollection.Add(model);
                            this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count();
                        }
                        else
                        {
                            temp += string.Format("{0},", item.ProductName);
                        }
                    }
                    if (temp.Length > 0)
                    {
                        temp = temp.Remove(temp.Length - 1);
                        Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("{0} {1}", temp, " have(has) existed !"), "Count Sheet");
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString());
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
        }
        #endregion

        #region SearchCountSheet
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
            short itemTypeGroup = (short)ItemTypes.Group;
            short itemTypeServices = (short)ItemTypes.Services;
            short itemInsurance = (short)ItemTypes.Insurance;
            predicate = predicate.And(x => x.StoreCode == storeId);
            predicate = predicate.And(x => x.base_Product.ItemTypeId != itemTypeGroup
                  && x.base_Product.ItemTypeId != itemTypeServices
                  && x.base_Product.ItemTypeId != itemInsurance);
            //predicate = predicate.And(x => x.base_Product.IsPurge.HasValue && !x.base_Product.IsPurge.Value);
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
            this.EditCommand = new RelayCommand<object>(this.OnEditCommandExecute, this.OnEditCommandCanExecute);
            this.SaveCommand = new RelayCommand(this.OnSaveCommandExecute, this.OnSaveCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(this.OnSearchCommandExecute, this.OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnDoubleClickViewCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
            this.ApplyCommand = new RelayCommand<object>(this.OnApplyCommandExecute, this.OnApplyCommandCanExecute);
            this.DeleteCountStockDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
            this.LoadProductByStepCommand = new RelayCommand<object>(this.OnLoadProductStepCommandExecute, this.OnLoadProductStepCommandCanExecute);
            this.SearchProductAdvanceCommand = new RelayCommand<object>(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
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
                if (Define.DisplayLoading)
                    IsBusy = true;
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
                    _log4net.Error(ex);
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
                //To count sheet from product.
                if (this._isTransferFromProduct && this._productCloneCollection != null)
                {
                    this.CountSheetFromProduct(_productCloneCollection as IEnumerable<base_ProductModel>);
                    this._isTransferFromProduct = false;
                    this._productCloneCollection = null;
                }

                //To count all User in Data base show on grid
                this.TotalCountSheet = _countStockRepository.GetIQueryable().Count();
                base.IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
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
                        this.SelectedCountStock.IsChangeQty = true;
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
                    }
                    else //Old Item Rollback data
                    {
                        this.RollBackCountSheet();
                    }
                }
            }
            else
            {
                if (this.SelectedCountStock != null && this.SelectedCountStock.IsNew)
                    this.IsSearchMode = true;
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
                this.SelectedCountStock.UserCreated = Define.USER.LoginName;
                this.SelectedCountStock.DateCreated = DateTime.Now;
                this.SelectedCountStock.Status = 2;
                this.SelectedCountStock.ToEntity();
                //To insert data into database.
                foreach (var item in this.SelectedCountStock.CountStockDetailCollection)
                {
                    item.CountedQuantity = item.CountedQty;
                    item.ToEntity();
                    this.SelectedCountStock.base_CountStock.base_CountStockDetail.Add(item.base_CountStockDetail);
                    item.IsCounted = true;
                    item.EndUpdate();
                }
                //To set Enable of Context Menu when user updates data.
                if (this.SelectedCountStock.CountStockDetailCollection.Count(x => !x.Difference.HasValue) > 0)
                    this.SelectedCountStock.HasValue = false;
                else
                    this.SelectedCountStock.HasValue = true;
                this._countStockRepository.Add(this.SelectedCountStock.base_CountStock);
                this._countStockRepository.Commit();
                this.SelectedCountStock.Id = this.SelectedCountStock.base_CountStock.Id;
                App.WriteUserLog("CountStock", "User counted a stock." + this.SelectedCountStock.Id);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                MessageBox.Show("Insert" + ex.ToString());
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
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                bgWorker.DoWork += (sender, e) =>
                {
                    if (Define.DisplayLoading)
                        IsBusy = true;
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
                        this._countStockRepository.Commit();
                    }
                    catch (Exception ex)
                    {
                        _log4net.Error(ex);
                        MessageBox.Show("Apply", ex.ToString());
                    }
                    bgWorker.ReportProgress(0, "Apply");
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {

                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    base.IsBusy = false;
                    this.SelectedCountStock.EndUpdate();
                    this.SelectedCountStock.IsEnable = false;
                    this.SelectedCountStock.IsLoad = false;
                    this.IsChangeStore = false;
                    this.SelectedCountStock.IsChangeProductCollection = false;
                    App.WriteUserLog("CountStock", "User applied stock count." + this.SelectedCountStock.Id);
                };
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("Update" + ex.ToString());
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
                this.SelectedCountStock.Status = 2;
                this.SelectedCountStock.ToEntity();
                //To clear data in base_CountStockDetail entity when user changes the store.
                int storeIndex = this.StoreCollection.IndexOf(this.SelectedStore);
                if (storeIndex >= 0 && this.SelectedCountStock.base_CountStock.base_CountStockDetail.First().StoreId != storeIndex)
                {
                    this.IsChangeStore = true;
                    _countStockDetailRepository.Delete(this.SelectedCountStock.base_CountStock.base_CountStockDetail);
                    this.SelectedCountStock.base_CountStock.base_CountStockDetail.Clear();
                }

                //To set data when user deletes data in the datagrid.
                if (this.SelectedCountStock.CountStockDetailCollection.DeletedItems.Count > 0
                    && this.SelectedCountStock.base_CountStock.base_CountStockDetail.Count > 0)
                {
                    foreach (var item in this.SelectedCountStock.CountStockDetailCollection.DeletedItems)
                    {
                        if (_countStockDetailRepository.Get(x => x.Id == item.base_CountStockDetail.Id) != null)
                            _countStockDetailRepository.Delete(item.base_CountStockDetail);
                        this.SelectedCountStock.base_CountStock.base_CountStockDetail.Remove(item.base_CountStockDetail);
                    }
                }

                //To update data in base_CountStockDetail entity when user uses loading step in the datagrid.
                if (this.IsSetAllQuantity && this.SelectedCountStock.base_CountStock.base_CountStockDetail != null)
                {
                    int skipCount = this.SelectedCountStock.CountStockDetailCollection.Count;
                    int takeCount = this.SelectedCountStock.base_CountStock.base_CountStockDetail.Count - skipCount;
                    IEnumerable<base_CountStockDetail> countDetails = this.SelectedCountStock.base_CountStock.base_CountStockDetail.Skip(skipCount).Take(takeCount);
                    foreach (var item in countDetails)
                    {
                        item.CountStockId = this.SelectedCountStock.Id;
                        item.CountedQuantity = 0;
                        item.Difference = item.CountedQuantity - item.Quantity;
                    }
                }

                //To update data in CountStockDetail Collection.
                if (this.SelectedCountStock.IsChangeProductCollection)
                {
                    foreach (var item in this.SelectedCountStock.CountStockDetailCollection)
                    {
                        item.CountStockId = this.SelectedCountStock.Id;
                        item.CountedQuantity = item.CountedQty;
                        item.ToEntity();
                        if (this.IsChangeStore)
                            this.SelectedCountStock.base_CountStock.base_CountStockDetail.Add(item.base_CountStockDetail);
                        item.IsCounted = true;
                        item.EndUpdate();
                        App.WriteUserLog("CountStock", "User update stock count." + this.SelectedCountStock.Id);
                    }
                }
                //To set Enable of Context Menu when user updates data.
                if (this.SelectedCountStock.base_CountStock.base_CountStockDetail.Count(x => !x.Difference.HasValue) > 0)
                    this.SelectedCountStock.HasValue = false;
                else
                    this.SelectedCountStock.HasValue = true;
                //To commit data.
                this._countStockRepository.Commit();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
                this.SelectedCountStock.CountStockDetailCollection = new CollectionBase<base_CountStockDetailModel>();
                foreach (var productStore in this.SelectedCountStock.base_CountStock.base_CountStockDetail)
                {
                    base_Product product = _productRepository.Get(x => x.Id == productStore.ProductId);
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

        #region LoadSteepCountDetail
        private void LoadSteepCountDetail()
        {
            if (this.SelectedCountStock != null
                && this.SelectedCountStock.base_CountStock.base_CountStockDetail != null
                && !this.IsChangeStore)
            {
                //this.SelectedCountStock.base_CountStock.base_CountStockDetail.Skip(100)
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                bgWorker.DoWork += (sender, e) =>
              {
                  if (Define.DisplayLoading)
                      IsBusy = true;
                  //To count all User in Data base show on grid.
                  this.TotalProducts = this.SelectedCountStock.base_CountStock.base_CountStockDetail.Count;
                  //To get data with range.
                  int skipCount = this.SelectedCountStock.CountStockDetailCollection.Count;
                  int takeCount = this.NumberOfDisplayItems;
                  IEnumerable<base_CountStockDetail> countDetails = this.SelectedCountStock.base_CountStock.base_CountStockDetail.Skip(skipCount).Take(takeCount);
                  foreach (var item in countDetails)
                      bgWorker.ReportProgress(0, item);
              };
                bgWorker.ProgressChanged += (sender, e) =>
                {
                    base_CountStockDetail countDetail = e.UserState as base_CountStockDetail;
                    Guid productResource = Guid.Parse(countDetail.ProductResource);
                    base_Product product = _productRepository.Get(x => x.Resource == productResource);
                    if (product != null)
                    {
                        base_CountStockDetailModel model = new base_CountStockDetailModel(countDetail);
                        model.Attribute = product.Attribute;
                        model.Size = product.Size;
                        model.CountedQty = model.CountedQuantity;
                        model.Description = product.Description;
                        model.ProductName = product.ProductName;
                        model.ProductCode = product.Code;
                        model.IsCounted = true;
                        model.EndUpdate();
                        model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                        this.SelectedCountStock.CountStockDetailCollection.Add(model);
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    if (this.SelectedCountStock.CountStockDetailCollection.Count(x => !x.Difference.HasValue) > 0)
                        this.SelectedCountStock.HasValue = false;
                    this.SelectedCountStock.IsLoad = false;
                    if (!this.SelectedCountStock.IsChangeProductCollection)
                        this.SelectedCountStock.EndUpdate();
                    base.IsBusy = false;
                };
                bgWorker.RunWorkerAsync();
            }
        }

        #endregion

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

        #region CountSheetFromProduct
        //To send product counted from ProductView.
        private void CountSheetFromProduct(IEnumerable<base_ProductModel> productCollection)
        {
            //To show detail form.
            this.OnNewCommandExecute(null);
            foreach (var item in productCollection)
            {
                base_CountStockDetailModel model = new base_CountStockDetailModel();
                model.ProductId = item.Id;
                model.ProductCode = item.Code;
                model.ProductResource = item.Resource.ToString();
                model.StoreId = Int16.Parse(Define.StoreCode.ToString());
                model.Quantity = item.QuantityOnHand;
                model.Attribute = item.Attribute;
                model.Size = item.Size;
                model.Description = item.Description;
                model.ProductName = item.ProductName;
                model.IsCounted = false;
                model.EndUpdate();
                model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                //To add item.
                this.SelectedCountStock.IsDirty = true;
                this.SelectedCountStock.CountStockDetailCollection.Add(model);
                this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count();
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
            else
            {
                this._isTransferFromProduct = true;
                this._productCloneCollection = param;
            }
        }
        #endregion

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Count Sheet", dropInfo.Data);
            }
        }

        #endregion
    }
}