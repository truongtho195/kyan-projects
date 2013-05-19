using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.ComponentModel;

namespace CPC.POS.Model
{
    class ItemModel : ModelBase
    {
        #region Ctor
        public ItemModel()
        {
            IsNew = true;
        } 
        #endregion

        #region Properties
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
                    IsDirty = true;
                    OnPropertyChanged(() => Id);
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
                    IsDirty = true;
                    OnPropertyChanged(() => Resource);
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

                    _text = value.Replace(",", "");
                    IsDirty = true;
                    OnPropertyChanged(() => Text);
                }
            }
        }
        #endregion 
        
        #region Detail
        private string _detail;
        /// <summary>
        /// Gets or sets the Detail.
        /// </summary>
        public string Detail
        {
            get { return _detail; }
            set
            {
                if (_detail != value)
                {
                    _detail = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Detail);
                }
            }
        }
        #endregion

        #endregion

        #region Methods
        public void EndUpdate()
        {
            this.IsDirty = false;
            this.IsNew = false;
        }
        #endregion
    }
}
