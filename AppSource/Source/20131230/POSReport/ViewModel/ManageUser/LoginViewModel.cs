using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Toolkit.Base;
using Toolkit.Command;
using CPC.POSReport.Repository;
using CPC.POSReport.View;
using SecurityLib;
using POSReport;
using System.ComponentModel;
using CPC.POSReport.Function;
using Xceed.Wpf.Toolkit;
using CPC.POSReport.Properties;
using System.Data;

namespace CPC.POSReport.ViewModel
{
    class LoginViewModel : ViewModelBase, IDataErrorInfo
    {
        #region -Property- 

        #region -LoginName and Password-
        private string _loginname = string.Empty;
        /// <summary>
        /// Set or get Login Name
        /// </summary>
        public string LoginName
        {
            get { return _loginname; }
            set 
            {
                if (_loginname != value)
                {
                    _loginname = value;
                    OnPropertyChanged(()=>LoginName);
                }
            }
        }

        private string _password = string.Empty;
        /// <summary>
        /// Set or get Password
        /// </summary>
        public string Password
        {
            get { return _password; }
            set 
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(()=> Password);
                }
            }
        }
        #endregion

        private bool _isRemember = true;
        /// <summary>
        /// Set or Get Remember
        /// </summary>
        public bool IsRemember
        {
            get { return _isRemember; }
            set 
            {
                if (_isRemember != value)
                {
                    _isRemember = value;
                    OnPropertyChanged(()=> IsRemember);
                }
            }
        }
        
        #endregion

        #region -Define-
        rpt_UserRepository userRepo = new rpt_UserRepository();
        base_ConfigurationRepository configRepo = new base_ConfigurationRepository();
        public CPC.POSReport.View.LoginView loginWindow { get; set; }
        #endregion

        #region -Contructor-

        public LoginViewModel(CPC.POSReport.View.LoginView login)
        {
            LoginCommand = new RelayCommand(LoginExecute, CanLoginExecute);
            loginWindow = login;
            CheckRememberPwd();
        }
        #endregion

        #region -Login Command-
        /// <summary>
        /// Login command
        /// </summary>
        public ICommand LoginCommand { get; private set; }

        private void LoginExecute()
        {
            try
            {                               
                // Encrypt password
                string encryptPwd = (Settings.Default.IsRemember && LoginName.Equals(Settings.Default.LoginName, StringComparison.OrdinalIgnoreCase) && Password == Common.PWD_TEMP) ?
                            Settings.Default.Password : AESSecurity.Encrypt(Password);
                bool resuilt = false;
                // Check User Login
                if (LoginName.Equals(Settings.Default.AdminUser, StringComparison.OrdinalIgnoreCase))
                {
                    if(encryptPwd.Equals(Common.ADMIN_PWD))
                    {
                        SetFullRight();
                        resuilt = true;
                    }
                }
                else
                {
                    DBHelper dbHelper = new DBHelper();
                    string param = "'"+ LoginName +"','"+ encryptPwd + "'";
                    DataTable da = dbHelper.ExecuteQuery("sp_check_user_login", param);
                    if (da.Rows.Count > 0)
                    {
                        if (da.Rows[0][0] != DBNull.Value && !(bool)da.Rows[0][0])
                        {
                            MessageBox.Show("Account not active", "Login", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            return;
                        }
                        else if (da.Rows[0][1] != DBNull.Value && !(bool)da.Rows[0][1])
                        {
                            MessageBox.Show("Expiry date!", "Login", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            return;
                        }
                        resuilt = true;
                        if (da.Rows[0][2] != DBNull.Value)
                        {
                            Common.USER_RESOURCE = "'" + da.Rows[0][2].ToString() + "'";
                        }
                        Common.IS_ADMIN = false;
                    }
                }
                if (resuilt)
                {
                    Common.LOGIN_NAME = LoginName;
                    // Open Main window
                    Main main = new Main();
                    App.Current.MainWindow = main;
                    main.DataContext = new MainViewModel();
                    main.Show();                    
                    // Close Login window
                    loginWindow.Close();                    
                    if (IsRemember != Settings.Default.IsRemember || (Settings.Default.IsRemember && Settings.Default.LoginName != LoginName))
                    {
                        // Update Default settings
                        UpdateDefaultSetting(encryptPwd);
                    }
                    // Get keep log
                    var config = configRepo.Get(x => x.Id > 0);
                    if (config != null)
                    {
                        Common.KEEP_LOG = (short)config.KeepLog;
                    }
                }
                else
                {                    
                    MessageBox.Show("Wrong username or password.", "Login", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanLoginExecute()
        {
            return (Errors.Count == 0);
        }
        #endregion

        #region -Private method-

        #region -Set full right to admin-
        /// <summary>
        /// Set full right to admin
        /// </summary>
        private void SetFullRight()
        {
            Common.SET_PRINT_COPY = true;
            Common.PRINT_REPORT = true;
            Common.PREVIEW_REPORT = true;
            Common.VIEW_SET_COPY = true;
            Common.NEW_SET_COPY = true;
            Common.DELETE_SET_COPY = true;
            Common.ADD_REPORT = true;
            Common.EDIT_REPORT = true;
            Common.DELETE_REPORT = true;
            Common.CHANGE_GROUP_REPORT = true;
            Common.NO_SHOW_REPORT = true;
            Common.SET_ASSIGN_AUTHORIZE_REPORT = true;
            Common.SET_PERMISSION = true;
            Common.IS_ADMIN = true;
        }
        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        /// <summary>
        /// Get or set Errors string.
        /// </summary>
        protected Dictionary<string, string> _errors = new Dictionary<string, string>();
        public Dictionary<string, string> Errors
        {
            get { return _errors; }
        }


        public string this[string columnName]
        {
            get
            {
                string message = null;
                this.Errors.Remove(columnName);
                System.Text.RegularExpressions.Regex regex;
                switch (columnName)
                {
                    case "LoginName":
                        if (!string.IsNullOrEmpty(LoginName) && !LoginName.Contains(' '))
                        {
                            regex = new System.Text.RegularExpressions.Regex(Common.LOGIN_NAME_FORMAT);
                            if (!regex.IsMatch(LoginName))
                            {
                                message = "Login name must a-z and length of 5-20 characters";
                            }
                        }
                        else
                        {                            
                            message = "Login name is required.";
                        }
                        break;
                    case "Password":
                        if (!string.IsNullOrEmpty(Password))
                        {
                            regex = new System.Text.RegularExpressions.Regex(Common.PASSWORD_FORMAT);
                            if (!regex.IsMatch(Password))
                            {
                                message = "Password must be 8 to 20 characters\n" +
                                             "and contain uppercase, lowercase characters and special or numeric characters!";
                            }
                        }
                        else
                        {
                            message = "Password is required.";
                        }
                        break;
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    this.Errors.Add(columnName, message);
                }
                return message;
            }
        }

        #endregion

        /// <summary>
        /// Check User is remember password
        /// </summary>
        private void CheckRememberPwd()
        {
            IsRemember = Settings.Default.IsRemember;
            if (IsRemember)
            {
                LoginName = Settings.Default.LoginName;
                Password = Common.PWD_TEMP;
            }
        }

        /// <summary>
        /// Update Default value Setting
        /// </summary>
        private void UpdateDefaultSetting(string pwd)
        {
            if (IsRemember)
            {
                // Update Username & Password
                Settings.Default.LoginName = LoginName;
                Settings.Default.Password = pwd;
            }
            else
            {
                // Clear Username & Password
                Settings.Default.LoginName = string.Empty;
                Settings.Default.Password = string.Empty;
            }
            Settings.Default.IsRemember = IsRemember;
            Settings.Default.Save();
        }
        #endregion
    }
}
