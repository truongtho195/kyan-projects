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

        private ICollectionView _orderDetailCollectionView;

        private IList<base_PurchaseOrderDetail> _purchaseOrderDetails;

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

        private short _stockStatus = 1;
        /// <summary>
        /// Gets or sets the StockStatus.
        /// </summary>
        public short StockStatus
        {
            get { return _stockStatus; }
            set
            {
                if (_stockStatus != value)
                {
                    _stockStatus = value;
                    OnPropertyChanged(() => StockStatus);

                    // Update company quantities
                    OnPropertyChanged(() => CompanyQuantities);
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

        /// <summary>
        /// Gets the CompanyQuantities
        /// </summary>
        public int CompanyQuantities
        {
            get
            {
                if (StockStatus.Equals(3))
                    return ProductStoreCollection.Sum(x => x.OnReservedQuantity);
                return ProductStoreCollection.Sum(x => x.QuantityOnHand);
            }
        }

        /// <summary>
        /// Gets the QuantityOnHand.
        /// </summary>
        public int QuantityOnHand
        {
            get { return ProductStoreCollection.Where(x => x.StoreCode.Equals(Define.StoreCode)).Sum(x => x.QuantityOnHand); }
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
        /// Gets or sets the QuantityOnSO.
        /// </summary>
        public int QuantityOnSO { get; private set; }

        /// <summary>
        /// Gets or sets the QuantityOnPO.
        /// </summary>
        public int QuantityOnPO { get; private set; }

        /// <summary>
        /// Gets or sets the QuantityAvailable.
        /// </summary>
        public int QuantityAvailable
        {
            get { return QuantityOnHand - QuantityOnSO; }
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

            for (short i = 0; i < _storeRepository.GetIQueryable().Count(); i++)
            {
                // Create new product store model
                base_ProductStoreModel productStoreModel = new base_ProductStoreModel { StoreCode = i };

                // Get product store by store code
                base_ProductStoreModel productStoreItem = productModel.ProductStoreCollection.SingleOrDefault(x => x.StoreCode.Equals(i));

                if (productStoreItem != null)
                {
                    productStoreModel.OldQuantity = productStoreItem.OldQuantity;
                    productStoreModel.QuantityOnHand = productStoreItem.QuantityOnHand;
                    productStoreModel.OnReservedQuantity = _purchaseOrderDetails.Where(x => x.base_PurchaseOrder.StoreCode.Equals(i)).Sum(x => x.Quantity);
                }

                // Register property changed event
                productStoreModel.PropertyChanged += new PropertyChangedEventHandler(productStoreItem_PropertyChanged);

                // Add product store to list
                ProductStoreCollection.Add(productStoreModel);
            }
        }

        /// <summary>
        /// Load order detail collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadOrderDetailCollection(base_ProductModel productModel)
        {
            // Get product resource
            string productResource = productModel.Resource.ToString();

            // Get sale order detail collection
            IEnumerable<base_SaleOrderDetailModel> _saleOrderDetailCollection = _saleOrderDetailRepository.
                GetAll(x => x.ProductResource.Equals(productResource) &&
                    x.base_SaleOrder.OrderStatus == (short)SaleOrderStatus.Open &&
                    x.base_SaleOrder.StoreCode.Equals(Define.StoreCode)).
                Select(x => new base_SaleOrderDetailModel(x)
                {
                    DocType = "Sale Order",
                    SaleOrderModel = new base_SaleOrderModel(x.base_SaleOrder)
                });

            // Get purchase order detail
            _purchaseOrderDetails = _purchaseOrderDetailRepository.
                GetAll(x => x.ProductResource.Equals(productResource) && x.base_PurchaseOrder.Status == (short)PurchaseStatus.Open);

            // Get purchase order detail collection
            IEnumerable<base_SaleOrderDetailModel> _purchaseOrderDetailCollection = _purchaseOrderDetails.
                Where(x => x.base_PurchaseOrder.StoreCode.Equals(Define.StoreCode)).
                Select(x => new base_SaleOrderDetailModel
                {
                    DocType = "Purchase Order",
                    SaleOrderModel = new base_SaleOrderModel
                    {
                        OrderDate = x.base_PurchaseOrder.PurchasedDate,
                        SONumber = x.base_PurchaseOrder.PurchaseOrderNo
                    },
                    Quantity = x.Quantity,
                    IsNew = false
                });

            // Get all order detail collection
            OrderDetailCollection = new ObservableCollection<base_SaleOrderDetailModel>(_saleOrderDetailCollection.Union(_purchaseOrderDetailCollection));

            // Update quantity values
            QuantityOnSO = _saleOrderDetailCollection.Sum(x => x.Quantity);
            QuantityOnPO = _purchaseOrderDetailCollection.Sum(x => x.Quantity);
            OnPropertyChanged(() => QuantityAvailable);

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

        #endregion

        #region Event Methods

        private void productStoreItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ProductStoreModel productStoreModel = sender as base_ProductStoreModel;

            if (e.PropertyName.Equals("QuantityOnHand"))
            {
                // Update company quantities
                OnPropertyChanged(() => CompanyQuantities);

                if (productStoreModel.StoreCode.Equals(Define.StoreCode))
                {
                    // Update quantity on hand
                    OnPropertyChanged(() => QuantityOnHand);

                    // Update quantity available
                    OnPropertyChanged(() => QuantityAvailable);
                }
            }
        }

        #endregion
    }
}
