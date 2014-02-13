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

namespace CPCToolkitExt.OtherControl
{
    public class CPCTabItemControl : TabItem, INotifyPropertyChanged
    {
        #region Fields
        /// <summary>
        /// To store error of row when the ErrorChangedHandler function execute.
        /// </summary>
        public readonly HashSet<ValidationError> Errors = new HashSet<ValidationError>();
        #endregion

        #region TabItemControl
        public CPCTabItemControl()
        {
            this.RowErrorContent = string.Empty;
        }

        #endregion

        #region Envent Control

        #region ErrorChangedHandler
        /// <summary>
        /// ErrorChangedHandler event of Validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorChangedHandler(object sender, ValidationErrorEventArgs e)
        {
            try
            {
                ///To get all error of row.
                if ((e.Error.BindingInError as BindingExpression).HasError
                    && e.Action == ValidationErrorEventAction.Added
                    && !this.Errors.Contains(e.Error))
                    this.Errors.Add(e.Error);
                ///To remove error of row.
                else
                {
                    if (e.Action == ValidationErrorEventAction.Removed)
                        this.Errors.Remove(e.Error);
                    if (this.Errors.Where(x => !(x.BindingInError as BindingExpression).HasError).Count() > 0)
                        this.Errors.RemoveWhere(x => !(x.BindingInError as BindingExpression).HasError);
                }
                this.RowErrorContent = string.Empty;
                this.VisibilityError = Visibility.Hidden;
                //To get error content from errors collection.
                if (this.Errors.Count > 0)
                {
                    foreach (var item in Errors.GroupBy(x => x.ErrorContent))
                        this.RowErrorContent += item.Select(z => z.ErrorContent).FirstOrDefault() + "\n";
                    this.RowErrorContent = this.RowErrorContent.TrimEnd('\n');
                    this.VisibilityError = Visibility.Visible;
                }
                //To raise RaiseValidationEvent
                this.OnRaiseValidation();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------CPCTabItemControl * ErrorChangedHandler *------------ \n" + ex.Message);
            }

        }
        #endregion

        #endregion

        #region Properties

        #region RowErrorContent
        /// <summary>
        /// Get ,Set content of error.
        /// </summary>
        private string _rowErrorContent = string.Empty;
        public string RowErrorContent
        {
            get { return _rowErrorContent; }
            set
            {
                if (_rowErrorContent != value)
                {
                    _rowErrorContent = value;
                    RaisePropertyChanged(() => RowErrorContent);
                }
            }
        }
        #endregion

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
        #endregion

        #region DependencyProperties

        #region VisibilityError
        /// <summary>
        /// Visible Error in TabItem
        /// </summary>
        public Visibility VisibilityError
        {
            get { return (Visibility)GetValue(VisibilityErrorProperty); }
            set
            {
                SetValue(VisibilityErrorProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VisibilityError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityErrorProperty =
            DependencyProperty.Register("VisibilityError", typeof(Visibility), typeof(CPCTabItemControl), new PropertyMetadata(Visibility.Collapsed));

        #endregion

        #region IsCheckError
        public bool IsCheckError
        {
            get { return (bool)GetValue(IsCheckErrorProperty); }
            set { SetValue(IsCheckErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCheckErrorContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckErrorProperty =
            DependencyProperty.Register("IsCheckError", typeof(bool), typeof(CPCTabItemControl), new UIPropertyMetadata(false));

        #endregion

        #region RaiseValidation
        /// <summary>
        /// RaiseValidation routed event
        /// </summary>
        public static readonly RoutedEvent RaiseValidationEvent = EventManager.RegisterRoutedEvent("RaiseValidation", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(CPCTabItemControl));

        /// <summary>
        /// Click event wrapper
        /// </summary>
        public event RoutedEventHandler RaiseValidation
        {
            add { AddHandler(RaiseValidationEvent, value); }
            remove { RemoveHandler(RaiseValidationEvent, value); }
        }

        /// <summary>
        /// Method that raises the RaiseValidation event
        /// </summary>
        public virtual void OnRaiseValidation()
        {
            RoutedEventArgs args = new RoutedEventArgs(RaiseValidationEvent);
            this.RaiseEvent(args);
        }
        #endregion

        #region VisibilityTabItem
        /// <summary>
        /// To set visibility of TabItem. 
        /// </summary>
        public Visibility VisibilityTabItem
        {
            get { return (Visibility)GetValue(VisibilityTabItemProperty); }
            set { SetValue(VisibilityTabItemProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityTabItemProperty =
            DependencyProperty.Register("VisibilityTabItem", typeof(Visibility), typeof(CPCTabItemControl), new PropertyMetadata(Visibility.Visible, OnValueVisibilityChanged));

        protected static void OnValueVisibilityChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as CPCTabItemControl).SetVisibilityTabItem((Visibility)e.NewValue);
        }
        #endregion

        #endregion

        #region Methods

        #region OnDataChanged
        /// <summary>
        /// To remove ErrorChangedHandler when data of TabItem is changed.
        /// To register ErrorChangedHandler when TabItem load.
        /// </summary>
        public void OnDataChanged()
        {
            this.RowErrorContent = string.Empty;
            if (this.IsCheckError)
            {
                this.RemoveErrorChangedHandler();
                Validation.AddErrorHandler(this, ErrorChangedHandler);
            }
        }
        #endregion

        #region RemoveErrorChangedHandler
        /// <summary>
        /// To remove ErrorChangedHandler when data of TabItem is changed.
        /// </summary>
        public void RemoveErrorChangedHandler()
        {
            this.Errors.Clear();
            this.RowErrorContent = string.Empty;
            this.VisibilityError = Visibility.Hidden;
            Validation.RemoveErrorHandler(this, ErrorChangedHandler);
        }
        #endregion

        #region SetVisibilityTabItem
        /// <summary>
        /// To set status of TabItem when VisibilityTabItem change value.
        /// </summary>
        /// <param name="value"></param>
        protected void SetVisibilityTabItem(Visibility value)
        {
            this.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                try
                                {
                                    this.Visibility = (Visibility)value;
                                    if ((this.Visibility == Visibility.Hidden || this.Visibility == Visibility.Collapsed)
                                        && this.IsSelected)
                                    {
                                        var item = (this.Parent as CPCTabControl).Items.OfType<TabItem>().Where(x => (x as TabItem).Visibility == Visibility.Visible);
                                        if (item != null && !item.ToList()[0].IsSelected)
                                            item.ToList()[0].IsSelected = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.Write("SetTabItemControl" + ex.ToString());
                                }

                            });
        }
        #endregion
        #endregion

    }
}

