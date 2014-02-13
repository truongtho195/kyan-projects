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
    public class TextBoxZip : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxZip()
            : base(string.Empty)//"Zip"
        {
            this.MaxLength = 9;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
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
                int output = 0;
                if (!int.TryParse(text, out output))
                    this.IsChangeText = false;
                else
                    this.Value = text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
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
                            this.Text = this.Text.Remove(i, 1).Replace("-", "");
                            this.Value = this.Text;
                            if (text.Contains("-") && i > 5)
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

                        if (text.Contains("-") && i > 5)
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
            int output = 0;
            if (this.IsReadOnly && !int.TryParse(e.Text, out output))
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
                && CheckNonSymbolstrange(char.Parse(e.Text)))
            {
                this.Text = string.Empty;
                this.Value = string.Empty;
            }
            //Set SelectionLength>0
            if ((this.Text.Contains("-") && this.Text.Replace("-", "").Length == 9)
                 || !CheckNonSymbolstrange(char.Parse(e.Text))) //check symbol
                e.Handled = true;

             //Set format text 
            else if (this.IsFocused)
            {
                string text = Text.Insert(CaretIndex, e.Text);
                int selectedStart = this.CaretIndex;
                if (text.Length == 9)
                {
                    if (this.CaretIndex > 4)
                        selectedStart = this.CaretIndex + 2;
                    else
                        selectedStart = this.CaretIndex + 1;
                    this.Text = text.Substring(0, 5) + "-" + text.Substring(5, 4);
                    e.Handled = true;
                    this.SelectionStart = selectedStart;
                }
                else if (!text.Contains("-"))
                    this.SelectionStart = selectedStart;

                //Set value for Value
                Value = text.Replace("-", "");
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
                    if (NewText.Length == 9)
                        Text = NewText.Substring(0, 5) + "-" + NewText.Substring(5, 4);
                    else
                        Text = NewText;
                }
                else
                    this.Text = string.Empty;
                //base.ChangeBackGround();
            }
            catch
            {
                //base.ChangeBackGround();
            }
            base.UpdateText(NewText, root);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!this.IsChangeText)
            {
                this.IsChangeText = true;
                this.Text = string.Empty;
                this.Value = string.Empty;
            }
            base.OnTextChanged(e);
        }

        #endregion

    }
}
