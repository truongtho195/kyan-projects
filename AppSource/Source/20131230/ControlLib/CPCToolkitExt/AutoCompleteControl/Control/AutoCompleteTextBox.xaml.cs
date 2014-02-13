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
    /// <summary>
    /// Interaction logic for ControlAutoComplete.xaml
    /// </summary>
    public partial class AutoCompleteTextBox : AutoCompleteBase
    {
        #region Ctor
        public AutoCompleteTextBox()
        {
            InitializeComponent();
            // Summary:
            //     Sets the System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled
            //     property for the specified object.
            ScrollViewer.SetIsDeferredScrollingEnabled(this.lstComplete, true);
            //
            // Summary:
            //     Sets the value of the System.Windows.Controls.VirtualizingStackPanel.IsVirtualizingProperty attached
            //     property.
            //
            // Parameters:
            //   element:
            //     The object to which the attached property value is set.
            //
            //   value:
            //     true if the System.Windows.Controls.VirtualizingStackPanel is virtualizing;
            //     otherwise false.
            VirtualizingStackPanel.SetIsVirtualizing(this.lstComplete, true);
            //To Register event for TextBox
            this.txtKeyWord.TextChanged += new TextChangedEventHandler(TxtKeyWord_TextChanged);
            this.txtKeyWord.PreviewKeyDown += new KeyEventHandler(txtKeyWord_PreviewKeyDown);
            this.txtKeyWord.GotFocus += new RoutedEventHandler(txtKeyWord_GotFocus);
            //To Register event for ListView
            this.lstComplete.PreviewKeyDown += new KeyEventHandler(LstComplete_PreviewKeyDown);
            this.lstComplete.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lstComplete_MouseLeftButtonDown);
            //To Register event for Popup
            this.popupResult.Closed += new EventHandler(PopupResult_Closed);
            //To Register event for Conrol
            this.Loaded += new RoutedEventHandler(AutoCompleteTextBox_Loaded);
            this.LostFocus += new RoutedEventHandler(AutoCompleteTextBox_LostFocus);
            this.GotFocus += new RoutedEventHandler(AutoCompleteTextBox_GotFocus);
        }
        #endregion

        #region Field
        #endregion

        #region The events of of control

        #region The events of of TextBox
        private void txtKeyWord_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                base.IsFocusControl = true;
                this.popupResult.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      ///To set SelectAll for TextBox 
                                      if (this.txtKeyWord.Text.Length > 0)
                                          this.txtKeyWord.SelectAll();
                                      //Hide Watermark TextBox.
                                      base.IsKeyAcceptFromList = false;
                                      base.IsFocusControl = true;
                                      if (this.IsNotBorder)
                                          this.txtKeyWord.BorderThickness = new Thickness(0);
                                  });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<txtKeyWord_GotFocus>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void TxtKeyWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (base.IsSelectedItem) return;
                base.IsLoad = true;
                //Set value for SelectedItem
                //Set value for SelectedItem
                this.lstComplete.SelectedItem = null;
                if (string.IsNullOrEmpty(this.txtKeyWord.Text)
                || this.txtKeyWord.Text.Trim().Length == 0)
                {
                    //Close Popup
                    if (this.popupResult.IsOpen)
                        this.ClosePopup();
                    //Set value default for SelectedItemResult,SelectedValue
                    base.SelectedItemClone = null;
                    base.SelectedItemResult = null;
                    base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                    ///Reset ItemSource when this.txtKeyWord.Text=string.Empty.
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        this.ReturnValueDefault();
                    });
                    base.IsLoad = false;
                    return;
                }
                else
                {
                    this.txtKeyWord.Dispatcher.BeginInvoke(DispatcherPriority.Input,
                        (ThreadStart)delegate
                        {
                            if (string.IsNullOrEmpty(this.txtKeyWord.Text)
                               || this.txtKeyWord.Text.Trim().Length == 0)
                                return;
                            //Filter data
                            this.Filter(false);
                            //Open Popup
                            this.OpenPopup(false);
                            Debug.WriteLine("Execute Filter");
                            ////Close popup when control was losted focus.
                            if (base.IsClickItem)
                            {
                                Keyboard.Focus(this);
                                this.ClosePopup();
                            }
                        });
                }
                ///Set value for Text
                base.Text = this.txtKeyWord.Text.Trim();
                base.IsKeyAcceptFromList = false;
                base.IsLoad = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<Text Changed>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }
        }
        private void txtKeyWord_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                Debug.WriteLine("Execute PreviewKeyDown " + e.Key);
                ///Set EventHanlder when IsReadOnly
                if (base.IsReadOnly || base.IsTextBlock)
                {
                    e.Handled = true;
                    return;
                }
                if (base.IsNavigationKey(e.Key)
                    && this.dbNoResult.Visibility == Visibility.Collapsed)
                {
                    if (!this.popupResult.IsOpen)
                        this.OpenPopup(true);
                    base.ISOpenPopup = true;
                    ///Set value default
                    if (e.Key == Key.Down)
                    {
                        if (!this.lstComplete.IsFocused)
                            FocusManager.SetIsFocusScope(this.lstComplete, true);
                        this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     if (this.lstComplete.SelectedItem == null)
                                     {
                                         this.lstComplete.SelectedIndex = 0;
                                         ListViewItem item = (ListViewItem)lstComplete.ItemContainerGenerator.ContainerFromIndex(0);
                                         if (item != null)
                                             item.Focus();
                                     }
                                     else
                                     {
                                         ListViewItem item = (ListViewItem)lstComplete.ItemContainerGenerator.ContainerFromItem(this.lstComplete.SelectedItem);
                                         if (item != null)
                                             item.Focus();
                                     }
                                     //ScrollIntoView
                                     this.lstComplete.ScrollIntoView(this.lstComplete.SelectedItem);
                                 });
                    }
                    base.IsKeyAcceptFromList = false;
                }
                else if (this.popupResult.IsOpen
                    && base.IsCancelKey(e.Key))
                {
                    this.ClosePopup();
                    base.IsKeyAcceptFromList = false;
                }
                base.IsCancelKey(e.Key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<txtKeyWord_PreviewKeyDown>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion

        #region The events of of ComboBox
        private void CbFieldSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            base.IsKeyAcceptFromList = false;
        }
        #endregion

        #region The events of ListView
        private void LstComplete_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (base.IsChooseCurrentItemKey(e.Key))
                {
                    base.IsKeyAcceptFromList = true;
                    FocusManager.SetIsFocusScope(this.txtKeyWord, true);
                    this.ClosePopup();
                }
                else if (base.IsCancelKey(e.Key))
                {
                    base.IsKeyAcceptFromList = false;
                    this.ClosePopup();
                    txtKeyWord.Focus();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<LstComplete_KeyUp>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void lstComplete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement((ListView)sender, e.OriginalSource as DependencyObject) is ListViewItem)
            {
                base.IsClickItem = true;
                this.ClosePopup();
            }
        }

        #endregion

        #region The event of Popup
        private void PopupResult_Closed(object sender, EventArgs e)
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                           DispatcherPriority.Input,
                           (ThreadStart)delegate
                           {
                               base.IsFocusControl = true;
                               base.ISOpenPopup = false;
                               base.IsLoad = true;
                               base.IsSelectedItem = true;
                               this.txtKeyWord.Text = string.Empty;
                               base.IsSelectedItem = false;
                               if (this.lstComplete.SelectedItem != null && base.IsClickItem)
                               {
                                   ///Set value for SelectedItemResult,SelectedValue
                                   if (base.AutoHiddenSelectedItem)
                                   {
                                       this.lstComplete.SelectedItem.GetType().GetProperty("IsSelected").SetValue(this.lstComplete.SelectedItem, true, null);
                                       base.SelectedItemClone = this.lstComplete.SelectedItem;
                                   }
                                   base.IsSelectedItem = true;
                                   base.SelectedItemResult = this.lstComplete.SelectedItem;
                                   if (this.lstComplete.SelectedValue != null)
                                       base.SelectedValue = this.lstComplete.SelectedValue;
                                   if (this.IsTextCompletionEnabled && base.IsClickItem)
                                   {
                                       base.IsSelectedItem = true;
                                       object content = this.GetDataFieldShow(this.SelectedItemResult, FieldShow);
                                       if (content != null)
                                       {
                                           this.txtKeyWord.Text = content.ToString();
                                           this.txtKeyWord.SelectAll();
                                       }
                                       base.IsSelectedItem = false;
                                   }
                                   base.IsSelectedItem = false;
                               }
                               if (!base.IsClickItem || !base.IsClearText)
                               {
                                   base.IsSelectedItem = true;
                                   this.lstComplete.SelectedItem = null;
                                   this.txtKeyWord.Text = string.Empty;
                                   //base.SelectedItemResult = null;
                                   //base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                                   base.IsSelectedItem = false;
                                   this.dbNoResult.Visibility = Visibility.Collapsed;
                               }
                               ///Set value for Text
                               base.IsPressCancelKey = false;
                               base.IsClickItem = false;
                               base.Text = this.txtKeyWord.Text;
                               base.IsLoad = false;
                               Keyboard.Focus(this.txtKeyWord);
                               this.txtKeyWord.Focus();
                               //To clear filter..
                               this.ClearFilter();
                           });

            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<PopupResult_Closed>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region The events of the control
        private void AutoCompleteTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ///Set PlacementTarget for Popup
                base.IsClickItem = false;
                this.popupResult.PlacementTarget = this;
                this.popupResult.IsOpen = false;
                this.popupResult.StaysOpen = true;
                ///Set MaxDropDownHeight for Control
                if (this.MaxDropDownHeight == double.PositiveInfinity
                    || this.MaxDropDownHeight == 0)
                    this.MaxDropDownHeight = 256;
                //Visibility grid 
                this.dbNoResult.Visibility = Visibility.Collapsed;
                //get BackgroundBase ,BorderBase for control
                if (this.BackgroundBase == null)
                    base.BackgroundBase = this.Background;
                if (this.BorderBase == null
                    || this.BorderBase == new Thickness(0))
                    base.BorderBase = this.BorderThickness;
                base.ISOpenPopup = false;
                ///Set width for Popup
                if (base.Columns != null)
                {
                    double width = this.Columns.Sum(x => x.ActualWidth);
                    if (width > 0)
                        //this.Shdw.MaxWidth = this.lstComplete.Width = width + 30;
                        this.lstComplete.MaxWidth = width + 20;
                    else if (this.Width > 0)
                        //this.Shdw.MaxWidth = this.lstComplete.Width = this.Width + 30;
                        this.lstComplete.MaxWidth = this.Width + 20;
                }
                ///Set SelectedValuePath for ListView
                if (!string.IsNullOrEmpty(base.SelectedValuePath))
                    this.lstComplete.SelectedValuePath = base.SelectedValuePath;
                if (this.IsFocused)
                    this.txtKeyWord.Focus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<AutoCompleteTextBox_Loaded>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        public void ControlAutoComplete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Close popup
            base.IsClickItem = true;
            this.ClosePopup();
        }

        private void AutoCompleteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            base.IsFocusControl = false;
        }

        private void AutoCompleteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            base.IsFocusControl = true;
            if (!this.IsClickItem && !this.txtKeyWord.IsFocused)
                this.txtKeyWord.Focus();
        }
        #endregion

        #endregion Events

        #region DependencyProperties

        #region SearchCommand
        //
        // Summary:
        //     Gets or sets the command to invoke when this button is pressed. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when this button is pressed. The default value is null.
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(AutoCompleteTextBox));

        [Category("Common Properties")]
        public ICommand SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }
        #endregion

        #region SearchCommandParamater
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty SearchCommandParamaterProperty =
            DependencyProperty.Register("SearchCommandParamater", typeof(object), typeof(AutoCompleteTextBox));

        [Category("Common Properties")]
        public object SearchCommandParamater
        {
            get { return (object)GetValue(SearchCommandParamaterProperty); }
            set { SetValue(SearchCommandParamaterProperty, value); }
        }
        #endregion

        #region ListViewItemStyle
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static DependencyProperty ListViewItemStyleProperty =
            DependencyProperty.Register("ListViewItemStyle", typeof(Style), typeof(AutoCompleteTextBox), new PropertyMetadata(null, OnStyleChanged));

        private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (d as AutoCompleteTextBox).ChangeListViewItemStyle(e.NewValue);
        }
        public Style ListViewItemStyle
        {
            get { return (Style)GetValue(ListViewItemStyleProperty); }
            set { SetValue(ListViewItemStyleProperty, value); }
        }
        #endregion

        #region WaterMark
        public FrameworkElement WaterMark
        {
            get { return (FrameworkElement)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
        // Using a DependencyProperty as the backing store for WaterMark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaterMarkProperty =
            DependencyProperty.Register("WaterMark", typeof(FrameworkElement), typeof(AutoCompleteTextBox), new UIPropertyMetadata(null));
        #endregion

        #endregion

        #region Methods

        #region Filter
        /// <summary>
        /// Filter item in ItemSource 
        /// Return data for ListView result
        /// </summary>
        private void ClearFilter()
        {
            this.lstComplete.Items.Filter = (item) =>
                    {
                        return true;
                    };
            if (base.SelectedItemResult != null)
            {
                base.IsSelectedItem = true;
                this.lstComplete.SelectedItem = base.SelectedItemResult;
                base.IsSelectedItem = false;
            }
        }
        private void Filter(bool isSelected)
        {
            try
            {
                //Default value for search
                if (this.FieldSource != null && this.FieldSource.Count > 0)
                {
                    this.lstComplete.Items.Filter = (item) =>
                    {
                        if (base.AutoHiddenSelectedItem
                            && (!Object.ReferenceEquals(base.SelectedItemClone, item))
                               && bool.Parse(item.GetType().GetProperty("IsSelected").GetValue(item, null).ToString()))
                            return false;

                        else if (isSelected)
                            return true;

                        foreach (var field in this.FieldSource)
                        {
                            if (base.GetDataHasChildren(item, field, this.txtKeyWord.Text.Trim()))
                                return true;
                        }
                        return false;
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<Filter()>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }
        }

        #endregion

        #region ReturnValueDefault
        /// <summary>
        /// Return data for ListView 
        /// </summary>
        private void ReturnValueDefault()
        {
            try
            {
                //Default value for search
                if (base.FieldSource != null && this.FieldSource.Count > 0)
                {
                    this.lstComplete.Items.Filter = (item) =>
                    {
                        if (base.AutoHiddenSelectedItem
                            && (!Object.ReferenceEquals(base.SelectedItemClone, item))
                                  && bool.Parse(item.GetType().GetProperty("IsSelected").GetValue(item, null).ToString()))
                            return false;
                        return true;
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<ReturnValueDefault()>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }
        }

        #endregion

        #region OpenPopup
        /// <summary>
        /// Show Result
        /// </summary>
        private void OpenPopup(bool isKey)
        {
            try
            {
                //To open Empty popup.
                if ((this.lstComplete.Items.IsEmpty
               || this.ItemsSource == null) && !string.IsNullOrEmpty(this.txtKeyWord.Text))
                {
                    this.dbShowResult.Visibility = Visibility.Collapsed;
                    this.dbNoResult.Visibility = Visibility.Visible;
                    this.dbNoResult.Width = this.AutoControl.Width;
                    this.popupResult.StaysOpen = false;
                    this.popupResult.IsOpen = true;
                }
                //To open Result popup.
                else
                {
                    this.dbShowResult.Visibility = Visibility.Visible;
                    this.dbNoResult.Visibility = Visibility.Collapsed;
                    this.popupResult.StaysOpen = false;
                    this.popupResult.IsOpen = true;
                    if (!isKey)
                        this.lstComplete.SelectedIndex = 0;

                }
                base.ISOpenPopup = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<OpenPopup>>>>>>>>>>>>>>" + ex.ToString() + ">>>>>>>>>>>>>>>>>>>>" + "\n");
            }
        }

        #endregion

        #region ClosePopup
        /// <summary>
        /// Close Result
        /// </summary>
        private void ClosePopup()
        {
            try
            {
                this.popupResult.StaysOpen = true;
                this.popupResult.IsOpen = false;
                this.dbNoResult.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<CloseHideDataSuggestion>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion

        #region SetValueWithSelectedItemResult
        /// <summary>
        /// Gets the text contents of the text box when selected item in ListView.
        /// </summary>
        protected override void SetValueWithSelectedItemResult()
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                             DispatcherPriority.Input,
                             (ThreadStart)delegate
                             {
                                 base.IsSelectedItem = true;
                                 if (base.SelectedItemResult != null && (base.ItemsSource != null && base.ItemsSource.Cast<object>().ToList().Count > 0))
                                 {
                                     if (base.IsTextCompletionEnabled && base.SelectedItemResult != null)
                                     {
                                         this.ReturnValueDefault();
                                         this.lstComplete.SelectedItem = base.SelectedItemResult;
                                         object content = this.GetDataFieldShow(base.SelectedItemResult, base.FieldShow);
                                         if (content != null)
                                             this.txtKeyWord.Text = content.ToString();
                                         if (base.AutoHiddenSelectedItem && base.SelectedItemResult != null)
                                         {
                                             ///Set ItemSource when AutoHiddenSelectedItem=true
                                             base.SelectedItemResult.GetType().GetProperty("IsSelected").SetValue(base.SelectedItemResult, true, null);
                                             this.lstComplete.SelectedItem.GetType().GetProperty("IsSelected").SetValue(base.SelectedItemResult, true, null);
                                             base.SelectedItemClone = this.lstComplete.SelectedItem;
                                         }
                                     }
                                     else
                                         this.txtKeyWord.Text = string.Empty;
                                 }
                                 else
                                     this.txtKeyWord.Text = string.Empty;
                                 base.IsSelectedItem = false;
                             });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<SetValueDefault>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.SetValueWithSelectedItemResult();
        }

        #endregion

        #region SetValueWithSelectedValue
        /// <summary>
        /// Gets the text contents of the text box when selected value in ListView.
        /// </summary>
        protected override void SetValueWithSelectedValue()
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                             DispatcherPriority.Input,
                             (ThreadStart)delegate
                             {
                                 if (base.SelectedValuePath != null
                                     && !string.IsNullOrEmpty(base.SelectedValuePath)
                                     && base.SelectedValuePath.Length > 0)
                                 {
                                     this.ReturnValueDefault();
                                     this.lstComplete.SelectedValuePath = base.SelectedValuePath;
                                     this.lstComplete.SelectedValue = base.SelectedValue;
                                     ///Set SelectedItemResult
                                     if (base.AutoChangeSelectedItem)
                                     {
                                         base.IsLoad = true;
                                         base.SelectedItemResult = this.lstComplete.SelectedItem;
                                         base.IsLoad = false;
                                     }
                                     if (base.IsTextCompletionEnabled && this.lstComplete.SelectedValue != null)
                                     {
                                         base.IsSelectedItem = true;
                                         object content = this.GetDataFieldShow(this.lstComplete.SelectedItem, base.FieldShow);
                                         if (content != null)
                                             this.txtKeyWord.Text = content.ToString();
                                         base.IsSelectedItem = false;
                                         if (base.AutoHiddenSelectedItem && base.SelectedValue != null)
                                         {
                                             ///Set ItemSource when AutoHiddenSelectedItem=true
                                             this.lstComplete.SelectedItem.GetType().GetProperty("IsSelected").SetValue(this.lstComplete.SelectedItem, true, null);
                                             base.SelectedItemClone = this.lstComplete.SelectedItem;
                                         }
                                     }
                                     else
                                     {
                                         base.IsSelectedItem = true;
                                         this.txtKeyWord.Text = string.Empty;
                                         base.IsSelectedItem = false;
                                     }
                                 }
                             });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<SetValueforSelectedValue>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.SetValueWithSelectedValue();
        }

        #endregion

        #region ClearValue
        /// <summary>
        /// Clear value
        /// </summary>
        protected override void ClearValue()
        {
            try
            {
                base.IsSelectedItem = true;
                this.txtKeyWord.Text = string.Empty;
                base.SelectedItemResult = null;
                base.SelectedItemClone = null;
                base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                base.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<ClearValue>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }

            base.ClearValue();
        }

        #endregion

        #region ChangeStyle
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
                                         this.popupResult.Visibility = Visibility.Collapsed;
                                         this.txtKeyWord.IsReadOnly = true;
                                         this.txtKeyWord.Background = Brushes.Transparent;
                                         this.recIsTextBlock.Visibility = Visibility.Visible;
                                     });
            else
            {
                this.popupResult.Visibility = Visibility.Collapsed;
                this.txtKeyWord.IsReadOnly = true;
                this.txtKeyWord.Background = Brushes.Transparent;
                this.recIsTextBlock.Visibility = Visibility.Visible;
            }
            base.ChangeStyle();
        }

        #endregion

        #region PreviousStyle
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
                                         this.popupResult.Visibility = Visibility.Collapsed;
                                         this.txtKeyWord.IsReadOnly = false;
                                         this.recIsTextBlock.Visibility = Visibility.Collapsed;
                                         this.txtKeyWord.Background = base.BackgroundBase;
                                     });
            else
            {
                this.popupResult.Visibility = Visibility.Collapsed;
                this.txtKeyWord.IsReadOnly = false;
                this.recIsTextBlock.Visibility = Visibility.Collapsed;
                this.txtKeyWord.Background = base.BackgroundBase;
            }
            base.PreviousStyle();
        }

        #endregion

        #region ChangeReadOnly
        /// <summary>
        /// ReadOnly of control
        /// </summary>
        /// <param name="isReadOnly"></param>
        protected override void ChangeReadOnly(bool isReadOnly)
        {
            this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     if (isReadOnly)
                                     {
                                         this.txtKeyWord.IsReadOnly = true;
                                         this.popupResult.Visibility = Visibility.Collapsed;
                                     }
                                     else
                                     {
                                         this.txtKeyWord.IsReadOnly = false;
                                         this.popupResult.Visibility = Visibility.Visible;
                                     }
                                 });
            base.ChangeReadOnly(isReadOnly);
        }

        #endregion

        #region SetMaxDropDownHeight
        /// <summary>
        /// Set MaxHeight for Popup
        /// </summary>
        /// <param name="value"></param>
        protected override void SetMaxDropDownHeight(double value)
        {
            this.popupResult.MaxHeight = value;
            base.SetMaxDropDownHeight(value);
        }
        #endregion

        #region SetFocus
        /// <summary>
        /// Set focus textBox
        /// </summary>
        public override void SetFocus()
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     Keyboard.Focus(this.txtKeyWord);
                                 });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<SetFocus>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.SetFocus();
        }

        #endregion

        #region SetBorder
        protected override void SetBorder(bool value)
        {
            if (value)
            {
                this.popupResult.Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            (ThreadStart)delegate
                            {
                                this.txtKeyWord.IsInsideControl = value;
                            });
            }
            base.SetBorder(value);
        }
        #endregion

        #region SetStyle
        protected override void SetStyle(Style value)
        {
            base.SetStyle(value);
            this.txtKeyWord.Style = value;
        }
        #endregion

        #region ChangeListViewItemStyle
        public void ChangeListViewItemStyle(object style)
        {
            Style oldStyle = this.lstComplete.ItemContainerStyle;
            oldStyle.BasedOn = style as Style;
        }
        #endregion

        #endregion
    }
}
