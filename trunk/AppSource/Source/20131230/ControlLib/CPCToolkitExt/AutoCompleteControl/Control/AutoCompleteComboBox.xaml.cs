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
using CPCToolkitExt.ComboBoxControl;

namespace CPCToolkitExt.AutoCompleteControl
{
    /// <summary>
    /// Interaction logic for AutoCompleteComboBox.xaml
    /// </summary>
    public partial class AutoCompleteComboBox : ComboBoxBase
    {
        #region Contrustor
        public AutoCompleteComboBox()
        {
            try
            {
                InitializeComponent();
                //To Register event for Control
                this.Loaded += new RoutedEventHandler(ComboBoxQuantity_Loaded);
                //To Register event for TextBox
                this.txtKeyWord.PreviewTextInput += new TextCompositionEventHandler(KeyWord_PreviewTextInput);
                this.txtKeyWord.TextChanged += new TextChangedEventHandler(KeyWord_TextChanged);
                this.txtKeyWord.PreviewKeyDown += new KeyEventHandler(KeyWord_PreviewKeyDown);
                this.txtKeyWord.AllowDrop = false;
                this.txtKeyWord.GotFocus += new RoutedEventHandler(TextBox_GotFocus);
                this.lstContent.PreviewKeyDown += new KeyEventHandler(lstContent_PreviewKeyDown);
                //To Register event for Button
                this.btnShowPopup.Click += new RoutedEventHandler(btnShowPopup_Click);
                //To Register event for Popup
                this.popupContent.Closed += new EventHandler(PopupContent_Closed);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        #endregion

        #region Methods

        #region AddItem
        ///Add item for ListBox
        private void AddItem()
        {
            try
            {
                ///Add item for ListBox
                base.IsSelectedItem = true;
                ///check item existed
                if (this.CanUserAddRow && !this.IsExitingNewItem() && this.IsTextUnit(this.txtKeyWord.Text.Trim()) == null)
                {
                    DataModel model = new DataModel();
                    model.ID = int.Parse(this.ItemsSource.Max(x => x.ID).ToString()) + 1;
                    model.Name = this.txtKeyWord.Text.Trim();
                    model.IsNew = true;
                    this.ItemsSource.Add(model);
                    this.SelectedValue = this.lstContent.SelectedValue = model.ID;
                }
                else if (this.IsTextUnit(this.txtKeyWord.Text.Trim()) != null)
                {
                    DataModel datamodel = this.IsTextUnit(this.txtKeyWord.Text.Trim());
                    if (datamodel.IsNew)
                    {
                        datamodel.IsDirty = true;
                        datamodel.Name = this.txtKeyWord.Text.Trim();
                    }
                    this.SelectedValue = this.lstContent.SelectedValue = datamodel.ID;
                }
                else if (this.CanUserAddRow)
                {
                    this.ItemsSource[this.ItemsSource.Count - 1].IsDirty = true;
                    this.ItemsSource[this.ItemsSource.Count - 1].Name = this.txtKeyWord.Text;
                    this.SelectedValue = this.lstContent.SelectedValue = this.ItemsSource[this.ItemsSource.Count - 1].ID;
                }
                else
                    this.SelectedValue = this.lstContent.SelectedValue = 0;
                base.IsSelectedItem = false;
            }
            catch (Exception Ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<AddItem AutoCompleteExtentsions>>>>>>>>>>>>>>>>>>>>>>" + Ex.ToString());
            }
        }
        #endregion

        #region Open,Close Popup
        private void OpenPopup()
        {
            if (!this.popupContent.IsOpen)
            {
                this.popupContent.StaysOpen = false;
                this.popupContent.IsOpen = true;
            }
            this.ISOpenPopup = true;
        }
        private void ClosePopup(bool isEmpty)
        {
            try
            {
                if (isEmpty)
                {
                    if (this.CanUserAddRow && this.SelectedValue != null
                        && int.Parse(this.SelectedValue.ToString()) == this.ItemsSource[this.ItemsSource.Count - 1].ID
                        && this.ItemsSource[this.ItemsSource.Count - 1].IsNew)
                    {
                        this.ItemsSource.RemoveAt(this.ItemsSource.Count - 1);
                    }
                    this.SelectedValue = this.lstContent.SelectedValue = null;
                }
                base.IsSelectedItem = false;
                this.popupContent.StaysOpen = true;
                this.popupContent.IsOpen = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< ClosePopup >>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region SetValueChanged
        private void SetValueChanged(object newValue)
        {
            try
            {
                if (base.IsSelectedItem) return;
                this.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                if (this.ItemsSource != null && this.IsLoaded && newValue != null)
                                {
                                    base.IsSelectedItem = true;
                                    if (this.ItemsSource.Select(x => x.ID).Contains(int.Parse(newValue.ToString())))
                                    {
                                        this.txtKeyWord.Text = this.ItemsSource.SingleOrDefault(x => x.ID == int.Parse(newValue.ToString())).Name;
                                        this.lstContent.SelectedValue = newValue;
                                    }
                                    else
                                    {
                                        this.txtKeyWord.Text = string.Empty;
                                        this.lstContent.SelectedValue = null;
                                    }
                                    base.IsSelectedItem = false;
                                }
                                else
                                {
                                    this.txtKeyWord.Text = string.Empty;
                                    this.lstContent.SelectedValue = null;
                                }
                            });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<Set value SetValueChanged>>>>>>>>>>>>" + "<<<<<<<<<<<<<<<<" + ex.ToString() + ">>>>>>>>>>>>>>>>>");
            }
        }
        #endregion

        #region ChangedValue
        private void ChangedValue()
        {
            try
            {
                this.AddItem();
                base.IsSelectedItem = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<< Changed Value " + ex.ToString());
            }
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
                                         ListBoxItem listViewItem = (ListBoxItem)lstContent.ItemContainerGenerator.ContainerFromIndex(this.lstContent.SelectedIndex);
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
        #endregion

        #region Check the contents of TextBox
        private bool IsAlphaNumeric(string input)
        {
            Regex pattern = new Regex("[a-z A-Z.0-9 && * @]");
            return pattern.IsMatch(input);
        }
        private DataModel IsTextUnit(string text)
        {
            foreach (var item in this.ItemsSource)
                if (item.Name.ToLower().Contains(text.ToLower()))
                    return item;

            return null;
        }

        private bool IsExitingNewItem()
        {
            foreach (var item in this.ItemsSource)
                if (item.IsNew)
                    return true;
            return false;
        }

        private bool IsExistingContent(string text)
        {
            foreach (var item in this.ItemsSource)
                if (item.Name.Contains(text.Remove(text.Length - 1)) && item.IsNew)
                    return true;

            return false;
        }

        private bool IsEqualContent(string text)
        {
            foreach (var item in this.ItemsSource)
                if (item.Name.Equals(text.Remove(text.Length - 1)) && item.IsNew)
                    return true;

            return false;
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
                    this.SelectedValue = (this.lstContent.SelectedItem as DataModel).ID;
                    this.SetFocusTextBox();
                }
                else
                    this.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                base.IsSelectedItem = false;
            }
            catch (Exception ex)

            { Debug.Write("<<<<<<<<<<<< SetValueTextBox " + ex.ToString()); }
        }

        private void SetFocusTextBox()
        {
            try
            {
                base.IsSelectedItem = true;
                if (this.lstContent.SelectedIndex >= 0)
                {
                    if (this.txtKeyWord.Text != (this.lstContent.SelectedItem as DataModel).Name)
                        this.txtKeyWord.Text = (this.lstContent.SelectedItem as DataModel).Name;
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                               DispatcherPriority.Input,
                               (ThreadStart)delegate
                               {
                                   this.txtKeyWord.Focus();
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

        //private void SetFocusTextBox()
        //{
        //    try
        //    {
        //        this.IsSelectedItemChanged = true;
        //        if (this.lstContent.SelectedItem != null)
        //        {
        //            this.SelectedValue = (this.lstContent.SelectedItem as DataModel).ID;
        //            this.txtKeyWord.Text = (this.lstContent.SelectedItem as DataModel).Name;//this.QuantityCollection[this.lstContent.SelectedIndex].Content;//(this.QuantityCollection[this.lstContent.SelectedIndex].Quantity + this.QuantityCollection[this.lstContent.SelectedIndex].TextUnit).Trim();
        //            this.txtKeyWord.Focus();
        //            this.txtKeyWord.Dispatcher.BeginInvoke(
        //                     DispatcherPriority.Input,
        //                     (ThreadStart)delegate
        //                     {
        //                         this.txtKeyWord.SelectAll();
        //                     });
        //        }
        //        else
        //            this.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
        //        this.IsSelectedItemChanged = false;
        //    }

        //    catch (Exception ex)
        //    {
        //        Debug.Write("<<<<<<<<<<<< SetFocusTextBox " + ex.ToString());
        //    }
        //}

        #endregion

        #region Changes style
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
                                      this.txtKeyWord.Background = Brushes.Transparent;
                                      this.recIsTextBlock.Visibility = Visibility.Visible;
                                  });
            else
            {
                this.txtKeyWord.BorderThickness = new Thickness(0);
                this.btnShowPopup.Visibility = Visibility.Collapsed;
                this.txtKeyWord.IsReadOnly = true;
                this.txtKeyWord.Background = Brushes.Transparent;
                this.recIsTextBlock.Visibility = Visibility.Visible;
            }
            base.ChangeStyle();
        }

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
                                         this.txtKeyWord.Background = base.BackgroundBase;
                                     });
            else
            {
                this.txtKeyWord.BorderThickness = new Thickness(1, 1, 1.2, 1);
                this.btnShowPopup.Visibility = Visibility.Visible;
                this.txtKeyWord.IsReadOnly = false;
                this.recIsTextBlock.Visibility = Visibility.Collapsed;
                this.txtKeyWord.Background = base.BackgroundBase;
            }
            base.PreviousStyle();
        }
        #endregion

