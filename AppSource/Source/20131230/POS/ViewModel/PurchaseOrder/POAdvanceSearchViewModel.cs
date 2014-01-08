using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Database;
using System.Linq.Expressions;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;

namespace CPC.POS.ViewModel
{
    public class POAdvanceSearchViewModel : ViewModelBase
    {
        #region Enum

        private enum CompareType
        {
            GreaterThan = 0,
            LessThan = 1,
            Equal = 2
        }

        #endregion

        #region Fields

        private CollectionBase<base_GuestModel> _vendorCollection;

        #endregion

        #region Constructors

        public POAdvanceSearchViewModel(CollectionBase<base_GuestModel> vendorCollection)
        {
            _vendorCollection = vendorCollection;
            if (_vendorCollection == null)
            {
                _vendorCollection = new CollectionBase<base_GuestModel>();
            }
        }

        #endregion

        #region Properties

        #region Predicate

        /// <summary>
        /// Gets Predicate.
        /// </summary>
        private Expression<Func<base_PurchaseOrder, bool>> _Predicate;
        public Expression<Func<base_PurchaseOrder, bool>> Predicate
        {
            get
            {
                return _Predicate;
            }
        }

        #endregion

        #region Barcode

        private string _barcode;
        /// <summary>
        /// Gets or sets Barcode.
        /// </summary>
        public string Barcode
        {
            get
            {
                return _barcode;
            }
            set
            {
                if (_barcode != value)
                {
                    _barcode = value;
                    OnPropertyChanged(() => Barcode);
                }
            }
        }

        #endregion

        #region PurchaseOrderNo

        private string _purchaseOrderNo;
        /// <summary>
        /// Gets or sets PurchaseOrderNo.
        /// </summary>
        public string PurchaseOrderNo
        {
            get
            {
                return _purchaseOrderNo;
            }
            set
            {
                if (_purchaseOrderNo != value)
                {
                    _purchaseOrderNo = value;
                    OnPropertyChanged(() => PurchaseOrderNo);
                }
            }
        }

        #endregion

        #region PurchasedDateFrom

        private DateTime? _purchasedDateFrom;
        /// <summary>
        /// Gets or sets PurchasedDateFrom.
        /// </summary>
        public DateTime? PurchasedDateFrom
        {
            get
            {
                return _purchasedDateFrom;
            }
            set
            {
                if (_purchasedDateFrom != value)
                {
                    _purchasedDateFrom = value;
                    OnPropertyChanged(() => PurchasedDateFrom);
                }
            }
        }

        #endregion

        #region PurchasedDateTo

        private DateTime? _purchasedDateTo;
        /// <summary>
        /// Gets or sets PurchasedDateTo.
        /// </summary>
        public DateTime? PurchasedDateTo
        {
            get
            {
                return _purchasedDateTo;
            }
            set
            {
                if (_purchasedDateTo != value)
                {
                    _purchasedDateTo = value;
                    OnPropertyChanged(() => PurchasedDateTo);
                }
            }
        }

        #endregion

        #region VendorName

        private string _vendorName;
        /// <summary>
        /// Gets or sets VendorName.
        /// </summary>
        public string VendorName
        {
            get
            {
                return _vendorName;
            }
            set
            {
                if (_vendorName != value)
                {
                    _vendorName = value;
                    OnPropertyChanged(() => VendorName);
                }
            }
        }

        #endregion

        #region PurchaseStatus

        private List<ComboItem> _purchaseStatus;
        public List<ComboItem> PurchaseStatus
        {
            get
            {
                if (_purchaseStatus == null)
                {
                    _purchaseStatus = new List<ComboItem>(Common.PurchaseStatus);
                    _purchaseStatus.Insert(0, new ComboItem
                    {
                        Text = string.Empty,
                        Value = 0
                    });
                }
                return _purchaseStatus;
            }
        }

        #endregion

        #region Status

