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
    public class ProductDepartmentModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public ProductDepartmentModel()
        {
            this.IsActived = true;
            this.IsNew = true;
            this.IsDirty = false;
            this.base_Department = new base_Department();
        }

        // Default constructor that set entity to field
        public ProductDepartmentModel(base_Department base_department)
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

        protected int _productDepartmentID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductDepartmentID</para>
        /// </summary>
        public int ProductDepartmentID
        {
            get
            {
                return this._productDepartmentID;
            }
            set
            {
                if (this._productDepartmentID != value)
                {
                    this.IsDirty = true;
                    this._productDepartmentID = value;
                    OnPropertyChanged(() => ProductDepartmentID);
                    PropertyChangedCompleted(() => ProductDepartmentID);
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

        #region ProductCategoryCollection

        private CollectionBase<ProductCategoryModel> _productCategoryCollection;
        /// <summary>
        /// Gets or sets ProductCategoryCollection that Contains all ProductCategoryModels.
        /// </summary>
        public CollectionBase<ProductCategoryModel> ProductCategoryCollection
        {
            get
            {
                return _productCategoryCollection;
            }
            set
            {
                if (_productCategoryCollection != value)
                {
                    _productCategoryCollection = value;
                    OnPropertyChanged(() => ProductCategoryCollection);
                }
            }
        }

        #endregion

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
                    PropertyChangedCompleted(() => IsSelected);
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
                this.base_Department.Id = this.ProductDepartmentID;
            this.base_Department.Name = this.Name;
            this.base_Department.ParentId = null;
            this.base_Department.TaxCodeId = null;
            this.base_Department.Margin = 0;
            this.base_Department.MarkUp = 0;
            this.base_Department.LevelId = 0;
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
            this._productDepartmentID = this.base_Department.Id;
            this._name = this.base_Department.Name;
            this._isActived = this.base_Department.IsActived;
            this._dateCreated = this.base_Department.DateCreated;
            this._dateUpdated = this.base_Department.DateUpdated;
            this._userCreated = this.base_Department.UserCreated;
            this._userUpdated = this.base_Department.UserUpdated;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void Restore()
        {
            ProductDepartmentID = this.base_Department.Id;
            Name = this.base_Department.Name;
            IsActived = this.base_Department.IsActived;
            DateCreated = this.base_Department.DateCreated;
            DateUpdated = this.base_Department.DateUpdated;
            UserCreated = this.base_Department.UserCreated;
            UserUpdated = this.base_Department.UserUpdated;
            IsDirty = false;
        }

        /// <summary>
        /// Creates a shallow copy of the current ProductDepartmentModel.
        /// </summary>
        /// <returns>A shallow copy of the ProductDepartmentModel</returns>
        public ProductDepartmentModel ShallowClone()
        {
            return this.MemberwiseClone() as ProductDepartmentModel;
        }

        #endregion

        #region Property Changed Methods

        protected override void PropertyChangedCompleted(string propertyName)
        {
            if (propertyName == "IsSelected")
            {
                if (_isSelected && !_isExpanded && _productCategoryCollection != null && _productCategoryCollection.Any())
                {
                    IsExpanded = true;
                }
            }
        }

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
