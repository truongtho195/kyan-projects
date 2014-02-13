using System;
using System.Collections.Generic;
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
using System.Windows.Markup;
using System.Globalization;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using CPCToolkitExtLibraries;
using System.Windows.Threading;
using System.Threading;
using CPCToolkitExt.Command;
using System.Collections.Specialized;

namespace CPCToolkitExt.AddressControl
{
    /// <summary>
    /// Interaction logic for Address.xaml
    /// </summary>
    public partial class Address : UserControl, IDataErrorInfo, INotifyPropertyChanged
    {
        #region Constructors
        /// <summary>
        /// Constructors
        /// </summary>
        public Address()
        {
            this.InitializeComponent();
            //To register event to control.
            this.Loaded += new RoutedEventHandler(Address_Loaded);
            this.IsValidationError = true;
            this.GotFocus += new RoutedEventHandler(Address_GotFocus);
            this.LostFocus += new RoutedEventHandler(Address_LostFocus);
            this.txtAddress.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(TxtAddress_MouseLeftButtonDown);
            //To register event to comboBox.
            this.cmbAddress.PreviewMouseWheel += new MouseWheelEventHandler(cmbAddress_PreviewMouseWheel);
        }

        #endregion

        #region Event Control
        private void Address_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Resources = Application.Current.Resources;
                //To register event when ItemSource change
                if (this.ItemsSource != null)
                    ((INotifyCollectionChanged)this.ItemsSource).CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<Load>>>>>>>>>>>" + ex.Message);
            }
        }

        #region ItemsSource_CollectionChanged
        /// <summary>
        /// This is event of ItemsSource property.It will ecxecute when ItemsSource change value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (this.IsExecuteInControl) return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems.Count > 0)
                        {
                            this.IsSetValue = true;
                            this.SetDataWithAddressType();
                            this.IsSetValue = false;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * ItemsSource_CollectionChanged *------------ \n" + ex.Message);
            }
        }
        #endregion

        private void btnAddress_Click(object sender, RoutedEventArgs e)
        {
            this.cmbAddress.Focus();
            this.cmbAddress.IsDropDownOpen = true;
        }

        private void TxtAddress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.txtAddress.Focus();
                if (e.MouseDevice.DirectlyOver.ToString().CompareTo("Microsoft.Windows.Themes.ScrollChrome") == 0
               || this.ItemsSource == null)
                    return;
                this.OpenAddressPopup();
                this.txtAddress.Focus();
                this.txtAddress.FocusVisualStyle = this.grdAddress.FocusVisualStyle;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<< PreviewMouseLeftButtonDown >>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        private void Address_LostFocus(object sender, RoutedEventArgs e)
        {
            this.KeyDown -= new KeyEventHandler(Address_KeyDown);
        }

        private void Address_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.txtAddress.FocusVisualStyle = this.grdAddress.FocusVisualStyle;
                this.KeyDown += new KeyEventHandler(Address_KeyDown);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<< Address_GotFocus >>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        private void Address_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (this.cmbAddress.SelectedItem == null
                   || this.ItemsSource == null)
                        return;
                    //To open PopupAddress view
                    this.OpenAddressPopup();
                    this.Focus();
                    this.txtAddress.FocusVisualStyle = this.grdAddress.FocusVisualStyle;
                }
                catch (Exception ex)
                {
                    Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<< PreviewMouseLeftButtonDown >>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
                }
            }
        }

        private void cmbAddress_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Methods
        protected AddressControlModel CurrentItem = null;
        protected AddressControlCollection AddressCollectionClone = null;
        public bool IsSetValue { get; internal set; }
        protected bool IsChangeAddresType { get; set; }
        protected bool IsExecuteInControl;

        #region SetVaueAddressTextBox
        /// <summary>
        /// Load data for Address textBox
        /// </summary>
        private void SetVaueAddressTextBox(int Id)
        {
            try
            {
                this.IsExecuteInControl = true;
                this.SelectedValue = null;
                string myText = string.Empty;
                //To get content of Address comboBox
                AddressControlModel addressModel = this.ItemsSource.SingleOrDefault(x => x.AddressTypeID == Id);
                if (addressModel != null && !string.IsNullOrEmpty(addressModel.AddressLine1))
                {
                    bool isDirty = addressModel.IsChangeData;
                    addressModel.AddressLine1 = addressModel.AddressLine1.Trim();
                    addressModel.City = addressModel.City.Trim();
                    addressModel.IsChangeData = isDirty;
                    myText = addressModel.AddressLine1 + "\n";
                    myText += addressModel.City;
                    if (addressModel.StateProvinceID > 0)
                    {
                        myText += ", " + this.GetValueState(addressModel.StateProvinceID);
                        if (!string.IsNullOrWhiteSpace(addressModel.PostalCode))
                            myText += " " + addressModel.PostalCode + "\n";
                        else
                            myText += "\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(addressModel.PostalCode) && addressModel.StateProvinceID <= 0)
                        myText += " " + addressModel.PostalCode + "\n";
                    else
                        myText += "\n";
                    myText += this.GetValueCountry(addressModel.CountryID);
                    ///To set value to CurrentItem.
                    this.CurrentItem = ControlHelper.DeepClone(addressModel, addressModel.GetType());
                }
                ///To set SelectedValue to control. 
                this.SelectedValue = addressModel;
                this.SelectedValueInPopup = addressModel;
                this.MyText = myText;
                this.SetIsError();
                this.IsExecuteInControl = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetVaueAddressTextBox" + ex.ToString());
            }
        }
        #endregion

        #region GetValueCountry
        /// <summary>
        /// get value Country
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetValueCountry(int CountryId)
        {
            if (this.ItemsSourceCountry != null)
                foreach (var item in ItemsSourceCountry)
                {
                    if (int.Parse(item.GetType().GetProperty(this.SelectedValuePathComboBoxCountry).GetValue(item, null).ToString()) == CountryId)
                        return item.GetType().GetProperty(this.DisplayMemderPathComboBoxCountry).GetValue(item, null).ToString();
                }
            return string.Empty;
        }

        #endregion

        #region GetValueState
        /// <summary>
        /// get value for State
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetValueState(int StateID)
        {
            if (this.ItemsSourceState != null)
                foreach (var item in this.ItemsSourceState)
                {
                    if (int.Parse(item.GetType().GetProperty(this.SelectedValuePathComboBoxState).GetValue(item, null).ToString()) == StateID)
                        return item.GetType().GetProperty(this.DisplayMemderPathComboBoxState).GetValue(item, null).ToString();
                }
            return string.Empty;
        }
        #endregion

        #region Value Default
        public void SetValueDefault(bool flag)
        {
            this.MyText = string.Empty;
            if (flag)
            {
                this.IsExecuteInControl = true;
                this.TypeCollection = new AddressTypeCollection();
                this.IsExecuteInControl = false;
            }
            this.cmbAddress.Visibility = Visibility.Visible;
            this.tblAddress.Visibility = Visibility.Collapsed;

        }
        #endregion

        #region SetDataWithAddressType
        /// <summary>
        /// To set value to TextBox and control.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetDataWithAddressType()
        {
            try
            {
                this.IsExecuteInControl = true;
                //To get data for AddressTypeCollection in Control.
                this.TypeCollection = new AddressTypeCollection();
                if (this.ItemSourceAddressType.Count > 0)
                    foreach (var item in this.ItemsSource)
                        this.TypeCollection.Add(new AddressTypeModel { ID = item.AddressTypeID, IsDefault = item.IsDefault, Name = this.ItemSourceAddressType.SingleOrDefault(x => x.ID == item.AddressTypeID).Name });
                ///To set visibility for grdAddressType(Grid) and tblAddress(TextBlock)
                if (this.TypeCollection.Count() == 1)
                {
                    this.grdAddressType.Visibility = Visibility.Collapsed;
                    this.tblAddress.Visibility = Visibility.Visible;
                    this.tblAddress.Text = TypeCollection[0].Name;
                    this.SelectedItemAddress = TypeCollection[0];
                }
                else
                {
                    if (this.TypeCollection.Count(x => x.IsDefault) > 0)
                        this.SelectedItemAddress = TypeCollection.SingleOrDefault(x => x.IsDefault);
                    else
                        this.SelectedItemAddress = TypeCollection[0];
                    this.grdAddressType.Visibility = Visibility.Visible;
                    this.tblAddress.Visibility = Visibility.Collapsed;
                    this.tblAddress.Text = string.Empty;
                }
                this.IsExecuteInControl = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<<<<<<<<<<<<<<<<<<<<<  SetDataWithAddressType   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region SetBindingTextBox
        public void SetBindingTextBox()
        {
            this.RaisePropertyChanged(() => MyText);
        }
        #endregion

        #region PropertyChagned
        public void SetPropertyChagned()
        {
        }
        #endregion

        #region SetIsError
        /// <summary>
        /// To set Error to Control when IsValidationError is True.
        /// </summary>
        private void SetIsError()
        {
            if (this.IsValidationError)
            {
                if (string.IsNullOrEmpty(this.MyText) || this.MyText.Length == 0)
                    this.ItemsSource.IsErrorData = true;
                else
                    this.ItemsSource.IsErrorData = false;
            }
        }
        #endregion

        public void SetValue(object value)
        {
            this.IsExecuteInControl = true;
            this.AddressTypeCopyCollection = new AddressTypeCollection();
            //To register event when ItemSource change
            if (this.ItemsSource != null)
            {
                ((INotifyCollectionChanged)this.ItemsSource).CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
                if (this.ItemsSource.Count > 0)
                {
                    this.IsSetValue = true;
                    this.SetDataWithAddressType();
                    this.IsSetValue = false;
                }
            }
            if (value == null || (value != null && (value as IList).Count == 0))
            {
                this.SetValueDefault(true);
                return;
            }
            this.IsExecuteInControl = false;
        }

        private void SetValuePopup()
        {
            this.IsExecuteInControl = true;
            if (this.ItemsSource.Select(x => x.AddressTypeID).Count(x => x == this.SelectedAddressInPopup.ID) == 0)
                this.ItemsSource.Add(new AddressControlModel { AddressTypeID = this.SelectedAddressInPopup.ID, IsNew = true, IsNewInControl = true });
            AddressControlModel addressModel = this.ItemsSource.SingleOrDefault(x => x.AddressTypeID == this.SelectedAddressInPopup.ID);
            ///To set SelectedValueInPopup to control. 
            this.SelectedValueInPopup = addressModel;
            //To get error on ItemsSource.
            if (this.ItemsSource.Count(x => x.Errors.Count > 0) > 0)
                this.ItemsSource.IsErrorData = true;
            else
                this.ItemsSource.IsErrorData = false;
            this.IsExecuteInControl = false;
        }

        private void ReloadDataCopyItem()
        {
            this.IsExecuteInControl = true;
            this.AddressTypeCopyCollection = new AddressTypeCollection();
            foreach (var item in this.ItemsSource.Where(x => x.Errors.Count == 0))
            {
                if (item.AddressTypeID != this.SelectedAddressInPopup.ID)
                    this.AddressTypeCopyCollection.Add(new AddressTypeModel { ID = item.AddressTypeID, Name = this.ItemSourceAddressType.SingleOrDefault(x => x.ID == item.AddressTypeID).Name, IsDefault = item.IsDefault });
            }
            if (this.AddressTypeCopyCollection.Count > 0)
            {
                this.PopupAddressView.cmbAddressCopy.Visibility = Visibility.Visible;
                this.PopupAddressView.txblCopy.Visibility = Visibility.Visible;
            }
            else
            {
                this.PopupAddressView.cmbAddressCopy.Visibility = Visibility.Collapsed;
                this.PopupAddressView.txblCopy.Visibility = Visibility.Collapsed;
            }
            this.IsExecuteInControl = false;
        }

        private void OpenAddressPopup()
        {
            this.IsExecuteInControl = true;
            //To open PopupAddress view
            this.PopupAddressView = new PopupAddressView();
            //if (this.ItemsSource.Count == 1 && this.ItemsSource[0].IsNew && this.ItemsSource.IsErrorData)
            //    this.PopupAddressView.cmbAddressType.IsEnabled = false;
            //else
            //    this.PopupAddressView.cmbAddressType.IsEnabled = true;
            this.PopupAddressView.DataContext = this;
            this.SelectedAddressInPopup = this.ItemSourceAddressType.SingleOrDefault(x => x.ID == this.SelectedItemAddress.ID);
            this.AddressCollectionClone = new AddressControlCollection();
            //To set value.
            if (this.SelectedValueInPopup == null)
            {
                this.SetValuePopup();
                this.SelectedValueInPopup.IsChangeData = false;
            }
            //To clone data.
            if (this.SelectedValueInPopup != null && !this.SelectedValueInPopup.IsNewInControl)
                this.CloneData(this.SelectedValueInPopup);
            this.PopupAddressView.ShowDialog();
            this.IsExecuteInControl = false;
        }

        private void ClosePopup()
        {
            this.IsExecuteInControl = true;
            for (int i = 0; i < this.ItemsSource.Count; i++)
            {
                //.To delete items which is new item and is errored.
                if (this.ItemsSource[i].Errors.Count > 0)
                {
                    this.ItemsSource.RemoveAt(i);
                    i--;
                }
                ///To set value for IsNewInControl property.
                else
                    this.ItemsSource[i].IsNewInControl = false;
            }
            //To get error on ItemsSource.
            if (this.ItemsSource.Count(x => x.IsDefault && x.Errors.Count > 0) > 0)
                this.ItemsSource.IsErrorData = true;
            else
                this.ItemsSource.IsErrorData = false;
            ///To set value for control.
            this.IsSetValue = true;
            if (this.ItemsSource != null && this.ItemsSource.Count > 0)
            {
                this.SetDataWithAddressType();
                //Set data edited.
                foreach (var item in this.ItemsSource)
                {
                    if (!item.IsDirty && item.IsChangeData)
                    {
                        item.IsDirty = item.IsChangeData;
                        item.IsChangeData = false;
                    }
                }
            }
            this.IsSetValue = false;
            this.IsChangeAddresType = false;
            this.IsExecuteInControl = false;
        }

        private void SetValueDefault(int ID)
        {
            try
            {
                foreach (var item in ItemsSource)
                {
                    if (item.AddressTypeID == ID)
                        item.IsDefault = true;
                    else
                        item.IsDefault = false;
                    item.IsChangeData = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<  SetValueDefault   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        private void CloneData(AddressControlModel item)
        {
            this.IsExecuteInControl = true;
            if (this.AddressCollectionClone != null
                && this.AddressCollectionClone.Where(x => x.AddressTypeID == item.AddressTypeID).Count() == 0)
                this.AddressCollectionClone.Add(this.CloneModel(item));
            this.IsExecuteInControl = false;
        }

        private AddressControlModel CloneModel(AddressControlModel item)
        {
            AddressControlModel cloneModel = ControlHelper.DeepClone(item) as AddressControlModel;
            return cloneModel;
        }

        private void RollBackData(AddressControlModel data1, AddressControlModel data2)
        {
            if (data2 != null)
            {
                this.IsExecuteInControl = true;
                data1.City = data2.City;
                data1.CountryID = data2.CountryID;
                data1.AddressLine1 = data2.AddressLine1;
                data1.StateProvinceID = data2.StateProvinceID;
                data1.PostalCode = data2.PostalCode;
                data1.IsNew = data2.IsNew;
                data1.IsDirty = data2.IsDirty;
                data1.Errors = data2.Errors;
                data1.IsChangeData = data2.IsChangeData;
                this.IsExecuteInControl = false;
            }
        }

        private void CheckHasState()
        {
            try
            {
                if (this.SelectedCountry != null && this.SelectedCountry.GetType().GetProperty("HasState").GetValue(this.SelectedCountry, null) != null)
                {
                    bool hasState = bool.Parse(this.SelectedCountry.GetType().GetProperty("HasState").GetValue(this.SelectedCountry, null).ToString());

                    if (this.SelectedAddressInPopup != null && this.ItemsSource != null)
                    {
                        var address = this.ItemsSource.SingleOrDefault(x => x.AddressTypeID == this.SelectedAddressInPopup.ID);
                        if (address != null)
                            if (hasState)
                                address.HasState = true;
                            else
                                address.HasState = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("<<<<<<<<<<<<<<<<<<<<<  Address Control **** CheckHasState   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region DependencyProperties

        #region ItemsSource
        /// <summary>
        /// ItemSource for Control
        /// </summary>
        public AddressControlCollection ItemsSource
        {
            get { return (AddressControlCollection)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(AddressControlCollection), typeof(Address), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Address).SetValue(e.NewValue);
        }

        #endregion

        #region ItemSourceAddressType
        public AddressTypeCollection ItemSourceAddressType
        {
            get { return (AddressTypeCollection)GetValue(ItemSourceAddressTypeProperty); }
            set { SetValue(ItemSourceAddressTypeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ItemSourceAddressType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceAddressTypeProperty =
            DependencyProperty.Register("ItemSourceAddressType", typeof(AddressTypeCollection), typeof(Address), new UIPropertyMetadata(null));
        #endregion

        #region ItemSource CmbAddress

        /// <Summary>
        //     Gets or sets a collection used to generate the content of the System.Windows.Controls.ItemsControl.
        //     This is a dependency property.
        //
        // Returns:
        //     A collection that is used to generate the content of the System.Windows.Controls.ItemsControl.
        //     The default is null.
        /// </summary>
        public IEnumerable ItemSourceCMBAddress
        {
            get { return (IEnumerable)GetValue(ItemSourceCMBAddressProperty); }
            set { SetValue(ItemSourceCMBAddressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSourceCMBAddress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceCMBAddressProperty =
            DependencyProperty.Register("ItemSourceCMBAddress", typeof(IEnumerable), typeof(Address));


        #endregion

        #region ItemsSource Country
        /// <summary>
        /// ItemSource for Country comboBox
        /// </summary>
        public IEnumerable ItemsSourceCountry
        {
            get { return (IEnumerable)GetValue(ItemsSourceCountryProperty); }
            set { SetValue(ItemsSourceCountryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSourceCountry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceCountryProperty =
            DependencyProperty.Register("ItemsSourceCountry", typeof(IEnumerable), typeof(Address));


        #endregion

        #region ItemsSource State
        /// <summary>
        /// ItemSource for State comboBox
        /// </summary>
        public IEnumerable ItemsSourceState
        {
            get { return (IEnumerable)GetValue(ItemsSourceStateProperty); }
            set { SetValue(ItemsSourceStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSourceState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceStateProperty =
            DependencyProperty.Register("ItemsSourceState", typeof(IEnumerable), typeof(Address));


        #endregion

        #region GridAddressTypeWidth
        /// <summary>
        /// Set value width of colum contain AddressType comboBox 
        /// </summary>
        public GridLength AddressTypeWidth
        {
            get { return (GridLength)GetValue(AddressTypeWidthProperty); }
            set { SetValue(AddressTypeWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdressTypeWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddressTypeWidthProperty =
            DependencyProperty.Register("AddressTypeWidth", typeof(GridLength), typeof(Address));


        #endregion

        #region GridContentAddressWidth
        /// <summary>
        /// Set value width of colum contain Address textBox 
        /// </summary>
        public GridLength ContentAddressWidth
        {
            get { return (GridLength)GetValue(ContentAddressWidthProperty); }
            set { SetValue(ContentAddressWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentAddressWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentAddressWidthProperty =
            DependencyProperty.Register("ContentAddressWidth", typeof(GridLength), typeof(Address));


        #endregion

        #region DisplayMemderPath
        // Summary:
        //     Gets or sets a path to a value on the source object to serve as the visual
        //     representation of the object. This is a dependency property.
        //
        // Returns:
        //     The path to a value on the source object. This can be any path, or an XPath
        //     such as "@Name". The default is an empty string ("").
        public string DisplayMemderPathComboBoxState
        {
            get { return (string)GetValue(DisplayMemderPathComboBoxStateProperty); }
            set { SetValue(DisplayMemderPathComboBoxStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemderPathComboBoxState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemderPathComboBoxStateProperty =
            DependencyProperty.Register("DisplayMemderPathComboBoxState", typeof(string), typeof(Address), new UIPropertyMetadata(string.Empty));
        //
        // Summary:
        //     Gets or sets a path to a value on the source object to serve as the visual
        //     representation of the object. This is a dependency property.
        //
        // Returns:
        //     The path to a value on the source object. This can be any path, or an XPath
        //     such as "@Name". The default is an empty string ("").
        public string DisplayMemderPathComboBoxCountry
        {
            get { return (string)GetValue(DisplayMemderPathComboBoxCountryProperty); }
            set { SetValue(DisplayMemderPathComboBoxCountryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemderPathComboBoxCountry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemderPathComboBoxCountryProperty =
            DependencyProperty.Register("DisplayMemderPathComboBoxCountry", typeof(string), typeof(Address), new UIPropertyMetadata(string.Empty));



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
        public string SelectedValuePathComboBoxState
        {
            get { return (string)GetValue(SelectedValuePathComboBoxStateProperty); }
            set { SetValue(SelectedValuePathComboBoxStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValuePathComboBoxState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuePathComboBoxStateProperty =
            DependencyProperty.Register("SelectedValuePathComboBoxState", typeof(string), typeof(Address), new UIPropertyMetadata(string.Empty));

        //
        // Summary:
        //     Gets or sets the path that is used to get the System.Windows.Controls.Primitives.Selector.SelectedValue
        //     from the System.Windows.Controls.Primitives.Selector.SelectedItem. This is
        //     a dependency property.
        //
        // Returns:
        //     The path used to get the System.Windows.Controls.Primitives.Selector.SelectedValue.
        //     The default is an empty string.


        public string SelectedValuePathComboBoxCountry
        {
            get { return (string)GetValue(SelectedValuePathComboBoxCountryProperty); }
            set { SetValue(SelectedValuePathComboBoxCountryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValuePathComboBoxCountry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuePathComboBoxCountryProperty =
            DependencyProperty.Register("SelectedValuePathComboBoxCountry", typeof(string), typeof(Address), new UIPropertyMetadata(string.Empty));


        #endregion

        #region SelectedValue
        /// <summary>
        /// 
        /// </summary>
        public AddressControlModel SelectedValue
        {
            get { return (AddressControlModel)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(AddressControlModel), typeof(Address), new PropertyMetadata(null, OnSelectedValueChanged));
        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue != null)
            {
                //(d as Address).IsSetValue = true;
                (d as Address).SetPropertyChagned();
            }
        }

        #endregion

        #region IsReloadData

        public bool IsReloadData
        {
            get { return (bool)GetValue(IsReloadDataProperty); }
            set { SetValue(IsReloadDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReloadData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReloadDataProperty =
            DependencyProperty.Register("IsReloadData", typeof(bool), typeof(Address), new PropertyMetadata(OnReloadDataChanged));
        private static void OnReloadDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue
                && e.NewValue != null
                && bool.Parse(e.NewValue.ToString()))
            {
                (d as Address).SetVaueAddressTextBox(0);
            }
        }

        #endregion

        #region IsValidationError
        public bool IsValidationError
        {
            get { return (bool)GetValue(IsValidationErrorProperty); }
            set { SetValue(IsValidationErrorProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsValidationError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidationErrorProperty =
            DependencyProperty.Register("IsValidationError", typeof(bool), typeof(Address), new PropertyMetadata(IsValidationErrorChanged));
        private static void IsValidationErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue != null
                && !string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                (d as Address).SetBindingTextBox();
            }
        }
        #endregion

        #region GetDataByPropertyName

        public string GetDataByPropertyName
        {
            get { return (string)GetValue(GetDataByPropertyNameProperty); }
            set { SetValue(GetDataByPropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GetDataByPropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GetDataByPropertyNameProperty =
            DependencyProperty.Register("GetDataByPropertyName", typeof(string), typeof(Address), new UIPropertyMetadata(string.Empty));



        #endregion

        #region DisplayDatabyAddressTypeID
        public int DisplayDatabyAddressTypeID
        {
            get { return (int)GetValue(DisplayDatabyAddressTypeIDProperty); }
            set { SetValue(DisplayDatabyAddressTypeIDProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DisplayDatabyAddressTypeID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayDatabyAddressTypeIDProperty =
            DependencyProperty.Register("DisplayDatabyAddressTypeID", typeof(int), typeof(Address), new UIPropertyMetadata(0));


        #endregion

        #endregion

        #region Properties

        #region MyText
        /// <summary>
        /// Gets or sets value for MyText
        /// </summary>
        private string _myText = string.Empty;
        public string MyText
        {
            get
            {
                return _myText;
            }
            set
            {
                _myText = value;
                RaisePropertyChanged(() => MyText);
            }
        }
        #endregion

        #region IsError
        /// <summary>
        /// Gets or sets value for IsError
        /// </summary>
        private bool _isError = false;
        public bool IsError
        {
            get { return _isError; }
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    RaisePropertyChanged(() => IsError);
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        #endregion

        #region AddressTypePopupCollection
        public AddressTypeCollection _addressTypePopupCollection;
        public AddressTypeCollection AddressTypePopupCollection
        {
            get
            {
                return _addressTypePopupCollection;
            }
            set
            {
                if (_addressTypePopupCollection != value)
                {
                    _addressTypePopupCollection = value;
                    RaisePropertyChanged(() => AddressTypePopupCollection);
                }
            }

        }
        #endregion

        #region AddressTypeCollection
        /// <summary>
        /// To get, set value AddressTypeCollection
        /// </summary>
        public AddressTypeCollection _addressTypeCollection;
        public AddressTypeCollection AddressTypeCollection
        {
            get
            {
                return _addressTypeCollection;
            }
            set
            {
                if (_addressTypeCollection != value)
                {
                    _addressTypeCollection = value;
                    RaisePropertyChanged(() => AddressTypeCollection);
                }
            }
        }

        #endregion

        #region TypeCollection
        /// <summary>
        /// To get, set value TypeCollection
        /// </summary>
        public AddressTypeCollection _typeCollection;
        public AddressTypeCollection TypeCollection
        {
            get
            {
                return _typeCollection;
            }
            set
            {
                if (_typeCollection != value)
                {
                    _typeCollection = value;
                    RaisePropertyChanged(() => TypeCollection);
                }
            }
        }

        #endregion

        #region SelectedItemAddressType

        private AddressTypeModel _selectedItemAddress;
        public AddressTypeModel SelectedItemAddress
        {
            get
            {
                return _selectedItemAddress;
            }
            set
            {
                if (_selectedItemAddress != value)
                {
                    _selectedItemAddress = value;
                    RaisePropertyChanged(() => SelectedItemAddress);
                    ///Load data for Address textbox !this.IsSetValue && 
                    if (this.SelectedItemAddress != null)
                    {
                        if (!this.IsSetValue)
                            this.SetValueDefault(this.SelectedItemAddress.ID);
                        this.SetVaueAddressTextBox(this.SelectedItemAddress.ID);
                    }
                }
            }
        }

        #endregion

        #region SelectedAddressInPopup

        private AddressTypeModel _selectedAddressInPopup;
        public AddressTypeModel SelectedAddressInPopup
        {
            get
            {
                return _selectedAddressInPopup;
            }
            set
            {
                if (value != _selectedAddressInPopup)
                {
                    _selectedAddressInPopup = value;
                    RaisePropertyChanged(() => SelectedAddressInPopup);
                    ///To load data for Address textbox !this.IsSetValue && 
                    if (this.SelectedAddressInPopup != null)
                    {
                        if (this.PopupAddressView.IsActive)
                        {
                            this.IsChangeAddresType = true;
                            this.SetValuePopup();
                            //To clone data when editing data on popup.
                            if (this.SelectedValueInPopup != null
                                && !this.SelectedValueInPopup.IsNewInControl)
                            {
                                this.CloneData(this.SelectedValueInPopup);
                                this.SelectedValueInPopup.IsChangeData = true;
                            }
                        }
                        //To load data for AddressCopy
                        this.ReloadDataCopyItem();
                    }
                }
                else
                {
                    if (this.AddressTypeCopyCollection != null && this.AddressTypeCopyCollection.Count == 0)
                    {
                        this.PopupAddressView.cmbAddressCopy.Visibility = Visibility.Collapsed;
                        this.PopupAddressView.txblCopy.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        #endregion

        #region SelectedValueInPopup

        private AddressControlModel _selectedValueInPopup;
        public AddressControlModel SelectedValueInPopup
        {
            get
            {
                return _selectedValueInPopup;
            }
            set
            {
                if (_selectedValueInPopup != value)
                {
                    _selectedValueInPopup = value;
                    RaisePropertyChanged(() => SelectedValueInPopup);
                }
            }
        }
        #endregion

        #region AddressTypeCopyCollection
        private AddressTypeCollection _addressTypeCopyCollection;
        public AddressTypeCollection AddressTypeCopyCollection
        {
            get
            {
                return _addressTypeCopyCollection;
            }
            set
            {
                if (_addressTypeCopyCollection != value)
                {
                    _addressTypeCopyCollection = value;
                    RaisePropertyChanged(() => AddressTypeCopyCollection);
                }
            }

        }
        #endregion

        #region SelectedItemAddressTypeCopy

        public AddressTypeModel _selectedItemAddressTypeCopy;
        public AddressTypeModel SelectedItemAddressTypeCopy
        {
            get
            {
                return _selectedItemAddressTypeCopy;
            }
            set
            {
                if (_selectedItemAddressTypeCopy != value)
                {
                    _selectedItemAddressTypeCopy = value;
                    RaisePropertyChanged(() => SelectedItemAddressTypeCopy);
                    if (this.SelectedItemAddressTypeCopy != null)
                        this.CopyExecute();
                }
            }

        }


        #endregion

        #region SelectedCountry

        private object _selectedCountry;
        public object SelectedCountry
        {
            get
            {
                return _selectedCountry;
            }
            set
            {
                if (_selectedCountry != value)
                {
                    _selectedCountry = value;
                    RaisePropertyChanged(() => SelectedCountry);
                    this.CheckHasState();
                }
            }
        }
        #endregion

        #endregion

        #region Field
        protected PopupAddressView PopupAddressView;
        #endregion

        #region Command

        #region SearchCommand
        /// <summary>
        /// SearchCommand
        /// <summary>
        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand(this.NewExecute);
                }
                return _searchCommand;
            }
        }
        private void NewExecute(object param)
        {

        }
        #endregion

        #region InsertCommand
        /// <summary>
        /// Insert address into AddressCollection.
        /// </summary>
        private ICommand insertCommand;
        public ICommand InsertCommand
        {
            get
            {
                if (insertCommand == null)
                {
                    insertCommand = new DelegateCommand(this.InsertExecute, this.CanInsertExecute);
                }
                return insertCommand;
            }
        }
        private bool CanInsertExecute()
        {
            if (this.SelectedValueInPopup != null
                && (this.SelectedValueInPopup.IsChangeData)
                && this.SelectedValueInPopup.Errors.Count == 0)
                return true;
            return false;
        }

        private void InsertExecute()
        {
            try
            {
                this.IsExecuteInControl = true;
                this.PopupAddressView.IsCancel = false;
                this.SetValueDefault(this.SelectedAddressInPopup.ID);
                this.PopupAddressView.Close();
                this.ClosePopup();
                this.IsExecuteInControl = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<<<<<<<<<<<<<<<<<<<<<<Cancel Execute Popup>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region CancelCommand
        /// <summary>
        /// Close address popup.
        /// </summary>
        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new DelegateCommand(this.CancelExecute);
                }
                return cancelCommand;
            }
        }
        private void CancelExecute()
        {
            try
            {
                //To set value Clone
                this.IsExecuteInControl = true;
                this.PopupAddressView.IsCancel = true;
                if (this.ItemsSource != null
                && this.ItemsSource.Count(x => (x.IsChangeData && this.SelectedValueInPopup.Errors.Count > 0 && x.AddressTypeID != this.SelectedValueInPopup.AddressTypeID)
                || (x.IsChangeData && this.SelectedValueInPopup.Errors.Count == 0)) > 0)
                {
                    MessageBoxResult msgResult = MessageBoxResult.None;
                    msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Some data has been changed. Do you want to save ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (msgResult == MessageBoxResult.No)
                    {
                        for (int i = 0; i < this.ItemsSource.Count; i++)
                        {
                            if (this.ItemsSource[i].IsNewInControl)
                            {
                                this.ItemsSource.RemoveAt(i);
                                i--;
                            }
                            else if (this.ItemsSource[i].IsChangeData)
                            {
                                this.ItemsSource[i].IsNewInControl = false;
                                this.RollBackData(this.ItemsSource[i], this.AddressCollectionClone.SingleOrDefault(x => x.AddressTypeID == this.ItemsSource[i].AddressTypeID));
                            }
                        }
                        if (this.ItemsSource.Count > 0 && this.ItemsSource.Count(x => x.IsDefault) == 0)
                            this.ItemsSource.First().IsDefault = true;
                    }
                    else
                    {
                        if (this.ItemsSource.Count == 1 && this.SelectedValueInPopup.Errors.Count > 0)
                            return;
                        for (int i = 0; i < this.ItemsSource.Count; i++)
                        {
                            if (this.ItemsSource[i].Errors.Count > 0)
                            {
                                this.ItemsSource.RemoveAt(i);
                                i--;
                            }
                            else
                                this.ItemsSource[i].IsNewInControl = false;
                        }
                        //To set value default for control
                        if (this.PopupAddressView.IsCancel
                     && this.ItemsSource != null
                     && this.ItemsSource.Count(x => x.IsChangeData && x.IsNewInControl && x.Errors.Count == 0) > 0)
                            this.ItemsSource[0].IsDefault = true;
                        else
                        {
                            foreach (var address in this.ItemsSource)
                                address.IsDefault = false;
                            var item = this.ItemsSource.SingleOrDefault(x => x.AddressTypeID == this.SelectedValueInPopup.AddressTypeID);
                            if (item != null)
                                item.IsDefault = true;
                        }
                    }
                }
                //To roll back data.
                else
                {
                    for (int i = 0; i < this.ItemsSource.Count; i++)
                    {
                        if (this.ItemsSource[i].IsNewInControl)
                        {
                            this.ItemsSource.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            this.ItemsSource[i].IsNewInControl = false;
                            this.RollBackData(this.ItemsSource[i], this.AddressCollectionClone.SingleOrDefault(x => x.AddressTypeID == this.ItemsSource[i].AddressTypeID));
                        }
                    }
                    if (this.ItemsSource.Count > 0 && this.ItemsSource.Count(x => x.IsDefault) == 0)
                        this.ItemsSource.First().IsDefault = true;
                }
                //To get error on ItemsSource.
                if (this.ItemsSource.Count(x => x.IsDefault && x.Errors.Count > 0) > 0)
                    this.ItemsSource.IsErrorData = true;
                else
                    this.ItemsSource.IsErrorData = false;
                ///To set value for control.
                this.IsSetValue = true;
                if (this.ItemsSource != null && this.ItemsSource.Count > 0)
                {
                    this.SetDataWithAddressType();
                    //Set data edited.
                    foreach (var item in this.ItemsSource)
                    {
                        if (!item.IsDirty && item.IsChangeData)
                            item.IsDirty = item.IsChangeData;
                        item.IsChangeData = false;
                    }
                }
                //this.ClosePopup();
                this.PopupAddressView.Close();
                ///To clear data.
                this.SelectedValue = null;
                this.SelectedValueInPopup = null;
                this.AddressCollectionClone.Clear();
                this.Focus();
                this.IsSetValue = false;
                this.IsExecuteInControl = false;
                this.txtAddress.FocusVisualStyle = this.grdAddress.FocusVisualStyle;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<<<<<<<<<<<<<<<<<<<<<<Cancel Execute Popup>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        private bool CanCopyExecute()
        {
            if (this.AddressTypeCopyCollection != null
                && this.AddressTypeCopyCollection.Count > 0
                && this.SelectedItemAddressTypeCopy != null)
                return true;
            return false;
        }
        #endregion

        #region CopyCommand
        /// <summary>
        /// Copy data from another address to current address.
        /// </summary>
        private ICommand _copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new DelegateCommand(this.CopyExecute, this.CanCopyExecute);
                }
                return _copyCommand;
            }
        }
        private void CopyExecute()
        {
            this.IsExecuteInControl = true;
            try
            {
                AddressControlModel model = this.ItemsSource.SingleOrDefault(x => x.AddressTypeID == this.SelectedItemAddressTypeCopy.ID);
                this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].City = model.City;
                this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].CountryID = model.CountryID;
                this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].AddressLine1 = model.AddressLine1;
                this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].StateProvinceID = model.StateProvinceID;
                this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].PostalCode = model.PostalCode;
                if (!this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].IsNew)
                    this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].IsNew = model.IsNew;
                if (!this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].IsNewInControl)
                    this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].IsNewInControl = model.IsNewInControl;
                this.ItemsSource[this.ItemsSource.IndexOf(this.SelectedValueInPopup)].IsChangeData = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<  Address Control **** CopyExecute   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            this.IsExecuteInControl = false;
        }
        #endregion

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { return string.Empty; }
        }

        protected Dictionary<string, string> _errors = new Dictionary<string, string>();
        public Dictionary<string, string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                RaisePropertyChanged(() => Errors);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string errorMessage = string.Empty;
                this.Errors.Remove(columnName);
                #region Find error
                switch (columnName)
                {
                    case "MyText":
                        if (this.IsValidationError && this.ItemsSource != null && (string.IsNullOrEmpty(this.MyText) || this.MyText.Trim().Length == 0))
                            errorMessage = "Address is requied";
                        break;
                }
                #endregion

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    this.Errors.Add(columnName, errorMessage);
                }
                return errorMessage;
            }
        }
        #endregion
    }
}

