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
    /// Model for table pga_jobclass
    /// </summary>
    [Serializable]
    public partial class pga_jobclassModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public pga_jobclassModel()
        {
            this.IsNew = true;
            this.pga_jobclass = new pga_jobclass();
        }

        // Default constructor that set entity to field
        public pga_jobclassModel(pga_jobclass pga_jobclass, bool isRaiseProperties = false)
        {
            this.pga_jobclass = pga_jobclass;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public pga_jobclass pga_jobclass { get; private set; }

        #endregion

        #region Primitive Properties

        protected int _jclid;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the jclid</para>
        /// </summary>
        public int jclid
        {
            get { return this._jclid; }
            set
            {
                if (this._jclid != value)
                {
                    this.IsDirty = true;
                    this._jclid = value;
                    OnPropertyChanged(() => jclid);
                    PropertyChangedCompleted(() => jclid);
                }
            }
        }

        protected string _jclname;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the jclname</para>
        /// </summary>
        public string jclname
        {
            get { return this._jclname; }
            set
            {
                if (this._jclname != value)
                {
                    this.IsDirty = true;
                    this._jclname = value;
                    OnPropertyChanged(() => jclname);
                    PropertyChangedCompleted(() => jclname);
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
                this.pga_jobclass.jclid = this.jclid;
            this.pga_jobclass.jclname = this.jclname;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._jclid = this.pga_jobclass.jclid;
            this._jclname = this.pga_jobclass.jclname;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.jclid = this.pga_jobclass.jclid;
            this.jclname = this.pga_jobclass.jclname;
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
                    case "jclid":
                        break;
                    case "jclname":
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
