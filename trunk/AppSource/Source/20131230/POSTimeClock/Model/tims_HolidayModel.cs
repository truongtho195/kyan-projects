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
using CPC.Toolkit.Base;
using CPC.TimeClock.Database;

namespace CPC.TimeClock.Model
{
    /// <summary>
    /// Model for table tims_Holiday
    /// </summary>
    [Serializable]
    public partial class tims_HolidayModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public tims_HolidayModel()
        {
            this.IsNew = true;
            this.tims_Holiday = new tims_Holiday();
        }

        // Default constructor that set entity to field
        public tims_HolidayModel(tims_Holiday tims_holiday, bool isRaiseProperties = false)
        {
            this.tims_Holiday = tims_holiday;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public tims_Holiday tims_Holiday { get; private set; }

        #endregion

        #region Primitive Properties

        protected int _id;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Id</param>
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

        protected string _title;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Title</param>
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set
            {
                if (this._title != value)
                {
                    this.IsDirty = true;
                    this._title = value;
                    OnPropertyChanged(() => Title);
                    PropertyChangedCompleted(() => Title);
                }
            }
        }

        protected int _holidayOption;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the HolidayOption</param>
        /// </summary>
        public int HolidayOption
        {
            get { return this._holidayOption; }
            set
            {
                if (this._holidayOption != value)
                {
                    this.IsDirty = true;
                    this._holidayOption = value;
                    OnPropertyChanged(() => HolidayOption);
                    PropertyChangedCompleted(() => HolidayOption);
                }
            }
        }

        protected Nullable<System.DateTime> _fromDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the FromDate</param>
        /// </summary>
        public Nullable<System.DateTime> FromDate
        {
            get { return this._fromDate; }
            set
            {
                if (this._fromDate != value)
                {
                    this.IsDirty = true;
                    this._fromDate = value;
                    OnPropertyChanged(() => FromDate);
                    PropertyChangedCompleted(() => FromDate);
                }
            }
        }

        protected Nullable<System.DateTime> _toDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ToDate</param>
        /// </summary>
        public Nullable<System.DateTime> ToDate
        {
            get { return this._toDate; }
            set
            {
                if (this._toDate != value)
                {
                    this.IsDirty = true;
                    this._toDate = value;
                    OnPropertyChanged(() => ToDate);
                    PropertyChangedCompleted(() => ToDate);
                }
            }
        }

        protected Nullable<int> _month;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Month</param>
        /// </summary>
        public Nullable<int> Month
        {
            get { return this._month; }
            set
            {
                if (this._month != value)
                {
                    this.IsDirty = true;
                    this._month = value;
                    OnPropertyChanged(() => Month);
                    PropertyChangedCompleted(() => Month);
                }
            }
        }

        protected Nullable<int> _day;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Day</param>
        /// </summary>
        public Nullable<int> Day
        {
            get { return this._day; }
            set
            {
                if (this._day != value)
                {
                    this.IsDirty = true;
                    this._day = value;
                    OnPropertyChanged(() => Day);
                    PropertyChangedCompleted(() => Day);
                }
            }
        }

        protected Nullable<int> _dayOfWeek;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the DayOfWeek</param>
        /// </summary>
        public Nullable<int> DayOfWeek
        {
            get { return this._dayOfWeek; }
            set
            {
                if (this._dayOfWeek != value)
                {
                    this.IsDirty = true;
                    this._dayOfWeek = value;
                    OnPropertyChanged(() => DayOfWeek);
                    PropertyChangedCompleted(() => DayOfWeek);
                }
            }
        }

        protected Nullable<int> _weekOfMonth;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the WeekOfMonth</param>
        /// </summary>
        public Nullable<int> WeekOfMonth
        {
            get { return this._weekOfMonth; }
            set
            {
                if (this._weekOfMonth != value)
                {
                    this.IsDirty = true;
                    this._weekOfMonth = value;
                    OnPropertyChanged(() => WeekOfMonth);
                    PropertyChangedCompleted(() => WeekOfMonth);
                }
            }
        }

