using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using SecurityLib;

namespace CPC.POS.ViewModel
{
    class UserListViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand DoubleClickViewCommand { get; private set; }
        //Repository
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_ResourceAccountRepository _resourceAccountRepository = new base_ResourceAccountRepository();
        private base_AuthorizeRepository _authorizeRepository = new base_AuthorizeRepository();
        private base_UserRightRepository _userRightRepository = new base_UserRightRepository();
        private string _userMarkType = MarkType.Employee.ToDescription();

        private bool _isCheckAllFlag = false;
        private bool _isCheckItemFlag = false;
        private bool _isSetIsCheckedItem = false;
        private bool _isEditIsCheckedItem = false;
        private bool _isSelectionChanged = false;
        private string _currentUserResource = string.Empty;
        private bool _cloneIsSetDefault = false;
        private ICollectionView _userRightsCollectionView;
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserListViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            this.InitialCommand();
        }

        #endregion

        #region Properties

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

        #region SelectedItemUser
        /// <summary>
        /// Gets or sets the SelectedItemUser.
        /// </summary>
        private base_ResourceAccountModel _selectedItemUser = null;
        public base_ResourceAccountModel SelectedItemUser
        {
            get
            {
                return _selectedItemUser;
            }
            set
            {
                if (_selectedItemUser != value)
                {
                    if (value != null)
                        this._isSelectionChanged = true;
                    //To check data of old item.
                    this.OnSelectChanging();
                    _selectedItemUser = value;
                    this.OnPropertyChanged(() => SelectedItemUser);
                    this.SetIsCheckedUserRight();
                    this.IsSetDefault = false;
                    this.OnPropertyChanged(() => IsCheckedAll);
                    this._isSelectionChanged = false;
                    this.IsEnableUserRight = false;
                    if (value == null)
                        this._currentUserResource = string.Empty;
                    else
                    {
                        this.IsEnableUserRight = true;
                        this._currentUserResource = value.UserResource;
                    }
                }
            }
        }
        #endregion

        #region UserCollection
        /// <summary>
        /// Gets or sets the UserCollection.
        /// </summary>
        private ObservableCollection<base_ResourceAccountModel> _userCollection = new ObservableCollection<base_ResourceAccountModel>();
        public ObservableCollection<base_ResourceAccountModel> UserCollection
        {
            get
            {
                return _userCollection;
            }
            set
            {
                if (_userCollection != value)
                {
                    _userCollection = value;
                    OnPropertyChanged(() => UserCollection);
                }
            }
        }

        #endregion

        #region UserRightCollection
        /// <summary>
        /// Gets or sets the UserRightCollection.
        /// </summary>
        private ObservableCollection<base_UserRightModel> _userRightCollection = new ObservableCollection<base_UserRightModel>();
        public ObservableCollection<base_UserRightModel> UserRightCollection
        {
            get
            {
                return _userRightCollection;
            }
            set
            {
                if (_userRightCollection != value)
                {
                    _userRightCollection = value;
                    OnPropertyChanged(() => UserRightCollection);
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

        #region TotalUsersRight
        private int _totalUsersRight;
        /// <summary>
        /// Gets or sets TotalUsersRight.
        /// </summary>
        public int TotalUsersRight
        {

            get
            {
                return _totalUsersRight;
            }
            set
            {
                if (_totalUsersRight != value)
                {
                    _totalUsersRight = value;
                    OnPropertyChanged(() => TotalUsersRight);
                }
            }
        }
        #endregion

        #region IsSetDefault
        protected bool _isSetDefault;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsSetDefault</para>
        /// </summary>
        public bool IsSetDefault
        {
            get { return this._isSetDefault; }
            set
            {
                if (this._isSetDefault != value)
                {
                    this._isSetDefault = value;
                    OnPropertyChanged(() => IsSetDefault);
                    if (!this._isSelectionChanged)
                        if (this.SelectedItemUser != null && value)
                        {
                            this.SelectedItemUser.ClonePassword = Define.DefaultPassword;
                            this.SelectedItemUser.ConfirmPassword = Define.DefaultPassword;
                            this.SelectedItemUser.IsEnablePassword = false;
                        }
                        else if (this.SelectedItemUser != null && !value)
                        {
                            this.SelectedItemUser.ClonePassword = string.Empty;
                            this.SelectedItemUser.ConfirmPassword = string.Empty;
                            this.SelectedItemUser.IsEnablePassword = true;
                        }
                }
            }
        }
        #endregion

        #region IsCheckedAll
        private bool? _isCheckedAll = false;
        /// <summary>
        /// Gets or sets the IsCheckedAll.
        /// </summary>
        public bool? IsCheckedAll
        {
            get { return _isCheckedAll; }
            set
            {
                if (_isCheckedAll != value)
                {
                    this._isCheckAllFlag = true;
                    _isCheckedAll = value;
                    if (!this._isCheckItemFlag && value.HasValue)
                    {
                        foreach (base_UserRightModel userRightItem in this.UserRightCollection)
                        {
                            if (SelectedGroupRight == 0)
                                userRightItem.IsChecked = value.Value;
                            else if (userRightItem.GroupId.Equals(SelectedGroupRight))
                                userRightItem.IsChecked = value.Value;
                        }

                        // Turn on IsDirty if user changes or selects item on UserRight datagrid.
                        if (!this._isSetIsCheckedItem)
                        {
                            this.SelectedItemUser.IsDirty = true;
                            this._isEditIsCheckedItem = true;
                        }
                    }
                    OnPropertyChanged(() => IsCheckedAll);
                    this._isCheckAllFlag = false;
                }
            }
        }
        #endregion

        #region IsEnableUserRight
        protected bool _isEnableUserRight = true;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsEnableUserRight</para>
        /// </summary>
        public bool IsEnableUserRight
        {
            get { return this._isEnableUserRight; }
            set
            {
                if (this._isEnableUserRight != value)
                {
                    this._isEnableUserRight = value;
                    OnPropertyChanged(() => IsEnableUserRight);
                }
            }
        }
        #endregion

        #region IsIncludeAccountLocked
        protected bool _isIncludeAccountLocked;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsIncludeAccountLocked</para>
        /// </summary>
        public bool IsIncludeAccountLocked
        {
            get { return this._isIncludeAccountLocked; }
            set
            {
                if (this._isIncludeAccountLocked != value)
                {
                    this._isIncludeAccountLocked = value;
                    OnPropertyChanged(() => IsIncludeAccountLocked);
                    this.SetVisibilityData(value);
                    this.SelectedItemUser = null;
                }
            }
        }
        #endregion

        #region EnableFilteringData
        /// <summary>
        /// To get , set value when enable colunm.
        /// </summary>
        private bool _enableFilteringData = true;
        public bool EnableFilteringData
        {
            get { return _enableFilteringData; }
            set
            {
                if (_enableFilteringData != value)
                {
                    _enableFilteringData = value;
                    OnPropertyChanged(() => EnableFilteringData);
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
                _currentPageIndex = value;
                OnPropertyChanged(() => CurrentPageIndex);
            }
        }

        #endregion

        #region SelectedGroupRight
        private short _selectedGroupRight;
        /// <summary>
        /// Gets or sets the SelectedGroupRight.
        /// </summary>
        public short SelectedGroupRight
        {
            get { return _selectedGroupRight; }
            set
            {
                if (_selectedGroupRight != value)
                {
                    _selectedGroupRight = value;
                    OnPropertyChanged(() => SelectedGroupRight);
                    OnSelectedGroupRightChanged();
                }
            }
        }
        #endregion

        #region TotalUserRights
        private int _totalUserRights;
        /// <summary>
        /// Gets or sets the TotalUserRights.
        /// </summary>
        public int TotalUserRights
        {
            get { return _totalUserRights; }
            set
            {
                if (_totalUserRights != value)
                {
                    _totalUserRights = value;
                    OnPropertyChanged(() => TotalUserRights);
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
        private bool OnNewCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return this.IsValid && (this.SelectedItemUser != null && this.SelectedItemUser.IsDirty);
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            try
            {
                // TODO: Handle command logic here
                if (this.SelectedItemUser.IsNewUser)
                    this.Insert();
                else
                    this.Update();
                this.SelectedItemUser.ToModelAndRaise();
                this.SelectedItemUser.EndUpdate();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine(ex);
            }

        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (this.SelectedItemUser != null
                && !this.SelectedItemUser.IsNewUser
                && (this.SelectedItemUser.IsLocked.HasValue && !this.SelectedItemUser.IsLocked.Value))
                return true;
            return false;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            // TODO: Handle command logic here
            this.Delete();
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
            this.IsSearchMode = !this.IsSearchMode;
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

        #region InitialData
        private void InitialData()
        {
            //To load data from base_UserRight.
            this.UserRightCollection.Clear();
            IOrderedEnumerable<base_UserRight> _userRights = _userRightRepository.GetAll().OrderBy(x => x.Id);
            foreach (var userRight in _userRights)
            {
                base_UserRightModel _userRightModel = new base_UserRightModel(userRight);
                _userRightModel.PropertyChanged += new PropertyChangedEventHandler(UserRightModel_PropertyChanged);
                this.UserRightCollection.Add(_userRightModel);
            }
            _userRightsCollectionView = CollectionViewSource.GetDefaultView(UserRightCollection);

            // Update total rows
            TotalUserRights = _userRightsCollectionView.Cast<base_UserRightModel>().Count();
        }
        #endregion

        #region PropertyChanged
        private void UserRightModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    UpdateIsCheckAll();
                    break;
            }
        }
        #endregion

        #region InitialCommand
        /// <summary>
        /// To initialize commands. 
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            this.NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            this.SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand(OnSearchCommandExecute, OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
        }
        #endregion

        #region LoadData
        /// <summary>
        /// To load data of base_ResourceAccount table.
        /// </summary>
        private void RefreshData()
        {
            this._guestRepository.Refresh();
            this._resourceAccountRepository.Refresh();
            this._authorizeRepository.Refresh();
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
                this.TotalUsers = _guestRepository.GetIQueryable(x => x.Mark.Equals(this._userMarkType) && !x.IsPurged).Count();
                //To get data with range
                int indexItem = 0;
                if (currentIndex > 1)
                    indexItem = (currentIndex - 1) * base.NumberOfDisplayItems;
                IList<base_Guest> users = _guestRepository.GetRange(indexItem, base.NumberOfDisplayItems, "It.Id", x => x.Mark.Equals(this._userMarkType) && !x.IsPurged);
                foreach (var item in users)
                    bgWorker.ReportProgress(0, item);
            };
            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_Guest _guest = e.UserState as base_Guest;
                string userResource = _guest.Resource.ToString();
                base_ResourceAccount _user = _resourceAccountRepository.GetIQueryable(x => x.UserResource.Equals(userResource)).SingleOrDefault();
                base_ResourceAccountModel resourceAccountModel;
                //To get data of ResourceAccount.
                if (_user != null)
                {
                    resourceAccountModel = new base_ResourceAccountModel(_user);
                    //To hidden item when IsLocked property is True.
                    if (resourceAccountModel.IsLocked.HasValue
                        && resourceAccountModel.IsLocked.Value)
                    {
                        resourceAccountModel.IsCheckLocked = true;
                        resourceAccountModel.Visibility = Visibility.Collapsed;
                    }
                    //To save password.
                    resourceAccountModel.ClonePassword = resourceAccountModel.ConfirmPassword = resourceAccountModel.Password;
                    resourceAccountModel.EndUpdate();
                }
                else
                {
                    resourceAccountModel = new base_ResourceAccountModel();
                    resourceAccountModel.IsNew = false;
                    resourceAccountModel.IsNewUser = true;
                    resourceAccountModel.IsLocked = false;
                }
                //To get data from base_Authorize
                string resource = resourceAccountModel.Resource.ToString();
                var Authorize = _authorizeRepository.GetIQueryable(x => x.Resource.Equals(resource));
                if (Authorize != null)
                    foreach (base_Authorize item in Authorize)
                        resourceAccountModel.AuthorizeCollection.Add(new base_AuthorizeModel(item));
                else
                    resourceAccountModel.AuthorizeCollection = null;
                //To show passwordBox to input data.
                resourceAccountModel.IsEnablePassword = true;
                //To get information of user from base_Guest table. 
                resourceAccountModel.UserResource = _guest.Resource.ToString();
                resourceAccountModel.UserName = string.Format("{0} {1}", _guest.FirstName, _guest.LastName);
                
                ComboItem departmentItem = Common.Departments.SingleOrDefault(x => Convert.ToInt32(x.ObjValue).Equals(_guest.Department));
                resourceAccountModel.Department = departmentItem!=null? departmentItem.Text :string.Empty;
                
                resourceAccountModel.PositionId = _guest.PositionId;
                resourceAccountModel.IsDirty = false;
                //To add item.
                this.UserCollection.Add(resourceAccountModel);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                base.IsBusy = false;
                if (this.UserCollection.Count == 0 || this.SelectedItemUser == null)
                {
                    this.IsEnableUserRight = false;
                }

            };
            bgWorker.RunWorkerAsync();
        }
        #endregion

        #region ChangeViewExecute
        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            //if (this.IsDirty)
            //{
            //    MessageBoxResult msgResult = MessageBoxResult.None;
            //    msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
            //    if (msgResult.Is(MessageBoxResult.Yes))
            //    {
            //        if (OnSaveCommandCanExecute(null))
            //        {
            //            //if (SaveCustomer())
            //            result = SaveCustomer();
            //        }
            //        else //Has Error
            //            result = false;

            //        // Remove popup note
            //        CloseAllPopupNote();
            //    }
            //    else
            //    {
            //        if (SelectedCustomer.IsNew)
            //        {
            //            DeleteNote();
            //            if (isClosing.HasValue && !isClosing.Value)
            //                IsSearchMode = true;
            //        }
            //        else //Old Item Rollback data
            //        {
            //            DeleteNote();
            //            SelectedCustomer.ToModelAndRaise();
            //            SetDataToModel(SelectedCustomer);
            //            SetSaleTaxFromAdditional();
            //        }
            //    }
            //}
            //else
            //{
            //    if (SelectedCustomer != null && SelectedCustomer.IsNew)
            //        DeleteNote();
            //    else
            //        // Remove popup note
            //        CloseAllPopupNote();
            //}
            return result;
        }
        #endregion

        #region Insert,Update,Delete
        private void Insert()
        {
            try
            {
                //To check loginName on DB.
                if (this.IsExistLoginName())
                {
                    this.ShowMessageBox("Username existed.!.Please enter another username.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                //To insert data into base_ResourceAccount table.
                this.SelectedItemUser.Resource = Guid.NewGuid();
                this.SelectedItemUser.IsNew = true;
                this.SelectedItemUser.Password = AESSecurity.Encrypt(this.SelectedItemUser.ClonePassword);
                this.SelectedItemUser.ClonePassword = this.SelectedItemUser.ConfirmPassword = this.SelectedItemUser.Password;
                this.SelectedItemUser.ToEntity();
                this._resourceAccountRepository.Add(this.SelectedItemUser.base_ResourceAccount);
                this._resourceAccountRepository.Commit();
                //To reset value of AuthorizeCollection.
                this.SelectedItemUser.AuthorizeCollection.Clear();
                //To insert data into base_Authorize table.
                if (this.UserRightCollection.Count(x => x.IsChecked) < this.UserRightCollection.Count)
                    foreach (base_UserRightModel item in this.UserRightCollection)
                        if (item.IsChecked)
                        {
                            base_Authorize _authorize = new base_Authorize();
                            _authorize.Resource = this.SelectedItemUser.base_ResourceAccount.Resource.ToString();
                            _authorize.Code = item.Code;
                            this._authorizeRepository.Add(_authorize);
                            this._authorizeRepository.Commit();
                            //To reset value of AuthorizeCollection.
                            this.SelectedItemUser.AuthorizeCollection.Add(new base_AuthorizeModel(_authorize));
                        }
                //To finish inserting data.
                this.SelectedItemUser.IsNewUser = false;
                this.SelectedItemUser.EndUpdate();
                this.SetDefaultValue();
                App.WriteUserLog("User Manegement", "User inserted a new user." + this.SelectedItemUser.Id);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("Insert" + ex.ToString());
            }
        }
        private void Update()
        {
            try
            {
                //To check loginName on DB.
                if (this.IsExistLoginName())
                {
                    this.ShowMessageBox("Login name existed.!.Please enter another login name.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                //To insert data into base_ResourceAccount table.
                if (!this.SelectedItemUser.Password.Equals(this.SelectedItemUser.ClonePassword))
                {
                    this.SelectedItemUser.Password = AESSecurity.Encrypt(this.SelectedItemUser.ClonePassword);
                    this.SelectedItemUser.ClonePassword = this.SelectedItemUser.ConfirmPassword = this.SelectedItemUser.Password;
                }
                //To unlock account.
                if (!this.SelectedItemUser.IsCheckLocked && this.SelectedItemUser.IsLocked.HasValue && this.SelectedItemUser.IsLocked.Value)
                {
                    this.SelectedItemUser.IsLocked = false;
                    this.SelectedItemUser.ExpiredDate = null;
                    App.WriteUserLog("User Manegement", "User unlocked a user." + this.SelectedItemUser.Id);
                }
                this.SelectedItemUser.ToEntity();
                this._resourceAccountRepository.Commit();

                //To insert data into base_Authorize table.
                if (this._isEditIsCheckedItem)
                {
                    //To delete old item in base_Authorize table.
                    string resource = this.SelectedItemUser.Resource.ToString();
                    var items = _authorizeRepository.GetIEnumerable(x => x.Resource == resource);
                    this._authorizeRepository.Delete(items);
                    this._authorizeRepository.Commit();
                    //To reset value of AuthorizeCollection.
                    this.SelectedItemUser.AuthorizeCollection.Clear();
                    //To insert data into base_Authorize table.
                    if (this.UserRightCollection.Count(x => x.IsChecked) < this.UserRightCollection.Count)
                        foreach (base_UserRightModel item in this.UserRightCollection)
                            if (item.IsChecked)
                            {
                                base_Authorize _authorize = new base_Authorize();
                                _authorize.Resource = this.SelectedItemUser.base_ResourceAccount.Resource.ToString();
                                _authorize.Code = item.Code;
                                this._authorizeRepository.Add(_authorize);
                                this._authorizeRepository.Commit();
                                //To reset value of AuthorizeCollection.
                                this.SelectedItemUser.AuthorizeCollection.Add(new base_AuthorizeModel(_authorize));
                            }
                }
                //To finish inserting data.
                this.SelectedItemUser.EndUpdate();
                this.SetDefaultValue();
                App.WriteUserLog("User Manegement", "User updated a new user." + this.SelectedItemUser.Id);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("Update" + ex.ToString());
            }
        }
        /// <summary>
        /// To update IsLock on base_ResoureceAccount table.
        /// </summary>
        private void Delete()
        {
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to lock this account?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    base_ResourceAccount item = _resourceAccountRepository.GetIQueryable(x => x.Resource == this.SelectedItemUser.Resource).SingleOrDefault();
                    if (item != null)
                    {
                        item.IsLocked = true;
                        //this.SelectedItemUser.ToEntity();
                        this._resourceAccountRepository.Commit();
                        //To hide item on DataGrid.
                        if (!this.IsIncludeAccountLocked)
                            this.SelectedItemUser.Visibility = Visibility.Collapsed;
                        else
                            this.SelectedItemUser.Visibility = Visibility.Visible;
                        this.SelectedItemUser.IsLocked = true;
                        this.SelectedItemUser.IsCheckLocked = true;
                        this.SelectedItemUser.IsDirty = false;
                        App.WriteUserLog("User Manegement", "User locked a user." + this.SelectedItemUser.Id);
                        this.SelectedItemUser = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("Delete" + ex.ToString());
            }
        }

        private bool IsEditData()
        {
            bool isUnactive = true;
            if (this.SelectedItemUser != null)
            {
                if (this.SelectedItemUser.IsDirty)
                {
                    this._cloneIsSetDefault = this.IsSetDefault;
                    MessageBoxResult msgResult = MessageBoxResult.None;
                    msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (msgResult == MessageBoxResult.Cancel)
                        //To don't do anything if user click Cancel.
                        return false;
                    if (msgResult == MessageBoxResult.Yes)
                        //To don't do anything if user click Cancel.
                        isUnactive = false;
                    else if (msgResult == MessageBoxResult.No || msgResult == MessageBoxResult.Cancel)
                        //To close view or to change item .
                        isUnactive = true;
                }
            }
            return isUnactive;
        }

        #endregion

        #region SetIsCheckedUserRight
        /// <summary>
        /// To set IsChecked on UserRight DataGrid.
        /// </summary>
        private void SetIsCheckedUserRight()
        {
            this._isSetIsCheckedItem = true;

            // Reset IsCheck of user right collection
            foreach (base_UserRightModel userRight in this.UserRightCollection)
                userRight.IsChecked = false;

            if (this.SelectedItemUser != null)
            {
                if (this.SelectedItemUser.AuthorizeCollection != null
                    && this.SelectedItemUser.AuthorizeCollection.Count > 0)
                {
                    // Set user right for selected user
                    foreach (base_AuthorizeModel authorize in this.SelectedItemUser.AuthorizeCollection)
                        foreach (base_UserRightModel userRight in this.UserRightCollection)
                            if (userRight.Code == authorize.Code)
                            {
                                userRight.IsChecked = true;
                                break;
                            }
                }
                else if (this.SelectedItemUser.LoginName != null && this.SelectedItemUser.LoginName.Length > 0)
                {
                    // Check all of items if this.SelectedItemUser.AuthorizeCollection is null.
                    foreach (base_UserRightModel userRight in this.UserRightCollection)
                        userRight.IsChecked = true;
                }
            }

            this._isSetIsCheckedItem = false;
        }
        #endregion

        #region SetDefaultValue
        /// <summary>
        /// To set default value for fields.
        /// </summary>
        private void SetDefaultValue()
        {
            this._isSetIsCheckedItem = false;
            this._isCheckItemFlag = false;
            this._isCheckAllFlag = false;
            this._isEditIsCheckedItem = false;
            this._isSelectionChanged = false;
        }
        #endregion

        #region IsExistLoginName
        /// <summary>
        /// To check login on DB.
        /// </summary>
        /// <returns></returns>
        private bool IsExistLoginName()
        {
            return (this._resourceAccountRepository.GetIQueryable(x => x.UserResource != this.SelectedItemUser.UserResource && x.LoginName.Trim().Equals(this.SelectedItemUser.LoginName.Trim())).Count() > 0);
        }
        #endregion

        #region RollBackData
        /// <summary>
        /// To rollback data when user click Cancel.
        /// </summary>
        private void RollBackData(bool isChangeItem)
        {
            if (this.SelectedItemUser != null)
            {
                base_ResourceAccountModel item = this.UserCollection.SingleOrDefault(x => x.UserResource.Equals(this._currentUserResource));
                //To rollback data when user click Cancel.
                if (isChangeItem)
                {
                    //To show passwordBox to input data.
                    item.IsEnablePassword = true;
                    item.LoginName = item.base_ResourceAccount.LoginName;
                    item.ExpiredDate = item.base_ResourceAccount.ExpiredDate;
                    item.Password = item.base_ResourceAccount.Password;
                    item.ClonePassword = item.ConfirmPassword = item.Password;
                    if (string.IsNullOrEmpty(item.LoginName)
                        || item.LoginName.Length == 0)
                        item.IsNewUser = true;
                    else
                        item.IsNewUser = false;
                    string resource = item.Resource.ToString();
                    var Authorize = _authorizeRepository.GetIQueryable(x => x.Resource.Equals(resource));
                    if (Authorize != null)
                        foreach (base_Authorize itemAuthorize in Authorize)
                            item.AuthorizeCollection.Add(new base_AuthorizeModel(itemAuthorize));
                    else
                        item.AuthorizeCollection = null;
                    item.IsDirty = false;
                }
                //To return old item when user click OK.
                else
                    App.Current.MainWindow.Dispatcher.BeginInvoke((Action)delegate
                    {
                        this.SelectedItemUser = item;
                        this.IsSetDefault = this._cloneIsSetDefault;
                    });
            }
        }
        #endregion

        #region OnSelectChanging
        private void OnSelectChanging()
        {
            if (this.SelectedItemUser != null
                && this._isSelectionChanged)
            {
                bool _result = this.IsEditData();
                this.RollBackData(_result);
            }
        }
        #endregion

        #region VisibilityData
        /// <summary>
        ///To show item when user check into Check Box.
        /// </summary>
        private void SetVisibilityData(bool value)
        {
            Visibility _visibility = value ? Visibility.Visible : Visibility.Collapsed;
            if (this.UserCollection != null)
                foreach (var item in this.UserCollection.Where(x => x.IsLocked.HasValue && x.IsLocked.Value))
                    item.Visibility = _visibility;
        }
        #endregion

        #region OnSelectedGroupRightChanged
        private void OnSelectedGroupRightChanged()
        {
            if (SelectedGroupRight == 0)
            {
                // Show all user right
                _userRightsCollectionView.Filter = null;
            }
            else
            {
                // Filter user right by selected group
                _userRightsCollectionView.Filter = (x) =>
                {
                    base_UserRightModel userRightModel = x as base_UserRightModel;
                    return userRightModel.GroupId.Equals(SelectedGroupRight);
                };
            }

            // Update total rows
            TotalUserRights = _userRightsCollectionView.Cast<base_UserRightModel>().Count();

            _isSetIsCheckedItem = true;
            UpdateIsCheckAll();
            _isSetIsCheckedItem = false;
        }
        #endregion

        #region UpdateIsCheckAll
        private void UpdateIsCheckAll()
        {
            if (!this._isCheckAllFlag)
            {
                this._isCheckItemFlag = true;

                IEnumerable<base_UserRightModel> userRights = UserRightCollection;
                if (SelectedGroupRight != 0)
                {
                    // Get all user right by group
                    userRights = UserRightCollection.Where(x => x.GroupId.Equals(SelectedGroupRight));
                }

                if (userRights.Count(x => x.IsChecked) == userRights.Count())
                    this.IsCheckedAll = true;
                else
                    this.IsCheckedAll = false;

                this._isCheckItemFlag = false;

                // Turn on IsDirty if user changes or selects item on UserRight datagrid.
                if (!this._isSetIsCheckedItem)
                {
                    this.SelectedItemUser.IsDirty = true;
                    this._isEditIsCheckedItem = true;
                }

                // Raise IsCheckedAll property.
                this.OnPropertyChanged(() => IsCheckedAll);
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region OnViewChangingCommandCanExecute
        /// <summary>
        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return this.IsEditData();
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            this.InitialData();
            this._isSelectionChanged = false;
            this.UserCollection.Clear();
            this.LoadUser(this.CurrentPageIndex);
        }
        #endregion

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                List<string> errors = new List<string>();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string msg = this[prop.Name];
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Add(msg);
                    }
                }
                return string.Join(Environment.NewLine, errors);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                switch (columnName)
                {
                    case "IsCheckedAll":
                        if ((this.IsCheckedAll == null
                            || !this.IsCheckedAll.Value)
                            && this.UserRightCollection.Count(x => x.IsChecked) == 0)
                            message = "User right must be selected.";
                        break;
                }
                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }

        #endregion
    }
}