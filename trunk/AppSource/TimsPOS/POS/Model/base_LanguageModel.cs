//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CPC.POS.Database;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table base_Language
    /// </summary>
    [Serializable]
    public partial class base_LanguageModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_LanguageModel()
        {
            this.IsNew = true;
            this.base_Language = new base_Language();
        }

        // Default constructor that set entity to field
        public base_LanguageModel(base_Language base_language, bool isRaiseProperties = false)
        {
            this.base_Language = base_language;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_Language base_Language { get; private set; }

        #endregion

        #region Primitive Properties

        protected int _id;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Id</para>
        /// </summary>
        public int Id
        {
            get { return this._id; }
            set
            {
                if (this._id != value)
                {
                    this.IsDirty = true;
                    this._id = value;
                    OnPropertyChanged(() => Id);
                    PropertyChangedCompleted(() => Id);
                }
            }
        }

        protected string _code;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Code</para>
        /// </summary>
        public string Code
        {
            get { return this._code; }
            set
            {
                if (this._code != value)
                {
                    this.IsDirty = true;
                    this._code = value;
                    OnPropertyChanged(() => Code);
                    PropertyChangedCompleted(() => Code);
                }
            }
        }

        protected string _name;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Name</para>
        /// </summary>
        public string Name
        {
            get { return this._name; }
            set
            {
                if (this._name != value)
                {
                    this.IsDirty = true;
                    this._name = value;
                    OnPropertyChanged(() => Name);
                    PropertyChangedCompleted(() => Name);
                }
            }
        }

        protected byte[] _flag;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Flag</para>
        /// </summary>
        public byte[] Flag
        {
            get { return this._flag; }
            set
            {
                if (this._flag != value)
                {
                    this.IsDirty = true;
                    this._flag = value;
                    OnPropertyChanged(() => Flag);
                    PropertyChangedCompleted(() => Flag);
                }
            }
        }

        protected bool _isLocked;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsLocked</para>
        /// </summary>
        public bool IsLocked
        {
            get { return this._isLocked; }
            set
            {
                if (this._isLocked != value)
                {
                    this.IsDirty = true;
                    this._isLocked = value;
                    OnPropertyChanged(() => IsLocked);
                    PropertyChangedCompleted(() => IsLocked);
                }
            }
        }

        protected string _xml;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Xml</para>
        /// </summary>
        public string Xml
        {
            get { return this._xml; }
            set
            {
                if (this._xml != value)
                {
                    this.IsDirty = true;
                    this._xml = value;
                    OnPropertyChanged(() => Xml);
                    PropertyChangedCompleted(() => Xml);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// <para>Public Method</para>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set PropertyModel to Entity</para>
        /// </summary>
        public void ToEntity()
        {
            if (IsNew)
                this.base_Language.Id = this.Id;
            this.base_Language.Code = this.Code;
            this.base_Language.Name = this.Name;
            this.base_Language.Flag = this.Flag;
            this.base_Language.IsLocked = this.IsLocked;
            this.base_Language.Xml = this.Xml;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_Language.Id;
            this._code = this.base_Language.Code;
            this._name = this.base_Language.Name;
            this._flag = this.base_Language.Flag;
            this._isLocked = this.base_Language.IsLocked;
            this._xml = this.base_Language.Xml;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_Language.Id;
            this.Code = this.base_Language.Code;
            this.Name = this.base_Language.Name;
            this.Flag = this.base_Language.Flag;
            this.IsLocked = this.base_Language.IsLocked;
            this.Xml = this.base_Language.Xml;
        }

        #endregion

        #region Custom Code


        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;

                switch (columnName)
                {
                    case "Id":
                        break;
                    case "Code":
                        break;
                    case "Name":
                        break;
                    case "Flag":
                        break;
                    case "IsLocked":
                        break;
                    case "Xml":
                        break;
                }

                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }

        #endregion
    }
}