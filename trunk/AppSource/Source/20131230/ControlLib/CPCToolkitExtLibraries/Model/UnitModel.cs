using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class UnitModel : ToolkitModelBase
    {
        #region Properties

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

        private string _barcode = string.Empty;
        public string Barcode
        {
            get { return _barcode; }
            set
            {
                if (_barcode != value)
                {
                    _barcode = value;
                    RaisePropertyChanged(() => Barcode);
                }
            }
        }

        #endregion
    }
}
