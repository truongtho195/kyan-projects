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
using System.Globalization;
using System.Diagnostics;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxPercent : TextBoxNumberBase
    {
        #region Contrustor
        public TextBoxPercent()
        {
            //this.AllowDrop = false;
            //this.ContextMenu = null;
            this.Loaded += new RoutedEventHandler(TextBoxPercent_Loaded);
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.TextAlignment = TextAlignment.Right;
            // this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            // this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Enter, ModifierKeys.Shift)));
            // this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
        }


        #endregion

        #region Override Methods
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
                    if (string.IsNullOrEmpty(this.Text) || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
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
                        if (i >= Text.Length) break;
                        if (this.Text[i].Equals('.'))
                        {
                            //Set value text
                            this.Text = Text.Remove(i, this.Text.Length - this.Text.IndexOf('.'));
                            //Set SelectionStart
                            this.CaretIndex = i;
                            //Set value text
                            if (string.IsNullOrEmpty(Text))
                                this.Value = "0";
                            else
                                this.Value = Text;
                            e.Handled = true;
                        }
                        else
                        {
                            string text = Text.Remove(i, 1);
                            if (string.IsNullOrEmpty(text))
                                this.Value = "0";
                            else
                                this.Value = text;
                        }
                        break;
                    }

                ///Back key
                case Key.Back:
                    if (string.IsNullOrEmpty(this.Text) || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
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
                        if (this.Text[i].Equals('.'))
                        {
                            //Set value text
                            this.Text = Text.Remove(i, this.Text.Length - this.Text.IndexOf('.'));

                            //Set SelectionStart
                            this.CaretIndex = i;

                            //Set value text
                            if (string.IsNullOrEmpty(Text))
                                Value = "0";
                            else
                                this.Value = this.Text;

                            e.Handled = true;
                        }
                        else
                        {
                            string text = Text.Remove(i, 1);
                            if (string.IsNullOrEmpty(text))
                                this.Value = "0";
                            else
                                this.Value = text;
                        }
                        break;
                    }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly
                || (this.Text.Length == this.MaxLength
                && (SelectionLength > 0 && SelectionLength < Text.Length)))
            {
                e.Handled = true;
                return;
            }
            //Set Key Shift + Insert
            if (e.Text.Length == 0
                || (this.SelectionLength > 0
                && this.SelectionLength < this.Text.Length))
            {
                e.Handled = true;
                return;
            }

            ////Set SelectionLength>0
            else if (this.SelectionLength > 0
                && this.SelectionLength == this.Text.Length
                && this.IsNumeric(e.Text))
            {
                Value = "0";
                Text = string.Empty;
            }

            else if ((this.Text.Contains(".")
                && this.Text.Substring(0, this.Text.IndexOf(".")).Length == this.TextLength
                && this.CaretIndex <= this.Text.IndexOf("."))///Input string "."

                || (!this.Text.Contains(".")
                && this.Text.Length == this.TextLength
                && !e.Text.Equals("."))

                || (this.Text.Contains(".")
                && (this.CaretIndex > Text.IndexOf(".")
                && this.Text.Substring(Text.IndexOf(".")).Length > 2)) ////Input string after "."
                )
            {
                e.Handled = true;
                return;
            }

            //Check symbol
            if (!IsNumeric(e.Text))
                e.Handled = true;

           ///Check "."
            else if (!e.Handled)
                if (char.ConvertToUtf32(e.Text.ToString(), 0) == 46)
                {
                    ////check existed of "."
                    foreach (var item in this.Text)
                        if (char.ConvertToUtf32(item.ToString(), 0) == 46)
                        {
                            e.Handled = true;
                            break;
                        }

                    ////Set value after "."
                    if (!e.Handled)
                    {
                        int selectionStart = CaretIndex;
                        string valid = Text.Insert(CaretIndex, e.Text);

                        if (valid.Length - (CaretIndex + 1) > 2)
                        {
                            Text = valid.Remove((CaretIndex + 1) + 2);
                            e.Handled = true;
                        }
                        Value = Text;
                        SelectionStart = selectionStart;
                    }
                }
                else if (this.IsFocused)
                {
                    int selectionStart = CaretIndex;
                    string text = Text.Insert(CaretIndex, e.Text);
                    if (text.IndexOf(".") == 0)
                        Value = String.Format(CultureInfo.InvariantCulture, "{0:#0.##}", decimal.Parse("0" + text, CultureInfo.InvariantCulture));
                    else
                        Value = String.Format(CultureInfo.InvariantCulture, "{0:#0.##}", decimal.Parse(text, CultureInfo.InvariantCulture));
                    this.Text = Value;
                    SelectionStart = selectionStart + 1;
                    e.Handled = true;
                }

            base.OnPreviewTextInput(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
            {
                e.Handled = true;
                return;
            }
            if (string.IsNullOrEmpty(this.Value) || this.Value.Equals("0"))
            {
                this.Text = string.Empty;
            }
            else
                this.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.##}", decimal.Parse(this.Value, CultureInfo.InvariantCulture));
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            ///Set text when IsReadOnly=true
            if (this.IsReadOnly)
            {
                return;
            }
            this.FormatCurrency();
            base.OnLostFocus(e);
        }

        private void TextBoxPercent_Loaded(object sender, RoutedEventArgs e)
        {
            //Set MaxLength
            if (this.TextLength == 0)
            {
                this.TextLength = 9;
                this.MaxLength = 9 + 3;
            }
            else
                this.MaxLength = this.TextLength + 3;
        }
        #endregion

        #region DependencyProperty

        #region TextLength
        public int TextLength
        {
            get { return (int)GetValue(TextLengthProperty); }
            set { SetValue(TextLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextLengthProperty =
            DependencyProperty.Register("TextLength", typeof(int), typeof(TextBoxPercent), new UIPropertyMetadata(0));
        #endregion

        #region SymbolCurrency
        //
        // Summary:
        //     Gets or sets the symbol currency of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the symbol currency of the text box. The default is an
        //     "$".
        public string SymbolCurrency
        {
            get { return (string)GetValue(SymbolCurrencyProperty); }
            set { SetValue(SymbolCurrencyProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SymbolCurrency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolCurrencyProperty =
            DependencyProperty.Register("SymbolCurrency", typeof(string), typeof(TextBoxPercent),
       new UIPropertyMetadata("$"));
        // new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeSymbol)));
        #endregion

        #region SymbolAlignment
        //
        // Summary:
        //     Gets or sets the horizontal alignment of the symbol currency of the text box. This
        //     is a dependency property.
        //
        // Returns:
        //     One of the Enum SymbolCurrencyAlignment values that specifies the horizontal
        //     alignment of the symbol currency of the text box. The default is SymbolCurrencyAlignment.Left.
        public SymbolCurrencyAlignment SymbolAlignment
        {
            get { return (SymbolCurrencyAlignment)GetValue(SymbolAlignmentProperty); }
            set { SetValue(SymbolAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolAlignmentProperty =
            DependencyProperty.Register("SymbolAlignment", typeof(SymbolCurrencyAlignment), typeof(TextBoxPercent), new UIPropertyMetadata(SymbolCurrencyAlignment.Left));


        #endregion

        #region IsVisibleSymbol

        public bool IsVisibleSymbol
        {
            get { return (bool)GetValue(IsVisibleSymbolProperty); }
            set { SetValue(IsVisibleSymbolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVisibleSymbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVisibleSymbolProperty =
            DependencyProperty.Register("IsVisibleSymbol", typeof(bool), typeof(TextBoxPercent), new UIPropertyMetadata(false));


        #endregion

        #endregion

        #region Methods

        private void ChangeBackGround()
        {
            this.Text = string.Empty;
        }

        private void FormatCurrency()
        {
            if (!this.IsFocused)
            {
                if (Text == ".") Text = "0";
                string temp = (Text == string.Empty ? "0" : Text);
                try
                {
                    decimal d = decimal.Parse(temp, CultureInfo.InvariantCulture);
                    if (this.IsVisibleSymbol)
                        Text = String.Format(CultureInfo.InvariantCulture, "{0:#0.00}%", d);
                    else
                        Text = String.Format(CultureInfo.InvariantCulture, "{0:#0.00}", d);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.ToString());
                }
            }
        }
        public override void SetValueFormat(string content)
        {

            ///Binding Value
            if (this.IsFocused) return;
            if (!String.IsNullOrEmpty(content))
                try
                {
                    decimal d = decimal.Parse(content, CultureInfo.InvariantCulture);
                    if (this.IsVisibleSymbol)
                        Text = String.Format(CultureInfo.InvariantCulture, "{0:#0.00}%", d);
                    else
                        Text = String.Format(CultureInfo.InvariantCulture, "{0:#0.00}", d);

                }
                catch (Exception ex)
                {
                    Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<SetValueFormat TextBoxDecimal>>>>>>>>>>>>>>>>" + ex.ToString());
                }
            else
            {
                if (this.IsVisibleSymbol)
                    Text = "0.00%";
                else
                    Text = "0.00";
            }
            base.SetValueFormat(content);
        }

        public override void SetValueDefault()
        {
            this.FormatCurrency();
            base.SetValueDefault();
        }

        public override void PreviousStyle()
        {
            try
            {
                if (this.IsFocused)
                {
                    if (string.IsNullOrEmpty(this.Value) || this.Value.Equals("0"))
                    {
                        Text = string.Empty;
                    }
                    else
                        Text = String.Format(CultureInfo.InvariantCulture, "{0:0.##}", decimal.Parse(this.Value, CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<PreviousStyle TextBoxPercent>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
     

        #endregion
    }

}
