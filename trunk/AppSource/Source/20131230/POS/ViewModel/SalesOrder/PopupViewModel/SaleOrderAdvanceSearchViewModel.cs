using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.Helper;
using CPC.POS.Database;
using System.Linq.Expressions;
using CPC.POS.Model;
using System.Collections.ObjectModel;

namespace CPC.POS.ViewModel
{
    class SalesOrderAdvanceSearchViewModel : ViewModelBase
    {
        #region Define

        #endregion

        #region Constructors
        public SalesOrderAdvanceSearchViewModel()
        {
            InitialCommand();
        }
        #endregion

        #region Properties

        #region ScanCode
        private string _scanCode;
        /// <summary>
        /// Gets or sets the ScanCode.
        /// </summary>
        public string ScanCode
        {
            get { return _scanCode; }
            set
            {
                if (_scanCode != value)
                {
                    _scanCode = value;
                    OnPropertyChanged(() => ScanCode);
                }
            }
        }
        #endregion

        #region DocumentNo
        private string _documentNo;
        /// <summary>
        /// Gets or sets the DocumentNum.
        /// </summary>
        public string DocumentNo
        {
            get { return _documentNo; }
            set
            {
                if (_documentNo != value)
                {
                    _documentNo = value;
                    OnPropertyChanged(() => DocumentNo);
                }
            }
        }
        #endregion

        #region OrderFrom
        private DateTime? _orderFrom;
        /// <summary>
        /// Gets or sets the OrderFrom.
        /// </summary>
        public DateTime? OrderFrom
        {
            get { return _orderFrom; }
            set
            {
                if (_orderFrom != value)
                {
                    _orderFrom = value;
                    OnPropertyChanged(() => OrderFrom);
                }
            }
        }
        #endregion

        #region OrderTo
        private DateTime? _orderTo;
        /// <summary>
        /// Gets or sets the OrderTo.
        /// </summary>
        public DateTime? OrderTo
        {
            get { return _orderTo; }
            set
            {
                if (_orderTo != value)
                {
                    _orderTo = value;
                    OnPropertyChanged(() => OrderTo);
                }
            }
        }
        #endregion

        #region CustomerName
        private string _customerName;
        /// <summary>
        /// Gets or sets the CustomerName.
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            set
            {
                if (_customerName != value)
                {
                    _customerName = value;
                    OnPropertyChanged(() => CustomerName);
                }
            }
        }
        #endregion

        #region TotalCompareType
        private string _totalCompareType = ">";
        /// <summary>
        /// Gets or sets the TotalCompareType.
        /// </summary>
        public string TotalCompareType
        {
            get { return _totalCompareType; }
            set
            {
                if (_totalCompareType != value)
                {
                    _totalCompareType = value;
                    OnPropertyChanged(() => TotalCompareType);
                }
            }
        }
        #endregion

