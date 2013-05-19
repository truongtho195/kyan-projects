using System;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    [Serializable]
    public class CheckBoxItemModel : NotifyPropertyChangedBase
    {
        #region Primitive Properties

        private int _value;
        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(() => Value);
                }
            }
        }

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
                    OnPropertyChanged(() => Text);
                }
            }
        }

        private bool _isChecked;
        /// <summary>
        /// Gets or sets the IsChecked.
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(() => IsChecked);
                }
            }
        }

        #endregion

        #region Constructors

        // Default constructor
        public CheckBoxItemModel()
        {

        }

        // Default constructor that set value to field
        public CheckBoxItemModel(ComboItem comboItem)
        {
            this.Value = comboItem.Value;
            this.Text = comboItem.Text;
        }
        
        #endregion
    }
}
