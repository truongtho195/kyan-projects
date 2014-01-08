using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Diagnostics;
using System.Text;
using System.Windows;
using CPC.Service;
using CPC.Service.FrameworkDialogs.OpenFile;
using CPC.TimeClock;
using log4net;
using log4net.Config;
using Microsoft.Shell;
using CPC.TimeClock.Repository;
using System.Linq;
using CPC.TimeClock.Database;
using CPC.TimeClock.Model;

namespace CPC.TimeClock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {

        #region Fields

        //private Tims.View.DatabaseView databaseView;
        private bool IsConnectAuthenicated { set; get; }

        // Static member variables
        private static readonly ILog m_Logger = LogManager.GetLogger(typeof(App).Name);

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor
        /// </summary>
        static App()
        {
            // Configure Log4Net
            XmlConfigurator.Configure();
        }

        public App()
        {
            this.InitializeComponent();
            this.Initialize();
        }
        #endregion

        #region Callback and show window methods

        private void Initialize()
        {

            ShowMainWindow();

            //if (IsConnectAuthenicated)
            //{
            //    // Load settings
            //    //this.LoadSharedData();

            //    ShowMainWindow();

            //}
            //else
            //{
            //    // Show database config view if the program connect fail
            //    App.Messenger.Register(define.DATABASE_CONFIG_RESULT,
            //                       new Action<bool>((result) => ConfigurationCallback(result)));

            //    if (Application.Current.ShutdownMode != System.Windows.ShutdownMode.OnExplicitShutdown)
            //    {
            //        Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            //    }

            //    databaseView = new DatabaseView();
            //    bool? res = databaseView.ShowDialog();

            //    if (false == res && !IsConnectAuthenicated)
            //    {
            //        databaseView.Close();
            //        databaseView = null;
            //        App.Current.Shutdown(1);
            //    }
            //}
        }

        /// <summary>
        /// Callback from LogOnScreenViewModel after user been verified
        /// Before show the main window, close the existed loginview
        /// </summary>
        /// <param name="IsVerified"></param>
        private void AuthenticateCallback(bool IsVerified)
        {
            ShowMainWindow();
        }

        /// <summary>
        /// Callback from DatabaseViewModel after user been verified
        /// </summary>
        /// <param name="IsVerified"></param>
        private void ConfigurationCallback(bool isConnectAuthenicated)
        {
            //// If user been verified, close the loginView
            //if (null != databaseView)
            //{
            //    databaseView.Close();
            //    databaseView = null;
            //}

            IsConnectAuthenicated = isConnectAuthenicated;

            this.Initialize();
        }

        private void ShowMainWindow()
        {
            // Configure the service locator
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            MainWindow mainView = new MainWindow();
            this.MainWindow = mainView;

            //MainWindowViewModel mainViewModel = new MainWindowViewModel();
            //mainView.DataContext = mainViewModel;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Show the main window
            mainView.Show();
        }

        #endregion

        #region Method Overrides

