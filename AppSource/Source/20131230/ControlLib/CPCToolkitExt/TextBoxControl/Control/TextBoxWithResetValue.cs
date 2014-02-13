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
using System.Windows.Controls;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxWithResetValue : DelayTextBox
    {
        //Ctor
        public TextBoxWithResetValue()
        {

        }

        /// <summary>
        /// To get button on TextBox stule.
        /// </summary>
        private Button _btnClearValue;
        /// <summary>
        /// To get value when Button is visible.
        /// </summary>
        private bool _isVisibleButton;

        //To override event OnTextChanged of TextBOx.
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            //To show button on TextBox.
            base.OnTextChanged(e);
            if (this.Text.Length > 0
                && !this._isVisibleButton
                && this._btnClearValue != null)
            {
                this._isVisibleButton = true;
                this._btnClearValue.Visibility = Visibility.Visible;
            }
            else if (this.Text.Length == 0 && this._btnClearValue != null)
            {
                this._isVisibleButton = false;
                this._btnClearValue.Visibility = Visibility.Collapsed;
            }

        }
        //To override event OnApplyTemplate of TextBOx.
        public override void OnApplyTemplate()
        {
            //To find button on style.
            _btnClearValue = GetTemplateChild("BTN_ClearValue") as Button;
            ////To register event for button.
            if (_btnClearValue != null)
            {
                _btnClearValue.Visibility = Visibility.Collapsed;
                _btnClearValue.Click += new System.Windows.RoutedEventHandler(BtnClearValue_Click);
            }
            base.OnApplyTemplate();
        }

        //To clear value of TextBox.
        private void BtnClearValue_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!this.IsReadOnly && !string.IsNullOrEmpty(this.Text))
                this.Text = string.Empty;
        }
    }
}
