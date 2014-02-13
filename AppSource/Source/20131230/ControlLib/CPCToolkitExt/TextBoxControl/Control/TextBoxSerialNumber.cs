using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxSerialNumber : TextBox
    {
        #region Field
        private Thickness _borderTextBoxBase;
        private Brush _backgroundTextBoxBase;
        #endregion

        #region Contrustor
        public TextBoxSerialNumber()
        {
            this.Loaded += new System.Windows.RoutedEventHandler(TextBoxSerialNumber_Loaded);
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            this.KeyUp += new KeyEventHandler(TextBoxSerialNumber_KeyUp);
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
            if (e.Text.Length == 0 ||
                (this.SelectionLength > 0 && this.SelectionLength < this.Text.Length))
            {
                return;
            }
            else if (this.SelectionLength > 0
                && this.SelectionLength == this.Text.Length
                && this.IsAlpha(e.Text))
            {
                this.Text = string.Empty;
            }

            //Set SelectionLength>0
            if (!this.IsAlpha(e.Text))
                e.Handled = true;

            base.OnPreviewTextInput(e);
        }
        #endregion

        #region Methods
        private bool IsAlpha(string input)
        {
            Regex pattern = new Regex("[a-z A-Z 0-9 -]");
            return pattern.IsMatch(input);
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
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                     DispatcherPriority.Input,
                                     (ThreadStart)delegate
                                     {
                                         this.BorderThickness = _borderTextBoxBase;
                                         this.Background = _backgroundTextBoxBase;
                                         this.IsReadOnly = false;
                                     });
            else
            {
                this.BorderThickness = _borderTextBoxBase;
                this.Background = _backgroundTextBoxBase;
                this.IsReadOnly = false;
            }
        }
        #endregion

        #region Event Control
        private void TextBoxSerialNumber_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.MaxLength == 0)
                this.MaxLength = 10;
            this._borderTextBoxBase = this.BorderThickness;
            this._backgroundTextBoxBase = this.Background;
        }

        private void TextBoxSerialNumber_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    if (this.Command != null)
                        this.Command.Execute(this.CommandParamater);
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<TextBoxSerialNumber_KeyUp>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion

        #region DependencyProperty

        #region IsTextBlock

        public bool IsTextBlock
        {
            get { return (bool)GetValue(IsTextBlockProperty); }
            set { SetValue(IsTextBlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextBlockProperty =
            DependencyProperty.Register("IsTextBlock", typeof(bool), typeof(TextBoxSerialNumber),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsTextBlock)));

        protected static void ChangeIsTextBlock(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue != e.OldValue && bool.Parse(e.NewValue.ToString()))
                (source as TextBoxSerialNumber).ChangeStyle();
            else
                (source as TextBoxSerialNumber).PreviousStyle();
        }

        #endregion

        #region Command
        //
        // Summary:
        //     Gets or sets the command to invoke when this button is pressed. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when this button is pressed. The default value is null.
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TextBoxSerialNumber));

        [Category("Common Properties")]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #endregion

        #region CommandParamater
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty CommandParamaterProperty =
            DependencyProperty.Register("CommandParamater", typeof(object), typeof(TextBoxSerialNumber));

        [Category("Common Properties")]
        public object CommandParamater
        {
            get { return (object)GetValue(CommandParamaterProperty); }
            set { SetValue(CommandParamaterProperty, value); }
        }
        #endregion


        #endregion

    }
}
