using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;

namespace CPC.POSReport.Model
{
    public class EmailModel : ModelBase
    {
        private bool _isSend;
        /// <summary>
        /// IsSend property
        /// </summary>
        public bool IsSend
        {
            get { return _isSend; }
            set
            {
                if (_isSend != value)
                {
                    _isSend = value;
                    OnPropertyChanged(()=> IsSend);
                }
            }
        }
        private string _address;
        /// <summary>
        /// Email address property
        /// </summary>
        public string Address
        {
            get { return _address; }
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged(()=> Address);
                }
            }
        }

        public EmailModel()
        {
        }
        public EmailModel(bool isSend, string Address)
        {
            this.IsSend = isSend;
            this.Address = Address;
        }
    }
}
