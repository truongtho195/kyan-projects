using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using CPC.DragDrop;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExt.DataGridControl;

namespace CPC.POS.ViewModel
{
    /// <summary>
    /// Using for Layaway
    /// </summary>
    public class LayawayViewModel : OrderViewModel, IDropTarget
    {
        #region Define

        private base_ProductStoreRepository _productStoreRespository = new base_ProductStoreRepository();
        private base_LayawayManagerRepository _layawayManagerRepository = new base_LayawayManagerRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();

        private SalesOrderAdvanceSearchViewModel _salesOrderAdvanceSearchViewModel = new SalesOrderAdvanceSearchViewModel();
        private bool IsAdvanced { get; set; }
        private string LayawayInfo;
        #endregion

        #region Constructors

        public LayawayViewModel(bool isList, object param)
            : base()
        {
            LoadDynamicData();

            ChangeSearchMode(isList, param);

            // Get delete product in layaway permission
            AllowDeleteProduct = UserPermissions.AllowDeleteProductLayaway;
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
                    || (SelectedSaleOrder.PaymentCollection != null && SelectedSaleOrder.PaymentCollection.Any(x => x.IsDirty))
                    || (SelectedSaleOrder.BillAddressModel != null && SelectedSaleOrder.BillAddressModel.IsDirty)
                    || (SelectedSaleOrder.ShipAddressModel != null && SelectedSaleOrder.ShipAddressModel.IsDirty);
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

        //FilterText will be stored to use for Load Step(load previous Fitler text that after user change)
        public string Keyword { get; set; }

        #endregion

        #region LayawayManagerCollection
        private List<base_LayawayManager> _layawayManagerCollection;
        /// <summary>
        /// Gets or sets the LayawayManagerCollection.
        /// </summary>
        public List<base_LayawayManager> LayawayManagerCollection
        {
            get { return _layawayManagerCollection; }
            set
            {
                if (_layawayManagerCollection != value)
                {
                    _layawayManagerCollection = value;
                    OnPropertyChanged(() => LayawayManagerCollection);
                }
            }
        }
        #endregion

        #region IsLayawayManagerValid
        private bool _isLayawayManagerValid;
        /// <summary>
        /// Gets or sets the IsLayawayManagerValid.
        /// </summary>
        public bool IsLayawayManagerValid
        {
            get { return _isLayawayManagerValid; }
            set
            {
                if (_isLayawayManagerValid != value)
                {
                    _isLayawayManagerValid = value;
                    OnPropertyChanged(() => IsLayawayManagerValid);
                }

            }
        }
        #endregion

        #region LayawayManagerModel
        private base_LayawayManagerModel _layawayManagerModel;
        /// <summary>
        /// Gets or sets the LayawayManagerModel.
        /// </summary>
        public base_LayawayManagerModel LayawayManagerModel
        {
            get { return _layawayManagerModel; }
            set
            {
                if (_layawayManagerModel != value)
                {
                    _layawayManagerModel = value;
                    OnPropertyChanged(() => LayawayManagerModel);
                }
            }
        }
        #endregion


        //Static Property

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

        #endregion

        #region Commands Methods

        #region NewCommand
        protected override bool OnNewCommandCanExecute(object param)
        {
            return base.OnNewCommandCanExecute(param) && UserPermissions.AllowAddLayaway;
        }
        protected override void OnNewCommandExecute(object param)
        {
            base.OnNewCommandExecute(param);

            if (ChangeViewExecute(null))
            {
                CreateNewLayawayModel();
            }
        }


        #endregion

        #region Save Command

        protected override bool OnSaveCommandCanExecute(object param)
        {
            return IsDirty && IsValid && IsOrderValid;
        }

        protected override void OnSaveCommandExecute(object param)
        {
            SaveLayaway(SelectedSaleOrder);
        }

        #endregion

        #region DeleteCommand

        protected override bool OnDeleteCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew && !SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Close) && !SelectedSaleOrder.IsVoided;
        }

