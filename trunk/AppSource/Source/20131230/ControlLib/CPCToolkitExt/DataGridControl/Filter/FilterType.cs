using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CPCToolkitExt.DataGridControl;

namespace CPCToolkitExt.DataGridControl
{
    /// <summary>
    /// Corresponds to the FilterCurrentData templates (DataTemplate) 
    /// of the DataGridColumnFilter defined in the Generic.xaml>
    /// </summary>
    public enum FilterType
    {
        None,
        Numeric,
        Text,
        Boolean,
        DateTime
    }
    /// <summary>
    /// To define value of control displying in DataGridColunmHeader.
    /// </summary>
    public enum DisplayType
    {
        None,
        TextBox,
        ComboBox,
        DateTimePicker
    }
    /// <summary>
    ///To get data for FilterCondition 
    /// </summary>
    public class FilterCondition
    {
        public string FieldName { get; set; }
        public string Content { get; set; }
        public FilterType FilterType { get; set; }
        public FrameworkElement Control { get; set; }
        public int Level { get; set; }
    }
}
