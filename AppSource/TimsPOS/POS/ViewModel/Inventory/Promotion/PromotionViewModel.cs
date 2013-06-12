using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PromotionViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Defines

        private base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        private base_PromotionScheduleRepository _promotionScheduleRepository = new base_PromotionScheduleRepository();
        private base_PromotionAffectRepository _promotionAffectRepository = new base_PromotionAffectRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();

        #endregion

        #region Properties

        #region IsSearchMode

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

        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public List<ComboItem> CategoryList { get; set; }

        /// <summary>
        /// Gets or sets the VendorList
        /// </summary>
        public List<ComboItem> VendorList { get; set; }

        private short _status;
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public short Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(() => Status);
                    OnStatusChanged();
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the PromotionViewModel class.
        /// </summary>
        public PromotionViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            // Load price schemas
            LoadPriceSchemas();

            InitialCommand();
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
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)//Search All
                {
                    Expression<Func<base_Promotion, bool>> predicate = CreateSearchPredicate(Keyword);
                    LoadDataByPredicate(predicate, false, 0);
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
                        Expression<Func<base_Promotion, bool>> predicate = CreateSearchPredicate(Keyword);
                        LoadDataByPredicate(predicate, false, 0);
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
            return true;
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
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this promotion?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                if (SelectedPromotion.IsNew)
                {
                    //DeleteNote();
                    SelectedPromotion = null;
                }
                else if (IsValid)
                {
                    //DeleteNote();
                    // Delete all promotion affect
                    SelectedPromotion.AffectDiscount = 0;
                    OnSavePromotionAffect();
                    // Delete promotion schedule
                    _promotionScheduleRepository.Delete(SelectedPromotion.PromotionScheduleModel.base_PromotionSchedule);
                    // Delete promotion
                    _promotionRepository.Delete(SelectedPromotion.base_Promotion);
                    // Accept changes
                    _promotionRepository.Commit();
                    SelectedPromotion.EndUpdate();
                    SelectedPromotion.PromotionScheduleModel.EndUpdate();
                    //SelectedPromotion.PromotionAffectModel.EndUpdate();
                    PromotionCollection.Remove(SelectedPromotion);
                }
                else
                    return;
                IsSearchMode = true;
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

                // Update Status
                if (SelectedPromotion.Status == (short)StatusBasic.Active)
                {
                    base_PromotionScheduleModel promotionScheduleModel = SelectedPromotion.PromotionScheduleModel;
                    if (promotionScheduleModel.ExpirationNoEndDate && promotionScheduleModel.EndDate.Value < DateTimeExt.Now)
                    {
                        Status = (short)StatusBasic.Deactive;
                        SavePromotion();
                    }
                }

                _status = SelectedPromotion.Status;
                OnPropertyChanged(() => Status);

                // Load PriceSchemaRange
                OnLoadPriceSchema();

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
            Expression<Func<base_Promotion, bool>> predicate = PredicateBuilder.True<base_Promotion>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = CreateSearchPredicate(Keyword);

            LoadDataByPredicate(predicate, false, PromotionCollection.Count);
        }

        #endregion

        #region PopupCustomCommand

        /// <summary>
        /// Gets the PopupCustomCommand command.
        /// </summary>
        public ICommand PopupCustomCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupCustomCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupCustomCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupCustomCommand command is executed.
        /// </summary>
        private void OnPopupCustomCommandExecute(object param)
        {
            if (SelectedPromotion != null)
            {
                if (SelectedPromotion != null &&
                    (param == null && SelectedPromotion.PromotionAffectList.Count == 0) ||
                    (param != null && SelectedPromotion.AffectDiscount == 3))
                {
                    PromotionCustomViewModel viewModel = new PromotionCustomViewModel(CategoryList, SelectedPromotion.PromotionAffectList);
                    bool? result = _dialogService.ShowDialog<CPC.POS.View.CustomView>(_ownerViewModel, viewModel, "Select products apply to this promotion");
                    if (result.HasValue && result.Value)
                    {
                        // Update promotion affect list
                        SelectedPromotion.PromotionAffectList = viewModel.PromotionAffectList;
                    }

                    // Raise total selected products
                    SelectedPromotion.RaiseTotalSelectedProducts();
                }
            }
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
            PromotionAdvanceSearchViewModel viewModel = new PromotionAdvanceSearchViewModel();
            bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PromotionAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue)
            {
                if (msgResult.Value)
                {
                    if (param != null)
                        Keyword = param.ToString();

                    // Create basic predicate combine with advance predicate
                    Expression<Func<base_Promotion, bool>> predicate = CreateSearchPredicate(Keyword);
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
            return (param != null);
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this promotion?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                for (int i = 0; i < (param as ObservableCollection<object>).Count; i++)
                {
                    base_PromotionModel model = (param as ObservableCollection<object>)[i] as base_PromotionModel;
                    //DeleteNote();
                    // Delete all promotion affect
                    model.AffectDiscount = 0;
                    this.OnSavePromotionAffect(model);
                    // Delete promotion schedule
                    _promotionScheduleRepository.Delete(model.PromotionScheduleModel.base_PromotionSchedule);
                    // Delete promotion
                    _promotionRepository.Delete(model.base_Promotion);
                    // Accept changes
                    _promotionRepository.Commit();
                    model.EndUpdate();
                    model.PromotionScheduleModel.EndUpdate();
                    this.PromotionCollection.Remove(model);
                    i--;
                }
            }
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
            return (param != null);
        }

        /// <summary>
        /// Method to invoke when the ChangeStatusCommand command is executed.
        /// </summary>
        private void OnChangeStatusCommandExecute(object param)
        {
            this.OnStatusChanged(param as base_PromotionModel);
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
            LoadStepCommand = new RelayCommand(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            PopupCustomCommand = new RelayCommand<object>(OnPopupCustomCommandExecute, OnPopupCustomCommandCanExecute);
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
                SelectedPromotion.PromotionScheduleModel.IsDirty ||
                SelectedPromotion.PromotionAffectList.IsDirty ||
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
                MessageBoxResult msgResult = MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);

                if (msgResult.Is(MessageBoxResult.Yes))
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

                    // Remove popup note
                    //CloseAllPopupNote();
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
                        // Rollback data
                        SelectedPromotion.PromotionScheduleModel = null;
                        SelectedPromotion.ToModelAndRaise();
                        SelectedPromotion.EndUpdate();
                        LoadRelationData(SelectedPromotion);

                        // Load PriceSchemaRange
                        OnLoadPriceSchema();

                        //// Remove popup note
                        //CloseAllPopupNote();
                    }
                }
            }
            else
            {
                //if (SelectedPromotion != null && SelectedPromotion.IsNew)
                //    DeleteNote();
                //else
                //    // Remove popup note
                //    CloseAllPopupNote();
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
            Expression<Func<base_Promotion, bool>> predicate = PredicateBuilder.True<base_Promotion>();
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.ItemName))
                {
                    predicate = predicate.And(x => x.Name.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Type))
                {
                    Expression<Func<base_Promotion, bool>> predicateType = PredicateBuilder.False<base_Promotion>();

                    IEnumerable<ComboItem> promotionTypes = Common.PromotionTypes.Where(x => x.Text.ToLower().Contains(keyword.ToLower()));
                    foreach (ComboItem promotionType in promotionTypes)
                    {
                        short promotionTypeID = promotionType.Value;
                        predicateType = predicateType.Or(x => x.PromotionTypeId.Equals(promotionTypeID));
                    }

                    predicate = predicate.And(predicateType);
                }
                if (SearchOption.Has(SearchOptions.Code))
                {
                    predicate = predicate.And(x => x.CouponBarCode.Contains(keyword.ToLower()));
                }
            }
            //predicate = predicate.And(x => x.Status.Equals((short)StatusBasic.Active));
            return predicate;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(Expression<Func<base_Promotion, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                PromotionCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;

                if (refreshData)
                {
                    _promotionRepository.Refresh();
                    _promotionScheduleRepository.Refresh();
                    _promotionAffectRepository.Refresh();
                    _departmentRepository.Refresh();
                    _guestRepository.Refresh();
                }

                // Load categories from department table
                LoadDepartment();

                // Load vendor from guest table
                LoadVendor();

                // Get data with range
                IList<base_Promotion> promotions = _promotionRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                foreach (base_Promotion promotion in promotions)
                {
                    bgWorker.ReportProgress(0, promotion);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_PromotionModel promotionModel = new base_PromotionModel((base_Promotion)e.UserState);
                LoadRelationData(promotionModel);
                PromotionCollection.Add(promotionModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for promotion
        /// </summary>
        /// <param name="promotionModel"></param>
        private void LoadRelationData(base_PromotionModel promotionModel)
        {
            // Load  promotion schedule
            if (promotionModel.PromotionScheduleModel == null)
            {
                promotionModel.PromotionScheduleModel = new base_PromotionScheduleModel(
                    promotionModel.base_Promotion.base_PromotionSchedule.SingleOrDefault());

                // Check ExpirationNoEndDate
                promotionModel.PromotionScheduleModel.SetExpirationNoEndDate(promotionModel.PromotionScheduleModel.EndDate != null);
            }

            // Load promotion affect
            OnLoadPromotionAffect(promotionModel);

            // Update vendor id if vendor is deactived
            if (promotionModel.AffectDiscount == 2 && promotionModel.VendorId.HasValue)
            {
                if (!VendorList.Select(x => x.LongValue).Contains(promotionModel.VendorId.Value))
                    promotionModel.VendorId = 0;
            }
        }

        /// <summary>
        /// Load price schemas from xml
        /// </summary>
        private void LoadPriceSchemas()
        {
            Common.Refresh();
            PriceSchemaCollection = new ObservableCollection<CheckBoxItemModel>();
            foreach (ComboItem comboItem in Common.PriceSchemas)
            {
                CheckBoxItemModel checkBoxItemModel = new CheckBoxItemModel(comboItem);
                checkBoxItemModel.PropertyChanged += new PropertyChangedEventHandler(checkBoxItemModel_PropertyChanged);
                PriceSchemaCollection.Add(checkBoxItemModel);
            }
        }

        /// <summary>
        /// Load promotion affect by AffectDiscount
        /// </summary>
        /// <param name="promotionModel"></param>
        private void OnLoadPromotionAffect(base_PromotionModel promotionModel)
        {
            if (promotionModel.PromotionAffectList == null)
                promotionModel.PromotionAffectList = new CollectionBase<base_PromotionAffectModel>();

            if (promotionModel.AffectDiscount == 1 && promotionModel.CategoryId.HasValue)
            {
                if (!CategoryList.Select(x => x.IntValue).Contains(promotionModel.CategoryId.Value))
                    promotionModel.CategoryId = 0;
            }

            // Promotion Affect is Custom
            if (promotionModel.AffectDiscount == 3)
            {
                promotionModel.PromotionAffectList = new CollectionBase<base_PromotionAffectModel>(
                        promotionModel.base_Promotion.base_PromotionAffect.Select(x => new base_PromotionAffectModel(x)));
                promotionModel.RaiseTotalSelectedProducts();
            }
        }

        /// <summary>
        /// Load data from department
        /// </summary>
        private void LoadDepartment()
        {
            if (CategoryList == null)
            {
                // Get all department that status is actived
                IQueryable<int> departmentList = _departmentRepository.
                    GetIQueryable(x => x.LevelId == 0 && x.IsActived == true).Select(x => x.Id);

                // Get all category that contain in department list
                CategoryList = new List<ComboItem>(_departmentRepository.
                    GetIQueryable(x => x.IsActived == true && x.LevelId == 1 && departmentList.Contains(x.ParentId.Value)).
                    OrderBy(x => x.Name).
                    Select(x => new ComboItem { IntValue = x.Id, Text = x.Name }));
                OnPropertyChanged(() => CategoryList);
            }

        }

        /// <summary>
        /// Load data from vendor
        /// </summary>
        private void LoadVendor()
        {
            if (VendorList == null)
            {
                string markType = MarkType.Vendor.ToDescription();
                VendorList = new List<ComboItem>(_guestRepository.
                    GetAll(x => x.Mark.Equals(markType) && !x.IsPurged && x.IsActived).
                    OrderBy(x => x.Company).
                    Select(x => new ComboItem { LongValue = x.Id, Text = x.Company }));
                OnPropertyChanged(() => VendorList);
            }
        }

        /// <summary>
        /// Create a new promotion
        /// </summary>
        private void NewPromotion()
        {
            // Create a new promotion with default values
            SelectedPromotion = new base_PromotionModel();
            _status = Define.CONFIGURATION.DefaultDiscountStatus;
            OnPropertyChanged(() => Status);
            SelectedPromotion.PromotionTypeId = Define.CONFIGURATION.DefaultDiscountType;
            SelectedPromotion.PriceSchemaRange = 0;
            SelectedPromotion.Description = string.Empty;
            SelectedPromotion.AffectDiscount = 0;
            SelectedPromotion.DateCreated = DateTimeExt.Now;
            if (Define.USER != null)
                SelectedPromotion.UserCreated = Define.USER.LoginName;
            SelectedPromotion.Resource = Guid.NewGuid();

            // Check PriceSchemaRange
            OnLoadPriceSchema();

            SelectedPromotion.PromotionScheduleModel = new base_PromotionScheduleModel();
            SelectedPromotion.PromotionAffectList = new CollectionBase<base_PromotionAffectModel>();

            // Turn off IsDirty
            SelectedPromotion.IsDirty = false;
            SelectedPromotion.PromotionScheduleModel.IsDirty = false;
        }

        /// <summary>
        /// Process save promotion function
        /// </summary>
        /// <returns></returns>
        private bool SavePromotion()
        {
            //// Update EndDate value
            //if (SelectedPromotion.PromotionScheduleModel.ExpirationNoEndDate)
            //    SelectedPromotion.PromotionScheduleModel.EndDate = null;

            // Update Status
            SelectedPromotion.Status = Status;

            // Update PriceSchemaRange value
            OnCheckPriceSchema();

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

            // Turn off IsDirty & IsNew
            SelectedPromotion.EndUpdate();
            SelectedPromotion.PromotionScheduleModel.EndUpdate();
            IsDirtyPriceSchemaCollection = false;

            return true;
        }

        /// <summary>
        /// Save when create new promotion
        /// </summary>
        private void SaveNew()
        {
            // Map data from model to entity
            SelectedPromotion.ToEntity();
            SelectedPromotion.PromotionScheduleModel.ToEntity();

            // Add new promotion schedule to repository
            SelectedPromotion.base_Promotion.base_PromotionSchedule.Add(SelectedPromotion.PromotionScheduleModel.base_PromotionSchedule);

            // Add new promotion affect to repository
            if (SelectedPromotion.AffectDiscount == 3)
            {
                foreach (base_PromotionAffectModel promotionAffectModel in SelectedPromotion.PromotionAffectList)
                {
                    // Map data from model to entity
                    promotionAffectModel.ToEntity();

                    SelectedPromotion.base_Promotion.base_PromotionAffect.Add(promotionAffectModel.base_PromotionAffect);
                }
            }
            else
                // Clear promotion affect list
                SelectedPromotion.PromotionAffectList = new CollectionBase<base_PromotionAffectModel>();

            // Add new promotion to repository
            _promotionRepository.Add(SelectedPromotion.base_Promotion);

            // Accept changes
            _promotionRepository.Commit();

            // Update ID from entity to model
            SelectedPromotion.Id = SelectedPromotion.base_Promotion.Id;
            SelectedPromotion.PromotionScheduleModel.PromotionId = SelectedPromotion.Id;
            SelectedPromotion.PromotionScheduleModel.Id = SelectedPromotion.PromotionScheduleModel.base_PromotionSchedule.Id;

            if (SelectedPromotion.AffectDiscount == 3)
            {
                foreach (base_PromotionAffectModel promotionAffectModel in SelectedPromotion.PromotionAffectList)
                {
                    promotionAffectModel.Id = promotionAffectModel.base_PromotionAffect.Id;
                    promotionAffectModel.PromotionId = promotionAffectModel.base_PromotionAffect.PromotionId;
                    promotionAffectModel.EndUpdate();
                }
            }

            // Push new promotion to collection
            PromotionCollection.Add(SelectedPromotion);
        }

        /// <summary>
        /// Save when edit or update promotion
        /// </summary>
        private void SaveUpdate()
        {
            SelectedPromotion.DateUpdated = DateTimeExt.Now;
            if (Define.USER != null)
                SelectedPromotion.UserUpdated = Define.USER.LoginName;

            // Map data from model to entity
            SelectedPromotion.ToEntity();
            SelectedPromotion.PromotionScheduleModel.ToEntity();

            // Save promotion affect by AffectDiscount
            OnSavePromotionAffect();

            // Raise total selected products
            SelectedPromotion.RaiseTotalSelectedProducts();

            // Accept changes
            _promotionRepository.Commit();

            if (SelectedPromotion.PromotionAffectList.Count(x => x.IsNew) > 0)
            {
                foreach (base_PromotionAffectModel promotionAffectModel in SelectedPromotion.PromotionAffectList.Where(x => x.IsNew))
                {
                    promotionAffectModel.Id = promotionAffectModel.base_PromotionAffect.Id;

                    // Turn off IsDirty & IsNew
                    promotionAffectModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Process promotion affect when save data
        /// </summary>
        private void OnSavePromotionAffect()
        {
            switch (SelectedPromotion.AffectDiscount)
            {
                case 0: // All items
                    // Delete all promotion affect in database
                    foreach (base_PromotionAffect promotionAffect in SelectedPromotion.base_Promotion.base_PromotionAffect.ToList())
                        _promotionAffectRepository.Delete(promotionAffect);

                    // Clear promotion affect in entity
                    SelectedPromotion.base_Promotion.base_PromotionAffect.Clear();

                    // Clear promotion affect list
                    SelectedPromotion.PromotionAffectList = new CollectionBase<base_PromotionAffectModel>();
                    break;
                case 1: // All items in department
                case 2: // All items in vendor
                    break;
                case 3: // Custom
                    // Remove promotion affect were deleted
                    if (SelectedPromotion.PromotionAffectList.DeletedItems != null)
                    {
                        foreach (base_PromotionAffectModel promotionAffectModel in SelectedPromotion.PromotionAffectList.DeletedItems)
                        {
                            // Remove promotion affect in entity
                            SelectedPromotion.base_Promotion.base_PromotionAffect.Remove(promotionAffectModel.base_PromotionAffect);

                            // Delete promotion affect in database
                            _promotionAffectRepository.Delete(promotionAffectModel.base_PromotionAffect);
                        }
                        SelectedPromotion.PromotionAffectList.DeletedItems.Clear();
                    }

                    // Create new promotion affect
                    foreach (base_PromotionAffectModel promotionAffectModel in SelectedPromotion.PromotionAffectList)
                    {
                        if (promotionAffectModel.IsNew)
                            promotionAffectModel.PromotionId = SelectedPromotion.Id;

                        // Map data from model to entity
                        promotionAffectModel.ToEntity();

                        // Add new to repository
                        if (promotionAffectModel.IsNew)
                            SelectedPromotion.base_Promotion.base_PromotionAffect.Add(promotionAffectModel.base_PromotionAffect);
                    }
                    break;
            }
        }

        /// <summary>
        /// Process price schema value when check box is checked
        /// </summary>
        private void OnCheckPriceSchema()
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
        private void OnLoadPriceSchema()
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

        /// <summary>
        /// Show popup reason to reactive when change status
        /// </summary>
        private void OnStatusChanged()
        {
            if (Status == (short)StatusBasic.Active && !SelectedPromotion.IsNew &&
                Define.CONFIGURATION.IsRequireDiscountReason.HasValue && Define.CONFIGURATION.IsRequireDiscountReason.Value)
            {
                PromotionReasonViewModel viewModel = new PromotionReasonViewModel();
                viewModel.ReasonReActive = SelectedPromotion.ReasonReActive;
                bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PromotionReasonView>(_ownerViewModel, viewModel, "Entry for reason");
                if (msgResult.HasValue)
                {
                    if (msgResult.Value)
                    {
                        SelectedPromotion.ReasonReActive = viewModel.ReasonReActiveBinding;
                    }
                    else
                    {
                        App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                        {
                            Status = (short)StatusBasic.Deactive;
                        });
                    }
                }
            }
            SelectedPromotion.IsDirty = true;
        }

        /// <summary>
        /// Process promotion affect when save data
        /// </summary>
        private void OnSavePromotionAffect(base_PromotionModel model)
        {
            switch (model.AffectDiscount)
            {
                case 0: // All items
                    // Delete all promotion affect in database
                    foreach (base_PromotionAffect promotionAffect in model.base_Promotion.base_PromotionAffect.ToList())
                        _promotionAffectRepository.Delete(promotionAffect);

                    // Clear promotion affect in entity
                    model.base_Promotion.base_PromotionAffect.Clear();

                    // Clear promotion affect list
                    model.PromotionAffectList = new CollectionBase<base_PromotionAffectModel>();
                    break;
                case 1: // All items in department
                case 2: // All items in vendor
                    break;
                case 3: // Custom
                    // Remove promotion affect were deleted
                    if (model.PromotionAffectList.DeletedItems != null)
                    {
                        foreach (base_PromotionAffectModel promotionAffectModel in model.PromotionAffectList.DeletedItems)
                        {
                            // Remove promotion affect in entity
                            model.base_Promotion.base_PromotionAffect.Remove(promotionAffectModel.base_PromotionAffect);

                            // Delete promotion affect in database
                            _promotionAffectRepository.Delete(promotionAffectModel.base_PromotionAffect);
                        }
                        model.PromotionAffectList.DeletedItems.Clear();
                    }

                    // Create new promotion affect
                    foreach (base_PromotionAffectModel promotionAffectModel in model.PromotionAffectList)
                    {
                        if (promotionAffectModel.IsNew)
                            promotionAffectModel.PromotionId = model.Id;

                        // Map data from model to entity
                        promotionAffectModel.ToEntity();

                        // Add new to repository
                        if (promotionAffectModel.IsNew)
                            model.base_Promotion.base_PromotionAffect.Add(promotionAffectModel.base_PromotionAffect);
                    }
                    break;
            }
        }

        /// <summary>
        /// Show popup reason to reactive when change status
        /// </summary>
        private void OnStatusChanged(base_PromotionModel model)
        {
            if (model.Status == 1)
                _status = 2;
            else
                _status = 1;
            OnPropertyChanged(() => Status);
            if (Status == (short)StatusBasic.Active && !model.IsNew &&
                Define.CONFIGURATION.IsRequireDiscountReason.HasValue && Define.CONFIGURATION.IsRequireDiscountReason.Value)
            {
                PromotionReasonViewModel viewModel = new PromotionReasonViewModel();
                viewModel.ReasonReActive = model.ReasonReActive;
                bool? msgResult = _dialogService.ShowDialog<CPC.POS.View.PromotionReasonView>(_ownerViewModel, viewModel, "Entry for reason");
                if (msgResult.HasValue)
                {
                    if (msgResult.Value)
                        model.ReasonReActive = viewModel.ReasonReActiveBinding;
                    else
                    {
                        App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                        {
                            Status = (short)StatusBasic.Deactive;
                        });
                    }
                }
            }
            model.Status = Status;
            model.DateUpdated = DateTimeExt.Now;
            if (Define.USER != null)
                model.UserUpdated = Define.USER.LoginName;
            // Map data from model to entity
            model.ToEntity();
            // Accept changes
            _promotionRepository.Commit();
            model.EndUpdate();
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            LoadPriceSchemas();
            if (SelectedPromotion != null)
                OnLoadPriceSchema();

            Expression<Func<base_Promotion, bool>> predicate = PredicateBuilder.True<base_Promotion>();
            if (!string.IsNullOrWhiteSpace(Keyword) && SearchOption > 0)//Load with Search Condition
            {
                predicate = CreateSearchPredicate(Keyword);
            }
            else
            {
                //predicate = predicate.And(x => x.Status.Equals((short)StatusBasic.Active));
            }
            LoadDataByPredicate(predicate, true);
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
