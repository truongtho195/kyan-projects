using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class BackSideModelBase : ModelBase
    {
        #region Constructors
        public BackSideModelBase()
        {

        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private int _backSideID;
        public int BackSideID
        {
            get { return _backSideID; }
            set
            {
                if (_backSideID != value)
                {
                    this.OnLessonBackSideIDChanging(value);
                    _backSideID = value;
                    RaisePropertyChanged(() => BackSideID);
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
        public int LessonID
        {
            get { return _lessonID; }
            set
            {
                if (_lessonID != value)
                {
                    this.OnLessonIdChanging(value);
                    _lessonID = value;
                    RaisePropertyChanged(() => LessonID);
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


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private LessonModel _lessonModel;
        public LessonModel LessonModel
        {
            get { return _lessonModel; }
            set
            {
                if (_lessonModel != value)
                {
                    this.OnLessonModelChanging(value);
                    _lessonModel = value;
                    RaisePropertyChanged(() => LessonModel);
                    this.OnLessonModelChanged();
                }
            }
        }

        protected virtual void OnLessonModelChanging(LessonModel value) { }
        protected virtual void OnLessonModelChanged() { }












        #endregion
    }
}
