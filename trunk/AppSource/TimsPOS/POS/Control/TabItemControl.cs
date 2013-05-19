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

namespace CPC.Control
{
    public class TabItemControl : TabItem
    {
        #region TabItemControl
        public TabItemControl()
        {
            this.Loaded += new RoutedEventHandler(TabItemControl_Loaded);
        }
        #endregion

        #region Envent Control

        private void TabItemControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.IsVisibleError)
                    Validation.AddErrorHandler(this, ErrorChangedHandler);
                if (this.IsSelected)
                    this.Focus();
            }
            catch (Exception ex)
            {
                Debug.Write("TabItemControl_Loaded" + ex.ToString());
            }

        }

        /// <summary>
        /// Event get errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorChangedHandler(object sender, ValidationErrorEventArgs e)
        {
            try
            {
                ValidationErrorExt ValidationErrorExt = new ValidationErrorExt();
                ValidationErrorExt.ValidationError = e.Error;
                //Add Source for ValidationErrorExt
                if (this.VisibilityErrorWhenChangeData != null && (e.OriginalSource as Control.TabItemControl).Tag != null)
                    ValidationErrorExt.OfTheContent = (e.OriginalSource as Control.TabItemControl).Tag;
                //Add error
                if (e.Action == ValidationErrorEventAction.Added)
                    Errors.Add(ValidationErrorExt);
                //Remove error
                else
                    Errors.RemoveWhere(x => x.ValidationError == e.Error);
                //Set value for VisibilityError
                if (this.VisibilityErrorWhenChangeData != null)
                    this.SetIsValid(this.VisibilityErrorWhenChangeData);
                else
                {
                    //Set value for VisibilityError
                    if (Errors.Count > 0)
                        this.VisibilityError = Visibility.Visible;
                    else
                        this.VisibilityError = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("ErrorChangedHandler" + ex.ToString() + "\n");
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Collection contain all errors in TabItem
        /// </summary>
        public readonly HashSet<ValidationErrorExt> Errors = new HashSet<ValidationErrorExt>();

        #endregion

        #region DependencyProperty

        /// <summary>
        /// Visible Error in TabItem
        /// </summary>
        public Visibility VisibilityError
        {
            get { return (Visibility)GetValue(VisibilityErrorProperty); }
            set { SetValue(VisibilityErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibilityError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityErrorProperty =
            DependencyProperty.Register("VisibilityError", typeof(Visibility), typeof(TabItemControl), new PropertyMetadata(Visibility.Collapsed));

        public object SelectedValueChanged
        {
            get { return (object)GetValue(SelectedValueChangedProperty); }
            set { SetValue(SelectedValueChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValueChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValueChangedProperty =
            DependencyProperty.Register("SelectedValueChanged", typeof(object), typeof(TabItemControl),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedValueChanged)));

        public bool IsVisibleError
        {
            get { return (bool)GetValue(IsVisibleErrorProperty); }
            set { SetValue(IsVisibleErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCheckErrorContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVisibleErrorProperty =
            DependencyProperty.Register("IsVisibleError", typeof(bool), typeof(TabItemControl), new UIPropertyMetadata(false));

        public Visibility VisibilityTabItemByData
        {
            get { return (Visibility)GetValue(VisibilityTabItemByDataProperty); }
            set { SetValue(VisibilityTabItemByDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityTabItemByDataProperty =
            DependencyProperty.Register("VisibilityTabItemByData", typeof(Visibility), typeof(TabItemControl), new PropertyMetadata(Visibility.Visible, OnValueVisibilityChanged));

        protected static void OnValueVisibilityChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as TabItemControl).SetTabItemControl((Visibility)e.NewValue);
        }

        public object VisibilityErrorWhenChangeData
        {
            get { return (object)GetValue(VisibilityErrorWhenChangeDataProperty); }
            set { SetValue(VisibilityErrorWhenChangeDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsValidErrorWithControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityErrorWhenChangeDataProperty =
            DependencyProperty.Register("VisibilityErrorWhenChangeData", typeof(object), typeof(TabItemControl), new UIPropertyMetadata(null, OnChanged));

        protected static void OnChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as TabItemControl).SetIsValid(e.NewValue);
        }

        #endregion

        #region Methods

        protected static void OnSelectedValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                //if ((source as TabItemControl).Tag != null 
                //    && !bool.Parse((source as TabItemControl).Tag.ToString()))
                //{
                //(source as TabItemControl).Errors.Clear();
                //(source as TabItemControl).VisibilityError = Visibility.Collapsed;
                //}
            }
        }

        protected void SetTabItemControl(Visibility value)
        {
            this.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                try
                                {
                                    //Set Visibility
                                    this.Visibility = (Visibility)value;
                                    //Set selecteditem for TabControl
                                    if (this.Visibility == Visibility.Collapsed && this.IsSelected)
                                    {
                                        var item = (this.Parent as TabControl).Items.OfType<TabItem>().Where(x => (x as TabItem).Visibility == Visibility.Visible);
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

        protected void SetIsValid(object value)
        {
            try
            {
                if (this.Errors.Count > 0
                    && this.Errors.Where(x => x.OfTheContent != null && x.OfTheContent.ToString().Equals(value.ToString())).Count() > 0)
                    this.VisibilityError = Visibility.Visible;
                else
                    this.VisibilityError = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.Write("SetIsValid" + ex.ToString());
            }
        }
        #endregion
    }

    public class ValidationErrorExt
    {
        /// <summary>
        /// Gets or sets the ValidationError
        /// </summary>
        public ValidationError ValidationError
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the OfTheContent
        /// </summary>
        public object OfTheContent
        {
            get;
            set;
        }
    }
}

