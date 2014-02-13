using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPCToolkitExtLibraries;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class DiscountModel : ToolkitModelBase
    {
        #region Properties

        /// <summary>
        ///Value
        /// </summary>
        private decimal _value=0;
        public decimal Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged(() => Value);
                }
            }
        }

        /// <summary>
        ///Type
        /// </summary>
        private bool _type =true;
        public bool Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    RaisePropertyChanged(() => Type);
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

        #endregion
    }
}
