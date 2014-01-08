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
    public class ProductCategoryModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public ProductCategoryModel()
        {
            this.IsActived = true;
            this.IsNew = true;
            this.IsDirty = false;
            this.base_Department = new base_Department();
        }

        // Default constructor that set entity to field
        public ProductCategoryModel(base_Department base_department)
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

        protected int _productCategoryID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductCategoryID</para>
        /// </summary>
        public int ProductCategoryID
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

        protected Nullable<int> _productDepartmentID;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ProductDepartmentID</para>
        /// </summary>
        public Nullable<int> ProductDepartmentID
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

        protected string _taxCodeId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the TaxCodeId</para>
        /// </summary>
        public string TaxCodeId
        {
            get
            {
                return this._taxCodeId;
            }
            set
            {
                if (this._taxCodeId != value)
                {
                    this.IsDirty = true;
                    this._taxCodeId = value;
                    OnPropertyChanged(() => TaxCodeId);
                    PropertyChangedCompleted(() => TaxCodeId);
                }
            }
        }

        protected decimal _margin;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Margin</para>
        /// </summary>
        public decimal Margin
        {
            get
            {
                return this._margin;
            }
            set
            {
                if (this._margin != value)
                {
                    this.IsDirty = true;
                    this._margin = value;
                    OnPropertyChanged(() => Margin);
                    PropertyChangedCompleted(() => Margin);
                }
            }
        }

        protected decimal _markUp;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the MarkUp</para>
        /// </summary>
        public decimal MarkUp
        {
            get
            {
                return this._markUp;
            }
            set
            {
                if (this._markUp != value)
                {
                    this.IsDirty = true;
                    this._markUp = value;
                    OnPropertyChanged(() => MarkUp);
                    PropertyChangedCompleted(() => MarkUp);
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

        #region ProductBrandCollection

        private CollectionBase<ProductBrandModel> _productBrandCollection;
        /// <summary>
        /// Gets or sets ProductBrandCollection that Contains all ProductBrandModels.
        /// </summary>
        public CollectionBase<ProductBrandModel> ProductBrandCollection
        {
            get
            {
                return _productBrandCollection;
            }
            set
            {
                if (_productBrandCollection != value)
                {
                    _productBrandCollection = value;
                    OnPropertyChanged(() => ProductBrandCollection);
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

        #region ProductDepartmentModel

        private ProductDepartmentModel _productDepartmentModel;
        /// <summary>
        /// Gets or sets department parent.
        /// </summary>
        public ProductDepartmentModel ProductDepartmentModel
        {
            get
            {
                return _productDepartmentModel;
            }
            set
            {
                if (_productDepartmentModel != value)
                {
                    _isDirty = true;
                    _productDepartmentModel = value;
                    OnPropertyChanged(() => ProductDepartmentModel);
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
                this.base_Department.Id = this.ProductCategoryID;
            this.base_Department.Name = this.Name;
            this.base_Department.ParentId = this.ProductDepartmentID;
            this.base_Department.TaxCodeId = this.TaxCodeId;
            this.base_Department.Margin = this.Margin;
            this.base_Department.MarkUp = this.MarkUp;
            this.base_Department.LevelId = 1;
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
            this._productCategoryID = this.base_Department.Id;
            this._name = this.base_Department.Name;
            this._productDepartmentID = this.base_Department.ParentId;
            this._taxCodeId = this.base_Department.TaxCodeId;
            this._margin = this.base_Department.Margin;
            this._markUp = this.base_Department.MarkUp;
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
            ProductCategoryID = this.base_Department.Id;
            Name = this.base_Department.Name;
            ProductDepartmentID = this.base_Department.ParentId;
            TaxCodeId = this.base_Department.TaxCodeId;
            Margin = this.base_Department.Margin;
            MarkUp = this.base_Department.MarkUp;
            IsActived = this.base_Department.IsActived;
            DateCreated = this.base_Department.DateCreated;
            DateUpdated = this.base_Department.DateUpdated;
            UserCreated = this.base_Department.UserCreated;
            UserUpdated = this.base_Department.UserUpdated;
            IsDirty = false;
        }

        /// <summary>
        /// Creates a shallow copy of the current ProductCategoryModel.
        /// </summary>
        /// <returns>A shallow copy of the ProductCategoryModel</returns>
        public ProductCategoryModel ShallowClone()
        {
            return this.MemberwiseClone() as ProductCategoryModel;
        }

        #endregion

        #region Override Methods

        protected override void PropertyChangedCompleted(string propertyName)
        {
            switch (propertyName)
            {
                case "Margin":

                    //Calculate MarkUp.
                    if (_margin >= 100)
                    {
                        _markUp = 0;
                    }
                    else
                    {
                        // -0.01 because 0.65 round to 0.6, 0.66 round to 0.7, 0.64 round to 0.6.
                        _markUp = Math.Round(Math.Round((_margin * 100) / (100 - _margin), 2) - 0.01M, 1, MidpointRounding.AwayFromZero);
                    }
                    OnPropertyChanged(() => MarkUp);

                    break;

                case "MarkUp":

                    //Calculate Margin.
                    if (_markUp < 0)
                    {
                        _margin = 0;
                    }
                    else
                    {
                        // -0.01 because 0.65 round to 0.6, 0.66 round to 0.7, 0.64 round to 0.6.
                        _margin = Math.Round(Math.Round((_markUp * 100) / (_markUp + 100), 2) - 0.01M, 1, MidpointRounding.AwayFromZero);
                    }
                    OnPropertyChanged(() => Margin);

                    break;

                case "IsSelected":

                    if (_isSelected && !_isExpanded && _productBrandCollection != null && _productBrandCollection.Any())
                    {
                        IsExpanded = true;
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
                string msg = null;
                msg += this["Name"];
                msg += this["TaxCodeId"];
                msg += this["Margin"];
                msg += this["MarkUp"];
                return msg;
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

                    case "TaxCodeId":

                        if (string.IsNullOrWhiteSpace(_taxCodeId))
                        {
                            message = Language.Error5;
                        }

                        break;
                }

                return message;
            }
        }

        #endregion
    }
}
