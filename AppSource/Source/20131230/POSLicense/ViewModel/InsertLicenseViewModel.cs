using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows.Threading;
using System.Windows;
using CPC.POS.Database;
using System.Data.EntityClient;
using POSLicense.Database;
using Ini;
using System.IO;
using System.Reflection;
using System.Net.Sockets;


namespace POSLicense.ViewModel
{
    class InsertLicenseViewModel : ViewModelBase
    {
        #region Define
        public string Title
        {
            get
            {
                return "Insert License";
            }
        }

        private CClientSocket _client = new CClientSocket("localhost", 8080);

        private UnitOfWork _entityDB;

        private IniFile _settingFile;

        private enum ValidResult
        {
            Success = 0,
            ExpiredDate = 1,
            NotValid = 2
        }
        #endregion

        #region Constructors
        public InsertLicenseViewModel()
        {
            InitialCommand();

            InitialSettingFile();

            this.ExpireDate = DateTime.Now;
        }

        #endregion

        #region Properties

        public string ConnectionString
        {
            get
            {
                return PgDBHelper.BuildEntityConnString("pos2013", IpServerDb, "Database.POS", UserName, Password).ToString();
            }
        }

        public bool IsValidConnectServer
        {
            get
            {
                return !string.IsNullOrWhiteSpace(IpServerDb)
                    && !string.IsNullOrWhiteSpace(UserName)
                    && !string.IsNullOrWhiteSpace(Password)
                    && !string.IsNullOrWhiteSpace(IpServerApp)
                    && Port > 0;
            }
        }

