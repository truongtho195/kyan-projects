using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;

namespace CPC.POS.ViewModel
{
    class PricingViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        //To define repositories to use them in class.
        private base_PricingManagerRepository _pricingManagerRepository = new base_PricingManagerRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_PricingChangeRepository _pricingChangeRepository = new base_PricingChangeRepository();
        private base_GuestRepository _vendorRepository = new base_GuestRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        //To define commands to use them in class.
        public RelayCommand<object> NewCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand RestoreCommand { get; private set; }
        public RelayCommand<object> FilterProductCommand { get; private set; }
        //To get type of Affectpricing.
        private AffectPricing _affectPricingType;
        private base_PricingManagerModel SelectedItemPricingClone { get; set; }

        private Expression<Func<base_Product, bool>> _productPredicate;

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;
        #endregion

        #region Constructors

        public PricingViewModel()
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

        public PricingViewModel(bool isList, object param = null)
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

        #region SelectedItemPricing

        private base_PricingManagerModel _selectedItemPricing;
        /// <summary>
        /// Gets or sets the SelectedItemPricing.
        /// </summary>
        public base_PricingManagerModel SelectedItemPricing
        {
            get
            {
                return _selectedItemPricing;
            }
            set
            {
                if (_selectedItemPricing != value)
                {
                    _selectedItemPricing = value;
                    this.OnPropertyChanged(() => SelectedItemPricing);
                    if (this.SelectedItemPricing != null)
                        this.SelectedItemPricing.PropertyChanged += new PropertyChangedEventHandler(SelectedItemPricing_PropertyChanged);
                }
            }
        }

