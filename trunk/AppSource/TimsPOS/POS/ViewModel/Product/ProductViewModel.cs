using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;

namespace CPC.POS.ViewModel
{
    public class ProductViewModel : ViewModelBase
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

        private base_CostAdjustmentRepository _costAdjustmentRepository = new base_CostAdjustmentRepository();
        private base_QuantityAdjustmentRepository _quantityAdjustmentRepository = new base_QuantityAdjustmentRepository();

        private ICollectionView _categoryCollectionView;
        private ICollectionView _brandCollectionView;

        private short _oldItemTypeID;

        #endregion

        #region Properties

        #region IsSearchMode

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

        #endregion

        private bool _isValidTab;
        /// <summary>
        /// Gets or sets the IsValidTab.
        /// </summary>
        public bool IsValidTab
        {
            get { return _isValidTab; }
            set
            {
                if (_isValidTab != value)
                {
                    _isValidTab = value;
                    OnPropertyChanged(() => IsValidTab);
                }
            }
        }

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

        /// <summary>
        /// Gets or sets the DepartmentList
        /// </summary>
        public ObservableCollection<base_DepartmentModel> DepartmentCollection { get; set; }

        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public ObservableCollection<base_DepartmentModel> CategoryCollection { get; set; }

        /// <summary>
        /// Gets or sets the BrandList
        /// </summary>
        public ObservableCollection<base_DepartmentModel> BrandCollection { get; set; }

        /// <summary>
        /// Gets or sets the VendorList
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorCollection { get; set; }

