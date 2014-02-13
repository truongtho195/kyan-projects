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
    /// Model for table base_PricingChange
    /// </summary>
    [Serializable]
    public partial class base_PricingChangeModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_PricingChangeModel()
        {
            this.IsNew = true;
            this.base_PricingChange = new base_PricingChange();
        }

        // Default constructor that set entity to field
        public base_PricingChangeModel(base_PricingChange base_pricingchange, bool isRaiseProperties = false)
        {
            this.base_PricingChange = base_pricingchange;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_PricingChange base_PricingChange { get; private set; }

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

        protected int _pricingManagerId;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PricingManagerId</param>
        /// </summary>
        public int PricingManagerId
        {
            get { return this._pricingManagerId; }
            set
            {
                if (this._pricingManagerId != value)
                {
                    this.IsDirty = true;
                    this._pricingManagerId = value;
                    OnPropertyChanged(() => PricingManagerId);
                    PropertyChangedCompleted(() => PricingManagerId);
                }
            }
        }

        protected string _pricingManagerResource;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PricingManagerResource</param>
        /// </summary>
        public string PricingManagerResource
        {
            get { return this._pricingManagerResource; }
            set
            {
                if (this._pricingManagerResource != value)
                {
                    this.IsDirty = true;
                    this._pricingManagerResource = value;
                    OnPropertyChanged(() => PricingManagerResource);
                    PropertyChangedCompleted(() => PricingManagerResource);
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

        protected Nullable<decimal> _cost;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the Cost</param>
        /// </summary>
        public Nullable<decimal> Cost
        {
            get { return this._cost; }
            set
            {
                if (this._cost != value)
                {
                    this.IsDirty = true;
                    this._cost = value;
                    OnPropertyChanged(() => Cost);
                    PropertyChangedCompleted(() => Cost);
                }
            }
        }

        protected Nullable<decimal> _currentPrice;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the CurrentPrice</param>
        /// </summary>
        public Nullable<decimal> CurrentPrice
        {
            get { return this._currentPrice; }
            set
            {
                if (this._currentPrice != value)
                {
                    this.IsDirty = true;
                    this._currentPrice = value;
                    OnPropertyChanged(() => CurrentPrice);
                    PropertyChangedCompleted(() => CurrentPrice);
                }
            }
        }

        protected Nullable<decimal> _newPrice;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the NewPrice</param>
        /// </summary>
        public Nullable<decimal> NewPrice
        {
            get { return this._newPrice; }
            set
            {
                if (this._newPrice != value)
                {
                    this.IsDirty = true;
                    this._newPrice = value;
                    OnPropertyChanged(() => NewPrice);
                    PropertyChangedCompleted(() => NewPrice);
                }
            }
        }

        protected Nullable<decimal> _priceChanged;
        /// <summary>
        /// Property Model
        /// <param>Gets or sets the PriceChanged</param>
        /// </summary>
        public Nullable<decimal> PriceChanged
        {
            get { return this._priceChanged; }
            set
            {
                if (this._priceChanged != value)
                {
                    this.IsDirty = true;
                    this._priceChanged = value;
                    OnPropertyChanged(() => PriceChanged);
                    PropertyChangedCompleted(() => PriceChanged);
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
                this.base_PricingChange.Id = this.Id;
            this.base_PricingChange.PricingManagerId = this.PricingManagerId;
            if (this.PricingManagerResource != null)
                this.base_PricingChange.PricingManagerResource = this.PricingManagerResource.Trim();
            this.base_PricingChange.ProductId = this.ProductId;
            if (this.ProductResource != null)
                this.base_PricingChange.ProductResource = this.ProductResource.Trim();
            this.base_PricingChange.Cost = this.Cost;
            this.base_PricingChange.CurrentPrice = this.CurrentPrice;
            this.base_PricingChange.NewPrice = this.NewPrice;
            this.base_PricingChange.PriceChanged = this.PriceChanged;
            this.base_PricingChange.DateCreated = this.DateCreated;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_PricingChange.Id;
            this._pricingManagerId = this.base_PricingChange.PricingManagerId;
            this._pricingManagerResource = this.base_PricingChange.PricingManagerResource;
            this._productId = this.base_PricingChange.ProductId;
            this._productResource = this.base_PricingChange.ProductResource;
            this._cost = this.base_PricingChange.Cost;
            this._currentPrice = this.base_PricingChange.CurrentPrice;
            this._newPrice = this.base_PricingChange.NewPrice;
            this._priceChanged = this.base_PricingChange.PriceChanged;
            this._dateCreated = this.base_PricingChange.DateCreated;
        }

        /// <summary>
        /// Public Method
        /// <param>Method for set Entity to PropertyModel</param>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_PricingChange.Id;
            this.PricingManagerId = this.base_PricingChange.PricingManagerId;
            this.PricingManagerResource = this.base_PricingChange.PricingManagerResource;
            this.ProductId = this.base_PricingChange.ProductId;
            this.ProductResource = this.base_PricingChange.ProductResource;
            this.Cost = this.base_PricingChange.Cost;
            this.CurrentPrice = this.base_PricingChange.CurrentPrice;
            this.NewPrice = this.base_PricingChange.NewPrice;
            this.PriceChanged = this.base_PricingChange.PriceChanged;
            this.DateCreated = this.base_PricingChange.DateCreated;
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
                    case "PricingManagerId":
                        break;
                    case "PricingManagerResource":
                        break;
                    case "ProductId":
                        break;
                    case "ProductResource":
                        break;
                    case "Cost":
                        break;
                    case "CurrentPrice":
                        break;
                    case "NewPrice":
                        break;
                    case "PriceChanged":
                        break;
                    case "DateCreated":
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