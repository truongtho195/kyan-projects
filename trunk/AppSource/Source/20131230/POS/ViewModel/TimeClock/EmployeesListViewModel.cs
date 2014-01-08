using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
        // Commands
        public ICommand OKCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand ToRightCommand { get; private set; }
        public ICommand ToLeftCommand { get; private set; }
        public ICommand MouseDoubleClickCommand { get; private set; }
        private ICollectionView _collectionView;
        #endregion

        #region Properties
        #region Keyword
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
                    FilterExecute(value);
                    OnPropertyChanged(() => Keyword);

                }
            }
        }
        #endregion

        #region LeftEmployeeCollection
        private ObservableCollection<base_Guest> _leftEmployeeCollection = new ObservableCollection<base_Guest>();
        /// <summary>
        /// Gets or sets the LeftEmployeeCollection.
        /// </summary>
        public ObservableCollection<base_Guest> LeftEmployeeCollection
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
        #endregion

        #region RightEmployeeCollection
        private ObservableCollection<base_Guest> _rightEmployeeCollection = new ObservableCollection<base_Guest>();
        /// <summary>
        /// Gets or sets the RightEmployeeCollection.
        /// </summary>
        public ObservableCollection<base_Guest> RightEmployeeCollection
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
        #endregion

        #region SelectedItemLeft
        private base_Guest _selectedItemLeft;
        /// <summary>
        /// Gets or sets the SelectedItemLeft.
        /// </summary>
        public base_Guest SelectedItemLeft
        {
            get { return _selectedItemLeft; }
            set
            {
                if (_selectedItemLeft != value)
                {
                    _selectedItemLeft = value;
                    OnPropertyChanged(() => SelectedItemLeft);
                }
            }
        }
        #endregion

        #region SelectedItemRight
        private base_Guest _selectedItemRight;
        /// <summary>
        /// Gets or sets the SelectedRight.
        /// </summary>
        public base_Guest SelectedItemRight
        {
            get { return _selectedItemRight; }
            set
            {
                if (_selectedItemRight != value)
                {
                    _selectedItemRight = value;
                    OnPropertyChanged(() => SelectedItemRight);
                }
            }
        }
        #endregion

        #region TotalItem

        /// <summary>
        /// Gets the TotalItem of LeftEmployeeCollection
        /// </summary>
        public int TotalItem
        {
            get
            {
                if (_collectionView != null)
                    return _collectionView.OfType<base_Guest>().Count();
                else if (LeftEmployeeCollection != null)
                    return LeftEmployeeCollection.Count();
                return 0;
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
            InitialData();
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOKCommandCanExecute()
        {
            if (RightEmployeeCollection == null)
                return false;
            return RightEmployeeCollection.Count > 0;
        }

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private void OnOKCommandExecute()
        {
            var window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

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
            var window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        /// <summary>
        /// Method to check whether the ToRightCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnToRightCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return param != null & (param as ObservableCollection<object>).Count > 0;
        }

        /// <summary>
        /// Method to invoke when the ToRightCommand command is executed.
        /// </summary>
        private void OnToRightCommandExecute(object param)
        {
            // TODO: Handle command logic here
            var employeeList = (ObservableCollection<object>)param;
            foreach (var item in employeeList.ToList())
            {
                ToRight(item as base_Guest);
            }
        }

        /// <summary>
        /// Method to check whether the ToLeftCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnToLeftCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return param != null & (param as ObservableCollection<object>).Count > 0;
        }

        /// <summary>
        /// Method to invoke when the ToLeftCommand command is executed.
        /// </summary>
        private void OnToLeftCommandExecute(object param)
        {
            var employeeList = (ObservableCollection<object>)param;
            foreach (var item in employeeList.ToList())
            {
                ToLeft(item as base_Guest);
            }
        }

        /// <summary>
        /// Method to check whether the OnMouseDoubleClickCommandExecuted command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMouseDoubleClickCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the OnMouseDoubleClickCommandExecute command is executed.
        /// </summary>
        private void OnMouseDoubleClickCommandExecute(object param)
        {
            if ("ToLeft".Equals(param.ToString()))
                ToLeft(SelectedItemRight);
            else
                ToRight(SelectedItemLeft);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initial Command On Constructors
        /// </summary>
        private void InitialCommand()
        {
            _log4net.Info("InitialCommand");
            OKCommand = new RelayCommand(OnOKCommandExecute, OnOKCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            ToRightCommand = new RelayCommand<object>(OnToRightCommandExecute, OnToRightCommandCanExecute);
            ToLeftCommand = new RelayCommand<object>(OnToLeftCommandExecute, OnToLeftCommandCanExecute);
            MouseDoubleClickCommand = new RelayCommand<object>(OnMouseDoubleClickCommandExecute, OnMouseDoubleClickCommandCanExecute);
        }

        private void InitialData()
        {
            try
            {
                string employeeMark = MarkType.Employee.ToDescription();
                base_Guest e = new base_Guest();
                base_GuestRepository employeeRepository = new base_GuestRepository();
                LeftEmployeeCollection = new ObservableCollection<base_Guest>(employeeRepository.GetAll(x => x.IsActived && !x.IsPurged && x.Mark.Equals(employeeMark)).OrderBy(y => y.Id));
                if (RightEmployeeCollection == null)
                    RightEmployeeCollection = new ObservableCollection<base_Guest>();

                OnPropertyChanged(() => TotalItem);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        private void ToLeft(base_Guest item)
        {
            LeftEmployeeCollection.Add(item);
            RightEmployeeCollection.Remove(item);
            OnPropertyChanged(() => TotalItem);
        }

        private void ToRight(base_Guest item)
        {

            RightEmployeeCollection.Add(item);
            LeftEmployeeCollection.Remove(item);
            OnPropertyChanged(() => TotalItem);
        }

        private void FilterExecute(string param)
        {
            if (LeftEmployeeCollection != null)
                _collectionView = CollectionViewSource.GetDefaultView(LeftEmployeeCollection);
            try
            {
                string keyword = string.Empty;
                this._collectionView.Filter = (item) =>
                {
                    bool result = false;
                    var employee = item as base_Guest;

                    if (param == null)
                        result = false;
                    else
                        keyword = param.ToString();

                    if (employee != null && string.IsNullOrWhiteSpace(employee.GuestNo))
                        result = false;
                    else
                        result |= employee.GuestNo.ToLower().Contains(keyword.TrimStart().ToLower());

                    if (employee != null)
                        result = false;
                    else
                        result |= employee.LastName.ToLower().Contains(keyword.TrimStart().ToLower());

                    if (employee != null)
                        result = false;
                    else
                        result |= employee.FirstName.ToLower().Contains(keyword.TrimStart().ToLower());

                    return result;
                };
                OnPropertyChanged(() => TotalItem);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method for set Employee Existed in WorkPermission to RightEmployeeList & remove in LeftEmployeeList
        /// </summary>
        /// <param name="employeeList"></param>
        public void SetValue(List<base_GuestModel> employeeList)
        {
            try
            {
                if (employeeList.Count > 0)
                {
                    foreach (var item in employeeList.ToList())
                    {
                        var employee = LeftEmployeeCollection.Where(x => x.Id.Equals(item.Id)).SingleOrDefault();
                        if (employee != null)
                        {
                            RightEmployeeCollection.Add(employee);
                            LeftEmployeeCollection.Remove(employee);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #region Override Methods
        protected override bool CanExecuteClosing()
        {
            if (OnOKCommandCanExecute())
            {
                var window = FindOwnerWindow(this);
                var result = window.DialogResult;
                if (!result.HasValue || (result.HasValue && !result.Value))
                {
                    MessageBoxResult returnValue = ShowMessageBox("Do you want to close this form?", "Message", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Information);
                    if (returnValue.Equals(MessageBoxResult.Yes))
                        return true;
                    else
                        return false;
                }
            }
            return base.CanExecuteClosing();
        }
        #endregion
    }
}