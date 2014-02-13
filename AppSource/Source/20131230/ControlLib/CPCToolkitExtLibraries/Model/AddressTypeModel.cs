using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Linq.Expressions;
using System.Reflection;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class AddressTypeModel : ToolkitModelBase
    {
        #region Properties

        private int _id;
        /// <summary>
        /// Gets or sets ID
        /// </summary>
        public int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged(() => ID);
                }
            }
        }

        private string _name = string.Empty;
        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        private bool _isDefault = false;
        /// <summary>
        /// Gets or sets IsDefault
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                if (_isDefault != value)
                {
                    _isDefault = value;
                    RaisePropertyChanged(() => IsDefault);
                }
            }
        }

        #endregion
    }
}
