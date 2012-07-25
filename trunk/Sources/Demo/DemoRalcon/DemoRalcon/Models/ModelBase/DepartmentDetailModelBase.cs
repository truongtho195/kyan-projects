using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;

namespace DemoFalcon.Model
{
    public class DepartmentDetailModelBase : ModelBase 
    {
        #region Constructors
        public DepartmentDetailModelBase()
        {

        }
        #endregion

        #region Properties



        private int _departmentDetailID;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int DepartmentDetailID
        {
            get { return _departmentDetailID; }
            set
            {
                if (_departmentDetailID != value)
                {
                    this.OnDepartmentDetailIDChanging(value);
                    _departmentDetailID = value;
                    RaisePropertyChanged(() => DepartmentDetailID);
                    this.OnDepartmentDetailIDChanged();
                }
            }
        }

        protected virtual void OnDepartmentDetailIDChanging(int value) { }
        protected virtual void OnDepartmentDetailIDChanged() { }




        private int _employeeID;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int EmployeeID
        {
            get { return _employeeID; }
            set
            {
                if (_employeeID != value)
                {
                    this.OnEmployeeIDChanging(value);
                    _employeeID = value;
                    RaisePropertyChanged(() => EmployeeID);
                    this.OnEmployeeIDChanged();
                }
            }
        }

        protected virtual void OnEmployeeIDChanging(int value) { }
        protected virtual void OnEmployeeIDChanged() { }



        private int _departmentID;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int DepartmentID
        {
            get { return _departmentID; }
            set
            {
                if (_departmentID != value)
                {
                    this.OnDepartmentIDChanging(value);
                    _departmentID = value;
                    RaisePropertyChanged(() => DepartmentID);
                    this.OnDepartmentIDChanged();
                }
            }
        }

        protected virtual void OnDepartmentIDChanging(int value) { }
        protected virtual void OnDepartmentIDChanged() { }




        private int _status;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    this.OnStatusChanging(value);
                    _status = value;
                    RaisePropertyChanged(() => Status);
                    this.OnStatusChanged();
                }
            }
        }

        protected virtual void OnStatusChanging(int value) { }
        protected virtual void OnStatusChanged() { }



        private DateTime _lastActive;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime LastActive
        {
            get { return _lastActive; }
            set
            {
                if (_lastActive != value)
                {
                    this.OnLastActiveChanging(value);
                    _lastActive = value;
                    RaisePropertyChanged(() => LastActive);
                    this.OnLastActiveChanged();
                }
            }
        }

        protected virtual void OnLastActiveChanging(DateTime value) { }
        protected virtual void OnLastActiveChanged() { }




        private DepartmentModel _departmentModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DepartmentModel DepartmentModel
        {
            get { return _departmentModel; }
            set
            {
                if (_departmentModel != value)
                {
                    this.OnDepartmentModelChanging(value);
                    _departmentModel = value;
                    RaisePropertyChanged(() => DepartmentModel);
                    this.OnDepartmentModelChanged();
                }
            }
        }

        protected virtual void OnDepartmentModelChanging(DepartmentModel value) { }
        protected virtual void OnDepartmentModelChanged() { }







        private EmployeeModel _employeeModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public EmployeeModel EmployeeModel
        {
            get { return _employeeModel; }
            set
            {
                if (_employeeModel != value)
                {
                    this.OnEmployeeModelChanging(value);
                    _employeeModel = value;
                    RaisePropertyChanged(() => EmployeeModel);
                    this.OnEmployeeModelChanged();
                }
            }
        }

        protected virtual void OnEmployeeModelChanging(EmployeeModel value) { }
        protected virtual void OnEmployeeModelChanged() { }




        private DateTime? _fromDate;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_fromDate != value)
                {
                    this.OnFromDateChanging(value);
                    _fromDate = value;
                    RaisePropertyChanged(() => FromDate);
                    this.OnFromDateChanged();
                }
            }
        }

        protected virtual void OnFromDateChanging(DateTime? value) { }
        protected virtual void OnFromDateChanged() { }



        private DateTime? _toDate;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                if (_toDate != value)
                {
                    this.OnToDateChanging(value);
                    _toDate = value;
                    RaisePropertyChanged(() => ToDate);
                    this.OnToDateChanged();
                }
            }
        }

        protected virtual void OnToDateChanging(DateTime? value) { }
        protected virtual void OnToDateChanged() { }
















        #endregion
    }
}
