using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using CPCToolkitExt.Command;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Specialized;
using CPCToolkitExt.DataGridControl.ValidationBase;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Shapes;
using System.Linq;
using CPCToolkitExt.DataGridControl.View;
namespace CPCToolkitExt.DataGridControl
{

    public class DataGridControl : DataGrid, IDataErrorInfo, INotifyPropertyChanged
    {
        #region Ctor
        public DataGridControl()
        {
            try
            {
                //To operate in DataGrid.
                this.IsExecuteInDataGrid = true;
                // Summary:
                //     Sets the System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled
                //     property for the specified object.
                ScrollViewer.SetIsDeferredScrollingEnabled(this, true);
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
                VirtualizingStackPanel.SetIsVirtualizing(this, true);
                // Summary:
                //     Sets the VirtualizingStackPanel.VirtualizationMode attached property on the
                //     specified object.
                VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
                //To register event Load of Control.
                this.Loaded += new RoutedEventHandler(DataGridControl_Loaded);
                //To set Validation on DataGridRow.
                this.RowValidationRules.Add(new RowDataInfoValidationRule { ValidationStep = ValidationStep.UpdatedValue });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl------------ \n" + ex.Message);
            }
        }
        #endregion

        #region Field
        //To store current item when this item is selected.
        private object _cloneItem = null;
        //To store data of row selected.
        private DataGridRow _currentRow;
        //To get current number of items in DataGrid.
        private int _currentNumberOfItems = 0;
        //To set value to IsExecuteInDataGrid . It is True when user are executing in DataGrid. It is False 
        protected bool IsExecuteInDataGrid { get; set; }
        //To get value of ScrollViewer
        private ScrollViewer _scrollViewer;
        /// To store error of row when the ErrorChangedHandler function execute.
        public readonly HashSet<ValidationError> Errors = new HashSet<ValidationError>();
        //Create _context menu of DataGridColumnHeader.
        private ContextMenu _contextMenu = null;
        private CustomizeColumnView _customizeColumnView = null;
        private List<KeyValuePair<object, RowModel>> AvailableColumns = new List<KeyValuePair<object, RowModel>>();
        private List<KeyValuePair<object, RowModel>> ChosenColumns = new List<KeyValuePair<object, RowModel>>();
        #endregion

        #region Events

