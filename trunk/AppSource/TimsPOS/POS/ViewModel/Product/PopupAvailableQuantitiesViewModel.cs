using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
                }
            }
        }

        private ObservableCollection<CheckBoxItemModel> _onHandStoreList = new ObservableCollection<CheckBoxItemModel>();
        /// <summary>
        /// Gets or sets the OnHandStoreList.
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> OnHandStoreList
        {
            get { return _onHandStoreList; }
            set
            {
                if (_onHandStoreList != value)
                {
                    _onHandStoreList = value;
                    OnPropertyChanged(() => OnHandStoreList);
                }
            }
        }

        /// <summary>
        /// Gets the CompanyQuantities
        /// </summary>
        public decimal CompanyQuantities
        {
            get
            {
                return OnHandStoreList.Sum(x => x.Value);
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
                    if (SelectedAvailableQuantity != null)
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
            get
            {
                if (SelectedProduct == null)
                    return 0;
                return SelectedProduct.QuantityOnHand - QuantityOnSO;
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

            LoadOnHandStoreList(SelectedProduct);
            LoadOrderDetailCollection(SelectedProduct);
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
            for (short i = 1; i <= _storeRepository.GetIQueryable().Count(); i++)
            {
                // Get on hand store value
                PropertyInfo onHandStoreProperty = productModel.GetType().GetProperty(i.ToString("OnHandStore#"));
                object onHandStoreValue = onHandStoreProperty.GetValue(productModel, null);

                // Convert on hand store value to int datatype
                int onHandStore = 0;
                if (int.TryParse(onHandStoreValue.ToString(), out onHandStore))
                    onHandStore = int.Parse(onHandStoreValue.ToString());

                // Create combo item to add list
                CheckBoxItemModel checkBoxItemModel = new CheckBoxItemModel { Text = i.ToString(), Value = onHandStore };
                checkBoxItemModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CheckBoxItemModel_PropertyChanged);
                OnHandStoreList.Add(checkBoxItemModel);
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
                GetAll(x => x.ProductResource.Equals(productResource) && x.base_SaleOrder.OrderStatus == (short)SaleOrderStatus.Open).
                Select(x => new base_SaleOrderDetailModel(x)
                {
                    DocType = "Sale Order",
                    SaleOrderModel = new base_SaleOrderModel(x.base_SaleOrder)
                });

            // Get purchase order detail collection
            IEnumerable<base_SaleOrderDetailModel> _purchaseOrderDetailCollection = _purchaseOrderDetailRepository.
                GetAll(x => x.ProductResource.Equals(productResource) && x.base_PurchaseOrder.Status == (short)PurchaseStatus.Open).
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

        #region Override Methods

        private void CheckBoxItemModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Value"))
            {
                // Update company quantities
                OnPropertyChanged(() => CompanyQuantities);
            }
        }

        #endregion
    }
}
