using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CPC.Helper;
using CPC.TimeClock;
using CPC.TimeClock.Database;
using CPC.TimeClock.Model;
using CPC.TimeClock.Properties;
using CPC.TimeClock.Repository;
using CPC.TimeClock.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.Utility;

namespace CPC.ViewModel
{
    class MainViewModel : ViewModelBase, DPFP.Capture.EventHandler
    {
        #region Define

        private MainWindow _view;
        private LoadingView _loadingView = new LoadingView();
        private BackgroundWorker _loadingWorker;
        private WarningView _warningView = new WarningView();
        private int _warningTime = 5;
        private bool _runWarning;

        private Timer _timer = new Timer(1000);
        private Timer _displayTimer = new Timer(5000);
        private DispatcherTimer _minimizeTimer = new DispatcherTimer();
        private DateTime _currentRegisterTime;
        private bool _isLoading;

        // Commands
        public ICommand OpenCommand { get; private set; }
        public ICommand MinimizeCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        private base_GuestFingerPrintRepository _employeeFingerprintRepository = new base_GuestFingerPrintRepository();
        private tims_TimeLogRepository _timeLogRepository = new tims_TimeLogRepository();

        // Fingerprint fields
        private DPFP.Sample _sample;
        private DPFP.Capture.Capture _capturer;
        private DPFP.Verification.Verification _verificator;

        // Fancy ballon
        private FancyBalloon _fancyBalloon;

        /// <summary>
        /// Store day of work week
        /// </summary>
        private Dictionary<string, tims_WorkWeekModel> _dayOfWorkWeekDictionary;

        private bool _isUseFingerprint;

        // Enum
        private enum FingerprintStatus
        {
            None,
            Touch,
            Successful,
            Failed,
            Connected,
            Disconnect
        }

        #endregion

        #region Properties

        private ObservableCollection<base_GuestFingerPrintModel> _employeeFingerprintCollection;
        /// <summary>
        /// Gets or sets the EmployeeFingerprintCollection.
        /// </summary>
        public ObservableCollection<base_GuestFingerPrintModel> EmployeeFingerprintCollection
        {
            get { return _employeeFingerprintCollection; }
            set
            {
                if (_employeeFingerprintCollection != value)
                {
                    _employeeFingerprintCollection = value;
                    OnPropertyChanged(() => EmployeeFingerprintCollection);
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
                    OnPropertyChanged(() => SelectedEmployee);
                }
            }
        }

        /// <summary>
        /// Gets the current date
        /// </summary>
        public string CurrentDate
        {
            get
            {
                return DateTimeExt.Today.ToString("dddd, MMM d, yyyy");
            }
        }

        /// <summary>
        /// Gets the current time
        /// </summary>
        public DateTime CurrentTime
        {
            get
            {
                DateTime localTime = DateTimeExt.Now;
                //if (localTime.TimeOfDay == defineApp.ReloadTime)
                //if (localTime.Hour == define.ReloadTime.Hours &&
                //    localTime.Minute == define.ReloadTime.Minutes &&
                //    localTime.Second == define.ReloadTime.Seconds)
                //    RefreshDatas();
                return localTime;
            }
        }

        private bool _isClockIn;
        /// <summary>
        /// Gets or sets the IsClockIn.
        /// </summary>
        public bool IsClockIn
        {
            get { return _isClockIn; }
            set
            {
                if (_isClockIn != value)
                {
                    _isClockIn = value;
                    OnPropertyChanged(() => IsClockIn);
                }
            }
        }

        private bool _isClockOut;
        /// <summary>
        /// Gets or sets the IsClockOut.
        /// </summary>
        public bool IsClockOut
        {
            get { return _isClockOut; }
            set
            {
                if (_isClockOut != value)
                {
                    _isClockOut = value;
                    OnPropertyChanged(() => IsClockOut);
                }
            }
        }

        private string _barCode;
        /// <summary>
        /// Gets or sets the BarCode.
        /// </summary>
        public string BarCode
        {
            get { return _barCode; }
            set
            {
                if (_barCode != value)
                {
                    _barCode = value;
                    OnPropertyChanged(() => BarCode);
                    _isUseFingerprint = false;
                    DoWork();
                }
            }
        }

        /// <summary>
        /// Gets the CompanyName.
        /// </summary>
        public string CompanyName
        {
            get
            {
                if (define.CONFIGURATION == null)
                    return string.Empty;
                return define.CONFIGURATION.CompanyName;
            }
        }

        /// <summary>
        /// Gets the Website.
        /// </summary>
        public string Website
        {
            get
            {
                if (define.CONFIGURATION == null)
                    return string.Empty;
                return define.CONFIGURATION.Website;
            }
        }

        #endregion

        #region Constructor

        // Default constructor
        public MainViewModel(MainWindow view)
        {
            try
            {
                _ownerViewModel = this;

                // Route the commands
                OpenCommand = new RelayCommand(OnOpenCommandExecute, OnOpenCommandCanExecute);
                MinimizeCommand = new RelayCommand(OnMinimizeCommandExecute);
                ExitCommand = new RelayCommand(OnExitCommandExecute);

                _view = view;
                _view.Closed += new EventHandler(_view_Closed);
                _view.Loaded += new RoutedEventHandler(_view_Loaded);
                _view.StateChanged += new EventHandler(_view_StateChanged);

                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\DigitalPersona");
                if (registryKey != null)
                    _verificator = new DPFP.Verification.Verification();
                else
                    _log4net.Warn("Fingerprint device is not already!");

                // Initial timers
                _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
                _timer.Start();
                _displayTimer.Elapsed += new ElapsedEventHandler(_displayTimer_Elapsed);
                _minimizeTimer.Interval = define.IdleTime;
                _minimizeTimer.Tick += new EventHandler(_minimizeTimer_Tick);

                _loadingWorker = new BackgroundWorker();
                _loadingWorker.DoWork += new DoWorkEventHandler(_loadingWorker_DoWork);
                _loadingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_loadingWorker_RunWorkerCompleted);

            }
            catch (Exception ex)
            {
                _log4net.Error(ex.ToString());
            }
            //LoadDatas();

            // Test data
            //MinimumBarCodeLength = 13;
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Method to check whether the OpenCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenCommandCanExecute()
        {
            return _view != null && _view.WindowState == WindowState.Minimized;
        }

