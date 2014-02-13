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
    public class TextBoxFax : TextBoxCustomBase
    {
        #region Contrustor
        public TextBoxFax()
            : base(string.Empty)//"Fax"
        {
            this.MaxLength = 10;
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
                int output = 0;
                if (!int.TryParse(text, out output))
                    this.IsChangeText = false;
                else
                {
                    //Set value for Value
                    this.Value = text.Replace("-", "");
                }
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
                                this.Text = text.Remove(i, 1).Replace("-", "");
                                ///Set value Value after removed
                                this.Value = Text;
                                ///Set SelectionStart after removed
                                if (text.Contains("-") && i > 6)
                                    this.SelectionStart = i - 2;
                                else if (text.Contains("-") && i > 3 && i <= 6)
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
                            this.Text = text.Remove(i, 1).Replace("-", "");
                            this.Value = Text;
                            ///Set SelectionStart after removed
                            if (text.Contains("-") && i > 6)
                                this.SelectionStart = i - 2;
                            else if (text.Contains("-") && i > 3 && i <= 6)
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
                else if (this.SelectionLength > 0
                    && this.SelectionLength == this.Text.Length
                    && CheckNonSymbolstrange(char.Parse(e.Text)))
                {
                    this.Text = string.Empty;
                    this.Value = string.Empty;
                }

                ///Check symbol input
                if ((this.Text.Contains("-") && this.Text.Replace("-", "").Length == 10)
                    || !CheckNonSymbolstrange(char.Parse(e.Text)))
                    e.Handled = true;

                //Set format text 
                else if (this.IsFocused && !e.Handled)
                {
                    string text = Text.Insert(CaretIndex, e.Text);
                    int selectedStart = this.CaretIndex;
                    if (text.Length == 10)
                    {
                        ///Set value for SelectionStart
                        if (this.CaretIndex >= 3 && this.CaretIndex < 6)
                            selectedStart = this.CaretIndex + 1;
                        else if (this.CaretIndex >= 6)
                            selectedStart = this.CaretIndex + 2;
                        this.Text = text.Substring(0, 3) + "-" + text.Substring(3, 3) + "-" + text.Substring(6, 4);
                        e.Handled = true;
                        this.SelectionStart = selectedStart + 1;
                    }
                    //Set value for Value
                    this.Value = text.Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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
                    this.Text = NewText.Substring(0, 3) + "-" + NewText.Substring(3, 3) + "-" + NewText.Substring(6, 4);
                }
                else
                    this.Text = string.Empty;
            }
            catch
            {
                this.Text = string.Empty;
            }
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
