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
    public partial class AutoCompleteTextBoxExt : AutoCompleteBase
    {
        #region Constructors
        public AutoCompleteTextBoxExt()
        {
            InitializeComponent();
            //To Register event for TextBox
            this.txtKeyWord.KeyUp += new KeyEventHandler(TxtKeyWord_KeyUp);
            this.txtKeyWord.KeyDown += new KeyEventHandler(txtKeyWord_KeyDown);
            this.txtKeyWord.TextChanged += new TextChangedEventHandler(TxtKeyWord_TextChanged);
            this.txtKeyWord.GotFocus += new RoutedEventHandler(txtKeyWord_GotFocus);
            //To Register event for ListView
            this.lstComplete.KeyUp += new KeyEventHandler(LstComplete_KeyUp);
            this.lstComplete.PreviewKeyDown += new KeyEventHandler(LstComplete_PreviewKeyDown);
            //To Register event for Popup
            this.popupResult.Closed += new EventHandler(PopupResult_Closed);
            //To Register event for Conrol
            this.Loaded += new RoutedEventHandler(AutoCompleteTextBox_Loaded);
            this.UnitDictionary = new Dictionary<int, int>();

        }
        #endregion

        #region The events of of control

        #region The events of of TextBox
        private void txtKeyWord_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {   //Close Popup 
                if (this.popupResult.IsOpen && IsCancelKey(e.Key))
                {
                    this.ClosePopup(this.txtKeyWord);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<TxtKeyWord_KeyDown>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void txtKeyWord_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.txtKeyWord.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      ///Set SelectAll for TextBox 
                                      if (this.txtKeyWord.Text.Length > 0)
                                          this.txtKeyWord.SelectAll();
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
                if (this.lstComplete.SelectedItem != null
                    && this.lstComplete.SelectedItem.GetType().GetProperty(this.CurrentUnitName).GetValue(this.lstComplete.SelectedItem, null) != null)
                {
                    this.lstComplete.SelectedItem.GetType().GetProperty(this.CurrentUnitName).SetValue(this.lstComplete.SelectedItem, AutoCompleteHelper.SetTypeBinding(this.lstComplete.SelectedItem.GetType().GetProperty(this.CurrentUnitName).GetValue(this.lstComplete.SelectedItem, null)), null);
                    this.lstComplete.SelectedItem = null;
                }
                if (string.IsNullOrEmpty(this.txtKeyWord.Text)
                || this.txtKeyWord.Text.Trim().Length == 0)
                {
                    this.lstComplete.SelectedItem = null;
                    ///Close Popup
                    if (this.popupResult.IsOpen)
                        this.ClosePopup(this.txtKeyWord);
                    else
                    {
                        //Set value default for SelectedItemResult,SelectedValue
                        base.SelectedItemResult = null;
                        base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                    }
                    ///Reset ItemSource when this.txtKeyWord.Text=string.Empty.
                    this.txtKeyWord.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        this.UnitDictionary.Clear();
                        this.ReturnValueDefault();
                    });
                    base.IsLoad = false;
                    return;
                }
                else if (this.FieldSource == null)
                {
                    //Show Popup Empty
                    this.OpenPopup(true, this.txtKeyWord);
                    base.IsLoad = false;
                    return;
                }
                ///Filter ItemSource with this.txtKeyWord.Text
                this.txtKeyWord.Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                (ThreadStart)delegate
                {
                    if (string.IsNullOrEmpty(this.txtKeyWord.Text)
                        || this.txtKeyWord.Text.Trim().Length == 0)
                    {
                        base.IsLoad = false;
                        return;
                    }
                    this.UnitDictionary.Clear();
                    //Filter data
                    this.Filter(false);
                    //Open Popup
                    this.OpenPopup(false);
                });
                ///Set value for Text
                base.Text = this.txtKeyWord.Text.Trim();
                base.IsLoad = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<Text Changed>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }
        }
        private void TxtKeyWord_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                ///Set EventHanlder when IsReadOnly
                if (base.IsReadOnly || base.IsTextBlock)
                {
                    e.Handled = true;
                    return;
                }
                if (base.IsNavigationKey(e.Key)
                    && this.dbNoResult.Visibility == Visibility.Collapsed)
                {
                    ///Open Popup 
                    this.BorderThickness = new Thickness(1, 1, 1, 0);
                    if (!this.popupResult.IsOpen)
                        this.OpenPopup(true, this.txtKeyWord);
                    //Set IsOpenPopup
                    base.ISOpenPopup = true;
                    ///Set value default
                    if (e.Key == Key.Down)
                        this.lstComplete.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     if (this.lstComplete.SelectedItem == null)
                                         this.lstComplete.SelectedIndex = 0;
                                     ListViewItem item = (ListViewItem)lstComplete.ItemContainerGenerator.ContainerFromItem(this.lstComplete.SelectedItem);
                                     if (item != null)
                                         item.Focus();
                                     ///ScrollIntoView
                                     this.lstComplete.ScrollIntoView(this.lstComplete.SelectedItem);
                                 });
                }
                if (this.popupResult.IsOpen && base.IsCancelKey(e.Key))
                    //Close Popup
                    this.ClosePopup(this.txtKeyWord);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<TxtKeyWord_KeyUp>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region The events of ListView
        private void LstComplete_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsNavigationKey(e.Key))
                return;
        }
        private void LstComplete_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (base.IsChooseCurrentItemKey(e.Key))
                {
                    this.ClosePopup(this.lstComplete);
                    txtKeyWord.Focus();
                }
                else if (base.IsCancelKey(e.Key))
                {
                    this.ClosePopup(this.lstComplete);
                    this.dbNoResult.Visibility = Visibility.Collapsed;
                    txtKeyWord.Focus();
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<LstComplete_KeyUp>>>>>>>>>>>>>>>" + ex.ToString());
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
                               try
                               {
                                   base.ISOpenPopup = false;
                                   base.BorderThickness = new Thickness(1, 1, 1, 1);
                                   base.IsLoad = true;
                                   if (this.lstComplete.SelectedItem != null)
                                   {
                                       ///Return value for CurrentUnit property
                                       if (base.SelectedItemResult != null
                        && base.SelectedItemResult.GetType().GetProperty(this.CurrentUnitName).GetValue(base.SelectedItemResult, null) != null)
                                           base.SelectedItemResult.GetType().GetProperty(this.CurrentUnitName).SetValue(base.SelectedItemResult, AutoCompleteHelper.SetTypeBinding(base.SelectedItemResult.GetType().GetProperty(this.CurrentUnitName).GetValue(base.SelectedItemResult, null)), null);
                                       ///Set value for SelectedItemResult,SelectedValue
                                       int id = int.Parse(this.lstComplete.SelectedItem.GetType().GetProperty(this.PrimaryKeyName).GetValue(this.lstComplete.SelectedItem, null).ToString());
                                       if (this.UnitDictionary.Count() > 0
                                           && this.UnitDictionary.Where(x => x.Key == id).Count() > 0)
                                       {
                                           foreach (var item in this.UnitDictionary)
                                               if (item.Key == id)
                                               {
                                                   this.lstComplete.SelectedItem.GetType().GetProperty(this.CurrentUnitName).SetValue(this.lstComplete.SelectedItem, item.Value, null);
                                                   break;
                                               }
                                       }
                                       else
                                       {
                                           string CurrentUnit = this.GetCurrentUnit(this.lstComplete.SelectedItem).Trim();
                                           this.lstComplete.SelectedItem.GetType().GetProperty(this.CurrentUnitName).SetValue(this.lstComplete.SelectedItem, int.Parse(CurrentUnit), null);
                                       }
                                       base.SelectedItemResult = AutoCompleteHelper.DeepClone(this.lstComplete.SelectedItem);
                                       if (this.lstComplete.SelectedValue != null)
                                           base.SelectedValue = this.lstComplete.SelectedValue;
                                       if (this.IsTextCompletionEnabled)
                                       {
                                           base.IsSelectedItem = true;
                                           object content = this.GetDataFieldShow(this.SelectedItemResult, FieldShow);
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
                                   ///Set value for Text
                                   base.Text = this.txtKeyWord.Text;
                                   base.IsLoad = false;
                               }
                               catch (Exception ex)
                               {
                                   Debug.Write("<<<<<<<<<<<<<<<<<<PopupResult_Closed>>>>>>>>>>>>>>" + ex.ToString());
                               }

                           });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<PopupResult_Closed>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region The events of the control
        private void AutoCompleteTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //this.Dispatcher.BeginInvoke(
                //           DispatcherPriority.Input,
                //           (ThreadStart)delegate
                //           {
                ///Set PlacementTarget for Popup
                this.popupResult.PlacementTarget = this;
                this.popupResult.IsOpen = false;
                this.popupResult.StaysOpen = true;
                ///Set MaxDropDownHeight for Control
                if (this.MaxDropDownHeight == double.PositiveInfinity || this.MaxDropDownHeight == 0)
                    this.MaxDropDownHeight = 256;
                //Visibility grid 
                this.dbNoResult.Visibility = Visibility.Collapsed;
                //get BackgroundBase ,BorderBase for control
                if (this.BackgroundBase == null)
                    base.BackgroundBase = this.Background;
                if (this.BorderBase == null || this.BorderBase == new Thickness(0))
                    base.BorderBase = this.BorderThickness;
                base.IsSelectedItem = false;
                base.ISOpenPopup = false;
                /////Set width for Popup
                if (base.Columns != null)
                {
                    double width = this.Columns.Sum(x => x.ActualWidth);
                    if (width > 0)
                        this.Shdw.MaxWidth = this.lstComplete.Width = width;
                    else if (this.Width > 0)
                        this.Shdw.MaxWidth = this.lstComplete.Width = this.Width;

                }
                ///Set SelectedValuePath for ListView
                if (!string.IsNullOrEmpty(base.SelectedValuePath))
                    this.lstComplete.SelectedValuePath = base.SelectedValuePath;

                //});
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<AutoCompleteTextBox_Loaded>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void ControlAutoComplete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Close popup
            this.ClosePopup(this.lstComplete);
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
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(AutoCompleteTextBoxExt));

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
            DependencyProperty.Register("SearchCommandParamater", typeof(object), typeof(AutoCompleteTextBoxExt));

        [Category("Common Properties")]
        public object SearchCommandParamater
        {
            get { return (object)GetValue(SearchCommandParamaterProperty); }
            set { SetValue(SearchCommandParamaterProperty, value); }
        }
        #endregion

        #region CurrentUnitName

        public string CurrentUnitName
        {
            get { return (string)GetValue(CurrentUnitNameProperty); }
            set { SetValue(CurrentUnitNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentUnitName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentUnitNameProperty =
            DependencyProperty.Register("CurrentUnitName", typeof(string), typeof(AutoCompleteTextBoxExt), new UIPropertyMetadata(string.Empty));



        #endregion

        #region AutoGenerateCurrentUnit
        public bool AutoGenerateCurrentUnit
        {
            get { return (bool)GetValue(AutoGenerateCurrentUnitProperty); }
            set { SetValue(AutoGenerateCurrentUnitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoGenerateCurrentUnit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoGenerateCurrentUnitProperty =
            DependencyProperty.Register("AutoGenerateCurrentUnit", typeof(bool), typeof(AutoCompleteTextBoxExt), new UIPropertyMetadata(false));
        #endregion

        #region PrimaryKeyName

        public string PrimaryKeyName
        {
            get { return (string)GetValue(PrimaryKeyNameProperty); }
            set { SetValue(PrimaryKeyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryKeyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryKeyNameProperty =
            DependencyProperty.Register("PrimaryKeyName", typeof(string), typeof(AutoCompleteTextBoxExt), new UIPropertyMetadata(string.Empty));


        #endregion

        #endregion

        #region Properties
        #region UnitDictionary
        public Dictionary<int, int> UnitDictionary
        {
            get;
            set;
        }
        #endregion
        #endregion

        #region Methods

        #region Filter
        /// <summary>
        /// Filter item in ItemSource 
        /// Return data for ListView result
        /// </summary>
        private void Filter(bool isSelected)
        {
            try
            {
                //Default value for search
                if (this.lstComplete.Items != null
                    && this.FieldSource != null
                    && this.FieldSource.Count > 0)
                {
                    this.lstComplete.Items.Filter = (item) =>
                    {
                        foreach (var field in this.FieldSource)
                        {
                            if (this.GetDataHasChildrenExt(item, field, this.txtKeyWord.Text.Trim()))
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

        #region GetDataHasChildrenExt
        /// <summary>
        /// Get data when search
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private bool GetDataHasChildrenExt(object data, DataSearchModel searchModel, string text)
        {
            try
            {
                //object content = null;
                switch (searchModel.Level)
                {
                    ///Search data when level=0
                    case 0:
                        object dataLevel = data.GetType().GetProperty(searchModel.KeyName).GetValue(data, null);
                        if (dataLevel == null) return false;
                        else if (AutoCompleteSearch.GetFilter(FilterMode, text, dataLevel.ToString()))
                        {
                            ///Set value for Current
                            if (text.Equals(dataLevel.ToString()))
                            {
                                int currentUnit = 0;
                                if (!string.IsNullOrEmpty(searchModel.CurrentPropertyName))
                                {
                                    string[] level = searchModel.CurrentPropertyName.Split('.');

                                    if (data.GetType().GetProperty(level[0]).GetValue(data, null) != null)
                                    {
                                        currentUnit = int.Parse(data.GetType().GetProperty(level[0]).GetValue(data, null).ToString());
                                    }
                                    else
                                    {
                                        currentUnit = int.Parse(data.GetType().GetProperty(level[1]).GetValue(data, null).ToString());
                                    }
                                }
                                this.UnitDictionary.Add(int.Parse(data.GetType().GetProperty(this.PrimaryKeyName).GetValue(data, null).ToString()), currentUnit);
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    ///Search data when level=1
                    case 1:
                        object dataLevel1 = data.GetType().GetProperty(searchModel.PropertyChildren).GetValue(data, null);
                        if (dataLevel1 == null) return false;
                        else if (searchModel.PropertyType.Equals(CPCToolkitExtPropertyType.Collection.ToString()))
                        {
                            foreach (var item in (dataLevel1 as IEnumerable))
                            {
                                int currentUnit = 0;
                                object contentLevel1 = item.GetType().GetProperty(searchModel.KeyName).GetValue(item, null);
                                if (contentLevel1 == null) return false;
                                if (AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel1.ToString()))
                                {
                                    if (text.Equals(contentLevel1.ToString()))
                                    {
                                        ///Set value for Current
                                        if (!string.IsNullOrEmpty(searchModel.CurrentPropertyName))
                                        {
                                            string[] level = searchModel.CurrentPropertyName.Split('.');
                                            currentUnit = int.Parse(item.GetType().GetProperty(level[0]).GetValue(item, null).ToString());
                                        }
                                        this.UnitDictionary.Add(int.Parse(data.GetType().GetProperty(this.PrimaryKeyName).GetValue(data, null).ToString()), currentUnit);
                                    }
                                    return true;
                                }
                            }
                            return false;
                        }
                        else
                        {
                            object contentLevel1 = dataLevel1.GetType().GetProperty(searchModel.KeyName).GetValue(dataLevel1, null);
                            if (contentLevel1 == null) return false;
                            return AutoCompleteSearch.GetFilter(FilterMode, text, contentLevel1.ToString());
                        }
                    ///Search data when level=2
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
                this.BorderThickness = new Thickness(1, 1, 1, 0);
                if (this.lstComplete.Items.IsEmpty
                || this.ItemsSource == null)
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
                    if (!isKey)
                        this.lstComplete.SelectedIndex = 0;
                }
                base.ISOpenPopup = true;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<ShowHideDataSuggestion>>>>>>>>>>>>>>" + ex.ToString() + ">>>>>>>>>>>>>>>>>>>>" + "\n");
            }
        }

        private void OpenPopup(bool isKey, Control control)
        {
            try
            {
                control.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        this.BorderThickness = new Thickness(1, 1, 1, 0);
                        if (this.lstComplete.Items.IsEmpty
                        || this.ItemsSource == null)
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
                            if (!isKey)
                                this.lstComplete.SelectedIndex = 0;
                        }
                        base.ISOpenPopup = true;
                    });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<ShowHideDataSuggestion>>>>>>>>>>>>>>" + ex.ToString() + ">>>>>>>>>>>>>>>>>>>>" + "\n");
            }
        }

        #endregion

        #region ClosePopup
        /// <summary>
        /// Close Result
        /// </summary>
        private void ClosePopup(Control control)
        {
            try
            {
                control.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        this.BorderThickness = new Thickness(1, 1, 1, 1);
                        this.popupResult.StaysOpen = true;
                        this.popupResult.IsOpen = false;
                        this.dbNoResult.Visibility = Visibility.Collapsed;
                    });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<<CloseHideDataSuggestion>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
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
                this.Dispatcher.BeginInvoke(
                             DispatcherPriority.Input,
                             (ThreadStart)delegate
                             {
                                 if (base.SelectedItemResult != null && (base.ItemsSource != null && base.ItemsSource.Cast<object>().ToList().Count > 0))
                                 {
                                     this.ReturnValueDefault();
                                     if (base.IsTextCompletionEnabled && base.SelectedItemResult != null)
                                     {
                                         base.IsSelectedItem = true;
                                         this.lstComplete.SelectedItem = base.SelectedItemResult;
                                         object content = this.GetDataFieldShow(base.SelectedItemResult, base.FieldShow);
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
                Debug.Write("<<<<<<<<<<<<<<<<<<<SetValueDefault>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
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
                this.Dispatcher.BeginInvoke(
                             DispatcherPriority.Input,
                             (ThreadStart)delegate
                             {
                                 base.IsLoad = true;
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

                                         base.SelectedItemResult = this.lstComplete.SelectedItem;
                                     }
                                     if (base.IsTextCompletionEnabled && this.lstComplete.SelectedValue != null)
                                     {
                                         base.IsSelectedItem = true;
                                         object content = this.GetDataFieldShow(this.lstComplete.SelectedItem, base.FieldShow);
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
                                 base.IsLoad = false;
                             });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<SetValueforSelectedValue>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
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
                this.Dispatcher.BeginInvoke(
                                DispatcherPriority.Input,
                                (ThreadStart)delegate
                                {
                                    base.IsSelectedItem = true;
                                    this.txtKeyWord.Text = string.Empty;
                                    base.SelectedItemResult = null;
                                    base.SelectedItemClone = null;
                                    base.SelectedValue = AutoCompleteHelper.SetTypeBinding(this.SelectedValue);
                                    base.IsSelectedItem = false;
                                });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<ClearValue>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
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
            this.Dispatcher.BeginInvoke(
                                DispatcherPriority.Input,
                                (ThreadStart)delegate
                                {
                                    this.popupResult.MaxHeight = value;
                                    base.SetMaxDropDownHeight(value);
                                });
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
                Debug.Write("<<<<<<<<<<<<<<<<<<SetFocus>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            base.SetFocus();
        }

        #endregion

        #region GetCurrentUnit
        private string GetCurrentUnit(object data)
        {
            string content = string.Empty;
            try
            {
                string fieldshow = FieldSource.CurrentPropertyDefault;
                string[] level = fieldshow.Split('.');
                /// level[0]
                object datalevel = data.GetType().GetProperty(level[0]).GetValue(data, null);
                if (datalevel != null)
                {
                    content = datalevel.ToString();
                }
                else
                {
                    object datalevel1 = data.GetType().GetProperty(level[1]).GetValue(data, null);
                    if (datalevel1 != null)
                    {
                        content = datalevel1.ToString();
                    }
                    else
                        content = string.Empty;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<GetDataFieldShow>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }
            return content;
        }
        #endregion

        #endregion

    }
}
