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
    /// Model for table tims_HolidayHistory
    /// </summary>
    [Serializable]
    public partial class tims_HolidayHistoryModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public tims_HolidayHistoryModel()
        {
            this.IsNew = true;
            this.tims_HolidayHistory = new tims_HolidayHistory();
        }

        // Default constructor that set entity to field
        public tims_HolidayHistoryModel(tims_HolidayHistory tims_holidayhistory, bool isRaiseProperties = false)
        {
            this.tims_HolidayHistory = tims_holidayhistory;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public tims_HolidayHistory tims_HolidayHistory { get; private set; }

        #endregion

        #region Primitive Properties

        protected System.DateTime _date;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Date</para>
        /// </summary>
        public System.DateTime Date
        {
            get { return this._date; }
            set
            {
                if (this._date != value)
                {
                    this.IsDirty = true;
                    this._date = value;
                    OnPropertyChanged(() => Date);
                    PropertyChangedCompleted(() => Date);
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
                this.tims_HolidayHistory.Date = this.Date;
            this.tims_HolidayHistory.Name = this.Name;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._date = this.tims_HolidayHistory.Date;
            this._name = this.tims_HolidayHistory.Name;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Date = this.tims_HolidayHistory.Date;
            this.Name = this.tims_HolidayHistory.Name;
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
                    case "Date":
                        break;
                    case "Name":
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