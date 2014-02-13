using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using CPCToolkitExtLibraries;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class QuantityModel : ToolkitModelBase
    {
        #region Properties

        /// <summary>
        ///Value
        /// </summary>
        private decimal _quantity = 0;
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    RaisePropertyChanged(() => Quantity);
                }
            }
        }

        private decimal _standardQuantity = 0;
        public decimal StandardQuantity
        {
            get { return _standardQuantity; }
            set
            {
                if (_standardQuantity != value)
                {
                    _standardQuantity = value;
                    RaisePropertyChanged(() => StandardQuantity);
                }
            }
        }

        /// <summary>
        ///Type
        /// </summary>
        private int _unit = -1;
        public int Unit
        {
            get { return _unit; }
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    RaisePropertyChanged(() => Unit);
                }
            }
        }
        private decimal _rate = 0;
        public decimal Rate
        {
            get { return _rate; }
            set
            {
                if (_rate != value)
                {
                    _rate = value;
                    RaisePropertyChanged(() => Rate);
                }
            }
        }

        public void SetValue()
        {
            this.StandardQuantity = Rate * this.Quantity;
            this.Content = String.Format(CultureInfo.InvariantCulture, "{0}{1}", this.Quantity, this.TextUnit);
        }

        private int _standardUnit = -1;
        public int StandardUnit
        {
            get { return _standardUnit; }
            set
            {
                if (_standardUnit != value)
                {
                    _standardUnit = value;
                    RaisePropertyChanged(() => StandardUnit);
                }
            }
        }

        private string _textStandardUnit = string.Empty;
        public string TextStandardUnit
        {
            get { return _textStandardUnit; }
            set
            {
                if (_textStandardUnit != value)
                {
                    _textStandardUnit = value;
                    RaisePropertyChanged(() => TextStandardUnit);
                }
            }
        }

        private string _textUnit = string.Empty;
        public string TextUnit
        {
            get { return _textUnit; }
            set
            {
                if (_textUnit != value)
                {
                    _textUnit = value;
                    RaisePropertyChanged(() => TextUnit);
                }
            }
        }

        /// <summary>
        ///Content
        /// </summary>
        private string _content = string.Empty;
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    RaisePropertyChanged(() => Content);
                }
            }
        }

        private decimal _unitPrice = 0;
        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    RaisePropertyChanged(() => UnitPrice);
                }
            }
        }
        #endregion
    }
}