        #region IsConnected
        private bool _isConnected;
        /// <summary>
        /// Gets or sets the IsConnected.
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(() => IsConnected);
                }
            }
        }
        #endregion

        #region IpServerDb
        private string _ipServerDb;
        /// <summary>
        /// Gets or sets the ipServerDb.
        /// </summary>
        public string IpServerDb
        {
            get { return _ipServerDb; }
            set
            {
                if (_ipServerDb != value)
                {
                    _ipServerDb = value;
                    OnPropertyChanged(() => IpServerDb);

                }
            }
        }
        #endregion

        #region IsDbConencted
        private bool _isDbConnected;
        /// <summary>
        /// Gets or sets the IsDbConencted.
        /// </summary>
        public bool IsDbConnected
        {
            get { return _isDbConnected; }
            set
            {
                if (_isDbConnected != value)
                {
                    _isDbConnected = value;
                    OnPropertyChanged(() => IsDbConnected);
                    SetConnected();
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

        #region Password
        private string _password;
        /// <summary>
        /// Gets or sets the Password.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(() => Password);
                }
            }
        }
        #endregion

        #region IpServerApp
        private string _ipServerApp;
        /// <summary>
        /// Gets or sets the IpServerApp.
        /// </summary>
        public string IpServerApp
        {
            get { return _ipServerApp; }
            set
            {
                if (_ipServerApp != value)
                {
                    _ipServerApp = value;
                    OnPropertyChanged(() => IpServerApp);
                }
            }
        }
        #endregion

        #region IsAppConnected
        private bool _isAppConnected;
        /// <summary>
        /// Gets the IsAppConnected.
        /// </summary>
        public bool IsAppConnected
        {
            get
            {
                if (_client == null)
                    _isAppConnected = false;
                else
                    _isAppConnected = _client.Connected;
                return _isAppConnected;
            }
            //set
            //{
            //    if (_isAppConnected != value)
            //    {
            //        _isAppConnected = value;
            //        OnPropertyChanged(() => IsAppConnected);
            //        SetConnected();
            //    }
            //}
        }
        #endregion

        #region Port
        private int _port;
        /// <summary>
        /// Gets or sets the Port.
        /// </summary>
        public int Port
        {
            get { return _port; }
            set
            {
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged(() => Port);
                }
            }
        }
        #endregion

        #region LicenseName
        private string _licenseName;
        /// <summary>
        /// Gets or sets the LicenseName.
        /// </summary>
        public string LicenseName
        {
            get { return _licenseName; }
            set
            {
                if (_licenseName != value)
                {
                    _licenseName = value;
                    OnPropertyChanged(() => LicenseName);
                }
            }
        }
        #endregion

        #region ApplicationId
        private string _applicationId;
        /// <summary>
        /// Gets or sets the ApplicationId.
        /// </summary>
        public string ApplicationId
        {
            get { return _applicationId; }
            set
            {
                if (_applicationId != value)
                {
                    _applicationId = value;
                    OnPropertyChanged(() => ApplicationId);
                }
            }
        }
        #endregion

        #region StoreQty
        private int _storeQty;
        /// <summary>
        /// Gets or sets the StoreQty.
        /// </summary>
        public int StoreQty
        {
            get { return _storeQty; }
            set
            {
                if (_storeQty != value)
                {
                    _storeQty = value;
                    OnPropertyChanged(() => StoreQty);
                }
            }
        }
        #endregion

        #region ExpireDate
        private DateTime? _expireDate;
        /// <summary>
        /// Gets or sets the ExpireDate.
        /// </summary>
        public DateTime? ExpireDate
        {
            get { return _expireDate; }
            set
            {
                if (_expireDate != value)
                {
                    _expireDate = value;
                    OnPropertyChanged(() => ExpireDate);
                }
            }
        }
        #endregion

        #region LicenseNumber
        private string _licenseNumber;
        /// <summary>
        /// Gets or sets the LicenseNumber.
        /// </summary>
        public string LicenseNumber
        {
            get { return _licenseNumber; }
            set
            {
                if (_licenseNumber != value)
                {
                    _licenseNumber = value;
                    OnPropertyChanged(() => LicenseNumber);
                }
            }
        }
        #endregion

        public base_Configuration Configuration { get; set; }

        #endregion

        #region Commands Methods

        #region ConnectServerCommand
        /// <summary>
        /// Gets the ConnectServer Command.
        /// <summary>

        public RelayCommand<object> ConnectServerCommand { get; private set; }


        /// <summary>
        /// Method to check whether the ConnectServer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnConnectServerCommandCanExecute(object param)
        {
            return IsValidConnectServer;
        }


        /// <summary>
        /// Method to invoke when the ConnectServer command is executed.
        /// </summary>
        private void OnConnectServerCommandExecute(object param)
        {
            ConnectAppServer();

            IsDbConnected = ConnectDb();

            WriteIniFile();

            NotifyConnectError();


        }


        #endregion

        #region BuildStoreIdCommand
        /// <summary>
        /// Gets the BuildStoreId Command.
        /// <summary>

        public RelayCommand<object> BuildStoreIdCommand { get; private set; }



        /// <summary>
        /// Method to check whether the BuildStoreId command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnBuildStoreIdCommandCanExecute(object param)
        {
            return IsDbConnected;
        }


        /// <summary>
        /// Method to invoke when the BuildStoreId command is executed.
        /// </summary>
        private void OnBuildStoreIdCommandExecute(object param)
        {
            try
            {
                Configuration = _entityDB.Get<base_Configuration>(x => true);
                if (Configuration != null)
                {
                    //get License Name from db
                    this.LicenseName = Configuration.CompanyName;
                    this.StoreQty = Convert.ToInt32(Configuration.TotalStore);
                    //Get
                    string volumnSerial = MachineUtility.GetHddSerial();
                    string appId = string.Empty;
                    if (!string.IsNullOrWhiteSpace(volumnSerial))
                        appId = ProductKeyGenerator.ToNumbericValue(volumnSerial);
                    this.ApplicationId = appId;
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region InsertRegisterNumberCommand
        /// <summary>
        /// Gets the InsertRegisterNumber Command.
        /// <summary>

        public RelayCommand<object> InsertRegisterNumberCommand { get; private set; }

        /// <summary>
        /// Method to check whether the InsertRegisterNumber command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnInsertRegisterNumberCommandCanExecute(object param)
        {
            return !string.IsNullOrWhiteSpace(LicenseNumber) && IsConnected
                && !string.IsNullOrWhiteSpace(LicenseName)
                && !string.IsNullOrWhiteSpace(ApplicationId);
        }


        /// <summary>
        /// Method to invoke when the InsertRegisterNumber command is executed.
        /// </summary>
        private void OnInsertRegisterNumberCommandExecute(object param)
        {
            //Descryt License Number
            string licenseDecrypt = ProductKeyGenerator.DescrytProductKey(LicenseNumber);
            var licenseArray = licenseDecrypt.Split('|');

            //Get StoreCode| POSID | ProjectId | ExpiredDates
            string md5License = licenseArray[0];
            int storeCode = Convert.ToInt32(licenseArray[1]);
            string posId = licenseArray[2];
            int projectId = Convert.ToInt32(licenseArray[3]);
            int intExpiredDate = Convert.ToInt32(licenseArray[4]);

            //Parse Date integer to Datetime
            if (intExpiredDate > 0)
                this.ExpireDate = DateTime.FromOADate(intExpiredDate);
            else
                this.ExpireDate = null;

            ValidResult result = ValidateLicense(md5License, ExpireDate);
            switch (result)
            {
                case ValidResult.Success:
                    //Insert To Db StoreCode| POSID | ProjectId | ExpiredDate ; GenDate
                    UpdateConfig(LicenseNumber, ExpireDate, md5License, storeCode, posId);
                    MessageBox.Show("License Key Validate Success!", "Validation License Key", MessageBoxButton.OK,MessageBoxImage.Information);
                    break;
                case ValidResult.ExpiredDate:
                    MessageBox.Show("License key is expired", "Validation License Key", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case ValidResult.NotValid:
                    MessageBox.Show("License key is not valid", "Validation License Key", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

        }


        #endregion
        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            ConnectServerCommand = new RelayCommand<object>(OnConnectServerCommandExecute, OnConnectServerCommandCanExecute);
            BuildStoreIdCommand = new RelayCommand<object>(OnBuildStoreIdCommandExecute, OnBuildStoreIdCommandCanExecute);
            InsertRegisterNumberCommand = new RelayCommand<object>(OnInsertRegisterNumberCommandExecute, OnInsertRegisterNumberCommandCanExecute);
        }

        /// <summary>
        /// Connect to Application Server
        /// </summary>
        private void ConnectAppServer()
        {
            _client = new CClientSocket(IpServerApp, Port);
            _client.OnConnect += new CClientSocket.ConnectionDelegate(_client_OnConnect);
            _client.OnDisconnect += new CClientSocket.ConnectionDelegate(_client_OnDisconnect);
            _client.OnRead += new CClientSocket.ConnectionDelegate(_client_OnRead);
            _client.OnError += new CClientSocket.ErrorDelegate(_client_OnError);
            _client.Connect();
        }

        /// <summary>
        /// Diconnect to Application Server
        /// </summary>
        private void DisConnectAppServer()
        {
            if (_client != null && _client.Connected)
                _client.Disconnect();
        }

        /// <summary>
        /// Connect DatabaseServer
        /// </summary>
        private bool ConnectDb()
        {
            try
            {
                _entityDB = new UnitOfWork(ConnectionString);
                if (_entityDB.Connection.State == System.Data.ConnectionState.Closed)
                {
                    //Try Connection
                    _entityDB.Connection.Open();
                }
                return true;
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException)
                    {
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                _log4net.Fatal(errorMessage);
                MessageBox.Show(errorMessage, "ReflectionTypeLoadException");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
                _log4net.Error(ex);
                _entityDB.Dispose();
                return false;
            }

        }

        /// <summary>
        /// Set IsConnected 
        /// </summary>
        private void SetConnected()
        {
            IsConnected = IsAppConnected && IsDbConnected;
        }

        /// <summary>
        /// Initial file setting
        /// </summary>
        private void InitialSettingFile()
        {
            string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            _settingFile = new IniFile(System.IO.Path.Combine(directoryName, "Setting.ini"));
            ReadIniFile();
        }

        /// <summary>
        /// Store Information to ini file
        /// </summary>
        private void WriteIniFile()
        {
            if (_settingFile != null)
            {
                _settingFile.IniWriteValue("Database", "IpServerDb", IpServerDb);
                _settingFile.IniWriteValue("Database", "UserName", UserName);
                _settingFile.IniWriteValue("Database", "Password", Password);
                _settingFile.IniWriteValue("AppServer", "IpServerApp", IpServerApp);
                _settingFile.IniWriteValue("AppServer", "Port", Port.ToString());
            }
        }
        /// <summary>
        /// Read Information from ini File
        /// </summary>
        private void ReadIniFile()
        {
            if (_settingFile != null)
            {
                IpServerDb = _settingFile.IniReadValue("Database", "IpServerDb");
                UserName = _settingFile.IniReadValue("Database", "UserName");
                Password = _settingFile.IniReadValue("Database", "Password");
                IpServerApp = _settingFile.IniReadValue("AppServer", "IpServerApp");
                string portStr = _settingFile.IniReadValue("AppServer", "Port");
                Port = string.IsNullOrWhiteSpace(portStr) ? 0 : Convert.ToInt32(portStr);
            }
        }

        /// <summary>
        /// Notify connected error
        /// </summary>
        private void NotifyConnectError()
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
            {
                if (!IsAppConnected && !IsDbConnected)
                {
                    MessageBox.Show("Not connect to server, Please try again", "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (!IsAppConnected)
                    {
                        MessageBox.Show("Application Server in not connected", "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (!IsDbConnected)
                    {
                        MessageBox.Show("Database Server in not connected", "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }));
        }

        /// <summary>
        /// Check Validate License key
        /// </summary>
        /// <param name="md5License"></param>
        /// <param name="expiredDate"></param>
        /// <returns></returns>
        private ValidResult ValidateLicense(string md5License, DateTime? expiredDate)
        {
            bool validationResult = false;
            //Validate License Number with Machine(ApplicationId & TotalStore)
            if (Configuration != null)
            {
                LicenseModel licenseModel = new LicenseModel(ApplicationId, Convert.ToInt32(Configuration.TotalStore));
                validationResult = licenseModel.LisenceKey.Equals(md5License);
            }

            if (validationResult)
            {
                if (expiredDate.HasValue && expiredDate.Value.Date < DateTime.Today)
                    return ValidResult.ExpiredDate;
                else
                    return ValidResult.Success;
            }
            else
            {
                return ValidResult.NotValid;
            }
        }

        /// <summary>
        /// Update License Key info to config db
        /// </summary>
        /// <param name="licenseKey"></param>
        /// <param name="expiredDate"></param>
        /// <param name="md5lic"></param>
        /// <param name="storeCode"></param>
        /// <param name="posId"></param>
        /// <returns></returns>
        private bool UpdateConfig(string licenseKey, DateTime? expiredDate, string md5lic, int storeCode, string posId)
        {
            bool result = false;
            try
            {
                this.Configuration.GenDate = DateTime.Now;
                this.Configuration.RegNo = licenseKey;
                int expireDateInt = expiredDate.HasValue? Convert.ToInt32(expiredDate.Value.ToOADate()):0;
                this.Configuration.ExpireDate = expireDateInt;
                this.Configuration.MD5Lic = md5lic;
                this.Configuration.StoreCode = Convert.ToInt16(storeCode);
                this.Configuration.POSId = posId;
                _entityDB.Update<base_Configuration>(this.Configuration);
                _entityDB.Commit();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        #endregion

        #region Event
        /// <summary>
        /// Client Connected
        /// </summary>
        /// <param name="soc"></param>
        private void _client_OnConnect(System.Net.Sockets.Socket soc)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
            {
                //IsAppConnected = true;
                SetConnected();
            }));

        }

        /// <summary>
        /// Client Disconnect
        /// </summary>
        /// <param name="soc"></param>
        private void _client_OnDisconnect(System.Net.Sockets.Socket soc)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
            {
                SetConnected();
            }));
        }

        /// <summary>
        /// Client Read Data Send From Server
        /// </summary>
        /// <param name="soc"></param>
        private void _client_OnRead(System.Net.Sockets.Socket soc)
        {

        }

        /// <summary>
        /// Client Error
        /// </summary>
        /// <param name="ErroMessage"></param>
        /// <param name="soc"></param>
        /// <param name="ErroCode"></param>
        private void _client_OnError(string ErroMessage, System.Net.Sockets.Socket soc, int ErroCode)
        {
            if (ErroCode != 0)
            {
                Console.WriteLine(ErroCode);
                if (!Convert.ToInt32(SocketError.ConnectionReset).Equals(ErroCode))//Avoid Show message box when Client disconnectd ErrorCode 10054
                {
                    MessageBox.Show("Server is disconnected", "Connection", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ErroMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    //IsAppConnected = false;

                    SetConnected();
                }));
            }
        }
        #endregion

    }


}
