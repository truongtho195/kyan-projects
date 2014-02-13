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
    public class TextBoxPhone : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxPhone()
            : base(string.Empty)
        {
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            DataObject.AddPastingHandler(this, new DataObjectPastingEventHandler(OnPaste));
        }

        #endregion

        #region Methods
        private bool IsChangeText = true;
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                if (this.IsReadOnly) return;
                var isText = e.SourceDataObject.GetDataPresent(System.Windows.DataFormats.Text, true);
                if (!isText) return;
                var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
                var intValue = text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                decimal output = 0;
                if (!decimal.TryParse(intValue, out output))
                    this.IsChangeText = false;
                else
                {
                    //Set value for Value
                    this.Value = intValue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        } 
        #endregion

        #region Override Methods

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            try
            {
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                if (!string.IsNullOrEmpty(this.Text)
                    && this.Text.Contains("(")
                    && this.Text.Contains(")"))
                {
                    this.Text = this.RestoreData(this.Text);
                }

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< OnGotFocus >>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            try
            {
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                ///Set formatText & Value
                if (this.Text.Length == this.PhoneLength
                    && !this.Text.Contains("(")
                    && !this.Text.Contains(")")
                    && !this.Text.Contains("-"))
                {
                    this.Text = this.FormatData(this.Text);
                    this.SelectionStart = this.Text.Length;
                }

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< OnLostFocus >>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.OnLostFocus(e);
        }

        private bool _deleteFlag = false;
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
                case Key.Space:
                    e.Handled = true;
                    break;

                ////***********************Repair***************//////////////
                case Key.Delete:
                    if (this.SelectionStart < this.Text.Length)
                        _deleteFlag = true;
                    break;

                ////***********************Repair***************//////////////
                case Key.Back:
                    if (this.SelectionStart >= 0)
                        _deleteFlag = true;
                    break;

            }

            base.OnPreviewKeyDown(e);
        }

        public override void TextBoxCustomBase_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!this.IsChangeText)
                {
                    this.IsChangeText = true;
                    this.Text = string.Empty;
                    this.Value = string.Empty;
                }
                if (_deleteFlag)
                    this.Value = this.Text;
                _deleteFlag = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            base.TextBoxCustomBase_TextChanged(sender, e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            try
            {
                //Set text when IsReadOnly=true
                if (this.IsReadOnly || !this.IsFocused)
                    return;

                //Set SelectionLength>0
                if (e.Text.Length == 0 || (this.SelectionLength > 0
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

                ///Check symbol input
                if ((this.MaxExtLength > 0
                    && this.Text.Length == (10 + this.MaxExtLength))
                    || !CheckNonSymbolstrange(char.Parse(e.Text)))
                    e.Handled = true;

                //Set format text 
                else if (this.IsFocused
                    && !e.Handled)
                {
                    string text = this.Text.Insert(this.CaretIndex, e.Text);
                    ////Set value for Value
                    this.Value = text.Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// root : (option 1) from Text property,(option 2) from Value property ; true= Value
        /// </summary>
        /// <param name="NewText"></param>
        /// <param name="root"></param>
        public override void UpdateText(string NewText, bool root)
        {
            try
            {
                if (this.IsFocused) return;
                if (!String.IsNullOrEmpty(NewText))
                {
                    this.Text = this.FormatData(NewText);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region Methods

        private string FormatData(string content)
        {
            if (content.Length == this.PhoneLength)
                return string.Format("({0}) {1}-{2}", content.Substring(0, 3), content.Substring(3, 3), content.Substring(6, this.PhoneLength - 6));
            else
                return content;
        }

        private string RestoreData(string content)
        {
            return content.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
        }
        #endregion

        #region DependencyProperty

        #region PhoneLength
        //
        // Summary:
        //     Gets or sets the maximum number of characters that can be manually entered
        //     into the text box. This is a dependency property.
        //
        // Returns:
        //     The maximum number of characters that can be manually entered into the text
        //     box. The default is 0, which indicates no limit.
        public int PhoneLength
        {
            get { return (int)GetValue(PhoneLengthProperty); }
            set { SetValue(PhoneLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PhoneLengthProperty =
            DependencyProperty.Register("PhoneLength", typeof(int), typeof(TextBoxPhone), new UIPropertyMetadata(10));

        #endregion

        #region MinExtLength
        //
        // Summary:
        //     Gets or sets the minimum number of characters that can be manually entered
        //     into the text box. This is a dependency property.
        //
        // Returns:
        //     The minimum number of characters that can be manually entered into the text
        //     box. The default is 0, which indicates no limit.
        public int MinExtLength
        {
            get { return (int)GetValue(MinExtLengthProperty); }
            set { SetValue(MinExtLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinExtLengthProperty =
            DependencyProperty.Register("MinExtLength", typeof(int), typeof(TextBoxDecimal), new UIPropertyMetadata(0));


        #endregion

        #region MaxExtLength
        //
        // Summary:
        //     Gets or sets the maximum number of characters that can be manually entered
        //     into the text box. This is a dependency property.
        //
        // Returns:
        //     The maximum number of characters that can be manually entered into the text
        //     box. The default is 0, which indicates no limit.
        public int MaxExtLength
        {
            get { return (int)GetValue(MaxExtLengthProperty); }
            set { SetValue(MaxExtLengthProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxExtLengthProperty =
            DependencyProperty.Register("MaxExtLength", typeof(int), typeof(TextBoxDecimal), new UIPropertyMetadata(0));


        #endregion

        #endregion
    }

    public enum TextBoxType
    {
        None,
        Phone = 0,
        CellPhone = 1,
        Identification = 2,
        SSN = 3,
        Zip = 4,
        Fax = 5
    }

    public class TextBoxCustomize : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxCustomize()
        {
            this.Loaded += new RoutedEventHandler(TextBoxCustomize_Loaded);
        }

        #endregion

        #region Event Control
        void TextBoxCustomize_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Override Methods

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            try
            {
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                switch (TextBoxType)
                {
                    case TextBoxType.CellPhone:
                        break;
                    case TextBoxType.Phone:
                        break;
                    case TextBoxType.Fax:
                        break;
                    case TextBoxType.SSN:
                        break;
                    case TextBoxType.Zip:
                        break;
                    case TextBoxType.Identification:
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(this.Text)
                    && this.Text.Contains("(")
                    && this.Text.Contains(")"))
                {
                    this.Text = this.RestoreData(this.Text);
                }

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< OnGotFocus >>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            try
            {
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly)
                {
                    e.Handled = true;
                    return;
                }
                ///Set formatText & Value
                if (this.Text.Length >= 10
                    && !this.Text.Contains("(")
                    && !this.Text.Contains(")")
                    && !this.Text.Contains("-"))
                {
                    this.Text = this.FormatData(this.Text);
                    this.SelectionStart = this.Text.Length;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< OnLostFocus >>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.OnLostFocus(e);
        }

        private bool _deleteFlag = false;

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
                case Key.Space:
                    e.Handled = true;
                    break;

                case Key.Enter:
                    e.Handled = true;
                    break;
                ////***********************Repair***************//////////////
                case Key.Delete:
                    if (this.SelectionStart != this.Text.Length)
                        _deleteFlag = true;
                    break;

                ////***********************Repair***************//////////////
                case Key.Back:
                    if (this.SelectionStart > 0)
                        _deleteFlag = true;
                    break;

            }
            base.OnPreviewKeyDown(e);
        }

        public override void TextBoxCustomBase_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_deleteFlag)
                this.Value = this.Text;
            _deleteFlag = false;
            base.TextBoxCustomBase_TextChanged(sender, e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            //Set text when IsReadOnly=true
            if (this.IsReadOnly || !this.IsFocused)
            {
                return;
            }

            //Set SelectionLength>0
            if (e.Text.Length == 0 || (this.SelectionLength > 0
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

            ///Check symbol input
            if ((this.MaxExtLength > 0
                && this.Text.Length == (10 + this.MaxExtLength))
                || !CheckNonSymbolstrange(char.Parse(e.Text)))
                e.Handled = true;

            //Set format text 
            else if (this.IsFocused
                && !e.Handled)
            {
                string text = this.Text.Insert(this.CaretIndex, e.Text);
                ////Set value for Value
                this.Value = text.Replace("-", "");
            }
            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// root : (option 1) from Text property,(option 2) from Value property ; true= Value
        /// </summary>
        /// <param name="NewText"></param>
        /// <param name="root"></param>
        public override void UpdateText(string NewText, bool root)
        {
            try
            {
                if (this.IsFocused) return;
                if (!String.IsNullOrEmpty(NewText))
                {
                    this.Text = this.FormatData(NewText);
                }
                else
                    base.ChangeBackGround();
            }
            catch
            {
                base.ChangeBackGround();
            }
        }
        #endregion

        #region Methods

        private string FormatData(string content)
        {
            if (content.Length == 10)
                return string.Format("({0}) {1}-{2}", content.Substring(0, 3), content.Substring(3, 3), content.Substring(6, 4));
            else if (content.Length > 10)
                return string.Format("({0}) {1}-{2} EXT {3}", content.Substring(0, 3), content.Substring(3, 3), content.Substring(6, 4), content.Substring(10, content.Length - 10));
            else
                return string.Empty;
        }

        private string RestoreData(string content)
        {
            return content.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace("EXT", "");
        }
        #endregion

        #region DependencyProperty

        #region MinExtLength
        //
        // Summary:
        //     Gets or sets the minimum number of characters that can be manually entered
        //     into the text box. This is a dependency property.
        //
        // Returns:
        //     The minimum number of characters that can be manually entered into the text
        //     box. The default is 0, which indicates no limit.
        public int MinExtLength
        {
            get { return (int)GetValue(MinExtLengthProperty); }
            set { SetValue(MinExtLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinExtLengthProperty =
            DependencyProperty.Register("MinExtLength", typeof(int), typeof(TextBoxDecimal), new UIPropertyMetadata(0));


        #endregion

        #region MaxExtLength
        //
        // Summary:
        //     Gets or sets the maximum number of characters that can be manually entered
        //     into the text box. This is a dependency property.
        //
        // Returns:
        //     The maximum number of characters that can be manually entered into the text
        //     box. The default is 0, which indicates no limit.
        public int MaxExtLength
        {
            get { return (int)GetValue(MaxExtLengthProperty); }
            set { SetValue(MaxExtLengthProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxExtLengthProperty =
            DependencyProperty.Register("MaxExtLength", typeof(int), typeof(TextBoxDecimal), new UIPropertyMetadata(0));


        #endregion

        #region TextBoxType
        public TextBoxType TextBoxType
        {
            get { return (TextBoxType)GetValue(TextBoxTypeProperty); }
            set { SetValue(TextBoxTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBoxType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxTypeProperty =
            DependencyProperty.Register("TextBoxType", typeof(TextBoxType), typeof(TextBoxCustomize), new UIPropertyMetadata(TextBoxType.Phone));
        #endregion

        #endregion
    }

}
