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
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupEmployeeSearchViewModel : ViewModelBase
    {
        #region Defines

        private base_GuestRepository _employeeRepository = new base_GuestRepository();
        private ICollectionView _employeeCollectionView;

        private string _keyword;

        #endregion

        #region Properties

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
                    if (EmployeeCollection != null)
                    {
                        _employeeCollectionView = CollectionViewSource.GetDefaultView(EmployeeCollection);
                        OnFilterEmployeeCollection();
                    }
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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupEmployeeSearchViewModel()
        {
            // Initial commands
            InitialCommand();

            // Load employee collection
            //LoadEmployeeCollection();
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
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                _keyword = param.ToString().ToLower();

                // Filter employee collection
                OnFilterEmployeeCollection();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
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
            return SelectedEmployee != null;
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
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Load employee collection
        /// </summary>
        private void LoadEmployeeCollection()
        {
            if (IsBusy) return;

            // Get employee mark
            string employeeMark = MarkType.Employee.ToDescription();

            // Initial predicate
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            // Default condition
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(employeeMark) && x.IsTrackingHour);

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                EmployeeCollection = new ObservableCollection<base_GuestModel>(_employeeRepository.
                    GetAll(predicate).Select(x => new base_GuestModel(x)));
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
        /// Filter employee collection when search
        /// </summary>
        private void OnFilterEmployeeCollection()
        {
            if (string.IsNullOrWhiteSpace(_keyword))
            {
                // Reset filter
                _employeeCollectionView.Filter = null;
            }
            else
            {
                // Get all statuses contain keyword
                IEnumerable<ComboItem> statusItems = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(_keyword));
                IEnumerable<bool> statusItemIDList = statusItems.Select(x => x.Value == (int)StatusBasic.Active);

                // Get all job title contain keyword
                IEnumerable<ComboItem> jobTitleItems = Common.JobTitles.Where(x => x.Text.ToLower().Contains(_keyword));
                IEnumerable<short> jobTitleItemIDList = jobTitleItems.Select(x => x.Value);

                _employeeCollectionView.Filter = item =>
                {
                    bool result = false;

                    // Get employee model
                    base_GuestModel employeeModel = item as base_GuestModel;

                    // Get all employee that GuestNo contain keyword
                    result |= employeeModel.GuestNo.ToLower().Contains(_keyword);

                    // Get all employee that Status contain keyword
                    if (statusItemIDList.Count() > 0)
                        result |= statusItemIDList.Contains(employeeModel.IsActived);

                    // Get all employee that FirstName contain keyword
                    result |= employeeModel.FirstName.ToLower().Contains(_keyword);

                    // Get all employee that LastName contain keyword
                    result |= employeeModel.LastName.ToLower().Contains(_keyword);

                    // Get all employee that Company contain keyword
                    result |= employeeModel.Company.ToLower().Contains(_keyword);

                    // Get all employee that JobTitle contain keyword
                    if (employeeModel.PositionId.HasValue && jobTitleItemIDList.Count() > 0)
                        result |= jobTitleItemIDList.Contains(employeeModel.PositionId.Value);

                    // Get all employee that Department contain keyword
                    if (Common.Departments.Any())
                    {
                        IEnumerable<int> departments = Common.Departments.Where(x => x.Text.ToLower().Contains(_keyword.ToLower())).Select(x => Convert.ToInt32(x.Value));
                        result |= departments.Contains(employeeModel.Department);
                    }

                    // Get all employee that Phone contain keyword
                    result |= employeeModel.Phone1.ToLower().Contains(_keyword);

                    // Get all employee that Email contain keyword
                    result |= employeeModel.Email.ToLower().Contains(_keyword);

                    return result;
                };
            }
        }

        #endregion
    }
}