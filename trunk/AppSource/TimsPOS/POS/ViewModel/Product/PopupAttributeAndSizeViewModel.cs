using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    class PopupAttributeAndSizeViewModel : ViewModelBase
    {
        #region Defines

        private base_StoreRepository _storeRepository = new base_StoreRepository();

        private int _numberOfStore = 10;

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
                    //if (SelectedTab != null && SelectedTab.Name.Equals("tabitemBarcode"))
                    //    UpdateProductCollection();
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
            InitialCommand();

            // Load store collection
            StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
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
                dataGridCellModel.Attribute = productItem.Attribute;
                dataGridCellModel.Size = productItem.Size;
                dataGridCellModel.IsNew = productItem.IsNew;
                dataGridCellModel.Barcode = productItem.Barcode;
                dataGridCellModel.PartNumber = productItem.PartNumber;

                // Load value list
                LoadValueList(productItem, dataGridCellModel);

                // Add new DataGridCell model to collection
                CellCollection.Add(dataGridCellModel);
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
            return true;
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
                    dataGridCellModel.Value = (int)productStoreItem.QuantityOnHand;

                // Create new combo item
                ComboItem comboItem = new ComboItem
                {
                    ParentId = productStoreItem.StoreCode,
                    IntValue = (int)productStoreItem.QuantityOnHand
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
                productModel.Attribute = dataGridCellItem.Attribute;
                productModel.Size = dataGridCellItem.Size;
                productModel.Barcode = dataGridCellItem.Barcode;
                productModel.PartNumber = dataGridCellItem.PartNumber;

                OnSelectedStoreIndexChanging();

                foreach (ComboItem comboItem in dataGridCellItem.ValueList)
                {
                    base_ProductStoreModel productStoreItem = productModel.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(comboItem.ParentId));
                    if (productStoreItem == null)
                    {
                        // Create new product store
                        productStoreItem = new base_ProductStoreModel();

                        // Add new product store to collection
                        productModel.ProductStoreCollection.Add(productStoreItem);
                    }

                    // Update product store
                    productStoreItem.QuantityOnHand = comboItem.IntValue;
                    productStoreItem.StoreCode = comboItem.ParentId;
                }

                // Update total quantity on product
                productModel.UpdateQuantityOnHand();
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
                comboItem.IntValue = dataGridCellItem.Value;
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
                dataGridCellItem.Value = comboItem.IntValue;
            }

            IsRaiseTotal = !IsRaiseTotal;
        }

        #endregion
    }
}
