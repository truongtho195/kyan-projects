﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;

namespace CPC.Toolkit.Behavior
{
    public static class FocusBehaviour
    {
        //public static readonly DependencyProperty IsFocusedProperty =
        //    DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(FocusBehaviour), new FrameworkPropertyMetadata(IsFocusedChanged));

        //public static bool? GetIsFocused(DependencyObject element)
        //{
        //    if (element == null)
        //    {
        //        throw new ArgumentNullException("element");
        //    }

        //    return (bool?)element.GetValue(IsFocusedProperty);
        //}

        //public static void SetIsFocused(DependencyObject element, bool? value)
        //{
        //    if (element == null)
        //    {
        //        throw new ArgumentNullException("element");
        //    }

        //    element.SetValue(IsFocusedProperty, value);
        //}

        //private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var fe = (FrameworkElement)d;

        //    if (e.OldValue == null)
        //    {
        //        fe.GotFocus += FrameworkElement_GotFocus;
        //        fe.LostFocus += FrameworkElement_LostFocus;
        //    }

        //    if (!fe.IsVisible)
        //    {
        //        fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(fe_IsVisibleChanged);
        //    }

        //    if ((bool)e.NewValue)
        //    {
        //        fe.Focus();
        //    }
        //}

        //private static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var fe = (FrameworkElement)sender;
        //    if (fe.IsVisible && (bool)((FrameworkElement)sender).GetValue(IsFocusedProperty))
        //    {
        //        fe.IsVisibleChanged -= fe_IsVisibleChanged;
        //        fe.Focus();
        //    }
        //}

        //private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        //}

        //private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        //}

        #region Dependency Properties
        /// <summary>
        /// <c>IsFocused</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool?),
                typeof(FocusBehaviour), new FrameworkPropertyMetadata(IsFocusedChanged));
        /// <summary>
        /// Gets the <c>IsFocused</c> property value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Value of the <c>IsFocused</c> property or <c>null</c> if not set.</returns>
        public static bool? GetIsFocused(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool?)element.GetValue(IsFocusedProperty);
        }
        /// <summary>
        /// Sets the <c>IsFocused</c> property value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        public static void SetIsFocused(DependencyObject element, bool? value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(IsFocusedProperty, value);
        }
        #endregion Dependency Properties

        #region Event Handlers
        /// <summary>
        /// Determines whether the value of the dependency property <c>IsFocused</c> has change.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Ensure it is a FrameworkElement instance.
            var fe = d as FrameworkElement;
            //if (fe != null && e.OldValue != null && e.NewValue != null && (bool)e.NewValue)
            //{
            //    // Attach to the Loaded event to set the focus there. If we do it here it will
            //    // be overridden by the view rendering the framework element.
            //    //fe.Loaded += FrameworkElementLoaded;

                
            //}

            if (fe != null && e.NewValue != null && (bool)e.NewValue)
            {
                fe.Focus();
                Keyboard.Focus(fe);
                
            }
        }
        /// <summary>
        /// Sets the focus when the framework element is loaded and ready to receive input.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private static void FrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure it is a FrameworkElement instance.
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                // Remove the event handler registration.
                fe.Loaded -= FrameworkElementLoaded;
                // Set the focus to the given framework element.
                fe.Focus();
                //Dispatcher.CurrentDispatcher.Invoke(new Action(() => { fe.Focus(); }), DispatcherPriority.ApplicationIdle);
                //System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => { fe.Focus(); }));
              
            }
        }
        #endregion Event Handlers
    }
}
