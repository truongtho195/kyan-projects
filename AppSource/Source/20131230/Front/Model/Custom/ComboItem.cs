﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.Linq.Expressions;
using System.Globalization;
using System.Windows.Media;
using System.ComponentModel;

namespace CPC.POS.Model
{
    [Serializable]
    public class ComboItem : ModelBase, IDataErrorInfo
    {
        private short _value;
        public short Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        private object _objValue;
        public object ObjValue
        {
            get
            {
                return _objValue;
            }
            set
            {
                _objValue = value;
            }
        }

        private int _intValue;
        public int IntValue
        {
            get
            {
                return _intValue;
            }
            set
            {
                _intValue = value;
            }
        }

        private long _longValue;
        public long LongValue
        {
            get
            {
                return _longValue;
            }
            set
            {
                _longValue = value;
            }
        }

        private decimal _decimalValue;
        public decimal DecimalValue
        {
            get
            {
                return _decimalValue;
            }
            set
            {
                _decimalValue = value;
            }
        }

        private string _text;
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
                    _text = value;
                    OnPropertyChanged(() => Text);
                }
            }
        }

        private bool _flag;
        public bool Flag
        {
            get
            {
                return _flag;
            }
            set
            {
                _flag = value;
            }
        }

        private string _group;
        public string Group
        {
            get
            {
                return _group;
            }
            set
            {
                _group = value;
            }
        }

        private string _symbol;
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
            }
        }

        private int _parentId;
        public int ParentId
        {
            get
            {
                return _parentId;
            }
            set
            {
                _parentId = value;
            }
        }

        private object _detail;
        public object Detail
        {
            get
            {
                return _detail;
            }
            set
            {
                _detail = value;
            }
        }

        private bool _islocked;
        public bool Islocked
        {
            get
            {
                return _islocked;
            }
            set
            {
                _islocked = value;
                OnPropertyChanged(() => Islocked);
            }
        }

        private bool _hasState;
        public bool HasState
        {
            get
            {
                return _hasState;
            }
            set
            {
                _hasState = value;
            }
        }

        private int _tab;
        public int Tab
        {
            get
            {
                return _tab;
            }
            set
            {
                _tab = value;
            }
        }

        private CultureInfo _cultureInfo;
        /// <summary>
        /// Gets or sets CultureInfo used for language.
        /// </summary>
        public CultureInfo CultureInfo
        {
            get
            {
                return _cultureInfo;
            }
            set
            {
                _cultureInfo = value;
            }
        }

        private string _code;
        /// <summary>
        /// Gets or sets Code used for language.
        /// </summary>
        public string Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
            }
        }

        private Brush _image;
        /// <summary>
        /// Gets or sets Image used for icon of language.
        /// </summary>
        public Brush Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
            }
        }

        private string _settingPart;
        /// <summary>
        /// Gets or sets SettingPart.
        /// </summary>
        public string SettingPart
        {
            get
            {
                return _settingPart;
            }
            set
            {
                _settingPart = value;
            }
        }

        private DateTime _from;
        public DateTime From
        {
            get
            {
                return _from;
            }
            set
            {
                if (_from != value)
                {
                    _isDirty = true;
                    _from = value;
                    OnPropertyChanged(() => From);
                    //Validate To property again.
                    OnPropertyChanged(() => To);
                }
            }
        }

        private DateTime _to;
        public DateTime To
        {
            get
            {
                return _to;
            }
            set
            {
                if (_to != value)
                {
                    _isDirty = true;
                    _to = value;
                    OnPropertyChanged(() => To);
                    //Validate From property again.
                    OnPropertyChanged(() => From);
                }
            }
        }

        #region IDataErrorInfo Members

        #region HasError

        /// <summary>
        /// Gets value indicate that this object has error or not.
        /// </summary>
        public bool HasError
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Error);
            }
        }

        #endregion

        public string Error
        {
            get
            {
                List<string> errors = new List<string>();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string msg = this[prop.Name];
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Add(msg);
                    }
                }

                return string.Join(Environment.NewLine, errors);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = null;

                switch (columnName)
                {
                    case "From":
                    case "To":

                        if (DateTime.Compare(_from, _to) >= 0)
                        {
                            message = "Begin time must before end time.";
                        }

                        break;
                }

                return message;
            }
        }

        #endregion
    }
}