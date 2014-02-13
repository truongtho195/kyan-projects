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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;
using CPCToolkitExtLibraries;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace CPCToolkitExt.ComboBoxControl
{
    /// <summary>
    /// Interaction logic for ComboBoxQuantity.xaml
    /// </summary>
    public partial class ComboBoxQuantity : ComboBoxBase
    {
        #region Constructor
        public ComboBoxQuantity()
        {
            try
            {
                InitializeComponent();
                //To register event of Control
                this.Loaded += new RoutedEventHandler(ComboBoxQuantity_Loaded);
                //To register event of Button
                this.btnShowPopup.Click += new RoutedEventHandler(btnShowPopup_Click);
                //To register event of TextBox
                this.txtKeyWord.PreviewKeyDown += new KeyEventHandler(KeyWord_PreviewKeyDown);
                this.txtKeyWord.PreviewTextInput += new TextCompositionEventHandler(KeyWord_PreviewTextInput);
                this.txtKeyWord.TextChanged += new TextChangedEventHandler(KeyWord_TextChanged);
                this.txtKeyWord.KeyUp += new KeyEventHandler(txtKeyWord_KeyUp);
                //To register event of Listview
                this.lstContent.KeyUp += new KeyEventHandler(lstContent_KeyUp);
                this.lstContent.SelectionChanged += new SelectionChangedEventHandler(lstContent_SelectionChanged);
                //To register event of Popup
                this.popupContent.Closed += new EventHandler(PopupContent_Closed);
                //Block key of TextBox
                this.txtKeyWord.AllowDrop = false;
                this.txtKeyWord.ContextMenu = null;
                this.txtKeyWord.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
                this.txtKeyWord.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
                this.txtKeyWord.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
                this.txtKeyWord.GotFocus += new RoutedEventHandler(TextBox_GotFocus);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }
        #endregion

        #region Properties

        #region QuantityCollection
        /// <summary>
        /// Collection of Quantity.
        /// </summary>
        private ObservableCollection<QuantityModel> _quantityCollection = new ObservableCollection<QuantityModel>();
        protected ObservableCollection<QuantityModel> QuantityCollection
        {
            get { return _quantityCollection; }
            set
            {
                if (_quantityCollection != value)
                {
                    _quantityCollection = value;
                    RaisePropertyChanged(() => QuantityCollection);
                }
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Open,close the popup
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
            try
            {
                base.IsSelectedItem = true;
                if (isEmpty)
                {
                    this.lstContent.SelectedIndex = -1;
                    this.lstContent.SelectedItem = null;
                    //Set value
                    this.Quantity = 0;
                    this.Unit = 0;
                    this.StandardQuantity = 0;
                    this.StandardUnit = 0;
                    this.UnitPrice = 0;
                    this.SelectedItemQuantity = null;
                    foreach (var item in QuantityCollection)
                    {
                        item.Quantity = 0;
                        item.SetValue();
                    }
                    this.txtKeyWord.Text = string.Empty;
                }
                if (this.popupContent.IsOpen)
                {
                    this.popupContent.StaysOpen = true;
                    this.popupContent.IsOpen = false;
                }
                base.IsSelectedItem = false;
                this.ISOpenPopup = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< ClosePopup >>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region Set value for Quantity property
        private void SetValueDefault()
        {
            try
            {
                this.QuantityCollection.Clear();
                this.lstContent.ItemsSource = this.QuantityCollection;
                foreach (var item in UnitCollection)
                {
                    QuantityModel model = new QuantityModel();
                    model.Quantity = 0;
                    model.Rate = item.Rate;
                    model.Unit = item.Unit;
                    model.TextStandardUnit = item.TextStandardUnit;
                    model.TextUnit = item.TextUnit;
                    model.StandardUnit = item.StandardUnit;
                    model.UnitPrice = item.UnitPrice;
                    model.SetValue();
                    this.QuantityCollection.Add(model);
                }
            }
            catch (Exception ex)
            {
                Debug.Write("Set value Quantity default" + "<<<<<<<<<<<<<<<<" + ex.ToString() + ">>>>>>>>>>>>>>>>>");
            }
        }
        private void SetValue(bool isLoad)
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                          DispatcherPriority.Input,
                          (ThreadStart)delegate
                          {
                              // if (!this._isLoadData)
                              if (this.UnitCollection == null || this.UnitCollection.Count == 0) return;
                              this.SetValueDefault();
                              base.IsSelectedItem = true;
                              if (this.Quantity == null || string.IsNullOrEmpty(this.Quantity.ToString()))
                                  this.Quantity = "0";
                              if (isLoad)
                              {
                                  decimal qt = Decimal.Parse(this.Quantity.ToString());
                                  if (qt > 0)
                                      foreach (var item in QuantityCollection)
                                      {
                                          item.Quantity = qt;
                                          if (this.Unit == item.Unit)
                                              item.Unit = this.Unit;
                                          item.SetValue();
                                      }
                                  //Set value for TextBox
                                  var model = this.QuantityCollection.FirstOrDefault(x => x.Quantity == qt && x.Unit == this.Unit);
                                  //Set SelectedItemQuantity
                                  //if (this.SelectedItemQuantity != null)
                                  //    model = this.SelectedItemQuantity as QuantityModel;
                                  if (model != null)
                                  {
                                      this.txtKeyWord.Text = model.Content.Trim();
                                      ///Set SelectedItem for ListView
                                      this.lstContent.SelectedItem = model;
                                      if (this.Rate != model.Rate)
                                          this.Rate = model.Rate;
                                      if (this.StandardQuantity != model.StandardQuantity)
                                          this.StandardQuantity = model.StandardQuantity;
                                      if (this.UnitPrice != model.UnitPrice)
                                          this.UnitPrice = model.UnitPrice;
                                  }
                                  else
                                      this.txtKeyWord.Text = string.Empty;
                                  //*****************Clone
                                  //else
                                  //    this.SelectedItemClone = null;
                              }
                              base.IsSelectedItem = false;
                          });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<< Set value Quantity >>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region ChangedValue
        private void ChangedValue()
        {
            try
            {
                base.IsSelectedItem = true;
                string number = this.txtKeyWord.Text;
                for (int i = 0; i < this.txtKeyWord.Text.Length; i++)
                {
                    if (this.IsCharacter(this.txtKeyWord.Text[i].ToString()))
                    {
                        number = this.txtKeyWord.Text.Remove(i, this.txtKeyWord.Text.Length - i);
                        break;
                    }
                }
                if (string.IsNullOrEmpty(number))
                    number = "0";
                foreach (var item in QuantityCollection)
                {
                    item.Quantity = Decimal.Parse(number, CultureInfo.InvariantCulture);
                    item.SetValue();
                }
                //SelectedIitem ComboBox
                this.lstContent.SelectedItem = this.QuantityCollection.Where(x => x.Content.Contains(this.txtKeyWord.Text)).FirstOrDefault();
                base.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< Changed Value >>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void ClearValue()
        {
            this.Quantity = 0;
            this.Unit = 0;
            this.StandardQuantity = 0;
            this.StandardUnit = 0;
            this.UnitPrice = 0;
            this.Rate = 0;
            this.SelectedItemQuantity = null;
        }
        #endregion

        #region Check value
        private bool IsInputUnit(string text)
        {
            int index = this.txtKeyWord.CaretIndex - 1;
            if (this.txtKeyWord.CaretIndex == 0 && this.txtKeyWord.Text.Length > 0)
                return false;
            return (base.IsNumeric(text) && base.IsCharacter(this.txtKeyWord.Text[index].ToString()));
        }
        private bool IsTextUnit(string text)
        {
            if (IsCharacter(text))
                foreach (var item in this.QuantityCollection)
                    if (item.TextUnit.Contains(text))
                        return true;

            return false;
        }
        private bool IsSample()
        {
            if (base.SelectedItemClone == null)
                return false;
            //return true;
            if ((base.SelectedItemClone as QuantityModel) == (this.lstContent.SelectedItem as QuantityModel))
                return true;

            return false;
        }
        #endregion

        #region SetFocusTextBox
        private void SetValueTextBox()
        {
            try
            {
                //if (this.IsSample())
                //    return;
                base.IsSelectedItem = true;
                if (this.lstContent.SelectedItem != null)
                {
                    this.SetFocusTextBox();
                    this.SetFocus();
                    this.Quantity = this.QuantityCollection[this.lstContent.SelectedIndex].Quantity;
                    this.Unit = this.QuantityCollection[this.lstContent.SelectedIndex].Unit;
                    this.StandardQuantity = this.QuantityCollection[this.lstContent.SelectedIndex].StandardQuantity;
                    this.StandardUnit = this.QuantityCollection[this.lstContent.SelectedIndex].StandardUnit;
                    this.Rate = this.QuantityCollection[this.lstContent.SelectedIndex].Rate;
                    this.UnitPrice = this.QuantityCollection[this.lstContent.SelectedIndex].UnitPrice;
                    this.SelectedItemQuantity = this.QuantityCollection[this.lstContent.SelectedIndex];
                    //*****************Clone
                    // this.SelectedItemClone = ComboBoxQuantity.DeepClone(this.lstContent.SelectedItem);
                }
                else
                {
                    this.Unit = 0;
                    this.Quantity = 0;
                    this.StandardQuantity = 0;
                    this.StandardUnit = 0;
                    this.Rate = 0;
                    this.UnitPrice = 0;
                    this.SelectedItemQuantity = null;
                    //*****************Clone
                    ////this.SelectedItemClone = null;
                }
                base.IsSelectedItem = false;
            }
            catch (Exception ex)

            { Debug.Write("<<<<<<<<<<<< SetValueTextBox >>>>>>>>>>>>>>>>>>" + ex.ToString()); }
        }
        private void SetFocusTextBox()
        {
            try
            {
                base.IsSelectedItem = true;
                if (this.lstContent.SelectedIndex >= 0)
                {
                    if (this.txtKeyWord.Text != this.QuantityCollection[this.lstContent.SelectedIndex].Content)
                        this.txtKeyWord.Text = this.QuantityCollection[this.lstContent.SelectedIndex].Content;
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

        #region ChangeStyle

        /// <summary>
        /// Change style when IsReadOnly=false
        /// </summary>
        protected override void PreviousStyle()
        {
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                   DispatcherPriority.Input,
                                   (ThreadStart)delegate
                                   {
                                       this.txtKeyWord.BorderThickness = new Thickness(1, 1, 1.2, 1);
                                       this.btnShowPopup.Visibility = Visibility.Visible;
                                       this.txtKeyWord.IsReadOnly = false;
                                       this.recIsTextBlock.Visibility = Visibility.Collapsed;
                                       base.PreviousStyle();
                                   });
            else
            {
                this.txtKeyWord.BorderThickness = new Thickness(1, 1, 1.2, 1);
                this.btnShowPopup.Visibility = Visibility.Visible;
                this.txtKeyWord.IsReadOnly = false;
                this.recIsTextBlock.Visibility = Visibility.Collapsed;
                base.PreviousStyle();
            }

        }

        /// <summary>
        /// Change style when IsReadOnly=true
        /// </summary>
        protected override void ChangeStyle()
        {
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                   DispatcherPriority.Input,
                                   (ThreadStart)delegate
                                   {
                                       this.txtKeyWord.BorderThickness = new Thickness(0);
                                       this.btnShowPopup.Visibility = Visibility.Collapsed;
                                       this.txtKeyWord.IsReadOnly = true;
                                       this.recIsTextBlock.Visibility = Visibility.Visible;
                                       base.ChangeStyle();
                                   });
            else
            {
                this.txtKeyWord.BorderThickness = new Thickness(0);
                this.btnShowPopup.Visibility = Visibility.Collapsed;
                this.txtKeyWord.IsReadOnly = true;
                this.recIsTextBlock.Visibility = Visibility.Visible;
                base.ChangeStyle();
            }
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
                                         ListViewItem listViewItem = (ListViewItem)lstContent.ItemContainerGenerator.ContainerFromIndex(this.lstContent.SelectedIndex);
                                         if (listViewItem != null)
                                         {
                                             listViewItem.Focus();
                                         }
                                     }
                                 });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<SetFocusListViewItem>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        ///// <summary>
        ///// Set focus for TextBox
        ///// </summary>
        //private void SetFocusTextBox()
        //{
        //    try
        //    {
        //        this.txtKeyWord.Dispatcher.BeginInvoke(
        //              DispatcherPriority.Input,
        //              (ThreadStart)delegate
        //              {
        //                  this.txtKeyWord.Focus();
        //                  if (!string.IsNullOrEmpty(this.txtKeyWord.Text) && this.txtKeyWord.Text.Length > 0)
        //                      this.txtKeyWord.SelectAll();
        //              });
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write("<<<<<<<<<<<<<<<<<<SetFocusListViewItem>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
        //    }
        //}
        #endregion

        #endregion

        #region DependencyProperties

        #region UnitCollection
        /// <summary>
        /// Get ,set collection of unit
        /// </summary>
        public UnitCollection UnitCollection
        {
            get { return (UnitCollection)GetValue(UnitCollectionProperty); }
            set { SetValue(UnitCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitCollectionProperty =
            DependencyProperty.Register("UnitCollection", typeof(UnitCollection), typeof(ComboBoxQuantity), new PropertyMetadata(null, ChangeValueUnitCollection));
        protected static void ChangeValueUnitCollection(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as ComboBoxQuantity).IsSelectedItem && e.NewValue != null && e.NewValue != e.OldValue)
                (source as ComboBoxQuantity).SetValue(true);
        }
        #endregion

        #region Quantity
        /// <summary>
        /// Get,set quantity
        /// </summary>
        public object Quantity
        {
            get { return (object)GetValue(QuantityProperty); }
            set { SetValue(QuantityProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuantityProperty =
            DependencyProperty.Register("Quantity", typeof(object), typeof(ComboBoxQuantity),
        new PropertyMetadata(null, ChangeValue));
        protected static void ChangeValue(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as ComboBoxQuantity).IsSelectedItem
                    && e.NewValue != null
                    && !String.Format(CultureInfo.InvariantCulture, "{0}", e.NewValue).Equals(String.Format(CultureInfo.InvariantCulture, "{0}", e.OldValue)))
                (source as ComboBoxQuantity).SetValue(true);
        }
        #endregion

        #region Unit
        /// <summary>
        /// Get,set quantity
        /// </summary>
        public int Unit
        {
            get { return (int)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Unit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(int), typeof(ComboBoxQuantity),
        new PropertyMetadata(-1, ChangeValueUnit));

        protected static void ChangeValueUnit(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as ComboBoxQuantity).IsSelectedItem && e.NewValue != null
            && !String.Format(CultureInfo.InvariantCulture, "{0}", e.NewValue).Equals(String.Format(CultureInfo.InvariantCulture, "{0}", e.OldValue)))
            {
                (source as ComboBoxQuantity).SetValue(true);
            }
        }
        #endregion

        #region StandardQuantity
        /// <summary>
        /// Get,set StandardQuantity
        /// </summary>
        public decimal StandardQuantity
        {
            get { return (decimal)GetValue(StandardQuantityProperty); }
            set { SetValue(StandardQuantityProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StandardQuantityProperty =
            DependencyProperty.Register("StandardQuantity", typeof(decimal), typeof(ComboBoxQuantity), new UIPropertyMetadata(0M));
        #endregion

        #region StandardUnit
        /// <summary>
        /// Get,set StandardUnit
        /// </summary>
        public int StandardUnit
        {
            get { return (int)GetValue(StandardUnitProperty); }
            set { SetValue(StandardUnitProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Unit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StandardUnitProperty =
            DependencyProperty.Register("StandardUnit", typeof(int), typeof(ComboBoxQuantity), new UIPropertyMetadata(0));
        #endregion

        #region Rate
        /// <summary>
        /// Get,set Rate
        /// </summary>
        public decimal Rate
        {
            get { return (decimal)GetValue(RateProperty); }
            set { SetValue(RateProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Rate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(decimal), typeof(ComboBoxQuantity), new UIPropertyMetadata(0M));


        #endregion

        #region UnitPrice
        /// <summary>
        /// Get,set UnitPrice
        /// </summary>
        public decimal UnitPrice
        {
            get { return (decimal)GetValue(UnitPriceProperty); }
            set { SetValue(UnitPriceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitPrice.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitPriceProperty =
            DependencyProperty.Register("UnitPrice", typeof(decimal), typeof(ComboBoxQuantity), new UIPropertyMetadata(0M));


        #endregion

        #region SelectedItemQuantity

        public object SelectedItemQuantity
        {
            get { return (object)GetValue(SelectedItemQuantityProperty); }
            set { SetValue(SelectedItemQuantityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItemQuantity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemQuantityProperty =
            DependencyProperty.Register("SelectedItemQuantity", typeof(object), typeof(ComboBoxQuantity),
            new PropertyMetadata(null, ChangeSelectedItem));
        protected static void ChangeSelectedItem(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as ComboBoxQuantity).IsSelectedItem
                    && e.NewValue != null
                    && !Object.ReferenceEquals(e.NewValue, e.OldValue))
                (source as ComboBoxQuantity).SetValue(true);
        }



        #endregion

        #endregion

        #region The Event of control

        #region The events of Control
        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxQuantity_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.InputMaxLength > 0)
                    this.txtKeyWord.MaxLength = this.InputMaxLength;
                else
                    this.txtKeyWord.MaxLength = 9;
                if (this.BackgroundBase == null)
                    base.BackgroundBase = this.Background;
                if (base.BorderBase == null || base.BorderBase == new Thickness(0))
                    base.BorderBase = this.BorderThickness;
                base.IsSelectedItem = false;
                base.ISOpenPopup = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<< Load" + ex.ToString());
            }
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

        /// <summary>
        /// Close Popup when Click item in ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstContent_KeyUp(object sender, KeyEventArgs e)
        {
            //Close Popup
            if (this.IsCancelKey(e.Key))
                this.ClosePopup(false);
        }

        /// <summary>
        /// Set value for TextBox when SelectedItem changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (base.IsNavigation)
                this.SetFocusTextBox();
        }
        #endregion

        #region The events of TextBox

        ///// <summary>
        ///// TextChanged
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
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
                            // Setvalue
                            if (!string.IsNullOrEmpty(this.txtKeyWord.Text)
                                && this.txtKeyWord.Text.Length > 0)
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
                Debug.Write("<<<<<<<<<<<<<<<<<< KeyWord_TextChanged" + ex.ToString());
            }
        }

        ///// <summary>
        ///// PreviewTextInput
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        private void KeyWord_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //Set input Text
                if (this.txtKeyWord.IsReadOnly || e.Text.Length == 0 || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 46)
                    && this.txtKeyWord.Text.Contains(".")) || ((char.ConvertToUtf32(e.Text.ToString(), 0) == 46)
                    && (this.txtKeyWord.Text.Length == 0 || this.txtKeyWord.CaretIndex == 0)) || (this.txtKeyWord.Text.Length > 0
                    && this.IsInputUnit(e.Text)) || ((this.txtKeyWord.CaretIndex == 0 || (this.txtKeyWord.CaretIndex > 0 && this.txtKeyWord.CaretIndex < this.txtKeyWord.Text.Length) || (this.txtKeyWord.SelectionLength > 0 && this.txtKeyWord.SelectionLength <= this.txtKeyWord.Text.Length)) && !this.IsNumeric(e.Text)) || (!this.IsNumeric(e.Text)
                    && !this.IsTextUnit(e.Text)))
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
                ////Change SelectedIndex for ListView
                //if (this.lstContent.SelectedItem == null)
                //    this.lstContent.SelectedIndex = 0;
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
                else if (this.popupContent.IsOpen && this.IsCancelKey(e.Key))
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

        /// <summary>
        /// Focus TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #region The event of Button
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
                Debug.Write("<<<<<<<<<<<<<<<<<<<btnShowPopup_Click>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #endregion
    }
}
