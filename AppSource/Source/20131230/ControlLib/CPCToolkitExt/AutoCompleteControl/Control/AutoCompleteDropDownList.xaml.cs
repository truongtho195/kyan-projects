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
    public partial class AutoCompleteDropDownList : AutoCompleteBase
    {
        #region Ctor
        public AutoCompleteDropDownList()
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
            //this.txtKeyWord.KeyUp += new KeyEventHandler(TxtKeyWord_KeyUp);
            this.txtKeyWord.TextChanged += new TextChangedEventHandler(TxtKeyWord_TextChanged);
            this.txtKeyWord.GotFocus += new RoutedEventHandler(txtKeyWord_GotFocus);
            //this.txtKeyWord.KeyDown += new KeyEventHandler(txtKeyWord_KeyDown);
            this.txtKeyWord.PreviewKeyDown += new KeyEventHandler(txtKeyWord_PreviewKeyDown);
            //To Register event for ListView
            this.lstComplete.KeyUp += new KeyEventHandler(LstComplete_KeyUp);
            this.lstComplete.PreviewKeyDown += new KeyEventHandler(LstComplete_PreviewKeyDown);
            this.lstComplete.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lstComplete_MouseLeftButtonDown);
            //To Register event for Popup
            this.popupResult.Closed += new EventHandler(PopupResult_Closed);
            //To Register event for Control
            this.Loaded += new RoutedEventHandler(AutoCompleteBox_Loaded);
            //To Register event for Button
            this.btnShowPopup.Click += new RoutedEventHandler(BtnShowPopup_Click);
        }
        #endregion

        #region The events of control

        #region The events of TextBox
        private void txtKeyWord_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (this.IsReadOnly || this.IsTextBlock)
                {
                    e.Handled = true;
                    return;
                }

                if (IsNavigationKey(e.Key)
                    && this.dbNoResult.Visibility == Visibility.Collapsed)
                {
                    ///Open Popup
                    if (!this.popupResult.IsOpen)
                    {
                        this.OpenPopup();
                    }
                    ///Set IsOpenPoup
                    base.ISOpenPopup = true;
                    ///Set value default
                    if (e.Key == Key.Down)
                        this.lstComplete.Dispatcher.BeginInvoke(
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
                                     ///ScrollIntoView
                                     this.lstComplete.ScrollIntoView(this.lstComplete.SelectedItem);
                                 });
                    base.IsKeyAcceptFromList = false;
                }

                else if (this.popupResult.IsOpen &&
                    IsCancelKey(e.Key))
                {
                    this.ClosePopup();
                    base.IsKeyAcceptFromList = false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<TxtKeyWord_PreviewKeyDown>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void txtKeyWord_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      if (this.txtKeyWord.Text.Length > 0)
                                          this.txtKeyWord.SelectAll();
                                      base.IsKeyAcceptFromList = false;
                                  });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<txtKeyWord_GotFocus>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void TxtKeyWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (base.IsSelectedItem) return;
                base.IsLoad = true;
                this.lstComplete.SelectedItem = null;
                if (string.IsNullOrEmpty(this.txtKeyWord.Text)
                    || this.txtKeyWord.Text.Trim().Length == 0)
                {
                    ///To close Popup
                    if (this.popupResult.IsOpen)
                        this.ClosePopup();
                    else
                    {
                        //To set value default for SelectedItemResult,SelectedValue
                        base.SelectedItemResult = null;
                        base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                    }
                    ///To reset ItemSource when this.txtKeyWord.Text=string.Empty.
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        this.ReturnValueDefault();
                    });
                    base.IsLoad = false;
                    return;
                }
                if (!base.IsKeyAcceptFromList)
                {
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        if (this.txtKeyWord.Text.Trim().Length == 0)
                        {
                            base.IsLoad = false;
                            return;
                        }
                        this.Filter();
                        this.OpenPopup();
                    });
                }
                base.IsKeyAcceptFromList = false;
                base.IsLoad = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<Text Changed>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }
        }
        private void TxtKeyWord_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (this.IsReadOnly || this.IsTextBlock)
                {
                    e.Handled = true;
                    return;
                }

                if (IsNavigationKey(e.Key)
                    && this.dbNoResult.Visibility == Visibility.Collapsed)
                {
                    ///Open Popup
                    if (!this.popupResult.IsOpen)
                    {
                        this.OpenPopup();
                    }
                    ///Set IsOpenPoup
                    base.ISOpenPopup = true;
                    ///Set value default
                    if (e.Key == Key.Down)
                        this.lstComplete.Dispatcher.BeginInvoke(
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
                                     ///ScrollIntoView
                                     this.lstComplete.ScrollIntoView(this.lstComplete.SelectedItem);
                                 });
                    base.IsKeyAcceptFromList = false;
                }

                else if (this.popupResult.IsOpen &&
                    IsCancelKey(e.Key))
                {
                    this.ClosePopup();
                    base.IsKeyAcceptFromList = false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<TxtKeyWord_KeyUp>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void txtKeyWord_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {   //Close Popup when 
                if (this.popupResult.IsOpen
                    && IsCancelKey(e.Key))
                {
                    this.ClosePopup();
                    base.IsKeyAcceptFromList = false;
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<TxtKeyWord_KeyDown>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region The events of ListView
        private void LstComplete_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsNavigationKey(e.Key))
            {
                base.IsKeyAcceptFromList = false;
                return;
            }
        }
        private void LstComplete_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsChooseCurrentItemKey(e.Key))
            {
                base.IsKeyAcceptFromList = true;
                this.ClosePopup();
                txtKeyWord.Focus();
            }
            else if (IsCancelKey(e.Key))
            {
                base.IsKeyAcceptFromList = false;
                this.ClosePopup();
                this.dbNoResult.Visibility = Visibility.Collapsed;
                txtKeyWord.Focus();
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

        #region The event of Popup,ComboBox
        private void PopupResult_Closed(object sender, EventArgs e)
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                           DispatcherPriority.Input,
                           (ThreadStart)delegate
                           {
                               base.ISOpenPopup = false;
                               this.IsLoad = true;
                               if (this.lstComplete.SelectedItem != null && base.IsClickItem)
                               {
                                   base.SelectedItemResult = this.lstComplete.SelectedItem;
                                   if (this.lstComplete.SelectedValue != null)
                                       base.SelectedValue = this.lstComplete.SelectedValue;
                                   if (this.IsTextCompletionEnabled)
                                   {
                                       base.IsSelectedItem = true;
                                       object content = this.GetDataFieldShow(this.SelectedItemResult, this.DisplayMemberPath);
                                       if (content != null)
                                       {
                                           this.txtKeyWord.Text = content.ToString();
                                           this.txtKeyWord.SelectAll();
                                           this.txtKeyWord.Focus();
                                       }
                                       base.IsSelectedItem = false;
                                   }
                               }
                               else
                               {
                                   base.IsSelectedItem = true;
                                   this.txtKeyWord.Text = string.Empty;
                                   base.SelectedItemResult = null;
                                   base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                                   base.IsSelectedItem = false;
                                   this.dbNoResult.Visibility = Visibility.Collapsed;
                               }
                               base.Text = this.txtKeyWord.Text;
                               base.IsLoad = false;
                           });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<PopupResult_Closed>>>>>>>>>>>>>>" + ex.ToString());
            }
        }


        #endregion

        #region The events of Control
        private void AutoCompleteBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                           DispatcherPriority.Input,
                           (ThreadStart)delegate
                           {
                               //Set PlacementTarget for Popup
                               base.IsClickItem = false;
                               this.popupResult.PlacementTarget = this;
                               this.popupResult.IsOpen = false;
                               this.popupResult.StaysOpen = true;
                               ///Set MaxDropDownHeight for Control
                               if (this.MaxDropDownHeight == double.PositiveInfinity
                                   || this.MaxDropDownHeight == 0)
                                   base.MaxDropDownHeight = 256;
                               this.dbNoResult.Visibility = Visibility.Collapsed;
                               //Get Background ,BorderThickness of Control
                               if (this.BackgroundBase == null)
                                   base.BackgroundBase = this.Background;
                               if (this.BorderBase == null || this.BorderBase == new Thickness(0))
                                   base.BorderBase = this.BorderThickness;
                               base.ISOpenPopup = false;
                               //Set width for Popup
                               if (base.Columns != null)
                               {
                                   double width = base.Columns.Sum(x => x.ActualWidth);
                                   if (width > 0)
                                       this.Shdw.MaxWidth = this.lstComplete.Width = width + 10;
                                   else if (this.Width > 0)
                                       this.Shdw.MaxWidth = this.lstComplete.Width = this.Width + 10;
                               }
                               ///Set SelectedValuePath for ListView
                               if (!string.IsNullOrEmpty(base.SelectedValuePath))
                                   this.lstComplete.SelectedValuePath = base.SelectedValuePath;
                           });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<AutoCompleteBox_Loaded>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        /// <summary>
        /// Close Popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlAutoComplete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Close Popup
            base.IsClickItem = true;
            this.ClosePopup();
        }


        #endregion

        #region The events of Button
        private void BtnShowPopup_Click(object sender, RoutedEventArgs e)
        {
            if (base.IsReadOnly || base.IsTextBlock)
                return;
            base.IsSelectedItem = true;
            this.OpenPopup();
            this.SetFocus();
            base.IsSelectedItem = false;
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
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(AutoCompleteDropDownList));

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
            DependencyProperty.Register("SearchCommandParamater", typeof(object), typeof(AutoCompleteDropDownList));

        [Category("Common Properties")]
        public object SearchCommandParamater
        {
            get { return (object)GetValue(SearchCommandParamaterProperty); }
            set { SetValue(SearchCommandParamaterProperty, value); }
        }
        #endregion

        #region DisplayMemberPath
        //
        // Summary:
        //     Gets or sets a path to a value on the source object to serve as the visual
        //     representation of the object. This is a dependency property.
        //
        // Returns:
        //     The path to a value on the source object. This can be any path, or an XPath
        //     such as "@Name". The default is an empty string ("").

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemberPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(AutoCompleteDropDownList), new UIPropertyMetadata(null));



        #endregion

        #endregion

        #region Methods

        #region Filter
        /// <summary>
        /// Filter item in ItemSource 
        /// Return data for ListView result
        /// </summary>
        private void Filter()
        {
            try
            {
                //To default value for search
                DataSearchModel field = null;
                //To search when when Combox cbFieldSearch are selected.
                if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                {
                    field = new DataSearchModel { ID = 1, Level = 0, KeyName = this.DisplayMemberPath, DisplayName = this.DisplayMemberPath };
                    this.lstComplete.Items.Filter = (item) =>
                    {
                        return this.GetDataHasChildren(item, field, this.txtKeyWord.Text.Trim());
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<Filter()>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
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
                //To set default value for searching.
                if (this.lstComplete.Items != null &&
                    this.lstComplete.Items.SourceCollection.OfType<object>().Count() > 0)
                {
                    this.lstComplete.Items.Filter = (item) =>
                    {
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

        #region Open,Close Popup

        /// <summary>
        /// Show Result
        /// </summary>
        private void OpenPopup()
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                           DispatcherPriority.Input,
                           (ThreadStart)delegate
                           {
                               if (this.lstComplete.Items.IsEmpty)
                               {
                                   this.dbShowResult.Visibility = Visibility.Collapsed;
                                   this.dbNoResult.Visibility = Visibility.Visible;
                                   this.dbNoResult.Width = this.AutoControl.Width;
                                   this.popupResult.StaysOpen = false;
                                   this.popupResult.IsOpen = true;
                               }
                               else
                               {
                                   this.dbShowResult.Visibility = Visibility.Visible;
                                   this.dbNoResult.Visibility = Visibility.Collapsed;
                                   this.popupResult.StaysOpen = false;
                                   this.popupResult.IsOpen = true;

                               }
                               base.ISOpenPopup = true;
                           });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<ShowHideDataSuggestion>>>>>>>>>>>>>>" + ex.ToString() + ">>>>>>>>>>>>>>>>>>>>" + "\n");
            }
        }

        /// <summary>
        /// Close Result
        /// </summary>
        private void ClosePopup()
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     this.popupResult.StaysOpen = true;
                                     this.popupResult.IsOpen = false;
                                     this.dbNoResult.Visibility = Visibility.Collapsed;
                                 });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<CloseHideDataSuggestion>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region SetValueWithSelectedItemResult
        /// <summary>
        /// To set value when item was selected.
        /// </summary>
        protected override void SetValueWithSelectedItemResult()
        {
            try
            {
                this.popupResult.Dispatcher.BeginInvoke(
                             DispatcherPriority.Input,
                             (ThreadStart)delegate
                             {
                                 if (this.SelectedItemResult != null && (this.ItemsSource != null && this.ItemsSource.Cast<object>().ToList().Count > 0))
                                 {
                                     this.lstComplete.SelectedItem = base.SelectedItemResult;
                                     if (base.IsTextCompletionEnabled && base.SelectedItemResult != null)
                                     {
                                         base.IsSelectedItem = true;
                                         object content = this.GetDataFieldShow(base.SelectedItemResult, this.DisplayMemberPath);
                                         if (content != null)
                                             this.txtKeyWord.Text = content.ToString();
                                         base.IsSelectedItem = false;
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
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<SetValueDefault>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region SetValueWithSelectedValue
        /// <summary>
        /// To set value when item was selected.
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
                                     this.lstComplete.SelectedValuePath = base.SelectedValuePath;
                                     if (base.SelectedValue == null || base.SelectedValue.Equals(""))
                                         this.lstComplete.SelectedValue = null;
                                     else
                                         this.lstComplete.SelectedValue = base.SelectedValue;
                                     ///Set SelectedItemResult
                                     if (base.AutoChangeSelectedItem)
                                     {
                                         base.IsLoad = true;
                                         base.SelectedItemResult = this.lstComplete.SelectedItem;
                                         base.IsLoad = false;
                                     }
                                     if (this.IsTextCompletionEnabled && this.lstComplete.SelectedValue != null)
                                     {
                                         base.IsSelectedItem = true;
                                         string content = null;
                                         if (this.lstComplete.SelectedItem != null)
                                             content = this.GetDataFieldShow(this.lstComplete.SelectedItem, this.DisplayMemberPath);
                                         if (content != null)
                                             this.txtKeyWord.Text = content;
                                         base.IsSelectedItem = false;
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
                base.SelectedValue = null;
                base.SelectedItemResult = null;
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<SetValueforSelectedValue>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region ClearValue
        protected override void ClearValue()
        {
            try
            {
                base.IsSelectedItem = true;
                this.txtKeyWord.Text = string.Empty;
                this.lstComplete.SelectedItem = null;
                this.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                base.SelectedItemResult = null;
                base.IsSelectedItem = false;
                base.ClearValue();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<ClearValue>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region Change Style
        /// <summary>
        /// To change style for control.
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
                                       this.popupResult.Visibility = Visibility.Collapsed;
                                       this.txtKeyWord.IsReadOnly = true;
                                       this.txtKeyWord.Background = Brushes.Transparent;
                                       this.recIsTextBlock.Visibility = Visibility.Visible;
                                   });
            else
            {
                this.txtKeyWord.BorderThickness = new Thickness(0);
                this.btnShowPopup.Visibility = Visibility.Collapsed;
                this.popupResult.Visibility = Visibility.Collapsed;
                this.txtKeyWord.IsReadOnly = true;
                this.txtKeyWord.Background = Brushes.Transparent;
                this.recIsTextBlock.Visibility = Visibility.Visible;
            }
            base.ChangeStyle();
        }

        /// <summary>
        /// To change style for control.
        /// </summary>
        protected override void PreviousStyle()
        {
            if (!this.IsLoaded)
                this.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      this.txtKeyWord.BorderThickness = new Thickness(1, 1, 1.1, 1.1);
                                      this.btnShowPopup.Visibility = Visibility.Visible;
                                      this.popupResult.Visibility = Visibility.Collapsed;
                                      this.txtKeyWord.IsReadOnly = false;
                                      this.recIsTextBlock.Visibility = Visibility.Collapsed;
                                      this.txtKeyWord.Background = this.BackgroundBase;
                                  });
            else
            {
                this.txtKeyWord.BorderThickness = new Thickness(1, 1, 1.1, 1.1);
                this.btnShowPopup.Visibility = Visibility.Visible;
                this.popupResult.Visibility = Visibility.Collapsed;
                this.txtKeyWord.IsReadOnly = false;
                this.recIsTextBlock.Visibility = Visibility.Collapsed;
                this.txtKeyWord.Background = this.BackgroundBase;
            }
            base.PreviousStyle();
        }
        #endregion

        #region ReadOnly
        /// <summary>
        /// To set Readonly for control.
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
                                      this.btnShowPopup.IsEnabled = false;
                                      this.popupResult.Visibility = Visibility.Collapsed;
                                  }
                                  else
                                  {
                                      this.txtKeyWord.IsReadOnly = false;
                                      this.btnShowPopup.IsEnabled = true;
                                      this.popupResult.Visibility = Visibility.Visible;
                                  }
                              });
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

        #endregion
    }
}
