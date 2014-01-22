using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
    class TimeClockManualEventEditingViewModel : ViewModelBase
    {
        #region Define

        private ICollectionView _employeeCollectionView;
        private ICollectionView _timeLogCollectionView;

        private base_GuestRepository _employeeRepository = new base_GuestRepository();
        private tims_TimeLogRepository _timeLogRepository = new tims_TimeLogRepository();

        #endregion

        #region Properties

        #region Search

        private bool isSearchMode = true;
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

        private ObservableCollection<string> _columnCollection;
        /// <summary>
        /// Gets or sets the ColumnCollection.
        /// </summary>
        public ObservableCollection<string> ColumnCollection
        {
            get { return _columnCollection; }
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

        private ObservableCollection<base_GuestModel> _employeeCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the EmployeeCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> EmployeeCollection
        {
            get { return _employeeCollection; }
            set
            {
                if (_employeeCollection != value)
                {
                    _employeeCollection = value;
                    OnPropertyChanged(() => EmployeeCollection);
                }
            }
        }

        private base_GuestModel _selectedEmployee;
        /// <summary>
        /// Gets or sets the SelectedEmployee.
        /// </summary>
        public base_GuestModel SelectedEmployee
        {
            get { return _selectedEmployee; }
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;

                    //if (null != value)
                    //{
                    //    _selectedEmployee.ToModel();
                    //    _selectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(_selectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x, true)));
                    //    FilterTimeLog(_selectedEmployee);
                    //}

                    OnPropertyChanged(() => SelectedEmployee);
                    //OnPropertyChanged(() => ReadOnlyForm);
                }
            }
        }

        private int _totalEmployees;
        /// <summary>
        /// Gets or sets the TotalEmployees.
        /// </summary>
        public int TotalEmployees
        {
            get { return _totalEmployees; }
            set
            {
                if (_totalEmployees != value)
                {
                    _totalEmployees = value;
                    OnPropertyChanged(() => TotalEmployees);
                }
            }
        }

        private DateTime? _fromDate;
        /// <summary>
        /// Gets or sets the FromDate.
        /// <para>For Filter In Grid</para>
        /// </summary>
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged(() => FromDate);
                }
            }
        }

        private DateTime? _toDate;
        /// <summary>
        /// Gets or sets the ToDate. 
        /// <para>For Filter In Grid</para>
        /// </summary>
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;
                    OnPropertyChanged(() => ToDate);
                }
            }
        }

        /// <summary>
        /// Gets the ReadOnlyForm.
        /// Read only set false when Employee Is Active
        /// </summary>
        public bool ReadOnlyForm
        {
            get
            {
                if (SelectedEmployee != null)
                    return !SelectedEmployee.IsActived;
                return false;
            }

        }

        private bool _isShowAll = true;
        /// <summary>
        /// Gets or sets the IsShowAll.
        /// </summary>
        public bool IsShowAll
        {
            get { return _isShowAll; }
            set
            {
                if (_isShowAll != value)
                {
                    _isShowAll = value;
                    OnPropertyChanged(() => IsShowAll);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TimeClockManualEventEditingViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            InitialCommand();

            LoadStaticData();

            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new System.Windows.Threading.DispatcherTimer();
                _waitingTimer.Interval = TimeSpan.FromSeconds(1);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
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
            PopupTimeClockManualAdvanceSearchViewModel viewModel = new PopupTimeClockManualAdvanceSearchViewModel();
            bool? msgResult = _dialogService.ShowDialog<PopupTimeClockManualAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue && msgResult.Value)
            {
                // Load data by search predicate
                LoadDataByPredicate(viewModel.AdvanceSearchPredicate, false, 0);
            }
        }

        #endregion

        #region NewTimeLogCommand

        /// <summary>
        /// Gets the NewEmployeeTimeLogCommand command.
        /// </summary>
        public ICommand NewEmployeeTimeLogCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewEmployeeTimeLogCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewEmployeeTimeLogCommandCanExecute()
        {
            return EmployeeCollection.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the NewEmployeeTimeLogCommand command is executed.
        /// </summary>
        private void OnNewEmployeeTimeLogCommandExecute()
        {
            if (ShowNotification())
            {
                // Show detail form
                IsSearchMode = false;

                _employeeCollectionView.Filter = null;
                PopupAssignHoursViewModel viewModel = new PopupAssignHoursViewModel(EmployeeCollection);
                bool? msgResult = _dialogService.ShowDialog<PopupAssignHoursView>(_ownerViewModel, viewModel, "Create New TimeLog");
                if (msgResult.HasValue && msgResult.Value)
                {
                    // Update selected employee
                    SelectedEmployee = viewModel.SelectedEmployee;

                    // Push new timelog to collection
                    SelectedEmployee.TimeLogCollection.Add(viewModel.SelectedTimeLog);

                    FilterTimeLog(SelectedEmployee);
                }
            }
        }

        #endregion

        #region ViewCommand

        /// <summary>
        /// Gets the ViewCommand command.
        /// </summary>
        public ICommand ViewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ViewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnViewCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the ViewCommand command is executed.
        /// </summary>
        private void OnViewCommandExecute(object param)
        {
            PopupAssignHoursViewModel viewModel = new PopupAssignHoursViewModel(param as tims_TimeLogModel, true);
            bool? msgResult = _dialogService.ShowDialog<PopupAssignHoursView>(_ownerViewModel, viewModel, "Assign Hours");
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
            if (SelectedEmployee == null)
                return false;
            return SelectedEmployee.TimeLogCollection != null &&
                SelectedEmployee.TimeLogCollection.Count(x => !x.ClockOut.HasValue) == 0;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            PopupAssignHoursViewModel viewModel = new PopupAssignHoursViewModel(NewTimeLog());
            bool? msgResult = _dialogService.ShowDialog<PopupAssignHoursView>(_ownerViewModel, viewModel, "Assign Hours");
            if (msgResult.HasValue && msgResult.Value)
            {
                // Push new timelog to collection
                SelectedEmployee.TimeLogCollection.Add(viewModel.SelectedTimeLog);

                FilterTimeLog(SelectedEmployee);
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
            tims_TimeLogModel timeLogModel = dataGridControl.SelectedItem as tims_TimeLogModel;
            PopupAssignHoursViewModel viewModel = new PopupAssignHoursViewModel(timeLogModel);
            bool? msgResult = _dialogService.ShowDialog<PopupAssignHoursView>(_ownerViewModel, viewModel, "Assign Hours");
            if (msgResult.HasValue && msgResult.Value)
            {
                timeLogModel.ClockIn = viewModel.SelectedTimeLog.ClockIn;
                timeLogModel.ClockOut = viewModel.SelectedTimeLog.ClockOut;
                //timeLogModel.IsManual = viewModel.SelectedTimeLog.IsManual;
                timeLogModel.ManualClockOutFlag = viewModel.SelectedTimeLog.ManualClockOutFlag;
                timeLogModel.Reason = viewModel.SelectedTimeLog.Reason;
                timeLogModel.DeductLunchTimeFlag = viewModel.SelectedTimeLog.DeductLunchTimeFlag;
                timeLogModel.OvertimeOptions = viewModel.SelectedTimeLog.OvertimeOptions;
            }
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
            return IsEdit();
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SaveTimeLog(SelectedEmployee);
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
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this item?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                // Convert param to DataGridControl
                DataGridControl dataGridControl = param as DataGridControl;

                // Remove timelog from collection
                SelectedEmployee.TimeLogCollection.Remove(dataGridControl.SelectedItem as tims_TimeLogModel);

                // Decreasement total timelog
                SelectedEmployee.TotalTimeLog--;
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
                // Update selected employee
                SelectedEmployee = param as base_GuestModel;

                // Load timelog collection
                _timeLogRepository.Refresh();
                SelectedEmployee.TimeLogCollection = null;
                LoadTimeLogCollection(SelectedEmployee);

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
                OnFilterEmployeeCollection();

                // Show list form
                IsSearchMode = true;
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
            FromDate = new DateTime().AddYears(DateTimeExt.Today.Year - 1);
            ToDate = FromDate.Value.AddYears(1).AddDays(-1);
        }

        /// <summary>
        /// Initital Command
        /// <remarks>
        /// 10/05/2012 
        ///   Edit Get All Employee no follow with  Staus.Employees is Actived can Edit TimeLog
        /// </remarks>
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            PopupAdvanceSearchCommand = new RelayCommand<object>(OnPopupAdvanceSearchCommandExecute, OnPopupAdvanceSearchCommandCanExecute);
            NewEmployeeTimeLogCommand = new RelayCommand(OnNewEmployeeTimeLogCommandExecute, OnNewEmployeeTimeLogCommandCanExecute);
            ViewCommand = new RelayCommand<object>(OnViewCommandExecute, OnViewCommandCanExecute);
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
        }

        /// <summary>
        /// Check has edit on form
        /// </summary>
        /// <returns></returns>
        private bool IsEdit()
        {
            if (SelectedEmployee == null)
                return false;

            return (SelectedEmployee.TimeLogCollection != null && SelectedEmployee.TimeLogCollection.IsDirty);
        }

        /// <summary>
        /// Notify when exit or change form
        /// </summary>
        /// <returns>True is continue action</returns>
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
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        result = SaveTimeLog(SelectedEmployee);
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    if (SelectedEmployee.IsNew)
                    {
                        SelectedEmployee = null;
                        if (isClosing.HasValue && !isClosing.Value)
                        {
                            IsSearchMode = true;
                        }
                    }
                    else
                    {
                        // Rollback employee
                        SelectedEmployee.TimeLogCollection = null;

                        // Load timelog collection
                        LoadTimeLogCollection(SelectedEmployee);
                    }
                }
            }

            // Clear selected item
            if (result && isClosing == null && SelectedEmployee != null)
                SelectedEmployee = null;

            return result;
        }

        /// <summary>
        /// Notify when exit or change form
        /// </summary>
        /// <returns>True is continue action</returns>
        private bool ShowNotification()
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
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        result = SaveTimeLog(SelectedEmployee);
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Guest, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<base_Guest>();

                // Get all products that GuestNo contain keyword
                predicate = predicate.Or(x => x.GuestNo.ToLower().Contains(keyword));

                // Get all products that FirstName contain keyword
                predicate = predicate.Or(x => x.FirstName.ToLower().Contains(keyword));

                // Get all products that LastName contain keyword
                predicate = predicate.Or(x => x.LastName.ToLower().Contains(keyword));

                // Get all products that ActualHours contain keyword
            }

            string employeeMark = MarkType.Employee.ToDescription();

            // Default condition
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(employeeMark) && x.IsTrackingHour);

            return predicate;
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Guest, bool>> predicate = CreateSearchPredicate(_keyword);

            // Load data by predicate
            LoadDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="predicate">Condition for load data</param>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(Expression<Func<base_Guest, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                EmployeeCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                if (refreshData)
                {
                }

                // Get total employee with condition in predicate
                TotalEmployees = _employeeRepository.GetIQueryable(predicate).Count();

                // Get data with range
                //IList<base_Guest> employees = _employeeRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.DateCreated, predicate);
                IList<base_Guest> employees = _employeeRepository.GetAll(predicate);
                foreach (base_Guest employee in employees)
                {
                    bgWorker.ReportProgress(0, employee);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create product model
                base_GuestModel employeeModel = new base_GuestModel((base_Guest)e.UserState);

                // Load relation data
                LoadRelationData(employeeModel);

                // Add to collection
                EmployeeCollection.Add(employeeModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Initial employee collection view to filter
                _employeeCollectionView = CollectionViewSource.GetDefaultView(EmployeeCollection);

                OnFilterEmployeeCollection();

                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationData(base_GuestModel employeeModel)
        {
            // Get employee schedule
            GetEmployeeSchedule(employeeModel);

            // Load timelog collection
            LoadTimeLogCollection(employeeModel);
        }

        /// <summary>
        /// Get current schedule of employee
        /// </summary>
        /// <param name="employeeModel"></param>
        private void GetEmployeeSchedule(base_GuestModel employeeModel)
        {
            // Get employee schedules that StartDate less than Today
            IEnumerable<base_GuestSchedule> previousEmployeeSchedules = employeeModel.base_Guest.base_GuestSchedule.
                Where(x => !x.Status.Equals((int)EmployeeScheduleStatuses.Inactive) && x.StartDate <= DateTimeExt.Today).
                OrderBy(x => x.StartDate);

            // If employee have two work schedule,
            // the first one is previous schedule
            // the second one is current schedule
            if (previousEmployeeSchedules.Count() == 2)
            {
                // Get previous employee schedule
                base_GuestSchedule previousSchedule = previousEmployeeSchedules.FirstOrDefault();

                if (previousSchedule != null)
                {
                    // Get previous employee schedule model
                    employeeModel.PreviousEmployeeScheduleModel = new base_GuestScheduleModel(previousSchedule);
                }
            }

            // Get current employee schedule
            base_GuestSchedule currentSchedule = previousEmployeeSchedules.LastOrDefault();

            if (currentSchedule != null)
            {
                // Get current employee schedule model
                employeeModel.CurrentEmployeeScheduleModel = new base_GuestScheduleModel(currentSchedule);
            }

        }

        /// <summary>
        /// Load timelog collection
        /// </summary>
        /// <param name="employeeModel"></param>
        private void LoadTimeLogCollection(base_GuestModel employeeModel)
        {
            if (employeeModel.TimeLogCollection == null)
            {
                // Initial timelog collection
                employeeModel.TimeLogCollection = new CollectionBase<tims_TimeLogModel>();

                // Get all timelogs
                IOrderedEnumerable<tims_TimeLog> timeLogs = employeeModel.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut);
                foreach (tims_TimeLog timeLog in timeLogs.Where(x => x.tims_WorkSchedule != null))
                {
                    tims_TimeLogModel timeLogModel = new tims_TimeLogModel(timeLog);
                    timeLogModel.IsManual = timeLogModel.ManualClockInFlag || timeLogModel.ManualClockOutFlag;
                    timeLogModel.ClockInDate = timeLog.ClockIn.Date;
                    timeLogModel.EmployeeSchedule = timeLog.base_Guest.base_GuestSchedule.
                        Where(x => x.StartDate <= timeLogModel.ClockIn).
                        OrderBy(y => y.StartDate).
                        LastOrDefault();
                    string workScheduleName = timeLog.tims_WorkSchedule.WorkScheduleName;
                    string startDate = timeLogModel.EmployeeSchedule.StartDate.ToString(Define.DateFormat);
                    timeLogModel.WorkScheduleGroup = string.Format("{0} - {1}", workScheduleName, startDate);
                    employeeModel.TimeLogCollection.Add(timeLogModel);

                    // Turn off IsDirty & IsNew
                    timeLogModel.EndUpdate();
                }

                FilterTimeLog(employeeModel);
            }
        }

        /// <summary>
        /// Filter employee collection by sum of timelogs
        /// </summary>
        private void OnFilterEmployeeCollection()
        {
            _employeeCollectionView.Filter = item =>
            {
                return (item as base_GuestModel).base_Guest.tims_TimeLog.Count > 0;
            };
        }

        /// <summary>
        /// Create new timelog
        /// </summary>
        /// <returns></returns>
        private tims_TimeLogModel NewTimeLog()
        {
            tims_TimeLogModel timeLogModel = new tims_TimeLogModel();
            if (SelectedEmployee.CurrentEmployeeScheduleModel != null)
            {
                timeLogModel.EmployeeSchedule = SelectedEmployee.CurrentEmployeeScheduleModel.base_GuestSchedule;
                timeLogModel.WorkScheduleId = timeLogModel.EmployeeSchedule.WorkScheduleId;
                string workScheduleName = SelectedEmployee.CurrentEmployeeScheduleModel.base_GuestSchedule.tims_WorkSchedule.WorkScheduleName;
                string startDate = timeLogModel.EmployeeSchedule.StartDate.ToString(Define.DateFormat);
                timeLogModel.WorkScheduleGroup = string.Format("{0} - {1}", workScheduleName, startDate);
            }
            timeLogModel.ClockInDate = DateTimeExt.Today;
            timeLogModel.ClockIn = DateTimeExt.Today.AddHours(9);
            timeLogModel.ClockOut = null;
            timeLogModel.IsManual = true;
            timeLogModel.ManualClockInFlag = true;
            timeLogModel.ActiveFlag = true;
            timeLogModel.GuestResource = SelectedEmployee.Resource.ToString();
            timeLogModel.EmployeeId = SelectedEmployee.Id;
            timeLogModel.EmployeeTemp = this.SelectedEmployee;
            timeLogModel.SetDuplicationTimeLog();
            timeLogModel.SetFixItemTimeClockNull();
            return timeLogModel;
        }

        /// <summary>
        /// Save timelog
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <returns></returns>
        private bool SaveTimeLog(base_GuestModel employeeModel)
        {
            if (employeeModel.TimeLogCollection.DeletedItems != null)
            {
                foreach (tims_TimeLogModel timeLogItem in employeeModel.TimeLogCollection.DeletedItems)
                {
                    // Remove timelog from database
                    _timeLogRepository.Delete(timeLogItem.tims_TimeLog);
                }

                // Clear deleted timelogs from collection
                employeeModel.TimeLogCollection.DeletedItems.Clear();
            }

            foreach (tims_TimeLogModel timeLogItem in employeeModel.TimeLogCollection.Where(x => x.IsDirty))
            {
                if (!timeLogItem.IsNew)
                {
                    timeLogItem.DateUpdated = DateTimeExt.Now;
                    timeLogItem.UserUpdated = Define.USER.LoginName;
                }

                // Map data from model to entity
                timeLogItem.ToEntity();

                if (timeLogItem.IsNew)
                {
                    // Add new timelog to database
                    employeeModel.base_Guest.tims_TimeLog.Add(timeLogItem.tims_TimeLog);
                }

                if (!employeeModel.CurrentEmployeeScheduleModel.Status.Equals((int)EmployeeScheduleStatuses.Active))
                {
                    // Update employee schedule status to Active
                    employeeModel.CurrentEmployeeScheduleModel.Status = (int)EmployeeScheduleStatuses.Active;
                    employeeModel.CurrentEmployeeScheduleModel.base_GuestSchedule.Status = (int)EmployeeScheduleStatuses.Active;
                }

                if (employeeModel.PreviousEmployeeScheduleModel != null &&
                    !employeeModel.PreviousEmployeeScheduleModel.Status.Equals((int)EmployeeScheduleStatuses.Inactive))
                {
                    // Update current employee schedule status to Inactive
                    employeeModel.PreviousEmployeeScheduleModel.Status = (int)EmployeeScheduleStatuses.Inactive;
                    employeeModel.PreviousEmployeeScheduleModel.base_GuestSchedule.Status = (int)EmployeeScheduleStatuses.Inactive;
                }
            }



            // Accept changes
            _timeLogRepository.Commit();

            foreach (tims_TimeLogModel timeLogItem in employeeModel.TimeLogCollection.Where(x => x.IsDirty))
            {
                // Turn off IsDirty & IsNew
                timeLogItem.EndUpdate();
            }

            FilterTimeLog(employeeModel);

            return true;
        }

        /// <summary>
        /// Filter TimeLog with current item
        /// </summary>
        private void FilterTimeLog(base_GuestModel employeeModel)
        {
            try
            {
                ICollectionView timeLogCollectionView = CollectionViewSource.GetDefaultView(employeeModel.TimeLogCollection);

                // Group timelog by work schedule
                if (timeLogCollectionView.GroupDescriptions.Count == 0)
                    timeLogCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("WorkScheduleGroup"));

                if (timeLogCollectionView != null)
                {
                    timeLogCollectionView.Filter = (x) =>
                    {
                        bool result = true;

                        if (FromDate.HasValue || ToDate.HasValue)
                        {
                            tims_TimeLogModel timeLogModel = x as tims_TimeLogModel;

                            if (FromDate.HasValue && ToDate.HasValue)
                            {
                                result = FromDate <= timeLogModel.ClockIn && timeLogModel.ClockIn < ToDate.Value.AddDays(1);
                            }
                            else if (FromDate.HasValue)
                            {
                                result = FromDate <= timeLogModel.ClockIn;
                            }
                            else if (ToDate.HasValue)
                            {
                                result = timeLogModel.ClockIn < ToDate.Value.AddDays(1);
                            }
                        }
                        return result;

                    };

                    employeeModel.TotalTimeLog = timeLogCollectionView.OfType<tims_TimeLogModel>().Count();
                    employeeModel.IsAvailableTimeLog = employeeModel.TotalTimeLog > 0;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save timeLog
        /// </summary>
        private void SaveTimeLog()
        {
            try
            {
                foreach (var timeLogModel in this.SelectedEmployee.TimeLogCollection.ToList())
                {
                    if (timeLogModel.IsDirty)
                    {
                        //Set Employee if is a new item
                        if (timeLogModel.IsNew)
                        {
                            timeLogModel.tims_TimeLog.base_Guest = SelectedEmployee.base_Guest;
                        }
                        ///Get all TimeLog relation to calculate OT , LeaveEarly
                        var relationDirtyItem = this.SelectedEmployee.TimeLogCollection.Where(x => x.ClockIn.Date == timeLogModel.ClockIn.Date);
                        foreach (var relationWithDirty in relationDirtyItem)
                        {

                            //Check is Day Off Or Holiday
                            if (!this.IsDayOffOrHoliday(relationWithDirty))
                            {
                                // Set For workTime
                                relationWithDirty.WorkTime = (float)relationWithDirty.GetWorkTime().TotalMinutes;

                                //Set For LunchTime 
                                relationWithDirty.LunchTime = (float)relationWithDirty.GetLunchTime().TotalMinutes;

                                ///
                                ///Set For Early Time & LeaveEarlyTime
                                ///
                                //Get TimeClock Min & TimeClock Max in TimeLogCollection Compare with current item iterated
                                var isTimeLogMin = this.SelectedEmployee.TimeLogCollection.Where(x => x.ClockIn.Date == relationWithDirty.ClockIn.Date).Aggregate((cur, next) => cur.ClockIn < next.ClockIn ? cur : next);
                                var isTimeLogMax = this.SelectedEmployee.TimeLogCollection.Where(x => x.ClockIn.Date == relationWithDirty.ClockIn.Date).Aggregate((cur, next) => cur.ClockOut > next.ClockOut ? cur : next);
                                //if current item is not TimeClock Min & Time Clock Max => set Late & Early =0
                                if (isTimeLogMin != relationWithDirty && relationWithDirty != isTimeLogMax)
                                {
                                    relationWithDirty.LateTime = 0;
                                    relationWithDirty.LeaveEarlyTime = 0;
                                }
                                else
                                {
                                    if (isTimeLogMin != null && isTimeLogMin == relationWithDirty)// Current item is a TimeClock Min
                                        UpdateLateTime(relationWithDirty);
                                    if (isTimeLogMax != null && isTimeLogMax == relationWithDirty)// Current item is a TimeClock Max
                                        UpdateLeaveEarlyTime(relationWithDirty);

                                    if (isTimeLogMin != isTimeLogMax && isTimeLogMin == relationWithDirty)//current item is a Min TimeClock & TimeClock min & max is not one =>  Set LeaveEarly Time = 0
                                        relationWithDirty.LeaveEarlyTime = 0;
                                    else if (isTimeLogMin != isTimeLogMax && isTimeLogMax == relationWithDirty)// Current item  is a max TimeClock & Min & max is not one = > Set LateTime is 0
                                        relationWithDirty.LateTime = 0;
                                }
                            }
                            else
                            {
                                relationWithDirty.WorkTime = 0;
                                relationWithDirty.LunchTime = 0;
                                relationWithDirty.LateTime = 0;
                                relationWithDirty.LeaveEarlyTime = 0;
                            }

                            //Calculate For Overtime
                            CalcOverTime(relationWithDirty, this.SelectedEmployee);
                            relationWithDirty.SetWorkPermissionOT();

                            ///SET FOR EMPLOYEE SCHEDULE & WORK SCHEDULE
                            /// Set Previous EmployeeSchedule To InActive if user using new WorkSchedule
                            ///
                            if (relationWithDirty.PreviousEmployeeSchedule != null)
                                relationWithDirty.PreviousEmployeeSchedule.Status = (int)EmployeeScheduleStatuses.Inactive;

                            if (relationWithDirty.EmployeeSchedule != null)
                            {
                                //Set Employee Schedule To Active
                                if (relationWithDirty.EmployeeSchedule.Status == (int)EmployeeScheduleStatuses.Pending)
                                    relationWithDirty.EmployeeSchedule.Status = (int)EmployeeScheduleStatuses.Active;

                                //Set WorkSchedule To Active
                                if (relationWithDirty.EmployeeSchedule.tims_WorkSchedule != null && relationWithDirty.EmployeeSchedule.tims_WorkSchedule.Status == (int)ScheduleStatuses.Pending)
                                    relationWithDirty.EmployeeSchedule.tims_WorkSchedule.Status = (int)ScheduleStatuses.Active;
                            }

                            //Trim Reason
                            if (!string.IsNullOrWhiteSpace(relationWithDirty.Reason))
                                relationWithDirty.Reason = relationWithDirty.Reason.Trim();

                            //Extend Info
                            //[FIX]relationWithDirty.ModifiedById = Define.UserLoginID;
                            relationWithDirty.DateUpdated = DateTimeExt.Now;
                            relationWithDirty.ActiveFlag = true;

                            relationWithDirty.ToEntity();
                            if (relationWithDirty.IsNew)
                                this.SelectedEmployee.base_Guest.tims_TimeLog.Add(relationWithDirty.tims_TimeLog);
                            relationWithDirty.EndUpdate();
                        }
                    }
                }

                ////Set Employee Schedule To Pending  When TimeLog of EmployeeSchedule Null
                foreach (var item in SelectedEmployee.base_Guest.base_GuestSchedule.Where(x => x.Status.Is(EmployeeScheduleStatuses.Active)))
                {
                    if (item.tims_WorkSchedule.tims_TimeLog.Count(x => x.EmployeeId == SelectedEmployee.base_Guest.Id && x.ClockIn >= item.StartDate) == 0)
                    {
                        item.Status = (int)EmployeeScheduleStatuses.Pending;
                    }
                }

                this.SelectedEmployee.ToEntity();
                _employeeRepository.Commit();
                SelectedEmployee.EndUpdate();

                //set again
                SelectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(SelectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x)
                {
                    ClockInDate = new DateTime(x.ClockIn.Year, x.ClockIn.Month, x.ClockIn.Day),
                    WorkScheduleGroup = x.tims_WorkSchedule.WorkScheduleName + " - " + x.base_Guest.base_GuestSchedule.Where(y => y.StartDate <= x.ClockIn.Date).OrderBy(y => y.StartDate).ThenBy(y => y.AssignDate).Last().StartDate.ToString("dd/MM/yyyy"),
                    IsManual = true,
                    IsDirty = false
                }));

                if (this.FromDate.HasValue && this.ToDate.HasValue)
                    SelectedEmployee.IsAvailableTimeLog = SelectedEmployee.base_Guest.tims_TimeLog.Count(x => x.ClockIn.Date >= this.FromDate.Value.Date && x.ClockIn.Date <= this.ToDate.Value.Date) > 0;
                else
                    SelectedEmployee.IsAvailableTimeLog = SelectedEmployee.base_Guest.tims_TimeLog.Count() > 0;

                //Set toggle button IsChecked = False to collapsed RowDetail when Collecion Empty
                if (!SelectedEmployee.IsAvailableTimeLog)
                    SelectedEmployee.IsChecked = false;
                SelectedEmployee.RaiseOnChangedActualHours();
                _timeLogCollectionView = CollectionViewSource.GetDefaultView(SelectedEmployee.TimeLogCollection);
                _timeLogCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("WorkScheduleGroup"));

                FilterTimeLog(SelectedEmployee);
                _log4net.Info("Save Workpermssion Success");
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Filter All Employee & TimeLog with FromDate & ToDate changed
        /// </summary>
        private void FilterAllTimeLog()
        {
            try
            {
                if (this.EmployeeCollection != null)
                {
                    foreach (var employeeModel in this.EmployeeCollection)
                    {
                        if (this.FromDate.HasValue && this.ToDate.HasValue)
                            employeeModel.IsAvailableTimeLog = employeeModel.base_Guest.tims_TimeLog.Count(x => x.ClockIn.Date >= this.FromDate.Value.Date && x.ClockIn.Date <= this.ToDate.Value.Date) > 0;
                        else
                            employeeModel.IsAvailableTimeLog = employeeModel.base_Guest.tims_TimeLog.Count() > 0;

                        //Set toggle button IsChecked = False to collapsed RowDetail when Collecion Empty
                        if (!employeeModel.IsAvailableTimeLog)
                        {
                            employeeModel.IsChecked = false;
                        }

                        if (employeeModel.TimeLogCollection != null)
                        {
                            ICollectionView timeLogCollectionView = CollectionViewSource.GetDefaultView(employeeModel.TimeLogCollection);
                            if (timeLogCollectionView != null)
                            {

                                //Avoid error "'DeferRefresh' is not allowed during an AddNew or EditItem transaction."
                                IEditableCollectionView collection = timeLogCollectionView as IEditableCollectionView;
                                if (collection.IsEditingItem)
                                    collection.CommitEdit();

                                timeLogCollectionView.Filter = (one) =>
                                {
                                    bool result = true;
                                    if (!this.FromDate.HasValue && !this.ToDate.HasValue)
                                        result &= true;

                                    var timeLogModel = one as tims_TimeLogModel;
                                    if (timeLogModel == null)
                                        result &= false;
                                    else
                                    {
                                        if (!timeLogModel.IsNew && this.FromDate.HasValue)
                                            result &= (timeLogModel.ClockIn.Date >= this.FromDate.Value.Date);
                                        if (!timeLogModel.IsNew && this.ToDate.HasValue)
                                            result &= (timeLogModel.ClockIn.Date <= this.ToDate.Value.Date);
                                    }

                                    return result;
                                };

                                employeeModel.TotalTimeLog = timeLogCollectionView.OfType<tims_TimeLogModel>().Count();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Method for Old Item if user 
        /// No Error :(user want to save?)
        ///          Yes : save this itme
        ///          No : rollback data 
        ///          Cancel : no action for user change
        /// Has Error: (User want to roll back ?) 
        ///          Yes :  Back Data
        ///          No : No action
        /// </summary>
        /// <param name="isSearchModel">
        /// isSearchModel : True => after handle change to GridSearch
        ///                 False => Change to Form
        ///                 
        /// </param>
        /// <returns>
        /// False to stop ; True continue
        /// </returns>
        /// <remarks>
        /// Required to check this is a old item !IsSearchMode && SelectedEmployee.TimeLogCollection.Count(x => x.IsNew) > 0
        /// </remarks>
        private bool OldItemCheck()
        {
            try
            {
                if (IsValid) // No Error : YES : Save; NO : RollBack, CANCEL : No Action
                {
                    var result = ShowMessageBox("Do you want to save this item ?", "TIMS - Time Clock Correction", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        SaveTimeLog();
                        IsSearchMode = true;
                        return true;
                    }
                    else if (MessageBoxResult.No.Equals(result))
                    {
                        ///Check Here Can need assign to model 
                        this.SelectedEmployee.ToModel();
                        SelectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(SelectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x)
                        {
                            ClockInDate = new DateTime(x.ClockIn.Year, x.ClockIn.Month, x.ClockIn.Day),
                            WorkScheduleGroup = x.tims_WorkSchedule.WorkScheduleName + " - " + x.base_Guest.base_GuestSchedule.Where(y => y.StartDate <= x.ClockIn.Date).OrderBy(y => y.StartDate).ThenBy(y => y.AssignDate).Last().StartDate.ToString("dd/MM/yyyy"),
                            IsManual = true,
                            IsDirty = false
                        }));
                        FilterTimeLog(SelectedEmployee);
                        IsSearchMode = true;
                        return true;
                    }
                    //Cancel here
                    return false;
                }
                else // Has Error : YES : roll Back Data; NO : No action
                {
                    var result = ShowMessageBox("Do you want to close this form?", "TIMS - Time Clock Correction", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        this.SelectedEmployee.ToModel();

                        SelectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(SelectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x)
                        {
                            ClockInDate = new DateTime(x.ClockIn.Year, x.ClockIn.Month, x.ClockIn.Day),
                            WorkScheduleGroup = x.tims_WorkSchedule.WorkScheduleName + " - " + x.tims_WorkSchedule.base_GuestSchedule.Last(y => y.WorkScheduleId == x.WorkScheduleId && y.GuestId == x.EmployeeId && y.StartDate <= x.ClockIn).StartDate.ToString("dd/MM/yyyy"),
                            IsManual = true,
                            IsDirty = false
                        }));
                        FilterTimeLog(SelectedEmployee);
                        IsSearchMode = true;
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
            return true;
        }

        /// <summary>
        /// Method for check new item if Item has changed & not error save this item
        /// No Error : (user want to save item?)
        ///         Yes : save this item
        ///         No : Delete this item
        ///         Cancel : No action
        /// Has Error : (user delete item?)
        ///         Yes : Delete item
        ///         No : No action
        ///<returns>
        /// False to stop ; True continue
        ///</returns>
        ///<remarks>
        ///Required to  check !IsSearchMode && (SelectedEmployee.TimeLogCollection.Count(x => x.IsDirty) > 0 || (SelectedEmployee.TimeLogCollection.DeletedItems != null && SelectedEmployee.TimeLogCollection.DeletedItems.Count > 0))
        ///
        /// </remarks>
        /// </summary>
        private bool NewItemCheck()
        {
            try
            {
                if (IsValid)// No Error : YES : Save; NO : Delete, CANCEL : No Action
                {
                    var result = ShowMessageBox("Do you want to save this item ?", "TIMS - Time Clock Correction", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        SaveTimeLog();
                        IsSearchMode = true;
                        return true;
                    }
                    else if (MessageBoxResult.No.Equals(result))
                    {
                        this.SelectedEmployee.ToModel();
                        this.SelectedEmployee.TimeLogCollection.Clear();

                        SelectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(SelectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x)
                        {
                            ClockInDate = new DateTime(x.ClockIn.Year, x.ClockIn.Month, x.ClockIn.Day),
                            WorkScheduleGroup = x.tims_WorkSchedule.WorkScheduleName + " - " + x.tims_WorkSchedule.base_GuestSchedule.Last(y => y.WorkScheduleId == x.WorkScheduleId && y.GuestId == x.EmployeeId && y.StartDate <= x.ClockIn).StartDate.ToString("dd/MM/yyyy"),
                            IsManual = true,
                            IsDirty = false
                        }));
                        FilterTimeLog(SelectedEmployee);
                        IsSearchMode = true;
                        return true;
                    }
                    //Cancel
                    return false;

                }
                else //Has Error => Delete
                {
                    var result = ShowMessageBox("Do you want to close this form?", "TIMS - Time Clock Correction", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        this.SelectedEmployee.ToModel();
                        this.SelectedEmployee.TimeLogCollection.Clear();
                        SelectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(SelectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x)
                        {
                            ClockInDate = new DateTime(x.ClockIn.Year, x.ClockIn.Month, x.ClockIn.Day),
                            WorkScheduleGroup = x.tims_WorkSchedule.WorkScheduleName + " - " + x.tims_WorkSchedule.base_GuestSchedule.Last(y => y.WorkScheduleId == x.WorkScheduleId && y.GuestId == x.EmployeeId && y.StartDate <= x.ClockIn).StartDate.ToString("dd/MM/yyyy"),
                            IsManual = true,
                            IsDirty = false
                        }));

                        FilterTimeLog(SelectedEmployee);
                        this.IsSearchMode = true;
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
            return true;
        }

        /// <summary>
        /// Add TimeLog WorkPermision If not existed
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="workPermissions"></param>
        private void AddTimeLogPermission(tims_TimeLogModel timeLogModel, tims_WorkPermission workPermission)
        {
            if ((workPermission != null && timeLogModel.ClockOut.HasValue && !timeLogModel.tims_TimeLog.tims_WorkPermission.Any(x => x == workPermission)))
            {
                timeLogModel.tims_TimeLog.tims_WorkPermission.Add(workPermission);
            }
        }

        /// <summary>
        /// Calculate & update leaveEarly if ClockOut less than WorkIN
        /// </summary>
        /// <param name="timeLogModel"></param>
        private void UpdateLeaveEarlyTime(tims_TimeLogModel timeLogModel)
        {
            if (timeLogModel.WorkWeek != null && timeLogModel.ClockOut.HasValue && timeLogModel.ClockOut < timeLogModel.WorkWeek.WorkOut)
                timeLogModel.LeaveEarlyTime = (float)timeLogModel.WorkWeek.WorkOut.Subtract(timeLogModel.ClockOut.Value).TotalMinutes;
            else
                timeLogModel.LeaveEarlyTime = 0;
        }

        /// <summary>
        /// Calculate & update for late time if clock in greater than latetime
        /// </summary>
        /// <param name="timeLogModel"></param>
        private void UpdateLateTime(tims_TimeLogModel timeLogModel)
        {
            if (timeLogModel.WorkWeek != null && timeLogModel.ClockIn > timeLogModel.WorkWeek.WorkIn)
                timeLogModel.LateTime = (float)timeLogModel.ClockIn.Subtract(timeLogModel.WorkWeek.WorkIn).TotalMinutes;
            else
                timeLogModel.LateTime = 0;
        }

        /// <summary>
        /// Get valid overtime value if have work permission
        /// </summary>
        /// <param name="overtimeValue"></param>
        /// <param name="workPermissionDictionary"></param>
        /// <param name="timeLogModel"></param>
        /// <param name="employeeModel"></param>
        /// <param name="workPermissionCollection"></param>
        /// <param name="overtime"></param>
        /// <remarks>
        /// Created by : LongHome
        /// </remarks>
        /// <returns></returns>
        private float GetOvertimeValid(float overtimeValue, IDictionary<Overtime, float> sumOvertimeDictionary,
            tims_TimeLogModel timeLogModel, base_GuestModel employeeModel,
            IEnumerable<tims_WorkPermission> workPermissionCollection, Overtime overtime)
        {
            try
            {
                // Declare overtime valid variable
                float overtimeValid = 0;

                // Declare overtime elapsed variable
                float overtimeElapsed = sumOvertimeDictionary[overtime];

                // Get work permission have over time option equal param
                tims_WorkPermission workPermission = workPermissionCollection.FirstOrDefault(x => overtime.In(x.OvertimeOptions));

                // Check work permission
                if (workPermission != null)
                {
                    // Get hour per day in work permission
                    float hourPerDay = (float)TimeSpan.FromHours(workPermission.HourPerDay).TotalMinutes;

                    // Declare overtime remaining variable
                    float overtimeRemaining = 0;
                    switch (overtime)
                    {
                        case Overtime.Holiday:
                            if (overtimeElapsed == 0)
                                overtimeValid = Math.Min(hourPerDay, overtimeValue);
                            else
                            {
                                overtimeRemaining = GetOvertimeRemaining(hourPerDay, overtimeElapsed);
                                overtimeValid = Math.Min(overtimeRemaining, overtimeValue);
                            }
                            break;
                        case Overtime.Before:
                            if (overtimeElapsed == 0)
                                overtimeValid = Math.Min(hourPerDay, overtimeValue);
                            else
                            {
                                overtimeRemaining = GetOvertimeRemaining(hourPerDay, overtimeElapsed);
                                overtimeValid = Math.Min(overtimeRemaining, overtimeValue);
                            }
                            break;
                        case Overtime.Break:
                            if (overtimeElapsed + sumOvertimeDictionary[Overtime.Before] + timeLogModel.OvertimeBefore == 0)
                                overtimeValid = Math.Min(hourPerDay, overtimeValue);
                            else
                            {
                                if (Overtime.Before.In(workPermission.OvertimeOptions))
                                    overtimeElapsed += sumOvertimeDictionary[Overtime.Before] + timeLogModel.OvertimeBefore;
                                //overtimeRemaining = GetOvertimeRemaining(hourPerDay, sumOvertimeDictionary[Overtime.Before]);
                                overtimeRemaining = GetOvertimeRemaining(hourPerDay, overtimeElapsed);
                                overtimeValid = Math.Min(overtimeRemaining, overtimeValue);
                            }
                            break;
                        case Overtime.After:
                            if (overtimeElapsed + sumOvertimeDictionary[Overtime.Before] + sumOvertimeDictionary[Overtime.Break] + timeLogModel.OvertimeBefore + timeLogModel.OvertimeLunch == 0)
                                overtimeValid = Math.Min(hourPerDay, overtimeValue);
                            else
                            {
                                if (Overtime.Before.In(workPermission.OvertimeOptions))
                                    overtimeElapsed += sumOvertimeDictionary[Overtime.Before] + timeLogModel.OvertimeBefore;
                                //overtimeRemaining = GetOvertimeRemaining(hourPerDay, sumOvertimeDictionary[Overtime.Before]);
                                if (Overtime.Break.In(workPermission.OvertimeOptions))
                                    overtimeElapsed += sumOvertimeDictionary[Overtime.Break] + timeLogModel.OvertimeLunch;
                                //overtimeRemaining = GetOvertimeRemaining(hourPerDay, sumOvertimeDictionary[Overtime.Before] + sumOvertimeDictionary[Overtime.Break]);
                                overtimeRemaining = GetOvertimeRemaining(hourPerDay, overtimeElapsed);
                                overtimeValid = Math.Min(overtimeRemaining, overtimeValue);
                            }
                            break;
                    }

                    //// Update timelog permission table
                    timeLogModel.OvertimeOptions = (int)((Overtime)timeLogModel.OvertimeOptions).Add(overtime);
                }
                else
                {
                    //Not Workpermission with has Checked OTOption(From Employee config or User Checked)
                    if (timeLogModel.OvertimeOptions.Has(overtime))
                    {
                        overtimeValid = overtimeValue;
                    }
                }
                return overtimeValid;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        private void CalcOverTime(tims_TimeLogModel timeLogModel, base_GuestModel employeeModel)
        {
            try
            {
                // Get work permission collecion
                var workPermissionCollection = employeeModel.base_Guest.tims_WorkPermission.Where(x => timeLogModel.ClockIn.Date >= x.FromDate.Date && timeLogModel.ClockIn.Date <= x.ToDate);

                //Add Permission Type With Arrivinglate & LeavingEarly
                this.AddTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => WorkPermissionTypes.ArrivingLate.In(x.PermissionType)));
                this.AddTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => WorkPermissionTypes.LeavingEarly.In(x.PermissionType)));

                // Get previous timelogs to calc overtime elapsed
                var timeLogGroup = (from x in employeeModel.TimeLogCollection.Where(x => x.ClockOut.HasValue &&
                    x.EmployeeId == timeLogModel.EmployeeId && x.WorkScheduleId == timeLogModel.WorkScheduleId)
                                    where x != timeLogModel && timeLogModel.ClockIn.Date == x.ClockIn.Date && timeLogModel.ClockIn > x.ClockIn
                                    group x by new { x.EmployeeId, x.WorkScheduleId } into g
                                    select new
                                    {
                                        SumOvertimeBefore = g.Sum(x => x.OvertimeBefore),
                                        SumOvertimeLunch = g.Sum(x => x.OvertimeLunch),
                                        SumOvertimeAfter = g.Sum(x => x.OvertimeAfter),
                                        SumOvertimeDayOff = g.Sum(x => x.OvertimeDayOff)
                                    }).FirstOrDefault();

                IDictionary<Overtime, float> sumOvertimeDictionary = new Dictionary<Overtime, float>();
                sumOvertimeDictionary.Add(Overtime.Before, 0);
                sumOvertimeDictionary.Add(Overtime.Break, 0);
                sumOvertimeDictionary.Add(Overtime.After, 0);
                sumOvertimeDictionary.Add(Overtime.Holiday, 0);

                if (timeLogGroup != null)
                {
                    sumOvertimeDictionary[Overtime.Before] = timeLogGroup.SumOvertimeBefore;
                    sumOvertimeDictionary[Overtime.Break] = timeLogGroup.SumOvertimeLunch;
                    sumOvertimeDictionary[Overtime.After] = timeLogGroup.SumOvertimeAfter;
                    sumOvertimeDictionary[Overtime.Holiday] = timeLogGroup.SumOvertimeDayOff;
                }

                //Clear All Work Permission in TimeLogPermission 
                // may be  duplicate WorkPermission if update entity
                //timeLogModel.TimeLog.WorkPermission.Clear();

                DateRange clockRange = new DateRange(timeLogModel.ClockIn, timeLogModel.ClockOut);
                //Set For OT Holiday & day off
                //Not Workpermission For holiday => Get Default Overtime Option from Employee
                //HolidayHistoryRepository holidayHistoryRep = new HolidayHistoryRepository();
                //var allHoliday = holidayHistoryRep.GetAllHolidayHistory();
                //Has holiday
                AddTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.Holiday.In(x.OvertimeOptions)));
                timeLogModel.OvertimeDayOff = 0;
                if ((timeLogModel.WorkWeek == null) && timeLogModel.ClockOut.HasValue)
                {
                    timeLogModel.OvertimeDayOff = GetOvertimeValid((float)timeLogModel.ClockOut.Value.Subtract(timeLogModel.ClockIn).TotalMinutes, sumOvertimeDictionary,
                        timeLogModel, employeeModel, workPermissionCollection, Overtime.Holiday);
                }
                else
                {

                    //    // Create before - work in date range
                    //DateRange beforeWorkIn = new DateRange(timeLogModel.WorkWeek.WorkOut.AddDays(-1).AddHours(defineHours), timeLogModel.WorkWeek.WorkIn);

                    //Set For OT Before
                    //Not Workpermission For Before => Get Default Overtime Option from Employee
                    AddTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.Before.In(x.OvertimeOptions)));
                    if (timeLogModel.WorkWeek != null && timeLogModel.ClockIn < timeLogModel.WorkWeek.WorkIn)
                    {
                        TimeSpan OTBefore = timeLogModel.WorkWeek.WorkIn.Subtract(timeLogModel.ClockIn);
                        timeLogModel.OvertimeBefore = GetOvertimeValid((float)OTBefore.TotalMinutes, sumOvertimeDictionary,
                            timeLogModel, employeeModel, workPermissionCollection, Overtime.Before);
                    }
                    else
                    {
                        timeLogModel.OvertimeBefore = 0;
                    }

                    //Set For OT Break
                    //Not Workpermission For Break => Get Default Overtime Option from Employee
                    AddTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.Break.In(x.OvertimeOptions)));
                    if (timeLogModel.WorkWeek != null && timeLogModel.WorkWeek.LunchBreakFlag && !timeLogModel.DeductLunchTimeFlag)
                    {
                        DateRange lunchRange = new DateRange(timeLogModel.WorkWeek.LunchOut, timeLogModel.WorkWeek.LunchIn);
                        if (clockRange.Intersects(lunchRange))
                        {
                            var OTBreak = clockRange.GetIntersection(lunchRange);
                            timeLogModel.OvertimeLunch = GetOvertimeValid((float)OTBreak.TimeSpan.TotalMinutes, sumOvertimeDictionary,
                                timeLogModel, employeeModel, workPermissionCollection, Overtime.Break);
                        }
                    }
                    else
                    {
                        timeLogModel.OvertimeLunch = 0;
                    }
                    //Set For OT After
                    //Not Workpermission For After => Get Default Overtime Option from Employee
                    AddTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.After.In(x.OvertimeOptions)));
                    if (timeLogModel.WorkWeek != null && timeLogModel.ClockOut > timeLogModel.WorkWeek.WorkOut)
                    {
                        TimeSpan OTAfter = timeLogModel.ClockOut.Value.Subtract(timeLogModel.WorkWeek.WorkOut);
                        timeLogModel.OvertimeAfter = GetOvertimeValid((float)OTAfter.TotalMinutes, sumOvertimeDictionary,
                            timeLogModel, employeeModel, workPermissionCollection, Overtime.After);
                    }
                    else
                    {
                        timeLogModel.OvertimeAfter = 0;
                    }


                }
                //End set For Over Time & WorkPermission
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        private float GetOvertimeRemaining(float hourPerDay, float overtimeElapsed)
        {
            float overtimeRemaining = 0;
            if (hourPerDay > overtimeElapsed)
                overtimeRemaining = hourPerDay - overtimeElapsed;
            return overtimeRemaining;
        }

        private bool IsDayOffOrHoliday(tims_TimeLogModel timeLogModel)
        {
            if (timeLogModel.WorkWeek == null)
            {
                return true;
            }
            return false;
        }

        private System.Diagnostics.Stopwatch _refeshTime = new System.Diagnostics.Stopwatch();

        private void RefreshAll()
        {
            _refeshTime.Start();
            //_employeeRepository.RefreshAll<tims_WorkPermission>();
            //_employeeRepository.RefreshAll<tims_TimeLog>();
            //_employeeRepository.RefreshAll<base_GuestSchedule>();
            //_employeeRepository.RefreshAll<tims_Holiday>();
            //_employeeRepository.RefreshAll<tims_WorkSchedule>();
            //_employeeRepository.RefreshAll<tims_WorkWeek>();
            _refeshTime.Stop();
            _log4net.Info("Time For Refesh TimeLogManual Editing: " + _refeshTime.ElapsedMilliseconds + " ms");
        }

        #endregion

        #region Override Methods

        protected override bool CanExecuteClosing()
        {
            try
            {
                if (SelectedEmployee != null)
                {
                    if (!IsSearchMode && SelectedEmployee.TimeLogCollection.Count(x => x.IsDirty) > 0 || (SelectedEmployee.TimeLogCollection.DeletedItems != null && SelectedEmployee.TimeLogCollection.DeletedItems.Count > 0))
                    {
                        if (SelectedEmployee.TimeLogCollection.Count(x => x.IsNew) > 0) // Is a New Item
                        {
                            return NewItemCheck();
                        }
                        else //if ((SelectedEmployee.TimeLogCollection.DeletedItems != null && SelectedEmployee.TimeLogCollection.DeletedItems.Count > 0)) // Is Old Item
                        {
                            return OldItemCheck();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
            return true;
        }

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            // Load data by predicate
            LoadDataByPredicate(true);
        }

        #endregion

        #region DelaySearch Methods

        /// <summary>
        /// Timer for searching
        /// </summary>
        protected System.Windows.Threading.DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

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
    }
}