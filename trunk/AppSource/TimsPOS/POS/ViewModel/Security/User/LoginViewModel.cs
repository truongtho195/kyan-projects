using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CPC.Toolkit.Command;
using System.Text.RegularExpressions;
using CPC.POS.Repository;
using System.Diagnostics;
using CPC.POS.Database;
using SecurityLib;
using CPC.Utility;
using System.Net;
using CPC.POS.View;
using System.Globalization;
using CPC.POS.Model;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    public class LoginViewModel : ViewModelBase, IDataErrorInfo
    {

        #region Define
        private base_ResourceAccountRepository _resourceAccountRepository = new base_ResourceAccountRepository();
        private base_AuthorizeRepository _authorizeRepository = new base_AuthorizeRepository();
        private base_UserLogRepository _userLogRepository = new base_UserLogRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private int _numberOfLogins = 0;
        private string _message = string.Empty;
        private bool IsUserAuthenicated { set; get; }
        private bool _isUpdateExpiredAccount = false;
        private string _defaultUsernName = "admin";
        private string _defaultPassword = "iktfcGzCJQ13CBk3uR6n9A==";
        private bool IsLoginDefaultUser = false;
        #endregion

        #region Properties

        #region Languages
        /// <summary>
        /// Gets or sets the Languages.
        /// </summary>
        public IList<LanguageItem> Languages
        {
            get
            {
                return Common.Languages;
            }

        }

        #endregion

        #region Shift
        /// <summary>
        /// Gets or sets the Shifts.
        /// </summary>
        public IList<LanguageItem> Shifts
        {
            get
            {
                return Common.Shifts;
            }

        }

        #endregion

        #region LoginStatus
        /// <summary>
        /// Display the login status to user
        /// </summary>
        private string loginStatus;
        public string LoginStatus
        {
            get { return loginStatus; }
            set
            {
                if (value != loginStatus)
                {
                    this.loginStatus = value;
                    OnPropertyChanged(() => LoginStatus);
                }
            }
        }
        #endregion

        #region UserName
        /// <summary>
        /// Username property with validation 
        /// </summary>
        private string userName = String.Empty;
        //[Required(ErrorMessage = "Field 'User Name' is required.")]
        //[RegularExpression("[a-zA-Z0-9]{3,50}", ErrorMessage = "User Name must a-z and length of 3-50 characters")]
        public string UserName
        {
            set
            {
                userName = value;
                OnPropertyChanged(() => UserName);
            }
            get
            {
                return userName;
            }
        }
        #endregion

        #region UserPassword
        /// <summary>
        /// Password property with regular expression validation 
        /// </summary>
        private string userPassword = String.Empty;
        //[Required(ErrorMessage = "Field 'Password' is required.")]
        //[RegularExpression("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}", ErrorMessage = "Password must a-z and length of 3-50 characters")]
        public string UserPassword
        {
            set
            {
                userPassword = value;
                OnPropertyChanged(() => UserPassword);
            }
            get
            {
                return userPassword;
            }
        }
        #endregion

        #region NewUserPassword
        /// <summary>
        /// Password property with regular expression validation 
        /// </summary>
        private string newUserPassword;
        //[Required(ErrorMessage = "Field 'Password' is required.")]
        //[RegularExpression("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}", ErrorMessage = "Password must a-z and length of 3-50 characters")]
        public string NewUserPassword
        {
            set
            {
                newUserPassword = value;
                OnPropertyChanged(() => NewUserPassword);
                OnPropertyChanged(() => ConfirmUserPassword);
            }
            get
            {
                return newUserPassword;
            }
        }
        #endregion

        #region ConfirmUserPassword
        /// <summary>
        /// Password property with regular expression validation 
        /// </summary>
        private string confirmUserPassword;
        //[Required(ErrorMessage = "Field 'Password' is required.")]
        //[RegularExpression("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}", ErrorMessage = "Password must a-z and length of 3-50 characters")]
        public string ConfirmUserPassword
        {
            set
            {
                confirmUserPassword = value;
                OnPropertyChanged(() => NewUserPassword);
                OnPropertyChanged(() => ConfirmUserPassword);
            }
            get
            {
                return confirmUserPassword;
            }
        }
        #endregion

        #region MoreOptionVisibility
        /// <summary>
        /// Show / Hide the more option fields
        /// </summary>
        private Visibility moreOptionVisibility = Visibility.Collapsed;
        public Visibility MoreOptionVisibility
        {
            get { return moreOptionVisibility; }
            set
            {
                if (value != moreOptionVisibility)
                {
                    this.moreOptionVisibility = value;

                    if (value == Visibility.Collapsed)
                    {
                        newUserPassword = String.Empty;
                        confirmUserPassword = String.Empty;
                    }

                    OnPropertyChanged(() => MoreOptionVisibility);
                    OnPropertyChanged(() => NewUserPassword);
                    OnPropertyChanged(() => ConfirmUserPassword);
                }
            }
        }
        #endregion

        #region Remember
        /// <summary>
        /// if Remenber is True ,the application will save username and password.
        /// </summary>
        private bool? _remember;
        public bool? Remember
        {
            get
            {
                if (null == _remember)
                    this.CheckRemember();
                return _remember.Value;
            }
            set
            {
                if (value != _remember)
                {
                    this._remember = value;
                    OnPropertyChanged(() => Remember);
                }
            }
        }
        #endregion

        #region IsAllowShift
        /// <summary>
        /// get ,set value of IsAllowShift.
        /// </summary>
        private bool _isAllowShift;
        public bool IsAllowShift
        {
            get
            {
                return _isAllowShift;
            }
            set
            {
                if (value != _isAllowShift)
                {
                    this._isAllowShift = value;
                    OnPropertyChanged(() => IsAllowShift);
                }
            }
        }
        #endregion

        #region VisibilityAllowShift
        /// <summary>
        /// To set Visibility of ComboBox "Shift".
        /// </summary>
        public Visibility VisibilityAllowShift
        {
            get
            {
                return Define.CONFIGURATION.IsAllowShift ? Visibility.Visible : Visibility.Hidden;
            }
        }
        #endregion

        #region LanguageItem
        /// <summary>
        /// To get, set value from Language ComboBox.
        /// </summary>
        private LanguageItem _languageItem;
        public LanguageItem LanguageItem
        {
            get { return _languageItem; }
            set
            {
                if (value != _languageItem && value != null)
                {
                    this._languageItem = value;
                    OnPropertyChanged(() => LanguageItem);
                }
            }
        }
        #endregion

        #region ShiftItem
        /// <summary>
        /// To get, set value from Shift ComboBox.
        /// </summary>
        private LanguageItem _shiftItem;
        public LanguageItem ShiftItem
        {
            get { return _shiftItem; }
            set
            {
                if (value != _shiftItem)
                {
                    this._shiftItem = value;
                    OnPropertyChanged(() => ShiftItem);
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        public ICommand LoginCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand MoreOptionsCommand { get; private set; }
        /// <summary>
        /// Gets the OpenViewCommand command.
        /// </summary>
        public ICommand OpenManagementUserCommand
        {
            get;
            private set;
        }
        private Window _view;
        #endregion

        #region Constructors
        public LoginViewModel()
        {
            //To route the commands 
            this.LoginCommand = new RelayCommand(this.OnLoginExecuted, this.CanOnLoginExecute);
            this.CancelCommand = new RelayCommand(this.OnCancelExecuted);
            this.MoreOptionsCommand = new RelayCommand(this.OnMoreOptionExecuted);
            this.IsUserAuthenicated = false;
        }
        public LoginViewModel(Window loginView)
        {
            _ownerViewModel = this;
            //To route the commands 
            this.LoginCommand = new RelayCommand(this.OnLoginExecuted, this.CanOnLoginExecute);
            this.CancelCommand = new RelayCommand(this.OnCancelExecuted);
            this.MoreOptionsCommand = new RelayCommand(this.OnMoreOptionExecuted);
            this.OpenManagementUserCommand = new RelayCommand<object>(this.OnOpenManagermentUserCommandExecute);
            this.IsUserAuthenicated = false;
            this._view = loginView;
            Define.USER_AUTHORIZATION = new System.Collections.ObjectModel.ObservableCollection<Model.base_AuthorizeModel>();
            this.LanguageItem = this.Languages.SingleOrDefault(x => x.Code.ToLower().Equals(Define.CONFIGURATION.DefaultLanguage.ToLower()));
            this._isUpdateExpiredAccount = false;
            this.IsLoginDefaultUser = false;
        }
        #endregion

        #region Command methods

        #region OnLoginExecuted
        /// <summary>
        /// Enable the Login button if all required field are validated IsValid = true.
        /// </summary>
        /// <returns></returns>
        private bool CanOnLoginExecute()
        {
            return base.IsValid;
        }
        /// <summary>
        ///To check the user login and change password/
        /// </summary>
        private void OnLoginExecuted()
        {
            try
            {
                //To check login.
                bool? result = this.IsLoginSuccess();
                if (result.HasValue)
                {
                    if (result.Value)
                    {
                        //To insert data into base_userLog table.
                        this.InsertUserLog();
                        //To insert data into base_UserLogDetail table.
                        App.WriteLUserLog("Login", "User logged on the application.");
                        //To remember the account login
                        if (this.Remember.HasValue && this.Remember.Value)
                        {
                            try
                            {
                                //To remember the account login.
                                var userEncrypt = SecurityLib.AESSecurity.Encrypt(userName);
                                string pw = string.Empty;
                                if (Define.USER != null)
                                    pw = Define.USER.Password;
                                else
                                    pw = this._defaultPassword;
                                ApplicationIsolatedSettings.Instance[Define.REMEMBER_KEY] = String.Concat(userEncrypt, ',', pw);
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                //To clear remember.
                                ApplicationIsolatedSettings.Instance[Define.REMEMBER_KEY] = String.Empty;

                            }
                            catch { }
                        }
                        //To close LoginView and open MainWindow.
                        App.Messenger.NotifyColleagues(Define.USER_LOGIN_RESULT, true);
                    }
                    else
                    {
                        //To count number of logins .
                        this._numberOfLogins++;
                        MessageBox.Show(this._message, "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {   //To count number of logins .
                    this._numberOfLogins++;
                    MessageBox.Show(this._message, "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //To shutdown the application when user login fail on 5 times. 
                if ((Define.CONFIGURATION.LoginAllow != null && this._numberOfLogins == Define.CONFIGURATION.LoginAllow) || this._numberOfLogins == 3)
                {
                    MessageBox.Show("The application will shutdown.", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this._view.Close();
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnLoginExecuted" + ex.ToString());
            }
        }
        #endregion

        #region OnMoreOptionExecuted
        /// <summary>
        /// show or hide more option fields
        /// </summary>
        private void OnMoreOptionExecuted()
        {
            if (Visibility.Collapsed == MoreOptionVisibility)
                MoreOptionVisibility = Visibility.Visible;
            else
                MoreOptionVisibility = Visibility.Collapsed;
        }

        private void OnCancelExecuted()
        {
            _view.DialogResult = false;
            _view.Close();
        }
        #endregion

        #region OnOpenManagermentUserCommandExecute
        /// <summary>
        /// Method to invoke when the OpenViewCommand command is executed.
        /// </summary>
        private void OnOpenManagermentUserCommandExecute(object param)
        {
            try
            {
                ManagementUserLogView view = new ManagementUserLogView();
                view.DataContext = new ManagementUserLogViewModel();
                view.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnOpenManagermentUserCommandExecute" + ex.ToString());
            }
        }
        #endregion

        #endregion

        #region Methods

        #region CheckRemember
        //To Remember logon
        void CheckRemember()
        {
            try
            {
                // Remember the account logon
                string account = ApplicationIsolatedSettings.GetSetting(Define.REMEMBER_KEY);
                if (String.Empty == account)
                {
                    _remember = false;
                }
                else
                {
                    _remember = true;

                    string[] accountArray = account.Split(',');
                    if (accountArray.Length > 1)
                    {
                        UserName = SecurityLib.AESSecurity.Decrypt(accountArray[0]);
                        UserPassword = SecurityLib.AESSecurity.Decrypt(accountArray[1]);
                    }
                }
            }
            catch
            {
                //To Clear remember
                _remember = false;
                ApplicationIsolatedSettings.Instance[Define.REMEMBER_KEY] = String.Empty;
            }
        }
        #endregion

        #region IsExistedUser
        //To check account if account is being used by another user.
        private bool IsExistedUser(string resource)
        {
            if (_userLogRepository.GetIQueryable(x => x.ResourceAccessed.Equals(resource) && x.IsDisconected.HasValue && !x.IsDisconected.Value).Count() > 0)
                return true;
            return false;
        }
        #endregion

        #region IsLoginSuccess
        /// <summary>
        /// To check login.
        /// </summary>
        /// <returns></returns>
        private bool? IsLoginSuccess()
        {
            try
            {
                this._message = string.Empty;
                string encryptUserName = AESSecurity.Encrypt(this.UserName);
                string encryptPassword = AESSecurity.Encrypt(this.UserPassword);
                base_ResourceAccount resourceAccount = _resourceAccountRepository.GetIQueryable(x => x.LoginName.Trim().Equals(this.UserName) && x.Password.Trim().Equals(encryptPassword)).SingleOrDefault();
                //To verify account of user.
                if (resourceAccount != null)
                {
                    var allUserLog = _userLogRepository.GetAll(x => x.ResourceAccessed.Equals(resourceAccount.UserResource));
                    if (allUserLog != null)
                    {
                        foreach (var item in allUserLog)
                        {
                            _userLogRepository.Delete(item);
                            _userLogRepository.Commit();
                        }
                    }

                    if (resourceAccount.IsLocked.HasValue && resourceAccount.IsLocked.Value)
                    {
                        this._message = "This account is locked . Please contact to admin to unlock this account.";
                        return false;
                    }
                    //To login seccessfully.
                    else if (((resourceAccount.ExpiredDate.HasValue && resourceAccount.ExpiredDate.Value > DateTimeExt.Now) || !resourceAccount.ExpiredDate.HasValue))
                    {
                        //To check account
                        if (!this.IsExistedUser(resourceAccount.UserResource))
                        {
                            Model.base_ResourceAccountModel user = new Model.base_ResourceAccountModel(resourceAccount);
                            Guid guestResource = Guid.Parse(user.UserResource);
                            base_Guest _guest = _guestRepository.GetIQueryable(x => x.Resource == guestResource).SingleOrDefault();
                            if (_guest != null)
                            {
                                user.UserResource = _guest.Resource.ToString();
                                user.UserName = string.Format("{0} {1}", _guest.FirstName, _guest.LastName);
                                user.Department = _guest.Department;
                                user.PositionId = _guest.PositionId;
                                user.IpAddress = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
                            }
                            //To get user inoformation.
                            Define.USER = user;
                            //To get user authorization.
                            string resource = user.Resource.ToString();
                            var Authorize = _authorizeRepository.GetIQueryable(x => x.Resource.Equals(resource));
                            if (Authorize != null)
                                foreach (base_Authorize item in Authorize)
                                    Define.USER_AUTHORIZATION.Add(new Model.base_AuthorizeModel(item));
                            return true;
                        }
                        else
                            this._message = "This account is being used by another user.";
                    }
                    //Account expired.
                    else
                    {
                        if (!this._isUpdateExpiredAccount)
                            this.UpdateExpiredAccount(resourceAccount.UserResource);
                        this._message = "This account expired . Please contact to admin to reset this account.";
                        return false;
                    }
                }
                else
                {
                    if (this.userName.Equals(this._defaultUsernName)
                        && encryptPassword.Equals(this._defaultPassword))
                    {
                        this.IsLoginDefaultUser = true;
                        //To get user inoformation as admin.
                        Define.USER = new base_ResourceAccountModel { LoginName = "admin", Password = this._defaultPassword, IpAddress = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString() };
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsLoginSuccess" + ex.ToString());
            }
            if (string.IsNullOrEmpty(this._message))
                this._message = "Could not verify this account . Please try again.";
            return null;
        }
        #endregion

        #region InsertUserLog
        /// <summary>
        /// To insert data into base_UserLog table.
        /// </summary>
        private void InsertUserLog()
        {
            if (!this.IsLoginDefaultUser)
            {
                base_UserLog userLog = new base_UserLog();
                userLog.IpSource = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
                userLog.ConnectedOn = DateTimeExt.Now;
                userLog.ResourceAccessed = Define.USER.UserResource.ToString();
                userLog.IsDisconected = false;
                _userLogRepository.Add(userLog);
                _userLogRepository.Commit();
                Define.USER.UserLogId = userLog.Id;
            }
        }
        #endregion

        #region UpdateUserLog
        /// <summary>
        /// To update data on base_UserLog table.
        /// </summary>
        private void UpdateUserLog()
        {
            if (!this.IsLoginDefaultUser)
            {
                base_UserLog userLog = _userLogRepository.GetIEnumerable(x => x.ResourceAccessed == Define.USER.UserResource).SingleOrDefault();
                userLog.DisConnectedOn = DateTimeExt.Now;
                userLog.IsDisconected = true;
                _userLogRepository.Commit();
            }
        }
        #endregion

        #region UpdateExpiredAccount
        /// <summary>
        /// To update data on base_UserLog table.
        /// </summary>
        private void UpdateExpiredAccount(string resource)
        {
            base_ResourceAccount resourceAccount = _resourceAccountRepository.GetIEnumerable(x => x.UserResource == resource).SingleOrDefault();
            if (resourceAccount != null
                && (resourceAccount.IsExpired == null || (resourceAccount.IsExpired.HasValue && !resourceAccount.IsExpired.Value)))
            {
                resourceAccount.IsExpired = true;
                _userLogRepository.Commit();
                this._isUpdateExpiredAccount = true;
            }
        }
        #endregion

        #region OnChangeLanguages
        /// <summary>
        /// This function will execute when user select laguage type on Laguage ComboBox.
        /// </summary>
        /// <param name="param"></param>
        private void OnChangeLanguages(int param)
        {
            //if (param == 0)
            //    LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
            //else if (param == 1)
            //    LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        }
        #endregion

        #endregion

        #region IDataErrorInfo Members

        protected HashSet<string> _extensionErrors = new HashSet<string>();
        /// <summary>
        /// <para> Gets or sets the ExtensionErrors </para>
        /// </summary>
        public HashSet<string> ExtensionErrors
        {
            get { return _extensionErrors; }
            set
            {
                if (_extensionErrors != value)
                {
                    _extensionErrors = value;
                    OnPropertyChanged(() => ExtensionErrors);
                }
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                this.ExtensionErrors.Clear();

                switch (columnName)
                {
                    case "UserName":
                        if (string.IsNullOrEmpty(this.userName))
                        {
                            message = "User name is required.";
                        }
                        else
                        {
                            Regex regex = new Regex("[a-zA-Z0-9]{3,50}");
                            if (!regex.IsMatch(this.userName))
                            {
                                message = "User name must a-z and length of 3-50 characters";
                            }
                        }
                        break;
                    case "UserPassword":
                        if (string.IsNullOrEmpty(this.userPassword))
                        {
                            message = "Password is required.";
                        }
                        else
                        {
                            Regex regex = new Regex("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}");
                            if (!regex.IsMatch(this.userPassword))
                            {
                                message = "Password must a-z and length of 3-50 characters";
                            }
                        }
                        break;
                    case "NewUserPassword":
                        if (moreOptionVisibility == Visibility.Visible)
                        {
                            if (string.IsNullOrEmpty(this.newUserPassword))
                            {
                                message = "New password is required.";
                            }
                            else
                            {
                                Regex regex = new Regex("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}");
                                if (!regex.IsMatch(this.newUserPassword))
                                {
                                    message = "New password must a-z and length of 3-50 characters";
                                }
                                else if (this.newUserPassword != this.confirmUserPassword)
                                {
                                    message = "New password is not match the re-typed passwrod.";
                                }
                            }
                        }
                        break;
                    case "ConfirmUserPassword":
                        if (moreOptionVisibility == Visibility.Visible)
                        {
                            if (string.IsNullOrEmpty(this.confirmUserPassword))
                            {
                                message = "Re-enter password is required.";
                            }
                            else
                            {
                                Regex regex = new Regex("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}");
                                if (!regex.IsMatch(this.confirmUserPassword))
                                {
                                    message = "Re-enter password must a-z and length of 3-50 characters";
                                }
                                else if (this.newUserPassword != this.confirmUserPassword)
                                {
                                    message = "Re-enter password is not match the re-typed passwrod.";
                                }
                            }
                        }
                        break;
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    this.ExtensionErrors.Add(columnName);
                    return message;
                }
                return null;
            }
        }

        #endregion

    }
}
