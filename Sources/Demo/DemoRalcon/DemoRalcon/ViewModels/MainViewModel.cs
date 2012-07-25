using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemoFalcon.Commands;
using System.Windows.Input;
using DemoFalcon.Views;
using DemoFalcon.Helper;

namespace DemoFalcon.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        #region Constructors
        public MainViewModel()
        {

        }
        #endregion

        #region Properties
        public string CurrentForm { get; set; }


        private string _titles="QUẢN LÝ NHÂN SỰ";
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Titles
        {
            get { return _titles; }
            set
            {
                if (_titles != value)
                {
                    _titles = value;
                    RaisePropertyChanged(() => Titles);
                }
            }
        }


        #endregion

        #region Commands


        #region OpenFormCommand
        private ICommand _openFormCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand OpenFormCommand
        {
            get
            {
                if (_openFormCommand == null)
                {
                    _openFormCommand = new RelayCommand(this.OpenFormExecute, this.CanOpenFormExecute);
                }
                return _openFormCommand;
            }
        }

        private bool CanOpenFormExecute(object param)
        {
            return true;
        }
       
        private void OpenFormExecute(object param)
        {
            switch (param.ToString())
            {
                case "Employee":
                    if (!"Employee".Equals(CurrentForm))
                    {
                        MainWindow.StaticMainView.grdContent.Children.Clear();
                        EmployeeView employeeView = new EmployeeView();
                        this.Titles = "QUẢN LÝ NHÂN VIÊN";
                        MainWindow.StaticMainView.grdContent.Children.Add(employeeView);
                        CurrentForm = "Employee";
                    }
                    break;
                case "Department":
                    if (!"Department".Equals(CurrentForm))
                    {
                        MainWindow.StaticMainView.grdContent.Children.Clear();
                        DepartmentView deparmentView = new DepartmentView();
                        this.Titles = "QUẢN LÝ PHÒNG BAN";
                        MainWindow.StaticMainView.grdContent.Children.Add(deparmentView);
                        CurrentForm = "Department";
                    }
                    break;
                case "EmployeeInfo":
                    if (!"EmployeeInfo".Equals(CurrentForm))
                    {
                        MainWindow.StaticMainView.grdContent.Children.Clear();
                        EmplyeeInfo employeeView = new EmplyeeInfo();
                        this.Titles = "THÔNG TIN NHÂN VIÊN";
                        MainWindow.StaticMainView.grdContent.Children.Add(employeeView);
                        CurrentForm = "EmployeeInfo";
                    }
                    break;
                default:

                    break;
            }



        }
        #endregion


        #endregion
    }
}
