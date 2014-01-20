using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.ComponentModel;
using CPC.POS.Database;
using CPC.Helper;

namespace CPC.POS.Model
{
    [Serializable]
    public class ProductBrandModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public ProductBrandModel()
        {
            this.IsActived = true;
            this.IsNew = true;
            this.IsDirty = false;
            this.base_Department = new base_Department();
        }

        // Default constructor that set entity to field
        public ProductBrandModel(base_Department base_department)
        {
            this.base_Department = base_department;
            this.ToModel();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_Department base_Department
        {
            get;
            private set;
        }

        #endregion

        #region Primitive Properties

        protected int _productBrandID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductBrandID</para>
        /// </summary>
        public int ProductBrandID
        {
            get
            {
                return this._productBrandID;
            }
            set
            {
                if (this._productBrandID != value)
                {
                    this.IsDirty = true;
                    this._productBrandID = value;
                    OnPropertyChanged(() => ProductBrandID);
                    PropertyChangedCompleted(() => ProductBrandID);
                }
            }
        }

        protected Nullable<int> _productCategoryID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductCategoryID</para>
        /// </summary>
        public Nullable<int> ProductCategoryID
        {
            get
            {
                return this._productCategoryID;
            }
            set
            {
                if (this._productCategoryID != value)
                {
                    this.IsDirty = true;
                    this._productCategoryID = value;
                    OnPropertyChanged(() => ProductCategoryID);
                    PropertyChangedCompleted(() => ProductCategoryID);
                }
            }
        }

        protected string _name;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Name</para>
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (this._name != value)
                {
                    this.IsDirty = true;
                    this._name = value;
                    OnPropertyChanged(() => Name);
                    PropertyChangedCompleted(() => Name);
                }
            }
        }

        protected Nullable<bool> _isActived;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsActived</para>
        /// </summary>
        public Nullable<bool> IsActived
        {
            get
            {
                return this._isActived;
            }
            set
            {
                if (this._isActived != value)
                {
                    this.IsDirty = true;
                    this._isActived = value;
                    OnPropertyChanged(() => IsActived);
                    PropertyChangedCompleted(() => IsActived);
                }
            }
        }

        protected Nullable<System.DateTime> _dateCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateCreated</para>
        /// </summary>
        public Nullable<System.DateTime> DateCreated
        {
            get
            {
                return this._dateCreated;
            }
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

        protected Nullable<System.DateTime> _dateUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateUpdated</para>
        /// </summary>
        public Nullable<System.DateTime> DateUpdated
        {
            get
            {
                return this._dateUpdated;
            }
            set
            {
                if (this._dateUpdated != value)
                {
                    this.IsDirty = true;
                    this._dateUpdated = value;
                    OnPropertyChanged(() => DateUpdated);
                    PropertyChangedCompleted(() => DateUpdated);
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
            get
            {
                return this._userCreated;
            }
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

        protected string _userUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UserUpdated</para>
        /// </summary>
        public string UserUpdated
        {
            get
            {
                return this._userUpdated;
            }
            set
            {
                if (this._userUpdated != value)
                {
                    this.IsDirty = true;
                    this._userUpdated = value;
                    OnPropertyChanged(() => UserUpdated);
                    PropertyChangedCompleted(() => UserUpdated);
                }
            }
        }

        #endregion

        #region Navigation Properties

        #region ProductCollection

        private CollectionBase<base_ProductModel> _productCollection;
        /// <summary>
        /// Gets or sets ProductCollection that Contains all base_ProductModels.
        /// </summary>
        public CollectionBase<base_ProductModel> ProductCollection
        {
            get
            {
                return _productCollection;
            }
            set
            {
                if (_productCollection != value)
                {
                    _productCollection = value;
                    OnPropertyChanged(() => ProductCollection);
                }
            }
        }

        #endregion

        #region ProductCategoryModel

        private ProductCategoryModel _productCategoryModel;
        /// <summary>
        /// Gets or sets category parent.
        /// </summary>
        public ProductCategoryModel ProductCategoryModel
        {
            get
            {
                return _productCategoryModel;
            }
            set
            {
                if (_productCategoryModel != value)
                {
                    _isDirty = true;
                    _productCategoryModel = value;
                    OnPropertyChanged(() => ProductCategoryModel);
                }
            }
        }

        #endregion

        #endregion

        #region Properties

        #region IsProductsLoaded

        /// <summary>
        /// Determine whether ProductCollection was loaded.
        /// </summary>
        public bool IsProductsLoaded
        {
            get
            {
                return _productCollection != null;
            }
        }

        #endregion

        #region ProductTotal

        private int _productTotal;
        /// <summary>
        /// Gets or sets product total.
        /// </summary>
        public int ProductTotal
        {
            get
            {
                return _productTotal;
            }
            set
            {
                if (_productTotal != value)
                {
                    _isDirty = true;
                    _productTotal = value;
                    OnPropertyChanged(() => ProductTotal);
                }
            }
        }

        #endregion

        #region IsExpanded

        private bool _isExpanded;
        /// <summary>
        /// Gets or sets whether the child items in this object are expanded or collapsed. 
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(() => IsExpanded);
                }
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
                this.base_Department.Id = this.ProductBrandID;
            this.base_Department.Name = this.Name;
            this.base_Department.ParentId = this.ProductCategoryID;
            this.base_Department.TaxCodeId = null;
            this.base_Department.Margin = 0;
            this.base_Department.MarkUp = 0;
            this.base_Department.LevelId = 2;
            this.base_Department.IsActived = this.IsActived;
            this.base_Department.DateCreated = this.DateCreated;
            this.base_Department.DateUpdated = this.DateUpdated;
            this.base_Department.UserCreated = this.UserCreated;
            this.base_Department.UserUpdated = this.UserUpdated;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._productBrandID = this.base_Department.Id;
            this._name = this.base_Department.Name;
            this._productCategoryID = this.base_Department.ParentId;
            this._isActived = this.base_Department.IsActived;
            this._dateCreated = this.base_Department.DateCreated;
            this._dateUpdated = this.base_Department.DateUpdated;
            this._userCreated = this.base_Department.UserCreated;
            this._userUpdated = this.base_Department.UserUpdated;
        }

        /// <summary>
        /// Creates a shallow copy of the current ProductBrandModel.
        /// </summary>
        /// <returns>A shallow copy of the ProductBrandModel</returns>
        public ProductBrandModel ShallowClone()
        {
            return this.MemberwiseClone() as ProductBrandModel;
        }

        #endregion

        #region Custom Code


        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                return this["Name"];
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = null;

                switch (columnName)
                {
                    case "Name":

                        if (string.IsNullOrWhiteSpace(_name))
                        {
                            message = Language.Error4;
                        }

                        break;
                }

                return message;
            }
        }

        #endregion
    }
}
