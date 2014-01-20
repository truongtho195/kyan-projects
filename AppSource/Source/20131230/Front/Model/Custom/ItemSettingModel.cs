using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class ItemSettingModel : ModelBase
    {
        #region Contructors

        public ItemSettingModel(SettingParts settingParts)
        {
            _settingParts = settingParts;
        }

        public ItemSettingModel(SettingParts settingParts, int id, string name)
        {
            _id = id;
            _name = name;
            _settingParts = settingParts;
        }

        #endregion

        #region Properties

        #region Id

        private int _id;
        /// <summary>
        /// Gets or sets item's id.
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(() => Id);
                }
            }
        }

        #endregion

        #region Name

        private string _name;
        /// <summary>
        /// Gets or sets item's name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        #endregion

        #region Parent

        private ItemSettingModel _parent;
        /// <summary>
        /// Gets or sets item's parent.
        /// </summary>
        public ItemSettingModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnPropertyChanged(() => Parent);
                }
            }
        }

        #endregion

        #region Childs

        private CollectionBase<ItemSettingModel> _childs;
        /// <summary>
        /// Gets or sets item's childs.
        /// </summary>
        public CollectionBase<ItemSettingModel> Childs
        {
            get
            {
                return _childs;
            }
            set
            {
                if (_childs != value)
                {
                    _childs = value;
                    OnPropertyChanged(() => Childs);
                }
            }
        }

        #endregion

        #region SettingParts

        private SettingParts _settingParts;
        /// <summary>
        /// Gets or sets item's SettingParts.
        /// </summary>
        public SettingParts SettingParts
        {
            get
            {
                return _settingParts;
            }
        }

        #endregion

        #region IsSelected

        private bool _isSelected;
        /// <summary>
        /// Gets or sets whether this object is selected. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }

        #endregion

        #region IsExpanded

        private bool _isExpanded;
        /// <summary>
        /// Gets or sets whether the child items in this object are expanded or collapsed. 
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(() => IsExpanded);
                }
            }
        }

        #endregion

        #endregion
    }
}
