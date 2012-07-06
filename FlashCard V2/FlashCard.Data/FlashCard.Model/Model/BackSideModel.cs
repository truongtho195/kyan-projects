using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FlashCard.Model
{
    public class BackSideModel : BackSideModelBase, IDataErrorInfo
    {
        #region Constructors
        public BackSideModel()
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
                    case "BackSideDetail":
                        if (BackSideDetail == null)
                            message = "Back side Detail is required!";
                        else
                        {
                            var range = new System.Windows.Documents.TextRange(BackSideDetail.ContentStart, BackSideDetail.ContentEnd);
                             if (string.IsNullOrWhiteSpace( range.Text))
                                message = "Back side Detail is required!";
                        }
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