        /// <summary>
        /// Gets or sets the UOMList
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> UOMList { get; set; }

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
                    return false;
                return SelectedProduct.ItemTypeId == 1;
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
                    _selectedVendor = value;
                    OnPropertyChanged(() => SelectedVendor);
                    if (SelectedVendor != null)
                        OnSelectedVendorChanged(SelectedProduct, SelectedVendor);
                }
            }
        }

        #region Search And Filter

        private int _searchOption;
        /// <summary>
        /// Gets or sets the SearchOption.
        /// </summary>
        public int SearchOption
        {
            get { return _searchOption; }
            set
            {
                if (_searchOption != value)
                {
                    _searchOption = value;
                    OnPropertyChanged(() => SearchOption);
                    if (!string.IsNullOrWhiteSpace(FilterText))
                        OnSearchCommandExecute(FilterText);
                }
            }
        }

        private string _filterText;
        /// <summary>
        /// Gets or sets the FilterText.
        /// <para>Keyword user input but not press enter</para>
        /// <remarks>Binding in textbox keyword</remarks>
        /// </summary>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(() => FilterText);
                }
            }
        }

        public string Keyword { get; set; }

        private string _searchAlert;
        /// <summary>
        /// Gets or sets the SearchAlert.
        /// </summary>
        public string SearchAlert
        {
            get { return _searchAlert; }
            set
            {
                if (_searchAlert != value)
                {
                    _searchAlert = value;
                    OnPropertyChanged(() => SearchAlert);
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ProductViewModel class.
        /// </summary>
        public ProductViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            LoadStaticData();

            InitialCommand();
        }

        /// <summary>
        /// Initializes a new instance of the ProductViewModel class with parameter.
        /// </summary>
        /// <param name="isList">True if show search list, otherwise show detail form</param>
        /// <param name="param">Optional parameter. Default is null</param>
        public ProductViewModel(bool isList, object param = null)
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
                SearchAlert = string.Empty;

                // Search all
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)
                {
                    // Load data by predicate
                    LoadDataByPredicate(false);
                }
                else if (param != null)
                {
                    Keyword = param.ToString();
                    if (SearchOption == 0)
                    {
                        // Alert: Search option is required
                        SearchAlert = "Search Option is required";
                    }
                    else
                    {
                        // Load data by predicate
                        LoadDataByPredicate(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
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
            return true;
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
            return IsValid && IsEdit() && IsValidTab;
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
            return !IsEdit() && !SelectedProduct.IsNew;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this product?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                if (SelectedProduct.IsNew)
                {
                    DeleteNote(SelectedProduct);

                    SelectedProduct = null;
                }
                else if (IsValid)
                {
                    DeleteNote(SelectedProduct);

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

            return dataGridControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the DeletesCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            DataGridControl dataGridControl = param as DataGridControl;

            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete these products?", "POS", MessageBoxButton.YesNo);

            if (msgResult.Is(MessageBoxResult.No))
                return;

            foreach (base_ProductModel productModel in dataGridControl.SelectedItems.Cast<base_ProductModel>().ToList())
            {
                // Delete all note of this product
                DeleteNote(productModel);

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

                // Update selected product
                SelectedProduct = param as base_ProductModel;

                // Load photo collection
                LoadPhotoCollection(SelectedProduct);

                // Load product store collection
                LoadProductStoreCollection(SelectedProduct, Define.StoreCode);

                // Load product UOM collection
                LoadProductUOMCollection(SelectedProduct, Define.StoreCode);

                // Load vendor product collection
                LoadVendorProductCollection(SelectedProduct);

                // Raise ProductDepartmentId, ProductCategoryId and BaseUOMId to run filter
                SelectedProduct.RaiseFilterCollectionView();

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
            DCBLocation departmentType = (DCBLocation)param;

            // Get parentID
            int? parentID = null;
            switch (departmentType)
            {
                case DCBLocation.Category:
                    if (SelectedProduct.ProductDepartmentId == 0)
                        return;
                    parentID = SelectedProduct.ProductDepartmentId;
                    break;
                case DCBLocation.Brand:
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
                    case DCBLocation.Department:
                        // Add new item to collection
                        DepartmentCollection.Add(viewModel.NewItem);

                        // Select new item
                        SelectedProduct.ProductDepartmentId = viewModel.NewItem.Id;
                        break;
                    case DCBLocation.Category:
                        // Add new item to collection
                        CategoryCollection.Add(viewModel.NewItem);

                        // Select new item
                        SelectedProduct.ProductCategoryId = viewModel.NewItem.Id;
                        break;
                    case DCBLocation.Brand:
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

                if (param == null)
                {
                    // Select new vendor
                    SelectedProduct.VendorId = viewModel.NewItem.Id;
                }
                else
                {
                    // Add new vendor to collection
                    OnSelectedVendorChanged(SelectedProduct, viewModel.NewItem);

                    // Select new vendor
                    SelectedVendor = viewModel.NewItem;
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
            if (result.HasValue && result.Value && viewModel.NewItem != null)
            {
                // Add new UOM to list
                UOMList.Add(new CheckBoxItemModel
                {
                    Value = viewModel.NewItem.Id,
                    Text = viewModel.NewItem.Name,
                });

                short uomType = 0;
                if (param != null && Int16.TryParse(param.ToString(), out uomType))
                    uomType = Int16.Parse(param.ToString());

                // Select new UOM
                if (uomType == 0)
                    SelectedProduct.BaseUOMId = viewModel.NewItem.Id;
                else
                    SelectedProduct.ProductUOMCollection[uomType - 1].UOMId = viewModel.NewItem.Id;
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
            return SelectedProduct != null && !SelectedProduct.IsNew;
        }

        /// <summary>
        /// Method to invoke when the PopupAttributeAndSizeCommand command is executed.
        /// </summary>
        private void OnPopupAttributeAndSizeCommandExecute()
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
                    // Check change
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
                        }

                        if (productStoreItem.StoreCode.Equals(Define.StoreCode))
                        {
                            // Update quantity store in product
                            SelectedProduct.OnHandStore = productStoreItem.QuantityOnHand;
                        }
                    }
                }

                // Update all quantity in product
                SelectedProduct.QuantityOnHand = viewModel.QuantityOnHand;
            }
        }

        #endregion

        #region PopupManageUOMCommand

        /// <summary>
        /// Gets the PopupManageUOMCommand command.
        /// </summary>
        public ICommand PopupManageUOMCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupManageUOMCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupManageUOMCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupManageUOMCommand command is executed.
        /// </summary>
        private void OnPopupManageUOMCommandExecute()
        {
            PopupManagementUOMViewModel viewModel = new PopupManagementUOMViewModel(UOMList, SelectedProduct, PriceSchemaList);
            bool? result = _dialogService.ShowDialog<PopupManagementUOMView>(_ownerViewModel, viewModel, "Management UOM");
            if (result.HasValue && result.Value)
            {
                viewModel.CopyUOM(SelectedProduct, viewModel.ResultProductModel);
                for (int i = 0; i < viewModel.ResultProductModel.ProductUOMCollection.Count; i++)
                {
                    base_ProductUOMModel uomModel = viewModel.ResultProductModel.ProductUOMCollection[i];
                    SelectedProduct.ProductUOMCollection[i].ToModel(uomModel);
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

            return dataGridControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the SellCommand command is executed.
        /// </summary>
        private void OnSellCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", dataGridControl.SelectedItems.Cast<base_ProductModel>());
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

            return dataGridControl.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the DuplicateCommand command is executed.
        /// </summary>
        private void OnDuplicateCommandExecute(object param)
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
            LoadProductUOMCollection(SelectedProduct, Define.StoreCode);

            // Load vendor product collection
            LoadVendorProductCollection(SelectedProduct);

            PopupDuplicateItemViewModel viewModel = new PopupDuplicateItemViewModel(SelectedProduct, DepartmentCollection, CategoryCollection);
            bool? result = _dialogService.ShowDialog<PopupDuplicateItemView>(_ownerViewModel, viewModel, "Duplicate Item");
            if (result.HasValue && result.Value)
            {
                // Register property changed event to process filter category, brand by department
                viewModel.DuplicateProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

                // Push new product to collection
                ProductCollection.Add(viewModel.DuplicateProduct);

                // Update total products
                TotalProducts++;
            }

            SelectedProduct = null;
        }

        #endregion

        #region ReceiveCommand

        /// <summary>
        /// Gets the ReceiveCommand command.
        /// </summary>
        public ICommand ReceiveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ReceiveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnReceiveCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the ReceiveCommand command is executed.
        /// </summary>
        private void OnReceiveCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            (_ownerViewModel as MainViewModel).OpenViewExecute("PurchaseOrder", dataGridControl.SelectedItems.Cast<base_ProductModel>());
        }

        #endregion

        #region TransferCommand

        /// <summary>
        /// Gets the TransferCommand command.
        /// </summary>
        public ICommand TransferCommand { get; private set; }

        /// <summary>
        /// Method to check whether the TransferCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnTransferCommandCanExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            if (dataGridControl == null)
                return false;

            return dataGridControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Method to invoke when the TransferCommand command is executed.
        /// </summary>
        private void OnTransferCommandExecute(object param)
        {
            // Convert param to DataGridControl
            DataGridControl dataGridControl = param as DataGridControl;

            (_ownerViewModel as MainViewModel).OpenViewExecute("TransferStock", dataGridControl.SelectedItems.Cast<base_ProductModel>());
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            // Load ComboBox ItemsSource
            // Load department, category and brand list
            IEnumerable<base_DepartmentModel> departments = _departmentRepository.
                GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                OrderBy(x => x.Name).Select(x => new base_DepartmentModel(x, false));

            DepartmentCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 0));
            CategoryCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 1));
            BrandCollection = new ObservableCollection<base_DepartmentModel>(departments.Where(x => x.LevelId == 2));

            // Initial category and brand collection view
            _categoryCollectionView = CollectionViewSource.GetDefaultView(CategoryCollection);
            _brandCollectionView = CollectionViewSource.GetDefaultView(BrandCollection);

            // Load vendor list
            string vendorType = MarkType.Vendor.ToDescription();
            VendorCollection = new ObservableCollection<base_GuestModel>(_guestRepository.
                GetAll(x => x.Mark.Equals(vendorType) && x.IsActived && !x.IsPurged).
                OrderBy(x => x.Company).
                Select(x => new base_GuestModel(x, false)));

            // Load UOM list
            UOMList = new ObservableCollection<CheckBoxItemModel>(_uomRepository.GetIQueryable(x => x.IsActived).
                OrderBy(x => x.Name).Select(x => new CheckBoxItemModel { Value = x.Id, Text = x.Name }));
            UOMList.Insert(0, new CheckBoxItemModel { Value = 0, Text = string.Empty });

            // Load sale tax location list
            var taxLocationPrimary = _taxLocationRepository.Get(x => x.ParentId == 0 && x.IsPrimary);
            if (taxLocationPrimary != null)
            {
                SaleTaxLocationList = new List<string>(_taxLocationRepository.
                    GetIQueryable(x => x.ParentId > 0 && x.ParentId.Equals(taxLocationPrimary.Id)).
                    OrderBy(x => x.TaxCode).
                    Select(x => x.TaxCode));
            }

            // Load PriceSchemas
            LoadPriceSchemas();

            // Initial note collection
            NotePopupCollection = new ObservableCollection<PopupContainer>();
            NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
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
            PopupAvailableCommand = new RelayCommand(OnPopupAvailableCommandExecute, OnPopupAvailableCommandCanExecute);
            PopupManageUOMCommand = new RelayCommand(OnPopupManageUOMCommandExecute, OnPopupManageUOMCommandCanExecute);
            PopupPricingCommand = new RelayCommand(OnPopupPricingCommandExecute, OnPopupPricingCommandCanExecute);
            SellCommand = new RelayCommand<object>(OnSellCommandExecute, OnSellCommandCanExecute);
            DuplicateCommand = new RelayCommand<object>(OnDuplicateCommandExecute, OnDuplicateCommandCanExecute);
            ReceiveCommand = new RelayCommand<object>(OnReceiveCommandExecute, OnReceiveCommandCanExecute);
            TransferCommand = new RelayCommand<object>(OnTransferCommandExecute, OnTransferCommandCanExecute);

            NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
        }

        /// <summary>
        /// Gets a value that indicates whether the data is edit.
        /// </summary>
        /// <returns>true if the data is edit; otherwise, false.</returns>
        private bool IsEdit()
        {
            if (SelectedProduct == null)
                return false;

            return SelectedProduct.IsDirty ||
                (SelectedProduct.PhotoCollection != null && SelectedProduct.PhotoCollection.IsDirty) ||
                (SelectedProduct.ProductCollection != null && SelectedProduct.ProductCollection.IsDirty) ||
                (SelectedProduct.ProductStoreCollection != null && SelectedProduct.ProductStoreCollection.IsDirty) ||
                (SelectedProduct.ProductUOMCollection != null && SelectedProduct.ProductUOMCollection.IsDirty) ||
                (SelectedProduct.VendorProductCollection != null && SelectedProduct.VendorProductCollection.IsDirty);
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
                MessageBoxResult msgResult = MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);

                if (msgResult.Is(MessageBoxResult.Yes))
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

                    // Remove popup note
                    CloseAllPopupNote();
                }
                else
                {
                    if (SelectedProduct.IsNew)
                    {
                        DeleteNote(SelectedProduct);

                        if (isClosing == false)
                        {
                            SelectedProduct = null;
                            IsSearchMode = true;
                        }
                    }
                    else
                    {
                        // Rollback data
                        SelectedProduct.PhotoCollection = null;
                        SelectedProduct.ProductCollection = null;
                        SelectedProduct.ProductUOMCollection = null;
                        SelectedProduct.VendorProductCollection = null;
                        _oldItemTypeID = 0;
                        SelectedProduct.ToModelAndRaise();
                        SelectedProduct.EndUpdate();

                        if (isClosing == false)
                        {
                            // Load photo collection
                            LoadPhotoCollection(SelectedProduct);

                            // Load product store collection
                            LoadProductStoreCollection(SelectedProduct, Define.StoreCode);

                            // Load product UOM collection
                            LoadProductUOMCollection(SelectedProduct, Define.StoreCode);

                            // Load vendor product collection
                            LoadVendorProductCollection(SelectedProduct);
                        }

                        // Remove popup note
                        CloseAllPopupNote();
                    }
                }
            }
            else
            {
                if (SelectedProduct != null && SelectedProduct.IsNew)
                    DeleteNote(SelectedProduct);
                else
                    // Remove popup note
                    CloseAllPopupNote();
            }

            // Clear selected item
            if (result && isClosing == null)
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

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false);

            //// Get all product that contain in department list
            //if (DepartmentList.Count() > 0)
            //{
            //    IEnumerable<int> departmentList = DepartmentList.Select(x => x.IntValue);
            //    predicate = predicate.And(x => departmentList.Contains(x.ProductDepartmentId));
            //}

            //// Get all product that contain in category list
            //if (CategoryList.Count() > 0)
            //{
            //    IEnumerable<int> categoryList = CategoryList.Select(x => x.IntValue);
            //    predicate = predicate.And(x => categoryList.Contains(x.ProductCategoryId));
            //}

            //// Get all product that contain in vendor list
            //if (VendorList.Count() > 0)
            //{
            //    IEnumerable<long> vendorList = VendorList.Select(x => x.LongValue);
            //    predicate = predicate.And(x => vendorList.Contains(x.VendorId));
            //}

            // Set conditions for predicate
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.Code))
                {
                    predicate = predicate.And(x => x.Code.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.ItemName))
                {
                    predicate = predicate.And(x => x.ProductName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.PartNumber))
                {
                    predicate = predicate.And(x => x.PartNumber.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Description))
                {
                    predicate = predicate.And(x => x.Description.ToLower().Contains(keyword.ToLower()) ||
                        x.StyleModel.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Vendor))
                {
                    // Get all vendors that contain keyword
                    IEnumerable<base_GuestModel> vendors = VendorCollection.Where(x => x.Company.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<long> vendorIDList = vendors.Select(x => x.Id);

                    // Get all product that contain in category list
                    if (vendorIDList.Count() > 0)
                        predicate = predicate.And(x => vendorIDList.Contains(x.VendorId));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
                if (SearchOption.Has(SearchOptions.Barcode))
                {
                    predicate = predicate.And(x => x.Barcode.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Category))
                {
                    // Get all categories that contain keyword
                    IEnumerable<base_DepartmentModel> categories = CategoryCollection.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<int> categoryIDList = categories.Select(x => x.Id);

                    // Get all brands that contain keyword
                    IEnumerable<base_DepartmentModel> brands = BrandCollection.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                    IEnumerable<int> brandIDList = brands.Select(x => x.Id);

                    // Get all product that contain in category or brand list
                    if (categoryIDList.Count() > 0)
                        predicate = predicate.And(x => categoryIDList.Contains(x.ProductCategoryId) ||
                            (x.ProductBrandId.HasValue && brandIDList.Contains(x.ProductBrandId.Value)));
                    else
                        // If condition in predicate is false, GetRange function can not get data from database.
                        // Solution for this problem is create fake condition
                        predicate = predicate.And(x => x.Id < 0);
                }
            }
            return predicate;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Product, bool>> predicate = CreateSearchPredicate(Keyword);

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                ProductCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                if (refreshData)
                {
                    OnPropertyChanged(() => AllowMutilUOM);
                }

                // Get total products with condition in predicate
                TotalProducts = _productRepository.GetIQueryable(predicate).Count();

                // Get data with range
                IList<base_Product> products = _productRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                foreach (base_Product product in products)
                {
                    bgWorker.ReportProgress(0, product);
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
            // Register property changed event to process filter category, brand by department
            productModel.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

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

            // Keep old cost
            productModel.OldCost = productModel.AverageUnitCost;

            // Load note
            LoadNote(productModel);

            // Turn off IsDirty & IsNew
            productModel.EndUpdate();
        }

        /// <summary>
        /// Load price schemas
        /// </summary>
        private void LoadPriceSchemas()
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

        /// <summary>
        /// Load photo collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadPhotoCollection(base_ProductModel productModel)
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

        /// <summary>
        /// Load all product that have same attribute group
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadAttributeAndSize(base_ProductModel productModel)
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
                    LoadProductUOMCollection(productItem, Define.StoreCode);

                    // Add new product to collection
                    productModel.ProductCollection.Add(productItem);
                }

                productModel.ProductCollection.Insert(0, productModel);
            }
        }

        /// <summary>
        /// Load product store collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadProductStoreCollection(base_ProductModel productModel, int storeCode)
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
            }
        }

        /// <summary>
        /// Load product UOM collection
        /// </summary>
        private void LoadProductUOMCollection(base_ProductModel productModel, int storeCode)
        {
            if (IsAllowMutilUOM(productModel) && productModel.ProductUOMCollection == null)
            {
                if (productModel.ProductUOMCollection == null)
                {
                    // Get product UOM from database and raise properties
                    List<base_ProductUOMModel> productUOMList = new List<base_ProductUOMModel>();
                    if (!productModel.ProductStoreDefault.IsNew)
                        foreach (base_ProductUOM productUOM in productModel.ProductStoreDefault.base_ProductStore.base_ProductUOM)
                        {
                            base_ProductUOMModel productUOMModel = new base_ProductUOMModel(productUOM, true);
                            productUOMModel.PropertyChanged += new PropertyChangedEventHandler(ProductUOMModel_PropertyChanged);
                            productUOMList.Add(productUOMModel);

                            // Turn off IsDirty & IsNew
                            productUOMModel.EndUpdate();
                        }

                    // Get selected category 
                    base_DepartmentModel categoryItem = _categoryCollectionView.Cast<base_DepartmentModel>().
                        FirstOrDefault(x => x.Id.Equals(productModel.ProductCategoryId));

                    // Add default product UOM model to collection
                    int deltaProductUOM = 3 - productUOMList.Count;
                    for (int i = 0; i < deltaProductUOM; i++)
                    {
                        // Create new a product UOM model
                        base_ProductUOMModel productUOMModel = new base_ProductUOMModel();

                        // Get default markdown
                        GetDefaultMarkdown(productUOMModel);

                        if (categoryItem != null)
                        {
                            // Set default margin and markup value
                            productUOMModel.MarginPercent = categoryItem.Margin;
                            productUOMModel.MarkupPercent = categoryItem.MarkUp;
                        }

                        // Register property changed event
                        productUOMModel.PropertyChanged += new PropertyChangedEventHandler(ProductUOMModel_PropertyChanged);

                        // Add default product UOM to collection
                        productUOMList.Add(productUOMModel);

                        // Turn off IsDirty & IsNew
                        productUOMModel.EndUpdate();
                    }

                    productModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>(productUOMList);
                }
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

            // Create a new product with default values
            SelectedProduct = new base_ProductModel { ProductDepartmentId = -1, ProductCategoryId = -1, ProductBrandId = -1 };

            // Register property changed event to process filter category, brand by department
            SelectedProduct.PropertyChanged += new PropertyChangedEventHandler(SelectedProduct_PropertyChanged);

            // Set default ItemType is Stockable
            SelectedProduct.ItemTypeId = 1;

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

            // Initial product store collection
            SelectedProduct.ProductStoreCollection = new CollectionBase<base_ProductStoreModel>();

            // Create new product store default
            SelectedProduct.ProductStoreDefault = new base_ProductStoreModel { StoreCode = Define.StoreCode };

            // Add new product store default to collection
            SelectedProduct.ProductStoreCollection.Add(SelectedProduct.ProductStoreDefault);

            if (AllowMutilUOM)
            {
                // Initital product uom list
                List<base_ProductUOMModel> productUOMList = new List<base_ProductUOMModel>();

                for (int i = 0; i < 3; i++)
                {
                    // Create new product uom model
                    base_ProductUOMModel productUOMModel = new base_ProductUOMModel();

                    // Register property changed event
                    productUOMModel.PropertyChanged += new PropertyChangedEventHandler(ProductUOMModel_PropertyChanged);

                    // Get default markdown and update price
                    GetDefaultMarkdown(productUOMModel);

                    // Add new product uom to list
                    productUOMList.Add(productUOMModel);

                    // Turn off IsDirty & IsNew
                    productUOMModel.EndUpdate();
                }

                // Initial product uom collection from list
                SelectedProduct.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>(productUOMList);
            }

            // Turn off IsDirty
            SelectedProduct.IsDirty = false;
        }

        /// <summary>
        /// Process save product function
        /// </summary>
        /// <returns></returns>
        private bool SaveProduct(base_ProductModel productModel)
        {
            // Check duplicate product
            if (IsDuplicateProduct(productModel))
            {
                MessageBox.Show("This product is duplicated");
                return false;
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
            }

            SaveAdjustment(productModel);

            // Update default photo
            productModel.PhotoDefault = productModel.PhotoCollection.FirstOrDefault();

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
            // Map data from model to entity
            productModel.ToEntity();

            // Save photo collection
            SavePhotoCollection(productModel);

            // Save product store collection
            SaveProductStoreCollection(productModel);

            // Save product UOM collection
            SaveProductUOMCollection(productModel, Define.StoreCode);

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

            // Update product UOM id
            UpdateProductUOMID(productModel);

            // Update vendor product id
            UpdateVendorProductID(productModel);

            // Push new product to collection
            ProductCollection.Add(productModel);

            // Update total products
            TotalProducts++;
        }

        /// <summary>
        /// Save when edit or update product
        /// </summary>
        private void SaveUpdate(base_ProductModel productModel)
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

            // Save product UOM collection
            SaveProductUOMCollection(productModel, Define.StoreCode);

            // Save vendor product collection
            SaveVendorProductCollection(productModel);

            // Accept changes
            _productRepository.Commit();

            // Update product store id
            UpdateProductStoreID(productModel);

            // Update product UOM id
            UpdateProductUOMID(productModel);

            // Update product id have same group attribute
            UpdateAttributeAndSize(productModel);

            // Update vendor product id
            UpdateVendorProductID(productModel);
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
        /// Save product collection have same attribute group when update product
        /// </summary>
        private void SaveAttributeAndSize(base_ProductModel productModel)
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
                        CopyProduct(productItem);
                        CopyProductStoreDefault(productModel, productItem);
                    }
                    else
                        productItem.DateUpdated = productModel.DateUpdated;

                    // Update quantity on hand
                    productItem.UpdateQuantityOnHand();

                    // Save product store collection
                    SaveProductStoreCollection(productItem);

                    // Map data from model to entity
                    productItem.ToEntity();

                    // Save product UOM collection
                    SaveProductUOMCollection(productItem, Define.StoreCode);

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
        /// Save vendor product collection
        /// </summary>
        /// <param name="productModel"></param>
        private void SaveVendorProductCollection(base_ProductModel productModel)
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

        /// <summary>
        /// Update product UOM id after add new to database
        /// </summary>
        /// <param name="productModel"></param>
        private void UpdateProductUOMID(base_ProductModel productModel)
        {
            if (productModel.ProductUOMCollection != null)
            {
                foreach (base_ProductUOMModel productUOMModel in productModel.ProductUOMCollection)
                {
                    if (productUOMModel.UOMId > 0 && productUOMModel.Id == 0)
                    {
                        productUOMModel.Id = productUOMModel.base_ProductUOM.Id;
                        productUOMModel.ProductStoreId = productUOMModel.base_ProductUOM.ProductStoreId;
                    }

                    // Turn off IsDirty & IsNew
                    productUOMModel.EndUpdate();
                }
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

                        // Update product UOM id
                        UpdateProductUOMID(productItem);

                        // Clear product UOM collection to reload
                        productItem.ProductUOMCollection = null;

                        // Push new product to collection
                        ProductCollection.Add(productItem);

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
        /// Copy values from selected product to other product
        /// </summary>
        /// <param name="targetProductModel"></param>
        private void CopyProduct(base_ProductModel targetProductModel)
        {
            //targetProductModel.Id = SelectedProduct.Id;
            //targetProductModel.Code = SelectedProduct.Code;
            targetProductModel.ItemTypeId = SelectedProduct.ItemTypeId;
            targetProductModel.ProductDepartmentId = SelectedProduct.ProductDepartmentId;
            targetProductModel.ProductCategoryId = SelectedProduct.ProductCategoryId;
            targetProductModel.ProductBrandId = SelectedProduct.ProductBrandId;
            targetProductModel.StyleModel = SelectedProduct.StyleModel;
            targetProductModel.ProductName = SelectedProduct.ProductName;
            targetProductModel.Description = SelectedProduct.Description;
            //targetProductModel.Barcode = SelectedProduct.Barcode;
            //targetProductModel.Attribute = SelectedProduct.Attribute;
            //targetProductModel.Size = SelectedProduct.Size;
            targetProductModel.IsSerialTracking = SelectedProduct.IsSerialTracking;
            targetProductModel.IsPublicWeb = SelectedProduct.IsPublicWeb;
            //targetProductModel.OnHandStore1 = SelectedProduct.OnHandStore1;
            //targetProductModel.OnHandStore2 = SelectedProduct.OnHandStore2;
            //targetProductModel.OnHandStore3 = SelectedProduct.OnHandStore3;
            //targetProductModel.OnHandStore4 = SelectedProduct.OnHandStore4;
            //targetProductModel.OnHandStore5 = SelectedProduct.OnHandStore5;
            //targetProductModel.OnHandStore6 = SelectedProduct.OnHandStore6;
            //targetProductModel.OnHandStore7 = SelectedProduct.OnHandStore7;
            //targetProductModel.OnHandStore8 = SelectedProduct.OnHandStore8;
            //targetProductModel.OnHandStore9 = SelectedProduct.OnHandStore9;
            //targetProductModel.OnHandStore10 = SelectedProduct.OnHandStore10;
            //targetProductModel.QuantityOnHand = SelectedProduct.QuantityOnHand;
            //targetProductModel.QuantityOnOrder = SelectedProduct.QuantityOnOrder;
            targetProductModel.CompanyReOrderPoint = SelectedProduct.CompanyReOrderPoint;
            targetProductModel.IsUnOrderAble = SelectedProduct.IsUnOrderAble;
            targetProductModel.IsEligibleForCommission = SelectedProduct.IsEligibleForCommission;
            targetProductModel.IsEligibleForReward = SelectedProduct.IsEligibleForReward;
            targetProductModel.RegularPrice = SelectedProduct.RegularPrice;
            targetProductModel.Price1 = SelectedProduct.Price1;
            targetProductModel.Price2 = SelectedProduct.Price2;
            targetProductModel.Price3 = SelectedProduct.Price3;
            targetProductModel.Price4 = SelectedProduct.Price4;
            targetProductModel.OrderCost = SelectedProduct.OrderCost;
            targetProductModel.AverageUnitCost = SelectedProduct.AverageUnitCost;
            targetProductModel.TaxCode = SelectedProduct.TaxCode;
            targetProductModel.MarginPercent = SelectedProduct.MarginPercent;
            targetProductModel.MarkupPercent = SelectedProduct.MarkupPercent;
            targetProductModel.BaseUOMId = SelectedProduct.BaseUOMId;
            targetProductModel.GroupAttribute = SelectedProduct.GroupAttribute;
            targetProductModel.Custom1 = SelectedProduct.Custom1;
            targetProductModel.Custom2 = SelectedProduct.Custom2;
            targetProductModel.Custom3 = SelectedProduct.Custom3;
            targetProductModel.Custom4 = SelectedProduct.Custom4;
            targetProductModel.Custom5 = SelectedProduct.Custom5;
            targetProductModel.Custom6 = SelectedProduct.Custom6;
            targetProductModel.Custom7 = SelectedProduct.Custom7;
            //targetProductModel.Resource = SelectedProduct.Resource;
            //targetProductModel.UserCreated = SelectedProduct.UserCreated;
            targetProductModel.DateCreated = DateTimeExt.Now;
            //targetProductModel.UserUpdated = SelectedProduct.UserUpdated;
            //targetProductModel.DateUpdated = DateTimeExt.Now;
            targetProductModel.WarrantyType = SelectedProduct.WarrantyType;
            targetProductModel.WarrantyNumber = SelectedProduct.WarrantyNumber;
            targetProductModel.WarrantyPeriod = SelectedProduct.WarrantyPeriod;
            //targetProductModel.PartNumber = SelectedProduct.PartNumber;
            targetProductModel.SellUOMId = SelectedProduct.SellUOMId;
            targetProductModel.OrderUOMId = SelectedProduct.OrderUOMId;
            targetProductModel.IsPurge = SelectedProduct.IsPurge;
            targetProductModel.VendorId = SelectedProduct.VendorId;
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
            }
        }

        /// <summary>
        /// Copy UOM of selected product to other product
        /// </summary>
        /// <param name="targetProductModel"></param>
        private void CopyUOM(base_ProductModel sourceProductModel, base_ProductModel targetProductModel)
        {
            if (sourceProductModel.ProductUOMCollection != null)
            {
                // Initial product UOM collection
                targetProductModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>();

                foreach (base_ProductUOMModel productUOMItem in sourceProductModel.ProductUOMCollection.Where(x => x.UOMId > 0))
                {
                    // Create new product UOM model
                    base_ProductUOMModel productUOMModel = new base_ProductUOMModel();

                    // Get values from selected product UOM
                    productUOMModel.ToModel(productUOMItem);

                    // Add product UOM to collection
                    targetProductModel.ProductUOMCollection.Add(productUOMModel);
                }
            }
        }

        /// <summary>
        /// Copy UOM of selected product to other product
        /// </summary>
        /// <param name="targetProductModel"></param>
        private void CopyUOM(base_ProductModel sourceProductModel, base_ProductStoreModel targetProductStoreModel)
        {
            if (sourceProductModel.ProductUOMCollection != null)
            {
                // Initial product UOM collection
                targetProductStoreModel.ProductUOMCollection = new CollectionBase<base_ProductUOMModel>();

                foreach (base_ProductUOMModel productUOMItem in sourceProductModel.ProductUOMCollection.Where(x => x.UOMId > 0))
                {
                    // Create new product UOM model
                    base_ProductUOMModel productUOMModel = new base_ProductUOMModel();

                    // Get values from selected product UOM
                    productUOMModel.ToModel(productUOMItem);

                    // Add product UOM to collection
                    targetProductStoreModel.ProductUOMCollection.Add(productUOMModel);
                }
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

            if (SelectedProduct.Attribute != SelectedProduct.base_Product.Attribute ||
                SelectedProduct.Size != SelectedProduct.base_Product.Size)
            {
                MessageBoxResult msgResult = MessageBox.Show("Do you want to save this product?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    List<string> attributeList = SelectedProduct.ProductCollection.Select(x => x.Attribute).Where(x => !x.Equals(SelectedProduct.Attribute)).ToList();
                    List<string> sizeList = SelectedProduct.ProductCollection.Select(x => x.Size).Where(x => !x.Equals(SelectedProduct.Size)).ToList();
                    if (SelectedProduct.Attribute != SelectedProduct.base_Product.Attribute)
                    {
                        if (IsAttributeOrSizeExisted(attributeList, SelectedProduct.Attribute))
                        {
                            MessageBox.Show("Attribute is existed", "POS", MessageBoxButton.OK);
                            return false;
                        }
                    }
                    else if (SelectedProduct.Size != SelectedProduct.base_Product.Size)
                    {
                        if (IsAttributeOrSizeExisted(sizeList, SelectedProduct.Size))
                        {
                            MessageBox.Show("Size is existed", "POS", MessageBoxButton.OK);
                            return false;
                        }
                    }
                    else
                    {
                        if (IsAttributeOrSizeExisted(attributeList, SelectedProduct.Attribute) ||
                            IsAttributeOrSizeExisted(sizeList, SelectedProduct.Size))
                        {
                            MessageBox.Show("Attribute and Size is existed", "POS", MessageBoxButton.OK);
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
            return Define.CONFIGURATION.IsAllowMutilUOM.Value &&
                (productModel != null && productModel.ItemTypeId <= 3);
        }

        #region ProductVendorTab

        /// <summary>
        /// Load vendor product collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadVendorProductCollection(base_ProductModel productModel)
        {
            if (productModel.VendorProductCollection == null)
            {
                // Initial vendor product collection
                productModel.VendorProductCollection = new CollectionBase<base_VendorProductModel>();

                //// Get product resource
                //string productResource = productModel.Resource.ToString();

                //// Get all vendor product of product resource
                //IQueryable<base_VendorProduct> vendorProducts = _vendorProductRepository.GetIQueryable(x => x.ProductResource.Equals(productResource));
                foreach (base_VendorProduct vendorProduct in productModel.base_Product.base_VendorProduct)
                {
                    // Get vendor model of product
                    base_GuestModel vendorModel = VendorCollection.FirstOrDefault(x => x.Resource.ToString().Equals(vendorProduct.VendorResource));

                    // Create new vendor product model
                    base_VendorProductModel vendorProductModel = new base_VendorProductModel(vendorProduct);
                    vendorProductModel.VendorCode = vendorModel.GuestNo;
                    vendorProductModel.Phone = vendorModel.Phone1;
                    vendorProductModel.Email = vendorModel.Email;

                    // Push new vendor product to collection
                    productModel.VendorProductCollection.Add(vendorProductModel);

                    // Turn off IsDirty & IsNew
                    vendorProductModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Add vendor product to collection when selected
        /// </summary>
        private void OnSelectedVendorChanged(base_ProductModel productModel, base_GuestModel vendorModel)
        {
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
                vendorProductModel.Phone = vendorModel.Phone1;
                vendorProductModel.Email = vendorModel.Email;

                // Push new vendor product to collection
                productModel.VendorProductCollection.Add(vendorProductModel);
            }
        }

        #endregion

        #region Save Adjustment

        private string _costAdjustmentReason = "Cost adjustment";
        private string _quantityAdjustmentReason = "Quantity adjustment";
        private DateTime _loggedTime;

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

        private void SaveAdjustment(base_ProductModel productModel, base_ProductStoreModel productStoreModel)
        {
            // Get logged time
            _loggedTime = DateTimeExt.Now;

            // Get new and old quantity
            int newQuantity = productStoreModel.QuantityOnHand;
            int oldQuantity = productStoreModel.OldQuantity;

            // Get new and old cost
            decimal newCost = productModel.AverageUnitCost;
            decimal oldCost = productModel.OldCost;

            // Check quatity or cost changed
            if (newQuantity != oldQuantity || newCost != oldCost)
            {
                // Save quantity adjustment
                // Create new quantity adjustment item
                base_QuantityAdjustment quantityAdjustment = new base_QuantityAdjustment();
                quantityAdjustment.ProductId = productModel.Id;
                quantityAdjustment.ProductResource = productModel.Resource.ToString();
                quantityAdjustment.NewQty = newQuantity;
                quantityAdjustment.OldQty = oldQuantity;
                quantityAdjustment.AdjustmentQtyDiff = newQuantity - oldQuantity;
                quantityAdjustment.CostDifference = newCost * quantityAdjustment.AdjustmentQtyDiff;
                quantityAdjustment.LoggedTime = _loggedTime;
                quantityAdjustment.Reason = _quantityAdjustmentReason;
                quantityAdjustment.Status = _quantityAdjustmentReason;
                quantityAdjustment.UserCreated = Define.USER.LoginName;
                quantityAdjustment.StoreCode = productStoreModel.StoreCode;

                // Add new quantity adjustment item to database
                _quantityAdjustmentRepository.Add(quantityAdjustment);

                // Save cost adjustment
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
                costAdjustment.LoggedTime = _loggedTime;
                costAdjustment.Reason = _costAdjustmentReason;
                costAdjustment.Status = _costAdjustmentReason;
                costAdjustment.UserCreated = Define.USER.LoginName;
                costAdjustment.StoreCode = productStoreModel.StoreCode;

                // Add new cost adjustment item to database
                _costAdjustmentRepository.Add(costAdjustment);
            }

            // Accept all changes
            _productRepository.Commit();

            // Update old cost
            productModel.OldCost = newCost;
        }

        #endregion

        #endregion

        #region Override Methods

        /// <summary>
        /// Process load data
        /// </summary>
        public override void LoadData()
        {
            // Refresh price schemas
            LoadPriceSchemas();

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
                if (param is base_ProductModel)
                {
                    productModel = param as base_ProductModel;
                }
                else if (Guid.TryParse(param.ToString(), out productGuid))
                {
                    productModel = new base_ProductModel(_productRepository.Get(x => x.Resource.Equals(productGuid)));
                }

                if (productModel != null)
                {
                    OnDoubleClickViewCommandExecute(productModel);
                }

                // Display product detail
                IsSearchMode = false;
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
                    OnPropertyChanged(() => AllowMutilUOM);
                    OnPropertyChanged(() => IsEditOnHandQuantity);
                    if (!IsEditOnHandQuantity && _oldItemTypeID == 1 && productModel.ItemTypeId != _oldItemTypeID)
                    {
                        MessageBoxResult msgResult = MessageBox.Show("This item type have no quantity. Do you want to continue?", "POS", MessageBoxButton.OKCancel);
                        if (msgResult.Is(MessageBoxResult.OK))
                        {
                            productModel.OnHandStore1 = 0;
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
                        _oldItemTypeID = productModel.ItemTypeId;
                    break;
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

                                if (productModel.ProductUOMCollection != null)
                                {
                                    // Set default margin and markup value for other UOM
                                    foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection)
                                    {
                                        productUOMItem.MarginPercent = categoryItem.Margin;
                                        productUOMItem.MarkupPercent = categoryItem.MarkUp;
                                    }
                                }
                            }
                        }

                        // Filter brand by category
                        _brandCollectionView.Filter = x =>
                        {
                            base_DepartmentModel brandItem = x as base_DepartmentModel;
                            return brandItem.ParentId.Equals(productModel.ProductCategoryId);
                        };
                    }
                    break;
                case "VendorId":
                    // Get vendor name for product
                    base_GuestModel vendorItem = VendorCollection.FirstOrDefault(x => x.Id.Equals(productModel.VendorId));
                    if (vendorItem != null)
                        productModel.VendorName = vendorItem.Company;
                    break;
                case "BaseUOMId":
                    // Update product UOM model
                    if (productModel.BaseUOMId == 0 && productModel.ProductUOMCollection != null)
                    {
                        // Update product UOM id value
                        foreach (base_ProductUOMModel productUOMModel in productModel.ProductUOMCollection)
                            productUOMModel.UOMId = 0;
                    }

                    // Get uom name for product
                    CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productModel.BaseUOMId));
                    if (uomItem != null)
                        productModel.UOMName = uomItem.Text;
                    break;
                case "RegularPrice":
                    // Calculator margin, markup and price
                    productModel.UpdateMarginMarkupAndPrice();
                    break;
                case "AverageUnitCost":
                    // Calculator margin, markup and price
                    productModel.UpdateMarginMarkupAndPrice();

                    // Update average cost for other UOM
                    if (productModel.ProductUOMCollection != null)
                        foreach (base_ProductUOMModel productUOMItem in productModel.ProductUOMCollection.Where(x => x.UOMId > 0))
                            productUOMItem.UpdateAverageCost(productModel.AverageUnitCost);
                    break;
                case "OnHandStore":
                    // Update quantity in product store
                    productModel.ProductStoreDefault.QuantityOnHand = productModel.OnHandStore;

                    // Update quantity in product
                    productModel.SetOnHandToStore(productModel.OnHandStore, Define.StoreCode);

                    // Update total quantity in product
                    productModel.UpdateQuantityOnHand();
                    break;
            }
        }

        /// <summary>
        /// Process when property of product UOM model changed
        /// </summary>
        /// <param name="sender">Product UOM model</param>
        /// <param name="e"></param>
        private void ProductUOMModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Get product UOM model
            base_ProductUOMModel productUOMModel = sender as base_ProductUOMModel;

            switch (e.PropertyName)
            {
                case "UOMId":
                    if (productUOMModel.UOMId == 0)
                    {
                        productUOMModel.BaseUnitNumber = 0;
                        productUOMModel.RegularPrice = 0;
                        productUOMModel.QuantityOnHand = 0;
                        productUOMModel.AverageCost = 0;
                    }
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

        #endregion

        #region Save Image

        /// <summary>
        /// Get product image folder
        /// </summary>
        private string IMG_PRODUCT_DIRECTORY = Path.Combine(Define.ImageFilesFolder, "Product");

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

        #region Defines

        private string _showStickies = "Show stickies";
        private string _hideStickies = "Hide stickies";

        private base_ResourceNoteRepository _noteRepository = new base_ResourceNoteRepository();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the NotePopupCollection.
        /// </summary>
        public ObservableCollection<PopupContainer> NotePopupCollection { get; set; }

        private string _addNoteText = "Add sticky";
        /// <summary>
        /// Gets or sets the AddNoteText.
        /// </summary>
        public string AddNoteText
        {
            get { return _addNoteText; }
            set
            {
                if (_addNoteText != value)
                {
                    _addNoteText = value;
                    OnPropertyChanged(() => AddNoteText);
                }
            }
        }

        /// <summary>
        /// Gets the ShowOrHiddenNote
        /// </summary>
        public string ShowOrHiddenNote
        {
            get
            {
                if (NotePopupCollection.Count == 0)
                    return _showStickies;
                else if (NotePopupCollection.Count == SelectedProduct.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                    return _hideStickies;
                else
                    return _showStickies;
            }
        }

        #endregion

        #region Command Methods

        #region NewNoteCommand

        /// <summary>
        /// Gets the NewNoteCommand command.
        /// </summary>
        public ICommand NewNoteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewNoteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewNoteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewNoteCommand command is executed.
        /// </summary>
        private void OnNewNoteCommandExecute()
        {
            if (SelectedProduct.ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
                return;

            // Create a new note
            base_ResourceNoteModel noteModel = new base_ResourceNoteModel
            {
                Resource = SelectedProduct.Resource.ToString(),
                Color = Define.DefaultColorNote,
                DateCreated = DateTimeExt.Now
            };

            // Create default position for note
            Point position = new Point(400, 200);
            if (SelectedProduct.ResourceNoteCollection.Count > 0)
            {
                Point lastPostion = SelectedProduct.ResourceNoteCollection.LastOrDefault().Position;
                if (lastPostion != null)
                    position = new Point(lastPostion.X + 10, lastPostion.Y + 10);
            }

            // Update position
            noteModel.Position = position;

            // Add new note to collection
            SelectedProduct.ResourceNoteCollection.Add(noteModel);

            // Show new note
            PopupContainer popupContainer = CreatePopupNote(noteModel);
            popupContainer.Show();
            NotePopupCollection.Add(popupContainer);
        }

        #endregion

        #region ShowOrHiddenNoteCommand

        /// <summary>
        /// Gets the ShowOrHiddenNoteCommand command.
        /// </summary>
        public ICommand ShowOrHiddenNoteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ShowOrHiddenNoteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShowOrHiddenNoteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ShowOrHiddenNoteCommand command is executed.
        /// </summary>
        private void OnShowOrHiddenNoteCommandExecute()
        {
            if (NotePopupCollection.Count == SelectedProduct.ResourceNoteCollection.Count)
            {
                // Created popup notes, only show or hidden them
                if (ShowOrHiddenNote.Equals(_hideStickies))
                {
                    foreach (PopupContainer popupContainer in NotePopupCollection)
                        popupContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (PopupContainer popupContainer in NotePopupCollection)
                        popupContainer.Show();
                }
            }
            else
            {
                // Close all note
                CloseAllPopupNote();

                Point position = new Point(400, 200);
                foreach (base_ResourceNoteModel noteModel in SelectedProduct.ResourceNoteCollection)
                {
                    noteModel.Position = position;
                    PopupContainer popupContainer = CreatePopupNote(noteModel);
                    popupContainer.Show();
                    NotePopupCollection.Add(popupContainer);
                    position = new Point(position.X + 10, position.Y + 10);
                }
            }

            // Update label "Show/Hidden Note"
            OnPropertyChanged(() => ShowOrHiddenNote);
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load notes
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadNote(base_ProductModel productModel)
        {
            // Load Note
            if (productModel.ResourceNoteCollection == null)
            {
                string resource = productModel.Resource.ToString();
                productModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _noteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        /// <summary>
        /// Create or update note
        /// </summary>
        /// <param name="noteModel"></param>
        private void SaveNote(base_ResourceNoteModel noteModel)
        {
            noteModel.ToEntity();
            if (noteModel.IsNew)
                _noteRepository.Add(noteModel.base_ResourceNote);
            _noteRepository.Commit();
            noteModel.EndUpdate();
        }

        /// <summary>
        /// Delete and close popup notes
        /// </summary>
        private void DeleteNote(base_ProductModel productModel)
        {
            // Remove popup note
            CloseAllPopupNote();

            // Delete note
            foreach (base_ResourceNoteModel noteModel in productModel.ResourceNoteCollection)
                _noteRepository.Delete(noteModel.base_ResourceNote);
            _noteRepository.Commit();
            productModel.ResourceNoteCollection.Clear();
        }

        /// <summary>
        /// Close popup notes
        /// </summary>
        private void CloseAllPopupNote()
        {
            // Remove popup note
            foreach (PopupContainer popupContainer in NotePopupCollection)
                popupContainer.Close();
            NotePopupCollection.Clear();
        }

        /// <summary>
        /// Create popup note
        /// </summary>
        /// <param name="noteModel"></param>
        /// <returns></returns>
        private PopupContainer CreatePopupNote(base_ResourceNoteModel noteModel)
        {
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.SelectedNote = noteModel;
            noteViewModel.NotePopupCollection = NotePopupCollection;
            noteViewModel.ResourceNoteCollection = SelectedProduct.ResourceNoteCollection;
            CPC.POS.View.NoteView noteView = new View.NoteView();

            PopupContainer popupContainer = new PopupContainer(noteView, true);
            popupContainer.WindowStartupLocation = WindowStartupLocation.Manual;
            popupContainer.DataContext = noteViewModel;
            popupContainer.Width = 150;
            popupContainer.Height = 120;
            popupContainer.MinWidth = 150;
            popupContainer.MinHeight = 120;
            popupContainer.FormBorderStyle = PopupContainer.BorderStyle.None;
            popupContainer.Deactivated += (sender, e) => { SaveNote(noteModel); };
            popupContainer.Loaded += (sender, e) =>
            {
                popupContainer.Left = noteModel.Position.X;
                popupContainer.Top = noteModel.Position.Y;
            };

            // Set key binding
            (_ownerViewModel as MainViewModel).SetKeyBinding(App.Current.MainWindow.InputBindings, popupContainer);

            return popupContainer;
        }

        #endregion

        #endregion
    }
}
