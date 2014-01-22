using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupReorderPointViewModel : ViewModelBase
    {
        #region Defines

        private base_StoreRepository _storeRepository = new base_StoreRepository();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

        private CollectionBase<base_ProductStoreModel> _productStoreCollection;
        /// <summary>
        /// Gets or sets the ProductStoreCollection.
        /// </summary>
        public CollectionBase<base_ProductStoreModel> ProductStoreCollection
        {
            get { return _productStoreCollection; }
            set
            {
                if (_productStoreCollection != value)
                {
                    _productStoreCollection = value;
                    OnPropertyChanged(() => ProductStoreCollection);
                }
            }
        }

        private decimal _companyReorderPoint;
        /// <summary>
        /// Gets or sets the CompanyReorderPoint.
        /// </summary>
        public decimal CompanyReorderPoint
        {
            get { return _companyReorderPoint; }
            set
            {
                if (_companyReorderPoint != value)
                {
                    this.IsDirty = true;
                    _companyReorderPoint = value;
                    OnPropertyChanged(() => CompanyReorderPoint);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupReorderPointViewModel()
        {
            InitialCommand();
        }

        public PopupReorderPointViewModel(base_ProductModel productModel)
            : this()
        {
            // Update company reorder point
            _companyReorderPoint = productModel.CompanyReOrderPoint;
            OnPropertyChanged(() => CompanyReorderPoint);

            // Load product store collection
            LoadProductStoreCollection(productModel);
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            if (ProductStoreCollection == null)
                return false;

            // Initial variable to check IsDirty of product store collection
            bool productStoreCollectionIsDirty = ProductStoreCollection.IsDirty; ;

            if (ProductStoreCollection.Any(x => x.IsNew))
            {
                // Check IsDirty of product store collection
                productStoreCollectionIsDirty = ProductStoreCollection.Count(x => x.IsNew && x.IsDirty) > 0;
            }

            return IsDirty || productStoreCollectionIsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Load on hand store list
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadProductStoreCollection(base_ProductModel productModel)
        {
            try
            {
                // Initial product store collection
                ProductStoreCollection = new CollectionBase<base_ProductStoreModel>();

                for (short storeCode = 0; storeCode < _storeRepository.GetIQueryable().Count(); storeCode++)
                {
                    // Create new product store model
                    base_ProductStoreModel productStoreModel = new base_ProductStoreModel { StoreCode = storeCode };
                    productStoreModel.Resource = Guid.NewGuid().ToString();
                    productStoreModel.ProductResource = productModel.Resource.ToString();

                    // Get product store by store code
                    base_ProductStoreModel productStoreItem = productModel.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(storeCode));

                    if (productStoreItem != null)
                    {
                        // Update reorder point
                        productStoreModel.ReorderPoint = productStoreItem.ReorderPoint;
                    }

                    // Register property changed event
                    productStoreModel.PropertyChanged += new PropertyChangedEventHandler(productStoreItem_PropertyChanged);

                    // Add product store to list
                    ProductStoreCollection.Add(productStoreModel);

                    // Turn off IsDirty
                    productStoreModel.IsDirty = false;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region Event Methods

        private void productStoreItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ProductStoreModel productStoreModel = sender as base_ProductStoreModel;

            if (e.PropertyName.Equals("ReorderPoint"))
            {
                // Update company quantities
                OnPropertyChanged(() => CompanyReorderPoint);
            }
        }

        #endregion
    }
}