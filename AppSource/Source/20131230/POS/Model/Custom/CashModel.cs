using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class CashModel : ModelBase
    {
        #region Contructors

        #endregion

        #region Properties

        #region Id

        private short _id;
        /// <summary>
        /// Gets or sets Id.
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
                    PropertyChangedCompleted(() => Id);
                }
            }
        }

        #endregion

        #region Value

        private decimal _value;
        /// <summary>
        /// Gets or sets Value.
        /// </summary>
        public decimal Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _isDirty = true;
                    _value = value;
                    OnPropertyChanged(() => Value);
                    PropertyChangedCompleted(() => Value);
                }
            }
        }

        #endregion

        #region Count

        private short _count;
        /// <summary>
        /// Gets or sets Count.
        /// </summary>
        public short Count
        {
            get
            {
                return _count;
            }
            set
            {
                if (_count != value)
                {
                    _isDirty = true;
                    _count = value;
                    OnPropertyChanged(() => Count);
                    PropertyChangedCompleted(() => Count);
                }
            }
        }

        #endregion

        #region Total

        private decimal _total;
        /// <summary>
        /// Gets or sets Total.
        /// </summary>
        public decimal Total
        {
            get
            {
                return _total;
            }
            set
            {
                if (_total != value)
                {
                    _isDirty = true;
                    _total = value;
                    OnPropertyChanged(() => Total);
                    PropertyChangedCompleted(() => Total);
                }
            }
        }

        #endregion

        #region Index

        private int _index;
        /// <summary>
        /// Gets or sets Index.
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index != value)
                {
                    _isDirty = true;
                    _index = value;
                    OnPropertyChanged(() => Index);
                    PropertyChangedCompleted(() => Index);
                }
            }
        }

        #endregion

        #endregion

        #region Property Changed Methods

        protected override void PropertyChangedCompleted(string propertyName)
        {
            switch (propertyName)
            {
                case "Count":
                    Total = _value * _count;
                    break;
            }
        }

        #endregion
    }
}
