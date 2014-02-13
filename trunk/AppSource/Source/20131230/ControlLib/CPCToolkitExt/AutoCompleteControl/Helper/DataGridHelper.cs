using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CPCToolkitExtLibraries;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.ObjectModel;
using CPCToolkitExt.AutoCompleteControl;
using CPCToolkitExt.ComboBoxControl;

namespace CPCToolkitExt
{
    public class DataGridHelper : IDataGridHelper, INotifyPropertyChanged
    {
        #region Constructor

        #endregion

        #region Field
        protected DataGrid DataGirdControl;
        protected int ColunmPosistion = -1;
        protected bool IsKeyEnter = false;
        #endregion

        #region Events
        private void DataGrid_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.DataGirdControl.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        //Set ColunmPosistion when focus Control
                        if (ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow
                            && (e.OriginalSource is Control))
                        {
                            foreach (var item in this.CellCollection)
                            {
                                if ((e.OriginalSource as Control).Name.Equals(item.NameChildren)
                                    || ((e.OriginalSource as Control).Tag != null && (e.OriginalSource as Control).Tag.Equals(item.NameChildren)))
                                {
                                    this.ColunmPosistion = item.CellID;
                                    return;
                                }
                            }
                            if (!(e.OriginalSource is ListViewItem)
                                || !(e.OriginalSource is ListBoxItem))
                                this.ColunmPosistion = 0;
                        }
                    });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<DatagridControl_GotFocus>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void DataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (this.DataGirdControl.SelectedItem == null) return;
                if ((e.Key == Key.Enter || e.Key == Key.Tab) && ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow)
                    e.Handled = true;
                this.DataGirdControl.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        if ((e.Key == Key.Enter || e.Key == Key.Tab) && ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow)
                        {
                            ///Set Handled=true;
                            e.Handled = true;
                            DataGridRow dataGridRow = null;
                            //Set Colunm
                            CellModel cellModel = this.CellCollection.SingleOrDefault(x => x.CellID == this.ColunmPosistion);
                            if (cellModel == null || this.IsErrorCurrentCell(this.DataGirdControl.SelectedItem, cellModel)
                                || this.IsControlNotFocus(ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow, cellModel))
                            {
                                e.Handled = true;
                                return;
                            }
                            else if (cellModel.IsLastCell)
                            {
                                int selectedIndex = this.DataGirdControl.Items.IndexOf(this.DataGirdControl.SelectedItem);
                                if (selectedIndex == this.DataGirdControl.Items.Count - 1)
                                {
                                    e.Handled = true;
                                    return;
                                }
                                object selectedItemNext = this.DataGirdControl.Items[selectedIndex + 1];
                                dataGridRow = (DataGridRow)DataGirdControl.ItemContainerGenerator.ContainerFromItem(selectedItemNext);
                                this.ColunmPosistion = this.CellCollection[0].CellID;
                                e.Handled = false;
                                this.DataGirdControl.SelectedItem = selectedItemNext;
                            }
                            else
                            {
                                //Get DataGridRow
                                dataGridRow = (DataGridRow)DataGirdControl.ItemContainerGenerator.ContainerFromItem(this.DataGirdControl.SelectedItem);
                                if (this.CellCollection.IndexOf(cellModel) + 1 < this.CellCollection.Count)
                                    this.ColunmPosistion = this.CellCollection[this.CellCollection.IndexOf(cellModel) + 1].CellID;
                                else
                                    return;
                            }
                            ///Set focus when enter
                            if (this.DataGirdControl == null) return;
                            //Get DataGridCell
                            DataGridCell dataGridcell = AutoCompleteHelper.GetCell(this.DataGirdControl, dataGridRow, this.CellCollection.SingleOrDefault(x => x.CellID == this.ColunmPosistion).CellID);
                            if (dataGridcell == null) return;
                            //Get control in DataCell
                            Control control = this.GetControlInCell(this.CellCollection.SingleOrDefault(x => x.CellID == this.ColunmPosistion), dataGridRow);
                            //Set focus for control.
                            if (control != null)
                            {
                                if (control is AutoCompleteBox)
                                    (control as AutoCompleteBox).SetFocus();
                                else if (control is AutoCompleteTextBox)
                                    (control as AutoCompleteTextBox).SetFocus();
                                else if (control is AutoCompleteComboBox)
                                    (control as AutoCompleteComboBox).SetFocus();
                                else if (control is AutoCompleteTextBoxExt)
                                    (control as AutoCompleteTextBoxExt).SetFocus();
                                else if (control is ComboBoxDiscount)
                                    (control as ComboBoxDiscount).SetFocus();
                                else if (control is ComboBoxQuantity)
                                    (control as ComboBoxQuantity).SetFocus();
                                else
                                {
                                    //  if (control is ComboBox)
                                    //  {
                                    //      control.Dispatcher.BeginInvoke(
                                    //DispatcherPriority.Input,
                                    //(ThreadStart)delegate
                                    //{
                                    //    Keyboard.Focus(control);
                                    //    (control as ComboBox).IsDropDownOpen = true;
                                    //});
                                    //  }
                                    //  else
                                    control.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Input,
                                 (ThreadStart)delegate
                                 {
                                     Keyboard.Focus(control);
                                 });
                                }
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<DatagridControl_PreviewKeyDown>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        private void DataGridFocusRow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (this.DataGirdControl.SelectedItem == null) return;
                    if (ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow)
                    {
                        this.IsKeyEnter = true;
                        DataGridRow row = (DataGridRow)this.DataGirdControl.ItemContainerGenerator.ContainerFromItem(this.DataGirdControl.SelectedItem);
                        if (row == null) return;
                        System.Collections.ObjectModel.ReadOnlyCollection<System.Windows.Controls.ValidationError> errors = Validation.GetErrors(row);
                        if (errors == null) return;
                        if (errors.Count > 0)
                        {
                            e.Handled = true;
                            return;
                        }
                        if (this.DataGirdControl.SelectedIndex < this.DataGirdControl.Items.Count - 1)
                        {
                            object selectedItemNext = this.DataGirdControl.Items[this.DataGirdControl.SelectedIndex + 1];
                            DataGridRow dataGridRow = (DataGridRow)this.DataGirdControl.ItemContainerGenerator.ContainerFromItem(selectedItemNext);
                            dataGridRow.Focus();
                            this.DataGirdControl.SelectedItem = selectedItemNext;
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<DataGridFocusRow_PreviewKeyDown>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
                }
            }
        }
        private void DataGridFocusRow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsKeyEnter)
            {
                try
                {
                    this.DataGirdControl.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                if (e.AddedItems.Count == 0) return;
                                DataGridRow row = (DataGridRow)this.DataGirdControl.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]);
                                if (row == null) return;
                                DataGridCell cell = AutoCompleteHelper.GetCell(this.DataGirdControl, row, CellCollection[0].CellID);
                                if (cell == null) return;
                                DataTemplate editingTemplate = (cell.Content as ContentPresenter).ContentTemplate;
                                Control cellContent = editingTemplate.FindName(CellCollection[0].NameChildren, (cell.Content as ContentPresenter)) as Control;
                                if (cellContent != null)
                                    Keyboard.Focus(cellContent);
                                this.IsKeyEnter = false;
                            });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<DataGridFocusRow_SelectionChanged>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
                }
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Sets Focus Row
        /// </summary>
        /// <param name="dataGrid"></param>
        public void SetFocusDataGridRow(DataGrid dataGrid)
        {
            this.DataGirdControl = dataGrid;
            //To Register event for DataGrid
            dataGrid.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(DataGridFocusRow_PreviewKeyDown);
            dataGrid.SelectionChanged += new SelectionChangedEventHandler(DataGridFocusRow_SelectionChanged);
        }

        /// <summary>
        /// Sets Focus Cell
        /// </summary>
        /// <param name="dataGrid"></param>
        public void SetFocusDataGridCell(DataGrid dataGrid)
        {
            this.DataGirdControl = dataGrid;
            //To Register event for DataGrid
            dataGrid.GotFocus += new System.Windows.RoutedEventHandler(DataGrid_GotFocus);
            dataGrid.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(DataGrid_PreviewKeyDown);
        }

        /// <summary>
        /// Gets control in DataGridCell
        /// </summary>
        /// <param name="cellModel"></param>
        /// <param name="dataGridRow"></param>
        /// <returns></returns>
        private Control GetControlInCell(CellModel cellModel, DataGridRow dataGridRow)
        {
            try
            {
                DataGridCell cell = AutoCompleteHelper.GetCell(this.DataGirdControl, dataGridRow, cellModel.CellID);
                if (cell == null) return null;
                if (!(cell.Content is ContentPresenter)) return null;
                DataTemplate editingTemplate = (cell.Content as ContentPresenter).ContentTemplate;
                if (editingTemplate == null) return null;
                Control cellContent = editingTemplate.FindName(cellModel.NameChildren, (cell.Content as ContentPresenter)) as Control;
                return cellContent;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<GetControlInCell>>>>>>>>>>>>>>" + ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// Get error in DataGridCell
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool IsErrorCurrentCell(object selectedItem, CellModel model)
        {
            try
            {
                if (selectedItem == null) return false;
                DataGridRow dataGridRow = (DataGridRow)this.DataGirdControl.ItemContainerGenerator.ContainerFromItem(this.DataGirdControl.SelectedItem);
                if (this.DataGirdControl == null) return true;
                DataGridCell dataGridcell = AutoCompleteHelper.GetCell(this.DataGirdControl, dataGridRow, model.CellID);
                if (dataGridcell == null) return true;
                ///Gets error of control in DataGridCell
                DataTemplate editingTemplate = (dataGridcell.Content as ContentPresenter).ContentTemplate;
                Control cellContent = editingTemplate.FindName(model.NameChildren, (dataGridcell.Content as ContentPresenter)) as Control;
                if (cellContent == null) return false;
                System.Collections.ObjectModel.ReadOnlyCollection<System.Windows.Controls.ValidationError> errors = Validation.GetErrors(cellContent);
                if (errors == null) return true;
                if (errors.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<IsErrorCurrentCell>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Gets Control Custom in DataGridCell
        /// </summary>
        /// <param name="dataGridRow"></param>
        /// <param name="cellModel"></param>
        /// <returns></returns>
        private bool IsControlNotFocus(DataGridRow dataGridRow, CellModel cellModel)
        {
            try
            {
                if (dataGridRow == null) return false;
                Control control = this.GetControlInCell(cellModel, dataGridRow);
                if (control == null) return false;
                if (control is AutoCompleteBox)
                    return ((control as AutoCompleteBox).ISOpenPopup);
                else if (control is AutoCompleteTextBox)
                    return ((control as AutoCompleteTextBox).ISOpenPopup);
                else if (control is AutoCompleteComboBox)
                    return ((control as AutoCompleteComboBox).ISOpenPopup);
                else if (control is AutoCompleteTextBoxExt)
                    return ((control as AutoCompleteTextBoxExt).ISOpenPopup);
                else if (control is ComboBoxDiscount)
                    return ((control as ComboBoxDiscount).ISOpenPopup);
                else if (control is ComboBoxQuantity)
                    return ((control as ComboBoxQuantity).ISOpenPopup);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<IsControlNotFocus>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            return false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets CellCollection
        /// </summary>
        private CellCollection _cellCollection;
        public CellCollection CellCollection
        {
            get { return _cellCollection; }
            set
            {
                if (_cellCollection != value)
                {
                    _cellCollection = value;
                    RaisePropertyChanged(() => CellCollection);
                }
            }
        }
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = this.PropertyChanged;
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
        #endregion
    }
}
