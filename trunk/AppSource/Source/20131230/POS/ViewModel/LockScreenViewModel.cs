using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using SecurityLib;

namespace CPC.POS.ViewModel
{
    class LockScreenViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define

        private Regex _regexPassWord = new Regex(Define.CONFIGURATION.PasswordFormat);

        private base_ResourceAccountRepository _accountRepository = new base_ResourceAccountRepository();
        private Window _currentView;

        #endregion

        #region Constructors

        public LockScreenViewModel(Window currentView)
        {
            _ownerViewModel = this;
            InitialCommand();
            _currentView = currentView;

            //Set UserName Default
            UserName = Define.USER.LoginName;
            LoginName = Define.USER.LoginName;
            InitialView();
        }

        #endregion

        #region Properties

        #region LoginName

        private string _loginName;
        /// <summary>
        /// Gets or sets the LoginName.
        /// </summary>
        public string LoginName
        {
            get { return _loginName; }
            set
            {
                if (_loginName != value)
                {
                    _loginName = value;
                    OnPropertyChanged(() => LoginName);
                }
            }
        }

        #endregion

        #region UserName

        private string _userName;
        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                }
            }
        }

        #endregion

        #region UserPassword

        private string _userPassword;
        /// <summary>
        /// Gets or sets the UserPassword.
        /// </summary>
        public string UserPassword
        {
            get { return _userPassword; }
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    OnPropertyChanged(() => UserPassword);
                }
            }
        }

        #endregion

        #endregion

        #region Commands Methods

        #region LoginCommand

        /// <summary>
        /// Gets the Login Command.
        /// <summary>
        public RelayCommand<object> LoginCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Login command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoginCommandCanExecute(object param)
        {
            return Errors.Count == 0;
        }

        /// <summary>
        /// Method to invoke when the Login command is executed.
        /// </summary>
        private void OnLoginCommandExecute(object param)
        {
            try
            {
                bool result = false;

                // Encrypt password
                string encryptPassword = AESSecurity.Encrypt(UserPassword);

                // Check default account
                if (UserName.Equals(Define.ADMIN_ACCOUNT) && encryptPassword.Equals(Define.ADMIN_PASSWORD))
                    result = true;
                else if (UserName.Equals(LoginName)) // Check login account
                {
                    // Get login account from database
                    base_ResourceAccount account = _accountRepository.Get(x => x.LoginName.Equals(UserName) && x.Password.Equals(encryptPassword));
                    result = account != null;
                }

                if (result)
                {
                    // Clear user password
                    UserPassword = string.Empty;

                    // Turn off lock screen view
                    if (_currentView != null)
                        _currentView.DialogResult = true;
                }
                else
                {
                    // Show alert message
                    Xceed.Wpf.Toolkit.MessageBox.Show("Username or Password is not valid, please try again!", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            LoginCommand = new RelayCommand<object>(OnLoginCommandExecute, OnLoginCommandCanExecute);

        }

        private void InitialView()
        {
            if (_currentView == null)
                return;
            // Get main window
            Window mainWindow = App.Current.MainWindow;

            _currentView.Width = mainWindow.ActualWidth;
            _currentView.Height = mainWindow.ActualHeight;

            switch (mainWindow.WindowState)
            {
                case WindowState.Maximized:
                    _currentView.Top = 0;
                    _currentView.Left = 0;
                    break;
                case WindowState.Minimized:
                case WindowState.Normal:
                    _currentView.Top = mainWindow.Top;
                    _currentView.Left = mainWindow.Left;
                    break;
            }
        }

        #endregion

        #region IDataError

        protected HashSet<string> _errors = new HashSet<string>();
        /// <summary>
        /// <para> Gets or sets the ExtensionErrors </para>
        /// </summary>
        public HashSet<string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                if (_errors != value)
                {
                    _errors = value;
                    OnPropertyChanged(() => Errors);
                }
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = null;
                this.Errors.Clear();

                switch (columnName)
                {
                    case "UserPassword":
                        if (string.IsNullOrEmpty(UserPassword))
                            message = "Password is required.";
                        else if (!_regexPassWord.IsMatch(UserPassword))
                            message = "Password must a-z and length of 3-50 characters";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(message))
                    this.Errors.Add(columnName);

                return message;
            }
        }

        #endregion
    }
}
