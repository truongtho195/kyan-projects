using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class DataSearchModel : ToolkitModelBase
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
                }
            }
        }

        private int _level = 0;
        /// <summary>
        /// Gets or sets the Level value.
        /// </summary>
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    RaisePropertyChanged(() => Level);
                }
            }
        }

        private string _propertyChildren = string.Empty;
        /// <summary>
        /// Gets or sets the PropertyChildren value.
        /// </summary>
        public string PropertyChildren
        {
            get { return _propertyChildren; }
            set
            {
                if (_propertyChildren != value)
                {
                    _propertyChildren = value;
                    RaisePropertyChanged(() => PropertyChildren);
                }
            }
        }

        private string _propertyType = string.Empty;
        /// <summary>
        /// Gets or sets the PropertyType value.
        /// </summary>
        public string PropertyType
        {
            get { return _propertyType; }
            set
            {
                if (_propertyType != value)
                {
                    _propertyType = value;
                    RaisePropertyChanged(() => PropertyType);
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

        private string _displayName = string.Empty;
        /// <summary>
        /// Gets or sets the DisplayName value.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    RaisePropertyChanged(() => DisplayName);
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
                }
            }
        }

        private string _content = string.Empty;
        /// <summary>
        /// Gets or sets the Content value.
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

        private string _currentPropertyName = string.Empty;
        /// <summary>
        /// Gets or sets the PropertyName value.
        /// </summary>
        public string CurrentPropertyName
        {
            get { return _currentPropertyName; }
            set
            {
                if (_currentPropertyName != value)
                {
                    _currentPropertyName = value;
                    RaisePropertyChanged(() => CurrentPropertyName);
                }
            }
        }

        #endregion
    }

}
