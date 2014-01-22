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
    /// Model for table tims_WorkPermission
    /// </summary>
    [Serializable]
    public partial class tims_WorkPermissionModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public tims_WorkPermissionModel()
        {
            this.IsNew = true;
            this.tims_WorkPermission = new tims_WorkPermission();
        }

        // Default constructor that set entity to field
        public tims_WorkPermissionModel(tims_WorkPermission tims_workpermission, bool isRaiseProperties = false)
        {
            this.tims_WorkPermission = tims_workpermission;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public tims_WorkPermission tims_WorkPermission { get; private set; }

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

        protected long _employeeId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the EmployeeId</param>
        /// </summary>
        public long EmployeeId
        {
            get { return this._employeeId; }
            set
            {
                if (this._employeeId != value)
                {
                    this.IsDirty = true;
                    this._employeeId = value;
                    OnPropertyChanged(() => EmployeeId);
                    PropertyChangedCompleted(() => EmployeeId);
                }
            }
        }

        protected int _permissionType;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PermissionType</param>
        /// </summary>
        public int PermissionType
        {
            get { return this._permissionType; }
            set
            {
                if (this._permissionType != value)
                {
                    this.IsDirty = true;
                    this._permissionType = value;
                    OnPropertyChanged(() => PermissionType);
                    PropertyChangedCompleted(() => PermissionType);
                }
            }
        }

        protected System.DateTime _fromDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the FromDate</param>
        /// </summary>
        public System.DateTime FromDate
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

        protected System.DateTime _toDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ToDate</param>
        /// </summary>
        public System.DateTime ToDate
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

        protected string _note;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Note</param>
        /// </summary>
        public string Note
        {
            get { return this._note; }
            set
            {
                if (this._note != value)
                {
                    this.IsDirty = true;
                    this._note = value;
                    OnPropertyChanged(() => Note);
                    PropertyChangedCompleted(() => Note);
                }
            }
        }

        protected short _noOfDays;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the NoOfDays</param>
        /// </summary>
        public short NoOfDays
        {
            get { return this._noOfDays; }
            set
            {
                if (this._noOfDays != value)
                {
                    this.IsDirty = true;
                    this._noOfDays = value;
                    OnPropertyChanged(() => NoOfDays);
                    PropertyChangedCompleted(() => NoOfDays);
                }
            }
        }

        protected float _hourPerDay;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the HourPerDay</param>
        /// </summary>
        public float HourPerDay
        {
            get { return this._hourPerDay; }
            set
            {
                if (this._hourPerDay != value)
                {
                    this.IsDirty = true;
                    this._hourPerDay = value;
                    OnPropertyChanged(() => HourPerDay);
                    PropertyChangedCompleted(() => HourPerDay);
                }
            }
        }

        protected bool _paidFlag;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PaidFlag</param>
        /// </summary>
        public bool PaidFlag
        {
            get { return this._paidFlag; }
            set
            {
                if (this._paidFlag != value)
                {
                    this.IsDirty = true;
                    this._paidFlag = value;
                    OnPropertyChanged(() => PaidFlag);
                    PropertyChangedCompleted(() => PaidFlag);
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

        protected int _overtimeOptions;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the OvertimeOptions</param>
        /// </summary>
        public int OvertimeOptions
        {
            get { return this._overtimeOptions; }
            set
            {
                if (this._overtimeOptions != value)
                {
                    this.IsDirty = true;
                    this._overtimeOptions = value;
                    OnPropertyChanged(() => OvertimeOptions);
                    PropertyChangedCompleted(() => OvertimeOptions);
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

        protected Nullable<System.DateTime> _dateCreated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the DateCreated</param>
        /// </summary>
        public Nullable<System.DateTime> DateCreated
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
                this.tims_WorkPermission.Id = this.Id;
            this.tims_WorkPermission.EmployeeId = this.EmployeeId;
            this.tims_WorkPermission.PermissionType = this.PermissionType;
            this.tims_WorkPermission.FromDate = this.FromDate;
            this.tims_WorkPermission.ToDate = this.ToDate;
            if (this.Note != null)
                this.tims_WorkPermission.Note = this.Note.Trim();
            this.tims_WorkPermission.NoOfDays = this.NoOfDays;
            this.tims_WorkPermission.HourPerDay = this.HourPerDay;
            this.tims_WorkPermission.PaidFlag = this.PaidFlag;
            this.tims_WorkPermission.ActiveFlag = this.ActiveFlag;
            this.tims_WorkPermission.OvertimeOptions = this.OvertimeOptions;
            if (this.UserCreated != null)
                this.tims_WorkPermission.UserCreated = this.UserCreated.Trim();
            this.tims_WorkPermission.DateCreated = this.DateCreated;
            if (this.UserUpdated != null)
                this.tims_WorkPermission.UserUpdated = this.UserUpdated.Trim();
            this.tims_WorkPermission.DateUpdated = this.DateUpdated;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.tims_WorkPermission.Id;
            this._employeeId = this.tims_WorkPermission.EmployeeId;
            this._permissionType = this.tims_WorkPermission.PermissionType;
            this._fromDate = this.tims_WorkPermission.FromDate;
            this._toDate = this.tims_WorkPermission.ToDate;
            this._note = this.tims_WorkPermission.Note;
            this._noOfDays = this.tims_WorkPermission.NoOfDays;
            this._hourPerDay = this.tims_WorkPermission.HourPerDay;
            this._paidFlag = this.tims_WorkPermission.PaidFlag;
            this._activeFlag = this.tims_WorkPermission.ActiveFlag;
            this._overtimeOptions = this.tims_WorkPermission.OvertimeOptions;
            this._userCreated = this.tims_WorkPermission.UserCreated;
            this._dateCreated = this.tims_WorkPermission.DateCreated;
            this._userUpdated = this.tims_WorkPermission.UserUpdated;
            this._dateUpdated = this.tims_WorkPermission.DateUpdated;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.tims_WorkPermission.Id;
            this.EmployeeId = this.tims_WorkPermission.EmployeeId;
            this.PermissionType = this.tims_WorkPermission.PermissionType;
            this.FromDate = this.tims_WorkPermission.FromDate;
            this.ToDate = this.tims_WorkPermission.ToDate;
            this.Note = this.tims_WorkPermission.Note;
            this.NoOfDays = this.tims_WorkPermission.NoOfDays;
            this.HourPerDay = this.tims_WorkPermission.HourPerDay;
            this.PaidFlag = this.tims_WorkPermission.PaidFlag;
            this.ActiveFlag = this.tims_WorkPermission.ActiveFlag;
            this.OvertimeOptions = this.tims_WorkPermission.OvertimeOptions;
            this.UserCreated = this.tims_WorkPermission.UserCreated;
            this.DateCreated = this.tims_WorkPermission.DateCreated;
            this.UserUpdated = this.tims_WorkPermission.UserUpdated;
            this.DateUpdated = this.tims_WorkPermission.DateUpdated;
        }

        #endregion

        #region Custom Code

        #region Properties
        #region PayEventSelected
        private int _payEventSelected;
        /// <summary>
        /// Gets or sets the PayEventSelected.
        /// </summary>
        public int PayEventSelected
        {
            get { return _payEventSelected; }
            set
            {
                if (_payEventSelected != value)
                {
                    this.IsDirty = true;
                    _payEventSelected = value;
                    OnPropertyChanged(() => PayEventSelected);
                    PropertyChangedCompleted(() => PayEventSelected);

                }
            }
        }
        #endregion

        #region HasDuplicated
        private bool _hasDuplicated = false;
        /// <summary>
        /// Gets or sets the HasDuplicated.
        /// Set From ViewModel with old item
        /// </summary>
        public bool HasDuplicated
        {
            get { return _hasDuplicated; }
            set
            {
                if (_hasDuplicated != value)
                {
                    _hasDuplicated = value;
                    OnPropertyChanged(() => HasDuplicated);
                }
            }
        }
        #endregion

        #region TotalHour
        /// <summary>
        /// Gets TotalHour.
        /// </summary>
        public float TotalHour
        {
            get
            {
                if (HourPerDay > 0 && NoOfDays > 0)
                    return HourPerDay * NoOfDays;
                return 0;
            }
        }
        #endregion

        #region WorkPermissionText
        private string _workPermissionText = string.Empty;
        public string WorkPermissionText
        {
            get
            {
                return WorkPermissionTypeText();
            }
            
        }
        #endregion

        #endregion

        #region Methods
        public void SetForPayEventSelected()
        {
            if (PaidFlag)
                PayEventSelected = 1;
            else
                PayEventSelected = 2;
        }

        public string WorkPermissionTypeText()
        {
            WorkPermissionTypes workPermission = (WorkPermissionTypes)PermissionType;
            Overtime overtimeType = (Overtime)OvertimeOptions;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (workPermission.HasFlag(WorkPermissionTypes.ArrivingLate))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.WorkPermissionType.SingleOrDefault(x => x.Value.Equals((int)WorkPermissionTypes.ArrivingLate)).Text);
            }
            if (workPermission.HasFlag(WorkPermissionTypes.LeavingEarly))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.WorkPermissionType.SingleOrDefault(x => x.Value.Equals((int)WorkPermissionTypes.LeavingEarly)).Text);
            }
            if (workPermission.HasFlag(WorkPermissionTypes.Absence))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.WorkPermissionType.SingleOrDefault(x => x.Value.Equals((int)WorkPermissionTypes.Absence)).Text);
            }
            if (workPermission.HasFlag(WorkPermissionTypes.SickLeave))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.WorkPermissionType.SingleOrDefault(x => x.Value.Equals((int)WorkPermissionTypes.SickLeave)).Text);
            }
            if (workPermission.HasFlag(WorkPermissionTypes.Vacations))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.WorkPermissionType.SingleOrDefault(x => x.Value.Equals((int)WorkPermissionTypes.Vacations)).Text);
            }
            if (workPermission.HasFlag(WorkPermissionTypes.DisciplinaryLeave))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.WorkPermissionType.SingleOrDefault(x => x.Value.Equals((int)WorkPermissionTypes.DisciplinaryLeave)).Text);
            }
            //Overtimes
            if (overtimeType.HasFlag(Overtime.Before))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.OvertimeTypes.SingleOrDefault(x => x.Value.Equals((int)Overtime.Before)).Text);
            }
            if (overtimeType.HasFlag(Overtime.Break))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.OvertimeTypes.SingleOrDefault(x => x.Value.Equals((int)Overtime.Break)).Text);
            }
            if (overtimeType.HasFlag(Overtime.After))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.OvertimeTypes.SingleOrDefault(x => x.Value.Equals((int)Overtime.After)).Text);
            }
            if (overtimeType.HasFlag(Overtime.Holiday))
            {
                sb.AppendFormat("{0}, ", CPC.Helper.Common.OvertimeTypes.SingleOrDefault(x => x.Value.Equals((int)Overtime.Holiday)).Text);
            }
            string result = string.Empty;
            if (sb.Length > 0)
                result = sb.Remove(sb.Length - 2, 2).ToString();
            return result;
        }
        #endregion

        #region Override Methods

        protected override void PropertyChangedCompleted(string propertyName)
        {
            base.PropertyChangedCompleted(propertyName);
            switch (propertyName)
            {
                case "WorkPermissionID":
                    break;
                case "EmployeeID":
                    break;
                case "PermissionType":
                    OnPropertyChanged(() => OvertimeOptions);
                    OnPropertyChanged(() => WorkPermissionText);
                    break;
                case "OvertimeOptions":
                    OnPropertyChanged(() => PermissionType);
                    break;
                case "FromDate":
                    OnPropertyChanged(() => ToDate);
                    OnPropertyChanged(() => NoOfDays);
                    break;
                case "ToDate":
                    OnPropertyChanged(() => FromDate);
                    OnPropertyChanged(() => NoOfDays);
                    break;
                case "Note":
                    break;
                case "NoOfDays":
                    OnPropertyChanged(() => TotalHour);
                    break;
                case "HourPerDay":
                    OnPropertyChanged(() => TotalHour);
                    break;
                case "PaidFlag":
                    break;
                case "PayEventSelected":
                    if (PayEventSelected == 1)
                        PaidFlag = true;
                    else if (PayEventSelected == 2)
                        PaidFlag = false;
                    break;
                case "ActiveFlag":
                    //OnPropertyChanged(() => Deactivated);
                    break;
                case "CreatedDate":
                    break;
                case "CreatedByID":
                    break;
                case "Modifieddate":
                    break;
                case "ModifiedByID":
                    break;
            }
        }

        #endregion

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                List<string> errors = new List<string>();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string msg = this[prop.Name];
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Add(msg);
                    }
                }

                return string.Join(Environment.NewLine, errors);
            }
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
                    case "EmployeeId":
                        break;
                    case "PermissionType":
                        if (OvertimeOptions == 0 && PermissionType == 0)
                            message = "Permission Type is required !";
                        break;
                    case "OvertimeOptions":
                        if (PermissionType == 0 && OvertimeOptions == 0)
                            message = "Overtime Type is required !";
                        break;
                    case "FromDate":
                        if (FromDate == null)
                            message = "From Date is required !";
                        else if (ToDate != null && FromDate > ToDate)
                            message = "From Date must be less than To Date !";
                        break;
                    case "ToDate":
                        if (ToDate == null)
                            message = "To Date is required !";
                        else if (FromDate != null && FromDate > ToDate)
                            message = "From Date must be less than To Date !";
                        break;
                    case "Note":
                        break;
                    case "NoOfDays":
                        if (NoOfDays <= 0)
                            message = "Number of date is required !";
                        else if (NoOfDays > ToDate.Subtract(FromDate).Days + 1)
                            message = "Number of date not larger than From Date & To Date !";
                        break;
                    case "HourPerDay":
                        if (this.HourPerDay < 0.5 || this.HourPerDay > 24)
                            message = "Hour per day must be between 0.5 and 24h";
                        break;
                    case "PaidFlag":

                        break;
                    case "PayEventSelected":
                        if (PayEventSelected == 0)
                            message = "Pay Event is required";
                        break;
                    case "ActiveFlag":
                        break;
                    case "CreatedDate":
                        break;
                    case "CreatedById":
                        break;
                    case "ModifiedDate":
                        break;
                    case "ModifiedById":
                        break;
                    case "HasDuplicated":
                        if (!IsNew && HasDuplicated)
                            message = "Has Duplicated Work Permission";
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
