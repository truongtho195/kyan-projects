using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupAssignHoursViewModel : ViewModelBase
    {
        #region Defines

        private base_GuestRepository _employeeRepository = new base_GuestRepository();

        #endregion

        #region Properties

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
                    _selectedTimeLog = value;
                    OnPropertyChanged(() => SelectedTimeLog);
                }
            }
        }

        private bool _isEditable = true;
        /// <summary>
        /// Gets or sets the IsEditable.
        /// </summary>
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged(() => IsEditable);
                }
            }
        }

        private Visibility _visibleEmployee = Visibility.Collapsed;
        /// <summary>
        /// Gets or sets the VisibleEmployee.
        /// </summary>
        public Visibility VisibleEmployee
        {
            get { return _visibleEmployee; }
            set
            {
                if (_visibleEmployee != value)
                {
                    _visibleEmployee = value;
                    OnPropertyChanged(() => VisibleEmployee);
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
                    OnPropertyChanged(() => SelectedEmployee);
                    if (SelectedEmployee != null)
                        OnSelectedEmployeeChanged();
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupAssignHoursViewModel()
        {
            // Get owner viewmodel
            _ownerViewModel = this;

            // Initial commands
            InitialCommand();
        }

        /// <summary>
        /// User for new, edit and view timelog
        /// </summary>
        /// <param name="selectedTimeLog"></param>
        /// <param name="isReadOnly"></param>
        public PopupAssignHoursViewModel(tims_TimeLogModel selectedTimeLog, bool isReadOnly = false)
            : this()
        {
            // Set is editable
            IsEditable = !isReadOnly;

            // Clone selected timelog
            SelectedTimeLog = selectedTimeLog.Clone();
            SelectedTimeLog.IsManual = IsEditable;
        }

        /// <summary>
        /// Use for new employee timelog
        /// </summary>
        /// <param name="employeeCollection"></param>
        public PopupAssignHoursViewModel(ObservableCollection<base_GuestModel> employeeCollection)
            : this()
        {
            // Show combobox and button search employee
            VisibleEmployee = Visibility.Visible;

            // Update employee collection
            EmployeeCollection = employeeCollection;

            // Set default selected employee
            _selectedEmployee = EmployeeCollection.FirstOrDefault();

            // Create new timelog
            SelectedTimeLog = NewTimeLog();
        }

        #endregion

        #region Command Methods

        #region PopupEmployeeSearchCommand

        /// <summary>
        /// Gets the PopupEmployeeSearchCommand command.
        /// </summary>
        public ICommand PopupEmployeeSearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupEmployeeSearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupEmployeeSearchCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupEmployeeSearchCommand command is executed.
        /// </summary>
        private void OnPopupEmployeeSearchCommandExecute()
        {
            PopupEmployeeSearchViewModel viewModel = new PopupEmployeeSearchViewModel();
            viewModel.EmployeeCollection = EmployeeCollection;
            bool? result = _dialogService.ShowDialog<PopupEmployeeSearchView>(_ownerViewModel, viewModel, "Employee Search");
            if (result.HasValue && result.Value)
            {
                SelectedEmployee = viewModel.SelectedEmployee;
            }
        }

        #endregion

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
            return IsValid && SelectedTimeLog.IsDirty;
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            PopupEmployeeSearchCommand = new RelayCommand(OnPopupEmployeeSearchCommandExecute, OnPopupEmployeeSearchCommandCanExecute);
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
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
        /// Process on selected employee changed
        /// </summary>
        private void OnSelectedEmployeeChanged()
        {
            if (SelectedEmployee.CurrentEmployeeScheduleModel != null)
            {
                SelectedTimeLog.EmployeeSchedule = SelectedEmployee.CurrentEmployeeScheduleModel.base_GuestSchedule;
                SelectedTimeLog.WorkScheduleId = SelectedTimeLog.EmployeeSchedule.WorkScheduleId;
                string workScheduleName = SelectedEmployee.CurrentEmployeeScheduleModel.base_GuestSchedule.tims_WorkSchedule.WorkScheduleName;
                string startDate = SelectedTimeLog.EmployeeSchedule.StartDate.ToString(Define.DateFormat);
                SelectedTimeLog.WorkScheduleGroup = string.Format("{0} - {1}", workScheduleName, startDate);
            }
            else
            {
                SelectedTimeLog.EmployeeSchedule = null;
                SelectedTimeLog.WorkScheduleId = null;
                SelectedTimeLog.WorkScheduleGroup = null;
            }
            SelectedTimeLog.GuestResource = SelectedEmployee.Resource.ToString();
            SelectedTimeLog.EmployeeId = SelectedEmployee.Id;
            SelectedTimeLog.EmployeeTemp = this.SelectedEmployee;
            SelectedTimeLog.SetDuplicationTimeLog();
            SelectedTimeLog.SetFixItemTimeClockNull();
            SelectedTimeLog.RaiseClockIn();
        }

        #endregion
    }
}