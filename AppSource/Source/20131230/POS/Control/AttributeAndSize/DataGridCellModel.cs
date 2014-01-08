using System.Collections.Generic;
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

        private decimal _value;
        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public decimal Value
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
                    _partNumber = value;
                    OnPropertyChanged(() => PartNumber);
                }
            }
        }

        private string _alu;
        /// <summary>
        /// Gets or sets the ALU.
        /// </summary>
        public string ALU
        {
            get { return _alu; }
            set
            {
                if (_alu != value)
                {
                    this.IsDirty = true;
                    _alu = value;
                    OnPropertyChanged(() => ALU);
                }
            }
        }

        private decimal _regularPrice;
        /// <summary>
        /// Gets or sets the RegularPrice.
        /// </summary>
        public decimal RegularPrice
        {
            get { return _regularPrice; }
            set
            {
                if (_regularPrice != value)
                {
                    this.IsDirty = true;
                    _regularPrice = value;
                    OnPropertyChanged(() => RegularPrice);
                }
            }
        }

        public List<ComboItem> ValueList { get; set; }

        private bool _isDuplicateBarcode;
        /// <summary>
        /// Gets or sets the IsDuplicateBarcode.
        /// </summary>
        public bool IsDuplicateBarcode
        {
            get { return _isDuplicateBarcode; }
            set
            {
                if (_isDuplicateBarcode != value)
                {
                    _isDuplicateBarcode = value;
                    OnPropertyChanged(() => IsDuplicateBarcode);
                }
            }
        }

        private bool _isDuplicateALU;
        /// <summary>
        /// Gets or sets the IsDuplicateALU.
        /// </summary>
        public bool IsDuplicateALU
        {
            get { return _isDuplicateALU; }
            set
            {
                if (_isDuplicateALU != value)
                {
                    _isDuplicateALU = value;
                    OnPropertyChanged(() => IsDuplicateALU);
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

        #region Public Methods

        /// <summary>
        /// <param>Public Method</param>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        #endregion
    }
}