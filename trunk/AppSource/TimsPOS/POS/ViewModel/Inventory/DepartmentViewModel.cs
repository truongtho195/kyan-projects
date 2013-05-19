using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.ComponentModel;
using CPC.POS.Repository;
using CPC.POS.Database;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Threading;

namespace CPC.POS.ViewModel
{
    public class DepartmentViewModel : ViewModelBase
    {
        #region Enum

        private enum Parts
        {
            Main = 0, // TabControl contains Categories and products be showed.
            Deparment = 1, //Create or edit department part be showed. 
            Category = 2, // //Create or edit category part be showed. 
            Brand = 3 //Create or edit brand part be showed. 
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets data on a separate thread.
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Column on product table used for sort.
        /// </summary>
        private string _productColumnSort = "It.Id";

        /// <summary>
        /// Current location is deparment part, or category part, or brand part. Used for insert, update deparment,
        /// category, brand.
        /// </summary>
        private Parts _currentPart = Parts.Main;

        /// <summary>
        /// Determine whether department, category, or brand is editing.
        /// </summary>
        private bool _isEditingElement = false;

        /// <summary>
        /// Determine whether department, category, or brand is create new.
        /// </summary>
        private bool _isCreateElement = false;

        /// <summary>
        /// Default tax code used when create new category.
        /// </summary>
        private string _defaultTaxCode = null;

        #endregion

        #region Constructors

        public DepartmentViewModel()
        {
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunWorkerCompleted);
        }

        #endregion

        #region Properties

        #region SaleTaxLocations

        private CollectionBase<base_SaleTaxLocationModel> _saleTaxLocations;
        /// <summary>
        /// Contains all base_SaleTaxLocationModels.
        /// </summary>
        public CollectionBase<base_SaleTaxLocationModel> SaleTaxLocations
        {
            get
            {
                return _saleTaxLocations;
            }
            set
            {
                if (_saleTaxLocations != value)
                {
                    _saleTaxLocations = value;
                    OnPropertyChanged(() => SaleTaxLocations);
                }
            }
        }

        #endregion

        #region DepartmentCollection

        private CollectionBase<DepartmentRootModel> _departmentCollection;
        /// <summary>
        /// Contains all ProductDepartmentModels, ProductCategoryModels and ProductBrandModels.
        /// </summary>
        public CollectionBase<DepartmentRootModel> DepartmentCollection
        {
            get
            {
                return _departmentCollection;
            }
            set
            {
                if (_departmentCollection != value)
                {
                    _departmentCollection = value;
                    OnPropertyChanged(() => DepartmentCollection);
                }
            }
        }

        #endregion

        #region ProductCategoryCollection

        private CollectionBase<ProductCategoryModel> _productCategoryCollection;
        /// <summary>
        /// Contains all ProductCategoryModels.
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

        #region SelectedItem

