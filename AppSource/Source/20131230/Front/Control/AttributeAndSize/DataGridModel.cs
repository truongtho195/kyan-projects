using System.Collections.ObjectModel;
using System.Linq;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class DataGridModel : ModelBase
    {
        #region Properties

        private string _rowHeaderName;
        /// <summary>
        /// Gets or sets the RowHeaderName.
        /// </summary>
        public string RowHeaderName
        {
            get { return _rowHeaderName; }
            set
            {
                if (_rowHeaderName != value)
                {
                    _rowHeaderName = value;
                    OnPropertyChanged(() => RowHeaderName);
                }
            }
        }

        private ObservableCollection<DataGridCellModel> _valueList = new ObservableCollection<DataGridCellModel>();
        /// <summary>
        /// Gets or sets the ValueList.
        /// </summary>
        public ObservableCollection<DataGridCellModel> ValueList
        {
            get { return _valueList; }
            set
            {
                if (_valueList != value)
                {
                    _valueList = value;
                    OnPropertyChanged(() => ValueList);
                }
            }
        }

        /// <summary>
        /// Gets the TotalItems.
        /// </summary>
        public decimal TotalItems
        {
            get { return ValueList.Sum(x => x.Value); }
        }

        private bool _isAddNewRow;
        /// <summary>
        /// Gets or sets the IsAddNewRow.
        /// </summary>
        public bool IsAddNewRow
        {
            get { return _isAddNewRow; }
            set
            {
                if (_isAddNewRow != value)
                {
                    _isAddNewRow = value;
                    OnPropertyChanged(() => IsAddNewRow);
                }
            }
        }

        private bool _isTotalRow;
        /// <summary>
        /// Gets or sets the IsTotalRow.
        /// </summary>
        public bool IsTotalRow
        {
            get { return _isTotalRow; }
            set
            {
                if (_isTotalRow != value)
                {
                    _isTotalRow = value;
                    OnPropertyChanged(() => IsTotalRow);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raise total items
        /// </summary>
        public void RaiseTotalItems()
        {
            OnPropertyChanged(() => TotalItems);
        }

        #endregion
    }
}