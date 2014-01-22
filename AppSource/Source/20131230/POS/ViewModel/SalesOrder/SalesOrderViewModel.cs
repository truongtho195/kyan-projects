using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using CPC.DragDrop;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;
using CPCToolkitExtLibraries;

namespace CPC.POS.ViewModel
{
    public class SalesOrderViewModel : OrderViewModel, IDropTarget
    {
        #region Define

        public RelayCommand<object> DoubleClickViewCommand { get; private set; }

        //Respository
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();
        private base_GuestRewardRepository _guestRewardRepository = new base_GuestRewardRepository();
        private base_SaleOrderShipRepository _saleOrderShipRepository = new base_SaleOrderShipRepository();
        private base_SaleOrderShipDetailRepository _saleOrderShipDetailRepository = new base_SaleOrderShipDetailRepository();
        private base_ResourceReturnRepository _resourceReturnRepository = new base_ResourceReturnRepository();
        private base_ResourceReturnDetailRepository _resourceReturnDetailRepository = new base_ResourceReturnDetailRepository();
        private base_ProductGroupRepository _productGroupRepository = new base_ProductGroupRepository();
        private base_ProductStoreRepository _productStoreRespository = new base_ProductStoreRepository();
        private base_ProductUOMRepository _productUOMRepository = new base_ProductUOMRepository();

        private SalesOrderAdvanceSearchViewModel _salesOrderAdvanceSearchViewModel = new SalesOrderAdvanceSearchViewModel();
        //private BackgroundWorker _saleOrderBgWorker = new BackgroundWorker { WorkerReportsProgress = true };

        private enum SaleOrderTab
        {
            Order = 0,
            Ship = 1,
            Payment = 2,
            Return = 3
        }


        private bool IsAdvanced { get; set; }


        #endregion

        #region Constructors

        public SalesOrderViewModel()
            : base()
        {
            _requireProductCard = true;
            LoadDynamicData();

            //_saleOrderBgWorker.DoWork+=new DoWorkEventHandler(_saleOrderBgWorker_DoWork);
            //_saleOrderBgWorker.ProgressChanged +=new ProgressChangedEventHandler(_saleOrderBgWorker_ProgressChanged);
            //_saleOrderBgWorker.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(_saleOrderBgWorker_RunWorkerCompleted);

            //Get value from config
            IsIncludeReturnFee = Define.CONFIGURATION.IsIncludeReturnFee;
        }

        public SalesOrderViewModel(bool isList, object param)
            : this()
        {
            ChangeSearchMode(isList, param);
        }

        #endregion

        #region Properties

        #region IsForceFocused
        private bool _isForceFocused;
        /// <summary>
        /// Gets or sets the IsForceFocus.
        /// </summary>
        public bool IsForceFocused
        {
            get { return _isForceFocused; }
            set
            {
                if (_isForceFocused != value)
                {
                    _isForceFocused = value;
                    OnPropertyChanged(() => IsForceFocused);
                }
            }
        }
        #endregion

        #region IsIncludeReturnFee
        private bool _isIncludeReturnFee;
        /// <summary>
        /// Gets or sets the IsIncludeReturnFee.
        /// </summary>
        public bool IsIncludeReturnFee
        {
            get { return _isIncludeReturnFee; }
            set
            {
                if (_isIncludeReturnFee != value)
                {
                    _isIncludeReturnFee = value;
                    OnPropertyChanged(() => IsIncludeReturnFee);
                }
            }
        }
        #endregion

        #region IsSearchMode
        private bool isSearchMode = false;
        /// <summary>
        /// Search Mode: 
        /// true open the Search grid.
        /// false close the search grid and open data entry.
        /// </summary>
        public bool IsSearchMode
        {
            get { return isSearchMode; }
            set
            {
                if (value != isSearchMode)
                {
                    isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }
        #endregion

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

        #region IsShipValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsShipValid
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                if (SelectedSaleOrder.SaleOrderShipCollection == null || (SelectedSaleOrder.SaleOrderShipCollection != null && !SelectedSaleOrder.SaleOrderShipCollection.Any()))
                    return true;

                return (SelectedSaleOrder.SaleOrderShipCollection != null && !SelectedSaleOrder.SaleOrderShipCollection.Any(x => x.IsError));
            }

        }
        #endregion

        #region IsOrderValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsOrderValid
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                if (SelectedSaleOrder.SaleOrderDetailCollection == null || (SelectedSaleOrder.SaleOrderDetailCollection != null && !SelectedSaleOrder.SaleOrderDetailCollection.Any()))
                    return true;

