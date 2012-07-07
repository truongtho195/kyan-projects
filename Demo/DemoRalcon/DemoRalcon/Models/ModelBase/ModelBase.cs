using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DemoFalcon.Helper;

namespace DemoFalcon.Model
{
    public abstract class ModelBase : NotifyPropertyChangedBase
    {
        #region Properties use in progress

        /// <summary>
        /// Creating status
        /// </summary>

        private bool _isNew = false;
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (_isNew != value)
                {
                    OnIsNewChanging(value);
                    _isNew = value;
                    OnIsNewChanged();
                    RaisePropertyChanged(() => IsNew);
                }
            }
        }

        /// <summary>
        /// Editing status
        /// </summary>

        private bool _isEdit = false;
        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                if (_isEdit != value)
                {
                    OnIsEditChanging(value);
                    _isEdit = value;
                    OnIsEditChanged();
                    RaisePropertyChanged(() => IsEdit);
                }
            }
        }

        /// <summary>
        /// Deleted status
        /// </summary>

        private bool _isDelete = false;
        public bool IsDelete
        {
            get { return _isDelete; }
            set
            {
                if (_isDelete != value)
                {
                    OnIsDeleteChanging(value);
                    _isDelete = value;
                    OnIsDeleteChanged();
                    RaisePropertyChanged(() => IsDelete);
                }
            }
        }

        /// <summary>
        /// Selected status
        /// </summary>

        private bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    OnIsCheckedChanging(value);
                    _isChecked = value;
                    OnIsCheckedChanged();
                    RaisePropertyChanged(() => IsChecked);
                }
            }
        }

        /// <summary>
        /// Selected status
        /// </summary>

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    OnIsSelectedChanging(value);
                    _isSelected = value;
                    OnIsSelectedChanged();
                    RaisePropertyChanged(() => IsSelected);
                }
            }
        }

        
        #endregion

        #region Extension Methods

        protected virtual void OnIsNewChanging(object value) { }
        protected virtual void OnIsNewChanged() { }

        protected virtual void OnIsEditChanging(object value) { }
        protected virtual void OnIsEditChanged() { }

        protected virtual void OnIsDeleteChanging(object value) { }
        protected virtual void OnIsDeleteChanged() { }

        protected virtual void OnIsCheckedChanging(object value) { }
        protected virtual void OnIsCheckedChanged() { }

        protected virtual void OnIsSelectedChanging(object value) { }
        protected virtual void OnIsSelectedChanged() { }

        #endregion

        #region General change Methods

        protected virtual void OnChanging(object value)
        {

        }
        protected virtual void OnChanged()
        {
            if (!IsEdit) IsEdit = true;
        }

        #endregion
    }
}
