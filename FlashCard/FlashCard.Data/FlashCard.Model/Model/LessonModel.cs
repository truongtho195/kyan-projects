using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class LessonModel : LessonModelBase
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
            get {
                if (!IsBackSide)
                    _sideName = "Front Side";
                else
                    _sideName = "Back Side";
                return _sideName; }
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

      

        #endregion


        #region Overide Changed
       
        #endregion

    }
}
