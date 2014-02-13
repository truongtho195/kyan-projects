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
    /// Model for table base_GuestReward
    /// </summary>
    [Serializable]
    public partial class base_GuestRewardModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_GuestRewardModel()
        {
            this.IsNew = true;
            this.base_GuestReward = new base_GuestReward();
        }

        // Default constructor that set entity to field
        public base_GuestRewardModel(base_GuestReward base_guestreward, bool isRaiseProperties = false)
        {
            this.base_GuestReward = base_guestreward;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_GuestReward base_GuestReward { get; private set; }

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

        protected int _rewardId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the RewardId</param>
        /// </summary>
        public int RewardId
        {
            get { return this._rewardId; }
            set
            {
                if (this._rewardId != value)
                {
                    this.IsDirty = true;
                    this._rewardId = value;
                    OnPropertyChanged(() => RewardId);
                    PropertyChangedCompleted(() => RewardId);
                }
            }
        }

        protected bool _isApply;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IsApply</param>
        /// </summary>
        public bool IsApply
        {
            get { return this._isApply; }
            set
            {
                if (this._isApply != value)
                {
                    this.IsDirty = true;
                    this._isApply = value;
                    OnPropertyChanged(() => IsApply);
                    PropertyChangedCompleted(() => IsApply);
                }
            }
        }

        protected Nullable<System.DateTime> _appliedDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the AppliedDate</param>
        /// </summary>
        public Nullable<System.DateTime> AppliedDate
        {
            get { return this._appliedDate; }
            set
            {
                if (this._appliedDate != value)
                {
                    this.IsDirty = true;
                    this._appliedDate = value;
                    OnPropertyChanged(() => AppliedDate);
                    PropertyChangedCompleted(() => AppliedDate);
                }
            }
        }

        protected Nullable<System.DateTime> _earnedDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the EarnedDate</param>
        /// </summary>
        public Nullable<System.DateTime> EarnedDate
        {
            get { return this._earnedDate; }
            set
            {
                if (this._earnedDate != value)
                {
                    this.IsDirty = true;
                    this._earnedDate = value;
                    OnPropertyChanged(() => EarnedDate);
                    PropertyChangedCompleted(() => EarnedDate);
                }
            }
        }

        protected decimal _rewardValueEarned;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the RewardValueEarned</param>
        /// </summary>
        public decimal RewardValueEarned
        {
            get { return this._rewardValueEarned; }
            set
            {
                if (this._rewardValueEarned != value)
                {
                    this.IsDirty = true;
                    this._rewardValueEarned = value;
                    OnPropertyChanged(() => RewardValueEarned);
                    PropertyChangedCompleted(() => RewardValueEarned);
                }
            }
        }

        protected decimal _rewardValueApplied;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the RewardValueApplied</param>
        /// </summary>
        public decimal RewardValueApplied
        {
            get { return this._rewardValueApplied; }
            set
            {
                if (this._rewardValueApplied != value)
                {
                    this.IsDirty = true;
                    this._rewardValueApplied = value;
                    OnPropertyChanged(() => RewardValueApplied);
                    PropertyChangedCompleted(() => RewardValueApplied);
                }
            }
        }

        protected decimal _totalRewardRedeemed;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the TotalRewardRedeemed</param>
        /// </summary>
        public decimal TotalRewardRedeemed
        {
            get { return this._totalRewardRedeemed; }
            set
            {
                if (this._totalRewardRedeemed != value)
                {
                    this.IsDirty = true;
                    this._totalRewardRedeemed = value;
                    OnPropertyChanged(() => TotalRewardRedeemed);
                    PropertyChangedCompleted(() => TotalRewardRedeemed);
                }
            }
        }

        protected decimal _totalRewardReturned;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the TotalRewardReturned</param>
        /// </summary>
        public decimal TotalRewardReturned
        {
            get { return this._totalRewardReturned; }
            set
            {
                if (this._totalRewardReturned != value)
                {
                    this.IsDirty = true;
                    this._totalRewardReturned = value;
                    OnPropertyChanged(() => TotalRewardReturned);
                    PropertyChangedCompleted(() => TotalRewardReturned);
                }
            }
        }

        protected decimal _rewardBalance;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the RewardBalance</param>
        /// </summary>
        public decimal RewardBalance
        {
            get { return this._rewardBalance; }
            set
            {
                if (this._rewardBalance != value)
                {
                    this.IsDirty = true;
                    this._rewardBalance = value;
                    OnPropertyChanged(() => RewardBalance);
                    PropertyChangedCompleted(() => RewardBalance);
                }
            }
        }

        protected string _remark;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Remark</param>
        /// </summary>
        public string Remark
        {
            get { return this._remark; }
            set
            {
                if (this._remark != value)
                {
                    this.IsDirty = true;
                    this._remark = value;
                    OnPropertyChanged(() => Remark);
                    PropertyChangedCompleted(() => Remark);
                }
            }
        }

        protected Nullable<System.DateTime> _activedDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ActivedDate</param>
        /// </summary>
        public Nullable<System.DateTime> ActivedDate
        {
            get { return this._activedDate; }
            set
            {
                if (this._activedDate != value)
                {
                    this.IsDirty = true;
                    this._activedDate = value;
                    OnPropertyChanged(() => ActivedDate);
                    PropertyChangedCompleted(() => ActivedDate);
                }
            }
        }

        protected Nullable<System.DateTime> _expireDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ExpireDate</param>
        /// </summary>
        public Nullable<System.DateTime> ExpireDate
        {
            get { return this._expireDate; }
            set
            {
                if (this._expireDate != value)
                {
                    this.IsDirty = true;
                    this._expireDate = value;
                    OnPropertyChanged(() => ExpireDate);
                    PropertyChangedCompleted(() => ExpireDate);
                }
            }
        }

        protected string _reason;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Reason</param>
        /// </summary>
        public string Reason
        {
            get { return this._reason; }
            set
            {
                if (this._reason != value)
                {
                    this.IsDirty = true;
                    this._reason = value;
                    OnPropertyChanged(() => Reason);
                    PropertyChangedCompleted(() => Reason);
                }
            }
        }

        protected Nullable<short> _status;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Status</param>
        /// </summary>
        public Nullable<short> Status
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

        protected string _scanCode;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ScanCode</param>
        /// </summary>
        public string ScanCode
        {
            get { return this._scanCode; }
            set
            {
                if (this._scanCode != value)
                {
                    this.IsDirty = true;
                    this._scanCode = value;
                    OnPropertyChanged(() => ScanCode);
                    PropertyChangedCompleted(() => ScanCode);
                }
            }
        }

        protected byte[] _scanCodeImg;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ScanCodeImg</param>
        /// </summary>
        public byte[] ScanCodeImg
        {
            get { return this._scanCodeImg; }
            set
            {
                if (this._scanCodeImg != value)
                {
                    this.IsDirty = true;
                    this._scanCodeImg = value;
                    OnPropertyChanged(() => ScanCodeImg);
                    PropertyChangedCompleted(() => ScanCodeImg);
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
                this.base_GuestReward.Id = this.Id;
            this.base_GuestReward.GuestId = this.GuestId;
            this.base_GuestReward.RewardId = this.RewardId;
            this.base_GuestReward.IsApply = this.IsApply;
            this.base_GuestReward.AppliedDate = this.AppliedDate;
            this.base_GuestReward.EarnedDate = this.EarnedDate;
            this.base_GuestReward.RewardValueEarned = this.RewardValueEarned;
            this.base_GuestReward.RewardValueApplied = this.RewardValueApplied;
            this.base_GuestReward.TotalRewardRedeemed = this.TotalRewardRedeemed;
            this.base_GuestReward.TotalRewardReturned = this.TotalRewardReturned;
            this.base_GuestReward.RewardBalance = this.RewardBalance;
            if (this.Remark != null)
                this.base_GuestReward.Remark = this.Remark.Trim();
            this.base_GuestReward.ActivedDate = this.ActivedDate;
            this.base_GuestReward.ExpireDate = this.ExpireDate;
            if (this.Reason != null)
                this.base_GuestReward.Reason = this.Reason.Trim();
            this.base_GuestReward.Status = this.Status;
            if (this.ScanCode != null)
                this.base_GuestReward.ScanCode = this.ScanCode.Trim();
            this.base_GuestReward.ScanCodeImg = this.ScanCodeImg;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_GuestReward.Id;
            this._guestId = this.base_GuestReward.GuestId;
            this._rewardId = this.base_GuestReward.RewardId;
            this._isApply = this.base_GuestReward.IsApply;
            this._appliedDate = this.base_GuestReward.AppliedDate;
            this._earnedDate = this.base_GuestReward.EarnedDate;
            this._rewardValueEarned = this.base_GuestReward.RewardValueEarned;
            this._rewardValueApplied = this.base_GuestReward.RewardValueApplied;
            this._totalRewardRedeemed = this.base_GuestReward.TotalRewardRedeemed;
            this._totalRewardReturned = this.base_GuestReward.TotalRewardReturned;
            this._rewardBalance = this.base_GuestReward.RewardBalance;
            this._remark = this.base_GuestReward.Remark;
            this._activedDate = this.base_GuestReward.ActivedDate;
            this._expireDate = this.base_GuestReward.ExpireDate;
            this._reason = this.base_GuestReward.Reason;
            this._status = this.base_GuestReward.Status;
            this._scanCode = this.base_GuestReward.ScanCode;
            this._scanCodeImg = this.base_GuestReward.ScanCodeImg;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_GuestReward.Id;
            this.GuestId = this.base_GuestReward.GuestId;
            this.RewardId = this.base_GuestReward.RewardId;
            this.IsApply = this.base_GuestReward.IsApply;
            this.AppliedDate = this.base_GuestReward.AppliedDate;
            this.EarnedDate = this.base_GuestReward.EarnedDate;
            this.RewardValueEarned = this.base_GuestReward.RewardValueEarned;
            this.RewardValueApplied = this.base_GuestReward.RewardValueApplied;
            this.TotalRewardRedeemed = this.base_GuestReward.TotalRewardRedeemed;
            this.TotalRewardReturned = this.base_GuestReward.TotalRewardReturned;
            this.RewardBalance = this.base_GuestReward.RewardBalance;
            this.Remark = this.base_GuestReward.Remark;
            this.ActivedDate = this.base_GuestReward.ActivedDate;
            this.ExpireDate = this.base_GuestReward.ExpireDate;
            this.Reason = this.base_GuestReward.Reason;
            this.Status = this.base_GuestReward.Status;
            this.ScanCode = this.base_GuestReward.ScanCode;
            this.ScanCodeImg = this.base_GuestReward.ScanCodeImg;
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
                    case "GuestId":
                        break;
                    case "RewardId":
                        break;
                    case "Amount":
                        break;
                    case "IsApply":
                        break;
                    case "EarnedDate":
                        break;
                    case "AppliedDate":
                        break;
                    case "RewardValue":
                        break;
                    case "SaleOrderResource":
                        break;
                    case "SaleOrderNo":
                        break;
                    case "Remark":
                        break;
                    case "ActivedDate":
                        break;
                    case "ExpireDate":
                        break;
                    case "Reason":
                        break;
                    case "Status":
                        break;
                    case "RewardSetupAmount":
                        break;
                    case "RewardSetupUnit":
                        break;
                    case "Sign":
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