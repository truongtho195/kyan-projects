using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxNumber : TextBoxNumberBase
    {
        #region Contrustor
        public TextBoxNumber()
        {
            this.AcceptsReturn = false;
            this.Loaded += new RoutedEventHandler(TextBoxNumber_Loaded);
        }

        #endregion

        #region Event Control
        private void TextBoxNumber_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.MaxLength == 0)
                this.MaxLength = 9;

            this.Dispatcher.BeginInvoke(
                          DispatcherPriority.Input,
                          (ThreadStart)delegate
                          {
                              if (this.Value == null || string.IsNullOrEmpty(this.Value))
                              {
                                  this.Text = "0";
                              }
                          });
        }
        #endregion

        #region Override Events
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
            {
                e.Handled = true;
                return;
            }
            int i = CaretIndex;
            switch (e.Key)
            {
                ///Block space key.
                case Key.Space:
                    e.Handled = true;
                    break;

                ///Delete key
                case Key.Delete:
                    if (string.IsNullOrEmpty(this.Text)
                        || (this.SelectionLength > 0
                        && this.SelectionLength < this.Text.Length))
                    {
                        e.Handled = true;
                        break;
                    }
                    else if (SelectionLength == Text.Length)
                    {
                        Value = "0";
                        Text = string.Empty;
                        break;
                    }
                    else
                    {
                        if (i >= Text.Length) break;
                        string text = Text.Remove(i, 1);
                        if (string.IsNullOrEmpty(text))
                            this.Value = "0";
                        else
                            this.Value = text;
                        break;
                    }

                ///Back key
                case Key.Back:
                    if (string.IsNullOrEmpty(this.Text)
                        || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
                    {
                        e.Handled = true;
                        break;

                    }
                    else if (SelectionLength == Text.Length)
                    {
                        this.Value = "0";
                        this.Text = string.Empty;
                        break;
                    }
                    else
                    {
                        if (i <= 0) break;
                        i--;
                        string text = Text.Remove(i, 1);
                        if (string.IsNullOrEmpty(text))
                            this.Value = "0";
                        else
                            this.Value = text;
                        break;
                    }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
            {
                return;
            }
            if (e.Text.Length == 0
                || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length)
                || (this.Text.Length == this.MaxLength && this.SelectionLength == 0))
            {
                e.Handled = true;
                return;
            }
            else if (this.SelectionLength > 0
                && this.SelectionLength == this.Text.Length
                && this.IsNumber(e.Text))
            {
                this.Text = string.Empty;
                this.Value = "0";
            }

            if (!this.IsNumber(e.Text))
                e.Handled = true;
            else
            {
                this.Value = Text.Insert(CaretIndex, e.Text);
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
                return;

            if (string.IsNullOrEmpty(this.Value)
                || this.Value.Equals("0"))
            {
                this.Text = String.Empty;
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.Value)
                || this.Value.Equals("0"))
            {
                this.Text = "0";
            }
            base.OnLostFocus(e);
        }
        #endregion

        #region DependencyProperties

        #region ValueDefult
        public int ValueDefault
        {
            get { return (int)GetValue(ValueDefaultProperty); }
            set { SetValue(ValueDefaultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueDefult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueDefaultProperty =
            DependencyProperty.Register("ValueDefault", typeof(int), typeof(TextBoxNumber), new UIPropertyMetadata(0));


        #endregion

        #endregion

        #region Override Methods
        public override void SetValueFormat(string content)
        {
            try
            {
                if (this.IsFocused) return;
                if (!string.IsNullOrEmpty(content))
                {
                    this.Text = content;
                }
                else
                {
                    this.Text = "0";
                }
            }
            catch (Exception ex)
            {
                this.Text = "0";
                Debug.Write("<<<<<<<<<<<<<<<<<<SetValueFormat>>>>>>>>>>>>>>>" + ex.ToString());

            }
            base.SetValueFormat(content);
        }

        public override void SetValueDefault()
        {
            this.Text = "0";
            base.SetValueDefault();
        }
        #endregion

    }
}
