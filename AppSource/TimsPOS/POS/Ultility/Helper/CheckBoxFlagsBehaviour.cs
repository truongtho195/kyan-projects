using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Threading;
using System.Threading;

namespace CPC.Helper
{
    class CheckBoxFlagsBehaviour
    {
        private static bool isValueChanging;

        public static Enum GetMask(DependencyObject obj)
        {
            return (Enum)obj.GetValue(MaskProperty);
        } // end GetMask

        public static void SetMask(DependencyObject obj, Enum value)
        {
            obj.SetValue(MaskProperty, value);
        } // end SetMask

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.RegisterAttached("Mask", typeof(Enum),
            typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(null));



        public static int GetMask2(DependencyObject obj)
        {
            return (int)obj.GetValue(Mask2Property);
        }

        public static void SetMask2(DependencyObject obj, string value)
        {
            obj.SetValue(Mask2Property, value);
        }

        // Using a DependencyProperty as the backing store for Mask2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Mask2Property =
            DependencyProperty.RegisterAttached("Mask2", typeof(int), typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(null));



        public static byte GetValue(DependencyObject obj)
        {
            return (byte)obj.GetValue(ValueProperty);
        } // end GetValue

        public static void SetValue(DependencyObject obj, byte value)
        {
            obj.SetValue(ValueProperty, value);
        } // end SetValue

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached("Value", typeof(byte),
            typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(default(byte), ValueChanged));



        public static int GetValue2(DependencyObject obj)
        {
            return (int)obj.GetValue(Value2Property);
        }

        public static void SetValue2(DependencyObject obj, int value)
        {
            obj.SetValue(Value2Property, value);
        }

        // Using a DependencyProperty as the backing store for Value2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.RegisterAttached("Value2", typeof(int), typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(0, ValueChanged));


        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CheckBox).Dispatcher.BeginInvoke(
                          DispatcherPriority.Input,
                          (ThreadStart)delegate
                          {
                              isValueChanging = true;
                              byte mask = Convert.ToByte(GetMask2(d));
                              byte value = Convert.ToByte(e.NewValue);

                              BindingExpression exp = BindingOperations.GetBindingExpression(d, IsCheckedProperty);
                              if (exp != null)
                              {
                                  object dataItem = GetUnderlyingDataItem(exp.DataItem);
                                  PropertyInfo pi = dataItem.GetType().GetProperty(exp.ParentBinding.Path.Path);
                                  pi.SetValue(dataItem, (value & mask) != 0, null);

                                  if (value == 129)
                                      ((CheckBox)d).IsChecked = null;
                                  else
                                      ((CheckBox)d).IsChecked = (value & mask) != 0;
                              }
                              isValueChanging = false;
                          });

        } // end ValueChanged

        public static bool? GetIsChecked(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsCheckedProperty);
        } // end GetIsChecked

        public static void SetIsChecked(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsCheckedProperty, value);
        } // end SetIsChecked

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.RegisterAttached("IsChecked", typeof(bool?),
            typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(false, IsCheckedChanged));

        private static void IsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (isValueChanging) return;

            bool? isChecked = (bool?)e.NewValue;
            if (isChecked != null)
            {
                BindingExpression exp = BindingOperations.GetBindingExpression(d, Value2Property);
                object dataItem = GetUnderlyingDataItem(exp.DataItem);
                //PropertyInfo pi = dataItem.GetType().GetProperty(exp.ParentBinding.Path.Path);
                PropertyInfo pi = dataItem.GetType().GetProperty("DataContext.SelectedPromotion");
                //PropertyInfo pi = dataItem.GetType().GetProperty("Value");

                byte mask = Convert.ToByte(GetMask2(d));
                byte value = Convert.ToByte(pi.GetValue(dataItem, null));

                if (isChecked.Value)
                {
                    if ((value & mask) == 0)
                    {
                        value = (byte)(value + mask);
                    }
                }
                else
                {
                    if ((value & mask) != 0)
                    {
                        value = (byte)(value - mask);
                    }
                }

                pi.SetValue(dataItem, value, null);
            }
        } // end IsCheckedChanged

        private static object GetUnderlyingDataItem(object o)
        {
            return o is DataRowView ? ((DataRowView)o).Row : o;
        } // end GetUnderlyingDataItem
    }
}
