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
    /// Model for table Study 
    /// </summary>
    public partial class StudyModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public StudyModel()
        {
            this.IsNew = true;
            this.Study = new Study();
        }

        // Default contructor that set entity to field
        public StudyModel(Study study)
        {
            this.Study = study;
            ToModel();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public Study Study { get; private set; }

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
                this.Study.StudyID = this.StudyID;
            this.Study.LastStudyDate = this.LastStudyDate;
        }

        /// <summary>
        ///Public Method
        ///<para> Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this.StudyID = this.Study.StudyID;
            this.LastStudyDate = this.Study.LastStudyDate;
        }

        #endregion

        #region Primitive Properties

        protected System.Guid _studyID;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the StudyID.</para>
        /// </summary>
        public System.Guid StudyID
        {
            get { return this._studyID; }
            set
            {
                if (this._studyID != value)
                {
                    this.IsDirty = true;
                    this._studyID = value;
                    RaisePropertyChanged(() => StudyID);
                }
            }
        }

        protected System.DateTime _lastStudyDate;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the LastStudyDate.</para>
        /// </summary>
        public System.DateTime LastStudyDate
        {
            get { return this._lastStudyDate; }
            set
            {
                if (this._lastStudyDate != value)
                {
                    this.IsDirty = true;
                    this._lastStudyDate = value;
                    RaisePropertyChanged(() => LastStudyDate);
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
