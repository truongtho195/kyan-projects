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
using System.Globalization;
using System.Windows.Threading;
using System.Threading;


namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxNumeric : TextBoxNumberBase
    {
        #region Contrustor
        public TextBoxNumeric()
        {
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.TextAlignment = TextAlignment.Right;
            this.Loaded += new RoutedEventHandler(TextBoxNumeric_Loaded);
        }
        #endregion

        #region Properties
        protected CultureInfo CultureInfo { get; set; }
        protected bool IsPreviewTextInput = false;
        #endregion

        #region Event of Control

        #endregion

        #region Loaded
        private void TextBoxNumeric_Loaded(object sender, RoutedEventArgs e)
        {
            //To Set interger length
            if (this.IntegerLength == 0)
                this.IntegerLength = 9;
            this.GetCultureInfo();
            ///To Set text when IsReadOnly=true
            if (this.IsFocused && this.IsLoaded)
                this.FormatTextGotFocus();
            else if (!String.IsNullOrEmpty(this.Value))
                this.Text = this.StringFormatDecimal(this.Value);
        }
        #endregion

        #region Override Events
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            try
            {
                this.IsPreviewTextInput = true;
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                int i = CaretIndex;
                switch (e.Key)
                {
                    ///Blocks space key.
                    case Key.Space:
                        e.Handled = true;
                        break;
                    ///Delete key
                    case Key.Delete:
                        if (string.IsNullOrEmpty(this.Text))
                        {
                            e.Handled = true;
                            break;
                        }
                        else if (this.SelectionLength == this.Text.Length)
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
                                //Sets text
                                this.Text = Text.Remove(i, this.Text.Length - this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator));
                                //Sets SelectionStart
                                this.CaretIndex = i;
                                //Sets  text
                                if (string.IsNullOrEmpty(Text))
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
                        if (string.IsNullOrEmpty(this.Text))
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
                                //Sets text
                                this.Text = Text.Remove(i, this.Text.Length - this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator));
                                //Sets SelectionStart
                                CaretIndex = i;
                                //Sets text
                                if (string.IsNullOrEmpty(Text))
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
                }
                this.IsPreviewTextInput = false;
                base.OnPreviewKeyDown(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            try
            {
                this.IsPreviewTextInput = true;
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly
                    || (this.Text.Length == this.CurrencyDecimalDigits + 1 + this.IntegerLength
                        && this.SelectionLength == 0)
                    || (this.CurrencyDecimalDigits == 0///Input string ".")
                    && (char.ConvertToUtf32(e.Text.ToString(), 0) == 44 || char.ConvertToUtf32(e.Text.ToString(), 0) == 46))
                    || (this.Text == "-" &&
                    (char.ConvertToUtf32(e.Text.ToString(), 0) == 46
                    || char.ConvertToUtf32(e.Text, 0) == 44))
                   ||
                   !this.IsNonSymbolstrange(e.Text)
                   ||
                   ((char.ConvertToUtf32(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator, 0) == 44 && char.ConvertToUtf32(e.Text, 0) == 46)
                    || (char.ConvertToUtf32(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator, 0) == 46
                    && char.ConvertToUtf32(e.Text, 0) == 44))
                   ||
                   (!this.IsNegativeNumber ///Input string "-" IsNegativeNumber=false
                    && char.ConvertToUtf32(e.Text.ToString(), 0) == 45)
                   ||
                   (this.CaretIndex == 0 && this.Text.Contains("-")
                    && this.SelectionLength < this.Text.Length) ///Input before "."
                   ||
                   (this.IsNegativeNumber///Input string "-" IsNegativeNumber=true
                   && char.ConvertToUtf32(e.Text.ToString(), 0) == 45 && this.CaretIndex > 0)
                   ||
                   (this.CurrencyDecimalDigits == 0///Input string ".")
                    && (char.ConvertToUtf32(e.Text.ToString(), 0) == 44 || char.ConvertToUtf32(e.Text.ToString(), 0) == 46))
                   ||
                   (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)//////check existed of "."
                       && (char.ConvertToUtf32(e.Text.ToString(), 0) == 44 || char.ConvertToUtf32(e.Text.ToString(), 0) == 46))
                   || (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && (this.CaretIndex > Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                       && this.Text.Substring(Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)).Length > this.CurrencyDecimalDigits))////Input string after "."
                   ||
                   (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && !this.Text.Contains("-")
                       && this.Text.Substring(0, this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)).Length == this.IntegerLength && this.CaretIndex <= this.Text.IndexOf(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator)
                       && !e.Text.Equals("-"))
                   || (!this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && !this.Text.Contains("-") && this.Text.Length == this.IntegerLength
                       && this.SelectionLength == 0
                       && !e.Text.Equals(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && !e.Text.Equals("-"))
                   || (!this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && this.Text.Contains("-")
                       && this.Text.Length == this.IntegerLength + 1 && !e.Text.Equals(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator))
                   || (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencyDecimalSeparator) && this.Text.Contains("-")
                       && this.Text.Length - this.CurrencyDecimalDigits == this.IntegerLength + this.CurrencyDecimalDigits)
                   )
                {
                    e.Handled = true;
                    return;
                }

                //Set Key Shift + Insert
                if (e.Text.Length == 0
                   || (SelectionLength > 0
                   && SelectionLength < Text.Length))
                {
                    e.Handled = true;
                    return;
                }

               ////Set SelectionLength>0
                else if (this.SelectionLength > 0
                   && this.SelectionLength == this.Text.Length
                   && this.IsNonSymbolstrange(e.Text))
                {
                    this.Text = e.Text;
                    if (char.ConvertToUtf32(e.Text.ToString(), 0) > 46)
                        this.Value = Decimal.Parse(this.Text).ToString();
                    else
                        this.Value = Decimal.Parse("0").ToString();
                    this.SelectionStart = 1;
                    e.Handled = true;
                    return;
                }

                ///Check "."
                if (char.ConvertToUtf32(e.Text.ToString(), 0) == 45 && this.Text.Length == 0)
                {
                    this.Text = e.Text;
                    this.Value = Decimal.Parse("0").ToString();
                    this.SelectionStart = 1;
                    e.Handled = true;
                }

                else if (char.ConvertToUtf32(e.Text.ToString(), 0) == 46 || char.ConvertToUtf32(e.Text.ToString(), 0) == 44)
                {
                    if (this.Text.Length == 0)
                        this.Value = Decimal.Parse("0").ToString();
                    ////Set value after "."
                    else
                    {
                        int selectionStart = CaretIndex;
                        string text = string.Empty;
                        string valid = Text.Insert(CaretIndex, e.Text);
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
                this.IsPreviewTextInput = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            ///Setss text when IsReadOnly=true
            if (this.IsReadOnly || !this.IsLoaded)
                return;
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
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            ///Sets text when IsReadOnly=true
            if (this.IsReadOnly)
                return;
            this.FormatTextLostFocus();
            //To set select all of text .
            this.SelectionLength = 0;
            base.OnLostFocus(e);
        }
        #endregion

        #region DependencyProperties

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
            DependencyProperty.Register("ConverterCulture", typeof(CultureInfo), typeof(TextBoxNumeric), new UIPropertyMetadata(new CultureInfo("en-US")));
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
            DependencyProperty.Register("SymbolCurrency", typeof(string), typeof(TextBoxNumeric),
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
            DependencyProperty.Register("SymbolAlignment", typeof(SymbolCurrencyAlignment), typeof(TextBoxNumeric), new UIPropertyMetadata(SymbolCurrencyAlignment.Left));


        #endregion

        #region Decimal Places
        //
        // Summary:
        //     Gets or sets the decimal places of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the decimal places of the text box. The default is string Empty.
        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DecimalPlaces.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(TextBoxNumeric), new UIPropertyMetadata(0));


        #endregion

        #region IsValueDefault
        public bool IsValueDefault
        {
            get { return (bool)GetValue(IsValueDefaultProperty); }
            set { SetValue(IsValueDefaultProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsValueDefault.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValueDefaultProperty =
            DependencyProperty.Register("IsValueDefault", typeof(bool), typeof(TextBoxNumeric), new UIPropertyMetadata(true));
        #endregion

        #region IsUsingSeparator
        public bool IsUsingSeparator
        {
            get { return (bool)GetValue(IsUsingSeparatorProperty); }
            set { SetValue(IsUsingSeparatorProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsUsingSeparator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUsingSeparatorProperty =
            DependencyProperty.Register("IsUsingSeparator", typeof(bool), typeof(TextBoxNumeric), new UIPropertyMetadata(false));
        #endregion

        #region Negativenumber
        public bool IsNegativeNumber
        {
            get { return (bool)GetValue(IsNegativeNumberProperty); }
            set { SetValue(IsNegativeNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNegativeNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNegativeNumberProperty =
            DependencyProperty.Register("IsNegativeNumber", typeof(bool), typeof(TextBoxNumeric), new UIPropertyMetadata(false));

        #endregion

        #region IntegerLength
        public int IntegerLength
        {
            get { return (int)GetValue(IntegerLengthProperty); }
            set { SetValue(IntegerLengthProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntegerLengthProperty =
            DependencyProperty.Register("IntegerLength", typeof(int), typeof(TextBoxNumeric), new UIPropertyMetadata(8));
        #endregion

        #region Nullable

        #endregion

        #region IsSetValueDefault
        public bool IsSetValueDefault
        {
            get { return (bool)GetValue(IsSetValueDefaultProperty); }
            set { SetValue(IsSetValueDefaultProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsValueDefault.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSetValueDefaultProperty =
            DependencyProperty.Register("IsSetValueDefault", typeof(bool), typeof(TextBoxNumberBase), new UIPropertyMetadata(false, ChangeIsSetValueDefault));
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
            DependencyProperty.Register("CurrencyDecimalDigits", typeof(int), typeof(TextBoxNumeric),
       new UIPropertyMetadata(0));
        #endregion

        #region NumericStringFormat
        //
        // Summary:
        //     Gets or sets the horizontal alignment of the symbol currency of the text box. This
        //     is a dependency property.
        //
        // Returns:
        //     One of the Enum SymbolCurrencyAlignment values that specifies the horizontal
        //     alignment of the symbol currency of the text box. The default is SymbolCurrencyAlignment.Left.
        public string NumericStringFormat
        {
            get { return (string)GetValue(NumericStringFormatProperty); }
            set { SetValue(NumericStringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumericStringFormatProperty =
            DependencyProperty.Register("NumericStringFormat", typeof(string), typeof(TextBox), new UIPropertyMetadata("{0:N2}"));

        #endregion

        #endregion

        #region Methods

        private void FormatCurrency()
        {
            if (!this.IsFocused && this.IsLoaded)
            {
                if (this.Text == ".") this.Text = "0";
                if (this.Text.Contains(this.CultureInfo.NumberFormat.CurrencySymbol)) return;
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

        protected static void ChangeIsSetValueDefault(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as TextBoxNumeric).SetValueFormatWithDefault();
        }

        private void SetValueFormatWithDefault()
        {
            try
            {
                ////Binding Value
                //if (this.IsFocused) return;
                //if (String.IsNullOrEmpty(this.Value)
                //    || !this.Value.ToUpper().Equals("NULL")
                //    || this.Value.Equals("0"))
                //{
                //    if (!this.IsSetValueDefault)
                //        this.Text = string.Empty;
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TextBoxNumeric  SetValueFormatWithDefault " + ex.ToString());
            }
        }

        #region MutilLaguage
        private void FormatCurrencyWithLanguage(System.Windows.Markup.XmlLanguage value)
        {
            if (this.Value != null)
                this.UpdateTextWithLanguage(this.Value);
        }
        //To update text of control when users change language.
        private void UpdateTextWithLanguage(string NewValue)
        {
            try
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
                    this.Text = this.StringFormatDecimal(NewValue);
                else
                    this.Text = this.StringFormatDecimal(0);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }
        private void GetCultureInfo()
        {
            try
            {
                if (this.CultureInfo != this.ConverterCulture)
                    this.CultureInfo = this.ConverterCulture;
                if (this.ConverterCulture == null)
                    this.CultureInfo = new CultureInfo("en-US");
                //if (this.CultureInfo != this.ConverterCulture)
                //{
                //    this.CultureInfo = this.ConverterCulture;
                //    this.CurrencyDecimalDigits = int.Parse(this.NumericStringFormat.Substring(this.NumericStringFormat.Length - 2, 1));
                //}
            }
            catch (Exception ex)
            {
                this.NumericStringFormat = "{0:N2}";
                this.CurrencyDecimalDigits = CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalDigits;
                Debug.WriteLine("GetCultureInfo" + ex);
            }
        }
        //To format Text when control is loaded.
        private void SetTextWithLanguage()
        {
            if (string.IsNullOrEmpty(this.Value)
                || this.Value.Equals("0")
                || decimal.Parse(this.Value) == 0)
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
                this.Text = StringFormatDecimal(this.Value);
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
                string content = string.Empty;
                if (!this.IsFormatWithZero && value.ToString().Equals("0"))
                    content = "0";
                else
                {
                    if (value.ToString().Contains(","))
                        value = value.ToString().Replace(",", ".");
                    Decimal DecimalValue = Decimal.Parse(value.ToString());
                    content = String.Format(this.CultureInfo, this.NumericStringFormat, DecimalValue);
                }
                return content;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StringFormatDecimal" + ex);
                return string.Empty;
            }
        }
        #endregion

        #endregion

        #region Override Methods

        public override void SetValueFormat(string content)
        {
            try
            {
                this.GetCultureInfo();
                ///Binding Value
                if (this.IsFocused && this.IsLoaded)
                {
                    if (!this.IsPreviewTextInput)
                        this.SetTextWithLanguage();
                    return;
                }
                else if (this.IsFocused && !this.IsLoaded)
                    this.SetTextWithLanguage();
                else if (!String.IsNullOrEmpty(content))
                {
                    this.Text = this.StringFormatDecimal(content);
                }
                else
                    this.Text = this.StringFormatDecimal(0);
                base.SetValueFormat(content);
            }
            catch (Exception ex)
            {
                Debug.Write("SetValueFormat" + ex);
            }
        }

        public override void SetValueDefault()
        {
            this.FormatCurrency();
            base.SetValueDefault();
        }
        #endregion
    }
}
