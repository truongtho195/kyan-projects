using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class DataModel : ToolkitModelBase
    {
        #region Properties

        private int _id;
        /// <summary>
        /// Gets or sets the ID value.
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
                    base.OnChanged();
                }
            }
        }

        private string _keyName = string.Empty;
        /// <summary>
        /// Gets or sets the Name value.
        /// </summary>
        public string KeyName
        {
            get { return _keyName; }
            set
            {
                if (_keyName != value)
                {
                    _keyName = value;
                    RaisePropertyChanged(() => KeyName);
                }
            }
        }


        private string _name = string.Empty;
        /// <summary>
        /// Gets or sets the Name value.
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
                    base.OnChanged();
                }
            }
        }

        private string _shortName;
        /// <summary>
        /// Gets or sets the ShortName value.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
            set
            {
                if (_shortName != value)
                {
                    _shortName = value;
                    RaisePropertyChanged(() => ShortName);
                    base.OnChanged();
                }
            }
        }

        private string _content;
        /// <summary>
        /// Gets or sets the ShortName value.
        /// </summary>
        public string Content
        {
            get { return _content; }
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