        #region OnItemsSourceChanged
        /// <summary>
        /// To reload default value when ItemSource is changed.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            try
            {
                //To set possition of Scroll when Data of dataGrid change.
                if (this.IsPaging
                    && this._scrollViewer != null
                    && this.IsLoaded)
                    this._scrollViewer.ScrollToTop();
                //To set value define to Fields.
                this.IsExecuteInDataGrid = true;
                this.IsRowError = false;
                this.IsEditingRow = false;
                this.PageIndex = 0;
                this.RowIndex = 0;
                this.CurrentPageIndex = 0;
                this._currentRow = null;
                base.OnItemsSourceChanged(oldValue, newValue);
                //To get current row in DataGrid.
                if (this.SelectedItem != null && this.IsRollBackData && this._currentRow == null)
                    this.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      _currentRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                                  });
                //To display PageIndex.
                if (this.Items.Count > 0 && this.DisplayItems > 0)
                    this.CurrentPageIndex = this.Items.Count / this.DisplayItems;
                if (this.IsRollBackData)
                    this._itemEditedCollection = new Dictionary<object, object>();
                //To operate in DataGrid.
                this.IsExecuteInDataGrid = false;
                //To register event when ItemSource change.
                if (this.ItemsSource != null)
                {
                    //To set RowIndex.
                    if (this.SelectedIndex == 0)
                        this.RowIndex = this.SelectedIndex + 1;
                    ((INotifyCollectionChanged)this.ItemsSource).CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
                }
                this.Dispatcher.BeginInvoke(
                             DispatcherPriority.Input,
                             (ThreadStart)delegate
                             {
                                 if (this.IsFilterDataColunm)
                                     this.RegisterFilter();
                             });

            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnItemsSourceChanged *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region DataGridControl_Loaded
        /// <summary>
        /// Load event of control 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Style == null)
                {
                    // Summary:
                    //     Provides a hash table / dictionary implementation that contains WPF resources
                    //     used by components and other elements of a WPF application. 
                    ResourceDictionary dictionary = new ResourceDictionary();
                    dictionary.Source = new Uri(@"pack://application:,,,/CPCToolkitExt;component/Theme/Dictionary.xaml");
                    this.Resources = dictionary;
                    //BindingExpression.TransferValue
                    // Summary:
                    //     Gets or sets the key to use to reference the style for this control, when
                    //     theme styles are used or defined.
                    //this.Style = this.FindResource("myDataGridControlStyle") as Style;
                    //this.DefaultStyleKey = typeof(DataGridControl);

                }//To get current Row in DataGrid.
                this.GetCurrentRow();
                if (this.IsPaging)
                {
                    //To display PageIndex
                    if (this.Items.Count > 0 && this.DisplayItems > 0)
                        this.CurrentPageIndex = this.Items.Count / this.DisplayItems;
                }
                if (this.IsRollBackData)
                    this._itemEditedCollection = new Dictionary<object, object>();
                //To operate in DataGrid.
                this.IsExecuteInDataGrid = false;
                //To register event when ItemSource change
                if (this.ItemsSource != null)
                    ((INotifyCollectionChanged)this.ItemsSource).CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
                //this.LoadGroupStyle();
                //To load name of current column.
                this.GetColumnsName();
                //this.HideColumns();
                if (this.IsColunmHidden)
                    this.LoadingRow += new EventHandler<DataGridRowEventArgs>(DataGridControl_LoadingRow);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * DataGridControl_Loaded *------------ \n" + ex.Message);
            }
        }
        void DataGridControl_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            this.HideColumns();
            this.LoadingRow -= new EventHandler<DataGridRowEventArgs>(DataGridControl_LoadingRow);
        }
        #endregion

        #region DataGridControl_MouseRightButtonUp
        private void DataGridControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DependencyObject DbObject = (DependencyObject)e.OriginalSource;
                while (DbObject != null && !(DbObject is DataGridColumnHeader))
                    DbObject = VisualTreeHelper.GetParent(DbObject);
                if (DbObject == null) return;
                if (DbObject is DataGridColumnHeader)
                    (DbObject as DataGridColumnHeader).ContextMenu = this._contextMenu;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        #endregion

        #region OnExecutedBeginEdit
        //
        // Summary:
        //     Provides handling for the System.Windows.Input.CommandBinding.Executed event
        //     associated with the System.Windows.Controls.DataGrid.BeginEditCommand command.
        //
        // Parameters:
        //   e:
        //     The data for the event.

        protected override void OnExecutedBeginEdit(ExecutedRoutedEventArgs e)
        {
            try
            {
                //To operate in DataGrid.
                base.OnExecutedBeginEdit(e);
                this.IsExecuteInDataGrid = true;
                this.IsEditingRow = true;
                this.BeginEditRow();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnExecutedBeginEdit *------------ \n" + ex.Message);
            }

        }
        #endregion

        #region OnRowEditEnding
        //
        // Summary:
        //     Raises the System.Windows.Controls.DataGrid.CellEditEnding event.
        //
        // Parameters:
        //   e:
        //     The data for the event.
        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            //To end edit row.
            this.EndEditRow();
            base.OnCellEditEnding(e);
        }
        protected override void OnRowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            try
            {
                //To end edit row.
                this.EndEditRow();
                if (this.IsRollBackData)
                {
                    if (this.IsEditItem(this.SelectedItem))
                        this.AddItemdEdited();
                }
                base.OnRowEditEnding(e);
                //To set that data is rolled back.
                if (this.IsRollBackData)
                {
                    //To add item when it was edited again.  
                    if (e.EditAction == DataGridEditAction.Commit
                        && this.ItemEditedCollection.Count > 0
                        && this.ItemEditedCollection.Where(x => x.Key == e.Row.DataContext).Count() > 0
                        //To check item when it is new item.
                        && this._currentRow != null
                        && !this.IsAddItem(this.SelectedItem))
                    {
                        this.ItemEditedCollection.Remove(e.Row.DataContext);
                        this.ItemEditedCollection.Add(e.Row.DataContext, true);
                    }
                }
                //To check that data is errored.
                var rule = this.RowValidationRules[0] as ValidationRule;
                if (rule != null)
                {
                    ValidationResult validationResult = rule.Validate(this._currentRow.BindingGroup, null);
                    if (validationResult != null && !validationResult.IsValid)
                        this.IsRowError = true;
                    else
                    {
                        if (this._currentRow == null && this.IsRowError)
                            this.EnableFilteringData = false;
                        else
                            this.IsRowError = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnRowEditEnding *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region OnPreviewKeyDown
        /// <summary>
        /// OnPreviewKeyDown event of control. It will execute when users press key on keyboard.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            try
            {
                //To set IsEditing to DataGridCell in DataGridRow.
                if (this.IsRollBackData && e.Key == Key.Escape && this.IsEditingRow)
                {
                    DataGridRow control = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                    if (this._currentRow == control)
                    {
                        //To return icon of row header.
                        this.EndEditRow();
                        //To set IsEditing to DataGridCell in DataGridRow.
                        for (int i = 0; i < this.Columns.Count; i++)
                        {
                            DataGridCell cell = this.GetCell(control, i);
                            cell.IsEditing = true;
                            cell.IsEditing = false;
                            cell.FocusVisualStyle = null;
                        }
                        //To rollback data to SelectedItem.
                        //Repaired this.IsRemoveRow.
                        if (this.IsAddItem(this.SelectedItem)
                            && this.IsErrorItem(this._currentRow)
                            //To check item when it is new item.
                            && this.SelectedItem == this._currentRow.DataContext)
                        {
                            this.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Input,
                                  (ThreadStart)delegate
                                  {
                                      //Cast to interface
                                      IEditableCollectionView items = this.Items;
                                      if (items.CanRemove)
                                          items.Remove(this.SelectedItem);
                                      if (this.ItemEditedCollection.Count == 0)
                                          this.IsEditedData = false;
                                  });
                        }
                        else if (this._cloneItem != null)
                            this.SelectedItem = Helper.ConverObject(_cloneItem, this.SelectedItem);
                        //To set default value to IsSorting
                        this.IsSorting = false;
                        //To remove item which was edited in ItemEditedCollection.
                        this.RemoveItemEdited(this.SelectedItem);
                        //To set default value to IsEditingRow
                        this.IsEditingRow = false;
                        //To set default value to IsRowError
                        this.IsRowError = false;
                    }
                }
                //To delete item when the DeleteItemCommand has value.
                else if (e.Key == KeyDelete
                    && this.SelectedItem != null
                    && this.DeleteItemCommand != null
                    && (!this.IsRowError || (this.IsRowError && this._currentRow.DataContext == this.SelectedItem))
                    && (e.KeyboardDevice.FocusedElement is DataGridCell
                    || e.KeyboardDevice.FocusedElement is DataGridRow))
                {
                    this.RowIndex = 0;
                    //To remove item which was edited in ItemEditedCollection when IsRollBackData is True.
                    if (this.IsRollBackData)
                        this.RemoveItemEdited(this.SelectedItem);
                    //To execute DeleteItemCommand.
                    this.DeleteItemCommand.Execute(this.DeleteCommandParameter);
                    //To focus next item after previous item was deleted.
                    this.ItemFocused();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnPreviewKeyDown * RollBackData------------ \n" + ex.Message);
            }
        }
        #endregion

        #region OnSelectionChanged
        /// <summary>
        /// Called when the selection changes.
        /// Parameters:
        ///  e:
        ///  The data for the event.
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            try
            {
                //To end edit current row.
                this.EndEditRow();
                if (this.SelectedItem != null)
                {
                    //To clone data when IsRollBackData is True.
                    if (this.IsRollBackData)
                    {
                        //To copy data of row to can rollback it.
                        if (e.AddedItems != null && !this.IsRowError)
                        {
                            this._cloneItem = null;
                            this._currentRow = null;
                            //To set default value to IsSorting
                            this.IsSorting = false;
                            //To set default value to IsEditingRow
                            this.IsEditingRow = false;
                            //To get current row in DataGrid.
                            if (this.IsLoaded)
                                this._currentRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                            //To check data if it is new.
                            if (this.IsAddItem(this.SelectedItem))
                            {
                                this.ScrollIntoView(this.SelectedItem);
                                this.IsEditingRow = true;
                                if (this._currentRow != null && !this.IsErrorItem(this._currentRow))
                                    this._cloneItem = Helper.Clone(this.SelectedItem);
                            }
                            //To set that data rollback.
                            else if (this.IsRollBackData)
                                this._cloneItem = Helper.Clone(this.SelectedItem);
                        }
                    }
                    //To check that data is errored.
                    else if (!this.IsRowError)
                    {
                        this._currentRow = null;
                        this.GetCurrentRow();
                    }
                    //To set row index.
                    this.RowIndex = this.SelectedIndex + 1;
                }
                else
                {
                    this._currentRow = null;
                    this.RowIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnSelectionChanged *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region OnApplyTemplate
        /// <summary>
        /// The method is called just before a UI element displays in an application.
        /// </summary>
        public override void OnApplyTemplate()
        {
            try
            {
                this.OverridesDefaultStyle = true;
                base.OnApplyTemplate();
                //To get DG_ScrollViewer in Templete and to set event to it. 
                _scrollViewer = GetTemplateChild("DG_ScrollViewer") as ScrollViewer;
                if (this.IsPaging)
                {
                    if (this._scrollViewer != null)
                        _scrollViewer.ScrollChanged += OnScrollChanged;
                }
                //To register event to button "Add Item"
                if (this.VisibilityAddItem == Visibility.Visible)
                {
                    Button _btnAddItem = GetTemplateChild("BtnAddItem") as Button;
                    if (_btnAddItem != null)
                        _btnAddItem.Click += delegate
                        {
                            if (this.AddItemCommand != null)
                                this.AddItemCommand.Execute(this.AddItemCommandParameter);
                        };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnApplyTemplate *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region OnScrollChanged
        /// <summary>
        /// Occurs when changes are detected to the scroll position, extent, or viewport    size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                if (this.IsPaging && !this.IsFilteringData)
                {
                    double d = e.VerticalOffset;
                    if (e.VerticalChange != 0 && e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
                        //To call the command of control.
                        if (this.Command != null && this.Items.Count > 0 && !this.IsAddItem(this.Items[this.Items.Count - 1]))
                        {
                            //To display PageIndex
                            if (this.NumberOfItems > 0 && this.DisplayItems > 0
                                && this.Items.Count < this.NumberOfItems)
                            {
                                if (this.VisibilityNavigationBar == Visibility.Visible)
                                {
                                    if (this.NumberOfItems > 0 && this.DisplayItems > 0)
                                    {
                                        this.CurrentPageIndex = this.Items.Count / this.DisplayItems + 1;
                                        this.Command.Execute(this.CommandParameter);
                                        //To display PageIndex
                                        if (this.NumberOfItems > 0 && this.DisplayItems > 0)
                                            this.PageIndex = this.Items.Count / this.DisplayItems;
                                    }
                                }
                                //To set possition of ScrollBar
                                //this.ScrollIntoView(this.Items[this.Items.Count - this.DisplayItems]);
                                Debug.WriteLine("To execute command to add new items." + " " + this.CurrentPageIndex + " " + this.Name);
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * OnScrollChanged *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region OnSorting
        /// <summary>
        /// Occurs when a column is being sorted.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            try
            {
                base.OnSorting(eventArgs);
                this.IsSorting = true;
                this.RowIndex = this.SelectedIndex + 1;
                Debug.WriteLine("OnSorting");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl------------ \n" + ex.Message);
            }
        }
        #endregion

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
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        if (this.SelectedItem == null)
                            this.RowIndex = 0;
                        this.CurrentPageIndex = 0;
                        //To delete erorrs in DataGrid.
                        PropertyInfo inf = this.GetType().BaseType.GetProperty("HasRowValidationError", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (inf != null)
                            inf.SetValue(this, false, null);
                        if (this.IsRowError)
                        {
                            //To set default value to IsSorting
                            this.IsSorting = false;
                            //To set default value to IsEditingRow
                            this.IsEditingRow = false;
                            //To clear error in collection
                            this.IsRowError = false;
                            this.RowErrorContent = string.Empty;
                            //To check item
                            if (this.SelectedItem != null)
                            {
                                if (this.IsAddItem(this.SelectedItem))
                                    this.IsEditingRow = true;
                                else
                                    _cloneItem = Helper.Clone(this.SelectedItem);
                            }
                        }
                        //To remove item which was edited in ItemEditedCollection when IsRollBackData is True.
                        if (this.IsRollBackData)
                            foreach (var item in e.OldItems)
                                this.RemoveItemEdited(item);
                        //To focus next item after previous item was deleted.
                        this.ItemFocused();
                        //To get current row.
                        this.GetCurrentRow();
                        this.EnableFilteringData = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * ItemsSource_CollectionChanged *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region MenuItem_Click
        private void menuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ReloadColumn();
                _customizeColumnView = new CustomizeColumnView();
                //To set ItemsSource.
                this._customizeColumnView.AvailableColumns = null;
                this._customizeColumnView.ChosenColumns = null;
                this._customizeColumnView.AvailableColumns = this.AvailableColumns;
                this._customizeColumnView.ChosenColumns = this.ChosenColumns;
                this._customizeColumnView.SetItemsSource();
                this._customizeColumnView.ShowDialog();
                if (!this._customizeColumnView.IsCancel)
                {
                    //To set visibility for Columns.
                    if (this._customizeColumnView.IsColumnsAddition)
                        foreach (var item in this.Columns)
                        {
                            if (((item.Header == null && item.HeaderTemplate == null) || (item.Header is string && string.IsNullOrEmpty(item.Header.ToString()))) && this._customizeColumnView.ChosenColumns.Count(x => item.HeaderTemplate == x.Key && x.Value.IsVisible) > 0)
                                item.Visibility = Visibility.Visible;
                            else if (this._customizeColumnView.ChosenColumns.Count(x => (item.HeaderTemplate != null && item.HeaderTemplate == x.Key) && !x.Value.IsVisible) > 0)
                                item.Visibility = Visibility.Collapsed;
                            else if (this._customizeColumnView.ChosenColumns.Count(x => (item.HeaderTemplate != null && item.HeaderTemplate == x.Key) || (item.Header != null && item.Header == x.Key)) > 0)
                                item.Visibility = Visibility.Visible;
                            else
                                item.Visibility = Visibility.Collapsed;
                        }

                    //To set posssition for Columns.
                    if (this._customizeColumnView.IsColumnsAddition || this._customizeColumnView.IsChangePossitionColumn)
                        foreach (var item in this._customizeColumnView.ChosenColumns)
                        {
                            var indexChosenColumn = this._customizeColumnView.ChosenColumns.IndexOf(item);
                            var CurrentColumn = this.Columns.SingleOrDefault(x => (x.HeaderTemplate != null && x.HeaderTemplate == item.Key) || (x.Header != null && x.Header == item.Key));
                            var indexCurrentColumn = this.Columns.IndexOf(CurrentColumn);
                            if (indexCurrentColumn >= 0 && indexChosenColumn != indexCurrentColumn)
                            {
                                this.Columns.Remove(CurrentColumn);
                                this.Columns.Insert(indexChosenColumn, CurrentColumn);
                            }
                        }
                    this._customizeColumnView.ChangeClose();
                    this.GetColumnsName();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        #endregion

        #region DataGridControl_IsVisibleChanged
        void DataGridControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                this.MouseRightButtonUp += new MouseButtonEventHandler(DataGridControl_MouseRightButtonUp);
            else
                this.MouseRightButtonUp -= new MouseButtonEventHandler(DataGridControl_MouseRightButtonUp);
        }
        #endregion

        #endregion

        #region Methods

        #region IsAddItem
        /// <summary>
        /// To check data when users add  new items.
        /// </summary>
        /// <returns></returns>
        private bool IsAddItem(object value)
        {
            //To get item
            if (value != null)
            {
                if (value.GetType().GetProperty(this.Field) != null)
                {
                    var item = value.GetType().GetProperty(this.Field).GetValue(value, null);
                    if (item != null && bool.Parse(item.ToString()))
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region IsEditItem
        /// <summary>
        /// To check data when users edit items.
        /// </summary>
        /// <returns></returns>
        private bool IsEditItem(object value)
        {
            //To get item
            if (value != null)
            {
                if (value.GetType().GetProperty(this.EditField) != null)
                {
                    var item = value.GetType().GetProperty(this.EditField).GetValue(value, null);
                    if (item != null && bool.Parse(item.ToString()))
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region IsRemoveItem
        /// <summary>
        /// To check data when users delete  new items.
        /// </summary>
        /// <returns></returns>
        private bool IsRemoveItem()
        {
            //To get item
            if (this._currentNumberOfItems > this.Items.Count - 1)
                return true;
            return false;
        }
        #endregion

        #region ExecuteChangeValue
        /// <summary>
        /// This is event of IsEditedData. It will execute when IsEditedData change its value.  
        /// </summary>
        /// <param name="value"></param>
        protected void ExecuteChangeValue(bool value)
        {
            //if (!value && this.ItemEditedCollection != null && this.ItemEditedCollection.Count > 0)
            //{
            //    foreach (var item in this.ItemEditedCollection)
            //    {
            //        DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(item.Key);
            //        dataGridRow.Tag = null;
            //    }
            //    this.CanUserSortColumns = true;
            //    this.ItemEditedCollection.Clear();
            //}
        }
        #endregion

        #region AddItemdEdited
        /// <summary>
        /// To add items which was eidted from DataGrid into ItemEditedCollection.
        /// </summary>
        private void AddItemdEdited()
        {
            this.CanUserSortColumns = false;
            //To operate in DataGrid.
            this.IsExecuteInDataGrid = true;
            if (this.ItemEditedCollection.Count == 0
                //|| this.IsRowError
                || (this.ItemEditedCollection.Count > 0
                && this.ItemEditedCollection.Where(x => x.Key == this.SelectedItem).Count() == 0))
            {
                //To add item when it was edited first.  
                this.ItemEditedCollection.Add(this.SelectedItem, false);
                //To set value to DataGridRow 
                DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                if (!this.IsRowError)
                    this.IsEditedData = true;
            }
            //To operate in DataGrid.
            this.IsExecuteInDataGrid = false;
        }
        #endregion

        #region DataGirdRow Error
        private void SetDataGridRowIsError()
        {
            this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                                     dataGridRow.Tag = false;
                                 });
        }
        private void SetDataGridRowIsEdit()
        {
            this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                                     dataGridRow.Tag = this.IsEditedData;
                                 });
        }
        private void SetDataGridRowIsSelected()
        {
            this.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
                                     dataGridRow.Tag = null;
                                 });
        }
        #endregion

        #region RemoveItemEdited
        /// <summary>
        /// To remove item which was edited in ItemEditedCollection.
        /// </summary>
        private void RemoveItemEdited(object item)
        {
            //To operate in DataGrid.
            this.IsExecuteInDataGrid = true;
            //To set value to DataGridRow.
            if (this.ItemEditedCollection.Where(x => x.Key == item).Count() == 1
                && (!bool.Parse(this.ItemEditedCollection.SingleOrDefault(x => x.Key == item).Value.ToString())
                 || this.IsRowError))
            {
                DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(item);
                if (dataGridRow != null)
                    dataGridRow.Tag = null;
                //To set data to ItemEditedCollection.
                this.ItemEditedCollection.Remove(item);
            }
            //To set value to IsEditedData when user edited item in DataGrid.
            if (this.ItemEditedCollection.Count() > 0)
                this.IsEditedData = true;
            else
                this.IsEditedData = false;
            //To operate in DataGrid.
            this.IsExecuteInDataGrid = false;
        }

        private void ReloadIconRowHeader()
        {
            try
            {
                this.Dispatcher.BeginInvoke(
                                     DispatcherPriority.Input,
                                     (ThreadStart)delegate
                                     {
                                         foreach (var item in this.ItemEditedCollection)
                                         {
                                             DataGridRow dataGridRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(item.Key);
                                             if (dataGridRow != null)
                                                 dataGridRow.Tag = true;

                                         }
                                     });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------DataGridControl * ReloadIconRowHeader *------------ \n" + ex.Message);
            }
        }
        #endregion

        #region ItemFocused
        /// <summary>
        /// To set focus on DataGridRow in DataGrid.
        /// </summary>
        private void ItemFocused()
        {
            //To set focus on DataGridRow.
            this.Focus();
            if (_currentRow != null && !_currentRow.IsFocused)
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    //To set focus on DaTaGridCell.
                    DataGridCell cell = this.GetCell(_currentRow, i);
                    cell.Focus();
                }
        }
        #endregion

        #region IsErrorItem
        /// <summary>
        /// To get error from control.
        /// </summary>
        /// <returns></returns>
        private bool IsErrorItem(FrameworkElement control)
        {
            //To get error from control.
            if (Validation.GetHasError(control))
                return true;
            return false;
        }
        #endregion

        #region Get Current Row
        /// <summary>
        /// To get current row of DataGrid.
        /// </summary>
        private void GetCurrentRow()
        {
            if (this.SelectedItem != null && this._currentRow == null)
                this._currentRow = (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
        }
        private DataGridRow GetPreviousRow(object row)
        {
            if (row != null)
                return (DataGridRow)this.ItemContainerGenerator.ContainerFromItem(row);
            return null;
        }
        #endregion

        #region Filter data on colunm.

        #region Fields
        private ICollectionView _collectonView;
        //private string FieldName = string.Empty;
        //private string Content = string.Empty;
        protected bool IsFilteringData { get; set; }
        protected bool IsClosingFilterData { get; set; }
        protected ObservableCollection<FilterCondition> _fieldNameCollection;
        public ObservableCollection<FilterCondition> FieldNameCollection
        {
            get
            {
                return _fieldNameCollection;
            }
            set
            {
                _fieldNameCollection = value;
                RaisePropertyChanged(() => FieldNameCollection);
            }
        }

        #endregion

        #region Events

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsClosingFilterData)
            {
                DatePicker control = sender as DatePicker;
                FilterCondition FilterCondition = control.Tag as FilterCondition;
                FilterCondition.Control = control;
                FilterCondition.Content = control.Text;
                this.AddConditionFilter(FilterCondition);
            }
        }
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //FilterType.Text
            if (this.IsError()) return;
            if (!this.IsClosingFilterData)
            {
                TextBox control = sender as TextBox;
                FilterCondition FilterCondition = control.Tag as FilterCondition;
                FilterCondition.Control = control;
                FilterCondition.Content = control.Text;
                this.AddConditionFilter(FilterCondition);
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FilterType.Numeric
            if (this.IsError()) return;
            if (!this.IsClosingFilterData)
            {
                ComboBox control = sender as ComboBox;
                FilterCondition FilterCondition = control.Tag as FilterCondition;
                FilterCondition.Control = control;
                FilterCondition.Content = control.SelectedValue.ToString();
                this.AddConditionFilter(FilterCondition);
            }
        }
        #endregion

        #region Methods

        #region RegisterFilter
        private void RegisterFilter()
        {
            this.IsFilteringData = false;
            //To set filter data on Colunm.
            var header = this.Columns;
            foreach (var item in header)
            {
                //To set DataContext for Colunm
                item.SetValue(FrameworkElement.DataContextProperty, this.DataContext);
                //To get DisplayType ,FilterType,FilterLevel, search content of DataGridColumn.
                FilterCondition FilterCondition = new FilterCondition();
                DisplayType displayType = DataGridColumnExtensions.GetDisplayType(item);
                FilterCondition.FilterType = DataGridColumnExtensions.GetFilterType(item);
                FilterCondition.Level = DataGridColumnExtensions.GetFilterLevel(item);
                FilterCondition.FieldName = DataGridColumnExtensions.GetFieldName(item);
                //To get DataGridColumnHeader of DataGridColumn. 
                DataGridColumnHeader dataGridColumnHeader = this.GetColumnHeaderFromColumn(item);
                if (dataGridColumnHeader == null)
                    continue;
                if (!DataGridColumnExtensions.GetIsFilter(item))
                {
                    Rectangle rectangle = dataGridColumnHeader.Template.FindName("RCT_NoFilter", dataGridColumnHeader) as Rectangle;
                    if (rectangle != null)
                        rectangle.Visibility = Visibility.Visible;
                    continue;
                }
                //To set DisplayType to DataGridColumn.
                switch (displayType)
                {
                    //TextBox
                    case DisplayType.TextBox:
                        TextBox textBox = dataGridColumnHeader.Template.FindName("TXT_Filter", dataGridColumnHeader) as TextBox;
                        if (textBox != null)
                        {
                            textBox.Visibility = Visibility.Visible;
                            textBox.Tag = FilterCondition;
                            //To search with FilterType is Text.
                            textBox.TextChanged += new TextChangedEventHandler(OnTextChanged);
                        }
                        break;
                    //ComboBox.
                    case DisplayType.ComboBox:
                        ComboBox comboBox = dataGridColumnHeader.Template.FindName("ComboBox_Filter", dataGridColumnHeader) as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.Visibility = Visibility.Visible;
                            comboBox.Tag = FilterCondition;
                            //To set binding value for comboBox.
                            Binding binding = new Binding();
                            binding.Source = BindingHelper.GetItemsSource(item);
                            comboBox.DisplayMemberPath = DataGridColumnExtensions.GetDisplayMemberPath(item);
                            comboBox.SelectedValuePath = DataGridColumnExtensions.GetSelectedValuePath(item);
                            comboBox.SetBinding(ComboBox.ItemsSourceProperty, binding);
                            comboBox.SelectionChanged += new SelectionChangedEventHandler(ComboBox_SelectionChanged);
                        }
                        break;

                    //DatePicker
                    case DisplayType.DateTimePicker:
                        DatePicker datePicker = dataGridColumnHeader.Template.FindName("DatePicker_Filter", dataGridColumnHeader) as DatePicker;
                        if (datePicker != null)
                        {
                            datePicker.Visibility = Visibility.Visible;
                            datePicker.Tag = FilterCondition;
                            datePicker.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(DatePicker_SelectedDateChanged);
                        }
                        break;
                }
                this.FieldNameCollection = new ObservableCollection<FilterCondition>();
                _collectonView = CollectionViewSource.GetDefaultView(this.ItemsSource);
            }
        }
        #endregion

        #region AddConditionFilter
        /// <summary>
        /// To add filter condition
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Type"></param>
        /// <param name="Control"></param>
        private void AddConditionFilter(FilterCondition FilterCondition)
        {
            try
            {
                this.IsFilteringData = true;
                //To remove old items.
                if (string.IsNullOrEmpty(FilterCondition.Content) && FilterCondition.Content.Trim().Length == 0
                    && this.FieldNameCollection.Count > 0)
                    this.FieldNameCollection.Remove(this.FieldNameCollection.SingleOrDefault(x => x.FieldName == FilterCondition.FieldName));
                else if (this.FieldNameCollection.Select(x => x.FieldName).Contains(FilterCondition.FieldName))
                {
                    this.FieldNameCollection.Remove(this.FieldNameCollection.SingleOrDefault(x => x.FieldName == FilterCondition.FieldName));
                    this.FieldNameCollection.Add(FilterCondition);
                }
                else
                    this.FieldNameCollection.Add(FilterCondition);
                //To filter data.
                this.Filter(FilterCondition.FieldName, FilterCondition.Content);
                Debug.WriteLine("----------All Filter time :");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("----------**************AddConditionFilter****************" + ex.ToString());
            }
        }
        #endregion

        #region Row' errors
        /// <summary>
        /// To get row's errors of datagrid.
        /// </summary>
        /// <returns></returns>
        private bool IsError()
        {
            if (this.IsEditingRow)
                if (this._currentRow != null && this.IsErrorItem(this._currentRow))
                    return true;
            return false;
        }
        #endregion

        #region Filter
        /// <summary>
        /// To execute filter data
        /// </summary>
        /// <param name="Property"></param>
        /// <param name="Content"></param>
        private void Filter(string Property, string Content)
        {
            if (this.FieldNameCollection.Count == 0)
            {
                this.DataFilterResult = null;
                this._collectonView.Refresh();
                this.IsFilteringData = false;
            }
            else
            {
                (this._collectonView as ListCollectionView).CommitEdit();
                this._collectonView.Filter = (item) =>
                     {
                         int i = 0;
                         foreach (var filterCondition in FieldNameCollection)
                         {
                             if (this.FilterData(item, filterCondition))
                                 i++;
                             //object property = item.GetType().GetProperty(key.FieldName).GetValue(item, null);
                             //if (property != null)
                             //{
                             //    string propertyContent = property.ToString();
                             //    if (key.FilterType == FilterType.Numeric
                             //       && propertyContent == key.Content.ToString())
                             //        i++;
                             //    else if (key.FilterType == FilterType.Text
                             //        && propertyContent.Trim().ToLower().Contains(key.Content.ToString().Trim().ToLower()))
                             //        i++;
                             //}
                         }
                         return (i == this.FieldNameCollection.Count) ? true : false;
                     };
                this.DataFilterResult = this.ReturnData();
                //To set row index.
                this.RowIndex = this.SelectedIndex + 1;
            }
            Debug.WriteLine("----------Filter time :");
        }
        #endregion

        #region ReturnData
        /// <summary>
        /// To return data after search.
        /// </summary>
        /// <returns></returns>
        private IEnumerable ReturnData()
        {
            List<object> data = new List<object>();
            if (this._collectonView.OfType<object>().Count() == 0)
                return data;
            foreach (var item in this._collectonView.OfType<object>())
                data.Add(item);
            return data;
        }
        #endregion

        #region GetColumnHeaderFromColumn
        /// <summary>
        /// To get column header from column of datagrid.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private DataGridColumnHeader GetColumnHeaderFromColumn(DataGridColumn column)
        {
            // dataGrid is the name of your DataGrid. In this case Name="dataGrid"
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(this);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                if (columnHeader.Column == column)
                {
                    return columnHeader;
                }
            }
            return null;
        }
        #endregion

        #region GetVisualChildCollection
        /// <summary>
        /// To find column header from column of datagrid.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
        #endregion

        #region FilterData
        /// <summary>
        /// To filter data from sub model of main model.
        /// </summary>
        /// <param name="data"> main model</param>
        /// <param name="filterCondition"> fiter condition contains filter type, field name,level and context.</param>
        /// <param name="text"></param>
        /// <returns></returns>
        protected bool FilterData(object data, FilterCondition filterCondition)
        {
            try
            {
                //object content = null;
                switch (filterCondition.Level)
                {
                    ///To search data when level=0
                    case 0:
                        object dataLevel = data.GetType().GetProperty(filterCondition.FieldName).GetValue(data, null);
                        if (dataLevel == null) return false;
                        else
                        {
                            if (filterCondition.FilterType == FilterType.Numeric)
                                return AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.Equals, filterCondition.Content, dataLevel.ToString());
                            else
                                return AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.Contains, filterCondition.Content, dataLevel.ToString());
                        }
                    ///To search data when level=1
                    case 1:
                        string[] arrayLevel = filterCondition.FieldName.Split('.');
                        object dataLevel1 = data.GetType().GetProperty(arrayLevel[0]).GetValue(data, null);
                        if (dataLevel1 == null) return false;
                        else if (dataLevel1 is ObservableCollection<object> || dataLevel1 is List<object>)
                        {
                            foreach (var item in (dataLevel1 as IEnumerable))
                            {
                                object contentLevel1 = item.GetType().GetProperty(arrayLevel[1]).GetValue(item, null);
                                if (contentLevel1 == null) return false;
                                if (filterCondition.FilterType == FilterType.Numeric)
                                    return AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.Equals, filterCondition.Content, dataLevel1.ToString());
                                else
                                    return AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.Contains, filterCondition.Content, dataLevel1.ToString());
                            }
                            return false;
                        }
                        else
                        {
                            object contentLevel1 = dataLevel1.GetType().GetProperty(arrayLevel[1]).GetValue(dataLevel1, null);
                            if (contentLevel1 == null) return false;
                            if (filterCondition.FilterType == FilterType.Numeric)
                                return AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.Equals, filterCondition.Content, contentLevel1.ToString());
                            else
                                return AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.Contains, filterCondition.Content, contentLevel1.ToString());
                        }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<FilterData>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.Message);
            }

            return false;
        }
        #endregion

        #endregion

        #region VisibleFilterData
        /// <summary>
        /// Get ,Set visible of Filter data.
        /// </summary>
        private Visibility _visibleFilterData;
        public Visibility VisibleFilterData
        {
            get { return _visibleFilterData; }
            set
            {
                if (_visibleFilterData != value)
                {
                    _visibleFilterData = value;
                    RaisePropertyChanged(() => VisibleFilterData);
                }
            }
        }
        #endregion

        #region FilterCommand
        /// <summary>
        /// FilterCommand
        /// <summary>
        private ICommand _filterCommand;
        public ICommand FilterCommand
        {
            get
            {
                if (_filterCommand == null)
                {
                    _filterCommand = new RelayCommand(this.FilterExecute, this.CanFilterExecute);
                }
                return _filterCommand;
            }
        }
        private bool CanFilterExecute(object param)
        {
            return true;
        }
        private void FilterExecute(object param)
        {
            if (this.IsFilteringData)
            {
                this.ClearFilterData();
                this.IsFilteringData = false;
            }
        }
        #endregion

        #region ClearFilterData
        private void ClearFilterData()
        {
            this.IsClosingFilterData = true;
            if (this.Items.Count < this.Items.SourceCollection.OfType<object>().Count())
            {
                foreach (var item in this.FieldNameCollection)
                {
                    if (item.Control is TextBox)
                    {
                        (item.Control as TextBox).Text = string.Empty;
                    }
                    else if (item.Control is ComboBox)
                    {
                        (item.Control as ComboBox).SelectedValue = -1;
                    }
                    else if (item.Control is DatePicker)
                    {
                        (item.Control as DatePicker).SelectedDate = null;
                    }
                }
                this.DataFilterResult = null;
                this.FieldNameCollection.Clear();
                this._collectonView.Refresh();
                //To set row index.
                this.RowIndex = this.SelectedIndex + 1;
            }
            this.IsClosingFilterData = false;
        }
        #endregion

        #region TotalItemsFilter
        /// <summary>
        /// To get , set TotalItemsFilter.
        /// </summary>
        private string _totalItemsFilter = "0 item.";
        public string TotalItemsFilter
        {
            get { return _totalItemsFilter; }
            set
            {
                if (_totalItemsFilter != value)
                {
                    _totalItemsFilter = value;
                    RaisePropertyChanged(() => TotalItemsFilter);
                }
            }
        }
        #endregion

        #region EnableFilteringData
        /// <summary>
        /// To get , set value when enable colunm.
        /// </summary>
        private bool _enableFilteringData = false;
        public bool EnableFilteringData
        {
            get { return _enableFilteringData; }
            set
            {
                if (_enableFilteringData != value)
                {
                    _enableFilteringData = value;
                    RaisePropertyChanged(() => EnableFilteringData);
                }
            }
        }
        #endregion

        #region DataFilterResult
        /// <summary>
        /// To return result when user filter data.
        /// Default value is null.
        /// </summary>
        public object DataFilterResult
        {
            get { return (object)GetValue(DataFilterResultProperty); }
            set { SetValue(DataFilterResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataFilterResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataFilterResultProperty =
            DependencyProperty.Register("DataFilterResult", typeof(object), typeof(DataGridControl), new UIPropertyMetadata(null));

        #endregion

        #endregion

        #region BeginEditRow
        /// <summary>
        /// To show icon when user begin to edit row.        
        /// </summary>
        /// <returns></returns>
        private void BeginEditRow()
        {
            if (this._currentRow != null)
                this._currentRow.Tag = 1;
        }

        /// <summary>
        /// To hidden icon when user end to edit row. 
        /// </summary>
        private void EndEditRow()
        {
            if (this._currentRow != null)
                this._currentRow.Tag = 0;
        }

        /// <summary>
        /// To show icon when user begin to edit row.        
        /// </summary>
        /// <returns></returns>
        private void BeginMutilSelectRow()
        {
            if (this._currentRow != null)
                this._currentRow.Tag = 2;
        }

        /// <summary>
        /// To show icon when user begin to edit row.        
        /// </summary>
        /// <returns></returns>
        private void EndMutilSelectRow()
        {
            if (this._currentRow != null)
                this._currentRow.Tag = 0;
        }
        #endregion

        #region OnIsLockChange
        public void OnIsLockChange(bool value)
        {
            this.EnableFilteringData = !value;
        }
        #endregion

        #region Grouping

        private void RegisterGrouping()
        {
            //To set filter data on Colunm.
            var header = this.Columns;
            foreach (var item in header)
            {
                //To set DataContext for Colunm
                item.SetValue(FrameworkElement.DataContextProperty, this.DataContext);
                DataGridColumnHeader dataGridColumnHeader = this.GetColumnHeaderFromColumn(item);
                if (dataGridColumnHeader == null)
                    continue;

                dataGridColumnHeader.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(dataGridColumnHeader_MouseLeftButtonDown);
                dataGridColumnHeader.Drop += new DragEventHandler(dataGridColumnHeader_Drop);
                dataGridColumnHeader.DragLeave += new DragEventHandler(dataGridColumnHeader_DragLeave);
            }


            //this.ColumnHeaderDragStarted += new EventHandler<DragStartedEventArgs>(DataGridControl_ColumnHeaderDragStarted);
            //this.ColumnHeaderDragCompleted += new EventHandler<DragCompletedEventArgs>(DataGridControl_ColumnHeaderDragCompleted);
            //this.ColumnHeaderDragDelta += new EventHandler<DragDeltaEventArgs>(DataGridControl_ColumnHeaderDragDelta);
            //this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DataGridControl_PreviewMouseLeftButtonDown);
            //this.DragLeave += new DragEventHandler(DataGridControl_DragLeave);
            //this.Drop += new DragEventHandler(DataGridControl_Drop);
        }

        void dataGridColumnHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop((DataGridColumnHeader)sender, ((DataGridColumnHeader)sender).DataContext, DragDropEffects.Move);
        }

        void dataGridColumnHeader_Drop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DataGridColumnHeader_Drop " + sender);
        }

        void dataGridColumnHeader_DragLeave(object sender, DragEventArgs e)
        {
            Debug.WriteLine(" DataGridColumnHeader_DragLeave " + sender);
        }

        void DataGridControl_ColumnHeaderDragDelta(object sender, DragDeltaEventArgs e)
        {
        }

        void DataGridControl_ColumnHeaderDragCompleted(object sender, DragCompletedEventArgs e)
        {
        }

        void DataGridControl_ColumnHeaderDragStarted(object sender, DragStartedEventArgs e)
        {

        }

        void DataGridControl_Drop(object sender, DragEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<< DataGridControl_Drop >>>>>>>>>" + ex.ToString());
            }
            CommandManager.InvalidateRequerySuggested();
        }

        void DataGridControl_DragLeave(object sender, DragEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<< DataGridControl_DragLeave >>>>>>>>>" + ex.ToString());
            }
        }

        void DataGridControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                object a = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject);

                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                if (dep is DataGridColumnHeader)
                {
                    DragDrop.DoDragDrop((ListBox)sender, dep, DragDropEffects.Move);
                    //(dep as DataGridColumnHeader).Drop += new DragEventHandler(DataGridControl_Drop);
                    //(dep as DataGridColumnHeader).DragLeave += new DragEventHandler(DataGridControl_DragLeave);
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<< DataGridControl_PreviewMouseLeftButtonDown >>>>>>>>>" + ex.ToString());
            }
            //DragDrop.DoDragDrop((ListBox)sender, row.Content, DragDropEffects.Move);
        }
        void LoadGroupStyle()
        {
            ObservableCollection<GroupStyle> style = this.GroupStyle;
            style[0].HeaderTemplate.LoadContent();
            object a = style[0].HeaderTemplate.FindName("TXT_GroupContent", this);
        }
        private childItem FindVisualChild<childItem>(DependencyObject obj)
        where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        #endregion

        #region HideColumns
        private void HideColumns()
        {
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(DataGridControl_IsVisibleChanged);
            if (this.IsVisible)
            {
                this.MouseRightButtonUp -= new MouseButtonEventHandler(DataGridControl_MouseRightButtonUp);
                this.MouseRightButtonUp += new MouseButtonEventHandler(DataGridControl_MouseRightButtonUp);
            }
            _contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "Customize columns";
            menuItem.Click += new RoutedEventHandler(menuItem_Click);
            this._contextMenu.Items.Add(menuItem);
            if (this.NumberOfColumnsDisplayed == 0)
                this.NumberOfColumnsDisplayed = this.Columns.Count;
            int i = 0;
            //To show or to hide columns of Datagrid.
            foreach (var item in this.Columns.Where(x => x.Visibility == Visibility.Visible))
            {
                if (i < this.NumberOfColumnsDisplayed)
                {
                    if (item.Visibility != Visibility.Collapsed)
                    {
                        item.Visibility = Visibility.Visible;
                        i++;
                    }
                }
                else
                    item.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region ReloadColumn
        private void ReloadColumn()
        {
            this.AvailableColumns = new List<KeyValuePair<object, RowModel>>();
            this.ChosenColumns = new List<KeyValuePair<object, RowModel>>();
            object key = null;
            RowModel content = null;
            foreach (var item in this.Columns.OrderBy(x => x.DisplayIndex))
            {
                content = new RowModel();
                content.IsVisible = true;
                //Get column header when they have headerTemplete or Header.
                if (item.HeaderTemplate != null && ((item.HeaderTemplate as DataTemplate).LoadContent() is TextBlock))
                {
                    TextBlock textBlock = ((item.HeaderTemplate as DataTemplate).LoadContent() as TextBlock);
                    content.Content = ((item.HeaderTemplate as DataTemplate).LoadContent() as TextBlock).Text;
                    if (textBlock.Tag != null && textBlock.Tag is Boolean)
                        content.IsVisible = bool.Parse(textBlock.Tag.ToString());
                    else
                        content.IsVisible = true;
                    key = item.HeaderTemplate;
                }
                else if (item.Header != null && !string.IsNullOrEmpty(item.Header.ToString()))
                {
                    if (item.Header is TextBlock)
                    {
                        TextBlock textBlock = ((item.HeaderTemplate as DataTemplate).LoadContent() as TextBlock);
                        content.Content = ((item.HeaderTemplate as DataTemplate).LoadContent() as TextBlock).Text;
                        if (textBlock.Tag != null && textBlock.Tag is Boolean)
                            content.IsVisible = bool.Parse(textBlock.Tag.ToString());
                        else
                            content.IsVisible = true;
                        key = item.Header;
                    }
                    else
                    {
                        content.IsVisible = true;
                        content.Content = item.Header;
                        key = item.Header;
                    }
                }
                else
                {
                    if (item.Visibility == Visibility.Visible)
                        content.IsVisible = true;
                    else
                        content.IsVisible = false;
                }
                //To add column to ChosenColumns collection or AvailableColumns collection.
                if (item.Visibility == Visibility.Visible || key == null)
                    this.ChosenColumns.Add(new KeyValuePair<object, RowModel>(key, content));
                else if (content.IsVisible)
                {
                    item.Visibility = Visibility.Collapsed;
                    this.AvailableColumns.Add(new KeyValuePair<object, RowModel>(key, content));
                }
            }
        }
        #endregion

        #region GetColumnsName
        //To get name of column when it is visible.
        private void GetColumnsName()
        {
            this.CurrentColumnCollection = new ObservableCollection<string>();
            foreach (var item in this.Columns.Where(x => x.Visibility == Visibility.Visible).OrderBy(x => x.DisplayIndex))
            {
                object name = DataGridColumnExtensions.GetName(item);
                if (name != null)
                    this.CurrentColumnCollection.Add(name.ToString());
            }
        }
        #endregion

        #endregion

        #region Properties

        #region IsRowError
        /// <summary>
        /// Get ,Set error of row.
        /// </summary>
        private bool _isRowError;
        public bool IsRowError
        {
            get { return _isRowError; }
            internal set
            {
                _isRowError = value;
                RaisePropertyChanged(() => IsRowError);
                this.EnableFilteringData = !value;
            }
        }
        #endregion

        #region RowErrorContent
        /// <summary>
        /// Get ,Set content of error.
        /// </summary>
        private string _rowErrorContent = string.Empty;
        public string RowErrorContent
        {
            get { return _rowErrorContent; }
            internal set
            {
                if (_rowErrorContent != value)
                {
                    _rowErrorContent = value;
                    RaisePropertyChanged(() => RowErrorContent);
                }
            }
        }
        #endregion

        #region IsGridSortVisible
        /// <summary>
        /// Get ,Set value of Grid when user sort data.
        /// </summary>
        private Visibility _isGridSortVisible = Visibility.Collapsed;
        public Visibility IsGridSortVisible
        {
            get { return _isGridSortVisible; }
            internal set
            {
                if (_isGridSortVisible != value)
                {
                    _isGridSortVisible = value;
                    RaisePropertyChanged(() => IsGridSortVisible);
                }
            }
        }
        #endregion

        #region IsSorting
        /// <summary>
        /// Get ,Set status of row when row is sorted.
        /// </summary>
        private bool _isSorting;
        public bool IsSorting
        {
            get { return _isSorting; }
            internal set
            {
                if (_isSorting != value)
                {
                    _isSorting = value;
                    RaisePropertyChanged(() => IsSorting);
                }
            }
        }
        #endregion

        #region IsEditingRow
        /// <summary>
        /// To get , set value when row is being edited.
        /// </summary>
        private bool _isEditingRow;
        public bool IsEditingRow
        {
            get { return _isEditingRow; }
            internal set
            {
                if (_isEditingRow != value)
                {
                    _isEditingRow = value;
                    RaisePropertyChanged(() => IsEditingRow);
                }
                if (value)
                    this.EnableFilteringData = false;
            }
        }
        #endregion

        #region Field
        /// <summary>
        /// To get , set value when row is being added.
        /// </summary>
        private string _field = "IsNew";
        public string Field
        {
            get { return _field; }
            set
            {
                if (_field != value)
                {
                    _field = value;
                    RaisePropertyChanged(() => Field);
                }
            }
        }
        #endregion

        #region EditField
        /// <summary>
        /// To get , set value when row is being edited..
        /// </summary>
        private string _editField = "IsDirty";
        public string EditField
        {
            get { return _editField; }
            set
            {
                if (_editField != value)
                {
                    _editField = value;
                    RaisePropertyChanged(() => EditField);
                }
            }
        }
        #endregion

        #region IsRemoveRow
        /// <summary>
        /// To get , set value when row is being rollback but it is being error .
        /// </summary>
        private bool _isRemoveRow;
        public bool IsRemoveRow
        {
            get { return _isRemoveRow; }
            set
            {
                if (_isRemoveRow != value)
                {
                    _isRemoveRow = value;
                    RaisePropertyChanged(() => IsRemoveRow);
                }
            }
        }
        #endregion

        #region IsRemoveItemAddNew
        /// <summary>
        /// To get , set value when row is being rollback but it is being error .
        /// </summary>
        private bool _isRemoveItemAddNew;
        public bool IsRemoveItemAddNew
        {
            get { return _isRemoveItemAddNew; }
            set
            {
                if (_isRemoveItemAddNew != value)
                {
                    _isRemoveItemAddNew = value;
                    RaisePropertyChanged(() => IsRemoveItemAddNew);
                }
            }
        }
        #endregion

        #region ItemEditedCollection
        /// <summary>
        /// This is collection which contains item which was edited in DataGrid.
        /// </summary>
        private Dictionary<object, object> _itemEditedCollection;
        public Dictionary<object, object> ItemEditedCollection
        {
            get { return _itemEditedCollection; }
            set
            {
                if (_itemEditedCollection != value)
                {
                    _itemEditedCollection = value;
                    RaisePropertyChanged(() => ItemEditedCollection);
                }
            }
        }
        #endregion

        #region RowIndex
        /// <summary>
        /// To get , set index of row.
        /// </summary>
        private int _rowIndex = 0;
        public int RowIndex
        {
            get { return _rowIndex; }
            set
            {
                if (_rowIndex != value)
                {
                    _rowIndex = value;
                    RaisePropertyChanged(() => RowIndex);
                }
            }
        }
        #endregion

        #region IsActiveCommand
        /// <summary>
        /// To get , set value when row is being rollback but it is being error .
        /// </summary>
        private bool _isActiveCommand;
        public bool IsActiveCommand
        {
            get { return _isActiveCommand; }
            set
            {
                if (_isActiveCommand != value)
                {
                    _isActiveCommand = value;
                    RaisePropertyChanged(() => IsActiveCommand);
                }
            }
        }
        #endregion

        #region IsRollBackData
        /// <summary>
        /// To get , set value when row is being rollback but it is being error .
        /// </summary>
        private bool _isRollBackData = true;
        public bool IsRollBackData
        {
            get { return _isRollBackData; }
            set
            {
                if (_isRollBackData != value)
                {
                    _isRollBackData = value;
                    RaisePropertyChanged(() => IsRollBackData);
                }
            }
        }
        #endregion

        #region VisibilityAddItem
        /// <summary>
        /// To get , set value when add button is visible.
        /// </summary>
        private Visibility _visibilityAddItem = Visibility.Collapsed;
        public Visibility VisibilityAddItem
        {
            get { return _visibilityAddItem; }
            set
            {
                if (_visibilityAddItem != value)
                {
                    _visibilityAddItem = value;
                    RaisePropertyChanged(() => VisibilityAddItem);
                }
            }
        }
        #endregion

        #region IsFilterDataColunm
        /// <summary>
        /// To get , set value when users can filter data on colunm.
        /// </summary>
        private bool _isFilterDataColunm = false;
        public bool IsFilterDataColunm
        {
            get { return _isFilterDataColunm; }
            set
            {
                if (_isFilterDataColunm != value)
                {
                    _isFilterDataColunm = value;
                    RaisePropertyChanged(() => IsFilterDataColunm);
                }
                if (!value)
                    this.VisibleFilterData = Visibility.Collapsed;
            }
        }
        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                return string.Empty;
            }
        }

        public string this[string columnName]
        {
            get
            {

                string message = string.Empty;
                switch (columnName)
                {
                    case "IsRowError":
                        if (this.IsRowError)
                            if (this._currentRow != null)
                                message = Validation.GetErrors(this._currentRow)[0].ErrorContent.ToString();
                        break;

                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
                return null;
            }
        }

        #endregion

        #region RowToolTip
        /// <summary>
        /// Display tooltip on row.
        /// </summary>
        public string RowToolTip
        {
            get { return (string)GetValue(RowToolTipProperty); }
            set { SetValue(RowToolTipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowTooltip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowToolTipProperty =
            DependencyProperty.Register("RowToolTip", typeof(string), typeof(DataGridControl), new UIPropertyMetadata(string.Empty));
        #endregion

        #region IsLoadDefaultStyle
        /// <summary>
        /// To get , set value when data is on RowDetail.
        /// </summary>
        private bool _isLoadDefaultStyle = true;
        [Category("Common Properties")]
        public bool IsLoadDefaultStyle
        {
            get { return _isLoadDefaultStyle; }
            set
            {
                if (_isLoadDefaultStyle != value)
                {
                    _isLoadDefaultStyle = value;
                    RaisePropertyChanged(() => IsLoadDefaultStyle);
                }
            }
        }
        #endregion

        #endregion

        #region DependencyProperties

        #region Command
        //
        // Summary:
        //     Gets or sets the command to invoke when ScrollBar in DataGrid is pulled down. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when ScrollBar in DataGrid is pulled down. The default value is null.
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DataGridControl));

        [Category("Common Properties")]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #endregion

        #region CommandParameter
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(DataGridControl));

        [Category("Common Properties")]
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        #endregion

        #region NavigationBarBrush
        // Summary:
        //     Gets or sets a brush that describes the background of navigation bar of control.
        //
        // Returns:
        //     The brush that is used to fill the background of navigation bar of the control. The default
        //     is System.Windows.Media.Brushes.LightGray.
        [Category("Brushes")]
        public Brush NavigationBarBrush
        {
            get { return (Brush)GetValue(NavigationBarBrushProperty); }
            set { SetValue(NavigationBarBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for NavigationBarBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NavigationBarBrushProperty =
            DependencyProperty.Register("NavigationBarBrush", typeof(Brush), typeof(DataGridControl), new UIPropertyMetadata(Brushes.LightGray));

        #endregion

        #region NavigationBarHeight
        /// <summary>
        /// Gets or sets the suggested height of the element.
        /// Returns:
        ///     The height of the element, in device-independent units (1/96th inch per unit).
        ///     The default value is System.Double.NaN. This value must be equal to or greater
        ///     than 0.0.
        /// </summary>
        [Category("Layout")]
        public double NavigationBarHeight
        {
            get { return (double)GetValue(NavigationBarHeightProperty); }
            set { SetValue(NavigationBarHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NavigationBarHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NavigationBarHeightProperty =
            DependencyProperty.Register("NavigationBarHeight", typeof(double), typeof(DataGridControl), new UIPropertyMetadata(20.0));

        #endregion

        #region VisibilityNavigationBar
        /// <summary>
        /// To display navigation bar when its value is Visible and Hidden navigation bar when its value is Visible .Default value is Visible.
        /// </summary>
        public Visibility VisibilityNavigationBar
        {
            get { return (Visibility)GetValue(VisibilityNavigationBarProperty); }
            set { SetValue(VisibilityNavigationBarProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibilityNavigationBar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityNavigationBarProperty =
            DependencyProperty.Register("VisibilityNavigationBar", typeof(Visibility), typeof(DataGridControl), new UIPropertyMetadata(Visibility.Visible));

        #endregion

        #region TopCommand
        /// <summary>
        /// TopExecute
        /// <summary>
        private ICommand _topCommand;
        public ICommand TopCommand
        {
            get
            {
                if (_topCommand == null)
                {
                    _topCommand = new RelayCommand(this.TopExecute, this.CanTopExecute);
                }
                return _topCommand;
            }
        }
        private bool CanTopExecute(object param)
        {
            if (this.Items.Count == 0)
                return false;
            return true;
        }
        Stopwatch _stopwatch;
        private void TopExecute(object param)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            if (this._scrollViewer != null)
            {
                if (param.Equals("Top"))
                    this._scrollViewer.ScrollToTop();
                else
                    this._scrollViewer.ScrollToEnd();
                _stopwatch.Stop();
            }
            Debug.WriteLine(_stopwatch.Elapsed.Milliseconds);
        }
        #endregion

        #region ColumnHeaderCommand
        /// <summary>
        /// TopExecute
        /// <summary>
        private ICommand _columnHeaderCommand;
        protected ICommand ColumnHeaderCommand
        {
            get
            {
                if (_columnHeaderCommand == null)
                {
                    _columnHeaderCommand = new RelayCommand(this.ColumnHeaderExecute);
                }
                return _columnHeaderCommand;
            }
        }
        private void ColumnHeaderExecute(object param)
        {
            if (param != null && bool.Parse(param.ToString()))
            {
                Debug.WriteLine(" Start Sorting ");
            }
        }
        #endregion

        #region NumberOfItems

        /// <summary>
        /// Get set number of Items
        /// </summary>
        public int NumberOfItems
        {
            get { return (int)GetValue(NumberOfItemsProperty); }
            set { SetValue(NumberOfItemsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for NumberOfItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfItemsProperty =
            DependencyProperty.Register("NumberOfItems", typeof(int), typeof(DataGridControl), new UIPropertyMetadata(0));


        #endregion

        #region PageIndex

        public int PageIndex
        {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register("PageIndex", typeof(int), typeof(DataGridControl), new UIPropertyMetadata(1));

        #endregion

        #region DisplayItems

        public int DisplayItems
        {
            get { return (int)GetValue(DisplayItemsProperty); }
            set { SetValue(DisplayItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayItemsProperty =
            DependencyProperty.Register("DisplayItems", typeof(int), typeof(DataGridControl), new UIPropertyMetadata(0));

        #endregion

        #region CurrentPageIndex

        public int CurrentPageIndex
        {
            get { return (int)GetValue(CurrentPageIndexProperty); }
            set { SetValue(CurrentPageIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPageIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageIndexProperty =
            DependencyProperty.Register("CurrentPageIndex", typeof(int), typeof(DataGridControl), new UIPropertyMetadata(0));


        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
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

        #region DeleteItemCommand
        //
        // Summary:
        //     Gets or sets the command to invoke when ScrollBar in DataGrid is pulled down. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when ScrollBar in DataGrid is pulled down. The default value is null.
        public static readonly DependencyProperty DeleteItemCommandProperty =
            DependencyProperty.Register("DeleteItemCommand", typeof(ICommand), typeof(DataGridControl));

        [Category("Common Properties")]
        public ICommand DeleteItemCommand
        {
            get { return (ICommand)GetValue(DeleteItemCommandProperty); }
            set { SetValue(DeleteItemCommandProperty, value); }
        }
        #endregion

        #region CommandParamater
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty DeleteCommandParameterProperty =
            DependencyProperty.Register("DeleteCommandParameter", typeof(object), typeof(DataGridControl));

        [Category("Common Properties")]
        public object DeleteCommandParameter
        {
            get { return (object)GetValue(DeleteCommandParameterProperty); }
            set { SetValue(DeleteCommandParameterProperty, value); }
        }
        #endregion

        #region KeyDelete
        /// <summary>
        /// To set key to delete item.
        /// </summary>
        public Key KeyDelete
        {
            get { return (Key)GetValue(KeyDeleteProperty); }
            set { SetValue(KeyDeleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyDelete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyDeleteProperty =
            DependencyProperty.Register("KeyDelete", typeof(Key), typeof(DataGridControl), new UIPropertyMetadata(Key.Delete));

        #endregion

        #region IsEditedData
        /// <summary>
        /// Get ,set value to IsEditedData when user edit item in DataGrid.
        /// </summary>
        public bool IsEditedData
        {
            get { return (bool)GetValue(IsEditedDataProperty); }
            set { SetValue(IsEditedDataProperty, value); }
        }


        // Using a DependencyProperty as the backing store for IsEditedData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditedDataProperty =
            DependencyProperty.Register("IsEditedData", typeof(bool), typeof(DataGridControl), new UIPropertyMetadata(true, ChangeValue));

        protected static void ChangeValue(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source as DataGridControl).IsExecuteInDataGrid)
            {
                (source as DataGridControl).ExecuteChangeValue(bool.Parse(e.NewValue.ToString()));
            }
        }
        #endregion

        #region AddItemCommand
        //
        // Summary:
        //     Gets or sets the command to invoke when user click button to add a new item. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when user click button to add a new item. The default value is null.
        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register("AddItemCommand", typeof(ICommand), typeof(DataGridControl));

        [Category("Common Properties")]
        public ICommand AddItemCommand
        {
            get { return (ICommand)GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }
        #endregion

        #region AddItemCommandParameter
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty AddItemCommandParameterProperty =
            DependencyProperty.Register("AddItemCommandParameter", typeof(object), typeof(DataGridControl));

        [Category("Common Properties")]
        public object AddItemCommandParameter
        {
            get { return (object)GetValue(AddItemCommandParameterProperty); }
            set { SetValue(AddItemCommandParameterProperty, value); }
        }
        #endregion

        #region IsLockFilter
        /// <summary>
        /// To hide control when data in datagrid is edited.
        /// </summary>
        public bool IsLockFilter
        {
            get { return (bool)GetValue(IsLockFilterProperty); }
            set { SetValue(IsLockFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableFilteringData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLockFilterProperty =
            DependencyProperty.Register("IsLockFilter", typeof(bool), typeof(DataGridControl), new PropertyMetadata(false, OnIsLockFilterValueChanged));

        private static void OnIsLockFilterValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                (d as DataGridControl).OnIsLockChange(bool.Parse(e.NewValue.ToString()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnIsLockFilterValueChanged " + ex.ToString());
            }
        }

        #endregion

        #region IsPaging
        /// <summary>
        /// To get , set value when row is being rollback but it is being error .
        /// </summary>
        public bool IsPaging
        {
            get { return (bool)GetValue(IsPagingProperty); }
            set { SetValue(IsPagingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableFilteringData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPagingProperty =
            DependencyProperty.Register("IsPaging", typeof(bool), typeof(DataGridControl), new PropertyMetadata(true));

        #endregion

        #region VisibilityGrouping
        /// <summary>
        /// To get , set value when row is being rollback but it is being error .
        /// </summary>
        public Visibility VisibilityGrouping
        {
            get { return (Visibility)GetValue(VisibilityGroupingProperty); }
            set { SetValue(VisibilityGroupingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableFilteringData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityGroupingProperty =
            DependencyProperty.Register("VisibilityGrouping", typeof(Visibility), typeof(DataGridControl), new PropertyMetadata(Visibility.Collapsed));

        #endregion

        #region IsGrouping
        /// <summary>
        /// To get , set value when users want to goup data on DataGrid.
        /// </summary>
        public bool IsGrouping
        {
            get { return (bool)GetValue(IsGroupingProperty); }
            set { SetValue(IsGroupingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableFilteringData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGroupingProperty =
            DependencyProperty.Register("IsGrouping", typeof(bool), typeof(DataGridControl), new PropertyMetadata(false));

        #endregion

        #region IsSelectedMutilItem
        /// <summary>
        /// To get , set value when users want to goup data on DataGrid.
        /// </summary>
        public bool IsSelectedMutilItem
        {
            get { return (bool)GetValue(IsSelectedMutilItemProperty); }
            set { SetValue(IsSelectedMutilItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableFilteringData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedMutilItemProperty =
            DependencyProperty.Register("IsSelectedMutilItem", typeof(bool), typeof(DataGridControl), new PropertyMetadata(false));

        #endregion

        #region IsColunmHidden
        /// <summary>
        /// Column will be hidden.
        /// </summary>
        public bool IsColunmHidden
        {
            get { return (bool)GetValue(IsColunmHiddenProperty); }
            set { SetValue(IsColunmHiddenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsColunmHidden.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsColunmHiddenProperty =
            DependencyProperty.Register("IsColunmHidden", typeof(bool), typeof(DataGridControl), new UIPropertyMetadata(false));

        #endregion

        #region NumberOfColumnsDisplayed
        public int NumberOfColumnsDisplayed
        {
            get { return (int)GetValue(NumberOfColumnsDisplayedProperty); }
            set { SetValue(NumberOfColumnsDisplayedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberOfItemsDisplayed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfColumnsDisplayedProperty =
            DependencyProperty.Register("NumberOfColumnsDisplayed", typeof(int), typeof(DataGridControl), new UIPropertyMetadata(5));

        #endregion

        #region CurrentColumnCollection
        /// <summary>
        ///To get all current column on datagrid.
        /// </summary>
        public ObservableCollection<string> CurrentColumnCollection
        {
            get { return (ObservableCollection<string>)GetValue(CurrentColumnCollectionProperty); }
            set { SetValue(CurrentColumnCollectionProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CurrentColumnCollections.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentColumnCollectionProperty =
            DependencyProperty.Register("CurrentColumnCollection", typeof(ObservableCollection<string>), typeof(DataGridControl), new UIPropertyMetadata(null));
        #endregion
        #endregion

    }

    public class RowModel
    {
        public int Index { get; set; }
        public object Content { get; set; }
        public bool IsVisible { get; set; }
    }
}
