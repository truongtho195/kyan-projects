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
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using CPCToolkitExtLibraries;

namespace CPCToolkitExt.ComboBoxControl
{
    /// <summary>
    /// Interaction logic for ComboBoxDiscount.xaml
    /// </summary>
    public partial class ComboBoxDiscount : ComboBoxBase
    {
        #region Contrustor
        public ComboBoxDiscount()
        {
            InitializeComponent();
            //To register event for Control
            this.Loaded += new RoutedEventHandler(ComboBoxDiscount_Loaded);
            //To register event for Button
            this.btnShowPopup.Click += new RoutedEventHandler(btnShowPopup_Click);
            //To register event for TextBox
            this.txtKeyWord.PreviewTextInput += new TextCompositionEventHandler(KeyWord_PreviewTextInput);
            this.txtKeyWord.TextChanged += new TextChangedEventHandler(KeyWord_TextChanged);
            this.txtKeyWord.PreviewKeyDown += new KeyEventHandler(KeyWord_PreviewKeyDown);
            this.txtKeyWord.KeyUp += new KeyEventHandler(txtKeyWord_KeyUp);
            //To register event for ListView
            this.lstContent.KeyUp += new KeyEventHandler(lstContent_KeyUp);
            this.lstContent.SelectionChanged += new SelectionChangedEventHandler(lstContent_SelectionChanged);
            //To register event for Popup
            this.popupContent.Closed += new EventHandler(PopupContent_Closed);
            this.txtKeyWord.AllowDrop = false;
            this.txtKeyWord.ContextMenu = null;
            this.txtKeyWord.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.txtKeyWord.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.txtKeyWord.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            this.txtKeyWord.GotFocus += new RoutedEventHandler(TextBox_GotFocus);
        }

        #endregion

        #region Properties

