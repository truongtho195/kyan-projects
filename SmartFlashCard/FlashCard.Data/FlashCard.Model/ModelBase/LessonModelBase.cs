﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace FlashCard.Model
{
    public class LessonModelBase : ModelBase 
    {
        #region Constructors
        public LessonModelBase()
        {

        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _lessonID;
        public int LessonID
        {
            get { return _lessonID; }
            set
            {
                if (_lessonID != value)
                {
                    this.OnLessonIDChanging(value);
                    _lessonID = value;
                    RaisePropertyChanged(() => LessonID);
                    this.OnChanged();
                    this.OnLessonIDChanged();
                }
            }
        }

        protected virtual void OnLessonIDChanging(int value) { }
        protected virtual void OnLessonIDChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _lessonName;
        public string LessonName
        {
            get { return _lessonName; }
            set
            {
                if (_lessonName != value)
                {
                    this.OnLessonNameChanging(value);
                    _lessonName = value;
                    RaisePropertyChanged(() => LessonName);
                    this.OnChanged();
                    this.OnLessonNameChanged();
                }
            }
        }

        protected virtual void OnLessonNameChanging(string value) { }
        protected virtual void OnLessonNameChanged() { }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private FlowDocument _description;
        public FlowDocument Description
        {
            get { return _description; }
            set
            {
                
                //if (_description != value)
                //{
                    this.OnDescriptionChanging(value);
                    _description = value;
                    RaisePropertyChanged(() => Description);
                    this.OnChanged();
                    this.OnDescriptionChanged();
                //}
            }
        }

        protected virtual void OnDescriptionChanging(FlowDocument value) { if (_description != null) _description.Blocks.Clear(); }
        protected virtual void OnDescriptionChanged() { }




        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _categoryID;
        public int CategoryID
        {
            get { return _categoryID; }
            set
            {
                if (_categoryID != value)
                {
                    this.OnCategoryIDChanging(value);
                    _categoryID = value;
                    RaisePropertyChanged(() => CategoryID);
                    this.OnChanged();
                    this.OnCategoryIDChanged();
                }
            }
        }

        protected virtual void OnCategoryIDChanging(int value) { }
        protected virtual void OnCategoryIDChanged() { }



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private CategoryModel _categoryModel;
        public CategoryModel CategoryModel
        {
            get { return _categoryModel; }
            set
            {
                if (_categoryModel != value)
                {
                    this.OnCategoryModelChanging(value);
                    _categoryModel = value;
                    RaisePropertyChanged(() => CategoryModel);
                    ////this.OnModelChanged(CategoryModel.IsEdit);
                    this.OnChanged();
                    this.OnCategoryModelChanged();
                }
            }
        }

        protected virtual void OnCategoryModelChanging(CategoryModel value) { }
        protected virtual void OnCategoryModelChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _typeID;
        public int TypeID
        {
            get { return _typeID; }
            set
            {
                if (_typeID != value)
                {
                    this.OnTypeIDChanging(value);
                    _typeID = value;
                    RaisePropertyChanged(() => TypeID);
                    this.OnChanged();
                    this.OnTypeIDChanged();
                }
            }
        }

        protected virtual void OnTypeIDChanging(int value) { }
        protected virtual void OnTypeIDChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private TypeModel _typeModel;
        public TypeModel TypeModel
        {
            get { return _typeModel; }
            set
            {
                if (_typeModel != value)
                {
                    this.OnTypeModelChanging(value);
                    _typeModel = value;
                    RaisePropertyChanged(() => TypeModel);
                    this.OnChanged();
                    ////this.OnModelChanged(TypeModel.IsEdit);
                    this.OnTypeModelChanged();
                }
            }
        }

        protected virtual void OnTypeModelChanging(TypeModel value) { }
        protected virtual void OnTypeModelChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private ObservableCollection<BackSideModel> _backSideCollection;
        public ObservableCollection<BackSideModel> BackSideCollection
        {
            get { return _backSideCollection; }
            set
            {
                if (_backSideCollection != value)
                {
                    this.OnBackSideCollectionChanging(value);
                    _backSideCollection = value;
                    RaisePropertyChanged(() => BackSideCollection);
                    this.OnChanged();
                    this.OnBackSideCollectionChanged();
                }
            }
        }

        protected virtual void OnBackSideCollectionChanging(ObservableCollection<BackSideModel> value) { }
        protected virtual void OnBackSideCollectionChanged() { }




        private bool _isActived;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsActived
        {
            get { return _isActived; }
            set
            {
                if (_isActived != value)
                {
                    this.OnIsActivedChanging(value);
                    _isActived = value;
                    RaisePropertyChanged(() => IsActived);
                    this.OnChanged();
                    this.OnIsActivedChanged();
                }
            }
        }

        protected virtual void OnIsActivedChanging(bool value) { }
        protected virtual void OnIsActivedChanged() { }





        #endregion
    }
}
