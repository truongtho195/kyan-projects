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
using CPC.POS.Database;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table base_TransferStockDetail
    /// </summary>
    [Serializable]
    public partial class base_TransferStockDetailModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_TransferStockDetailModel()
        {
            this.IsNew = true;
            this.base_TransferStockDetail = new base_TransferStockDetail();
        }

        // Default constructor that set entity to field
        public base_TransferStockDetailModel(base_TransferStockDetail base_transferstockdetail, bool isRaiseProperties = false)
        {
            this.base_TransferStockDetail = base_transferstockdetail;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_TransferStockDetail base_TransferStockDetail { get; private set; }

        #endregion

        #region Primitive Properties

        protected long _id;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Id</para>
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

        protected long _transferStockId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the TransferStockId</para>
        /// </summary>
        public long TransferStockId
        {
            get { return this._transferStockId; }
            set
            {
                if (this._transferStockId != value)
                {
                    this.IsDirty = true;
                    this._transferStockId = value;
                    OnPropertyChanged(() => TransferStockId);
                    PropertyChangedCompleted(() => TransferStockId);
                }
            }
        }

        protected string _transferStockResource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the TransferStockResource</para>
        /// </summary>
        public string TransferStockResource
        {
            get { return this._transferStockResource; }
            set
            {
                if (this._transferStockResource != value)
                {
                    this.IsDirty = true;
                    this._transferStockResource = value;
                    OnPropertyChanged(() => TransferStockResource);
                    PropertyChangedCompleted(() => TransferStockResource);
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
            get { return this._productResource; }
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
            get { return this._itemCode; }
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
            get { return this._itemName; }
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
            get { return this._itemAtribute; }
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
            get { return this._itemSize; }
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

        protected int _quantity;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Quantity</para>
        /// </summary>
        public int Quantity
        {
            get { return this._quantity; }
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

        protected int _uOMId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UOMId</para>
        /// </summary>
        public int UOMId
        {
            get { return this._uOMId; }
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

        protected string _baseUOM;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the BaseUOM</para>
        /// </summary>
        public string BaseUOM
        {
            get { return this._baseUOM; }
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

        protected decimal _amount;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Amount</para>
        /// </summary>
        public decimal Amount
        {
            get { return this._amount; }
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

        protected string _serialTracking;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the SerialTracking</para>
        /// </summary>
        public string SerialTracking
        {
            get { return this._serialTracking; }
            set
            {
                if (this._serialTracking != value)
                {
                    this.IsDirty = true;
                    this._serialTracking = value;
                    OnPropertyChanged(() => SerialTracking);
                    PropertyChangedCompleted(() => SerialTracking);
                }
            }
        }

        protected int _avlQuantity;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AvlQuantity</para>
        /// </summary>
        public int AvlQuantity
        {
            get { return this._avlQuantity; }
            set
            {
                if (this._avlQuantity != value)
                {
                    this.IsDirty = true;
                    this._avlQuantity = value;
                    OnPropertyChanged(() => AvlQuantity);
                    PropertyChangedCompleted(() => AvlQuantity);
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
                this.base_TransferStockDetail.Id = this.Id;
            this.base_TransferStockDetail.TransferStockId = this.TransferStockId;
            this.base_TransferStockDetail.TransferStockResource = this.TransferStockResource;
            this.base_TransferStockDetail.ProductResource = this.ProductResource;
            this.base_TransferStockDetail.ItemCode = this.ItemCode;
            this.base_TransferStockDetail.ItemName = this.ItemName;
            this.base_TransferStockDetail.ItemAtribute = this.ItemAtribute;
            this.base_TransferStockDetail.ItemSize = this.ItemSize;
            this.base_TransferStockDetail.Quantity = this.Quantity;
            this.base_TransferStockDetail.UOMId = this.UOMId;
            this.base_TransferStockDetail.BaseUOM = this.BaseUOM;
            this.base_TransferStockDetail.Amount = this.Amount;
            this.base_TransferStockDetail.SerialTracking = this.SerialTracking;
            this.base_TransferStockDetail.AvlQuantity = this.AvlQuantity;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_TransferStockDetail.Id;
            this._transferStockId = this.base_TransferStockDetail.TransferStockId;
            this._transferStockResource = this.base_TransferStockDetail.TransferStockResource;
            this._productResource = this.base_TransferStockDetail.ProductResource;
            this._itemCode = this.base_TransferStockDetail.ItemCode;
            this._itemName = this.base_TransferStockDetail.ItemName;
            this._itemAtribute = this.base_TransferStockDetail.ItemAtribute;
            this._itemSize = this.base_TransferStockDetail.ItemSize;
            this._quantity = this.base_TransferStockDetail.Quantity;
            this._uOMId = this.base_TransferStockDetail.UOMId;
            this._baseUOM = this.base_TransferStockDetail.BaseUOM;
            this._amount = this.base_TransferStockDetail.Amount;
            this._serialTracking = this.base_TransferStockDetail.SerialTracking;
            this._avlQuantity = this.base_TransferStockDetail.AvlQuantity;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_TransferStockDetail.Id;
            this.TransferStockId = this.base_TransferStockDetail.TransferStockId;
            this.TransferStockResource = this.base_TransferStockDetail.TransferStockResource;
            this.ProductResource = this.base_TransferStockDetail.ProductResource;
            this.ItemCode = this.base_TransferStockDetail.ItemCode;
            this.ItemName = this.base_TransferStockDetail.ItemName;
            this.ItemAtribute = this.base_TransferStockDetail.ItemAtribute;
            this.ItemSize = this.base_TransferStockDetail.ItemSize;
            this.Quantity = this.base_TransferStockDetail.Quantity;
            this.UOMId = this.base_TransferStockDetail.UOMId;
            this.BaseUOM = this.base_TransferStockDetail.BaseUOM;
            this.Amount = this.base_TransferStockDetail.Amount;
            this.SerialTracking = this.base_TransferStockDetail.SerialTracking;
            this.AvlQuantity = this.base_TransferStockDetail.AvlQuantity;
        }

        #endregion

        #region Custom Code
       
        #region IsLoad
        protected bool _isLoad;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Id</para>
        /// </summary>
        public bool IsLoad
        {
            get { return this._isLoad; }
            set
            {
                if (this._isLoad != value)
                {
                    this._isLoad = value;
                    OnPropertyChanged(() => IsLoad);
                    PropertyChangedCompleted(() => IsLoad);
                }
            }
        }
        #endregion

        #region Price
        protected decimal _price;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the RegularPrice</para>
        /// </summary>
        public decimal Price
        {
            get { return this._price; }
            set
            {
                if (this._price != value)
                {
                    this._price = value;
                    OnPropertyChanged(() => Price);
                }
            }
        }
        #endregion

        #region ProductModel
        private base_ProductModel _productModel;
        /// <summary>
        /// Gets or sets the ProductModel.
        /// </summary>
        public base_ProductModel ProductModel
        {
            get { return _productModel; }
            set
            {
                if (_productModel != value)
                {
                    _productModel = value;
                    OnPropertyChanged(() => ProductModel);
                }
            }
        }
        #endregion

        #region ProductUOMCollection
        private ObservableCollection<base_ProductUOMModel> _productUOMCollection;
        /// <summary>
        /// Gets or sets the ProductUOMCollection.
        /// </summary>
        public ObservableCollection<base_ProductUOMModel> ProductUOMCollection
        {
            get { return _productUOMCollection; }
            set
            {
                if (_productUOMCollection != value)
                {
                    _productUOMCollection = value;
                    OnPropertyChanged(() => ProductUOMCollection);
                }
            }
        }
        #endregion

        #region CreateNewTransferDetail
        public void CreateNewTransferDetail()
        {
            this.IsNew = true;
            this.base_TransferStockDetail = new base_TransferStockDetail();
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
                string message = string.Empty;

                switch (columnName)
                {
                    case "Id":
                        break;
                    case "SaleOrderId":
                        break;
                    case "ProductResource":
                        break;
                    case "ItemCode":
                        break;
                    case "ItemName":
                        break;
                    case "ItemAtribute":
                        break;
                    case "ItemSize":
                        break;
                    case "Quantity":
                        break;
                    case "UOMId":
                        break;
                    case "BaseUOM":
                        break;
                    case "Amount":
                        break;
                    case "SerialTracking":
                        break;
                    case "AvlQuantity":
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