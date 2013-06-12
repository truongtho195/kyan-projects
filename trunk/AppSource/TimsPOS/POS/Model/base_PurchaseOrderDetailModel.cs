//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CPC.POS.Database;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table base_PurchaseOrderDetail
    /// </summary>
    [Serializable]
    public partial class base_PurchaseOrderDetailModel : ModelBase, IDataErrorInfo, IEditableObject
    {
        #region Constructor

        // Default constructor
        public base_PurchaseOrderDetailModel()
        {
            this.IsNew = true;
            this.base_PurchaseOrderDetail = new base_PurchaseOrderDetail();
        }

        // Default constructor that set entity to field
        public base_PurchaseOrderDetailModel(base_PurchaseOrderDetail base_purchaseorderdetail, bool isRaiseProperties = false)
        {
            this.base_PurchaseOrderDetail = base_purchaseorderdetail;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_PurchaseOrderDetail base_PurchaseOrderDetail
        {
            get;
            private set;
        }

        #endregion

        #region Primitive Properties

        protected long _id;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Id</para>
        /// </summary>
        public long Id
        {
            get
            {
                return this._id;
            }
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

        protected long _purchaseOrderId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the PurchaseOrderId</para>
        /// </summary>
        public long PurchaseOrderId
        {
            get
            {
                return this._purchaseOrderId;
            }
            set
            {
                if (this._purchaseOrderId != value)
                {
                    this.IsDirty = true;
                    this._purchaseOrderId = value;
                    OnPropertyChanged(() => PurchaseOrderId);
                    PropertyChangedCompleted(() => PurchaseOrderId);
                }
            }
        }

        protected string _productResource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductResource</para>
        /// </summary>
        public string ProductResource
        {
            get
            {
                return this._productResource;
            }
            set
            {
                if (this._productResource != value)
                {
                    this.IsDirty = true;
                    this._productResource = value;
                    OnPropertyChanged(() => ProductResource);
                    PropertyChangedCompleted(() => ProductResource);
                }
            }
        }

        protected string _itemCode;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ItemCode</para>
        /// </summary>
        public string ItemCode
        {
            get
            {
                return this._itemCode;
            }
            set
            {
                if (this._itemCode != value)
                {
                    this.IsDirty = true;
                    this._itemCode = value;
                    OnPropertyChanged(() => ItemCode);
                    PropertyChangedCompleted(() => ItemCode);
                }
            }
        }

        protected string _itemName;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ItemName</para>
        /// </summary>
        public string ItemName
        {
            get
            {
                return this._itemName;
            }
            set
            {
                if (this._itemName != value)
                {
                    this.IsDirty = true;
                    this._itemName = value;
                    OnPropertyChanged(() => ItemName);
                    PropertyChangedCompleted(() => ItemName);
                }
            }
        }

        protected string _itemAtribute;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ItemAtribute</para>
        /// </summary>
        public string ItemAtribute
        {
            get
            {
                return this._itemAtribute;
            }
            set
            {
                if (this._itemAtribute != value)
                {
                    this.IsDirty = true;
                    this._itemAtribute = value;
                    OnPropertyChanged(() => ItemAtribute);
                    PropertyChangedCompleted(() => ItemAtribute);
                }
            }
        }

        protected string _itemSize;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ItemSize</para>
        /// </summary>
        public string ItemSize
        {
            get
            {
                return this._itemSize;
            }
            set
            {
                if (this._itemSize != value)
                {
                    this.IsDirty = true;
                    this._itemSize = value;
                    OnPropertyChanged(() => ItemSize);
                    PropertyChangedCompleted(() => ItemSize);
                }
            }
        }

        protected string _baseUOM;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the BaseUOM</para>
        /// </summary>
        public string BaseUOM
        {
            get
            {
                return this._baseUOM;
            }
            set
            {
                if (this._baseUOM != value)
                {
                    this.IsDirty = true;
                    this._baseUOM = value;
                    OnPropertyChanged(() => BaseUOM);
                    PropertyChangedCompleted(() => BaseUOM);
                }
            }
        }

        protected int _uOMId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UOMId</para>
        /// </summary>
        public int UOMId
        {
            get
            {
                return this._uOMId;
            }
            set
            {
                if (this._uOMId != value)
                {
                    this.IsDirty = true;
                    this._uOMId = value;
                    OnPropertyChanged(() => UOMId);
                    PropertyChangedCompleted(() => UOMId);
                }
            }
        }

        protected decimal _price;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Price</para>
        /// </summary>
        public decimal Price
        {
            get
            {
                return this._price;
            }
            set
            {
                if (this._price != value)
                {
                    this.IsDirty = true;
                    this._price = value;
                    OnPropertyChanged(() => Price);
                    PropertyChangedCompleted(() => Price);
                }
            }
        }

        protected int _quantity;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Quantity</para>
        /// </summary>
        public int Quantity
        {
            get
            {
                return this._quantity;
            }
            set
            {
                if (this._quantity != value)
                {
                    this.IsDirty = true;
                    this._quantity = value;
                    OnPropertyChanged(() => Quantity);
                    PropertyChangedCompleted(() => Quantity);
                }
            }
        }

        protected int _receivedQty;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ReceivedQty</para>
        /// </summary>
        public int ReceivedQty
        {
            get
            {
                return this._receivedQty;
            }
            set
            {
                if (this._receivedQty != value)
                {
                    this.IsDirty = true;
                    this._receivedQty = value;
                    OnPropertyChanged(() => ReceivedQty);
                    PropertyChangedCompleted(() => ReceivedQty);
                }
            }
        }

        protected int _dueQty;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DueQty</para>
        /// </summary>
        public int DueQty
        {
            get
            {
                return this._dueQty;
            }
            set
            {
                if (this._dueQty != value)
                {
                    this.IsDirty = true;
                    this._dueQty = value;
                    OnPropertyChanged(() => DueQty);
                    PropertyChangedCompleted(() => DueQty);
                }
            }
        }

        protected decimal _unFilledQty;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UnFilledQty</para>
        /// </summary>
        public decimal UnFilledQty
        {
            get
            {
                return this._unFilledQty;
            }
            set
            {
                if (this._unFilledQty != value)
                {
                    this.IsDirty = true;
                    this._unFilledQty = value;
                    OnPropertyChanged(() => UnFilledQty);
                    PropertyChangedCompleted(() => UnFilledQty);
                }
            }
        }

        protected Nullable<decimal> _amount;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Amount</para>
        /// </summary>
        public Nullable<decimal> Amount
        {
            get
            {
                return this._amount;
            }
            set
            {
                if (this._amount != value)
                {
                    this.IsDirty = true;
                    this._amount = value;
                    OnPropertyChanged(() => Amount);
                    PropertyChangedCompleted(() => Amount);
                }
            }
        }

        protected string _serial;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Serial</para>
        /// </summary>
        public string Serial
        {
            get
            {
                return this._serial;
            }
            set
            {
                if (this._serial != value)
                {
                    this.IsDirty = true;
                    this._serial = value;
                    OnPropertyChanged(() => Serial);
                    PropertyChangedCompleted(() => Serial);
                }
            }
        }

        protected Nullable<System.DateTime> _lastReceived;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the LastReceived</para>
        /// </summary>
        public Nullable<System.DateTime> LastReceived
        {
            get
            {
                return this._lastReceived;
            }
            set
            {
                if (this._lastReceived != value)
                {
                    this.IsDirty = true;
                    this._lastReceived = value;
                    OnPropertyChanged(() => LastReceived);
                    PropertyChangedCompleted(() => LastReceived);
                }
            }
        }

        protected System.Guid _resource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Resource</para>
        /// </summary>
        public System.Guid Resource
        {
            get
            {
                return this._resource;
            }
            set
            {
                if (this._resource != value)
                {
                    this.IsDirty = true;
                    this._resource = value;
                    OnPropertyChanged(() => Resource);
                    PropertyChangedCompleted(() => Resource);
                }
            }
        }

        protected bool _isFullReceived;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsFullReceived</para>
        /// </summary>
        public bool IsFullReceived
        {
            get
            {
                return this._isFullReceived;
            }
            set
            {
                if (this._isFullReceived != value)
                {
                    this.IsDirty = true;
                    this._isFullReceived = value;
                    OnPropertyChanged(() => IsFullReceived);
                    PropertyChangedCompleted(() => IsFullReceived);
                }
            }
        }

        protected decimal _discount;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Discount</para>
        /// </summary>
        public decimal Discount
        {
            get
            {
                return this._discount;
            }
            set
            {
                if (this._discount != value)
                {
                    this.IsDirty = true;
                    this._discount = value;
                    OnPropertyChanged(() => Discount);
                    PropertyChangedCompleted(() => Discount);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// <para>Public Method</para>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set PropertyModel to Entity</para>
        /// </summary>
        public void ToEntity()
        {
            if (IsNew)
                this.base_PurchaseOrderDetail.Id = this.Id;
            this.base_PurchaseOrderDetail.PurchaseOrderId = this.PurchaseOrderId;
            this.base_PurchaseOrderDetail.ProductResource = this.ProductResource;
            this.base_PurchaseOrderDetail.ItemCode = this.ItemCode;
            this.base_PurchaseOrderDetail.ItemName = this.ItemName;
            this.base_PurchaseOrderDetail.ItemAtribute = this.ItemAtribute;
            this.base_PurchaseOrderDetail.ItemSize = this.ItemSize;
            this.base_PurchaseOrderDetail.BaseUOM = this.BaseUOM;
            this.base_PurchaseOrderDetail.UOMId = this.UOMId;
            this.base_PurchaseOrderDetail.Price = this.Price;
            this.base_PurchaseOrderDetail.Quantity = this.Quantity;
            this.base_PurchaseOrderDetail.ReceivedQty = this.ReceivedQty;
            this.base_PurchaseOrderDetail.DueQty = this.DueQty;
            this.base_PurchaseOrderDetail.UnFilledQty = this.UnFilledQty;
            this.base_PurchaseOrderDetail.Amount = this.Amount;
            this.base_PurchaseOrderDetail.Serial = this.Serial;
            this.base_PurchaseOrderDetail.LastReceived = this.LastReceived;
            this.base_PurchaseOrderDetail.Resource = this.Resource;
            this.base_PurchaseOrderDetail.IsFullReceived = this.IsFullReceived;
            this.base_PurchaseOrderDetail.Discount = this.Discount;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_PurchaseOrderDetail.Id;
            this._purchaseOrderId = this.base_PurchaseOrderDetail.PurchaseOrderId;
            this._productResource = this.base_PurchaseOrderDetail.ProductResource;
            this._itemCode = this.base_PurchaseOrderDetail.ItemCode;
            this._itemName = this.base_PurchaseOrderDetail.ItemName;
            this._itemAtribute = this.base_PurchaseOrderDetail.ItemAtribute;
            this._itemSize = this.base_PurchaseOrderDetail.ItemSize;
            this._baseUOM = this.base_PurchaseOrderDetail.BaseUOM;
            this._uOMId = this.base_PurchaseOrderDetail.UOMId;
            this._price = this.base_PurchaseOrderDetail.Price;
            this._quantity = this.base_PurchaseOrderDetail.Quantity;
            this._receivedQty = this.base_PurchaseOrderDetail.ReceivedQty;
            this._dueQty = this.base_PurchaseOrderDetail.DueQty;
            this._unFilledQty = this.base_PurchaseOrderDetail.UnFilledQty;
            this._amount = this.base_PurchaseOrderDetail.Amount;
            this._serial = this.base_PurchaseOrderDetail.Serial;
            this._lastReceived = this.base_PurchaseOrderDetail.LastReceived;
            this._resource = this.base_PurchaseOrderDetail.Resource;
            this._isFullReceived = this.base_PurchaseOrderDetail.IsFullReceived;
            this._discount = this.base_PurchaseOrderDetail.Discount;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_PurchaseOrderDetail.Id;
            this.PurchaseOrderId = this.base_PurchaseOrderDetail.PurchaseOrderId;
            this.ProductResource = this.base_PurchaseOrderDetail.ProductResource;
            this.ItemCode = this.base_PurchaseOrderDetail.ItemCode;
            this.ItemName = this.base_PurchaseOrderDetail.ItemName;
            this.ItemAtribute = this.base_PurchaseOrderDetail.ItemAtribute;
            this.ItemSize = this.base_PurchaseOrderDetail.ItemSize;
            this.BaseUOM = this.base_PurchaseOrderDetail.BaseUOM;
            this.UOMId = this.base_PurchaseOrderDetail.UOMId;
            this.Price = this.base_PurchaseOrderDetail.Price;
            this.Quantity = this.base_PurchaseOrderDetail.Quantity;
            this.ReceivedQty = this.base_PurchaseOrderDetail.ReceivedQty;
            this.DueQty = this.base_PurchaseOrderDetail.DueQty;
            this.UnFilledQty = this.base_PurchaseOrderDetail.UnFilledQty;
            this.Amount = this.base_PurchaseOrderDetail.Amount;
            this.Serial = this.base_PurchaseOrderDetail.Serial;
            this.LastReceived = this.base_PurchaseOrderDetail.LastReceived;
            this.Resource = this.base_PurchaseOrderDetail.Resource;
            this.IsFullReceived = this.base_PurchaseOrderDetail.IsFullReceived;
            this.Discount = this.base_PurchaseOrderDetail.Discount;
        }

        #endregion

        #region Custom Code

        #region Fields

        /// <summary>
        /// Holds backup of this object.
        /// </summary>
        private base_PurchaseOrderDetailModel _backup;

        private short _oldStatus;

        #endregion

        #region Navigation Properties

        #region PurchaseOrder

        private base_PurchaseOrderModel _purchaseOrder;
        /// <summary>
        /// Gets or sets PurchaseOrder.
        /// </summary>
        public base_PurchaseOrderModel PurchaseOrder
        {
            get
            {
                return _purchaseOrder;
            }
            set
            {
                if (_purchaseOrder != value)
                {
                    _purchaseOrder = value;
                    OnPropertyChanged(() => PurchaseOrder);
                }
            }
        }

        #endregion

        #endregion

        #region Properties

        #region UOMCollection

        private CollectionBase<base_ProductUOMModel> _UOMCollection;
        /// <summary>
        /// Gets or sets UOMCollection that contains units of product in this object.
        /// </summary>
        public CollectionBase<base_ProductUOMModel> UOMCollection
        {
            get
            {
                return _UOMCollection;
            }
            set
            {
                if (_UOMCollection != value)
                {
                    _UOMCollection = value;
                    OnPropertyChanged(() => UOMCollection);
                }
            }
        }

        #endregion

        #region UnitName

        private string _unitName;
        /// <summary>
        /// Gets or sets unit's name that this object is holding.
        /// </summary>
        public string UnitName
        {
            get
            {
                return _unitName;
            }
            set
            {
                if (_unitName != value)
                {
                    _isDirty = true;
                    _unitName = value;
                    OnPropertyChanged(() => UnitName);
                }
            }
        }

        #endregion

        #region OnHandQty

        private int _onHandQty;
        /// <summary>
        /// Gets or sets OnHandQty.
        /// </summary>
        public int OnHandQty
        {
            get
            {
                return _onHandQty;
            }
            set
            {
                if (_onHandQty != value)
                {
                    _isDirty = true;
                    _onHandQty = value;
                    OnPropertyChanged(() => OnHandQty);
                }
            }
        }

        #endregion

        #region OnHandQtyTemp

        private int _onHandQtyTemp;
        /// <summary>
        /// Gets or sets OnHandQtyTemp.
        /// </summary>
        public int OnHandQtyTemp
        {
            get
            {
                return _onHandQtyTemp;
            }
            set
            {
                if (_onHandQtyTemp != value)
                {
                    _isDirty = true;
                    _onHandQtyTemp = value;
                    OnPropertyChanged(() => OnHandQtyTemp);
                }
            }
        }

        #endregion

        #region BackupQuantity

        private Nullable<decimal> _backupQuantity;
        /// <summary>
        /// Gets or sets BackupQuantity that holds quantity.
        /// </summary>
        public Nullable<decimal> BackupQuantity
        {
            get
            {
                return _backupQuantity;
            }
            set
            {
                if (_backupQuantity != value)
                {
                    _isDirty = true;
                    _backupQuantity = value;
                    OnPropertyChanged(() => BackupQuantity);
                }
            }
        }

        #endregion

        #region IsSerialTracking

        private bool _isSerialTracking;
        /// <summary>
        /// Gets or sets IsSerialTracking.
        /// </summary>
        public bool IsSerialTracking
        {
            get
            {
                return _isSerialTracking;
            }
            set
            {
                if (_isSerialTracking != value)
                {
                    _isDirty = true;
                    _isSerialTracking = value;
                    OnPropertyChanged(() => IsSerialTracking);
                }
            }
        }

        #endregion

        #region HasReceivedItem

        /// <summary>
        /// Gets value indicate that this object has received item or not.
        /// </summary>
        public bool HasReceivedItem
        {
            get
            {
                if (_purchaseOrder != null && _purchaseOrder.PurchaseOrderReceiveCollection != null)
                {
                    return _purchaseOrder.PurchaseOrderReceiveCollection.Any(x => x.PODResource == _resource.ToString() && x.IsReceived);
                }

                return false;
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

        #region SomeSerial

        /// <summary>
        /// Gets SomeSerial.
        /// </summary>
        public string SomeSerial
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_serial))
                {
                    string[] serials = _serial.Split(new string[] { "," }, StringSplitOptions.None);
                    if (serials.Length > Define.NumberOfSerialDisplay)
                    {
                        return string.Join(",", serials.Where((s, i) => i < Define.NumberOfSerialDisplay)) + ", ...";
                    }
                }
                return _serial;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region RaiseQuantityChanged

        /// <summary>
        /// Raise quantity changed.
        /// </summary>
        public void RaiseQuantityChanged()
        {
            OnPropertyChanged(() => Quantity);
        }

        #endregion

        #region RaiseHasReceivedItemChanged

        /// <summary>
        /// Raise HasReceivedItem changed.
        /// </summary>
        //public void RaiseHasReceivedItemChanged()
        //{
        //    OnPropertyChanged(() => HasReceivedItem);
        //}

        #endregion

        #region ShallowClone

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        /// <returns>A shallow copy of this object.</returns>
        public base_PurchaseOrderDetailModel ShallowClone()
        {
            return (base_PurchaseOrderDetailModel)this.MemberwiseClone();
        }

        #endregion

        #region GetOldStatusPurchaseOrder

        /// <summary>
        /// Gets old status perchase order.
        /// </summary>
        public short GetOldStatusPurchaseOrder()
        {
            return _oldStatus;
        }

        #endregion

        #endregion

        #region Override Methods

        #region PropertyChangedCompleted

        protected override void PropertyChangedCompleted(string propertyName)
        {
            switch (propertyName)
            {
                case "Serial":

                    OnPropertyChanged(() => SomeSerial);

                    break;
            }
        }

        #endregion

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
                    case "Quantity":

                        if (Define.CONFIGURATION.IsAllowRGO != true && _quantity < _receivedQty)
                        {
                            message = "Quantity >= ReceivedQty";
                        }

                        break;
                }

                return message;
            }
        }

        #endregion

        #region IEditableObject Members

        void IEditableObject.BeginEdit()
        {
            if (_backup == null)
            {
                _backup = ShallowClone();
                if (_backup.PurchaseOrder != null)
                {
                    _oldStatus = _backup.PurchaseOrder.Status;
                }
            }
        }

        void IEditableObject.CancelEdit()
        {
            if (_backup != null)
            {
                Id = _backup.Id;
                PurchaseOrderId = _backup.PurchaseOrderId;
                ProductResource = _backup.ProductResource;
                ItemCode = _backup.ItemCode;
                ItemName = _backup.ItemName;
                ItemAtribute = _backup.ItemAtribute;
                ItemSize = _backup.ItemSize;
                UOMId = _backup.UOMId;
                BaseUOM = _backup.BaseUOM;
                Price = _backup.Price;
                UnitName = _backup.UnitName;
                OnHandQty = _backup.OnHandQty;
                OnHandQtyTemp = _backup.OnHandQtyTemp;
                Quantity = _backup.Quantity;
                Discount = _backup.Discount;
                BackupQuantity = _backup.BackupQuantity;
                ReceivedQty = _backup.ReceivedQty;
                DueQty = _backup.DueQty;
                UnFilledQty = _backup.UnFilledQty;
                Amount = _backup.Amount;
                Serial = _backup.Serial;
                LastReceived = _backup.LastReceived;
                IsFullReceived = _backup.IsFullReceived;
                IsSerialTracking = _backup.IsSerialTracking;
                Resource = _backup.Resource;
                IsTemporary = _backup.IsTemporary;
                IsChecked = _backup.IsChecked;
                IsDeleted = _backup.IsDeleted;
                IsNew = _backup.IsNew;
                IsDirty = _backup.IsDirty;
                _backup = null;

                if (_purchaseOrder != null)
                {
                    _purchaseOrder.Status = _oldStatus;
                }
            }
        }

        void IEditableObject.EndEdit()
        {
            if (_backup != null)
            {
                _backup = null;
                IsTemporary = false;
            }
        }

        #endregion

        #endregion
    }
}
