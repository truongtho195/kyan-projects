using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.ComponentModel;
using CPC.POS.Model;
using BarcodeLib;
using CPC.Helper;
using CPC.POS.Repository;
using CPC.POS.Database;

namespace CPC.POS.ViewModel
{
    public class GiftCardCreationViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        private base_CardManagementRepository _cardManagementRepository = new base_CardManagementRepository();
        #endregion

        #region Constructors
        public GiftCardCreationViewModel()
        {
            InitialCommand();

        }
        #endregion

        #region Properties

        #region CardCode
        private string _cardNumber;
        /// <summary>
        /// Gets or sets the CardCode.
        /// </summary>
        public string CardNumber
        {
            get { return _cardNumber; }
            set
            {
                if (_cardNumber != value)
                {
                    _cardNumber = value;
                    OnPropertyChanged(() => CardNumber);
                    ValidateCardInCardManager();
                }
            }
        }

       
        #endregion

        #region Amount
        private decimal _amount;
        /// <summary>
        /// Gets or sets the Amount.
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(() => Amount);
                }
            }
        }
        #endregion


        #region StoreCardModel
        private base_CardManagementModel _storeCardModel;
        /// <summary>
        /// Gets or sets the StoreCardModel.
        /// </summary>
        public base_CardManagementModel StoreCardModel
        {
            get { return _storeCardModel; }
            set
            {
                if (_storeCardModel != value)
                {
                    _storeCardModel = value;
                    OnPropertyChanged(() => StoreCardModel);
                }
            }
        }
        #endregion

        #region IsCardValid
        private bool _isCardValid =true;
        /// <summary>
        /// Gets or sets the IsCardValid.
        /// </summary>
        public bool IsCardValid
        {
            get { return _isCardValid; }
            set
            {
                if (_isCardValid != value)
                {
                    _isCardValid = value;
                    OnPropertyChanged(() => IsCardValid);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Gets the Ok Command.
        /// <summary>

        public RelayCommand<object> OkCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute(object param)
        {
            return IsValid;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            Barcode barcodeObject = new Barcode();

            StoreCardModel = new base_CardManagementModel();
            StoreCardModel.CardNumber = CardNumber;
            StoreCardModel.InitialAmount = Amount;
            StoreCardModel.RemainingAmount = Amount;
            barcodeObject.Encode(TYPE.UPCA, StoreCardModel.CardNumber, 200, 70);
            StoreCardModel.ScanCode = barcodeObject.RawData;
            StoreCardModel.ScanImg = barcodeObject.Encoded_Image_Bytes;
            StoreCardModel.CardTypeId = 256;//StoreCard
            StoreCardModel.Status = Common.StatusBasic.Any() ? Common.StatusBasic.First().Value : (short)0;
            StoreCardModel.UserCreated = Define.USER.LoginName;
            StoreCardModel.DateCreated = DateTime.Now;
            StoreCardModel.IsNew = true;
            StoreCardModel.IsDirty = false;

            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region CancelCommand
        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion
        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Check card existed in db
        /// </summary>
        private void ValidateCardInCardManager()
        {
            IsCardValid = true;
            if (CardNumber.Length.Equals(12))
            {
                string cardNumber = CardNumber.Trim();
                IQueryable<base_CardManagement> query = _cardManagementRepository.GetIQueryable(x =>x.IsSold && x.CardNumber.Equals(cardNumber));

                //Card Existed
                IsCardValid = !query.Any();
            }
        }
        #endregion

        #region Public Methods
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
                string message = string.Empty;
                switch (columnName)
                {
                    case "Amount":
                      
                        break;
                    case "CardNumber":
                        if (string.IsNullOrWhiteSpace(CardNumber))
                            message = "Card Number is required";
                        else if (CardNumber.Trim().Length < 12)
                            message = "Card Number is 12 character";
                        break;
                    case "IsCardValid":
                        if (!string.IsNullOrWhiteSpace(CardNumber) && !IsCardValid)
                            message = "Card is not valid";
                        break;
                }


                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }
    }
}
