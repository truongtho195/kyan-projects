using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Data;

namespace CPCToolkitExt.DataGridControl
{
    public class DataGridColumnExtensions
    {

        #region FieldName
        public static DependencyProperty FieldNameProperty =
          DependencyProperty.RegisterAttached("FieldName",
              typeof(string), typeof(DataGridColumn), new PropertyMetadata(string.Empty));

        public static string GetFieldName(DependencyObject target)
        {
            return (string)target.GetValue(FieldNameProperty);
        }

        public static void SetFieldName(DependencyObject target, string value)
        {
            target.SetValue(FieldNameProperty, value);
        }
        #endregion

        #region DisplayType
        /// <summary>
        /// To get, set control displaying in DataGridColunmHeader.
        /// </summary>
        public static DependencyProperty DisplayTypeProperty =
          DependencyProperty.RegisterAttached("DisplayType",
              typeof(DisplayType), typeof(DataGridColumn),
              new PropertyMetadata(DisplayType.None));

        public static DisplayType GetDisplayType(DependencyObject target)
        {
            return (DisplayType)target.GetValue(DisplayTypeProperty);
        }

        public static void SetDisplayType(DependencyObject target, DisplayType value)
        {
            target.SetValue(DisplayTypeProperty, value);
        }
        #endregion

        #region FilterType
        /// <summary>
        /// To get filter type of DataGridColumn.
        /// </summary>
        public static DependencyProperty FilterTypeProperty =
          DependencyProperty.RegisterAttached("FilterType",
              typeof(FilterType), typeof(DataGridColumn),
              new PropertyMetadata(FilterType.Text));

        public static FilterType GetFilterType(DependencyObject target)
        {
            return (FilterType)target.GetValue(FilterTypeProperty);
        }

        public static void SetFilterType(DependencyObject target, FilterType value)
        {
            target.SetValue(FilterTypeProperty, value);
        }
        #endregion

        #region IsFilter
        public static DependencyProperty IsFilterProperty =
          DependencyProperty.RegisterAttached("IsFilter",
              typeof(bool), typeof(DataGridColumn), new PropertyMetadata(false));

        public static bool GetIsFilter(DependencyObject target)
        {
            return (bool)target.GetValue(IsFilterProperty);
        }

        public static void SetIsFilter(DependencyObject target, bool value)
        {
            target.SetValue(IsFilterProperty, value);
        }
        #endregion

        #region FilterLevel
        //To get filter level of DataGridColumn.
        public static DependencyProperty FilterLevelProperty =
          DependencyProperty.RegisterAttached("FilterLevel",
              typeof(int), typeof(DataGridColumn),
              new PropertyMetadata(0));

        public static int GetFilterLevel(DependencyObject target)
        {
            return (int)target.GetValue(FilterLevelProperty);
        }
        public static void SetFilterLevel(DependencyObject target, int value)
        {
            target.SetValue(FilterLevelProperty, value);
        }
        #endregion

        #region DisplayMemberPath

        public static DependencyProperty DisplayMemberPathProperty =
         DependencyProperty.RegisterAttached("DisplayMemberPath",
             typeof(string), typeof(DataGridColumn), new PropertyMetadata(string.Empty));

        public static string GetDisplayMemberPath(DependencyObject target)
        {
            return (string)target.GetValue(DisplayMemberPathProperty);
        }

        public static void SetDisplayMemberPath(DependencyObject target, string value)
        {
            target.SetValue(DisplayMemberPathProperty, value);
        }
        #endregion

        #region SelectedValuePath
        public static DependencyProperty SelectedValuePathProperty =
        DependencyProperty.RegisterAttached("SelectedValuePath",
            typeof(string), typeof(DataGridColumn), new PropertyMetadata(string.Empty));

        public static string GetSelectedValuePath(DependencyObject target)
        {
            return (string)target.GetValue(SelectedValuePathProperty);
        }

        public static void SetSelectedValuePath(DependencyObject target, string value)
        {
            target.SetValue(SelectedValuePathProperty, value);
        }

        #endregion

        #region Name
        public static DependencyProperty NameProperty =
          DependencyProperty.RegisterAttached("Name",
              typeof(object), typeof(DataGridColumn), new PropertyMetadata(null));
        public static object GetName(DependencyObject target)
        {
            return (object)target.GetValue(NameProperty);
        }

        public static void SetName(DependencyObject target, object value)
        {
            target.SetValue(NameProperty, value);
        }
        #endregion
    }

    public class BindingHelper
    {
        public static DependencyProperty ItemsSourceProperty =
           DependencyProperty.RegisterAttached("ItemsSource",
               typeof(object), typeof(BindingHelper)
               , new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static object GetItemsSource(DependencyObject target)
        {
            return (object)target.GetValue(ItemsSourceProperty);
        }

        public static void SetItemsSource(DependencyObject target, object value)
        {
            target.SetValue(ItemsSourceProperty, value);
        }
    }

    public class DataGridTemplateColumnExt : DataGridTemplateColumn
    {
        public object Tag
        {
            get { return (object)GetValue(TagProperty); }
            set { SetValue(TagProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Tag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagProperty =
            DependencyProperty.Register("Tag", typeof(object), typeof(DataGridTemplateColumnExt), new UIPropertyMetadata(null));

    }
}
