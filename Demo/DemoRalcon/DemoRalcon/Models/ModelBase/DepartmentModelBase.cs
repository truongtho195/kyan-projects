using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;

namespace DemoFalcon.Model
{
    public class DepartmentModelBase : ModelBase 
    {
        #region Constructors
        public DepartmentModelBase()
        {

        }
        #endregion

        #region Properties
            
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
                    OnChanged();
                    this.OnDepartmentIDChanged();
                }
            }
        }

        protected virtual void OnDepartmentIDChanging(int value) { }
        protected virtual void OnDepartmentIDChanged() { }



        private string _departmentName;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string DepartmentName
        {
            get { return _departmentName; }
            set
            {
                if (_departmentName != value)
                {
                    this.OnDepartmentNameChanging(value);
                    _departmentName = value;
                    RaisePropertyChanged(() => DepartmentName);
                    OnChanged();
                    this.OnDepartmentNameChanged();
                }
            }
        }

        protected virtual void OnDepartmentNameChanging(string value) { }
        protected virtual void OnDepartmentNameChanged() { }





        private ObservableCollection<DepartmentDetailModel> _deparmentDetailCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<DepartmentDetailModel> DepartmentDetailCollection
        {
            get { return _deparmentDetailCollection; }
            set
            {
                if (_deparmentDetailCollection != value)
                {
                    this.OnDepartmentDetailCollectionChanging(value);
                    _deparmentDetailCollection = value;
                    RaisePropertyChanged(() => DepartmentDetailCollection);
                    OnChanged();
                    this.OnDepartmentDetailCollectionChanged();
                }
            }
        }

        protected virtual void OnDepartmentDetailCollectionChanging(ObservableCollection<DepartmentDetailModel> value) { }
        protected virtual void OnDepartmentDetailCollectionChanged() { }









        #endregion
    }
}
