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


namespace FlashCard.Database
{
    /// <summary>
    /// Model for table Category 
    /// </summary>
    public partial class CategoryModel : ViewModelBase
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
        }

        #endregion

        #region Entity Properties

        public Category Category { get; private set; }

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

        public string CategoryID
        {
            get { return this.Category.CategoryID; }
            set
            {
                if (this.Category.CategoryID != value)
                {
                    this.IsDirty = true;
                    this.Category.CategoryID = value;
                    RaisePropertyChanged(() => CategoryID);
                }
            }
        }
        public string CategoryName
        {
            get { return this.Category.CategoryName; }
            set
            {
                if (this.Category.CategoryName != value)
                {
                    this.IsDirty = true;
                    this.Category.CategoryName = value;
                    RaisePropertyChanged(() => CategoryName);
                }
            }
        }
        public int CategoryOf
        {
            get { return this.Category.CategoryOf; }
            set
            {
                if (this.Category.CategoryOf != value)
                {
                    this.IsDirty = true;
                    this.Category.CategoryOf = value;
                    RaisePropertyChanged(() => CategoryOf);
                }
            }
        }

        #endregion

        #region all the custom code


        #endregion
    }
}
