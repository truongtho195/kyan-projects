using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using System.Linq.Expressions;
using CPC.POS.Database;
using System.ComponentModel;
using CPC.POS.Repository;
using CPC.Helper;
using System.Windows;
using CPC.POS.View;
using CPCToolkitExt.DataGridControl;

namespace CPC.POS.ViewModel
{
    public class LayawaySetupViewModel : ViewModelBase
    {
        #region Define
        private base_LayawayManagerRepository _layawayManagerRepository = new base_LayawayManagerRepository();
        private bool _skipCallSelectedItem = false;
        #endregion

        #region Constructors
        public LayawaySetupViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            InitialCommand();

        }
        #endregion

        #region Properties

        #region IsHiddenWarming
        private bool _isHiddenWarming;
        /// <summary>
        /// Gets or sets the IsHiddenWarming.
        /// </summary>
        public bool IsHiddenWarming
        {
            get
            {
                return _isHiddenWarming;
            }
            set
            {
                if (_isHiddenWarming != value)
                {
                    _isHiddenWarming = value;
                    OnPropertyChanged(() => IsHiddenWarming);
                    OnPropertyChanged(() => NumberColumnDisplay);
                }
            }
        }
        #endregion

        #region TotalLayaway
        private int _totalLayaway;
        /// <summary>
        /// Gets or sets the TotalLayaway.
        /// </summary>
        public int TotalLayaway
        {
            get
            {
                return _totalLayaway;
            }
            set
            {
                if (_totalLayaway != value)
                {
                    _totalLayaway = value;
                    OnPropertyChanged(() => TotalLayaway);
                }
            }
        }
        #endregion

        #region LayawayCollection
        private CollectionBase<base_LayawayManagerModel> _layawayCollection = new CollectionBase<base_LayawayManagerModel>();
        /// <summary>
        /// Gets or sets the LayawayCollection.
        /// </summary>
        public CollectionBase<base_LayawayManagerModel> LayawayCollection
        {
            get
            {
                return _layawayCollection;
            }
            set
            {
                if (_layawayCollection != value)
                {
                    _layawayCollection = value;
                    OnPropertyChanged(() => LayawayCollection);
                }
            }
        }
        #endregion

        #region SelectedLayaway
        private base_LayawayManagerModel _selectedLayaway;
        /// <summary>
        /// Gets or sets the SelectedLayaway.
        /// </summary>
        public base_LayawayManagerModel SelectedLayaway
        {
            get
            {
                return _selectedLayaway;
            }
            set
            {
                if (_selectedLayaway != value)
                {
                    _selectedLayaway = value;
                    OnPropertyChanged(() => SelectedLayaway);
                    if (SelectedLayaway != null)
                    {
                        SelectedLayaway.PropertyChanged -= new PropertyChangedEventHandler(SelectedLayaway_PropertyChanged);
                        SelectedLayaway.PropertyChanged += new PropertyChangedEventHandler(SelectedLayaway_PropertyChanged);
                    }
                }
            }
        }
        #endregion

