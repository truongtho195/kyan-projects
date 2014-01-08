using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupPricingViewModel : ViewModelBase
    {
        #region Defines

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the SelectedProduct
        /// </summary>
        public base_ProductModel SelectedProduct { get; set; }

        /// <summary>
        /// Gets or sets the ProductUOMList.
        /// </summary>
        public CollectionBase<base_ProductUOMModel> ProductUOMList { get; set; }

        private base_ProductUOMModel _selectedProductUOM;
        /// <summary>
        /// Gets or sets the SelectedProductUOM.
        /// </summary>
        public base_ProductUOMModel SelectedProductUOM
        {
            get { return _selectedProductUOM; }
            set
            {
                if (_selectedProductUOM != value)
                {
                    _selectedProductUOM = value;
                    OnPropertyChanged(() => SelectedProductUOM);
                }
            }
        }

        /// <summary>
        /// Gets or sets the UOMList.
        /// </summary>
        public List<CheckBoxItemModel> UOMList { get; set; }

        private CheckBoxItemModel _selectedUOM;
        /// <summary>
        /// Gets or sets the SelectedUOM.
        /// </summary>
        public CheckBoxItemModel SelectedUOM
        {
            get { return _selectedUOM; }
            set
            {
                if (_selectedUOM != value)
                {
                    _selectedUOM = value;
                    OnPropertyChanged(() => SelectedUOM);
                    if (SelectedUOM != null)
                        OnSelectedUOMChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the PriceSchemaList
        /// </summary>
        public List<PriceModel> PriceSchemaList { get; set; }

        private bool _isEnabledSelectedUOM;
        /// <summary>
        /// Gets or sets the IsEnabledSelectedUOM.
        /// </summary>
        public bool IsEnabledSelectedUOM
        {
            get { return _isEnabledSelectedUOM; }
            set
            {
                if (_isEnabledSelectedUOM != value)
                {
                    _isEnabledSelectedUOM = value;
                    OnPropertyChanged(() => IsEnabledSelectedUOM);
                }
            }
        }

        private bool _isEnabledAverageCost;
        /// <summary>
        /// Gets or sets the IsEnabledAverageCost.
        /// </summary>
        public bool IsEnabledAverageCost
        {
            get { return _isEnabledAverageCost && !IsGroupItemType; }
            set
            {
                if (_isEnabledAverageCost != value)
                {
                    _isEnabledAverageCost = value;
                    OnPropertyChanged(() => IsEnabledAverageCost);
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
        /// Gets the IsManualPriceCalculation.
        /// </summary>
        public bool IsManualPriceCalculation
        {
            get
            {
                if (Define.CONFIGURATION == null)
                    return false;
                return Define.CONFIGURATION.IsManualPriceCalculation;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupPricingViewModel()
        {
            InitialCommand();
        }

        public PopupPricingViewModel(base_ProductModel productModel, ObservableCollection<CheckBoxItemModel> uomList, List<PriceModel> priceSchemaList, int selectedUOMID = 0)
            : this()
        {
            SelectedProduct = productModel;

            // Get product UOM list
            ProductUOMList = new CollectionBase<base_ProductUOMModel>();
            if (SelectedProduct.ProductUOMCollection != null)
                foreach (base_ProductUOMModel productUOMItem in SelectedProduct.ProductUOMCollection.Where(x => x.UOMId > 0).CloneList())
                {
                    // Register property changed event
                    productUOMItem.PropertyChanged += new PropertyChangedEventHandler(ProductUOMModel_PropertyChanged);

                    // Add item to list
                    ProductUOMList.Add(productUOMItem);

                    // Turn off IsDirty & IsNew
                    productUOMItem.EndUpdate();
                }

            // Create base UOM
            base_ProductUOMModel baseUOMModel = new base_ProductUOMModel();

            // Copy values from product to base UOM
            ProductToProductUOM(SelectedProduct, baseUOMModel);

            // Register property changed event
            baseUOMModel.PropertyChanged += new PropertyChangedEventHandler(ProductUOMModel_PropertyChanged);

            // Insert item to the first
            ProductUOMList.Insert(0, baseUOMModel);

            // Turn off IsDirty & IsNew
            baseUOMModel.EndUpdate();

            // Get UOM list
            UOMList = new List<CheckBoxItemModel>(uomList.Where(x => x.IsChecked));

            // Get price schema list
            PriceSchemaList = new List<PriceModel>(priceSchemaList);

            if (selectedUOMID == 0)
            {
                selectedUOMID = baseUOMModel.UOMId;

                // When popup pricing from product, allow change UOM
                IsEnabledSelectedUOM = true;
            }
            else
            {
                FocusDefault = true;
            }

            // Set selected UOM when popup pricing from UOM management
            SelectedUOM = UOMList.FirstOrDefault(x => x.Value.Equals(selectedUOMID));
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
            if (ProductUOMList == null)
                return false;
            return ProductUOMList.IsDirty;
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
        /// Process when select UOM id changed
        /// </summary>
        private void OnSelectedUOMChanged()
        {
            // Update selected product UOM
            SelectedProductUOM = ProductUOMList.FirstOrDefault(x => x.UOMId.Equals(SelectedUOM.Value));

            // Enable average cost field if selected UOM is base UOM
            IsEnabledAverageCost = SelectedProductUOM.UOMId == SelectedProduct.BaseUOMId;

            // Get base UOM
            base_ProductUOMModel productUOMModel = ProductUOMList.SingleOrDefault(x => x.UOMId == SelectedProduct.BaseUOMId);

            if (!IsEnabledAverageCost && productUOMModel != null)
                // Update average cost value by average unit
                SelectedProductUOM.UpdateAverageCost(productUOMModel.AverageCost);
        }

        /// <summary>
        /// Copy some properties of product to product UOM
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="productUOMModel"></param>
        /// <param name="isReverse"></param>
        public void ProductToProductUOM(base_ProductModel productModel, base_ProductUOMModel productUOMModel, bool isReverse = false)
        {
            if (!isReverse)
            {
                productUOMModel.UOMId = productModel.BaseUOMId;
                productUOMModel.RegularPrice = productModel.RegularPrice;
                productUOMModel.AverageCost = productModel.AverageUnitCost;
                productUOMModel.Price1 = productModel.Price1;
                productUOMModel.Price2 = productModel.Price2;
                productUOMModel.Price3 = productModel.Price3;
                productUOMModel.Price4 = productModel.Price4;
                productUOMModel.MarkDownPercent1 = productModel.MarkdownPercent1;
                productUOMModel.MarkDownPercent2 = productModel.MarkdownPercent2;
                productUOMModel.MarkDownPercent3 = productModel.MarkdownPercent3;
                productUOMModel.MarkDownPercent4 = productModel.MarkdownPercent4;
                productUOMModel.MarginPercent = productModel.MarginPercent;
                productUOMModel.MarkupPercent = productModel.MarkupPercent;
            }
            else
            {
                productModel.BaseUOMId = productUOMModel.UOMId;
                productModel.RegularPrice = productUOMModel.RegularPrice;
                productModel.AverageUnitCost = productUOMModel.AverageCost;
                productModel.Price1 = productUOMModel.Price1;
                productModel.Price2 = productUOMModel.Price2;
                productModel.Price3 = productUOMModel.Price3;
                productModel.Price4 = productUOMModel.Price4;
                productModel.MarkdownPercent1 = productUOMModel.MarkDownPercent1;
                productModel.MarkdownPercent2 = productUOMModel.MarkDownPercent2;
                productModel.MarkdownPercent3 = productUOMModel.MarkDownPercent3;
                productModel.MarkdownPercent4 = productUOMModel.MarkDownPercent4;
                productModel.MarginPercent = productUOMModel.MarginPercent;
                productModel.MarkupPercent = productUOMModel.MarkupPercent;
            }
        }

        #endregion

        #region Override Methods

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
                case "RegularPrice":
                case "AverageCost":
                    // Update margin, markup and price
                    productUOMModel.UpdateMarginMarkupAndPrice();
                    break;
                case "Price1":
                    // Update markdown percent
                    productUOMModel.MarkDownPercent1 = productUOMModel.CalcMarkDown(productUOMModel.Price1);
                    break;
                case "Price2":
                    // Update markdown percent
                    productUOMModel.MarkDownPercent2 = productUOMModel.CalcMarkDown(productUOMModel.Price2);
                    break;
                case "Price3":
                    // Update markdown percent
                    productUOMModel.MarkDownPercent3 = productUOMModel.CalcMarkDown(productUOMModel.Price3);
                    break;
                case "Price4":
                    // Update markdown percent
                    productUOMModel.MarkDownPercent4 = productUOMModel.CalcMarkDown(productUOMModel.Price4);
                    break;
                case "MarkDownPercent1":
                    // Update price
                    productUOMModel.Price1 = productUOMModel.CalcPrice(productUOMModel.MarkDownPercent1);
                    break;
                case "MarkDownPercent2":
                    // Update price
                    productUOMModel.Price2 = productUOMModel.CalcPrice(productUOMModel.MarkDownPercent2);
                    break;
                case "MarkDownPercent3":
                    // Update price
                    productUOMModel.Price3 = productUOMModel.CalcPrice(productUOMModel.MarkDownPercent3);
                    break;
                case "MarkDownPercent4":
                    // Update price
                    productUOMModel.Price4 = productUOMModel.CalcPrice(productUOMModel.MarkDownPercent4);
                    break;
            }
        }

        #endregion
    }
}