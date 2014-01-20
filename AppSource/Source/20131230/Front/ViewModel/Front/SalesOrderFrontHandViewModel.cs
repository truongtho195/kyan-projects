using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Helper;
using System.Collections.ObjectModel;
using System.Windows;
using CPC.POS.Database;
using System.Linq.Expressions;
using CPC.POS.View;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Controls;
using CPC.Control;
using Wosk;
using System.Threading;

namespace CPC.POS.ViewModel
{
    class SalesOrderFrontHandViewModel : ViewModelBase
    {
        #region Define
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_SaleTaxLocationRepository _saleTaxRepository = new base_SaleTaxLocationRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_ResourcePaymentRepository _paymentRepository = new base_ResourcePaymentRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();
        private base_CardManagementRepository _cardManagementRepository = new base_CardManagementRepository();
        private base_ResourceReturnRepository _resourceReturnRepository = new base_ResourceReturnRepository();
        private base_ResourceReturnDetailRepository _resourceReturnDetailRepository = new base_ResourceReturnDetailRepository();
        private base_GuestRewardSaleOrderRepository _guestRewardSaleOrderRepository = new base_GuestRewardSaleOrderRepository();


        private ICollectionView _saleOrderDetailCollectionView;
        private bool BreakSODetailChange { get; set; }

        private DispatcherTimer _timer = new DispatcherTimer();

        protected string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        protected string EMPLOYEE_MARK = MarkType.Employee.ToDescription();
        #endregion

