using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.Windows.Input;
using CPC.Toolkit.Command;
using System.ComponentModel;
using CPC.POS.Repository;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Report;
using System.Data;
using Npgsql;

namespace CPC.POS.ViewModel
{
    class CashOutViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public CashOutViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            Initialize();
        }

        #endregion

        #region Properties

        #region CashFlowModel

        private base_CashFlowModel _cashFlowModel;
        /// <summary>
        /// Gets or sets CashFlowModel.
        /// </summary>
        public base_CashFlowModel CashFlowModel
        {
            get
            {
                return _cashFlowModel;
            }
            set
            {
                if (_cashFlowModel != value)
                {
                    _cashFlowModel = value;
                    OnPropertyChanged(() => CashFlowModel);
                }
            }
        }

        #endregion

        #region CashCollection

        private CollectionBase<CashModel> _cashCollection;
        /// <summary>
        /// Gets or sets CashCollection.
        /// </summary>
        public CollectionBase<CashModel> CashCollection
        {
            get
            {
                return _cashCollection;
            }
            set
            {
                if (_cashCollection != value)
                {
                    _cashCollection = value;
                    OnPropertyChanged(() => CashCollection);
                }
            }
        }

        #endregion

        #region CreditCollection

        private CollectionBase<base_ResourcePaymentDetailModel> _creditCollection;
        /// <summary>
        /// Gets or sets CreditCollection.
        /// </summary>
        public CollectionBase<base_ResourcePaymentDetailModel> CreditCollection
        {
            get
            {
                return _creditCollection;
            }
            set
            {
                if (_creditCollection != value)
                {
                    _creditCollection = value;
                    OnPropertyChanged(() => CreditCollection);
                }
            }
        }

        #endregion

        #region VoucherCollection

        private CollectionBase<base_ResourcePaymentDetailModel> _voucherCollection;
        /// <summary>
        /// Gets or sets VoucherCollection.
        /// </summary>
        public CollectionBase<base_ResourcePaymentDetailModel> VoucherCollection
        {
            get
            {
                return _voucherCollection;
            }
            set
            {
                if (_voucherCollection != value)
                {
                    _voucherCollection = value;
                    OnPropertyChanged(() => VoucherCollection);
                }
            }
        }

        #endregion

        #region CheckCollection

        private CollectionBase<base_ResourcePaymentDetailModel> _checkCollection;
        /// <summary>
        /// Gets or sets CheckCollection.
        /// </summary>
        public CollectionBase<base_ResourcePaymentDetailModel> CheckCollection
        {
            get
            {
                return _checkCollection;
            }
            set
            {
                if (_checkCollection != value)
                {
                    _checkCollection = value;
                    OnPropertyChanged(() => CheckCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// Save the cash.
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
        /// Cancel.
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
        /// Save the cash.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Determine whether can save the cash.
        /// </summary>
        /// <returns></returns>
        private bool CanSaveExecute()
        {
            if (_cashFlowModel == null || _cashCollection == null)
            {
                return false;
            }

            if (!_cashFlowModel.IsDirty && !_cashCollection.IsDirty)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region CashModelPropertyChanged

        private void CashModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CashModel cashModel = sender as CashModel;
            switch (e.PropertyName)
            {
                case "Total":
                    CalculateCashTotal();
                    break;
            }
        }

        #endregion

        #region CashFlowModelPropertyChanged

        private void CashFlowModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_CashFlowModel cashFlowModel = sender as base_CashFlowModel;
            switch (e.PropertyName)
            {
                case "CloseOtherCashTotal":
                    CalculateCashTotal();
                    break;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Initialize()
        {
            try
            {
                base_CashFlowRepository cashFlowRepository = new base_CashFlowRepository();
                base_ResourcePaymentDetailRepository resourcePaymentDetailRepository = new base_ResourcePaymentDetailRepository();
                DateTime current = DateTime.Now.Date;
                base_CashFlow cashFlow = null;
                string shift = Define.ShiftCode;
                // Gets a base_CashFlow based on shift.
                if (!Define.CONFIGURATION.IsAllowShift)
                {
                    cashFlow = cashFlowRepository.Get(x => x.CashierResource == Define.USER.UserResource && x.OpenDate == current);
                }
                else
                {
                    cashFlow = cashFlowRepository.Get(x => x.CashierResource == Define.USER.UserResource && x.OpenDate == current && x.Shift == shift);
                }
                if (cashFlow != null)
                {
                    // Gets the base_CashFlow.
                    CashFlowModel = new base_CashFlowModel(cashFlow)
                    {
                        CashierName = Define.USER.LoginName,
                    };

                    if (CashFlowModel.IsCashOut)
                    {
                        throw new Exception("You was cash out.");
                    }
                }
                else
                {
                    throw new Exception("Cashin not found!");
                }
                CashFlowModel.CloseOtherCashTotal = 0;
                CashFlowModel.CloseCashTotal = 0;
                CashFlowModel.IsDirty = false;
                // Register CashFlowModelPropertyChanged to calculate CashTotal.
                CashFlowModel.PropertyChanged += CashFlowModelPropertyChanged;

                // Gets cash list.
                List<ComboItem> cashList = Common.CashList.OrderBy(x => x.Value).ToList();
                if (cashList.Any())
                {
                    CashCollection = new CollectionBase<CashModel>();
                    CashModel cashModel;
                    decimal value = 0;
                    foreach (ComboItem cashItem in cashList)
                    {
                        cashModel = new CashModel();
                        cashModel.Id = cashItem.Value;
                        if (decimal.TryParse(cashItem.Text, out value))
                        {
                            cashModel.Value = value;
                        }
                        cashModel.Index = cashList.IndexOf(cashItem) + 1;
                        cashModel.IsDirty = false;
                        // Register CashModelPropertyChanged to calculate CashTotal.
                        cashModel.PropertyChanged += CashModelPropertyChanged;
                        _cashCollection.Add(cashModel);
                    }
                }

                // Gets CreditCollection.
                DBHelper dbHelper = new DBHelper();
                short creditCardMethod = (short)PaymentMethod.CreditCard;
                string commandText = "sp_get_payment_method";
                NpgsqlCommand command = new NpgsqlCommand(commandText);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new NpgsqlParameter("paymentmethod_id", DbType.Int32));
                command.Parameters.Add(new NpgsqlParameter("payment_date", DbType.Date));
                command.Parameters.Add(new NpgsqlParameter("shift", DbType.String));
                command.Parameters[0].Value = creditCardMethod;
                command.Parameters[1].Value = current;
                command.Parameters[2].Value = shift;
                DataTable table = dbHelper.ExecuteQuery(command);
                CreditCollection = new CollectionBase<base_ResourcePaymentDetailModel>(table.Rows.OfType<DataRow>().Select(x => new base_ResourcePaymentDetailModel
                {
                    PaymentMethod = x.Field<string>("CardName"),
                    Paid = x.Field<decimal>("Paid"),
                    Count = x.Field<long>("Count"),
                    CardType = x.Field<short>("CardType")
                }));
                if (_creditCollection.Any())
                {
                    CashFlowModel.CreditCashTotal = _creditCollection.Sum(x => x.Paid);
                }

                // Gets VoucherCollection.
                creditCardMethod = (short)PaymentMethod.GiftCard;
                command.Parameters[0].Value = creditCardMethod;
                table = dbHelper.ExecuteQuery(command);
                VoucherCollection = new CollectionBase<base_ResourcePaymentDetailModel>(table.Rows.OfType<DataRow>().Select(x => new base_ResourcePaymentDetailModel
                {
                    PaymentMethod = x.Field<string>("CardName"),
                    Paid = x.Field<decimal>("Paid"),
                    Count = x.Field<long>("Count"),
                    CardType = x.Field<short>("CardType")
                }));

                creditCardMethod = (short)PaymentMethod.GiftCertificate;
                command.Parameters[0].Value = creditCardMethod;
                table = dbHelper.ExecuteQuery(command);
                VoucherCollection = new CollectionBase<base_ResourcePaymentDetailModel>(_voucherCollection.Union(
                    new CollectionBase<base_ResourcePaymentDetailModel>(table.Rows.OfType<DataRow>().Select(x => new base_ResourcePaymentDetailModel
                {
                    PaymentMethod = x.Field<string>("CardName"),
                    Paid = x.Field<decimal>("Paid"),
                    Count = x.Field<long>("Count"),
                    CardType = x.Field<short>("CardType")
                }))));
                if (_voucherCollection.Any())
                {
                    CashFlowModel.VoucherTotal = _voucherCollection.Sum(x => x.Paid);
                }

                // Gets CheckCollection.
                creditCardMethod = (short)PaymentMethod.Cheque;
                command.Parameters[0].Value = creditCardMethod;
                table = dbHelper.ExecuteQuery(command);
                CheckCollection = new CollectionBase<base_ResourcePaymentDetailModel>(table.Rows.OfType<DataRow>().Select(x => new base_ResourcePaymentDetailModel
                {
                    PaymentMethod = x.Field<string>("CardName"),
                    Paid = x.Field<decimal>("Paid"),
                    Count = x.Field<long>("Count"),
                    CardType = x.Field<short>("CardType")
                }));
                if (_checkCollection.Any())
                {
                    CashFlowModel.CheckTotal = _checkCollection.Sum(x => x.Paid);
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save the cash.
        /// </summary>
        private void Save()
        {
            try
            {
                if (_cashFlowModel == null)
                {
                    throw new Exception("Cashin not found!");
                }

                base_CashFlowRepository cashFlowRepository = new base_CashFlowRepository();
                _cashFlowModel.IsCashOut = true;

                foreach (CashModel cashModel in _cashCollection)
                {
                    #region Select

                    switch (cashModel.Index)
                    {
                        case 1:
                            _cashFlowModel.C1 = cashModel.Count;
                            break;
                        case 2:
                            _cashFlowModel.C2 = cashModel.Count;
                            break;
                        case 3:
                            _cashFlowModel.C3 = cashModel.Count;
                            break;
                        case 4:
                            _cashFlowModel.C4 = cashModel.Count;
                            break;
                        case 5:
                            _cashFlowModel.C5 = cashModel.Count;
                            break;
                        case 6:
                            _cashFlowModel.C6 = cashModel.Count;
                            break;
                        case 7:
                            _cashFlowModel.C7 = cashModel.Count;
                            break;
                        case 8:
                            _cashFlowModel.C8 = cashModel.Count;
                            break;
                        case 9:
                            _cashFlowModel.C9 = cashModel.Count;
                            break;
                        case 10:
                            _cashFlowModel.C10 = cashModel.Count;
                            break;
                        case 11:
                            _cashFlowModel.C11 = cashModel.Count;
                            break;
                        case 12:
                            _cashFlowModel.C12 = cashModel.Count;
                            break;
                        case 13:
                            _cashFlowModel.C13 = cashModel.Count;
                            break;
                        case 14:
                            _cashFlowModel.C14 = cashModel.Count;
                            break;
                        case 15:
                            _cashFlowModel.C15 = cashModel.Count;
                            break;
                    }

                    #endregion
                }

                _cashFlowModel.UserUpdated = Define.USER.LoginName;
                _cashFlowModel.DateUpdated = DateTime.Now;
                _cashFlowModel.ToEntity();
                cashFlowRepository.Commit();

                // Reset dirty nature.
                _cashFlowModel.IsNew = false;
                _cashFlowModel.IsDirty = false;
                foreach (CashModel cashModel in _cashCollection)
                {
                    cashModel.IsDirty = false;
                }

                Close(true);
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
            Close(false);
        }

        #endregion

        #region Close

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Close(bool result)
        {
            FindOwnerWindow(this).DialogResult = result;
        }

        #endregion

        #region CalculateCashTotal

        /// <summary>
        /// Calculate CashTotal.
        /// </summary>
        private void CalculateCashTotal()
        {
            decimal sum = 0;
            foreach (CashModel cashModel in _cashCollection)
            {
                sum += cashModel.Total;
            }
            sum += _cashFlowModel.CloseOtherCashTotal;
            _cashFlowModel.CloseCashTotal = sum;
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
