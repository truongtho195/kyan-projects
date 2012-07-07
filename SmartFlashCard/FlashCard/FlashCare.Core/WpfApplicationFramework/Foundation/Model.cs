
namespace System.Waf.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Reflection;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the base class for a model.
    /// </summary>
    [Serializable]
    public class Model : INotifyPropertyChanged
    {
        [NonSerialized]
        private PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = propertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        ///// <summary>
        ///// Raises the <see cref="E:PropertyChanged"/> event.
        ///// </summary>
        ///// <param name="propertyName">The property name of the property that has changed.</param>
        //protected void RaisePropertyChanged(string propertyName)
        //{
        //    if (WafConfiguration.Debug) { CheckPropertyName(propertyName); }
        //    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        //}

        ///// <summary>
        ///// Raises the <see cref="E:PropertyChanged"/> event.
        ///// </summary>
        ///// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        //protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    if (propertyChanged != null) { propertyChanged(this, e); }
        //}

        //private void CheckPropertyName(string propertyName)
        //{
        //    PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(this)[propertyName];
        //    if (propertyDescriptor == null)
        //    {
        //        throw new InvalidOperationException(string.Format(null,
        //            "The property with the propertyName '{0}' doesn't exist.", propertyName));
        //    }
        //}

        /// <summary>
        /// Example: RaisePropertyChanged(() => this.Age, () => this.Weight);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpressions"></param>
        protected void RaisePropertyChanged<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
                RaisePropertyChanged<T>(propertyExpression);
        }

    }
}
