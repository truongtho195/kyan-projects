using System;
using System.Reflection;
using System.Windows.Controls;

namespace CPC.Control
{
    public class DataGridEditable : DataGrid
    {
        private bool _isRowEditing = true;

        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            InvokeMethod(e.Row.Item, "BeginEdit", null);

            base.OnPreparingCellForEdit(e);
        }

        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                var content = e.Column.HeaderTemplate.LoadContent();
                if (content is TextBlock)
                    InvokeMethod(e.Row.Item, "CancelEdit", new object[] { (content as TextBlock).Tag.ToString() });
            }
            else
            {
                _isRowEditing = false;
                InvokeMethod(e.Row.Item, "EndEdit", null);
            }

            base.OnCellEditEnding(e);
        }

        protected override void OnRowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
                InvokeMethod(e.Row.Item, "CancelEdit", new object[] { string.Empty });
            else if (_isRowEditing)
                InvokeMethod(e.Row.Item, "EndEdit", null);
            else
            {
                _isRowEditing = true;
                e.Cancel = true;
            }

            base.OnRowEditEnding(e);
        }

        protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
        {
            if (e.RemovedCells.Count > 0)
                foreach (var cell in e.RemovedCells)
                    InvokeMethod(cell.Item, "EndEdit", null);

            base.OnSelectedCellsChanged(e);
        }

        private void InvokeMethod(object control, string methodName, object[] parameters)
        {
            Type type = control.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);
            if (methodInfo != null)
                methodInfo.Invoke(control, parameters);
        }
    }
}
