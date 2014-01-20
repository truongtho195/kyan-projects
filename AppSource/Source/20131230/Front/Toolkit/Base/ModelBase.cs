using System;
using System.Linq.Expressions;

namespace CPC.Toolkit.Base
{
    [Serializable]
    public abstract class ModelBase : NotifyPropertyChangedBase
    {
        #region Status Properties

        protected bool _isNew;
        /// <summary>
        /// Property Base
        /// <para>Gets or sets the IsNew</para>
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (_isNew != value)
                {
                    OnIsNewChanging(value);
                    _isNew = value;
                    OnPropertyChanged(() => IsNew);
                    OnIsNewChanged();
                }
            }
        }

        protected bool _isDirty;
        /// <summary>
        /// Property Base
        /// <para>Gets or sets the IsDirty</para>
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    OnIsDirtyChanging(value);
                    _isDirty = value;
                    OnPropertyChanged(() => IsDirty);
                    OnIsDirtyChanged();
                }
            }
        }

        protected bool _isDeleted;
        /// <summary>
        /// Property Base
        /// <para>Gets or sets the IsDeleted</para>
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    OnIsDeletedChanging(value);
                    _isDeleted = value;
                    OnPropertyChanged(() => IsDeleted);
                    OnIsDeletedChanged();
                }
            }
        }

        protected bool _isChecked;
        /// <summary>
        /// Property Base
        /// <para>Gets or sets the IsChecked</para>
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    OnIsCheckedChanging(value);
                    _isChecked = value;
                    OnPropertyChanged(() => IsChecked);
                    OnIsCheckedChanged();
                }
            }
        }

        protected bool _isTemporary;
        /// <summary>
        /// Gets or sets the IsTemporary.
        /// </summary>
        public bool IsTemporary
        {
            get { return _isTemporary; }
            set
            {
                if (_isTemporary != value)
                {
                    OnIsTemporaryChanging(value);
                    _isTemporary = value;
                    OnPropertyChanged(() => IsTemporary);
                    OnIsTemporaryChanged();
                }
            }
        }

        #endregion

        #region Override Properties

        protected virtual void OnIsNewChanging(bool value) { }
        protected virtual void OnIsNewChanged() { }

        protected virtual void OnIsDirtyChanging(bool value) { }
        protected virtual void OnIsDirtyChanged() { }

        protected virtual void OnIsDeletedChanging(bool value) { }
        protected virtual void OnIsDeletedChanged() { }

        protected virtual void OnIsCheckedChanging(bool value) { }
        protected virtual void OnIsCheckedChanged() { }

        protected virtual void OnIsTemporaryChanging(bool value)
        {
        }
        protected virtual void OnIsTemporaryChanged()
        {
        }

        #endregion

        #region NotifyPropertyChanged Methods

        protected void PropertyChangedCompleted<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as System.Reflection.PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            PropertyChangedCompleted(propertyInfo.Name);
        }

        protected virtual void PropertyChangedCompleted(string propertyName)
        {
        }

        #endregion
    }
}