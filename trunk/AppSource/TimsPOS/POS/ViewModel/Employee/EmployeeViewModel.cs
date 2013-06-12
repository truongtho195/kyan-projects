using System;
using System.Collections;
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
using CPCToolkitExtLibraries;
using Microsoft.Win32;
using System.Windows.Input;

namespace CPC.POS.ViewModel
{
    class EmployeeViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand<object> SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand NoteCommand { get; private set; }

        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();
        private base_GuestAdditionalRepository _guestAdditionalRepository = new base_GuestAdditionalRepository();
        private base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();

        private BackgroundWorker _bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

        private string _employeeMark = MarkType.Employee.ToDescription();
        #endregion

        #region Constructors

        public EmployeeViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            InitialCommand();
            Parameter = new Common();


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
                    || SelectedItemEmployee.PersonalInfoModel.IsDirty);
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
                    OnPropertyChanged(() => FilterText);
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
            if (ChangeViewExecute(null))
            {
                CreateEmployee();
                IsSearchMode = false;
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
            if (SelectedItemEmployee == null)
                return false;
            return IsValid && this.IsDirty &&
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
            if (SelectedItemEmployee == null)
                return false;
            return !SelectedItemEmployee.IsNew && !IsDirty;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult result = MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo);
            if (result.Is(MessageBoxResult.Yes))
            {
                if (SelectedItemEmployee.IsNew)
                {
                    DeleteNote();
                    SelectedItemEmployee = null;
                    IsSearchMode = true;
                }
                else
                {
                    List<ItemModel> ItemModel = new List<ItemModel>();
                    string resource = SelectedItemEmployee.Resource.Value.ToString();
                    if (!_saleCommissionRepository.GetAll().Select(x => x.GuestResource).Contains(resource))
                    {
                        SelectedItemEmployee.IsPurged = true;
                        SaveEmployee();
                        EmployeeCollection.Remove(SelectedItemEmployee);
                        SelectedItemEmployee = EmployeeCollection.First();
                        NumberOfItems = NumberOfItems - 1;
                        DeleteNote();
                        IsSearchMode = true;
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
                SearchAlert = string.Empty;
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)//Search All
                {
                    Expression<Func<base_Guest, bool>> predicate = CreatePredicateWithConditionSearch(Keyword);
                    LoadDataByPredicate(predicate, false, 0);


                }
                else if (param != null)
                {
                    Keyword = param.ToString();
                    if (SearchOption == 0)
                    {
                        //Thong bao Can co dk
                        SearchAlert = "Search Option is required";
                    }
                    else
                    {
                        Expression<Func<base_Guest, bool>> predicate = CreatePredicateWithConditionSearch(Keyword);
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

        #region DoubleClickViewCommand
        /// <summary>
        /// Method to check whether the DoubleClickViewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        private void OnDoubleClickViewCommandExecute(object param)
        {

            if (param != null && IsSearchMode)
            {
                SelectedItemEmployee = param as base_GuestModel;
                this.LoadDataWhenSelected();
                IsSearchMode = false;
            }
            else if (!IsSearchMode)//Change from Edit form to Search Gird check view has dirty
            {
                if (this.ChangeViewExecute(null))
                    this.IsSearchMode = true;
            }
            else
                this.IsSearchMode = !this.IsSearchMode;//Change View To
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
            if (SelectedItemEmployee.ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
                return;

            // Create a new note
            base_ResourceNoteModel noteModel = new base_ResourceNoteModel
            {
                Resource = SelectedItemEmployee.Resource.ToString(),
                Color = Define.DefaultColorNote,
                DateCreated = DateTimeExt.Now
            };

            // Create default position for note
            Point position = new Point(600, 200);
            if (SelectedItemEmployee.ResourceNoteCollection.Count > 0)
            {
                Point lastPostion = SelectedItemEmployee.ResourceNoteCollection.LastOrDefault().Position;
                if (lastPostion != null)
                    position = new Point(lastPostion.X + 10, lastPostion.Y + 10);
            }

            // Update position
            noteModel.Position = position;

            // Add new note to collection
            SelectedItemEmployee.ResourceNoteCollection.Add(noteModel);

            // Show new note
            PopupContainer popupContainer = CreatePopupNote(noteModel);
            popupContainer.Show();
            NotePopupCollection.Add(popupContainer);
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
            if (NotePopupCollection.Count == SelectedItemEmployee.ResourceNoteCollection.Count)
            {
                // Created popup notes, only show or hidden them
                if (ShowOrHiddenNote.Equals("Hide Stickies"))
                {
                    foreach (PopupContainer popupContainer in NotePopupCollection)
                        popupContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (PopupContainer popupContainer in NotePopupCollection)
                        popupContainer.Show();
                }
            }
            else
            {
                // Close all note
                CloseAllPopupNote();

                Point position = new Point(600, 200);
                foreach (base_ResourceNoteModel noteModel in SelectedItemEmployee.ResourceNoteCollection)
                {
                    noteModel.Position = position;
                    PopupContainer popupContainer = CreatePopupNote(noteModel);
                    popupContainer.Show();
                    NotePopupCollection.Add(popupContainer);
                    position = new Point(position.X + 10, position.Y + 10);
                }
            }

            // Update label "Show/Hidden Note"
            OnPropertyChanged(() => ShowOrHiddenNote);
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
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_employeeMark));
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = CreatePredicateWithConditionSearch(Keyword);
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
                base_GuestFingerPrintModel employeeFingerPrintModel = this.SelectedItemEmployee.EmployeeFingerprintCollection.SingleOrDefault(x => x.FingerIndex == viewModel.FingerID && x.HandFlag == rightHand);
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
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this vendor?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                bool flag = false;
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
                        this.DeleteNoteExt(model);
                        i--;
                    }
                    else
                    {
                        ItemModel.Add(new ItemModel { Id = model.Id, Text = model.GuestNo, Resource = resource });
                        flag = true;
                    }
                }
                if (flag)
                    _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "Employee"), "Problem Detection");
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
            this.NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            this.SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            this.NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            this.ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            this.RecordFingerprintCommand = new RelayCommand<object>(OnRecordFingerprintCommandExecute, OnRecordFingerprintCommandCanExecute);
            this.DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
            ///To load AddressTypeCollection
            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = "Home" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = "Business" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });


            NotePopupCollection = new ObservableCollection<PopupContainer>();
            NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };

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
            this.SelectedItemEmployee.IsActived = true;
            this.SelectedItemEmployee.GuestTypeId = 1;
            this.SelectedItemEmployee.IsPrimary = false;
            this.SelectedItemEmployee.Company = string.Empty;
            this.SelectedItemEmployee.Department = string.Empty;
            this.SelectedItemEmployee.DateCreated = DateTime.Now;
            this.SelectedItemEmployee.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            this.SelectedItemEmployee.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
            this.SelectedItemEmployee.Mark = MarkType.Employee.ToDescription();
            this.SelectedItemEmployee.PositionId = 0;
            this.SelectedItemEmployee.OvertimeOption = 0;
            SelectedItemEmployee.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();

            //Personal Info
            this.SelectedItemEmployee.PersonalInfoModel = new base_GuestProfileModel();
            this.SelectedItemEmployee.PersonalInfoModel.IsSpouse = false;
            this.SelectedItemEmployee.PersonalInfoModel.SEmail = string.Empty;
            this.SelectedItemEmployee.PersonalInfoModel.DOB = DateTime.Today.AddYears(-10);
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
            this.SelectedItemEmployee.IsDirty = false;
        }
        #endregion

        #region SaveEmployee
        /// <summary>
        /// Function save Employee
        /// </summary>
        /// <param name="param"></param>
        private bool SaveEmployee()
        {
            bool result = true;
            try
            {
                //To close detail grid of Employee after saving data.
                if (this.SelectedItemEmployee.IsNew)
                    this.Insert();
                //To update item when it is edited.
                else
                    this.Update();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString(), "Error");
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
                SelectedItemEmployee.PersonalInfoModel.ToEntity();
                if (SelectedItemEmployee.PersonalInfoModel.IsNew)
                    SelectedItemEmployee.base_Guest.base_GuestProfile.Add(SelectedItemEmployee.PersonalInfoModel.base_GuestProfile);
                SelectedItemEmployee.PersonalInfoModel.EndUpdate();
            }

            ///Created by Thaipn.
            base_GuestAddressModel addressModel;
            bool firstAddress = true;
            //To insert an address. 
            //Convert from AddressControlCollection To AddressModel 
            foreach (AddressControlModel addressControlModel in this.SelectedItemEmployee.AddressControlCollection)
            {
                addressModel = new base_GuestAddressModel();
                addressModel.UserCreated = string.Empty;
                addressModel.ToModel(addressControlModel);
                addressModel.IsDefault = firstAddress;
                addressModel.EndUpdate();
                // To convert data from model to entity
                addressModel.ToEntity();
                this.SelectedItemEmployee.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                firstAddress = false;
                addressModel.EndUpdate();
                addressControlModel.IsDirty = false;
                addressControlModel.IsNew = false;

            }

            //Save FingerPrint
            foreach (base_GuestFingerPrintModel fingerPrintModel in SelectedItemEmployee.EmployeeFingerprintCollection)
            {
                fingerPrintModel.ToEntity();
                if (fingerPrintModel.IsNew)
                    SelectedItemEmployee.base_Guest.base_GuestFingerPrint.Add(fingerPrintModel.base_GuestFingerPrint);
                fingerPrintModel.EndUpdate();

            }

            SavePhotoResource(this.SelectedItemEmployee);
            //To commit image.
            _guestRepository.Add(this.SelectedItemEmployee.base_Guest);
            _guestRepository.Commit();

            // To update ID from entity to model
            this.SelectedItemEmployee.Id = this.SelectedItemEmployee.base_Guest.Id;

            // To turn off IsDirty & IsNew
            this.SelectedItemEmployee.EndUpdate();
            this.EmployeeCollection.Add(this.SelectedItemEmployee);
        }
        #endregion

        #region Update
        /// <summary>
        /// To update item when it was edited.
        /// </summary>
        private void Update()
        {
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

            // Insert or update address
            // Created by Thaipn
            foreach (AddressControlModel addressControlModel in this.SelectedItemEmployee.AddressControlCollection.Where(x => x.IsDirty))
            {
                base_GuestAddressModel addressModel = new base_GuestAddressModel();

                // Insert new address
                if (addressControlModel.IsNew)
                {
                    addressModel.DateCreated = DateTimeExt.Now;
                    addressModel.UserCreated = string.Empty;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);

                    // Map data from model to entity
                    addressModel.ToEntity();
                    SelectedItemEmployee.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                    addressModel.EndUpdate();

                }
                // Update address
                else
                {
                    base_GuestAddress address = SelectedItemEmployee.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId == addressControlModel.AddressTypeID);
                    addressModel = new base_GuestAddressModel(address);

                    addressModel.DateUpdated = DateTimeExt.Now;
                    //addressModel.UserUpdated = string.Empty;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);
                    addressModel.ToEntity();
                }

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

            //Save,Update or delete PhotoResource
            SavePhotoResource(this.SelectedItemEmployee);

            _guestRepository.Commit();
            // To turn off IsDirty & IsNew
            this.SelectedItemEmployee.EndUpdate();
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
                msgResult = MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                    {
                        //if (SaveCustomer())
                        result = SaveEmployee();
                    }
                    else //Has Error
                        result = false;

                    // Remove popup note
                    CloseAllPopupNote();
                }
                else
                {
                    if (SelectedItemEmployee.IsNew)
                    {
                        DeleteNote();
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                        //SelectedItemEmployee = null;
                    }
                    else //Old Item Rollback data
                    {
                        DeleteNote();
                        SelectedItemEmployee.ToModelAndRaise();
                        SetDataDefaultToModel(SelectedItemEmployee);
                        SetDataRelationToModel(SelectedItemEmployee);
                    }
                }
            }
            else
            {
                if (SelectedItemEmployee != null && SelectedItemEmployee.IsNew)
                {
                    DeleteNote();
                }
                else
                { // Remove popup note
                    CloseAllPopupNote();
                }
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

        #region LoadDataWhenSelected
        /// <summary>
        /// To load data when an item is selected.
        /// </summary>
        private void LoadDataWhenSelected()
        {
            SetDataRelationToModel(SelectedItemEmployee);
        }
        #endregion

        #region Save Image
        /// <summary>
        /// Save images into folder if this ImageCollection have data.
        /// </summary>
        /// 
        string IMG_EMPLOYEE_DIRECTORY = System.IO.Path.Combine(Define.ImageFilesFolder, "Employee\\");
        private void SaveImage(base_ResourcePhotoModel model)
        {
            try
            {
                string imgGuestDirectory = IMG_EMPLOYEE_DIRECTORY + SelectedItemEmployee.GuestNo + "\\";

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
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(Keyword) && SearchOption > 0)//Load with Search Condition
            {
                predicate = CreatePredicateWithConditionSearch(Keyword);
            }
            else
            {
                predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_employeeMark));
            }
            LoadDataByPredicate(predicate, true);


            /////Thai Pham
            //this.NumberOfItems = 100;
            //this.EmployeeCollection = new ObservableCollection<base_GuestModel>();
            /////To get AddressType.
            //foreach (var employee in _guestRepository.GetAll(x => x.Mark.Equals("E")))
            //{
            //    base_GuestModel model = new base_GuestModel(employee);
            //    model.AddressControlCollection = new AddressControlCollection();
            //    //To load Address.
            //    AddressControlModel addressModel = new AddressControlModel();
            //    addressModel.AddressTypeID = model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).AddressTypeId;
            //    addressModel.AddressLine1 = model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).AddressLine1;
            //    addressModel.AddressLine2 = model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).AddressLine2;
            //    addressModel.City = model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).City;
            //    addressModel.StateProvinceID = Int16.Parse(model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).StateProvinceId.ToString());
            //    addressModel.PostalCode = model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).PostalCode;
            //    addressModel.CountryID = Int16.Parse(model.base_Guest.base_GuestAddress.SingleOrDefault(y => y.IsDefault).CountryId.ToString());
            //    addressModel.IsNew = false;
            //    addressModel.IsDirty = false;
            //    model.AddressControlCollection.Add(addressModel);
            //    model.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            //    //To load Image.
            //    if (model.base_Guest.base_GuestPhoto != null && model.base_Guest.base_GuestPhoto.Count > 0)
            //    {
            //        base_ResourcePhotoModel photoModel = new base_ResourcePhotoModel(model.base_Guest.base_GuestPhoto.First());
            //        photoModel.ImagePath = IMG_EMPLOYEE_DIRECTORY + @"\" + photoModel.LargePhotoFilename;
            //        photoModel.EndUpdate();
            //        model.PhotoCollection.Add(photoModel);
            //    }
            //    model.IsNew = false;
            //    model.IsDirty = false;
            //    this.EmployeeCollection.Add(model);
            //}
        }
        #endregion

        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreatePredicateWithConditionSearch(string keyword)
        {
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.AccountNum))
                {
                    predicate = predicate.And(x => x.GuestNo.Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.FirstName))
                {
                    predicate = predicate.And(x => x.FirstName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.LastName))
                {
                    predicate = predicate.And(x => x.LastName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Company))
                {
                    predicate = predicate.And(x => x.Company.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Phone))
                {
                    predicate = predicate.And(x => x.Phone1.ToLower().Contains(keyword.ToLower()) || x.Phone2.ToLower().Contains(keyword.ToLower()));
                }
            }
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_employeeMark));
            return predicate;
        }

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
                IsBusy = true;
                if (refreshData)
                {
                    _resourceNoteRepository.Refresh();
                    _guestRepository.Refresh();
                    _guestAdditionalRepository.Refresh();
                    _guestAddressRepository.Refresh();
                    _photoRepository.Refresh();
                }
                //Cout all Customer in Data base show on grid
                NumberOfItems = _guestRepository.GetIQueryable(predicate).Count();

                //Get data with range
                IList<base_Guest> employees = _guestRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                foreach (base_Guest employee in employees)
                {
                    bgWorker.ReportProgress(0, employee);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_GuestModel employeeModel = new base_GuestModel((base_Guest)e.UserState);
                SetDataDefaultToModel(employeeModel);
                EmployeeCollection.Add(employeeModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }


        /// <summary>
        /// Set data default for
        /// </summary>
        /// <param name="employeeModel"></param>
        private void SetDataDefaultToModel(base_GuestModel employeeModel)
        {
            //Load PersonalInfoModel
            if (employeeModel.base_Guest.base_GuestProfile.Count > 0)
                employeeModel.PersonalInfoModel = new base_GuestProfileModel(employeeModel.base_Guest.base_GuestProfile.First());
            else
            {
                employeeModel.PersonalInfoModel = new base_GuestProfileModel();
                employeeModel.PersonalInfoModel.SEmail = string.Empty;
                employeeModel.PersonalInfoModel.DOB = DateTime.Today.AddYears(-10);
                employeeModel.PersonalInfoModel.IsSpouse = false;
                employeeModel.PersonalInfoModel.IsEmergency = false;
                employeeModel.PersonalInfoModel.Gender = Common.Gender.First().Value;
                employeeModel.PersonalInfoModel.Marital = Common.MaritalStatus.First().Value;
                employeeModel.PersonalInfoModel.SGender = Common.Gender.First().Value;
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
                {
                    employeeModel.EmployeeWorkScheduleName = employeeModel.base_Guest.base_GuestSchedule.First().tims_WorkSchedule.WorkScheduleName;
                }
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

            //Load Resource Photo
            LoadResourcePhoto(employeeModel);

            //Load Note Resource
            LoadNote(employeeModel);
            employeeModel.IsDirty = false;
        }

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
                    employeeModel.AddressControlCollection.Add(addressControlModel);
                }

                //Load PhotoCollection
                LoadResourcePhoto(SelectedItemEmployee);
                employeeModel.IsDirty = false;
            }
        }

        /// <summary>
        /// Load Resource Photo Collection & DefaultPhoto for GuestModel
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadResourcePhoto(base_GuestModel guestModel)
        {
            if (guestModel.PhotoCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(
                    _photoRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourcePhotoModel(x)
                    {
                        ImagePath = System.IO.Path.Combine(IMG_EMPLOYEE_DIRECTORY, guestModel.GuestNo, x.LargePhotoFilename),
                        IsDirty = false
                    }));

                if (guestModel.PhotoCollection.Count > 0)
                    guestModel.PhotoDefault = guestModel.PhotoCollection.FirstOrDefault();
                else
                    guestModel.PhotoDefault = new base_ResourcePhotoModel();
            }
        }

        /// <summary>
        /// SaveNew,Update or detete photo to resource & set photo default for guestModel
        /// </summary>
        private void SavePhotoResource(base_GuestModel guestModel)
        {
            //To remove image deleted.
            if (guestModel.PhotoCollection.DeletedItems != null
                && guestModel.PhotoCollection.DeletedItems.Count > 0)
            {
                foreach (base_ResourcePhotoModel item in guestModel.PhotoCollection.DeletedItems)
                {
                    _photoRepository.Delete(item.base_ResourcePhoto);
                }
                _photoRepository.Commit();
                guestModel.PhotoCollection.DeletedItems.Clear();
            }

            //To update image.
            if (guestModel.PhotoCollection != null && guestModel.PhotoCollection.Count > 0)
            {
                foreach (base_ResourcePhotoModel photoModel in guestModel.PhotoCollection.Where(x => x.IsDirty))
                {
                    photoModel.Resource = guestModel.Resource.ToString();
                    photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;
                    //To map data from model to entity
                    photoModel.ToEntity();
                    if (photoModel.IsNew)
                        _photoRepository.Add(photoModel.base_ResourcePhoto);

                    //To save image to store.
                    this.SaveImage(photoModel);
                    _photoRepository.Commit();

                    //set Id
                    photoModel.Id = photoModel.base_ResourcePhoto.Id;
                    photoModel.EndUpdate();
                }

                if (guestModel.PhotoCollection.Count > 0)
                    guestModel.PhotoDefault = guestModel.PhotoCollection.FirstOrDefault();
                else
                    guestModel.PhotoDefault = new base_ResourcePhotoModel();
            }
        }

        #region Note Methods

        /// <summary>
        /// Load notes
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadNote(base_GuestModel guestModel)
        {
            // Load Note
            if (guestModel.ResourceNoteCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
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
            // Remove popup note
            CloseAllPopupNote();

            // Delete note
            foreach (base_ResourceNoteModel noteModel in SelectedItemEmployee.ResourceNoteCollection)
                _resourceNoteRepository.Delete(noteModel.base_ResourceNote);
            _resourceNoteRepository.Commit();

            SelectedItemEmployee.ResourceNoteCollection.Clear();
        }

        /// <summary>
        /// Close popup notes
        /// </summary>
        private void CloseAllPopupNote()
        {
            // Remove popup note
            foreach (PopupContainer popupContainer in NotePopupCollection)
                popupContainer.Close();
            NotePopupCollection.Clear();
        }

        /// <summary>
        /// Create popup note
        /// </summary>
        /// <param name="noteModel"></param>
        /// <returns></returns>
        private PopupContainer CreatePopupNote(base_ResourceNoteModel noteModel)
        {
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.SelectedNote = noteModel;
            noteViewModel.NotePopupCollection = NotePopupCollection;
            noteViewModel.ResourceNoteCollection = SelectedItemEmployee.ResourceNoteCollection;
            CPC.POS.View.NoteView noteView = new View.NoteView();

            PopupContainer popupContainer = new PopupContainer(noteView, true);
            popupContainer.WindowStartupLocation = WindowStartupLocation.Manual;
            popupContainer.DataContext = noteViewModel;
            popupContainer.Width = 150;
            popupContainer.Height = 120;
            popupContainer.MinWidth = 150;
            popupContainer.MinHeight = 120;
            popupContainer.FormBorderStyle = PopupContainer.BorderStyle.None;
            popupContainer.Deactivated += (sender, e) => { SaveNote(noteModel); };
            popupContainer.Loaded += (sender, e) =>
            {
                popupContainer.Left = noteModel.Position.X;
                popupContainer.Top = noteModel.Position.Y;
            };
            return popupContainer;
        }

        private void DeleteNoteExt(base_GuestModel model)
        {
            // Remove popup note
            CloseAllPopupNote();

            // Delete note
            foreach (base_ResourceNoteModel noteModel in model.ResourceNoteCollection)
                _resourceNoteRepository.Delete(noteModel.base_ResourceNote);
            _resourceNoteRepository.Commit();
            model.ResourceNoteCollection.Clear();
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
            catch
            {
                return true;
            }
            return false;
        }
        #endregion

        #endregion

        #region Override Methods

        #region LoadData

        public override void LoadData()
        {
            InitialData();
            //if (!this.IsSearchMode && this.SelectedItemEmployee == null)
            //    this.CreateEmployee();
        }
        #endregion

        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ChangeViewExecute(null))
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

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ChangeViewExecute(isClosing);
        }

        #endregion
    }
}
