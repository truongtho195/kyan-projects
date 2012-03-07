using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    this.OnLessonNameChanged();
                }
            }
        }

        protected virtual void OnLessonNameChanging(string value) { }
        protected virtual void OnLessonNameChanged() { }



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
                    this.OnTypeModelChanged();
                }
            }
        }

        protected virtual void OnTypeModelChanging(TypeModel value) { }
        protected virtual void OnTypeModelChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private List<BackSideModel> _backSideCollection;
        public List<BackSideModel> BackSideCollection
        {
            get { return _backSideCollection; }
            set
            {
                if (_backSideCollection != value)
                {
                    this.OnBackSideCollectionChanging(value);
                    _backSideCollection = value;
                    RaisePropertyChanged(() => BackSideCollection);
                    this.OnBackSideCollectionChanged();
                }
            }
        }

        protected virtual void OnBackSideCollectionChanging(List<BackSideModel> value) { }
        protected virtual void OnBackSideCollectionChanged() { }









        #endregion
    }
}
