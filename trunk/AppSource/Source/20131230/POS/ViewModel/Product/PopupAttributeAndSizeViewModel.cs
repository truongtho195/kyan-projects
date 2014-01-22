using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class PopupAttributeAndSizeViewModel : ViewModelBase
    {
        #region Defines

        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();

        #endregion

        #region Properties

        public base_ProductModel SelectedProduct { get; set; }

        private CollectionBase<DataGridCellModel> _cellCollection;
        /// <summary>
        /// Gets or sets the CellCollection.
        /// </summary>
        public CollectionBase<DataGridCellModel> CellCollection
        {
            get { return _cellCollection; }
            set
            {
                if (_cellCollection != value)
                {
                    _cellCollection = value;
                    OnPropertyChanged(() => CellCollection);
                }
            }
        }

        private TabItem _selectedTab;
        /// <summary>
        /// Gets or sets the SelectedTab.
        /// </summary>
        public TabItem SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged(() => SelectedTab);
                }
            }
        }

        private ObservableCollection<base_Store> _storeCollection;
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public ObservableCollection<base_Store> StoreCollection
        {
            get { return _storeCollection; }
            set
            {
                if (_storeCollection != value)
                {
                    _storeCollection = value;
                    OnPropertyChanged(() => StoreCollection);
                }
            }
        }

        private int _selectedStoreIndex = Define.StoreCode;
        /// <summary>
        /// Gets or sets the SelectedStoreIndex.
        /// </summary>
        public int SelectedStoreIndex
        {
            get { return _selectedStoreIndex; }
            set
            {
                if (_selectedStoreIndex != value)
                {
                    OnSelectedStoreIndexChanging();
                    _selectedStoreIndex = value;
                    OnPropertyChanged(() => SelectedStoreIndex);
                    OnSelectedStoreIndexChanged();
                }
            }
        }

        private bool _isRaiseTotal;
        /// <summary>
        /// Gets or sets the IsRaiseTotal.
        /// </summary>
        public bool IsRaiseTotal
        {
            get { return _isRaiseTotal; }
            set
            {
                if (_isRaiseTotal != value)
                {
                    _isRaiseTotal = value;
                    OnPropertyChanged(() => IsRaiseTotal);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupAttributeAndSizeViewModel()
        {
            try
            {
                InitialCommand();

                // Load store collection
                StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        public PopupAttributeAndSizeViewModel(base_ProductModel selectedProduct)
            : this()
        {
            SelectedProduct = selectedProduct;

            // Convert ProductCollection to DataGridCellCollection
            CellCollection = new CollectionBase<DataGridCellModel>();
            foreach (base_ProductModel productItem in SelectedProduct.ProductCollection)
            {
                // Create new DataGridCell model
                DataGridCellModel dataGridCellModel = new DataGridCellModel();
                dataGridCellModel.CellResource = productItem.Resource.ToString();
                dataGridCellModel.Attribute = productItem.Size;
                dataGridCellModel.Size = productItem.Attribute;
                dataGridCellModel.IsNew = productItem.IsNew;
                dataGridCellModel.Barcode = productItem.Barcode;
                dataGridCellModel.PartNumber = productItem.PartNumber;
                dataGridCellModel.ALU = productItem.ALU;
                dataGridCellModel.RegularPrice = productItem.RegularPrice;

                // Load value list
                LoadValueList(productItem, dataGridCellModel);

                // Add new DataGridCell model to collection
                CellCollection.Add(dataGridCellModel);

                // Turn off IsDirty & IsNew
                dataGridCellModel.EndUpdate();
            }
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
            if (CellCollection == null)
                return false;
            return CellCollection.IsDirty &&
                !CellCollection.Any(x => x.IsDuplicateBarcode) && !CellCollection.Any(x => x.IsDuplicateALU);
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            UpdateProductCollection();

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
            if (param != null)
            {
                DataGridCellModel dataGridCellModel = param as DataGridCellModel;
                dataGridCellModel.IsDuplicateBarcode = IsDuplicateBarcode(SelectedProduct, dataGridCellModel.Barcode);

                // Check duplicate barcode
                if (dataGridCellModel.IsDuplicateBarcode)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Barcode of this product is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (param != null)
            {
                DataGridCellModel dataGridCellModel = param as DataGridCellModel;
                dataGridCellModel.IsDuplicateALU = IsDuplicateALU(SelectedProduct, dataGridCellModel.ALU);

                // Check duplicate barcode
                if (dataGridCellModel.IsDuplicateALU)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Alternate lookup of this product is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            CheckBarcodeCommand = new RelayCommand<object>(OnCheckBarcodeCommandExecute);
            CheckALUCommand = new RelayCommand<object>(OnCheckALUCommandExecute);
        }

        /// <summary>
        /// Load value list
        /// </summary>
        /// <param name="productItem"></param>
        /// <param name="dataGridCellModel"></param>
        private static void LoadValueList(base_ProductModel productItem, DataGridCellModel dataGridCellModel)
        {
            // Initial value list
            dataGridCellModel.ValueList = new List<ComboItem>();

            foreach (base_ProductStoreModel productStoreItem in productItem.ProductStoreCollection)
            {
                // Update quantity
                if (productStoreItem.StoreCode.Equals(Define.StoreCode))
                    dataGridCellModel.Value = productStoreItem.QuantityOnHand;

                // Create new combo item
                ComboItem comboItem = new ComboItem
                {
                    ParentId = productStoreItem.StoreCode,
                    DecimalValue = productStoreItem.QuantityOnHand
                };

                // Add new combo item to list
                dataGridCellModel.ValueList.Add(comboItem);
            }
        }

        /// <summary>
        /// Update product collection from cell collection
        /// </summary>
        private void UpdateProductCollection()
        {
            // Get deleted products
            IEnumerable<base_ProductModel> deletedProducts = SelectedProduct.ProductCollection.
                Where(x => !CellCollection.Select(y => y.CellResource).Contains(x.Resource.ToString()));

            // Remove all deleted products
            foreach (base_ProductModel productModel in deletedProducts.ToList())
                SelectedProduct.ProductCollection.Remove(productModel);

            if (CellCollection.DeletedItems != null && CellCollection.DeletedItems.Count > 0)
            {
                // Get deleted products
                deletedProducts = SelectedProduct.ProductCollection.
                    Where(x => CellCollection.DeletedItems.Select(y => y.CellResource).Contains(x.Resource.ToString()));

                // Remove all deleted products
                foreach (base_ProductModel productModel in deletedProducts.ToList())
                    SelectedProduct.ProductCollection.Remove(productModel);
            }

            // Avoid product code is duplicate
            int productCode = 0;

            // Update or create new product
            foreach (DataGridCellModel dataGridCellItem in CellCollection)
            {
                // Get edited product
                base_ProductModel productModel = SelectedProduct.ProductCollection.
                    SingleOrDefault(x => x.Resource.ToString().Equals(dataGridCellItem.CellResource));

                if (productModel == null)
                {
                    // Create new product model
                    productModel = new base_ProductModel();
                    productModel.Code = DateTimeExt.Now.AddMilliseconds(productCode++).ToString(Define.ProductCodeFormat);
                    productModel.Resource = Guid.NewGuid();
                    productModel.ProductStoreCollection = new CollectionBase<base_ProductStoreModel>();

                    // Add new product to collection
                    SelectedProduct.ProductCollection.Add(productModel);
                }

                if (SelectedStoreIndex.Equals(Define.StoreCode))
                    productModel.OnHandStore = dataGridCellItem.Value;
                productModel.Attribute = dataGridCellItem.Size;
                productModel.Size = dataGridCellItem.Attribute;
                if (!string.IsNullOrWhiteSpace(dataGridCellItem.Barcode))
                    productModel.Barcode = dataGridCellItem.Barcode.Trim();
                if (!string.IsNullOrWhiteSpace(dataGridCellItem.PartNumber))
                    productModel.PartNumber = dataGridCellItem.PartNumber.Trim();
                if (!string.IsNullOrWhiteSpace(dataGridCellItem.ALU))
                    productModel.ALU = dataGridCellItem.ALU.Trim();
                productModel.RegularPrice = dataGridCellItem.RegularPrice;

                OnSelectedStoreIndexChanging();

                foreach (ComboItem comboItem in dataGridCellItem.ValueList)
                {
                    base_ProductStoreModel productStoreItem = productModel.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(comboItem.ParentId));
                    if (productStoreItem == null)
                    {
                        // Create new product store
                        productStoreItem = new base_ProductStoreModel();
                        productStoreItem.Resource = Guid.NewGuid().ToString();
                        productStoreItem.ProductResource = productModel.Resource.ToString();

                        // Add new product store to collection
                        productModel.ProductStoreCollection.Add(productStoreItem);
                    }

                    // Update product store
                    productStoreItem.QuantityOnHand = comboItem.DecimalValue;
                    productStoreItem.StoreCode = comboItem.ParentId;
                    productStoreItem.UpdateAvailableQuantity();
                }

                // Update total quantity on product
                productModel.UpdateQuantityOnHand();

                // Update total available quantity on product
                productModel.UpdateAvailableQuantity();
            }
        }

        /// <summary>
        /// Backup data grid cell value
        /// </summary>
        private void OnSelectedStoreIndexChanging()
        {
            foreach (DataGridCellModel dataGridCellItem in CellCollection)
            {
                // Get combo item by old store
                ComboItem comboItem = dataGridCellItem.ValueList.SingleOrDefault(x => x.ParentId.Equals(SelectedStoreIndex));

                if (comboItem == null)
                {
                    // Create new combo item
                    comboItem = new ComboItem { ParentId = SelectedStoreIndex };

                    // Add new combo item to list
                    dataGridCellItem.ValueList.Add(comboItem);
                }

                // Backup data grid cell value
                comboItem.DecimalValue = dataGridCellItem.Value;
            }
        }

        /// <summary>
        /// Restore data grid cell value
        /// </summary>
        private void OnSelectedStoreIndexChanged()
        {
            foreach (DataGridCellModel dataGridCellItem in CellCollection)
            {
                // Get combo item by new store
                ComboItem comboItem = dataGridCellItem.ValueList.SingleOrDefault(x => x.ParentId.Equals(SelectedStoreIndex));
                if (comboItem == null)
                {
                    // Create new combo item
                    comboItem = new ComboItem { ParentId = SelectedStoreIndex };

                    // Add new combo item to list
                    dataGridCellItem.ValueList.Add(comboItem);
                }

                // Restore data grid cell value
                dataGridCellItem.Value = comboItem.DecimalValue;
            }

            IsRaiseTotal = !IsRaiseTotal;
        }

        /// <summary>
        /// Check barcode duplicate
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateBarcode(base_ProductModel productModel, string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return false;

                // Get barcode from product collection
                IEnumerable<string> barcodes = CellCollection.Where(x => !string.IsNullOrWhiteSpace(x.Barcode)).Select(x => x.Barcode);

                if (productModel.ProductUOMCollection != null)
                {
                    // Get barcode from product uom collection
                    IEnumerable<string> upcs = productModel.ProductUOMCollection.Where(x => !string.IsNullOrWhiteSpace(x.UPC)).Select(x => x.UPC);

                    // Check duplicate in collection
                    if (barcodes.Concat(upcs).Count(x => x.Equals(barcode)) > 1)
                        return true;
                }
                else
                {
                    // Check duplicate in collection
                    if (barcodes.Count(x => x.Equals(barcode)) > 1)
                        return true;
                }


                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.GroupAttribute.Value.Equals(productModel.GroupAttribute.Value));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.Barcode.ToLower().Equals(barcode));

                // Create predicate
                Expression<Func<base_ProductUOM, bool>> predicateUOM = PredicateBuilder.True<base_ProductUOM>();

                // Get all ProductUOM that IsPurge is false
                predicateUOM = predicateUOM.And(x => x.base_ProductStore.base_Product.IsPurge == false && !x.base_ProductStore.base_Product.GroupAttribute.Value.Equals(productModel.GroupAttribute.Value));

                // Get all ProductUOM that duplicate barcode
                predicateUOM = predicateUOM.And(x => x.UPC.ToLower().Equals(barcode));

                return _productRepository.GetIQueryable(predicate).Count() > 0 || _productUOMRepository.GetIQueryable(predicateUOM).Count() > 0;
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
        private bool IsDuplicateALU(base_ProductModel productModel, string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return false;

                // Get barcode from product collection
                IEnumerable<string> barcodes = CellCollection.Where(x => !string.IsNullOrWhiteSpace(x.ALU)).Select(x => x.ALU);

                if (productModel.ProductUOMCollection != null)
                {
                    // Get barcode from product uom collection
                    IEnumerable<string> upcs = productModel.ProductUOMCollection.Where(x => !string.IsNullOrWhiteSpace(x.ALU)).Select(x => x.ALU);

                    // Check duplicate in collection
                    if (barcodes.Concat(upcs).Count(x => x.Equals(barcode)) > 1)
                        return true;
                }
                else
                {
                    // Check duplicate in collection
                    if (barcodes.Count(x => x.Equals(barcode)) > 1)
                        return true;
                }

                // Create predicate
                Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

                // Get all products that IsPurge is false
                predicate = predicate.And(x => x.IsPurge == false && !x.GroupAttribute.Value.Equals(productModel.GroupAttribute.Value));

                // Get all products that duplicate barcode
                predicate = predicate.And(x => x.ALU.ToLower().Equals(barcode));

                // Create predicate
                Expression<Func<base_ProductUOM, bool>> predicateUOM = PredicateBuilder.True<base_ProductUOM>();

                // Get all ProductUOM that IsPurge is false
                predicateUOM = predicateUOM.And(x => x.base_ProductStore.base_Product.IsPurge == false && !x.base_ProductStore.base_Product.GroupAttribute.Value.Equals(productModel.GroupAttribute.Value));

                // Get all ProductUOM that duplicate barcode
                predicateUOM = predicateUOM.And(x => x.ALU.ToLower().Equals(barcode));

                return _productRepository.GetIQueryable(predicate).Count() > 0 || _productUOMRepository.GetIQueryable(predicateUOM).Count() > 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
        }

        #endregion
    }
}