        #region Total
        private decimal? _total;
        /// <summary>
        /// Gets or sets the Total.
        /// </summary>
        public decimal? Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(() => Total);
                }
            }
        }
        #endregion

        #region TaxCode
        private string _taxCode;
        /// <summary>
        /// Gets or sets the TaxCode.
        /// </summary>
        public string TaxCode
        {
            get { return _taxCode; }
            set
            {
                if (_taxCode != value)
                {
                    _taxCode = value;
                    OnPropertyChanged(() => TaxCode);
                }
            }
        }
        #endregion

        #region Status
        private short _staus;
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public short Status
        {
            get { return _staus; }
            set
            {
                if (_staus != value)
                {
                    _staus = value;
                    OnPropertyChanged(() => Status);
                }
            }
        }
        #endregion

        //Extent

        #region CustomerCollection
        private List<base_GuestModel> _customerCollection;
        /// <summary>
        /// Gets or sets the CustomerCollection.
        /// </summary>
        public List<base_GuestModel> CustomerCollection
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
        #endregion


        #region StatusCollection
        private ObservableCollection<ComboItem> _statusCollection = new ObservableCollection<ComboItem>();
        /// <summary>
        /// Gets or sets the StatusCollection.
        /// </summary>
        public ObservableCollection<ComboItem> StatusCollection
        {
            get { return _statusCollection; }
            set
            {
                if (_statusCollection != value)
                {
                    _statusCollection = value;
                    OnPropertyChanged(() => StatusCollection);
                }
            }
        }
        #endregion



        public Expression<Func<base_SaleOrder, bool>> SearchAdvancePredicate { get; set; }
        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Gets the Ok Command.
        /// <summary>

        public RelayCommand<object> OkCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            Search();
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }


        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Search Execute
        /// </summary>
        private void Search()
        {
            Expression<Func<base_SaleOrder, bool>> predicate = PredicateBuilder.True<base_SaleOrder>();

            //Search SO Card by SaleOrderBarcode
            if (!string.IsNullOrWhiteSpace(ScanCode))
                predicate = predicate.And(x => x.SOCard.ToLower().Contains(ScanCode.ToLower()));

            //Search SO Number by DocumentNo
            if (!string.IsNullOrWhiteSpace(DocumentNo))
                predicate = predicate.And(x => x.SONumber.ToLower().Contains(DocumentNo.ToLower()));

            //Search by OrderFrom of OrderDate
            if (OrderFrom.HasValue)
            {
                DateTime from = OrderFrom.Value.Date;
                predicate = predicate.And(x => from <= x.OrderDate);
            }

            //Search OrderDate by OrderFrom
            if (OrderTo.HasValue)
            {
                DateTime to = OrderTo.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                predicate = predicate.And(x => to >= x.OrderDate);
            }
            //Search Customer by CustomerName
            if (!string.IsNullOrWhiteSpace(CustomerName))
            {
                var customerList = CustomerCollection.Where(y => y.LastName.ToLower().Contains(CustomerName.ToLower()) || y.FirstName.ToLower().Contains(CustomerName.ToLower())).Select(x => x.Resource.ToString());
                predicate = predicate.And(x => customerList.Contains(x.CustomerResource));
            }

            //Search Total By Total & CompareType
            if (this.Total.HasValue)
            {
                switch (this.TotalCompareType)
                {
                    case ">":
                        predicate = predicate.And(x => x.Total > this.Total);
                        break;
                    case "<":
                        predicate = predicate.And(x => x.Total < this.Total);
                        break;
                    case "=":
                        predicate = predicate.And(x => x.Total == this.Total);
                        break;
                }
            }

            //Search Tax Code by Tax Code key word
            if (!string.IsNullOrWhiteSpace(this.TaxCode))
                predicate = predicate.And(x => x.TaxCode.ToLower().Contains(this.TaxCode.ToLower()));

            //Search Status
            if (Status > 0)
            {
                predicate = predicate.And(x => x.OrderStatus.Equals(Status));
            }

            //Map Result Coditional
            SearchAdvancePredicate = predicate;
        }

        private void LoadStatusCollection(string view)
        {
            IEnumerable<ComboItem> statusList = new List<ComboItem>();
            switch (view)
            {
                case "WorkOrder":
                    statusList = Common.StatusSalesOrders.Where(x => Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Open)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Shipping)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.FullyShipped)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Close));
                    break;
                case "Quotation":
                    statusList = Common.StatusSalesOrders.Where(x => Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Open)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Shipping)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.FullyShipped)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Close)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Quote));

                    break;
                case "SaleOrder":
                    statusList = Common.StatusSalesOrders.Where(x => Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Open)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Shipping)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.FullyShipped)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.PaidInFull));
                                              
                    break;
                case "Layaway":
                    statusList = Common.StatusSalesOrders.Where(x => Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Open)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Close)
                                              || Convert.ToInt16(x.ObjValue).Equals((short)SaleOrderStatus.Layaway));
                    break;
                
            }

            StatusCollection = new ObservableCollection<ComboItem>(statusList);
            if (!StatusCollection.Any(x => x.Value.Equals(0)))
            {
                ComboItem defautItem = new ComboItem()
                {
                    ObjValue = Convert.ToInt16(0),
                    Value = 0,
                    Text = string.Empty
                };
                StatusCollection.Insert(0, defautItem);
            }
        }

        public void ResetKeyword()
        {
            ScanCode = string.Empty;
            DocumentNo = string.Empty;
            OrderFrom = null;
            OrderTo = null;
            CustomerName = string.Empty;
            TotalCompareType = ">";
            Total = null;

        }

        /// <summary>
        /// Call LoadData to load require data for search
        /// </summary>

        public void LoadData(string view)
        {
            LoadStatusCollection(view);
        }


       
        #endregion

        #region Public Methods
        #endregion
    }


}