        #region DiscountCollection
        /// <summary>
        /// Get ,set collection of discount
        /// </summary>
        private DiscountCollection _discountCollection = new DiscountCollection();
        protected DiscountCollection DiscountCollection
        {
            get { return _discountCollection; }
            set
            {
                if (_discountCollection != value)
                {
                    _discountCollection = value;
                    RaisePropertyChanged(() => DiscountCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Open ,close popup
        private void OpenPopup()
        {
            if (!this.popupContent.IsOpen)
            {
                this.popupContent.StaysOpen = false;
                this.popupContent.IsOpen = true;
            }
            base.ISOpenPopup = true;
        }

        private void ClosePopup(bool isEmpty)
        {
            if (isEmpty)
            {
                this.lstContent.SelectedItem = null;
            }
            this.SetFocus();
            this.popupContent.StaysOpen = true;
            this.popupContent.IsOpen = false;
            base.IsSelectedItem = false;
        }
        #endregion

        #region SetValue
        private void SetValueDefault()
        {
            try
            {
                this.DiscountCollection.Clear();
                DiscountModel model = new DiscountModel();
                model.Value = 0;
                model.Type = true;
                model.Content = "0%";
                this.DiscountCollection.Add(model);

                DiscountModel model1 = new DiscountModel();
                model1.Value = 0;
                model1.Type = false;
                model1.Content = this.FormatCurrency(0);
                this.DiscountCollection.Add(model1);

                this.lstContent.ItemsSource = this.DiscountCollection;
                this.lstContent.DisplayMemberPath = "Content";
            }
            catch (Exception ex)
            {
                Debug.Write("Set Value Default Discount Control" + "<<<<<<<<<<<<<<<<" + ex.ToString() + ">>>>>>>>>>>>>>>>>");
            }
        }

        private void SetValue(bool isChangedvalue)
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                        DispatcherPriority.Input,
                        (ThreadStart)delegate
                        {
                            this.SetValueDefault();
                            if (this.Value == null) return;
                            base.IsSelectedItem = true;
                            if (isChangedvalue)
                            {
                                ///Set data 
                                decimal value = Decimal.Parse(this.Value.ToString());
                                this.DiscountCollection[0].Value = value;
                                this.DiscountCollection[0].Content = this.FormatPercent(this.DiscountCollection[0].Value);
                                if (Decimal.Parse(this.Value.ToString()) < 0)
                                    this.DiscountCollection[1].Value = Decimal.Parse(this.Value.ToString().Remove(0, 1), CultureInfo.InvariantCulture);
                                else
                                    this.DiscountCollection[1].Value = value;
                                this.DiscountCollection[1].Content = this.FormatCurrency(this.DiscountCollection[1].Value);
                                this.lstContent.SelectedItem = this.DiscountCollection.Where(x => x.Value == value && x.Type == this.IsPercent).SingleOrDefault();
                                this.txtKeyWord.Text = this.DiscountCollection[this.lstContent.SelectedIndex].Content;
                            }
                            else
                            {
                                this.txtKeyWord.Text = this.DiscountCollection[0].Content;
                                this.lstContent.SelectedIndex = 0;
                                this.Value = this.DiscountCollection[0].Value;
                                this.IsPercent = false;
                            }
                            base.IsSelectedItem = false;
                        });
            }
            catch (Exception ex)
            {
                Debug.Write("Set value Discount Control" + "<<<<<<<<<<<<<<<<" + ex.ToString() + ">>>>>>>>>>>>>>>>>");
            }
        }
        #endregion

        #region ChangedValue
        private void ChangedValue()
        {
            try
            {
                base.IsSelectedItem = true;
                if (this.txtKeyWord.Text.Contains("$")
                     || this.txtKeyWord.Text.Contains("%"))
                {
                    StringBuilder text = new StringBuilder(this.txtKeyWord.Text);

                    //Set valued before symbol "$",".","%"
                    if (this.txtKeyWord.Text.Length == 1 && (this.txtKeyWord.Text.ToString().Contains("$") || this.txtKeyWord.Text.ToString().Contains("%") || this.txtKeyWord.Text.ToString().Contains(".")))
                        text = new StringBuilder("0");
                    else if (this.txtKeyWord.Text.Contains("$"))
                    {
                        text = text.Remove(0, 1);
                    }
                    else
                        text = text.Remove(this.txtKeyWord.Text.Length - 1, 1);

                    //Set value for ListBox
                    if (text.ToString().Contains("-"))
                    {
                        this.DiscountCollection[1].Value = Decimal.Parse(text.ToString(), CultureInfo.InvariantCulture);
                        this.DiscountCollection[0].Value = Decimal.Parse(text.Remove(0, 1).ToString(), CultureInfo.InvariantCulture);

                    }
                    else
                        this.DiscountCollection[1].Value = this.DiscountCollection[0].Value = Decimal.Parse(text.ToString(), CultureInfo.InvariantCulture);

                    this.DiscountCollection[0].Content = this.FormatPercent(this.DiscountCollection[0].Value);
                    this.DiscountCollection[1].Content = FormatCurrency(this.DiscountCollection[1].Value);
                    this.lstContent.SelectedItem = this.DiscountCollection.Where(x => x.Content.Contains(this.txtKeyWord.Text)).SingleOrDefault();

                }
                else
                {
                    string text = string.Empty;
                    if (this.txtKeyWord.Text.ToString().Contains("-") && this.txtKeyWord.Text.Length == 1)
                    {
                        this.DiscountCollection[1].Value = Decimal.Parse("0", CultureInfo.InvariantCulture);
                        this.DiscountCollection[0].Value = Decimal.Parse("0", CultureInfo.InvariantCulture);
                    }
                    else if (this.txtKeyWord.Text.ToString().Contains("-"))
                    {
                        this.DiscountCollection[0].Value = Decimal.Parse(this.txtKeyWord.Text, CultureInfo.InvariantCulture);
                        this.DiscountCollection[1].Value = Decimal.Parse(this.txtKeyWord.Text.Remove(0, 1), CultureInfo.InvariantCulture);
                    }
                    else
                        this.DiscountCollection[0].Value = this.DiscountCollection[1].Value = Decimal.Parse(this.txtKeyWord.Text, CultureInfo.InvariantCulture);

                    this.DiscountCollection[0].Content = this.FormatPercent(this.DiscountCollection[0].Value);//this.DiscountCollection[0].Type;
                    this.DiscountCollection[1].Content = this.FormatCurrency(this.DiscountCollection[1].Value);//this.DiscountCollection[1].Type;
                    this.lstContent.SelectedItem = this.DiscountCollection[0];
                }

                //if (this.lstContent.SelectedItem != null)
                //{
                //    this.txtKeyWord.Text
                //    //this.Value = this.DiscountCollection[this.lstContent.SelectedIndex].Value;
                //    ///this.IsPercent = this.DiscountCollection[this.lstContent.SelectedIndex].Type;
                //}
                //else
                //    this.Value = null;
                this.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< Changed Value DisCount Control" + ex.ToString());
            }
        }

        private void ClearValue()
        {
            base.IsSelectedItem = true;
            this.Value = AutoCompleteHelper.SetTypeBinding(this.Value);
            this.IsPercent = true;
            this.txtKeyWord.Text = string.Empty;
            base.IsSelectedItem = false;
        }
        #endregion

        #region Format string
        private string FormatCurrency(decimal content)
        {
            return String.Format(CultureInfo.InvariantCulture, "{1}{0:#,0.00}", content, "$"); ;
        }

        private string FormatPercent(decimal content)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}", content, "%"); ;
        }

