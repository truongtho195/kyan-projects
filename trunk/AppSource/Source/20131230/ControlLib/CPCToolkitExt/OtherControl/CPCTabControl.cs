using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using CPCToolkitExt.OtherControl;

namespace CPCToolkitExt.OtherControl
{
    public class CPCTabControl : TabControl, INotifyPropertyChanged
    {
        public CPCTabControl()
        {

        }
        private void item_RaiseValidation(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Items.Cast<object>().Where(x => (x is CPCTabItemControl) && (x as CPCTabItemControl).Errors.Count > 0).Count() > 0)
                    this.IsValid = false;
                else
                    this.IsValid = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*****************Item_RaiseValidation************" + ex);
            }
        }

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
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
        #endregion

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(CPCTabControl), new UIPropertyMetadata(true));

        public object DataChanged
        {
            get { return (object)GetValue(DataChangedProperty); }
            set { SetValue(DataChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDataContextChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataChangedProperty =
            DependencyProperty.Register("DataChanged", typeof(object), typeof(CPCTabControl), new UIPropertyMetadata(null, OnDataContextChanged));

        protected static void OnDataContextChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (source is CPCTabControl)
                    (source as CPCTabControl).OnDataChanged(e.NewValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*****************CPCTabControl************" + ex);
            }
        }
        protected void OnDataChanged(object value)
        {
            if (this.Items.Count > 0)
            {
                FrameworkElement control = this.Items[0] as FrameworkElement;
                if (control != null)
                    control.Focus();
            }
            this.IsValid = true;
            if (value != null)
                //To register RaiseValidation event.
                foreach (CPCTabItemControl item in this.Items.Cast<object>().Where(x => x is CPCTabItemControl))
                {
                    if (item.IsCheckError)
                    {
                        item.RowErrorContent = string.Empty;
                        item.OnDataChanged();
                        item.RaiseValidation += new RoutedEventHandler(item_RaiseValidation);
                    }
                }
            else
                //To remove RaiseValidation event.
                foreach (CPCTabItemControl item in this.Items.Cast<object>().Where(x => x is CPCTabItemControl))
                {
                    if (item.IsCheckError)
                    {
                        item.RowErrorContent = string.Empty;
                        item.RemoveErrorChangedHandler();
                        item.RaiseValidation -= new RoutedEventHandler(item_RaiseValidation);
                    }
                }
        }
    }
}