        private object _selectedItem;
        /// <summary>
        /// Gets or sets SelectedItem on TreeView.
        /// </summary>
        public object SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    OnSelectedItemChanging();
                    _selectedItem = value;
                    OnPropertyChanged(() => SelectedItem);
                    OnSelectedItemChanged();
                }
            }
        }

        #endregion

        #region ProductDepartmentModel

        private ProductDepartmentModel _productDepartmentModel;
        /// <summary>
        /// Used for create new or edit deparment.
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
                    _productDepartmentModel = value;
                    OnPropertyChanged(() => ProductDepartmentModel);
                }
            }
        }

        #endregion

        #region ProductCategoryModel

        private ProductCategoryModel _productCategoryModel;
        /// <summary>
        /// Used for create new or edit category.
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
                    _productCategoryModel = value;
                    OnPropertyChanged(() => ProductCategoryModel);
                }
            }
        }

        #endregion

        #region ProductBrandModel

        private ProductBrandModel _productBrandModel;
        /// <summary>
        /// /// <summary>
        /// Used for create new or edit brand.
        /// </summary>
        /// </summary>
        public ProductBrandModel ProductBrandModel
        {
            get
            {
                return _productBrandModel;
            }
            set
            {
                if (_productBrandModel != value)
                {
                    _productBrandModel = value;
                    OnPropertyChanged(() => ProductBrandModel);
                }
            }
        }

        #endregion

        #region SelectedIndex

        private int _selectedIndex;
        /// <summary>
        /// Gets or sets SelectedIndex on TabControl.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(() => SelectedIndex);
                    OnSelectedIndexChanged();
                }
            }
        }

        #endregion

        #region SearchString

        private string _searchString = string.Empty;
        /// <summary>
        /// Gets or sets key word used for search department, category, brand.
        /// </summary>
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    OnPropertyChanged(() => SearchString);
                    OnSearchStringChanged();
                }
            }
        }

        #endregion

        #region VisibilityTabControl

        private Visibility _visibilityTabControl = Visibility.Visible;
        /// <summary>
        /// Show or not show TabControl.
        /// </summary>
        public Visibility VisibilityTabControl
        {
            get
            {
                return _visibilityTabControl;
            }
            set
            {
                if (_visibilityTabControl != value)
                {
                    _visibilityTabControl = value;
                    OnPropertyChanged(() => VisibilityTabControl);
                }
            }
        }

        #endregion

        #region VisibilityCategoriesTabItem

        private Visibility _visibilityCategoriesTabItem = Visibility.Visible;
        /// <summary>
        /// Show or not show 'All Categories' TabItem.
        /// </summary>
        public Visibility VisibilityCategoriesTabItem
        {
            get
            {
                return _visibilityCategoriesTabItem;
            }
            set
            {
                if (_visibilityCategoriesTabItem != value)
                {
                    _visibilityCategoriesTabItem = value;
                    OnPropertyChanged(() => VisibilityCategoriesTabItem);
                }
            }
        }

        #endregion

        #region VisibilityAllPart

        private Visibility _visibilityAllPart = Visibility.Collapsed;
        /// <summary>
        /// Show or not show Grid that contains deparment, category, and brand part.
        /// </summary>
        public Visibility VisibilityAllPart
        {
            get
            {
                return _visibilityAllPart;
            }
            set
            {
                if (_visibilityAllPart != value)
                {
                    _visibilityAllPart = value;
                    OnPropertyChanged(() => VisibilityAllPart);
                }
            }
        }

        #endregion

        #region VisibilityDepartmentPart

        private Visibility _visibilityDepartmentPart = Visibility.Collapsed;
        /// <summary>
        /// Show or not show create new or edit part of deparment.
        /// </summary>
        public Visibility VisibilityDepartmentPart
        {
            get
            {
                return _visibilityDepartmentPart;
            }
            set
            {
                if (_visibilityDepartmentPart != value)
                {
                    _visibilityDepartmentPart = value;
                    OnPropertyChanged(() => VisibilityDepartmentPart);
                }
            }
        }

        #endregion

        #region VisibilityCategoryPart

        private Visibility _visibilityCategoryPart = Visibility.Collapsed;
        /// <summary>
        /// Show or not show create new or edit part of category.
        /// </summary>
        public Visibility VisibilityCategoryPart
        {
            get
            {
                return _visibilityCategoryPart;
            }
            set
            {
                if (_visibilityCategoryPart != value)
                {
                    _visibilityCategoryPart = value;
                    OnPropertyChanged(() => VisibilityCategoryPart);
                }
            }
        }

        #endregion

        #region VisibilityBrandPart

        private Visibility _visibilityBrandPart = Visibility.Collapsed;
        /// <summary>
        /// Show or not show create new or edit part of brand.
        /// </summary>
        public Visibility VisibilityBrandPart
        {
            get
            {
                return _visibilityBrandPart;
            }
            set
            {
                if (_visibilityBrandPart != value)
                {
                    _visibilityBrandPart = value;
                    OnPropertyChanged(() => VisibilityBrandPart);
                }
            }
        }

        #endregion

        #region HeaderDepartmentPart

        private string _headerDepartmentPart;
        /// <summary>
        /// Header of create new or edit part of deparment.
        /// </summary>
        public string HeaderDepartmentPart
        {
            get
            {
                return _headerDepartmentPart;
            }
            set
            {
                if (_headerDepartmentPart != value)
                {
                    _headerDepartmentPart = value;
                    OnPropertyChanged(() => HeaderDepartmentPart);
                }
            }
        }

        #endregion

        #region HeaderCategoryPart

        private string _headerCategoryPart;
        /// <summary>
        /// Header of create new or edit part of category.
        /// </summary>
        public string HeaderCategoryPart
        {
            get
            {
                return _headerCategoryPart;
            }
            set
            {
                if (_headerCategoryPart != value)
                {
                    _headerCategoryPart = value;
                    OnPropertyChanged(() => HeaderCategoryPart);
                }
            }
        }

        #endregion

        #region HeaderBrandPart

        private string _headerBrandPart;
        /// <summary>
        /// Header of create new or edit part of brand.
        /// </summary>
        public string HeaderBrandPart
        {
            get
            {
                return _headerBrandPart;
            }
            set
            {
                if (_headerBrandPart != value)
                {
                    _headerBrandPart = value;
                    OnPropertyChanged(() => HeaderBrandPart);
                }
            }
        }

        #endregion

        #region IsEnableSearchPart

        private bool _isEnableSearchPart = true;
        /// <summary>
        /// Search part is enabled in the user interface or not.
        /// </summary>
        public bool IsEnableSearchPart
        {
            get
            {
                return _isEnableSearchPart;
            }
            set
            {
                if (_isEnableSearchPart != value)
                {
                    _isEnableSearchPart = value;
                    OnPropertyChanged(() => IsEnableSearchPart);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SelectedItemChangedCommand

        private ICommand _selectedItemChangedCommand;
        /// <summary>
        /// When event SelectedItemChanged on TreeView occurs, SelectedItemChangedCommand will executes.
        /// </summary>
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (_selectedItemChangedCommand == null)
                {
                    _selectedItemChangedCommand = new RelayCommand<TreeView>(SelectedItemChangedCommandExecute);
                }
                return _selectedItemChangedCommand;
            }
        }

        #endregion

        #region NewCommand

        private ICommand _newCommand;
        /// <summary>
        /// Create new department, category, brand.
        /// </summary>
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(NewExecute, CanNewExecute);
                }
                return _newCommand;
            }
        }

        #endregion

        #region EditCommand

        private ICommand _editCommand;
        /// <summary>
        /// Edit department, category, brand.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(EditExecute, CanEditExecute);
                }
                return _editCommand;
            }
        }

        #endregion

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// Save department, category, brand.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand<string>(SaveExecute, CanSaveExecute);
                }
                return _saveCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// Restore categories or close department part, category part, brand part.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute, CanCancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #region GetProductsCommand

        private ICommand _getProductsCommand;
        /// <summary>
        /// Gets products by page.
        /// </summary>
        public ICommand GetProductsCommand
        {
            get
            {
                if (_getProductsCommand == null)
                {
                    _getProductsCommand = new RelayCommand<object>(GetProductsExecute, CanGetProductsExecute);
                }
                return _getProductsCommand;
            }
        }

        #endregion

        #region EditCategoryCommand

        private ICommand _editCategoryCommand;
        /// <summary>
        /// Occurs when user double click on DataGridRow that contains categories.
        /// </summary>
        public ICommand EditCategoryCommand
        {
            get
            {
                if (_editCategoryCommand == null)
                {
                    _editCategoryCommand = new RelayCommand<object>(EditCategoryExecute);
                }
                return _editCategoryCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SelectedItemChangedCommandExecute

        /// <summary>
        /// Corresponds with event SelectedItemChanged on TreeView.
        /// </summary>
        private void SelectedItemChangedCommandExecute(TreeView treeView)
        {
            // Gets SelectedItem on TreeView.
            SelectedItem = treeView.SelectedItem;
            ShowOrHiddenCategoriesTabItem();
        }

        #endregion

        #region NewExecute

        /// <summary>
        /// Create new department, category, brand.
        /// </summary>
        private void NewExecute()
        {
            // Save or restore when categories is dirty.
            SaveOrRestoreCategories();

            if (SelectedItem is DepartmentRootModel)
            {
                ShowDeparmentPart(true);
                ProductDepartmentModel = new ProductDepartmentModel()
                {
                    ProductCategoryCollection = new CollectionBase<ProductCategoryModel>()
                };
                _isCreateElement = true;
                _isEditingElement = false;
            }
            else if (SelectedItem is ProductDepartmentModel)
            {
                // Check save department before create new category.
                bool isShowCategoryPart = true;
                if (ProductDepartmentModel != null)
                {
                    isShowCategoryPart = SaveProductDeparmentWithQuestion();
                }
                if (isShowCategoryPart)
                {
                    ShowCategoryPart(true);
                    base_SaleTaxLocationModel defaultTaxCodeItem = _saleTaxLocations.FirstOrDefault(x => string.Compare(x.TaxCode, _defaultTaxCode, false) == 0);
                    ProductDepartmentModel deparment = SelectedItem as ProductDepartmentModel;
                    ProductCategoryModel = new ProductCategoryModel()
                    {
                        ProductDepartmentModel = deparment,
                        ProductDepartmentID = deparment.ProductDepartmentID,
                        ProductBrandCollection = new CollectionBase<ProductBrandModel>(),
                        TaxCodeId = defaultTaxCodeItem != null ? defaultTaxCodeItem.TaxCode : null
                    };
                    _isCreateElement = true;
                    _isEditingElement = false;
                }
            }
            else if (SelectedItem is ProductCategoryModel)
            {
                // Check save category before create new brand.
                bool isShowBrandPart = true;
                if (ProductCategoryModel != null)
                {
                    isShowBrandPart = SaveProductCategoryWithQuestion();
                }
                if (isShowBrandPart)
                {
                    ShowBrandPart(true);
                    ProductCategoryModel category = SelectedItem as ProductCategoryModel;
                    ProductBrandModel = new ProductBrandModel
                    {
                        ProductCategoryModel = category,
                        ProductCategoryID = category.ProductCategoryID
                    };
                    _isCreateElement = true;
                    _isEditingElement = false;
                }
            }
        }

        #endregion

        #region CanNewExecute

        private bool CanNewExecute()
        {
            if (_selectedItem == null || _selectedItem is ProductBrandModel || _isCreateElement)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region EditExecute

        /// <summary>
        /// Edit department, category, brand.
        /// </summary>
        private void EditExecute()
        {
            // Save or restore when categories is dirty.
            SaveOrRestoreCategories();

            if (SelectedItem is ProductDepartmentModel)
            {
                // Check save category before edit department.
                bool isShowDeparmentPart = true;
                if (ProductCategoryModel != null)
                {
                    isShowDeparmentPart = SaveProductCategoryWithQuestion();
                }
                if (isShowDeparmentPart)
                {
                    ShowDeparmentPart(false);
                    ProductDepartmentModel = (_selectedItem as ProductDepartmentModel).ShallowClone();
                    _isEditingElement = true;
                    _isCreateElement = false;
                }
            }
            else if (SelectedItem is ProductCategoryModel)
            {
                // Check save brand before edit category.
                bool isShowCategoryPart = true;
                if (ProductBrandModel != null)
                {
                    isShowCategoryPart = SaveProductBrandWithQuestion();
                }
                if (isShowCategoryPart)
                {
                    ShowCategoryPart(false);
                    ProductCategoryModel = (_selectedItem as ProductCategoryModel).ShallowClone();
                    _isEditingElement = true;
                    _isCreateElement = false;
                }
            }
            else if (SelectedItem is ProductBrandModel)
            {
                ShowBrandPart(false);
                ProductBrandModel = (_selectedItem as ProductBrandModel).ShallowClone();
                _isEditingElement = true;
                _isCreateElement = false;
            }
        }

        #endregion

        #region CanEditExecute

        private bool CanEditExecute()
        {
            if (_selectedItem == null || _selectedItem is DepartmentRootModel || _isEditingElement)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region SaveExecute

        /// <summary>
        /// Save department, category, brand.
        /// </summary>
        private void SaveExecute(string parameter)
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        private bool CanSaveExecute(string parameter)
        {
            if (_currentPart == Parts.Main)
            {
                if (_selectedItem == null || _selectedItem is ProductCategoryModel || _selectedItem is ProductBrandModel)
                {
                    return false;
                }

                if (_selectedItem is DepartmentRootModel)
                {
                    DepartmentRootModel depRoot = _selectedItem as DepartmentRootModel;
                    if (depRoot.ProductCategoryCollection == null || !depRoot.ProductCategoryCollection.IsDirty)
                    {
                        return false;
                    }
                    return true;
                }
                else if (_selectedItem is ProductDepartmentModel)
                {
                    ProductDepartmentModel deparment = _selectedItem as ProductDepartmentModel;
                    if (deparment.ProductCategoryCollection == null || !deparment.ProductCategoryCollection.IsDirty)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (_currentPart == Parts.Deparment)
            {
                if (ProductDepartmentModel == null || (!ProductDepartmentModel.IsNew && !ProductDepartmentModel.IsDirty) ||
                    !string.IsNullOrWhiteSpace(ProductDepartmentModel.Error))
                {
                    return false;
                }

                return true;
            }
            else if (_currentPart == Parts.Category)
            {
                if (ProductCategoryModel == null || (!ProductCategoryModel.IsNew && !ProductCategoryModel.IsDirty) ||
                    !string.IsNullOrWhiteSpace(ProductCategoryModel.Error))
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (ProductBrandModel == null || (!ProductBrandModel.IsNew && !ProductBrandModel.IsDirty) ||
                    !string.IsNullOrWhiteSpace(ProductBrandModel.Error))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Restore categories or close department part, category part, brand part.
        /// </summary>
        private void CancelExecute()
        {
            switch (_currentPart)
            {
                case Parts.Main:

                    if (_selectedItem is DepartmentRootModel)
                    {
                        DepartmentRootModel depRoot = _selectedItem as DepartmentRootModel;
                        RestoreCatagories(depRoot.ProductCategoryCollection);
                    }
                    else if (_selectedItem is ProductDepartmentModel)
                    {
                        ProductDepartmentModel deparment = _selectedItem as ProductDepartmentModel;
                        RestoreCatagories(deparment.ProductCategoryCollection);
                    }

                    break;

                case Parts.Deparment:

                    bool isCloseCurrentPart = SaveProductDeparmentWithQuestion();
                    if (isCloseCurrentPart)
                    {
                        CloseCurrentPart();
                    }

                    break;

                case Parts.Category:

                    isCloseCurrentPart = SaveProductCategoryWithQuestion();
                    if (isCloseCurrentPart)
                    {
                        CloseCurrentPart();
                    }

                    break;

                case Parts.Brand:

                    isCloseCurrentPart = SaveProductBrandWithQuestion();
                    if (isCloseCurrentPart)
                    {
                        CloseCurrentPart();
                    }

                    break;
            }
        }

        #endregion

        #region CanCancelExecute

        private bool CanCancelExecute()
        {
            if (_currentPart == Parts.Main)
            {
                if (_selectedItem == null || _selectedItem is ProductCategoryModel || _selectedItem is ProductBrandModel)
                {
                    return false;
                }

                if (_selectedItem is DepartmentRootModel)
                {
                    DepartmentRootModel depRoot = _selectedItem as DepartmentRootModel;
                    if (depRoot.ProductCategoryCollection == null || !depRoot.ProductCategoryCollection.IsDirty)
                    {
                        return false;
                    }
                    return true;
                }
                else if (_selectedItem is ProductDepartmentModel)
                {
                    ProductDepartmentModel deparment = _selectedItem as ProductDepartmentModel;
                    if (deparment.ProductCategoryCollection == null || !deparment.ProductCategoryCollection.IsDirty)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region GetProductsExecute

        /// <summary>
        /// Gets a range product by page.
        /// </summary>
        private void GetProductsExecute(object parameter)
        {
            if (IsBusy)
            {
                return;
            }

            _backgroundWorker.RunWorkerAsync("GetProducts");
        }

        #endregion

        #region CanGetProductsExecute

        private bool CanGetProductsExecute(object parameter)
        {
            if (_selectedItem == null)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region EditCategoryExecute

        /// <summary>
        /// Occurs when user double click on DataGrid that contains categories.
        /// </summary>
        /// <param name="parameter"></param>
        private void EditCategoryExecute(object parameter)
        {
            // Fix error: 'Dispatcher processing has been suspended, but messages are still being processed.'.
            SaveOrRestoreCategories();

            // Get selected category.
            ProductCategoryModel cate = parameter as ProductCategoryModel;

            // Jumps to selected category.
            SelectBeforeItem(cate.ProductCategoryID);
            ShowOrHiddenCategoriesTabItem();

            // Show category part.
            ShowCategoryPart(false);
            ProductCategoryModel = cate.ShallowClone();
            _isEditingElement = true;
            _isCreateElement = false;
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnSelectedIndexChanged

        /// <summary>
        /// Occur after SelectedIndex property changed.
        /// </summary>
        private void OnSelectedIndexChanged()
        {
            // Gets products by page.
            if (_selectedIndex == 1 && SelectedItem != null)
            {
                if (SelectedItem is DepartmentRootModel)
                {
                    DepartmentRootModel deproot = SelectedItem as DepartmentRootModel;
                    if (!deproot.IsProductsLoaded)
                    {
                        _backgroundWorker.RunWorkerAsync("GetProducts");
                    }
                }
                else if (SelectedItem is ProductDepartmentModel)
                {
                    ProductDepartmentModel dep = SelectedItem as ProductDepartmentModel;
                    if (!dep.IsProductsLoaded)
                    {
                        _backgroundWorker.RunWorkerAsync("GetProducts");
                    }
                }
                else if (SelectedItem is ProductCategoryModel)
                {
                    ProductCategoryModel cate = SelectedItem as ProductCategoryModel;
                    if (!cate.IsProductsLoaded)
                    {
                        _backgroundWorker.RunWorkerAsync("GetProducts");
                    }
                }
                else
                {
                    ProductBrandModel brand = SelectedItem as ProductBrandModel;
                    if (!brand.IsProductsLoaded)
                    {
                        _backgroundWorker.RunWorkerAsync("GetProducts");
                    }
                }
            }
        }

        #endregion

        #region OnSelectedItemChanging

        /// <summary>
        /// Occur before SelectedItem property changed.
        /// </summary>
        private void OnSelectedItemChanging()
        {
            if (_selectedItem != null)
            {
                SaveOrRestoreCategories();
            }

            // Fix error: 'DeferRefresh' is not allowed during an AddNew or EditItem transaction.
            ListCollectionView categoryView = CollectionViewSource.GetDefaultView(ProductCategoryCollection) as ListCollectionView;
            if (categoryView != null && categoryView.IsEditingItem)
            {
                categoryView.CommitEdit();
            }
        }

        #endregion

        #region OnSelectedItemChanged

        /// <summary>
        /// Occur after SelectedItem property changed.
        /// </summary>
        private void OnSelectedItemChanged()
        {
            if (_selectedItem != null)
            {
                if (_selectedItem is DepartmentRootModel)
                {
                    ProductCategoryCollection = (_selectedItem as DepartmentRootModel).ProductCategoryCollection;
                }
                else if (_selectedItem is ProductDepartmentModel)
                {
                    ProductCategoryCollection = (_selectedItem as ProductDepartmentModel).ProductCategoryCollection;
                }
                else
                {
                    ProductCategoryCollection = null;
                }
            }
            else
            {
                ProductCategoryCollection = null;
            }
        }

        #endregion

        #region OnSearchStringChanged

        /// <summary>
        /// Occur after SearchString property changed.
        /// </summary>
        private void OnSearchStringChanged()
        {
            Search(_searchString);
        }

        #endregion

        #endregion

        #region Private Methods

        #region GetSaleTaxLocations

        /// <summary>
        /// Gets SaleTaxLocations
        /// </summary>
        private void GetSaleTaxLocations()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_SaleTaxLocationRepository saleTaxLocationRepository = new base_SaleTaxLocationRepository();

                    //Get SaleTaxLocation primary.
                    base_SaleTaxLocation saleTaxLocationPrimary = saleTaxLocationRepository.Get(x => x.IsPrimary && x.IsActived);
                    if (saleTaxLocationPrimary != null)
                    {
                        SaleTaxLocations = new CollectionBase<base_SaleTaxLocationModel>(saleTaxLocationRepository.GetAll(x =>
                            x.ParentId == saleTaxLocationPrimary.Id).Select(x => new base_SaleTaxLocationModel(x)));
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetProductDepartments

        /// <summary>
        /// Get all ProductDepartments.
        /// </summary>
        private void GetProductDepartments()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                    // Get all ProductDepartments that have LevelId = 0.
                    IList<base_Department> departments = departmentRepository.GetAll(x => x.LevelId == Define.ProductDeparmentLevel);

                    foreach (base_Department deparment in departments)
                    {
                        if (departmentRepository.Refresh(deparment) != null)
                        {
                            _backgroundWorker.ReportProgress(0, deparment);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region CreateDepartmentCollection

        /// <summary>
        /// Initialize DepartmentCollection and add element root.
        /// </summary>
        private void CreateDepartmentCollection()
        {
            DepartmentCollection = new CollectionBase<DepartmentRootModel>();

            // Add root element.
            DepartmentRootModel departmentRootModel = new DepartmentRootModel
            {
                Name = "All Departments",
                ProductDepartmentCollection = new CollectionBase<ProductDepartmentModel>(),
                ProductCategoryCollection = new CollectionBase<ProductCategoryModel>(),
            };
            DepartmentCollection.Add(departmentRootModel);
        }

        #endregion

        #region CreateProductDepartments

        /// <summary>
        /// Creates ProductDepartment, ProductCategory, ProductBrand on decentralization model.
        /// </summary>
        private void CreateProductDepartments(base_Department deparment)
        {
            lock (UnitOfWork.Locker)
            {
                base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                // Gets root element.
                DepartmentRootModel departmentRootModel = _departmentCollection.First();

                ProductDepartmentModel productDepartmentModel;
                ProductCategoryModel productCategoryModel;
                ProductBrandModel productBrandModel;

                // Initialize ProductDepartmentModel.
                productDepartmentModel = new ProductDepartmentModel(deparment)
                {
                    ProductCategoryCollection = new CollectionBase<ProductCategoryModel>(),
                    IsNew = false,
                    IsDirty = false
                };

                // For each ProductCategory in ProductDepartment.
                departmentRepository.Refresh(deparment.base_Department1);
                foreach (base_Department category in deparment.base_Department1)
                {
                    if (category.LevelId != Define.ProductCategoryLevel)
                    {
                        continue;
                    }

                    // Initialize ProductCategoryModel.
                    productCategoryModel = new ProductCategoryModel(category)
                    {
                        ProductDepartmentModel = productDepartmentModel,
                        ProductBrandCollection = new CollectionBase<ProductBrandModel>(),
                        IsNew = false,
                        IsDirty = false
                    };

                    // For each ProductBrand in ProductCategory.
                    departmentRepository.Refresh(category.base_Department1);
                    foreach (base_Department brand in category.base_Department1)
                    {
                        if (brand.LevelId != Define.ProductBrandLevel)
                        {
                            continue;
                        }

                        // Initialize ProductBrandModel.
                        productBrandModel = new ProductBrandModel(brand)
                        {
                            ProductCategoryModel = productCategoryModel,
                            IsNew = false,
                            IsDirty = false
                        };
                        productCategoryModel.ProductBrandCollection.Add(productBrandModel);
                    }

                    productDepartmentModel.ProductCategoryCollection.Add(productCategoryModel);
                    departmentRootModel.ProductCategoryCollection.Add(productCategoryModel);
                }

                departmentRootModel.ProductDepartmentCollection.Add(productDepartmentModel);
            }
        }

        #endregion

        #region ShowOrHiddenCategoriesTab

        /// <summary>
        /// Show or hidden 'All categories' TabItem.
        /// </summary>
        private void ShowOrHiddenCategoriesTabItem()
        {
            if (_selectedItem == null)
            {
                return;
            }

            if (_selectedItem is DepartmentRootModel || _selectedItem is ProductDepartmentModel)
            {
                // Show 'All Categories' TabItem.
                VisibilityCategoriesTabItem = Visibility.Visible;
                _selectedIndex = 0;
                OnPropertyChanged(() => SelectedIndex);
            }
            else
            {
                // Hidden 'All Categories' TabItem.
                VisibilityCategoriesTabItem = Visibility.Collapsed;
                _selectedIndex = 1;
                OnPropertyChanged(() => SelectedIndex);

                if (SelectedItem is ProductCategoryModel)
                {
                    ProductCategoryModel cate = SelectedItem as ProductCategoryModel;
                    if (!cate.IsProductsLoaded)
                    {
                        _backgroundWorker.RunWorkerAsync("GetProducts");
                    }
                }
                else if (SelectedItem is ProductBrandModel)
                {
                    ProductBrandModel brand = SelectedItem as ProductBrandModel;
                    if (!brand.IsProductsLoaded)
                    {
                        _backgroundWorker.RunWorkerAsync("GetProducts");
                    }
                }
            }
        }

        #endregion

        #region ShowDeparmentPart

        /// <summary>
        /// Show create new or edit part of deparment.
        /// </summary>
        private void ShowDeparmentPart(bool isCreateNew)
        {
            VisibilityTabControl = Visibility.Collapsed;
            VisibilityCategoryPart = Visibility.Collapsed;
            VisibilityBrandPart = Visibility.Collapsed;
            VisibilityAllPart = Visibility.Visible;
            VisibilityDepartmentPart = Visibility.Visible;
            IsEnableSearchPart = false;
            _currentPart = Parts.Deparment;
            if (isCreateNew)
            {
                HeaderDepartmentPart = "Creates New Department";
            }
            else
            {
                HeaderDepartmentPart = "Edit Department";
            }
        }

        #endregion

        #region ShowCategoryPart

        /// <summary>
        /// Show create new or edit part of category.
        /// </summary>
        private void ShowCategoryPart(bool isCreateNew)
        {
            VisibilityTabControl = Visibility.Collapsed;
            VisibilityDepartmentPart = Visibility.Collapsed;
            VisibilityBrandPart = Visibility.Collapsed;
            VisibilityAllPart = Visibility.Visible;
            VisibilityCategoryPart = Visibility.Visible;
            IsEnableSearchPart = false;
            _currentPart = Parts.Category;
            if (isCreateNew)
            {
                HeaderCategoryPart = "Creates New Category";
            }
            else
            {
                HeaderCategoryPart = "Edit Category";
            }
        }

        #endregion

        #region ShowBrandPart

        /// <summary>
        /// Show create new or edit part of category.
        /// </summary>
        private void ShowBrandPart(bool isCreateNew)
        {
            VisibilityTabControl = Visibility.Collapsed;
            VisibilityDepartmentPart = Visibility.Collapsed;
            VisibilityCategoryPart = Visibility.Collapsed;
            VisibilityAllPart = Visibility.Visible;
            VisibilityBrandPart = Visibility.Visible;
            IsEnableSearchPart = false;
            _currentPart = Parts.Brand;
            if (isCreateNew)
            {
                HeaderBrandPart = "Creates New Brand";
            }
            else
            {
                HeaderBrandPart = "Edit Brand";
            }
        }

        #endregion

        #region CloseCurrentPart

        /// <summary>
        /// Close department, category and brand part. Show TabControl that contains categories and products.
        /// </summary>
        private void CloseCurrentPart()
        {
            VisibilityTabControl = Visibility.Visible;
            VisibilityAllPart = Visibility.Collapsed;
            VisibilityCategoryPart = Visibility.Collapsed;
            VisibilityBrandPart = Visibility.Collapsed;
            VisibilityDepartmentPart = Visibility.Collapsed;
            IsEnableSearchPart = true;
            _currentPart = Parts.Main;
            _isCreateElement = false;
            _isEditingElement = false;
            ProductDepartmentModel = null;
            ProductCategoryModel = null;
            ProductBrandModel = null;
        }

        #endregion

        #region Search

        /// <summary>
        /// Search department, category, brand.
        /// </summary>
        /// <param name="keyword">Keyword to search.</param>
        private void Search(string keyword)
        {
            DepartmentRootModel departmentRootModel = DepartmentCollection.First();
            ListCollectionView departmentCollectionView;
            ListCollectionView categoryCollectionView;
            ListCollectionView brandCollectionView;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ClearSearch(departmentRootModel);
                return;
            }

            // Try search in department.
            departmentCollectionView = CollectionViewSource.GetDefaultView(departmentRootModel.ProductDepartmentCollection) as ListCollectionView;
            departmentCollectionView.Filter = (dep) =>
            {
                ProductDepartmentModel depTemp = dep as ProductDepartmentModel;
                if (depTemp.Name != null && depTemp.Name.ToLower().Contains(_searchString.ToLower()))
                {
                    return true;
                }
                else
                {
                    // Try search in category.
                    categoryCollectionView = CollectionViewSource.GetDefaultView(depTemp.ProductCategoryCollection) as ListCollectionView;
                    categoryCollectionView.Filter = (cate) =>
                    {
                        ProductCategoryModel cateTemp = cate as ProductCategoryModel;
                        if (cateTemp.Name != null && cateTemp.Name.ToLower().Contains(_searchString.ToLower()))
                        {
                            return true;
                        }
                        else
                        {
                            // Try search in brand.
                            brandCollectionView = CollectionViewSource.GetDefaultView(cateTemp.ProductBrandCollection) as ListCollectionView;
                            brandCollectionView.Filter = (brand) =>
                            {
                                ProductBrandModel brandTemp = brand as ProductBrandModel;
                                if (brandTemp.Name == null)
                                {
                                    return false;
                                }
                                return brandTemp.Name.ToLower().Contains(_searchString.ToLower());
                            };

                            return brandCollectionView.Count > 0;
                        }
                    };

                    return categoryCollectionView.Count > 0;
                }
            };
        }

        #endregion

        #region ClearSearch

        /// <summary>
        /// Show all item, not filter.
        /// </summary>
        private void ClearSearch(DepartmentRootModel departmentRootModel)
        {
            ListCollectionView departmentCollectionView;
            ListCollectionView categoryCollectionView;
            ListCollectionView brandCollectionView;

            departmentCollectionView = CollectionViewSource.GetDefaultView(departmentRootModel.ProductDepartmentCollection) as ListCollectionView;
            departmentCollectionView.Filter = null;

            foreach (var dep in departmentCollectionView)
            {
                ProductDepartmentModel depTemp = dep as ProductDepartmentModel;
                categoryCollectionView = CollectionViewSource.GetDefaultView(depTemp.ProductCategoryCollection) as ListCollectionView;
                categoryCollectionView.Filter = null;

                foreach (var cate in categoryCollectionView)
                {
                    ProductCategoryModel cateTemp = cate as ProductCategoryModel;
                    brandCollectionView = CollectionViewSource.GetDefaultView(cateTemp.ProductBrandCollection) as ListCollectionView;
                    brandCollectionView.Filter = null;
                }
            }
        }

        #endregion

        #region GetProducts

        /// <summary>
        /// Gets a range product by page.
        /// </summary>
        private void GetProducts()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_ProductRepository productRepository = new base_ProductRepository();
                    IList<base_Product> products;

                    if (SelectedItem is DepartmentRootModel)
                    {
                        DepartmentRootModel deproot = SelectedItem as DepartmentRootModel;
                        if (!deproot.IsProductsLoaded)
                        {
                            deproot.ProductCollection = new CollectionBase<base_ProductModel>();
                            deproot.ProductTotal = productRepository.GetIQueryable().Count();
                            deproot.IsDirty = false;
                        }
                        products = productRepository.GetRange(deproot.ProductCollection.Count, NumberOfDisplayItems, _productColumnSort);
                        foreach (base_Product product in products)
                        {
                            if (productRepository.Refresh(product) != null)
                            {
                                _backgroundWorker.ReportProgress(0, product);
                            }
                        }
                    }
                    else if (SelectedItem is ProductDepartmentModel)
                    {
                        ProductDepartmentModel dep = SelectedItem as ProductDepartmentModel;
                        if (!dep.IsProductsLoaded)
                        {
                            dep.ProductCollection = new CollectionBase<base_ProductModel>();
                            dep.ProductTotal = productRepository.GetIQueryable(x => x.ProductDepartmentId == dep.ProductDepartmentID).Count();
                            dep.IsDirty = false;
                        }
                        products = productRepository.GetRange(dep.ProductCollection.Count, NumberOfDisplayItems, _productColumnSort, x => x.ProductDepartmentId == dep.ProductDepartmentID);
                        foreach (base_Product product in products)
                        {
                            if (productRepository.Refresh(product) != null)
                            {
                                _backgroundWorker.ReportProgress(0, product);
                            }
                        }
                    }
                    else if (SelectedItem is ProductCategoryModel)
                    {
                        ProductCategoryModel cate = SelectedItem as ProductCategoryModel;
                        if (!cate.IsProductsLoaded)
                        {
                            cate.ProductCollection = new CollectionBase<base_ProductModel>();
                            cate.ProductTotal = productRepository.GetIQueryable(x => x.ProductCategoryId == cate.ProductCategoryID).Count();
                            cate.IsDirty = false;
                        }
                        products = productRepository.GetRange(cate.ProductCollection.Count, NumberOfDisplayItems, _productColumnSort, x => x.ProductCategoryId == cate.ProductCategoryID);
                        foreach (base_Product product in products)
                        {
                            if (productRepository.Refresh(product) != null)
                            {
                                _backgroundWorker.ReportProgress(0, product);
                            }
                        }
                    }
                    else
                    {
                        ProductBrandModel brand = SelectedItem as ProductBrandModel;
                        if (!brand.IsProductsLoaded)
                        {
                            brand.ProductCollection = new CollectionBase<base_ProductModel>();
                            brand.ProductTotal = productRepository.GetIQueryable(x => x.ProductBrandId == brand.ProductBrandID).Count();
                            brand.IsDirty = false;
                        }
                        products = productRepository.GetRange(brand.ProductCollection.Count, NumberOfDisplayItems, _productColumnSort, x => x.ProductBrandId == brand.ProductBrandID);
                        foreach (base_Product product in products)
                        {
                            if (productRepository.Refresh(product) != null)
                            {
                                _backgroundWorker.ReportProgress(0, product);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region CreateProducts

        /// <summary>
        /// Create products.
        /// </summary>
        private void CreateProducts(base_Product product)
        {
            if (SelectedItem is DepartmentRootModel)
            {
                DepartmentRootModel deproot = SelectedItem as DepartmentRootModel;
                if (!deproot.IsProductsLoaded)
                {
                    deproot.ProductCollection = new CollectionBase<base_ProductModel>();
                }
                if (deproot.ProductCollection.FirstOrDefault(x => x.Id == product.Id) == null)
                {
                    deproot.ProductCollection.Add(new base_ProductModel(product));
                }
            }
            else if (SelectedItem is ProductDepartmentModel)
            {
                ProductDepartmentModel dep = SelectedItem as ProductDepartmentModel;
                if (!dep.IsProductsLoaded)
                {
                    dep.ProductCollection = new CollectionBase<base_ProductModel>();
                }
                if (dep.ProductCollection.FirstOrDefault(x => x.Id == product.Id) == null)
                {
                    dep.ProductCollection.Add(new base_ProductModel(product));
                }
            }
            else if (SelectedItem is ProductCategoryModel)
            {
                ProductCategoryModel cate = SelectedItem as ProductCategoryModel;
                if (!cate.IsProductsLoaded)
                {
                    cate.ProductCollection = new CollectionBase<base_ProductModel>();
                }
                if (cate.ProductCollection.FirstOrDefault(x => x.Id == product.Id) == null)
                {
                    cate.ProductCollection.Add(new base_ProductModel(product));
                }
            }
            else
            {
                ProductBrandModel brand = SelectedItem as ProductBrandModel;
                if (!brand.IsProductsLoaded)
                {
                    brand.ProductCollection = new CollectionBase<base_ProductModel>();
                }
                if (brand.ProductCollection.FirstOrDefault(x => x.Id == product.Id) == null)
                {
                    brand.ProductCollection.Add(new base_ProductModel(product));
                }
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save department, category, brand.
        /// </summary>
        private void Save()
        {
            try
            {
                switch (_currentPart)
                {
                    case Parts.Main:

                        SaveCategories();

                        break;

                    case Parts.Deparment:

                        SaveProductDeparment();

                        break;

                    case Parts.Category:

                        SaveProductCategory();

                        break;

                    case Parts.Brand:

                        SaveProductBrand();

                        break;
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region SaveProductDeparment

        /// <summary>
        /// Insert, Update deparment.
        /// </summary>
        private void SaveProductDeparment()
        {
            base_DepartmentRepository departmentRepository = new base_DepartmentRepository();
            DateTime now = DateTime.Now;

            if (ProductDepartmentModel.IsNew)
            {
                ProductDepartmentModel.DateCreated = now;
                ProductDepartmentModel.DateUpdated = now;
                ProductDepartmentModel.ToEntity();
                departmentRepository.Add(ProductDepartmentModel.base_Department);
                departmentRepository.Commit();

                ProductDepartmentModel.ProductDepartmentID = ProductDepartmentModel.base_Department.Id;
                ProductDepartmentModel.IsNew = false;
                ProductDepartmentModel.IsDirty = false;
                if (SelectedItem is DepartmentRootModel)
                {
                    (SelectedItem as DepartmentRootModel).ProductDepartmentCollection.Add(ProductDepartmentModel);
                }
                else
                {
                    _departmentCollection.First().ProductDepartmentCollection.Add(ProductDepartmentModel);
                }

                ProductDepartmentModel = new ProductDepartmentModel()
                {
                    ProductCategoryCollection = new CollectionBase<ProductCategoryModel>()
                };
            }
            else
            {
                ProductDepartmentModel.DateUpdated = now;
                ProductDepartmentModel.ToEntity();
                departmentRepository.Commit();

                ProductDepartmentModel department = SelectedItem as ProductDepartmentModel;
                department.Name = ProductDepartmentModel.Name;
                department.IsActived = ProductDepartmentModel.IsActived;
                department.IsDirty = false;

                CloseCurrentPart();
            }
        }

        #endregion

        #region SaveProductDeparmentWithQuestion

        private bool SaveProductDeparmentWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (string.IsNullOrWhiteSpace(ProductDepartmentModel.Error))
            {
                if (ProductDepartmentModel.IsNew || (!ProductDepartmentModel.IsNew && ProductDepartmentModel.IsDirty))
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveProductDeparment();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region SaveProductCategory

        /// <summary>
        /// Insert, Update category.
        /// </summary>
        private void SaveProductCategory()
        {
            base_DepartmentRepository departmentRepository = new base_DepartmentRepository();
            DateTime now = DateTime.Now;
            DepartmentRootModel departmentRootModel = DepartmentCollection.First() as DepartmentRootModel;

            if (ProductCategoryModel.IsNew)
            {
                ProductCategoryModel.DateCreated = now;
                ProductCategoryModel.DateUpdated = now;
                ProductCategoryModel.ToEntity();
                departmentRepository.Add(ProductCategoryModel.base_Department);
                departmentRepository.Commit();

                ProductCategoryModel.ProductCategoryID = ProductCategoryModel.base_Department.Id;
                ProductCategoryModel.IsNew = false;
                ProductCategoryModel.IsDirty = false;
                if (SelectedItem is ProductDepartmentModel)
                {
                    ProductDepartmentModel department = SelectedItem as ProductDepartmentModel;
                    department.ProductCategoryCollection.Add(ProductCategoryModel);
                    departmentRootModel.ProductCategoryCollection.Add(ProductCategoryModel);
                    ProductCategoryModel = new ProductCategoryModel()
                    {
                        ProductBrandCollection = new CollectionBase<ProductBrandModel>(),
                        ProductDepartmentModel = department,
                        ProductDepartmentID = department.ProductDepartmentID
                    };
                }
                else
                {
                    ProductCategoryModel category = SelectedItem as ProductCategoryModel;
                    category.ProductDepartmentModel.ProductCategoryCollection.Add(ProductCategoryModel);
                    departmentRootModel.ProductCategoryCollection.Add(ProductCategoryModel);
                    ProductCategoryModel = new ProductCategoryModel()
                    {
                        ProductBrandCollection = new CollectionBase<ProductBrandModel>(),
                        ProductDepartmentModel = category.ProductDepartmentModel,
                        ProductDepartmentID = category.ProductDepartmentModel.ProductDepartmentID
                    };
                }
            }
            else
            {
                ProductCategoryModel.DateUpdated = now;
                ProductCategoryModel.ToEntity();
                departmentRepository.Commit();

                ProductCategoryModel category = SelectedItem as ProductCategoryModel;
                category.Name = ProductCategoryModel.Name;
                category.TaxCodeId = ProductCategoryModel.TaxCodeId;
                category.Margin = ProductCategoryModel.Margin;
                category.MarkUp = ProductCategoryModel.MarkUp;
                category.IsActived = ProductCategoryModel.IsActived;
                category.IsDirty = false;

                CloseCurrentPart();
            }
        }

        #endregion

        #region SaveProductCategoryWithQuestion

        private bool SaveProductCategoryWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (string.IsNullOrWhiteSpace(ProductCategoryModel.Error))
            {
                if (ProductCategoryModel.IsNew || (!ProductCategoryModel.IsNew && ProductCategoryModel.IsDirty))
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveProductCategory();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region SaveProductBrand

        /// <summary>
        /// Insert, Update brand.
        /// </summary>
        private void SaveProductBrand()
        {
            base_DepartmentRepository departmentRepository = new base_DepartmentRepository();
            DateTime now = DateTime.Now;

            if (ProductBrandModel.IsNew)
            {
                ProductBrandModel.DateCreated = now;
                ProductBrandModel.DateUpdated = now;
                ProductBrandModel.ToEntity();
                departmentRepository.Add(ProductBrandModel.base_Department);
                departmentRepository.Commit();

                ProductBrandModel.ProductBrandID = ProductBrandModel.base_Department.Id;
                ProductBrandModel.IsNew = false;
                ProductBrandModel.IsDirty = false;
                if (SelectedItem is ProductCategoryModel)
                {
                    ProductCategoryModel category = SelectedItem as ProductCategoryModel;
                    category.ProductBrandCollection.Add(ProductBrandModel);
                    ProductBrandModel = new ProductBrandModel
                    {
                        ProductCategoryModel = category,
                        ProductCategoryID = category.ProductCategoryID
                    };
                }
                else
                {
                    ProductBrandModel brand = SelectedItem as ProductBrandModel;
                    brand.ProductCategoryModel.ProductBrandCollection.Add(ProductBrandModel);
                    ProductBrandModel = new ProductBrandModel
                    {
                        ProductCategoryModel = brand.ProductCategoryModel,
                        ProductCategoryID = brand.ProductCategoryModel.ProductCategoryID
                    };
                }
            }
            else
            {
                ProductBrandModel.DateUpdated = now;
                ProductBrandModel.ToEntity();
                departmentRepository.Commit();

                ProductBrandModel brand = SelectedItem as ProductBrandModel;
                brand.Name = ProductBrandModel.Name;
                brand.IsActived = ProductBrandModel.IsActived;
                brand.IsDirty = false;

                CloseCurrentPart();
            }
        }

        #endregion

        #region SaveProductBrandWithQuestion

        private bool SaveProductBrandWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (string.IsNullOrWhiteSpace(ProductBrandModel.Error))
            {
                if (ProductBrandModel.IsNew || (!ProductBrandModel.IsNew && ProductBrandModel.IsDirty))
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveProductBrand();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region RestoreCatagories

        /// <summary>
        /// Restore all items in category collection.
        /// </summary>
        /// <param name="categoryCollection">Category collection to restore.</param>
        private void RestoreCatagories(CollectionBase<ProductCategoryModel> categoryCollection)
        {
            if (categoryCollection.IsDirty)
            {
                foreach (ProductCategoryModel category in categoryCollection)
                {
                    if (!category.IsNew && category.IsDirty)
                    {
                        category.Restore();
                    }
                }
            }
        }

        #endregion

        #region SaveCategories

        /// <summary>
        /// Save all item in categories of deparment root or deparment.
        /// </summary>
        private void SaveCategories()
        {
            if (_selectedItem is DepartmentRootModel)
            {
                DepartmentRootModel depRoot = _selectedItem as DepartmentRootModel;
                SaveCategories(depRoot.ProductCategoryCollection);
            }
            else if (_selectedItem is ProductDepartmentModel)
            {
                ProductDepartmentModel deparment = _selectedItem as ProductDepartmentModel;
                SaveCategories(deparment.ProductCategoryCollection);
            }
        }

        /// <summary>
        /// Save all item in categories.
        /// </summary>
        /// <param name="categoryCollection"></param>
        private void SaveCategories(CollectionBase<ProductCategoryModel> categoryCollection)
        {
            try
            {
                if (categoryCollection.IsDirty)
                {
                    base_DepartmentRepository departmentRepository = new base_DepartmentRepository();
                    DateTime now = DateTime.Now;
                    foreach (ProductCategoryModel category in categoryCollection)
                    {
                        if (!category.IsNew && category.IsDirty)
                        {
                            category.DateUpdated = now;
                            category.ToEntity();
                            departmentRepository.Commit();
                            category.IsDirty = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region SaveOrRestoreCategories

        /// <summary>
        /// Save or restore all item in categories of deparment root or deparment.
        /// </summary>
        /// <returns>Return True to closes window.</returns>
        private bool SaveOrRestoreCategories()
        {
            if (_selectedItem is DepartmentRootModel)
            {
                DepartmentRootModel depRoot = _selectedItem as DepartmentRootModel;
                if (depRoot.ProductCategoryCollection.IsDirty)
                {
                    MessageBoxResult result = MessageBox.Show("Categories have been edited, do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        SaveCategories(depRoot.ProductCategoryCollection);
                    }
                    else
                    {
                        RestoreCatagories(depRoot.ProductCategoryCollection);
                    }
                }
            }
            else if (_selectedItem is ProductDepartmentModel)
            {
                ProductDepartmentModel department = _selectedItem as ProductDepartmentModel;
                if (department.ProductCategoryCollection.IsDirty)
                {
                    MessageBoxResult result = MessageBox.Show("Categories have been edited, do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        SaveCategories(department.ProductCategoryCollection);
                    }
                    else
                    {
                        RestoreCatagories(department.ProductCategoryCollection);
                    }
                }
            }

            return true;
        }

        #endregion

        #region SelectDefaultItem

        /// <summary>
        /// Select default item.
        /// </summary>
        /// <param name="item">Item set to default.</param>
        private void SelectDefaultItem(object item)
        {
            if (item == null)
            {
                return;
            }

            if (item is DepartmentRootModel)
            {
                DepartmentRootModel depRoot = item as DepartmentRootModel;
                depRoot.IsExpanded = true;
                depRoot.IsSelected = true;
                SelectedItem = depRoot;
            }
            else if (item is ProductDepartmentModel)
            {
                ProductDepartmentModel dep = item as ProductDepartmentModel;
                dep.IsExpanded = true;
                dep.IsSelected = true;
                SelectedItem = dep;
            }
            else if (item is ProductCategoryModel)
            {
                ProductCategoryModel cate = item as ProductCategoryModel;
                cate.IsExpanded = true;
                cate.IsSelected = true;
                SelectedItem = cate;
            }
            else
            {
                ProductBrandModel brand = item as ProductBrandModel;
                brand.IsSelected = true;
                SelectedItem = brand;
            }

            ShowOrHiddenCategoriesTabItem();
        }

        #endregion

        #region FindBeforeSelectedItem

        private object FindBeforeSelectedItem(int id)
        {
            foreach (ProductDepartmentModel dep in DepartmentCollection.First().ProductDepartmentCollection)
            {
                if (dep.ProductDepartmentID == id)
                {
                    return dep;
                }

                foreach (ProductCategoryModel cate in dep.ProductCategoryCollection)
                {
                    if (cate.ProductCategoryID == id)
                    {
                        return cate;
                    }

                    foreach (ProductBrandModel brand in cate.ProductBrandCollection)
                    {
                        if (brand.ProductBrandID == id)
                        {
                            return brand;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region SelectBeforeItem

        /// <summary>
        /// Select before selected item.
        /// </summary>
        /// <param name="id">Before selected item 'Id</param>
        private void SelectBeforeItem(int id)
        {
            object beforeItem = FindBeforeSelectedItem(id);
            if (beforeItem == null)
            {
                SelectDefaultItem(DepartmentCollection.First());
            }

            DepartmentCollection.First().IsExpanded = true;
            if (beforeItem is ProductDepartmentModel)
            {
                ProductDepartmentModel dep = beforeItem as ProductDepartmentModel;
                dep.IsExpanded = true;
                dep.IsSelected = true;
                SelectedItem = dep;
            }
            else if (beforeItem is ProductCategoryModel)
            {
                ProductCategoryModel cate = beforeItem as ProductCategoryModel;
                cate.ProductDepartmentModel.IsExpanded = true;
                cate.IsExpanded = true;
                cate.IsSelected = true;
                SelectedItem = cate;
            }
            else if (beforeItem is ProductBrandModel)
            {
                ProductBrandModel brand = beforeItem as ProductBrandModel;
                brand.ProductCategoryModel.ProductDepartmentModel.IsExpanded = true;
                brand.ProductCategoryModel.IsExpanded = true;
                brand.IsSelected = true;
                SelectedItem = brand;
            }
        }

        #endregion

        #endregion

        #region Override Methods

        #region LoadData

        /// <summary>
        /// Load all data.
        /// </summary>
        public override void LoadData()
        {
            CreateDepartmentCollection();
            GetSaleTaxLocations();

            _defaultTaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;

            // Get all ProductDepartments.
            _backgroundWorker.RunWorkerAsync("GetProductDepartments");
        }

        #endregion

        #region OnViewChangingCommandCanExecute

        /// <summary>
        /// Occur when view is closing or unactive.
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            bool isUnactive = true;

            try
            {
                switch (_currentPart)
                {
                    #region Main

                    case Parts.Main:

                        isUnactive = SaveOrRestoreCategories();

                        break;

                    #endregion

                    #region Deparment Part

                    case Parts.Deparment:

                        isUnactive = SaveProductDeparmentWithQuestion();

                        if (isUnactive)
                        {
                            CloseCurrentPart();
                        }

                        break;

                    #endregion

                    #region Category Part

                    case Parts.Category:

                        isUnactive = SaveProductCategoryWithQuestion();

                        if (isUnactive)
                        {
                            CloseCurrentPart();
                        }

                        break;

                    #endregion

                    #region Brand Part

                    case Parts.Brand:

                        isUnactive = SaveProductBrandWithQuestion();

                        if (isUnactive)
                        {
                            CloseCurrentPart();
                        }

                        break;

                    #endregion
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return isUnactive;
        }

        #endregion

        #endregion

        #region BackgroundWorker Events

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;

            switch (e.Argument as string)
            {
                case "GetProductDepartments":

                    GetProductDepartments();
                    e.Result = "GetProductDepartments";

                    break;

                case "GetProducts":

                    GetProducts();
                    e.Result = "GetProducts";

                    break;
            }
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is base_Department)
            {
                CreateProductDepartments(e.UserState as base_Department);
            }
            else if (e.UserState is base_Product)
            {
                CreateProducts(e.UserState as base_Product);
            }
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result.ToString() == "GetProductDepartments")
            {
                if (_selectedItem == null)
                {
                    SelectDefaultItem(DepartmentCollection.First());
                }
                else
                {
                    if (_selectedItem is DepartmentRootModel)
                    {
                        SelectDefaultItem(DepartmentCollection.First());
                    }
                    else
                    {
                        if (_selectedItem is ProductDepartmentModel)
                        {
                            SelectBeforeItem((_selectedItem as ProductDepartmentModel).ProductDepartmentID);
                        }
                        else if (_selectedItem is ProductCategoryModel)
                        {
                            SelectBeforeItem((_selectedItem as ProductCategoryModel).ProductCategoryID);
                        }
                        else if (_selectedItem is ProductBrandModel)
                        {
                            SelectBeforeItem((_selectedItem as ProductBrandModel).ProductBrandID);
                        }
                    }
                }
            }

            IsBusy = false;
        }

        #endregion
    }
}