        private bool IsNumericExt(string input)
        {
            Regex pattern = new Regex("[-.0-9%$]");
            return pattern.IsMatch(input);
        }
        #endregion

        #region SetFocusTextBox
        private void SetValueTextBox()
        {
            try
            {
                base.IsSelectedItem = true;
                if (this.lstContent.SelectedItem != null)
                {
                    this.SetFocusTextBox();
                    this.Value = this.DiscountCollection[this.lstContent.SelectedIndex].Value;
                    this.IsPercent = this.DiscountCollection[this.lstContent.SelectedIndex].Type;
                }
                else
                    this.ClearValue();
                base.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< Set value TextBox Discount " + ex.ToString());
            }
        }

        private void SetFocusTextBox()
        {
            try
            {
                base.IsSelectedItem = true;
                if (this.lstContent.SelectedIndex >= 0)
                {
                    if (this.txtKeyWord.Text != this.DiscountCollection[this.lstContent.SelectedIndex].Content)
                        this.txtKeyWord.Text = this.DiscountCollection[this.lstContent.SelectedIndex].Content;
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                               DispatcherPriority.Input,
                               (ThreadStart)delegate
                               {
                                   this.txtKeyWord.SelectAll();
                               });
                }
                else
                    this.txtKeyWord.Text = string.Empty;

                base.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< SetFocusTextBox >>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region Change style
        protected override void ChangeStyle()
        {
            this.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      this.txtKeyWord.BorderThickness = new Thickness(0);
                                      this.btnShowPopup.Visibility = Visibility.Collapsed;
                                      this.txtKeyWord.IsReadOnly = true;
                                      this.recIsTextBlock.Visibility = Visibility.Visible;
                                  });
            base.ChangeStyle();
        }
        protected override void PreviousStyle()
        {
            this.Dispatcher.BeginInvoke(
                               DispatcherPriority.Input,
                               (ThreadStart)delegate
                               {
                                   this.txtKeyWord.BorderThickness = new Thickness(1, 1, 1.2, 1);
                                   this.btnShowPopup.Visibility = Visibility.Visible;
                                   this.txtKeyWord.IsReadOnly = false;
                                   this.recIsTextBlock.Visibility = Visibility.Collapsed;
                               });
            base.PreviousStyle();
        }
        #endregion

        #region ChangeReadOnly
        protected override void ChangeReadOnly(bool isReadOnly)
        {
            this.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      if (isReadOnly)
                                      {
                                          this.txtKeyWord.IsReadOnly = true;
                                          this.btnShowPopup.IsEnabled = false;
                                          this.popupContent.Visibility = Visibility.Collapsed;
                                      }
                                      else
                                      {
                                          this.txtKeyWord.IsReadOnly = false;
                                          this.btnShowPopup.IsEnabled = true;
                                          this.popupContent.Visibility = Visibility.Visible;
                                      }
                                  });
            base.ChangeReadOnly(isReadOnly);
        }
        #endregion

        #region SetFocus
        /// <summary>
        /// Set focus for TextBox
        /// </summary>
        public void SetFocus()
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     Keyboard.Focus(this.txtKeyWord);
                                     if (!string.IsNullOrEmpty(this.txtKeyWord.Text) && this.txtKeyWord.Text.Length > 0)
                                         this.txtKeyWord.SelectAll();
                                 });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<SetFocus>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        /// <summary>
        /// Set focus ListViewItem
        /// </summary>
        private void SetFocusListViewItem()
        {
            try
            {
                this.lstContent.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     if (this.lstContent.SelectedIndex >= 0)
                                     {
                                         ListBoxItem listBoxItem = (ListBoxItem)lstContent.ItemContainerGenerator.ContainerFromIndex(this.lstContent.SelectedIndex);
                                         if (listBoxItem != null)
                                             listBoxItem.Focus();
                                     }
                                 });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<SetFocusListViewItem>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion

        #endregion

        #region DependencyProperties

        #region DiscountType
        public DiscountType DiscountTypeDefault
        {
            get { return (DiscountType)GetValue(DiscountTypeDefaultProperty); }
            set { SetValue(DiscountTypeDefaultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DiscountTypeDefault.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DiscountTypeDefaultProperty =
            DependencyProperty.Register("DiscountTypeDefault", typeof(DiscountType), typeof(ComboBoxDiscount), new UIPropertyMetadata(DiscountType.Percent));

        #endregion

        #region UsingSeparator
        public bool IsUsingSeparator
        {
            get { return (bool)GetValue(IsUsingSeparatorProperty); }
            set { SetValue(IsUsingSeparatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsUsingSeparator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUsingSeparatorProperty =
            DependencyProperty.Register("IsUsingSeparator", typeof(bool), typeof(ComboBoxDiscount), new UIPropertyMetadata(false));
        #endregion

        #region Negativenumber

        public bool IsNegativeNumber
        {
            get { return (bool)GetValue(IsNegativeNumberProperty); }
            set { SetValue(IsNegativeNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNegativeNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNegativeNumberProperty =
            DependencyProperty.Register("IsNegativeNumber", typeof(bool), typeof(ComboBoxDiscount), new UIPropertyMetadata(false));

        #endregion

        #region IsPercent

        public bool IsPercent
        {
            get { return (bool)GetValue(IsPercentProperty); }
            set { SetValue(IsPercentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPercent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPercentProperty =
            DependencyProperty.Register("IsPercent", typeof(bool), typeof(ComboBoxDiscount),
        new UIPropertyMetadata(true, ChangeValueIsPercent));

        protected static void ChangeValueIsPercent(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as ComboBoxDiscount).IsSelectedItem
                && e.NewValue != null
                && !string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                (source as ComboBoxDiscount).SetValue(true);
            }
        }
        #endregion

        #region Value

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(ComboBoxDiscount),
        new UIPropertyMetadata(null, ChangeValue));

        protected static void ChangeValue(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as ComboBoxDiscount).IsSelectedItem
                && e.NewValue != null
                && !string.IsNullOrEmpty(e.NewValue.ToString())
                && !String.Format(CultureInfo.InvariantCulture, "{0}", e.NewValue).Equals(String.Format(CultureInfo.InvariantCulture, "{0}", e.OldValue)))
            {
                (source as ComboBoxDiscount).SetValue(true);
            }
            else if (!(source as ComboBoxDiscount).IsSelectedItem
              && e.NewValue == null)
            {
                (source as ComboBoxDiscount).ClearValue();
            }
        }
        #endregion

        #endregion

        #region The event of control

        #region The events of control
        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxDiscount_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Value == null
                || string.IsNullOrEmpty(this.Value.ToString()))
            {
                this.SetValue(false);
            }
            if (this.InputMaxLength > 0)
                this.txtKeyWord.MaxLength = this.InputMaxLength;
            else
                this.txtKeyWord.MaxLength = 9;
            if (base.BackgroundBase == null)
                base.BackgroundBase = this.Background;
            if (base.BorderBase == null || base.BorderBase == new Thickness(0))
                base.BorderBase = this.BorderThickness;
            base.IsSelectedItem = false;
            base.ISOpenPopup = false;
        }

        /// <summary>
        /// Close Popup when Click item in ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Content_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Close Popup
            this.ClosePopup(false);
        }

        private void lstContent_KeyUp(object sender, KeyEventArgs e)
        {
            //Close Popup
            if (this.IsCancelKey(e.Key))
                this.ClosePopup(false);
        }

        private void lstContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (base.IsNavigation)
                this.SetFocusTextBox();

        }
        #endregion

        #region The event of TextBox
        /// <summary>
        /// TextChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (this.txtKeyWord.IsReadOnly) return;
                if (!base.IsSelectedItem)
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                        DispatcherPriority.Input,
                        (ThreadStart)delegate
                        {
                            //Setvalue
                            if (!string.IsNullOrEmpty(this.txtKeyWord.Text)
                                 && this.txtKeyWord.Text.Trim().Length > 0)
                            {
                                this.ChangedValue();
                                this.OpenPopup();
                            }
                            else//Close Popup
                                this.ClosePopup(true);
                        });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<< KeyWord_TextChanged" + ex.ToString());
            }
        }
        /// <summary>
        /// PreviewTextInput
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyWord_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //Set input Text
                if (this.txtKeyWord.IsReadOnly || e.Text.Length == 0
                    || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 45) && !this.IsNegativeNumber)
                    || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 45 || char.ConvertToUtf32(e.Text.ToString(), 0) == 36) && this.txtKeyWord.CaretIndex > 0 && this.txtKeyWord.CaretIndex == this.txtKeyWord.Text.Length)
                    || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 36 || char.ConvertToUtf32(e.Text.ToString(), 0) == 37) && (this.txtKeyWord.Text.Contains("%") || this.txtKeyWord.Text.Contains("$")))
                    || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 45) && this.txtKeyWord.Text.Contains("-"))
                    || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 45) && (this.txtKeyWord.Text.Contains(",") || this.txtKeyWord.Text.Contains("$")))
                    || (this.txtKeyWord.CaretIndex == 0 && this.txtKeyWord.Text.Length == 0 && (char.ConvertToUtf32(e.Text.ToString(), 0) == 45 || char.ConvertToUtf32(e.Text.ToString(), 0) == 46 || char.ConvertToUtf32(e.Text.ToString(), 0) == 36 || char.ConvertToUtf32(e.Text.ToString(), 0) == 37))
                    || (this.txtKeyWord.SelectionLength >= 0 && this.txtKeyWord.CaretIndex > 0 && this.txtKeyWord.CaretIndex < this.txtKeyWord.Text.Length && this.txtKeyWord.Text.Length > 0 && (char.ConvertToUtf32(e.Text.ToString(), 0) == 45 || char.ConvertToUtf32(e.Text.ToString(), 0) == 46 || char.ConvertToUtf32(e.Text.ToString(), 0) == 36 || char.ConvertToUtf32(e.Text.ToString(), 0) == 37))
                    || (this.txtKeyWord.CaretIndex == 0 && this.txtKeyWord.Text.Length >= 0 && (char.ConvertToUtf32(e.Text.ToString(), 0) == 46 || char.ConvertToUtf32(e.Text.ToString(), 0) == 37))
                    || (this.txtKeyWord.CaretIndex == this.txtKeyWord.Text.Length && this.txtKeyWord.Text.Contains("%"))
                    || (this.txtKeyWord.CaretIndex == 0 && this.txtKeyWord.SelectionLength == 0 && this.txtKeyWord.Text.Contains("$"))
                    || !this.IsNumericExt(e.Text) || (this.txtKeyWord.Text.Contains(".") && char.ConvertToUtf32(e.Text.ToString(), 0) == 46)
                    || (this.txtKeyWord.Text.Contains("$") && char.ConvertToUtf32(e.Text.ToString(), 0) == 36)
                    || (this.txtKeyWord.Text.Contains("%") && char.ConvertToUtf32(e.Text.ToString(), 0) == 37)
                    )
                {
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< Text Input " + ex.ToString());
            }
        }
        private void KeyWord_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Space || this.IsReadOnly || this.IsTextBlock)
                {
                    e.Handled = true;
                    return;
                }
                if (this.lstContent.SelectedItem == null)
                    this.lstContent.SelectedIndex = 0;
                //Set focus for ListViewItem
                if (base.IsNavigationKey(e.Key))
                {
                    //Open Popup
                    if (!this.popupContent.IsOpen)
                        this.OpenPopup();
                    this.SetFocusListViewItem();
                    base.IsNavigation = true;
                }
                //Close Popup
                else if (this.popupContent.IsOpen && base.IsCancelKey(e.Key))
                    this.ClosePopup(false);
                //When users press "Enter",the mouse will focus next control.
                else if (e.Key == Key.Enter)
                {
                    FocusNavigationDirection focusDirection = new System.Windows.Input.FocusNavigationDirection();
                    focusDirection = System.Windows.Input.FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    //To get the element with keyboard focus.
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    //To change keyboard focus.
                    if (elementWithFocus != null)
                        elementWithFocus.MoveFocus(request);
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< PreviewKeyDown " + ex.ToString());
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //Set focus for TextBox
            this.SetFocus();
        }
        private void txtKeyWord_KeyUp(object sender, KeyEventArgs e)
        {
            //Close Popup
            if (this.IsCancelKey(e.Key))
                this.ClosePopup(false);
        }
        #endregion

        #region The events of Popup
        private void PopupContent_Closed(object sender, EventArgs e)
        {
            if (!base.IsSelectedItem)
                this.SetValueTextBox();
            base.ISOpenPopup = false;
            base.IsSelectedItem = false;
            base.IsNavigation = false;
        }
        #endregion

        #region The events of Button
        private void btnShowPopup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                base.IsSelectedItem = true;
                this.OpenPopup();
                this.SetFocus();
                base.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<< BtnShowPopup_Click >>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #endregion
    }
}
