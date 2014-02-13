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
    public class TextBoxSSN : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxSSN()
            : base(string.Empty)//"Ssn"
        {
            this.MaxLength = 9;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            DataObject.AddPastingHandler(this, new DataObjectPastingEventHandler(OnPaste));
        }
        #endregion

        #region Methods
        private bool IsChangeText = true;
        private bool IsPasteData = false;
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                if (this.IsReadOnly) return;
                this.IsPasteData = true;
                var isText = e.SourceDataObject.GetDataPresent(System.Windows.DataFormats.Text, true);
                if (!isText) return;
                var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
                var intValue = text.Replace("-", "").Replace(" ", "");
                int output = 0;
                if (!int.TryParse(intValue, out output))
                    this.IsChangeText = false;
                else
                {
                    //Set value for Value
                    this.Value = intValue;
                }
            }
            catch (Exception ex)
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
                Debug.WriteLine(ex.ToString());
            }
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
                    return;
                }
                int i = CaretIndex;
                switch (e.Key)
                {
                    case Key.Space:
                        e.Handled = true;
                        break;

                    case Key.Delete:
                        if (string.IsNullOrEmpty(this.Text))
                        {
                            e.Handled = true;
                            break;
                        }
                        else if (SelectionLength == Text.Length)
                        {
                            this.Value = string.Empty;
                            this.Text = string.Empty;
                        }
                        else
                        {
                            if (i >= Text.Length) break;
                            if (this.SelectionLength > 0 || char.ConvertToUtf32(Text[i].ToString(), 0) == 45)
                                e.Handled = true;
                            else
                            {
                                string text = string.Empty;
                                if (!this.IsSecurity || !this.Text.Contains("-"))
                                    text = Text;
                                else if (this.Text.Contains("-"))
                                    text = Value.Substring(0, 3) + "-" + Value.Substring(3, 2) + "-" + Value.Substring(5, 4);
                                this.Text = text.Remove(i, 1).Replace("-", "");
                                ///Set value Value after removed
                                this.Value = Text;
                                ///Set SelectionStart after removed
                                if (text.Contains("-") && i > 5)
                                    this.SelectionStart = i - 2;
                                else if (text.Contains("-") && i > 2 && i <= 5)
                                    this.SelectionStart = i - 1;
                                else
                                    this.SelectionStart = i;
                                e.Handled = true;
                            }
                            break;
                        }
                        break;

                    case Key.Back:
                        if (string.IsNullOrEmpty(this.Text))
                        {
                            e.Handled = true;
                            break;
                        }
                        else if (this.SelectionLength == this.Text.Length)
                        {
                            this.Value = string.Empty;
                            this.Text = string.Empty;
                        }

                        if (i <= 0)
                            break;

                        i--;
                        if (this.SelectionLength > 0 || char.ConvertToUtf32(this.Text[i].ToString(), 0) == 45)
                            e.Handled = true;
                        else
                        {
                            string text = string.Empty;
                            if (!this.IsSecurity || !this.Text.Contains("-"))
                                text = Text;
                            else if (this.Text.Contains("-"))
                                text = Value.Substring(0, 3) + "-" + Value.Substring(3, 2) + "-" + Value.Substring(5, 4);
                            this.Text = text.Remove(i, 1).Replace("-", "");
                            this.Value = Text;
                            ///Set SelectionStart after removed
                            if (text.Contains("-") && i > 5)
                                this.SelectionStart = i - 2;
                            else if (text.Contains("-") && i > 2 && i <= 5)
                                this.SelectionStart = i - 1;
                            else
                                this.SelectionStart = i;
                            e.Handled = true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
                Debug.WriteLine(ex.ToString());
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            try
            {
                ///Set text when IsReadOnly=true
                if (this.IsReadOnly)
                {
                    return;
                }

                //Set SelectionLength>0
                if (e.Text.Length == 0 || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
                {
                    e.Handled = true;
                    return;
                }
                else if (this.SelectionLength > 0 && this.SelectionLength == this.Text.Length && CheckNonSymbolstrange(char.Parse(e.Text)))
                {
                    this.Text = string.Empty;
                    this.Value = string.Empty;
                }

                ///check symbol and "-" 
                if ((this.Text.Contains("-") && this.Text.Replace("-", "").Length == 9)
                || !CheckNonSymbolstrange(char.Parse(e.Text)))
                    e.Handled = true;

                //Set format text 
                else if (this.IsFocused && !e.Handled)
                {
                    string text = Text.Insert(CaretIndex, e.Text);
                    int selectedStart = this.CaretIndex;
                    if (text.Length == 9)
                    {
                        ///Set value for SelectionStart
                        if (this.CaretIndex > 2 && this.CaretIndex < 5)
                            selectedStart = this.CaretIndex + 1;
                        else if (this.CaretIndex >= 5)
                            selectedStart = this.CaretIndex + 2;

                        if (this.IsSecurity)
                            ///Set format X**-**-XXXX
                            this.Text = text.Substring(0, 1) + "**-**-" + text.Substring(5, 4);
                        else
                            ///Set format XXX-XX-XXXX
                            this.Text = text.Substring(0, 3) + "-" + text.Substring(3, 2) + "-" + text.Substring(5, 4);
                        e.Handled = true;
                        this.SelectionStart = selectedStart + 1;
                    }
                    else if (!text.Contains("-"))
                        this.SelectionStart = selectedStart;

                    //Set value for Value
                    this.Value = text.Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
                Debug.WriteLine(ex.ToString());
            }
            base.OnPreviewTextInput(e);
        }

        public override void UpdateText(string NewText, bool root)
        {
            try
            {
                if (this.IsFocused) return;
                if (!string.IsNullOrEmpty(NewText))
                {
                    if (this.IsSecurity)
                        this.Text = NewText.Substring(0, 1) + "**-**-" + NewText.Substring(5, 4);
                    else
                        this.Text = NewText.Substring(0, 3) + "-" + NewText.Substring(3, 2) + "-" + NewText.Substring(5, 4);
                }
                else
                    this.Text = string.Empty;
            }
            catch
            {
                this.Text = string.Empty;
                //base.ChangeBackGround();
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
            typeof(TextBoxCustomBase),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsSecurity))
            );
        protected static void ChangeIsSecurity(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                (source as TextBoxCustomBase).UpdateText((source as TextBoxCustomBase).Value, true);
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            try
            {
                if (!this.IsChangeText)
                {
                    this.IsChangeText = true;
                    this.Text = string.Empty;
                    this.Value = string.Empty;
                }
                else if (this.IsPasteData)
                {
                    this.IsPasteData = false;
                    if (!string.IsNullOrEmpty(this.Text) && this.Text.Length > 0)
                    {
                        if (this.IsSecurity)
                            this.Text = this.Text.Substring(0, 1) + "**-**-" + this.Text.Substring(5, 4);
                        else
                            this.Text = this.Text.Substring(0, 3) + "-" + this.Text.Substring(3, 2) + "-" + this.Text.Substring(5, 4);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
                Debug.WriteLine(ex.ToString());
            }

            base.OnTextChanged(e);
        }

        #endregion

    }
}
