using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CPC.Helper;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class AddNewStateViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define

        #endregion

        #region Constructors
        public AddNewStateViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }
        #endregion

        #region Properties

        #region StateName
        private string _stateName;
        /// <summary>
        /// Gets or sets the StateName.
        /// </summary>
        public string StateName
        {
            get { return _stateName; }
            set
            {
                if (_stateName != value)
                {
                    _stateName = value;
                    OnPropertyChanged(() => StateName);
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


        #region ItemState
        private ComboItem _itemState;
        /// <summary>
        /// Gets or sets the ItemState.
        /// </summary>
        public ComboItem ItemState
        {
            get { return _itemState; }
            set
            {
                if (_itemState != value)
                {
                    _itemState = value;
                    OnPropertyChanged(() => ItemState);
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
            SaveStateToXml();

            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }

        private void SaveStateToXml()
        {
            // Load XML file.
            Stream stream = Common.LoadCurrentLanguagePackage();
            // Get file path.
            string fileLanguage = (stream as FileStream).Name;
            XDocument xDoc = XDocument.Load(stream);
            stream.Close();
            stream.Dispose();

            //// Get Prices in xml element.
            var stateCollection = xDoc.Root.Elements("combo").FirstOrDefault(x => x.Attribute("key").Value == "state");
            int maxIdState = Common.States.Max(x => x.Value);
            int Id = maxIdState + 1;
            if (stateCollection == null)
            {
                XElement comboItem = new XElement("combo");
                comboItem.Add(new XAttribute("key", "state"));

                XElement item = new XElement("item");
                item.Add(new XElement("value", 0));
                item.Add(new XElement("name", string.Empty));
                item.Add(new XElement("symbol", string.Empty));

                XElement item1 = new XElement("item");
                item1.Add(new XElement("value", 1));
                item1.Add(new XElement("name", StateName.Trim()));
                item1.Add(new XElement("symbol", string.Empty));
                xDoc.Add(comboItem);
            }
            else
            {
                XElement root = new XElement("item");
                root.Add(new XElement("value", Id));
                root.Add(new XElement("name", StateName.Trim()));
                root.Add(new XElement("symbol", Symbol.Trim()));
                stateCollection.Add(root);
            }

            xDoc.Save(fileLanguage);

            ItemState = new ComboItem()
            {
                ObjValue = Id,
                Value = Convert.ToInt16(Id),
                Text = StateName,
                Symbol = Symbol.Trim()
            };

            //Insert to Current Collection
            Common.States.Add(ItemState);
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
                    case "StateName":
                        if (string.IsNullOrWhiteSpace(StateName))
                        {
                            message = "State name is required";
                        }
                        break;

                    case "Symbol":
                        if (string.IsNullOrWhiteSpace(Symbol))
                            message = "Code is required!";
                        else if (Common.States != null && Common.States.Any(x => x.Symbol.ToLower().Equals(Symbol.ToLower())))
                            message = "Code is existed";
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