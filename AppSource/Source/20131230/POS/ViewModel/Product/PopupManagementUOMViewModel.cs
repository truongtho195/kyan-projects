using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupManagementUOMViewModel : ViewModelBase
    {
        #region Defines

        /// <summary>
        /// Get product image folder
        /// </summary>
        private string IMG_PRODUCT_DIRECTORY = Path.Combine(Define.CONFIGURATION.DefautlImagePath, "Product");

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();

        private int? _oldOrderUOMID;

        private base_Product _existedProduct;
        private base_ProductUOM _existedProductUOM;
        private base_ProductModel _existedProductModel;

        #endregion

        #region Properties

        private base_ProductModel _selectedProduct = new base_ProductModel();
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

        /// <summary>
        /// Gets or sets the UOMList.
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> UOMList { get; set; }

        /// <summary>
        /// Gets or sets the PriceSchemaList
        /// </summary>
        public List<PriceModel> PriceSchemaList { get; set; }

        private ObservableCollection<CheckBoxItemModel> _selectedUOMList;
        /// <summary>
        /// Gets or sets the SelectedUOMList.
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> SelectedUOMList
        {
            get { return _selectedUOMList; }
            set
            {
                if (_selectedUOMList != value)
                {
                    _selectedUOMList = value;
                    OnPropertyChanged(() => SelectedUOMList);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ResultProductModel
        /// </summary>
        public base_ProductModel ResultProductModel { get; set; }

        /// <summary>
        /// Gets the AllowEditProductUOM.
        /// </summary>
        public bool AllowEditProductUOM
        {
            get
            {
                if (SelectedProduct == null)
                    return UserPermissions.AllowAccessProductPermission;

                return SelectedProduct.BaseUOMId != 0 && UserPermissions.AllowAccessProductPermission;
            }
        }

        /// <summary>
        /// Gets the IsStockableItemType
        /// </summary>
        public bool IsStockableItemType
        {
            get
            {
                if (SelectedProduct == null)
                    return false;
                return SelectedProduct.ItemTypeId == (short)ItemTypes.Stockable && UserPermissions.AllowAccessProductPermission;
            }
        }

        /// <summary>
        /// Gets or sets the ItemTypeList.
        /// </summary>
        public ObservableCollection<ComboItem> ItemTypeList { get; set; }

        /// <summary>
        /// Gets or sets the DepartmentCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> DepartmentCollection { get; set; }

        /// <summary>
        /// Gets or sets the CategoryCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> CategoryCollection { get; set; }

        /// <summary>
        /// Gets or sets the BrandCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> BrandCollection { get; set; }

        /// <summary>
        /// Gets or sets the VendorCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorCollection { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupManagementUOMViewModel()
        {
            _ownerViewModel = this;

            InitialCommand();
            OnPropertyChanged(() => IsStockableItemType);
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="uomList"></param>
        /// <param name="selectedProduct"></param>
        public PopupManagementUOMViewModel(ObservableCollection<CheckBoxItemModel> uomList,
            base_ProductModel selectedProduct, List<PriceModel> priceSchemaList)
            : this()
        {
            // Create UOM list
            UOMList = new ObservableCollection<CheckBoxItemModel>(uomList.CloneList());

            PriceSchemaList = new List<PriceModel>(priceSchemaList);

            // Create selected UOM list
            SelectedUOMList = new ObservableCollection<CheckBoxItemModel>(uomList.Where(x => x.IsChecked));
            SelectedUOMList.Insert(0, new CheckBoxItemModel());

            // Copy UOM values to backup
            CopyUOM(SelectedProduct, selectedProduct);
            SelectedProduct.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>(selectedProduct.ProductUOMCollection.CloneList());

            // Register event to update selected UOM list
            SelectedProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

            // Register event to update selected UOM list
            foreach (base_ProductUOMModel productUOMModel in SelectedProduct.ProductUOMCollection)
            {
                productUOMModel.PropertyChanged += new PropertyChangedEventHandler(productUOMModel_PropertyChanged);
                productUOMModel.IsChecked = productUOMModel.UOMId > 0 && UserPermissions.AllowAccessProductPermission;
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
            return IsValid() && IsEdit();
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            ResultProductModel = SelectedProduct;

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

        #region PopupPricingCommand

        /// <summary>
        /// Gets the PopupPricingCommand command.
        /// </summary>
        public ICommand PopupPricingCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupPricingCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupPricingCommandCanExecute(object param)
        {
            if (param == null)
                return false;

            short uomID = 0;
            if (Int16.TryParse(param.ToString(), out uomID))
                uomID = Int16.Parse(param.ToString());

            return uomID > 0 && UserPermissions.AllowAccessProductPermission;
        }

        /// <summary>
        /// Method to invoke when the PopupPricingCommand command is executed.
        /// </summary>
        private void OnPopupPricingCommandExecute(object param)
        {
            int selectedUOMID = 0;
            if (param != null && int.TryParse(param.ToString(), out selectedUOMID))
            {
                selectedUOMID = int.Parse(param.ToString());
                PopupPricingViewModel viewModel = new PopupPricingViewModel(SelectedProduct, UOMList, PriceSchemaList, selectedUOMID);
                bool? result = _dialogService.ShowDialog<PopupPricingView>(this, viewModel, "Pricing");
                if (result.HasValue && result.Value)
                {
                    base_ProductUOMModel baseProductUOM = viewModel.ProductUOMList.FirstOrDefault(x => x.UOMId.Equals(SelectedProduct.BaseUOMId));
                    if (baseProductUOM != null)
                        viewModel.ProductToProductUOM(SelectedProduct, baseProductUOM, true);

                    foreach (base_ProductUOMModel productUOMModel in SelectedProduct.ProductUOMCollection)
                    {
                        base_ProductUOMModel productUOMItem = viewModel.ProductUOMList.FirstOrDefault(x => x.UOMId.Equals(productUOMModel.UOMId));
                        if (productUOMItem != null)
                            productUOMModel.ToModel(productUOMItem);
                    }
                }
            }
        }

        #endregion

        #region CheckBarcodeCommand

        /// <summary>
        /// Gets the CheckBarcodeCommand command.
        /// </summary>
        public ICommand CheckBarcodeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CheckBarcodeCommand command is executed.
        /// </summary>
        private void OnCheckBarcodeCommandExecute(object param)
        {
            bool isDuplicate = false;
            string barcode = string.Empty;
            if (param == null)
            {
                // Get barcode of base uom
                if (!string.IsNullOrWhiteSpace(SelectedProduct.Barcode))
                    barcode = SelectedProduct.Barcode.Trim();

                isDuplicate = IsDuplicateUPC(SelectedProduct, barcode.ToLower());
                SelectedProduct.IsDuplicateUPC = isDuplicate;
            }
            else
            {
                base_ProductUOMModel productUOMModel = param as base_ProductUOMModel;

                // Get barcode of other uom
                if (!string.IsNullOrWhiteSpace(productUOMModel.UPC))
                    barcode = productUOMModel.UPC.Trim();

                isDuplicate = IsDuplicateUPC(SelectedProduct, barcode.ToLower());
                productUOMModel.IsDuplicateUPC = isDuplicate;
            }

            // Check duplicate barcode
            if (isDuplicate)
            {
                string message = string.Format("Scan code: {0} of this product is existed.\nDo you wan to view existed product?", barcode);
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(message, "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    ShowPopupViewExistedProduct();
                }
            }
        }

        #endregion

        #region CheckALUCommand

        /// <summary>
        /// Gets the CheckALUCommand command.
        /// </summary>
        public ICommand CheckALUCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CheckALUCommand command is executed.
        /// </summary>
        private void OnCheckALUCommandExecute(object param)
        {
            bool isDuplicate = false;
            string barcode = string.Empty;
            if (param == null)
            {
                // Get alternate barcode of base uom
                if (!string.IsNullOrWhiteSpace(SelectedProduct.ALU))
                    barcode = SelectedProduct.ALU.Trim();

                isDuplicate = IsDuplicateALU(SelectedProduct, barcode.ToLower());
                SelectedProduct.IsDuplicateALU = isDuplicate;
            }
            else
            {
                base_ProductUOMModel productUOMModel = param as base_ProductUOMModel;

                // Get alternate barcode of base uom
                // Get barcode of other uom
                if (!string.IsNullOrWhiteSpace(productUOMModel.ALU))
                    barcode = productUOMModel.ALU.Trim();

                isDuplicate = IsDuplicateALU(SelectedProduct, barcode.ToLower());
                productUOMModel.IsDuplicateALU = isDuplicate;
            }

            // Check duplicate alternate barcode
            if (isDuplicate)
            {
                string message = string.Format("ALU: {0} of this product is existed.\nDo you wan to view existed product?", barcode);
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(message, "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    ShowPopupViewExistedProduct();
                }
            }
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
            PopupPricingCommand = new RelayCommand<object>(OnPopupPricingCommandExecute, OnPopupPricingCommandCanExecute);
            CheckBarcodeCommand = new RelayCommand<object>(OnCheckBarcodeCommandExecute);
            CheckALUCommand = new RelayCommand<object>(OnCheckALUCommandExecute);
        }

        private bool IsValid()
        {
            return !SelectedProduct.IsDuplicateUPC && !SelectedProduct.IsDuplicateALU &&
                (SelectedProduct.ProductUOMCollection != null &&
                !SelectedProduct.ProductUOMCollection.Any(x => x.IsDuplicateUPC || x.IsDuplicateALU));
        }

        private bool IsEdit()
        {
            return SelectedProduct.IsDirty ||
                (SelectedProduct.ProductUOMCollection != null && SelectedProduct.ProductUOMCollection.IsDirty);
        }

        /// <summary>
        /// Copy UOM values
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public void CopyUOM(base_ProductModel target, base_ProductModel source)
        {
            target.Code = source.Code;
            target.ItemTypeId = source.ItemTypeId;
            target.ProductName = source.ProductName;
            target.ProductDepartmentId = source.ProductDepartmentId;
            target.ProductCategoryId = source.ProductCategoryId;
            target.ProductBrandId = source.ProductBrandId;
            target.VendorId = source.VendorId;
            target.StyleModel = source.StyleModel;
            target.Attribute = source.Attribute;
            target.Size = source.Size;
            target.Description = source.Description;
            target.OnHandStore = source.OnHandStore;
            target.RegularPrice = source.RegularPrice;
            target.OrderCost = source.OrderCost;
            target.Price1 = source.Price1;
            target.Price2 = source.Price2;
            target.Price3 = source.Price3;
            target.Price4 = source.Price4;
            target.AverageUnitCost = source.AverageUnitCost;
            target.MarginPercent = source.MarginPercent;
            target.MarkupPercent = source.MarkupPercent;
            target.BaseUOMId = source.BaseUOMId;
            target.SellUOMId = source.SellUOMId;
            target.OrderUOMId = source.OrderUOMId;
            target.MarkdownPercent1 = source.MarkdownPercent1;
            target.MarkdownPercent2 = source.MarkdownPercent2;
            target.MarkdownPercent3 = source.MarkdownPercent3;
            target.MarkdownPercent4 = source.MarkdownPercent4;
            target.IsDuplicateUPC = source.IsDuplicateUPC;
            target.IsDuplicateALU = source.IsDuplicateALU;
            if (!string.IsNullOrWhiteSpace(source.Barcode))
                target.Barcode = source.Barcode.Trim();
            if (!string.IsNullOrWhiteSpace(source.ALU))
                target.ALU = source.ALU.Trim();
            target.Resource = source.Resource;
            target.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(source.PhotoCollection);
        }

        /// <summary>
        /// Update selected UOM List
        /// </summary>
        /// <param name="uomID"></param>
        private void UpdateSelectedUOMList(int uomID)
        {
            // Get selected UOM item
            CheckBoxItemModel selectedUOMItem = UOMList.FirstOrDefault(x => x.Value.Equals(uomID));
            if (selectedUOMItem != null && uomID > 0)
                SelectedUOMList.Add(selectedUOMItem);

            // Get selected UOM list
            List<int> selectedUOMList = SelectedProduct.ProductUOMCollection.Select(x => x.UOMId).ToList();
            selectedUOMList.Add(SelectedProduct.BaseUOMId);

            // Get deleted UOM item
            CheckBoxItemModel deletedUOMItem = SelectedUOMList.FirstOrDefault(x => !selectedUOMList.Contains(x.Value));
            SelectedUOMList.Remove(deletedUOMItem);
        }

        /// <summary>
        /// Check barcode duplicate
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateUPC(base_ProductModel productModel, string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return false;

                _existedProduct = null;

                // Check duplicate in collection
                List<string> barcodes = productModel.ProductUOMCollection.Where(x => !string.IsNullOrWhiteSpace(x.UPC)).Select(x => x.UPC).ToList();
                if (!string.IsNullOrWhiteSpace(productModel.Barcode))
                    barcodes.Add(productModel.Barcode);
                if (barcodes.Count(x => x.Equals(barcode)) > 1)
                {
                    _existedProductModel = productModel;
                    return true;
                }
                else
                {
                    // Clear duplicate error
                    if (productModel.Barcode.Equals(barcode))
                    {
                        productModel.IsDuplicateUPC = false;
                    }
                    foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection.Where(x => !string.IsNullOrWhiteSpace(x.UPC) && x.UPC.Equals(barcode)))
                    {
                        productUOMItem.IsDuplicateUPC = false;
                    }
                }

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.Resource.Equals(productModel.Resource));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.Barcode.ToLower().Equals(barcode));

                // Create predicate
                Expression<Func<base_ProductUOM, bool>> predicateUOM = PredicateBuilder.True<base_ProductUOM>();

                // Get all ProductUOM that IsPurge is false
                predicateUOM = predicateUOM.And(x => x.base_ProductStore.base_Product.IsPurge == false && !x.base_ProductStore.base_Product.Resource.Equals(productModel.Resource));

                // Get all ProductUOM that duplicate barcode
                predicateUOM = predicateUOM.And(x => x.UPC.ToLower().Equals(barcode));

                _existedProduct = _productRepository.Get(predicate);
                _existedProductUOM = _productUOMRepository.Get(predicateUOM);

                return _existedProduct != null || _existedProductUOM != null;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Check alternate barcode duplicate
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateALU(base_ProductModel productModel, string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return false;

                _existedProduct = null;
                _existedProductUOM = null;

                // Check duplicate in collection
                List<string> barcodes = productModel.ProductUOMCollection.Where(x => !string.IsNullOrWhiteSpace(x.ALU)).Select(x => x.ALU).ToList();
                if (!string.IsNullOrWhiteSpace(productModel.ALU))
                    barcodes.Add(productModel.ALU);
                if (barcodes.Count(x => x.Equals(barcode)) > 1)
                {
                    _existedProductModel = productModel;
                    return true;
                }
                else
                {
                    // Clear duplicate error
                    if (productModel.Barcode.Equals(barcode))
                    {
                        productModel.IsDuplicateALU = false;
                    }
                    foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection.Where(x => !string.IsNullOrWhiteSpace(x.ALU) && x.ALU.Equals(barcode)))
                    {
                        productUOMItem.IsDuplicateALU = false;
                    }
                }

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.Resource.Equals(productModel.Resource));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.ALU.ToLower().Equals(barcode));

                // Create predicate
                Expression<Func<base_ProductUOM, bool>> predicateUOM = PredicateBuilder.True<base_ProductUOM>();

                // Get all ProductUOM that IsPurge is false
                predicateUOM = predicateUOM.And(x => x.base_ProductStore.base_Product.IsPurge == false && !x.base_ProductStore.base_Product.Resource.Equals(productModel.Resource));

                // Get all ProductUOM that duplicate barcode
                predicateUOM = predicateUOM.And(x => x.ALU.ToLower().Equals(barcode));

                _existedProduct = _productRepository.Get(predicate);
                _existedProductUOM = _productUOMRepository.Get(predicateUOM);

                return _existedProduct != null || _existedProductUOM != null;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Load photo collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadPhotoCollection(base_ProductModel productModel)
        {
            try
            {
                if (productModel.PhotoCollection == null)
                {
                    // Get product resource
                    string resource = productModel.Resource.ToString();

                    // Get all photo from database
                    productModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(
                        _photoRepository.GetAll(x => x.Resource.Equals(resource)).
                        Select(x => new base_ResourcePhotoModel(x)
                        {
                            ImagePath = Path.Combine(IMG_PRODUCT_DIRECTORY, productModel.Code, x.LargePhotoFilename),
                            IsDirty = false
                        }));

                    // Set default photo
                    productModel.PhotoDefault = productModel.PhotoCollection.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// View detail existed product
        /// </summary>
        private void ShowPopupViewExistedProduct()
        {
            // Get existed product model
            if (_existedProduct != null)
            {
                _existedProductModel = new base_ProductModel(_existedProduct);
            }
            else if (_existedProductUOM != null)
            {
                _existedProductModel = new base_ProductModel(_existedProductUOM.base_ProductStore.base_Product);
            }

            // Load photo collection
            LoadPhotoCollection(_existedProductModel);

            PopupViewExistedProductViewModel viewModel = new PopupViewExistedProductViewModel(_existedProductModel);
            viewModel.ItemTypeList = ItemTypeList;
            viewModel.DepartmentCollection = DepartmentCollection;
            viewModel.CategoryCollection = CategoryCollection;
            viewModel.BrandCollection = BrandCollection;
            viewModel.VendorCollection = VendorCollection;
            bool? result = _dialogService.ShowDialog<PopupViewExistedProductView>(_ownerViewModel, viewModel, "View Existed Product");
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Process update selected UOM list when UOM ID changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void productUOMModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Get product UOM model from sender
            base_ProductUOMModel productUOMModel = sender as base_ProductUOMModel;

            switch (e.PropertyName)
            {
                case "UOMId":
                    UpdateSelectedUOMList(productUOMModel.UOMId);
                    if (productUOMModel.UOMId == 0)
                    {
                        productUOMModel.BaseUnitNumber = 0;
                        productUOMModel.RegularPrice = 0;
                        productUOMModel.QuantityOnHand = 0;
                        productUOMModel.AverageCost = 0;
                        productUOMModel.UPC = null;
                        productUOMModel.ALU = null;
                    }
                    else if (productUOMModel.BaseUnitNumber == 0)
                    {
                        productUOMModel.BaseUnitNumber = 1;
                    }

                    // Set permission edit product UOM detail
                    productUOMModel.AllowEditProductUOMDetail = productUOMModel.UOMId != 0 && UserPermissions.AllowAccessProductPermission;
                    break;
                case "BaseUnitNumber":
                    // Update average cost
                    productUOMModel.UpdateAverageCost(SelectedProduct.AverageUnitCost);

                    // Update quantity on hand
                    productUOMModel.UpdateQuantityOnHand(SelectedProduct.OnHandStore);
                    break;
                case "RegularPrice":
                case "AverageCost":
                    // Calculator margin, markup and price
                    productUOMModel.UpdateMarginMarkupAndPrice();
                    break;
            }
        }

        /// <summary>
        /// Process update select UOM list when base UOM changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedProduct_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Get product model from sender
            base_ProductModel productModel = sender as base_ProductModel;

            switch (e.PropertyName)
            {
                case "BaseUOMId":
                    UpdateSelectedUOMList(productModel.BaseUOMId);
                    if (productModel.BaseUOMId == 0)
                    {
                        // Update product UOM id value
                        foreach (base_ProductUOMModel productUOMModel in productModel.ProductUOMCollection)
                            productUOMModel.UOMId = 0;
                    }

                    OnPropertyChanged(() => AllowEditProductUOM);
                    break;
                case "OrderUOMId":
                    if (productModel.OrderUOMId > 0 &&
                        productModel.OrderUOMId != productModel.BaseUOMId &&
                        productModel.OrderUOMId != _oldOrderUOMID)
                    {
                        PopupUpdateOrderCostViewModel viewModel = new PopupUpdateOrderCostViewModel(productModel.OrderCost);
                        bool? result = _dialogService.ShowDialog<PopupUpdateOrderCostView>(this, viewModel, "Update order cost");
                        if (result.HasValue && result.Value)
                        {
                            // Backup order UOM id value
                            _oldOrderUOMID = productModel.OrderUOMId;

                            if (viewModel.UpdateOrderCostOption.Equals(0))
                                productModel.OrderCost = viewModel.NewOrderCost;
                            else if (viewModel.UpdateOrderCostOption.Equals(1))
                                productModel.OrderCost = 0;
                        }
                        else
                        {
                            // Restore order UOM id value
                            App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                            {
                                productModel.OrderUOMId = _oldOrderUOMID;
                            });
                        }
                    }
                    else
                        _oldOrderUOMID = productModel.OrderUOMId;
                    break;
                case "RegularPrice":
                    // Calculator margin, markup and price
                    productModel.UpdateMarginMarkupAndPrice();
                    break;
                case "AverageUnitCost":
                    // Calculator margin, markup and price
                    productModel.UpdateMarginMarkupAndPrice();

                    // Update average cost for other UOM
                    foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection.Where(x => x.UOMId > 0))
                        productUOMItem.UpdateAverageCost(productModel.AverageUnitCost);
                    break;
                case "OnHandStore":
                    productModel.IsDirty = true;

                    // Update quantity on hand for other UOM
                    foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection.Where(x => x.UOMId > 0))
                        productUOMItem.UpdateQuantityOnHand(productModel.OnHandStore);
                    break;
            }
        }

        #endregion
    }
}