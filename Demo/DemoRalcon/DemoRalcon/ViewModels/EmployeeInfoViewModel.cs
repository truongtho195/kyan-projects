using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemoFalcon.Helper;
using System.Collections.ObjectModel;
using DemoFalcon.Model;
using DemoFalcon.DataAccess;
using System.Windows.Input;
using DemoFalcon.Commands;

namespace DemoFalcon.ViewModels
{
    public class EmployeeInfoViewModel : NotifyPropertyChangedBase
    {
        #region Constructors
        public EmployeeInfoViewModel()
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
                }
            }
        }


        
        




        #endregion

        #region Commands
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
                SelectedItem = employee;
            }
        }

        #endregion
        #endregion

        #region Methods
        private void Initialize()
        {
            EmployeeDataAccess employeeDataAccess = new EmployeeDataAccess();
            EmployeeCollection = new ObservableCollection<EmployeeModel>(employeeDataAccess.GetAllWithRelation());
        }
        #endregion
    }
}
