using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Markup;
using System.Globalization;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBox : System.Windows.Controls.TextBox
    {
        #region Ctor
        public TextBox()
        {  
            //ResourceDictionary dictionary = new ResourceDictionary();
            //dictionary.Source = new Uri(@"pack://application:,,,/CPCToolkitExt;component/Theme/Dictionary.xaml");
            //this.Resources = dictionary;
            ////Set Style for TabItem
            //this.Style = this.FindResource("myTextBoxStyle") as Style;
            //this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;    
        }
        #endregion

        #region Event
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (this.IsScrollToHome)
                this.ScrollToHome();
            base.OnGotFocus(e);
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (this.IsScrollToHome)
                this.ScrollToHome();
            base.OnLostFocus(e);
        }
        #endregion

        #region DependencyProperties

        #region FocusingBrush
        [Category("Brushes")]
        public Brush FocusingBrush
        {
            get { return (Brush)GetValue(FocusingBrushProperty); }
            set { SetValue(FocusingBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FocusingBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusingBrushProperty =
            DependencyProperty.Register("FocusingBrush", typeof(Brush), typeof(TextBox), new UIPropertyMetadata(Brushes.Bisque));
        #endregion

        #region AllowDrag
        public bool AllowDrag
        {
            get { return (bool)GetValue(AllowDragProperty); }
            set { SetValue(AllowDragProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FocusingBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowDragProperty =
            DependencyProperty.Register("AllowDrag", typeof(bool), typeof(TextBox), new UIPropertyMetadata(true));
        #endregion

        #region IsInsideControl
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public bool IsInsideControl
        {
            get { return (bool)GetValue(IsInsideControlProperty); }
            set { SetValue(IsInsideControlProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsInsideControlProperty =
            DependencyProperty.Register(
                "IsInsideControl",
                typeof(bool),
                typeof(TextBox),
                new PropertyMetadata(false, OnIsInsideControlPropertyChanged));

        private static void OnIsInsideControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && e.NewValue is bool)
                    (d as TextBox).SetBorder(bool.Parse(e.NewValue.ToString()));
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<OnMaxDropDownHeightPropertyChanged>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion public double MaxDropDownHeight

        #region ControlLanguage
        /// <summary>
        /// Gets or sets the language of control
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>Gets or sets the language of control
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is English<see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public XmlLanguage ControlLanguage
        {
            get { return (XmlLanguage)GetValue(ControlLanguageProperty); }
            set { SetValue(ControlLanguageProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty ControlLanguageProperty =
            DependencyProperty.Register(
                "ControlLanguage",
                typeof(XmlLanguage),
                typeof(TextBox),
                new PropertyMetadata(null, OnIControlLanguageChanged));

        private static void OnIControlLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && e.NewValue is XmlLanguage)
                    (d as TextBox).ChangeLanguage((XmlLanguage)e.NewValue);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<OnMaxDropDownHeightPropertyChanged>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion public double MaxDropDownHeight

        #region StringFormat
        //
        // Summary:
        //     Gets or sets the horizontal alignment of the symbol currency of the text box. This
        //     is a dependency property.
        //
        // Returns:
        //     One of the Enum SymbolCurrencyAlignment values that specifies the horizontal
        //     alignment of the symbol currency of the text box. The default is SymbolCurrencyAlignment.Left.
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", typeof(string), typeof(TextBox), new UIPropertyMetadata("{0:N2}"));

        #endregion

        //#region ConverterCulture
        ////
        //// Summary:
        ////     Gets or sets the horizontal alignment of the symbol currency of the text box. This
        ////     is a dependency property.
        ////
        //// Returns:
        ////     One of the Enum SymbolCurrencyAlignment values that specifies the horizontal
        ////     alignment of the symbol currency of the text box. The default is SymbolCurrencyAlignment.Left.
        //public CultureInfo ConverterCulture
        //{
        //    get { return (CultureInfo)GetValue(ConverterCultureProperty); }
        //    set { SetValue(ConverterCultureProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ConverterCultureProperty =
        //    DependencyProperty.Register("ConverterCulture", typeof(CultureInfo), typeof(TextBox), new UIPropertyMetadata(new CultureInfo("en-US")));
        //#endregion

        #region IsInsideControl
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public bool IsFormatWithZero
        {
            get { return (bool)GetValue(IsFormatWithZeroProperty); }
            set { SetValue(IsFormatWithZeroProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsFormatWithZeroProperty =
            DependencyProperty.Register(
                "IsFormatWithZero",
                typeof(bool),
                typeof(TextBox),
                new PropertyMetadata(true));
        #endregion public double MaxDropDownHeight

        #region IsPermission
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public bool IsPermission
        {
            get { return (bool)GetValue(IsPermissionProperty); }
            set { SetValue(IsPermissionProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsPermissionProperty =
            DependencyProperty.Register(
                "IsPermission",
                typeof(bool),
                typeof(TextBox),
                new PropertyMetadata(true));

        #endregion public double MaxDropDownHeight

        #region IsSelectedAll
        public bool IsSelectedAll
        {
            get { return (bool)GetValue(IsSelectedAllProperty); }
            set { SetValue(IsSelectedAllProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelectedAll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedAllProperty =
            DependencyProperty.Register("IsSelectedAll", typeof(bool), typeof(TextBox), new UIPropertyMetadata(false));
        #endregion

        #region IsScrollToHome
        //
        // Summary:
        //     Scrolls the view of the editing control to the beginning of the viewport.
        public bool IsScrollToHome
        {
            get { return (bool)GetValue(IsScrollToHomeProperty); }
            set { SetValue(IsScrollToHomeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsScrollToHome.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsScrollToHomeProperty =
            DependencyProperty.Register("IsScrollToHome", typeof(bool), typeof(TextBox), new UIPropertyMetadata(false));
        #endregion

        #endregion

        #region Methods
        #region SetBorder
        /// <summary>
        /// SetBorder
        /// </summary>
        protected virtual void SetBorder(bool value)
        {

        }

        #endregion

        #region ChangeLanguage
        /// <summary>
        /// ChangeLanguage
        /// </summary>
        protected virtual void ChangeLanguage(XmlLanguage value)
        {

        }

        #endregion
        #endregion
    }
}
