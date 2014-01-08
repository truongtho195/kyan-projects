using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class ManagementUserLogViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand<object> ConnectedCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand DoubleClickViewCommand { get; private set; }

        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_ResourceAccountRepository _resourceAccountRepository = new base_ResourceAccountRepository();
        private base_UserLogRepository _userLogRepository = new base_UserLogRepository();
        private int _numberOfLogins = 0;
        private string _userMarkType = MarkType.Employee.ToDescription();
        #endregion

        #region Constructors
        public ManagementUserLogViewModel()
        {
            this.InitialCommand();
            this.IsHiddenColunm = true;
        }
        #endregion

        #region Properties

        #region UserLogCollection
        /// <summary>
        /// Gets or sets the UserLogCollection.
        /// </summary>
        private ObservableCollection<base_UserLogModel> _userLogCollection = new ObservableCollection<base_UserLogModel>();
        public ObservableCollection<base_UserLogModel> UserLogCollection
        {
            get
            {
                return _userLogCollection;
            }
            set
            {
                if (_userLogCollection != value)
                {
                    _userLogCollection = value;
                    OnPropertyChanged(() => UserLogCollection);
                }
            }
        }

        #endregion

        #region SelectedItemUserLog
        /// <summary>
        /// Gets or sets the SelectedItemUserLog.
        /// </summary>
        private base_UserLogModel _selectedItemUserLog;
        public base_UserLogModel SelectedItemUserLog
        {
            get
            {
                return _selectedItemUserLog;
            }
            set
            {
                if (_selectedItemUserLog != value)
                {
                    _selectedItemUserLog = value;
                    this.OnPropertyChanged(() => SelectedItemUserLog);
                }
            }
        }
        #endregion

        #region TotalUsers
        private int _totalUsers;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalUsers
        {

            get
            {
                return _totalUsers;
            }
            set
            {
                if (_totalUsers != value)
                {
                    _totalUsers = value;
                    OnPropertyChanged(() => TotalUsers);
                }
            }
        }
        #endregion

        #region CurrentPageIndex
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get
            {
                return _currentPageIndex;
            }
            set
            {
                if (value != _currentPageIndex)
                {
                    _currentPageIndex = value;
                    OnPropertyChanged(() => CurrentPageIndex);
                }
            }
        }

        #endregion

        #region IsHiddenColunm
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private bool _isHiddenColunm = false;
        public bool IsHiddenColunm
        {
            get
            {
                return _isHiddenColunm;
            }
            set
            {
                if (value != _isHiddenColunm)
                {
                    _isHiddenColunm = value;
                    OnPropertyChanged(() => IsHiddenColunm);
                }
            }
        }

        #endregion

        #endregion

        #region Commands Methods

        #region ConnectedCommand
        /// <summary>
        /// Method to check whether the ConnectedCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnConnectedCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ConnectedCommand is executed.
        /// </summary>
        private void OnConnectedCommandExecute(object param)
        {
            // TODO: Handle command logic here
            try
            {
                if (param != null && param is base_UserLogModel)
                {
                    base_UserLogModel userLogModel = param as base_UserLogModel;
                    userLogModel.DisConnectedOn = DateTimeExt.Now;
                    userLogModel.IsDisconected = true;
                    this.UpdateUserLog(userLogModel);
                    userLogModel.IsDirty = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update UserLog" + ex.ToString());
            }
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return true;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region SearchCommand
        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute()
        {
            return true;
        }

        private void OnSearchCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion

        #region DoubleClickCommand

        /// <summary>
        /// Method to check whether the DoubleClick command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the DoubleClick command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute()
        {

        }
        #endregion

        #region LoadDataByStepCommand

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
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            this.LoadUser(this.CurrentPageIndex);
        }
        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            // Route the commands
            this.ConnectedCommand = new RelayCommand<object>(this.OnConnectedCommandExecute, this.OnConnectedCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            this.UserLogCollection.Clear();
            this.LoadUser(this.CurrentPageIndex);
        }

        private void LoadUser(int currentIndex)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            bgWorker.DoWork += (sender, e) =>
            {
                base.IsBusy = true;
                //if(refreshData)
                //this.RefreshData();
                //To count all User in Data base show on grid
                this.TotalUsers = _userLogRepository.GetIQueryable().Count();
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * 30;
                IList<base_UserLog> userLogs = _userLogRepository.GetRange<DateTime>(indexItem, 30, x => x.ConnectedOn, x => true);
                //.GetIQueryable().OrderByDescending(x => x.ConnectedOn)
                foreach (var item in userLogs)
                    bgWorker.ReportProgress(0, item);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_UserLog _userLog = e.UserState as base_UserLog;
                Guid userResource = Guid.Parse(_userLog.ResourceAccessed);
                base_Guest _user = _guestRepository.GetIQueryable(x => x.Resource.HasValue && x.Resource.Value == userResource).SingleOrDefault();
                base_ResourceAccount resourceAccount = _resourceAccountRepository.GetIQueryable(x => x.UserResource == _userLog.ResourceAccessed).SingleOrDefault();
                base_UserLogModel userLogModel;
                //To get data of base_Guest.
                userLogModel = new base_UserLogModel(_userLog);
                if (_user != null)
                    userLogModel.UserName = string.Format("{0} {1}", _user.FirstName, _user.LastName);
                if (resourceAccount != null)
                    userLogModel.LoginName = resourceAccount.LoginName;
                //To add item.
                userLogModel.IsDirty = false;
                this.UserLogCollection.Add(userLogModel);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                base.IsBusy = false;
                //this.UserLogCollection = new ObservableCollection<base_UserLogModel>(this.UserLogCollection.OrderBy(x => x.ConnectedOn));
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// To update data on base_UserLog table.
        /// </summary>
        private void UpdateUserLog(base_UserLogModel userLogModel)
        {
            base_UserLog userLog = _userLogRepository.GetIEnumerable(x => x.ResourceAccessed == userLogModel.ResourceAccessed && x.IsDisconected.HasValue && !x.IsDisconected.Value).SingleOrDefault();
            if (userLog != null)
            {
                userLog.DisConnectedOn = userLogModel.DisConnectedOn;
                userLog.IsDisconected = true;
                _userLogRepository.Commit();
            }

        }
        #endregion

        #region Public Methods
        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            //this.UserLogCollection.Clear();
            //this.LoadUser(this.CurrentPageIndex);
        }
        #endregion
        #endregion
    }
}