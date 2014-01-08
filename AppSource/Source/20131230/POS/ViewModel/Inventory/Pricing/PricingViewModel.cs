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
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Globalization;

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
        public RelayCommand<object> EditCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand RestoreCommand { get; private set; }
        public RelayCommand<object> FilterProductCommand { get; private set; }
        //To get type of Affectpricing.
        private string _typeAffectPricing = string.Empty;
        private base_PricingManagerModel SelectedItemPricingClone { get; set; }
        #endregion

        #region Constructors

        public PricingViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            StickyManagementViewModel = new PopupStickyViewModel();
            this.InitialCommand();
            this.InitialData();
        }

        public PricingViewModel(bool isList, object param = null)
            : this()
        {
            this.ChangeSearchMode(isList, param);

            // Get permission
            GetPermission();
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
        /// <summary>
        /// Gets or sets the SelectedItemPricing.
        /// </summary>
        private base_PricingManagerModel _selectedItemPricing;
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
            return AllowAddPricing;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute(object param)
        {

            if (this.ChangeViewExecute(null))
            {
                // TODO: Handle command logic here
                this.TotalProducts = this.ProductCollection.Count;
                this.SelectedItemPricing = new base_PricingManagerModel();
                this.SelectedItemPricing.IsLoad = true;
                this.SelectedItemPricing.Status = EmployeeStatuses.Pending.ToString();
                this.SelectedItemPricing.BasePrice = 1;
                this.SelectedItemPricing.CalculateMethod = 1;
                this.SelectedItemPricing.PriceLevel = CPC.Helper.Common.PriceSchemas.FirstOrDefault().Text;
                this.SelectedItemPricing.AmountChange = 0;
                this.SelectedItemPricing.AmountUnit = 1;
                this.SelectedItemPricing.IsLoad = false;
                this.SelectedItemPricing.Resource = Guid.NewGuid();
                this.SelectedItemPricing.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
                StickyManagementViewModel.SetParentResource(SelectedItemPricing.Resource.ToString(), SelectedItemPricing.ResourceNoteCollection);
                this.SelectedItemPricing.IsDirty = false;
                this.LoadProduct();
                this.SelectedItemPricing.ProductCollection = this.ProductCollection;
                this.SelectedItemPricing.IsErrorProductCollection = false;
                this.ChangeCurrentPrice();
                this.TotalProducts = this.SelectedItemPricing.ProductCollection.Count;
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
            return this.SelectedItemPricing != null && !this.SelectedItemPricing.DateRestored.HasValue;
        }
        #endregion

        #region Save Command
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
            this.SelectedItemPricing.Shift = Define.ShiftCode;
            // TODO: Handle command logic here
            if (this.SelectedItemPricing.IsNew)
            {
                this.Insert();
                this.PricingCollection.Insert(0, this.SelectedItemPricing);
                this.TotalPricings = this.PricingCollection.Count;
                this.SelectedItemPricing.VisibilityApplied = Visibility.Visible;
            }
            else
                this.Update();
            this.SelectedItemPricing.ToModelAndRaise();
            this.SelectedItemPricing.EndUpdate();
            this.SelectedItemPricing.IsChangeProductCollection = false;
            this.SelectedItemPricing.IsErrorProductCollection = false;
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
                // TODO: Handle command logic here
                this.SearchAlert = string.Empty;
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())))//Search All
                {
                    Expression<Func<base_PricingManager, bool>> predicate = PredicateBuilder.True<base_PricingManager>();
                    this.LoadPricing(predicate, false, 0);
                }
                else if (param != null)
                {
                    this.Keyword = param.ToString();
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
            if (param != null && this.IsSearchMode)
            {
                //To set pricing detail
                this.SetPricingDetail();
                //To show detail form.
                this.IsSearchMode = false;
            }
            else if (this.IsSearchMode)
            {
                //To show detail form.
                this.IsSearchMode = false;
            }
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
            if (!this.SelectedItemPricing.IsLoad)
            {
                //To load data with Category
                if (param.Equals("Category"))
                {
                    if (!this._typeAffectPricing.Equals("Category"))
                        this.SelectedItemPricing.ProductCollection.Clear();
                    PricingCategoryViewModel viewModel = new PricingCategoryViewModel();
                    bool? result = _dialogService.ShowDialog<PricingCategoryView>(_ownerViewModel, viewModel, "Select products to apply to this pricing.");
                    if (result.HasValue && result.Value)
                    {
                        isLoad = true;
                        this.LoadProduct(viewModel.CategoryList, "Category");
                    }
                    else
                    {
                        this.SelectedItemPricing.IsChangeProductCollection = true;
                        //this.SelectedItemPricing.ProductCollection.Clear();
                    }
                    _typeAffectPricing = "Category";
                }
                //To load data with Vendor
                else if (param.Equals("Vendor"))
                {
                    if (!this._typeAffectPricing.Equals("Vendor"))
                        this.SelectedItemPricing.ProductCollection.Clear();
                    PricingVendorViewModel viewModel = new PricingVendorViewModel();
                    bool? result = _dialogService.ShowDialog<PricingVendorView>(_ownerViewModel, viewModel, "Select products to apply to this pricing.");
                    if (result.HasValue && result.Value)
                    {
                        isLoad = true;
                        this.LoadProduct(viewModel.CategoryList, "Vendor");
                    }
                    else
                    {
                        this.SelectedItemPricing.IsChangeProductCollection = true;
                        //this.SelectedItemPricing.ProductCollection.Clear();
                    }
                    _typeAffectPricing = "Vendor";
                }
                //To load data with Custom
                else if (param.Equals("Custom"))
                {
                    if (!this._typeAffectPricing.Equals("Custom"))
                        this.SelectedItemPricing.ProductCollection.Clear();
                    // Get all category that contain in department list
                    List<ComboItem> CategoryList = new List<ComboItem>(_departmentRepository.
                GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                OrderBy(x => x.Name).Where(x => x.LevelId == 1).
                        Select(x => new ComboItem { IntValue = x.Id, Text = x.Name }));
                    PricingCustomViewModel viewModel = new PricingCustomViewModel(CategoryList);
                    bool? result = _dialogService.ShowDialog<CustomView>(_ownerViewModel, viewModel, "Select products to apply to this pricing.");
                    if (result.HasValue && result.Value)
                    {
                        isLoad = true;
                        this.LoadProduct(viewModel.ProductIDList, "Custom");
                    }
                    else
                    {
                        this.SelectedItemPricing.IsChangeProductCollection = true;
                    }
                    _typeAffectPricing = "Custom";
                }
                //To load all of data.
                else
                {
                    if (!this._typeAffectPricing.Equals("Default"))
                        this.SelectedItemPricing.ProductCollection.Clear();
                    this.SelectedItemPricing.ProductCollection.Clear();
                    isLoad = true;
                    this.LoadProduct();
                    this.SelectedItemPricing.ProductCollection = this.ProductCollection;
                    _typeAffectPricing = "Default";
                }
                if (isLoad)
                {
                    this.ChangeCurrentPrice();
                }
                //To change price of product when user changes affrectpricing.
                if (isLoad && !this.SelectedItemPricing.IsLoad && this.IsActiveChangData())
                    this.ChangeDataOfProduct(this.SelectedItemPricing.BasePrice);
                this.TotalProducts = this.SelectedItemPricing.ProductCollection.Count;
                this.SelectedItemPricing.IsErrorProductCollection = true;
            }
        }
        #endregion

        #region ApplyCommand
        /// <summary>
        /// Method to check whether the ApplyCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnApplyCommandCanExecute()
        {
            return this.IsValid && (this.SelectedItemPricing != null && !this.SelectedItemPricing.IsDirty && !this.SelectedItemPricing.IsNew && !this.SelectedItemPricing.DateApplied.HasValue);
        }

        /// <summary>
        /// Method to invoke when the ApplyCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute()
        {
            //To close product view
            if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text14, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //To apply that change pricing.
            this.SelectedItemPricing.IsEnable = false;
            this.Apply();

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
                    this.Restore();
                    this.IsSearchMode = false;
                }
            }

        }
        #endregion

        #endregion

        #region Private Methods

        private void InitialData()
        {
            this.NotePopupCollection = new ObservableCollection<PopupContainer>();
            this.NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };
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
                    if (int.TryParse(keyword, out IntValue))
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
                    if (decimal.TryParse(keyword, out amountValue))
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
            this.EditCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnEditCommandCanExecute);
            this.SaveCommand = new RelayCommand(this.OnSaveCommandExecute, this.OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(this.OnDeleteCommandExecute, this.OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(this.OnSearchCommandExecute, this.OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnDoubleClickViewCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
            this.FilterProductCommand = new RelayCommand<object>(this.OnFilterProductCommandExecute, this.OnFilterProductCommandCanExecute);
            this.ApplyCommand = new RelayCommand(this.OnApplyCommandExecute, this.OnApplyCommandCanExecute);
            this.RestoreCommand = new RelayCommand(this.OnRestoreCommandExecute, this.OnRestoreCommandCanExecute);
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
        /// To load product when user change AffectPricing.
        /// </summary>
        /// <param name="Conditions"></param>
        /// <param name="type"></param>
        private void LoadProduct(List<ComboItem> Conditions, string type)
        {
            try
            {
                IEnumerable<int> CategoryIds = null;
                IEnumerable<long> VendorIds = null;
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
                if (type.Equals("Vendor"))
                {
                    VendorIds = Conditions.Select(x => x.LongValue);
                    predicate = predicate.And(x => VendorIds.Contains(x.VendorId));
                }
                else if (type.Equals("Category"))
                {
                    CategoryIds = Conditions.Select(x => x.IntValue);
                    predicate = predicate.And(x => CategoryIds.Contains(x.ProductCategoryId));
                }
                else
                {
                    IEnumerable<long> productIDs = Conditions.Select(x => x.LongValue);
                    predicate = predicate.And(x => productIDs.Contains(x.Id));
                }
                IEnumerable<base_Product> _products = _productRepository.GetIEnumerable(predicate);

                if (this.SelectedItemPricing.ProductCollection == null || this.SelectedItemPricing.ProductCollection.Count == 0)
                {
                    foreach (var item in _products)
                    {
                        base_ProductModel _productModel = new base_ProductModel(item);
                        this.SelectedItemPricing.ProductCollection.Add(_productModel);
                    }
                }
                else
                {
                    foreach (var item in _products)
                    {
                        if (!this.SelectedItemPricing.ProductCollection.Select(x => x.Id).Contains(item.Id))
                        {
                            base_ProductModel _productModel = new base_ProductModel(item);
                            this.SelectedItemPricing.ProductCollection.Add(_productModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadProduct" + ex.ToString());
            }
        }

        /// <summary>
        /// To load product when user select item on datagrid.
        /// </summary>
        /// <param name="Conditions"></param>
        /// <param name="type"></param>
        private void LoadPricingChange(IEnumerable<long> Conditions, string type)
        {
            try
            {
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
                predicate = predicate.And(x => Conditions.Contains(x.Id));
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
                Debug.WriteLine("LoadPricingChange" + ex.ToString());
            }
        }
        private void LoadPricingChangeWithProduct()
        {
            try
            {
                this.SelectedItemPricing.ProductCollection.Clear();
                IEnumerable<long> ProductIDs = this.SelectedItemPricing.base_PricingManager.base_PricingChange.Select(x => x.ProductId);
                //To get product within pricing.
                Expression<Func<base_Product, bool>> predicateWithInPricing = PredicateBuilder.True<base_Product>();
                predicateWithInPricing = predicateWithInPricing.And(x => !ProductIDs.Contains(x.ProductCategoryId));
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
                predicateWithoutPricing = predicateWithoutPricing.And(x => !ProductIDs.Contains(x.ProductCategoryId));
                IEnumerable<base_Product> _productWithoutPricing = _productRepository.GetIEnumerable(predicateWithoutPricing);
                foreach (var item in _productWithoutPricing)
                {
                    base_ProductModel _productModel = new base_ProductModel(item);
                    this.SelectedItemPricing.ProductCollection.Add(_productModel);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadProduct" + ex.ToString());
            }
        }
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

        #region Insert
        /// <summary>
        /// To insert data into base_PricingManager and base_PricingChange table.
        /// </summary>
        private void Insert()
        {
            try
            {
                this.SelectedItemPricing.DateCreated = DateTimeExt.Today;
                if (Define.USER != null)
                    this.SelectedItemPricing.UserCreated = Define.USER.LoginName;
                else
                    this.SelectedItemPricing.UserCreated = "admin";
                this.SelectedItemPricing.ItemCount = this.SelectedItemPricing.ProductCollection.Count;
                this.SelectedItemPricing.ToEntity();
                this._pricingManagerRepository.Commit();
                this._pricingManagerRepository.Add(this.SelectedItemPricing.base_PricingManager);
                foreach (var item in this.SelectedItemPricing.ProductCollection)
                {
                    base_PricingChangeModel model = new base_PricingChangeModel();
                    model.base_PricingChange.PricingManagerId = this.SelectedItemPricing.base_PricingManager.Id;
                    model.base_PricingChange.PricingManagerResource = this.SelectedItemPricing.Resource.ToString();
                    model.base_PricingChange.ProductId = item.Id;
                    model.base_PricingChange.ProductResource = item.Resource.ToString();
                    model.base_PricingChange.Cost = item.AverageUnitCost;
                    model.base_PricingChange.CurrentPrice = item.CurrentPrice;
                    model.base_PricingChange.NewPrice = item.NewPrice;
                    model.base_PricingChange.PriceChanged = item.PriceChange;
                    model.base_PricingChange.DateCreated = DateTimeExt.Today;
                    this.SelectedItemPricing.base_PricingManager.base_PricingChange.Add(model.base_PricingChange);
                    model.EndUpdate();
                }
                this._pricingManagerRepository.Commit();
                this.SelectedItemPricing.Id = this.SelectedItemPricing.base_PricingManager.Id;
                this.SelectedItemPricing.EndUpdate();
                App.WriteUserLog("Pricing", "User insterd a new pricing." + this.SelectedItemPricing.base_PricingManager.Id);
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
                //To load data of product.
                if (this.SelectedItemPricing.ProductCollection == null)
                {
                    if (this.SelectedItemPricing.AffectPricing == 0)
                        this.LoadPricingChangeWithProduct();
                    else
                        this.LoadPricingChange(this.SelectedItemPricing.base_PricingManager.base_PricingChange.Select(x => x.ProductId), string.Empty);
                }
                this.SelectedItemPricing.Shift = Define.ShiftCode;
                //To update data on base_PriceManager table.
                this.SelectedItemPricing.Status = Common.PricingStatus.SingleOrDefault(x => x.Value == 2).Text;
                this.SelectedItemPricing.DateApplied = DateTimeExt.Today;
                if (Define.USER != null)
                    this.SelectedItemPricing.UserApplied = Define.USER.LoginName;
                else
                    this.SelectedItemPricing.UserApplied = Define.USER.LoginName;
                this.SelectedItemPricing.ToEntity();
                this._productRepository.Commit();
                //To update price of product.
                foreach (base_ProductModel item in this.SelectedItemPricing.ProductCollection)
                {
                    base_Product product = this._productRepository.GetIQueryable(x => x.Id == item.Id).SingleOrDefault();
                    if (product != null)
                    {
                        //To update price of product.
                        if (this.SelectedItemPricing.BasePrice == 1)
                            item.AverageUnitCost = item.NewPrice;
                        else
                            this.SetProductPrice(product, item.NewPrice);
                        this._productRepository.Commit();
                    }
                }
                this.SelectedItemPricing.VisibilityRestore = Visibility.Visible;
                this.SelectedItemPricing.VisibilityApplied = Visibility.Collapsed;
                this.SelectedItemPricing.EndUpdate();
                App.WriteUserLog("Pricing", "User insterd a new pricing." + this.SelectedItemPricing.Id);
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
                this.SelectedItemPricing.ItemCount = this.SelectedItemPricing.ProductCollection.Count();
                this.SelectedItemPricing.ToEntity();
                if (this.SelectedItemPricing.IsChangeProductCollection)
                {
                    _pricingChangeRepository.Delete(this.SelectedItemPricing.base_PricingManager.base_PricingChange);//.Where(x => x.PricingManagerId == this.SelectedItemPricing.Id);
                    this.SelectedItemPricing.base_PricingManager.base_PricingChange.Clear();
                    _pricingChangeRepository.Commit();
                    foreach (var item in this.SelectedItemPricing.ProductCollection)
                    {
                        base_PricingChange base_PricingChange = new base_PricingChange();
                        base_PricingChange.PricingManagerId = this.SelectedItemPricing.base_PricingManager.Id;
                        base_PricingChange.PricingManagerResource = this.SelectedItemPricing.Resource.ToString();
                        base_PricingChange.ProductId = item.Id;
                        base_PricingChange.ProductResource = item.Resource.ToString();
                        base_PricingChange.Cost = item.AverageUnitCost;
                        base_PricingChange.CurrentPrice = item.CurrentPrice;
                        base_PricingChange.NewPrice = item.NewPrice;
                        base_PricingChange.PriceChanged = item.PriceChange;
                        base_PricingChange.DateCreated = DateTimeExt.Today;
                        this.SelectedItemPricing.base_PricingManager.base_PricingChange.Add(base_PricingChange);
                        item.EndUpdate();
                    }
                }
                this._pricingManagerRepository.Commit();
                this.SelectedItemPricing.EndUpdate();
                App.WriteUserLog("Pricing", "User insterd a new pricing." + this.SelectedItemPricing.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update" + ex.ToString());
            }
        }
        #endregion

        #region Restore
        /// <summary>
        /// To restore old price for product on base_Product table.
        /// </summary>
        private void Restore()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.SelectedItemPricing.UserApplied) && this.SelectedItemPricing.DateApplied.HasValue)
                {
                    //To load data of product.
                    if (this.SelectedItemPricing.ProductCollection == null)
                    {
                        if (this.SelectedItemPricing.AffectPricing == 0)
                            this.LoadPricingChangeWithProduct();
                        else
                            this.LoadPricingChange(this.SelectedItemPricing.base_PricingManager.base_PricingChange.Select(x => x.ProductId), string.Empty);
                    }
                    this.SelectedItemPricing.Shift = Define.ShiftCode;
                    this.SelectedItemPricing.Status = Common.PricingStatus.SingleOrDefault(x => x.Value == 3).Text;
                    //To update data on base_PriceManager table.
                    this.SelectedItemPricing.DateRestored = DateTimeExt.Today;
                    if (Define.USER != null)
                        this.SelectedItemPricing.UserRestored = Define.USER.LoginName;
                    else
                        this.SelectedItemPricing.UserRestored = "admin";
                    this.SelectedItemPricing.ToEntity();
                    //To update price of product.
                    foreach (base_ProductModel item in this.SelectedItemPricing.ProductCollection)
                    {
                        string productResource = item.Resource.ToString();
                        base_Product product = this._productRepository.GetIQueryable(x => x.Id == item.Id).SingleOrDefault();
                        base_PricingChange pricingChange = this.SelectedItemPricing.base_PricingManager.base_PricingChange.SingleOrDefault(x => x.ProductId == item.Id && x.ProductResource.Equals(productResource));
                        if (pricingChange != null)
                        {
                            //To update price of product.
                            if (this.SelectedItemPricing.BasePrice == 1)
                                product.AverageUnitCost = pricingChange.Cost.Value;
                            else
                                this.RestoreProductPrice(product, pricingChange);
                            this._productRepository.Commit();
                        }
                    }
                    this.SelectedItemPricing.IsEnable = false;
                    this.SelectedItemPricing.EndUpdate();
                    App.WriteUserLog("Pricing", "User insterd a new pricing." + this.SelectedItemPricing.Id);
                }
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
            if (this.SelectedItemPricing == null)
                return false;

            return ((this.SelectedItemPricing.IsDirty || this.SelectedItemPricing.IsChangeProductCollection));
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

        #region SetProductPrice
        /// <summary>
        /// To set type of price 
        /// </summary>
        /// <param name="priceLevel">it is string of priceLevel.</param>
        /// <param name="product">It is a item of product.</param>
        /// <returns></returns>
        private void SetProductPrice(base_Product product, decimal NewPrice)
        {
            if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 1)
                product.RegularPrice = NewPrice;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 2)
                product.Price1 = NewPrice;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 4)
                product.Price2 = NewPrice;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 8)
                product.Price3 = NewPrice;
            else
                product.Price4 = NewPrice;
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
            if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 1)
                product.RegularPrice = pricingChange.CurrentPrice.Value;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 2)
                product.Price1 = pricingChange.CurrentPrice.Value;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 4)
                product.Price2 = pricingChange.CurrentPrice.Value;
            else if (Common.PriceSchemas.SingleOrDefault(x => x.Text.Equals(this.SelectedItemPricing.PriceLevel)).Value == 8)
                product.Price3 = pricingChange.CurrentPrice.Value;
            else
                product.Price4 = pricingChange.CurrentPrice.Value;
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
        private void SetPricingDetail()
        {
            if (this.SelectedItemPricing != null)
            {
                this.SelectedItemPricing.ProductCollection.Clear();
                //To set Affect pricing.
                if (this.SelectedItemPricing.AffectPricing == 0)
                    this._typeAffectPricing = "Default";
                else if (this.SelectedItemPricing.AffectPricing == 1)
                    this._typeAffectPricing = "Category";
                else if (this.SelectedItemPricing.AffectPricing == 2)
                    this._typeAffectPricing = "Vendor";
                else
                    this._typeAffectPricing = "Custom";
                if (this.SelectedItemPricing.AffectPricing == 0)
                    this.LoadPricingChangeWithProduct();
                else
                    this.LoadPricingChange(this.SelectedItemPricing.base_PricingManager.base_PricingChange.Select(x => x.ProductId), string.Empty);
                this.TotalProducts = this.SelectedItemPricing.ProductCollection.Count();
                //To set visibility of Button.
                if (this.SelectedItemPricing.DateApplied.HasValue)
                {
                    this.SelectedItemPricing.VisibilityApplied = Visibility.Collapsed;
                    this.SelectedItemPricing.VisibilityRestore = Visibility.Visible;
                }
                if (this.SelectedItemPricing.DateApplied.HasValue || this.SelectedItemPricing.DateRestored.HasValue)
                    this.SelectedItemPricing.IsEnable = false;

                // Load resource note collection
                this.LoadResourceNoteCollection(this.SelectedItemPricing);
                StickyManagementViewModel.SetParentResource(SelectedItemPricing.Resource.ToString(), SelectedItemPricing.ResourceNoteCollection);

                this.SelectedItemPricing.IsErrorProductCollection = false;
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
            this.POSConfig = Define.CONFIGURATION;
            this.CommissionTypes = new ObservableCollection<ComboItem>();
            this.CommissionTypes.Add(new ComboItem { Value = 0, Text = "%" });
            this.CommissionTypes.Add(new ComboItem { Value = 1, Text = POSConfig.CurrencySymbol });
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

        #region Permission

        #region Properties

        private bool _allowAddPricing = true;
        /// <summary>
        /// Gets or sets the AllowAddPricing.
        /// </summary>
        public bool AllowAddPricing
        {
            get { return _allowAddPricing; }
            set
            {
                if (_allowAddPricing != value)
                {
                    _allowAddPricing = value;
                    OnPropertyChanged(() => AllowAddPricing);
                }
            }
        }

        #endregion

        /// <summary>
        /// Get permissions
        /// </summary>
        public override void GetPermission()
        {
            if (!IsAdminPermission)
            {
                if (IsFullPermission)
                {
                    // Set default permission
                    AllowAddPricing = IsMainStore;
                }
                else
                {
                    // Get all user rights
                    IEnumerable<string> userRightCodes = Define.USER_AUTHORIZATION.Select(x => x.Code);

                    // Get edit quantity permission
                    AllowAddPricing = userRightCodes.Contains("IV100-02-01") && IsMainStore;
                }
            }
        }

        #endregion
    }
}