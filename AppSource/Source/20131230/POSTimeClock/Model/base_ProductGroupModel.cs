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
using CPC.Toolkit.Base;
using CPC.TimeClock.Database;

namespace CPC.TimeClock.Model
{
    /// <summary>
    /// Model for table base_ProductGroup
    /// </summary>
    [Serializable]
    public partial class base_ProductGroupModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_ProductGroupModel()
        {
            this.IsNew = true;
            this.base_ProductGroup = new base_ProductGroup();
        }

        // Default constructor that set entity to field
        public base_ProductGroupModel(base_ProductGroup base_productgroup, bool isRaiseProperties = false)
        {
            this.base_ProductGroup = base_productgroup;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_ProductGroup base_ProductGroup { get; private set; }

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

        protected Nullable<long> _productParentId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ProductParentId</param>
        /// </summary>
        public Nullable<long> ProductParentId
        {
            get { return this._productParentId; }
            set
            {
                if (this._productParentId != value)
                {
                    this.IsDirty = true;
                    this._productParentId = value;
                    OnPropertyChanged(() => ProductParentId);
                    PropertyChangedCompleted(() => ProductParentId);
                }
            }
        }

        protected long _productId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ProductId</param>
        /// </summary>
        public long ProductId
        {
            get { return this._productId; }
            set
            {
                if (this._productId != value)
                {
                    this.IsDirty = true;
                    this._productId = value;
                    OnPropertyChanged(() => ProductId);
                    PropertyChangedCompleted(() => ProductId);
                }
            }
        }

        protected string _productResource;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ProductResource</param>
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
        /// <param>Gets or sets the ItemCode</param>
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
        /// <param>Gets or sets the ItemName</param>
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

        protected string _itemAttribute;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ItemAttribute</param>
        /// </summary>
        public string ItemAttribute
        {
            get { return this._itemAttribute; }
            set
            {
                if (this._itemAttribute != value)
                {
                    this.IsDirty = true;
                    this._itemAttribute = value;
                    OnPropertyChanged(() => ItemAttribute);
                    PropertyChangedCompleted(() => ItemAttribute);
                }
            }
        }

        protected string _itemSize;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the ItemSize</param>
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

        protected decimal _quantity;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Quantity</param>
        /// </summary>
        public decimal Quantity
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

        protected decimal _regularPrice;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the RegularPrice</param>
        /// </summary>
        public decimal RegularPrice
        {
            get { return this._regularPrice; }
            set
            {
                if (this._regularPrice != value)
                {
                    this.IsDirty = true;
                    this._regularPrice = value;
                    OnPropertyChanged(() => RegularPrice);
                    PropertyChangedCompleted(() => RegularPrice);
                }
            }
        }

        protected int _uOMId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the UOMId</param>
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

        protected string _uOM;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the UOM</param>
        /// </summary>
        public string UOM
        {
            get { return this._uOM; }
            set
            {
                if (this._uOM != value)
                {
                    this.IsDirty = true;
                    this._uOM = value;
                    OnPropertyChanged(() => UOM);
                    PropertyChangedCompleted(() => UOM);
                }
            }
        }

        protected decimal _amount;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Amount</param>
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

        protected decimal _onHandQty;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the OnHandQty</param>
        /// </summary>
        public decimal OnHandQty
        {
            get { return this._onHandQty; }
            set
            {
                if (this._onHandQty != value)
                {
                    this.IsDirty = true;
                    this._onHandQty = value;
                    OnPropertyChanged(() => OnHandQty);
                    PropertyChangedCompleted(() => OnHandQty);
                }
            }
        }

        protected System.Guid _resource;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Resource</param>
        /// </summary>
        public System.Guid Resource
        {
            get { return this._resource; }
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
                this.base_ProductGroup.Id = this.Id;
            this.base_ProductGroup.ProductParentId = this.ProductParentId;
            this.base_ProductGroup.ProductId = this.ProductId;
            if (this.ProductResource != null)
                this.base_ProductGroup.ProductResource = this.ProductResource.Trim();
            if (this.ItemCode != null)
                this.base_ProductGroup.ItemCode = this.ItemCode.Trim();
            if (this.ItemName != null)
                this.base_ProductGroup.ItemName = this.ItemName.Trim();
            if (this.ItemAttribute != null)
                this.base_ProductGroup.ItemAttribute = this.ItemAttribute.Trim();
            if (this.ItemSize != null)
                this.base_ProductGroup.ItemSize = this.ItemSize.Trim();
            this.base_ProductGroup.Quantity = this.Quantity;
            this.base_ProductGroup.RegularPrice = this.RegularPrice;
            this.base_ProductGroup.UOMId = this.UOMId;
            if (this.UOM != null)
                this.base_ProductGroup.UOM = this.UOM.Trim();
            this.base_ProductGroup.Amount = this.Amount;
            this.base_ProductGroup.OnHandQty = this.OnHandQty;
            this.base_ProductGroup.Resource = this.Resource;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_ProductGroup.Id;
            this._productParentId = this.base_ProductGroup.ProductParentId;
            this._productId = this.base_ProductGroup.ProductId;
            this._productResource = this.base_ProductGroup.ProductResource;
            this._itemCode = this.base_ProductGroup.ItemCode;
            this._itemName = this.base_ProductGroup.ItemName;
            this._itemAttribute = this.base_ProductGroup.ItemAttribute;
            this._itemSize = this.base_ProductGroup.ItemSize;
            this._quantity = this.base_ProductGroup.Quantity;
            this._regularPrice = this.base_ProductGroup.RegularPrice;
            this._uOMId = this.base_ProductGroup.UOMId;
            this._uOM = this.base_ProductGroup.UOM;
            this._amount = this.base_ProductGroup.Amount;
            this._onHandQty = this.base_ProductGroup.OnHandQty;
            this._resource = this.base_ProductGroup.Resource;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_ProductGroup.Id;
            this.ProductParentId = this.base_ProductGroup.ProductParentId;
            this.ProductId = this.base_ProductGroup.ProductId;
            this.ProductResource = this.base_ProductGroup.ProductResource;
            this.ItemCode = this.base_ProductGroup.ItemCode;
            this.ItemName = this.base_ProductGroup.ItemName;
            this.ItemAttribute = this.base_ProductGroup.ItemAttribute;
            this.ItemSize = this.base_ProductGroup.ItemSize;
            this.Quantity = this.base_ProductGroup.Quantity;
            this.RegularPrice = this.base_ProductGroup.RegularPrice;
            this.UOMId = this.base_ProductGroup.UOMId;
            this.UOM = this.base_ProductGroup.UOM;
            this.Amount = this.base_ProductGroup.Amount;
            this.OnHandQty = this.base_ProductGroup.OnHandQty;
            this.Resource = this.base_ProductGroup.Resource;
        }

        #endregion

        #region Custom Code


        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
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
                    case "ProductParentId":
                        break;
                    case "ProductId":
                        break;
                    case "ProductResource":
                        break;
                    case "ItemCode":
                        break;
                    case "ItemName":
                        break;
                    case "ItemAttribute":
                        break;
                    case "ItemSize":
                        break;
                    case "Quantity":
                        break;
                    case "RegularPrice":
                        break;
                    case "UOMId":
                        break;
                    case "UOM":
                        break;
                    case "Amount":
                        break;
                    case "OnHandQty":
                        break;
                    case "Resource":
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