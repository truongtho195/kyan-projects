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

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxUserName : TextBox
    {
        #region Contrustor
        public TextBoxUserName()
        {
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            this.Loaded += new RoutedEventHandler(TextBoxUserName_Loaded);
        }

        void TextBoxUserName_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.MaxLength == 0)
                this.MaxLength = 30;
        }
        #endregion

        #region Field
        private string _tempContent = string.Empty;
        private bool _flagUnikey = false;
        #endregion

        #region Override Methods
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.IsReadOnly)
            {
                e.Handled = true;
                return;
            }
            switch (e.Key)
            {
                //Block key Space
                case Key.Space:
                    e.Handled = true;
                    break;

                //Block key Enter
                case Key.Enter:
                    e.Handled = true;
                    break;
            }
            base.OnPreviewKeyDown(e);
        }
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            //Check value==null
            if (this.IsReadOnly 
                || e.Text.Length == 0
                || (this.Text.Length == this.MaxLength && (SelectionLength == 0))
                || !IsAlphaNumeric(e.Text))
            {
                e.Handled = true;
                return;
            }

            //Selected all text
            else if (this.SelectionLength > 0 
                && this.SelectionLength == this.Text.Length)
            {
                this.Text = string.Empty;
            }
            //return text value 
            else
            {
                int selectionstart = this.SelectionStart;
                this.Text = this.Text.Insert(this.CaretIndex, e.Text);
                this.SelectionStart = selectionstart + 1;
                e.Handled = true;
                //_tempContent = this.Text;//set temp value
            }
            base.OnPreviewTextInput(e);

        }


        #endregion

        #region Methods
        /// <summary>
        /// Check text input 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool IsAlphaNumeric(string input)
        {
            Regex pattern = new Regex("[a-zA-Z0-9]");
            return pattern.IsMatch(input);
        }
        #endregion
    }
}
