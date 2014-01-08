using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using CustomTextBox;

namespace CPC.Control
{
    delegate FrameworkElement DelegateFilterSelection(FilterComboBox combo);

    class FilterComboBox : ComboBox, INotifyPropertyChanged
    {
        #region Members
        public DelegateFilterSelection FilterSelection;

        public FrameworkElement KeywordElement;
        #endregion

        #region Dependency Properties
        public new FilterItemModel SelectedItem
        {
            get { return (FilterItemModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(FilterItemModel), typeof(FilterComboBox),
            new FrameworkPropertyMetadata(null,
                new PropertyChangedCallback(FilterComboBox.OnSelectedItemChanged)));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (null != e.OldValue)
            {
                (e.OldValue as FilterItemModel).IsSelected = false;
            }

            if (null != e.NewValue)
            {
                (e.NewValue as FilterItemModel).IsSelected = true;
            }
        }
        #endregion

        #region Events
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            // Must manually set the selectedItem for sender because we use the new keyword for SelectedItem property dependency
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is FilterItemModel)
            {
                if (this.KeywordElement is StatusComboBox)
                {
                    BindingOperations.ClearBinding(this.KeywordElement, StatusComboBox.SelectedItemsProperty);
                    BindingOperations.ClearBinding(this.KeywordElement, StatusComboBox.ItemsSourceProperty);
                }
                else if (this.KeywordElement is DateComboBox)
                {
                    BindingOperations.ClearBinding(this.KeywordElement, DateComboBox.FromDateProperty);
                    BindingOperations.ClearBinding(this.KeywordElement, DateComboBox.ToDateProperty);
                }
                else if (this.KeywordElement is TextBoxNumber)
                {
                    BindingOperations.ClearBinding(this.KeywordElement, TextBoxNumber.TextRealDependencyProperty);
                }
                else if (this.KeywordElement is TextBox)
                {
                    BindingOperations.ClearBinding(this.KeywordElement, TextBox.TextProperty);
                }

                this.SelectedItem = e.AddedItems[0] as FilterItemModel;
                if (this.FilterSelection != null)
                {
                    this.KeywordElement = this.FilterSelection.Invoke(this);
                }
            }

            e.Handled = true;
        }

        protected override void OnPreviewMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;

            base.OnPreviewKeyDown(e);
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }
        #endregion
    }
}
