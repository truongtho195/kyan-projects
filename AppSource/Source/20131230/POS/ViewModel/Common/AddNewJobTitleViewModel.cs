using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using System.IO;
using CPC.Helper;
using System.Xml.Linq;
using CPC.POS.Model;
using System.ComponentModel;
using CPC.POS.Repository;
using CPC.POS.Database;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class AddNewJobTitleViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        private base_GenericCodeRepository _genericCodeRepository = new base_GenericCodeRepository();
        #endregion

        #region Constructors
        public AddNewJobTitleViewModel()
        {
            _ownerViewModel = this;
            this.InitialCommand();
        }
        #endregion

        #region Properties

        #region ID
        private int _id;
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(() => ID);
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
                    _text = value;
                    OnPropertyChanged(() => Text);
                }
            }
        }
        #endregion

        #region Symbol
        private string _symbol = string.Empty;
        /// <summary>
        /// Gets or sets the Symbol.
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    OnPropertyChanged(() => Symbol);
                }
            }
        }
        #endregion

        #region ItemJobTitle
        private ComboItem _itemJobTitle;
        /// <summary>
        /// Gets or sets the ItemJobTitle.
        /// </summary>
        public ComboItem ItemJobTitle
        {
            get { return _itemJobTitle; }
            set
            {
                if (_itemJobTitle != value)
                {
                    _itemJobTitle = value;
                    OnPropertyChanged(() => ItemJobTitle);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Gets the OkCommand Command.
        /// <summary>
        public RelayCommand OkCommand { get; private set; }


        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return IsValid;
        }
        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            this.SaveJobTitle();
            this.FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        private void SaveJobTitle()
        {
            string JobTitleCode = GenericCode.JT.ToString();
            var query = this._genericCodeRepository.GetAll(x => x.Code == JobTitleCode && x.Code.ToLower().Equals(this.Text.ToLower()));
            if (query != null && query.Count > 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("JobTitle has existed.", Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            base_GenericCode code = new base_GenericCode();
            code.Code = "JT";
            code.Name = this.Text;
            code.Type = "GC";
            code.Language = Define.CONFIGURATION.DefaultLanguage;
            this._genericCodeRepository.Add(code);
            this._genericCodeRepository.Commit();
            ItemJobTitle = new ComboItem()
            {
                ObjValue = code.Id,
                Value = Convert.ToInt16(code.Id),
                Text = this.Text,
                Symbol = code.Code
            };
            //Insert to Current Collection
            Common.JobTitles.Add(ItemJobTitle);
        }

        #endregion

        #region CancelCommand
        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>
        public RelayCommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion

        #region Public Methods
        #endregion

        #region IDataError
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

        public bool IsValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(Error);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;

                switch (columnName)
                {
                    case "Text":
                        if (string.IsNullOrWhiteSpace(this.Text))
                            message = "JobTitle is required.";
                        else if (Common.JobTitles != null && Common.JobTitles.Any(x => x.Text.ToLower().Equals(this.Text.ToLower())))
                            message = "JobTitle existed.";
                        break;
                }


                if (!string.IsNullOrWhiteSpace(message))
                    return message;

                return null;
            }
        }
        #endregion
    }


}
