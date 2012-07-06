using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class UserModelBase: ModelBase
    {
        #region Constructors
        public UserModelBase()
        {

        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _userID;
        public int UserID
        {
            get { return _userID; }
            set
            {
                if (_userID != value)
                {
                    this.OnUserIDChanging(value);
                    _userID = value;
                    RaisePropertyChanged(() => UserID);
                    this.OnUserIDChanged();
                }
            }
        }

        public virtual void OnUserIDChanging(int value) { }
        protected virtual void OnUserIDChanged() { }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    this.OnUserNameChanging(value);
                    _userName = value;
                    RaisePropertyChanged(() => UserName);
                    this.OnUserNameChanged();
                }
            }
        }

        protected virtual void OnUserNameChanging(string value) { }
        protected virtual void OnUserNameChanged() { }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    this.OnPasswordChanging(value);
                    _password = value;
                    RaisePropertyChanged(() => Password);
                    this.OnPasswordChanged();
                }
            }
        }

        protected virtual void OnPasswordChanging(string value) { }
        protected virtual void OnPasswordChanged() { }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    this.OnFullNameChanging(value);
                    _fullName = value;
                    RaisePropertyChanged(() => FullName);
                    this.OnFullNameChanged();
                }
            }
        }

        protected virtual void OnFullNameChanging(string value) { }
        protected virtual void OnFullNameChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private DateTime _lastLogin;
        public DateTime LastLogin
        {
            get { return _lastLogin; }
            set
            {
                if (_lastLogin != value)
                {
                    this.OnLastLoginChanging(value);
                    _lastLogin = value;
                    RaisePropertyChanged(() => LastLogin);
                    this.OnLastLoginChanged();
                }
            }
        }

        protected virtual void OnLastLoginChanging(DateTime value) { }
        protected virtual void OnLastLoginChanged() { }

        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //private List<LessonModel> _lessonCollection;
        //public List<LessonModel> LessonCollection
        //{
        //    get { return _lessonCollection; }
        //    set
        //    {
        //        if (_lessonCollection != value)
        //        {
        //            this.OnLessonCollectionChanging(value);
        //            _lessonCollection = value;
        //            RaisePropertyChanged(() => LessonCollection);
        //            this.OnLessonCollectionChanged();
        //        }
        //    }
        //}

        //protected virtual void OnLessonCollectionChanging(List<LessonModel> value) { }
        //protected virtual void OnLessonCollectionChanged() { }






        #endregion
    }
}
