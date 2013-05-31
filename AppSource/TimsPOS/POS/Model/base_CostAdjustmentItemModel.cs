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
    /// Model for table base_CostAdjustmentItem
    /// </summary>
    [Serializable]
    public partial class base_CostAdjustmentItemModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_CostAdjustmentItemModel()
        {
            this.IsNew = true;
            this.base_CostAdjustmentItem = new base_CostAdjustmentItem();
        }

        // Default constructor that set entity to field
        public base_CostAdjustmentItemModel(base_CostAdjustmentItem base_costadjustmentitem, bool isRaiseProperties = false)
        {
            this.base_CostAdjustmentItem = base_costadjustmentitem;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_CostAdjustmentItem base_CostAdjustmentItem { get; private set; }

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

        protected System.Guid _resource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Resource</para>
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

        protected long _productId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductId</para>
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

        protected string _productCode;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductCode</para>
        /// </summary>
        public string ProductCode
        {
            get { return this._productCode; }
            set
            {
                if (this._productCode != value)
                {
                    this.IsDirty = true;
                    this._productCode = value;
                    OnPropertyChanged(() => ProductCode);
                    PropertyChangedCompleted(() => ProductCode);
                }
            }
        }

        protected decimal _costDifference;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the CostDifference</para>
        /// </summary>
        public decimal CostDifference
        {
            get { return this._costDifference; }
            set
            {
                if (this._costDifference != value)
                {
                    this.IsDirty = true;
                    this._costDifference = value;
                    OnPropertyChanged(() => CostDifference);
                    PropertyChangedCompleted(() => CostDifference);
                }
            }
        }

        protected decimal _adjustmentNewCost;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AdjustmentNewCost</para>
        /// </summary>
        public decimal AdjustmentNewCost
        {
            get { return this._adjustmentNewCost; }
            set
            {
                if (this._adjustmentNewCost != value)
                {
                    this.IsDirty = true;
                    this._adjustmentNewCost = value;
                    OnPropertyChanged(() => AdjustmentNewCost);
                    PropertyChangedCompleted(() => AdjustmentNewCost);
                }
            }
        }

        protected decimal _adjustmentOldCost;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AdjustmentOldCost</para>
        /// </summary>
        public decimal AdjustmentOldCost
        {
            get { return this._adjustmentOldCost; }
            set
            {
                if (this._adjustmentOldCost != value)
                {
                    this.IsDirty = true;
                    this._adjustmentOldCost = value;
                    OnPropertyChanged(() => AdjustmentOldCost);
                    PropertyChangedCompleted(() => AdjustmentOldCost);
                }
            }
        }

        protected System.DateTime _loggedTime;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the LoggedTime</para>
        /// </summary>
        public System.DateTime LoggedTime
        {
            get { return this._loggedTime; }
            set
            {
                if (this._loggedTime != value)
                {
                    this.IsDirty = true;
                    this._loggedTime = value;
                    OnPropertyChanged(() => LoggedTime);
                    PropertyChangedCompleted(() => LoggedTime);
                }
            }
        }

        protected string _parentResource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ParentResource</para>
        /// </summary>
        public string ParentResource
        {
            get { return this._parentResource; }
            set
            {
                if (this._parentResource != value)
                {
                    this.IsDirty = true;
                    this._parentResource = value;
                    OnPropertyChanged(() => ParentResource);
                    PropertyChangedCompleted(() => ParentResource);
                }
            }
        }

        protected Nullable<bool> _isQuantityChanged;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsQuantityChanged</para>
        /// </summary>
        public Nullable<bool> IsQuantityChanged
        {
            get { return this._isQuantityChanged; }
            set
            {
                if (this._isQuantityChanged != value)
                {
                    this.IsDirty = true;
                    this._isQuantityChanged = value;
                    OnPropertyChanged(() => IsQuantityChanged);
                    PropertyChangedCompleted(() => IsQuantityChanged);
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
                this.base_CostAdjustmentItem.Id = this.Id;
            this.base_CostAdjustmentItem.Resource = this.Resource;
            this.base_CostAdjustmentItem.ProductId = this.ProductId;
            this.base_CostAdjustmentItem.ProductCode = this.ProductCode;
            this.base_CostAdjustmentItem.CostDifference = this.CostDifference;
            this.base_CostAdjustmentItem.AdjustmentNewCost = this.AdjustmentNewCost;
            this.base_CostAdjustmentItem.AdjustmentOldCost = this.AdjustmentOldCost;
            this.base_CostAdjustmentItem.LoggedTime = this.LoggedTime;
            this.base_CostAdjustmentItem.ParentResource = this.ParentResource;
            this.base_CostAdjustmentItem.IsQuantityChanged = this.IsQuantityChanged;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_CostAdjustmentItem.Id;
            this._resource = this.base_CostAdjustmentItem.Resource;
            this._productId = this.base_CostAdjustmentItem.ProductId;
            this._productCode = this.base_CostAdjustmentItem.ProductCode;
            this._costDifference = this.base_CostAdjustmentItem.CostDifference;
            this._adjustmentNewCost = this.base_CostAdjustmentItem.AdjustmentNewCost;
            this._adjustmentOldCost = this.base_CostAdjustmentItem.AdjustmentOldCost;
            this._loggedTime = this.base_CostAdjustmentItem.LoggedTime;
            this._parentResource = this.base_CostAdjustmentItem.ParentResource;
            this._isQuantityChanged = this.base_CostAdjustmentItem.IsQuantityChanged;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_CostAdjustmentItem.Id;
            this.Resource = this.base_CostAdjustmentItem.Resource;
            this.ProductId = this.base_CostAdjustmentItem.ProductId;
            this.ProductCode = this.base_CostAdjustmentItem.ProductCode;
            this.CostDifference = this.base_CostAdjustmentItem.CostDifference;
            this.AdjustmentNewCost = this.base_CostAdjustmentItem.AdjustmentNewCost;
            this.AdjustmentOldCost = this.base_CostAdjustmentItem.AdjustmentOldCost;
            this.LoggedTime = this.base_CostAdjustmentItem.LoggedTime;
            this.ParentResource = this.base_CostAdjustmentItem.ParentResource;
            this.IsQuantityChanged = this.base_CostAdjustmentItem.IsQuantityChanged;
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
                    case "Resource":
                        break;
                    case "ParentResource":
                        break;
                    case "ProductId":
                        break;
                    case "ProductCode":
                        break;
                    case "CostDifference":
                        break;
                    case "AdjustmentNewCost":
                        break;
                    case "AdjustmentOldCost":
                        break;
                    case "LoggedTime":
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
