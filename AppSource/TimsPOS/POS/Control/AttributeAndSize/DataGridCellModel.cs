using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class DataGridCellModel : ModelBase
    {
        #region Properties

        private string _cellResource = string.Empty;
        /// <summary>
        /// Gets or sets the CellResource.
        /// </summary>
        public string CellResource
        {
            get { return _cellResource; }
            set
            {
                if (_cellResource != value)
                {
                    _cellResource = value;
                    OnPropertyChanged(() => CellResource);
                }
            }
        }

        private int _value;
        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    this.IsDirty = true;
                    _value = value;
                    OnPropertyChanged(() => Value);
                }
            }
        }

        private string _attribute;
        /// <summary>
        /// Gets or sets the Attribute.
        /// </summary>
        public string Attribute
        {
            get { return _attribute; }
            set
            {
                if (_attribute != value)
                {
                    this.IsDirty = true;
                    _attribute = value;
                    OnPropertyChanged(() => Attribute);
                }
            }
        }

        private string _size;
        /// <summary>
        /// Gets or sets the Size.
        /// </summary>
        public string Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    this.IsDirty = true;
                    _size = value;
                    OnPropertyChanged(() => Size);
                }
            }
        }

        private string _barcode = string.Empty;
        /// <summary>
        /// Gets or sets the Barcode.
        /// </summary>
        public string Barcode
        {
            get { return _barcode; }
            set
            {
                if (_barcode != value)
                {
                    _barcode = value;
                    OnPropertyChanged(() => Barcode);
                }
            }
        }

        private string _partNumber = string.Empty;
        /// <summary>
        /// Gets or sets the PartNumber.
        /// </summary>
        public string PartNumber
        {
            get { return _partNumber; }
            set
            {
                if (_partNumber != value)
                {
                    _partNumber = value;
                    OnPropertyChanged(() => PartNumber);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// Turn on IsNew
        /// </summary>
        public DataGridCellModel()
        {
            this.IsNew = true;
        }

        #endregion
    }
}