        protected bool _activeFlag;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ActiveFlag</param>
        /// </summary>
        public bool ActiveFlag
        {
            get { return this._activeFlag; }
            set
            {
                if (this._activeFlag != value)
                {
                    this.IsDirty = true;
                    this._activeFlag = value;
                    OnPropertyChanged(() => ActiveFlag);
                    PropertyChangedCompleted(() => ActiveFlag);
                }
            }
        }

        protected System.DateTime _dateCreated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the DateCreated</param>
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

        protected Nullable<System.DateTime> _dateUpdated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the DateUpdated</param>
        /// </summary>
        public Nullable<System.DateTime> DateUpdated
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

        protected string _userCreated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the UserCreated</param>
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

        protected string _userUpdated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the UserUpdated</param>
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
                this.tims_Holiday.Id = this.Id;
            if (this.Title != null)
                this.tims_Holiday.Title = this.Title.Trim();
            this.tims_Holiday.HolidayOption = this.HolidayOption;
            this.tims_Holiday.FromDate = this.FromDate;
            this.tims_Holiday.ToDate = this.ToDate;
            this.tims_Holiday.Month = this.Month;
            this.tims_Holiday.Day = this.Day;
            this.tims_Holiday.DayOfWeek = this.DayOfWeek;
            this.tims_Holiday.WeekOfMonth = this.WeekOfMonth;
            this.tims_Holiday.ActiveFlag = this.ActiveFlag;
            this.tims_Holiday.DateCreated = this.DateCreated;
            this.tims_Holiday.DateUpdated = this.DateUpdated;
            if (this.UserCreated != null)
                this.tims_Holiday.UserCreated = this.UserCreated.Trim();
            if (this.UserUpdated != null)
                this.tims_Holiday.UserUpdated = this.UserUpdated.Trim();
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.tims_Holiday.Id;
            this._title = this.tims_Holiday.Title;
            this._holidayOption = this.tims_Holiday.HolidayOption;
            this._fromDate = this.tims_Holiday.FromDate;
            this._toDate = this.tims_Holiday.ToDate;
            this._month = this.tims_Holiday.Month;
            this._day = this.tims_Holiday.Day;
            this._dayOfWeek = this.tims_Holiday.DayOfWeek;
            this._weekOfMonth = this.tims_Holiday.WeekOfMonth;
            this._activeFlag = this.tims_Holiday.ActiveFlag;
            this._dateCreated = this.tims_Holiday.DateCreated;
            this._dateUpdated = this.tims_Holiday.DateUpdated;
            this._userCreated = this.tims_Holiday.UserCreated;
            this._userUpdated = this.tims_Holiday.UserUpdated;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.tims_Holiday.Id;
            this.Title = this.tims_Holiday.Title;
            this.HolidayOption = this.tims_Holiday.HolidayOption;
            this.FromDate = this.tims_Holiday.FromDate;
            this.ToDate = this.tims_Holiday.ToDate;
            this.Month = this.tims_Holiday.Month;
            this.Day = this.tims_Holiday.Day;
            this.DayOfWeek = this.tims_Holiday.DayOfWeek;
            this.WeekOfMonth = this.tims_Holiday.WeekOfMonth;
            this.ActiveFlag = this.tims_Holiday.ActiveFlag;
            this.DateCreated = this.tims_Holiday.DateCreated;
            this.DateUpdated = this.tims_Holiday.DateUpdated;
            this.UserCreated = this.tims_Holiday.UserCreated;
            this.UserUpdated = this.tims_Holiday.UserUpdated;
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
                    case "Title":
                        break;
                    case "Description":
                        break;
                    case "HolidayOption":
                        break;
                    case "FromDate":
                        break;
                    case "ToDate":
                        break;
                    case "Month":
                        break;
                    case "Day":
                        break;
                    case "DayOfWeek":
                        break;
                    case "WeekOfMonth":
                        break;
                    case "ActiveFlag":
                        break;
                    case "DateCreated":
                        break;
                    case "DateUpdated":
                        break;
                    case "UserCreated":
                        break;
                    case "UserUpdated":
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