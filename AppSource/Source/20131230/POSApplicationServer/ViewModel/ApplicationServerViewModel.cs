using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using POSApplicationServer.Model;
using System.IO;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Net.Sockets;

using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Management;

namespace POSApplicationServer.ViewModel
{
    class POSApplicationSeverViewModel : ViewModelBase
    {
        #region Define
        DispatcherTimer _systemTimer = new DispatcherTimer();

        private CServerSocket _serverSocket;
        private ObservableCollection<Socket> _clientList = new ObservableCollection<Socket>();

        private int LIMIT = 5;
        TaskbarIcon notifyIcon;

        private bool isForceDisconnect = false;

        #endregion

        #region Constructors
        public POSApplicationSeverViewModel()
        {
            InitialCommand();
            InitialData();
            _systemTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _systemTimer.Tick += new EventHandler(_systemTimer_Tick);
            _systemTimer.Start();
            StartServer();
        }


        #endregion

        #region Properties
        public string Title
        {
            get
            {
                return "Application Setting";
            }

        }

        #region ComputerName
        private string _computerName;
        /// <summary>
        /// Gets or sets the ComputerName.
        /// </summary>
        public string ComputerName
        {
            get { return _computerName; }
            set
            {
                if (_computerName != value)
                {
                    _computerName = value;
                    OnPropertyChanged(() => ComputerName);
                }
            }
        }
        #endregion

        #region SystemTime
        private DateTime _systemTime;
        /// <summary>
        /// Gets or sets the SystemTime.
        /// </summary>
        public DateTime SystemTime
        {
            get { return _systemTime; }
            set
            {
                if (_systemTime != value)
                {
                    _systemTime = value;
                    OnPropertyChanged(() => SystemTime);
                }
            }
        }
        #endregion

        #region WindowsUpTime
        private DateTime? _windowsUpTime;
        /// <summary>
        /// Gets or sets the WindowsUpTime.
        /// </summary>
        public DateTime? WindowsUpTime
        {
            get { return _windowsUpTime; }
            set
            {
                if (_windowsUpTime != value)
                {
                    _windowsUpTime = value;
                    OnPropertyChanged(() => WindowsUpTime);
                }
            }
        }
        #endregion

        #region FreeSystemRam
        private string _freeSystemRam;
        /// <summary>
        /// Gets or sets the FreeRam.
        /// </summary>
        public string FreeSystemRam
        {
            get { return _freeSystemRam; }
            set
            {
                if (_freeSystemRam != value)
                {
                    _freeSystemRam = value;
                    OnPropertyChanged(() => FreeSystemRam);
                }
            }
        }
        #endregion

        #region TotalSystemRam
        private string _totalSystemRam;
        /// <summary>
        /// Gets or sets the TotalSystemRam.
        /// </summary>
        public string TotalSystemRam
        {
            get { return _totalSystemRam; }
            set
            {
                if (_totalSystemRam != value)
                {
                    _totalSystemRam = value;
                    OnPropertyChanged(() => TotalSystemRam);
                }
            }
        }
        #endregion

        #region HardDiskCollection
        private List<ItemModel> _hardDiskCollection = new List<ItemModel>();
        /// <summary>
        /// Gets or sets the HardDiskCollection.
        /// </summary>
        public List<ItemModel> HardDiskCollection
        {
            get { return _hardDiskCollection; }
            set
            {
                if (_hardDiskCollection != value)
                {
                    _hardDiskCollection = value;
                    OnPropertyChanged(() => HardDiskCollection);
                }
            }
        }
        #endregion

        //Network
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

