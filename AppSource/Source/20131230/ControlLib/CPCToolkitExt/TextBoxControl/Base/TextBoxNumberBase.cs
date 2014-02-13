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
using System.Reflection;
using System.Resources;
using System.Windows.Interop;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxNumberBase : TextBox
    {
        #region Field
        protected Thickness BorderTextBoxBase;
        protected Brush BackgroundTextBoxBase;
        #endregion

        #region Contrustor
        public TextBoxNumberBase()
        {
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.IsUndoEnabled = false;
            this.AcceptsReturn = false;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            this.Loaded += new RoutedEventHandler(TextBoxNumberBase_Loaded);
        }
        #endregion

        #region DependencyProperties

        #region Value
        //
        // Summary:
        //     Gets or sets the text contents value return of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the text contents of the text box. The default is an
        //     "".
        public static readonly DependencyProperty ValueDependencyProperty = DependencyProperty.Register(
        "Value",
        typeof(string),
        typeof(TextBoxNumberBase),
         new UIPropertyMetadata(string.Empty, ChangeText));

        public string Value
        {
            get { return (string)GetValue(ValueDependencyProperty); }
            set { SetValue(ValueDependencyProperty, value); }
        }
        #endregion

        #region IsTextBlock

        public bool IsTextBlock
        {
            get { return (bool)GetValue(IsTextBlockProperty); }
            set { SetValue(IsTextBlockProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsTextBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextBlockProperty =
            DependencyProperty.Register("IsTextBlock", typeof(bool), typeof(TextBoxNumberBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsTextBlock)));

        protected static void ChangeIsTextBlock(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && bool.Parse(e.NewValue.ToString()))
                (source as TextBoxNumberBase).ChangeStyle();
            else
                (source as TextBoxNumberBase).PreviousStyle();
        }

        #endregion

        #region BackgroundControl

        public Brush BackgroundControl
        {
            get { return (Brush)GetValue(BackgroundControlProperty); }
            set { SetValue(BackgroundControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChangeBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundControlProperty =
            DependencyProperty.Register("BackgroundControl", typeof(Brush), typeof(TextBoxNumberBase), new UIPropertyMetadata(Brushes.White));


        #endregion

        #endregion

        #region Event
        private void TextBoxNumberBase_Loaded(object sender, RoutedEventArgs e)
        {
            //BackgroundBase
            if (this.BackgroundTextBoxBase == null)
                this.BackgroundTextBoxBase = this.Background;
            //BorderBase
            if (this.BorderTextBoxBase == null || this.BorderTextBoxBase == new Thickness(0))
                this.BorderTextBoxBase = this.BorderThickness;
            this.AcceptsReturn = false;
        }
        #endregion

        #region Methods

        protected static void ChangeText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue != e.OldValue)
                (source as TextBoxNumberBase).SetValueFormat(e.NewValue.ToString());
            else
                (source as TextBoxNumberBase).SetValueDefault();
        }

        public virtual void SetValueFormat(string content)
        {

        }

        public bool IsNumber(string input)
        {
            Regex pattern = new Regex("[0-9]");
            return pattern.IsMatch(input);
        }

        public bool IsNonSymbolstrange(string input)
        {
            Regex pattern = new Regex("[0-9,.-]");
            return pattern.IsMatch(input);
        }

        public bool IsNumeric(string input)
        {
            Regex pattern = new Regex("[0-9,.]");
            return pattern.IsMatch(input);
        }

        public virtual void SetValueDefault()
        {
            this.Text = string.Empty;
        }

        public virtual void ChangeStyle()
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

        public virtual void PreviousStyle()
        {
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                           DispatcherPriority.Input,
                                           (ThreadStart)delegate
                                           {
                                               this.BorderThickness = this.BorderTextBoxBase;
                                               this.Background = this.BackgroundTextBoxBase;
                                               this.IsReadOnly = false;
                                           });
            else
            {
                this.BorderThickness = this.BorderTextBoxBase;
                this.Background = this.BackgroundTextBoxBase;
                this.IsReadOnly = false;
            }
        }
        #endregion
    }
}
