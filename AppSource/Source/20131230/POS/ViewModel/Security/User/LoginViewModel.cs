using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using SecurityLib;

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
        private bool IsLoginDefaultUser = false;
        #endregion

        #region Properties

        #region Languages
        /// <summary>
        /// Gets or sets the Languages.
        /// </summary>
        public IList<ComboItem> Languages
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
        public IList<ComboItem> Shifts
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
        private ComboItem _languageItem;
        public ComboItem LanguageItem
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
        private ComboItem _shiftItem;
        public ComboItem ShiftItem
        {
            get { return _shiftItem; }
            set
            {
                if (value != _shiftItem)
                {
                    this._shiftItem = value;
                    OnPropertyChanged(() => ShiftItem);
                    if (this.ShiftItem != null)
                        Define.ShiftCode = this.ShiftItem.Code;
                    else
                        Define.ShiftCode = null;
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
            this.ChangeShiftCode();
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
                        Define.StoreCode = Define.CONFIGURATION.StoreCode.HasValue ? (int)Define.CONFIGURATION.StoreCode.Value : 0;
                        Define.UserPermissions = new UserPermissions();
                        //SynchronizationViewModel viewModel = new SynchronizationViewModel(Define.StoreCode);
                        //To insert data into base_userLog table.
                        this.InsertUserLog();
                        //To insert data into base_UserLogDetail table.
                        App.WriteUserLog("Login", "User logged on the application.");
                        //To remember the account login
                        if (this.Remember.HasValue && this.Remember.Value)
                        {
                            try
                            {
                                //To remember the account login.
                                var userEncrypt = SecurityLib.AESSecurity.Encrypt(this.UserName.Trim().ToLower());
                                string pw = string.Empty;
                                if (Define.USER != null)
                                    pw = Define.USER.Password;
                                else
                                    pw = Define.ADMIN_PASSWORD;
                                CPC.POS.Properties.Settings.Default.Username = userEncrypt;
                                CPC.POS.Properties.Settings.Default.Password = pw;
                                CPC.POS.Properties.Settings.Default.Save();
                            }
                            catch (Exception ex)
                            {
                                _log4net.Error(ex);
                                Xceed.Wpf.Toolkit.MessageBox.Show("That application save password is error! /n" + ex.ToString(), Language.Information, MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            try
                            {
                                //To clear remember.
                                CPC.POS.Properties.Settings.Default.Username = string.Empty;
                                CPC.POS.Properties.Settings.Default.Password = string.Empty;
                                CPC.POS.Properties.Settings.Default.Save();
                            }
                            catch (Exception ex)
                            {
                                _log4net.Error(ex);
                                Xceed.Wpf.Toolkit.MessageBox.Show("That application clear password is error! /n" + ex.ToString(), Language.Information, MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        //To change ShiftCode in setting file.
                        if (this.ShiftItem != null)
                        {
                            CPC.POS.Properties.Settings.Default.Shift = this.ShiftItem.Code;
                            CPC.POS.Properties.Settings.Default.Save();
                        }
                        //To close LoginView and open MainWindow.
                        App.Messenger.NotifyColleagues(Define.USER_LOGIN_RESULT, true);
                    }
                    else
                    {
                        //To count number of logins .
                        this._numberOfLogins++;
                        Xceed.Wpf.Toolkit.MessageBox.Show(this._message, Language.Information, MessageBoxButton.OK, MessageBoxImage.Warning);

                    }
                }
                else
                {   //To count number of logins .
                    this._numberOfLogins++;
                    Xceed.Wpf.Toolkit.MessageBox.Show(this._message, Language.Information, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //To shutdown the application when login fail on 5 times. 
                if ((Define.CONFIGURATION != null && Define.CONFIGURATION.LoginAllow != null && this._numberOfLogins == Define.CONFIGURATION.LoginAllow) || this._numberOfLogins == 3)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.FindResource("Login_Message_Shutdown") as string, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    this._view.Close();
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
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
                _log4net.Error(ex);
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
                //To remember the account logon
                if (String.IsNullOrEmpty(Define.Username)
                    && String.IsNullOrEmpty(Define.Password))
                {
                    this.UserName = string.Empty;
                    this.UserPassword = string.Empty;
                    this.Remember = false;
                }
                else
                {
                    this.Remember = true;
                    this.UserName = SecurityLib.AESSecurity.Decrypt(Define.Username);
                    this.UserPassword = SecurityLib.AESSecurity.Decrypt(Define.Password);
                }
            }
            catch(Exception ex)
            {
                //To Clear remember
                this.Remember = false;
                CPC.POS.Properties.Settings.Default.Username = string.Empty;
                CPC.POS.Properties.Settings.Default.Password = string.Empty;
                CPC.POS.Properties.Settings.Default.Save();
                _log4net.Error(ex);
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

        #region IsExistedUserInGuest
        //To check account if account is deleted by user.
        private bool IsExistedUserInGuest(string resource)
        {
            Guid guid = Guid.Parse(resource);
            base_GuestRepository guestRepository = new base_GuestRepository();
            if (guestRepository.GetIQueryable(x => x.Resource.HasValue && x.Resource.Value == guid && !x.IsPurged).Count() > 0)
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
                string encryptPassword = AESSecurity.Encrypt(this.UserPassword.Trim());
                string _name = this.UserName.Trim().ToLower();
                base_ResourceAccount resourceAccount = _resourceAccountRepository.GetIQueryable(x => x.LoginName.Trim().ToLower().Equals(_name) && x.Password.Trim().Equals(encryptPassword)).SingleOrDefault();
                //To verify account of user.
                if (resourceAccount != null && this.IsExistedUserInGuest(resourceAccount.UserResource))
                {
                    if (resourceAccount.IsLocked.HasValue && resourceAccount.IsLocked.Value)
                    {
                        this._message = Application.Current.FindResource("Login_Message_AccountLocked") as string;
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
                                user.IpAddress = Dns.GetHostName();
                            }
                            //To get user inoformation.
                            Define.USER = user;
                            //To get user authorization.
                            string resource = user.Resource.ToString();
                            var Authorize = _authorizeRepository.GetIQueryable(x => x.Resource.Equals(resource));
                            this._authorizeRepository.Refresh(Authorize);
                            if (Authorize != null)
                                foreach (base_Authorize item in Authorize)
                                    Define.USER_AUTHORIZATION.Add(new Model.base_AuthorizeModel(item));
                            return true;
                        }
                        else
                            this._message = Application.Current.FindResource("Login_Message_AccountAnother") as string;
                    }
                    //Account expired.
                    else
                    {
                        if (!this._isUpdateExpiredAccount)
                            this.UpdateExpiredAccount(resourceAccount.UserResource);
                        this._message = Application.Current.FindResource("Login_Message_AccountExpired") as string;
                        return false;
                    }
                }
                else
                {
                    if (this.userName.ToLower().Equals(Define.ADMIN_ACCOUNT.ToLower())
                        && encryptPassword.Equals(Define.ADMIN_PASSWORD))
                    {
                        this.IsLoginDefaultUser = true;
                        //To get user inoformation as admin.
                        Define.USER = new base_ResourceAccountModel { LoginName = Define.ADMIN_ACCOUNT, Password = Define.ADMIN_PASSWORD, IpAddress = Dns.GetHostName(), Resource = Guid.Empty };
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("IsLoginSuccess" + ex.ToString());
            }
            if (string.IsNullOrEmpty(this._message))
                this._message = Application.Current.FindResource("Login_Message_AccountVerify") as string;
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
                userLog.IpSource = Dns.GetHostName();
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

        #region ChangeShiftCode
        /// <summary>
        /// To show shiftcode.
        /// </summary>
        private void ChangeShiftCode()
        {
            try
            {
                if (Define.CONFIGURATION.IsAllowShift)
                {
                    //To check shift code on database.
                    base_CustomFieldRepository _customFieldRepository = new base_CustomFieldRepository();
                    IList<base_CustomField> shiftCode = _customFieldRepository.GetAll(x => x.Mark == "S");
                    foreach (var item in shiftCode)
                    {
                        string[] hours = item.Label.Trim().Split('-');
                        DateTime fromhours = DateTime.Parse(hours[0].Trim());
                        DateTime tohours = DateTime.Parse(hours[1].Trim());
                        if (DateTime.Now >= fromhours && DateTime.Now <= tohours)
                        {
                            this.ShiftItem = this.Shifts.SingleOrDefault(x => x.Code == item.FieldName);
                            break;
                        }
                    }
                }
                else
                    Define.ShiftCode = null;
            }
            catch (Exception ex) 
            {
                _log4net.Error(ex);
                Debug.WriteLine(ex.ToString());
            }
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
                            Regex regex = new Regex("[a-zA-Z0-9]{5,50}");
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
                            Regex regex = new Regex(Define.CONFIGURATION.PasswordFormat);
                            if (!regex.IsMatch(this.userPassword))
                            {
                                message = "Password must a-z and length of 8-50 characters";
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
                                Regex regex = new Regex(Define.CONFIGURATION.PasswordFormat);
                                if (!regex.IsMatch(this.newUserPassword))
                                {
                                    message = "New password must a-z and length of 8-50 characters";
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
                                Regex regex = new Regex(Define.CONFIGURATION.PasswordFormat);
                                if (!regex.IsMatch(this.confirmUserPassword))
                                {
                                    message = "Re-enter password must a-z and length of 8-50 characters";
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