        #region Constructors
        public SalesOrderFrontHandViewModel()
        {
            try
            {
                _ownerViewModel = this;
                InitialCommand();

                _guestRepository.CreateDefaultCustomer();

                LoadDynamicData();

                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += new EventHandler(_timer_Tick);
                _timer.Start();

                if (SelectedSaleOrder == null)
                {
                    CreateNewSaleOrder();
                }

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }


        #endregion

        #region Properties
        #region IsDirty
        /// <summary>
        /// Gets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                return SelectedSaleOrder.IsDirty
                    || (SelectedSaleOrder.SaleOrderDetailCollection != null
                            && (SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsDirty)
                            || SelectedSaleOrder.SaleOrderDetailCollection.DeletedItems.Any()))
                    || (SelectedSaleOrder.SaleOrderShipCollection != null
                            && (SelectedSaleOrder.SaleOrderShipCollection.Any(x => x.IsDirty)
                            || SelectedSaleOrder.SaleOrderShipCollection.DeletedItems.Any()))
                    || (SelectedSaleOrder.ReturnModel != null && (SelectedSaleOrder.ReturnModel.IsDirty || SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.IsDirty)))
                    || (SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any(x => x.IsDirty))
                    || (SelectedSaleOrder.BillAddressModel != null && SelectedSaleOrder.BillAddressModel.IsDirty)
                    || (SelectedSaleOrder.ShipAddressModel != null && SelectedSaleOrder.ShipAddressModel.IsDirty);
            }

        }
        #endregion

        #region SelectedCustomer
        private base_GuestModel _selectedCustomer;
        /// <summary>
        /// Gets or sets the SelectedCustomer.
        /// </summary>
        public base_GuestModel SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged(() => SelectedCustomer);
                    SelectedCustomerChanged();
                }
            }
        }
        #endregion

        #region SelectedProduct
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

                    SelectedProductChanged();
                }
            }
        }


        #endregion

        #region BarcodeProduct
        private string _barcodeProduct;
        /// <summary>
        /// Gets or sets the Barcode.
        /// </summary>
        public string BarcodeProduct
        {
            get { return _barcodeProduct; }
            set
            {
                if (_barcodeProduct != value)
                {
                    _barcodeProduct = value;
                    OnPropertyChanged(() => BarcodeProduct);
                }
            }
        }

        #endregion

        //Order
        #region SelectedSaleOrder
        private base_SaleOrderModel _selectedSaleOrder;
        /// <summary>
        /// Gets or sets the SelectedSaleOrder.
        /// </summary>
        public base_SaleOrderModel SelectedSaleOrder
        {
            get { return _selectedSaleOrder; }
            set
            {
                if (_selectedSaleOrder != value)
                {
                    _selectedSaleOrder = value;
                    OnPropertyChanged(() => SelectedSaleOrder);
                }
            }
        }
        #endregion

        #region SelectedSaleOrderDetail
        private base_SaleOrderDetailModel _selectedSaleOrderDetail;
        /// <summary>
        /// Gets or sets the SelectedSaleOrderDetail.
        /// </summary>
        public base_SaleOrderDetailModel SelectedSaleOrderDetail
        {
            get { return _selectedSaleOrderDetail; }
            set
            {
                if (_selectedSaleOrderDetail != value)
                {
                    _selectedSaleOrderDetail = value;
                    OnPropertyChanged(() => SelectedSaleOrderDetail);
                }
            }
        }
        #endregion

        //Other Property
        #region SaleTaxCollection
        public List<base_SaleTaxLocationModel> SaleTaxLocationCollection
        {
            get;
            set;
        }
        #endregion

        #region ExtensionProducts
        private List<base_ProductModel> _extensionProducts = new List<base_ProductModel>();
        /// <summary>
        /// Gets or sets the ExtensionProducts.
        /// </summary>
        public List<base_ProductModel> ExtensionProducts
        {
            get { return _extensionProducts; }
            set
            {
                if (_extensionProducts != value)
                {
                    _extensionProducts = value;
                }
            }
        }
        #endregion

        #region CustomerCollection
        private CollectionBase<base_GuestModel> _customerCollection;
        /// <summary>
        /// Gets or sets the CustomerCollection.
        /// </summary>
        public CollectionBase<base_GuestModel> CustomerCollection
        {
            get { return _customerCollection; }
            set
            {
                if (_customerCollection != value)
                {
                    _customerCollection = value;
                    OnPropertyChanged(() => CustomerCollection);
                }
            }
        }

        /// <summary>
        /// Deactive Customer
        /// </summary>
        public List<base_GuestModel> CustomerList { get; set; }
        #endregion

        #region PromotionList
        private List<base_PromotionModel> _promotionList
        {
            get;
            set;
        }
        #endregion

        #region RewardManagerCollection
        public List<base_RewardManager> RewardManagerCollection { get; set; }
        #endregion

        #region CurrentDateTime
        private DateTime _currentDateTime;
        /// <summary>
        /// Gets or sets the CurrentDateTime.
        /// <para>Show in view</para>
        /// </summary>
        public DateTime CurrentDateTime
        {
            get { return _currentDateTime; }
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged(() => CurrentDateTime);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Gets the New Command.
        /// <summary>

        public RelayCommand<object> NewCommand { get; private set; }


        /// <summary>
        /// Method to check whether the New command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the New command is executed.
        /// </summary>
        private void OnNewCommandExecute(object param)
        {
            CreateNewSaleOrder();
        }
        #endregion

        #region SavePrintCommand
        /// <summary>
        /// Gets the SavePrint Command.
        /// <summary>

        public RelayCommand<object> SavePrintCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SavePrint command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSavePrintCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SavePrint command is executed.
        /// </summary>
        private void OnSavePrintCommandExecute(object param)
        {
            //Save SaleOrder
            SaveSalesOrder(SelectedSaleOrder);

            //Print SaleOrder
            //Print Methods here

        }
        #endregion

        #region QuantityCommand
        /// <summary>
        /// Gets the Quantity Command.
        /// <summary>

        public RelayCommand<object> QuantityCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Quantity command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnQuantityCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Quantity command is executed.
        /// </summary>
        private void OnQuantityCommandExecute(object param)
        {

        }
        #endregion

        #region PriceCommand
        /// <summary>
        /// Gets the Price Command.
        /// <summary>

        public RelayCommand<object> PriceCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Price command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPriceCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Price command is executed.
        /// </summary>
        private void OnPriceCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region SellGiftCardCommand
        /// <summary>
        /// Gets the SellGiftCard Command.
        /// <summary>

        public RelayCommand<object> SellGiftCardCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SellGiftCard command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSellGiftCardCommandCanExecute(object param)
        {

            return ExtensionProducts != null && ExtensionProducts.Any();
        }


        /// <summary>
        /// Method to invoke when the SellGiftCard command is executed.
        /// </summary>
        private void OnSellGiftCardCommandExecute(object param)
        {
            if (ExtensionProducts.Any())
            {
                SelectedProduct = ExtensionProducts.FirstOrDefault();
            }
        }
        #endregion

        #region LockBillCommand
        /// <summary>
        /// Gets the LockBill Command.
        /// <summary>

        public RelayCommand<object> LockBillCommand { get; private set; }



        /// <summary>
        /// Method to check whether the LockBill command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLockBillCommandCanExecute(object param)
        {
            return SelectedSaleOrder != null && IsDirty;
        }


        /// <summary>
        /// Method to invoke when the LockBill command is executed.
        /// </summary>
        private void OnLockBillCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_LockOrder"), Language.GetMsg("InformationCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                SelectedSaleOrder.IsLocked = true;
                SaveSalesOrder(SelectedSaleOrder);

                CreateNewSaleOrder();
            }

        }
        #endregion

        #region SearchBillCommand
        /// <summary>
        /// Gets the SearchBill Command.
        /// <summary>

        public RelayCommand<object> SearchBillCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SearchBill command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchBillCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SearchBill command is executed.
        /// </summary>
        private void OnSearchBillCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region DiscountCommand
        /// <summary>
        /// Gets the Discount Command.
        /// <summary>

        public RelayCommand<object> DiscountCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Discount command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDiscountCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Discount command is executed.
        /// </summary>
        private void OnDiscountCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region AddCustomerCommand
        /// <summary>
        /// Gets the AddCustomer Command.
        /// <summary>

        public RelayCommand<object> AddCustomerCommand { get; private set; }



        /// <summary>
        /// Method to check whether the AddCustomer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddCustomerCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddCustomer command is executed.
        /// </summary>
        private void OnAddCustomerCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region CashInCommand
        /// <summary>
        /// Gets the CashIn Command.
        /// <summary>

        public RelayCommand<object> CashInCommand { get; private set; }



        /// <summary>
        /// Method to check whether the CashIn command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCashInCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CashIn command is executed.
        /// </summary>
        private void OnCashInCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region CashOutCommand
        /// <summary>
        /// Gets the CashOut Command.
        /// <summary>

        public RelayCommand<object> CashOutCommand { get; private set; }



        /// <summary>
        /// Method to check whether the CashOut command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCashOutCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CashOut command is executed.
        /// </summary>
        private void OnCashOutCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region PaymentCommand
        /// <summary>
        /// Gets the Payment Command.
        /// <summary>

        public RelayCommand<object> PaymentCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Payment command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPaymentCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary>
        private void OnPaymentCommandExecute(object param)
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region LockScreenCommand
        /// <summary>
        /// Gets the LockScreen Command.
        /// <summary>

        public RelayCommand<object> LockScreenCommand { get; private set; }



        /// <summary>
        /// Method to check whether the LockScreen command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLockScreenCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the LockScreen command is executed.
        /// </summary>
        private void OnLockScreenCommandExecute(object param)
        {
            LockScreenExecuted();
        }


        #endregion

        #region VoidBill
        /// <summary>
        /// Gets the VoidBill Command.
        /// <summary>

        public RelayCommand<object> VoidBillCommand { get; private set; }



        /// <summary>
        /// Method to check whether the VoidBill command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnVoidBillCommandCanExecute(object param)
        {
            return SelectedSaleOrder != null && !SelectedSaleOrder.IsNew;
        }


        /// <summary>
        /// Method to invoke when the VoidBill command is executed.
        /// </summary>
        private void OnVoidBillCommandExecute(object param)
        {
            //"Do you want to Void this item?"
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_VoidBill"), Language.GetMsg("SO_Title_VoidBill"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Is(MessageBoxResult.Yes))
            {
                //ReasonViewModel reasonViewModel = new ReasonViewModel(SelectedSaleOrder.VoidReason);
                //bool? dialogResult = _dialogService.ShowDialog<ReasonView>(_ownerViewModel, reasonViewModel, Language.GetMsg("SO_Title_VoidBill"));
                //if (dialogResult == true)
                //{
                //    SelectedSaleOrder.VoidReason = reasonViewModel.Reason;
                //    SelectedSaleOrder.IsVoided = true;
                //    SelectedSaleOrder.OrderStatus = (int)SaleOrderStatus.Void;
                //    SaveSalesOrder(SelectedSaleOrder);
                //}
            }
        }
        #endregion

        #region CloseCommand
        /// <summary>
        /// Gets the Close Command.
        /// <summary>

        public RelayCommand<object> CloseCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Close command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCloseCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Close command is executed.
        /// </summary>
        private void OnCloseCommandExecute(object param)
        {
            App.Current.Shutdown();
        }
        #endregion

        #region SearchProductCommand
        /// <summary>
        /// Gets the SearchProduct Command.
        /// <summary>

        public RelayCommand<object> SearchProductCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SearchProduct command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchProductCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SearchProduct command is executed.
        /// </summary>
        private void OnSearchProductCommandExecute(object param)
        {
            SearchProductByBarcode();
        }


        #endregion

        #region MoveItemCommand
        /// <summary>
        /// Gets the MoveItem Command.
        /// <summary>

        public RelayCommand<object> MoveItemCommand { get; private set; }



        /// <summary>
        /// Method to check whether the MoveItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMoveItemCommandCanExecute(object param)
        {
            return SelectedSaleOrder != null && SelectedSaleOrder.SaleOrderDetailCollection != null;
        }


        /// <summary>
        /// Method to invoke when the MoveItem command is executed.
        /// </summary>
        private void OnMoveItemCommandExecute(object param)
        {
            MovingItemExecute(param.ToString());
        }


        #endregion

        #region DeleteItemCommand
        /// <summary>
        /// Gets the DeleteItem Command.
        /// <summary>

        public RelayCommand<object> DeleteItemCommand { get; private set; }



        /// <summary>
        /// Method to check whether the DeleteItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteItemCommandCanExecute(object param)
        {
            return SelectedSaleOrderDetail != null;
        }


        /// <summary>
        /// Method to invoke when the DeleteItem command is executed.
        /// </summary>
        private void OnDeleteItemCommandExecute(object param)
        {
            if (SelectedSaleOrderDetail.PickQty == 0)
            {
                DeleteItemSaleOrderDetail(SelectedSaleOrderDetail);
            }
        }
        #endregion


        #region ShowOnScreenKeyboardCommand
        /// <summary>
        /// Gets the ShowOnScreenKeyboard Command.
        /// <summary>

        public RelayCommand<object> ShowOnScreenKeyboardCommand { get; private set; }



        /// <summary>
        /// Method to check whether the ShowOnScreenKeyboard command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShowOnScreenKeyboardCommandCanExecute(object param)
        {
            return true;
        }

        OnScreenKeyboard view;
        /// <summary>
        /// Method to invoke when the ShowOnScreenKeyboard command is executed.
        /// </summary>
        private void OnShowOnScreenKeyboardCommandExecute(object param)
        {

            //Window owner = FindOwnerWindow(_ownerViewModel);

            //keyboardView.ShowInTaskbar = false;
            //keyboardView.Show();

            OnScreenKeyboard view = new OnScreenKeyboard();
            view.Show();



           
            ////Call Keyboard from another thread
            //Thread newWindowThread = new Thread(new ThreadStart(() =>
            //{

            //    view = new OnScreenKeyboard();
            //    view.ShowInTaskbar = false;
            //    view.Topmost = true;
            //    view.Show();
            //    System.Windows.Threading.Dispatcher.Run();
            //}));
            //newWindowThread.SetApartmentState(ApartmentState.STA);
            //newWindowThread.IsBackground = true;
            //newWindowThread.Start();
            //view.Owner = App.Current.MainWindow;
            
            
        }
        private static AutoResetEvent s_event = new AutoResetEvent(false);



        #endregion

        #endregion

        #region Private Methods

        #region Initial & LoadData

        private void InitialCommand()
        {
            NewCommand = new RelayCommand<object>(OnNewCommandExecute, OnNewCommandCanExecute);
            SavePrintCommand = new RelayCommand<object>(OnSavePrintCommandExecute, OnSavePrintCommandCanExecute);
            CloseCommand = new RelayCommand<object>(OnCloseCommandExecute, OnCloseCommandCanExecute);
            VoidBillCommand = new RelayCommand<object>(OnVoidBillCommandExecute, OnVoidBillCommandCanExecute);
            QuantityCommand = new RelayCommand<object>(OnQuantityCommandExecute, OnQuantityCommandCanExecute);
            PriceCommand = new RelayCommand<object>(OnPriceCommandExecute, OnPriceCommandCanExecute);
            SellGiftCardCommand = new RelayCommand<object>(OnSellGiftCardCommandExecute, OnSellGiftCardCommandCanExecute);
            LockBillCommand = new RelayCommand<object>(OnLockBillCommandExecute, OnLockBillCommandCanExecute);
            SearchBillCommand = new RelayCommand<object>(OnSearchBillCommandExecute, OnSearchBillCommandCanExecute);
            DiscountCommand = new RelayCommand<object>(OnDiscountCommandExecute, OnDiscountCommandCanExecute);
            AddCustomerCommand = new RelayCommand<object>(OnAddCustomerCommandExecute, OnAddCustomerCommandCanExecute);
            CashInCommand = new RelayCommand<object>(OnCashInCommandExecute, OnCashInCommandCanExecute);
            CashOutCommand = new RelayCommand<object>(OnCashOutCommandExecute, OnCashOutCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);
            LockScreenCommand = new RelayCommand<object>(OnLockScreenCommandExecute, OnLockScreenCommandCanExecute);

            MoveItemCommand = new RelayCommand<object>(OnMoveItemCommandExecute, OnMoveItemCommandCanExecute);
            SearchProductCommand = new RelayCommand<object>(OnSearchProductCommandExecute, OnSearchProductCommandCanExecute);
            DeleteItemCommand = new RelayCommand<object>(OnDeleteItemCommandExecute, OnDeleteItemCommandCanExecute);
            ShowOnScreenKeyboardCommand = new RelayCommand<object>(OnShowOnScreenKeyboardCommandExecute, OnShowOnScreenKeyboardCommandCanExecute);
        }

        /// <summary>
        /// Load Dynamic Data
        /// </summary>
        private void LoadDynamicData()
        {
            LoadSaleTax();

            LoadProductExtension();

            LoadDiscountProgram();

            LoadCustomer();

            LoadRewardManagers();
        }

        /// <summary>
        /// Load SaleTaxCollection
        /// </summary>
        protected void LoadSaleTax()
        {
            IList<base_SaleTaxLocation> saleTaxList = _saleTaxRepository.GetAll();
            if (SaleTaxLocationCollection == null)
                SaleTaxLocationCollection = new List<base_SaleTaxLocationModel>(saleTaxList.Select(x => new base_SaleTaxLocationModel(x)));
            else
            {
                foreach (base_SaleTaxLocation saleTax in saleTaxList)
                {
                    if (SaleTaxLocationCollection.Any(x => x.Id.Equals(saleTax.Id)))
                    {
                        base_SaleTaxLocationModel saleTaxModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id.Equals(saleTax.Id));
                        saleTaxModel.UpdateModel(saleTax);
                        saleTaxModel.EndUpdate();
                    }
                    else
                    {
                        SaleTaxLocationCollection.Add(new base_SaleTaxLocationModel(saleTax));
                    }
                }

                //Remove Item From Local collection if in db collection is not existed
                IList<int> itemReomoveList = SaleTaxLocationCollection.Select(x => x.Id).Except(saleTaxList.Select(x => x.Id)).ToList();
                if (itemReomoveList != null)
                {
                    SaleTaxLocationCollection.RemoveAll(x => itemReomoveList.Contains(x.Id));
                }
            }
        }

        /// <summary>
        /// Load Extension product for search
        /// <para>Gift Card</para>
        /// </summary>
        protected void LoadProductExtension()
        {
            if (!ExtensionProducts.Any(x => x.IsCoupon))
            {
                string strGuidPatern = "{0}{0}{0}{0}{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                string strCodePatern = "{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                string strBarCodePatern = "{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";

                Guid giftCardGuid = Guid.Parse(string.Format(strGuidPatern, 1));
                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(PaymentMethod.GiftCard) && !ExtensionProducts.Any(x => x.Resource.Equals(giftCardGuid)))
                {
                    base_ProductModel couponItem = new base_ProductModel();
                    ComboItem card = Common.PaymentMethods.FirstOrDefault(x => x.Value.Equals((short)PaymentMethod.GiftCard));
                    couponItem.ProductName = card.Text;
                    //Using for CardTypeId
                    couponItem.ProductCategoryId = card.Value;
                    couponItem.Code = string.Format(strCodePatern, 1);//64 : PaymentMethod.GiftCard
                    couponItem.Barcode = string.Format(strBarCodePatern, 1);
                    couponItem.Resource = giftCardGuid;
                    couponItem.IsOpenItem = true;
                    couponItem.IsCoupon = true;
                    ExtensionProducts.Add(couponItem);
                }

                Guid giftCertificateGuid = Guid.Parse(string.Format(strGuidPatern, 2));
                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(PaymentMethod.GiftCertificate) && !ExtensionProducts.Any(x => x.Resource.Equals(giftCertificateGuid)))
                {
                    base_ProductModel couponItem = new base_ProductModel();
                    ComboItem card = Common.PaymentMethods.FirstOrDefault(x => x.Value.Equals((short)PaymentMethod.GiftCertificate));
                    //Using for CardTypeId
                    couponItem.ProductCategoryId = card.Value;
                    couponItem.ProductName = card.Text;
                    couponItem.Code = string.Format(strCodePatern, 2);//128 : PaymentMethod.GiftCertificate
                    couponItem.Barcode = string.Format(strBarCodePatern, 2);
                    couponItem.Resource = giftCertificateGuid;
                    couponItem.IsOpenItem = true;
                    couponItem.IsCoupon = true;
                    ExtensionProducts.Add(couponItem);
                }

            }

        }

        /// <summary>
        /// Load Discount Program
        /// </summary>
        protected void LoadDiscountProgram()
        {
            short promotionStatus = (short)StatusBasic.Active;
            IEnumerable<base_Promotion> promotionList = _promotionRepository.GetAll(x => x.Status.Equals(promotionStatus));
            if (_promotionList == null || !_promotionList.Any())
            {
                _promotionList = new List<base_PromotionModel>(promotionList.OrderByDescending(x => x.DateUpdated).Select(x => new base_PromotionModel(x)));
            }
            else
            {
                foreach (base_Promotion promotion in promotionList)
                {
                    if (_promotionList.Any(x => x.Id.Equals(promotion.Id)))
                    {
                        base_PromotionModel promotionModel = _promotionList.SingleOrDefault(x => x.Id.Equals(promotion.Id));
                        _promotionRepository.Refresh(promotionModel.base_Promotion);
                        promotionModel.ToModel();
                        promotionModel.EndUpdate();
                    }
                    else
                    {
                        _promotionRepository.Refresh(promotion);
                        _promotionList.Add(new base_PromotionModel(promotion));
                    }
                }
            }
        }

        /// <summary>
        /// Load All Customer From DB
        /// </summary>
        protected virtual void LoadCustomer()
        {
            lock (UnitOfWork.Locker)
            {
                string guidString = Define.DefaultGuestId.ToString("00000000-0000-0000-0000-00000000000#");
                Guid defaultGuid = Guid.Parse(guidString);
                IList<base_Guest> customerList = _guestRepository.GetAll(x => x.Mark.Equals(CUSTOMER_MARK) && (!x.IsPurged || x.Resource == defaultGuid));//&& x.IsActived

                if (CustomerCollection == null)
                    CustomerCollection = new CollectionBase<base_GuestModel>();//customerList.OrderBy(x => x.Id).Select(x => new base_GuestModel(x))

                CustomerCollection.DeletedItems.Clear();
                //else
                //{
                foreach (base_Guest customer in customerList)
                {
                    if (customer.IsActived)
                    {
                        //Check Item is existed,update model for item
                        if (CustomerCollection.Any(x => x.Resource.Equals(customer.Resource)))
                        {
                            base_GuestModel customerModel = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(customer.Resource));
                            customerModel.UpdateModel(customer);
                            customerModel.EndUpdate();
                        }
                        else //Add new item
                        {
                            CustomerCollection.Add(new base_GuestModel(customer));
                        }
                    }
                    else
                    {
                        //Remove item has in customer collection is deActived
                        if (CustomerCollection.Any(x => x.Resource.Equals(customer.Resource)))
                        {
                            base_GuestModel itemRemoved = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(customer.Resource));
                            CustomerCollection.Remove(itemRemoved);
                        }
                        else
                        {
                            CustomerCollection.DeletedItems.Add(new base_GuestModel(customer));
                        }
                    }
                }
                //Remove Item From Local collection if in db collection is not existed
                IList<Guid?> itemReomoveList = CustomerCollection.Select(x => x.Resource).Except(customerList.Where(x => x.IsActived).Select(x => x.Resource)).ToList();
                if (itemReomoveList != null)
                {
                    foreach (Guid resource in itemReomoveList)
                    {
                        base_GuestModel itemRemoved = CustomerCollection.SingleOrDefault(x => x.Resource.Equals(resource));
                        CustomerCollection.Remove(itemRemoved);
                        CustomerCollection.DeletedItems.Remove(itemRemoved);
                    }
                }
                //}
            }
        }

        /// <summary>
        /// Load Reward Management
        /// </summary>
        protected virtual void LoadRewardManagers()
        {
            if (RewardManagerCollection == null || RewardManagerCollection.Any())
            {
                IList<base_RewardManager> rewards = _rewardManagerRepository.GetAll();
                if (RewardManagerCollection == null)
                {
                    RewardManagerCollection = new List<base_RewardManager>(rewards);
                }
                else
                {
                    foreach (base_RewardManager reward in rewards)
                    {
                        _rewardManagerRepository.Refresh(reward);
                        if (RewardManagerCollection.Any(x => x.Id.Equals(reward.Id)))
                        {
                            base_RewardManager rewardUpdated = RewardManagerCollection.SingleOrDefault(x => x.Id.Equals(reward.Id));
                            rewardUpdated = reward;
                        }
                        else
                        {
                            RewardManagerCollection.Add(reward);
                        }

                    }
                }

            }
        }
        //Initial
        /// <summary>
        /// Create New saleOrder
        /// </summary>
        /// 
        private base_SaleOrderModel CreateNewSaleOrder()
        {
            try
            {
                _selectedSaleOrder = new base_SaleOrderModel();
                _selectedSaleOrder.Shift = Define.ShiftCode;
                _selectedSaleOrder.IsTaxExemption = false;
                _selectedSaleOrder.IsConverted = true;
                _selectedSaleOrder.IsLocked = false;
                _selectedSaleOrder.SONumber = DateTime.Now.ToString(Define.GuestNoFormat);
                _saleOrderRepository.SOCardGenerate(_selectedSaleOrder, _selectedSaleOrder.SONumber);
                _selectedSaleOrder.DateCreated = DateTime.Now;
                _selectedSaleOrder.BookingChanel = Convert.ToInt16(Common.BookingChannel.First().ObjValue);
                _selectedSaleOrder.StoreCode = Define.StoreCode;//Default StoreCode
                _selectedSaleOrder.OrderDate = DateTime.Now;
                _selectedSaleOrder.RequestShipDate = DateTime.Now;
                _selectedSaleOrder.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                _selectedSaleOrder.TaxPercent = 0;
                _selectedSaleOrder.TaxAmount = 0;
                _selectedSaleOrder.Deposit = 0;
                _selectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
                _selectedSaleOrder.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(_selectedSaleOrder.OrderStatus));
                _selectedSaleOrder.Mark = MarkType.SaleOrder.ToDescription();
                _selectedSaleOrder.TermNetDue = 0;
                _selectedSaleOrder.TermDiscountPercent = 0;
                _selectedSaleOrder.TermPaidWithinDay = 0;
                _selectedSaleOrder.PaymentTermDescription = string.Empty;
                //Set Price Schema
                _selectedSaleOrder.PriceSchemaId = 1;
                _selectedSaleOrder.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(_selectedSaleOrder.PriceSchemaId));

                _selectedSaleOrder.TaxExemption = string.Empty;
                _selectedSaleOrder.SaleRep = string.Empty;
                _selectedSaleOrder.Resource = Guid.NewGuid();
                _selectedSaleOrder.WeightUnit = Common.ShipUnits.First().Value;
                _selectedSaleOrder.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit
                _selectedSaleOrder.WeightUnit = Define.CONFIGURATION.DefaultShipUnit.HasValue ? Define.CONFIGURATION.DefaultShipUnit.Value : Convert.ToInt16(Common.ShipUnits.First().ObjValue);
                _selectedSaleOrder.IsHiddenErrorColumn = true;

                _selectedSaleOrder.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                _selectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                //Get TaxLocation & taxCodeModel
                GetSaleTax(_selectedSaleOrder);

                //Create a sale order detail collection
                _selectedSaleOrder.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
                //_selectedSaleOrder.SaleOrderDetailCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(SaleOrderDetailCollection_CollectionChanged);

                //create a sale order Ship Collection
                _selectedSaleOrder.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();
                _selectedSaleOrder.SaleOrderShippedCollection = new CollectionBase<base_SaleOrderDetailModel>();

                // Create new payment collection
                _selectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                //ReturnModel & ReturnDetailCollection
                //_selectedSaleOrder.ReturnModel = new base_ResourceReturnModel();
                //_selectedSaleOrder.ReturnModel.DocumentNo = SelectedSaleOrder.SONumber;
                //_selectedSaleOrder.ReturnModel.DocumentResource = SelectedSaleOrder.Resource.ToString();
                //_selectedSaleOrder.ReturnModel.TotalAmount = SelectedSaleOrder.Total;
                //_selectedSaleOrder.ReturnModel.Resource = Guid.NewGuid();
                //_selectedSaleOrder.ReturnModel.TotalRefund = 0;
                //_selectedSaleOrder.ReturnModel.TotalAmount = 0;
                //_selectedSaleOrder.ReturnModel.Mark = "SO";
                //_selectedSaleOrder.ReturnModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                //_selectedSaleOrder.ReturnModel.DateCreated = DateTime.Today;
                //_selectedSaleOrder.ReturnModel.IsDirty = false;
                //_selectedSaleOrder.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
                ////_selectedSaleOrder.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;
                //_selectedSaleOrder.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();


                //Additional
                _selectedSaleOrder.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing, IsDirty = false };
                _selectedSaleOrder.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping, IsDirty = false };

                //GuestRewardSaleOrder
                _selectedSaleOrder.GuestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel();

                _selectedCustomer = null;
                OnPropertyChanged(() => SelectedCustomer);

                //Set to fist tab & skip TabChanged Methods in SelectedTabIndex property
                _selectedSaleOrder.IsDirty = false;
                _selectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
                //_selectedSaleOrder.ReturnModel.PropertyChanged += new PropertyChangedEventHandler(ReturnModel_PropertyChanged);
                OnPropertyChanged(() => SelectedSaleOrder);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return _selectedSaleOrder;
        }

        /// <summary>
        /// Create New Member Ship Model & Gen Barcode
        /// </summary>
        /// <returns></returns>
        protected base_MemberShipModel NewMemberShipModel(base_GuestModel customerModel)
        {
            try
            {
                base_MemberShipModel memberShipModel = new base_MemberShipModel();
                memberShipModel.Status = (short)MemberShipStatus.Actived;
                memberShipModel.GuestResource = customerModel.Resource.ToString();
                memberShipModel.DateCreated = DateTime.Today;
                memberShipModel.DateUpdated = DateTime.Now;
                memberShipModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                memberShipModel.IsPurged = false;
                memberShipModel.Status = Convert.ToInt16(MemberShipStatus.Actived);
                memberShipModel.MemberType = MemberShipType.Normal.ToDescription();

                IdCardBarcodeGen(memberShipModel);
                return memberShipModel;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        #endregion

        #region SaveData
        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void InsertSaleOrder(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (SelectedSaleOrder.IsNew)
                {
                    //Sale Order Detail Model
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                    {
                        if (saleOrderDetailModel.ProductModel.IsCoupon)
                        {
                            if (saleOrderDetailModel.CouponCardModel.IsNew)
                            {
                                saleOrderDetailModel.CouponCardModel.GuestResourcePurchased = SelectedSaleOrder.GuestModel.Resource.ToString();
                                saleOrderDetailModel.CouponCardModel.GuestGiftedResource = SelectedSaleOrder.GuestModel.Resource.ToString();
                                saleOrderDetailModel.CouponCardModel.RemainingAmount = saleOrderDetailModel.CouponCardModel.InitialAmount;
                                saleOrderDetailModel.CouponCardModel.PurchaseDate = DateTime.Now;
                                saleOrderDetailModel.CouponCardModel.DateCreated = DateTime.Now;
                            }
                            saleOrderDetailModel.CouponCardModel.ToEntity();
                        }

                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, SelectedSaleOrder.StoreCode, saleOrderDetailModel.Quantity);

                        saleOrderDetailModel.ToEntity();
                        SelectedSaleOrder.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                    }
                    _productRepository.Commit();

                    SavePaymentCollection(SelectedSaleOrder);

                    //Set DateCreated/ Updated/ User to SO
                    SelectedSaleOrder.Shift = Define.ShiftCode;
                    SelectedSaleOrder.DateUpdated = DateTime.Now;
                    SelectedSaleOrder.DateCreated = DateTime.Now;
                    SelectedSaleOrder.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                    SelectedSaleOrder.ToEntity();
                    _saleOrderRepository.Add(SelectedSaleOrder.base_SaleOrder);

                    _saleOrderRepository.Commit();
                    SelectedSaleOrder.EndUpdate();
                    //Set ID
                    SelectedSaleOrder.ToModel();
                    SelectedSaleOrder.EndUpdate();
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                    {
                        if (saleOrderDetailModel.ProductModel.IsCoupon)
                        {
                            saleOrderDetailModel.CouponCardModel.EndUpdate();
                        }
                        saleOrderDetailModel.ToModel();

                        saleOrderDetailModel.EndUpdate();
                    }

                    if (SelectedSaleOrder.PaymentCollection != null)
                    {
                        foreach (base_ResourcePaymentModel paymentModel in SelectedSaleOrder.PaymentCollection.Where(x => x.IsNew))
                        {
                            paymentModel.ToModel();
                            //Update or Add New PaymentDetail
                            if (paymentModel.PaymentDetailCollection != null)
                            {
                                foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentModel.PaymentDetailCollection.Where(x => x.IsNew))
                                {
                                    paymentDetailModel.ToModel();
                                    paymentDetailModel.EndUpdate();
                                }
                            }
                            paymentModel.EndUpdate();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void UpdateSaleOrder(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                #region SaleOrderDetail
                SaveOrderDetailCollection(saleOrderModel);
                #endregion

                #region SaleOrderShip
                SaveSaleOrderShipCollection(saleOrderModel);
                #endregion

                #region SaleOrderReturn
                SaveReturnCollection(saleOrderModel);
                #endregion

                #region PaymentCollection
                SavePaymentCollection(saleOrderModel);
                #endregion

                #region CommissionCollection
                if (saleOrderModel.CommissionCollection != null && saleOrderModel.CommissionCollection.Any())
                {
                    foreach (base_SaleCommissionModel saleCommissionModel in saleOrderModel.CommissionCollection)
                    {
                        saleCommissionModel.ToEntity();
                        if (saleCommissionModel.IsNew)
                            _saleCommissionRepository.Add(saleCommissionModel.base_SaleCommission);
                    }
                    _saleCommissionRepository.Commit();
                    saleOrderModel.CommissionCollection.Clear();
                }
                #endregion

                saleOrderModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                //set dateUpdate
                saleOrderModel.DateUpdated = DateTime.Now;

                saleOrderModel.ToEntity();
                _saleOrderRepository.Commit();
                _productRepository.Commit();

                //Set ID
                #region Update Id & Set End Update
                saleOrderModel.ToModel();
                saleOrderModel.EndUpdate();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        saleOrderDetailModel.CouponCardModel.EndUpdate();
                    }
                    saleOrderDetailModel.ToModel();
                    saleOrderDetailModel.EndUpdate();
                }

                foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection)
                {
                    saleOrderShipModel.ToModel();
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        saleOrderShipDetailModel.ToModel();
                        saleOrderShipDetailModel.EndUpdate();
                    }
                    saleOrderShipModel.EndUpdate();
                }

                //Update ID For Payment
                if (saleOrderModel.PaymentCollection != null)
                {
                    foreach (base_ResourcePaymentModel paymentModel in saleOrderModel.PaymentCollection.Where(x => x.IsNew))
                    {
                        paymentModel.ToModel();
                        //Update or Add New PaymentDetail
                        if (paymentModel.PaymentDetailCollection != null)
                        {
                            foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentModel.PaymentDetailCollection.Where(x => x.IsNew))
                            {
                                paymentDetailModel.ToModel();
                                paymentDetailModel.EndUpdate();
                            }
                        }
                        paymentModel.EndUpdate();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Save Sale Order
        /// </summary>
        /// <returns></returns>
        private bool SaveSalesOrder(base_SaleOrderModel saleOrderModel)
        {
            bool result = false;
            try
            {
                UnitOfWork.BeginTransaction();
                if (saleOrderModel.IsNew)
                    InsertSaleOrder(saleOrderModel);
                else
                    UpdateSaleOrder(saleOrderModel);

                UpdateCustomer(saleOrderModel);
                UnitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                result = false;
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            return result;
        }

        /// <summary>
        /// Save OrderDetail Collection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SaveOrderDetailCollection(base_SaleOrderModel saleOrderModel)
        {
            //Delete SaleOrderDetail
            if (saleOrderModel.SaleOrderDetailCollection.DeletedItems.Any())
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection.DeletedItems)
                {
                    if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        saleOrderDetailModel.CouponCardModel.ResetCard();
                        saleOrderDetailModel.CouponCardModel.ToEntity();
                    }
                    //Get quantity from entity to substract store(avoid quantity in model is changed)
                    _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                    _saleOrderDetailRepository.Delete(saleOrderDetailModel.base_SaleOrderDetail);
                }
                _saleOrderDetailRepository.Commit();
                saleOrderModel.SaleOrderDetailCollection.DeletedItems.Clear();
            }

            if (saleOrderModel.IsVoided)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        saleOrderDetailModel.CouponCardModel.ResetCard();
                        saleOrderDetailModel.CouponCardModel.ToEntity();
                    }
                    _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                }
            }
            else
            {
                //Sale Order Detail Model
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection.Where(x => x.IsDirty))
                {
                    if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        if (saleOrderDetailModel.CouponCardModel.IsNew)
                        {
                            saleOrderDetailModel.CouponCardModel.GuestResourcePurchased = saleOrderModel.GuestModel.Resource.ToString();
                            saleOrderDetailModel.CouponCardModel.GuestGiftedResource = saleOrderModel.GuestModel.Resource.ToString();
                            saleOrderDetailModel.CouponCardModel.RemainingAmount = saleOrderDetailModel.CouponCardModel.InitialAmount;
                            saleOrderDetailModel.CouponCardModel.PurchaseDate = DateTime.Now;
                            saleOrderDetailModel.CouponCardModel.DateCreated = DateTime.Now;
                        }
                        saleOrderDetailModel.CouponCardModel.ToEntity();
                    }

                    //Need to check difference store code (user change to another store)
                    if (saleOrderModel.StoreCode.Equals(saleOrderModel.base_SaleOrder.StoreCode))
                    {
                        if (saleOrderDetailModel.Quantity != saleOrderDetailModel.base_SaleOrderDetail.Quantity || saleOrderDetailModel.UOMId != saleOrderDetailModel.base_SaleOrderDetail.UOMId) //addition quantity
                        {
                            _saleOrderRepository.UpdateCustomerQuantityChanged(saleOrderDetailModel, saleOrderModel.StoreCode);
                        }
                    }
                    else
                    {
                        //Subtract quantity from "old store"(user change to another store)
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                        //Add quantity to new store
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.StoreCode, saleOrderDetailModel.Quantity);
                    }

                    saleOrderDetailModel.ToEntity();
                    if (saleOrderDetailModel.IsNew)
                        saleOrderModel.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                    saleOrderDetailModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Save Return Process
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SaveReturnCollection(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.ReturnModel != null)
            {
                bool calcGuestReward = false;

                saleOrderModel.ReturnModel.ToEntity();
                //Update Refund for SaleOrder
                saleOrderModel.RefundedAmount = saleOrderModel.ReturnModel.TotalRefund < 0 ? saleOrderModel.ReturnModel.TotalRefund * -1 : saleOrderModel.ReturnModel.TotalRefund;

                if (saleOrderModel.ReturnModel.IsNew && saleOrderModel.ReturnModel.ReturnDetailCollection.DeletedItems.Any())
                {
                    foreach (base_ResourceReturnDetailModel returnDetailModel in saleOrderModel.ReturnModel.ReturnDetailCollection.DeletedItems.Where(x => !x.IsTemporary))
                        _resourceReturnDetailRepository.Delete(returnDetailModel.base_ResourceReturnDetail);
                }
                //Clear which item deleted in collection
                saleOrderModel.ReturnModel.ReturnDetailCollection.DeletedItems.Clear();

                var reward = GetReward(saleOrderModel.OrderDate.Value.Date);

                //Amount Of product is Eligible reward returned
                decimal productReturnRewardAmount = 0;

                //Total Reward After Return 
                decimal totalRewardBeforeReturn = 0;
                if (reward != null)
                    totalRewardBeforeReturn = saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold;

                foreach (base_ResourceReturnDetailModel returnDetailModel in saleOrderModel.ReturnModel.ReturnDetailCollection.Where(x => x.IsDirty))
                {
                    if (returnDetailModel.SaleOrderDetailModel.ProductModel != null)
                    {
                        if (returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                        {
                            returnDetailModel.SaleOrderDetailModel.CouponCardModel.Status = (short)StatusBasic.Deactive;
                            returnDetailModel.SaleOrderDetailModel.CouponCardModel.ToEntity();
                        }
                        else
                        {
                            decimal totalQuantityBaseUom = 0;
                            if (!returnDetailModel.base_ResourceReturnDetail.IsReturned && returnDetailModel.IsReturned)//New Item Return
                            {
                                base_ProductUOMModel baseUnitProduct = returnDetailModel.SaleOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId.Equals(returnDetailModel.SaleOrderDetailModel.UOMId));

                                if (baseUnitProduct != null)
                                {
                                    decimal quantityBaseUnit = baseUnitProduct.BaseUnitNumber;

                                    totalQuantityBaseUom = quantityBaseUnit * returnDetailModel.ReturnQty;
                                    //Update Product Profit
                                    _productRepository.UpdateProductStore(returnDetailModel.ProductResource, saleOrderModel.StoreCode, 0, 0, totalQuantityBaseUom, returnDetailModel.Price * returnDetailModel.ReturnQty, true);

                                    //Increase Store for return product
                                    _productRepository.UpdateOnHandQuantity(returnDetailModel.ProductResource, saleOrderModel.StoreCode, totalQuantityBaseUom);

                                    //Calculate return commission for Employee & Manager
                                    CommissionReturn(saleOrderModel, returnDetailModel);

                                    //Subtract PurchaseTrackingPeriod with Product Eligible Reward
                                    if (returnDetailModel.SaleOrderDetailModel.ProductModel.IsEligibleForReward)
                                    {
                                        calcGuestReward = true;
                                        productReturnRewardAmount += (returnDetailModel.Amount);// + returnDetailModel.VAT
                                    }
                                }
                            }
                        }
                    }


                    //Has Payment & Create reward (Has sum in PurchaseDuringTrackingPeriod)
                    if (saleOrderModel.GuestRewardSaleOrderModel != null && !string.IsNullOrWhiteSpace(saleOrderModel.GuestRewardSaleOrderModel.SaleOrderResource))
                        saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod -= productReturnRewardAmount;

                    returnDetailModel.ToEntity();
                    if (returnDetailModel.IsNew)
                        saleOrderModel.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail.Add(returnDetailModel.base_ResourceReturnDetail);
                }

                //Handle Return Reward For reward Member
                if (saleOrderModel.GuestModel.IsRewardMember && calcGuestReward && reward != null)
                {
                    //CustomerReturnReward(saleOrderModel, reward, productReturnRewardAmount, totalRewardBeforeReturn, false/*UpdateValue & delete reward*/);
                    //Calculate Next Reward
                    saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold - ((saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold) % 1) * reward.PurchaseThreshold;
                }

                //SaveStoreCardReturned
                SaveStoreCardReturned(saleOrderModel);

                if (saleOrderModel.ReturnModel.IsNew)
                    _resourceReturnRepository.Add(saleOrderModel.ReturnModel.base_ResourceReturn);
                _resourceReturnRepository.Commit();

                //Check Has any Return
                saleOrderModel.IsReturned = saleOrderModel.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned);

                calcGuestReward = false;
                //Update ID
                saleOrderModel.ReturnModel.Id = saleOrderModel.ReturnModel.base_ResourceReturn.Id;
                saleOrderModel.ReturnModel.EndUpdate();

                foreach (base_ResourceReturnDetailModel returnDetailModel in saleOrderModel.ReturnModel.ReturnDetailCollection.Where(x => x.IsDirty))
                {
                    returnDetailModel.Id = returnDetailModel.base_ResourceReturnDetail.Id;
                    returnDetailModel.ResourceReturnId = returnDetailModel.base_ResourceReturnDetail.ResourceReturnId;
                    returnDetailModel.EndUpdate();
                }

            }
        }

        /// <summary>
        /// Save payment collection, payment detail and payment product
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void SavePaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.PaymentCollection != null)
                {
                    foreach (base_ResourcePaymentModel paymentItem in saleOrderModel.PaymentCollection.Where(x => x.IsDirty))
                    {
                        // Map data from model to entity
                        paymentItem.ToEntity();

                        if (paymentItem.PaymentDetailCollection != null)
                        {
                            foreach (base_ResourcePaymentDetailModel paymentDetailModel in paymentItem.PaymentDetailCollection.Where(x => x.IsDirty))
                            {
                                // Map data from model to entity
                                paymentDetailModel.ToEntity();
                                if (paymentDetailModel.CouponCardModel != null && paymentDetailModel.CouponCardModel.IsDirty)
                                {
                                    paymentDetailModel.CouponCardModel.RemainingAmount -= paymentDetailModel.Paid;
                                    paymentDetailModel.CouponCardModel.IsUsed = true;
                                    paymentDetailModel.CouponCardModel.LastUsed = DateTime.Now;
                                    paymentDetailModel.CouponCardModel.ToEntity();
                                    paymentDetailModel.CouponCardModel.EndUpdate();
                                }

                                // Add new payment detail to database
                                if (paymentDetailModel.Id == 0)
                                    paymentItem.base_ResourcePayment.base_ResourcePaymentDetail.Add(paymentDetailModel.base_ResourcePaymentDetail);
                            }
                        }

                        if (paymentItem.IsNew)
                        {
                            paymentItem.Shift = Define.ShiftCode;
                            _paymentRepository.Add(paymentItem.base_ResourcePayment);
                        }

                        _paymentRepository.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Save Store Card(gift card customer return transfer to store card)
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SaveStoreCardReturned(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.ReturnModel.StoreCardCollection.Any())
                {
                    foreach (base_CardManagementModel cardModel in saleOrderModel.ReturnModel.StoreCardCollection)
                    {
                        cardModel.ToEntity();
                        if (cardModel.IsNew)
                            _cardManagementRepository.Add(cardModel.base_CardManagement);
                    }
                    _cardManagementRepository.Commit();
                    saleOrderModel.ReturnModel.StoreCardCollection.Clear();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Save Order Ship & Relation to DB
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void SaveSaleOrderShipCollection(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.SaleOrderShipCollection == null)
                    return;

                base_SaleOrderShipRepository saleOrderShipRepository = new base_SaleOrderShipRepository();
                base_SaleOrderShipDetailRepository saleOrderShipDetailRepository = new base_SaleOrderShipDetailRepository();

                if (saleOrderModel.SaleOrderShipCollection.DeletedItems.Any())
                {
                    //Delete Sale Order Ship Model
                    foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection.DeletedItems)
                        saleOrderShipRepository.Delete(saleOrderShipModel.base_SaleOrderShip);
                    saleOrderShipRepository.Commit();
                    saleOrderModel.SaleOrderShipCollection.DeletedItems.Clear();
                }

                //Sale Order Ship Model
                foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection.Where(x => x.IsDirty || x.IsNew))
                {
                    //saleOrderShipModel.IsShipped = saleOrderShipModel.IsChecked;

                    // Delete SaleOrderShipDetail
                    if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Any())
                    {
                        foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModelDel in saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems)
                            saleOrderShipDetailRepository.Delete(saleOrderShipDetailModelDel.base_SaleOrderShipDetail);
                        saleOrderShipDetailRepository.Commit();
                        saleOrderShipModel.SaleOrderShipDetailCollection.DeletedItems.Clear();
                    }

                    //Update SaleOrderShipDetail & Upd
                    if (saleOrderShipModel.SaleOrderShipDetailCollection != null && saleOrderShipModel.SaleOrderShipDetailCollection.Any())
                    {
                        foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                        {
                            //Package is shipped & is a new shipped =>update Onhand,CustomerQty
                            if (saleOrderShipModel.IsShipped && !saleOrderShipModel.base_SaleOrderShip.IsShipped && !saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                            {
                                //Descrease OnHand product Store which product in SaleOrderShipDetail
                                //Descrease store with Product On Hand in group with parent product
                                //Cause : Item Product Group not stockable 
                                if (saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                                {
                                    foreach (base_ProductGroup productGroup in saleOrderShipDetailModel.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1)
                                    {
                                        long productId = productGroup.base_Product.Id;
                                        base_Product product = _productRepository.Get(x => x.Id.Equals(productId));
                                        if (product != null)
                                        {
                                            //Get Product From ProductCollection (child product)
                                            base_ProductModel productInGroupModel = new base_ProductModel(product);

                                            //Get Unit Of Product
                                            base_ProductUOM productGroupUOM = _saleOrderRepository.GetProductUomOfProductInGroup(saleOrderModel.StoreCode, productGroup);
                                            if (productGroupUOM != null)
                                            {
                                                decimal baseUnitNumber = productGroupUOM.BaseUnitNumber;

                                                //productGroup.Quantity : quantity default of group
                                                decimal packQty = productGroup.Quantity * saleOrderShipDetailModel.PackedQty;
                                                _productRepository.UpdateOnHandQuantity(productInGroupModel.Resource.ToString(), saleOrderModel.StoreCode, packQty, true, baseUnitNumber);
                                            }
                                        }
                                        else
                                        {

                                            _log4net.Info("ProductId :'" + productId + "' is not existed");
                                        }
                                    }
                                }
                                else
                                {
                                    decimal baseUnitNumber = saleOrderShipDetailModel.SaleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(saleOrderShipDetailModel.SaleOrderDetailModel.UOMId)).BaseUnitNumber;
                                    _productRepository.UpdateOnHandQuantity(saleOrderShipDetailModel.ProductResource, saleOrderModel.StoreCode, saleOrderShipDetailModel.PackedQty, true, baseUnitNumber);
                                }

                                //Descrease Quantity OnCustomer which product in SaleOrderDetail
                                _saleOrderRepository.UpdateCustomerQuantity(saleOrderShipDetailModel.SaleOrderDetailModel, saleOrderModel.StoreCode, saleOrderShipDetailModel.PackedQty, false);
                            }

                            saleOrderShipDetailModel.ToEntity();
                            if (saleOrderShipDetailModel.IsNew)
                                saleOrderShipModel.base_SaleOrderShip.base_SaleOrderShipDetail.Add(saleOrderShipDetailModel.base_SaleOrderShipDetail);
                        }

                        //Calulate Profit For product Package is shipped & is a new shipped
                        if (!saleOrderShipModel.base_SaleOrderShip.IsShipped && saleOrderShipModel.IsShipped)
                        {
                            /// Calulate Profit For product
                            var gShip = saleOrderShipModel.SaleOrderShipDetailCollection.GroupBy(x => x.ProductResource);
                            foreach (var item in gShip)//Foreach collection group with Product
                            {
                                if (item.Any(x => x.SaleOrderDetailModel.ProductModel.IsCoupon))//Not Calculate OnHand with Coupon
                                    continue;
                                decimal totalQuantityBaseUom = 0;

                                if (item.Any(x => x.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group)))
                                {
                                    foreach (var saleOrderShipDetail in item)
                                    {
                                        //Get Product In Group
                                        foreach (base_ProductGroup productGroup in saleOrderShipDetail.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1)
                                        {
                                            base_ProductUOM productGroupUOM = _saleOrderRepository.GetProductUomOfProductInGroup(saleOrderModel.StoreCode, productGroup);
                                            decimal quantityBaseUnit = productGroupUOM.BaseUnitNumber;
                                            totalQuantityBaseUom += quantityBaseUnit * saleOrderShipDetail.PackedQty;
                                            decimal total = 0;
                                            //Get SaleOrderDetail to know is item change price ?
                                            base_SaleOrderDetailModel saleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetail.SaleOrderDetailResource));
                                            if (saleOrderDetailModel != null)
                                            {
                                                int decimalPlace = Define.CONFIGURATION.DecimalPlaces ?? 0;
                                                decimal totalPriceProductGroup = saleOrderShipDetail.SaleOrderDetailModel.ProductModel.base_Product.base_ProductGroup1.Sum(x => x.Amount);
                                                decimal unitPrice = productGroup.RegularPrice + (productGroup.RegularPrice * (saleOrderDetailModel.SalePrice - totalPriceProductGroup) / totalPriceProductGroup);
                                                total = Math.Round(productGroup.Quantity * unitPrice, 2);
                                                _productRepository.UpdateProductStore(productGroup.ProductResource, saleOrderModel.StoreCode, totalQuantityBaseUom, total, 0, 0, true);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var saleOrderShipDetail in item)
                                    {
                                        decimal quantityBaseUnit = saleOrderShipDetail.SaleOrderDetailModel.ProductUOMCollection.Single(x => x.UOMId.Equals(saleOrderShipDetail.SaleOrderDetailModel.UOMId)).BaseUnitNumber;
                                        totalQuantityBaseUom += quantityBaseUnit * saleOrderShipDetail.PackedQty;
                                    }
                                    _productRepository.UpdateProductStore(item.Key, saleOrderModel.StoreCode, totalQuantityBaseUom, item.Sum(x => x.SaleOrderDetailModel.SalePrice * x.PackedQty), 0, 0, true);
                                }
                            }
                        }

                    }
                    //Map value Of Model To Entity
                    saleOrderShipModel.ToEntity();
                    if (saleOrderShipModel.IsNew)
                        saleOrderModel.base_SaleOrder.base_SaleOrderShip.Add(saleOrderShipModel.base_SaleOrderShip);

                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="returnDetailModel"></param>
        private void CommissionReturn(base_SaleOrderModel saleOrderModel, base_ResourceReturnDetailModel returnDetailModel)
        {
            try
            {
                if (saleOrderModel.CommissionCollection == null)
                    saleOrderModel.CommissionCollection = new CollectionBase<base_SaleCommissionModel>();

                //get SaleRep of this Customer
                Guid customerGuid = Guid.Parse(saleOrderModel.CustomerResource);
                //Get Customer with CustomerResource
                base_GuestModel customerModel = CustomerCollection.SingleOrDefault(x => x.Resource == customerGuid);
                if (customerModel == null || !customerModel.SaleRepId.HasValue)
                    return;

                base_Guest employee = _guestRepository.Get(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived && x.Id == customerModel.SaleRepId);
                if (employee == null)
                    return;

                base_GuestModel employeeModel = new base_GuestModel(employee);
                string employeeResource = employeeModel.Resource.ToString();
                IQueryable<base_SaleCommission> saleCommissions = _saleCommissionRepository.GetIQueryable(x => x.GuestResource.Equals(employeeResource) && x.SaleOrderDetailResource.Equals(returnDetailModel.OrderDetailResource) && x.ProductResource.Equals(returnDetailModel.ProductResource));
                if (saleCommissions.Any())
                {
                    base_SaleCommissionModel employeeCommission = new base_SaleCommissionModel();
                    employeeCommission.Remark = MarkType.SaleOrderReturn.ToDescription();
                    employeeCommission.GuestResource = employeeModel.Resource.ToString();
                    employeeCommission.Sign = "-";
                    employeeCommission.Mark = "E";
                    employeeCommission.SOResource = saleOrderModel.Resource.ToString();
                    employeeCommission.SONumber = saleOrderModel.SONumber;
                    employeeCommission.SOTotal = saleOrderModel.RewardAmount;
                    employeeCommission.SODate = saleOrderModel.OrderDate;
                    employeeCommission.SaleOrderDetailResource = returnDetailModel.OrderDetailResource;
                    employeeCommission.ProductResource = returnDetailModel.ProductResource;
                    employeeCommission.Attribute = returnDetailModel.SaleOrderDetailModel.ProductModel.Attribute;
                    employeeCommission.Size = returnDetailModel.SaleOrderDetailModel.ProductModel.Size;
                    employeeCommission.Quanity = returnDetailModel.ReturnQty;
                    employeeCommission.RegularPrice = returnDetailModel.SaleOrderDetailModel.RegularPrice;
                    employeeCommission.Price = returnDetailModel.SaleOrderDetailModel.SalePrice;
                    employeeCommission.Amount = returnDetailModel.Amount;
                    employeeCommission.ComissionPercent = employeeModel.CommissionPercent;


                    if (returnDetailModel.SaleOrderDetailModel.ProductModel.CommissionUnit == 1) //$
                        employeeCommission.CommissionAmount = returnDetailModel.SaleOrderDetailModel.ProductModel.ComissionPercent;
                    else
                    {
                        decimal comissionOfProduct = (returnDetailModel.SaleOrderDetailModel.ProductModel.ComissionPercent * employeeCommission.Amount.Value) / 100;
                        employeeCommission.CommissionAmount = (comissionOfProduct * employeeCommission.ComissionPercent) / 100;
                    }

                    saleOrderModel.CommissionCollection.Add(employeeCommission);

                    ///when get manager not get with Employee.ManagerResource, because manager may by change to another one, that manager is not received after
                    //Get Manager get commission from this SaleOrder to subtract product return
                    string saleOrderResource = saleOrderModel.Resource.ToString();
                    //Manger(mark=M) get commssion (Sign : '+') of product (ProductResource) from SaleOrderDetail (SaleOrderDetailResource) of SaleOrder (saleOrderResource)
                    base_SaleCommission mangerCommission = _saleCommissionRepository.Get(x => x.Sign.Equals("+") && x.Mark.Equals("M") && x.SOResource.Equals(saleOrderResource) && x.SaleOrderDetailResource.Equals(returnDetailModel.OrderDetailResource) && x.ProductResource.Equals(returnDetailModel.ProductResource));
                    if (mangerCommission != null)//manger get Commission
                    {
                        Guid managerResourceGuid = Guid.Parse(mangerCommission.GuestResource);
                        base_Guest manager = _guestRepository.Get(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived && x.Resource == managerResourceGuid);
                        if (manager != null)
                        {
                            base_GuestModel managerModel = new base_GuestModel(manager);
                            base_SaleCommissionModel managerCommssionReturn = new base_SaleCommissionModel();
                            managerCommssionReturn.Remark = MarkType.SaleOrderReturn.ToDescription();
                            managerCommssionReturn.GuestResource = managerModel.Resource.ToString();
                            managerCommssionReturn.Sign = "-";
                            managerCommssionReturn.Mark = "M";
                            managerCommssionReturn.SOResource = saleOrderModel.Resource.ToString();
                            managerCommssionReturn.SONumber = saleOrderModel.SONumber;
                            managerCommssionReturn.SOTotal = saleOrderModel.RewardAmount;
                            managerCommssionReturn.SODate = saleOrderModel.OrderDate;
                            managerCommssionReturn.SaleOrderDetailResource = returnDetailModel.OrderDetailResource;
                            managerCommssionReturn.ProductResource = returnDetailModel.ProductResource;
                            managerCommssionReturn.Attribute = returnDetailModel.SaleOrderDetailModel.ProductModel.Attribute;
                            managerCommssionReturn.Size = returnDetailModel.SaleOrderDetailModel.ProductModel.Size;
                            managerCommssionReturn.Quanity = returnDetailModel.ReturnQty;
                            managerCommssionReturn.RegularPrice = returnDetailModel.SaleOrderDetailModel.RegularPrice;
                            managerCommssionReturn.Price = returnDetailModel.SaleOrderDetailModel.SalePrice;
                            managerCommssionReturn.Amount = returnDetailModel.Amount;
                            managerCommssionReturn.ComissionPercent = employeeModel.CommissionPercent;

                            managerCommssionReturn.CommissionAmount = (employeeCommission.CommissionAmount * managerCommssionReturn.ComissionPercent) / 100;

                            saleOrderModel.CommissionCollection.Add(managerCommssionReturn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Update Customer when PaymentTerm changed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void UpdateCustomer(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.GuestModel.IsDirty)
                {
                    //Update Term
                    saleOrderModel.GuestModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                    saleOrderModel.GuestModel.PaymentTermDescription = saleOrderModel.PaymentTermDescription;
                    saleOrderModel.GuestModel.TermDiscount = saleOrderModel.TermDiscountPercent;
                    saleOrderModel.GuestModel.TermNetDue = saleOrderModel.TermNetDue;
                    saleOrderModel.GuestModel.TermPaidWithinDay = saleOrderModel.TermPaidWithinDay;

                    //Update Customer Reward 
                    saleOrderModel.GuestModel.ToEntity();
                }

                //Onlyt reward Member
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    //Update Membership
                    if (saleOrderModel.GuestModel.MembershipValidated != null && saleOrderModel.GuestModel.MembershipValidated.IsDirty)
                    {
                        saleOrderModel.GuestModel.MembershipValidated.GuestId = saleOrderModel.GuestModel.Id;
                        saleOrderModel.GuestModel.MembershipValidated.ToEntity();
                        if (saleOrderModel.GuestModel.MembershipValidated.IsNew)
                            saleOrderModel.GuestModel.base_Guest.base_MemberShip.Add(saleOrderModel.GuestModel.MembershipValidated.base_MemberShip);
                    }

                    if (saleOrderModel.GuestModel.GuestRewardCollection != null)
                    {
                        //Update Guest Reward Create New
                        if (saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                        {
                            //Add New Guest Reward Created
                            foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems)
                            {
                                guestRewardModel.ToEntity();
                                if (guestRewardModel.IsNew)
                                    saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                            }

                        }

                        //Update Reward Redeem
                        foreach (base_GuestRewardModel guestRewardModel in saleOrderModel.GuestModel.GuestRewardCollection)
                        {
                            //Skip item is a temporary (reward is a sum of cash rewards)
                            if (guestRewardModel.GuestRewardDetailCollection != null)
                            {
                                foreach (base_GuestRewardDetailModel guestRewardDetailModel in guestRewardModel.GuestRewardDetailCollection)
                                {
                                    guestRewardDetailModel.DateApplied = DateTime.Now;
                                    guestRewardDetailModel.UserApplied = Define.USER != null ? Define.USER.LoginName : string.Empty;
                                    guestRewardDetailModel.ToEntity();
                                    if (guestRewardDetailModel.IsNew)
                                        guestRewardModel.base_GuestReward.base_GuestRewardDetail.Add(guestRewardDetailModel.base_GuestRewardDetail);

                                }
                            }

                            guestRewardModel.ToEntity();

                            if (guestRewardModel.IsNew)
                                saleOrderModel.GuestModel.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);

                        }

                    }
                }

                _guestRepository.Commit();

                if (saleOrderModel.GuestRewardSaleOrderModel != null && !string.IsNullOrWhiteSpace(saleOrderModel.GuestRewardSaleOrderModel.SaleOrderResource))
                {
                    //Release reward in store
                    if ((!string.IsNullOrWhiteSpace(saleOrderModel.GuestRewardSaleOrderModel.RewardRef) || saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId > 0)
                        && saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any() && saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                    {
                        base_GuestRewardModel guestRewardReleased = saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.SingleOrDefault(x => x.Id.Equals(saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId) || x.GetHashCode().ToString().Equals(saleOrderModel.GuestRewardSaleOrderModel.RewardRef));
                        saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId = guestRewardReleased.base_GuestReward.Id;
                    }

                    saleOrderModel.GuestRewardSaleOrderModel.ToEntity();
                    //Add GuestRewardSaleOrder
                    if (saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                    {
                        _guestRewardSaleOrderRepository.Add(saleOrderModel.GuestRewardSaleOrderModel.base_GuestRewardSaleOrder);
                    }

                    //Commit GuestRewardSaleOrderModel
                    _guestRewardSaleOrderRepository.Commit();
                }



                //Set Id For Reward]
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    if (saleOrderModel.GuestModel.GuestRewardCollection != null)
                        saleOrderModel.GuestModel.GuestRewardCollection.Clear();

                    if (saleOrderModel.GuestModel.GuestRewardCollection != null && saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                        saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Clear();
                    saleOrderModel.GuestModel.EndUpdate();

                    if (saleOrderModel.GuestModel.MembershipValidated != null && saleOrderModel.GuestModel.MembershipValidated.IsDirty)
                    {
                        saleOrderModel.GuestModel.MembershipValidated.ToModel();
                        saleOrderModel.GuestModel.MembershipValidated.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region SaleOrderDetailInput
        /// <summary>
        /// Selected Customer of autocomplete changed
        /// <param name="setRelation">set value using for Change customer</param>
        /// </summary>
        protected virtual void SelectedCustomerChanged()
        {
            //Don't set SaleOrder relation
            if (SelectedCustomer == null)
                return;

            SelectedSaleOrder.CustomerResource = SelectedCustomer.Resource.ToString();
            SelectedSaleOrder.GuestModel = CustomerCollection.Where(x => x.Resource.Equals(SelectedCustomer.Resource)).FirstOrDefault();

            //Get Reward & set PurchaseThreshold if Customer any reward
            var reward = GetReward(SelectedSaleOrder.OrderDate.Value.Date);

            SelectedSaleOrder.IsApplyReward = true;
            SelectedSaleOrder.IsApplyReward &= reward != null ? true : false;
            //isReward Member
            if (SelectedSaleOrder.GuestModel.IsRewardMember)
            {
                //Get GuestReward collection
                SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();

                if (SelectedSaleOrder.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                    SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

                //Get member ship
                SetMemberShipValidated(SelectedSaleOrder);
            }
            else
            {
                if (reward != null && reward.IsPromptEnroll && !SelectedSaleOrder.GuestModel.IsRewardMember)
                {
                    //"This customer is not currently a member of reward program. Do you want to enroll this one?"
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_EnrollRewardMember"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        SelectedSaleOrder.GuestModel.IsRewardMember = true;
                        SelectedCustomer.IsRewardMember = true;
                        SelectedSaleOrder.GuestModel.MembershipValidated = NewMemberShipModel(SelectedSaleOrder.GuestModel);
                        SelectedSaleOrder.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                        if (SelectedSaleOrder.GuestModel.RequirePurchaseNextReward == 0 && reward != null)
                            SelectedSaleOrder.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;
                    }
                }
            }
            //Set To Show Reward Icon
            SelectedSaleOrder.IsApplyReward &= SelectedSaleOrder.GuestModel.IsRewardMember;


            //PaymentTerm
            SelectedSaleOrder.TermDiscountPercent = SelectedCustomer.TermDiscount;
            SelectedSaleOrder.TermNetDue = SelectedCustomer.TermNetDue;
            SelectedSaleOrder.TermPaidWithinDay = SelectedCustomer.TermPaidWithinDay;
            SelectedSaleOrder.PaymentTermDescription = SelectedCustomer.PaymentTermDescription;
            if (SelectedCustomer.SaleRepId.HasValue)
            {
                base_Guest sale = _guestRepository.Get(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived && x.Id == SelectedCustomer.SaleRepId);
                if (sale != null)
                {
                    base_GuestModel saleRepModel = new base_GuestModel(sale);
                    SelectedSaleOrder.SaleRep = saleRepModel.LegalName;
                }
            }

            _saleOrderRepository.SetBillShipAddress(SelectedCustomer, SelectedSaleOrder);

            //SetTaxLocation

            SetSaleTaxLocationForSaleOrder(SelectedSaleOrder);

            //set Customer option in additional of markdownprice Level
            _saleOrderRepository.SetGuestAdditionalModel(SelectedSaleOrder);

            if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.MarkdownPriceLevel))
                SelectedSaleOrder.PriceSchemaId = SelectedSaleOrder.GuestModel.AdditionalModel.PriceSchemeId.Value;


            if (SelectedSaleOrder.SaleOrderDetailCollection.Any())
            {
                //Confirm if existed customer option discount  PriceLevelType.MarkdownPriceLevel 
                CalcDiscountAllItemWithCustomerAdditional();
            }
        }

        /// <summary>
        /// Search product by barcode
        /// </summary>
        private void SearchProductByBarcode()
        {
            short productGroupType = (short)ItemTypes.Group;
            base_Product product;
            Expression<Func<base_Product, bool>> productCondition = PredicateBuilder.True<base_Product>();

            //Condition : Product is not remove (Ispurge) & if is a product group, need has product child
            productCondition = productCondition.And(x => !x.IsPurge.Value && (!x.ItemTypeId.Equals(productGroupType) || (x.ItemTypeId.Equals(productGroupType) && x.base_ProductGroup1.Any())));

            if (Define.StoreCode == 0)
            {
                productCondition = productCondition.And(x => x.Barcode != null && x.Barcode.Equals(BarcodeProduct));
                product = _productRepository.Get(productCondition);
            }
            else
            {
                //get product base on store
                productCondition = productCondition.And(x => x.base_ProductStore.Any(y => y.StoreCode.Equals(Define.StoreCode)));
                //Get Product by barcode
                productCondition = productCondition.And(x => x.Barcode != null && x.Barcode.Equals(BarcodeProduct));

                //Get Product
                product = _productRepository.Get(productCondition);
            }

            if (product != null)
                SelectedProduct = new base_ProductModel(product);

            BarcodeProduct = string.Empty;
        }

        /// <summary>
        /// Selected Product Changed 
        /// <para>Input product into saleOrderDetail</para>
        /// </summary>
        private void SelectedProductChanged()
        {
            if (SelectedProduct != null)
            {
                try
                {
                    ProductInputProcess(SelectedProduct);
                }
                catch
                {
                    _selectedProduct = null;
                }
            }
            _selectedProduct = null;
        }



        /// <summary>
        /// add new sale Order Detail & Get UOM
        /// </summary>
        /// <returns></returns>
        protected base_SaleOrderDetailModel AddNewSaleOrderDetail(base_ProductModel productModel, string ParentResource = "", base_ProductModel productParentItem = null)
        {
            try
            {
                base_SaleOrderDetailModel salesOrderDetailModel = new base_SaleOrderDetailModel();
                salesOrderDetailModel.Resource = Guid.NewGuid();
                base_ProductGroup productGroupItem = null;
                //Get Product GroupItem
                if (productParentItem != null)
                {
                    productGroupItem = productModel.base_Product.base_ProductGroup.SingleOrDefault(x => x.ProductParentId.Equals(productParentItem.Id));
                    salesOrderDetailModel.ProductGroupItem = productGroupItem;
                    salesOrderDetailModel.ProductParentId = productParentItem.Id;
                }

                salesOrderDetailModel.Quantity = productGroupItem == null ? 1 : productGroupItem.Quantity;//Set Quantity Default of ProductInGroup
                salesOrderDetailModel.PromotionId = 0;
                salesOrderDetailModel.SerialTracking = string.Empty;
                salesOrderDetailModel.TaxCode = productModel.TaxCode;
                salesOrderDetailModel.ItemCode = productModel.Code;
                salesOrderDetailModel.ItemName = productModel.ProductName;
                salesOrderDetailModel.ProductResource = productModel.Resource.ToString();
                salesOrderDetailModel.OnHandQty = _saleOrderRepository.GetUpdateProduct(SelectedSaleOrder.StoreCode, productModel);
                salesOrderDetailModel.ItemAtribute = productModel.Attribute;
                salesOrderDetailModel.ItemSize = productModel.Size;
                salesOrderDetailModel.ProductModel = productModel;
                salesOrderDetailModel.ParentResource = ParentResource;

                //Set Item type Sale Order to know item is group/child or none
                if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                    salesOrderDetailModel.ItemType = 1;
                else if (!string.IsNullOrWhiteSpace(salesOrderDetailModel.ParentResource))//Child item of group
                    salesOrderDetailModel.ItemType = 2;
                else
                    salesOrderDetailModel.ItemType = 0;

                salesOrderDetailModel.CalcSubTotal();
                SelectedSaleOrder.SaleOrderDetailCollection.Add(salesOrderDetailModel);
                return salesOrderDetailModel;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Create SaleOrder Detail by ProductModel
        /// <para>ShowSerialPopup :  false => not show popup serial , using for multi product Selection</para>
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="showSerialPopup"></param>
        private void ProductInputProcess(base_ProductModel productModel, bool showSerialPopup = true)
        {

            try
            {

                if (!productModel.IsCoupon)
                {
                    //Get VendorName
                    base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == productModel.VendorId));
                    if (vendorModel != null)
                        productModel.VendorName = vendorModel.LegalName;
                }

                string lastSaleOrderDetailResource = string.Empty;
                //Product Group Process
                if (productModel.ItemTypeId.Equals((short)ItemTypes.Group))//Process for ProductGroup
                {
                    //Check Has Children in product Group
                    if (productModel.base_Product.base_ProductGroup1.Any())
                    {
                        //Product Group :Create new SaleOrderDetail , ProductUOM & add to collection 
                        //this.BreakSODetailChange = true;
                        string parentResource = SaleOrderDetailProcess(productModel, showSerialPopup, string.Empty);
                        lastSaleOrderDetailResource = parentResource;
                        //this.BreakSODetailChange = false;

                        //ProductGroup Child
                        foreach (base_ProductGroup productGroup in productModel.base_Product.base_ProductGroup1)
                        {
                            long productId = productGroup.base_Product.Id;
                            base_Product product = _productRepository.Get(x => x.Id.Equals(productId));
                            if (product != null)
                            {
                                //Get Product From ProductCollection
                                base_ProductModel productInGroupModel = new base_ProductModel(product);
                                //Create new SaleOrderDetail , ProductUOM & add to collection
                                //this.BreakSODetailChange = true;
                                SaleOrderDetailProcess(productInGroupModel, showSerialPopup, parentResource, productModel);
                                //this.BreakSODetailChange = false;
                            }
                            else
                            {
                                _log4net.Info("Product Id =" + productId + " is not exited");
                            }
                        }
                    }
                    else
                    {
                        _log4net.Info("Product Group but not child");
                    }
                }
                else
                {
                    //Create new SaleOrderDetail , ProductUOM & add to collection
                    //this.BreakSODetailChange = true;
                    lastSaleOrderDetailResource = SaleOrderDetailProcess(productModel, showSerialPopup);
                    //this.BreakSODetailChange = false;
                }

                if (!string.IsNullOrWhiteSpace(lastSaleOrderDetailResource))
                {
                    SelectedSaleOrderDetail = SelectedSaleOrder.SaleOrderDetailCollection.FirstOrDefault(x => x.Resource.ToString().Equals(lastSaleOrderDetailResource));
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handle single new sale Order detail
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="showSerialPopup"></param>
        /// <param name="productSerialParent"></param>
        protected string SaleOrderDetailProcess(base_ProductModel productModel, bool showSerialPopup, string ParentResource = "", base_ProductModel productParentModel = null)
        {
            base_SaleOrderDetailModel salesOrderDetailModel;
            try
            {
                salesOrderDetailModel = AddNewSaleOrderDetail(productModel, ParentResource, productParentModel);

                if (salesOrderDetailModel.ProductModel.IsCoupon)
                {
                    salesOrderDetailModel.IsQuantityAccepted = true;
                    salesOrderDetailModel.IsReadOnlyUOM = true;
                    OpenCouponView(salesOrderDetailModel);
                }
                else
                {
                    //Get Product UOMCollection
                    _saleOrderRepository.GetProductUOMforSaleOrderDetail(salesOrderDetailModel);

                    SetPriceUOM(salesOrderDetailModel);

                    //Update price when regular price =0
                    if (UpdateProductPrice(salesOrderDetailModel.ProductModel))
                    {
                        salesOrderDetailModel.RegularPrice = salesOrderDetailModel.ProductModel.RegularPrice;
                        salesOrderDetailModel.SalePrice = salesOrderDetailModel.ProductModel.RegularPrice;

                        //Update Product Unit Collection (useful for setPriceUom when change again PriceSchema)
                        base_ProductUOMModel baseUnitModel = salesOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId.Equals(salesOrderDetailModel.ProductModel.BaseUOMId));
                        if (baseUnitModel != null)
                        {
                            baseUnitModel.RegularPrice = salesOrderDetailModel.ProductModel.RegularPrice;
                        }
                    }

                    //SetUnit Price for productInGroup
                    if (!string.IsNullOrWhiteSpace(ParentResource))
                    {
                        int decimalPlace = Define.CONFIGURATION.DecimalPlaces.HasValue ? Define.CONFIGURATION.DecimalPlaces.Value : 0;
                        decimal totalPriceProductGroup = productParentModel.base_Product.base_ProductGroup1.Sum(x => x.Amount);
                        base_ProductGroup productGroup = productParentModel.base_Product.base_ProductGroup1.SingleOrDefault(x => x.ProductId.Equals(productModel.Id));
                        base_SaleOrderDetailModel salesOrderDetailParentModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(ParentResource));
                        if (productGroup != null && salesOrderDetailParentModel != null)
                        {
                            decimal unitPrice = productModel.RegularPrice + (productModel.RegularPrice * (salesOrderDetailParentModel.SalePrice - totalPriceProductGroup) / totalPriceProductGroup);
                            salesOrderDetailModel.SalePrice = unitPrice;
                        }
                    }

                    //Calculate Discount for product
                    CalculateDiscount(salesOrderDetailModel);

                    //Check Show Detail
                    _saleOrderRepository.CheckToShowDatagridRowDetail(salesOrderDetailModel);

                    salesOrderDetailModel.CalcDueQty();
                    salesOrderDetailModel.CalUnfill();

                    //Check on hand quatity
                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, salesOrderDetailModel);

                    if (productModel.IsSerialTracking && showSerialPopup)
                        OpenTrackingSerialNumber(salesOrderDetailModel, true);
                }

                if (salesOrderDetailModel != null)
                {
                    CalculateAllTax(SelectedSaleOrder);

                    salesOrderDetailModel.CalcSubTotal();
                    salesOrderDetailModel.CalcDueQty();
                    salesOrderDetailModel.CalUnfill();
                    SelectedSaleOrder.CalcSubTotal();
                    SelectedSaleOrder.CalcBalance();
                    return salesOrderDetailModel.Resource.ToString();
                }
            }
            catch (Exception ex)
            {

                _log4net.Error(ex);
                throw ex;
            }
            return string.Empty;
        }

        #endregion

        #region Calculate Tax

        /// <summary>
        /// Calculate Price Dependent Tax
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="subTotal"></param>
        /// <param name="taxPercent"></param>
        /// <param name="taxAmount"></param>
        private decimal CalcPriceDependentTax(base_SaleOrderModel saleOrderModel)
        {
            decimal taxAmount = 0;
            if (saleOrderModel.IsTaxExemption == true)
            {
                taxAmount = 0;
            }
            else if (saleOrderModel.TaxLocationModel != null && Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price))
            {
                base_SaleTaxLocationOptionModel saleTaxLocationOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    taxAmount += _saleOrderRepository.CalcPriceDependentItem(saleOrderDetailModel.SubTotal, saleOrderDetailModel.SalePrice, saleTaxLocationOptionModel);
                }
            }
            else
            {
                taxAmount = 0;
            }
            return taxAmount;
        }

        /// <summary>
        /// Calculate Tax for each other saleOrderDetail with itemprice (regular price)
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private decimal CalcMultiTaxForProduct(base_SaleOrderModel saleOrderModel)
        {
            decimal taxAmount = 0;
            if (saleOrderModel.IsTaxExemption == true)
            {
                taxAmount = 0;
            }
            else
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    //if (!saleOrderDetailModel.ProductModel.IsCoupon)//18/06/2013: not calculate tax for coupon
                    //8/11/2013 Calculate tax for Coupon
                    taxAmount += _saleOrderRepository.CalcMultiTaxForItem(saleOrderModel.TaxLocationModel.SaleTaxLocationOptionCollection, saleOrderDetailModel.SubTotal, saleOrderDetailModel.SalePrice);
                }
            }
            return taxAmount;
        }

        /// <summary>
        /// Calculate multi, price dependent tax when sale price changed
        /// </summary>
        private void CalculateMultiNPriceTax()
        {
            if (SelectedSaleOrder.TaxLocationModel != null && SelectedSaleOrder.TaxLocationModel.TaxCodeModel != null)
            {
                if (Convert.ToInt32(SelectedSaleOrder.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Single))
                    return;

                if (Convert.ToInt32(SelectedSaleOrder.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Multi))
                {
                    SelectedSaleOrder.ProductTaxAmount = CalcMultiTaxForProduct(SelectedSaleOrder);
                }
                else if (Convert.ToInt32(SelectedSaleOrder.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Price))
                {
                    SelectedSaleOrder.ProductTaxAmount = CalcPriceDependentTax(SelectedSaleOrder);
                }

                SelectedSaleOrder.TaxAmount = SelectedSaleOrder.ProductTaxAmount + SelectedSaleOrder.ShipTaxAmount;
                SelectedSaleOrder.TaxPercent = 0;
            }
        }

        /// <summary>
        /// Calc Ship Tax with PriceDepent
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private decimal CalcShipTaxAmount(CPC.POS.Model.base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null
                      && Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is(SalesTaxOption.Price)
                       && saleOrderModel.TaxLocationModel.IsShipingTaxable)
                {
                    CPC.POS.Model.base_SaleTaxLocationModel shippingTaxCode = SaleTaxLocationCollection.SingleOrDefault(x => x.Id.Equals(saleOrderModel.TaxLocationModel.ShippingTaxCodeId));
                    if (shippingTaxCode != null
                        && Convert.ToInt32(shippingTaxCode.TaxOption).Is(SalesTaxOption.Price)
                        && shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault() != null
                        && shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault().IsApplyAmountOver)
                    {
                        CPC.POS.Model.base_SaleTaxLocationOptionModel taxOptionModel = new CPC.POS.Model.base_SaleTaxLocationOptionModel(shippingTaxCode.base_SaleTaxLocation.base_SaleTaxLocationOption.FirstOrDefault());
                        return _saleOrderRepository.CalcPriceDependentItem(saleOrderModel.Shipping, saleOrderModel.Shipping, taxOptionModel);
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Apply Tax
        /// </summary>
        private void CalculateAllTax(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null)
            {
                if (saleOrderModel.SaleOrderDetailCollection.Count(x => x.ProductModel.IsCoupon) == saleOrderModel.SaleOrderDetailCollection.Count())
                {
                    saleOrderModel.ProductTaxAmount = 0;
                    saleOrderModel.TaxPercent = 0;
                }
                else
                {
                    if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Multi))
                    {
                        saleOrderModel.TaxPercent = 0;
                        saleOrderModel.ProductTaxAmount = CalcMultiTaxForProduct(saleOrderModel);
                    }
                    else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Price))
                    {
                        saleOrderModel.TaxPercent = 0;
                        saleOrderModel.ProductTaxAmount = CalcPriceDependentTax(saleOrderModel);
                        saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
                    }
                    else
                    {
                        decimal taxAmount = 0;
                        decimal taxPercent = 0;
                        _saleOrderRepository.CalcSingleTax(saleOrderModel, saleOrderModel.SubTotal, out taxPercent, out taxAmount);
                        saleOrderModel.ProductTaxAmount = taxAmount;
                        saleOrderModel.TaxPercent = taxPercent;
                    }
                }

            }
        }

        /// <summary>
        /// Load Tax for SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void GetSaleTax(base_SaleOrderModel saleOrderModel)
        {
            saleOrderModel.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == saleOrderModel.TaxLocation);
            if (saleOrderModel.TaxLocationModel != null)
            {
                //Get Tax Code
                saleOrderModel.TaxLocationModel.TaxCodeModel = SaleTaxLocationCollection.SingleOrDefault(x => x.ParentId == saleOrderModel.TaxLocationModel.Id && x.TaxCode.Equals(saleOrderModel.TaxCode));
                if (saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                    saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(saleOrderModel.TaxLocationModel.TaxCodeModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)));
            }
        }
        #endregion

        #region Calculate Discounts
        /// <summary>
        /// Discount with Customer option in Additional. & set price if customer has additional MarkdownPriceLevel
        /// <para>when PriceSchemeId = PriceLevelType.FixedDiscountOnAllItems</para>
        /// <para>using for user change customer</para>
        /// </summary>
        private void CalcDiscountAllItemWithCustomerAdditional()
        {
            try
            {
                //Not calculate discount for layaway
                if (!SelectedSaleOrder.Mark.Equals(CPC.POS.MarkType.Layaway) && SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any())
                {
                    if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.FixedDiscountOnAllItems))
                    {
                        //"Do you want to apply customer discount?"
                        MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ApplyCustomerDiscount"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                        if (result.Equals(MessageBoxResult.Yes))
                        {
                            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                            {
                                CalcDiscountWithAdditional(saleOrderDetailModel);
                            }
                        }
                    }
                    else if (SelectedSaleOrder.GuestModel.AdditionalModel != null && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((short)PriceLevelType.MarkdownPriceLevel))
                    {
                        PriceSchemaChanged();
                    }
                    else //Calculate Promotion of POS if existed
                    {
                        CalcDiscountAllItemWithPOSProgram();
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Calculate All item with pos program discount
        /// </summary>
        private void CalcDiscountAllItemWithPOSProgram()
        {
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
            {
                CalcProductDiscountWithProgram(SelectedSaleOrder, saleOrderDetailModel);
            }
        }

        /// <summary>
        /// Calculate discount with GuestAddtion or program
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        protected void CalculateDiscount(base_SaleOrderDetailModel salesOrderDetailModel)
        {
            try
            {
                //Not calculate Discount for layaway
                if (SelectedSaleOrder.Mark.Equals(CPC.POS.MarkType.Layaway.ToDescription()))
                    return;
                if (salesOrderDetailModel.IsManual)
                    salesOrderDetailModel.CalcDicountByPercent();
                else if (SelectedSaleOrder.GuestModel != null &&
                   SelectedSaleOrder.GuestModel.AdditionalModel != null
                   && SelectedSaleOrder.GuestModel.AdditionalModel.PriceLevelType.Equals((int)PriceLevelType.FixedDiscountOnAllItems))
                    CalcDiscountWithAdditional(salesOrderDetailModel);
                else
                    CalcProductDiscountWithProgram(SelectedSaleOrder, salesOrderDetailModel);

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Calculate discount with Customer additional 
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        protected void CalcDiscountWithAdditional(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            try
            {
                if ((saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group) || !string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)) && saleOrderDetailModel.RegularPrice < saleOrderDetailModel.SalePrice)
                {
                    saleOrderDetailModel.PromotionId = 0;
                    saleOrderDetailModel.PromotionName = string.Empty;
                    return;
                }
                saleOrderDetailModel.IsManual = true;
                saleOrderDetailModel.PromotionId = 0;
                if (Convert.ToInt32(SelectedSaleOrder.GuestModel.AdditionalModel.Unit).Equals(1))//$
                {
                    if (saleOrderDetailModel.RegularPrice > SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value)
                    {
                        saleOrderDetailModel.UnitDiscount = SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value;
                        //So tien dc giam trên 1 đợn vi
                        saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.RegularPrice - saleOrderDetailModel.UnitDiscount;
                        saleOrderDetailModel.SalePrice = saleOrderDetailModel.DiscountAmount;
                        saleOrderDetailModel.DiscountPercent = Math.Round((saleOrderDetailModel.RegularPrice - saleOrderDetailModel.SalePrice) / saleOrderDetailModel.RegularPrice * 100, 2);
                        //Tổng số tiền dc giảm trên tổng số sản phẩm
                        saleOrderDetailModel.TotalDiscount = saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity;
                        _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                    }
                    else
                    {
                        _saleOrderRepository.ResetProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    }
                }
                else //Discount with percent
                {
                    //so tien giảm trên 1 đơn vi
                    saleOrderDetailModel.UnitDiscount = (saleOrderDetailModel.RegularPrice * SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value / 100);
                    saleOrderDetailModel.DiscountPercent = SelectedSaleOrder.GuestModel.AdditionalModel.FixDiscount.Value;
                    //So tien dc giam trên 1 đợn vi
                    saleOrderDetailModel.DiscountAmount = saleOrderDetailModel.RegularPrice - saleOrderDetailModel.UnitDiscount;
                    saleOrderDetailModel.SalePrice = saleOrderDetailModel.DiscountAmount;
                    //Tổng số tiền dc giảm trên tổng số sản phẩm
                    saleOrderDetailModel.TotalDiscount = saleOrderDetailModel.UnitDiscount * saleOrderDetailModel.Quantity;
                    _saleOrderRepository.HandleOnSaleOrderDetailModel(SelectedSaleOrder, saleOrderDetailModel);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Calculate discount for product
        /// auto choice promotion program for current SaleOrderDetailItem
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void CalcProductDiscountWithProgram(base_SaleOrderModel saleOrderModel, base_SaleOrderDetailModel saleOrderDetailModel, bool resetDiscPercent = false)
        {
            try
            {
                //reset Discount percent Or calculate new Price when change UOM
                if (resetDiscPercent)
                {
                    _saleOrderRepository.ResetProductDiscount(saleOrderModel, saleOrderDetailModel);
                }

                base_PromotionModel promotionModel = GetPromotionForSaleOrderDetail(saleOrderModel, saleOrderDetailModel);

                if (promotionModel != null && !saleOrderDetailModel.IsManual)
                {
                    saleOrderDetailModel.PromotionId = promotionModel.Id;
                    saleOrderDetailModel.PromotionName = promotionModel.Name;

                    //BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(saleOrderModel, saleOrderDetailModel);
                    //BreakSODetailChange = false;
                }
                else
                {
                    if (saleOrderDetailModel.IsManual)
                    {
                        //BreakSODetailChange = true;
                        _saleOrderRepository.CalcProductDiscount(saleOrderModel, saleOrderDetailModel);
                        //BreakSODetailChange = false;
                    }
                    else
                    {
                        saleOrderDetailModel.PromotionId = 0;
                        saleOrderDetailModel.PromotionName = string.Empty;

                        _saleOrderRepository.ResetProductDiscount(saleOrderModel, saleOrderDetailModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }

        }

        /// <summary>
        /// Get Promotion for sale order detail
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        /// <returns></returns>
        protected base_PromotionModel GetPromotionForSaleOrderDetail(base_SaleOrderModel saleOrderModel, base_SaleOrderDetailModel saleOrderDetailModel)
        {
            base_PromotionModel promotionModel;
            try
            {
                //Not calculate for Parent & child in product group or manual
                if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)
                    || saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group)
                    || saleOrderDetailModel.IsManual)
                {
                    return null;
                }

                int productCategoryId = saleOrderDetailModel.ProductModel.ProductCategoryId;
                //Check condition : product in saleOrder has in Promotion Category 
                //                  OrderDate in Start & Endate if existed 
                promotionModel = _promotionList.FirstOrDefault(x => x.CategoryId.HasValue && x.CategoryId.Value.Equals(productCategoryId)
                                    && Convert.ToInt32(saleOrderModel.PriceSchemaId).In(x.PriceSchemaRange.Value)
                                    && (!x.StartDate.HasValue || (x.StartDate.HasValue && x.StartDate.Value <= saleOrderModel.OrderDate))
                                    && (!x.EndDate.HasValue || (x.EndDate.HasValue && x.EndDate.Value >= saleOrderModel.OrderDate)));

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
            return promotionModel;
        }
        #endregion

        #region Reward
        /// <summary>
        /// Get Reward From Reward Collection
        /// </summary>
        /// <param name="orderDate">
        /// order Date !=null : which reward is Active & OrderDate>= Start date & OrderDate <= EndDate
        /// OrderDate =null : Get reward without conditional
        /// </param>
        /// <returns></returns>
        protected base_RewardManager GetReward(DateTime? orderDate = null)
        {
            base_RewardManager rewardMangers;
            try
            {
                if (orderDate.HasValue)
                {
                    rewardMangers = RewardManagerCollection.SingleOrDefault(x => x.Status.Is(StatusBasic.Active) &&
                                                   ((x.IsTrackingPeriod && ((x.IsNoEndDay && x.StartDate <= orderDate) || (!x.IsNoEndDay && x.StartDate <= orderDate && orderDate <= x.EndDate))
                                                  || !x.IsTrackingPeriod)));

                }
                else
                {
                    rewardMangers = RewardManagerCollection.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }

            return rewardMangers;
        }
        #endregion


        //Set Value
        #region SetValueTo
        /// <summary>
        /// Set Price to SaleOrderDetail with PriceLevelId(PriceSchemaId)
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        protected void SetPriceUOM(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            try
            {
                if (saleOrderDetailModel.ProductUOMCollection != null)
                {
                    base_ProductUOMModel productUOM = saleOrderDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == saleOrderDetailModel.UOMId);
                    base_ProductStore productStore = saleOrderDetailModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(SelectedSaleOrder.StoreCode));

                    if (productStore != null)
                    {
                        base_ProductUOM unitStore = productStore.base_ProductUOM.SingleOrDefault(x => x.UOMId.Equals(saleOrderDetailModel.UOMId));

                        //Update Quantity with base unit for ProductStore
                        if (unitStore == null)
                        {
                            if (saleOrderDetailModel.UOMId.Equals(saleOrderDetailModel.ProductModel.BaseUOMId))
                                saleOrderDetailModel.OnHandQty = productStore.QuantityOnHand;//Get Quantity Onhand from baseUnit
                            else
                                saleOrderDetailModel.OnHandQty = Convert.ToDecimal(productStore.QuantityOnHand) / Convert.ToDecimal(productUOM.BaseUnitNumber);
                        }
                        else
                        {
                            saleOrderDetailModel.OnHandQty = unitStore.QuantityOnHand;
                        }

                    }
                    else
                        saleOrderDetailModel.OnHandQty = 0;

                    if (productUOM != null)
                    {
                        saleOrderDetailModel.UnitName = productUOM.Name;
                        saleOrderDetailModel.UOM = productUOM.Name;

                        if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.RegularPrice))
                        {
                            //set Price with Price Level
                            if (productUOM.RegularPrice > 0)
                            {
                                saleOrderDetailModel.RegularPrice = productUOM.RegularPrice;
                                if (!saleOrderDetailModel.IsManual)
                                    saleOrderDetailModel.SalePrice = productUOM.RegularPrice;
                            }
                            else
                            {
                                //Get Price with UserUpdate when regularPrice =0 (UpdateProductPrice)
                                saleOrderDetailModel.SalePrice = saleOrderDetailModel.ProductModel.RegularPrice;
                            }
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.SalePrice))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price1;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price1;
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.WholesalePrice))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price2;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price2;
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.Employee))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price3;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price3;
                        }
                        else if (Convert.ToInt32(SelectedSaleOrder.PriceSchemaId).Is(PriceTypes.CustomPrice))
                        {
                            saleOrderDetailModel.RegularPrice = productUOM.Price4;
                            if (!saleOrderDetailModel.IsManual)
                                saleOrderDetailModel.SalePrice = productUOM.Price4;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Update price when PriceSchema Changed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected void PriceSchemaChanged()
        {
            try
            {
                if (SelectedSaleOrder.SaleOrderDetailCollection != null)
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                    {
                        SetPriceUOM(saleOrderDetailModel);

                        saleOrderDetailModel.CalcSubTotal();

                        saleOrderDetailModel.CalcDueQty();

                        saleOrderDetailModel.CalUnfill();

                        CalculateDiscount(saleOrderDetailModel);

                        _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    }
                    SelectedSaleOrder.CalcSubTotal();
                    SelectedSaleOrder.CalcDiscountAmount();
                    //Need Calculate Total after Subtotal & Discount Percent changed
                    SelectedSaleOrder.CalcTotal();
                    SelectedSaleOrder.CalcBalance();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Set MemberShip For SaleOrder Guest
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected virtual void SetMemberShipValidated(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.GuestModel != null && saleOrderModel.GuestModel.IsRewardMember)
            {
                //Get MemberShip
                short memebershipActivedStatus = (short)MemberShipStatus.Actived;
                base_MemberShip membership = saleOrderModel.GuestModel.base_Guest.base_MemberShip.FirstOrDefault(x => x.Status.Equals(memebershipActivedStatus));
                if (membership != null)
                    saleOrderModel.GuestModel.MembershipValidated = new base_MemberShipModel(membership);
                else
                    saleOrderModel.GuestModel.MembershipValidated = NewMemberShipModel(saleOrderModel.GuestModel);
            }
        }

        /// <summary>
        /// Set SaleTax with Booking Channel & Customer additional
        /// require when customer change & Booking Channel Change
        /// </summary>
        protected void SetSaleTaxLocationForSaleOrder(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                //V3: Not follow with booking channel Booking Channel
                if (Define.CONFIGURATION.IsAllowAntiExemptionTax)//Using Default Tax although these customer has set Tax
                {
                    saleOrderModel.IsTaxExemption = false;
                    saleOrderModel.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                    saleOrderModel.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                }
                else
                {
                    if (SelectedCustomer != null && SelectedCustomer.base_Guest.base_GuestAdditional.Any())
                    {
                        //Check Customer Additionnal has set SaleTax=> true : Using that SaleTax ,otherwise TaxExemption
                        base_GuestAdditional guestAdditional = SelectedCustomer.base_Guest.base_GuestAdditional.FirstOrDefault();
                        //This customer is tax excemption
                        if (guestAdditional.IsTaxExemption)
                        {
                            saleOrderModel.IsTaxExemption = true;
                            //Tax exemption code is a ResellerTaxId
                            saleOrderModel.TaxExemption = guestAdditional.ResellerTaxId;
                            saleOrderModel.TaxAmount = 0;
                            saleOrderModel.TaxPercent = 0;
                            saleOrderModel.TaxLocation = 0;
                            saleOrderModel.TaxCode = string.Empty;
                        }
                        else
                        {
                            saleOrderModel.IsTaxExemption = false;
                            saleOrderModel.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                            saleOrderModel.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                        }
                    }
                    else
                    {
                        saleOrderModel.IsTaxExemption = false;
                        saleOrderModel.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                        saleOrderModel.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                    }


                }

                if (saleOrderModel.TaxLocation > 0)
                {
                    GetSaleTax(saleOrderModel);
                }

                //Calculate SaleOrderDetail Tax when Tax Changed
                if (saleOrderModel.SaleOrderDetailCollection.Any())
                {
                    CalculateAllTax(SelectedSaleOrder);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion

        /// <summary>
        /// Method confirm & delete saleorder detail
        /// <para>Using for Delete by key & Menucontext</para>
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void DeleteItemSaleOrderDetail(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            //msg: Do you want to delete? 
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Is(MessageBoxResult.Yes))
            {
                //Item is product group => remove child item
                if (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                {
                    foreach (base_SaleOrderDetailModel soDetailInGroup in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.Resource.ToString())).ToList())
                    {
                        SelectedSaleOrder.SaleOrderDetailCollection.Remove(soDetailInGroup);
                    }

                }//Get ProductInGroup if current item deleted is only 1, remove parent item
                else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))
                {
                    int numTheSameItem = SelectedSaleOrder.SaleOrderDetailCollection.Count(x => x.ParentResource.Equals(saleOrderDetailModel.ParentResource) && !x.Resource.Equals(saleOrderDetailModel.Resource));
                    if (numTheSameItem == 0)
                    {
                        base_SaleOrderDetailModel parentDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetailModel.ParentResource));
                        if (parentDetailModel != null)
                        {
                            SelectedSaleOrder.SaleOrderDetailCollection.Remove(parentDetailModel);
                        }
                    }
                }

                SelectedSaleOrder.SaleOrderDetailCollection.Remove(saleOrderDetailModel);
                //Not calculate discount on product group or product in group
                if (string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource)
                    && saleOrderDetailModel.ProductModel != null
                    && !saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                {
                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    BreakSODetailChange = false;
                }
                SelectedSaleOrder.CalcSubTotal();
                SelectedSaleOrder.CalcBalance();
                if (SelectedSaleOrder.SaleOrderDetailCollection != null)
                    SelectedSaleOrder.IsHiddenErrorColumn = !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);

                _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
            }
        }

        //Popup
        /// <summary>
        /// Open coupon view to update Amount & Coupon Code
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        protected void OpenCouponView(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            try
            {
                //string titleView = saleOrderDetailModel.ProductModel.ProductCategoryId.Equals((int)PaymentMethod.GiftCard) ? Language.GetMsg("SO_Title_GiftCard") : Language.GetMsg("SO_Title_GiftCertification");
                //CouponViewModel couponViewModel = new CouponViewModel();
                //couponViewModel.SaleOrderDetailModel = saleOrderDetailModel;
                //couponViewModel.SaleOrderModel = SelectedSaleOrder;
                //bool? result = _dialogService.ShowDialog<CouponView>(_ownerViewModel, couponViewModel, titleView);
                //if (result == true)
                //{
                //    _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                //}
                //else
                //{
                //    if (saleOrderDetailModel.CouponCardModel == null)
                //        SelectedSaleOrder.SaleOrderDetailCollection.Remove(saleOrderDetailModel);
                //}
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Update price when product price is 0
        /// </summary>
        /// <param name="productModel"></param>
        protected bool UpdateProductPrice(base_ProductModel productModel)
        {
            bool resultValue = false;
            try
            {
                if (productModel.RegularPrice <= 0)
                {
                    //UpdateTransactionViewModel updateTransactionViewModel = new UpdateTransactionViewModel(productModel);
                    //bool? result = _dialogService.ShowDialog<UpdateTransactionView>(_ownerViewModel, updateTransactionViewModel, Language.GetMsg("SO_Title_UpdateProductPrice"));
                    //if (result == true)
                    //{
                    //    productModel.RegularPrice = updateTransactionViewModel.NewPrice;


                    //    //Update ProductColletion
                    //    if (updateTransactionViewModel.IsUpdateProductPrice)
                    //    {
                    //        base_Product product = _productRepository.GetProductByResource(updateTransactionViewModel.ProductModel.Resource.ToString());
                    //        if (product != null)
                    //        {
                    //            base_ProductModel productUpdate = new base_ProductModel(product);
                    //            productUpdate.RegularPrice = updateTransactionViewModel.ProductModel.RegularPrice;
                    //        }
                    //    }
                    //    resultValue = true;
                    //}
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return resultValue;
        }

        /// <summary>
        /// Open form tracking serial number
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        /// <param name="isShowQty"></param>
        protected void OpenTrackingSerialNumber(base_SaleOrderDetailModel salesOrderDetailModel, bool isShowQty = false, bool isEditing = true)
        {
            try
            {
                if (!salesOrderDetailModel.ProductModel.IsSerialTracking)
                    return;
                ////Show Tracking Serial
                //SelectTrackingNumberViewModel trackingNumberViewModel = new SelectTrackingNumberViewModel(salesOrderDetailModel, isShowQty, isEditing);
                //bool? result = _dialogService.ShowDialog<SelectTrackingNumberView>(_ownerViewModel, trackingNumberViewModel, Language.GetMsg("SO_Title_TrackingSerialNumber"));

                //if (result == true)
                //{
                //    if (isEditing)
                //    {
                //        salesOrderDetailModel = trackingNumberViewModel.SaleOrderDetailModel;

                //        CalculateDiscount(salesOrderDetailModel);
                //    }
                //}
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Util

        /// <summary>
        /// Moving Item in SaleOrderDetail
        /// </summary>
        /// <param name="navigator"></param>
        private void MovingItemExecute(string navigator)
        {
            if (SelectedSaleOrder != null && SelectedSaleOrder.SaleOrderDetailCollection != null && _saleOrderDetailCollectionView == null)
            {
                _saleOrderDetailCollectionView = CollectionViewSource.GetDefaultView(SelectedSaleOrder.SaleOrderDetailCollection);
            }


            if (!_saleOrderDetailCollectionView.IsEmpty)
            {
                int currentPosition = _saleOrderDetailCollectionView.CurrentPosition;
                if (navigator.Equals("MoveUp"))
                {
                    if (currentPosition > 0)
                        _saleOrderDetailCollectionView.MoveCurrentToPrevious();
                }
                else
                {
                    if (currentPosition < _saleOrderDetailCollectionView.OfType<object>().Count() - 1)
                        _saleOrderDetailCollectionView.MoveCurrentToNext();
                }
            }
        }

        /// <summary>
        /// Generate Barcode with EAN13 Format (13 digit numberic)
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        protected void IdCardBarcodeGen(base_MemberShipModel memberShipModel)
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                //GenMemberShipCard
                if (string.IsNullOrWhiteSpace(memberShipModel.IdCard))
                {
                    using (BarcodeLib.Barcode barCode = new BarcodeLib.Barcode())
                    {
                        barCode.IncludeLabel = true;
                        string idCard = currentDate.ToString("yyMMddHHmmss");
                        barCode.Encode(BarcodeLib.TYPE.EAN13, idCard, 200, 70);
                        memberShipModel.IdCardImg = barCode.Encoded_Image_Bytes;
                        memberShipModel.IdCard = barCode.RawData;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Lock Screen
        /// </summary>
        private void LockScreenExecuted()
        {
            // Get main window
            Window mainWindow = App.Current.MainWindow;

            LockScreenView lockScreenView = new LockScreenView();
            LockScreenViewModel viewModel = new LockScreenViewModel(lockScreenView);

            lockScreenView.DataContext = viewModel;

            // Register closing event
            lockScreenView.Closing += (senderLockScreen, eLockScreen) =>
            {
                // Prevent closing lock screen view when login have not success
                if (!lockScreenView.DialogResult.HasValue)
                    eLockScreen.Cancel = true;
            };

            // Get active window if main show popup
            Window activeWindow = mainWindow.OwnedWindows.Cast<Window>().SingleOrDefault(x => x.IsActive);
            if (activeWindow != null)
            {
                // Set active window is owner lock screen view
                lockScreenView.Owner = activeWindow;
            }
            else
            {
                // Set main window is owner lock screen view
                lockScreenView.Owner = mainWindow;
            }

            // Show lock screen view
            if (lockScreenView.ShowDialog().HasValue)
            {
                mainWindow.Activate();
            }
        }
        #endregion

        #region PropertyChanged

        private void SelectedSaleOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_SaleOrderModel saleOrderModel = sender as base_SaleOrderModel;
            switch (e.PropertyName)
            {
                case "SubTotal":
                    CalculateAllTax(saleOrderModel);
                    saleOrderModel.CalcDiscountAmount();
                    break;
                case "Total":
                    saleOrderModel.CalcBalance();
                    saleOrderModel.RewardAmount = saleOrderModel.Total - saleOrderModel.RewardValueApply;
                    break;
                case "RefundedAmount":
                case "RewardAmount":
                case "Deposit":
                    saleOrderModel.CalcBalance();
                    break;
                case "Paid":
                    saleOrderModel.CalcBalance();
                    break;
                case "Shipping":
                    saleOrderModel.ShipTaxAmount = CalcShipTaxAmount(saleOrderModel);
                    saleOrderModel.CalcTotal();
                    break;
                case "ProductTaxAmount":
                case "ShipTaxAmount":
                    if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel.IsTaxAfterDiscount)
                        saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                    else
                        saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;

                    break;
                case "TaxAmount":
                    saleOrderModel.CalcTotal();
                    break;
                case "DiscountAmount":
                    saleOrderModel.CalcDiscountPercent();
                    saleOrderModel.SkipDisc = false;
                    if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                    {
                        if (saleOrderModel.TaxLocationModel.TaxCodeModel.IsTaxAfterDiscount)
                            saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                        else
                            saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;
                    }

                    saleOrderModel.CalcTotal();
                    break;
                case "DiscountPercent":
                    saleOrderModel.CalcDiscountAmount();
                    saleOrderModel.SkipDisc = false;
                    break;
                case "PriceSchemaId"://Update Price When Price Schema Changed
                    PriceSchemaChanged();

                    saleOrderModel.PriceLevelItem = Common.PriceSchemas.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.PriceSchemaId));
                    break;
                case "OrderStatus":
                    //SetAllowChangeOrder(saleOrderModel);
                    saleOrderModel.SetFullPayment();

                    //Set Text Status
                    saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                    break;
                case "StoreCode":
                    break;
                case "TotalPaid":
                    saleOrderModel.ReturnModel.CalcBalance(saleOrderModel.TotalPaid);
                    break;

            }
        }


        private void SaleOrderDetailCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base_SaleOrderDetailModel saleOrderDetailModel;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    saleOrderDetailModel = item as base_SaleOrderDetailModel;
                    saleOrderDetailModel.PropertyChanged += new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    saleOrderDetailModel = item as base_SaleOrderDetailModel;
                    saleOrderDetailModel.PropertyChanged -= new PropertyChangedEventHandler(SaleOrderDetailModel_PropertyChanged);
                }
            }
        }

        private void SaleOrderDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BreakSODetailChange)
                return;
            base_SaleOrderDetailModel saleOrderDetailModel = sender as base_SaleOrderDetailModel;
            switch (e.PropertyName)
            {
                case "SalePrice":
                    saleOrderDetailModel.SalePriceChanged(false);
                    saleOrderDetailModel.CalcSubTotal();
                    CalculateMultiNPriceTax();
                    _saleOrderRepository.CheckToShowDatagridRowDetail(saleOrderDetailModel);
                    break;
                case "Quantity":
                    //Update child quantity when parent change (apply only for Product Group)

                    if (saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                    {
                        var childInGroup = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.Resource.ToString()));
                        if (childInGroup.Any())//Is a group 
                        {
                            foreach (base_SaleOrderDetailModel saleOrderDetaiInGrouplModel in childInGroup)
                            {
                                saleOrderDetaiInGrouplModel.Quantity = saleOrderDetaiInGrouplModel.ProductGroupItem.Quantity * saleOrderDetailModel.Quantity;
                                //Update Parent Pick Qty
                                //UpdatePickQtyForParent(saleOrderDetaiInGrouplModel);
                                if (saleOrderDetaiInGrouplModel.ProductModel.IsSerialTracking)
                                    OpenTrackingSerialNumber(saleOrderDetaiInGrouplModel, true, true);
                            }
                        }
                    }
                    else//Child of Product Group Change Quanity
                    {
                        //UpdatePickQtyForParent(saleOrderDetailModel);
                    }
                    saleOrderDetailModel.CalcDueQty();
                    saleOrderDetailModel.CalcSubTotal();
                    if (!saleOrderDetailModel.ProductModel.IsSerialTracking)
                    {
                        BreakSODetailChange = true;
                        //Calculate Discount with exited discount after that
                        _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                        BreakSODetailChange = false;
                    }

                    CalculateMultiNPriceTax();
                    SelectedSaleOrder.CalcSubTotal();
                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                    //SetShipStatus();
                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    break;
                case "DueQty":
                    saleOrderDetailModel.CalUnfill();
                    break;
                case "PickQty":
                    //Calc PickQty for parent if pickqty change is a child of ProductGroup
                    //UpdatePickQtyForParent(saleOrderDetailModel);

                    saleOrderDetailModel.CalcDueQty();
                    break;
                case "UOMId":
                    SetPriceUOM(saleOrderDetailModel);

                    CalculateDiscount(saleOrderDetailModel);

                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);

                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    break;
                case "SubTotal":
                    SelectedSaleOrder.CalcSubTotal();
                    break;
                case "IsQuantityAccepted":
                    if (SelectedSaleOrder.SaleOrderDetailCollection != null)
                        SelectedSaleOrder.IsHiddenErrorColumn = !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);
                    break;
            }
        }


        private void _timer_Tick(object sender, EventArgs e)
        {
            CurrentDateTime = DateTime.Now;
        }
        #endregion

    }
}
