﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace CPC.Toolkit.Behavior
{
    public class MoveFocusHelper
    {
        public static DependencyProperty MoveFocusProperty =
             DependencyProperty.RegisterAttached("MoveFocus",
             typeof(bool),
             typeof(MoveFocusHelper),
             new UIPropertyMetadata(false, OnMoveFocusChanged));

        public static bool GetMoveFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(MoveFocusProperty);
        }

        public static void SetMoveFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(MoveFocusProperty, value);
        }
        public static void OnMoveFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                (sender as FrameworkElement).KeyDown += new System.Windows.Input.KeyEventHandler(MoveFocusHelper_PreviewKeyDown);
            }
        }

        static void MoveFocusHelper_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                //When users press "Enter",the mouse will focus next control.
                if (e.Key == Key.Enter)
                {
                    FocusNavigationDirection focusDirection = new System.Windows.Input.FocusNavigationDirection();
                    focusDirection = System.Windows.Input.FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    //To get the element with keyboard focus.
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    //To change keyboard focus.
                    if (elementWithFocus != null)
                    {
                        elementWithFocus.MoveFocus(request);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------ComboBoxMoveFocusControl--- OnPreviewKeyDown ------------ \n" + ex.Message);
            }
        }
    }
}