//------------------------------------------------------------------------------
// <History>
//11/12/2012 : Set Employee IsChecked = false with TimeLog Collection is Empty
//             When user clicked Toggle button to show rowDetail set Collapsed with Timelog Collection Empty
// </History>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS;
using CPC.Control;

namespace CPC.POS.ViewModel
{
    class TimeClockManualEventEditingViewModel : ViewModelBase
    {
        #region Define

        // Commands
        public ICommand NewCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand MouseDoubleClickCommand { get; private set; }
        public ICommand EmployeeSelectionChangedCommand { get; private set; }
        public ICommand TimeLogSelectionChangedCommand { get; private set; }
        public ICommand NewTimeLogCommand { get; private set; }
        public ICommand RemoveTimeLogCommand { get; private set; }
        public ICommand FilterEmployeeHasTimeLog { get; private set; }

        private ICollectionView _collectionView;
        private ICollectionView _timeLogCollectionView;

        private base_GuestRepository _employeeRepository = new base_GuestRepository();

        private List<tims_HolidayHistory> HolidayHistoryCollection;

        #endregion

        #region Properties
        #region IsSearchMode
        /// <summary>
        /// Search Mode: 
        /// true open the Search grid.
        /// false close the search grid and open data entry.
        /// </summary>
        private bool isSearchMode = false;
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

        #region EmployeeCollection
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
        #endregion

        #region Selected Employee
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

