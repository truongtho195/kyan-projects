using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemoFalcon.Helper;
using System.Windows.Input;
using DemoFalcon.Commands;
using DemoFalcon.Model;
using System.Windows;
using DemoFalcon.DataAccess;
using System.Collections.ObjectModel;
using DemoFalcon.Views;
using System.ComponentModel;
using System.Windows.Data;

namespace DemoFalcon.ViewModels
{
    public class DepartmentViewModel : NotifyPropertyChangedBase
    {
        #region Constructors
        public DepartmentViewModel()
        {
            Initialize();
        }
        #endregion

        #region Properies

        private ObservableCollection<DepartmentModel> _departmentCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<DepartmentModel> DepartmentCollection
        {
            get { return _departmentCollection; }
            set
            {
                if (_departmentCollection != value)
                {
                    _departmentCollection = value;
                    RaisePropertyChanged(() => DepartmentCollection);
                }
            }
        }



        private DepartmentModel _selectedItem;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DepartmentModel SelectedItem
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
                var department = param as DepartmentModel;
                if (department.IsEdit)
                {
                    if (department.Errors.Count() == 0)
                    {
                        var confirm = MessageBox.Show("Bạn có muốn lưu dữ liệu bạn đã thay đổi không ?", "LƯU DỮ LIỆU ", MessageBoxButton.YesNoCancel);
                        if (confirm.Equals(MessageBoxResult.Yes))
                        {
                            SaveExecute(null);
                        }
                        else if (confirm.Equals(MessageBoxResult.No))
                            SelectedItem = department;

                    }
                }
                SelectedItem = department;
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
            SelectedItem = new DepartmentModel();
            SelectedItem.IsNew = true;
            SelectedItem.IsEdit = false;
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
            DepartmentDataAccess departmentDataAccess = new DepartmentDataAccess();
            if (SelectedItem.IsNew)
            {
                SelectedItem.DepartmentDetailCollection = new ObservableCollection<DepartmentDetailModel>();
                departmentDataAccess.Insert(SelectedItem);
                DepartmentCollection.Add(SelectedItem);
            }
            else
            {
                departmentDataAccess.Update(SelectedItem);
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
            //var confirm = MessageBox.Show("Bạn có muốn xóa Phòng này không?", "XÓA NHÂN VIÊN", MessageBoxButton.YesNo);
            //if (confirm.Equals(MessageBoxResult.Yes))
            //{
            //    DepartmentDataAccess employeeDataaccess = new DepartmentDataAccess();
            //    employeeDataaccess.Delete(SelectedItem);
            //    EmployeeCollection.Remove(SelectedItem);
            //    SelectedItem = EmployeeCollection.FirstOrDefault();
            //}
        }
        #endregion


        #region AddEmployeeCommand
        private ICommand _addEmployeeCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand AddEmployeeCommand
        {
            get
            {
                if (_addEmployeeCommand == null)
                {
                    _addEmployeeCommand = new RelayCommand(this.AddEmployeeExecute, this.CanAddEmployeeExecute);
                }
                return _addEmployeeCommand;
            }
        }

        private bool CanAddEmployeeExecute(object param)
        {
            if (SelectedItem == null)
                return false;
            return SelectedItem != null && SelectedItem.DepartmentDetailCollection != null;
        }
        EmployeeList _employeeView;
        private void AddEmployeeExecute(object param)
        {
            if (SelectedItem.DepartmentDetailCollection == null)
                SelectedItem.DepartmentDetailCollection = new ObservableCollection<DepartmentDetailModel>();
            _employeeView = new EmployeeList();
            EmployeeDataAccess employeeDataAccess = new EmployeeDataAccess();
            EmployeeCollection = new ObservableCollection<EmployeeModel>(employeeDataAccess.GetAll());
            foreach (var item in SelectedItem.DepartmentDetailCollection)
            {
                var employeeModel = EmployeeCollection.Where(x => x.EmployeeID == item.EmployeeID).SingleOrDefault();
                if (employeeModel != null)
                {
                    EmployeeCollection.Remove(employeeModel);
                }
            }
            _employeeView.DataContext = this;
            _employeeView.ShowDialog();
        }
        #endregion



        #region EmployeeButtonHandlerCommand
        private ICommand _EmployeeButtonHandlerCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand EmployeeButtonHandlerCommand
        {
            get
            {
                if (_EmployeeButtonHandlerCommand == null)
                {
                    _EmployeeButtonHandlerCommand = new RelayCommand(this.EmployeeButtonHandlerExecute, this.CanEmployeeButtonHandlerExecute);
                }
                return _EmployeeButtonHandlerCommand;
            }
        }

        private bool CanEmployeeButtonHandlerExecute(object param)
        {
            return true;
        }

        private void EmployeeButtonHandlerExecute(object param)
        {
            if ("OK".Equals(param.ToString()))
            {
                foreach (var item in EmployeeCollection.Where(x => x.IsChecked))
                {
                    DepartmentDetailModel departmentDetail = new DepartmentDetailModel();
                    departmentDetail.EmployeeID = item.EmployeeID;
                    departmentDetail.DepartmentID = SelectedItem.DepartmentID;
                    departmentDetail.EmployeeModel = item;
                    departmentDetail.LastActive = DateTime.Today;
                    departmentDetail.Status = 0;
                    DepartmentDetailDataAccess deparmentDetailDA = new DepartmentDetailDataAccess();
                    deparmentDetailDA.Insert(departmentDetail);
                    SelectedItem.DepartmentDetailCollection.Add(departmentDetail);
                    RaisePropertyChanged(() => SelectedItem);
                }
            }

            if (_employeeView.ShowActivated)
            {
                _employeeView.Close();
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
            return true;
        }

        private void FilterTextChangedExecute(object param)
        {
            FilterEmployee(param);
        }
        #endregion


        #endregion

        #region Methods
        private ICollectionView _departmentFilterView;
        private void Initialize()
        {
            DepartmentDataAccess departmentDataAccess = new DepartmentDataAccess();
            DepartmentCollection = new ObservableCollection<DepartmentModel>(departmentDataAccess.GetAllWithRelation());
        }

        private void FilterEmployee(object keyword)
        {

            this._departmentFilterView = CollectionViewSource.GetDefaultView(this.DepartmentCollection);

            try
            {
                this._departmentFilterView.Filter = (item) =>
                {
                    if (item as DepartmentModel == null)
                        return true;
                    if (keyword == null)
                        return true;
                    DepartmentModel department = (DepartmentModel)item;
                    if (department.DepartmentName.ToLower().Contains(keyword.ToString().TrimStart().ToLower()))
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
