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
using System.Diagnostics;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxCardNumber : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxCardNumber()
            : base(string.Empty)//"CardNumber"
        {
            this.MaxLength = 16;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
        }
        #endregion

        #region Override Methods

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            try
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
                    case Key.Space:
                        e.Handled = true;
                        break;

                    case Key.Delete:
                        TextBoxCardNumber._textClone = string.Empty;
                        if (string.IsNullOrEmpty(this.Text))
                        {
                            e.Handled = true;
                            break;
                        }
                        else if (SelectionLength == Text.Length)
                        {
                            Value = string.Empty;
                            Text = string.Empty;
                        }
                        else
                        {
                            if (i >= Text.Length) break;
                            if (this.SelectionLength > 0 || char.ConvertToUtf32(Text[i].ToString(), 0) == 45)
                                e.Handled = true;
                            else
                            {
                                string text = string.Empty;
                                if (this.Text.Contains("-") && this.Value.Length == 16)
                                {
                                    text = Value.Substring(0, 4) + "-" + Value.Substring(4, 4) + "-" + Value.Substring(8, 4) + "-" + Value.Substring(12, 4);
                                    this.Text = this.Text.Remove(i, 1);
                                    this.Value = text.Remove(i, 1).Replace("-", "");
                                }
                                else if (this.Text.Contains("-") && this.Value.Length == 15)
                                {
                                    text = Value.Substring(0, 4) + "-" + Value.Substring(4, 4) + "-" + Value.Substring(8, 4) + "-" + Value.Substring(12, 3);
                                    Text = text.Remove(i, 1).Replace("-", "");
                                    Value = Text;
                                }
                                else
                                {
                                    this.Text = this.Text.Remove(i, 1);
                                    this.Value = this.Text;
                                }
                                ///Set SelectionStart after removed
                                this.SelectionStart = i;
                                e.Handled = true;
                            }
                            break;
                        }
                        break;

                    case Key.Back:
                        TextBoxCardNumber._textClone = string.Empty;
                        if (string.IsNullOrEmpty(this.Text))
                        {
                            e.Handled = true;
                            break;
                        }
                        else if (this.SelectionLength == this.Text.Length)
                        {
                            Value = string.Empty;
                            Text = string.Empty;
                        }

                        if (i <= 0)
                            break;

                        i--;
                        if (this.SelectionLength > 0 || char.ConvertToUtf32(this.Text[i].ToString(), 0) == 45)
                            e.Handled = true;
                        else
                        {
                            string text = string.Empty;
                            if (this.Text.Contains("-") && this.Value.Length == 16)
                            {
                                text = Value.Substring(0, 4) + "-" + Value.Substring(4, 4) + "-" + Value.Substring(8, 4) + "-" + Value.Substring(12, 4);
                                this.Text = this.Text.Remove(i, 1);
                                this.Value = text.Remove(i, 1).Replace("-", "");
                            }
                            else if (this.Text.Contains("-") && this.Value.Length == 15)
                            {
                                text = Value.Substring(0, 4) + "-" + Value.Substring(4, 4) + "-" + Value.Substring(8, 4) + "-" + Value.Substring(12, 3);
                                Text = text.Remove(i, 1).Replace("-", "");
                                Value = Text;
                            }
                            else
                            {
                                this.Text = this.Text.Remove(i, 1);
                                this.Value = this.Text;
                            }
                            ///Set SelectionStart after removed
                            this.SelectionStart = i;
                            e.Handled = true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            try
            {
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly
                    || this.Text.Length == this.MaxLength)
                {
                    e.Handled = true;
                    return;
                }
                //Set SelectionLength>0
                if (e.Text.Length == 0
                    || (this.SelectionLength > 0
                    && this.SelectionLength < this.Text.Length))
                {
                    e.Handled = true;
                    return;
                }
                else if (this.SelectionLength > 0
                    && this.SelectionLength == this.Text.Length
                    && CheckNonSymbolstrange(char.Parse(e.Text)))
                {
                    this.Text = string.Empty;
                    this.Value = string.Empty;
                }

                ///check symbol and "-" 
                if ((this.Text.Contains("-") && this.Text.Replace("-", "").Length == 16)
                || !CheckNonSymbolstrange(char.Parse(e.Text)))
                    e.Handled = true;

                //Set format text 
                else if (this.IsFocused && !e.Handled)
                {
                    string text = Text.Insert(CaretIndex, e.Text);
                    int selectedStart = this.CaretIndex;
                    if (text.Length == 15)
                    {
                        TextBoxCardNumber._textClone = text;
                        /////Set value for SelectionStart
                        if (this.IsSecurity)
                            ///Set format X**-**-XXXX
                            this.Text = text.Substring(0, 1) + "***-****-****-" + text.Substring(12, 3);
                        else
                            ///Set format XXX-XX-XXXX
                            this.Text = text.Substring(0, 4) + "-" + text.Substring(4, 4) + "-" + text.Substring(8, 4) + "-" + text.Substring(12, 3);
                        e.Handled = true;
                        this.SelectionStart = this.Text.Length;
                    }
                    else if (text.Replace("-", "").Length == 16)
                        this.Text = text;
                    else if (!text.Contains("-"))
                        this.SelectionStart = selectedStart;
                    //Set value for Value
                    if (text.Length == 15)
                        this.Value = text.Replace("-", "");
                    else if (text.Length > 15)
                        this.Value = TextBoxCardNumber._textClone + e.Text;
                }
                this.SelectionStart = this.Text.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            base.OnPreviewTextInput(e);

        }

        private static string _textClone = string.Empty;

        public override void UpdateText(string NewText, bool root)
        {
            try
            {
                if (this.IsFocused) return;
                if (!string.IsNullOrEmpty(NewText))
                {
                    if (this.IsSecurity)
                        this.Text = NewText.Substring(0, 1) + "***-****-****-" + (NewText.Length == 16 ? NewText.Substring(12, 4) : NewText.Substring(12, 3));
                    else
                        ///Set format XXX-XX-XXXX
                        this.Text = NewText.Substring(0, 4) + "-" + NewText.Substring(4, 4) + "-" + NewText.Substring(8, 4) + "-" + (NewText.Length == 16 ? NewText.Substring(12, 4) : NewText.Substring(12, 3));
                }
                else
                    this.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public bool IsSecurity
        {
            get { return (bool)GetValue(IsSecurityProperty); }
            set { SetValue(IsSecurityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSecurity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSecurityProperty =
            DependencyProperty.Register("IsSecurity", typeof(bool),
            typeof(TextBoxCardNumber),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsSecurity))
            );
        protected static void ChangeIsSecurity(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                (source as TextBoxCardNumber).UpdateText((source as TextBoxCardNumber).Value, true);
            }
        }

        #endregion

    }
}
