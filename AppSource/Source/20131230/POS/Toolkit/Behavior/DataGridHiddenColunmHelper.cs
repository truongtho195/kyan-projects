using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using CPCToolkitExt.DataGridControl;

namespace CPC.Toolkit.Behavior
{
    public class DataGridHiddenColunmHelper
    {
        #region ColunmIndex
        public static DependencyProperty ColunmIndexProperty =
            DependencyProperty.RegisterAttached("ColunmIndex",
            typeof(int),
            typeof(DataGridHiddenColunmHelper),
            new UIPropertyMetadata(-1));

        public static int GetColunmIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(ColunmIndexProperty);
        }

        public static void SetColunmIndex(DependencyObject obj, int value)
        {
            obj.SetValue(ColunmIndexProperty, value);
        }
        #endregion

        #region IsHiddenColunm
        public static DependencyProperty IsHiddenColunmProperty =
            DependencyProperty.RegisterAttached("IsHiddenColunm",
            typeof(bool),
            typeof(DataGridHiddenColunmHelper),
            new UIPropertyMetadata(false, OnIsHiddenColunmChanged));

        public static bool GetIsHiddenColunm(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHiddenColunmProperty);
        }

        public static void SetIsHiddenColunm(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHiddenColunmProperty, value);
        }
        public static void OnIsHiddenColunmChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (sender is DataGridControl)
                {
                    DataGridControl dataGridControl = sender as DataGridControl;
                    int colunmIndex = DataGridHiddenColunmHelper.GetColunmIndex(dataGridControl);
                    if (e.NewValue is Boolean && Boolean.Parse(e.NewValue.ToString()))
                        dataGridControl.Columns[colunmIndex].Visibility = Visibility.Collapsed;
                    else
                        dataGridControl.Columns[colunmIndex].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnIsHiddenColunmChanged" + ex.ToString());
            }
        }
        #endregion

    }
}
