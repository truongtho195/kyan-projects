using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.EntityClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using Npgsql;

namespace CPC.POS.ViewModel
{
    public class BackupRestoreViewModel : ViewModelBase
    {
        #region Define

        public RelayCommand BackupCommand { get; private set; }
        public RelayCommand<object> RestoreCommand { get; private set; }
        private readonly string TempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BackupRestoreDB.bat");

        #endregion

        #region Constructors

        public BackupRestoreViewModel()
        {
            this.InitialCommand();
            this.GetAllFile();

            // Get permission
            GetPermission();
        }

        #endregion

        #region Properties

        #region UserLogCollection
        /// <summary>
        /// Gets or sets the UserLogCollection.
        /// </summary>
        private ObservableCollection<base_UserLogModel> _userLogCollection = new ObservableCollection<base_UserLogModel>();
        public ObservableCollection<base_UserLogModel> UserLogCollection
        {
            get
            {
                return _userLogCollection;
            }
            set
            {
                if (_userLogCollection != value)
                {
                    _userLogCollection = value;
                    OnPropertyChanged(() => UserLogCollection);
                }
            }
        }

        #endregion

        #region SelectedItemUserLog
        /// <summary>
        /// Gets or sets the SelectedItemUserLog.
        /// </summary>
        private base_UserLogModel _selectedItemUserLog;
        public base_UserLogModel SelectedItemUserLog
        {
            get
            {
                return _selectedItemUserLog;
            }
            set
            {
                if (_selectedItemUserLog != value)
                {
                    _selectedItemUserLog = value;
                    this.OnPropertyChanged(() => SelectedItemUserLog);
                }
            }
        }
        #endregion

