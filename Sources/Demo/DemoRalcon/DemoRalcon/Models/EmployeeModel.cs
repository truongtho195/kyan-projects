using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace DemoFalcon.Model
{
    public class EmployeeModel : EmployeeModelBase, IDataErrorInfo
    {
        #region Constructors
        public EmployeeModel()
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
                    case "FirstName":
                        if (string.IsNullOrWhiteSpace(this.FirstName))
                            message = "Họ không được rỗng !";
                        break;
                    case "LastName":
                        if (string.IsNullOrWhiteSpace(this.LastName))
                            message = "Tên không được rỗng !";
                        break;
                    case "Email":
                        if (string.IsNullOrWhiteSpace(Email))
                        {
                            message = "Phone không được rỗng !";
                        }
                        else 
                        {
                            string MatchEmailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                                                     + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				                                            [0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                                                     + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				                                            [0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                                                     + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
                            Regex reStrict = new Regex(MatchEmailPattern);
                            if (!reStrict.IsMatch(Email))
                                message = "Email không đúng định dạng !";
                        }
                        break;
                    case "Phone":
                        if(string.IsNullOrWhiteSpace(Phone))
                            message = "Phone không được rỗng !";
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
