using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using CPC.Helper;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupAddNewWarrantyViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Defines

        #endregion

        #region Properties

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

        private ComboItem _warrantyItem = new ComboItem();
        /// <summary>
        /// Gets or sets the WarrantyItem.
        /// </summary>
        public ComboItem WarrantyItem
        {
            get { return _warrantyItem; }
            set
            {
                if (_warrantyItem != value)
                {
                    _warrantyItem = value;
                    OnPropertyChanged(() => WarrantyItem);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupAddNewWarrantyViewModel()
        {
            InitialCommand();
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

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
            WarrantyItem.Text = this.Text.Trim();
            SaveXml(Common.WarrantyTypeAll, "WarrantyType");

            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Save new element to xml
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="itemList"></param>
        private void SaveXml(IList<ComboItem> itemList, string sKey)
        {
            // Load xml file stream
            FileStream fileStream = Common.LoadCurrentLanguagePackage() as FileStream;

            // Get file path
            string xmlFilePath = fileStream.Name;

            // Load xml document
            XDocument xDoc = XDocument.Load(fileStream);

            // Close stream
            fileStream.Close();
            fileStream.Dispose();

            // Get xml element by key
            XElement xElements = xDoc.Root.Elements("combo").FirstOrDefault(x => x.Attribute("key").Value.Equals(sKey));

            if (xElements == null)
            {
                XElement newElements = new XElement("combo");
                newElements.Add(new XAttribute("key", sKey));

                xDoc.Add(newElements);
            }

            // Get max id and next id
            int maxID = itemList.Max(x => x.Value);
            WarrantyItem.Value = Convert.ToInt16(maxID + 1);
            WarrantyItem.ObjValue = WarrantyItem.Value;

            XElement newElement = new XElement("item");
            newElement.Add(new XElement("value", WarrantyItem.Value));
            newElement.Add(new XElement("name", WarrantyItem.Text.Trim()));
            xElements.Add(newElement);

            xDoc.Save(xmlFilePath);

            // Add new element to list
            itemList.Add(WarrantyItem);
        }

        #endregion

        #region IDataErrorInfo Members

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
                string message = string.Empty;

                switch (columnName)
                {
                    case "Text":
                        if (string.IsNullOrWhiteSpace(Text))
                            message = "Warranty name is required";
                        else if (Common.WarrantyTypeAll != null && Common.WarrantyTypes.Any(x => x.Text.ToLower().Equals(this.Text.ToLower())))
                            message = "Warranty name is existed.";
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