        #region ServerLog
        private string _severLog;
        /// <summary>
        /// Gets or sets the ServerLog.
        /// </summary>
        public string ServerLog
        {
            get { return _severLog; }
            set
            {
                if (_severLog != value)
                {
                    _severLog = value;
                    OnPropertyChanged(() => ServerLog);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region RefreshCommand
        /// <summary>
        /// Gets the Refesh Command.
        /// <summary>

        public RelayCommand<object> RefreshCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Refesh command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRefreshCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Refesh command is executed.
        /// </summary>
        private void OnRefreshCommandExecute(object param)
        {
            StartServer();
        }
        #endregion

        #region HiddenCommand

        /// <summary>
        /// Gets the Hidden Command.
        /// <summary>

        public RelayCommand<object> HiddenCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Hidden command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnHiddenCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Hidden command is executed.
        /// </summary>
        private void OnHiddenCommandExecute(object param)
        {
            notifyIcon = new TaskbarIcon();
            SetToolTipNotitfyIcon();
            notifyIcon.DoubleClickCommand = NotifyIconDoubleClickCommand;
            //set Image
            Uri iconUri = new Uri("pack://application:,,,/Image/POS.ico", UriKind.RelativeOrAbsolute);
            notifyIcon.IconSource = BitmapFrame.Create(iconUri);
            App.Current.Windows[0].Hide();
        }
        #endregion

        #region NotifyIconDoubleClickCommand
        /// <summary>
        /// Gets the NotifyIconDoubleClick Command.
        /// <summary>

        public RelayCommand<object> NotifyIconDoubleClickCommand { get; private set; }



        /// <summary>
        /// Method to check whether the NotifyIconDoubleClick command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNotifyIconDoubleClickCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the NotifyIconDoubleClick command is executed.
        /// </summary>
        private void OnNotifyIconDoubleClickCommandExecute(object param)
        {
            App.Current.Windows[0].Show();
            if (notifyIcon != null)
                notifyIcon.Dispose();
        }

        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            RefreshCommand = new RelayCommand<object>(OnRefreshCommandExecute, OnRefreshCommandCanExecute);
            HiddenCommand = new RelayCommand<object>(OnHiddenCommandExecute, OnHiddenCommandCanExecute);
            NotifyIconDoubleClickCommand = new RelayCommand<object>(OnNotifyIconDoubleClickCommandExecute, OnNotifyIconDoubleClickCommandCanExecute);
        }

        /// <summary>
        /// Initial Data
        /// </summary>
        private void InitialData()
        {
            GetAllHardDiskInfo();
            this.ComputerName = string.Format("{0} {1}", MachineUtility.GetHostName(), MachineUtility.GetIpAddress());

            //Set System Time
            this.SystemTime = DateTime.Now;

            //Set Total Ram
            this.TotalSystemRam = string.Format("{0} MB", ((MachineUtility.GetTotalRam() / 1024) / 1024).ToString("N0"));

            //Set Free Ram
            //this.FreeSystemRam = string.Format("{0} MB", ((NetworkUltil.GetFreeRam() / 1024) / 1024);

            string lastStatup = MachineUtility.LastBootupTime();
            DateTime lastStartUpDate;
            if (!string.IsNullOrWhiteSpace(lastStatup))
            {
                if (DateTime.TryParse(lastStatup, out lastStartUpDate))
                    this.WindowsUpTime = lastStartUpDate;
                else
                    this.WindowsUpTime = null;
            }
        }

        /// <summary>
        /// Get AllHardDiskInfo
        /// </summary>
        private void GetAllHardDiskInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            int i = 1;
            foreach (DriveInfo d in allDrives)
            {
                try
                {
                    ItemModel itemModel = new ItemModel();
                    itemModel.Id = i;
                    //get & convert byte to mb
                    long freeSpace = (d.AvailableFreeSpace / 1024) / 1024;
                    //Get Disk Volumn or defaul
                    string diskLabel = !string.IsNullOrWhiteSpace(d.VolumeLabel) ? d.VolumeLabel : "Local Disk";
                    itemModel.Detail = string.Format("{0} [{1}] Free Space : {2} MB", d.Name, diskLabel, freeSpace.ToString("N0"));
                    itemModel.Text = d.Name;
                    _hardDiskCollection.Add(itemModel);
                }
                catch { }
            }
        }

        /// <summary>
        /// Start Server Socket
        /// </summary>
        private void StartServer()
        {
            if (this.Port > 0)
            {
                if (_serverSocket != null)
                {
                    if (_serverSocket.ActiveConnections > 0) //Connect In Use
                        DisconnectClients();
                    else
                        ActiveServer();
                }
                else
                    ActiveServer();

            }

            if (_serverSocket == null || !_serverSocket.ReadyConnected)
            {
                string msg = string.Format("Application not ready!");
                SetSeverLog(msg);
            }

        }


        /// <summary>
        /// Active Server Port to connect
        /// </summary>
        /// <returns></returns>
        private bool ActiveServer()
        {
            if (_serverSocket != null )
            {
                if (_serverSocket.Port == Port)//Avoid Active the same port in use
                    return false;
                else if (_serverSocket.ReadyConnected)//Change to another port
                    _serverSocket.Deactive();
            }

            _serverSocket = new CServerSocket(Port);
            _serverSocket.OnConnect += new CServerSocket.ConnectionDelegate(_serverSocket_OnConnect);
            _serverSocket.OnListen += new CServerSocket.ListenDelegate(_serverSocket_OnListen);
            _serverSocket.OnDisconnect += new CServerSocket.ConnectionDelegate(_serverSocket_OnDisconnect);
            _serverSocket.OnError += new CServerSocket.ErrorDelegate(_serverSocket_OnError);
            _serverSocket.Active();
            return true;
        }




        /// <summary>
        ///  check client in LIMIT client require &  Add connected client to list 
        /// </summary>
        /// <param name="clientSocket"></param>
        private void AddClient(Socket clientSocket)
        {
            try
            {
                App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                   {
                       if (_serverSocket.ActiveConnections < LIMIT)
                       {
                           _clientList.Add(clientSocket);
                       }
                       else
                       {
                           clientSocket.Disconnect(false);
                       }

                   }));
            }
            catch (Exception ex)
            {
                _log4net.Error(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Remove Client is Disconnected
        /// </summary>
        /// <param name="clientSocket"></param>
        private void RemoveClient(Socket clientSocket)
        {
            try
            {
                App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
                {
                    _clientList.Remove(clientSocket);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// Disconnect All Client Connected to server
        /// </summary>
        private void DisconnectClients()
        {
            MessageBoxResult result = MessageBox.Show("Client ready in use this connect. Do you want to disconnect all?", "Disconect client", MessageBoxButton.YesNo);
            if (result.Equals(MessageBoxResult.Yes))
            {
                isForceDisconnect = true;
                foreach (var item in _clientList)
                {
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        int socketIndex = _serverSocket.IndexOf(item);
                        _serverSocket.CloseConnection(socketIndex);
                    }));
                }
                isForceDisconnect = false;
            }
        }

        /// <summary>
        /// Set Online Or Offline
        /// </summary>
        private void SetStatusConnect()
        {
            if (_serverSocket != null)
                IsConnected = _serverSocket.ReadyConnected;
            else
                IsConnected = false;
        }

        /// <summary>
        /// Merge Message & show
        /// </summary>
        /// <param name="msg"></param>
        private void SetSeverLog(string msg)
        {
            this.ServerLog += msg + "\n";
        }

        /// <summary>
        /// Set Tootip for notify icon
        /// </summary>
        private void SetToolTipNotitfyIcon()
        {
            string notifyIconText = IsConnected ? "Online" : "Offline";

            if (IsConnected == true && _serverSocket != null)
                notifyIconText += "\n" + string.Format("{0} client(s) connected", _serverSocket.ActiveConnections);

            if (notifyIcon != null)
                notifyIcon.ToolTipText = notifyIconText;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Events
        private void _systemTimer_Tick(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
           {
               //Set System Time
               this.SystemTime = DateTime.Now;

               //Set Free Ram
               this.FreeSystemRam = string.Format("{0} MB", ((MachineUtility.GetFreeRam() / 1024) / 1024).ToString("N0"));

               SetToolTipNotitfyIcon();
           }));
        }


        /// <summary>
        /// Listen client connect to server & Add to collection
        /// </summary>
        /// <param name="soc"></param>
        private void _serverSocket_OnConnect(System.Net.Sockets.Socket soc)
        {
            string msg = string.Format("Client {0} connected on: {1}", soc.RemoteEndPoint, DateTime.Now);

            SetSeverLog(msg);

            AddClient(soc);
        }

        /// <summary>
        /// Listen client is disconnected 
        /// </summary>
        /// <param name="soc"></param>
        private void _serverSocket_OnDisconnect(Socket soc)
        {
            if (!isForceDisconnect)//avoid disconnect from server not access client socket to get info
            {
                string msg = string.Format("Client is disconnected on {0}", DateTime.Now);
                App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
                  {
                      try
                      {
                          msg = string.Format("[Client]{0} is disconnected on {1}", soc.RemoteEndPoint, DateTime.Now);//(Check)Has Error when Client Shutdown
                      }
                      catch
                      {
                      }
                      SetSeverLog(msg);
                  }));
            }
            else
            {
                SetSeverLog(string.Format("Diconnect all Client on {0}", DateTime.Now));
            }

