using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Report;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.Toolkit.Layout;
using Npgsql;
using SecurityLib;

namespace CPC.POS.ViewModel
{
    partial class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Layout Defines

        // Number of rows
        private int _rowNumbers = 4;

        // Number of columns
        private int _columnNumbers = 2;

        // Host view list
        private List<BorderLayoutHost> _hostList = new List<BorderLayoutHost>();

        // Grid contain hosts
        private Grid _grdHost;

        // Grid contain targets
        private Grid _grdTarget;

        // Store expand status of view
        private bool _isPanelExpanded;

        // Store columns to expand
        private ColumnDefinition _colSubItem;
        private ColumnDefinition _colSubItemExpanded;

        #endregion

        #region Defines
        // Current view name
        string curViewName = string.Empty;
        private base_UserLogRepository _userLogRepository = new base_UserLogRepository();
        private base_ResourceAccountRepository _accountRepository = new base_ResourceAccountRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_SaleTaxLocationRepository _saleTaxLocationRepository = new base_SaleTaxLocationRepository();
        private base_LayawayManagerRepository _layawayManagerRepository = new base_LayawayManagerRepository();

        private DispatcherTimer _idleTimer;
        private Regex _regexPassWord = new Regex(Define.CONFIGURATION.PasswordFormat);

        private LockScreenView _lockScreenView;
        private bool _isSwitchDatabase;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the IsAnimationCompletedAll
        /// </summary>
        public bool IsAnimationCompletedAll
        {
            get
            {
                return _hostList.Count(x => !x.IsAnimationCompleted) == 0;
                //return _hostList.Count(x => !x.IsAnimationCompleted || x.ViewModelBase.IsBusy) == 0;
            }
        }

        private ObservableCollection<string> _hiddenHostList = new ObservableCollection<string>();
        /// <summary>
        /// Gets or sets the HiddenHostList.
        /// </summary>
        public ObservableCollection<string> HiddenHostList
        {
            get
            {
                return _hiddenHostList;
            }
            set
            {
                if (_hiddenHostList != value)
                {
                    _hiddenHostList = value;
                    OnPropertyChanged(() => HiddenHostList);
                }
            }
        }

