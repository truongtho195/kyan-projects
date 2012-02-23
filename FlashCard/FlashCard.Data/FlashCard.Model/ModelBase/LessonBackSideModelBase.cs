using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class LessonBackSideModelBase : ModelBase
    {
        #region Constructors
        public LessonBackSideModelBase()
        {

        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _lessonBackSideID;
        public int LessonBackSideID
        {
            get { return _lessonBackSideID; }
            set
            {
                if (_lessonBackSideID != value)
                {
                    this.OnLessonBackSideIDChanging(value);
                    _lessonBackSideID = value;
                    RaisePropertyChanged(() => LessonBackSideID);
                    this.OnLessonBackSideIDChanged();
                }
            }
        }

        protected virtual void OnLessonBackSideIDChanging(int value) { }
        protected virtual void OnLessonBackSideIDChanged() { }



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _lessonID;
        public int LessonId
        {
            get { return _lessonID; }
            set
            {
                if (_lessonID != value)
                {
                    this.OnLessonIdChanging(value);
                    _lessonID = value;
                    RaisePropertyChanged(() => LessonId);
                    this.OnLessonIdChanged();
                }
            }
        }

        protected virtual void OnLessonIdChanging(int value) { }
        protected virtual void OnLessonIdChanged() { }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _content;
        public string Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    this.OnContentChanging(value);
                    _content = value;
                    RaisePropertyChanged(() => Content);
                    this.OnContentChanged();
                }
            }
        }

        protected virtual void OnContentChanging(string value) { }
        protected virtual void OnContentChanged() { }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private bool? _isCorrect;
        public bool? IsCorrect
        {
            get { return _isCorrect; }
            set
            {
                if (_isCorrect != value)
                {
                    this.OnIsCorrectChanging(value);
                    _isCorrect = value;
                    RaisePropertyChanged(() => IsCorrect);
                    this.OnIsCorrectChanged();
                }
            }
        }

        protected virtual void OnIsCorrectChanging(bool? value) { }
        protected virtual void OnIsCorrectChanged() { }












        #endregion
    }
}