        /// <summary>
        /// Method to invoke when the OpenCommand command is executed.
        /// </summary>
        private void OnOpenCommandExecute()
        {
            _view.Activate();
            if (_view.WindowState == WindowState.Minimized)
                _view.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Method to invoke when the MinimizeCommand command is executed.
        /// </summary>
        private void OnMinimizeCommandExecute()
        {
            if (_view.WindowState != WindowState.Minimized)
                _view.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Method to invoke when the ExitCommand command is executed.
        /// </summary>
        private void OnExitCommandExecute()
        {
            if (_dialogService.ShowMessageBox(_ownerViewModel, "Do you want to exit?", "Tims TimeClock",
                MessageBoxButton.YesNo, MessageBoxImage.Question).Is(MessageBoxResult.Yes))
                _view.Close();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update timelog permission table
        /// </summary>
        /// <param name="workPermissionCollection"></param>
        /// <param name="timeLogModel"></param>
        private void UpdateTimeLogPermission(IEnumerable<tims_WorkPermissionModel> workPermissionCollection, tims_TimeLogModel timeLogModel)
        {
            foreach (var workPermissionModel in workPermissionCollection)
                timeLogModel.tims_TimeLog.tims_WorkPermission.Add(workPermissionModel.tims_WorkPermission);
        }

        /// <summary>
        /// Check block register if timelog not completed
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <returns></returns>
        private bool IsBlockRegisterIfNotCompleted(ref base_GuestModel employeeModel)
        {
            if (define.blockRegisterIfNotCompleted)
            {
                ShowMessageWarning("There is a previous timelog don't have Clock-Out");
                _log4net.Info("There is a previous timelog don't have Clock-Out");
                employeeModel = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check is late register
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <param name="workWeekModel"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private bool IsLateRegister(ref base_GuestModel employeeModel, tims_WorkWeekModel workWeekModel)
        {
            if (employeeModel != null && employeeModel.IsBlockArriveLate && workWeekModel != null &&
                _currentRegisterTime > workWeekModel.WorkIn.AddMinutes(employeeModel.LateMinutes))
            {
                ShowMessageWarning("This user is late register");
                _log4net.Info("This user is late register");
                employeeModel = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check today is holiday or day off
        /// </summary>
        /// <param name="workWeekModel"></param>
        /// <returns></returns>
        private bool IsHolidayOrDayOff(tims_WorkWeekModel workWeekModel)
        {
            return workWeekModel == null;
        }

        /// <summary>
        /// Get overtime remaining
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
        /// Get valid overtime value if have work permission
        /// </summary>
        /// <param name="overtimeValue"></param>
        /// <param name="workPermissionDictionary"></param>
        /// <param name="timeLogModel"></param>
        /// <param name="employeeModel"></param>
        /// <param name="workPermissionCollection"></param>
        /// <param name="overtime"></param>
        /// <returns></returns>
        private float GetOvertimeValid(float overtimeValue, IDictionary<Overtime, float> sumOvertimeDictionary,
            tims_TimeLogModel timeLogModel, base_GuestModel employeeModel,
            IEnumerable<tims_WorkPermissionModel> workPermissionCollection, Overtime overtime)
        {
            // Declare overtime valid variable
            float overtimeValid = 0;

            // Declare overtime elapsed variable
            float overtimeElapsed = sumOvertimeDictionary[overtime];

            // Get work permission have over time option equal param
            tims_WorkPermissionModel workPermissionModel = workPermissionCollection.FirstOrDefault(x => overtime.In(x.OvertimeOptions));

            // Check work permission
            if (workPermissionModel != null)
            {
                if (overtimeValue > 0)
                {
                    // Get hour per day in work permission
                    float hourPerDay = (float)TimeSpan.FromHours(workPermissionModel.HourPerDay).TotalMinutes;

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
                            if (overtimeElapsed + sumOvertimeDictionary[Overtime.Before] == 0)
                                overtimeValid = Math.Min(hourPerDay, overtimeValue);
                            else
                            {
                                if (Overtime.Before.In(workPermissionModel.OvertimeOptions))
                                    overtimeElapsed += sumOvertimeDictionary[Overtime.Before];
                                //overtimeRemaining = GetOvertimeRemaining(hourPerDay, sumOvertimeDictionary[Overtime.Before]);
                                overtimeRemaining = GetOvertimeRemaining(hourPerDay, overtimeElapsed);
                                overtimeValid = Math.Min(overtimeRemaining, overtimeValue);
                            }
                            break;
                        case Overtime.After:
                            if (overtimeElapsed + sumOvertimeDictionary[Overtime.Before] + sumOvertimeDictionary[Overtime.Break] == 0)
                                overtimeValid = Math.Min(hourPerDay, overtimeValue);
                            else
                            {
                                if (Overtime.Before.In(workPermissionModel.OvertimeOptions))
                                    overtimeElapsed += sumOvertimeDictionary[Overtime.Before];
                                //overtimeRemaining = GetOvertimeRemaining(hourPerDay, sumOvertimeDictionary[Overtime.Before]);
                                if (Overtime.Break.In(workPermissionModel.OvertimeOptions))
                                    overtimeElapsed += sumOvertimeDictionary[Overtime.Break];
                                //overtimeRemaining = GetOvertimeRemaining(hourPerDay, sumOvertimeDictionary[Overtime.Before] + sumOvertimeDictionary[Overtime.Break]);
                                overtimeRemaining = GetOvertimeRemaining(hourPerDay, overtimeElapsed);
                                overtimeValid = Math.Min(overtimeRemaining, overtimeValue);
                            }
                            break;
                    }
                }

                // Update timelog permission table
                timeLogModel.tims_TimeLog.tims_WorkPermission.Add(workPermissionModel.tims_WorkPermission);

                // Update overtime options
                timeLogModel.OvertimeOptions = (int)((Overtime)timeLogModel.OvertimeOptions).Add(overtime);
            }
            else if (overtime.In(employeeModel.OvertimeOption))
            {
                overtimeValid = overtimeValue;

                // Update overtime options
                timeLogModel.OvertimeOptions = (int)((Overtime)timeLogModel.OvertimeOptions).Add(overtime);
            }

            return overtimeValid;
        }

        /// <summary>
        /// Update over time for timelog
        /// Insert to timelog permission if employee have permission
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        /// <param name="employeeModel"></param>
        private void UpdateOverTime(tims_TimeLogModel timeLogModel, tims_WorkWeekModel dayOfWorkWeekModel, base_GuestModel employeeModel, DateTime beginOfDay)
        {
            // Get work permission collecion
            var workPermissionCollection = employeeModel.WorkPermissionCollection.Where(
                x => x.FromDate <= _currentRegisterTime.Date && _currentRegisterTime.Date <= x.ToDate);

            // Get work permission ArrivingLate and LeavingEarly
            var permissionCollection = workPermissionCollection.Where(
                x => x.PermissionType.Has(WorkPermissionTypes.ArrivingLate) || x.PermissionType.Has(WorkPermissionTypes.LeavingEarly));
            if (permissionCollection.Count() > 0)
            {
                foreach (var workPermissionModel in permissionCollection)
                    timeLogModel.tims_TimeLog.tims_WorkPermission.Add(workPermissionModel.tims_WorkPermission);
            }

            // Get previous timelogs to calc overtime elapsed
            var timeLogGroup = (from x in _timeLogRepository.GetAll(x => x.ClockOut.HasValue &&
                x.EmployeeId == timeLogModel.EmployeeId && x.WorkScheduleId == timeLogModel.WorkScheduleId)
                                where beginOfDay < x.ClockIn && x.ClockOut.Value < _currentRegisterTime
                                group x by new { x.EmployeeId, x.WorkScheduleId } into g
                                select new
                                {
                                    SumOvertimeBefore = g.Sum(x => x.OvertimeBefore),
                                    SumOvertimeLunch = g.Sum(x => x.OvertimeLunch),
                                    SumOvertimeAfter = g.Sum(x => x.OvertimeAfter),
                                    SumOvertimeDayOff = g.Sum(x => x.OvertimeDayOff)
                                }).FirstOrDefault();

            // Create default sum overtime dictionary
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

            float overtimeValue = 0;

            // Create clock in - clock out date range
            DateRange clockInOut = new DateRange(timeLogModel.ClockIn, timeLogModel.ClockOut);

            // Check today is holiday or day off
            if (IsHolidayOrDayOff(dayOfWorkWeekModel))
            {
                // Get overtime day off value
                overtimeValue = (float)clockInOut.TimeSpan.TotalMinutes;

                // Update over time day off
                timeLogModel.OvertimeDayOff = GetOvertimeValid(overtimeValue, sumOvertimeDictionary,
                    timeLogModel, employeeModel, workPermissionCollection, Overtime.Holiday);
            }
            else
            {
                IDictionary<int, float> workPermissionDictionary = new Dictionary<int, float>();

                // Create before - work in date range
                DateRange beforeWorkIn = new DateRange(dayOfWorkWeekModel.WorkOut.AddDays(-1).AddHours(define.hours), dayOfWorkWeekModel.WorkIn);

                // Get overtime before value
                if (clockInOut.Intersects(beforeWorkIn))
                    overtimeValue = (float)clockInOut.GetIntersection(beforeWorkIn).TimeSpan.TotalMinutes;

                // Update over time before work time
                timeLogModel.OvertimeBefore = GetOvertimeValid(overtimeValue, sumOvertimeDictionary,
                    timeLogModel, employeeModel, workPermissionCollection, Overtime.Before);

                // Update sum overtime before
                sumOvertimeDictionary[Overtime.Before] += timeLogModel.OvertimeBefore;

                if (dayOfWorkWeekModel.LunchBreakFlag)
                {
                    // Create lunch out - lunch in date range
                    DateRange lunchOutIn = new DateRange(dayOfWorkWeekModel.LunchOut, dayOfWorkWeekModel.LunchIn);

                    // Get overtime lunch value
                    if (clockInOut.Intersects(lunchOutIn))
                        overtimeValue = (float)clockInOut.GetIntersection(lunchOutIn).TimeSpan.TotalMinutes;
                    else
                        overtimeValue = 0;

                    // Update over time lunch break
                    if (!timeLogModel.DeductLunchTimeFlag)
                        timeLogModel.OvertimeLunch = GetOvertimeValid(overtimeValue, sumOvertimeDictionary,
                            timeLogModel, employeeModel, workPermissionCollection, Overtime.Break);

                    // Update sum overtime Break
                    sumOvertimeDictionary[Overtime.Break] += timeLogModel.OvertimeLunch;
                }

                // Create after - work out date range
                DateRange afterWorkOut = new DateRange(dayOfWorkWeekModel.WorkOut, dayOfWorkWeekModel.WorkOut.AddHours(define.hours));

                // Get overtime after value
                if (clockInOut.Intersects(afterWorkOut))
                    overtimeValue = (float)clockInOut.GetIntersection(afterWorkOut).TimeSpan.TotalMinutes;
                else
                    overtimeValue = 0;

                // Update over time after work time
                timeLogModel.OvertimeAfter = GetOvertimeValid(overtimeValue, sumOvertimeDictionary,
                    timeLogModel, employeeModel, workPermissionCollection, Overtime.After);
            }
        }

        /// <summary>
        /// Update work time for timelog
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        private void UpdateWorkTime(tims_TimeLogModel timeLogModel, tims_WorkWeekModel dayOfWorkWeekModel)
        {
            // If today is holiday, have not work time
            if (!IsHolidayOrDayOff(dayOfWorkWeekModel))
            {
                // Create clock in - clock out date range
                DateRange clockInOut = new DateRange(timeLogModel.ClockIn, timeLogModel.ClockOut);

                // Create work in - work out date range
                DateRange workInOut = new DateRange(dayOfWorkWeekModel.WorkIn, dayOfWorkWeekModel.WorkOut);

                // Update work time
                if (dayOfWorkWeekModel.LunchBreakFlag)
                {
                    // Create work in - lunch out date range
                    DateRange workInLunchOut = new DateRange(dayOfWorkWeekModel.WorkIn, dayOfWorkWeekModel.LunchOut);

                    // Create lunch in - work out date range
                    DateRange lunchInWorkOut = new DateRange(dayOfWorkWeekModel.LunchIn, dayOfWorkWeekModel.WorkOut);

                    // Get work time before lunch break
                    if (clockInOut.Intersects(workInLunchOut))
                        timeLogModel.WorkTime = (float)clockInOut.GetIntersection(workInLunchOut).TimeSpan.TotalMinutes;

                    // Get work time after lunch break
                    if (clockInOut.Intersects(lunchInWorkOut))
                        timeLogModel.WorkTime += (float)clockInOut.GetIntersection(lunchInWorkOut).TimeSpan.TotalMinutes;

                }
                else if (clockInOut.Intersects(workInOut))
                    timeLogModel.WorkTime = (float)clockInOut.GetIntersection(workInOut).TimeSpan.TotalMinutes;
            }
        }

        /// <summary>
        /// Update lunch time for timelog
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        private void UpdateLunchTime(tims_TimeLogModel timeLogModel, tims_WorkWeekModel dayOfWorkWeekModel)
        {
            if (!IsHolidayOrDayOff(dayOfWorkWeekModel))
                if (dayOfWorkWeekModel.LunchBreakFlag)
                {
                    // Create clock in - clock out date range
                    DateRange clockInOut = new DateRange(timeLogModel.ClockIn, timeLogModel.ClockOut);

                    // Create lunch out - lunch in date range
                    DateRange lunchOutIn = new DateRange(dayOfWorkWeekModel.LunchOut, dayOfWorkWeekModel.LunchIn);

                    if (clockInOut.Intersects(lunchOutIn))
                        timeLogModel.LunchTime = (float)clockInOut.GetIntersection(lunchOutIn).TimeSpan.TotalMinutes;
                }
        }

        /// <summary>
        /// Update early time for timelog
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        private void UpdateEarlyTime(tims_TimeLogModel timeLogModel, tims_WorkWeekModel dayOfWorkWeekModel)
        {
            // If today is holiday, have not early time
            if (!IsHolidayOrDayOff(dayOfWorkWeekModel) && dayOfWorkWeekModel.WorkOut > timeLogModel.ClockOut.Value)
                timeLogModel.LeaveEarlyTime = (float)(dayOfWorkWeekModel.WorkOut - timeLogModel.ClockOut.Value).TotalMinutes;
        }

        /// <summary>
        /// Update late time for timelog
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        private void UpdateLateTime(tims_TimeLogModel timeLogModel, tims_WorkWeekModel dayOfWorkWeekModel)
        {
            // If today is holiday, have not late time
            if (!IsHolidayOrDayOff(dayOfWorkWeekModel) && timeLogModel.ClockIn > dayOfWorkWeekModel.WorkIn)
                timeLogModel.LateTime = (float)(timeLogModel.ClockIn - dayOfWorkWeekModel.WorkIn).TotalMinutes;
        }

        /// <summary>
        /// Get work week by datetime and employee
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private tims_WorkWeekModel GetDayOfWorkWeek(base_GuestModel employeeModel, DateTime dateTime)
        {
            tims_WorkWeekModel dayOfWorkWeekModel = null;

            // Get work schedule of this employee
            base_GuestScheduleModel employeeScheduleModel = employeeModel.EmployeeScheduleCollection.LastOrDefault(x => x.StartDate.Date <= dateTime.Date);
            if (employeeScheduleModel != null)
            {
                employeeModel.EmployeeScheduleModel = employeeScheduleModel;
                if (employeeModel.EmployeeScheduleModel.WorkScheduleModel.WorkScheduleType < 3)
                    dayOfWorkWeekModel = employeeModel.EmployeeScheduleModel.WorkScheduleModel.WorkWeekCollection.FirstOrDefault(
                        x => (DayOfWeek)(x.Day % 7) == dateTime.Date.DayOfWeek);
                else
                {
                    int dayOfWeek = (int)employeeModel.EmployeeScheduleModel.StartDate.DayOfWeek;
                    int weeks = (dateTime - employeeModel.EmployeeScheduleModel.StartDate.AddDays(-dayOfWeek)).Days / 7;
                    int index = weeks % employeeModel.EmployeeScheduleModel.WorkScheduleModel.Rotate;
                    dayOfWorkWeekModel = employeeModel.EmployeeScheduleModel.WorkScheduleModel.WorkWeekCollection.FirstOrDefault(
                        x => x.Week == index + 1 && (DayOfWeek)(x.Day % 7) == dateTime.Date.DayOfWeek);
                }

                if (dayOfWorkWeekModel != null)
                {
                    // Get delta day between today and work in
                    int deltaDay = (dateTime.Date - dayOfWorkWeekModel.WorkIn.Date).Days;

                    // Update work week day by today
                    dayOfWorkWeekModel.WorkIn = dayOfWorkWeekModel.WorkIn.AddDays(deltaDay);
                    dayOfWorkWeekModel.LunchOut = dayOfWorkWeekModel.LunchOut.Value.AddDays(deltaDay);
                    dayOfWorkWeekModel.LunchIn = dayOfWorkWeekModel.LunchIn.Value.AddDays(deltaDay);
                    dayOfWorkWeekModel.WorkOut = dayOfWorkWeekModel.WorkOut.AddDays(deltaDay);
                }
            }
            else if (dateTime.Date == DateTimeExt.Today)
            {
                ShowMessageWarning("This employee have not assigned work schedule");
                _log4net.Info("This employee have not assigned work schedule");
            }

            return dayOfWorkWeekModel;
        }

        /// <summary>
        /// Process timelog when clock out
        /// </summary>
        /// <param name="timeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        /// <param name="employeeModel"></param>
        private void ProcessClockOut(tims_TimeLogModel timeLogModel, tims_WorkWeekModel dayOfWorkWeekModel, base_GuestModel employeeModel, DateTime beginOfDay)
        {
            // Update clock out
            timeLogModel.ClockOut = _currentRegisterTime;

            // Update early time
            UpdateEarlyTime(timeLogModel, dayOfWorkWeekModel);

            // Update lunch time
            UpdateLunchTime(timeLogModel, dayOfWorkWeekModel);

            // Update work time
            UpdateWorkTime(timeLogModel, dayOfWorkWeekModel);

            // Update over time
            UpdateOverTime(timeLogModel, dayOfWorkWeekModel, employeeModel, beginOfDay);

            // Map model to entity
            timeLogModel.ToEntity();

            // Accept changes
            _timeLogRepository.Commit();

            // Change Clock-In-Out status
            IsClockIn = false;
            IsClockOut = true;
        }

        /// <summary>
        /// Process timelog when clock in
        /// </summary>
        /// <param name="firstTimeLogModel"></param>
        /// <param name="dayOfWorkWeekModel"></param>
        /// <param name="employeeModel"></param>
        private void ProcessClockIn(tims_TimeLogModel firstTimeLogModel, tims_WorkWeekModel dayOfWorkWeekModel, base_GuestModel employeeModel)
        {
            // Create a new timelog
            tims_TimeLogModel timeLogModel = new tims_TimeLogModel();
            timeLogModel.GuestResource = employeeModel.Resource.ToString();
            timeLogModel.EmployeeId = employeeModel.Id;

            if (employeeModel.EmployeeScheduleModel != null)
                // Update work schedule ID
                timeLogModel.WorkScheduleId = employeeModel.EmployeeScheduleModel.WorkScheduleId;

            // Update deduct lunch time flag
            timeLogModel.DeductLunchTimeFlag = employeeModel.IsDeductLunchTime;

            // Update clock in
            timeLogModel.ClockIn = _currentRegisterTime;

            if (employeeModel.TimeLogCollection.Count == 0)
                // Update LateTime
                UpdateLateTime(timeLogModel, dayOfWorkWeekModel);
            else if (firstTimeLogModel != null)
            {
                // Update EarlyTime
                firstTimeLogModel.LeaveEarlyTime = null;

                // Map model to entity
                firstTimeLogModel.ToEntity();
            }

            // Map model to entity
            timeLogModel.ToEntity();

            // Add a new timelog
            _timeLogRepository.Add(timeLogModel.tims_TimeLog);

            // Update employee schedule status
            if (employeeModel.base_Guest.tims_TimeLog.Count > 0)
            {
                if (employeeModel.EmployeeScheduleModel != null)
                {
                    // Get previous employee schedule
                    var previousEmployeeSchedule = employeeModel.EmployeeScheduleCollection.
                        OrderBy(x => x.StartDate).ThenBy(x => x.AssignDate).
                        LastOrDefault(x => !x.Status.Is(EmployeeScheduleStatuses.Inactive) &&
                        x.StartDate < employeeModel.EmployeeScheduleModel.StartDate);

                    // Update status for previous employee schedule
                    if (previousEmployeeSchedule != null)
                        previousEmployeeSchedule.base_GuestSchedule.Status = (int)EmployeeScheduleStatuses.Inactive;

                    if (employeeModel.EmployeeScheduleModel.Status.Is(EmployeeScheduleStatuses.Pending))
                    {
                        // Update employee schedule status is Active
                        employeeModel.EmployeeScheduleModel.Status = (int)EmployeeScheduleStatuses.Active;

                        // Map model to entity
                        employeeModel.EmployeeScheduleModel.ToEntity();
                    }

                    // Update work schedule status is Active
                    var workSchedule = employeeModel.EmployeeScheduleModel.WorkScheduleModel.tims_WorkSchedule;
                    if (workSchedule.Status.Is(ScheduleStatuses.Pending))
                        workSchedule.Status = (int)ScheduleStatuses.Active;
                }
            }

            // Accept changes
            _timeLogRepository.Commit();

            // Update timelog ID
            timeLogModel.Id = timeLogModel.tims_TimeLog.Id;

            // Change Clock-In-Out status
            IsClockOut = false;
            IsClockIn = true;

            // Insert into timelog collection
            _view.Dispatcher.Invoke((Action)delegate { employeeModel.TimeLogCollection.Insert(0, timeLogModel); });
        }

        /// <summary>
        /// Handle employee and relative task
        /// </summary>
        /// <param name="employeeModel"></param>
        /// <returns></returns>
        private base_GuestModel ProcessEmployee(base_GuestModel employeeModel)
        {
            // Get current register time
            _currentRegisterTime = DateTimeExt.Now.Round(DateTimeExt.RoundTo.Minute);

            tims_WorkWeekModel dayOfWorkWeekModel = null;

            // Create work week key
            string dayOfWorkWeekKey = _currentRegisterTime.Date.ToString() + employeeModel.Id;

            if (_dayOfWorkWeekDictionary.ContainsKey(dayOfWorkWeekKey))
                // Get day of work week model in dictionary
                dayOfWorkWeekModel = _dayOfWorkWeekDictionary.FirstOrDefault(x => x.Key == dayOfWorkWeekKey).Value;
            else
            {
                // Get new day of work week in database
                dayOfWorkWeekModel = GetDayOfWorkWeek(employeeModel, _currentRegisterTime.Date);

                // Add new work week to dictionary
                if (dayOfWorkWeekModel != null)
                    _dayOfWorkWeekDictionary.Add(dayOfWorkWeekKey, dayOfWorkWeekModel);
            }

            // Get the first timelog in today
            var firstTimeLogModel = employeeModel.TimeLogCollection.FirstOrDefault();
            // If there isn't timelog in today, get timelog in previous day
            if (firstTimeLogModel == null)
            {
                // Get work schedule ID
                int workScheduleID = employeeModel.EmployeeScheduleModel.WorkScheduleId;

                string employeeResource = employeeModel.Resource.ToString();

                // Get previous timelogs that it's not completed
                var timeLogNotCompleteds = _timeLogRepository.GetAll(x => x.GuestResource == employeeResource &&
                                            x.WorkScheduleId == workScheduleID && !x.ClockOut.HasValue).
                                            Where(x => x.ClockIn < _currentRegisterTime);

                if (timeLogNotCompleteds.Count() > 0)
                    // Get previous time log that clock out is null
                    firstTimeLogModel = new tims_TimeLogModel(timeLogNotCompleteds.LastOrDefault());
            }

            if (firstTimeLogModel != null && firstTimeLogModel.ClockOut == null) // Clock Out
            {
                tims_WorkWeekModel clockInDayOfWorkWeekModel = GetDayOfWorkWeek(employeeModel, firstTimeLogModel.ClockIn.Date);
                DateTime beginOfDay = firstTimeLogModel.ClockIn.Date.AddDays(1);

                if (!IsHolidayOrDayOff(clockInDayOfWorkWeekModel))
                {
                    // Check time register with work out
                    if (_currentRegisterTime <= clockInDayOfWorkWeekModel.WorkOut.AddHours(define.hours))
                    {
                        // Get previous day of work week
                        tims_WorkWeekModel previousDayOfWorkWeekModel = GetDayOfWorkWeek(employeeModel, clockInDayOfWorkWeekModel.WorkIn.AddDays(-1));

                        // Get begin of day
                        if (!IsHolidayOrDayOff(previousDayOfWorkWeekModel))
                            beginOfDay = previousDayOfWorkWeekModel.WorkOut.AddHours(define.hours);

                        // If validate time, allow clock out
                        ProcessClockOut(firstTimeLogModel, clockInDayOfWorkWeekModel, employeeModel, beginOfDay);
                    }
                    else if (!IsBlockRegisterIfNotCompleted(ref employeeModel) && !IsLateRegister(ref employeeModel, dayOfWorkWeekModel))
                    {
                        ProcessClockIn(null, dayOfWorkWeekModel, employeeModel);
                    }
                }
                else
                {
                    // Check time register with 12:00 AM
                    if (_currentRegisterTime <= firstTimeLogModel.ClockIn.Date.AddDays(1))
                    {
                        // If validate time, allow clock out
                        ProcessClockOut(firstTimeLogModel, clockInDayOfWorkWeekModel, employeeModel, beginOfDay);
                    }
                    else if (!IsBlockRegisterIfNotCompleted(ref employeeModel) && !IsLateRegister(ref employeeModel, dayOfWorkWeekModel))
                    {
                        ProcessClockIn(null, dayOfWorkWeekModel, employeeModel);
                    }
                }
            }
            else
            {
                // If user have a previous timelog, ignore checking late register
                if (firstTimeLogModel != null)
                    ProcessClockIn(firstTimeLogModel, dayOfWorkWeekModel, employeeModel);
                else if (!IsLateRegister(ref employeeModel, dayOfWorkWeekModel)) // Clock In
                    ProcessClockIn(firstTimeLogModel, dayOfWorkWeekModel, employeeModel);
            }

            return employeeModel;
        }

        /// <summary>
        /// Process when user scan by barcode
        /// </summary>
        private void ProcessBarCode()
        {
            if (!string.IsNullOrWhiteSpace(BarCode))
            {
                if (BarCode.Length > define.MinimumBarCodeLength)
                {
                    // Checks input-code to gets the information of the appropriate employee's timelogs
                    int barCode;
                    int.TryParse(BarCode, out barCode);
                    var employeeFingerprintModel = EmployeeFingerprintCollection.FirstOrDefault(x => x.GuestId == barCode);
                    if (employeeFingerprintModel != null)
                        SelectedEmployee = ProcessEmployee(employeeFingerprintModel.EmployeeModel);
                    else
                    {
                        //ShowMessageWarning("The employee don't have fingerprint.");
                        _log4net.Warn("The employee don't have fingerprint.");
                        SelectedEmployee = null;
                    }

                    if (SelectedEmployee != null)
                    {
                        _displayTimer.Start();
                        NotifySuccess();
                    }
                    else
                    {
                        IsClockIn = false;
                        IsClockOut = false;
                        NotifyFailure();
                    }
                }
                else
                {
                    IsClockIn = false;
                    IsClockOut = false;
                    NotifyFailure();
                    _log4net.Warn("BarCode of employee must be greater than MinimumBarCodeLength");
                }
                _barCode = null;
            }
            else
            {
                SelectedEmployee = null;
                IsClockIn = false;
                IsClockOut = false;
            }
        }

        /// <summary>
        /// Process when user scan by fingerprint
        /// </summary>
        private void ProcessFingerprint()
        {
            // Have fingerprint scaned
            if (_sample != null)
            {
                SelectedEmployee = VerifyFingerprint(_sample);

                if (SelectedEmployee != null)
                {
                    _displayTimer.Start();
                    NotifySuccess();
                }
                else
                {
                    // Clear information on form
                    IsClockIn = false;
                    IsClockOut = false;
                    NotifyFailure();
                }
                _sample = null;
            }
            else
            {
                // Clear information on form
                SelectedEmployee = null;
                IsClockIn = false;
                IsClockOut = false;
            }
        }

        /// <summary>
        /// Process when scan timelog
        /// </summary>
        private void DoWork()
        {
            try
            {
                if (!_isLoading)
                {
                    _displayTimer.Stop();
                    _minimizeTimer.Stop();
                    if (_isUseFingerprint && _verificator != null)
                        ProcessFingerprint();
                    else
                        ProcessBarCode();
                    if (define.EnableIdleTime)
                        _minimizeTimer.Start();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex.ToString());
            }
        }

        /// <summary>
        /// Get full name of contact
        /// </summary>
        /// <param name="guest"></param>
        /// <returns></returns>
        private string GetFullName(base_Guest guest)
        {
            string fullName = string.Empty;
            _loadingView.progressBar1.Dispatcher.Invoke((Action)delegate
            {
                if (_loadingView.progressBar1.Value < _loadingView.progressBar1.Maximum)
                    _loadingView.progressBar1.Value += 1;
            });
            if (guest != null)
                fullName = string.Format("{0} {1} {2}", guest.FirstName, guest.MiddleName, guest.LastName);
            return fullName;
        }

        /// <summary>
        /// Load data from database
        /// </summary>
        private void LoadDatas()
        {
            try
            {
                // Load configuration model
                base_ConfigurationRepository configurationRepository = new base_ConfigurationRepository();
                IQueryable<base_Configuration> configQuery = configurationRepository.GetIQueryable();
                if (configQuery.Count() > 0)
                {
                    define.CONFIGURATION = new base_ConfigurationModel(configurationRepository.GetIQueryable().FirstOrDefault());
                }
                OnPropertyChanged(() => CompanyName);
                OnPropertyChanged(() => Website);

                DateTime tomorrow = DateTimeExt.Today.AddDays(1);

                // Refresh data
                if (EmployeeFingerprintCollection != null)
                {
                    _employeeFingerprintRepository.Refresh();
                }

                EmployeeFingerprintCollection = new ObservableCollection<base_GuestFingerPrintModel>();

                IList<base_GuestFingerPrint> employeeFingerPrints = _employeeFingerprintRepository.GetAll(x => !x.base_Guest.IsPurged &&
                    x.base_Guest.IsActived && x.base_Guest.IsTrackingHour);

                foreach (base_GuestFingerPrint employeeFingerPrint in employeeFingerPrints)
                {
                    // Initial employee fingerprint model
                    base_GuestFingerPrintModel employeeFingerPrintModel = new base_GuestFingerPrintModel(employeeFingerPrint);

                    // Initial employee model by fingerprint
                    employeeFingerPrintModel.EmployeeModel = new base_GuestModel(employeeFingerPrint.base_Guest);

                    // Get previous employee fingerprint model
                    base_GuestFingerPrintModel previousEmployeeFingerPrintModel = EmployeeFingerprintCollection.SingleOrDefault(x => x.GuestId.Equals(employeeFingerPrintModel.GuestId));
                    if (previousEmployeeFingerPrintModel == null)
                    {
                        // Load timelog collection
                        employeeFingerPrintModel.EmployeeModel.TimeLogCollection = new ObservableCollection<tims_TimeLogModel>(
                            employeeFingerPrint.base_Guest.tims_TimeLog.
                            Where(x => x.ClockIn.Date == DateTimeExt.Today).
                            OrderByDescending(x => x.ClockIn).
                            ThenBy(x => x.ClockOut).
                            Select(x => new tims_TimeLogModel(x)));
                    }
                    else
                    {
                        employeeFingerPrintModel.EmployeeModel.TimeLogCollection = new ObservableCollection<tims_TimeLogModel>();
                    }

                    // Load employee schedule collection
                    employeeFingerPrintModel.EmployeeModel.EmployeeScheduleCollection = new ObservableCollection<base_GuestScheduleModel>(
                        employeeFingerPrint.base_Guest.base_GuestSchedule.
                        Where(x => x.StartDate <= tomorrow && x.Status > (int)EmployeeScheduleStatuses.Inactive).
                        Select(x => new base_GuestScheduleModel(x)
                        {
                            WorkScheduleModel = new tims_WorkScheduleModel(x.tims_WorkSchedule)
                            {
                                WorkWeekCollection = new ObservableCollection<tims_WorkWeekModel>(
                                    x.tims_WorkSchedule.tims_WorkWeek.Select(y => new tims_WorkWeekModel(y)))
                            }
                        }));

                    // Load work permission collection
                    employeeFingerPrintModel.EmployeeModel.WorkPermissionCollection = new ObservableCollection<tims_WorkPermissionModel>(
                        employeeFingerPrint.base_Guest.tims_WorkPermission.
                        Where(x => (x.FromDate <= DateTimeExt.Today && DateTimeExt.Today <= x.ToDate) ||
                            (x.FromDate <= tomorrow && tomorrow <= x.ToDate)).
                        Select(y => new tims_WorkPermissionModel(y)));

                    // Add employee fingerprint to collection
                    EmployeeFingerprintCollection.Add(employeeFingerPrintModel);
                }

                _dayOfWorkWeekDictionary = new Dictionary<string, tims_WorkWeekModel>();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        public void RefreshDatas()
        {
            if (!_isLoading)
            {
                _isLoading = true;
                _loadingView.progressBar1.Value = 1;
                _loadingView.progressBar1.Maximum = _employeeFingerprintRepository.GetAll().Count;
                _loadingView.Show();
                _loadingWorker.RunWorkerAsync();
            }
        }

        private void ShowMessageWarning(string message)
        {
            _runWarning = true;
            _warningView.Dispatcher.Invoke((Action)delegate
            {
                _warningView.txtblMessage.Text = message;
                _warningView.txtblTime.Text = string.Format("Message box auto close after {0} seconds", _warningTime);
                //_warningView.progressBar.Value = _warningTime;
                //_warningView.progressBar.Maximum = _warningTime;
                _warningView.Show();
            });
        }

        private void Notify(string message, FingerprintStatus status)
        {
            _view.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                Icon taskbarIcon = null;
                BitmapSource bitmapSource = null;
                System.Windows.Media.Brush foregroundBrush = System.Windows.Media.Brushes.Black;
                switch (status)
                {
                    case FingerprintStatus.Successful:
                        taskbarIcon = Resources.icoComplete;
                        bitmapSource = Conversion.CreateBitmapSourceFromBitmap(Resources.imgComplete);
                        foregroundBrush = System.Windows.Media.Brushes.DimGray;
                        break;
                    case FingerprintStatus.Failed:
                        taskbarIcon = Resources.icoUnsuccessful;
                        bitmapSource = Conversion.CreateBitmapSourceFromBitmap(Resources.imgUnsuccessful);
                        foregroundBrush = System.Windows.Media.Brushes.Red;
                        break;
                    case FingerprintStatus.Connected:
                        taskbarIcon = Resources.icoConnect;
                        bitmapSource = Conversion.CreateBitmapSourceFromBitmap(Resources.imgConnect);
                        foregroundBrush = System.Windows.Media.Brushes.DimGray;
                        Console.Beep(2500, 250);
                        break;
                    case FingerprintStatus.Disconnect:
                        taskbarIcon = Resources.icoDisconect;
                        bitmapSource = Conversion.CreateBitmapSourceFromBitmap(Resources.imgDisconect);
                        foregroundBrush = System.Windows.Media.Brushes.Red;
                        break;
                }

                //_view.tbIcon.Icon = taskbarIcon;
                _view.tbIcon.ToolTipText = message;

                if (bitmapSource != null)
                {
                    _fancyBalloon = new FancyBalloon();
                    _fancyBalloon.ImageSource = bitmapSource;
                    _fancyBalloon.BalloonText = message;
                    _fancyBalloon.ForegroundText = foregroundBrush;
                    _view.tbIcon.ShowCustomBalloon(_fancyBalloon, PopupAnimation.Slide, 4000);
                }
            }));
        }

        private void NotifySuccess()
        {
            Notify("The register is success", FingerprintStatus.Successful);
            Console.Beep(2500, 250);
        }

        private void NotifyFailure()
        {
            Notify("The register is failure", FingerprintStatus.Failed);
            Console.Beep(4000, 200);
            System.Threading.Thread.Sleep(1);
            Console.Beep(4000, 200);
        }

        private void BeepBlock()
        {
            Console.Beep(4000, 200);
            System.Threading.Thread.Sleep(1);
            Console.Beep(4000, 200);
            System.Threading.Thread.Sleep(1);
            Console.Beep(4000, 200);
        }

        #endregion

        #region Fingerprint Methods

        /// <summary>
        /// Verify fingerprint
        /// </summary>
        /// <param name="Sample">DPFPSample fingerprint input sample</param>
        /// <returns>EmployeeModel</returns>
        private base_GuestModel VerifyFingerprint(DPFP.Sample Sample)
        {
            base_GuestModel employeeModel = null;

            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet featureSet = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            // Check quality of the sample and start verification if it's good
            // TODO: move to a separate task
            if (featureSet != null)
            {
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
                DPFP.Template template = new DPFP.Template();
                bool hasFingerprint = false;
                foreach (base_GuestFingerPrintModel employeeFingerprintItem in EmployeeFingerprintCollection)
                {
                    template.DeSerialize(employeeFingerprintItem.FingerPrintImage);

                    // Compare the feature set with our template
                    _verificator.Verify(featureSet, template, ref result);
                    if (result.Verified && result.FARAchieved < 1000)
                    {
                        hasFingerprint = true;

                        // Check other fingerprint                        
                        base_GuestFingerPrintModel employeeFingerprintModel = EmployeeFingerprintCollection.SingleOrDefault(x => !x.Equals(employeeFingerprintItem) && x.GuestId.Equals(employeeFingerprintItem.GuestId));
                        if (employeeFingerprintModel != null && employeeFingerprintModel.EmployeeModel.TimeLogCollection.Count > 0)
                        {
                            employeeModel = ProcessEmployee(employeeFingerprintModel.EmployeeModel);
                        }
                        else
                        {
                            employeeModel = ProcessEmployee(employeeFingerprintItem.EmployeeModel);
                        }
                        break;
                        //m_Logger.Info("The fingerprint was VERIFIED.");
                    }
                }
                if (!hasFingerprint)
                {
                    //ShowMessageWarning("The employee don't have fingerprint.");
                    _log4net.Info("The employee don't have fingerprint.");
                }
            }
            return employeeModel;
        }

        /// <summary>
        /// Extract feature from fingerprint scanner
        /// </summary>
        /// <param name="sample">DPFP Sample</param>
        /// <param name="dataPurpose">DPFP DataPurpose</param>
        /// <returns>DPFP FeatureSet</returns>
        protected static DPFP.FeatureSet ExtractFeatures(DPFP.Sample sample, DPFP.Processing.DataPurpose dataPurpose)
        {
            // Create a feature extractor
            DPFP.Processing.FeatureExtraction featureExtraction = new DPFP.Processing.FeatureExtraction();
            DPFP.Capture.CaptureFeedback captureFeedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet featureSet = new DPFP.FeatureSet();

            // TODO: return features as a result?
            featureExtraction.CreateFeatureSet(sample, dataPurpose, ref captureFeedback, ref featureSet);

            if (captureFeedback == DPFP.Capture.CaptureFeedback.Good)
                return featureSet;
            else
                return null;
        }

        /// <summary>
        /// Initiate capture operation
        /// </summary>
        protected virtual void Init()
        {
            try
            {

                // Create a capture operation.
                _capturer = new DPFP.Capture.Capture();

                if (null != _capturer)
                    // Subscribe for capturing events.
                    _capturer.EventHandler = this;
                else
                    _log4net.Error("Can't initiate capture operation!");
                //Console.WriteLine("Can't initiate capture operation!");
            }
            catch
            {
                _log4net.Error("catch Can't initiate capture operation!");
                //MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Start capture fingerprint
        /// </summary>
        protected void Start()
        {
            if (null != _capturer)
            {
                try
                {
                    _capturer.StartCapture();
                    _log4net.Error("Using the fingerprint reader, scan your fingerprint.");
                    //Console.WriteLine("Using the fingerprint reader, scan your fingerprint.");
                }
                catch
                {
                    _log4net.Error("Can't initiate capture!");
                    //Console.WriteLine("Can't initiate capture!");
                }
            }
        }

        /// <summary>
        /// Stop capture fingerprint
        /// </summary>
        protected void Stop()
        {
            if (null != _capturer)
            {
                try
                {
                    _capturer.StopCapture();
                }
                catch
                {
                    Console.WriteLine("Can't terminate capture!");
                }
            }
        }

        #endregion

        #region EventHandler Members

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            if (define.BlockFingerprint)
            {
                BeepBlock();
                return;
            }

            _sample = Sample;
            _isUseFingerprint = true;
            DoWork();
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            //throw new System.NotImplementedException();
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            //throw new System.NotImplementedException();
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            Notify("The reader fingerprint is connected", FingerprintStatus.Connected);
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            Notify("The reader fingerprint is disconnected", FingerprintStatus.Disconnect);
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            //if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
            //    BeepSuccess();
            //else
            //    BeepFailure();
        }

        #endregion

        #region Override Methods

        private void _loadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _isLoading = false;
            _loadingView.Hide();
        }

        private void _loadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadDatas();
        }

        private void _view_StateChanged(object sender, EventArgs e)
        {
            if (_view.WindowState == WindowState.Maximized || _view.WindowState == WindowState.Normal)
            {
                _view.Focus();
                _view.Activate();
                _view.ShowInTaskbar = true;
                if (define.EnableIdleTime)
                    _minimizeTimer.Start();
                RefreshDatas();
            }
            else if (_view.WindowState == WindowState.Minimized)
            {
                _view.ShowInTaskbar = false;
                _view.tbIcon.HideBalloonTip();
                _minimizeTimer.Stop();
            }
        }

        private void _minimizeTimer_Tick(object sender, EventArgs e)
        {
            _view.WindowState = WindowState.Minimized;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //_warningView.progressBar.Dispatcher.Invoke((Action)delegate
            //{
            //    _warningView.progressBar.Value--;
            //    if (_warningView.progressBar.Value == 0)
            //        _warningView.Hide();
            //});
            if (_runWarning)
                _warningView.txtblTime.Dispatcher.Invoke((Action)delegate
                {
                    _warningTime--;
                    _warningView.txtblTime.Text = string.Format("Message box auto close after {0} seconds", _warningTime);
                    if (_warningTime == 0)
                    {
                        _warningView.Hide();
                        _warningTime = 5;
                        _runWarning = false;
                    }
                });
            OnPropertyChanged(() => CurrentDate);
            OnPropertyChanged(() => CurrentTime);
        }

        private void _displayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoWork();
        }

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            if (_verificator != null)
            {
                // Initial capture
                Init();

                // Start capture operation.
                Start();
            }

            if (define.EnableIdleTime)
                _minimizeTimer.Start();
            _loadingView.Owner = _view;
            _warningView.Owner = _view;
            RefreshDatas();
        }

        private void _view_Closed(object sender, EventArgs e)
        {
            _loadingView.Close();
            _warningView.Close();
            _view.tbIcon.Dispose();
            Stop();
        }

        #endregion
    }
}