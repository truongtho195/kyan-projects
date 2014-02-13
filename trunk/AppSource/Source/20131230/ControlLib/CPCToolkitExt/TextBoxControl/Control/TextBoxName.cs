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
using System.Globalization;
using System.Diagnostics;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxName : TextBox
    {
        #region Contrustor

        public TextBoxName()
        {
            this.Loaded += new RoutedEventHandler(TextBoxName_Loaded);
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.CharacterCasing = CharacterCasing.Normal;
            //    this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            //    this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
        }

        void TextBoxName_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.MaxLength == 0)
                this.MaxLength = 30;
        }

        #endregion

        #region Override Methods

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            object a = Encoding.Default.GetEncoder().ToString();
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
            {
                e.Handled = true;
                return;
            }
            switch (e.Key)
            {
                case Key.Space:
                    if (this.CaretIndex > 0)
                    {
                        StringBuilder text;// = new StringBuilder(string.Empty);
                        if (this.Text.Length > this.CaretIndex)
                        {
                            int index = this.CaretIndex;
                            text = new StringBuilder((this.Text.Insert(index, " ")));
                            text = text.Replace(text[index + 1], char.Parse(text[index + 1].ToString().ToUpper()), index + 1, 1);
                            this.Text = text.ToString();
                            this.SelectionStart = index + 1;
                            e.Handled = true;
                        }
                    }
                    else
                        e.Handled = true;
                    break;

                case Key.Enter:
                    e.Handled = true;
                    break;

                case Key.Delete:

                    try
                    {
                        if (String.IsNullOrEmpty(this.Text)
                           || !this.IsToUpper
                           || this.CaretIndex == this.Text.Length) break;
                        else
                        {
                            int selectionStart = this.CaretIndex;
                            StringBuilder text = new StringBuilder(this.Text);
                            if (this.SelectionLength == 0)
                                text = text.Remove(this.CaretIndex, 1);
                            else
                                text = text.Remove(this.CaretIndex, this.SelectionLength);
                            this.Text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToString());
                            this.SelectionStart = selectionStart;
                            e.Handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write("<<<Delete TextBoxName >>>/n" + ex.ToString());
                    }
                    break;

                case Key.Back:
                    try
                    {
                        if (this.CaretIndex == 0
                            || String.IsNullOrEmpty(this.Text)
                            || !this.IsToUpper
                            || this.CaretIndex == this.Text.Length) break;
                        else
                        {
                            StringBuilder text = new StringBuilder(this.Text);
                            int selectionStart = this.CaretIndex - 1;
                            ///set value
                            if (this.SelectionLength == 0)
                                text = text.Remove(this.CaretIndex - 1, 1);
                            else
                                text = text.Remove(this.CaretIndex, this.SelectionLength);
                            //Format value 
                            if (text.Length > this.CaretIndex - 1)
                            {
                                this.Text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToString());
                                this.SelectionStart = selectionStart;
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write("<<<Key Back TextBoxName >>>/n" + ex.ToString());
                    }
                    break;

            }
            base.OnPreviewKeyDown(e);
        }
        private string _textClone = string.Empty;
        //private bool _flagUnikey = false;
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            /////Set text when IsReadOnly=true
            int selectedStart = this.CaretIndex;
            if (this.IsReadOnly
                || !this.IsAlphaNumeric(e.Text)
                || e.Text.Length == 0
                || this.Text.Length == this.MaxLength)
            {
                e.Handled = true;
                return;
            }

            else if (this.SelectionLength > 0
                && this.SelectionLength == this.Text.Length
                && this.IsAlphaNumeric(e.Text))
            {
                this.Text = e.Text.ToUpper();
                this.SelectionStart = 1;
                e.Handled = true;
                return;
            }

            else
            {
                if (this.IsToUpper)
                    this.Text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(this.Text.Insert(this.CaretIndex, e.Text));
                else
                    this.Text = this.Text.Insert(this.CaretIndex, e.Text);
                this.SelectionStart = selectedStart + 1;
                this._textClone = this.Text;
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }
        #endregion

        #region Methods
        private bool IsAlphaNumeric(string input)
        {
            Regex pattern = new Regex("[a-z A-Z 0-9]");
            return pattern.IsMatch(input);
        }

        public string FormatText(string content)
        {
            string text = string.Empty;
            bool flagUpper = false;
            for (int i = 0; i < content.Length; i++)
            {
                if (flagUpper)
                {
                    text += content[i].ToString().ToUpper();
                    flagUpper = false;
                    continue;
                }
                if (i + 1 < content.Length && char.ConvertToUtf32(content[i].ToString(), 0) == 32)
                    flagUpper = true;
                if (i == 0)
                    text = content[0].ToString().ToUpper();
                else
                    text += content[i].ToString().ToLower();
            }
            return text;
        }
        #endregion

        #region DependencyProperty

        #region UpperCharater
        public bool IsToUpper
        {
            get { return (bool)GetValue(IsToUpperProperty); }
            set { SetValue(IsToUpperProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsToUpper.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsToUpperProperty =
            DependencyProperty.Register("IsToUpper", typeof(bool), typeof(TextBoxName), new UIPropertyMetadata(false));

        #endregion

        #endregion
    }
}
