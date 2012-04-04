using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
   public class CategoryModelBase : ModelBase
    {
        #region Constructors
        public CategoryModelBase()
        {

        }
        #endregion

        #region Properties


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
        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (_categoryName != value)
                {
                    this.OnCategoryNameChanging(value);
                    _categoryName = value;
                    RaisePropertyChanged(() => CategoryName);
                    this.OnChanged();
                    this.OnCategoryNameChanged();
                }
            }
        }

        protected virtual void OnCategoryNameChanging(string value) { }
        protected virtual void OnCategoryNameChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private List<LessonModel> _lessonCollection;
        public List<LessonModel> LessonCollection
        {
            get { return _lessonCollection; }
            set
            {
                if (_lessonCollection != value)
                {
                    this.OnLessonCollectionChanging(value);
                    _lessonCollection = value;
                    RaisePropertyChanged(() => LessonCollection);
                    this.OnChanged();
                    this.OnLessonCollectionChanged();
                }
            }
        }

        protected virtual void OnLessonCollectionChanging(List<LessonModel> value) { }
        protected virtual void OnLessonCollectionChanged() { }








        #endregion
    }
}