                    if (null != value)
                    {
                        _selectedEmployee.ToModel();
                        _selectedEmployee.TimeLogCollection = new CollectionBase<tims_TimeLogModel>(_selectedEmployee.base_Guest.tims_TimeLog.OrderBy(x => x.ClockIn).ThenBy(x => x.ClockOut).Select(x => new tims_TimeLogModel(x,true)
                        {
                         
                            ClockInDate = new DateTime(x.ClockIn.Year, x.ClockIn.Month, x.ClockIn.Day),
                            WorkScheduleGroup = x.tims_WorkSchedule.WorkScheduleName + " - " + x.base_Guest.base_GuestSchedule.Where(y => y.StartDate <= x.ClockIn.Date).OrderBy(y => y.StartDate).ThenBy(y => y.AssignDate).Last().StartDate.ToString("dd/MM/yyyy"),
                            IsManual = true,
                            IsDirty = false
                        }));
                        //BK: WorkScheduleGroup = x.WorkSchedule.WorkScheduleName + " - " + x.WorkSchedule.EmployeeSchedule.Last(y => y.WorkScheduleID == x.WorkScheduleID && y.EmployeeID == x.EmployeeID && y.StartDate <= x.ClockIn).StartDate.ToString("dd/MM/yyyy"),
                        FilterTimeLog(_selectedEmployee);
                    }
                    OnPropertyChanged(() => SelectedEmployee);
                    OnPropertyChanged(() => ReadOnlyForm);
                }
            }
        }
        #endregion

        #region SelectedTimeLog
        private tims_TimeLogModel _selectedTimeLog;
        /// <summary>
        /// Gets or sets the SelectedTimeLog.
        /// </summary>
        public tims_TimeLogModel SelectedTimeLog
        {
            get { return _selectedTimeLog; }
            set
            {
                if (_selectedTimeLog != value)
                {
                    if (_selectedTimeLog != null)
                        SelectedTimeLog.IsChecked = false;
                    _selectedTimeLog = value;
                    OnPropertyChanged(() => SelectedTimeLog);
                    if (SelectedTimeLog != null)
                    {
                        this.SelectedTimeLog.IsChecked = true;
                        //if (SelectedTimeLog.TimeLogList == null)
                        //    SelectedTimeLog.TimeLogList = new System.Collections.Generic.List<TimeLogModel>();
                        //SelectedTimeLog.TimeLogList.Clear();
                        //SelectedTimeLog.TimeLogList = new System.Collections.Generic.List<TimeLogModel>(SelectedEmployee.TimeLogCollection.ToList());
                        this.SelectedTimeLog.EmployeeTemp = SelectedEmployee;
                        this.SelectedTimeLog.SetWorkPermissionOT();
                        this.SelectedTimeLog.SetOverTimeFromEmployee();
                        this.SelectedTimeLog.RaiseRowDetailEditing();
                        SelectedTimeLog.PropertyChanged -= new PropertyChangedEventHandler(_selectedTimeLog_PropertyChanged);
                        SelectedTimeLog.PropertyChanged += new PropertyChangedEventHandler(_selectedTimeLog_PropertyChanged);
                    }
                }
            }
        }

        private void _selectedTimeLog_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("TempItem".Equals(e.PropertyName))
            {
                var timeLog = sender as tims_TimeLogModel;
                if (timeLog != null && timeLog.TempItem)
                {
                    SelectedEmployee.TimeLogCollection.Remove(timeLog);
                }
            }
        }
        #endregion

        #region Searches
        /// <summary>
        /// Properties For Searh control
        /// </summary>
        private ObservableCollection<object> _searches;
        public ObservableCollection<object> Searches
        {
            get
            {
                if (_searches == null)
                    LoadSearchList();
                return _searches;
            }
        }
        private void LoadSearchList()
        {
            _searches = new ObservableCollection<object>();
            _searches.Add(
               new FilterItemModel
               {
                   ValueMember = "EmployeeNum",
                   DisplayMember = "Employee #",
                   Type = SearchType.Text,
                   IsDefault = true
               });

            _searches.Add(
               new FilterItemModel
               {
                   ValueMember = "LastName",
                   DisplayMember = "Last Name",
                   Type = SearchType.Text
               });
            _searches.Add(
               new FilterItemModel
               {
                   ValueMember = "FirstName",
                   DisplayMember = "First Name",
                   Type = SearchType.Text
               });
        }
        #endregion

        #region IsAdvanceMode
        private bool _isAdvanceMode;

        public bool IsAdvanceMode
        {
            get { return _isAdvanceMode; }
            set
            {
                _isAdvanceMode = value;
                OnPropertyChanged(() => IsAdvanceMode);
            }
        }
        #endregion

        #region FilterText
        /// <summary>
        /// GroupPermission filter text
        /// </summary>
        private string filterText = string.Empty;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                OnSearchCommandExecute(filterText);
                OnPropertyChanged(() => FilterText);
            }
        }
        #endregion

        #region TotalItem
        /// <summary>
        /// Gets or sets the CountFilter.
        /// </summary>
        public int TotalItem
        {
            get
            {
                if (_collectionView != null)
                    return _collectionView.OfType<base_GuestModel>().Count();
                //else if (EmployeeCollection != null)
                //    return EmployeeCollection.Count();
                return 0;
            }
        }
        #endregion

        #region FromDate
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
                    //FilterAllTimeLog();
                }
            }
        }
        #endregion

        #region ToDate
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
                    //FilterAllTimeLog();
                }
            }
        }
        #endregion

        #region ReadOnlyForm
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
        #endregion

        #region IsShowAll
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

        #region SearchParam
        private object _searchParam;
        /// <summary>
        /// Gets or sets the SearchParam.
        /// </summary>
        public object SearchParam
        {
            get { return _searchParam; }
            set
            {
                if (_searchParam != value)
                {
                    _searchParam = value;
                    OnPropertyChanged(() => SearchParam);
                }
            }
        }
        #endregion

        #endregion

        #region Constructor
        // Default contructor
        public TimeClockManualEventEditingViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            InitialCommand();

            GetCurrentYear();

            InitialData();

        }

        public TimeClockManualEventEditingViewModel(bool isSearchMode)
            : this()
        {
            this.IsSearchMode = isSearchMode;
            if (!IsSearchMode)
            {
                //new item
            }
        }
        #endregion

        #region Command Methods

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
            // TODO: Handle command logic here
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            if (SelectedEmployee == null)
                return false;
            return IsValid && (SelectedEmployee.TimeLogCollection.Has<tims_TimeLogModel>("IsDirty") || (SelectedEmployee.TimeLogCollection.DeletedItems != null && SelectedEmployee.TimeLogCollection.DeletedItems.Count > 0));
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            //Process Save TimeLog & relatives
            SaveTimeLog();
            //Go to Grid Search
            IsSearchMode = true;
        }

        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        //Search
        #region SearchCommand
        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            if (EmployeeCollection == null || (EmployeeCollection != null && EmployeeCollection.Count == 0))
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                if(EmployeeCollection == null || (EmployeeCollection != null && EmployeeCollection.Count == 0))
                    return;

                if (param == null)
                    param = string.Empty;

                if (IsAdvanceMode)
                {
                    // Advance Filter
                    if (param is ObservableCollection<FilterItemModel>)
                    {
                        ObservableCollection<FilterItemModel> collection = (param as ObservableCollection<FilterItemModel>);
                        _collectionView.Filter = (one) =>
                        {
                            bool result = true;
                            foreach (var item in collection)
                            {
                                if (item == null)
                                {
                                    result &= true;
                                    continue;
                                }

                                base_GuestModel employeeModel = one as base_GuestModel;
                                switch (item.ValueMember)
                                {
                                    case "EmployeeNum":
                                        if (employeeModel != null && string.IsNullOrWhiteSpace(employeeModel.base_Guest.GuestNo))
                                            result = false;
                                        else if (item.Value1 != null)
                                            result &= employeeModel.base_Guest.GuestNo.ToLower().Contains(item.Value1.ToString().ToLower());
                                        break;
                                    case "LastName":
                                        if (employeeModel != null && string.IsNullOrWhiteSpace(employeeModel.base_Guest.LastName))
                                            result = false;
                                        else if (item.Value1 != null)
                                            result &= employeeModel.base_Guest.LastName.ToLower().Contains(item.Value1.ToString().ToLower());
                                        break;
                                    case "FirstName":
                                        if (employeeModel != null&& string.IsNullOrWhiteSpace(employeeModel.base_Guest.FirstName))
                                            result = false;
                                        else if (item.Value1 != null)
                                            result &= employeeModel.base_Guest.FirstName.ToLower().Contains(item.Value1.ToString().ToLower());
                                        break;
                                }
                                //if (!this.IsShowAll)
                                //    result &= employeeModel.Employee.TimeLog.Count > 0;
                                if (!this.IsShowAll)
                                {
                                    if (this.FromDate.HasValue && this.ToDate.HasValue)
                                        result &= employeeModel.base_Guest.tims_TimeLog.Count(x => x.ClockIn.Date >= this.FromDate.Value.Date && x.ClockIn.Date <= this.ToDate.Value.Date) > 0;
                                    else
                                        result &= employeeModel.base_Guest.tims_TimeLog.Count() > 0;
                                }
                            }

                            return result;
                        };
                    }
                }
                else // Simple Filter
                {
                    //if (param == null)
                    //    _collectionView.Refresh();
                    if (param != null)
                    {
                        _collectionView.Filter = (one) =>
                        {
                            bool result = false;
                            base_GuestModel employeeModel = one as base_GuestModel;
                            //Fitler for  Employee Number
                            if (employeeModel != null && string.IsNullOrWhiteSpace(employeeModel.base_Guest.GuestNo))
                                result = false;
                            else
                                result |= employeeModel.base_Guest.GuestNo.ToLower().Contains(param.ToString().ToLower());

                            //Fitler for LastName
                            if (employeeModel != null  && string.IsNullOrWhiteSpace(employeeModel.base_Guest.LastName))
                                result = false;
                            else
                                result |= employeeModel.base_Guest.LastName.ToLower().Contains(param.ToString().ToLower());

                            //Fitler for  FirstName
                            if (employeeModel != null && string.IsNullOrWhiteSpace(employeeModel.base_Guest.FirstName))
                                result = false;
                            else
                                result |= employeeModel.base_Guest.FirstName.ToLower().Contains(param.ToString().ToLower());

                            //Fitler for  FromDate && ToDate
                            if (employeeModel != null && employeeModel.base_Guest.tims_WorkPermission == null)
                                result = false;
                            else
                                result |= (employeeModel.base_Guest.tims_WorkPermission.Count(x => x.ToDate.ToString().Contains(param.ToString()) || x.FromDate.ToString().Contains(param.ToString())) > 0);

                            //if (!this.IsShowAll)
                            //    result &= employeeModel.Employee.TimeLog.Count > 0;
                            if (!this.IsShowAll)
                            {
                                if (this.FromDate.HasValue && this.ToDate.HasValue)
                                    result &= employeeModel.base_Guest.tims_TimeLog.Count(x => x.ClockIn.Date >= this.FromDate.Value.Date && x.ClockIn.Date <= this.ToDate.Value.Date) > 0;
                                else
                                    result &= employeeModel.base_Guest.tims_TimeLog.Count() > 0;
                            }
                            return result;
                        };
                    }

                }
                FilterAllTimeLog();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
            OnPropertyChanged(() => TotalItem);
        }
        #endregion

        #region NewTimeLogCommand
        /// <summary>
        /// Method to check whether the NewTimeLogCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewTimeLogCommandCanExecute(object param)
        {
            if (SelectedEmployee == null)
                return false;
            //else if (SelectedEmployee != null && SelectedEmployee.Employee.EmployeeSchedule.Count(x => x.StartDate.Date > DateTimeExt.Today.Date) == 0)
            //    return false;
            var NotClockOutNull= SelectedEmployee.TimeLogCollection.Count()>0 ? SelectedEmployee.TimeLogCollection.Any(x=>!x.IsFixClockOutNull) : true;
            return SelectedEmployee.TimeLogCollection.Count(x => x.Errors.Count > 0) == 0 && NotClockOutNull;
        }

        /// <summary>
        /// Method to invoke when the NewTimeLogCommand command is executed.
        /// Edited : 
        /// 04/12/2012 : Change TimeLog.ClockOut can null
        /// </summary>
        private void OnNewTimeLogCommandExecute(object param)
        {
            try
            {
                tims_TimeLogModel timeLogModel = new tims_TimeLogModel();
                timeLogModel.Editing = true;
                
                timeLogModel.EmployeeId = this.SelectedEmployee.base_Guest.Id;
                timeLogModel.GuestResource = this.SelectedEmployee.Resource.ToString();
                timeLogModel.EmployeeTemp = this.SelectedEmployee;
                timeLogModel.IsManual = true;
                timeLogModel.ManualClockInFlag = true;
                timeLogModel.ClockIn = new DateTime(DateTimeExt.Today.Year, DateTimeExt.Today.Month, DateTimeExt.Today.Day, 9, 0, 0);
                timeLogModel.ClockOut = null;
                timeLogModel.ClockInDate = DateTimeExt.Today;
                SelectedEmployee.TimeLogCollection.Add(timeLogModel);
                timeLogModel.SetDuplicationTimeLog();
                timeLogModel.SetFixItemTimeClockNull();
                timeLogModel.Editing = false;
                
                SelectedTimeLog = timeLogModel;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #region RemoveTimeLog Command
        /// <summary>
        /// Method to check whether the NewTimeLogCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRemoveTimeLogCommandCanExecute(object param)
        {
            if ((this.SelectedEmployee != null && this.SelectedEmployee.TimeLogCollection.Count == 0) || SelectedTimeLog == null)
                return false;
            else if (this.SelectedEmployee.TimeLogCollection.Count(x => !string.IsNullOrWhiteSpace(x.Error)) > 0 && string.IsNullOrWhiteSpace(SelectedTimeLog.Error))
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewTimeLogCommand command is executed.
        /// </summary>
        private void OnRemoveTimeLogCommandExecute(object param)
        {
            try
            {
                var result = ShowMessageBox("Do you want to delete this item", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MessageBoxResult.Yes.Equals(result))
                {
                    var dataGrid = param as DataGrid;
                    var selectItems = dataGrid.SelectedItems.Cast<tims_TimeLogModel>();
                    PropertyInfo inf = dataGrid.GetType().BaseType.GetProperty("HasRowValidationError", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (inf != null)
                    {
                        inf.SetValue(dataGrid, false, null);
                    }
                    SelectedTimeLog.CancelEdit(string.Empty);
                    var raiseTimeLogDuplicate = this.SelectedEmployee.TimeLogCollection.Where(x => x.ClockIn.Date == this.SelectedTimeLog.ClockIn.Date);
                    SelectedEmployee.TimeLogCollection.Remove(SelectedTimeLog);
                    SelectedTimeLog = null;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        //Selection Change Command
        #region Mouse Double Click Command
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool CanMouseDoubleClickExecute(object param)
        {
            if (IsSearchMode && param == null)
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void OnMouseDoubleClickExecuted(object param)
        {
            try
            {
                if (!IsSearchMode && SelectedEmployee.TimeLogCollection.Count(x => x.IsNew) > 0) // Is a New Item
                {
                    NewItemCheck();
                }
                else if (!IsSearchMode && (SelectedEmployee.TimeLogCollection.Count(x => x.IsDirty) > 0 || (SelectedEmployee.TimeLogCollection.DeletedItems != null && SelectedEmployee.TimeLogCollection.DeletedItems.Count > 0))) // Is Old Item
                {
                    OldItemCheck();
                }
                else
                {
                    IsSearchMode = !IsSearchMode;
                }

                //if (IsSearchMode)
                //{
                //    ICollectionView timeLogCollectionView = CollectionViewSource.GetDefaultView(SelectedEmployee.TimeLogCollection);
                //     if (timeLogCollectionView != null)
                //     {
                //         //Avoid error "'DeferRefresh' is not allowed during an AddNew or EditItem transaction."
                //         IEditableCollectionView collection = timeLogCollectionView as IEditableCollectionView;
                //         if (collection.IsEditingItem)
                //             collection.CommitEdit();
                //     }
                //}


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

        #region TimeLog Selection Changed
        /// <summary>
        ///  Employee Selection Changed
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool OnTimeLogSelectionChangedCanExecuted(object param)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void OnTimeLogSelectionChangedExecuted(object param)
        {
            try
            {
                SelectedTimeLog = param as tims_TimeLogModel;
                if (this.SelectedTimeLog != null)
                {
                    //if (SelectedTimeLog.TimeLogList == null)
                    //    SelectedTimeLog.TimeLogList = new System.Collections.Generic.List<TimeLogModel>();
                    //SelectedTimeLog.TimeLogList.Clear();
                    //SelectedTimeLog.TimeLogList = new System.Collections.Generic.List<TimeLogModel>(SelectedEmployee.TimeLogCollection.ToList());
                    this.SelectedTimeLog.EmployeeTemp = SelectedEmployee;
                    this.SelectedTimeLog.SetWorkPermissionOT();
                    this.SelectedTimeLog.SetOverTimeFromEmployee();
                }
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
        /// Initital Command
        /// <remarks>
        /// 10/05/2012 
        ///   Edit Get All Employee no follow with  Staus.Employees is Actived can Edit TimeLog
        /// </remarks>
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);

            NewTimeLogCommand = new RelayCommand<object>(OnNewTimeLogCommandExecute, OnNewTimeLogCommandCanExecute);
            RemoveTimeLogCommand = new RelayCommand<object>(OnRemoveTimeLogCommandExecute, OnRemoveTimeLogCommandCanExecute);

            //Selection Changed Command
            EmployeeSelectionChangedCommand = new RelayCommand<object>(OnEmployeeSelectionChangedExecuted, OnEmployeeSelectionChangedCanExecuted);
            TimeLogSelectionChangedCommand = new RelayCommand<object>(OnTimeLogSelectionChangedExecuted, OnTimeLogSelectionChangedCanExecuted);

            //Double Click Command
            MouseDoubleClickCommand = new RelayCommand<object>(OnMouseDoubleClickExecuted, this.CanMouseDoubleClickExecute);
        }

        private void InitialData()
        {
            try
            {
                RefreshAll();
                string employeeMark = MarkType.Employee.ToDescription();
                //Get All Employee 
                var all = _employeeRepository.GetAll(x => !x.IsPurged && x.Mark.Equals(employeeMark));

                EmployeeCollection = new ObservableCollection<base_GuestModel>(all.Select(x => new base_GuestModel(x)).OrderBy(x => x.Id));
                _collectionView = CollectionViewSource.GetDefaultView(EmployeeCollection);
                tims_HolidayHistoryRepository holidayRepository = new tims_HolidayHistoryRepository();
                this.HolidayHistoryCollection = holidayRepository.GetAll().ToList();
                FilterAllTimeLog();
                OnPropertyChanged(() => TotalItem);
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
                //Delete some time log 
                if (this.SelectedEmployee.TimeLogCollection.DeletedItems != null)
                {
                    foreach (var timeLogModel in this.SelectedEmployee.TimeLogCollection.DeletedItems)
                    {
                        tims_TimeLogRepository timeLogRep = new tims_TimeLogRepository();
                        this.SelectedEmployee.TimeLogCollection.Remove(timeLogModel);
                        timeLogRep.Delete(timeLogModel.tims_TimeLog);
                        timeLogRep.Commit();
                        var deletedItemRelation = this.SelectedEmployee.TimeLogCollection.Where(x => x.ClockIn.Date == timeLogModel.ClockIn.Date);
                        foreach (var relationWithDeleted in deletedItemRelation)
                        {
                            relationWithDeleted.IsDirty = true;
                        }
                    }
                    this.SelectedEmployee.TimeLogCollection.DeletedItems.Clear();
                }

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
                            relationWithDirty.ModifiedDate = DateTimeExt.Now;
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
                                if (collection.IsEditingItem )
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
        /// Filter TimeLog with current item
        /// </summary>
        private void FilterTimeLog(base_GuestModel employeeModel)
        {
            try
            {
                ICollectionView collectionView = CollectionViewSource.GetDefaultView(employeeModel.TimeLogCollection);
                if (collectionView.GroupDescriptions.Count == 0)
                    collectionView.GroupDescriptions.Add(new PropertyGroupDescription("WorkScheduleGroup"));
                if (collectionView != null)
                {
                    collectionView.Filter = (one) =>
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
                    employeeModel.TotalTimeLog = collectionView.OfType<tims_TimeLogModel>().Count();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Get Current Year for filter 
        /// </summary>
        private void GetCurrentYear()
        {
            FromDate = new DateTime(DateTimeExt.Now.Year, 1, 1, 0, 0, 0, 0);
            ToDate = new DateTime(DateTimeExt.Now.Year, 12, DateTime.DaysInMonth(DateTimeExt.Now.Year, 12), 23, 59, 59, 999);
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
            if ((workPermission != null &&timeLogModel.ClockOut.HasValue && !timeLogModel.tims_TimeLog.tims_WorkPermission.Any(x => x == workPermission)))
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
                if ((HolidayHistoryCollection.Count(x => timeLogModel.ClockIn.Date == x.Date.Date) > 0 || timeLogModel.WorkWeek == null) && timeLogModel.ClockOut.HasValue)
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
            if (HolidayHistoryCollection.Count(x => timeLogModel.ClockIn.Date == x.Date.Date) > 0 || timeLogModel.WorkWeek == null)
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

        public override void LoadData()
        {
          
        }
        #endregion
    }
}