        private string _selectedHiddenHost;
        /// <summary>
        /// Gets or sets the SelectedHiddenHost.
        /// </summary>
        public string SelectedHiddenHost
        {
            get
            {
                return _selectedHiddenHost;
            }
            set
            {
                if (_selectedHiddenHost != value)
                {
                    _selectedHiddenHost = value;
                    OnPropertyChanged(() => SelectedHiddenHost);

                    if (SelectedHiddenHost != null)
                    {
                        int index = HiddenHostList.IndexOf(SelectedHiddenHost);
                        HiddenHostList.RemoveAt(index);
                        var host = _hostList.ElementAt(_hostList.Count - 1 - index);
                        ChangeLayoutItem(host.Container.btnImageIcon);
                        HiddenHostList.Add(_hostList.ElementAt(_rowNumbers + 1).Container.btnTitle.Content.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Gets the HiddenHostVisibility
        /// </summary>
        public Visibility HiddenHostVisibility
        {
            get
            {
                return HiddenHostList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string _loginName;
        /// <summary>
        /// Gets or sets the LoginName.
        /// </summary>
        public string LoginName
        {
            get
            {
                return _loginName;
            }
            set
            {
                if (_loginName != value)
                {
                    _loginName = value;
                    OnPropertyChanged(() => LoginName);
                }
            }
        }

        private string _userName = string.Empty;
        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                }
            }
        }

        private string _userPassword = string.Empty;
        /// <summary>
        /// Gets or sets the UserPassword.
        /// </summary>
        public string UserPassword
        {
            get
            {
                return _userPassword;
            }
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    OnPropertyChanged(() => UserPassword);
                }
            }
        }

        /// <summary>
        /// Gets the Status
        /// </summary>
        public string Status
        {
            get
            {
                return "Connecting...";
            }
        }

        /// <summary>
        /// Gets the Server
        /// </summary>
        public string Server
        {
            get
            {
                if (Define.USER != null)
                    return Define.USER.IpAddress;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the Database
        /// </summary>
        public string Database
        {
            get;
            private set;
        }

        private bool _isLockScreen;
        /// <summary>
        /// Gets or sets the IsLockScreen.
        /// </summary>
        public bool IsLockScreen
        {
            get
            {
                return _isLockScreen;
            }
            set
            {
                if (_isLockScreen != value)
                {
                    _isLockScreen = value;
                    OnPropertyChanged(() => IsLockScreen);
                }
            }
        }

        private string _storeName;
        /// <summary>
        /// Gets or sets the StoreName.
        /// </summary>
        public string StoreName
        {
            get
            {
                return _storeName;
            }
            set
            {
                if (_storeName != value)
                {
                    _storeName = value;
                    OnPropertyChanged(() => StoreName);
                }
            }
        }

        private string _taxLocation;
        /// <summary>
        /// Gets or sets the TaxLocation.
        /// </summary>
        public string TaxLocation
        {
            get
            {
                return _taxLocation;
            }
            set
            {
                if (_taxLocation != value)
                {
                    _taxLocation = value;
                    OnPropertyChanged(() => TaxLocation);
                }
            }
        }

        private string _taxCode;
        /// <summary>
        /// Gets or sets the TaxCode.
        /// </summary>
        public string TaxCode
        {
            get
            {
                return _taxCode;
            }
            set
            {
                if (_taxCode != value)
                {
                    _taxCode = value;
                    OnPropertyChanged(() => TaxCode);
                }
            }
        }

        /// <summary>
        /// Gets the DashboardVisibility.
        /// </summary>
        public Visibility DashboardVisibility
        {
            get
            {
                return _hostList.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets the IsPracticeMode.
        /// </summary>
        public bool IsPracticeMode
        {
            get
            {
                return Database.Contains("train");
                ;
            }
        }

        private Skins _selectedSkin;
        /// <summary>
        /// Gets or sets the SelectedSkin.
        /// </summary>
        public Skins SelectedSkin
        {
            get
            {
                return _selectedSkin;
            }
            set
            {
                if (_selectedSkin != value)
                {
                    _selectedSkin = value;
                    OnPropertyChanged(() => SelectedSkin);

                    try
                    {
                        // Change ribbon image folder by selected skin
                        RibbonImageFolder = string.Format("/Image/RibbonImages/{0}/", SelectedSkin);

                        // Get color resource
                        ResourceDictionary colorResource = Application.Current.Resources.MergedDictionaries[1];

                        // Get image resource
                        ResourceDictionary imageResource = Application.Current.Resources.MergedDictionaries[2];

                        // Change color and image source by selected skin
                        colorResource.Source = new Uri(string.Format(@"..\Dictionary\Brushes\{0}\{0}Resources.xaml", SelectedSkin), UriKind.Relative);
                        imageResource.Source = new Uri(string.Format(@"..\Dictionary\Brushes\{0}\{0}ImageResources.xaml", SelectedSkin), UriKind.Relative);

                        colorResource = Application.Current.Resources.MergedDictionaries[1];
                        imageResource = Application.Current.Resources.MergedDictionaries[2];
                    }
                    catch (Exception ex)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "POS", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #region SelectedLanguage

        private string _iconLanguagePath = @"/Image/RibbonImages/RibbonLanguage.png";
        /// <summary>
        /// Gets or sets the IconLanguagePath.
        /// </summary>
        public string IconLanguagePath
        {
            get
            {
                return _iconLanguagePath;
            }
            set
            {
                if (_iconLanguagePath != value)
                {
                    _iconLanguagePath = value;
                    OnPropertyChanged(() => IconLanguagePath);
                }
            }
        }

        private ComboItem _selectedLanguage;
        /// <summary>
        /// Gets or sets the SelectedLanguage.
        /// </summary>
        public ComboItem SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged(() => SelectedLanguage);

                    Common.ChangeLanguage(_selectedLanguage.CultureInfo);

                    foreach (ComboItem comboItem in Common.Languages)
                    {
                        // Reset languages checked
                        comboItem.Flag = false;
                    }

                    // Checked new language is selected
                    SelectedLanguage.Flag = true;

                    string iconLanguagePath = @"/Image/RibbonImages/";
                    IconLanguagePath = string.Format("{0}RibbonLanguage{1}.png", iconLanguagePath, SelectedLanguage.Code);
                }
            }
        }

        #endregion

        private string _ribbonImageFolder;
        /// <summary>
        /// Gets or sets the RibbonImageFolder.
        /// </summary>
        public string RibbonImageFolder
        {
            get
            {
                return _ribbonImageFolder;
            }
            set
            {
                if (_ribbonImageFolder != value)
                {
                    _ribbonImageFolder = value;
                    OnPropertyChanged(() => RibbonImageFolder);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainViewModel()
            : base()
        {
            InitialCommand();
            CheckIdleTime();
            LoadLayout();
            LoadStatusInformation();

            SelectedSkin = Skins.Grey;
            SelectedLanguage = Common.Languages.FirstOrDefault(x => x.Code.Equals(Define.CONFIGURATION.DefaultLanguage));

            // Get permission
            GetPermission();

            Initialize();
        }

        #endregion

        #region Command Methods

        #region OpenViewCommand

        /// <summary>
        /// Gets the OpenViewCommand command.
        /// </summary>
        public ICommand OpenViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the OpenViewCommand command is executed.
        /// </summary>
        private void OnOpenViewCommandExecute(object param)
        {
            OpenViewExecute(param.ToString());
        }

        #endregion

        #region ChangeViewCommand

        /// <summary>
        /// Gets the ChangeViewCommand command.
        /// </summary>
        public ICommand ChangeViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the ChangeViewCommand command is executed.
        /// </summary>
        private void OnChangeViewCommandExecute()
        {
            if (HiddenHostList.Count > 0)
            {
                HiddenHostList.RemoveAt(0);
                HiddenHostList.Add(_hostList.ElementAt(_rowNumbers + 1).Container.btnTitle.Content.ToString());
            }
            var host = _hostList.LastOrDefault();
            if (host != null)
                ChangeLayoutItem(host);
        }

        #endregion

        #region CloseViewCommand

        /// <summary>
        /// Gets the CloseViewCommand command.
        /// </summary>
        public ICommand CloseViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the CloseViewCommand command is executed.
        /// </summary>
        private void OnCloseViewCommandExecute(object param)
        {
            int index = HiddenHostList.IndexOf(param.ToString());
            if (index >= 0 && index < HiddenHostList.Count)
            {
                HiddenHostList.RemoveAt(index);
                OnPropertyChanged(() => HiddenHostVisibility);
                _hostList.RemoveAt(_hostList.Count - 1 - index);
            }
        }

        #endregion

        #region ClearViewCommand

        /// <summary>
        /// Gets the ClearViewCommand command.
        /// </summary>
        public ICommand ClearViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the ClearViewCommand command is executed.
        /// </summary>
        private void OnClearViewCommandExecute()
        {
            // Clear host list
            _hostList.Clear();
            HiddenHostList.Clear();
            OnPropertyChanged(() => HiddenHostVisibility);
            // Clear UIElement hosts
            _grdHost.Children.Clear();
        }

        #endregion

        #region CloseCommand

        /// <summary>
        /// Gets the CloseCommand command.
        /// </summary>
        public ICommand CloseCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the CloseCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCloseCommandCanExecute()
        {
            if (_hostList.Count == 0)
                return false;
            BorderLayoutHost currentHost = _hostList.ElementAt(0) as BorderLayoutHost;
            return !currentHost.ShowNotification(true);
        }

        /// <summary>
        /// Method to invoke when the CloseCommand command is executed.
        /// </summary>
        private void OnCloseCommandExecute()
        {
            // To write log into base_UserLogDetail table.
            App.WriteUserLog("Exit", "User closed the application.");
            // To Update status of user.
            this.UpdateUserLog();

            if (_isSwitchDatabase)
            {
                string connectionName = "POSDBEntities";
                string appConfigPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                // Get content app config file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(appConfigPath);

                // Get connection string node
                XmlNode connectionStringsNode = xmlDoc.SelectSingleNode("configuration/connectionStrings");
                if (connectionStringsNode != null)
                {
                    foreach (XmlNode childNode in connectionStringsNode)
                    {
                        if (childNode.Attributes["name"].Value.Equals(connectionName))
                        {
                            // Get current connection string value
                            string connectionValue = childNode.Attributes["connectionString"].Value;

                            // Initial entity connection string builder
                            EntityConnectionStringBuilder entityConnectionBuilder = new EntityConnectionStringBuilder(connectionValue);

                            string databaseName = string.Format("Database={0}", Database);
                            string trainDBName = string.Format("Database=train_{0}", Database);
                            if (IsPracticeMode)
                                trainDBName = string.Format("Database={0}", Database.Split('_').LastOrDefault());

                            // Modify database name
                            entityConnectionBuilder.ProviderConnectionString = entityConnectionBuilder.ProviderConnectionString.Replace(databaseName, trainDBName);

                            // Update new connection string
                            childNode.Attributes["connectionString"].Value = entityConnectionBuilder.ConnectionString;

                            // Save app config file
                            xmlDoc.Save(appConfigPath);

                            // Update connection string in configuration manager
                            ConfigurationManager.RefreshSection("connectionStrings");

                            // Reload unit of work
                            UnitOfWork.Reload();
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region ChangeStyleCommand

        /// <summary>
        /// Gets the ChangeStyleCommand command.
        /// </summary>
        public ICommand ChangeStyleCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ChangeStyleCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChangeStyleCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ChangeStyleCommand command is executed.
        /// </summary>
        private void OnChangeStyleCommandExecute(object param)
        {
            //int count = Application.Current.Resources.MergedDictionaries.Count;
            //ResourceDictionary skin = Application.Current.Resources.MergedDictionaries[count - 2];
            SelectedSkin = (Skins)param;

            //switch (SelectedSkin)
            //{
            //    case Skins.Blue:
            //        skin.Source = new Uri(string.Format(@"..\Dictionary\Brushes\Blue\{0}Resources.xaml", param.ToString()), UriKind.Relative);
            //        skin = Application.Current.Resources.MergedDictionaries[count - 1];
            //        break;
            //    case Skins.Grey:
            //        skin.Source = new Uri(string.Format(@"..\Dictionary\Brushes\Grey\{0}Resources.xaml", param.ToString()), UriKind.Relative);
            //        skin = Application.Current.Resources.MergedDictionaries[count - 1];
            //        break;
            //    case Skins.Red:
            //        skin.Source = new Uri(string.Format(@"..\Dictionary\Brushes\Red\{0}Resources.xaml", param.ToString()), UriKind.Relative);
            //        skin = Application.Current.Resources.MergedDictionaries[count - 1];
            //        break;
            //}

        }

        #endregion

        #region LoginCommand

        /// <summary>
        /// Gets the LoginCommand command.
        /// </summary>
        public ICommand LoginCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the LoginCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoginCommandCanExecute()
        {
            return ExtensionErrors.Count == 0;
        }

        /// <summary>
        /// Method to invoke when the LoginCommand command is executed.
        /// </summary>
        private void OnLoginCommandExecute()
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
                    _lockScreenView.DialogResult = true;
                }
                else
                {
                    // Show alert message
                    Xceed.Wpf.Toolkit.MessageBox.Show("Username or Password is not valid, please try again!", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnLoginCommand" + ex.ToString());
            }
        }

        #endregion

        #region LogOutCommand

        /// <summary>
        /// Gets the LogOutCommand command.
        /// </summary>
        public ICommand LogOutCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Enable the Login button if all required field are validated IsValid = true
        ///Created by Thaipn.
        /// </summary>
        /// <returns></returns>
        private bool CanOnLogOutExecute()
        {
            return true;
        }

        /// <summary>
        /// Check the user login
        /// </summary>
        private void OnLogOutExecuted()
        {
            if (App.Current.MainWindow is MainWindow)
            {
                // Stop idle timer
                _idleTimer.Stop();
                //To Update status of user.
                //this.UpdateUserLog();
                //To write log into base_UserLogDetail table.
                App.WriteUserLog("Logout", "User logged out the application.");
                // To LogOut Windows.
                App.Messenger.NotifyColleagues(Define.USER_LOGOUT_RESULT);

                //Reload context. cause some object is stored in objectcontext after that.
                UnitOfWork.Reload();
            }
        }

        #endregion

        #region OpenManagementUserCommand

        /// <summary>
        /// Gets the OpenManagementUserCommand command.
        /// </summary>
        public ICommand OpenManagementUserCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the OpenViewCommand command is executed.
        /// </summary>
        private void OnOpenManagermentUserCommandExecute(object param)
        {
            ManagementUserLogView view = new ManagementUserLogView();
            view.DataContext = new ManagementUserLogViewModel();
            view.Show();
        }

        #endregion

        #region ChangePasswordCommand

        /// <summary>
        /// Gets the ChangePasswordCommand command.
        /// </summary>
        public ICommand ChangePasswordCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ChangePasswordCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChangePasswordCommandCanExecute()
        {
            if (Define.USER == null)
                return false;
            return !Define.ADMIN_ACCOUNT.Equals(Define.USER.LoginName);
        }

        /// <summary>
        /// Method to invoke when the ChangePasswordCommand command is executed.
        /// </summary>
        private void OnChangePasswordCommandExecute()
        {
            ChangePasswordViewModel viewModel = new ChangePasswordViewModel();
            bool? result = _dialogService.ShowDialog<ChangePasswordView>(this, viewModel, "Change Password");
            if (result.HasValue && result.Value)
            {
                LoginName = viewModel.ResourceAccountModel.LoginName;
            }
        }

        #endregion

        #region LockScreenCommand

        /// <summary>
        /// Gets the LockScreenCommand command.
        /// </summary>
        public ICommand LockScreenCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the LockScreenCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLockScreenCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LockScreenCommand command is executed.
        /// </summary>
        public void OnLockScreenCommandExecute()
        {
            // Stop idle timer
            _idleTimer.Stop();

            // Set user name default
            UserName = LoginName;

            // Get main window
            Window mainWindow = App.Current.MainWindow;

            // Initial lock screen view
            _lockScreenView = new LockScreenView();
            _lockScreenView.DataContext = this;

            // Register closing event
            _lockScreenView.Closing += (senderLockScreen, eLockScreen) =>
            {
                // Prevent closing lock screen view when login have not success
                if (!_lockScreenView.DialogResult.HasValue)
                    eLockScreen.Cancel = true;
            };

            // Set default position over main window
            _lockScreenView.Width = mainWindow.ActualWidth;
            _lockScreenView.Height = mainWindow.ActualHeight;
            switch (mainWindow.WindowState)
            {
                case WindowState.Maximized:
                    _lockScreenView.Top = 0;
                    _lockScreenView.Left = 0;
                    break;
                case WindowState.Minimized:
                case WindowState.Normal:
                    _lockScreenView.Top = mainWindow.Top;
                    _lockScreenView.Left = mainWindow.Left;
                    break;
            }

            // Set login binding
            Binding loginBinding = new Binding("LoginCommand");
            BindingOperations.SetBinding(_lockScreenView.btnLogin, Button.CommandProperty, loginBinding);

            // Get active window if main show popup
            Window activeWindow = mainWindow.OwnedWindows.Cast<Window>().SingleOrDefault(x => x.IsActive);
            if (activeWindow != null)
            {
                // Set active window is owner lock screen view
                _lockScreenView.Owner = activeWindow;
            }
            else
            {
                // Set main window is owner lock screen view
                _lockScreenView.Owner = mainWindow;
            }

            // Show lock screen view
            if (_lockScreenView.ShowDialog().HasValue)
            {
                mainWindow.Activate();

                IdleTimeHelper.LostFocusTime = null;

                // Star idle timer
                _idleTimer.Start();
            }
        }

        #endregion

        #region -Help Command-

        /// <summary>
        /// Gets the HelpCommand command.
        /// </summary>
        public RelayCommand HelpCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the HelpCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnHelpCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the HelpCommand command is executed.
        /// </summary>
        public void OnHelpCommandExecute()
        {
            OpenHelpFile();
        }

        /// <summary>
        /// Open help file
        /// </summary>
        private void OpenHelpFile()
        {
            try
            {
                // Help file path
                string helpFilePath = @"Language/Help/test.chm";
                if (curViewName.Length == 0)
                {
                    System.Windows.Forms.Help.ShowHelp(null, helpFilePath);
                }
                else
                {
                    System.Windows.Forms.Help.ShowHelp(null, helpFilePath, System.Windows.Forms.HelpNavigator.KeywordIndex, curViewName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion

        #region DataCommand

        /// <summary>
        /// Gets the LockScreenCommand command.
        /// </summary>
        public RelayCommand<object> DataCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the LockScreenCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDataCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LockScreenCommand command is executed.
        /// </summary>
        public void OnDataCommandExecute(object param)
        {
            try
            {
                switch (param.ToString())
                {
                    case "Backup":
                        MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text38, Language.Information, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            //Backup data
                            BackgroundWorker bgWorker = new BackgroundWorker
                            {
                                WorkerReportsProgress = true
                            };
                            bgWorker.DoWork += (sender, e) =>
                            {
                                // Turn on BusyIndicator
                                if (Define.DisplayLoading)
                                    IsBusy = true;
                                BackupRestoreHelper.BackupDB();
                            };
                            bgWorker.RunWorkerCompleted += (sender, e) =>
                            {
                                // Turn off BusyIndicator
                                IsBusy = false;
                                if (BackupRestoreHelper.SuccessfulFlag == 1)
                                {
                                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text32, Language.Information, System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                                    //LogOut Windows.
                                    App.Messenger.NotifyColleagues(Define.USER_LOGOUT_RESULT);

                                    //Reload context. cause some object is stored in objectcontext after that.
                                    UnitOfWork.Reload();
                                }
                                else
                                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text33, Language.Warning, System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                            };
                            // Run async background worker
                            bgWorker.RunWorkerAsync();
                        }
                        break;

                    case "Restore":
                        MessageBoxResult resultRestore = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text39, Language.Information, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (resultRestore == MessageBoxResult.Yes)
                        {
                            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
                            openFile.InitialDirectory = BackupRestoreHelper.BackupPath;
                            openFile.Filter = "File (*.backup)|*.backup";
                            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                //Backup data
                                BackgroundWorker bgWorkerBackup = new BackgroundWorker
                                {
                                    WorkerReportsProgress = true
                                };
                                bgWorkerBackup.DoWork += (sender, e) =>
                                {
                                    // Turn on BusyIndicator
                                    if (Define.DisplayLoading)
                                        IsBusy = true;
                                    BackupRestoreHelper.BackupDB();
                                };
                                // Run async background worker
                                bgWorkerBackup.RunWorkerAsync();

                                BackgroundWorker bgWorker = new BackgroundWorker
                                {
                                    WorkerReportsProgress = true
                                };
                                bgWorker.DoWork += (sender, e) =>
                                {
                                    // Turn on BusyIndicator
                                    if (Define.DisplayLoading)
                                        IsBusy = true;
                                    BackupModel model = new BackupModel();
                                    model.Path = openFile.FileName;
                                    BackupRestoreHelper.RestoreDB(model);
                                };
                                bgWorker.RunWorkerCompleted += (sender, e) =>
                                {
                                    // Turn off BusyIndicator
                                    IsBusy = false;
                                    if (BackupRestoreHelper.SuccessfulFlag == 1)
                                    {
                                        Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text35, Language.Information, System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                                        //LogOut Windows.
                                        App.Messenger.NotifyColleagues(Define.USER_LOGOUT_RESULT);

                                        //Reload context. cause some object is stored in objectcontext after that.
                                        UnitOfWork.Reload();
                                    }
                                    else
                                        Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text36, Language.Warning, System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                                };
                                // Run async background worker
                                bgWorker.RunWorkerAsync();
                            }
                        }
                        break;

                    case "Clear":
                        MessageBoxResult resultClearAll = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text40, Language.Information, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (resultClearAll == MessageBoxResult.Yes)
                        {
                            BackupRestoreHelper.ClearAllData();
                            Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text37, Language.Warning);
                            //LogOut Windows.
                            App.Messenger.NotifyColleagues(Define.USER_LOGOUT_RESULT);
                            //Reload context. cause some object is stored in objectcontext after that.
                            UnitOfWork.Reload();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion

        #region ExitCommand

        /// <summary>
        /// Gets the ExitCommand command.
        /// </summary>
        public ICommand ExitCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ExitCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnExitCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ExitCommand command is executed.
        /// </summary>
        private void OnExitCommandExecute()
        {
            App.Current.MainWindow.Close();
        }

        #endregion

        #region SwitchDatabaseCommand

        /// <summary>
        /// Gets the SwitchDatabaseCommand command.
        /// </summary>
        public ICommand SwitchDatabaseCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the SwitchDatabaseCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSwitchDatabaseCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SwitchDatabaseCommand command is executed.
        /// </summary>
        private void OnSwitchDatabaseCommandExecute()
        {
            string mode = string.Format("Do you want to switch application to {0}?", IsPracticeMode ? Language.RealMode : Language.PracticeMode);
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(mode, "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                // Modify connection string when close main window
                _isSwitchDatabase = true;

                // Logout application
                App.Messenger.NotifyColleagues(Define.USER_LOGOUT_RESULT);

                ////Reload context. cause some object is stored in objectcontext after that.
                //UnitOfWork.Reload();
            }
        }

        #endregion

        #region ChangeLanguageCommand

        /// <summary>
        /// Gets the ChangeLanguageCommand command.
        /// </summary>
        public ICommand ChangeLanguageCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ChangeLanguageCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChangeLanguageCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ChangeLanguageCommand command is executed.
        /// </summary>
        private void OnChangeLanguageCommandExecute(object param)
        {
            //foreach (ComboItem comboItem in Common.Languages)
            //{
            //    // Reset languages checked
            //    comboItem.Flag = false;
            //}

            //// Checked new language is selected
            //(param as ComboItem).Flag = true;

            SelectedLanguage = param as ComboItem;
        }

        #endregion

        #region CloseDayCommand

        /// <summary>
        /// Gets the CloseDayCommand command.
        /// </summary>
        public ICommand CloseDayCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ExitCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCloseDayCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ExitCommand command is executed.
        /// </summary>
        private void OnCloseDayCommandExecute()
        {
            Xceed.Wpf.Toolkit.MessageBox.Show("Service does not exist !", Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            //Define.SynchronizationViewModel.SyncData();
        }

        #endregion

        #endregion

        #region Layout Methods

        /// <summary>
        /// Open a UserControl as view by name
        /// </summary>
        /// <param name="viewName">Name of view</param>
        /// <param name="param">Opening with parameter. Default parameter is null</param>
        public void OpenViewExecute(string viewName, object param = null)
        {
            // Check all animation is completed
            if (IsAnimationCompletedAll)
            {
                // Create new target
                BorderLayoutTarget target = new BorderLayoutTarget();

                // Define position for target
                _grdTarget.Children.Add(target);
                Grid.SetRowSpan(target, _rowNumbers);
                if (_hostList.Count == 0) // The largest target
                    Grid.SetColumnSpan(target, _columnNumbers);

                // Create new host with defined target
                BorderLayoutHost host = new BorderLayoutHost(target);
                host.SetHostName(viewName);

                // Set parameter for view
                if (param != null)
                    host.Tag = param;

                // Check parameter to open view
                var hostClicked = _hostList.FirstOrDefault(x => x.KeyName.Equals(host.KeyName));
                if (hostClicked == null)
                {
                    // Show notification when open a new form
                    if (_hostList.Count > 0 && !_hostList.ElementAt(0).ShowNotification(false))
                        return;

                    if (!CreateContainerView(host))
                        return;

                    if (_hostList.Count > 0)
                    {
                        // Set focus to title bar to active keybinding
                        _hostList.ElementAt(0).Container.btnTitle.Focus();

                        // Turn on screenshot
                        _hostList.ElementAt(0).AllowScreenShot = true;
                    }

                    _hostList.Insert(0, host);
                    _grdHost.Children.Insert(0, host);
                    host.Loaded += new RoutedEventHandler(host_Loaded);
                    SetPositionTarget();

                    if (_hostList.Count > _rowNumbers + 1)
                    {
                        HiddenHostList.Add(_hostList.ElementAt(_rowNumbers + 1).Container.btnTitle.Content.ToString());
                        OnPropertyChanged(() => HiddenHostVisibility);
                    }
                }
                else
                {
                    ChangeLayoutItem(hostClicked);
                    hostClicked.ViewModelBase.ChangeSearchMode(host.IsOpenList, host.Tag);
                }

                // Hide dashboard when open one view
                OnPropertyChanged(() => DashboardVisibility);
            }
        }

        /// <summary>
        /// Create container to contain view and register events
        /// </summary>
        /// <param name="host">Host contain container</param>
        /// <returns>True is create view. False is show popup</returns>
        private bool CreateContainerView(BorderLayoutHost host)
        {
            bool showPopup = false;
            host.Child = new ContainerView();
            UserControl view = new UserControl();
            ViewModelBase viewModel = new ViewModelBase();
            //TestViewModel testViewModel = new TestViewModel { TitleView = name };
            //host.SetDataContext(testViewModel);
            //host.DataContext = testViewModel;
            //host.Container.grdContent.Children.Add(view);
            host.Container.btnTitle.Content = host.DisplayName;
            host.Container.btnTitle.PreviewMouseDoubleClick += new MouseButtonEventHandler(btnTitle_MouseDoubleClick);
            host.Container.btnImageIcon.Click += new RoutedEventHandler(btnImageIcon_Click);
            host.Container.btnTitle.Click += new RoutedEventHandler(btnTitle_Click);
            host.Container.btnClose.Click += new RoutedEventHandler(btnClose_Click);
            host.Container.btnHelp.Click += new RoutedEventHandler(btnHelp_Click);
            host.Container.tgExpand.IsChecked = _isPanelExpanded;
            host.Container.tgExpand.Checked += new RoutedEventHandler(tgExpand_Checked);
            host.Container.tgExpand.Unchecked += new RoutedEventHandler(tgExpand_Unchecked);
            if (_hostList.Count == 0)
                host.Container.tgExpand.Visibility = Visibility.Collapsed;
            host.Container.btnImageIcon.Visibility = Visibility.Collapsed;
            curViewName = "Problem";
            switch (host.KeyName)
            {
                #region Application Menu

                case "CompanySetting":
                    view = new CompanySettingView();
                    viewModel = new CompanySettingViewModel();
                    //PopupGuestViewModel popupVendorViewModel = new PopupGuestViewModel(MarkType.Customer);
                    //_dialogService.ShowDialog<PopupGuestView>(this, popupVendorViewModel, null);
                    //var item = popupVendorViewModel.NewItem;
                    //showPopup = true;
                    break;
                case "Department":
                    view = new DepartmentView();
                    viewModel = new DepartmentViewModel();
                    break;

                case "CashIn":
                    if (_dialogService.ShowDialog<CashInView>(this, new CashInViewModel(), null).Value)
                        GetCashInOutPermission();
                    showPopup = true;
                    break;

                case "CashOut":
                    if (_dialogService.ShowDialog<CashOutView>(this, new CashOutViewModel(), null).Value)
                        GetCashInOutPermission();
                    showPopup = true;
                    break;

                #endregion

                #region Store Tab

                case "Employee":
                    view = new EmployeeInformationView();
                    viewModel = new EmployeeViewModel(host.IsOpenList);
                    break;
                #endregion

                #region Sales Tab

                case "Customer":
                    view = new CustomerView();
                    viewModel = new CustomerViewModel(host.IsOpenList);
                    break;
                case "RewardSetup":
                    view = new RewardSetupView();
                    viewModel = new RewardSetupViewModel();
                    break;
                case "RewardHistory":
                    view = new RewardListandHistoryView();
                    viewModel = new RewardListandHistoryViewModel();
                    break;
                case "GiftCard":
                    view = new GiftCardView();
                    viewModel = new GiftCardViewModel();
                    break;
                case "Quotation":
                    view = new QuotationView();
                    viewModel = new QuotationViewModel(host.IsOpenList);
                    break;

                case "LayawaySetup":
                    view = new LayawaySetupView();
                    viewModel = new LayawaySetupViewModel();
                    break;
                case "Layaway":
                    view = new LayawayView();
                    viewModel = new LayawayViewModel(host.IsOpenList);
                    break;
                case "SalesOrder":
                    view = new SalesOrderView();
                    viewModel = new SalesOrderViewModel(host.IsOpenList, host.Tag);
                    break;
                case "SalesOrderLocked":
                    view = new LockSalesOrderView();
                    viewModel = new LockSalesOrderViewModel(host.IsOpenList, host.Tag);
                    break;
                case "SalesOrderReturn":
                    _dialogService.ShowDialog<SalesOrderReturnSearchView>(this, new SalesOrderReturnSearchViewModel(), "Sale Order Return Search");
                    showPopup = true;
                    break;

                case "VoucherGiftCard":
                    view = new CertificateView();
                    viewModel = new CertificateViewModel(CertificateCardTypeId.GiftCard);
                    break;

                case "VoucherGiftCertificate":
                    view = new CertificateView();
                    viewModel = new CertificateViewModel(CertificateCardTypeId.GiftCertificate);
                    break;

                #endregion

                #region Purchase Tab

                case "Vendor":
                    view = new VendorView();
                    viewModel = new VendorViewModel(host.IsOpenList, host.Tag);
                    break;
                case "PurchaseOrder":
                    view = new PurchaseOrderView();
                    viewModel = new PurchaseOrderViewModel(host.IsOpenList, host.Tag);
                    break;
                case "PurchaseOrderLocked":
                    view = new LockPOListView();
                    viewModel = new LockPOListViewModel(host.IsOpenList, host.Tag);
                    break;
                case "PurchaseOrderReturn":
                    _dialogService.ShowDialog<POReturnSearchView>(this, new POReturnSearchViewModel(), null);
                    showPopup = true;
                    break;

                #endregion

                #region Inventory Tab

                case "Product":
                    view = new ProductView();
                    viewModel = new ProductViewModel(host.IsOpenList, host.Tag);
                    break;
                case "ProductManual":
                    view = new ProductManualView();
                    viewModel = new ProductManualViewModel(host.IsOpenList, host.Tag);
                    break;
                case "MovementHistory":
                    view = new ProductMovementHistoryView();
                    viewModel = new ProductMovementHistoryViewModel();
                    break;
                case "Pricing":
                    view = new PricingView();
                    viewModel = new PricingViewModel(host.IsOpenList);
                    break;
                case "PricingList":
                    view = new PriceManagementView();
                    viewModel = new PriceManagementViewModel();
                    break;
                case "Discount":
                    view = new PromotionView();
                    viewModel = new PromotionViewModel(host.IsOpenList);
                    break;
                case "CountSheet":
                    view = new CountSheetView();
                    viewModel = new CountSheetViewModel(host.IsOpenList);
                    break;
                case "CountSheetList":
                    view = new CountSheetView();
                    viewModel = new CountSheetViewModel();
                    break;
                case "StockAdjustment":
                    view = new AdjustmentInformationView();
                    viewModel = new AdjustmentInformationViewModel();
                    break;
                case "StockAdjustmentList":
                    view = new AdjustmentInformationView();
                    viewModel = new AdjustmentInformationViewModel();
                    break;
                case "TransferStock":
                    view = new TransferStockView();
                    viewModel = new TransferStockViewModel(host.IsOpenList, host.Tag);
                    break;
                case "ReOrderStock":
                    view = new ReOrderStockView();
                    viewModel = new ReorderStockViewModel();
                    break;
                case "WorkOrder":
                    view = new WorkOrderView();
                    viewModel = new WorkOrderViewModel(host.IsOpenList);
                    break;
                case "CostAdjustment":
                    view = new CostAdjustmentHistoryView();
                    viewModel = new CostAdjustmentHistoryViewModel();
                    break;
                case "QuantityAdjustment":
                    view = new QuantityAdjustmentHistoryView();
                    viewModel = new QuantityAdjustmentHistoryViewModel();
                    break;

                #endregion

                #region Report Tab

                case "InventoryReport":
                    view = new InventoryReportView();
                    viewModel = new InventoryReportViewModel();
                    break;
                case "SalesReport":
                    break;
                case "PurchaseReport":
                    break;

                #endregion

                #region Configuration Tab

                case "Attachment":
                    view = new AttachmentView();
                    viewModel = new AttachmentViewModel();
                    break;
                case "SalesTax":
                    view = new SalesTaxView();
                    viewModel = new SalesTaxViewModel();
                    break;
                case "Style":
                    break;

                #endregion

                #region TimeClock

                case "Holiday":
                    view = new HolidayView();
                    viewModel = new HolidayViewModel();
                    break;
                case "WorkSchedule":
                    view = new WorkScheduleView();
                    viewModel = new WorkScheduleViewModel();
                    break;
                case "WorkPermission":
                    view = new WorkPermissionView();
                    viewModel = new WorkPermissionViewModel(host.IsOpenList);
                    break;
                case "ManualEventEditing":
                    view = new TimeClockManualEventEditingView();
                    viewModel = new TimeClockManualEventEditingViewModel();
                    break;
                case "PunctualityComparativeReport":
                    break;
                case "AnalysisReport":
                    break;
                case "EmployeeTimeClockDisplay":
                    break;
                case "Calculator":
                    _dialogService.ShowDialog<CalculatorView>(this, null, "Calculator");
                    showPopup = true;
                    break;

                #endregion

                #region Unknown Tab

                // Settings
                case "Archive":
                    break;
                case "Message":
                    break;
                case "UserPermission":
                    view = new UserListView();
                    viewModel = new UserListViewModel();
                    break;
                case "GroupPermission":
                    view = new GroupPermissionView();
                    viewModel = new GroupPermissionViewModel();
                    break;
                case "CurrentStock":
                    view = new CurrentStockView();
                    viewModel = new CurrentStockViewModel();
                    break;
                case "LayawayHistory":
                    break;

                #endregion

                #region Maintenance
                case "BackUpData":
                    _dialogService.ShowDialog<BackupDataView>(this, new BackupRestoreViewModel(), null);
                    showPopup = true;
                    break;
                #endregion
            }

            host.DataContext = viewModel;
            host.Container.grdContent.Children.Add(view);

            return !showPopup;
        }

        /// <summary>
        /// Set shortcut key for usercontrol from main view
        /// </summary>
        /// <param name="host"></param>
        public void SetKeyBinding(InputBindingCollection inputBindingCollection)
        {
            //SetKeyBinding(inputBindingCollection, App.Current.MainWindow);
        }

        /// <summary>
        /// Set shortcut key for usercontrol from main view
        /// </summary>
        /// <param name="host"></param>
        public void SetKeyBinding(InputBindingCollection inputBindingCollection, Window target)
        {
            // Get input binding collection from source window
            InputBindingCollection sourceInputBindingCollection = inputBindingCollection;

            if (sourceInputBindingCollection != null)
            {
                foreach (InputBinding sourceInputBindingItem in sourceInputBindingCollection)
                {
                    // Get key gesture of input binding
                    KeyGesture sourceKeyGesture = sourceInputBindingItem.Gesture as KeyGesture;

                    // Create key binding for main
                    KeyBinding targetKeyBinding = new KeyBinding(sourceInputBindingItem.Command, sourceKeyGesture);
                    //targetKeyBinding.CommandTarget = host;
                    targetKeyBinding.CommandParameter = sourceInputBindingItem.CommandParameter + "Main";

                    // Get key binding from main
                    InputBinding keyBinding = target.InputBindings.Cast<InputBinding>().FirstOrDefault(
                        x => ((KeyGesture)x.Gesture).Key.Equals(sourceKeyGesture.Key) &&
                            ((KeyGesture)x.Gesture).Modifiers.Equals(sourceKeyGesture.Modifiers));

                    // Check exist key binding
                    if (keyBinding != null)
                    {
                        // Remove key binding is existed from main
                        target.InputBindings.Remove(keyBinding);
                    }

                    // Add new key binding to main
                    target.InputBindings.Add(targetKeyBinding);
                }
            }
        }

        /// <summary>
        /// Expand current view when click expand button
        /// </summary>
        /// <param name="tgExpand"></param>
        private void ExpandItem(ToggleButton tgExpand)
        {
            if (IsAnimationCompletedAll)
            {
                _isPanelExpanded = tgExpand.IsChecked.Value;
                int colSubItemIndex = _grdTarget.ColumnDefinitions.IndexOf(_colSubItem);
                int colSubItemExpandedIndex = _grdTarget.ColumnDefinitions.IndexOf(_colSubItemExpanded);
                if (_isPanelExpanded)
                {
                    if (colSubItemIndex >= 0)
                        _grdTarget.ColumnDefinitions.Remove(_colSubItem);
                    if (colSubItemExpandedIndex < 0)
                        _grdTarget.ColumnDefinitions.Add(_colSubItemExpanded);
                }
                else
                {
                    if (colSubItemExpandedIndex >= 0)
                        _grdTarget.ColumnDefinitions.Remove(_colSubItemExpanded);
                    if (colSubItemIndex < 0)
                        _grdTarget.ColumnDefinitions.Add(_colSubItem);
                }
                for (int i = 1; i < _hostList.Count; i++)
                {
                    BorderLayoutHost host = _hostList.ElementAt(i);
                    host.RotateItem(_isPanelExpanded);
                }
            }
        }

        /// <summary>
        /// Call close view function when click close button
        /// </summary>
        /// <param name="btnClicked"></param>
        private void CloseItem(Button btnClicked)
        {
            BorderLayoutHost hostClicked = _hostList.SingleOrDefault(x => x.Container.btnClose.Equals(btnClicked));
            CloseItem(hostClicked);
        }

        /// <summary>
        /// Call close view function from other view
        /// </summary>
        /// <param name="viewName"></param>
        public void CloseItem(string viewName)
        {
            // Close hidden view
            OnCloseViewCommandExecute(viewName);

            // Close visible view
            BorderLayoutHost hostClicked = _hostList.FirstOrDefault(x => x.KeyName.Equals(viewName));
            CloseItem(hostClicked);
        }

        /// <summary>
        /// Process close view function
        /// </summary>
        /// <param name="hostClicked"></param>
        private void CloseItem(BorderLayoutHost hostClicked)
        {
            if (IsAnimationCompletedAll && hostClicked != null)
            {
                // Show notification when close a form
                int hostClickedPosition = _hostList.IndexOf(hostClicked);
                if (hostClickedPosition == 0 && !hostClicked.ShowNotification(true))
                    return;

                if (HiddenHostList.Count > 0)
                {
                    HiddenHostList.RemoveAt(HiddenHostList.Count - 1);
                    OnPropertyChanged(() => HiddenHostVisibility);
                }

                _hostList.Remove(hostClicked);
                _grdHost.Children.Remove(hostClicked);
                if (_hostList.Count > 0)
                {
                    BorderLayoutHost host = _hostList.ElementAt(0);
                    KeyboardNavigation.SetTabNavigation(hostClicked.View, KeyboardNavigationMode.Continue);
                    BorderLayoutTarget target = host.Target;
                    Grid.SetRow(target, 0);
                    Grid.SetRowSpan(target, _rowNumbers);
                    Grid.SetColumn(target, 0);
                    if (_hostList.Count > 1)
                    {
                        host.Container.tgExpand.IsChecked = _isPanelExpanded;
                        host.Container.tgExpand.Visibility = Visibility.Visible;
                    }
                    else // Only one view
                    {
                        Grid.SetColumnSpan(target, _columnNumbers);
                        //host.Container.tgExpand.IsChecked = false;
                        host.Container.tgExpand.Visibility = Visibility.Collapsed;
                    }
                    host.Container.btnImageIcon.Visibility = Visibility.Collapsed;
                    host.RotateItem(false);

                    // Refresh data
                    if (hostClickedPosition == 0)
                        host.IsRefreshData = true;
                    SetPositionTarget();
                    //SetKeyBinding(host.View.InputBindings);
                }

                // Show dashboard when close all view
                OnPropertyChanged(() => DashboardVisibility);
            }
        }

        /// <summary>
        /// Call change view layout function
        /// </summary>
        /// <param name="btnClicked"></param>
        private void ChangeLayoutItem(Button btnClicked)
        {
            if (IsAnimationCompletedAll)
            {
                // Get clicked host
                var hostClicked = _hostList.SingleOrDefault(
                    x => x.Container.btnImageIcon.Equals(btnClicked) || x.Container.btnTitle.Equals(btnClicked));
                ChangeLayoutItem(hostClicked);
            }
        }

        /// <summary>
        /// Process change view layout
        /// </summary>
        /// <param name="hostClicked"></param>
        private void ChangeLayoutItem(BorderLayoutHost hostClicked)
        {
            if (IsAnimationCompletedAll)
            {
                // Turn on screenshot of main host
                if (_hostList.Count > 0)
                    _hostList.ElementAt(0).AllowScreenShot = true;

                // If clicked host is not the first host, swap position that host
                if (hostClicked != null && _hostList.IndexOf(hostClicked) > 0)
                {
                    // Show notification when change position form
                    if (!_hostList.ElementAt(0).ShowNotification(false))
                        return;

                    // Set position target of clicked host
                    BorderLayoutTarget target = hostClicked.Target;
                    Grid.SetRow(target, 0);
                    Grid.SetRowSpan(target, _rowNumbers);
                    Grid.SetColumn(target, 0);
                    if (_hostList.Count == 0)
                        Grid.SetColumnSpan(target, _columnNumbers);

                    _hostList.Remove(hostClicked);
                    _grdHost.Children.Remove(hostClicked);
                    _hostList.Insert(0, hostClicked);
                    _grdHost.Children.Insert(0, hostClicked);

                    KeyboardNavigation.SetTabNavigation(hostClicked.View, KeyboardNavigationMode.Continue);
                    hostClicked.Container.tgExpand.IsChecked = _isPanelExpanded;
                    hostClicked.Container.tgExpand.Visibility = Visibility.Visible;
                    hostClicked.Container.btnImageIcon.Visibility = Visibility.Collapsed;
                    hostClicked.RotateItem(false);

                    // Refresh data
                    hostClicked.IsRefreshData = true;
                    SetPositionTarget();
                    //SetKeyBinding(hostClicked.View.InputBindings);
                }
            }
        }

        /// <summary>
        /// Set position of target from the second to last
        /// </summary>
        private void SetPositionTarget()
        {
            for (int i = 1; i < _hostList.Count; i++)
            {
                BorderLayoutHost host = _hostList.ElementAt(i);
                BorderLayoutTarget target = host.Target;
                Grid.SetRow(target, i - 1);
                Grid.SetRowSpan(target, 1);
                Grid.SetColumn(target, 1);
                Grid.SetColumnSpan(target, 1);

                // Update host is not main
                // Disable focusable of host's view
                KeyboardNavigation.SetTabNavigation(host.View, KeyboardNavigationMode.None);
                // Collapse expand button
                host.Container.tgExpand.Visibility = Visibility.Collapsed;
                // Visible image icon button
                host.Container.btnImageIcon.Visibility = Visibility.Visible;
                // If expand button is checked, rotate title bar of host
                host.RotateItem(_isPanelExpanded);
            }
        }

        /// <summary>
        /// Create default layout
        /// </summary>
        private void LoadLayout()
        {
            // Get all grid to layout
            Grid grdMainView = App.Current.MainWindow.FindName("grdMainView") as Grid;
            _grdTarget = new Grid
            {
                Name = "grdTarget"
            };
            _grdHost = new Grid
            {
                Name = "grdHost",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            grdMainView.Children.Add(_grdTarget);
            grdMainView.Children.Add(_grdHost);

            // Add column for target grid
            _grdTarget.ColumnDefinitions.Add(new ColumnDefinition());
            _colSubItem = new ColumnDefinition
            {
                Width = new GridLength(215)
            };
            _colSubItemExpanded = new ColumnDefinition
            {
                Width = new GridLength(36)
            };
            _grdTarget.ColumnDefinitions.Add(_colSubItem);

            // Add row for target grid
            for (int i = 0; i < _rowNumbers; i++)
                _grdTarget.RowDefinitions.Add(new RowDefinition());
            _grdTarget.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(0)
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands
        /// </summary>
        private void InitialCommand()
        {
            OpenViewCommand = new RelayCommand<object>(OnOpenViewCommandExecute);
            ChangeViewCommand = new RelayCommand(OnChangeViewCommandExecute);
            CloseViewCommand = new RelayCommand<object>(OnCloseViewCommandExecute);
            ClearViewCommand = new RelayCommand(OnClearViewCommandExecute);
            CloseCommand = new RelayCommand(OnCloseCommandExecute, OnCloseCommandCanExecute);
            ChangeStyleCommand = new RelayCommand<object>(OnChangeStyleCommandExecute, OnChangeStyleCommandCanExecute);
            LoginCommand = new RelayCommand(OnLoginCommandExecute, OnLoginCommandCanExecute);
            LogOutCommand = new RelayCommand(OnLogOutExecuted, CanOnLogOutExecute);
            OpenManagementUserCommand = new RelayCommand<object>(OnOpenManagermentUserCommandExecute);
            ChangePasswordCommand = new RelayCommand(OnChangePasswordCommandExecute, OnChangePasswordCommandCanExecute);
            LockScreenCommand = new RelayCommand(OnLockScreenCommandExecute, OnLockScreenCommandCanExecute);
            DataCommand = new RelayCommand<object>(OnDataCommandExecute, OnDataCommandCanExecute);
            ExitCommand = new RelayCommand(OnExitCommandExecute, OnExitCommandCanExecute);
            SwitchDatabaseCommand = new RelayCommand(OnSwitchDatabaseCommandExecute, OnSwitchDatabaseCommandCanExecute);
            ChangeLanguageCommand = new RelayCommand<object>(OnChangeLanguageCommandExecute, OnChangeLanguageCommandCanExecute);
            this.CloseDayCommand = new RelayCommand(OnCloseDayCommandExecute, OnCloseDayCommandCanExecute);
            HelpCommand = new RelayCommand(OnHelpCommandExecute, OnHelpCommandCanExecute);
        }

        /// <summary>
        /// To update data on base_UserLog table.
        /// </summary>
        private void UpdateUserLog()
        {
            try
            {
                if (Define.USER != null)
                {
                    CPC.POS.Repository.base_UserLogRepository userLogRepository = new Repository.base_UserLogRepository();
                    CPC.POS.Database.base_UserLog userLog = userLogRepository.GetIEnumerable(x => x.ResourceAccessed == Define.USER.UserResource && x.IsDisconected.HasValue && !x.IsDisconected.Value).SingleOrDefault();
                    if (userLog != null)
                    {
                        userLog.DisConnectedOn = DateTimeExt.Now;
                        userLog.IsDisconected = true;
                        userLogRepository.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Logout Fail" + ex.ToString());
            }
        }

        /// <summary>
        /// Check idle time function
        /// </summary>
        private void CheckIdleTime()
        {
            _idleTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            _idleTimer.Interval = TimeSpan.FromSeconds(1);
            _idleTimer.Tick += (sender, e) =>
            {
                if (IsIdle())
                {
                    if (App.Current.MainWindow.WindowState.Equals(WindowState.Minimized))
                    {
                        // Stop idle timer
                        _idleTimer.Stop();

                        // Turn on lock screen when window active
                        IsLockScreen = true;
                    }
                    else
                        OnLockScreenCommandExecute();
                }
            };

            // Star idle timer
            _idleTimer.Start();
        }

        /// <summary>
        /// Check system and application idle
        /// </summary>
        /// <returns></returns>
        private bool IsIdle()
        {
            // Check idle to LogOut application if TimeOutMinute is not null
            if (!Define.CONFIGURATION.TimeOutMinute.HasValue)
                return false;

            // Define time out minute value
            TimeSpan activityThreshold = TimeSpan.FromMinutes(Define.CONFIGURATION.TimeOutMinute.Value);
            //TimeSpan activityThreshold = TimeSpan.FromSeconds(10);

            // Get last input time to get system idle time
            TimeSpan machineIdle = IdleTimeHelper.GetIdleTime();

            // Get application idle time
            TimeSpan? appIdle = !IdleTimeHelper.LostFocusTime.HasValue ? null : (TimeSpan?)DateTime.Now.Subtract(IdleTimeHelper.LostFocusTime.Value);

            // Check is system idle
            bool isMachineIdle = machineIdle > activityThreshold;

            // Check is application idle
            bool isAppIdle = appIdle.HasValue && appIdle > activityThreshold;

            return isMachineIdle || isAppIdle;
        }

        /// <summary>
        /// Get connection string info
        /// </summary>
        /// <param name="connectionString">ConnectionString</param>
        /// <param name="server">Server</param>
        /// <param name="userID">UserName</param>
        /// <param name="password">Password</param>
        /// <param name="database">Database</param>
        private void GetConnectionStringInfo(string connectionString)
        {
            //connectionString = connectionString.Replace(";", "; ");
            Regex nameval = new Regex(@"(?<name>[^=]+)\s*=\s*(?<val>[^;]+?)\s*(;|$)",
                RegexOptions.Singleline);

            foreach (Match m in nameval.Matches(connectionString))
            {
                //Console.WriteLine("name=[{0}], val=[{1}]",
                //    m.Groups["name"].ToString(), m.Groups["val"].ToString());

                switch (m.Groups["name"].ToString())
                {
                    //case "Server":
                    //    Server = m.Groups["val"].ToString();
                    //    break;
                    //case "UserID":
                    //    userID = m.Groups["val"].ToString();
                    //    break;
                    //case "Password":
                    //    password = m.Groups["val"].ToString();
                    //    break;
                    case "Database":
                        Database = m.Groups["val"].ToString();
                        break;
                    //case "Port":
                    //    port = m.Groups["val"].ToString();
                    //    break;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check view is opened
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>True is opened</returns>
        public bool IsOpenedView(string viewName)
        {
            return _hostList.Count(x => x.KeyName.Equals(viewName)) > 0;
        }

        /// <summary>
        /// Check is active opened view
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public bool IsActiveView(string viewName)
        {
            if (!IsOpenedView(viewName))
                return false;
            return _hostList.FirstOrDefault().KeyName.Equals(viewName);
        }

        /// <summary>
        /// Load status information
        /// </summary>
        public void LoadStatusInformation()
        {
            LoadStoreName();
            LoadTaxLocationAndCode();

            // Get login name
            LoginName = Define.USER.LoginName;

            // Get database name
            GetConnectionStringInfo(ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString);
        }

        /// <summary>
        /// Load store name
        /// </summary>
        public void LoadStoreName()
        {
            // Get store name

            IOrderedEnumerable<base_Store> store = _storeRepository.GetAll().OrderBy(x => x.Id);
            if (store != null && store.Count() > 0)
                StoreName = store.ElementAt(Define.StoreCode).Name;
        }

        /// <summary>
        /// Load tax location and tax code
        /// </summary>
        public void LoadTaxLocationAndCode()
        {
            // Get tax location
            base_SaleTaxLocationModel taxLocationModel = null;
            if (Define.CONFIGURATION.DefaultSaleTaxLocation.HasValue && Define.CONFIGURATION.DefaultSaleTaxLocation.Value > 0)
            {
                base_SaleTaxLocation saleTaxLocation = _saleTaxLocationRepository.Get(x => x.Id.Equals(Define.CONFIGURATION.DefaultSaleTaxLocation.Value));
                if (saleTaxLocation != null)
                {
                    taxLocationModel = new base_SaleTaxLocationModel(saleTaxLocation);
                    // Get tax code
                    TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
                }
            }

            if (taxLocationModel != null)
                TaxLocation = taxLocationModel.Name;
            else
            {
                taxLocationModel = _saleTaxLocationRepository.CreateDefaulSaleTaxLocation();
                //Set Default taxCode & TaxLocation
                TaxLocation = taxLocationModel.Name;
                TaxCode = taxLocationModel.TaxCodeModel.TaxCode;
            }


        }

        #endregion

        #region Event Methods
        /// <summary>
        /// Call Help file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            OpenHelpFile();
        }


        /// <summary>
        /// Set keybinding after form loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void host_Loaded(object sender, RoutedEventArgs e)
        {
            BorderLayoutHost host = sender as BorderLayoutHost;
            //SetKeyBinding(host.View.InputBindings);
        }

        /// <summary>
        /// Shrink current view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tgExpand_Unchecked(object sender, RoutedEventArgs e)
        {
            ExpandItem(sender as ToggleButton);
        }

        /// <summary>
        /// Expand current view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tgExpand_Checked(object sender, RoutedEventArgs e)
        {
            ExpandItem(sender as ToggleButton);
        }

        /// <summary>
        /// Call close view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseItem(sender as Button);
            // Clear current view name
            curViewName = string.Empty;
        }

        /// <summary>
        /// Call change view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTitle_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutItem(sender as Button);
        }

        /// <summary>
        /// Call change view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImageIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutItem(sender as Button);
        }

        /// <summary>
        /// Call expand current view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsAnimationCompletedAll)
                _hostList.ElementAt(0).Container.tgExpand.IsChecked = true;
        }

        #endregion

        #region IDataErrorInfo Members

        protected HashSet<string> _extensionErrors = new HashSet<string>();
        /// <summary>
        /// <para> Gets or sets the ExtensionErrors </para>
        /// </summary>
        public HashSet<string> ExtensionErrors
        {
            get
            {
                return _extensionErrors;
            }
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
                this.ExtensionErrors.Clear();

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
                    this.ExtensionErrors.Add(columnName);

                return message;
            }
        }

        #endregion

        #region Permission

        #region Properties

        #region Application Menu

        private bool _allowAccessCashIn;
        /// <summary>
        /// Gets or sets the AllowAccessCashIn.
        /// </summary>
        public bool AllowAccessCashIn
        {
            get
            {
                return _allowAccessCashIn;
            }
            set
            {
                if (_allowAccessCashIn != value)
                {
                    _allowAccessCashIn = value;
                    OnPropertyChanged(() => AllowAccessCashIn);
                }
            }
        }

        private bool _allowAccessCashOut;
        /// <summary>
        /// Gets or sets the AllowAccessCashOut.
        /// </summary>
        public bool AllowAccessCashOut
        {
            get
            {
                return _allowAccessCashOut;
            }
            set
            {
                if (_allowAccessCashOut != value)
                {
                    _allowAccessCashOut = value;
                    OnPropertyChanged(() => AllowAccessCashOut);
                }
            }
        }

        #endregion

        #region Sale Module

        private bool _allowAccessSaleModule = true;
        /// <summary>
        /// Gets or sets the AllowAccessSaleModule.
        /// </summary>
        public bool AllowAccessSaleModule
        {
            get
            {
                return _allowAccessSaleModule;
            }
            set
            {
                if (_allowAccessSaleModule != value)
                {
                    _allowAccessSaleModule = value;
                    OnPropertyChanged(() => AllowAccessSaleModule);
                }
            }
        }

        private bool _allowAccessCustomer = true;
        /// <summary>
        /// Gets or sets the AllowAccessCustomer.
        /// </summary>
        public bool AllowAccessCustomer
        {
            get
            {
                return _allowAccessCustomer;
            }
            set
            {
                if (_allowAccessCustomer != value)
                {
                    _allowAccessCustomer = value;
                    OnPropertyChanged(() => AllowAccessCustomer);
                }
            }
        }

        private bool _allowAddCustomer = true;
        /// <summary>
        /// Gets or sets the AllowAddCustomer.
        /// </summary>
        public bool AllowAddCustomer
        {
            get
            {
                return _allowAddCustomer;
            }
            set
            {
                if (_allowAddCustomer != value)
                {
                    _allowAddCustomer = value;
                    OnPropertyChanged(() => AllowAddCustomer);
                }
            }
        }

        private bool _allowAccessReward = true;
        /// <summary>
        /// Gets or sets the AllowAddReward.
        /// </summary>
        public bool AllowAccessReward
        {
            get
            {
                return _allowAccessReward;
            }
            set
            {
                if (_allowAccessReward != value)
                {
                    _allowAccessReward = value;
                    OnPropertyChanged(() => AllowAccessReward);
                }
            }
        }

        private bool _allowAccessSaleQuotation = true;
        /// <summary>
        /// Gets or sets the AllowAccessSaleQuotation.
        /// </summary>
        public bool AllowAccessSaleQuotation
        {
            get
            {
                return _allowAccessSaleQuotation;
            }
            set
            {
                if (_allowAccessSaleQuotation != value)
                {
                    _allowAccessSaleQuotation = value;
                    OnPropertyChanged(() => AllowAccessSaleQuotation);
                }
            }
        }

        private bool _allowAddSaleQuotation = true;
        /// <summary>
        /// Gets or sets the AllowAddSaleQuotation.
        /// </summary>
        public bool AllowAddSaleQuotation
        {
            get
            {
                return _allowAddSaleQuotation;
            }
            set
            {
                if (_allowAddSaleQuotation != value)
                {
                    _allowAddSaleQuotation = value;
                    OnPropertyChanged(() => AllowAddSaleQuotation);
                }
            }
        }

        private bool _allowAccessLayaway = true;
        /// <summary>
        /// Gets or sets the AllowAccessLayaway.
        /// </summary>
        public bool AllowAccessLayaway
        {
            get
            {
                return _allowAccessLayaway;
            }
            set
            {
                if (_allowAccessLayaway != value)
                {
                    _allowAccessLayaway = value;
                    OnPropertyChanged(() => AllowAccessLayaway);
                }
            }
        }

        private bool _allowAddLayaway = true;
        /// <summary>
        /// Gets or sets the AllowAddLayaway.
        /// </summary>
        public bool AllowAddLayaway
        {
            get
            {
                return _allowAddLayaway;
            }
            set
            {
                if (_allowAddLayaway != value)
                {
                    _allowAddLayaway = value;
                    OnPropertyChanged(() => AllowAddLayaway);
                }
            }
        }

        private bool _allowAccessWorkOrder = true;
        /// <summary>
        /// Gets or sets the AllowAccessWorkOrder.
        /// </summary>
        public bool AllowAccessWorkOrder
        {
            get
            {
                return _allowAccessWorkOrder;
            }
            set
            {
                if (_allowAccessWorkOrder != value)
                {
                    _allowAccessWorkOrder = value;
                    OnPropertyChanged(() => AllowAccessWorkOrder);
                }
            }
        }

        private bool _allowAddWorkOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddWorkOrder.
        /// </summary>
        public bool AllowAddWorkOrder
        {
            get
            {
                return _allowAddWorkOrder;
            }
            set
            {
                if (_allowAddWorkOrder != value)
                {
                    _allowAddWorkOrder = value;
                    OnPropertyChanged(() => AllowAddWorkOrder);
                }
            }
        }

        private bool _allowAccessSaleOrder = true;
        /// <summary>
        /// Gets or sets the AllowAccessSaleOrder.
        /// </summary>
        public bool AllowAccessSaleOrder
        {
            get
            {
                return _allowAccessSaleOrder;
            }
            set
            {
                if (_allowAccessSaleOrder != value)
                {
                    _allowAccessSaleOrder = value;
                    OnPropertyChanged(() => AllowAccessSaleOrder);
                }
            }
        }

        private bool _allowAddSaleOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddSaleOrder.
        /// </summary>
        public bool AllowAddSaleOrder
        {
            get
            {
                return _allowAddSaleOrder;
            }
            set
            {
                if (_allowAddSaleOrder != value)
                {
                    _allowAddSaleOrder = value;
                    OnPropertyChanged(() => AllowAddSaleOrder);
                }
            }
        }

        #endregion

        #region Purchase Module

        private bool _allowAccessPurchaseModule = true;
        /// <summary>
        /// Gets or sets the AllowAccessPurchaseModule.
        /// </summary>
        public bool AllowAccessPurchaseModule
        {
            get
            {
                return _allowAccessPurchaseModule;
            }
            set
            {
                if (_allowAccessPurchaseModule != value)
                {
                    _allowAccessPurchaseModule = value;
                    OnPropertyChanged(() => AllowAccessPurchaseModule);
                }
            }
        }

        private bool _allowAccessVendor = true;
        /// <summary>
        /// Gets or sets the AllowAccessVendor.
        /// </summary>
        public bool AllowAccessVendor
        {
            get
            {
                return _allowAccessVendor;
            }
            set
            {
                if (_allowAccessVendor != value)
                {
                    _allowAccessVendor = value;
                    OnPropertyChanged(() => AllowAccessVendor);
                }
            }
        }

        private bool _allowAddVendor = true;
        /// <summary>
        /// Gets or sets the AllowAddVendor.
        /// </summary>
        public bool AllowAddVendor
        {
            get
            {
                return _allowAddVendor;
            }
            set
            {
                if (_allowAddVendor != value)
                {
                    _allowAddVendor = value;
                    OnPropertyChanged(() => AllowAddVendor);
                }
            }
        }

        private bool _allowAccessPurchaseOrder = true;
        /// <summary>
        /// Gets or sets the allowAccessPurchaseOrder.
        /// </summary>
        public bool AllowAccessPurchaseOrder
        {
            get
            {
                return _allowAccessPurchaseOrder;
            }
            set
            {
                if (_allowAccessPurchaseOrder != value)
                {
                    _allowAccessPurchaseOrder = value;
                    OnPropertyChanged(() => AllowAccessPurchaseOrder);
                }
            }
        }

        private bool _allowAddPurchaseOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddPO.
        /// </summary>
        public bool AllowAddPurchaseOrder
        {
            get
            {
                return _allowAddPurchaseOrder;
            }
            set
            {
                if (_allowAddPurchaseOrder != value)
                {
                    _allowAddPurchaseOrder = value;
                    OnPropertyChanged(() => AllowAddPurchaseOrder);
                }
            }
        }

        #endregion

        #region Inventory Module

        private bool _allowAccessInventoryModule = true;
        /// <summary>
        /// Gets or sets the AllowAccessProductModule.
        /// </summary>
        public bool AllowAccessInventoryModule
        {
            get
            {
                return _allowAccessInventoryModule;
            }
            set
            {
                if (_allowAccessInventoryModule != value)
                {
                    _allowAccessInventoryModule = value;
                    OnPropertyChanged(() => AllowAccessInventoryModule);
                }
            }
        }

        private bool _allowAccessProduct = true;
        /// <summary>
        /// Gets or sets the AllowAccessProduct.
        /// </summary>
        public bool AllowAccessProduct
        {
            get
            {
                return _allowAccessProduct;
            }
            set
            {
                if (_allowAccessProduct != value)
                {
                    _allowAccessProduct = value;
                    OnPropertyChanged(() => AllowAccessProduct);
                }
            }
        }

        private bool _allowAddProduct = true;
        /// <summary>
        /// Gets or sets the AllowAddProduct.
        /// </summary>
        public bool AllowAddProduct
        {
            get
            {
                return _allowAddProduct;
            }
            set
            {
                if (_allowAddProduct != value)
                {
                    _allowAddProduct = value;
                    OnPropertyChanged(() => AllowAddProduct);
                }
            }
        }

        private bool _allowAddDepartment = true;
        /// <summary>
        /// Gets or sets the AllowAddDepartment.
        /// </summary>
        public bool AllowAddDepartment
        {
            get
            {
                return _allowAddDepartment;
            }
            set
            {
                if (_allowAddDepartment != value)
                {
                    _allowAddDepartment = value;
                    OnPropertyChanged(() => AllowAddDepartment);
                }
            }
        }

        private bool _allowAccessPricing = true;
        /// <summary>
        /// Gets or sets the AllowAccessPricing.
        /// </summary>
        public bool AllowAccessPricing
        {
            get
            {
                return _allowAccessPricing;
            }
            set
            {
                if (_allowAccessPricing != value)
                {
                    _allowAccessPricing = value;
                    OnPropertyChanged(() => AllowAccessPricing);
                }
            }
        }

        private bool _allowAddPricing = true;
        /// <summary>
        /// Gets or sets the AllowAddPricing.
        /// </summary>
        public bool AllowAddPricing
        {
            get
            {
                return _allowAddPricing;
            }
            set
            {
                if (_allowAddPricing != value)
                {
                    _allowAddPricing = value;
                    OnPropertyChanged(() => AllowAddPricing);
                }
            }
        }

        private bool _allowAccessDiscountProgram = true;
        /// <summary>
        /// Gets or sets the AllowAccessDiscountProgram.
        /// </summary>
        public bool AllowAccessDiscountProgram
        {
            get
            {
                return _allowAccessDiscountProgram;
            }
            set
            {
                if (_allowAccessDiscountProgram != value)
                {
                    _allowAccessDiscountProgram = value;
                    OnPropertyChanged(() => AllowAccessDiscountProgram);
                }
            }
        }

        private bool _allowAddPromotion = true;
        /// <summary>
        /// Gets or sets the AllowAddPromotion.
        /// </summary>
        public bool AllowAddPromotion
        {
            get
            {
                return _allowAddPromotion;
            }
            set
            {
                if (_allowAddPromotion != value)
                {
                    _allowAddPromotion = value;
                    OnPropertyChanged(() => AllowAddPromotion);
                }
            }
        }

        private bool _allowAccessStock = true;
        /// <summary>
        /// Gets or sets the AllowAccessCurrentStock.
        /// </summary>
        public bool AllowAccessStock
        {
            get
            {
                return _allowAccessStock;
            }
            set
            {
                if (_allowAccessStock != value)
                {
                    _allowAccessStock = value;
                    OnPropertyChanged(() => AllowAccessStock);
                }
            }
        }

        private bool _allowViewCurrentStock = true;
        /// <summary>
        /// Gets or sets the AllowViewCurrentStock.
        /// </summary>
        public bool AllowViewCurrentStock
        {
            get
            {
                return _allowViewCurrentStock;
            }
            set
            {
                if (_allowViewCurrentStock != value)
                {
                    _allowViewCurrentStock = value;
                    OnPropertyChanged(() => AllowViewCurrentStock);
                }
            }
        }

        private bool _allowAddCountSheet = true;
        /// <summary>
        /// Gets or sets the AllowAddCountSheet.
        /// </summary>
        public bool AllowAddCountSheet
        {
            get
            {
                return _allowAddCountSheet;
            }
            set
            {
                if (_allowAddCountSheet != value)
                {
                    _allowAddCountSheet = value;
                    OnPropertyChanged(() => AllowAddCountSheet);
                }
            }
        }

        private bool _allowAddTransferStock = true;
        /// <summary>
        /// Gets or sets the AllowAddTransferStock.
        /// </summary>
        public bool AllowAddTransferStock
        {
            get
            {
                return _allowAddTransferStock;
            }
            set
            {
                if (_allowAddTransferStock != value)
                {
                    _allowAddTransferStock = value;
                    OnPropertyChanged(() => AllowAddTransferStock);
                }
            }
        }

        private bool _allowAccessAdjustHistory = true;
        /// <summary>
        /// Gets or sets the AllowAccessAdjustHistory.
        /// </summary>
        public bool AllowAccessAdjustHistory
        {
            get
            {
                return _allowAccessAdjustHistory;
            }
            set
            {
                if (_allowAccessAdjustHistory != value)
                {
                    _allowAccessAdjustHistory = value;
                    OnPropertyChanged(() => AllowAccessAdjustHistory);
                }
            }
        }

        private bool _allowAccessCostAdjustment = true;
        /// <summary>
        /// Gets or sets the AllowAccessCostAdjustment.
        /// </summary>
        public bool AllowAccessCostAdjustment
        {
            get
            {
                return _allowAccessCostAdjustment;
            }
            set
            {
                if (_allowAccessCostAdjustment != value)
                {
                    _allowAccessCostAdjustment = value;
                    OnPropertyChanged(() => AllowAccessCostAdjustment);
                }
            }
        }

        private bool _allowAccessQuantityAdjustment = true;
        /// <summary>
        /// Gets or sets the AllowAccessQuantityAdjustment.
        /// </summary>
        public bool AllowAccessQuantityAdjustment
        {
            get
            {
                return _allowAccessQuantityAdjustment;
            }
            set
            {
                if (_allowAccessQuantityAdjustment != value)
                {
                    _allowAccessQuantityAdjustment = value;
                    OnPropertyChanged(() => AllowAccessQuantityAdjustment);
                }
            }
        }

        #endregion

        #region Configuration Module

        private bool _allowChangeConfiguration = true;
        /// <summary>
        /// Gets or sets the AllowChangeConfiguration.
        /// </summary>
        public bool AllowChangeConfiguration
        {
            get
            {
                return _allowChangeConfiguration;
            }
            set
            {
                if (_allowChangeConfiguration != value)
                {
                    _allowChangeConfiguration = value;
                    OnPropertyChanged(() => AllowChangeConfiguration);
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Get permission for CashIn or CashOut
        /// </summary>
        private void GetCashInOutPermission()
        {
            base_CashFlowRepository cashFlowRepository = new base_CashFlowRepository();
            DateTime current = DateTime.Now.Date;

            // Get cash flow by user and shift
            base_CashFlow cashFlow = null;
            if (Define.ShiftCode == null)
                cashFlow = cashFlowRepository.Get(x => x.CashierResource == Define.USER.UserResource && x.OpenDate == current);
            else
                cashFlow = cashFlowRepository.Get(x => x.CashierResource == Define.USER.UserResource && x.OpenDate == current && x.Shift.Equals(Define.ShiftCode));

            AllowAccessCashIn = false;
            AllowAccessCashOut = false;

            if (cashFlow == null)
            {
                AllowAccessCashIn = !IsAdminPermission;
            }
            else if (!cashFlow.IsCashOut)
            {
                AllowAccessCashOut = !IsAdminPermission;
            }
        }

        /// <summary>
        /// Get permissions
        /// </summary>
        public override void GetPermission()
        {
            GetCashInOutPermission();

            if (!IsAdminPermission)
            {
                if (IsFullPermission)
                {
                    // Set default permission
                    AllowAccessReward = IsMainStore;

                    AllowAddPurchaseOrder = IsMainStore;

                    AllowAddProduct = IsMainStore;
                    AllowAddPricing = IsMainStore;
                    AllowAddPromotion = IsMainStore;
                    AllowAccessCostAdjustment = IsMainStore;
                    AllowAccessQuantityAdjustment = IsMainStore;

                    AllowChangeConfiguration = IsMainStore;
                }
                else
                {
                    // Get all user rights
                    IEnumerable<string> userRightCodes = Define.USER_AUTHORIZATION.Select(x => x.Code);

                    #region Sale Module

                    // Get access sale module permission
                    AllowAccessSaleModule = userRightCodes.Contains("SO100");

                    // Get access customer permission
                    AllowAccessCustomer = userRightCodes.Contains("SO100-01") && AllowAccessSaleModule;

                    // Get add/copy customer permission
                    AllowAddCustomer = userRightCodes.Contains("SO100-01-01") && AllowAccessCustomer;

                    // Get access reward permission
                    AllowAccessReward = userRightCodes.Contains("SO100-02") && AllowAccessSaleModule && IsMainStore;

                    // Get access sale quotation permission
                    AllowAccessSaleQuotation = userRightCodes.Contains("SO100-03") && AllowAccessSaleModule;

                    // Get add/copy sale quotation permission
                    AllowAddSaleQuotation = userRightCodes.Contains("SO100-03-01") && AllowAccessSaleQuotation;

                    // Get access layaway permission
                    AllowAccessLayaway = userRightCodes.Contains("SO100-05") && AllowAccessSaleModule;

                    // Get add/copy layaway permission
                    AllowAddLayaway = userRightCodes.Contains("SO100-05-02") && AllowAccessLayaway;

                    // Get access work order permission
                    AllowAccessWorkOrder = userRightCodes.Contains("SO100-06") && AllowAccessSaleModule;

                    // Get add/copy work order permission
                    AllowAddWorkOrder = userRightCodes.Contains("SO100-06-02") && AllowAccessWorkOrder;

                    // Get access sale order permission
                    AllowAccessSaleOrder = userRightCodes.Contains("SO100-04") && AllowAccessSaleModule;

                    // Get add/copy sale order permission
                    AllowAddSaleOrder = userRightCodes.Contains("SO100-04-02") && AllowAccessSaleOrder;

                    #endregion

                    #region Purchase Module

                    // Get access purchase module permission
                    AllowAccessPurchaseModule = userRightCodes.Contains("PO100");

                    // Get access vendor permission
                    AllowAccessVendor = userRightCodes.Contains("PO100-01") && AllowAccessPurchaseModule;

                    // Get add/copy vendor permission
                    AllowAddVendor = userRightCodes.Contains("PO100-01-01") && AllowAccessVendor;

                    // Get access purchase order permission
                    AllowAccessPurchaseOrder = userRightCodes.Contains("PO100-02") && AllowAccessPurchaseModule;

                    // Get add purchase order permission
                    AllowAddPurchaseOrder = userRightCodes.Contains("PO100-02-02") && AllowAccessPurchaseOrder && IsMainStore;

                    #endregion

                    #region Inventory Module

                    // Get access inventory module permission
                    AllowAccessInventoryModule = userRightCodes.Contains("IV100");

                    // Get access product permission
                    AllowAccessProduct = userRightCodes.Contains("IV100-01") && AllowAccessInventoryModule;

                    // Get add/copy product permission
                    AllowAddProduct = userRightCodes.Contains("IV100-01-01") && AllowAccessProduct && IsMainStore;

                    // Get add/copy department permission
                    AllowAddDepartment = userRightCodes.Contains("IV100-01-03") && AllowAccessProduct;

                    // Get access pricing permission
                    AllowAccessPricing = userRightCodes.Contains("IV100-02") && AllowAccessInventoryModule;

                    // Get add/copy pricing permission
                    AllowAddPricing = userRightCodes.Contains("IV100-02-01") && AllowAccessPricing && IsMainStore;

                    // Get access discount program permission
                    AllowAccessDiscountProgram = userRightCodes.Contains("IV100-03") && AllowAccessInventoryModule;

                    // Get add/copy promotion permission
                    AllowAddPromotion = userRightCodes.Contains("IV100-03-01") && AllowAccessDiscountProgram && IsMainStore;

                    // Get access stock permission
                    AllowAccessStock = userRightCodes.Contains("IV100-04") && AllowAccessInventoryModule;

                    // Get view current stock permission
                    AllowViewCurrentStock = userRightCodes.Contains("IV100-04-01") && AllowAccessStock;

                    // Get add count sheet permission
                    AllowAddCountSheet = userRightCodes.Contains("IV100-04-02") && AllowAccessStock;

                    // Get add transfer stock permission
                    AllowAddTransferStock = userRightCodes.Contains("IV100-04-05") && AllowAccessStock;

                    // Get access adjust history permission
                    AllowAccessAdjustHistory = userRightCodes.Contains("IV100-05") && AllowAccessInventoryModule;

                    // Get access cost adjustment permission
                    AllowAccessCostAdjustment = userRightCodes.Contains("IV100-05-01") && AllowAccessAdjustHistory && IsMainStore;

                    // Get access quantity adjustment permission
                    AllowAccessQuantityAdjustment = userRightCodes.Contains("IV100-05-02") && AllowAccessAdjustHistory && IsMainStore;

                    #endregion

                    #region Configuration Module

                    // Get change configuration permission
                    AllowChangeConfiguration = userRightCodes.Contains("CF100-05") && IsMainStore;

                    #endregion
                }
            }

            //// Marge allow add layaway permission
            //AllowAddLayaway &= _layawayManagerRepository.GetIQueryable(x => x.Status.Equals((short)StatusBasic.Active)).Count() > 0;
        }

        #endregion
    }

    /// <summary>
    /// Reminder.
    /// </summary>
    partial class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Fields

        /// <summary>
        /// Determine whether show reminder of today.
        /// </summary>
        private bool _isShowReminderToday = true;

        /// <summary>
        /// Represent 'Today' text.
        /// </summary>
        private const string _todayText = "Today";
        /// <summary>
        /// Represent 'All Tasks' text.
        /// </summary>
        private const string _allTasksText = "All Tasks";

        /// <summary>
        /// A dispatcherTimer auto run alarm.
        /// </summary>
        private DispatcherTimer _timerReminder;

        /// <summary>
        /// A dispatcherTimer auto run alarm.
        /// </summary>
        private DispatcherTimer _timerRewardService;

        private CollectionBase<base_CustomerReminderModel> _customerReminderList;

        private base_RewardManagerModel _rewardManagerModel;

        #endregion

        #region Properties

        #region IsOpenSearch

        private bool _isOpenSearch;
        /// <summary>
        /// Determine whether search component is show.
        /// </summary>
        public bool IsOpenSearch
        {
            get
            {
                return _isOpenSearch;
            }
            set
            {
                if (_isOpenSearch != value)
                {
                    _isOpenSearch = value;
                    OnPropertyChanged(() => IsOpenSearch);
                }
            }
        }

        #endregion

        #region Keyword

        private string _keyword;
        /// <summary>
        /// Key search.
        /// </summary>
        public string Keyword
        {
            get
            {
                return _keyword;
            }
            set
            {
                if (_keyword != value)
                {
                    _keyword = value;
                    OnPropertyChanged(() => Keyword);
                    Filter(_keyword);
                }
            }
        }

        #endregion

        #region ReminderList

        private CollectionBase<base_ReminderModel> _reminderList;
        /// <summary>
        /// Reminder list.
        /// </summary>
        public CollectionBase<base_ReminderModel> ReminderList
        {
            get
            {
                return _reminderList;
            }
            set
            {
                if (_reminderList != value)
                {
                    _reminderList = value;
                    OnPropertyChanged(() => ReminderList);
                }
            }
        }

        #endregion

        #region SelectedReminder

        private base_ReminderModel _selectedReminder;
        /// <summary>
        /// Holds selected reminder.
        /// </summary>
        public base_ReminderModel SelectedReminder
        {
            get
            {
                return _selectedReminder;
            }
            set
            {
                if (_selectedReminder != value)
                {
                    _selectedReminder = value;
                    OnPropertyChanged(() => SelectedReminder);
                }
            }
        }

        #endregion

        #region LoadStatus

        private string _loadStatus = _todayText;
        /// <summary>
        /// Holds 'Today' text or 'All Tasks' text.
        /// </summary>
        public string LoadStatus
        {
            get
            {
                return _loadStatus;
            }
            set
            {
                if (_loadStatus != value)
                {
                    _loadStatus = value;
                    OnPropertyChanged(() => LoadStatus);
                }
            }
        }

        #endregion

        #region CountTodayTasks

        private int _countTodayTasks;
        /// <summary>
        /// Total tasks of today.
        /// </summary>
        public int CountTodayTasks
        {
            get
            {
                return _countTodayTasks;
            }
            set
            {
                if (_countTodayTasks != value)
                {
                    _countTodayTasks = value;
                    OnPropertyChanged(() => CountTodayTasks);
                }
            }
        }

        #endregion

        #region CountAllTasks

        private int _countAllTasks;
        /// <summary>
        /// Total tasks.
        /// </summary>
        public int CountAllTasks
        {
            get
            {
                return _countAllTasks;
            }
            set
            {
                if (_countAllTasks != value)
                {
                    _countAllTasks = value;
                    OnPropertyChanged(() => CountAllTasks);
                }
            }
        }

        #endregion

        #region CountAlarmTasks

        private int _countAlarmTasks;
        /// <summary>
        /// Total tasks was alarm.
        /// </summary>
        public int CountAlarmTasks
        {
            get
            {
                return _countAlarmTasks;
            }
            set
            {
                if (_countAlarmTasks != value)
                {
                    _countAlarmTasks = value;
                    OnPropertyChanged(() => CountAlarmTasks);
                }
            }
        }

        #endregion

        #region CountAlarmBithday

        private int _countAlarmBithday;
        /// <summary>
        /// Total customers was birthday alarm.
        /// </summary>
        public int CountAlarmBithday
        {
            get
            {
                return _countAlarmBithday;
            }
            set
            {
                if (_countAlarmBithday != value)
                {
                    _countAlarmBithday = value;
                    OnPropertyChanged(() => CountAlarmBithday);
                }
            }
        }

        #endregion

        #region RewardCustomerList

        private CollectionBase<base_GuestModel> _rewardCustomerList;
        public CollectionBase<base_GuestModel> RewardCustomerList
        {
            get
            {
                return _rewardCustomerList;
            }
            set
            {
                if (_rewardCustomerList != value)
                {
                    _rewardCustomerList = value;
                    OnPropertyChanged(() => RewardCustomerList);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region ChangeModeCommand

        private ICommand _changeModeCommand;
        /// <summary>
        /// Open or close seach component.
        /// </summary>
        public ICommand ChangeModeCommand
        {
            get
            {
                if (_changeModeCommand == null)
                {
                    _changeModeCommand = new RelayCommand(ChangeModeExecute);
                }
                return _changeModeCommand;
            }
        }

        #endregion

        #region NewCommand

        private ICommand _newCommand;
        /// <summary>
        /// New Task.
        /// </summary>
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(NewExecute, CanNewExecute);
                }
                return _newCommand;
            }
        }

        #endregion

        #region EditCommand

        private ICommand _editCommand;
        /// <summary>
        /// Edit task.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(EditExecute, CanEditExecute);
                }
                return _editCommand;
            }
        }

        #endregion

        #region DeleteCommand

        private ICommand _deleteCommand;
        /// <summary>
        /// Delete task.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(DeleteExecute, CanDeleteExecute);
                }
                return _deleteCommand;
            }
        }

        #endregion

        #region ChangeLoadStatusCommand

        private ICommand _changeLoadStatusCommand;
        /// <summary>
        /// Change 'Today' tasks or 'All Tasks'.
        /// </summary>
        public ICommand ChangeLoadStatusCommand
        {
            get
            {
                if (_changeLoadStatusCommand == null)
                {
                    _changeLoadStatusCommand = new RelayCommand<string>(ChangeLoadStatusExecute);
                }
                return _changeLoadStatusCommand;
            }
        }

        #endregion

        #region ShowTaskInfoCommand

        private ICommand _showTaskInfoCommand;
        /// <summary>
        /// Show task information.
        /// </summary>
        public ICommand ShowTaskInfoCommand
        {
            get
            {
                if (_showTaskInfoCommand == null)
                {
                    _showTaskInfoCommand = new RelayCommand(ShowTaskInfoExecute, CanShowTaskInfoExecute);
                }
                return _showTaskInfoCommand;
            }
        }

        #endregion

        #region OpenAlarmListCommand

        private ICommand _openAlarmListCommand;
        public ICommand OpenAlarmListCommand
        {
            get
            {
                if (_openAlarmListCommand == null)
                {
                    _openAlarmListCommand = new RelayCommand(OpenAlarmListExecute);
                }
                return _openAlarmListCommand;
            }
        }

        #endregion

        #region OpenAlarmBirthdayCommand

        private ICommand _openAlarmBirthdayCommand;
        public ICommand OpenAlarmBirthdayCommand
        {
            get
            {
                if (_openAlarmBirthdayCommand == null)
                {
                    _openAlarmBirthdayCommand = new RelayCommand(OpenAlarmBirthdayExecute);
                }
                return _openAlarmBirthdayCommand;
            }
        }

        #endregion

        #region OpenRewardCutOffServiceCommand

        private ICommand _openRewardCutOffServiceCommand;
        public ICommand OpenRewardCutOffServiceCommand
        {
            get
            {
                if (_openRewardCutOffServiceCommand == null)
                {
                    _openRewardCutOffServiceCommand = new RelayCommand(OpenRewardCutOffServiceExecute, CanOpenRewardCutOffServiceExecute);
                }
                return _openRewardCutOffServiceCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region ChangeModeExecute

        /// <summary>
        /// Open or close seach component.
        /// </summary>
        private void ChangeModeExecute()
        {
            ChangeMode();
        }

        #endregion

        #region NewExecute

        /// <summary>
        /// New task.
        /// </summary>
        private void NewExecute()
        {
            NewTask();
        }

        #endregion

        #region CanNewExecute

        /// <summary>
        /// Check whether NewExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanNewExecute()
        {
            return true;
        }

        #endregion

        #region EditExecute

        /// <summary>
        /// Edit task.
        /// </summary>
        private void EditExecute()
        {
            EditTask();
        }

        #endregion

        #region CanEditExecute

        /// <summary>
        /// Check whether EditExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanEditExecute()
        {
            if (_selectedReminder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeleteExecute

        /// <summary>
        /// Delete task.
        /// </summary>
        private void DeleteExecute()
        {
            Delete();
        }

        #endregion

        #region CanDeleteExecute

        /// <summary>
        /// Check whether DeleteExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanDeleteExecute()
        {
            if (_reminderList != null && _reminderList.Any(x => x.IsCompleted))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region ChangeLoadStatusExecute

        /// <summary>
        /// Change 'Today' tasks or 'All Tasks'.
        /// </summary>
        private void ChangeLoadStatusExecute(string parameter)
        {
            ChangeLoadStatus(parameter);
        }

        #endregion

        #region ShowTaskInfoExecute

        /// <summary>
        /// Show task information.
        /// </summary>
        private void ShowTaskInfoExecute()
        {
            ShowTaskInfo();
        }

        #endregion

        #region CanShowTaskInfoExecute

        /// <summary>
        /// Check whether ShowTaskInfoExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanShowTaskInfoExecute()
        {
            if (_selectedReminder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region OpenAlarmListExecute

        private void OpenAlarmListExecute()
        {
            OpenAlarmList();
        }

        #endregion

        #region OpenAlarmBirthdayExecute

        private void OpenAlarmBirthdayExecute()
        {
            OpenAlarmBirthday();
        }

        #endregion

        #region CanOpenRewardCutOffServiceExecute

        /// <summary>
        /// Determine whether can call OpenRewardCutOffServiceExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanOpenRewardCutOffServiceExecute()
        {
            if (_rewardManagerModel == null || _rewardCustomerList == null || _rewardCustomerList.Count <= 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region OpenRewardCutOffServiceExecute

        private void OpenRewardCutOffServiceExecute()
        {
            OpenRewardCutOffService();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            InitData();
            InitTimer();
        }

        #region InitData

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void InitData()
        {
            GetReminderList();
            GetBirthdayList();
            RunRewardCutOffService();
            Filter();
            CountTask();
        }

        #endregion

        #region InitTimer

        /// <summary>
        /// Initialize timer.
        /// </summary>
        private void InitTimer()
        {
            _timerReminder = new DispatcherTimer(DispatcherPriority.Background);
            _timerReminder.Interval = new TimeSpan(0, 1, 0);
            _timerReminder.Tick += new EventHandler(TimerReminderTick);
            _timerReminder.Start();
        }

        #endregion

        #region GetReminderList

        private void GetReminderList()
        {
            try
            {
                base_ReminderRepository reminderRepository = new base_ReminderRepository();
                string userResource = Define.USER.UserResource;
                string loginName = Define.USER.LoginName;
                ReminderList = new CollectionBase<base_ReminderModel>(reminderRepository.GetAll(x =>
                    !x.IsCompleted && (x.UserCreated == loginName || x.ResourceAssigned == userResource)).Select(x => new base_ReminderModel(x)));
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region GetBirthdayList

        private void GetBirthdayList()
        {
            try
            {
                DBHelper dbHelper = new DBHelper();
                NpgsqlCommand command = new NpgsqlCommand("sp_get_reminder");
                command.CommandType = CommandType.StoredProcedure;
                dbHelper.ExecuteNonQuery(command);

                base_CustomerReminderRepository customerReminderRepository = new base_CustomerReminderRepository();
                _customerReminderList = new CollectionBase<base_CustomerReminderModel>(customerReminderRepository.GetAll(x => !x.IsSend).Select(x =>
                    new base_CustomerReminderModel(x)));
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region RunRewardCutOffService

        private void RunRewardCutOffService()
        {
            try
            {
                base_RewardManagerRepository rewardManagerRepository = new base_RewardManagerRepository();

                //Get configuration reward.
                base_RewardManager rewardManager = rewardManagerRepository.Get(x => x.Status == (short)StatusBasic.Active);
                if (rewardManager == null)
                {
                    //throw new Exception("Reward Configuration not found!");
                    //Xceed.Wpf.Toolkit.MessageBox.Show("Reward Configuration not found!", Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                _rewardManagerModel = new base_RewardManagerModel(rewardManager);

                if (_rewardManagerModel.CutOffType == (short)CutOffType.Date)
                {
                    RunRewardWithDateType();
                }
                else if (_rewardManagerModel.CutOffType == (short)CutOffType.CashOrPoint)
                {
                    RunRewardWithCashOrPointType();

                    _timerRewardService = new DispatcherTimer(DispatcherPriority.Background);
                    _timerRewardService.Interval = new TimeSpan(0, 5, 0);
                    _timerRewardService.Tick += new EventHandler(TimerRewardServiceTick);
                    _timerRewardService.Start();
                }
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            finally
            {
                if (_rewardCustomerList == null)
                {
                    RewardCustomerList = new CollectionBase<base_GuestModel>();
                }
            }
        }

        #region RunRewardWithDateType

        private void RunRewardWithDateType()
        {
            // Determine start date.
            DateTime? startDate = null;
            if (_rewardManagerModel.CurrentDayCutOff != 0)
            {
                startDate = DateTime.FromOADate(_rewardManagerModel.CurrentDayCutOff).Date;
            }
            else
            {
                startDate = _rewardManagerModel.EndDate;
            }

            // Check start date.
            if (!startDate.HasValue)
            {
                //throw new Exception("Can not determine start date.");
                return;
            }

            // Determine reward date.
            DateTime moment = DateTime.Now.Date;
            DateTime rewardDate = moment.AddDays(-1);
            switch ((CutOffScheduleType)_rewardManagerModel.CutOffScheduleType)
            {
                #region Weekly

                case CutOffScheduleType.Weekly:

                    foreach (WeeklySchedule dayOfWeek in Enum.GetValues(typeof(WeeklySchedule)))
                    {
                        short value = (short)dayOfWeek;
                        if (value > 0 && ((_rewardManagerModel.WeeklyOnDay & value) == value))
                        {
                            rewardDate = RecurrencePattern.GetDateBaseOnWeeklySchedule(startDate.Value.Date, dayOfWeek, _rewardManagerModel.WeeklyNumber);
                            if (rewardDate < moment)
                            {
                                // Try to get next weeks.
                                rewardDate = RecurrencePattern.GetDateBaseOnWeeklySchedule(startDate.Value.Date, dayOfWeek, _rewardManagerModel.WeeklyNumber, true);
                            }
                            if (rewardDate.Date == moment)
                            {
                                break;
                            }
                        }
                    }

                    break;

                #endregion

                #region Monthly

                case CutOffScheduleType.Monthly:

                    if (_rewardManagerModel.MOption == (short)MonthOption.Day)
                    {
                        rewardDate = RecurrencePattern.GetDateBaseOnMonthlySchedule(startDate.Value.Date, _rewardManagerModel.MonthlyDay, _rewardManagerModel.MonthlyEveryMonth);
                        if (rewardDate < moment)
                        {
                            // Try to get next months.
                            rewardDate = RecurrencePattern.GetDateBaseOnMonthlySchedule(startDate.Value.Date, _rewardManagerModel.MonthlyDay, _rewardManagerModel.MonthlyEveryMonth, true);
                        }
                    }
                    else if (_rewardManagerModel.MOption == (short)MonthOption.WeeksOfMonth)
                    {
                        rewardDate = RecurrencePattern.GetDateBaseOnMonthlySchedule(startDate.Value.Date, (WeeksOfMonth)_rewardManagerModel.MSequence, (DaysOfWeek)_rewardManagerModel.MSequenceOnDay, _rewardManagerModel.MSequenceOnMonth);
                        if (rewardDate < moment)
                        {
                            // Try to get next months.
                            rewardDate = RecurrencePattern.GetDateBaseOnMonthlySchedule(startDate.Value.Date, (WeeksOfMonth)_rewardManagerModel.MSequence, (DaysOfWeek)_rewardManagerModel.MSequenceOnDay, _rewardManagerModel.MSequenceOnMonth, true);
                        }
                    }

                    break;

                #endregion

                #region Yearly

                case CutOffScheduleType.Yearly:

                    if (_rewardManagerModel.YOption == (short)YearOption.Month)
                    {
                        rewardDate = RecurrencePattern.GetDateBaseOnYearlySchedule(startDate.Value.Date, (MonthOfYear)_rewardManagerModel.YearlyOnDay, _rewardManagerModel.YearlyDateOnDay);
                        if (rewardDate < moment)
                        {
                            // Try to get next year.
                            rewardDate = RecurrencePattern.GetDateBaseOnYearlySchedule(startDate.Value.Date, (MonthOfYear)_rewardManagerModel.YearlyOnDay, _rewardManagerModel.YearlyDateOnDay, true);
                        }
                    }
                    else if (_rewardManagerModel.YOption == (short)YearOption.WeeksOfMonth)
                    {
                        rewardDate = RecurrencePattern.GetDateBaseOnYearlySchedule(startDate.Value.Date, (WeeksOfMonth)_rewardManagerModel.YSequence, (DaysOfWeek)_rewardManagerModel.YSequenceOnDay, (MonthOfYear)_rewardManagerModel.YSequenceOnMonth);
                        if (rewardDate < moment)
                        {
                            // Try to get next year.
                            rewardDate = RecurrencePattern.GetDateBaseOnYearlySchedule(startDate.Value.Date, (WeeksOfMonth)_rewardManagerModel.YSequence, (DaysOfWeek)_rewardManagerModel.YSequenceOnDay, (MonthOfYear)_rewardManagerModel.YSequenceOnMonth, true);
                        }
                    }

                    break;

                #endregion
            }

            if (rewardDate.Date == moment)
            {
                // Gets reward customer.
                base_GuestRewardSaleOrderRepository guestRewardSaleOrderRepository = new base_GuestRewardSaleOrderRepository();
                base_SaleOrderRepository saleOrderRepository = new base_SaleOrderRepository();
                base_GuestRepository guestRepository = new base_GuestRepository();
                string customerType = MarkType.Customer.ToDescription();

                // Gets base_GuestRewardSaleOrder list.
                List<base_GuestRewardSaleOrderModel> guestRewardSaleOrderList = new List<base_GuestRewardSaleOrderModel>(guestRewardSaleOrderRepository.GetAll(x =>
                    x.GuestRewardId == 0).Select(x => new base_GuestRewardSaleOrderModel(x)));

                if (_rewardManagerModel.RewardAmtType == (int)RewardAmountType.Cur)
                {
                    RewardCustomerList = new CollectionBase<base_GuestModel>(guestRewardSaleOrderList.Join(
                        saleOrderRepository.GetIQueryable(),
                        x => x.SOResource,
                        y => y.Resource,
                        (z, t) => new
                        {
                            CashRewardEarned = z.CashRewardEarned,
                            CustomerResource = new Guid(t.CustomerResource)
                        }).GroupBy(x => x.CustomerResource).Select(x => new
                        {
                            CustomerResource = x.Key,
                            CashReward = x.Sum(y => y.CashRewardEarned)
                        }).Join(
                        guestRepository.GetIQueryable(x => x.Mark == customerType),
                        y => y.CustomerResource,
                        z => z.Resource,
                        (u, t) => new base_GuestModel(t)
                        {
                            CashReward = u.CashReward
                        }));
                }
                else if (_rewardManagerModel.RewardAmtType == (int)RewardAmountType.Point)
                {
                    RewardCustomerList = new CollectionBase<base_GuestModel>(guestRewardSaleOrderList.Join(
                        saleOrderRepository.GetIQueryable(),
                        x => x.SOResource,
                        y => y.Resource,
                        (z, t) => new
                        {
                            PointRewardEarned = z.PointRewardEarned,
                            CustomerResource = new Guid(t.CustomerResource)
                        }).GroupBy(x => x.CustomerResource).Select(x => new
                        {
                            CustomerResource = x.Key,
                            CashReward = (x.Sum(y => y.PointRewardEarned) * _rewardManagerModel.DollarConverter) / _rewardManagerModel.PointConverter
                        }).Join(
                        guestRepository.GetIQueryable(x => x.Mark == customerType),
                        y => y.CustomerResource,
                        z => z.Resource,
                        (u, t) => new base_GuestModel(t)
                        {
                            CashReward = u.CashReward
                        }));
                }

                // Gets addition information.
                foreach (base_GuestModel customer in _rewardCustomerList)
                {
                    customer.AddressCollection = new ObservableCollection<base_GuestAddressModel>(customer.base_Guest.base_GuestAddress.Select(x =>
                        new base_GuestAddressModel(x)));
                    customer.AddressModel = customer.AddressCollection.SingleOrDefault(x => x.IsDefault);

                    // Gets CountryItem
                    if (customer.AddressModel != null)
                    {
                        customer.CountryItem = Common.Countries.SingleOrDefault(x => x.Value == (short)customer.AddressModel.CountryId);
                    }
                }

                // Update rewardDate.

                if (startDate.Value.Date != rewardDate.Date)
                {
                    base_RewardManagerRepository rewardManagerRepository = new base_RewardManagerRepository();
                    _rewardManagerModel.CurrentDayCutOff = (int)rewardDate.Date.ToOADate();
                    _rewardManagerModel.ToEntity();
                    rewardManagerRepository.Commit();
                }
            }
        }

        #endregion

        #region RunRewardWithCashOrPointType

        private void RunRewardWithCashOrPointType()
        {
            // Gets reward customer.
            base_GuestRewardSaleOrderRepository guestRewardSaleOrderRepository = new base_GuestRewardSaleOrderRepository();
            base_SaleOrderRepository saleOrderRepository = new base_SaleOrderRepository();
            base_GuestRepository guestRepository = new base_GuestRepository();
            string customerType = MarkType.Customer.ToDescription();

            // Gets base_GuestRewardSaleOrder list.
            List<base_GuestRewardSaleOrderModel> guestRewardSaleOrderList = new List<base_GuestRewardSaleOrderModel>(guestRewardSaleOrderRepository.GetAll(x =>
                x.GuestRewardId == 0).Select(x => new base_GuestRewardSaleOrderModel(x)));


            if (_rewardManagerModel.RewardAmtType == (int)RewardAmountType.Cur)
            {
                // RewardAmountType is Cur then CutOffCash is certain.
                // Sum CashRewardEarned, compare CutOffCash.
                RewardCustomerList = new CollectionBase<base_GuestModel>(guestRewardSaleOrderList.Join(
                    saleOrderRepository.GetIQueryable(),
                    x => x.SOResource,
                    y => y.Resource,
                    (z, t) => new
                    {
                        CashRewardEarned = z.CashRewardEarned,
                        CustomerResource = new Guid(t.CustomerResource)
                    }).GroupBy(x => x.CustomerResource).Select(x => new
                    {
                        CustomerResource = x.Key,
                        CashReward = x.Sum(y => y.CashRewardEarned)
                    }).Where(x => x.CashReward >= _rewardManagerModel.CutOffCash).Join(
                    guestRepository.GetIQueryable(x => x.Mark == customerType),
                    y => y.CustomerResource,
                    z => z.Resource,
                    (u, t) => new base_GuestModel(t)
                    {
                        CashReward = u.CashReward
                    }));
            }
            else if (_rewardManagerModel.RewardAmtType == (int)RewardAmountType.Point)
            {
                if (_rewardManagerModel.CutOffPoint != 0)
                {
                    // RewardAmountType is Point, CutOffPoint has a value.
                    // Sum PointRewardEarned, compare with CutOffPoint, convert to cash.
                    RewardCustomerList = new CollectionBase<base_GuestModel>(guestRewardSaleOrderList.Join(
                        saleOrderRepository.GetIQueryable(),
                        x => x.SOResource,
                        y => y.Resource,
                        (z, t) => new
                        {
                            PointRewardEarned = z.PointRewardEarned,
                            CustomerResource = new Guid(t.CustomerResource)
                        }).GroupBy(x => x.CustomerResource).Select(x => new
                        {
                            CustomerResource = x.Key,
                            SumPointRewardEarned = x.Sum(y => y.PointRewardEarned)
                        }).Where(x => x.SumPointRewardEarned >= _rewardManagerModel.CutOffPoint).Select(x => new
                        {
                            CustomerResource = x.CustomerResource,
                            CashReward = (x.SumPointRewardEarned * _rewardManagerModel.DollarConverter) / _rewardManagerModel.PointConverter
                        }).Join(
                        guestRepository.GetIQueryable(x => x.Mark == customerType),
                        y => y.CustomerResource,
                        z => z.Resource,
                        (u, t) => new base_GuestModel(t)
                        {
                            CashReward = u.CashReward
                        }));
                }
                else
                {
                    // RewardAmountType is Point, CutOffPoint hasn't a value, CutOffCash has a value.
                    // Sum PointRewardEarned, convert to cash, compare with CutOffCash.
                    RewardCustomerList = new CollectionBase<base_GuestModel>(guestRewardSaleOrderList.Join(
                        saleOrderRepository.GetIQueryable(),
                        x => x.SOResource,
                        y => y.Resource,
                        (z, t) => new
                        {
                            PointRewardEarned = z.PointRewardEarned,
                            CustomerResource = new Guid(t.CustomerResource)
                        }).GroupBy(x => x.CustomerResource).Select(x => new
                        {
                            CustomerResource = x.Key,
                            CashReward = (x.Sum(y => y.PointRewardEarned) * _rewardManagerModel.DollarConverter) / _rewardManagerModel.PointConverter,
                        }).Where(x => x.CashReward >= _rewardManagerModel.CutOffCash).Join(
                        guestRepository.GetIQueryable(x => x.Mark == customerType),
                        y => y.CustomerResource,
                        z => z.Resource,
                        (u, t) => new base_GuestModel(t)
                        {
                            CashReward = u.CashReward
                        }));
                }
            }

            // Gets addition information.
            foreach (base_GuestModel customer in _rewardCustomerList)
            {
                customer.AddressCollection = new ObservableCollection<base_GuestAddressModel>(customer.base_Guest.base_GuestAddress.Select(x =>
                    new base_GuestAddressModel(x)));
                customer.AddressModel = customer.AddressCollection.SingleOrDefault(x => x.IsDefault);

                // Gets CountryItem
                if (customer.AddressModel != null)
                {
                    customer.CountryItem = Common.Countries.SingleOrDefault(x => x.Value == (short)customer.AddressModel.CountryId);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region ChangeMode

        /// <summary>
        /// Open or close seach component.
        /// </summary>
        private void ChangeMode()
        {
            IsOpenSearch = !_isOpenSearch;
            if (!_isOpenSearch)
            {
                ClearKeyword();
                Filter();
            }
        }

        #endregion

        #region NewTask

        /// <summary>
        /// New task.
        /// </summary>
        private void NewTask()
        {
            NewTaskViewModel newTaskViewModel = new NewTaskViewModel(new base_ReminderModel()
            {
                Color = Colors.Red.ToString(),
                DueDate = GetCurrentDate(),
                Time = GetCurrentDateTime()
            });
            bool? result = _dialogService.ShowDialog<NewTaskView>(App.Current.MainWindow.DataContext, newTaskViewModel, "Add Task");
            if (result == true)
            {
                _reminderList.Add(newTaskViewModel.Reminder);
                CountTask();
            }
        }

        #endregion

        #region EditTask

        /// <summary>
        /// Edit task.
        /// </summary>
        private void EditTask()
        {
            NewTaskViewModel newTaskViewModel = new NewTaskViewModel(_selectedReminder);
            bool? result = _dialogService.ShowDialog<NewTaskView>(App.Current.MainWindow.DataContext, newTaskViewModel, "Add Task");
            if (result == true)
            {
                CountTask();
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete task.
        /// </summary>
        private void Delete()
        {
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.Warning, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    base_ReminderRepository reminderRepository = new base_ReminderRepository();
                    List<base_ReminderModel> deletedList = _reminderList.Where(x => x.IsCompleted).ToList();
                    foreach (base_ReminderModel item in deletedList)
                    {
                        item.UserUpdated = Define.USER.LoginName;
                        item.DateUpdated = DateTime.Now;
                        item.ToEntity();
                        reminderRepository.Commit();
                        _reminderList.Remove(item);
                    }
                    CountTask();
                }
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region GetCurrentDateTime

        /// <summary>
        /// Get current date time.
        /// </summary>
        private DateTime GetCurrentDateTime()
        {
            DateTime current = DateTime.Now;
            return new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
        }

        #endregion

        #region GetDateTime

        /// <summary>
        /// Get date time.
        /// </summary>
        private DateTime GetDateTime(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }

        #endregion

        #region GetCurrentDate

        /// <summary>
        /// Get current date.
        /// </summary>
        private DateTime GetCurrentDate()
        {
            return DateTime.Now.Date;
        }

        #endregion

        #region ChangeLoadStatus

        /// <summary>
        /// Change load status.
        /// </summary>
        private void ChangeLoadStatus(string parameter)
        {
            if (parameter == _todayText)
            {
                _isShowReminderToday = true;
                LoadStatus = _todayText;
            }
            else
            {
                _isShowReminderToday = false;
                LoadStatus = _allTasksText;
            }

            ClearKeyword();
            Filter();
        }

        #endregion

        #region ShowTaskInfo

        /// <summary>
        /// Show task information.
        /// </summary>
        private void ShowTaskInfo()
        {
            _dialogService.ShowDialog<TaskInformationView>(App.Current.MainWindow.DataContext, new TaskInformationViewModel(_selectedReminder), "Task Information");
        }

        #endregion

        #region ClearKeyword

        /// <summary>
        /// Clear keyword.
        /// </summary>
        private void ClearKeyword()
        {
            _keyword = null;
            OnPropertyChanged(() => Keyword);
        }

        #endregion

        #region Filter

        /// <summary>
        /// Filter with keyword.
        /// </summary>
        private void Filter(string key)
        {
            ListCollectionView reminderCollectionView = CollectionViewSource.GetDefaultView(ReminderList) as ListCollectionView;
            if (reminderCollectionView != null)
            {
                reminderCollectionView.Filter = (x) =>
                {
                    base_ReminderModel reminder = x as base_ReminderModel;
                    if (reminder == null)
                    {
                        return false;
                    }
                    reminder.IsCompleted = false;

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        return reminder.Task.ToLower().Contains(key.ToLower());
                    }
                    else
                    {
                        return true;
                    }
                };
            }
        }

        #endregion

        #region Filter

        /// <summary>
        /// Filter Today, All Tasks.
        /// </summary>
        private void Filter()
        {
            ListCollectionView reminderCollectionView = CollectionViewSource.GetDefaultView(ReminderList) as ListCollectionView;
            if (reminderCollectionView != null)
            {
                reminderCollectionView.Filter = (x) =>
                {
                    base_ReminderModel reminder = x as base_ReminderModel;
                    if (reminder == null)
                    {
                        return false;
                    }
                    reminder.IsCompleted = false;

                    if (_isShowReminderToday)
                    {
                        return reminder.DueDate.Date == DateTime.Now.Date;
                    }
                    else
                    {
                        return true;
                    }
                };
            }
        }

        #endregion

        #region GetDateInNextMonth

        /// <summary>
        /// Gets date in next month.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetDateInNextMonth(DateTime date)
        {
            DateTime newDate = date.AddMonths(1);

            while (newDate.DayOfWeek != date.DayOfWeek)
            {
                newDate = newDate.AddDays(1);
            }

            return newDate;
        }

        #endregion

        #region GetDateInNextWeek

        /// <summary>
        /// Gets date in next week.
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private DateTime GetDateInNextWeek(DateTime now)
        {
            return now.AddDays(7);
        }

        #endregion

        #region GetNextDate

        /// <summary>
        /// Get next date.
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private DateTime GetNextDate(DateTime now)
        {
            return now.AddDays(1);
        }

        #endregion

        #region UpdateReminder

        /// <summary>
        /// Update reminder.
        /// </summary>
        private void UpdateReminder(base_ReminderModel reminder)
        {
            string commandText = "UPDATE \"base_Reminder\" SET \"IsActived\" = :isActived WHERE \"Id\" = :id";
            NpgsqlCommand command = new NpgsqlCommand(commandText);
            command.Parameters.Add(new NpgsqlParameter("isActived", DbType.Boolean));
            command.Parameters.Add(new NpgsqlParameter("id", DbType.Int32));
            command.Parameters[0].Value = reminder.IsActived;
            command.Parameters[1].Value = reminder.Id;
            DBHelper dbHelper = new DBHelper();
            dbHelper.ExecuteNonQuery(command);

            base_ReminderRepository reminderRepository = new base_ReminderRepository();
            reminderRepository.Refresh(reminder.base_Reminder);
            reminder.IsDirty = false;
        }

        #endregion

        #region CalculateTime

        /// <summary>
        /// Calculate time.
        /// </summary>
        private void CalculateTime(base_ReminderModel reminder)
        {
            if (reminder.Repeat == (short)ReminderRepeat.Once)
            {
                return;
            }

            switch ((ReminderRepeat)reminder.Repeat)
            {
                case ReminderRepeat.Daily:

                    // Nhac nho da qua.
                    if (DateTime.Compare(GetDateTime(reminder.Time), GetCurrentDateTime()) < 0)
                    {
                        // Lay thoi gian nhac nho duoc tinh theo ngay hien tai.
                        DateTime current = DateTime.Now;
                        DateTime reminderTime = new DateTime(current.Year, current.Month, current.Day, reminder.Time.Hour, reminder.Time.Minute, 0);
                        reminder.Time = GetNextDate(reminderTime);
                    }

                    break;

                case ReminderRepeat.Weekly:

                    // Nhac nho da qua.
                    while (DateTime.Compare(GetDateTime(reminder.Time), GetCurrentDateTime()) < 0)
                    {
                        reminder.Time = GetDateInNextWeek(reminder.Time);
                    }

                    break;

                case ReminderRepeat.Monthly:

                    // Nhac nho da qua.
                    while (DateTime.Compare(GetDateTime(reminder.Time), GetCurrentDateTime()) < 0)
                    {
                        reminder.Time = GetDateInNextMonth(reminder.Time);
                    }

                    break;
            }

            if (reminder.IsDirty)
            {
                UpdateReminder(reminder);
                reminder.IsDirty = false;
            }
        }

        #endregion

        #region CountTask

        /// <summary>
        /// Count task.
        /// </summary>
        private void CountTask()
        {
            CountAllTasks = _reminderList.Count;
            CountTodayTasks = _reminderList.Count(x => x.DueDate.Date == DateTime.Now.Date);
            CountAlarmTasks = _reminderList.Count(x => !x.IsCompleted && x.IsActived);
            CountAlarmBithday = _customerReminderList.Count();
        }

        #endregion

        #region OpenAlarmList

        /// <summary>
        /// Open alarm list.
        /// </summary>
        private void OpenAlarmList()
        {
            bool? result = _dialogService.ShowDialog<TaskListReminderView>(App.Current.MainWindow.DataContext, new TaskListReminderViewModel(ReminderList), "Alarm List");
            if (result == true)
            {
                CountTask();
            }
        }

        #endregion

        #region OpenAlarmBirthday

        /// <summary>
        /// Open alarm birthday of customers.
        /// </summary>
        private void OpenAlarmBirthday()
        {
            bool? result = _dialogService.ShowDialog<CustomerReminderView>(App.Current.MainWindow.DataContext, new CustomerReminderViewModel(_customerReminderList), "Alarm List");
            if (result == true)
            {
                CountTask();
            }
        }

        #endregion

        #region OpenRewardCutOffService

        /// <summary>
        /// Open RewardCutOffServiceView
        /// </summary>
        private void OpenRewardCutOffService()
        {
            bool? result = _dialogService.ShowDialog<RewardCutOffServiceView>(App.Current.MainWindow.DataContext, new RewardCutOffServiceViewModel(_rewardManagerModel, _rewardCustomerList), "Reward Cut-Off Service");
            if (result == true)
            {
                RunRewardCutOffService();
            }
        }

        #endregion

        #endregion

        #region Events

        #region TimerReminderTick

        private void TimerReminderTick(object sender, EventArgs e)
        {
            if (_reminderList == null || !_reminderList.Any())
            {
                return;
            }

            DateTime now = GetCurrentDateTime();

            List<base_ReminderModel> alarmList = _reminderList.Where(x =>
                x.IsReminder && !x.IsCompleted && DateTime.Compare(GetDateTime(x.Time), now) == 0).ToList();
            if (alarmList.Any())
            {
                foreach (base_ReminderModel alarm in alarmList)
                {
                    alarm.IsActived = true;
                    UpdateReminder(alarm);
                }
                CountTask();
            }
        }

        #endregion

        #region TimerRewardServiceTick

        private void TimerRewardServiceTick(object sender, EventArgs e)
        {
            RunRewardWithCashOrPointType();
        }

        #endregion

        #endregion
    }
}