        #region Status
        private short _status;
        /// <summary>
        /// Gets or sets the Status.
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
                    StatusChanged();
                }
            }
        }


        #endregion

        #region NumberColumnDisplay

        /// <summary>
        /// Gets the NumberColumnDisplay.
        /// </summary>
        public int NumberColumnDisplay
        {
            get
            {
                if (IsHiddenWarming)
                    return 7;
                return 8;
            }
        }
        #endregion

        #region IsSearchMode

        private bool _isSearchMode;
        /// <summary>
        /// Gets a value indicates whether search component is open.
        /// </summary>
        public bool IsSearchMode
        {
            get
            {
                return _isSearchMode;
            }
            private set
            {
                if (_isSearchMode != value)
                {
                    _isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
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

        public RelayCommand<object> NewCommand
        {
            get;
            private set;
        }



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
            CreateNewLayaway();
        }


        #endregion

        #region SaveCommand
        /// <summary>
        /// Gets the Save Command.
        /// <summary>

        public RelayCommand<object> SaveCommand
        {
            get;
            private set;
        }



        /// <summary>
        /// Method to check whether the Save command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute(object param)
        {
            if (SelectedLayaway == null)
                return false;
            return IsValid && SelectedLayaway.IsDirty;
        }


        /// <summary>
        /// Method to invoke when the Save command is executed.
        /// </summary>
        private void OnSaveCommandExecute(object param)
        {
            try
            {
                if (SelectedLayaway.EndDate <= DateTime.Now.Date)
                {
                    _status = (short)StatusBasic.Deactive;
                    OnPropertyChanged(() => Status);
                }
                SelectedLayaway.Status = Status;
                SelectedLayaway.ToEntity();
                CheckConflict(SelectedLayaway);
                ShowWarningConflict(SelectedLayaway);
                ShowHideWarming();
                if (SelectedLayaway.IsNew)
                {
                    _layawayManagerRepository.Add(SelectedLayaway.base_LayawayManager);
                    _layawayManagerRepository.Commit();
                    _skipCallSelectedItem = true;
                    LayawayCollection.Add(SelectedLayaway);
                    _skipCallSelectedItem = false;
                }
                else
                {
                    _layawayManagerRepository.Commit();
                }

                //Set Id 
                SelectedLayaway.Id = SelectedLayaway.base_LayawayManager.Id;
                SelectedLayaway.EndUpdate();
                SetTextDisplay(SelectedLayaway);
                ShowHideWarming();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Gets the Delete Command.
        /// <summary>

        public RelayCommand<object> DeleteCommand
        {
            get;
            private set;
        }



        /// <summary>
        /// Method to check whether the Delete command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute(object param)
        {
            if (SelectedLayaway == null)
                return false;
            return SelectedLayaway.Status.Equals((short)StatusBasic.Deactive);
        }


        /// <summary>
        /// Method to invoke when the Delete command is executed.
        /// </summary>
        private void OnDeleteCommandExecute(object param)
        {
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to delete this item?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result.Is(MessageBoxResult.Yes))
                {
                    _layawayManagerRepository.Delete(SelectedLayaway.base_LayawayManager);
                    _layawayManagerRepository.Commit();

                    base_LayawayManagerModel layawayDeleted = LayawayCollection.SingleOrDefault(x => x.Resource.Equals(SelectedLayaway.Resource));

                    if (layawayDeleted != null)
                    {
                        LayawayCollection.Remove(layawayDeleted);
                    }

                    if (LayawayCollection.Any())
                        SelectedLayaway = LayawayCollection.FirstOrDefault();
                    else
                        CreateNewLayaway();

                    TotalLayaway--;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
        #endregion

        #region SelectedItemCommand
        /// <summary>
        /// Gets the SelectedItem Command.
        /// <summary>

        public RelayCommand<object> SelectedItemCommand
        {
            get;
            private set;
        }



        /// <summary>
        /// Method to check whether the SelectedItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSelectedItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the SelectedItem command is executed.
        /// </summary>
        private void OnSelectedItemCommandExecute(object param)
        {
            if (_skipCallSelectedItem)
                return;

            DataGridControl dg = param as DataGridControl;
            SelectedLayaway = dg.SelectedItem as base_LayawayManagerModel;
            if (_selectedLayaway != null)
            {
                _status = SelectedLayaway.Status;
                OnPropertyChanged(() => Status);
            }
            //MessageBoxResult resultInfo;
            //if (ChangeViewExecute(null, out resultInfo))
            //{
            //    if (dg.SelectedItem != null)
            //    {
            //        SelectedLayaway = dg.SelectedItem as base_LayawayManagerModel;
            //        _status = SelectedLayaway.Status;
            //        OnPropertyChanged(() => Status);
            //    }
            //    else
            //    {
            //        CreateNewLayaway();
            //    }
            //}
            //else
            //{
            //    if (resultInfo.Is(MessageBoxResult.Cancel))
            //    {
            //        _skipCallSelectedItem = true;
            //        dg.SelectedItem = null;
            //        dg.SelectedItem = SelectedLayaway;

            //        _skipCallSelectedItem = false;
            //        dg.UpdateLayout();
            //    }

            //}
        }


        #endregion

        #region StatusChangedCommand
        /// <summary>
        /// Gets the StatusChanged Command.
        /// <summary>

        public RelayCommand<object> StatusChangedCommand
        {
            get;
            private set;
        }



        /// <summary>
        /// Method to check whether the StatusChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnStatusChangedCommandCanExecute(object param)
        {

            return true;
        }


        /// <summary>
        /// Method to invoke when the StatusChanged command is executed.
        /// </summary>
        private void OnStatusChangedCommandExecute(object param)
        {
            ReasonViewModel reasonViewModel = new ReasonViewModel();
            bool? dialogResult = _dialogService.ShowDialog<ReasonView>(_ownerViewModel, reasonViewModel, Language.GetMsg("SO_Title_Reason"));
        }
        #endregion

        #region OpenSearchComponentCommand

        private RelayCommand _openSearchComponentCommand;
        /// <summary>
        /// When 'Search' Button clicked, OpenSearchComponentCommand will executes.
        /// </summary>
        public RelayCommand OpenSearchComponentCommand
        {
            get
            {
                if (_openSearchComponentCommand == null)
                {
                    _openSearchComponentCommand = new RelayCommand(OpenSearchComponentExecute);
                }
                return _openSearchComponentCommand;
            }
        }

        #region OpenSearchComponentExecute

        /// <summary>
        /// Open search component.
        /// </summary>
        private void OpenSearchComponentExecute()
        {
            OpenSearchComponent();
        }

        #endregion

        #endregion

        #region CloseSearchComponentCommand

        private RelayCommand _closeSearchComponentCommand;
        /// <summary>
        /// When double clicked on DataGridRow in DataGrid, CloseSearchComponentCommand will executes.
        /// </summary>
        public RelayCommand CloseSearchComponentCommand
        {
            get
            {
                if (_closeSearchComponentCommand == null)
                {
                    _closeSearchComponentCommand = new RelayCommand(CloseSearchComponentExecute, CanCloseSearchComponentExecute);
                }
                return _closeSearchComponentCommand;
            }
        }

        #region CloseSearchComponentExecute

        /// <summary>
        /// Close search component.
        /// </summary>
        private void CloseSearchComponentExecute()
        {
            CloseSearchComponent();
        }

        #endregion

        #region CanCloseSearchComponentExecute

        /// <summary>
        /// Determine whether can call CloseSearchComponentExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanCloseSearchComponentExecute()
        {
            return true;
        }

        #endregion

        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            NewCommand = new RelayCommand<object>(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SelectedItemCommand = new RelayCommand<object>(OnSelectedItemCommandExecute, OnSelectedItemCommandCanExecute);
            StatusChangedCommand = new RelayCommand<object>(OnStatusChangedCommandExecute, OnStatusChangedCommandCanExecute);
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(Expression<Func<base_LayawayManager, bool>> predicate, bool refreshData = false)
        {

            BackgroundWorker bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            LayawayCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;
                //Cout all Customer in Data base show on grid
                TotalLayaway = _layawayManagerRepository.GetIQueryable(predicate).Count();


                //Get All data
                IList<base_LayawayManager> layaways = _layawayManagerRepository.GetAll(predicate);
                _layawayManagerRepository.Refresh(layaways);
                foreach (base_LayawayManager customer in layaways)
                {
                    bgWorker.ReportProgress(0, customer);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_LayawayManagerModel layawayModel = new base_LayawayManagerModel((base_LayawayManager)e.UserState);
                SetDataToModel(layawayModel);
                if (layawayModel.Status == (short)StatusBasic.Active)
                {
                    LayawayCollection.Insert(0, layawayModel);
                }
                else
                {
                    LayawayCollection.Add(layawayModel);
                }
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (LayawayCollection.Any())
                {

                    SelectedLayaway = LayawayCollection.FirstOrDefault();

                    if (SelectedLayaway != null && SelectedLayaway.EndDate <= DateTime.Now.Date && _selectedLayaway.Status == (short)StatusBasic.Active)
                    {
                        _status = (short)StatusBasic.Deactive;
                        OnPropertyChanged(() => Status);
                        SelectedLayaway.Status = Status;
                        SelectedLayaway.ToEntity();
                        _layawayManagerRepository.Commit();
                        SelectedLayaway.EndUpdate();
                        SetTextDisplay(SelectedLayaway);
                    }
                    else
                    {
                        if (SelectedLayaway != null)
                        {
                            _status = SelectedLayaway.Status;
                            OnPropertyChanged(() => Status);
                        }
                    }
                }
                else
                {
                    CreateNewLayaway();
                }

                IEnumerable<base_LayawayManagerModel> activedLayaways = LayawayCollection.Where(x => x.Status.Equals((short)StatusBasic.Active));
                if (activedLayaways.Count() > 1)
                {
                    foreach (base_LayawayManagerModel item in activedLayaways)
                    {
                        item.IsConflict = true;
                    }
                }

                ShowHideWarming();
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        private void SetDataToModel(base_LayawayManagerModel layawayModel)
        {
            if (layawayModel != null)
            {
                layawayModel.SetEndDateTemp();
                SetTextDisplay(layawayModel);
                layawayModel.IsDirty = false;
            }
        }

        private void SetTextDisplay(base_LayawayManagerModel layawayModel)
        {
            //Set Text For Status
            ComboItem statusItem = Common.StatusBasic.SingleOrDefault(x => x.Value.Equals((short)layawayModel.Status));
            if (statusItem != null)
                layawayModel.StatusItem = statusItem;
            else
                layawayModel.StatusItem = new ComboItem
                {
                    Text = string.Empty
                };

            //Set text PaymentSchedule
            SetPaymentScheduleName(layawayModel);
        }



        private void CreateNewLayaway()
        {
            _selectedLayaway = new base_LayawayManagerModel();
            _selectedLayaway.Resource = Guid.NewGuid();
            _selectedLayaway.Status = Common.StatusBasic.First().Value;
            //_selectedLayaway.PaymentScheduleType = 0;
            _selectedLayaway.StartDate = DateTime.Now;
            _selectedLayaway.EndDate = _selectedLayaway.StartDate.AddHours(Define.CONFIGURATION.DefautlDiscountScheduleTime);
            //_selectedLayaway.DepositUnit = 0;
            //_selectedLayaway.FeeUnit = 0;
            _selectedLayaway.DateCreated = DateTime.Now;
            _selectedLayaway.DateUpdated = DateTime.Now;
            _status = _selectedLayaway.Status;
            _selectedLayaway.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SetTextDisplay(_selectedLayaway);
            _selectedLayaway.IsDirty = false;
            OnPropertyChanged(() => Status);
            OnPropertyChanged(() => SelectedLayaway);
            IsSearchMode = false;
        }

        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing, out MessageBoxResult resultInfo)
        {
            bool result = true;
            MessageBoxResult msgResult = MessageBoxResult.None;
            if (this.SelectedLayaway != null && this.SelectedLayaway.IsDirty)
            {
                //Some data has changed. Do you want to save?
                if (!isClosing.HasValue)
                    msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M106"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                else
                    msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M106"), Language.GetMsg("POSCaption"), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);

                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                    {

                        OnSaveCommandExecute(null);
                    }
                    else //Has Error
                        result = false;
                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (!this.SelectedLayaway.IsNew)
                    {
                        //Old Item Rollback data
                        SelectedLayaway.ToModelAndRaise();
                        SetDataToModel(SelectedLayaway);
                    }
                }
                else
                    result = false;

            }
            resultInfo = msgResult;
            return result;
        }

        private void CheckConflict(base_LayawayManagerModel layawayModel)
        {
            if (layawayModel.Status.Equals((short)StatusBasic.Active))
            {
                short statusActived = (short)StatusBasic.Active;
                int itemConflict = LayawayCollection.Count(x => !x.Resource.Equals(layawayModel.Resource) && x.Status.Equals(statusActived));
                if (itemConflict > 0)
                {
                    if (itemConflict == 1)
                    {
                        base_LayawayManagerModel layawayActived = LayawayCollection.SingleOrDefault(x => !x.Resource.Equals(layawayModel.Resource) && x.Status.Equals((short)StatusBasic.Active));
                        layawayActived.IsConflict = true;
                    }

                    layawayModel.IsConflict = true;
                }
            }
            else
            {
                CheckRemoveConflict(layawayModel);
            }
        }

        /// <summary>
        /// Item is not actived
        /// </summary>
        /// <param name="layawayModel"></param>
        private void CheckRemoveConflict(base_LayawayManagerModel layawayModel)
        {
            if (layawayModel.Status.Equals((short)StatusBasic.Deactive) && layawayModel.IsConflict)
            {
                layawayModel.IsConflict = false;
                IEnumerable<base_LayawayManagerModel> conflictItems = LayawayCollection.Where(x => x.IsConflict);
                if (conflictItems.Count() == 1)
                {
                    base_LayawayManagerModel remainItem = LayawayCollection.SingleOrDefault(x => x.IsConflict);
                    remainItem.IsConflict = false;
                }
            }
        }

        /// <summary>
        /// Show warning conflict
        /// </summary>
        /// <param name="promotionModel"></param>
        private void ShowWarningConflict(base_LayawayManagerModel layawayModel)
        {
            if (layawayModel.IsConflict)
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("One or more items on the layaway are listed or other active layaway. The layaway defined may not be applied.", "POS", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }

        private void ShowHideWarming()
        {
            IsHiddenWarming = LayawayCollection.Count(x => x.IsConflict) == 0;
        }

        /// <summary>
        /// Flag Status Changed
        /// </summary>
        private void StatusChanged()
        {
            if (SelectedLayaway != null && !SelectedLayaway.IsNew && Status.Equals((short)StatusBasic.Active))
            {
                ReasonViewModel reasonViewModel = new ReasonViewModel(SelectedLayaway.ReasonReActive);
                bool? dialogResult = _dialogService.ShowDialog<ReasonView>(_ownerViewModel, reasonViewModel, Language.GetMsg("SO_Title_Reason"));
                if (dialogResult == true)
                {
                    SelectedLayaway.ReasonReActive = reasonViewModel.Reason;
                }
                else
                {
                    App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                    {
                        Status = (short)StatusBasic.Deactive;
                    });
                }
            }
            else
                SelectedLayaway.IsDirty = true;
        }

        private void SetPaymentScheduleName(base_LayawayManagerModel layawayModel)
        {
            //ComboItem paymentScheduleItem = Common.PaymentSchedule.SingleOrDefault(x => x.Value.Equals((short)layawayModel.PaymentScheduleType));
            //if (paymentScheduleItem != null)
            //    layawayModel.PaymentScheduleItem = paymentScheduleItem;
            //else
            //    layawayModel.PaymentScheduleItem = new ComboItem() { Text = string.Empty};
        }

        #region OpenSearchComponent

        /// <summary>
        /// Open search component.
        /// </summary>
        private void OpenSearchComponent()
        {
            MessageBoxResult resultInfo;
            if (ChangeViewExecute(null, out resultInfo))
            {
                if (_selectedLayaway != null)
                {
                    _status = SelectedLayaway.Status;
                    OnPropertyChanged(() => Status);
                }
                else
                {
                    CreateNewLayaway();
                }

                IsSearchMode = true;
            }
        }

        #endregion

        #region CloseSearchComponent

        /// <summary>
        /// Close search component.
        /// </summary>
        private void CloseSearchComponent()
        {
            try
            {
                IsSearchMode = false;

                if (SelectedLayaway.EndDate <= DateTime.Now.Date && _selectedLayaway.Status == (short)StatusBasic.Active)
                {
                    _status = (short)StatusBasic.Deactive;
                    OnPropertyChanged(() => Status);
                    SelectedLayaway.Status = Status;
                    SelectedLayaway.ToEntity();
                    _layawayManagerRepository.Commit();
                    SelectedLayaway.EndUpdate();
                    SetTextDisplay(SelectedLayaway);
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #endregion

        #region PropertyChanged
        private void SelectedLayaway_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base_LayawayManagerModel layawayModel = sender as base_LayawayManagerModel;
            switch (e.PropertyName)
            {
                case "PaymentScheduleType":
                    SetPaymentScheduleName(layawayModel);
                    break;
                case "PaymentSchedule":

                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Override Methods

        public override void LoadData()
        {
            base.LoadData();
            Expression<Func<base_LayawayManager, bool>> predicate = PredicateBuilder.True<base_LayawayManager>();
            LoadDataByPredicate(predicate);
        }


        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            MessageBoxResult resultInfo;
            return ChangeViewExecute(isClosing, out resultInfo);
        }
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
