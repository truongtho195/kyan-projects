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
                    this.OnCategoryNameChanged();
                }
            }
        }

        protected virtual void OnCategoryNameChanging(string value) { }
        protected virtual void OnCategoryNameChanged() { }



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







        #endregion
    }
}