            RemoveClient(soc);
        }

        /// <summary>
        /// Error occur
        /// </summary>
        /// <param name="ErroMessage"></param>
        /// <param name="soc"></param>
        /// <param name="ErroCode"></param>
        private void _serverSocket_OnError(string ErroMessage, Socket soc, int ErroCode)
        {
            string msg = string.Empty;
            if (soc != null)
                msg = string.Format("[Error] Client {0} : {1} ", soc.RemoteEndPoint, ErroMessage);
            else
                msg = string.Format("[Error] " + ErroMessage);
            SetSeverLog(msg);
            Console.WriteLine(ErroCode);

            if (Convert.ToInt32(SocketError.AddressAlreadyInUse).Equals(ErroCode))
            {
                MessageBox.Show("Port ready in use, try another!", "Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!Convert.ToInt32(SocketError.ConnectionReset).Equals(ErroCode))//Avoid Show message box when Client disconnectd ErrorCode 10054
            {
                MessageBox.Show(msg, "Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Server Ready connect Event
        /// </summary>
        void _serverSocket_OnListen()
        {
            SetStatusConnect();
            if (_serverSocket != null && _serverSocket.ReadyConnected)
            {
                string msg = string.Format("Start Application with port {0} on: {1}", _serverSocket.Port, DateTime.Now);
                SetSeverLog(msg);
            }
        }
        #endregion
    }


}
