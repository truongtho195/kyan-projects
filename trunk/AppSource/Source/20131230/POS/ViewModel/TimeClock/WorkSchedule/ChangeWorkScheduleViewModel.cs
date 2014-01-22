using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Database;
using System.Collections.Generic;

namespace CPC.POS.ViewModel
{
    class ChangeWorkScheduleViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Defines

        private tims_WorkScheduleRepository _workScheduleRepository = new tims_WorkScheduleRepository();
        private base_GuestScheduleRepository _employeeScheduleRepository = new base_GuestScheduleRepository();

        #endregion

        #region Properties

        /// <summary>
        /// Store work schedule collection
        /// </summary>
        public ObservableCollection<tims_WorkScheduleModel> WorkScheduleCollection { get; set; }

        private tims_WorkScheduleModel _currentWorkSchedule;
        /// <summary>
        /// Gets or sets the CurrentWorkSchedule.
        /// </summary>
        public tims_WorkScheduleModel CurrentWorkSchedule
        {
            get { return _currentWorkSchedule; }
            set
            {
                if (_currentWorkSchedule != value)
                {
                    _currentWorkSchedule = value;
                    OnPropertyChanged(() => CurrentWorkSchedule);

                    // Clear right employee collection
                    if (_rightResultFilterList != null)
                        _rightResultFilterList.Clear();
                    if (RightEmployeeCollection != null)
                        RightEmployeeCollection.Clear();

                    // Reload left employee collection
                    LeftEmployeeCollection.Clear();
                    LoadLeftEmployeeCollection(CurrentWorkSchedule);

                    // Raise total left collection
                    OnPropertyChanged(() => TotalLeft);
                }
            }
        }

        private tims_WorkScheduleModel _changeToWorkSchedule;
        /// <summary>
        /// Gets or sets the ChangeToWorkSchedule.
        /// </summary>
        public tims_WorkScheduleModel ChangeToWorkSchedule
        {
            get { return _changeToWorkSchedule; }
            set
            {
                if (_changeToWorkSchedule != value)
                {
                    _changeToWorkSchedule = value;
                    OnPropertyChanged(() => ChangeToWorkSchedule);
                }
            }
        }

        private DateTime? _startDate;
        /// <summary>
        /// Gets or sets the StartDate.
        /// </summary>
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(() => StartDate);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsDirty
        /// </summary>
        public bool IsDirty { get; set; }

        #region LeftCollection

        private ObservableCollection<base_GuestModel> _leftEmployeeCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the LeftEmployeeCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> LeftEmployeeCollection
        {
            get { return _leftEmployeeCollection; }
            set
            {
                if (_leftEmployeeCollection != value)
                {
                    _leftEmployeeCollection = value;
                    OnPropertyChanged(() => LeftEmployeeCollection);
                }
            }
        }

        /// <summary>
        /// Gets the TotalLeft.
        /// </summary>
        public int TotalLeft
        {
            get
            {
                if (_leftResultFilterList != null)
                    return _leftResultFilterList.Count;
                else
                    return LeftEmployeeCollection.Count;
            }
        }

        private List<ModelBase> _leftResultFilterList;

        private object _leftFilterResult;
        /// <summary>
        /// Gets or sets the LeftFilterResult.
        /// </summary>
        public object LeftFilterResult
        {
            get { return _leftFilterResult; }
            set
            {
                if (_leftFilterResult != value)
                {
                    // Reset IsChecked before filter
                    IsCheckedAllLeft = false;

                    _leftFilterResult = value;
                    OnPropertyChanged(() => LeftFilterResult);

                    if (LeftFilterResult != null)
                        _leftResultFilterList = (LeftFilterResult as List<object>).Cast<ModelBase>().ToList();
                    else
                        _leftResultFilterList = null;

                    // Raise total items for left collection
                    OnPropertyChanged(() => TotalLeft);
                }
            }
        }

        private bool _isCheckingAllLeft = false;

