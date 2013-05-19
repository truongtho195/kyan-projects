using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows.Controls;
using System.Reflection;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    class PopupAttributeAndSizeViewModel : ViewModelBase
    {
        #region Defines

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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupAttributeAndSizeViewModel()
        {
            InitialCommand();
        }

        public PopupAttributeAndSizeViewModel(base_ProductModel selectedProduct)
            : this()
        {
            SelectedProduct = selectedProduct;

            // Convert ProductCollection to DataGridCellCollection
            CellCollection = new CollectionBase<DataGridCellModel>();
            foreach (base_ProductModel productItem in SelectedProduct.ProductCollection)
            {
                // Get OnHandStore from on hand quantity
                productItem.OnHandStore = productItem.GetOnHandFromStore(Define.StoreCode);

                // Create new DataGridCell model
                DataGridCellModel dataGridCellModel = new DataGridCellModel();
                dataGridCellModel.CellResource = productItem.Resource.ToString();
                dataGridCellModel.Value = productItem.OnHandStore;
                dataGridCellModel.Attribute = productItem.Attribute;
                dataGridCellModel.Size = productItem.Size;
                dataGridCellModel.IsNew = productItem.IsNew;
                dataGridCellModel.Barcode = productItem.Barcode;
                dataGridCellModel.PartNumber = productItem.PartNumber;

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
        /// Update product collection from cell collection
        /// </summary>
        private void UpdateProductCollection()
        {
            // Get deleted products and remove them
            IEnumerable<base_ProductModel> deletedProducts = SelectedProduct.ProductCollection.Where(x => !CellCollection.Select(y => y.CellResource).Contains(x.Resource.ToString()));
            foreach (base_ProductModel productModel in deletedProducts.ToList())
                SelectedProduct.ProductCollection.Remove(productModel);

            if (CellCollection.DeletedItems != null && CellCollection.DeletedItems.Count > 0)
            {
                deletedProducts = SelectedProduct.ProductCollection.Where(x => CellCollection.DeletedItems.Select(y => y.CellResource).Contains(x.Resource.ToString()));
                foreach (base_ProductModel productModel in deletedProducts.ToList())
                    SelectedProduct.ProductCollection.Remove(productModel);
            }

            // Avoid product code is duplicate
            int productCode = 0;

            // Update or create new product
            foreach (DataGridCellModel dataGridCellItem in CellCollection)
            {
                // Get edited product
                base_ProductModel productModel = SelectedProduct.ProductCollection.SingleOrDefault(x => x.Resource.ToString().Equals(dataGridCellItem.CellResource));

                if (productModel == null)
                {
                    // Create new product model
                    productModel = new base_ProductModel();
                    productModel.Code = DateTimeExt.Now.AddMilliseconds(productCode++).ToString(Define.ProductCodeFormat);
                    productModel.Resource = Guid.NewGuid();

                    // Add new product to collection
                    SelectedProduct.ProductCollection.Add(productModel);
                }

                productModel.OnHandStore = dataGridCellItem.Value;
                productModel.Attribute = dataGridCellItem.Attribute;
                productModel.Size = dataGridCellItem.Size;
                productModel.Barcode = dataGridCellItem.Barcode;
                productModel.PartNumber = dataGridCellItem.PartNumber;

                // Set on hand quantity from OnHandStore
                productModel.SetOnHandToStore(productModel.OnHandStore, Define.StoreCode);


            }
        }

        #endregion
    }
}