        protected override void OnDeleteCommandExecute(object param)
        {
            if (LayawayManagerModel == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(LayawayInfo, Language.POS, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }


            if (SelectedSaleOrder != null)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_VoidBill"), Language.GetMsg("SO_Title_VoidBill"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result.Is(MessageBoxResult.Yes))
                {
                    VoidLayawayViewModel viewModel = new VoidLayawayViewModel(SelectedSaleOrder, LayawayManagerModel);
                    bool? dialogResult = _dialogService.ShowDialog<VoidLayawayView>(_ownerViewModel, viewModel, Language.GetMsg("SO_Title_VoidBill"));
                    if (dialogResult ?? false)
                    {
                        if (viewModel.RefundAmount != 0)
                        {
                            if (Define.CONFIGURATION.DefaultCashiedUserName ?? false)
                                viewModel.PaymentModel.Cashier = Define.USER.LoginName;
                            viewModel.PaymentModel.Shift = Define.ShiftCode;

                            SelectedSaleOrder.PaymentCollection.Add(viewModel.PaymentModel);
                            SelectedSaleOrder.Paid = SelectedSaleOrder.PaymentCollection.Where(x => !x.IsDeposit.Value && x.TotalPaid > 0).Sum(x => x.TotalPaid);
                            SelectedSaleOrder.RefundedAmount = viewModel.RefundAmount > 0 ? viewModel.RefundAmount : 0;
                            SelectedSaleOrder.CalcBalance();
                        }

                        VoidBillProcess(SelectedSaleOrder);
                        IsSearchMode = true;
                        _selectedSaleOrder = null;
                    }
                }
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
            Keyword = FilterText;
            IsAdvanced = false;

            //Reset if advance search has value to know current search simple
            _salesOrderAdvanceSearchViewModel.ResetKeyword();

            //Create & Execute Search Simple
            Expression<Func<base_SaleOrder, bool>> predicate = CreateSimpleSearchPredicate(FilterText);
            LoadDataByPredicate(predicate, false, 0);
        }

        #endregion

        #region DoubleClickCommand

        public RelayCommand<object> DoubleClickViewCommand { get; private set; }

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

                SetSaleOrderRelation(SelectedSaleOrder);
                SelectedSaleOrder.RaiseTotalPaid();

                GetLayawayManager(SelectedSaleOrder);

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

                SetAllowChangeOrder(SelectedSaleOrder);
                SelectedSaleOrder.IsDirty = false;

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
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
            if (IsAdvanced) //Current is use advanced search
            {
                predicate = _salesOrderAdvanceSearchViewModel.SearchAdvancePredicate;
            }
            else //Simple Search
            {
                if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                    predicate = CreateSimpleSearchPredicate(Keyword); //CreatePredicateWithConditionSearch(Keyword);           
            }
            LoadDataByPredicate(predicate, false, SaleOrderCollection.Count);
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
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsVoided && !SelectedSaleOrder.IsNew && SelectedSaleOrder.Balance > 0;
        }

        /// <summary>
        /// Method to invoke when the Payment command is executed.
        /// </summary>
        private void OnPaymentCommandExecute(object param)
        {
            LayawayPaymentProcess();
        }


        #endregion

        #region OpenServiceFeeDetailCommand
        /// <summary>
        /// Gets the OpenServiceFeeDetail Command.
        /// <summary>

        public RelayCommand<object> OpenServiceFeeDetailCommand { get; private set; }



