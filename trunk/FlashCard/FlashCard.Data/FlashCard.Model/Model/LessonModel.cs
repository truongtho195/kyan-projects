using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FlashCard.Model
{
    public class LessonModel : LessonModelBase, IDataErrorInfo
    {
        #region Constructors
        public LessonModel()
        {

        }
        #endregion

        #region Variables
        //LessonType 1: Phrase | 2: SingleChoice | 3: MultiChoice
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private bool _isBackSide;
        public bool IsBackSide
        {
            get { return _isBackSide; }
            set
            {
                if (_isBackSide != value)
                {
                    _isBackSide = value;
                    RaisePropertyChanged(() => IsBackSide);
                    RaisePropertyChanged(() => SideName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        private string _sideName;
        public string SideName
        {
            get
            {
                if (!IsBackSide)
                    _sideName = "Front Side";
                else
                    _sideName = "Back Side";
                return _sideName;
            }
            set
            {
                if (_sideName != value)
                {
                    _sideName = value;
                    RaisePropertyChanged(() => SideName);
                }
            }
        }

        private BackSideModel _backSideModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public BackSideModel BackSideModel
        {
            get { return _backSideModel; }
            set
            {
                if (_backSideModel != value)
                {
                    _backSideModel = value;
                    RaisePropertyChanged(() => BackSideModel);
                }
            }
        }

        private bool _isEditing;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    RaisePropertyChanged(() => IsEditing);
                }
            }
        }

        #region IsNewType
        private bool _isNewType;
        /// <summary>
        /// Gets or sets the IsNewType.
        /// </summary>
        public bool IsNewType
        {
            get { return _isNewType; }
            set
            {
                if (_isNewType != value)
                {
                    _isNewType = value;
                    RaisePropertyChanged(() => IsNewType);
                }
            }
        }
        #endregion

        #region IsNewCate
        private bool _isNewCate;
        /// <summary>
        /// Gets or sets the IsNewCate.
        /// </summary>
        public bool IsNewCate
        {
            get { return _isNewCate; }
            set
            {
                if (_isNewCate != value)
                {
                    _isNewCate = value;
                    RaisePropertyChanged(() => IsNewCate);
                }
            }
        }
        #endregion

        #endregion



        #region Overide Changed

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
                    case "LessonName":
                        if (string.IsNullOrWhiteSpace(LessonName))
                            message = "Lesson Name is required!";
                        break;
                    case "Description":
                        if (Description == null)
                            message = "Description is required!";
                        else
                        {
                            var range = new System.Windows.Documents.TextRange(Description.ContentStart, Description.ContentEnd);
                            if (range.IsEmpty)
                                message = "Description is required!";
                        }
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
