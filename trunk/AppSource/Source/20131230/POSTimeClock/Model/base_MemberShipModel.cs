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
    /// Model for table base_MemberShip
    /// </summary>
    [Serializable]
    public partial class base_MemberShipModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_MemberShipModel()
        {
            this.IsNew = true;
            this.base_MemberShip = new base_MemberShip();
        }

        // Default constructor that set entity to field
        public base_MemberShipModel(base_MemberShip base_membership, bool isRaiseProperties = false)
        {
            this.base_MemberShip = base_membership;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_MemberShip base_MemberShip { get; private set; }

        #endregion

        #region Primitive Properties

        protected long _id;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Id</param>
        /// </summary>
        public long Id
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

        protected string _idCard;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IdCard</param>
        /// </summary>
        public string IdCard
        {
            get { return this._idCard; }
            set
            {
                if (this._idCard != value)
                {
                    this.IsDirty = true;
                    this._idCard = value;
                    OnPropertyChanged(() => IdCard);
                    PropertyChangedCompleted(() => IdCard);
                }
            }
        }

        protected byte[] _idCardImg;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IdCardImg</param>
        /// </summary>
        public byte[] IdCardImg
        {
            get { return this._idCardImg; }
            set
            {
                if (this._idCardImg != value)
                {
                    this.IsDirty = true;
                    this._idCardImg = value;
                    OnPropertyChanged(() => IdCardImg);
                    PropertyChangedCompleted(() => IdCardImg);
                }
            }
        }

        protected long _guestId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the GuestId</param>
        /// </summary>
        public long GuestId
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

        protected string _memberType;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the MemberType</param>
        /// </summary>
        public string MemberType
        {
            get { return this._memberType; }
            set
            {
                if (this._memberType != value)
                {
                    this.IsDirty = true;
                    this._memberType = value;
                    OnPropertyChanged(() => MemberType);
                    PropertyChangedCompleted(() => MemberType);
                }
            }
        }

        protected short _status;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Status</param>
        /// </summary>
        public short Status
        {
            get { return this._status; }
            set
            {
                if (this._status != value)
                {
                    this.IsDirty = true;
                    this._status = value;
                    OnPropertyChanged(() => Status);
                    PropertyChangedCompleted(() => Status);
                }
            }
        }

        protected bool _isPurged;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IsPurged</param>
        /// </summary>
        public bool IsPurged
        {
            get { return this._isPurged; }
            set
            {
                if (this._isPurged != value)
                {
                    this.IsDirty = true;
                    this._isPurged = value;
                    OnPropertyChanged(() => IsPurged);
                    PropertyChangedCompleted(() => IsPurged);
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

        protected System.DateTime _dateUpdated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the DateUpdated</param>
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

        protected string _guestResource;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the GuestResource</param>
        /// </summary>
        public string GuestResource
        {
            get { return this._guestResource; }
            set
            {
                if (this._guestResource != value)
                {
                    this.IsDirty = true;
                    this._guestResource = value;
                    OnPropertyChanged(() => GuestResource);
                    PropertyChangedCompleted(() => GuestResource);
                }
            }
        }

        protected decimal _totalCutOffThreshold;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the TotalCutOffThreshold</param>
        /// </summary>
        public decimal TotalCutOffThreshold
        {
            get { return this._totalCutOffThreshold; }
            set
            {
                if (this._totalCutOffThreshold != value)
                {
                    this.IsDirty = true;
                    this._totalCutOffThreshold = value;
                    OnPropertyChanged(() => TotalCutOffThreshold);
                    PropertyChangedCompleted(() => TotalCutOffThreshold);
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
                this.base_MemberShip.Id = this.Id;
            if (this.IdCard != null)
                this.base_MemberShip.IdCard = this.IdCard.Trim();
            this.base_MemberShip.IdCardImg = this.IdCardImg;
            this.base_MemberShip.GuestId = this.GuestId;
            if (this.MemberType != null)
                this.base_MemberShip.MemberType = this.MemberType.Trim();
            this.base_MemberShip.Status = this.Status;
            this.base_MemberShip.IsPurged = this.IsPurged;
            if (this.UserCreated != null)
                this.base_MemberShip.UserCreated = this.UserCreated.Trim();
            if (this.UserUpdated != null)
                this.base_MemberShip.UserUpdated = this.UserUpdated.Trim();
            this.base_MemberShip.DateCreated = this.DateCreated;
            this.base_MemberShip.DateUpdated = this.DateUpdated;
            if (this.GuestResource != null)
                this.base_MemberShip.GuestResource = this.GuestResource.Trim();
            this.base_MemberShip.TotalCutOffThreshold = this.TotalCutOffThreshold;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_MemberShip.Id;
            this._idCard = this.base_MemberShip.IdCard;
            this._idCardImg = this.base_MemberShip.IdCardImg;
            this._guestId = this.base_MemberShip.GuestId;
            this._memberType = this.base_MemberShip.MemberType;
            this._status = this.base_MemberShip.Status;
            this._isPurged = this.base_MemberShip.IsPurged;
            this._userCreated = this.base_MemberShip.UserCreated;
            this._userUpdated = this.base_MemberShip.UserUpdated;
            this._dateCreated = this.base_MemberShip.DateCreated;
            this._dateUpdated = this.base_MemberShip.DateUpdated;
            this._guestResource = this.base_MemberShip.GuestResource;
            this._totalCutOffThreshold = this.base_MemberShip.TotalCutOffThreshold;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_MemberShip.Id;
            this.IdCard = this.base_MemberShip.IdCard;
            this.IdCardImg = this.base_MemberShip.IdCardImg;
            this.GuestId = this.base_MemberShip.GuestId;
            this.MemberType = this.base_MemberShip.MemberType;
            this.Status = this.base_MemberShip.Status;
            this.IsPurged = this.base_MemberShip.IsPurged;
            this.UserCreated = this.base_MemberShip.UserCreated;
            this.UserUpdated = this.base_MemberShip.UserUpdated;
            this.DateCreated = this.base_MemberShip.DateCreated;
            this.DateUpdated = this.base_MemberShip.DateUpdated;
            this.GuestResource = this.base_MemberShip.GuestResource;
            this.TotalCutOffThreshold = this.base_MemberShip.TotalCutOffThreshold;
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
                    case "IdCard":
                        break;
                    case "IdCardImg":
                        break;
                    case "GuestId":
                        break;
                    case "MemberType":
                        break;
                    case "Status":
                        break;
                    case "IsPurged":
                        break;
                    case "UserCreated":
                        break;
                    case "UserUpdated":
                        break;
                    case "DateCreated":
                        break;
                    case "DateUpdated":
                        break;
                    case "GuestResource":
                        break;
                    case "TotalCashReward":
                        break;
                    case "TotalPointReward":
                        break;
                    case "TotalPercentReward":
                        break;
                    case "CashRewardCode":
                        break;
                    case "CashRewardImg":
                        break;
                    case "PointRewardCode":
                        break;
                    case "PointRewardImg":
                        break;
                    case "PercentRewardCode":
                        break;
                    case "PercentRewardImg":
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