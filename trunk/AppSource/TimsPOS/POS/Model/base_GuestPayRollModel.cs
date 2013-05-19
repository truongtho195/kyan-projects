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
    /// Model for table base_GuestPayRoll
    /// </summary>
    [Serializable]
    public partial class base_GuestPayRollModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_GuestPayRollModel()
        {
            this.IsNew = true;
            this.base_GuestPayRoll = new base_GuestPayRoll();
        }

        // Default constructor that set entity to field
        public base_GuestPayRollModel(base_GuestPayRoll base_guestpayroll, bool isRaiseProperties = false)
        {
            this.base_GuestPayRoll = base_guestpayroll;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_GuestPayRoll base_GuestPayRoll { get; private set; }

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

        protected string _payrollName;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the PayrollName</para>
        /// </summary>
        public string PayrollName
        {
            get { return this._payrollName; }
            set
            {
                if (this._payrollName != value)
                {
                    this.IsDirty = true;
                    this._payrollName = value;
                    OnPropertyChanged(() => PayrollName);
                    PropertyChangedCompleted(() => PayrollName);
                }
            }
        }

        protected string _payrollType;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the PayrollType</para>
        /// </summary>
        public string PayrollType
        {
            get { return this._payrollType; }
            set
            {
                if (this._payrollType != value)
                {
                    this.IsDirty = true;
                    this._payrollType = value;
                    OnPropertyChanged(() => PayrollType);
                    PropertyChangedCompleted(() => PayrollType);
                }
            }
        }

        protected decimal _rate;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Rate</para>
        /// </summary>
        public decimal Rate
        {
            get { return this._rate; }
            set
            {
                if (this._rate != value)
                {
                    this.IsDirty = true;
                    this._rate = value;
                    OnPropertyChanged(() => Rate);
                    PropertyChangedCompleted(() => Rate);
                }
            }
        }

        protected System.DateTime _dateCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateCreated</para>
        /// </summary>
        public System.DateTime DateCreated
        {
            get { return this._dateCreated; }
            set
            {
                if (this._dateCreated != value)
                {
                    this.IsDirty = true;
                    this._dateCreated = value;
                    OnPropertyChanged(() => DateCreated);
                    PropertyChangedCompleted(() => DateCreated);
                }
            }
        }

        protected string _userCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UserCreated</para>
        /// </summary>
        public string UserCreated
        {
            get { return this._userCreated; }
            set
            {
                if (this._userCreated != value)
                {
                    this.IsDirty = true;
                    this._userCreated = value;
                    OnPropertyChanged(() => UserCreated);
                    PropertyChangedCompleted(() => UserCreated);
                }
            }
        }

        protected System.DateTime _dateUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateUpdated</para>
        /// </summary>
        public System.DateTime DateUpdated
        {
            get { return this._dateUpdated; }
            set
            {
                if (this._dateUpdated != value)
                {
                    this.IsDirty = true;
                    this._dateUpdated = value;
                    OnPropertyChanged(() => DateUpdated);
                    PropertyChangedCompleted(() => DateUpdated);
                }
            }
        }

        protected string _userUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UserUpdated</para>
        /// </summary>
        public string UserUpdated
        {
            get { return this._userUpdated; }
            set
            {
                if (this._userUpdated != value)
                {
                    this.IsDirty = true;
                    this._userUpdated = value;
                    OnPropertyChanged(() => UserUpdated);
                    PropertyChangedCompleted(() => UserUpdated);
                }
            }
        }

        protected Nullable<long> _guestId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the GuestId</para>
        /// </summary>
        public Nullable<long> GuestId
        {
            get { return this._guestId; }
            set
            {
                if (this._guestId != value)
                {
                    this.IsDirty = true;
                    this._guestId = value;
                    OnPropertyChanged(() => GuestId);
                    PropertyChangedCompleted(() => GuestId);
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
                this.base_GuestPayRoll.Id = this.Id;
            this.base_GuestPayRoll.PayrollName = this.PayrollName;
            this.base_GuestPayRoll.PayrollType = this.PayrollType;
            this.base_GuestPayRoll.Rate = this.Rate;
            this.base_GuestPayRoll.DateCreated = this.DateCreated;
            this.base_GuestPayRoll.UserCreated = this.UserCreated;
            this.base_GuestPayRoll.DateUpdated = this.DateUpdated;
            this.base_GuestPayRoll.UserUpdated = this.UserUpdated;
            this.base_GuestPayRoll.GuestId = this.GuestId;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_GuestPayRoll.Id;
            this._payrollName = this.base_GuestPayRoll.PayrollName;
            this._payrollType = this.base_GuestPayRoll.PayrollType;
            this._rate = this.base_GuestPayRoll.Rate;
            this._dateCreated = this.base_GuestPayRoll.DateCreated;
            this._userCreated = this.base_GuestPayRoll.UserCreated;
            this._dateUpdated = this.base_GuestPayRoll.DateUpdated;
            this._userUpdated = this.base_GuestPayRoll.UserUpdated;
            this._guestId = this.base_GuestPayRoll.GuestId;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_GuestPayRoll.Id;
            this.PayrollName = this.base_GuestPayRoll.PayrollName;
            this.PayrollType = this.base_GuestPayRoll.PayrollType;
            this.Rate = this.base_GuestPayRoll.Rate;
            this.DateCreated = this.base_GuestPayRoll.DateCreated;
            this.UserCreated = this.base_GuestPayRoll.UserCreated;
            this.DateUpdated = this.base_GuestPayRoll.DateUpdated;
            this.UserUpdated = this.base_GuestPayRoll.UserUpdated;
            this.GuestId = this.base_GuestPayRoll.GuestId;
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
                    case "PayrollName":
                        break;
                    case "PayrollType":
                        break;
                    case "Rate":
                        break;
                    case "DateCreated":
                        break;
                    case "UserCreated":
                        break;
                    case "DateUpdated":
                        break;
                    case "UserUpdated":
                        break;
                    case "GuestId":
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
