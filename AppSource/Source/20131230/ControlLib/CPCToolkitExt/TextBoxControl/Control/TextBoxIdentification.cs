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

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxIdentification : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxIdentification()
            : base(string.Empty)//"Identification"
        {
            this.MaxLength = 8;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
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
                            string text = Text;
                            if (!this.IsSecurity || !this.Text.Contains("*"))
                                text = Text;
                            else if (this.Text.Contains("*"))
                                text = Value;
                            this.Text = text.Remove(i, 1);
                            Value = Text;
                            this.SelectionStart = i;
                            e.Handled = true;
                        }
                        break;
                    }
                    break;

                case Key.Back:
                    if (this.SelectionLength == this.Text.Length)
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
                        string text = Text;
                        if (!this.IsSecurity || !this.Text.Contains("*"))
                            text = Text;
                        else if (this.Text.Contains("*"))
                            text = Value;
                        this.Text = text.Remove(i, 1);
                        this.Value = Text;
                        this.SelectionStart = i;
                        e.Handled = true;
                    }
                    break;
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
            if (e.Text.Length == 0 || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
            {
                e.Handled = true;
                return;
            }
            else if (this.SelectionLength > 0
                && this.SelectionLength == this.Text.Length
                && this.IsAlpha(e.Text))
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
            }

            //Set value input
            if ((this.Text.Length == 0 && !this.IsAlpha(e.Text))
                || this.Text.Length == 8)
            {
                e.Handled = true;
                return;
            }
            else if ((this.Text.Length > 0
                && this.IsAlpha(this.Text.Substring(0, 1))
                && !this.CheckNonSymbolstrange(char.Parse(e.Text)))
                || (!this.IsAlpha(e.Text) && this.CaretIndex == 0)
                || (this.Text.Length > 0 && !this.IsAlpha(this.Text.Substring(0, 1)) && this.CaretIndex > 0)
                )
            {
                e.Handled = true;
                return;
            }
            //Set format text 
            else if (this.IsFocused)
            {
                string text = Text.Insert(CaretIndex, e.Text.ToUpper());
                int selectedStart = this.CaretIndex;
                if (text.Length == 8)
                {
                    if (this.IsSecurity)
                        this.Text = text.Substring(0, 1) + "***" + text.Substring(4, 4);
                    else
                        this.Text = text;
                    e.Handled = true;
                }
                else
                {
                    this.Text = text;
                    e.Handled = true;
                }
                //Set selectionStart
                this.SelectionStart = selectedStart + 1;
                //Set value for Value
                this.Value = text;
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
                    if (this.IsSecurity)
                        this.Text = NewText.Substring(0, 1) + "***" + NewText.Substring(4, 4);
                    else
                        this.Text = NewText;
                else
                    this.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.Write("UpdateText TextBoxControl" + "\n" + ex.ToString());
            }

        }

        #endregion

        #region DependencyProperty
        public bool IsSecurity
        {
            get { return (bool)GetValue(IsSecurityProperty); }
            set { SetValue(IsSecurityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSecurity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSecurityProperty =
            DependencyProperty.Register("IsSecurity", typeof(bool),
            typeof(TextBoxIdentification), new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsSecurity))
            );


        protected static void ChangeIsSecurity(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                (source as TextBoxIdentification).UpdateText((source as TextBoxIdentification).Value, true);
            }
        }


        #endregion

        #region Methods
        private bool IsAlpha(string input)
        {
            Regex pattern = new Regex("[a-z A-Z]");
            return pattern.IsMatch(input);
        }
        #endregion
    }
}
