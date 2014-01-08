using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class PopupDuplicateItemViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();
        private base_VendorProductRepository _vendorProductRepository = new base_VendorProductRepository();

        private ICollectionView _categoryCollectionView;

        #endregion

        #region Properties

        private base_ProductModel _duplicateProduct;
        /// <summary>
        /// Gets or sets the DuplicateProduct.
        /// </summary>
        public base_ProductModel DuplicateProduct
        {
            get { return _duplicateProduct; }
            set
            {
                if (_duplicateProduct != value)
                {
                    _duplicateProduct = value;
                    OnPropertyChanged(() => DuplicateProduct);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedProduct.
        /// </summary>
        public base_ProductModel SelectedProduct { get; set; }

        /// <summary>
        /// Gets or sets the DepartmentList
        /// </summary>
        public ObservableCollection<base_DepartmentModel> DepartmentCollection { get; private set; }

        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public ObservableCollection<base_DepartmentModel> CategoryCollection { get; private set; }

        private bool _isChangeInformation = true;
        /// <summary>
        /// Gets or sets the IsChangeInformation.
        /// </summary>
        public bool IsChangeInformation
        {
            get { return _isChangeInformation; }
            set
            {
                if (_isChangeInformation != value)
                {
                    this.DuplicateProduct.IsDirty = true;
                    _isChangeInformation = value;
                    OnPropertyChanged(() => IsChangeInformation);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupDuplicateItemViewModel()
        {
            InitialCommand();
        }

        public PopupDuplicateItemViewModel(base_ProductModel selectedProduct,
            ObservableCollection<base_DepartmentModel> departmentCollection,
            ObservableCollection<base_DepartmentModel> categoryCollection)
            : this()
        {
            DepartmentCollection = departmentCollection;
            CategoryCollection = categoryCollection;
            SelectedProduct = selectedProduct;

            // Initial category and brand collection view
            _categoryCollectionView = CollectionViewSource.GetDefaultView(CategoryCollection);

            // Create new duplicate product
            DuplicateProduct = new base_ProductModel();

            // Register property changed event to process filter category, brand by department
            DuplicateProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

            // Copy selected product to another
            DuplicateProduct.ToModelByDuplicate(SelectedProduct);
            DuplicateProduct.Code = GenProductCode();
            DuplicateProduct.ProductDepartmentId = SelectedProduct.ProductDepartmentId;
            DuplicateProduct.Barcode = string.Empty;
            DuplicateProduct.GroupAttribute = Guid.NewGuid();
            DuplicateProduct.Resource = Guid.NewGuid();
            DuplicateProduct.UserCreated = Define.USER.LoginName;
            DuplicateProduct.DateCreated = DateTimeExt.Now;
            DuplicateProduct.CategoryName = SelectedProduct.CategoryName;
            DuplicateProduct.UOMName = SelectedProduct.UOMName;
            DuplicateProduct.VendorName = SelectedProduct.VendorName;
            DuplicateProduct.Shift = Define.ShiftCode;

            // Turn off IsDirty & IsNew
            DuplicateProduct.EndUpdate();
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
            if (DuplicateProduct == null)
                return false;
            return IsValid && DuplicateProduct.IsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            if (IsChangeInformation)
            {
                // Removes all leading and trailing white-space characters from the ProductName
                DuplicateProduct.ProductName = DuplicateProduct.ProductName.Trim();

                // Check duplicate product
                if (IsDuplicateProduct(DuplicateProduct))
                {
                    // Display alert message
                    Xceed.Wpf.Toolkit.MessageBox.Show("This product is duplicated", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // Create a copy product
                    CopyProduct(SelectedProduct, DuplicateProduct);

                    // Save product to database
                    SaveProduct(DuplicateProduct);
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
        /// Gen product code by format
        /// </summary>
        /// <returns></returns>
        private string GenProductCode()
        {
            return DateTimeExt.Now.ToString(Define.ProductCodeFormat);
        }

        /// <summary>
        /// Check duplicate product
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateProduct(base_ProductModel productModel)
        {
            // Create predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Get all products that IsPurge is false
            predicate = predicate.And(x => x.IsPurge == false);

            // Get all products that duplicate name
            predicate = predicate.And(x => x.ProductName.Equals(productModel.ProductName));

            // Get all products that duplicate category
            predicate = predicate.And(x => x.ProductCategoryId.Equals(productModel.ProductCategoryId));

            return _productRepository.GetIQueryable(predicate).Count() > 0;
        }

        /// <summary>
        /// Check whether allow mutil UOM
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsAllowMutilUOM(base_ProductModel productModel)
        {
            if (!Define.CONFIGURATION.IsAllowMutilUOM.HasValue)
                return false;
            return Define.CONFIGURATION.IsAllowMutilUOM.Value && (productModel != null &&
                (productModel.ItemTypeId == (short)ItemTypes.Stockable || productModel.ItemTypeId == (short)ItemTypes.NonStocked));
        }

        #region Copy Methods

        /// <summary>
        /// Copy relation data of product to target product
        /// </summary>
        /// <param name="targetProductModel"></param>
        private void CopyProduct(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            // Copy photo collection
            CopyPhotoCollection(sourceProductModel, targetProductModel);

            // Copy product store default
            CopyProductStoreDefault(sourceProductModel, targetProductModel);

            // Copy product UOM collection
            CopyProductUOMCollection(sourceProductModel, targetProductModel);

            // Copy product group collection
            CopyProductGroupCollection(sourceProductModel, targetProductModel);

            // Copy vendor product collection
            CopyVendorProductCollection(sourceProductModel, targetProductModel);

            //targetProductModel.ProductStoreCollection = new ObservableCollection<base_ProductStoreModel>();
            targetProductModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
        }

        /// <summary>
        /// Copy photo of selected product to target product
        /// </summary>
        /// <param name="targetProductModel"></param>
        private void CopyPhotoCollection(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            if (sourceProductModel.PhotoCollection != null)
            {
                // Initial photo collection
                targetProductModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();

                foreach (base_ResourcePhotoModel photoItem in sourceProductModel.PhotoCollection)
                {
                    // Create new photo model
                    base_ResourcePhotoModel photoModel = new base_ResourcePhotoModel();

                    // Copy photo of selected product to target product
                    photoModel.ToModel(photoItem);
                    photoModel.ImagePath = photoItem.ImagePath;
                    photoModel.Resource = targetProductModel.Resource.ToString();

                    // Add product UOM to collection
                    targetProductModel.PhotoCollection.Add(photoModel);
                }
            }
        }

        /// <summary>
        /// Copy product store default from source to target product
        /// </summary>
        /// <param name="sourceProductModel">Source product</param>
        /// <param name="targetProductModel">Target product</param>
        /// <param name="storeCode">Store code</param>
        private void CopyProductStoreDefault(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            if (sourceProductModel.ProductStoreDefault != null)
            {
                // Initial product store collection
                targetProductModel.ProductStoreDefault = new base_ProductStoreModel();
                targetProductModel.ProductStoreDefault.Resource = Guid.NewGuid().ToString();
                targetProductModel.ProductStoreDefault.ProductResource = targetProductModel.Resource.ToString();

                // Copy product store default
                targetProductModel.ProductStoreDefault.StoreCode = sourceProductModel.ProductStoreDefault.StoreCode;

                // Initial product store collection
                targetProductModel.ProductStoreCollection = new CollectionBase<base_ProductStoreModel>();

                // Add new product store to collection
                targetProductModel.ProductStoreCollection.Add(targetProductModel.ProductStoreDefault);
            }
        }

        /// <summary>
        /// Copy product UOM of selected product to target product
        /// </summary>
        /// <param name="targetProductModel"></param>
        private void CopyProductUOMCollection(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            if (IsAllowMutilUOM(sourceProductModel) && sourceProductModel.ProductUOMCollection != null)
            {
                // Initial product UOM collection
                targetProductModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>();

                foreach (base_ProductUOMModel productUOMItem in sourceProductModel.ProductUOMCollection)
                {
                    // Create new product UOM model
                    base_ProductUOMModel productUOMModel = new base_ProductUOMModel();

                    // Copy product UOM of selected product to target product
                    productUOMModel.ToModel(productUOMItem);
                    productUOMModel.Resource = Guid.NewGuid().ToString();
                    productUOMModel.ProductStoreResource = targetProductModel.ProductStoreDefault.Resource;

                    // Add product UOM to collection
                    targetProductModel.ProductUOMCollection.Add(productUOMModel);
                }
            }
        }

        /// <summary>
        /// Copy product group collecion
        /// </summary>
        /// <param name="sourceProductModel"></param>
        /// <param name="targetProductModel"></param>
        private void CopyProductGroupCollection(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            if (sourceProductModel.ProductGroupCollection != null)
            {
                // Initial product group collection
                targetProductModel.ProductGroupCollection = new CollectionBase<base_ProductGroupModel>();

                if (sourceProductModel.ProductGroupCollection.Count > 0)
                {
                    targetProductModel.RegularPrice = sourceProductModel.RegularPrice;

                    foreach (base_ProductGroupModel productGroupItem in sourceProductModel.ProductGroupCollection)
                    {
                        // Create new product group model
                        base_ProductGroupModel productGroupModel = new base_ProductGroupModel();

                        // Copy product group from source to target
                        productGroupModel.ToModel(productGroupItem);
                        productGroupModel.Resource = Guid.NewGuid();

                        // Add product group to collection
                        targetProductModel.ProductGroupCollection.Add(productGroupModel);
                    }
                }
            }
        }

        /// <summary>
        /// Copy vendor product collection
        /// </summary>
        /// <param name="sourceProductModel">Source product</param>
        /// <param name="targetProductModel">Target product</param>
        private void CopyVendorProductCollection(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            if (sourceProductModel.VendorProductCollection != null)
            {
                // Initial vendor product collection
                targetProductModel.VendorProductCollection = new CollectionBase<base_VendorProductModel>();

                foreach (base_VendorProductModel vendorProductItem in sourceProductModel.VendorProductCollection)
                {
                    // Create new vendor product model
                    base_VendorProductModel vendorProductModel = new base_VendorProductModel();

                    // Copy vendor product from source to target
                    vendorProductModel.ToModel(vendorProductItem);
                    vendorProductModel.ProductResource = targetProductModel.Resource.ToString();
                    vendorProductModel.VendorCode = vendorProductItem.VendorCode;
                    vendorProductModel.Company = vendorProductItem.Company;
                    vendorProductModel.Phone = vendorProductItem.Phone;
                    vendorProductModel.Email = vendorProductItem.Email;

                    // Add vendor product to collection
                    targetProductModel.VendorProductCollection.Add(vendorProductModel);
                }
            }
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// Save duplicate product
        /// </summary>
        private void SaveProduct(base_ProductModel productModel)
        {
            // Map data from model to entity
            productModel.ToEntity();

            // Save photo collection
            SavePhotoCollection(productModel);

            // Save product store default
            SaveProductStoreCollection(productModel);

            // Save product UOM collection
            SaveProductUOMCollection(productModel, Define.StoreCode);

            // Save product group collection
            SaveProductGroupCollection(productModel);

            // Save vendor product collection
            SaveVendorProductCollection(productModel);

            // Add new product to repository
            _productRepository.Add(productModel.base_Product);

            // Accept changes
            _productRepository.Commit();

            // Update ID from entity to model
            productModel.Id = productModel.base_Product.Id;

            // Update product store id
            UpdateProductStoreID(productModel, Define.StoreCode);

            // Update product UOM id
            UpdateProductUOMID(productModel);

            // Update product group id
            UpdateProductGroupID(productModel);

            // Update vendor product id
            UpdateVendorProductID(productModel);

            // Update default photo if it is deleted
            productModel.PhotoDefault = productModel.PhotoCollection.FirstOrDefault();

            // Clear product UOM collection to refresh
            productModel.ProductUOMCollection = null;

            // Unregister property changed event
            productModel.PropertyChanged -= new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

            // Update ItemType
            productModel.ItemTypeItem = Common.ItemTypes.SingleOrDefault(x => x.Value.Equals(productModel.ItemTypeId));

            // Turn off IsDirty & IsNew
            productModel.EndUpdate();
        }

        /// <summary>
        /// Save photo collection
        /// </summary>
        private void SavePhotoCollection(base_ProductModel productModel)
        {
            if (productModel.PhotoCollection != null)
            {
                foreach (base_ResourcePhotoModel photoItem in productModel.PhotoCollection.DeletedItems)
                {
                    // Delete photo from database
                    _photoRepository.Delete(photoItem.base_ResourcePhoto);
                }

                // Clear deleted photos
                productModel.PhotoCollection.DeletedItems.Clear();

                foreach (base_ResourcePhotoModel photoModel in productModel.PhotoCollection.Where(x => x.IsDirty))
                {
                    // Get photo filename by format
                    string dateTime = DateTimeExt.Now.ToString(Define.GuestNoFormat);
                    string guid = Guid.NewGuid().ToString().Substring(0, 8);
                    string ext = new FileInfo(photoModel.ImagePath).Extension;

                    // Rename photo
                    photoModel.LargePhotoFilename = string.Format("{0}{1}{2}", dateTime, guid, ext);

                    // Update resource photo
                    if (string.IsNullOrWhiteSpace(photoModel.Resource))
                        photoModel.Resource = productModel.Resource.ToString();

                    // Map data from model to entity
                    photoModel.ToEntity();

                    if (photoModel.IsNew)
                        _photoRepository.Add(photoModel.base_ResourcePhoto);

                    // Copy image from client to server
                    SaveImage(photoModel, productModel.Code);

                    // Turn off IsDirty & IsNew
                    photoModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Save product store collection
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="storeCode"></param>
        private void SaveProductStoreCollection(base_ProductModel productModel)
        {
            foreach (base_ProductStoreModel productStoreItem in productModel.ProductStoreCollection)
            {
                // Update quantity in product
                productModel.SetOnHandToStore(productStoreItem.QuantityOnHand, productStoreItem.StoreCode);

                // Backup quantity value
                if (!productStoreItem.IsNew)
                    productStoreItem.OldQuantity = productStoreItem.base_ProductStore.QuantityOnHand;

                // Map data from model to entity
                productStoreItem.ToEntity();

                if (productStoreItem.IsNew)
                {
                    // Add new product store to database
                    productModel.base_Product.base_ProductStore.Add(productStoreItem.base_ProductStore);
                }
                else
                {
                    // Turn off IsDirty & IsNew
                    productStoreItem.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Save product UOM collection
        /// </summary>
        private void SaveProductUOMCollection(base_ProductModel productModel, int storeCode)
        {
            if (IsAllowMutilUOM(productModel) && productModel.ProductUOMCollection != null)
            {
                foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection)
                {
                    if (productUOMItem.UOMId > 0)
                    {
                        // Update quantity on hand for other UOM
                        productUOMItem.UpdateQuantityOnHand(productModel.ProductStoreDefault.QuantityOnHand);

                        // Map data from model to entity
                        productUOMItem.ToEntity();

                        if (productUOMItem.Id == 0)
                        {
                            // Add new product UOM to database
                            productModel.ProductStoreDefault.base_ProductStore.base_ProductUOM.Add(productUOMItem.base_ProductUOM);
                        }
                        else if (productModel.ProductStoreDefault.base_ProductStore.base_ProductUOM.Count(x => x.Id.Equals(productUOMItem.Id)) == 0)
                        {
                            // Add new product UOM to database
                            productModel.ProductStoreDefault.base_ProductStore.base_ProductUOM.Add(productUOMItem.base_ProductUOM);
                        }
                    }
                    else if (productUOMItem.Id > 0)
                    {
                        // Get deleted product UOM
                        base_ProductUOM productUOM = productModel.ProductStoreDefault.base_ProductStore.base_ProductUOM.SingleOrDefault(x => x.Id.Equals(productUOMItem.Id));

                        if (productUOM != null)
                        {
                            // Delete product UOM from database
                            _productUOMRepository.Delete(productUOM);
                        }

                        // Delete entity of product UOM
                        productUOMItem.ClearEntity();
                    }
                }
            }
        }

        /// <summary>
        /// Save product group collection
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveProductGroupCollection(base_ProductModel productModel)
        {
            if (productModel.ProductGroupCollection != null)
            {
                foreach (base_ProductGroupModel productGroupItem in productModel.ProductGroupCollection)
                {
                    // Map data from model to entity
                    productGroupItem.ToEntity();

                    // Add new vendor product to database
                    productModel.base_Product.base_ProductGroup1.Add(productGroupItem.base_ProductGroup);

                    // Turn off IsDirty & IsNew
                    productGroupItem.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Save vendor product collection
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveVendorProductCollection(base_ProductModel productModel)
        {
            if (productModel.VendorProductCollection != null)
            {
                foreach (base_VendorProductModel vendorProductItem in productModel.VendorProductCollection.DeletedItems)
                {
                    // Delete item from database
                    _vendorProductRepository.Delete(vendorProductItem.base_VendorProduct);
                }

                // Clear deleted items
                productModel.VendorProductCollection.DeletedItems.Clear();

                foreach (base_VendorProductModel vendorProductItem in productModel.VendorProductCollection.Where(x => x.IsDirty))
                {
                    // Map data from model to entity
                    vendorProductItem.ToEntity();

                    if (vendorProductItem.IsNew)
                    {
                        // Add new vendor product to database
                        _vendorProductRepository.Add(vendorProductItem.base_VendorProduct);
                    }
                    else
                    {
                        // Turn off IsDirty & IsNew
                        vendorProductItem.EndUpdate();
                    }
                }
            }
        }

        #endregion

        #region UpdateID Methods

        /// <summary>
        /// Update product store id
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="storeCode"></param>
        private void UpdateProductStoreID(base_ProductModel productModel, int storeCode)
        {
            if (productModel.ProductStoreDefault != null && productModel.ProductStoreDefault.IsNew)
            {
                productModel.ProductStoreDefault.Id = productModel.ProductStoreDefault.base_ProductStore.Id;
                productModel.ProductStoreDefault.ProductId = productModel.ProductStoreDefault.base_ProductStore.ProductId;

                // Turn off IsDirty & IsNew
                productModel.ProductStoreDefault.EndUpdate();
            }
        }

        /// <summary>
        /// Update product UOM id after add new to database
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateProductUOMID(base_ProductModel productModel)
        {
            if (productModel.ProductUOMCollection != null)
            {
                foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection)
                {
                    if (productUOMItem.Id == 0)
                    {
                        productUOMItem.Id = productUOMItem.base_ProductUOM.Id;
                        //productUOMItem.ProductId = productUOMItem.base_ProductUOM.ProductId;
                    }

                    // Turn off IsDirty & IsNew
                    productUOMItem.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Update product group id
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateProductGroupID(base_ProductModel productModel)
        {
            if (productModel.ProductGroupCollection != null)
            {
                foreach (base_ProductGroupModel productGroupItem in productModel.ProductGroupCollection)
                {
                    productGroupItem.ProductParentId = productModel.base_Product.Id;

                    // Turn off IsDirty & IsNew
                    productGroupItem.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Update vendor product id after add new to database
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateVendorProductID(base_ProductModel productModel)
        {
            if (productModel.VendorProductCollection != null)
            {
                foreach (base_VendorProductModel vendorProductItem in productModel.VendorProductCollection)
                {
                    if (vendorProductItem.IsNew)
                    {
                        vendorProductItem.Id = vendorProductItem.base_VendorProduct.Id;
                        vendorProductItem.ProductId = vendorProductItem.base_VendorProduct.ProductId;
                    }

                    // Turn off IsDirty & IsNew
                    vendorProductItem.EndUpdate();
                }
            }
        }

        #endregion

        #endregion

        #region Override Methods

        /// <summary>
        /// Process when property changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedProduct_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Get product model
            base_ProductModel productModel = sender as base_ProductModel;

            switch (e.PropertyName)
            {
                case "ProductDepartmentId":
                    if (productModel.ProductDepartmentId >= 0)
                    {
                        // Filter category by department
                        _categoryCollectionView.Filter = x =>
                        {
                            base_DepartmentModel categoryItem = x as base_DepartmentModel;
                            return categoryItem.ParentId.Equals(productModel.ProductDepartmentId);
                        };
                    }
                    break;
                case "ProductCategoryId":
                    if (productModel.ProductCategoryId >= 0)
                    {
                        // Update product brand ID when change product category ID
                        productModel.ProductBrandId = null;

                        // Get selected category 
                        base_DepartmentModel categoryItem = _categoryCollectionView.Cast<base_DepartmentModel>().
                            FirstOrDefault(x => x.Id.Equals(productModel.ProductCategoryId));

                        if (categoryItem != null)
                        {
                            // Update category name
                            productModel.CategoryName = categoryItem.Name;
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Save Image

        /// <summary>
        /// Get product image folder
        /// </summary>
        private string IMG_PRODUCT_DIRECTORY = Path.Combine(Define.CONFIGURATION.DefautlImagePath, "Product");

        /// <summary>
        /// Copy image to server folder
        /// </summary>
        /// <param name="photoModel"></param>
        private void SaveImage(base_ResourcePhotoModel photoModel, string subDirectory)
        {
            try
            {
                // Server image path
                string imgDirectory = Path.Combine(IMG_PRODUCT_DIRECTORY, subDirectory);

                // Create folder image on server if is not exist
                if (!Directory.Exists(imgDirectory))
                    Directory.CreateDirectory(imgDirectory);

                // Check client image to copy to server
                FileInfo clientFileInfo = new FileInfo(photoModel.ImagePath);
                if (clientFileInfo.Exists)
                {
                    // Get file name image
                    string serverFileName = Path.Combine(imgDirectory, photoModel.LargePhotoFilename);
                    FileInfo serverFileInfo = new FileInfo(serverFileName);
                    if (!serverFileInfo.Exists)
                        clientFileInfo.CopyTo(serverFileName, true);
                    photoModel.ImagePath = serverFileName;
                }
                else
                    photoModel.ImagePath = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save Image" + ex.ToString());
            }
        }

        #endregion
    }
}