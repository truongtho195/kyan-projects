using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using CPCToolkitExtLibraries;
using Microsoft.Win32;

namespace CPC.POS.ViewModel
{
    class EmployeeViewModel : ViewModelBase
    {
        #region Define
        //To define repositories to use them in class.
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_GuestAdditionalRepository _guestAdditionalRepository = new base_GuestAdditionalRepository();
        private base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();
        //To define commands to use them in class.
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand<object> SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand NoteCommand { get; private set; }
        public RelayCommand<object> DuplicateCommand { get; private set; }
        public RelayCommand<object> EditCommand { get; private set; }
        public RelayCommand PrintCommand { get; private set; }
        //To define mark type of employee  to use them in class.
        private string _employeeMark = MarkType.Employee.ToDescription();
        public bool IsAdvanced { get; set; }

        private Expression<Func<base_Guest, bool>> AdvanceSearchPredicate;

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

        public EmployeeViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            StickyManagementViewModel = new PopupStickyViewModel();
            this.InitialCommand();
            Parameter = new Common();

            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
        }

        public EmployeeViewModel(bool isList, object param = null)
            : this()
        {
            this.ChangeSearchMode(isList, param);
        }

        #endregion

        #region Properties

        #region IsDirty
        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (SelectedItemEmployee == null)
                    return false;
                return (SelectedItemEmployee.IsDirty
                    || (this.SelectedItemEmployee.EmployeeFingerprintCollection != null && this.SelectedItemEmployee.EmployeeFingerprintCollection.Any(x => x.IsDirty))
                    || (this.SelectedItemEmployee.AddressControlCollection != null && this.SelectedItemEmployee.AddressControlCollection.IsEditingData)
                    || (this.SelectedItemEmployee.PhotoCollection != null && this.SelectedItemEmployee.PhotoCollection.IsDirty)
                    || this.SelectedItemEmployee.PersonalInfoModel.IsDirty);
            }
        }
        #endregion

        #region EmployeeCollection
        /// <summary>
        /// Gets or sets the WorkScheduleCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> _employeeCollection = new ObservableCollection<base_GuestModel>();
        public ObservableCollection<base_GuestModel> EmployeeCollection
        {
            get
            {
                return _employeeCollection;
            }
            set
            {
                if (_employeeCollection != value)
                {
                    _employeeCollection = value;
                    OnPropertyChanged(() => EmployeeCollection);
                }
            }

        }
        #endregion

        #region SelectedItemEmployee
        /// <summary>
        /// Gets or sets the SelectedItemEmployee.
        /// </summary>
        private base_GuestModel _selectedItemEmployee;
        public base_GuestModel SelectedItemEmployee
        {
            get
            {
                return _selectedItemEmployee;
            }
            set
            {
                if (_selectedItemEmployee != value)
                {
                    _selectedItemEmployee = value;
                    OnPropertyChanged(() => SelectedItemEmployee);
                }
            }
        }
        #endregion

        #region Parameter
        /// <summary>
        /// Gets or sets the SelectedItemEmployee.
        /// </summary>
        private Common _parameter;
        public Common Parameter
        {
            get
            {
                return _parameter;
            }
            set
            {
                _parameter = value;
                OnPropertyChanged(() => Parameter);
            }
        }

        #endregion

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

        #region IsAdvanceMode
        private bool _isAdvanceMode;
        /// <summary>
        /// Gets or sets the IsAdvanceMode.
        /// Using for Search. False is a simple Search
        /// </summary>
        public bool IsAdvanceMode
        {
            get { return _isAdvanceMode; }
            set
            {
                if (_isAdvanceMode != value)
                {
                    _isAdvanceMode = value;
                    OnPropertyChanged(() => IsAdvanceMode);
                }
            }
        }
        #endregion

        #region TotalItem
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalItem
        {
            get
            {
                return 0;
            }
        }
        #endregion

        #region AddressTypeCollection
        private AddressTypeCollection _addressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection AddressTypeCollection
        {
            get { return _addressTypeCollection; }
            set
            {
                if (_addressTypeCollection != value)
                {
                    _addressTypeCollection = value;
                    OnPropertyChanged(() => AddressTypeCollection);
                }
            }
        }
        #endregion

        #region NumberOfItems
        /// <summary>
        /// Gets or sets the NumberOfItems.
        /// </summary>
        private int _numberOfItems;
        public int NumberOfItems
        {
            get
            {
                return _numberOfItems;
            }
            set
            {
                _numberOfItems = value;
                OnPropertyChanged(() => NumberOfItems);
            }
        }

        #endregion

        #region DisplayItems
        /// <summary>
        /// Gets or sets the DisplayItems.
        /// </summary>
        private int _displayItems = 10;
        public int DisplayItems
        {
            get
            {
                return _displayItems;
            }
            set
            {
                _displayItems = value;
                OnPropertyChanged(() => DisplayItems);
            }
        }

        #endregion

        #region CurrentPageIndex
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private int _currentPageIndex;
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

        #region SearchOption
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
        #endregion

        #region FilterText & Keyword
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
        #endregion

        #region SearchAlert
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

        /// <summary>
        /// Gets the ShowOrHiddenNote
        /// </summary>
        public string ShowOrHiddenNote
        {
            get
            {
                if (NotePopupCollection.Count == 0)
                    return "Show Stickies";
                else if (NotePopupCollection.Count == SelectedItemEmployee.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                    return "Hide Stickies";
                else
                    return "Show Stickies";
            }
        }
        #endregion

        #region IsSpouse
        private bool? _isSpouse = false;
        public bool? IsSpouse
        {
            get { return _isSpouse; }
            set
            {
                if (value != _isSpouse)
                {
                    _isSpouse = value;
                    OnPropertyChanged(() => IsSpouse);
                }
            }
        }
        #endregion

        #region IsEmergency
        private bool? _isEmergency = false;
        public bool? IsEmergency
        {
            get { return _isEmergency; }
            set
            {
                if (value != _isEmergency)
                {
                    _isEmergency = value;
                    OnPropertyChanged(() => IsEmergency);
                }
            }
        }
        #endregion

        #region StateCollection
        private ObservableCollection<ComboItem> _stateCollection;
        /// <summary>
        /// Gets or sets the StateCollection.
        /// </summary>
        public ObservableCollection<ComboItem> StateCollection
        {
            get { return _stateCollection; }
            set
            {
                if (_stateCollection != value)
                {
                    _stateCollection = value;
                    OnPropertyChanged(() => StateCollection);
                }
            }
        }
        #endregion

        #region JobTitleCollection
        private ObservableCollection<ComboItem> _jobTitleCollection;
        /// <summary>
        /// Gets or sets the JobTitleCollection.
        /// </summary>
        public ObservableCollection<ComboItem> JobTitleCollection
        {
            get { return _jobTitleCollection; }
            set
            {
                if (_jobTitleCollection != value)
                {
                    _jobTitleCollection = value;
                    OnPropertyChanged(() => JobTitleCollection);
                }
            }
        }
        #endregion


        #region DeparmentCollection
        private ObservableCollection<ComboItem> _departmentCollection;
        /// <summary>
        /// Gets or sets the DeparmentCollection.
        /// </summary>
        public ObservableCollection<ComboItem> DepartmentCollection
        {
            get { return _departmentCollection; }
            set
            {
                if (_departmentCollection != value)
                {
                    _departmentCollection = value;
                    OnPropertyChanged(() => DepartmentCollection);
                }
            }
        }
        #endregion


        #region IsForceFocused
        private bool _isForceFocused;
        /// <summary>
        /// Gets or sets the IsForceFocused.
        /// </summary>
        public bool IsForceFocused
        {
            get { return _isForceFocused; }
            set
            {
                if (_isForceFocused != value)
                {
                    _isForceFocused = value;
                    OnPropertyChanged(() => IsForceFocused);
                }
            }
        }
        #endregion

        #region IsManualGenerate
        /// <summary>
        /// Gets the IsManualGenerate.
        /// </summary>
        public bool IsManualGenerate
        {
            get
            {
                if (Define.CONFIGURATION == null)
                    return false;
                return Define.CONFIGURATION.IsManualGenerate;
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
        private bool OnNewCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (this.ChangeViewExecute(null))
            {
                this.CreateEmployee();
                //To set enable of detail grid.
                this.IsSearchMode = false;
            }
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute(object param)
        {
            if (this.SelectedItemEmployee == null)
                return false;
            return this.IsValid && this.IsDirty &&
                (this.SelectedItemEmployee.AddressControlCollection != null && !this.SelectedItemEmployee.AddressControlCollection.IsErrorData);
            //if (!this.IsError() && this.IsEdit())
            //    return true;
            //return false;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute(object param)
        {
            // TODO: Handle command logic here
            this.SaveEmployee();
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (this.SelectedItemEmployee == null)
                return false;
            return !SelectedItemEmployee.IsNew && !IsDirty;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                if (SelectedItemEmployee.IsNew)
                {
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();

                    SelectedItemEmployee = null;
                    IsSearchMode = true;
                }
                else
                {
                    List<ItemModel> ItemModel = new List<ItemModel>();
                    string resource = SelectedItemEmployee.Resource.Value.ToString();
                    if (!_saleCommissionRepository.GetAll().Select(x => x.GuestResource).Contains(resource))
                    {
                        this.SelectedItemEmployee.IsPurged = true;
                        this.SaveEmployee();
                        this.EmployeeCollection.Remove(SelectedItemEmployee);
                        this.SelectedItemEmployee = EmployeeCollection.First();
                        this.NumberOfItems = NumberOfItems - 1;
                        // Remove all popup sticky
                        this.StickyManagementViewModel.DeleteAllResourceNote();
                        this.IsSearchMode = true;
                        App.WriteUserLog("Employee", "User deleted an employee.");
                    }
                    else
                    {
                        ItemModel.Add(new ItemModel { Id = SelectedItemEmployee.Id, Text = SelectedItemEmployee.GuestNo, Resource = resource });
                        _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "Employee"), "Problem Detection");
                    }
                }
                IsSearchMode = true;
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
                this.IsAdvanced = false;
                Expression<Func<base_Guest, bool>> predicate = this.CreatePredicateWithConditionSearch(this.FilterText);
                this.LoadDataByPredicate(predicate, false, 0);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion

        #region DoubleClickViewCommand
        /// <summary>
        /// Method to check whether the DoubleClickViewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            if (IsSearchMode && param == null)
                return false;
            return true;
        }

        private void OnDoubleClickViewCommandExecute(object param)
        {
            try
            {
                if (param != null && this.IsSearchMode)
                {
                    this.SelectedItemEmployee = param as base_GuestModel;
                    this.LoadManagerResource(this.SelectedItemEmployee.Resource.Value);
                    if (this.SelectedItemEmployee.base_Guest.ManagerResource != string.Empty)
                        this.SelectedItemEmployee.ManagerResource = this.SelectedItemEmployee.base_Guest.ManagerResource;
                    this.LoadDataWhenSelected();
                    this.IsSearchMode = false;
                }
                else if (!IsSearchMode)//Change from Edit form to Search Gird check view has dirty
                {
                    if (this.ChangeViewExecute(null))
                        this.IsSearchMode = true;
                }
                else
                    this.IsSearchMode = !this.IsSearchMode;//Change View To
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
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (this.IsAdvanced) //Load Contitnue with advance Search
                predicate = this.AdvanceSearchPredicate;
            else if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = this.CreatePredicateWithConditionSearch(this.Keyword);
            LoadDataByPredicate(predicate, false, EmployeeCollection.Count);
        }
        #endregion

        #region RecordFingerprintCommand
        /// <summary>
        /// Gets the RecordFingerprint Command.
        /// <summary>

        public RelayCommand<object> RecordFingerprintCommand { get; private set; }

        /// <summary>
        /// Method to check whether the RecordFingerprint command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRecordFingerprintCommandCanExecute(object param)
        {
            if (SelectedItemEmployee == null || param == null)
                return false;
            return !FingerPrintNotSupport();
        }

        /// <summary>
        /// Method to invoke when the RecordFingerprint command is executed.
        /// </summary>
        private void OnRecordFingerprintCommandExecute(object param)
        {
            bool rightHand = bool.Parse(param.ToString());
            RecordFingerprintViewModel viewModel = new RecordFingerprintViewModel();
            viewModel.IsLeft = !rightHand;
            bool? result = _dialogService.ShowDialog<RecordFingerprintView>(_ownerViewModel, viewModel, "Register right fingerprint");
            if (result.HasValue && result.Value)
            {
                base_GuestFingerPrintModel employeeFingerPrintModel = this.SelectedItemEmployee.EmployeeFingerprintCollection.SingleOrDefault(x => x.HandFlag == rightHand);
                if (employeeFingerPrintModel != null)
                {
                    employeeFingerPrintModel.DateUpdated = DateTime.Now;
                    employeeFingerPrintModel.FingerIndex = viewModel.FingerID;
                    employeeFingerPrintModel.FingerPrintImage = viewModel.Temp;
                }
                else
                {
                    employeeFingerPrintModel = new base_GuestFingerPrintModel();
                    employeeFingerPrintModel.HandFlag = rightHand;
                    employeeFingerPrintModel.FingerIndex = viewModel.FingerID;
                    employeeFingerPrintModel.FingerPrintImage = viewModel.Temp;
                    employeeFingerPrintModel.DateUpdated = DateTime.Now;
                    this.SelectedItemEmployee.EmployeeFingerprintCollection.Add(employeeFingerPrintModel);
                }

                //Set has FingerPrint
                this.SelectedItemEmployee.HasFingerPrintRight = this.SelectedItemEmployee.EmployeeFingerprintCollection.Any(x => x.HandFlag);

                this.SelectedItemEmployee.HasFingerPrintLeft = this.SelectedItemEmployee.EmployeeFingerprintCollection.Any(x => !x.HandFlag);
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
            return (param == null || (param is ObservableCollection<object> && (param as ObservableCollection<object>).Count == 0)) ? false : true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                bool flag = false;
                string employeeID = string.Empty;
                List<ItemModel> ItemModel = new List<ItemModel>();
                for (int i = 0; i < (param as ObservableCollection<object>).Count; i++)
                {
                    base_GuestModel model = (param as ObservableCollection<object>)[i] as base_GuestModel;
                    string resource = model.Resource.Value.ToString();
                    if (!_saleCommissionRepository.GetAll().Select(x => x.GuestResource).Contains(resource))
                    {
                        model.IsPurged = true;
                        model.ToEntity();
                        _guestRepository.Commit();
                        model.EndUpdate();
                        this.EmployeeCollection.Remove(model);
                        NumberOfItems = NumberOfItems - 1;
                        // Remove all popup sticky
                        this.LoadResourceNoteCollection(model);
                        this.StickyManagementViewModel.DeleteAllResourceNote(model.ResourceNoteCollection);
                        i--;
                        employeeID += employeeID + model.GuestNo;
                    }
                    else
                    {
                        ItemModel.Add(new ItemModel { Id = model.Id, Text = model.GuestNo, Resource = resource });
                        flag = true;
                    }
                }
                if (flag)
                    _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "Employee"), "Problem Detection");
                if (ItemModel.Count < (param as ObservableCollection<object>).Count)
                    App.WriteUserLog("Employee", "User deleted employee(s)." + employeeID);
            }
        }
        #endregion

        #region DuplicateCommand
        /// <summary>
        /// Method to check whether the DuplicateCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDuplicateCommandCanExecute(object param)
        {
            return (param == null || (param is ObservableCollection<object> && ((param as ObservableCollection<object>).Count == 0 || (param as ObservableCollection<object>).Count > 1))) ? false : true;
        }

        /// <summary>
        /// Method to invoke when the DuplicateCommand command is executed.
        /// </summary>
        private void OnDuplicateCommandExecute(object param)
        {
            try
            {
                //To set enable of detail grid.
                this.IsSearchMode = false;
                this.CreateDuplicate((param as ObservableCollection<object>)[0] as base_GuestModel);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("OnDuplicateCommandExecute" + ex.ToString());
            }

        }

        private void CreateDuplicate(base_GuestModel GuestModel)
        {
            this.SelectedItemEmployee = new base_GuestModel();
            this.SelectedItemEmployee.MiddleName = GuestModel.MiddleName;
            this.SelectedItemEmployee.LastName = GuestModel.LastName;
            this.SelectedItemEmployee.Company = GuestModel.Company;
            this.SelectedItemEmployee.Phone1 = GuestModel.Phone1;
            this.SelectedItemEmployee.Ext1 = GuestModel.Ext1;
            this.SelectedItemEmployee.Phone2 = GuestModel.Phone2;
            this.SelectedItemEmployee.Ext2 = GuestModel.Ext2;
            this.SelectedItemEmployee.Fax = GuestModel.Fax;
            this.SelectedItemEmployee.CellPhone = GuestModel.CellPhone;
            this.SelectedItemEmployee.Email = string.Empty;
            this.SelectedItemEmployee.Website = GuestModel.Website;
            this.SelectedItemEmployee.UserCreated = GuestModel.UserCreated;
            this.SelectedItemEmployee.UserUpdated = GuestModel.UserUpdated;
            this.SelectedItemEmployee.DateCreated = GuestModel.DateCreated;
            this.SelectedItemEmployee.DateUpdated = GuestModel.DateUpdated;
            this.SelectedItemEmployee.IsPurged = GuestModel.IsPurged;
            this.SelectedItemEmployee.GuestTypeId = GuestModel.GuestTypeId;
            this.SelectedItemEmployee.IsActived = GuestModel.IsActived;
            this.SelectedItemEmployee.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
            this.SelectedItemEmployee.PositionId = GuestModel.PositionId;
            this.SelectedItemEmployee.Department = GuestModel.Department;
            this.SelectedItemEmployee.Mark = GuestModel.Mark;
            this.SelectedItemEmployee.AccountNumber = GuestModel.AccountNumber;
            this.SelectedItemEmployee.ParentId = GuestModel.ParentId;
            this.SelectedItemEmployee.IsRewardMember = GuestModel.IsRewardMember;
            this.SelectedItemEmployee.CheckLimit = GuestModel.CheckLimit;
            this.SelectedItemEmployee.CreditLimit = GuestModel.CreditLimit;
            this.SelectedItemEmployee.BalanceDue = GuestModel.BalanceDue;
            this.SelectedItemEmployee.AvailCredit = GuestModel.AvailCredit;
            this.SelectedItemEmployee.PastDue = GuestModel.PastDue;
            this.SelectedItemEmployee.IsPrimary = GuestModel.IsPrimary;
            this.SelectedItemEmployee.CommissionPercent = GuestModel.CommissionPercent;
            this.SelectedItemEmployee.TotalRewardRedeemed = GuestModel.TotalRewardRedeemed;
            this.SelectedItemEmployee.PurchaseDuringTrackingPeriod = GuestModel.PurchaseDuringTrackingPeriod;
            this.SelectedItemEmployee.RequirePurchaseNextReward = GuestModel.RequirePurchaseNextReward;
            this.SelectedItemEmployee.HireDate = GuestModel.HireDate;
            this.SelectedItemEmployee.IsBlockArriveLate = GuestModel.IsBlockArriveLate;
            this.SelectedItemEmployee.IsDeductLunchTime = GuestModel.IsDeductLunchTime;
            this.SelectedItemEmployee.IsBalanceOvertime = GuestModel.IsBalanceOvertime;
            this.SelectedItemEmployee.LateMinutes = GuestModel.LateMinutes;
            this.SelectedItemEmployee.OvertimeOption = GuestModel.OvertimeOption;
            this.SelectedItemEmployee.OTLeastMinute = GuestModel.OTLeastMinute;
            this.SelectedItemEmployee.IsTrackingHour = GuestModel.IsTrackingHour;
            this.SelectedItemEmployee.TermDiscount = GuestModel.TermDiscount;
            this.SelectedItemEmployee.TermNetDue = GuestModel.TermNetDue;
            this.SelectedItemEmployee.TermPaidWithinDay = GuestModel.TermPaidWithinDay;
            this.SelectedItemEmployee.PaymentTermDescription = GuestModel.PaymentTermDescription;
            this.SelectedItemEmployee.SaleRepId = GuestModel.SaleRepId;
            this.SelectedItemEmployee.Shift = GuestModel.Shift;
            this.SelectedItemEmployee.IdCard = GuestModel.IdCard;
            this.SelectedItemEmployee.IdCardImg = GuestModel.IdCardImg;
            this.SelectedItemEmployee.Remark = GuestModel.Remark;
            this.SelectedItemEmployee.GroupResource = GuestModel.GroupResource;
            //To load Address Model
            this.SelectedItemEmployee.AddressCollection = new ObservableCollection<base_GuestAddressModel>();
            this.SelectedItemEmployee.AddressControlCollection = new AddressControlCollection();
            foreach (var item in GuestModel.base_Guest.base_GuestAddress)
            {
                base_GuestAddressModel addressModel = new base_GuestAddressModel(item);
                addressModel.CreateGuestAddress();
                this.SelectedItemEmployee.AddressCollection.Add(addressModel);
                AddressControlModel addressControlModel = addressModel.ToAddressControlModel();
                addressControlModel.IsDirty = true;
                addressControlModel.IsNew = true;
                this.SelectedItemEmployee.AddressControlCollection.Add(addressControlModel);
            }
            //To load PhotoCollection
            this.SelectedItemEmployee.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            base_ResourcePhotoModel base_ResourcePhotoModel = new base_ResourcePhotoModel();
            base_ResourcePhotoModel.IsNew = true;
            base_ResourcePhotoModel.IsDirty = true;
            base_ResourcePhotoModel.ImageBinary = GuestModel.Picture;
            this.SelectedItemEmployee.PhotoCollection.Add(base_ResourcePhotoModel);
            if (this.SelectedItemEmployee.PhotoCollection.Count > 0)
                this.SelectedItemEmployee.PhotoDefault = this.SelectedItemEmployee.PhotoCollection.FirstOrDefault();
            else
                this.SelectedItemEmployee.PhotoDefault = new base_ResourcePhotoModel();

            this.SelectedItemEmployee.Resource = Guid.NewGuid();
            this.SelectedItemEmployee.FirstName = GuestModel.FirstName + "(Copy)";
            this.SelectedItemEmployee.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            this.StickyManagementViewModel.SetParentResource(this.SelectedItemEmployee.Resource.ToString(), this.SelectedItemEmployee.ResourceNoteCollection);
            this.SelectedItemEmployee.EmployeeFingerprintCollection = new CollectionBase<base_GuestFingerPrintModel>();
            if (GuestModel.base_Guest.base_GuestProfile == null
                || GuestModel.base_Guest.base_GuestProfile.Count == 0)
            {
                this.SelectedItemEmployee.PersonalInfoModel = new base_GuestProfileModel();
                this.IsSpouse = false;
                this.IsEmergency = false;
            }
            else
            {
                this.SelectedItemEmployee.PersonalInfoModel = new base_GuestProfileModel(GuestModel.base_Guest.base_GuestProfile.First(), true);
                this.IsSpouse = this.SelectedItemEmployee.PersonalInfoModel.IsSpouse;
                this.IsEmergency = this.SelectedItemEmployee.PersonalInfoModel.IsEmergency;
            }
            //To load Manager Resource
            this.LoadManagerResource(this.SelectedItemEmployee.Resource.Value);
            this.SelectedItemEmployee.ManagerResource = GuestModel.ManagerResource;
            this.SelectedItemEmployee.PersonalInfoModel.CreateBase_GuestProfile();
            this.SelectedItemEmployee.PersonalInfoModel.IsNew = true;
            this.SelectedItemEmployee.PersonalInfoModel.IsDirty = true;
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

        /// <summary>
        /// Method to invoke when the EditCommand command is executed.
        /// </summary>
        private void OnEditCommandExecute(object param)
        {
            try
            {
                this.SelectedItemEmployee = (param as ObservableCollection<object>)[0] as base_GuestModel;
                this.LoadDataWhenSelected();
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

        #region InsertDateStampCommand
        /// <summary>
        /// Gets the InsertDateStamp Command.
        /// <summary>

        public RelayCommand<object> InsertDateStampCommand { get; private set; }


        /// <summary>
        /// Method to check whether the InsertDateStamp command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnInsertDateStampCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the InsertDateStamp command is executed.
        /// </summary>
        private void OnInsertDateStampCommandExecute(object param)
        {
            CPCToolkitExt.TextBoxControl.TextBox remarkTextBox = param as CPCToolkitExt.TextBoxControl.TextBox;
            SetValueControlHelper.InsertTimeStamp(remarkTextBox);
        }
        #endregion

        #region AddNewStateCommand
        /// <summary>
        /// Gets the AddNewState Command.
        /// <summary>

        public RelayCommand<object> AddNewStateCommand { get; private set; }


        /// <summary>
        /// Method to check whether the AddNewState command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddNewStateCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the AddNewState command is executed.
        /// </summary>
        private void OnAddNewStateCommandExecute(object param)
        {
            AddNewStateViewModel viewModel = new AddNewStateViewModel();
            bool? result = _dialogService.ShowDialog<AddNewStateView>(_ownerViewModel, viewModel, "Add New State");
            if (result == true)
            {
                StateCollection.Insert(StateCollection.Count, viewModel.ItemState);
                if (param != null)
                {
                    if (param.ToString().Equals("Personal"))
                        this.SelectedItemEmployee.PersonalInfoModel.State = viewModel.ItemState.ObjValue.ToString();
                    else
                        this.SelectedItemEmployee.PersonalInfoModel.SState = viewModel.ItemState.ObjValue.ToString();
                }
            }
        }
        #endregion

        #region AddNewJobTitleCommand
        /// <summary>
        /// Gets the AddNewJobTitleCommand Command.
        /// <summary>

        public RelayCommand<object> AddNewJobTitleCommand { get; private set; }


        /// <summary>
        /// Method to check whether the AddNewJobTitle command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddNewJobTitleCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the AddNewState command is executed.
        /// </summary>
        private void OnAddNewJobTitleCommandExecute(object param)
        {
            AddNewJobTitleViewModel viewModel = new AddNewJobTitleViewModel();
            bool? result = _dialogService.ShowDialog<AddNewJobTitleView>(_ownerViewModel, viewModel, "Add Job Title");
            if (result == true)
            {
                this.JobTitleCollection.Insert(JobTitleCollection.Count, viewModel.ItemJobTitle);
                this.SelectedItemEmployee.PositionId = viewModel.ItemJobTitle.Value;
            }

        }
        #endregion

        #region -Print Command-
        /// <summary>
        /// Method to excute view report window
        /// </summary>
        public void OnPrintCommandExecute()
        {
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            string resource = "'" + SelectedItemEmployee.Resource + "'";
            rpt.ShowReport("rptEmployee", resource);
        }

        /// <summary>
        /// Method to check can view report window
        /// </summary>
        /// <returns></returns>
        public bool OnPrintCommandCanExecute()
        {
            return (SelectedItemEmployee != null && !SelectedItemEmployee.IsDirty && !SelectedItemEmployee.IsNew);
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
            try
            {
                if (_waitingTimer != null)
                    _waitingTimer.Stop();

                EmployeeAdvanceSearchViewModel viewModel = new EmployeeAdvanceSearchViewModel();
                bool? dialogResult = _dialogService.ShowDialog<EmployeeAdvanceSearchView>(_ownerViewModel, viewModel, Language.GetMsg("C104"));
                if (dialogResult ?? false)
                {
                    IsAdvanced = true;
                    this.AdvanceSearchPredicate = viewModel.AdvanceSearchPredicate;
                    this.LoadDataByPredicate(this.AdvanceSearchPredicate, false, 0);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion

        #region AddNewDepartmentCommand
        /// <summary>
        /// Gets the AddNewDepartment Command.
        /// <summary>

        public RelayCommand<object> AddNewDepartmentCommand { get; private set; }



        /// <summary>
        /// Method to check whether the AddNewDepartment command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddNewDepartmentCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddNewDepartment command is executed.
        /// </summary>
        private void OnAddNewDepartmentCommandExecute(object param)
        {
            AddNewDepartmentViewModel viewModel = new AddNewDepartmentViewModel();
            bool? result = _dialogService.ShowDialog<AddNewDepartmentView>(_ownerViewModel, viewModel, "Add Department");
            if (result == true)
            {
                this.DepartmentCollection.Insert(DepartmentCollection.Count, viewModel.DepartmentItem);
                this.SelectedItemEmployee.Department = Convert.ToInt32(viewModel.DepartmentItem.Value);
            }
        }
        #endregion
        #endregion

        #region Private Methods

        #region InitialCommand
        /// <summary>
        /// To create commands when View is opened.
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            this.NewCommand = new RelayCommand(OnNewCommandExecute, null);
            this.SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            this.RecordFingerprintCommand = new RelayCommand<object>(OnRecordFingerprintCommandExecute, OnRecordFingerprintCommandCanExecute);
            this.DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
            this.DuplicateCommand = new RelayCommand<object>(OnDuplicateCommandExecute, OnDuplicateCommandCanExecute);
            this.EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            this.PrintCommand = new RelayCommand(OnPrintCommandExecute, OnPrintCommandCanExecute);
            this.PopupAdvanceSearchCommand = new RelayCommand<object>(OnPopupAdvanceSearchCommandExecute, OnPopupAdvanceSearchCommandCanExecute);
            this.InsertDateStampCommand = new RelayCommand<object>(OnInsertDateStampCommandExecute, OnInsertDateStampCommandCanExecute);
            this.AddNewStateCommand = new RelayCommand<object>(OnAddNewStateCommandExecute, OnAddNewStateCommandCanExecute);
            this.AddNewJobTitleCommand = new RelayCommand<object>(this.OnAddNewJobTitleCommandExecute, this.OnAddNewJobTitleCommandCanExecute);
            this.AddNewDepartmentCommand = new RelayCommand<object>(OnAddNewDepartmentCommandExecute, OnAddNewDepartmentCommandCanExecute);
            ///To load AddressTypeCollection
            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = "Home" });
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = "Business" });
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });
            this.NotePopupCollection = new ObservableCollection<PopupContainer>();
            this.StateCollection = new ObservableCollection<ComboItem>(Common.States);
            if (Common.JobTitles != null && Common.JobTitles.Count > 0)
                this.JobTitleCollection = new ObservableCollection<ComboItem>(Common.JobTitles);
            else
                this.JobTitleCollection = new ObservableCollection<ComboItem>();
            this.NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };

            if (Common.Departments.Any())
                this.DepartmentCollection = new ObservableCollection<ComboItem>(Common.Departments);
            else
                this.DepartmentCollection = new ObservableCollection<ComboItem>();



        }
        #endregion

        #region CreateEmployee
        /// <summary>
        ///To create new an rmployeeModel and defaults value.
        /// </summary>
        private void CreateEmployee()
        {
            this.SelectedItemEmployee = new base_GuestModel();
            this.SelectedItemEmployee.Resource = Guid.NewGuid();
            this.SelectedItemEmployee.Title = 1;
            this.SelectedItemEmployee.IsActived = true;
            this.SelectedItemEmployee.GuestTypeId = 1;
            this.SelectedItemEmployee.IsPrimary = false;
            this.SelectedItemEmployee.Company = string.Empty;
            this.SelectedItemEmployee.Department = 0;
            this.SelectedItemEmployee.DateCreated = DateTime.Now;
            this.SelectedItemEmployee.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            this.SelectedItemEmployee.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
            this.SelectedItemEmployee.Mark = MarkType.Employee.ToDescription();
            this.SelectedItemEmployee.PositionId = 0;
            this.SelectedItemEmployee.OvertimeOption = 0;
            this.SelectedItemEmployee.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            this.StickyManagementViewModel.SetParentResource(SelectedItemEmployee.Resource.ToString(), SelectedItemEmployee.ResourceNoteCollection);
            //Personal Info
            this.SelectedItemEmployee.PersonalInfoModel = new base_GuestProfileModel();
            this.IsSpouse = false;
            this.SelectedItemEmployee.PersonalInfoModel.IsSpouse = false;
            this.SelectedItemEmployee.PersonalInfoModel.SEmail = string.Empty;
            this.SelectedItemEmployee.PersonalInfoModel.DOB = DateTime.Today.AddYears(-10);
            this.IsEmergency = false;
            this.SelectedItemEmployee.PersonalInfoModel.IsEmergency = false;
            this.SelectedItemEmployee.PersonalInfoModel.Gender = Common.Gender.First().Value;
            this.SelectedItemEmployee.PersonalInfoModel.Marital = Common.MaritalStatus.First().Value;
            this.SelectedItemEmployee.PersonalInfoModel.SGender = Common.Gender.First().Value;
            this.SelectedItemEmployee.PersonalInfoModel.IsDirty = false;
            //Collection relation
            this.SelectedItemEmployee.AddressControlCollection = new AddressControlCollection { new AddressControlModel { IsDefault = true, AddressTypeID = 0, IsNew = true, IsDirty = false } };
            this.SelectedItemEmployee.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            this.SelectedItemEmployee.AddressCollection = new ObservableCollection<base_GuestAddressModel>();
            this.SelectedItemEmployee.AddressCollection.Add(new base_GuestAddressModel { AddressLine1 = string.Empty, City = string.Empty });
            this.SelectedItemEmployee.EmployeeFingerprintCollection = new CollectionBase<base_GuestFingerPrintModel>();
            this.SelectedItemEmployee.PropertyChanged += new PropertyChangedEventHandler(SelectedItemEmployee_PropertyChanged);
            //To load ManagerResource
            this.LoadManagerResource(this.SelectedItemEmployee.Resource.Value);
            this.SelectedItemEmployee.IsDirty = false;
        }
        #endregion

        #region Save
        /// <summary>
        /// Function save Employee
        /// </summary>
        /// <param name="param"></param>
        private bool SaveEmployee()
        {
            bool result = true;
            try
            {
                //Save Picture
                if (this.SelectedItemEmployee.PhotoCollection != null
                    && this.SelectedItemEmployee.PhotoCollection.Count > 0)
                {
                    this.SelectedItemEmployee.PhotoCollection.FirstOrDefault().IsDirty = false;
                    this.SelectedItemEmployee.PhotoCollection.FirstOrDefault().IsNew = false;
                    this.SelectedItemEmployee.Picture = this.SelectedItemEmployee.PhotoCollection.FirstOrDefault().ImageBinary;
                }
                else
                    this.SelectedItemEmployee.Picture = null;
                if (this.SelectedItemEmployee.PhotoCollection.DeletedItems != null &&
                this.SelectedItemEmployee.PhotoCollection.DeletedItems.Count > 0)
                    this.SelectedItemEmployee.PhotoCollection.DeletedItems.Clear();
                this.SelectedItemEmployee.Shift = Define.ShiftCode;
                //To close detail grid of Employee after saving data.
                if (this.SelectedItemEmployee.IsNew)
                    this.Insert();
                //To update item when it is edited.
                else
                    this.Update();

                SetDepartmentComboboxItem(this.SelectedItemEmployee);

                this.SelectedItemEmployee.PersonalInfoModel.ToModelAndRaise();
                this.SelectedItemEmployee.PersonalInfoModel.EndUpdate();
                this.SelectedItemEmployee.ToModelAndRaise();
                this.SelectedItemEmployee.EndUpdate();
                this.SelectedItemEmployee.GuestCollection = this.EmployeeCollection;
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error");
            }
            return result;
        }
        #endregion

        #region Insert
        /// <summary>
        /// To save when create new Employee
        /// </summary>
        private void Insert()
        {
            // To convert data from model to entity
            this.SelectedItemEmployee.ToEntity();
            //Mapping Personal Info
            if (SelectedItemEmployee.PersonalInfoModel != null)
            {
                //Mapping Personal Info
                this.SelectedItemEmployee.PersonalInfoModel.ToEntity();
                if (SelectedItemEmployee.PersonalInfoModel.IsNew)
                    SelectedItemEmployee.base_Guest.base_GuestProfile.Add(SelectedItemEmployee.PersonalInfoModel.base_GuestProfile);
                this.SelectedItemEmployee.PersonalInfoModel.EndUpdate();
            }

            ///Created by Thaipn.
            base_GuestAddressModel addressModel;
            bool firstAddress = true;
            //To insert an address. 
            //Convert from AddressControlCollection To AddressModel 
            foreach (AddressControlModel addressControlModel in this.SelectedItemEmployee.AddressControlCollection)
            {
                addressModel = new base_GuestAddressModel();
                addressModel.UserCreated = Define.USER.LoginName;
                addressModel.ToModel(addressControlModel);
                addressModel.IsDefault = firstAddress;
                addressModel.DateCreated = DateTimeExt.Now;
                addressModel.GuestResource = this.SelectedItemEmployee.Resource.ToString();
                addressModel.EndUpdate();
                //To convert data from model to entity
                addressModel.ToEntity();
                this.SelectedItemEmployee.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                firstAddress = false;
                addressModel.EndUpdate();
                addressControlModel.IsDirty = false;
                addressControlModel.IsNew = false;
            }
            //Save FingerPrint
            if (this.SelectedItemEmployee.EmployeeFingerprintCollection != null)
            {
                foreach (base_GuestFingerPrintModel fingerPrintModel in this.SelectedItemEmployee.EmployeeFingerprintCollection)
                {
                    fingerPrintModel.ToEntity();
                    if (fingerPrintModel.IsNew)
                        SelectedItemEmployee.base_Guest.base_GuestFingerPrint.Add(fingerPrintModel.base_GuestFingerPrint);
                    fingerPrintModel.EndUpdate();

                }
            }
            //To commit image.
            _guestRepository.Add(this.SelectedItemEmployee.base_Guest);
            _guestRepository.Commit();
            // To update ID from entity to model
            this.SelectedItemEmployee.Id = this.SelectedItemEmployee.base_Guest.Id;
            // To turn off IsDirty & IsNew
            this.SelectedItemEmployee.EndUpdate();
            this.EmployeeCollection.Add(this.SelectedItemEmployee);
            this.NumberOfItems = this.EmployeeCollection.Count;
            App.WriteUserLog("Employee", "User inserted new employee.");
        }
        #endregion

        #region Update
        /// <summary>
        /// To update item when it was edited.
        /// </summary>
        private void Update()
        {
            //Update commission when change commission of manager.
            if (this.SelectedItemEmployee.CommissionPercent != this.SelectedItemEmployee.base_Guest.CommissionPercent)
            {
                string managerResource = this.SelectedItemEmployee.Resource.ToString();
                var guestCommmission = _guestRepository.GetAll(x => !x.IsPurged && x.Mark.Equals(_employeeMark) && x.ManagerResource == managerResource);
                if (guestCommmission.Count() > 0)
                {
                    foreach (var item in guestCommmission.Where(x => x.CommissionPercent > 100 - this.SelectedItemEmployee.CommissionPercent))
                    {
                        item.CommissionPercent = 100 - this.SelectedItemEmployee.CommissionPercent;
                        this._guestRepository.Commit();
                        var employeecommission = this.EmployeeCollection.SingleOrDefault(x => x.Resource == item.Resource);
                        if (employeecommission != null)
                        {
                            employeecommission.CommissionPercent = item.CommissionPercent;
                            employeecommission.base_Guest.CommissionPercent = item.CommissionPercent;
                            employeecommission.EndUpdate();
                        }
                    }
                }
            }
            this.SelectedItemEmployee.DateUpdated = DateTime.Now;
            this.SelectedItemEmployee.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            // To map data from model to entity
            this.SelectedItemEmployee.ToEntity();
            //Mapping Personal Info
            if (SelectedItemEmployee.PersonalInfoModel != null && SelectedItemEmployee.PersonalInfoModel.IsDirty)
            {
                //Mapping Personal Info
                SelectedItemEmployee.PersonalInfoModel.ToEntity();
                if (SelectedItemEmployee.PersonalInfoModel.IsNew)
                    SelectedItemEmployee.base_Guest.base_GuestProfile.Add(SelectedItemEmployee.PersonalInfoModel.base_GuestProfile);
                SelectedItemEmployee.PersonalInfoModel.EndUpdate();
            }
            //Clear old value of address.
            this._guestAddressRepository.Delete(this.SelectedItemEmployee.base_Guest.base_GuestAddress);
            this.SelectedItemEmployee.base_Guest.base_GuestAddress.Clear();
            base_GuestAddressModel addressModel;
            bool firstAddress = true;
            // Insert or update address
            foreach (AddressControlModel addressControlModel in this.SelectedItemEmployee.AddressControlCollection)
            {
                addressModel = new base_GuestAddressModel();
                addressModel.ToModel(addressControlModel);
                addressModel.IsDefault = firstAddress;
                addressModel.DateUpdated = DateTimeExt.Now;
                addressModel.UserUpdated = Define.USER.LoginName;
                addressModel.GuestResource = this.SelectedItemEmployee.Resource.ToString();
                addressModel.EndUpdate();
                //To convert data from model to entity
                addressModel.ToEntity();
                this.SelectedItemEmployee.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                firstAddress = false;
                // Update default address
                if (addressModel.IsDefault)
                    SelectedItemEmployee.AddressModel = addressModel;
                // Turn off IsDirty & IsNew
                addressModel.EndUpdate();
                addressControlModel.IsNew = false;
                addressControlModel.IsDirty = false;
            }

            //Save FingerPrint
            foreach (base_GuestFingerPrintModel fingerPrintModel in SelectedItemEmployee.EmployeeFingerprintCollection.Where(x => x.IsDirty))
            {
                fingerPrintModel.ToEntity();
                if (fingerPrintModel.IsNew)
                    SelectedItemEmployee.base_Guest.base_GuestFingerPrint.Add(fingerPrintModel.base_GuestFingerPrint);
                fingerPrintModel.EndUpdate();

            }
            _guestRepository.Commit();
            // To turn off IsDirty & IsNew
            this.SelectedItemEmployee.EndUpdate();
            App.WriteUserLog("Employee", "User updated an employee.");
        }
        #endregion

        #region ChangeViewExecute
        /// <summary>
        /// To execute BarListExecute when user click Bar Button.
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Cancel))
                    return false;
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                        //if (SaveCustomer())
                        result = SaveEmployee();
                    else //Has Error
                        result = false;
                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (this.SelectedItemEmployee.IsNew)
                    {
                        // Remove all popup sticky
                        this.StickyManagementViewModel.DeleteAllResourceNote();
                        if (isClosing.HasValue && !isClosing.Value)
                            this.IsSearchMode = true;
                        this.SelectedItemEmployee = null;
                    }
                    else //Old Item Rollback data
                    {
                        // Close all popup sticky
                        this.StickyManagementViewModel.CloseAllPopupSticky();
                        this.SelectedItemEmployee.ToModelAndRaise();
                        this.SetToEmployeeModel(SelectedItemEmployee);
                        this.SetDataDefaultToModel(SelectedItemEmployee);
                        this.SetDataRelationToModel(SelectedItemEmployee);
                    }
                }
            }
            else
            {
                if (SelectedItemEmployee != null && SelectedItemEmployee.IsNew)
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();

                else
                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();

            }
            return result;
        }
        #endregion

        #region IsEdit
        /// <summary>
        ///To check has edit on form
        /// </summary>
        /// <returns></returns>
        private bool IsEdit()
        {
            if (this.SelectedItemEmployee == null)
                return false;

            return this.SelectedItemEmployee.IsDirty
                || SelectedItemEmployee.PersonalInfoModel.IsDirty
                || (this.SelectedItemEmployee.AddressControlCollection != null && this.SelectedItemEmployee.AddressControlCollection.IsEditingData)
                || (this.SelectedItemEmployee.PhotoCollection != null && this.SelectedItemEmployee.PhotoCollection.IsDirty);
        }

        // <summary>
        ///To check has error on form
        /// </summary>
        /// <returns></returns>
        private bool IsError()
        {
            if (this.SelectedItemEmployee == null)
                return false;
            if (this.IsValid || this.HasError())
                return true;
            return false;
            //return this.IsValid ;//&& (this.SelectedItemEmployee.AddressCollection == null || (this.SelectedItemEmployee.AddressCollection.Where(x => x.HasError).Count() > 0)));
        }

        private bool HasError()
        {
            if (this.SelectedItemEmployee.AddressCollection == null)
                return false;
            return (this.SelectedItemEmployee.AddressCollection.Where(x => x.HasError).Count() > 0);
        }
        #endregion

        #region LoadDataSelected
        /// <summary>
        /// To load data when an item is selected.
        /// </summary>
        private void LoadDataWhenSelected()
        {
            this.SetDataDefaultToModel(this.SelectedItemEmployee);
            this.SetDataRelationToModel(this.SelectedItemEmployee);
            this.SelectedItemEmployee.PropertyChanged += new PropertyChangedEventHandler(SelectedItemEmployee_PropertyChanged);
            this.SelectedItemEmployee.IsDirty = false;
        }
        #endregion

        #region Save Image
        /// <summary>
        /// Save images into folder if this ImageCollection have data.
        /// </summary>
        /// 
        string IMG_EMPLOYEE_DIRECTORY = System.IO.Path.Combine(Define.CONFIGURATION.DefautlImagePath, "Employee\\");
        private void SaveImage(base_ResourcePhotoModel model)
        {
            try
            {
                string imgGuestDirectory = IMG_EMPLOYEE_DIRECTORY + this.SelectedItemEmployee.GuestNo + "\\";
                if (!System.IO.Directory.Exists(imgGuestDirectory))
                    System.IO.Directory.CreateDirectory(imgGuestDirectory);
                ///To check file on client and copy file to server
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(model.ImagePath);
                if (fileInfo.Exists)
                {
                    ///To copy image to server
                    string filename = System.IO.Path.Combine(imgGuestDirectory, model.LargePhotoFilename);
                    System.IO.FileInfo file = new System.IO.FileInfo(filename);
                    if (!file.Exists)
                        fileInfo.CopyTo(filename, true);
                    model.ImagePath = filename;
                }
                else
                    model.ImagePath = string.Empty;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("Save Image" + ex.ToString());
            }
        }
        #endregion

        #region InitialData
        /// <summary>
        /// To load data of Employee from DB.
        /// </summary>
        private void InitialData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.FilterText))
                {
                    Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
                    predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_employeeMark));
                    this.LoadDataByPredicate(predicate, true);
                }
                else
                {
                    Expression<Func<base_Guest, bool>> predicate = this.CreatePredicateWithConditionSearch(this.FilterText);
                    this.LoadDataByPredicate(predicate, true);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region CreatePredicateWithConditionSearch
        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreatePredicateWithConditionSearch(string keyword)
        {
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Set conditions for predicate.
                predicate = PredicateBuilder.False<base_Guest>();
                //To search data with GuestNo.
                if (this.ColumnCollection.Contains(SearchOptions.AccountNum.ToString()))
                    predicate = predicate.Or(x => x.GuestNo.Contains(keyword.ToLower()));
                //To search data with Status.
                if (this.ColumnCollection.Contains(SearchOptions.Status.ToString()))
                {
                    IEnumerable<bool> EmStatus = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value == (int)StatusBasic.Active);
                    predicate = predicate.Or(x => EmStatus.Contains(x.IsActived));
                }
                //To search data with Employee Type.
                if (this.ColumnCollection.Contains(SearchOptions.Type.ToString()))
                {
                    IEnumerable<short> EmType = Common.EmployeeTypes.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                    predicate = predicate.Or(x => x.GuestTypeId.HasValue && EmType.Contains(x.GuestTypeId.Value));
                }
                //To search data with First Name.
                if (this.ColumnCollection.Contains(SearchOptions.FirstName.ToString()))
                    predicate = predicate.Or(x => x.FirstName.ToLower().Contains(keyword.ToLower()));
                //To search data with Last Name.
                if (this.ColumnCollection.Contains(SearchOptions.LastName.ToString()))
                    predicate = predicate.Or(x => x.LastName.ToLower().Contains(keyword.ToLower()));
                //To search data with Jotitle.
                if (this.ColumnCollection.Contains(SearchOptions.Position.ToString()))
                {
                    IEnumerable<short> Jobtitle = Common.JobTitles.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                    predicate = predicate.Or(x => x.PositionId.HasValue && Jobtitle.Contains(x.PositionId.Value));
                }
                //To search data with Company Type.
                if (this.ColumnCollection.Contains(SearchOptions.Company.ToString()))
                    predicate = predicate.Or(x => x.Company.ToLower().Contains(keyword.ToLower()));
                //To search data with Department.
                if (this.ColumnCollection.Contains(SearchOptions.Department.ToString()) && Common.Departments.Any())
                {
                    IEnumerable<int> departments = Common.Departments.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => Convert.ToInt32(x.Value));
                    predicate = predicate.Or(x => departments.Contains(x.Department));
                }
                //To search data with Phone.
                if (this.ColumnCollection.Contains(SearchOptions.Phone.ToString()))
                    predicate = predicate.Or(x => x.Phone1.ToLower().Contains(keyword.ToLower()) || x.Phone2.ToLower().Contains(keyword.ToLower()));
                //To search data with Email.
                if (this.ColumnCollection.Contains(SearchOptions.Email.ToString()))
                    predicate = predicate.Or(x => x.Email.ToLower().Contains(keyword.ToLower()));
            }
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_employeeMark));
            return predicate;
        }

        #endregion

        #region LoadDataByPredicate
        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(Expression<Func<base_Guest, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                EmployeeCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;
                //Get data with range
                IList<base_Guest> employees = _guestRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                foreach (base_Guest employee in employees)
                    bgWorker.ReportProgress(0, employee);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_GuestModel employeeModel = new base_GuestModel((base_Guest)e.UserState);
                SetToEmployeeModel(employeeModel);
                this.EmployeeCollection.Add(employeeModel);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                base.IsBusy = false;
                //Cout all Employee in Data base show on grid
                this.NumberOfItems = _guestRepository.GetIQueryable(predicate).Count();
            };
            bgWorker.RunWorkerAsync();
        }

        #endregion

        #region SetToEmployeeModel
        /// <summary>
        /// Set Data to Employee model when load to datagrid
        /// </summary>
        /// <param name="employeeModel"></param>
        private void SetToEmployeeModel(base_GuestModel employeeModel)
        {
            SetDepartmentComboboxItem(employeeModel);
        }

        /// <summary>
        /// Set Department for combobox Item
        /// </summary>
        /// <param name="employeeModel"></param>
        private void SetDepartmentComboboxItem(base_GuestModel employeeModel)
        {
            ComboItem departmentItem = null;
            if (Common.Departments.Any())
            {
                departmentItem = Common.Departments.SingleOrDefault(x => Convert.ToInt32(x.ObjValue).Equals(employeeModel.Department));
            }
            if (departmentItem != null)
            {
                employeeModel.DepartmentItem = departmentItem;
            }
            else
            {
                employeeModel.DepartmentItem = new ComboItem() { Text = string.Empty };
            }
        }
        #endregion

        #region SetDataDefaultToModel
        /// <summary>
        /// Set data default for
        /// </summary>
        /// <param name="employeeModel"></param>
        private void SetDataDefaultToModel(base_GuestModel employeeModel)
        {
            //Load PersonalInfoModel
            if (employeeModel.base_Guest.base_GuestProfile.Count > 0)
            {
                employeeModel.PersonalInfoModel = new base_GuestProfileModel(employeeModel.base_Guest.base_GuestProfile.First());
                this.IsSpouse = employeeModel.PersonalInfoModel.IsSpouse;
                this.IsEmergency = employeeModel.PersonalInfoModel.IsEmergency;
            }
            else
            {
                employeeModel.PersonalInfoModel = new base_GuestProfileModel();
                employeeModel.PersonalInfoModel.IsSpouse = false;
                employeeModel.PersonalInfoModel.IsEmergency = false;
                this.IsSpouse = false;
                this.IsEmergency = false;
                employeeModel.PersonalInfoModel.IsDirty = false;
            }
            //Load DefaultAdress Address
            if (employeeModel.base_Guest.base_GuestAddress.Count > 0)
                employeeModel.AddressModel = new base_GuestAddressModel(employeeModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.IsDefault));
            //Load Schedule
            if (employeeModel.base_Guest.base_GuestSchedule != null
                      && employeeModel.base_Guest.base_GuestSchedule.Count > 0
                      && employeeModel.base_Guest.base_GuestSchedule.Where(x => x.Status > 0).Count() > 0)
            {
                if (employeeModel.base_Guest.base_GuestSchedule.Count == 1)
                    employeeModel.EmployeeWorkScheduleName = employeeModel.base_Guest.base_GuestSchedule.First().tims_WorkSchedule.WorkScheduleName;
                var query = employeeModel.base_Guest.base_GuestSchedule.Where(x => x.Status > 0).LastOrDefault();
                if (query != null)
                    employeeModel.EmployeeWorkScheduleName = query.tims_WorkSchedule.WorkScheduleName;
            }
            else
                employeeModel.EmployeeWorkScheduleName = "Employee not work schedule";
            //Load FingerPrint
            employeeModel.EmployeeFingerprintCollection = new CollectionBase<base_GuestFingerPrintModel>(
                            employeeModel.base_Guest.base_GuestFingerPrint.Select(x => new base_GuestFingerPrintModel(x)));
            if (employeeModel.EmployeeFingerprintCollection.Count > 0)
            {
                employeeModel.HasFingerPrintLeft = employeeModel.EmployeeFingerprintCollection.Any(x => !x.HandFlag);
                employeeModel.HasFingerPrintRight = employeeModel.EmployeeFingerprintCollection.Any(x => x.HandFlag);
            }
            //Load resource note collection
            LoadResourceNoteCollection(employeeModel);
            employeeModel.IsDirty = false;
        }

        #endregion

        #region SetDataRelationToModel
        /// <summary>
        /// Load data relation in form.
        /// Using for initial data or rollback data
        /// <para>Item will be set : AddressCollection,AddressControlCollection,PhotoCollection</para>
        /// </summary>
        /// <param name="employeeModel"></param>
        private void SetDataRelationToModel(base_GuestModel employeeModel)
        {
            ///To load data from DB.
            if (employeeModel != null)
            {
                //Load Address Model
                employeeModel.AddressCollection = new ObservableCollection<base_GuestAddressModel>(employeeModel.base_Guest.base_GuestAddress.Select(x => new base_GuestAddressModel(x)));
                //AddressCollection For "Address Control"
                employeeModel.AddressControlCollection = new AddressControlCollection();
                //Set AddresssModel to address for "Address Control"
                foreach (base_GuestAddressModel guestAddressModel in employeeModel.AddressCollection)
                {
                    AddressControlModel addressControlModel = guestAddressModel.ToAddressControlModel();
                    addressControlModel.IsDirty = false;
                    addressControlModel.IsChangeData = false;
                    employeeModel.AddressControlCollection.Add(addressControlModel);
                }
                //Load PhotoCollection
                this.SelectedItemEmployee.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
                this.LoadResourcePhoto(this.SelectedItemEmployee);
                this.StickyManagementViewModel.SetParentResource(SelectedItemEmployee.Resource.ToString(), SelectedItemEmployee.ResourceNoteCollection);
                employeeModel.IsDirty = false;
            }
        }

        #endregion

        #region LoadResourcePhoto
        /// <summary>
        /// Load Resource Photo Collection & DefaultPhoto for GuestModel
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadResourcePhoto(base_GuestModel guestModel)
        {
            CollectionBase<base_ResourcePhotoModel> images = new CollectionBase<base_ResourcePhotoModel>();
            if (guestModel.Picture != null && guestModel.Picture.Length > 0)
            {
                base_ResourcePhotoModel ResourcePhotoModel = new base_ResourcePhotoModel();
                ResourcePhotoModel.ImageBinary = guestModel.Picture;
                ResourcePhotoModel.IsDirty = false;
                ResourcePhotoModel.IsNew = false;
                images.Add(ResourcePhotoModel);
                guestModel.PhotoCollection = images;
                if (guestModel.PhotoCollection.Count > 0)
                    guestModel.PhotoDefault = guestModel.PhotoCollection.FirstOrDefault();
                else
                    guestModel.PhotoDefault = new base_ResourcePhotoModel();
            }
        }
        #endregion

        #region FingerFrint
        /// <summary>
        /// Check Finger Print driver had setup 
        /// </summary>
        /// <returns>
        /// True : finger print setup
        /// False : Support
        /// </returns>
        private bool FingerPrintNotSupport()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\DigitalPersona");
                if (registryKey == null)
                    return true;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
            return false;
        }
        #endregion

        #region CheckDuplicateGuestNo
        void SelectedItemEmployee_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_GuestModel employeeModel = sender as base_GuestModel;
            switch (e.PropertyName)
            {
                case "GuestNo":
                    this.CheckDuplicateGuestNo(employeeModel);
                    break;
                case "Department":
                    LoadManagerResource(employeeModel.Resource.Value);
                    break;
                case "ManagerResource":
                    //Check percent of commission of this employee.
                    if (!string.IsNullOrEmpty(this.SelectedItemEmployee.ManagerResource))
                    {
                        Guid managerresource = Guid.Parse(this.SelectedItemEmployee.ManagerResource);
                        var managerCommssion = this._guestRepository.Get(x => x.Resource != null && x.Resource == managerresource);
                        if (managerCommssion != null)
                            this.SelectedItemEmployee.ManagerCommission = managerCommssion.CommissionPercent;
                    }
                    else
                        this.SelectedItemEmployee.ManagerCommission = 0;
                    break;

                case "CommissionPercent":
                    //Check percent of commission of this employee.
                    if (!string.IsNullOrEmpty(this.SelectedItemEmployee.ManagerResource))
                    {
                        Guid managerresource = Guid.Parse(this.SelectedItemEmployee.ManagerResource);
                        var managerCommssion = this._guestRepository.Get(x => x.Resource != null && x.Resource == managerresource);
                        if (managerCommssion != null)
                            this.SelectedItemEmployee.ManagerCommission = managerCommssion.CommissionPercent;
                    }
                    break;

                default:
                    break;
            }
        }

        private bool CheckDuplicateGuestNo(base_GuestModel employeeModel)
        {
            bool result = false;
            try
            {
                lock (UnitOfWork.Locker)
                {
                    IQueryable<base_Guest> query = _guestRepository.GetIQueryable(x => x.Mark == this._employeeMark && x.Resource != employeeModel.Resource && x.GuestNo.Equals(employeeModel.GuestNo));
                    if (query.Any())
                        result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _log4net.Error(ex);
            }
            employeeModel.IsDuplicateGuestNo = result;
            return result;
        }
        #endregion

        #region LoadManagerResource
        private void LoadManagerResource(Guid resource)
        {
            if (this.SelectedItemEmployee.ManagerResourceCollection == null)
                this.SelectedItemEmployee.ManagerResourceCollection = new ObservableCollection<ItemModel>();
            else
                this.SelectedItemEmployee.ManagerResourceCollection.Clear();

            var list = _guestRepository.GetIQueryable(x => x.Resource != resource && x.Mark.Equals(_employeeMark) && x.Department == this.SelectedItemEmployee.Department);
            ItemModel model;
            if (list != null)
            {
                this.SelectedItemEmployee.ManagerResourceCollection.Add(new ItemModel { Id = -1, Resource = string.Empty, Text = string.Empty });
                foreach (var item in list)
                {
                    model = new ItemModel();
                    model.Id = item.Id;
                    model.Resource = item.Resource.ToString();
                    model.Text = string.Format("{0} {1}", item.FirstName, item.LastName);
                    this.SelectedItemEmployee.ManagerResourceCollection.Add(model);
                }
            }
        }
        #endregion

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
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                this._waitingTimer.Stop();
                this._waitingTimer.Start();
                _timerCounter = 0;
            }
        }

        #endregion

        #region Override Methods

        #region LoadData
        public override void LoadData()
        {
            this.InitialData();
        }
        #endregion

        #region ChangeSearchMode
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (this.ChangeViewExecute(null))
                {
                    if (!isList)
                    {
                        this.CreateEmployee();
                        IsSearchMode = false;
                    }
                    else
                        IsSearchMode = true;
                }
            }
        }
        #endregion

        #region OnViewChangingCommandCanExecute
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ChangeViewExecute(isClosing);
        }
        #endregion

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
        /// <param name="guestModel"></param>
        private void LoadResourceNoteCollection(base_GuestModel guestModel)
        {
            // Load resource note collection
            if (guestModel.ResourceNoteCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        #endregion
    }
}