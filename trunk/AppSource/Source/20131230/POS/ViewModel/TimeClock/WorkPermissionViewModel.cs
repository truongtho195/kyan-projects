using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Linq.Expressions;

namespace CPC.POS.ViewModel
{
    class WorkPermissionViewModel : ViewModelBase
    {
        #region Define

        private tims_WorkPermissionRepository _workPermissionRepository = new tims_WorkPermissionRepository();
        private base_GuestRepository _employeeRepository = new base_GuestRepository();

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

        private string _keyword;

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

        private ObservableCollection<tims_WorkPermissionModel> _workPermissionCollection = new ObservableCollection<tims_WorkPermissionModel>();
        /// <summary>
        /// Gets or sets the WorkPermissionCollection.
        /// </summary>
        public ObservableCollection<tims_WorkPermissionModel> WorkPermissionCollection
        {
            get { return _workPermissionCollection; }
            set
            {
                if (_workPermissionCollection != value)
                {
                    _workPermissionCollection = value;
                    OnPropertyChanged(() => WorkPermissionCollection);
                }
            }
        }

        private int _totalWorkPermissions;
        /// <summary>
        /// Gets or sets the TotalWorkPermissions.
        /// </summary>
        public int TotalWorkPermissions
        {
            get { return _totalWorkPermissions; }
            set
            {
                if (_totalWorkPermissions != value)
                {
                    _totalWorkPermissions = value;
                    OnPropertyChanged(() => TotalWorkPermissions);
                }
            }
        }

