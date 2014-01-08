using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.POS.ViewModel;
using CPC.Service;
using CPC.Service.FrameworkDialogs.OpenFile;
using log4net;
using log4net.Config;
using CPC.POS.Model;

namespace CPC.POS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        #region Defines

        private static readonly ILog m_Logger = LogManager.GetLogger(typeof(App).Name);
        private LoginView _loginView;
        private bool IsUserAuthenicated { set; get; }
        private bool _isLogout = false;

        #endregion

        #region Properties

        readonly static Messenger.Message _messenger = new Messenger.Message();
        /// <summary>
        /// Gets the Messenger
        /// </summary>
        internal static Messenger.Message Messenger
        {
            get { return _messenger; }
        }

        #endregion

        #region Constructors

        public App()
        {
            this.InitializeComponent();
            // Configure the service locator
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();
        }

        #endregion

        #region Methods

        #region InitialData
        /// <summary>
        /// Initial data from database
        /// </summary>
        private void InitialData()
        {
            try
            {
                Define.CONFIGURATION = null;
                Define.NumericFormat = null;
                Define.CurrencyFormat = null;
                Define.ConverterCulture = null;
                Define.DecimalFormat = "{0:#,##0.00;(#,##0.00);0}";
                base_ConfigurationRepository configurationRepository = new base_ConfigurationRepository();
                IQueryable<base_Configuration> configQuery = configurationRepository.GetIQueryable();
                if (configQuery.Count() > 0)
                {
                    configurationRepository.Refresh(configQuery);
                    Define.CONFIGURATION = new Model.base_ConfigurationModel(configurationRepository.GetIQueryable().FirstOrDefault());
                    Define.DecimalPlaces = Define.CONFIGURATION.DecimalPlaces.Value;
                    Define.ConverterCulture = new CultureInfo(Define.CONFIGURATION.FomartCurrency);
                    //To Define currency format
                    if (Define.CONFIGURATION.NegativeNumber == 0)
                    {
                        Define.NumericFormat = this.FormatCurrency(string.Empty, string.Empty, (Int16)Define.NumericDecimalDigits);
                        Define.CurrencyFormat = this.FormatCurrency(Define.CONFIGURATION.FomartCurrency, Define.CONFIGURATION.CurrencySymbol, Define.CONFIGURATION.DecimalPlaces);
                    }
                    else
                    {
                        Define.NumericFormat = "{0:N" + Define.NumericDecimalDigits + "}";
                        string format = "{0:N" + Define.CONFIGURATION.DecimalPlaces + "}";
                        if (Define.CONFIGURATION.FomartCurrency.Equals("vi-VN"))
                            Define.CurrencyFormat = string.Format("{0} {1}", format, Define.CONFIGURATION.CurrencySymbol);
                        else
                            Define.CurrencyFormat = string.Format("{0}{1}", Define.CONFIGURATION.CurrencySymbol, format);
                    }

                    // Get symbol
                    Define.CurrencySymbol = Define.CONFIGURATION.CurrencySymbol;

                    //To define Text Number Align
                    if (Define.CONFIGURATION.TextNumberAlign == 0)
                        Define.TextNumberAlign = TextAlignment.Right;
                    else
                        Define.TextNumberAlign = TextAlignment.Left;

                    if (Define.CONFIGURATION.IsStateCode)
                        Define.StateDisplayMemberPath = "Symbol";
                    else
                        Define.StateDisplayMemberPath = "Text";
                }

                //To get JobTitle
                base_GenericCodeRepository _genericCodeRepository = new base_GenericCodeRepository();
                string JobTitleCode = GenericCode.JT.ToString();
                IList<base_GenericCode> codes = _genericCodeRepository.GetAll(x => x.Code.Equals(JobTitleCode));
                IList<ComboItem> comboItems = new List<ComboItem>();
                if (codes != null)
                {
                    foreach (var item in codes)
                    {
                        ComboItem ItemJobTitle = new ComboItem()
                        {
                            ObjValue = item.Id,
                            Value = Convert.ToInt16(item.Id),
                            Text = item.Name,
                            Symbol = item.Code
                        };
                        comboItems.Add(ItemJobTitle);
                    }
                }
                Common.JobTitles = comboItems;
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Configuration");
                throw;
            }
        }

        private string FormatCurrency(string fomartCurrency, string CurrencySymbol, short? decimalPlaces)
        {
            string format = string.Empty;
            string Negativeformat = string.Empty;
            if (decimalPlaces > 0)
            {
                Negativeformat = "0.";
                for (int i = 0; i < decimalPlaces; i++)
                    Negativeformat += "0";
            }
            else
                Negativeformat = string.Empty;
            if (string.IsNullOrEmpty(fomartCurrency))
                format = "{" + string.Format("0:#,##{0};(#,##{0});0", Negativeformat) + "}";
            else if (fomartCurrency.Equals("vi-VN"))
                format = "{" + string.Format("0:#,##{1} {0};(#,##{1} {0});0", CurrencySymbol, Negativeformat) + "}";
            else
                format = "{" + string.Format("0:{0}#,##{1};({0}#,##{1});0", CurrencySymbol, Negativeformat) + "}";
            return format;
        }
        #endregion

        #region OpenLoginView
        /// <summary>
        /// To open loginView.
        /// </summary>
        private void OpenLoginView(bool ISUpdateOldUser)
        {
            try
            {
                if (ISUpdateOldUser)
                {
                    //Reload configuration data from database
                    this.InitialData();
                }
                if (Application.Current.ShutdownMode != ShutdownMode.OnExplicitShutdown)
                    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                _loginView = new LoginView();
                _loginView.Loaded += delegate
                {
                    _loginView.DataContext = new LoginViewModel(_loginView);
                };
                bool? res = _loginView.ShowDialog();
                if (false == res && !IsUserAuthenicated)
                {
                    _loginView.Close();
                    _loginView = null;
                    this.Shutdown(1);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error("OpenLoginView" + ex.ToString());
            }
        }
        #endregion

        #region OpenMainWindowView
        private void OpenMainWindowView()
        {
            try
            {
                // Initial main window
                MainWindow mainWindow = new MainWindow();

                // Initial main view model
                MainViewModel mainViewModel = new MainViewModel();
                mainWindow.DataContext = mainViewModel;

                // Register Activated, Deactivated and StateChanged event to auto lock screen
                this.Activated += (sender, e) => { IdleTimeHelper.LostFocusTime = null; };
                this.Deactivated += (sender, e) => { IdleTimeHelper.LostFocusTime = DateTimeExt.Now; };
                mainWindow.StateChanged += (sender, e) =>
                {
                    if (mainViewModel.IsLockScreen)
                    {
                        mainViewModel.IsLockScreen = false;

                        // Restore window when window state is minimized
                        if (mainWindow.WindowState.Equals(WindowState.Minimized))
                            IdleTimeHelper.RestoreWindow();

                        // Display lock screen view
                        mainViewModel.OnLockScreenCommandExecute();
                    }

                    if (mainWindow.WindowState.Equals(WindowState.Minimized))
                        IdleTimeHelper.LostFocusTime = DateTimeExt.Now;
                };

                // Register Closing event to process close main window
                mainWindow.Closing += (sender, e) =>
                {
                    // Get can close main window
                    bool result = mainViewModel.CloseCommand.CanExecute(null);
                    if (!result)
                        mainViewModel.CloseCommand.Execute(null);
                    e.Cancel = result;

                    // Shutdown the application
                    if (!this._isLogout && !result)
                    {
                        //Define.SynchronizationViewModel.CloseService();
                        this.Shutdown();
                    }
                };

                // Show main window
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                m_Logger.Error("OpenMainWindowView" + ex.ToString());

            }
        }
        #endregion

        #region LogOutCallback
        /// <summary>
        /// Logout Callback, clear user login and call the login view
        /// </summary>
        private void LogOutCallback()
        {
            try
            {
                this._isLogout = true;
                //To close successful
                if (Application.Current.ShutdownMode != ShutdownMode.OnExplicitShutdown)
                    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                App.Current.MainWindow.Close();
                if (App.Current.MainWindow == null)
                {
                    CPC.POS.Properties.Settings.Default.Username = string.Empty;
                    CPC.POS.Properties.Settings.Default.Password = string.Empty;
                    CPC.POS.Properties.Settings.Default.Save();
                    Define.Username = string.Empty;
                    Define.Password = string.Empty;
                    Define.USER = null;
                    Define.USER_AUTHORIZATION = null;
                    this.IsUserAuthenicated = false;
                    App.WriteUserLog("LogOut", "User log out.");
                    CPC.POS.Properties.Settings.Default.Reload();
                    this.OpenLoginView(true);
                }
                else
                    Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                this._isLogout = false;
            }
            catch (Exception ex)
            {
                m_Logger.Error("LogOutCallback" + ex.ToString());
            }
        }
        #endregion

        #region AuthenticateCallback
        /// <summary>
        /// Callback from LogOnScreenViewModel after user been verified
        /// Before show the main window, close the existed loginview
        /// </summary>
        /// <param name="IsVerified"></param>
        private void AuthenticateCallback(bool IsVerified)
        {
            try
            {
                // If user been verified, close the loginView
                if (null != this._loginView)
                {
                    this._loginView.Close();
                    this._loginView = null;
                }
                this.IsUserAuthenicated = IsVerified;
                //To clear data on base_UserLogDetail table.
                App.ClearDataOnUserLog();
                this.OpenMainWindowView();
            }
            catch (Exception ex)
            {
                m_Logger.Error("AuthenticateCallback" + ex.ToString());
            }
        }
        #endregion

        #region ClearDataOnUserLog
        /// <summary>
        /// To clear data on base_UserLog table.
        /// </summary>
        public static void ClearDataOnUserLog()
        {
            try
            {
                base_UserLogRepository userLogRepository = new base_UserLogRepository();
                int date = 7;
                if (Define.CONFIGURATION.KeepLog.HasValue)
                    date = -Define.CONFIGURATION.KeepLog.Value;
                DateTime curentDate = DateTimeExt.Now.AddDays(date);
                IQueryable<base_UserLog> userLogs = userLogRepository.GetIQueryable(x => x.ConnectedOn < curentDate);
                if (userLogs != null && userLogs.Count() > 0)
                {
                    userLogRepository.Delete(userLogs);
                    userLogRepository.Commit();
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error("ClearDataOnUserLog" + ex.ToString());
            }
        }
        #endregion

        #region InserDataOnUserLogDetail
        /// <summary>
        /// To clear data on base_UserLog table.
        /// </summary>
        public static void WriteUserLog(string moduleName, string description)
        {
            try
            {
                if (Define.USER != null && Define.USER.Resource != null && Define.USER.Resource != Guid.Empty)
                {
                    base_UserLogDetail userLogDetail = new base_UserLogDetail();
                    //userLogDetail.Id = Guid.NewGuid();
                    userLogDetail.UserLogId = Define.USER.UserLogId;
                    userLogDetail.AccessedTime = DateTimeExt.Now;
                    userLogDetail.ModuleName = moduleName;
                    userLogDetail.ActionDescription = description;
                    base_UserLogDetailRepository userLogDetailRepository = new base_UserLogDetailRepository();
                    userLogDetailRepository.Add(userLogDetail);
                    userLogDetailRepository.Commit();
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error("WriteUserLog" + ex.ToString());
            }
        }
        public static bool IsExistResourceAccount(Guid Resource)
        {
            base_ResourceAccountRepository resourceAccountRepository = new base_ResourceAccountRepository();
            var query = resourceAccountRepository.GetIQueryable(x => x.Resource == Resource).SingleOrDefault();
            if (query != null)
                return true;
            return false;
        }
        #endregion

        #endregion

        #region Override Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                // Configure Log4Net
                XmlConfigurator.Configure();
                //Get Configuration data from database
                this.InitialData();
                //To register to execute logout
                App.Messenger.Register(Define.USER_LOGOUT_RESULT,
                                          new Action(() => LogOutCallback()));
                //To register to execute opening MainWindow.
                App.Messenger.Register(Define.USER_LOGIN_RESULT,
                                       new Action<bool>((result) => AuthenticateCallback(result)));
                //To open LoginView
                this.OpenLoginView(false);
            }
            catch (Exception ex)
            {
                m_Logger.Error("OnStartup" + ex.ToString());
            }
        }

        #endregion

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return true;
        }

        #endregion
    }
}
