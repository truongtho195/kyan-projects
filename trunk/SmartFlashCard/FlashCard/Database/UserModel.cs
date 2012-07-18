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
using FlashCard.Models;


namespace FlashCard.Database
{
    /// <summary>
    /// Model for table User 
    /// </summary>
    public partial class UserModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public UserModel()
        {
            this.IsNew = true;
            this.User = new User();
        }

        // Default contructor that set entity to field
        public UserModel(User user)
        {
            this.User = user;
            ToModel();
        }

        #endregion

        #region Entity Properties

        public User User { get; private set; }

        protected bool _isNew;
        /// <summary>
        /// Gets or sets the IsNew
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    RaisePropertyChanged(() => IsNew);
                }
            }
        }

        protected bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }

        protected bool _isDeleted;
        /// <summary>
        /// Gets or sets the IsDeleted
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    RaisePropertyChanged(() => IsDeleted);
                }
            }
        }

        protected bool _isChecked;
        /// <summary>
        /// Gets or sets the IsChecked
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged(() => IsChecked);
                }
            }
        }

        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        public void ToEntity()
        {
            if (IsNew)
                this.User.UserID = this.UserID;
            this.User.UserName = this.UserName;
            this.User.Password = this.Password;
            this.User.FullName = this.FullName;
            this.User.LastLogin = this.LastLogin;
        }

        public void ToModel()
        {
            this.UserID = this.User.UserID;
            this.UserName = this.User.UserName;
            this.Password = this.User.Password;
            this.FullName = this.User.FullName;
            this.LastLogin = this.User.LastLogin;
        }

        #endregion

        #region Primitive Properties

        protected long _userID;
        /// <summary>
        /// Gets or sets the UserID.
        /// </summary>
        public long UserID
        {
            get { return this._userID; }
            set
            {
                if (this._userID != value)
                {
                    this.IsDirty = true;
                    this._userID = value;
                    RaisePropertyChanged(() => UserID);
                }
            }
        }

        protected string _userName;
        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        public string UserName
        {
            get { return this._userName; }
            set
            {
                if (this._userName != value)
                {
                    this.IsDirty = true;
                    this._userName = value;
                    RaisePropertyChanged(() => UserName);
                }
            }
        }

        protected string _password;
        /// <summary>
        /// Gets or sets the Password.
        /// </summary>
        public string Password
        {
            get { return this._password; }
            set
            {
                if (this._password != value)
                {
                    this.IsDirty = true;
                    this._password = value;
                    RaisePropertyChanged(() => Password);
                }
            }
        }

        protected string _fullName;
        /// <summary>
        /// Gets or sets the FullName.
        /// </summary>
        public string FullName
        {
            get { return this._fullName; }
            set
            {
                if (this._fullName != value)
                {
                    this.IsDirty = true;
                    this._fullName = value;
                    RaisePropertyChanged(() => FullName);
                }
            }
        }

        protected Nullable<System.DateTime> _lastLogin;
        /// <summary>
        /// Gets or sets the LastLogin.
        /// </summary>
        public Nullable<System.DateTime> LastLogin
        {
            get { return this._lastLogin; }
            set
            {
                if (this._lastLogin != value)
                {
                    this.IsDirty = true;
                    this._lastLogin = value;
                    RaisePropertyChanged(() => LastLogin);
                }
            }
        }


        #endregion

        #region Custom Code

        #region DataErrorInfo
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        private Dictionary<string, string> _errors = new Dictionary<string, string>();
        public Dictionary<string, string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                if (_errors != value)
                {
                    _errors = value;
                    RaisePropertyChanged(() => Errors);
                }
            }
        }
        public string this[string columnName]
        {
            get
            {
                string message = String.Empty;
                this.Errors.Remove(columnName);
                switch (columnName)
                {

                    case "Content":

                        break;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    this.Errors.Add(columnName, message);
                }
                return message;
            }
        }
        #endregion
        #endregion
    }
}