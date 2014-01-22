using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class CashInViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Determine whether change drawer.
        /// </summary>
        private bool _hasChangeDrawer = false;

        #endregion

        #region Constructors

        public CashInViewModel()
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

        #region DrawerCollection

        private CollectionBase<DrawerModel> _drawerCollection;
        /// <summary>
        /// Gets or sets DrawerCollection.
        /// </summary>
        public CollectionBase<DrawerModel> DrawerCollection
        {
            get
            {
                return _drawerCollection;
            }
            set
            {
                if (_drawerCollection != value)
                {
                    _drawerCollection = value;
                    OnPropertyChanged(() => DrawerCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// Save reminder.
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
            if (!_hasChangeDrawer && !_cashFlowModel.IsDirty && !_cashCollection.IsDirty)
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
                case "OpenOtherCashTotal":
                    CalculateCashTotal();
                    break;
            }
        }

        #endregion

        #region DrawerModelPropertyChanged

        private void DrawerModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DrawerModel drawerModel = sender as DrawerModel;
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!_hasChangeDrawer)
                    {
                        _hasChangeDrawer = true;
                    }
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
                        IsDirty = false
                    };
                }
                else
                {
                    // Creates a new CashFlowModel.
                    CashFlowModel = new base_CashFlowModel()
                    {
                        CashierName = Define.USER.LoginName,
                        CashierResource = Define.USER.UserResource,
                        IsDirty = false
                    };
                }
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
                        if (cashFlow != null)
                        {
                            #region Select

                            switch (cashModel.Index)
                            {
                                case 1:
                                    cashModel.Count = _cashFlowModel.O1;
                                    break;
                                case 2:
                                    cashModel.Count = _cashFlowModel.O2;
                                    break;
                                case 3:
                                    cashModel.Count = _cashFlowModel.O3;
                                    break;
                                case 4:
                                    cashModel.Count = _cashFlowModel.O4;
                                    break;
                                case 5:
                                    cashModel.Count = _cashFlowModel.O5;
                                    break;
                                case 6:
                                    cashModel.Count = _cashFlowModel.O6;
                                    break;
                                case 7:
                                    cashModel.Count = _cashFlowModel.O7;
                                    break;
                                case 8:
                                    cashModel.Count = _cashFlowModel.O8;
                                    break;
                                case 9:
                                    cashModel.Count = _cashFlowModel.O9;
                                    break;
                                case 10:
                                    cashModel.Count = _cashFlowModel.O10;
                                    break;
                                case 11:
                                    cashModel.Count = _cashFlowModel.O11;
                                    break;
                                case 12:
                                    cashModel.Count = _cashFlowModel.O12;
                                    break;
                                case 13:
                                    cashModel.Count = _cashFlowModel.O13;
                                    break;
                                case 14:
                                    cashModel.Count = _cashFlowModel.O14;
                                    break;
                                case 15:
                                    cashModel.Count = _cashFlowModel.O15;
                                    break;
                            }

                            #endregion
                        }
                        cashModel.IsDirty = false;
                        // Register CashModelPropertyChanged to calculate CashTotal.
                        cashModel.PropertyChanged += CashModelPropertyChanged;
                        _cashCollection.Add(cashModel);
                    }
                }

                // Gets drawer list.
                List<ComboItem> drawerList = Common.DrawerList.OrderBy(x => x.Value).ToList();
                if (drawerList.Any())
                {
                    DrawerCollection = new CollectionBase<DrawerModel>();
                    DrawerModel drawerModel;
                    foreach (ComboItem drawerItem in drawerList)
                    {
                        drawerModel = new DrawerModel();
                        drawerModel.Id = drawerItem.Value;
                        drawerModel.Text = drawerItem.Text;
                        drawerModel.IsDirty = false;
                        // Register DrawerModelPropertyChanged to track change on Drawer's IsDirty property.
                        drawerModel.PropertyChanged += DrawerModelPropertyChanged;
                        _drawerCollection.Add(drawerModel);
                    }

                    // Select drawer match with DrawerNo.
                    drawerModel = _drawerCollection.FirstOrDefault(x => x.Id == _cashFlowModel.DrawerNo);
                    if (drawerModel != null)
                    {
                        drawerModel.IsChecked = true;
                    }
                    else
                    {
                        _drawerCollection.First().IsChecked = true;
                    }
                    _hasChangeDrawer = false;
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
                base_CashFlowRepository cashFlowRepository = new base_CashFlowRepository();
                _cashFlowModel.IsCashOut = false;
                _cashFlowModel.OpenDate = DateTime.Now;
                _cashFlowModel.DrawerNo = _drawerCollection.First(x => x.IsChecked).Id;
                if (Define.CONFIGURATION.IsAllowShift)
                {
                    _cashFlowModel.Shift = Define.ShiftCode;
                }

                foreach (CashModel cashModel in _cashCollection)
                {
                    #region Select

                    switch (cashModel.Index)
                    {
                        case 1:
                            _cashFlowModel.O1 = cashModel.Count;
                            break;
                        case 2:
                            _cashFlowModel.O2 = cashModel.Count;
                            break;
                        case 3:
                            _cashFlowModel.O3 = cashModel.Count;
                            break;
                        case 4:
                            _cashFlowModel.O4 = cashModel.Count;
                            break;
                        case 5:
                            _cashFlowModel.O5 = cashModel.Count;
                            break;
                        case 6:
                            _cashFlowModel.O6 = cashModel.Count;
                            break;
                        case 7:
                            _cashFlowModel.O7 = cashModel.Count;
                            break;
                        case 8:
                            _cashFlowModel.O8 = cashModel.Count;
                            break;
                        case 9:
                            _cashFlowModel.O9 = cashModel.Count;
                            break;
                        case 10:
                            _cashFlowModel.O10 = cashModel.Count;
                            break;
                        case 11:
                            _cashFlowModel.O11 = cashModel.Count;
                            break;
                        case 12:
                            _cashFlowModel.O12 = cashModel.Count;
                            break;
                        case 13:
                            _cashFlowModel.O13 = cashModel.Count;
                            break;
                        case 14:
                            _cashFlowModel.O14 = cashModel.Count;
                            break;
                        case 15:
                            _cashFlowModel.O15 = cashModel.Count;
                            break;
                    }

                    #endregion
                }
                if (_cashFlowModel.IsNew)
                {
                    // Insert.
                    _cashFlowModel.UserCreated = Define.USER.LoginName;
                    _cashFlowModel.DateCreated = DateTime.Now;
                    _cashFlowModel.UserUpdated = Define.USER.LoginName;
                    _cashFlowModel.DateUpdated = DateTime.Now;

                    _cashFlowModel.ToEntity();
                    cashFlowRepository.Add(_cashFlowModel.base_CashFlow);
                }
                else
                {
                    // Update.
                    _cashFlowModel.UserUpdated = Define.USER.LoginName;
                    _cashFlowModel.DateUpdated = DateTime.Now;

                    _cashFlowModel.ToEntity();
                }

                cashFlowRepository.Commit();

                // Reset dirty nature.
                if (_hasChangeDrawer)
                {
                    _hasChangeDrawer = false;
                }
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
            sum += _cashFlowModel.OpenOtherCashTotal;
            _cashFlowModel.OpenCashTotal = sum;
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