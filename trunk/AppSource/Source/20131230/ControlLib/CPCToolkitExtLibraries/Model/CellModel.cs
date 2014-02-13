using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPCToolkitExtLibraries;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class CellModel : ToolkitModelBase
    {
        #region Properties

        private int _cellID = 0;
        /// <summary>
        /// Gets or sets the CellID value.
        /// </summary>
        public int CellID
        {
            get { return _cellID; }
            set
            {
                if (_cellID != value)
                {
                    _cellID = value;
                    RaisePropertyChanged(() => CellID);
                }
            }
        }

        private bool _isFocused = false;
        /// <summary>
        /// Gets or sets the IsFocused value.
        /// </summary>
        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;
                    RaisePropertyChanged(() => IsFocused);
                }
            }
        }

        private bool _isLastCell = false;
        /// <summary>
        /// Gets or sets the IsLastCell value.
        /// </summary>
        public bool IsLastCell
        {
            get { return _isLastCell; }
            set
            {
                if (_isLastCell != value)
                {
                    _isLastCell = value;
                    RaisePropertyChanged(() => IsLastCell);
                }
            }
        }

        private string _nameChildren = string.Empty;
        /// <summary>
        /// Gets or sets the PropertyChildren value.
        /// </summary>
        public string NameChildren
        {
            get { return _nameChildren; }
            set
            {
                if (_nameChildren != value)
                {
                    _nameChildren = value;
                    RaisePropertyChanged(() => NameChildren);
                }
            }
        }
        #endregion
    }
}
