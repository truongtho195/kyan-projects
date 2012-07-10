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
    /// Model for table Lesson 
    /// </summary>
    public partial class LessonModel : ViewModelBase
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
        }

        #endregion

        #region Entity Properties

        public Lesson Lesson { get; private set; }

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

        public long LessonID
        {
            get { return this.Lesson.LessonID; }
            set
            {
                if (this.Lesson.LessonID != value)
                {
                    this.IsDirty = true;
                    this.Lesson.LessonID = value;
                    RaisePropertyChanged(() => LessonID);
                }
            }
        }
        public string LessonName
        {
            get { return this.Lesson.LessonName; }
            set
            {
                if (this.Lesson.LessonName != value)
                {
                    this.IsDirty = true;
                    this.Lesson.LessonName = value;
                    RaisePropertyChanged(() => LessonName);
                }
            }
        }
        public string Description
        {
            get { return this.Lesson.Description; }
            set
            {
                if (this.Lesson.Description != value)
                {
                    this.IsDirty = true;
                    this.Lesson.Description = value;
                    RaisePropertyChanged(() => Description);
                }
            }
        }
        public Nullable<long> CategoryID
        {
            get { return this.Lesson.CategoryID; }
            set
            {
                if (this.Lesson.CategoryID != value)
                {
                    this.IsDirty = true;
                    this.Lesson.CategoryID = value;
                    RaisePropertyChanged(() => CategoryID);
                }
            }
        }
        public Nullable<bool> IsActived
        {
            get { return this.Lesson.IsActived; }
            set
            {
                if (this.Lesson.IsActived != value)
                {
                    this.IsDirty = true;
                    this.Lesson.IsActived = value;
                    RaisePropertyChanged(() => IsActived);
                }
            }
        }
        public Nullable<long> KindID
        {
            get { return this.Lesson.KindID; }
            set
            {
                if (this.Lesson.KindID != value)
                {
                    this.IsDirty = true;
                    this.Lesson.KindID = value;
                    RaisePropertyChanged(() => KindID);
                }
            }
        }

        #endregion

        #region all the custom code
        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private bool _isBackSide;
        public bool IsBackSide
        {
            get { return _isBackSide; }
            set
            {
                if (_isBackSide != value)
                {
                    _isBackSide = value;
                    RaisePropertyChanged(() => IsBackSide);
                    RaisePropertyChanged(() => SideName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _sideName;
        public string SideName
        {
            get
            {
                if (!IsBackSide)
                    _sideName = "Front Side";
                else
                    _sideName = "Back Side";
                return _sideName;
            }
            set
            {
                if (_sideName != value)
                {
                    _sideName = value;
                    RaisePropertyChanged(() => SideName);
                }
            }
        }

        private BackSideModel _backSideModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public BackSideModel BackSideModel
        {
            get { return _backSideModel; }
            set
            {
                if (_backSideModel != value)
                {
                    _backSideModel = value;
                    RaisePropertyChanged(() => BackSideModel);
                }
            }
        }

        private bool _isEditing;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    RaisePropertyChanged(() => IsEditing);
                }
            }
        }

        #region IsNewType
        private bool _isNewType;
        /// <summary>
        /// Gets or sets the IsNewType.
        /// </summary>
        public bool IsNewType
        {
            get { return _isNewType; }
            set
            {
                if (_isNewType != value)
                {
                    _isNewType = value;
                    RaisePropertyChanged(() => IsNewType);
                }
            }
        }
        #endregion

        #region IsNewCate
        private bool _isNewCate;
        /// <summary>
        /// Gets or sets the IsNewCate.
        /// </summary>
        public bool IsNewCate
        {
            get { return _isNewCate; }
            set
            {
                if (_isNewCate != value)
                {
                    _isNewCate = value;
                    RaisePropertyChanged(() => IsNewCate);
                }
            }
        }
        #endregion

        #endregion

        #endregion
    }
}