        private void SelectedItemPricing_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!SelectedItemPricing.IsLoad)
                switch (e.PropertyName)
                {
                    case "PriceLevel":
                        this.ChangeCurrentPrice();
                        if (!this.SelectedItemPricing.IsLoad && this.IsActiveChangData())
                            this.ChangeDataOfProduct(this.SelectedItemPricing.BasePrice);
                        break;
                    case "BasePrice":
                        if (!this.SelectedItemPricing.IsLoad && this.IsActiveChangData())
                            this.ChangeDataOfProduct(this.SelectedItemPricing.BasePrice);
                        break;
                    case "CalculateMethod":
                        if (!this.SelectedItemPricing.IsLoad && this.IsActiveChangData())
                            this.ChangeDataOfProduct(this.SelectedItemPricing.BasePrice);
                        break;
                    case "AmountChange":
                        if (!this.SelectedItemPricing.IsLoad && this.IsActiveChangData())
                            this.ChangeDataOfProduct(this.SelectedItemPricing.BasePrice);
                        break;
                    case "AmountUnit":
                        if (!this.SelectedItemPricing.IsLoad && this.IsActiveChangData())
                            this.ChangeDataOfProduct(this.SelectedItemPricing.BasePrice);
                        break;
                }
        }

        #endregion

        #region PricingCollection
        /// <summary>
        /// Gets or sets the PricingCollection.
        /// </summary>
        private ObservableCollection<base_PricingManagerModel> _pricingCollection = new ObservableCollection<base_PricingManagerModel>();
        public ObservableCollection<base_PricingManagerModel> PricingCollection
        {
            get
            {
                return _pricingCollection;
            }
            set
            {
                if (_pricingCollection != value)
                {
                    _pricingCollection = value;
                    OnPropertyChanged(() => PricingCollection);
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

        #region TotalPricings
        private int _totalPricings;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalPricings
        {

            get
            {
                return _totalPricings;
            }
            set
            {
                if (_totalPricings != value)
                {
                    _totalPricings = value;
                    OnPropertyChanged(() => TotalPricings);
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

        #region CurrentPagePricingIndex
        /// <summary>
        /// Gets or sets the CurrentPagePricingIndex.
        /// </summary>
        private int _currentPagePricingIndex = 0;
        public int CurrentPagePricingIndex
        {
            get
            {
                return _currentPagePricingIndex;
            }
            set
            {
                _currentPagePricingIndex = value;
                OnPropertyChanged(() => CurrentPagePricingIndex);
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

        #region VendorCollection
        private ObservableCollection<base_GuestModel> _vendorCollection = new ObservableCollection<base_GuestModel>();
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

        #region NotePopupCollection
        /// <summary>
        /// Gets or sets the NotePopupCollection.
        /// </summary>
        public ObservableCollection<PopupContainer> NotePopupCollection { get; set; }

        /// <summary>
        /// Gets the ShowOrHiddenNote
        /// </summary>
        public string ShowOrHiddenNote
        {
            get
            {
                if (this.NotePopupCollection.Count == 0)
                    return "Show Stickies";
                else if (this.NotePopupCollection.Count == this.SelectedItemPricing.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                    return "Hide Stickies";
                else
                    return "Show Stickies";
            }
        }
        #endregion

        #region POSConfig
        private base_ConfigurationModel _posConfig;
        /// <summary>
        /// Gets or sets the POSConfi.
        /// </summary>
        public base_ConfigurationModel POSConfig
        {
            get { return _posConfig; }
            set
            {
                if (_posConfig != value)
                {
                    _posConfig = value;
                    OnPropertyChanged(() => POSConfig);
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

        #region CommissionTypes
        /// <summary>
        /// Gets or sets the CommissionTypes.
        /// </summary>
        public ObservableCollection<ComboItem> _commissionTypes;
        public ObservableCollection<ComboItem> CommissionTypes
        {
            get
            {
                return _commissionTypes;
            }
            set
            {
                if (_commissionTypes != value)
                {
                    _commissionTypes = value;
                    OnPropertyChanged(() => CommissionTypes);
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
            return UserPermissions.AllowAddPricing;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute(object param)
        {

            if (this.ChangeViewExecute(null))
            {
                // TODO: Handle command logic here
                this.TotalProducts = 0;
                this.SelectedItemPricing = new base_PricingManagerModel();
                this.SelectedItemPricing.IsLoad = true;
                this.SelectedItemPricing.Status = EmployeeStatuses.Pending.ToString();
                this.SelectedItemPricing.BasePrice = 1;
                this.SelectedItemPricing.CalculateMethod = 1;
                this.SelectedItemPricing.PriceLevel = CPC.Helper.Common.PriceSchemas.FirstOrDefault().Text;
                this.SelectedItemPricing.AffectPricing = 1;
                this.SelectedItemPricing.AmountChange = 0;
                this.SelectedItemPricing.AmountUnit = 1;
                this.SelectedItemPricing.IsLoad = false;
                this.SelectedItemPricing.Resource = Guid.NewGuid();
                this.SelectedItemPricing.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
                StickyManagementViewModel.SetParentResource(SelectedItemPricing.Resource.ToString(), SelectedItemPricing.ResourceNoteCollection);
                this.SelectedItemPricing.IsDirty = false;
                this.SelectedItemPricing.IsErrorProductCollection = true;
                //To set enable of detail grid.
                this.IsSearchMode = false;
            }
        }

        #endregion

        #region EditCommand

        /// <summary>
        /// Gets the EditCommand command.
        /// </summary>
        public ICommand EditCommand { get; private set; }

        /// <summary>
        /// Method to check whether the EditCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null || dataGridControl.SelectedItems.Count > 1)
                return false;

            base_PricingManagerModel pricingManagerModel = dataGridControl.SelectedItem as base_PricingManagerModel;

            return pricingManagerModel != null && !pricingManagerModel.DateRestored.HasValue;
        }

        /// <summary>
        /// Method to invoke when the EditCommand command is executed.
        /// </summary>
        private void OnEditCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            OnDoubleClickViewCommandExecute(dataGridControl.SelectedItem);
        }

        #endregion

        #region SaveCommand

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return this.IsValid && (this.SelectedItemPricing != null && this.SelectedItemPricing.ProductCollection.Count > 0 && (this.SelectedItemPricing.IsDirty || this.SelectedItemPricing.IsChangeProductCollection));
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            if (SelectedItemPricing.IsNew)
            {
                SaveNew(SelectedItemPricing);
            }
            else
            {
                SaveUpdate(SelectedItemPricing);
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
            if (this.SelectedItemPricing != null)
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
                if (_waitingTimer != null)
                    _waitingTimer.Stop();

                this.SearchAlert = string.Empty;
                if (string.IsNullOrWhiteSpace(Keyword))
                {
                    Expression<Func<base_PricingManager, bool>> predicate = PredicateBuilder.True<base_PricingManager>();
                    this.LoadPricing(predicate, false, 0);
                }
                else
                {
                    Expression<Func<base_PricingManager, bool>> predicate = this.CreateSearchPredicate(this.Keyword);
                    this.LoadPricing(predicate, false, 0);
                }

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
            if (param != null && IsSearchMode)
            {
                try
                {
                    // Create background worker
                    BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

                    SelectedItemPricing = param as base_PricingManagerModel;

                    // Clear product collection
                    SelectedItemPricing.ProductCollection.Clear();

                    bgWorker.DoWork += (sender, e) =>
                    {
                        // Turn on BusyIndicator
                        if (Define.DisplayLoading)
                            IsBusy = true;

                        // Set affect pricing
                        _affectPricingType = (AffectPricing)SelectedItemPricing.AffectPricing;

                        // Get all pricing changes
                        IEnumerable<base_PricingChange> pricingChanges = SelectedItemPricing.base_PricingManager.base_PricingChange.Skip(0).Take(NumberOfDisplayItems);

                        foreach (base_PricingChange pricingChange in pricingChanges)
                        {
                            // Create new product model
                            base_ProductModel productModel = new base_ProductModel(pricingChange.base_Product);

                            productModel.CurrentPrice = pricingChange.CurrentPrice.Value;
                            productModel.NewPrice = pricingChange.NewPrice.Value;
                            productModel.PriceChange = pricingChange.PriceChanged.Value;

                            // Turn off IsDirty & IsNew
                            productModel.EndUpdate();

                            bgWorker.ReportProgress(0, productModel);
                        }
                    };

                    bgWorker.ProgressChanged += (sender, e) =>
                    {
                        // Add to collection
                        SelectedItemPricing.ProductCollection.Add(e.UserState as base_ProductModel);
                    };

                    bgWorker.RunWorkerCompleted += (sender, e) =>
                    {
                        // Update total products
                        TotalProducts = SelectedItemPricing.base_PricingManager.base_PricingChange.Count;

                        // Set visibility of buttons
                        if (SelectedItemPricing.DateApplied.HasValue)
                        {
                            // Hide apply button
                            SelectedItemPricing.VisibilityApplied = Visibility.Collapsed;

                            // Show restore button
                            SelectedItemPricing.VisibilityRestore = Visibility.Visible;
                        }

                        if (SelectedItemPricing.DateApplied.HasValue || SelectedItemPricing.DateRestored.HasValue)
                            SelectedItemPricing.IsEnable = false;

                        SelectedItemPricing.IsErrorProductCollection = false;

                        // Turn off IsDirty & IsNew
                        SelectedItemPricing.EndUpdate();

                        // Load resource note collection
                        LoadResourceNoteCollection(SelectedItemPricing);
                        StickyManagementViewModel.SetParentResource(SelectedItemPricing.Resource.ToString(), SelectedItemPricing.ResourceNoteCollection);

                        // Show detail form
                        IsSearchMode = false;

                        // Turn off BusyIndicator
                        IsBusy = false;
                    };

                    // Run async background worker
                    bgWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                    _log4net.Error("LOAD PRICING DETAIL\n" + ex.ToString());
                }
            }
            else if (IsSearchMode)
            {
                //To show detail form.
                IsSearchMode = false;
            }
            else if (ShowNotification(null))
            {
                // Show list form
                IsSearchMode = true;
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
            Expression<Func<base_PricingManager, bool>> predicate = PredicateBuilder.True<base_PricingManager>();
            if (!string.IsNullOrWhiteSpace(this.Keyword))//Load Step Current With Search Current with Search
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadPricing(predicate, false, this.CurrentPagePricingIndex);
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

            if (!SelectedItemPricing.IsLoad)
            {
                // Clear product collection if affect pricing type changed
                if (!_affectPricingType.Equals((AffectPricing)SelectedItemPricing.AffectPricing))
                {
                    SelectedItemPricing.ProductCollection.Clear();
                    _affectPricingType = (AffectPricing)SelectedItemPricing.AffectPricing;
                }

                // Open popup form by selected affect pricing
                if (AffectPricing.Category.Equals((AffectPricing)SelectedItemPricing.AffectPricing))
                {
                    PricingCategoryViewModel viewModel = new PricingCategoryViewModel();
                    bool? msgResult = _dialogService.ShowDialog<PricingCategoryView>(_ownerViewModel, viewModel, Language.GetMsg("PR_Message_SelectProductByCategory"));
                    if (msgResult.HasValue && msgResult.Value)
                    {
                        isLoad = true;
                        LoadProductCollection(viewModel.CategoryList, AffectPricing.Category);
                    }
                    else
                    {
                        SelectedItemPricing.IsChangeProductCollection = true;
                    }
                }
                else if (AffectPricing.Vendor.Equals((AffectPricing)SelectedItemPricing.AffectPricing))
                {
                    PricingVendorViewModel viewModel = new PricingVendorViewModel();
                    bool? msgResult = _dialogService.ShowDialog<PricingVendorView>(_ownerViewModel, viewModel, Language.GetMsg("PR_Message_SelectProductByVendor"));
                    if (msgResult.HasValue && msgResult.Value)
                    {
                        isLoad = true;
                        LoadProductCollection(viewModel.CategoryList, AffectPricing.Vendor);
                    }
                    else
                    {
                        SelectedItemPricing.IsChangeProductCollection = true;
                    }
                }
                else
                {
                    // Get all categories that contain in department list
                    List<ComboItem> categoryList = new List<ComboItem>(_departmentRepository.
                        GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                        OrderBy(x => x.Name).Where(x => x.LevelId == 1).
                        Select(x => new ComboItem { IntValue = x.Id, Text = x.Name }));

                    PricingCustomViewModel viewModel = new PricingCustomViewModel(categoryList);
                    bool? msgResult = _dialogService.ShowDialog<CustomView>(_ownerViewModel, viewModel, Language.GetMsg("PR_Message_SelectProductByCustom"));
                    if (msgResult.HasValue && msgResult.Value)
                    {
                        isLoad = true;
                        LoadProductCollection(viewModel.ProductIDList, AffectPricing.Custom);
                    }
                    else
                    {
                        SelectedItemPricing.IsChangeProductCollection = true;
                    }
                }

                if (isLoad)
                {
                    ChangeCurrentPrice();
                }

                //To change price of product when user changes affrectpricing.
                if (isLoad && !SelectedItemPricing.IsLoad && IsActiveChangData())
                    ChangeDataOfProduct(SelectedItemPricing.BasePrice);

                TotalProducts = SelectedItemPricing.ProductCollection.Count;
                SelectedItemPricing.IsErrorProductCollection = true;
            }
        }
        #endregion

        #region ApplyCommand

        /// <summary>
        /// Gets the ApplyCommand command.
        /// </summary>
        public ICommand ApplyCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ApplyCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnApplyCommandCanExecute(object param)
        {
            if (param == null)
            {
                return IsValid && (SelectedItemPricing != null && !SelectedItemPricing.IsDirty &&
                    !SelectedItemPricing.IsNew && !SelectedItemPricing.DateApplied.HasValue);
            }
            else
            {
                // Convert param to DataGridControl
                DataGridControl dataGridControl = param as DataGridControl;

                if (dataGridControl == null || dataGridControl.SelectedItems.Count > 1)
                    return false;

                base_PricingManagerModel pricingManagerModel = dataGridControl.SelectedItem as base_PricingManagerModel;

                return IsValid && (pricingManagerModel != null && !pricingManagerModel.IsDirty &&
                    !pricingManagerModel.IsNew && !pricingManagerModel.DateApplied.HasValue);
            }
        }

        /// <summary>
        /// Method to invoke when the ApplyCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute(object param)
        {
            // if product view is opened, show notification to close it
            if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text14, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (param != null)
            {
                // Convert param to DataGridControl
                DataGridControl dataGridControl = param as DataGridControl;

                // Update selected pricing manager
                SelectedItemPricing = dataGridControl.SelectedItem as base_PricingManagerModel;
            }

            SelectedItemPricing.IsEnable = false;

            // Apply pricing changes
            ApplyPricing(SelectedItemPricing);
        }

        #endregion

        #region RestoreCommand

        /// <summary>
        /// Method to check whether the RestoreCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRestoreCommandCanExecute()
        {
            return this.IsValid && (this.SelectedItemPricing != null && !this.SelectedItemPricing.IsDirty && !this.SelectedItemPricing.IsNew && !this.SelectedItemPricing.DateRestored.HasValue);
        }

        /// <summary>
        /// Method to invoke when the RestoreCommand command is executed.
        /// </summary>
        private void OnRestoreCommandExecute()
        {
            //To close product view
            if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text15, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //To apply that restore pricing.
            PromotionReasonViewModel viewModel = new PromotionReasonViewModel(this.SelectedItemPricing.Reason);
            bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PromotionReasonView>(_ownerViewModel, viewModel, "Entry for reason");
            if (msgResult.HasValue)
            {
                if (msgResult.Value)
                {
                    //To close product view
                    (this._ownerViewModel as MainViewModel).CloseItem("Product");
                    this.SelectedItemPricing.Reason = viewModel.ReasonReactive;
                    this.RestorePricing(SelectedItemPricing);
                    this.IsSearchMode = false;
                }
            }

        }
        #endregion

        #region LoadStepProductCommand

        /// <summary>
        /// Gets the LoadStepProductCommand command.
        /// </summary>
        public ICommand LoadStepProductCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadStepProductCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepProductCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStepProductCommand command is executed.
        /// </summary>
        private void OnLoadStepProductCommandExecute()
        {
            // Load data by predicate
            LoadProductDataByPredicate(_productPredicate, false, SelectedItemPricing.ProductCollection.Count);
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitialData()
        {
            StickyManagementViewModel = new PopupStickyViewModel();

            this.NotePopupCollection = new ObservableCollection<PopupContainer>();
            this.NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };

            this.POSConfig = Define.CONFIGURATION;
            this.CommissionTypes = new ObservableCollection<ComboItem>();
            this.CommissionTypes.Add(new ComboItem { Value = 0, Text = "%" });
            this.CommissionTypes.Add(new ComboItem { Value = 1, Text = POSConfig.CurrencySymbol });

            _productPredicate = CreateSearchProductPredicate();
        }

        #region SearchPricing
        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_PricingManager, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_PricingManager, bool>> predicate = PredicateBuilder.True<base_PricingManager>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Set conditions for predicate
                predicate = PredicateBuilder.False<base_PricingManager>();
                //To search with Name.
                if (this.ColumnCollection.Contains(SearchOptions.ItemName.ToString()))
                    predicate = predicate.Or(x => x.Name.ToLower().Contains(keyword.ToLower()));
                //To search with Status. 
                if (this.ColumnCollection.Contains(SearchOptions.Status.ToString()))
                    predicate = predicate.Or(x => x.Status.ToLower().Contains(keyword.ToLower()));
                //To search with Item count.
                if (this.ColumnCollection.Contains(SearchOptions.ItemCount.ToString()))
                {
                    int IntValue = 0;
                    if (int.TryParse(keyword, out IntValue) && IntValue != 0)
                        predicate = predicate.Or(x => x.ItemCount.HasValue && x.ItemCount.Value.Equals(IntValue));
                }
                //To search with Price Level.
                if (this.ColumnCollection.Contains(SearchOptions.PriceLevel.ToString()))
                    predicate = predicate.Or(x => x.PriceLevel.ToLower().Contains(keyword.ToLower()));
                //To search with Base price.
                if (this.ColumnCollection.Contains(SearchOptions.BasePrice.ToString()))
                {
                    IEnumerable<short> basePrice = Common.BasePriceTypes.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                    predicate = predicate.Or(x => basePrice.Contains(x.BasePrice));
                }
                //To search with Adjustment Method.
                if (this.ColumnCollection.Contains(SearchOptions.Adjustment.ToString()))
                {
                    IEnumerable<short> adjustmentType = Common.AdjustmentTypes.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                    predicate = predicate.Or(x => x.CalculateMethod.HasValue && adjustmentType.Contains(x.CalculateMethod.Value));
                }
                //To search with Amount change.
                if (this.ColumnCollection.Contains(SearchOptions.AmountChange.ToString()))
                {
                    decimal amountValue = 0;
                    if (decimal.TryParse(keyword, out amountValue) && amountValue != 0)
                        predicate = predicate.Or(x => x.AmountChange.HasValue && x.AmountChange.Value.Equals(amountValue));
                    //To search with Amount Unit.
                    IEnumerable<int> query = CommissionTypes.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => Int32.Parse((x.Value).ToString()));
                    predicate = predicate.Or(x => x.AmountUnit.HasValue && query.Contains(x.AmountUnit.Value));
                }
                //To search with Date Created.
                DateTime date;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    int year = date.Year;
                    int month = date.Month;
                    int day = date.Day;
                    //To search with Date Created.
                    if (this.ColumnCollection.Contains(SearchOptions.DateCreated.ToString()))
                        predicate = predicate.Or(x => x.DateCreated.HasValue && x.DateCreated.Value.Year == year && x.DateCreated.Value.Month == month && x.DateCreated.Value.Day == day);
                    //To search with Date Applied.
                    if (this.ColumnCollection.Contains(SearchOptions.DateApplied.ToString()))
                        predicate = predicate.Or(x => x.DateApplied.HasValue && x.DateApplied.Value.Year == year && x.DateApplied.Value.Month == month && x.DateApplied.Value.Day == day);
                    //To search with Date Restored.
                    if (this.ColumnCollection.Contains(SearchOptions.DateRestored.ToString()))
                        predicate = predicate.Or(x => x.DateRestored.HasValue && x.DateRestored.Value.Year == year && x.DateRestored.Value.Month == month && x.DateRestored.Value.Day == day);
                }
                //To search with User Created.
                if (this.ColumnCollection.Contains(SearchOptions.UserCreated.ToString()))
                    predicate = predicate.Or(x => x.UserCreated.ToLower().Equals(keyword.ToLower()));
                //To search with User UserRestored.
                if (this.ColumnCollection.Contains(SearchOptions.UserRestored.ToString()))
                    predicate = predicate.Or(x => x.UserRestored.ToLower().Equals(keyword.ToLower()));
                //To search with User UserApplied.
                if (this.ColumnCollection.Contains(SearchOptions.UserApplied.ToString()))
                    predicate = predicate.Or(x => x.UserApplied.ToLower().Equals(keyword.ToLower()));
            }
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
            EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            this.SaveCommand = new RelayCommand(this.OnSaveCommandExecute, this.OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(this.OnDeleteCommandExecute, this.OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(this.OnSearchCommandExecute, this.OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnDoubleClickViewCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
            this.FilterProductCommand = new RelayCommand<object>(this.OnFilterProductCommandExecute, this.OnFilterProductCommandCanExecute);
            ApplyCommand = new RelayCommand<object>(OnApplyCommandExecute, OnApplyCommandCanExecute);
            this.RestoreCommand = new RelayCommand(this.OnRestoreCommandExecute, this.OnRestoreCommandCanExecute);
            LoadStepProductCommand = new RelayCommand(OnLoadStepProductCommandExecute, OnLoadStepProductCommandCanExecute);
        }
        #endregion

        #region LoadData

        /// <summary>
        /// To load data of base_ResourceAccount table.
        /// </summary>
        private void RefreshData()
        {

        }

        /// <summary>
        /// To load product when user select item on datagrid.
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="type"></param>
        private void LoadPricingChange(IEnumerable<long> conditions)
        {
            try
            {
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
                predicate = predicate.And(x => conditions.Contains(x.Id));
                IEnumerable<base_Product> _products = _productRepository.GetIEnumerable(predicate);
                foreach (var item in _products)
                {
                    base_PricingChange pricingChange = this.SelectedItemPricing.base_PricingManager.base_PricingChange.ToList().SingleOrDefault(x => x.ProductId == item.Id);
                    base_ProductModel _productModel = new base_ProductModel(item);
                    _productModel.CurrentPrice = pricingChange.CurrentPrice.Value;
                    _productModel.NewPrice = pricingChange.NewPrice.Value;
                    _productModel.PriceChange = pricingChange.PriceChanged.Value;
                    _productModel.EndUpdate();
                    this.SelectedItemPricing.ProductCollection.Add(_productModel);
                }
                this.SelectedItemPricing.EndUpdate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("LoadPricingChange" + ex.ToString());
            }
        }

        private void LoadPricingChangeWithProduct()
        {
            try
            {
                this.SelectedItemPricing.ProductCollection.Clear();
                IEnumerable<long> productIDs = this.SelectedItemPricing.base_PricingManager.base_PricingChange.Select(x => x.ProductId);
                //To get product within pricing.
                Expression<Func<base_Product, bool>> predicateWithInPricing = PredicateBuilder.True<base_Product>();
                predicateWithInPricing = predicateWithInPricing.And(x => !productIDs.Contains(x.ProductCategoryId));
                IEnumerable<base_Product> _productWithInPricing = _productRepository.GetIEnumerable(predicateWithInPricing);

                foreach (var item in _productWithInPricing)
                {
                    base_PricingChange pricingChange = this.SelectedItemPricing.base_PricingManager.base_PricingChange.SingleOrDefault(x => x.ProductId == item.Id);
                    base_ProductModel _productModel = new base_ProductModel(item);
                    _productModel.NewPrice = pricingChange.NewPrice.Value;
                    _productModel.PriceChange = pricingChange.PriceChanged.Value;
                    _productModel.EndUpdate();
                    this.SelectedItemPricing.ProductCollection.Add(_productModel);
                }

                //To get product without pricing.
                Expression<Func<base_Product, bool>> predicateWithoutPricing = PredicateBuilder.True<base_Product>();
                predicateWithoutPricing = predicateWithoutPricing.And(x => !productIDs.Contains(x.ProductCategoryId));
                IEnumerable<base_Product> _productWithoutPricing = _productRepository.GetIEnumerable(predicateWithoutPricing);
                foreach (var item in _productWithoutPricing)
                {
                    base_ProductModel _productModel = new base_ProductModel(item);
                    this.SelectedItemPricing.ProductCollection.Add(_productModel);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("LoadProduct" + ex.ToString());
            }
        }

        private void LoadProduct()
        {
            try
            {
                LoadProductDataByPredicate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("LoadProduct" + ex.ToString());
            }
        }

        private void LoadPricing(Expression<Func<base_PricingManager, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                this.PricingCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                base.IsBusy = true;
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * base.NumberOfDisplayItems;
                IList<base_PricingManager> pricings = _pricingManagerRepository.GetRangeDescending(indexItem, base.NumberOfDisplayItems, x => x.DateCreated, predicate);
                foreach (var item in pricings)
                    bgWorker.ReportProgress(0, item);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                //To add item.
                base_PricingManagerModel model = new base_PricingManagerModel(e.UserState as base_PricingManager);
                this.PricingCollection.Add(model);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (this.SelectedItemPricingClone != null && !this.IsSearchMode)
                {
                    this.SelectedItemPricing = this.PricingCollection.SingleOrDefault(x => x.Id == this.SelectedItemPricingClone.Id);
                    this.SetPricingDetail();
                    this.SelectedItemPricingClone = null;
                }
                //To count all User in Data base show on grid
                this.TotalPricings = _pricingManagerRepository.GetIQueryable(predicate).Count();
                base.IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
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
            if (this.SelectedItemPricing != null && this.SelectedItemPricing.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Information, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Cancel))
                    return false;
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        this.OnSaveCommandExecute();
                        result = true;
                    }
                    else //Has Error
                        result = false;

                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (this.SelectedItemPricing.IsNew)
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                        this.SelectedItemPricing = null;
                        this.SelectedItemPricingClone = this.SelectedItemPricing;
                    }
                    else //Old Item Rollback data
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();
                        this.RollBackPricing();
                        this.SelectedItemPricingClone = this.SelectedItemPricing;
                    }
                }
            }
            else
            {
                if (this.SelectedItemPricing != null && this.SelectedItemPricing.IsNew)
                {
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();
                }
                else
                {
                    if (this.SelectedItemPricing != null
                        && !this.SelectedItemPricing.IsNew
                        && !this.IsSearchMode)
                    {
                        this.SelectedItemPricingClone = null;
                        this.SelectedItemPricingClone = this.SelectedItemPricing;
                    }
                    if (this.SelectedItemPricing != null && this.SelectedItemPricing.IsNew)
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();
                    else
                        // Close all popup sticky
                        StickyManagementViewModel.CloseAllPopupSticky();


                }
            }
            return result;
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

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Delete" + ex.ToString());
            }
        }
        #endregion

        #region IsEditData
        private bool IsEditData()
        {
            if (this.SelectedItemPricing == null)
                return false;

            return ((this.SelectedItemPricing.IsDirty || this.SelectedItemPricing.IsChangeProductCollection));
        }
        #endregion

        #region RollBackData
        /// <summary>
        /// To rollback data when user click Cancel.
        /// </summary>
        private void RollBackData(bool isChangeItem)
        {
            //if (this.SelectedItemUser != null)
            //{
            //    base_ResourceAccountModel item = this.UserCollection.SingleOrDefault(x => x.UserResource.Equals(this._currentUserResource));
            //    To rollback data when user click Cancel.
            //    if (isChangeItem)
            //    {

            //    }
            //    To return old item when user click OK.
            //    else
            //        App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
            //        {
            //            this.SelectedItemUser = item;
            //            this.IsSetDefault = this._cloneIsSetDefault;
            //        });
            //}
        }
        #endregion

        #region VisibilityData
        /// <summary>
        ///To show item when user check into Check Box.
        /// </summary>
        private void SetVisibilityData(bool value)
        {
            Visibility _visibility = value ? Visibility.Visible : Visibility.Collapsed;
            //if (this.UserCollection != null)
            //    foreach (var item in this.UserCollection.Where(x => x.IsLocked.HasValue && x.IsLocked.Value))
            //        item.Visibility = _visibility;
        }
        #endregion

        #region ChangeCurrentPrice
        //To changecurrent price when user change list product.
        private void ChangeCurrentPrice()
        {
            foreach (var item in this.SelectedItemPricing.ProductCollection)
            {
                //To get current price.
                item.CurrentPrice = this.ReturnPrice(this.SelectedItemPricing.PriceLevel, item);
            }
        }
        #endregion

        #region ChangeDataOfProduct
        /// <summary>
        /// To change price of product.
        /// </summary>
        /// <param name="basePriceID"></param>
        private void ChangeDataOfProduct(int basePriceID)
        {
            this.SelectedItemPricing.IsChangeProductCollection = true;
            this.SelectedItemPricing.IsDirty = true;
            //To change Price with Cost
            //PriceChanged=NewPrice - Cost.Cost is type BasePrice when user choose it on BasePrice ComboBox.
            if (basePriceID == 1)
                foreach (var item in this.SelectedItemPricing.ProductCollection)
                {
                    //To caculate new price
                    item.NewPrice = this.CalculationData(this.SelectedItemPricing.CalculateMethod.Value, item.AverageUnitCost, this.SelectedItemPricing.AmountChange.Value, this.SelectedItemPricing.AmountUnit.Value);
                    //To caculate PriceChange
                    item.PriceChange = item.NewPrice - item.AverageUnitCost;
                }
            else
                //To change Price with Price
                //PriveChange=NewPrice - Price. Price is type price when user choose it on PriceLevel ComboBox.
                foreach (var item in this.SelectedItemPricing.ProductCollection)
                {
                    //To caculate new price
                    item.NewPrice = this.CalculationData(this.SelectedItemPricing.CalculateMethod.Value, item.CurrentPrice, this.SelectedItemPricing.AmountChange.Value, this.SelectedItemPricing.AmountUnit.Value);
                    //To caculate PriceChange
                    item.PriceChange = item.NewPrice - item.CurrentPrice;
                }
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
            if (amountUnit == 1)
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
            if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 1)
                return product.RegularPrice;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 2)
                return product.Price1;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 4)
                return product.Price2;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 8)
                return product.Price3;
            else
                return product.Price4;
        }
        #endregion

        #region IsActiveChangData
        /// <summary>
        /// To check condition to execute method..
        /// </summary>
        /// <returns></returns>
        private bool IsActiveChangData()
        {
            return (!string.IsNullOrEmpty(this.SelectedItemPricing.PriceLevel)
                && this.SelectedItemPricing.BasePrice > 0
                && this.SelectedItemPricing.AmountChange.HasValue
                && this.SelectedItemPricing.AmountChange.Value > 0
                && this.SelectedItemPricing.AmountUnit.HasValue
                && this.SelectedItemPricing.CalculateMethod.HasValue);
        }
        #endregion

        #region ShowNotification
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

                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
                else
                {
                    if (this.SelectedItemPricing.IsNew)
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();

                        //SelectedProduct = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                        this.SelectedItemPricing = null;
                        this.SelectedItemPricingClone = this.SelectedItemPricing;
                    }
                    else
                    {
                        // Close all popup sticky
                        StickyManagementViewModel.CloseAllPopupSticky();

                        this.RollBackPricing();
                    }
                }
            }
            else
            {
                if (this.SelectedItemPricing != null && this.SelectedItemPricing.IsNew)
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();
                else
                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
            }
            return result;
        }
        #endregion

        #region RollBackPricing
        //To return data when users dont save them.
        private void RollBackPricing()
        {
            this.SelectedItemPricing.IsLoad = true;
            this.SelectedItemPricing.ToModelAndRaise();
            this.SelectedItemPricing.IsLoad = false;
            this.SelectedItemPricing.EndUpdate();
            this.SelectedItemPricing.IsChangeProductCollection = false;
        }
        #endregion

        #region SetPricingDetail

        /// <summary>
        /// Set pricing detail
        /// </summary>
        private void SetPricingDetail()
        {
            if (SelectedItemPricing != null)
            {
                // Clear product collection
                SelectedItemPricing.ProductCollection.Clear();

                // Set affect pricing
                _affectPricingType = (AffectPricing)SelectedItemPricing.AffectPricing;

                LoadPricingChange(SelectedItemPricing.base_PricingManager.base_PricingChange.Select(x => x.ProductId));

                // Update total products
                TotalProducts = SelectedItemPricing.ProductCollection.Count();

                // Set visibility of buttons
                if (SelectedItemPricing.DateApplied.HasValue)
                {
                    // Hide apply button
                    SelectedItemPricing.VisibilityApplied = Visibility.Collapsed;

                    // Show restore button
                    SelectedItemPricing.VisibilityRestore = Visibility.Visible;
                }

                if (SelectedItemPricing.DateApplied.HasValue || SelectedItemPricing.DateRestored.HasValue)
                    SelectedItemPricing.IsEnable = false;

                SelectedItemPricing.IsErrorProductCollection = false;

                // Load resource note collection
                LoadResourceNoteCollection(SelectedItemPricing);
                StickyManagementViewModel.SetParentResource(SelectedItemPricing.Resource.ToString(), SelectedItemPricing.ResourceNoteCollection);
            }
        }

        #endregion

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateSearchProductPredicate()
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false);

            return predicate;
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadProductDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Product, bool>> predicate = CreateSearchProductPredicate();

            // Load data by predicate
            LoadProductDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="predicate">Condition for load data</param>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadProductDataByPredicate(Expression<Func<base_Product, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                SelectedItemPricing.ProductCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                if (SelectedItemPricing != null && !SelectedItemPricing.IsNew)
                {
                    // Get total products with condition in predicate
                    TotalProducts = SelectedItemPricing.base_PricingManager.base_PricingChange.Count;

                    IEnumerable<base_PricingChange> pricingChanges = SelectedItemPricing.base_PricingManager.base_PricingChange.Skip(SelectedItemPricing.ProductCollection.Count).Take(NumberOfDisplayItems);
                    foreach (base_PricingChange pricingChange in pricingChanges)
                    {
                        if (!SelectedItemPricing.IsDirty)
                        {
                            // Create new product model
                            base_ProductModel productModel = new base_ProductModel(pricingChange.base_Product);

                            productModel.CurrentPrice = pricingChange.CurrentPrice.Value;
                            productModel.NewPrice = pricingChange.NewPrice.Value;
                            productModel.PriceChange = pricingChange.PriceChanged.Value;

                            // Turn off IsDirty & IsNew
                            productModel.EndUpdate();

                            bgWorker.ReportProgress(0, productModel);
                        }
                        else
                        {
                            bgWorker.ReportProgress(0, pricingChange.base_Product);
                        }
                    }
                }
                else
                {
                    // Get total products with condition in predicate
                    TotalProducts = _productRepository.GetIQueryable(predicate).Count();

                    // Get data with range
                    IList<base_Product> products = _productRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.DateCreated, predicate);
                    foreach (base_Product product in products)
                    {
                        bgWorker.ReportProgress(0, product);
                    }
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create product model
                base_ProductModel productModel = new base_ProductModel();

                if (SelectedItemPricing == null || SelectedItemPricing.IsNew || SelectedItemPricing.IsDirty)
                {
                    productModel = new base_ProductModel((base_Product)e.UserState);

                    // Load relation data
                    LoadProductRelationData(productModel);
                }
                else
                {
                    productModel = e.UserState as base_ProductModel;
                }

                // Add to collection
                SelectedItemPricing.ProductCollection.Add(productModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (SelectedItemPricing.ProductCollection.Count > 0)
                    SelectedItemPricing.IsErrorProductCollection = false;

                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for product
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadProductRelationData(base_ProductModel productModel)
        {
            // Caculate new price
            productModel.NewPrice = CalculationData(SelectedItemPricing.CalculateMethod.Value, productModel.AverageUnitCost,
                SelectedItemPricing.AmountChange.Value, SelectedItemPricing.AmountUnit.Value);

            // Caculate PriceChange
            if (SelectedItemPricing.BasePrice == 1)
                productModel.PriceChange = productModel.NewPrice - productModel.AverageUnitCost;
            else
                productModel.PriceChange = productModel.NewPrice - productModel.CurrentPrice;

            // Turn off IsDirty & IsNew
            productModel.EndUpdate();
        }

        /// <summary>
        /// Load product collection when affect pricing changed
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="affectPricing"></param>
        private void LoadProductCollection(List<ComboItem> conditions, AffectPricing affectPricing)
        {
            try
            {
                // Initial predicate
                _productPredicate = PredicateBuilder.True<base_Product>();

                // Default condition
                _productPredicate = _productPredicate.And(x => x.IsPurge == false);

                if (affectPricing.Equals(AffectPricing.Category))
                {
                    IEnumerable<int> categoryIds = conditions.Select(x => x.IntValue);
                    _productPredicate = _productPredicate.And(x => categoryIds.Contains(x.ProductCategoryId));
                }
                else if (affectPricing.Equals(AffectPricing.Vendor))
                {
                    IEnumerable<long> vendorIds = conditions.Select(x => x.LongValue);
                    _productPredicate = _productPredicate.And(x => vendorIds.Contains(x.VendorId));
                }
                else
                {
                    IEnumerable<long> productIDs = conditions.Select(x => x.LongValue);
                    _productPredicate = _productPredicate.And(x => productIDs.Contains(x.Id));
                }

                // Load product data by predicate
                LoadProductDataByPredicate(_productPredicate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadProduct" + ex.ToString());
            }
        }

        /// <summary>
        /// Create new pricing change model
        /// </summary>
        /// <param name="productModel"></param>
        private base_PricingChangeModel CreateNewPricingChangeModel(base_ProductModel productModel)
        {
            // Initial new pricing change model
            base_PricingChangeModel pricingChangeModel = new base_PricingChangeModel();

            // Set pricing change values
            pricingChangeModel.base_PricingChange.PricingManagerId = SelectedItemPricing.base_PricingManager.Id;
            pricingChangeModel.base_PricingChange.PricingManagerResource = SelectedItemPricing.Resource.ToString();
            pricingChangeModel.base_PricingChange.ProductId = productModel.Id;
            pricingChangeModel.base_PricingChange.ProductResource = productModel.Resource.ToString();
            pricingChangeModel.base_PricingChange.Cost = productModel.AverageUnitCost;
            pricingChangeModel.base_PricingChange.CurrentPrice = productModel.CurrentPrice;
            pricingChangeModel.base_PricingChange.NewPrice = productModel.NewPrice;
            pricingChangeModel.base_PricingChange.PriceChanged = productModel.PriceChange;
            pricingChangeModel.base_PricingChange.DateCreated = DateTimeExt.Today;

            // Turn off IsDirty & IsNew
            pricingChangeModel.EndUpdate();

            return pricingChangeModel;
        }

        /// <summary>
        /// Update product price
        /// </summary>
        /// <param name="product"></param>
        /// <param name="price"></param>
        private void UpdateProductPrice(base_Product product, decimal price)
        {
            // Get price schema
            ComboItem priceSchema = Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel));

            // Set new price by price schema
            switch (priceSchema.Value)
            {
                case 1:
                    product.RegularPrice = price;
                    break;
                case 2:
                    product.Price1 = price;
                    break;
                case 4:
                    product.Price2 = price;
                    break;
                case 8:
                    product.Price3 = price;
                    break;
                default:
                    product.Price4 = price;
                    break;
            }
        }

        /// <summary>
        /// Save new pricing manager and pricing changes
        /// </summary>
        /// <param name="pricingManagerModel"></param>
        private void SaveNew(base_PricingManagerModel pricingManagerModel)
        {
            try
            {
                // Create background worker
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

                bgWorker.DoWork += (sender, e) =>
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    pricingManagerModel.DateCreated = DateTimeExt.Today;
                    pricingManagerModel.UserCreated = Define.USER.LoginName;
                    pricingManagerModel.ItemCount = TotalProducts;
                    pricingManagerModel.Shift = Define.ShiftCode;

                    // Map data from model to entity 
                    pricingManagerModel.ToEntity();

                    // Add new pricing manager to database
                    _pricingManagerRepository.Add(pricingManagerModel.base_PricingManager);

                    // Accept changes
                    _pricingManagerRepository.Commit();

                    // Save changes of loaded products to pricing change table
                    foreach (base_ProductModel productModel in pricingManagerModel.ProductCollection)
                    {
                        // Create new pricing change model
                        base_PricingChangeModel pricingChangeModel = CreateNewPricingChangeModel(productModel);

                        // Add new pricing change to database
                        pricingManagerModel.base_PricingManager.base_PricingChange.Add(pricingChangeModel.base_PricingChange);
                    }

                    // Get all products that have not loaded by predicate
                    IList<base_Product> products = _productRepository.GetRangeDescending(pricingManagerModel.ProductCollection.Count, TotalProducts - pricingManagerModel.ProductCollection.Count, x => x.DateCreated, _productPredicate);

                    foreach (base_Product product in products)
                    {
                        // Create product model
                        base_ProductModel productModel = new base_ProductModel(product);

                        // Calculate new price
                        productModel.NewPrice = CalculationData(pricingManagerModel.CalculateMethod.Value, productModel.AverageUnitCost,
                            pricingManagerModel.AmountChange.Value, pricingManagerModel.AmountUnit.Value);

                        // Calculate price change for product
                        if (pricingManagerModel.BasePrice == 1)
                            productModel.PriceChange = productModel.NewPrice - productModel.AverageUnitCost;
                        else
                            productModel.PriceChange = productModel.NewPrice - productModel.CurrentPrice;

                        bgWorker.ReportProgress(0, productModel);

                        // Create new pricing change model
                        base_PricingChangeModel pricingChangeModel = CreateNewPricingChangeModel(productModel);

                        // Add new pricing change to database
                        pricingManagerModel.base_PricingManager.base_PricingChange.Add(pricingChangeModel.base_PricingChange);
                    }

                    // Accept changes
                    _pricingManagerRepository.Commit();

                    // Update pricing manager id
                    pricingManagerModel.Id = pricingManagerModel.base_PricingManager.Id;

                    // Turn off IsDirty & IsNew
                    pricingManagerModel.EndUpdate();

                    // Write log
                    App.WriteUserLog("Pricing", "User insterd a new pricing." + pricingManagerModel.base_PricingManager.Id);
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {
                    // Add to collection
                    pricingManagerModel.ProductCollection.Add(e.UserState as base_ProductModel);
                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    // Insert new pricing manager to collection
                    PricingCollection.Insert(0, pricingManagerModel);

                    // Update total pricing managers
                    TotalPricings = PricingCollection.Count;

                    // Show Apply button
                    pricingManagerModel.VisibilityApplied = Visibility.Visible;

                    // Turn off IsDirty & IsNew
                    pricingManagerModel.EndUpdate();

                    pricingManagerModel.IsChangeProductCollection = false;
                    pricingManagerModel.IsErrorProductCollection = false;

                    // Map data from entity to model
                    pricingManagerModel.ToModelAndRaise();

                    // Turn off BusyIndicator
                    IsBusy = false;
                };

                // Run async background worker
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error("SAVE NEW PRICING\n" + ex.ToString());
            }
        }

        /// <summary>
        /// Save update pricing manager and pricing changes
        /// </summary>
        /// <param name="pricingManagerModel"></param>
        private void SaveUpdate(base_PricingManagerModel pricingManagerModel)
        {
            try
            {
                // Create background worker
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

                bgWorker.DoWork += (sender, e) =>
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    pricingManagerModel.ItemCount = TotalProducts;

                    // Map data from model to entity
                    pricingManagerModel.ToEntity();

                    if (pricingManagerModel.IsChangeProductCollection)
                    {
                        // Delete all pricing change items
                        _pricingChangeRepository.Delete(pricingManagerModel.base_PricingManager.base_PricingChange);
                        pricingManagerModel.base_PricingManager.base_PricingChange.Clear();

                        // Accept changes
                        _pricingChangeRepository.Commit();

                        // Save changes of loaded products to pricing change table
                        foreach (base_ProductModel productModel in pricingManagerModel.ProductCollection)
                        {
                            // Create new pricing change model
                            base_PricingChangeModel pricingChangeModel = CreateNewPricingChangeModel(productModel);

                            // Add new pricing change to database
                            pricingManagerModel.base_PricingManager.base_PricingChange.Add(pricingChangeModel.base_PricingChange);
                        }

                        // Get all products that have not loaded by predicate
                        IList<base_Product> products = _productRepository.GetRangeDescending(pricingManagerModel.ProductCollection.Count, TotalProducts - pricingManagerModel.ProductCollection.Count, x => x.DateCreated, _productPredicate);

                        foreach (base_Product product in products)
                        {
                            // Create product model
                            base_ProductModel productModel = new base_ProductModel(product);

                            // Calculate new price
                            productModel.NewPrice = CalculationData(pricingManagerModel.CalculateMethod.Value, productModel.AverageUnitCost,
                                pricingManagerModel.AmountChange.Value, pricingManagerModel.AmountUnit.Value);

                            // Calculate price change for product
                            if (pricingManagerModel.BasePrice == 1)
                                productModel.PriceChange = productModel.NewPrice - productModel.AverageUnitCost;
                            else
                                productModel.PriceChange = productModel.NewPrice - productModel.CurrentPrice;

                            bgWorker.ReportProgress(0, productModel);

                            // Create new pricing change model
                            base_PricingChangeModel pricingChangeModel = CreateNewPricingChangeModel(productModel);

                            // Add new pricing change to database
                            pricingManagerModel.base_PricingManager.base_PricingChange.Add(pricingChangeModel.base_PricingChange);
                        }
                    }

                    // Accept changes
                    _pricingManagerRepository.Commit();

                    // Turn off IsDirty & IsNew
                    pricingManagerModel.EndUpdate();

                    // Write log
                    App.WriteUserLog("Pricing", "User insterd a new pricing." + pricingManagerModel.Id);
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {
                    // Add to collection
                    pricingManagerModel.ProductCollection.Add(e.UserState as base_ProductModel);
                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    // Turn off IsDirty & IsNew
                    pricingManagerModel.EndUpdate();

                    pricingManagerModel.IsChangeProductCollection = false;
                    pricingManagerModel.IsErrorProductCollection = false;

                    // Map data from entity to model
                    pricingManagerModel.ToModelAndRaise();

                    // Turn off BusyIndicator
                    IsBusy = false;
                };

                // Run async background worker
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error("SAVE UPDATE PRICING\n" + ex.ToString());
            }
        }

        /// <summary>
        /// Apply pricing, update new price to product
        /// </summary>
        /// <param name="pricingManagerModel"></param>
        private void ApplyPricing(base_PricingManagerModel pricingManagerModel)
        {
            try
            {
                // Create background worker
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

                bgWorker.DoWork += (sender, e) =>
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    pricingManagerModel.Status = Common.PricingStatus.SingleOrDefault(x => x.Value == 2).Text;
                    pricingManagerModel.DateApplied = DateTimeExt.Today;
                    pricingManagerModel.UserApplied = Define.USER.LoginName;

                    // Map data from model to entity
                    pricingManagerModel.ToEntity();

                    if (IsSearchMode)
                    {
                        foreach (base_PricingChange pricingChange in pricingManagerModel.base_PricingManager.base_PricingChange)
                        {
                            if (pricingChange.base_Product != null && pricingChange.NewPrice.HasValue)
                            {
                                if (pricingManagerModel.BasePrice == 1)
                                    pricingChange.base_Product.AverageUnitCost = pricingChange.NewPrice.Value;
                                else
                                    UpdateProductPrice(pricingChange.base_Product, pricingChange.NewPrice.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (base_ProductModel productItem in pricingManagerModel.ProductCollection)
                        {
                            // Update price of product
                            if (pricingManagerModel.BasePrice == 1)
                            {
                                productItem.AverageUnitCost = productItem.NewPrice;
                                productItem.base_Product.AverageUnitCost = productItem.NewPrice;
                            }
                            else
                                UpdateProductPrice(productItem.base_Product, productItem.NewPrice);
                        }

                        IEnumerable<base_PricingChange> pricingChanges = SelectedItemPricing.base_PricingManager.base_PricingChange.Skip(SelectedItemPricing.ProductCollection.Count).Take(TotalProducts - pricingManagerModel.ProductCollection.Count);
                        foreach (base_PricingChange pricingChange in pricingChanges)
                        {
                            // Create new product model
                            base_ProductModel productModel = new base_ProductModel(pricingChange.base_Product);

                            productModel.CurrentPrice = pricingChange.CurrentPrice.Value;
                            productModel.NewPrice = pricingChange.NewPrice.Value;
                            productModel.PriceChange = pricingChange.PriceChanged.Value;

                            // Update price of product
                            if (pricingManagerModel.BasePrice == 1)
                            {
                                productModel.AverageUnitCost = productModel.NewPrice;
                                productModel.base_Product.AverageUnitCost = productModel.NewPrice;
                            }
                            else
                                UpdateProductPrice(productModel.base_Product, productModel.NewPrice);

                            // Turn off IsDirty & IsNew
                            productModel.EndUpdate();

                            bgWorker.ReportProgress(0, productModel);
                        }
                    }

                    // Accept changes
                    _productRepository.Commit();

                    // Show restore button
                    pricingManagerModel.VisibilityRestore = Visibility.Visible;

                    // Hide apply button
                    pricingManagerModel.VisibilityApplied = Visibility.Collapsed;

                    // Turn off IsDirty & IsNew
                    pricingManagerModel.EndUpdate();

                    // Write log
                    App.WriteUserLog("Pricing", "User insterd a new pricing." + pricingManagerModel.Id);
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {
                    // Add new product to collection
                    pricingManagerModel.ProductCollection.Add(e.UserState as base_ProductModel);
                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    // Turn off BusyIndicator
                    IsBusy = false;
                };

                // Run async background worker
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error("APPLY PRICING\n" + ex.ToString());
            }
        }

        /// <summary>
        /// Restore pricing
        /// </summary>
        /// <param name="pricingManagerModel"></param>
        private void RestorePricing(base_PricingManagerModel pricingManagerModel)
        {
            try
            {
                // Create background worker
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

                bgWorker.DoWork += (sender, e) =>
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    if (!string.IsNullOrWhiteSpace(pricingManagerModel.UserApplied) && pricingManagerModel.DateApplied.HasValue)
                    {
                        pricingManagerModel.Status = Common.PricingStatus.SingleOrDefault(x => x.Value == 3).Text;
                        pricingManagerModel.DateRestored = DateTimeExt.Today;
                        pricingManagerModel.UserRestored = Define.USER.LoginName;

                        // Map data from model to entity
                        pricingManagerModel.ToEntity();

                        // Update price of product
                        foreach (base_ProductModel productItem in pricingManagerModel.ProductCollection)
                        {
                            // Get pricing manager resource
                            string pricingManagerResource = pricingManagerModel.Resource.ToString();

                            base_PricingChange pricingChange = productItem.base_Product.base_PricingChange.SingleOrDefault(x => x.PricingManagerResource.Equals(pricingManagerResource));

                            if (pricingChange != null)
                            {
                                // Update price of product
                                if (pricingManagerModel.BasePrice == 1)
                                {
                                    productItem.AverageUnitCost = pricingChange.Cost.Value;
                                    productItem.base_Product.AverageUnitCost = pricingChange.Cost.Value;
                                }
                                else
                                    UpdateProductPrice(productItem.base_Product, pricingChange.CurrentPrice.Value);
                            }
                        }

                        IEnumerable<base_PricingChange> pricingChanges = SelectedItemPricing.base_PricingManager.base_PricingChange.Skip(SelectedItemPricing.ProductCollection.Count).Take(TotalProducts - pricingManagerModel.ProductCollection.Count);
                        foreach (base_PricingChange pricingChange in pricingChanges)
                        {
                            // Create new product model
                            base_ProductModel productModel = new base_ProductModel(pricingChange.base_Product);

                            productModel.CurrentPrice = pricingChange.CurrentPrice.Value;
                            productModel.NewPrice = pricingChange.NewPrice.Value;
                            productModel.PriceChange = pricingChange.PriceChanged.Value;

                            // Get pricing manager resource
                            string pricingManagerResource = pricingManagerModel.Resource.ToString();

                            if (pricingChange != null)
                            {
                                // Update price of product
                                if (pricingManagerModel.BasePrice == 1)
                                {
                                    productModel.AverageUnitCost = pricingChange.Cost.Value;
                                    productModel.base_Product.AverageUnitCost = pricingChange.Cost.Value;
                                }
                                else
                                    UpdateProductPrice(productModel.base_Product, pricingChange.CurrentPrice.Value);
                            }

                            // Turn off IsDirty & IsNew
                            productModel.EndUpdate();

                            bgWorker.ReportProgress(0, productModel);
                        }

                        // Accept changes
                        _productRepository.Commit();

                        pricingManagerModel.IsEnable = false;

                        // Turn off IsDirty & IsNew
                        pricingManagerModel.EndUpdate();

                        // Write log
                        App.WriteUserLog("Pricing", "User insterd a new pricing." + pricingManagerModel.Id);
                    }
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {
                    // Add new product to collection
                    pricingManagerModel.ProductCollection.Add(e.UserState as base_ProductModel);
                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    // Turn off BusyIndicator
                    IsBusy = false;
                };

                // Run async background worker
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error("RESTORE PRICING\n" + ex.ToString());
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
            this.PricingCollection.Clear();
            Expression<Func<base_PricingManager, bool>> predicate = PredicateBuilder.True<base_PricingManager>();
            if (!string.IsNullOrWhiteSpace(this.Keyword))
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadPricing(predicate, true);
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

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                switch (columnName)
                {
                    case "IsCheckedAll":
                        //if ((this.IsCheckedAll == null
                        //    || !this.IsCheckedAll.Value)
                        //    && this.UserRightCollection.Count(x => x.IsChecked) == 0)
                        //    message = "User right must be selected.";
                        break;
                }
                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }

        #endregion

        #region Note Module

        /// <summary>
        /// Initial resource note repository
        /// </summary>
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();

        /// <summary>
        /// Get or sets the StickyManagementViewModel
        /// </summary>
        public PopupStickyViewModel StickyManagementViewModel { get; set; }

        /// <summary>
        /// Load resource note collection
        /// </summary>
        /// <param name="pricingManagerModel"></param>
        private void LoadResourceNoteCollection(base_PricingManagerModel pricingManagerModel)
        {
            // Load resource note collection
            if (pricingManagerModel.ResourceNoteCollection == null)
            {
                string resource = pricingManagerModel.Resource.ToString();
                pricingManagerModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        #endregion
    }
}