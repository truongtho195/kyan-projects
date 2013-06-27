using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupAvailableQuantitiesViewModel : ViewModelBase
    {
        #region Defines

        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_PurchaseOrderDetailRepository _purchaseOrderDetailRepository = new base_PurchaseOrderDetailRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();

        private ICollectionView _orderDetailCollectionView;

        private short _previousSelectedStockStatus;

        #endregion

        #region Properties

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

        private short _selectedStockStatus = (short)StockStatus.Available;
        /// <summary>
        /// Gets or sets the SelectedStockStatus.
        /// </summary>
        public short SelectedStockStatus
        {
            get { return _selectedStockStatus; }
            set
            {
                if (_selectedStockStatus != value)
                {
                    // Keep selected stock status before changed
                    _previousSelectedStockStatus = SelectedStockStatus;

                    _selectedStockStatus = value;
                    OnPropertyChanged(() => SelectedStockStatus);

                    // Process when stock status changed
                    OnSelectedStockStatusChanged();
                }
            }
        }

        private ObservableCollection<base_ProductStoreModel> _productStoreCollection;
        /// <summary>
        /// Gets or sets the ProductStoreCollection.
        /// </summary>
        public ObservableCollection<base_ProductStoreModel> ProductStoreCollection
        {
            get { return _productStoreCollection; }
            set
            {
                if (_productStoreCollection != value)
                {
                    _productStoreCollection = value;
                    OnPropertyChanged(() => ProductStoreCollection);
                }
            }
        }

        private base_ProductStoreModel _productStoreDefault;
        /// <summary>
        /// Gets or sets the ProductStoreDefault.
        /// </summary>
        public base_ProductStoreModel ProductStoreDefault
        {
            get { return _productStoreDefault; }
            set
            {
                if (_productStoreDefault != value)
                {
                    _productStoreDefault = value;
                    OnPropertyChanged(() => ProductStoreDefault);
                }
            }
        }

        private short _selectedAvailableQuantity = 1;
        /// <summary>
        /// Gets or sets the SelectedAvailableQuantity.
        /// </summary>
        public short SelectedAvailableQuantity
        {
            get { return _selectedAvailableQuantity; }
            set
            {
                if (_selectedAvailableQuantity != value)
                {
                    _selectedAvailableQuantity = value;
                    OnPropertyChanged(() => SelectedAvailableQuantity);
                    OnSelectedAvailableQuantityChanged();
                }
            }
        }

        private ObservableCollection<base_SaleOrderDetailModel> _orderDetailCollection;
        /// <summary>
        /// Gets or sets the OrderDetailCollection.
        /// </summary>
        public ObservableCollection<base_SaleOrderDetailModel> OrderDetailCollection
        {
            get { return _orderDetailCollection; }
            set
            {
                if (_orderDetailCollection != value)
                {
                    _orderDetailCollection = value;
                    OnPropertyChanged(() => OrderDetailCollection);
                }
            }
        }

        /// <summary>
        /// Get total quantities by selected stock status
        /// </summary>
        public int TotalQuantities
        {
            get
            {
                return ProductStoreCollection.Sum(x => x.Quantity);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupAvailableQuantitiesViewModel()
        {
            InitialCommand();
        }

        public PopupAvailableQuantitiesViewModel(base_ProductModel productModel)
            : this()
        {
            SelectedProduct = productModel;

            LoadOrderDetailCollection(SelectedProduct);
            LoadOnHandStoreList(SelectedProduct);
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
        /// Load on hand store list
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadOnHandStoreList(base_ProductModel productModel)
        {
            // Initial product store collection
            ProductStoreCollection = new ObservableCollection<base_ProductStoreModel>();

            for (int storeCode = 0; storeCode < _storeRepository.GetIQueryable().Count(); storeCode++)
            {
                // Create new product store model
                base_ProductStoreModel productStoreModel = new base_ProductStoreModel { StoreCode = storeCode };

                // Get product store by store code
                base_ProductStoreModel productStoreItem = productModel.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(storeCode));

                if (productStoreItem != null)
                {
                    productStoreModel.StoreCode = productStoreItem.StoreCode;
                    productStoreModel.OldQuantity = productStoreItem.OldQuantity;
                    productStoreModel.QuantityOnHand = productStoreItem.QuantityOnHand;
                    productStoreModel.QuantityOnOrder = productStoreItem.QuantityOnOrder;
                    productStoreModel.QuantityOnCustomer = productStoreItem.QuantityOnCustomer;
                    productStoreModel.QuantityAvailable = productStoreItem.QuantityAvailable;
                    productStoreModel.Quantity = productStoreItem.QuantityAvailable;
                }

                // Register property changed event
                productStoreModel.PropertyChanged += new PropertyChangedEventHandler(productStoreItem_PropertyChanged);

                // Add product store to list
                ProductStoreCollection.Add(productStoreModel);

                // Get product store default by define store code
                if (storeCode.Equals(Define.StoreCode))
                    ProductStoreDefault = productStoreModel;
            }
        }

        /// <summary>
        /// Load order detail collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadOrderDetailCollection(base_ProductModel productModel)
        {
            // Get product store by store code
            base_ProductStore productStore = productModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));

            // Get product resource
            string productResource = productModel.Resource.ToString();

            // Get sale order detail
            IEnumerable<base_SaleOrderDetail> saleOrderDetails = _saleOrderDetailRepository.
                GetAll(x => x.ProductResource.Equals(productResource) && !x.base_SaleOrder.IsPurge &&
                    x.base_SaleOrder.OrderStatus == (short)SaleOrderStatus.Open &&
                    x.base_SaleOrder.StoreCode.Equals(Define.StoreCode));

            // Create sale order detail collection
            IList<base_SaleOrderDetailModel> saleOrderDetailCollection = new List<base_SaleOrderDetailModel>();

            foreach (base_SaleOrderDetail saleOrderDetail in saleOrderDetails)
            {
                // Create new sale order detail model
                base_SaleOrderDetailModel saleOrderDetailModel = new base_SaleOrderDetailModel();
                saleOrderDetailModel.DocType = "Sale Order";
                saleOrderDetailModel.SaleOrderModel = new base_SaleOrderModel(saleOrderDetail.base_SaleOrder);

                if (productStore != null)
                {
                    // Get product UOM
                    base_ProductUOM productUOM = productStore.base_ProductUOM.SingleOrDefault(x => x.UOMId.Equals(saleOrderDetail.UOMId));

                    if (productUOM != null)
                        saleOrderDetailModel.Quantity = saleOrderDetail.Quantity * productUOM.BaseUnitNumber;
                }

                // Add new sale order detail to collection
                saleOrderDetailCollection.Add(saleOrderDetailModel);
            }

            // Get purchase order detail
            IList<base_PurchaseOrderDetail> purchaseOrderDetails = _purchaseOrderDetailRepository.
                GetAll(x => x.ProductResource.Equals(productResource) && !x.base_PurchaseOrder.IsPurge &&
                    x.base_PurchaseOrder.Status == (short)PurchaseStatus.Receiving &&
                    x.base_PurchaseOrder.StoreCode.Equals(Define.StoreCode));

            // Create purchase order detail collection
            IList<base_SaleOrderDetailModel> purchaseOrderDetailCollection = new List<base_SaleOrderDetailModel>();

            foreach (base_PurchaseOrderDetail purchaseOrderDetail in purchaseOrderDetails)
            {
                // Create new purchase order detail model
                base_SaleOrderDetailModel purchaseOrderDetailModel = new base_SaleOrderDetailModel();
                purchaseOrderDetailModel.DocType = "Purchase Order";
                purchaseOrderDetailModel.SaleOrderModel = new base_SaleOrderModel
                {
                    OrderDate = purchaseOrderDetail.base_PurchaseOrder.PurchasedDate,
                    SONumber = purchaseOrderDetail.base_PurchaseOrder.PurchaseOrderNo
                };

                if (productStore != null)
                {
                    // Get product UOM
                    base_ProductUOM productUOM = productStore.base_ProductUOM.SingleOrDefault(x => x.UOMId.Equals(purchaseOrderDetail.UOMId));

                    if (productUOM != null)
                        purchaseOrderDetailModel.Quantity = purchaseOrderDetail.Quantity * productUOM.BaseUnitNumber;
                }

                // Add new purchase order detail to collection
                purchaseOrderDetailCollection.Add(purchaseOrderDetailModel);
            }

            // Get all order detail collection
            OrderDetailCollection = new ObservableCollection<base_SaleOrderDetailModel>(saleOrderDetailCollection.Union(purchaseOrderDetailCollection));

            // Get default view for filter
            _orderDetailCollectionView = CollectionViewSource.GetDefaultView(OrderDetailCollection);
        }

        /// <summary>
        /// Filter order detail collection when selected available quantity changed
        /// </summary>
        private void OnSelectedAvailableQuantityChanged()
        {
            // Initial keyword for filter
            string keyWord = string.Empty;

            switch (SelectedAvailableQuantity)
            {
                case 1: // All Order
                    _orderDetailCollectionView.Refresh();
                    break;
                case 2: // Sale Order
                    keyWord = "Sale";
                    break;
                case 3: // Purchase Order
                    keyWord = "Purchase";
                    break;
                case 4: // Pending Order
                    keyWord = "Pending";
                    break;
            }

            // Filter by keyword
            _orderDetailCollectionView.Filter = x =>
            {
                base_SaleOrderDetailModel orderDetailItem = x as base_SaleOrderDetailModel;
                return orderDetailItem.DocType.Contains(keyWord);
            };
        }

        /// <summary>
        /// Process when stock status changed
        /// </summary>
        private void OnSelectedStockStatusChanged()
        {
            foreach (base_ProductStoreModel productStoreItem in ProductStoreCollection)
            {
                switch (_previousSelectedStockStatus)
                {
                    case (short)StockStatus.Available:
                        // Update quantity available
                        productStoreItem.QuantityAvailable = productStoreItem.Quantity;
                        break;
                    case (short)StockStatus.OnHand:
                        // Update quantity on hand
                        productStoreItem.QuantityOnHand = productStoreItem.Quantity;
                        break;
                    case (short)StockStatus.OnReserved:
                        // Update quantity on order
                        productStoreItem.QuantityOnOrder = productStoreItem.Quantity;
                        break;
                }

                switch (SelectedStockStatus)
                {
                    case (short)StockStatus.Available:
                        // Update quantity available
                        productStoreItem.Quantity = productStoreItem.QuantityAvailable;
                        break;
                    case (short)StockStatus.OnHand:
                        // Update quantity on hand
                        productStoreItem.Quantity = productStoreItem.QuantityOnHand;
                        break;
                    case (short)StockStatus.OnReserved:
                        // Update quantity on order
                        productStoreItem.Quantity = productStoreItem.QuantityOnOrder;
                        break;
                }
            }

            // Update total quantities
            OnPropertyChanged(() => TotalQuantities);
        }

        #endregion

        #region Event Methods

        private void productStoreItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ProductStoreModel productStoreModel = sender as base_ProductStoreModel;
            switch (e.PropertyName)
            {
                case "Quantity":
                    if (SelectedStockStatus.Equals((short)StockStatus.OnHand))
                        productStoreModel.QuantityOnHand = productStoreModel.Quantity;
                    break;
                case "QuantityOnHand":
                    // Update total quantities
                    OnPropertyChanged(() => TotalQuantities);

                    // Update quantity available
                    productStoreModel.UpdateAvailableQuantity();
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