        /// <summary>
        /// Initializes the application.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            m_Logger.Info("TimeClock Starts");
        }

        /// <summary>
        /// Logs application exit.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            m_Logger.Info("TimeClock closed");
        }

        #endregion

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return true;
        }

        #endregion

        #region Messenger

        /// <summary>
        /// Provides loosely-coupled messaging to communicate between ViewModels through the program
        /// All references to objects are stored weakly, to prevent memory leaks.
        /// 
        /// Usage: First register the message without passing any parameters
        /// Messenger.Register(
        ///                    "MY_MESSAGE",
        ///                    new Action(() => DoMyStuff()));
        ///                         
        /// Then
        ///     Messenger.NotifyColleagues(App.MSG_LOG_APPENDED);
        ///     this will execute DoMyStuff()
        ///     
        /// OR
        /// 
        /// Register the message to pass a integer
        ///  Messenger.Register(
        ///                         "MY_MESSAGE",
        ///                         new Action<int>)((param) => GotoMyFunction(param)));
        ///  
        ///  void GotoMyFunction(int param)
        ///  {
        ///  }
        ///    
        ///   The same logic can be applied for param of any object model (array)
        /// </summary>
        readonly static Messenger.Message _messenger = new Messenger.Message();
        internal static Messenger.Message Messenger
        {
            get { return _messenger; }
        }

        #endregion Messenger

        #region Handle program Exception
        private void PosDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.StackTrace.ToString());
            e.Handled = true;
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void HandleException(Exception e, bool isTerminating)
        {
            if (e == null) { return; }

            Trace.TraceError(e.ToString());

            if (!isTerminating)
            {
                // show the message to the user
            }
        }
        #endregion Handle program Exception

        #region Build connection strings

        /// <summary>
        /// Dynamic build the connection string for db
        /// i.e. BuildEntityConnString("posadventure", "localhost", "Database.POSDB", "postgres", "postgres")
        /// Store it in ApplicationIsolatedSetting
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <param name="server"></param>
        /// <param name="resourceData"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string BuildConnectionString(string dbFileName, string server, string username, string password)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Port=5432;Encoding=WIN1252;Server=");
            builder.Append(server);
            builder.Append(";Database=");
            builder.Append(dbFileName);
            builder.Append(";UserID=");
            builder.Append(username);
            builder.Append(";Password=");
            builder.Append(password);

            return builder.ToString();
        }

        /// <summary>
        /// Dynamic build the connection string for db
        /// i.e. BuildEntityConnString("posadventure", "localhost", "Database.POSDB", "postgres", "postgres")
        /// Store it in ApplicationIsolatedSetting
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <param name="server"></param>
        /// <param name="resourceData"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static void BuildEntityConnString(string dbFileName, string server, string resourceData, string username, string password)
        {
            string resAll = @"res://*/";
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            entityBuilder.Metadata = string.Format("{0}{1}.csdl|{0}{1}.ssdl|{0}{1}.msl", resAll, resourceData);
            entityBuilder.Provider = "Npgsql";
            entityBuilder.ProviderConnectionString = BuildConnectionString(dbFileName, server, username, password);
        }

        #endregion DB connection string

        #region LoadSharedData

        //private void LoadSharedData()
        //{
        //    // Declare repository
        //    base_ConfigurationRepository configurationRepository = new base_ConfigurationRepository();
        //    tims_HolidayRepository holidayRepository = new Repository.tims_HolidayRepository();

        //    // ______________________ Init Data for TimeClock System
        //    string computerName = System.Windows.Forms.SystemInformation.ComputerName;

        //    // Load settings
        //    var configuration = configurationRepository.GetAll().FirstOrDefault();

        //    if (null == configuration)
        //    {
        //        define.CONFIGURATION = new base_ConfigurationModel();
        //    }
        //    else
        //    {
        //        define.CONFIGURATION = new base_ConfigurationModel(configuration);
        //    }

        //    //switch ((FingerprintOptions)define.CONFIGURATION.FingerprintOption)
        //    //{
        //    //    case FingerprintOptions.Never:
        //    //        define.BlockFingerprint = true;
        //    //        break;
        //    //    case FingerprintOptions.Computer:
        //    //        var computerNames = define.CONFIGURATION.FingerprintComputers.Replace(" ", String.Empty).Split(',');
        //    //        if (!computerNames.Contains(computerName))
        //    //        {
        //    //            define.BlockFingerprint = true;
        //    //        }
        //    //        break;
        //    //    default:
        //    //        define.BlockFingerprint = false;
        //    //        break;
        //    //}

        //    if (define.BlockFingerprint == true) return;

        //    System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(computerName);
        //    var ipAddress = host.AddressList[0].ToString();

        //    //var workStation = workStationRespository.GetAllWorkStation(x => x.WorkStationName == computerName || x.IpAddress == ipAddress).FirstOrDefault();
        //    //if (null != workStation)
        //    //{
        //    //    define.ReloadTime = workStation.ReloadTime.TimeOfDay;
        //    //    define.EnableIdleTime = workStation.MinimizeFlag;
        //    //}

        //    if (define.CONFIGURATION.LastHolidayUpdated == null || DateTimeExt.Today.Year > define.CONFIGURATION.LastHolidayUpdated)
        //    {
        //        // Get holidays of the current year
        //        HashSet<KeyValuePair<DateTime, string>> holidays = new HashSet<KeyValuePair<DateTime, string>>();
        //        var query = holidayRepository.GetAll(x => x.HolidayOption == (int)HolidayOption.SpecificDay ||
        //            x.HolidayOption == (int)HolidayOption.DynamicDay);

        //        if (query.Count > 0)
        //        {
        //            // Case fix day and month (but year)
        //            holidays.UnionWith(query.Where(x => x.HolidayOption.Is(HolidayOption.SpecificDay)).Select(x => new KeyValuePair<DateTime, string>(
        //                    new DateTime(DateTimeExt.Today.Year, x.Month.Value, x.Day.Value), x.Title)));
        //            // Case dynamic day of week (Sun to Saturday)
        //            holidays.UnionWith(query.Where(x => x.HolidayOption.Is(HolidayOption.DynamicDay) && x.DayOfWeek.Value > 3).Select(x => new KeyValuePair<DateTime, string>(
        //                    new DateTime(DateTimeExt.Today.Year, x.Month.Value, 1).GetNthWeekofMonth(x.WeekOfMonth.Value, (DayOfWeek)(x.DayOfWeek.Value - 4)).Month > x.Month.Value ?
        //                    new DateTime(DateTimeExt.Today.Year, x.Month.Value, 1).GetNthWeekofMonth(x.WeekOfMonth.Value, (DayOfWeek)(x.DayOfWeek.Value - 4)).AddDays(-7) :
        //                    new DateTime(DateTimeExt.Today.Year, x.Month.Value, 1).GetNthWeekofMonth(x.WeekOfMonth.Value, (DayOfWeek)(x.DayOfWeek.Value - 4)), x.Title)));
        //            // Case dynamic day of month
        //            holidays.UnionWith(query.Where(x => x.HolidayOption.Is(HolidayOption.DynamicDay) && x.DayOfWeek.Value == 1).Select(x => new KeyValuePair<DateTime, string>(
        //                    x.WeekOfMonth.Value == 5 ? new DateTime(DateTimeExt.Today.Year, x.Month.Value, 1).AddMonths(1).AddDays(-1) :
        //                    new DateTime(DateTimeExt.Today.Year, x.Month.Value, x.WeekOfMonth.Value), x.Title)));

        //            // Case dynamic weekdays and weekend days
        //            foreach (var h in query.Where(x => x.HolidayOption.Is(HolidayOption.DynamicDay)))
        //            {
        //                var startDate = new DateTime(DateTimeExt.Today.Year, h.Month.Value, 1);
        //                if (h.DayOfWeek.Value == 2)
        //                {
        //                    // Weekdays
        //                    switch (h.WeekOfMonth.Value)
        //                    {
        //                        case 1: // First
        //                            var friday = startDate.GetNthWeekofMonth(1, DayOfWeek.Friday);
        //                            var iDate = friday;

        //                            int counter = 0;
        //                            while (iDate >= startDate && counter < 5)
        //                            {
        //                                holidays.Add(new KeyValuePair<DateTime, string>(iDate, h.Title));
        //                                iDate = iDate.AddDays(-1);
        //                                counter++;
        //                            }
        //                            break;
        //                        default:
        //                            //case 2: // Second
        //                            //case 3: // Third
        //                            //case 4: // Fourth
        //                            //case 5: // Last
        //                            var monday = startDate.GetNthWeekofMonth(h.WeekOfMonth.Value, DayOfWeek.Monday);
        //                            iDate = monday;
        //                            var endDate = startDate.AddMonths(1).AddDays(-1);

        //                            counter = 0;
        //                            while (iDate <= endDate && counter < 5)
        //                            {
        //                                holidays.Add(new KeyValuePair<DateTime, string>(iDate, h.Title));
        //                                startDate = startDate.AddDays(1);
        //                                counter++;
        //                            }
        //                            break;
        //                    }
        //                }
        //                else if (h.DayOfWeek.Value == 3)
        //                {
        //                    // Weekend days
        //                    switch (h.WeekOfMonth.Value)
        //                    {
        //                        case 1: // First
        //                            var sunday = startDate.GetNthWeekofMonth(1, DayOfWeek.Sunday);
        //                            var iDate = sunday;

        //                            int counter = 0;
        //                            while (iDate >= startDate && counter < 2)
        //                            {
        //                                holidays.Add(new KeyValuePair<DateTime, string>(iDate, h.Title));
        //                                iDate = iDate.AddDays(-1);
        //                                counter++;
        //                            }
        //                            break;
        //                        default:
        //                            //case 2: // Second
        //                            //case 3: // Third
        //                            //case 4: // Fourth
        //                            //case 5: // Last
        //                            var saturday = startDate.GetNthWeekofMonth(h.WeekOfMonth.Value, DayOfWeek.Saturday);
        //                            iDate = saturday;
        //                            var endDate = startDate.AddMonths(1).AddDays(-1);

        //                            counter = 0;
        //                            while (iDate <= endDate && counter < 2)
        //                            {
        //                                holidays.Add(new KeyValuePair<DateTime, string>(iDate, h.Title));
        //                                startDate = startDate.AddDays(1);
        //                                counter++;
        //                            }
        //                            break;
        //                    }
        //                }
        //            }

        //            // Compare the holidays of current year to the holidays in the Holiday History
        //            var dates = holidays.Where(x => define.Holidays.Count(y => y.Date == x.Key) == 0);

        //            if (dates.Count() > 0)
        //            {
        //                // Not the same between 2 collections, add the holidays are not available.
        //                foreach (var d in dates)
        //                {
        //                    Database.HolidayHistory holidayHistory = new Database.HolidayHistory
        //                    {
        //                        Date = d.Key,
        //                        Name = d.Value
        //                    };
        //                    define.Holidays.Add(holidayHistory);
        //                    holidayHistoryRepository.AddHolidayHistory(holidayHistory);
        //                }

        //                holidayHistoryRepository.Commit();
        //            }

        //            // Enable the holidays updated year.
        //            define.CONFIGURATION.LastHolidayUpdated = DateTimeExt.Today.Year;
        //            define.CONFIGURATION.ToEntity();
        //            configurationRepository.UpdateConfiguration(define.CONFIGURATION.Configuration);
        //            configurationRepository.Commit();
        //        }
        //    }
        //}

        #endregion

    }
}