        /// <summary>
        /// Method to check whether the OpenServiceFeeDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenServiceFeeDetailCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the OpenServiceFeeDetail command is executed.
        /// </summary>
        private void OnOpenServiceFeeDetailCommandExecute(object param)
        {
            ServiceFeeDetailViewModel viewModel = new ServiceFeeDetailViewModel(SelectedSaleOrder.OpenACFee, SelectedSaleOrder.LayawayFee, SelectedSaleOrder.LOtherFee, SelectedSaleOrder.LTotalFee);
            _dialogService.ShowDialog<ServiceFeeDetailView>(_ownerViewModel, viewModel, Language.GetMsg("SO_Title_ServiceFeeDetail"));
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
            productCondition = productCondition.And(x => !x.IsPurge.Value && x.IsLayaway && !x.IsOpenItem);

            //Condition : Product is not remove (Ispurge) & if is a product group, need has product child
            productCondition = productCondition.And(x => (!x.ItemTypeId.Equals(productGroupType) || (x.ItemTypeId.Equals(productGroupType) && x.base_ProductGroup1.Any())));

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
        #endregion

        //Main Grid
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

            if (NotifyLayawayManager(SelectedSaleOrder, out LayawayInfo))
            {
                LayawayManagerModel = new base_LayawayManagerModel(LayawayManagerCollection.FirstOrDefault());
                SelectedSaleOrder.OpenACFee = LayawayManagerModel.OpenACFee;

                SelectedSaleOrder.CopyFrom(saleOrderSource);
                SelectedSaleOrder.CalcBalance();
                SetSaleOrderToModel(SelectedSaleOrder);
                //Check not set to collection
                if (saleOrderSource.SaleOrderDetailCollection == null && saleOrderSource.base_SaleOrder.base_SaleOrderDetail.Any())
                {
                    SetSaleOrderRelation(saleOrderSource);

                }

                if (saleOrderSource.SaleOrderDetailCollection != null)
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderSource.SaleOrderDetailCollection.Where(x => string.IsNullOrWhiteSpace(x.ParentResource)))
                    {
                        string parentResource = CloneSaleOrderDetailModel(saleOrderDetailModel);
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

                SaveLayaway(SelectedSaleOrder);

                _selectedCustomer = null;
                //Set for selectedCustomer
                _selectedCustomer = CustomerCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedSaleOrder.CustomerResource));
                OnPropertyChanged(() => SelectedCustomer);
                SetAllowChangeOrder(SelectedSaleOrder);
                SelectedSaleOrder.IsDirty = false;
                IsSearchMode = false;
                IsForceFocused = true;
            }
            else
            {
                _selectedSaleOrder = null;
                IsSearchMode = true;
            }




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
            newSaleOrderDetailModel.CalcDueQty();

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

        #region ShipCommand
        /// <summary>
        /// Gets the Ship Command.
        /// <summary>

        public RelayCommand<object> ShipCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Ship command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShipCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            //SaleOrder Is Saved, has product in order & has paid or deposit and need to full payment
            return !SelectedSaleOrder.IsNew
                && SelectedSaleOrder.SaleOrderDetailCollection.Any()
                && SelectedSaleOrder.Balance <= 0
                && (SelectedSaleOrder.SaleOrderShipCollection == null ||
                    (SelectedSaleOrder.SaleOrderShipCollection != null && !SelectedSaleOrder.SaleOrderShipCollection.Any())) // Not Any Ship
                && (SelectedSaleOrder.Deposit > 0 || SelectedSaleOrder.Paid > 0);
        }


        /// <summary>
        /// Method to invoke when the Ship command is executed.
        /// </summary>
        private void OnShipCommandExecute(object param)
        {
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("SO_Message_LayawayReceivedAll"), Language.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Equals(MessageBoxResult.No) || result.Equals(MessageBoxResult.None))
                return;

            SelectedSaleOrder.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();
            base_SaleOrderShipModel saleOrderShipModel = new base_SaleOrderShipModel()
            {
                Resource = Guid.NewGuid(),
                SaleOrderId = SelectedSaleOrder.Id,
                SaleOrderResource = SelectedSaleOrder.Resource.ToString(),
                Weight = 1,
                TrackingNo = string.Empty,
                Carrier = string.Empty,
                IsShipped = true,
                Remark = CPC.POS.MarkType.Layaway.ToDescription(),
                SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>()
            };

            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SelectedSaleOrder.SaleOrderDetailCollection.Where(x => !x.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group)))
            {
                saleOrderDetailModel.PickQty += saleOrderDetailModel.Quantity;
                base_SaleOrderShipDetailModel saleOrderShipDetailModel = new base_SaleOrderShipDetailModel();
                saleOrderShipDetailModel.Resource = Guid.NewGuid();
                //Set SaleOrder Detail to get Information of product or SaleOrderDetail
                saleOrderShipDetailModel.SaleOrderDetailModel = saleOrderDetailModel;
                saleOrderShipDetailModel.SaleOrderShipResource = saleOrderShipModel.Resource.ToString();
                saleOrderShipDetailModel.SaleOrderDetailResource = saleOrderDetailModel.Resource.ToString();
                saleOrderShipDetailModel.ProductResource = saleOrderDetailModel.ProductResource;
                saleOrderShipDetailModel.ItemCode = saleOrderDetailModel.ItemCode;
                saleOrderShipDetailModel.ItemName = saleOrderDetailModel.ItemName;
                saleOrderShipDetailModel.ItemAtribute = saleOrderDetailModel.ItemAtribute;
                saleOrderShipDetailModel.ItemSize = saleOrderDetailModel.ItemSize;
                saleOrderShipDetailModel.PackedQty = saleOrderDetailModel.QtyOfPick;
                saleOrderShipModel.SaleOrderShipDetailCollection.Add(saleOrderShipDetailModel);
            }

            SelectedSaleOrder.SaleOrderShipCollection.Add(saleOrderShipModel);
            SelectedSaleOrder.ShippedBox = Convert.ToInt16(SelectedSaleOrder.SaleOrderShipCollection.Count(x => x.IsShipped));
            SaveLayaway(SelectedSaleOrder);
            SelectedSaleOrder.IsReceived = SelectedSaleOrder.base_SaleOrder.base_SaleOrderShip.Any();
        }
        #endregion

        #region PrintCommand
        /// <summary>
        /// Gets the Print Command.
        /// <summary>

        public RelayCommand<object> PrintCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Print command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPrintCommandCanExecute(object param)
        {
            if (SelectedSaleOrder == null)
                return false;
            return !SelectedSaleOrder.IsNew;
        }

        /// <summary>
        /// Method to invoke when the Print command is executed.
        /// </summary>
        private void OnPrintCommandExecute(object param)
        {
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            rpt.ShowReport("rptPaymentPlan", "'" + SelectedSaleOrder.Resource.ToString() + "'");
        }
        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// LoadDataByPredicate
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
                try
                {
                    IsBusy = true;
                    //Only load Layaway Order
                    string layawayMark = MarkType.Layaway.ToDescription();

                    Expression<Func<base_SaleOrder, bool>> predicateAll = PredicateBuilder.True<base_SaleOrder>();
                    predicateAll = predicateAll.And(x => !x.IsPurge && !x.IsLocked && x.Mark == layawayMark).And(predicate);

                    //Cout all SaleOrder in Data base show on grid
                    lock (UnitOfWork.Locker)
                    {
                        TotalSaleOrder = _saleOrderRepository.GetIQueryable(predicateAll).Count();

                        //Get data with range
                        IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetRangeDescending<long>(currentIndex - _numberNewItem, NumberOfDisplayItems, x => x.Id, predicateAll);

                        foreach (base_SaleOrder saleOrder in saleOrders)
                        {
                            bgWorker.ReportProgress(0, saleOrder);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
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
                if (_viewExisted && !IsSearchMode && SelectedSaleOrder != null && SaleOrderCollection.Any() && !SelectedSaleOrder.IsNew) //Item is selected
                {
                    SelectedSaleOrder = SaleOrderCollection.SingleOrDefault(x => x.Id.Equals(SelectedSaleOrder.Id));
                    SetSaleOrderRelation(SelectedSaleOrder, true);
                    GetLayawayManager(SelectedSaleOrder, false);
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
                }

                //Search Customer Name
                var customerList = CustomerCollection.Where(y => y.LastName.ToLower().Contains(keyword.ToLower()) || y.FirstName.ToLower().Contains(keyword.ToLower())).Select(x => x.Resource.ToString());
                predicate = predicate.Or(x => customerList.Contains(x.CustomerResource));

                //Search deciaml

                decimal decimalValue = 0;
                if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalValue) && decimalValue != 0)
                {
                    //Total 
                    predicate = predicate.Or(x => x.Total.Equals(decimalValue));

                    //Deposit 
                    predicate = predicate.Or(x => x.Deposit == decimalValue);

                    //Balance 
                    predicate = predicate.Or(x => x.Balance.Equals(decimalValue));
                }

                //Price Level
                IEnumerable<short> priceSchemaList = Common.PriceSchemas.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => Convert.ToInt16(x.Value));
                predicate = predicate.Or(x => priceSchemaList.Contains(x.PriceSchemaId));

                //Tax Code & Tax Code Excemption
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
        /// Set CustomerRewardCollection for RewardMember
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
        /// Extent Set To model
        /// </summary>
        /// <param name="saleOrderModel"></param>
        protected override void SetSaleOrderToModel(base_SaleOrderModel saleOrderModel)
        {
            base.SetSaleOrderToModel(saleOrderModel);
            saleOrderModel.IsReceived = saleOrderModel.base_SaleOrder.base_SaleOrderShip.Any();

            saleOrderModel.IsDirty = true;
        }

        /// <summary>
        /// Set SaleOrder relation
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        protected override void SetSaleOrderRelation(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            base.SetSaleOrderRelation(saleOrderModel, isForce);

            LoadSaleOrderShipCollection(saleOrderModel, isForce);

            LoadPaymentCollection(saleOrderModel);

            saleOrderModel.RaiseAnyShipped();



            saleOrderModel.IsDirty = false;

        }



        /// <summary>
        /// Load payment collection 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void LoadPaymentCollection(base_SaleOrderModel saleOrderModel)
        {
            // Get document resource
            string docResource = saleOrderModel.Resource.ToString();

            // Get all payment by document resource
            IEnumerable<base_ResourcePayment> payments = _paymentRepository.GetAll(x => x.DocumentResource.Equals(docResource));

            // Load payment collection
            saleOrderModel.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>(payments.Select(x => new base_ResourcePaymentModel(x)));

            // Check show PaymentTab
            saleOrderModel.PaymentProcess = saleOrderModel.PaymentCollection.Any();
        }

        /// <summary>
        /// Load SaleOrder ShipCollection
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <param name="isForce"></param>
        private void LoadSaleOrderShipCollection(base_SaleOrderModel saleOrderModel, bool isForce = false)
        {
            if (isForce || saleOrderModel.SaleOrderShipCollection == null || !saleOrderModel.SaleOrderShipCollection.Any())
            {
                saleOrderModel.SaleOrderShipCollection = new CollectionBase<base_SaleOrderShipModel>();

                foreach (base_SaleOrderShip saleOrderShip in saleOrderModel.base_SaleOrder.base_SaleOrderShip)
                {
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
        /// Load Layaway Manager Collection
        /// </summary>
        private void LoadLayawayManagerCollection()
        {
            short layawayManagerStatus = (short)StatusBasic.Active;
            LayawayManagerCollection = new List<base_LayawayManager>(_layawayManagerRepository.GetAll(x => x.Status.Equals(layawayManagerStatus)));
        }

        /// <summary>
        /// Create New Item
        /// </summary>
        private void CreateNewLayawayModel()
        {
            try
            {
                _isForceFocused = false;
                CreateNewSaleOrder();

                if (NotifyLayawayManager(SelectedSaleOrder, out LayawayInfo))
                {
                    LayawayManagerModel = new base_LayawayManagerModel(LayawayManagerCollection.FirstOrDefault());
                    SelectedSaleOrder.OpenACFee = LayawayManagerModel.OpenACFee;
                    SelectedSaleOrder.SaleReference = LayawayManagerModel.Resource.ToString();
                    SelectedSaleOrder.IsDirty = false;
                    IsSearchMode = false;
                    IsForceFocused = true;
                }
                else
                {
                    _selectedSaleOrder = null;
                    IsSearchMode = true;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Insert New sale order
        /// </summary>
        private void InsertLayaway(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                if (saleOrderModel.IsNew)
                {
                    UpdateCustomerAddress(saleOrderModel.BillAddressModel);
                    saleOrderModel.BillAddressId = saleOrderModel.BillAddressModel.Id;
                    UpdateCustomerAddress(saleOrderModel.ShipAddressModel);
                    saleOrderModel.ShipAddressId = saleOrderModel.ShipAddressModel.Id;
                    //Sale Order Detail Model
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                    {
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.StoreCode, saleOrderDetailModel.Quantity);
                        saleOrderDetailModel.ToEntity();
                        saleOrderModel.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                    }
                    _productRepository.Commit();

                    SavePaymentCollection(saleOrderModel);

                    saleOrderModel.Shift = Define.ShiftCode;
                    saleOrderModel.DateUpdated = DateTime.Now;
                    saleOrderModel.DateCreated = DateTime.Now;
                    saleOrderModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;

                    saleOrderModel.ToEntity();
                    _saleOrderRepository.Add(saleOrderModel.base_SaleOrder);

                    _saleOrderRepository.Commit();
                    saleOrderModel.EndUpdate();
                    //Set ID
                    saleOrderModel.ToModel();
                    saleOrderModel.EndUpdate();
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                    {
                        saleOrderDetailModel.ToModel();
                        saleOrderDetailModel.EndUpdate();
                    }

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
                    SaleOrderCollection.Insert(0, saleOrderModel);
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
        /// UpdateLayaway
        /// </summary>
        /// <param name="UpdateQtyCustomer"></param>
        private void UpdateLayaway(base_SaleOrderModel saleOrderModel)
        {
            try
            {
                //Insert or update address for customer
                UpdateCustomerAddress(saleOrderModel.BillAddressModel);
                UpdateCustomerAddress(saleOrderModel.ShipAddressModel);

                #region SaleOrderDetail
                //Delete SaleOrderDetail
                if (saleOrderModel.SaleOrderDetailCollection.DeletedItems.Any())
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection.DeletedItems)
                    {
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.StoreCode, saleOrderDetailModel.Quantity, false/*=Descrease*/);
                        _saleOrderDetailRepository.Delete(saleOrderDetailModel.base_SaleOrderDetail);
                    }
                    _saleOrderDetailRepository.Commit();
                    saleOrderModel.SaleOrderDetailCollection.DeletedItems.Clear();
                }
                if (saleOrderModel.IsVoided)
                {
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                    {
                        _saleOrderRepository.UpdateCustomerQuantity(saleOrderDetailModel, saleOrderModel.base_SaleOrder.StoreCode, saleOrderDetailModel.base_SaleOrderDetail.Quantity, false/*descrease quantity*/);
                    }
                }
                else
                {
                    //Sale Order Detail Model
                    foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                    {
                        if (saleOrderDetailModel.Quantity != saleOrderDetailModel.base_SaleOrderDetail.Quantity || saleOrderDetailModel.UOMId != saleOrderDetailModel.base_SaleOrderDetail.UOMId)
                        {
                            _saleOrderRepository.UpdateCustomerQuantityChanged(saleOrderDetailModel, saleOrderModel.StoreCode);
                        }
                        saleOrderDetailModel.ToEntity();
                        if (saleOrderDetailModel.IsNew)
                            saleOrderModel.base_SaleOrder.base_SaleOrderDetail.Add(saleOrderDetailModel.base_SaleOrderDetail);
                    }

                }

                _productRepository.Commit();
                #endregion

                //Save ShipColletion
                SaveSaleOrderShipCollection(saleOrderModel);

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

                saleOrderModel.DateUpdated = DateTime.Now;
                saleOrderModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                saleOrderModel.ToEntity();
                _saleOrderRepository.Commit();

                //Set ID
                saleOrderModel.ToModel();
                saleOrderModel.EndUpdate();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderModel.SaleOrderDetailCollection)
                {
                    saleOrderDetailModel.ToModel();
                    saleOrderDetailModel.EndUpdate();
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
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// SalveLayaway
        /// </summary>
        /// <returns></returns>
        private bool SaveLayaway(base_SaleOrderModel saleOrderModel)
        {
            bool result = false;
            try
            {
                if (LayawayManagerModel != null)
                {
                    if (CheckLayawayMinimumPurchase(saleOrderModel))
                    {
                        UnitOfWork.BeginTransaction();
                        if (saleOrderModel.IsNew)
                            InsertLayaway(saleOrderModel);
                        else
                            UpdateLayaway(saleOrderModel);

                        UpdateCustomer(saleOrderModel);
                        UnitOfWork.CommitTransaction();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                result = false;
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            return result;
        }

        /// <summary>
        /// Void Bill
        /// </summary>
        /// <param name="SelectedSaleOrder"></param>
        private void VoidBillProcess(base_SaleOrderModel saleOrderModel)
        {
            saleOrderModel.OrderStatus = (short)SaleOrderStatus.Void;
            saleOrderModel.IsVoided = true;
            SaveLayaway(saleOrderModel);
        }

        //View Util
        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M106"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                        result = SaveLayaway(SelectedSaleOrder);
                    else //Has Error
                        result = false;
                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (SelectedSaleOrder.IsNew)
                    {
                        SelectedSaleOrder = null;
                        if (isClosing.HasValue && !isClosing.Value)
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


        //Get Set Value
        /// <summary>
        /// set user change order follow config & order status
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetAllowChangeOrder(base_SaleOrderModel saleOrderModel)
        {
            if (BreakAllChange)
                return;

            if (!saleOrderModel.OrderStatus.Equals((short)SaleOrderStatus.Open)
                || saleOrderModel.IsVoided
                || saleOrderModel.IsLocked
                || !IsLayawayManagerValid
                || (saleOrderModel.PaymentCollection != null && saleOrderModel.PaymentCollection.Any()))
                this.IsAllowChangeOrder = false;
            else
                this.IsAllowChangeOrder = true;
            //saleOrderModel.OrderStatus == (short)SaleOrderStatus.FullyShipped && Define.CONFIGURATION.IsAllowChangeOrder.Value;

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
        /// Deposite for Quotation
        /// </summary>
        private void DepositProcess()
        {
            try
            {
                if (SelectedSaleOrder.OrderStatus.Equals((short)SaleOrderStatus.Open))
                    SelectedSaleOrder.RewardAmount = SelectedSaleOrder.Total;

                decimal balance = SelectedSaleOrder.RewardAmount - SelectedSaleOrder.Deposit.Value;
                decimal depositTaken = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);

                //calculate deposit from percent
                decimal depositAmountByPercent = (SelectedSaleOrder.Total * LayawayManagerModel.DepositPercent) / 100;
                decimal depositRequired = (depositAmountByPercent > LayawayManagerModel.DepositAmount ? depositAmountByPercent : LayawayManagerModel.DepositAmount) - depositTaken;

                //Show Payment
                SalesOrderPaymenViewModel paymentViewModel = new SalesOrderPaymenViewModel(SelectedSaleOrder, balance, depositTaken, depositRequired);
                bool? dialogResult = _dialogService.ShowDialog<DepositPaymentView>(_ownerViewModel, paymentViewModel, Language.GetMsg("SO_Title_Deposit"));
                if (dialogResult == true)
                {
                    if (Define.CONFIGURATION.DefaultCashiedUserName.HasValue && Define.CONFIGURATION.DefaultCashiedUserName.Value)
                        paymentViewModel.PaymentModel.Cashier = Define.USER.LoginName;
                    paymentViewModel.PaymentModel.Shift = Define.ShiftCode;

                    if (SelectedSaleOrder.PaymentCollection == null)
                        SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                    SelectedSaleOrder.PaymentCollection.Add(paymentViewModel.PaymentModel);
                    SelectedSaleOrder.Deposit = SelectedSaleOrder.PaymentCollection.Where(x => x.IsDeposit.Value).Sum(x => x.TotalPaid);
                    SelectedSaleOrder.CalcSubTotal();
                }

                SelectedSaleOrder.PaymentProcess = SelectedSaleOrder.PaymentCollection.Any();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open Sale Order or Quotation Advance Search
        /// </summary>
        private void OpenSOAdvanceSearch()
        {
            if (_waitingTimer != null)
                _waitingTimer.Stop();
            _salesOrderAdvanceSearchViewModel.CustomerCollection = this.CustomerCollection.ToList();
            _salesOrderAdvanceSearchViewModel.LoadData("Layaway");
            bool? dialogResult = _dialogService.ShowDialog<SalesOrderAdvanceSearchView>(_ownerViewModel, _salesOrderAdvanceSearchViewModel, Language.GetMsg("C104"));
            if (dialogResult == true)
            {
                IsAdvanced = true;
                Expression<Func<base_SaleOrder, bool>> predicate = _salesOrderAdvanceSearchViewModel.SearchAdvancePredicate;
                LoadDataByPredicate(predicate, false, 0);
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
                decimal parentPickQty = totalOfPick * parentSaleOrderDetailModel.Quantity / totalQty;
                parentSaleOrderDetailModel.PickQty = Math.Round(parentPickQty, 2);
            }
        }

        /// <summary>
        /// Layaway Payment 
        /// <para>Deposit & Payment Layaway order</para>
        /// </summary>
        private void LayawayPaymentProcess()
        {
            try
            {
                if (LayawayManagerModel == null)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(this.LayawayInfo, Language.POS, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    return;
                }

                if (SelectedSaleOrder.PaymentCollection == null)
                    SelectedSaleOrder.PaymentCollection = new ObservableCollection<base_ResourcePaymentModel>();

                //Check minumum purchase need to paid
                if (CheckLayawayMinimumPurchase(SelectedSaleOrder))
                {
                    LayawayPaymentViewModel viewModel = new LayawayPaymentViewModel(SelectedSaleOrder, LayawayManagerModel);
                    bool? dialogResult = _dialogService.ShowDialog<LayawayPaymentView>(_ownerViewModel, viewModel, "Layaway Payment");
                    if (dialogResult ?? false)
                    {
                        SelectedSaleOrder.CalcBalance();

                        //Set Status For Layaway
                        SetLayawayStatus(SelectedSaleOrder);

                        if (SelectedSaleOrder.Balance <= 0)//Full payment
                            SaveSaleCommission(SelectedSaleOrder);

                        SetAllowChangeOrder(SelectedSaleOrder);

                        SaveLayaway(SelectedSaleOrder);
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
        /// Get & Check Isvalid layaway
        /// </summary>
        /// <param name="orderDate"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool LayawayValid(DateTime orderDate, out string info)
        {
            if (LayawayManagerCollection == null || !LayawayManagerCollection.Any())
            {
                info = Language.GetMsg("SO_TextBlock_NotAnyLayaway");
                return false;
            }
            else
            {
                if (LayawayManagerCollection.Count() > 1)
                {
                    info = Language.GetMsg("SO_TextBlock_LayawayConflict");
                    return false;
                }
                else if (LayawayManagerCollection.Count() == 1 && LayawayManagerCollection.Any(x => x.StartDate <= orderDate.Date && orderDate.Date <= x.EndDate))
                {
                    info = string.Empty;
                    return true;
                }
                else
                {
                    info = Language.GetMsg("SO_TextBlock_NotAnyLayaway");
                    return false;
                }
            }

        }

        /// <summary>
        /// Check minimum purchase customer need to execute process if not any payment
        /// <para></para>
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <returns></returns>
        private bool CheckLayawayMinimumPurchase(base_SaleOrderModel saleOrderModel)
        {
            if (!saleOrderModel.PaymentCollection.Any() && saleOrderModel.Total < LayawayManagerModel.MinimumPurchase)
            {
                string info = string.Format(Language.GetMsg("SO_Message_LW_NotSmallerMinimumPurchase") + " ", string.Format(Define.ConverterCulture, Define.CurrencyFormat, LayawayManagerModel.MinimumPurchase));
                Xceed.Wpf.Toolkit.MessageBox.Show(info, Language.GetMsg("POSCaption"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get Layaway For SaleOrder
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void GetLayawayManager(base_SaleOrderModel saleOrderModel, bool isNotify = false)
        {
            if (NotifyLayawayManager(saleOrderModel, out LayawayInfo, isNotify))
                LayawayManagerModel = new base_LayawayManagerModel(LayawayManagerCollection.FirstOrDefault());
            else
                LayawayManagerModel = null;
        }

        /// <summary>
        /// Check & Notify Layaway not valid
        /// <para>isnotify =true: show message box notitfy layawy not valid</para>
        /// </summary>
        private bool NotifyLayawayManager(base_SaleOrderModel saleOrderModel, out string info, bool isNotify = true)
        {
            IsLayawayManagerValid = LayawayValid(saleOrderModel.OrderDate.Value, out info);

            SetAllowChangeOrder(saleOrderModel);
            if (!IsLayawayManagerValid)
            {
                if (isNotify)
                    Xceed.Wpf.Toolkit.MessageBox.Show(info, Language.POS, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set Layaway status
        /// </summary>
        /// <param name="saleOrderModel"></param>
        private void SetLayawayStatus(base_SaleOrderModel saleOrderModel)
        {
            if (saleOrderModel.PaymentCollection.Any() && saleOrderModel.Balance <= 0)
                saleOrderModel.OrderStatus = (short)SaleOrderStatus.Close;
            else if (saleOrderModel.PaymentCollection.Any() && saleOrderModel.Balance > 0)
                saleOrderModel.OrderStatus = (short)SaleOrderStatus.InProcess;
            else
                saleOrderModel.OrderStatus = (short)SaleOrderStatus.Open;
        }

        /// <summary>
        /// Calculate Other Fee
        /// </summary>
        private decimal CalculateOtherFee(base_SaleOrderModel saleOrderModel)
        {
            if (LayawayManagerModel != null)
            {
                if (LayawayManagerModel.OtherFeeUnit.Is(UnitType.Money))
                {
                    return LayawayManagerModel.OtherFee;
                }
                else
                {
                    return saleOrderModel.SubTotal * LayawayManagerModel.OtherFee / 100;
                }
            }
            return 0;
        }

        /// <summary>
        /// Calculate Layaway Fee 
        /// </summary>
        /// <param name="saleOrderModel"></param>
        /// <returns></returns>
        private decimal CalculateLayawayFee(base_SaleOrderModel saleOrderModel)
        {
            if (LayawayManagerModel != null)
            {
                if (LayawayManagerModel.OtherFeeUnit.Is(UnitType.Money))
                {
                    return LayawayManagerModel.LayawayFee;
                }
                else
                {
                    return saleOrderModel.SubTotal * LayawayManagerModel.LayawayFee / 100;
                }
            }
            return 0;
        }

        #endregion

        #region PropertyChanged

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
                    saleOrderModel.LayawayFee = CalculateLayawayFee(saleOrderModel);
                    saleOrderModel.LOtherFee = CalculateOtherFee(saleOrderModel);
                    saleOrderModel.LTotalFee = saleOrderModel.LayawayFee + saleOrderModel.LOtherFee + saleOrderModel.OpenACFee;
                    saleOrderModel.CalcSubTotal();
                    break;
                case "Total":
                    saleOrderModel.RewardAmount = saleOrderModel.Total;
                    saleOrderModel.CalcBalance();
                    break;
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
                    if (saleOrderModel.TaxLocationModel.TaxCodeModel.IsTaxAfterDiscount)
                        saleOrderModel.TaxAmount = saleOrderModel.ProductTaxAmount + saleOrderModel.ShipTaxAmount - saleOrderModel.DiscountAmount;
                    else
                        saleOrderModel.TaxAmount = saleOrderModel.ShipTaxAmount + saleOrderModel.ProductTaxAmount;

                    break;
                case "TaxAmount":
                    saleOrderModel.CalcTotal();
                    break;
                case "DiscountAmount":
                    break;
                case "DiscountPercent":
                    break;
                case "PriceSchemaId"://Update Price When Price Schema Changed
                    PriceSchemaChanged();
                    break;
                case "OrderStatus":
                    saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                    SetAllowChangeOrder(saleOrderModel);
                    break;
                case "StoreCode":
                    StoreChanged();
                    break;
                case "TotalPaid":

                    break;


            }
        }
        protected override void SaleOrderDetailCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SaleOrderDetailCollection_CollectionChanged(sender, e);


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

        protected override void SaleOrderDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                    if (saleOrderDetailModel.ProductModel != null)
                    {
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
                    }
                    saleOrderDetailModel.CalcDueQty();
                    saleOrderDetailModel.CalcSubTotal();
                    if (!saleOrderDetailModel.ProductModel.IsSerialTracking)
                    {
                        BreakSODetailChange = true;
                        _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                        BreakSODetailChange = false;
                    }

                    CalculateMultiNPriceTax();
                    SelectedSaleOrder.CalcSubTotal();
                    _saleOrderRepository.CalcOnHandStore(SelectedSaleOrder, saleOrderDetailModel);
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

                    BreakSODetailChange = true;
                    _saleOrderRepository.CalcProductDiscount(SelectedSaleOrder, saleOrderDetailModel);
                    BreakSODetailChange = false;

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

        #endregion

        #region Override Methods

        protected override void InitialCommand()
        {
            base.InitialCommand();

            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            DuplicateItemCommand = new RelayCommand<object>(OnDuplicateItemCommandExecute, OnDuplicateItemCommandCanExecute);
            EditItemCommand = new RelayCommand<object>(OnEditItemCommandExecute, OnEditItemCommandCanExecute);
            SaleOrderAdvanceSearchCommand = new RelayCommand<object>(OnSaleOrderAdvanceSearchCommandExecute, OnSaleOrderAdvanceSearchCommandCanExecute);
            PaymentCommand = new RelayCommand<object>(OnPaymentCommandExecute, OnPaymentCommandCanExecute);
            ShipCommand = new RelayCommand<object>(OnShipCommandExecute, OnShipCommandCanExecute);
            PrintCommand = new RelayCommand<object>(OnPrintCommandExecute, OnPrintCommandCanExecute);

            OpenServiceFeeDetailCommand = new RelayCommand<object>(OnOpenServiceFeeDetailCommandExecute, OnOpenServiceFeeDetailCommandCanExecute);
        }

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
            };

            bg.RunWorkerCompleted += (sender, e) =>
            {
                this.SaleOrderCollection.Clear();
                IsBusy = false;
                Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();
                if (!string.IsNullOrWhiteSpace(Keyword))//Load with Search Condition
                    predicate = CreateSimpleSearchPredicate(Keyword); // CreatePredicateWithConditionSearch(Keyword);

                LoadDataByPredicate(predicate);

                //if (_viewExisted && SelectedSaleOrder != null)
                //{
                //    if (SelectedSaleOrder.IsNew)
                //    {
                //        if (!NotifyLayawayManager(SelectedSaleOrder, out LayawayInfo))
                //        {
                //            _selectedSaleOrder = null;
                //            IsSearchMode = true;
                //        }
                //        else
                //        {
                //            IsSearchMode = false;
                //        }
                //    }
                //}

                _viewExisted = true;
            };
            bg.RunWorkerAsync();

        }

        protected override void LoadDynamicData()
        {
            try
            {
                base.LoadDynamicData();

                //Load Layaway Setup
                LoadLayawayManagerCollection();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    if (!isList)
                    {
                        CreateNewLayawayModel();
                    }
                    else
                        IsSearchMode = true;
                }
            }
            else
            {
                if (param is ComboItem)
                {

                    //Currently form is be called from another form, bellow methods get param & set Id to temparate variable(SaleOrderId).
                    //if param has not isChecked(form is Actived), form with be load again. after form loaded, set SelectedSaleOrder item base one temp variable(SaleOrderId)
                    //Otherwise,LoadData method won't be loaded, need to set selectedSaleOrder after recived value

                    ComboItem cmbValue = param as ComboItem;
                    if (cmbValue.Text.Equals("Customer"))//Create SaleOrder With Customer
                    {
                        CreateNewLayawayModel();
                        if (_selectedSaleOrder != null)
                        {
                            long customerId = Convert.ToInt64(cmbValue.Detail);
                            SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Id.Equals(customerId));
                            this.IsSearchMode = false;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void CreateExtentSaleOrder()
        {
            base.CreateExtentSaleOrder();
            _selectedCustomer = null;
            OnPropertyChanged(() => SelectedCustomer);
            _selectedSaleOrder.Mark = MarkType.Layaway.ToDescription();
            _selectedSaleOrder.OrderStatus = (short)SaleOrderStatus.Open;
            _selectedSaleOrder.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(_selectedSaleOrder.OrderStatus));
            SetAllowChangeOrder(_selectedSaleOrder);
            _selectedSaleOrder.SaleOrderDetailCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(SaleOrderDetailCollection_CollectionChanged);
            _selectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SelectedSaleOrderChanged()
        {
            base.SelectedSaleOrderChanged();

            if (SelectedSaleOrder != null)
            {
                SelectedSaleOrder.PropertyChanged -= new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
                SelectedSaleOrder.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleOrder_PropertyChanged);
            }
        }

        /// <summary>
        /// Open Popup Search Product Advanced
        /// </summary>
        protected override void SearchProductAdvance()
        {
            //Get Product Search in advance with condition Product Type
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel(false/*Not Coupon*/, true/*Group*/, false/*Service*/, false/*Insurace*/, true/*Layaway*/, false/*OpenItem*/);
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, Language.GetMsg("SO_Title_SearchProduct"));
            if (dialogResult == true)
            {
                CreateSaleOrderDetailWithProducts(productSearchViewModel.SelectedProducts);
            }
        }

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            if (dropInfo.Data is ComboItem)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(DropInfo dropInfo)
        {
            if (dropInfo.Data is ComboItem)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Layaway", dropInfo.Data);
            }
        }

        #endregion
    }
}