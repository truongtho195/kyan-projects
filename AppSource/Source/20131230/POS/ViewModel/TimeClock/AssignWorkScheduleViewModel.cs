using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Database;

namespace CPC.POS.ViewModel
{
    class AssignWorkScheduleViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Defines

        private base_GuestRepository _employeeRepository = new base_GuestRepository();

        #endregion

        #region Properties

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

        /// <summary>
        /// Contain employees has not been assigned work schedule
        /// </summary>
        public ObservableCollection<base_GuestModel> LeftEmployeeCollection { get; set; }

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

        /// <summary>
        /// Contain employees assigned work schedule
        /// </summary>
        public ObservableCollection<base_GuestModel> RightEmployeeCollection { get; set; }

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
        public AssignWorkScheduleViewModel()
        {
            _ownerViewModel = this;

            InitialCommand();
        }

        public AssignWorkScheduleViewModel(tims_WorkScheduleModel selectedWorkSchedule)
            : this()
        {
            SelectedWorkSchedule = selectedWorkSchedule;

            InitialData();

            StartDate = DateTimeExt.Today;
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
                    employeeModel.EmployeeScheduleModel.WorkScheduleId = SelectedWorkSchedule.Id;
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
                    employeeModel.EmployeeScheduleModel.WorkScheduleId = SelectedWorkSchedule.Id;
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

        protected override bool CanExecuteClosing()
        {
            Window window = FindOwnerWindow(this);
            if (RightEmployeeCollection.Count > 0 && IsDirty && (window != null && !window.DialogResult.HasValue))
            {
                MessageBoxResult result = _dialogService.ShowMessageBox(_ownerViewModel, "Do you want to close this form?", "TIMS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return false;
            }
            return true;
        }

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
        /// Load datas
        /// </summary>
        private void InitialData()
        {
            // Get employee mark
            string employeeMark = MarkType.Employee.ToDescription();

            // Initial right employee collection
            RightEmployeeCollection = new ObservableCollection<base_GuestModel>();

            foreach (base_GuestModel employeeItem in SelectedWorkSchedule.EmployeeCollection.Where(x => x.IsNew).CloneList())
            {
                // Reset IsChecked
                employeeItem.IsChecked = false;

                // Register property changed event to process check all
                employeeItem.PropertyChanged += new PropertyChangedEventHandler(employeeModelRight_PropertyChanged);
                RightEmployeeCollection.Add(employeeItem);
            }

            // Initial left employee collection
            LeftEmployeeCollection = new ObservableCollection<base_GuestModel>();

            // Get all employees
            IEnumerable<base_Guest> employees = _employeeRepository.
                GetAll(x => x.Mark.Equals(employeeMark) && x.IsActived && !x.IsPurged).
                Where(x => !RightEmployeeCollection.Select(y => y.Id).Contains(x.Id)).
                OrderBy(x => x.FirstName);

            foreach (base_Guest employee in employees)
            {
                // Create new employee model
                base_GuestModel employeeModel = new base_GuestModel(employee);

                base_GuestSchedule employeeSchedule = employee.base_GuestSchedule.OrderBy(x => x.StartDate).LastOrDefault();
                if (employee.base_GuestSchedule.Count == 0 || employeeSchedule != null && employeeSchedule.Status == (int)ScheduleStatuses.Inactive)
                {
                    // Register property changed event to process check all
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);

                    // Push new employee model to collection
                    LeftEmployeeCollection.Add(employeeModel);
                }
            }
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