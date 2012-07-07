using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.EntityClient;
using DemoFalcon.Model;
using DemoFalcon.DataAccess;
using System.Collections.ObjectModel;
using DemoFalcon.Helper;
using System.Windows.Input;
using DemoFalcon.Commands;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;

namespace DemoFalcon.ViewModels
{
    public class EmployeeViewModel : NotifyPropertyChangedBase
    {
        #region Constructors
        public EmployeeViewModel()
        {

            Initialize();
        }
        #endregion
        #region Properties

        private ObservableCollection<EmployeeModel> _employeeCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<EmployeeModel> EmployeeCollection
        {
            get { return _employeeCollection; }
            set
            {
                if (_employeeCollection != value)
                {
                    _employeeCollection = value;
                    RaisePropertyChanged(() => EmployeeCollection);
                }
            }
        }


        private EmployeeModel _selectedItem;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public EmployeeModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    RaisePropertyChanged(() => SelectedItem);
                    RaisePropertyChanged(() => UnLockForm);
                }
            }
        }



        private List<FieldModel> _genderCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<FieldModel> GenderCollection
        {
            get { return _genderCollection; }
            set
            {
                if (_genderCollection != value)
                {
                    _genderCollection = value;
                    RaisePropertyChanged(() => GenderCollection);
                }
            }
        }




        private List<CountryModel> _countryCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<CountryModel> CountryCollection
        {
            get { return _countryCollection; }
            set
            {
                if (_countryCollection != value)
                {
                    _countryCollection = value;
                    RaisePropertyChanged(() => CountryCollection);
                }
            }
        }



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool UnLockForm
        {
            get
            {
                if (SelectedItem == null)
                    return false;
                return true;
            }
        }


        private ICollectionView _employeeFilterView;


        #endregion
        #region Command

        #region SelectionChangedCommand
        private ICommand _selectionChangedCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new RelayCommand(this.SelectionChangedExecute, this.CanSelectionChangedExecute);
                }
                return _selectionChangedCommand;
            }
        }

        private bool CanSelectionChangedExecute(object param)
        {
            return true;
        }

        private void SelectionChangedExecute(object param)
        {
            if (param != null)
            {
                var employee = param as EmployeeModel;
                if (employee.IsEdit)
                {
                    if (employee.Errors.Count() == 0)
                    {
                        var confirm = MessageBox.Show("Bạn có muốn lưu dữ liệu bạn đã thay đổi không ?", "LƯU DỮ LIỆU ", MessageBoxButton.YesNoCancel);
                        if (confirm.Equals(MessageBoxResult.Yes))
                        {
                            SaveExecute(null);
                        }
                        else if (confirm.Equals(MessageBoxResult.No))
                            SelectedItem = employee;

                    }
                }
                SelectedItem = employee;
            }


        }

        #endregion


        #region NewItemCommand
        private ICommand _newItemCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand NewItemCommand
        {
            get
            {
                if (_newItemCommand == null)
                {
                    _newItemCommand = new RelayCommand(this.NewItemExecute, this.CanNewItemExecute);
                }
                return _newItemCommand;
            }
        }

        private bool CanNewItemExecute(object param)
        {
            if (SelectedItem == null)
                return true;
            return (SelectedItem != null && !SelectedItem.IsEdit);
        }

        private void NewItemExecute(object param)
        {
            SelectedItem = NewDefault();
        }
        #endregion


        #region SaveCommand
        private ICommand _saveCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(this.SaveExecute, this.CanSaveExecute);
                }
                return _saveCommand;
            }
        }

        private bool CanSaveExecute(object param)
        {
            if (SelectedItem == null)
                return false;
            return SelectedItem.IsEdit && SelectedItem.Errors.Count == 0;
        }

        private void SaveExecute(object param)
        {
            EmployeeDataAccess employeeDataAccess = new EmployeeDataAccess();
            if (SelectedItem.IsNew)
            {
                employeeDataAccess.Insert(SelectedItem);
                EmployeeCollection.Add(SelectedItem);
            }
            else
            {
                employeeDataAccess.Update(SelectedItem);
            }
        }
        #endregion


        #region DeleteCommand
        private ICommand _deleteCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(this.DeleteExecute, this.CanDeleteExecute);
                }
                return _deleteCommand;
            }
        }

        private bool CanDeleteExecute(object param)
        {
            if (SelectedItem == null)
                return false;
            return true;
        }

        private void DeleteExecute(object param)
        {
            var confirm = MessageBox.Show("Bạn có muốn xóa nhân viên này không?", "XÓA NHÂN VIÊN", MessageBoxButton.YesNo);
            if (confirm.Equals(MessageBoxResult.Yes))
            {
                EmployeeDataAccess employeeDataaccess = new EmployeeDataAccess();
                employeeDataaccess.Delete(SelectedItem);
                EmployeeCollection.Remove(SelectedItem);
                SelectedItem = EmployeeCollection.FirstOrDefault();
            }
        }
        #endregion


        #region FilterTextChangedCommand
        private ICommand _filterTextChangedCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand FilterTextChangedCommand
        {
            get
            {
                if (_filterTextChangedCommand == null)
                {
                    _filterTextChangedCommand = new RelayCommand(this.FilterTextChangedExecute, this.CanFilterTextChangedExecute);
                }
                return _filterTextChangedCommand;
            }
        }

        private bool CanFilterTextChangedExecute(object param)
        {
            if (EmployeeCollection == null)
                return false;
            return EmployeeCollection.Count > 0;
        }

        private void FilterTextChangedExecute(object param)
        {
            FilterEmployee(param);
        }
        #endregion


        #endregion
        #region Methods
        private void Initialize()
        {
            //Create Gender
            GenderCollection = new List<FieldModel>();
            GenderCollection.Add(new FieldModel() { ID = 1, Name = "Nam" });
            GenderCollection.Add(new FieldModel() { ID = 0, Name = "Nữ" });

            //Get Country 
            CountryDataAccess countryDataAccess = new CountryDataAccess();
            CountryCollection = new List<CountryModel>(countryDataAccess.GetAll());

            //Get Employee
            EmployeeDataAccess employeeDataAccess = new EmployeeDataAccess();
            EmployeeCollection = new ObservableCollection<EmployeeModel>(employeeDataAccess.GetAll());

            this._employeeFilterView = CollectionViewSource.GetDefaultView(this.EmployeeCollection);
        }

        private EmployeeModel NewDefault()
        {
            EmployeeModel employee = new EmployeeModel();
            employee.BirthDate = DateTime.Now.AddYears(-5);
            employee.IsNew = true;
            employee.IsEdit = false;
            return employee;

        }


        private void FilterEmployee(object keyword)
        {

            this._employeeFilterView = CollectionViewSource.GetDefaultView(this.EmployeeCollection);

            try
            {
                this._employeeFilterView.Filter = (item) =>
                {
                    if (item as EmployeeModel == null)
                        return true;
                    if (keyword == null)
                        return true;
                    EmployeeModel employee = (EmployeeModel)item;

                    string fullName = string.Join(" ", employee.FirstName, employee.MiddleName, employee.LastName).ToLower();
                    string strKeyword = string.Empty;
                    DateTime dtKeyWord = DateTime.MinValue;
                    if (DateTime.TryParse(keyword.ToString(), out dtKeyWord))
                    {
                        if (dtKeyWord >= employee.BirthDate)
                            return true;
                    }
                    else if (fullName.Contains(keyword.ToString().TrimStart().ToLower()))
                    {
                        return true;
                    }
                    return false;

                };

            }
            catch (Exception ex)
            {

            }
        }


        #endregion
    }


}
