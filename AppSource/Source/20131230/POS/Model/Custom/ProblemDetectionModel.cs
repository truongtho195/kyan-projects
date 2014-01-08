using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CPC.POS.Model
{
    class ProblemDetectionModel : ModelBase
    {
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

        #region AssociateCollection
        /// <summary>
        /// Gets or sets the AssociateCollection.
        /// </summary>
        private ObservableCollection<ItemModel> _associateCollection = new ObservableCollection<ItemModel>();
        public ObservableCollection<ItemModel> AssociateCollection
        {
            get
            {
                return _associateCollection;
            }
            set
            {
                if (_associateCollection != value)
                {
                    _associateCollection = value;
                    OnPropertyChanged(() => AssociateCollection);
                }
            }
        }

        #endregion
      
        #endregion
    }
}
