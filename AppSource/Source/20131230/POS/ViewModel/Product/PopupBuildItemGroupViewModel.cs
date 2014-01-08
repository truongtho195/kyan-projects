using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;
using System.Linq.Expressions;

namespace CPC.POS.ViewModel
{
    class PopupBuildItemGroupViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_UOMRepository _uomRepository = new base_UOMRepository();
        private base_ProductGroupRepository _productGroupRepository = new base_ProductGroupRepository();

        #endregion

        #region Properties

        private string _barcodeProduct;
        /// <summary>
        /// Gets or sets the BarcodeProduct.
        /// </summary>
        public string BarcodeProduct
        {
            get { return _barcodeProduct; }
            set
            {
                if (_barcodeProduct != value)
                {
                    _barcodeProduct = value;
                    OnPropertyChanged(() => BarcodeProduct);
                }
            }
        }

        private base_ProductModel _selectedProduct;
        /// <summary>
        /// Gets or sets the SelectedProduct.
        /// </summary>
        public base_ProductModel SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(() => SelectedProduct);
                }
            }
        }

        private base_ProductModel _selectedSubProduct;
        /// <summary>
        /// Gets or sets the SelectedSubProduct.
        /// </summary>
        public base_ProductModel SelectedSubProduct
        {
            get { return _selectedSubProduct; }
            set
            {
                if (_selectedSubProduct != value)
                {
                    _selectedSubProduct = value;
                    OnPropertyChanged(() => SelectedSubProduct);
                    OnSelectedSubProductChanged();
                }
            }
        }

        private DataSearchCollection _productFieldCollection;
        /// <summary>
        /// Gets or sets the ProductFieldCollection.
        /// </summary>
        public DataSearchCollection ProductFieldCollection
        {
            get { return _productFieldCollection; }
            set
            {
                if (_productFieldCollection != value)
                {
                    _productFieldCollection = value;
                    OnPropertyChanged(() => ProductFieldCollection);
                }
            }
        }

        private CollectionBase<base_ProductModel> _productCollection;
        /// <summary>
        /// Gets or sets the ProductCollection.
        /// </summary>
        public CollectionBase<base_ProductModel> ProductCollection
        {
            get { return _productCollection; }
            set
            {
                if (_productCollection != value)
                {
                    _productCollection = value;
                    OnPropertyChanged(() => ProductCollection);
                }
            }
        }

        /// <summary>
        /// Gets or sets the UOMList
        /// </summary>
        public List<CheckBoxItemModel> UOMList { get; set; }

        private ObservableCollection<base_GuestModel> _vendorCollection;
        /// <summary>
        /// Gets or sets the VendorCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorCollection
        {
            get { return _vendorCollection; }
            set
            {
                if (_vendorCollection != value)
                {
                    _vendorCollection = value;
                    OnPropertyChanged(() => VendorCollection);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Total
        /// </summary>
        public decimal Total { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupBuildItemGroupViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            InitialCommand();

            // Initial collection for search products
            ProductFieldCollection = new DataSearchCollection();
            ProductFieldCollection.Add(new DataSearchModel { ID = 1, Level = 0, DisplayName = "Code", KeyName = "Code" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 2, Level = 0, DisplayName = "Barcode", KeyName = "Barcode" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 3, Level = 0, DisplayName = "Product Name", KeyName = "ProductName" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 4, Level = 0, DisplayName = "Attribute", KeyName = "Attribute" });
            ProductFieldCollection.Add(new DataSearchModel { ID = 6, Level = 0, DisplayName = "Size", KeyName = "Size" });
        }

        public PopupBuildItemGroupViewModel(base_ProductModel productModel, ObservableCollection<base_GuestModel> vendorCollection)
            : this()
        {
            VendorCollection = vendorCollection;
            LoadStaticData(productModel.Id);

            // Create new product model
            SelectedProduct = new base_ProductModel();

            SelectedProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

            SelectedProduct.Id = productModel.Id;
            SelectedProduct.Resource = productModel.Resource;
            SelectedProduct.CategoryName = productModel.CategoryName;
            SelectedProduct.ProductName = productModel.ProductName;
            SelectedProduct.Attribute = productModel.Attribute;
            SelectedProduct.Size = productModel.Size;
            SelectedProduct.RegularPrice = productModel.RegularPrice;
            SelectedProduct.AverageUnitCost = productModel.AverageUnitCost;
            SelectedProduct.OnHandStore = productModel.OnHandStore;
            SelectedProduct.CompanyReOrderPoint = productModel.CompanyReOrderPoint;
            Total = productModel.RegularPrice;

            // Load product group collection
            if (SelectedProduct.ProductGroupCollection == null)
            {
                // Initial product group collection
                SelectedProduct.ProductGroupCollection = new CollectionBase<base_ProductGroupModel>();

                foreach (base_ProductGroupModel productGroupItem in productModel.ProductGroupCollection)
                {
                    // Clone product groups
                    base_ProductGroupModel productGroupModel = productGroupItem.Clone();

                    // Register property changed event
                    productGroupModel.PropertyChanged += new PropertyChangedEventHandler(productGroupModel_PropertyChanged);

                    // Add product group model to collection
                    SelectedProduct.ProductGroupCollection.Add(productGroupModel);

                    //// Get product item exist in product group collection
                    //base_ProductModel productItem = ProductCollection.SingleOrDefault(x => x.Id.Equals(productGroupItem.ProductId));

                    //// Remove product item exist in product group collection
                    //ProductCollection.Remove(productItem);

                    // Turn off IsDirty & IsNew
                    productGroupModel.EndUpdate();
                }
            }

            // Turn off IsDirty & IsNew
            SelectedProduct.EndUpdate();
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
            if (SelectedProduct == null)
                return false;
            return SelectedProduct.IsDirty || SelectedProduct.ProductGroupCollection.IsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            // Removes all leading and trailing white-space characters
            SelectedProduct.ProductName = SelectedProduct.ProductName.Trim();
            SelectedProduct.Attribute = SelectedProduct.Attribute.Trim();
            SelectedProduct.Size = SelectedProduct.Size.Trim();

            if (Total != SelectedProduct.RegularPrice &&
                (SelectedProduct.ProductGroupCollection.IsDirty || SelectedProduct.IsChecked))
            {
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Would you like to update the price to equal the sum of the new component prices?", "POS", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.OK))
                {
                    SelectedProduct.RegularPrice = Total;
                }
            }

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

        #region DeleteCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                base_ProductGroupModel productGroupModel = param as base_ProductGroupModel;
                //productGroupModel.IsChecked = true;

                //if (ProductCollection.DeletedItems != null)
                //{
                //    base_ProductModel productModel = ProductCollection.DeletedItems.SingleOrDefault(x => x.Id.Equals(productGroupModel.ProductId));

                //    // Add product item to collection
                //    ProductCollection.Add(productModel);

                //    // Remove product item in DeletedItems collection
                //    ProductCollection.DeletedItems.Remove(productModel);
                //}

                // Turn off IsNew to push item into DeletedItems collecion
                productGroupModel.IsNew = false;

                SelectedProduct.ProductGroupCollection.Remove(productGroupModel);

                // Update regular price
                UpdateRegularPrice(SelectedProduct);
            }
        }

        #endregion

        #region SearchProductAdvanceCommand

        /// <summary>
        /// Gets the SearchProductAdvanceCommand command.
        /// </summary>
        public ICommand SearchProductAdvanceCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SearchProductAdvanceCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchProductAdvanceCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchProductAdvanceCommand command is executed.
        /// </summary>
        private void OnSearchProductAdvanceCommandExecute()
        {
            ProductSearchViewModel viewModel = new ProductSearchViewModel(false, false, false, false, false, false);
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, viewModel, "Search Product");
            if (dialogResult == true)
            {
                SelectedSubProduct = viewModel.SelectedProducts.FirstOrDefault();
            }
        }

        #endregion

        #region SearchProductCommand

        /// <summary>
        /// Gets the SearchProductCommand command.
        /// </summary>
        public ICommand SearchProductCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SearchProductCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchProductCommandCanExecute()
        {
            return !string.IsNullOrWhiteSpace(BarcodeProduct);
        }

        /// <summary>
        /// Method to invoke when the SearchProductCommand command is executed.
        /// </summary>
        private void OnSearchProductCommandExecute()
        {
            long parentID = SelectedProduct.Id;
            short itemTypeID = (short)ItemTypes.Group;

            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false && !x.Id.Equals(parentID) && x.ItemTypeId != itemTypeID);

            // Get product have the same barcode
            predicate = predicate.And(x => x.Barcode!=null && x.Barcode.Equals(BarcodeProduct));

            // Get Product
            base_Product product = _productRepository.Get(predicate);

            if (product != null)
            {
                // Create new product model
                base_ProductModel productModel = new base_ProductModel(product);

                // Get vendor name for product
                if (string.IsNullOrWhiteSpace(productModel.VendorName))
                {
                    base_GuestModel vendorItem = VendorCollection.FirstOrDefault(x => x.Id.Equals(productModel.VendorId));
                    if (vendorItem != null)
                        productModel.VendorName = vendorItem.Company;
                }

                // Get uom name for product
                if (string.IsNullOrWhiteSpace(productModel.UOMName))
                {
                    CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productModel.BaseUOMId));
                    if (uomItem != null)
                        productModel.UOMName = uomItem.Text;
                }

                SelectedSubProduct = productModel;
            }

            // Clear barcode
            BarcodeProduct = string.Empty;
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
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchProductAdvanceCommand = new RelayCommand(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
            SearchProductCommand = new RelayCommand(OnSearchProductCommandExecute, OnSearchProductCommandCanExecute);
        }

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData(long parentID)
        {
            // Load UOM list
            if (UOMList == null)
            {
                UOMList = new List<CheckBoxItemModel>(_uomRepository.GetIQueryable(x => x.IsActived).
                        OrderBy(x => x.Name).Select(x => new CheckBoxItemModel { Value = x.Id, Text = x.Name }));
            }

            //// Initial product collection
            //ProductCollection = new CollectionBase<base_ProductModel>();

            //short itemTypeID = (short)ItemTypes.Group;

            //// Get all product
            //IEnumerable<base_Product> products = _productRepository.
            //    GetAll(x => x.IsPurge == false && !x.Id.Equals(parentID) && x.ItemTypeId != itemTypeID).OrderBy(x => x.Id);
            //foreach (base_Product product in products)
            //{
            //    // Create new product model
            //    base_ProductModel productModel = new base_ProductModel(product);

            //    // Get vendor name for product
            //    if (string.IsNullOrWhiteSpace(productModel.VendorName))
            //    {
            //        base_GuestModel vendorItem = VendorCollection.FirstOrDefault(x => x.Id.Equals(productModel.VendorId));
            //        if (vendorItem != null)
            //            productModel.VendorName = vendorItem.Company;
            //    }

            //    // Get uom name for product
            //    if (string.IsNullOrWhiteSpace(productModel.UOMName))
            //    {
            //        CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productModel.BaseUOMId));
            //        if (uomItem != null)
            //            productModel.UOMName = uomItem.Text;
            //    }

            //    // Add new product to collection
            //    ProductCollection.Add(productModel);
            //}
        }

        /// <summary>
        /// Process when selected sub product changed
        /// </summary>
        private void OnSelectedSubProductChanged()
        {
            if (SelectedSubProduct != null)
            {
                if (SelectedSubProduct.base_Product.RegularPrice == 0)
                {
                    UpdateTransactionViewModel viewModel = new UpdateTransactionViewModel(SelectedSubProduct);
                    bool? result = _dialogService.ShowDialog<UpdateTransactionView>(_ownerViewModel, viewModel, "Update Product Price");
                    if (result.HasValue && result.Value)
                    {
                        // Update new regular price for product
                        SelectedSubProduct.RegularPrice = viewModel.NewPrice;

                        // Update new regular price to database
                        if (viewModel.IsUpdateProductPrice)
                        {
                            UpdateRegularPriceProductGroup(SelectedSubProduct);

                            // Map data from model to entity
                            SelectedSubProduct.base_Product.RegularPrice = SelectedSubProduct.RegularPrice;

                            // Accept changes
                            _productRepository.Commit();
                        }

                        // Turn off IsDirty
                        SelectedSubProduct.IsDirty = false;
                    }
                }

                // Create new product group model
                base_ProductGroupModel productGroupModel = new base_ProductGroupModel();

                // Add new product group to collection
                SelectedProduct.ProductGroupCollection.Add(productGroupModel);

                // Register property changed event
                productGroupModel.PropertyChanged += new PropertyChangedEventHandler(productGroupModel_PropertyChanged);

                productGroupModel.ProductParentId = SelectedProduct.Id;
                productGroupModel.ProductId = SelectedSubProduct.Id;
                productGroupModel.ProductResource = SelectedSubProduct.Resource.ToString();
                productGroupModel.ItemCode = SelectedSubProduct.Code;
                productGroupModel.ItemName = SelectedSubProduct.ProductName;
                productGroupModel.ItemAttribute = SelectedSubProduct.Attribute;
                productGroupModel.ItemSize = SelectedSubProduct.Size;
                productGroupModel.RegularPrice = SelectedSubProduct.RegularPrice;
                productGroupModel.UOMId = SelectedSubProduct.BaseUOMId;
                productGroupModel.UOM = SelectedSubProduct.UOMName;
                //productGroupModel.OnHandQty = SelectedSubProduct.GetOnHandFromStore(Define.StoreCode);
                productGroupModel.Quantity = 1;
                productGroupModel.Resource = Guid.NewGuid();

                if (productGroupModel.ProductUOMCollection == null)
                {
                    // Initial product UOM collection
                    productGroupModel.ProductUOMCollection = new ObservableCollection<base_ProductUOMModel>();
                    productGroupModel.ProductUOMCollection.Add(new base_ProductUOMModel
                    {
                        Name = productGroupModel.UOM,
                        UOMId = productGroupModel.UOMId,
                        RegularPrice = productGroupModel.RegularPrice,
                        QuantityOnHand = productGroupModel.OnHandQty
                    });

                    // Get product store by store
                    base_ProductStore productStore = SelectedSubProduct.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));

                    if (productStore != null)
                    {
                        productGroupModel.OnHandQty = productStore.QuantityOnHand;

                        foreach (base_ProductUOM productUOM in productStore.base_ProductUOM)
                        {
                            // Create new product uom model
                            base_ProductUOMModel productUOMModel = new base_ProductUOMModel(productUOM);

                            // Get uom name for product
                            if (string.IsNullOrWhiteSpace(productUOMModel.Name))
                            {
                                CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productUOMModel.UOMId));
                                if (uomItem != null)
                                    productUOMModel.Name = uomItem.Text;
                            }

                            // Add new product uom to collection
                            productGroupModel.ProductUOMCollection.Add(productUOMModel);
                        }
                    }
                }

                //// Remove product item exist in product group collection
                //ProductCollection.Remove(SelectedSubProduct);

                _selectedSubProduct = null;
            }
        }

        /// <summary>
        /// Update amount
        /// </summary>
        /// <param name="productGroupModel"></param>
        private void UpdateAmount(base_ProductGroupModel productGroupModel)
        {
            productGroupModel.Amount = productGroupModel.RegularPrice * productGroupModel.Quantity;
        }

        /// <summary>
        /// Update regular price
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateRegularPrice(base_ProductModel productModel)
        {
            Total = productModel.ProductGroupCollection.Sum(x => x.Amount);
        }

        /// <summary>
        /// Update regular price for product group
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateRegularPriceProductGroup(base_ProductModel productModel)
        {
            if (Define.CONFIGURATION.IsAUPPG)
            {
                // Get all product group that same id
                IEnumerable<base_ProductGroup> productGroups = _productGroupRepository.GetAll(x => x.base_Product.Id.Equals(productModel.Id));

                foreach (base_ProductGroup productGroup in productGroups)
                {
                    // Update new regular price for product group by base UOM
                    if (productGroup.UOMId.Equals(productModel.BaseUOMId) &&
                        productModel.RegularPrice != productModel.base_Product.RegularPrice)
                    {
                        productGroup.RegularPrice = productModel.RegularPrice;
                    }
                    else if (productModel.ProductUOMCollection != null)
                    {
                        base_ProductUOMModel productUOMModel = productModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId.Equals(productGroup.UOMId));
                        if (productUOMModel != null && productUOMModel.RegularPrice != productUOMModel.base_ProductUOM.RegularPrice)
                            productGroup.RegularPrice = productUOMModel.RegularPrice;
                    }

                    // Update product group amount
                    productGroup.Amount = productGroup.RegularPrice * productGroup.Quantity;
                }
            }
        }

        #endregion

        #region Event Methods

        private void productGroupModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Get product group model
            base_ProductGroupModel productGroupModel = sender as base_ProductGroupModel;

            switch (e.PropertyName)
            {
                case "Quantity":
                    UpdateAmount(productGroupModel);
                    UpdateRegularPrice(SelectedProduct);
                    break;
                case "UOMId":
                    if (productGroupModel.ProductUOMCollection != null)
                    {
                        // Get product uom model
                        base_ProductUOMModel productUOMModel = productGroupModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId.Equals(productGroupModel.UOMId));

                        if (productUOMModel != null)
                        {
                            productGroupModel.OnHandQty = productUOMModel.QuantityOnHand;
                            productGroupModel.RegularPrice = productUOMModel.RegularPrice;
                            productGroupModel.UOM = productUOMModel.Name;
                        }
                        else
                        {
                            productGroupModel.OnHandQty = 0;
                            productGroupModel.RegularPrice = 0;
                        }

                        UpdateAmount(productGroupModel);
                        UpdateRegularPrice(SelectedProduct);
                    }
                    break;
            }
        }

        private void SelectedProduct_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Get product group model
            base_ProductModel productModel = sender as base_ProductModel;

            switch (e.PropertyName)
            {
                case "RegularPrice":
                    productModel.IsChecked = true;
                    break;
            }
        }

        #endregion
    }
}