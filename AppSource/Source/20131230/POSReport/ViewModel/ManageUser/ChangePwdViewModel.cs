using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;
using CPC.POSReport.Model;
using Toolkit.Command;
using System.ComponentModel;
using CPC.POSReport.Function;
using CPC.POSReport.Repository;
using Xceed.Wpf.Toolkit;
using SecurityLib;
using CPC.POSReport.Properties;

namespace CPC.POSReport.ViewModel
{
    class ChangePwdViewModel : ModelBase, IDataErrorInfo
    {
        #region -Properties-
        private string _loginName;
        /// <summary>
        /// Set or get Login Name
        /// </summary>
        public string LoginName
        {
            get { return _loginName; }
            set {_loginName = value; }
        }

        private string _pwd;
        /// <summary>
        /// Set or get Password
        /// </summary>
        public string Password
        {
            get { return _pwd; }
            set
            {
                if (_pwd != value)
                {
                    _pwd = value;
                    OnPropertyChanged(() => Password);
                }
            }
        }

        private string _newPwd;
        /// <summary>
        /// Set or get new Password
        /// </summary>
        public string NewPassword
        {
            get { return _newPwd; }
            set
            {
                if (_newPwd != value)
                {
                    _newPwd = value;
                    OnPropertyChanged(() => NewPassword);
                    PropertyChangedCompleted(() => NewPassword);
                }
            }
        }

        private string _confirmPwd;
        /// <summary>
        /// Set or get Confirm Password
        /// </summary>
        public string ConfirmPassword
        {
            get { return _confirmPwd; }
            set
            {
                if (_confirmPwd != value)
                {
                    _confirmPwd = value;
                    OnPropertyChanged(() => ConfirmPassword);
                    PropertyChangedCompleted(() => ConfirmPassword);
                }
            }
        }
        #endregion

        #region -Defines-
        rpt_UserRepository userRepo = new rpt_UserRepository();
        private View.ChangePasswordView ChangePasswordView { set; get; }
        #endregion
        
        #region -Contructor-
        public ChangePwdViewModel(View.ChangePasswordView changePasswordView)
        {
            LoginName = Common.LOGIN_NAME;
            InitCommand();
            ChangePasswordView = changePasswordView;
        }
        
        #endregion

        #region -Command-
        /// <summary>
        /// Init command
        /// </summary>
        private void InitCommand()
        {
            OkCommand = new RelayCommand(OkExecute, CanOkExecute);
            CancelCommand = new RelayCommand(CancelExecute);
        }

        #region -Ok Commmad-
        /// <summary>
        /// Set or get Ok Command
        /// </summary>
        public RelayCommand OkCommand { get; set; }

        public void OkExecute()
        {
            try
            {
                string encryptPwd = string.Empty;
                encryptPwd = AESSecurity.Encrypt(Password);
                var user = userRepo.Get(x => x.LoginName == LoginName && x.Password == encryptPwd);
                if (user != null)
                {
                    encryptPwd = AESSecurity.Encrypt(NewPassword);
                    rpt_UserModel ur = new rpt_UserModel(user);
                    ur.Password = encryptPwd;
                    ur.ToEntity();
                    userRepo.Commit();
                    // Clear remember password
                    if (Settings.Default.IsRemember)
                    {
                        Settings.Default.IsRemember = false;
                        Settings.Default.LoginName = string.Empty;
                        Settings.Default.Password = string.Empty;
                        Settings.Default.Save();
                    } 
                    MessageBox.Show("Password changed successfully.", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    ChangePasswordView.Close();
                }
                else
                {
                    MessageBox.Show("Password change failed.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public bool CanOkExecute()
        {
            return Errors.Count == 0;
        }
        #endregion

        #region -Cancel Command-
        /// <summary>
        /// Set or get Cancel command
        /// </summary>
        public RelayCommand CancelCommand { get; set; }

        public void CancelExecute()
        {
            ChangePasswordView.Close();            
        }
        #endregion

        #endregion

        #region IDataErrorInfo Members

        protected override void PropertyChangedCompleted(string propertyName)
        {
            base.PropertyChangedCompleted(propertyName);
            if (propertyName == "ConfirmPassword")
            {
                OnPropertyChanged(()=> NewPassword);
            }
            if (propertyName == "NewPassword")
            {
                OnPropertyChanged(() => ConfirmPassword);
            }
        }

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
                        if (!string.IsNullOrEmpty(LoginName))
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
                    case "NewPassword":
                        if (!string.IsNullOrEmpty(NewPassword))
                        {
                            regex = new System.Text.RegularExpressions.Regex(Common.PASSWORD_FORMAT);
                            if (!regex.IsMatch(NewPassword))
                            {
                                message = "Password must be 8 to 20 characters\n" +
                                             "and contain uppercase, lowercase characters and special or numeric characters!";
                            }
                        }
                        else
                        {
                            message = "New Password is required.";
                        }
                        break;
                    case "ConfirmPassword":
                        if (!string.IsNullOrEmpty(ConfirmPassword))
                        {
                            if (this.NewPassword != ConfirmPassword)
                            {
                                message = "Confirm password is mismatch";
                            }
                        }
                        else
                        {
                            message = "Confirm password is required";
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
    }
}