        private ObservableCollection<base_GuestModel> _employeeCollection;
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
                    if (_selectedEmployee != null)
                    {
                        //if (!_isWorkPermissionChanged)
                        //{
                        _selectedEmployee.WorkPermissionCollection = new ObservableCollection<tims_WorkPermissionModel>(_selectedEmployee.base_Guest.tims_WorkPermission.OrderBy(x => x.FromDate).Select(x => new tims_WorkPermissionModel(x)));
                        if (_selectedEmployee.WorkPermissionCollection.Count > 0)
                            SelectedWorkPermission = _selectedEmployee.WorkPermissionCollection.FirstOrDefault();
                        else
                            SelectedWorkPermission = null;
                        //}
                    }
                    OnPropertyChanged(() => SelectedEmployee);

                }
            }
        }

        private tims_WorkPermissionModel _selectedWorkPermission;
        /// <summary>
        /// Gets or sets the SelectedPermission.
        /// </summary>
        public tims_WorkPermissionModel SelectedWorkPermission
        {
            get { return _selectedWorkPermission; }
            set
            {
                if (_selectedWorkPermission != value)
                {
                    _selectedWorkPermission = value;
                    if (null != value)
                    {
                        if (value.IsNew)
                        {
                            _selectedWorkPermission.FromDate = DateTime.Today;
                            _selectedWorkPermission.ToDate = DateTime.Today;
                            _selectedWorkPermission.PayEventSelected = 0;
                            //_selectedWorkPermission.SetForPayEventSelected();
                            _selectedWorkPermission.ActiveFlag = true;
                            _selectedWorkPermission.Note = string.Empty;
                            _selectedWorkPermission.NoOfDays = 1;
                            _selectedWorkPermission.HourPerDay = 1;
                            _selectedWorkPermission.IsDirty = false;
                            this.EmployeeList.Clear();
                        }
                        else
                        {
                            _selectedWorkPermission.ToModel();
                            _selectedWorkPermission.SetForPayEventSelected();
                            _selectedWorkPermission.EndUpdate();
                        }

                    }
                    OnPropertyChanged(() => SelectedWorkPermission);
                    OnPropertyChanged(() => Deactivated);

                    if (SelectedWorkPermission != null)
                    {
                        SelectedWorkPermission.PropertyChanged -= new PropertyChangedEventHandler(SelectedWorkPermission_PropertyChanged);
                        SelectedWorkPermission.PropertyChanged += new PropertyChangedEventHandler(SelectedWorkPermission_PropertyChanged);
                    }
                }
            }
        }

        #region Employee List
        private ObservableCollection<base_GuestModel> _employeeList = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the EmployeeList.
        /// </summary>
        public ObservableCollection<base_GuestModel> EmployeeList
        {
            get { return _employeeList; }
            set
            {
                if (_employeeList != value)
                {
                    _employeeList = value;
                    OnPropertyChanged(() => EmployeeList);
                }
            }
        }
        #endregion

        #region Deactived
        /// <summary>
        /// Gets or sets the Deactived.
        /// </summary>
        public bool Deactivated
        {
            get
            {
                if (SelectedWorkPermission != null)
                    return SelectedWorkPermission.ActiveFlag;
                return true;
            }

        }
        #endregion

        private ObservableCollection<ComboItem> _payEventCollection;
        /// <summary>
        /// Gets or sets the PayEventCollection.
        /// </summary>
        public ObservableCollection<ComboItem> PayEventCollection
        {
            get { return _payEventCollection; }
            set
            {
                if (_payEventCollection != value)
                {
                    _payEventCollection = value;
                    OnPropertyChanged(() => PayEventCollection);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkPermissionViewModel()
            : base()
        {
            // Get main viewmodel
            _ownerViewModel = App.Current.MainWindow.DataContext;

            // Load static datas
            LoadStaticData();

            // Initial commands
            InitialCommand();
        }

        public WorkPermissionViewModel(bool isList, object param = null)
            : this()
        {
            ChangeSearchMode(isList, param);
        }

        #endregion

        #region Command Methods

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
            return IsValid & IsEdit();
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            try
            {
                SaveWorkPermission();
                //Go to Grid Search
                this.IsSearchMode = true;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        #endregion

        #region ClearCommand

        /// <summary>
        /// Gets the ClearCommand command.
        /// </summary>
        public ICommand ClearCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ClearCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnClearCommandCanExecute()
        {
            if (SelectedWorkPermission == null)
                return false;
            return SelectedWorkPermission.IsNew && SelectedWorkPermission.IsDirty;
        }

        /// <summary>
        /// Method to invoke when the ClearCommand command is executed.
        /// </summary>
        private void OnClearCommandExecute()
        {
            SelectedWorkPermission = new tims_WorkPermissionModel();
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
            AssignEmployee();
        }

        #endregion

        #region RemoveEmployeeCommand

        /// <summary>
        /// Gets the RemoveEmployeeCommand command.
        /// </summary>
        public ICommand RemoveEmployeeCommand { get; private set; }

        /// <summary>
        /// Method to check whether the RemoveEmployeeCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRemoveEmployeeCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return param != null & (param as ObservableCollection<object>).Count > 0;
        }

        /// <summary>
        /// Method to invoke when the RemoveEmployeeCommand command is executed.
        /// </summary>
        private void OnRemoveEmployeeCommandExecute(object param)
        {
            RemoveEmployee(param);
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

        #region Mouse Double Click Command

        /// <summary>
        /// Gets the MouseDoubleClickCommand command.
        /// </summary>
        public ICommand MouseDoubleClickCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool CanMouseDoubleClickExecute(object param)
        {
            if (IsSearchMode && param == null)
                return false;
            return SelectedWorkPermission != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void OnMouseDoubleClickExecuted(object param)
        {
            try
            {
                if (param != null && param is tims_WorkPermissionModel)
                {
                    OnWorkPermissionSelectionChangedExecuted(param);
                    OnPropertyChanged(() => Deactivated);
                }

                if (!IsSearchMode && SelectedWorkPermission.IsNew && SelectedWorkPermission.IsDirty) // Is a New Item
                    NewItemDirtyCheck(true);
                else if (!IsSearchMode && SelectedWorkPermission.IsDirty) // Is Old Item
                    OldItemDirtyCheck(true);
                else
                    IsSearchMode = !IsSearchMode;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region Work Permission Selection Changed

        /// <summary>
        /// Gets the WorkPermissionSelectionChangedCommand command.
        /// </summary>
        public ICommand WorkPermissionSelectionChangedCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool OnWorkPermissionSelectionChangedCanExecuted(object item)
        {
            if (item == null)
                return false;
            return item is tims_WorkPermissionModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void OnWorkPermissionSelectionChangedExecuted(object item)
        {
            try
            {
                SelectedWorkPermission = item as tims_WorkPermissionModel;
                //_isWorkPermissionChanged = true;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region Employee Selection Changed

        /// <summary>
        /// Gets the EmployeeSelectionChangedCommand command.
        /// </summary>
        public ICommand EmployeeSelectionChangedCommand { get; private set; }

        /// <summary>
        ///  Employee Selection Changed
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool OnEmployeeSelectionChangedCanExecuted(object item)
        {
            if (item == null)
                return false;
            return item is base_GuestModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void OnEmployeeSelectionChangedExecuted(object item)
        {
            try
            {
                SelectedEmployee = item as base_GuestModel;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
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
                // Get employee mark
                string employeeMark = MarkType.Employee.ToDescription();

                // Load employee collection
                EmployeeCollection = new ObservableCollection<base_GuestModel>(_employeeRepository.
                    GetAll(x => !x.IsPurged && x.Mark.Equals(employeeMark) && x.IsActived).
                    OrderBy(x => x.FirstName).ThenBy(x => x.LastName).
                    Select(x => new base_GuestModel(x)));

                // Load pay event collection
                PayEventCollection = new ObservableCollection<ComboItem>(Common.PayEvents);
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
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            ClearCommand = new RelayCommand(OnClearCommandExecute, OnClearCommandCanExecute);
            AssignEmployeeCommand = new RelayCommand(OnAssignEmployeeCommandExecute, OnAssignEmployeeCommandCanExecute);
            RemoveEmployeeCommand = new RelayCommand<object>(OnRemoveEmployeeCommandExecute, OnRemoveEmployeeCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            MouseDoubleClickCommand = new RelayCommand<object>(OnMouseDoubleClickExecuted, this.CanMouseDoubleClickExecute);
            WorkPermissionSelectionChangedCommand = new RelayCommand<object>(OnWorkPermissionSelectionChangedExecuted, OnWorkPermissionSelectionChangedCanExecuted);
            EmployeeSelectionChangedCommand = new RelayCommand<object>(OnEmployeeSelectionChangedExecuted, OnEmployeeSelectionChangedCanExecuted);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            if (this.SelectedWorkPermission == null)
                return false;
            else if (this.SelectedWorkPermission.IsNew)
                return EmployeeList.Count > 0 && this.EmployeeList.Count(x => x.DuplicateWorkPermission) == 0;
            return this.SelectedWorkPermission.IsDirty && IsValid && !SelectedWorkPermission.HasDuplicated;
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
                        result = SaveWorkPermission();
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    if (SelectedWorkPermission.IsNew)
                    {
                        SelectedWorkPermission = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else
                    {
                        // Refresh product datas

                    }
                }
            }
            else
            {
                if (SelectedWorkPermission != null && SelectedWorkPermission.IsNew)
                {
                    SelectedWorkPermission = null;
                }
            }

            if (result && isClosing == null && SelectedWorkPermission != null)
            {
                // Refresh product datas

                // Clear selected item
                SelectedWorkPermission = null;
            }

            return result;
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<tims_WorkPermission, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<tims_WorkPermission, bool>> predicate = PredicateBuilder.True<tims_WorkPermission>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<tims_WorkPermission>();
            }

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
            Expression<Func<tims_WorkPermission, bool>> predicate = CreateSearchPredicate(_keyword);

            // Load data by predicate
            LoadDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="predicate">Condition for load data</param>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(Expression<Func<tims_WorkPermission, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                WorkPermissionCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                if (refreshData)
                {
                }

                // Get total work permissions with condition in predicate
                TotalWorkPermissions = _workPermissionRepository.GetIQueryable(predicate).Count();

                // Get data with range
                IList<tims_WorkPermission> products = _workPermissionRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.CreatedDate, predicate);
                foreach (tims_WorkPermission product in products)
                {
                    bgWorker.ReportProgress(0, product);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create product model
                tims_WorkPermissionModel workPermissionModel = new tims_WorkPermissionModel((tims_WorkPermission)e.UserState);

                // Load relation data
                LoadRelationData(workPermissionModel);

                // Add to collection
                WorkPermissionCollection.Add(workPermissionModel);
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
        /// Load relation data for product
        /// </summary>
        /// <param name="workPermissionModel"></param>
        private void LoadRelationData(tims_WorkPermissionModel workPermissionModel)
        {
            // Turn off IsDirty & IsNew
            workPermissionModel.EndUpdate();
        }

        /// <summary>
        /// Save Workpermission
        /// </summary>
        private bool SaveWorkPermission()
        {
            try
            {
                tims_WorkPermissionRepository workPermissionRepository = new tims_WorkPermissionRepository();
                if (SelectedWorkPermission.IsNew)
                {
                    base_GuestRepository employeeRepository = new base_GuestRepository();
                    foreach (var item in EmployeeList)
                    {
                        tims_WorkPermissionModel workPermissionModel = new tims_WorkPermissionModel();
                        workPermissionModel = SelectedWorkPermission.Clone<tims_WorkPermissionModel>();
                        workPermissionModel.EmployeeId = item.Id;
                        workPermissionModel.CreatedDate = DateTimeExt.Now;
                        //[FIX]workPermissionModel.CreatedById = Define.UserLoginID;
                        workPermissionModel.ToEntity();
                        workPermissionModel.Id = 0;
                        workPermissionModel.IsNew = true;
                        workPermissionRepository.Add(workPermissionModel.tims_WorkPermission);
                        workPermissionRepository.Commit();
                        workPermissionModel.EndUpdate();
                        //Add to collection if a New Item
                        var employeeModel = EmployeeCollection.Where(x => x.Id == item.Id).SingleOrDefault();
                        if (employeeModel.WorkPermissionCollection != null)
                            employeeModel.WorkPermissionCollection.Add(workPermissionModel);
                        else
                            employeeModel.WorkPermissionCollection = new ObservableCollection<tims_WorkPermissionModel>(employeeModel.base_Guest.tims_WorkPermission.OrderBy(x => x.FromDate).Select(x => new tims_WorkPermissionModel(x)));

                        UpdateTimeLog(true, item);
                    }
                    this.SelectedWorkPermission = null;
                    this.SelectedEmployee = null;
                    EmployeeList.Clear();

                }
                else if (SelectedWorkPermission.IsDirty)
                {
                    //SelectedWorkPermission.ModifiedByID = Define.UserLoginID;
                    SelectedWorkPermission.ModifiedDate = DateTimeExt.Now;
                    this.SelectedWorkPermission.ToEntity();
                    workPermissionRepository.Commit();
                    this.SelectedWorkPermission.EndUpdate();

                    UpdateTimeLog(false, this.SelectedEmployee);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeModel"></param>
        private void SetEmployeeDuplicatedWorkPermission(base_GuestModel employeeModel)
        {
            try
            {
                var dupicate = from d in employeeModel.base_Guest.tims_WorkPermission
                               where d.ActiveFlag
                                      && d.Id != SelectedWorkPermission.Id
                                      && new DateRange(d.FromDate.Date, d.ToDate.Date).Intersects(new DateRange(SelectedWorkPermission.FromDate.Date, SelectedWorkPermission.ToDate.Date))
                                      && (d.OvertimeOptions > 0 && SelectedWorkPermission.OvertimeOptions > 0 && (d.OvertimeOptions.Has(SelectedWorkPermission.OvertimeOptions) || d.OvertimeOptions.In(SelectedWorkPermission.OvertimeOptions))
                                      || (d.PermissionType > 0 && SelectedWorkPermission.PermissionType > 0 && (d.PermissionType.Has(SelectedWorkPermission.PermissionType) || d.PermissionType.In(SelectedWorkPermission.PermissionType))))
                               select d;
                if (SelectedWorkPermission.IsNew)
                {
                    employeeModel.DuplicateWorkPermission = false;
                    if (dupicate.Count() > 0)
                        employeeModel.DuplicateWorkPermission = true;
                }
                else
                {
                    SelectedWorkPermission.HasDuplicated = false;
                    if (dupicate.Count() > 0)
                        SelectedWorkPermission.HasDuplicated = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Update TimeLog After Create or Update WorkPermission
        /// </summary>
        /// <param name="isNewItem">If New WorkPermission , Update what item belong to FromDate - ToDate of WorkPermssion
        ///                         else Update all item TimeLog of that employee
        /// </param>
        /// <param name="employeeModel"></param>
        private void UpdateTimeLog(bool isNewItem, base_GuestModel employeeModel)
        {
            try
            {
                tims_TimeLogRepository timeLogRepository = new tims_TimeLogRepository();
                //Check 13/11/2012
                //if (isNewItem)
                employeeModel.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(employeeModel.base_Guest.tims_TimeLog.Where(x => x.ClockIn.Date >= SelectedWorkPermission.FromDate.Date && x.ClockOut.HasValue && x.ClockOut.Value.Date <= SelectedWorkPermission.ToDate.Date).Select(x => new tims_TimeLogModel(x)));
                //else
                //    employeeModel.TimeLogCollection = new CollectionBase<TimeLogModel>(employeeModel.Employee.TimeLog.Select(x => new TimeLogModel(x)));

                foreach (var timeLogModel in employeeModel.TimeLogCollection)
                {

                    //Check is Day Off Or Holiday
                    if (!this.IsDayOffOrHoliday(timeLogModel))
                    {
                        // Set For workTime
                        timeLogModel.WorkTime = (float)timeLogModel.GetWorkTime().TotalMinutes;

                        //Set For LunchTime 
                        timeLogModel.LunchTime = (float)timeLogModel.GetLunchTime().TotalMinutes;

                        ///
                        ///Set For Early Time & LeaveEarlyTime
                        ///
                        //Get TimeClock Min & TimeClock Max in TimeLogCollection Compare with current item iterated
                        var isTimeLogMin = employeeModel.TimeLogCollection.Where(x => x.ClockIn.Date == timeLogModel.ClockIn.Date).Aggregate((cur, next) => cur.ClockIn < next.ClockIn ? cur : next);
                        var isTimeLogMax = employeeModel.TimeLogCollection.Where(x => x.ClockIn.Date == timeLogModel.ClockIn.Date).Aggregate((cur, next) => cur.ClockOut > next.ClockOut ? cur : next);
                        //if current item is not TimeClock Min & Time Clock Max => set Late & Early =0
                        if (isTimeLogMin != timeLogModel && timeLogModel != isTimeLogMax)
                        {
                            timeLogModel.LateTime = 0;
                            timeLogModel.LeaveEarlyTime = 0;
                        }
                        else
                        {
                            if (isTimeLogMin != null && isTimeLogMin == timeLogModel)// Current item is a TimeClock Min
                                UpdateLateTime(timeLogModel);
                            if (isTimeLogMax != null && isTimeLogMax == timeLogModel)// Current item is a TimeClock Max
                                UpdateLeaveEarlyTime(timeLogModel);

                            if (isTimeLogMin != isTimeLogMax && isTimeLogMin == timeLogModel)//current item is a Min TimeClock & TimeClock min & max is not one =>  Set LeaveEarly Time = 0
                                timeLogModel.LeaveEarlyTime = 0;
                            else if (isTimeLogMin != isTimeLogMax && isTimeLogMax == timeLogModel)// Current item  is a max TimeClock & Min & max is not one = > Set LateTime is 0
                                timeLogModel.LateTime = 0;
                        }
                    }
                    else
                    {
                        timeLogModel.WorkTime = 0;
                        timeLogModel.LunchTime = 0;
                        timeLogModel.LateTime = 0;
                        timeLogModel.LeaveEarlyTime = 0;
                    }
                    ////Calculate For Overtime
                    CalcOverTime(timeLogModel, employeeModel);

                    timeLogModel.ToEntity();

                    timeLogRepository.Commit();
                    timeLogModel.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
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
            // Declare overtime valid variable
            float overtimeValid = 0;
            try
            {
                // Declare overtime elapsed variable
                float overtimeElapsed = sumOvertimeDictionary[overtime];

                // Get work permission have over time option equal param
                tims_WorkPermission workPermission = workPermissionCollection.FirstOrDefault(x => x.ActiveFlag && overtime.In(x.OvertimeOptions));

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

                    timeLogModel.OvertimeOptions = (int)((Overtime)timeLogModel.OvertimeOptions).Add(overtime);


                }
                else if (overtime.In(timeLogModel.OvertimeOptions))
                {
                    overtimeValid = overtimeValue;

                    // Update overtime options
                    timeLogModel.OvertimeOptions = (int)((Overtime)timeLogModel.OvertimeOptions).Add(overtime);
                }

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return overtimeValid;
        }

        /// <summary>
        /// Calculate Over Time For employee with TimeLog & WorkPermission
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="employeeModel"></param>
        private void CalcOverTime(tims_TimeLogModel timeLogModel, base_GuestModel employeeModel)
        {
            try
            {
                // Get work permission collecion
                var workPermissionCollection = employeeModel.base_Guest.tims_WorkPermission.Where(x => timeLogModel.ClockIn.Date >= x.FromDate.Date && timeLogModel.ClockOut.HasValue && timeLogModel.ClockOut.Value.Date <= x.ToDate);
                foreach (var item in workPermissionCollection)
                {
                    UpdateTimeLogPermission(timeLogModel, item);
                }
                // Get previous timelogs to calc overtime elapsed
                var timeLogGroup = (from x in employeeModel.TimeLogCollection.Where(x =>
                                    x.ActiveFlag &&
                                    x.ClockOut.HasValue &&
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
                //Has holiday
                //UpdateTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.Holiday.In(x.OvertimeOptions)));
                timeLogModel.OvertimeDayOff = 0;
                if ((timeLogModel.WorkWeek == null) && timeLogModel.ClockOut.HasValue)
                {
                    timeLogModel.OvertimeDayOff = GetOvertimeValid((float)timeLogModel.ClockOut.Value.Subtract(timeLogModel.ClockIn).TotalMinutes, sumOvertimeDictionary,
                        timeLogModel, employeeModel, workPermissionCollection, Overtime.Holiday);
                }
                else
                {

                    //Set For OT Before
                    //Not Workpermission For Before => Get Default Overtime Option from Employee
                    //UpdateTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.Before.In(x.OvertimeOptions)));
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
                    //UpdateTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.Break.In(x.OvertimeOptions)));
                    if (timeLogModel.WorkWeek.LunchBreakFlag && !timeLogModel.DeductLunchTimeFlag)
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
                    //UpdateTimeLogPermission(timeLogModel, workPermissionCollection.FirstOrDefault(x => Overtime.After.In(x.OvertimeOptions)));
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
                throw ex;
            }
        }

        /// <summary>
        /// Calcute Overtime Remain
        /// </summary>
        /// <param name="hourPerDay"></param>
        /// <param name="overtimeElapsed"></param>
        /// <returns></returns>
        private float GetOvertimeRemaining(float hourPerDay, float overtimeElapsed)
        {
            float overtimeRemaining = 0;
            if (hourPerDay > overtimeElapsed)
                overtimeRemaining = hourPerDay - overtimeElapsed;
            return overtimeRemaining;
        }

        /// <summary>
        /// Add TimeLog WorkPermision If not existed
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="workPermissions"></param>
        private void UpdateTimeLogPermission(tims_TimeLogModel timeLogModel, tims_WorkPermission workPermissions)
        {
            if (workPermissions != null)
            {
                if (!workPermissions.ActiveFlag)
                {
                    var removeItem = timeLogModel.tims_TimeLog.tims_WorkPermission.Where(x => x.Id == workPermissions.Id).SingleOrDefault();
                    if (removeItem != null)
                        timeLogModel.tims_TimeLog.tims_WorkPermission.Remove(removeItem);
                }
                else if (!timeLogModel.tims_TimeLog.tims_WorkPermission.Any(x => x == workPermissions))
                {
                    timeLogModel.tims_TimeLog.tims_WorkPermission.Add(workPermissions);
                }
            }
        }

        /// <summary>
        /// Calculate & update leaveEarly if ClockOut less than WorkIN
        /// </summary>
        /// <param name="timeLogModel"></param>
        private void UpdateLeaveEarlyTime(tims_TimeLogModel timeLogModel)
        {
            if (timeLogModel.WorkWeek != null && timeLogModel.ClockOut < timeLogModel.WorkWeek.WorkOut && timeLogModel.ClockOut.HasValue)
                timeLogModel.LeaveEarlyTime = (float)timeLogModel.WorkWeek.WorkOut.Subtract(timeLogModel.ClockOut.Value).TotalMinutes;
        }

        /// <summary>
        /// Calculate & update for late time if clock in greater than latetime
        /// </summary>
        /// <param name="timeLogModel"></param>
        private void UpdateLateTime(tims_TimeLogModel timeLogModel)
        {
            if (timeLogModel.WorkWeek != null && timeLogModel.ClockIn > timeLogModel.WorkWeek.WorkIn)
                timeLogModel.LateTime = (float)timeLogModel.ClockIn.Subtract(timeLogModel.WorkWeek.WorkIn).TotalMinutes;
        }

        private bool IsDayOffOrHoliday(tims_TimeLogModel timeLogModel)
        {
            if (timeLogModel.WorkWeek == null)
            {
                return true;
            }
            return false;
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
        ///<remarks>
        ///Required to  check SelectedWorkPermission.IsNew && SelectedWorkPermission.IsDirty
        /// </remarks>
        /// </summary>
        private bool NewItemDirtyCheck(bool isSearchModel)
        {
            try
            {
                if (IsValid && EmployeeList.Count > 0)// No Error : YES : Save; NO : Delete, CANCEL : No Action
                {
                    var result = ShowMessageBox("Do you want to save this item ?", "TIMS - Work Permission", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        SaveWorkPermission();
                        IsSearchMode = isSearchModel;//true
                        return true;
                    }
                    else if (MessageBoxResult.No.Equals(result))
                    {
                        IsSearchMode = isSearchModel;//true
                        return true;
                    }
                    return false;
                }
                else //Has Error => Delete
                {
                    var result = ShowMessageBox("Do you want close this form ?", "TIMS - Work Permission", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        IsSearchMode = isSearchModel;//true
                        SelectedWorkPermission = null;
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
        /// </param>
        /// /// <remarks>
        /// Required to check this is a old item (SelectedWorkPermission) : IsNew = false &  Has Dirty
        /// </remarks>
        private bool OldItemDirtyCheck(bool isSearchModel)
        {
            try
            {
                if (IsValid) // No Error : YES : Save; NO : RollBack, CANCEL : No Action
                {
                    var result = ShowMessageBox("Do you want to save this item ?", "TIMS - Work Permission", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        SaveWorkPermission();
                        IsSearchMode = isSearchModel;
                        return true;
                    }
                    else if (MessageBoxResult.No.Equals(result))
                    {
                        SelectedWorkPermission.ToModel();
                        SelectedWorkPermission.EndUpdate();
                        IsSearchMode = isSearchModel;
                        return true;
                    }
                    return false;
                }
                else // Has Error : YES : roll Back Data; NO : No action
                {
                    var result = ShowMessageBox("Do you want to close this form?", "TIMS - Work Permission", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes.Equals(result))
                    {
                        SelectedWorkPermission.ToModel();
                        SelectedWorkPermission.EndUpdate();
                        IsSearchMode = isSearchModel;
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
        /// 
        /// </summary>
        private void AvalidableEmployeeWorkpermission()
        {
            if (EmployeeCollection != null)
            {
                ICollectionView colletionView = CollectionViewSource.GetDefaultView(EmployeeCollection);
                if (colletionView != null)
                {
                    colletionView.Filter = (one) =>
                       {
                           base_GuestModel employeeModel = one as base_GuestModel;
                           if (employeeModel == null)
                               return false;
                           else
                               return employeeModel.base_Guest.tims_WorkPermission.Count > 0;
                       };
                }

            }
        }

        /// <summary>
        /// Assign Employee for work permission
        /// </summary>
        private void AssignEmployee()
        {
            try
            {
                EmployeesListViewModel employeeListViewModel = new EmployeesListViewModel();
                if (EmployeeList != null && EmployeeList.Count > 0)
                    employeeListViewModel.SetValue(EmployeeList.ToList());
                bool? result = _dialogService.ShowDialog<EmployeesListView>(_ownerViewModel, employeeListViewModel, "Employee List");

                if (result == true)
                {
                    this.EmployeeList.Clear();
                    var employeeList = employeeListViewModel.RightEmployeeCollection;

                    foreach (base_Guest employee in employeeList)
                    {
                        base_GuestModel employeeModel = new base_GuestModel(employee);
                        /// Create Check Duplidate workpermission here & set to DuplicateWorkPermission
                        if (SelectedWorkPermission.OvertimeOptions != 0 || SelectedWorkPermission.PermissionType != 0)
                        {
                            SetEmployeeDuplicatedWorkPermission(employeeModel);
                        }
                        this.EmployeeList.Add(employeeModel);
                    }
                    this.SelectedWorkPermission.IsDirty = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Remove Employee from Work permission
        /// </summary>
        /// <param name="param"></param>
        private void RemoveEmployee(object param)
        {
            try
            {
                var employeeList = (ObservableCollection<object>)param;
                if (employeeList != null)
                {
                    foreach (var item in employeeList.ToList())
                    {
                        EmployeeList.Remove(item as base_GuestModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        private System.Diagnostics.Stopwatch _refeshTime = new System.Diagnostics.Stopwatch();

        private void RefreshAll()
        {
            _refeshTime.Start();
            _employeeRepository.Refresh();
            //_employeeRepository.RefreshAll<WorkPermission>();
            //_employeeRepository.RefreshAll<TimeLog>();
            //_employeeRepository.RefreshAll<EmployeeSchedule>();
            //_employeeRepository.RefreshAll<Holiday>();
            //_employeeRepository.RefreshAll<WorkSchedule>();
            //_employeeRepository.RefreshAll<WorkWeek>();
            _refeshTime.Stop();
            _log4net.Info("Time For Refesh WorkPermission: " + _refeshTime.ElapsedMilliseconds + " ms");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method call from MainViewModel if DataContext existed & check item has dirty or new 
        /// </summary>
        /// <param name="isSearchMode"></param>
        public void MainWindow(bool isSearchMode)
        {
            try
            {
                bool canAddNewItem = true;

                if (SelectedWorkPermission != null && (SelectedWorkPermission.IsDirty)) //item is has edited
                {
                    if (SelectedWorkPermission.IsNew)
                        canAddNewItem = NewItemDirtyCheck(isSearchMode);
                    else
                        canAddNewItem = OldItemDirtyCheck(isSearchMode);
                }
                else
                {
                    this.IsSearchMode = isSearchMode;
                }

                if (canAddNewItem && !isSearchMode) // Change To Form & Use not stop to edit error item
                {
                    SelectedWorkPermission = new tims_WorkPermissionModel();
                    this.IsSearchMode = isSearchMode;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        #endregion

        #region Override Methods

        protected override bool CanExecuteClosing()
        {
            try
            {
                if (!IsSearchMode && SelectedWorkPermission != null && (SelectedWorkPermission.IsDirty)) //item is has edited
                {
                    if (SelectedWorkPermission.IsNew)
                        return NewItemDirtyCheck(false);
                    else
                        return OldItemDirtyCheck(false);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            if (SelectedEmployee != null && !SelectedEmployee.IsNew)
            {
                lock (UnitOfWork.Locker)
                {
                    // Refresh static data

                }

                // Refresh product datas

            }

            // Load data by predicate
            LoadDataByPredicate(true);
        }

        /// <summary>
        /// Process when change display view
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (ShowNotification(null))
            {
                // When user clicked create new button
                if (!isList)
                {
                    // Create new product
                    SelectedWorkPermission = new tims_WorkPermissionModel();

                    // Display product detail
                    IsSearchMode = false;
                }
                else
                {
                    // When user click view list button
                    // Display product list
                    IsSearchMode = true;
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

        private void SelectedWorkPermission_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("FromDate".Equals(e.PropertyName) || "ToDate".Equals(e.PropertyName) || "OvertimeOptions".Equals(e.PropertyName) || "PermissionType".Equals(e.PropertyName))
            {
                if (SelectedWorkPermission.ToDate >= SelectedWorkPermission.FromDate)
                {
                    if (SelectedWorkPermission.IsNew)
                    {
                        foreach (var employeeModel in this.EmployeeList)
                        {
                            SetEmployeeDuplicatedWorkPermission(employeeModel);
                        }
                    }
                    else
                    {
                        SetEmployeeDuplicatedWorkPermission(SelectedEmployee);
                    }
                }
            }
        }

        #endregion
    }
}