using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using CPCToolkitExtLibraries;
using System.Text.RegularExpressions;

namespace CPCToolkitExt.AutoCompleteControl
{
    public partial class AutoCompleteBase : UserControl
    {
        #region Constructors
        public AutoCompleteBase()
        {
            this.IsLoad = false;
        }
        #endregion

        #region Field
        //Get the BorderThickness of Control
        protected Thickness BorderBase;
        //Get the Background of Control
        protected Brush BackgroundBase;
        //Set reload data of control
        protected bool IsLoad = false;
        //Set reload the contents of TextBox
        protected bool IsSelectedItem = false;
        protected bool IsPressCancelKey = false;
        protected bool IsKeyAcceptFromList = false;
        protected object SelectedItemClone = null;
        protected bool IsClickItem = false;
        #endregion

        #region Properties

        #region Columns
        // Get collection GridViewColumn for ListView
        private GridViewColumnCollection _columns;
        public GridViewColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new GridViewColumnCollection();
                }
                return _columns;
            }
        }
        #endregion

        #region ISOpenPopup
        /// <summary>
        ///  Get the IsOpen of the popup.
        /// </summary>
        public bool ISOpenPopup
        {
            get;
            set;
        }
        #endregion

        #endregion

        #region DependencyProperties

        #region ItemsSource

        /// <summary>
        /// ItemsSource of DataGrid Result
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoCompleteBase), new UIPropertyMetadata(null));

        #endregion

        #region SelectedItemResult
        /// <summary>
        /// Select item of Listbox autocomplete
        /// </summary>
        public object SelectedItemResult
        {
            get { return (object)GetValue(SelectedItemResultProperty); }
            set { SetValue(SelectedItemResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemResultProperty =
            DependencyProperty.Register("SelectedItemResult", typeof(object), typeof(AutoCompleteBase), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d as AutoCompleteBase).IsSelectedItem)
            {
                if (e.NewValue != null)
                    (d as AutoCompleteBase).SetValueWithSelectedItemResult();
                else
                    (d as AutoCompleteBase).ClearValue();
            }
        }

        #endregion

        #region SelectedValuePath
        //
        // Summary:
        //     Gets or sets the path that is used to get the System.Windows.Controls.Primitives.Selector.SelectedValue
        //     from the System.Windows.Controls.Primitives.Selector.SelectedItem.
        //
        // Returns:
        //     The path used to get the System.Windows.Controls.Primitives.Selector.SelectedValue.
        //     The default is an empty string.
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(AutoCompleteBase), new UIPropertyMetadata(string.Empty));
        #endregion

        #region SelectedValue
        //
        // Summary:
        //     Gets or sets the path that is used to get the System.Windows.Controls.Primitives.Selector.SelectedValue
        //     from the System.Windows.Controls.Primitives.Selector.SelectedItem.
        //
        // Returns:
        //     The path used to get the System.Windows.Controls.Primitives.Selector.SelectedValue.
        //     The default is an null.
        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(AutoCompleteBase), new PropertyMetadata(null, SelectedValueChanged));
        private static void SelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d as AutoCompleteBase).IsSelectedItem)
            {
                if (e.NewValue != null && e.NewValue != "")
                    (d as AutoCompleteBase).SetValueWithSelectedValue();
                else
                    (d as AutoCompleteBase).ClearValue();
            }
        }

        #endregion

        #region Text
        //
        // Summary:
        //     Gets or sets the text contents of the text box.
        //
        // Returns:
        //     A string containing the text contents of the text box. The default is an
        //     empty string ("").
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteBase), new UIPropertyMetadata(string.Empty));

        #endregion

        #region FieldSource
        /// <summary>
        /// Source for Field want to search
        /// </summary>
        public DataSearchCollection FieldSource
        {
            get { return (DataSearchCollection)GetValue(FieldSourceProperty); }
            set { SetValue(FieldSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FieldSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldSourceProperty =
            DependencyProperty.Register("FieldSource", typeof(DataSearchCollection), typeof(AutoCompleteBase));

        #endregion

        #region FieldShow
        /// <summary>
        /// Field to show data select in suggestion list
        /// </summary>
        public string FieldShow
        {
            get { return (string)GetValue(FieldShowProperty); }
            set { SetValue(FieldShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FieldShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldShowProperty =
            DependencyProperty.Register("FieldShow", typeof(string), typeof(AutoCompleteBase));

        #endregion

        #region NameChildrenControl
        public object NameChildrenControl
        {
            get { return (object)GetValue(NameChildrenControlProperty); }
            set { SetValue(NameChildrenControlProperty, value); }
        }
        // Using a DependencyProperty as the backing store for NameChildrenControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameChildrenControlProperty =
            DependencyProperty.Register("NameChildrenControl", typeof(object), typeof(AutoCompleteBase), new UIPropertyMetadata(null));
        #endregion

        #region AutoHiddenSelectedItem
        public bool AutoHiddenSelectedItem
        {
            get { return (bool)GetValue(AutoHiddenSelectedItemProperty); }
            set { SetValue(AutoHiddenSelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoHiddenSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoHiddenSelectedItemProperty =
            DependencyProperty.Register("AutoHiddenSelectedItem", typeof(bool), typeof(AutoCompleteBase), new UIPropertyMetadata(false));

        #endregion

        #region public bool IsTextCompletionEnabled
        /// <summary>
        /// Gets or sets a value indicating whether the first possible match
        /// found during the filtering process will be displayed automatically
        /// in the text box.
        /// </summary>
        /// <value>
        /// True if the first possible match found will be displayed
        /// automatically in the text box; otherwise, false. The default is
        /// false.
        /// </value>
        public bool IsTextCompletionEnabled
        {
            get { return (bool)GetValue(IsTextCompletionEnabledProperty); }
            set { SetValue(IsTextCompletionEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.IsTextCompletionEnabled" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.IsTextCompletionEnabled" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsTextCompletionEnabledProperty =
            DependencyProperty.Register(
                "IsTextCompletionEnabled",
                typeof(bool),
                typeof(AutoCompleteBase),
                new PropertyMetadata(false));

        #endregion public bool IsTextCompletionEnabled

        #region IsTextBlock

        public bool IsTextBlock
        {
            get { return (bool)GetValue(IsTextBlockProperty); }
            set { SetValue(IsTextBlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextBlockProperty =
            DependencyProperty.Register("IsTextBlock", typeof(bool), typeof(AutoCompleteBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsTextBlock)));

        protected static void ChangeIsTextBlock(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null
                && e.NewValue != e.OldValue
                && bool.Parse(e.NewValue.ToString()))
                (source as AutoCompleteBase).ChangeStyle();
            else
                (source as AutoCompleteBase).PreviousStyle();
        }

        #endregion

        #region IsReadOnly
        //
        // Summary:
        //     Gets or sets a value that indicates whether the text editing control is read-only
        //     to a user interacting with the control. This is a dependency property.
        //
        // Returns:
        //     true if the contents of the text editing control are read-only to a user;
        //     otherwise, the contents of the text editing control can be modified by the
        //     user. The default value is false.

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(AutoCompleteBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsReadOnly)));

        protected static void ChangeIsReadOnly(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue != e.OldValue && bool.Parse(e.NewValue.ToString()))
                (source as AutoCompleteBase).ChangeReadOnly(true);
            else
                (source as AutoCompleteBase).ChangeReadOnly(false);
        }

        #endregion

        #region FilterMode

        public AutoCompleteFilterMode FilterMode
        {
            get { return (AutoCompleteFilterMode)GetValue(FilterModeProperty); }
            set { SetValue(FilterModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterModeProperty =
            DependencyProperty.Register("FilterMode", typeof(AutoCompleteFilterMode), typeof(AutoCompleteBase), new UIPropertyMetadata(AutoCompleteFilterMode.StartsWith));


        #endregion

        #region MaxDropDownHeight
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(
                "MaxDropDownHeight",
                typeof(double),
                typeof(AutoCompleteBase),
                new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));

        private static void OnMaxDropDownHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && e.NewValue is double)
                    (d as AutoCompleteBase).SetMaxDropDownHeight(Double.Parse(e.NewValue.ToString()));

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<OnMaxDropDownHeightPropertyChanged>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion public double MaxDropDownHeight

        #region AutoChangeSelectedItem
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of SelectedItem when seletedvalue changed
        //
        // Returns:
        //     true if the user can change; otherwise, false. The registered default
        //     is false. For more information about what can influence the value, see System.Windows.DependencyProperty.
        public bool AutoChangeSelectedItem
        {
            get { return (bool)GetValue(AutoChangeSelectedItemProperty); }
            set { SetValue(AutoChangeSelectedItemProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoChangeSelectedItemProperty =
            DependencyProperty.Register("AutoChangeSelectedItem", typeof(bool), typeof(AutoCompleteBase), new UIPropertyMetadata(false));

        #endregion

        #region ButtonBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of Button Background
        //
        [Category("Brushes")]
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(AutoCompleteBase), new UIPropertyMetadata(Brushes.Brown));

        #endregion

        #region RegularPolygonBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of RegularPolygon BackgroundProperty
        //
        [Category("Brushes")]
        public Brush RegularPolygonBackground
        {
            get { return (Brush)GetValue(RegularPolygonBackgroundProperty); }
            set { SetValue(RegularPolygonBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegularPolygonBackgroundProperty =
            DependencyProperty.Register("RegularPolygonBackground", typeof(Brush), typeof(AutoCompleteBase), new UIPropertyMetadata(Brushes.White));

        #endregion

        #region SelectedItemBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of item.
        //
        [Category("Brushes")]
        public Brush SelectedItemBackground
        {
            get { return (Brush)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register("SelectedItemBackground", typeof(Brush), typeof(AutoCompleteBase), new UIPropertyMetadata(Brushes.Blue));

        #endregion

        #region MoveOverItemBrush
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of item.
        //
        [Category("Brushes")]
        public Brush MoveOverItemBrush
        {
            get { return (Brush)GetValue(MoveOverItemBrushProperty); }
            set { SetValue(MoveOverItemBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveOverItemBrushProperty =
            DependencyProperty.Register("MoveOverItemBrush", typeof(Brush), typeof(AutoCompleteBase), new UIPropertyMetadata(Brushes.Yellow));

        #endregion

        #region ColunmHeaderBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of ColunmHeader.
        //
        [Category("Brushes")]
        public Brush ColunmHeaderBackground
        {
            get { return (Brush)GetValue(ColunmHeaderBackgroundProperty); }
            set { SetValue(ColunmHeaderBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColunmHeaderBackgroundProperty =
            DependencyProperty.Register("ColunmHeaderBackground", typeof(Brush), typeof(AutoCompleteBase), new UIPropertyMetadata(Brushes.Blue));
        #endregion

        #region ColunmHeaderHeight
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public double ColunmHeaderHeight
        {
            get { return (double)GetValue(ColunmHeaderHeightProperty); }
            set { SetValue(ColunmHeaderHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty ColunmHeaderHeightProperty =
            DependencyProperty.Register(
                "ColunmHeaderHeight",
                typeof(double),
                typeof(AutoCompleteBase),
                new PropertyMetadata(double.PositiveInfinity));

        #endregion public double MaxDropDownHeight

        #region public IsClearText
        /// <summary>
        ///When ISClearText value is True,control will clear value of textBox on control.
        /// </summary>
        /// <value>
        /// True if the first possible match found will be displayed
        /// automatically in the text box; otherwise, false. The default is
        /// false.
        /// </value>
        public bool IsClearText
        {
            get { return (bool)GetValue(IsClearTextProperty); }
            set { SetValue(IsClearTextProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.IsTextCompletionEnabled" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.IsTextCompletionEnabled" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsClearTextProperty =
            DependencyProperty.Register(
                "IsClearText",
                typeof(bool),
                typeof(AutoCompleteBase),
                new PropertyMetadata(false, IsClearTextChanged));

        private static void IsClearTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue.Equals("") && bool.Parse(e.NewValue.ToString()))
                (d as AutoCompleteBase).ClearTextValue();
        }
        #endregion public bool IsTextCompletionEnabled

        #region VisibilityColunmHeader
        /// <summary>
        /// Gets,sets value of IsReadOnly.Default value is Visible.When value is Collapsed, listview will hide its header.
        /// </summary>

        public Visibility VisibilityColunmHeader
        {
            get { return (Visibility)GetValue(VisibilityColunmHeaderProperty); }
            set { SetValue(VisibilityColunmHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityColunmHeaderProperty =
            DependencyProperty.Register("VisibilityColunmHeader", typeof(Visibility), typeof(AutoCompleteBase),
        new PropertyMetadata(Visibility.Visible));
        #endregion

        #region public IsFocusControl
        /// <summary>
        ///When control is focused ,IsClearText is true.
        /// </summary>
        public bool IsFocusControl
        {
            get { return (bool)GetValue(IsFocusControlProperty); }
            set { SetValue(IsFocusControlProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.IsTextCompletionEnabled" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.IsTextCompletionEnabled" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsFocusControlProperty =
            DependencyProperty.Register(
                "IsFocusControl",
                typeof(bool),
                typeof(AutoCompleteBase),
                new PropertyMetadata(false));


        #endregion public bool IsTextCompletionEnabled

        #region IsNotBorder
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public bool IsNotBorder
        {
            get { return (bool)GetValue(IsNotBorderProperty); }
            set { SetValue(IsNotBorderProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsNotBorderProperty =
            DependencyProperty.Register(
                "IsNotBorder",
                typeof(bool),
                typeof(AutoCompleteBase),
                new PropertyMetadata(false, OnIsNotBorderPropertyChanged));

        private static void OnIsNotBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && e.NewValue is bool)
                    (d as AutoCompleteBase).SetBorder(bool.Parse(e.NewValue.ToString()));
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<OnMaxDropDownHeightPropertyChanged>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion public double MaxDropDownHeight

        #region BorderThicknessControlProperty
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteTextBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public Thickness BorderThicknessControl
        {
            get { return (Thickness)GetValue(BorderThicknessControlProperty); }
            set { SetValue(BorderThicknessControlProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty BorderThicknessControlProperty =
            DependencyProperty.Register(
                "BorderThicknessControl",
                typeof(Thickness),
                typeof(AutoCompleteBase),
                new PropertyMetadata(new Thickness(1)));

        #endregion public double MaxDropDownHeight

        #region TextBoxStyle
        //
        // Summary:
        //     Gets or sets a value that indicates whether the text editing control is read-only
        //     to a user interacting with the control. This is a dependency property.
        //
        // Returns:
        //     true if the contents of the text editing control are read-only to a user;
        //     otherwise, the contents of the text editing control can be modified by the
        //     user. The default value is false.

        public Style TextBoxStyle
        {
            get { return (Style)GetValue(TextBoxStyleProperty); }
            set { SetValue(TextBoxStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register("TextBoxStyle", typeof(Style), typeof(AutoCompleteBase),
        new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ChangeTextBoxStyle)));

        protected static void ChangeTextBoxStyle(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is Style)
                (source as AutoCompleteBase).SetStyle(e.NewValue as Style);
        }

        #endregion

        #region ColunmHeaderBorderBrush
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of ColunmHeader.
        //
        [Category("Brushes")]
        public Brush ColunmHeaderBorderBrush
        {
            get { return (Brush)GetValue(ColunmHeaderBorderBrushProperty); }
            set { SetValue(ColunmHeaderBorderBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColunmHeaderBorderBrushProperty =
            DependencyProperty.Register("ColunmHeaderBorderBrush", typeof(Brush), typeof(AutoCompleteBase), new UIPropertyMetadata(Brushes.Black));

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
                typeof(AutoCompleteBase),
                new PropertyMetadata(false, OnIsInsideControlPropertyChanged));

        private static void OnIsInsideControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && e.NewValue is bool)
                    (d as AutoCompleteBase).SetBorder(bool.Parse(e.NewValue.ToString()));
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<OnMaxDropDownHeightPropertyChanged>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion public double MaxDropDownHeight

        #region IsWatermark

        public bool IsWatermark
        {
            get { return (bool)GetValue(IsWatermarkProperty); }
            set { SetValue(IsWatermarkProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty IsWatermarkProperty =
            DependencyProperty.Register(
                "IsWatermark",
                typeof(bool),
                typeof(AutoCompleteBase),
                new PropertyMetadata(false, OnIsWatermarkPropertyChanged));

        private static void OnIsWatermarkPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<OnMaxDropDownHeightPropertyChanged>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion public double MaxDropDownHeight

        #region WatermarkContent
        [Category("Common Properties")]
        public string WatermarkContent
        {
            get { return (string)GetValue(WatermarkContentProperty); }
            set { SetValue(WatermarkContentProperty, value); }
        }
        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteTextBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty WatermarkContentProperty =
            DependencyProperty.Register(
                "WatermarkContent",
                typeof(string),
                typeof(AutoCompleteBase),
                new PropertyMetadata(string.Empty));

        #endregion public double MaxDropDownHeight

        #endregion

        #region Methods

        #region GetDataHasChildren
        /// <summary>
        /// Get data when search
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        protected bool GetDataHasChildren(object data, DataSearchModel searchModel, string text)
        {
            try
            {
                //object content = null;
                switch (searchModel.Level)
                {
                    ///To search data when level=0
                    case 0:
                        object dataLevel = data.GetType().GetProperty(searchModel.KeyName).GetValue(data, null);
                        if (dataLevel == null) return false;
                        else
                            return AutoCompleteSearch.GetFilter(FilterMode, text, dataLevel.ToString());

                    ///To search data when level=1
                    case 1:
                        object dataLevel1 = data.GetType().GetProperty(searchModel.PropertyChildren).GetValue(data, null);
                        if (dataLevel1 == null) return false;
                        else if (searchModel.PropertyType.Equals(CPCToolkitExtPropertyType.Collection.ToString()))
                        {
                            foreach (var item in (dataLevel1 as IEnumerable))
                            {
                                object contentLevel1 = item.GetType().GetProperty(searchModel.KeyName).GetValue(item, null);
                                if (contentLevel1 == null) return false;
                                if (AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel1.ToString()))
                                    return true;
                            }
                            return false;
                        }
                        else
                        {
                            object contentLevel1 = dataLevel1.GetType().GetProperty(searchModel.KeyName).GetValue(dataLevel1, null);
                            if (contentLevel1 == null) return false;
                            return AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel1.ToString());
                        }

                    ///To search data when level=2
                    case 2:
                        string[] arrayLevel = searchModel.PropertyChildren.Split('.');
                        object dataFirstArray = data.GetType().GetProperty(arrayLevel[0]).GetValue(data, null);

                        if (dataFirstArray == null) return false;

                        //Level 1 is colllection
                        else if (dataFirstArray is IEnumerable)
                        {
                            foreach (var itemFirstArray in (dataFirstArray as IEnumerable))
                            {
                                object dataItemFirstArray = itemFirstArray.GetType().GetProperty(arrayLevel[1]).GetValue(itemFirstArray, null);
                                if (dataItemFirstArray != null)
                                {
                                    //Level 2 is collection
                                    if (dataItemFirstArray is IEnumerable)
                                    {
                                        foreach (var itemLastArray in (dataItemFirstArray as IEnumerable))
                                        {
                                            //////////////************///////////////
                                            object dataItemLastArray = itemLastArray.GetType().GetProperty(searchModel.KeyName).GetValue(itemLastArray, null);
                                            if (dataItemLastArray != null && AutoCompleteSearch.GetFilter(FilterMode, text, dataItemLastArray.ToString()))
                                                return true;
                                        }
                                    }
                                    //Level 2 is model
                                    else
                                    {
                                        //////////////************///////////////
                                        object contentLevel2 = dataItemFirstArray.GetType().GetProperty(searchModel.KeyName).GetValue(dataItemFirstArray, null);
                                        if (contentLevel2 != null && AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel2.ToString()))
                                            return true;
                                    }
                                }
                            }
                            return false;
                        }

                        //Level 1 is model
                        else
                        {
                            object dataLevel2 = dataFirstArray.GetType().GetProperty(arrayLevel[1]).GetValue(dataFirstArray, null);
                            if (dataLevel2 == null) return false;
                            //Level 2 is collection
                            else if (searchModel.PropertyType.Equals(CPCToolkitExtPropertyType.Collection.ToString()))
                            {
                                foreach (var itemChildren in (dataLevel2 as IEnumerable))
                                {
                                    object contentLevel2 = itemChildren.GetType().GetProperty(searchModel.KeyName).GetValue(itemChildren, null);
                                    if (contentLevel2 != null && AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel2.ToString()))
                                        return true;
                                }
                                return false;
                            }
                            //Level 2 is model
                            else
                            {
                                object contentLevel2 = dataLevel2.GetType().GetProperty(searchModel.KeyName).GetValue(dataLevel2, null);
                                if (contentLevel2 == null) return false;
                                return AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel2.ToString());
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<GetDataHasChildren>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }

            return false;
        }

        #endregion

        #region GetDataFieldShow

        /// <summary>
        /// Gets the text contents of the text box.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldshow"></param>
        /// <returns></returns>
        protected string GetDataFieldShow(object data, string fieldshow)
        {
            string content = string.Empty;
            try
            {
                string[] level = fieldshow.Split('.');
                switch (level.Count())
                {
                    case 1:
                        object datalevel = data.GetType().GetProperty(level[0]).GetValue(data, null);
                        if (datalevel == null) content = string.Empty;
                        else
                            content = datalevel.ToString();
                        break;
                    case 2:
                        object datahaschildren = data.GetType().GetProperty(level[0]).GetValue(data, null);
                        if (datahaschildren == null)
                            return content = string.Empty;
                        object datalevel1 = datahaschildren.GetType().GetProperty(level[1]).GetValue(datahaschildren, null);
                        if (datalevel1 == null)
                            content = string.Empty;
                        else
                            content = datalevel1.ToString();
                        break;
                    default:
                        return content = string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<GetDataFieldShow>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
                return string.Empty;
            }
            return content;
        }

        #endregion

        #region SetValueWithSelectedItemResult
        /// <summary>
        /// Gets the text contents of the text box when selected item in ListView.
        /// </summary>
        protected virtual void SetValueWithSelectedItemResult()
        {

        }

        #endregion

        #region SetValueWithSelectedValue
        /// <summary>
        /// Gets the text contents of the text box when selected value in ListView.
        /// </summary>
        protected virtual void SetValueWithSelectedValue()
        {

        }

        #endregion

        #region Filter
        //
        // Summary:
        //     Gets or sets a callback used to determine if an item is suitable for inclusion
        //     in the view.
        //
        // Returns:
        //     A method used to determine if an item is suitable for inclusion in the view.
        //

        #endregion

        #region ChangeReadOnly
        protected virtual void ChangeReadOnly(bool isReadOnly)
        {

        }
        #endregion

        #region ChangeStyle
        protected virtual void ChangeStyle()
        {
            //Set Background ,BorderThickness when IsReadOnly= true
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                   DispatcherPriority.Input,
                                   (ThreadStart)delegate
                                   {
                                       this.Background = Brushes.Transparent;
                                       this.BorderThickness = new Thickness(0);
                                   });
            else
            {
                this.Background = Brushes.Transparent;
                this.BorderThickness = new Thickness(0);
            }
        }
        #endregion

        #region PreviousStyle
        protected virtual void PreviousStyle()
        {
            //Return value for Background,BorderThickness when IsReadOnly= false
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                   DispatcherPriority.Input,
                                   (ThreadStart)delegate
                                   {
                                       this.Background = this.BackgroundBase;
                                       this.BorderThickness = this.BorderBase;
                                   });

            else
            {
                this.Background = this.BackgroundBase;
                this.BorderThickness = this.BorderBase;
            }
        }
        #endregion

        #region ClearValue
        /// <summary>
        /// Clear value
        /// </summary>
        protected virtual void ClearValue()
        {

        }

        #endregion

        #region SetFocus
        /// <summary>
        /// Set focus TextBox
        /// </summary>
        public virtual void SetFocus()
        {

        }

        #endregion

        #region SetMaxDropDownHeight
        protected virtual void SetMaxDropDownHeight(double value)
        {

        }

        #endregion

        #region Key Method

        protected bool IsCancelKey(Key key)
        {
            if (key == Key.Escape)
                this.IsPressCancelKey = true;
            if (key == Key.Enter || key == Key.Return)
                this.IsClickItem = true;
            return key == Key.Escape
                || key == Key.Enter
                || key == Key.Tab;
        }

        protected bool IsChooseCurrentItemKey(Key pressed)
        {
            if (pressed == Key.Enter || pressed == Key.Return)
                this.IsClickItem = true;
            return pressed == Key.Enter
                || pressed == Key.Return
                || pressed == Key.Tab;
        }

        protected bool IsNavigationKey(Key Pressed)
        {

            return Pressed == Key.Up
                || Pressed == Key.Down
                || Pressed == Key.PageUp
                || Pressed == Key.NumPad8
                || Pressed == Key.NumPad2
                || Pressed == Key.PageDown;
        }

        protected int GetIncrementValueForKey(Key pressed)
        {
            switch (pressed)
            {
                case Key.Down:
                case Key.Up:
                case Key.NumPad8:
                case Key.NumPad2:
                    return 1;
                default:
                    return 0;
            }
        }

        #endregion

        #region ContentAlignment

        // Summary:
        //     Gets or sets the horizontal alignment of the control's content.

        // Returns:
        //     One of the System.Windows.HorizontalAlignment values. The default is System.Windows.HorizontalAlignment.Left.
        public HorizontalAlignment ContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(ContentAlignmentProperty); }
            set { SetValue(ContentAlignmentProperty, value); }
        }

        //Using a DependencyProperty as the backing store for ContentAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register("ContentAlignment", typeof(HorizontalAlignment), typeof(AutoCompleteBase), new UIPropertyMetadata(HorizontalAlignment.Left));

        #endregion

        #region ClearTextValue
        /// <summary>
        /// Clear text value
        /// </summary>
        protected virtual void ClearTextValue()
        {

        }

        #endregion

        #region SetBorder
        /// <summary>
        /// Clear value
        /// </summary>
        protected virtual void SetBorder(bool value)
        {

        }

        #endregion

        #region SetStyle
        /// <summary>
        /// SetStyle
        /// </summary>
        protected virtual void SetStyle(Style value)
        {

        }
        #endregion

        #endregion
    }
}
