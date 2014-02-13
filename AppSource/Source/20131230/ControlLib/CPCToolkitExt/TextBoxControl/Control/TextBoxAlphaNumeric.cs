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
    public class TextBoxAlphaNumeric : TextBoxNumberBase
    {
        #region Contrustor
        public TextBoxAlphaNumeric()
        {
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.IsUndoEnabled = false;
            this.Loaded += new RoutedEventHandler(TextBoxAlphaNumeric_Loaded);
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
        }
        #endregion

        #region DependencyProperties
        //
        // Summary:
        //     Gets or sets the type of text contents value return of the text box. This is a dependency property.
        //
        public TextType TextType
        {
            get { return (TextType)GetValue(TextTypeProperty); }
            set { SetValue(TextTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextTypeProperty =
            DependencyProperty.Register("TextType", typeof(TextType), typeof(TextBoxAlphaNumeric), new UIPropertyMetadata(TextType.Numeric));


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
            switch (e.Key)
            {
                case Key.Space:
                    e.Handled = true;
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
            if (e.Text.Length == 0
                || (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length)
                || (this.Text.Length == this.MaxLength && this.SelectionLength == 0))
            {
                e.Handled = true;
                return;
            }

            //Set SelectionLength>0
            if (((this.TextType == TextType.Alpha && !this.IsAlpha(e.Text))
                || (this.TextType == TextType.Numeric && !this.IsNumber(e.Text))))
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewTextInput(e);
        }
        #endregion

        #region Methods
        private bool IsAlpha(string input)
        {
            Regex pattern = new Regex("[a-z A-Z]");
            return pattern.IsMatch(input);
        }
        #endregion

        #region Event
        void TextBoxAlphaNumeric_Loaded(object sender, RoutedEventArgs e)
        {   
            if (this.MaxLength == 0)
                this.MaxLength = 10;
        }
        #endregion

    }

    public enum TextType
    {
        // Summary:
        //     Default. Symbol is aligned to the left.
        Alpha = 0,
        //
        // Summary:
        //     Symbol is aligned to the right.
        Numeric = 1
    }
}
