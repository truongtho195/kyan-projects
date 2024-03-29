//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using CPC.Helper;
using CPC.POS.Database;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table base_CardManagement
    /// </summary>
    [Serializable]
    public partial class base_CardManagementModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_CardManagementModel()
        {
            this.IsNew = true;
            this.base_CardManagement = new base_CardManagement();
        }

        // Default constructor that set entity to field
        public base_CardManagementModel(base_CardManagement base_cardmanagement, bool isRaiseProperties = false)
        {
            this.base_CardManagement = base_cardmanagement;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_CardManagement base_CardManagement { get; private set; }

        #endregion

        #region Primitive Properties

        protected long _id;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Id</param>
        /// </summary>
        public long Id
        {
            get { return this._id; }
            set
            {
                if (this._id != value)
                {
                    this.IsDirty = true;
                    this._id = value;
                    OnPropertyChanged(() => Id);
                    PropertyChangedCompleted(() => Id);
                }
            }
        }

        protected string _cardNumber;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the CardNumber</param>
        /// </summary>
        public string CardNumber
        {
            get { return this._cardNumber; }
            set
            {
                if (this._cardNumber != value)
                {
                    this.IsDirty = true;
                    this._cardNumber = value;
                    OnPropertyChanged(() => CardNumber);
                    PropertyChangedCompleted(() => CardNumber);
                }
            }
        }

        protected Nullable<System.DateTime> _purchaseDate;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PurchaseDate</param>
        /// </summary>
        public Nullable<System.DateTime> PurchaseDate
        {
            get { return this._purchaseDate; }
            set
            {
                if (this._purchaseDate != value)
                {
                    this.IsDirty = true;
                    this._purchaseDate = value;
                    OnPropertyChanged(() => PurchaseDate);
                    PropertyChangedCompleted(() => PurchaseDate);
                }
            }
        }

        protected decimal _initialAmount;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the InitialAmount</param>
        /// </summary>
        public decimal InitialAmount
        {
            get { return this._initialAmount; }
            set
            {
                if (this._initialAmount != value)
                {
                    this.IsDirty = true;
                    this._initialAmount = value;
                    OnPropertyChanged(() => InitialAmount);
                    PropertyChangedCompleted(() => InitialAmount);
                }
            }
        }

        protected Nullable<System.DateTime> _lastUsed;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the LastUsed</param>
        /// </summary>
        public Nullable<System.DateTime> LastUsed
        {
            get { return this._lastUsed; }
            set
            {
                if (this._lastUsed != value)
                {
                    this.IsDirty = true;
                    this._lastUsed = value;
                    OnPropertyChanged(() => LastUsed);
                    PropertyChangedCompleted(() => LastUsed);
                }
            }
        }

        protected decimal _remainingAmount;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the RemainingAmount</param>
        /// </summary>
        public decimal RemainingAmount
        {
            get { return this._remainingAmount; }
            set
            {
                if (this._remainingAmount != value)
                {
                    this.IsDirty = true;
                    this._remainingAmount = value;
                    OnPropertyChanged(() => RemainingAmount);
                    PropertyChangedCompleted(() => RemainingAmount);
                }
            }
        }

        protected string _guestResourcePurchased;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the GuestResourcePurchased</param>
        /// </summary>
        public string GuestResourcePurchased
        {
            get { return this._guestResourcePurchased; }
            set
            {
                if (this._guestResourcePurchased != value)
                {
                    this.IsDirty = true;
                    this._guestResourcePurchased = value;
                    OnPropertyChanged(() => GuestResourcePurchased);
                    PropertyChangedCompleted(() => GuestResourcePurchased);
                }
            }
        }

        protected string _guestGiftedResource;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the GuestGiftedResource</param>
        /// </summary>
        public string GuestGiftedResource
        {
            get { return this._guestGiftedResource; }
            set
            {
                if (this._guestGiftedResource != value)
                {
                    this.IsDirty = true;
                    this._guestGiftedResource = value;
                    OnPropertyChanged(() => GuestGiftedResource);
                    PropertyChangedCompleted(() => GuestGiftedResource);
                }
            }
        }

        protected bool _isUsed;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IsUsed</param>
        /// </summary>
        public bool IsUsed
        {
            get { return this._isUsed; }
            set
            {
                if (this._isUsed != value)
                {
                    this.IsDirty = true;
                    this._isUsed = value;
                    OnPropertyChanged(() => IsUsed);
                    PropertyChangedCompleted(() => IsUsed);
                }
            }
        }

        protected string _userCreated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the UserCreated</param>
        /// </summary>
        public string UserCreated
        {
            get { return this._userCreated; }
            set
            {
                if (this._userCreated != value)
                {
                    this.IsDirty = true;
                    this._userCreated = value;
                    OnPropertyChanged(() => UserCreated);
                    PropertyChangedCompleted(() => UserCreated);
                }
            }
        }

        protected Nullable<System.DateTime> _dateCreated;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the DateCreated</param>
        /// </summary>
        public Nullable<System.DateTime> DateCreated
        {
            get { return this._dateCreated; }
            set
            {
                if (this._dateCreated != value)
                {
                    this.IsDirty = true;
                    this._dateCreated = value;
                    OnPropertyChanged(() => DateCreated);
                    PropertyChangedCompleted(() => DateCreated);
                }
            }
        }

        protected bool _isPurged;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IsPurged</param>
        /// </summary>
        public bool IsPurged
        {
            get { return this._isPurged; }
            set
            {
                if (this._isPurged != value)
                {
                    this.IsDirty = true;
                    this._isPurged = value;
                    OnPropertyChanged(() => IsPurged);
                    PropertyChangedCompleted(() => IsPurged);
                }
            }
        }

        protected string _purgeReason;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PurgeReason</param>
        /// </summary>
        public string PurgeReason
        {
            get { return this._purgeReason; }
            set
            {
                if (this._purgeReason != value)
                {
                    this.IsDirty = true;
                    this._purgeReason = value;
                    OnPropertyChanged(() => PurgeReason);
                    PropertyChangedCompleted(() => PurgeReason);
                }
            }
        }

        protected string _scanCode;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ScanCode</param>
        /// </summary>
        public string ScanCode
        {
            get { return this._scanCode; }
            set
            {
                if (this._scanCode != value)
                {
                    this.IsDirty = true;
                    this._scanCode = value;
                    OnPropertyChanged(() => ScanCode);
                    PropertyChangedCompleted(() => ScanCode);
                }
            }
        }

        protected byte[] _scanImg;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ScanImg</param>
        /// </summary>
        public byte[] ScanImg
        {
            get { return this._scanImg; }
            set
            {
                if (this._scanImg != value)
                {
                    this.IsDirty = true;
                    this._scanImg = value;
                    OnPropertyChanged(() => ScanImg);
                    PropertyChangedCompleted(() => ScanImg);
                }
            }
        }

        protected bool _isSold;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the IsSold</param>
        /// </summary>
        public bool IsSold
        {
            get { return this._isSold; }
            set
            {
                if (this._isSold != value)
                {
                    this.IsDirty = true;
                    this._isSold = value;
                    OnPropertyChanged(() => IsSold);
                    PropertyChangedCompleted(() => IsSold);
                }
            }
        }

        protected short _status;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Status</param>
        /// </summary>
        public short Status
        {
            get { return this._status; }
            set
            {
                if (this._status != value)
                {
                    this.IsDirty = true;
                    this._status = value;
                    OnPropertyChanged(() => Status);
                    PropertyChangedCompleted(() => Status);
                }
            }
        }

        protected short _cardTypeId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the CardTypeId</param>
        /// </summary>
        public short CardTypeId
        {
            get { return this._cardTypeId; }
            set
            {
                if (this._cardTypeId != value)
                {
                    this.IsDirty = true;
                    this._cardTypeId = value;
                    OnPropertyChanged(() => CardTypeId);
                    PropertyChangedCompleted(() => CardTypeId);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// <param>Public Method</param>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set PropertyModel to Entity</param>
        /// </summary>
        public void ToEntity()
        {
            if (IsNew)
                this.base_CardManagement.Id = this.Id;
            if (this.CardNumber != null)
                this.base_CardManagement.CardNumber = this.CardNumber.Trim();
            this.base_CardManagement.PurchaseDate = this.PurchaseDate;
            this.base_CardManagement.InitialAmount = this.InitialAmount;
            this.base_CardManagement.LastUsed = this.LastUsed;
            this.base_CardManagement.RemainingAmount = this.RemainingAmount;
            if (this.GuestResourcePurchased != null)
                this.base_CardManagement.GuestResourcePurchased = this.GuestResourcePurchased.Trim();
            if (this.GuestGiftedResource != null)
                this.base_CardManagement.GuestGiftedResource = this.GuestGiftedResource.Trim();
            this.base_CardManagement.IsUsed = this.IsUsed;
            if (this.UserCreated != null)
                this.base_CardManagement.UserCreated = this.UserCreated.Trim();
            this.base_CardManagement.DateCreated = this.DateCreated;
            this.base_CardManagement.IsPurged = this.IsPurged;
            if (this.PurgeReason != null)
                this.base_CardManagement.PurgeReason = this.PurgeReason.Trim();
            if (this.ScanCode != null)
                this.base_CardManagement.ScanCode = this.ScanCode.Trim();
            this.base_CardManagement.ScanImg = this.ScanImg;
            this.base_CardManagement.IsSold = this.IsSold;
            this.base_CardManagement.Status = this.Status;
            this.base_CardManagement.CardTypeId = this.CardTypeId;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_CardManagement.Id;
            this._cardNumber = this.base_CardManagement.CardNumber;
            this._purchaseDate = this.base_CardManagement.PurchaseDate;
            this._initialAmount = this.base_CardManagement.InitialAmount;
            this._lastUsed = this.base_CardManagement.LastUsed;
            this._remainingAmount = this.base_CardManagement.RemainingAmount;
            this._guestResourcePurchased = this.base_CardManagement.GuestResourcePurchased;
            this._guestGiftedResource = this.base_CardManagement.GuestGiftedResource;
            this._isUsed = this.base_CardManagement.IsUsed;
            this._userCreated = this.base_CardManagement.UserCreated;
            this._dateCreated = this.base_CardManagement.DateCreated;
            this._isPurged = this.base_CardManagement.IsPurged;
            this._purgeReason = this.base_CardManagement.PurgeReason;
            this._scanCode = this.base_CardManagement.ScanCode;
            this._scanImg = this.base_CardManagement.ScanImg;
            this._isSold = this.base_CardManagement.IsSold;
            this._status = this.base_CardManagement.Status;
            this._cardTypeId = this.base_CardManagement.CardTypeId;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_CardManagement.Id;
            this.CardNumber = this.base_CardManagement.CardNumber;
            this.PurchaseDate = this.base_CardManagement.PurchaseDate;
            this.InitialAmount = this.base_CardManagement.InitialAmount;
            this.LastUsed = this.base_CardManagement.LastUsed;
            this.RemainingAmount = this.base_CardManagement.RemainingAmount;
            this.GuestResourcePurchased = this.base_CardManagement.GuestResourcePurchased;
            this.GuestGiftedResource = this.base_CardManagement.GuestGiftedResource;
            this.IsUsed = this.base_CardManagement.IsUsed;
            this.UserCreated = this.base_CardManagement.UserCreated;
            this.DateCreated = this.base_CardManagement.DateCreated;
            this.IsPurged = this.base_CardManagement.IsPurged;
            this.PurgeReason = this.base_CardManagement.PurgeReason;
            this.ScanCode = this.base_CardManagement.ScanCode;
            this.ScanImg = this.base_CardManagement.ScanImg;
            this.IsSold = this.base_CardManagement.IsSold;
            this.Status = this.base_CardManagement.Status;
            this.CardTypeId = this.base_CardManagement.CardTypeId;
        }

        #endregion

        #region Custom Code

        #region Navigation Properties

        #endregion

        #region Properties

        #region CustomerPurchased

        private string _customerPurchased;
        public string CustomerPurchased
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_guestResourcePurchased))
                {
                    _customerPurchased = "System";
                }
                return _customerPurchased;
            }
            set
            {
                if (_customerPurchased != value)
                {
                    _customerPurchased = value;
                    OnPropertyChanged(() => CustomerPurchased);
                }
            }
        }

        #endregion

        #region CustomerGifted

        private string _customerGifted;
        public string CustomerGifted
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_guestGiftedResource))
                {
                    _customerGifted = "System";
                }
                return _customerGifted;
            }
            set
            {
                if (_customerGifted != value)
                {
                    _customerGifted = value;
                    OnPropertyChanged(() => CustomerGifted);
                }
            }
        }

        #endregion

        #region StatusName

        public string StatusName
        {
            get
            {
                ComboItem item = Common.StatusBasic.FirstOrDefault(x => x.Value == _status);
                if (item != null)
                {
                    return item.Text;
                }

                return null;
            }
        }

        #endregion

        #region CardTypeName

        public string CardTypeName
        {
            get
            {
                ComboItem item = Common.PaymentMethods.FirstOrDefault(x => x.Value == _cardTypeId);
                if (item != null)
                {
                    return item.Text;
                }

                return null;
            }
        }

        #endregion

        #region CanEditCardNumber

        public bool CanEditCardNumber
        {
            get
            {
                return CanEdit && Define.CONFIGURATION.IsManualGenerate;
            }
        }

        #endregion

        #region CanEdit

        public bool CanEdit
        {
            get
            {
                return !_isSold && string.IsNullOrWhiteSpace(_guestResourcePurchased);
            }
        }

        #endregion

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

        #region IsSelected

        private bool _isSelected;
        /// <summary>
        /// Gets or sets whether this object is selected. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }

        #endregion

        #region UnusedDuration

        public double UnusedDuration
        {
            get
            {
                if (_lastUsed.HasValue)
                {
                    return (DateTime.Now.Date - _lastUsed.Value.Date).TotalDays;
                }

                return 0;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region ResetCard

        /// <summary>
        /// Reset card.
        /// </summary>
        public void ResetCard()
        {
            this.PurchaseDate = null;
            this.LastUsed = null;
            this.GuestResourcePurchased = string.Empty;
            this.GuestGiftedResource = string.Empty;
            this.IsUsed = false;
        }

        #endregion

        #region Restore

        /// <summary>
        /// Restore.
        /// </summary>
        public void Restore()
        {
            ToModelAndRaise();
        }

        #endregion

        #endregion

        #region Override Methods

        protected override void PropertyChangedCompleted(string propertyName)
        {
            switch (propertyName)
            {
                case "Status":
                    OnPropertyChanged(() => StatusName);
                    break;

                case "CardTypeId":
                    OnPropertyChanged(() => CardTypeName);
                    break;

                case "InitialAmount":
                    if (CanEdit)
                    {
                        RemainingAmount = _initialAmount;
                    }
                    break;
            }
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
                string message = null;

                switch (columnName)
                {
                    case "CardNumber":

                        if (string.IsNullOrWhiteSpace(_cardNumber))
                        {
                            message = "Required not empty";
                        }
                        else if (_cardNumber.Length < 11 || _cardNumber.Length > 12)
                        {
                            message = "Card Number length must be 11 or 12";
                        }

                        break;

                    case "InitialAmount":

                        if (_initialAmount <= 0)
                        {
                            message = "Required greater than 0";
                        }

                        break;
                }

                return message;
            }
        }

        #endregion

        #endregion
    }
}
