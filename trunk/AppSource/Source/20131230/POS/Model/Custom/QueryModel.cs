using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPC.POS.Model
{
    public class QueryModel
    {
        #region Id
        private object _id;
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public object Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                }
            }
        }
        #endregion

        #region Resource
        private string _resource;
        /// <summary>
        /// Gets or sets the Resource.
        /// </summary>
        public string Resource
        {
            get { return _resource; }
            set
            {
                if (_resource != value)
                {
                    _resource = value;

                }
            }
        }
        #endregion

        #region Text
        private string _text;
        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                }
            }
        }
        #endregion
    }
}
