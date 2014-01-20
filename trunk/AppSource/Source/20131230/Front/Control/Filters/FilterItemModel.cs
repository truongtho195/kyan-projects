using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CPC.Toolkit.Base;
using CPC.POS;

namespace CPC.Control
{
    public class FilterItemModel : ModelBase
    {

        #region Properties

        private object _itemsSource;
        public object ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource != value)
                {
                    _itemsSource = value;
                    OnPropertyChanged(() => ItemsSource);
                }
            }
        }

        private object _value1;
        public object Value1
        {
            get { return _value1; }
            set
            {
                if (_value1 != value)
                {
                    _value1 = value;
                    OnPropertyChanged(() => Value1);
                }
            }
        }

        private object _value2;
        public object Value2
        {
            get { return _value2; }
            set
            {
                if (_value2 != value)
                {
                    _value2 = value;
                    OnPropertyChanged(() => Value2);
                }
            }
        }

        private string _valueMember;
        public string ValueMember
        {
            get { return _valueMember; }
            set { _valueMember = value; }
        }

        private string _displayMember;
        public string DisplayMember
        {
            get { return _displayMember; }
            set
            {
                _displayMember = value;
                OnPropertyChanged(() => DisplayMember);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(() => IsSelected);
            }
        }

        private bool _isDefault;
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                OnPropertyChanged(() => IsDefault);
            }
        }

        private SearchType _type;
        public SearchType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(() => Type);
            }
        }

        #endregion

    }
}
