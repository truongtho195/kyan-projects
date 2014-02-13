using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace CPCToolkitExtLibraries
{
    /// <summary>
    /// Model for table Address
    /// </summary>
    [Serializable]
    public partial class AddressControlModel : ToolkitModelBase, IDataErrorInfo
    {
        #region Primitive Properties

        protected int _addressID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AddressID</para>
        /// </summary>
        public int AddressID
        {
            get { return this._addressID; }
            set
            {
                if (this._addressID != value)
                {
                    this.IsChangeData = true;
                    this._addressID = value;
                    RaisePropertyChanged(() => AddressID);
                }
            }
        }

        protected int _addressTypeID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AddressTypeID</para>
        /// </summary>
        public int AddressTypeID
        {
            get { return this._addressTypeID; }
            set
            {
                if (this._addressTypeID != value)
                {
                    this.IsChangeData = true;
                    this._addressTypeID = value;
                    RaisePropertyChanged(() => AddressTypeID);
                }
            }
        }

        protected string _addressLine1;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AddressLine1</para>
        /// </summary>
        public string AddressLine1
        {
            get { return this._addressLine1; }
            set
            {
                if (this._addressLine1 != value)
                {
                    this.IsChangeData = true;
                    this._addressLine1 = value;
                    RaisePropertyChanged(() => AddressLine1);
                }
            }
        }

        protected string _addressLine2;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AddressLine2</para>
        /// </summary>
        public string AddressLine2
        {
            get { return this._addressLine2; }
            set
            {
                if (this._addressLine2 != value)
                {
                    this.IsChangeData = true;
                    this._addressLine2 = value;
                    RaisePropertyChanged(() => AddressLine2);
                }
            }
        }

        protected string _city;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the City</para>
        /// </summary>
        public string City
        {
            get { return this._city; }
            set
            {
                if (this._city != value)
                {
                    this.IsChangeData = true;
                    this._city = value;
                    RaisePropertyChanged(() => City);
                }
            }
        }

        protected short _stateProvinceID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the StateProvinceID</para>
        /// </summary>
        public short StateProvinceID
        {
            get { return this._stateProvinceID; }
            set
            {
                if (this._stateProvinceID != value)
                {
                    this.IsChangeData = true;
                    this._stateProvinceID = value;
                    RaisePropertyChanged(() => StateProvinceID);
                }
            }
        }

        protected string _postalCode;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the PostalCode</para>
        /// </summary>
        public string PostalCode
        {
            get { return this._postalCode; }
            set
            {
                if (this._postalCode != value)
                {
                    this.IsChangeData = true;
                    this._postalCode = value;
                    RaisePropertyChanged(() => PostalCode);
                }
            }
        }

        protected short _countryID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the CountryID</para>
        /// </summary>
        public short CountryID
        {
            get { return this._countryID; }
            set
            {
                if (this._countryID != value)
                {
                    this.IsChangeData = true;
                    this._countryID = value;
                    RaisePropertyChanged(() => CountryID);
                }
            }
        }

        protected Nullable<System.DateTime> _modifiedDate;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ModifiedDate</para>
        /// </summary>
        public Nullable<System.DateTime> ModifiedDate
        {
            get { return this._modifiedDate; }
            set
            {
                if (this._modifiedDate != value)
                {
                    this.IsChangeData = true;
                    this._modifiedDate = value;
                    RaisePropertyChanged(() => ModifiedDate);
                }
            }
        }

        protected string _modifiedBy;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ModifiedBy</para>
        /// </summary>
        public string ModifiedBy
        {
            get { return this._modifiedBy; }
            set
            {
                if (this._modifiedBy != value)
                {
                    this.IsChangeData = true;
                    this._modifiedBy = value;
                    RaisePropertyChanged(() => ModifiedBy);
                }
            }
        }
        protected bool _isDefault;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsDefault</para>
        /// </summary>
        public bool IsDefault
        {
            get { return this._isDefault; }
            set
            {
                if (this._isDefault != value)
                {
                    this.IsChangeData = true;
                    this._isDefault = value;
                    RaisePropertyChanged(() => IsDefault);
                }
            }
        }
        protected bool _isError;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsError</para>
        /// </summary>
        public bool IsError
        {
            get { return this._isError; }
            set
            {
                if (this._isError != value)
                {
                    this._isError = value;
                    RaisePropertyChanged(() => IsError);
                }
            }
        }

        protected bool _isNewInControl;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsNewInControl</para>
        /// </summary>
        public bool IsNewInControl
        {
            get { return this._isNewInControl; }
            set
            {
                if (this._isNewInControl != value)
                {
                    this._isNewInControl = value;
                    RaisePropertyChanged(() => IsNewInControl);
                }
            }
        }

        protected bool _hasState;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the HasState</para>
        /// </summary>
        public bool HasState
        {
            get { return this._hasState; }
            set
            {
                if (this._hasState != value)
                {
                    this._hasState = value;
                    RaisePropertyChanged(() => HasState);
                    this.RaisePropertyChanged(() => PostalCode);
                    this.RaisePropertyChanged(() => StateProvinceID);
                }
            }
        }

        private bool _isChangeData = false;
        public bool IsChangeData
        {
            get { return _isChangeData; }
            set
            {
                if (_isChangeData != value)
                {
                    _isChangeData = value;
                    RaisePropertyChanged(() => IsChangeData);
                }
            }
        }

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        [XmlIgnore]
        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        [XmlIgnore]
        public Dictionary<string, string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                if (_errors != value)
                {
                    _errors = value;
                    RaisePropertyChanged(() => Errors);
                }
            }
        }
        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                this.Errors.Remove(columnName);
                switch (columnName)
                {
                    case "City":
                        if (string.IsNullOrEmpty(this.City))
                        {
                            message = "City is required.";
                        }
                        break;

                    case "StateProvinceID":
                        if (this.StateProvinceID <= 0 && this.HasState)
                        {
                            message = "StateProvince is required.";
                        }
                        break;

                    case "AddressLine1":
                        if (string.IsNullOrEmpty(AddressLine1))
                        {
                            message = "AddressLine is required.";
                        }

                        break;
                    case "PostalCode":
                        if (this.HasState)
                            if (string.IsNullOrEmpty(this.PostalCode))
                            {
                                message = "PostalCode is required.";
                            }
                            else if (this.PostalCode.Length != 5 && this.PostalCode.Length != 9)
                                message = "PostalCode must be contained 5 or 9 digit";
                        break;

                    case "CountryID":
                        if (this.CountryID <= 0)
                        {
                            message = "Country is required.";
                        }
                        break;

                }
                if (!String.IsNullOrEmpty(message))
                    this.Errors.Add(columnName, message);
                return message;
            }
        }

        #endregion
        #endregion
    }
}
