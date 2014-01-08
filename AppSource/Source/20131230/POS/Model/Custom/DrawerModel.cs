using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class DrawerModel : ModelBase
    {
        #region Contructors

        #endregion

        #region Properties

        #region Id

        private short _id;
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public short Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _isDirty = true;
                    _id = value;
                    OnPropertyChanged(() => Id);
                    PropertyChangedCompleted(() => Id);
                }
            }
        }

        #endregion

        #region Text

        private string _text;
        /// <summary>
        /// Gets or sets Text.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _isDirty = true;
                    _text = value;
                    OnPropertyChanged(() => Text);
                    PropertyChangedCompleted(() => Text);
                }
            }
        }

        #endregion

        #endregion
    }
}
