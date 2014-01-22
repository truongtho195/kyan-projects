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
    class EmployeesListViewModel : ViewModelBase
    {
        #region Define

        public ICommand ToRightCommand { get; private set; }
        public ICommand ToLeftCommand { get; private set; }
        public ICommand MouseDoubleClickCommand { get; private set; }
        private base_GuestRepository _employeeRepository = new base_GuestRepository();
        private ICollectionView _collectionView;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the SelectedWorkPermission.
        /// </summary>
        public tims_WorkPermissionModel SelectedWorkPermission { get; set; }

        /// <summary>
        /// Gets or sets the EmployeeList.
        /// </summary>
        public ObservableCollection<base_GuestModel> EmployeeList { get; set; }

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

        #region Constructor

        // Default contructor
        public EmployeesListViewModel()
            : base()
        {
            _ownerViewModel = this;

            InitialCommand();
        }

        public EmployeesListViewModel(tims_WorkPermissionModel selectedWorkPermission, ObservableCollection<base_GuestModel> employeeList)
            : this()
        {
            SelectedWorkPermission = selectedWorkPermission;
            EmployeeList = employeeList;

            InitialData();
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
            return IsDirty;
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
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
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
            return !IsCheckedAllLeft.HasValue || IsCheckedAllLeft == true;
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
        /// Initial Command On Constructors
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            MoveCommand = new RelayCommand(OnMoveCommandExecute, OnMoveCommandCanExecute);
            BackCommand = new RelayCommand(OnBackCommandExecute, OnBackCommandCanExecute);
        }

        private void InitialData()
        {
            try
            {
                // Get employee mark
                string employeeMark = MarkType.Employee.ToDescription();

                foreach (base_GuestModel employeeItem in EmployeeList.CloneList())
                {
                    // Reset IsChecked
                    employeeItem.IsChecked = false;

                    // Register property changed event to process check all
                    employeeItem.PropertyChanged += new PropertyChangedEventHandler(employeeModelRight_PropertyChanged);

                    // Push new employee model to collection
                    RightEmployeeCollection.Add(employeeItem);
                }

                // Get all employees
                IEnumerable<base_Guest> employees = _employeeRepository.
                    GetAll(x => x.Mark.Equals(employeeMark) && x.IsActived && !x.IsPurged).
                    Where(x => !RightEmployeeCollection.Select(y => y.Id).Contains(x.Id)).
                    OrderBy(x => x.FirstName);

                foreach (base_Guest employee in employees)
                {
                    // Create new employee model
                    base_GuestModel employeeModel = new base_GuestModel(employee);

                    // Register property changed event to process check all
                    employeeModel.PropertyChanged += new PropertyChangedEventHandler(employeeModelLeft_PropertyChanged);

                    // Push new employee model to collection
                    LeftEmployeeCollection.Add(employeeModel);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
    }
}