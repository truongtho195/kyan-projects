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

namespace CPC.POS.ViewModel
{
    class CountSheetViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        public RelayCommand<object> NewCommand { get; private set; }
        public RelayCommand<object> EditCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand RestoreCommand { get; private set; }
        public RelayCommand<object> FilterProductCommand { get; private set; }

        //Repository
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
        //To get type of Affectpricing.
        private string _typeAffectPricing = string.Empty;
        private base_CountStockModel SelectedCountStockClone { get; set; }
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
                //if (this.NotePopupCollection.Count == 0)
                //    return "Show Stickies";
                //else if (this.NotePopupCollection.Count == this.SelectedCountStock.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                //    return "Hide Stickies";
                //else
                return "Show Stickies";
            }
        }

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
            //To set enable of detail grid.
            if (param != null)
                this.IsSearchMode = !this.IsSearchMode;
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
                this.SelectedCountStock.DateCreated = DateTimeExt.Today;
                this.SelectedCountStock.CountStockDetailCollection = new ObservableCollection<base_CountStockDetailModel>();
                this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count;
                Expression<Func<base_ProductStore, bool>> predicate = PredicateBuilder.True<base_ProductStore>();
                this.LoadProductWithStore(predicate, true);
                this.SelectedCountStock.IsDirty = false;
                this.SelectedCountStock.IsLoad = false;
                this.SelectedCountStock.IsChangeProductCollection = false;
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
            // TODO: Handle command logic here
            if (this.SelectedCountStock.IsNew)
            {
                this.Insert();
                this.CountStockCollection.Add(this.SelectedCountStock);
            }
            else
                this.Update();
            this.SelectedCountStock.EndUpdate();
            this.SelectedCountStock.IsChangeProductCollection = false;
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
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && this.SearchOption == 0)//Search All
                {
                    Expression<Func<base_CountStock, bool>> predicate = this.CreateSearchPredicate(Keyword);
                    this.LoadCountSheet(predicate, false, 0);
                }
                else if (param != null)
                {
                    this.Keyword = param.ToString();
                    if (this.SearchOption == 0)
                    {
                        //Thong bao Can co dk
                        this.SearchAlert = "Search Option is required";
                    }
                    //Notification when search option is 1 and date is not format.
                    else if ((this.SearchOption == 4 || this.SearchOption == 8 || this.SearchOption == 12) && !this.IsDateTime())
                        this.SearchAlert = "DateTime is not format";
                    else
                    {
                        Expression<Func<base_CountStock, bool>> predicate = this.CreateSearchPredicate(Keyword);
                        this.LoadCountSheet(predicate, false, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        private bool IsDateTime()
        {
            return (!this.Keyword.Equals("1/1/0001") && this.Keyword.Length > 8);
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
                this.SelectedCountStock = null;
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
            return (this.SelectedCountStock != null && !this.SelectedCountStock.IsDirty && !this.SelectedCountStock.IsNew && this.SelectedCountStock.Status == 2);
        }
        /// <summary>
        /// Method to invoke when the ApplyCommand command is executed.
        /// </summary>
        private void OnApplyCommandExecute()
        {
            //To close product view
            if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
            {
                MessageBox.Show("When you adjust product in stores, You should close product view.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string content = string.Format("Do you want to adjust numbers of products in stocks?");
            MessageBoxResult msgResult = MessageBox.Show(content, "POS", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msgResult.Is(MessageBoxResult.Yes))
                this.Apply();
        }
        #endregion

        #region NewNoteCommand

        /// <summary>
        /// Gets the NewNoteCommand command.
        /// </summary>
        public RelayCommand NewNoteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewNoteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewNoteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewNoteCommand command is executed.
        /// Create and show a new note
        /// </summary>
        private void OnNewNoteCommandExecute()
        {
            //if (this.SelectedCountStock.ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
            //    return;

            //// Create a new note
            //base_ResourceNoteModel noteModel = new base_ResourceNoteModel
            //{
            //    Resource = this.SelectedCountStock.Resource.ToString(),
            //    Color = Define.DefaultColorNote,
            //    DateCreated = DateTimeExt.Today
            //};

            //// Create default position for note
            //Point position = new Point(600, 200);
            //if (this.SelectedCountStock.ResourceNoteCollection.Count > 0)
            //{
            //    Point lastPostion = this.SelectedCountStock.ResourceNoteCollection.LastOrDefault().Position;
            //    if (lastPostion != null)
            //        position = new Point(lastPostion.X + 10, lastPostion.Y + 10);
            //}

            //// Update position
            //noteModel.Position = position;

            //// Add new note to collection
            //this.SelectedCountStock.ResourceNoteCollection.Add(noteModel);

            //// Show new note
            //PopupContainer popupContainer = CreatePopupNote(noteModel);
            //popupContainer.Show();
            //NotePopupCollection.Add(popupContainer);
        }

        #endregion

        #region ShowOrHiddenNoteCommand

        /// <summary>
        /// Gets the ShowOrHiddenNoteCommand command.
        /// </summary>
        public RelayCommand ShowOrHiddenNoteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ShowOrHiddenNoteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShowOrHiddenNoteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ShowOrHiddenNoteCommand command is executed.
        /// </summary>
        private void OnShowOrHiddenNoteCommandExecute()
        {
            //if (NotePopupCollection.Count == this.SelectedCountStock.ResourceNoteCollection.Count)
            //{
            //    // Created popup notes, only show or hidden them
            //    if (ShowOrHiddenNote.Equals("Hide Stickies"))
            //    {
            //        foreach (PopupContainer popupContainer in NotePopupCollection)
            //            popupContainer.Visibility = Visibility.Collapsed;
            //    }
            //    else
            //    {
            //        foreach (PopupContainer popupContainer in NotePopupCollection)
            //            popupContainer.Show();
            //    }
            //}
            //else
            //{
            //    // Close all note
            //    CloseAllPopupNote();

            //    Point position = new Point(600, 200);
            //    foreach (base_ResourceNoteModel noteModel in this.SelectedCountStock.ResourceNoteCollection)
            //    {
            //        noteModel.Position = position;
            //        PopupContainer popupContainer = CreatePopupNote(noteModel);
            //        popupContainer.Show();
            //        NotePopupCollection.Add(popupContainer);
            //        position = new Point(position.X + 10, position.Y + 10);
            //    }
            //}

            //// Update label "Show/Hidden Note"
            //OnPropertyChanged(() => ShowOrHiddenNote);
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
                MessageBoxResult result = MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo);
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

        private void InitialData()
        {
            this.LoadStore();
            this.NotePopupCollection = new ObservableCollection<PopupContainer>();
            this.NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };
        }

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
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (this.SearchOption.Has(SearchCountSheetOptions.SheetNo))
                    predicate = predicate.And(x => x.DocumentNo.ToLower().Contains(keyword.ToLower()));
                else if (SearchOption.Has(SearchCountSheetOptions.Status))
                {
                    IEnumerable<int> query = Common.CountStockStatus.Where(x => x.Text.ToLower().Contains(this.Keyword)).Select(x => Int32.Parse((x.Value).ToString()));
                    predicate = predicate.And(x => query.Contains(x.Status));
                }
                else if (SearchOption.Has(SearchCountSheetOptions.StartedDate))
                {
                    DateTime time;
                    DateTime.TryParse(this.Keyword, out time);
                    if (time != DateTime.Parse("1/1/0001"))
                        predicate = predicate.And(x => x.DateCreated == time);
                }
                else if (SearchOption.Has(SearchCountSheetOptions.CompletedDate))
                {
                    DateTime time;
                    DateTime.TryParse(this.Keyword, out time);
                    if (time != DateTime.Parse("1/1/0001"))
                        predicate = predicate.And(x => x.CompletedDate == time);
                }
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
            //this.RestoreCommand = new RelayCommand(this.OnRestoreCommandExecute, this.OnRestoreCommandCanExecute);
            this.NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            this.ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
            this.DeleteCountStockDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
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
                //IEnumerable<int> CategoryIds = null;
                //IEnumerable<long> VendorIds = null;
                //Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
                //if (type.Equals("Vendor"))
                //{
                //    VendorIds = Conditions.Select(x => x.LongValue);
                //    predicate = predicate.And(x => VendorIds.Contains(x.VendorId));
                //}
                //else if (type.Equals("Category"))
                //{
                //    CategoryIds = Conditions.Select(x => x.IntValue);
                //    predicate = predicate.And(x => CategoryIds.Contains(x.ProductCategoryId));
                //}
                //else
                //{
                //    IEnumerable<long> productIDs = Conditions.Select(x => x.LongValue);
                //    predicate = predicate.And(x => productIDs.Contains(x.Id));
                //}
                //IEnumerable<base_Product> _products = _productRepository.GetIEnumerable(predicate);

                //if (this.SelectedCountStock.ProductCollection == null || this.SelectedCountStock.ProductCollection.Count == 0)
                //{
                //    foreach (var item in _products)
                //    {
                //        base_ProductModel _productModel = new base_ProductModel(item);
                //        this.SelectedCountStock.ProductCollection.Add(_productModel);
                //    }
                //}
                //else
                //{
                //    foreach (var item in _products)
                //    {
                //        if (!this.SelectedCountStock.ProductCollection.Select(x => x.Id).Contains(item.Id))
                //        {
                //            base_ProductModel _productModel = new base_ProductModel(item);
                //            this.SelectedCountStock.ProductCollection.Add(_productModel);
                //        }
                //    }
                //}
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
                //Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();
                //predicate = predicate.And(x => Conditions.Contains(x.Id));
                //IEnumerable<base_Product> _products = _productRepository.GetIEnumerable(predicate);
                //foreach (var item in _products)
                //{
                //    base_PricingChange pricingChange = this.SelectedCountStock.base_PricingManager.base_PricingChange.ToList().SingleOrDefault(x => x.ProductId == item.Id);
                //    base_ProductModel _productModel = new base_ProductModel(item);
                //    _productModel.CurrentPrice = pricingChange.CurrentPrice.Value;
                //    _productModel.NewPrice = pricingChange.NewPrice.Value;
                //    _productModel.PriceChange = pricingChange.PriceChanged.Value;
                //    _productModel.EndUpdate();
                //    this.SelectedCountStock.ProductCollection.Add(_productModel);
                //}
                //this.SelectedCountStock.EndUpdate();
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
        private void LoadCountSheet(Expression<Func<base_CountStock, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                this.CountStockCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                base.IsBusy = true;
                //if(refreshData)
                //To count all User in Data base show on grid
                this.TotalCountSheet = _countStockRepository.GetIQueryable().Count();
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * base.NumberOfDisplayItems;
                IList<base_CountStock> pricings = _countStockRepository.GetRange(indexItem, base.NumberOfDisplayItems, "It.Id", predicate);
                foreach (var item in pricings)
                    bgWorker.ReportProgress(0, item);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                //To add item.
                base_CountStockModel model = new base_CountStockModel(e.UserState as base_CountStock);
                if (model.Status == 3)
                    model.IsEnable = false;
                this.CountStockCollection.Add(model);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (this.SelectedCountStockClone != null && !this.IsSearchMode)
                {
                    this.SelectedCountStock = this.CountStockCollection.SingleOrDefault(x => x.Id == this.SelectedCountStockClone.Id);
                    this.SetCountSheetDetail();
                    this.SelectedCountStockClone = null;
                }
                base.IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }
        private void LoadProductWithStore(Expression<Func<base_ProductStore, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            // BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                this.SelectedCountStock.CountStockDetailCollection.Clear();
            //bgWorker.DoWork += (sender, e) =>
            //{
            //    base.IsBusy = true;
            //if(refreshData)
            //To add item.
            IEnumerable<base_ProductStore> productStores = _productStoreRepository.GetAll(predicate).OrderBy(x => x.StoreCode);
            foreach (var productStore in productStores)
            //};
            //bgWorker.ProgressChanged += (sender, e) =>
            {
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
                model.EndUpdate();
                model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                this.SelectedCountStock.CountStockDetailCollection.Add(model);
            }
            //};
            //bgWorker.RunWorkerCompleted += (sender, e) =>
            //{
            //To count all User in Data base show on grid
            this.TotalProducts = this.SelectedCountStock.CountStockDetailCollection.Count;
            //    base.IsBusy = false;
            //};
            //bgWorker.RunWorkerAsync();
        }
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!this.SelectedCountStock.IsLoad)
                switch (e.PropertyName)
                {
                    case "CountedQuantity":
                        this.SelectedCountStock.IsDirty = true;
                        this.SelectedCountStock.IsChangeProductCollection = true;
                        break;
                }
        }
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
                msgResult = MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (this.OnSaveCommandCanExecute())
                    {
                        this.OnSaveCommandExecute();
                        result = true;
                    }
                    else //Has Error
                        result = false;

                    // Remove popup note
                    this.CloseAllPopupNote();
                }
                else
                {
                    if (this.SelectedCountStock.IsNew)
                    {
                        this.DeleteNote();
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                    }
                    else //Old Item Rollback data
                    {
                        this.DeleteNote();
                        this.RollBackCountSheet();
                    }
                }
            }
            else
            {
                this.SelectedCountStockClone = null;
                if (this.SelectedCountStock != null && this.SelectedCountStock.IsNew)
                {
                    this.DeleteNote();
                    this.IsSearchMode = true;
                }
                else
                {
                    if (this.SelectedCountStock != null && !this.SelectedCountStock.IsNew && !this.IsSearchMode)
                        this.SelectedCountStockClone = this.SelectedCountStock;
                    // Remove popup note
                    this.CloseAllPopupNote();
                }
            }
            if (isClosing.HasValue && isClosing.Value)
                this.SelectedCountStockClone = null;
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
                this.SelectedCountStock.Status = 2;
                this.SelectedCountStock.ToEntity();
                foreach (var item in this.SelectedCountStock.CountStockDetailCollection)
                {
                    item.ToEntity();
                    this.SelectedCountStock.base_CountStock.base_CountStockDetail.Add(item.base_CountStockDetail);
                    item.EndUpdate();
                }
                this._countStockRepository.Add(this.SelectedCountStock.base_CountStock);
                this._countStockRepository.Commit();
                this.SelectedCountStock.Id = this.SelectedCountStock.base_CountStock.Id;
                this.SelectedCountStock.EndUpdate();
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
                this.SelectedCountStock.UserCounted = Define.USER.LoginName;
                this.SelectedCountStock.CompletedDate = DateTimeExt.Today;
                this.SelectedCountStock.Status = 3;
                this.SelectedCountStock.ToEntity();
                foreach (var item in this.SelectedCountStock.base_CountStock.base_CountStockDetail)
                {
                    this._productRepository.UpdateOnHandQuantity(item.ProductResource, item.StoreId, item.Quantity, true);
                    this._productRepository.UpdateOnHandQuantity(item.ProductResource, item.StoreId, item.CountedQuantity);
                }
                this._pricingManagerRepository.Commit();
                this.SelectedCountStock.EndUpdate();
                this.SelectedCountStock.IsEnable = false;
                this.SelectedCountStock.IsLoad = false;
                this.SelectedCountStock.IsChangeProductCollection = false;
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
                MessageBoxResult result = MessageBox.Show("Do you want to lock this account?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                this.SelectedCountStock.Status = 2;
                this.SelectedCountStock.ToEntity();
                if (this.SelectedCountStock.IsChangeProductCollection)
                {
                    _countStockDetailRepository.Delete(this.SelectedCountStock.base_CountStock.base_CountStockDetail);
                    this.SelectedCountStock.base_CountStock.base_CountStockDetail.Clear();
                    _pricingChangeRepository.Commit();
                    foreach (var item in this.SelectedCountStock.CountStockDetailCollection)
                    {
                        item.ToEntity();
                        this.SelectedCountStock.base_CountStock.base_CountStockDetail.Add(item.base_CountStockDetail);
                        item.EndUpdate();
                    }
                }
                this._pricingManagerRepository.Commit();
                this.SelectedCountStock.EndUpdate();
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
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;
            // Check data is edited
            if (this.IsEditData())
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
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

                    // Remove popup note
                    this.CloseAllPopupNote();
                }
                else
                {
                    if (this.SelectedCountStock.IsNew)
                    {
                        this.DeleteNote();
                        //SelectedProduct = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                    }
                    else
                    {
                        //Remove popup note
                        this.CloseAllPopupNote();
                        this.RollBackCountSheet();
                    }
                }
            }
            else
            {
                if (this.SelectedCountStock != null && this.SelectedCountStock.IsNew)
                    this.DeleteNote();
                else
                    // Remove popup note
                    this.CloseAllPopupNote();
            }

            // Clear selected item
            if (result && isClosing == null)
                this.SelectedCountStock = null;

            return result;
        }
        #endregion

        #region RollBackCountSheet
        //To return data when users dont save them.
        private void RollBackCountSheet()
        {
            this.SelectedCountStock.ToModelAndRaise();
        }
        #endregion

        #region SetCountSheetDetail
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
                        model.Description = product.Description;
                        model.ProductName = product.ProductName;
                        model.ProductCode = product.Code;
                        model.EndUpdate();
                        this.SelectedCountStock.CountStockDetailCollection.Add(model);
                    }
                }
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
            if (!string.IsNullOrWhiteSpace(Keyword) && SearchOption > 0)
            {
                predicate = this.CreateSearchPredicate(Keyword);
            }
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

        #region Note Methods

        /// <summary>
        /// Load notes
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadNote(base_PricingManagerModel pricingChangeModel)
        {
            // Load Note
            if (pricingChangeModel.ResourceNoteCollection == null)
            {
                string resource = pricingChangeModel.Resource.ToString();
                pricingChangeModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        /// <summary>
        /// Create or update note
        /// </summary>
        /// <param name="noteModel"></param>
        private void SaveNote(base_ResourceNoteModel noteModel)
        {
            noteModel.ToEntity();
            if (noteModel.IsNew)
                _resourceNoteRepository.Add(noteModel.base_ResourceNote);
            _resourceNoteRepository.Commit();
            noteModel.EndUpdate();
        }

        /// <summary>
        /// Delete and close popup notes
        /// </summary>
        private void DeleteNote()
        {
            //// Remove popup note
            //CloseAllPopupNote();

            //// Delete note
            //foreach (base_ResourceNoteModel noteModel in this.SelectedCountStock.ResourceNoteCollection)
            //    _resourceNoteRepository.Delete(noteModel.base_ResourceNote);
            //_resourceNoteRepository.Commit();

            //this.SelectedCountStock.ResourceNoteCollection.Clear();
        }

        /// <summary>
        /// Close popup notes
        /// </summary>
        private void CloseAllPopupNote()
        {
            if (NotePopupCollection != null)
            {
                // Remove popup note
                foreach (PopupContainer popupContainer in NotePopupCollection)
                    popupContainer.Close();
                NotePopupCollection.Clear();
            }
        }

        /// <summary>
        /// Create popup note
        /// </summary>
        /// <param name="noteModel"></param>
        /// <returns></returns>
        private PopupContainer CreatePopupNote(base_ResourceNoteModel noteModel)
        {
            //NoteViewModel noteViewModel = new NoteViewModel();
            //noteViewModel.SelectedNote = noteModel;
            //noteViewModel.NotePopupCollection = NotePopupCollection;
            //noteViewModel.ResourceNoteCollection = this.SelectedCountStock.ResourceNoteCollection;
            //CPC.POS.View.NoteView noteView = new View.NoteView();

            //PopupContainer popupContainer = new PopupContainer(noteView, true);
            //popupContainer.WindowStartupLocation = WindowStartupLocation.Manual;
            //popupContainer.DataContext = noteViewModel;
            //popupContainer.Width = 150;
            //popupContainer.Height = 120;
            //popupContainer.MinWidth = 150;
            //popupContainer.MinHeight = 120;
            //popupContainer.FormBorderStyle = PopupContainer.BorderStyle.None;
            //popupContainer.Deactivated += (sender, e) => { SaveNote(noteModel); };
            //popupContainer.Loaded += (sender, e) =>
            //{
            //    popupContainer.Left = noteModel.Position.X;
            //    popupContainer.Top = noteModel.Position.Y;
            //};
            //return popupContainer;
            return null;
        }

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
    }
}