        private bool? _isCheckedAllLeft = false;
        /// <summary>
        /// Gets or sets the IsCheckedAllLeft.
        /// </summary>
        public bool? IsCheckedAllLeft
        {
            get { return _isCheckedAllLeft; }
            set
            {
                if (_isCheckedAllLeft != value)
                {
                    _isCheckedAllLeft = value;
                    OnPropertyChanged(() => IsCheckedAllLeft);
                    if (IsCheckedAllLeft.HasValue)
                    {
                        _isCheckingAllLeft = true;
                        if (_leftResultFilterList != null)
                        {
                            foreach (ModelBase modelItem in _leftResultFilterList)
                                modelItem.IsChecked = IsCheckedAllLeft.Value;
                        }
                        else
                        {
                            foreach (base_GuestModel employeeItem in LeftEmployeeCollection)
                                employeeItem.IsChecked = IsCheckedAllLeft.Value;
                        }
                        _isCheckingAllLeft = false;
                    }
                }
            }
        }

        #endregion

        #region RightCollection

        private ObservableCollection<base_GuestModel> _rightEmployeeCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the RightEmployeeCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> RightEmployeeCollection
        {
            get { return _rightEmployeeCollection; }
            set
            {
                if (_rightEmployeeCollection != value)
                {
                    _rightEmployeeCollection = value;
                    OnPropertyChanged(() => RightEmployeeCollection);
                }
            }
        }

        /// <summary>
        /// Gets the TotalRight.
        /// </summary>
        public int TotalRight
        {
            get
            {
                if (_rightResultFilterList != null)
                    return _rightResultFilterList.Count;
                else
                    return RightEmployeeCollection.Count;
            }
        }

        private List<ModelBase> _rightResultFilterList;

        private object _rightFilterResult;
        /// <summary>
        /// Gets or sets the RightFilterResult.
        /// </summary>
        public object RightFilterResult
        {
            get { return _rightFilterResult; }
            set
            {
                if (_rightFilterResult != value)
                {
                    // Reset IsChecked before filter
                    IsCheckedAllRight = false;

                    _rightFilterResult = value;
                    OnPropertyChanged(() => RightFilterResult);

                    if (RightFilterResult != null)
                        _rightResultFilterList = (RightFilterResult as List<object>).Cast<ModelBase>().ToList();
                    else
                        _rightResultFilterList = null;

                    // Raise total items for right collection
                    OnPropertyChanged(() => TotalRight);
                }
            }
        }

        private bool _isCheckingAllRight = false;

