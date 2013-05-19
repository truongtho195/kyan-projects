using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using CPC.Toolkit.Base;
using System.Linq.Expressions;

namespace CPC.POS.Model
{
    [Serializable]
    public class ComboItem : NotifyPropertyChangedBase
    {
        private short _value;
        public short Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private object _objValue;
        public object ObjValue
        {
            get { return _objValue; }
            set { _objValue = value; }
        }

        private int _intValue;
        public int IntValue
        {
            get { return _intValue; }
            set { _intValue = value; }
        }

        private long _longValue;
        public long LongValue
        {
            get { return _longValue; }
            set { _longValue = value; }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private bool _flag;
        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }


        private string _group;
        public string Group
        {
            get { return _group; }
            set { _group = value; }
        }

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        private int _parentId;
        public int ParentId
        {
            get { return _parentId; }
            set { _parentId = value; }
        }

        private object _detail;
        public object Detail
        {
            get { return _detail; }
            set { _detail = value; }
        }
    }


    [Serializable]
    public class StatusItem : ComboItem
    {
        private string _foreColor;
        public string ForeColor
        {
            get { return _foreColor; }
            set { _foreColor = value; }
        }

        private string _backColor;
        public string BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }
    }

    [Serializable]
    public class GroupOfDrop : ComboItem
    {
        private int _tab;
        public int Tab
        {
            get { return _tab; }
            set { _tab = value; }
        }
    }

    [Serializable]
    public class ReportItem : ComboItem
    {
        private string _group;
        public string Group
        {
            get { return _group; }
            set { _group = value; }
        }
    }

    [Serializable]
    public class StateItem : ComboItem
    {
        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }
    }

    [Serializable]
    public class CountryItem : StateItem
    {
        private bool _hasState;
        public bool HasState
        {
            get { return _hasState; }
            set { _hasState = value; }
        }
    }

    [Serializable]
    public class LanguageItem : ComboItem
    {
        private string _culture;
        public string Culture
        {
            get { return _culture; }
            set { _culture = value; }
        }
        private string _code;
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }
    }

    [Serializable]
    public class ComboItemFilter : ComboItem
    {
        private int _parentID;
        public int ParentID
        {
            get { return _parentID; }
            set { _parentID = value; }
        }
    }

}
