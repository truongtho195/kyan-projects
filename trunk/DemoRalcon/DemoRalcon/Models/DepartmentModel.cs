﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;
using System.ComponentModel;

namespace DemoFalcon.Model
{
    public class DepartmentModel : DepartmentModelBase,IDataErrorInfo 
    {
        #region Constructors
        public DepartmentModel()
        {

        }
        #endregion



        #region DataErrorInfo
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        private Dictionary<string, string> _errors = new Dictionary<string, string>();
        public Dictionary<string, string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                if (_errors != value)
                {
                    _errors = value;
                    OnChanged();
                    RaisePropertyChanged(() => Errors);
                }
            }
        }
        public string this[string columnName]
        {
            get
            {
                string message = String.Empty;
                this.Errors.Remove(columnName);
                switch (columnName)
                {
                    case "DepartmentName":
                        if (string.IsNullOrWhiteSpace(DepartmentName))
                            message = "Tên phòng ban không được rỗng !";
                        break;
                    default:
                        break;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    this.Errors.Add(columnName, message);
                }
                return message;
            }
        }
        #endregion
        
    }
}