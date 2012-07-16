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
    /// Model for table Lesson 
    /// </summary>
    public partial class LessonModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public LessonModel()
        {
            this.IsNew = true;
            this.Lesson = new Lesson();
        }

        // Default contructor that set entity to field
        public LessonModel(Lesson lesson)
        {
            this.Lesson = lesson;
            ToModel();
        }

        #endregion

        #region Entity Properties

        public Lesson Lesson { get; private set; }

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
                this.Lesson.LessonID = this.LessonID;
            this.Lesson.LessonName = this.LessonName;
            this.Lesson.Description = this.Description;
            this.Lesson.CategoryID = this.CategoryID;
            this.Lesson.CardID = this.CardID;
        }

        public void ToModel()
        {
            this.LessonID = this.Lesson.LessonID;
            this.LessonName = this.Lesson.LessonName;
            this.Description = this.Lesson.Description;
            this.CategoryID = this.Lesson.CategoryID;
            this.CardID = this.Lesson.CardID;
        }

        #endregion

        #region Primitive Properties

        protected string _lessonID;
        /// <summary>
        /// Gets or sets the LessonID.
        /// </summary>
        public string LessonID
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

        protected string _lessonName;
        /// <summary>
        /// Gets or sets the LessonName.
        /// </summary>
        public string LessonName
        {
            get { return this._lessonName; }
            set
            {
                if (this._lessonName != value)
                {
                    this.IsDirty = true;
                    this._lessonName = value;
                    RaisePropertyChanged(() => LessonName);
                }
            }
        }

        protected string _description;
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description
        {
            get { return this._description; }
            set
            {
                if (this._description != value)
                {
                    this.IsDirty = true;
                    this._description = value;
                    RaisePropertyChanged(() => Description);
                }
            }
        }

        protected string _categoryID;
        /// <summary>
        /// Gets or sets the CategoryID.
        /// </summary>
        public string CategoryID
        {
            get { return this._categoryID; }
            set
            {
                if (this._categoryID != value)
                {
                    this.IsDirty = true;
                    this._categoryID = value;
                    RaisePropertyChanged(() => CategoryID);
                }
            }
        }

        protected string _cardID;
        /// <summary>
        /// Gets or sets the CardID.
        /// </summary>
        public string CardID
        {
            get { return this._cardID; }
            set
            {
                if (this._cardID != value)
                {
                    this.IsDirty = true;
                    this._cardID = value;
                    RaisePropertyChanged(() => CardID);
                }
            }
        }


        #endregion

        #region Custom Code
        #region Properties
        private bool _isBackSide;
        /// <summary>
        /// This is Extend Properties
        ///<para> Gets or sets the property value.</para>
        /// </summary>
        public bool IsBackSide
        {
            get { return _isBackSide; }
            set
            {
                if (_isBackSide != value)
                {
                    _isBackSide = value;
                    RaisePropertyChanged(() => IsBackSide);

                }
            }
        }


        private ObservableCollection<BackSideModel> _backSideCollection;
        /// <summary>
        /// Extention property.
        /// </summary>
        public ObservableCollection<BackSideModel> BackSideCollection
        {
            get
            {

                return _backSideCollection;

            }
            set
            {
                if (_backSideCollection != value)
                {
                    _backSideCollection = value;
                    RaisePropertyChanged(() => BackSideCollection);
                }
            }
        }


        #endregion

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
                    case "LessonName":
                        if (string.IsNullOrWhiteSpace(LessonName))
                            message = "Lesson Name is required!";
                        break;
                    case "Description":
                        if (string.IsNullOrWhiteSpace(Description))
                            message = "Description is required!";
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
