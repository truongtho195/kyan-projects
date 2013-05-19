using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupManagementUOMViewModel : ViewModelBase
    {
        #region Defines

        private int? _oldOrderUOMID;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupManagementUOMViewModel()
        {
            InitialCommand();
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="uomList"></param>
        /// <param name="selectedProduct"></param>
        public PopupManagementUOMViewModel(ObservableCollection<CheckBoxItemModel> uomList, base_ProductModel selectedProduct, List<PriceModel> priceSchemaList)
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
                productUOMModel.PropertyChanged += new PropertyChangedEventHandler(productUOMModel_PropertyChanged);
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

            return uomID > 0;
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            PopupPricingCommand = new RelayCommand<object>(OnPopupPricingCommandExecute, OnPopupPricingCommandCanExecute);
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Copy UOM values
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public void CopyUOM(base_ProductModel target, base_ProductModel source)
        {
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

        #endregion

        #region Override Methods

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
                        productModel.RegularPrice = 0;
                        productModel.QuantityOnHand = 0;
                        productModel.AverageUnitCost = 0;
                    }
                    break;
                case "OrderUOMId":
                    if (productModel.OrderUOMId > 0 &&
                        productModel.OrderUOMId != productModel.BaseUOMId &&
                        productModel.OrderUOMId != _oldOrderUOMID)
                    {
                        PopupUpdateOrderCostViewModel viewModel = new PopupUpdateOrderCostViewModel(SelectedProduct.OrderCost);
                        bool? result = _dialogService.ShowDialog<PopupUpdateOrderCostView>(this, viewModel, "Update order cost");
                        if (result.HasValue && result.Value)
                        {
                            // Backup order UOM id value
                            _oldOrderUOMID = productModel.OrderUOMId;

                            if (viewModel.UpdateOrderCostOption.Equals(0))
                                SelectedProduct.OrderCost = viewModel.NewOrderCost;
                            else if (viewModel.UpdateOrderCostOption.Equals(1))
                                SelectedProduct.OrderCost = 0;
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
                    foreach (base_ProductUOMModel productUOMItem in SelectedProduct.ProductUOMCollection.Where(x => x.UOMId > 0))
                        productUOMItem.UpdateAverageCost(SelectedProduct.AverageUnitCost);
                    break;
                case "OnHandStore":
                    // Update quantity on hand for other UOM
                    foreach (base_ProductUOMModel productUOMItem in SelectedProduct.ProductUOMCollection.Where(x => x.UOMId > 0))
                        productUOMItem.UpdateQuantityOnHand(SelectedProduct.OnHandStore);
                    break;
            }
        }

        #endregion
    }
}