        private short _status;
        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public short Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(() => Status);
                }
            }
        }

        #endregion

        #region TotalCompareType

        private int _totalCompareType;
        /// <summary>
        /// Gets or sets TotalCompareType.
        /// </summary>
        public int TotalCompareType
        {
            get
            {
                return _totalCompareType;
            }
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

        private decimal _total;
        /// <summary>
        /// Gets or sets Total.
        /// </summary>
        public decimal Total
        {
            get
            {
                return _total;
            }
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

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' button clicked, command will executes.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(SaveExecute, CanSaveExecute);
                }
                return _saveCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// When 'Cancel' button clicked, command will executes.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SaveExecute

        /// <summary>
        /// Save.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Determine SaveExecute method can execute or not.
        /// </summary>
        /// <returns>True is execute.</returns>
        private bool CanSaveExecute()
        {
            return true;
        }

        #endregion

        #region CancelExecute

        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #endregion

        #region Private Methods

        #region Save

        /// <summary>
        /// Save.
        /// </summary>
        private void Save()
        {
            try
            {
                base_ProductRepository productRepository = new base_ProductRepository();
                base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                _Predicate = PredicateBuilder.True<base_PurchaseOrder>();
                _Predicate = _Predicate.And(x => !x.IsPurge);

                // POCard.
                if (!string.IsNullOrWhiteSpace(_barcode))
                {
                    _Predicate = _Predicate.And(x => x.POCard != null && x.POCard.ToLower().Contains(_barcode.ToLower()));
                }

                // PurchaseOrderNo.
                if (!string.IsNullOrWhiteSpace(_purchaseOrderNo))
                {
                    _Predicate = _Predicate.And(x => x.PurchaseOrderNo != null && x.PurchaseOrderNo.ToLower().Contains(_purchaseOrderNo.ToLower()));
                }

                // PurchasedDate.
                if (_purchasedDateFrom.HasValue && _purchasedDateTo.HasValue)
                {
                    DateTime from = _purchasedDateFrom.Value.Date;
                    DateTime to = new DateTime(_purchasedDateTo.Value.Year, _purchasedDateTo.Value.Month, _purchasedDateTo.Value.Day, 23, 59, 59);
                    _Predicate = _Predicate.And(x => x.PurchasedDate >= from && x.PurchasedDate <= to);
                }
                else if (_purchasedDateFrom.HasValue)
                {
                    DateTime from = _purchasedDateFrom.Value.Date;
                    _Predicate = _Predicate.And(x => x.PurchasedDate >= from);
                }
                else if (_purchasedDateTo.HasValue)
                {
                    DateTime to = new DateTime(_purchasedDateTo.Value.Year, _purchasedDateTo.Value.Month, _purchasedDateTo.Value.Day, 23, 59, 59);
                    _Predicate = _Predicate.And(x => x.PurchasedDate <= to);
                }

                // Vendor company name.
                if (!string.IsNullOrWhiteSpace(_vendorName))
                {
                    IEnumerable<string> vendorIDList = _vendorCollection.Where(x =>
                              x.Mark == MarkType.Vendor.ToDescription() &&
                              x.Company != null &&
                              x.Company.ToLower().Contains(_vendorName.ToLower())).Select(x => x.Resource.ToString());
                    _Predicate = _Predicate.And(x => vendorIDList.Contains(x.VendorResource));
                }

                // Total.
                if (Total != 0)
                {
                    if (_totalCompareType == (int)CompareType.GreaterThan)
                    {
                        _Predicate = _Predicate.And(x => x.Total > _total);
                    }
                    else if (_totalCompareType == (int)CompareType.LessThan)
                    {
                        _Predicate = _Predicate.And(x => x.Total < _total);
                    }
                    else if (_totalCompareType == (int)CompareType.Equal)
                    {
                        _Predicate = _Predicate.And(x => x.Total == _total);
                    }
                }

                // Status.
                if (_status != (short)CPC.POS.PurchaseStatus.None)
                {
                    _Predicate = _Predicate.And(x => x.Status == _status);
                }

                FindOwnerWindow(this).DialogResult = true;
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            _Predicate = null;
            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

        #endregion

        #region WriteLog

        private void WriteLog(Exception exception)
        {
            _log4net.Error(string.Format("Message: {0}. Source: {1}.", exception.Message, exception.Source));
            if (exception.InnerException != null)
            {
                _log4net.Error(exception.InnerException.ToString());
            }
        }

        #endregion
    }
}
