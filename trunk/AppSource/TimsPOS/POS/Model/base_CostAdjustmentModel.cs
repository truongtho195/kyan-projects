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
    /// Model for table base_CostAdjustment
    /// </summary>
    [Serializable]
    public partial class base_CostAdjustmentModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_CostAdjustmentModel()
        {
            this.IsNew = true;
            this.base_CostAdjustment = new base_CostAdjustment();
        }

        // Default constructor that set entity to field
        public base_CostAdjustmentModel(base_CostAdjustment base_costadjustment, bool isRaiseProperties = false)
        {
            this.base_CostAdjustment = base_costadjustment;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_CostAdjustment base_CostAdjustment { get; private set; }

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

        protected decimal _newCost;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the NewCost</para>
        /// </summary>
        public decimal NewCost
        {
            get { return this._newCost; }
            set
            {
                if (this._newCost != value)
                {
                    this.IsDirty = true;
                    this._newCost = value;
                    OnPropertyChanged(() => NewCost);
                    PropertyChangedCompleted(() => NewCost);
                }
            }
        }

        protected decimal _oldCost;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the OldCost</para>
        /// </summary>
        public decimal OldCost
        {
            get { return this._oldCost; }
            set
            {
                if (this._oldCost != value)
                {
                    this.IsDirty = true;
                    this._oldCost = value;
                    OnPropertyChanged(() => OldCost);
                    PropertyChangedCompleted(() => OldCost);
                }
            }
        }

        protected Nullable<decimal> _adjustmentNewCost;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AdjustmentNewCost</para>
        /// </summary>
        public Nullable<decimal> AdjustmentNewCost
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

        protected Nullable<decimal> _adjustmentOldCost;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AdjustmentOldCost</para>
        /// </summary>
        public Nullable<decimal> AdjustmentOldCost
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

        protected decimal _adjustCostDifference;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AdjustCostDifference</para>
        /// </summary>
        public decimal AdjustCostDifference
        {
            get { return this._adjustCostDifference; }
            set
            {
                if (this._adjustCostDifference != value)
                {
                    this.IsDirty = true;
                    this._adjustCostDifference = value;
                    OnPropertyChanged(() => AdjustCostDifference);
                    PropertyChangedCompleted(() => AdjustCostDifference);
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

        protected string _userCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UserCreated</para>
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

        protected Nullable<bool> _isReversed;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsReversed</para>
        /// </summary>
        public Nullable<bool> IsReversed
        {
            get { return this._isReversed; }
            set
            {
                if (this._isReversed != value)
                {
                    this.IsDirty = true;
                    this._isReversed = value;
                    OnPropertyChanged(() => IsReversed);
                    PropertyChangedCompleted(() => IsReversed);
                }
            }
        }

        protected Nullable<int> _storeCode;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the StoreCode</para>
        /// </summary>
        public Nullable<int> StoreCode
        {
            get { return this._storeCode; }
            set
            {
                if (this._storeCode != value)
                {
                    this.IsDirty = true;
                    this._storeCode = value;
                    OnPropertyChanged(() => StoreCode);
                    PropertyChangedCompleted(() => StoreCode);
                }
            }
        }

        protected string _resource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Resource</para>
        /// </summary>
        public string Resource
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

        protected short _status;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Status</para>
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

        protected short _reason;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Reason</para>
        /// </summary>
        public short Reason
        {
            get { return this._reason; }
            set
            {
                if (this._reason != value)
                {
                    this.IsDirty = true;
                    this._reason = value;
                    OnPropertyChanged(() => Reason);
                    PropertyChangedCompleted(() => Reason);
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
                this.base_CostAdjustment.Id = this.Id;
            this.base_CostAdjustment.ProductId = this.ProductId;
            this.base_CostAdjustment.ProductResource = this.ProductResource;
            this.base_CostAdjustment.CostDifference = this.CostDifference;
            this.base_CostAdjustment.NewCost = this.NewCost;
            this.base_CostAdjustment.OldCost = this.OldCost;
            this.base_CostAdjustment.AdjustmentNewCost = this.AdjustmentNewCost;
            this.base_CostAdjustment.AdjustmentOldCost = this.AdjustmentOldCost;
            this.base_CostAdjustment.AdjustCostDifference = this.AdjustCostDifference;
            this.base_CostAdjustment.LoggedTime = this.LoggedTime;
            this.base_CostAdjustment.UserCreated = this.UserCreated;
            this.base_CostAdjustment.IsReversed = this.IsReversed;
            this.base_CostAdjustment.StoreCode = this.StoreCode;
            this.base_CostAdjustment.Resource = this.Resource;
            this.base_CostAdjustment.Status = this.Status;
            this.base_CostAdjustment.Reason = this.Reason;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_CostAdjustment.Id;
            this._productId = this.base_CostAdjustment.ProductId;
            this._productResource = this.base_CostAdjustment.ProductResource;
            this._costDifference = this.base_CostAdjustment.CostDifference;
            this._newCost = this.base_CostAdjustment.NewCost;
            this._oldCost = this.base_CostAdjustment.OldCost;
            this._adjustmentNewCost = this.base_CostAdjustment.AdjustmentNewCost;
            this._adjustmentOldCost = this.base_CostAdjustment.AdjustmentOldCost;
            this._adjustCostDifference = this.base_CostAdjustment.AdjustCostDifference;
            this._loggedTime = this.base_CostAdjustment.LoggedTime;
            this._userCreated = this.base_CostAdjustment.UserCreated;
            this._isReversed = this.base_CostAdjustment.IsReversed;
            this._storeCode = this.base_CostAdjustment.StoreCode;
            this._resource = this.base_CostAdjustment.Resource;
            this._status = this.base_CostAdjustment.Status;
            this._reason = this.base_CostAdjustment.Reason;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_CostAdjustment.Id;
            this.ProductId = this.base_CostAdjustment.ProductId;
            this.ProductResource = this.base_CostAdjustment.ProductResource;
            this.CostDifference = this.base_CostAdjustment.CostDifference;
            this.NewCost = this.base_CostAdjustment.NewCost;
            this.OldCost = this.base_CostAdjustment.OldCost;
            this.AdjustmentNewCost = this.base_CostAdjustment.AdjustmentNewCost;
            this.AdjustmentOldCost = this.base_CostAdjustment.AdjustmentOldCost;
            this.AdjustCostDifference = this.base_CostAdjustment.AdjustCostDifference;
            this.LoggedTime = this.base_CostAdjustment.LoggedTime;
            this.UserCreated = this.base_CostAdjustment.UserCreated;
            this.IsReversed = this.base_CostAdjustment.IsReversed;
            this.StoreCode = this.base_CostAdjustment.StoreCode;
            this.Resource = this.base_CostAdjustment.Resource;
            this.Status = this.base_CostAdjustment.Status;
            this.Reason = this.base_CostAdjustment.Reason;
        }

        #endregion

        #region Custom Code

        #region Properties

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

        private string _storeName;
        /// <summary>
        /// Gets or sets the StoreName.
        /// </summary>
        public string StoreName
        {
            get { return _storeName; }
            set
            {
                if (_storeName != value)
                {
                    _storeName = value;
                    OnPropertyChanged(() => StoreName);
                }
            }
        }

        #endregion

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
                    case "ProductId":
                        break;
                    case "ProductResource":
                        break;
                    case "CostDifference":
                        break;
                    case "NewCost":
                        break;
                    case "OldCost":
                        break;
                    case "AdjustmentNewCost":
                        break;
                    case "AdjustmentOldCost":
                        break;
                    case "AdjustCostDifference":
                        break;
                    case "LoggedTime":
                        break;
                    case "Reason":
                        break;
                    case "Status":
                        break;
                    case "UserCreated":
                        break;
                    case "IsReversed":
                        break;
                    case "StoreCode":
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
