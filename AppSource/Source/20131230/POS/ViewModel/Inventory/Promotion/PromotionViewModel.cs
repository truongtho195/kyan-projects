using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;

namespace CPC.POS.ViewModel
{
    public class PromotionViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Defines

        private base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        #endregion

        #region Properties

        #region Search

        private bool isSearchMode = false;
        /// <summary>
        /// Gets or sets a value that indicates whether the grid-search is open.
        /// </summary>
        /// <returns>true if open grid-search; otherwise, false.</returns>
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


        #region Keyword
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
                    ResetTimer();
                    OnPropertyChanged(() => Keyword);
                }
            }
        }
        #endregion


        #endregion

        private ObservableCollection<base_PromotionModel> _promotionCollection = new ObservableCollection<base_PromotionModel>();
        /// <summary>
        /// Gets or sets the PromotionCollection.
        /// </summary>
        public ObservableCollection<base_PromotionModel> PromotionCollection
        {
            get { return _promotionCollection; }
            set
            {
                if (_promotionCollection != value)
                {
                    _promotionCollection = value;
                    OnPropertyChanged(() => PromotionCollection);
                }
            }
        }

        private base_PromotionModel _selectedPromotion;
        /// <summary>
        /// Gets or sets the SelectedPromotion.
        /// </summary>
        public base_PromotionModel SelectedPromotion
        {
            get { return _selectedPromotion; }
            set
            {
                if (_selectedPromotion != value)
                {
                    _selectedPromotion = value;
                    OnPropertyChanged(() => SelectedPromotion);
                }
            }
        }

        private ObservableCollection<ComboItem> _categoryCollection;
        /// <summary>
        /// Gets or sets the CategoryCollection.
        /// </summary>
        public ObservableCollection<ComboItem> CategoryCollection
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

        private ObservableCollection<ComboItem> _vendorCollection;
        /// <summary>
        /// Gets or sets the VendorCollection.
        /// </summary>
        public ObservableCollection<ComboItem> VendorCollection
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

        private short _status;
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public short Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(() => Status);
                if (SelectedPromotion != null)
                {
                    if (OnStatusChanged(SelectedPromotion))
                        SelectedPromotion.Status = Status;
                }
            }
        }

        private ObservableCollection<ComboItem> _promotionTypes = new ObservableCollection<ComboItem>();
        /// <summary>
        /// Gets or sets the PromotionTypes.
        /// </summary>
        public ObservableCollection<ComboItem> PromotionTypes
        {
            get { return _promotionTypes; }
            set
            {
                if (_promotionTypes != value)
                {
                    _promotionTypes = value;
                    OnPropertyChanged(() => PromotionTypes);
                }
            }
        }

        private ObservableCollection<ComboItem> _takeOffOptions = new ObservableCollection<ComboItem>();
        /// <summary>
        /// Gets or sets the TakeOffOptions.
        /// </summary>
        public ObservableCollection<ComboItem> TakeOffOptions
        {
            get { return _takeOffOptions; }
            set
            {
                if (_takeOffOptions != value)
                {
                    _takeOffOptions = value;
                    OnPropertyChanged(() => TakeOffOptions);
                }
            }
        }

        #region ListCheckBox PriceSchema

        private ObservableCollection<CheckBoxItemModel> _priceSchemaCollection;
        /// <summary>
        /// Gets or sets the PriceSchemaCollection.
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> PriceSchemaCollection
        {
            get { return _priceSchemaCollection; }
            set
            {
                if (_priceSchemaCollection != value)
                {
                    _priceSchemaCollection = value;
                    OnPropertyChanged(() => PriceSchemaCollection);
                }
            }
        }

        public bool IsDirtyPriceSchemaCollection { get; set; }

        public bool IsCheckedPriceSchemaCollection
        {
            get
            {
                if (PriceSchemaCollection == null)
                    return true;
                return PriceSchemaCollection.Count(x => x.IsChecked) > 0;
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the PromotionViewModel class.
        /// </summary>
        public PromotionViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            // Load static data
            LoadStaticData();

            InitialCommand();

            //Initial Auto Complete Search
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
        }

        /// <summary>
        /// Initializes a new instance of the PromotionViewModel class with parameter.
        /// </summary>
        /// <param name="isList">true if show list, otherwise, false.</param>
        public PromotionViewModel(bool isList, object param = null)
            : this()
        {
            ChangeSearchMode(isList, param);
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
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                //_keyword = param.ToString();
                if (_waitingTimer != null)
                    _waitingTimer.Stop();

                // Load data by predicate
                LoadDataByPredicate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region NewCommand

        /// <summary>
        /// Gets the NewCommand command.
        /// </summary>
        public ICommand NewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return UserPermissions.AllowAddPromotion;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (ShowNotification(null))
                NewPromotion();
            this.IsSearchMode = false;
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

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the EditCommand command is executed.
        /// </summary>
        private void OnEditCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            // Edit selected item
            OnDoubleClickViewCommandExecute(dataGridControl.SelectedItem);
        }

        #endregion

        #region SaveCommand

        /// <summary>
        /// Gets the SaveCommand command.
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return IsValid && IsEdit() && IsCheckedPriceSchemaCollection;
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SavePromotion();
        }

        #endregion

        #region DeleteCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (SelectedPromotion == null)
                return false;
            return !IsEdit() && !SelectedPromotion.IsNew;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this promotion?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                try
                {
                    if (SelectedPromotion.IsNew)
                    {
                        SelectedPromotion = null;
                    }
                    else if (IsValid)
                    {
                        // Delete promotion
                        _promotionRepository.Delete(SelectedPromotion.base_Promotion);

                        // Accept changes
                        _promotionRepository.Commit();

                        // Turn off IsDirty & IsNew
                        SelectedPromotion.EndUpdate();

                        // Remove promotion from collection
                        PromotionCollection.Remove(SelectedPromotion);

                        SelectedPromotion = null;
                    }
                    else
                        return;

                    IsSearchMode = true;
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region DeletesCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeletesCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeletesCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            DataGridControl dataGridControl = param as DataGridControl;

            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this promotion?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult.Is(MessageBoxResult.Yes))
            {
                try
                {
                    foreach (base_PromotionModel promotionItem in dataGridControl.SelectedItems.Cast<base_PromotionModel>().ToList())
                    {
                        // Delete promotion
                        _promotionRepository.Delete(promotionItem.base_Promotion);

                        // Accept changes
                        _promotionRepository.Commit();

                        // Turn off IsDirty & IsNew
                        promotionItem.EndUpdate();

                        // Remove promotion from collection
                        PromotionCollection.Remove(promotionItem);
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region DoubleClickViewCommand

        /// <summary>
        /// Gets the DoubleClickViewCommand command.
        /// </summary>
        public ICommand DoubleClickViewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DoubleClickViewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DoubleClickViewCommand command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            if (param != null && IsSearchMode)
            {
                SelectedPromotion = param as base_PromotionModel;

                // Load PriceSchemaRange
                OnCheckPriceSchema();

                // Update Status
                if (SelectedPromotion.Status == (short)StatusBasic.Active)
                {
                    if (SelectedPromotion.ExpirationNoEndDate && SelectedPromotion.EndDate.Value < DateTimeExt.Now)
                    {
                        Status = (short)StatusBasic.Deactive;
                        SavePromotion();
                    }
                }

                _status = SelectedPromotion.Status;
                OnPropertyChanged(() => Status);

                IsSearchMode = false;
            }
            else if (!IsSearchMode)
            {
                if (ShowNotification(null))
                    IsSearchMode = true;
            }
            else
                // Show detail form
                IsSearchMode = false;
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
            LoadDataByPredicate();
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
            if (_waitingTimer != null)
                _waitingTimer.Stop();

            PromotionAdvanceSearchViewModel viewModel = new PromotionAdvanceSearchViewModel(PromotionTypes);
            bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PromotionAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue)
            {
                if (msgResult.Value)
                {
                    // Create basic predicate combine with advance predicate
                    Expression<Func<base_Promotion, bool>> predicate = CreateSearchPredicate(_keyword);
                    predicate = predicate.And(viewModel.AdvanceSearchPredicate);

                    // Load data by search predicate
                    LoadDataByPredicate(predicate, false, 0);
                }
            }
        }

        #endregion

        #region NewNoteCommand

        /// <summary>
        /// Gets the NewNoteCommand command.
        /// </summary>
        public ICommand NewNoteCommand { get; private set; }

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
        /// </summary>
        private void OnNewNoteCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion

        #region ShowOrHiddenNoteCommand

        /// <summary>
        /// Gets the ShowOrHiddenNoteCommand command.
        /// </summary>
        public ICommand ShowOrHiddenNoteCommand { get; private set; }

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
            // TODO: Handle command logic here
        }

        #endregion

        #region ChangeStatusCommand

        /// <summary>
        /// Gets the ChangeStatusCommand command.
        /// </summary>
        public ICommand ChangeStatusCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ChangeStatusCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChangeStatusCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the ChangeStatusCommand command is executed.
        /// </summary>
        private void OnChangeStatusCommandExecute(object param)
        {
            try
            {
                // Convert param to DataGridControl
                DataGridControl dataGridControl = param as DataGridControl;

                // Get promotion model
                base_PromotionModel promotionModel = dataGridControl.SelectedItem as base_PromotionModel;

                // Update status value in ViewModel
                if (promotionModel.Status.Equals((short)StatusBasic.Active))
                    _status = (short)StatusBasic.Deactive;
                else
                    _status = (short)StatusBasic.Active;
                OnPropertyChanged(() => Status);

                // Process when status changed
                if (OnStatusChanged(promotionModel))
                {
                    promotionModel.DateUpdated = DateTimeExt.Now;
                    if (Define.USER != null)
                        promotionModel.UserUpdated = Define.USER.LoginName;

                    // Update status promotion
                    promotionModel.Status = Status;

                    // Map data from model to entity
                    promotionModel.ToEntity();

                    // Accept changes
                    _promotionRepository.Commit();

                    // Turn off IsDirty & IsNew
                    promotionModel.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
            try
            {
                // Load category collection
                if (CategoryCollection == null)
                {
                    // Get all department that status is actived
                    IQueryable<int> departmentIDList = _departmentRepository.
                        GetIQueryable(x => x.LevelId == 0 && x.IsActived == true).Select(x => x.Id);

                    // Get all category that contain in department list
                    CategoryCollection = new ObservableCollection<ComboItem>(_departmentRepository.
                        GetIQueryable(x => x.IsActived == true && x.LevelId == 1 && departmentIDList.Contains(x.ParentId.Value)).
                        OrderBy(x => x.Name).
                        Select(x => new ComboItem { IntValue = x.Id, Text = x.Name }));
                }

                // Load vendor collection
                if (VendorCollection == null)
                {
                    string markType = MarkType.Vendor.ToDescription();
                    VendorCollection = new ObservableCollection<ComboItem>(_guestRepository.
                        GetAll(x => x.Mark.Equals(markType) && !x.IsPurged && x.IsActived).
                        OrderBy(x => x.Company).
                        Select(x => new ComboItem { LongValue = x.Id, Text = x.Company }));
                }

                Common.Refresh();

                // Load PriceSchemaCollection
                PriceSchemaCollection = new ObservableCollection<CheckBoxItemModel>();
                foreach (ComboItem comboItem in Common.PriceSchemas)
                {
                    CheckBoxItemModel checkBoxItemModel = new CheckBoxItemModel(comboItem);
                    checkBoxItemModel.PropertyChanged += new PropertyChangedEventHandler(checkBoxItemModel_PropertyChanged);
                    PriceSchemaCollection.Add(checkBoxItemModel);
                }

                // Load PromotionTypes
                PromotionTypes.Clear();
                foreach (ComboItem comboItem in Common.PromotionTypes)
                {
                    if (comboItem.Text.Contains("$"))
                        comboItem.Text = comboItem.Text.Replace("$", Define.CONFIGURATION.CurrencySymbol);
                    PromotionTypes.Add(comboItem);
                }

                // Load TakeOffOptions
                TakeOffOptions.Clear();
                foreach (ComboItem comboItem in Common.TakeOffOptions)
                {
                    if (comboItem.Text.Contains("$"))
                        comboItem.Text = comboItem.Text.Replace("$", Define.CONFIGURATION.CurrencySymbol);
                    TakeOffOptions.Add(comboItem);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
            LoadStepCommand = new RelayCommand(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            PopupAdvanceSearchCommand = new RelayCommand<object>(OnPopupAdvanceSearchCommandExecute, OnPopupAdvanceSearchCommandCanExecute);
            DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
            ChangeStatusCommand = new RelayCommand<object>(OnChangeStatusCommandExecute, OnChangeStatusCommandCanExecute);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            if (SelectedPromotion == null)
                return false;

            return SelectedPromotion.IsDirty ||
                IsDirtyPriceSchemaCollection;
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <param name="isClosing">
        /// true if form is closing;
        /// false if form is changing;
        /// null if switch change search mode
        /// </param>
        /// <returns>true if continue action; otherwise, false.</returns>
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;

            // Check data is edited
            if (IsEdit())
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (msgResult.Is(MessageBoxResult.Cancel))
                {
                    return false;
                }
                else if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        result = SavePromotion();
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    if (SelectedPromotion.IsNew)
                    {
                        //DeleteNote();

                        SelectedPromotion = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else
                    {
                        // Refresh promotion datas
                        SelectedPromotion.ToModelAndRaise();
                        SelectedPromotion.EndUpdate();
                        LoadRelationData(SelectedPromotion);

                        // Load PriceSchemaRange
                        OnCheckPriceSchema();
                    }
                }
            }

            if (result && isClosing == null && SelectedPromotion != null && !SelectedPromotion.IsNew)
            {
                // Refresh promotion datas
                SelectedPromotion.ToModelAndRaise();
                SelectedPromotion.EndUpdate();
                LoadRelationData(SelectedPromotion);

                // Load PriceSchemaRange
                OnCheckPriceSchema();

                // Clear selected item
                SelectedPromotion = null;
            }

            return result;
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Promotion, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Promotion, bool>> predicate = PredicateBuilder.True<base_Promotion>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<base_Promotion>();

                // Get all promotion that keyword contain in Name
                predicate = predicate.Or(x => x.Name.ToLower().Contains(keyword));

                // Get all PromotionType that contain keyword
                IEnumerable<ComboItem> promotionTypes = Common.PromotionTypes.Where(x => x.Text.ToLower().Contains(keyword));
                IEnumerable<short> promotionTypeIDList = promotionTypes.Select(x => x.Value);

                // Get all promotion that contain in promotion type id list
                if (promotionTypeIDList.Count() > 0)
                    predicate = predicate.Or(x => promotionTypeIDList.Contains(x.PromotionTypeId));

                // Get all StatusItem that contain keyword
                IEnumerable<ComboItem> statusItems = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(keyword));
                IEnumerable<short> statusItemIDList = statusItems.Select(x => x.Value);

                // Get all promotion that contain in status item id list
                if (statusItemIDList.Count() > 0)
                    predicate = predicate.Or(x => statusItemIDList.Contains(x.Status));

                // Get all promotion that keyword contain in CouponBarCode
                predicate = predicate.Or(x => x.CouponBarCode.Contains(keyword));

                DateTime dateTimeKeyword = DateTimeExt.Now;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeKeyword))
                {
                    int year = dateTimeKeyword.Year;
                    int month = dateTimeKeyword.Month;
                    int day = dateTimeKeyword.Day;

                    // Get all product that keyword contain in StartDate
                    predicate = predicate.Or(x => x.StartDate.HasValue &&
                        x.StartDate.Value.Year.Equals(year) &&
                        x.StartDate.Value.Month.Equals(month) &&
                        x.StartDate.Value.Day.Equals(day));

                    // Get all product that keyword contain in EndDate
                    predicate = predicate.Or(x => x.EndDate.HasValue &&
                        x.EndDate.Value.Year.Equals(year) &&
                        x.EndDate.Value.Month.Equals(month) &&
                        x.EndDate.Value.Day.Equals(day));

                    // Get all product that keyword contain in DateCreated
                    predicate = predicate.Or(x => x.DateCreated.Year.Equals(year) &&
                        x.DateCreated.Month.Equals(month) && x.DateCreated.Day.Equals(day));
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
        /// <param name="currentIndex">index to load if index = 0 , clear collection</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Promotion, bool>> predicate = CreateSearchPredicate(_keyword);

            // Load data by predicate
            LoadDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index = 0 , clear collection</param>
        private void LoadDataByPredicate(Expression<Func<base_Promotion, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                PromotionCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                try
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    if (refreshData)
                    {
                        _promotionRepository.Refresh();
                        _departmentRepository.Refresh();
                        _guestRepository.Refresh();
                    }

                    // Get all promotions
                    IList<base_Promotion> promotions = _promotionRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.DateCreated, predicate);
                    foreach (base_Promotion promotion in promotions)
                    {
                        bgWorker.ReportProgress(0, promotion);
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create promotion model
                base_PromotionModel promotionModel = new base_PromotionModel((base_Promotion)e.UserState);

                // Load relation data
                LoadRelationData(promotionModel);

                // Add to collection
                PromotionCollection.Add(promotionModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for promotion
        /// </summary>
        /// <param name="promotionModel"></param>
        private void LoadRelationData(base_PromotionModel promotionModel)
        {
            // Get promotion type name
            if (string.IsNullOrWhiteSpace(promotionModel.PromotionTypeName))
            {
                ComboItem promotionType = PromotionTypes.FirstOrDefault(x => x.Value.Equals(promotionModel.PromotionTypeId));
                if (promotionType != null)
                    promotionModel.PromotionTypeName = promotionType.Text;
            }

            // Check ExpirationNoEndDate
            promotionModel.SetExpirationNoEndDate(promotionModel.EndDate != null);
        }

        /// <summary>
        /// Create a new promotion
        /// </summary>
        private void NewPromotion()
        {
            // Create a new promotion with default values
            SelectedPromotion = new base_PromotionModel();
            Status = Define.CONFIGURATION.DefaultDiscountStatus;
            SelectedPromotion.PromotionTypeId = Define.CONFIGURATION.DefaultDiscountType;
            SelectedPromotion.PriceSchemaRange = 0;
            SelectedPromotion.Description = string.Empty;
            SelectedPromotion.DateCreated = DateTimeExt.Now;
            SelectedPromotion.UserCreated = Define.USER.LoginName;
            SelectedPromotion.Resource = Guid.NewGuid();

            // Check PriceSchemaRange
            OnCheckPriceSchema();

            // Turn off IsDirty
            SelectedPromotion.IsDirty = false;
        }

        /// <summary>
        /// Process save promotion function
        /// </summary>
        /// <returns></returns>
        private bool SavePromotion()
        {
            try
            {
                // Update PriceSchemaRange value
                OnSavePriceSchema();

                // Create new promotion
                if (SelectedPromotion.IsNew)
                {
                    // Call function create new promotion
                    SaveNew();
                }
                else
                {
                    // Vendor is edited
                    // Call function update promotion
                    SaveUpdate();
                }

                // Get promotion type name
                ComboItem promotionType = PromotionTypes.FirstOrDefault(x => x.Value.Equals(SelectedPromotion.PromotionTypeId));
                if (promotionType != null)
                    SelectedPromotion.PromotionTypeName = promotionType.Text;

                // Turn off IsDirty & IsNew
                SelectedPromotion.EndUpdate();
                IsDirtyPriceSchemaCollection = false;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "POS", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return true;
        }

        /// <summary>
        /// Save when create new promotion
        /// </summary>
        private void SaveNew()
        {
            try
            {
                // Set shift
                SelectedPromotion.Shift = Define.ShiftCode;

                // Map data from model to entity
                SelectedPromotion.ToEntity();

                // Add new promotion to repository
                _promotionRepository.Add(SelectedPromotion.base_Promotion);

                // Accept changes
                _promotionRepository.Commit();

                // Update ID from entity to model
                SelectedPromotion.Id = SelectedPromotion.base_Promotion.Id;

                // Push new promotion to collection
                PromotionCollection.Insert(0, SelectedPromotion);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save when edit or update promotion
        /// </summary>
        private void SaveUpdate()
        {
            try
            {
                SelectedPromotion.DateUpdated = DateTimeExt.Now;
                if (Define.USER != null)
                    SelectedPromotion.UserUpdated = Define.USER.LoginName;

                // Map data from model to entity
                SelectedPromotion.ToEntity();

                // Accept changes
                _promotionRepository.Commit();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Process price schema value when check box is checked
        /// </summary>
        private void OnSavePriceSchema()
        {
            int priceSchemaRange = 0;
            foreach (CheckBoxItemModel checkBoxItemModel in PriceSchemaCollection.Where(x => x.IsChecked))
            {
                priceSchemaRange ^= checkBoxItemModel.Value;
            }
            SelectedPromotion.PriceSchemaRange = priceSchemaRange;
        }

        /// <summary>
        /// Process check box when load price schema
        /// </summary>
        private void OnCheckPriceSchema()
        {
            if (SelectedPromotion != null)
            {
                foreach (CheckBoxItemModel checkBoxItemModel in PriceSchemaCollection)
                {
                    if ((SelectedPromotion.PriceSchemaRange & checkBoxItemModel.Value) == checkBoxItemModel.Value)
                        checkBoxItemModel.IsChecked = true;
                    else
                        checkBoxItemModel.IsChecked = false;
                }
                IsDirtyPriceSchemaCollection = false;
            }
        }

        /// <summary>
        /// Show popup reason to reactive when change status
        /// </summary>
        private bool OnStatusChanged(base_PromotionModel promotionModel)
        {
            bool result = true;

            // Check required discount reason when reactive promotion
            if (Status == (short)StatusBasic.Active && !promotionModel.IsNew &&
                Define.CONFIGURATION.IsRequireDiscountReason == true)
            {
                PromotionReasonViewModel viewModel = new PromotionReasonViewModel(promotionModel.ReasonReActive);
                bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PromotionReasonView>(_ownerViewModel, viewModel, "Entry for reason");
                if (msgResult.HasValue)
                {
                    if (msgResult.Value)
                        promotionModel.ReasonReActive = viewModel.ReasonReactive;
                    else
                    {
                        result = false;
                        App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                        {
                            Status = (short)StatusBasic.Deactive;
                        });
                    }
                }
            }

            return result;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            if (SelectedPromotion != null && !SelectedPromotion.IsNew)
            {
                lock (UnitOfWork.Locker)
                {
                    // Refresh static data
                    CategoryCollection = null;
                    VendorCollection = null;

                    // Load static data
                    LoadStaticData();
                }

                // Refresh promotion datas
                SelectedPromotion.PromotionTypeId = 0;
                SelectedPromotion.TakeOffOption = -1;
                SelectedPromotion.ToModelAndRaise();
                SelectedPromotion.EndUpdate();
                LoadRelationData(SelectedPromotion);

                // Load PriceSchemaRange
                OnCheckPriceSchema();

                _status = SelectedPromotion.Status;
                OnPropertyChanged(() => Status);
            }

            // Load data by predicate
            LoadDataByPredicate();
        }

        /// <summary>
        /// Process when change display view
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ShowNotification(null))
                {
                    // When user clicked create new button
                    if (!isList)
                    {
                        // Create new promotion
                        NewPromotion();

                        // Display promotion detail
                        IsSearchMode = false;
                    }
                    else
                    {
                        // When user click view list button
                        // Display promotion list
                        IsSearchMode = true;
                    }
                }
            }
        }

        /// <summary>
        /// Process when changed view
        /// </summary>
        /// <param name="isClosing">Form is closing or changing</param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ShowNotification(isClosing);
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Set IsDirty for PriceSchemaCollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxItemModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsChecked"))
            {
                if (!IsDirtyPriceSchemaCollection)
                    IsDirtyPriceSchemaCollection = true;
                OnPropertyChanged(() => IsCheckedPriceSchemaCollection);
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
            if (Define.CONFIGURATION.IsAutoSearch && _waitingTimer != null)
            {
                this._waitingTimer.Stop();
                this._waitingTimer.Start();
                _timerCounter = 0;
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
                    case "IsCheckedPriceSchemaCollection":
                        if (!IsCheckedPriceSchemaCollection)
                            message = "PriceSchemaRange must be selected";
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