                return (SelectedSaleOrder.SaleOrderDetailCollection != null && !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsError))
                    && !SelectedSaleOrder.SaleOrderDetailCollection.Any(x => !x.IsQuantityAccepted);
            }

        }
        #endregion

        #region IsReturnValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsReturnValid
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return false;
                return (SelectedSaleOrder.ReturnModel != null && !SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.HasError));
            }

        }
        #endregion

        #region IsValidTab
        private bool _isValidTab;
        /// <summary>
        /// Gets or sets the IsValidTab.
        /// </summary>
        public bool IsValidTab
        {
            get { return _isValidTab; }
            set
            {
                if (_isValidTab != value)
                {
                    _isValidTab = value;
                    OnPropertyChanged(() => IsValidTab);
                }
            }
        }
        #endregion

        #region StoreCollection
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
        #endregion

        #region FilterText & Keyword
        private string _filterText;
        /// <summary>
        /// Gets or sets the FilterText.
        /// <para>Keyword user input but not press enter</para>
        /// <remarks>Binding in textbox keyword</remarks>
        /// </summary>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    ResetTimer();
                    OnPropertyChanged(() => FilterText);

                }
            }
        }

        public string Keyword { get; set; }
        #endregion

        //Sale Order
        #region SaleOrderCollection
        private CollectionBase<base_SaleOrderModel> _saleOrderCollection = new CollectionBase<base_SaleOrderModel>();
        /// <summary>
        /// Gets or sets the SaleOrderCollection.
        /// </summary>
        public CollectionBase<base_SaleOrderModel> SaleOrderCollection
        {
            get { return _saleOrderCollection; }
            set
            {
                if (_saleOrderCollection != value)
                {
                    _saleOrderCollection = value;
                    OnPropertyChanged(() => SaleOrderCollection);
                }
            }
        }
        #endregion

        #region TotalSaleOrder
        private int _totalSaleOrder;
        /// <summary>
        /// Gets or sets the TotalSaleOrder.
        /// </summary>
        public int TotalSaleOrder
        {
            get { return _totalSaleOrder; }
            set
            {
                if (_totalSaleOrder != value)
                {
                    _totalSaleOrder = value;
                    OnPropertyChanged(() => TotalSaleOrder);
                }
            }
        }
        #endregion

        #region SelectedSaleOrderShip
        private base_SaleOrderShipModel _selectedSaleOrderShip;
        /// <summary>
        /// Gets or sets the SelectedSaleOrderShip.
        /// </summary>
        public base_SaleOrderShipModel SelectedSaleOrderShip
        {
            get { return _selectedSaleOrderShip; }
            set
            {
                if (_selectedSaleOrderShip != value)
                {
                    _selectedSaleOrderShip = value;
                    OnPropertyChanged(() => SelectedSaleOrderShip);
                    if (SelectedSaleOrderShip != null)
                    {
                        SelectedSaleOrderShip.PropertyChanged -= new PropertyChangedEventHandler(SelectedSaleOrderShip_PropertyChanged);
                        SelectedSaleOrderShip.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrderShip_PropertyChanged);
                    }
                }
            }
        }


        #endregion

        #region SaleOrderId
        private long _saleOrderId = 0;
        /// <summary>
        /// Gets or sets the QuotationId.
        /// </summary>
        public long SaleOrderId
        {
            get { return _saleOrderId; }
            set
            {
                if (_saleOrderId != value)
                {
                    _saleOrderId = value;
                    OnPropertyChanged(() => SaleOrderId);
                }
            }
        }
        /// <summary>
        /// Flag using for call from another from & set what tab user want
        /// </summary>
        private SaleOrderTab SaleOrderSelectedTab { get; set; }
        #endregion

        //Products
        #region SelectedTabIndex
        private int _previousTabIndex;
        private int _selectedTabIndex;
        /// <summary>
        /// Gets or sets the SelectedTabIndex.
        /// </summary>
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _previousTabIndex = _selectedTabIndex;
                    _selectedTabIndex = value;
                    TabChanged(value);
                    OnPropertyChanged(() => SelectedTabIndex);
                }
            }
        }
        #endregion

        //Return
        #region SaleOrderShipDetailFieldCollection
        private DataSearchCollection _saleOrderShipDetailFieldCollection;
        /// <summary>
        /// Gets or sets the SaleOrderShipDetailFieldCollection.
        /// </summary>
        public DataSearchCollection SaleOrderShipDetailFieldCollection
        {
            get { return _saleOrderShipDetailFieldCollection; }
            set
            {
                if (_saleOrderShipDetailFieldCollection != value)
                {
                    _saleOrderShipDetailFieldCollection = value;
                    OnPropertyChanged(() => SaleOrderShipDetailFieldCollection);
                }
            }
        }
        #endregion

        #region SelectedReturnDetail
        private object _selectedReturnDetail;
        /// <summary>
        /// Gets or sets the SelectedReturnDetail.
        /// </summary>
        public object SelectedReturnDetail
        {
            get { return _selectedReturnDetail; }
            set
            {
                if (_selectedReturnDetail != value)
                {
                    _selectedReturnDetail = value;
                    OnPropertyChanged(() => SelectedReturnDetail);
                    SelectedReturnDetailChanged();
                }
            }
        }


        #endregion

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected override bool OnNewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        protected override void OnNewCommandExecute(object param)
        {
            base.OnNewCommandExecute(param);
            if (ChangeViewExecute(null))
            {
                CreateNewSaleOrder();
                IsSearchMode = false;
                IsForceFocused = true;
            }
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected override bool OnSaveCommandCanExecute(object param)
        {
            return IsDirty && IsValid && IsShipValid & IsOrderValid & IsReturnValid;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        protected override void OnSaveCommandExecute(object param)
        {
            SaveSalesOrder(SelectedSaleOrder);
        }

        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected override bool OnDeleteCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew && (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Open) || SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Quote));
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        protected override void OnDeleteCommandExecute(object param)
        {
            if (SelectedSaleOrder != null)
            {
                //"Do you want to delete this item?"
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_VoidBill"), Language.GetMsg("SO_Title_VoidBill"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result.Is(MessageBoxResult.Yes))
                {
                    ReasonViewModel reasonViewModel = new ReasonViewModel(SelectedSaleOrder.VoidReason);
                    bool? dialogResult = _dialogService.ShowDialog<ReasonView>(_ownerViewModel, reasonViewModel, Language.GetMsg("SO_Title_VoidBill"));
                    if (dialogResult == true)
                    {
                        SelectedSaleOrder.VoidReason = reasonViewModel.Reason;
                        VoidBillProcess(SelectedSaleOrder);
                        IsSearchMode = true;
                    }
                }
            }
        }


        #endregion

        #region PrintCommand
        /// <summary>
        /// Gets the Print Command.
        /// <summary>

        public RelayCommand<string> PrintCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Print command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPrintCommandCanExecute(string param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew;
        }


        /// <summary>
        /// Method to invoke when the Print command is executed.
        /// </summary>
        private void OnPrintCommandExecute(string param)
        {
            if (param != null)
            {
                // Open  Report window   
                View.Report.ReportWindow rpt = new View.Report.ReportWindow();
                rpt.ShowReport("rptSODetails", "", param, SelectedSaleOrder);
            }
        }
        #endregion

        #region SearchCommand
        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        protected override bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        protected override void OnSearchCommandExecute(object param)
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();

            Keyword = FilterText ?? string.Empty;
            IsAdvanced = false;

            //Reset if advance search has value to know current search simple
            _salesOrderAdvanceSearchViewModel.ResetKeyword();

            //Create & Execute Search Simple
            _predicate = CreateSimpleSearchPredicate(Keyword);
            LoadDataByPredicate(_predicate, false, 0);

            //Case 2 : use the same backgroud worker
            //SaleOrderCollection.Clear();
            //if (!_saleOrderBgWorker.IsBusy)
            //    _saleOrderBgWorker.RunWorkerAsync();
        }



        #endregion

        #region SearchProductCommand
        protected override bool OnSearchProductCommandCanExecute(object param)
        {
            return base.OnSearchProductCommandCanExecute(param);

        }

        protected override void OnSearchProductCommandExecute(object param)
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
            else if (_requireProductCard && ExtensionProducts.Any(x => x.Barcode != null && x.Barcode.Equals(BarcodeProduct)))
            {
                SelectedProduct = ExtensionProducts.FirstOrDefault(x => x.Barcode.Equals(BarcodeProduct));
            }
            BarcodeProduct = string.Empty;
        }
        #endregion

        #region DoubleClickCommand

        /// <summary>
        /// Method to check whether the DoubleClick command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DoubleClick command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            if (param != null && IsSearchMode)
            {

                SelectedSaleOrder = param as base_SaleOrderModel;

                _saleOrderRepository.Refresh(SelectedSaleOrder.base_SaleOrder);

                SelectedSaleOrder.ToModelAndRaise();

                SetSaleOrderRelation(SelectedSaleOrder);

                SelectedSaleOrder.RaiseTotalPaid();

                _selectedTabIndex = (int)SaleOrderTab.Order;
                OnPropertyChanged(() => SelectedTabIndex);

                SetSelectedCustomer();

                SetAllowChangeOrder(SelectedSaleOrder);

                SelectedSaleOrder.IsDirty = false;

                OnPropertyChanged(() => AllowSOShipping);
                OnPropertyChanged(() => AllowSOReturn);

                IsSearchMode = false;
            }
            else if (!IsSearchMode)//Change from Edit form to Search Gird check view has dirty
            {
                if (this.ChangeViewExecute(null))
                    this.IsSearchMode = true;
            }
            else
                this.IsSearchMode = !this.IsSearchMode;//Change View To
        }

        #endregion

        #region LoadDatByStepCommand

        public RelayCommand<object> LoadStepCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadStep command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStep command is executed.
        /// </summary>
        private void OnLoadStepCommandExecute(object param)
        {
            _predicate = PredicateBuilder.True<base_SaleOrder>();
            if (IsAdvanced) //Current is use advanced search
            {
                _predicate = _salesOrderAdvanceSearchViewModel.SearchAdvancePredicate;
            }
            else //Simple Search
            {
                if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                    _predicate = CreateSimpleSearchPredicate(Keyword); //CreatePredicateWithConditionSearch(Keyword);           
            }

            LoadDataByPredicate(_predicate, false, SaleOrderCollection.Count);

            //_saleOrderBgWorker.RunWorkerAsync();
        }
        #endregion

        #region PickPackCommand
        /// <summary>
        /// Gets the PickPack Command.
        /// <summary>
        public RelayCommand<object> PickPackCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PickPack command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPickPackCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;

            return !SelectedSaleOrder.IsLocked && SelectedSaleOrder.SaleOrderDetailCollection != null && SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.DueQty > 0);
        }


        /// <summary>
        /// Method to invoke when the PickPack command is executed.
        /// </summary>
        private void OnPickPackCommandExecute(object param)
        {
            if (SelectedSaleOrder.Mark.Equals(MarkType.Layaway.ToDescription()) && SelectedSaleOrder.Balance > 0)
            {
                //msg:"You should paid in full"
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_NotifyShouldPaidInFull"), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;

            }

            PickPackViewModel pickPackViewModel = new PickPackViewModel(SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel != null && x.DueQty > 0));
            bool? dialogResult = _dialogService.ShowDialog<PickPackView>(_ownerViewModel, pickPackViewModel, Language.GetMsg("SO_Title_PickPack"));
            if (dialogResult == true)
            {

                if (pickPackViewModel.SaleOrderShipModel != null)
                {
                    pickPackViewModel.SaleOrderShipModel.SaleOrderId = SelectedSaleOrder.Id;
                    pickPackViewModel.SaleOrderShipModel.SaleOrderResource = SelectedSaleOrder.Resource.ToString();
                    //Check item is Existed in collection
                    var saleOrderShipModel = SelectedSaleOrder.SaleOrderShipCollection.SingleOrDefault(x => x.Resource == pickPackViewModel.SaleOrderShipModel.Resource);
                    if (saleOrderShipModel != null)
                    {
                        if (pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection != null && !pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection.Any())
                            saleOrderShipModel = pickPackViewModel.SaleOrderShipModel;
                        else
                            SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                    }
                    else
                        SelectedSaleOrder.SaleOrderShipCollection.Add(pickPackViewModel.SaleOrderShipModel);

                    //Update SaleOrderDetail
                    foreach (var item in pickPackViewModel.SaleOrderDetailList)
                    {
                        base_SaleOrderDetailModel saleOrderDetailUpdate = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == item.Resource);
                        saleOrderDetailUpdate.PickQty = item.PickQty;
                    }
                }

                SelectedSaleOrder.RaiseTotalPackedBox();
                SetShippingStatus(SelectedSaleOrder);
            }
        }
        #endregion

        #region DeleteSaleOrderShipCommand
        /// <summary>
        /// Gets the DeleteSaleOrderShip Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderShipCommand { get; private set; }


        /// <summary>
        /// Method to check whether the DeleteSaleOrderShip command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderShipCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return !(param as base_SaleOrderShipModel).IsShipped;
        }


        /// <summary>
        /// Method to invoke when the DeleteSaleOrderShip command is executed.
        /// </summary>
        private void OnDeleteSaleOrderShipCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;

            DeleteItemSaleOrderShip(saleOrderShipModel);
        }

        /// <summary>
        /// Gets the DeleteSaleOrderShip Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderShipWithKeyCommand { get; private set; }


        /// <summary>
        /// Method to check whether the DeleteSaleOrderShip command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderShipWithKeyCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteSaleOrderShip command is executed.
        /// </summary>
        private void OnDeleteSaleOrderShipWithKeyCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            if (saleOrderShipModel.IsShipped == true)
            {
                //This item is picked, can't delete
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ItemPicked"), Language.GetMsg("DeleteCaption"), MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }
            else
            {
                DeleteItemSaleOrderShip(saleOrderShipModel);
            }
        }

        /// <summary>
        ///confirm Delete item saleordership
        /// </summary>
        /// <param name="saleOrderShipModel"></param>
        private void DeleteItemSaleOrderShip(base_SaleOrderShipModel saleOrderShipModel)
        {
            //msg: NotifyDeleteItem
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Is(MessageBoxResult.Yes))
            {
                if (saleOrderShipModel.SaleOrderShipDetailCollection != null)
                {
                    //UpdatePickQty
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        Guid saleOrderDetailResource = Guid.Parse(saleOrderShipDetailModel.SaleOrderDetailResource);
                        var saleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == saleOrderDetailResource);
                        saleOrderDetailModel.PickQty -= saleOrderShipDetailModel.PackedQty;
                    }
                    SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                    SelectedSaleOrder.RaiseTotalPackedBox();
                }
            }
        }
        #endregion

        #region EditSaleOrderShipCommand
        /// <summary>
        /// Gets the EditSaleOrderShip Command.
        /// <summary>

        public RelayCommand<object> EditSaleOrderShipCommand { get; private set; }

        /// <summary>
        /// Method to check whether the EditSaleOrderShip command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditSaleOrderShipCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param as base_SaleOrderShipModel).IsShipped == false;
        }


        /// <summary>
        /// Method to invoke when the EditSaleOrderShip command is executed.
        /// </summary>
        private void OnEditSaleOrderShipCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            //Get list SaleOrderDetailResource
            var listSaleOrder = saleOrderShipModel.SaleOrderShipDetailCollection.Select(x => x.SaleOrderDetailResource);
            //Get SaleOrderDetail with list Sale OrderResource
            CollectionBase<base_SaleOrderDetailModel> listSaleOrderDetail = new CollectionBase<base_SaleOrderDetailModel>();
            //SelectedSaleOrder.SaleOrderDetailCollection.Where(x => listSaleOrder.Contains(x.Resource.ToString())).ToList();
            foreach (string resource in listSaleOrder)
            {
                base_SaleOrderDetailModel saleOrderDetailPacked = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(resource));
                if (saleOrderDetailPacked != null)
                {
                    //Check Has Parent => Add item parent 
                    if (!string.IsNullOrWhiteSpace(saleOrderDetailPacked.ParentResource))
                    {
                        base_SaleOrderDetailModel saleOrdeDetailParent = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetailPacked.ParentResource));
                        if (saleOrdeDetailParent != null && listSaleOrderDetail.Any(x => x.Resource.Equals(saleOrdeDetailParent.Resource)))
                            listSaleOrderDetail.Add(saleOrdeDetailParent);
                    }

                    //Add Children
                    listSaleOrderDetail.Add(saleOrderDetailPacked);
                }
            }


            PickPackViewModel pickPackViewModel = new PickPackViewModel(listSaleOrderDetail, saleOrderShipModel);
            bool? dialogResult = _dialogService.ShowDialog<PickPackView>(_ownerViewModel, pickPackViewModel, Language.GetMsg("SO_Title_EditPickPack"));
            if (dialogResult == true)
            {
                if (pickPackViewModel.SaleOrderShipModel != null)
                {
                    if (pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection != null && !pickPackViewModel.SaleOrderShipModel.SaleOrderShipDetailCollection.Any())
                    {
                        SelectedSaleOrder.SaleOrderShipCollection.Remove(saleOrderShipModel);
                    }
                    else
                    {
                        //Update SaleOrderShip
                        saleOrderShipModel = pickPackViewModel.SaleOrderShipModel;
                    }
                }
                //Update SaleOrderDetail
                foreach (var item in pickPackViewModel.SaleOrderDetailList)
                {
                    base_SaleOrderDetailModel saleOrderDetailUpdate = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource == item.Resource);
                    saleOrderDetailUpdate.PickQty = item.PickQty;
                }
                SelectedSaleOrder.RaiseTotalPackedBox();
            }

        }
        #endregion

        #region ViewPnPDetailCommand
        /// <summary>
        /// Gets the ViewPnPDetail Command.
        /// <summary>

        public RelayCommand<object> ViewPnPDetailCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ViewPnPDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnViewPnPDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the ViewPnPDetail command is executed.
        /// </summary>
        private void OnViewPnPDetailCommandExecute(object param)
        {
            base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
            //Get list SaleOrderDetailResource
            var listSaleOrder = saleOrderShipModel.SaleOrderShipDetailCollection.Select(x => x.SaleOrderDetailResource);
            //Get SaleOrderDetail with list Sale OrderResource
            var listSaleOrderDetail = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => listSaleOrder.Contains(x.Resource.ToString())).ToList();

            PickPackViewModel pickPackViewModel = new PickPackViewModel(listSaleOrderDetail, saleOrderShipModel, true);
            bool? dialogResult = _dialogService.ShowDialog<PickPackView>(_ownerViewModel, pickPackViewModel, Language.GetMsg("SO_Title_ViewPickPack"));
        }
        #endregion

        #region Shipped Command
        /// <summary>
        /// Gets the Shipped Command.
        /// <summary>

        public RelayCommand<object> ShippedCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Shipped command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShippedCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the Shipped command is executed.
        /// </summary>
        private void OnShippedCommandExecute(object param)
        {
            ShippedProcess(param);
        }


        #endregion

        #region PaymentCommand
        /// <summary>
        /// Gets the Payment Command.
        /// <summary>

        public RelayCommand<object> PaymentCommand { get; private set; }


        /// Method to check whether the Payment command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPaymentCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null || SelectedSaleOrder.PaymentCollection == null)
                return false;

            decimal returnValue = 0;
            if (SelectedSaleOrder.ReturnModel != null && SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any())
            {
                //decimal returnTax = Math.Round(Math.Round(CalculateReturnTax(SelectedSaleOrder.ReturnModel, SelectedSaleOrder), Define.CONFIGURATION.DecimalPlaces.Value) - 0.01M, MidpointRounding.AwayFromZero);
                returnValue = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsReturned).Sum(x => x.Amount + x.VAT - x.RewardRedeem - ((x.Amount * SelectedSaleOrder.DiscountPercent) / 100));
            }

            //End Return Product Proccess
            decimal paidValue = SelectedSaleOrder.PaymentCollection.Sum(x => x.TotalPaid - x.Change);
            decimal balance = SelectedSaleOrder.RewardAmount - returnValue - paidValue - (SelectedSaleOrder.Deposit ?? 0);

            return IsOrderValid
                && balance > 0
                && !SelectedSaleOrder.IsNew
                && !SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull)
                && SelectedSaleOrder.SubTotal > 0;
        }

        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary> 
        private void OnPaymentCommandExecute(object param)
        {
            SaleOrderPayment();
            SelectedSaleOrder.RaiseTotalPaid();
            SelectedSaleOrder.PaymentProcess = SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any();
            ShowShipTab(SelectedSaleOrder);
        }
        #endregion

        #region PaymentHistoryDetailCommand
        /// <summary>
        /// Gets the PaymentHistoryDetail Command.
        /// <summary>

        public RelayCommand<object> PaymentHistoryDetailCommand { get; private set; }


        /// <summary>
        /// Method to check whether the PaymentHistoryDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPaymentHistoryDetailCommandCanExecute(object param)
        {
            return param != null;
        }


        /// <summary>
        /// Method to invoke when the PaymentHistoryDetail command is executed.
        /// </summary>
        private void OnPaymentHistoryDetailCommandExecute(object param)
        {
            base_ResourcePaymentModel paymentModel = param as base_ResourcePaymentModel;
            POSOPaymentHistoryDetailViewModel viewModel = new POSOPaymentHistoryDetailViewModel(paymentModel);

            bool? dialogResult = _dialogService.ShowDialog<POSOPaymentHistoryDetailView>(_ownerViewModel, viewModel, Language.GetMsg("Title_PaymentHistoryDetail"));

        }
        #endregion

        #region LockOrderCommand
        /// <summary>
        /// Gets the LockOrder Command.
        /// <summary>

        public RelayCommand<object> LockOrderCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LockOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLockOrderCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return IsValid && IsShipValid & IsOrderValid & IsReturnValid;
        }


        /// <summary>
        /// Method to invoke when the LockOrder command is executed.
        /// </summary>
        private void OnLockOrderCommandExecute(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_LockOrder"), Language.GetMsg("InformationCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (msgResult.Is(MessageBoxResult.Yes))
            {

                SelectedSaleOrder.IsLocked = true;
                SetAllowChangeOrder(SelectedSaleOrder);
                SaveSalesOrder(SelectedSaleOrder);
                SaleOrderCollection.Remove(SelectedSaleOrder);
                TotalSaleOrder -= 1;
                IsSearchMode = true;
            }

        }
        #endregion

        //Main Grid
        #region DeleteItemsCommand
        /// <summary>
        /// Gets the DeleteItems Command.
        /// <summary>

        public RelayCommand<object> DeleteItemsCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteItems command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteItemsCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count > 0 && !datagrid.SelectedItems.Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Open));
        }


        /// <summary>
        /// Method to invoke when the DeleteItems command is executed.
        /// </summary>
        private void OnDeleteItemsCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            object collectionDelete = datagrid.SelectedItems.Cast<object>();
            DeleteItemsSaleOrder(collectionDelete);
        }



        /// <summary>
        /// Gets the DeleteItems Command.
        /// <summary>

        public RelayCommand<object> DeleteItemsWithKeyCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteItems command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteItemsWithKeyCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteItems command is executed.
        /// </summary>
        private void OnDeleteItemsWithKeyCommandExecute(object param)
        {
            if (param == null || (param as ObservableCollection<object>).Cast<base_SaleOrderModel>().Any(x => !x.OrderStatus.Equals((short)SaleOrderStatus.Open)))
            {

                //you could delete these item(s) with staus is open
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_CouldDeleteItemIsOpen"), Language.GetMsg("InformationCaption"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }

            DeleteItemsSaleOrder(param);
        }

        /// <summary>
        /// Confirm & delete saleorders
        /// </summary>
        /// <param name="param"></param>
        private void DeleteItemsSaleOrder(object param)
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_VoidBill"), Language.GetMsg("SO_Title_VoidBill"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                ReasonViewModel reasonViewModel = new ReasonViewModel(string.Empty);
                bool? dialogResult = _dialogService.ShowDialog<ReasonView>(_ownerViewModel, reasonViewModel, Language.GetMsg("SO_Title_VoidBill"));
                if (dialogResult ?? false)
                {
                    foreach (base_SaleOrderModel saleOrderModel in (param as ObservableCollection<object>).Cast<base_SaleOrderModel>().ToList())
                    {
                        saleOrderModel.VoidReason = reasonViewModel.Reason;

                        //Need to set To collection, because may be this item not double click to load relation to saleorder item
                        SetSaleOrderRelation(saleOrderModel);

                        saleOrderModel.RaiseTotalPaid();

                        VoidBillProcess(saleOrderModel);
                    }

                    if (SelectedSaleOrder != null)
                        _selectedSaleOrder = null;
                }
            }
        }
        #endregion

        #region DuplicateItemCommand
        /// <summary>
        /// Gets the DuplicateItem Command.
        /// <summary>

        public RelayCommand<object> DuplicateItemCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DuplicateItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDuplicateItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count == 1;
        }


        /// <summary>
        /// Method to invoke when the DuplicateItem command is executed.
        /// </summary>
        private void OnDuplicateItemCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            base_SaleOrderModel saleOrderSource = datagrid.SelectedItem as base_SaleOrderModel;
            CreateNewSaleOrder();
            SelectedSaleOrder.CopyFrom(saleOrderSource);

            SetSaleOrderToModel(SelectedSaleOrder);
            //Check not set to collection
            if (saleOrderSource.SaleOrderDetailCollection == null && saleOrderSource.base_SaleOrder.base_SaleOrderDetail.Any())
                SetSaleOrderRelation(saleOrderSource);

            if (saleOrderSource.SaleOrderDetailCollection != null)
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderSource.SaleOrderDetailCollection.Where(x => string.IsNullOrWhiteSpace(x.ParentResource)))
                {
                    if (saleOrderDetailModel.ProductModel == null)
                        continue;

                    string parentResource = CloneSaleOrderDetailModel(saleOrderDetailModel);
                    if (!string.IsNullOrWhiteSpace(parentResource))
                    {
                        //Get Child item follow Resource parent
                        var childInGroup = saleOrderSource.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.Resource.ToString()));
                        if (childInGroup.Any())//Is a group 
                        {
                            foreach (base_SaleOrderDetailModel saleOrderDetaiInGrouplModel in childInGroup)
                            {
                                CloneSaleOrderDetailModel(saleOrderDetaiInGrouplModel, parentResource);
                            }
                        }
                    }
                }
            }
            SelectedSaleOrder.CalcSubTotal();
            SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total;
            SelectedSaleOrder.CalcBalance();
            _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
            CalculateAllTax(SelectedSaleOrder);

            SaveSalesOrder(SelectedSaleOrder);

            _selectedTabIndex = (int)SaleOrderTab.Order;
            OnPropertyChanged(() => SelectedTabIndex);
            _selectedCustomer = null;
            //Set for selectedCustomer

            _selectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));
            OnPropertyChanged(() => SelectedCustomer);

            SetAllowChangeOrder(SelectedSaleOrder);
            SelectedSaleOrder.IsDirty = false;

            IsSearchMode = false;
        }

        /// <summary>
        /// Clone & AddNew SaleOrderDetail
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        /// <returns></returns>
        private string CloneSaleOrderDetailModel(base_SaleOrderDetailModel saleOrderDetailModel, string parentResource = "")
        {
            base_SaleOrderDetailModel newSaleOrderDetailModel = new base_SaleOrderDetailModel();
            newSaleOrderDetailModel.Resource = Guid.NewGuid();
            newSaleOrderDetailModel.ParentResource = parentResource;
            newSaleOrderDetailModel.CopyFrom(saleOrderDetailModel);

            //Calculate Discount for product
            CalculateDiscount(newSaleOrderDetailModel);

            //Check Show Detail
            _saleOrderRepository.CheckToShowDatagridRowDetail(newSaleOrderDetailModel);
            newSaleOrderDetailModel.CalcSubTotal();
            newSaleOrderDetailModel.CalcDueQty();
            newSaleOrderDetailModel.CalUnfill();


            //Set Item type Sale Order to know item is group/child or none
            if (newSaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                newSaleOrderDetailModel.ItemType = 1;
            else if (!string.IsNullOrWhiteSpace(newSaleOrderDetailModel.ParentResource))//Child item of group
                newSaleOrderDetailModel.ItemType = 2;
            else
                newSaleOrderDetailModel.ItemType = 0;

            SelectedSaleOrder.SaleOrderDetailCollection.Add(newSaleOrderDetailModel);
            _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, newSaleOrderDetailModel);
            return newSaleOrderDetailModel.Resource.ToString();
        }
        #endregion

        #region EditItemCommand
        /// <summary>
        /// Gets the EditItem Command.
        /// <summary>

        public RelayCommand<object> EditItemCommand { get; private set; }


        /// <summary>
        /// Method to check whether the EditItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            DataGridControl datagrid = param as DataGridControl;
            return datagrid.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Method to invoke when the EditItem command is executed.
        /// </summary>
        private void OnEditItemCommandExecute(object param)
        {
            DataGridControl datagrid = param as DataGridControl;
            OnDoubleClickViewCommandExecute(datagrid.SelectedItem);
        }
        #endregion

        #region SaleOrderAdvanceSearchCommand
        /// <summary>
        /// Gets the SaleOrderAdvanceSearch Command.
        /// <summary>

        public RelayCommand<object> SaleOrderAdvanceSearchCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SaleOrderAdvanceSearch command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaleOrderAdvanceSearchCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SaleOrderAdvanceSearch command is executed.
        /// </summary>
        private void OnSaleOrderAdvanceSearchCommandExecute(object param)
        {
            OpenSOAdvanceSearch();
        }


        #endregion

        //Return
        #region ReturnAllCommand
        /// <summary>
        /// Gets the ReturnAll Command.
        /// <summary>

        public RelayCommand<object> ReturnAllCommand { get; private set; }

        ///Inital ReturnAll Command
        ///Need move to Constructor


        /// <summary>
        /// Method to check whether the ReturnAll command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnReturnAllCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null || SelectedSaleOrder.ReturnModel == null || SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.HasError))
                return false;
            return SelectedSaleOrder.SaleOrderShipCollection != null && SelectedSaleOrder.SaleOrderShipCollection.Any(x => x.IsShipped);
        }


        /// <summary>
        /// Method to invoke when the ReturnAll command is executed.
        /// </summary>
        private void OnReturnAllCommandExecute(object param)
        {
            ReturnAll();
        }


        #endregion

        #region DeleteReturnDetailCommand
        /// <summary>
        /// Gets the DeleteReturnDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteReturnDetailCommand { get; private set; }

        ///Inital DeleteReturnDetail Command
        ///Need move to Constructor


        /// <summary>
        /// Method to check whether the DeleteReturnDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteReturnDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param is base_ResourceReturnDetailModel) && !(param as base_ResourceReturnDetailModel).IsReturned;
        }


        /// <summary>
        /// Method to invoke when the DeleteReturnDetail command is executed.
        /// </summary>
        private void OnDeleteReturnDetailCommandExecute(object param)
        {
            base_ResourceReturnDetailModel returnDetailModel = param as base_ResourceReturnDetailModel;
            if (SelectedReturnDetail == null || returnDetailModel.IsTemporary)
                return;
            DeleteItemSaleOrderReturnDetail(returnDetailModel);
        }

        /// <summary>
        /// Gets the DeleteReturnDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteReturnDetailWithKeyCommand { get; private set; }

        ///Inital DeleteReturnDetail Command
        ///Need move to Constructor


        /// <summary>
        /// Method to check whether the DeleteReturnDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteReturnDetailWithKeyCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DeleteReturnDetail command is executed.
        /// </summary>
        private void OnDeleteReturnDetailWithKeyCommandExecute(object param)
        {
            base_ResourceReturnDetailModel returnDetailModel = SelectedReturnDetail as base_ResourceReturnDetailModel;
            if (returnDetailModel == null || returnDetailModel.IsTemporary)
                return;

            if (returnDetailModel.IsReturned)
            {
                //Msg: Item has been returned, can not delete this item."
                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_NotifyItemReturned"), Language.GetMsg("InformationCaption"), MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }
            DeleteItemSaleOrderReturnDetail(returnDetailModel);
        }

        /// <summary>
        /// Delete SaleOrderReturn Detail
        /// </summary>
        /// <param name="returnDetailModel"></param>
        private void DeleteItemSaleOrderReturnDetail(base_ResourceReturnDetailModel returnDetailModel)
        {
            // Try to find ResourceReturnDetail error.
            base_ResourceReturnDetailModel resourceReturnDetailError = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.FirstOrDefault(x => x.HasError);
            bool isContainsErrorItem = false;
            if (resourceReturnDetailError != null)
            {
                ListCollectionView resourceReturnDetailView = CollectionViewSource.GetDefaultView(SelectedSaleOrder.ReturnModel.ReturnDetailCollection) as ListCollectionView;
                if (resourceReturnDetailView != null)
                {
                    if (resourceReturnDetailView.CurrentEditItem != null)
                        isContainsErrorItem = object.ReferenceEquals(resourceReturnDetailView.CurrentEditItem, returnDetailModel);
                    else if (resourceReturnDetailView.CurrentAddItem != null)
                        isContainsErrorItem = object.ReferenceEquals(resourceReturnDetailView.CurrentAddItem, returnDetailModel);
                    else
                        isContainsErrorItem = true;
                }
            }

            if (resourceReturnDetailError == null || isContainsErrorItem)
            {
                //msg:Do you want to delete item(s) ?
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Remove(returnDetailModel);
                    CalculateReturnSubtotal(SelectedSaleOrder);
                }
            }
        }
        #endregion

        #region SaleOrderRefunded Command
        /// <summary>
        /// Gets the SaleOrderRefunded Command.
        /// <summary>

        public RelayCommand<object> SaleOrderRefundedCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SaleOrderRefunded command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaleOrderRefundedCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Sum(x => x.TotalPaid - x.Change) > 0/*Has paid*/
                && SelectedSaleOrder.ReturnModel != null
                && SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.IsReturned);//Has Return
        }


        /// <summary>
        /// Method to invoke when the SaleOrderRefunded command is executed.
        /// </summary>
        private void OnSaleOrderRefundedCommandExecute(object param)
        {
            RefundViewModel viewModel = new RefundViewModel(SelectedSaleOrder);
            bool? dialogResult = _dialogService.ShowDialog<RefundView>(_ownerViewModel, viewModel, Language.GetMsg("SO_Title_SaleOrderRefund"));
            if (dialogResult == true)
            {
                base_ResourcePaymentModel refundPaymentModel = viewModel.PaymentModel;
                if (Define.CONFIGURATION.DefaultCashiedUserName ?? false)
                    refundPaymentModel.Cashier = Define.USER.LoginName;
                refundPaymentModel.Shift = Define.ShiftCode;
                SelectedSaleOrder.PaymentCollection.Add(refundPaymentModel);
                SelectedSaleOrder.Paid = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid > 0).Sum(x => x.TotalPaid);
                SelectedSaleOrder.ReturnModel.TotalRefund = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid < 0).Sum(x => x.TotalPaid);
            }
        }
        #endregion

        #endregion "\Commands Methods"

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        protected override void InitialCommand()
        {
            base.InitialCommand();

            PrintCommand = new RelayCommand<string>(OnPrintCommandExecute, OnPrintCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            PickPackCommand = new RelayCommand<object>(OnPickPackCommandExecute, OnPickPackCommandCanExecute);
            DeleteSaleOrderShipCommand = new RelayCommand<object>(OnDeleteSaleOrderShipCommandExecute, OnDeleteSaleOrderShipCommandCanExecute);
            DeleteSaleOrderShipWithKeyCommand = new RelayCommand<object>(OnDeleteSaleOrderShipWithKeyCommandExecute, OnDeleteSaleOrderShipWithKeyCommandCanExecute);
            EditSaleOrderShipCommand = new RelayCommand<object>(OnEditSaleOrderShipCommandExecute, OnEditSaleOrderShipCommandCanExecute);
            ViewPnPDetailCommand = new RelayCommand<object>(OnViewPnPDetailCommandExecute, OnViewPnPDetailCommandCanExecute);
            ShippedCommand = new RelayCommand<object>(OnShippedCommandExecute, OnShippedCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);
            PaymentHistoryDetailCommand = new RelayCommand<object>(OnPaymentHistoryDetailCommandExecute, OnPaymentHistoryDetailCommandCanExecute);

            LockOrderCommand = new RelayCommand<object>(OnLockOrderCommandExecute, OnLockOrderCommandCanExecute);
            //Using for Main Datagrid
            DeleteItemsCommand = new RelayCommand<object>(OnDeleteItemsCommandExecute, OnDeleteItemsCommandCanExecute);
            DeleteItemsWithKeyCommand = new RelayCommand<object>(OnDeleteItemsWithKeyCommandExecute, OnDeleteItemsWithKeyCommandCanExecute);
            DuplicateItemCommand = new RelayCommand<object>(OnDuplicateItemCommandExecute, OnDuplicateItemCommandCanExecute);
            EditItemCommand = new RelayCommand<object>(OnEditItemCommandExecute, OnEditItemCommandCanExecute);
            SaleOrderAdvanceSearchCommand = new RelayCommand<object>(OnSaleOrderAdvanceSearchCommandExecute, OnSaleOrderAdvanceSearchCommandCanExecute);

            //Return
            ReturnAllCommand = new RelayCommand<object>(OnReturnAllCommandExecute, OnReturnAllCommandCanExecute);
            DeleteReturnDetailCommand = new RelayCommand<object>(OnDeleteReturnDetailCommandExecute, OnDeleteReturnDetailCommandCanExecute);
            DeleteReturnDetailWithKeyCommand = new RelayCommand<object>(OnDeleteReturnDetailWithKeyCommandExecute, OnDeleteReturnDetailWithKeyCommandCanExecute);
            SaleOrderRefundedCommand = new RelayCommand<object>(OnSaleOrderRefundedCommandExecute, OnSaleOrderRefundedCommandCanExecute);

        }

        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        public bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                //Some data has changed. Do you want to save?
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M106"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                    {

                        result = SaveSalesOrder(SelectedSaleOrder);
                    }
                    else //Has Error
                        result = false;
                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (SelectedSaleOrder.IsNew)
                    {
                        _selectedSaleOrder = null;
                        if (!isClosing ?? false)
                            IsSearchMode = true;
                    }
                    else //Old Item Rollback data
                    {
                        SelectedSaleOrder.ToModelAndRaise();
                        SetSaleOrderToModel(SelectedSaleOrder);
                        SetSaleOrderRelation(SelectedSaleOrder, true);

                    }
                }
                else
                {
                    result = false;
                }

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadStaticData()
        {
            base.LoadStaticData();

            SaleOrderShipDetailFieldCollection = new DataSearchCollection
            {
                 new DataSearchModel { ID = 1, Level = 0, DisplayName = Language.GetMsg("C174"), KeyName = "ItemCode" },
                 new DataSearchModel { ID = 3, Level = 0, DisplayName = Language.GetMsg("C175"), KeyName = "ItemName" },
                 new DataSearchModel { ID = 4, Level = 0, DisplayName = Language.GetMsg("C116"), KeyName = "ItemAtribute" },
                 new DataSearchModel { ID = 6, Level = 0, DisplayName = Language.GetMsg("C117"), KeyName = "ItemSize" }
            };

        }

        /// <summary>
        /// Load relate data with form from database
        /// </summary>
        protected override void LoadDynamicData()
        {
            //Load
            base.LoadDynamicData();

            //Load Extention
            //Get Store
            LoadStores();

            //Load Product Extension
            LoadProductExtension();
        }

        /// <summary>
        /// Load Store from db
        /// </summary>
        private void LoadStores()
        {
            IList<base_Store> stores = _storeRepository.GetAll();
            if (StoreCollection == null)
                StoreCollection = new ObservableCollection<base_Store>(stores.OrderBy(x => x.Id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        protected override void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            base.SetSaleOrderRelation(saleOrderModel, isForce);

            SetForSaleOrderShip(saleOrderModel, isForce);

            //Get SaleOrderShipDetail for return
            SetForShippedCollection(saleOrderModel, isForce);

            SetToSaleOrderReturn(saleOrderModel, isForce);

            //Load GuestReward SaleOrder
            SetGuestRewardSaleOrderModel(saleOrderModel);

            saleOrderModel.RaiseAnyShipped();
            saleOrderModel.IsDirty = false;
        }

        /// <summary>
        /// Load Sale Order Detail Collection with SaleOrderDetailCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce">Can set SaleOrderDetailCollection when difference null</param>
        protected override void SetSaleOrderDetail(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            base.SetSaleOrderDetail(saleOrderModel, isForce);

            ShowShipTab(saleOrderModel);
        }

        /// <summary>
        /// Load Sale Order Shippeds Collection with SaleOrderShipDetailCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce">Can set SaleOrderShipDetailCollection when difference null</param>
        private void SetForShippedCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            if ((saleOrderModel.SaleOrderShipDetailCollection == null && saleOrderModel.SaleOrderShipCollection != null) || isForce)
            {
                saleOrderModel.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
                foreach (base_SaleOrderShipModel saleOrderShipModel in saleOrderModel.SaleOrderShipCollection.Where(x => x.IsShipped == true))
                {
                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        saleOrderModel.SaleOrderShipDetailCollection.Add(saleOrderShipDetailModel);

                    }
                }

                saleOrderModel.SaleOrderShippedCollection = new CollectionBase<base_SaleOrderDetailModel>();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    //Item is shipped
                    if (saleOrderModel.SaleOrderShipDetailCollection.Any(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())))
                    {
                        //Item is shipped => lock Uom this item
                        if (!saleOrderDetailModel.IsReadOnlyUOM)
                            saleOrderDetailModel.IsReadOnlyUOM = true;

                        base_SaleOrderDetailModel saleOrderShipModel = saleOrderDetailModel.Clone();
                        saleOrderShipModel.IsNew = false;
                        saleOrderShipModel.PickQty = saleOrderModel.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty);
                        saleOrderShipModel.SubTotal = saleOrderShipModel.PickQty * saleOrderShipModel.SalePrice;
                        saleOrderModel.SaleOrderShippedCollection.Add(saleOrderShipModel);
                    }
                }
            }
        }

        /// <summary>
        /// Load Sale Order Return
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetToSaleOrderReturn(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Get Return Resource
            string saleOrderResource = saleOrderModel.Resource.ToString();
            base_ResourceReturn resourceReturn = _resourceReturnRepository.Get(x => x.DocumentResource.Equals(saleOrderResource));


            if (resourceReturn != null)
            {
                saleOrderModel.ReturnModel = new base_ResourceReturnModel(resourceReturn);
                saleOrderModel.ReturnModel.IsDirty = false;
            }
            else
            {
                saleOrderModel.ReturnModel = new base_ResourceReturnModel();
                saleOrderModel.ReturnModel.DocumentNo = saleOrderModel.SONumber;
                saleOrderModel.ReturnModel.TotalAmount = saleOrderModel.Total;
                saleOrderModel.ReturnModel.DocumentResource = saleOrderModel.Resource.ToString();
                saleOrderModel.ReturnModel.Resource = Guid.NewGuid();
                saleOrderModel.ReturnModel.Redeemed = 0;
                saleOrderModel.ReturnModel.TotalRefund = 0;
                saleOrderModel.ReturnModel.Mark = "SO";
                saleOrderModel.ReturnModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                saleOrderModel.ReturnModel.DateCreated = DateTime.Today;
                saleOrderModel.ReturnModel.IsDirty = false;
            }

            if (isForce || saleOrderModel.ReturnModel.ReturnDetailCollection == null || !saleOrderModel.ReturnModel.ReturnDetailCollection.Any())
            {
                saleOrderModel.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
                saleOrderModel.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;

                foreach (base_ResourceReturnDetail resourceReturnDetail in saleOrderModel.ReturnModel.base_ResourceReturn.base_ResourceReturnDetail)
                {
                    base_ResourceReturnDetailModel returnDetailModel = new base_ResourceReturnDetailModel(resourceReturnDetail);
                    returnDetailModel.SaleOrderModel = saleOrderModel;
                    returnDetailModel.SaleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource));
                    returnDetailModel.UnitName = returnDetailModel.SaleOrderDetailModel.UnitName;
                    CalcReturnDetailSubTotal(saleOrderModel, returnDetailModel);
                    saleOrderModel.ReturnModel.ReturnDetailCollection.Add(returnDetailModel);
                    returnDetailModel.IsDirty = false;
                    returnDetailModel.IsTemporary = false;
                }
            }
            saleOrderModel.ReturnModel.PropertyChanged += new PropertyChangedEventHandler(ReturnModel_PropertyChanged);
        }

        /// <summary>
        /// Load Sale Order Ship Collection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void SetForSaleOrderShip(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Collection Sale Order Ship
            if (isForce || saleOrderModel.SaleOrderShipCollection == null || !saleOrderModel.SaleOrderShipCollection.Any())
            {
                saleOrderModel.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();

                foreach (base_SaleOrderShip saleOrderShip in saleOrderModel.base_SaleOrder.base_SaleOrderShip)
                {
                    _saleOrderShipRepository.Refresh(saleOrderShip);
                    base_SaleOrderShipModel saleOrderShipModel = new base_SaleOrderShipModel(saleOrderShip);
                    saleOrderShipModel.IsChecked = saleOrderShipModel.IsShipped;
                    saleOrderShipModel.IsDirty = false;
                    //SaleOrderShipDetail
                    saleOrderShipModel.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
                    foreach (base_SaleOrderShipDetail saleOrderShipDetail in saleOrderShip.base_SaleOrderShipDetail)
                    {
                        base_SaleOrderShipDetailModel saleOrderShipDetailModel = new base_SaleOrderShipDetailModel(saleOrderShipDetail);
                        saleOrderShipDetailModel.SaleOrderDetailModel = saleOrderModel.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetail.SaleOrderDetailResource));
                        saleOrderShipDetailModel.IsDirty = false;
                        saleOrderShipModel.SaleOrderShipDetailCollection.Add(saleOrderShipDetailModel);
                    }
                    saleOrderModel.SaleOrderShipCollection.Add(saleOrderShipModel);
                }
            }
        }

        /// <summary>
        /// Set CustomerRewardCollection for RewardMember
        /// <remarks>Require validation memebership</remarks>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetCustomerRewardCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            //Get GuestReward collection
            if (isForce || saleOrderModel.GuestModel.GuestRewardCollection == null || !saleOrderModel.GuestModel.GuestRewardCollection.Any())
            {
                saleOrderModel.GuestModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    foreach (base_GuestReward guestReward in saleOrderModel.GuestModel.base_Guest.base_GuestReward.Where(x => x.GuestId.Equals(saleOrderModel.GuestModel.Id) && !x.IsApply && x.ActivedDate.Value <= DateTime.Today && (!x.ExpireDate.HasValue || x.ExpireDate.HasValue && DateTime.Today <= x.ExpireDate.Value)))
                    {
                        saleOrderModel.GuestModel.GuestRewardCollection.Add(new base_GuestRewardModel(guestReward));
                    }
                }
            }
        }


        /// <summary>
        /// Load Guest Reward SaleOrder is Existed
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetGuestRewardSaleOrderModel(base_SaleOrderModel saleOrderModel)
        {
            string saleOrderResource = saleOrderModel.Resource.ToString();
            base_GuestRewardSaleOrder guestRewardSaleOrder = _guestRewardSaleOrderRepository.Get(x => x.SaleOrderResource.Equals(saleOrderResource));
            if (guestRewardSaleOrder != null)
            {
                saleOrderModel.GuestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel(guestRewardSaleOrder);
            }
            else
            {
                saleOrderModel.GuestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel();
            }

        }

        /// <summary>
        /// Set SelectedCustomer with no raise selection change in combobox
        /// <para>Check Customer is existed(deactived customer) & get from CustomerCollection.DeletedItems to show </para>
        /// </summary>
        private void SetSelectedCustomer()
        {
            //Remove another customer deactived
            RemoveCustomerDeactived();

            //Check customer is a Active or Deactive 
            //if Deactive => need to get customer deactive store in CustomerCollection.DeletedItems && add to collection using for choice customer
            if (!CustomerCollection.Any(x => x.Resource.Equals(SelectedSaleOrder.GuestModel.Resource)))
            {
                base_GuestModel customerDeActived = CustomerCollection.DeletedItems.SingleOrDefault(x => x.Resource.Equals(SelectedSaleOrder.GuestModel.Resource));
                CustomerCollection.Add(customerDeActived);
                CustomerCollection.DeletedItems.Remove(customerDeActived);
            }
            //Set for selectedCustomer
            _selectedCustomer = SelectedSaleOrder.GuestModel;
            OnPropertyChanged(() => SelectedCustomer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex"></param>
        private void LoadDataByPredicate(Expression<Func<base_SaleOrder, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            if (IsBusy)//Break multi call to server
            {
                return;
            }

            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                SaleOrderCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;

                Expression<Func<base_SaleOrder, bool>> predicateAll = PredicateBuilder.True<base_SaleOrder>();
                predicateAll = predicateAll.And(x => x.IsConverted && !x.IsVoided && !x.IsPurge && !x.IsLocked).And(predicate);
                if (Define.StoreCode != 0)
                {
                    predicateAll = predicateAll.And(x => x.StoreCode.Equals(Define.StoreCode));
                }

                //Cout all SaleOrder in Data base show on grid
                lock (UnitOfWork.Locker)
                {
                    TotalSaleOrder = _saleOrderRepository.GetIQueryable(predicateAll).Count();

                    //Get data with range
                    IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetRange<DateTime>(currentIndex - _numberNewItem, NumberOfDisplayItems, x => x.OrderDate.Value, predicateAll);

                    foreach (base_SaleOrder saleOrder in saleOrders)
                    {
                        bgWorker.ReportProgress(0, saleOrder);
                    }
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_SaleOrderModel saleOrderModel = new base_SaleOrderModel((base_SaleOrder)e.UserState);
                SetSaleOrderToModel(saleOrderModel);
                SaleOrderCollection.Add(saleOrderModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (SaleOrderId > 0)
                {
                    SetSelectedSaleOrderFromAnother();
                }
                else
                {
                    //Sale Order View is Open & in Edit View
                    if (_viewExisted && !IsSearchMode && SelectedSaleOrder != null && SaleOrderCollection.Any() && !SelectedSaleOrder.IsNew) //Item is selected
                    {
                        SetSelectedSaleOrderFromDbOrCollection();
                    }
                }

                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Create predicate Simple Search Condition
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_SaleOrder, bool>> CreateSimpleSearchPredicate(string keyword)
        {
            //Default Condition is Search All
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                //Set Condition
                predicate = PredicateBuilder.False<base_SaleOrder>();

                //SO Number
                predicate = predicate.Or(x => x.SONumber.ToLower().Contains(keyword.ToLower()));

                //Scan Code
                predicate = predicate.Or(x => x.SOCard.ToLower().Contains(keyword.ToLower()));

                //Status
                IEnumerable<short> statusList = Common.StatusSalesOrders.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                predicate = predicate.Or(x => statusList.Contains(x.OrderStatus));

                //Search Date 
                DateTime orderDate;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out orderDate))
                {
                    int orderYear = orderDate.Year;
                    int orderMonth = orderDate.Month;
                    int orderDay = orderDate.Day;
                    //Order Date
                    predicate = predicate.Or(x => x.OrderDate.HasValue && x.OrderDate.Value.Year == orderYear && x.OrderDate.Value.Month == orderMonth && x.OrderDate.Value.Day == orderDay);
                    //Ship Date
                    predicate = predicate.Or(x => x.RequestShipDate.Year == orderYear && x.RequestShipDate.Month == orderMonth && x.RequestShipDate.Day == orderDay);
                }

                //Search Customer Name
                var customerList = CustomerCollection.Where(y => y.LastName.ToLower().Contains(keyword.ToLower()) || y.FirstName.ToLower().Contains(keyword.ToLower())).Select(x => x.Resource.ToString());
                predicate = predicate.Or(x => customerList.Contains(x.CustomerResource));

                //Search deciaml
                decimal decimalValue = 0;

                if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalValue) && decimalValue != 0)
                {
                    //Total 
                    predicate = predicate.Or(x => x.Total == decimalValue);

                    //Deposit 
                    predicate = predicate.Or(x => x.Deposit == decimalValue);

                    //Balance 
                    predicate = predicate.Or(x => x.Balance.Equals(decimalValue));

                    //Refund 
                    predicate = predicate.Or(x => x.RefundedAmount.Equals(decimalValue));
                }

                //Price Level
                IEnumerable<short> priceSchemaList = Common.PriceSchemas.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => Convert.ToInt16(x.Value));
                predicate = predicate.Or(x => priceSchemaList.Contains(x.PriceSchemaId));

                ///
                ///Search Store 
                ///
                IEnumerable<base_Store> storeList = StoreCollection.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                //Collection Store index , cause StoreCode sale order storage by index
                IList<int> storeIndexList = new List<int>();
                foreach (base_Store item in storeList)
                {
                    int storeIndex = StoreCollection.IndexOf(item);
                    if (!storeIndexList.Any(x => x.Equals(storeIndex)))
                        storeIndexList.Add(storeIndex);
                }
                predicate = predicate.Or(x => storeIndexList.Contains(x.StoreCode));

                //Tax Code
                predicate = predicate.Or(x => x.TaxCode.ToLower().Contains(keyword.ToLower()));

                //Tax Location
                IEnumerable<int> taxLocationList = SaleTaxLocationCollection.Where(x => x.ParentId == 0 && x.Name.ToLower().Contains(keyword.ToLower())).Select(x => x.Id);
                predicate = predicate.Or(x => taxLocationList.Contains(x.TaxLocation));

                //Tax Excemption
                if (Language.GetMsg("SO_TextBlock_TaxExemptionDisplay").ToLower().Contains(keyword.ToLower()))
                {
                    predicate = predicate.Or(x => x.IsTaxExemption);
                }

                //Tax Code Excemption
                predicate = predicate.Or(x => x.TaxExemption.ToLower().Contains(keyword.ToLower()));
            }
            return predicate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        protected override base_SaleOrderModel CreateNewSaleOrder()
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
                _selectedSaleOrder.SaleRep = EmployeeCollection.FirstOrDefault().GuestNo;
                _selectedSaleOrder.Resource = Guid.NewGuid();
                _selectedSaleOrder.WeightUnit = Common.ShipUnits.First().Value;
                _selectedSaleOrder.IsDeposit = Define.CONFIGURATION.AcceptedPaymentMethod.HasValue ? Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(16) : false;//Accept Payment with deposit
                _selectedSaleOrder.WeightUnit = Define.CONFIGURATION.DefaultShipUnit.HasValue ? Define.CONFIGURATION.DefaultShipUnit.Value : Convert.ToInt16(Common.ShipUnits.First().ObjValue);
                _selectedSaleOrder.IsHiddenErrorColumn = true;

                _selectedSaleOrder.TaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);
                _selectedSaleOrder.TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                //Get TaxLocation
                _selectedSaleOrder.TaxLocationModel = SaleTaxLocationCollection.SingleOrDefault(x => x.Id == _selectedSaleOrder.TaxLocation);

                //Create a sale order detail collection
                _selectedSaleOrder.SaleOrderDetailCollection = new CollectionBase<base_SaleOrderDetailModel>();
                _selectedSaleOrder.SaleOrderDetailCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(SaleOrderDetailCollection_CollectionChanged);

                //create a sale order Ship Collection
                _selectedSaleOrder.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();
                _selectedSaleOrder.SaleOrderShippedCollection = new CollectionBase<base_SaleOrderDetailModel>();

                // Create new payment collection
                _selectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                //ReturnModel & ReturnDetailCollection
                _selectedSaleOrder.ReturnModel = new base_ResourceReturnModel();
                _selectedSaleOrder.ReturnModel.DocumentNo = SelectedSaleOrder.SONumber;
                _selectedSaleOrder.ReturnModel.DocumentResource = SelectedSaleOrder.Resource.ToString();
                _selectedSaleOrder.ReturnModel.TotalAmount = SelectedSaleOrder.Total;
                _selectedSaleOrder.ReturnModel.Resource = Guid.NewGuid();
                _selectedSaleOrder.ReturnModel.TotalRefund = 0;
                _selectedSaleOrder.ReturnModel.TotalAmount = 0;
                _selectedSaleOrder.ReturnModel.Mark = "SO";
                _selectedSaleOrder.ReturnModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                _selectedSaleOrder.ReturnModel.DateCreated = DateTime.Today;
                _selectedSaleOrder.ReturnModel.IsDirty = false;
                _selectedSaleOrder.ReturnModel.ReturnDetailCollection = new CollectionBase<base_ResourceReturnDetailModel>();
                _selectedSaleOrder.ReturnModel.ReturnDetailCollection.CollectionChanged += ReturnDetailCollection_CollectionChanged;
                _selectedSaleOrder.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
                //Additional
                _selectedSaleOrder.BillAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Billing, IsDirty = false };
                _selectedSaleOrder.ShipAddressModel = new base_GuestAddressModel() { AddressTypeId = (int)AddressType.Shipping, IsDirty = false };

                //GuestRewardSaleOrder
                _selectedSaleOrder.GuestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel();

                _selectedCustomer = null;
                OnPropertyChanged(() => SelectedCustomer);

                //Set to fist tab & skip TabChanged Methods in SelectedTabIndex property
                _selectedTabIndex = 0;
                OnPropertyChanged(() => SelectedTabIndex);
                SetAllowChangeOrder(_selectedSaleOrder);
                _selectedSaleOrder.IsDirty = false;
                _selectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
                _selectedSaleOrder.ReturnModel.PropertyChanged += new PropertyChangedEventHandler(ReturnModel_PropertyChanged);
                OnPropertyChanged(() => SelectedSaleOrder);
                OnPropertyChanged(() => AllowSOShipping);
                OnPropertyChanged(() => AllowSOReturn);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return _selectedSaleOrder;
        }

        /// <summary>
        /// Get SelectedSaleOrder From collection when Convert from quotation
        /// </summary>
        private void SetSelectedSaleOrderFromAnother()
        {
            try
            {
                if (SaleOrderId > 0)
                {
                    SetSelectedSaleOrderFromDbOrCollection();
                    //Calc Onhand
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                    {
                        if (!saleOrderDetailModel.IsQuantityAccepted)
                        {
                            saleOrderDetailModel.IsQuantityAccepted = true;
                            _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
                        }
                    }

                    SetSelectedCustomer();

                    SetAllowChangeOrder(SelectedSaleOrder);
                    ShowShipTab(SelectedSaleOrder);
                    SelectedSaleOrder.IsDirty = false;

                    //Changed tab
                    _selectedTabIndex = (int)SaleOrderSelectedTab;
                    OnPropertyChanged(() => SelectedTabIndex);
                    _saleOrderId = 0;
                    IsSearchMode = false;
                    IsForceFocused = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load Selected SaleOrder when item is selected with get from db or collection
        /// </summary>
        private void SetSelectedSaleOrderFromDbOrCollection()
        {
            try
            {
                if (SaleOrderCollection.Any(x => x.Id.Equals(SaleOrderId)))
                {
                    _selectedSaleOrder = SaleOrderCollection.SingleOrDefault(x => x.Id.Equals(SaleOrderId));
                }
                else
                {
                    lock (UnitOfWork.Locker)
                    {
                        //If Current SaleOrder loading not yet
                        base_SaleOrder saleOrder = _saleOrderRepository.Get(x => x.Id.Equals(SaleOrderId));
                        if (saleOrder != null)
                        {
                            _selectedSaleOrder = new base_SaleOrderModel(saleOrder);
                            SetSaleOrderToModel(SelectedSaleOrder);
                        }
                    }
                }

                if (SelectedSaleOrder != null)
                {
                    SetSaleOrderRelation(SelectedSaleOrder, true);
                    SetSelectedCustomer();
                    OnPropertyChanged(() => SelectedSaleOrder);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ShowShip Tab
        /// </summary>
        private void SetShippingStatus(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.OrderStatus < (short)SaleOrderStatus.Shipping && saleOrderModel.SaleOrderShipCollection != null && saleOrderModel.SaleOrderShipCollection.Any())
                saleOrderModel.OrderStatus = (short)SaleOrderStatus.Shipping;
        }

        /// <summary>
        /// Set for SaleOrderStatus when order is Ship full
        /// </summary>
        private void SetShipStatus(base_SaleOrderModel saleOrderModel)
        {
            bool ShipAll = saleOrderModel.SaleOrderDetailCollection.Any();
            foreach (var item in saleOrderModel.SaleOrderDetailCollection)
            {
                if (item.ProductModel == null || item.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                    continue;

                decimal shipTotal = saleOrderModel.SaleOrderShipCollection.Where(x => x.IsShipped == true).Sum(x => x.SaleOrderShipDetailCollection.Where(y => y.SaleOrderDetailResource == item.Resource.ToString() && y.ProductResource == item.ProductResource).Sum(z => z.PackedQty));
                ShipAll &= item.Qty > 0 && shipTotal == item.Qty;
            }

            if (!saleOrderModel.Mark.Equals(MarkType.SaleOrder.ToDescription()))//Set Close for layaway
            {
                if (ShipAll)
                {
                    saleOrderModel.OrderStatus = (short)SaleOrderStatus.Close;
                }
            }
            else
            {
                if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull))//Not change status when PaidInFull
                    return;

                if (ShipAll)
                    saleOrderModel.OrderStatus = (short)SaleOrderStatus.FullyShipped;
                else if (saleOrderModel.SaleOrderShipCollection.Any())
                    saleOrderModel.OrderStatus = (short)SaleOrderStatus.Shipping;
            }
        }

        /// <summary>
        /// Check to Show ship tab when has saleorder detail
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void ShowShipTab(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel != null)
            {
                if (saleOrderModel.Mark.Equals(MarkType.Layaway.ToDescription()))
                    saleOrderModel.ShipProcess = saleOrderModel.Balance == 0;
                else
                    saleOrderModel.ShipProcess = (saleOrderModel.SaleOrderDetailCollection != null ? saleOrderModel.SaleOrderDetailCollection.Any() : false) && !saleOrderModel.IsNew;

                OnPropertyChanged(() => AllowSOShipping);
                OnPropertyChanged(() => AllowSOReturn);
            }

        }

        /// <summary>
        /// set user change order follow config & order status
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetAllowChangeOrder(base_SaleOrderModel saleOrderModel)
        {
            if (BreakAllChange)
                return;
            if (saleOrderModel.IsLocked)
                this.IsAllowChangeOrder = false;
            else if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Quote))
                this.IsAllowChangeOrder = true;
            else if (saleOrderModel.PaymentCollection != null && saleOrderModel.PaymentCollection.Any(x => !x.IsDeposit.Value))/*has paid*/
                this.IsAllowChangeOrder = false;
            else if (saleOrderModel.OrderStatus < (short)SaleOrderStatus.FullyShipped)//Open or Shipping
                this.IsAllowChangeOrder = true;
            else if (saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.PaidInFull))
                this.IsAllowChangeOrder = false;
            else
                this.IsAllowChangeOrder = saleOrderModel.OrderStatus == (short)SaleOrderStatus.FullyShipped && Define.CONFIGURATION.IsAllowChangeOrder.Value;

        }

        #region SelectedItem

        /// <summary>
        /// Selected Return Detail Changed
        /// when item is selected,is check collection reference  exited with item choice (compare saleOrderResource)?
        /// unless get item from DeletedItems(used for store item) add to collection shipped(collection autocompelete choice item)
        /// </summary>
        private void SelectedReturnDetailChanged()
        {
            base_ResourceReturnDetailModel selectedReturnDetail = SelectedReturnDetail as base_ResourceReturnDetailModel;
            if (selectedReturnDetail == null || selectedReturnDetail.SaleOrderDetailModel == null)
                return;

            CheckReturned(selectedReturnDetail);
        }


        #endregion

        //CRUD region
        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void InsertSaleOrder(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (SelectedSaleOrder.IsNew)
                {
                    UpdateCustomerAddress(SelectedSaleOrder.BillAddressModel);
                    SelectedSaleOrder.BillAddressId = SelectedSaleOrder.BillAddressModel.Id;
                    UpdateCustomerAddress(SelectedSaleOrder.ShipAddressModel);
                    SelectedSaleOrder.ShipAddressId = SelectedSaleOrder.ShipAddressModel.Id;
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
                    SaleOrderCollection.Insert(0, SelectedSaleOrder);
                    TotalSaleOrder++;
                    ShowShipTab(SelectedSaleOrder);
                    _numberNewItem++;
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
                //Usefull for situation : Order 5 unit after ship 2 unit , change order qty to 2 unit 
                //=> that order is full shipped but not set that current quantity because conflit with condition "Allow Change Order" when full shipping, may be make order disable
                SetShipStatus(saleOrderModel);
                //Insert or update address for customer
                UpdateCustomerAddress(saleOrderModel.BillAddressModel);

                UpdateCustomerAddress(saleOrderModel.ShipAddressModel);

                #region SaleOrderDetail

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
                #endregion

                #region SaleOrderShip
                SaveSaleOrderShipCollection(saleOrderModel);
                #endregion

                #region SaleOrderReturn
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
                #endregion

                #region Payment
                SavePaymentCollection(saleOrderModel);
                #endregion

                #region Commission
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
                ShowShipTab(saleOrderModel);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Void Bill Execute
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void VoidBillProcess(base_SaleOrderModel saleOrderModel)
        {
            saleOrderModel.IsVoided = true;
            saleOrderModel.OrderStatus = (int)SaleOrderStatus.Void;
            SaveSalesOrder(saleOrderModel);
            this.SaleOrderCollection.Remove(saleOrderModel);
            TotalSaleOrder -= 1;
            saleOrderModel = null;

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
        /// Execute payment
        /// </summary>
        private void SaleOrderPayment()
        {
            try
            {
                if (SelectedSaleOrder.IsNew)
                    return;

                bool? resultReward;//True : go to payment process, False : Break process
                bool isPayFull = false;

                //Show Reward Form
                //Need check has any Guest Reward
                //Show Reward only SaleOrder Payment
                #region Check & Apply Reward

                resultReward = ConfirmNApplyReward(ref isPayFull);

                #endregion "Check & Apply Reward"

                if (resultReward == true)
                {
                    SelectedSaleOrder.RewardValueApply = 0;

                    //Calc Subtotal user apply reward
                    if (SelectedSaleOrder.GuestModel.GuestRewardCollection != null && SelectedSaleOrder.GuestModel.GuestRewardCollection.Any())
                    {
                        base_RewardManager rewardProgram = GetReward();

                        base_GuestRewardModel guestRewardModel = SelectedSaleOrder.GuestModel.GuestRewardCollection.FirstOrDefault();
                        guestRewardModel.GuestRewardDetailCollection = new ObservableCollection<base_GuestRewardDetailModel>();

                        //Reward Detail
                        base_GuestRewardDetailModel guestRewardDetailModel = null;
                        if (guestRewardModel != null)
                        {
                            decimal subTotal = 0;
                            if (Define.CONFIGURATION.IsRewardOnTax)//Check reward include tax ?
                                subTotal = SelectedSaleOrder.SubTotal - SelectedSaleOrder.DiscountAmount + SelectedSaleOrder.TaxAmount + SelectedSaleOrder.Shipping;
                            else
                                subTotal = SelectedSaleOrder.SubTotal - SelectedSaleOrder.DiscountAmount + SelectedSaleOrder.Shipping;


                            decimal rewardAmountRemain = guestRewardModel.RewardValueEarned - guestRewardModel.base_GuestReward.base_GuestRewardDetail.Sum(x => x.RewardRedeemed);


                            //Update Subtoal After apply reward redeem && 
                            decimal depositeTotal = SelectedSaleOrder.PaymentCollection != null ? SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid) : 0;
                            if (rewardAmountRemain > subTotal - depositeTotal)
                                SelectedSaleOrder.RewardValueApply = subTotal - depositeTotal;
                            else
                                SelectedSaleOrder.RewardValueApply = rewardAmountRemain;

                            guestRewardDetailModel = new base_GuestRewardDetailModel()
                            {
                                SaleOrderNo = SelectedSaleOrder.SONumber,
                                SaleOrderResource = SelectedSaleOrder.Resource.ToString(),
                                RewardRedeemed = SelectedSaleOrder.RewardValueApply,
                                DateApplied = DateTime.Now,
                                Sign = "-",

                            };


                            //Update Reward Value
                            guestRewardModel.TotalRewardRedeemed += guestRewardDetailModel.RewardRedeemed;

                            //Add Reward Detail To Collection
                            if (guestRewardDetailModel != null)
                            {
                                guestRewardModel.GuestRewardDetailCollection.Add(guestRewardDetailModel);
                            }
                        }
                    }
                    //Update total have to paid
                    if (SelectedSaleOrder.RewardValueApply != 0)
                        SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total - SelectedSaleOrder.RewardValueApply;

                    ///Return Product Proccess

                    //Subtract total of refunded in Return process
                    decimal refunded = SelectedSaleOrder.ReturnModel != null ? SelectedSaleOrder.ReturnModel.TotalRefund : 0;

                    //Handle subtract money when has some product is return
                    decimal returnValue = 0;
                    if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any())
                    {
                        returnValue = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsReturned).Sum(x => x.Amount + x.VAT - x.RewardRedeem - ((x.Amount * SelectedSaleOrder.DiscountPercent) / 100));
                    }

                    ///End Return Product Proccess

                    decimal paidValue = SelectedSaleOrder.PaymentCollection.Sum(x => x.TotalPaid - x.Change);//Incluce deposit
                    decimal balance = SelectedSaleOrder.RewardAmount - returnValue - paidValue;

                    decimal totalDeposit = 0;
                    decimal lastPayment = 0;
                    if (SelectedSaleOrder.PaymentCollection != null)
                    {
                        totalDeposit = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
                        base_ResourcePaymentModel lastPaymentModel = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid > 0).OrderBy(x => x.DateCreated).LastOrDefault();
                        if (lastPaymentModel != null)
                            lastPayment = lastPaymentModel.TotalPaid;
                    }

                    //Show Payment
                    SalesOrderPaymenViewModel paymentViewModel = new SalesOrderPaymenViewModel(SelectedSaleOrder, balance, totalDeposit, lastPayment, isPayFull);
                    bool? dialogResult = _dialogService.ShowDialog<SalesOrderPaymentView>(_ownerViewModel, paymentViewModel, Language.GetMsg("SO_Title_Payment"));
                    if (dialogResult == true)
                    {
                        //Calc Reward , redeem & update subtotal
                        CalcRedeemReward(SelectedSaleOrder);

                        if (Define.CONFIGURATION.DefaultCashiedUserName ?? false)
                            paymentViewModel.PaymentModel.Cashier = Define.USER.LoginName;
                        paymentViewModel.PaymentModel.Shift = Define.ShiftCode;
                        // Add new payment to collection
                        SelectedSaleOrder.PaymentCollection.Add(paymentViewModel.PaymentModel);

                        SelectedSaleOrder.Paid = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid > 0).Sum(x => x.TotalPaid - x.Change);
                        SelectedSaleOrder.CalcBalance();

                        //Full Payment
                        if (SelectedSaleOrder.Paid + SelectedSaleOrder.Deposit.Value >= SelectedSaleOrder.RewardAmount - returnValue)
                        {
                            SaleOrderFullPaymentProcess();
                        }
                    }
                    else
                    {
                        if (SelectedSaleOrder.GuestModel != null //Need for Quotation
                            && SelectedSaleOrder.GuestModel.GuestRewardCollection != null)
                        {
                            //Clear Reward Apply
                            SelectedSaleOrder.GuestModel.GuestRewardCollection.Clear();

                            SelectedSaleOrder.IsRedeeem = false;
                        }
                        SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total;
                    }
                    // Reset reward apply after use
                    SelectedSaleOrder.RewardValueApply = 0;
                }

                SetAllowChangeOrder(SelectedSaleOrder);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Commissions


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

                base_GuestModel employeeModel = EmployeeCollection.SingleOrDefault(x => x.Id == customerModel.SaleRepId);
                if (employeeModel == null)
                    return;

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
                        base_GuestModel managerModel = EmployeeCollection.SingleOrDefault(x => x.Resource.ToString().Equals(mangerCommission.GuestResource));
                        if (managerModel != null)
                        {
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
        /// Calculate Commission for refunded 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalcCommissionForReturn(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                ComboItem item = Common.BookingChannel.SingleOrDefault(x => x.Value == SelectedSaleOrder.BookingChanel);
                if (item.Flag)//True : this booking channel dont use commission
                    return;

                if (saleOrderModel.CommissionCollection == null)
                    saleOrderModel.CommissionCollection = new CollectionBase<base_SaleCommissionModel>();

                Guid customerGuid = Guid.Parse(saleOrderModel.CustomerResource);
                //Get Customer with CustomerResource
                base_GuestModel customerModel = CustomerCollection.Where(x => x.Resource == customerGuid).SingleOrDefault();
                if (customerModel != null && customerModel.SaleRepId.HasValue)
                {
                    base_GuestModel employeeModel = EmployeeCollection.Where(x => x.Id == customerModel.SaleRepId).SingleOrDefault();
                    string remarkReturn = MarkType.SaleOrderReturn.ToDescription();
                    base_SaleCommission saleCommission = _saleCommissionRepository.Get(x => x.Sign.Equals("-") && x.Remark.Equals(remarkReturn) && x.GuestResource == employeeModel.ResourceString && x.SOResource.Equals(saleOrderModel.ResourceString));
                    if (saleCommission == null)
                    {
                        base_SaleCommissionModel newSaleCommission = new base_SaleCommissionModel();
                        newSaleCommission.ComissionPercent = employeeModel.CommissionPercent;
                        newSaleCommission.GuestResource = employeeModel.Resource.ToString();
                        newSaleCommission.Remark = MarkType.SaleOrderReturn.ToDescription();
                        newSaleCommission.Sign = "-";
                        newSaleCommission.SODate = saleOrderModel.OrderDate;
                        newSaleCommission.SONumber = saleOrderModel.SONumber;
                        newSaleCommission.SOResource = saleOrderModel.Resource.ToString();
                        newSaleCommission.SOTotal = saleOrderModel.Paid + saleOrderModel.Deposit;
                        newSaleCommission.CommissionAmount = saleOrderModel.ReturnModel.TotalRefund * newSaleCommission.ComissionPercent / 100;
                        saleOrderModel.CommissionCollection.Add(newSaleCommission);
                    }
                    else
                    {
                        base_SaleCommissionModel UpdateSaleCommission = new base_SaleCommissionModel(saleCommission);
                        UpdateSaleCommission.SOTotal = saleOrderModel.Paid + saleOrderModel.Deposit;
                        UpdateSaleCommission.CommissionAmount = saleOrderModel.ReturnModel.TotalRefund * UpdateSaleCommission.ComissionPercent / 100;
                        if (saleOrderModel.CommissionCollection.Any(x => x.Sign.Equals("-")))
                        {
                            base_SaleCommissionModel updateCommisionModel = saleOrderModel.CommissionCollection.SingleOrDefault(x => x.Sign.Equals("-"));
                            updateCommisionModel = UpdateSaleCommission;
                        }
                        else
                        {
                            saleOrderModel.CommissionCollection.Add(UpdateSaleCommission);
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
        /// Delete Sale Commision of SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void DeleteSaleCommission(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                base_SaleCommission saleCommission = _saleCommissionRepository.GetAll().ToList().SingleOrDefault(x => x.SOResource.Equals(saleOrderModel.Resource.ToString()));
                if (saleCommission != null)
                {
                    _saleCommissionRepository.Delete(saleCommission);
                    _saleCommissionRepository.Commit();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Rewards

        /// <summary>
        /// Get Reward Amount with Member Type
        /// </summary>
        /// <param name="memberType"></param>
        /// <param name="reward"></param>
        /// <returns></returns>
        private decimal GetRewardAmountWithLevel(string memberType, base_RewardManager reward)
        {
            decimal rewardAmount = reward.RewardAmount;

            switch (memberType)
            {
                case "N"://Normal Member
                    rewardAmount = reward.RewardAmount;
                    break;
                case "B"://Brozen Member
                    rewardAmount = reward.L1Amount;
                    break;
                case "S"://Silver Member
                    rewardAmount = reward.L2Amount;
                    break;
                case "G"://Gold Memeber
                    rewardAmount = reward.L3Amount;
                    break;
                case "P"://Platium Member
                    rewardAmount = reward.L4Amount;
                    break;
                case "D"://Diamon Member
                    rewardAmount = reward.L5Amount;
                    break;
                case "O"://Other Member
                    rewardAmount = reward.L6Amount;
                    break;
            }
            return rewardAmount;
        }

        /// <summary>
        /// Using Reward
        /// Method confirm & accept user apply reward when customer payment full
        /// </summary>
        /// <param name="isPayFull">Flag to know user need to paid full & apply reward</param>
        /// <returns></returns>
        private bool? ConfirmNApplyReward(ref bool isPayFull)
        {
            base_RewardManager rewardProgram = GetReward();

            bool? resultReward;
            try
            {
                //Apply reward when customer is a reward member(isRewardMember) && if reward has End Date, after reward program ending collect reward 
                if (SelectedSaleOrder.GuestModel.IsRewardMember && (rewardProgram.IsNoEndDay || (!rewardProgram.IsNoEndDay && rewardProgram.EndDate < SelectedSaleOrder.OrderDate)))
                {
                    bool isRewardOnDiscount = Define.CONFIGURATION.IsRewardOnDiscount ?? false;
                    bool isApplyRewardDiscount = isRewardOnDiscount || (!isRewardOnDiscount && SelectedSaleOrder.DiscountPercent == 0);
                    if (isApplyRewardDiscount && SelectedSaleOrder.PaymentCollection != null
                               && !SelectedSaleOrder.PaymentCollection.Any(x => !x.IsDeposit.Value) /* This order is paid with multi pay*/)
                    {
                        //Confirm User want to Payment Full
                        //msg: You have some rewards, you need to pay fully and use these rewards. Do you?
                        MessageBoxResult confirmPayFull = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_NotifyPayfullUseReward"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                        if (confirmPayFull.Equals(MessageBoxResult.Yes))//User Payment full
                        {
                            isPayFull = true;
                            int ViewActionType;

                            VerifyRedeemRewardViewModel verifyRedeemRewardViewModel = new VerifyRedeemRewardViewModel(SelectedSaleOrder);
                            resultReward = _dialogService.ShowDialog<ConfirmMemberRedeemRewardView>(_ownerViewModel, verifyRedeemRewardViewModel, Language.GetMsg("SO_Message_RedeemReward") + "Validate reward Code");
                            ViewActionType = (int)verifyRedeemRewardViewModel.ViewActionType;

                            if (resultReward == true)
                            {
                                if (ViewActionType == (int)VerifyRedeemRewardViewModel.ReeedemRewardType.Redeemded)
                                {
                                    SelectedSaleOrder.RewardValueApply = 0;
                                    isPayFull = false;
                                }
                                else
                                    SelectedSaleOrder.IsRedeeem = true;//Customer used reward
                            }
                        }
                        else
                        {
                            //Customer don't want to apply reward & full payment 
                            isPayFull = false;
                            //proccess can open Payment Popup but not need to full payment
                            resultReward = true;
                        }
                    }
                    else
                    {
                        //User has payment
                        resultReward = true;
                    }
                }
                else
                {
                    //Customer is not a Reward member
                    resultReward = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }

            return resultReward;
        }

        /// <summary>
        /// Calculate Reward apply for guest
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="saleOrderModel"></param>
        private void CalcRedeemReward(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (!saleOrderModel.GuestModel.IsRewardMember)
                    return;
                //Calc Subtotal user apply reward
                if (saleOrderModel.GuestModel.GuestRewardCollection.Any())
                {
                    base_GuestRewardModel guestRewardModel = saleOrderModel.GuestModel.GuestRewardCollection.FirstOrDefault();
                    guestRewardModel.AppliedDate = DateTime.Today;

                    guestRewardModel.CalculateRewardBalance();

                    //Reward Is Empty => Set Status
                    if (guestRewardModel.RewardBalance == 0)
                    {
                        guestRewardModel.IsApply = true;
                        guestRewardModel.Status = (short)GuestRewardStatus.Redeemed;
                    }
                    //Set Total Reward Redeemed
                    saleOrderModel.GuestModel.TotalRewardRedeemed += guestRewardModel.TotalRewardRedeemed;

                    //Update Reward Redeemed to Reward manager
                    base_RewardManager rewardManager = _rewardManagerRepository.Get(x => x.Id.Equals(guestRewardModel.RewardId));
                    if (rewardManager != null)
                        rewardManager.TotalRewardRedeemed += guestRewardModel.TotalRewardRedeemed;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Create New Reward for customer
        /// <remarks>Create Reward when Reward Program Actived & Order Date in StartDate & EndDate Or No EndDate</remarks>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CreateNewReward(base_SaleOrderModel saleOrderModel, decimal totalProductReward)
        {
            try
            {
                if (!saleOrderModel.GuestModel.IsRewardMember)
                    return;

                var reward = GetReward();

                //Reward Amount calculate for Member with member Type(Brozen,Gold,Silver..)
                decimal rewardAmount = 0;
                int totalOfReward = 0;
                if (reward != null && reward.Status.Is(StatusBasic.Active) && saleOrderModel.GuestModel.MembershipValidated != null && (reward.IsNoEndDay || (!reward.IsNoEndDay && reward.EndDate >= SelectedSaleOrder.OrderDate)))
                {
                    //Check if not set Purchase Threshold
                    if (saleOrderModel.GuestModel.RequirePurchaseNextReward == 0)
                        saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;

                    if (reward.RewardType.Equals(0))//Standard Program Type
                    {
                        rewardAmount = reward.RewardAmount;
                    }
                    else //Tier Program Type
                    {
                        rewardAmount = GetRewardAmountWithLevel(saleOrderModel.GuestModel.MembershipValidated.MemberType, reward);
                    }

                    //No Cut Off :Current SaleOrder with amount of product reward  is more than PurchaseThreshold, customer will be receviced a reward
                    if (reward.CutOffType.Is(CutOffType.NoCutOff)) //If totalProductReward of current SaleOrder >=  reward.PurchaseThreshold => Create Reward
                    {
                        if (reward.IsIncrementalWithNoCutOff)
                        {
                            //Total Product Reward purchased
                            decimal totalOfPurchase = totalProductReward + (reward.PurchaseThreshold - saleOrderModel.GuestModel.RequirePurchaseNextReward);
                            if (totalOfPurchase >= reward.PurchaseThreshold)
                            {
                                totalOfReward = Convert.ToInt32(Math.Truncate(totalOfPurchase / reward.PurchaseThreshold));
                                //Create 1 Reward with total of reward amount
                                decimal rewardSetupAmount = totalOfReward * rewardAmount;

                                //Create new Reward
                                base_GuestRewardModel guestRewardModel = CreateNewGuestReward(saleOrderModel, reward, rewardSetupAmount);


                                //Add To Collection History Create Reward
                                if (saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                                {
                                    //Add To Collection History Create Reward
                                    base_GuestRewardSaleOrderModel guestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel
                                    {
                                        SaleOrderResource = saleOrderModel.Resource.ToString(),
                                        SaleOrderNo = saleOrderModel.SONumber,
                                        SaleAmount = totalProductReward
                                    };
                                    saleOrderModel.GuestRewardSaleOrderModel = guestRewardSaleOrderModel;
                                }

                                if (reward.RewardAmtType.Is(RewardType.Money))
                                {
                                    saleOrderModel.GuestRewardSaleOrderModel.CashRewardEarned = guestRewardModel.RewardValueEarned;
                                }
                                else
                                {
                                    saleOrderModel.GuestRewardSaleOrderModel.PointRewardEarned = guestRewardModel.RewardValueEarned;
                                }

                                //Update ref to Save GuestRewardId
                                saleOrderModel.GuestRewardSaleOrderModel.RewardRef = guestRewardModel.GetHashCode().ToString();

                                //Add to Temp Collection to Insert to db
                                saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Add(guestRewardModel);
                            }

                            //Calculate Require Purchase Next Reward
                            //A is PurchaseDuringTrackingPeriod
                            //P is PurchaseThreshold
                            //R is RequirePurchaseNextReward
                            //R = P - (A/P % 2 * P)

                            saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold - ((saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold) % 1) * reward.PurchaseThreshold;

                        }
                        else //Noe Cut Off with none accumulation
                        {
                            //Total PurchaseThreshold is enoungh to create reward & Order in Start & EndDate.
                            if (totalProductReward >= reward.PurchaseThreshold)
                            {
                                //Total Reward customer received
                                totalOfReward = Convert.ToInt32(Math.Truncate(totalProductReward / reward.PurchaseThreshold));

                                //Create 1 Reward with total of reward amount
                                decimal rewardSetupAmount = totalOfReward * rewardAmount;

                                //Create new Reward
                                base_GuestRewardModel guestRewardModel = CreateNewGuestReward(saleOrderModel, reward, rewardSetupAmount);


                                if (saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                                {
                                    //Add To Collection History Create Reward
                                    base_GuestRewardSaleOrderModel guestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel
                                    {
                                        SaleOrderResource = saleOrderModel.Resource.ToString(),
                                        SaleOrderNo = saleOrderModel.SONumber,
                                        SaleAmount = totalProductReward
                                    };
                                    saleOrderModel.GuestRewardSaleOrderModel = guestRewardSaleOrderModel;
                                }

                                //Update Value
                                if (reward.RewardAmtType.Is(RewardType.Money))
                                {
                                    saleOrderModel.GuestRewardSaleOrderModel.CashRewardEarned = guestRewardModel.RewardValueEarned;
                                }
                                else
                                {
                                    saleOrderModel.GuestRewardSaleOrderModel.PointRewardEarned = guestRewardModel.RewardValueEarned;
                                }

                                //Update ref to Save GuestRewardId
                                saleOrderModel.GuestRewardSaleOrderModel.RewardRef = guestRewardModel.GetHashCode().ToString();

                                //Add to Temp Collection to Insert to db
                                saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Add(guestRewardModel);
                            }
                            //Require next reward is Purchase Threshold
                            saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold;
                        }
                    }
                    else //Cash Point & Date => Need to Sum TotalCashReward
                    {
                        //Total Product Reward purchased
                        decimal totalOfPurchase = totalProductReward + (reward.PurchaseThreshold - saleOrderModel.GuestModel.RequirePurchaseNextReward);
                        totalOfReward = Convert.ToInt32(Math.Truncate(totalOfPurchase / reward.PurchaseThreshold));

                        //Add To Collection History Create Reward
                        if (saleOrderModel.GuestRewardSaleOrderModel.IsNew)
                        {
                            //Add To Collection History Create Reward
                            base_GuestRewardSaleOrderModel guestRewardSaleOrderModel = new base_GuestRewardSaleOrderModel
                            {
                                SaleOrderResource = saleOrderModel.Resource.ToString(),
                                SaleOrderNo = saleOrderModel.SONumber,
                                SaleAmount = totalProductReward
                            };
                            saleOrderModel.GuestRewardSaleOrderModel = guestRewardSaleOrderModel;
                        }


                        if (reward.RewardAmtType.Is(RewardType.Money))
                        {
                            saleOrderModel.GuestRewardSaleOrderModel.CashRewardEarned = totalOfReward * rewardAmount;
                        }
                        else
                        {
                            saleOrderModel.GuestRewardSaleOrderModel.PointRewardEarned = totalOfReward * rewardAmount;
                        }


                        //Calculate Require Purchase Next Reward
                        //A is PurchaseDuringTrackingPeriod
                        //P is PurchaseThreshold
                        //R is RequirePurchaseNextReward
                        //R = P - (A/P % 2 * P)

                        saleOrderModel.GuestModel.RequirePurchaseNextReward = reward.PurchaseThreshold - ((saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold) % 1) * reward.PurchaseThreshold;
                    }


                    //Notify to Cashier about reward customer earned ? 
                    if (reward.IsInformCashier)
                    {

                        if (saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Any())
                        {
                            string rewardProgram = string.Empty;
                            //Total Of Reward customer received
                            int totalRewardReceived = saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Count();

                            base_GuestRewardModel guestRewardReceived = saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.FirstOrDefault();

                            string msgExpireDate = guestRewardReceived.ExpireDate.HasValue ? guestRewardReceived.ExpireDate.Value.ToString(Define.DateFormat) : Language.GetMsg("SO_Message_RewardNeverExpired");
                            if (reward.RewardAmtType.Equals((int)RewardType.Money))
                            {
                                rewardProgram = string.Format(Language.GetMsg("SO_Message_RewardAmount"), string.Format(Define.ConverterCulture, Define.CurrencyFormat, guestRewardReceived.RewardValueEarned));

                                //Msg : You are received : {0} reward(s) {1}  \nExpire Date : {2}
                                Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.GetMsg("SO_Message_ReceivedReward").ToString().Replace("\\n", "\n"), totalRewardReceived, rewardProgram, msgExpireDate), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                            }
                            else if (reward.RewardAmtType.Equals((int)RewardType.Point))
                            {
                                rewardProgram = string.Format(Language.GetMsg("SO_Message_RewardAmount"), string.Format(Define.ConverterCulture, Define.DecimalFormat, guestRewardReceived.RewardValueEarned));
                                //Msg : You are received : {0} reward(s) {1} point  \nExpire Date : {2}
                                decimal pointReward = rewardAmount * totalOfReward;
                                Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.GetMsg("SO_Message_ReceivedPointReward").ToString().Replace("\\n", "\n"), totalRewardReceived, rewardProgram, msgExpireDate), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                            }

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
        /// Create New Guest Reward
        /// <para>Release reward by Cash</para>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="reward"></param>
        /// <returns></returns>
        private base_GuestRewardModel CreateNewGuestReward(base_SaleOrderModel saleOrderModel, base_RewardManager reward, decimal rewardSetupAmounts)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                base_GuestRewardModel guestRewardModel = new base_GuestRewardModel();
                guestRewardModel.EarnedDate = DateTime.Today;
                guestRewardModel.IsApply = false;
                guestRewardModel.RewardId = reward.Id;
                guestRewardModel.GuestId = saleOrderModel.GuestModel.Id;
                guestRewardModel.Remark = string.Empty;

                //Reward Earned by Cash
                if (reward.RewardAmtType.Is(RewardType.Money))
                {
                    guestRewardModel.RewardValueEarned = rewardSetupAmounts;
                }
                else
                {
                    decimal pointToMoney = rewardSetupAmounts * reward.DollarConverter / reward.PointConverter;
                    guestRewardModel.RewardValueEarned = pointToMoney;
                }


                //Rewward create with another reward
                guestRewardModel.RewardValueApplied = saleOrderModel.Total - saleOrderModel.RewardAmount;

                currentDate = currentDate.AddSeconds(1);
                string idCard = currentDate.ToString("yyMMddHHmmss");
                string scancode = string.Empty;
                byte[] codeImage = null;
                if (BarCodeGenerate(idCard, out scancode, out codeImage))
                {
                    guestRewardModel.ScanCode = scancode;
                    guestRewardModel.ScanCodeImg = codeImage;
                }

                //Set Block reward redeemption for ??? days after earned
                if (reward.IsBlockRedemption && reward.RedemptionAfterDays > 0)
                {
                    guestRewardModel.Status = (short)GuestRewardStatus.Pending;
                    guestRewardModel.ActivedDate = guestRewardModel.EarnedDate.Value.AddDays(reward.RedemptionAfterDays);
                }
                else
                {
                    guestRewardModel.Status = (int)GuestRewardStatus.Available;
                    guestRewardModel.ActivedDate = guestRewardModel.EarnedDate.Value;
                }
                //Set Expired Date For Reward
                if (reward.RewardExpiration != 0)//RewardExpiration =0 (Never Expired)
                {
                    int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == reward.RewardExpiration).Detail);
                    guestRewardModel.ExpireDate = guestRewardModel.ActivedDate.Value.AddDays(expireDay);
                }
                else
                {
                    guestRewardModel.ExpireDate = null;
                }
                guestRewardModel.CalculateRewardBalance();
                return guestRewardModel;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }

        }

        /// <summary>
        /// Calc Return Reward for item returned
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="returnDetailModel"></param>
        private void CalcReturnDetailRewardRedeem(base_SaleOrderModel saleOrderModel, base_ResourceReturnDetailModel returnDetailModel)
        {
            if (returnDetailModel.SaleOrderDetailModel != null
                && saleOrderModel.Total != saleOrderModel.RewardAmount
                && saleOrderModel.RewardAmount > 0)//Has Apply Reward
            {
                decimal total = SelectedSaleOrder.SaleOrderDetailCollection.Sum(x => x.SubTotal);// saleOrderModel.Total
                decimal rewardApply = saleOrderModel.Total - saleOrderModel.RewardAmount;
                //Calculate reward redeem with amount include tax
                decimal rewardRedeem = Math.Round(Math.Round((returnDetailModel.Amount * rewardApply) / total, Define.CONFIGURATION.DecimalPlaces.Value) - 0.01M, MidpointRounding.AwayFromZero);
                returnDetailModel.RewardRedeem = rewardRedeem;
            }
        }

        /// <summary>
        /// Caculate Cash of reward returnModel
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalcRewardReturn(base_SaleOrderModel saleOrderModel)
        {
            var reward = GetReward();

            //Check reward is existed & if reward is no endDate => check reward is redeem,otherwise orderdate is out of enddate
            if (reward != null && (reward.IsNoEndDay || (!reward.IsNoEndDay && reward.EndDate <= saleOrderModel.OrderDate.Value)))
            {
                //Amount Of product is Eligible reward returned
                decimal productReturnRewardAmount = 0;
                //Total Reward After Return 
                decimal totalRewardAfterReturn = saleOrderModel.GuestModel.PurchaseDuringTrackingPeriod / reward.PurchaseThreshold;

                foreach (base_ResourceReturnDetailModel returnDetailModel in saleOrderModel.ReturnModel.ReturnDetailCollection.Where(x => x.SaleOrderDetailModel.ProductModel.IsEligibleForReward))
                {
                    if (returnDetailModel.SaleOrderDetailModel.ProductModel != null && !returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                    {
                        //New Item
                        if (returnDetailModel.IsReturned && !returnDetailModel.base_ResourceReturnDetail.IsReturned)
                            productReturnRewardAmount += returnDetailModel.Amount;// + returnDetailModel.VAT
                    }
                }

                //Handle Return Reward For reward Member
                if (saleOrderModel.GuestModel.IsRewardMember)
                {
                    CustomerReturnReward(saleOrderModel, reward, productReturnRewardAmount, totalRewardAfterReturn);
                }
            }
        }

        /// <summary>
        /// This methods handle return reward if this SaleOrder Has Create reward in purchasing process
        /// <para>Case 1: these rewards is not apply another sale order=> Remove reward base on return product amount</para>
        /// <para>Case 2: these rewards is apply another sale order=> Remove reward base on return product amount &
        /// if return reward amount more than reward existed, remove some rewads & transfer to cash to customer return to store</para>
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="productReturnRewardAmount"></param>
        /// <param name="totalRewardAfterReturn"></param>
        /// <param name="isCalcTemp">Not delete reward & update value to membership</param>
        private void CustomerReturnReward(base_SaleOrderModel saleOrderModel, base_RewardManager reward, decimal productReturnRewardAmount, decimal totalRewardAfterReturn, bool isCalcTemp = true)
        {
            try
            {
                if (reward != null && saleOrderModel.Balance == 0)
                {
                    string saleOrderResource = saleOrderModel.Resource.ToString();
                    decimal rewardAmount = 0;
                    if (reward.RewardType.Equals(0))//Standard Program Type
                    {
                        rewardAmount = reward.RewardAmount;
                    }
                    else //Tier Program Type
                    {
                        rewardAmount = GetRewardAmountWithLevel(saleOrderModel.GuestModel.MembershipValidated.MemberType, reward);
                    }

                    base_GuestRewardModel guestRewardModel;

                    if (reward.CutOffType.Is(CutOffType.NoCutOff))
                    {
                        #region NoCutOff
                        base_GuestRewardSaleOrder guestRewardSaleOrder = _guestRewardSaleOrderRepository.Get(x => x.SaleOrderResource.Equals(saleOrderResource));
                        if (guestRewardSaleOrder != null)
                        {
                            base_GuestReward guestRewardUpdate = saleOrderModel.GuestModel.base_Guest.base_GuestReward.SingleOrDefault(x => x.Id.Equals(guestRewardSaleOrder.GuestRewardId));
                            if (guestRewardUpdate != null)
                            {
                                guestRewardModel = new base_GuestRewardModel(guestRewardUpdate);

                                if (guestRewardModel.Id > 0)
                                {
                                    if (reward.IsIncrementalWithNoCutOff)
                                    {
                                        // reward is returned base on quantity of product returned
                                        decimal rewardReturn = (productReturnRewardAmount / reward.PurchaseThreshold);

                                        decimal amountRewardAddToAnother = 0;
                                        if ((rewardReturn % 1) > 0)
                                            amountRewardAddToAnother = reward.PurchaseThreshold - (rewardReturn % 1);

                                        //Total Return is ($Reward Earn) - ($RewardReturn) - (Reward redeem(used))
                                        guestRewardModel.TotalRewardReturned = guestRewardModel.RewardValueEarned - (rewardReturn * rewardAmount) - guestRewardModel.TotalRewardRedeemed - amountRewardAddToAnother;
                                        if (guestRewardModel.TotalRewardReturned < 0)
                                            guestRewardModel.TotalRewardReturned *= -1;

                                        //Calculate balance
                                        guestRewardModel.CalculateRewardBalance();
                                    }
                                    else //With Not Incremental
                                    {
                                        if (guestRewardModel != null)
                                        {
                                            decimal totalRewardProductReturn = productReturnRewardAmount + (guestRewardModel.TotalRewardReturned * rewardAmount);
                                            //Total Reward customer received
                                            decimal amountRewardReceived = guestRewardModel.RewardValueEarned - (Convert.ToInt32(Math.Truncate(totalRewardProductReturn / reward.PurchaseThreshold)) * rewardAmount);
                                            //total Reward Return is Reward before Return subtract for reward received currently
                                            guestRewardModel.TotalRewardReturned = guestRewardModel.TotalRewardRedeemed - amountRewardReceived;
                                            //Calculate balance
                                            guestRewardModel.CalculateRewardBalance();
                                        }
                                    }

                                    //Update Value & Calculate Customer Need to Reward return by cash
                                    if (guestRewardModel != null)
                                    {
                                        //Update Status
                                        if (guestRewardModel.RewardBalance == 0)
                                        {
                                            guestRewardModel.Status = (short)GuestRewardStatus.Removed;
                                        }

                                        //Customer is using reward more than amount of reward customer received after returned
                                        if (guestRewardModel.RewardBalance < 0)
                                        {
                                            saleOrderModel.ReturnModel.Redeemed = (guestRewardModel.RewardBalance * -1); //Sum amount of cash customer need to pay to store
                                        }

                                        //Add Or Update To Reward Collection Delete to Update value for Guest Reward

                                        //Remove Item Existed
                                        base_GuestRewardModel rewardUpdate = saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.SingleOrDefault(x => x.Id.Equals(guestRewardModel.Id));
                                        if (rewardUpdate != null)
                                            saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Remove(rewardUpdate);
                                        //Add To Collection
                                        saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Add(guestRewardModel);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else //Cash Point && Cut Off Date
                    {
                        #region Cash Point & Cut Off
                        short avaliableStatus = (short)GuestRewardStatus.Available;
                        short PendingStatus = (short)GuestRewardStatus.Pending;
                        decimal totalRewardReturn = productReturnRewardAmount / reward.PurchaseThreshold;
                        decimal rewardReturnAmount = totalRewardReturn * rewardAmount;
                        DateTime lastPayment = saleOrderModel.PaymentCollection.OrderBy(x => x.DateCreated).LastOrDefault().DateCreated;

                        //Get Guest reward Sale Order to know guestReward

                        if (saleOrderModel.GuestRewardSaleOrderModel != null)
                        {
                            //Get Guest Reward existed?
                            base_GuestReward guestReward = saleOrderModel.GuestModel.base_Guest.base_GuestReward.SingleOrDefault(x => x.Id.Equals(saleOrderModel.GuestRewardSaleOrderModel.GuestRewardId) && (x.Status.Equals(avaliableStatus) || x.Status.Equals(PendingStatus)));
                            if (guestReward != null)
                            {
                                guestRewardModel = new base_GuestRewardModel(guestReward);

                                if (guestRewardModel != null && guestRewardModel.Id > 0)//Return Reward by subtract which existed
                                {

                                    decimal returnRemain = 0;

                                    if (guestRewardModel.RewardBalance >= rewardReturnAmount)
                                    {
                                        //subtract All Return Reward Amount
                                        returnRemain = rewardReturnAmount;
                                    }
                                    else
                                    {
                                        //Subtract all guest reward existed
                                        returnRemain = guestReward.RewardBalance;
                                    }
                                    //Update return
                                    guestRewardModel.TotalRewardReturned += returnRemain;
                                    //Calculate balance
                                    guestRewardModel.CalculateRewardBalance();

                                    rewardReturnAmount -= returnRemain;

                                    //Set Status Guest Reward
                                    if (guestRewardModel.RewardBalance == 0)
                                    {
                                        guestRewardModel.Status = (short)GuestRewardStatus.Removed;
                                    }

                                    //Remove Item Existed
                                    base_GuestRewardModel rewardUpdate = saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.SingleOrDefault(x => x.Id.Equals(guestRewardModel.Id));
                                    if (rewardUpdate != null)
                                        saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Remove(rewardUpdate);
                                    //Add To Collection
                                    saleOrderModel.GuestModel.GuestRewardCollection.DeletedItems.Add(guestRewardModel);
                                }

                                //remain reward return need to paid by cash to Store if ()
                                if (rewardReturnAmount > 0)
                                {
                                    //Customer is using reward more than amount of reward customer received after returned
                                    saleOrderModel.ReturnModel.Redeemed = rewardReturnAmount; //Sum amount of cash customer need to pay to store
                                }
                            }
                        }
                        #endregion
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

        #region "Calculate Tax"

        /// <summary>
        /// Apply Tax
        /// </summary>
        private decimal CalculateReturnTax(base_ResourceReturnModel returnModel, base_SaleOrderModel saleOrderModel)
        {
            decimal result = 0;
            try
            {
                if (saleOrderModel.TaxLocationModel != null && saleOrderModel.TaxLocationModel.TaxCodeModel != null)
                {
                    if (saleOrderModel.IsTaxExemption)
                    {
                        result = 0;
                    }
                    else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Multi))
                    {
                        saleOrderModel.TaxPercent = 0;

                        foreach (base_ResourceReturnDetailModel returnDetailModel in returnModel.ReturnDetailCollection.Where(x => x.IsReturned))
                        {
                            if (returnDetailModel.SaleOrderDetailModel.ProductModel != null && !returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)//18/06/2013: not calculate tax for coupon
                                result += _saleOrderRepository.CalcMultiTaxForItem(saleOrderModel.TaxLocationModel.SaleTaxLocationOptionCollection, returnDetailModel.Amount, returnDetailModel.SaleOrderDetailModel.SalePrice);
                        }
                    }
                    else if (Convert.ToInt32(saleOrderModel.TaxLocationModel.TaxCodeModel.TaxOption).Is((int)SalesTaxOption.Price))
                    {
                        saleOrderModel.TaxPercent = 0;
                        base_SaleTaxLocationOptionModel saleTaxLocationOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                        foreach (base_ResourceReturnDetailModel returnDetailModel in returnModel.ReturnDetailCollection.Where(x => x.IsReturned))
                        {
                            if (returnDetailModel.SaleOrderDetailModel.ProductModel != null && !returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                                result += _saleOrderRepository.CalcPriceDependentItem(returnDetailModel.Amount, returnDetailModel.SaleOrderDetailModel.SalePrice, saleTaxLocationOptionModel);
                        }
                    }
                    else
                    {


                        base_SaleTaxLocationOptionModel taxOptionModel = saleOrderModel.TaxLocationModel.TaxCodeModel.SaleTaxLocationOptionCollection.FirstOrDefault();
                        if (taxOptionModel != null)
                        {
                            foreach (base_ResourceReturnDetailModel returnDetailModel in returnModel.ReturnDetailCollection.Where(x => x.IsReturned))
                            {
                                result += returnDetailModel.Amount * taxOptionModel.TaxRate / 100;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }
        #endregion

        #region Return Proccess
        /// <summary>
        /// Return All 
        /// Set all item is shipped to return collection
        /// If exited item in return collection and item is not set returned, it will be added quantity. Otherwise create new item to return collection 
        /// </summary>
        private void ReturnAll()
        {
            try
            {
                if (SelectedSaleOrder.SaleOrderShipDetailCollection != null)
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection)
                    {
                        if (SelectedSaleOrder.SaleOrderShipDetailCollection.Any(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())))
                        {
                            base_ResourceReturnDetailModel returnDetailModel = new base_ResourceReturnDetailModel();
                            returnDetailModel.SaleOrderDetailModel = saleOrderDetailModel;
                            returnDetailModel.OrderDetailResource = saleOrderDetailModel.Resource.ToString();
                            returnDetailModel.SaleOrderModel = SelectedSaleOrder;
                            returnDetailModel.IsParent = (returnDetailModel.SaleOrderDetailModel.ProductModel != null && returnDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group));
                            CalculateRemainReturnQty(returnDetailModel, true);

                            //Gift Card Is Used
                            if (returnDetailModel.SaleOrderDetailModel.ProductModel != null && returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon && saleOrderDetailModel.CouponCardModel.InitialAmount > saleOrderDetailModel.CouponCardModel.RemainingAmount)
                            {
                                continue;
                            }

                            if (returnDetailModel.ReturnQty > 0)
                            {
                                returnDetailModel.ProductResource = saleOrderDetailModel.ProductResource;
                                returnDetailModel.ItemCode = saleOrderDetailModel.ItemCode;
                                returnDetailModel.ItemName = saleOrderDetailModel.ItemName;
                                returnDetailModel.ItemAtribute = saleOrderDetailModel.ItemAtribute;
                                returnDetailModel.ItemSize = saleOrderDetailModel.ItemSize;
                                returnDetailModel.UnitName = saleOrderDetailModel.UnitName;
                                returnDetailModel.Price = saleOrderDetailModel.SalePrice;
                                returnDetailModel.Amount = returnDetailModel.Price * returnDetailModel.ReturnQty;
                                CalcReturnQtyBaseUnit(returnDetailModel, returnDetailModel.SaleOrderDetailModel);
                                returnDetailModel.IsTemporary = false;
                                //Existed item not return & the same of SaleOrderDetailResource=>update Return Qty
                                if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsReturned && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Any())
                                {
                                    base_ResourceReturnDetailModel returnDetailModelUpdate = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.SingleOrDefault(x => !x.IsReturned && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource));
                                    returnDetailModelUpdate.ReturnQty += returnDetailModel.ReturnQty;
                                }
                                else
                                {


                                    SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Add(returnDetailModel);
                                    returnDetailModel.IsTemporary = false;
                                }

                                returnDetailModel.VAT = _saleOrderRepository.CalculateReturnDetailTax(returnDetailModel, SelectedSaleOrder);
                                CalcReturnDetailRewardRedeem(SelectedSaleOrder, returnDetailModel);
                                CalcReturnDetailSubTotal(SelectedSaleOrder, returnDetailModel);
                            }
                        }
                    }
                    CalculateReturnSubtotal(SelectedSaleOrder);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check item is return all. 
        /// if item is return all, remove collection shipped to not show in autocomplete choice Product
        /// </summary>
        private void CheckReturned()
        {
            try
            {
                if (SelectedSaleOrder == null)
                    return;
                var allReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.SaleOrderDetailModel != null);


                foreach (var item in allReturn)
                {
                    decimal totalReturn = allReturn.Where(x => x.OrderDetailResource.Equals(item.OrderDetailResource)).Sum(x => x.ReturnQty);
                    decimal totalShipped = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(item.OrderDetailResource)).Sum(x => x.PickQty);
                    totalShipped += SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Where(x => x.Resource.ToString().Equals(item.OrderDetailResource)).Sum(x => x.PickQty);
                    if (totalShipped <= totalReturn)
                    {
                        base_SaleOrderDetailModel saleOrderShippedModel = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(item.OrderDetailResource));
                        if (saleOrderShippedModel != null)
                            SelectedSaleOrder.SaleOrderShippedCollection.Remove(saleOrderShippedModel);
                    }
                    else
                    {
                        base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(item.OrderDetailResource));
                        if (saleOrderShippedRemoved != null)
                        {
                            //add To CollectionShipped
                            SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                            //Remove In Collection DeletedItems
                            SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
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
        /// Check Item Is Return All
        /// </summary>
        /// <param name="selectedReturnDetail"></param>
        private void CheckReturned(base_ResourceReturnDetailModel selectedReturnDetail)
        {
            try
            {
                if (SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Any(x => x.OrderDetailResource.Equals(selectedReturnDetail.OrderDetailResource)))
                {
                    base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(selectedReturnDetail.OrderDetailResource));
                    if (saleOrderShippedRemoved != null)
                    {
                        SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                        SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
                    }
                    //Remove Item Returned All
                    //Get Item Diffrent with Current Item Selected
                    var saleOrderShipped = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => !x.Resource.ToString().Equals(selectedReturnDetail.OrderDetailResource));
                    foreach (base_SaleOrderDetailModel saleOrderShippedModel in saleOrderShipped.ToList())
                    {
                        decimal totalReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.SaleOrderDetailModel != null && x.SaleOrderDetailModel.Resource.Equals(saleOrderShippedModel.Resource)).Sum(x => x.ReturnQty);
                        decimal totalShipped = saleOrderShippedModel.PickQty;
                        totalShipped += SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Where(x => x.Resource.Equals(saleOrderShippedModel.Resource)).Sum(x => x.PickQty);
                        if (totalShipped <= totalReturn)
                        {
                            SelectedSaleOrder.SaleOrderShippedCollection.Remove(saleOrderShippedModel);
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
        /// Calculate Remain Return Quantity
        /// </summary>
        /// <param name="returnDetailModel"></param>
        /// <param name="IsCalcAll">false : Calculate quantity is returned not include current item</param>
        private void CalculateRemainReturnQty(base_ResourceReturnDetailModel returnDetailModel, bool IsCalcAll = false)
        {
            try
            {
                decimal TotalItemReturn = 0;

                if (IsCalcAll)
                    TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
                else
                    TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.Resource.Equals(returnDetailModel.Resource) && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
                decimal remainQuantity = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource)).Sum(x => Convert.ToDecimal(x.PickQty)) - TotalItemReturn;
                returnDetailModel.ReturnQty = remainQuantity;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Calculate Subtotal of Return
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void CalculateReturnSubtotal(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.ReturnModel != null && saleOrderModel.ReturnModel.ReturnDetailCollection.Any())
                {
                    //saleOrderModel.ReturnModel.SubTotal = saleOrderModel.ReturnModel.ReturnDetailCollection.Sum(x => x.Amount);
                    decimal subtotal = saleOrderModel.ReturnModel.ReturnDetailCollection.Sum(x => x.Amount + x.VAT - x.RewardRedeem - ((x.Amount * saleOrderModel.DiscountPercent) / 100));
                    int decimalPlace = Define.CONFIGURATION.DecimalPlaces ?? 0;
                    saleOrderModel.ReturnModel.SubTotal = subtotal;// Math.Round(Math.Round(subtotal, decimalPlace) - 0.01M, MidpointRounding.AwayFromZero);
                }
                else
                    saleOrderModel.ReturnModel.SubTotal = 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Return Detail Subtotal = Amount + VAT - rewardReedem - Discount(Order)
        /// </summary>
        /// <param name="returnDetailModel"></param>
        private void CalcReturnDetailSubTotal(base_SaleOrderModel saleOrderModel, base_ResourceReturnDetailModel returnDetailModel)
        {
            decimal subtotal = returnDetailModel.Amount + returnDetailModel.VAT - returnDetailModel.RewardRedeem - ((returnDetailModel.Amount * saleOrderModel.DiscountPercent) / 100);
            int decimalPlace = Define.CONFIGURATION.DecimalPlaces ?? 0;
            returnDetailModel.SubTotalDetail = subtotal;//Math.Round(Math.Round(subtotal, decimalPlace) - 0.01M, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculate return quantity base on baseUnit
        /// </summary>
        /// <param name="resourceReturnDetailModel"></param>
        /// <param name="saleOrderDetail"></param>
        private void CalcReturnQtyBaseUnit(base_ResourceReturnDetailModel resourceReturnDetailModel, base_SaleOrderDetailModel saleOrderDetail)
        {
            //Get BaseUnit & convert value to Qty to BaseUnit for ReturnQtyUOM

            base_ProductUOMModel productUomModel = null;
            if (saleOrderDetail.ProductUOMCollection != null)
                productUomModel = saleOrderDetail.ProductUOMCollection.SingleOrDefault(x => saleOrderDetail.ProductModel != null && !saleOrderDetail.ProductModel.IsCoupon && x.UOMId.Equals(saleOrderDetail.UOMId));
            if (productUomModel != null)
            {
                decimal quantityBaseUnit = productUomModel.BaseUnitNumber * resourceReturnDetailModel.ReturnQty;
                //Update To ReturnQtyUOM
                resourceReturnDetailModel.ReturnQtyUOM = quantityBaseUnit;
            }
            else
                resourceReturnDetailModel.ReturnQtyUOM = resourceReturnDetailModel.ReturnQty;
        }
        #endregion

        /// <summary>
        /// Methods execute when Order is payment fully
        /// </summary>
        private void SaleOrderFullPaymentProcess()
        {
            try
            {
                if (SelectedSaleOrder.GuestModel.IsRewardMember && SelectedSaleOrder.GuestModel.MembershipValidated != null)//Only for Reward Member & validated is a membership
                {
                    decimal totalProductReward = 0;
                    if (SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.ProductModel.IsEligibleForReward))
                    {
                        foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel.IsEligibleForReward))
                        {
                            //Quantity of return after Payment
                            decimal returnQty = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => x.IsReturned && x.OrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.ReturnQty);

                            //Amount of order detail include returnqty
                            decimal amountOrderDetail = (saleOrderDetailModel.Quantity - returnQty) * saleOrderDetailModel.SalePrice;

                            //Tax collected on SaleOrderDetail
                            decimal orderDetailTax = _saleOrderRepository.CalculateSaleOrderDetailTax(saleOrderDetailModel, SelectedSaleOrder, amountOrderDetail);

                            totalProductReward += (amountOrderDetail);// + orderDetailTax);
                        }
                    }
                    //PurchaseDuringTrackingPeriod is a total product reward is purchased
                    SelectedSaleOrder.GuestModel.PurchaseDuringTrackingPeriod += totalProductReward;

                    //Check IsCalRewardAfterRedeem Config
                    bool isRewardApplied = SelectedSaleOrder.IsRedeeem;
                    if (Define.CONFIGURATION.IsCalRewardAfterRedeem //Calc reward anyway
                        || (!Define.CONFIGURATION.IsCalRewardAfterRedeem && !isRewardApplied))//calc new reward when so not apply redeem
                    {
                        CreateNewReward(SelectedSaleOrder, totalProductReward);
                    }
                }

                //Update Card Manager IsSold if existed
                if (SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.ProductModel != null && x.ProductModel.IsCoupon))
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ProductModel.IsCoupon))
                    {
                        saleOrderDetailModel.CouponCardModel.IsSold = true;
                        //Turn isdirty on for saleOrderDetail
                        saleOrderDetailModel.IsDirty = true;
                    }
                }

                if (SelectedSaleOrder.Mark.Equals(MarkType.SaleOrder.ToDescription()))
                {
                    SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.PaidInFull;
                }
                else
                {
                    SelectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Close;//Set status to close when SO convert from Layaway/workorder/Quote
                }

                //Calculate & create commission for Employee
                SaveSaleCommission(SelectedSaleOrder);

                SaveSalesOrder(SelectedSaleOrder);

                //Clear Guest Collection After Save
                if (SelectedSaleOrder.GuestModel.IsRewardMember && SelectedSaleOrder.GuestModel.GuestRewardCollection != null)
                    SelectedSaleOrder.GuestModel.GuestRewardCollection.Clear();

                SendEmailToCustomer();

                this.IsSearchMode = true;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

            }
        }

        /// <summary>
        /// Store Changed
        /// </summary>
        private void StoreChanged()
        {
            try
            {
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in this.SelectedSaleOrder.SaleOrderDetailCollection)
                {
                    SetPriceUOM(saleOrderDetailModel);

                    CalculateDiscount(saleOrderDetailModel);

                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);

                    saleOrderDetailModel.CalcSubTotal();

                    saleOrderDetailModel.CalcDueQty();

                    saleOrderDetailModel.CalUnfill();
                }
                SelectedSaleOrder.CalcSubTotal();
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
        /// <param name="param"></param>
        private void TabChanged(int saleTab)
        {
            if (this.SelectedSaleOrder == null)
                return;
            //if (!IsDirty)
            //    return;
            bool allowChangeTab = true;
            switch (saleTab)
            {
                case (int)SaleOrderTab.Order:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                        allowChangeTab = false;
                    break;
                case (int)SaleOrderTab.Ship:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                    {
                        allowChangeTab = false;
                    }
                    else
                        if (SelectedSaleOrder.SaleOrderDetailCollection.Any(x => x.IsDirty) && _previousTabIndex.Is(SaleOrderTab.Order))//Change from SaleOrderTab
                        {
                            if (IsValid & IsOrderValid)
                                SaveSalesOrder(SelectedSaleOrder);
                        }
                    break;
                case (int)SaleOrderTab.Payment:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                    {
                        allowChangeTab = false;
                    }
                    break;
                case (int)SaleOrderTab.Return:
                    if (!IsOrderValid || !IsReturnValid || !IsShipValid)
                    {
                        allowChangeTab = false;
                    }
                    break;
            }

            if (!allowChangeTab)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //msg: notify fix error
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M107"), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    _selectedTabIndex = _previousTabIndex;
                    OnPropertyChanged(() => SelectedTabIndex);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }


        }

        /// <summary>
        /// Shipped Proccess 
        /// User click to shipped
        /// </summary>
        /// <param name="param"></param>
        private void ShippedProcess(object param)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                //msg : "Do you want to ship?"
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ConfirmShipItem"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                base_SaleOrderShipModel saleOrderShipModel = param as base_SaleOrderShipModel;
                if (result.Is(MessageBoxResult.Yes))
                {

                    saleOrderShipModel.IsShipped = saleOrderShipModel.IsChecked;

                    SelectedSaleOrder.ShippedBox = Convert.ToInt16(SelectedSaleOrder.SaleOrderShipCollection.Count(x => x.IsShipped));

                    SelectedSaleOrder.RaiseAnyShipped();

                    SetShipStatus(SelectedSaleOrder);

                    if (SelectedSaleOrder.PaymentCollection == null)
                        SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                    //Set Referrence value Refund fee from config
                    if (Define.CONFIGURATION.IsIncludeReturnFee || (SelectedSaleOrder.ReturnModel.ReturnFeePercent == 0 && SelectedSaleOrder.ReturnModel.ReturnFee == 0))
                        SelectedSaleOrder.ReturnModel.ReturnFeePercent = Define.CONFIGURATION.ReturnFeePercent;

                    foreach (base_SaleOrderShipDetailModel saleOrderShipDetailModel in saleOrderShipModel.SaleOrderShipDetailCollection)
                    {
                        saleOrderShipDetailModel.SaleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                        base_SaleOrderShipDetailModel saleOrderShipClone = saleOrderShipDetailModel.Clone();
                        saleOrderShipClone.SaleOrderDetailModel = saleOrderShipDetailModel.SaleOrderDetailModel;

                        SelectedSaleOrder.SaleOrderShipDetailCollection.Add(saleOrderShipClone);

                        //Set for return Collection
                        //Existed item SaleOrderShippedDetail in Shipped Collection
                        if (SelectedSaleOrder.SaleOrderShippedCollection.Any(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource))
                            || SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Any(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource)))
                        {
                            base_SaleOrderDetailModel saleOrderDetailModel = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                            if (saleOrderDetailModel != null)
                            {
                                saleOrderDetailModel.PickQty = SelectedSaleOrder.SaleOrderShipDetailCollection.Where(x => x.SaleOrderDetailResource.Equals(saleOrderDetailModel.Resource.ToString())).Sum(x => x.PackedQty);
                                saleOrderDetailModel.SubTotal = saleOrderDetailModel.PickQty * saleOrderDetailModel.SalePrice;
                            }
                            else
                            {
                                base_SaleOrderDetailModel saleOrderShippedRemoved = SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource));
                                if (saleOrderShippedRemoved != null)
                                {
                                    SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderShippedRemoved);
                                    SelectedSaleOrder.SaleOrderShippedCollection.DeletedItems.Remove(saleOrderShippedRemoved);
                                }
                            }
                        }
                        else
                        {
                            base_SaleOrderDetailModel saleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderShipDetailModel.SaleOrderDetailResource)).Clone();
                            saleOrderDetailModel.PickQty = saleOrderShipDetailModel.PackedQty;
                            saleOrderDetailModel.SubTotal = saleOrderDetailModel.PickQty * saleOrderDetailModel.SalePrice;
                            SelectedSaleOrder.SaleOrderShippedCollection.Add(saleOrderDetailModel);
                        }

                        //lock quantity Combobox when item is shipped
                        Guid saleOrderShipDetailResource = Guid.Parse(saleOrderShipDetailModel.SaleOrderDetailResource);
                        base_SaleOrderDetailModel lockUOM = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.Equals(saleOrderShipDetailResource));
                        if (lockUOM != null && !lockUOM.IsReadOnlyUOM)
                        {
                            lockUOM.IsReadOnlyUOM = true;
                        }

                    }
                    _saleOrderRepository.UpdateQtyOrderNRelate(SelectedSaleOrder);
                    SelectedSaleOrder.SetFullPayment();
                    //Save SaleOrder After Shipped
                    UpdateSaleOrder(SelectedSaleOrder);
                    _productRepository.Commit();
                    //Lock Order if any item is shipped
                    SetAllowChangeOrder(SelectedSaleOrder);
                    UnitOfWork.CommitTransaction();
                }
                else
                {

                    saleOrderShipModel.IsChecked = false;
                    saleOrderShipModel.IsShipped = false;
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

            }
        }

        /// <summary>
        /// Update Pick quatity for parent when Child of Product Group Changed qty of pick pack
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void UpdatePickQtyForParent(base_SaleOrderDetailModel saleOrderDetailModel)
        {
            if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))//ChildOf ProductGroup
            {
                //Get Parent Item for update
                base_SaleOrderDetailModel parentSaleOrderDetailModel = SelectedSaleOrder.SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.ToString().Equals(saleOrderDetailModel.ParentResource));
                var childGroupList = SelectedSaleOrder.SaleOrderDetailCollection.Where(x => x.ParentResource.Equals(saleOrderDetailModel.ParentResource));
                decimal totalQty = childGroupList.Sum(x => x.Quantity);
                decimal totalOfPick = childGroupList.Sum(x => x.PickQty);
                decimal parentPickQty = totalQty == 0 ? 0 : totalOfPick * parentSaleOrderDetailModel.Quantity / totalQty;
                parentSaleOrderDetailModel.PickQty = Math.Round(parentPickQty, 2);
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

        //Handle from another form
        /// <summary>
        /// Open Sale Order or Quotation Advance Search
        /// </summary>
        private void OpenSOAdvanceSearch()
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();
            _salesOrderAdvanceSearchViewModel.CustomerCollection = this.CustomerCollection.ToList();
            _salesOrderAdvanceSearchViewModel.LoadData("SaleOrder");
            bool? dialogResult = _dialogService.ShowDialog<SalesOrderAdvanceSearchView>(_ownerViewModel, _salesOrderAdvanceSearchViewModel, Language.GetMsg("C104"));
            if (dialogResult == true)
            {
                IsAdvanced = true;
                _predicate = _salesOrderAdvanceSearchViewModel.SearchAdvancePredicate;

                SaleOrderCollection.Clear();
                LoadDataByPredicate(_predicate, false, 0);

                //_saleOrderBgWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Method Check config accept send email to customer
        /// </summary>
        private void SendEmailToCustomer()
        {

            //Send Email To Customer
            if (Define.CONFIGURATION.IsSendEmailCustomer && !string.IsNullOrWhiteSpace(SelectedSaleOrder.GuestModel.Email))
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_ConfirmSendEmailToCustomer"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result.Equals(MessageBoxResult.Yes))
                {
                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += (sender, e) =>
                    {
                        IsBusy = true;
                        ReportViewModel rtp = new ReportViewModel(null, "rptSODetails", "", "Receipt", SelectedSaleOrder);
                    };
                    bg.RunWorkerCompleted += (sender, e) =>
                    {
                        IsBusy = false;
                    };
                    bg.RunWorkerAsync();
                }
            }

        }

        /// <summary>
        /// Open Popup create new gift card to return to customer
        /// <remarks>This method only handle create & add to collection storeCard, will saved in click save saleOrder</remarks>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="returnDetailModel"></param>
        private bool OpenCardCreation(base_SaleOrderModel saleOrderModel, base_ResourceReturnDetailModel returnDetailModel)
        {
            try
            {
                GiftCardCreationViewModel viewModel = new GiftCardCreationViewModel();
                viewModel.Amount = returnDetailModel.Amount;
                bool? dialogResult = _dialogService.ShowDialog<GiftCardCreationView>(_ownerViewModel, viewModel, "Gift Card Creation");
                if (dialogResult ?? false)
                {
                    //Update Customer Purchase & CustomerGifted
                    viewModel.StoreCardModel.GuestResourcePurchased = saleOrderModel.GuestModel.Resource.ToString();
                    viewModel.StoreCardModel.GuestGiftedResource = saleOrderModel.GuestModel.Resource.ToString();
                    viewModel.StoreCardModel.PurchaseDate = DateTime.Now;
                    returnDetailModel.StoreCardNo = viewModel.StoreCardModel.CardNumber;
                    saleOrderModel.ReturnModel.StoreCardCollection.Add(viewModel.StoreCardModel);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }


        /// <summary>
        /// Generate Barcode For Reward
        /// </summary>
        /// <param name="code"></param>
        /// <param name="barCodeId"></param>
        /// <param name="barCodeImage"></param>
        private bool BarCodeGenerate(string code, out string barCodeId, out byte[] barCodeImage)
        {
            barCodeId = string.Empty;
            barCodeImage = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    using (BarcodeLib.Barcode barCode = new BarcodeLib.Barcode())
                    {
                        barCode.IncludeLabel = true;
                        barCode.Encode(Define.CONFIGURATION.DefaultScanMethodType, code, 200, 70);
                        barCodeImage = barCode.Encoded_Image_Bytes;
                        barCodeId = barCode.RawData;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }


        #endregion

        #region Propertychanged
        private void SelectedSaleOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_SaleOrderModel saleOrderModel = sender as base_SaleOrderModel;
            switch (e.PropertyName)
            {
                case "SONumber":
                    CheckDuplicateSoNum(saleOrderModel);
                    break;
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
                    SetAllowChangeOrder(saleOrderModel);
                    saleOrderModel.SetFullPayment();

                    //Set Text Status
                    saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                    break;
                case "StoreCode":
                    StoreChanged();
                    break;
                case "TotalPaid":
                    saleOrderModel.ReturnModel.CalcBalance(saleOrderModel.TotalPaid);
                    break;
                //case "BookingChanel":
                //    SetSaleTaxLocationForSaleOrder(saleOrderModel);
                //    break;




            }
        }

        protected override void SaleOrderDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BreakAllChange || BreakSODetailChange)
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
                                UpdatePickQtyForParent(saleOrderDetaiInGrouplModel);
                                if (saleOrderDetaiInGrouplModel.ProductModel.IsSerialTracking)
                                    OpenTrackingSerialNumber(saleOrderDetaiInGrouplModel, true, true);
                            }
                        }
                    }
                    else//Child of Product Group Change Quanity
                        UpdatePickQtyForParent(saleOrderDetailModel);
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
                    UpdatePickQtyForParent(saleOrderDetailModel);

                    saleOrderDetailModel.CalcDueQty();
                    break;
                case "UOMId":
                    SetPriceUOM(saleOrderDetailModel);

                    //BreakSODetailChange = true;
                    ////Calculate Discount with exited discount after that
                    //_saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    //BreakSODetailChange = false;
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

        protected override void SaleOrderDetailCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        private void SelectedSaleOrderShip_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BoxNo":
                    SelectedSaleOrder.RaiseTotalShipBox();
                    break;
                case "Weight":
                    SelectedSaleOrder.RaiseTotalWeight();
                    break;
            }

        }

        private void ResourceReturnDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourceReturnDetailModel returnDetailModel = sender as base_ResourceReturnDetailModel;
            switch (e.PropertyName)
            {
                case "SaleOrderDetailModel":
                    if (returnDetailModel.SaleOrderDetailModel != null)
                    {
                        if (string.IsNullOrWhiteSpace(returnDetailModel.OrderDetailResource) || !returnDetailModel.SaleOrderDetailModel.ProductResource.Equals(returnDetailModel.ProductResource))
                        {
                            returnDetailModel.OrderDetailResource = returnDetailModel.SaleOrderDetailModel.Resource.ToString();
                            returnDetailModel.SaleOrderModel = SelectedSaleOrder;
                            returnDetailModel.ProductResource = returnDetailModel.SaleOrderDetailModel.ProductResource;
                            returnDetailModel.ItemCode = returnDetailModel.SaleOrderDetailModel.ItemCode;
                            returnDetailModel.ItemName = returnDetailModel.SaleOrderDetailModel.ItemName;
                            returnDetailModel.ItemAtribute = returnDetailModel.SaleOrderDetailModel.ItemAtribute;
                            returnDetailModel.ItemSize = returnDetailModel.SaleOrderDetailModel.ItemSize;
                            returnDetailModel.UnitName = returnDetailModel.SaleOrderDetailModel.UnitName;
                            returnDetailModel.Price = returnDetailModel.SaleOrderDetailModel.SalePrice;
                            //Product is Parent of goup not change quantity when return
                            returnDetailModel.IsParent = (returnDetailModel.SaleOrderDetailModel.ProductModel != null && returnDetailModel.SaleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group));

                            CalculateRemainReturnQty(returnDetailModel);

                            if (returnDetailModel.SaleOrderDetailModel.ProductModel != null
                                && returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                            {
                                base_CardManagement cardManagement = _cardManagementRepository.Get(x => x.CardNumber.Equals(returnDetailModel.SaleOrderDetailModel.SerialTracking.Trim()));
                                if (cardManagement != null)
                                    returnDetailModel.IsCardUsed = cardManagement.InitialAmount > cardManagement.RemainingAmount;

                            }

                        }
                    }
                    else
                    {
                        returnDetailModel.OrderDetailResource = null;
                        returnDetailModel.ProductResource = null;
                        returnDetailModel.ItemCode = null;
                        returnDetailModel.ItemName = null;
                        returnDetailModel.ItemAtribute = null;
                        returnDetailModel.ItemSize = null;
                        returnDetailModel.Price = 0;
                        returnDetailModel.ReturnQty = 0;
                    }
                    break;
                case "Price":
                    returnDetailModel.Amount = returnDetailModel.Price * returnDetailModel.ReturnQty;
                    break;
                case "ReturnQty":
                    //resourceReturnDetailModel.SaleOrderDetailModel.ProductModel.
                    returnDetailModel.Amount = returnDetailModel.Price * returnDetailModel.ReturnQty;
                    base_SaleOrderDetailModel saleOrderDetail = SelectedSaleOrder.SaleOrderShippedCollection.SingleOrDefault(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource));
                    if (saleOrderDetail != null && SelectedSaleOrder != null)
                    {
                        decimal TotalItemReturn = SelectedSaleOrder.ReturnModel.ReturnDetailCollection.Where(x => !x.IsTemporary && x.OrderDetailResource.Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.ReturnQty);
                        var remainQuantity = SelectedSaleOrder.SaleOrderShippedCollection.Where(x => x.Resource.ToString().Equals(returnDetailModel.OrderDetailResource)).Sum(x => x.PickQty) - TotalItemReturn;
                        saleOrderDetail.QtyAfterRerturn = remainQuantity;

                        CalcReturnQtyBaseUnit(returnDetailModel, saleOrderDetail);
                        CheckReturned(returnDetailModel);
                    }
                    break;

                case "Amount":
                    returnDetailModel.VAT = _saleOrderRepository.CalculateReturnDetailTax(returnDetailModel, SelectedSaleOrder);
                    CalcReturnDetailRewardRedeem(SelectedSaleOrder, returnDetailModel);
                    CalcReturnDetailSubTotal(SelectedSaleOrder, returnDetailModel);
                    CalculateReturnSubtotal(SelectedSaleOrder);
                    break;

                case "IsReturned":
                    if (returnDetailModel.IsReturned)
                    {
                        if (!returnDetailModel.HasError)
                        {
                            //"Are you sure you return this item ?"
                            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M110"), Language.GetMsg("POSCaption"), MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);
                            if (result == MessageBoxResult.None || result == MessageBoxResult.Cancel)
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    returnDetailModel.IsReturned = false;
                                }), System.Windows.Threading.DispatcherPriority.Normal);
                            }
                            else
                            {
                                if (returnDetailModel.SaleOrderDetailModel.ProductModel.IsCoupon)
                                {
                                    short giftCardPayment = (short)PaymentMethod.GiftCard;
                                    short certificatePayment = (short)PaymentMethod.GiftCertificate;

                                    bool hasPaymentWitGiftcard = SelectedSaleOrder.PaymentCollection.Any(x => x.PaymentDetailCollection.Any(y => y.PaymentMethodId.Equals(giftCardPayment) || y.PaymentMethodId.Equals(certificatePayment)));

                                    //Has Payment by gift card or gift Certification
                                    //When Customer has payment with gift card, customer have to refund by store card(another giftcard)
                                    if (hasPaymentWitGiftcard)
                                    {
                                        bool forceCreateGiftCard = OpenCardCreation(SelectedSaleOrder, returnDetailModel);
                                        if (!forceCreateGiftCard)
                                        {
                                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                returnDetailModel.IsReturned = false;
                                            }), System.Windows.Threading.DispatcherPriority.Background);
                                        }
                                    }
                                    else
                                    {
                                        ////Not payment by gift card need to confirm customer want to refunded by gift card
                                        ReturnMethodViewModel returnMethodViewModel = new ReturnMethodViewModel();
                                        bool? resultReturn = _dialogService.ShowDialog<ReturnMethodView>(_ownerViewModel, returnMethodViewModel, Language.GetMsg("POSCaption"));

                                        if (resultReturn ?? false)
                                        {
                                            if (returnMethodViewModel.IsStoreCard)
                                            {
                                                if (!OpenCardCreation(SelectedSaleOrder, returnDetailModel)) //Cancel Create storeCard
                                                {
                                                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                                    {
                                                        returnDetailModel.IsReturned = false;
                                                    }), System.Windows.Threading.DispatcherPriority.Background);
                                                }
                                            }
                                        }
                                        else //Customer click cancel reset checked 
                                        {
                                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                returnDetailModel.IsReturned = false;
                                            }), System.Windows.Threading.DispatcherPriority.Background);
                                        }
                                    }
                                }
                                else
                                {
                                    CalcRewardReturn(SelectedSaleOrder);
                                }
                            }
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                returnDetailModel.IsReturned = false;

                                Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M111"), Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                        SelectedSaleOrder.ReturnModel.RaiseRefundAccepted();
                    }
                    break;

            }
        }

        private void ReturnModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ResourceReturnModel returnModel = sender as base_ResourceReturnModel;
            switch (e.PropertyName)
            {
                case "TotalRefund":
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "Redeemed":
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "SubTotal":
                    returnModel.CalcReturnFee();
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "ReturnFee":
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
                case "ReturnFeePercent":
                    returnModel.CalcBalance(SelectedSaleOrder.TotalPaid);
                    break;
            }
        }

        private void ReturnDetailCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base_ResourceReturnDetailModel resourceReturnDetail;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.ReturnedDate = DateTime.Now;
                    resourceReturnDetail.Resource = Guid.NewGuid();
                    resourceReturnDetail.IsTemporary = true;
                    resourceReturnDetail.IsDirty = false;
                    resourceReturnDetail.PropertyChanged += ResourceReturnDetailModel_PropertyChanged;
                }
                CheckReturned();
                SelectedSaleOrder.ReturnModel.RaiseRefundAccepted();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    resourceReturnDetail = item as base_ResourceReturnDetailModel;
                    resourceReturnDetail.PropertyChanged -= ResourceReturnDetailModel_PropertyChanged;
                }
                //CheckReturned();
            }
        }

        #endregion

        #region Events
        private void _saleOrderBgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Define.DisplayLoading)
                IsBusy = true;

            Expression<Func<base_SaleOrder, bool>> predicateAll = PredicateBuilder.True<base_SaleOrder>();
            predicateAll = predicateAll.And(x => x.IsConverted && !x.IsVoided && !x.IsPurge && !x.IsLocked).And(_predicate);
            if (Define.StoreCode != 0)
            {
                predicateAll = predicateAll.And(x => x.StoreCode.Equals(Define.StoreCode));
            }

            //Cout all SaleOrder in Data base show on grid
            lock (UnitOfWork.Locker)
            {
                TotalSaleOrder = _saleOrderRepository.GetIQueryable(predicateAll).Count();

                //Get data with range
                IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetRange<DateTime>(SaleOrderCollection.Count() - _numberNewItem, NumberOfDisplayItems, x => x.OrderDate.Value, predicateAll);

                foreach (base_SaleOrder saleOrder in saleOrders)
                {
                    //_saleOrderBgWorker.ReportProgress(0, saleOrder);
                }
            }
        }

        private void _saleOrderBgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            base_SaleOrderModel saleOrderModel = new base_SaleOrderModel((base_SaleOrder)e.UserState);
            SetSaleOrderToModel(saleOrderModel);
            SaleOrderCollection.Add(saleOrderModel);
        }

        private void _saleOrderBgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (SaleOrderId > 0)
            {
                SetSelectedSaleOrderFromAnother();
            }
            else
            {
                //Sale Order View is Open & in Edit View
                if (_viewExisted && !IsSearchMode && SelectedSaleOrder != null && SaleOrderCollection.Any() && !SelectedSaleOrder.IsNew) //Item is selected
                {
                    SetSelectedSaleOrderFromDbOrCollection();
                }
            }

            IsBusy = false;
        }

        #endregion

        #region Override Methods

        public override void LoadData()
        {
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += (sender, e) =>
            {
                IsBusy = true;
                _numberNewItem = 0;
                //Flag When Existed view Call LoadDynamicData Data
                if (_viewExisted)
                {
                    LoadDynamicData();
                    _salesOrderAdvanceSearchViewModel.ResetKeyword();
                }

                // Get delete product in sale order permission
                AllowDeleteProduct = UserPermissions.AllowDeleteProductSalesOrder;

                _viewExisted = true;
            };

            bg.RunWorkerCompleted += (sender, e) =>
            {
                IsBusy = false;
                this._saleOrderCollection.Clear();
                _predicate = PredicateBuilder.True<base_SaleOrder>();
                if (!string.IsNullOrWhiteSpace(Keyword))//Load with Search Condition
                    _predicate = CreateSimpleSearchPredicate(Keyword); // CreatePredicateWithConditionSearch(Keyword);

                LoadDataByPredicate(_predicate);

                //_saleOrderBgWorker.RunWorkerAsync();

            };
            bg.RunWorkerAsync();
        }

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            if (IsBusy)
                return false;
            return ChangeViewExecute(isClosing);
        }

        /// <summary>
        /// Change view from Ribbon
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ChangeViewExecute(null))
                {
                    if (isList)
                    {
                        IsSearchMode = true;
                    }
                    else
                    {
                        CreateNewSaleOrder();
                        IsSearchMode = false;
                        IsForceFocused = true;
                    }
                }
            }
            else
            {
                if (param is ComboItem)
                {

                    //Currently form is be called from another form, bellow methods get param & set Id to temparate variable(SaleOrderId).
                    //if param has not isChecked(form is Actived), form with be load again. after form loaded, set SelectedSaleOrder item base one temp variable(SaleOrderId)
                    //Otherwise,LoadData method won't be loaded, need to set selectedSaleOrder after recived value

                    SaleOrderSelectedTab = SaleOrderTab.Order;
                    ComboItem cmbValue = param as ComboItem;
                    if (cmbValue.Text.Equals("Quotation") || cmbValue.Text.Equals(MarkType.WorkOrder.ToDescription()))
                    {
                        IsSearchMode = false;
                        SaleOrderId = Convert.ToInt32(cmbValue.Detail);
                        _selectedSaleOrder = null;
                    }
                    else if (cmbValue.Text.Equals("UnLock"))
                    {
                        SaleOrderId = Convert.ToInt32(cmbValue.Detail);
                        OnPropertyChanged(() => SelectedSaleOrder);
                        IsSearchMode = false;
                    }
                    else if (cmbValue.Text.Equals("Customer"))//Create SaleOrder With Customer
                    {
                        CreateNewSaleOrder();
                        long customerId = Convert.ToInt64(cmbValue.Detail);
                        SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Id.Equals(customerId));
                        this.IsSearchMode = false;
                    }
                    else if (cmbValue.Text.Equals("SaleOrderReturn.New"))
                    {
                        CreateNewSaleOrder();
                        this.IsSearchMode = false;
                        SaleOrderSelectedTab = SaleOrderTab.Return;
                    }
                    else if (cmbValue.Text.Equals("SaleOrderReturn.SaleOrderList"))
                    {
                        this.IsSearchMode = true;
                    }
                    else if (cmbValue.Text.Equals("SaleOrderReturn.SelectedItem"))
                    {
                        SaleOrderId = Convert.ToInt32(cmbValue.Detail);
                        _selectedSaleOrder = null;
                        this.IsSearchMode = false;
                        SaleOrderSelectedTab = SaleOrderTab.Return;
                    }

                    if (cmbValue.IsChecked)//Form is Actived after open from another form(useful for current form called from popup form)
                    {
                        if (SaleOrderId > 0)
                        {
                            SetSelectedSaleOrderFromAnother();
                        }
                        else
                        {
                            //Sale Order View is Open & in Edit View
                            if (_viewExisted && !IsSearchMode && SelectedSaleOrder != null && SaleOrderCollection.Any() && !SelectedSaleOrder.IsNew) //Item is selected
                            {
                                SetSelectedSaleOrderFromDbOrCollection();
                            }
                        }
                    }

                }
                else //Create saleOrder with ProductCollection
                {
                    CreateNewSaleOrder();
                    this.IsSearchMode = false;
                    IEnumerable<base_ProductModel> productCollection = param as IEnumerable<base_ProductModel>;
                    CreateSaleOrderDetailWithProducts(productCollection);

                }
            }
        }

        protected override void SelectedSaleOrderChanged()
        {
            base.SelectedSaleOrderChanged();
            if (SelectedSaleOrder != null)
            {
                SelectedSaleOrder.PropertyChanged -= new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
                SelectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
            }
        }


        #endregion

        #region Permission

        #region Properties

        /// <summary>
        /// Gets the AllowSOShipping.
        /// </summary>
        public bool AllowSOShipping
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return UserPermissions.AllowSalesOrderShipping;
                return UserPermissions.AllowSalesOrderShipping && SelectedSaleOrder.ShipProcess;
            }
        }

        /// <summary>
        /// Gets the AllowSOReturn.
        /// </summary>
        public bool AllowSOReturn
        {
            get
            {
                if (SelectedSaleOrder == null)
                    return UserPermissions.AllowSalesOrderReturn;
                return UserPermissions.AllowSalesOrderReturn && SelectedSaleOrder.ShipProcess;
            }
        }

        #endregion

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if (dropInfo.Data is ComboItem)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Sales Order", dropInfo.Data);
            }
            else if (dropInfo.Data is ComboItem)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Sales Order", dropInfo.Data);
            }
        }

        #endregion
    }
}