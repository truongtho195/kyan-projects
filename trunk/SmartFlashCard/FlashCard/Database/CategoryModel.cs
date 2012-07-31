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
    /// Model for table Category 
    /// </summary>
    public partial class CategoryModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public CategoryModel()
        {
            this.IsNew = true;
            this.Category = new Category();
        }

        // Default contructor that set entity to field
        public CategoryModel(Category category)
        {
            this.Category = category;
            ToModel();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public Category Category { get; private set; }

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
                this.Category.CategoryID = this.CategoryID;
            this.Category.CategoryName = this.CategoryName.Trim();
            this.Category.CategoryOf = this.CategoryOf;
        }
        

        /// <summary>
        ///Public Method
        ///<para> Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this.CategoryID = this.Category.CategoryID;
            this.CategoryName = this.Category.CategoryName;
            this.CategoryOf = this.Category.CategoryOf;
        }

        #endregion

        #region Primitive Properties

        protected string _categoryID;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the CategoryID.</para>
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

        protected string _categoryName;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the CategoryName.</para>
        /// </summary>
        public string CategoryName
        {
            get { return this._categoryName; }
            set
            {
                if (this._categoryName != value)
                {
                    this.IsDirty = true;
                    this._categoryName = value;
                    RaisePropertyChanged(() => CategoryName);
                }
            }
        }

        protected int _categoryOf;
        /// <summary>
        ///Property Model
        ///<para> Gets or sets the CategoryOf.</para>
        /// </summary>
        public int CategoryOf
        {
            get { return this._categoryOf; }
            set
            {
                if (this._categoryOf != value)
                {
                    this.IsDirty = true;
                    this._categoryOf = value;
                    RaisePropertyChanged(() => CategoryOf);
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
