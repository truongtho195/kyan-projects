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
    /// Model for table UserLesson 
    /// </summary>
    public partial class UserLessonModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public UserLessonModel()
        {
            this.IsNew = true;
            this.UserLesson = new UserLesson();
        }

        // Default contructor that set entity to field
        public UserLessonModel(UserLesson userlesson)
        {
            this.UserLesson = userlesson;
            ToModel();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public UserLesson UserLesson { get; private set; }

        protected bool _isNew;
        /// <summary>
        /// Property Base
        /// <para> Gets or sets the IsNew </para>
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
        /// Property Base
        /// <para>Gets or sets the IsDirty</para>
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
        /// Property Base
        ///<para>Gets or sets the IsDeleted</para>
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
        /// Property Base
        ///<para> Gets or sets the IsChecked</para>
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

        
        /// <summary>
        ///<para>Public Method</para>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        /// <summary>
        ///Public Method
        ///<para> Method for set PropertyModel to Entity</para>
        /// </summary>
        public void ToEntity()
        {
            if (IsNew)
                this.UserLesson.UserLessonID = this.UserLessonID;
            this.UserLesson.UserID = this.UserID;
            this.UserLesson.LessonID = this.LessonID;
        }

        /// <summary>
        ///Public Method
        ///<para> Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this.UserLessonID = this.UserLesson.UserLessonID;
            this.UserID = this.UserLesson.UserID;
            this.LessonID = this.UserLesson.LessonID;
        }

        #endregion

        #region Primitive Properties

        protected long _userLessonID;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the UserLessonID.</para>
        /// </summary>
        public long UserLessonID
        {
            get { return this._userLessonID; }
            set
            {
                if (this._userLessonID != value)
                {
                    this.IsDirty = true;
                    this._userLessonID = value;
                    RaisePropertyChanged(() => UserLessonID);
                }
            }
        }

        protected Nullable<long> _userID;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the UserID.</para>
        /// </summary>
        public Nullable<long> UserID
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

        protected Nullable<long> _lessonID;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the LessonID.</para>
        /// </summary>
        public Nullable<long> LessonID
        {
            get { return this._lessonID; }
            set
            {
                if (this._lessonID != value)
                {
                    this.IsDirty = true;
                    this._lessonID = value;
                    RaisePropertyChanged(() => LessonID);
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