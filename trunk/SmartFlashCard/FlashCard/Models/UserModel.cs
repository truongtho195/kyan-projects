//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FlashCard.Models;
using FlashCard.Database;


namespace FlashCard.Models
{
    /// <summary>
    /// Model for table User 
    /// </summary>
    public partial class UserModel : ViewModelBase
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
        }

        #endregion

        #region Entity Properties

        public User User { get; private set; }

        public bool IsNew { get; private set; }
        public bool IsDirty { get; private set; }
        public bool Deleted { get; set; }
        public bool Checked { get; set; }
        
        public void EndUpdate()
        {
            IsNew = false;
            IsDirty = false;
        }
        

        #endregion

        #region Primitive Properties

        public long UserID
        {
            get { return this.User.UserID; }
            set
            {
                if (this.User.UserID != value)
                {
                    this.IsDirty = true;
                    this.User.UserID = value;
                    RaisePropertyChanged(() => UserID);
                }
            }
        }
        public string UserName
        {
            get { return this.User.UserName; }
            set
            {
                if (this.User.UserName != value)
                {
                    this.IsDirty = true;
                    this.User.UserName = value;
                    RaisePropertyChanged(() => UserName);
                }
            }
        }
        public string Password
        {
            get { return this.User.Password; }
            set
            {
                if (this.User.Password != value)
                {
                    this.IsDirty = true;
                    this.User.Password = value;
                    RaisePropertyChanged(() => Password);
                }
            }
        }
        public string FullName
        {
            get { return this.User.FullName; }
            set
            {
                if (this.User.FullName != value)
                {
                    this.IsDirty = true;
                    this.User.FullName = value;
                    RaisePropertyChanged(() => FullName);
                }
            }
        }
        public Nullable<System.DateTime> LastLogin
        {
            get { return this.User.LastLogin; }
            set
            {
                if (this.User.LastLogin != value)
                {
                    this.IsDirty = true;
                    this.User.LastLogin = value;
                    RaisePropertyChanged(() => LastLogin);
                }
            }
        }

        #endregion

        #region all the custom code


        #endregion
    }
}
