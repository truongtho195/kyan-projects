using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FlashCard.Model
{
    public class CategoryModel : CategoryModelBase, IDataErrorInfo
    {
        public CategoryModel()
        {

        }


        #region LessonNum
        private int _lessonNum;
        /// <summary>
        /// Gets or sets the LessonNum.
        /// </summary>
        public int LessonNum
        {
            get
            {
                if (this.LessonCollection != null)
                {
                    _lessonNum = this.LessonCollection.Count;
                }
                return _lessonNum;
            }
            set
            {
                if (_lessonNum != value)
                {
                    _lessonNum = value;
                    RaisePropertyChanged(() => LessonNum);
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
                    case "CategoryName":
                        if (string.IsNullOrWhiteSpace(CategoryName))
                            message = "Category Name is required !";
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

    }
}
