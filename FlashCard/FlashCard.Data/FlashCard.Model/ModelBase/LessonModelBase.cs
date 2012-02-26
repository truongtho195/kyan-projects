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









        #endregion
    }
}