        #endregion

        #region DependencyProperties

        #region ItemsSource

        public DataCollection ItemsSource
        {
            get { return (DataCollection)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(DataCollection), typeof(AutoCompleteComboBox), new UIPropertyMetadata(null));


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

        public object DisplayMemberPath
        {
            get { return (object)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemberPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(object), typeof(AutoCompleteComboBox), new UIPropertyMetadata(null));



        #endregion

        #region SelectedValuePath
        //
        // Summary:
        //     Gets or sets the path that is used to get the System.Windows.Controls.Primitives.Selector.SelectedValue
        //     from the System.Windows.Controls.Primitives.Selector.SelectedItem. This is
        //     a dependency property.
        //
        // Returns:
        //     The path used to get the System.Windows.Controls.Primitives.Selector.SelectedValue.
        //     The default is an empty string.
        public object SelectedValuePath
        {
            get { return (object)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(object), typeof(AutoCompleteComboBox), new UIPropertyMetadata(null));

        #endregion

        #region SelectedValue
        //
        // Summary:
        //     Gets or sets the value of the System.Windows.Controls.Primitives.Selector.SelectedItem,
        //     obtained by using System.Windows.Controls.Primitives.Selector.SelectedValuePath.
        //     This is a dependency property.
        //
        // Returns:
        //     Value of the selected item.

        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(AutoCompleteComboBox), new UIPropertyMetadata(null, OnValueChanged));

        protected static void OnValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                (source as AutoCompleteComboBox).SetValueChanged(e.NewValue);
        }

        #endregion

        #region CanUserAddRow
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can add new rows to the Control.
        //
        // Returns:
        //     true if the user can add new row; otherwise, false. The registered default
        //     is true. For more information about what can influence the value, see System.Windows.DependencyProperty.
        public bool CanUserAddRow
        {
            get { return (bool)GetValue(CanUserAddRowProperty); }
            set { SetValue(CanUserAddRowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAddItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanUserAddRowProperty =
            DependencyProperty.Register("CanUserAddRow", typeof(bool), typeof(AutoCompleteComboBox), new UIPropertyMetadata(true));
        #endregion

        #endregion

        #region The events of control

        #region The events of control
        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxQuantity_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //get BackgroundBase ,BorderBase for control
                if (this.BackgroundBase == null)
                    base.BackgroundBase = this.Background;
                if (this.BorderBase == null || this.BorderBase == new Thickness(0))
                    base.BorderBase = this.BorderThickness;
                base.IsSelectedItem = false;
                base.ISOpenPopup = false;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<< Load >>>>>>>>>" + ex.ToString());
            }
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
                if (this.txtKeyWord.IsReadOnly || base.IsReadOnly) return;
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
                            else
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
                ////Set input Text
                if ((this.txtKeyWord.IsReadOnly || e.Text.Length == 0) &&
                     !this.IsAlphaNumeric(e.Text))
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
                if (base.IsReadOnly || base.IsTextBlock)
                {
                    e.Handled = true;
                    return;
                }
                if (this.lstContent.SelectedItem == null)
                    this.lstContent.SelectedIndex = 0;
                //Set focus for ListViewItem
                if (base.IsNavigationKey(e.Key))
                {
                    if (!this.popupContent.IsOpen)
                        //Open Popup
                        this.OpenPopup();
                    this.SetFocusListViewItem();
                    base.IsNavigation = true;
                }
                else if (this.popupContent.IsOpen && base.IsCancelKey(e.Key))
                    //Close Popup
                    this.ClosePopup(false);
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
            this.txtKeyWord.Dispatcher.BeginInvoke(
                   DispatcherPriority.Input,
                   (ThreadStart)delegate
                   {
                       (sender as TextBox).SelectAll();
                   });
        }
        #endregion

        #region The events of Popup
        /// <summary>
        /// Close Popup when Click item in ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupContent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Close Popup
            this.ClosePopup(false);
        }

        private void PopupContent_Closed(object sender, EventArgs e)
        {
            if (!base.IsSelectedItem)
                this.SetValueTextBox();
            base.ISOpenPopup = false;
            base.IsSelectedItem = false;
            base.IsNavigation = false;
        }

        private void lstContent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (base.IsReadOnly || base.IsTextBlock)
                    return;
                if (base.IsCancelKey(e.Key))
                    //Close Popup
                    this.ClosePopup(false);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<< lstContent_PreviewKeyDown >>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region The events of Button
        private void btnShowPopup_Click(object sender, RoutedEventArgs e)
        {
            if (base.IsReadOnly || base.IsTextBlock)
                return;
            base.IsSelectedItem = true;
            this.OpenPopup();
            this.SetFocus();
            base.IsSelectedItem = false;
        }
        #endregion
        #endregion
    }
}
