using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPCToolkitExt.TextBoxControl;
using Xceed.Wpf.Toolkit;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for DataGridCanAddColumnRowControl.xaml
    /// </summary>
    public partial class DataGridCanAddColumnRowControl : UserControl, INotifyPropertyChanged
    {
        #region Defines

        private bool _isDeleting;

        private string _messageErrorSize = "Size is existed";
        private string _messageErrorAttribute = "Attribute is existed";
        private string _messageNullSize = "Size is required";
        private string _messageNullAttribute = "Attribute is required";

        #endregion

        #region Properties

        private ObservableCollection<DataGridModel> _dataRowList = new ObservableCollection<DataGridModel>();
        /// <summary>
        /// Gets or sets the DataRowList.
        /// </summary>
        public ObservableCollection<DataGridModel> DataRowList
        {
            get { return _dataRowList; }
            set
            {
                if (_dataRowList != value)
                {
                    _dataRowList = value;
                    OnPropertyChanged(() => DataRowList);
                }
            }
        }

        private ObservableCollection<string> _columnHeaderNameList = new ObservableCollection<string>();
        /// <summary>
        /// Gets or sets the ColumnHeaderNameList.
        /// </summary>
        public ObservableCollection<string> ColumnHeaderNameList
        {
            get { return _columnHeaderNameList; }
            set
            {
                if (_columnHeaderNameList != value)
                {
                    _columnHeaderNameList = value;
                    OnPropertyChanged(() => ColumnHeaderNameList);
                }
            }
        }

        /// <summary>
        /// Gets or sets the TotalRow
        /// </summary>
        public DataGridModel TotalRow { get; set; }

        #endregion

        #region Dependency Properties

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DataGridCanAddColumnRowControl).SetValueItemsSource();
        }

        public CollectionBase<DataGridCellModel> ItemsSource
        {
            get { return (CollectionBase<DataGridCellModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(CollectionBase<DataGridCellModel>),
            typeof(DataGridCanAddColumnRowControl), new UIPropertyMetadata(OnItemsSourceChanged));

        #endregion

        #region Constructors

        // Default constructor
        public DataGridCanAddColumnRowControl()
        {
            InitializeComponent();
            Init();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initial default properties
        /// </summary>
        private void Init()
        {
            // Insert AddNew row to datagrid
            DataRowList.Add(new DataGridModel { IsAddNewRow = true });

            // Insert Total row to datagrid
            TotalRow = new DataGridModel { IsTotalRow = true };
            DataRowList.Add(TotalRow);
        }

        /// <summary>
        /// Insert new column to datagrid
        /// </summary>
        private void AddColumnToDataGrid(int columnIndex)
        {
            #region Create header template

            // Create the "Delete Column" menu item.
            MenuItem mniDeleteColumn = new MenuItem { Header = "Remove this size" };
            mniDeleteColumn.Click += new RoutedEventHandler(mniDeleteColumn_Click);
            mniDeleteColumn.Tag = columnIndex;

            // Create context menu
            ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
            contextMenu.Items.Add(mniDeleteColumn);

            // Create header name binding
            Binding headerNameBinding = new Binding(GetPropertyName(() => ColumnHeaderNameList) + "[" + columnIndex + "]");
            headerNameBinding.ElementName = "ucDataGridCanAddColumnRow";
            headerNameBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

            // Create CPC TextBoxControl
            FrameworkElementFactory txtHeaderName = new FrameworkElementFactory(typeof(CPCToolkitExt.TextBoxControl.TextBox));
            txtHeaderName.SetBinding(CPCToolkitExt.TextBoxControl.TextBox.TextProperty, headerNameBinding);
            txtHeaderName.SetValue(CPCToolkitExt.TextBoxControl.TextBox.ContextMenuProperty, contextMenu);
            //txtHeaderName.SetValue(CPCToolkitExt.TextBoxControl.TextBox.HeightProperty, (double)28);
            txtHeaderName.SetValue(CPCToolkitExt.TextBoxControl.TextBox.MinWidthProperty, (double)50);
            txtHeaderName.SetValue(CPCToolkitExt.TextBoxControl.TextBox.StyleProperty, this.FindResource("TextBoxRowHeaderName"));
            txtHeaderName.AddHandler(CPCToolkitExt.TextBoxControl.TextBox.LostFocusEvent, new RoutedEventHandler(txtColumnHeaderName_LostFocus));

            // Create data template
            DataTemplate headerTemplate = new DataTemplate(typeof(CPCToolkitExt.TextBoxControl.TextBox));
            headerTemplate.VisualTree = txtHeaderName;

            #endregion

            #region Create triggers

            #region Create IsAddNewRow trigger

            // Create trigger binding
            Binding bindingIsAddNewRow = new Binding("Item." + GetPropertyName(() => TotalRow.IsAddNewRow));
            bindingIsAddNewRow.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1);

            // Create setter
            Setter setterIsAddNewRow = new Setter(VisibilityProperty, Visibility.Collapsed);

            // Create trigger
            DataTrigger triggerIsAddNewRow = new DataTrigger();
            triggerIsAddNewRow.Binding = bindingIsAddNewRow;
            triggerIsAddNewRow.Value = true;
            triggerIsAddNewRow.Setters.Add(setterIsAddNewRow);

            #endregion

            #region Create IsTotalRow trigger

            // Create trigger binding
            Binding bindingIsTotalRow = new Binding("Item." + GetPropertyName(() => TotalRow.IsTotalRow));
            bindingIsTotalRow.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1);

            // Create setter
            Setter setterIsTotalRow = new Setter(VisibilityProperty, Visibility.Collapsed, "txtValue");
            Setter setterIsTotalRow2 = new Setter(VisibilityProperty, Visibility.Visible, "txtblTotalRowValue");

            // Create trigger
            DataTrigger triggerIsTotalRow = new DataTrigger();
            triggerIsTotalRow.Binding = bindingIsTotalRow;
            triggerIsTotalRow.Value = true;
            triggerIsTotalRow.Setters.Add(setterIsTotalRow);
            triggerIsTotalRow.Setters.Add(setterIsTotalRow2);

            #endregion

            #region Create IsTemporary trigger

            // Create trigger binding
            Binding bindingIsTemporary = new Binding(GetPropertyName(() => TotalRow.ValueList) + "[" + columnIndex + "].IsTemporary");

            // Create setter
            Setter setterIsTemporary = new Setter(VisibilityProperty, Visibility.Collapsed);

            // Create trigger
            DataTrigger triggerIsTemporary = new DataTrigger();
            triggerIsTemporary.Binding = bindingIsTemporary;
            triggerIsTemporary.Value = true;
            triggerIsTemporary.Setters.Add(setterIsTemporary);

            #endregion

            #endregion

            #region Create cell template and trigger

            // Create cell value binding
            Binding bindingCellTemplate = new Binding(GetPropertyName(() => TotalRow.ValueList) + "[" + columnIndex + "].Value");
            bindingCellTemplate.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingCellTemplate.Mode = BindingMode.OneWay;

            // Create TextBlock
            FrameworkElementFactory txtblValue = new FrameworkElementFactory(typeof(TextBlock), "txtblValue");
            txtblValue.SetBinding(TextBlock.TextProperty, bindingCellTemplate);
            txtblValue.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right);

            // Create data template
            DataTemplate valueCellTemplate = new DataTemplate(typeof(TextBlock));
            valueCellTemplate.VisualTree = txtblValue;
            valueCellTemplate.Triggers.Add(triggerIsAddNewRow);
            valueCellTemplate.Triggers.Add(triggerIsTemporary);

            #endregion

            #region Create cell editing template

            // Create cell value binding
            Binding bindingCellEditingTemplate = new Binding(GetPropertyName(() => TotalRow.ValueList) + "[" + columnIndex + "].Value");
            bindingCellEditingTemplate.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
            bindingCellEditingTemplate.Mode = BindingMode.TwoWay;

            // Creat TextBlock to show data when double click on total row
            FrameworkElementFactory txtblTotalRowValue = new FrameworkElementFactory(typeof(TextBlock), "txtblTotalRowValue");
            txtblTotalRowValue.SetBinding(TextBlock.TextProperty, bindingCellTemplate);
            txtblTotalRowValue.SetValue(TextBlock.VisibilityProperty, Visibility.Collapsed);
            txtblTotalRowValue.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right);

            // Create CPC TextBoxNumeric
            FrameworkElementFactory txtValue = new FrameworkElementFactory(typeof(TextBoxNumeric), "txtValue");
            txtValue.SetBinding(TextBoxNumeric.ValueDependencyProperty, bindingCellEditingTemplate);
            txtValue.AddHandler(TextBoxNumeric.PreviewKeyDownEvent, new KeyEventHandler(txtValue_PreviewKeyDown));
            txtValue.AddHandler(TextBoxNumeric.LostFocusEvent, new RoutedEventHandler(txtValue_LostFocus));

            // Create grid to contain CPC TextBoxNumeric and TextBlock
            FrameworkElementFactory gridCellEditing = new FrameworkElementFactory(typeof(Grid));
            gridCellEditing.AppendChild(txtblTotalRowValue);
            gridCellEditing.AppendChild(txtValue);

            // Create data template
            DataTemplate valueCellEditingTemplate = new DataTemplate(typeof(Grid));
            valueCellEditingTemplate.VisualTree = gridCellEditing;
            valueCellEditingTemplate.Triggers.Add(triggerIsAddNewRow);
            valueCellEditingTemplate.Triggers.Add(triggerIsTotalRow);

            #endregion

            #region Insert value for row

            foreach (DataGridModel dataGridModel in DataRowList)
            {
                // Create a new cell
                DataGridCellModel dataGridCellModel = new DataGridCellModel
                {
                    CellResource = Guid.NewGuid().ToString(),
                    Attribute = dataGridModel.RowHeaderName,
                    Size = ColumnHeaderNameList[columnIndex]
                };

                // Add new cell to datagrid
                dataGridModel.ValueList.Add(dataGridCellModel);

                if (this.IsLoaded && !dataGridModel.IsAddNewRow && !dataGridModel.IsTotalRow && !_isDeleting)
                {
                    // Add new cell to collection
                    ItemsSource.Add(dataGridCellModel);
                }
            }

            // Update total row value
            if (this.IsLoaded)
                TotalRow.ValueList[columnIndex].Value = DataRowList.Where(x => !x.IsTotalRow && !x.IsAddNewRow).Sum(x => x.ValueList[columnIndex].Value);

            #endregion

            #region Create DataGrid column

            // Create new DataGridColumn
            DataGridTemplateColumn dataGridColumn = new DataGridTemplateColumn();

            // Set header template
            dataGridColumn.HeaderTemplate = headerTemplate;

            // Set cell template
            dataGridColumn.CellTemplate = valueCellTemplate;

            // Set cell editing template
            dataGridColumn.CellEditingTemplate = valueCellEditingTemplate;

            // Set DataGridCell style
            dataGridColumn.SetValue(DataGridColumn.CellStyleProperty, this.FindResource("DataGridCell_TotalRow_Attribute"));

            // Insert column to datagrid
            this.dataGrid.Columns.Insert(columnIndex, dataGridColumn);

            #endregion
        }

        /// <summary>
        /// Insert new row to datagrid
        /// </summary>
        private void AddRowToDataGrid(string rowHeaderName)
        {
            // Get row index to insert new row
            int rowIndex = this.DataRowList.Count - 2;

            // Create new DataGridModel
            DataGridModel dataGridModel = new DataGridModel();

            // Set row header name
            dataGridModel.RowHeaderName = rowHeaderName;

            // Set default value by number of columns
            for (int i = 0; i < ColumnHeaderNameList.Count; i++)
            {
                // Create a new cell
                DataGridCellModel dataGridCellModel = new DataGridCellModel
                {
                    CellResource = Guid.NewGuid().ToString(),
                    Attribute = dataGridModel.RowHeaderName,
                    Size = ColumnHeaderNameList[i]
                };

                // Add new cell to datagrid
                dataGridModel.ValueList.Add(dataGridCellModel);

                if (this.IsLoaded && !dataGridModel.IsAddNewRow && !dataGridModel.IsTotalRow)
                {
                    // Add new cell to collection
                    ItemsSource.Add(dataGridCellModel);
                }
            }

            // Insert new row to datagrid
            DataRowList.Insert(rowIndex, dataGridModel);
        }

        /// <summary>
        /// Update source cell value when cell value changed
        /// </summary>
        private void OnCellValueChanged()
        {
            // Commite datagrid row
            dataGrid.CommitEdit();

            // Get datagrid model object
            DataGridModel dataGridModel = dataGrid.CurrentItem as DataGridModel;

            // Sum columns
            dataGridModel.RaiseTotalItems();

            // Get column index
            int columnIndex = dataGrid.CurrentCell.Column.DisplayIndex;

            // Resum total value
            TotalRow.ValueList[columnIndex].Value = DataRowList.Where(x => !x.IsTotalRow && !x.IsAddNewRow).Sum(x => x.ValueList[columnIndex].Value);

            // Sum total grid
            TotalRow.RaiseTotalItems();

            // Update cell value
            DataGridCellModel cellModel = dataGridModel.ValueList[columnIndex];
            DataGridCellModel sourceCellModel = ItemsSource.SingleOrDefault(x => x.CellResource.Equals(cellModel.CellResource));
            if (sourceCellModel != null)
                sourceCellModel.Value = cellModel.Value;

            // Add new item when value is edited
            if (cellModel.IsDirty && cellModel.IsTemporary)
            {
                cellModel.IsTemporary = false;

                ItemsSource.Add(cellModel);
            }
        }

        /// <summary>
        /// Check column header name is exist
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        private bool IsColumnHeaderNameExist(string headerName, string currentHeaderName = "")
        {
            bool result = true;

            // Remove all space in string to compare
            string validHeaderName = headerName.Trim().ToLower().Replace(" ", "");

            if (!string.IsNullOrWhiteSpace(validHeaderName))
            {
                List<string> headerNames = ColumnHeaderNameList.ToList();

                // Remove current column header name
                if (!string.IsNullOrWhiteSpace(currentHeaderName))
                    headerNames.Remove(currentHeaderName);

                result = headerNames.Select(x => x.Trim().ToLower().Replace(" ", "")).Contains(validHeaderName);
            }

            if (result)
                System.Windows.MessageBox.Show(_messageErrorSize);

            return result;
        }

        /// <summary>
        /// Check row header name is exist
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        private bool IsRowHeaderNameExist(string headerName, string currentHeaderName = "")
        {
            bool result = true;

            // Remove all space in string to compare
            string validHeaderName = headerName.Trim().ToLower().Replace(" ", "");

            if (!string.IsNullOrWhiteSpace(validHeaderName))
            {
                List<string> headerNames = DataRowList.Where(x => !string.IsNullOrWhiteSpace(x.RowHeaderName)).
                    Select(x => x.RowHeaderName).ToList();

                // Remove current column header name
                if (!string.IsNullOrWhiteSpace(currentHeaderName))
                    headerNames.Remove(currentHeaderName);

                result = headerNames.Select(x => x.Trim().ToLower().Replace(" ", "")).Contains(validHeaderName);
            }

            if (result)
                System.Windows.MessageBox.Show(_messageErrorAttribute);

            return result;
        }

        /// <summary>
        /// Set value for ItemsSource
        /// </summary>
        private void SetValueItemsSource()
        {
            // Group items by row
            IEnumerable<IGrouping<string, DataGridCellModel>> rowGroups = ItemsSource.GroupBy(x => x.Attribute);

            // Add rows to datagrid
            foreach (IGrouping<string, DataGridCellModel> rowGroup in rowGroups)
            {
                // Get row index
                int rowIndex = DataRowList.Count - 2;

                // Create new row
                DataGridModel rowModel = new DataGridModel();
                rowModel.RowHeaderName = rowGroup.Key;

                // Insert new row before AddNew and Total row
                DataRowList.Insert(rowIndex, rowModel);
            }

            // Add column header names
            ColumnHeaderNameList = new ObservableCollection<string>(ItemsSource.GroupBy(x => x.Size).Select(x => x.Key));

            // Add columns to datagrid
            for (int i = 0; i < ColumnHeaderNameList.Count; i++)
                AddColumnToDataGrid(i);

            // Reupdate cell value
            for (int columnIndex = 0; columnIndex < ColumnHeaderNameList.Count; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < DataRowList.Count - 2; rowIndex++)
                {
                    // Get cell to binding
                    DataGridCellModel cellModel = DataRowList[rowIndex].ValueList[columnIndex];

                    // Get source cell from items source
                    DataGridCellModel sourceCellModel = ItemsSource.
                        FirstOrDefault(x => x.Attribute.Equals(cellModel.Attribute) && x.Size.Equals(cellModel.Size));

                    if (sourceCellModel != null)
                    {
                        // Binding source cell with datagrid cell
                        DataRowList[rowIndex].ValueList[columnIndex] = ItemsSource.
                            FirstOrDefault(x => x.Attribute.Equals(cellModel.Attribute) && x.Size.Equals(cellModel.Size));
                    }
                    else
                    {
                        // Set datagrid cell by new a source cell
                        DataRowList[rowIndex].ValueList[columnIndex] = new DataGridCellModel
                        {
                            CellResource = Guid.NewGuid().ToString(),
                            Attribute = cellModel.Attribute,
                            Size = cellModel.Size,
                            IsTemporary = true,
                            IsDirty = false
                        };
                    }
                }

                // Update total value
                TotalRow.ValueList[columnIndex].Value = DataRowList.Where(x => !x.IsTotalRow && !x.IsAddNewRow).Sum(x => x.ValueList[columnIndex].Value);
            }
        }

        /// <summary>
        /// Get name of property to binding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Add new column to datagrid when press enter in ClickToAdd textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAddColumn_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                // Get textbox object
                WatermarkTextBox txtHeader = sender as WatermarkTextBox;

                if (!string.IsNullOrWhiteSpace(txtHeader.Text))
                {
                    if (!IsColumnHeaderNameExist(txtHeader.Text))
                    {
                        // Add new column name to header list
                        ColumnHeaderNameList.Add(txtHeader.Text);

                        // Get column index to insert new column
                        int columnIndex = this.dataGrid.Columns.Count - 2;

                        // Add new column to datagrid
                        AddColumnToDataGrid(columnIndex);
                    }
                    else
                        e.Handled = true;

                    // Clear text
                    txtHeader.Clear();
                }
            }
        }

        /// <summary>
        /// Clear text in ClickToAdd textbox when it lost focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAddColumn_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get textbox object
            WatermarkTextBox txtHeader = sender as WatermarkTextBox;

            if (!string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                if (!IsColumnHeaderNameExist(txtHeader.Text))
                {
                    // Add new column name to header list
                    ColumnHeaderNameList.Add(txtHeader.Text);

                    // Get column index to insert new column
                    int columnIndex = this.dataGrid.Columns.Count - 2;

                    // Add new column to datagrid
                    AddColumnToDataGrid(columnIndex);
                }

                // Clear text
                txtHeader.Clear();
            }
        }

        /// <summary>
        /// Add new row to datagrid when press enter in ClickToAdd textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAddRow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                // Get textbox object
                WatermarkTextBox txtHeader = sender as WatermarkTextBox;

                if (!string.IsNullOrWhiteSpace(txtHeader.Text))
                {
                    if (!IsRowHeaderNameExist(txtHeader.Text))
                    {
                        // Add new row to datagrid
                        AddRowToDataGrid(txtHeader.Text);
                    }
                    else
                        e.Handled = true;

                    // Clear text
                    txtHeader.Clear();
                }
            }
        }

        /// <summary>
        /// Clear text in ClickToAdd textbox when it lost focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAddRow_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get textbox object
            WatermarkTextBox txtHeader = sender as WatermarkTextBox;

            if (!string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                if (!IsRowHeaderNameExist(txtHeader.Text))
                {
                    // Add new row to datagrid
                    AddRowToDataGrid(txtHeader.Text);
                }

                // Clear text
                txtHeader.Clear();
            }
        }

        /// <summary>
        /// Update total value when cell value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Tab))
                OnCellValueChanged();
        }

        /// <summary>
        /// Update total value when cell value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtValue_LostFocus(object sender, RoutedEventArgs e)
        {
            OnCellValueChanged();
        }

        /// <summary>
        /// Delete selected row when click menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mniDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Get datagrid model object
            DataGridModel dataGridModel = dataGrid.CurrentItem as DataGridModel;

            // Can not delete the first row
            if (DataRowList.IndexOf(dataGridModel) > 0)
            {
                // Remove product
                foreach (DataGridCellModel dataGridCellModel in dataGridModel.ValueList)
                    ItemsSource.Remove(dataGridCellModel);

                // Remove selected row
                DataRowList.Remove(dataGridModel);
            }
        }

        /// <summary>
        /// Delete selected column when click menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mniDeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            // Get menu item object
            MenuItem menuItem = sender as MenuItem;

            // Get column index
            int columnIndex = (int)menuItem.Tag;

            // Can not delete the first column
            if (columnIndex > 0)
            {
                // Remove product
                IEnumerable<IGrouping<string, DataGridCellModel>> groupSizes = ItemsSource.GroupBy(x => x.Size);
                foreach (DataGridCellModel dataGridCellModel in groupSizes.ElementAt(columnIndex))
                    ItemsSource.Remove(dataGridCellModel);

                // Remove all column from column index to last
                for (int i = columnIndex; i < ColumnHeaderNameList.Count; i++)
                {
                    // Get datagrid column
                    DataGridTemplateColumn dataGridColumn = dataGrid.Columns[columnIndex] as DataGridTemplateColumn;

                    // Remove column template
                    dataGridColumn.HeaderTemplate = null;
                    dataGridColumn.CellTemplate = null;
                    dataGridColumn.CellEditingTemplate = null;
                    dataGridColumn.CellStyle = null;

                    // Remove column at datagrid
                    dataGrid.Columns.RemoveAt(columnIndex);
                }

                foreach (DataGridModel dataGridModel in DataRowList)
                {
                    // Remove column value at each row, include total row
                    dataGridModel.ValueList.RemoveAt(columnIndex);

                    // Sum columns, include total row
                    dataGridModel.RaiseTotalItems();
                }

                // Remove column header name in list
                ColumnHeaderNameList.RemoveAt(columnIndex);

                _isDeleting = true;
                for (int i = columnIndex; i < ColumnHeaderNameList.Count; i++)
                {
                    // Reinsert column to datagrid to update binding
                    AddColumnToDataGrid(i);
                }
                _isDeleting = false;
            }
        }

        /// <summary>
        /// Update product size when size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtColumnHeaderName_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get textbox object
            CPCToolkitExt.TextBoxControl.TextBox txtHeader = sender as CPCToolkitExt.TextBoxControl.TextBox;

            // Get menu item object
            MenuItem menuItem = txtHeader.ContextMenu.Items[0] as MenuItem;

            // Get column index
            int columnIndex = (int)menuItem.Tag;

            if (string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                System.Windows.MessageBox.Show(_messageNullSize);

                // If column header name is exist, rollback value
                txtHeader.Text = ColumnHeaderNameList[columnIndex];
            }
            else if (IsColumnHeaderNameExist(txtHeader.Text, ColumnHeaderNameList[columnIndex]))
            {
                // If column header name is exist, rollback value
                txtHeader.Text = ColumnHeaderNameList[columnIndex];
            }
            else
            {
                BindingExpression bindingExpression = txtHeader.GetBindingExpression(CPCToolkitExt.TextBoxControl.TextBox.TextProperty);
                bindingExpression.UpdateSource();

                // Update product size
                IEnumerable<IGrouping<string, DataGridCellModel>> groupSizes = ItemsSource.GroupBy(x => x.Size);
                foreach (DataGridCellModel groupSize in groupSizes.ElementAt(columnIndex))
                    groupSize.Size = txtHeader.Text;
            }
        }

        /// <summary>
        /// Update product attribute when attribute changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRowHeaderName_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get textbox object
            CPCToolkitExt.TextBoxControl.TextBox txtHeader = sender as CPCToolkitExt.TextBoxControl.TextBox;

            // Get datagrid model object
            DataGridModel dataGridModel = txtHeader.Tag as DataGridModel;

            if (string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                System.Windows.MessageBox.Show(_messageNullAttribute);

                // If row header name is exist, rollback value
                txtHeader.Text = dataGridModel.RowHeaderName;
            }
            else if (IsRowHeaderNameExist(txtHeader.Text, dataGridModel.RowHeaderName))
            {
                // If row header name is exist, rollback value
                txtHeader.Text = dataGridModel.RowHeaderName;
            }
            else
            {
                BindingExpression bindingExpression = txtHeader.GetBindingExpression(CPCToolkitExt.TextBoxControl.TextBox.TextProperty);
                bindingExpression.UpdateSource();

                // Update product attribute
                foreach (DataGridCellModel dataGridCellModel in dataGridModel.ValueList)
                    dataGridCellModel.Attribute = txtHeader.Text;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as System.Reflection.PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        #endregion
    }
}
