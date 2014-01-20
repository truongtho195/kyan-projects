using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.Obselete;

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
                (sender as FrameworkElement).GotFocus += new RoutedEventHandler(MoveFocusHelper_GotFocus);
            }
        }

        private static void MoveFocusHelper_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //To change focus when to click enter or Tab key.
                if (Keyboard.IsKeyDown(Key.Tab) || Keyboard.IsKeyDown(Key.Enter))
                {
                    (sender as FrameworkElement).Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                (ThreadStart)delegate
                                {
                                    //To get the element with keyboard focus.
                                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                                    //elementWithFocus.Focus();
                                    if (elementWithFocus is TextBox && !(elementWithFocus as TextBox).IsReadOnly)
                                        (elementWithFocus as TextBox).SelectAll();
                                });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------ComboBoxMoveFocusControl--- OnPreviewKeyDown ------------ \n" + ex.Message);
            }
        }
        private static void MoveFocusHelper_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                //When users press "Enter",the mouse will focus next control.
                if (e.Key == Key.Enter)
                {
                    //To get the element with keyboard focus.
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    FocusNavigationDirection focusDirection = new System.Windows.Input.FocusNavigationDirection();
                    focusDirection = System.Windows.Input.FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    //To change keyboard focus.
                    if (elementWithFocus != null)
                        elementWithFocus.MoveFocus(request);
                    //(sender as FrameworkElement).Dispatcher.BeginInvoke(
                    //           DispatcherPriority.Background,
                    //           (ThreadStart)delegate
                    //           {
                    //               //To get the element with keyboard focus.
                    //               UIElement element = Keyboard.FocusedElement as UIElement;
                    //               //elementWithFocus.Focus();
                    //               if (element is TextBox && !(element as TextBox).IsReadOnly)
                    //                   (element as TextBox).SelectAll();
                    //           });
                }
                //if (e.Key == Key.Tab)
                //{
                //    (sender as FrameworkElement).Dispatcher.BeginInvoke(
                //                DispatcherPriority.Background,
                //                (ThreadStart)delegate
                //                {
                //                    //To get the element with keyboard focus.
                //                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                //                    //elementWithFocus.Focus();
                //                    if (elementWithFocus is TextBox && !(elementWithFocus as TextBox).IsReadOnly)
                //                        (elementWithFocus as TextBox).SelectAll();
                //                });
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------ComboBoxMoveFocusControl--- OnPreviewKeyDown ------------ \n" + ex.Message);
            }
        }


        public static DependencyProperty FocusedProperty =
              DependencyProperty.RegisterAttached("Focused",
              typeof(bool),
              typeof(MoveFocusHelper),
              new UIPropertyMetadata(false, OnFocusedChanged));

        public static bool GetFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusedProperty);
        }

        public static void SetFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusedProperty, value);
        }

        public static void OnFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (sender is CPCToolkitExt.TextBoxControl.TextBox)
                {
                    object elementWithFocus = Keyboard.GetKeyStates(Key.Tab);
                    CPCToolkitExt.TextBoxControl.TextBox textBox = sender as CPCToolkitExt.TextBoxControl.TextBox;
                    if (MoveFocusHelper.GetFocused(textBox))
                        textBox.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                (ThreadStart)delegate
                                {
                                    textBox.SelectAll();
                                });
                    else
                        textBox.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                (ThreadStart)delegate
                                {
                                    textBox.SelectionLength = 0;
                                });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}
