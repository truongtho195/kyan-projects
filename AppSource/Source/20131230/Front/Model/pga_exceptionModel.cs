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
using System.Linq;
using System.Linq.Expressions;
using CPC.Helper;
using CPC.POS.Database;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table pga_exception
    /// </summary>
    [Serializable]
    public partial class pga_exceptionModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public pga_exceptionModel()
        {
            this.IsNew = true;
            this.pga_exception = new pga_exception();
        }

        // Default constructor that set entity to field
        public pga_exceptionModel(pga_exception pga_exception, bool isRaiseProperties = false)
        {
            this.pga_exception = pga_exception;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public pga_exception pga_exception { get; private set; }

        #endregion

        #region Primitive Properties

        protected int _jexid;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the jexid</param>
        /// </summary>
        public int jexid
        {
            get { return this._jexid; }
            set
            {
                if (this._jexid != value)
                {
                    this.IsDirty = true;
                    this._jexid = value;
                    OnPropertyChanged(() => jexid);
                    PropertyChangedCompleted(() => jexid);
                }
            }
        }

        protected int _jexscid;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the jexscid</param>
        /// </summary>
        public int jexscid
        {
            get { return this._jexscid; }
            set
            {
                if (this._jexscid != value)
                {
                    this.IsDirty = true;
                    this._jexscid = value;
                    OnPropertyChanged(() => jexscid);
                    PropertyChangedCompleted(() => jexscid);
                }
            }
        }

        protected Nullable<System.DateTime> _jexdate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the jexdate</param>
        /// </summary>
        public Nullable<System.DateTime> jexdate
        {
            get { return this._jexdate; }
            set
            {
                if (this._jexdate != value)
                {
                    this.IsDirty = true;
                    this._jexdate = value;
                    OnPropertyChanged(() => jexdate);
                    PropertyChangedCompleted(() => jexdate);
                }
            }
        }

        protected Nullable<System.TimeSpan> _jextime;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the jextime</param>
        /// </summary>
        public Nullable<System.TimeSpan> jextime
        {
            get { return this._jextime; }
            set
            {
                if (this._jextime != value)
                {
                    this.IsDirty = true;
                    this._jextime = value;
                    OnPropertyChanged(() => jextime);
                    PropertyChangedCompleted(() => jextime);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// <param>Public Method</param>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set PropertyModel to Entity</param>
        /// </summary>
        public void ToEntity()
        {
            if (IsNew)
                this.pga_exception.jexid = this.jexid;
            this.pga_exception.jexscid = this.jexscid;
            this.pga_exception.jexdate = this.jexdate;
            this.pga_exception.jextime = this.jextime;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._jexid = this.pga_exception.jexid;
            this._jexscid = this.pga_exception.jexscid;
            this._jexdate = this.pga_exception.jexdate;
            this._jextime = this.pga_exception.jextime;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.jexid = this.pga_exception.jexid;
            this.jexscid = this.pga_exception.jexscid;
            this.jexdate = this.pga_exception.jexdate;
            this.jextime = this.pga_exception.jextime;
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
                    case "jexid":
                        break;
                    case "jexscid":
                        break;
                    case "jexdate":
                        break;
                    case "jextime":
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
