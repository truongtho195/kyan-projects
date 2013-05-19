﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System;
using System.Linq;
using System.ComponentModel;
using CPC.Control;

namespace CPC.Toolkit.Behavior
{
    public class ContainerViewClosingBehavior
    {
        public static ICommand GetClosed(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClosedProperty);
        }

        public static void SetClosed(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClosedProperty, value);
        }

        public static readonly DependencyProperty ClosedProperty
                                                                = DependencyProperty.RegisterAttached(
                                                                "Closed", typeof(ICommand), typeof(ContainerViewClosingBehavior),
                                                                new UIPropertyMetadata(new PropertyChangedCallback(ClosedChanged)));

        private static void ClosedChanged(
          DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ContainerView window = target as ContainerView;

            if (window != null)
            {
                if (e.NewValue != null)
                {
                    window.ClosedEventHandlerDelegate += Window_Closed;
                }
                else
                {
                    window.ClosedEventHandlerDelegate -= Window_Closed;
                }
            }
        }

        public static ICommand GetClosing(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClosingProperty);
        }

        public static void SetClosing(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClosingProperty, value);
        }

        public static readonly DependencyProperty ClosingProperty
                                                                = DependencyProperty.RegisterAttached(
                                                                "Closing", typeof(ICommand), typeof(ContainerViewClosingBehavior),
                                                                new UIPropertyMetadata(new PropertyChangedCallback(ClosingChanged)));

        private static void ClosingChanged(
          DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ContainerView window = target as ContainerView;

            if (window != null)
            {
                if (e.NewValue != null)
                {
                    window.ClosingEventHandlerDelegate += Window_Closing;
                }
                else
                {
                    window.ClosingEventHandlerDelegate -= Window_Closing;
                }
            }
        }

        public static ICommand GetCancelClosing(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CancelClosingProperty);
        }

        public static void SetCancelClosing(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CancelClosingProperty, value);
        }

        public static readonly DependencyProperty CancelClosingProperty
                                                                        = DependencyProperty.RegisterAttached(
                                                                        "CancelClosing", typeof(ICommand), typeof(ContainerViewClosingBehavior));

        static void Window_Closed(object sender, EventArgs e)
        {
            ICommand closed = GetClosed((sender as ContainerView).grdContent.Children[0]);
            if (closed != null)
            {
                closed.Execute(null);
            }
        }

        static void Window_Closing(object sender, CancelEventArgs e)
        {
            ICommand closing = GetClosing(sender as ContainerView);
            if (closing != null)
            {
                if (closing.CanExecute(null))
                {
                    closing.Execute(null);
                }
                else
                {
                    ICommand cancelClosing = GetCancelClosing((sender as ContainerView).grdContent.Children[0]);
                    if (cancelClosing != null)
                    {
                        cancelClosing.Execute(null);
                    }

                    e.Cancel = true;
                }
            }
        }
    }
}
