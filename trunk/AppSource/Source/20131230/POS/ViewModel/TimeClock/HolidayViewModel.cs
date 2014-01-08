using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    class HolidayViewModel : ViewModelBase
    {
        #region Defines

        private tims_HolidayRepository _holidayRepository = new tims_HolidayRepository();
        private tims_TimeLogRepository _timeLogRepository = new tims_TimeLogRepository();
        private ObservableCollection<DateTime> _listDateInTimelog;

        private bool _isSelectedHolidayChanged;

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

        private ObservableCollection<tims_HolidayModel> _holidayCollection = new ObservableCollection<tims_HolidayModel>();
        /// <summary>
        /// Gets or sets the HolidayCollection.
        /// </summary>
        public ObservableCollection<tims_HolidayModel> HolidayCollection
        {
            get { return _holidayCollection; }
            set
            {
                if (_holidayCollection != value)
                {
                    _holidayCollection = value;
                    OnPropertyChanged(() => HolidayCollection);
                }
            }
        }

        private tims_HolidayModel _selectedHoliday;
        /// <summary>
        /// Gets or sets the SelectedHoliday.
        /// </summary>
        public tims_HolidayModel SelectedHoliday
        {
            get { return _selectedHoliday; }
            set
            {
                if (_selectedHoliday != value)
                {
                    _selectedHoliday = value;
                    OnPropertyChanged(() => SelectedHoliday);
                    OnPropertyChanged(() => IsEditHoliday);

                    //_isSelectedHolidayChanged = true;
                    //SelectedItemHoliday = null;
                    //SelectedItemHoliday = SelectedHoliday;
                    //_isSelectedHolidayChanged = false;
                }
            }
        }

        private tims_HolidayModel _selectedItemHoliday;
        /// <summary>
        /// Gets or sets the SelectedItemHoliday.
        /// </summary>
        public tims_HolidayModel SelectedItemHoliday
        {
            get { return _selectedItemHoliday; }
            set
            {
                if (_selectedItemHoliday != value)
                {
                    _selectedItemHoliday = value;
                    OnPropertyChanged(() => SelectedItemHoliday);
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
                        foreach (tims_HolidayModel holidayItem in HolidayCollection)
                            holidayItem.IsChecked = IsCheckedAll.Value;
                        _isCheckingAll = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the IsEditHoliday.
        /// </summary>
        public bool IsEditHoliday
        {
            get { return SelectedHoliday != null; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default contructor
        /// </summary>
        public HolidayViewModel()
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
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (ShowNotification(null))
                NewHoliday();
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
            return IsValid && IsEdit();
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SaveHoliday(SelectedHoliday);
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
            return HolidayCollection.Count(x => x.IsChecked) > 0;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            if (ShowNotification(null))
            {
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this item?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    foreach (tims_HolidayModel holidayItem in HolidayCollection.Where(x => x.IsChecked).ToList())
                    {
                        // Reset IsChecked before remove
                        holidayItem.IsChecked = false;

                        // Remove  holiday from database
                        _holidayRepository.Delete(holidayItem.tims_Holiday);

                        // Accept changes
                        _holidayRepository.Commit();

                        // Remove holiday from collection
                        HolidayCollection.Remove(holidayItem);
                    }

                    SelectedHoliday = null;
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
            if (param != null && !_isSelectedHolidayChanged)
            {
                if (ShowNotification(null))
                {
                    // Update selected holiday
                    SelectedHoliday = (param as tims_HolidayModel).Clone();

                    switch ((HolidayOption)SelectedHoliday.HolidayOption)
                    {
                        case HolidayOption.SpecificDay:
                            SelectedHoliday.MonthItem = Common.Months.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == SelectedHoliday.Month);
                            break;
                        case HolidayOption.DynamicDay:
                            SelectedHoliday.WeekOfMonthItem = Common.WeeksOfMonth.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == SelectedHoliday.WeekOfMonth);
                            SelectedHoliday.DayOfWeekItem = Common.DaysOfWeek.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == SelectedHoliday.DayOfWeek);
                            SelectedHoliday.Month1Item = Common.Months.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == SelectedHoliday.Month1);
                            break;
                    }

                    // Turn off IsDirty & IsNew
                    if (!SelectedHoliday.IsNew)
                        SelectedHoliday.EndUpdate();
                    else
                        SelectedHoliday.IsDirty = false;
                }
                else
                {
                    App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                    {
                        _isSelectedHolidayChanged = true;
                        SelectedItemHoliday = null;
                        SelectedItemHoliday = HolidayCollection.SingleOrDefault(x => x.Id.Equals(SelectedHoliday.Id));
                        _isSelectedHolidayChanged = false;
                    });
                }
            }
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
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            if (SelectedHoliday == null)
                return false;

            return SelectedHoliday.IsDirty;
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
                        result = SaveHoliday(SelectedHoliday);
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    if (SelectedHoliday.IsNew)
                    {
                        SelectedHoliday = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else
                    {
                        // Refresh product datas
                        RefreshHolidayDatas();
                    }
                }
            }

            if (result && isClosing == null && SelectedHoliday != null)
            {
                if (SelectedHoliday.IsNew)
                {
                    // Clear selected item
                    SelectedHoliday = null;
                }
                else
                {
                    // Refresh product datas
                    RefreshHolidayDatas();
                }
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
                HolidayCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                // Get all holidays
                IOrderedEnumerable<tims_Holiday> holidays = _holidayRepository.GetAll().OrderBy(x => x.DateCreated);
                foreach (tims_Holiday holiday in holidays)
                {
                    bgWorker.ReportProgress(0, holiday);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create holiday model
                tims_HolidayModel holidayModel = new tims_HolidayModel((tims_Holiday)e.UserState);

                // Load relation data
                LoadRelationData(holidayModel);

                // Add to collection
                HolidayCollection.Add(holidayModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (HolidayCollection.Count > 0)
                {
                    SelectedHoliday = HolidayCollection.FirstOrDefault().Clone();
                }
                else
                {
                    NewHoliday();
                }

                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for holiday
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationData(tims_HolidayModel holidayModel)
        {
            switch ((HolidayOption)holidayModel.HolidayOption)
            {
                case HolidayOption.SpecificDay:
                    holidayModel.MonthItem = Common.Months.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.Month);
                    break;
                case HolidayOption.DynamicDay:
                    holidayModel.Month1 = holidayModel.Month;
                    holidayModel.Month = null;

                    holidayModel.WeekOfMonthItem = Common.WeeksOfMonth.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.WeekOfMonth);
                    holidayModel.DayOfWeekItem = Common.DaysOfWeek.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.DayOfWeek);
                    holidayModel.Month1Item = Common.Months.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.Month1);
                    break;
            }

            holidayModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(holidayModel_PropertyChanged);
        }

        /// <summary>
        /// Create new holiday
        /// </summary>
        private void NewHoliday()
        {
            SelectedHoliday = new tims_HolidayModel();
            SelectedHoliday.ActiveFlag = true;
            SelectedHoliday.DateCreated = DateTimeExt.Now;
            SelectedHoliday.UserCreated = Define.USER.LoginName;

            SelectedHoliday.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(holidayModel_PropertyChanged);

            // Turn off IsDirty
            SelectedHoliday.IsDirty = false;
        }

        /// <summary>
        /// Save holiday
        /// </summary>
        /// <param name="holidayModel"></param>
        /// <returns></returns>
        private bool SaveHoliday(tims_HolidayModel holidayModel)
        {
            switch ((HolidayOption)holidayModel.HolidayOption)
            {
                case HolidayOption.SpecificDay:
                    holidayModel.MonthItem = Common.Months.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.Month);

                    holidayModel.FromDate = null;
                    holidayModel.ToDate = null;
                    holidayModel.DayOfWeek = null;
                    holidayModel.WeekOfMonth = null;
                    holidayModel.Month1 = null;
                    break;
                case HolidayOption.DynamicDay:
                    holidayModel.WeekOfMonthItem = Common.WeeksOfMonth.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.WeekOfMonth);
                    holidayModel.DayOfWeekItem = Common.DaysOfWeek.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.DayOfWeek);
                    holidayModel.Month1Item = Common.Months.SingleOrDefault(x => Convert.ToInt32(x.ObjValue) == holidayModel.Month1);

                    holidayModel.FromDate = null;
                    holidayModel.ToDate = null;
                    holidayModel.Month = holidayModel.Month1;
                    holidayModel.Day = null;
                    break;
                case HolidayOption.Duration:
                    holidayModel.Month = null;
                    holidayModel.Day = null;
                    holidayModel.DayOfWeek = null;
                    holidayModel.WeekOfMonth = null;
                    holidayModel.Month1 = null;
                    break;
            }

            if (holidayModel.IsNew)
            {
                // Map data from model to entity
                holidayModel.ToEntity();

                // Add new holiday to database
                _holidayRepository.Add(holidayModel.tims_Holiday);

                // Accept changes
                _holidayRepository.Commit();

                // Update holiday id
                holidayModel.Id = holidayModel.tims_Holiday.Id;

                // Push new holiday to collection
                HolidayCollection.Add(SelectedHoliday);
            }
            else
            {
                tims_HolidayModel holidayItem = HolidayCollection.SingleOrDefault(x => x.Id.Equals(holidayModel.Id));
                holidayItem.Title = holidayModel.Title;
                //holidayItem.Description = holidayModel.Description;
                holidayItem.HolidayOption = holidayModel.HolidayOption;
                holidayItem.FromDate = holidayModel.FromDate;
                holidayItem.ToDate = holidayModel.ToDate;
                holidayItem.Month = holidayModel.Month;
                holidayItem.Day = holidayModel.Day;
                holidayItem.DayOfWeek = holidayModel.DayOfWeek;
                holidayItem.WeekOfMonth = holidayModel.WeekOfMonth;
                holidayItem.ActiveFlag = holidayModel.ActiveFlag;
                holidayItem.DateUpdated = DateTimeExt.Now;
                holidayItem.UserUpdated = Define.USER.LoginName;

                holidayItem.MonthItem = holidayModel.MonthItem;
                holidayItem.WeekOfMonthItem = holidayModel.WeekOfMonthItem;
                holidayItem.DayOfWeekItem = holidayModel.DayOfWeekItem;
                holidayItem.Month1Item = holidayModel.Month1Item;

                // Map data from model to entity
                holidayItem.ToEntity();

                // Accept changes
                _holidayRepository.Commit();
            }

            if (holidayModel.HolidayOption.Equals((int)HolidayOption.DynamicDay))
                holidayModel.Month = null;

            // Turn off IsDirty & IsNew
            holidayModel.EndUpdate();

            return true;
        }

        /// <summary>
        /// Refresh holiday datas
        /// </summary>
        private void RefreshHolidayDatas()
        {
            SelectedHoliday.Month1 = null;

            // Rollback data from entity
            SelectedHoliday.ToModelAndRaise();

            // Check month and month1 property
            if (SelectedHoliday.HolidayOption.Equals((int)HolidayOption.DynamicDay))
            {
                SelectedHoliday.Month1 = SelectedHoliday.Month;
                SelectedHoliday.Month = null;
            }

            // Turn off IsDirty & IsNew
            SelectedHoliday.EndUpdate();
        }

        #endregion

        #region Old Methods

        /// <summary>
        /// Returns true if has DayOfWeek(Mon,Tue,Wed,Thu,Fri) from fist to last, otherwise false (Sat,Sun)
        /// </summary>
        /// <param name="fist">The first System.DateTime</param>
        /// <param name="last">The last System.DateTime</param>
        private bool IsHasWeekday(DateTime fist, DateTime last)
        {
            for (DateTime d = fist; d <= last; d = d.AddDays(1))
            {
                if ((d.DayOfWeek == DayOfWeek.Monday || d.DayOfWeek == DayOfWeek.Tuesday || d.DayOfWeek == DayOfWeek.Wednesday || d.DayOfWeek == DayOfWeek.Thursday || d.DayOfWeek == DayOfWeek.Friday))
                {
                    return true;
                }
            }
            return false;
        }

        private string listDate(ObservableCollection<DateTime> dates)
        {
            string dateString = string.Empty;
            if (dates != null && dates.Count > 0)
            {
                foreach (var item in dates.OrderBy(x => x.Date))
                {
                    dateString = dateString + item.ToShortDateString() + "\n";
                }
            }
            return dateString;
        }

        private bool SaveHoliday()
        {
            #region check date input (first week or end week of month)
            if (SelectedHoliday.HolidayOption.Is(HolidayOption.DynamicDay) && ((DaysOfWeek)SelectedHoliday.DayOfWeek.Value == DaysOfWeek.WeekDay))
            {
                DateTime fromDate, toDate;
                DateTime date = new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, 1);
                if ((WeeksOfMonth)SelectedHoliday.WeekOfMonth.Value == WeeksOfMonth.First)
                {
                    fromDate = GetFirstDateOfWeekByMonth(date, SelectedHoliday.WeekOfMonth.Value);
                    toDate = GetLastDateOfWeekByMonth(date, SelectedHoliday.WeekOfMonth.Value);
                    if (!IsHasWeekday(fromDate, toDate))
                    {
                        ShowMessageBox("No weekday in first week of " + date.ToString("MMMM") + ". Please select other dates", "TIMS", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else if ((WeeksOfMonth)SelectedHoliday.WeekOfMonth.Value == WeeksOfMonth.Last)
                {
                    fromDate = GetFirstDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                    toDate = GetLastDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                    if (!IsHasWeekday(fromDate, toDate))
                    {
                        ShowMessageBox("No weekday in end week of " + date.ToString("MMMM") + ". Please select other dates", "TIMS", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            #endregion

            var AllTimeLogModel = _timeLogRepository.GetAll();
            //Update or Insert data
            if (SelectedHoliday.IsNew)
            {
                //Check Date of holiday is already exists in Timeclock.
                if (IsHasDateInTimeclock(SelectedHoliday, AllTimeLogModel))
                {
                    MessageBoxResult dialog = ShowMessageBox("Date of holiday is already existed in Timeclock. Cannot insert.\n"
                                                              + listDate(_listDateInTimelog),
                                                              "TIMS", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (IsHasDateInHoliday(_holidayRepository.GetAll()))
                {
                    ShowMessageBox("Conflicted the date in Holiday", "TIMS", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                SelectedHoliday.ActiveFlag = true;
                //[Fix]SelectedHoliday.CreatedByID = Define.UserLoginID;
                SelectedHoliday.DateCreated = DateTimeExt.Now;
                SelectedHoliday.ToEntity();

                _holidayRepository.Add(SelectedHoliday.tims_Holiday);
            }
            else if (SelectedHoliday.IsDirty)
            {
                tims_Holiday holiday = _holidayRepository.GetAll(x => x.Id == SelectedHoliday.Id).SingleOrDefault();

                ObservableCollection<DateTime> oldDates, newDates;
                oldDates = DateOfHolidayInTimelog(new tims_HolidayModel(holiday), AllTimeLogModel);
                newDates = DateOfHolidayInTimelog(SelectedHoliday, AllTimeLogModel);
                if (!oldDates.Count.Equals(newDates.Count)) //if not the same
                {
                    MessageBoxResult dialog = ShowMessageBox("Date of holiday is already existed in Timeclock. Cannot modify or addition.\n"
                                                              + listDate(new ObservableCollection<DateTime>(oldDates.Concat(newDates).Distinct())),
                                                              "TIMS", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (IsHasDateInHoliday(_holidayRepository.GetAll(x => x.Id != SelectedHoliday.Id)))
                {
                    ShowMessageBox("Conflicted the Date in Holiday", "TIMS", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else
                {
                    //[FIX]SelectedHoliday.ModifiedByID = Define.UserLoginID;
                    SelectedHoliday.DateUpdated = DateTimeExt.Now;
                    SelectedHoliday.ToEntity();
                    //[FIX]_holidayRepository.UpdateHoliday(SelectedHoliday.Holiday);
                }
            }
            _holidayRepository.Commit();
            if (SelectedHoliday.IsNew)
                SelectedHoliday.Id = SelectedHoliday.tims_Holiday.Id;
            SelectedHoliday.EndUpdate();

            return true;
        }

        private HashSet<KeyValuePair<DateTime, string>> AllDateInHoliday(IList<tims_Holiday> holidayAll)
        {
            // Get all date in holidays
            HashSet<KeyValuePair<DateTime, string>> holidays = new HashSet<KeyValuePair<DateTime, string>>();
            //Duration
            var queryDuration = holidayAll.Where(x => x.HolidayOption == (int)HolidayOption.Duration);
            if (null != queryDuration && queryDuration.Count() > 0)
            {
                foreach (var item in queryDuration)
                {
                    for (DateTime d = item.FromDate.Value; d <= item.ToDate.Value; d = d.AddDays(1))
                    {
                        holidays.Add(new KeyValuePair<DateTime, string>(d, item.Title));
                    }
                }
            }
            //SpecificDay
            var querySpecificDay = holidayAll.Where(x => x.HolidayOption == (int)HolidayOption.SpecificDay);
            if (null != querySpecificDay && querySpecificDay.Count() > 0)
            {
                holidays.UnionWith(querySpecificDay.Select(x => new KeyValuePair<DateTime, string>(new DateTime(DateTimeExt.Today.Year, x.Month.Value, x.Day.Value), x.Title)));
            }
            //DynamicDay
            var queryDynamicDay = holidayAll.Where(x => x.HolidayOption == (int)HolidayOption.DynamicDay);
            if (null != queryDynamicDay && queryDynamicDay.Count() > 0)
            {
                foreach (var item in queryDynamicDay)
                {
                    #region DynamicDay
                    if ((DaysOfWeek)item.DayOfWeek.Value == DaysOfWeek.Day)
                    {
                        int day = 0;
                        if ((WeeksOfMonth)item.WeekOfMonth == WeeksOfMonth.Last)
                        {
                            day = new DateTime(DateTimeExt.Today.Year, item.Month.Value, 1).LastDate().Day;
                        }
                        else
                        {
                            day = (int)item.WeekOfMonth;
                        }
                        DateTime date = new DateTime(DateTimeExt.Today.Year, item.Month.Value, day);
                        holidays.Add(new KeyValuePair<DateTime, string>(date, item.Title));
                    }
                    else if ((DaysOfWeek)item.DayOfWeek.Value == DaysOfWeek.WeekDay)
                    {
                        DateTime fromDate, toDate;
                        DateTime date = new DateTime(DateTimeExt.Today.Year, item.Month.Value, 1);
                        if ((WeeksOfMonth)item.WeekOfMonth.Value == WeeksOfMonth.Last)
                        {
                            fromDate = GetFirstDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                            toDate = GetLastDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                        }
                        else
                        {
                            fromDate = GetFirstDateOfWeekByMonth(date, item.WeekOfMonth.Value);
                            toDate = GetLastDateOfWeekByMonth(date, item.WeekOfMonth.Value);
                        }
                        for (DateTime d = fromDate; d <= toDate; d = d.AddDays(1))
                        {
                            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                            {
                                holidays.Add(new KeyValuePair<DateTime, string>(d, item.Title));
                            }
                        }
                    }
                    else if ((DaysOfWeek)item.DayOfWeek.Value == DaysOfWeek.WeekendDay)
                    {
                        DateTime saturday, sunday;
                        DateTime date = new DateTime(DateTimeExt.Today.Year, item.Month.Value, 1);
                        if ((WeeksOfMonth)item.WeekOfMonth.Value == WeeksOfMonth.Last)
                        {
                            saturday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Saturday);
                            holidays.Add(new KeyValuePair<DateTime, string>(saturday, item.Title));
                            sunday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Sunday);
                            holidays.Add(new KeyValuePair<DateTime, string>(saturday, item.Title));
                        }
                        else
                        {
                            saturday = GetNthDayOfMonth(date, item.WeekOfMonth.Value, DayOfWeek.Saturday);
                            holidays.Add(new KeyValuePair<DateTime, string>(saturday, item.Title));
                            sunday = GetNthDayOfMonth(date, item.WeekOfMonth.Value, DayOfWeek.Sunday);
                            holidays.Add(new KeyValuePair<DateTime, string>(saturday, item.Title));
                        }
                    }
                    else // 1 Day in (Mon,Tus,....)
                    {
                        DateTime date = new DateTime(DateTimeExt.Today.Year, item.Month.Value, 1);
                        if ((WeeksOfMonth)item.WeekOfMonth.Value == WeeksOfMonth.Last)
                        {
                            date = GetNthEndDayOfMonth(date.LastDate(), (DayOfWeek)item.DayOfWeek.Value - 4);
                        }
                        else
                        {
                            date = GetNthDayOfMonth(date, item.WeekOfMonth.Value, (DayOfWeek)item.DayOfWeek.Value - 4);
                        }
                        holidays.Add(new KeyValuePair<DateTime, string>(date, item.Title));
                    }
                    #endregion
                }
            }
            //result
            return holidays;
        }

        private bool IsHasDateInHoliday(IList<tims_Holiday> holidayAll)
        {
            HashSet<KeyValuePair<DateTime, string>> allDateInHoliday = AllDateInHoliday(holidayAll);

            if (allDateInHoliday != null && allDateInHoliday.Count > 0)
            {
                if (SelectedHoliday.HolidayOption.Is(HolidayOption.Duration))
                {
                    if (allDateInHoliday.Where(x => SelectedHoliday.FromDate.Value <= x.Key && x.Key <= SelectedHoliday.ToDate.Value).Count() > 0)
                        return true;
                }
                else if (SelectedHoliday.HolidayOption.Is(HolidayOption.SpecificDay))
                {
                    return (allDateInHoliday.Where(x => x.Key == new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, SelectedHoliday.Day.Value)).Count() > 0);
                }
                else if (SelectedHoliday.HolidayOption.Is(HolidayOption.DynamicDay))
                {
                    #region DynamicDay
                    if ((DaysOfWeek)SelectedHoliday.DayOfWeek.Value == DaysOfWeek.Day)
                    {
                        int day = 0;
                        DateTime date = new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, 1);
                        if ((WeeksOfMonth)SelectedHoliday.WeekOfMonth == WeeksOfMonth.Last)
                        {
                            day = date.LastDate().Day;
                        }
                        else
                        {
                            day = (int)SelectedHoliday.WeekOfMonth;
                        }
                        return (allDateInHoliday.Where(x => x.Key == new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, day)).Count() > 0);
                    }
                    else if ((DaysOfWeek)SelectedHoliday.DayOfWeek.Value == DaysOfWeek.WeekDay)
                    {
                        DateTime fromDate, toDate;
                        DateTime date = new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, 1);
                        if ((WeeksOfMonth)SelectedHoliday.WeekOfMonth.Value == WeeksOfMonth.Last)
                        {
                            fromDate = GetFirstDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                            toDate = GetLastDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                        }
                        else
                        {
                            fromDate = GetFirstDateOfWeekByMonth(date, SelectedHoliday.WeekOfMonth.Value);
                            toDate = GetLastDateOfWeekByMonth(date, SelectedHoliday.WeekOfMonth.Value);
                        }
                        for (DateTime d = fromDate; d <= toDate; d = d.AddDays(1))
                        {
                            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                            {
                                if (allDateInHoliday.Where(x => fromDate <= x.Key && x.Key <= toDate).Count() > 0)
                                    return true;
                            }
                        }
                    }
                    else if ((DaysOfWeek)SelectedHoliday.DayOfWeek.Value == DaysOfWeek.WeekendDay)
                    {
                        DateTime saturday, sunday;
                        DateTime date = new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, 1);
                        if ((WeeksOfMonth)SelectedHoliday.WeekOfMonth.Value == WeeksOfMonth.Last)
                        {
                            saturday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Saturday);
                            sunday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Sunday);
                            if (allDateInHoliday.Where(x => x.Key == saturday || x.Key == sunday).Count() > 0)
                                return true;
                        }
                        else
                        {
                            saturday = GetNthDayOfMonth(date, SelectedHoliday.WeekOfMonth.Value, DayOfWeek.Saturday);
                            sunday = GetNthDayOfMonth(date, SelectedHoliday.WeekOfMonth.Value, DayOfWeek.Sunday);
                            if (allDateInHoliday.Where(x => x.Key == saturday || x.Key == sunday).Count() > 0)
                                return true;
                        }
                    }
                    else // 1 Day in (Mon,Tus,....)
                    {
                        DateTime date = new DateTime(DateTimeExt.Today.Year, SelectedHoliday.Month.Value, 1);
                        if ((WeeksOfMonth)SelectedHoliday.WeekOfMonth.Value == WeeksOfMonth.Last)
                        {
                            return (allDateInHoliday.Where(x => x.Key == GetNthEndDayOfMonth(date.LastDate(), (DayOfWeek)SelectedHoliday.DayOfWeek.Value - 4)).Count() > 0);
                        }
                        else
                        {
                            return (allDateInHoliday.Where(x => x.Key == GetNthDayOfMonth(date, SelectedHoliday.WeekOfMonth.Value, (DayOfWeek)SelectedHoliday.DayOfWeek.Value - 4)).Count() > 0);
                        }

                    }
                    #endregion
                }
            }

            return false;
        }

        private bool IsHasDateInTimeclock(tims_HolidayModel holiday, IList<tims_TimeLog> allTimeLog)
        {
            if (holiday.HolidayOption.Is(HolidayOption.Duration))
            {
                DateTime fromDate = holiday.FromDate.Value;
                DateTime toDate = holiday.ToDate.Value;
                var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date >= fromDate && x.ClockIn.Date <= toDate);
                if (null != queryTimelog && queryTimelog.Count() > 0)
                {
                    _listDateInTimelog = new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                    return true;
                }
            }
            else if (holiday.HolidayOption.Is(HolidayOption.SpecificDay))
            {
                DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, holiday.Day.Value);
                var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date == date);
                if (null != queryTimelog && queryTimelog.Count() > 0)
                {
                    _listDateInTimelog = new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                    return true;
                }
            }
            #region else if (holiday.HolidayOption.Is(HolidayOption.DynamicDay))
            else if (holiday.HolidayOption.Is(HolidayOption.DynamicDay))
            {
                if ((DaysOfWeek)holiday.DayOfWeek.Value == DaysOfWeek.Day)
                {
                    int day = 0;
                    if ((WeeksOfMonth)holiday.WeekOfMonth == WeeksOfMonth.Last)
                    {
                        day = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1).LastDate().Day;
                    }
                    else
                    {
                        day = (int)holiday.WeekOfMonth;
                    }
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, day);
                    var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date == date);
                    if (null != queryTimelog && queryTimelog.Count() > 0)
                    {
                        _listDateInTimelog = new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                        return true;
                    }
                }
                else if ((DaysOfWeek)holiday.DayOfWeek.Value == DaysOfWeek.WeekDay)
                {
                    DateTime fromDate, toDate;
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1);
                    if ((WeeksOfMonth)holiday.WeekOfMonth.Value == WeeksOfMonth.Last)
                    {
                        fromDate = GetFirstDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                        toDate = GetLastDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                    }
                    else
                    {
                        fromDate = GetFirstDateOfWeekByMonth(date, holiday.WeekOfMonth.Value);
                        toDate = GetLastDateOfWeekByMonth(date, holiday.WeekOfMonth.Value);
                    }
                    var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date >= fromDate && x.ClockIn.Date <= toDate);
                    if (null != queryTimelog && queryTimelog.Count() > 0)
                    {
                        _listDateInTimelog = new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                        return true;
                    }
                }
                else if ((DaysOfWeek)holiday.DayOfWeek.Value == DaysOfWeek.WeekendDay)
                {
                    DateTime saturday, sunday;
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1);
                    if ((WeeksOfMonth)holiday.WeekOfMonth.Value == WeeksOfMonth.Last)
                    {
                        saturday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Saturday);
                        sunday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Sunday);
                    }
                    else
                    {
                        saturday = GetNthDayOfMonth(date, holiday.WeekOfMonth.Value, DayOfWeek.Saturday);
                        sunday = GetNthDayOfMonth(date, holiday.WeekOfMonth.Value, DayOfWeek.Sunday);
                    }
                    var queryTimelogWeekendDay = allTimeLog.Where(x => x.ClockIn.Date == saturday || x.ClockIn.Date == sunday);
                    if (null != queryTimelogWeekendDay && queryTimelogWeekendDay.Count() > 0)
                    {
                        _listDateInTimelog = new ObservableCollection<DateTime>(queryTimelogWeekendDay.Select(x => x.ClockIn.Date));
                        return true;
                    }
                }
                else // 1 Day in (Mon,Tus,....)
                {
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1);
                    if ((WeeksOfMonth)holiday.WeekOfMonth.Value == WeeksOfMonth.Last)
                    {
                        date = GetNthEndDayOfMonth(date.LastDate(), (DayOfWeek)holiday.DayOfWeek.Value - 4);
                    }
                    else
                    {
                        date = GetNthDayOfMonth(date, holiday.WeekOfMonth.Value, (DayOfWeek)holiday.DayOfWeek.Value - 4);
                    }
                    var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date == date);
                    if (null != queryTimelog && queryTimelog.Count() > 0)
                    {
                        _listDateInTimelog = new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                        return true;
                    }
                }
            }
            #endregion
            return false;
        }

        private ObservableCollection<DateTime> DateOfHolidayInTimelog(tims_HolidayModel holiday, IList<tims_TimeLog> allTimeLog)
        {
            if (holiday.HolidayOption.Is(HolidayOption.Duration))
            {
                DateTime fromDate = holiday.FromDate.Value;
                DateTime toDate = holiday.ToDate.Value;
                var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date >= fromDate && x.ClockIn.Date <= toDate);
                if (null != queryTimelog && queryTimelog.Count() > 0)
                    return new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                else
                    return new ObservableCollection<DateTime>();
            }
            else if (holiday.HolidayOption.Is(HolidayOption.SpecificDay))
            {
                DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, holiday.Day.Value);
                var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date == date);
                if (null != queryTimelog && queryTimelog.Count() > 0)
                    return new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
            }
            #region else if (holiday.HolidayOption.Is(HolidayOption.DynamicDay))
            else if (holiday.HolidayOption.Is(HolidayOption.DynamicDay))
            {
                if ((DaysOfWeek)holiday.DayOfWeek.Value == DaysOfWeek.Day)
                {
                    int day = 0;
                    if ((WeeksOfMonth)holiday.WeekOfMonth == WeeksOfMonth.Last)
                    {
                        day = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1).LastDate().Day;
                    }
                    else
                    {
                        day = (int)holiday.WeekOfMonth;
                    }
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, day);
                    var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date == date);
                    if (null != queryTimelog && queryTimelog.Count() > 0)
                        return new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                    else
                        return new ObservableCollection<DateTime>();
                }
                else if ((DaysOfWeek)holiday.DayOfWeek.Value == DaysOfWeek.WeekDay)
                {
                    DateTime fromDate, toDate;
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1);
                    if ((WeeksOfMonth)holiday.WeekOfMonth.Value == WeeksOfMonth.Last)
                    {
                        fromDate = GetFirstDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                        toDate = GetLastDateOfWeekByMonth(date, date.LastDate().GetWeekOfMonth());
                    }
                    else
                    {
                        fromDate = GetFirstDateOfWeekByMonth(date, holiday.WeekOfMonth.Value);
                        toDate = GetLastDateOfWeekByMonth(date, holiday.WeekOfMonth.Value);
                    }
                    var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date >= fromDate && x.ClockIn.Date <= toDate);
                    if (null != queryTimelog && queryTimelog.Count() > 0)
                        return new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                    else
                        return new ObservableCollection<DateTime>();
                }
                else if ((DaysOfWeek)holiday.DayOfWeek.Value == DaysOfWeek.WeekendDay)
                {
                    DateTime saturday, sunday;
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1);
                    if ((WeeksOfMonth)holiday.WeekOfMonth.Value == WeeksOfMonth.Last)
                    {
                        saturday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Saturday);
                        sunday = GetNthEndDayOfMonth(date.LastDate(), DayOfWeek.Sunday);
                    }
                    else
                    {
                        saturday = GetNthDayOfMonth(date, holiday.WeekOfMonth.Value, DayOfWeek.Saturday);
                        sunday = GetNthDayOfMonth(date, holiday.WeekOfMonth.Value, DayOfWeek.Sunday);
                    }
                    var queryTimelogWeekendDay = allTimeLog.Where(x => x.ClockIn.Date == saturday || x.ClockIn.Date == sunday);
                    if (null != queryTimelogWeekendDay && queryTimelogWeekendDay.Count() > 0)
                        return new ObservableCollection<DateTime>(queryTimelogWeekendDay.Select(x => x.ClockIn.Date));
                    else
                        return new ObservableCollection<DateTime>();
                }
                else // 1 Day in (Mon,Tus,....)
                {
                    DateTime date = new DateTime(DateTimeExt.Today.Year, holiday.Month.Value, 1);
                    if ((WeeksOfMonth)holiday.WeekOfMonth.Value == WeeksOfMonth.Last)
                    {
                        date = GetNthEndDayOfMonth(date.LastDate(), (DayOfWeek)holiday.DayOfWeek.Value - 4);
                    }
                    else
                    {
                        date = GetNthDayOfMonth(date, holiday.WeekOfMonth.Value, (DayOfWeek)holiday.DayOfWeek.Value - 4);
                    }
                    var queryTimelog = allTimeLog.Where(x => x.ClockIn.Date == date);
                    if (null != queryTimelog && queryTimelog.Count() > 0)
                        return new ObservableCollection<DateTime>(queryTimelog.Select(x => x.ClockIn.Date));
                    else
                        return new ObservableCollection<DateTime>();
                }
            }
            #endregion
            return new ObservableCollection<DateTime>();
        }

        private DateTime GetFirstDateOfWeekByMonth(DateTime date, int nthWeekOfMonth)
        {
            DateTime endDate = date.AddMonths(1).AddDays(-1);
            for (DateTime d = date; d <= endDate; d = d.AddDays(1))
            {
                if (d.GetWeekOfMonth() == nthWeekOfMonth)
                {
                    return d;
                }
            }

            return DateTime.Today.Date;
        }

        private DateTime GetLastDateOfWeekByMonth(DateTime date, int nthWeekOfMonth)
        {
            DateTime endDate = date.AddMonths(1).AddDays(-1);
            for (DateTime d = endDate; d >= date; d = d.AddDays(-1))
            {
                if (d.GetWeekOfMonth() == nthWeekOfMonth)
                {
                    return d;
                }
            }

            return DateTime.Today.Date;
        }

        private static DateTime GetNthDayOfMonth(DateTime FirstDateOfMonth, int nthWeekOfMonth, DayOfWeek dayOfWeek)
        {
            DateTime date = FirstDateOfMonth.AddDays((nthWeekOfMonth - 1) * 7);
            while (date.DayOfWeek != dayOfWeek)
                date = date.AddDays(1);
            return date;
        }

        private static DateTime GetNthEndDayOfMonth(DateTime EndDateOfMonth, DayOfWeek dayOfWeek)
        {
            DateTime date = EndDateOfMonth;
            while (date.DayOfWeek != dayOfWeek)
                date = date.AddDays(-1);
            return date;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Load work schedule data
        /// </summary>
        public override void LoadData()
        {
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

        private void holidayModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!_isCheckingAll)
                    {
                        int numOfItemChecked = HolidayCollection.Count(x => x.IsChecked);
                        if (numOfItemChecked == 0)
                            _isCheckedAll = false;
                        else if (numOfItemChecked == HolidayCollection.Count)
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