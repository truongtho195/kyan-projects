using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class WorkScheduleViewModel : ViewModelBase
    {
        #region Defines

        tims_WorkScheduleRepository _workScheduleRepository = new tims_WorkScheduleRepository();
        tims_WorkWeekRepository _workWeekRepository = new tims_WorkWeekRepository();
        base_GuestRepository _employeeRepository = new base_GuestRepository();
        base_GuestScheduleRepository _employeeScheduleRepository = new base_GuestScheduleRepository();
        tims_TimeLogRepository _timeLogRepository = new tims_TimeLogRepository();

        #endregion

        #region Properties

        private ObservableCollection<tims_WorkScheduleModel> _workScheduleCollection = new ObservableCollection<tims_WorkScheduleModel>();
        /// <summary>
        /// Gets or sets the WorkScheduleCollection.
        /// </summary>
        public ObservableCollection<tims_WorkScheduleModel> WorkScheduleCollection
        {
            get { return _workScheduleCollection; }
            set
            {
                if (_workScheduleCollection != value)
                {
                    _workScheduleCollection = value;
                    OnPropertyChanged(() => WorkScheduleCollection);
                }
            }
        }

        private tims_WorkScheduleModel _selectedWorkSchedule;
        /// <summary>
        /// Gets or sets the SelectedWorkSchedule.
        /// </summary>
        public tims_WorkScheduleModel SelectedWorkSchedule
        {
            get { return _selectedWorkSchedule; }
            set
            {
                if (_selectedWorkSchedule != value)
                {
                    _selectedWorkSchedule = value;
                    OnPropertyChanged(() => SelectedWorkSchedule);
                }
            }
        }

        private bool _isSearchMode = true;
        /// <summary>
        /// Search Mode: 
        /// true open the Search grid.
        /// false close the search grid and open data entry.
        /// </summary>
        public bool IsSearchMode
        {
            get { return _isSearchMode; }
            set
            {
                if (_isSearchMode != value)
                {
                    _isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }

        private bool _focusDefault;
        /// <summary>
        /// Gets or sets the FocusDefault.
        /// </summary>
        public bool FocusDefault
        {
            get { return _focusDefault; }
            set
            {
                if (_focusDefault != value)
                {
                    _focusDefault = value;
                    OnPropertyChanged(() => FocusDefault);
                }
            }
        }

        private bool _isCheckingAll = false;

        private bool? _isCheckedAll = false;
        /// <summary>
        /// Gets or sets the IsCheckedAll.
        /// </summary>
        public bool? IsCheckedAll
        {
            get { return _isCheckedAll; }
            set
            {
                if (_isCheckedAll != value)
                {
                    _isCheckedAll = value;
                    OnPropertyChanged(() => IsCheckedAll);
                    if (IsCheckedAll.HasValue)
                    {
                        _isCheckingAll = true;
                        foreach (base_GuestModel employeeItem in SelectedWorkSchedule.EmployeeCollection)
                            employeeItem.IsChecked = IsCheckedAll.Value;
                        _isCheckingAll = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the WorkHour.
        /// </summary>
        public short WorkHour
        {
            get
            {
                if (Define.CONFIGURATION == null)
                    return 0;
                return Define.CONFIGURATION.WorkHour;
            }
        }

        /// <summary>
        /// Gets the WeekHour.
        /// </summary>
        public int WeekHour
        {
            get
            {
                if (Define.CONFIGURATION == null)
                    return 0;
                return Define.CONFIGURATION.WeekHour;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkScheduleViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            InitialCommand();
        }

        #endregion

        #region Command Methods

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
            return WorkScheduleCollection != null && WorkScheduleCollection.Count(x => x.IsNew) == 0;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (IsSearchMode)
            {
                IsSearchMode = false;
                NewWorkSchedule();
            }
            else if (ShowNotification(null))
                NewWorkSchedule();
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
            if (SelectedWorkSchedule == null)
                return false;

            return IsValid && IsEdit() &&
                !SelectedWorkSchedule.WorkWeekCollection.Any(x => x.DayOfWorkWeekCollection.Any(z => !string.IsNullOrWhiteSpace(z.Error)));
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SaveWorkSchedule(SelectedWorkSchedule);
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
        private bool OnDeleteCommandCanExecute(object param)
        {
            // Get work schedule model
            tims_WorkScheduleModel workScheduleModel = param as tims_WorkScheduleModel;

            return workScheduleModel != null && workScheduleModel.Status.Is(ScheduleStatuses.Pending);
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute(object param)
        {
            // Get work schedule model
            tims_WorkScheduleModel workScheduleModel = param as tims_WorkScheduleModel;

            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this item?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                _workScheduleRepository.Delete(workScheduleModel.tims_WorkSchedule);
                _workScheduleRepository.Commit();
                WorkScheduleCollection.Remove(workScheduleModel);
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
                EditWorkSchedule(param as tims_WorkScheduleModel);

                // Show detail form
                IsSearchMode = false;
            }
            else if (IsSearchMode)
            {
                // Show detail form
                IsSearchMode = false;
            }
            else if (ShowNotification(null))
            {
                // Show list form
                IsSearchMode = true;
            }
        }

        #endregion

        #region AssignEmployeeCommand

        /// <summary>
        /// Gets the AssignEmployeeCommand command.
        /// </summary>
        public ICommand AssignEmployeeCommand { get; private set; }

        /// <summary>
        /// Method to check whether the AssignEmployeeCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAssignEmployeeCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the AssignEmployeeCommand command is executed.
        /// </summary>
        private void OnAssignEmployeeCommandExecute()
        {
            // Set window title by work schedule name
            string title = string.IsNullOrWhiteSpace(SelectedWorkSchedule.WorkScheduleName) ? "POS" : SelectedWorkSchedule.WorkScheduleName;

            // Show assign work schedule popup
            AssignWorkScheduleViewModel viewModel = new AssignWorkScheduleViewModel(SelectedWorkSchedule);
            bool? result = _dialogService.ShowDialog<AssignWorkScheduleView>(_ownerViewModel, viewModel, title);
            if (result.HasValue && result.Value)
            {
                // Update employee collection
                UpdateEmployeeCollection(viewModel.RightEmployeeCollection, SelectedWorkSchedule.EmployeeCollection);
            }
        }

        #endregion

        #region UnassignEmployeeCommand

        /// <summary>
        /// Gets the UnassignEmployeeCommand command.
        /// </summary>
        public ICommand UnassignEmployeeCommand { get; private set; }

        /// <summary>
        /// Method to check whether the UnassignEmployeeCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnUnassignEmployeeCommandCanExecute()
        {
            if (SelectedWorkSchedule == null || (IsCheckedAll.HasValue && !IsCheckedAll.Value))
                return false;
            IEnumerable<base_GuestModel> selectedItems = SelectedWorkSchedule.EmployeeCollection.Where(x => x.IsChecked);
            int numberOfEmployeeSchedulePending = selectedItems.Count(x => x.EmployeeScheduleModel.Status.Is(EmployeeScheduleStatuses.Pending));
            return numberOfEmployeeSchedulePending > 0 && numberOfEmployeeSchedulePending == selectedItems.Count();
        }

        /// <summary>
        /// Method to invoke when the UnassignEmployeeCommand command is executed.
        /// </summary>
        private void OnUnassignEmployeeCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete these item?", "POS",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                foreach (base_GuestModel employeeModel in SelectedWorkSchedule.EmployeeCollection.Where(x => x.IsChecked).ToList())
                {
                    // Reset IsChecked before unassign employee
                    employeeModel.IsChecked = false;

                    SelectedWorkSchedule.EmployeeCollection.Remove(employeeModel);
                }
            }
        }

        #endregion

        #region PopupChangeWorkScheduleCommand

        /// <summary>
        /// Gets the PopupChangeWorkScheduleCommand command.
        /// </summary>
        public ICommand PopupChangeWorkScheduleCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupChangeWorkScheduleCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupChangeWorkScheduleCommandCanExecute(object param)
        {
            return param != null && WorkScheduleCollection.Count > 1;
        }

        /// <summary>
        /// Method to invoke when the PopupChangeWorkScheduleCommand command is executed.
        /// </summary>
        private void OnPopupChangeWorkScheduleCommandExecute(object param)
        {
            // Get employee schedule model
            tims_WorkScheduleModel workScheduleModel = param as tims_WorkScheduleModel;

            // Show change work schedule popup
            ChangeWorkScheduleViewModel viewModel = new ChangeWorkScheduleViewModel(workScheduleModel);
            bool? result = _dialogService.ShowDialog<ChangeWorkScheduleView>(_ownerViewModel, viewModel, "Change Work Schedule");
            if (result.HasValue && result.Value)
            {
                int currentWorkScheduleID = viewModel.CurrentWorkSchedule.Id;
                int changeToWorkScheduleID = viewModel.ChangeToWorkSchedule.Id;

                // Get current work schedule model
                tims_WorkScheduleModel currentWorkScheduleModel = WorkScheduleCollection.SingleOrDefault(x => x.Id.Equals(currentWorkScheduleID));

                // Reload employee collection for current work schedule
                currentWorkScheduleModel.EmployeeCollection = null;
                LoadEmployeeCollection(currentWorkScheduleModel);

                // Get change to work schedule model
                tims_WorkScheduleModel changeToWorkScheduleModel = WorkScheduleCollection.SingleOrDefault(x => x.Id.Equals(changeToWorkScheduleID));

                // Reload employee collection for change to work schedule
                changeToWorkScheduleModel.EmployeeCollection = null;
                LoadEmployeeCollection(changeToWorkScheduleModel);
            }
        }

        #endregion

        #region PopupEmployeeInformationDetailCommand

        /// <summary>
        /// Gets the PopupEmployeeInformationDetailCommand command.
        /// </summary>
        public ICommand PopupEmployeeInformationDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupEmployeeInformationDetailCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupEmployeeInformationDetailCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the PopupEmployeeInformationDetailCommand command is executed.
        /// </summary>
        private void OnPopupEmployeeInformationDetailCommandExecute(object param)
        {
            // Get employee model
            base_GuestModel employeeModel = param as base_GuestModel;

            // Show change work schedule popup
            EmployeeInformationDetailViewModel viewModel = new EmployeeInformationDetailViewModel(employeeModel);
            bool? result = _dialogService.ShowDialog<EmployeeInformationDetailView>(_ownerViewModel, viewModel, "Employee Information Detail");
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            AssignEmployeeCommand = new RelayCommand(OnAssignEmployeeCommandExecute, OnAssignEmployeeCommandCanExecute);
            UnassignEmployeeCommand = new RelayCommand(OnUnassignEmployeeCommandExecute, OnUnassignEmployeeCommandCanExecute);
            PopupChangeWorkScheduleCommand = new RelayCommand<object>(OnPopupChangeWorkScheduleCommandExecute, OnPopupChangeWorkScheduleCommandCanExecute);
            PopupEmployeeInformationDetailCommand = new RelayCommand<object>(OnPopupEmployeeInformationDetailCommandExecute, OnPopupEmployeeInformationDetailCommandCanExecute);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            if (SelectedWorkSchedule == null)
                return false;

            return SelectedWorkSchedule.IsDirty ||
                (SelectedWorkSchedule.EmployeeCollection != null && SelectedWorkSchedule.EmployeeCollection.IsDirty) ||
                (SelectedWorkSchedule.WorkWeekCollection != null &&
                SelectedWorkSchedule.WorkWeekCollection.Count(x => x.DayOfWorkWeekCollection.Has(y => y.IsDirty)) > 0);
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
                        result = SaveWorkSchedule(SelectedWorkSchedule);
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    if (SelectedWorkSchedule.IsNew)
                    {
                        SelectedWorkSchedule = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else
                    {
                        // Refresh work schedule
                        RefreshWorkSchedule();
                    }
                }
            }

            if (result && isClosing == null && SelectedWorkSchedule != null)
            {
                // Refresh work schedule
                RefreshWorkSchedule();

                // Clear selected item
                SelectedWorkSchedule = null;
            }

            return result;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                WorkScheduleCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                // Get all holidays
                IOrderedEnumerable<tims_WorkSchedule> workSchedules = _workScheduleRepository.GetAll().OrderBy(x => x.DateCreated);
                foreach (tims_WorkSchedule workSchedule in workSchedules)
                {
                    bgWorker.ReportProgress(0, workSchedule);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create work schedule model
                tims_WorkScheduleModel workScheduleModel = new tims_WorkScheduleModel((tims_WorkSchedule)e.UserState);

                // Load relation data
                LoadRelationData(workScheduleModel);

                // Add to collection
                WorkScheduleCollection.Add(workScheduleModel);
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
        /// Load relation data for work schedule
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationData(tims_WorkScheduleModel workScheduleModel)
        {
            // Load employee collection
            LoadEmployeeCollection(workScheduleModel);
        }

        /// <summary>
        /// Refresh work schedule
        /// </summary>
        private void RefreshWorkSchedule()
        {
            SelectedWorkSchedule.WorkWeekCollection = null;
            SelectedWorkSchedule.EmployeeCollection = null;
            SelectedWorkSchedule.ToModelAndRaise();
            SelectedWorkSchedule.EndUpdate();
        }

        /// <summary>
        /// Create a new work schedule with default values
        /// </summary>
        private void NewWorkSchedule()
        {
            SelectedWorkSchedule = new tims_WorkScheduleModel { Rotate = 2 };
            SelectedWorkSchedule.WorkWeekCollection = new ObservableCollection<tims_WorkWeekModel>();
            SelectedWorkSchedule.EmployeeCollection = new CollectionBase<base_GuestModel>();

            // Set work schedule type default is fixed
            SelectedWorkSchedule.WorkScheduleType = (int)ScheduleTypes.Fixed;

            // Set work schedule status is pending
            SelectedWorkSchedule.Status = (int)ScheduleStatuses.Pending;

            // Initial one work week for work schedule
            SelectedWorkSchedule.AddWorkWeek(1);

            // Set selected tab item
            SelectedWorkSchedule.SelectedWorkWeek = SelectedWorkSchedule.WorkWeekCollection.First();

            // Turn off IsDirty
            SelectedWorkSchedule.WorkWeekEndUpdate();
            SelectedWorkSchedule.IsDirty = false;

            FocusDefault = false;
            FocusDefault = true;
        }

        /// <summary>
        /// Load employee to work schedule
        /// </summary>
        private void LoadEmployeeCollection(tims_WorkScheduleModel workScheduleModel)
        {
            // Load employee collection if it's not loaded
            if (workScheduleModel.EmployeeCollection == null)
            {
                // Initial employee collection
                workScheduleModel.EmployeeCollection = new CollectionBase<base_GuestModel>();

                // Get active employees of this work schedule
                int workScheduleID = workScheduleModel.Id;

                // Get employee schedules
                IOrderedEnumerable<base_GuestSchedule> employeeSchedules = _employeeScheduleRepository.
                    GetAll(x => x.base_Guest.IsActived && !x.base_Guest.IsPurged &&
                        !x.Status.Equals((int)EmployeeScheduleStatuses.Inactive) && x.WorkScheduleId == workScheduleID).
                        OrderBy(x => x.base_Guest.FirstName);

                foreach (base_GuestSchedule employeeSchedule in employeeSchedules)
                {
                    // Create new employee model
                    base_GuestModel employeeModel = new base_GuestModel(employeeSchedule.base_Guest);

                    // Get employee schedules that StartDate less than Today
                    IEnumerable<base_GuestSchedule> previousEmployeeSchedules = employeeSchedule.base_Guest.base_GuestSchedule.
                        Where(x => !x.Status.Equals((int)EmployeeScheduleStatuses.Inactive) && x.StartDate <= DateTimeExt.Today).
                        OrderBy(x => x.StartDate);

                    // Get current employee schedule
                    base_GuestSchedule currentSchedule = previousEmployeeSchedules.FirstOrDefault();

                    // Get employee schedule that StartDate greater than Today
                    base_GuestSchedule nextSchedule = employeeSchedule.base_Guest.base_GuestSchedule.
                        Where(x => !x.Status.Equals((int)EmployeeScheduleStatuses.Inactive) && x.StartDate > DateTimeExt.Today).
                        OrderBy(x => x.StartDate).SingleOrDefault();

                    if (nextSchedule == null && previousEmployeeSchedules.Count() == 2)
                    {
                        nextSchedule = previousEmployeeSchedules.LastOrDefault();
                    }

                    if (currentSchedule != null)
                    {
                        // Get current employee schedule model
                        employeeModel.CurrentEmployeeScheduleModel = new base_GuestScheduleModel(currentSchedule);
                    }

                    if (nextSchedule != null)
                    {
                        // Get employee schedule model
                        employeeModel.EmployeeScheduleModel = new base_GuestScheduleModel(nextSchedule);
                    }
                    else if (currentSchedule != null)
                    {
                        // Get employee schedule model
                        employeeModel.EmployeeScheduleModel = new base_GuestScheduleModel(currentSchedule);
                    }

                    if (currentSchedule != null && nextSchedule != null)
                    {
                        if (nextSchedule.WorkScheduleId.Equals(workScheduleID))
                        {
                            // Push new employee schedule to collection
                            workScheduleModel.EmployeeCollection.Add(employeeModel);
                        }
                    }
                    else
                    {
                        // Push new employee schedule to collection
                        workScheduleModel.EmployeeCollection.Add(employeeModel);
                    }

                    // Register property changed event to process check all
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModel_PropertyChanged);
                }

                // Update number of employees
                workScheduleModel.NumberOfEmployees = workScheduleModel.EmployeeCollection.Count;
            }
        }

        /// <summary>
        /// Load work week to work schedule
        /// </summary>
        private void LoadWorkWeekCollection(tims_WorkScheduleModel workScheduleModel)
        {
            // Load work week collection if it's not loaded
            if (workScheduleModel.WorkWeekCollection == null)
            {
                // Initial work week collection
                workScheduleModel.WorkWeekCollection = new ObservableCollection<tims_WorkWeekModel>();

                // Load and group work week by week
                int workScheduleID = workScheduleModel.Id;
                var workWeeks = from x in _workWeekRepository.GetAll(x => x.WorkScheduleId == workScheduleID)
                                group x by x.Week into g
                                select new { WorkScheduleID = workScheduleID, Week = g.Key, DayOfWorkWeeks = g };

                // Create temporary work week for schedule
                for (int i = 0; i < workWeeks.Count(); i++)
                    workScheduleModel.AddWorkWeek(i + 1);

                // Update temporary work week by database
                foreach (var workWeekModel in workScheduleModel.WorkWeekCollection)
                {
                    // Get work week by week
                    var workWeek = workWeeks.Single(x => x.Week == workWeekModel.Week);
                    foreach (var dayOfWorkWeekModel in workWeekModel.DayOfWorkWeekCollection)
                    {
                        // Get work day by day
                        var dayOfWorkWeek = workWeek.DayOfWorkWeeks.FirstOrDefault(x => x.Day == dayOfWorkWeekModel.Day);

                        // Update work day if it's exist
                        if (dayOfWorkWeek != null)
                        {
                            // Update entity for work day
                            dayOfWorkWeekModel.SetEntity(dayOfWorkWeek);

                            // This day have work
                            dayOfWorkWeekModel.HasWork = true;

                            // Map entity to model
                            dayOfWorkWeekModel.ToModel();

                            // Turn off IsNew for work day
                            dayOfWorkWeekModel.IsNew = false;
                        }
                        else
                        {
                            // This day is OFF
                            dayOfWorkWeekModel.HasWork = false;
                        }

                        // Turn off IsDirty for work day
                        dayOfWorkWeekModel.IsDirty = false;
                    }

                    // Turn off IsNew & IsDirty for work week
                    workWeekModel.EndUpdate();
                }

                // Set default selected work week
                workScheduleModel.SelectedWorkWeek = workScheduleModel.WorkWeekCollection.FirstOrDefault();
            }
        }

        /// <summary>
        /// Show work schedule detail to edit or update
        /// </summary>
        /// <param name="workScheduleModel"></param>
        private void EditWorkSchedule(tims_WorkScheduleModel workScheduleModel)
        {
            if (workScheduleModel != null)
            {
                // Load work week for work schedule
                LoadWorkWeekCollection(workScheduleModel);

                // Load employee for work schedule
                LoadEmployeeCollection(workScheduleModel);

                SelectedWorkSchedule = workScheduleModel;
            }
        }

        /// <summary>
        /// Update employee collection after assign work schedule
        /// </summary>
        /// <param name="sourceCollection"></param>
        /// <param name="targetCollection"></param>
        private void UpdateEmployeeCollection(ObservableCollection<base_GuestModel> sourceCollection, ObservableCollection<base_GuestModel> targetCollection)
        {
            // Update item to target collection
            var updateEmployees = from source in sourceCollection
                                  join target in targetCollection.Where(z => z.IsNew)
                                  on source.Id equals target.Id
                                  select new { source, target };
            foreach (var employeeItem in updateEmployees)
            {
                employeeItem.target.EmployeeScheduleModel.StartDate = employeeItem.source.EmployeeScheduleModel.StartDate;
            }

            // Add item to target collection
            var newEmployees = from source in sourceCollection
                               where !(from target in targetCollection.Where(z => z.IsNew)
                                       select target.Id).Contains(source.Id)
                               select source;
            foreach (var employee in newEmployees)
            {
                employee.IsNew = true;
                targetCollection.Add(employee);

                // Register property changed event to process check all
                employee.PropertyChanged += new PropertyChangedEventHandler(employeeModel_PropertyChanged);
            }

            // Remove item from target collection
            var deleteEmployees = from target in targetCollection.Where(z => z.IsNew)
                                  where !(from source in sourceCollection
                                          select source.Id).Contains(target.Id)
                                  select target;
            foreach (var employee in deleteEmployees.ToList())
                targetCollection.Remove(employee);
        }

        /// <summary>
        /// Process when save work schedule
        /// </summary>
        /// <param name="workScheduleModel"></param>
        /// <returns></returns>
        private bool SaveWorkSchedule(tims_WorkScheduleModel workScheduleModel)
        {
            // Update status for work schedule
            int numberOfEmployeeScheduleActive = workScheduleModel.EmployeeCollection.Count(x => x.EmployeeScheduleModel.Status.Is(ScheduleStatuses.Active));
            if (workScheduleModel.EmployeeCollection.Count > 0 &&
                numberOfEmployeeScheduleActive > 0 &&
                workScheduleModel.Status.Is(ScheduleStatuses.Pending))
                workScheduleModel.Status = (int)ScheduleStatuses.Active;

            // Update rotate value if work schedule type is fixed or variable
            if (workScheduleModel.WorkScheduleType != (int)ScheduleTypes.Rotate)
            {
                // Update rotate property in model
                workScheduleModel.Rotate = 1;
            }
            //else if (SelectedWorkSchedule.Rotate == 1)
            //    SelectedWorkSchedule.Rotate = 2;

            if (workScheduleModel.IsNew)
            {
                // Save word schedule when created new
                SaveNew(workScheduleModel);
            }
            else
            {
                // Save word schedule when edited
                SaveUpdate(workScheduleModel);
            }

            // Update ID and turn off IsNew & IsDirty of child
            workScheduleModel.UpdateIDToModel();

            // Update number of employees has assigned
            workScheduleModel.NumberOfEmployees = workScheduleModel.EmployeeCollection.Count;

            // Raise binding properties
            workScheduleModel.RaiseWorkScheduleTypeName();

            // Turn off IsDirty & IsNew
            workScheduleModel.EndUpdate();

            return true;
        }

        /// <summary>
        /// Save when create new word schedule
        /// </summary>
        /// <param name="workScheduleModel"></param>
        private void SaveNew(tims_WorkScheduleModel workScheduleModel)
        {
            workScheduleModel.DateCreated = DateTimeExt.Now;
            workScheduleModel.UserCreated = Define.USER.LoginName;

            // Map data from model to entity
            workScheduleModel.WorkWeekToEntity();
            workScheduleModel.EmployeeScheduleToEntity();

            // Add new work schedule and work week to database
            _workScheduleRepository.Add(workScheduleModel.tims_WorkSchedule);

            // Map data from model to entity
            workScheduleModel.ToEntity();

            // Accept save changes
            _workScheduleRepository.Commit();

            // Add work schedule to collection
            WorkScheduleCollection.Add(workScheduleModel);
        }

        /// <summary>
        /// Save when edit or update word schedule
        /// </summary>
        /// <param name="workScheduleModel"></param>
        private void SaveUpdate(tims_WorkScheduleModel workScheduleModel)
        {
            workScheduleModel.DateUpdated = DateTimeExt.Now;
            workScheduleModel.UserUpdated = Define.USER.LoginName;

            // Save work week and employee schedule
            // Turn off IsNew & IsDirty
            SaveWorkWeekCollection();
            SaveEmployeeCollection();

            // Map data from model to entity
            workScheduleModel.ToEntity();

            // Accept save changes
            _workScheduleRepository.Commit();
        }

        /// <summary>
        /// Update employee schedule for work schedule
        /// </summary>
        private void SaveEmployeeCollection()
        {
            foreach (base_GuestModel employeeItem in SelectedWorkSchedule.EmployeeCollection.DeletedItems)
            {
                // Map value model to entity property
                employeeItem.EmployeeScheduleModel.ToEntity();

                if (employeeItem.EmployeeScheduleModel.Status == (int)EmployeeScheduleStatuses.Pending)
                {
                    // Delete employee schedule from database
                    _employeeScheduleRepository.Delete(employeeItem.EmployeeScheduleModel.base_GuestSchedule);

                    //int currentWorkScheduleID = employeeItem.CurrentEmployeeScheduleModel.WorkScheduleId;

                    //// Get current work schedule model
                    //tims_WorkScheduleModel currentWorkScheduleModel = WorkScheduleCollection.SingleOrDefault(x => x.Id.Equals(currentWorkScheduleID));

                    //// Reload employee collection for current work schedule
                    //currentWorkScheduleModel.EmployeeCollection = null;
                    //LoadEmployeeCollection(currentWorkScheduleModel);
                }
                else
                {
                    // Update status property in model
                    employeeItem.EmployeeScheduleModel.Status = (int)EmployeeScheduleStatuses.Inactive;

                    // Update status property in entity
                    employeeItem.EmployeeScheduleModel.base_GuestSchedule.Status = (int)EmployeeScheduleStatuses.Inactive;
                }
            }
            SelectedWorkSchedule.EmployeeCollection.DeletedItems.Clear();

            foreach (var employeeItem in SelectedWorkSchedule.EmployeeCollection)
            {
                // Map value model to entity property
                employeeItem.EmployeeScheduleModel.ToEntity();

                if (employeeItem.IsNew)
                {
                    long employeeID = employeeItem.Id;
                    base_GuestSchedule employeeSchedule = _employeeScheduleRepository.
                        GetAll(x => x.Status != (int)EmployeeScheduleStatuses.Active && x.GuestId == employeeID).
                        OrderBy(x => x.StartDate).LastOrDefault();
                    if (employeeSchedule != null && employeeSchedule.Status == (int)EmployeeScheduleStatuses.Pending)
                    {
                        // Delete old employee schedule that StartDate > Today
                        _employeeScheduleRepository.Delete(employeeSchedule);
                    }
                    // Add new employee schedule
                    _employeeScheduleRepository.Add(employeeItem.EmployeeScheduleModel.base_GuestSchedule);
                }

                // Turn off IsDirty & IsNew
                employeeItem.EmployeeScheduleModel.EndUpdate();
                employeeItem.EndUpdate();
            }
        }

        /// <summary>
        /// Update work week for work schedule
        /// </summary>
        private void SaveWorkWeekCollection()
        {
            foreach (var workWeekModel in SelectedWorkSchedule.WorkWeekCollection.ToList())
            {
                // Delete all day of work week if work week is deleted
                if (workWeekModel.IsDeleted)
                {
                    // Get days of work week that is deleted
                    var dayOfWorkWeeks = SelectedWorkSchedule.tims_WorkSchedule.tims_WorkWeek.Where(x => x.Week == workWeekModel.Week);

                    // Remove days of work week that is deleted
                    // Use delete all function in repository
                    _workWeekRepository.Delete(dayOfWorkWeeks);

                    // Remove work week model
                    SelectedWorkSchedule.WorkWeekCollection.Remove(workWeekModel);
                }
                else
                {
                    foreach (var dayOfWorkWeekModel in workWeekModel.DayOfWorkWeekCollection.Where(x => x.IsDirty))
                    {
                        dayOfWorkWeekModel.ToEntity();
                        var dayOfWorkWeek = SelectedWorkSchedule.tims_WorkSchedule.tims_WorkWeek.FirstOrDefault(
                            x => x.Week == workWeekModel.Week && x.Day == dayOfWorkWeekModel.Day);
                        if (dayOfWorkWeek != null)
                        {
                            // Delete day of work week
                            if (!dayOfWorkWeekModel.HasWork)
                                _workWeekRepository.Delete(dayOfWorkWeekModel.tims_WorkWeek);
                            else // Update day of work week if exist
                            {
                                // Update entity value
                                //dayOfWorkWeekModel.ToEntityCustom(dayOfWorkWeek);
                                //_workWeekRepository.UpdateWorkWeek(dayOfWorkWeekModel.WorkWeek);
                            }
                        }
                        else if (dayOfWorkWeekModel.HasWork)
                            // Add new day of work week
                            SelectedWorkSchedule.tims_WorkSchedule.tims_WorkWeek.Add(dayOfWorkWeekModel.tims_WorkWeek);
                        //_workWeekRepository.AddWorkWeek(dayOfWorkWeekModel.WorkWeek);
                        dayOfWorkWeekModel.EndUpdate();
                    }
                }
                workWeekModel.EndUpdate();
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Load work schedule data
        /// </summary>
        public override void LoadData()
        {
            if (SelectedWorkSchedule != null && !SelectedWorkSchedule.IsNew)
            {
                // Refresh product datas
                RefreshWorkSchedule();

                // Load work week for work schedule
                LoadWorkWeekCollection(SelectedWorkSchedule);

                // Load employee for work schedule
                LoadEmployeeCollection(SelectedWorkSchedule);
            }

            // Load data by predicate
            LoadDataByPredicate();
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

        private void employeeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!_isCheckingAll)
                    {
                        int numOfItemChecked = SelectedWorkSchedule.EmployeeCollection.Count(x => x.IsChecked);
                        if (numOfItemChecked == 0)
                            _isCheckedAll = false;
                        else if (numOfItemChecked == SelectedWorkSchedule.EmployeeCollection.Count)
                            _isCheckedAll = true;
                        else
                            _isCheckedAll = null;
                        OnPropertyChanged(() => IsCheckedAll);
                    }
                    break;
            }
        }

        #endregion
    }
}