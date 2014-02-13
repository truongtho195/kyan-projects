using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using CPCToolkitExt.TextBoxControl;
using System.Xml.Serialization;
using System.Linq.Expressions;
using System.Reflection;


namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxEmail : TextBox, INotifyPropertyChanged
    {
        #region Ctor
        public TextBoxEmail()
        {

        } 
        #endregion

        #region Properties

        /// <summary>
        /// To set value for IsValidation .It's True when control is errored.
        /// </summary>
        public bool IsValidation { get; set; }

        protected bool IsExecute { get; set; }

        /// <summary>
        /// To get, set value when control is errored. 
        /// </summary>
        private bool _isError;
        public bool IsError
        {
            get { return _isError; }
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    RaisePropertyChanged(() => IsError);
                }
            }
        }

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
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
        #endregion

        #region Event

        protected override void OnTextChanged(System.Windows.Controls.TextChangedEventArgs e)
        {
            this.IsExecute = true;
            base.OnTextChanged(e);
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern);
            if (reg.IsMatch(this.Text))
            {
                this.Value = this.Text;
                this.IsError = false;
            }
            else
            {
                if (!this.IsError)
                    this.Value = string.Empty;
                this.IsError = true;
            }
            this.IsExecute = false;
        }

        #endregion

        #region DependencyProperties
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(TextBoxEmail), new UIPropertyMetadata(string.Empty, ChangeValue));

        protected static void ChangeValue(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as TextBoxEmail).IsExecute)
            {
                (source as TextBoxEmail).ExecuteChangeValue(e.NewValue);
            }
        } 
        #endregion

        #region Methods
        public void ExecuteChangeValue(object value)
        {
            this.IsExecute = true;
            if (value != null && value.ToString().Length > 0)
                this.Text = value.ToString();
            else
                this.Text = string.Empty;
            this.IsExecute = false;
        } 
        #endregion

    }
}
