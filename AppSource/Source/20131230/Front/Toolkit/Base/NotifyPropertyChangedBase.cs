using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CPC.Toolkit.Base
{
    /// <summary>
    /// Base class that implements the INotifyPropertyChanged interface for databinding
    /// </summary>
    [Serializable]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Warns the developer if this object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail("Invalid property name: " + propertyName);
            }
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as System.Reflection.PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        #endregion
    }
}
