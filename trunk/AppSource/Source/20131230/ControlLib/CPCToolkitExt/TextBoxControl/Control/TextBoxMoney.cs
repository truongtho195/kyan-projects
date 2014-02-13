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
using System.Windows.Threading;
using System.Threading;


namespace CPCToolkitExt.TextBoxControl
{
    public enum SymbolCurrencyAlignment
    {
        // Summary:
        //     Default. Symbol is aligned to the left.
        Left = 0,
        //
        // Summary:
        //     Symbol is aligned to the right.
        Right = 1
    }

    public class TextBoxMoney : TextBox
    {
        #region Field
        protected Thickness BorderTextBoxBase;
        protected Brush BackgroundTextBoxBase;
        #endregion

        #region Contrustor
        public TextBoxMoney()
        {
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.Loaded += new RoutedEventHandler(TextBoxMoney_Loaded);
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Enter, ModifierKeys.Shift)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
        }
        #endregion

        #region Override Methods

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            try
            {
                this.IsPreviewTextInput = true;
                ///To set text when IsReadOnly is True.
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
                        if (string.IsNullOrEmpty(this.Text) || (this.SelectionLength > 0
                            && this.SelectionLength < this.Text.Length))
                        {
                            e.Handled = true;
                            break;
                        }
                        else if (this.SelectionLength == Text.Length)
                        {
                            this.Value = Decimal.Parse("0").ToString();
                            this.Text = string.Empty;
                            break;
                        }
                        else
                        {
                            if (i >= Text.Length) break;
                            if (this.Text[i].Equals(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator))
                            {
                                //To set value text.
                                this.Text = Text.Remove(i, this.Text.Length - this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator));
                                //To set SelectionStart.
                                CaretIndex = i;
                                //To set value text.
                                if (string.IsNullOrEmpty(this.Text))
                                    this.Value = Decimal.Parse("0").ToString();
                                else
                                {
                                    string text = this.Text;
                                    if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",") && text.Contains(","))
                                        text = text.Replace(",", ".");
                                    Decimal outputDecimal;
                                    if (Decimal.TryParse(text, out outputDecimal))
                                        this.Value = Decimal.Parse(text).ToString();
                                }
                                e.Handled = true;
                            }
                            else
                            {
                                string text = Text.Remove(i, 1);
                                if (string.IsNullOrEmpty(text))
                                    this.Value = Decimal.Parse("0").ToString();
                                else
                                {
                                    if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",") && text.Contains(","))
                                        text = text.Replace(",", ".");
                                    Decimal outputDecimal;
                                    if (Decimal.TryParse(text, out outputDecimal))
                                        this.Value = Decimal.Parse(text).ToString();
                                }
                            }
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
                            this.Value = Decimal.Parse("0").ToString();
                            this.Text = string.Empty;
                            break;
                        }
                        else
                        {
                            if (i <= 0) break;
                            i--;
                            if (Text[i].Equals(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator))
                            {
                                //To set value text.
                                this.Text = Text.Remove(i, this.Text.Length - this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator));

                                //To set SelectionStart.
                                this.CaretIndex = i;

                                //To set value text.
                                if (string.IsNullOrEmpty(this.Text))
                                    this.Value = string.Format(this.CurrencyStringFormat, "0");
                                else
                                {
                                    string text = this.Text;
                                    if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",") && text.Contains(","))
                                        text = text.Replace(",", ".");
                                    Decimal outputDecimal;
                                    if (Decimal.TryParse(text, out outputDecimal))
                                        this.Value = Decimal.Parse(text).ToString();
                                }
                                e.Handled = true;
                            }
                            else
                            {
                                string text = this.Text.Remove(i, 1);
                                if (string.IsNullOrEmpty(text))
                                    this.Value = Decimal.Parse("0").ToString();
                                else
                                    if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",") && text.Contains(","))
                                        text = text.Replace(",", ".");
                                Decimal outputDecimal;
                                if (Decimal.TryParse(text, out outputDecimal))
                                    this.Value = Decimal.Parse(text).ToString();
                            }
                            break;
                        }
                }
                this.IsPreviewTextInput = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TextBox Money OnPreviewKeyDown" + ex.ToString());
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            try
            {
                this.IsPreviewTextInput = true;
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly
                    || (!this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && this.Text.Length == this.MaxLength && this.SelectionLength == 0)
                    || (this.CurrencyDecimalDigits == 0//Input string ". ,"
                    && (char.ConvertToUtf32(e.Text.ToString(), 0) == 44 || char.ConvertToUtf32(e.Text.ToString(), 0) == 46))
                    || (this.Text.Length == this.MaxLength
                    && (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length)))
                {
                    e.Handled = true;
                    return;
                }
                //Set Key Shift + Insert
                if (e.Text.Length == 0
                    || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length)
                    || ((char.ConvertToUtf32(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator, 0) == 44 && char.ConvertToUtf32(e.Text, 0) == 46)
                    || (char.ConvertToUtf32(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator, 0) == 46
                    && char.ConvertToUtf32(e.Text, 0) == 44)))
                {
                    e.Handled = true;
                    return;
                }
                ////Set SelectionLength>0
                else if (this.SelectionLength > 0
                    && this.SelectionLength == this.Text.Length
                    && this.CheckNonSymbolstrange(char.Parse(e.Text)))
                {
                    this.Value = "0";
                    this.Text = string.Empty;
                }
                else if ((this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                    && this.Text.Substring(0, this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)).Length == this.TextLength
                    && this.CaretIndex <= this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator))///Input string "."
                    ||
                   (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)//////check existed of "."
                    && e.Text.ToString().Equals(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator))
                    || (!this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                    && this.Text.Length == this.TextLength
                    && this.SelectionLength == 0
                    && !e.Text.Equals(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator))
                    ////Input string after "."
                    || (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                    && (this.CaretIndex > this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                    && this.Text.Substring(this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)).Length > this.CurrencyDecimalDigits))
                    )
                {
                    e.Handled = true;
                    return;
                }
                //Check symbol
                if (!this.CheckNonSymbolstrange(char.Parse(e.Text)))
                {
                    e.Handled = true;
                    return;
                }
                ///Check "."
                else
                {
                    if (char.ConvertToUtf32(e.Text.ToString(), 0) == 46 || char.ConvertToUtf32(e.Text.ToString(), 0) == 44)
                    {
                        ////Set value.
                        if (!e.Handled)
                        {
                            int selectionStart = CaretIndex;
                            string text = string.Empty;
                            string valid = this.Text.Insert(CaretIndex, e.Text);
                            text = valid;
                            if (valid.Length - (CaretIndex + 1) > this.CurrencyDecimalDigits)
                            {
                                this.Text = valid.Remove((CaretIndex + 1) + this.CurrencyDecimalDigits);
                                text = this.Text;
                                e.Handled = true;
                            }
                            if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",")
                                 && text.Contains(","))
                                text = text.Replace(",", ".");
                            Decimal outputDecimal;
                            if (Decimal.TryParse(text, out outputDecimal))
                            {
                                this.Value = Decimal.Parse(text).ToString();
                                this.SelectionStart = selectionStart;
                            }
                            else
                                this.Value = Decimal.Parse("0").ToString();
                        }
                    }
                    else if (this.IsFocused)
                    {
                        string text = Text.Insert(CaretIndex, e.Text);
                        if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",")
                            && text.Contains(","))
                            text = text.Replace(",", ".");
                        Decimal outputDecimal;
                        if (Decimal.TryParse(text, out outputDecimal))
                            this.Value = Decimal.Parse(text).ToString();
                    }
                }
                //To set MaxLength again if Text contain "," or ".".
                if (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                    && this.Text.Length == this.MaxLength)
                    this.MaxLength = this.MaxLength + 1;
                else
                    this.MaxLength = this.TextLength + this.CurrencyDecimalDigits;
                this.IsPreviewTextInput = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TextBox Money OnPreviewTextInput" + ex.ToString());
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            try
            {
                ///To set text when IsReadOnly is True.
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                this.FormatTextGotFocus();
                //To set select all of text .
                if (this.IsSelectedAll)
                    this.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Background,
                                    (ThreadStart)delegate
                                    {
                                        this.SelectAll();
                                    });
                else
                    this.SelectionLength = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TextBox Money OnGotFocus" + ex.ToString());
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            try
            {
                ///To set text when IsReadOnly is True.
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                this.FormatTextLostFocus();
                //To set select all of text.
                this.SelectionLength = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TextBox Money OnLostFocus" + ex.ToString());
            }
            base.OnLostFocus(e);
        }

        protected void TextBoxMoney_Loaded(object sender, RoutedEventArgs e)
        {
            //BackgroundBase
            if (this.BackgroundTextBoxBase == null)
                this.BackgroundTextBoxBase = this.Background;
            //BorderBase
            if (this.BorderTextBoxBase == null || this.BorderTextBoxBase == new Thickness(0))
                this.BorderTextBoxBase = this.BorderThickness;
            //Set MaxLength
            if (this.TextLength == 0)
            {
                this.TextLength = 12;
                this.MaxLength = this.TextLength + this.CurrencyDecimalDigits;
            }
            else
                this.MaxLength = this.TextLength + this.CurrencyDecimalDigits;

            this.GetCultureInfo();

            //To load data when ConverterCulture and CurrencyStringFormat don't change.
            if (this.IsFocused && this.IsLoaded)
                this.Text = this.FormatTextWithLanguage();
            else if (!string.IsNullOrEmpty(this.Value))
                this.Text = this.StringFormatDecimal(this.Value);
            this.AcceptsReturn = false;
        }

        #endregion

        #region Properties
        protected CultureInfo CultureInfo { get; set; }
        protected bool IsPreviewTextInput = false;
        #endregion

        #region DependencyProperty

        #region ConverterCulture
        //
        // Summary:
        //     Gets or sets the horizontal alignment of the symbol currency of the text box. This
        //     is a dependency property.
        //
        // Returns:
        //     One of the Enum SymbolCurrencyAlignment values that specifies the horizontal
        //     alignment of the symbol currency of the text box. The default is SymbolCurrencyAlignment.Left.
        public CultureInfo ConverterCulture
        {
            get { return (CultureInfo)GetValue(ConverterCultureProperty); }
            set { SetValue(ConverterCultureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConverterCultureProperty =
            DependencyProperty.Register("ConverterCulture", typeof(CultureInfo), typeof(TextBoxMoney), new FrameworkPropertyMetadata(new CultureInfo("en-US")));
        #endregion

        #region CurrencyStringFormat
        //
        // Summary:
        //     Gets or sets the horizontal alignment of the symbol currency of the text box. This
        //     is a dependency property.
        //
        // Returns:
        //     One of the Enum SymbolCurrencyAlignment values that specifies the horizontal
        //     alignment of the symbol currency of the text box. The default is SymbolCurrencyAlignment.Left.
        public string CurrencyStringFormat
        {
            get { return (string)GetValue(CurrencyStringFormatProperty); }
            set { SetValue(CurrencyStringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrencyStringFormatProperty =
            DependencyProperty.Register("CurrencyStringFormat", typeof(string), typeof(TextBoxMoney), new FrameworkPropertyMetadata("${0:N2}"));

        #endregion

        #region Value
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(string),
        typeof(TextBoxMoney),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChangeText)));
        //
        // Summary:
        //     Gets or sets the text contents value return of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the text contents of the text box. The default is an
        //     "0".
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        #endregion

        #region TextLength
        public int TextLength
        {
            get { return (int)GetValue(TextLengthProperty); }
            set { SetValue(TextLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextLengthProperty =
            DependencyProperty.Register("TextLength", typeof(int), typeof(TextBoxMoney), new UIPropertyMetadata(0));


        #endregion

        #region CurrencyDecimalDigits
        //
        // Summary:
        //     Gets or sets the symbol currency of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the symbol currency of the text box. The default is an
        //     "$".
        public int CurrencyDecimalDigits
        {
            get { return (int)GetValue(CurrencyDecimalDigitsProperty); }
            set { SetValue(CurrencyDecimalDigitsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolCurrency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrencyDecimalDigitsProperty =
            DependencyProperty.Register("CurrencyDecimalDigits", typeof(int), typeof(TextBoxMoney),
       new UIPropertyMetadata(0));
        #endregion

        #region CurrencySymbol
        //
        // Summary:
        //     Gets or sets the symbol currency of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the symbol currency of the text box. The default is an
        //     "$".
        public string CurrencySymbol
        {
            get { return (string)GetValue(CurrencySymbolProperty); }
            set { SetValue(CurrencySymbolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolCurrency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrencySymbolProperty =
            DependencyProperty.Register("CurrencySymbol", typeof(string), typeof(TextBoxMoney),
       new UIPropertyMetadata("$"));
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
            DependencyProperty.Register("SymbolAlignment", typeof(SymbolCurrencyAlignment), typeof(TextBoxMoney), new UIPropertyMetadata(SymbolCurrencyAlignment.Left));


        #endregion

        #region IsTextBlock

        public bool IsTextBlock
        {
            get { return (bool)GetValue(IsTextBlockProperty); }
            set { SetValue(IsTextBlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextBlockProperty =
            DependencyProperty.Register("IsTextBlock", typeof(bool), typeof(TextBoxMoney),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsTextBlock)));

        protected static void ChangeIsTextBlock(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null
                && e.NewValue != e.OldValue
                && bool.Parse(e.NewValue.ToString()))
                (source as TextBoxMoney).ChangeStyle();
            else
                (source as TextBoxMoney).PreviousStyle();
        }

        #endregion

        #endregion

        #region Methods

        #region OldCode

        public static void ChangeText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as TextBoxMoney).UpdateTextWithLanguage(e.NewValue.ToString());
            else
                (source as TextBoxMoney).FormatCurrency();
        }

        private void ChangeBackGround()
        {
            this.Text = string.Empty;
        }

        private void FormatCurrency()
        {
            if (!this.IsFocused && this.IsLoaded)
            {
                if (this.Text == "." || this.Text == ",") this.Text = "0";
                decimal outDecimal = 0;
                if (!Decimal.TryParse(this.Text, out outDecimal)) return;
                string temp = (this.Text == string.Empty ? "0" : this.Text);
                try
                {
                    decimal d = decimal.Parse(temp, CultureInfo.InvariantCulture);
                    this.Text = this.StringFormatDecimal(d);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        ///checkNonSymbolstrange
        public bool CheckNonSymbolstrange(char localchar)
        {
            int temp = char.ConvertToUtf32(localchar.ToString(), 0);
            ////"." , 0-->9
            if (temp == 44 || //,
                temp == 46 || //.
                temp == 48 || // 0
                temp == 49 || // 1
                temp == 50 || // 2
                temp == 51 || // 3
                temp == 52 || // 4
                temp == 53 || // 5
                temp == 54 || // 6
                temp == 55 || // 7
                temp == 56 || // 8
                temp == 57    // 9
                )
                return true;

            return false;
        }

        public void ChangeStyle()
        {
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                           DispatcherPriority.Input,
                                           (ThreadStart)delegate
                                           {
                                               this.BorderThickness = new Thickness(0);
                                               this.Background = Brushes.Transparent;
                                               this.IsReadOnly = true;
                                           });
            else
            {
                this.BorderThickness = new Thickness(0);
                this.Background = Brushes.Transparent;
                this.IsReadOnly = true;
            }
        }

        public void PreviousStyle()
        {
            try
            {
                if (!this.IsLoaded)
                    this.Dispatcher.BeginInvoke(
                                               DispatcherPriority.Input,
                                               (ThreadStart)delegate
                                               {
                                                   this.BorderThickness = this.BorderTextBoxBase;
                                                   this.Background = this.BackgroundTextBoxBase;
                                                   this.IsReadOnly = false;
                                               });
                else
                {
                    this.BorderThickness = this.BorderTextBoxBase;
                    this.Background = this.BackgroundTextBoxBase;
                    this.IsReadOnly = false;
                }
                if (this.IsFocused)
                {
                    Text = Text.Replace(",", string.Empty);
                    Text = Text.Replace(this.CultureInfo.NumberFormat.CurrencySymbol, string.Empty);
                    if (string.IsNullOrEmpty(this.Value) || this.Value.Equals("0"))
                        Text = string.Empty;
                    else
                        Text = String.Format(CultureInfo.InvariantCulture, "{0:0.##}", decimal.Parse(this.Value, CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                Debug.Write("Change IsTextBlock" + ex.ToString());
            }
        }

        protected override void ChangeLanguage(System.Windows.Markup.XmlLanguage value)
        {
            base.ChangeLanguage(value);
            this.Language = value;
            this.CultureInfo = value.GetSpecificCulture();
            ///To get number format when user changes language.
            this.FormatCurrencyWithLanguage(value);
        }
        #endregion

        #region MutilLaguage
        private void FormatCurrencyWithLanguage(System.Windows.Markup.XmlLanguage value)
        {
            if (this.Value != null)
                this.UpdateTextWithLanguage(this.Value);
        }
        //To update text of control when users change language.
        private void UpdateTextWithLanguage(string NewValue)
        {
            this.GetCultureInfo();
            ///Binding Value
            if (this.IsFocused && this.IsLoaded)
            {
                if (!this.IsPreviewTextInput)
                    this.Text = this.FormatTextWithLanguage();
                return;
            }
            else if (this.IsFocused && !this.IsLoaded)
                this.Text = this.FormatTextWithLanguage();
            else if (!String.IsNullOrEmpty(NewValue))
            {
                try
                {
                    this.Text = this.StringFormatDecimal(NewValue);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.ToString());
                }
            }
            else
                this.Text = this.StringFormatDecimal(0);
        }
        private void GetCultureInfo()
        {
            try
            {
                if (this.CultureInfo != this.ConverterCulture)
                    this.CultureInfo = this.ConverterCulture;
                if (this.ConverterCulture == null)
                    this.CultureInfo = new CultureInfo("en-US");
            }
            catch (Exception ex)
            {
                this.CurrencyStringFormat = "${0:N2}";
                this.CurrencyDecimalDigits = CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalDigits;
                Debug.WriteLine("GetCultureInfo" + ex);
            }
        }
        //To format Text when control is loaded.
        private void SetTextWithLanguage()
        {
            if (string.IsNullOrEmpty(this.Value))
                this.Text = string.Empty;
            else
                this.Text = this.StringFormatDecimal(this.Value);
        }
        //To format Text when control isn't focused.
        private void FormatTextGotFocus()
        {
            if (string.IsNullOrEmpty(this.Value))
                this.Text = string.Empty;
            else
                this.Text = this.FormatTextWithLanguage();
        }
        //To format Text when control is focused.
        private void FormatTextLostFocus()
        {
            if (string.IsNullOrEmpty(this.Value))
                this.Text = string.Empty;
            else
                this.Text = this.StringFormatDecimal(this.Value);
        }
        //To format Text when control is focused.
        private string FormatTextWithLanguage()
        {
            try
            {
                string content = string.Empty;
                if ((Decimal.Parse(this.Value) - Decimal.Truncate(Decimal.Parse(this.Value))) > 0)
                {
                    if (this.CultureInfo.NumberFormat.CurrencyDecimalSeparator.Equals(",")
                       && Decimal.Parse(this.Value).ToString().Contains("."))
                        content = this.Value.ToString().Replace(".", ",");
                    else
                        content = Decimal.Parse(this.Value).ToString();
                }
                else
                    content = Decimal.Truncate(Decimal.Parse(this.Value)).ToString();
                return content;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return string.Empty;
            }
        }
        //To format Text when control is loaded.
        private string StringFormatDecimal(object value)
        {
            try
            {
                string NewValue = string.Empty;
                if (value.ToString().Contains(","))
                    value = value.ToString().Replace(",", ".");
                Decimal DecimalValue = Decimal.Parse(value.ToString());
                NewValue = String.Format(this.CultureInfo, this.CurrencyStringFormat, DecimalValue);
                return NewValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StringFormatDecimal" + ex);
                return string.Empty;
            }
        }
        #endregion

        #endregion

    }
}
