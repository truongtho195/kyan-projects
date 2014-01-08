using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    [Serializable]
    public class DepartmentRootModel : ModelBase
    {
        #region Constructor

        // Default constructor
        public DepartmentRootModel()
        {
            this.IsNew = true;
        }

        #endregion

        #region Primitive Properties

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

        #endregion

        #region Navigation Properties

        #region ProductDepartmentCollection

        private CollectionBase<ProductDepartmentModel> _productDepartmentCollection;
        /// <summary>
        /// Gets or sets ProductDepartmentCollection that Contains all ProductDepartmentModels.
        /// </summary>
        public CollectionBase<ProductDepartmentModel> ProductDepartmentCollection
        {
            get
            {
                return _productDepartmentCollection;
            }
            set
            {
                if (_productDepartmentCollection != value)
                {
                    _productDepartmentCollection = value;
                    OnPropertyChanged(() => ProductDepartmentCollection);
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

        #endregion

        #region Property Changed Methods

        protected override void PropertyChangedCompleted(string propertyName)
        {
            if (propertyName == "IsSelected")
            {
                if (_isSelected && !_isExpanded && _productDepartmentCollection != null && _productDepartmentCollection.Any())
                {
                    IsExpanded = true;
                }
            }
        }

        #endregion
    }
}
