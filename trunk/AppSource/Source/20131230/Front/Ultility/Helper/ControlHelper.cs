using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace CPC.Helper
{
    public class ControlHelper
    {
        #region DoubleClickCommand

        public static ICommand GetDoubleClickCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DoubleClickCommandProperty);
        }

        public static void SetDoubleClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DoubleClickCommandProperty, value);
        }

        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommand", typeof(ICommand), typeof(ControlHelper), new UIPropertyMetadata(null, DoubleClickCommandChanged));

        private static void DoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.Control control = d as System.Windows.Controls.Control;
            if (control != null)
            {
                control.MouseDoubleClick += new MouseButtonEventHandler(ControlMouseDoubleClick);
            }
        }

        private static void ControlMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Control control = sender as System.Windows.Controls.Control;
            ICommand command = GetDoubleClickCommand(control);
            object commandParameter = GetDoubleClickCommandParameter(control);
            command.Execute(commandParameter);
        }

        #endregion

        #region DoubleClickCommandParameter

        public static object GetDoubleClickCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(DoubleClickCommandParameterProperty);
        }

        public static void SetDoubleClickCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(DoubleClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty DoubleClickCommandParameterProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommandParameter", typeof(object), typeof(ControlHelper), new UIPropertyMetadata(null));

        #endregion
    }
}
