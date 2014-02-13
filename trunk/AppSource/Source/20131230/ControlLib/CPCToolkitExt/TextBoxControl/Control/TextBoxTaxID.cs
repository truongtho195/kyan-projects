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

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxTaxID : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxTaxID()
            : base(string.Empty)//"TaxID"
        {
            this.MaxLength = 9;
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
                            this.Text = Text.Remove(i, 1).Replace("-", "");
                            this.Value = Text;
                            if (text.Contains("-") && i > 2)
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
                        string text = Text;
                        this.Text = Text.Remove(i, 1).Replace("-", "");
                        this.Value = Text;

                        if (text.Contains("-") && i > 2)
                            this.SelectionStart = i - 1;
                        else
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
            //Set SelectionLength>0
            if (e.Text.Length == 0 || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
            {
                e.Handled = true;
                return;
            }
            ///Set value when selected all
            else if (this.SelectionLength > 0 && this.SelectionLength == this.Text.Length && CheckNonSymbolstrange(char.Parse(e.Text)))
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
            }

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
                    if (this.CaretIndex > 2)
                        selectedStart = this.CaretIndex + 2;
                    else
                        selectedStart = this.CaretIndex + 1;
                    this.Text = text.Substring(0, 2) + "-" + text.Substring(2, 7);
                    e.Handled = true;
                    this.SelectionStart = selectedStart;
                }
                else if (!text.Contains("-"))
                    this.SelectionStart = selectedStart;

                //Set value for Value
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
                    this.Text = NewText.Substring(0, 2) + "-" + NewText.Substring(2, 7);
                else
                    this.Text = string.Empty;
                //base.ChangeBackGround();
            }
            catch
            {
                this.Text = string.Empty;
                //base.ChangeBackGround();
            }
        }

        #endregion


    }
}
