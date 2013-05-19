using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.ComponentModel;

namespace POS.Control
{
    //FocusManager.IsFocusScope="{Binding IsFocused,RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type DataGridCell}}}"
    //Visibility="{Binding IsFocused,RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type DataGridCell}},Converter={StaticResource errorContentConverter}}"
    public class DataGridValidationControl : DataGrid
    {

        #region Constructors
        public DataGridValidationControl()
        {
            Validation.AddErrorHandler(this, ErrorChangedHandler);
            TypeDescriptor.GetProperties(this)["ItemsSource"].AddValueChanged(this, new EventHandler(ListView_ItemsSourceChanged));
        }
        #endregion

        #region Fields
        public readonly HashSet<ValidationError> Errors = new HashSet<ValidationError>();
        #endregion

        #region Methods
        private void ListView_ItemsSourceChanged(object sender, EventArgs e)
        {
            // This doesn't get fired 
        }
        private void DataGridControl_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Errors.Count > 0)
                e.Handled = false;
            else
                e.Handled = true;
        }
        private void ErrorChangedHandler(object sender, ValidationErrorEventArgs e)
        {
            if ((e.Error.BindingInError as BindingExpression).HasError && e.Action == ValidationErrorEventAction.Added)
                Errors.Add(e.Error);
            else
                Errors.Remove(e.Error);

            if (this.Errors.Count > 0)
            {
                BindingExpression binding = e.Error.BindingInError as BindingExpression;
                DataGridRowExt datagrd = (DataGridRowExt)this.ItemContainerGenerator.ContainerFromItem(binding.DataItem);
                if (datagrd != null)
                {
                    datagrd.IsEnabled = true;
                    DataGridCell cell = new DataGridCell();
                    foreach (var item in this.ItemsSource)
                    {
                        DataGridRowExt datagrid = (DataGridRowExt)this.ItemContainerGenerator.ContainerFromItem(item);
                        if (datagrd != datagrid)
                        {
                            datagrid.IsEnabled = false;

                        }
                    }
                    datagrd.IsVisibleChanged += new DependencyPropertyChangedEventHandler(datagrid_IsVisibleChanged);
                }
            }
            else
            {
                foreach (var item in this.ItemsSource)
                {
                    DataGridRowExt datagrid = (DataGridRowExt)this.ItemContainerGenerator.ContainerFromItem(item);
                    datagrid.IsEnabled = true;
                    datagrid.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(datagrid_IsVisibleChanged);
                }
            }
        }
        private void datagrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue && this.ItemsSource != null)
            {
                foreach (var item in this.ItemsSource)
                {
                    DataGridRowExt datagrid = (DataGridRowExt)this.ItemContainerGenerator.ContainerFromItem(item);
                    if (datagrid != null)
                        datagrid.IsEnabled = true;
                }
            }
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DataGridRowExt();
        }
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is DataGridRowExt;
        }
        #endregion

    }

    public class DataGridRowExt : DataGridRow
    {

        #region Constructors
        public DataGridRowExt()
        {
            this.TextInput += new System.Windows.Input.TextCompositionEventHandler(DataGridRowExt_TextInput);
            this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(DataGridRowExt_PreviewKeyDown);
        }
        #endregion

        #region Events
        private void DataGridRowExt_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!this.IsEditingData)
                this.IsEditingData = true;
        }

        private void DataGridRowExt_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
        }

        #endregion

        #region Dependency Properties
        public bool IsEditingData
        {
            get { return (bool)GetValue(IsEditingDataProperty); }
            set { SetValue(IsEditingDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditingData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditingDataProperty =
            DependencyProperty.Register("IsEditingData", typeof(bool), typeof(DataGridRowExt), new UIPropertyMetadata(false));
        #endregion

    }

    public class ErrorContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !bool.Parse(value.ToString())) return System.Windows.Visibility.Hidden;
            return System.Windows.Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values == null) return Visibility.Collapsed;
                if (bool.Parse(values[1].ToString()) && values[0].ToString() == "Collapsed")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
