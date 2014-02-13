using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CPCToolkitExt
{
    public interface IDataGridHelper 
	{
        void SetFocusDataGridRow(DataGrid dataGrid);
        void SetFocusDataGridCell(DataGrid dataGrid);
	}
}
