﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;
using CPCToolkitExtLibraries;
using CPC.DragDrop;
using CPC.DragDrop.Helper;

namespace CPC.POS.ViewModel
{
    public class ProductManualViewModel : ViewModelBase, IDragSource, IDropTarget
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_UOMRepository _uomRepository = new base_UOMRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();
        private base_SaleTaxLocationRepository _taxLocationRepository = new base_SaleTaxLocationRepository();
        private base_VendorProductRepository _vendorProductRepository = new base_VendorProductRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_PurchaseOrderDetailRepository _purchaseOrderDetailRepository = new base_PurchaseOrderDetailRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_ProductGroupRepository _productGroupRepository = new base_ProductGroupRepository();
        private base_ProductStoreRepository _productStoreRepository = new base_ProductStoreRepository();

        private base_CostAdjustmentRepository _costAdjustmentRepository = new base_CostAdjustmentRepository();
        private base_QuantityAdjustmentRepository _quantityAdjustmentRepository = new base_QuantityAdjustmentRepository();

        private ICollectionView _categoryCollectionView;
        private ICollectionView _brandCollectionView;

        private short _oldItemTypeID;
        private short _itemTypeID = (short)ItemTypes.Group;

        #endregion

        #region Properties

        #region Search

        private bool isSearchMode = false;
        /// <summary>
        /// Gets or sets a value that indicates whether the grid-search is open.
        /// </summary>
        /// <returns>true if open grid-search; otherwise, false.</returns>
        public bool IsSearchMode
        {
            get { return isSearchMode; }
            set
            {
                if (value != isSearchMode)
                {
                    isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }

        private string _keyword;

        private ObservableCollection<string> _columnCollection;
        /// <summary>
        /// Gets or sets the ColumnCollection.
        /// </summary>
        public ObservableCollection<string> ColumnCollection
        {
            get { return _columnCollection; }
            set
            {
                if (_columnCollection != value)
                {
                    _columnCollection = value;
                    OnPropertyChanged(() => ColumnCollection);
                }
            }
        }

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

        #endregion

        private ObservableCollection<base_ProductModel> _productCollection = new ObservableCollection<base_ProductModel>();
        /// <summary>
        /// Gets or sets the ProductCollection.
        /// </summary>
        public ObservableCollection<base_ProductModel> ProductCollection
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

        private int _totalProducts;
        /// <summary>
        /// Gets or sets the TotalProducts.
        /// </summary>
        public int TotalProducts
        {
            get { return _totalProducts; }
            set
            {
                if (_totalProducts != value)
                {
                    _totalProducts = value;
                    OnPropertyChanged(() => TotalProducts);
                }
            }
        }

        private ObservableCollection<base_DepartmentModel> _departmentCollection;
        /// <summary>
        /// Gets or sets the DepartmentCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> DepartmentCollection
        {
            get { return _departmentCollection; }
            set
            {
                if (_departmentCollection != value)
                {
                    _departmentCollection = value;
                    OnPropertyChanged(() => DepartmentCollection);
                }
            }
        }

        private ObservableCollection<base_DepartmentModel> _categoryCollection;
        /// <summary>
        /// Gets or sets the CategoryCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> CategoryCollection
        {
            get { return _categoryCollection; }
            set
            {
                if (_categoryCollection != value)
                {
                    _categoryCollection = value;
                    OnPropertyChanged(() => CategoryCollection);
                }
            }
        }

        private ObservableCollection<base_DepartmentModel> _brandCollection;
        /// <summary>
        /// Gets or sets the BrandCollection.
        /// </summary>
        public ObservableCollection<base_DepartmentModel> BrandCollection
        {
            get { return _brandCollection; }
            set
            {
                if (_brandCollection != value)
                {
                    _brandCollection = value;
                    OnPropertyChanged(() => BrandCollection);
                }
            }
        }

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

        private ObservableCollection<base_GuestModel> _productVendorCollection;
        /// <summary>
        /// Gets or sets the ProductVendorCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> ProductVendorCollection
        {
            get { return _productVendorCollection; }
            set
            {
                if (_productVendorCollection != value)
                {
                    _productVendorCollection = value;
                    OnPropertyChanged(() => ProductVendorCollection);
                }
            }
        }

        private ObservableCollection<CheckBoxItemModel> _uomList;
        /// <summary>
        /// Gets or sets the UOMList.
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> UOMList
        {
            get { return _uomList; }
            set
            {
                if (_uomList != value)
                {
                    _uomList = value;
                    OnPropertyChanged(() => UOMList);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SaleTaxLocationList
        /// </summary>
        public List<string> SaleTaxLocationList { get; set; }

        /// <summary>
        /// Gets the IsAllowMutilUOM
        /// </summary>
        public bool AllowMutilUOM
        {
            get
            {
                return IsAllowMutilUOM(SelectedProduct);
            }
        }

        /// <summary>
        /// Gets the IsEditOnHandQuantity
        /// </summary>
        public bool IsEditOnHandQuantity
        {
            get
            {
                if (SelectedProduct == null)
                    return AllowAccessPermission;
                return SelectedProduct.ItemTypeId == (short)ItemTypes.Stockable && AllowAccessPermission;
            }
        }

        /// <summary>
        /// Gets or sets the PriceSchemaList.
        /// </summary>
        public List<PriceModel> PriceSchemaList { get; set; }

        private base_GuestModel _selectedVendor;
        /// <summary>
        /// Gets or sets the SelectedVendor.
        /// </summary>
        public base_GuestModel SelectedVendor
        {
            get { return _selectedVendor; }
            set
            {
                if (_selectedVendor != value)
                {
                    OnSelectedVendorChanging(SelectedVendor, value);
                    _selectedVendor = value;
                    OnPropertyChanged(() => SelectedVendor);
                }
            }
        }

        private base_GuestModel _selectedVendorProduct;
        /// <summary>
        /// Gets or sets the SelectedVendorProduct.
        /// </summary>
        public base_GuestModel SelectedVendorProduct
        {
            get { return _selectedVendorProduct; }
            set
            {
                if (_selectedVendorProduct != value)
                {
                    _selectedVendorProduct = value;
                    OnPropertyChanged(() => SelectedVendor);
                    if (SelectedVendorProduct != null)
                        OnSelectedVendorProductChanged(SelectedProduct, SelectedVendorProduct);
                }
            }
        }

        private base_SaleOrderModel _totalSaleOrder;
        /// <summary>
        /// Gets or sets the TotalSaleOrder.
        /// </summary>
        public base_SaleOrderModel TotalSaleOrder
        {
            get { return _totalSaleOrder; }
            set
            {
                if (_totalSaleOrder != value)
                {
                    _totalSaleOrder = value;
                    OnPropertyChanged(() => TotalSaleOrder);
                }
            }
        }

        private base_PurchaseOrderModel _totalPurchaseOrder;
        /// <summary>
        /// Gets or sets the TotalPurchaseOrder.
        /// </summary>
        public base_PurchaseOrderModel TotalPurchaseOrder
        {
            get { return _totalPurchaseOrder; }
            set
            {
                if (_totalPurchaseOrder != value)
                {
                    _totalPurchaseOrder = value;
                    OnPropertyChanged(() => TotalPurchaseOrder);
                }
            }
        }

        private ObservableCollection<ComboItem> _itemTypeList;
        /// <summary>
        /// Gets or sets the ItemTypeList.
        /// </summary>
        public ObservableCollection<ComboItem> ItemTypeList
        {
            get { return _itemTypeList; }
            set
            {
                if (_itemTypeList != value)
                {
                    _itemTypeList = value;
                    OnPropertyChanged(() => ItemTypeList);
                }
            }
        }

        /// <summary>
        /// Gets the IsGroupItemType
        /// </summary>
        public bool IsGroupItemType
        {
            get
            {
                if (SelectedProduct == null)
                    return false;
                return SelectedProduct.ItemTypeId == (short)ItemTypes.Group;
            }
        }

        /// <summary>
        /// Gets the AllowGroupItemType
        /// </summary>
        public bool AllowGroupItemType
        {
            get { return !IsGroupItemType; }
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
                    //OnSelectedSubProductChanged();
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

        private CollectionBase<base_ProductModel> _productSubCollection;
        /// <summary>
        /// Gets or sets the ProductSubCollection.
        /// </summary>
        public CollectionBase<base_ProductModel> ProductSubCollection
        {
            get { return _productSubCollection; }
            set
            {
                if (_productSubCollection != value)
                {
                    _productSubCollection = value;
                    OnPropertyChanged(() => ProductSubCollection);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Total
        /// </summary>
        public decimal Total { get; set; }

        private bool _focusDefault;
        /// <summary>
        /// Gets or sets the FocusDefault.
        /// </summary>
        public bool FocusDefault
        {
            get { return _focusDefault; }
            set
            {
                if (_focusDefault != value)
                {
                    _focusDefault = value;
                    OnPropertyChanged(() => FocusDefault);
                }
            }
        }

        /// <summary>
        /// Gets the IsManualGenerate.
        /// </summary>
        public bool IsManualGenerate
        {
            get
            {
                if (Define.CONFIGURATION == null)
                    return false;
                return Define.CONFIGURATION.IsManualGenerate;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ProductViewModel class.
        /// </summary>
        public ProductManualViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            StickyManagementViewModel = new PopupStickyViewModel();

            // Load static data
            LoadStaticData();

            InitialCommand();
        }

        /// <summary>
        /// Initializes a new instance of the ProductViewModel class with parameter.
        /// </summary>
        /// <param name="isList">True if show search list, otherwise show detail form</param>
        /// <param name="param">Optional parameter. Default is null</param>
        public ProductManualViewModel(bool isList, object param = null)
            : this()
        {
            ChangeSearchMode(isList, param);
        }

        #endregion

        #region Command Methods

        #region SearchCommand

        /// <summary>
        /// Gets the SearchCommand command.
        /// </summary>
        public ICommand SearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                _keyword = param.ToString();

                // Load data by predicate
                LoadDataByPredicate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region PopupAdvanceSearchCommand

        /// <summary>
        /// Gets the PopupAdvanceSearchCommand command.
        /// </summary>
        public ICommand PopupAdvanceSearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupAdvanceSearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupAdvanceSearchCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupAdvanceSearchCommand command is executed.
        /// </summary>
        private void OnPopupAdvanceSearchCommandExecute(object param)
        {
            PopupProductManualAdvanceSearchViewModel viewModel = new PopupProductManualAdvanceSearchViewModel();
            bool? msgResult = _dialogService.ShowDialog<PopupProductManualAdvanceSearchView>(_ownerViewModel, viewModel, "Advance Search");
            if (msgResult.HasValue && msgResult.Value)
            {
                // Load data by search predicate
                LoadDataByPredicate(viewModel.AdvanceSearchPredicate, false, 0);
            }
        }

        #endregion

        #region NewCommand

        /// <summary>
        /// Gets the NewCommand command.
        /// </summary>
        public ICommand NewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return UserPermissions.AllowAddProduct;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            if (IsSearchMode)
            {
                IsSearchMode = false;
                NewProduct();
            }
            else if (ShowNotification(null))
                NewProduct();
        }

        #endregion

        #region EditCommand

        /// <summary>
        /// Gets the EditCommand command.
        /// </summary>
        public ICommand EditCommand { get; private set; }

        /// <summary>
        /// Method to check whether the EditCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the EditCommand command is executed.
        /// </summary>
        private void OnEditCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            // Edit selected item
            OnDoubleClickViewCommandExecute(dataGridControl.SelectedItem);
        }

        #endregion

        #region SaveCommand

        /// <summary>
        /// Gets the SaveCommand command.
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return IsValid && IsEdit();
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            SaveProduct(SelectedProduct);
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
        private bool OnDeleteCommandCanExecute()
        {
            if (SelectedProduct == null)
                return false;
            return !IsEdit() && !SelectedProduct.IsNew && UserPermissions.AllowDeleteProduct;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this product?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                try
                {
                    if (SelectedProduct.IsNew)
                    {
                        StickyManagementViewModel.DeleteAllResourceNote();

                        SelectedProduct = null;
                    }
                    else if (IsValid)
                    {
                        StickyManagementViewModel.DeleteAllResourceNote();

                        // Delete product
                        SelectedProduct.base_Product.IsPurge = true;

                        // Accept changes
                        _productRepository.Commit();

                        // Turn off IsDirty & IsNew
                        SelectedProduct.EndUpdate();

                        // Remove from collection
                        ProductCollection.Remove(SelectedProduct);

                        // Update total products
                        TotalProducts--;
                    }
                    else
                        return;

                    IsSearchMode = true;
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region DeletesCommand

        /// <summary>
        /// Gets the DeletesCommand command.
        /// </summary>
        public ICommand DeletesCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeletesCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeletesCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count > 0 && UserPermissions.AllowDeleteProduct;
        }

        /// <summary>
        /// Method to invoke when the DeletesCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            DataGridControl dataGridControl = param as DataGridControl;

            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete these products?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult.Is(MessageBoxResult.No))
                return;

            try
            {
                foreach (base_ProductModel productModel in dataGridControl.SelectedItems.Cast<base_ProductModel>().ToList())
                {
                    // Delete all note of this product
                    StickyManagementViewModel.DeleteAllResourceNote(productModel.ResourceNoteCollection);

                    // Delete product
                    productModel.base_Product.IsPurge = true;

                    // Accept changes
                    _productRepository.Commit();

                    // Turn off IsDirty & IsNew
                    productModel.EndUpdate();

                    // Remove from collection
                    ProductCollection.Remove(productModel);

                    // Update total products
                    TotalProducts--;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region DoubleClickViewCommand

        /// <summary>
        /// Gets the DoubleClickViewCommand command.
        /// </summary>
        public ICommand DoubleClickViewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DoubleClickViewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DoubleClickViewCommand command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            if (param != null && IsSearchMode)
            {
                // Reset UOM list
                foreach (CheckBoxItemModel checkBoxItemModel in UOMList)
                    checkBoxItemModel.IsChecked = false;

                // Keep old item type id
                _oldItemTypeID = (param as base_ProductModel).ItemTypeId;

                // Update selected product
                SelectedProduct = param as base_ProductModel;

                // Keep old cost
                SelectedProduct.OldCost = SelectedProduct.AverageUnitCost;

                // Load photo collection
                LoadPhotoCollection(SelectedProduct);

                // Load product store collection
                LoadProductStoreCollection(SelectedProduct, Define.StoreCode);

                // Load product UOM collection
                //LoadProductUOMCollection(SelectedProduct, Define.StoreCode);
                CheckBoxItemModel checkBoxUOMItem = UOMList.SingleOrDefault(x => x.Value.Equals(SelectedProduct.BaseUOMId));
                if (checkBoxUOMItem != null)
                    checkBoxUOMItem.IsChecked = true;

                // Load all product group that have same parent ID
                LoadProductGroupCollection(SelectedProduct);

                // Load vendor product collection
                LoadVendorProductCollection(SelectedProduct);

                // Load sale order collection
                LoadSaleOrderCollection(SelectedProduct, Define.StoreCode);

                // Load purchase order collection
                LoadPurchaseOrderCollection(SelectedProduct, Define.StoreCode);

                // Register property changed event to process filter category, brand by department
                SelectedProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

                // Raise ProductDepartmentId, ProductCategoryId and BaseUOMId to run filter
                SelectedProduct.RaiseFilterCollectionView();

                // Set parent resource
                StickyManagementViewModel.SetParentResource(SelectedProduct.Resource.ToString(), SelectedProduct.ResourceNoteCollection);

                // Show detail form
                IsSearchMode = false;
            }
            else if (IsSearchMode)
            {
                // Show detail form
                IsSearchMode = false;
            }
            else if (ShowNotification(null))
            {
                // Show list form
                IsSearchMode = true;
            }
        }

        #endregion

        #region LoadStepCommand

        /// <summary>
        /// Gets the LoadStepCommand command.
        /// </summary>
        public ICommand LoadStepCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadStepCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStepCommand command is executed.
        /// </summary>
        private void OnLoadStepCommandExecute()
        {
            // Load data by predicate
            LoadDataByPredicate(false, ProductCollection.Count);
        }

        #endregion

        #region PopupDepartmentCommand

        /// <summary>
        /// Gets the PopupDepartmentCommand command.
        /// </summary>
        public ICommand PopupDepartmentCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupDepartmentCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupDepartmentCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupDepartmentCommand command is executed.
        /// </summary>
        private void OnPopupDepartmentCommandExecute(object param)
        {
            // Get department type
            ProductDeparmentLevel departmentType = (ProductDeparmentLevel)param;

            // Get parentID
            int? parentID = null;
            switch (departmentType)
            {
                case ProductDeparmentLevel.Category:
                    if (SelectedProduct.ProductDepartmentId == 0)
                        return;
                    parentID = SelectedProduct.ProductDepartmentId;
                    break;
                case ProductDeparmentLevel.Brand:
                    if (SelectedProduct.ProductCategoryId == 0)
                        return;
                    parentID = SelectedProduct.ProductCategoryId;
                    break;
            }

            // Create popup deparment, category and brand viewmodel
            PopupDepartmentCategoryBrandViewModel viewModel = new PopupDepartmentCategoryBrandViewModel(departmentType, parentID);

            // Show dialog and get result when close popup
            bool? result = _dialogService.ShowDialog<PopupDepartmentCategoryBrandView>(_ownerViewModel, viewModel, string.Format("Create new {0}", departmentType));

            // Check result if ok button is clicked
            if (result.HasValue && result.Value && viewModel.NewItem != null)
            {
                switch (departmentType)
                {
                    case ProductDeparmentLevel.Department:
                        // Add new item to collection
                        DepartmentCollection.Add(viewModel.NewItem);

                        // Select new item
                        SelectedProduct.ProductDepartmentId = viewModel.NewItem.Id;
                        break;
                    case ProductDeparmentLevel.Category:
                        // Add new item to collection
                        CategoryCollection.Add(viewModel.NewItem);

                        // Select new item
                        SelectedProduct.ProductCategoryId = viewModel.NewItem.Id;
                        break;
                    case ProductDeparmentLevel.Brand:
                        // Add new item to collection
                        BrandCollection.Add(viewModel.NewItem);

                        // Select new item
                        SelectedProduct.ProductBrandId = viewModel.NewItem.Id;
                        break;
                }
            }
        }

        #endregion

        #region PopupVendorCommand

        /// <summary>
        /// Gets the PopupVendorCommand command.
        /// </summary>
        public ICommand PopupVendorCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupVendorCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupVendorCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupVendorCommand command is executed.
        /// </summary>
        private void OnPopupVendorCommandExecute(object param)
        {
            PopupGuestViewModel viewModel = new PopupGuestViewModel();
            bool? result = _dialogService.ShowDialog<PopupGuestView>(_ownerViewModel, viewModel, "Create new vendor");
            if (result.HasValue && result.Value && viewModel.NewItem != null)
            {
                // Add new vendor to list
                VendorCollection.Add(viewModel.NewItem);

                // Clone a new vendor item
                base_GuestModel newVendorModel = viewModel.NewItem.Clone();

                // Add new vendor to collection
                ProductVendorCollection.Add(newVendorModel);

                if (param == null)
                {
                    // Get vendor item
                    base_GuestModel vendorItem = ProductVendorCollection.SingleOrDefault(x => x.Id.Equals(SelectedProduct.VendorId));

                    // Visible old vendor item
                    if (vendorItem != null)
                        vendorItem.IsChecked = false;

                    // Hidden new vendor item
                    newVendorModel.IsChecked = true;

                    // Select new vendor
                    SelectedProduct.VendorId = viewModel.NewItem.Id;
                }
                else
                {
                    // Select new vendor
                    SelectedVendor = viewModel.NewItem;
                    SelectedVendorProduct = newVendorModel;
                }
            }
        }

        #endregion

        #region PopupUOMCommand

        /// <summary>
        /// Gets the PopupUOMCommand command.
        /// </summary>
        public ICommand PopupUOMCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupUOMCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupUOMCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupUOMCommand command is executed.
        /// </summary>
        private void OnPopupUOMCommandExecute(object param)
        {
            PopupUOMViewModel viewModel = new PopupUOMViewModel();
            bool? result = _dialogService.ShowDialog<PopupUOMView>(_ownerViewModel, viewModel, "Create new UOM");
            if (result.HasValue && result.Value && viewModel.UOMModel != null)
            {
                // Add new UOM to list
                UOMList.Add(new CheckBoxItemModel
                {
                    Value = viewModel.UOMModel.Id,
                    Text = viewModel.UOMModel.Name,
                });

                short uomType = 0;
                if (param != null && Int16.TryParse(param.ToString(), out uomType))
                    uomType = Int16.Parse(param.ToString());

                // Select new UOM
                if (uomType == 0)
                    SelectedProduct.BaseUOMId = viewModel.UOMModel.Id;
            }
        }

        #endregion

        #region PopupAttributeAndSizeCommand

        /// <summary>
        /// Gets the PopupAttributeAndSizeCommand command.
        /// </summary>
        public ICommand PopupAttributeAndSizeCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupAttributeAndSizeCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupAttributeAndSizeCommandCanExecute()
        {
            return IsValid;
        }

        /// <summary>
        /// Method to invoke when the PopupAttributeAndSizeCommand command is executed.
        /// </summary>
        private void OnPopupAttributeAndSizeCommandExecute()
        {
            if (SaveProduct(SelectedProduct))
            {
                if (SelectedProduct.ProductCollection == null)
                {
                    // Temp code, will deleted when clear data in database
                    if (SelectedProduct.GroupAttribute == null)
                        SelectedProduct.GroupAttribute = Guid.NewGuid();

                    // Load product collection that have same attribute group
                    LoadAttributeAndSize(SelectedProduct);
                }

                if (!CheckAttributeAndSize())
                    return;

                PopupAttributeAndSizeViewModel viewModel = new PopupAttributeAndSizeViewModel(SelectedProduct);
                bool? result = _dialogService.ShowDialog<PopupAttributeAndSizeView>(_ownerViewModel, viewModel, "Attribute and Size");
            }
        }

        #endregion

        #region PopupReorderPointCommand

        /// <summary>
        /// Gets the PopupReorderPointCommand command.
        /// </summary>
        public ICommand PopupReorderPointCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupReorderPointCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupReorderPointCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupReorderPointCommand command is executed.
        /// </summary>
        private void OnPopupReorderPointCommandExecute()
        {
            PopupReorderPointViewModel viewModel = new PopupReorderPointViewModel(SelectedProduct);
            bool? result = _dialogService.ShowDialog<PopupReorderPointView>(_ownerViewModel, viewModel, "Reorder Point");
            if (result.HasValue && result.Value)
            {
                foreach (base_ProductStoreModel productStoreItem in viewModel.ProductStoreCollection)
                {
                    // Check change
                    if (productStoreItem.IsDirty)
                    {
                        // Get product store by store code
                        base_ProductStoreModel productStoreModel = SelectedProduct.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(productStoreItem.StoreCode));

                        if (productStoreModel == null)
                        {
                            // Add new product store to collection
                            SelectedProduct.ProductStoreCollection.Add(productStoreItem);
                        }
                        else
                        {
                            // Update quantity in product store
                            productStoreModel.ReorderPoint = productStoreItem.ReorderPoint;
                        }
                    }
                }

                // Update company store in product
                SelectedProduct.CompanyReOrderPoint = viewModel.CompanyReorderPoint;
            }
        }

        #endregion

        #region PopupAvailableCommand

        /// <summary>
        /// Gets the PopupAvailableCommand command.
        /// </summary>
        public ICommand PopupAvailableCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupAvailableCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupAvailableCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupAvailableCommand command is executed.
        /// </summary>
        private void OnPopupAvailableCommandExecute()
        {
            PopupAvailableQuantitiesViewModel viewModel = new PopupAvailableQuantitiesViewModel(SelectedProduct);
            bool? result = _dialogService.ShowDialog<PopupAvailableQuantitiesView>(_ownerViewModel, viewModel, "Available Quantities");
            if (result.HasValue && result.Value)
            {
                foreach (base_ProductStoreModel productStoreItem in viewModel.ProductStoreCollection)
                {
                    // Check quantity on hand change
                    if (productStoreItem.QuantityOnHand != productStoreItem.OldQuantity)
                    {
                        // Get product store by store code
                        base_ProductStoreModel productStoreModel = SelectedProduct.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(productStoreItem.StoreCode));

                        if (productStoreModel == null)
                        {
                            // Add new product store to collection
                            SelectedProduct.ProductStoreCollection.Add(productStoreItem);
                        }
                        else
                        {
                            // Update quantity in product store
                            productStoreModel.QuantityOnHand = productStoreItem.QuantityOnHand;

                            // Update quantity available in product store
                            productStoreModel.QuantityAvailable = productStoreItem.QuantityAvailable;

                            if (productStoreItem.StoreCode.Equals(Define.StoreCode))
                            {
                                // Update quantity store in product
                                SelectedProduct.OnHandStore = productStoreItem.QuantityOnHand;
                            }
                            else
                            {
                                // Update quantity in product by store code
                                SelectedProduct.SetOnHandToStore(productStoreItem.QuantityOnHand, productStoreItem.StoreCode);
                            }
                        }
                    }
                }
            }
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
        private bool OnPopupPricingCommandCanExecute()
        {
            if (SelectedProduct == null)
                return false;
            return SelectedProduct.BaseUOMId > 0 ||
                (SelectedProduct.ProductUOMCollection != null && SelectedProduct.ProductUOMCollection.Count(x => x.UOMId > 0) > 0);
        }

        /// <summary>
        /// Method to invoke when the PopupPricingCommand command is executed.
        /// </summary>
        private void OnPopupPricingCommandExecute()
        {
            // Load PriceSchemas
            LoadPriceSchemas();

            PopupPricingViewModel viewModel = new PopupPricingViewModel(SelectedProduct, UOMList, PriceSchemaList);
            bool? result = _dialogService.ShowDialog<PopupPricingView>(_ownerViewModel, viewModel, "Pricing");
            if (result.HasValue && result.Value)
            {
                base_ProductUOMModel baseProductUOM = viewModel.ProductUOMList.FirstOrDefault(x => x.UOMId.Equals(SelectedProduct.BaseUOMId));
                if (baseProductUOM != null)
                    viewModel.ProductToProductUOM(SelectedProduct, baseProductUOM, true);

                if (SelectedProduct.ProductUOMCollection != null)
                    foreach (base_ProductUOMModel productUOMModel in SelectedProduct.ProductUOMCollection)
                    {
                        base_ProductUOMModel productUOMItem = viewModel.ProductUOMList.FirstOrDefault(x => x.UOMId.Equals(productUOMModel.UOMId));
                        if (productUOMItem != null)
                            productUOMModel.ToModel(productUOMItem);
                    }
            }
        }

        #endregion

        #region SellCommand

        /// <summary>
        /// Gets the SellCommand command.
        /// </summary>
        public ICommand SellCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SellCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSellCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count > 0 && UserPermissions.AllowSaleProduct;
        }

        /// <summary>
        /// Method to invoke when the SellCommand command is executed.
        /// </summary>
        private void OnSellCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            (_ownerViewModel as MainViewModel).OpenViewExecute("Sales Order", dataGridControl.SelectedItems.Cast<base_ProductModel>());
        }

        #endregion

        #region DuplicateCommand

        /// <summary>
        /// Gets the DuplicateCommand command.
        /// </summary>
        public ICommand DuplicateCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DuplicateCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDuplicateCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count == 1 && UserPermissions.AllowAddProduct;
        }

        /// <summary>
        /// Method to invoke when the DuplicateCommand command is executed.
        /// </summary>
        private void OnDuplicateCommandExecute(object param)
        {
            try
            {
                // Convert param to DataGridControl
                DataGridControl dataGridControl = param as DataGridControl;

                // Get selected item
                SelectedProduct = dataGridControl.SelectedItem as base_ProductModel;

                // Load photo collection
                LoadPhotoCollection(SelectedProduct);

                // Load product store collection
                LoadProductStoreCollection(SelectedProduct, Define.StoreCode);

                // Load product UOM collection
                //LoadProductUOMCollection(SelectedProduct, Define.StoreCode);

                // Load product group collection
                LoadProductGroupCollection(SelectedProduct);

                // Load vendor product collection
                LoadVendorProductCollection(SelectedProduct);

                PopupDuplicateItemViewModel viewModel = new PopupDuplicateItemViewModel(SelectedProduct, DepartmentCollection, CategoryCollection);
                bool? result = _dialogService.ShowDialog<PopupDuplicateItemView>(_ownerViewModel, viewModel, "Duplicate Item");

                SelectedProduct = null;

                if (result.HasValue && result.Value)
                {
                    if (viewModel.IsChangeInformation)
                    {
                        // Register property changed event to process filter category, brand by department
                        //viewModel.DuplicateProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

                        // Push new product to collection
                        ProductCollection.Insert(0, viewModel.DuplicateProduct);

                        // Update total products
                        TotalProducts++;
                    }
                    else
                    {
                        // Show detail selected product
                        OnDoubleClickViewCommandExecute(dataGridControl.SelectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region ViewVendorDetailCommand

        /// <summary>
        /// Gets the ViewVendorDetailCommand command.
        /// </summary>
        public ICommand ViewVendorDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ViewVendorDetailCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnViewVendorDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;

            return true;
        }

        /// <summary>
        /// Method to invoke when the ViewVendorDetailCommand command is executed.
        /// </summary>
        private void OnViewVendorDetailCommandExecute(object param)
        {
            // Convert param to vendor product model
            base_VendorProductModel vendorProductModel = param as base_VendorProductModel;

            // Open vendor view and view detail item
            (_ownerViewModel as MainViewModel).OpenViewExecute("Vendor", vendorProductModel.VendorResource);
        }

        #endregion

        #region DeleteVendorCommand

        /// <summary>
        /// Gets the DeleteVendorCommand command.
        /// </summary>
        public ICommand DeleteVendorCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteVendorCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteVendorCommandCanExecute(object param)
        {
            if (param == null)
                return false;

            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteVendorCommand command is executed.
        /// </summary>
        private void OnDeleteVendorCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                try
                {
                    // Convert param to vendor product model
                    base_VendorProductModel vendorProductModel = param as base_VendorProductModel;

                    // Remove vendor product from collection
                    SelectedProduct.VendorProductCollection.Remove(vendorProductModel);

                    // Get vendor item
                    base_GuestModel vendorItem = ProductVendorCollection.SingleOrDefault(x => x.Resource.Value.ToString().Equals(vendorProductModel.VendorResource));

                    // Visible vendor item
                    vendorItem.IsChecked = false;
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            }
        }

        #endregion

        #region DeleteSubProductCommand

        /// <summary>
        /// Gets the DeleteSubProductCommand command.
        /// </summary>
        public ICommand DeleteSubProductCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteSubProductCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSubProductCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the DeleteSubProductCommand command is executed.
        /// </summary>
        private void OnDeleteSubProductCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                base_ProductGroupModel productGroupModel = param as base_ProductGroupModel;
                //productGroupModel.IsChecked = true;

                //if (ProductSubCollection.DeletedItems != null)
                //{
                //    base_ProductModel productModel = ProductSubCollection.DeletedItems.SingleOrDefault(x => x.Id.Equals(productGroupModel.ProductId));

                //    // Add product item to collection
                //    ProductSubCollection.Add(productModel);

                //    // Remove product item in DeletedItems collection
                //    ProductSubCollection.DeletedItems.Remove(productModel);
                //}

                //// Turn off IsNew to push item into DeletedItems collecion
                //productGroupModel.IsNew = false;

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
                foreach (base_ProductModel productItem in viewModel.SelectedProducts)
                {
                    // Get uom name for product
                    if (string.IsNullOrWhiteSpace(productItem.UOMName))
                    {
                        CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productItem.BaseUOMId));
                        if (uomItem != null)
                            productItem.UOMName = uomItem.Text;
                    }

                    OnSelectedSubProductChanged(productItem);
                }
                //SelectedSubProduct = viewModel.SelectedProducts.FirstOrDefault();
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
            try
            {
                long parentID = SelectedProduct.Id;
                short itemTypeID = (short)ItemTypes.Group;

                // Initial predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Default condition
                predicate = predicate.And(x => x.IsPurge == false && !x.Id.Equals(parentID) && x.ItemTypeId != itemTypeID);

                // Get product have the same barcode
                predicate = predicate.And(x => x.Barcode.Equals(BarcodeProduct));

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

                    //SelectedSubProduct = productModel;
                    OnSelectedSubProductChanged(productModel);
                }

                // Clear barcode
                BarcodeProduct = string.Empty;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            try
            {
                // Load ComboBox ItemsSource
                // Load department, category and brand list
                if (DepartmentCollection == null)
                {
                    IEnumerable<base_DepartmentModel> departments = _departmentRepository.
                        GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                        OrderBy(x => x.Name).Select(x => new base_DepartmentModel(x, false));

                    DepartmentCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 0));
                    CategoryCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 1));
                    BrandCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 2));

                    // Initial category and brand collection view
                    _categoryCollectionView = CollectionViewSource.GetDefaultView(CategoryCollection);
                    _brandCollectionView = CollectionViewSource.GetDefaultView(BrandCollection);
                }

                // Load vendor list
                if (VendorCollection == null)
                {
                    string vendorType = MarkType.Vendor.ToDescription();
                    VendorCollection = new ObservableCollection<base_GuestModel>(_guestRepository.
                        GetAll(x => x.Mark.Equals(vendorType) && x.IsActived && !x.IsPurged).
                        OrderBy(x => x.Company).
                        Select(x => new base_GuestModel(x, false)));

                    // Load product vendor collection
                    ProductVendorCollection = new ObservableCollection<base_GuestModel>(VendorCollection.CloneList());
                }

                // Load UOM list
                if (UOMList == null)
                {
                    UOMList = new ObservableCollection<CheckBoxItemModel>(_uomRepository.GetIQueryable(x => x.IsActived).
                            OrderBy(x => x.Name).Select(x => new CheckBoxItemModel { Value = x.Id, Text = x.Name }));
                    UOMList.Insert(0, new CheckBoxItemModel { Value = 0, Text = string.Empty });
                }

                // Load sale tax location list
                if (SaleTaxLocationList == null)
                {
                    base_SaleTaxLocation taxLocationPrimary = _taxLocationRepository.Get(x => x.ParentId == 0 && x.IsPrimary);
                    if (taxLocationPrimary != null)
                    {
                        SaleTaxLocationList = new List<string>(_taxLocationRepository.
                            GetIQueryable(x => x.ParentId > 0 && x.ParentId.Equals(taxLocationPrimary.Id)).
                            OrderBy(x => x.TaxCode).
                            Select(x => x.TaxCode));
                    }
                }

                // Load PriceSchemas
                LoadPriceSchemas();

                // Load ItemType
                ItemTypeList = new ObservableCollection<ComboItem>(Common.ItemTypes.Where(x => x.Flag));

                // Initial collection for search products
                ProductFieldCollection = new DataSearchCollection();
                ProductFieldCollection.Add(new DataSearchModel { ID = 1, Level = 0, DisplayName = "Code", KeyName = "Code" });
                ProductFieldCollection.Add(new DataSearchModel { ID = 2, Level = 0, DisplayName = "Barcode", KeyName = "Barcode" });
                ProductFieldCollection.Add(new DataSearchModel { ID = 3, Level = 0, DisplayName = "Product Name", KeyName = "ProductName" });
                ProductFieldCollection.Add(new DataSearchModel { ID = 4, Level = 0, DisplayName = "Attribute", KeyName = "Attribute" });
                ProductFieldCollection.Add(new DataSearchModel { ID = 6, Level = 0, DisplayName = "Size", KeyName = "Size" });

                //// Initial product collection
                //ProductSubCollection = new CollectionBase<base_ProductModel>();

                //// Get all product
                //IEnumerable<base_Product> products = _productRepository.
                //    GetAll(x => x.IsPurge == false && x.ItemTypeId != _itemTypeID).OrderBy(x => x.Id);
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
                //    ProductSubCollection.Add(productModel);
                //}
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            PopupAdvanceSearchCommand = new RelayCommand<object>(OnPopupAdvanceSearchCommandExecute, OnPopupAdvanceSearchCommandCanExecute);
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            LoadStepCommand = new RelayCommand(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            PopupDepartmentCommand = new RelayCommand<object>(OnPopupDepartmentCommandExecute, OnPopupDepartmentCommandCanExecute);
            PopupVendorCommand = new RelayCommand<object>(OnPopupVendorCommandExecute, OnPopupVendorCommandCanExecute);
            PopupUOMCommand = new RelayCommand<object>(OnPopupUOMCommandExecute, OnPopupUOMCommandCanExecute);
            PopupAttributeAndSizeCommand = new RelayCommand(OnPopupAttributeAndSizeCommandExecute, OnPopupAttributeAndSizeCommandCanExecute);
            PopupReorderPointCommand = new RelayCommand(OnPopupReorderPointCommandExecute, OnPopupReorderPointCommandCanExecute);
            PopupAvailableCommand = new RelayCommand(OnPopupAvailableCommandExecute, OnPopupAvailableCommandCanExecute);
            PopupPricingCommand = new RelayCommand(OnPopupPricingCommandExecute, OnPopupPricingCommandCanExecute);
            SellCommand = new RelayCommand<object>(OnSellCommandExecute, OnSellCommandCanExecute);
            DuplicateCommand = new RelayCommand<object>(OnDuplicateCommandExecute, OnDuplicateCommandCanExecute);
            ViewVendorDetailCommand = new RelayCommand<object>(OnViewVendorDetailCommandExecute, OnViewVendorDetailCommandCanExecute);
            DeleteVendorCommand = new RelayCommand<object>(OnDeleteVendorCommandExecute, OnDeleteVendorCommandCanExecute);
            DeleteSubProductCommand = new RelayCommand<object>(OnDeleteSubProductCommandExecute, OnDeleteSubProductCommandCanExecute);
            SearchProductAdvanceCommand = new RelayCommand(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
            SearchProductCommand = new RelayCommand(OnSearchProductCommandExecute, OnSearchProductCommandCanExecute);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            if (SelectedProduct == null)
                return false;

            // Initial variable to check IsDirty of product store collection
            bool productStoreCollectionIsDirty = false;
            if (SelectedProduct.ProductStoreCollection != null)
            {
                if (SelectedProduct.ProductStoreCollection.Any(x => x.IsNew))
                {
                    // Check IsDirty of product store collection
                    productStoreCollectionIsDirty = SelectedProduct.ProductStoreCollection.Count(x => x.IsNew && x.IsDirty) > 0;
                }
                else
                {
                    // Check IsDirty of product store collection
                    productStoreCollectionIsDirty = SelectedProduct.ProductStoreCollection.IsDirty;
                }
            }

            return SelectedProduct.IsDirty || productStoreCollectionIsDirty ||
                (SelectedProduct.PhotoCollection != null && SelectedProduct.PhotoCollection.IsDirty) ||
                (SelectedProduct.ProductCollection != null && SelectedProduct.ProductCollection.IsDirty) ||
                (SelectedProduct.VendorProductCollection != null && SelectedProduct.VendorProductCollection.IsDirty) ||
                (SelectedProduct.ProductGroupCollection != null && SelectedProduct.ProductGroupCollection.IsDirty);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <param name="isClosing">
        /// true if form is closing;
        /// false if form is changing;
        /// null if switch change search mode
        /// </param>
        /// <returns>true if continue action; otherwise, false.</returns>
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;

            // Check data is edited
            if (IsEdit())
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (msgResult.Is(MessageBoxResult.Cancel))
                {
                    return false;
                }
                else if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        result = SaveProduct(SelectedProduct);
                    }
                    else
                    {
                        result = false;
                    }

                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
                else
                {
                    if (SelectedProduct.IsNew)
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();

                        SelectedProduct = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else
                    {
                        // Rollback data
                        SelectedProduct.PhotoCollection = null;
                        SelectedProduct.ProductCollection = null;
                        SelectedProduct.ProductStoreDefault = null;
                        SelectedProduct.ProductStoreCollection = null;
                        SelectedProduct.ProductGroupCollection = null;
                        SelectedProduct.VendorProductCollection = null;
                        _oldItemTypeID = 0;
                        SelectedProduct.ToModelAndRaise();
                        SelectedProduct.EndUpdate();

                        // Close all popup sticky
                        StickyManagementViewModel.CloseAllPopupSticky();
                    }
                }
            }
            else
            {
                if (SelectedProduct != null && SelectedProduct.IsNew)
                {
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();
                }
                else
                {
                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
            }

            if (SelectedProduct != null && !SelectedProduct.IsNew)
            {
                // Register property changed event to process filter category, brand by department
                SelectedProduct.PropertyChanged -= new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);
            }

            // Clear selected item
            if (result && isClosing == null && SelectedProduct != null)
                SelectedProduct = null;

            return result;
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();

                predicate = PredicateBuilder.False<base_Product>();

                if (ColumnCollection.Count > 0)
                {
                    if (ColumnCollection.Contains(SearchOptions.Code.ToString()))
                    {
                        // Get all products that Code contain keyword
                        predicate = predicate.Or(x => x.Code.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Type.ToString()))
                    {
                        // Get all item types contain keyword
                        IEnumerable<ComboItem> itemTypes = Common.ItemTypes.Where(x => x.Text.ToLower().Contains(keyword));
                        IEnumerable<short> itemTypeIDList = itemTypes.Select(x => x.Value);

                        // Get all products that ItemType contain keyword
                        if (itemTypeIDList.Count() > 0)
                            predicate = predicate.Or(x => itemTypeIDList.Contains(x.ItemTypeId));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Category.ToString()))
                    {
                        // Get all categories contain keyword
                        IEnumerable<base_DepartmentModel> categories = CategoryCollection.Where(x => x.Name.ToLower().Contains(keyword));
                        IEnumerable<int> categoryIDList = categories.Select(x => x.Id);

                        // Get all products that Category contain keyword
                        if (categoryIDList.Count() > 0)
                            predicate = predicate.Or(x => categoryIDList.Contains(x.ProductCategoryId));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Vendor.ToString()))
                    {
                        // Get all vendors contain keyword
                        IEnumerable<base_GuestModel> vendors = VendorCollection.Where(x => x.Company.ToLower().Contains(keyword));
                        IEnumerable<long> vendorIDList = vendors.Select(x => x.Id);

                        // Get all products that Vendor contain keyword
                        if (vendorIDList.Count() > 0)
                            predicate = predicate.Or(x => vendorIDList.Contains(x.VendorId));
                    }
                    if (ColumnCollection.Contains(SearchOptions.ItemName.ToString()))
                    {
                        // Get all products that ProductName contain keyword
                        predicate = predicate.Or(x => x.ProductName.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Description.ToString()))
                    {
                        // Get all products that Description contain keyword
                        predicate = predicate.Or(x => x.Description.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Attribute.ToString()))
                    {
                        // Get all products that Attribute contain keyword
                        predicate = predicate.Or(x => x.Attribute.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Size.ToString()))
                    {
                        // Get all products that Size contain keyword
                        predicate = predicate.Or(x => x.Size.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.Barcode.ToString()))
                    {
                        // Get all products that Barcode contain keyword
                        predicate = predicate.Or(x => x.Barcode.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.ALU.ToString()))
                    {
                        // Get all products that ALU contain keyword
                        predicate = predicate.Or(x => x.ALU.ToLower().Contains(keyword));
                    }
                    if (ColumnCollection.Contains(SearchOptions.UOM.ToString()))
                    {
                        // Get all unit of measures contain keyword
                        IEnumerable<CheckBoxItemModel> uomItems = UOMList.Where(x => x.Text.ToLower().Contains(keyword));
                        IEnumerable<int> uomIDList = uomItems.Select(x => x.Value);

                        // Get all products that UOM contain keyword
                        predicate = predicate.Or(x => uomIDList.Contains(x.BaseUOMId));
                    }
                    if (ColumnCollection.Contains(SearchOptions.TaxCode.ToString()))
                    {
                        // Get all products that TaxCode contain keyword
                        predicate = predicate.Or(x => x.TaxCode.ToLower().Contains(keyword));
                    }

                    // Parse keyword to Decimal
                    decimal decimalKeyword = 0;
                    if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalKeyword) && decimalKeyword != 0)
                    {
                        if (ColumnCollection.Contains(SearchOptions.RegularPrice.ToString()))
                        {
                            // Get all products that RegularPrice contain keyword
                            predicate = predicate.Or(x => x.RegularPrice.Equals(decimalKeyword));
                        }
                        if (ColumnCollection.Contains(SearchOptions.QuantityOnHand.ToString()))
                        {
                            // Get all products that QuantityOnHand contain keyword
                            predicate = predicate.Or(x => x.QuantityOnHand.Equals(decimalKeyword));
                        }
                        if (ColumnCollection.Contains(SearchOptions.QuantityOnOrder.ToString()))
                        {
                            // Get all products that QuantityOnOrder contain keyword
                            predicate = predicate.Or(x => x.QuantityOnOrder.Equals(decimalKeyword));
                        }
                    }

                    // Parse keyword to DateTime
                    DateTime dateTimeKeyword = DateTimeExt.Now;
                    if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeKeyword))
                    {
                        int year = dateTimeKeyword.Year;
                        int month = dateTimeKeyword.Month;
                        int day = dateTimeKeyword.Day;

                        if (ColumnCollection.Contains(SearchOptions.DateCreated.ToString()))
                        {
                            // Get all products that DateCreated contain keyword
                            predicate = predicate.Or(x => x.DateCreated.Year.Equals(year) &&
                                x.DateCreated.Month.Equals(month) && x.DateCreated.Day.Equals(day));
                        }
                    }
                }
            }

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false && x.ItemTypeId == _itemTypeID);

            if (!IsMainStore)
            {
                // Get all product by store code
                predicate = predicate.And(x => x.base_ProductStore.Any(y => y.StoreCode.Equals(Define.StoreCode)));
            }

            return predicate;
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Product, bool>> predicate = CreateSearchPredicate(_keyword);

            // Load data by predicate
            LoadDataByPredicate(predicate, refreshData, currentIndex);
        }

        /// <summary>
        /// Get all datas from database by predicate
        /// </summary>
        /// <param name="predicate">Condition for load data</param>
        /// <param name="refreshData">Refresh data. Default is False</param>
        /// <param name="currentIndex">Load data from index. If index = 0, clear collection. Default is 0</param>
        private void LoadDataByPredicate(Expression<Func<base_Product, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy) return;

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                ProductCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                try
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    if (refreshData)
                    {
                        // Refresh allow multi UOM
                        OnPropertyChanged(() => AllowMutilUOM);
                    }

                    // Get total products with condition in predicate
                    TotalProducts = _productRepository.GetIQueryable(predicate).Count();

                    // Get data with range
                    IList<base_Product> products = _productRepository.GetRangeDescending(currentIndex, NumberOfDisplayItems, x => x.DateCreated, predicate);
                    foreach (base_Product product in products)
                    {
                        bgWorker.ReportProgress(0, product);
                    }
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create product model
                base_ProductModel productModel = new base_ProductModel((base_Product)e.UserState);

                // Load relation data
                LoadRelationData(productModel);

                // Add to collection
                ProductCollection.Add(productModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for product
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationData(base_ProductModel productModel)
        {
            // Update ItemType
            productModel.ItemTypeItem = Common.ItemTypes.SingleOrDefault(x => x.Value.Equals(productModel.ItemTypeId));

            // Get category name for product
            if (string.IsNullOrWhiteSpace(productModel.CategoryName))
            {
                base_DepartmentModel categoryItem = CategoryCollection.FirstOrDefault(x => x.Id.Equals(productModel.ProductCategoryId));
                if (categoryItem != null)
                    productModel.CategoryName = categoryItem.Name;
            }

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

            // Load resource note collection
            LoadResourceNoteCollection(productModel);

            // Turn off IsDirty & IsNew
            productModel.EndUpdate();
        }

        /// <summary>
        /// Load price schemas
        /// </summary>
        private void LoadPriceSchemas()
        {
            try
            {
                Common.Refresh();

                PriceSchemaList = new List<PriceModel>();

                using (Stream stream = Common.LoadCurrentLanguagePackage())
                {
                    if (stream == null)
                        return;

                    XDocument xDocument = XDocument.Load(stream);

                    string comboElementName = "combo";
                    string keyAttributeName = "key";
                    string keyElementName = "PriceSchemas";
                    string valueElementName = "value";
                    string nameElementName = "name";
                    string markdownElementName = "markDown";

                    // Get all elements type is combo
                    IEnumerable<XElement> xCombos = xDocument.Root.Elements(comboElementName);

                    if (xCombos != null)
                    {
                        // Get element have attribute is key and value is PriceSchemas
                        XElement xPriceSchemas = xCombos.SingleOrDefault(
                            x => x.Attribute(keyAttributeName) != null && x.Attribute(keyAttributeName).Value.Equals(keyElementName));

                        if (xPriceSchemas != null)
                        {
                            foreach (XElement xPriceSchema in xPriceSchemas.Elements())
                            {
                                // Create new price model
                                PriceModel priceModel = new PriceModel();

                                // Set ID value
                                short value = 0;
                                XElement xValue = xPriceSchema.Element(valueElementName);
                                if (xValue != null && Int16.TryParse(xValue.Value, out value))
                                    priceModel.Id = Int16.Parse(xValue.Value);

                                // Set name value
                                XElement xName = xPriceSchema.Element(nameElementName);
                                if (xName != null)
                                    priceModel.Name = xName.Value;

                                // Set markdown value
                                decimal markdown = 0;
                                XElement xMarkdown = xPriceSchema.Element(markdownElementName);
                                if (xMarkdown != null && decimal.TryParse(xMarkdown.Value, out markdown))
                                    priceModel.MarkDown = decimal.Parse(xMarkdown.Value);

                                if (!Define.CONFIGURATION.DefaultPriceSchema.HasValue || !priceModel.Id.Equals(Define.CONFIGURATION.DefaultPriceSchema.Value))
                                    // Push price model to list
                                    PriceSchemaList.Add(priceModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
        /// Load all product that have same attribute group
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadAttributeAndSize(base_ProductModel productModel)
        {
            try
            {
                if (productModel.ProductCollection == null)
                {
                    // Initial product collection
                    productModel.ProductCollection = new CollectionBase<base_ProductModel>();

                    // Initial predicate
                    Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                    // Default condition
                    predicate = predicate.And(x => x.IsPurge == false && !x.Code.Equals(productModel.Code));
                    predicate = predicate.And(x => x.GroupAttribute == productModel.GroupAttribute);

                    // Load product collection that have same attribute group
                    IEnumerable<base_Product> products = _productRepository.GetAll(predicate);
                    foreach (base_Product product in products)
                    {
                        // Create new product model
                        base_ProductModel productItem = new base_ProductModel(product);

                        // Load product store collection
                        LoadProductStoreCollection(productItem, Define.StoreCode);

                        // Load product UOM collection
                        //LoadProductUOMCollection(productItem, Define.StoreCode);

                        // Add new product to collection
                        productModel.ProductCollection.Add(productItem);
                    }

                    productModel.ProductCollection.Insert(0, productModel);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load product store collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadProductStoreCollection(base_ProductModel productModel, int storeCode)
        {
            try
            {
                if (productModel.ProductStoreCollection == null)
                {
                    productModel.ProductStoreCollection = new CollectionBase<base_ProductStoreModel>(
                        productModel.base_Product.base_ProductStore.Select(x => new base_ProductStoreModel(x) { OldQuantity = x.QuantityOnHand }));

                    // Get product store default by store code
                    base_ProductStoreModel productStoreDefault = productModel.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(storeCode));

                    if (productStoreDefault == null)
                    {
                        // Create new product store default
                        productStoreDefault = new base_ProductStoreModel { StoreCode = storeCode };
                        productStoreDefault.Resource = Guid.NewGuid().ToString();
                        productStoreDefault.ProductResource = productModel.Resource.ToString();

                        // Add new product store to collection
                        productModel.ProductStoreCollection.Add(productStoreDefault);
                    }

                    if (productModel.ProductStoreDefault == null)
                    {
                        // Update product store default
                        productModel.ProductStoreDefault = productStoreDefault;
                    }

                    // Get quantity for product
                    productModel.OnHandStore = productModel.ProductStoreDefault.QuantityOnHand;

                    // Update available quantity
                    //productModel.ProductStoreDefault.UpdateAvailableQuantity();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load sale order collection
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="storeCode"></param>
        private void LoadSaleOrderCollection(base_ProductModel productModel, int storeCode)
        {
            try
            {
                if (productModel.SaleOrderCollection == null)
                {
                    string productResource = productModel.Resource.ToString();

                    // Get all sale order that sold this product
                    productModel.SaleOrderCollection = new ObservableCollection<base_SaleOrderModel>(_saleOrderDetailRepository.
                        GetAll(x => x.ProductResource.Equals(productResource) && x.base_SaleOrder.StoreCode.Equals(storeCode)).
                        OrderByDescending(x => x.base_SaleOrder.DateCreated).
                        Select(x => new base_SaleOrderModel(x.base_SaleOrder)));

                    // Get total sale order for this product
                    TotalSaleOrder = new base_SaleOrderModel
                    {
                        Total = productModel.SaleOrderCollection.Sum(x => x.Total),
                        Paid = productModel.SaleOrderCollection.Sum(x => x.Paid),
                        Balance = productModel.SaleOrderCollection.Sum(x => x.Balance)
                    };
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load purchase order collection
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="storeCode"></param>
        private void LoadPurchaseOrderCollection(base_ProductModel productModel, int storeCode)
        {
            try
            {
                if (productModel.PurchaseOrderCollection == null)
                {
                    string productResource = productModel.Resource.ToString();

                    // Get all purchase order that sold this product
                    productModel.PurchaseOrderCollection = new ObservableCollection<base_PurchaseOrderModel>(_purchaseOrderDetailRepository.
                        GetAll(x => x.ProductResource.Equals(productResource) && x.base_PurchaseOrder.StoreCode.Equals(storeCode)).
                        OrderByDescending(x => x.base_PurchaseOrder.DateCreated).
                        Select(x => new base_PurchaseOrderModel(x.base_PurchaseOrder)));

                    // Get total purchase order for this product
                    TotalPurchaseOrder = new base_PurchaseOrderModel
                    {
                        Total = productModel.PurchaseOrderCollection.Sum(x => x.Total),
                        Paid = productModel.PurchaseOrderCollection.Sum(x => x.Paid),
                        Balance = productModel.PurchaseOrderCollection.Sum(x => x.Balance)
                    };
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Load all product that have same parent ID
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadProductGroupCollection(base_ProductModel productModel)
        {
            try
            {
                if (productModel.ProductGroupCollection == null)
                {
                    // Initial product collection
                    productModel.ProductGroupCollection = new CollectionBase<base_ProductGroupModel>();

                    // Get all product group
                    IEnumerable<base_ProductGroup> productGroups = productModel.base_Product.base_ProductGroup1.
                        Where(x => x.base_Product.IsPurge != true);

                    foreach (base_ProductGroup productGroup in productGroups)
                    {
                        // Create new product group model
                        base_ProductGroupModel productGroupModel = new base_ProductGroupModel(productGroup);

                        // Get product store of product group
                        base_ProductStore productStore = productGroupModel.base_ProductGroup.base_Product.base_ProductStore.
                            SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));

                        // Load product UOM collection for product group
                        LoadProductUOMCollectionForGroup(productGroupModel, productStore);

                        // Register property changed event
                        productGroupModel.PropertyChanged += new PropertyChangedEventHandler(productGroupModel_PropertyChanged);

                        // Add new product group to collection
                        productModel.ProductGroupCollection.Add(productGroupModel);

                        //// Get product item exist in product group collection
                        //base_ProductModel productItem = ProductSubCollection.SingleOrDefault(x => x.Id.Equals(productGroupModel.ProductId));

                        //// Remove product item exist in product group collection
                        //ProductSubCollection.Remove(productItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        private void NewProduct()
        {
            // Reset UOMList
            foreach (CheckBoxItemModel uomItem in UOMList)
                uomItem.IsChecked = false;

            // Reset product vendor collection
            foreach (base_GuestModel vendorItem in ProductVendorCollection)
                vendorItem.IsChecked = false;

            // Create a new product with default values
            SelectedProduct = new base_ProductModel
            {
                ProductDepartmentId = -1,
                ProductCategoryId = -1,
            };

            // Register property changed event to process filter category, brand by department
            SelectedProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

            // Set default ItemType is Group
            SelectedProduct.ItemTypeId = (short)ItemTypes.Group;

            // Set default BaseUOMID
            if (UOMList.Count > 0)
                SelectedProduct.BaseUOMId = UOMList.FirstOrDefault().Value;

            // Set default TaxCode
            if (SaleTaxLocationList.Count > 0)
                SelectedProduct.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;

            // Get default markdown and update price
            GetDefaultMarkdown(SelectedProduct);

            SelectedProduct.Code = GenProductCode();
            SelectedProduct.IsPurge = false;
            SelectedProduct.ProductDepartmentId = 0;
            SelectedProduct.ProductCategoryId = 0;
            SelectedProduct.ProductBrandId = 0;
            SelectedProduct.Barcode = string.Empty;
            SelectedProduct.PartNumber = string.Empty;
            SelectedProduct.StyleModel = string.Empty;
            SelectedProduct.Attribute = string.Empty;
            SelectedProduct.Size = string.Empty;
            SelectedProduct.Description = string.Empty;
            SelectedProduct.DateCreated = DateTimeExt.Now;
            if (Define.USER != null)
                SelectedProduct.UserCreated = Define.USER.LoginName;
            SelectedProduct.Resource = Guid.NewGuid();
            SelectedProduct.GroupAttribute = Guid.NewGuid();
            if (UOMList.Count > 1)
                SelectedProduct.BaseUOMId = UOMList.FirstOrDefault(x => x.Value > 0).Value;

            SelectedProduct.Custom1 = string.Empty;
            SelectedProduct.Custom2 = string.Empty;
            SelectedProduct.Custom3 = string.Empty;
            SelectedProduct.Custom4 = string.Empty;
            SelectedProduct.Custom5 = string.Empty;
            SelectedProduct.Custom6 = string.Empty;
            SelectedProduct.Custom7 = string.Empty;

            SelectedProduct.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            SelectedProduct.VendorProductCollection = new CollectionBase<base_VendorProductModel>();
            SelectedProduct.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            StickyManagementViewModel.SetParentResource(SelectedProduct.Resource.ToString(), SelectedProduct.ResourceNoteCollection);

            // Initial product group collection
            SelectedProduct.ProductGroupCollection = new CollectionBase<base_ProductGroupModel>();

            // Initial product store collection
            SelectedProduct.ProductStoreCollection = new CollectionBase<base_ProductStoreModel>();

            // Create new product store default
            SelectedProduct.ProductStoreDefault = new base_ProductStoreModel { StoreCode = Define.StoreCode };
            SelectedProduct.ProductStoreDefault.Resource = Guid.NewGuid().ToString();
            SelectedProduct.ProductStoreDefault.ProductResource = SelectedProduct.Resource.ToString();

            // Add new product store default to collection
            SelectedProduct.ProductStoreCollection.Add(SelectedProduct.ProductStoreDefault);

            // Turn off IsDirty
            SelectedProduct.IsDirty = false;
            SelectedProduct.ProductStoreDefault.IsDirty = false;

            // Raise permissions
            OnPropertyChanged(() => AllowEditQuantity);

            FocusDefault = false;
            FocusDefault = true;
        }

        /// <summary>
        /// Process save product function
        /// </summary>
        /// <returns></returns>
        private bool SaveProduct(base_ProductModel productModel)
        {
            // Check duplicate code
            if (IsDuplicateCode(productModel))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Code of this product is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check duplicate product
            if (IsDuplicateProduct(productModel))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("This product is duplicated", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check duplicate barcode
            if (IsDuplicateBarcode(productModel))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Barcode of this product is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check duplicate alternate barcode
            if (IsDuplicateALU(productModel))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("ALU of this product is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check RegularPrice
            if (Total != SelectedProduct.RegularPrice && SelectedProduct.ProductGroupCollection.IsDirty)
            {
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Would you like to update the price to equal the sum of the new component prices?", "POS", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.OK))
                {
                    SelectedProduct.RegularPrice = Total;
                }
            }

            if (productModel.IsNew)
            {
                // Save product when created new
                SaveNew(productModel);
            }
            else
            {
                // Save product when edited
                SaveUpdate(productModel);

                // Save adjustment
                SaveAdjustment(productModel);
            }

            // Update old cost
            productModel.OldCost = productModel.AverageUnitCost;

            // Update default photo
            productModel.PhotoDefault = productModel.PhotoCollection.FirstOrDefault();

            base_ProductModel productItem = ProductCollection.SingleOrDefault(x => x.Resource.Equals(productModel.Resource));
            if (productItem != null)
            {
                // Refresh product
                productItem.ToModelAndRaise();

                // Clear relation data to refresh
                productItem.CategoryName = string.Empty;
                productItem.VendorName = string.Empty;
                productItem.UOMName = string.Empty;

                // Refresh relation data
                LoadRelationData(productItem);
            }

            // Turn off IsDirty & IsNew
            productModel.EndUpdate();

            return true;
        }

        /// <summary>
        /// Save when create new product
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveNew(base_ProductModel productModel)
        {
            try
            {
                // Set shift
                productModel.Shift = Define.ShiftCode;

                // Map data from model to entity
                productModel.ToEntity();

                // Save photo collection
                SavePhotoCollection(productModel);

                // Save product store collection
                SaveProductStoreCollection(productModel);

                // Save product group collection
                SaveProductGroupCollection(productModel);

                // Save vendor product collection
                SaveVendorProductCollection(productModel);

                // Add new product to repository
                _productRepository.Add(SelectedProduct.base_Product);

                // Accept changes
                _productRepository.Commit();

                // Update ID from entity to model
                productModel.Id = productModel.base_Product.Id;

                // Update product store id
                UpdateProductStoreID(productModel);

                // Update product group id
                UpdateProductGroupID(productModel);

                // Update vendor product id
                UpdateVendorProductID(productModel);

                // Push new product to collection
                ProductCollection.Insert(0, productModel);

                // Update total products
                TotalProducts++;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save when edit or update product
        /// </summary>
        private void SaveUpdate(base_ProductModel productModel)
        {
            try
            {
                productModel.DateUpdated = DateTimeExt.Now;
                if (Define.USER != null)
                    productModel.UserUpdated = Define.USER.LoginName;

                // Save product store collection
                SaveProductStoreCollection(productModel);

                // Map data from model to entity
                productModel.ToEntity();

                // Save photo collection
                SavePhotoCollection(productModel);

                // Save product collection have same group attribute
                SaveAttributeAndSize(productModel);

                // Save product group collection
                SaveProductGroupCollection(productModel);

                // Save vendor product collection
                SaveVendorProductCollection(productModel);

                // Accept changes
                _productRepository.Commit();

                // Update product store id
                UpdateProductStoreID(productModel);

                // Update product id have same group attribute
                UpdateAttributeAndSize(productModel);

                // Update product group id
                UpdateProductGroupID(productModel);

                // Update vendor product id
                UpdateVendorProductID(productModel);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save photo collection
        /// </summary>
        private void SavePhotoCollection(base_ProductModel productModel)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save product collection have same attribute group when update product
        /// </summary>
        private void SaveAttributeAndSize(base_ProductModel productModel)
        {
            try
            {
                if (productModel.ProductCollection != null)
                {
                    foreach (base_ProductModel productItem in productModel.ProductCollection.DeletedItems)
                    {
                        // Turn on IsPurge to delete product
                        productItem.IsPurge = true;

                        // Map data from model to entity
                        productItem.ToEntity();
                    }

                    foreach (base_ProductModel productItem in productModel.ProductCollection.Where(x => !x.Code.Equals(productModel.Code)))
                    {
                        if (productItem.IsNew)
                        {
                            // Copy product and relation data
                            productItem.ToModelByAttribute(SelectedProduct);
                            productItem.DateCreated = DateTimeExt.Now;
                            productItem.Shift = Define.ShiftCode;
                            productItem.IsPurge = false;

                            CopyProductStoreDefault(productModel, productItem);
                        }
                        else
                            productItem.DateUpdated = productModel.DateUpdated;

                        // Update total quantity on product
                        productItem.UpdateQuantityOnHand();

                        // Update total available quantity on product
                        productModel.UpdateAvailableQuantity();

                        // Save product store collection
                        SaveProductStoreCollection(productItem);

                        // Map data from model to entity
                        productItem.ToEntity();

                        if (productItem.IsNew)
                        {
                            // Add new product to database
                            _productRepository.Add(productItem.base_Product);
                        }
                        else
                        {
                            // Turn off IsDirty & IsNew
                            productItem.EndUpdate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save product store collection
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="storeCode"></param>
        private void SaveProductStoreCollection(base_ProductModel productModel)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save product group collection
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveProductGroupCollection(base_ProductModel productModel)
        {
            try
            {
                if (productModel.ProductGroupCollection != null)
                {
                    if (productModel.ProductGroupCollection.DeletedItems != null)
                    {
                        foreach (base_ProductGroupModel productGroupItem in productModel.ProductGroupCollection.DeletedItems)
                        {
                            // Remove product group from database
                            _productGroupRepository.Delete(productGroupItem.base_ProductGroup);
                        }

                        productModel.ProductGroupCollection.DeletedItems.Clear();
                    }

                    foreach (base_ProductGroupModel productGroupItem in productModel.ProductGroupCollection)
                    {
                        // Map data from model to entity
                        productGroupItem.ToEntity();

                        if (productGroupItem.IsNew)
                        {
                            // Add new product group to database
                            productModel.base_Product.base_ProductGroup1.Add(productGroupItem.base_ProductGroup);
                        }
                        else
                        {
                            // Turn off IsDirty & IsNew
                            productGroupItem.EndUpdate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save vendor product collection
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveVendorProductCollection(base_ProductModel productModel)
        {
            try
            {
                if (productModel.VendorProductCollection != null)
                {
                    foreach (base_VendorProductModel vendorProductItem in productModel.VendorProductCollection.DeletedItems)
                    {
                        // Delete vendor product from database
                        _vendorProductRepository.Delete(vendorProductItem.base_VendorProduct);
                    }

                    // Clear deleted vendor products
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Update product store id
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="storeCode"></param>
        private void UpdateProductStoreID(base_ProductModel productModel)
        {
            foreach (base_ProductStoreModel productStoreItem in productModel.ProductStoreCollection)
            {
                if (productStoreItem.IsNew)
                {
                    productStoreItem.Id = productStoreItem.base_ProductStore.Id;
                    productStoreItem.ProductId = productStoreItem.base_ProductStore.ProductId;

                    // Turn off IsDirty & IsNew
                    productStoreItem.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Update product id have same attribute group
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateAttributeAndSize(base_ProductModel productModel)
        {
            if (productModel.ProductCollection != null)
            {
                // Get deleted products from collection
                IEnumerable<base_ProductModel> deletedProducts = ProductCollection.Where(
                    x => productModel.ProductCollection.DeletedItems.Select(y => y.Code).Contains(x.Code));

                foreach (base_ProductModel productItem in deletedProducts.ToList())
                {
                    // Remove product from collection
                    ProductCollection.Remove(productItem);

                    // Update total products
                    TotalProducts--;
                }

                // Clear deleted products
                productModel.ProductCollection.DeletedItems.Clear();

                foreach (base_ProductModel productItem in productModel.ProductCollection.Where(x => !x.Code.Equals(productModel.Code)))
                {
                    if (productItem.IsNew)
                    {
                        productItem.Id = productItem.base_Product.Id;

                        // Update product store id
                        UpdateProductStoreID(productItem);

                        // Push new product to collection
                        ProductCollection.Insert(0, productItem);

                        // Update total products
                        TotalProducts++;

                        // Turn off IsDirty & IsNew
                        productModel.EndUpdate();
                    }
                }

                // Reupdate all product that same attibute group
                IEnumerable<base_ProductModel> productGroup = ProductCollection.Where(
                    x => productModel.ProductCollection.Select(y => y.GroupAttribute).Contains(x.GroupAttribute));
                foreach (base_ProductModel productItem in productGroup.ToList())
                {
                    // Raise attribute and size when edit in control
                    productItem.ToModelAndRaise();

                    // Clear product collection to reload group attribute
                    productItem.ProductCollection = null;

                    // Load relation data
                    LoadRelationData(productItem);

                    // Turn off IsDirty & IsNew
                    productItem.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Update product group id
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateProductGroupID(base_ProductModel productModel)
        {
            foreach (base_ProductGroupModel productGroupItem in productModel.ProductGroupCollection)
            {
                if (productGroupItem.IsNew)
                {
                    productGroupItem.Id = productGroupItem.base_ProductGroup.Id;
                    productGroupItem.ProductParentId = productGroupItem.base_ProductGroup.ProductParentId;

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

        /// <summary>
        /// Get default markdown and update price
        /// </summary>
        /// <param name="productModel"></param>
        private void GetDefaultMarkdown(base_ProductModel productModel)
        {
            // Set default markdown
            for (int i = 1; i <= 4; i++)
            {
                decimal markdown = PriceSchemaList[i - 1].MarkDown;
                PropertyInfo markdownProperty = productModel.GetType().GetProperty(i.ToString("MarkdownPercent#"));
                if (markdownProperty != null)
                    markdownProperty.SetValue(productModel, markdown, null);

                // Set default price
                productModel.CalcPrice(markdown);
            }
        }

        /// <summary>
        /// Get default markdown and update price
        /// </summary>
        /// <param name="productUOMModel"></param>
        private void GetDefaultMarkdown(base_ProductUOMModel productUOMModel)
        {
            // Set default markdown
            for (int i = 1; i <= 4; i++)
            {
                decimal markdown = PriceSchemaList[i - 1].MarkDown;
                PropertyInfo markdownProperty = productUOMModel.GetType().GetProperty(i.ToString("MarkDownPercent#"));
                if (markdownProperty != null)
                    markdownProperty.SetValue(productUOMModel, markdown, null);

                // Set default price
                productUOMModel.CalcPrice(markdown);
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

                // Copy product store default
                targetProductModel.ProductStoreDefault.ToModel(sourceProductModel.ProductStoreDefault);
                targetProductModel.ProductStoreDefault.Resource = Guid.NewGuid().ToString();
                targetProductModel.ProductStoreDefault.ProductResource = targetProductModel.Resource.ToString();
            }
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
            try
            {
                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.Resource.Equals(productModel.Resource));

                // Get all products that duplicate name
                predicate = predicate.And(x => x.ProductName.Equals(productModel.ProductName));

                // Get all products that duplicate category
                predicate = predicate.And(x => x.ProductCategoryId.Equals(productModel.ProductCategoryId));

                // Get all products that duplicate attribute and size
                predicate = predicate.And(x => x.Attribute.Equals(productModel.Attribute) && x.Size.Equals(productModel.Size));

                return _productRepository.GetIQueryable(predicate).Count() > 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
        }

        /// <summary>
        /// Check barcode duplicate
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateBarcode(base_ProductModel productModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productModel.Barcode))
                    return false;

                string barcode = productModel.Barcode.Trim().ToLower();

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.Resource.Equals(productModel.Resource));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.Barcode.ToLower().Equals(barcode));

                return _productRepository.GetIQueryable(predicate).Count() > 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
        }

        /// <summary>
        /// Check alternate barcode duplicate
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateALU(base_ProductModel productModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productModel.ALU))
                    return false;

                string barcode = productModel.ALU.Trim().ToLower();

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.Resource.Equals(productModel.Resource));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.ALU.ToLower().Equals(barcode));

                return _productRepository.GetIQueryable(predicate).Count() > 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
        }

        /// <summary>
        /// Check code duplicate
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateCode(base_ProductModel productModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productModel.Code))
                    return false;

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.Resource.Equals(productModel.Resource));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.Code.ToLower().Equals(productModel.Code.ToLower()));

                return _productRepository.GetIQueryable(predicate).Count() > 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
        }

        /// <summary>
        /// Check attribute or size is exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsAttributeOrSizeExisted(List<string> list, string name)
        {
            bool result = true;

            // Remove all space in string to compare
            string validName = name.Trim().ToLower().Replace(" ", "");

            if (!string.IsNullOrWhiteSpace(validName))
            {
                result = list.Select(x => x.Trim().ToLower().Replace(" ", "")).Contains(validName);
            }

            return result;
        }

        /// <summary>
        /// Check exist of attribute and size
        /// </summary>
        /// <returns></returns>
        private bool CheckAttributeAndSize()
        {
            bool result = true;

            try
            {
                if (SelectedProduct.Attribute != SelectedProduct.base_Product.Attribute ||
                        SelectedProduct.Size != SelectedProduct.base_Product.Size)
                {
                    MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to save this product?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (msgResult.Is(MessageBoxResult.Yes))
                    {
                        List<string> attributeList = SelectedProduct.ProductCollection.Select(x => x.Attribute).Where(x => !x.Equals(SelectedProduct.Attribute)).ToList();
                        List<string> sizeList = SelectedProduct.ProductCollection.Select(x => x.Size).Where(x => !x.Equals(SelectedProduct.Size)).ToList();
                        if (SelectedProduct.Attribute != SelectedProduct.base_Product.Attribute)
                        {
                            if (IsAttributeOrSizeExisted(attributeList, SelectedProduct.Attribute))
                            {
                                Xceed.Wpf.Toolkit.MessageBox.Show("Attribute is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }
                        else if (SelectedProduct.Size != SelectedProduct.base_Product.Size)
                        {
                            if (IsAttributeOrSizeExisted(sizeList, SelectedProduct.Size))
                            {
                                Xceed.Wpf.Toolkit.MessageBox.Show("Size is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }
                        else
                        {
                            if (IsAttributeOrSizeExisted(attributeList, SelectedProduct.Attribute) ||
                                IsAttributeOrSizeExisted(sizeList, SelectedProduct.Size))
                            {
                                Xceed.Wpf.Toolkit.MessageBox.Show("Attribute and Size is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }

                        // Update attribute and size before show popup
                        SelectedProduct.base_Product.Attribute = SelectedProduct.Attribute;
                        SelectedProduct.base_Product.Size = SelectedProduct.Size;
                        _productRepository.Commit();
                    }
                    else
                        result = false;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }

            return result;
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

        /// <summary>
        /// Process when selected sub product changed
        /// </summary>
        private void OnSelectedSubProductChanged(base_ProductModel selectedSubProduct)
        {
            try
            {
                if (selectedSubProduct != null)
                {
                    if (selectedSubProduct.base_Product.RegularPrice == 0)
                    {
                        UpdateTransactionViewModel viewModel = new UpdateTransactionViewModel(selectedSubProduct);
                        bool? result = _dialogService.ShowDialog<UpdateTransactionView>(_ownerViewModel, viewModel, "Update Product Price");
                        if (result.HasValue && result.Value)
                        {
                            // Update new regular price for product
                            selectedSubProduct.RegularPrice = viewModel.NewPrice;

                            // Update new regular price to database
                            if (viewModel.IsUpdateProductPrice)
                            {
                                UpdateRegularPriceProductGroup(selectedSubProduct);

                                // Map data from model to entity
                                selectedSubProduct.base_Product.RegularPrice = selectedSubProduct.RegularPrice;

                                // Accept changes
                                _productRepository.Commit();
                            }

                            // Turn off IsDirty
                            selectedSubProduct.IsDirty = false;
                        }
                    }

                    // Create new product group model
                    base_ProductGroupModel productGroupModel = new base_ProductGroupModel();

                    // Register property changed event
                    productGroupModel.PropertyChanged += new PropertyChangedEventHandler(productGroupModel_PropertyChanged);

                    productGroupModel.ProductParentId = SelectedProduct.Id;
                    productGroupModel.ProductId = selectedSubProduct.Id;
                    productGroupModel.ProductResource = selectedSubProduct.Resource.ToString();
                    productGroupModel.ItemCode = selectedSubProduct.Code;
                    productGroupModel.ItemName = selectedSubProduct.ProductName;
                    productGroupModel.ItemAttribute = selectedSubProduct.Attribute;
                    productGroupModel.ItemSize = selectedSubProduct.Size;
                    productGroupModel.RegularPrice = selectedSubProduct.RegularPrice;
                    productGroupModel.UOMId = selectedSubProduct.BaseUOMId;
                    productGroupModel.UOM = selectedSubProduct.UOMName;
                    productGroupModel.Quantity = 1;
                    productGroupModel.Resource = Guid.NewGuid();

                    // Get product store
                    base_ProductStore productStore = selectedSubProduct.base_Product.base_ProductStore.
                        SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));
                    if (productStore != null)
                        productGroupModel.OnHandQty = productStore.QuantityOnHand;

                    // Load product UOM collection for product group
                    LoadProductUOMCollectionForGroup(productGroupModel, productStore);

                    // Add new product group to collection
                    SelectedProduct.ProductGroupCollection.Add(productGroupModel);

                    //// Get product item exist in product group collection
                    //base_ProductModel productItem = ProductSubCollection.SingleOrDefault(x => x.Id.Equals(productGroupModel.ProductId));

                    //// Remove product item exist in product group collection
                    //ProductSubCollection.Remove(productItem);

                    _selectedSubProduct = null;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
        /// Get UOM name
        /// </summary>
        /// <param name="productUOMModel"></param>
        private void GetUOMName(base_ProductUOMModel productUOMModel)
        {
            if (string.IsNullOrWhiteSpace(productUOMModel.Name))
            {
                CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productUOMModel.UOMId));
                if (uomItem != null)
                    productUOMModel.Name = uomItem.Text;
            }
        }

        /// <summary>
        /// Load product uom collection for product group
        /// </summary>
        /// <param name="productGroupModel"></param>
        private void LoadProductUOMCollectionForGroup(base_ProductGroupModel productGroupModel, base_ProductStore productStore)
        {
            try
            {
                if (productGroupModel.ProductUOMCollection == null)
                {
                    // Initial product UOM collection for product group
                    productGroupModel.ProductUOMCollection = new ObservableCollection<base_ProductUOMModel>();

                    // Get base UOM for product group
                    base_ProductUOMModel baseProductUOMModel = new base_ProductUOMModel();

                    if (productGroupModel.IsNew)
                    {
                        baseProductUOMModel.Name = productGroupModel.UOM;
                        baseProductUOMModel.UOMId = productGroupModel.UOMId;
                        baseProductUOMModel.RegularPrice = productGroupModel.RegularPrice;
                        baseProductUOMModel.QuantityOnHand = productGroupModel.OnHandQty;
                    }
                    else
                    {
                        baseProductUOMModel.UOMId = productGroupModel.base_ProductGroup.base_Product.BaseUOMId;
                        baseProductUOMModel.RegularPrice = productGroupModel.base_ProductGroup.base_Product.RegularPrice;

                        // Get uom name
                        GetUOMName(baseProductUOMModel);

                        // Get quantity on hand
                        if (productStore != null)
                            baseProductUOMModel.QuantityOnHand = productStore.QuantityOnHand;
                    }

                    // Add new product uom to collection
                    productGroupModel.ProductUOMCollection.Add(baseProductUOMModel);

                    if (productStore != null)
                    {
                        // Get other UOM for product group
                        foreach (base_ProductUOM productUOM in productStore.base_ProductUOM)
                        {
                            if (!productGroupModel.ProductUOMCollection.Select(x => x.UOMId).Contains(productUOM.UOMId))
                            {
                                // Create new product uom model
                                base_ProductUOMModel productUOMModel = new base_ProductUOMModel(productUOM);

                                // Get uom name for product
                                GetUOMName(productUOMModel);

                                // Add new product uom to collection
                                productGroupModel.ProductUOMCollection.Add(productUOMModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Update regular price for product group
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateRegularPriceProductGroup(base_ProductModel productModel)
        {
            try
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
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #region ProductVendorTab

        /// <summary>
        /// Load vendor product collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadVendorProductCollection(base_ProductModel productModel)
        {
            try
            {
                if (productModel.VendorProductCollection == null)
                {
                    // Initial vendor product collection
                    productModel.VendorProductCollection = new CollectionBase<base_VendorProductModel>();

                    foreach (base_VendorProduct vendorProduct in productModel.base_Product.base_VendorProduct)
                    {
                        // Get vendor model of product
                        base_GuestModel vendorModel = ProductVendorCollection.SingleOrDefault(x => x.Resource.Value.ToString().Equals(vendorProduct.VendorResource));

                        if (vendorModel != null)
                        {
                            // Create new vendor product model
                            base_VendorProductModel vendorProductModel = new base_VendorProductModel(vendorProduct);
                            vendorProductModel.VendorCode = vendorModel.GuestNo;
                            vendorProductModel.Company = vendorModel.Company;
                            vendorProductModel.Phone = vendorModel.Phone1;
                            vendorProductModel.Email = vendorModel.Email;

                            // Push new vendor product to collection
                            productModel.VendorProductCollection.Add(vendorProductModel);

                            // Turn off IsDirty & IsNew
                            vendorProductModel.EndUpdate();
                        }
                    }
                }

                // Reset vendor collection
                foreach (base_GuestModel vendorItem in ProductVendorCollection)
                {
                    // Get vendor id list
                    IEnumerable<long> vendorIDs = productModel.VendorProductCollection.Select(x => x.VendorId);

                    if (vendorItem.Id.Equals(productModel.VendorId) || vendorIDs.Contains(vendorItem.Id))
                    {
                        // Hidden selected vendor
                        vendorItem.IsChecked = true;
                    }
                    else
                    {
                        // Visible vendor item
                        vendorItem.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Process before selected vendor changed
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private void OnSelectedVendorChanging(base_GuestModel oldValue, base_GuestModel newValue)
        {
            if (oldValue != null)
            {
                // Get vendor item
                base_GuestModel vendorItem = ProductVendorCollection.FirstOrDefault(x => x.Id.Equals(oldValue.Id));

                // Visible vendor item
                vendorItem.IsChecked = false;
            }
        }

        /// <summary>
        /// Add vendor product to collection when selected
        /// </summary>
        private void OnSelectedVendorProductChanged(base_ProductModel productModel, base_GuestModel vendorModel)
        {
            // Hidden selected vendor
            vendorModel.IsChecked = true;

            // Get vendor resource
            string vendorResource = vendorModel.Resource.ToString();

            // Check vendor is existed
            if (!vendorModel.Id.Equals(productModel.VendorId) &&
                !productModel.VendorProductCollection.Select(x => x.VendorResource).Contains(vendorResource))
            {
                // Create new vendor product model
                base_VendorProductModel vendorProductModel = new base_VendorProductModel();
                if (!productModel.IsNew)
                    vendorProductModel.ProductId = productModel.Id;
                vendorProductModel.ProductResource = productModel.Resource.ToString();
                vendorProductModel.VendorId = vendorModel.Id;
                vendorProductModel.VendorResource = vendorModel.Resource.ToString();
                vendorProductModel.VendorCode = vendorModel.GuestNo;
                vendorProductModel.Company = vendorModel.Company;
                vendorProductModel.Phone = vendorModel.Phone1;
                vendorProductModel.Email = vendorModel.Email;

                // Push new vendor product to collection
                productModel.VendorProductCollection.Add(vendorProductModel);
            }
        }

        #endregion

        #region Save Adjustment

        /// <summary>
        /// Save adjustment for product that have same attribute group
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveAdjustment(base_ProductModel productModel)
        {
            if (productModel.ProductCollection == null)
            {
                foreach (base_ProductStoreModel productStoreItem in productModel.ProductStoreCollection)
                {
                    SaveAdjustment(productModel, productStoreItem);
                }
            }
            else
            {
                foreach (base_ProductModel productItem in productModel.ProductCollection)
                {
                    foreach (base_ProductStoreModel productStoreItem in productItem.ProductStoreCollection)
                    {
                        SaveAdjustment(productItem, productStoreItem);
                    }
                }
            }
        }

        /// <summary>
        /// Save adjustment when cost or quantity changed
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="productStoreModel"></param>
        private void SaveAdjustment(base_ProductModel productModel, base_ProductStoreModel productStoreModel)
        {
            try
            {
                // Get logged time
                DateTime loggedTime = DateTimeExt.Now;

                // Get new and old quantity
                decimal newQuantity = productStoreModel.QuantityOnHand;
                decimal oldQuantity = productStoreModel.OldQuantity;

                // Get new and old cost
                decimal newCost = productModel.AverageUnitCost;
                decimal oldCost = productModel.OldCost;

                // Check quatity or cost changed
                if (newQuantity != oldQuantity)
                {
                    // Save quantity adjustment
                    SaveQuantityAdjustment(productModel, productStoreModel, loggedTime, newQuantity, oldQuantity, newCost);

                    // Save cost adjustment
                    SaveCostAdjustment(productModel, productStoreModel, loggedTime, newQuantity, newCost, oldCost);
                }
                else if (newCost != oldCost && productStoreModel.StoreCode.Equals(Define.StoreCode))
                {
                    // Save quantity adjustment
                    SaveQuantityAdjustment(productModel, productStoreModel, loggedTime, newQuantity, oldQuantity, newCost);

                    // Save cost adjustment
                    SaveCostAdjustment(productModel, productStoreModel, loggedTime, newQuantity, newCost, oldCost);
                }

                // Accept all changes
                _productRepository.Commit();

                // Update old quantity
                productStoreModel.OldQuantity = productStoreModel.QuantityOnHand;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save quantity adjustment
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="productStoreModel"></param>
        /// <param name="loggedTime"></param>
        /// <param name="newQuantity"></param>
        /// <param name="oldQuantity"></param>
        /// <param name="newCost"></param>
        private void SaveQuantityAdjustment(base_ProductModel productModel, base_ProductStoreModel productStoreModel, DateTime loggedTime, decimal newQuantity, decimal oldQuantity, decimal newCost)
        {
            try
            {
                // Create new quantity adjustment item
                base_QuantityAdjustment quantityAdjustment = new base_QuantityAdjustment();
                quantityAdjustment.ProductId = productModel.Id;
                quantityAdjustment.ProductResource = productModel.Resource.ToString();
                quantityAdjustment.NewQty = newQuantity;
                quantityAdjustment.OldQty = oldQuantity;
                quantityAdjustment.AdjustmentQtyDiff = newQuantity - oldQuantity;
                quantityAdjustment.CostDifference = newCost * quantityAdjustment.AdjustmentQtyDiff;
                quantityAdjustment.LoggedTime = loggedTime;
                quantityAdjustment.Reason = (short)AdjustmentReason.ItemEdited;
                quantityAdjustment.Status = (short)AdjustmentStatus.Normal;
                quantityAdjustment.UserCreated = Define.USER.LoginName;
                quantityAdjustment.IsReversed = false;
                quantityAdjustment.StoreCode = productStoreModel.StoreCode;

                // Add new quantity adjustment item to database
                _quantityAdjustmentRepository.Add(quantityAdjustment);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Save cost adjustment
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="productStoreModel"></param>
        /// <param name="loggedTime"></param>
        /// <param name="newQuantity"></param>
        /// <param name="newCost"></param>
        /// <param name="oldCost"></param>
        private void SaveCostAdjustment(base_ProductModel productModel, base_ProductStoreModel productStoreModel, DateTime loggedTime, decimal newQuantity, decimal newCost, decimal oldCost)
        {
            try
            {
                if (newQuantity > 0)
                {
                    // Create new cost adjustment item
                    base_CostAdjustment costAdjustment = new base_CostAdjustment();
                    costAdjustment.ProductId = productModel.Id;
                    costAdjustment.ProductResource = productModel.Resource.ToString();
                    costAdjustment.AdjustmentNewCost = newCost;
                    costAdjustment.AdjustmentOldCost = oldCost;
                    costAdjustment.AdjustCostDifference = newCost - oldCost;
                    costAdjustment.NewCost = newCost * newQuantity;
                    costAdjustment.OldCost = oldCost * newQuantity;
                    costAdjustment.CostDifference = costAdjustment.NewCost - costAdjustment.OldCost;
                    costAdjustment.LoggedTime = loggedTime;
                    costAdjustment.Reason = (short)AdjustmentReason.ItemEdited;
                    costAdjustment.Status = (short)AdjustmentStatus.Normal;
                    costAdjustment.UserCreated = Define.USER.LoginName;
                    costAdjustment.IsReversed = false;
                    costAdjustment.StoreCode = productStoreModel.StoreCode;

                    // Add new cost adjustment item to database
                    _costAdjustmentRepository.Add(costAdjustment);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #endregion

        #region Override Methods

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            if (SelectedProduct != null && !SelectedProduct.IsNew)
            {
                lock (UnitOfWork.Locker)
                {
                    // Refresh static data
                    DepartmentCollection = null;
                    VendorCollection = null;
                    UOMList = null;
                    SaleTaxLocationList = null;

                    // Load static data
                    LoadStaticData();
                }

                // Rollback data
                SelectedProduct.PhotoCollection = null;
                SelectedProduct.ProductCollection = null;
                SelectedProduct.ProductStoreDefault = null;
                SelectedProduct.ProductStoreCollection = null;
                SelectedProduct.ProductGroupCollection = null;
                SelectedProduct.VendorProductCollection = null;
                SelectedProduct.SaleOrderCollection = null;
                SelectedProduct.PurchaseOrderCollection = null;
                _oldItemTypeID = 0;
                SelectedProduct.ToModelAndRaise();
                SelectedProduct.EndUpdate();

                // Unregister property changed event to avoid raise other properties
                SelectedProduct.PropertyChanged -= new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

                // Load photo collection
                LoadPhotoCollection(SelectedProduct);

                // Load product store collection
                LoadProductStoreCollection(SelectedProduct, Define.StoreCode);

                // Load product UOM collection
                //LoadProductUOMCollection(SelectedProduct, Define.StoreCode);

                // Load all product group that have same parent ID
                LoadProductGroupCollection(SelectedProduct);

                // Load vendor product collection
                LoadVendorProductCollection(SelectedProduct);

                // Refresh data from entity
                SelectedProduct.ToModelAndRaise();

                // Keep old cost
                SelectedProduct.OldCost = SelectedProduct.AverageUnitCost;

                // Register property changed event
                SelectedProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

                // Raise ProductDepartmentId, ProductCategoryId and BaseUOMId to run filter
                SelectedProduct.RaiseFilterCollectionView();

                // Turn off IsDirty & IsNew
                SelectedProduct.EndUpdate();
            }

            // Load data by predicate
            LoadDataByPredicate(true);
        }

        /// <summary>
        /// Process when change display view
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ShowNotification(null))
                {
                    // When user clicked create new button
                    if (!isList)
                    {
                        // Create new product
                        NewProduct();

                        // Display product detail
                        IsSearchMode = false;
                    }
                    else
                    {
                        // When user click view list button
                        // Display product list
                        IsSearchMode = true;
                    }
                }
            }
            else
            {
                base_ProductModel productModel = null;
                Guid productGuid = new Guid();
                if (Guid.TryParse(param.ToString(), out productGuid))
                {
                    // Get product from product collection if product view is opened
                    productModel = ProductCollection.SingleOrDefault(x => x.Resource.Equals(productGuid));

                    if (productModel == null)
                    {
                        // Get product from database if product is not loaded
                        productModel = new base_ProductModel(_productRepository.Get(x => x.Resource.Equals(productGuid)));
                    }

                    if (productModel != null)
                    {
                        // Display detail grid
                        IsSearchMode = true;

                        // Load relation collection
                        OnDoubleClickViewCommandExecute(productModel);
                    }
                }
            }
        }

        /// <summary>
        /// Process when changed view
        /// </summary>
        /// <param name="isClosing">Form is closing or changing</param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ShowNotification(isClosing);
        }

        #endregion

        #region Event Methods

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
                case "ItemTypeId":
                    #region ItemTypeIdChanged

                    if (productModel.ProductGroupCollection != null && productModel.ProductGroupCollection.Count > 0 &&
                        !IsGroupItemType && _oldItemTypeID == (short)ItemTypes.Group)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("You must remove all sub item of this product.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Restore product item type
                        App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                        {
                            productModel.ItemTypeId = _oldItemTypeID;
                        });
                    }
                    else
                    {
                        OnPropertyChanged(() => AllowMutilUOM);
                        OnPropertyChanged(() => IsEditOnHandQuantity);
                        OnPropertyChanged(() => AllowEditQuantity);
                        OnPropertyChanged(() => IsGroupItemType);
                        OnPropertyChanged(() => AllowEditCost);
                        OnPropertyChanged(() => AllowGroupItemType);

                        if (!IsEditOnHandQuantity &&
                            productModel.ItemTypeId != (short)ItemTypes.Stockable && _oldItemTypeID == (short)ItemTypes.Stockable)
                        {
                            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("This item type have no quantity. Do you want to continue?", "POS", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (msgResult.Is(MessageBoxResult.OK))
                            {
                                productModel.OnHandStore = 0;
                                _oldItemTypeID = productModel.ItemTypeId;
                            }
                            else
                            {
                                // Restore product item type
                                App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    productModel.ItemTypeId = _oldItemTypeID;
                                });
                            }
                        }
                        else
                        {
                            // Backup item type id
                            _oldItemTypeID = productModel.ItemTypeId;
                        }
                    }

                    if (IsGroupItemType)
                    {
                        // Turn off IsUnOrderAble
                        productModel.IsUnOrderAble = false;
                    }
                    OnPropertyChanged(() => AllowAccessUnOrderAble);

                    #endregion
                    break;
                case "ProductDepartmentId":
                    #region ProductDepartmentIdChanged

                    if (productModel.ProductDepartmentId >= 0)
                    {
                        // Filter category by department
                        _categoryCollectionView.Filter = x =>
                        {
                            base_DepartmentModel categoryItem = x as base_DepartmentModel;
                            return categoryItem.ParentId.Equals(productModel.ProductDepartmentId);
                        };
                    }

                    #endregion
                    break;
                case "ProductCategoryId":
                    #region ProductCategoryIdChanged

                    if (productModel.ProductCategoryId >= 0)
                    {
                        // Get selected category 
                        base_DepartmentModel categoryItem = _categoryCollectionView.Cast<base_DepartmentModel>().
                            FirstOrDefault(x => x.Id.Equals(productModel.ProductCategoryId));

                        if (categoryItem != null)
                        {
                            // Update category name
                            productModel.CategoryName = categoryItem.Name;

                            if (productModel.IsNew)
                            {
                                // Set default margin and markup value for base unit
                                productModel.MarginPercent = categoryItem.Margin;
                                productModel.MarkupPercent = categoryItem.MarkUp;
                            }
                        }

                        // Filter brand by category
                        _brandCollectionView.Filter = x =>
                        {
                            base_DepartmentModel brandItem = x as base_DepartmentModel;
                            return brandItem.ParentId.Equals(productModel.ProductCategoryId);
                        };
                    }

                    #endregion
                    break;
                case "VendorId":
                    #region VendorIdChanged

                    // Get vendor item
                    base_GuestModel vendorItem = ProductVendorCollection.SingleOrDefault(x => x.Id.Equals(productModel.VendorId));

                    if (vendorItem != null)
                    {
                        // Hidden vendor item
                        vendorItem.IsChecked = true;

                        // Get vendor name for product
                        productModel.VendorName = vendorItem.Company;
                    }

                    if (productModel.VendorProductCollection != null)
                    {
                        // Get vendor product model
                        base_VendorProductModel vendorProductItem = productModel.VendorProductCollection.SingleOrDefault(x => x.VendorId.Equals(productModel.VendorId));

                        if (vendorProductItem != null)
                        {
                            // Remove vendor product from collection
                            productModel.VendorProductCollection.Remove(vendorProductItem);
                        }
                    }

                    #endregion
                    break;
                case "BaseUOMId":
                    #region BaseUOMIdChanged

                    // Get uom name for product
                    CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productModel.BaseUOMId));
                    if (uomItem != null)
                        productModel.UOMName = uomItem.Text;

                    #endregion
                    break;
                case "RegularPrice":
                    // Calculator margin, markup and price
                    productModel.UpdateMarginMarkupAndPrice();
                    break;
                case "AverageUnitCost":
                    // Calculator margin, markup and price
                    productModel.UpdateMarginMarkupAndPrice();
                    break;
                case "OnHandStore":
                    #region OnHandStoreChanged

                    // Update quantity in product store
                    productModel.ProductStoreDefault.QuantityOnHand = productModel.OnHandStore;

                    // Update quantity in product
                    productModel.SetOnHandToStore(productModel.OnHandStore, Define.StoreCode);

                    // Update available quantity
                    productModel.ProductStoreDefault.UpdateAvailableQuantity();

                    // Update total quantity in product
                    productModel.UpdateQuantityOnHand();

                    // Update total available quantity on product
                    productModel.UpdateAvailableQuantity();

                    #endregion
                    break;
            }
        }

        /// <summary>
        /// Process when property of product group model changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #region Note Module

        /// <summary>
        /// Initial resource note repository
        /// </summary>
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();

        /// <summary>
        /// Get or sets the StickyManagementViewModel
        /// </summary>
        public PopupStickyViewModel StickyManagementViewModel { get; set; }

        /// <summary>
        /// Load resource note collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadResourceNoteCollection(base_ProductModel productModel)
        {
            try
            {
                // Load resource note collection
                if (productModel.ResourceNoteCollection == null)
                {
                    string resource = productModel.Resource.ToString();
                    productModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                        _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                        Select(x => new base_ResourceNoteModel(x)));
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        #endregion

        #region Permission

        #region Properties

        /// <summary>
        /// Gets the AllowEditQuantity.
        /// </summary>
        public bool AllowEditQuantity
        {
            get
            {
                if (SelectedProduct == null)
                    return UserPermissions.AllowEditQuantity;
                return UserPermissions.AllowEditQuantity && IsEditOnHandQuantity;
            }
        }

        /// <summary>
        /// Gets the AllowEditCost.
        /// </summary>
        public bool AllowEditCost
        {
            get
            {
                return UserPermissions.AllowEditCost && !IsGroupItemType;
            }
        }

        /// <summary>
        /// Gets the AllowAccessUnOrderAble.
        /// </summary>
        public bool AllowAccessUnOrderAble
        {
            get
            {
                if (IsGroupItemType)
                    return false;
                return UserPermissions.AllowAccessProductPermission;
            }
        }

        #endregion

        #endregion

        #region IDragSource Members

        public void StartDrag(DragInfo dragInfo)
        {
            dragInfo.Data = TypeUtilities.CreateDynamicallyTypedList(dragInfo.SourceItems);

            dragInfo.Effects = (dragInfo.Data != null) ? DragDropEffects.Move : DragDropEffects.None;
        }

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(DropInfo dropInfo)
        {

        }

        #endregion
    }
}