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

        
      

        #endregion

    }
}
