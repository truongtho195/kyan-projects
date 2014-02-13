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
    /// Model for table base_UserRight
    /// </summary>
    [Serializable]
    public partial class base_UserRightModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_UserRightModel()
        {
            this.IsNew = true;
            this.base_UserRight = new base_UserRight();
        }

        // Default constructor that set entity to field
        public base_UserRightModel(base_UserRight base_userright, bool isRaiseProperties = false)
        {
            this.base_UserRight = base_userright;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_UserRight base_UserRight { get; private set; }

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

        protected string _code;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Code</param>
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
        /// <param>Gets or sets the Name</param>
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

        protected Nullable<short> _groupId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the GroupId</param>
        /// </summary>
        public Nullable<short> GroupId
        {
            get { return this._groupId; }
            set
            {
                if (this._groupId != value)
                {
                    this.IsDirty = true;
                    this._groupId = value;
                    OnPropertyChanged(() => GroupId);
                    PropertyChangedCompleted(() => GroupId);
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
                this.base_UserRight.Id = this.Id;
            if (this.Code != null)
                this.base_UserRight.Code = this.Code.Trim();
            if (this.Name != null)
                this.base_UserRight.Name = this.Name.Trim();
            this.base_UserRight.GroupId = this.GroupId;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_UserRight.Id;
            this._code = this.base_UserRight.Code;
            this._name = this.base_UserRight.Name;
            this._groupId = this.base_UserRight.GroupId;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_UserRight.Id;
            this.Code = this.base_UserRight.Code;
            this.Name = this.base_UserRight.Name;
            this.GroupId = this.base_UserRight.GroupId;
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
                    case "GroupId":
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