        #region SelectedFile
        /// <summary>
        /// Gets or sets the SelectedFile.
        /// </summary>
        private BackupModel _selectedFile;
        public BackupModel SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    this.OnPropertyChanged(() => SelectedFile);
                }
            }
        }
        #endregion

        #region TotalUsers
        private int _totalUsers;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalUsers
        {

            get
            {
                return _totalUsers;
            }
            set
            {
                if (_totalUsers != value)
                {
                    _totalUsers = value;
                    OnPropertyChanged(() => TotalUsers);
                }
            }
        }
        #endregion

        #region CurrentPageIndex
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get
            {
                return _currentPageIndex;
            }
            set
            {
                if (value != _currentPageIndex)
                {
                    _currentPageIndex = value;
                    OnPropertyChanged(() => CurrentPageIndex);
                }
            }
        }

        #endregion

        #region IsHiddenColunm
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private bool _isHiddenColunm = false;
        public bool IsHiddenColunm
        {
            get
            {
                return _isHiddenColunm;
            }
            set
            {
                if (value != _isHiddenColunm)
                {
                    _isHiddenColunm = value;
                    OnPropertyChanged(() => IsHiddenColunm);
                }
            }
        }

        #endregion

        #region FileCollection
        /// <summary>
        /// Gets or sets the FileCollection.
        /// </summary>
        private ObservableCollection<BackupModel> _fileCollection = new ObservableCollection<BackupModel>();
        public ObservableCollection<BackupModel> FileCollection
        {
            get
            {
                return _fileCollection;
            }
            set
            {
                if (_fileCollection != value)
                {
                    _fileCollection = value;
                    OnPropertyChanged(() => FileCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Commands Methods

        #region BackupCommand
        /// <summary>
        /// Method to check whether the Backup command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnBackupCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ConnectedCommand is executed.
        /// </summary>
        private void OnBackupCommandExecute()
        {
            // TODO: Handle command logic here
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text38, Language.Information, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    //Backup data
                    BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                    bgWorker.DoWork += (sender, e) =>
                    {
                        // Turn on BusyIndicator
                        if (Define.DisplayLoading)
                            IsBusy = true;
                        BackupRestoreHelper.BackupPath = Define.CONFIGURATION.BackupPath + @"\Backup\";
                        BackupRestoreHelper.BackupDB();
                    };
                    bgWorker.RunWorkerCompleted += (sender, e) =>
                    {
                        this.GetAllFile();
                        // Turn off BusyIndicator
                        IsBusy = false;
                        if (BackupRestoreHelper.SuccessfulFlag == 1)
                            Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text32, Language.Information, System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                            Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text33, Language.Warning, System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                    };
                    // Run async background worker
                    bgWorker.RunWorkerAsync();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update UserLog" + ex.ToString());
            }
        }
        #endregion

        #region RestoreCommand

        /// <summary>
        /// Method to check whether the Restore command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRestoreCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return AllowRestoreData;
        }

        /// <summary>
        /// Method to invoke when the LoadStep command is executed.
        /// </summary>
        private void OnRestoreCommandExecute(object param)
        {
            MessageBoxResult resultRestore = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text39, Language.Information, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultRestore == MessageBoxResult.Yes)
            {
                BackgroundWorker bgWorkerBackup = new BackgroundWorker { WorkerReportsProgress = true };
                bgWorkerBackup.DoWork += (sender, e) =>
                 {
                     // Turn on BusyIndicator
                     if (Define.DisplayLoading)
                         IsBusy = true;
                     BackupRestoreHelper.BackupPath = Define.CONFIGURATION.BackupPath + @"\Backup\";
                     BackupRestoreHelper.BackupDB();
                 };
                // Run async background worker
                bgWorkerBackup.RunWorkerAsync();

                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                bgWorker.DoWork += (sender, e) =>
                {
                    // Turn on BusyIndicator
                    if (Define.DisplayLoading)
                        IsBusy = true;
                    BackupRestoreHelper.BackupPath = Define.CONFIGURATION.BackupPath + @"\Backup\";
                    BackupRestoreHelper.RestoreDB(param as BackupModel);
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
                    }
                    else
                        Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text36, Language.Warning, System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                };
                // Run async background worker
                bgWorker.RunWorkerAsync();
            }
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = this.FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            // Route the commands
            this.BackupCommand = new RelayCommand(this.OnBackupCommandExecute, this.OnBackupCommandCanExecute);
            this.RestoreCommand = new RelayCommand<object>(this.OnRestoreCommandExecute, OnRestoreCommandCanExecute);
            this.CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private void GetFileInformation(string path)
        {
            try
            {
                //Get the root directory and print out some information about it.
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(path);
                Debug.WriteLine(dirInfo.Attributes.ToString());
                // Get the files in the directory and print out some information about them.
                System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (System.IO.FileInfo file in fileNames.OrderByDescending(x => x.CreationTime))
                {
                    BackupModel model = new BackupModel();
                    model.Name = file.Name;
                    model.Path = file.FullName;
                    model.Detail = file.CreationTime.ToString("MM-dd-yyyy hh:mm:ss");
                    this.FileCollection.Add(model);
                    Debug.WriteLine("{0}: {1}: {2}", file.Name, file.LastAccessTime, file.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void DeleteFolder(string path)
        {
            Directory.Delete(path);
        }

        private void GetAllFile()
        {
            try
            {
                this.FileCollection.Clear();
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(BackupRestoreHelper.BackupPath);
                Debug.WriteLine(dirInfo.Attributes.ToString());
                // Get the files in the directory and print out some information about them.
                System.IO.DirectoryInfo[] folderNames = dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                foreach (System.IO.DirectoryInfo folder in folderNames.OrderByDescending(x => x.CreationTime))
                {
                    this.GetFileInformation(folder.FullName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion

        #region Permission

        #region Properties

        private bool _allowRestoreData = true;
        /// <summary>
        /// Gets or sets the AllowRestoreData.
        /// </summary>
        public bool AllowRestoreData
        {
            get { return _allowRestoreData; }
            set
            {
                if (_allowRestoreData != value)
                {
                    _allowRestoreData = value;
                    OnPropertyChanged(() => AllowRestoreData);
                }
            }
        }

        #endregion

        /// <summary>
        /// Get permissions
        /// </summary>
        public override void GetPermission()
        {
            if (!IsAdminPermission && !IsFullPermission)
            {
                // Get all user rights
                IEnumerable<string> userRightCodes = Define.USER_AUTHORIZATION.Select(x => x.Code);

                // Get edit quantity permission
                AllowRestoreData = userRightCodes.Contains("MN200");
            }
        }

        #endregion
    }

    public class BackupRestoreHelper
    {
        #region Field

        protected static string TempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BackupRestoreDB.bat");
        protected static string ProviderConnectString = ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString;
        protected static string ConnectString = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString).ProviderConnectionString;
        public static string BackupPath = Define.CONFIGURATION.BackupPath + @"\Backup\";
        protected static string POSTGRESQL_DIRECTORY = string.Empty;
        protected static string Server = string.Empty;
        protected static string UserID = string.Empty;
        protected static string Password = string.Empty;
        protected static string Database = string.Empty;
        protected static string Port = string.Empty;
        public static int SuccessfulFlag = 0;
        public static string DBDefault = "postgres";

        #endregion

        #region Private Methods

        #region GenerateBackupBatchFile
        private static void GenerateBackupBatchFile()
        {
            try
            {
                string server = String.Empty,
                    userID = String.Empty,
                    password = String.Empty,
                    database = String.Empty,
                    port = string.Empty;
                string datePath = BackupPath + DateTimeExt.Today.ToString("MM-dd-yyyy");
                Parse(ConnectString, ref server, ref userID, ref password, ref database, ref port);
                TextWriter write = new StreamWriter(TempPath);
                write.WriteLine(@"@echo off");
                write.WriteLine("set BACKUPDIR={0}\\", datePath);
                write.WriteLine("set PGHOST={0}", server);
                write.WriteLine("set PGUSER={0}", userID);
                write.WriteLine("set PGBIN={0}", POSTGRESQL_DIRECTORY.Replace('/', '\\'));
                write.WriteLine("set PGDB={0}", database);
                write.WriteLine("set PGPORT={0}", port);
                write.WriteLine("for /f \"tokens=1-4 delims=/ \" %%i in (\"%date%\") do (");
                write.WriteLine("\tset dow=%%i");
                write.WriteLine("\tset month=%%j");
                write.WriteLine("\tset day=%%k");
                write.WriteLine("\tset year=%%l");
                write.WriteLine(")");
                write.WriteLine("for /f \"tokens=1-3 delims=: \" %%i in (\"%time%\") do (");
                write.WriteLine("\tset hh=%%i");
                write.WriteLine("\tset mm=%%j");
                write.WriteLine("\tset ss=%%k");
                write.WriteLine(")");
                write.WriteLine("cd %PGBIN%");
                write.WriteLine("C:");
                write.WriteLine(@"pg_dump -i -h %PGHOST% -p %PGPORT% -U %PGUSER% -F c -b -v --inserts -f ""%BACKUPDIR%%PGDB%_%year%%month%%day%%hh%%mm%%ss%.backup"" %PGDB%");
                write.Close();
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Data error flow detail error" + ex.Message, "Tims POS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region GenerateRestoreBatchFile
        private static string GenerateRestoreBatchFile(BackupModel model)
        {
            try
            {
                string server = String.Empty,
                    userID = String.Empty,
                    password = String.Empty,
                    database = String.Empty,
                    port = string.Empty;
                Parse(ConnectString, ref server, ref userID, ref password, ref database, ref port);
                TextWriter write = new StreamWriter(TempPath);
                write.WriteLine(@"@echo off");
                write.WriteLine("set PATH_FILE_RESTORE=\"{0}\"", model.Path);
                write.WriteLine("set PGHOST={0}", server);
                write.WriteLine("set PGUSER={0}", userID);
                write.WriteLine("set PGBIN={0}", POSTGRESQL_DIRECTORY.Replace('/', '\\'));
                write.WriteLine("set PGDB={0}", database);
                write.WriteLine("set PGPORT={0}", port);
                write.WriteLine("cd %PGBIN%");
                write.WriteLine("C:");
                write.WriteLine("createdb -U %PGUSER% -e %PGDB%");
                write.WriteLine(@"pg_restore -i -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDB% -v %PATH_FILE_RESTORE%");
                write.WriteLine(@"pause");
                write.Close();
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Data error flow detail error" + ex.Message, "Tims POS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return model.Path;
        }
        #endregion

        #region GetPostgresqlDirectory
        private static string GetPostgresqlDirectory()
        {
            string directory = String.Empty;
            try
            {
                using (var service = new POSEntities(ProviderConnectString))
                {
                    service.Connection.Open();
                    var result = service.ExecuteStoreQuery<string>("SELECT setting FROM pg_settings WHERE name = 'data_directory'");
                    directory = result.SingleOrDefault();
                    if (null != directory && directory.EndsWith("/data", StringComparison.InvariantCultureIgnoreCase))
                    {
                        directory = directory.Substring(0, directory.Length - "/data".Length) + "/bin";
                    }
                    service.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
            return directory;
        }
        #endregion

        #region Parse
        private static void Parse(string connectionString, ref string server, ref string userID, ref string password, ref string database, ref string port)
        {
            Regex nameval = new Regex(@"(?<name>[^=]+)\s*=\s*(?<val>[^;]+?)\s*(;|$)", RegexOptions.Singleline);
            foreach (Match m in nameval.Matches(connectionString))
            {
                Console.WriteLine(
                "name=[{0}], val=[{1}]",
                m.Groups["name"].ToString(),
                m.Groups["val"].ToString());

                switch (m.Groups["name"].ToString().ToLower())
                {
                    case "server":
                        server = m.Groups["val"].ToString();
                        break;
                    case "userid":
                        userID = m.Groups["val"].ToString();
                        break;
                    case "password":
                        password = m.Groups["val"].ToString();
                        break;
                    case "database":
                        database = m.Groups["val"].ToString();
                        break;
                    case "port":
                        port = m.Groups["val"].ToString();
                        break;
                }
            }

        }
        #endregion

        #region Backup
        /// <summary>
        //Return Pathfile or PathFolder is valid
        //Return empty is Invalid
        /// </summary>
        /// <param name="path"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        private static string ValidFileBatch(string path, string commandType)
        {
            string vfb = string.Empty;

            if (string.IsNullOrEmpty(path)
                || System.IO.Path.GetInvalidPathChars().Intersect(
                                  path.ToCharArray()).Count() != 0
                || !new System.IO.FileInfo(path).Exists)
            {
                vfb = null;
            }
            else if (new System.IO.FileInfo(path).Exists)
            {
                IList<string> s = System.IO.File.ReadAllLines(path);
                if (s.Count > 0)
                    vfb = s[1].Substring(s[1].IndexOf('=') + 1, s[1].Length - (s[1].IndexOf('=') + 1));
                else
                {
                    return null;
                }

                if (commandType == "pg_dump")
                {
                    if (!new System.IO.DirectoryInfo(vfb).Exists)
                    {
                        Directory.CreateDirectory(vfb);
                    }
                    else if (!HasWritePermission(vfb))
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("The user does not have permission to access: " + vfb, "Backup", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                        vfb = string.Empty;
                    }
                }
                else if (commandType == "pg_restore")
                {
                    if (!new System.IO.FileInfo(vfb.Replace('"', ' ')).Exists)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("Not exists file", "Restore", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                        vfb = string.Empty;
                    }
                    else if (!HasReadPermission(vfb.Replace('"', ' ').Trim()))
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("The user does not have permission to access: " + vfb, "Restore", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                        vfb = string.Empty;
                    }
                }
            }

            return vfb;
        }
        /// <summary>
        /// Can backup execute
        /// </summary>
        /// <returns>True / false</returns>
        private bool CanBackupGenerateExecute()
        {
            return (!string.IsNullOrEmpty(Define.CONFIGURATION.BackupPath));
        }

        private void BrowseExecuted()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog =
                new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Define.CONFIGURATION.BackupPath;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Define.CONFIGURATION.BackupPath = folderBrowserDialog.SelectedPath;
        }

        #endregion

        #region WriteFilePGPASS
        private static bool WriteFilePGPASS(string commandType)
        {
            try
            {
                string server = String.Empty, userID = String.Empty, password = String.Empty, database = String.Empty, port = string.Empty;
                string pathFolderApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string pathFile = pathFolderApplicationData + @"\postgresql\pgpass.conf";
                Parse(ConnectString, ref server, ref userID, ref password, ref database, ref port);
                string pgpassString = server + ":" + port + ":" + database + ":" + userID + ":" + password;

                if (new System.IO.FileInfo(pathFile).Exists)
                {
                    string[] lines = File.ReadAllLines(pathFile);
                    foreach (var line in lines)
                    {
                        if (line.ToUpper().Equals(pgpassString.ToUpper()))
                            return true;
                    }
                    FileStream fs = new FileStream(pathFile, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter m_streamWriter = new StreamWriter(fs);
                    // Write to the file using StreamWriter class
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine(pgpassString);
                    m_streamWriter.Flush();
                }
                else
                {
                    FileStream fs = new FileStream(pathFile, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter m_streamWriter = new StreamWriter(fs);
                    // Write to the file using StreamWriter class
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine(pgpassString);
                    m_streamWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                if (commandType == "pg_dump")
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Backup cannot created", "Backup", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (commandType == "pg_restore")
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Backup cannot created", "Restore", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return false;
            }
            return true;
        }
        #endregion

        #region HasWritePermission
        private static bool HasWritePermission(string FilePath)
        {
            try
            {
                string fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Ticks.ToString() + ".txt";
                using (FileStream fs = File.Create(Path.Combine(FilePath, fileName), 1, FileOptions.DeleteOnClose))
                { }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region HasReadPermission
        private static bool HasReadPermission(string FilePath)
        {
            try
            {
                FileSystemSecurity security;
                if (File.Exists(FilePath))
                {
                    security = File.GetAccessControl(FilePath);
                }
                else
                {
                    security = Directory.GetAccessControl(Path.GetDirectoryName(FilePath));
                }
                var rules = security.GetAccessRules(true, true, typeof(NTAccount));

                var currentuser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool result = false;
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (0 == (rule.FileSystemRights &
                        (FileSystemRights.ReadData | FileSystemRights.Read)))
                    {
                        continue;
                    }

                    if (rule.IdentityReference.Value.StartsWith("S-1-"))
                    {
                        var sid = new SecurityIdentifier(rule.IdentityReference.Value);
                        if (!currentuser.IsInRole(sid))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!currentuser.IsInRole(rule.IdentityReference.Value))
                        {
                            continue;
                        }
                    }

                    if (rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    if (rule.AccessControlType == AccessControlType.Allow)
                        result = true;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region ExecuteCommand
        private static void ExecuteCommand(string commandType)
        {
            if (!string.IsNullOrEmpty(ValidFileBatch(TempPath, commandType)))
            {
                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(TempPath);
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.CreateNoWindow = true;

                    Process process = new Process();
                    process.EnableRaisingEvents = true;
                    process.StartInfo = processStartInfo;

                    bool processStarted = process.Start();

                    if (processStarted)
                    {
                        //Get the output stream
                        //Display the result
                        string displayError = "==============" + Environment.NewLine;
                        displayError += process.StandardError.ReadToEnd();
                        Debug.WriteLine(displayError);

                        while (!process.HasExited)
                            System.Threading.Thread.Sleep(1000);

                        process.WaitForExit(1000 * 60 * 5);//5 minutes
                        process.Close();

                        if (commandType == "pg_dump")
                        {
                            if (displayError.IndexOf("cannot") < 0)
                                SuccessfulFlag = 1;
                            //Xceed.Wpf.Toolkit.MessageBox.Show("Backup successfuly.", "Backup", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                SuccessfulFlag = 0;
                            //Xceed.Wpf.Toolkit.MessageBox.Show("Backup cannot created", "Backup", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (commandType == "pg_restore")
                        {
                            if (displayError.IndexOf("cannot") < 0)
                                SuccessfulFlag = 1;
                            else
                                SuccessfulFlag = 0;
                        }
                    }
                }
                catch (Win32Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region Backup
        public static void BackupDB()
        {
            BackupPath = Define.CONFIGURATION.BackupPath + @"\Backup\";
            POSTGRESQL_DIRECTORY = GetPostgresqlDirectory();
            if (WriteFilePGPASS("pg_dump"))
            {
                GenerateBackupBatchFile();
                ExecuteCommand("pg_dump");
            }
        }
        #endregion

        #region Restore
        public static void RestoreDB(BackupModel model)
        {
            try
            {
                //Clear Current DB
                BackupPath = Define.CONFIGURATION.BackupPath + @"\Backup\";
                POSTGRESQL_DIRECTORY = GetPostgresqlDirectory();
                DropCurrentDB();
                if (WriteFilePGPASS("pg_restore"))
                {
                    GenerateRestoreBatchFile(model);
                    ExecuteCommand("pg_restore");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Restore");
            }
        }
        #endregion

        #region ClearAllData
        public static void ClearAllData()
        {
            try
            {
                using (NpgsqlConnection pgsqlConnection = new NpgsqlConnection(ConnectString))
                {
                    // Open the PgSQL Connection.                
                    pgsqlConnection.Open();
                    string function = string.Format("clearalldata('{0}')", Define.UserPostgres);
                    string selectCommand = string.Format("select {0}", function);
                    using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(selectCommand, pgsqlConnection))
                    {
                        using (NpgsqlTransaction tran = pgsqlConnection.BeginTransaction())
                        {
                            pgsqlcommand.CommandType = System.Data.CommandType.Text;
                            pgsqlcommand.ExecuteNonQuery();
                            tran.Commit();
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
        }

        public static void DropAllData()
        {
            try
            {
                using (NpgsqlConnection pgsqlConnection = new NpgsqlConnection(ConnectString))
                {
                    // Open the PgSQL Connection.                
                    pgsqlConnection.Open();
                    string selectCommand = string.Format("select {0}", "droplldb('postgres')");
                    using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(selectCommand, pgsqlConnection))
                    {
                        using (NpgsqlTransaction tran = pgsqlConnection.BeginTransaction())
                        {
                            pgsqlcommand.CommandType = System.Data.CommandType.Text;
                            pgsqlcommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
        }

        public static void CreateTempDB()
        {
            try
            {
                string server = String.Empty,
                    userID = String.Empty,
                    password = String.Empty,
                    database = String.Empty,
                    port = string.Empty;
                Parse(ConnectString, ref server, ref userID, ref password, ref database, ref port);
                TextWriter write = new StreamWriter(TempPath);
                write.WriteLine(@"@echo off");
                write.WriteLine("set PGHOST={0}", server);
                write.WriteLine("set PGUSER={0}", userID);
                write.WriteLine("set PGBIN={0}", POSTGRESQL_DIRECTORY.Replace('/', '\\'));
                write.WriteLine("set PGDB={0}", "tempdb");
                write.WriteLine("set PGPORT={0}", port);
                write.WriteLine("cd %PGBIN%");
                write.WriteLine("C:");
                write.WriteLine(@"createdb -h %PGHOST% -p %PGPORT% -U %PGUSER% -e %PGDB%");
                write.WriteLine(@"pause");
                write.Close();
                ProcessStartInfo processStartInfo = new ProcessStartInfo(TempPath);
                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                Process process = new Process();
                process.EnableRaisingEvents = true;
                process.StartInfo = processStartInfo;
                bool processStarted = process.Start();
                if (processStarted)
                {
                    //Get the output stream
                    //Display the result
                    string displayError = "==============" + Environment.NewLine;
                    displayError += process.StandardError.ReadToEnd();
                    Debug.WriteLine(displayError);

                    while (!process.HasExited)
                        System.Threading.Thread.Sleep(1000);
                    process.WaitForExit(1000 * 60 * 5);//5 minutes
                    process.Close();
                    //Connect Temp Database
                    using (NpgsqlConnection pgsqlConnection = new NpgsqlConnection(ConnectString))
                    {
                        pgsqlConnection.ChangeDatabase("tempdb");
                        // Open the PgSQL Connection.                
                        string selectCommand = "SELECT pg_terminate_backend(pg_stat_activity.procpid)FROM pg_stat_activity WHERE pg_stat_activity.datname = 'pos2013'";
                        using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(selectCommand, pgsqlConnection))
                        {
                            pgsqlcommand.CommandType = System.Data.CommandType.Text;
                            pgsqlcommand.ExecuteNonQuery();
                        }
                        string dropCommand = string.Format("DROP DATABASE {0}", database);
                        using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(dropCommand, pgsqlConnection))
                        {
                            pgsqlcommand.CommandType = System.Data.CommandType.Text;
                            pgsqlcommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Data error flow detail error" + ex.Message, "Tims POS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void DropCurrentDB()
        {
            try
            {
                string server = String.Empty,
                    userID = String.Empty,
                    password = String.Empty,
                    database = String.Empty,
                    port = string.Empty;
                Parse(ConnectString, ref server, ref userID, ref password, ref database, ref port);
                //Connect Temp Database
                using (NpgsqlConnection pgsqlConnection = new NpgsqlConnection(ConnectString))
                {
                    pgsqlConnection.ChangeDatabase(DBDefault);
                    // Open the PgSQL Connection.                
                    string selectCommand = string.Format("SELECT pg_terminate_backend(pg_stat_activity.procpid)FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'", database);
                    using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(selectCommand, pgsqlConnection))
                    {
                        pgsqlcommand.CommandType = System.Data.CommandType.Text;
                        pgsqlcommand.ExecuteNonQuery();
                    }
                    string dropCommand = string.Format("DROP DATABASE {0}", database);
                    using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(dropCommand, pgsqlConnection))
                    {
                        pgsqlcommand.CommandType = System.Data.CommandType.Text;
                        pgsqlcommand.ExecuteNonQuery();
                    }
                    pgsqlConnection.Close();
                }
            }
            catch (NpgsqlException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Data error flow detail error" + ex.Message, "Tims POS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region ClearFileBackupFolder
        public static void ClearFileBackupFolder(string path)
        {
            try
            {
                DateTime KeepDate = DateTimeExt.Today.AddDays(-Double.Parse(Define.CONFIGURATION.KeepBackUp.Value.ToString()));
                // Get the root directory and print out some information about it.
                //@"\\192.168.1.1\h\Projects\SoftDept\POS\Images\POS2013\Backup\"
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(path);
                Debug.WriteLine(dirInfo.Attributes.ToString());
                // Get the files in the directory and print out some information about them.
                System.IO.DirectoryInfo[] folderNames = dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                foreach (System.IO.DirectoryInfo folder in folderNames)
                {
                    if (folder.CreationTime.Date < KeepDate.Date)
                    {
                        Debug.WriteLine("{0} {1}", folder.Name, folder.LastAccessTime);
                        folder.Delete(true);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ClearDataOnUserLog" + ex.ToString());
            }
        }
        #endregion

        #endregion
    }
}