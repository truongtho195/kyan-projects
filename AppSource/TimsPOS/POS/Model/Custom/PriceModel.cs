using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class PriceModel : ModelBase
    {
        #region Contructors

        public PriceModel()
        {
            _isNew = true;
        }

        #endregion

        #region Properties

        #region Id

        private short _id;
        /// <summary>
        /// Gets or sets price's Id.
        /// </summary>
        public short Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _isDirty = true;
                    _id = value;
                    OnPropertyChanged(() => Id);
                }
            }
        }

        #endregion

        #region Name

        private string _name;
        /// <summary>
        /// Gets or sets price's name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _isDirty = true;
                    _name = value;
                    OnPropertyChanged(() => Name);
                    OnNameChanged();
                }
            }
        }

        #endregion

        #region MarkDown

        private decimal _markDown;
        /// <summary>
        /// Gets or sets price's markDown.
        /// </summary>
        public decimal MarkDown
        {
            get
            {
                return _markDown;
            }
            set
            {
                if (_markDown != value)
                {
                    _isDirty = true;
                    _markDown = value;
                    OnPropertyChanged(() => MarkDown);
                }
            }
        }

        #endregion

        #region Currency

        private string _currency;
        /// <summary>
        /// Gets or sets price's currency.
        /// </summary>
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                if (_currency != value)
                {
                    _isDirty = true;
                    _currency = value;
                    OnPropertyChanged(() => Currency);
                }
            }
        }

        #endregion

        #region PriceSchemaCollection

        private CollectionBase<PriceModel> _priceSchemaCollection;
        /// <summary>
        /// Gets or sets price collection.
        /// </summary>
        public CollectionBase<PriceModel> PriceSchemaCollection
        {
            get
            {
                return _priceSchemaCollection;
            }
            set
            {
                if (_priceSchemaCollection != value)
                {
                    _priceSchemaCollection = value;
                    OnPropertyChanged(() => PriceSchemaCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnNameChanged

        /// <summary>
        /// Occur after Name property changed.
        /// </summary>
        private void OnNameChanged()
        {
            if (_priceSchemaCollection != null)
            {
                PriceModel price = _priceSchemaCollection.FirstOrDefault(x => x.Id == this.Id);
                if (price != null)
                {
                    price.SetName(_name);
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods.

        #region SetName

        /// <summary>
        /// Set value for Name property.
        /// </summary>
        private void SetName(string name)
        {
            if (_name != name)
            {
                _name = name;
                OnPropertyChanged(() => Name);
            }
        }

        #endregion

        #endregion

        #region Methods

        #region ShallowClone

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        /// <returns>A shallow copy of this object.</returns>
        public PriceModel ShallowClone()
        {
            return (PriceModel)this.MemberwiseClone();
        }

        #endregion

        #endregion
    }
}