        private bool? _isCheckedAllRight = false;
        /// <summary>
        /// Gets or sets the IsCheckedAllRight.
        /// </summary>
        public bool? IsCheckedAllRight
        {
            get { return _isCheckedAllRight; }
            set
            {
                if (_isCheckedAllRight != value)
                {
                    _isCheckedAllRight = value;
                    OnPropertyChanged(() => IsCheckedAllRight);
                    if (IsCheckedAllRight.HasValue)
                    {
                        _isCheckingAllRight = true;
                        if (_rightResultFilterList != null)
                        {
                            foreach (ModelBase modelItem in _rightResultFilterList)
                                modelItem.IsChecked = IsCheckedAllRight.Value;
                        }
                        else
                        {
                            foreach (base_GuestModel employeeItem in RightEmployeeCollection)
                                employeeItem.IsChecked = IsCheckedAllRight.Value;
                        }
                        _isCheckingAllRight = false;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChangeWorkScheduleViewModel()
        {
            _ownerViewModel = this;

            InitialCommand();
        }

        public ChangeWorkScheduleViewModel(tims_WorkScheduleModel workScheduleModel)
            : this()
        {
            // Set default StartDate
            StartDate = DateTimeExt.Today.AddDays(1);

            // Load active or pending work schedule
            WorkScheduleCollection = new ObservableCollection<tims_WorkScheduleModel>(_workScheduleRepository.
                GetAll(x => x.Status != (int)ScheduleStatuses.Inactive).
                Select(x => new tims_WorkScheduleModel(x)).
                OrderBy(x => x.WorkScheduleName));

            if (WorkScheduleCollection.Count > 0)
            {
                // Set selected current work schedule
                CurrentWorkSchedule = WorkScheduleCollection.FirstOrDefault(x => x.Id.Equals(workScheduleModel.Id));

                // Hidden selected work schedule
                CurrentWorkSchedule.IsChecked = true;

                if (WorkScheduleCollection.Count > 1)
                {
                    // Set selected change to work schedule
                    ChangeToWorkSchedule = WorkScheduleCollection.Where(x => !x.Id.Equals(workScheduleModel.Id)).FirstOrDefault();

                    // Hidden selected work schedule
                    ChangeToWorkSchedule.IsChecked = true;
                }
            }
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return IsValid && IsDirty && RightEmployeeCollection.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            foreach (base_GuestModel employeeItem in RightEmployeeCollection)
            {
                // Update work schedule id
                employeeItem.EmployeeScheduleModel.WorkScheduleId = ChangeToWorkSchedule.Id;

                if (employeeItem.CurrentEmployeeScheduleModel == null ||
                    (employeeItem.CurrentEmployeeScheduleModel.StartDate <= DateTimeExt.Today &&
                    employeeItem.CurrentEmployeeScheduleModel.Status != (int)EmployeeScheduleStatuses.Pending))
                {
                    if (employeeItem.NextEmployeeScheduleModel != null)
                    {
                        if (employeeItem.CurrentEmployeeScheduleModel.WorkScheduleId.Equals(employeeItem.EmployeeScheduleModel.WorkScheduleId))
                        {
                            // When change schedule from next schedule to current schedule
                            // Delete next employee schedule from database
                            _employeeScheduleRepository.Delete(employeeItem.NextEmployeeScheduleModel.base_GuestSchedule);
                        }
                        else
                        {
                            // Update next employee schedule
                            employeeItem.NextEmployeeScheduleModel.WorkScheduleId = employeeItem.EmployeeScheduleModel.WorkScheduleId;
                            employeeItem.NextEmployeeScheduleModel.StartDate = employeeItem.EmployeeScheduleModel.StartDate;
                            employeeItem.NextEmployeeScheduleModel.AssignDate = employeeItem.EmployeeScheduleModel.AssignDate;

                            // Map data from model to entity
                            employeeItem.NextEmployeeScheduleModel.ToEntity();
                        }
                    }
                    else
                    {
                        // Map data from model to entity
                        employeeItem.EmployeeScheduleModel.ToEntity();

                        // Add new employee schedule to database
                        _employeeScheduleRepository.Add(employeeItem.EmployeeScheduleModel.base_GuestSchedule);
                    }
                }
                else
                {
                    // Update current employee schedule
                    employeeItem.CurrentEmployeeScheduleModel.WorkScheduleId = employeeItem.EmployeeScheduleModel.WorkScheduleId;
                    employeeItem.CurrentEmployeeScheduleModel.StartDate = employeeItem.EmployeeScheduleModel.StartDate;
                    employeeItem.CurrentEmployeeScheduleModel.AssignDate = employeeItem.EmployeeScheduleModel.AssignDate;

                    // Map data from model to entity
                    employeeItem.CurrentEmployeeScheduleModel.ToEntity();
                }

                // Accept changes
                _employeeScheduleRepository.Commit();
            }

            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #region MoveCommand

        /// <summary>
        /// Gets the MoveCommand command.
        /// </summary>
        public ICommand MoveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the MoveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMoveCommandCanExecute()
        {
            return (!IsCheckedAllLeft.HasValue || IsCheckedAllLeft == true) && IsValid;
        }

        /// <summary>
        /// Method to invoke when the MoveCommand command is executed.
        /// </summary>
        private void OnMoveCommandExecute()
        {
            if (_leftResultFilterList != null)
            {
                foreach (base_GuestModel employeeModel in _leftResultFilterList.Cast<base_GuestModel>().Where(x => x.IsChecked).ToList())
                {
                    // Reset IsChecked before to move employees
                    employeeModel.IsChecked = false;

                    employeeModel.PropertyChanged -= new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelRight_PropertyChanged);

                    if (employeeModel.EmployeeScheduleModel == null)
                        employeeModel.EmployeeScheduleModel = new base_GuestScheduleModel();
                    employeeModel.EmployeeScheduleModel.GuestId = employeeModel.Id;
                    employeeModel.EmployeeScheduleModel.StartDate = StartDate.Value;
                    employeeModel.EmployeeScheduleModel.AssignDate = DateTimeExt.Now;
                    employeeModel.EmployeeScheduleModel.Status = (int)EmployeeScheduleStatuses.Pending;

                    // Add to destination collection
                    if (_rightResultFilterList != null)
                        _rightResultFilterList.Add(employeeModel);
                    RightEmployeeCollection.Add(employeeModel);

                    // Remove from source collection
                    if (_leftResultFilterList != null)
                        _leftResultFilterList.Remove(employeeModel);
                    LeftEmployeeCollection.Remove(employeeModel);
                }
            }
            else
            {
                foreach (base_GuestModel employeeModel in LeftEmployeeCollection.Where(x => x.IsChecked).ToList())
                {
                    // Reset IsChecked before to move employees
                    employeeModel.IsChecked = false;

                    employeeModel.PropertyChanged -= new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelRight_PropertyChanged);

                    if (employeeModel.EmployeeScheduleModel == null)
                        employeeModel.EmployeeScheduleModel = new base_GuestScheduleModel();
                    employeeModel.EmployeeScheduleModel.GuestId = employeeModel.Id;
                    employeeModel.EmployeeScheduleModel.StartDate = StartDate.Value;
                    employeeModel.EmployeeScheduleModel.AssignDate = DateTimeExt.Now;
                    employeeModel.EmployeeScheduleModel.Status = (int)EmployeeScheduleStatuses.Pending;

                    // Add to destination collection
                    if (_rightResultFilterList != null)
                        _rightResultFilterList.Add(employeeModel);
                    RightEmployeeCollection.Add(employeeModel);

                    // Remove from source collection
                    if (_leftResultFilterList != null)
                        _leftResultFilterList.Remove(employeeModel);
                    LeftEmployeeCollection.Remove(employeeModel);
                }
            }

            // Raise total items for two collection
            OnPropertyChanged(() => TotalLeft);
            OnPropertyChanged(() => TotalRight);

            // Turn on IsDirty
            IsDirty = true;
        }

        #endregion

        #region BackCommand

        /// <summary>
        /// Gets the BackCommand command.
        /// </summary>
        public ICommand BackCommand { get; private set; }

        /// <summary>
        /// Method to check whether the BackCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnBackCommandCanExecute()
        {
            return !IsCheckedAllRight.HasValue || IsCheckedAllRight == true;
        }

        /// <summary>
        /// Method to invoke when the BackCommand command is executed.
        /// </summary>
        private void OnBackCommandExecute()
        {
            if (_rightResultFilterList != null)
            {
                foreach (base_GuestModel employeeModel in _rightResultFilterList.Cast<base_GuestModel>().Where(x => x.IsChecked).ToList())
                {
                    // Reset IsChecked before to move employees
                    employeeModel.IsChecked = false;

                    employeeModel.PropertyChanged -= new PropertyChangedEventHandler(employeeModelRight_PropertyChanged);
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);

                    // Add to destination collection
                    if (_leftResultFilterList != null)
                        _leftResultFilterList.Add(employeeModel);
                    LeftEmployeeCollection.Add(employeeModel);

                    // Remove from source collection
                    if (_rightResultFilterList != null)
                        _rightResultFilterList.Remove(employeeModel);
                    RightEmployeeCollection.Remove(employeeModel);
                }
            }
            else
            {
                foreach (base_GuestModel employeeModel in RightEmployeeCollection.Where(x => x.IsChecked).ToList())
                {
                    // Reset IsChecked before to move employees
                    employeeModel.IsChecked = false;

                    employeeModel.PropertyChanged -= new PropertyChangedEventHandler(employeeModelRight_PropertyChanged);
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);

                    // Add to destination collection
                    if (_leftResultFilterList != null)
                        _leftResultFilterList.Add(employeeModel);
                    LeftEmployeeCollection.Add(employeeModel);

                    // Remove from source collection
                    if (_rightResultFilterList != null)
                        _rightResultFilterList.Remove(employeeModel);
                    RightEmployeeCollection.Remove(employeeModel);
                }
            }

            // Raise total items for two collection
            OnPropertyChanged(() => TotalLeft);
            OnPropertyChanged(() => TotalRight);

            // Turn on IsDirty
            IsDirty = true;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            MoveCommand = new RelayCommand(OnMoveCommandExecute, OnMoveCommandCanExecute);
            BackCommand = new RelayCommand(OnBackCommandExecute, OnBackCommandCanExecute);
        }

        /// <summary>
        /// Load left employee collection
        /// </summary>
        private void LoadLeftEmployeeCollection(tims_WorkScheduleModel workScheduleModel)
        {
            if (LeftEmployeeCollection.Count == 0)
            {
                // Initial left employee collection
                LeftEmployeeCollection = new ObservableCollection<base_GuestModel>();

                // Get employee mark
                string employeeMark = MarkType.Employee.ToDescription();

                // Get active employees of this work schedule
                int workScheduleID = workScheduleModel.Id;

                // Get employee schedules
                IOrderedEnumerable<base_GuestSchedule> employeeSchedules = _employeeScheduleRepository.
                    GetAll(x => x.base_Guest.IsActived && !x.base_Guest.IsPurged &&
                        x.Status != (int)EmployeeScheduleStatuses.Inactive && x.WorkScheduleId == workScheduleID).
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
                        // Get employee schedule model
                        employeeModel.CurrentEmployeeScheduleModel = new base_GuestScheduleModel(currentSchedule);
                    }

                    if (nextSchedule != null)
                    {
                        // Get next employee schedule model
                        employeeModel.NextEmployeeScheduleModel = new base_GuestScheduleModel(nextSchedule);
                    }

                    if (currentSchedule != null && nextSchedule != null)
                    {
                        if (nextSchedule.WorkScheduleId.Equals(workScheduleID))
                        {
                            // Push new employee schedule to collection
                            LeftEmployeeCollection.Add(employeeModel);
                        }
                    }
                    else
                    {
                        // Push new employee schedule to collection
                        LeftEmployeeCollection.Add(employeeModel);
                    }

                    // Register property changed event to process check all
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);
                }
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Process when form closing
        /// </summary>
        /// <returns></returns>
        protected override bool CanExecuteClosing()
        {
            Window window = FindOwnerWindow(this);
            if (OnOkCommandCanExecute() && (window != null && !window.DialogResult.HasValue))
            {
                MessageBoxResult result = _dialogService.ShowMessageBox(_ownerViewModel, "Do you want to close this form?", "TIMS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return false;
            }
            return true;
        }

        #endregion

        #region Event Methods

        private void employeeModelLeft_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!_isCheckingAllLeft)
                    {
                        int numOfItemChecked = LeftEmployeeCollection.Count(x => x.IsChecked);
                        if (numOfItemChecked == 0)
                            _isCheckedAllLeft = false;
                        else if (numOfItemChecked == LeftEmployeeCollection.Count)
                            _isCheckedAllLeft = true;
                        else
                            _isCheckedAllLeft = null;
                        OnPropertyChanged(() => IsCheckedAllLeft);
                    }
                    break;
            }
        }

        private void employeeModelRight_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!_isCheckingAllRight)
                    {
                        int numOfItemChecked = RightEmployeeCollection.Count(x => x.IsChecked);
                        if (numOfItemChecked == 0)
                            _isCheckedAllRight = false;
                        else if (numOfItemChecked == RightEmployeeCollection.Count)
                            _isCheckedAllRight = true;
                        else
                            _isCheckedAllRight = null;
                        OnPropertyChanged(() => IsCheckedAllRight);
                    }
                    break;
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
                    case "StartDate":
                        if (!StartDate.HasValue)
                            message = "StartDate is required";
                        else if (StartDate < DateTimeExt.Today)
                            message = "StartDate is not smaller than today";
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